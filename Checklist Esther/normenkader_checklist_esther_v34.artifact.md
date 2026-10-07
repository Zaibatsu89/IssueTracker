# Normenkader — Checklist Esther v34 RC

**Concept — niet vrijgegeven.** Versie: concept v34 RC, gebaseerd op de 149 rijen van de brondekkingsmatrix. Dit bestand is uitsluitend een voorstel voor normatieve uitwerking; geen akkoord, pilotstart, proceswijziging of bewijs van uitvoering. De bronmatrix blijft intact. Er zijn geen bronbestanden, andere artifacts, planningstate of lifecycle gewijzigd.

## Herkomst en begrenzing

Volledig gelezen: [brondekking.artifact.md](brondekking.artifact.md), [intakepakket.artifact.md](intakepakket.artifact.md) en [implementation_plan.artifact.md](implementation_plan.artifact.md). De matrix verwijst naar `C:/Zaibatsu89/IssueTracker/Checklist Esther/Checklist Esther versie 34 RC oktober 2025.docx`. De oorspronkelijke extractie rapporteert 85 niet-lege paragrafen, 63 gelijke Choice/Fallback-paren en 149 afzonderlijke eisen/velden; de oorspronkelijke dekking blijft 9 volledig, 121 deels, 19 ontbrekend. De uitwerking hieronder verandert dat historische oordeel niet.

`Pnnn` is een XML-paragraafreferentie uit de matrix, geen pagina- of checklistnummer. De cijfers in de eerste tabelkolom zijn **bronmatrixrijnummers**, geen Jira-puntcodes. Er is geen nieuwe extractie of visuele Word-controle uitgevoerd. Tekstvakposities, pijlen, kleuren, vinkvakrelaties, veldbetekenissen en tijdtrigger-layout zijn niet bewezen. De voorstellen mogen niet als gereconstrueerde bron-layout worden gelezen. Ook zijn geen live Jira-controles, code-inspecties, builds of softwaretests uitgevoerd.

## Gebruik, bewijs en acceptatie

- Compacte set: **17 bestaande referentiepoorten + 2 voorgestelde poorten = 19**. Alleen EFF-01 en REF-01 zijn nieuwe codevoorstellen; niet ingevoerd of geaccordeerd. Verbreding van bestaande labels tot onderstaande subcriteria is eveneens ter review. In Jira komt na vrijgave alleen de compacte poortenset met versie/link, status en bewijs; niet deze 149-rijentabel in Description.
- Dit document is versiegebonden referentiedocumentatie, geen tweede ingevulde checklist. Het bevat uitsluitend **vereist** bewijs, geen daadwerkelijk verkregen bewijs. Per ticket worden bewijslinks en subcriteriumresultaten herleidbaar in de ene checklistsectie en de gekoppelde commentaren vastgelegd. Geen losse lokale checklist of onafhankelijke urenteller.
- Bewijs heeft ticket-link, versie/commit of testrun, auteur/beoordelaar, datum/tijd en uitkomst. Codebewijs linkt naar concrete regels/diff; testrapport naar scope, omgeving, run en resultaat; commentaar naar een stabiele commentaarlink; actiepunten naar subtaken/gekoppelde issues; worklog naar concrete sessies. Een tekst 'gereed' of alleen een groene pipeline bewijst niet alle subcriteria.
- **Poortacceptatie:** uitsluitend Afgerond als ieder toepasselijk subcriterium afzonderlijk bewezen is volgens de rijregel én ieder ander subcriterium N.v.t. met concrete, toetsbare reden is. Een gedeeltelijk toepasselijke poort is niet geheel N.v.t. Onopgeloste besluiten/visuele interpretaties blijven open en blokkeren acceptatie van de betrokken criteria; niet verhullen als N.v.t. Voortijdige uitval is geen kunstmatig voltooide checklist.
- In de kolom Toep.: **T** = iedere pilot-hoofdtaak binnen scope; **B** = bug; **F** = feature; **C** = expliciet genoemde conditie. T omvat uitvoering in de genoemde fase en herhaling waar relevant. Ook bij C moet afwezigheid van de conditie onderbouwd worden. Ontbrekend bewijs is geen reden voor N.v.t.; verplichte akkoorden mogen niet eenzijdig N.v.t. worden.
- **X** betekent exact: **Besluit vereist — voorgestelde vervanging door Jira**. Het geldt voor 26, 27, 47, 48, 65, 66, 80, 81, 114 en 115, omdat ook de Jira-update uit dezelfde Excel-verwerkingsstap afhankelijk is van het besluit. Bronplicht blijft zichtbaar. Voorstel A: actiepunten als Jira-subtaak/gekoppeld issue met bronherkomst, eigenaar, verwerking en uitkomst; reflectie in afsluitcommentaar. Voorstel B bij afwijzing van A vereist apart besluit over centraal Excel-bestand, eigenaar, rechten, gezag en verwerking naar Jira. Geen van beide is geaccordeerd. X is geen geldende vervangingsnorm en geen N.v.t.; acceptatie vergt formele dispositie door Jeroen én Esther en daarna bewijs volgens die dispositie.
- **V** = visuele/semantische bronreview vereist; registreer locatie, interpretatie, reviewer en besluit, zonder nu veldbetekenis of positie te verzinnen. Bij V is tekstuele voorbereiding mogelijk maar definitieve acceptatie wacht op die review. Waar bronregels botsen met werkwijze of techniek, blijft de bronregel behouden tot formele dispositie (regel, reden, vervanging, beoordelaars, datum, versie en besluitlink). Geen stille correctie van 'elke IF heeft ELSE', de 100%-dekkingseis, timebox, terugkeerregels of triggerpercentages.
- Akkoorden: aanvraag door Rinse/aangewezen waarnemer is geen akkoord; Jeroen of vooraf bevoegde vervanger geeft expliciet besluitcommentaar. Gezamenlijke uitvoering blijft beperkt tot analyse, ontwerp en test; andere contactplichten blijven. Zonder verplicht akkoord geen volgende inhoudelijke fase. Bestaande wachtregistratie: Flagged, 16 werkuren tot escalatie naar Esther, 8 extra tot parkeren, herbeoordeling maandag 10:00; ma–do 09:00–17:00 Europe/Amsterdam, feestdagen uitgesloten. Esther is niet automatisch accordeerder. Statusbehoud bij ontbrekende transitie vervangt geen inhoudelijke terugkeer/herziening. Beschikbaarheid van transities/rechten/bordvlag is niet live geverifieerd.

## Primaire subcriteria per poort

Elke onderstaande numerieke rij is precies één primaire bestemming. Eventuele gebruiksrelaties met andere poorten leveren geen tweede primaire toewijzing op. Alle acceptatieregels gelden naast de algemene poortacceptatie hierboven.

