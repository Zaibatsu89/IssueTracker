using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace WorkbookTextGuard;

/// <summary>
/// Applies approved policy mappings to an OOXML workbook, producing a cleaned copy.
///
/// Design:
/// - Uses RunOffsetMap to distribute edits across rich-text runs without destroying rPr.
/// - Applies edits right-to-left so original offsets remain valid.
/// - Shared strings: copy-on-write when cells require different edits.
///   In-place only when ALL referencing cells get the same approved edits.
/// - Scope evaluation uses MappingScopeMatcher with worksheet context.
/// - Formula string literals are cleaned via FormulaTokenizer.
/// - calcPr/@fullCalcOnLoad and @forceFullCalc are set when formulas exist.
/// - Unmodified ZIP parts are copied byte-for-byte with SHA-256 verification.
/// </summary>
internal sealed class WorkbookCleaner
{
    private static readonly XNamespace Ns = OoxmlNamespaces.SpreadsheetMl;

    private readonly OoxmlPackage _pkg;
    private readonly Policy _policy;
    private readonly EmoticonDetector _emoticonDetector;

    // Parts that have been modified (partPath → new XML bytes)
    private readonly Dictionary<string, byte[]> _modifiedParts =
        new(StringComparer.OrdinalIgnoreCase);

    // Staged worksheet documents (partPath → XDocument) — shared between SST and formula passes
    private readonly Dictionary<string, XDocument> _stagedWorksheets =
        new(StringComparer.OrdinalIgnoreCase);

    // Shared reverse index: SST index → referencing cells
    private SharedStringReferenceIndex _ssIndex = new();

    // COW map: (originalSstIdx, sheetPartPath, cellAddress) → new SST index
    private readonly Dictionary<(int OrigIdx, string SheetPath, string CellAddr), int> _cowIndexMap = new();

    public WorkbookCleaner(OoxmlPackage pkg, Policy policy)
    {
        _pkg    = pkg;
        _policy = policy;
        _emoticonDetector = new EmoticonDetector(
            policy.EmoticonProfile.Equals("markup", StringComparison.OrdinalIgnoreCase)
                ? EmoticonDetector.BoundaryProfile.Markup
                : EmoticonDetector.BoundaryProfile.Plain);
    }

    public CleanResult Clean(string outputPath)
    {
        var log = new List<string>();

        // Stage all worksheet documents first so SST and formula passes share them
        foreach (var sheet in _pkg.Sheets)
            if (_pkg.PartExists(sheet.PartPath))
                _stagedWorksheets[sheet.PartPath] = _pkg.LoadXml(sheet.PartPath);

        // Validate group authorization before any SST, formula, or cache mutation.
        FormulaScopeValidator.Validate(_pkg, _policy, _stagedWorksheets);

        // Defined-name literals are detection-only, including direct cleaner calls.
        var definedNameViolations = PreflightValidator.Validate(
            TextExtractor.ExtractAll(_pkg).Where(l => l.Source == TextSource.DefinedName).ToList(),
            _policy, _emoticonDetector);
        if (definedNameViolations.Count > 0)
            throw new InvalidDataException(definedNameViolations[0].Message);

        // Build shared reverse index from staged worksheets
        _ssIndex = SharedStringReferenceIndex.Build(_pkg, _stagedWorksheets);

        if (_pkg.SharedStringPartPath is not null && _pkg.PartExists(_pkg.SharedStringPartPath))
            ProcessSharedStrings(log);

        foreach (var sheet in _pkg.Sheets)
            ProcessWorksheet(sheet, log);

        // Flush staged worksheets that were modified
        foreach (var (partPath, doc) in _stagedWorksheets)
            if (_modifiedParts.ContainsKey(partPath))
                _modifiedParts[partPath] = SerialiseXml(doc);

        SetCalcPr(log);
        WriteOutput(outputPath, log);

        return new CleanResult(outputPath, log);
    }

    // ── Shared strings ───────────────────────────────────────────────────────

