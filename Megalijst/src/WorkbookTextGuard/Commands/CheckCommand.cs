using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WorkbookTextGuard.Commands;

internal static class CheckCommand
{
    public static int Run(string[] args)
    {
        string? inputPath      = null;
        string? policyPath     = null;
        bool    requireCalc    = false;
        string? calcReportPath = null;
        string  mode           = "release"; // preflight | release

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--input":               inputPath      = args[++i]; break;
                case "--policy":              policyPath     = args[++i]; break;
                case "--require-calculated":  requireCalc    = true;      break;
                case "--calculation-report":  calcReportPath = args[++i]; break;
                case "--mode":                mode           = args[++i]; break;
                default:
                    Console.Error.WriteLine($"Unknown argument: {args[i]}");
                    return 1;
            }
        }

        if (inputPath is null || policyPath is null)
        {
            Console.Error.WriteLine("check: --input <file.xlsx> and --policy <policy.json> are required.");
            return 1;
        }

        if (mode != "preflight" && mode != "release")
        {
            Console.Error.WriteLine($"check: --mode must be 'preflight' or 'release'. Got: '{mode}'");
            return 1;
        }

        if (!File.Exists(inputPath))  { Console.Error.WriteLine($"Input file not found: {inputPath}");  return 1; }
        if (!File.Exists(policyPath)) { Console.Error.WriteLine($"Policy file not found: {policyPath}"); return 1; }

        Policy policy;
        try { policy = Policy.Load(policyPath); }
        catch (Exception ex) { Console.Error.WriteLine($"Failed to load policy: {ex.Message}"); return 1; }

        var emoticonProfile = policy.EmoticonProfile.Equals("markup", StringComparison.OrdinalIgnoreCase)
            ? EmoticonDetector.BoundaryProfile.Markup
            : EmoticonDetector.BoundaryProfile.Plain;

        Console.WriteLine($"[check] Mode: {mode} | Opening: {inputPath}");
        using var pkg = OoxmlPackage.Open(inputPath);
        Console.WriteLine($"[check] SHA-256: {pkg.Sha256Hex}");

        var detector  = new EmoticonDetector(emoticonProfile);
        var violations = new List<ViolationEntry>();

        if (mode == "preflight")
        {
            // Use shared PreflightValidator (package-based) for consistent preflight logic
            List<PreflightViolation> preflightViolations;
            try
            {
                preflightViolations = PreflightValidator.Validate(pkg, policy);
            }
            catch (InvalidDataException ex)
            {
                Console.Error.WriteLine($"[check] Formula/integrity error: {ex.Message}");
                return 2;
            }

            foreach (var v in preflightViolations)
            {
                string typeStr = v.Type switch
                {
                    ViolationType.NonTransformableSource => "non-transformable",
                    ViolationType.UnmappedEmoji          => "emoji-unmapped",
                    ViolationType.UnmappedEmoticon       => "emoticon-unmapped",
                    _                                    => v.Type.ToString()
                };
                violations.Add(MakeViolation(typeStr, v.Sequence, v.Location, v.Message));
            }
        }
        else // release
        {
            var locations = TextExtractor.ExtractAll(pkg);

            foreach (var loc in locations)
            {
                bool isCache = loc.Source == TextSource.FormulaCachedValue;
                string text  = loc.LogicalText;

                // Release: ANY emoji in ANY location blocks release — no policy exemption
                var emojiMatches = EmojiDetector.FindAll(text);
                foreach (var m in emojiMatches)
                    violations.Add(MakeViolation("emoji", m.Sequence, loc,
                        isCache
                            ? "Emoji in formula cached value — may originate from UNICHAR() or external source. Blocks release."
                            : "Emoji found in release scan. Output must be fully clean."));

                // Release: emoticons in non-formula text, formula literals AND cached values
                bool scanEmoticons = loc.Source is TextSource.FormulaLiteral or TextSource.DefinedName || !loc.IsFormula;
                if (scanEmoticons)
                {
                    var emoticonMatches = detector.FindAll(text);
                    foreach (var m in emoticonMatches)
                        violations.Add(MakeViolation("emoticon", m.Value, loc,
                            "Emoticon found in release scan. Output must be fully clean."));
                }
            }
        }

        // ── Calculation report verification (release only) ───────────────────
        string verificationState = mode == "preflight" ? "PREFLIGHT" : "STATIC_ONLY";

        if (requireCalc)
        {
            if (mode == "preflight")
            {
                Console.Error.WriteLine("[check] --require-calculated is not applicable in preflight mode.");
                return 1;
            }

            if (calcReportPath is null)
            {
                Console.Error.WriteLine("[check] --require-calculated requires --calculation-report <report.json>.");
                return 2;
            }

            if (!File.Exists(calcReportPath))
            {
                Console.Error.WriteLine($"[check] Calculation report not found: {calcReportPath}");
                return 2;
            }

            JsonDocument calcReport;
            try { calcReport = JsonDocument.Parse(File.ReadAllText(calcReportPath)); }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[check] Cannot parse calculation report: {ex.Message}");
                return 2;
            }

            bool calcSuccess = calcReport.RootElement.TryGetProperty("success", out var s) && s.GetBoolean();
            if (!calcSuccess)
            {
                Console.Error.WriteLine("[check] Calculation report indicates failure. Verification blocked.");
                return 2;
            }

            string? reportOutputHash = calcReport.RootElement
                .TryGetProperty("outputSha256", out var oh) ? oh.GetString() : null;

            if (reportOutputHash is null)
            {
                Console.Error.WriteLine("[check] Calculation report missing 'outputSha256'. Verification blocked.");
                return 2;
            }

            if (!reportOutputHash.Equals(pkg.Sha256Hex, StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine(
                    $"[check] Calculation report outputSha256 ({reportOutputHash}) " +
                    $"does not match input file SHA-256 ({pkg.Sha256Hex}). Verification blocked.");
                return 2;
            }

            verificationState = "CALCULATED_CANDIDATE";
            Console.WriteLine($"[check] Calculation report verified. State: {verificationState}");
        }

        // ── Output ───────────────────────────────────────────────────────────
        if (violations.Count == 0)
        {
            string finalState = verificationState == "CALCULATED_CANDIDATE" ? "VERIFIED" : verificationState;
            Console.WriteLine($"[check] No violations found. State: {finalState}");
            return 0;
        }

        Console.WriteLine($"[check] {violations.Count} violation(s) found. State: BLOCKED");
        foreach (var v in violations)
            Console.WriteLine($"  [{v.Type.ToUpperInvariant()}] {v.PartPath} | {v.SheetName}!{v.CellAddress} | {v.Sequence} — {v.Message}");

        string violationsJson = JsonSerializer.Serialize(violations, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        Console.Error.WriteLine(violationsJson);

        return 1;
    }

    private static ViolationEntry MakeViolation(string type, string sequence, TextLocation loc, string message)
        => new(type, sequence, ToCodepointList(sequence),
               loc.PartPath, loc.SheetName, loc.CellAddress, loc.Source.ToString(), message);

    private static List<string> ToCodepointList(string s)
    {
        var list = new List<string>();
        for (int i = 0; i < s.Length; )
        {
            int cp = char.ConvertToUtf32(s, i);
            list.Add($"U+{cp:X4}");
            i += char.IsHighSurrogate(s[i]) ? 2 : 1;
        }
        return list;
    }

    private sealed record ViolationEntry(
        string Type, string Sequence, List<string> Codepoints,
        string PartPath, string? SheetName, string? CellAddress,
        string Source, string Message);
}