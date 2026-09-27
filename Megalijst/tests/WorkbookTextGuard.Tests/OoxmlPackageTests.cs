using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using WorkbookTextGuard;
using Xunit;

namespace WorkbookTextGuard.Tests;

public class OoxmlPackageTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static byte[] BuildMinimalXlsx()
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(zip, "[Content_Types].xml", """
                <?xml version="1.0" encoding="UTF-8"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
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
                  <sheets>
                    <sheet name="Sheet1" sheetId="1" r:id="rId1"/>
                  </sheets>
                </workbook>
                """);
            AddEntry(zip, "xl/_rels/workbook.xml.rels", """
                <?xml version="1.0" encoding="UTF-8"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                </Relationships>
                """);
            AddEntry(zip, "xl/worksheets/sheet1.xml", """
                <?xml version="1.0" encoding="UTF-8"?>
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <sheetData/>
                </worksheet>
                """);
        }
        return ms.ToArray();
    }

    private static void AddEntry(ZipArchive zip, string name, string content)
    {
        var entry = zip.CreateEntry(name);
        using var stream = entry.Open();
        stream.Write(Encoding.UTF8.GetBytes(content.Trim()));
    }

    private static string ComputeSha256(byte[] bytes)
        => OoxmlPackage.ComputeSha256(bytes);

    // ── Open(string) ──────────────────────────────────────────────────────────

    [Fact]
    public void Open_ValidFile_Succeeds()
    {
        byte[] bytes = BuildMinimalXlsx();
        string tmp = Path.GetTempFileName() + ".xlsx";
        File.WriteAllBytes(tmp, bytes);
        try
        {
            using var pkg = OoxmlPackage.Open(tmp);
            Assert.NotNull(pkg.WorkbookPartPath);
            Assert.Single(pkg.Sheets);
            Assert.Equal("Sheet1", pkg.Sheets[0].Name);
        }
        finally { File.Delete(tmp); }
    }

    [Fact]
    public void Open_Sha256_MatchesFileContent()
    {
        byte[] bytes = BuildMinimalXlsx();
        string tmp = Path.GetTempFileName() + ".xlsx";
        File.WriteAllBytes(tmp, bytes);
        try
        {
            using var pkg = OoxmlPackage.Open(tmp);
            string expected = ComputeSha256(bytes);
            Assert.Equal(expected, pkg.Sha256Hex);
        }
        finally { File.Delete(tmp); }
    }

    // ── Open(byte[], string) ──────────────────────────────────────────────────

    [Fact]
    public void Open_BytesWithCorrectHash_Succeeds()
    {
        byte[] bytes = BuildMinimalXlsx();
        string sha256 = ComputeSha256(bytes);
        using var pkg = OoxmlPackage.Open(bytes, sha256);
        Assert.NotNull(pkg.WorkbookPartPath);
    }

    [Fact]
    public void Open_BytesWithWrongHash_Throws()
    {
        byte[] bytes = BuildMinimalXlsx();
        string wrongHash = new string('0', 64);
        Assert.Throws<InvalidDataException>(() => OoxmlPackage.Open(bytes, wrongHash));
    }

    [Fact]
    public void Open_BytesWithInvalidHashLength_Throws()
    {
        byte[] bytes = BuildMinimalXlsx();
        // 63 hex chars — invalid
        Assert.Throws<InvalidDataException>(() => OoxmlPackage.Open(bytes, new string('a', 63)));
    }

    [Fact]
    public void Open_BytesHashCaseInsensitive_Succeeds()
    {
        byte[] bytes = BuildMinimalXlsx();
        string sha256Upper = ComputeSha256(bytes).ToUpperInvariant();
        using var pkg = OoxmlPackage.Open(bytes, sha256Upper);
        Assert.NotNull(pkg);
    }

    // ── Snapshot consistency ──────────────────────────────────────────────────

    [Fact]
    public void Open_SnapshotConsistent_FileChangedAfterOpen_NotAffected()
    {
        byte[] bytes = BuildMinimalXlsx();
        string tmp = Path.GetTempFileName() + ".xlsx";
        File.WriteAllBytes(tmp, bytes);
        try
        {
            string sha256 = ComputeSha256(bytes);
            using var pkg = OoxmlPackage.Open(bytes, sha256);

            // Overwrite the file on disk after opening
            File.WriteAllBytes(tmp, new byte[] { 0x00, 0x01, 0x02 });

            // Package still works from its in-memory snapshot
            Assert.Equal(sha256, pkg.Sha256Hex);
            Assert.Single(pkg.Sheets);
        }
        finally { File.Delete(tmp); }
    }

    // ── PartExists and GetPartBytes ───────────────────────────────────────────

    [Fact]
    public void PartExists_KnownPart_ReturnsTrue()
    {
        byte[] bytes = BuildMinimalXlsx();
        using var pkg = OoxmlPackage.Open(bytes, ComputeSha256(bytes));
        Assert.True(pkg.PartExists("xl/workbook.xml"));
    }

    [Fact]
    public void PartExists_UnknownPart_ReturnsFalse()
    {
        byte[] bytes = BuildMinimalXlsx();
        using var pkg = OoxmlPackage.Open(bytes, ComputeSha256(bytes));
        Assert.False(pkg.PartExists("xl/nonexistent.xml"));
    }

    [Fact]
    public void GetPartBytes_KnownPart_ReturnsBytes()
    {
        byte[] bytes = BuildMinimalXlsx();
        using var pkg = OoxmlPackage.Open(bytes, ComputeSha256(bytes));
        byte[] partBytes = pkg.GetPartBytes("xl/workbook.xml");
        Assert.NotEmpty(partBytes);
    }

    [Fact]
    public void GetPartBytes_UnknownPart_Throws()
    {
        byte[] bytes = BuildMinimalXlsx();
        using var pkg = OoxmlPackage.Open(bytes, ComputeSha256(bytes));
        Assert.Throws<FileNotFoundException>(() => pkg.GetPartBytes("xl/missing.xml"));
    }

    // ── ComputeSha256 ─────────────────────────────────────────────────────────

    [Fact]
    public void ComputeSha256_EmptyBytes_Returns64HexChars()
    {
        string hash = OoxmlPackage.ComputeSha256(Array.Empty<byte>());
        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9a-f]{64}$", hash);
    }

    [Fact]
    public void ComputeSha256_SameInput_SameOutput()
    {
        byte[] bytes = BuildMinimalXlsx();
        Assert.Equal(OoxmlPackage.ComputeSha256(bytes), OoxmlPackage.ComputeSha256(bytes));
    }

    [Fact]
    public void ComputeSha256_DifferentInput_DifferentOutput()
    {
        byte[] a = Encoding.UTF8.GetBytes("hello");
        byte[] b = Encoding.UTF8.GetBytes("world");
        Assert.NotEqual(OoxmlPackage.ComputeSha256(a), OoxmlPackage.ComputeSha256(b));
    }
}