    private void ProcessSharedStrings(List<string> log)
    {
        var doc = _pkg.LoadXml(_pkg.SharedStringPartPath!);
        var root = doc.Root!;
        var sstEntries = root.Elements(Ns + "si").ToList();
        int originalCount = sstEntries.Count;

        if (int.TryParse(root.Attribute("uniqueCount")?.Value, out int declared)
            && declared != originalCount)
            log.Add($"[WARN] sst/@uniqueCount={declared} but actual count={originalCount}. Proceeding.");

        bool sstModified = false;
        var newEntries = new List<XElement>();

        for (int idx = 0; idx < sstEntries.Count; idx++)
        {
            var si = sstEntries[idx];
            var map = new RunOffsetMap(si);
            string logical = map.LogicalText;

            var refs = _ssIndex.GetRefs(idx);

            if (refs.Count == 0)
            {
                sstModified |= ProcessOrphanSharedString(si, idx, newlyOrphaned: false, log);
                continue;
            }

            // Compute desired edits per referencing cell (keyed by sheetPartPath + cellAddress)
            var editsByCell = new Dictionary<(string SheetPath, string CellAddr), List<TextEdit>>();
            foreach (var cellRef in refs)
            {
                var edits = BuildEdits(logical, cellRef.SheetPartPath, cellRef.SheetName, cellRef.CellAddress);
                editsByCell[(cellRef.SheetPartPath, cellRef.CellAddress)] = edits;
            }

            bool allSame = AllEditsIdentical(editsByCell.Values);

            if (allSame)
            {
                var edits = editsByCell.Values.First();
                if (edits.Count == 0) continue;

                string preview = PreviewEdits(logical, edits);
                map.ApplyEdits(si, edits);
                log.Add($"[CLEAN] SharedString[{idx}] in-place ({refs.Count} ref(s)): {Ellipsis(logical)} → {Ellipsis(preview)}");
                sstModified = true;
            }
            else
            {
                // Copy-on-write: different edits per cell
                foreach (var cellRef in refs)
                {
                    if (!editsByCell.TryGetValue((cellRef.SheetPartPath, cellRef.CellAddress), out var edits))
                        continue;
                    if (edits.Count == 0) continue;

                    var cloned = new XElement(si);
                    var clonedMap = new RunOffsetMap(cloned);
                    string preview = PreviewEdits(logical, edits);
                    clonedMap.ApplyEdits(cloned, edits);

                    int newIdx = originalCount + newEntries.Count;
                    newEntries.Add(cloned);
                    _cowIndexMap[(idx, cellRef.SheetPartPath, cellRef.CellAddress)] = newIdx;

                    log.Add($"[CLEAN-COW] SharedString[{idx}] → new[{newIdx}] for {cellRef.SheetName}!{cellRef.CellAddress}: {Ellipsis(logical)} → {Ellipsis(preview)}");
                    sstModified = true;
                }
            }
        }

        // Append new COW entries to SST
        foreach (var entry in newEntries)
            root.Add(entry);

        // Track actual redirections, not merely planned COW edits.
        var redirectedOriginals = new HashSet<int>();

        // Update worksheet <v> elements for COW cells
        if (_cowIndexMap.Count > 0)
        {
            foreach (var sheet in _pkg.Sheets)
            {
                if (!_stagedWorksheets.TryGetValue(sheet.PartPath, out var wsDoc)) continue;
                bool wsModified = false;
                foreach (var c in wsDoc.Descendants(Ns + "c"))
                {
                    if (c.Attribute("t")?.Value != "s") continue;
                    var vEl = c.Element(Ns + "v");
                    if (vEl is null || !int.TryParse(vEl.Value, out int oldIdx)) continue;
                    string? addr = c.Attribute("r")?.Value;
                    if (addr is null) continue;
                    if (_cowIndexMap.TryGetValue((oldIdx, sheet.PartPath, addr), out int newIdx))
                    {
                        vEl.Value = newIdx.ToString();
                        redirectedOriginals.Add(oldIdx);
                        wsModified = true;
                    }
                }
                if (wsModified)
                    _modifiedParts[sheet.PartPath] = Array.Empty<byte>(); // mark for re-serialisation
            }
        }

        // Only originals proven to have lost all references through this COW pass
        // may have unmapped forbidden content stripped. Never sanitize live originals.
        if (redirectedOriginals.Count > 0)
        {
            var remainingIndex = SharedStringReferenceIndex.Build(_pkg, _stagedWorksheets);
            foreach (int idx in redirectedOriginals.OrderBy(i => i))
                if (_ssIndex.GetRefs(idx).Count > 0 && remainingIndex.GetRefs(idx).Count == 0)
                    sstModified |= ProcessOrphanSharedString(sstEntries[idx], idx, newlyOrphaned: true, log);
        }

        if (sstModified)
        {
            root.SetAttributeValue("uniqueCount", originalCount + newEntries.Count);
            _modifiedParts[_pkg.SharedStringPartPath!] = SerialiseXml(doc);
        }
    }

