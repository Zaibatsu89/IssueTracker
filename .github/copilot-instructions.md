## Purpose

This file gives focused, actionable guidance for an AI coding agent (Copilot-style) to be immediately productive in this repository.

## Quick snapshot (what I found)

- Repository root inspected: `xs4all/jobflow`
- Detectable files in the workspace at inspection time:
  - `1 Issue Tracker intake.drawio`
  - `2 Issue Tracker tijdtriggers.drawio`
  - `3 Issue Tracker review.drawio`
- No code files, package manifests (package.json, pyproject.toml, .csproj, etc.), or README.md were discoverable during the initial scan. If code exists elsewhere, point the agent to the code subfolder.

## How to get started (concrete steps for the agent)

1. Run a manifest scan to discover the app language and build system. Look for these files (stop when you find one): `package.json`, `pyproject.toml`, `requirements.txt`, `Pipfile`, `setup.py`, `requirements.txt`, `pom.xml`, `build.gradle`, `Dockerfile`, `Makefile`, `*.sln`, `*.csproj`.

   Example PowerShell quick-check (run once at repo root):

   ```powershell
   Get-ChildItem -Path . -Recurse -File -Include package.json,pyproject.toml,requirements.txt,setup.py,pom.xml,build.gradle,Dockerfile,Makefile,*.sln,*.csproj | Select-Object -ExpandProperty FullName
   ```

2. Open the three draw.io diagram files (listed above). They appear to model an "Issue Tracker" flow. Extract component names, actors, and data-flows from shapes and map them to code paths (for example: UI -> API -> DB). If the diagrams reference files or endpoints, prioritize those.

3. If no code manifests are found, ask the repo owner these exact questions before editing:
   - Where is the application code (subfolder path)?
   - What language/runtime/build system should be used? (Node, Python, .NET, Java, etc.)
   - What are the canonical dev commands (build, test, run, lint)?

4. When editing or adding code:
   - Create a short-lived topic branch: `feature/<short-desc>` or `fix/<short-desc>`.
   - Use imperative, concise commit messages and include an issue/ticket number if provided.

## Project-specific cues and patterns

- The only concrete artifacts present are three draw.io files that document issue-tracking flows; treat them as source-of-truth for high-level architecture until code is provided.
- No explicit tests or CI configuration were found; do not assume CI conventions (GitHub Actions) unless you find `.github/workflows`.

## Integration points to look for (when code appears)

- Issue tracker APIs (look for routes or controllers named `issue`, `ticket`, `intake`, `review`).
- Time-triggered jobs (the file `tijdtriggers` suggests scheduled/background tasks) — search for cron, timers, or job schedulers.

## When adding code or docs — what to include

- If you add a service implementation, also add:
  - a short README fragment explaining the service's purpose and how it maps to the diagrams
  - one minimal smoke test that demonstrates the happy path
- If you add a new dependency, update the manifest and keep transitive additions minimal.

## Questions to surface to the human maintainer (if any of the below are unknown)

- Where is the production deployment target (host, container registry, or server)?
- Which authentication/authorization model is used (OAuth, API keys, internal)?
- Are there existing database schemas or migrations the code must follow?

---

If you'd like, I can now run the manifest scan for you and open the three drawio files to extract component names and suggest an initial skeleton (service folders, minimal manifests). Which would you prefer next?
