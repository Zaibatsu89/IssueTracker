using Xunit;

namespace EfficiencyTool.Tests;

/// <summary>Controleert dat <c>VoerLijstUit</c> argumentfouten afvangt (de CLI-parser vangt een leeg pad zelf al eerder af).</summary>
[Collection("ConsoleEnExitCode")]
public class LijstFoutafhandelingTests
{
    [Fact]
    public void VoerLijstUit_met_leeg_pad_toont_fout_en_zet_exitcode_1()
    {
        var oudeUitvoer = Console.Out;
        int oudeExitCode = Environment.ExitCode;
        using var uitvoer = new StringWriter();
        try
        {
            Console.SetOut(uitvoer);
            Environment.ExitCode = 0;

            Program.VoerLijstUit("");

            Assert.Equal(1, Environment.ExitCode);
            Assert.Contains("Fout: Opslagpad mag niet leeg zijn.", uitvoer.ToString());
        }
        finally
        {
            Console.SetOut(oudeUitvoer);
            Environment.ExitCode = oudeExitCode;
        }
    }
}
