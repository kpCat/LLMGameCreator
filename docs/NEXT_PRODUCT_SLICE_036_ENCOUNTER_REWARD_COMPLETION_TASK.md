# Product Slice 036 - Encounter/Obstacle + Reward/Completion Loop

## Purpose

Extend the generated microgame from active-goal/progress into a small playable challenge loop.

S036 should add a generated obstacle/encounter and a visible reward/completion step.

## Functional goal

After one-click generation and Runtime Preview start, the player should be able to:

- see the active goal from S035;
- encounter a generated obstacle/challenge;
- resolve it through existing runtime/package/preview concepts;
- receive reward/progress feedback;
- see completion or near-completion state.

## Implementation direction

Use current contracts first:

- package encounters;
- abilities;
- interactions;
- items/resources;
- quest objectives/rewards;
- runtime encounter/interaction/quest services;
- preview generated-content panels.

Allowed narrow adapter:

- preview-level microgame challenge resolver if runtime cannot fully connect generated encounter resolution yet.

Forbidden:

- broad runtime redesign;
- broad GamePackage schema redesign;
- new combat system;
- Unity/Lua/media/provider work.

## Required behavior

Minimum acceptable behavior:

- one generated challenge/encounter is selected deterministically;
- it is linked to active goal;
- player can trigger/resolve it via existing command or narrow preview action;
- reward or progress delta becomes visible;
- completion condition is represented in preview/report.

If true completion cannot be stored in existing runtime state, use a deterministic preview-level completion model and document the limitation.

## Tests

Add focused tests:

- generated challenge is deterministic;
- challenge references existing package encounter/item/NPC/quest ids;
- resolve action changes reward/progress/completion projection;
- no external execution.

Suggested filter:

```text
FullyQualifiedName~GeneratedMicrogameChallenge
```

## Product smoke

Add scenario:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-microgame-challenge-loop
```

Smoke must prove:

```text
one-click workflow
-> runtime start
-> active goal
-> generated challenge/encounter visible
-> resolve step
-> reward/completion evidence
```

## Docs/state

After S036, update state:

- S036 completed;
- next recommended task is S037 `microgame_acceptance_polish`;
- no broad systems unlocked.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GeneratedMicrogameChallenge"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-microgame-challenge-loop
.\.devflow\scripts\check-all.ps1
```

## Constraints

Do not add LLM/provider/Lua/Unity/media execution.

Do not build a full combat system.

Do not redesign package/runtime broadly.

