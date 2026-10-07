# Brondekking — Checklist Esther v34 RC versus 17 referenties

## Reikwijdte en oordeel

Bron: `C:/Zaibatsu89/IssueTracker/Checklist Esther/Checklist Esther versie 34 RC oktober 2025.docx`.
Vergelijkingsdocument: `intakepakket.artifact.md` in deze artifactdirectory, inclusief korte labels, bevestigingsmail, commentaarsjablonen en metingen/startcontrole.

Dit is uitsluitend brononderzoek. Geen bron, intakepakket, Jira, planningstate of lifecycle gewijzigd. Geen nieuwe puntcodes voorgesteld of ingevoerd. De nummers in de eerste kolom zijn uitsluitend matrixrijnummers. Codekoppelingen zijn inhoudelijke reviewvoorstellen binnen de bestaande 17 referenties, geen vastgestelde uitbreiding van hun definitie.

**Oordeel:** de korte 17 labels zijn geen volledige operationele vervanging voor de Word-checklist. Het intakepakket zegt dit zelf en verlangt gebruik van de complete onderliggende checklist, maar legt de specifieke onderliggende eisen nog niet traceerbaar vast per code. Die algemene verwijzing telt hier niet als volledige dekking van iedere bronzin. Een inhoudelijk passende code met ontbrekende concretisering is `deels`; een wezenlijk ander proces, ontbrekende zelfstandige handeling of alleen een speculatieve codekoppeling is `ontbreekt`. `volledig` betekent uitsluitend dat het onderzochte intakepakket de afzonderlijke eis inhoudelijk voldoende expliciet omvat; niet dat uitvoering of bewijs is geverifieerd.

De matrix bevat **149 afzonderlijke controle-eisen: 9 volledig, 121 deels en 19 ontbreekt**, ook uit TOELICHTING en EVALUATIE. Samengestelde vragen zijn gesplitst; dezelfde verplichting in verschillende fasen is per fase opgenomen. Decoratieve losse cijfers, lege stippen en niet benoemde percentages zijn geen zelfstandig bewezen eis. Benoemde registratievelden zijn wel opgenomen. Er is geen visueel afgeleide associatie tussen een zwevend veld en een procesfase aangenomen.

## Extractie, bronverwijzing en visuele beperking

Read-only .NET `ZipFile`/XML-extractie van `word/document.xml` via PowerShell. Er zijn 63 `mc:AlternateContent`-elementen met ieder één Choice/Fallback-paar en 126 tekstvakcontainers. De teksten van alle 63 paren zijn gelijk. Alleen Choice is meegenomen; Fallback is uitgesloten. Tekst is per eigen `w:p` verzameld, met uitsluitend `w:t` waarvan die paragraaf de dichtstbijzijnde paragraafvoorouder is. Daarmee zijn tekstvakken behouden zonder de duplicatie door zowel hun ankerparagraaf als hun eigen paragrafen. Geen geneste AlternateContent; geen `w:t` buiten body; geen media-, header- of footeronderdelen gevonden in de onderzochte pakketlijst.

`Pnnn` verwijst naar de volgorde van de 85 niet-lege, aldus ontdubbelde tekstparagrafen, **niet** naar paginanummers of checklistnummers. P034 bevat de hoofdtekst ANALYSE t/m TEST in één XML-paragraaf; P055 bevat EVALUATIE. Fragmenten zijn letterlijk overgenomen, inclusief bronspelling, hoofdletters en aangehechte cijfers. Lange spaties en afbrekingen door consoleweergave zijn niet als inhoudelijke eisen geïnterpreteerd. Bij samengestelde registratievelden zijn exacte losse tekstfragmenten afzonderlijk geciteerd.

Read-only visuele mogelijkheden onderzocht: Word COM is geregistreerd; `C:\Program Files\Microsoft Office\Root\Office16\WINWORD.EXE` is aanwezig. Word/LibreOffice-executables werden niet via PATH gevonden. Er is geen Windows-desktop-screenshot- of Word-render-tool beschikbaar in deze sessie; beschikbare UI/screenshot-tools richten zich op Android. Word is niet gestart en er is geen PDF/export/previewbestand gemaakt, omdat alleen deze matrix geschreven mag worden. **Geen visuele verificatie uitgevoerd.** Posities, pijlen, kleur, vinkvakrelaties, tijdtrigger-layout en koppeling van losse velden aan fasen blijven visueel te controleren, bijvoorbeeld door de bron handmatig read-only in de aanwezige Word-installatie te openen. XML-tekstdekking is niet hetzelfde als Word-layoutdekking.

## Dekkingsmatrix

