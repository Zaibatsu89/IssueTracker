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
    default:
        Console.WriteLine("Gebruik: EfficiencyTool start | afmelden [key] | lijst [--bestand <pad>]");
        Environment.ExitCode = 1;
        return;
}

static DateTime LeesInvoermoment(string prompt)
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

static void VoerAfmeldenUit(string pad, string? key)
{
    WerkItem? geselecteerd = null;
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

        geselecteerd = item;

        if (open.Count > 1)
            Console.WriteLine("Let op: er staan meer items open; roosteruren kunnen overlappen en mogen niet worden opgeteld.");

        DateTime eind = LeesInvoermoment("Afmeldtijd na alle iteraties (leeg = nu, yyyy-MM-dd HH:mm): ");
        double roosterUren = Werkrooster.BerekenRoosterUren(item.Start, eind);
        Console.WriteLine($"Roosteruren tussen start en afmelding: {F(roosterUren)} uur");

        if (roosterUren <= 0.0)
            throw new InvalidOperationException($"Geen roosteruren tussen start ({item.Start:yyyy-MM-dd HH:mm}) en afmelding ({eind:yyyy-MM-dd HH:mm}); afmelden is niet mogelijk. Het werkrooster is ma, di, do en vr 09:00-12:30 en 13:00-15:30; kies een afmeldtijd binnen die uren.");
        double effort = roosterUren;

        double extra = LeesGetal("Extra uren boven 6 per dag (in uren, niet al in de effort meegeteld; 0 = geen): ");

        WerkItem afgerond = ItemRegistratie.RegistreerAfmelding(pad, item.Key, eind, effort, extra);

        Console.WriteLine($"Afgemeld: {afgerond.Key}");
        Console.WriteLine($"Roosteruren: {F(afgerond.RoosterUren!.Value)} | extra uren: {F(afgerond.ExtraUren!.Value)}");
        double f1 = afgerond.EfficientieFactor!.Value;
        double f2 = afgerond.EfficientieFactorInclExtraUren!.Value;
        Console.WriteLine($"Efficiëntiefactor (standaard): {F(f1)} ({Efficiency.Programmeren(f1)})");
        Console.WriteLine($"Efficiëntiefactor (incl. extra uren): {F(f2)} ({Efficiency.Programmeren(f2)})");
    }
    catch (Exception ex) when (ex is InvalidOperationException or IOException or ArgumentException or OverflowException or JsonException or UnauthorizedAccessException)
    {
        Console.WriteLine(geselecteerd is null ? $"Fout: {ex.Message}" : $"Fout: {ex.Message} (item blijft open)");
        Environment.ExitCode = 1;
    }
}

/// <summary>Impliciete programmaklasse; bevat de hulpmethoden die tests rechtstreeks aanroepen.</summary>
partial class Program
{
    private static string F(double waarde, string formaat = "0.#####") => waarde.ToString(formaat, CultureInfo.InvariantCulture);

    /// <summary>Toont de openstaande items; meldt verwachte fouten als <c>Fout: ...</c> met exitcode 1.</summary>
    internal static void VoerLijstUit(string pad)
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
        catch (Exception ex) when (ex is InvalidOperationException or IOException or ArgumentException or JsonException or UnauthorizedAccessException)
        {
            Console.WriteLine($"Fout: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }
}
