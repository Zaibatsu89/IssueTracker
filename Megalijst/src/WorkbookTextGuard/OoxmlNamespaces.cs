namespace WorkbookTextGuard;

/// <summary>
/// Well-known OOXML namespace URIs used throughout the tool.
/// </summary>
internal static class OoxmlNamespaces
{
    public const string SpreadsheetMl      = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    public const string Relationships      = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    public const string PackageRelationships = "http://schemas.openxmlformats.org/package/2006/relationships";
    public const string MarkupCompatibility = "http://schemas.openxmlformats.org/markup-compatibility/2006";
    public const string DrawingMl          = "http://schemas.openxmlformats.org/drawingml/2006/main";
    public const string SpreadsheetDrawing = "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing";
    public const string Vml                = "urn:schemas-microsoft-com:vml";
    public const string Office             = "urn:schemas-microsoft-com:office:office";
    public const string Excel              = "urn:schemas-microsoft-com:office:excel";

    // Relationship type constants
    public const string RelTypeWorkbook       = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument";
    public const string RelTypeWorksheet      = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet";
    public const string RelTypeSharedStrings  = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings";
    public const string RelTypeStyles         = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles";
    public const string RelTypeComments       = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/comments";
    public const string RelTypeVmlDrawing     = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/vmlDrawing";
    public const string RelTypeDrawing        = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing";
    public const string RelTypeExternalLink   = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/externalLink";
}