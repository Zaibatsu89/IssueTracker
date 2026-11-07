# Issue Tracker

Deze repository bevat procesflowcharts, Word-brondocumenten en tools voor het
Issue Tracker-proces. De hoofdapplicatie `IssueTrackerTool` begeleidt lokale
processtappen en genereert Word-rapporten. Daarnaast zijn er een WinUI-prototype,
werkboektooling en een Draw.io-naar-Mermaid-converter.

## Repositoryoverzicht

- **Genummerde `.drawio`-bestanden:** bronflowcharts voor de 12 procesfasen;
  bijbehorende `.mermaid`-bestanden en enkele SVG-weergaven staan in de hoofdmap.
- **[Actielijst.docx](Actielijst.docx) en [Checklijst.docx](Checklijst.docx):**
  brondocumenten voor rapportgeneratie; ook
  [IssueTracker_Stappenplan.docx](IssueTracker_Stappenplan.docx) is beschikbaar.
- **[IssueTrackerTool](IssueTrackerTool/IssueTrackerTool.csproj):** Windows
  Forms-applicatie op .NET Framework 4.7.2, met GUI en headless rapportgeneratie
  via OpenXML 2.20.0.
- **[JiraIssueTracker](JiraIssueTracker/JiraIssueTracker.csproj):**
  WinUI/MSIX-prototype op .NET 10 met sampledata; geen live Jira-client.