| Rij | Bronsectie / XML-paragraaf | Afzonderlijke controle-eis en letterlijke brontekst | Relevante bestaande code | Dekking | Gat / noodzakelijke concretisering voor review |
|---:|---|---|---|---|---|
| 1 | Algemeen, P011 | Vooraf ruwe schatting: “Bepaal voordat je begint een ruwe effort schatting” | INT-03 | volledig | Effortschatting is expliciet intakeonderdeel; geen afzonderlijk tekstgat voor deze eis. |
| 2 | Algemeen, P009 | Titelregistratie: “Titel:” | INT-01 | deels | Scope benoemd, maar titelveld/verplichte titelregistratie niet vastgelegd. |
| 3 | Algemeen, P006 | Startregistratie: “Start:  .....” | AFR-02 | deels | Intake- en aanvraagmomenten worden geregistreerd; algemene taakstart/bronveld niet expliciet gemapt. |
| 4 | Algemeen, P004 | Stopregistratie: “Stop:  .....” | AFR-02 | deels | Oplevering/uitval genoemd; precieze betekenis van bronveld Stop en registratie niet vastgelegd. |
| 5 | Algemeen, P002/P005/P007/P008 | Effortregistratie: “Effort:  .....” | INT-03; AFR-02 | deels | Schatting wel aanwezig; betekenis van herhaalde effortvelden en werkelijk bestede effort niet bepaald. |
| 6 | Algemeen, P031 | Nieuwe bug als afzonderlijk issue: “Maak een nieuwe issueals je een bug of refactor tegen komtof als je een feature wilt indienen5” | INT-01 (dichtstbij) | ontbreekt | Pilot omvat Bugs, maar verplicht nieuw issue aanmaken voor gevonden bug ontbreekt. |
| 7 | Algemeen, P031 | Refactor als afzonderlijk issue: “Maak een nieuwe issueals je een bug of refactor tegen komtof als je een feature wilt indienen5” | INT-01 (dichtstbij) | ontbreekt | Verplicht issue voor refactor ontbreekt; Task-label bewijst dit niet. |
| 8 | Algemeen, P031 | Feature indienen als afzonderlijk issue: “Maak een nieuwe issueals je een bug of refactor tegen komtof als je een feature wilt indienen5” | INT-01 (dichtstbij) | ontbreekt | Verplicht issue aanmaken voor feature ontbreekt. |
| 9 | INTAKE, P032 | Intake maximaal vijf minuten: “INTAKE (max. 5 minuten)” | INT-03 (dichtstbij) | ontbreekt | Geen intake-timebox; akkoordresponstijd is geen intake-effort/timebox. |
| 10 | INTAKE, P032 | Oplossingsrichting vastleggen: “Oplossingsrichting, deliverable specificatie en initiële schatting gemaakt?” | ANA-02; INT-01 | deels | ANA-02 dekt onderwerp, maar beschikbaarheid al tijdens intake niet vastgelegd. |
| 11 | INTAKE, P032/P051 | Deliverable specificeren: “Oplossingsrichting, deliverable specificatie en initiële schatting gemaakt?”; “Deliverable specificatie:” | INT-01 | deels | Scopebepaling impliceert richting, maar concrete deliverable-specificatie niet verplicht gemaakt. |
| 12 | INTAKE, P032 | Initiële schatting maken: “Oplossingsrichting, deliverable specificatie en initiële schatting gemaakt?” | INT-03 | volledig | Expliciet schattingsonderwerp in intake. |
| 13 | INTAKE, P032 | Contact/controlestap Jeroen: “Controle/contact moment met Jeroen” | INT-01 (dichtstbij) | deels | Mail behoudt bestaande contactverplichtingen, maar afzonderlijk intakecontact niet gemapt. Gezamenlijke uitvoering beperkt intakecontact niet weg. |
| 14 | INTAKE / registratie, P051 | Confidence level voor oplossingsrichting registreren: “Oplossingsrichting:                                                                       Confidence level:       %” | ANA-02; INT-03 | deels | Oplossingsrichting wel, confidence-percentage ontbreekt. Ruimtelijke relatie alleen XML-nabijheid. |
| 15 | ANALYSE, P034 | Aanleiding bepalen: “Wat is de aanleiding of wat is het probleem (BUGS)?” | ANA-01 | volledig | Probleemanalyse omvat aanleiding/probleem; geen specifieke extra methode in dit fragment. |
| 16 | ANALYSE, P034 | Probleem bepalen: “Wat is de aanleiding of wat is het probleem (BUGS)?” | ANA-01 | volledig | Probleemanalyse expliciet aanwezig. |
| 17 | ANALYSE, P034 | Vaststellen of het een bug is: “Is het issue een Bug? → Reproduceer de bug(s)!” | ANA-01 | deels | Probleemanalyse bevat geen expliciete bugclassificatie. |
| 18 | ANALYSE, P034 | Bug reproduceren: “Is het issue een Bug? → Reproduceer de bug(s)!” | ANA-01 | deels | Verplichte reproduceerstap en reproduceerbewijs ontbreken. |
| 19 | ANALYSE, P034 | Oplossingswijze bedenken: “Hoe kun je het oplossen?” | ANA-02 | volledig | Oplossingsrichting expliciet aanwezig. |
| 20 | ANALYSE, P034 | Minimaal twee oplossingen: “Oplossingen bedenken (minimaal 2), gebruik bestaande oplossingen als template voor je ontwerp!” | ANA-02 | deels | Minimumaantal ontbreekt. |
| 21 | ANALYSE, P034 | Bestaande oplossing als template: “Oplossingen bedenken (minimaal 2), gebruik bestaande oplossingen als template voor je ontwerp!” | ANA-02; DES-01 | deels | Hergebruik/template als verplicht toetscriterium ontbreekt. |
| 22 | ANALYSE, P034 | Selecteren op hoogste confidence: “Kies hiervan de oplossing met je hoogste confidence level (KISS), schop de mogelijke oplossingen om → Ik kies...4” | ANA-02 | deels | Selectiecriterium en confidence-afweging ontbreken. |
| 23 | ANALYSE, P034 | Eenvoud/KISS toetsen: “Kies hiervan de oplossing met je hoogste confidence level (KISS), schop de mogelijke oplossingen om → Ik kies...4” | ANA-02 | deels | KISS expliciet opnemen; alleen oplossingsrichting niet voldoende. |
| 24 | ANALYSE, P034 | Alternatieven kritisch beproeven: “schop de mogelijke oplossingen om → Ik kies...4” | ANA-02 | deels | Weerlegging/afweging alternatieven niet vereist. |
| 25 | ANALYSE, P034 | Expliciete referentie gekozen oplossing: “Heb je een expliciete referentie bij je gekozen oplossing benoemd?” | ANA-02 | deels | Referentie/bewijs in besluitaanvraag is niet specifiek oplossingsreferentie/template. |
| 26 | ANALYSE, P034 | Excel-actiepunten verwerken: “Ik heb actiepunten uit Excel bestand verwerkt en Jira geüpdated” | ANA-01; ANA-03 (dichtstbij) | ontbreekt | Excel-actiepunten zijn niet expliciet verbonden aan analyseafronding. Alleen gegevens in Jira sluit verwerking van externe actiepunten niet af. |
| 27 | ANALYSE, P034 | Jira actualiseren: “Ik heb actiepunten uit Excel bestand verwerkt en Jira geüpdated” | ANA-01; ANA-03 | deels | Checklist/besluitregistratie wel; bredere Jira-update na actiepunten niet benoemd. |
| 28 | ANALYSE, P034 | Contact/controlestap Jeroen: “Controle/contact moment met Jeroen” | ANA-03 | deels | Analyse gezamenlijk en akkoord vastgelegd; specifieke contactcontrole moet traceerbaar blijven. |
| 29 | ANALYSE, P034 | Keuze opdrachtgever verklaren: “Waarom heeft de opdrachtgever deze keuze gemaakt? 7” | ANA-01; ANA-02 | deels | Opdrachtgevermotivering niet expliciet verplicht. |
| 30 | ANALYSE, P034 | Noodzaak verklaren: “Waarom is dit nodig?” | ANA-01 | deels | Noodzaak/rationale niet expliciet in analyselabel. |
| 31 | ANALYSE, P034 | Werking verklaren: “Hoe werkt het? Heb je de requirements opgeschreven?” | ANA-01; ANA-02 | deels | Werkingsbeschrijving ontbreekt als acceptatiecriterium. |
| 32 | ANALYSE, P034 | Requirements opschrijven: “Hoe werkt het? Heb je de requirements opgeschreven?” | ANA-01 | deels | Expliciet vastgelegde requirements ontbreken. |
| 33 | ANALYSE, P034 | Bijzondere afhandeling oudere systemen onderzoeken: “Zijn er bijzondere afhandelingen voor oudere systemen? Is de implementatie altijd zo geweest?” | ANA-01; INT-02 | deels | Legacy-uitzonderingen niet expliciet als afhankelijkheid/analysecriterium. |
| 34 | ANALYSE, P034 | Historie implementatie onderzoeken: “Is de implementatie altijd zo geweest?” | ANA-01 | deels | Historische werking/regressieonderzoek ontbreekt. |
| 35 | ANALYSE, P034 | Eenvoud opnieuw toetsen: “Is het inderdaad de meest eenvoudige oplossing?” | ANA-02 | deels | Expliciete hercontrole eenvoud ontbreekt. |
| 36 | ANALYSE, P034 | Verwachte effort afstemmen: “Verwachte effort schatting afstemmen” | INT-03; ANA-03 | deels | Initiële schatting wel; analyse-afstemming met beoordelaar niet expliciet. |
| 37 | ANALYSE, P034 | Expliciete toestemming voor volgende fase: “Geen expliciete toestemming gekregen om de volgende status uit te voeren? → dan terug naar begin analyse.” | ANA-03 | volledig | Mail verbiedt volgende inhoudelijke fase zonder verplicht akkoord; uitvoerderscommentaar is geen akkoord. |
| 38 | ANALYSE, P034 | Zonder toestemming terug naar begin analyse: “Geen expliciete toestemming gekregen om de volgende status uit te voeren? → dan terug naar begin analyse.” | ANA-03 | deels | Wachten/parkeren en herziening bij afwijzing vervangen niet expliciet terug naar begin bij ontbrekende toestemming. Jira-statusbehoud is op zichzelf geen inhoudelijk herstel. |
| 39 | DESIGN/ONTWERP, P034 | Eerst concept kiezen: “Eerst concept kiezen, eventueel pseudo codes invoeren of meteen implementeren.” | DES-01 | deels | Conceptkeuze/volgorde niet expliciet; pseudocode of meteen implementeren zijn bronopties, geen dubbele verplichting. |
| 40 | DESIGN/ONTWERP, P034 | Ontwerpreferentie vastleggen: “Wat is de referentie?11” | DES-01 | deels | Specifieke ontwerp/template-referentie ontbreekt. |
| 41 | DESIGN/ONTWERP, P034 | Exact volgen gekozen template controleren: “Ben ik nog bezig met het implementeren van het gekozen template, op de kop af?” | DES-01; IMP-01 | deels | Conform ontwerp wel; gekozen template exact volgen niet expliciet. |
| 42 | DESIGN/ONTWERP, P034 | Confidence level beoordelen: “Wat is je confidence level?” | DES-01 | deels | Confidence-meting ontbreekt. |
| 43 | DESIGN/ONTWERP, P034 | Verwachte effort eventueel bijstellen: “Verwachte effort schatting eventueel aanpassen” | INT-03; DES-01 | deels | Bijstelmoment/actuele schatting niet vastgelegd. |
| 44 | DESIGN/ONTWERP, P034 | Pseudocode op juistheid controleren: “Is de pseudocode juist?2” | DES-01 | deels | Controle indien pseudocode gebruikt niet expliciet. |
| 45 | DESIGN/ONTWERP, P034 | Plaats pseudocode controleren en herstellen: “Zit hij op de juiste plek? Niet →dan terug!” | DES-01; DES-02 | deels | Plaatsing en herstelactie ontbreken. |
| 46 | DESIGN/ONTWERP, P034 | Proces tot nu bespreken: “Proces tot nu toe bespreken.” | DES-03 | deels | Akkoord ontwerp/gezamenlijk ontwerp wel; procesbespreking niet specifiek. |
| 47 | DESIGN/ONTWERP, P034 | Excel-actiepunten verwerken: “Ik heb actiepunten uit Excel bestand verwerkt en Jira geüpdated” | DES-01; DES-03 (dichtstbij) | ontbreekt | Verwerking Excel-actiepunten ontbreekt als ontwerpcontrole. |
| 48 | DESIGN/ONTWERP, P034 | Jira actualiseren: “Ik heb actiepunten uit Excel bestand verwerkt en Jira geüpdated” | DES-01; DES-03 | deels | Besluiten/checklist wel; bredere actualisatie niet gemapt. |
| 49 | DESIGN/ONTWERP, P034 | Contact/controlestap Jeroen: “Controle/contact moment met Jeroen” | DES-03 | deels | Contactplicht algemeen behouden, niet apart traceerbaar. |
| 50 | DESIGN/ONTWERP, P034 | Effectiviteit oplossing beoordelen: “Geef oordeel over meest effectieve oplossing.” | DES-01; DES-03 | deels | Effectiviteitsoordeel niet verplicht in ontwerpbewijs. |
| 51 | DESIGN/ONTWERP, P034 | Jeroen akkoord voor vervolg: “Is Jeroen akkoord? →door met de opdracht.” | DES-03 | volledig | Expliciet akkoord door Jeroen/bevoegde vervanger en fasegate vastgelegd. |
| 52 | DESIGN/ONTWERP, P034 | Expliciete toestemming volgende fase: “Geen expliciete toestemming gekregen om de volgende status uit te voeren? → dan terug naar begin design/ontwerp.” | DES-03 | volledig | Geen volgende inhoudelijke fase zonder verplicht akkoord. |
| 53 | DESIGN/ONTWERP, P034 | Zonder toestemming terug naar begin ontwerp: “Geen expliciete toestemming gekregen om de volgende status uit te voeren? → dan terug naar begin design/ontwerp.” | DES-03 | deels | Inhoudelijke terugkeer bij ontbreken toestemming niet expliciet; wachten is niet hetzelfde. |
| 54 | IMPLEMENTATIE, P034 | Timer zetten: “Set timer!!” | AFR-02 (dichtstbij) | ontbreekt | Timer tijdens implementatie ontbreekt; meten akkoordwachttijd is iets anders. |
| 55 | IMPLEMENTATIE, P034 | Testen na iedere afgeronde functie: “Telkens als er een functie is afgerond TESTEN” | IMP-02 | deels | Unittests gereed noemt niet functiegewijs testmoment. |
| 56 | IMPLEMENTATIE, P034 | Steppend debuggen: “steppend debuggen” | IMP-02 | deels | Methode ontbreekt; unittests zijn geen steppend debuggen. |
| 57 | IMPLEMENTATIE, P034 | Werkende functie controleren: “check of wat            gemaakt is ook werkt” | IMP-02 | deels | Unittests ondersteunen werking, maar concrete controle per functie niet uitgewerkt. |
| 58 | IMPLEMENTATIE, P034 | 100% dekking gewijzigde code: “100% dekking van gewijzigde code” | IMP-02 | deels | Percentage en scope gewijzigde code ontbreken. |
| 59 | IMPLEMENTATIE, P034 | Eenvoud blijven controleren: “Houd ik me nog aan de eenvoudigste oplossing?” | IMP-01 | deels | Ontwerpconformiteit is niet expliciet eenvoud/KISS. |
| 60 | IMPLEMENTATIE, P034 | Codeconventie volgen: “Houd ik me aan de code conventie?13” | IMP-01; IMP-03 | deels | Conventietoets ontbreekt; reviewverzoek garandeert geen controle. |
| 61 | IMPLEMENTATIE, P034 | Alle If/ELSE-paden testen: “Heb ik alle If en ELSE paden getest?” | IMP-02 | deels | Volledige branch/paddekking niet expliciet. |
| 62 | IMPLEMENTATIE, P034 | Warnings/messages controleren: “Heb je geen warnings/messages?” | IMP-01; IMP-02 | deels | Warning/messagecontrole ontbreekt; geslaagde pipeline is niet noodzakelijk warningvrij. |
| 63 | IMPLEMENTATIE, P034 | Confidence level beoordelen: “Wat is je confidence level?” | IMP-01 | deels | Implementatie-confidence ontbreekt. |
| 64 | IMPLEMENTATIE, P034 | Hulp vragen bij problemen: “Vragen stellen!! →  Niet ieder probleem kún je in je eentje oplossen.” | IMP-01 (dichtstbij) | ontbreekt | Algemene inhoudelijke hulpvraag ontbreekt; escalatie voor uitblijvende akkoorden is beperkter. |
| 65 | IMPLEMENTATIE, P034 | Excel-actiepunten verwerken: “Ik heb actiepunten uit Excel bestand verwerkt en Jira geüpdated” | IMP-01 (dichtstbij) | ontbreekt | Verwerking Excel-actiepunten niet als implementatie-eis vastgelegd. |
| 66 | IMPLEMENTATIE, P034 | Jira actualiseren: “Ik heb actiepunten uit Excel bestand verwerkt en Jira geüpdated” | IMP-01 | deels | Registratie checklist wel; overige Jira-update niet expliciet. |
| 67 | IMPLEMENTATIE, P034 | Expliciete toestemming volgende fase: “Geen expliciete toestemming gekregen om de volgende status uit te voeren? → dan terug naar begin implementatie.” | IMP-03; TST-03 (dichtstbij) | deels | Algemene gate is behouden, maar IMP-03 is alleen reviewverzoek; implementatieakkoord niet als afzonderlijke gate met bewijs/beoordelaar gemapt. |
| 68 | IMPLEMENTATIE, P034 | Zonder toestemming terug naar begin implementatie: “Geen expliciete toestemming gekregen om de volgende status uit te voeren? → dan terug naar begin implementatie.” | IMP-03 (dichtstbij) | ontbreekt | Terugkeer naar begin implementatie ontbreekt; een reviewaanvraag of wachten dekt deze herstelstap niet. |
| 69 | TEST, P034 | Functioneel testen ieder beschikbaar haardtype: “FUNCTIONEEL testen voor ieder soort haard, indien beschikbaar.” | TST-01; TST-02 | deels | Integratie/acceptatietest genoemd; haardtypematrix en beschikbaarheidsvoorwaarde ontbreken. |
| 70 | TEST, P034 | Test compleet uitvoeren: “Is de test compleet uitgevoerd?10” | TST-01; TST-02 | deels | Tests genoemd, maar volledige uitvoering/omvang en bewijs niet expliciet. |
| 71 | TEST, P034 | Eigen code reviewen: “Code review van eigen code” | IMP-03 (dichtstbij) | ontbreekt | Reviewverzoek is geen uitgevoerde self-review. |
| 72 | TEST, P034 | Eigen ontwerp/review uitvoeren: “Eigen review/ontwerp review2” | DES-01; IMP-03 (dichtstbij) | ontbreekt | Uitgevoerde eigen ontwerp-review in testfase ontbreekt. |
| 73 | TEST, P034 | Legacy-afhandelingen controleren: “Heb ik rekening gehouden met bijzondere afhandelingen voor oudere systemen?” | TST-01; TST-02; DES-02 | deels | Legacygevallen niet als verplichte testdekking benoemd. |
| 74 | TEST, P034 | Iedere IF heeft ELSE: “Heeft elke IF een ELSE?” | IMP-02; TST-01 | deels | Structuurcontrole ontbreekt en is niet identiek aan testen van bestaande ELSE-paden. |
| 75 | TEST, P034 | Confidence level beoordelen: “Wat is je confidence level?” | TST-03 | deels | Test-confidence ontbreekt. |
| 76 | TEST, P034 | Ontwerpconformiteit controleren: “Heb ik me aan het ontwerp gehouden?” | IMP-01; TST-03 | deels | Realisatie conform ontwerp aanwezig; expliciete nacontrole in testfase niet gemapt. |
| 77 | TEST, P034 | Bijzonderheden vastleggen/beoordelen: “Zijn er bijzonderheden?1” | TST-03 | deels | Besluitmotivering wel; verplichte inventarisatie bijzonderheden ontbreekt. |
| 78 | TEST, P034 | Dekkingsgraad bepalen: “Wat was de dekkingsgraad en wat is de kans op bugs?” | IMP-02; TST-01; TST-02 | deels | Testdekkingsgraad niet verplicht geregistreerd. |
| 79 | TEST, P034 | Bugkans beoordelen: “Wat was de dekkingsgraad en wat is de kans op bugs?” | TST-03 | deels | Risico/bugkans ontbreekt als expliciet testbesluitgegeven. |
| 80 | TEST, P034 | Excel-actiepunten verwerken: “Ik heb actiepunten uit Excel bestand verwerkt en Jira geüpdated” | TST-03 (dichtstbij) | ontbreekt | Verwerking Excel-actiepunten niet als testafrondingseis opgenomen. |
| 81 | TEST, P034 | Jira actualiseren: “Ik heb actiepunten uit Excel bestand verwerkt en Jira geüpdated” | TST-03 | deels | Checklist/besluitregistratie wel, volledige update niet gemapt. |
| 82 | TEST, P034 | Contact/controlestap Jeroen: “Controle/contact moment met Jeroen” | TST-02; TST-03 | deels | Gezamenlijke test/akkoord wel; specifieke contactcontrole niet afzonderlijk traceerbaar. |
| 83 | TEST, P034 | Demonstratie geven: “Geef een demonstratie van de oplossing.” | TST-02 | deels | Acceptatietest kan demonstratie bevatten, maar verplicht demo-bewijs ontbreekt. |
| 84 | TEST, P034 | Review met Jeroen: “Review met Jeroen12” | TST-02; TST-03 | deels | Acceptatietest en akkoord wel; inhoudelijke gezamenlijke review niet expliciet gedefinieerd. |
| 85 | TEST, P034 | Commit naar Git: “Commit en push je werk op Git-archief” | AFR-01 | deels | Merge/pipeline vereisen normaal commits, maar expliciete commit-handeling ontbreekt. |
| 86 | TEST, P034 | Push naar Git-archief: “Commit en push je werk op Git-archief” | AFR-01 | deels | Merge naar doelbranch is sterker eindcriterium, maar bronhandeling push niet expliciet gemapt. |
| 87 | EVALUATIE, P055 | Resterende tijd schatten: “Hoe lang denk je nog nodig te hebben?” | AFR-02 | deels | Tijdsevaluatie aanwezig; resterende effort ontbreekt als expliciet veld. |
| 88 | EVALUATIE, P055 | Toets binnen 150%: “Past het binnen de 150% en past het binnen de volgende tijdtrigger? (van 2/3 tot 100% of van 100% tot 150%)” | AFR-02 | deels | 150%-grens ontbreekt. Basis van percentage vereist bronreview; geen eigen grensdefinitie toegevoegd. |
| 89 | EVALUATIE, P055 | Volgende tijdtrigger toetsen: “past het binnen de volgende tijdtrigger? (van 2/3 tot 100% of van 100% tot 150%)” | AFR-02 | deels | Triggers 2/3, 100%, 150% ontbreken. 16+8 werkuren betreffen akkoordwachten, niet deze efforttriggers. |
| 90 | EVALUATIE, P055 | Bij positieve toets doorgaan: “Zo ja, verlaat de evaluatie en ga door met de processtap” | AFR-02 | deels | Beslisactie doorgaan in actuele processtap niet vastgelegd. |
| 91 | EVALUATIE, P055 | Oorzaak tijdoverschrijding analyseren: “Hoe heeft het zo lang kunnen duren?” | AFR-02 | volledig | Tijdsevaluatie & overschrijdingsanalyse dekt deze vraag rechtstreeks. |
| 92 | EVALUATIE, P055 | Snellere methodieken onderzoeken: “Zijn er snellere methodieken mogelijk?” | AFR-02 | deels | Versnellingsalternatieven ontbreken als verplicht evaluatieonderdeel. |
| 93 | EVALUATIE, P055 | Contact Jeroen, die vervolg bepaalt: “Controle/contact moment met Jeroen. Hij bepaalt het vervolg” | AFR-02 | deels | Algemene contactplicht behouden, maar inspannings-evaluatiebesluit door Jeroen niet expliciet; escalatieontvanger Esther vervangt dit niet. |
| 94 | TOELICHTING, P057 | Commentaar in code schrijven: “Schrijf commentaar in de code en kort in de check-in omschrijving.” | IMP-01; IMP-03 | deels | Codecommentaar niet expliciet als realisatie/reviewcriterium. |
| 95 | TOELICHTING, P057 | Korte check-in-omschrijving schrijven: “Schrijf commentaar in de code en kort in de check-in omschrijving.” | IMP-01; AFR-01 | deels | Korte inhoudelijke commit/check-in-omschrijving ontbreekt. |
| 96 | TOELICHTING, P058 | Zelf vandaag begrijpelijk: “Begrijp ik het vandaag?” | IMP-01; IMP-03 | deels | Begrijpelijkheidstoets voor auteur ontbreekt. |
| 97 | TOELICHTING, P059 | Opdrachtgever vandaag begrijpt: “Begrijpt de opdrachtgever het vandaag?” | IMP-01; IMP-03 | deels | Begrijpelijkheid voor opdrachtgever niet expliciet. |
| 98 | TOELICHTING, P060 | Zelf na jaar begrijpelijk: “Begrijp ik het over een jaar?” | IMP-01; IMP-03 | deels | Duurzame begrijpelijkheid voor auteur niet expliciet. |
| 99 | TOELICHTING, P061 | Opdrachtgever na jaar begrijpt: “Begrijpt de opdrachtgever het over een jaar?” | IMP-01; IMP-03 | deels | Duurzame begrijpelijkheid voor opdrachtgever niet expliciet. |
| 100 | TOELICHTING, P062 | Beschrijving crosschecken: “Crosscheck de beschrijving en deel je verbetering.” | INT-01; ANA-01 | deels | Expliciete crosscheck issuebeschrijving ontbreekt. |
| 101 | TOELICHTING, P062 | Verbetering delen: “Crosscheck de beschrijving en deel je verbetering.” | INT-01 (dichtstbij) | ontbreekt | Verplicht delen van beschrijvingsverbetering niet opgenomen. |
| 102 | TOELICHTING, P062 | Jira-opmerking vanuit ontvanger toetsen: “Sta in de schoenen van de ontvanger van een opmerking in Jira.” | ANA-03; DES-03; TST-03 | deels | Commentaarsjablonen wel; ontvangergerichte leesbaarheidstoets ontbreekt. |
| 103 | TOELICHTING, P063 | Alarm bij opnieuw beginnen: “Opnieuw beginnen? Alarm!” | AFR-02 (dichtstbij) | ontbreekt | Inhoudelijk herstartalarm ontbreekt; akkoorddeadline-escalatie heeft andere trigger. |
| 104 | TOELICHTING, P064 | Root cause oplossen: “Kies een oplossing die het probleem bij de bron (‘root cause’) oplost” | ANA-01; ANA-02 | deels | Root-causecriterium ontbreekt. |
| 105 | TOELICHTING, P065 | Meerdere concepten melden: “Worden er meerdere concepten gebruikt? Zo ja, opbiechten!” | ANA-02; DES-01 | deels | Meldplicht bij conceptmix ontbreekt. |
| 106 | TOELICHTING, P066 | Afwijkingen tijdig melden: “Meld afwijkingen, hoe langer je wacht, des te groter en pijnlijker de situatie” | IMP-01; DES-03 (dichtstbij) | ontbreekt | Algemene inhoudelijke afwijkingsmeldplicht ontbreekt; wachttijd- en DoD-procesafwijkingen zijn smaller. |
| 107 | TOELICHTING, P067 | Requirements valideren: “Requirements valideren: meld afwijkingen” | ANA-01; TST-02 | deels | Expliciete requirementsvalidatie ontbreekt. |
| 108 | TOELICHTING, P067 | Requirementsafwijkingen melden: “Requirements valideren: meld afwijkingen” | ANA-03; TST-03 | deels | Besluitmotivering aanwezig, maar afwijkingsmeldplicht niet specifiek. |
| 109 | TOELICHTING, P068 | Alleen bestanden openen met Jira-nummer: “Geen bestanden openen zonder Jira nummer” | INT-01 (dichtstbij) | ontbreekt | Deze voorafgaande werkvoorwaarde is niet aanwezig. |
| 110 | TOELICHTING, P069 | Threadmodel controleren: “Draait de oplossing in één thread? Zo niet, kunnen er race condities optreden?” | DES-01; DES-02 | deels | Threadmodelcontrole niet expliciet. |
| 111 | TOELICHTING, P069 | Racecondities beoordelen bij meerdere threads: “Zo niet, kunnen er race condities optreden?” | DES-02; TST-01 | deels | Conditionele concurrency/race-analyse ontbreekt. |
| 112 | TOELICHTING, P070 | Verwachting bugs versus testduur/gewicht beoordelen: “Verwacht bugs: aantal en gewicht wordt gehalveerd, bijv. 8 bugs worden na 2 uur testen 4 zware bugs, 4 bugs worden na 1 uur testen 2 lichtere bugs” | TST-01; TST-02 | deels | Teststrategie/risicoheuristiek ontbreekt. Behandel voorbeelden niet zonder review als bewezen norm of exacte testduur. |
| 113 | TOELICHTING, P071–P072 | Mainstream gedocumenteerde oplossing toetsen voor .NET: “Maakt het gekozen concept gebruik van de mainstream oplossing zoals gedocumenteerd voor:” / “.NET,” | ANA-02; DES-01 | deels | Gedocumenteerd mainstreamgebruik voor toepasselijke .NET-oplossing ontbreekt; geen verplicht gebruik van .NET verondersteld. |
| 114 | TOELICHTING, P075 | Na reflectie verbeterpunt eigen werk opschrijven: “Verbeterpunt opschrijven in míjn werk en voeg actiepunten aan Excel bestand toe na reflectie” | AFR-02 (dichtstbij) | ontbreekt | Persoonlijke inhoudelijke reflectie/verbeterregistratie ontbreekt; tijdsevaluatie dekt niet alle reflectie. |
| 115 | TOELICHTING, P075 | Na reflectie actiepunten toevoegen aan Excel: “voeg actiepunten aan Excel bestand toe na reflectie” | AFR-02 (dichtstbij) | ontbreekt | Excel-terugkoppeling ontbreekt; bestemming conflicteert mogelijk met afspraak alle ingevulde gegevens in Jira. Alleen na review beslissen. |
| 116 | TOELICHTING, P076 | Conditie minimaal, irrelevante zaken uitsluiten: “Is mijn conditie de minimale set? Zitter er niet zaken in die er niet toe doen?” | IMP-01; IMP-03 | deels | Minimaliteits-/relevantietoets conditionele logica ontbreekt. Twee formuleringen van dezelfde toets. |
| 117 | TOELICHTING / Jira-beschrijving, P077–P078 | Omgeving vastleggen: “De beschrijving van Jira bestaat uit de volgende onderdelen:” / “Omgeving” | INT-01; INT-02 | deels | Bestaande beschrijving behouden is niet hetzelfde als verplicht omgevingsveld. |
| 118 | TOELICHTING / Jira-beschrijving, P077/P079 | Scenario vastleggen: “Scenario” | INT-01; ANA-01 | deels | Verplicht scenario ontbreekt. |
| 119 | TOELICHTING / Jira-beschrijving, P077/P080 | Bij bug afwijkend gedrag: “Wat er niet volgens verwachting gaat (bij bug)” | ANA-01 | deels | Conditioneel veld feitelijk afwijkend gedrag ontbreekt. |
| 120 | TOELICHTING / Jira-beschrijving, P077/P081 | Bij bug verwacht gedrag: “Wat ik had verwacht (bij bug)” | ANA-01 | deels | Conditioneel veld verwacht gedrag ontbreekt. |
| 121 | TOELICHTING / Jira-beschrijving, P077/P082 | Bij feature huidige implementatie: “Huidige implementatie (bij feature)” | ANA-01 | deels | Conditioneel veld huidige implementatie ontbreekt. |
| 122 | TOELICHTING / Jira-beschrijving, P077/P083 | Probleem in beschrijving: “Probleem” | ANA-01 | deels | Probleemanalyse is aanwezig, maar expliciete plaatsing in Jira-beschrijving niet verplicht. |
| 123 | TOELICHTING / Jira-beschrijving, P077/P084 | Bij bug hypothese: “Hypothese (bij bug)” | ANA-01; ANA-02 | deels | Conditioneel hypotheseveld ontbreekt. |
| 124 | TOELICHTING / Jira-beschrijving, P077/P085 | Bij feature oplossingsrichting: “Oplossingsrichting (bij feature)” | ANA-02 | deels | Oplossingsrichting is aanwezig, maar expliciete plaatsing in Jira-beschrijving niet verplicht. |

