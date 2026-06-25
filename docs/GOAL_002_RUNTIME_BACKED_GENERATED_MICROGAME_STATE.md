# Goal 002 - Runtime-Backed Generated Microgame State

## Why this goal

Goal 001 succeeded: the user manually verified that the one-click generated microgame loop is visible and playable enough in Runtime Preview.

Observed manual evidence:

- Generate Preview works.
- Generated package loads automatically.
- Start runs Runtime Preview.
- Map is visible.
- Player movement works.
- Interactions work.
- Active goal is visible.
- Goal progress reaches `3/3`.
- Challenge is resolved.
- Reward and completion are visible.

However, Goal 001 intentionally used preview-level projection for parts of the microgame:

- generated goal progress is tracked by Runtime Preview quest journal/projection;
- challenge/reward/completion are deterministic preview projections;
- package/runtime contracts were not redesigned.

That was acceptable for Goal 001. It is not enough for the next stable foundation.

Goal 002 must move the generated microgame loop from "preview says it happened" toward "runtime/package state owns it", then expose enough presets/options and seed variation to make the loop useful beyond one generated sample.

## Target outcome

After Goal 002:

```text
Generate Preview
-> Start
-> interact/move
-> runtime state records generated goal progress
-> runtime state records reward/challenge/completion evidence
-> preview renders that runtime-owned state
-> acceptance smoke proves state survives the chosen runtime state path
-> user can choose or run deterministic seed/preset variants
-> variation smoke proves multiple generated microgames differ while remaining playable
```

This does not need to be a full RPG quest system. It must be the narrowest runtime-backed microgame state foundation that avoids broad redesign.

## Hard limit

Maximum implementation slices:

1. Product Slice 038 - Runtime-Owned Generated Goal Progress
2. Product Slice 039 - Runtime-Backed Reward/Challenge/Completion State
3. Product Slice 040 - Runtime Microgame State Acceptance + Save/Reload
4. Product Slice 041 - Generation Presets and Options
5. Product Slice 042 - Microgame Variation Acceptance

Do not continue past S042 inside this goal.

If a blocker appears, stop and report. Do not invent S043.

## First gate: record manual Goal 001 verification and clean state/docs

The user manually verified Goal 001 after S037:

- active goal visible;
- objective/progress visible;
- movement and interaction work;
- challenge resolved;
- reward visible;
- completion visible;
- generated content panel shows the microgame summary.

Before S038, update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md` if needed

State should record:

```text
manual_microgame_loop_verification: passed
```

Then set the active next work to:

```text
runtime_backed_generated_microgame_state
```

Also fix known handoff issues:

- `docs/CURRENT_GENERATOR_STATE.json` currently references `docs/NEXT_PRODUCT_SLICE_035_ACTIVE_GOAL_PROGRESS_TASK.md`; ensure this points to the actual S035 task filename in the repo.
- Record the technical debt that `OneClickGeneratedPreviewWorkflowService` still supports service-side current-package replacement. Do not fix this unless it is tiny and directly relevant; at minimum make sure WinForms still uses caller-deferred replacement.

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Proceed only after this passes.

## Slice sequence

### S038

Use:

`docs/NEXT_PRODUCT_SLICE_038_RUNTIME_OWNED_GOAL_PROGRESS_TASK.md`

Required result:

- generated active goal progress is stored in runtime-owned state or an existing runtime state extension point;
- Runtime Preview reads and displays that runtime-owned state;
- preview-level tracker remains only as fallback/compatibility;
- no broad runtime/schema redesign.

Stop after S038 if:

- no existing runtime state path can represent progress without broad redesign;
- GamePackage schema expansion would be broad;
- tests/smoke/check-all fail and cannot be repaired locally.

### S039

Use only after S038 passes:

`docs/NEXT_PRODUCT_SLICE_039_RUNTIME_REWARD_CHALLENGE_STATE_TASK.md`

Required result:

- generated reward is represented in runtime-owned state where possible;
- generated challenge/completion evidence is represented in runtime-owned state where possible;
- Runtime Preview displays runtime-owned reward/challenge/completion state;
- preview projection does not claim completion without backing evidence, unless explicitly flagged as fallback.

Stop after S039 if:

- reward/challenge state requires broad runtime redesign;
- current runtime state model cannot support any narrow extension.

### S040

Use only after S039 passes:

`docs/NEXT_PRODUCT_SLICE_040_RUNTIME_MICROGAME_STATE_ACCEPTANCE_TASK.md`

Required result:

- generated microgame state acceptance proves start, movement, interaction, progress, reward, challenge/completion;
- chosen runtime state path survives save/reload or runtime snapshot serialization if existing project facilities support it;
- manual verification doc is updated;
- state stops at manual runtime-backed microgame verification.

### S041

Use only after S040 passes:

`docs/NEXT_PRODUCT_SLICE_041_GENERATION_PRESETS_AND_OPTIONS_TASK.md`

Required result:

- one-click preview can use deterministic generation presets/options;
- at minimum seed, mode and one small preset/style choice are exposed through a narrow UI/service path;
- generated package/runtime microgame state still passes acceptance.

Stop after S041 if:

- preset/options work requires broad UI redesign;
- option changes break package/runtime acceptance;
- manual verification is required before continuing.

### S042

Use only after S041 passes:

`docs/NEXT_PRODUCT_SLICE_042_MICROGAME_VARIATION_ACCEPTANCE_TASK.md`

Required result:

- multiple seed/preset variants produce visibly different microgames;
- each accepted variant remains playable enough under headless acceptance;
- deterministic variation report is written;
- state stops at manual configurable microgame verification.

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
- extra report formats unrelated to runtime-backed microgame state.

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

Goal 002 is complete only if S042 reports:

- one-click generation works;
- runtime preview starts;
- generated goal progress is runtime-backed;
- generated reward/challenge/completion has runtime-owned or explicitly validated state evidence;
- state evidence survives the selected save/reload/snapshot path if existing facilities support it;
- generation presets/options exist;
- multiple deterministic seed/preset variants are accepted;
- docs state next action as `manual_configurable_microgame_verification`.

If this is not reached in five slices, stop and report what blocked it.

## Expected completion report

Report:

- slices completed;
- files changed per slice;
- whether state is runtime-backed or fallback-backed for each feature;
- product smoke scenarios added;
- verification results;
- whether full `check-all.ps1` passed after each slice;
- remaining manual verification steps;
- any architecture blockers.
