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

Om ervoor te zorgen dat de `IssueTrackerTool` de processtappen correct kan interpreteren, mappen en valideren, gelden de volgende richtlijnen:

### 1. BPMN Lane Structuur (Review-bestanden)
De review flowcharts (zoals `2`, `4`, `6`, `10` en `12`) maken gebruik van een BPMN-swimlane-structuur om rollen te verdelen:
* **Medewerker (links)** en **Opdrachtgever (rechts)**.
* Gerealiseerd via een Draw.io tabel-component (`shape=table;childLayout=tableLayout`).

### 2. Action ID Mappings per Review-fase
Om overlappingen te voorkomen, heeft elke review-fase een eigen unieke numerieke reeks:
* **Fase 2 (Review aanpak)**: 200-reeks (bijv. `201`, `202`, `203`, `204`)
* **Fase 4 (Review analyse)**: 400-reeks (bijv. `401`, `402`, `403`, `404`)
* **Fase 6 (Review ontwerp)**: 600-reeks (bijv. `601`, `602`, `603`, `604`)
* **Fase 10 (Review final)**: 1000-reeks (bijv. `1001`, `1002`, `1003`, `1004`)
* **Fase 12 (Review special action)**: 1200-reeks (bijv. `1201`, `1202`, `1203`, `1204`)

### 3. XOR-gateway Vertakkingen en Letter-achtervoegsels (T/F)
* **Achtervoegsel Beperking**: Als een Action ID eindigt met een letter, mag dit **alleen** een `T` (True/Ja) of `F` (False/Nee) zijn. Elke andere letter (zoals `G`, `H`, etc.) is in overtreding en triggert een validatiewaarschuwing.
* **Gateway-edges**: De uitgaande edges vanaf beslissingen/XOR-gateways dragen de Action ID's met het juiste achtervoegsel om de routering consistent te mappen (bijv. `604T` voor goedgekeurd / True en `604F` voor afgekeurd / False).
* **Eerste Stap na Keuze**: Bij een vertakking met meerdere stappen krijgt **alleen het eerste item** (direct na de keuze/gateway) de `T` of `F` achter het ID. De daaropvolgende stappen in hetzelfde pad krijgen een reguliere numerieke ID zonder achtervoegsel.
