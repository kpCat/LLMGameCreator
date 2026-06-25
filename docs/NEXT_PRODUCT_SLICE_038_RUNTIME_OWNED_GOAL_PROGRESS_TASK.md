# Product Slice 038 - Runtime-Owned Generated Goal Progress

## Purpose

Move generated active goal progress from preview-only projection toward runtime-owned state.

Goal 001 made progress visible. S038 must make progress owned by runtime state or by an existing runtime state extension point.

## Functional goal

After one-click generation and Runtime Preview start:

- active generated goal is visible;
- interaction advances goal progress;
- progress is stored in runtime-owned state or an existing serializable runtime state field/extension point;
- Runtime Preview renders that runtime-owned progress.

## Implementation direction

Read existing runtime state models and services first:

- `LLMGameCreator.Runtime.Abstractions`
- `LLMGameCreator.Runtime`
- quest/objective runtime services;
- runtime snapshot serialization/store;
- existing `GameState` fields and extension mechanisms.

Prefer:

- existing quest runtime state if available;
- existing flags/variables/resources/inventory state if available;
- narrow generated microgame state record if no current field fits.

Avoid:

- broad new quest system;
- broad GamePackage schema redesign;
- broad runtime command/state redesign.

## Required behavior

Minimum acceptable behavior:

- one generated active goal is selected deterministically;
- progress starts at 0 or a clearly defined start value;
- interaction command advances runtime-owned progress;
- progress reaches completion after deterministic steps;
- preview displays source of truth as runtime-backed, not just projection-backed;
- diagnostics clearly say whether any fallback path was used.

If full runtime ownership is blocked, stop and report exact blocker instead of building another projection-only layer.

## Tests

Add focused tests:

- runtime-owned progress initializes deterministically;
- interaction advances runtime-owned progress;
- preview reads runtime-owned progress;
- fallback/projection-only path is not silently used;
- no external execution.

Suggested filter:

```text
FullyQualifiedName~RuntimeOwnedGoalProgress
```

## Product smoke

Add scenario:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario runtime-owned-goal-progress
```

Smoke must prove:

```text
one-click workflow
-> runtime start
-> interaction
-> runtime-owned progress update
-> preview displays runtime-owned progress
```

## Docs/state

After S038:

- update state;
- next recommended task is S039 `runtime_reward_challenge_state`;
- no broad systems unlocked.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~RuntimeOwnedGoalProgress"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario runtime-owned-goal-progress
.\.devflow\scripts\check-all.ps1
```

## Constraints

Do not add LLM/provider/Lua/Unity/media execution.

Do not perform broad schema/runtime/UI redesign.

