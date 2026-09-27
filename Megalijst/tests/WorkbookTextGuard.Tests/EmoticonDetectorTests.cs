using System;
using System.Collections.Generic;
using WorkbookTextGuard;
using Xunit;

namespace WorkbookTextGuard.Tests;

public class EmoticonDetectorTests
{
    private readonly EmoticonDetector _plain  = new(EmoticonDetector.BoundaryProfile.Plain);
    private readonly EmoticonDetector _markup = new(EmoticonDetector.BoundaryProfile.Markup);

    // ── Helper: build a single-entry mapping dictionary ──────────────────────

    private static IReadOnlyDictionary<string, string> Map(string emoticon, string replacement)
        => new Dictionary<string, string> { [emoticon] = replacement };

    private static IReadOnlyDictionary<string, string> EmptyMap()
        => new Dictionary<string, string>();

    // ── Positive matches — plain profile ─────────────────────────────────────

    [Theory]
    [InlineData(":-)",  ":-)")]
    [InlineData(":)",   ":)")]
    [InlineData(";-)",  ";-)")]
    [InlineData(";)",   ";)")]
    [InlineData("XD",   "XD")]
    [InlineData(":-D",  ":-D")]
    public void Plain_StandaloneEmoticon_Detected(string input, string expected)
    {
        var matches = _plain.FindAll(input);
        Assert.Single(matches);
        Assert.Equal(expected, matches[0].Value);
    }

    [Theory]
    [InlineData("Issue opgelost :-), doorgaan",  ":-)")]
    [InlineData("(status: :-))",                 ":-)")]
    [InlineData("Let op!:-)",                    ":-)")]
    [InlineData("Let op! :-)",                   ":-)")]
    [InlineData("status: :-)",                   ":-)")]
    public void Plain_EmoticonInContext_Detected(string input, string expected)
    {
        var matches = _plain.FindAll(input);
        Assert.Single(matches);
        Assert.Equal(expected, matches[0].Value);
    }

    // ── Quoted emoticons — all six quote pairs + U+201E/U+201F ───────────────

    [Theory]
    [InlineData("\":-)\"")] // straight double
    [InlineData("'XD'")]    // straight single
    [InlineData("\u201C:-)\u201D")] // " "
    [InlineData("\u2018XD\u2019")]  // ' '
    [InlineData("\u201E:-)\u201F")] // „ ‟
    [InlineData("\u201EXD\u201F")]  // „XD‟
    public void Plain_QuotedEmoticon_Detected(string input)
    {
        var matches = _plain.FindAll(input);
        Assert.Single(matches);
    }

    // ── ReplaceWithMappings: quotes preserved ─────────────────────────────────

    [Fact]
    public void ReplaceWithMappings_StraightDoubleQuotes_Preserved()
    {
        string result = _plain.ReplaceWithMappings("\":-)\""  , Map(":-)", "OK"));
        Assert.Equal("\"OK\"", result);
    }

    [Fact]
    public void ReplaceWithMappings_U201E_U201F_Preserved()
    {
        string result = _plain.ReplaceWithMappings("\u201E:-)\u201F", Map(":-)", "OK"));
        Assert.Equal("\u201EOK\u201F", result);
    }

    [Fact]
    public void ReplaceWithMappings_CurlyDoubleQuotes_Preserved()
    {
        string result = _plain.ReplaceWithMappings("\u201C:-)\u201D", Map(":-)", "OK"));
        Assert.Equal("\u201COK\u201D", result);
    }

    // ── ReplaceWithMappings: only mapped matches replaced ────────────────────

    [Fact]
    public void ReplaceWithMappings_OnlyMappedReplaced_UnmappedPreserved()
    {
        // :-) is mapped, XD is not
        string text   = "Status :-) en XD";
        string result = _plain.ReplaceWithMappings(text, Map(":-)", ""));
        Assert.Equal("Status  en XD", result);
    }

    [Fact]
    public void ReplaceWithMappings_EmptyMap_NothingReplaced()
    {
        string text   = "Status :-)";
        string result = _plain.ReplaceWithMappings(text, EmptyMap());
        Assert.Equal(text, result);
    }

    [Fact]
    public void ReplaceWithMappings_EmptyReplacement_EmoticonRemoved()
    {
        string result = _plain.ReplaceWithMappings("Status :-)", Map(":-)", ""));
        Assert.Equal("Status ", result);
    }

    [Fact]
    public void ReplaceWithMappings_MultipleMappings_AllReplaced()
    {
        var mappings = new Dictionary<string, string>
        {
            [":-)"] = "blij",
            ["XD"]  = "lol"
        };
        string result = _plain.ReplaceWithMappings("Status :-) en XD", mappings);
        Assert.Equal("Status blij en lol", result);
    }

    // ── Dollar signs and backreferences: treated as literal text ─────────────

    [Fact]
    public void ReplaceWithMappings_DollarSign_LiteralNotBackreference()
    {
        // Replacement "$1" must appear literally, not as a regex backreference
        string result = _plain.ReplaceWithMappings("Status :-)", Map(":-)", "$1"));
        Assert.Equal("Status $1", result);
    }

    [Fact]
    public void ReplaceWithMappings_DollarAmpersand_LiteralNotBackreference()
    {
        string result = _plain.ReplaceWithMappings("Status :-)", Map(":-)", "$&"));
        Assert.Equal("Status $&", result);
    }

