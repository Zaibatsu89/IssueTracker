namespace EfficiencyTool;

/// <summary>
/// Berekening van gewerkte roosteruren over het werkrooster
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

    /// <summary>Geeft aan of de dag een roosterwerkdag is (ma, di, do, vr).</summary>
    public static bool IsWerkdag(DayOfWeek dag) =>
        dag is DayOfWeek.Monday or DayOfWeek.Tuesday or DayOfWeek.Thursday or DayOfWeek.Friday;

    /// <summary>
    /// Roosteruren tussen <paramref name="start"/> en <paramref name="eind"/> (lokale kloktijd, geen tijdzone):
    /// intersectie met de werkblokken 09:00-12:30 en 13:00-15:30 op ma/di/do/vr.
    /// </summary>
    /// <exception cref="ArgumentException">Als <paramref name="eind"/> niet na <paramref name="start"/> ligt.</exception>
    public static double BerekenRoosterUren(DateTime start, DateTime eind)
    {
        if (eind <= start)
            throw new ArgumentException("Eindtijd moet na de starttijd liggen.", nameof(eind));

        double minuten = 0.0;
        for (DateTime dag = start.Date; ; dag = dag.AddDays(1))
        {
            if (IsWerkdag(dag.DayOfWeek))
            {
                minuten += BerekenOverlapMinuten(start, eind, dag + WerkdagStart.ToTimeSpan(), dag + PauzeStart.ToTimeSpan());
                minuten += BerekenOverlapMinuten(start, eind, dag + PauzeEind.ToTimeSpan(), dag + WerkdagEind.ToTimeSpan());
            }
            if (dag >= eind.Date)
                break;
        }

        return Math.Round(minuten / 60.0, 5, MidpointRounding.AwayFromZero);
    }

    private static double BerekenOverlapMinuten(DateTime start, DateTime eind, DateTime blokStart, DateTime blokEind)
    {
        DateTime van = start > blokStart ? start : blokStart;
        DateTime tot = eind < blokEind ? eind : blokEind;
        return tot > van ? (tot - van).TotalMinutes : 0.0;
    }
}
