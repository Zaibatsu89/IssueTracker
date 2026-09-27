using System.Collections.Generic;
using System.Xml.Linq;
using WorkbookTextGuard;
using Xunit;

namespace WorkbookTextGuard.Tests;

public class TextExtractorTests
{
    private static readonly XNamespace Ns = OoxmlNamespaces.SpreadsheetMl;

    // ── GetLogicalText ────────────────────────────────────────────────────────

    [Fact]
    public void GetLogicalText_PlainT_ReturnsValue()
    {
        var si = new XElement(Ns + "si", new XElement(Ns + "t", "Hallo wereld"));
        Assert.Equal("Hallo wereld", TextExtractor.GetLogicalText(si));
    }

    [Fact]
    public void GetLogicalText_RichTextRuns_ConcatenatesAll()
    {
        var si = new XElement(Ns + "si",
            new XElement(Ns + "r", new XElement(Ns + "t", "Hallo ")),
            new XElement(Ns + "r", new XElement(Ns + "t", "wereld")));
        Assert.Equal("Hallo wereld", TextExtractor.GetLogicalText(si));
    }

    [Fact]
    public void GetLogicalText_EmojiSplitAcrossRuns_JoinedCorrectly()
    {
        // ⚠️ split: U+26A0 in run 1, U+FE0F in run 2
        var si = new XElement(Ns + "si",
            new XElement(Ns + "r", new XElement(Ns + "t", "\u26A0")),
            new XElement(Ns + "r", new XElement(Ns + "t", "\uFE0F")));
        string logical = TextExtractor.GetLogicalText(si);
        Assert.Equal("\u26A0\uFE0F", logical);

        var matches = EmojiDetector.FindAll(logical);
        Assert.Single(matches);
    }

    [Fact]
    public void GetLogicalText_EmptyRuns_ReturnsEmpty()
    {
        var si = new XElement(Ns + "si",
            new XElement(Ns + "r", new XElement(Ns + "t", "")));
        Assert.Equal("", TextExtractor.GetLogicalText(si));
    }

    // ── ExtractFormulaStringLiterals (legacy helper) ──────────────────────────

    [Fact]
    public void ExtractFormulaStringLiterals_SingleLiteral()
    {
        var lits = TextExtractor.ExtractFormulaStringLiterals("IF(A1=1,\"✅\",\"❌\")");
        Assert.Equal(2, lits.Count);
        Assert.Equal("✅", lits[0]);
        Assert.Equal("❌", lits[1]);
    }

    [Fact]
    public void ExtractFormulaStringLiterals_DoubledQuotes()
    {
        var lits = TextExtractor.ExtractFormulaStringLiterals("\"He said \"\"hello\"\"\"");
        Assert.Single(lits);
        Assert.Equal("He said \"hello\"", lits[0]);
    }

    [Fact]
    public void ExtractFormulaStringLiterals_NoLiterals_ReturnsEmpty()
    {
        Assert.Empty(TextExtractor.ExtractFormulaStringLiterals("SUM(A1:A10)"));
    }

    [Fact]
    public void ExtractFormulaStringLiterals_EmptyLiteral()
    {
        var lits = TextExtractor.ExtractFormulaStringLiterals("IF(A1,\"\",\"x\")");
        Assert.Equal(2, lits.Count);
        Assert.Equal("", lits[0]);
        Assert.Equal("x", lits[1]);
    }

    [Fact]
    public void ExtractFormulaStringLiterals_ColumnRange_NotExtracted()
    {
        Assert.Empty(TextExtractor.ExtractFormulaStringLiterals("SUM(C:D)"));
    }

    [Fact]
    public void ExtractFormulaStringLiterals_EmptyFormula_ReturnsEmpty()
    {
        Assert.Empty(TextExtractor.ExtractFormulaStringLiterals(""));
    }

    // ── FormulaTokenizer integration ──────────────────────────────────────────

    [Fact]
    public void FormulaTokenizer_SingleQuotedIdentifier_NotExtractedAsLiteral()
    {
        // 'Sheet1'!A1 — identifier, not a string literal
        var tokens = FormulaTokenizer.ExtractLiterals("'Sheet1'!A1");
        Assert.Empty(tokens);
    }

    [Fact]
    public void FormulaTokenizer_IdentifierWithDoubleQuote_NotExtracted()
    {
        // Double quote inside single-quoted identifier must not be extracted
        var tokens = FormulaTokenizer.ExtractLiterals("'Sheet \"quoted\"'!A1");
        Assert.Empty(tokens);
    }

    [Fact]
    public void FormulaTokenizer_IdentifierAndLiteralCombined_OnlyLiteralExtracted()
    {
        var tokens = FormulaTokenizer.ExtractLiterals("'Sheet1'!A1&\"suffix\"");
        Assert.Single(tokens);
        Assert.Equal("suffix", tokens[0].DecodedText);
    }

    [Fact]
    public void FormulaTokenizer_EscapedApostropheInIdentifier_Skipped()
    {
        var tokens = FormulaTokenizer.ExtractLiterals("'O''Brien'!A1");
        Assert.Empty(tokens);
    }

    [Fact]
    public void FormulaTokenizer_ReferencesPreservedAfterRebuild()
    {
        string formula = "VLOOKUP(A1,B:C,2,\"exact\")";
        var tokens = FormulaTokenizer.ExtractLiterals(formula);
        string result = FormulaTokenizer.RebuildWithEdits(formula, tokens,
            new Dictionary<int, string> { [0] = "EXACT" });
        // B:C column range must be preserved exactly
        Assert.Contains("B:C", result);
        Assert.Equal("VLOOKUP(A1,B:C,2,\"EXACT\")", result);
    }

    [Fact]
    public void FormulaTokenizer_MultipleLiterals_AllExtracted()
    {
        var tokens = FormulaTokenizer.ExtractLiterals("IF(A1=\"✅\",\"Gereed\",\"Fout\")");
        Assert.Equal(3, tokens.Count);
        Assert.Equal("✅", tokens[0].DecodedText);
        Assert.Equal("Gereed", tokens[1].DecodedText);
        Assert.Equal("Fout", tokens[2].DecodedText);
    }
}