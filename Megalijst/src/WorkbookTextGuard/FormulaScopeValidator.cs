using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace WorkbookTextGuard;

/// <summary>
/// Proves authorization for shared-formula literal edits without expanding formulas.
/// Only groups whose formula text would be edited require metadata validation.
/// </summary>
internal static class FormulaScopeValidator
{
    private static readonly XNamespace Ns = OoxmlNamespaces.SpreadsheetMl;

    public static void Validate(OoxmlPackage pkg, Policy policy,
        IReadOnlyDictionary<string, XDocument> worksheets)
    {
        var detector = new EmoticonDetector(
            policy.EmoticonProfile.Equals("markup", StringComparison.OrdinalIgnoreCase)
                ? EmoticonDetector.BoundaryProfile.Markup
                : EmoticonDetector.BoundaryProfile.Plain);

        foreach (var sheet in pkg.Sheets)
        {
            if (!worksheets.TryGetValue(sheet.PartPath, out var document)) continue;
            var cells = document.Descendants(Ns + "c").ToList();
            var formulas = cells.SelectMany(c => c.Elements(Ns + "f")).ToList();
            foreach (var formula in formulas)
            {
                string? address = formula.Parent?.Attribute("r")?.Value;
                var mappings = SelectedMappings(formula.Value, policy, detector, sheet, address);
                if (mappings.Count == 0) continue;

                string type = formula.Attribute("t")?.Value ?? "normal";
                if (type == "array")
                    throw Error(sheet, address, "Array formula literal edits require manual correction.");
                if (type == "normal" && formula.Attribute("si") is null && formula.Attribute("ref") is null)
                    continue;
                if (type != "shared")
                    throw Error(sheet, address, "Unsupported formula type or group metadata.");

                if (!TrySharedIndex(formula, out uint index))
                    throw Error(sheet, address, "Shared formula requires a valid si.");
                var group = formulas.Where(f => f.Attribute("t")?.Value == "shared"
                    && TrySharedIndex(f, out uint other) && other == index).ToList();
                var masters = group.Where(f => !string.IsNullOrWhiteSpace(f.Value)).ToList();
                if (masters.Count != 1 || masters[0] != formula)
                    throw Error(sheet, address, "Shared group requires exactly one master.");
                if (!TryRange(formula.Attribute("ref")?.Value, out var range)
                    || !TryAddress(address, out var master) || !range.Contains(master))
                    throw Error(sheet, address, "Shared master requires a valid cell address and rectangular ref.");

                var occupied = new HashSet<CellAddress>();
                foreach (var member in group)
                {
                    if (member.Parent is null || member.Parent.Elements(Ns + "f").Count() != 1
                        || !TryAddress(member.Parent.Attribute("r")?.Value, out var memberAddress)
                        || !range.Contains(memberAddress) || !occupied.Add(memberAddress)
                        || (member != formula && (member.Attribute("ref") is not null || member.Value.Length != 0)))
                        throw Error(sheet, address, "Invalid or conflicting shared-group member metadata.");
                }
                // Compare counts, not an expanded range (a ref can cover a whole worksheet).
                if (occupied.Count != range.CellCount)
                    throw Error(sheet, address, "Shared ref is not completely represented by its group.");
                var seenCells = new HashSet<CellAddress>();
                var members = group.ToHashSet();
                foreach (var cell in cells)
                    if (TryAddress(cell.Attribute("r")?.Value, out var cellAddress)
                        && range.Contains(cellAddress)
                        && (!members.Contains(cell.Element(Ns + "f")!) || !seenCells.Add(cellAddress)))
                        throw Error(sheet, address, "Conflicting cells within shared ref.");
                foreach (var other in formulas)
                    if (!members.Contains(other) && TryRange(other.Attribute("ref")?.Value, out var otherRange)
                        && range.First.Column <= otherRange.Last.Column && range.Last.Column >= otherRange.First.Column
                        && range.First.Row <= otherRange.Last.Row && range.Last.Row >= otherRange.First.Row)
                        throw Error(sheet, address, "Overlapping formula refs.");

                foreach (var mapping in mappings)
                {
                    // Matching the master already proves part/sheet fields. A cell field
                    // can authorize only a singleton, never the rest of a shared range.
                    // Do not substitute another mapping with the same replacement text.
                    if (!MappingScopeMatcher.Matches(mapping, sheet.PartPath, sheet.Name, address)
                        || (mapping.Scope?.Cell is not null && range.CellCount != 1))
                        throw Error(sheet, address,
                            "The same approved mapping must cover the entire shared ref; cell-only scope conflicts.");
                }
            }
        }
    }

