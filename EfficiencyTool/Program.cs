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

    DateTime invoermoment = LeesInvoermoment();
    var overzicht = Werkrooster.BerekenWeekoverzicht(invoermoment, werkelijkeEffort);
    Console.WriteLine($"Theoretisch startpunt (100% roosterallocatie, incl. pauze 12:30-13:00, excl. overhead; effort in uren): {overzicht[0].Start:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine("Weekoverzicht:");
    foreach (WerkTijdvak tijdvak in overzicht)
        Console.WriteLine($"  {tijdvak.Start:ddd yyyy-MM-dd HH:mm:ss} - {tijdvak.Eind:HH:mm:ss} ({tijdvak.Duur.TotalHours.ToString("0.00", CultureInfo.InvariantCulture)} u)");
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"Ongeldige invoer: {ex.Message}");
}

static DateTime LeesInvoermoment()
{
    while (true)
    {
        Console.Write("Invoermoment (leeg = nu, yyyy-MM-dd HH:mm): ");
        string? invoer = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(invoer))
            return DateTime.Now;
        if (DateTime.TryParseExact(invoer.Trim(), "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime moment))
            return moment;
        Console.WriteLine("Ongeldige invoer. Gebruik yyyy-MM-dd HH:mm.");
    }
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
