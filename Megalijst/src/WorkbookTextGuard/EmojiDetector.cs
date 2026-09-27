using System;
using System.Collections.Generic;
using System.Text;

namespace WorkbookTextGuard;

/// <summary>
/// Detects emoji sequences in strings using a longest-match, codepoint-aware approach.
/// Covers: single emoji, variation selectors (U+FE0E/FE0F), ZWJ sequences,
/// regional indicator pairs (flags), keycap sequences, modifier sequences, tag sequences.
/// Uses a pinned internal table derived from Unicode Emoji data (v15.1 subset).
/// </summary>
internal static class EmojiDetector
{
    // ── Sentinel codepoints ──────────────────────────────────────────────────
    private const int ZWJ        = 0x200D;
    private const int VS15       = 0xFE0E; // text variation selector
    private const int VS16       = 0xFE0F; // emoji variation selector
    private const int COMBINING_ENCLOSING_KEYCAP = 0x20E3;
    private const int TAG_CANCEL = 0xE007F;

    // Regional indicators: U+1F1E6..U+1F1FF
    private const int RI_START = 0x1F1E6;
    private const int RI_END   = 0x1F1FF;

    // Tag characters: U+E0020..U+E007E
    private const int TAG_START = 0xE0020;
    private const int TAG_END   = 0xE007E;

    // Emoji modifier base range (skin tone modifiers U+1F3FB..U+1F3FF)
    private const int MOD_START = 0x1F3FB;
    private const int MOD_END   = 0x1F3FF;

    // Keycap base characters: 0-9, #, *
    private static readonly HashSet<int> KeycapBases = new() { '#', '*', '0','1','2','3','4','5','6','7','8','9' };

