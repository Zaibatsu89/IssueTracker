using Xunit;
using System.Globalization;
using EfficiencyTool;

namespace EfficiencyTool.Tests;

public class EfficiencyTests
{
    public static TheoryData<double> OngeldigeGetallen => new()
    {
        -0.00001, double.NaN, BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8000000000000)),
        double.PositiveInfinity, double.NegativeInfinity,
    };

    [Theory]
    [InlineData(0.33333)]
    [InlineData(0.33334)]
    [InlineData(1.0)]
    [InlineData(1.66667)]
    public void Programmeren_Boven_of_op_drempel_is_Programmeur(double factor) =>
        Assert.Equal("Programmeur", Efficiency.Programmeren(factor));

    [Theory]
    [InlineData(0.333329)]
    [InlineData(0.33332)]
    [InlineData(0.0)]
    public void Programmeren_Onder_drempel_is_Lopende_band_werk(double factor) =>
        Assert.Equal("Lopende band werk", Efficiency.Programmeren(factor));

    [Fact]
    public void Programmeren_Negatieve_nul_is_Lopende_band_werk() =>
        Assert.Equal("Lopende band werk", Efficiency.Programmeren(-0.0));

    [Theory]
    [InlineData(1, 5, 0.33333)]
    [InlineData(1, 5.01, 0.33267)]
    [InlineData(1, 1.66667, 1.0)]
    [InlineData(1, 1, 1.66667)]
    [InlineData(0, 1, 0.0)]
    public void BerekenEfficientieFactor_geeft_verwachte_factor(double schatting, double effort, double verwacht) =>
        Assert.Equal(verwacht, Efficiency.BerekenEfficientieFactor(schatting, effort));

    [Fact]
    public void BerekenEfficientieFactor_negatieve_nul_wordt_zonder_min_teken_getoond()
    {
        double f = Efficiency.BerekenEfficientieFactor(-0.0, 1);
        Assert.Equal(0.0, f);
        Assert.Equal("0", f.ToString("0.#####", CultureInfo.InvariantCulture));
    }

    [Theory]
    [MemberData(nameof(OngeldigeGetallen))]
    public void Ongeldige_getallen_geven_exceptie(double w)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Efficiency.Programmeren(w));
        Assert.Throws<ArgumentOutOfRangeException>(() => Efficiency.BerekenEfficientieFactor(w, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Efficiency.BerekenEfficientieFactor(1, w));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(double.MaxValue, 1)]
    [InlineData(1, double.Epsilon)]
    public void BerekenEfficientieFactor_nul_deler_of_overflow_geeft_exceptie(double schatting, double effort) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Efficiency.BerekenEfficientieFactor(schatting, effort));

    [Theory]
    [InlineData("0,33333")]
    [InlineData("0.33333")]
    public void TryParse_accepteert_komma_en_punt(string invoer)
    {
        Assert.True(Efficiency.TryParseGetal(invoer, out double w));
        Assert.Equal(0.33333, w);
    }

    [Fact]
    public void TryParse_accepteert_min_nul() => Assert.True(Efficiency.TryParseGetal("-0", out _));

    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    [InlineData("1e500")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("abc")]
    public void TryParse_weigert_ongeldige_invoer(string? invoer) =>
        Assert.False(Efficiency.TryParseGetal(invoer, out _));

    [Theory]
    [InlineData(6, 5, 0, 2.0)]
    [InlineData(6, 5, 5, 1.0)]
    [InlineData(3, 5, 0, 1.0)]
    public void BerekenEfficientieFactorInclExtraUren_geeft_verwachte_factor(double schatting, double effort, double extra, double verwacht) =>
        Assert.Equal(verwacht, Efficiency.BerekenEfficientieFactorInclExtraUren(schatting, effort, extra));

    [Theory]
    [InlineData(0, 5, 0)]
    [InlineData(-1, 5, 0)]
    [InlineData(6, 0, 0)]
    [InlineData(6, -1, 0)]
    [InlineData(6, 5, -1)]
    [InlineData(double.NaN, 5, 0)]
    [InlineData(6, double.NaN, 0)]
    [InlineData(6, 5, double.NaN)]
    [InlineData(6, 5, double.PositiveInfinity)]
    public void BerekenEfficientieFactorInclExtraUren_weigert_ongeldige_invoer(double schatting, double effort, double extra) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Efficiency.BerekenEfficientieFactorInclExtraUren(schatting, effort, extra));
}
