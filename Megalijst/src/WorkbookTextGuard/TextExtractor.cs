using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WorkbookTextGuard;

/// <summary>
/// Extracts all logical text strings from an OOXML workbook package,
/// including shared strings (with rich-text run reconstruction),
/// inline strings, worksheet names, headers/footers, comments, VML text,
/// DrawingML text, and formula string literals / cached values.
/// </summary>
internal static class TextExtractor
{
    private static readonly XNamespace Ns  = OoxmlNamespaces.SpreadsheetMl;
    private static readonly XNamespace Vns = OoxmlNamespaces.Vml;

    /// <summary>Extract all text locations from the package.</summary>
    public static List<TextLocation> ExtractAll(OoxmlPackage pkg)
    {
        var results = new List<TextLocation>();

        // 1. Shared strings
        if (pkg.SharedStringPartPath is not null && pkg.PartExists(pkg.SharedStringPartPath))
            results.AddRange(ExtractSharedStrings(pkg));

        // 2. Worksheet cells (inline strings, formula string literals, cached values)
        foreach (var sheet in pkg.Sheets)
            results.AddRange(ExtractWorksheet(pkg, sheet));

        // 3. Workbook-level: defined names, sheet names
        if (pkg.WorkbookPartPath is not null)
            results.AddRange(ExtractWorkbookLevel(pkg));

        // 4. Comments
        foreach (var commentPath in pkg.CommentPartPaths)
            if (pkg.PartExists(commentPath))
                results.AddRange(ExtractComments(pkg, commentPath));

        // 5. VML drawings (legacy text boxes, comment shapes)
        foreach (var vmlPath in pkg.VmlDrawingPartPaths)
            if (pkg.PartExists(vmlPath))
                results.AddRange(ExtractVml(pkg, vmlPath));

        // 6. DrawingML (modern shapes, text boxes)
        foreach (var drawingPath in pkg.DrawingPartPaths)
            if (pkg.PartExists(drawingPath))
                results.AddRange(ExtractDrawingMl(pkg, drawingPath));

        return results;
    }

    // ── Shared strings ───────────────────────────────────────────────────────

    public static List<SharedStringEntry> ExtractSharedStringEntries(OoxmlPackage pkg)
    {
        var entries = new List<SharedStringEntry>();
        if (pkg.SharedStringPartPath is null || !pkg.PartExists(pkg.SharedStringPartPath))
            return entries;

        var doc = pkg.LoadXml(pkg.SharedStringPartPath);
        var sis = doc.Root?.Elements(Ns + "si").ToList() ?? new();

        for (int idx = 0; idx < sis.Count; idx++)
        {
            string logical = GetLogicalText(sis[idx]);
            entries.Add(new SharedStringEntry(idx, logical, sis[idx]));
        }
        return entries;
    }

    private static IEnumerable<TextLocation> ExtractSharedStrings(OoxmlPackage pkg)
    {
        var entries = ExtractSharedStringEntries(pkg);
        foreach (var e in entries)
            yield return new TextLocation(
                Source: TextSource.SharedString,
                PartPath: pkg.SharedStringPartPath!,
                SheetName: null,
                CellAddress: null,
                SharedStringIndex: e.Index,
                LogicalText: e.LogicalText,
                IsFormula: false);
    }

    // ── Worksheet ────────────────────────────────────────────────────────────

