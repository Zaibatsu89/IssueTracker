# WorkbookTextGuard

> **Bouwpad:** `C:\Zaibatsu89\IssueTracker\Megalijst\WorkbookTextGuard.sln`

Zelfstandige C# CLI voor inventarisatie, controle, opschoning en herberekening
van OOXML-werkboeken (`.xlsx`). Gebruikt uitsluitend BCL:
`System.IO.Compression.ZipArchive` en `System.Xml.Linq.XDocument`.
Geen `DocumentFormat.OpenXml` SDK.

---

## Bouwen

```powershell
# Werkdirectory
cd C:\Zaibatsu89\IssueTracker\Megalijst

# Build (met waarschuwingen als fouten)
dotnet build WorkbookTextGuard.sln --no-incremental -warnaserror

# Tests
dotnet test WorkbookTextGuard.sln --verbosity normal -warnaserror

# Release build + tests
dotnet build WorkbookTextGuard.sln -c Release --no-incremental -warnaserror
dotnet test  WorkbookTextGuard.sln -c Release --no-build --verbosity normal -warnaserror
```

> **Let op:** Succesvolle unittests vervangen geen echte Excel COM-praktijkverificatie.
> Voor COM-verificatie zijn Windows en geïnstalleerd Excel vereist.

Op Windows wordt de Excel COM-adapter automatisch gecompileerd
(`WINDOWS` constant via MSBuild `IsOSPlatform`).

---

## Modi

### `inventory` — Inventariseer emoji en emoticons

```powershell
WorkbookTextGuard inventory `
  --input  issue-tracker-overzicht.xlsx `
  --output report.json `
  [--profile plain|markup]
```

Scant het werkboek read-only en produceert een stabiel gesorteerd JSON-rapport
met unieke sequenties, codepunten, bronlocaties en SHA-256-hashes.
Geen bestandswijzigingen.

---

### `check` — Valideer tegen policy

```powershell
WorkbookTextGuard check `
  --input   issue-tracker-overzicht.xlsx `
  --policy  policy.json `
  [--mode   preflight|release] `
  [--require-calculated] `
  [--calculation-report calc-report.json]
```

**Modi:**

| Modus | Wat wordt gescand | Vrijstelling via policy |
|-------|-------------------|------------------------|
| `preflight` (default: `release`) | Bronteksten (`<t>`, gedecodeerde stringliteralen in `<f>`) | Ja — gemapte sequenties zijn OK |
| `release` | Alles, inclusief gecachte formulewaarden (`<v>`) | Nee — iedere emoji/emoticon blokkeert |

**`--require-calculated`** (alleen `release`):
Vereist een geldig rekenrapport. Controleert:
- `report.success == true`
- `report.outputSha256 == SHA-256 van het invoerbestand`

Ontbrekend rapport, mislukte berekening of hash-mismatch → exit 2.

**Exitcodes:**

| Code | Betekenis |
|------|-----------|
| `0` | Geen overtredingen |
| `1` | Overtredingen gevonden |
| `2` | Integriteits- of configuratiefout |

---

### `clean` — Schoon werkboek produceren

```powershell
WorkbookTextGuard clean `
  --input   issue-tracker-overzicht.xlsx `
  --policy  policy.json `
  --output  cleaned.xlsx `
  [--expected-sha256 <64 hextekens>]
```

**Volgorde:**
1. Lees bronbestand exact één keer; bereken SHA-256.
2. Verifieer `--expected-sha256` (indien opgegeven) vóór enige bestandswijziging.
3. Voer preflight uit op de snapshot; onbekende sequenties blokkeren.
4. Schrijf byte-identieke backup naar `<input>.bak` vanuit de snapshot.
5. Schrijf output naar tijdelijk bestand; publiceer pas na succesvolle afronding.
6. Ruim mislukte tijdelijke output op.

**Beperkingen:**
- `--output` mag niet gelijk zijn aan `--input`.
- Bestaande backup of output wordt niet stilzwijgend overschreven.
- `--expected-sha256` moet exact 64 hextekens zijn.

---

### `recalculate` — Herbereken via Excel COM (Windows)

```powershell
WorkbookTextGuard recalculate `
  --input  cleaned.xlsx `
  --output calculated.xlsx `
  --report calc-report.json
```

Vereist: Windows + geïnstalleerd Excel. Niet beschikbaar op andere platforms.

Architectuur: supervisor-thread (timeouts, Job Object) + STA worker-thread
(alle COM-aanroepen). Containment-modi: `JOB_CONTAINED` of `SUPERVISOR_ONLY`.

Het rekenrapport bevat `inputSha256`, `outputSha256`, `excelVersion` en `success`.

---

## Aanbevolen workflow

```powershell
# 1. Inventariseer (read-only)
WorkbookTextGuard inventory --input werkboek.xlsx --output report.json

# 2. Stel policy.json op en bevestig iedere mapping

# 3. Preflight-check
WorkbookTextGuard check --input werkboek.xlsx --policy policy.json --mode preflight

# 4. Opschonen
WorkbookTextGuard clean `
  --input werkboek.xlsx --policy policy.json --output cleaned.xlsx `
  --expected-sha256 <hash uit stap 1>

# 5. Herberekenen (alleen bij formules, alleen op Windows met Excel)
WorkbookTextGuard recalculate `
  --input cleaned.xlsx --output calculated.xlsx --report calc-report.json

# 6. Release-check
WorkbookTextGuard check `
  --input calculated.xlsx --policy policy.json --mode release `
  --require-calculated --calculation-report calc-report.json
```

---

## Verificatiestatus

| Status | Betekenis |
|--------|-----------|
| `PREFLIGHT` | Preflight geslaagd; bronteksten schoon |
| `STATIC_ONLY` | Release-scan zonder rekenrapport; geen vrijgave |
| `CALCULATED_CANDIDATE` | Rekenrapport geverifieerd; klaar voor eindcheck |
| `VERIFIED` | Release-check geslaagd na herberekening |
| `BLOCKED` | Overtredingen gevonden |

---

## policy.json

```json
{
  "version": "1.0",
  "unicodeVersion": "15.1",
  "emoticonProfile": "plain",
  "emojiMappings": [
    { "sequence": "⚠️", "replacement": "Aandachtspunt", "condition": "Betekenis bevestigd" },
    { "sequence": "⏱️", "replacement": "",              "condition": "Decoratief" }
  ],
  "emoticonMappings": [
    { "sequence": ":-)", "replacement": "" }
  ],
  "decorativeSequences": []
}
```

**Regels:**
- Geen catch-all; iedere sequentie expliciet goedkeuren.
- Overlappende mappings worden afgekeurd; langste sequentie wint.
- Onbekende sequenties blokkeren `clean` en release.
- `emoticonProfile`: `plain` of `markup` (Markdown-grenzen).

---

## Exitcodes

| Code | Betekenis |
|------|-----------|
| `0` | Succes / geen overtredingen |
| `1` | Overtredingen of configuratiefout |
| `2` | Integriteits-, hash- of platformfout |