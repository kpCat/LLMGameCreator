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
agent-task-pack-003-m4-1-gates-and-automation
```

The next pack should improve M4.1 gates/automation and should not unlock M5/M6 unless repository current state explicitly unlocks them.

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

If the repository still has no real M4.1 evaluation report, prefer task specs that improve deterministic coverage:

```text
- parser corpus coverage;
- evaluation markdown golden output;
- repair prompt guardrails;
- docs/task-spec consistency guard.
```

If a real M4.1 report exists, prefer task specs that import/analyze it and update current-state docs only after explicit human gate review.
