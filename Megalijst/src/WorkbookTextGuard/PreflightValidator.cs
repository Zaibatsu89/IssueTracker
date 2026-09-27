using System;
using System.Collections.Generic;

namespace WorkbookTextGuard;

/// <summary>
/// Shared preflight validation logic used by both 'check --mode preflight' and 'clean'.
///
/// Entry points:
/// - Validate(OoxmlPackage, Policy) — package-based, builds worksheet context internally.
/// - Validate(IReadOnlyList&lt;TextLocation&gt;, Policy, EmoticonDetector) — location-based.
///
/// Rules:
/// - SST locations are replaced by per-referencing-cell locations for scope evaluation.
/// - Uncoupled SST entries are tested against global or SST-part-scoped rules only.
/// - Formula cached values are skipped in preflight.
/// - Non-transformable sources block even with a mapping.
/// - Decorative sequences are normalised into EmojiMappings by Policy.Load.
/// </summary>
internal static class PreflightValidator
{
    /// <summary>
    /// Package-based entry point: builds worksheet context and validates.
    /// </summary>
    public static List<PreflightViolation> Validate(OoxmlPackage pkg, Policy policy)
    {
        var emoticonProfile = policy.EmoticonProfile.Equals("markup", StringComparison.OrdinalIgnoreCase)
            ? EmoticonDetector.BoundaryProfile.Markup
            : EmoticonDetector.BoundaryProfile.Plain;
        var detector = new EmoticonDetector(emoticonProfile);

        // Stage worksheets to build the reverse index
        var stagedWorksheets = new Dictionary<string, System.Xml.Linq.XDocument>(StringComparer.OrdinalIgnoreCase);
        foreach (var sheet in pkg.Sheets)
            if (pkg.PartExists(sheet.PartPath))
                stagedWorksheets[sheet.PartPath] = pkg.LoadXml(sheet.PartPath);

        FormulaScopeValidator.Validate(pkg, policy, stagedWorksheets);
        var ssIndex = SharedStringReferenceIndex.Build(pkg, stagedWorksheets);
        var locations = TextExtractor.ExtractAll(pkg);

        return ValidateInternal(locations, policy, detector, ssIndex, pkg.SharedStringPartPath);
    }

    /// <summary>
    /// Location-based entry point (for use when locations are already extracted).
    /// Does not resolve SST cell context — use the package-based overload for full scope evaluation.
    /// </summary>
    public static List<PreflightViolation> Validate(
        IReadOnlyList<TextLocation> locations,
        Policy policy,
        EmoticonDetector detector)
    {
        return ValidateInternal(locations, policy, detector, null, null);
    }

    private static List<PreflightViolation> ValidateInternal(
        IReadOnlyList<TextLocation> locations,
        Policy policy,
        EmoticonDetector detector,
        SharedStringReferenceIndex? ssIndex,
        string? sstPartPath)
    {
        var violations = new List<PreflightViolation>();

        foreach (var loc in locations)
        {
            // Skip formula cached values in preflight
            if (loc.Source == TextSource.FormulaCachedValue) continue;

            bool isTransformable = IsTransformableSource(loc.Source);

            // For SST locations: expand to per-cell context if index is available
            if (loc.Source == TextSource.SharedString && loc.SharedStringIndex.HasValue && ssIndex is not null)
            {
                var refs = ssIndex.GetRefs(loc.SharedStringIndex.Value);
                if (refs.Count == 0)
                {
                    // Uncoupled entry: test against global or SST-part-scoped rules only
                    ValidateText(loc.LogicalText, loc, policy, detector,
                        isTransformable, violations, useUncoupledScope: true, sstPartPath: sstPartPath);
                }
                else
                {
                    // Test each referencing cell with its worksheet context
                    foreach (var cellRef in refs)
                    {
                        var cellLoc = loc with
                        {
                            PartPath    = cellRef.SheetPartPath,
                            SheetName   = cellRef.SheetName,
                            CellAddress = cellRef.CellAddress
                        };
                        ValidateText(loc.LogicalText, cellLoc, policy, detector,
                            isTransformable, violations, useUncoupledScope: false, sstPartPath: null);
                    }
                }
                continue;
            }

            ValidateText(loc.LogicalText, loc, policy, detector,
                isTransformable, violations, useUncoupledScope: false, sstPartPath: null);
        }

        return violations;
    }

    private static void ValidateText(
        string text,
        TextLocation loc,
        Policy policy,
        EmoticonDetector detector,
        bool isTransformable,
        List<PreflightViolation> violations,
        bool useUncoupledScope,
        string? sstPartPath)
    {
        // ── Emoji ────────────────────────────────────────────────────────────
        var emojiMatches = EmojiDetector.FindAll(text);
        foreach (var m in emojiMatches)
        {
            if (isTransformable)
            {
                bool mapped = policy.EmojiMappings.Exists(pm =>
                    pm.Sequence.Equals(m.Sequence, StringComparison.Ordinal)
                    && (useUncoupledScope
                        ? (MappingScopeMatcher.IsGlobal(pm) || MappingScopeMatcher.IsSstPartScope(pm, sstPartPath))
                        : MappingScopeMatcher.Matches(pm, loc.PartPath, loc.SheetName, loc.CellAddress)));

                if (!mapped)
                    violations.Add(new PreflightViolation(
                        ViolationType.UnmappedEmoji, m.Sequence, loc,
                        "Unmapped emoji — add to policy.json before running clean."));
            }
            else
            {
                violations.Add(new PreflightViolation(
                    ViolationType.NonTransformableSource, m.Sequence, loc,
                    $"Emoji in non-transformable source ({loc.Source}). Manual correction required."));
            }
        }

        // ── Emoticons ────────────────────────────────────────────────────────
        bool scanEmoticons = loc.Source is TextSource.FormulaLiteral or TextSource.DefinedName
            || (!loc.IsFormula && loc.Source != TextSource.FormulaCachedValue);

        if (scanEmoticons)
        {
            var emoticonMatches = detector.FindAll(text);
            foreach (var m in emoticonMatches)
            {
                if (isTransformable)
                {
                    bool mapped = policy.EmoticonMappings.Exists(pm =>
                        pm.Sequence.Equals(m.Value, StringComparison.Ordinal)
                        && (useUncoupledScope
                            ? (MappingScopeMatcher.IsGlobal(pm) || MappingScopeMatcher.IsSstPartScope(pm, sstPartPath))
                            : MappingScopeMatcher.Matches(pm, loc.PartPath, loc.SheetName, loc.CellAddress)));

                    if (!mapped)
                        violations.Add(new PreflightViolation(
                            ViolationType.UnmappedEmoticon, m.Value, loc,
                            "Unmapped emoticon — add to policy.json before running clean."));
                }
                else
                {
                    violations.Add(new PreflightViolation(
                        ViolationType.NonTransformableSource, m.Value, loc,
                        $"Emoticon in non-transformable source ({loc.Source}). Manual correction required."));
                }
            }
        }
    }

    private static bool IsTransformableSource(TextSource source) => source switch
    {
        TextSource.SharedString   => true,
        TextSource.InlineString   => true,
        TextSource.FormulaLiteral => true,
        TextSource.DefinedName    => false, // Detection only; requires manual correction.
        _                         => false
    };
}

internal sealed record PreflightViolation(
    ViolationType Type,
    string Sequence,
    TextLocation Location,
    string Message);

internal enum ViolationType
{
    UnmappedEmoji,
    UnmappedEmoticon,
    NonTransformableSource
}