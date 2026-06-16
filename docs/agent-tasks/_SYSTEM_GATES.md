# _SYSTEM_GATES.md — command-level gate definitions

This document describes system gates. Not every gate has a dedicated script yet. Until a script exists, the gate is satisfied by focused xUnit tests plus `check-all.ps1`.

## Existing universal gate

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Expected behavior:

```text
- restore/build/test run;
- output is English/UTF-8;
- unexpected warnings fail the gate;
- test result is saved under .devflow/runs/...;
- final line is CHECK-ALL PASSED.
```

## Existing devflow state gate

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
```

Expected behavior:

```text
- .devflow task graph/cursor docs are parseable enough for local agent use;
- known warnings config is valid;
- no missing critical devflow files.
```

## Planned named gates

These names are allowed in task specs even before separate scripts exist. If no script exists, implement focused tests and run `check-all.ps1`.

```text
docs_consistency_gate
manifest_integrity_gate
artifact_schema_gate
package_validator_gate
runtime_smoke_gate
snapshot_golden_gate
```

## Gate implementation rule

A future dedicated gate script must:

```text
- be under .devflow/scripts/;
- write logs under .devflow/runs/<timestamp>/;
- return non-zero on failure;
- not call real LLM/provider/network;
- not mutate GamePackage unless explicitly a dry-run or temp fixture;
- be callable independently and from check-all later.
```
