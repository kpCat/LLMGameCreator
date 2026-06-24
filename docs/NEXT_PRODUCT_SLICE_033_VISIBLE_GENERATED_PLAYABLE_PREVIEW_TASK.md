# Product Slice 033 - Visible Generated Playable Preview

## Purpose

Product Slice 033 must expose the generated package MVP from Slice 032 through the smallest existing preview/simulator/runtime path needed for a user-visible prototype.

The goal is not full game polish, not Unity, and not a new UI subsystem. The goal is to make the generated package MVP usable by the existing preview/runtime projection path so the user can manually launch Visual Studio and inspect/play a minimal generated preview after Codex finishes.

Target pipeline:

```text
S029 ProceduralGeneratedGamePlan
+ S030 FormulaEffectActionRulePack
+ S031 TinyGeneratedRuntimeLoopResult
+ S032 GeneratedPackageMvp package.json
-> existing runtime start / preview projection path
-> deterministic preview snapshot/report sidecars
-> user can manually inspect visible generated preview
```

## Important manual verification boundary

Do not require Codex to launch Visual Studio, run the WinForms app interactively, or perform manual visual inspection.

Codex should implement code, tests, smoke routes, deterministic preview artifacts and clear manual verification instructions.

The user will manually run the app/preview after the task if needed.

## Files to delete before starting

Delete these only if present in the repository working tree:

- root `README_SLICE_029_TASK.md`
- root `README_SLICE_030_TASK.md`
- root `README_SLICE_031_TASK.md`
- root `README_SLICE_032_TASK.md`
- root `README_APPLY_AGENT_TASK_PACK_*.md`
- root `README_APPLY_PRODUCT_SLICE_*.md`
- root `README_APPLY_PACK_008.md`
- root `README_APPLY_CAPABILITY_COMPOSER_V2_PACK.md`
- root `LLMGameCreator_slice*_task.zip`
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_CODEX_PROMPT.md` for slices before 029
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_KILO_PROMPT.md`
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_ARCHIVE_MANIFEST.md`
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_README_APPLY_PRODUCT_SLICE.md`

Do not delete current source-of-truth docs or completed S029-S032 task docs before this slice:

- `docs/NEXT_PRODUCT_SLICE_029_SEEDED_PROCEDURAL_GAME_KERNEL_TASK.md`
- `docs/NEXT_PRODUCT_SLICE_030_FORMULA_EFFECT_ACTION_REGISTRY_TASK.md`
- `docs/NEXT_PRODUCT_SLICE_031_TINY_GENERATED_RUNTIME_LOOP_TASK.md`
- `docs/NEXT_PRODUCT_SLICE_032_GENERATED_PACKAGE_MVP_TASK.md`
- `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`

## First gate: S032 content hash/provenance cleanup

Before implementing visible preview wiring, address the S032 provenance/hash ambiguity:

Current concern:

- `GeneratedPackageMvpService` adds a `GeneratedContentArtifactProvenance` record using a package hash computed before the provenance record is added.
- `GeneratedPackageMvpReport.PackageHash` is then computed from the final package JSON after provenance is added.
- This can be valid to avoid a self-referential hash, but the meaning should be explicit.

Required fix:

- Keep deterministic behavior.
- Do not try to make a self-referential final package hash inside the provenance record.
- Rename/report/document the provenance hash meaning as a pre-provenance or content-source hash, for example:
  - `pre_provenance_package_hash`;
  - `source_content_hash`;
  - or a clearly documented equivalent using existing fields/metadata.
- Ensure the final report still contains the final package hash.
- Add/adjust tests so the distinction is clear and deterministic.

