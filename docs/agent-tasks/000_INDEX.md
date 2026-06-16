# 000_INDEX.md — executable agent task specs

Purpose: provide small, test-first, execution-ready technical task specifications for local agents.

This folder is not a general roadmap. It is the layer between broad phase plans and code execution.

```text
.devflow/NEXT_TASK.md
  -> docs/agent-tasks/000_INDEX.md
  -> exactly one task spec file
  -> source docs named by that task spec
  -> target files / local analogs named by that task spec
  -> proof tests and system gates
  -> .devflow/NEXT_TASK.md update
```

## Rules for agents

1. Do not read all task specs.
2. Read this index, then exactly one task spec referenced by `.devflow/NEXT_TASK.md`.
3. If a task spec is locked, blocked, missing proof tests, or requires approval not granted by the user, stop and write `.devflow/BLOCKERS.md`.
4. Do not implement directly from a phase plan when an agent task spec exists.
5. Do not broaden a task spec. If the implementation needs files/classes/methods not allowed by the spec, stop.
6. A task is not done until its proof tests and system gates pass or are explicitly blocked with reason.
7. If a task spec conflicts with current source code, stop and report the conflict instead of inventing a new architecture.

## Pack state

Current task pack ledger:

```text
docs/agent-tasks/001_TASK_PACK_LEDGER.md
```

Request for the next generated pack:

```text
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
```

## Shared templates and gates

| File | Purpose |
|---|---|
| `_TASK_TEMPLATE.md` | Canonical task spec template. |
| `_TASK_READINESS_CHECKLIST.md` | Checks before a task is allowed for autonomous execution. |
| `_GATE_MATRIX.md` | Required gates by task type. |
| `_SYSTEM_GATES.md` | Build/test/runtime/docs gate definitions and command expectations. |

## Current executable area

The repository is currently in the M4.1 real-model evaluation gate. M5/M6/M8 production work remains locked until current-state docs explicitly unlock it.

## Task specs

### M4.1 — real evaluation gate and strict generation hardening

Recommended sequence file:

```text
M4_1/000_M4_1_SEQUENCE.md
```

| Task | Status | Spec |
|---|---|---|
| M4_1_001 | Ready when a real evaluation report exists | `M4_1/M4_1_001_REAL_EVALUATION_REPORT_IMPORT.md` |
| M4_1_002 | Proposed, user approval recommended | `M4_1/M4_1_002_STRICT_OUTPUT_CORPUS_FIXTURES.md` |
| M4_1_003 | Ready only after diagnostic hot spots exist | `M4_1/M4_1_003_REPAIR_POLICY_HARDENING.md` |
| M4_1_004 | Ready with approval | `M4_1/M4_1_004_STRICT_JSON_PARSER_CORPUS_GUARD.md` |
| M4_1_005 | Ready after M4_1_004 or real report | `M4_1/M4_1_005_EVALUATION_MARKDOWN_GOLDEN_RECOMMENDATIONS.md` |
| M4_1_006 | Ready after diagnostic hot spots or parser corpus | `M4_1/M4_1_006_STRICT_REPAIR_PROMPT_GUARDRAILS.md` |
| M4_1_007 | Ready after real evaluation summary exists | `M4_1/M4_1_007_M4_GATE_DECISION_REPORT.md` |
| M4_1_008 | Ready with approval | `M4_1/M4_1_008_AGENT_TASK_DOCS_CONSISTENCY_GUARD.md` |

### M5 — Lua module executor integration

Locked until M4.1 gate review passes.

| Task | Status | Spec |
|---|---|---|
| M5_001 | Locked by M4.1 gate | `M5/M5_001_LUA_EXECUTOR_CONTRACTS.md` |
| M5_002 | Locked by M4.1 gate | `M5/M5_002_LUA_MANIFEST_VALIDATION.md` |
| M5_003 | Locked by M4.1 gate | `M5/M5_003_LUA_STATIC_SANDBOX_POLICY.md` |

### M6 — rich GamePackage assembly

Locked until M4.1 gate review passes and selected artifact families are stable.

| Task | Status | Spec |
|---|---|---|
| M6_001 | Locked by M4.1 gate | `M6/M6_001_ARTIFACT_TO_PACKAGE_MAPPING_CONTRACTS.md` |

## NEXT_TASK pointer shape for agent task specs

Use this shape in `.devflow/NEXT_TASK.md` when a task spec should be executed:

```text
# NEXT_TASK

Mode: single-task
Task source: agent_task_spec
Task id: M4_1_004
Task spec file: docs/agent-tasks/M4_1/M4_1_004_STRICT_JSON_PARSER_CORPUS_GUARD.md
Reason: Add fixture-driven proof coverage for strict JSON parser behavior before further prompt/repair changes.
User approval: approved | missing | required
Expected stop after completion: yes
```
