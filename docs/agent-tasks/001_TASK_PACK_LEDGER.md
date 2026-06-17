# 001_TASK_PACK_LEDGER.md — generated task pack ledger

This file is the state ledger for generated agent-task packs. Future packs must update it instead of relying on chat memory.

## Pack 001 — agent task specification framework

Pack id: `agent-task-pack-001`

Generated purpose:

```text
Add an executable task-spec layer for local agents so future work can be driven by small technical contracts with proof tests and system gates.
```

First task spec batch:

```text
M4_1_001_REAL_EVALUATION_REPORT_IMPORT
M4_1_002_STRICT_OUTPUT_CORPUS_FIXTURES
M4_1_003_REPAIR_POLICY_HARDENING
M5_001_LUA_EXECUTOR_CONTRACTS
M5_002_LUA_MANIFEST_VALIDATION
M5_003_LUA_STATIC_SANDBOX_POLICY
M6_001_ARTIFACT_TO_PACKAGE_MAPPING_CONTRACTS
```

## Pack 002 — M4.1 executable strict-generation specs

Pack id: `agent-task-pack-002-m4-1-executable-specs`

Generated purpose:

```text
Add additional M4.1 executable task specs based on the current strict generation/evaluation source layout, without unlocking M5/M6.
```

## Pack 003 — M4.1 gates and automation specs

Pack id: `agent-task-pack-003-m4-1-gates-and-automation`

Generated purpose:

```text
Add executable task specs for M4.1 gate automation, local-agent run review, real evaluation artifact discovery, and current-state update discipline.
```

Files added by Pack 003:

```text
docs/agent-tasks/M4_1/M4_1_009_DEVFLOW_NAMED_GATES_CHECK_ALL.md
docs/agent-tasks/M4_1/M4_1_010_REAL_EVALUATION_ARTIFACT_DISCOVERY.md
docs/agent-tasks/M4_1/M4_1_011_CURRENT_STATE_GATE_REVIEW_UPDATE.md
docs/agent-tasks/M4_1/M4_1_012_OVERNIGHT_RUN_REPORT_REVIEW_GATE.md
```

## Pack 004 — shared execution quality hardening

Pack id: `agent-task-pack-004-quality-hardening`

Generated purpose:

```text
Move repeated quality requirements into shared docs so task prompts can stay small and local agents naturally discover exact test/fixture/diff rules through the normal read chain.
```

Repository feedback used:

```text
A real Kilo execution of M4_1_004 was broadly successful, but exposed shared-rule gaps:
- weak deterministic diagnostic assertion needed tightening;
- old readable raw string style was mechanically degraded and then restored;
- generated run artifacts needed explicit diff hygiene discipline;
- common proof-test quality should be centralized instead of repeated in every task prompt.
```

Files added by Pack 004:

```text
docs/agent-tasks/_TEST_QUALITY_RULES.md
docs/agent-tasks/_FIXTURE_AND_GOLDEN_RULES.md
docs/agent-tasks/_DIFF_HYGIENE_RULES.md
docs/agent-tasks/_AGENT_EXECUTION_QUALITY_RULES.md
```

Files updated by Pack 004:

```text
.devflow/AUTONOMOUS_RUNBOOK.md
.devflow/CODE_QUALITY_AND_STYLE.md
.devflow/DEFINITION_OF_DONE.md
.devflow/LOCAL_AGENT_REVIEW_CHECKLIST.md
.devflow/prompts/local_agent_start_prompt.md
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/_TASK_TEMPLATE.md
docs/agent-tasks/_TASK_READINESS_CHECKLIST.md
```

## Current active gate assumption

```text
M4.1 real-model evaluation gate
```

Allowed now:

```text
- strict parser corpus/fixture coverage;
- evaluation report/analyzer improvements if report exists;
- evaluation markdown/golden output guardrails;
- repair prompt hardening based on diagnostics/corpus evidence;
- docs/devflow/task-spec consistency work;
- optional devflow gate automation, only with user approval.
```

Locked now:

```text
- M5 Lua module executor production integration;
- M6 rich GamePackage assembly;
- broad artifact contract expansion;
- runtime preview repair loop.
```

## Do not regenerate in next pack

Do not replace the Pack 001 framework unless repository review finds a concrete problem.

Do not regenerate M5/M6 specs from scratch. Amend them only if repository source changes make the contracts stale.

Do not unlock M5/M6 from task-pack files alone. Only current-state docs may unlock those phases.

Do not copy shared quality rules into every future task spec. Reference the shared docs and add only task-specific emphasis.

## Next pack should cover

Preferred next generated pack after this one is applied and pushed:

```text
agent-task-pack-005-m4-1-completion-and-gate-review
```

Suggested contents:

```text
- M4.1 completion specs for real evaluation run/review;
- current-state update task after manual gate decision;
- report import/analyzer task if real evaluation artifact exists;
- no M5/M6 executable production specs unless current state has changed.
```

## Open questions for next pack

1. Has Pack 004 been applied on the branch used for local-agent execution?
2. Did local `check-all.ps1` pass after Pack 004 apply?
3. Has Kilo executed M4_1_005 or later using the shared quality docs?
4. Is a real strict LLM evaluation report present in `.llmgc/generator-plans/`?
5. Should M4.1 be marked passed, needs repair, or blocked?
