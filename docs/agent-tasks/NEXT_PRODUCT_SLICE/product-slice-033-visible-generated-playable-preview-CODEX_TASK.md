# Codex Task - Product Slice 033: Visible Generated Playable Preview

## Objective

Implement Product Slice 033: expose the S032 generated package MVP through the smallest existing runtime/preview/simulator path needed for a user-visible generated prototype.

Codex must not be required to launch Visual Studio or manually inspect the UI. Implement wiring, tests, smoke and manual verification instructions. The user will do the manual visual check after the slice.

## First gate: S032 content hash/provenance clarification

Fix the S032 provenance/hash ambiguity:

- `GeneratedPackageMvpService` currently stores a package hash in provenance before adding the provenance record, then reports a final package hash after provenance is included.
- Keep deterministic behavior.
- Do not try to create a self-referential final hash in the provenance record.
- Make the meaning explicit as pre-provenance/source-content hash in report/provenance metadata or wording.
- Keep final package hash in the S032 report.
- Add or update tests proving both hashes are deterministic and distinct in meaning.

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GeneratedPackageMvp"
```

Proceed only after this passes.

## Reuse existing preview path

Reuse existing code where possible:

- `src/LLMGameCreator.Application/RuntimePreview/GeneratedPackageRuntimePreviewService.cs`
- `LLMGameCreator.Runtime.DefaultGameRuntime`
- `tests/LLMGameCreator.Tests/ProductSmoke/GeneratedPackageRuntimePreviewSmokeTests.cs`

Do not create a parallel preview architecture unless the existing path is impossible to reuse.

## Implement S033

Add a focused generated playable preview adapter/service.

Suggested files:

- `VisibleGeneratedPlayablePreviewModels.cs`
- `VisibleGeneratedPlayablePreviewService.cs`
- `VisibleGeneratedPlayablePreviewMarkdownRenderer.cs`

Prefer `src/LLMGameCreator.Application/RuntimePreview` if dependency direction allows it. If runtime implementation references cannot live in Application, place the narrow runtime-start adapter in an existing test/runtime-facing project or another appropriate layer without creating dependency cycles.

Expected deterministic outputs:

```text
.llmgc/procedural/visible-generated-playable-preview/visible-generated-playable-preview-snapshot.json
.llmgc/procedural/visible-generated-playable-preview/visible-generated-playable-preview-report.json
.llmgc/procedural/visible-generated-playable-preview/visible-generated-playable-preview-report.md
```

Optional:

```text
.llmgc/procedural/visible-generated-playable-preview/manual-verification.md
```

The service should run:

```text
S029 plan -> S030 rule pack -> S031 tiny loop -> S032 generated package MVP -> S033 preview snapshot
```

The preview snapshot/report must include:

- package id/title;
- start/current map id;
- runtime start success/failure;
- player start position;
- movement or interaction attempt result if safely available;
- generated profile/current scene projection;
- counts and representative ids for regions/NPCs/items/encounters/quests/mechanics/provenance;
- diagnostics/warnings;
- plan/rule-pack/tiny-loop/package hashes.

## Optional UI wiring

If there is already a low-risk WinForms preview/simulator entry point that can load an existing `GamePackageDefinition` or `package.json`, add the smallest wiring needed to make the generated package MVP selectable/openable.

Do not build a new multi-page UI.

Do not require Codex to launch the app.

## Tests and smoke

Add:

- `VisibleGeneratedPlayablePreviewServiceTests`
- `VisibleGeneratedPlayablePreviewProductSmoke`

Smoke scenario:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario visible-generated-playable-preview
```

Update `.devflow/scripts/run-product-smoke.ps1` so the summary path points to the S033 preview snapshot JSON.

## Docs/state update

Update:

- `README.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`

Add a short manual verification doc:

`docs/MANUAL_VISIBLE_GENERATED_PLAYABLE_PREVIEW_CHECK.md`

After S033, recommended next work item:

```text
manual_user_preview_verification
```

Do not recommend another Codex implementation slice until the user manually verifies the generated preview, unless the user explicitly asks for a fix.

## Constraints

Do not add:

- LLM/provider execution;
- Lua execution;
- Unity execution;
- media generation;
- broad GamePackage schema changes;
- broad runtime command/state contract redesign;
- large UI rewrite;
- C# code generation for mechanics.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GeneratedPackageMvp"
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~VisibleGeneratedPlayablePreview"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario visible-generated-playable-preview
.\.devflow\scripts\check-all.ps1
```

Do not launch Visual Studio or manually inspect the UI as part of Codex completion.

## Completion report

Report:

- content-hash/provenance fix;
- files changed;
- generated sidecar paths;
- preview/runtime evidence;
- manual verification doc path;
- verification commands and results;
- whether `check-all.ps1` passed;
- confirmation that no LLM/provider/Lua/Unity/media execution, broad schema redesign, broad runtime redesign or large UI rewrite was introduced.

