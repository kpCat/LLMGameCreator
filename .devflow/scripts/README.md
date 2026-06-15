# Devflow scripts

Entry points:

- `check-all.ps1` — preferred gate for local agents. It configures UTF-8 console output, requests English dotnet/MSBuild output, restores, builds, checks warnings against `.devflow/KNOWN_WARNINGS.json`, runs tests, and writes `.devflow/runs/<timestamp>-check-all/summary.json`.
- `build.ps1` — lightweight manual build wrapper with the same UTF-8 / English environment setup.
- `test.ps1` — lightweight manual test wrapper with the same UTF-8 / English environment setup.
- `collect-diagnostics.ps1` — creates a diagnostics ZIP from devflow state, latest check-all run and latest strict LLM evaluation artifacts when present.
- `check-devflow-state.ps1` — validates required devflow files and task graph structure.

Warning policy:

- Known baseline warnings are listed in `.devflow/KNOWN_WARNINGS.json`.
- `check-all.ps1` fails on unexpected warnings by default.
- Use `-AllowUnexpectedWarnings` only for investigation runs, not for task completion.


## v3 hotfix

Fixes `Invoke-DevflowLoggedCommand` so command output is written to UTF-8 logs without leaking every build line into the PowerShell function return value. This fixes the `BuildLogPath` conversion error in `check-all.ps1`.
