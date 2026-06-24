# Product Slice 032 - Generated Package MVP

## Purpose

Product Slice 032 must convert the generated procedural pipeline into the first minimal package/runtime-facing MVP.

The goal is not a full game and not UI polish. The goal is:

```text
Slice 029 generated plan
+ Slice 030 validated formula/effect/action rule pack
+ Slice 031 tiny runtime loop result
-> minimal generated GamePackage MVP artifact
-> existing validation/runtime bootstrap path
-> deterministic reports
```

This slice should prove that generated content can cross from procedural sidecars into the project package/runtime world without a broad schema redesign.

## Files to delete before starting

Delete these only if present in the repository working tree:

- root `README_SLICE_029_TASK.md`
- root `README_SLICE_030_TASK.md`
- root `README_SLICE_031_TASK.md`
- root `README_APPLY_AGENT_TASK_PACK_*.md`
- root `README_APPLY_PRODUCT_SLICE_*.md`
- root `README_APPLY_PACK_008.md`
- root `README_APPLY_CAPABILITY_COMPOSER_V2_PACK.md`
- root `LLMGameCreator_slice*_task.zip`
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_CODEX_PROMPT.md` for slices before 029
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_KILO_PROMPT.md`
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_ARCHIVE_MANIFEST.md`
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_README_APPLY_PRODUCT_SLICE.md`

Do not delete current source-of-truth docs or completed S029-S031 task docs before this slice:

- `docs/NEXT_PRODUCT_SLICE_029_SEEDED_PROCEDURAL_GAME_KERNEL_TASK.md`
- `docs/NEXT_PRODUCT_SLICE_030_FORMULA_EFFECT_ACTION_REGISTRY_TASK.md`
- `docs/NEXT_PRODUCT_SLICE_031_TINY_GENERATED_RUNTIME_LOOP_TASK.md`
- `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`

## First gate: cleanup/fix current handoff issues

Before implementing the MVP, repair these known small issues from S031:

1. `docs/CURRENT_GENERATOR_STATE.md`
   - In `Allowed next sequence`, remove already completed S030 and S031 from the active future sequence.
   - It should list only `Generated Package MVP` or the current S032 wording.

2. `.devflow/scripts/run-product-smoke.ps1`
   - Add a route for:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario formula-effect-action-registry
