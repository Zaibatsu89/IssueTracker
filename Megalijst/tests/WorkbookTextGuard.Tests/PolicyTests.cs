using System;
using System.IO;
using WorkbookTextGuard;
using Xunit;

namespace WorkbookTextGuard.Tests;

public class PolicyTests
{
    private static string WriteTempPolicy(string json)
    {
        string path = Path.GetTempFileName();
        File.WriteAllText(path, json);
        return path;
    }

    // ── Valid policy loading ──────────────────────────────────────────────────

    [Fact]
    public void Load_ValidPolicy_Succeeds()
    {
        string path = WriteTempPolicy("""
        {
          "version": "1.0",
          "unicodeVersion": "15.1",
          "emoticonProfile": "plain",
          "emojiMappings": [
            { "sequence": "✅", "replacement": "Gereed" }
          ],
          "emoticonMappings": [],
          "decorativeSequences": []
        }
        """);
        var policy = Policy.Load(path);
        Assert.Single(policy.EmojiMappings);
        Assert.Equal("✅", policy.EmojiMappings[0].Sequence);
        Assert.Equal("Gereed", policy.EmojiMappings[0].Replacement);
        File.Delete(path);
    }

    [Fact]
    public void Load_EmptyReplacement_Allowed()
    {
        string path = WriteTempPolicy("""
        {
          "emojiMappings": [{ "sequence": "⏱️", "replacement": "" }],
          "emoticonMappings": [],
          "decorativeSequences": []
        }
        """);
        var policy = Policy.Load(path);
        Assert.Single(policy.EmojiMappings);
        Assert.Equal("", policy.EmojiMappings[0].Replacement);
        File.Delete(path);
    }

    // ── Null / empty / whitespace sequence validation ─────────────────────────

    [Fact]
    public void Load_EmptyEmojiSequence_Throws()
    {
        string path = WriteTempPolicy("""
        {
          "emojiMappings": [{ "sequence": "", "replacement": "X" }],
          "emoticonMappings": [],
          "decorativeSequences": []
        }
        """);
        Assert.Throws<InvalidDataException>(() => Policy.Load(path));
        File.Delete(path);
    }

    [Fact]
    public void Load_WhitespaceOnlyEmojiSequence_Throws()
    {
        string path = WriteTempPolicy("""
        {
          "emojiMappings": [{ "sequence": "   ", "replacement": "X" }],
          "emoticonMappings": [],
          "decorativeSequences": []
        }
        """);
        Assert.Throws<InvalidDataException>(() => Policy.Load(path));
        File.Delete(path);
    }

    [Fact]
    public void Load_EmptyEmoticonSequence_Throws()
    {
        string path = WriteTempPolicy("""
        {
          "emojiMappings": [],
          "emoticonMappings": [{ "sequence": "", "replacement": "" }],
          "decorativeSequences": []
        }
        """);
        Assert.Throws<InvalidDataException>(() => Policy.Load(path));
        File.Delete(path);
    }

    [Fact]
    public void Load_EmptyDecorativeSequence_Throws()
    {
        string path = WriteTempPolicy("""
        {
          "emojiMappings": [],
          "emoticonMappings": [],
          "decorativeSequences": [""]
        }
        """);
        Assert.Throws<InvalidDataException>(() => Policy.Load(path));
        File.Delete(path);
    }

    [Fact]
    public void Load_WhitespaceDecorativeSequence_Throws()
    {
        string path = WriteTempPolicy("""
        {
          "emojiMappings": [],
          "emoticonMappings": [],
          "decorativeSequences": [" "]
        }
        """);
        Assert.Throws<InvalidDataException>(() => Policy.Load(path));
        File.Delete(path);
    }

    // ── Duplicate detection ───────────────────────────────────────────────────

    [Fact]
    public void Load_DuplicateEmojiSequence_Throws()
    {
        string path = WriteTempPolicy("""
        {
          "emojiMappings": [
            { "sequence": "✅", "replacement": "Gereed" },
            { "sequence": "✅", "replacement": "OK" }
          ],
          "emoticonMappings": [],
          "decorativeSequences": []
        }
        """);
        Assert.Throws<InvalidDataException>(() => Policy.Load(path));
        File.Delete(path);
    }

    [Fact]
    public void Load_DuplicateEmoticonSequence_Throws()
    {
        string path = WriteTempPolicy("""
        {
          "emojiMappings": [],
          "emoticonMappings": [
            { "sequence": ":-)", "replacement": "" },
            { "sequence": ":-)", "replacement": "blij" }
          ],
          "decorativeSequences": []
        }
        """);
        Assert.Throws<InvalidDataException>(() => Policy.Load(path));
        File.Delete(path);
    }

    // ── Decorative sequences ──────────────────────────────────────────────────

    [Fact]
    public void Load_DecorativeOnly_MergedAsEmptyMapping()
    {
        string path = WriteTempPolicy("""
        {
          "emojiMappings": [],
          "emoticonMappings": [],
          "decorativeSequences": ["⏱️", "🔬"]
        }
        """);
        var policy = Policy.Load(path);
        // Both decorative sequences merged into EmojiMappings as empty replacements
        Assert.Equal(2, policy.EmojiMappings.Count);
        Assert.All(policy.EmojiMappings, m => Assert.Equal("", m.Replacement));
        File.Delete(path);
    }

