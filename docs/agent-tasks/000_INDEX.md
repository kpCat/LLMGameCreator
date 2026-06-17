# 000_INDEX.md — executable agent task specs

Purpose: provide small, test-first, execution-ready technical task specifications for local agents.

This folder is not a general roadmap. It is the layer between broad phase plans and code execution.

```text
.devflow/NEXT_TASK.md
  -> docs/agent-tasks/000_INDEX.md
  -> shared quality docs
  -> exactly one task spec file
  -> source docs named by that task spec
  -> target files / local analogs named by that task spec
  -> proof tests and system gates
  -> .devflow/NEXT_TASK.md update
```

## Rules for agents

1. Do not read all task specs.
2. Read this index, shared quality docs, then exactly one task spec referenced by `.devflow/NEXT_TASK.md`.
3. If a task spec is locked, blocked, missing proof tests, or requires approval not granted by the user, stop and write `.devflow/BLOCKERS.md`.
4. Do not implement directly from a phase plan when an agent task spec exists.
5. Do not broaden a task spec. If the implementation needs files/classes/methods not allowed by the spec, stop.
6. A task is not done until its proof tests and system gates pass or are explicitly blocked with reason.
7. If a task spec conflicts with current source code, stop and report the conflict instead of inventing a new architecture.
8. Shared quality docs are binding for every task unless the task spec explicitly overrides them with user approval.
9. Future phase sequence files are routing docs, not executable specs.
10. Locked future specs are not executable permission. They must be refreshed against current source when their gate opens.

## Shared templates, gates, and quality docs

| File | Purpose |
|---|---|
| `_TASK_TEMPLATE.md` | Canonical task spec template. |
| `_TASK_READINESS_CHECKLIST.md` | Checks before a task is allowed for autonomous execution. |
| `_GATE_MATRIX.md` | Required gates by task type. |
| `_SYSTEM_GATES.md` | Build/test/runtime/docs gate definitions and command expectations. |
| `_TEST_QUALITY_RULES.md` | Proof-test rules, exact assertions, weak-test rejection. |
| `_FIXTURE_AND_GOLDEN_RULES.md` | Fixture/golden naming, size, determinism, readability. |
| `_DIFF_HYGIENE_RULES.md` | Final changed-file cleanliness and generated artifact discipline. |
| `_AGENT_EXECUTION_QUALITY_RULES.md` | General local-agent execution quality after first real Kilo feedback. |

Agent must not read every task spec, but these shared docs are not task specs; they are common execution rules.

## Pack state

Current task pack ledger:

```text
docs/agent-tasks/001_TASK_PACK_LEDGER.md
```

Request for the next generated pack:

```text
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
```

Roadmap and pack generation policy:

```text
docs/agent-tasks/003_DEVELOPMENT_ROADMAP.md
docs/agent-tasks/004_PACK_GENERATION_POLICY.md
```

## Current executable area

The repository is currently in the M4.1 real-model evaluation gate. M5/M6/M8/M9/M10 production work remains locked until current-state docs explicitly unlock it.

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
| M4_1_009 | Proposed, script-change approval recommended | `M4_1/M4_1_009_DEVFLOW_NAMED_GATES_CHECK_ALL.md` |
| M4_1_010 | Ready when real evaluation artifacts may exist | `M4_1/M4_1_010_REAL_EVALUATION_ARTIFACT_DISCOVERY.md` |
| M4_1_011 | Ready after user reviews M4.1 result | `M4_1/M4_1_011_CURRENT_STATE_GATE_REVIEW_UPDATE.md` |
| M4_1_012 | Ready after overnight/local-agent run exists | `M4_1/M4_1_012_OVERNIGHT_RUN_REPORT_REVIEW_GATE.md` |
| M4_1_013 | Ready for manual process documentation | `M4_1/M4_1_013_STRICT_EVALUATION_RUNBOOK_FOR_USER.md` |
| M4_1_014 | Ready when evidence files exist or user wants manifest discipline | `M4_1/M4_1_014_REAL_EVALUATION_EVIDENCE_MANIFEST.md` |
| M4_1_015 | Ready when report fixture/import behavior must be guarded | `M4_1/M4_1_015_REAL_REPORT_IMPORT_FIXTURE_GUARD.md` |
| M4_1_016 | Ready after evidence review | `M4_1/M4_1_016_M4_GATE_CLOSURE_DECISION.md` |
| M4_1_017 | Ready when preparing to close M4.1 | `M4_1/M4_1_017_M4_1_COMPLETION_CHECKLIST.md` |

### M5 — Lua module executor integration

Sequence file:

```text
M5/000_M5_SEQUENCE.md
```

Locked until M4.1 gate review passes.

| Task | Status | Spec |
|---|---|---|
| M5_001 | Locked by M4.1 gate | `M5/M5_001_LUA_EXECUTOR_CONTRACTS.md` |
| M5_002 | Locked by M4.1 gate and M5_001 | `M5/M5_002_LUA_MANIFEST_VALIDATION.md` |
| M5_003 | Locked by M4.1 gate and M5_001/M5_002 | `M5/M5_003_LUA_STATIC_SANDBOX_POLICY.md` |
| M5_004 | Locked draft by M4.1 gate | `M5/M5_004_LUA_EXECUTOR_TEST_HARNESS.md` |
| M5_005 | Locked draft by M4.1 gate and M5_004 | `M5/M5_005_LUA_EXECUTION_REQUEST_RESULT_CONTRACTS.md` |
| M5_006 | Locked draft by M4.1 gate and M5_005 | `M5/M5_006_LUA_MANIFEST_BINDING_TO_REQUEST.md` |
| M5_007 | Locked draft by M4.1 gate and M5_003/M5_004 | `M5/M5_007_FORBIDDEN_API_GOLDEN_FIXTURES.md` |
| M5_008 | Locked draft by M4.1 gate and M5_004/M5_005 | `M5/M5_008_NO_GAMEPACKAGE_MUTATION_GUARD.md` |
| M5_009 | Locked draft by M4.1 gate and M5_001..M5_008 | `M5/M5_009_ONE_MODULE_FAMILY_ARTIFACT_ENVELOPE_SLICE.md` |

### M6 — rich GamePackage assembly

Sequence file:

```text
M6/000_M6_SEQUENCE.md
```

Locked until M4.1 gate review passes and selected artifact families are stable.

| Task | Status | Spec |
|---|---|---|
| M6_001 | Locked by M4.1 gate | `M6/M6_001_ARTIFACT_TO_PACKAGE_MAPPING_CONTRACTS.md` |
| M6_002..M6_008 | Skeleton only | `M6/000_M6_SEQUENCE.md` |

### M8 — runtime preview validation loop

Sequence file:

```text
M8/000_M8_SEQUENCE.md
```

Locked until package assembly path exists.

### M9 — templates and balancing

Sequence file:

```text
M9/000_M9_SEQUENCE.md
```

Locked until package generation and validation paths are stable.

### M10 — export profiles and Unity IR

Sequence file:

```text
M10/000_M10_SEQUENCE.md
```

Locked until export profile work is explicitly unlocked.

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
