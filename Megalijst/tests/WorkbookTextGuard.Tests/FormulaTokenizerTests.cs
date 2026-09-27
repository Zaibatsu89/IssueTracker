using System;
using System.Collections.Generic;
using WorkbookTextGuard;
using Xunit;

namespace WorkbookTextGuard.Tests;

public class FormulaTokenizerTests
{
    // ── Basic literal extraction ──────────────────────────────────────────────

    [Fact]
    public void ExtractLiterals_SingleLiteral_Extracted()
    {
        var tokens = FormulaTokenizer.ExtractLiterals("IF(A1,\"hello\",\"world\")");
        Assert.Equal(2, tokens.Count);
        Assert.Equal("hello", tokens[0].DecodedText);
        Assert.Equal("world", tokens[1].DecodedText);
    }

    [Fact]
    public void ExtractLiterals_EmptyLiteral_Extracted()
    {
        var tokens = FormulaTokenizer.ExtractLiterals("IF(A1,\"\",\"x\")");
        Assert.Equal(2, tokens.Count);
        Assert.Equal("", tokens[0].DecodedText);
        Assert.Equal("x", tokens[1].DecodedText);
    }

    [Fact]
    public void ExtractLiterals_DoubledQuotes_Decoded()
    {
        // "He said ""hello""" → He said "hello"
        var tokens = FormulaTokenizer.ExtractLiterals("\"He said \"\"hello\"\"\"");
        Assert.Single(tokens);
        Assert.Equal("He said \"hello\"", tokens[0].DecodedText);
    }

    [Fact]
    public void ExtractLiterals_NoLiterals_ReturnsEmpty()
    {
        var tokens = FormulaTokenizer.ExtractLiterals("SUM(A1:A10)");
        Assert.Empty(tokens);
    }

    [Fact]
    public void ExtractLiterals_EmptyFormula_ReturnsEmpty()
    {
        Assert.Empty(FormulaTokenizer.ExtractLiterals(""));
    }

    // ── Single-quoted identifiers are skipped ─────────────────────────────────

    [Fact]
    public void ExtractLiterals_SingleQuotedIdentifier_Skipped()
    {
        // 'Sheet Name'!A1 — no string literal
        var tokens = FormulaTokenizer.ExtractLiterals("'Sheet Name'!A1");
        Assert.Empty(tokens);
    }

    [Fact]
    public void ExtractLiterals_SingleQuotedWithEscapedApostrophe_Skipped()
    {
        // 'O''Brien'!A1 — escaped apostrophe inside identifier
        var tokens = FormulaTokenizer.ExtractLiterals("'O''Brien'!A1");
        Assert.Empty(tokens);
    }

    [Fact]
    public void ExtractLiterals_ExternalWorkbookReference_Skipped()
    {
        // '[Book1.xlsx]Sheet1'!A1
        var tokens = FormulaTokenizer.ExtractLiterals("'[Book1.xlsx]Sheet1'!A1");
        Assert.Empty(tokens);
    }

    [Fact]
    public void ExtractLiterals_IdentifierAndLiteralCombined_OnlyLiteralExtracted()
    {
        // 'Sheet1'!A1&"suffix"
        var tokens = FormulaTokenizer.ExtractLiterals("'Sheet1'!A1&\"suffix\"");
        Assert.Single(tokens);
        Assert.Equal("suffix", tokens[0].DecodedText);
    }

    [Fact]
    public void ExtractLiterals_DoubleQuoteInsideIdentifier_NotExtracted()
    {
        // 'Sheet "quoted"'!A1 — double quote inside single-quoted identifier
        var tokens = FormulaTokenizer.ExtractLiterals("'Sheet \"quoted\"'!A1");
        Assert.Empty(tokens);
    }

    // ── Column range: C:D is NOT a string literal ─────────────────────────────

    [Fact]
    public void ExtractLiterals_ColumnRange_NotExtracted()
    {
        var tokens = FormulaTokenizer.ExtractLiterals("SUM(C:D)");
        Assert.Empty(tokens);
    }

    // ── Emoji and emoticons in literals ───────────────────────────────────────

    [Fact]
    public void ExtractLiterals_EmojiInLiteral_Extracted()
    {
        var tokens = FormulaTokenizer.ExtractLiterals("IF(A1=\"✅\",\"❌\",\"\")");
        Assert.Equal(3, tokens.Count);
        Assert.Equal("✅", tokens[0].DecodedText);
        Assert.Equal("❌", tokens[1].DecodedText);
        Assert.Equal("", tokens[2].DecodedText);
    }