### INT-01 — Scopebepaling (11 rijen)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 2 | P009 | Registreer een herkenbare taaktitel. | Ticket-link naar titel. | T | Titel identificeert opdracht/scope. |
| 6 | P031 | Maak voor iedere aangetroffen nieuwe bug een afzonderlijk issue. | Nieuw issue-link, vindcontext, relatie hoofdtaak. | C: bug gevonden | Alle vondsten hebben eigen issue; geen stille scope-inlijving. |
| 7 | P031 | Maak voor iedere aangetroffen refactor een afzonderlijk issue. | Refactor-issue-link en aanleiding. | C: refactor gevonden | Iedere refactor traceerbaar afzonderlijk ingediend. |
| 8 | P031 | Dien iedere nieuwe feature via een afzonderlijk issue in. | Feature-issue-link en hoofdtaakrelatie. | C: feature indienen | Alle nieuwe features afzonderlijk vastgelegd. |
| 11 | P032/P051 | Specificeer de concrete deliverable tijdens intake. | Ticketbeschrijving: resultaat en grenzen. | T | Opleverbaar resultaat en scope toetsbaar vóór vervolg. |
| 13 | P032 | Voer intakecontact/controlestap met Jeroen uit. | Contactcommentaar met datum, deelnemers, uitkomst. | T | Contact werkelijk uitgevoerd; gezamenlijk werk later is geen vervanging. |
| 100 | P062 | Crosscheck de issuebeschrijving op juistheid en volledigheid. | Reviewcommentaar en beschrijvingsdiff. | T | Crosscheck uitgevoerd; tekortkomingen zichtbaar verwerkt/open. |
| 101 | P062 | Deel de verbetering van de beschrijving met betrokkenen. | Commentaarlink met verbetering en ontvangers. | C: verbetering | Gedeeld en vindbaar; alleen lokaal wijzigen voldoet niet. |
| 109 | P068 | Open geen werkbestanden zonder Jira-nummer. | Ticketnummer en start-/werkcommentaar met bestandscontext. | T | Nummer beschikbaar vóór bestandopening; overtreding als afwijking, niet achteraf wegpoetsen. |
| 117 | P077/P078 | Leg de relevante omgeving vast in Jira-beschrijving. | Ticket-link naar omgeving/versies/configuratie. | T | Omgeving voldoende bepaald voor uitvoering en tests. |
| 118 | P077/P079 | Leg het scenario vast in Jira-beschrijving. | Ticket-link naar stappen/condities. | T | Scenario navolgbaar, niet alleen titel. |

### INT-02 — Afhankelijkheden (1 rij)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 33 | P034 | Onderzoek bijzondere afhandelingen voor oudere systemen. | Analysecommentaar met systeem-/versieonderzoek en issue-/codelinks. | T: onderzoek; C: legacy aanwezig | Onderzoek vastgelegd; gevonden uitzonderingen expliciet meegenomen. |

Aanvullend uit intake, geen extra matrixrij: registreer algemene afhankelijkheden, randvoorwaarden en eigenaars met ticketlinks en verificatie. Afwezigheid onderbouwen; dit aanvullende register vervangt het legacyonderzoek niet.

### INT-03 — Effortschatting (14 rijen)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 1 | P011 | Bepaal vóór werkbegin een ruwe effortschatting. | Gedateerd intakecommentaar met eenheid. | T | Schatting aantoonbaar vóór begin; oorspronkelijke waarde behouden. |
| 9 | P032 | Beperk intake tot maximaal vijf minuten. | Intake start/einde en duurcommentaar. | T | Duur ≤5 minuten; conflict/overschrijding vraagt dispositie, geen gelijkstelling aan responstijd. |
| 12 | P032 | Maak een initiële schatting bij intake. | Ticketcommentaar met waarde, eenheid en aannames. | T | Initiële schatting beschikbaar vóór fasevervolg. |
| 36 | P034 | Stem verwachte effort af tijdens analyse. | Afstemmingscommentaar met Jeroen/beoordelaar en waarde. | T | Afstemming bewezen, niet alleen eigen schatting. |
| 43 | P034 | Beoordeel in ontwerp of verwachte effort bijstelling nodig heeft. | Commentaar: oude/nieuwe prognose of reden behoud. | T | Beoordeling zichtbaar; oorspronkelijke referentie niet overschreven. |
| 128 | P048 | Registreer analyseschatting per subiteratie. | Commentaar met a-identificatie, waarde en eenheid. | T, iedere analyse-subiteratie | Schatting per subiteratie herleidbaar. |
| 131 | P047 | Registreer ontwerpschatting per subiteratie. | Commentaar met o-identificatie, waarde en eenheid. | T, iedere ontwerp-subiteratie | Iedere subiteratie heeft eigen schatting. |
| 134 | P039 | Registreer implementatieschatting per subiteratie. | Commentaar met im-identificatie, waarde en eenheid. | T, iedere implementatie-subiteratie | Iedere subiteratie heeft eigen schatting. |
| 137 | P046 | Registreer testschatting per subiteratie. | Commentaar met t-identificatie, waarde en eenheid. | T, iedere test-subiteratie | Iedere subiteratie heeft eigen schatting. |
| 138 | P051 | Leg analysecomponent van schattingsopbouw vast. | Ticketcommentaar met componentwaarde; V-reviewlink. | T; V | Waarde en gereviewde veldbetekenis aanwezig. |
| 139 | P051 | Leg ontwerpcomponent van schattingsopbouw vast. | Componentwaarde en V-reviewlink. | T; V | Ontwerpcomponent afzonderlijk herleidbaar. |
| 140 | P051 | Leg implementatiecomponent van schattingsopbouw vast. | Componentwaarde en V-reviewlink. | T; V | Implementatiecomponent afzonderlijk herleidbaar. |
| 141 | P051 | Leg testcomponent van schattingsopbouw vast. | Componentwaarde en V-reviewlink. | T; V | Testcomponent afzonderlijk herleidbaar. |
| 142 | P051 | Leg totaal van analyse + ontwerp + implementatie + test vast. | Componentensom en V-reviewlink. | T; V | Totaal sluit rekenkundig aan bij componenten en gereviewde betekenis. |

