using Xunit;
using EfficiencyTool;

namespace EfficiencyTool.Tests;

public class WerkroosterTests
{
    // 2026-11-02 is een maandag. Rooster: 09:00-12:30 (3,5 u) en 13:00-15:30 (2,5 u) = 6,0 u/dag, 24,0 u/week.
    public static TheoryData<DateTime, double, DateTime> StartpuntGrenswaarden => new()
    {
        // Binnen het middagblok: vr 15:30 + 2 u -> vr 13:30
        { new DateTime(2026, 11, 6, 15, 30, 0), 2.0, new DateTime(2026, 11, 6, 13, 30, 0) },
        // Exact het middagblok: vr 15:30 + 2,5 u -> vr 13:00
        { new DateTime(2026, 11, 6, 15, 30, 0), 2.5, new DateTime(2026, 11, 6, 13, 0, 0) },
        // Over de pauze heen: vr 15:30 + 3 u -> 2,5 u middag + 0,5 u ochtend -> vr 12:00
        { new DateTime(2026, 11, 6, 15, 30, 0), 3.0, new DateTime(2026, 11, 6, 12, 0, 0) },
        // Exact één werkdag: vr 15:30 + 6,0 u -> vr 09:00 (niet do 15:30)
        { new DateTime(2026, 11, 6, 15, 30, 0), 6.0, new DateTime(2026, 11, 6, 9, 0, 0) },
        // 1 seconde over een werkdag -> do 15:29:59
        { new DateTime(2026, 11, 6, 15, 30, 0), 6.0 + 1.0 / 3600.0, new DateTime(2026, 11, 5, 15, 29, 59) },
        // Invoermoment in de pauze (12:45) en exact einde pauze (13:00) + 1 u -> 11:30
        { new DateTime(2026, 11, 6, 12, 45, 0), 1.0, new DateTime(2026, 11, 6, 11, 30, 0) },
        { new DateTime(2026, 11, 6, 13, 0, 0), 1.0, new DateTime(2026, 11, 6, 11, 30, 0) },
        // Op 09:00 donderdag + 1 u -> woensdag overslaan -> di 14:30
        { new DateTime(2026, 11, 5, 9, 0, 0), 1.0, new DateTime(2026, 11, 3, 14, 30, 0) },
        // Vóór 09:00 dinsdag (07:45) + 1,5 u -> ma 14:00
        { new DateTime(2026, 11, 3, 7, 45, 0), 1.5, new DateTime(2026, 11, 2, 14, 0, 0) },
        // Ná 15:30 maandag (18:15) + 6,0 u -> ma 09:00
        { new DateTime(2026, 11, 2, 18, 15, 0), 6.0, new DateTime(2026, 11, 2, 9, 0, 0) },
        // Vrije dagen: wo 12:00 -> di 13:30; za 10:00 / zo 22:00 -> vr 13:30
        { new DateTime(2026, 11, 4, 12, 0, 0), 2.0, new DateTime(2026, 11, 3, 13, 30, 0) },
        { new DateTime(2026, 11, 7, 10, 0, 0), 2.0, new DateTime(2026, 11, 6, 13, 30, 0) },
        { new DateTime(2026, 11, 8, 22, 0, 0), 2.0, new DateTime(2026, 11, 6, 13, 30, 0) },
        // Meerdere weken: 28 u vanaf vr 6 nov 15:30 = 24 u (ma 2 t/m vr 6 nov) + 4 u op vr 30 okt -> 11:00
        { new DateTime(2026, 11, 6, 15, 30, 0), 28.0, new DateTime(2026, 10, 30, 11, 0, 0) },
        // Sub-seconden worden afgekapt: 09:00:00.500 telt als 09:00:00 (vorige werkdag).
        { new DateTime(2026, 11, 3, 9, 0, 0).AddMilliseconds(500), 1.0, new DateTime(2026, 11, 2, 14, 30, 0) },
    };

    [Theory]
    [MemberData(nameof(StartpuntGrenswaarden))]
    public void BerekenStartpunt_Grenswaarden_GeeftVerwachtStartpunt(DateTime invoermoment, double effortUren, DateTime verwacht)
    {
        Assert.Equal(verwacht, Werkrooster.BerekenStartpunt(invoermoment, effortUren));
    }

    [Theory]
    [InlineData(2.0)]
    [InlineData(6.0)]
    [InlineData(10.25)]
    [InlineData(28.0)]
    public void BerekenWeekoverzicht_GeldigeEffort_VoldoetAanRoosterInvarianten(double effortUren)
    {
        // Met milliseconden: sub-seconden mogen de som niet beïnvloeden.
        DateTime invoermoment = new DateTime(2026, 11, 6, 14, 15, 0).AddMilliseconds(600);

        IReadOnlyList<WerkTijdvak> overzicht = Werkrooster.BerekenWeekoverzicht(invoermoment, effortUren);

        long verwachteSeconden = (long)Math.Round(effortUren * 3600.0, MidpointRounding.AwayFromZero);
        Assert.Equal(TimeSpan.FromSeconds(verwachteSeconden), overzicht.Aggregate(TimeSpan.Zero, (som, t) => som + t.Duur));
        Assert.Equal(Werkrooster.BerekenStartpunt(invoermoment, effortUren), overzicht[0].Start);
        Assert.All(overzicht, t =>
        {
            Assert.True(Werkrooster.IsWerkdag(t.Start.DayOfWeek));
            Assert.Equal(t.Start.Date, t.Eind.Date);
            TimeOnly startTijd = TimeOnly.FromDateTime(t.Start);
            TimeOnly eindTijd = TimeOnly.FromDateTime(t.Eind);
            bool inOchtend = startTijd >= Werkrooster.WerkdagStart && eindTijd <= Werkrooster.PauzeStart;
            bool inMiddag = startTijd >= Werkrooster.PauzeEind && eindTijd <= Werkrooster.WerkdagEind;
            Assert.True(inOchtend || inMiddag);
            Assert.Equal(t.Eind - t.Start, t.Duur);
        });
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.5)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(0.0001)]
    [InlineData(100_000.01)]
    [InlineData(double.MaxValue)]
    public void BerekenWeekoverzicht_OngeldigeEffort_GooitArgumentOutOfRangeException(double effortUren)
    {
        var invoermoment = new DateTime(2026, 11, 6, 15, 30, 0);

        Assert.Throws<ArgumentOutOfRangeException>(() => Werkrooster.BerekenWeekoverzicht(invoermoment, effortUren));
    }
}