Avoid broad GamePackage schema changes for this. Prefer provenance metadata/report wording if existing contracts do not have a dedicated field.

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GeneratedPackageMvp"
```

Proceed only after the targeted tests pass.

## Existing path to reuse

There is already a precedent:

- `tests/LLMGameCreator.Tests/ProductSmoke/GeneratedPackageRuntimePreviewSmokeTests.cs`
- `src/LLMGameCreator.Application/RuntimePreview/GeneratedPackageRuntimePreviewService.cs`
- `LLMGameCreator.Runtime.DefaultGameRuntime`

S033 should reuse this path for the generated S032 package rather than inventing a parallel preview architecture.

## Functional goal

Add a generated playable preview service or adapter that:

- runs the S029-S032 pipeline;
- starts the generated package through the existing runtime path where possible;
- builds `GeneratedPackageRuntimePreviewService` projection for the generated package;
- executes at least one minimal runtime command if the existing runtime supports it for the generated package;
- writes deterministic preview snapshot/report sidecars;
- provides manual verification instructions for the user.

Suggested namespace/path:

`src/LLMGameCreator.Application/RuntimePreview`

or, if existing organization clearly prefers it:

`src/LLMGameCreator.Application/Generation/Procedural`

Suggested files:

- `VisibleGeneratedPlayablePreviewModels.cs`
- `VisibleGeneratedPlayablePreviewService.cs`
- `VisibleGeneratedPlayablePreviewMarkdownRenderer.cs`

If the service must reference `LLMGameCreator.Runtime`, place it in a project that already references runtime, or add the narrowest safe project reference only if it fits existing dependency direction. Do not create a dependency cycle.

## Output sidecars

Write deterministic artifacts under:

```text
.llmgc/procedural/visible-generated-playable-preview/
```

Expected outputs:

- `visible-generated-playable-preview-snapshot.json`
- `visible-generated-playable-preview-report.json`
- `visible-generated-playable-preview-report.md`
- optional: `manual-verification.md`

The snapshot/report should include:

- package id/title;
- start map id/current map id;
- runtime start success/failure;
- player start position;
- result of at least one runtime command if executed;
- current scene/profile projection;
- regions/NPCs/items/encounters/quests/mechanics counts;
- representative generated ids visible to the preview;
- warnings/diagnostics;
- source hashes:
  - plan hash;
  - rule pack hash;
  - tiny loop state hash;
  - generated package final hash.

## Runtime/preview requirements

Required:

- Use the generated package from S032, not a handcrafted baseline package.
- Use existing `GeneratedPackageRuntimePreviewService` projection.
- Use existing `DefaultGameRuntime` or existing runtime abstractions if project dependencies allow it.
- Prove at least:
  - generated package starts in runtime, or exact blocker is diagnosed;
  - preview projection has non-empty generated profile/scene/regions/quests/mechanics;
  - a movement or inspect/interaction step is attempted when safe.

If the generated package cannot run through `DefaultGameRuntime` without broad schema/runtime changes:

- Do not redesign runtime broadly.
- Add diagnostics explaining the exact blocker.
- Still produce a deterministic preview projection from the package.
- Keep the product smoke focused on the strongest existing path.

## Optional UI wiring, no manual launch

If there is already a low-risk existing WinForms preview/simulator entry point that can load a `GamePackageDefinition` or a `package.json`, add the minimal wiring needed for the generated package MVP to be selected or opened.

Do not build a new large UI.

Do not require Codex to launch Visual Studio or the WinForms app.

Any UI wiring must be small, testable where practical, and documented for manual user verification.

Acceptable examples:

- add a generated preview command/service exposed to an existing preview presenter;
- add a deterministic load path for `.llmgc/procedural/generated-package-mvp/package.json` if a package-load command already exists;
- add a small manual verification section in docs explaining what to click/run.

Non-acceptable examples:

- new multi-page UI;
- Unity preview;
- asset/media generation;
- manual visual assertions by Codex.

## Architecture constraints

Do not add:

- LLM/provider execution;
- Lua execution;
- Unity execution;
- media generation;
- broad GamePackage schema changes;
- broad runtime command/state contract redesign;
- C# code generation for mechanics;
- large UI rewrite.

Allowed:

- focused preview adapter/service;
- narrow reuse of existing runtime/preview services;
- small dependency adjustment only if consistent with existing project direction;
- focused tests and one product smoke route;
- docs/manual verification instructions.

## Tests

Add focused tests under one or both of:

- `tests/LLMGameCreator.Tests/Application/RuntimePreview`
- `tests/LLMGameCreator.Tests/Application/Procedural`

Suggested test file:

`VisibleGeneratedPlayablePreviewServiceTests.cs`

Required coverage:

- same S029-S032 pipeline input produces byte-identical preview snapshot/report outputs;
- preview projection contains generated package title/profile/current scene;
- projection exposes generated regions, quests, mechanics and provenance;
- runtime start succeeds if supported by existing runtime path;
- at least one movement/interaction attempt is represented in the report;
- if runtime command execution is blocked, the blocker is deterministic and explicit;
- S032 content-hash/provenance distinction is tested;
- no external execution is reported.

Add product smoke coverage:

`tests/LLMGameCreator.Tests/ProductSmoke/VisibleGeneratedPlayablePreviewSmokeTests.cs`

Product smoke scenario:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario visible-generated-playable-preview
```

