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
.devflow/CODE_QUALITY_AND_STYLE.md
.devflow/DEFINITION_OF_DONE.md
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/003_DEVELOPMENT_ROADMAP.md
docs/agent-tasks/004_PACK_GENERATION_POLICY.md
docs/agent-tasks/_TEST_QUALITY_RULES.md
docs/agent-tasks/_FIXTURE_AND_GOLDEN_RULES.md
docs/agent-tasks/_DIFF_HYGIENE_RULES.md
docs/agent-tasks/M4_1/000_M4_1_SEQUENCE.md
docs/agent-tasks/M5/000_M5_SEQUENCE.md
docs/agent-tasks/M6/000_M6_SEQUENCE.md
```

Then read only phase/source files relevant to the next unlocked or explicitly requested locked pack.

## Current pack state

Latest generated pack:

```text
agent-task-pack-006-future-phase-sequence-skeletons
```

## Expected next pack decision

The next pack depends on current gate state:

```text
Case A — M4.1 is still active and no real report exists:
  Prefer running existing M4.1 tasks instead of generating more executable specs.
  If a pack is still requested, generate only M4.1 execution-support or repair/hardening docs.

Case B — user wants documentation-only roadmap continuation while M4.1 is still active:
  Generate locked M5 entry draft specs or contract outlines.
  They must be explicitly non-executable until M4.1 passes.

Case C — docs/CURRENT_GENERATOR_STATE.md and .json say M4.1 passed:
  Generate Pack 007 M5 executable entry specs from current source layout.

Case D — local-agent execution exposed quality/process problems:
  Generate a repair/hardening pack before new feature specs.
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
- exact proof assertions
- fixture/golden policy when applicable
- diff hygiene risks
- system gates
- stop conditions
- next task pointer
```

No task spec is executable if it lacks a proof test.

No proof test is acceptable if it only checks broad failure/success without pinning the contract.

## Current recommendation

If the repository still has no real M4.1 evaluation report and M4.1 is not marked passed, do not produce M5/M6 executable implementation tasks.

Prefer one of:

```text
- run existing M4.1 executable tasks on an agent branch;
- generate a locked M5 draft-spec pack for documentation only;
- generate a repair/hardening pack if execution feedback requires it.
```
