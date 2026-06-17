# _GATE_MATRIX.md — proof gates by task type

This matrix defines what evidence is required before a task can be reported as done.

## Universal gates

Every task:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Every task must also update `.devflow/CURRENT_RUN.md` and report by `.devflow/RUN_REPORT_TEMPLATE.md`.

## Task type gates

| Task type | Required proof | Forbidden shortcut |
|---|---|---|
| Docs/devflow | `check-devflow-state.ps1`, `check-all.ps1` when feasible | Production code change |
| Agent task spec | index/ledger updated, task has proof tests, no unlocked future work | Executable task without proof tests |
| Strict LLM output parser | raw corpus tests: fenced JSON, text before/after JSON, broken JSON, wrong root, id drift | Real LLM call in tests |
| Repair policy | fake client tests: invalid->valid, invalid->invalid, max attempts | Infinite repair or silent pass |
| Evaluation report | saved JSON/report import test, metric grouping, diagnostic hot spots, markdown/report generation | Provider call during import |
| Validator | pass sample, fail sample, stable diagnostic code | Weaken severity without evidence |
| Artifact schema | valid artifact fixture, invalid artifact fixture, version/contract id check | Accept unknown contract silently |
| Manifest integrity | canonical field test, missing path test, duplicate id test, capability mismatch test | Execute module while validating manifest |
| Lua sandbox | forbidden API tests, deterministic output, max declarations/time policy | Filesystem/network/process/debug/package/load APIs |
| Package assembly | fixture artifacts -> patch/package, invalid ref rejection, package validator pass | Silent schema change |
| Runtime smoke | load/start/wait/command/serialize/deserialize scenario | Runtime calls LLM/provider/UI |
| Snapshot/golden | deterministic output comparison with intentional update path | Unreviewed snapshot rewrite |

## Gate escalation

If a task touches more than one type, use the union of gates.

If the union requires more than the task file limit, stop and split.