| 125 | Registratie, P033 | Iteratie vastleggen: “Iteratie:   . .” | INT-03; AFR-02 | deels | Iteratienummer niet expliciet; positie ten opzichte van fasen visueel onbevestigd. |
| 126 | Registratie analyse a, P048 | Iteratie/subiteratie vastleggen: “[iteratie].[subiteratie]”; “a  .1” | INT-03; ANA-01; AFR-02 | deels | Herhaalde analyse-iteratieregistratie ontbreekt. |
| 127 | Registratie analyse a, P048 | Starttijd per subiteratie: “[starttijd]”; “a  .1” | ANA-01; AFR-02 | deels | Aanvraagmoment is niet analysestarttijd. |
| 128 | Registratie analyse a, P048 | Schatting per subiteratie: “[schatting]”; “a  .1” | INT-03; ANA-01 | deels | Herhaalde analyseschatting ontbreekt. |
| 129 | Registratie ontwerp o, P047 | Iteratie/subiteratie vastleggen: “[iteratie].[subiteratie]”; “o  .1” | INT-03; DES-01; AFR-02 | deels | Herhaalde ontwerp-iteratieregistratie ontbreekt. |
| 130 | Registratie ontwerp o, P047 | Starttijd per subiteratie: “[starttijd]”; “o  .1” | DES-01; AFR-02 | deels | Ontwerpstarttijd per subiteratie ontbreekt. |
| 131 | Registratie ontwerp o, P047 | Schatting per subiteratie: “[schatting]”; “o  .1” | INT-03; DES-01 | deels | Herhaalde ontwerpschatting ontbreekt. |
| 132 | Registratie implementatie im, P039 | Iteratie/subiteratie vastleggen: “[iteratie].[subiteratie]”; “im  .1” | INT-03; IMP-01; AFR-02 | deels | Herhaalde implementatie-iteratieregistratie ontbreekt. |
| 133 | Registratie implementatie im, P039 | Starttijd per subiteratie: “[starttijd]”; “im  .1” | IMP-01; AFR-02 | deels | Implementatiestarttijd per subiteratie ontbreekt. |
| 134 | Registratie implementatie im, P039 | Schatting per subiteratie: “[schatting]”; “im  .1” | INT-03; IMP-01 | deels | Herhaalde implementatieschatting ontbreekt. |
| 135 | Registratie test t, P046 | Iteratie/subiteratie vastleggen: “[iteratie].[subiteratie]”; “t  .1” | INT-03; TST-01; AFR-02 | deels | Herhaalde test-iteratieregistratie ontbreekt. |
| 136 | Registratie test t, P046 | Starttijd per subiteratie: “[starttijd]”; “t  .1” | TST-01; AFR-02 | deels | Teststarttijd per subiteratie ontbreekt. |
| 137 | Registratie test t, P046 | Schatting per subiteratie: “[schatting]”; “t  .1” | INT-03; TST-01 | deels | Herhaalde testschatting ontbreekt. |
| 138 | Registratie schattingsopbouw, P051 | Analysecomponent vastleggen: “Analyse:           + ontwerp:           + implementatie:           + test:           =” | INT-03; ANA-01 | deels | Analysecomponent in effortopbouw niet expliciet; veldbetekenis visueel te bevestigen. |
| 139 | Registratie schattingsopbouw, P051 | Ontwerpcomponent vastleggen: “Analyse:           + ontwerp:           + implementatie:           + test:           =” | INT-03; DES-01 | deels | Ontwerpcomponent in effortopbouw niet expliciet. |
| 140 | Registratie schattingsopbouw, P051 | Implementatiecomponent vastleggen: “Analyse:           + ontwerp:           + implementatie:           + test:           =” | INT-03; IMP-01 | deels | Implementatiecomponent in effortopbouw niet expliciet. |
| 141 | Registratie schattingsopbouw, P051 | Testcomponent vastleggen: “Analyse:           + ontwerp:           + implementatie:           + test:           =” | INT-03; TST-01 | deels | Testcomponent in effortopbouw niet expliciet. |
| 142 | Registratie schattingsopbouw, P051 | Totaal van componenten vastleggen: “Analyse:           + ontwerp:           + implementatie:           + test:           =” | INT-03 | deels | Initiële schatting wel; expliciete componentensom ontbreekt. |
| 143 | Registratie, P038/P040/P052/P053 | Intakewaarde registreren: “Intake:” | INT-03; AFR-02 | deels | Benoemd intakeveld niet gemapt; soort waarde en herhaalde veldbetekenis vereisen visuele review. |
| 144 | Registratie, P037/P041/P054 | Analysewaarde registreren: “Analyse:” | INT-03; ANA-01; AFR-02 | deels | Benoemd analyseveld niet gemapt; soort waarde en veldbetekenis vereisen visuele review. |
| 145 | Registratie, P036/P042 | Ontwerpwaarde registreren: “Ontwerp:” | INT-03; DES-01; AFR-02 | deels | Benoemd ontwerpveld niet gemapt; soort waarde en veldbetekenis vereisen visuele review. |
| 146 | Registratie, P043 | Implementatiewaarde registreren: “Impl.:” | INT-03; IMP-01; AFR-02 | deels | Benoemd implementatieveld niet gemapt; soort waarde vereist visuele review. |
| 147 | TOELICHTING, P062 | Jira-opmerking na jaar toetsen: “Ook denk je na hoe je de opmerking over een jaar zou lezen.” | ANA-03; DES-03; TST-03 | deels | Duurzame leesbaarheidstoets ontbreekt. |
| 148 | TOELICHTING, P071/P073 | Mainstream gedocumenteerde oplossing toetsen voor Xamarin: “Maakt het gekozen concept gebruik van de mainstream oplossing zoals gedocumenteerd voor:” / “Xamarin,” | ANA-02; DES-01 | deels | Gedocumenteerd mainstreamgebruik indien Xamarin van toepassing is ontbreekt. |
| 149 | TOELICHTING, P071/P074 | Mainstream gedocumenteerde oplossing toetsen voor plugins: “Maakt het gekozen concept gebruik van de mainstream oplossing zoals gedocumenteerd voor:” / “plugins, bijvoorbeeld SkiaSharp” | ANA-02; DES-01 | deels | Gedocumenteerd mainstreamgebruik bij toepasselijke plugins ontbreekt; SkiaSharp is een bronvoorbeeld, geen verplichting. |

