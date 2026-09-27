using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using WorkbookTextGuard;
using WorkbookTextGuard.Commands;
using Xunit;

namespace WorkbookTextGuard.Tests;

public sealed class SstOrphanRegressionTests
{
    private static readonly XNamespace Ns = OoxmlNamespaces.SpreadsheetMl;
    private const string StandardSst = "xl/sharedStrings.xml";
    private const string CustomSst = "xl/text/custom.xml";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EmptyTableOrPlainOrphan_IsUnchanged(bool plainOrphan)
    {
        using var fixture = new Fixture(plainOrphan ? "<si><t>ordinary text</t></si>" : "", "");
        using var pkg = OoxmlPackage.Open(fixture.Input);
        string hash = pkg.PartSha256(StandardSst);
        Assert.Empty(PreflightValidator.Validate(pkg, Policy.Load(fixture.PolicyPath)));
        new WorkbookCleaner(pkg, Policy.Load(fixture.PolicyPath)).Clean(fixture.Output);
        using var output = OoxmlPackage.Open(fixture.Output);
        Assert.Equal(hash, output.PartSha256(StandardSst));
        Assert.Equal(plainOrphan ? 1 : 0, fixture.Entries().Length);
        Assert.Equal(0, fixture.Release());
    }

    [Theory]
    [InlineData(null, StandardSst)]
    [InlineData(StandardSst, StandardSst)]
    [InlineData(CustomSst, CustomSst)]
    public void ExistingOrphan_GlobalOrResolvedPartMappings_AreApplied(string? scopePart, string actualPart)
    {
        using var fixture = new Fixture("<si><t>before ⚠️ :-) after</t></si>", "", actualPart);
        var scope = scopePart is null ? null : new MappingScope { Part = scopePart };
        fixture.SetPolicy(new Policy
        {
            EmojiMappings = { Mapping("⚠️", "warning", scope) },
            EmoticonMappings = { Mapping(":-)", "smile", scope) }
        });
        Assert.Equal(0, fixture.Preflight());
        Assert.Equal(0, fixture.CleanCli());
        Assert.Equal("before warning smile after", Text(Assert.Single(fixture.Entries())));
        Assert.Equal(0, fixture.Release());
    }

    [Theory]
    [InlineData("none")]
    [InlineData("cell")]
    [InlineData("sheet")]
    [InlineData("wrong-part")]
    [InlineData("part-and-cell")]
    public void ExistingOrphan_InsufficientScope_BlocksWithoutWriting(string scopeKind)
    {
        using var fixture = new Fixture("<si><t>⚠️ :-)</t></si>", "", CustomSst);
        MappingScope? scope = scopeKind switch
        {
            "cell" => new MappingScope { Cell = "A1" },
            "sheet" => new MappingScope { Sheet = "Sheet1" },
            "wrong-part" => new MappingScope { Part = StandardSst },
            "part-and-cell" => new MappingScope { Part = CustomSst, Cell = "A1" },
            _ => null
        };
        if (scopeKind != "none")
            fixture.SetPolicy(new Policy
            {
                EmojiMappings = { Mapping("⚠️", "warning", scope) },
                EmoticonMappings = { Mapping(":-)", "smile", scope) }
            });
        byte[] before = File.ReadAllBytes(fixture.Input);
        using var pkg = OoxmlPackage.Open(fixture.Input);
        var violations = PreflightValidator.Validate(pkg, Policy.Load(fixture.PolicyPath));
        Assert.Equal(2, violations.Count);
        Assert.Throws<InvalidDataException>(() =>
            new WorkbookCleaner(pkg, Policy.Load(fixture.PolicyPath)).Clean(fixture.Output));
        Assert.Equal(1, fixture.CleanCli());
        Assert.False(File.Exists(fixture.Output));
        Assert.False(File.Exists(fixture.Input + ".bak"));
        Assert.Equal(before, File.ReadAllBytes(fixture.Input));
    }

    [Theory]
    [InlineData("⚠️ :-)", "warning")]
    [InlineData("⚠️", "✅")]
    public void ExistingOrphan_RemainingOrIntroducedForbiddenContent_BlocksDirectCleaner(string text, string replacement)
    {
        using var fixture = new Fixture($"<si><t>{text}</t></si>", "");
        fixture.SetPolicy(new Policy { EmojiMappings = { Mapping("⚠️", replacement) } });
        using var pkg = OoxmlPackage.Open(fixture.Input);
        Assert.Throws<InvalidDataException>(() =>
            new WorkbookCleaner(pkg, Policy.Load(fixture.PolicyPath)).Clean(fixture.Output));
        Assert.False(File.Exists(fixture.Output));
    }

