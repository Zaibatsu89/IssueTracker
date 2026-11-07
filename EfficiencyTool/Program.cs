using System.Globalization;
using System.Text.Json;
using EfficiencyTool;

GeparseerdeArgumenten cli;
try
{
    cli = CliArgumenten.Parse(args);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Fout: {ex.Message}");
    Environment.ExitCode = 1;
    return;
}

switch (cli.Commando)
{
    case "start":
        VoerStartUit(cli.OpslagPad);
        return;
    case "afmelden":
        VoerAfmeldenUit(cli.OpslagPad, cli.RestArgs.Count > 0 ? cli.RestArgs[0] : null);
        return;
    case "lijst":
        VoerLijstUit(cli.OpslagPad);
        return;
}

string[] argumenten = cli.RestArgs.ToArray();
int docxIndex = Array.IndexOf(argumenten, "--docx");
if (docxIndex >= 0)
{
    string pad = docxIndex + 1 < argumenten.Length ? argumenten[docxIndex + 1] : "Opdracht Efficiëntiefactor.docx";
    WordDocumentGenerator.Genereer(pad);
    Console.WriteLine($"Word-document geschreven: {Path.GetFullPath(pad)}");
    return;
}

double schatting = LeesGetal("Eerste schatting werkitem (in uren): ");
double werkelijkeEffort = LeesGetal("Werkelijke effort (in uren, alle iteraties tot check-in): ", striktPositief: true);

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
    Environment.ExitCode = 1;
}

static DateTime LeesInvoermoment(string prompt = "Invoermoment (leeg = nu, yyyy-MM-dd HH:mm): ")
{
    while (true)
    {
        Console.Write(prompt);
        string? invoer = Console.ReadLine() ?? throw new InvalidOperationException("Invoerstroom is gesloten (EOF).");
        if (string.IsNullOrWhiteSpace(invoer))
            return ItemRegistratie.NormaliseerKloktijd(DateTime.Now);
        if (DateTime.TryParseExact(invoer.Trim(), "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime moment))
            return ItemRegistratie.NormaliseerKloktijd(moment);
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

static string F(double waarde, string formaat = "0.#####") => waarde.ToString(formaat, CultureInfo.InvariantCulture);

static void VoerStartUit(string pad)
{
    try
    {
        Console.Write("Jira-key (bijv. ABC-123): ");
        string key = Console.ReadLine() ?? throw new InvalidOperationException("Invoerstroom is gesloten (EOF).");
        double schatting = LeesGetal("Eerste schatting (in uren): ", striktPositief: true);
        DateTime start = LeesInvoermoment("Starttijd (leeg = nu, yyyy-MM-dd HH:mm): ");

        WerkItem item = ItemRegistratie.RegistreerStart(pad, key, schatting, start);
        Console.WriteLine($"Gestart: {item.Key}, schatting {F(item.SchattingUren)} uur, start {item.Start:yyyy-MM-dd HH:mm}");
    }
    catch (Exception ex) when (ex is InvalidOperationException or IOException or ArgumentException or JsonException or UnauthorizedAccessException)
    {
        Console.WriteLine($"Fout: {ex.Message}");
        Environment.ExitCode = 1;
    }
}

static void VoerLijstUit(string pad)
{
    try
    {
        var open = ItemRegistratie.LaadOpenstaand(pad);
        if (open.Count == 0)
        {
            Console.WriteLine("Geen openstaande items.");
            return;
        }
        Console.WriteLine("Openstaande items:");
        foreach (WerkItem item in open)
            Console.WriteLine($"  {item.Key}: schatting {F(item.SchattingUren)} uur, start {item.Start:yyyy-MM-dd HH:mm}");
    }
    catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
    {
        Console.WriteLine($"Fout: {ex.Message}");
        Environment.ExitCode = 1;
    }
}

static void VoerAfmeldenUit(string pad, string? key)
{
    try
    {
        var open = ItemRegistratie.LaadOpenstaand(pad);
        if (open.Count == 0)
        {
            Console.WriteLine("Geen openstaande items.");
            return;
        }

        WerkItem item;
        if (key is not null)
        {
            item = ItemRegistratie.ZoekOpenstaandItem(open, key) ?? throw new InvalidOperationException($"Geen openstaand item met key {key}.");
        }
        else if (open.Count == 1)
        {
            item = open[0];
        }
        else
        {
            Console.WriteLine("Openstaande items: " + string.Join(", ", open.Select(i => i.Key)));
            Console.Write("Key om af te melden: ");
            string invoer = Console.ReadLine() ?? throw new InvalidOperationException("Invoerstroom is gesloten (EOF).");
            item = ItemRegistratie.ZoekOpenstaandItem(open, invoer) ?? throw new InvalidOperationException($"Geen openstaand item met key {invoer.Trim()}.");
        }

        if (open.Count > 1)
            Console.WriteLine("Let op: er staan meer items open; roosteruren kunnen overlappen en mogen niet worden opgeteld.");

        DateTime eind = LeesInvoermoment("Afmeldtijd na alle iteraties (leeg = nu, yyyy-MM-dd HH:mm): ");
        double roosterUren = Werkrooster.BerekenRoosterUren(item.Start, eind);
        Console.WriteLine($"Roosteruren tussen start en afmelding: {F(roosterUren)} uur");

        double effort;
        while (true)
        {
            Console.Write(roosterUren > 0.0
                ? "Werkelijke effort (in uren, leeg = roosteruren): "
                : "Werkelijke effort (in uren, verplicht want roosteruren = 0): ");
            string invoer = Console.ReadLine() ?? throw new InvalidOperationException("Invoerstroom is gesloten (EOF).");
            if (string.IsNullOrWhiteSpace(invoer) && roosterUren > 0.0)
            {
                effort = roosterUren;
                break;
            }
            if (Efficiency.TryParseGetal(invoer, out effort) && effort > 0.0)
                break;
            Console.WriteLine("Ongeldige invoer. Voer een getal > 0 in.");
        }

        double extra = LeesGetal("Extra uren boven 6 per dag (in uren, niet al in de effort meegeteld; 0 = geen): ");

        WerkItem afgerond = ItemRegistratie.RegistreerAfmelding(pad, item.Key, eind, effort, extra);

        Console.WriteLine($"Afgemeld: {afgerond.Key}");
        Console.WriteLine($"Roosteruren: {F(afgerond.RoosterUren!.Value)} | effort: {F(afgerond.WerkelijkeEffortUren!.Value)} | extra uren: {F(afgerond.ExtraUren!.Value)}");
        double f1 = afgerond.EfficientieFactor!.Value;
        double f2 = afgerond.EfficientieFactorInclExtraUren!.Value;
        Console.WriteLine($"Efficiëntiefactor (standaard): {F(f1)} ({Efficiency.Programmeren(f1)})");
        Console.WriteLine($"Efficiëntiefactor (incl. extra uren): {F(f2)} ({Efficiency.Programmeren(f2)})");
    }
    catch (Exception ex) when (ex is InvalidOperationException or IOException or ArgumentException or OverflowException or JsonException or UnauthorizedAccessException)
    {
        Console.WriteLine($"Fout: {ex.Message} (item blijft open)");
        Environment.ExitCode = 1;
    }
}
