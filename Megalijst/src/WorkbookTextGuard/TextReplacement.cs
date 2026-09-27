using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace WorkbookTextGuard;

/// <summary>
/// Represents a single text replacement edit on a logical string.
/// All positions are in UTF-16 code units (C# string indices).
/// </summary>
internal readonly record struct TextEdit(int Start, int Length, string Replacement)
{
    /// <summary>Exclusive end position.</summary>
    public int End => Start + Length;
}

/// <summary>
/// Collects non-overlapping edits on a logical text string and applies them
/// in a single right-to-left pass so original offsets remain valid.
///
/// Rules:
/// - Longest-match wins at the same start position.
/// - Replacement text is never re-scanned.
/// - Surrogate pairs are never split: a match may not begin or end inside a pair.
/// - Overlapping matches are skipped (first match wins by start position).
/// </summary>
internal sealed class TextEditList
{
    private readonly List<TextEdit> _edits = new();

    /// <summary>Add an edit. Validates surrogate boundaries.</summary>
    public void Add(string source, int start, int length, string replacement)
    {
        ValidateSurrogateBoundary(source, start, length);
        _edits.Add(new TextEdit(start, length, replacement));
    }

    /// <summary>
    /// Sort edits by start position, remove overlaps (first wins),
    /// then apply right-to-left on <paramref name="source"/>.
    /// </summary>
    public string Apply(string source)
    {
        if (_edits.Count == 0) return source;

        // Sort by start ascending, then by length descending (longest-match first)
        _edits.Sort((a, b) => a.Start != b.Start
            ? a.Start.CompareTo(b.Start)
            : b.Length.CompareTo(a.Length));

        // Remove overlapping edits (keep first/longest at each position)
        var nonOverlapping = new List<TextEdit>(_edits.Count);
        int lastEnd = 0;
        foreach (var edit in _edits)
        {
            if (edit.Start >= lastEnd)
            {
                nonOverlapping.Add(edit);
                lastEnd = edit.End;
            }
        }

        // Apply right-to-left so earlier offsets stay valid
        var chars = new System.Text.StringBuilder(source);
        for (int i = nonOverlapping.Count - 1; i >= 0; i--)
        {
            var e = nonOverlapping[i];
            chars.Remove(e.Start, e.Length);
            chars.Insert(e.Start, e.Replacement);
        }
        return chars.ToString();
    }

    public bool HasEdits => _edits.Count > 0;

    private static void ValidateSurrogateBoundary(string source, int start, int length)
    {
        // Start must not be inside a surrogate pair
        if (start > 0 && char.IsHighSurrogate(source[start - 1]) && start < source.Length && char.IsLowSurrogate(source[start]))
            throw new ArgumentException($"Edit start={start} splits a surrogate pair in: {source}");

        int end = start + length;
        if (end > 0 && end < source.Length && char.IsHighSurrogate(source[end - 1]) && char.IsLowSurrogate(source[end]))
            throw new ArgumentException($"Edit end={end} splits a surrogate pair in: {source}");

        // Lone surrogates within the match are rejected
        for (int i = start; i < end; i++)
        {
            char c = source[i];
            if (char.IsHighSurrogate(c))
            {
                if (i + 1 >= end || !char.IsLowSurrogate(source[i + 1]))
                    throw new ArgumentException($"Lone high surrogate at position {i} in: {source}");
                i++; // skip low surrogate
            }
            else if (char.IsLowSurrogate(c))
            {
                throw new ArgumentException($"Lone low surrogate at position {i} in: {source}");
            }
        }
    }
}

/// <summary>
/// Maps a position in the logical (concatenated) text of a rich-text &lt;si&gt;
/// back to the specific &lt;r&gt;/&lt;t&gt; element and local offset within it.
/// </summary>
internal sealed class RunOffsetMap
{
    private static readonly XNamespace Ns = OoxmlNamespaces.SpreadsheetMl;

    public readonly record struct RunSpan(XElement Run, XElement T, int LogicalStart, int LogicalEnd);

    private readonly List<RunSpan> _spans = new();
    public string LogicalText { get; }