    /// <summary>
    /// Extended_Pictographic codepoint ranges (Unicode 15.1, pinned).
    /// Losse cijfers, # en * zijn GEEN emoji zonder keycap-sequentie.
    /// </summary>
    private static readonly (int Start, int End)[] ExtendedPictographic =
    {
        (0x00A9, 0x00A9), (0x00AE, 0x00AE),
        (0x203C, 0x203C), (0x2049, 0x2049),
        (0x2122, 0x2122), (0x2139, 0x2139),
        (0x2194, 0x2199), (0x21A9, 0x21AA),
        (0x231A, 0x231B), (0x2328, 0x2328),
        (0x23CF, 0x23CF), (0x23E9, 0x23F3),
        (0x23F8, 0x23FA), (0x24C2, 0x24C2),
        (0x25AA, 0x25AB), (0x25B6, 0x25B6),
        (0x25C0, 0x25C0), (0x25FB, 0x25FE),
        (0x2600, 0x2604), (0x260E, 0x260E),
        (0x2611, 0x2611), (0x2614, 0x2615),
        (0x2618, 0x2618), (0x261D, 0x261D),
        (0x2620, 0x2620), (0x2622, 0x2623),
        (0x2626, 0x2626), (0x262A, 0x262A),
        (0x262E, 0x262F), (0x2638, 0x263A),
        (0x2640, 0x2640), (0x2642, 0x2642),
        (0x2648, 0x2653), (0x265F, 0x2660),
        (0x2663, 0x2663), (0x2665, 0x2666),
        (0x2668, 0x2668), (0x267B, 0x267B),
        (0x267E, 0x267F), (0x2692, 0x2697),
        (0x2699, 0x2699), (0x269B, 0x269C),
        (0x26A0, 0x26A1), (0x26A7, 0x26A7),
        (0x26AA, 0x26AB), (0x26B0, 0x26B1),
        (0x26BD, 0x26BE), (0x26C4, 0x26C5),
        (0x26CE, 0x26CF), (0x26D1, 0x26D1),
        (0x26D3, 0x26D4), (0x26E9, 0x26EA),
        (0x26F0, 0x26F5), (0x26F7, 0x26FA),
        (0x26FD, 0x26FD), (0x2702, 0x2702),
        (0x2705, 0x2705), (0x2708, 0x270D),
        (0x270F, 0x270F), (0x2712, 0x2712),
        (0x2714, 0x2714), (0x2716, 0x2716),
        (0x271D, 0x271D), (0x2721, 0x2721),
        (0x2728, 0x2728), (0x2733, 0x2734),
        (0x2744, 0x2744), (0x2747, 0x2747),
        (0x274C, 0x274C), (0x274E, 0x274E),
        (0x2753, 0x2755), (0x2757, 0x2757),
        (0x2763, 0x2764), (0x2795, 0x2797),
        (0x27A1, 0x27A1), (0x27B0, 0x27B0),
        (0x27BF, 0x27BF), (0x2934, 0x2935),
        (0x2B05, 0x2B07), (0x2B1B, 0x2B1C),
        (0x2B50, 0x2B50), (0x2B55, 0x2B55),
        (0x3030, 0x3030), (0x303D, 0x303D),
        (0x3297, 0x3297), (0x3299, 0x3299),
        (0x1F004, 0x1F004), (0x1F0CF, 0x1F0CF),
        (0x1F170, 0x1F171), (0x1F17E, 0x1F17F),
        (0x1F18E, 0x1F18E), (0x1F191, 0x1F19A),
        (0x1F1E0, 0x1F1FF), // regional indicators (also flag bases)
        (0x1F201, 0x1F202), (0x1F21A, 0x1F21A),
        (0x1F22F, 0x1F22F), (0x1F232, 0x1F23A),
        (0x1F250, 0x1F251),
        (0x1F300, 0x1F321), (0x1F324, 0x1F393),
        (0x1F396, 0x1F397), (0x1F399, 0x1F39B),
        (0x1F39E, 0x1F3F0), (0x1F3F3, 0x1F3F5),
        (0x1F3F7, 0x1F4FD), (0x1F4FF, 0x1F53D),
        (0x1F549, 0x1F54E), (0x1F550, 0x1F567),
        (0x1F56F, 0x1F570), (0x1F573, 0x1F57A),
        (0x1F587, 0x1F587), (0x1F58A, 0x1F58D),
        (0x1F590, 0x1F590), (0x1F595, 0x1F596),
        (0x1F5A4, 0x1F5A5), (0x1F5A8, 0x1F5A8),
        (0x1F5B1, 0x1F5B2), (0x1F5BC, 0x1F5BC),
        (0x1F5C2, 0x1F5C4), (0x1F5D1, 0x1F5D3),
        (0x1F5DC, 0x1F5DE), (0x1F5E1, 0x1F5E1),
        (0x1F5E3, 0x1F5E3), (0x1F5E8, 0x1F5E8),
        (0x1F5EF, 0x1F5EF), (0x1F5F3, 0x1F5F3),
        (0x1F5FA, 0x1F64F),
        (0x1F680, 0x1F6C5), (0x1F6CB, 0x1F6D2),
        (0x1F6D5, 0x1F6D7), (0x1F6DC, 0x1F6E5),
        (0x1F6E9, 0x1F6E9), (0x1F6EB, 0x1F6EC),
        (0x1F6F0, 0x1F6F0), (0x1F6F3, 0x1F6FC),
        (0x1F7E0, 0x1F7EB), (0x1F7F0, 0x1F7F0),
        (0x1F90C, 0x1F93A), (0x1F93C, 0x1F945),
        (0x1F947, 0x1F9FF),
        (0x1FA00, 0x1FA53), (0x1FA60, 0x1FA6D),
        (0x1FA70, 0x1FA7C), (0x1FA80, 0x1FA88),
        (0x1FA90, 0x1FABD), (0x1FABF, 0x1FAC5),
        (0x1FACE, 0x1FADB), (0x1FAE0, 0x1FAE8),
        (0x1FAF0, 0x1FAF8),
        (0x1F3FB, 0x1F3FF), // skin tone modifiers
    };

    public static bool IsExtendedPictographic(int cp)
    {
        foreach (var (s, e) in ExtendedPictographic)
            if (cp >= s && cp <= e) return true;
        return false;
    }

    private static bool IsRegionalIndicator(int cp) => cp >= RI_START && cp <= RI_END;
    private static bool IsTagChar(int cp) => cp >= TAG_START && cp <= TAG_END;
    private static bool IsModifier(int cp) => cp >= MOD_START && cp <= MOD_END;