### ANA-01 — Probleemanalyse (17 rijen)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 15 | P034 | Bepaal aanleiding van de opdracht. | Analysecommentaar/description met aanleiding. | T | Aanleiding concreet vastgelegd. |
| 16 | P034 | Bepaal het probleem. | Probleembeschrijving en feitenlinks. | T | Probleem onderscheiden van voorgestelde oplossing. |
| 17 | P034 | Stel vast of het issue een bug is. | Classificatiecommentaar met reden. | T | Bug/niet-bug expliciet en onderbouwd. |
| 18 | P034 | Reproduceer de bug(s). | Reproductierapport: stappen, omgeving, feitelijk resultaat. | B | Iedere gemelde bug reproduceerbaar bewezen; niet reproduceerbaar blijft open met onderzoek. |
| 26 | P034 | Verwerk Excel-actiepunten bij analyse. | X: bronactiepunt → Jira-issue/commentaar, eigenaar, verwerkingsbewijs. | T; X | Besluit vereist — voorgestelde vervanging door Jira; pas na dispositie alle analyseactiepunten bewezen verwerkt. |
| 27 | P034 | Actualiseer Jira na analyseactiepunten. | X: ticketdiff/commentaar met verwerkte punten en resterende acties. | T; X | Besluit vereist — voorgestelde vervanging door Jira; update compleet en met actiepunten reconcilieerbaar na besluit. |
| 29 | P034 | Verklaar waarom opdrachtgever deze keuze heeft gemaakt. | Commentaar met opdrachtgevermotivering/broncontact. | T | Motivering expliciet, niet eigen aanname over opdrachtgever. |
| 30 | P034 | Verklaar waarom de wijziging nodig is. | Noodzaak/rationale in analyse. | T | Behoefte en gevolg van niet uitvoeren duidelijk. |
| 31 | P034 | Beschrijf hoe het werkt. | Werkingsbeschrijving met code-/ontwerpreferentie. | T | Werking navolgbaar voor beoordelaar. |
| 32 | P034 | Schrijf requirements op. | Genummerde requirements in ticket/link. | T | Requirements toetsbaar vastgelegd, niet uitsluitend mondeling. |
| 34 | P034 | Onderzoek of implementatie altijd zo is geweest. | Historische commit-/ticketlinks en conclusie. | T | Historie/regressie onderzocht en conclusie onderbouwd. |
| 107 | P067 | Valideer requirements. | Validatiecommentaar/testrapport per requirement. | T | Juistheid/haalbaarheid en afwijkingen expliciet beoordeeld. |
| 119 | P077/P080 | Beschrijf bij bug wat niet volgens verwachting gaat. | Jira-beschrijving met feitelijk afwijkend gedrag. | B | Afwijkend gedrag concreet beschreven. |
| 120 | P077/P081 | Beschrijf bij bug het verwachte gedrag. | Jira-beschrijving met verwachting en referentie. | B | Verwachting onderscheidbaar van feitelijk gedrag. |
| 121 | P077/P082 | Beschrijf bij feature de huidige implementatie. | Jira-beschrijving met huidige werking/codelink. | F | Huidige situatie navolgbaar vastgelegd. |
| 122 | P077/P083 | Neem het probleem op in Jira-beschrijving. | Ticket-link naar probleemsectie. | T | Probleem staat expliciet in description, niet alleen elders in analyse. |
| 123 | P077/P084 | Neem bij bug een hypothese op in Jira-beschrijving. | Hypothese met feiten en toetsplan. | B | Hypothese expliciet; niet als bewezen oorzaak gepresenteerd. |

### ANA-02 — Oplossingsrichting (15 rijen)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 10 | P032 | Leg oplossingsrichting reeds tijdens intake vast. | Gedateerd intakecommentaar. | T | Richting vóór analysevervolg beschikbaar. |
| 14 | P032/P051 | Registreer confidence-percentage voor oplossingsrichting. | Percentage met onderbouwing en V-reviewlink. | T; V: ruimtelijke relatie | Waarde expliciet; veldrelatie pas na bronreview definitief. |
| 19 | P034 | Bedenk hoe het probleem opgelost kan worden. | Oplossingsbeschrijving in analyse. | T | Werkbare richting met probleemrelatie beschreven. |
| 20 | P034 | Bedenk minimaal twee oplossingen. | Alternatievenlijst met minstens twee onderscheiden opties. | T | ≥2 inhoudelijk uitgewerkte alternatieven. |
| 21 | P034 | Gebruik bestaande oplossingen als template voor ontwerp. | Bestaande oplossing-/template-link en toepassing. | T | Hergebruik en relevante overeenkomsten aantoonbaar. |
| 22 | P034 | Kies de oplossing met hoogste confidence. | Vergelijking confidence per optie en keuzecommentaar. | T | Selectie volgt onderbouwde confidence-afweging. |
| 23 | P034 | Toets keuze aan eenvoud/KISS. | KISS-afweging met complexiteitsargumenten. | T | Eenvoud expliciet getoetst. |
| 24 | P034 | Beproef alternatieven kritisch ('schop om'). | Tegenargumenten, experiment-/testlinks en afweging. | T | Opties kritisch onderzocht; niet alleen opgesomd. |
| 25 | P034 | Benoem expliciete referentie voor gekozen oplossing. | Concrete documentatie-/template-/codelink. | T | Referentie relevant en vindbaar, niet alleen algemene bewijslink. |
| 35 | P034 | Hercontroleer of dit de meest eenvoudige oplossing is. | Afzonderlijke hercontrolecommentaar. | T | Hernieuwde eenvoudtoets zichtbaar naast eerste keuze. |
| 104 | P064 | Kies oplossing die root cause aanpakt. | Oorzaakanalyse en oplossing→oorzaak-relatie. | T | Bronoorzaak aangepakt, symptoombestrijding niet stilzwijgend geaccepteerd. |
| 113 | P071/P072 | Toets concept aan gedocumenteerde mainstream .NET-oplossing. | .NET-documentatielink en vergelijking. | C: .NET van toepassing | Mainstreamconformiteit bewezen; afwijking vraagt formele dispositie. |
| 124 | P077/P085 | Neem bij feature oplossingsrichting op in Jira-beschrijving. | Ticket-link naar feature-oplossingsrichting. | F | Richting expliciet in description aanwezig. |
| 148 | P071/P073 | Toets concept aan gedocumenteerde mainstream Xamarin-oplossing. | Xamarin-documentatielink en vergelijking. | C: Xamarin van toepassing | Conformiteit bewezen of formele dispositie; geen verplicht technologiegebruik afgeleid. |
| 149 | P071/P074 | Toets plugins aan gedocumenteerde mainstreamoplossing. | Plugin-documentatie en conceptvergelijking. | C: plugins gebruikt | Toets per relevante plugin; SkiaSharp is voorbeeld, niet verplicht. |

### ANA-03 — Akkoord analyse (6 rijen)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 28 | P034 | Voer analysecontact/controlestap met Jeroen uit. | Contactcommentaar: deelnemers, onderwerpen, uitkomst. | T | Contact bewezen; alleen aanvraag is onvoldoende. |
| 37 | P034 | Verkrijg expliciete toestemming voor volgende inhoudelijke fase. | Aanvraaglink en besluitcommentaar bevoegde toetser. | T | Expliciet akkoord vooraf; geen fasepassage bij ontbreken. |
| 38 | P034 | Ga zonder expliciete toestemming inhoudelijk terug naar begin analyse. | Herzieningscommentaar en nieuwe analyse/aanvraag; dispositie bij conflict. | C: toestemming ontbreekt | Terugkeer aantoonbaar; alleen wachten/parkeren/statusbehoud voldoet niet. Conflict wacht op formeel besluit. |
| 102 | P062 | Toets Jira-opmerking vanuit positie van ontvanger. | Commentaarreview met ontvanger, bedoeling en leesbaarheid. | T, iedere relevante opmerking | Betekenis/actie voor ontvanger expliciet gecontroleerd; fase-overstijgend. |
| 108 | P067 | Meld afwijkingen uit requirementsvalidatie. | Afwijkingscommentaar, ontvangers en issue-links. | C: afwijking gevonden | Alle afwijkingen gemeld vóór afhankelijk vervolg. |
| 147 | P062 | Toets hoe Jira-opmerking over een jaar gelezen zou worden. | Reviewcommentaar met context/duurzame uitleg. | T, iedere relevante opmerking | Jaarperspectief beoordeeld; geen claim van daadwerkelijk toekomstbegrip. |

