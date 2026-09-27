using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace WorkbookTextGuard;

/// <summary>
/// Reads an .xlsx ZIP package via OOXML relationships, providing access to
/// parts by content type and relationship type without assuming fixed paths.
///
/// Two construction paths:
/// - <see cref="Open(string)"/>: read-only path-based open (inventory, check).
/// - <see cref="Open(byte[], string)"/>: snapshot-based open for mutating commands
///   (clean). The caller supplies the pre-computed SHA-256 hex; the package
///   verifies it against the bytes and keeps the snapshot immutable.
/// </summary>
internal sealed class OoxmlPackage : IDisposable
{
    private readonly ZipArchive _zip;
    private readonly Dictionary<string, byte[]> _partBytes = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    public string FilePath { get; }
    public string Sha256Hex { get; }

    // Resolved part paths
    public string? WorkbookPartPath { get; private set; }
    public string? SharedStringPartPath { get; private set; }
    public List<SheetInfo> Sheets { get; } = new();
    public List<string> CommentPartPaths { get; } = new();
    public List<string> VmlDrawingPartPaths { get; } = new();
    public List<string> DrawingPartPaths { get; } = new();
    public List<string> ExternalLinkPartPaths { get; } = new();
    public List<string> UnrecognisedTextPartPaths { get; } = new();

    private OoxmlPackage(string filePath, ZipArchive zip, string sha256Hex)
    {
        FilePath = filePath;
        _zip = zip;
        Sha256Hex = sha256Hex;
    }

