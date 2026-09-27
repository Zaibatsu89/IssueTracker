using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkbookTextGuard;

/// <summary>
/// Represents the approved emoji/emoticon replacement policy loaded from policy.json.
///
/// Validation rules:
/// - Sequences must be non-null, non-empty, and not whitespace-only.
/// - Duplicate emoji sequences are rejected.
/// - Decorative sequences are merged into EmojiMappings as empty-replacement entries.
///   A decorative sequence that conflicts with an existing non-empty mapping is rejected.
///   An identical existing empty mapping is reused (no duplicate added).
/// - Scoped mappings with unsupported conditions block with a clear error.
/// - Mappings are sorted longest-first after validation.
/// </summary>
internal sealed class Policy
{
    public string Version { get; init; } = "1.0";
    public string UnicodeVersion { get; init; } = "15.1";
    public string EmoticonProfile { get; init; } = "plain";

    /// <summary>Approved emoji sequence → replacement text mappings (longest-first).</summary>
    public List<PolicyMapping> EmojiMappings { get; init; } = new();

    /// <summary>Approved emoticon → replacement text mappings (longest-first).</summary>
    public List<PolicyMapping> EmoticonMappings { get; init; } = new();

    /// <summary>
    /// Emoji sequences explicitly approved for removal (replacement = "").
    /// Merged into EmojiMappings at load time.
    /// </summary>
    public List<string> DecorativeSequences { get; init; } = new();

    public static Policy Load(string path)
    {
        string json = File.ReadAllText(path);
        var policy = JsonSerializer.Deserialize<Policy>(json, JsonOptions)
            ?? throw new InvalidDataException($"policy.json at '{path}' deserialised to null.");

        Validate(policy);
        return policy;
    }

    private static void Validate(Policy policy)
    {
        // ── Validate emoji mappings ──────────────────────────────────────────
        var seenEmoji = new HashSet<string>(StringComparer.Ordinal);
        foreach (var m in policy.EmojiMappings)
        {
            ValidateSequence(m.Sequence, "emojiMappings");
            if (!seenEmoji.Add(m.Sequence))
                throw new InvalidDataException($"Duplicate emoji mapping for sequence: '{m.Sequence}'");
        }

        // ── Validate emoticon mappings ────────────────────────────────────────
        var seenEmoticon = new HashSet<string>(StringComparer.Ordinal);
        foreach (var m in policy.EmoticonMappings)
        {
            ValidateSequence(m.Sequence, "emoticonMappings");
            if (!seenEmoticon.Add(m.Sequence))
                throw new InvalidDataException($"Duplicate emoticon mapping for sequence: '{m.Sequence}'");
        }

        // ── Validate and merge decorative sequences ───────────────────────────
        foreach (var seq in policy.DecorativeSequences)
        {
            ValidateSequence(seq, "decorativeSequences");

            // Check for conflict with existing non-empty emoji mapping
            var existing = policy.EmojiMappings.Find(m =>
                m.Sequence.Equals(seq, StringComparison.Ordinal));

            if (existing is not null)
            {
                if (!string.IsNullOrEmpty(existing.Replacement))
                    throw new InvalidDataException(
                        $"Decorative sequence '{seq}' conflicts with existing non-empty emoji mapping " +
                        $"(replacement: '{existing.Replacement}'). Remove the conflict before proceeding.");
                // Identical empty mapping already exists — reuse, do not add duplicate
            }
            else
            {
                // Add as empty-replacement mapping
                policy.EmojiMappings.Add(new PolicyMapping
                {
                    Sequence    = seq,
                    Replacement = "",
                    Condition   = "Decorative — merged from decorativeSequences"
                });
                seenEmoji.Add(seq);
            }
        }

        // ── Sort longest-first (after all merges) ────────────────────────────
        policy.EmojiMappings.Sort((a, b) => b.Sequence.Length.CompareTo(a.Sequence.Length));
        policy.EmoticonMappings.Sort((a, b) => b.Sequence.Length.CompareTo(a.Sequence.Length));
    }

    private static void ValidateSequence(string? seq, string fieldName)
    {
        if (seq is null)
            throw new InvalidDataException($"A sequence in '{fieldName}' is null.");
        if (seq.Length == 0)
            throw new InvalidDataException($"A sequence in '{fieldName}' is empty.");
        if (string.IsNullOrWhiteSpace(seq))
            throw new InvalidDataException($"A sequence in '{fieldName}' contains only whitespace: '{seq}'");
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling         = JsonCommentHandling.Skip,
        AllowTrailingCommas         = true,
        Converters                  = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}

internal sealed class PolicyMapping
{
    [JsonPropertyName("sequence")]
    public string Sequence { get; init; } = "";

    [JsonPropertyName("replacement")]
    public string Replacement { get; init; } = "";

    [JsonPropertyName("condition")]
    public string? Condition { get; init; }

    [JsonPropertyName("scope")]
    public MappingScope? Scope { get; init; }
}

internal sealed class MappingScope
{
    [JsonPropertyName("part")]
    public string? Part { get; init; }

    [JsonPropertyName("sheet")]
    public string? Sheet { get; init; }

    [JsonPropertyName("cell")]
    public string? Cell { get; init; }
}