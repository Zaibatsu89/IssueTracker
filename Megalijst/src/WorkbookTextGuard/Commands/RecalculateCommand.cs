using System;
using System.IO;
using System.Text.Json;

namespace WorkbookTextGuard.Commands;

/// <summary>
/// Triggers Excel COM recalculation on Windows via a separate STA worker process.
/// On non-Windows platforms, exits with code 2 and a clear diagnostic.
/// </summary>
internal static class RecalculateCommand
{
    public static int Run(string[] args)
    {
        string? inputPath  = null;
        string? outputPath = null;
        string? reportPath = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--input":  inputPath  = args[++i]; break;
                case "--output": outputPath = args[++i]; break;
                case "--report": reportPath = args[++i]; break;
                default:
                    Console.Error.WriteLine($"Unknown argument: {args[i]}");
                    return 1;
            }
        }

        if (inputPath is null || outputPath is null || reportPath is null)
        {
            Console.Error.WriteLine("recalculate: --input <file.xlsx> --output <out.xlsx> --report <report.json> are all required.");
            return 1;
        }

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"Input file not found: {inputPath}");
            return 1;
        }

        // Normalise all paths immediately after validation
        string absInput  = Path.GetFullPath(inputPath);
        string absOutput = Path.GetFullPath(outputPath);
        string absReport = Path.GetFullPath(reportPath);

        // Guard: no path collisions
        if (absInput.Equals(absOutput, StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("recalculate: --output must differ from --input.");
            return 1;
        }
        if (absReport.Equals(absInput, StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("recalculate: --report must differ from --input.");
            return 1;
        }
        if (absReport.Equals(absOutput, StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("recalculate: --report must differ from --output.");
            return 1;
        }

        if (!OperatingSystem.IsWindows())
        {
            Console.Error.WriteLine("[recalculate] Excel COM recalculation is only supported on Windows with Excel installed.");
            Console.Error.WriteLine("[recalculate] Static validation (inventory/check) remains available on all platforms.");
            WriteFailureReport(absReport, absInput, "Platform not supported: Excel COM requires Windows.");
            return 2;
        }

#if WINDOWS
        return RunWindows(absInput, absOutput, absReport);
#else
        Console.Error.WriteLine("[recalculate] This build was not compiled with Windows COM support.");
        WriteFailureReport(absReport, absInput, "Build does not include Windows COM adapter.");
        return 2;
#endif
    }

#if WINDOWS
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static int RunWindows(string inputPath, string outputPath, string reportPath)
    {
        return ExcelComAdapter.Recalculate(inputPath, outputPath, reportPath);
    }
#endif

    internal static void WriteFailureReport(string reportPath, string inputPath, string reason)
    {
        string inputSha256 = "";
        try
        {
            byte[] bytes = File.ReadAllBytes(inputPath);
            inputSha256 = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
        }
        catch { /* best-effort */ }

        var report = new
        {
            success      = false,
            inputFile    = inputPath,
            inputSha256,
            outputFile   = (string?)null,
            outputSha256 = (string?)null,
            excelVersion = (string?)null,
            reason,
            generatedUtc = DateTime.UtcNow.ToString("o")
        };

        File.WriteAllText(reportPath,
            JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }),
            System.Text.Encoding.UTF8);
    }
}