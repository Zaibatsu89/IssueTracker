using System.Collections.Generic;
using WorkbookTextGuard;
using Xunit;

namespace WorkbookTextGuard.Tests;

public class EmojiDetectorTests
{
    // ── Basic single emoji ────────────────────────────────────────────────────

    [Theory]
    [InlineData("⚠️")]   // U+26A0 + U+FE0F
    [InlineData("✅")]
    [InlineData("❌")]
    [InlineData("🔴")]
    [InlineData("🟡")]
    [InlineData("🟢")]
    [InlineData("⏱️")]
    [InlineData("🔬")]
    [InlineData("✏️")]
    [InlineData("⌨️")]
    [InlineData("🔎")]
    [InlineData("⚙️")]
    public void FindAll_SingleEmoji_DetectsOne(string input)
    {
        var matches = EmojiDetector.FindAll(input);
        Assert.Single(matches);
        Assert.Equal(input, matches[0].Sequence);
    }

    // ── Bare digits/# are NOT emoji ───────────────────────────────────────────

    [Theory]
    [InlineData("0"), InlineData("1"), InlineData("9"),
     InlineData("#"), InlineData("*")]
    public void FindAll_BareDigitOrHash_NotEmoji(string input)
    {
        var matches = EmojiDetector.FindAll(input);
        Assert.Empty(matches);
    }

    // ── Keycap sequences ARE emoji ────────────────────────────────────────────

    [Fact]
    public void FindAll_KeycapSequence_Detected()
    {
        // 1 + VS16 + COMBINING_ENCLOSING_KEYCAP
        string keycap = "1\uFE0F\u20E3";
        var matches = EmojiDetector.FindAll(keycap);
        Assert.Single(matches);
        Assert.Equal(3, matches[0].CharLength); // 1 char + VS16 + keycap
    }

    // ── Regional indicator pair (flag) ────────────────────────────────────────

    [Fact]
    public void FindAll_FlagNL_DetectedAsSingleMatch()
    {
        // 🇳🇱 = U+1F1F3 U+1F1F1
        string flag = "\U0001F1F3\U0001F1F1";
        var matches = EmojiDetector.FindAll(flag);
        Assert.Single(matches);
        Assert.Equal(flag, matches[0].Sequence);
    }

    // ── ZWJ sequence ──────────────────────────────────────────────────────────

    [Fact]
    public void FindAll_ZwjSequence_DetectedAsOne()
    {
        // 👨‍💻 = U+1F468 ZWJ U+1F4BB
        string zwj = "\U0001F468\u200D\U0001F4BB";
        var matches = EmojiDetector.FindAll(zwj);
        Assert.Single(matches);
        Assert.Equal(zwj, matches[0].Sequence);
    }

    // ── Skin tone modifier ────────────────────────────────────────────────────

    [Fact]
    public void FindAll_SkinToneModifier_DetectedAsOne()
    {
        // 👍🏽 = U+1F44D + U+1F3FD
        string thumbs = "\U0001F44D\U0001F3FD";
        var matches = EmojiDetector.FindAll(thumbs);
        Assert.Single(matches);
        Assert.Equal(thumbs, matches[0].Sequence);
    }

    // ── Variation selectors ───────────────────────────────────────────────────

    [Fact]
    public void FindAll_VariationSelector15_IncludedInSequence()
    {
        // ⚠ + VS15 (text presentation)
        string s = "\u26A0\uFE0E";
        var matches = EmojiDetector.FindAll(s);
        Assert.Single(matches);
        Assert.Equal(s, matches[0].Sequence);
    }

    [Fact]
    public void FindAll_VariationSelector16_IncludedInSequence()
    {
        string s = "\u26A0\uFE0F"; // ⚠️
        var matches = EmojiDetector.FindAll(s);
        Assert.Single(matches);
        Assert.Equal(s, matches[0].Sequence);
    }

    // ── Mixed text ────────────────────────────────────────────────────────────

    [Fact]
    public void FindAll_MixedText_OnlyEmojiDetected()
    {
        string text = "Status: ✅ Gereed, ❌ Geblokkeerd";
        var matches = EmojiDetector.FindAll(text);
        Assert.Equal(2, matches.Count);
        Assert.Equal("✅", matches[0].Sequence);
        Assert.Equal("❌", matches[1].Sequence);
    }

    [Fact]
    public void FindAll_NoEmoji_ReturnsEmpty()
    {
        var matches = EmojiDetector.FindAll("Gewone tekst zonder emoji.");
        Assert.Empty(matches);
    }

    [Fact]
    public void FindAll_EmptyString_ReturnsEmpty()
    {
        Assert.Empty(EmojiDetector.FindAll(""));
    }

    // ── Emoji split across rich-text runs (logical text joined) ───────────────

    [Fact]
    public void FindAll_EmojiAtStartAndEnd_BothDetected()
    {
        string text = "⚠️ Let op! Zie ook ✅";
        var matches = EmojiDetector.FindAll(text);
        Assert.Equal(2, matches.Count);
    }

    // ── IsExtendedPictographic ────────────────────────────────────────────────

    [Theory]
    [InlineData(0x26A0, true)]   // ⚠
    [InlineData(0x2705, true)]   // ✅
    [InlineData(0x0041, false)]  // A
    [InlineData(0x0031, false)]  // 1
    [InlineData(0x0023, false)]  // #
    public void IsExtendedPictographic_CorrectResult(int cp, bool expected)
    {
        Assert.Equal(expected, EmojiDetector.IsExtendedPictographic(cp));
    }
}