### DES-01 — Technisch ontwerp (11 rijen)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 39 | P034 | Kies eerst concept; kies daarna pseudocode óf bronoptie direct implementeren. | Conceptkeuze en gekozen routecommentaar. | T | Volgorde bewezen; opties geen dubbele plicht. Direct implementeren omzeilt geen verplichte gate; conflict ter dispositie. |
| 40 | P034 | Leg ontwerpreferentie vast. | Concrete ontwerp-/template-link. | T | Referentie passend en vindbaar. |
| 41 | P034 | Controleer exact volgen van gekozen template ('op de kop af'). | Template→ontwerp/code-vergelijking met verschillen. | T | Exacte conformiteit bewezen; afwijking niet stilzwijgend toestaan, vraagt dispositie. |
| 42 | P034 | Beoordeel confidence in ontwerp. | Confidencewaarde/oordeel met onderbouwing. | T | Confidence expliciet geregistreerd. |
| 44 | P034 | Controleer juistheid van pseudocode. | Pseudocode-link en reviewbevindingen. | C: pseudocode gebruikt | Logica gecontroleerd en fouten hersteld. |
| 45 | P034 | Controleer juiste plaats van pseudocode; bij fout terug en herstel. | Plaatsingsreview, herstel-/herreviewlink. | C: pseudocode gebruikt | Juiste plaats bewezen of na terugkeer hersteld. |
| 47 | P034 | Verwerk Excel-actiepunten bij ontwerp. | X: actiepunt→Jira-link met eigenaar, verwerking, resultaat. | T; X | Besluit vereist — voorgestelde vervanging door Jira; alle ontwerpacties bewezen verwerkt na dispositie. |
| 48 | P034 | Actualiseer Jira na ontwerpactiepunten. | X: ticketdiff/commentaar en actiepuntenreconciliatie. | T; X | Besluit vereist — voorgestelde vervanging door Jira; update volledig na besluit. |
| 50 | P034 | Geef oordeel over meest effectieve oplossing. | Effectiviteitsvergelijking en ontwerpcommentaar. | T | Expliciet oordeel met argumenten, niet alleen gekozen richting. |
| 105 | P065 | Meld gebruik van meerdere concepten. | Conceptinventaris en meldcommentaar aan beoordelaar. | T: inventaris; C: meerdere concepten | Conceptmix zichtbaar gemeld, geen onbesproken combinatie. |
| 110 | P069 | Controleer of oplossing in één thread draait. | Threadmodel-/codelink en analysecommentaar. | T | Eén/meerdere threads expliciet vastgesteld; input voor race-toets. |

### DES-02 — Raakvlakken (1 rij)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 111 | P069 | Beoordeel mogelijke racecondities bij meerdere threads. | Concurrencyanalyse, gedeelde state, synchronisatie-/testlinks. | C: meerdere threads | Risico's beoordeeld en beheersing bewezen; aanwezigheid niet verdoezelen met N.v.t. |

Aanvullend uit intake: controleer technische raakvlakken en afhankelijkheden met interface-/systeemreferenties en eigenaarbevestiging. Dit is geen zelfstandig letterlijk matrixcriterium en vervangt rij 111 niet.

### DES-03 — Akkoord ontwerp (5 rijen)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 46 | P034 | Bespreek proces tot nu toe. | Procesbesprekingscommentaar met bevindingen/acties. | T | Procesinhoud besproken, niet alleen technisch akkoord. |
| 49 | P034 | Voer ontwerpcontact/controlestap met Jeroen uit. | Contactcommentaar met datum en uitkomst. | T | Contact daadwerkelijk uitgevoerd. |
| 51 | P034 | Verkrijg Jeroens akkoord om met opdracht door te gaan. | Expliciet besluitcommentaar Jeroen/bevoegde vervanger. | T | Akkoord vooraf, voorwaarden aantoonbaar voldaan. |
| 52 | P034 | Verkrijg expliciete toestemming voor volgende fase. | Besluitlink bij ontwerp-gate. | T | Volgende inhoudelijke fase uitsluitend na toestemming. |
| 53 | P034 | Ga zonder toestemming terug naar begin ontwerp. | Herzieningsbewijs, nieuwe aanvraag; conflictbesluitlink. | C: toestemming ontbreekt | Inhoudelijke terugkeer bewezen; administratief wachten is niet genoeg; dispositie bij conflict. |

### IMP-01 — Realisatie conform ontwerp (14 rijen)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 59 | P034 | Controleer tijdens bouw blijven bij eenvoudigste oplossing. | Code-/ontwerpvergelijking en KISS-commentaar. | T | Eenvoud tijdens realisatie aantoonbaar herbeoordeeld. |
| 60 | P034 | Volg de codeconventie. | Versiegebonden conventielink, lint/reviewrapport, codelinks. | T | Alle toepasselijke conventies gecontroleerd en nageleefd; open afwijking vraagt dispositie. |
| 62 | P034 | Controleer dat er geen warnings/messages zijn. | Volledige build/analyse/debug-output met berichtinventaris. | T | Afwezigheid bewezen voor relevante runs; groene pipeline alleen onvoldoende. Betekenis/scope 'messages' ter formele review, niet schrappen. |
| 63 | P034 | Beoordeel implementatie-confidence. | Confidencecommentaar met argumenten. | T | Confidence expliciet vastgelegd. |
| 64 | P034 | Vraag inhoudelijke hulp bij problemen die niet zelfstandig oplosbaar zijn. | Probleem-/hulpvraagcommentaar met ontvanger en uitkomst. | C: dergelijk probleem | Tijdige hulpvraag bewezen; akkoord-escalatie is geen vervanging. |
| 65 | P034 | Verwerk Excel-actiepunten bij implementatie. | X: actiepunt→Jira-link met verwerking en resultaat. | T; X | Besluit vereist — voorgestelde vervanging door Jira; alle implementatieacties verwerkt na dispositie. |
| 66 | P034 | Actualiseer Jira na implementatieactiepunten. | X: ticketdiff/commentaar met actiepuntenreconciliatie. | T; X | Besluit vereist — voorgestelde vervanging door Jira; update volledig na besluit. |
| 94 | P057 | Schrijf commentaar in code. | Commit-/codelinks naar relevante uitleg. | T | Codecommentaar aanwezig en inhoudelijk passend; geen vervanging door Jira-commentaar alleen. |
| 96 | P058 | Toets of auteur het vandaag begrijpt. | Self-reviewcommentaar met concrete code/uitlegreferenties. | T | Actuele begrijpelijkheid door auteur beoordeeld. |
| 97 | P059 | Toets of opdrachtgever het vandaag begrijpt. | Review-/democommentaar met opdrachtgeverperspectief. | T | Begrijpelijkheid voor opdrachtgever onderbouwd, niet aangenomen. |
| 98 | P060 | Toets of auteur het over een jaar kan begrijpen. | Duurzaamheidsreview: context, namen, commentaar/codelinks. | T | Jaarperspectief expliciet beoordeeld, geen toekomstbewijs geclaimd. |
| 99 | P061 | Toets of opdrachtgever het over een jaar kan begrijpen. | Reviewcommentaar met duurzame context/uitleg. | T | Opdrachtgeverperspectief op langere termijn beoordeeld. |
| 106 | P066 | Meld inhoudelijke afwijkingen tijdig. | Afwijkingscommentaar met ontdek-/meldmoment en ontvanger. | C: afwijking | Melding zonder uitstel aantoonbaar; vertraging als afwijking vastleggen. |
| 116 | P076 | Maak condities minimale relevante set; sluit irrelevante zaken uit. | Code-/reviewlinks met relevantie per conditie. | C: conditionele logica | Minimaliteit en relevantie expliciet getoetst; beide bronformuleringen behouden in één toets. |

