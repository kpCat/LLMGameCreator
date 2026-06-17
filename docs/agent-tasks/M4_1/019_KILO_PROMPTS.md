# 019_KILO_PROMPTS.md — ready prompts for M4.1 execution

These prompts are for Kilo/local-agent execution branches. Run one task per branch/session until the agent proves reliable.

## Common prefix

Use this prefix before every task-specific block:

```text
Работай строго по одному task spec. Не расширяй задачу.

Запрещено:
- git commands;
- package install / NuGet add;
- VS Designer edits;
- repo-wide read;
- M5/M6/M8/M9/M10 production work;
- .sln/.csproj changes unless task explicitly allows;
- committing generated .devflow/runs/** artifacts;
- running advance-next-task.ps1 unless focused tests and check-all.ps1 passed.

Обязательно:
- русский финальный отчёт;
- перечислить прочитанные source docs;
- перечислить изменённые файлы вручную;
- proof tests with exact assertions;
- run required focused tests and check-all;
- stop after exactly one task, even if advance-next-task.ps1 updates NEXT_TASK.md.
```

If the completed task is listed in `.devflow/task-queue.json`, the agent may run:

```text
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\advance-next-task.ps1
```

only after focused tests and `check-all.ps1` pass. The script only updates `.devflow/NEXT_TASK.md`; it does not run Kilo, tests, git, or the next task. Stop immediately after the pointer advances.

## M4_1_005 prompt

```text
Task source: agent_task_spec
Task id: M4_1_005
Task spec file: docs/agent-tasks/M4_1/M4_1_005_EVALUATION_MARKDOWN_GOLDEN_RECOMMENDATIONS.md
Reason: Add stable markdown/golden recommendation checks after parser corpus guard.
User approval: approved
Expected stop after completion: yes

Read in order:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- .devflow/AUTONOMOUS_RUNBOOK.md
- .devflow/CODE_QUALITY_AND_STYLE.md
- .devflow/DEFINITION_OF_DONE.md
- docs/agent-tasks/000_INDEX.md
- docs/agent-tasks/_TEST_QUALITY_RULES.md
- docs/agent-tasks/_FIXTURE_AND_GOLDEN_RULES.md
- docs/agent-tasks/_DIFF_HYGIENE_RULES.md
- docs/agent-tasks/M4_1/M4_1_005_EVALUATION_MARKDOWN_GOLDEN_RECOMMENDATIONS.md

Before edits, update .devflow/CURRENT_RUN.md with the task plan.
After focused tests and check-all pass, if this task is listed in .devflow/task-queue.json, run advance-next-task.ps1 to update .devflow/NEXT_TASK.md only.
Run the task's focused test command and then:
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## M4_1_006 prompt

```text
Task source: agent_task_spec
Task id: M4_1_006
Task spec file: docs/agent-tasks/M4_1/M4_1_006_STRICT_REPAIR_PROMPT_GUARDRAILS.md
Reason: Add repair prompt guardrails after markdown/golden recommendation checks.
User approval: approved
Expected stop after completion: yes

Read the shared quality docs, then exactly this task spec and the source docs it names. Do not execute M4_1_008 or later in the same run.
Run the task's focused test command and then check-all. If this task is listed in `.devflow/task-queue.json`, run `advance-next-task.ps1` only after those checks pass, then stop.
```

## M4_1_008 prompt

```text
Task source: agent_task_spec
Task id: M4_1_008
Task spec file: docs/agent-tasks/M4_1/M4_1_008_AGENT_TASK_DOCS_CONSISTENCY_GUARD.md
Reason: Add task-doc consistency guard after parser/markdown/repair prompt hardening.
User approval: approved
Expected stop after completion: yes

Read the shared quality docs, then exactly this task spec and the source docs it names. Do not modify M5/M6/M8/M9/M10 production code. Do not unlock any future phase.
Run the task's focused test command and then check-all. If this task is listed in `.devflow/task-queue.json`, run `advance-next-task.ps1` only after those checks pass, then stop.
```

## Real-evaluation closure prompt starter

Use only when real/manual strict evaluation evidence exists:

```text
Task source: agent_task_spec
Task id: M4_1_014
Task spec file: docs/agent-tasks/M4_1/M4_1_014_REAL_EVALUATION_EVIDENCE_MANIFEST.md
Reason: Record real/manual strict evaluation evidence before report import/gate closure.
User approval: approved
Expected stop after completion: yes
```
