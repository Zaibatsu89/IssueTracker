# Evaluatie en invoering van de checklist van Esther

## Voortgang en vereiste inhoudelijke revisie

**Pilotstatus: Geblokkeerd.** Tekstuele brondekking is onderzocht: [brondekking.artifact.md](file:///C:/Users/Rinse/AppData/Local/Google/AndroidStudio2026.2.1/projects/issuetracker.5c12e1c0/.artifacts/dcdd2b17-76a8-4c92-a36e-1284a462096f/brondekking.artifact.md).

De matrix omvat 149 afzonderlijk geanalyseerde bronvereisten/registratievelden: 9 expliciet volledig, 121 deels, 19 ontbrekend in het huidige intakepakket. Dit is een tekstueel volledigheidsoordeel, geen bewijs van visuele broncontrole of 149 oorspronkelijke genummerde Word-items. Het 17-code-sjabloon is niet vrijgegeven.

### Revisie vóór verdere uitvoering

De systeemconflicten vereisen formele besluiten; een tekstuele uitbreiding van de 17 labels alleen volstaat niet. Na goedkeuring van deze revisie worden de volgende stappen uitgevoerd:

1. **Eerst conceptnormenkader, daarna bekrachtiging, dan definitieve uitwerking.** De eerstvolgende uitvoeringsstap na planapproval is het aanmaken van `normenkader_checklist_esther_v34.artifact.md` als **Concept — niet vrijgegeven**, met volledige toewijzing van alle 149 matrixrijen, subcriteria en bewijsvoering (code, testrapport, ticket-link, commentaar of worklog). Daarmee wordt nog geen Excel-dispositie bekrachtigd. Excel-afhankelijke criteria krijgen voorlopig `Besluit vereist — voorgestelde vervanging door Jira`; geen onbesloten vervanging als geldende norm presenteren.
2. **Excel-sanering vóór definitieve generatie bekrachtigen.** Leg optie A expliciet ter formele goedkeuring voor aan zowel Jeroen als Esther. De normatieve eindversie kan pas na hun besluit worden vastgesteld. Bij goedkeuring krijgen alle Excel-verwijzingen, waaronder matrixrijen 26, 47, 65, 80, 114 en 115, uniform de dispositie `Vervangen door Jira (subtaak/link/commentaar)`, met concrete bewijs- en acceptatiecriteria.
   - **A — Sanering (voorkeursvoorstel, nog niet besloten):** Excel-stappen krijgen de disposition `Vervangen door Jira`. Actiepunten worden Jira-subtaken of gekoppelde issues; reflectiepunten komen in het afsluitende ticket-commentaar. Subtaken blijven buiten de zelfstandige pilotcohort maar kunnen bewijsmateriaal van de hoofdtaak zijn.
   - **B — Integratie (uitsluitend terugval bij afwijzing van A; nieuwe review vereist):** wijs één specifiek centraal Excel-bestand, eigenaar, toegangsrechten en vaste ticketlink aan. Excel beheert uitsluitend actiepunten; checkliststatus en besluiten blijven in Jira. Definieer welke gegevens gezaghebbend zijn en hoe verwerking naar Jira wordt aangetoond. Dit is een expliciete uitzondering op de eerdere Jira-onlyregel, geen tweede ingevulde checklist.
   - Zonder dit besluit blijven bronvereisten over Excel onopgelost; niet stilzwijgend `N.v.t.` markeren.
3. **Effort-interrupt en Jira Work Log formeel vaststellen.** Scheid tussentijdse effortsturing van post-mortem `AFR-02` en reviewresponstijden; zie sectie 2. Brontriggers rond 2/3, 100% en 150% worden visueel gecontroleerd en behouden of expliciet vervangen bij besluit. Alleen een 100%-regel toevoegen rechtvaardigt niet het schrappen van andere brontriggers.
4. **Visuele broncontrole in Microsoft Word, na beoordeling van het besluitpakket.** Open de bron na uitvoeringsgoedkeuring read-only met de aanwezige `WINWORD.EXE`. Controleer tekstvakken, pijlen, sectie-/itemrelaties en tijdtriggers tegenover de XML-extractie van de 63 Choice/Fallback-paren. Leg bevindingen met bronlocatie en beoordelaar vast. De huidige tooling kan geen Windows-desktopbeelden inspecteren: indien dat ongewijzigd blijft, voert Rinse of een aangewezen reviewer de visuele inspectie uit en levert controlebewijs. XML-extractie of alleen Word openen geldt niet als visuele verificatie.
5. **Conceptnormenkader completeren en definitief vaststellen na besluiten en visuele controle.** Behoud een compact Jira-sjabloon met 17 hoofdpoorten of een gemotiveerde, te accorderen uitbreiding tot circa 20–22 poorten. Geen tabel met 149 rijen in Description.
   - Werk het in stap 1 aangemaakte concept uit: normatieve subcriteria per poort, bronmatrixrijen, toepasselijkheid, vereist bewijs en expliciete acceptatieregel. Behoud de oorspronkelijke matrixrijnummers als herkomstreferentie; dit zijn geen extra Jira-puntcodes.
   - Behoud `brondekking.artifact.md` als onderzoeks-/herkomstbewijs; de transformatie naar een normenkader wist de oorspronkelijke gatenanalyse niet.
   - Iedere van de 149 analyserijen krijgt een traceerbare bestemming: behouden criterium, gemotiveerde samenvoeging met behoud van inhoud, of formeel geaccordeerde vervanging. Geen ongemerkte weglatingen.
   - Een poort mag uitsluitend `Afgerond` worden als alle toepasselijke subcriteria zijn voldaan en overige criteria gemotiveerd `N.v.t.` zijn. Eén gedeeltelijk toepasselijke poort kan niet volledig `N.v.t.` worden verklaard.
   - Bijvoorbeeld `IMP-02`: specificeer functiegewijs testen, steppend debuggen en relevante paddekking. Waarschuwingsvrij bouwen krijgt een expliciet criterium onder een passende poort; de keuze wordt in de mapping verantwoord.
   - Het normenkader is versiegebonden referentiedocumentatie, geen parallelle taakregistratie. Jira bevat normenkaderversie/link en per poort status, bewijs en eventuele uitzonderingen. Bepaal de publicatielocatie en borg toegang vóór vrijgave.
6. **Besluitpakket in twee rondes voorleggen.** Leg eerst het conceptnormenkader met Excel-sanering en Work Log-registratie voor aan Esther en Jeroen. Leg na hun besluiten en de visuele broncontrole de definitieve volledige mapping, normenkaderversie, effortregel en compacte poortenset ter eindaccordering voor; pas daarna intakepakket en sjabloon aan. Registreer besluit, beoordelaars, datum en documentversie; een conceptbestand is geen akkoordbewijs.
7. Verifieer in de actieve Jira-omgeving `Flagged`, de transitie `Review design` → `Review Nok` → `Designing` en eventuele afwijzingstransities voor `Review Analyse`/`Review Final`. Er is nog geen live verificatie uitgevoerd.

### Vrijgavecriteria

De pilot blijft Geblokkeerd totdat formele besluiten, volledige traceerbaarheid, visuele broncontrole en live Jira-controles met bewijs zijn afgerond. Planapproval is geen vervanging van het akkoord van de proceseigenaren op inhoudelijke afwijkingen.

## Doel en uitgangspunt

Rinse wil de checklist van Esther integraal gebruiken voor Jira-hoofdtaken binnen de afgesproken scope. Gezamenlijke uitvoering met Jeroen wordt beperkt tot analyse, ontwerp en test; formele akkoordmomenten blijven daarvan onderscheiden.

Dit plan gaat over werkafspraken en een voorstel aan Jeroen, niet over aanpassingen aan software of procesdiagrammen. Er worden nu geen bronbestanden gewijzigd.

Geraadpleegd:
- [Checklist Esther versie 34 RC oktober 2025.docx](file:///C:/Zaibatsu89/IssueTracker/Checklist%20Esther/Checklist%20Esther%20versie%2034%20RC%20oktober%202025.docx).
- [README.md](file:///C:/Zaibatsu89/IssueTracker/README.md).
- [WT_Workflow_05.md](file:///C:/Zaibatsu89/IssueTracker/WT_Workflow_05.md).

## Beoordeling van de voorgestelde evaluatie

De richting is concreet en positief: één vaste checklist, consequent gebruik en ruimte voor samenwerking. De tekst benoemt echter vooral een voornemen. Een evaluatie hoort ook te zeggen wat de huidige procespapieren opleveren, wat onvoldoende werkt en hoe verbetering wordt vastgesteld. Rinse heeft inmiddels ervaringen en met Jeroen bevestigde startafspraken aangeleverd. Deze zijn hieronder verwerkt als door Rinse gerapporteerde gegevens; de onderliggende Jira-historie is niet onafhankelijk gecontroleerd.

De checklist sluit inhoudelijk aan: zij bevat intake, analyse, ontwerp, implementatie, test, contactmomenten met Jeroen en evaluatie van tijd en voortgang. Samenwerking is dus niet alleen een aanvulling, maar deels al voorzien.

> [!IMPORTANT]
> Samen een fase uitvoeren is niet hetzelfde als goedkeuring krijgen. De checklist vraagt expliciete toestemming voor bepaalde vervolgstappen. Leg daarom vast wanneer samenwerking gewenst is en wanneer een akkoord noodzakelijk is.

De projectdocumentatie beschrijft analyse, ontwerp en test momenteel als interne fasen, met afzonderlijke reviewfasen. Gezamenlijke uitvoering is een aanvullende werkafspraak; daarmee veranderen de Jira-statussen niet automatisch.

## Bevestigde startafspraken en resterende verificatie

De volgende gegevens zijn door Rinse aangeleverd als met Jeroen bevestigd:

| Onderdeel | Vastgestelde afspraak |
|---|---|
| Start | Maandag 19 oktober 2026; procesbeoordeling na vier weken op 16 november 2026; definitieve cohort-rework na acht weken op 14 december 2026. Instroom: 19 oktober 2026 09:00:00 inclusief tot 12 november 2026 17:00:00 exclusief, Europe/Amsterdam. Deze instroomgrens vervroegt niet automatisch de opleveringsgrens voor cohort-rework; exacte evaluatietijdstippen nog bevestigen. |
| Registratie | Veld `Description`, vaste sectie onderaan met kop `### Checklist Esther v34`. Geen ingevulde checklist in bijlagen of elders. |
| Scope | `Story`, `Task`, `Bug`; geen `Sub-task`, `Hotfix`, administratieve of vóór 19 oktober gestarte taken. Alleen intake tijdens de vierweekse instroomperiode. |
| Documentatiestatus | Aanvulling op bestaande procesdocumentatie; geen bestaande regels vervallen. |
| Werkrooster | Maandag t/m donderdag 09:00–17:00, 32 werkuren per normale week. Vrijdagen, weekenden en feestdagen uitgesloten. Europe/Amsterdam; officiële Nederlandse feestdagenkalender, volgens gebruikersbevestiging geen uitzonderingsdagen tussen 19 oktober en 14 december 2026. Bereken lokale werkuren, niet met een vaste UTC-offset. |
| DoD | Code gemerged naar doelbranch met geslaagde pipeline; checklist 100% gevuld (`Afgerond` of gemotiveerd `N.v.t.`); expliciet acceptatiecommentaar van Jeroen aanwezig. |
| Responstijd | 16 werkuren vanaf akkoordaanvraag. |
| Escalatie | Naar Esther als proceseigenaar/lead; haar escalatierol verleent niet automatisch de bevoegdheid namens Jeroen akkoord te geven. |
| Parkeren | Na 8 extra werkuren zonder besluit na escalatie. Herbeoordeling wekelijks maandag 10:00 tijdens planning. |
| Norm overschrijdingen | Maximaal 10% van alle akkoordaanvragen overschrijdt 16 werkuren, en iedere overschrijding krijgt direct een escalatienotitie en `Flagged`. Open aanvragen voorbij de termijn tellen mee als overschrijding. |

De aangeleverde puntcode-referentielijst geldt als de door Rinse als goedgekeurd gemelde pilotreferentie, niet als bewijs dat deze codes letterlijk in het Word-document staan. De lijst staat volledig in het intakepakket. Bronmapping en behoud van alle bestaande checklistverplichtingen moeten vóór intake nog worden gecontroleerd; een korte puntnaam vervangt niet de onderliggende checklistinhoud.

Onderstaande eerder geformuleerde reviewregels blijven gelden voor zover niet ingevuld door deze bevestigde afspraken:


- **Procespapieren:** bevestigd als aanvulling; niets wordt afgeschaft.
- **Scope:** voorgesteld zijn Jira-hoofdtaken van type `Story`, `Task` en `Bug`. `Sub-task`, expliciet als `Hotfix` gemarkeerde issues en zuiver administratieve handelingen vallen buiten de pilot. Bevestig de werkelijk beschikbare issuetypes en de afbakening vóór de start. Uitzonderingen worden met reden in het ticket vastgelegd; bestaande kwaliteits- en akkoordregels blijven gelden. Taken die vóór de pilot-startdatum al zijn gestart, worden categorisch uitgesloten van de pilot en doorlopen de bestaande werkwijze. De pilot-cohort bestaat uitsluitend uit taken waarvan de intake op of na de startdatum en binnen de vierweekse instroomperiode aanvangt; geen retroactieve invoering op lopende taken.
- **Vastlegging:** de ingevulde checklist staat uitsluitend in één vaste tekstcomponent in de Jira-beschrijving of één specifiek tekstveld, met stabiele puntcodes. Wijs vóór de start precies één veld aan. Bijlagen, lokale Word-kopieën en parallelle registratie in Confluence zijn uitgesloten. Alleen Rinse of de per ticket expliciet aangewezen waarnemer muteert de checkliststatus in dit veld; op ieder moment is er precies één bevoegde bewerker volgens de overdrachtsclausule in sectie 2. Jeroen of de vooraf bevoegde vervanger valideert akkoorden en afwijzingen uitsluitend via ticket-commentaren; Rinse verwijst vanuit het checklistpunt naar het betreffende commentaar. Commentaren vormen de beslisgeschiedenis, niet een tweede checklist. Gebruik uitsluitend de aangeleverde goedgekeurde referentielijst voor versie 34 RC. Afwijkende of tijdens de pilot zelf samengestelde coderingen zijn niet toegestaan; controleer de mapping naar de bron vóór intake.

> [!IMPORTANT]
> De uitgelezen Word-tekst bevestigt nog geen expliciete codes zoals `INT-01` of `ANA-02`; deze voorbeelden gelden daarom niet als geverifieerde broncodes. Controleer vóór de formele intake de visuele sectie- en itemnummering in het brondocument en leg de letterlijk overgenomen referenties vast. Indien de bron geen unieke puntcodes bevat, is eerst een expliciet goedgekeurde referentielijst nodig; Rinse en Jeroen introduceren geen eigen nummering tijdens de pilot.
- **Escalatieprotocol:** bepaal de maximale responstijd voor Jeroen in werkuren, de escalatieontvanger of bevoegde vervanger en de vervolgtermijn. Definieer tevens de wachtregistratie en de status bij overschrijding. Zonder deze afspraken start de pilot niet.
- **Niet-toepasselijk:** gebruik `N.v.t.` met een concrete reden. Een verplicht akkoordmoment kan niet eenzijdig als niet-toepasselijk worden afgedaan.
- **Pilot:** bevestig startdatum, vierweekse looptijd, procesbeoordeling op week 4 en definitieve rework-evaluatie op week 8. Bevestig meetdefinities, historische vergelijking of baseline-aanpak en vooraf een numerieke acceptatiegrens voor responstijdoverschrijdingen.

> [!WARNING]
> `Blocked` is in de beschreven Jira-workflow geen generieke wachtstatus: de gedocumenteerde uitgangen gaan alleen naar Implementing en Testing. `In Review` is niet als status gedocumenteerd. Gebruik daarom voorlopig de bestaande fase- of reviewstatus met een expliciete wacht-/blokkaderegistratie in het ticket. Kies `Blocked` alleen als de benodigde heen- én terugtransitie al beschikbaar zijn. Een workflowwijziging valt buiten dit plan.

## Voorgestelde stappen na goedkeuring

### 1. Evaluatie onderbouwen

Door Rinse gerapporteerde ervaringen:
- De fasering Analyse, Ontwerp, Implementatie en Test biedt theoretische houvast; de technische acceptatiecriteria voor code-oplevering zijn inhoudelijk helder.
- **IT-142:** mondeling afgestemde eisen zonder vastgelegde randvoorwaarden. Tijdens ontwerp-review bleek de datastructuur strijdig met niet-gedocumenteerde afhankelijkheden. Gevolg: 14 uur rework, herhaalde analyse en volledige ontwerpherziening. Preventieve koppeling: `INT-01`, `INT-02`, `DES-02` en expliciete akkoordregistratie.
- **IT-158:** vijf werkdagen review zonder terugkoppeling; implementatie gestart zonder afgesproken escalatie. Jeroen wees het analysevoorstel later af. Gevolg: implementatiecode onbruikbaar, doorlooptijdverlies en dubbel werk; het aantal hersteluren is niet opgegeven. Preventieve koppeling: `ANA-03`, de akkoordgrens, vlagzichtbaarheid en het escalatie-/parkeerprotocol.

Deze casussen onderbouwen de werkafspraak, maar vormen zonder volledige vergelijkbare historische meetgegevens geen kwantitatieve nulmeting.

### 2. Werkafspraken met Jeroen vastleggen

- Gebruik één checklist per Jira-hoofdtaak binnen de scope, vanaf intake en met hergebruik bij iteraties.
- Leg taaknummer, checklistversie, resultaat en initiële effortschatting vast.
- Voer gezamenlijke sessies uitsluitend uit voor analyse, ontwerp en test. Andere verplichte contact- en akkoordmomenten blijven bestaan.
- Alleen Rinse registreert ieder checklistpunt als `Open`, `Afgerond` of `N.v.t.` in het aangewezen veld; bij `N.v.t.` is een reden verplicht. Bij officiële afwezigheid of overdracht van een taak wijst Rinse de specifieke waarnemer expliciet aan in een ticket-commentaar, met ingangsdatum en eindmoment of terugnamevoorwaarde. Deze waarnemer verkrijgt daarmee tijdelijk het exclusieve bewerkingsrecht voor dat ticket; Rinse bewerkt het veld gedurende die periode niet. Zonder expliciete overdracht blijft de mutatiestop voor anderen van kracht. Terugname wordt eveneens in een commentaar vastgelegd, zodat nooit twee bewerkers tegelijk bevoegd zijn. Dit is een procesmandaat, geen automatische wijziging van Jira-permissies. Bewaar de historie binnen het ticket.
- Tijdens een geldige overdracht voert de waarnemer de in dit plan aan Rinse toegewezen ticketregistratie, akkoordaanvragen, vlagbeheer en escalaties uit. Het mandaat verleent geen bevoegdheid om namens Jeroen akkoord te geven; formele toetsing blijft gescheiden van checklistbewerking.
- Rinse logt akkoordaanvragen in ticket-commentaren met puntcode, gevraagde beslissing, aanvrager, beoordelaar en tijdstempel. Jeroen of de bevoegde vervanger logt daar zelf het expliciete akkoord of de afwijzing. Rinse koppelt de veldregistratie aan dit besluitcommentaar, zonder zelf namens de toetser akkoord te geven.
- Zonder verplicht akkoord geen overgang naar de volgende inhoudelijke fase. Administratieve wachtregistratie geeft geen inhoudelijke toestemming.
- Gebruik de bestaande evaluatiestappen voor tijdsoverschrijding en meld afwijkingen tijdig.

#### Verbruikte effort registreren — Jira Work Log

**Voorgestelde registratieafspraak, ter bekrachtiging in het besluitpakket:** standaard Jira `Log Work` is de enige gezaghebbende registratie van werkelijk bestede effort. Geen onafhankelijke cumulatieve urenteller in Description. Leg bij intake de initiële schatting onveranderlijk vast in een ticket-commentaar; een gewijzigde Remaining Estimate vervangt deze referentie niet.

- Rinse of de aangewezen waarnemer logt iedere aaneengesloten werksessie en bij iedere taak-/fasewisseling, met uitvoerder, lokaal startmoment, werkelijk bestede duur, fase en korte activiteitomschrijving. Tijdstip van invoer is niet automatisch tijdstip van uitvoering.
- Controleer vóór iedere implementatiesessie het cumulatieve verbruik tegenover de initiële schatting. Beperk de sessie tot het resterende effortbudget; registreer en onderbreek direct bij de 100%-grens. Alleen achteraf per dag loggen is onvoldoende om de interrupt te handhaven. Een lokale sessietimer mag als hulpmiddel dienen, maar is geen tweede officiële gegevensdrager.
- Log werkelijk bestede tijd, niet de roosterduur of passieve reviewwachttijd. Wachtintervallen blijven afzonderlijk herleidbaar uit aanvraag-/besluitcommentaren. Corrigeer foutieve worklogs met een traceerbare toelichting.
- Leg tijdregistratie op de pilot-hoofdtaak vast. Uren van gekoppelde subtaken tellen niet automatisch mee: bij gedelegeerd werk moet de gekozen hoofdtaakregistratie expliciet worden gevolgd, zonder dezelfde tijd ook op de subtaak te loggen. Taken buiten de scope mogen geen effort aan de pilot toevoegen.
- De initiële schatting en worklogsom gebruiken dezelfde eenheid: uren. Controleer vooraf Jira-tijdconversie, zichtbaarheid en rechten voor toevoegen/corrigeren; invoer van dagen mag niet ongemerkt een afwijkende Jira-daglengte gebruiken.
- Beschikbaarheid, permissies en een controleerbare cumulatieve worklogsom worden live geverifieerd. Bij ontbrekende Work Log-functionaliteit blijft de pilot geblokkeerd; een alternatieve teller vereist expliciete herziening en akkoord.

#### Wachten, escaleren en parkeren

**Voorgestelde bindende effort-interrupt, te accorderen vóór vrijgave:** `AFR-02` blijft de post-mortem tijdsevaluatie en DoD-gerelateerde afrondingscontrole; het is geen vervanging voor tussentijdse effortsturing. Zodra daadwerkelijk verbruikte uitvoerings-effort 100% van de bij intake vastgelegde initiële schatting bereikt, stopt verdere implementatie tot verplichte afstemming met Jeroen en een expliciet vervolgakkoord in het ticket. Registreer initiële schatting, verbruikte effort, resterende schatting, afwijkingsreden en besluit. De initiële referentieschatting wordt niet achteraf verhoogd om de trigger te omzeilen; een herziene prognose wordt apart vastgelegd.

Effort is werkelijk bestede uitvoertijd, niet verstreken werkroostertijd of reviewwachten. Wachten op het effortbesluit krijgt `Flagged` en hetzelfde 16+8-werkurenprotocol; de effortmeting en de responstijdklok blijven afzonderlijke grootheden. Nog formeel vast te stellen: trigger op taak- of faseniveau, werkwijze voor nul/ontbrekende schatting en vervolgtriggers na een akkoord. Registratiemethode is het voorgestelde Jira Work Log-protocol hierboven; dit wordt in het besluitpakket bekrachtigd. De bronsturing bij circa 2/3 en 150% mag niet verdwijnen zonder expliciete dispositie na visuele broncontrole.


1. Bij aanvraag begint de responstijd te lopen volgens het afgesproken werkrooster. Het ticket vermeldt `Wacht op akkoord`, de verantwoordelijke en de deadline; het blijft in de toepasselijke bestaande fase- of reviewstatus. Bij registratie van `Wacht op akkoord` markeert Rinse het issue tevens met een Jira-vlag (`Flagged` / `Add Flag`), zonder de status te wijzigen. Rinse verwijdert de wachtvlag direct zodra het formele akkoordcommentaar is geplaatst, mits geen andere belemmering resteert. Bij een andere belemmering blijft de vlag actief met een bijgewerkte reden. Controleer vóór de intake dat vlaggen beschikbaar zijn en op het gebruikte bord zichtbaar worden.
2. Bij overschrijding van **16 werkuren** registreert Rinse de blokkade en escaleert naar **Esther (proceseigenaar/lead)**. Gebruik alleen een Jira-statusovergang die voor deze fase aantoonbaar beschikbaar is.
3. Blijft een besluit uit binnen **8 werkuren na escalatie**, dan wordt het afhankelijke werk geparkeerd. Noteer reden, eigenaar en **het eerstvolgende wekelijkse planningsmoment op maandag 10:00**. Bij parkeren blijft de vlag actief en wordt de reden in het blokkadecommentaar bijgewerkt. Rinse kan ander, onafhankelijk werk oppakken, maar passeert geen akkoordgrens. Bij daadwerkelijke hervatting verwijdert Rinse de vlag zodra geen belemmering meer resteert; hervatting van herzieningswerk na afwijzing geeft geen toestemming voor de volgende inhoudelijke fase.
4. Hervatten van de volgende inhoudelijke fase gebeurt uitsluitend na een expliciet akkoord van Jeroen of een vooraf aangewezen bevoegde vervanger. Leg het besluit en het einde van de wachttijd vast. Bij afwijzing controleert Rinse of de bestaande workflow een passende terugwaartse transitie toestaat en gebruikt deze indien beschikbaar. Volgens de geraadpleegde workflow kan `Review design` via `Review Nok` terug naar `Designing`; voor `Review Analyse` en `Review Final` is geen afwijzingstransitie gedocumenteerd. Verifieer dit vóór de pilot tegen de actieve Jira-configuratie. Bij afwijzing blijft de Jira-status ongewijzigd indien terugwaartse transities technisch ontbreken. Rinse logt `Afgekeurd - herziening vereist` administratief in het ticket totdat een nieuw akkoord is aangevraagd. Herzieningswerk passeert geen akkoordgrens; na herziening volgt een nieuwe aanvraag met verwijzing naar de eerdere afwijzing.

De termijnen en escalatieontvanger zijn inmiddels door Rinse als met Jeroen bevestigd aangeleverd. Een eventuele vervanger met formele akkoordbevoegdheid is niet aangewezen; Esther is hier escalatieontvanger.

### 3. Voorstel aan Jeroen aanscherpen

De oorspronkelijke bespreektekst hieronder is achterhaald door de aangeleverde bevestigde afspraken. De definitieve bevestigingsmail staat in [intakepakket.artifact.md](file:///C:/Users/Rinse/AppData/Local/Google/AndroidStudio2026.2.1/projects/issuetracker.5c12e1c0/.artifacts/dcdd2b17-76a8-4c92-a36e-1284a462096f/intakepakket.artifact.md); zij is nog niet verstuurd.

Oorspronkelijke bespreektekst (historie):

> Jeroen,
>
> Voorstel voor het uniformeren van de werkstroom:
> 1. We hanteren de checklist van Esther integraal per Jira-taak (hoofdtaken binnen de afgesproken scope waarvan de intake tijdens de pilot begint). Reeds lopende taken blijven buiten de pilot en volgen de bestaande werkwijze.
> 2. Gezamenlijke sessies worden beperkt tot: analyse, ontwerp en test.
> 3. De checklist staat in één Jira-tekstveld en wordt uitsluitend door mij bijgewerkt, of tijdelijk door één expliciet aangewezen waarnemer bij overdracht. Bij wachten op jouw akkoord markeer ik het ticket op het bord met een vlag (`Flagged`); bij parkeren blijft die actief. Jij valideert formele akkoorden en afwijzingen via ticket-commentaren. Zonder akkoord geen statusovergang naar de volgende inhoudelijke fase.
> 4. We draaien dit als pilot van vier weken, startend op [datum]. Procesflow en wachttijden beoordelen we op week 4; rework definitief op week 8.
>
> Bespreekpunten voor akkoord:
> - Welk enkel Jira-tekstveld wijzen we aan voor de checklist (beschrijving of specifiek tekstveld)? Bijlagen zijn uitgesloten.
> - Welke responstijd hanteren we voor jouw verificatiemomenten voordat een taak als geblokkeerd geldt?
> - Naar wie escaleren we, en wanneer parkeren en herbeoordelen we het werk?
> - Bevestigen we de scope en uitzonderingen zoals beschreven in dit plan?
> - Gaan we akkoord met de referentielijst voor checklist-puntcodes zoals afgeleid uit versie 34 RC? Deze lijst moet vóór de eerste pilot-intake zijn vastgesteld.
>
> Rinse

Ervaringen en startafspraken zijn verwerkt. Resterende controles betreffen de actieve Jira-configuratie, bronmapping van de referentielijst, tijdzone/feestdagenkalender en exacte pilotgrenzen. Zonder praktijkgegevens kunnen de toekomstige evaluaties niet worden uitgevoerd.

### 4. Gebruik evalueren

- Start na overeenstemming met alle hoofdtaken binnen de scope; kies niet alleen een steekproef. Taken die vóór de pilot-startdatum al zijn gestart, worden categorisch uitgesloten van de pilot en doorlopen de bestaande werkwijze. De pilot-cohort bestaat uitsluitend uit taken waarvan de intake op of na de startdatum en binnen de vierweekse instroomperiode aanvangt. Gebruik hetzelfde vastgelegde start- en eindtijdstip voor alle metingen.
- Meet vier weken vanaf de vastgestelde startdatum. Rapporteer succesvol opgeleverde, nog onderhanden/wachtende en voortijdig geannuleerde, gestaakte of ingetrokken taken afzonderlijk. Succesvol opgeleverd betekent dat de afgesproken Definition of Done (DoD) aantoonbaar is behaald; alleen een terminale Jira-status is daarvoor onvoldoende.
- Leg vóór de start zo mogelijk een vergelijkbare historische periode vast als referentie. Indien geen vergelijkbare historische referentieperiode kan worden vastgesteld, dienen de pilotuitkomsten als initiële nulmeting (baseline), niet als vergelijkend experiment. Dit blokkeert besluitvorming niet; zonder betrouwbare referentie worden geen relatieve verbeteringen of causale effecten geclaimd.
- Besluitvorming vindt bij een baseline plaats op basis van absolute acceptatiecriteria: checklistvolledigheid = 100% en nultolerantie op ongeregistreerde akkoorddoorbrekingen. Iedere overgang voorbij een verplichte akkoordgrens zonder voorafgaand formeel akkoord is tevens een procesafwijking, ook als zij achteraf wordt geregistreerd.
- Leg vóór de start een numerieke acceptatiegrens voor responstijdoverschrijdingen vast. Zonder vastgestelde grens worden overschrijdingen gerapporteerd, maar niet als acceptabel geclassificeerd. Bij ontbrekende succesvol opgeleverde taken is volledigheid niet meetbaar en kan die norm niet als behaald gelden.
- De pilot voor invoer van de werkwijze duurt vier weken. De proces-eindevaluatie vindt na vier weken plaats voor procesflow en wachttijden; het besluit over voortzetten of aanpassen is voor rework voorlopig. De definitieve evaluatie van de rework-parameter vindt plaats op week 8: pilot-einddatum + vier weken observatietijd.

| Parameter | Definitie en registratie |
|---|---|
| Checklistvolledigheid (DoD-nalevingsaudit, geen effectmaat) | Aantal succesvol opgeleverde pilot-taken binnen de scope met alle punten `Afgerond` of onderbouwd `N.v.t.`, gedeeld door alle succesvol opgeleverde pilot-taken binnen de scope × 100%. Norm: 100%. Open punten tellen niet als ingevuld. Voortijdig geannuleerde, gestaakte of ingetrokken taken tellen niet mee in deze berekening, maar worden afzonderlijk gerapporteerd met reden van uitval. |
| Gemiddelde akkoordwachttijd | Som van werkuren tussen aanvraag en expliciet akkoord, gedeeld door het aantal toegekende akkoorden. Rapporteer afwijzingen apart. Open aanvragen tellen niet als nul: vermeld aantal en actuele wachtduur. |
| Responstijdoverschrijdingen | Aantal aanvragen voorbij de afgesproken responstijd en aantal daarvan met tijdig gelogde escalatie. |
| Pilot-doorrol (WIP op week 4) | Aantal taken binnen scope gestart tijdens de pilot maar niet opgeleverd op dag 28, gedeeld door totaal aantal gestarte taken binnen scope × 100%. Start is het vastgelegde begin van intake. Dient ter duiding van selectiebias in de rework-cohort. Rapporteer aantallen en redenen voor niet-oplevering, inclusief parkeren of annulering, zonder deze uit de noemer te verwijderen. |
| Rework na oplevering | Aantal afzonderlijke herstelincidenten wegens een tekortkoming in opgeleverd werk, gekoppeld aan het oorspronkelijke ticket. Nieuwe scope telt niet mee. Tel ieder incident éénmaal. |
| Doorlooptijd en reviewwachten | Meet per succesvol opgeleverde pilot-taak zowel de totale doorlooptijd (start intake tot oplevering) als de som van reviewwachtintervallen in werkuren volgens hetzelfde afgesproken werkrooster. Rapporteer het aandeel wachttijd als: (totale reviewwachttijd in werkuren / totale doorlooptijd in werkuren) × 100%. Overlappende wachtintervallen tellen eenmaal en vallen binnen het gemeten doorlooptijdinterval. Bij nul werkuren doorlooptijd is de ratio `Niet meetbaar`. |

Voor de definitieve rework-evaluatie omvat de cohort uitsluitend succesvol opgeleverde pilot-taken binnen de scope waarvan zowel de intake als de oplevering binnen de vierweekse pilotperiode vallen. Voortijdig geannuleerde, gestaakte of ingetrokken taken vallen buiten checklistvolledigheid en rework; ze blijven wel in teller en noemer van pilot-doorrol/uitval zolang geen succesvolle oplevering heeft plaatsgevonden. Rapporteer bij pilot-doorrol de werkelijk nog onderhanden taken en beëindigde uitval afzonderlijk, zodat het gecombineerde percentage niet wordt aangezien voor uitsluitend actief WIP. Reeds vóór de pilot gestarte taken zijn van alle pilotmetingen uitgesloten. Per taak worden incidenten binnen vier weken na oplevering geteld. Bij de initiële evaluatie op week 4 rapporteert Rinse uitsluitend het voorlopige aantal incidenten met de actuele observatieduur per taak. Op week 8 heeft ook een taak opgeleverd op de pilot-einddatum het volledige venster doorlopen; dan wordt de definitieve rework-parameter vastgesteld. Taken die pas na de pilot worden opgeleverd, worden afzonderlijk gevolgd en vallen niet in deze week-8-cohort. Een eventuele historische rework-vergelijking vereist hetzelfde observatievenster en dezelfde incidentdefinitie. Bij een lege noemer is een percentage of gemiddelde `Niet meetbaar`, niet nul.

Indien de pilot-doorrol hoger is dan 20%, geldt de cohort-rework op week 8 als indicatief en niet als representatief voor complexe taken of de totale werkstroom. Vermeld dit expliciet in de evaluatie en bespreek de niet-opgeleverde taken afzonderlijk. Een doorrol van maximaal 20% bewijst op zichzelf geen representativiteit.

### DoD-audit op week 4

Audit alle pilot-tickets die als `Done`/`Resolved` of anderszins opgeleverd zijn gemarkeerd, ook als zij niet aan de DoD voldoen. Rapporteer ontbrekende volledige brondekking, onvolledige checklist, ontbrekend acceptatiecommentaar en overige DoD-schendingen als formele procesafwijkingen met ticketreferentie en herstelactie. Deze tickets zijn niet succesvol opgeleverd, maar blijven zichtbaar in de audit en uitvalregistratie. Een automatisch 100%-percentage bij de DoD-cohort is geen effectbewijs.

## Verificatieplan

### Inhoudelijke controle

- Controleer of contactmomenten, tijdsevaluatie en akkoorden overeenkomen met de checklist.
- Controleer of geen bestaande procespapieren of Jira-statussen ongemerkt worden vervangen.
- Laat Jeroen de rolverdeling, taakdefinitie en akkoordmomenten bevestigen.
- Controleer vóór de start de feitelijk beschikbare afwijzings-/terugtransities in Jira en de administratieve terugvalregistratie bij ontbrekende transities.
- Controleer dat één tekstveld en per ticket op ieder moment één checklistbewerker zijn aangewezen; verifieer overdrachts- en terugnamecommentaren bij waarneming. Toetserbesluiten worden uitsluitend via ticket-commentaren vastgelegd.
- Controleer dat taken met intake vóór de pilot-startdatum van alle pilotmetingen zijn uitgesloten en niet retroactief naar de nieuwe werkwijze worden overgezet.
- Leg baseline of vergelijkingsperiode, absolute acceptatiegrenzen en beide evaluatiedata vóór de start vast.
- Leg de DoD en het gemeenschappelijke werkrooster vast. Controleer dat doorlooptijd en reviewwachten beide in werkuren worden berekend en dat terminale Jira-statussen zonder succesvolle oplevering niet in checklistvolledigheid of rework meetellen.
- Verifieer vóór de formele intake brongetrouwe, eenduidige puntreferenties en identiek gebruik daarvan in veld en commentaren.
- Controleer op het gebruikte Jira-bord de zichtbaarheid van `Flagged` bij wachten en parkeren, en de werkwijze voor verwijderen bij akkoord of hervatting.

### Praktijkcontrole

Controleer per taak binnen de scope:
- **Registratie:** is de checklist traceerbaar opgeslagen binnen het Jira-ticket, op de afgesproken enige locatie?
- **Flow:** zijn alle verplichte beslismomenten expliciet afgetekend met beoordelaar, besluit en tijdstempel in het ticket?
- **Validatie:** zijn niet-toepasselijke punten gemarkeerd als `N.v.t.` met een eenduidige reden?
- **Escalatie:** zijn overschrijdingen, escalaties, parkeren en hervatten volgens de afgesproken termijnen geregistreerd?
- **Impact:** hoeveel van de doorlooptijd bestaat aantoonbaar uit reviewwachttijd? Vergelijk dit met de referentieperiode indien beschikbaar; dit aandeel bewijst op zichzelf geen causale vertraging.
- **Bordzichtbaarheid:** zijn wachtende en geparkeerde tickets gevlagd, met actuele reden, en worden opgeloste belemmeringen direct verwijderd?
- **Traceerbaarheid:** gebruiken Rinse en de toetser exact dezelfde, gecontroleerde bronreferenties voor ieder checklistpunt?
- **Meetbaarheid:** zijn volledigheidspercentage, akkoordwachttijd, pilot-doorrol en rework-aantal reproduceerbaar uit de ticketregistratie? Is bij doorrol boven 20% de beperkte representativiteit expliciet vermeld?

Er zijn voor dit werkafsprakenplan geen builds of softwaretests nodig.
