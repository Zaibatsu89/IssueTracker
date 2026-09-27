using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using WorkbookTextGuard.Commands;
using Xunit;

namespace WorkbookTextGuard.Tests;

public class FormulaScopeValidatorTests
{
    private static readonly XNamespace Ns = OoxmlNamespaces.SpreadsheetMl;

    [Theory]
    [InlineData("global")]
    [InlineData("sheet")]
    [InlineData("part")]
    [InlineData("sheet-part")]
    public void WholeGroupAuthorization_CleansMasterWithoutExpansion(string scope)
    {
        using var fixture = new FormulaRegressionFixture(SharedCells());
        var policy = Mapping(scope);
        using var pkg = OoxmlPackage.Open(fixture.Input);
        Assert.Empty(PreflightValidator.Validate(pkg, policy));
        new WorkbookCleaner(pkg, policy).Clean(fixture.Output);
        using var output = OoxmlPackage.Open(fixture.Output);
        var formulas = output.LoadXml(FormulaRegressionFixture.SheetPart).Descendants(Ns + "f").ToList();
        Assert.Equal(2, formulas.Count);
        Assert.Equal("\"safe\"&B1", formulas[0].Value);
        Assert.Equal("", formulas[1].Value);
        Assert.Equal("A1:A2", (string?)formulas[0].Attribute("ref"));
        Assert.All(formulas, f => Assert.Equal("shared", (string?)f.Attribute("t")));
        Assert.All(formulas, f => Assert.Equal("7", (string?)f.Attribute("si")));
        Assert.Empty(output.LoadXml(FormulaRegressionFixture.SheetPart).Descendants(Ns + "v"));
        Assert.Equal(0, CheckCommand.Run(new[] { "--input", fixture.Output, "--policy", fixture.PolicyPath }));
    }

    [Fact]
    public void MasterOnlyScope_ThrowsBeforeDirectOutputOrCliBackup()
    {
        using var fixture = new FormulaRegressionFixture(SharedCells());
        var policy = Mapping("cell");
        fixture.WritePolicy(policy);
        byte[] before = File.ReadAllBytes(fixture.Input);
        using var pkg = OoxmlPackage.Open(fixture.Input);
        Assert.Throws<InvalidDataException>(() => PreflightValidator.Validate(pkg, policy));
        Assert.Throws<InvalidDataException>(() => new WorkbookCleaner(pkg, policy).Clean(fixture.Output));
        Assert.False(File.Exists(fixture.Output));
        Assert.Equal(2, fixture.CheckPreflight());
        Assert.Equal(2, fixture.Clean());
        Assert.False(File.Exists(fixture.Input + ".bak"));
        Assert.False(File.Exists(fixture.Output));
        Assert.Equal(before, File.ReadAllBytes(fixture.Input));
        Assert.Empty(Directory.GetFiles(fixture.DirectoryPath, ".tmp_*"));
    }

    [Fact]
    public void SameReplacementFromDifferentCellMappings_IsNotGroupAuthorization()
    {
        using var fixture = new FormulaRegressionFixture(SharedCells());
        var policy = new Policy
        {
            EmojiMappings = new()
            {
                new() { Sequence = "😀", Replacement = "safe", Scope = new() { Cell = "A1" } },
                new() { Sequence = "😀", Replacement = "safe", Scope = new() { Cell = "A2" } }
            }
        };
        using var pkg = OoxmlPackage.Open(fixture.Input);
        Assert.Throws<InvalidDataException>(() => PreflightValidator.Validate(pkg, policy));
        Assert.Throws<InvalidDataException>(() => new WorkbookCleaner(pkg, policy).Clean(fixture.Output));
    }

    [Fact]
    public void SingletonSharedRef_AllowsCellScope()
    {
        using var fixture = new FormulaRegressionFixture(
            "<row r='1'><c r='A1'><f t='shared' si='7' ref='A1'>\"😀\"</f></c></row>");
        using var pkg = OoxmlPackage.Open(fixture.Input);
        Assert.Empty(PreflightValidator.Validate(pkg, Mapping("cell")));
        new WorkbookCleaner(pkg, Mapping("cell")).Clean(fixture.Output);
        using var output = OoxmlPackage.Open(fixture.Output);
        Assert.Equal("\"safe\"", output.LoadXml(FormulaRegressionFixture.SheetPart).Descendants(Ns + "f").Single().Value);
    }

