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
```

Then read only phase/source files relevant to the next unlocked pack.

## Expected next pack

```text
agent-task-pack-006-full-sequence-skeletons
```

The next pack should define phase sequence skeletons for M5/M6/M8/M9/M10 and must keep future production tasks locked unless current-state docs explicitly unlock them.

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

No task spec is executable if it lacks proof tests.

No proof test is acceptable if it only checks broad failure/success without pinning the contract.

## Current recommendation

If M4.1 has not explicitly passed in `docs/CURRENT_GENERATOR_STATE.md` and `.json`, generate only locked future skeletons and M4.1 closure support. Do not generate executable M5/M6 production specs.

If M4.1 has passed, generate M5 entry executable specs using current source code and the shared quality docs.

If Kilo/local-agent execution fails or drifts, generate a repair/hardening pack before generating more feature tasks.
