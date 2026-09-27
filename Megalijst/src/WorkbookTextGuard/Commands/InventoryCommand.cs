using System;
using System.IO;

namespace WorkbookTextGuard.Commands;

internal static class InventoryCommand
{
    public static int Run(string[] args)
    {
        string? inputPath  = null;
        string? outputPath = null;
        string  profile    = "plain";

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--input":   inputPath  = args[++i]; break;
                case "--output":  outputPath = args[++i]; break;
                case "--profile": profile    = args[++i]; break;
                default:
                    Console.Error.WriteLine($"Unknown argument: {args[i]}");
                    return 1;
            }
        }

        // Validate required arguments before any path operations
        if (inputPath is null)
        {
            Console.Error.WriteLine("inventory: --input <file.xlsx> is required.");
            return 1;
        }

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"Input file not found: {inputPath}");
            return 1;
        }

        // Normalise paths and guard against collisions
        string absInput = Path.GetFullPath(inputPath);
        if (outputPath is not null)
        {
            string absOutput = Path.GetFullPath(outputPath);
            if (absInput.Equals(absOutput, StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine("inventory: --output must differ from --input.");
                return 1;
            }
        }

        var emoticonProfile = profile.Equals("markup", StringComparison.OrdinalIgnoreCase)
            ? EmoticonDetector.BoundaryProfile.Markup
            : EmoticonDetector.BoundaryProfile.Plain;

        Console.WriteLine($"[inventory] Opening: {absInput}");
        using var pkg = OoxmlPackage.Open(absInput);
        Console.WriteLine($"[inventory] SHA-256: {pkg.Sha256Hex}");
        Console.WriteLine($"[inventory] Sheets: {pkg.Sheets.Count}");

        var locations = TextExtractor.ExtractAll(pkg);
        Console.WriteLine($"[inventory] Text locations found: {locations.Count}");

        var detector = new EmoticonDetector(emoticonProfile);
        var report   = InventoryReport.Build(pkg, locations, detector);

        string json = report.ToJson();

        if (outputPath is not null)
        {
            // Write with explicit UTF-8 without BOM
            File.WriteAllText(outputPath, json, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Console.WriteLine($"[inventory] Report written to: {outputPath}");
        }
        else
        {
            Console.WriteLine(json);
        }

        int emojiCount    = report.EmojiSequences.Count;
        int emoticonCount = report.EmoticonMatches.Count;

        Console.WriteLine($"[inventory] Unique emoji sequences: {emojiCount}");
        Console.WriteLine($"[inventory] Unique emoticon patterns: {emoticonCount}");

        if (emojiCount > 0 || emoticonCount > 0)
        {
            Console.WriteLine("[inventory] Emoji/emoticon content detected. Review report and define policy.json before running 'clean'.");
            return 0;
        }

        Console.WriteLine("[inventory] No emoji or emoticon content detected.");
        return 0;
    }
}