De aanvullende rijen 125–149 staan achter de hoofdtekst om de eerder genummerde broncriteria stabiel te houden. Per fase vormen iteratie en subiteratie samen één samengestelde identificatie; de vier lege herhalingsregels zijn geen vier verschillende eisen. De losse benoemde fasevelden worden apart van de expliciete componentensom geïnventariseerd, zonder te beweren dat hun waarde effort, verstreken tijd of confidence is: dat is juist het te reviewen layoutgat.

### Aantallen

| Broncluster | Rijen | Volledig | Deels | Ontbreekt |
|---|---:|---:|---:|---:|
| Algemeen (1–8) | 8 | 1 | 4 | 3 |
| Intake (9–14) | 6 | 1 | 4 | 1 |
| Analyse (15–38) | 24 | 4 | 19 | 1 |
| Ontwerp (39–53) | 15 | 2 | 12 | 1 |
| Implementatie (54–68) | 15 | 0 | 11 | 4 |
| Test (69–86) | 18 | 0 | 15 | 3 |
| Evaluatie (87–93) | 7 | 1 | 6 | 0 |
| Toelichting incl. Jira-beschrijving (94–124, 147–149) | 34 | 0 | 28 | 6 |
| Registratievelden (125–146) | 22 | 0 | 22 | 0 |
| **Totaal** | **149** | **9** | **121** | **19** |