Aanvullend uit intake: realisatie moet overeenkomen met geaccordeerd ontwerp; bewijs via ontwerp→diff-vergelijking. Dat aanvullende algemene criterium bewijst niet automatisch eenvoud, conventies of bronregels hierboven.

### IMP-02 — Unittests gereed (5 rijen)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 55 | P034 | Test telkens zodra een functie is afgerond. | Functie→commit→testrun-matrix met volgorde/tijd. | C: functies afgerond | Iedere functie direct bij afronding getest; alleen eindrun onvoldoende. |
| 56 | P034 | Debug steppend. | Debugrapport/commentaar met doorlopen functies, stappen en bevindingen. | T: gewijzigde uitvoerbare code | Steppende controle aantoonbaar uitgevoerd, niet vervangen door unittests. Technische onmogelijkheid vraagt dispositie. |
| 57 | P034 | Controleer dat gemaakte functie werkt. | Per functie verwacht/feitelijk resultaat en testrapport. | C: functie gemaakt/gewijzigd | Werking per functie bewezen. |
| 58 | P034 | Bereik 100% dekking van gewijzigde code. | Diff-scope en dekkingsrapport met teller/noemer/meetmethode. | C: code gewijzigd | Exact 100% binnen vastgestelde gewijzigde-code-scope; metriek ter review, niet stilzwijgend verlagen. |
| 61 | P034 | Test alle IF- en ELSE-paden. | Branch-/padinventaris en testmapping/resultaten. | C: IF/ELSE-logica | Ieder IF/ELSE-pad bewezen getest; uitzonderingen alleen na formele dispositie. |

Aanvullend: unittests gereed met actuele geslaagde runs. Dit labelbewijs alleen vervangt geen functiegewijze tests, stepping of volledige paddekking.

### IMP-03 — Code review verzoek (4 rijen)

Verbreding ter review: deze bestaande poort bevat naast reviewaanvraag een afzonderlijk verplicht implementatiebesluit en uitgevoerde self-review in testfase. Geen nieuwe implementatiecode nodig als deze definitie expliciet wordt bekrachtigd. **Verzoek ≠ uitgevoerde review ≠ akkoord.**

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 67 | P034 | Verkrijg expliciete toestemming voor volgende fase na implementatie. | Reviewaanvraag plus afzonderlijk implementatiebesluit van bevoegde toetser. | T | Akkoord vóór vervolg; aanvraag/TST-akkoord achteraf voldoet niet. Beoordelaar/gatemapping ter bekrachtiging. |
| 68 | P034 | Ga zonder toestemming terug naar begin implementatie. | Herzieningscommentaar, gewijzigde uitvoering, nieuwe aanvraag. | C: toestemming ontbreekt | Inhoudelijke terugkeer bewezen; conflict met wachten/parkeren vergt formele dispositie. |
| 71 | P034 | Review eigen code in testfase. | Uitgevoerde self-reviewrapportage met diff en bevindingen/herstel. | T | Eigen review daadwerkelijk uitgevoerd; reviewverzoek is geen bewijs. |
| 72 | P034 | Voer eigen ontwerp-/designreview in testfase uit. | Ontwerp-self-review met referentie, bevindingen en herstel. | T | Afzonderlijke eigen ontwerp-review uitgevoerd en traceerbaar. |

Aanvullend uit intake: verzoek externe codereview bevat scope, commit/diff, beoordelaar en aanvraaglink. Houd uitgevoerde reviews, bevindingen, herstel en formeel besluit afzonderlijk traceerbaar.

### TST-01 — Integratietest (8 rijen)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 69 | P034 | Test functioneel ieder beschikbaar soort haard. | Haardtypen-/beschikbaarheidsmatrix en functionele runs. | C: haardtypen van toepassing | Elk beschikbaar type getest; niet-beschikbaarheid met reden/risico en besluit, geen ongeteste claim. |
| 70 | P034 | Voer de test compleet uit. | Testplan→uitvoeringsmatrix met alle resultaten. | T | Volledige toepasselijke scope uitgevoerd; open/overgeslagen tests blokkeren. |
| 73 | P034 | Controleer bijzondere afhandelingen voor oudere systemen. | Legacytestmatrix op analyse-uitzonderingen en runs. | C: legacy-afhandelingen | Iedere gevonden legacy-uitzondering aantoonbaar gecontroleerd. |
| 74 | P034 | Controleer dat elke IF een ELSE heeft. | Structurele code-inventaris/self-review per IF. | C: IF aanwezig | **Elke IF heeft ELSE** bewezen; geen correctie naar alleen branchdekking. Afwijkende technische regel vraagt formele dispositie. |
| 78 | P034 | Bepaal de dekkingsgraad in testfase. | Testrapport met metriek, scope, teller/noemer en percentage. | T | Werkelijke dekkingsgraad geregistreerd; niet gelijkgesteld aan rij 58 zonder scopebewijs. |
| 112 | P070 | Beoordeel verwachte bugaantal/gewicht tegenover testduur volgens bronheuristiek. | Risicocommentaar met testduur, bugverwachting en verwijzing naar voorbeelden. | T | Heuristiek besproken: 8 bugs→na 2 uur 4 zware, 4→na 1 uur 2 lichtere; voorbeelden geen bewezen exacte wet/testduur. Norminterpretatie ter review. |
| 135 | P046 | Registreer testiteratie/subiteratie. | Commentaar met t-identificatie en historie. | T, iedere test-subiteratie | Iteratie en subiteratie gezamenlijk uniek traceerbaar. |
| 136 | P046 | Registreer starttijd van iedere test-subiteratie. | Startcommentaar/sessielink met t-identificatie. | T, iedere test-subiteratie | Uitvoeringsstart vastgelegd, niet alleen aanvraag-/invoertijd. |

Aanvullend uit intake: integratiegevallen op relevante raakvlakken uitvoeren en bewijzen. Integratietest vervangt niet alle functionele haardtests, legacytests of structuurcontrole.

### TST-02 — Acceptatietest Rinse & Jeroen (3 rijen)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 82 | P034 | Voer testcontact/controlestap met Jeroen uit. | Contactcommentaar met datum, deelnemers en uitkomst. | T | Afzonderlijke contactcontrole traceerbaar. |
| 83 | P034 | Demonstreer de oplossing. | Democommentaar met scenario's, versie, deelnemers en resultaat. | T | Demonstratie daadwerkelijk gegeven; alleen acceptatietestlabel onvoldoende. |
| 84 | P034 | Review de oplossing met Jeroen. | Gezamenlijk inhoudelijk reviewrapport en acties/herstel. | T | Review uitgevoerd; akkoordcommentaar zonder reviewbewijs onvoldoende. |

