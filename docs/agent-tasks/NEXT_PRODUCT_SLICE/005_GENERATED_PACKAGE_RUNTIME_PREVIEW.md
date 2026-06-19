# Product Slice 005 Task: Generated Package Runtime Preview

## Task type

Large product implementation slice, bounded to Runtime Preview + generated-content preview projection.

## Goal

Make generated package content visible and smoke-testable in Runtime Preview.

Current chain:

```text
Capability selection
-> strict artifacts
-> review/approve
-> package assembly
-> package.json
-> headless package smoke
```

This task adds:

```text
assembled package
-> runtime preview start
-> generated content readable in preview
-> headless runtime-preview smoke
```

## Recommended Codex reasoning level

High.

Do not use Max/Ultra on first attempt.
Do not use Low/Medium: this slice touches UI, runtime preview, tests and smoke flow.

## Source-of-truth docs to read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/PRODUCT_SLICE_005_GENERATED_PACKAGE_RUNTIME_PREVIEW.md
src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.cs
src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.Designer.cs
src/LLMGameCreator.Runtime/DefaultGameRuntime.cs
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
tests/LLMGameCreator.Tests/ProductSmoke/BaselineStrictArtifactsPackageAssemblySmokeTests.cs
.devflow/scripts/run-product-smoke.ps1
```

Then search narrowly for runtime tests, canvas code and package test helpers.

## Allowed files

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/**RuntimePreview**
src/LLMGameCreator.Application/RuntimePreview/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.cs
src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimeMapCanvas.cs
src/LLMGameCreator.WinForms/CompositionRoot.cs
tests/LLMGameCreator.Tests/ProductSmoke/**
tests/LLMGameCreator.Tests/Runtime/**
tests/LLMGameCreator.Tests/Design/GeneratorPlanGamePackageAssemblyPipelineTests.cs
.devflow/scripts/run-product-smoke.ps1
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

Only touch `CompositionRoot.cs` if registering a new preview service is necessary.

## Forbidden files

```text
src/LLMGameCreator.Scripting/**
generator-library/**
src/LLMGameCreator.Infrastructure/Generation/**
LLMGameCreator.sln
*.csproj
.devflow/NEXT_TASK.md
.devflow/task-queue.json
docs/agent-tasks/M6/**
docs/agent-tasks/M8/**
docs/agent-tasks/M9/**
docs/agent-tasks/M10/**
```

Do not add NuGet packages.

## Required behavior

### 1. Add generated package preview projection

Create a small read-only projection service/model.

Possible names:

```text
GeneratedPackageRuntimePreviewService
GeneratedPackageRuntimePreviewModel
GeneratedPackageRuntimePreviewScene
GeneratedPackageRuntimePreviewQuest
GeneratedPackageRuntimePreviewMechanic
```

The projection should accept:

```text
GamePackageDefinition package
GameState? state
```

and return:

```text
package title/description
current map id/name
current generated scene title/description/purpose
profile title/description/genre/tone/core loop/pillars
quests count + readable summaries
mechanics count + readable summaries
applied artifacts count + contracts
warnings when generatedContent is empty
```

If `state` is null, use package manifest start map.

### 2. Runtime Preview UI

Update RuntimePreview page so after Start and after commands it refreshes generated content details.

User should see at least:

```text
Current scene:
<title>
<description>
<purpose>

Profile:
<title/description/genre/tone/core loop>

Quests:
- title: description

Mechanics:
- name: description

Applied artifacts:
- contract id / artifact id / mapping result
```

Acceptable layout:
- right side `TabControl` with `Log` and `Generated Content`;
- or add second read-only multiline TextBox under the log.
Prefer minimal stable WinForms UI.

### 3. Fix RuntimePreview split safety if needed

`RuntimePreviewPageControl.Designer.cs` currently sets a hard splitter distance. Replace or guard it with a safe pattern if it can throw at startup or resize.

Do not break Designer validity.

### 4. Runtime behavior remains intact

Existing map start/move/interact behavior must continue to work.

Do not replace `DefaultGameRuntime` with a new engine.
Do not implement full quest/combat/dialogue simulation.

### 5. Headless smoke test

Add or extend ProductSmoke tests for:

```text
GeneratedPackageRuntimePreviewSmoke
```

It must:
1. build baseline approved artifact fixture;
2. assemble package;
3. start `DefaultGameRuntime`;
4. build generated preview projection;
5. assert current generated scene is resolved;
6. assert profile/quests/mechanics/provenance are visible;
7. execute a simple movement command and verify movement still works;
8. assert no LLM/provider markers are needed.

### 6. Product smoke script

If practical, extend:

```text
.devflow/scripts/run-product-smoke.ps1
```

to support:

```powershell
-Scenario generated-package-runtime-preview
```

This scenario can run:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~GeneratedPackageRuntimePreviewSmoke"
```

If adding a scenario is too risky, keep script unchanged and document the test command.

### 7. Docs/state

Update:

```text
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

Say that generated package runtime preview is smoke-tested headlessly.

Do not mark Unity complete.

## Tests

Required:

1. preview projection resolves start scene from generatedContent.scenes by map id.
2. projection exposes profile title/description/core loop.
3. projection exposes quests and mechanics.
4. projection exposes applied artifact provenance contracts.
5. Runtime start + generated preview projection works for assembled baseline package.
6. Movement still works after runtime start.
7. No LLM/provider dependency in runtime preview smoke.

## Focused commands

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~RuntimePreview"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~GeneratedPackageRuntimePreviewSmoke"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
```

## Required checks

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

If `generated-package-runtime-preview` scenario is added:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
```

## Manual verification

Manual UI verification is useful for this slice.

Expected manual steps:

```text
1. Open app.
2. Open assembled/generated test project.
3. Open Runtime Preview.
4. Press Start.
5. Confirm map appears.
6. Confirm log starts.
7. Confirm generated content panel shows scene/profile/quests/mechanics/provenance.
8. Move player once.
9. Confirm movement still logs and generated content panel remains populated.
```

## Stop conditions

Stop and report if:

- `.sln` or `.csproj` changes are required;
- Unity/runtime rewrite becomes necessary;
- Lua/script execution becomes necessary;
- LLM/provider calls are needed;
- WinForms designer becomes invalid;
- check-all fails after 2 repair attempts.

## Expected final report in Russian

Include:

- files read;
- files changed;
- preview projection behavior;
- Runtime Preview UI behavior;
- split safety handling;
- runtime movement compatibility;
- headless smoke tests;
- product smoke script changes;
- check-devflow-state result;
- check-all result;
- manual verification status;
- remaining gaps and recommended next slice.