```

   - It should map to `FullyQualifiedName~FormulaEffectActionRegistryProductSmoke`.
   - In smoke summary path selection, for this scenario point `package_json_path` to:

```text
.llmgc/procedural/formula-effect-action-rule-pack.json
```

3. `docs/CONTEXT_INDEX.md` and `docs/CURRENT_GENERATOR_STATE.json`
   - Ensure the handoff references completed S029, S030 and S031 docs consistently if those docs exist.
   - Ensure the active next work item is S032 / `generated_package_mvp`.

4. Run docs/current-state tests after cleanup:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Do not start S032 implementation until this gate passes.

## Functional goal

Add a deterministic Application-layer service that produces a minimal generated package MVP from the S029-S031 pipeline.

Suggested namespace/path:

`src/LLMGameCreator.Application/Generation/Procedural`

Suggested files:

- `GeneratedPackageMvpModels.cs`
- `GeneratedPackageMvpService.cs`
- `GeneratedPackageMvpMarkdownRenderer.cs`

The service should consume:

- `ProceduralGeneratedGamePlan`
- `FormulaEffectActionRulePack`
- `FormulaEffectActionValidationReport`
- `TinyGeneratedRuntimeLoopResult` or state/report output

The service should produce deterministic sidecars:

- `.llmgc/procedural/generated-package-mvp/package.json`
- `.llmgc/procedural/generated-package-mvp/generated-package-mvp-report.json`
- `.llmgc/procedural/generated-package-mvp/generated-package-mvp-report.md`
- optional, if useful: `.llmgc/procedural/generated-package-mvp/runtime-bootstrap-report.json`
- optional, if useful: `.llmgc/procedural/generated-package-mvp/runtime-bootstrap-report.md`

Keep exact names only if they fit existing path conventions. If a stronger existing convention exists, follow it and document the choice.

## Package mapping requirements

Inspect existing `GamePackage` models, validators, samples and runtime bootstrap paths before designing new models.

The generated package must use existing package contracts as much as possible.

Minimum mapping:

- package id/title derived deterministically from seed/mode;
- at least two generated regions represented as map/location data if existing contracts support it;
- at least one generated player/start state or equivalent package-supported start metadata;
- at least one generated actor/NPC/encounter represented in the closest existing package-supported structure;
- at least one generated item/resource represented in the closest existing package-supported structure;
- at least one generated quest/event/progression hook represented in the closest existing package-supported structure;
- generation metadata linking back to:
  - plan id/hash;
  - rule pack id/hash;
  - tiny loop state hash;
  - source seed/mode.

If an exact generated concept cannot be represented by the current package schema, do not expand the schema broadly. Put the unmapped concept into deterministic report diagnostics and map the closest safe subset.

## Validation/runtime requirements

The generated package MVP must be validated through existing validation code.

Required behavior:

- run existing package validator if available;
- include validation issues in report JSON/Markdown;
- package output should be validator-clean if current contracts can support the minimal subset;
- if full validator-clean output is blocked by existing schema limitations, report exact blockers and keep the mapped subset deterministic.

Runtime/bootstrap requirement:

- use existing headless runtime/bootstrap/simulator services if possible without changing public runtime contracts;
- prove at least one runtime-facing operation, such as load/bootstrap package, inspect initial state, move/enter first location, or execute a minimal existing command;
- write runtime/bootstrap diagnostics to the MVP report.

If existing runtime APIs cannot execute a command against the generated package without broad contract changes, do not redesign runtime in this slice. Instead, implement the narrowest adapter/report showing the exact missing contract and keep the generated package validator-clean.

## Architecture constraints

Do not add:

- LLM/provider execution;
- Lua execution;
- Unity execution;
- media generation;
- UI;
- broad GamePackage schema changes;
- broad runtime command/state contract redesign;
- C# code generation for mechanics.

Allowed:

- small Application-layer mapping service;
- small helper methods for deterministic package construction;
- focused validator/runtime bootstrap adapter only if it uses existing contracts;
- focused tests and one product smoke route.

## Determinism requirements

For identical inputs, outputs must be byte-stable after repository-normalized paths.

Avoid:

- current time;
- random GUIDs;
- machine names;
- absolute local paths inside artifacts;
- nondeterministic dictionary iteration;
- culture-sensitive formatting.

All collections written to JSON/Markdown must be explicitly ordered.

## Tests

Add focused tests under:

`tests/LLMGameCreator.Tests/Application/Procedural`

Suggested test file:

`GeneratedPackageMvpServiceTests.cs`

Required coverage:

- same S029-S031 pipeline input produces byte-identical package/report outputs;
- generated package contains deterministic package metadata linking plan/rule-pack/tiny-loop hashes;
- package includes at least one mapped region/location, item/resource, encounter/actor and quest/event/progression element where current contracts support them;
- validation report is included and deterministic;
- missing/unmappable source concepts produce diagnostics instead of unhandled exceptions;
- no external execution is reported.

Add product smoke coverage under:

`tests/LLMGameCreator.Tests/ProductSmoke`

Suggested test file:

`GeneratedPackageMvpSmokeTests.cs`

The smoke should run:

```text
S029 plan -> S030 rule pack -> S031 tiny loop -> S032 generated package MVP
```

and verify expected sidecars exist.

## Devflow and docs updates

Update:

- `.devflow/scripts/run-product-smoke.ps1`
- `README.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`

Add product smoke scenario:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-mvp
```

It should map to:

```text
FullyQualifiedName~GeneratedPackageMvpProductSmoke
```

After S032, `CURRENT_GENERATOR_STATE` should state that:

- generated plan/rules/tiny-loop can produce a minimal generated package MVP artifact;
- validation/runtime-bootstrap evidence exists or exact blockers are documented;
- M5/Lua/full runtime expansion remains locked unless explicitly approved;
- next recommended work is the smallest visible playable preview step, not infrastructure polish.

Recommended next work item after S032:

```text
visible_generated_playable_preview
```

Acceptable wording:

```text
Visible Generated Playable Preview: expose the generated package MVP through the smallest existing preview/simulator path needed for a user-visible 5-minute prototype, without unlocking broad UI or Unity work.
```

## Verification commands

Run in this order:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario formula-effect-action-registry
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GeneratedPackageMvp"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-mvp
.\.devflow\scripts\check-all.ps1
```

If `check-all.ps1` cannot be run, state the exact reason and list the narrower checks that passed. Do not report the slice as fully accepted without `check-all.ps1` or an explicit gap.

## Acceptance criteria

Slice 032 is acceptable only if:

- S031 handoff cleanup is complete;
- `formula-effect-action-registry` smoke route works;
- generated package MVP sidecars are produced deterministically;
- package mapping uses existing contracts as much as possible;
- package validation/runtime-bootstrap evidence is present in the report;
- targeted tests pass;
- product smoke `generated-package-mvp` passes;
- full `check-all.ps1` passes or the gap is clearly reported;
- no LLM/provider/Lua/Unity/media execution is introduced;
- no broad schema/runtime redesign is introduced;
- next state points to a visible playable preview step.

## Non-goals

Do not build a full game.

Do not implement a new UI.

Do not unlock Unity work.

Do not unlock Lua execution.

Do not add asset/media generation.

Do not rewrite GamePackage schema broadly.

Do not build external map/OSM support.

Do not create a general ECS/runtime framework.

Do not add large unrelated test suites.

