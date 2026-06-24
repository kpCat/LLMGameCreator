# Goal 001 - First 5-Minute Generated Microgame Loop

## Goal

Turn the current one-click generated preview into the first small generated microgame loop.

Current proven state:

```text
S029 generated plan
S030 rule pack
S031 tiny runtime loop
S032 generated package MVP
S033 visible generated playable preview
S034 one-click Generate Preview workflow
S034 hotfix: one-click preview works manually after UI-thread fix
```

Target outcome:

```text
User clicks Generate Preview
-> generated package loads
-> user clicks Start
-> user sees a clear active goal
-> user can interact with NPC/object
-> user can collect/use a generated item or resolve a generated obstacle
-> quest/progress state updates visibly
-> reward/completion condition is visible
-> this can be manually played for roughly 5 minutes
```

This goal is bounded to at most three product slices.

## Hard limit

Maximum implementation slices under this goal:

1. Product Slice 035 - Active Goal + Quest Progress Loop
2. Product Slice 036 - Encounter/Obstacle + Reward/Completion Loop
3. Product Slice 037 - Microgame Acceptance + Playability Polish

Do not continue past S037 inside this goal.

If a blocker appears, stop and report. Do not invent S038.

## First gate: record manual S034 one-click verification

The user manually verified after S034 hotfix:

- Runtime Preview `Generate Preview` no longer throws cross-thread exception.
- Generated package loads automatically.
- `Start` runs runtime preview.
- Generated map is visible.
- Player movement works.
- Generated interaction/dialogue/item-cache behavior is visible.

Before S035, update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md` if needed

State should record:

```text
manual_one_click_preview_verification: passed
```

Then set the active next work to:

```text
first_5_minute_generated_microgame_loop
```

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Proceed only after this passes.

## Slice sequence

### S035

Use:

`docs/NEXT_PRODUCT_SLICE_035_ACTIVE_GOAL_QUEST_PROGRESS_TASK.md`

Required result:

- generated package starts with a visible active goal;
- generated quest/progress state is visible in Runtime Preview;
- interacting with generated NPC/object can advance goal/progress;
- one-click workflow still works;
- no broad runtime/schema/UI redesign.

Stop after S035 if:

- generated package cannot express quest/progress without broad schema changes;
- Runtime Preview cannot display active goal without significant UI redesign;
- tests or product smoke fail and cannot be repaired locally.

### S036

Use only after S035 passes:

`docs/NEXT_PRODUCT_SLICE_036_ENCOUNTER_REWARD_COMPLETION_TASK.md`

Required result:

- generated loop includes a simple obstacle/encounter;
- player action can resolve it through existing runtime/package concepts;
- reward/progress/completion state is visible;
- no Unity/Lua/media/provider work.

Stop after S036 if:

- completion requires broad runtime contract redesign;
- encounter/reward cannot be represented with current contracts plus narrow adapters;
- manual verification is needed before further implementation.

### S037

Use only after S036 passes:

`docs/NEXT_PRODUCT_SLICE_037_MICROGAME_ACCEPTANCE_POLISH_TASK.md`

Required result:

- one-click generated microgame has a coherent 5-minute loop;
- Runtime Preview gives enough information to play it manually;
- acceptance smoke proves start, movement, interaction, progress, reward/completion evidence;
- docs state next action as manual microgame verification.

## Forbidden work

Do not add:

- LLM/provider execution;
- Lua execution;
- Unity work;
- media generation;
- broad GamePackage schema redesign;
- broad runtime command/state redesign;
- large UI rewrite;
- external maps/OSM;
- C# code generation for mechanics.

Do not spend slices on:

- semantic catalog UI;
- manual import polish;
- archive review polish;
- extra report formats unrelated to the microgame loop.

## Verification spine

After each slice:

```powershell
dotnet test .\LLMGameCreator.sln --filter "<slice-specific-filter>"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario <slice-specific-scenario>
.\.devflow\scripts\check-all.ps1
```

If state docs changed:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

If `check-all.ps1` cannot be run, stop and report exact reason.

## Completion condition for the goal

Goal 001 is complete only if S037 reports:

- one-click generation works;
- runtime preview starts;
- generated active goal is visible;
- generated interaction advances progress;
- generated obstacle/encounter or equivalent challenge is resolvable;
- reward/completion state is visible;
- manual verification doc is updated;
- next state is `manual_microgame_loop_verification`.

If this is not reached in three slices, stop and report what blocked it.

## Expected completion report

Report:

- slices completed;
- files changed per slice;
- product smoke scenarios added;
- verification results;
- whether full `check-all.ps1` passed after each slice;
- remaining manual verification steps;
- any architecture blockers.