    private bool ProcessOrphanSharedString(XElement si, int index, bool newlyOrphaned, List<string> log)
    {
        var map = new RunOffsetMap(si);
        string original = map.LogicalText;
        var edits = BuildOrphanEdits(original);
        string mapped = PreviewEdits(original, edits);

        // Existing input orphans are fail-closed, including direct cleaner calls.
        // Validate the result as replacements can themselves contain forbidden text.
        if (!newlyOrphaned && HasForbiddenContent(mapped))
            throw new InvalidDataException(
                $"SharedString[{index}] existing orphan contains forbidden content without an effective global or SST-part mapping.");

        bool modified = edits.Count > 0;
        if (modified) map.ApplyEdits(si, edits);

        if (newlyOrphaned)
        {
            // Rebuild offsets after mapping and after each stripping pass. Deletion
            // can expose another emoticon at a boundary, so scan until clean.
            while (true)
            {
                map = new RunOffsetMap(si);
                var removals = BuildForbiddenRemovalEdits(map.LogicalText);
                if (removals.Count == 0) break;
                map.ApplyEdits(si, removals);
                modified = true;
            }
        }

        if (modified)
        {
            string kind = newlyOrphaned ? "CLEAN-COW-ORPHAN" : "CLEAN-ORPHAN";
            log.Add($"[{kind}] SharedString[{index}]: {Ellipsis(original)} → {Ellipsis(new RunOffsetMap(si).LogicalText)}");
        }
        return modified;
    }

    private List<TextEdit> BuildForbiddenRemovalEdits(string text)
    {
        var candidates = new List<TextEdit>();
        foreach (var match in EmojiDetector.FindAll(text))
            candidates.Add(new TextEdit(match.CharStart, match.CharLength, ""));
        foreach (var match in _emoticonDetector.FindAll(text))
            candidates.Add(new TextEdit(match.CharStart, match.CharLength, ""));
        candidates.Sort((a, b) => a.Start.CompareTo(b.Start));

        // Merge overlapping detector spans so no forbidden suffix is left behind.
        var removals = new List<TextEdit>();
        foreach (var edit in candidates)
        {
            if (removals.Count == 0 || edit.Start > removals[^1].End)
                removals.Add(edit);
            else
            {
                var previous = removals[^1];
                removals[^1] = new TextEdit(previous.Start,
                    Math.Max(previous.End, edit.End) - previous.Start, "");
            }
        }
        return removals;
    }

    // ── Worksheets ───────────────────────────────────────────────────────────

