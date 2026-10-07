# Intakepakket — checklist Esther v34

## Status

**Pilotstatus: Geblokkeerd. Het 17-code-sjabloon is niet vrijgegeven voor Jira.** De tekstuele dekkingsanalyse heeft ontbrekende en deels uitgewerkte bronvereisten aangetoond; uitbreiding/concretisering en visuele review zijn vereist.

Werkafspraken aangeleverd door Rinse als bevestigd met Jeroen. Geen Jira-tickets gewijzigd, geen mail verstuurd en geen pilot gestart. Dit document bevat een leeg sjabloon, geen tweede registratie van ingevulde ticketchecklists.

## Definitieve bevestigingsmail

> Jeroen,
>
> De huidige procespapieren bieden houvast door de fasering en heldere technische acceptatiecriteria. Twee ervaringen laten zien waar aanvullende vastlegging nodig is: bij IT-142 leidden niet vastgelegde afhankelijkheden tot 14 uur rework en volledige ontwerpherziening. Bij IT-158 werd na vijf werkdagen zonder reviewreactie implementatie gestart; na afwijzing van de analyse bleek die code onbruikbaar.
>
> Conform onze bevestigde afspraken gebruiken we vanaf maandag 19 oktober 2026 de checklist van Esther als aanvulling op de bestaande procesdocumentatie:
> 1. Voor Story, Task en Bug waarvan de intake tijdens de vierweekse pilot begint. Sub-task, Hotfix, administratieve en reeds lopende taken zijn uitgesloten.
> 2. Gezamenlijke uitvoering beperkt zich tot analyse, ontwerp en test. Alle bestaande contact- en akkoordverplichtingen blijven gelden.
> 3. De checklist staat onderaan Description onder `### Checklist Esther v34`. Ik ben de enige bewerker, tenzij ik per ticket tijdelijk één waarnemer expliciet aanwijz. Jij registreert akkoorden en afwijzingen in commentaren. Zonder verplicht akkoord geen volgende inhoudelijke fase.
> 4. Bij wachten vlaggen we het ticket met Flagged; bij parkeren blijft de vlag actief. De status blijft behouden als geen passende transitie bestaat. Een vlag verdwijnt zodra de belemmering is opgelost, mits geen andere belemmering resteert.
> 5. Na 16 werkuren zonder besluit escaleren we naar Esther. Na 8 extra werkuren zonder besluit parkeren we het afhankelijke werk; herbeoordeling is maandag 10:00. Esther is escalatieontvanger, niet automatisch vervangend accordeerder.
> 6. We rekenen met maandag t/m donderdag 09:00–17:00, exclusief feestdagen. Maximaal 10% van alle akkoordaanvragen mag de responstijd overschrijden, met directe escalatienotitie en vlag voor iedere overschrijding.
> 7. DoD: merge naar doelbranch met geslaagde pipeline, volledig ingevulde checklist en jouw expliciete acceptatiecommentaar.
> 8. Procesflow en wachttijden beoordelen we op 16 november 2026; cohort-rework definitief op 14 december 2026. Zonder vergelijkbare historische gegevens dient de pilot als baseline. Bij pilot-doorrol boven 20% rapporteren we cohort-rework als indicatief.
>
> De instroom loopt van 19 oktober 2026 09:00:00 inclusief tot 12 november 2026 17:00:00 exclusief, in Europe/Amsterdam. Procesevaluatie is op 16 november; cohort-rework op 14 december. De instroom sluit eerder dan het procesevaluatiemoment; succesvolle opleveringen tot die evaluatiegrens kunnen nog in de rework-cohort vallen.
>
> De feestdagenkalender is de door Rinse bevestigde Nederlandse officiële kalender: geen uitzonderingsdagen binnen 19 oktober–14 december. Roosterberekeningen gebruiken lokale tijden in Europe/Amsterdam, inclusief de overgang naar wintertijd; geen vaste UTC-offset.
>
> Het brondekkingsonderzoek toont dat de 17 korte labels nog niet alle vereisten expliciet afdekken. We starten niet voordat de volledige bronmapping, eventuele extra codes en visuele controle zijn goedgekeurd, en Jira-transities en bordzichtbaarheid daadwerkelijk zijn gecontroleerd.
>
> Groeten,
> Rinse

## Leeg sjabloon voor Description

Behoud de bestaande issuebeschrijving en voeg deze sectie onderaan toe. `Open` is geen afgevinkt punt. Vul reden/bewijs en bij besluiten de link naar het besliscommentaar in. Gebruik de complete onderliggende checklistinhoud bij uitvoering; de korte labels hieronder zijn referenties, geen vervanging daarvan.

### Checklist Esther v34

