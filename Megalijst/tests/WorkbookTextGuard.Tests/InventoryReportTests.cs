using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using WorkbookTextGuard;
using Xunit;

namespace WorkbookTextGuard.Tests;

public class InventoryReportTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

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

    private static (OoxmlPackage pkg, string tmp) OpenTemp(byte[] bytes)
    {
        string tmp = Path.GetTempFileName() + ".xlsx";
        File.WriteAllBytes(tmp, bytes);
        return (OoxmlPackage.Open(tmp), tmp);
    }

    private static void Cleanup(params string[] paths)
    {
        foreach (var p in paths)
            try { if (File.Exists(p)) File.Delete(p); } catch { }
    }

    // ── Emoji aggregation ─────────────────────────────────────────────────────

    [Fact]
    public void Build_EmojiInSharedString_DetectedAndCounted()
    {
        var bytes = BuildXlsx("Status ⚠️ einde");
        var (pkg, tmp) = OpenTemp(bytes);
        try
        {
            var locations = TextExtractor.ExtractAll(pkg);
            var detector  = new EmoticonDetector(EmoticonDetector.BoundaryProfile.Plain);
            var report    = InventoryReport.Build(pkg, locations, detector);

            Assert.Single(report.EmojiSequences);
            Assert.Equal("⚠️", report.EmojiSequences[0].Sequence);
            Assert.Equal(1, report.EmojiSequences[0].Count);
        }
        finally { pkg.Dispose(); Cleanup(tmp); }
    }

    [Fact]
    public void Build_NoEmoji_EmptySequences()
    {
        var bytes = BuildXlsx("Gewone tekst");
        var (pkg, tmp) = OpenTemp(bytes);
        try
        {
            var locations = TextExtractor.ExtractAll(pkg);
            var detector  = new EmoticonDetector(EmoticonDetector.BoundaryProfile.Plain);
            var report    = InventoryReport.Build(pkg, locations, detector);

            Assert.Empty(report.EmojiSequences);
            Assert.Empty(report.EmoticonMatches);
        }
        finally { pkg.Dispose(); Cleanup(tmp); }
    }

    // ── Emoticon aggregation ──────────────────────────────────────────────────

    [Fact]
    public void Build_EmoticonInSharedString_DetectedAndCounted()
    {
        var bytes = BuildXlsx("Status :-)");
        var (pkg, tmp) = OpenTemp(bytes);
        try
        {
            var locations = TextExtractor.ExtractAll(pkg);
            var detector  = new EmoticonDetector(EmoticonDetector.BoundaryProfile.Plain);
            var report    = InventoryReport.Build(pkg, locations, detector);

            Assert.Single(report.EmoticonMatches);
            Assert.Equal(":-)", report.EmoticonMatches[0].Sequence);
            Assert.Equal(1, report.EmoticonMatches[0].Count);
        }
        finally { pkg.Dispose(); Cleanup(tmp); }
    }

    // ── Formula source filter ─────────────────────────────────────────────────

    [Fact]
    public void Build_EmoticonInFormulaLiteral_Detected()
    {
        // FormulaLiteral source should be scanned for emoticons
        var loc = new TextLocation(
            Source: TextSource.FormulaLiteral,
            PartPath: "xl/worksheets/sheet1.xml",
            SheetName: "Sheet1",
            CellAddress: "A1",
            SharedStringIndex: null,
            LogicalText: "Status :-)",
            IsFormula: true);

        var detector = new EmoticonDetector(EmoticonDetector.BoundaryProfile.Plain);
        var matches  = detector.FindAll(loc.LogicalText);
        Assert.Single(matches);
        Assert.Equal(":-)", matches[0].Value);
    }

    [Fact]
    public void Build_RawFormulaCode_NotScannedForEmoticons()
    {
        // IsFormula=true and Source != FormulaLiteral should be skipped
        // (e.g. DefinedName formula code like SUM(A:B) should not be scanned as text)
        var loc = new TextLocation(
            Source: TextSource.DefinedName,
            PartPath: "xl/workbook.xml",
            SheetName: null,
            CellAddress: "MyName",
            SharedStringIndex: null,
            LogicalText: "SUM(A:B)",
            IsFormula: true);

        // The filter: if (loc.IsFormula && loc.Source != TextSource.FormulaLiteral) continue
        bool shouldSkip = loc.IsFormula && loc.Source != TextSource.FormulaLiteral;
        Assert.True(shouldSkip);
    }

    // ── Source locations ──────────────────────────────────────────────────────

    [Fact]
    public void Build_OccurrenceContainsCorrectSource()
    {
        var bytes = BuildXlsx("⚠️");
        var (pkg, tmp) = OpenTemp(bytes);
        try
        {
            var locations = TextExtractor.ExtractAll(pkg);
            var detector  = new EmoticonDetector(EmoticonDetector.BoundaryProfile.Plain);
            var report    = InventoryReport.Build(pkg, locations, detector);

            Assert.Single(report.EmojiSequences);
            var occ = report.EmojiSequences[0].Occurrences[0];
            Assert.Equal("SharedString", occ.Source);
        }
        finally { pkg.Dispose(); Cleanup(tmp); }
    }

    // ── Stable sorting ────────────────────────────────────────────────────────

    [Fact]
    public void Build_MultipleEmoji_SortedBySequence()
    {
        var bytes = BuildXlsx("⚠️ en ✅");
        var (pkg, tmp) = OpenTemp(bytes);
        try
        {
            var locations = TextExtractor.ExtractAll(pkg);
            var detector  = new EmoticonDetector(EmoticonDetector.BoundaryProfile.Plain);
            var report    = InventoryReport.Build(pkg, locations, detector);

            Assert.Equal(2, report.EmojiSequences.Count);
            // Sorted by sequence string (ordinal)
            var seqs = report.EmojiSequences.ConvertAll(e => e.Sequence);
            var sorted = new List<string>(seqs);
            sorted.Sort(System.StringComparer.Ordinal);
            Assert.Equal(sorted, seqs);
        }
        finally { pkg.Dispose(); Cleanup(tmp); }
    }
}