    private void ProcessWorksheet(SheetInfo sheet, List<string> log)
    {
        if (!_stagedWorksheets.TryGetValue(sheet.PartPath, out var doc)) return;
        bool modified = false;

        foreach (var c in doc.Descendants(Ns + "c"))
        {
            string? t    = c.Attribute("t")?.Value;
            string? addr = c.Attribute("r")?.Value;

            // Inline strings
            if (t == "inlineStr")
            {
                var isEl = c.Element(Ns + "is");
                if (isEl is null) continue;
                var map = new RunOffsetMap(isEl);
                string logical = map.LogicalText;
                var edits = BuildEdits(logical, sheet.PartPath, sheet.Name, addr);
                if (edits.Count > 0)
                {
                    string preview = PreviewEdits(logical, edits);
                    map.ApplyEdits(isEl, edits);
                    log.Add($"[CLEAN] {sheet.Name}!{addr} inline: {Ellipsis(logical)} → {Ellipsis(preview)}");
                    modified = true;
                }
            }

            // Formula string literals via FormulaTokenizer
            var fEl = c.Element(Ns + "f");
            if (fEl is not null)
            {
                string formula = fEl.Value;
                string cleanedFormula = CleanFormulaLiterals(formula, log, sheet.Name, addr, sheet.PartPath);
                if (cleanedFormula != formula)
                {
                    fEl.Value = cleanedFormula;
                    modified = true;
                }

                // Invalidate cached formula value
                var vEl = c.Element(Ns + "v");
                if (vEl is not null)
                {
                    vEl.Remove();
                    log.Add($"[CACHE-INVALIDATE] {sheet.Name}!{addr} cached value removed.");
                    modified = true;
                }
            }
        }

        if (modified)
            _modifiedParts[sheet.PartPath] = Array.Empty<byte>(); // mark for re-serialisation
    }

    // ── Formula literal cleaning ─────────────────────────────────────────────

    private string CleanFormulaLiterals(string formula, List<string> log,
        string? sheetName, string? cell, string? sheetPartPath)
    {
        if (string.IsNullOrEmpty(formula)) return formula;

        // Let InvalidDataException propagate — invalid formulas must not be silently skipped
        var tokens = FormulaTokenizer.ExtractLiterals(formula);
        if (tokens.Count == 0) return formula;

        var editsPerToken = new Dictionary<int, string>();
        for (int t = 0; t < tokens.Count; t++)
        {
            string decoded = tokens[t].DecodedText;
            var edits = BuildEdits(decoded, sheetPartPath, sheetName, cell);
            if (edits.Count > 0)
            {
                string cleaned = PreviewEdits(decoded, edits);
                editsPerToken[t] = cleaned;
                log.Add($"[CLEAN-FORMULA] {sheetName}!{cell} literal[{t}]: {Ellipsis(decoded)} → {Ellipsis(cleaned)}");
            }
        }

        if (editsPerToken.Count == 0) return formula;
        return FormulaTokenizer.RebuildWithEdits(formula, tokens, editsPerToken);
    }

    // ── Edit building ────────────────────────────────────────────────────────

    private List<TextEdit> BuildEdits(string text,
        string? sheetPartPath, string? sheetName, string? cellAddress)
    {
        if (string.IsNullOrEmpty(text)) return new();
        if (_policy.EmojiMappings.Count == 0 && _policy.EmoticonMappings.Count == 0)
            return new();

        var candidates = new List<TextEdit>();

        foreach (var mapping in _policy.EmojiMappings)
        {
            if (!MappingScopeMatcher.Matches(mapping, sheetPartPath, sheetName, cellAddress))
                continue;
            int pos = 0;
            while (true)
            {
                int idx = text.IndexOf(mapping.Sequence, pos, StringComparison.Ordinal);
                if (idx < 0) break;
                candidates.Add(new TextEdit(idx, mapping.Sequence.Length, mapping.Replacement));
                pos = idx + mapping.Sequence.Length;
            }
        }

        if (_policy.EmoticonMappings.Count > 0)
        {
            var emoticonLookup = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var mapping in _policy.EmoticonMappings)
                if (MappingScopeMatcher.Matches(mapping, sheetPartPath, sheetName, cellAddress))
                    emoticonLookup[mapping.Sequence] = mapping.Replacement;

            if (emoticonLookup.Count > 0)
            {
                var matches = _emoticonDetector.FindAll(text);
                foreach (var m in matches)
                    if (emoticonLookup.TryGetValue(m.Value, out string? rep))
                        candidates.Add(new TextEdit(m.CharStart, m.CharLength, rep));
            }
        }

        if (candidates.Count == 0) return new();

        candidates.Sort((a, b) => a.Start != b.Start
            ? a.Start.CompareTo(b.Start)
            : b.Length.CompareTo(a.Length));