    [Fact]
    public void ExistingRichTextOrphan_MappingsAcrossRuns_PreserveFormattingAndIndices()
    {
        using var fixture = new Fixture("""
            <si><r><rPr><b/></rPr><t>before ⚠</t></r><r><rPr><i/></rPr><t>️ :-</t></r><r><rPr><u/></rPr><t>) after</t></r></si>
            <si><t>still referenced</t></si>
            """, "<c r=\"A1\" t=\"s\"><v>1</v></c>");
        fixture.SetPolicy(new Policy
        {
            EmojiMappings = { Mapping("⚠️", "warning") },
            EmoticonMappings = { Mapping(":-)", "smile") }
        });
        Assert.Equal(0, fixture.CleanCli());
        var entries = fixture.Entries();
        Assert.Equal(2, entries.Length);
        Assert.Equal("before warning smile after", Text(entries[0]));
        Assert.Equal("still referenced", Text(entries[1]));
        Assert.NotNull(entries[0].Elements(Ns + "r").ElementAt(0).Element(Ns + "rPr")!.Element(Ns + "b"));
        Assert.NotNull(entries[0].Elements(Ns + "r").ElementAt(1).Element(Ns + "rPr")!.Element(Ns + "i"));
        Assert.NotNull(entries[0].Elements(Ns + "r").ElementAt(2).Element(Ns + "rPr")!.Element(Ns + "u"));
        Assert.Equal(new[] { 1 }, fixture.References());
        Assert.Equal(0, fixture.Release());
    }

    [Fact]
    public void Cow_AllCellsMoved_StripsOnlyNewOrphan_WithFormattingPreserved()
    {
        // Distinct sequences, accepted by Policy.Load. Direct cleaner deliberately
        // exercises COW stripping independently of preflight's exact-match rules.
        using var fixture = new Fixture("""
            <si><r><rPr><b/></rPr><t>before ⚠</t></r><r><rPr><i/></rPr><t>️ :-</t></r><r><rPr><u/></rPr><t>) after</t></r></si>
            <si><t>existing plain orphan</t></si>
            """, TwoCells);
        fixture.SetPolicy(new Policy
        {
            EmojiMappings =
            {
                Mapping("⚠️ :-)", "first", new MappingScope { Cell = "A1" }),
                Mapping("before ⚠️ :-) after", "second", new MappingScope { Cell = "B1" })
            }
        });
        using var pkg = OoxmlPackage.Open(fixture.Input);
        var result = new WorkbookCleaner(pkg, Policy.Load(fixture.PolicyPath)).Clean(fixture.Output);
        var entries = fixture.Entries();
        Assert.Equal(4, entries.Length);
        Assert.Equal("before   after", Text(entries[0]));
        Assert.Equal("existing plain orphan", Text(entries[1]));
        Assert.Equal("before first after", Text(entries[2]));
        Assert.Equal("second", Text(entries[3]));
        Assert.Equal(new[] { 2, 3 }, fixture.References());
        Assert.Contains(result.Log, line => line.Contains("[CLEAN-COW-ORPHAN]"));
        Assert.NotNull(entries[0].Elements(Ns + "r").First().Element(Ns + "rPr")!.Element(Ns + "b"));
        Assert.NotNull(entries[0].Elements(Ns + "r").Last().Element(Ns + "rPr")!.Element(Ns + "u"));
        Assert.Equal("4", fixture.Sst().Root!.Attribute("uniqueCount")!.Value);
        Assert.Equal("2", fixture.Sst().Root!.Attribute("count")!.Value);
        Assert.Equal(pkg.PartSha256("xl/workbook.xml"), fixture.PartHash("xl/workbook.xml"));
        Assert.Equal(0, fixture.Release());
    }

