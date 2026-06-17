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

## Pack 005 — M4.1 completion and roadmap policy

Pack id: `agent-task-pack-005-m4-1-completion-and-roadmap-policy`

Generated purpose:

```text
Add M4.1 gate-closure task specs plus durable roadmap/pack-generation policy so future archives are generated consistently from repo state.
```

Files added by Pack 005:

```text
docs/agent-tasks/003_DEVELOPMENT_ROADMAP.md
docs/agent-tasks/004_PACK_GENERATION_POLICY.md
docs/agent-tasks/M4_1/M4_1_013_STRICT_EVALUATION_RUNBOOK_FOR_USER.md
docs/agent-tasks/M4_1/M4_1_014_REAL_EVALUATION_EVIDENCE_MANIFEST.md
docs/agent-tasks/M4_1/M4_1_015_REAL_REPORT_IMPORT_FIXTURE_GUARD.md
docs/agent-tasks/M4_1/M4_1_016_M4_GATE_CLOSURE_DECISION.md
docs/agent-tasks/M4_1/M4_1_017_M4_1_COMPLETION_CHECKLIST.md
```

## Pack 006 — future phase sequence skeletons

Pack id: `agent-task-pack-006-future-phase-sequence-skeletons`

Generated purpose:

```text
Add locked sequence skeletons for M5/M6/M8/M9/M10 so the roadmap is visible in repo docs without producing stale executable implementation specs while M4.1 remains active.
```

Files added by Pack 006:

```text
docs/agent-tasks/M5/000_M5_SEQUENCE.md
docs/agent-tasks/M6/000_M6_SEQUENCE.md
docs/agent-tasks/M8/000_M8_SEQUENCE.md
docs/agent-tasks/M9/000_M9_SEQUENCE.md
docs/agent-tasks/M10/000_M10_SEQUENCE.md
```

Files updated by Pack 006:

```text
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
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
- optional devflow gate automation, only with user approval;
- roadmap/locked skeleton documentation for future phases.
```

Locked now:

```text
- M5 Lua module executor production integration;
- M6 rich GamePackage assembly;
- M8 runtime preview validation loop;
- M9 template/balancing implementation;
- M10 export/Unity IR implementation;
- broad artifact contract expansion.
```

## Do not regenerate in next pack

Do not replace Pack 001 framework unless repository review finds a concrete problem.

Do not regenerate existing M4.1 executable specs from scratch. Amend them only if execution feedback or source changes expose concrete problems.

Do not unlock M5/M6/M8/M9/M10 from task-pack files alone. Only current-state docs may unlock phases.

Do not convert future sequence skeletons into executable specs until the relevant gate is open.

## Next pack should cover

Preferred next generated pack after this one is applied and pushed:

```text
agent-task-pack-007-next-step-by-gate-state
```

Decision policy:

```text
- If M4.1 is still active and no real report exists, prefer running existing M4.1 tasks before generating more executable specs.
- If user wants documentation-only continuation, generate locked M5 entry draft specs, not executable production tasks.
- If current-state docs explicitly pass M4.1, generate M5 executable entry specs from current source layout.
- If local-agent execution finds problems, insert a repair/hardening pack before progressing.
```

## Open questions for next pack

1. Has Pack 006 been applied to main and has `check-all.ps1` passed?
2. Has Kilo executed M4_1_005 or later using Pack 004 quality docs?
3. Is a real strict LLM evaluation report present in `.llmgc/generator-plans/`?
4. Have `docs/CURRENT_GENERATOR_STATE.md` and `.json` marked M4.1 as passed?
5. Should the next pack be M4.1 execution support, locked M5 draft specs, or executable M5 entry specs?
