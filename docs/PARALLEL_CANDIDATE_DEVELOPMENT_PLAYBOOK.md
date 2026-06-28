# Parallel Candidate Development Playbook

## Purpose

This document defines how to run multiple Codex tasks safely without corrupting the active source of truth.

Parallel work is allowed only when it is separated by path ownership, candidate status and serial adoption.

## Core rule

Only one active writer may update current state:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Parallel branches or worktrees must not claim accepted gates, advance current work or start future goals. They produce candidate modules.

## How multiple branches/worktrees work

You cannot be "on three branches" in one folder at the same time.

Use one of these models:

### Model A: separate clones

```text
C:\Users\endim\LLMGameCreator
C:\Users\endim\LLMGameCreator-candidate-combat
C:\Users\endim\LLMGameCreator-candidate-factions
```

Each folder has its own branch and working tree.

### Model B: git worktrees

```powershell
git worktree add C:\Users\endim\LLMGameCreator-worktrees\candidate-combat candidate/combat
git worktree add C:\Users\endim\LLMGameCreator-worktrees\candidate-factions candidate/factions
```

Each worktree is a separate folder connected to the same repository object database.

Codex does not magically make safe parallel state by itself. If multiple Codex dialogs point at the same local path, they can overwrite/conflict. Give each parallel Codex a distinct working folder or restrict it to read-only planning.

## Safe parallel roles

### Active implementation lane

- one at a time;
- may update state docs;
- may have one final manual gate;
- may write current `.llmgc/procedural/<goal>/` artifact root;
- may be reviewed and accepted.

### Candidate module lane

- may implement a module in an owned folder;
- may write candidate artifacts under `.llmgc/candidates/<candidate-id>/` or branch-local current artifact root;
- must not update active state docs;
- must not claim final gate passed;
- must not mutate historical artifacts;
- must pass module-level verification.

### Read-only planning lane

- docs/planning only;
- no production code;
- no state docs;
- no accepted artifacts;
- useful for scouting, contract drafts, fixture design and risk audits.

## Conflict resolution

Conflicts are resolved by serial adoption, not by allowing branches to fight over main.

Adoption flow:

1. Rebase or refresh candidate branch/worktree on accepted main.
2. Run candidate module verification.
3. Run module manifest validation.
4. Run compatibility matrix.
5. Reject candidates that change forbidden/shared paths without explicit adoption approval.
6. Adopt candidates one at a time.
7. Update state docs once.
8. Run campaign/kernel verification.
9. Run scope guard.

## Shared path policy

Candidate module branches must not edit these unless the task is a kernel/adoption task:

- `docs/CURRENT_GENERATOR_STATE.*`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `.devflow/scripts/run-product-smoke.ps1` after manifest routing exists
- `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs` after module registry exists
- public `GamePackage` schema
- `.sln` / `.csproj`
- historical `.llmgc/procedural/**`

## Module compatibility requirements

Every candidate module should declare:

- module id;
- input contracts;
- output contracts;
- required dependencies;
- optional dependencies;
- absence behavior;
- validators;
- owned artifact root;
- owned smoke scenario manifest;
- deterministic proof hashes;
- forbidden runtime/provider dependencies.

Compatibility matrix checks:

- required input contracts exist;
- output contracts have known schema;
- dependency versions match;
- optional dependency absence is handled;
- missing module does not crash kernel;
- forbidden dependencies absent;
- owned paths only;
- deterministic output stable.

## Verification tiers

### Tier 1: module-only

Used by candidate modules:

- focused module tests;
- module smoke via manifest;
- module manifest validation;
- compatibility matrix for touched modules;
- artifact scope guard.

### Tier 2: kernel

Used when registry, manifest loader or shared kernel changes:

- all module manifests parse;
- registry compatibility tests;
- absence/presence tests;
- smoke manifest loader tests;
- selected module smokes;
- check-all if kernel behavior changed.

### Tier 3: campaign integration

Used after several candidate modules are adopted:

- check-all;
- selected cross-module smokes;
- compatibility matrix across all current modules.

### Tier 4: product vertical gate

Rare. Used for generated playable/simulatable/runtime-facing proof.

## Stop conditions

Stop parallel adoption and return diagnosis when:

- two candidate modules require conflicting versions of the same contract;
- a candidate needs public schema changes but was not scoped for it;
- a candidate edits active state docs;
- a candidate mutates historical artifacts;
- a candidate fails absence behavior;
- compatibility matrix cannot prove safe interaction.
