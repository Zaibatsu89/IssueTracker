# CLAUDE.md

## 1. Reikwijdte
- Deze repository bevat meerdere losstaande projecten (`IssueTrackerTool` op .NET Framework 4.7.2, `JiraIssueTracker` op WinUI/.NET 10, `EfficiencyTool` op .NET 10, `Megalijst` en `drawio_to_mermaid.py`).
- Onderstaande richtlijnen gelden voor `EfficiencyTool/` en `EfficiencyTool.Tests/` tenzij anders vermeld.
- `.github/copilot-instructions.md` bevat generieke ASP.NET/EF/Clean Architecture-regels en is **niet** bindend voor deze repository.

## 2. Bouwen en testen (`EfficiencyTool`)
- Bouwen: `dotnet build EfficiencyTool.slnx` (`TreatWarningsAsErrors` staat aan in beide projecten).
- Testen: voer de xUnit v3-testexecutable rechtstreeks uit:
  `./EfficiencyTool.Tests/bin/Debug/net10.0/EfficiencyTool.Tests.exe`
  *(Gebruik niet `dotnet test`; dat faalt lokaal onder de geïnstalleerde preview-SDK 11 door VSTest-modus.)*
- CI (`.github/workflows/efficiencytool.yml`) voert dezelfde stappen uit in `Release`-configuratie en publiceert self-contained (`win-x64`, `PublishSingleFile=true`) naar `EfficiencyTool/bin/Release/net10.0/publish/win-x64/`.

## 3. Domeinregels (`EfficiencyTool`)
- **Eenheid en tijd:** Alle hoeveelheden zijn in **uren**. Tijdstippen zijn lokale Nederlandse kloktijd zonder tijdzone (`DateTimeKind.Unspecified`).
- **Werkrooster:** Maandag, dinsdag, donderdag en vrijdag `09:00–12:30` en `13:00–15:30` (pauze `12:30–13:00`; max. 6,0 uur/dag, 24,0 uur/week).
- **Effort bij afmelden:** `werkelijkeEffortUren` is altijd gelijk aan de berekende roosteruren (`Werkrooster.BerekenRoosterUren`). Bij `0` roosteruren wordt afmelden geweigerd en blijft het item open.
- **Factoren:**
  - Factor 1: `(schatting * 5/3) / effort`
  - Factor 2: `(schatting * 5/3) / (effort + extraUren)` (`extraUren` zijn uren > 6u/dag **bovenop** de effort, niet dubbel geteld; `schatting > 0`).
  - Beide factoren worden afgerond op 5 decimalen (`MidpointRounding.AwayFromZero`). Drempel `"Programmeur"` is `>= 0,33333`.

## 4. Opslag en gelijktijdigheid (`ItemRegistratie`)
- Paden worden bij binnenkomst genormaliseerd met `Path.GetFullPath`.
- Exclusieve processynchronisatie verloopt via een permanent lockbestand `<pad>.lock` (`FileShare.None`, retry op `IOException` en `UnauthorizedAccessException`).
- Schrijven gebeurt via `<pad>.tmp` in dezelfde map met `Flush(true)` gevolgd door `File.Move(..., overwrite: true)`. Beschadigde JSON mag nooit worden overschreven met een lege lijst.
- Leesacties (`LaadAlles`, `LaadOpenstaand`) maken geen map of `.lock`-bestand aan als de bovenliggende map nog niet bestaat.
- `SlaOp` en `VerkrijgBestandsLock` zijn `internal` (alleen voor tests zichtbaar via `InternalsVisibleTo("EfficiencyTool.Tests")`).

## 5. Code- en teststijl
- Gebruik file-scoped namespaces, collection expressions (`[]`), `Nullable` enabled, en XML-doc op alle publieke API's.
- Gebruik Nederlandse namen en foutmeldingen, met uitsluitend ASCII-tekens in identifiers (geen trema's/accenten zoals `ë`).
- Gebruik standaard .NET-exceptions (`ArgumentException`, `ArgumentOutOfRangeException`, `InvalidOperationException`, `IOException`) en zet bij CLI-fouten `Environment.ExitCode = 1`.
- Gebruik in tests uitsluitend standaard xUnit-`Assert` (geen FluentAssertions of Moq), `TheoryData<T>` + `[MemberData]` voor array-parameters en `TestContext.Current.CancellationToken` bij async taken (voorkomt `xUnit1051`).

## 6. Werkafspraken
- Commit of push **nooit** zonder expliciet verzoek van de gebruiker.
- Werk `README.md` bij wanneer CLI-gedrag, opties of domeinregels veranderen.
