# Product Slice 035 - Active Goal + Quest Progress Loop

## Purpose

Make the generated preview feel like a small game with a clear active goal and visible progress.

Current S034 preview works technically, but the user still has to infer purpose from debug logs and generated content. S035 should add a playable goal/progress layer using existing package/runtime/preview concepts.

## Functional goal

When the user clicks `Generate Preview` and then `Start`, Runtime Preview should show:

- a clear active generated goal;
- current objective text;
- related generated NPC/object/item;
- progress status;
- what player action advanced the goal.

This must be generated from the S029-S034 pipeline, not hardcoded as a one-off sample.

## Implementation direction

Use existing contracts first:

- generated quests;
- generated dialogues;
- generated interactions;
- generated item/resource seeds;
- runtime quest/progress services if already available;
- Runtime Preview generated-content/quest journal panels.

Add the narrowest mapping/adapters needed to connect:

```text
generated quest/event seed
-> package quest/objective
-> preview active goal
-> interaction/dialogue/item action
-> visible progress update
```

## Required behavior

Minimum acceptable behavior:

- one generated quest is selected as active at runtime start;
- active quest title/objective is visible in Runtime Preview;
- generated NPC/object/item connected to the quest is visible;
- interacting with the connected entity records progress visibly;
- generated-content summary includes active goal/progress counts;
- logs use readable labels, not only internal ids.

If current runtime cannot truly mutate quest progress from interactions without broad redesign, add a narrow preview-level progress tracker and explicitly diagnose the limitation. Do not redesign runtime broadly in S035.

## Suggested files

Prefer small additions near existing preview services:

- `src/LLMGameCreator.Application/RuntimePreview/GeneratedMicrogameGoalPreviewService.cs`
- optional models/renderer if needed.

Touch WinForms only where necessary:

- `RuntimePreviewPageControl.cs`
- designer only for small controls/labels if needed.

## Tests

Add focused tests:

- active generated goal is selected deterministically;
- active objective references generated quest/NPC/item/scene;
- interaction/progress event changes visible preview state or preview tracker state;
- one-click workflow still returns generated package and preview data;
- no external execution.

Suggested filter:

```text
FullyQualifiedName~GeneratedMicrogameGoal
```

## Product smoke

Add scenario:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-microgame-goal-loop
```

Smoke must run:

```text
one-click workflow
-> runtime start
-> active goal projection
-> one interaction/progress step
```

No WinForms launch required.

## Docs/state

Update current state after completion:

- S035 completed;
- next recommended task is S036 `encounter_reward_completion_loop`;
- infrastructure-only work remains frozen.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GeneratedMicrogameGoal"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-microgame-goal-loop
.\.devflow\scripts\check-all.ps1
```

## Constraints

Do not add LLM/provider/Lua/Unity/media execution.

Do not perform broad schema/runtime/UI redesign.

