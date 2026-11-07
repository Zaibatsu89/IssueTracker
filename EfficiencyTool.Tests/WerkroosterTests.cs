using Xunit;
using EfficiencyTool;

namespace EfficiencyTool.Tests;

public class WerkroosterTests
{
    // 2026-11-02 is een maandag; 9999-12-31 is een vrijdag.
    [Theory]
    [InlineData(2026, 11, 2, 9, 0, 2026, 11, 2, 11, 30, 2.5)]
    [InlineData(2026, 11, 2, 11, 30, 2026, 11, 2, 14, 0, 2.0)]
    [InlineData(2026, 11, 2, 8, 0, 2026, 11, 2, 18, 0, 6.0)]
    [InlineData(2026, 11, 3, 13, 0, 2026, 11, 5, 12, 30, 6.0)]
    [InlineData(2026, 11, 4, 9, 0, 2026, 11, 4, 15, 30, 0.0)]
    [InlineData(9999, 12, 31, 9, 0, 9999, 12, 31, 15, 30, 6.0)]
    public void BerekenRoosterUren_geeft_verwachte_uren(int j1, int m1, int d1, int u1, int mi1, int j2, int m2, int d2, int u2, int mi2, double verwacht) =>
        Assert.Equal(verwacht, Werkrooster.BerekenRoosterUren(new DateTime(j1, m1, d1, u1, mi1, 0), new DateTime(j2, m2, d2, u2, mi2, 0)));

    [Fact]
    public void BerekenRoosterUren_eind_voor_of_gelijk_aan_start_geeft_exception()
    {
        var t = new DateTime(2026, 11, 2, 10, 0, 0);
        Assert.Throws<ArgumentException>(() => Werkrooster.BerekenRoosterUren(t, t));
        Assert.Throws<ArgumentException>(() => Werkrooster.BerekenRoosterUren(t, t.AddHours(-1)));
    }
}
