# Issue Tracker

Dit project beheert de proces flowcharts (`.drawio`) en bevat een C#-tool (`IssueTrackerTool`) om automatisch Word-documenten (`Actielijst.docx` en `Checklijst.docx`) te genereren en bij te werken op basis van deze flowcharts.

---

## Applicatie-modi

De `IssueTrackerTool` is een native .NET Framework 4.7.2 applicatie en ondersteunt twee uitvoeringsmodi:

### 1. Interactieve GUI-modus (Windows)
Wanneer de applicatie zonder argumenten wordt gestart, opent een interactieve Windows Forms GUI. Deze modus stelt medewerkers in staat om:
* **Fase-selectie**: Interactief door de stappen van specifieke fasen te navigeren (bijv. *Analyse*, *Ontwerp*, *Implementatie*, *Test*, of *Special action*).
* **Ingebouwde Timer**: Een aftel-timer (standaard 60 seconden) om de tijd per processtap te monitoren.
* **Audit Trail**: Bij het voltooien van stappen worden acties, tijdstippen en eventuele opmerkingen gelogd in een lokaal logbestand (`WitTronics_AuditLog.txt`).
* **Document-generatie**: Via de knop **Genereer Rapport** kan direct vanuit de GUI een gecombineerd procesrapport in Word-formaat worden gegenereerd.

### 2. Headless CLI-modus (Windows & Linux)
De tool kan headlessly (zonder GUI) worden uitgevoerd met de `--test-generate` vlag om automatisch procesrapporten te genereren op basis van een Jira-issue.

#### Windows-omgeving
Navigeer naar de hoofdmap van het project en voer het volgende commando uit in Command Prompt (cmd) of PowerShell:
```cmd
IssueTrackerTool\bin\Debug\IssueTrackerTool.exe --test-generate "<JiraIssue>" "<PadNaarOutputBestand.docx>"
```
*Voorbeeld:*
```cmd
IssueTrackerTool\bin\Debug\IssueTrackerTool.exe --test-generate "TEST-123" "C:\pad\naar\TestOutput.docx"
```

#### Linux-omgeving
Op Linux kan de applicatie headlessly worden uitgevoerd met behulp van **Mono**:
```bash
mono IssueTrackerTool/bin/Debug/IssueTrackerTool.exe --test-generate "<JiraIssue>" "<PadNaarOutputBestand.docx>"
```
*Voorbeeld:*
```bash
mono IssueTrackerTool/bin/Debug/IssueTrackerTool.exe --test-generate "TEST-123" "/home/rinse/GeminiProjects/IssueTracker/TestOutput.docx"
```

---

## Document-generatie werking

Bij het genereren van een procesrapport voert de tool de volgende stappen uit:
1. De bron templates (`Actielijst.docx` en `Checklijst.docx`) inladen.
2. De XML-definities uit de `.drawio` bestanden van alle actieve procesfasen parsen.
3. Actiecodes (bijv. `602`, `603`) en labels mappen tussen de flowcharts, actielijst-taken en controlelijst-checks.
4. Een compleet en gestructureerd OpenXML Word-rapport (`.docx`) genereren op de doellocatie met alle processtappen en bijbehorende check-bulletpoints in de juiste volgorde.

---

## Draw.io Flowchart & BPMN Conventies

Het gehele project is gemoderniseerd naar **BPMN 2.0 modelleringsstandaarden**. Om ervoor te zorgen dat de `IssueTrackerTool` de processtappen correct kan interpreteren, mappen en valideren, gelden de volgende richtlijnen:

### 1. BPMN Vormen en Elementen (Project-breed)
Alle `.drawio` flowcharts maken exclusief gebruik van gestandaardiseerde BPMN-componenten in plaats van klassieke flowchart-vormen:
* **Events (`shape=mxgraph.bpmn.event`)**:
  * *Start Events*: Groene dunne cirkels (`outline=standard;symbol=general`) die het begin van de fase aanduiden.
  * *Intermediate/Timer Catch Events*: Dubbele cirkels (`outline=catch;symbol=timer`) voor tijds- of ETC-triggers.
  * *End Events*: Dikke cirkels (`outline=end;symbol=general`) die de beëindiging of doorstroming naar een volgende fase representeren.
* **Tasks (`shape=mxgraph.bpmn.task`)**:
  * Gekoppeld aan actieve werkstappen met een unieke `Action` ID (zoals `301`, `504`). Voorzien van de `taskMarker=user` markering.
