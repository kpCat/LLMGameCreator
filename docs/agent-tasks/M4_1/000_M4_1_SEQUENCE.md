# 000_M4_1_SEQUENCE.md — recommended M4.1 executable task order

This file is routing guidance. Do not implement directly from it. Pick exactly one task spec and execute only that spec.

## Current gate

```text
M4.1 real-model evaluation gate is active.
M5/M6/M8 remain locked until docs/CURRENT_GENERATOR_STATE.md and .json explicitly unlock them.
```

## Recommended deterministic sequence before real report review

```text
M4_1_004 -> M4_1_005 -> M4_1_006 -> M4_1_008
```

Purpose:

```text
Improve strict generation/evaluation confidence with deterministic parser, markdown, repair-prompt and task-doc consistency coverage before broadening contracts or moving to Lua/package assembly.
```

## Recommended gate automation sequence

```text
M4_1_009 -> M4_1_012
```

Use this only when the user approves script/gate automation work or after an overnight/local-agent run exists.

## Recommended real evaluation closure sequence

If no user-facing real evaluation runbook exists yet:

```text
M4_1_013
```

If real/manual evaluation evidence exists:

```text
M4_1_014 -> M4_1_015 -> M4_1_016 -> M4_1_017
```

If a real strict LLM evaluation report already exists and older tasks are preferred:

```text
M4_1_001 -> M4_1_010 -> M4_1_003 -> M4_1_007 -> M4_1_011
```

Meaning:

```text
prepare runbook -> record evidence manifest -> import/analyze report fixture -> close gate by user decision -> verify M4.1 completion checklist
```

## Task map

| Task | Use when | Next candidate |
|---|---|---|
| M4_1_001 | Real strict LLM report exists and needs import/analyzer | M4_1_003 or M4_1_007 |
| M4_1_002 | Need broad raw-output corpus foundation | M4_1_004 |
| M4_1_003 | Real diagnostic hot spots exist | M4_1_007 |
| M4_1_004 | Need deterministic parser corpus proof tests | M4_1_005 |
| M4_1_005 | Need stable markdown/golden recommendations | M4_1_006 |
| M4_1_006 | Need repair prompt guardrails | M4_1_008 or M4_1_007 |
| M4_1_007 | Need M4.1 decision report | M4_1_011 |
| M4_1_008 | Need docs/task-spec consistency guard | M4_1_009 or stop |
| M4_1_009 | User approves named gate script automation | M4_1_012 |
| M4_1_010 | Real evaluation artifacts may exist but path/schema is unclear | M4_1_001 or M4_1_007 |
| M4_1_011 | User has explicitly decided M4.1 gate status | stop or M5 entry pack |
| M4_1_012 | Overnight/local-agent run exists and needs review gate | stop or next spec based on report |
| M4_1_013 | Need clear manual runbook for real strict evaluation | M4_1_014 |
| M4_1_014 | Real/manual evaluation evidence exists and needs manifest | M4_1_015 |
| M4_1_015 | Evidence manifest exists and report fixture/import guard is needed | M4_1_016 |
| M4_1_016 | User explicitly decides pass/needs_repair/blocked | M4_1_017 or M5 pack |
| M4_1_017 | M4.1 needs final completion checklist | stop or M5 pack |

## Stop rules

Stop instead of continuing if:

```text
- M4.1 decision needs user review;
- a task requires M5/M6/M8 production work;
- a task requires schema/dependency/project changes;
- a real report is missing for report-dependent tasks;
- check-all fails after repair attempts;
- local agent changed files outside allowed boundaries;
- current-state docs do not explicitly unlock the next phase.
```