    [Theory]
    [InlineData("missing-ref")]
    [InlineData("missing-si")]
    [InlineData("invalid-si")]
    [InlineData("overflow-si")]
    [InlineData("missing-address")]
    [InlineData("invalid-address")]
    [InlineData("invalid-ref")]
    [InlineData("reversed-ref")]
    [InlineData("outside-ref")]
    [InlineData("missing-member")]
    [InlineData("wrong-member-si")]
    [InlineData("missing-member-si")]
    [InlineData("outside-member")]
    [InlineData("duplicate-master")]
    [InlineData("follower-ref")]
    [InlineData("duplicate-address")]
    [InlineData("overlap-ref")]
    [InlineData("multiple-formulas")]
    public void InvalidMetadata_WhenAnEditIsRequired_FailsClosed(string defect)
    {
        var document = XDocument.Parse($"<worksheet xmlns='{Ns}'><sheetData>{SharedCells()}</sheetData></worksheet>");
        var cells = document.Descendants(Ns + "c").ToList();
        var master = cells[0].Element(Ns + "f")!;
        var follower = cells[1].Element(Ns + "f")!;
        switch (defect)
        {
            case "missing-ref": master.Attribute("ref")!.Remove(); break;
            case "missing-si": master.Attribute("si")!.Remove(); break;
            case "invalid-si": master.SetAttributeValue("si", "-1"); break;
            case "overflow-si": master.SetAttributeValue("si", "4294967296"); break;
            case "missing-address": cells[0].Attribute("r")!.Remove(); break;
            case "invalid-address": cells[0].SetAttributeValue("r", "XFE1"); break;
            case "invalid-ref": master.SetAttributeValue("ref", "A1:A2 B1"); break;
            case "reversed-ref": master.SetAttributeValue("ref", "A2:A1"); break;
            case "outside-ref": master.SetAttributeValue("ref", "B1:B2"); break;
            case "missing-member": cells[1].Remove(); break;
            case "wrong-member-si": follower.SetAttributeValue("si", "8"); break;
            case "missing-member-si": follower.Attribute("si")!.Remove(); break;
            case "outside-member": cells[1].SetAttributeValue("r", "A3"); break;
            case "duplicate-master": follower.Value = "1+1"; break;
            case "follower-ref": follower.SetAttributeValue("ref", "A1:A2"); break;
            case "duplicate-address": cells[0].AddAfterSelf(new XElement(cells[0])); break;
            case "overlap-ref": cells[1].Parent!.Add(new XElement(Ns + "c", new XAttribute("r", "B2"),
                new XElement(Ns + "f", new XAttribute("t", "array"), new XAttribute("ref", "A1:B2"), "1"))); break;
            case "multiple-formulas": cells[0].Add(new XElement(Ns + "f", "1")); break;
        }
        using var fixture = new FormulaRegressionFixture(string.Concat(document.Descendants(Ns + "row")));
        fixture.WritePolicy(Mapping("global"));
        using var pkg = OoxmlPackage.Open(fixture.Input);
        Assert.Throws<InvalidDataException>(() => PreflightValidator.Validate(pkg, Mapping("global")));
        Assert.Throws<InvalidDataException>(() => new WorkbookCleaner(pkg, Mapping("global")).Clean(fixture.Output));
        Assert.Equal(2, fixture.Clean());
        Assert.False(File.Exists(fixture.Input + ".bak"));
        Assert.False(File.Exists(fixture.Output));
    }

    [Theory]
    [InlineData("😀", false)]
    [InlineData(":-)", true)]
    public void ArrayLiteralEdit_IsRejected(string sequence, bool emoticon)
    {
        using var fixture = new FormulaRegressionFixture(
            $"<row r='1'><c r='A1'><f t='array' ref='A1:A2'>\"{sequence}\"</f></c></row>");
        var mapping = new PolicyMapping { Sequence = sequence, Replacement = "safe" };
        var policy = emoticon ? new Policy { EmoticonMappings = new() { mapping } }
            : new Policy { EmojiMappings = new() { mapping } };
        fixture.WritePolicy(policy);
        using var pkg = OoxmlPackage.Open(fixture.Input);
        Assert.Throws<InvalidDataException>(() => PreflightValidator.Validate(pkg, policy));
        Assert.Throws<InvalidDataException>(() => new WorkbookCleaner(pkg, policy).Clean(fixture.Output));
        Assert.Equal(2, fixture.Clean());
        Assert.False(File.Exists(fixture.Input + ".bak"));
        Assert.False(File.Exists(fixture.Output));
    }

