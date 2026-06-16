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

Files updated by Pack 002:

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
- docs/devflow/task-spec consistency work.
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
agent-task-pack-003-m4-1-gates-and-automation
```

Suggested contents:

```text
- optional named gates in .devflow/scripts/check-all.ps1 only if the user wants script changes;
- docs consistency guard task expansion if M4_1_008 is not enough;
- agent task specs for importing real local-model evaluation artifacts if a report is present;
- a task spec for updating CURRENT_GENERATOR_STATE after manual M4.1 gate review;
- no M5/M6 executable production specs unless current state has changed.
```

## Open questions for next pack

1. Is a real strict LLM evaluation report present in `.llmgc/generator-plans/`?
2. Has Kilo executed any M4_1 task spec successfully?
3. Should `check-all.ps1` gain named optional gates for docs/manifests/runtime/snapshots now, or wait until fixtures exist?
4. Should `NEXT_TASK.md` be moved from `BASELINE-001` to `agent_task_spec:M4_1_004` after user approval?
