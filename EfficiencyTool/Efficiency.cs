using System.Globalization;
using System.Runtime.CompilerServices;

namespace EfficiencyTool;

/// <summary>Berekening en beoordeling van de efficiëntiefactor ten opzichte van de gemiddelde programmeur.</summary>
public static class Efficiency
{
    public const double MinimaleEfficientieFactor = 0.33333;
    public const double GemiddeldeProgrammeurFactor = 5.0 / 3.0;

    /// <summary>Geeft "Programmeur" bij factor &gt;= 0,33333, anders "Lopende band werk".</summary>
    public static string Programmeren(double efficientieFactor)
    {
        ValideerNietNegatiefEindig(efficientieFactor);
        return efficientieFactor >= MinimaleEfficientieFactor ? "Programmeur" : "Lopende band werk";
    }

    /// <summary>
    /// Efficiëntiefactor t.o.v. de gemiddelde programmeur = (schatting × 5/3) / werkelijke effort.
    /// Schatting en effort in dezelfde eenheid.
    /// </summary>
    public static double BerekenEfficientieFactor(double schatting, double werkelijkeEffort)
    {
        ValideerNietNegatiefEindig(schatting);
        ValideerPositiefEindig(werkelijkeEffort);

        double factor = schatting * GemiddeldeProgrammeurFactor / werkelijkeEffort;
        ValideerNietNegatiefEindig(factor);
        return Math.Round(factor, 5, MidpointRounding.AwayFromZero) + 0.0;
    }

    /// <summary>
    /// Tweede efficiëntiefactor: (schatting × 5/3) / (effort + extra uren). Alle waarden in uren.
    /// Extra uren zijn uren boven 6 per werkdag die BOVENOP de effort komen (niet al in de effort meegeteld).
    /// Strikter dan factor 1: schatting moet &gt; 0 zijn.
    /// </summary>
    public static double BerekenEfficientieFactorInclExtraUren(double schattingUren, double effortUren, double extraUren)
    {
        ValideerPositiefEindig(schattingUren);
        ValideerPositiefEindig(effortUren);
        ValideerNietNegatiefEindig(extraUren);

        double totaleEffort = effortUren + extraUren;
        if (!double.IsFinite(totaleEffort))
            throw new OverflowException("Totale effort is niet eindig.");
        ValideerPositiefEindig(totaleEffort);

        double factor = schattingUren * GemiddeldeProgrammeurFactor / totaleEffort;
        if (!double.IsFinite(factor))
            throw new OverflowException("Efficiëntiefactor is niet eindig.");
        return Math.Round(factor, 5, MidpointRounding.AwayFromZero) + 0.0;
    }

    public static bool TryParseGetal(string? invoer, out double waarde)
    {
        return double.TryParse(invoer?.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out waarde)
            && double.IsFinite(waarde);
    }

    private static void ValideerNietNegatiefEindig(double waarde, [CallerArgumentExpression(nameof(waarde))] string? naam = null)
    {
        if (!double.IsFinite(waarde))
            throw new ArgumentOutOfRangeException(naam, waarde, "Waarde moet een eindig getal zijn.");
        ArgumentOutOfRangeException.ThrowIfLessThan(waarde, 0.0, naam);
    }

    private static void ValideerPositiefEindig(double waarde, [CallerArgumentExpression(nameof(waarde))] string? naam = null)
    {
        if (!double.IsFinite(waarde))
            throw new ArgumentOutOfRangeException(naam, waarde, "Waarde moet een eindig getal zijn.");
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(waarde, 0.0, naam);
    }
}
