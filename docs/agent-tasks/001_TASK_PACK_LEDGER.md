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

Repository state observed for this pack:

```text
Branch reviewed: kilo-night-001
Compared to main: ahead by 1 commit, docs/devflow-only task pack changes.
Current phase: M4.1 real-model evaluation gate.
Strict parser owner: GeneratorPlanStrictJsonResponseParser.
Strict parser tests: GeneratorPlanStrictJsonResponseParserTests.
Strict evaluation owner: GeneratorPlanStrictLlmEvaluationService.
Strict evaluation markdown owner: GeneratorPlanStrictLlmEvaluationMarkdownRenderer.
Repair prompt owner: GeneratorPlanStrictLlmArtifactRepairPromptBuilder.
```

Files added by Pack 002:

```text
docs/agent-tasks/M4_1/000_M4_1_SEQUENCE.md
docs/agent-tasks/M4_1/M4_1_004_STRICT_JSON_PARSER_CORPUS_GUARD.md
docs/agent-tasks/M4_1/M4_1_005_EVALUATION_MARKDOWN_GOLDEN_RECOMMENDATIONS.md
docs/agent-tasks/M4_1/M4_1_006_STRICT_REPAIR_PROMPT_GUARDRAILS.md
docs/agent-tasks/M4_1/M4_1_007_M4_GATE_DECISION_REPORT.md
docs/agent-tasks/M4_1/M4_1_008_AGENT_TASK_DOCS_CONSISTENCY_GUARD.md
```

## Pack 003 — M4.1 gates and automation specs

Pack id: `agent-task-pack-003-m4-1-gates-and-automation`

Generated purpose:

```text
Add executable task specs for M4.1 gate automation, local-agent run review, real evaluation artifact discovery, and current-state update discipline.
```

Repository state observed for this pack:

```text
Branch reviewed: kilo-night-001
Compared to main: ahead by 2 commits, docs/devflow/task-spec changes only.
Latest pushed commit checked for GitHub workflow runs: no workflow runs found.
M4.1 remains active gate.
M5/M6/M8 remain locked by current-state docs.
```

Files added by Pack 003:

```text
docs/agent-tasks/M4_1/M4_1_009_DEVFLOW_NAMED_GATES_CHECK_ALL.md
docs/agent-tasks/M4_1/M4_1_010_REAL_EVALUATION_ARTIFACT_DISCOVERY.md
docs/agent-tasks/M4_1/M4_1_011_CURRENT_STATE_GATE_REVIEW_UPDATE.md
docs/agent-tasks/M4_1/M4_1_012_OVERNIGHT_RUN_REPORT_REVIEW_GATE.md
```

Files updated by Pack 003:

```text
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/M4_1/000_M4_1_SEQUENCE.md
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

## Next pack should cover

Preferred next generated pack after this one is applied and pushed:

```text
agent-task-pack-004-m4-1-execution-results-or-m5-entry
```

Suggested contents:

```text
- If Kilo has executed M4_1_004..M4_1_006, review reports/diff and refine task specs based on actual failures.
- If a real strict LLM evaluation report exists, add/update import/analyzer and gate decision specs.
- If current-state docs explicitly pass M4.1, generate M5 entry execution specs based on current source.
- If M4.1 is still not reviewed, do not unlock M5/M6; continue deterministic coverage/gate work.
```

## Open questions for next pack

1. Has Kilo executed any M4_1 task spec successfully?
2. Is `.devflow/OVERNIGHT_RUN_REPORT.md` present and useful?
3. Is a real strict LLM evaluation report present in `.llmgc/generator-plans/`?
4. Did local `check-all.ps1` pass after Pack 003 apply?
5. Should M4.1 be marked passed, needs repair, or blocked?
