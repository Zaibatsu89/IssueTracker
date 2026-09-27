using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using WorkbookTextGuard;
using Xunit;

namespace WorkbookTextGuard.Tests;

/// <summary>
/// Unit tests for WorkbookCleaner and TextReplacement using synthetic XLSX fixtures.
/// No real workbook is modified.
/// </summary>
public class WorkbookCleanerTests
{
    private static readonly XNamespace Ns = OoxmlNamespaces.SpreadsheetMl;

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Build a minimal in-memory XLSX with one sheet and a shared strings part.</summary>
    private static byte[] BuildMinimalXlsx(
        string sharedStringValue,
        bool richText = false,
        string? run1 = null,
        string? run2 = null)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            // [Content_Types].xml
            AddEntry(zip, "[Content_Types].xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                </Types>
                """);

            // _rels/.rels
            AddEntry(zip, "_rels/.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);

            // xl/workbook.xml
            AddEntry(zip, "xl/workbook.xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="Sheet1" sheetId="1" r:id="rId1"/>
                  </sheets>
                </workbook>
                """);

            // xl/_rels/workbook.xml.rels
            AddEntry(zip, "xl/_rels/workbook.xml.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>
                </Relationships>
                """);

            // xl/worksheets/sheet1.xml — cell A1 references shared string 0
            AddEntry(zip, "xl/worksheets/sheet1.xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <sheetData>
                    <row r="1">
                      <c r="A1" t="s"><v>0</v></c>
                    </row>
                  </sheetData>
                </worksheet>
                """);

            // xl/sharedStrings.xml
            string siContent;
            if (richText && run1 is not null && run2 is not null)
            {
                siContent = $"""
                    <si>
                      <r><t>{EscapeXml(run1)}</t></r>
                      <r><t>{EscapeXml(run2)}</t></r>
                    </si>
                    """;
            }
            else
            {
                siContent = $"<si><t>{EscapeXml(sharedStringValue)}</t></si>";
            }

