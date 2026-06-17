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
docs/agent-tasks/005_ROADMAP_FREEZE.md
docs/agent-tasks/006_BRANCH_RUNBOOK.md
```

Then read only phase/source files relevant to the next unlocked work or repair pack.

## Expected next pack

```text
agent-task-pack-013-by-execution-feedback
```

## Current decision tree

```text
1. If M4.1 is still active and Kilo has not run M4_1_005:
   Do not generate more future roadmap packs. Run M4_1_005 on a dedicated execution branch.

2. If M4_1_005/M4_1_006/M4_1_008 execution produced problems:
   Generate a focused repair/hardening pack using the branch diff and agent report.

3. If deterministic M4.1 hardening tasks passed and real evaluation evidence exists:
   Generate/import support for M4_1_014..M4_1_017 or update the gate decision path.

4. If docs/CURRENT_GENERATOR_STATE.md and .json explicitly pass M4.1:
   Generate source-refreshed M5 executable entry specs from current source layout.

5. If the user asks for more speculative future roadmap docs while M4.1 is active:
   Stop and ask for a concrete gap. The roadmap is frozen after M10 locked drafts.
```

## Naming policy

Use short filenames for new task specs to avoid Windows path/archive issues.

Good examples:

```text
M9_001_TEMPLATES.md
M10_001_EXPORTS.md
M4_1/018_EXEC_QUEUE.md
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

Pack 012 freezes future documentation packs and prepares M4.1 execution on a branch.

Next practical step:

```text
Create an execution branch from main and run exactly M4_1_005 with Kilo/local agent.
```

Recommended execution sequence after M4_1_005 passes:

```text
M4_1_006 -> M4_1_008
```

Recommended real-evaluation closure path when evidence exists:

```text
M4_1_013 -> M4_1_014 -> M4_1_015 -> M4_1_016 -> M4_1_017
```