| Puntcode | Onderwerp | Status | Reden/bewijs/besliscommentaar |
|---|---|---|---|
| INT-01 | Scopebepaling | Open | |
| INT-02 | Afhankelijkheden | Open | |
| INT-03 | Effortschatting | Open | |
| ANA-01 | Probleemanalyse | Open | |
| ANA-02 | Oplossingsrichting | Open | |
| ANA-03 | Akkoord analyse | Open | |
| DES-01 | Technisch ontwerp | Open | |
| DES-02 | Raakvlakken | Open | |
| DES-03 | Akkoord ontwerp | Open | |
| IMP-01 | Realisatie conform ontwerp | Open | |
| IMP-02 | Unittests gereed | Open | |
| IMP-03 | Code review verzoek | Open | |
| TST-01 | Integratietest | Open | |
| TST-02 | Acceptatietest Rinse & Jeroen | Open | |
| TST-03 | Akkoord test | Open | |
| AFR-01 | DoD-controle | Open | |
| AFR-02 | Tijdsevaluatie & overschrijdingsanalyse | Open | |

Bij AFR-01 kan het checklistonderdeel van de DoD worden gecontroleerd als ‘alle overige punten afgerond of gemotiveerd N.v.t.’. Markeer AFR-01 pas Afgerond nadat merge, pipeline en acceptatiecommentaar zijn gecontroleerd; daarmee is ook de gehele checklist gevuld. Voortijdig gestopte taken worden niet kunstmatig volledig gemaakt met N.v.t.

## Commentaarsjablonen

- **Aanvraag (uitvoerder):** puntcode; gevraagde beslissing; bewijs/referentie; beoordelaar; aanvraagmoment; deadline na 16 werkuren; `Wacht op akkoord`; vlag aangebracht.
- **Besluit (Jeroen/bevoegde vervanger):** puntcode; expliciet `Akkoord` of `Afgewezen`; motivering/voorwaarden; tijdstempel. Een aanvraag of commentaar van de uitvoerder is geen akkoord.
- **Escalatie:** oorspronkelijke aanvraag; berekende overschrijding; melding aan Esther; tijdstempel; vlag actief; deadline na 8 extra werkuren.
- **Parkeren:** reden; eigenaar; eerstvolgende maandag 10:00 voor herbeoordeling; vlag actief.
- **Afwijzing zonder terugtransitie:** `Afgekeurd - herziening vereist`; besluitlink; Jira-status behouden; herzieningsactie en nieuwe aanvraag na uitvoering.
- **Overdracht:** ticket; waarnemer; ingangsdatum; eindmoment/terugnamevoorwaarde; exclusief bewerkingsmandaat en overdracht van registratie, vlagbeheer en escalatie. Geen akkoordmandaat.

## Metingen en startcontrole

Alle ingevulde gegevens blijven in Jira. Noteer intake, oplevering/uitval, aanvraag- en besluitmomenten en reden van uitval. Bereken reviewwachten en totale doorlooptijd beide in werkuren volgens één rooster. Niet beantwoorde aanvragen voorbij 16 werkuren tellen als overschrijding; aanvragen waarvan de deadline bij de meetgrens nog niet is verstreken worden apart als open/nog niet beoordeelbaar vermeld. Rapporteer nulnoemers als Niet meetbaar.

**DoD-nalevingscontrole (audit):** 100% checklistvolledigheid onder DoD-opgeleverde taken is per definitie vereist, geen onafhankelijk effectbewijs. Audit daarnaast alle pilot-tickets die in Jira als `Done`/`Resolved` of anderszins opgeleverd zijn gemarkeerd: rapporteer ontbrekende bronvereisten, onvolledige checklist, ontbrekend acceptatiecommentaar of andere DoD-schending als formele procesafwijking in week 4. Rapporteer ticket, tekortkoming, markeermoment en herstelactie. Deze tickets tellen niet als succesvol opgeleverd; sluit ze niet uit van het auditrapport doordat ze de DoD missen.

Nog vereist vóór start:
- [x] Instroom bindend vastgelegd: [19 oktober 2026 09:00:00, 12 november 2026 17:00:00), Europe/Amsterdam. Werkrooster ma–do 09:00–17:00; officiële Nederlandse feestdagen uitgesloten, volgens gebruikersbevestiging geen binnen het meetinterval.
- [ ] Exacte tijdstippen van procesevaluatie/opleveringsgrens op 16 november en rework-evaluatie op 14 december nog bevestigen; geen nieuwe instroom na 12 november.
- [ ] Dekkingsgaten oplossen en mapping goedkeuren: [brondekking.artifact.md](file:///C:/Users/Rinse/AppData/Local/Google/AndroidStudio2026.2.1/projects/issuetracker.5c12e1c0/.artifacts/dcdd2b17-76a8-4c92-a36e-1284a462096f/brondekking.artifact.md) bevat 149 geanalyseerde eisen: 9 expliciet volledig, 121 deels, 19 ontbrekend. Geen visuele verificatie uitgevoerd. Geen extra codes zonder review ingevoerd.
- [ ] Actieve Jira-transities, commentaar-/bewerkrechten en Flagged-bordzichtbaarheid gecontroleerd.
- [ ] Historische vergelijkingsdata beoordeeld; bij ontbreken baseline-aanpak gebruikt.

Procesmeetmoment: 16 november 2026. Reworkmeetmoment: 14 december 2026. Alleen taken met intake én succesvolle oplevering binnen de pilot vormen de definitieve rework-cohort; ieder krijgt vier weken observatie na oplevering. Later opgeleverde taken en uitval apart rapporteren.
