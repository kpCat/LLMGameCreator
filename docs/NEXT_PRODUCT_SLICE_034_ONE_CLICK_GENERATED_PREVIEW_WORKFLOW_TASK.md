# Product Slice 034 - One-Click Generated Preview Workflow + Playable Preview UX Pass

## Purpose

Product Slice 034 must turn the manually verified S033 pipeline into a usable in-app workflow.

The current state after S033:

- generated package MVP can be produced;
- Runtime Preview can open and run it;
- manual user verification passed;
- but the user still has to run smoke commands, find `.devflow/runs/...`, open the generated package folder manually, and then start Runtime Preview.

S034 should reduce that friction.

Target outcome:

```text
User opens LLMGameCreator
-> clicks one clear generated preview action
-> app runs S029-S033 generation pipeline headlessly
-> generated package is loaded as current project/package
-> Runtime Preview can start immediately
-> generated content panel is easier to inspect
```

This is not a full product UI redesign. This is a narrow workflow and UX pass around the already working generated preview.

## Files to delete before starting

Delete these only if present in the repository working tree:

- root `README_SLICE_029_TASK.md`
- root `README_SLICE_030_TASK.md`
- root `README_SLICE_031_TASK.md`
- root `README_SLICE_032_TASK.md`
- root `README_SLICE_033_TASK.md`
- root `README_APPLY_AGENT_TASK_PACK_*.md`
- root `README_APPLY_PRODUCT_SLICE_*.md`
- root `README_APPLY_PACK_008.md`
- root `README_APPLY_CAPABILITY_COMPOSER_V2_PACK.md`
- root `LLMGameCreator_slice*_task.zip`
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_CODEX_PROMPT.md` for slices before 029
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_KILO_PROMPT.md`
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_ARCHIVE_MANIFEST.md`
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_README_APPLY_PRODUCT_SLICE.md`

Do not delete current source-of-truth docs or completed S029-S033 task docs before this slice:

- `docs/NEXT_PRODUCT_SLICE_029_SEEDED_PROCEDURAL_GAME_KERNEL_TASK.md`
- `docs/NEXT_PRODUCT_SLICE_030_FORMULA_EFFECT_ACTION_REGISTRY_TASK.md`
- `docs/NEXT_PRODUCT_SLICE_031_TINY_GENERATED_RUNTIME_LOOP_TASK.md`
- `docs/NEXT_PRODUCT_SLICE_032_GENERATED_PACKAGE_MVP_TASK.md`
- `docs/NEXT_PRODUCT_SLICE_033_VISIBLE_GENERATED_PLAYABLE_PREVIEW_TASK.md`
- `docs/MANUAL_VISIBLE_GENERATED_PLAYABLE_PREVIEW_CHECK.md`
- `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`

## First gate: record manual S033 verification passed

The user manually verified S033 after running:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario visible-generated-playable-preview
```

Manual observed result:

- generated package opened in WinForms;
- Runtime Preview started;
- generated map was visible;
- player movement worked;
- generated interaction/dialogue/item-cache behavior was visible;
- project status showed a generated MVP package.

Before implementing S034, update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md` if needed

State should record:

- `manual_user_preview_verification`: passed;
- manual verification evidence is from the user after S033;
- next Codex task is now S034 / `one_click_generated_preview_workflow`.

Do not leave state saying manual verification is still pending after this gate.

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Proceed only after docs/current-state tests pass.

## Functional goal

Add a narrow one-click generated preview workflow.

Minimum behavior:

- expose one clear WinForms action to generate/open the visible generated preview;
- run the S029-S033 generation pipeline through existing services;
- write deterministic artifacts under a project/devflow-safe output folder;
- load the generated `GamePackageDefinition` into `ICurrentGamePackageService`;
- make Runtime Preview usable without manually browsing to `.devflow/runs/...`;
- show enough status/diagnostics so the user knows where artifacts were written and whether generation succeeded.

Acceptable UI location:

- `Runtime Preview` page, if it is the least confusing place;
- or `Игры` / Projects page, if it is better for loading the generated package;
- or a small generated-preview action in an existing page if the repo already has a better local pattern.

Do not add a new large page unless the existing layout makes a tiny action impossible.

## UX requirements

This is a debug/playable preview UX pass, not final game UI.

Required improvements:

- Add one visible button/action, for example:
  - `Generate Preview`
  - `Generate & Open Preview`
  - `Сгенерировать preview`
  - `Сгенерировать и открыть`
- Disable the action while generation is running or guard against double-click concurrent runs.
- Show success/failure in a visible status/log area.
- On success, report:
  - generated package title/id;
  - output folder;
  - runtime preview instruction, if runtime is not auto-started.
- On failure, show deterministic diagnostics instead of silent failure.
- Do not block the UI thread for long-running work. Use existing async patterns where available.

Generated content readability pass:

