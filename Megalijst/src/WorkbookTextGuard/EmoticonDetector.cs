using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WorkbookTextGuard;

/// <summary>
/// Detects textual emoticons in strings using a closed, case-sensitive list
/// with strict boundary rules (no bare \b around punctuation emoticons).
///
/// Two boundary profiles are supported:
///   plain  — whitespace, common punctuation and quote characters (incl. U+201E/U+201F)
///   markup — additionally includes *, _, ~, ` (Markdown/wiki delimiters)
///
/// The profile is set once at construction and used consistently for both
/// check and clean operations.
///
/// API:
///   FindAll(text)                                    — locate all matches
///   ReplaceWithMappings(text, mappings)              — replace only explicitly mapped matches;
///                                                      unmapped matches and context are preserved
/// </summary>
internal sealed class EmoticonDetector
{
    public enum BoundaryProfile { Plain, Markup }

    // Emoticon alternatives (longest first to satisfy longest-match intent)
    private const string EmoticonAlts = @":-\)|:\)|;-\)|;\)|:-D|XD";

    // Boundary character classes — include U+201E („) and U+201F (‟)
    private const string PlainLeft  = @"[\s(\[!?,.""\u201C\u201D\u201E\u201F\u2018\u2019'']";
    private const string PlainRight = @"[\s)\]!?,.;:""\u201C\u201D\u201E\u201F\u2018\u2019'']";
    private const string MarkupLeft  = @"[\s(\[!?,.""\u201C\u201D\u201E\u201F\u2018\u2019''*_~\x60]";
    private const string MarkupRight = @"[\s)\]!?,.;:""\u201C\u201D\u201E\u201F\u2018\u2019''*_~\x60]";

    private readonly Regex _regex;
    public BoundaryProfile Profile { get; }

    public EmoticonDetector(BoundaryProfile profile = BoundaryProfile.Plain)
    {
        Profile = profile;
        string left  = profile == BoundaryProfile.Markup ? MarkupLeft  : PlainLeft;
        string right = profile == BoundaryProfile.Markup ? MarkupRight : PlainRight;

        // Fixed single-char lookbehind; absolute anchors \A and \z
        string pattern = $@"(?:\A|(?<={left}))(?<emoticon>{EmoticonAlts})(?=\z|{right})";
        _regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }

    /// <summary>Find all emoticon matches in <paramref name="text"/>.</summary>
    public List<EmoticonMatch> FindAll(string text)
    {
        var results = new List<EmoticonMatch>();
        if (string.IsNullOrEmpty(text)) return results;

        foreach (Match m in _regex.Matches(text))
        {
            var g = m.Groups["emoticon"];
            results.Add(new EmoticonMatch(g.Index, g.Length, g.Value));
        }
        return results;
    }

    /// <summary>
    /// Replace emoticon matches that appear in <paramref name="mappings"/> with their
    /// mapped replacement. Unmapped matches are left intact. Context (quotes, punctuation)
    /// is always preserved. Replacement text is never re-scanned.
    ///
    /// Uses the MatchEvaluator overload so that $ characters in replacement values
    /// are treated as literal text, not as regex backreferences.
    /// </summary>
    public string ReplaceWithMappings(string text, IReadOnlyDictionary<string, string> mappings)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (mappings.Count == 0) return text;

        return _regex.Replace(text, match =>
        {
            var g = match.Groups["emoticon"];
            string val = g.Value;

            // Positions relative to match.Value
            int preLen    = g.Index - match.Index;
            int postStart = g.Index + g.Length - match.Index;

            string pre  = match.Value[..preLen];
            string post = match.Value[postStart..];

            if (mappings.TryGetValue(val, out string? replacement))
                return pre + replacement + post;

            // Unmapped: return original match text unchanged
            return match.Value;
        });
    }
}

internal readonly record struct EmoticonMatch(int CharStart, int CharLength, string Value);