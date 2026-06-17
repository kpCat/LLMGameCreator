# README_APPLY_AGENT_TASK_PACK_005.md

Pack: `agent-task-pack-005-m4-1-completion-and-roadmap-policy`

Purpose: add M4.1 completion/gate-review task specs and permanent roadmap / pack-generation policy docs.

This pack is docs-only. It does **not** change production code, tests, scripts, solution/project files, GamePackage schema, runtime behavior, Lua execution, providers, or UI.

## Apply

From repository root:

```powershell
cd C:\Users\endim\LLMGameCreator
# unzip llmgc_agent_task_pack_005.zip into this folder, replacing files

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## What this pack adds

```text
docs/agent-tasks/003_DEVELOPMENT_ROADMAP.md
docs/agent-tasks/004_PACK_GENERATION_POLICY.md

docs/agent-tasks/M4_1/M4_1_013_STRICT_EVALUATION_RUNBOOK_FOR_USER.md
docs/agent-tasks/M4_1/M4_1_014_REAL_EVALUATION_EVIDENCE_MANIFEST.md
docs/agent-tasks/M4_1/M4_1_015_REAL_REPORT_IMPORT_FIXTURE_GUARD.md
docs/agent-tasks/M4_1/M4_1_016_M4_GATE_CLOSURE_DECISION.md
docs/agent-tasks/M4_1/M4_1_017_M4_1_COMPLETION_CHECKLIST.md
```

## What this pack updates

```text
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/M4_1/000_M4_1_SEQUENCE.md
```

## Important gate rule

M5/M6/M8 production work remains locked until `docs/CURRENT_GENERATOR_STATE.md` and `docs/CURRENT_GENERATOR_STATE.json` explicitly say M4.1 passed.

This pack adds roadmap and future routing, but does not unlock those phases.

## Recommended next action after apply

If M4.1 still has no real evaluation evidence, set `.devflow/NEXT_TASK.md` to `M4_1_013` or run an already-ready deterministic M4.1 task such as `M4_1_005` / `M4_1_006`.

If real evaluation evidence exists, prefer `M4_1_014` -> `M4_1_015` -> `M4_1_016`.