        var result = new List<TextEdit>();
        int lastEnd = 0;
        foreach (var e in candidates)
            if (e.Start >= lastEnd) { result.Add(e); lastEnd = e.End; }
        return result;
    }

    /// <summary>
    /// Build edits for an orphan (uncoupled) SST entry using only global or
    /// SST-part-scoped mappings — never cell- or sheet-scoped mappings.
    /// This matches the orphan-scope rules used by PreflightValidator.
    /// </summary>
    private List<TextEdit> BuildOrphanEdits(string text)
    {
        if (string.IsNullOrEmpty(text)) return new();

        var candidates = new List<TextEdit>();

        foreach (var mapping in _policy.EmojiMappings)
        {
            // Only global or SST-part-scoped mappings apply to orphan entries
            if (!MappingScopeMatcher.IsGlobal(mapping) && !MappingScopeMatcher.IsSstPartScope(mapping, _pkg.SharedStringPartPath))
                continue;

            int pos = 0;
            while (true)
            {
                int idx = text.IndexOf(mapping.Sequence, pos, StringComparison.Ordinal);
                if (idx < 0) break;
                candidates.Add(new TextEdit(idx, mapping.Sequence.Length, mapping.Replacement));
                pos = idx + mapping.Sequence.Length;
            }
        }

        if (_policy.EmoticonMappings.Count > 0)
        {
            var emoticonLookup = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var mapping in _policy.EmoticonMappings)
                if (MappingScopeMatcher.IsGlobal(mapping) || MappingScopeMatcher.IsSstPartScope(mapping, _pkg.SharedStringPartPath))
                    emoticonLookup[mapping.Sequence] = mapping.Replacement;

            if (emoticonLookup.Count > 0)
            {
                var matches = _emoticonDetector.FindAll(text);
                foreach (var m in matches)
                    if (emoticonLookup.TryGetValue(m.Value, out string? rep))
                        candidates.Add(new TextEdit(m.CharStart, m.CharLength, rep));
            }
        }

        if (candidates.Count == 0) return new();

        candidates.Sort((a, b) => a.Start != b.Start
            ? a.Start.CompareTo(b.Start)
            : b.Length.CompareTo(a.Length));

        var result = new List<TextEdit>();
        int lastEnd = 0;
        foreach (var e in candidates)
            if (e.Start >= lastEnd) { result.Add(e); lastEnd = e.End; }
        return result;
    }

    private bool HasForbiddenContent(string text)
        => EmojiDetector.FindAll(text).Count > 0
        || _emoticonDetector.FindAll(text).Count > 0;

    private static string PreviewEdits(string text, List<TextEdit> edits)
    {
        var el = new TextEditList();
        foreach (var e in edits) el.Add(text, e.Start, e.Length, e.Replacement);
        return el.Apply(text);
    }

    private static bool AllEditsIdentical(IEnumerable<List<TextEdit>> editCollections)
    {
        List<TextEdit>? reference = null;
        foreach (var edits in editCollections)
        {
            if (reference is null) { reference = edits; continue; }
            if (edits.Count != reference.Count) return false;
            for (int i = 0; i < edits.Count; i++)
                if (edits[i] != reference[i]) return false;
        }
        return true;
    }

    // ── calcPr ───────────────────────────────────────────────────────────────

    private void SetCalcPr(List<string> log)
    {
        if (_pkg.WorkbookPartPath is null) return;
        var doc = _pkg.LoadXml(_pkg.WorkbookPartPath);
        var root = doc.Root;
        if (root is null) return;

        bool hasFormulas = _stagedWorksheets.Values.Any(d => d.Descendants(Ns + "f").Any());
        if (!hasFormulas) return;

        var calcPrList = root.Elements(Ns + "calcPr").ToList();
        if (calcPrList.Count > 1)
            throw new InvalidDataException("Multiple <calcPr> elements in workbook.xml.");

        if (calcPrList.Count == 1)
        {
            calcPrList[0].SetAttributeValue("fullCalcOnLoad", "1");
            calcPrList[0].SetAttributeValue("forceFullCalc", "1");
            log.Add("[CALC] Updated existing calcPr.");
        }
        else
        {
            var calcPr = new XElement(Ns + "calcPr",
                new XAttribute("fullCalcOnLoad", "1"),
                new XAttribute("forceFullCalc", "1"));
            InsertCalcPrAtSchemaPosition(root, calcPr, log);
        }

        _modifiedParts[_pkg.WorkbookPartPath] = SerialiseXml(doc);
    }

    private static readonly string[] WorkbookChildOrder =
    {
        "fileVersion", "fileSharing", "workbookPr", "workbookProtection",
        "bookViews", "sheets", "functionGroups", "externalReferences",
        "definedNames", "calcPr", "oleSize", "customWorkbookViews",
        "pivotCaches", "smartTagPr", "smartTagTypes", "webPublishing",
        "fileRecoveryPr", "webPublishObjects", "extLst"
    };

    private static void InsertCalcPrAtSchemaPosition(XElement root, XElement calcPr, List<string> log)
    {
        int rank = Array.IndexOf(WorkbookChildOrder, "calcPr");
        XElement? insertAfter = null;
        for (int r = rank - 1; r >= 0; r--)
        {
            var el = root.Element(Ns + WorkbookChildOrder[r]);
            if (el is not null) { insertAfter = el; break; }
        }
        XElement? insertBefore = null;
        for (int r = rank + 1; r < WorkbookChildOrder.Length; r++)
        {
            var el = root.Element(Ns + WorkbookChildOrder[r]);
            if (el is not null) { insertBefore = el; break; }
        }

        if (insertAfter is not null) insertAfter.AddAfterSelf(calcPr);
        else if (insertBefore is not null) insertBefore.AddBeforeSelf(calcPr);
        else root.Add(calcPr);

        log.Add("[CALC] Inserted <calcPr fullCalcOnLoad='1' forceFullCalc='1'/>.");
    }

    // ── ZIP output ───────────────────────────────────────────────────────────

    private void WriteOutput(string outputPath, List<string> log)
    {
        // Re-serialise staged worksheets marked with empty bytes
        foreach (var (partPath, bytes) in _modifiedParts.ToList())
        {
            if (bytes.Length == 0 && _stagedWorksheets.TryGetValue(partPath, out var wsDoc))
                _modifiedParts[partPath] = SerialiseXml(wsDoc);
        }

        var originalHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in _pkg.AllEntryNames())
        {
            if (!_modifiedParts.ContainsKey(name))
            {
                try { originalHashes[name] = _pkg.PartSha256(name); } catch { }
            }
        }

        using var outStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var outZip = new ZipArchive(outStream, ZipArchiveMode.Create, leaveOpen: false);

        foreach (var entryName in _pkg.AllEntryNames())
        {
            var outEntry = outZip.CreateEntry(entryName, CompressionLevel.Optimal);
            using var outEntryStream = outEntry.Open();

            if (_modifiedParts.TryGetValue(entryName, out byte[]? modBytes) && modBytes.Length > 0)
            {
                outEntryStream.Write(modBytes);
                log.Add($"[WRITE] Modified: {entryName} ({modBytes.Length} bytes)");
            }
            else
            {
                byte[] original = _pkg.GetPartBytes(entryName);
                outEntryStream.Write(original);

                string writtenHash = Convert.ToHexString(SHA256.HashData(original)).ToLowerInvariant();
                if (originalHashes.TryGetValue(entryName, out string? expected) && writtenHash != expected)
                    log.Add($"[ERROR] SHA-256 mismatch for unmodified part: {entryName}");
            }
        }

        log.Add($"[DONE] Written to: {outputPath}");
    }

    // ── Serialisation ────────────────────────────────────────────────────────

    private static byte[] SerialiseXml(XDocument doc)
    {
        using var ms = new MemoryStream();
        using (var writer = XmlWriter.Create(ms, XmlSettings.ForOoxmlPart()))
            doc.Save(writer);
        return ms.ToArray();
    }

    private static string Ellipsis(string s, int max = 60)
        => s.Length <= max ? s : s[..max] + "…";
}

internal sealed record CleanResult(string OutputPath, List<string> Log);

/// <summary>Reference to a cell that uses a shared string.</summary>
internal sealed record CellRef(string SheetPartPath, string SheetName, string CellAddress);