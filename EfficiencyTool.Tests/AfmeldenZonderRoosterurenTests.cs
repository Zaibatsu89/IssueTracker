using System.Diagnostics;
using Xunit;
using EfficiencyTool;

namespace EfficiencyTool.Tests;

/// <summary>Controleert het CLI-pad voor afmelden zonder roosteruren (<c>roosterUren &lt;= 0.0</c> in <c>Program.cs</c>).</summary>
public class AfmeldenZonderRoosterurenTests
{
    // 2026-11-09 is een maandag; het rooster begint om 09:00.
    [Fact]
    public async Task Afmelden_zonder_roosteruren_toont_duidelijke_melding_en_laat_item_open()
    {
        string map = Path.Combine(Path.GetTempPath(), "EfficiencyTool.Tests-" + Guid.NewGuid().ToString("N"));
        string pad = Path.Combine(map, "items.json");
        try
        {
            ItemRegistratie.RegistreerStart(pad, "FFA-9", 0.17, new DateTime(2026, 11, 9, 8, 45, 0));

            var info = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "EfficiencyTool.exe"))
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            info.ArgumentList.Add("afmelden");
            info.ArgumentList.Add("FFA-9");
            info.ArgumentList.Add("--bestand");
            info.ArgumentList.Add(pad);

            using var proces = Process.Start(info) ?? throw new InvalidOperationException("Proces kon niet worden gestart.");
            var ct = TestContext.Current.CancellationToken;
            await proces.StandardInput.WriteLineAsync("2026-11-09 08:57".AsMemory(), ct);
            proces.StandardInput.Close();
            string uitvoer = await proces.StandardOutput.ReadToEndAsync(ct);
            await proces.WaitForExitAsync(ct);

            Assert.Equal(1, proces.ExitCode);
            Assert.Contains("Fout: Geen roosteruren tussen start (2026-11-09 08:45) en afmelding (2026-11-09 08:57); afmelden is niet mogelijk.", uitvoer);
            Assert.Contains("ma, di, do en vr 09:00-12:30 en 13:00-15:30", uitvoer);
            Assert.Contains("(item blijft open)", uitvoer);
            Assert.Single(ItemRegistratie.LaadOpenstaand(pad));
        }
        finally
        {
            if (Directory.Exists(map))
                Directory.Delete(map, recursive: true);
        }
    }
}
