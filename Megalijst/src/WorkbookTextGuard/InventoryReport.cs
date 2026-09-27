using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkbookTextGuard;

/// <summary>
/// Builds and serialises the inventory report produced by the 'inventory' mode.
/// The report is stable-sorted and contains unique sequences with counts,
/// codepoints, source locations, and SHA-256 hashes.
/// </summary>
internal sealed class InventoryReport
{
    public string InputFile { get; init; } = "";
    public string InputSha256 { get; init; } = "";
    public string GeneratedUtc { get; init; } = DateTime.UtcNow.ToString("o");
    public string UnicodeVersion { get; init; } = "15.1";

    public List<SequenceEntry> EmojiSequences { get; init; } = new();
    public List<SequenceEntry> EmoticonMatches { get; init; } = new();
    public List<PartHashEntry> PartHashes { get; init; } = new();
    public List<string> BlockedParts { get; init; } = new();
    public StructuralBaseline Baseline { get; init; } = new();

    public static InventoryReport Build(
        OoxmlPackage pkg,
        List<TextLocation> locations,
        EmoticonDetector emoticonDetector)
    {
        // ── Emoji sequences ──────────────────────────────────────────────────
        var emojiDict = new Dictionary<string, SequenceEntry>(StringComparer.Ordinal);
        foreach (var loc in locations)
        {
            // Skip raw formula code (not literals); cached values are included
            if (loc.IsFormula && loc.Source is not (TextSource.FormulaLiteral or TextSource.DefinedName)) continue;

            var matches = EmojiDetector.FindAll(loc.LogicalText);
            foreach (var m in matches)
            {
                if (!emojiDict.TryGetValue(m.Sequence, out var entry))
                {
                    entry = new SequenceEntry
                    {
                        Sequence    = m.Sequence,
                        Codepoints  = ToCodepointList(m.Sequence),
                        Occurrences = new List<OccurrenceEntry>()
                    };
                    emojiDict[m.Sequence] = entry;
                }
                entry.Occurrences.Add(new OccurrenceEntry(
                    loc.PartPath, loc.SheetName, loc.CellAddress,
                    loc.Source.ToString(), loc.SharedStringIndex));
                entry.Count++;
            }
        }

        // ── Emoticon matches ─────────────────────────────────────────────────
        var emoticonDict = new Dictionary<string, SequenceEntry>(StringComparer.Ordinal);
        foreach (var loc in locations)
        {
            // Scan formula literals; skip raw formula code and cached values
            if (loc.IsFormula && loc.Source is not (TextSource.FormulaLiteral or TextSource.DefinedName)) continue;

            var matches = emoticonDetector.FindAll(loc.LogicalText);
            foreach (var m in matches)
            {
                if (!emoticonDict.TryGetValue(m.Value, out var entry))
                {
                    entry = new SequenceEntry
                    {
                        Sequence    = m.Value,
                        Codepoints  = ToCodepointList(m.Value),
                        Occurrences = new List<OccurrenceEntry>()
                    };
                    emoticonDict[m.Value] = entry;
                }
                entry.Occurrences.Add(new OccurrenceEntry(
                    loc.PartPath, loc.SheetName, loc.CellAddress,
                    loc.Source.ToString(), loc.SharedStringIndex));
                entry.Count++;
            }
        }

        // ── Part hashes ──────────────────────────────────────────────────────
        var partHashes = new List<PartHashEntry>();
        foreach (var name in pkg.AllEntryNames().OrderBy(n => n, StringComparer.Ordinal))
        {
            try
            {
                string sha = pkg.PartSha256(name);
                partHashes.Add(new PartHashEntry(name, sha));
            }
            catch { /* skip unreadable entries */ }
        }

        // ── Baseline ─────────────────────────────────────────────────────────
        var baseline = new StructuralBaseline
        {
            SheetCount        = pkg.Sheets.Count,
            SheetNames        = pkg.Sheets.Select(s => s.Name).ToList(),
            HasSharedStrings  = pkg.SharedStringPartPath is not null,
            SharedStringCount = CountSharedStrings(pkg),
            CommentPartCount  = pkg.CommentPartPaths.Count,
            ExternalLinkCount = pkg.ExternalLinkPartPaths.Count
        };

        return new InventoryReport
        {
            InputFile       = pkg.FilePath,
            InputSha256     = pkg.Sha256Hex,
            GeneratedUtc    = DateTime.UtcNow.ToString("o"),
            EmojiSequences  = emojiDict.Values.OrderBy(e => e.Sequence, StringComparer.Ordinal).ToList(),
            EmoticonMatches = emoticonDict.Values.OrderBy(e => e.Sequence, StringComparer.Ordinal).ToList(),
            PartHashes      = partHashes,
            Baseline        = baseline
        };
    }

    private static int CountSharedStrings(OoxmlPackage pkg)
    {
        if (pkg.SharedStringPartPath is null || !pkg.PartExists(pkg.SharedStringPartPath))
            return 0;
        var entries = TextExtractor.ExtractSharedStringEntries(pkg);
        return entries.Count;
    }

    private static List<string> ToCodepointList(string s)
    {
        var list = new List<string>();
        for (int i = 0; i < s.Length; )
        {
            int cp = char.ConvertToUtf32(s, i);
            list.Add($"U+{cp:X4}");
            i += char.IsHighSurrogate(s[i]) ? 2 : 1;
        }
        return list;
    }

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}

internal sealed class SequenceEntry
{
    [JsonPropertyName("sequence")]    public string Sequence { get; init; } = "";
    [JsonPropertyName("codepoints")]  public List<string> Codepoints { get; init; } = new();
    [JsonPropertyName("count")]       public int Count { get; set; }
    [JsonPropertyName("occurrences")] public List<OccurrenceEntry> Occurrences { get; init; } = new();
}

internal sealed record OccurrenceEntry(
    [property: JsonPropertyName("part")]    string PartPath,
    [property: JsonPropertyName("sheet")]   string? SheetName,
    [property: JsonPropertyName("cell")]    string? CellAddress,
    [property: JsonPropertyName("source")]  string Source,
    [property: JsonPropertyName("ssIndex")] int? SharedStringIndex);

internal sealed record PartHashEntry(
    [property: JsonPropertyName("part")]   string PartPath,
    [property: JsonPropertyName("sha256")] string Sha256);

internal sealed class StructuralBaseline
{
    [JsonPropertyName("sheetCount")]        public int SheetCount { get; init; }
    [JsonPropertyName("sheetNames")]        public List<string> SheetNames { get; init; } = new();
    [JsonPropertyName("hasSharedStrings")]  public bool HasSharedStrings { get; init; }
    [JsonPropertyName("sharedStringCount")] public int SharedStringCount { get; init; }
    [JsonPropertyName("commentPartCount")]  public int CommentPartCount { get; init; }
    [JsonPropertyName("externalLinkCount")] public int ExternalLinkCount { get; init; }
}