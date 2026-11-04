using System.Globalization;
using EfficiencyTool;

string[] argumenten = args;
int docxIndex = Array.IndexOf(argumenten, "--docx");
if (docxIndex >= 0)
{
    string pad = docxIndex + 1 < argumenten.Length ? argumenten[docxIndex + 1] : "Opdracht Efficiëntiefactor.docx";
    WordDocumentGenerator.Genereer(pad);
    Console.WriteLine($"Word-document geschreven: {Path.GetFullPath(pad)}");
    return;
}

double schatting = LeesGetal("Eerste schatting werkitem: ");
double werkelijkeEffort = LeesGetal("Werkelijke effort (alle iteraties tot check-in): ", striktPositief: true);

try
{
    double factor = Efficiency.BerekenEfficientieFactor(schatting, werkelijkeEffort);
    if (schatting > 0.0)
    {
        double effortFactor = Efficiency.BerekenEffortVsSchattingFactor(schatting, werkelijkeEffort);
        Console.WriteLine($"Werkelijke effort vs. schatting factor: {effortFactor.ToString("0.00", CultureInfo.InvariantCulture)}");
    }
    Console.WriteLine($"Efficiëntiefactor: {factor.ToString("0.#####", CultureInfo.InvariantCulture)} ({Efficiency.Programmeren(factor)})");
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"Ongeldige invoer: {ex.Message}");
}

static double LeesGetal(string prompt, bool striktPositief = false)
{
    while (true)
    {
        Console.Write(prompt);
        string? invoer = Console.ReadLine() ?? throw new InvalidOperationException("Invoerstroom is gesloten (EOF).");
        if (Efficiency.TryParseGetal(invoer, out double waarde) && (striktPositief ? waarde > 0.0 : waarde >= 0.0))
            return waarde;
        Console.WriteLine("Ongeldige invoer. Voer een geldig getal in.");
    }
}