- **[EfficiencyTool](EfficiencyTool/EfficiencyTool.csproj):** .NET 10-consoleapp
  voor `start`, `lijst` en `afmelden` van werkitems met twee efficiëntiefactoren; zie
  [EfficiencyTool](#efficiencytool-efficiëntiefactor).
- **[Megalijst](Megalijst/README.md):** `WorkbookTextGuard` voor inventarisatie,
  controle, opschoning en herberekening van `.xlsx`-werkboeken.
- **[Fase-3-tooling](Megalijst/phase3-tooling/README.md):** snapshotgebonden
  operator-tooling met afzonderlijke goedkeurings- en verificatiegrenzen.
- **[drawio_to_mermaid.py](drawio_to_mermaid.py):** Python 3-helper die een
  Draw.io-bestand omzet naar Mermaid op standaarduitvoer.

## IssueTrackerTool: bouwen en starten

### Vereisten (Windows)

- Visual Studio met de workload `.NET desktop development`, of overeenkomstige
  Visual Studio Build Tools met MSBuild.
- .NET Framework 4.7.2-targeting pack voor het bouwen en een geschikte
  .NET Framework-runtime voor het uitvoeren.
- NuGet-restore voor de afhankelijkheden uit
  [IssueTrackerTool.csproj](IssueTrackerTool/IssueTrackerTool.csproj).
- Een checkout met de Word-brondocumenten en `.drawio`-bestanden in de hoofdmap.

### Bouwen

Voer vanuit de repositoryroot in een **Developer PowerShell** of
**Developer Command Prompt** van Visual Studio uit:

```powershell
MSBuild.exe IssueTrackerTool/IssueTrackerTool.csproj /restore /p:Configuration=Debug /p:Platform=AnyCPU
```

Het standaardpad van het gecompileerde bestand is:

```text
IssueTrackerTool/bin/Debug/IssueTrackerTool.exe
```

Gebruik voor een Release-build `Configuration=Release`; de uitvoer komt dan in
`IssueTrackerTool/bin/Release/`. De aparte oplossing is
[IssueTrackerTool.slnx](IssueTrackerTool/IssueTrackerTool.slnx).

### Bronbestanden en projectmap-zoeklogica

De rapportgenerator zoekt vanaf **de eigen binaire locatie**
(`AppDomain.CurrentDomain.BaseDirectory`) naar boven in de directorystructuur.
De eerste map met `Actielijst.docx` wordt als projectroot gebruikt.
**Het huidige werkpad (Working Directory) is niet het startpunt van deze zoekactie.**

In die gevonden map moet ook `Checklijst.docx` staan. De generator zoekt daar
tevens de genummerde `.drawio`-bestanden. Ontbrekende Word-brondocumenten leiden
tot een fout; ontbrekende of onleesbare flowcharts kunnen zonder foutmelding
worden overgeslagen.

Laat de executable en zijn afhankelijkheden daarom bij voorkeur in de buildmap
onder de repositoryroot staan. Een verplaatste executable kan de bronbestanden
niet terugvinden door alleen het werkpad naar de repositoryroot te wijzigen.
Als geen map met `Actielijst.docx` wordt gevonden, gebruikt de generator de
binaire map als fallback.

### Interactieve GUI (Windows)

Start zonder argumenten vanuit de repositoryroot:

```powershell
./IssueTrackerTool/bin/Debug/IssueTrackerTool.exe
```

De GUI biedt:

- Faseselectie voor Analyse, Ontwerp, Implementatie, Test en Special action.
- Een afteltimer met standaard 60 seconden per stap.
- Lokale registratie van acties, tijdstippen en ingevoerde details in
  `WitTronics_AuditLog.txt`, **naast de executable**. Die map moet schrijfbaar zijn.
- Een knop **Genereer Rapport** voor een gecombineerd Word-procesrapport.

De GUI gebruikt eigen, in code vastgelegde actielijsten; de faseselectie is geen
algemene uitvoeringsengine voor de volledige Draw.io-workflow.

### Headless rapportgeneratie

Gebruik vanuit de repositoryroot:

```powershell
./IssueTrackerTool/bin/Debug/IssueTrackerTool.exe --test-generate "TEST-123" "C:/Reports/TestOutput.docx"
```

Vervang het uitvoerpad door een pad in een bestaande, schrijfbare map en kies
een nieuw bestand om onbedoeld overschrijven te vermijden.

De argumenten zijn `--test-generate`, een issue-aanduiding en het pad van het
uitvoerbestand. De issue-aanduiding wordt in het rapport opgenomen:
**deze opdracht haalt geen issue op via een Jira-API**.
De entrypoint retourneert exitcode `0` bij geslaagde generatie en `1` bij een
gevangen generatiefout. Gebruik de vlag met beide argumenten; anders valt de
entrypoint terug op GUI-start.

**Linux/Mono:** een alternatieve headless aanroep is:

```bash
mono IssueTrackerTool/bin/Debug/IssueTrackerTool.exe --test-generate "TEST-123" "/pad/naar/TestOutput.docx"
```

Deze route is hier niet als geteste platformondersteuning bevestigd. Mono en
compatibele .NET Framework-/Windows Forms-afhankelijkheden moeten beschikbaar
zijn; er is geen native Linux-GUI-ondersteuning toegezegd.

## Rapportgeneratie en parsergrenzen

De generator:

1. Opent `Actielijst.docx` en `Checklijst.docx` read-only en leest hun alineateksten.
2. Groepeert acties en checks en doorloopt de 12 in code vastgelegde procesfasen.
3. Leest uit de flowcharts `Action`-/labelkoppelingen en koppelt deze aan
   documentkoppen via codes en tekstnormalisatie.
4. Schrijft een nieuw OpenXML Word-rapport met fasen, acties en bijbehorende checks.

De bron-Word-documenten worden hierbij **niet bijgewerkt**. Het rapport is een
samengestelde documentweergave, geen bewijs dat de workflow volledig is doorlopen
of dat alle bronmappings geldig zijn.

### Geautomatiseerde tool-inspectie

De Draw.io-parser in `IssueTrackerTool` doet het volgende:

- Leest XML-`object`-elementen met `Action` en `label`; HTML wordt gedecodeerd,
  tags worden verwijderd en witruimte wordt genormaliseerd.
- Geeft een consolewaarschuwing wanneer een niet-lege Action ID eindigt met
  een letter anders dan de hoofdletter `T` of `F`. Dit blokkeert generatie niet.
- Leest als fallback teksten uit `mxCell`-elementen wanneer geen Action-mappings
  beschikbaar zijn.
- Slaat ontbrekende flowcharts over en onderdrukt lees-/parsefouten.
- Decomprimeert geen gecomprimeerde Draw.io-diagramtekst; daarvoor moeten de
  relevante XML-elementen direct beschikbaar zijn.

De parser valideert **geen BPMN-vormtypes, lane-indeling, gateway-routering,
ID-uniciteit of ID-reeksen**. Een suffixwaarschuwing bewijst niet dat een volledig
Action ID geldig is. Omdat de applicatie als `WinExe` is gebouwd, is de
consolewaarschuwing ook niet gegarandeerd zichtbaar in de GUI.

## JiraIssueTracker: WinUI-prototype

[JiraIssueTracker.slnx](JiraIssueTracker/JiraIssueTracker.slnx) is een aparte
Windows-oplossing. Open deze in een Visual Studio-versie met ondersteuning voor
.NET 10, WinUI en MSIX, selecteer bijvoorbeeld `x64` en herstel de NuGet-pakketten.

Het [projectbestand](JiraIssueTracker/JiraIssueTracker.csproj) definieert:

- Target framework `net10.0-windows10.0.19041.0` en minimale Windows-platformversie
  `10.0.17763.0`.
- Windows App SDK 1.8, Windows SDK Build Tools en Syncfusion WinUI Editors.
- Platformen `x86`, `x64` en `ARM64`, met MSIX-packaging.

Het huidige ViewModel laadt `IssueFactory.CreateSample()`. Checks, resultaten,
opmerkingen en voortgang worden lokaal in het geheugen verwerkt. **Save Result**
werkt een tijdstempel en UI-melding bij; **Back to Queue** verandert een melding.
Er is geen live Jira-koppeling of persistente resultaatopslag geïmplementeerd in
het huidige ViewModel. Deze beschrijving is geen bevestiging van een geslaagde
build, deployment of productiegeschiktheid.

## EfficiencyTool: efficiëntiefactor

Console-app (.NET 10) die beoordeelt of werk "Programmeur" of "Lopende band werk"
is, ten opzichte van de gemiddelde programmeur.

- **Efficiëntiefactor** = (schatting × 5/3) / werkelijke effort, afgerond op
  5 decimalen.
- **Drempel:** `Programmeren(double)` geeft "Programmeur" bij factor ≥ 0,33333
  (maximaal 5× de eigen schatting), anders "Lopende band werk".
- **Werkrooster:** `Werkrooster.BerekenRoosterUren` telt de uren tussen start en
  afmelding op maandag, dinsdag, donderdag en vrijdag, 09:00-12:30 en
  13:00-15:30 (pauze 12:30-13:00; 6,0 uur netto per dag, 24 per week).
  Woensdag, weekend en feestdagen tellen niet mee. Tijden zijn lokale kloktijd.
  Zonder subcommando toont de tool een gebruiksaanwijzing met exitcode 1.

### Start/afmelden-flow (alle hoeveelheden in **uren**, niet minuten)

1. `start`: Jira-key, eerste schatting (uren) en starttijd (leeg = nu) worden
   onthouden. Een tweede openstaand item met dezelfde key (ook met andere
   hoofdletters) wordt geweigerd.
2. `afmelden [key]`: afmeldtijd na alle iteraties (leeg = nu). De tool berekent
   de **roosteruren** tussen start en afmelding (`Werkrooster.BerekenRoosterUren`);
   deze worden direct als effort gebruikt; bij 0 roosteruren wordt afmelden
   geweigerd en blijft het item open. Daarna vraagt de tool de **extra uren**.
3. `lijst`: toont openstaande items.

- **Factor 1** = (schatting × 5/3) / effort.
- **Factor 2 (incl. extra uren)** = (schatting × 5/3) / (effort + extra uren).
  Extra uren zijn de uren boven 6 per werkdag **bovenop** de effort; tel ze niet
  ook al in de effort mee (dubbel tellen).
- **Opslag:** `%LOCALAPPDATA%\EfficiencyTool\items.json`, te overschrijven met
  `--bestand <pad>` (absoluut of relatief, op elke positie). Het pad wordt
  genormaliseerd. Opslaan schrijft eerst naar `<pad>.tmp` in dezelfde map en
  vervangt daarna het bestand; bij een fout blijft het bestaande bestand intact.
  Dit is geen universele crash-atomiciteitsgarantie voor elk bestandssysteem.
  Schrijven én lezen (ook `lijst`) gebeuren onder een permanent lockbestand
  (`<pad>.lock`, blijft bestaan). Een beschadigd JSON-bestand wordt nooit
  overschreven. Afgemelde items blijven bewaard met roosteruren (door de tool
  berekend), effort, extra uren, beide factoren en `BerekeningsVersie`.
- **Lokaal bestandssysteem (geen netwerkshares):** De exclusieve lock op `<pad>.lock` via `FileShare.None` en het vervangen via `File.Move(..., overwrite: true)` zijn afhankelijk van ondersteuning door het onderliggende bestandssysteem en niet betrouwbaar op netwerkshares (SMB/NFS) of gesynchroniseerde cloudmappen. Gebruik voor `--bestand` altijd een pad op een lokaal bestandssysteem.
- **Strikte CLI:** onbekende opties, onbekende woorden of overtollige argumenten
  geven een foutmelding en exitcode 1; ook opslag- en invoerfouten geven exitcode 1.
- **Tijden** zijn lokale Nederlandse kloktijd zonder tijdzone.
- **Overlap:** items worden afzonderlijk berekend. Bij gelijktijdig openstaande
  items kunnen roosteruren overlappen; tel resultaten dus niet op tot totaal
  gewerkte uren.

```bash
dotnet run --project EfficiencyTool -- start         # item starten
dotnet run --project EfficiencyTool -- afmelden ABC-123
dotnet run --project EfficiencyTool -- lijst

# Bouwen en testen (rechtstreeks via de xUnit v3-executable; 'dotnet test' faalt lokaal onder preview-SDK 11)
dotnet build EfficiencyTool.slnx
./EfficiencyTool.Tests/bin/Debug/net10.0/EfficiencyTool.Tests.exe
```

### Handmatige test tegen Jira TimeTracker

1. Kies een afgeronde issue met geregistreerde tijd; noteer de eerste schatting
   (uren), het startmoment, het afmeldmoment en de gelogde uren.
2. Start en meld af met een tijdelijk opslagbestand:
   `dotnet run --project EfficiencyTool -- --bestand ./jira-check.json start`
   en daarna `... afmelden`.
3. Vergelijk de berekende roosteruren met de worklogs in Jira TimeTracker, bij
   voorkeur over een hele werkweek (24 roosteruren).
4. Leg het verschil vast als niet-ticketgebonden tijd (stand-ups, reviews,
   ondersteuning, contextwissels, extra pauzes; de lunchpauze 12:30-13:00 zit
   al buiten het rooster) en noteer afwijkingen (woensdag gewerkt, feestdag).

### CI / Publish

De GitHub Actions-workflow `.github/workflows/efficiencytool.yml` (`EfficiencyTool CI`) bouwt de solution in `Release`-configuratie (met `TreatWarningsAsErrors`), draait alle unit tests en publiceert op `main`, handmatige runs (`workflow_dispatch`) en `v*`-tags een self-contained `win-x64` single-file executable als workflow-artifact `EfficiencyTool-win-x64` (en als GitHub Release bij een `v*`-tag).

## Draw.io naar Mermaid

De converter vereist **Python 3 en uitsluitend de standaardbibliotheek**:
`sys`, `os`, `xml.etree.ElementTree`, `base64`, `zlib`, `urllib.parse`, `re` en `html`.
Er zijn geen externe Python-pakketten of `pip install`-stappen nodig.

Voer vanuit de repositoryroot uit:

```powershell
python drawio_to_mermaid.py "1 Issue Tracker intake.drawio"
```

Op systemen waar Python 3 `python3` heet, gebruik je die opdrachtnaam.
De Mermaid-tekst verschijnt op standaarduitvoer; het script schrijft zelf geen
`.mermaid`-bestand. Het ondersteunt zowel direct aanwezige XML als gecomprimeerde
Draw.io-diagramtekst, maar is geen BPMN-validator en reconstrueert niet de volledige
Draw.io-opmaak.

## Megalijst en operator-tooling

Zie [Megalijst/README.md](Megalijst/README.md) voor de bouwinstructies, modi,
policies en beperkingen van `WorkbookTextGuard`. Herberekenen via Excel COM
vereist Windows en geïnstalleerd Excel; unittests zijn geen vervanging voor een
COM-praktijktest.

De [fase-3-operatorhandleiding](Megalijst/phase3-tooling/README.md) beschrijft een
**afzonderlijke, snapshotgebonden tool**. Fysieke kandidaatgeneratie vereist een
aparte operator-GO en de aangewezen externe uitvoeringsomgeving. Een build,
RAM-only dry-run of schemaresultaat geeft geen automatische vrijgave. Volg de
actuele handleiding en goedkeuringsgrenzen; deze hoofd-README verleent geen
uitvoerings- of releasegoedkeuring.

## Draw.io-modelleerconventies (handmatig)

Onderstaande regels zijn afspraken voor ontwerpers van de flowcharts,
**geen volledige automatische validatie door `IssueTrackerTool`** en geen
certificering van BPMN 2.0-compliance.

### BPMN-vormen

- **Events** (`shape=mxgraph.bpmn.event`): start-events met
  `outline=standard;symbol=general`, timer catch-events met
  `outline=catch;symbol=timer` en end-events met `outline=end;symbol=general`.
- **Tasks** (`shape=mxgraph.bpmn.task`): werkstappen met een unieke `Action` ID
  en een `taskMarker=user`-markering.
- **Gateways** (`shape=mxgraph.bpmn.gateway`): XOR-keuzes met
  `gatewaySymbol=exclusive` en parallelle AND-paden met `gatewaySymbol=parallel`.

### Lanes

Voor overleg- en reviewfasen **2, 4, 6, 9, 10 en 12** geldt een indeling met
**Medewerker (links)** en **Opdrachtgever (rechts)**, via een Draw.io-tabelcontainer
(`shape=table;childLayout=tableLayout;container=1`).
De interne fasen **1, 3, 5, 7, 8 en 11** worden zonder deze lanes gemodelleerd.

### Action ID-reeksen

| Fase | Naam                  | Reeks |
|------|-----------------------|-------|
| 1    | Intake                | 100   |
| 2    | Review aanpak         | 200   |
| 3    | Analyse               | 300   |
| 4    | Review analyse        | 400   |
| 5    | Ontwerp               | 500   |
| 6    | Review ontwerp        | 600   |
| 7    | Implementatie         | 700   |
| 8    | Test                  | 800   |
| 9    | Meldplicht            | 900   |
| 10   | Review final          | 1000  |
| 11   | Special action        | 1100  |
| 12   | Review special action | 1200  |

Gebruik unieke codes binnen de bijbehorende reeks. Een letterachtervoegsel mag
uitsluitend `T` (True/Ja) of `F` (False/Nee) zijn. Uitgaande XOR-edges krijgen de
bijbehorende code, bijvoorbeeld `604T` en `604F`. Bij een pad met meerdere stappen
krijgt alleen het eerste item direct na de keuze het achtervoegsel; vervolgstappen
krijgen reguliere numerieke codes. Controleer de routing en mapping handmatig;
de beperkte suffixwaarschuwing van de parser controleert deze structuur niet.

## Jira-workflowstatussen en procesfasen

Niet elke procesfase heeft een eigen technische Jira-status.
De bestaande ontwerpafspraak is om zijprocessen en tussentijdse reviews binnen
de bestaande Jira-status af te handelen, zonder extra statussen toe te voegen:

- **Meldplicht (fase 9):** geen aparte Jira-status. De meldplicht wordt uitgevoerd
  binnen de actieve status, bijvoorbeeld Ontwerp, Implementatie of Test.
- **Review special action (fase 12):** administratieve review binnen
  **SpecialAction**, dat in de beschreven workflow een terminale status zonder
  verdere uitgaande transities is.

Dit zijn proces-/workflowafspraken, geen door de lokale tools afgedwongen
Jira-transities.