    [Fact]
    public void Cow_EmptyOriginalRetainsItsIndex_AndExistingForbiddenOrphanStillBlocks()
    {
        const string richOriginal = "<si><r><rPr><b/></rPr><t>⚠</t></r><r><rPr><i/></rPr><t>️</t></r></si>";
        var policy = new Policy
        {
            EmojiMappings =
            {
                Mapping("⚠️", "first", new MappingScope { Cell = "A1" }),
                Mapping("⚠", "second", new MappingScope { Cell = "B1" }),
                Mapping("️", "", new MappingScope { Cell = "B1" })
            }
        };
        using var fixture = new Fixture(richOriginal, TwoCells);
        fixture.SetPolicy(policy);
        using var pkg = OoxmlPackage.Open(fixture.Input);
        new WorkbookCleaner(pkg, Policy.Load(fixture.PolicyPath)).Clean(fixture.Output);
        Assert.Equal(new[] { 1, 2 }, fixture.References());
        var empty = fixture.Entries()[0];
        Assert.Empty(empty.Elements(Ns + "r"));
        Assert.Equal("", Assert.Single(empty.Elements(Ns + "t")).Value);
        Assert.Equal(0, fixture.Release());

        using var blocked = new Fixture(richOriginal + "<si><t>:-)</t></si>", TwoCells);
        blocked.SetPolicy(policy);
        using var blockedPkg = OoxmlPackage.Open(blocked.Input);
        Assert.Throws<InvalidDataException>(() =>
            new WorkbookCleaner(blockedPkg, Policy.Load(blocked.PolicyPath)).Clean(blocked.Output));
        Assert.False(File.Exists(blocked.Output));
    }

    [Fact]
    public void Cow_RemainingReference_OriginalIsNeverOrphanSanitized()
    {
        using var fixture = new Fixture("<si><t>⚠️ :-)</t></si>", TwoCells);
        fixture.SetPolicy(new Policy
        {
            EmojiMappings = { Mapping("⚠️ :-)", "first", new MappingScope { Cell = "A1" }) },
            EmoticonMappings = { Mapping(":-)", "orphan-only", new MappingScope { Part = StandardSst }) }
        });
        using var pkg = OoxmlPackage.Open(fixture.Input);
        var result = new WorkbookCleaner(pkg, Policy.Load(fixture.PolicyPath)).Clean(fixture.Output);
        Assert.Equal(new[] { 1, 0 }, fixture.References());
        Assert.Equal("⚠️ :-)", Text(fixture.Entries()[0]));
        Assert.Equal("first", Text(fixture.Entries()[1]));
        Assert.DoesNotContain(result.Log, line => line.Contains("[CLEAN-COW-ORPHAN]"));
        Assert.Equal(1, fixture.Release()); // live unmapped text must remain visible
    }

    [Fact]
    public void Cow_NewOrphan_AppliesResolvedPartMappingsBeforeStrippingRest()
    {
        using var fixture = new Fixture("<si><t>⚠️ :-)</t></si>", TwoCells, CustomSst);
        fixture.SetPolicy(new Policy
        {
            EmojiMappings =
            {
                Mapping("⚠️ :-)", "first", new MappingScope { Cell = "A1" }),
                Mapping("⚠️", "second", new MappingScope { Cell = "B1" })
            },
            EmoticonMappings = { Mapping(":-)", "orphan smile", new MappingScope { Part = CustomSst }) }
        });
        using var pkg = OoxmlPackage.Open(fixture.Input);
        new WorkbookCleaner(pkg, Policy.Load(fixture.PolicyPath)).Clean(fixture.Output);
        Assert.Equal(new[] { 1, 2 }, fixture.References());
        Assert.Equal(" orphan smile", Text(fixture.Entries()[0]));
        Assert.Equal("first", Text(fixture.Entries()[1]));
        Assert.Equal("second :-)", Text(fixture.Entries()[2])); // no orphan scope leaks into cell clones
    }

    [Fact]
    public void CliCow_MultiSequenceScopedPolicy_ProducesReleaseCleanOutput()
    {
        using var fixture = new Fixture("<si><t>alpha ⚠️ :-) beta</t></si>", TwoCells);
        fixture.SetPolicy(new Policy
        {
            EmojiMappings =
            {
                Mapping("⚠️", "warning"),
                Mapping("alpha", "first", new MappingScope { Cell = "A1" }),
                Mapping("beta", "second", new MappingScope { Cell = "B1" })
            },
            EmoticonMappings = { Mapping(":-)", "smile") }
        });
        Assert.Equal(0, fixture.Preflight());
        Assert.Equal(0, fixture.CleanCli());
        Assert.Equal(new[] { 1, 2 }, fixture.References());
        Assert.Equal(new[] { "alpha warning smile beta", "first warning smile beta", "alpha warning smile second" },
            fixture.Entries().Select(Text).ToArray());
        Assert.Equal(0, fixture.Release());
        Assert.Equal(File.ReadAllBytes(fixture.Input), File.ReadAllBytes(fixture.Input + ".bak"));
    }