    private static IEnumerable<TextLocation> ExtractWorksheet(OoxmlPackage pkg, SheetInfo sheet)
    {
        if (!pkg.PartExists(sheet.PartPath)) yield break;

        var doc = pkg.LoadXml(sheet.PartPath);

        // Header/footer
        foreach (var hf in doc.Descendants(Ns + "oddHeader")
            .Concat(doc.Descendants(Ns + "oddFooter"))
            .Concat(doc.Descendants(Ns + "evenHeader"))
            .Concat(doc.Descendants(Ns + "evenFooter"))
            .Concat(doc.Descendants(Ns + "firstHeader"))
            .Concat(doc.Descendants(Ns + "firstFooter")))
        {
            string text = hf.Value;
            if (!string.IsNullOrEmpty(text))
                yield return new TextLocation(
                    Source: TextSource.HeaderFooter,
                    PartPath: sheet.PartPath,
                    SheetName: sheet.Name,
                    CellAddress: null,
                    SharedStringIndex: null,
                    LogicalText: text,
                    IsFormula: false);
        }

        // Cells
        foreach (var row in doc.Descendants(Ns + "row"))
        foreach (var c in row.Elements(Ns + "c"))
        {
            string? addr = c.Attribute("r")?.Value;
            string? t    = c.Attribute("t")?.Value;

            // Inline string
            if (t == "inlineStr")
            {
                var isEl = c.Element(Ns + "is");
                if (isEl is not null)
                {
                    string text = GetLogicalText(isEl);
                    yield return new TextLocation(
                        Source: TextSource.InlineString,
                        PartPath: sheet.PartPath,
                        SheetName: sheet.Name,
                        CellAddress: addr,
                        SharedStringIndex: null,
                        LogicalText: text,
                        IsFormula: false);
                }
            }

            // Formula: extract string literals via FormulaTokenizer.
            // InvalidDataException propagates — invalid formulas must not be silently skipped.
            var fEl = c.Element(Ns + "f");
            if (fEl is not null)
            {
                string formulaText = fEl.Value;
                var tokens = FormulaTokenizer.ExtractLiterals(formulaText);

                foreach (var token in tokens)
                    yield return new TextLocation(
                        Source: TextSource.FormulaLiteral,
                        PartPath: sheet.PartPath,
                        SheetName: sheet.Name,
                        CellAddress: addr,
                        SharedStringIndex: null,
                        LogicalText: token.DecodedText,
                        IsFormula: true);

                // Cached formula value (string type t="str") — extracted exactly once
                if (t == "str")
                {
                    var vEl = c.Element(Ns + "v");
                    if (vEl is not null && !string.IsNullOrEmpty(vEl.Value))
                        yield return new TextLocation(
                            Source: TextSource.FormulaCachedValue,
                            PartPath: sheet.PartPath,
                            SheetName: sheet.Name,
                            CellAddress: addr,
                            SharedStringIndex: null,
                            LogicalText: vEl.Value,
                            IsFormula: false);
                }
            }
            else if (t == "str")
            {
                // Array follow-cell: t="str" without <f> — cached string value
                var vEl = c.Element(Ns + "v");
                if (vEl is not null && !string.IsNullOrEmpty(vEl.Value))
                    yield return new TextLocation(
                        Source: TextSource.FormulaCachedValue,
                        PartPath: sheet.PartPath,
                        SheetName: sheet.Name,
                        CellAddress: addr,
                        SharedStringIndex: null,
                        LogicalText: vEl.Value,
                        IsFormula: false);
            }
        }
    }

    // ── Workbook level ───────────────────────────────────────────────────────

    private static IEnumerable<TextLocation> ExtractWorkbookLevel(OoxmlPackage pkg)
    {
        var doc = pkg.LoadXml(pkg.WorkbookPartPath!);

        // Sheet names
        foreach (var sheet in pkg.Sheets)
            yield return new TextLocation(
                Source: TextSource.SheetName,
                PartPath: pkg.WorkbookPartPath!,
                SheetName: sheet.Name,
                CellAddress: null,
                SharedStringIndex: null,
                LogicalText: sheet.Name,
                IsFormula: false);

        // Defined names — extract string literals via FormulaTokenizer.
        // InvalidDataException propagates — invalid formulas must not be silently skipped.
        XNamespace ns = OoxmlNamespaces.SpreadsheetMl;
        foreach (var dn in doc.Descendants(ns + "definedName"))
        {
            string? name = dn.Attribute("name")?.Value;
            string val   = dn.Value;
            if (string.IsNullOrEmpty(val)) continue;

            var tokens = FormulaTokenizer.ExtractLiterals(val);
            foreach (var token in tokens)
                yield return new TextLocation(
                    Source: TextSource.DefinedName,
                    PartPath: pkg.WorkbookPartPath!,
                    SheetName: null,
                    CellAddress: name,
                    SharedStringIndex: null,
                    LogicalText: token.DecodedText,
                    IsFormula: true);
        }
    }

    // ── Comments ─────────────────────────────────────────────────────────────

