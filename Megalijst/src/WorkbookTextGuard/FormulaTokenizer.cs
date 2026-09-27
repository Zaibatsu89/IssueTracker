using System;
using System.Collections.Generic;

namespace WorkbookTextGuard;

/// <summary>
/// Tokenizes OOXML formula strings to extract string literal spans.
///
/// Rules:
/// - Single-quoted identifiers (sheet names, external references) are skipped entirely,
///   including '' escape sequences within them.
/// - Double-quoted string literals are extracted with their decoded content and
///   original UTF-16 span in the formula string.
/// - Doubled double-quotes ("") within a literal represent a single literal quote.
/// - An unterminated single-quote or double-quote makes the entire formula invalid:
///   InvalidDataException is thrown and no partial tokens are returned.
/// - The full formula is validated before any tokens are yielded to callers.
/// </summary>
internal static class FormulaTokenizer
{
    public readonly record struct StringLiteralToken(
        int FormulaStart,   // inclusive start in formula string (UTF-16 index)
        int FormulaEnd,     // exclusive end in formula string (UTF-16 index)
        string DecodedText  // decoded content (doubled "" → single ")
    );

    /// <summary>
    /// Extract all string literal tokens from <paramref name="formula"/>.
    /// Throws <see cref="InvalidDataException"/> if the formula contains an
    /// unterminated single-quote or double-quote sequence.
    /// </summary>
    public static IReadOnlyList<StringLiteralToken> ExtractLiterals(string formula)
    {
        if (string.IsNullOrEmpty(formula)) return Array.Empty<StringLiteralToken>();

        // Two-pass: validate first, then collect
        Validate(formula);
        return Collect(formula);
    }

    private static void Validate(string formula)
    {
        int i = 0;
        while (i < formula.Length)
        {
            char c = formula[i];

            if (c == '\'')
            {
                // Single-quoted identifier: skip until closing '
                i++;
                bool closed = false;
                while (i < formula.Length)
                {
                    if (formula[i] == '\'')
                    {
                        if (i + 1 < formula.Length && formula[i + 1] == '\'')
                        {
                            i += 2; // escaped ''
                        }
                        else
                        {
                            i++; // closing quote
                            closed = true;
                            break;
                        }
                    }
                    else { i++; }
                }
                if (!closed)
                    throw new InvalidDataException(
                        $"Unterminated single-quoted identifier in formula: {Ellipsis(formula)}");
            }
            else if (c == '"')
            {
                // Double-quoted string literal: skip until closing "
                i++;
                bool closed = false;
                while (i < formula.Length)
                {
                    if (formula[i] == '"')
                    {
                        if (i + 1 < formula.Length && formula[i + 1] == '"')
                        {
                            i += 2; // escaped ""
                        }
                        else
                        {
                            i++; // closing quote
                            closed = true;
                            break;
                        }
                    }
                    else { i++; }
                }
                if (!closed)
                    throw new InvalidDataException(
                        $"Unterminated double-quoted string literal in formula: {Ellipsis(formula)}");
            }
            else
            {
                i++;
            }
        }
    }

    private static List<StringLiteralToken> Collect(string formula)
    {
        var tokens = new List<StringLiteralToken>();
        int i = 0;

        while (i < formula.Length)
        {
            char c = formula[i];

            if (c == '\'')
            {
                // Skip single-quoted identifier
                i++;
                while (i < formula.Length)
                {
                    if (formula[i] == '\'')
                    {
                        if (i + 1 < formula.Length && formula[i + 1] == '\'')
                            i += 2;
                        else { i++; break; }
                    }
                    else { i++; }
                }
            }
            else if (c == '"')
            {
                int start = i;
                i++; // skip opening quote
                var decoded = new System.Text.StringBuilder();
                while (i < formula.Length)
                {
                    if (formula[i] == '"')
                    {
                        if (i + 1 < formula.Length && formula[i + 1] == '"')
                        {
                            decoded.Append('"');
                            i += 2;
                        }
                        else { i++; break; } // closing quote
                    }
                    else { decoded.Append(formula[i]); i++; }
                }
                tokens.Add(new StringLiteralToken(start, i, decoded.ToString()));
            }
            else
            {
                i++;
            }
        }

        return tokens;
    }

    /// <summary>
    /// Rebuild a formula string after applying edits to specific literal tokens.
    /// Only the decoded text of the specified tokens is replaced; all other
    /// formula syntax (references, operators, identifiers) is preserved exactly.
    /// </summary>
    public static string RebuildWithEdits(
        string formula,
        IReadOnlyList<StringLiteralToken> tokens,
        IReadOnlyDictionary<int, string> tokenIndexToNewText)
    {
        if (tokenIndexToNewText.Count == 0) return formula;

        var sb = new System.Text.StringBuilder();
        int pos = 0;

        for (int t = 0; t < tokens.Count; t++)
        {
            var token = tokens[t];
            // Copy formula text before this token (including the opening quote)
            sb.Append(formula, pos, token.FormulaStart - pos + 1); // up to and including "
            pos = token.FormulaStart + 1; // skip opening quote

            if (tokenIndexToNewText.TryGetValue(t, out string? newText))
            {
                // Write new text with doubled quotes
                sb.Append(newText.Replace("\"", "\"\""));
            }
            else
            {
                // Write original encoded content (between the quotes)
                // token.FormulaEnd - 1 is the closing quote position
                sb.Append(formula, pos, token.FormulaEnd - 1 - pos);
            }

            sb.Append('"'); // closing quote
            pos = token.FormulaEnd;
        }

        // Append remaining formula text after last token
        sb.Append(formula, pos, formula.Length - pos);
        return sb.ToString();
    }

    private static string Ellipsis(string s, int max = 80)
        => s.Length <= max ? s : s[..max] + "…";
}