    /// <summary>Open from file path (read-only commands: inventory, check).</summary>
    public static OoxmlPackage Open(string filePath)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);
        string sha256 = ComputeSha256(fileBytes);
        return OpenFromBytes(filePath, fileBytes, sha256);
    }

    /// <summary>
    /// Open from a pre-read byte snapshot with a caller-supplied expected SHA-256.
    /// Used by mutating commands (clean) to guarantee hash/ZIP consistency.
    /// Throws <see cref="InvalidDataException"/> if the computed hash does not
    /// match <paramref name="expectedSha256Hex"/>.
    /// </summary>
    public static OoxmlPackage Open(byte[] bytes, string expectedSha256Hex)
    {
        string actual = ComputeSha256(bytes);
        if (!actual.Equals(expectedSha256Hex, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"SHA-256 mismatch: expected {expectedSha256Hex}, computed {actual}.");
        return OpenFromBytes("<snapshot>", bytes, actual);
    }

    private static OoxmlPackage OpenFromBytes(string filePath, byte[] bytes, string sha256)
    {
        var zip = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read, leaveOpen: false);
        var pkg = new OoxmlPackage(filePath, zip, sha256);
        pkg.Resolve();
        return pkg;
    }

    public static string ComputeSha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static string ComputeSha256(string filePath)
        => ComputeSha256(File.ReadAllBytes(filePath));

    private void Resolve()
    {
        var rootRels = ReadRelationships("_rels/.rels");
        string? workbookTarget = rootRels
            .FirstOrDefault(r => r.Type == OoxmlNamespaces.RelTypeWorkbook)?.Target;

        if (workbookTarget is null)
            throw new InvalidDataException("No workbook relationship found in _rels/.rels.");

        WorkbookPartPath = NormalisePath("", workbookTarget);

        string wbDir = GetDirectory(WorkbookPartPath);
        string wbRelsPath = GetRelsPath(WorkbookPartPath);
        var wbRels = ReadRelationships(wbRelsPath);

        var ssRel = wbRels.FirstOrDefault(r => r.Type == OoxmlNamespaces.RelTypeSharedStrings);
        if (ssRel is not null)
            SharedStringPartPath = NormalisePath(wbDir, ssRel.Target);

        var wbDoc = LoadXml(WorkbookPartPath);
        XNamespace ns = OoxmlNamespaces.SpreadsheetMl;

        var sheetElements = wbDoc.Descendants(ns + "sheet").ToList();
        foreach (var sheetEl in sheetElements)
        {
            string? name    = sheetEl.Attribute("name")?.Value;
            string? sheetId = sheetEl.Attribute("sheetId")?.Value;
            string? rId     = sheetEl.Attribute(XName.Get("id", OoxmlNamespaces.Relationships))?.Value;
            bool hidden = sheetEl.Attribute("state")?.Value
                ?.Equals("hidden", StringComparison.OrdinalIgnoreCase) == true
                || sheetEl.Attribute("state")?.Value
                ?.Equals("veryHidden", StringComparison.OrdinalIgnoreCase) == true;

            if (rId is null) continue;
            var rel = wbRels.FirstOrDefault(r => r.Id == rId);
            if (rel is null) continue;

            string partPath = NormalisePath(wbDir, rel.Target);
            var info = new SheetInfo(name ?? partPath, sheetId ?? "", partPath, hidden);

            string sheetRelsPath = GetRelsPath(partPath);
            var sheetRels = ReadRelationships(sheetRelsPath);
            string sheetDir = GetDirectory(partPath);

            foreach (var sr in sheetRels)
            {
                string target = NormalisePath(sheetDir, sr.Target);
                if (sr.Type == OoxmlNamespaces.RelTypeComments)
                {
                    info.CommentPartPath = target;
                    if (!CommentPartPaths.Contains(target)) CommentPartPaths.Add(target);
                }
                else if (sr.Type == OoxmlNamespaces.RelTypeVmlDrawing)
                {
                    info.VmlDrawingPartPath = target;
                    if (!VmlDrawingPartPaths.Contains(target)) VmlDrawingPartPaths.Add(target);
                }
                else if (sr.Type == OoxmlNamespaces.RelTypeDrawing)
                {
                    if (!DrawingPartPaths.Contains(target)) DrawingPartPaths.Add(target);
                }
            }

            Sheets.Add(info);
        }

        foreach (var rel in wbRels.Where(r => r.Type == OoxmlNamespaces.RelTypeExternalLink))
            ExternalLinkPartPaths.Add(NormalisePath(wbDir, rel.Target));
    }

    public XDocument LoadXml(string partPath)
    {
        byte[] bytes = GetPartBytes(partPath);
        using var ms = new MemoryStream(bytes);
        using var reader = System.Xml.XmlReader.Create(ms, XmlSettings.ForOoxmlRead());
        return XDocument.Load(reader, LoadOptions.PreserveWhitespace);
    }

    public byte[] GetPartBytes(string partPath)
    {
        if (_partBytes.TryGetValue(partPath, out byte[]? cached)) return cached;

        var entry = _zip.GetEntry(partPath)
            ?? _zip.GetEntry(partPath.TrimStart('/'))
            ?? throw new FileNotFoundException($"Part not found in ZIP: {partPath}");

        using var stream = entry.Open();
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        byte[] bytes = ms.ToArray();
        _partBytes[partPath] = bytes;
        return bytes;
    }

    public bool PartExists(string partPath)
        => _zip.GetEntry(partPath) is not null
        || _zip.GetEntry(partPath.TrimStart('/')) is not null;

    public IEnumerable<string> AllEntryNames()
        => _zip.Entries.Select(e => e.FullName);

    public string PartSha256(string partPath)
    {
        byte[] bytes = GetPartBytes(partPath);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private List<RelationshipEntry> ReadRelationships(string relsPath)
    {
        var result = new List<RelationshipEntry>();
        if (!PartExists(relsPath)) return result;

        var doc = LoadXml(relsPath);
        XNamespace ns = OoxmlNamespaces.PackageRelationships;
        foreach (var rel in doc.Descendants(ns + "Relationship"))
        {
            string? id     = rel.Attribute("Id")?.Value;
            string? type   = rel.Attribute("Type")?.Value;
            string? target = rel.Attribute("Target")?.Value;
            if (id is not null && type is not null && target is not null)
                result.Add(new RelationshipEntry(id, type, target));
        }
        return result;
    }

    private static string NormalisePath(string baseDir, string target)
    {
        if (target.StartsWith('/')) return target.TrimStart('/');
        if (string.IsNullOrEmpty(baseDir)) return target;

        string combined = baseDir.TrimEnd('/') + "/" + target;
        var parts = new List<string>();
        foreach (var seg in combined.Split('/'))
        {
            if (seg == "..")      { if (parts.Count > 0) parts.RemoveAt(parts.Count - 1); }
            else if (seg != ".") { parts.Add(seg); }
        }
        return string.Join("/", parts);
    }

    private static string GetDirectory(string partPath)
    {
        int idx = partPath.LastIndexOf('/');
        return idx < 0 ? "" : partPath[..idx];
    }

    private static string GetRelsPath(string partPath)
    {
        string dir  = GetDirectory(partPath);
        string file = partPath[(partPath.LastIndexOf('/') + 1)..];
        return string.IsNullOrEmpty(dir)
            ? $"_rels/{file}.rels"
            : $"{dir}/_rels/{file}.rels";
    }

    public void Dispose()
    {
        if (!_disposed) { _zip.Dispose(); _disposed = true; }
    }

    private sealed record RelationshipEntry(string Id, string Type, string Target);
}

internal sealed class SheetInfo
{
    public string Name { get; }
    public string SheetId { get; }
    public string PartPath { get; }
    public bool Hidden { get; }
    public string? CommentPartPath { get; set; }
    public string? VmlDrawingPartPath { get; set; }

    public SheetInfo(string name, string sheetId, string partPath, bool hidden)
    {
        Name = name; SheetId = sheetId; PartPath = partPath; Hidden = hidden;
    }
}