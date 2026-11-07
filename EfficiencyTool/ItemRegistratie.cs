using System.Text.Json;

namespace EfficiencyTool;

/// <summary>
/// Een geregistreerd Jira-werkitem. Alle tijden zijn lokale Nederlandse kloktijd zonder tijdzone
/// (<see cref="DateTimeKind.Unspecified"/>); alle hoeveelheden zijn in uren.
/// </summary>
/// <param name="Key">Jira-key, bijv. ABC-123 (hoofdletterongevoelig, getrimd opgeslagen).</param>
/// <param name="SchattingUren">Eerste schatting in uren (&gt; 0).</param>
/// <param name="Start">Starttijdstip.</param>
/// <param name="Eind">Afmeldtijdstip na alle iteraties; null = nog openstaand.</param>
/// <param name="RoosterUren">Door de domeinlaag berekende roosteruren tussen start en afmelding.</param>
/// <param name="WerkelijkeEffortUren">Definitieve effort in uren (gelijk aan de berekende roosteruren, &gt; 0).</param>
/// <param name="ExtraUren">Uren boven 6 per werkdag, BOVENOP de effort (niet al in de effort meegeteld).</param>
/// <param name="EfficientieFactor">Factor 1 (afgerond op 5 decimalen).</param>
/// <param name="EfficientieFactorInclExtraUren">Factor 2 inclusief extra uren (afgerond op 5 decimalen).</param>
/// <param name="BerekeningsVersie">Versie van de berekeningsregels.</param>
public sealed record WerkItem(
    string Key,
    double SchattingUren,
    DateTime Start,
    DateTime? Eind = null,
    double? RoosterUren = null,
    double? WerkelijkeEffortUren = null,
    double? ExtraUren = null,
    double? EfficientieFactor = null,
    double? EfficientieFactorInclExtraUren = null,
    int BerekeningsVersie = ItemRegistratie.HuidigeBerekeningsVersie);

/// <summary>Opslag (JSON, permanent lockbestand, veilige vervanging via .tmp) en registratielogica voor werkitems.</summary>
public static class ItemRegistratie
{
    public const int HuidigeBerekeningsVersie = 1;

    private static readonly JsonSerializerOptions JsonOpties = new() { WriteIndented = true };

