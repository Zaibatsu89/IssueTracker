using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace WorkbookTextGuard;

/// <summary>
/// Builds and exposes a reverse index from SST entry index to all referencing cells,
/// with full worksheet context (part path, sheet name, cell address).
///
/// Used by both WorkbookCleaner and PreflightValidator to ensure consistent
/// scope resolution for shared strings.
/// </summary>
internal sealed class SharedStringReferenceIndex
{
    private static readonly XNamespace Ns = OoxmlNamespaces.SpreadsheetMl;

    // SST index → list of referencing cells
    private readonly Dictionary<int, List<CellRef>> _index = new();

    public IReadOnlyDictionary<int, List<CellRef>> Index => _index;

    /// <summary>Build the reverse index from all staged worksheet documents.</summary>
    public static SharedStringReferenceIndex Build(
        OoxmlPackage pkg,
        IReadOnlyDictionary<string, XDocument> stagedWorksheets)
    {
        var result = new SharedStringReferenceIndex();

        foreach (var sheet in pkg.Sheets)
        {
            if (!stagedWorksheets.TryGetValue(sheet.PartPath, out var doc)) continue;

            foreach (var c in doc.Descendants(Ns + "c"))
            {
                if (c.Attribute("t")?.Value != "s") continue;
                var vEl = c.Element(Ns + "v");
                if (vEl is null || !int.TryParse(vEl.Value, out int idx)) continue;

                var addr = c.Attribute("r")?.Value;
                if (addr is null)
                    throw new InvalidDataException(
                        $"Celadres (attribuut 'r') ontbreekt in werkblad '{sheet.Name}' ({sheet.PartPath}) voor gedeelde tekenreeks met index {idx}.");

                if (!result._index.TryGetValue(idx, out var list))
                    result._index[idx] = list = new List<CellRef>();

                list.Add(new CellRef(sheet.PartPath, sheet.Name, addr));
            }
        }

        return result;
    }

    /// <summary>
    /// Returns all cells referencing the given SST index, or an empty list.
    /// </summary>
    public IReadOnlyList<CellRef> GetRefs(int sstIndex)
        => _index.TryGetValue(sstIndex, out var list) ? list : Array.Empty<CellRef>();

    /// <summary>
    /// Returns true if the given SST index has at least one referencing cell.
    /// </summary>
    public bool IsReferenced(int sstIndex) => _index.ContainsKey(sstIndex);
}