using System.IO;
using System.Linq;
using System.Xml.Linq;
using WorkbookTextGuard.Commands;
using Xunit;

namespace WorkbookTextGuard.Tests;

public class DefinedNameRegressionTests
{
    [Theory]
    [InlineData("😀", false)]
    [InlineData(":-)", true)]
    public void DefinedNameLiteral_IsDetectedButNeverTransformable(string sequence, bool emoticon)
    {
        using var fixture = new FormulaRegressionFixture(definedNames:
            $"<definedNames><definedName name='Status'>\"{sequence}\"</definedName></definedNames>");
        var mapping = new PolicyMapping { Sequence = sequence, Replacement = "safe" };
        var policy = emoticon ? new Policy { EmoticonMappings = new() { mapping } }
            : new Policy { EmojiMappings = new() { mapping } };
        fixture.WritePolicy(policy);
        byte[] before = File.ReadAllBytes(fixture.Input);
        using var pkg = OoxmlPackage.Open(fixture.Input);
        var locations = TextExtractor.ExtractAll(pkg);
        var literal = Assert.Single(locations.Where(l => l.Source == TextSource.DefinedName));
        Assert.True(literal.IsFormula);
        Assert.Equal(sequence, literal.LogicalText);
        Assert.Equal("Status", literal.CellAddress);
        Assert.DoesNotContain(locations, l => l.Source == TextSource.FormulaLiteral);

        var violation = Assert.Single(PreflightValidator.Validate(pkg, policy));
        Assert.Equal(ViolationType.NonTransformableSource, violation.Type);
        Assert.Equal(TextSource.DefinedName, violation.Location.Source);
        Assert.Contains("Manual correction required", violation.Message);
        var locationViolation = Assert.Single(PreflightValidator.Validate(locations, policy, new EmoticonDetector()));
        Assert.Equal(ViolationType.NonTransformableSource, locationViolation.Type);
        Assert.Single(PreflightValidator.Validate(pkg, new Policy()));

        var report = InventoryReport.Build(pkg, locations, new EmoticonDetector());
        var inventory = Assert.Single(emoticon ? report.EmoticonMatches : report.EmojiSequences);
        Assert.Equal(sequence, inventory.Sequence);
        Assert.Equal("DefinedName", Assert.Single(inventory.Occurrences).Source);
        Assert.Equal(1, fixture.CheckPreflight());
        Assert.Equal(1, CheckCommand.Run(new[] { "--input", fixture.Input, "--policy", fixture.PolicyPath }));
        Assert.Equal(1, fixture.Clean());
        Assert.False(File.Exists(fixture.Input + ".bak"));
        Assert.False(File.Exists(fixture.Output));
        var error = Assert.Throws<InvalidDataException>(() => new WorkbookCleaner(pkg, policy).Clean(fixture.Output));
        Assert.Contains("Manual correction required", error.Message);
        Assert.False(File.Exists(fixture.Output));
        Assert.Equal(before, File.ReadAllBytes(fixture.Input));
        Assert.Empty(Directory.GetFiles(fixture.DirectoryPath, ".tmp_*"));
    }

    [Fact]
    public void DefinedNames_OnlyDecodedLiteralsAreReported()
    {
        using var fixture = new FormulaRegressionFixture(definedNames:
            "<definedNames><definedName name='Status'>IF(A1&gt;0,\"say \"\"hello\"\" :-)\",\"😀\")</definedName>"
            + "<definedName name='RangeOnly'>Sheet1!$A$1:$A$2</definedName></definedNames>");
        using var pkg = OoxmlPackage.Open(fixture.Input);
        var literals = TextExtractor.ExtractAll(pkg).Where(l => l.Source == TextSource.DefinedName).ToList();
        Assert.Equal(new[] { "say \"hello\" :-)", "😀" }, literals.Select(l => l.LogicalText));
        Assert.All(literals, l => Assert.Equal("Status", l.CellAddress));
        var inventory = InventoryReport.Build(pkg, TextExtractor.ExtractAll(pkg), new EmoticonDetector());
        Assert.Single(inventory.EmojiSequences);
        Assert.Single(inventory.EmoticonMatches);
        Assert.Equal(2, PreflightValidator.Validate(pkg, new Policy()).Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CleanDefinedNames_AreNotChangedEvenWhenWorkbookIsReserialized(bool hasFormula)
    {
        const string names = "<definedNames><definedName name='Message'>\"plain\"</definedName>"
            + "<definedName name='RangeOnly'>Sheet1!$A$1:$A$2</definedName></definedNames>";
        string rows = hasFormula ? "<row r='1'><c r='A1'><f>1+1</f><v>2</v></c></row>" : "";
        using var fixture = new FormulaRegressionFixture(rows, names);
        using var pkg = OoxmlPackage.Open(fixture.Input);
        Assert.Empty(PreflightValidator.Validate(pkg, new Policy()));
        byte[] beforeWorkbook = pkg.GetPartBytes(pkg.WorkbookPartPath!);
        new WorkbookCleaner(pkg, new Policy()).Clean(fixture.Output);
        using var output = OoxmlPackage.Open(fixture.Output);
        XNamespace ns = OoxmlNamespaces.SpreadsheetMl;
        var before = pkg.LoadXml(pkg.WorkbookPartPath!).Descendants(ns + "definedNames").Single();
        var after = output.LoadXml(output.WorkbookPartPath!).Descendants(ns + "definedNames").Single();
        Assert.True(XNode.DeepEquals(before, after));
        if (!hasFormula) Assert.Equal(beforeWorkbook, output.GetPartBytes(output.WorkbookPartPath!));
        Assert.Equal(0, CheckCommand.Run(new[] { "--input", fixture.Output, "--policy", fixture.PolicyPath }));
    }
}