    /// <summary>%LOCALAPPDATA%\EfficiencyTool\items.json</summary>
    public static string StandaardPad { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EfficiencyTool", "items.json");

    /// <summary>Maakt van een invoermoment lokale kloktijd zonder tijdzone.</summary>
    public static DateTime NormaliseerKloktijd(DateTime moment) => DateTime.SpecifyKind(moment, DateTimeKind.Unspecified);

    private static string NormaliseerPad(string pad)
    {
        if (string.IsNullOrWhiteSpace(pad))
            throw new ArgumentException("Opslagpad mag niet leeg zijn.", nameof(pad));
        return Path.GetFullPath(pad);
    }

    private static List<WerkItem> LaadZonderLock(string volledigPad)
    {
        if (!File.Exists(volledigPad))
            return [];
        string json = File.ReadAllText(volledigPad);
        if (string.IsNullOrWhiteSpace(json))
            return [];
        return JsonSerializer.Deserialize<List<WerkItem>>(json, JsonOpties) ?? [];
    }

    /// <summary>
    /// Proces-veilige (gelockte) leesactie voor alle items (openstaand en afgerond).
    /// Maakt geen map aan als de bovenliggende map vóór de controle nog niet bestaat (geeft dan een lege lijst terug zonder lock).
    /// Mocht de map door een gelijktijdige race verdwijnen tussen de <c>Directory.Exists</c>-controle en het verkrijgen van de lock,
    /// dan maakt <see cref="VerkrijgBestandsLock"/> de map opnieuw aan en wordt alsnog veilig een lege lijst gelezen.
    /// </summary>
    public static List<WerkItem> LaadAlles(string pad)
    {
        string volledigPad = NormaliseerPad(pad);
        if (Directory.Exists(volledigPad))
            throw new IOException($"Opslagpad verwijst naar een bestaande map: {volledigPad}");

        string? map = Path.GetDirectoryName(volledigPad);
        if (!string.IsNullOrEmpty(map) && !Directory.Exists(map))
            return [];

        using var bestandsLock = VerkrijgBestandsLock(volledigPad);
        return LaadZonderLock(volledigPad);
    }

    /// <summary>
    /// Proces-veilige (gelockte) leesactie voor alle openstaande items (<c>Eind == null</c>).
    /// Maakt geen map aan als de bovenliggende map nog niet bestaat (geeft dan een lege lijst terug zonder lock).
    /// </summary>
    public static List<WerkItem> LaadOpenstaand(string pad) =>
        LaadAlles(pad).Where(i => i.Eind is null).ToList();

    /// <summary>
    /// Interne, ongelockte laag-niveau schrijfmethode (wordt door <see cref="RegistreerStart"/> en <see cref="RegistreerAfmelding"/>
    /// binnen een actieve bestandslock aangeroepen, en is alleen zichtbaar voor tests via <c>InternalsVisibleTo</c>). Schrijft eerst volledig naar <c>&lt;pad&gt;.tmp</c> in dezelfde map
    /// (hetzelfde bestandssysteem) met <c>Flush(true)</c> en vervangt daarna het doelbestand via
    /// <c>File.Move(..., overwrite: true)</c> zonder voorafgaande <c>File.Delete</c>.
    /// Bij een mislukte schrijfpoging of vervanging blijft het bestaande doelbestand intact en geldt de registratie als niet-voltooid.
    /// Let op: dit biedt geen universele crash-atomiciteitsgarantie op elk besturingssysteem of bestandssysteem.
    /// </summary>
    internal static void SlaOp(string pad, IReadOnlyList<WerkItem> items)
    {
        string volledigPad = NormaliseerPad(pad);
        if (Directory.Exists(volledigPad))
            throw new IOException($"Opslagpad verwijst naar een bestaande map: {volledigPad}");

        string? map = Path.GetDirectoryName(volledigPad);
        if (!string.IsNullOrEmpty(map))
            Directory.CreateDirectory(map);

        string tmp = volledigPad + ".tmp";
        try
        {
            using (var stream = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, items, JsonOpties);
                stream.Flush(true);
            }
            File.Move(tmp, volledigPad, overwrite: true);
        }
        finally
        {
            try { if (File.Exists(tmp)) File.Delete(tmp); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    /// <summary>
    /// Interne hulpmethode die een exclusieve bestandslock verkrijgt via een permanent lockbestand
    /// <c>&lt;pad&gt;.lock</c> (<c>FileShare.None</c>) met korte retry (alleen intern gebruikt en zichtbaar
    /// voor tests via <c>InternalsVisibleTo</c>). Maakt de bovenliggende map aan indien nodig.
    /// Vrijgeven door de geretourneerde stream te disposen.
    /// </summary>
    /// <exception cref="IOException">Als het bestand blijvend vergrendeld is of het pad een map is.</exception>
    internal static IDisposable VerkrijgBestandsLock(string pad, int pogingen = 20, int wachtMs = 100)
    {
        string volledigPad = NormaliseerPad(pad);
        if (Directory.Exists(volledigPad))
            throw new IOException($"Opslagpad verwijst naar een bestaande map: {volledigPad}");

        string? map = Path.GetDirectoryName(volledigPad);
        if (!string.IsNullOrEmpty(map))
            Directory.CreateDirectory(map);

        string lockPad = volledigPad + ".lock";
        for (int poging = 1; ; poging++)
        {
            try
            {
                return new FileStream(lockPad, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception ex) when ((ex is IOException or UnauthorizedAccessException) && poging < pogingen)
            {
                Thread.Sleep(wachtMs);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new IOException($"Opslagbestand is vergrendeld door een ander proces: {lockPad}", ex);
            }
        }
    }

    /// <summary>Zoekt een openstaand item (Eind == null) in een reeds geladen collectie, hoofdletterongevoelig.</summary>
    public static WerkItem? ZoekOpenstaandItem(IEnumerable<WerkItem> items, string key) =>
        items.FirstOrDefault(i => i.Eind is null && string.Equals(i.Key, key.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Proces-veilige (gelockte) registratie van een nieuw openstaand item met getrimde key.
    /// Volgorde: pad normaliseren → lock verkrijgen → bestaande JSON lezen → valideren → item toevoegen → SlaOp → lock vrijgeven.
    /// </summary>
    /// <exception cref="ArgumentException">Bij een lege of whitespace key.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Bij schatting &lt;= 0 of niet-eindig.</exception>
    /// <exception cref="InvalidOperationException">Als de key al openstaat.</exception>
    public static WerkItem RegistreerStart(string pad, string key, double schattingUren, DateTime start)
    {
        string volledigPad = NormaliseerPad(pad);
        using var bestandsLock = VerkrijgBestandsLock(volledigPad);
        List<WerkItem> items = LaadZonderLock(volledigPad);

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Jira-key mag niet leeg zijn.", nameof(key));
        key = key.Trim();

        if (!double.IsFinite(schattingUren) || schattingUren <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(schattingUren), schattingUren, "Schatting moet een eindig getal > 0 zijn (in uren).");

        if (ZoekOpenstaandItem(items, key) is not null)
            throw new InvalidOperationException($"Er staat al een openstaand item met key {key}.");

        var item = new WerkItem(key, schattingUren, NormaliseerKloktijd(start));
        items.Add(item);
        SlaOp(volledigPad, items);
        return item;
    }

    /// <summary>
    /// Proces-veilige (gelockte) afmelding van een openstaand item.
    /// Volgorde: pad normaliseren → lock verkrijgen → bestaande JSON lezen → valideren → roosteruren en factoren berekenen → SlaOp → lock vrijgeven.
    /// Bij een fout blijft het bestaande bestand op schijf ongewijzigd en het item open.
    /// </summary>
    /// <exception cref="InvalidOperationException">Onbekend of al afgerond item.</exception>
    /// <exception cref="ArgumentException">Eind niet na start.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Effort wijkt af van de berekende roosteruren, effort &lt;= 0, extra &lt; 0, of niet-eindig.</exception>
    public static WerkItem RegistreerAfmelding(string pad, string key, DateTime eind, double werkelijkeEffortUren, double extraUren)
    {
        string volledigPad = NormaliseerPad(pad);
        using var bestandsLock = VerkrijgBestandsLock(volledigPad);
        List<WerkItem> items = LaadZonderLock(volledigPad);

        WerkItem open = ZoekOpenstaandItem(items, key)
            ?? throw new InvalidOperationException($"Geen openstaand item met key {key.Trim()}.");

        eind = NormaliseerKloktijd(eind);
        double roosterUren = Werkrooster.BerekenRoosterUren(open.Start, eind);

        if (!(Math.Abs(werkelijkeEffortUren - roosterUren) <= 1e-9))
            throw new ArgumentOutOfRangeException(nameof(werkelijkeEffortUren), werkelijkeEffortUren, $"Effort moet gelijk zijn aan de berekende roosteruren ({roosterUren}).");

        double factor = Efficiency.BerekenEfficientieFactor(open.SchattingUren, werkelijkeEffortUren);
        double factorExtra = Efficiency.BerekenEfficientieFactorInclExtraUren(open.SchattingUren, werkelijkeEffortUren, extraUren);

        WerkItem afgerond = open with
        {
            Eind = eind,
            RoosterUren = roosterUren,
            WerkelijkeEffortUren = werkelijkeEffortUren,
            ExtraUren = extraUren,
            EfficientieFactor = factor,
            EfficientieFactorInclExtraUren = factorExtra,
            BerekeningsVersie = HuidigeBerekeningsVersie,
        };

        items[items.IndexOf(open)] = afgerond;
        SlaOp(volledigPad, items);
        return afgerond;
    }
}
