using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace EfficiencyTool;

public static class WordDocumentGenerator
{
    public static void Genereer(string pad)
    {
        string volledigPad = Path.GetFullPath(pad);
        if (Path.GetDirectoryName(volledigPad) is { Length: > 0 } map)
            Directory.CreateDirectory(map);

        using var doc = WordprocessingDocument.Create(volledigPad, WordprocessingDocumentType.Document);
        var main = doc.AddMainDocumentPart();
        var body = new Body();
        main.Document = new Document(body);

        body.Append(Kop("Opdracht: efficiëntiefactor en functie Programmeren", 32));
        body.Append(Tekst("Maak een C#-consoleapplicatie met de functie Programmeren(double efficientieFactor). " +
            "De parameter is mijn efficiëntiefactor; de returnwaarde is \"Programmeur\" of \"Lopende band werk\"."));

        body.Append(Kop("Definitie van de efficiëntiefactor", 26));
        body.Append(Tekst("Invoer: de eerste schatting van het werkitem en de werkelijke effort (alle iteraties tot goedkeuring en check-in), in dezelfde eenheid."));
        body.Append(Tekst("Werkelijke effort vs. schatting factor = werkelijke effort / eerste schatting. Eigen schatting precies halen = 1,00; een gemiddelde programmeur = 1,67."));
        body.Append(Tekst("Efficiëntiefactor (ten opzichte van de gemiddelde programmeur) = (schatting × 1,67) / werkelijke effort, afgerond op 5 decimalen."));

        body.Append(Kop("Beoordeling", 26));
        body.Append(Tekst("Minimale efficiëntiefactor = 0,33333 (= 1,67 / 5,00). Werkelijke effort vs. schatting factor ≤ 5,00: \"Programmeur\"; daarboven: \"Lopende band werk\"."));
        body.Append(Tekst("Gevolg: maximaal 3× de tijd van een gemiddelde programmeur, oftewel maximaal 5× (vijf keer) de eerste schatting."));
        body.Append(Tekst("Voorbeeld: 1 dag geschat en 5 dagen besteed geeft factor 5,00 en efficiëntiefactor 0,33333 = Programmeur."));

        main.Document.Save();
    }

    private static Paragraph Kop(string tekst, int halvePunten) =>
        new(new Run(new RunProperties(new Bold(), new FontSize { Val = halvePunten.ToString() }), new Text(tekst)));

    private static Paragraph Tekst(string tekst) =>
        new(new Run(new Text(tekst) { Space = SpaceProcessingModeValues.Preserve }));
}