    // Mirror cleaner candidate selection, retaining the actual mapping identity.
    private static List<PolicyMapping> SelectedMappings(string formula, Policy policy,
        EmoticonDetector detector, SheetInfo sheet, string? address)
    {
        var selected = new List<PolicyMapping>();
        foreach (var token in FormulaTokenizer.ExtractLiterals(formula))
        {
            string text = token.DecodedText;
            var candidates = new List<(int Start, int Length, PolicyMapping Mapping)>();
            foreach (var mapping in policy.EmojiMappings)
            {
                if (!MappingScopeMatcher.Matches(mapping, sheet.PartPath, sheet.Name, address)) continue;
                if (mapping.Sequence.Length == 0) throw Error(sheet, address, "Empty mapping sequence.");
                int position = 0;
                while (true)
                {
                    int start = text.IndexOf(mapping.Sequence, position, StringComparison.Ordinal);
                    if (start < 0) break;
                    candidates.Add((start, mapping.Sequence.Length, mapping));
                    position = start + mapping.Sequence.Length;
                }
            }
            var emoticons = new Dictionary<string, PolicyMapping>(StringComparer.Ordinal);
            foreach (var mapping in policy.EmoticonMappings)
                if (MappingScopeMatcher.Matches(mapping, sheet.PartPath, sheet.Name, address))
                    emoticons[mapping.Sequence] = mapping;
            foreach (var match in detector.FindAll(text))
                if (emoticons.TryGetValue(match.Value, out var mapping))
                    candidates.Add((match.CharStart, match.CharLength, mapping));
            candidates.Sort((a, b) => a.Start != b.Start
                ? a.Start.CompareTo(b.Start) : b.Length.CompareTo(a.Length));
            int lastEnd = 0;
            foreach (var candidate in candidates)
                if (candidate.Start >= lastEnd)
                {
                    selected.Add(candidate.Mapping);
                    lastEnd = candidate.Start + candidate.Length;
                }
        }
        return selected;
    }

    private static bool TrySharedIndex(XElement formula, out uint index)
        => uint.TryParse(formula.Attribute("si")?.Value, NumberStyles.None,
            CultureInfo.InvariantCulture, out index);

    private static bool TryAddress(string? value, out CellAddress address)
    {
        address = default;
        if (string.IsNullOrEmpty(value)) return false;
        int i = 0, column = 0;
        while (i < value.Length && char.IsAsciiLetter(value[i]))
        {
            if (i == 3) return false;
            column = column * 26 + char.ToUpperInvariant(value[i++]) - 'A' + 1;
        }
        if (column is < 1 or > 16384 || i == value.Length || value[i] == '0') return false;
        if (!int.TryParse(value.AsSpan(i), NumberStyles.None, CultureInfo.InvariantCulture, out int row)
            || row is < 1 or > 1048576) return false;
        address = new CellAddress(column, row);
        return true;
    }

    private static bool TryRange(string? value, out CellRange range)
    {
        range = default;
        if (value is null) return false;
        string[] ends = value.Split(':');
        if (ends.Length is < 1 or > 2 || !TryAddress(ends[0], out var first)
            || !TryAddress(ends[^1], out var last)
            || first.Column > last.Column || first.Row > last.Row) return false;
        range = new CellRange(first, last);
        return true;
    }

    private static InvalidDataException Error(SheetInfo sheet, string? address, string message)
        => new($"{sheet.Name}!{address}: {message}");

    private readonly record struct CellAddress(int Column, int Row);
    private readonly record struct CellRange(CellAddress First, CellAddress Last)
    {
        public long CellCount => (long)(Last.Column - First.Column + 1) * (Last.Row - First.Row + 1);
        public bool Contains(CellAddress address) => address.Column >= First.Column
            && address.Column <= Last.Column && address.Row >= First.Row && address.Row <= Last.Row;
    }
}