    [Fact]
    public void SharedEmoticonMasterOnlyEdit_IsRejected()
    {
        using var fixture = new FormulaRegressionFixture(SharedCells().Replace("😀", ":-)"));
        var policy = new Policy { EmoticonMappings = new() {
            new() { Sequence = ":-)", Replacement = "safe", Scope = new() { Cell = "A1" } } } };
        using var pkg = OoxmlPackage.Open(fixture.Input);
        Assert.Throws<InvalidDataException>(() => PreflightValidator.Validate(pkg, policy));
    }

    [Theory]
    [InlineData("shared")]
    [InlineData("array")]
    public void UneditedFormulas_RetainTextAndMetadata(string type)
    {
        using var fixture = new FormulaRegressionFixture(SharedCells().Replace("😀", "plain").Replace("shared", type));
        using var pkg = OoxmlPackage.Open(fixture.Input);
        var before = pkg.LoadXml(FormulaRegressionFixture.SheetPart).Descendants(Ns + "f").Select(f => f.ToString()).ToList();
        Assert.Empty(PreflightValidator.Validate(pkg, new Policy()));
        new WorkbookCleaner(pkg, new Policy()).Clean(fixture.Output);
        using var output = OoxmlPackage.Open(fixture.Output);
        Assert.Equal(before, output.LoadXml(FormulaRegressionFixture.SheetPart).Descendants(Ns + "f").Select(f => f.ToString()).ToList());
    }

    [Theory]
    [InlineData("wrong-sheet")]
    [InlineData("wrong-part")]
    [InlineData("wrong-cell")]
    public void NonMatchingScope_DoesNotAuthorizeOrEditMaster(string scope)
    {
        using var fixture = new FormulaRegressionFixture(SharedCells());
        var policy = new Policy { EmojiMappings = new() { new() {
            Sequence = "😀", Replacement = "safe", Scope = scope switch
            {
                "wrong-sheet" => new MappingScope { Sheet = "Other" },
                "wrong-part" => new MappingScope { Part = "xl/sharedStrings.xml" },
                _ => new MappingScope { Cell = "A2" }
            }
        } } };
        fixture.WritePolicy(policy);
        Assert.Equal(1, fixture.CheckPreflight());
        Assert.Equal(1, fixture.Clean());
        Assert.False(File.Exists(fixture.Input + ".bak"));
        Assert.False(File.Exists(fixture.Output));
    }

    [Fact]
    public void RectangularGroup_WithGlobalMapping_IsNotExpanded()
    {
        using var fixture = new FormulaRegressionFixture(
            "<row r='1'><c r='A1'><f t='shared' si='7' ref='A1:B2'>\"😀\"</f></c>"
            + "<c r='B1'><f t='shared' si='7'/></c></row>"
            + "<row r='2'><c r='A2'><f t='shared' si='7'/></c><c r='B2'><f t='shared' si='7'/></c></row>");
        using var pkg = OoxmlPackage.Open(fixture.Input);
        Assert.Empty(PreflightValidator.Validate(pkg, Mapping("global")));
        new WorkbookCleaner(pkg, Mapping("global")).Clean(fixture.Output);
        using var output = OoxmlPackage.Open(fixture.Output);
        var formulas = output.LoadXml(FormulaRegressionFixture.SheetPart).Descendants(Ns + "f").ToList();
        Assert.Equal(4, formulas.Count);
        Assert.Equal("\"safe\"", formulas[0].Value);
        Assert.All(formulas.Skip(1), f => Assert.Empty(f.Value));
    }