- In `Runtime Preview`, improve generated-content visibility enough for manual use.
- Prefer small changes:
  - clearer generated package summary;
  - counts for regions/NPCs/items/encounters/quests/mechanics;
  - current scene/profile summary;
  - representative generated ids;
  - preserve existing category/list/detail behavior.
- Do not redesign the whole Runtime Preview page.

## Service requirements

Prefer an Application-layer service for the reusable workflow logic.

Suggested files:

- `src/LLMGameCreator.Application/RuntimePreview/OneClickGeneratedPreviewWorkflowModels.cs`
- `src/LLMGameCreator.Application/RuntimePreview/OneClickGeneratedPreviewWorkflowService.cs`
- optional renderer/report helper if needed.

The workflow service should:

- accept seed/mode/style/variant options, with safe defaults;
- run existing `VisibleGeneratedPlayablePreviewService`;
- write S029-S033 artifacts through existing `WriteAsync` methods where useful;
- return:
  - generated package;
  - package output path;
  - visible preview output path;
  - snapshot/report paths;
  - diagnostics;
  - stable summary.

If a WinForms page can directly use `VisibleGeneratedPlayablePreviewService` without creating another service, that is acceptable only if tests remain focused and the page code does not become the core workflow owner.

## Determinism and safety

Avoid:

- timestamps inside deterministic artifacts;
- random GUIDs in deterministic outputs;
- absolute machine-specific paths inside deterministic JSON/Markdown except UI-only status messages;
- concurrent overlapping generation runs;
- LLM/provider/Lua/Unity/media execution.

Generated preview output folders may include a run folder name for UI workflow convenience, but deterministic artifact contents must remain stable for identical inputs.

## Tests

Add focused tests for the workflow service if added.

Suggested test file:

`tests/LLMGameCreator.Tests/Application/RuntimePreview/OneClickGeneratedPreviewWorkflowServiceTests.cs`

Required coverage:

- workflow with default request produces a generated package and visible preview result;
- generated package can be loaded into current-package service or equivalent test seam if available;
- diagnostics are deterministic;
- double-run/concurrency guard behavior is covered if implemented outside UI;
- no external execution is reported;
- generated package title/id and output paths are returned.

Add or adjust WinForms/presenter tests only if there is already an established test pattern for the touched page.

Do not add brittle UI pixel tests.

## Product smoke

Add product smoke scenario:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario one-click-generated-preview-workflow
```

The smoke should exercise the workflow service headlessly:

```text
one-click workflow service
-> visible generated playable preview pipeline
-> generated package returned
-> package can be set/loaded as current package through an application seam or equivalent verification
-> artifacts exist
```

Do not require launching WinForms in product smoke.

Update `.devflow/scripts/run-product-smoke.ps1` summary path to point at the most useful artifact, likely:

```text
.llmgc/procedural/visible-generated-playable-preview/visible-generated-playable-preview-snapshot.json
```

or a new one-click workflow report if added.

## Docs updates

Update:

- `README.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/MANUAL_VISIBLE_GENERATED_PLAYABLE_PREVIEW_CHECK.md`

Manual verification doc should be updated so the user no longer needs to browse `.devflow/runs/...` manually as the primary path.

It should include:

- the new one-click action location;
- what button to press;
- what success looks like;
- what to do if generation fails;
- where artifacts are written.

## Verification commands

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~OneClickGeneratedPreviewWorkflow"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario one-click-generated-preview-workflow
.\.devflow\scripts\check-all.ps1
```

Codex should not launch Visual Studio or manually inspect the UI. The user will manually test the button after the slice.

If `check-all.ps1` cannot be run, state the exact reason and list the narrower checks that passed.

## Acceptance criteria

S034 is acceptable only if:

- manual S033 verification is recorded as passed;
- one clear in-app generated preview action exists;
- generated preview can be produced/opened without manually browsing to `.devflow/runs/...`;
- current package is set to the generated MVP package after successful workflow execution;
- Runtime Preview can use the generated package after the workflow;
- generated content readability is improved modestly;
- product smoke `one-click-generated-preview-workflow` passes;
- full `check-all.ps1` passes or an exact gap is reported;
- no LLM/provider/Lua/Unity/media execution is introduced;
- no broad GamePackage schema/runtime/UI redesign is introduced.

## Recommended next state after S034

After S034, do not automatically recommend a new feature slice.

Recommended next work item:

```text
manual_one_click_preview_verification
```

Acceptable wording:

```text
Manual One-Click Preview Verification: user launches WinForms, presses the new generated-preview action, verifies the package loads automatically, Runtime Preview starts/works, and generated content is readable enough before Codex receives another feature slice.
```

## Non-goals

Do not build final game UI.

Do not implement Unity.

Do not unlock Lua execution.

Do not call providers or LLMs.

Do not generate media.

Do not redesign the full editor shell.

Do not add a new broad project format.

Do not implement external maps/OSM.

Do not make Codex perform manual Visual Studio UI verification.

