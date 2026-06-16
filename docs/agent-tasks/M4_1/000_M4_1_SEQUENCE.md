# 000_M4_1_SEQUENCE.md — recommended M4.1 agent-task sequence

This file is a small routing aid for the M4.1 real-model evaluation gate. It is not a roadmap and not an implementation spec.

## Current gate

```text
M4.1 real-model evaluation gate
```

The current state blocks M5/M6/M8 production work until the real strict LLM evaluation has been run, reviewed, and current-state docs explicitly unlock follow-up work.

## Recommended deterministic sequence before using real report evidence

Use this sequence if no real local-model report is available yet:

```text
1. M4_1_004_STRICT_JSON_PARSER_CORPUS_GUARD
2. M4_1_005_EVALUATION_MARKDOWN_GOLDEN_RECOMMENDATIONS
3. M4_1_006_STRICT_REPAIR_PROMPT_GUARDRAILS
4. M4_1_008_AGENT_TASK_DOCS_CONSISTENCY_GUARD
```

## Recommended evidence sequence after real report exists

Use this sequence if `.llmgc/generator-plans/` contains a real strict evaluation report or the user provides it:

```text
1. M4_1_001_REAL_EVALUATION_REPORT_IMPORT
2. M4_1_003_REPAIR_POLICY_HARDENING
3. M4_1_007_M4_GATE_DECISION_REPORT
```

## Rules

```text
- Read exactly one task spec at a time.
- Do not execute M5/M6 from this file.
- Do not update CURRENT_GENERATOR_STATE to unlock M5/M6 from agent judgment alone.
- Human gate review is required before phase unlock.
```

## Suggested NEXT_TASK pointer for first deterministic task

```text
# NEXT_TASK

Mode: single-task
Task source: agent_task_spec
Task id: M4_1_004
Task spec file: docs/agent-tasks/M4_1/M4_1_004_STRICT_JSON_PARSER_CORPUS_GUARD.md
Reason: Add fixture-driven proof coverage for strict JSON parser behavior before further prompt/repair changes.
User approval: approved
Expected stop after completion: yes
```
