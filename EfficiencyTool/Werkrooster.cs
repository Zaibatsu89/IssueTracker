namespace EfficiencyTool;

/// <summary>Aaneengesloten gewerkt tijdvak op één werkdag.</summary>
/// <param name="Start">Startmoment van het tijdvak.</param>
/// <param name="Eind">Eindmoment van het tijdvak.</param>
/// <param name="Duur">Netto duur van het tijdvak.</param>
public readonly record struct WerkTijdvak(DateTime Start, DateTime Eind, TimeSpan Duur);

/// <summary>
/// Rekent een startpunt terug vanaf het invoermoment over het werkrooster
/// (maandag, dinsdag, donderdag, vrijdag, 09:00-15:30 met pauze 12:30-13:00; 6,0 uur/dag, 24 uur/week).
/// Lokale kloktijd, zonder feestdagen.
/// </summary>
public static class Werkrooster
{
    /// <summary>Begin van een werkdag (09:00).</summary>
    public static readonly TimeOnly WerkdagStart = new(9, 0);

    /// <summary>Begin van de dagelijkse pauze (12:30).</summary>
    public static readonly TimeOnly PauzeStart = new(12, 30);

    /// <summary>Einde van de dagelijkse pauze (13:00).</summary>
    public static readonly TimeOnly PauzeEind = new(13, 0);

    /// <summary>Einde van een werkdag (15:30).</summary>
    public static readonly TimeOnly WerkdagEind = new(15, 30);

    /// <summary>Maximale effort in uren (voorkomt overflow en extreem lange lussen).</summary>
    public const double MaximaleEffortUren = 100_000.0;

    /// <summary>Geeft aan of de dag een roosterwerkdag is (ma, di, do, vr).</summary>
    public static bool IsWerkdag(DayOfWeek dag) =>
        dag is DayOfWeek.Monday or DayOfWeek.Tuesday or DayOfWeek.Thursday or DayOfWeek.Friday;

    /// <summary>Theoretisch startpunt: begin van het eerste tijdvak uit <see cref="BerekenWeekoverzicht"/>.</summary>
    public static DateTime BerekenStartpunt(DateTime invoermoment, double effortUren) =>
        BerekenWeekoverzicht(invoermoment, effortUren)[0].Start;

    /// <summary>Chronologische tijdvakken tussen het teruggerekende startpunt en het invoermoment.</summary>
    public static IReadOnlyList<WerkTijdvak> BerekenWeekoverzicht(DateTime invoermoment, double effortUren)
    {
        if (!double.IsFinite(effortUren))
            throw new ArgumentOutOfRangeException(nameof(effortUren), effortUren, "Effort moet een eindig getal zijn.");
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(effortUren);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(effortUren, MaximaleEffortUren);

        double seconden = Math.Round(effortUren * 3600.0, MidpointRounding.AwayFromZero);
        if (seconden < 1.0)
            throw new ArgumentOutOfRangeException(nameof(effortUren), effortUren, "Effort moet na afronding minimaal 1 seconde zijn.");

        TimeSpan resterend = TimeSpan.FromSeconds((long)seconden);
        DateTime cursor = NormaliseerStartCursor(invoermoment);
        var tijdvakken = new List<WerkTijdvak>();

        while (resterend > TimeSpan.Zero)
        {
            DateOnly datum = DateOnly.FromDateTime(cursor);
            TimeOnly tijd = TimeOnly.FromDateTime(cursor);
            bool inMiddagblok = tijd > PauzeEind;

            DateTime blokStart = datum.ToDateTime(inMiddagblok ? PauzeEind : WerkdagStart);
            TimeSpan beschikbaar = cursor - blokStart;

            if (resterend <= beschikbaar)
            {
                tijdvakken.Add(new WerkTijdvak(cursor - resterend, cursor, resterend));
                resterend = TimeSpan.Zero;
            }
            else
            {
                tijdvakken.Add(new WerkTijdvak(blokStart, cursor, beschikbaar));
                resterend -= beschikbaar;
                cursor = inMiddagblok ? datum.ToDateTime(PauzeStart) : VorigeWerkdagEind(datum);
            }
        }

        tijdvakken.Reverse();
        return tijdvakken;
    }

    private static DateTime NormaliseerStartCursor(DateTime moment)
    {
        // Eerst sub-seconden afkappen (DateTime.Now heeft tick-precisie), vóór de grenscontroles.
        moment = moment.AddTicks(-(moment.Ticks % TimeSpan.TicksPerSecond));
        DateOnly datum = DateOnly.FromDateTime(moment);
        TimeOnly tijd = TimeOnly.FromDateTime(moment);

        if (!IsWerkdag(moment.DayOfWeek) || tijd <= WerkdagStart)
            return VorigeWerkdagEind(datum);

        if (tijd > PauzeStart && tijd <= PauzeEind)
            return datum.ToDateTime(PauzeStart);

        return tijd > WerkdagEind ? datum.ToDateTime(WerkdagEind) : moment;
    }

    private static DateTime VorigeWerkdagEind(DateOnly vanafDatum)
    {
        DateOnly dag = vanafDatum;
        do
        {
            if (dag == DateOnly.MinValue)
                throw new ArgumentOutOfRangeException(nameof(vanafDatum), "Datumbereik overschreden bij terugrekenen.");
            dag = dag.AddDays(-1);
        }
        while (!IsWerkdag(dag.DayOfWeek));

        return dag.ToDateTime(WerkdagEind);
    }
}