            AddEntry(zip, "xl/sharedStrings.xml", $"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="1" uniqueCount="1">
                  {siContent}
                </sst>
                """);
        }
        return ms.ToArray();
    }

    private static void AddEntry(ZipArchive zip, string name, string content)
    {
        var entry = zip.CreateEntry(name);
        using var stream = entry.Open();
        var bytes = Encoding.UTF8.GetBytes(content.Trim());
        stream.Write(bytes);
    }

    private static string EscapeXml(string s)
        => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
            .Replace("\"", "&quot;").Replace("'", "&apos;");

    private static Policy BuildPolicy(params (string seq, string rep)[] mappings)
    {
        string path = Path.GetTempFileName();
        var sb = new StringBuilder();
        sb.AppendLine("{\"emojiMappings\":[");
        for (int i = 0; i < mappings.Length; i++)
        {
            sb.Append($"{{\"sequence\":\"{mappings[i].seq}\",\"replacement\":\"{mappings[i].rep}\"}}");
            if (i < mappings.Length - 1) sb.Append(",");
        }
        sb.AppendLine("],\"emoticonMappings\":[],\"decorativeSequences\":[]}");
        File.WriteAllText(path, sb.ToString());
        var policy = Policy.Load(path);
        File.Delete(path);
        return policy;
    }

    private static (OoxmlPackage pkg, string tmpPath) OpenTempXlsx(byte[] bytes)
    {
        string tmp = Path.GetTempFileName() + ".xlsx";
        File.WriteAllBytes(tmp, bytes);
        return (OoxmlPackage.Open(tmp), tmp);
    }

    private static string ReadSharedString(string outputPath)
    {
        using var zip = ZipFile.OpenRead(outputPath);
        var entry = zip.GetEntry("xl/sharedStrings.xml")!;
        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        XNamespace ns = OoxmlNamespaces.SpreadsheetMl;
        // Concatenate all <t> values
        return string.Concat(doc.Descendants(ns + "t").Select(t => t.Value));
    }

    // ── TextEditList tests ────────────────────────────────────────────────────

    [Fact]
    public void TextEditList_SingleEdit_Applied()
    {
        var el = new TextEditList();
        el.Add("Hello ⚠️ World", 6, "⚠️".Length, "Aandachtspunt");
        string result = el.Apply("Hello ⚠️ World");
        Assert.Equal("Hello Aandachtspunt World", result);
    }

    [Fact]
    public void TextEditList_MultipleEdits_RightToLeft()
    {
        string text = "⚠️ en ✅";
        var el = new TextEditList();
        // Find positions
        int pos1 = text.IndexOf("⚠️", StringComparison.Ordinal);
        int pos2 = text.IndexOf("✅", StringComparison.Ordinal);
        el.Add(text, pos1, "⚠️".Length, "WARN");
        el.Add(text, pos2, "✅".Length, "OK");
        string result = el.Apply(text);
        Assert.Equal("WARN en OK", result);
    }

    [Fact]
    public void TextEditList_OverlappingEdits_FirstWins()
    {
        string text = "⚠️";
        var el = new TextEditList();
        el.Add(text, 0, "⚠️".Length, "WARN");
        el.Add(text, 0, 1, "X"); // overlapping shorter match
        string result = el.Apply(text);
        Assert.Equal("WARN", result);
    }

    [Fact]
    public void TextEditList_EmptyReplacement_RemovesText()
    {
        string text = "Prefix ⚠️ Suffix";
        var el = new TextEditList();
        int pos = text.IndexOf("⚠️", StringComparison.Ordinal);
        el.Add(text, pos, "⚠️".Length, "");
        string result = el.Apply(text);
        Assert.Equal("Prefix  Suffix", result);
    }

    [Fact]
    public void TextEditList_SurrogatePairWithinMatch_Allowed()
    {
        // 🔴 is a surrogate pair (U+1F534)
        string text = "Status 🔴 einde";
        var el = new TextEditList();
        int pos = text.IndexOf("🔴", StringComparison.Ordinal);
        el.Add(text, pos, "🔴".Length, "Geblokkeerd");
        string result = el.Apply(text);
        Assert.Equal("Status Geblokkeerd einde", result);
    }

    [Fact]
    public void TextEditList_SplitSurrogatePair_Throws()
    {
        string text = "🔴"; // high surrogate at 0, low at 1
        var el = new TextEditList();
        // Try to start edit at position 1 (inside surrogate pair)
        Assert.Throws<ArgumentException>(() => el.Add(text, 1, 1, "X"));
    }

    // ── RunOffsetMap tests ────────────────────────────────────────────────────

    [Fact]
    public void RunOffsetMap_PlainT_LogicalTextCorrect()
    {
        var si = new XElement(Ns + "si", new XElement(Ns + "t", "Hallo ⚠️"));
        var map = new RunOffsetMap(si);
        Assert.Equal("Hallo ⚠️", map.LogicalText);
    }

    [Fact]
    public void RunOffsetMap_RichText_LogicalTextConcatenated()
    {
        var si = new XElement(Ns + "si",
            new XElement(Ns + "r", new XElement(Ns + "t", "Hallo ")),
            new XElement(Ns + "r", new XElement(Ns + "t", "⚠️")));
        var map = new RunOffsetMap(si);
        Assert.Equal("Hallo ⚠️", map.LogicalText);
    }

    [Fact]
    public void RunOffsetMap_ApplyEdits_PlainT_Updated()
    {
        var si = new XElement(Ns + "si", new XElement(Ns + "t", "Status ⚠️ einde"));
        var map = new RunOffsetMap(si);
        string text = map.LogicalText;
        int pos = text.IndexOf("⚠️", StringComparison.Ordinal);
        var edits = new List<TextEdit> { new(pos, "⚠️".Length, "Aandachtspunt") };
        map.ApplyEdits(si, edits);
        Assert.Equal("Status Aandachtspunt einde", si.Element(Ns + "t")!.Value);
    }

    [Fact]
    public void RunOffsetMap_ApplyEdits_RichText_FirstRunGetsReplacement()
    {
        // ⚠️ split across two runs: U+26A0 in run1, U+FE0F in run2
        var si = new XElement(Ns + "si",
            new XElement(Ns + "r",
                new XElement(Ns + "rPr", new XElement(Ns + "b")), // bold formatting
                new XElement(Ns + "t", "Status \u26A0")),
            new XElement(Ns + "r",
                new XElement(Ns + "t", "\uFE0F einde")));

        var map = new RunOffsetMap(si);
        string text = map.LogicalText; // "Status ⚠️ einde"
        int pos = text.IndexOf("\u26A0", StringComparison.Ordinal);
        var edits = new List<TextEdit> { new(pos, "⚠️".Length, "Aandachtspunt") };
        map.ApplyEdits(si, edits);

        // Concatenate all remaining <t> values
        string result = string.Concat(si.Descendants(Ns + "t").Select(t => t.Value));
        Assert.Equal("Status Aandachtspunt einde", result);

        // Bold formatting on first run must be preserved
        var firstRun = si.Elements(Ns + "r").First();
        Assert.NotNull(firstRun.Element(Ns + "rPr"));
    }

    [Fact]
    public void RunOffsetMap_ApplyEdits_EmptyCell_OneEmptyT()
    {
        // Cell contains only an emoji — after removal, exactly one empty <t/> remains
        var si = new XElement(Ns + "si", new XElement(Ns + "t", "⚠️"));
        var map = new RunOffsetMap(si);
        string text = map.LogicalText;
        var edits = new List<TextEdit> { new(0, text.Length, "") };
        map.ApplyEdits(si, edits);

        var tElements = si.Descendants(Ns + "t").ToList();
        Assert.Single(tElements);
        Assert.Equal("", tElements[0].Value);
    }

    // ── WorkbookCleaner integration tests ─────────────────────────────────────

    [Fact]
    public void Cleaner_PlainSharedString_EmojiReplaced()
    {
        var bytes = BuildMinimalXlsx("Status ⚠️ einde");
        var policy = BuildPolicy(("⚠️", "Aandachtspunt"));
        var (pkg, tmp) = OpenTempXlsx(bytes);
        string output = tmp + ".out.xlsx";

        try
        {
            var cleaner = new WorkbookCleaner(pkg, policy);
            var result = cleaner.Clean(output);
            pkg.Dispose();

            string ss = ReadSharedString(output);
            Assert.Equal("Status Aandachtspunt einde", ss);
        }
        finally
        {
            pkg.Dispose();
            if (File.Exists(tmp)) File.Delete(tmp);
            if (File.Exists(output)) File.Delete(output);
        }
    }

    [Fact]
    public void Cleaner_DecorativeEmoji_Removed()
    {
        var bytes = BuildMinimalXlsx("⏱️ Start timer");
        var policy = BuildPolicy(("⏱️", ""));
        var (pkg, tmp) = OpenTempXlsx(bytes);
        string output = tmp + ".out.xlsx";

        try
        {
            var cleaner = new WorkbookCleaner(pkg, policy);
            cleaner.Clean(output);
            pkg.Dispose();

            string ss = ReadSharedString(output);
            Assert.Equal(" Start timer", ss);
        }
        finally
        {
            pkg.Dispose();
            if (File.Exists(tmp)) File.Delete(tmp);
            if (File.Exists(output)) File.Delete(output);
        }
    }

    [Fact]
    public void Cleaner_RichText_EmojiSplitAcrossRuns_Replaced()
    {
        // ⚠️ split: U+26A0 in run1, U+FE0F in run2
        var bytes = BuildMinimalXlsx("", richText: true, run1: "Status \u26A0", run2: "\uFE0F einde");
        var policy = BuildPolicy(("⚠️", "Aandachtspunt"));
        var (pkg, tmp) = OpenTempXlsx(bytes);
        string output = tmp + ".out.xlsx";

        try
        {
            var cleaner = new WorkbookCleaner(pkg, policy);
            cleaner.Clean(output);
            pkg.Dispose();

            string ss = ReadSharedString(output);
            Assert.Equal("Status Aandachtspunt einde", ss);
        }
        finally
        {
            pkg.Dispose();
            if (File.Exists(tmp)) File.Delete(tmp);
            if (File.Exists(output)) File.Delete(output);
        }
    }

    [Fact]
    public void Cleaner_UnmodifiedParts_ByteIdentical()
    {
        var bytes = BuildMinimalXlsx("Geen emoji hier");
        var policy = BuildPolicy(("⚠️", "Aandachtspunt"));
        var (pkg, tmp) = OpenTempXlsx(bytes);
        string output = tmp + ".out.xlsx";

        try
        {
            // Compute SHA-256 of uncompressed sharedStrings before clean
            string beforeHash = pkg.PartSha256("xl/sharedStrings.xml");

            var cleaner = new WorkbookCleaner(pkg, policy);
            cleaner.Clean(output);
            pkg.Dispose();

            // Read output and check sharedStrings unchanged
            using var outZip = ZipFile.OpenRead(output);
            var entry = outZip.GetEntry("xl/sharedStrings.xml")!;
            using var stream = entry.Open();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            string afterHash = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(ms.ToArray())).ToLowerInvariant();

            Assert.Equal(beforeHash, afterHash);
        }
        finally
        {
            pkg.Dispose();
            if (File.Exists(tmp)) File.Delete(tmp);
            if (File.Exists(output)) File.Delete(output);
        }
    }

    [Fact]
    public void Cleaner_Idempotent_SecondCleanNoChange()
    {
        var bytes = BuildMinimalXlsx("Status ⚠️ einde");
        var policy = BuildPolicy(("⚠️", "Aandachtspunt"));
        var (pkg, tmp) = OpenTempXlsx(bytes);
        string output1 = tmp + ".out1.xlsx";
        string output2 = tmp + ".out2.xlsx";

        try
        {
            new WorkbookCleaner(pkg, policy).Clean(output1);
            pkg.Dispose();

            var (pkg2, _) = OpenTempXlsx(File.ReadAllBytes(output1));
            new WorkbookCleaner(pkg2, policy).Clean(output2);
            pkg2.Dispose();

            string ss1 = ReadSharedString(output1);
            string ss2 = ReadSharedString(output2);
            Assert.Equal(ss1, ss2);
        }
        finally
        {
            pkg.Dispose();
            if (File.Exists(tmp)) File.Delete(tmp);
            if (File.Exists(output1)) File.Delete(output1);
            if (File.Exists(output2)) File.Delete(output2);
        }
    }

    [Fact]
    public void Cleaner_MultipleEmojisInOneCell_AllReplaced()
    {
        var bytes = BuildMinimalXlsx("🔴 Geblokkeerd 🟡 In behandeling 🟢 Gereed");
        var policy = BuildPolicy(
            ("🔴", "Rood"),
            ("🟡", "Geel"),
            ("🟢", "Groen"));
        var (pkg, tmp) = OpenTempXlsx(bytes);
        string output = tmp + ".out.xlsx";

        try
        {
            new WorkbookCleaner(pkg, policy).Clean(output);
            pkg.Dispose();

            string ss = ReadSharedString(output);
            Assert.Equal("Rood Geblokkeerd Geel In behandeling Groen Gereed", ss);
        }
        finally
        {
            pkg.Dispose();
            if (File.Exists(tmp)) File.Delete(tmp);
            if (File.Exists(output)) File.Delete(output);
        }
    }
}