# Fase 3 tooling — operator guide

**Owner/operator: Rinse Cramer. Snapshot-only tool, not a general workbook editor. No Fase 4 release is implied.**

The compiled manifest implements exactly the approved 43 cell changes + 2 worksheet metadata changes. SST resource effects are separate: 43 appends and count 1163→1173 / uniqueCount 490→533. No count=1206 error, deduplication, row addition, formula, new part, styles change or conflict resolution. K01–K12 and all absence exclusions remain unresolved/frozen.

## Contents / reproducibility

- `rust/approved-delta.json`: exact old refs/text, ABSENT as null, styles, all 16 source hashes, relationship bindings, exclusions, metadata and source identities. JSON-equivalence to the compiled copy is required. Not a configurable allowlist. Editing it is a new approval/code-review requirement.
- `rust/src/`: Rust console/library and synthetic-only tests. zip 2.4.2 `raw_copy_file`, quick-xml 0.37.5; no workbook roundtrip.
- `rust/Cargo.lock`: generated/locked transitive dependencies; Rust 1.98.1 pinned.
- `validator/`: independent read-only OpenXmlValidator console, DocumentFormat.OpenXml 3.3.0, explicitly Office2019. Schema errors on either original or candidate fail; no baseline exemptions. This Office target is the tooling's explicit choice: operator must confirm it is the agreed target before accepting evidence; another target requires an explicit tooling revision.
- `validator-tests/`: xUnit synthetic schema-pass/schema-fail/read-only/corrupt-package/CLI tests. NuGet lockfiles and .NET SDK 10.0.401 pinned.

Use an authorized external terminal outside the IDE agent/sandbox for real candidate creation. Do not ask the IDE agent to generate a real candidate through a build or shell workaround. Never overwrite the source.

## Build and test (sequential)

Prerequisites: already installed Rust 1.98.1 MSVC and .NET SDK 10.0.401; do not install missing toolchains without permission. Set `RUSTUP_AUTO_INSTALL=0` in your terminal to avoid rustup implicit installation. Keep all lockfiles. From this folder:

```text
cd rust
cargo test --locked
cargo build --release --locked
cd ..
dotnet restore validator-tests/Validator.Tests.csproj --locked-mode
dotnet build validator/Validator.csproj -c Release --no-restore
dotnet test validator-tests/Validator.Tests.csproj --no-restore
```

Recorded final results: Rust 17 passed / 0 failed, release build exit 0; .NET 4 passed / 0 failed, release build and locked restore exit 0. Tests create only synthetic in-memory and temporary fixtures. No tests consume a real workbook.

## Read-only preflight

From `rust` (use full executable paths if not on Windows):

```text
target/release/issuetracker-phase3.exe dry-run C:/Zaibatsu89/IssueTracker approved-delta.json
```

JSON stdout; exit 0 means preflight passed, exit 1 means BLOCKED (stderr JSON). Dry-run reads and rechecks all sources, parses source entries and computes selective changes **in memory only**. It creates no candidate/temp ZIP or report file. Counts in this mode are prospective, not a measured candidate. Candidate hash is null; status is `DRY_RUN_READ_ONLY_NO_CANDIDATE`. The eight raw-entry hashes are source/in-memory projection evidence, not evidence of candidate compression preservation.

## Operator-only candidate generation

Choose an existing local directory and a **nonexistent** absolute filename, preferably including `UNVALIDATED`. Quiesce source editing, sync tools and Excel, protect source directory against external writes for the entire run. Source hashes cover saved disk bytes, not unsaved editor buffers. Example only; not executed by the IDE agent:

```text
target/release/issuetracker-phase3.exe candidate C:/Zaibatsu89/IssueTracker approved-delta.json C:/OperatorEvidence/issue-tracker.UNVALIDATED.xlsx
```

Capture stdout as the proof JSON using your approved external evidence collection route. The program itself writes only its temporary candidate and new output, never source files or a report. If proof capture fails after publication, keep candidate quarantined and do not release. The candidate is published atomically/no-clobber on the same filesystem after sync, reopening and verification. Existing output and race-created output are refused. On pre-publication failure, temporary file is removed; no partial new output. This is no-clobber atomic publication, not a claim of crash-durable directory synchronization across all filesystems.

Status after successful publication is **`UNVALIDATED_CANDIDATE_PENDING_INDEPENDENT_OPENXML_VALIDATOR`**. It is not a release marker. Proof includes original/candidate/manifest/source hashes, all 43 old/new references and new texts, eight raw compressed/uncompressed part hashes, 1227 unchanged original non-target cells and all 80 frozen cells.

Checks before publication:
- exact 16 source SHA-256 hashes before/after, original bytes unchanged;
- exact sheet names, sheet IDs, namespace-qualified rIds and relationship targets/types;
- zero cell `<f>` and no calcChain, valid coordinates/order, style indices and shared refs;
- 1260→1270 cells, 1163→1173 shared refs, 490→533 si, all original 490 si byte-identical;
- each existing target old type/style/ref/text exact, each new F/G absent; style 1 header / 3 data;
- only four targeted parts changed, no added/removed part, all other parts including styles and unknown parts raw-copy with compressed bytes, payload, compression, CRC, timestamp, Unix mode, extra field and comment compared;
- source XML byte spans outside 33 `<v>` replacements, 10 cell insertions, 43 si insertions, SST counter values and two metadata attribute values are copied verbatim; declarations, quotes, namespaces, opaque extensions and rich text remain. New plaintext si uses escaped XML and xml:space=preserve, never edits old runs.
- all 12 CF rules/formulas/dxf/priorities byte-preserved; one approved sqref A2:E110→A2:G110, dimension A1:E110→A1:G110. Snapshot has exactly one relevant CF element; another shape fails closed.

A package with new unknown protected parts cannot pass the immutable real source hash. Synthetic tests separately prove unknown streams are preserved, not targeted. This is intentionally stricter than generic future-snapshot handling.

## Independent operator schema validation

From this folder, **after** capturing successful Rust proof and confirming the explicit Office target:

```text
dotnet validator/bin/Release/net10.0/Validator.dll Office2019 C:/Zaibatsu89/IssueTracker/Megalijst/issue-tracker-overzicht.xlsx C:/OperatorEvidence/issue-tracker.UNVALIDATED.xlsx
```

Both files are opened read-only, separately hashed before/after. JSON includes all errors with part URI/path and no exemptions. Exit 0: both schema passes, **not release**. Exit 1: validation errors/open errors; exit 2: argument/unhandled operational errors. Preserve nonzero reports; baseline errors are blocking, not suppressed. The validator does not calculate formulas, mutate files or claim process consistency. A fatal file-read error can produce only a BLOCKED top-level report; fix access and rerun both inputs.

## Next gate / limitations

No real candidate has been generated here, and no schema validation of the real original/candidate has been executed. Obtain operator execution environment/date/hash evidence, Rust proof and independent original/candidate Office2019 schema reports; then perform Fase 4 semantic/package review and visual F/G usability review. Existing widths are unchanged: do not edit styles/cols to improve readability without fresh approval. No recalculation is needed for this snapshot's zero cell formulas; CF expressions stay unchanged.

Source directory must remain immutable during operator execution; repeated hashes detect changes but are not a substitute for OS-level write exclusion against malicious concurrent modification. The package is snapshot-bound, UTF-8 XML only and fails on unsupported shapes/compression/preservation differences rather than falling back to workbook reconstruction. ZIP directory offsets and whole-archive bytes may change. No source restoration, release copying or parent plan/register updates are performed by this tool. All unresolved process exclusions and K01–K12 remain outside the approved deltas.