    private const string TwoCells = "<c r=\"A1\" t=\"s\"><v>0</v></c><c r=\"B1\" t=\"s\"><v>0</v></c>";
    private static string Text(XElement si) => new RunOffsetMap(si).LogicalText;
    private static PolicyMapping Mapping(string sequence, string replacement, MappingScope? scope = null)
        => new() { Sequence = sequence, Replacement = replacement, Scope = scope };

    private sealed class Fixture : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "sst-regression-" + Guid.NewGuid().ToString("N"));
        private readonly string _sstPart;
        public string Input => Path.Combine(_directory, "input.xlsx");
        public string Output => Path.Combine(_directory, "output.xlsx");
        public string PolicyPath => Path.Combine(_directory, "policy.json");

        public Fixture(string entries, string cells, string sstPart = StandardSst)
        {
            _sstPart = sstPart;
            Directory.CreateDirectory(_directory);
            SetPolicy(new Policy());
            using var zip = ZipFile.Open(Input, ZipArchiveMode.Create);
            Add(zip, "[Content_Types].xml", $"""
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/{sstPart}" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                </Types>
                """);
            Add(zip, "_rels/.rels", """
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="office" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
            Add(zip, "xl/workbook.xml", $"""
                <workbook xmlns="{Ns}" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets><sheet name="Sheet1" sheetId="1" r:id="sheet"/></sheets>
                </workbook>
                """);
            Add(zip, "xl/_rels/workbook.xml.rels", $"""
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="sheet" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="sst" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="/{sstPart}"/>
                </Relationships>
                """);
            var sheet = XDocument.Parse($"<worksheet xmlns=\"{Ns}\"><sheetData><row r=\"1\">{cells}</row></sheetData></worksheet>");
            Add(zip, "xl/worksheets/sheet1.xml", sheet.ToString());
            var sst = XDocument.Parse($"<sst xmlns=\"{Ns}\">{entries}</sst>");
            var root = sst.Root!;
            root.SetAttributeValue("uniqueCount", root.Elements(Ns + "si").Count());
            root.SetAttributeValue("count", sheet.Descendants(Ns + "c").Count());
            Add(zip, sstPart, sst.ToString());
        }

        public void SetPolicy(Policy policy)
        {
            File.WriteAllText(PolicyPath, JsonSerializer.Serialize(policy));
            _ = Policy.Load(PolicyPath); // all policies, including direct-call cases, are CLI-loadable
        }
        public int Preflight() => CheckCommand.Run(new[] { "--input", Input, "--policy", PolicyPath, "--mode", "preflight" });
        public int Release() => CheckCommand.Run(new[] { "--input", Output, "--policy", PolicyPath, "--mode", "release" });
        public int CleanCli() => CleanCommand.Run(new[] { "--input", Input, "--output", Output, "--policy", PolicyPath });
        public XDocument Sst()
        {
            using var pkg = OoxmlPackage.Open(Output);
            Assert.Equal(_sstPart, pkg.SharedStringPartPath);
            return pkg.LoadXml(_sstPart);
        }
        public XElement[] Entries() => Sst().Root!.Elements(Ns + "si").ToArray();
        public int[] References()
        {
            using var pkg = OoxmlPackage.Open(Output);
            return pkg.LoadXml("xl/worksheets/sheet1.xml").Descendants(Ns + "c")
                .Select(c => int.Parse(c.Element(Ns + "v")!.Value)).ToArray();
        }
        public string PartHash(string part)
        {
            using var pkg = OoxmlPackage.Open(Output);
            return pkg.PartSha256(part);
        }
        private static void Add(ZipArchive zip, string name, string xml)
        {
            using var stream = zip.CreateEntry(name).Open();
            stream.Write(Encoding.UTF8.GetBytes(xml));
        }
        public void Dispose() => Directory.Delete(_directory, recursive: true);
    }
}
