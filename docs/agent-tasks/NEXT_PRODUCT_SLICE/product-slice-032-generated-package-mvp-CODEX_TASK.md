# Codex Task - Product Slice 032: Generated Package MVP

## Objective

Implement Product Slice 032: convert the S029-S031 generated procedural pipeline into the first minimal generated package/runtime-facing MVP.

Target pipeline:

```text
S029 ProceduralGeneratedGamePlan
+ S030 FormulaEffectActionRulePack
+ S031 TinyGeneratedRuntimeLoopResult
-> generated package MVP artifact
-> existing validation/runtime bootstrap evidence
```

## Cleanup gate first

Before implementation:

1. Fix `docs/CURRENT_GENERATOR_STATE.md`
   - Remove completed S030/S031 from active `Allowed next sequence`.
   - Keep active next work as Generated Package MVP / S032.

2. Fix `.devflow/scripts/run-product-smoke.ps1`
   - Add scenario `formula-effect-action-registry`.
   - Map it to `FullyQualifiedName~FormulaEffectActionRegistryProductSmoke`.
   - Summary `package_json_path` for this scenario should point to `.llmgc/procedural/formula-effect-action-rule-pack.json`.

3. Ensure `docs/CONTEXT_INDEX.md` and `docs/CURRENT_GENERATOR_STATE.json` consistently reference completed S029/S030/S031 docs if present and point next work to S032.

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario formula-effect-action-registry
```

Proceed only after this gate passes.

## Implement S032

Add Application-layer generated package MVP service.

Suggested files:

- `src/LLMGameCreator.Application/Generation/Procedural/GeneratedPackageMvpModels.cs`
- `src/LLMGameCreator.Application/Generation/Procedural/GeneratedPackageMvpService.cs`
- `src/LLMGameCreator.Application/Generation/Procedural/GeneratedPackageMvpMarkdownRenderer.cs`

The service should produce deterministic sidecars under:

```text
.llmgc/procedural/generated-package-mvp/
```

Expected outputs:

- `package.json`
- `generated-package-mvp-report.json`
- `generated-package-mvp-report.md`
- optional runtime/bootstrap report JSON/Markdown if useful.

## Mapping requirements

Inspect existing `GamePackage` models, validators, samples and runtime bootstrap paths before implementing.

Use existing package contracts as much as possible.

Map at least:

- deterministic package id/title from seed/mode;
- generated regions into map/location data where supported;
- generated actor/encounter into the closest existing supported structure;
- generated item/resource into the closest existing supported structure;
- generated quest/event/progression hook into the closest existing supported structure;
- source metadata for plan hash, rule pack hash and tiny-loop state hash.

If current package contracts cannot represent a concept without broad schema changes, do not redesign schema. Add deterministic diagnostics and map the safe subset.

## Validation/runtime evidence

Run existing package validation if available and include issues in report.

Use existing headless runtime/bootstrap/simulator APIs if possible to prove at least one runtime-facing operation:

- package load/bootstrap;
- inspect initial state;
- enter/move to first location;
- execute one existing minimal command.

If this cannot be done without public runtime contract changes, report the exact blocker and keep package generation/validation deterministic.

## Constraints

Do not add:

- LLM/provider execution;
- Lua execution;
- Unity execution;
- media generation;
- UI;
- broad GamePackage schema changes;
- broad runtime command/state contract redesign;
- C# code generation for mechanics.

## Tests and smoke

Add:

- `GeneratedPackageMvpServiceTests`
- `GeneratedPackageMvpProductSmoke`

Product smoke scenario:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-mvp
```

The smoke should run:

```text
S029 plan -> S030 rule pack -> S031 tiny loop -> S032 generated package MVP
```

## Docs/state update

Update:

- `README.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`

After S032, recommended next work item:

```text
visible_generated_playable_preview
```

M5/Lua/Unity/provider/media/full runtime expansion remain locked unless explicitly approved by the user.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario formula-effect-action-registry
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GeneratedPackageMvp"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-mvp
.\.devflow\scripts\check-all.ps1
```

## Completion report

Report:

- cleanup fixes completed;
- files changed;
- generated sidecar paths;
- validation/runtime-bootstrap evidence;
- verification commands and results;
- whether `check-all.ps1` passed;
- confirmation that no LLM/provider/Lua/Unity/media execution, UI, broad schema redesign or broad runtime contract redesign was introduced.

