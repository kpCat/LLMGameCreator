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
.devflow/PHASE_PLAN_INDEX.md
.devflow/RECURSIVE_TASK_SELECTION_PROTOCOL.md
.devflow/CODE_QUALITY_AND_STYLE.md
.devflow/DEFINITION_OF_DONE.md
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/003_DEVELOPMENT_ROADMAP.md
docs/agent-tasks/004_PACK_GENERATION_POLICY.md
```

Then read only phase/source files relevant to the next unlocked or intentionally locked documentation pack.

## Expected next pack

```text
agent-task-pack-012-m4-1-execution-support-or-roadmap-freeze
```

## Current decision tree

```text
1. If M4.1 is still active and user is ready to run agents:
   Prefer executing existing M4.1 task specs instead of generating more future docs.

2. If M4.1 is still active and user wants one more documentation-only pack:
   Generate a roadmap freeze/checklist pack that prevents further speculative expansion and points back to M4.1 execution.

3. If M4.1 has explicitly passed in docs/CURRENT_GENERATOR_STATE.md and .json:
   Generate source-refreshed M5 executable entry specs from current source layout.

4. If Kilo/local-agent execution produced problems:
   Generate a focused repair/hardening pack before progressing.
```

## Required output format

The next pack must be a patch archive with:

```text
ARCHIVE_MANIFEST.md
README_APPLY_*.md
new/updated docs/agent-tasks files
only necessary .devflow updates
```

## Naming policy

Use short filenames for new task specs to avoid Windows path/archive issues.

Good examples:

```text
M10_001_EXPORTS.md
M10_002_UNITY_IR.md
M10_005_ASSETS.md
```

Avoid long descriptive filenames for new locked draft specs.

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

Pack 011 completes the locked future draft/spec skeleton through M10. If M4.1 is still active, stop producing future implementation specs and run or prepare execution of M4.1 tasks:

```text
M4_1_005 -> M4_1_006 -> M4_1_008
```

or use real-evaluation tasks if a report exists:

```text
M4_1_013 -> M4_1_014 -> M4_1_015 -> M4_1_016 -> M4_1_017
```

If one more documentation pack is requested before running Kilo, make it a roadmap freeze/checklist pack, not another future implementation pack.
