# README_APPLY_AGENT_TASK_PACK_007.md

Pack id: `agent-task-pack-007-locked-m5-entry-draft-specs`

Purpose:

```text
Add locked, non-executable M5 entry draft specs for Lua module executor work so the roadmap has concrete next-phase task contracts, while M4.1 remains the active gate.
```

This pack is documentation-only.

It does not unlock M5. It does not change source code. It does not change tests. It does not change `.sln`, `.csproj`, `.devflow/scripts`, Runtime, WinForms, GamePackage schema, Lua implementation, or provider wiring.

## Apply

From repository root:

```powershell
cd C:\Users\endim\LLMGameCreator
# unzip llmgc_agent_task_pack_007.zip here, replacing files

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Files added

```text
docs/agent-tasks/M5/M5_004_LUA_EXECUTOR_TEST_HARNESS.md
docs/agent-tasks/M5/M5_005_LUA_EXECUTION_REQUEST_RESULT_CONTRACTS.md
docs/agent-tasks/M5/M5_006_LUA_MANIFEST_BINDING_TO_REQUEST.md
docs/agent-tasks/M5/M5_007_FORBIDDEN_API_GOLDEN_FIXTURES.md
docs/agent-tasks/M5/M5_008_NO_GAMEPACKAGE_MUTATION_GUARD.md
docs/agent-tasks/M5/M5_009_ONE_MODULE_FAMILY_ARTIFACT_ENVELOPE_SLICE.md
```

## Files updated

```text
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/M5/000_M5_SEQUENCE.md
```

## Gate note

All new M5 task specs have:

```text
Status: locked_until_M4_1_gate_passes
Allowed before current gate review: no
```

They are intended to be refreshed from current source before execution when M4.1 is explicitly passed in `docs/CURRENT_GENERATOR_STATE.md` and `docs/CURRENT_GENERATOR_STATE.json`.

## Suggested next action

Commit this documentation pack to `main`, keep M4.1 as the active executable gate, and decide later whether the next pack should be:

```text
- M4.1 execution support / next concrete agent task;
- M5 draft refinement after source review;
- locked M6 high-level planning only;
- repair/hardening if Kilo execution exposes issues.
```