    private static IEnumerable<TextLocation> ExtractComments(OoxmlPackage pkg, string partPath)
    {
        var doc = pkg.LoadXml(partPath);
        XNamespace ns = OoxmlNamespaces.SpreadsheetMl;

        foreach (var comment in doc.Descendants(ns + "comment"))
        {
            string? addr = comment.Attribute("ref")?.Value;
            var textEl = comment.Element(ns + "text");
            if (textEl is null) continue;
            string text = GetLogicalText(textEl);
            yield return new TextLocation(
                Source: TextSource.Comment,
                PartPath: partPath,
                SheetName: null,
                CellAddress: addr,
                SharedStringIndex: null,
                LogicalText: text,
                IsFormula: false);
        }
    }

    // ── VML drawings ─────────────────────────────────────────────────────────

    private static IEnumerable<TextLocation> ExtractVml(OoxmlPackage pkg, string partPath)
    {
        var doc = pkg.LoadXml(partPath);
        foreach (var tb in doc.Descendants(Vns + "textbox"))
        {
            string text = tb.Value;
            if (!string.IsNullOrEmpty(text))
                yield return new TextLocation(
                    Source: TextSource.VmlText,
                    PartPath: partPath,
                    SheetName: null,
                    CellAddress: null,
                    SharedStringIndex: null,
                    LogicalText: text,
                    IsFormula: false);
        }
    }

    // ── DrawingML ────────────────────────────────────────────────────────────

    private static IEnumerable<TextLocation> ExtractDrawingMl(OoxmlPackage pkg, string partPath)
    {
        if (!pkg.PartExists(partPath)) yield break;
        var doc = pkg.LoadXml(partPath);

        XNamespace dml = "http://schemas.openxmlformats.org/drawingml/2006/main";
        XNamespace xdr = "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing";

        // Iterate over shape containers — never merge text across shapes
        foreach (var sp in doc.Descendants(xdr + "sp"))
        {
            // Within each shape, iterate over paragraphs — never merge across paragraphs
            foreach (var para in sp.Descendants(dml + "p"))
            {
                var sb = new System.Text.StringBuilder();
                foreach (var r in para.Elements(dml + "r"))
                {
                    var t = r.Element(dml + "t");
                    if (t is not null) sb.Append(t.Value);
                }
                string text = sb.ToString();
                if (!string.IsNullOrEmpty(text))
                    yield return new TextLocation(
                        Source: TextSource.DrawingMlText,
                        PartPath: partPath,
                        SheetName: null,
                        CellAddress: null,
                        SharedStringIndex: null,
                        LogicalText: text,
                        IsFormula: false);
            }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Reconstruct logical text from an &lt;si&gt; or &lt;is&gt; element,
    /// concatenating all &lt;t&gt; values across rich-text runs.
    /// </summary>
    public static string GetLogicalText(XElement siOrIs)
    {
        var plainT = siOrIs.Element(Ns + "t");
        if (plainT is not null && !siOrIs.Elements(Ns + "r").Any())
            return plainT.Value;

        var sb = new System.Text.StringBuilder();
        foreach (var r in siOrIs.Elements(Ns + "r"))
        {
            var t = r.Element(Ns + "t");
            if (t is not null) sb.Append(t.Value);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Legacy helper: extract string literals from a formula string using the naive parser.
    /// Prefer FormulaTokenizer.ExtractLiterals for new code.
    /// </summary>
    public static List<string> ExtractFormulaStringLiterals(string formula)
    {
        var results = new List<string>();
        if (string.IsNullOrEmpty(formula)) return results;

        int i = 0;
        while (i < formula.Length)
        {
            if (formula[i] == '"')
            {
                i++;
                var sb = new System.Text.StringBuilder();
                while (i < formula.Length)
                {
                    if (formula[i] == '"')
                    {
                        if (i + 1 < formula.Length && formula[i + 1] == '"')
                        { sb.Append('"'); i += 2; }
                        else { i++; break; }
                    }
                    else { sb.Append(formula[i]); i++; }
                }
                results.Add(sb.ToString());
            }
            else { i++; }
        }
        return results;
    }
}

internal sealed record TextLocation(
    TextSource Source,
    string PartPath,
    string? SheetName,
    string? CellAddress,
    int? SharedStringIndex,
    string LogicalText,
    bool IsFormula);

internal sealed record SharedStringEntry(int Index, string LogicalText, XElement Element);

internal enum TextSource
{
    SharedString,
    InlineString,
    FormulaLiteral,
    FormulaCachedValue,
    SheetName,
    DefinedName,
    HeaderFooter,
    Comment,
    VmlText,
    DrawingMlText,
    Other
}