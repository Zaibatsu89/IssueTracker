namespace EfficiencyTool;

/// <summary>Resultaat van het parsen van de commandoregel.</summary>
/// <param name="OpslagPad">Pad van het JSON-opslagbestand.</param>
/// <param name="Commando">Kleine letters: start, afmelden, lijst, of null als er geen subcommando is.</param>
/// <param name="RestArgs">Argumenten na het commando (bij afmelden: de optionele Jira-key).</param>
public sealed record GeparseerdeArgumenten(string OpslagPad, string? Commando, IReadOnlyList<string> RestArgs);

/// <summary>Strikte, testbare routing van de commandoregel: globale <c>--bestand &lt;pad&gt;</c> op elke positie, daarna het commando.</summary>
public static class CliArgumenten
{
    /// <exception cref="ArgumentException">Bij een ongeldig <c>--bestand</c>, onbekende optie of commando, of overtollige argumenten.</exception>
    public static GeparseerdeArgumenten Parse(string[] args, string? standaardPad = null)
    {
        ArgumentNullException.ThrowIfNull(args);

        string opslagPad = standaardPad ?? ItemRegistratie.StandaardPad;
        var overig = new List<string>();

        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--bestand", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length || string.IsNullOrWhiteSpace(args[i + 1]) || args[i + 1].StartsWith("--", StringComparison.Ordinal))
                    throw new ArgumentException("--bestand vereist een pad.");
                opslagPad = args[++i];
            }
            else
            {
                overig.Add(args[i]);
            }
        }

        if (overig.Count == 0)
            return new GeparseerdeArgumenten(opslagPad, null, []);

        string eerste = overig[0];

        if (string.Equals(eerste, "start", StringComparison.OrdinalIgnoreCase) || string.Equals(eerste, "lijst", StringComparison.OrdinalIgnoreCase))
        {
            if (overig.Count > 1)
                throw new ArgumentException($"Onbekende optie of commando: {overig[1]}");
            return new GeparseerdeArgumenten(opslagPad, eerste.ToLowerInvariant(), []);
        }

        if (string.Equals(eerste, "afmelden", StringComparison.OrdinalIgnoreCase))
        {
            if (overig.Count > 2 || (overig.Count == 2 && overig[1].StartsWith("--", StringComparison.Ordinal)))
                throw new ArgumentException($"Onbekende optie of commando: {overig[^1]}");
            return new GeparseerdeArgumenten(opslagPad, "afmelden", overig.Skip(1).ToList());
        }

        throw new ArgumentException($"Onbekende optie of commando: {eerste}");
    }
}
