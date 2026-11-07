using System.Text.Json;
using Xunit;
using EfficiencyTool;

namespace EfficiencyTool.Tests;

public class ItemRegistratieTests : IDisposable
{
    private readonly string _map = Path.Combine(Path.GetTempPath(), "EfficiencyToolTests-" + Guid.NewGuid().ToString("N"));
    private string Pad => Path.Combine(_map, "items.json");

    private static readonly DateTime Maandag0900 = new(2026, 11, 2, 9, 0, 0);
    private static readonly DateTime Maandag1530 = new(2026, 11, 2, 15, 30, 0);

    public void Dispose()
    {
        if (Directory.Exists(_map))
            Directory.Delete(_map, true);
    }

    [Fact]
    public void Leesacties_op_niet_bestaande_map_geven_lege_lijst_en_maken_geen_map_of_lock_aan()
    {
        Assert.False(Directory.Exists(_map));

        Assert.Empty(ItemRegistratie.LaadAlles(Pad));
        Assert.Empty(ItemRegistratie.LaadOpenstaand(Pad));

        Assert.False(Directory.Exists(_map));
        Assert.False(File.Exists(Pad + ".lock"));
    }

    [Fact]
    public void Start_en_afmelding_roundtrip_berekent_roosteruren_intern_en_behoudt_historie()
    {
        ItemRegistratie.RegistreerStart(Pad, "  ABC-1  ", 6, Maandag0900);
        WerkItem klaar = ItemRegistratie.RegistreerAfmelding(Pad, "abc-1", Maandag1530, 5, 5);

        double verwachteRoosterUren = Werkrooster.BerekenRoosterUren(Maandag0900, Maandag1530);
        WerkItem geladen = Assert.Single(ItemRegistratie.LaadAlles(Pad));

        Assert.Equal(klaar, geladen);
        Assert.Equal("ABC-1", geladen.Key);
        Assert.Equal(verwachteRoosterUren, geladen.RoosterUren);
        Assert.Equal(6.0, geladen.RoosterUren);
        Assert.Equal(2.0, geladen.EfficientieFactor);
        Assert.Equal(1.0, geladen.EfficientieFactorInclExtraUren);
        Assert.Equal(ItemRegistratie.HuidigeBerekeningsVersie, geladen.BerekeningsVersie);
        Assert.Equal(DateTimeKind.Unspecified, geladen.Start.Kind);
        Assert.Empty(ItemRegistratie.LaadOpenstaand(Pad));
        Assert.True(File.Exists(Pad + ".lock"));
        Assert.False(File.Exists(Pad + ".tmp"));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-2.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void RegistreerStart_ongeldige_schatting_gooit_ArgumentOutOfRangeException(double schatting)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ItemRegistratie.RegistreerStart(Pad, "ABC-1", schatting, Maandag0900));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RegistreerStart_lege_of_whitespace_key_gooit_ArgumentException(string legeKey)
    {
        Assert.Throws<ArgumentException>(() =>
            ItemRegistratie.RegistreerStart(Pad, legeKey, 6, Maandag0900));
    }

    [Fact]
    public void Dubbele_openstaande_key_ook_met_spaties_en_andere_hoofdletters_wordt_geweigerd()
    {
        WerkItem gestart = ItemRegistratie.RegistreerStart(Pad, "  ABC-1  ", 6, Maandag0900);
        Assert.Equal("ABC-1", gestart.Key);

        Assert.Throws<InvalidOperationException>(() => ItemRegistratie.RegistreerStart(Pad, "abc-1", 3, Maandag0900));
        Assert.Throws<InvalidOperationException>(() => ItemRegistratie.RegistreerStart(Pad, "  abc-1 ", 3, Maandag0900));
    }

    [Fact]
    public void Afmelden_onbekend_of_afgerond_item_geeft_fout()
    {
        Assert.Throws<InvalidOperationException>(() => ItemRegistratie.RegistreerAfmelding(Pad, "X-1", Maandag1530, 5, 0));
        ItemRegistratie.RegistreerStart(Pad, "ABC-1", 6, Maandag0900);
        ItemRegistratie.RegistreerAfmelding(Pad, "ABC-1", Maandag1530, 5, 0);
        Assert.Throws<InvalidOperationException>(() => ItemRegistratie.RegistreerAfmelding(Pad, "ABC-1", Maandag1530, 5, 0));
    }

