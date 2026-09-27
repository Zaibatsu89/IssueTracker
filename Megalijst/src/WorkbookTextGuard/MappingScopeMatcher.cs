namespace WorkbookTextGuard;

/// <summary>
/// Evaluates whether a <see cref="PolicyMapping"/> scope matches a given cell context.
///
/// Scope semantics (conjunctive — ALL specified fields must match):
/// - <c>part</c>: the worksheet part path (e.g. "xl/worksheets/sheet1.xml").
///   For inline and shared-string cells, this is always the WORKSHEET part path,
///   never "xl/sharedStrings.xml". SST-part scope is only valid for uncoupled
///   entries or SST-file-level checks.
/// - <c>sheet</c>: the visible worksheet name (e.g. "Sheet1").
/// - <c>cell</c>: the cell address of the referencing cell (e.g. "A1").
///
/// Missing scope fields do not restrict matching (they match any value).
/// A null or absent scope matches everything (global mapping).
/// An absent location field (null) can never satisfy a specified scope field.
/// </summary>
internal static class MappingScopeMatcher
{
    /// <summary>
    /// Returns true if <paramref name="mapping"/> applies to the given cell context.
    /// </summary>
    public static bool Matches(
        PolicyMapping mapping,
        string? sheetPartPath,
        string? sheetName,
        string? cellAddress)
    {
        var scope = mapping.Scope;
        if (scope is null) return true; // global mapping

        // part field: must match worksheet part path
        if (scope.Part is not null)
        {
            if (sheetPartPath is null) return false;
            if (!scope.Part.Equals(sheetPartPath, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // sheet field: must match visible sheet name
        if (scope.Sheet is not null)
        {
            if (sheetName is null) return false;
            if (!scope.Sheet.Equals(sheetName, StringComparison.Ordinal))
                return false;
        }

        // cell field: must match cell address
        if (scope.Cell is not null)
        {
            if (cellAddress is null) return false;
            if (!scope.Cell.Equals(cellAddress, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if <paramref name="mapping"/> is a global mapping (no scope).
    /// </summary>
    public static bool IsGlobal(PolicyMapping mapping) => mapping.Scope is null;

    /// <summary>
    /// Returns true if <paramref name="mapping"/> has a scope that references
    /// the SST part path directly (only valid for uncoupled entry checks).
    /// </summary>
    public static bool IsSstPartScope(PolicyMapping mapping, string? sstPartPath)
        => sstPartPath is not null
        && mapping.Scope?.Part is not null
        && mapping.Scope.Part.Equals(sstPartPath, StringComparison.OrdinalIgnoreCase)
        && mapping.Scope.Sheet is null
        && mapping.Scope.Cell is null;
}