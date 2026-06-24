# Codex Task - Product Slice 034: One-Click Generated Preview Workflow

## Objective

Implement Product Slice 034: a narrow one-click in-app workflow that generates and opens the S033 visible generated playable preview without requiring the user to manually browse `.devflow/runs/...`.

Codex must not launch Visual Studio or manually inspect the UI. Implement code, tests, smoke and docs. The user will manually test the new button/action after completion.

## First gate: record manual S033 verification passed

The user manually verified S033 in WinForms:

- generated package opened;
- Runtime Preview started;
- generated map was visible;
- player movement worked;
- generated interaction/dialogue/item-cache behavior was visible.

Update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md` if needed

State should no longer say manual preview verification is pending. It should record S033 manual verification as passed and set current Codex work to S034 / `one_click_generated_preview_workflow`.

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Proceed only after this passes.

## Implement S034

Add one clear generated preview action in WinForms.

Acceptable locations:

- `Runtime Preview` page;
- or `Игры` / Projects page;
- or another existing page if clearly better.

Expected behavior:

```text
Click Generate/Open Preview
-> run S029-S033 pipeline
-> write artifacts
-> load generated GamePackage as current package
-> make Runtime Preview ready to start/use
-> show status/output folder/diagnostics
```

Prefer a reusable application service:

- `OneClickGeneratedPreviewWorkflowModels.cs`
- `OneClickGeneratedPreviewWorkflowService.cs`

Use existing:

- `VisibleGeneratedPlayablePreviewService`
- `ICurrentGamePackageService`
- existing project/package save/load patterns
- existing Runtime Preview page/presenter patterns

Do not create a broad new UI.

## UX requirements

- One visible button/action.
- Guard against double-click/concurrent generation.
- Async workflow; avoid UI freeze.
- Success status includes generated package title/id and output folder.
- Failure status includes deterministic diagnostics.
- Current package is set to the generated MVP package after success.
- Runtime Preview generated content display gets a small readability improvement:
  - package/profile/current scene summary;
  - counts for regions/NPCs/items/encounters/quests/mechanics;
  - representative generated ids where useful.

## Product smoke

Add:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario one-click-generated-preview-workflow
```

Smoke should exercise the headless workflow service and verify:

- generated package exists;
- visible preview artifacts exist;
- generated package is returned/loaded through a current-package seam or equivalent application verification;
- no external execution.

Do not launch WinForms in smoke.

## Docs

Update:

- `README.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/MANUAL_VISIBLE_GENERATED_PLAYABLE_PREVIEW_CHECK.md`

Manual doc should now explain the new button/action path as the primary path.

After S034, recommended next work item:

```text
manual_one_click_preview_verification
```

Do not recommend another Codex feature slice until the user manually verifies the new workflow.

## Constraints

Do not add:

- LLM/provider execution;
- Lua execution;
- Unity execution;
- media generation;
- broad GamePackage schema changes;
- broad runtime command/state redesign;
- large UI rewrite.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~OneClickGeneratedPreviewWorkflow"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario one-click-generated-preview-workflow
.\.devflow\scripts\check-all.ps1
```

Do not launch Visual Studio or perform manual UI verification as Codex.

## Completion report

Report:

- manual S033 verification recorded;
- files changed;
- new button/action location;
- generated artifacts/output paths;
- smoke scenario added;
- verification commands and results;
- whether `check-all.ps1` passed;
- confirmation that no LLM/provider/Lua/Unity/media execution, broad schema redesign, broad runtime redesign or large UI rewrite was introduced.