    /// <summary>
    /// Find all emoji/emoticon matches in <paramref name="text"/>.
    /// Returns a list of (startIndex, length, sequence) in string index terms.
    /// </summary>
    public static List<EmojiMatch> FindAll(string text)
    {
        var results = new List<EmojiMatch>();
        if (string.IsNullOrEmpty(text)) return results;

        int[] cps = ToCodepoints(text);
        int[] cpToCharIdx = CodepointToCharIndex(text);

        int i = 0;
        while (i < cps.Length)
        {
            int matchLen = TryMatchSequence(cps, i);
            if (matchLen > 0)
            {
                int charStart = cpToCharIdx[i];
                int charEnd   = (i + matchLen < cps.Length)
                    ? cpToCharIdx[i + matchLen]
                    : text.Length;
                string seq = text[charStart..charEnd];
                results.Add(new EmojiMatch(charStart, charEnd - charStart, seq));
                i += matchLen;
            }
            else
            {
                i++;
            }
        }
        return results;
    }

    /// <summary>
    /// Returns the number of codepoints consumed by an emoji sequence starting at <paramref name="i"/>,
    /// or 0 if no emoji starts here.
    /// </summary>
    private static int TryMatchSequence(int[] cps, int i)
    {
        int cp = cps[i];

        // ── Regional indicator pair (flag) ───────────────────────────────────
        if (IsRegionalIndicator(cp))
        {
            if (i + 1 < cps.Length && IsRegionalIndicator(cps[i + 1]))
                return 2;
            return 1; // lone RI is still emoji
        }

        // ── Keycap sequence: base + VS16 + COMBINING_ENCLOSING_KEYCAP ───────
        if (KeycapBases.Contains(cp))
        {
            if (i + 2 < cps.Length && cps[i + 1] == VS16 && cps[i + 2] == COMBINING_ENCLOSING_KEYCAP)
                return 3;
            if (i + 1 < cps.Length && cps[i + 1] == COMBINING_ENCLOSING_KEYCAP)
                return 2;
            return 0; // bare digit/# is NOT emoji
        }

        // ── Extended pictographic ────────────────────────────────────────────
        if (!IsExtendedPictographic(cp)) return 0;

        int len = 1;

        // Optional variation selector
        if (i + len < cps.Length && (cps[i + len] == VS15 || cps[i + len] == VS16))
            len++;

        // Optional skin tone modifier
        if (i + len < cps.Length && IsModifier(cps[i + len]))
            len++;

        // Optional tag sequence (e.g. subdivision flags): base + tags + TAG_CANCEL
        if (i + len < cps.Length && IsTagChar(cps[i + len]))
        {
            int tagLen = len;
            while (i + tagLen < cps.Length && IsTagChar(cps[i + tagLen]))
                tagLen++;
            if (i + tagLen < cps.Length && cps[i + tagLen] == TAG_CANCEL)
            {
                tagLen++; // include TAG_CANCEL
                len = tagLen;
            }
        }

        // ZWJ chain: emoji ZWJ emoji ZWJ emoji ...
        while (i + len < cps.Length && cps[i + len] == ZWJ)
        {
            int zwjPos = i + len;
            if (zwjPos + 1 >= cps.Length) break;
            int next = TryMatchSingleOrModified(cps, zwjPos + 1);
            if (next == 0) break;
            len += 1 + next; // ZWJ + next emoji
        }

        return len;
    }

    /// <summary>Match a single emoji codepoint with optional VS and modifier (no ZWJ).</summary>
    private static int TryMatchSingleOrModified(int[] cps, int i)
    {
        if (i >= cps.Length) return 0;
        int cp = cps[i];
        if (!IsExtendedPictographic(cp) && !IsRegionalIndicator(cp)) return 0;
        int len = 1;
        if (i + len < cps.Length && (cps[i + len] == VS15 || cps[i + len] == VS16)) len++;
        if (i + len < cps.Length && IsModifier(cps[i + len])) len++;
        return len;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static int[] ToCodepoints(string s)
    {
        var list = new List<int>(s.Length);
        for (int i = 0; i < s.Length; )
        {
            int cp = char.ConvertToUtf32(s, i);
            list.Add(cp);
            i += char.IsHighSurrogate(s[i]) ? 2 : 1;
        }
        return list.ToArray();
    }

    /// <summary>Maps codepoint index → char index in the original string.</summary>
    private static int[] CodepointToCharIndex(string s)
    {
        var list = new List<int>(s.Length);
        for (int i = 0; i < s.Length; )
        {
            list.Add(i);
            i += char.IsHighSurrogate(s[i]) ? 2 : 1;
        }
        return list.ToArray();
    }
}

internal readonly record struct EmojiMatch(int CharStart, int CharLength, string Sequence);