* **Gateways (`shape=mxgraph.bpmn.gateway`)**:
  * *Exclusive (XOR)*: Rhombus-vormen met een kruis (`gatewaySymbol=exclusive`) om keuzes en vertakkingen te modelleren.
  * *Parallel (AND)*: Rhombus-vormen met een plusteken (`gatewaySymbol=parallel`) om parallelle workflows (zoals gelijktijdige urenregistratie/ETC-timers en medewerkerstaken) te splitsen of samen te voegen.

### 2. BPMN Lane Structuur (Review- en Overlegfases)
Fases die een samenwerkings- of goedkeuringsmoment met de opdrachtgever/baas bevatten (**Fase 2, 4, 6, 9, 10 en 12**) zijn uitgebreid met een fysieke zwembaan-indeling:
* **Medewerker (links)** en **Opdrachtgever (rechts)**.
* Gerealiseerd via een Draw.io tabel-component (`shape=table;childLayout=tableLayout;container=1`).

De puur interne inhoudelijke fases (**Fase 1, 3, 5, 7, 8 en 11**) hebben géén zwembanen (omdat deze uitsluitend door de medewerker worden uitgevoerd), maar maken wel volledig gebruik van de hierboven genoemde BPMN-vormen.

### 3. Action ID Mappings per Review-fase
Om overlappingen te voorkomen, heeft elke proces- en reviewfase een eigen unieke numerieke reeks:
* **Fase 1 (Intake)**: 100-reeks
* **Fase 2 (Review aanpak)**: 200-reeks
* **Fase 3 (Analyse)**: 300-reeks
* **Fase 4 (Review analyse)**: 400-reeks
* **Fase 5 (Ontwerp)**: 500-reeks
* **Fase 6 (Review ontwerp)**: 600-reeks
* **Fase 7 (Implementatie)**: 700-reeks
* **Fase 8 (Test)**: 800-reeks
* **Fase 9 (Meldplicht)**: 900-reeks
* **Fase 10 (Review final)**: 1000-reeks
* **Fase 11 (Special action)**: 1100-reeks
* **Fase 12 (Review special action)**: 1200-reeks

### 4. XOR-gateway Vertakkingen en Letter-achtervoegsels (T/F)
* **Achtervoegsel Beperking**: Als een Action ID eindigt met een letter, mag dit **alleen** een `T` (True/Ja) of `F` (False/Nee) zijn. Elke andere letter (zoals `G`, `H`, etc.) is in overtreding en triggert een validatiewaarschuwing.
* **Gateway-edges**: De uitgaande edges vanaf beslissingen/XOR-gateways dragen de Action ID's met het juiste achtervoegsel om de routering consistent te mappen (bijv. `604T` voor goedgekeurd / True en `604F` voor afgekeurd / False).
* **Eerste Stap na Keuze**: Bij een vertakking met meerdere stappen krijgt **alleen het eerste item** (direct na de keuze/gateway) de `T` of `F` achter het ID. De daaropvolgende stappen in hetzelfde pad krijgen een reguliere numerieke ID zonder achtervoegsel.

---

## Jira Workflow Status Mapping & Discrepanties

Om de Jira-workflow zo overzichtelijk en onderhoudbaar mogelijk te houden, is er een bewuste ontwerpkeuze gemaakt over hoe om te gaan met discrepanties tussen de inhoudelijke processtappen (de flowcharts) en de technische Jira-statussen:

**Discrepanties worden afgehandeld binnen de bestaande Jira-statussen.** Er worden geen extra Jira-statussen aangemaakt voor zij-processen of tussentijdse reviews.

### Belangrijkste uitwerkingen:
1. **Meldplicht (`9 Issue Tracker meldplicht.drawio`)**:
   * Hoewel Meldplicht in de codebase een aparte procesfase is, bestaat er **geen aparte `Meldplicht` status** in Jira.
   * Wanneer de meldplicht wordt getriggerd vanuit een actieve fase (zoals *Ontwerp*, *Implementatie*, of *Test*), wordt dit zij-proces volledig binnen de bestaande Jira-status van die actieve fase uitgevoerd en afgerond.
2. **Review Special Action (`12 Issue Tracker review special action.drawio`)**:
   * In de Jira-workflow is `SpecialAction` een terminale status zonder verdere uitgaande transities.
   * De review van een speciale actie door de baas (Fase 12) wordt administratief volledig afgehandeld binnen de bestaande Jira-status **SpecialAction**.
