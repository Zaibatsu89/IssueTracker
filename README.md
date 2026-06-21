# Issue Tracker

Dit project beheert de proces flowcharts (`.drawio`) en bevat een C#-tool (`IssueTrackerTool`) om automatisch Word-documenten (`Actielijst.docx` en `Checklijst.docx`) te genereren en bij te werken op basis van deze flowcharts.

## Headless Uitvoeren

De `IssueTrackerTool` kan headlessly (zonder GUI) worden uitgevoerd met de `--test-generate` vlag om automatisch procesrapporten te genereren.

### Windows-omgeving

Omdat de `IssueTrackerTool` een native .NET Framework 4.7.2 Windows-applicatie is, kan deze direct op Windows worden uitgevoerd zonder extra software:

#### Commando
Navigeer naar de hoofdmap van het project en voer het volgende commando uit in Command Prompt (cmd) of PowerShell:

```cmd
IssueTrackerTool\bin\Debug\IssueTrackerTool.exe --test-generate "<JiraIssue>" "<PadNaarOutputBestand.docx>"
```

#### Voorbeeld
```cmd
IssueTrackerTool\bin\Debug\IssueTrackerTool.exe --test-generate "TEST-123" "C:\pad\naar\TestOutput.docx"
```

---

### Linux-omgeving

Op Linux kan de applicatie headlessly worden uitgevoerd met behulp van **Mono**:

#### Vereisten
Zorg ervoor dat `mono` is geïnstalleerd op de Linux-omgeving.

#### Commando
Navigeer naar de hoofdmap van het project en voer het volgende commando uit:

```bash
mono IssueTrackerTool/bin/Debug/IssueTrackerTool.exe --test-generate "<JiraIssue>" "<PadNaarOutputBestand.docx>"
```

#### Voorbeeld
```bash
mono IssueTrackerTool/bin/Debug/IssueTrackerTool.exe --test-generate "TEST-123" "/home/rinse/GeminiProjects/IssueTracker/TestOutput.docx"
```

---

### Wat dit commando doet:
1. De bron templates (`Actielijst_Template.docx` en `Checklijst_Template.docx`) kopiëren en patchen naar de actuele templates (`Actielijst.docx` en `Checklijst.docx`) op basis van de definities in de `.drawio` XML-bestanden.
2. Een gecombineerd procesrapport genereren op de doellocatie met alle stappen in de juiste volgorde.
