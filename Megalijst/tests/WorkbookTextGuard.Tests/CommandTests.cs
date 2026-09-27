using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using WorkbookTextGuard;
using WorkbookTextGuard.Commands;
using Xunit;

namespace WorkbookTextGuard.Tests;

/// <summary>
/// Integration tests for CLI commands using synthetic XLSX fixtures.
/// No real workbook is modified.
/// </summary>
public class CommandTests
{
    // ── Fixture helpers ───────────────────────────────────────────────────────

    private static byte[] BuildXlsx(string sharedStringValue)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(zip, "[Content_Types].xml", """
                <?xml version="1.0" encoding="UTF-8"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                </Types>
                """);
            AddEntry(zip, "_rels/.rels", """
                <?xml version="1.0" encoding="UTF-8"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
            AddEntry(zip, "xl/workbook.xml", """
                <?xml version="1.0" encoding="UTF-8"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets><sheet name="Sheet1" sheetId="1" r:id="rId1"/></sheets>
                </workbook>
                """);
            AddEntry(zip, "xl/_rels/workbook.xml.rels", """
                <?xml version="1.0" encoding="UTF-8"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>
                </Relationships>
                """);
            AddEntry(zip, "xl/worksheets/sheet1.xml", """
                <?xml version="1.0" encoding="UTF-8"?>
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <sheetData><row r="1"><c r="A1" t="s"><v>0</v></c></row></sheetData>
                </worksheet>
                """);
            string escaped = sharedStringValue
                .Replace("&","&amp;").Replace("<","&lt;").Replace(">","&gt;");
            AddEntry(zip, "xl/sharedStrings.xml",
                $"""<?xml version="1.0" encoding="UTF-8"?><sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="1" uniqueCount="1"><si><t>{escaped}</t></si></sst>""");
        }
        return ms.ToArray();
    }

    private static void AddEntry(ZipArchive zip, string name, string content)
    {
        var e = zip.CreateEntry(name);
        using var s = e.Open();
        s.Write(Encoding.UTF8.GetBytes(content.Trim()));
    }

    private static string WriteTemp(byte[] bytes, string ext = ".xlsx")
    {
        string p = Path.GetTempFileName() + ext;
        File.WriteAllBytes(p, bytes);
        return p;
    }

    private static string WritePolicy(string json)
    {
        string p = Path.GetTempFileName();
        File.WriteAllText(p, json);
        return p;
    }

    private static string MappedPolicy() => WritePolicy("""
        {
          "emojiMappings": [{ "sequence": "⚠️", "replacement": "Aandachtspunt" }],
          "emoticonMappings": [],
          "decorativeSequences": []
        }
        """);

    private static string EmptyPolicy() => WritePolicy("""
        {"emojiMappings":[],"emoticonMappings":[],"decorativeSequences":[]}
        """);

    private static void Cleanup(params string[] paths)
    {
        foreach (var p in paths)
            try { if (File.Exists(p)) File.Delete(p); } catch { }
    }

    // ── check --mode preflight ────────────────────────────────────────────────

    [Fact]
    public void Check_Preflight_MappedInput_Returns0()
    {
        string input  = WriteTemp(BuildXlsx("Status ⚠️ einde"));
        string policy = MappedPolicy();
        try
        {
            int rc = CheckCommand.Run(new[] { "--input", input, "--policy", policy, "--mode", "preflight" });
            Assert.Equal(0, rc);
        }
        finally { Cleanup(input, policy); }
    }

    [Fact]
    public void Check_Preflight_UnmappedInput_Returns1()
    {
        string input  = WriteTemp(BuildXlsx("Status ⚠️ einde"));
        string policy = EmptyPolicy(); // ⚠️ not in policy
        try
        {
            int rc = CheckCommand.Run(new[] { "--input", input, "--policy", policy, "--mode", "preflight" });
            Assert.Equal(1, rc);
        }
        finally { Cleanup(input, policy); }
    }

    [Fact]
    public void Check_Preflight_CleanInput_Returns0()
    {
        string input  = WriteTemp(BuildXlsx("Geen emoji hier"));
        string policy = EmptyPolicy();
        try
        {
            int rc = CheckCommand.Run(new[] { "--input", input, "--policy", policy, "--mode", "preflight" });
            Assert.Equal(0, rc);
        }
        finally { Cleanup(input, policy); }
    }

    // ── check --mode release ──────────────────────────────────────────────────

    [Fact]
    public void Check_Release_CleanInput_Returns0()
    {
        string input  = WriteTemp(BuildXlsx("Schone tekst"));
        string policy = EmptyPolicy();
        try
        {
            int rc = CheckCommand.Run(new[] { "--input", input, "--policy", policy, "--mode", "release" });
            Assert.Equal(0, rc);
        }
        finally { Cleanup(input, policy); }
    }

    [Fact]
    public void Check_Release_EmojiPresent_Returns1()
    {
        string input  = WriteTemp(BuildXlsx("Status ⚠️ einde"));
        string policy = MappedPolicy(); // policy gives no exemption in release mode
        try
        {
            int rc = CheckCommand.Run(new[] { "--input", input, "--policy", policy, "--mode", "release" });
            Assert.Equal(1, rc);
        }
        finally { Cleanup(input, policy); }
    }

    // ── check --require-calculated ────────────────────────────────────────────

    [Fact]
    public void Check_RequireCalculated_MissingReport_Returns2()
    {
        string input  = WriteTemp(BuildXlsx("Schone tekst"));
        string policy = EmptyPolicy();
        try
        {
            int rc = CheckCommand.Run(new[]
            {
                "--input", input, "--policy", policy,
                "--mode", "release",
                "--require-calculated", "--calculation-report", "/nonexistent/report.json"
            });
            Assert.Equal(2, rc);
        }
        finally { Cleanup(input, policy); }
    }

    [Fact]
    public void Check_RequireCalculated_FailedReport_Returns2()
    {
        string input  = WriteTemp(BuildXlsx("Schone tekst"));
        string policy = EmptyPolicy();
        string report = WritePolicy("""{"success":false,"inputSha256":"","outputSha256":""}""");
        try
        {
            int rc = CheckCommand.Run(new[]
            {
                "--input", input, "--policy", policy,
                "--mode", "release",
                "--require-calculated", "--calculation-report", report
            });
            Assert.Equal(2, rc);
        }
        finally { Cleanup(input, policy, report); }
    }

    [Fact]
    public void Check_RequireCalculated_HashMismatch_Returns2()
    {
        string input  = WriteTemp(BuildXlsx("Schone tekst"));
        string policy = EmptyPolicy();
        string report = WritePolicy("""{"success":true,"inputSha256":"aabbcc","outputSha256":"0000000000000000000000000000000000000000000000000000000000000000"}""");
        try
        {
            int rc = CheckCommand.Run(new[]
            {
                "--input", input, "--policy", policy,
                "--mode", "release",
                "--require-calculated", "--calculation-report", report
            });
            Assert.Equal(2, rc);
        }
        finally { Cleanup(input, policy, report); }
    }

    [Fact]
    public void Check_RequireCalculated_CorrectOutputHash_Returns0()
    {
        byte[] bytes  = BuildXlsx("Schone tekst");
        string input  = WriteTemp(bytes);
        string policy = EmptyPolicy();
        string sha256 = OoxmlPackage.ComputeSha256(bytes);
        // Report's outputSha256 must match the input file's hash
        string report = WritePolicy($"{{\"success\":true,\"inputSha256\":\"{sha256}\",\"outputSha256\":\"{sha256}\"}}");
        try
        {
            int rc = CheckCommand.Run(new[]
            {
                "--input", input, "--policy", policy,
                "--mode", "release",
                "--require-calculated", "--calculation-report", report
            });
            Assert.Equal(0, rc);
        }
        finally { Cleanup(input, policy, report); }
    }

    // ── clean ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Clean_MappedInput_Succeeds_OutputExists()
    {
        string input  = WriteTemp(BuildXlsx("Status ⚠️ einde"));
        string policy = MappedPolicy();
        string output = input + ".out.xlsx";
        string backup = input + ".bak";
        try
        {
            int rc = CleanCommand.Run(new[]
            {
                "--input", input, "--policy", policy, "--output", output
            });
            Assert.Equal(0, rc);
            Assert.True(File.Exists(output));
            Assert.True(File.Exists(backup));
        }
        finally { Cleanup(input, policy, output, backup); }
    }

    [Fact]
    public void Clean_UnmappedInput_Returns1_NoOutputCreated()
    {
        string input  = WriteTemp(BuildXlsx("Status ⚠️ einde"));
        string policy = EmptyPolicy();
        string output = input + ".out.xlsx";
        try
        {
            int rc = CleanCommand.Run(new[]
            {
                "--input", input, "--policy", policy, "--output", output
            });
            Assert.Equal(1, rc);
            Assert.False(File.Exists(output));
        }
        finally { Cleanup(input, policy, output, input + ".bak"); }
    }

    [Fact]
    public void Clean_CorrectExpectedHash_Succeeds()
    {
        byte[] bytes  = BuildXlsx("Status ⚠️ einde");
        string input  = WriteTemp(bytes);
        string policy = MappedPolicy();
        string output = input + ".out.xlsx";
        string sha256 = OoxmlPackage.ComputeSha256(bytes);
        try
        {
            int rc = CleanCommand.Run(new[]
            {
                "--input", input, "--policy", policy, "--output", output,
                "--expected-sha256", sha256
            });
            Assert.Equal(0, rc);
        }
        finally { Cleanup(input, policy, output, input + ".bak"); }
    }

    [Fact]
    public void Clean_WrongExpectedHash_Returns2_NoFilesChanged()
    {
        string input  = WriteTemp(BuildXlsx("Status ⚠️ einde"));
        string policy = MappedPolicy();
        string output = input + ".out.xlsx";
        string backup = input + ".bak";
        try
        {
            int rc = CleanCommand.Run(new[]
            {
                "--input", input, "--policy", policy, "--output", output,
                "--expected-sha256", new string('0', 64)
            });
            Assert.Equal(2, rc);
            Assert.False(File.Exists(output), "Output must not be created on hash mismatch.");
            Assert.False(File.Exists(backup), "Backup must not be created on hash mismatch.");
        }
        finally { Cleanup(input, policy, output, backup); }
    }

    [Fact]
    public void Clean_InvalidHashLength_Returns2()
    {
        string input  = WriteTemp(BuildXlsx("Status ⚠️ einde"));
        string policy = MappedPolicy();
        string output = input + ".out.xlsx";
        try
        {
            int rc = CleanCommand.Run(new[]
            {
                "--input", input, "--policy", policy, "--output", output,
                "--expected-sha256", "tooshort"
            });
            Assert.Equal(2, rc);
        }
        finally { Cleanup(input, policy, output, input + ".bak"); }
    }

    [Fact]
    public void Clean_InputEqualsOutput_Returns1()
    {
        string input  = WriteTemp(BuildXlsx("tekst"));
        string policy = EmptyPolicy();
        try
        {
            int rc = CleanCommand.Run(new[]
            {
                "--input", input, "--policy", policy, "--output", input
            });
            Assert.Equal(1, rc);
        }
        finally { Cleanup(input, policy); }
    }

    [Fact]
    public void Clean_ExistingBackup_Returns1_NoOverwrite()
    {
        string input  = WriteTemp(BuildXlsx("tekst"));
        string policy = MappedPolicy();
        string output = input + ".out.xlsx";
        string backup = input + ".bak";
        File.WriteAllText(backup, "existing backup");
        try
        {
            int rc = CleanCommand.Run(new[]
            {
                "--input", input, "--policy", policy, "--output", output
            });
            Assert.Equal(1, rc);
            // Existing backup content must be preserved
            Assert.Equal("existing backup", File.ReadAllText(backup));
        }
        finally { Cleanup(input, policy, output, backup); }
    }

    [Fact]
    public void Clean_ExistingOutput_Returns1_NoOverwrite()
    {
        string input  = WriteTemp(BuildXlsx("tekst"));
        string policy = MappedPolicy();
        string output = input + ".out.xlsx";
        File.WriteAllText(output, "existing output");
        try
        {
            int rc = CleanCommand.Run(new[]
            {
                "--input", input, "--policy", policy, "--output", output
            });
            Assert.Equal(1, rc);
            Assert.Equal("existing output", File.ReadAllText(output));
        }
        finally { Cleanup(input, policy, output, input + ".bak"); }
    }

    // ── inventory ─────────────────────────────────────────────────────────────

    [Fact]
    public void Inventory_WithEmoji_Returns0_ReportContainsSequence()
    {
        string input  = WriteTemp(BuildXlsx("Status ⚠️ einde"));
        string report = input + ".report.json";
        try
        {
            int rc = InventoryCommand.Run(new[] { "--input", input, "--output", report });
            Assert.Equal(0, rc);
            Assert.True(File.Exists(report));
            string json = File.ReadAllText(report);
            Assert.Contains("26A0", json); // U+26A0 is part of ⚠️
        }
        finally { Cleanup(input, report); }
    }

    [Fact]
    public void Inventory_NoEmoji_Returns0()
    {
        string input = WriteTemp(BuildXlsx("Gewone tekst"));
        try
        {
            int rc = InventoryCommand.Run(new[] { "--input", input });
            Assert.Equal(0, rc);
        }
        finally { Cleanup(input); }
    }

    // ── recalculate (non-Windows) ─────────────────────────────────────────────

    [Fact]
    public void Recalculate_NonWindows_Returns2()
    {
        if (OperatingSystem.IsWindows()) return; // skip on Windows

        string input  = WriteTemp(BuildXlsx("tekst"));
        string output = input + ".calc.xlsx";
        string report = input + ".report.json";
        try
        {
            int rc = RecalculateCommand.Run(new[]
            {
                "--input", input, "--output", output, "--report", report
            });
            Assert.Equal(2, rc);
            Assert.True(File.Exists(report));
            var doc = JsonDocument.Parse(File.ReadAllText(report));
            Assert.False(doc.RootElement.GetProperty("success").GetBoolean());
        }
        finally { Cleanup(input, output, report); }
    }
}