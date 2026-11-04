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

    /// <summary>Factor van Jeroen = werkelijke effort / eerste schatting (gemiddelde programmeur 1,67; grens 5,00).</summary>
    public static double BerekenEffortVsSchattingFactor(double schatting, double werkelijkeEffort)
    {
        ValideerPositiefEindig(schatting);
        ValideerPositiefEindig(werkelijkeEffort);

        double factor = werkelijkeEffort / schatting;
        ValideerNietNegatiefEindig(factor);
        return Math.Round(factor, 2, MidpointRounding.AwayFromZero) + 0.0;
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
