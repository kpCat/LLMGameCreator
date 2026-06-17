# ARCHIVE_MANIFEST.md — Agent Task Pack 005

Archive: `llmgc_agent_task_pack_005.zip`

Pack id: `agent-task-pack-005-m4-1-completion-and-roadmap-policy`

Generated purpose:

```text
Add M4.1 completion/gate-review task specs plus permanent roadmap and pack-generation policy docs so future packs are generated from repository state instead of chat memory.
```

## Scope

Docs-only.

No changes to:

```text
src/**
tests/**
*.sln
*.csproj
.devflow/scripts/**
GamePackage schema
runtime behavior
Lua execution/provider integrations
WinForms UI
```

## Files

Added:

```text
docs/agent-tasks/003_DEVELOPMENT_ROADMAP.md
docs/agent-tasks/004_PACK_GENERATION_POLICY.md

docs/agent-tasks/M4_1/M4_1_013_STRICT_EVALUATION_RUNBOOK_FOR_USER.md
docs/agent-tasks/M4_1/M4_1_014_REAL_EVALUATION_EVIDENCE_MANIFEST.md
docs/agent-tasks/M4_1/M4_1_015_REAL_REPORT_IMPORT_FIXTURE_GUARD.md
docs/agent-tasks/M4_1/M4_1_016_M4_GATE_CLOSURE_DECISION.md
docs/agent-tasks/M4_1/M4_1_017_M4_1_COMPLETION_CHECKLIST.md
```

Updated:

```text
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/M4_1/000_M4_1_SEQUENCE.md
```

Root helper docs:

```text
README_APPLY_AGENT_TASK_PACK_005.md
ARCHIVE_MANIFEST.md
```

## Apply checks

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```
