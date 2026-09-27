using System;
using System.IO;
using WorkbookTextGuard.Commands;

namespace WorkbookTextGuard;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        string mode = args[0].ToLowerInvariant();
        string[] rest = args[1..];

        try
        {
            return mode switch
            {
                "inventory"   => InventoryCommand.Run(rest),
                "check"       => CheckCommand.Run(rest),
                "clean"       => CleanCommand.Run(rest),
                "recalculate" => RecalculateCommand.Run(rest),
                "--help" or "-h" or "help" => Help(),
                _ => UnknownMode(mode)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[FATAL] {ex.Message}");
            if (ex.InnerException is not null)
                Console.Error.WriteLine($"  Caused by: {ex.InnerException.Message}");
            return 2;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("WorkbookTextGuard — OOXML emoji/emoticon inventory, check, clean and recalculate");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  WorkbookTextGuard inventory   --input <file.xlsx> [--output <report.json>]");
        Console.WriteLine("  WorkbookTextGuard check       --input <file.xlsx> --policy <policy.json> [--require-calculated] [--calculation-report <report.json>]");
        Console.WriteLine("  WorkbookTextGuard clean       --input <file.xlsx> --policy <policy.json> --output <out.xlsx>");
        Console.WriteLine("  WorkbookTextGuard recalculate --input <file.xlsx> --output <out.xlsx> --report <report.json>");
        Console.WriteLine();
        Console.WriteLine("Modes:");
        Console.WriteLine("  inventory    Scan workbook for emoji/emoticon sequences; produce a sorted report.");
        Console.WriteLine("  check        Validate workbook against policy; report violations without modifying.");
        Console.WriteLine("  clean        Apply approved policy mappings to produce a cleaned workbook.");
        Console.WriteLine("  recalculate  Trigger Excel COM recalculation (Windows only; requires Excel).");
    }

    private static int Help()
    {
        PrintUsage();
        return 0;
    }

    private static int UnknownMode(string mode)
    {
        Console.Error.WriteLine($"Unknown mode: '{mode}'. Use inventory, check, clean, or recalculate.");
        return 1;
    }
}