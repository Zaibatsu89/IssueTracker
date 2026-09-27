using System.Text;
using System.Xml;

namespace WorkbookTextGuard;

/// <summary>
/// Canonical XmlWriterSettings for serialising modified OOXML parts.
/// BOM-less UTF-8, no indentation, no newline normalisation, XML declaration preserved.
/// </summary>
internal static class XmlSettings
{
    public static XmlWriterSettings ForOoxmlPart() => new()
    {
        Encoding             = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
        OmitXmlDeclaration   = false,
        NewLineHandling      = NewLineHandling.None,
        Indent               = false,
        CheckCharacters      = true,
        CloseOutput          = false
    };

    public static XmlReaderSettings ForOoxmlRead() => new()
    {
        DtdProcessing        = DtdProcessing.Prohibit,
        XmlResolver          = null,
        IgnoreWhitespace     = false,
        IgnoreComments       = false,
        IgnoreProcessingInstructions = false
    };
}