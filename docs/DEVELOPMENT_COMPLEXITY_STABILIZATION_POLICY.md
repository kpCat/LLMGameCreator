# Development Complexity Stabilization Policy

Status: Goal 022 policy  
Final gate: `development_complexity_stabilization_verification`

## Purpose

Future goals must be reviewable without forensic cleanup of unrelated generated artifacts. This policy defines which paths a goal may mutate, how product smoke routes declare root artifact writes, and how final verification must prove that unrelated tracked artifacts stayed stable.

The policy is enforceable through:

- `.devflow/artifact-scope/artifact-scope-policy.json`;
- `.devflow/scripts/check-artifact-scope.ps1`;
- focused xUnit tests under `tests/LLMGameCreator.Tests/Devflow/`;
- the `development-complexity-stabilization` product smoke route.

## Artifact Mutability Classes

### Source Code And Docs

Source code, scripts and durable docs may change only when the active goal explicitly lists them. Project files, public package schema files, generator-library assets, WinForms UI files and Unity build entrypoints are forbidden by default.

### State And Handoff Docs

The standard mutable state docs are:

- `docs/CURRENT_GENERATOR_STATE.json`;
- `docs/CURRENT_GENERATOR_STATE.md`;
- `docs/CONTEXT_INDEX.md`;
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`.

They may change only when a task changes gates, recommended next work, generator workflow, or source-of-truth routing.

### Current-Goal Compact Review Artifacts

A goal may write only its declared compact artifact root, for example:

```text
.llmgc/procedural/development-complexity-stabilization/
```

Product smoke routes that intentionally regenerate repo-root compact artifacts must declare the scenario id and artifact family in `run-product-smoke.ps1` and in the task artifacts.

### Historical Compact Artifacts

Historical compact artifacts under `.llmgc/procedural/<older-family>/` are normally read-only during unrelated goals. A bounded hotfix may mutate them only when the task names the exact family and restoration paths.

### Heavy Generated Build And Runtime Outputs

Heavy generated build/runtime outputs are normally untracked or ignored. This includes Unity build folders, player logs, cache/output folders, data folders, and review-package binary payloads. They may be inventoried and warned about, but this goal does not delete or untrack them.

### Task-Pack Docs

Task wrappers and goal docs may change only when they are the active task source or when the state handoff must link the active goal.

## Goal Scope Rules

- A goal may write only its declared artifact root and declared state docs unless a bounded hotfix explicitly lists restoration paths.
- `check-all.ps1` and ordinary tests must not mutate tracked historical artifacts.
- Product smoke routes that intentionally regenerate repo-root compact artifacts must declare that behavior and artifact family.
- After every hotfix and after every goal final verification, artifact scope must be checked before the gate is accepted.
- Invalid/fake/leak matrices should use shared mutation and validation helpers where possible instead of bespoke report-only diagnostics.
- If more than 8-10 files or more than one artifact family must change, split the task or document a bounded exception before editing.
- Housekeeping and cleanup tasks must not be mixed into product goals unless explicitly requested.

## Check-All Isolation

`check-all.ps1` must set product-smoke/test artifact output variables to paths under the current check-all run directory for its test phase, then restore the previous values in `finally`:

- `LLMGC_PRODUCT_SMOKE_PROJECT_DIR`;
- `LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR`.

When existing root-artifact product smoke tests need accepted baseline evidence, `check-all.ps1` may copy `.llmgc/procedural/` into the run-local project root before tests. Tests may read and mutate that run-local copy, but must not mutate repo-root historical artifacts.

`check-all.ps1` should treat product smoke routes as explicit scenario verification, not ordinary full-suite churn. The full test phase may exclude `FullyQualifiedName~ProductSmoke` while the active goal still runs its required product smoke route separately.

Scenario-specific `run-product-smoke.ps1` behavior may still regenerate declared repo-root compact artifacts for explicit review scenarios.

## Scope Guard Usage

Run the scope guard after final smoke/check-all verification with exact allowed files and prefixes for the active goal. A typical invocation declares:

- policy/config/script files added by the goal;
- the active compact artifact root;
- focused test folders;
- state/routing docs;
- active task docs.

Violations are blocking unless the active task explicitly declares a bounded exception.

## Audit Cadence

- After every 5 accepted goals, run a stabilization audit over artifact scope, smoke routes and check-all isolation.
- After every 10 accepted goals, run an architecture/process audit to check that goal sequencing, manual gates and artifact families still reduce review load.

## Non-Goals

This policy does not authorize Capability Bundle Selection, Goal 023, S185, GamePackage schema changes, Unity build work, provider/LLM/RAG/Lua/media execution, generator-library edits, WinForms UI work, or broad cleanup of old generated outputs.