The smoke should run:

```text
S029 plan -> S030 rule pack -> S031 tiny loop -> S032 generated package MVP -> S033 preview snapshot
```

and verify expected sidecars exist.

## Devflow and docs updates

Update:

- `.devflow/scripts/run-product-smoke.ps1`
- `README.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`

After S033, `CURRENT_GENERATOR_STATE` should state:

- visible generated playable preview path exists;
- whether runtime start/command execution succeeds or which exact blocker remains;
- user manual verification is the next human action;
- M5/Lua/Unity/provider/media/full runtime expansion remains locked unless explicitly approved.

Recommended next work item after S033:

```text
manual_user_preview_verification
```

Acceptable wording:

```text
Manual User Preview Verification: user launches the app/preview from Visual Studio and verifies the generated package MVP is visible and minimally playable before Codex receives more feature slices.
```

Do not recommend another Codex implementation slice until manual preview verification is complete, unless the user explicitly asks for a fix task.

## Manual verification instructions

Add a short doc or section, for example:

`docs/MANUAL_VISIBLE_GENERATED_PLAYABLE_PREVIEW_CHECK.md`

It should tell the user:

- where generated preview artifacts are written;
- which smoke command produces them;
- which app/preview/simulator path to open manually;
- what to check visually:
  - generated package title visible;
  - generated map/scene visible;
  - player can at least move or inspect one generated object if supported;
  - generated quest/encounter/mechanic/provenance visible in the existing preview/projection;
  - no LLM/provider/Lua/Unity/media execution is required.

Keep this doc short and practical.

## Verification commands for Codex

Codex should run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GeneratedPackageMvp"
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~VisibleGeneratedPlayablePreview"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario visible-generated-playable-preview
.\.devflow\scripts\check-all.ps1
```

Codex should not be required to launch Visual Studio or manually inspect the WinForms UI.

If `check-all.ps1` cannot be run, state the exact reason and list the narrower checks that passed.

## Acceptance criteria

S033 is acceptable only if:

- S032 content-hash/provenance ambiguity is clarified and tested;
- generated package MVP is consumed by the preview/runtime path;
- deterministic preview sidecars are produced;
- product smoke `visible-generated-playable-preview` passes;
- full `check-all.ps1` passes or an exact gap is reported;
- docs include manual verification instructions;
- state points to manual user preview verification, not another automatic feature slice;
- no LLM/provider/Lua/Unity/media execution is introduced;
- no broad schema/runtime/UI redesign is introduced.

## Non-goals

Do not implement a full game.

Do not implement Unity work.

Do not unlock Lua execution.

Do not add provider/media generation.

Do not build a new large UI.

Do not require Codex to run Visual Studio.

Do not require Codex to visually verify the app.

Do not implement external map/OSM support.

Do not create a general ECS/runtime framework.

