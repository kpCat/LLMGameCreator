# Product Slice 039 - Runtime-Backed Reward/Challenge/Completion State

## Purpose

Move generated reward/challenge/completion evidence from preview projection toward runtime-owned state.

S038 handles runtime-owned goal progress. S039 must add runtime-backed reward/challenge/completion evidence where current contracts allow.

## Functional goal

After one-click generation and Runtime Preview start:

- player can trigger generated interaction/challenge;
- runtime-owned state records challenge resolution or equivalent generated flag;
- runtime-owned state records reward evidence where possible;
- completion state is derived from runtime-owned evidence;
- Runtime Preview displays reward/challenge/completion as runtime-backed.

## Implementation direction

Use existing runtime state first:

- inventory/item state;
- resource state;
- flags/variables/status if present;
- quest/objective state;
- interaction runtime events;
- encounter runtime events;
- runtime snapshot serialization.

Allowed narrow extension:

- a small generated microgame state block in runtime state if no existing state can represent the required evidence.

Forbidden:

- new combat system;
- broad encounter runtime redesign;
- broad package schema redesign.

## Required behavior

Minimum acceptable behavior:

- challenge selected deterministically;
- interaction or existing command resolves challenge;
- reward evidence is runtime-owned, for example item/resource/flag/progress;
- completion uses runtime-owned goal progress + runtime-owned challenge/reward evidence;
- preview clearly shows runtime-backed status.

If reward/challenge cannot be fully runtime-backed, report exact blocker and implement the smallest truthful fallback with explicit diagnostics.

## Tests

Add focused tests:

- challenge state initializes deterministically;
- interaction records resolution evidence;
- reward evidence is stored in runtime-owned state or explicit narrow state;
- completion derives from state evidence;
- preview displays runtime-backed state.

Suggested filter:

```text
FullyQualifiedName~RuntimeRewardChallengeState
```

## Product smoke

Add scenario:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario runtime-reward-challenge-state
```

Smoke must prove:

```text
one-click workflow
-> runtime start
-> interaction/challenge
-> runtime-owned reward/challenge/completion evidence
-> preview displays evidence
```

## Docs/state

After S039:

- update state;
- next recommended task is S040 `runtime_microgame_state_acceptance`;
- no broad systems unlocked.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~RuntimeRewardChallengeState"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario runtime-reward-challenge-state
.\.devflow\scripts\check-all.ps1
```

## Constraints

Do not add LLM/provider/Lua/Unity/media execution.

Do not build a new combat system.

Do not redesign package/runtime broadly.

