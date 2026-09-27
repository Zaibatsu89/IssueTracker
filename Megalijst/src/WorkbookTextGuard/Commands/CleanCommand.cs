using System;
using System.IO;
using System.Text.RegularExpressions;

namespace WorkbookTextGuard.Commands;

internal static class CleanCommand
{
    private static readonly Regex Sha256Regex = new(@"^[0-9a-fA-F]{64}$", RegexOptions.Compiled);

    public static int Run(string[] args)
    {
        string? inputPath      = null;
        string? policyPath     = null;
        string? outputPath     = null;
        string? expectedSha256 = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--input":           inputPath      = args[++i]; break;
                case "--policy":          policyPath     = args[++i]; break;
                case "--output":          outputPath     = args[++i]; break;
                case "--expected-sha256": expectedSha256 = args[++i]; break;
                default:
                    Console.Error.WriteLine($"Unknown argument: {args[i]}");
                    return 1;
            }
        }

        if (inputPath is null || policyPath is null || outputPath is null)
        {
            Console.Error.WriteLine("clean: --input <file.xlsx> --policy <policy.json> --output <out.xlsx> are all required.");
            return 1;
        }

        if (expectedSha256 is not null && !Sha256Regex.IsMatch(expectedSha256))
        {
            Console.Error.WriteLine($"clean: --expected-sha256 must be exactly 64 hex characters. Got: '{expectedSha256}'");
            return 2;
        }

        if (!File.Exists(inputPath))  { Console.Error.WriteLine($"Input file not found: {inputPath}");  return 1; }
        if (!File.Exists(policyPath)) { Console.Error.WriteLine($"Policy file not found: {policyPath}"); return 1; }

        string absInput  = Path.GetFullPath(inputPath);
        string absOutput = Path.GetFullPath(outputPath);
        string absPolicy = Path.GetFullPath(policyPath);
        string backupPath = absInput + ".bak";

        if (absInput.Equals(absOutput, StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("clean: --output must differ from --input.");
            return 1;
        }
        if (absOutput.Equals(absPolicy, StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("clean: --output must differ from --policy.");
            return 1;
        }
        if (File.Exists(backupPath))
        {
            Console.Error.WriteLine($"clean: Backup already exists at {backupPath}. Remove or rename it before running clean.");
            return 1;
        }
        if (File.Exists(absOutput))
        {
            Console.Error.WriteLine($"clean: Output file already exists at {absOutput}. Choose a different path or remove it first.");
            return 1;
        }

        // ── Read source exactly once ─────────────────────────────────────────
        byte[] sourceBytes;
        try { sourceBytes = File.ReadAllBytes(absInput); }
        catch (Exception ex) { Console.Error.WriteLine($"clean: Cannot read input file: {ex.Message}"); return 2; }

        string actualSha256 = OoxmlPackage.ComputeSha256(sourceBytes);
        Console.WriteLine($"[clean] Input SHA-256: {actualSha256}");

        if (expectedSha256 is not null
            && !actualSha256.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine($"[clean] SHA-256 mismatch: expected {expectedSha256}, actual {actualSha256}. Aborting before any file change.");
            return 2;
        }

        // ── Load policy ──────────────────────────────────────────────────────
        Policy policy;
        try { policy = Policy.Load(absPolicy); }
        catch (Exception ex) { Console.Error.WriteLine($"[clean] Failed to load policy: {ex.Message}"); return 1; }
        Console.WriteLine($"[clean] Policy: {policy.EmojiMappings.Count} emoji mapping(s), {policy.EmoticonMappings.Count} emoticon mapping(s).");

        // ── Open package from snapshot for preflight ─────────────────────────
        OoxmlPackage pkg;
        try { pkg = OoxmlPackage.Open(sourceBytes, actualSha256); }
        catch (Exception ex) { Console.Error.WriteLine($"[clean] Failed to open workbook: {ex.Message}"); return 2; }

        Console.WriteLine("[clean] Running preflight check...");
        int preflightResult;
        using (pkg)
        {
            preflightResult = RunPreflight(pkg, policy);
        }

        if (preflightResult == 2)
        {
            Console.Error.WriteLine("[clean] Preflight blocked by formula/integrity error (exit 2). No files changed.");
            return 2;
        }
        if (preflightResult != 0)
        {
            Console.Error.WriteLine("[clean] Preflight failed: unmapped/non-transformable sequences detected. No files changed.");
            return 1;
        }

        // ── Write backup from snapshot (non-overwriting) ──────────────────────
        try
        {
            using var backupStream = new FileStream(backupPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            backupStream.Write(sourceBytes);
            Console.WriteLine($"[clean] Backup written: {backupPath}");
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"[clean] Failed to write backup: {ex.Message}");
            return 2;
        }

        // ── Re-open from same snapshot for cleaning ───────────────────────────
        pkg = OoxmlPackage.Open(sourceBytes, actualSha256);

        // ── Reserve temporary output atomically ───────────────────────────────
        string outputDir = Path.GetDirectoryName(absOutput) ?? ".";
        string tmpOutput = Path.Combine(outputDir, $".tmp_{Guid.NewGuid():N}.xlsx");
        FileStream? tmpStream = null;
        try
        {
            tmpStream = new FileStream(tmpOutput, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"[clean] Failed to reserve temporary output file: {ex.Message}");
            pkg.Dispose();
            return 2;
        }

        try
        {
            tmpStream.Dispose();
            tmpStream = null;

            using (pkg)
            {
                var cleaner = new WorkbookCleaner(pkg, policy);
                var result  = cleaner.Clean(tmpOutput);
                foreach (var entry in result.Log)
                    Console.WriteLine(entry);
            }

            File.Move(tmpOutput, absOutput, overwrite: false);
            Console.WriteLine($"[clean] Output published: {absOutput}");
            Console.WriteLine("[clean] Run 'check' on the output to verify. If formulas exist, run 'recalculate' before release.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[clean] Error during cleaning: {ex.Message}");
            try { if (File.Exists(tmpOutput)) File.Delete(tmpOutput); } catch { }
            return 2;
        }
        finally
        {
            tmpStream?.Dispose();
            pkg.Dispose();
        }
    }

    /// <summary>
    /// Preflight using the shared PreflightValidator (package-based, with SST cell context).
    /// Returns 0 = clean, 1 = policy violations, 2 = formula/integrity error.
    /// </summary>
    private static int RunPreflight(OoxmlPackage pkg, Policy policy)
    {
        List<PreflightViolation> violations;
        try
        {
            violations = PreflightValidator.Validate(pkg, policy);
        }
        catch (InvalidDataException ex)
        {
            Console.Error.WriteLine($"[preflight] Formula/integrity error: {ex.Message}");
            return 2;
        }

        foreach (var v in violations)
            Console.Error.WriteLine(
                $"[preflight] {v.Type}: {v.Sequence} in {v.Location.PartPath} " +
                $"{v.Location.SheetName}!{v.Location.CellAddress} — {v.Message}");

        return violations.Count > 0 ? 1 : 0;
    }
}