    [Fact]
    public void UneditedSharedMetadata_IsNotReconstructed()
    {
        using var fixture = new FormulaRegressionFixture(
            "<row r='1'><c r='A1'><f t='shared'>\"plain\"</f></c></row>");
        using var pkg = OoxmlPackage.Open(fixture.Input);
        Assert.Empty(PreflightValidator.Validate(pkg, new Policy()));
        new WorkbookCleaner(pkg, new Policy()).Clean(fixture.Output);
        using var output = OoxmlPackage.Open(fixture.Output);
        var formula = output.LoadXml(FormulaRegressionFixture.SheetPart).Descendants(Ns + "f").Single();
        Assert.Equal("\"plain\"", formula.Value);
        Assert.Null(formula.Attribute("ref"));
        Assert.Null(formula.Attribute("si"));
    }

    [Fact]
    public void FailedDirectValidation_DoesNotOverwriteExistingOutput()
    {
        using var fixture = new FormulaRegressionFixture(SharedCells());
        File.WriteAllText(fixture.Output, "sentinel");
        using var pkg = OoxmlPackage.Open(fixture.Input);
        Assert.Throws<InvalidDataException>(() => new WorkbookCleaner(pkg, Mapping("cell")).Clean(fixture.Output));
        Assert.Equal("sentinel", File.ReadAllText(fixture.Output));
    }

    private static Policy Mapping(string scope) => new()
    {
        EmojiMappings = new() { new() { Sequence = "😀", Replacement = "safe", Scope = scope switch
        {
            "sheet" => new MappingScope { Sheet = "Sheet1" },
            "part" => new MappingScope { Part = FormulaRegressionFixture.SheetPart },
            "sheet-part" => new MappingScope { Sheet = "Sheet1", Part = FormulaRegressionFixture.SheetPart },
            "cell" => new MappingScope { Cell = "A1" },
            _ => null
        } } }
    };

    private static string SharedCells() => "<row r='1'><c r='A1'><f t='shared' si='7' ref='A1:A2'>\"😀\"&amp;B1</f><v>old</v></c></row>"
        + "<row r='2'><c r='A2'><f t='shared' si='7'/><v>old</v></c></row>";
}

/// <summary>Self-contained synthetic package; no production workbooks or policies.</summary>
internal sealed class FormulaRegressionFixture : IDisposable
{
    internal const string SheetPart = "xl/worksheets/custom.xml";
    public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), "formula-regression-" + Guid.NewGuid().ToString("N"));
    public string Input => Path.Combine(DirectoryPath, "input.xlsx");
    public string Output => Path.Combine(DirectoryPath, "output.xlsx");
    public string PolicyPath => Path.Combine(DirectoryPath, "policy.json");

    public FormulaRegressionFixture(string rows = "", string definedNames = "")
    {
        Directory.CreateDirectory(DirectoryPath);
        using (var zip = ZipFile.Open(Input, ZipArchiveMode.Create))
        {
            Add(zip, "[Content_Types].xml", """
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                <Default Extension="xml" ContentType="application/xml"/>
                <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                <Override PartName="/xl/worksheets/custom.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                </Types>
                """);
            Add(zip, "_rels/.rels", """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                <Relationship Id="r1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
            Add(zip, "xl/_rels/workbook.xml.rels", """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                <Relationship Id="r1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/custom.xml"/>
                </Relationships>
                """);
            Add(zip, "xl/workbook.xml", $"""
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                <sheets><sheet name="Sheet1" sheetId="1" r:id="r1"/></sheets>{definedNames}
                </workbook>
                """);
            Add(zip, SheetPart, $"<worksheet xmlns='{OoxmlNamespaces.SpreadsheetMl}'><sheetData>{rows}</sheetData></worksheet>");
        }
        WritePolicy(new Policy());
    }

    public void WritePolicy(Policy policy) => File.WriteAllText(PolicyPath, JsonSerializer.Serialize(policy));
    public int CheckPreflight() => CheckCommand.Run(new[] { "--input", Input, "--policy", PolicyPath, "--mode", "preflight" });
    public int Clean() => CleanCommand.Run(new[] { "--input", Input, "--policy", PolicyPath, "--output", Output });
    public void Dispose() => Directory.Delete(DirectoryPath, recursive: true);
    private static void Add(ZipArchive zip, string path, string text)
    {
        using var stream = zip.CreateEntry(path).Open();
        stream.Write(Encoding.UTF8.GetBytes(text));
    }
}