    // ── Unterminated quotes: InvalidDataException ─────────────────────────────

    [Fact]
    public void ExtractLiterals_UnterminatedDoubleQuote_Throws()
    {
        Assert.Throws<InvalidDataException>(() =>
            FormulaTokenizer.ExtractLiterals("IF(A1,\"unterminated,B1)"));
    }

    [Fact]
    public void ExtractLiterals_UnterminatedSingleQuote_Throws()
    {
        Assert.Throws<InvalidDataException>(() =>
            FormulaTokenizer.ExtractLiterals("'Unterminated!A1"));
    }

    [Fact]
    public void ExtractLiterals_ValidLiteralFollowedByUnterminatedSingleQuote_Throws()
    {
        // Valid literal first, then defect token — must throw, no partial results
        Assert.Throws<InvalidDataException>(() =>
            FormulaTokenizer.ExtractLiterals("\"valid\"&'unterminated"));
    }

    [Fact]
    public void ExtractLiterals_ValidLiteralFollowedByUnterminatedDoubleQuote_Throws()
    {
        Assert.Throws<InvalidDataException>(() =>
            FormulaTokenizer.ExtractLiterals("\"valid\"&\"unterminated"));
    }

    // ── Span positions ────────────────────────────────────────────────────────

    [Fact]
    public void ExtractLiterals_SpanPositions_Correct()
    {
        string formula = "IF(A1,\"hello\",B1)";
        var tokens = FormulaTokenizer.ExtractLiterals(formula);
        Assert.Single(tokens);
        // FormulaStart points to the opening quote
        Assert.Equal('"', formula[tokens[0].FormulaStart]);
        // FormulaEnd points one past the closing quote
        Assert.Equal(',', formula[tokens[0].FormulaEnd]);
    }

    // ── RebuildWithEdits ──────────────────────────────────────────────────────

    [Fact]
    public void RebuildWithEdits_NoEdits_OriginalPreserved()
    {
        string formula = "IF(A1,\"hello\",\"world\")";
        var tokens = FormulaTokenizer.ExtractLiterals(formula);
        string result = FormulaTokenizer.RebuildWithEdits(formula, tokens,
            new Dictionary<int, string>());
        Assert.Equal(formula, result);
    }

    [Fact]
    public void RebuildWithEdits_FirstToken_Replaced()
    {
        string formula = "IF(A1,\"✅\",\"❌\")";
        var tokens = FormulaTokenizer.ExtractLiterals(formula);
        string result = FormulaTokenizer.RebuildWithEdits(formula, tokens,
            new Dictionary<int, string> { [0] = "Gereed" });
        Assert.Equal("IF(A1,\"Gereed\",\"❌\")", result);
    }

    [Fact]
    public void RebuildWithEdits_AllTokens_Replaced()
    {
        string formula = "IF(A1,\"✅\",\"❌\")";
        var tokens = FormulaTokenizer.ExtractLiterals(formula);
        string result = FormulaTokenizer.RebuildWithEdits(formula, tokens,
            new Dictionary<int, string> { [0] = "Gereed", [1] = "Fout" });
        Assert.Equal("IF(A1,\"Gereed\",\"Fout\")", result);
    }

    [Fact]
    public void RebuildWithEdits_ReplacementWithQuote_DoubledCorrectly()
    {
        string formula = "\"hello\"";
        var tokens = FormulaTokenizer.ExtractLiterals(formula);
        string result = FormulaTokenizer.RebuildWithEdits(formula, tokens,
            new Dictionary<int, string> { [0] = "say \"hi\"" });
        Assert.Equal("\"say \"\"hi\"\"\"", result);
    }

    [Fact]
    public void RebuildWithEdits_ReferencesPreserved()
    {
        // References and operators must not be modified
        string formula = "VLOOKUP(A1,B:C,2,\"exact\")";
        var tokens = FormulaTokenizer.ExtractLiterals(formula);
        string result = FormulaTokenizer.RebuildWithEdits(formula, tokens,
            new Dictionary<int, string> { [0] = "EXACT" });
        Assert.Equal("VLOOKUP(A1,B:C,2,\"EXACT\")", result);
    }
}