    [Fact]
    public void ReplaceWithMappings_CurrencySymbol_LiteralText()
    {
        // € symbol in replacement must appear literally
        string result = _plain.ReplaceWithMappings("Prijs :-)", Map(":-)", "€5,00"));
        Assert.Equal("Prijs €5,00", result);
    }

    // ── No cascade: replacement text not re-scanned ───────────────────────────

    [Fact]
    public void ReplaceWithMappings_ReplacementContainsEmoticon_NoCascade()
    {
        // Replace :-) with ":)" — the replacement itself is not re-scanned
        string result = _plain.ReplaceWithMappings("Status :-)", Map(":-)", ":)"));
        Assert.Equal("Status :)", result);
    }

    // ── Trailing parenthesis preserved ────────────────────────────────────────

    [Fact]
    public void ReplaceWithMappings_TrailingParenthesisPreserved()
    {
        // ":-))": emoticon is ":-)", second ")" is context
        string result = _plain.ReplaceWithMappings(":-))", Map(":-)", ""));
        Assert.Equal(")", result);
    }

    [Fact]
    public void ReplaceWithMappings_ParenthesisContext_Preserved()
    {
        string result = _plain.ReplaceWithMappings("(status: :-))", Map(":-)", ""));
        Assert.Equal("(status: )", result);
    }

    // ── Negative matches — plain profile ─────────────────────────────────────

    [Theory]
    [InlineData("::-)")] // colon as left boundary → no match
    [InlineData(";;-)")] // semicolon as left boundary → no match
    [InlineData(":XD")]  // colon left boundary → no match
    [InlineData(";:-D")] // semicolon left boundary → no match
    public void Plain_InvalidLeftBoundary_NotDetected(string input)
    {
        Assert.Empty(_plain.FindAll(input));
    }

    [Fact]
    public void Plain_XD_InsideWord_NotDetected()
    {
        Assert.Empty(_plain.FindAll("LMAXD"));
        Assert.Empty(_plain.FindAll("XDfoo"));
        Assert.Empty(_plain.FindAll("prefixXDsuffix"));
    }

    [Fact]
    public void Plain_ColonD_InsideCode_NotDetected()
    {
        Assert.Empty(_plain.FindAll("C:D"));
    }

    // ── Markup profile ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("*:-)*")]
    [InlineData("_XD_")]
    [InlineData("**:-D**")]
    [InlineData("~XD~")]
    [InlineData("`XD`")]
    public void Markup_MarkdownDelimiters_Detected(string input)
    {
        Assert.Single(_markup.FindAll(input));
    }

    [Fact]
    public void Markup_DelimitersPreservedAfterReplace()
    {
        string result = _markup.ReplaceWithMappings("*:-)*", Map(":-)", ""));
        Assert.Equal("**", result);
    }

    [Fact]
    public void Markup_UnderscoreTechnicalText_Detected()
    {
        var matches = _markup.FindAll("prefix_XD_suffix");
        Assert.Single(matches);
        Assert.Equal("XD", matches[0].Value);
    }

    [Fact]
    public void Plain_MarkdownDelimiters_NotDetected()
    {
        Assert.Empty(_plain.FindAll("*:-)*"));
    }

    // ── Profile consistency ───────────────────────────────────────────────────

    [Fact]
    public void Plain_CheckAndClean_SameProfile()
    {
        var d1 = new EmoticonDetector(EmoticonDetector.BoundaryProfile.Plain);
        var d2 = new EmoticonDetector(EmoticonDetector.BoundaryProfile.Plain);
        Assert.Equal(d1.FindAll("Status :-)").Count, d2.FindAll("Status :-)").Count);
    }

    // ── Idempotence ───────────────────────────────────────────────────────────

    [Fact]
    public void ReplaceWithMappings_Idempotent()
    {
        string input = "Goed gedaan :-)";
        string pass1 = _plain.ReplaceWithMappings(input, Map(":-)", ""));
        string pass2 = _plain.ReplaceWithMappings(pass1, Map(":-)", ""));
        Assert.Equal(pass1, pass2);
    }

    // ── All six quote pairs at both boundaries ────────────────────────────────

    [Theory]
    [InlineData('"',  '"')]
    [InlineData('\'', '\'')]
    [InlineData('\u201C', '\u201D')]
    [InlineData('\u2018', '\u2019')]
    [InlineData('\u201E', '\u201F')]
    public void Plain_AllQuotePairs_BothBoundaries(char open, char close)
    {
        string input = $"{open}:-){close}";
        var matches = _plain.FindAll(input);
        Assert.Single(matches);
        Assert.Equal(":-)", matches[0].Value);

        string replaced = _plain.ReplaceWithMappings(input, Map(":-)", "OK"));
        Assert.Equal($"{open}OK{close}", replaced);
    }

    // ── Empty / null-safe ─────────────────────────────────────────────────────

    [Fact]
    public void FindAll_EmptyString_ReturnsEmpty()
        => Assert.Empty(_plain.FindAll(""));

    [Fact]
    public void ReplaceWithMappings_EmptyString_ReturnsEmpty()
        => Assert.Equal("", _plain.ReplaceWithMappings("", Map(":-)", "X")));
}