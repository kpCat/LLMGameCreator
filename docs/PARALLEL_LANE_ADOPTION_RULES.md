# Parallel Lane Adoption Rules

Status: proposed after Goal 029.

## Purpose

This document defines how LLMGameCreator can use several long-lived lane folders and lane branches without losing a single stable `main`.

The goal is faster candidate development, not uncontrolled branching.

## Stable Source Of Truth

`main` is the only accepted source of truth.

Only `main` may contain the current accepted state:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- accepted `.llmgc/procedural/**` reports
- accepted product smoke routing

Parallel lanes are not accepted state. They are candidate workspaces.

## Lane Layout

Recommended local folders:

- `C:\Users\endim\LLMDevelop\main\LLMGameCreator`
- `C:\Users\endim\LLMDevelop\lane-a\LLMGameCreator`
- `C:\Users\endim\LLMDevelop\lane-b\LLMGameCreator`
- `C:\Users\endim\LLMDevelop\lane-c\LLMGameCreator`
- `C:\Users\endim\LLMDevelop\lane-d\LLMGameCreator`

Recommended lane branch names:

- `lane-a`
- `lane-b`
- `lane-c`
- `lane-d`

Lane names may be reused. Candidate ids must be unique.

Example:

- branch: `lane-a`
- candidate id: `candidate_dialogue_narrative_tooling_v1`
- final status: `candidate_ready_for_serial_adoption`

## Candidate Task Rules

A candidate task may:

- add candidate-owned docs;
- add candidate-owned module contracts;
- add candidate-owned tests;
- add candidate-owned smoke scenario manifests;
- add candidate-owned compact proof artifacts;
- create an external technology scouting report;
- propose adapter boundaries.

A candidate task must not:

- claim an accepted manual gate;
- update `docs/CURRENT_GENERATOR_STATE.md`;
- update `docs/CURRENT_GENERATOR_STATE.json`;
- update `docs/CONTEXT_INDEX.md` unless the task is explicitly documentation adoption only;
- update `docs/FULL_GENERATOR_GOAL_QUEUE.md`;
- modify public `GamePackage` schema;
- modify `.sln` or `.csproj`;
- modify WinForms UI;
- modify Unity runtime entrypoints;
- modify generator-library content;
- add live runtime LLM, RAG, provider, media, or network dependency;
- execute Lua/generators unless the task explicitly allows it;
- perform branch management, pushing, rebasing, or broad merge work.

If a candidate needs a shared/kernel change, it must stop and report:

```text
requires_kernel_or_serial_adoption_task
```

## External Technology Scouting Rule

Before any non-trivial subsystem implementation, the candidate must include external technology scouting.

Scouting must answer:

- what was searched;
- candidate libraries, datasets, algorithms, formats, tools, or Unity packages found;
- what was accepted, adapted, used only as reference, deferred, or rejected;
- license and attribution impact;
- maturity and maintenance signal;
- deterministic behavior;
- offline compatibility;
- runtime footprint;
- dependency replacement plan;
- whether an adapter/wrapper is required;
- whether the candidate introduces any runtime provider, live API, LLM, RAG, or paid/proprietary dependency.

Default architecture:

```text
LLMGameCreator contract
  -> internal adapter
  -> optional external library/dataset/tool
```

The external dependency must not become the architecture.

## Serial Adoption

Candidates are accepted into `main` one at a time.

Recommended adoption order:

1. Choose exactly one lane candidate.
2. Review changed files and candidate report.
3. Verify candidate-owned paths do not overlap accepted/shared ownership.
4. Bring candidate changes into the main working copy only if they fit current contracts.
5. Run focused candidate tests.
6. Run product smoke scenario if the candidate added one.
7. Run current state docs tests if state docs are updated during adoption.
8. Run full or agreed validation.
9. Update current state docs only in `main`.
10. Stop on the single manual gate required for that adoption.

Do not accept several candidates in one uncontrolled step.

## Conflict Classification

Simple candidate conflicts may be resolved during serial adoption:

- independent files under different candidate-owned roots;
- independent smoke scenario manifest files;
- independent test folders;
- documentation index references;
- non-overlapping compact proof artifacts.

Architectural conflicts must stop adoption:

- two candidates use the same module id;
- two candidates own the same artifact root;
- two candidates define incompatible contract versions;
- candidate changes public `GamePackage` schema;
- candidate changes `.sln` or `.csproj`;
- candidate changes `GeneratorPlanGamePackageAssembler.cs` without explicit kernel/adoption scope;
- candidate changes `.devflow/scripts/run-product-smoke.ps1` without explicit kernel/adoption scope;
- candidate changes active state docs directly;
- candidate introduces runtime provider, live API, RAG, LLM, media, or Lua execution dependency outside scope.

Stop status:

```text
candidate_requires_split_or_kernel_decision
```

## Lane Refresh Rule

After a candidate is accepted or rejected, the lane should be refreshed to the latest accepted `main` before the next candidate starts.

This is user-controlled branch management. Candidate task prompts should not ask agents to perform branch management.

## Minimum Final Report From Candidate Agent

Every candidate final report must include:

- candidate id;
- base accepted gate;
- changed files;
- external scouting summary;
- adopted/adapted/reference/rejected technology decisions;
- module ids and artifact roots, if any;
- tests run;
- smoke scenarios run;
- stop status;
- exact blockers, if any.

Required final status for a successful candidate:

```text
candidate_ready_for_serial_adoption
```

