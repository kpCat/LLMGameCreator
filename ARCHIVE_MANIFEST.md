# Archive manifest — agent task pack 001

Archive: `llmgc_agent_task_pack_001.zip`

Purpose: add the first executable agent-task specification layer above `.devflow` without changing production code.

This pack is docs/devflow-only. It does not modify `src/`, `tests/`, `.sln`, `.csproj`, scripts, package schema, runtime behavior, Lua execution, or provider integration.

## Added

```text
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/_TASK_TEMPLATE.md
docs/agent-tasks/_TASK_READINESS_CHECKLIST.md
docs/agent-tasks/_GATE_MATRIX.md
docs/agent-tasks/_SYSTEM_GATES.md

docs/agent-tasks/M4_1/M4_1_001_REAL_EVALUATION_REPORT_IMPORT.md
docs/agent-tasks/M4_1/M4_1_002_STRICT_OUTPUT_CORPUS_FIXTURES.md
docs/agent-tasks/M4_1/M4_1_003_REPAIR_POLICY_HARDENING.md

docs/agent-tasks/M5/M5_001_LUA_EXECUTOR_CONTRACTS.md
docs/agent-tasks/M5/M5_002_LUA_MANIFEST_VALIDATION.md
docs/agent-tasks/M5/M5_003_LUA_STATIC_SANDBOX_POLICY.md

docs/agent-tasks/M6/M6_001_ARTIFACT_TO_PACKAGE_MAPPING_CONTRACTS.md
```

## Replaced / updated

```text
.devflow/RECURSIVE_TASK_SELECTION_PROTOCOL.md
.devflow/CONTEXT_BUDGET_POLICY.md
.devflow/DEFINITION_OF_DONE.md
.devflow/PHASE_PLAN_INDEX.md
.devflow/prompts/local_agent_start_prompt.md
```

## Apply

Extract this archive into repository root with overwrite enabled.

Recommended checks after apply:

```powershell
cd C:\Users\endim\LLMGameCreator
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Safety

M5/M6 task specs are planning contracts only until the active M4.1 gate is explicitly reviewed and current state docs are updated. Their existence is not approval to execute M5/M6 production work.
