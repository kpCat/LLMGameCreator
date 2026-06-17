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
docs/agent-tasks/007_START_RUN.md
```

Then read only the branch diff/report if a Kilo/local-agent execution has occurred.

## Expected next pack

```text
agent-task-pack-014-by-m4-1-005-result
```

## Current decision tree

```text
1. If M4_1_005 has not run yet:
   Do not generate more speculative roadmap packs. Run M4_1_005 on a dedicated execution branch.

2. If M4_1_005 passed cleanly:
   Generate an execution-start pack for M4_1_006 or update NEXT_TASK to M4_1_006 with approval.

3. If M4_1_005 failed or changed files outside scope:
   Generate a focused repair/hardening pack based on the exact diff and agent report.

4. If M4_1_005 exposed task-spec ambiguity:
   Generate a small task-spec amendment pack before rerun.

5. If real strict evaluation evidence appears:
   Use M4_1_014..M4_1_017 closure path.

6. If docs/CURRENT_GENERATOR_STATE.md and .json explicitly pass M4.1:
   Generate source-refreshed M5 executable entry specs from current source layout.
```

## Naming policy

Use short filenames for new task docs to avoid Windows path/archive issues.

## Quality bar

Every executable task or repair task must contain:

```text
- Task ID or repair target;
- dependencies;
- allowed files;
- forbidden files;
- exact behavior;
- proof tests with exact assertions;
- system gates;
- stop conditions;
- next task pointer.
```

No proof test is acceptable if it only checks broad failure/success without pinning the contract.

## Current recommendation

Pack 013 activates `M4_1_005` by setting `.devflow/NEXT_TASK.md` approval for an execution branch.

Next practical step:

```text
Create branch from main, run Kilo/local agent for exactly M4_1_005, then review with docs/agent-tasks/M4_1/022_REPORT.md.
```