Dit zijn aantallen controle-eisen, geen code-aantallen. De dekking meet expliciete inhoud in het intakepakket, niet geschiktheid van de labels als containers: veel deelgedekte eisen passen inhoudelijk onder bestaande codes zodra hun definitie na review wordt uitgewerkt.

## Noodzakelijke wijzigingen — uitsluitend ter review

1. **Maak de volledige subcriteria per bestaande code traceerbaar.** De globale verwijzing naar de Word-bron en generieke labels leveren geen aantoonbaar volledige brondekking. Koppel ieder concreet criterium aan uitvoeringsbewijs, conditionele N.v.t.-reden en toepasselijke fase. Behoud de bronplicht totdat een afwijking expliciet is goedgekeurd.
2. **Herstel intake-inhoud:** maximale vijf minuten, deliverable, vroegtijdige oplossingsrichting/confidence, contact Jeroen, registratie en verplichte nieuwe issues voor bugs/refactors/features. INT-02 heeft wel een aanvullende afhankelijkhedenfunctie, maar de bron benoemt geen algemeen afhankelijkhedenregister als zelfstandige eis.
3. **Werk analyse/ontwerp uit:** minimaal twee alternatieven, templates/referenties, confidence/KISS/root cause/mainstream, opdrachtgeverrationale, requirements/legacy/historie, pseudocodecontrole, effectiviteit en schattingsafstemming/bijstelling.
4. **Werk implementatie/test uit:** timer, functiegewijs testen en steppend debuggen, 100% gewijzigde-code-/paddekking, conventies, warnings/messages, confidence, hulpvragen, haardtypematrix, legacytests, iedere IF een ELSE, self-review, ontwerp-review, demo en review met Jeroen, dekkingsgraad/bugrisico en commit/push. IMP-03 (reviewverzoek) niet als bewijs van uitgevoerde review gebruiken; TST-01 (integratietest) niet als vervanging van alle functionele haardtests presenteren.
5. **Herzie gates alleen na akkoord:** inhoudelijk terug naar begin van analyse/ontwerp/implementatie bij ontbrekende toestemming expliciet afstemmen met wacht-/parkerenprotocol. Een implementatiegate is niet gelijk aan een reviewverzoek. Bestaande codes eerst op mogelijke toereikendheid reviewen; geen nieuwe code automatisch toevoegen. Bronoptie direct implementeren in ontwerp mag verplichte akkoorden niet stilzwijgend omzeilen.
6. **Maak EVALUATIE fase-overstijgend:** resterende effort, 150%-grens, tijdtriggers 2/3→100%→150%, oorzaken, snellere methodieken en Jeroens vervolgbesluit. AFR-02 alleen bij afronding of de 16+8-werkuren-akkoordregeling dekt dit niet. Basis van percentages en exacte triggers moeten met bronhouder worden bevestigd, mede visueel.
7. **Los Excel/Jira-keuze op:** actiepunten verwerken in iedere fase en reflectiepunten toevoegen aan Excel zijn bronplichten. Intakepakket bepaalt alle ingevulde gegevens in Jira. Laat expliciet reviewen of Excel werkbron blijft en Jira het bewijs bevat, of een goedgekeurde vervanging nodig is. Geen stille schrapping.
8. **Neem TOELICHTING en Jira-inhoud mee:** code/check-in-commentaar, begrijpelijkheid nu/na jaar, crosscheck/delen, herstartalarm, tijdige afwijkingsmeldingen, Jira-nummer vóór bestandopening, thread/race-toets, testheuristiek, reflectie, minimale condities en alle acht conditionele beschrijvingsvelden.
9. **Aanvullende intake-eisen niet als letterlijke Word-eisen aanduiden:** algemene afhankelijkheden, integratietest, acceptatietest met Rinse én Jeroen, merge naar doelbranch, geslaagde pipeline, DoD-volledigheid en definitieve acceptatiecommentaar zijn in deze XML niet allemaal expliciet aanwezig. Zij mogen aanvullend gelden, maar bewijzen geen dekking van ontbrekende bronhandelingen. Hetzelfde geldt voor pilot-/wachttijdafspraken.

## Reviewstatus

Geen nieuwe codes en geen gewijzigde procesafspraken. De voorgestelde subcriteriumkoppelingen, verschillen en mogelijke conflicten vereisen review. Visuele broncontrole blijft open. De tekstueel identificeerbare controle-eisen en benoemde registratievelden zijn opgenomen; definitieve aanvaarding als bronmapping vereist review van de gaten en de nog onbevestigde visuele veldrelaties.
