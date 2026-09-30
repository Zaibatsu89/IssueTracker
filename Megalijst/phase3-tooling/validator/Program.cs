using System.Security.Cryptography;
using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;

namespace Phase3Validator;
public static class Audit
{
    public static object Inspect(string file, FileFormatVersions version, out bool passed)
    {
        var before = SHA256.HashData(File.ReadAllBytes(file));
        var errors = new List<object>();
        try
        {
            using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var doc = SpreadsheetDocument.Open(stream, false);
            var validator = new OpenXmlValidator(version) { MaxNumberOfErrors = 0 };
            foreach (var error in validator.Validate(doc))
                errors.Add(new { id = error.Id, type = error.ErrorType.ToString(), description = error.Description,
                    part = error.Part?.Uri.ToString(), path = error.Path?.XPath });
        }
        catch (Exception e) { errors.Add(new { id = "OPEN_OR_VALIDATE_FAILED", description = e.Message }); }
        var after = SHA256.HashData(File.ReadAllBytes(file));
        if (!before.SequenceEqual(after)) errors.Add(new { id = "SOURCE_CHANGED", description = "File bytes changed during read-only validation" });
        passed = errors.Count == 0;
        return new { file = Path.GetFullPath(file), officeVersion = version.ToString(), sha256 = Convert.ToHexStringLower(before),
            readOnly = true, passed, errors, baselineExemptions = 0 };
    }
}
public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length != 3 || args[0] != "Office2019")
                throw new ArgumentException("usage: Validator Office2019 ORIGINAL.xlsx CANDIDATE.xlsx (Office2019 is explicit required target)");
            if (Path.GetFullPath(args[1]).Equals(Path.GetFullPath(args[2]), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Original and candidate must be separate paths");
            var original = Audit.Inspect(args[1], FileFormatVersions.Office2019, out var originalPassed);
            var candidate = Audit.Inspect(args[2], FileFormatVersions.Office2019, out var candidatePassed);
            Console.WriteLine(JsonSerializer.Serialize(new { status = originalPassed && candidatePassed ? "SCHEMA_PASS_NOT_RELEASE" : "BLOCKED",
                sdk = typeof(OpenXmlValidator).Assembly.GetName().Version?.ToString(), original, candidate }, new JsonSerializerOptions { WriteIndented = true }));
            return originalPassed && candidatePassed ? 0 : 1;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(JsonSerializer.Serialize(new { status = "BLOCKED", error = e.Message }));
            return 2;
        }
    }
}
