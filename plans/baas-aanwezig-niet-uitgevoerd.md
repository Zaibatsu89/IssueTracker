# Plan: "Baas Aanwezig" Negatief Resultaat Routering (met Fase-onderscheid)

## Background & Motivation
In de verschillende review flowcharts (Aanpak, Analyse, Ontwerp, Review Final, Special Action) bevat de stap "Is de baas aanwezig? (indien niet aanwezig: meldplicht)" momenteel geen vertakking (nee-pad). De wens is om bij een negatief resultaat (baas is niet aanwezig) direct te routeren naar een specifieke "niet uitgevoerd" status en vervolgens door te stromen naar de actie "De baas bepaalt volgende status".

Om verwarring te voorkomen, maken we een duidelijk onderscheid tussen de verschillende fasen: **opdracht**, **intake**, **analyse**, **ontwerp** en de **speciale actie**.

*Let op: Zoals overeengekomen zal dit plan pas worden uitgevoerd nadat er afstemming is geweest met Jeroen (de baas) over deze nieuwe, nog onbesproken feature "niet uitgevoerd".*

## Fase-onderscheid & Status Mapping
Wanneer de baas niet aanwezig is bij de overleg/review-stap, zal de status overgaan in een specifieke `Issue [<fase> niet uitgevoerd]` status. De mapping is als volgt gedefinieerd:

| Flowchart / Reviewbestand | Reviewt Resultaat Van | Doelstatus bij "Nee" (Niet Aanwezig) | Vervolgstap (Terminator) |
| :--- | :--- | :--- | :--- |
| **2 Issue Tracker review aanpak.drawio** | Intake (`Issue [intake voltooid]`) | `Issue [intake niet uitgevoerd]` | `De baas bepaalt volgende status` (Action 206F) |
| **4 Issue Tracker review analyse.drawio** | Analyse (`Issue [geanalyseerd]`) | `Issue [analyse niet uitgevoerd]` | `De baas bepaalt volgende status` (Action 406F) |
| **6 Issue Tracker review ontwerp.drawio** | Ontwerp (`Issue [ontworpen]`) | `Issue [ontwerp niet uitgevoerd]` | `De baas bepaalt volgende status` (Action 606F) |
| **10 Issue Tracker review final.drawio** | Hele opdracht (`Issue [getest]`) | `Issue [opdracht niet uitgevoerd]` | `De baas bepaalt volgende status` (Action 1007F) |
| **12 Issue Tracker review special action.drawio**| Speciale actie (`Issue [speciale actie uitgevoerd]`) | `Issue [speciale actie niet uitgevoerd]` | `De baas bepaalt volgende status` (Action 1206) |

---

## Proposed Solution
Per review flowchart `.drawio` bestand zullen we de volgende XML-transformaties geautomatiseerd uitvoeren:

1.  **Vorm van de beslissingsstap aanpassen**:
    - Het procesblok "Is de baas aanwezig?..." (bijv. Action 202, 402, etc.) wordt omgezet naar een beslissingsruit (`shape=mxgraph.flowchart.decision`).
2.  **Bestaande uitgaande pijl ("ja") markeren**:
    - De bestaande uitgaande pijl naar het overleg- of demonstratieblok krijgt de waarde/label `ja`.
3.  **Nieuw nee-pad toevoegen**:
    - We voegen een nieuwe uitgaande pijl (`mxCell` edge) toe vanaf de beslissingsruit met de waarde/label `nee`.
4.  **Nieuwe specifieke status-node toevoegen**:
    - Er wordt een nieuw rechthoekig statusblok (`shape=rounded=0`) aangemaakt met het specifieke label zoals gedefinieerd in de tabel hierboven (bijv. `Issue [intake niet uitgevoerd]`).
5.  **Verbinden naar terminator**:
    - De nieuwe status-node wordt via een pijl verbonden met een terminator-node (let op: zorg ervoor dat dit pad niet doorstroomt naar de afgekeurd-status in de flowcharts waar De baas bepaalt volgende status een procesblok is).

---

## Implementation Steps
1.  **Goedkeuring**: Wacht op expliciet akkoord van Jeroen (de baas) op de voorgestelde "niet uitgevoerd" statussen, aangezien dit een nieuwe feature betreft.
2.  **XML Transformatie-script**: Schrijf een robuust C# of Python script dat:
    - De XML-structuur van de 5 doelfiles inlaadt.
    - Het element met het specifieke Action ID (bijv. `Action="202"`) opspoort.
    - De stijl verandert naar een beslissingsruit.
    - De bestaande edge labelt met `ja`.
    - Een nieuwe node `Issue [<fase> niet uitgevoerd]` aanmaakt met een uniek ID en berekende x/y coördinaten (bijvoorbeeld links of rechts van het beslissingsblok, om overlapping te voorkomen).
    - Een nieuwe edge met label `nee` trekt van de beslissingsruit naar deze nieuwe node.
    - Een nieuwe edge trekt van deze nieuwe node naar de terminator `De baas bepaalt volgende status`.
3.  **Uitvoering**: Draai het script op de 5 bestanden.
4.  **Visuele check & Layout-tuning**: Open de bestanden in draw.io om te controleren of alle pijlen netjes lopen en geen andere elementen overlappen. Lijn de elementen desgewenst handmatig of via coördinatenaanpassingen in het script visueel perfect uit.

## Verification
- **Visueel**: Handmatige controle van de 5 flowcharts in draw.io.
- **Functioneel**: Draai de `IssueTrackerTool` om er zeker van te zijn dat de gegenereerde checklists en actielijsten correct blijven werken en geen fouten genereren.

## Rollback
Mocht er onverhoopt iets misgaan of mocht de baas de feature herroepen, dan kan de staat eenvoudig worden hersteld met:
```bash
git restore "*.drawio"
```
of door de specifieke commit te reverten.