Aanvullend uit intake: acceptatietest samen door Rinse en Jeroen met resultaat per acceptatiecriterium. Samen testen is geen automatisch formeel akkoord.

### TST-03 — Akkoord test (6 rijen)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 75 | P034 | Beoordeel test-confidence. | Confidencecommentaar met onderbouwing en testrapportlinks. | T | Confidence expliciet vóór testbesluit. |
| 76 | P034 | Hercontroleer of ontwerp is gevolgd. | Ontwerp→realisatie-nacontrole met diff en bevindingen. | T | Expliciete controle in testfase; algemene realisatieclaim niet genoeg. |
| 77 | P034 | Inventariseer en beoordeel bijzonderheden. | Bijzonderhedencommentaar, inclusief expliciet 'geen' indien onderzocht. | T | Inventaris en impactbeoordeling zichtbaar. |
| 79 | P034 | Beoordeel kans op bugs. | Risico-/bugkanscommentaar met dekking en resterende risico's. | T | Bugkans onderbouwd beoordeeld, niet automatisch nul bij groene tests. |
| 80 | P034 | Verwerk Excel-actiepunten bij testafronding. | X: actiepunt→Jira-link met verwerking/resultaat. | T; X | Besluit vereist — voorgestelde vervanging door Jira; alle testacties verwerkt na dispositie. |
| 81 | P034 | Actualiseer Jira na testactiepunten. | X: ticketdiff/commentaar en actiepuntenreconciliatie. | T; X | Besluit vereist — voorgestelde vervanging door Jira; update compleet na besluit. |

Aanvullend uit intake: expliciet testakkoord door Jeroen/bevoegde vervanger met bewijs, voorwaarden, datum en besluitlink. Niet afleiden uit demo, testuitvoering of aanvraag.

### AFR-01 — DoD-controle (3 rijen)

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 85 | P034 | Commit het werk naar Git. | Commitlink met ticketrelatie en werkversie. | C: Git-werk | Werk aantoonbaar gecommit; mergeclaim alleen onvoldoende voor handeling. |
| 86 | P034 | Push het werk naar Git-archief. | Remote commit-/branchlink en pushbewijs/commentaar. | C: Git-werk | Relevante commits aantoonbaar in remote archief. |
| 95 | P057 | Schrijf een korte inhoudelijke check-in-omschrijving. | Commit/check-in-berichtlink. | C: check-in | Beknopte begrijpelijke omschrijving van wijziging aanwezig. |

Aanvullende DoD uit intake (geen nieuwe matrixrijen): merge naar doelbranch, geslaagde pipeline op geleverde versie, alle overige poorten/subcriteria bewezen of gemotiveerd N.v.t., expliciet acceptatiecommentaar van Jeroen. AFR-01 pas Afgerond na deze controles, zodat geen circulair 'checklist gereed' ontstaat. Audit ook Done/Resolved-tickets die deze eisen missen als procesafwijking; die zijn niet succesvol opgeleverd. Broncommit/push blijven onderscheiden van aanvullende merge/pipeline.

### AFR-02 — Tijdsevaluatie & overschrijdingsanalyse (2 rijen)

**Post-mortem/afronding**, niet de tussentijdse effortbesturing en niet de reviewresponstijdklok. Rij 91 kan oorzaakinformatie uit tussentijdse evaluatie gebruiken, maar de bronvraag blijft zelfstandig toetsbaar.

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 4 | P004 | Registreer taakstop en onderscheid oplevering/uitval. | Stopcommentaar met tijd, reden en V-reviewlink. | T; V: betekenis Stop | Stop feitelijk vastgelegd; definitieve bronveldinterpretatie na review. |
| 91 | P055 | Analyseer waarom het zo lang heeft geduurd. | Oorzaakanalysecommentaar met effort-/tijdreferenties. | C: evaluatie van langdurig werk/overschrijding | Concrete oorzaken beoordeeld; alleen totaaluren onvoldoende. |

Aanvullend: afsluitende vergelijking schatting/verbruik, tijdsevaluatie en overschrijdingsanalyse met gekoppelde beslisgeschiedenis. Dit vervangt geen tussenfase-interrupts of persoonlijke inhoudelijke reflectie.

### EFF-01 — Tussentijdse effortsturing en uitvoeringsregistratie (**nieuwe code, uitsluitend voorstel**, 21 rijen)

