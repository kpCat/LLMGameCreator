# 002_NEXT_PACK_REQUEST.md — request contract for the next generated pack

Use this file when asking ChatGPT to generate the next archive.

## Request

Generate the next `docs/agent-tasks` pack from the current repository state, not from chat memory.

## Required read set for the generator

Read only what is needed:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/RECURSIVE_TASK_SELECTION_PROTOCOL.md
.devflow/CONTEXT_BUDGET_POLICY.md
.devflow/PHASE_PLAN_INDEX.md
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/M4_1/000_M4_1_SEQUENCE.md
```

Then read only phase/source files relevant to the next unlocked pack.

## Expected next pack

```text
agent-task-pack-004-m4-1-execution-results-or-m5-entry
```

The next pack depends on repository state after Pack 003 and/or after local-agent execution:

```text
- If no local-agent task was executed yet, do not create more speculative implementation specs; recommend executing M4_1_004 first.
- If M4_1 tasks were executed, inspect diffs and reports and refine only the failing/next layer.
- If real M4.1 evaluation artifacts exist, add/update report import and gate decision specs.
- If current-state docs explicitly pass M4.1, generate M5 entry specs based on current source.
- If M4.1 is still active, do not unlock M5/M6.
```

## Required output format

The next pack must be a patch archive with:

```text
ARCHIVE_MANIFEST.md
README_APPLY_*.md
new/updated docs/agent-tasks files
only necessary .devflow updates
```

## Quality bar

Every new executable task spec must contain:

```text
- Task ID
- dependencies
- allowed files
- forbidden files
- existing patterns to inspect
- exact behavior
- diagnostic/failure behavior
- proof tests
- system gates
- stop conditions
- next task pointer
```

No task spec is executable if it lacks a proof test.

## Current recommendation

Before requesting Pack 004, prefer to run one bounded local-agent implementation task, starting with:

```text
M4_1_004_STRICT_JSON_PARSER_CORPUS_GUARD
```

Then push the branch and ask for repository review. Pack 004 should be based on the actual diff/report, not on more speculative planning.