    [Fact]
    public void Load_DecorativeWithExistingEmptyMapping_Reused_NoDuplicate()
    {
        // ⏱️ already has an empty mapping — decorative entry should not add a duplicate
        string path = WriteTempPolicy("""
        {
          "emojiMappings": [{ "sequence": "⏱️", "replacement": "" }],
          "emoticonMappings": [],
          "decorativeSequences": ["⏱️"]
        }
        """);
        var policy = Policy.Load(path);
        Assert.Single(policy.EmojiMappings); // no duplicate added
        File.Delete(path);
    }

    [Fact]
    public void Load_DecorativeConflictsWithNonEmptyMapping_Throws()
    {
        // ⏱️ has a non-empty mapping — decorative entry conflicts
        string path = WriteTempPolicy("""
        {
          "emojiMappings": [{ "sequence": "⏱️", "replacement": "Timer" }],
          "emoticonMappings": [],
          "decorativeSequences": ["⏱️"]
        }
        """);
        Assert.Throws<InvalidDataException>(() => Policy.Load(path));
        File.Delete(path);
    }

    // ── Longest-first sorting ─────────────────────────────────────────────────

    [Fact]
    public void Load_LongestFirst_SortedDescending()
    {
        string path = WriteTempPolicy("""
        {
          "emojiMappings": [
            { "sequence": "A",   "replacement": "short" },
            { "sequence": "ABC", "replacement": "long"  },
            { "sequence": "AB",  "replacement": "medium"}
          ],
          "emoticonMappings": [],
          "decorativeSequences": []
        }
        """);
        var policy = Policy.Load(path);
        Assert.Equal("ABC", policy.EmojiMappings[0].Sequence);
        Assert.Equal("AB",  policy.EmojiMappings[1].Sequence);
        Assert.Equal("A",   policy.EmojiMappings[2].Sequence);
        File.Delete(path);
    }

    // ── Emoticon profile ──────────────────────────────────────────────────────

    [Fact]
    public void Load_EmoticonProfilePlain_Parsed()
    {
        string path = WriteTempPolicy("""
        {"emoticonProfile":"plain","emojiMappings":[],"emoticonMappings":[],"decorativeSequences":[]}
        """);
        Assert.Equal("plain", Policy.Load(path).EmoticonProfile);
        File.Delete(path);
    }

    [Fact]
    public void Load_EmoticonProfileMarkup_Parsed()
    {
        string path = WriteTempPolicy("""
        {"emoticonProfile":"markup","emojiMappings":[],"emoticonMappings":[],"decorativeSequences":[]}
        """);
        Assert.Equal("markup", Policy.Load(path).EmoticonProfile);
        File.Delete(path);
    }

    // ── Comments and trailing commas ──────────────────────────────────────────

    [Fact]
    public void Load_WithComments_Succeeds()
    {
        string path = WriteTempPolicy("""
        {
          // comment
          "emojiMappings": [
            { "sequence": "⚠️", "replacement": "Aandachtspunt" },
          ],
          "emoticonMappings": [],
          "decorativeSequences": []
        }
        """);
        var policy = Policy.Load(path);
        Assert.Single(policy.EmojiMappings);
        File.Delete(path);
    }

    // ── Scoped mappings ───────────────────────────────────────────────────────

    [Fact]
    public void Load_ScopedMapping_Parsed()
    {
        string path = WriteTempPolicy("""
        {
          "emojiMappings": [{
            "sequence": "🔴",
            "replacement": "Geblokkeerd",
            "scope": { "sheet": "Overzicht", "cell": "B2" }
          }],
          "emoticonMappings": [],
          "decorativeSequences": []
        }
        """);
        var policy = Policy.Load(path);
        Assert.Single(policy.EmojiMappings);
        Assert.NotNull(policy.EmojiMappings[0].Scope);
        Assert.Equal("Overzicht", policy.EmojiMappings[0].Scope!.Sheet);
        File.Delete(path);
    }

    // ── Version fields ────────────────────────────────────────────────────────

    [Fact]
    public void Load_VersionFields_Parsed()
    {
        string path = WriteTempPolicy("""
        {"version":"2.0","unicodeVersion":"15.1","emojiMappings":[],"emoticonMappings":[],"decorativeSequences":[]}
        """);
        var policy = Policy.Load(path);
        Assert.Equal("2.0", policy.Version);
        Assert.Equal("15.1", policy.UnicodeVersion);
        File.Delete(path);
    }

    // ── Missing file ──────────────────────────────────────────────────────────

    [Fact]
    public void Load_MissingFile_Throws()
    {
        // The parent directory must exist to test a missing file, not a missing directory.
        string path = Path.Combine(Path.GetTempPath(), $"missing-policy-{Guid.NewGuid():N}.json");
        Assert.Throws<FileNotFoundException>(() => Policy.Load(path));
    }
}