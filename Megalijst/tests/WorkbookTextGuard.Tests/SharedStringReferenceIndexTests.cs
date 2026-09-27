using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace WorkbookTextGuard.Tests;

public class SharedStringReferenceIndexTests
{
    private const string SheetPath = "xl/worksheets/sheet1.xml";

    [Fact]
    public void Build_SharedStringWithoutAddress_ThrowsWithSheetAndIndex()
    {
        using var pkg = BuildPackage();
        var staged = StageWorksheet("<c t=\"s\"><v>7</v></c>");

        var error = Assert.Throws<InvalidDataException>(
            () => SharedStringReferenceIndex.Build(pkg, staged));

        Assert.Contains("Celadres (attribuut 'r') ontbreekt", error.Message);
        Assert.Contains("Gegevens", error.Message);
        Assert.Contains(SheetPath, error.Message);
        Assert.Contains("index 7", error.Message);
    }

    [Fact]
    public void Build_SharedStringsWithAddresses_PreservesAllCellReferences()
    {
        using var pkg = BuildPackage();
        var staged = StageWorksheet(
            "<c r=\"A1\" t=\"s\"><v>7</v></c><c r=\"B1\" t=\"s\"><v>7</v></c>");

        var index = SharedStringReferenceIndex.Build(pkg, staged);

        Assert.Single(index.Index);
        Assert.True(index.IsReferenced(7));
        Assert.Equal(
            new[] { new CellRef(SheetPath, "Gegevens", "A1"), new CellRef(SheetPath, "Gegevens", "B1") },
            index.GetRefs(7));
        Assert.False(index.IsReferenced(8));
        Assert.Empty(index.GetRefs(8));
    }

    private static Dictionary<string, XDocument> StageWorksheet(string cells)
        => new()
        {
            [SheetPath] = XDocument.Parse($"""
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <sheetData><row r="1">{cells}</row></sheetData>
                </worksheet>
                """)
        };

    private static OoxmlPackage BuildPackage()
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(zip, "[Content_Types].xml", """
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                </Types>
                """);
            AddEntry(zip, "_rels/.rels", """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
            AddEntry(zip, "xl/workbook.xml", """
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets><sheet name="Gegevens" sheetId="1" r:id="rId1"/></sheets>
                </workbook>
                """);
            AddEntry(zip, "xl/_rels/workbook.xml.rels", """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                </Relationships>
                """);
            AddEntry(zip, SheetPath, """
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData/></worksheet>
                """);
        }

        byte[] bytes = ms.ToArray();
        return OoxmlPackage.Open(bytes, OoxmlPackage.ComputeSha256(bytes));
    }

    private static void AddEntry(ZipArchive zip, string name, string content)
    {
        using var stream = zip.CreateEntry(name).Open();
        stream.Write(Encoding.UTF8.GetBytes(content));
    }
}