Motivering: timer, fase-overstijgende evaluatie, sessie-/iteratieregistratie en herstartalarm vinden tijdens uitvoering plaats. Onder post-mortem AFR-02 zouden ze te laat toetsbaar zijn; onder alleen INT-03 zouden werkelijke tijd en vervolgtoestemming ontbreken. Eén compacte extra poort bundelt deze zonder extra code per fase.

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 3 | P006 | Registreer taakstart. | Startcommentaar/sessielink met tijd; V-reviewlink. | T; V: betekenis Start | Werkelijke start herleidbaar, niet gelijk aan akkoordaanvraag. |
| 5 | P002/P005/P007/P008 | Behoud alle herhaalde Effort-velden en bepaal hun betekenis. | V-veldregister per genoemde P; schatting-/verbruiksbewijs volgens besluit. | T; V | Geen veld weggelaten; schatting versus werkelijk verbruik pas na interpretatie; Work Log is voorstel. |
| 54 | P034 | Zet timer tijdens implementatie. | Sessiestart-/timercommentaar en sessieresultaat. | T: implementatie | Timer aantoonbaar gebruikt; geen vervanging door akkoordwachttijd. |
| 87 | P055 | Schat bij evaluatie de nog benodigde tijd. | Evaluatiecommentaar met resterende effort en eenheid. | C: tussentijdse evaluatie | Actuele restschatting vóór vervolgbesluit. |
| 88 | P055 | Toets of vervolg binnen 150% past. | Evaluatieberekening met basis, prognose en V-/besluitlink. | C: evaluatie; V | 150%-toets behouden; basis/interpretatie wacht op visuele review, geen eigen grensdefinitie. |
| 89 | P055 | Toets volgende tijdtrigger: 2/3→100% of 100%→150%. | Trigger-/evaluatiecommentaar met verbruik, restant en V-review. | C: brontrigger; V | Alle genoemde grenzen behouden; betekenis en exacte momenten na review. Niet vervangen door 16+8 werkuren of alleen 100%. |
| 90 | P055 | Verlaat bij positieve toets evaluatie en ga door met actuele processtap. | Positieve toets en vervolgcommentaar, overige akkoordlinks. | C: positieve evaluatietoets | Vervolg aantoonbaar gegrond op toets, geen omzeiling andere verplichte toestemming. |
| 92 | P055 | Onderzoek snellere methodieken. | Alternatieven-/effortcommentaar met oordeel. | C: evaluatie langdurig werk | Versnellingsmogelijkheden expliciet onderzocht, ook als geen passende methode gevonden. |
| 93 | P055 | Neem contact op met Jeroen; hij bepaalt vervolg. | Contact- en expliciet vervolgbesluitcommentaar. | C: betreffende evaluatiestap | Jeroens besluit bewezen vóór afhankelijk vervolg; Esther-escalatie is geen vervanging. |
| 103 | P063 | Sla alarm wanneer opnieuw beginnen nodig is. | Herstartalarmcommentaar met ontdek-/meldtijd, reden, ontvanger. | C: herstart nodig | Alarm aantoonbaar, niet vervangen door akkoorddeadline-escalatie. |
| 125 | P033 | Registreer iteratie. | Iteratiecommentaar en V-review van fase-/veldrelatie. | T; V | Iteratienummer aanwezig; geen onbewezen fasepositie aangenomen. |
| 126 | P048 | Registreer analyse-iteratie/subiteratie (a). | a-identificatie en iteratiehistorie. | T, iedere analyse-subiteratie | Samengestelde identificatie uniek herleidbaar. |
| 127 | P048 | Registreer starttijd per analyse-subiteratie. | a-startcommentaar/sessielink. | T, iedere analyse-subiteratie | Uitvoeringsstart aanwezig, niet alleen aanvraagmoment. |
| 129 | P047 | Registreer ontwerp-iteratie/subiteratie (o). | o-identificatie en historie. | T, iedere ontwerp-subiteratie | Samengestelde identificatie uniek herleidbaar. |
| 130 | P047 | Registreer starttijd per ontwerp-subiteratie. | o-startcommentaar/sessielink. | T, iedere ontwerp-subiteratie | Uitvoeringsstart aanwezig. |
| 132 | P039 | Registreer implementatie-iteratie/subiteratie (im). | im-identificatie en historie. | T, iedere implementatie-subiteratie | Samengestelde identificatie uniek herleidbaar. |
| 133 | P039 | Registreer starttijd per implementatie-subiteratie. | im-startcommentaar/sessielink. | T, iedere implementatie-subiteratie | Uitvoeringsstart aanwezig. |
| 143 | P038/P040/P052/P053 | Behoud en registreer alle benoemde Intake-waarden. | V-veldregister per P, betekenisbesluit en waardecommentaar. | T; V | Alle herhalingen verantwoord; soort waarde niet als effort/tijd/confidence verzonnen. |
| 144 | P037/P041/P054 | Behoud en registreer alle benoemde Analyse-waarden. | V-veldregister per P, betekenisbesluit en waardecommentaar. | T; V | Alle herhalingen verantwoord volgens gereviewde betekenis. |
| 145 | P036/P042 | Behoud en registreer alle benoemde Ontwerp-waarden. | V-veldregister per P, betekenisbesluit en waardecommentaar. | T; V | Beide veldlocaties verantwoord, niet ongezien samengevoegd. |
| 146 | P043 | Behoud en registreer benoemde Impl.-waarde. | V-betekenisbesluit en waardecommentaar. | T; V | Waarde volgens gereviewde betekenis; niet gelijkgesteld aan componentensom zonder bewijs. |

**Aanvullend voorstel — Work Log en 100%-interrupt, niet geaccordeerd:**

1. Jira Log Work als enige gezaghebbende registratie van werkelijk bestede effort; oorspronkelijke intake-schatting onveranderlijk in commentaar, eenheid uren. Geen onafhankelijke cumulatieve urenteller in Description. Iedere aaneengesloten werksessie en taak-/fasewisseling loggen met uitvoerder, lokaal werkelijk startmoment, duur, fase/iteratie en activiteit. Invoermoment is niet uitvoeringsmoment. Correcties met traceerbare toelichting.
2. Tijd op hoofdtaak registreren; geen dubbelboeking op gekoppelde subtaak. Subtaken kunnen actiepuntenbewijs zijn maar geen zelfstandige pilotcohort. Passief reviewwachten niet in werkelijk uitvoeringsverbruik; wachttijd uit aanvraag-/besluitcommentaren apart. Jira-urenconversie, rechten, zichtbaarheid en controleerbare cumulatieve som vóór vrijgave live verifiëren; ontbrekende functie vraagt herziening, geen stille alternatieve teller.
3. Vóór iedere implementatiesessie cumulatief verbruik vergelijken met initiële schatting; sessie begrenzen door resterend budget. Zodra 100% werkelijk uitvoeringsverbruik bereikt is, implementatie onderbreken, loggen, afstemmen met Jeroen en expliciet vervolgakkoord afwachten. Alleen achteraf dagelijks loggen is ontoereikend. Lokale timer is hulpmiddel, geen tweede officiële registratie.
4. Besluitbewijs bevat initiële schatting, verbruik, resterende prognose, afwijkingsreden en toestemming. Referentie niet achteraf ophogen om trigger te omzeilen. Wachten op besluit via Flagged en bestaande 16+8-werkurenregeling; responstijdklok blijft apart van effortmeting.
5. Open besluiten: trigger op taak- of faseniveau, nul/ontbrekende schatting, vervolgtriggers na akkoord; betekenis/basis van 2/3 en 150% en alle brontriggerrelaties wachten op visuele bronreview. **Geen schrapping** van 2/3 of 150% door introductie van de voorgestelde 100%-interrupt. Ook rij 90 geeft geen toestemming om de nieuwe interrupt vooruitlopend op akkoord te negeren: onderlinge normverhouding vraagt bekrachtiging.

### REF-01 — Persoonlijke reflectie en verbeteracties (**nieuwe code, uitsluitend voorstel**, 2 rijen)

Motivering: persoonlijke inhoudelijke verbetering en terugkoppeling van reflectieactiepunten zijn geen uitsluitend tijdgerelateerde post-mortem. Een aparte compacte poort voorkomt dat AFR-02 dit slechts impliciet of beperkt afdekt. Niet als extra broncode gepresenteerd.

| Rij | Bron | Normatief subcriterium | Vereist bewijs | Toep. | Acceptatieregel |
|---:|---|---|---|---|---|
| 114 | P075 | Schrijf na reflectie verbeterpunt voor eigen werk op, met actiepuntterugkoppeling behouden. | X: persoonlijk reflectiecommentaar met concreet verbeterpunt en actiepuntlink. | T; X | Besluit vereist — voorgestelde vervanging door Jira; persoonlijke inhoudelijke verbetering bewezen na dispositie, niet alleen tijdsevaluatie. |
| 115 | P075 | Voeg na reflectie actiepunten aan Excel toe; Jira-bestemming is voorstel. | X: reflectie→actiepunt-link, eigenaar, vervolg; formeel bestemmingbesluit. | T; X | Besluit vereist — voorgestelde vervanging door Jira; alle reflectieacties traceerbaar geregistreerd volgens besloten bestemming. |

## Exact dekkingsoverzicht

Bereiken zijn inclusief; losse nummers en bereiken zijn uitsluitend matrixverwijzingen. Alleen de tabellen in de poortsecties hierboven vormen de primaire rijen. Dit overzicht herhaalt de mapping ter controle, niet als extra primaire bestemming.