    [Fact]
    public void Afmelden_met_ongeldige_waarden_laat_item_open()
    {
        ItemRegistratie.RegistreerStart(Pad, "ABC-1", 6, Maandag0900);
        Assert.Throws<ArgumentException>(() => ItemRegistratie.RegistreerAfmelding(Pad, "ABC-1", Maandag0900.AddHours(-1), 5, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemRegistratie.RegistreerAfmelding(Pad, "ABC-1", Maandag1530, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemRegistratie.RegistreerAfmelding(Pad, "ABC-1", Maandag1530, 5, double.NaN));
        Assert.Single(ItemRegistratie.LaadOpenstaand(Pad));
    }

    [Fact]
    public void Beschadigde_json_blijft_byte_identiek_na_mislukte_start_en_afmelding()
    {
        Directory.CreateDirectory(_map);
        byte[] corrupteBytes = "{ dit is geen geldige JSON ["u8.ToArray();
        File.WriteAllBytes(Pad, corrupteBytes);

        Assert.ThrowsAny<JsonException>(() => ItemRegistratie.RegistreerStart(Pad, "ABC-1", 6, Maandag0900));
        Assert.Equal(corrupteBytes, File.ReadAllBytes(Pad));

        Assert.ThrowsAny<JsonException>(() => ItemRegistratie.RegistreerAfmelding(Pad, "ABC-1", Maandag1530, 5, 0));
        Assert.Equal(corrupteBytes, File.ReadAllBytes(Pad));

        Assert.ThrowsAny<JsonException>(() => ItemRegistratie.LaadOpenstaand(Pad));
        Assert.Equal(corrupteBytes, File.ReadAllBytes(Pad));
    }

    [Fact]
    public void Relatief_pad_met_dot_segmenten_wordt_genormaliseerd()
    {
        string submap = Path.Combine(_map, "sub");
        Directory.CreateDirectory(submap);
        string nietGenormaliseerdPad = Path.Combine(submap, "..", "items.json");

        ItemRegistratie.RegistreerStart(nietGenormaliseerdPad, "REL-1", 4, Maandag0900);

        Assert.True(File.Exists(Pad));
        Assert.True(File.Exists(Pad + ".lock"));
        Assert.Single(ItemRegistratie.LaadOpenstaand(Pad));
    }

    [Fact]
    public void Mislukte_schrijfpoging_laat_bestaand_bestand_ongemoeid()
    {
        ItemRegistratie.RegistreerStart(Pad, "ABC-1", 6, Maandag0900);
        string voor = File.ReadAllText(Pad);

        using (new FileStream(Pad + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
        {
            Assert.Throws<IOException>(() => ItemRegistratie.SlaOp(Pad, []));
        }

        Assert.Equal(voor, File.ReadAllText(Pad));
    }

    [Fact]
    public void Tweede_lock_poging_faalt_zolang_eerste_lock_actief_is_en_lockbestand_blijft_bestaan()
    {
        using (ItemRegistratie.VerkrijgBestandsLock(Pad))
        {
            Assert.Throws<IOException>(() => ItemRegistratie.VerkrijgBestandsLock(Pad, pogingen: 2, wachtMs: 1));
        }

        Assert.True(File.Exists(Pad + ".lock"));
        using (ItemRegistratie.VerkrijgBestandsLock(Pad)) { }
    }

    [Fact]
    public void Pad_dat_een_map_is_geeft_fout()
    {
        Directory.CreateDirectory(Pad);
        Assert.Throws<IOException>(() => ItemRegistratie.RegistreerStart(Pad, "ABC-1", 6, Maandag0900));
        Assert.Throws<IOException>(() => ItemRegistratie.LaadAlles(Pad));
    }

    [Fact]
    public void SlaOp_is_internal_en_niet_public()
    {
        var methode = typeof(ItemRegistratie).GetMethod(
            nameof(ItemRegistratie.SlaOp),
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        Assert.NotNull(methode);
        Assert.False(methode.IsPublic);
        Assert.True(methode.IsAssembly);
    }

    [Fact]
    public void VerkrijgBestandsLock_is_internal_en_niet_public()
    {
        var methode = typeof(ItemRegistratie).GetMethod(
            nameof(ItemRegistratie.VerkrijgBestandsLock),
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        Assert.NotNull(methode);
        Assert.False(methode.IsPublic);
        Assert.True(methode.IsAssembly);
    }

    [Fact]
    public void Roosteruren_op_woensdag_zijn_nul() =>
        Assert.Equal(0.0, Werkrooster.BerekenRoosterUren(new DateTime(2026, 5, 6, 9, 0, 0), new DateTime(2026, 5, 6, 15, 30, 0)));

    [Fact]
    public async Task Concurrency_parallelle_starts_en_laadOpenstaand_behouden_alle_items_zonder_corruptie()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;

        for (int i = 0; i < 20; i++)
        {
            int idx = i;
            Task t1 = Task.Run(() => ItemRegistratie.RegistreerStart(Pad, $"KEY-A-{idx}", 4, Maandag0900), ct);
            Task t2 = Task.Run(() => ItemRegistratie.RegistreerStart(Pad, $"KEY-B-{idx}", 6, Maandag0900), ct);
            Task<List<WerkItem>> tLees = Task.Run(() => ItemRegistratie.LaadOpenstaand(Pad), ct);

            await Task.WhenAll(t1, t2, tLees);
        }

        List<WerkItem> open = ItemRegistratie.LaadOpenstaand(Pad);
        Assert.Equal(40, open.Count);
        Assert.False(File.Exists(Pad + ".tmp"));
        Assert.True(File.Exists(Pad + ".lock"));
    }

    [Fact]
    public async Task Concurrency_twee_parallelle_afmeldingen_op_zelfde_item_precies_een_slaagt()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;

        for (int i = 0; i < 10; i++)
        {
            string key = $"RACE-{i}";
            ItemRegistratie.RegistreerStart(Pad, key, 6, Maandag0900);

            Task<Exception?> t1 = Task.Run(() => Record.Exception(() =>
                ItemRegistratie.RegistreerAfmelding(Pad, key, Maandag1530, 5, 0)), ct);
            Task<Exception?> t2 = Task.Run(() => Record.Exception(() =>
                ItemRegistratie.RegistreerAfmelding(Pad, key, Maandag1530, 5, 0)), ct);

            Exception?[] fouten = await Task.WhenAll(t1, t2);

            Assert.Single(fouten, e => e is null);
            Exception fout = Assert.Single(fouten, e => e is not null)!;
            Assert.IsType<InvalidOperationException>(fout);
        }

        List<WerkItem> alles = ItemRegistratie.LaadAlles(Pad);
        Assert.Equal(10, alles.Count);
        Assert.All(alles, item => Assert.NotNull(item.Eind));
        Assert.Empty(ItemRegistratie.LaadOpenstaand(Pad));
        Assert.False(File.Exists(Pad + ".tmp"));
        Assert.True(File.Exists(Pad + ".lock"));
    }

    [Fact]
    public void Cli_routing_met_bestand_op_elke_positie()
    {
        GeparseerdeArgumenten a = CliArgumenten.Parse([], "std");
        Assert.Equal(("std", (string?)null), (a.OpslagPad, a.Commando));
        Assert.Empty(a.RestArgs);

        a = CliArgumenten.Parse(["start", "--bestand", "x.json"], "std");
        Assert.Equal(("x.json", "start"), (a.OpslagPad, a.Commando));

        a = CliArgumenten.Parse(["--bestand", "x.json", "start"], "std");
        Assert.Equal(("x.json", "start"), (a.OpslagPad, a.Commando));

        a = CliArgumenten.Parse(["afmelden", "ABC-123", "--bestand", "x.json"], "std");
        Assert.Equal("afmelden", a.Commando);
        Assert.Equal(["ABC-123"], a.RestArgs);

        a = CliArgumenten.Parse(["LIJST"], "std");
        Assert.Equal(("std", "lijst"), (a.OpslagPad, a.Commando));
    }

    public static TheoryData<string[]> OngeldigeCliArgumenten => new()
    {
        new[] { "start", "--bestand" },
        new[] { "--bestand", "--x" },
        new[] { "--onbekend", "start" },
        new[] { "start", "--onbekend" },
        new[] { "start", "extra" },
        new[] { "lijst", "extra" },
        new[] { "afmelden", "A", "B" },
        new[] { "afmelden", "--onbekend" },
        new[] { "onbekendwoord" },
        new[] { "--docx" },
    };

    [Theory]
    [MemberData(nameof(OngeldigeCliArgumenten))]
    public void Cli_ongeldige_of_onbekende_argumenten_geven_ArgumentException(string[] args)
    {
        Assert.Throws<ArgumentException>(() => CliArgumenten.Parse(args, "std"));
    }
}