    public RunOffsetMap(XElement siOrIs)
    {
        var runs = siOrIs.Elements(Ns + "r");
        bool hasRuns = false;
        int pos = 0;
        var sb = new System.Text.StringBuilder();

        foreach (var r in runs)
        {
            hasRuns = true;
            var t = r.Element(Ns + "t");
            string val = t?.Value ?? "";
            _spans.Add(new RunSpan(r, t ?? new XElement(Ns + "t"), pos, pos + val.Length));
            sb.Append(val);
            pos += val.Length;
        }

        if (!hasRuns)
        {
            // Plain <t>
            var t = siOrIs.Element(Ns + "t");
            string val = t?.Value ?? "";
            if (t is not null)
                _spans.Add(new RunSpan(siOrIs, t, 0, val.Length));
            sb.Append(val);
        }

        LogicalText = sb.ToString();
    }

    /// <summary>
    /// Apply a list of edits to the rich-text element.
    /// Edits are distributed across runs; run formatting (rPr) is preserved.
    /// Empty runs are removed after all edits; a completely empty si gets one empty &lt;t/&gt;.
    /// </summary>
    public void ApplyEdits(XElement siOrIs, List<TextEdit> edits)
    {
        if (edits.Count == 0) return;

        // Sort right-to-left
        edits.Sort((a, b) => b.Start.CompareTo(a.Start));

        // Build mutable per-run text buffers
        var runTexts = new Dictionary<XElement, System.Text.StringBuilder>();
        foreach (var span in _spans)
            if (!runTexts.ContainsKey(span.Run))
                runTexts[span.Run] = new System.Text.StringBuilder(span.T.Value);

        bool isPlain = _spans.Count == 1 && _spans[0].Run == siOrIs;

        foreach (var edit in edits)
        {
            // Find which runs are touched by [edit.Start, edit.End)
            bool replacementPlaced = false;

            for (int si = 0; si < _spans.Count; si++)
            {
                var span = _spans[si];
                if (span.LogicalEnd <= edit.Start) continue;
                if (span.LogicalStart >= edit.End) break;

                // Overlap: [max(edit.Start, span.LogicalStart), min(edit.End, span.LogicalEnd))
                int localStart = Math.Max(edit.Start, span.LogicalStart) - span.LogicalStart;
                int localEnd   = Math.Min(edit.End,   span.LogicalEnd)   - span.LogicalStart;
                int localLen   = localEnd - localStart;

                var buf = runTexts[span.Run];

                if (!replacementPlaced)
                {
                    // First touched run: replace the overlapping chars with the replacement
                    buf.Remove(localStart, localLen);
                    buf.Insert(localStart, edit.Replacement);
                    replacementPlaced = true;
                }
                else
                {
                    // Subsequent runs: remove only the overlapping chars
                    buf.Remove(localStart, localLen);
                }
            }
        }

        // Write back and clean up
        if (isPlain)
        {
            var t = _spans[0].T;
            t.Value = runTexts[siOrIs].ToString();
            EnsureXmlSpacePreserve(t);
            return;
        }

        var toRemove = new List<XElement>();
        foreach (var span in _spans)
        {
            string newVal = runTexts[span.Run].ToString();
            span.T.Value = newVal;
            EnsureXmlSpacePreserve(span.T);
            if (newVal.Length == 0)
                toRemove.Add(span.Run);
        }

        foreach (var r in toRemove)
            r.Remove();

        // If all runs removed, ensure exactly one empty <t/>
        if (!siOrIs.Elements(Ns + "r").GetEnumerator().MoveNext())
        {
            // Remove any stale plain <t> and add a fresh empty one
            siOrIs.Element(Ns + "t")?.Remove();
            siOrIs.Add(new XElement(Ns + "t"));
        }
    }

    private static void EnsureXmlSpacePreserve(XElement t)
    {
        string val = t.Value;
        if (val.Length > 0 && (char.IsWhiteSpace(val[0]) || char.IsWhiteSpace(val[^1])))
            t.SetAttributeValue(XNamespace.Xml + "space", "preserve");
        else
            t.Attribute(XNamespace.Xml + "space")?.Remove();
    }
}