| Poort | Status code | Exacte primaire bronmatrixrijen | Aantal |
|---|---|---|---:|
| INT-01 | Bestaand | 2, 6–8, 11, 13, 100–101, 109, 117–118 | 11 |
| INT-02 | Bestaand | 33 | 1 |
| INT-03 | Bestaand | 1, 9, 12, 36, 43, 128, 131, 134, 137–142 | 14 |
| ANA-01 | Bestaand | 15–18, 26–27, 29–32, 34, 107, 119–123 | 17 |
| ANA-02 | Bestaand | 10, 14, 19–25, 35, 104, 113, 124, 148–149 | 15 |
| ANA-03 | Bestaand | 28, 37–38, 102, 108, 147 | 6 |
| DES-01 | Bestaand | 39–42, 44–45, 47–48, 50, 105, 110 | 11 |
| DES-02 | Bestaand | 111 | 1 |
| DES-03 | Bestaand | 46, 49, 51–53 | 5 |
| IMP-01 | Bestaand | 59–60, 62–66, 94, 96–99, 106, 116 | 14 |
| IMP-02 | Bestaand | 55–58, 61 | 5 |
| IMP-03 | Bestaand | 67–68, 71–72 | 4 |
| TST-01 | Bestaand | 69–70, 73–74, 78, 112, 135–136 | 8 |
| TST-02 | Bestaand | 82–84 | 3 |
| TST-03 | Bestaand | 75–77, 79–81 | 6 |
| AFR-01 | Bestaand | 85–86, 95 | 3 |
| AFR-02 | Bestaand | 4, 91 | 2 |
| EFF-01 | Voorstel | 3, 5, 54, 87–90, 92–93, 103, 125–127, 129–130, 132–133, 143–146 | 21 |
| REF-01 | Voorstel | 114–115 | 2 |
| **Totaal** | **17 bestaand + 2 voorstel** | **1..149, ieder precies eenmaal primair** | **149** |

Geen rij geschrapt of formeel vervangen. Samengestelde norminhoud blijft binnen de betreffende rij zichtbaar (bijvoorbeeld minimale/relevante conditie; iteratie/subiteratie), zonder de oorspronkelijke matrix te hernummeren. Herhaalde fasevelden blijven per genoemde P traceerbaar; lege herhalingsregels tellen niet als extra bronvereisten. Aanvullende intake-eisen zijn afzonderlijk benoemd en verhogen de 149 niet.

## Open besluiten en vrijgavebelemmeringen

1. Bekrachtig deze volledige mapping en de verbrede bestaande poortdefinities, waaronder IMP-03 als aanvraag + zelfstandig implementatiebesluit + uitgevoerde self-reviews, en de nieuwe **voorstellen EFF-01 en REF-01**. Geen codewijziging in Jira uitgevoerd.
2. Jeroen én Esther beslissen over Excel-sanering: X op de zes expliciet gevraagde rijen én de vier afhankelijke Jira-updaterijen. Geen akkoord of N.v.t. afgeleid uit Jira-only. Bij afwijzing van voorstel A is voorstel B een aparte uitzondering met nieuwe review.
3. Bekrachtig voorgesteld Work Log-protocol en 100%-interrupt, taak-/fasenniveau, nulschatting en vervolgtriggers. Visuele review beslist over 2/3, 150%, basis en layout; geen brontrigger mag ongemerkt verdwijnen.
4. Visuele/semantische bronreview: algemene Start/Stop/Effort en herhalingen; confidence-relatie P051; iteratie-/subiteratievelden en componenten; alle Intake/Analyse/Ontwerp/Impl.-velden; tekstvakken, pijlen, nummering, vinkvakrelaties en fase-/triggerrelaties. Ook expliciete a/o/im/t-tekstidentificaties bewijzen niet alle visuele positionering.
5. Formele dispositie waar nodig: inhoudelijk terug naar begin bij ontbrekende toestemming versus wachten/herzieningsregistratie; ontwerpoptie direct implementeren versus akkoorden; maximaal vijf minuten intake versus noodzakelijke inhoud; exacte templateconformiteit; meetmethode 100% gewijzigde-code-dekking en alle paden; iedere IF heeft ELSE; warnings/messages-scope; bug/testduurheuristiek. Tot besluit blijft letterlijke broninhoud behouden, niet technisch 'verbeterd'.
6. Vóór vrijgave: publicatielocatie/toegang en normenkaderversie; bevoegdheden/overdracht; actieve Jira-transities, Flagged-bordzichtbaarheid en Work Log verifiëren; exacte evaluatietijdstippen bevestigen. Bestaande scope/meetafspraken blijven uit intake/plan afkomstig, niet nieuwe Word-eisen. Geen claim dat enige controle hier uitgevoerd is.

## Controlebewijs en grenzen van de controle

De primaire tabellen zijn machineleesbaar via regels die beginnen met `| <matrixrijnummer> |`; poortkoppen beginnen met `### <code> —`. Het dekkingsoverzicht heeft geen numerieke eerste kolom en telt daardoor niet dubbel. Controle vereist: 149 primaire rijen, 149 unieke waarden, exact de verzameling 1..149, geen dubbele/ontbrekende/buitenbereikrijen, per-poortaantallen overeenkomstig het overzicht en geen afwijkingen in de exacte mapping.

De inhoudelijke zelfcontrole bij opstellen bewaart alle 149 bestemmingen, X-disposities, zelfstandige self-review, functiegewijze tests/stepping/dekking, conventies/warnings, IF/ELSE-structuur, contact/terugkeer, persoonlijke reflectie, veldregistraties en alle efforttriggers. Dit is geen uitgevoerde ticketacceptatie of visuele/live controle.

**Uitgevoerde read-only PowerShell-controle na opslaan: PASS, exitcode 0.** Via `[System.IO.File]::ReadAllText` is uitsluitend dit concept gelezen; geen scriptbestand geschreven. Reguliere expressies `^### ([A-Z]+-\d{2}) —` en `^\|\s*(\d+)\s*\|` selecteerden de poortkoppen en primaire rijen. De numerieke verzameling is vergeleken met `1..149`; de geordende rijlijst per poort met een expliciete verwachte mapping gelijk aan het dekkingsoverzicht.

| Controle | Waargenomen resultaat |
|---|---|
| Primaire rijen / unieke rijnummers | 149 / 149 |
| Ontbrekende / dubbele / buiten 1..149 | 0 / 0 / 0 |
| Afwijkingen exacte poortmapping / onverwachte poorten | 0 / 0 |
| Poorten | 19: 17 bestaande + 2 voorstellen |
| Aantallen per poort | INT-01 11; INT-02 1; INT-03 14; ANA-01 17; ANA-02 15; ANA-03 6; DES-01 11; DES-02 1; DES-03 5; IMP-01 14; IMP-02 5; IMP-03 4; TST-01 8; TST-02 3; TST-03 6; AFR-01 3; AFR-02 2; EFF-01 21; REF-01 2 |

Deze controle bewijst rij-ID-volledigheid en exacte primaire bestemming, niet inhoudelijke accordering, bewezen uitvoering, visuele bron-layout of live Jira-functionaliteit. De toevoeging van dit controlebewijs verandert geen primaire rij of poortmapping.
