using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Xunit;
using Phase3Validator;

public class AuditTests
{
    private static string Fixture(bool invalid)
    {
        var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xlsx");
        using var doc = SpreadsheetDocument.Create(file, SpreadsheetDocumentType.Workbook);
        var workbook = doc.AddWorkbookPart();
        workbook.Workbook = new Workbook();
        var worksheet = workbook.AddNewPart<WorksheetPart>();
        worksheet.Worksheet = new Worksheet(new SheetData());
        var sheets = workbook.Workbook.AppendChild(new Sheets());
        var sheet = new Sheet { Name = "Synthetic", SheetId = 1U, Id = workbook.GetIdOfPart(worksheet) };
        if (invalid) sheet.Name = null;
        sheets.Append(sheet);
        workbook.Workbook.Save();
        return file;
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SchemaResultAndBytesAreHonest(bool invalid)
    {
        var file = Fixture(invalid);
        try
        {
            var before = File.ReadAllBytes(file);
            var report = Audit.Inspect(file, FileFormatVersions.Office2019, out var passed);
            Assert.Equal(!invalid, passed);
            Assert.Equal(before, File.ReadAllBytes(file));
            Assert.NotNull(report);
        }
        finally { File.Delete(file); }
    }
    [Fact]
    public void CorruptPackageFailsWithoutBaselineExemptions()
    {
        var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xlsx");
        try { File.WriteAllText(file, "synthetic malformed ZIP"); Audit.Inspect(file, FileFormatVersions.Office2019, out var passed); Assert.False(passed); }
        finally { File.Delete(file); }
    }
    [Fact]
    public void CliRequiresExplicitOfficeAndSeparatePaths()
    {
        Assert.Equal(2, Program.Main(["original", "candidate"]));
        Assert.Equal(2, Program.Main(["Office2019", "same.xlsx", "same.xlsx"]));
    }
}
