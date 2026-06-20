# Product Slice 008 Task: Active Generated Package Flow + Quest/Dialogue Preview Stubs

## Task type

Large bounded product slice.

This task has a mandatory repair block first, then a small gameplay-preview feature block.

## Goal

Fix the package assembly -> current package -> Runtime Preview seam, then add preview-only quest/dialogue interaction stubs.

Current problem found manually:

```text
Artifact Review / Save decisions + apply creates:
.llmgc/package-assembly/package.json

But Runtime Preview still starts:
project-root package.json

Result:
generatedContent appears empty unless the user manually copies package.json.
```

This must be fixed as part of the slice.

## Recommended Codex reasoning level

High.

Do not use Max/Ultra on first attempt.
Do not use Medium because this crosses Artifact Review, package assembly output, current package service, Runtime Preview, UI and smoke tests.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/PRODUCT_SLICE_008_ACTIVE_GENERATED_PACKAGE_FLOW_QUEST_DIALOGUE_PREVIEW.md

src/LLMGameCreator.WinForms/Pages/ArtifactReview/ArtifactReviewPageControl.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyService.cs
src/LLMGameCreator.Application/Projects/ICurrentGamePackageService.cs
src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.cs
src/LLMGameCreator.Application/RuntimePreview/GeneratedPackageRuntimePreviewService.cs
src/LLMGameCreator.Application/RuntimePreview/GeneratedContentInteractionPreviewService.cs
src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
tests/LLMGameCreator.Tests/ProductSmoke/GeneratedContentInteractionPreviewSmokeTests.cs
.devflow/scripts/run-product-smoke.ps1
```

Then search narrowly for:
- current package service implementation;
- package export/load repository;
- Artifact Review presenter/service tests;
- Runtime Preview tests.

## Allowed files

```text
src/LLMGameCreator.Application/Projects/**
src/LLMGameCreator.Application/RuntimePreview/**
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyService.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/*GamePackageAssembly*.cs
src/LLMGameCreator.WinForms/Pages/ArtifactReview/**
src/LLMGameCreator.WinForms/Pages/RuntimePreview/**
src/LLMGameCreator.WinForms/CompositionRoot.cs
tests/LLMGameCreator.Tests/ProductSmoke/**
tests/LLMGameCreator.Tests/Runtime/**
tests/LLMGameCreator.Tests/WinForms/*ArtifactReview*Tests.cs
tests/LLMGameCreator.Tests/WinForms/*RuntimePreview*Tests.cs
tests/LLMGameCreator.Tests/Application/**
.devflow/scripts/run-product-smoke.ps1
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

Only touch package assembly service if necessary to expose the last export path/result cleanly.

## Forbidden files

```text
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Infrastructure/Generation/**
generator-library/**
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactContractCatalog.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactValidator.cs
LLMGameCreator.sln
*.csproj
.devflow/NEXT_TASK.md
.devflow/task-queue.json
```

Do not add NuGet packages.
Do not call LLM/provider.
Do not execute generated effects.
Do not overwrite root package.json by default.

## Required behavior

## Task 0: active assembled package repair

### 0.1 Expose assembled package activation

Add a safe way to activate the latest assembled package.

Possible names:

```text
GeneratedPackageActivationService
AssembledGamePackageActivationService
CurrentGamePackageActivationService
```

It should:
1. locate `.llmgc/package-assembly/package.json` for the current project folder;
2. read it through existing JSON package repository or existing package-load path;
3. validate it;
4. set it as active current package in `ICurrentGamePackageService` / implementation;
5. return status/result with package title, source path, validation diagnostics if any.

If `ICurrentGamePackageService` has no setter/update method, add a narrow non-breaking method to the interface and implementation.

Do not replace the root project package file by default.

### 0.2 Artifact Review UI

After successful `Save decisions + apply`:
- show the export path;
- enable a button:

```text
Use assembled package as current
```

Clicking it should activate the assembled package and show status:

```text
Current package switched to assembled generated package. Open Runtime Preview to start.
```

If no assembled package exists:
- do not crash;
- show clear status.

If validation fails:
- do not switch current package;
- show diagnostics/status.

### 0.3 Runtime Preview effect

After activation, Runtime Preview must start the generated assembled package without manual file copy.

Headless smoke must prove this.

## Task 1: quest/dialogue preview state

Add preview-only state/service for generated quest/dialogue stubs.

Possible names:

```text
GeneratedContentPreviewSession
GeneratedContentPreviewSessionService
GeneratedQuestDialoguePreviewService
GeneratedQuestPreviewState
```

Keep it in-memory only for now.

It should support:
- start quest preview by quest id;
- mark next quest step/objective preview;
- list active/completed/available quest preview state;
- preview dialogue lines by dialogue id;
- find dialogues linked to NPC id.

No real package mutation is required.
No effect execution.

## Task 2: Runtime Preview UI actions

Extend Runtime Preview Browser.

For selected entries:

### NPC
Show linked dialogues in details when possible.
Button/action may append linked dialogue ids to log.

### Dialogue
Add button or reuse action:

```text
Preview dialogue
```

It should append dialogue title and lines to log.

### Quest
Add buttons/actions:

```text
Start quest preview
Mark next quest step
```

It should update in-memory preview session and show quest journal/status in details/log.

### Generic
Keep existing:

```text
Append selected to log
```

Do not break it.

## Task 3: preview journal display

Add a small readable quest preview journal, either:
- in the Browser details;
- or as a new inner tab under Generated Content:
  - Browser
  - Summary
  - Quest Journal

Minimum journal display:

```text
Available quests: N
Active preview quests: ...
Completed preview quests: ...
Current/next step text
```

## Task 4: headless smoke

Add scenario:

```text
active-package-quest-dialogue-preview
```

It should:
1. assemble expanded approved artifacts;
2. export assembled package to `.llmgc/package-assembly/package.json` under a temp project folder;
3. activate assembled package through the same activation service used by UI;
4. assert current package generatedContent is not empty;
5. start runtime from active package;
6. build generated interaction catalog;
7. select NPC and resolve linked dialogue;
8. preview dialogue lines;
9. start a quest preview;
10. mark next quest step preview;
11. assert journal state changes;
12. execute a movement command and verify runtime still works;
13. assert no LLM/provider dependency.

## Task 5: script support

Extend `.devflow/scripts/run-product-smoke.ps1`:

```powershell
-Scenario active-package-quest-dialogue-preview
```

Existing scenarios must keep working:
- baseline-strict-package-assembly
- generated-package-runtime-preview
- expanded-contract-batch-smoke
- generated-content-interaction-preview

## Task 6: docs/state

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

Do not mark Unity complete.

## Tests

Required test coverage:

1. activation service fails clearly when no assembled package exists.
2. activation service loads/validates `.llmgc/package-assembly/package.json`.
3. activation service sets current package so generatedContent is available.
4. Runtime Preview can start activated assembled package without copying root `package.json`.
5. dialogue preview returns dialogue lines by dialogue id.
6. NPC lookup finds linked dialogues.
7. start quest preview changes journal state.
8. mark next quest step changes journal state.
9. active-package-quest-dialogue-preview smoke passes.
10. existing product smoke scenarios pass.

Prefer service/presenter/headless tests over fragile UI automation.

## Focused commands

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Activation"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~QuestDialoguePreview"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
```

## Required checks

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario expanded-contract-batch-smoke
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-content-interaction-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario active-package-quest-dialogue-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

Manual UI verification is expected.

### Active package repair

```text
1. Generate/apply full_small_rpg_seed.
2. Artifact Review -> Save decisions + apply.
3. Click Use assembled package as current.
4. Open Runtime Preview.
5. Start.
6. Generated Content must not be empty.
7. No manual copy of package.json should be needed.
```

### Quest/dialogue preview

```text
1. Runtime Preview -> Generated Content -> Browser.
2. Select NPC.
3. Confirm linked dialogue info appears if fixture/content has links.
4. Select Dialogue.
5. Click Preview dialogue or Append/preview action.
6. Confirm dialogue lines appear in Log.
7. Select Quest.
8. Click Start quest preview.
9. Confirm quest appears active in journal/details/log.
10. Click Mark next quest step.
11. Confirm current/next step changes.
12. Move player.
13. Confirm preview state/browser remains usable.
```

## Stop conditions

Stop and report if:
- `.sln` or `.csproj` changes are required;
- root package overwrite becomes required for activation;
- full runtime rewrite is needed;
- Unity/Lua/effect execution is needed;
- LLM/provider is needed;
- WinForms Designer becomes invalid;
- check-all fails after 2 repair attempts;
- more than 22 files need changes.

## Final report

Russian report with:
- files read;
- files changed;
- active assembled package flow;
- whether root package is left untouched;
- Runtime Preview behavior without manual copy;
- quest/dialogue preview session behavior;
- UI actions;
- smoke scenario results;
- check-all/check-devflow results;
- manual verification status;
- remaining gaps and recommended next slice.
