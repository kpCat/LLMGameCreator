# Goal 119 - Accepted Alpha Unity Playable Projection

## Task ID

`goal-119-accepted-alpha-unity-playable-projection`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Explicit user approval

The user explicitly approved this product-oriented lane:

```text
User approval: Goal119 Accepted Alpha Unity Playable Projection may change Unity Assets/Scripts and Assets/Editor, but must not touch Runtime, schema, providers, Lua, generator-library, live geodata, ProjectSettings, Packages, scenes/prefabs without a separate decision.
```

This approval is limited to this Goal119 task.

## Goal type

This is an aggressive product composite goal.

This goal must produce a hands-on Unity result, not another proof-only dashboard. Evidence/tests/docs are required as guardrails, but the core deliverable is a Unity Editor entrypoint that builds/refreshes an accepted Alpha playable projection from the accepted offline geoworld baseline.

## What this must give hands-on

After this goal, the user should be able to open Unity and use:

```text
LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection
```

Expected result:

- a generated temporary root object in the currently open scene, not a saved scene asset;
- a visible accepted-alpha map/projection representation using primitive GameObjects;
- a player proxy;
- chunk/window or boundary/prefetch diagnostics;
- interaction target markers;
- an objective checklist/status representation;
- save/load/replay smoke status;
- a readable diagnostics object/status string;
- a clear/reset action;
- no manual JSON copying/editing.

The user may need to click the menu item. They must not need to edit files manually.

## Why this goal exists now

Goal116 accepted `offline_geoworld_alpha_manual_acceptance_verification` by explicit human decision.
Goal117 selected the safe post-acceptance continuation lane.
Goal118 created `offline_geoworld_alpha_accepted_baseline_v1`.

The next step must stop polishing evidence and produce a visible Unity projection over the accepted Alpha baseline.

## Read first

Read these files before editing:

```text
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
.devflow/artifact-scope/artifact-scope-policy.json

.llmgc/procedural/goal-118-offline-geoworld-accepted-alpha-baseline-review/offline-geoworld-accepted-alpha-baseline-dashboard.json
.llmgc/procedural/goal-118-offline-geoworld-accepted-alpha-baseline-review/offline-geoworld-accepted-alpha-baseline-manifest.json
.llmgc/procedural/goal-118-offline-geoworld-accepted-alpha-baseline-review/offline-geoworld-accepted-alpha-baseline-source-index.json
.llmgc/procedural/goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection/offline-geoworld-alpha-post-acceptance-continuation-matrix.json
.llmgc/procedural/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record/offline-geoworld-alpha-manual-gate-acceptance-record.json

unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaSliceWindow.cs
unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaAcceptanceRunnerWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaSliceCoordinator.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractiveTravelController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionSaveLoadController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldObjectiveAcceptanceController.cs
```

Read payload files under `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/**` as inputs, but do not modify them.

The local `.llmgc/manual/**` result may still exist as untracked human input. Do not stage it, do not require it, do not embed it.

## Allowed paths

You may create or modify only these paths:

```text
docs/agent-tasks/goal-119-accepted-alpha-unity-playable-projection/**
.devflow/artifact-scope/artifact-scope-policy.json

.llmgc/procedural/goal-119-accepted-alpha-unity-playable-projection/**
.llmgc/exports/goal-119-accepted-alpha-unity-playable-projection/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/accepted-alpha-unity-playable-projection.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Status.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal119.cs

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDiagnostics.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
unity/LLMGameCreatorAlpha/Assets/Scenes/**
unity/LLMGameCreatorAlpha/Assets/**/*.unity
unity/LLMGameCreatorAlpha/Assets/**/*.prefab
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
provider / LLM / RAG / media provider code
public GamePackage schema files
Lua / Scripting code
generator-library/**
*.sln
*.csproj
Directory.Build.*
dependency/package files
LFZ source/archive
```

No live geodata ingestion, no provider calls, no network fetching, no runtime online behavior, no real map scraping, no final art, no atlas, no scene/prefab saved assets, no ProjectSettings/Packages changes, no StreamingAssets writes, and no release packaging.

## Unity behavior requirements

Implement an EditorWindow:

```text
unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
```

Menu path:

```text
LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection
```

Window/buttons:

```text
Refresh Accepted Baseline
Build/Refresh Playable Projection
Run Local Projection Smoke
Clear Projection
Copy Diagnostics
```

The window must:

1. Read existing accepted baseline / Alpha Slice / Goal101-108/109 payload summaries from repository files and/or existing StreamingAssets.
2. Create/update one temporary scene root object named `__LLMGC_AcceptedAlphaPlayableProjection__`.
3. Attach `AcceptedAlphaPlayableProjectionController`.
4. Build a projection in the currently open scene using primitives only: root object, player proxy, chunk/window markers, boundary/prefetch markers, interaction target markers, objective marker/checklist representation and diagnostics label/status object.
5. Avoid saving the scene, creating prefabs, creating assets, modifying ProjectSettings, modifying Packages, or writing StreamingAssets.
6. Provide `Clear Projection` that removes only the generated root object.
7. Provide a deterministic smoke result with baseline loaded, at least one player proxy, at least one chunk/window marker, at least one interaction/objective marker or checklist entry, diagnostics status string and zero fatal errors.
8. Be safe if payloads are missing: show diagnostics and avoid exceptions.

The runtime scripts may be MonoBehaviours, but they must not call network/providers/LLM/RAG/media or Runtime/GamePackage schema.

## Application evidence seam

Add a bounded BCL-only service:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/
```

It must produce deterministic evidence that Goal119 added the Unity playable projection entrypoint.

It must verify by source scanning and artifact inspection:

- menu path exists exactly;
- new Unity scripts are present;
- no forbidden Unity paths changed in expected changed paths;
- no ProjectSettings/Packages/StreamingAssets changes are expected;
- no Runtime/schema/provider/Lua/generator-library paths are expected;
- accepted baseline from Goal118 exists and is ready;
- Goal116 manual gate status is accepted;
- Goal119 is not final release.

## Required artifacts

Create deterministic artifacts under:

```text
.llmgc/procedural/goal-119-accepted-alpha-unity-playable-projection/
.llmgc/exports/goal-119-accepted-alpha-unity-playable-projection/
```

Recommended files:

```text
accepted-alpha-unity-playable-projection-dashboard.json
accepted-alpha-unity-playable-projection-script-inventory.json
accepted-alpha-unity-playable-projection-smoke-plan.json
accepted-alpha-unity-playable-projection-report.md
accepted-alpha-unity-playable-projection-quality-gate-scan.json
accepted-alpha-unity-playable-projection-negative-proof.json
accepted-alpha-unity-playable-projection-file-index.json
```

Artifacts must not embed raw `.llmgc/manual/**`.

## Visual World Stream Preview Workspace

Add a read-only Goal119 section showing:

```text
projectionStatus
unityMenuPath
baselineId
acceptedBaselineReady
expectedGeneratedRootName
scriptInventoryCount
smokePlanStepCount
forbiddenUnitySurfaceClean
doNotStartAutomatically
evidencePath
exportPath
```

## Docs/current state

Update current-state and queue docs so they clearly say:

- Goal119 creates a Unity Editor/Scripts playable projection entrypoint over the accepted Alpha baseline.
- The user can verify by opening Unity and selecting `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.
- Goal119 is not final release and does not authorize live geodata, providers, Runtime, schema, Lua, generator-library, final art, atlas, Unity scene/prefab/project-settings/StreamingAssets changes, or release packaging.
- Next step after Goal119 should be based on hands-on Unity verification, not another pure review goal unless a P0/P1 appears.

## Artifact-scope policy

Add scenario:

```text
goal-119-accepted-alpha-unity-playable-projection
```

It must allow only Goal119 expected files and exclude `.llmgc/manual/**`, Unity scenes/prefabs/settings/Packages/StreamingAssets, Runtime/schema/provider/Lua/generator-library.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~Goal119|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-119-accepted-alpha-unity-playable-projection
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Also run Unity batchmode compile, because this goal changes Unity scripts:

```powershell
Unity.exe -batchmode -quit -projectPath .\unity\LLMGameCreatorAlpha -logFile .\.llmgc\procedural\goal-119-accepted-alpha-unity-playable-projection\unity-batchmode-compile.log
```

If `Unity.exe` is not on PATH, use the installed Unity 6000.1.10f1 editor path if available. If Unity is unavailable, mark BLOCKED unless all code/source checks pass and the only missing check is local Unity availability; do not fake compile success.

## Quality gate

GREEN requires:

- Unity batchmode compile passes.
- Editor menu path exists.
- Projection scripts compile.
- Application evidence and tests pass.
- Goal119 WinForms workspace section exists.
- `.llmgc/manual/**` is not staged/committed/embedded.
- No forbidden path changes.
- No scene/prefab/project settings/package/StreamingAssets changes.
- Artifact scope passes.
- Final worktree after push is clean except possible expected untracked `.llmgc/manual/...result.json`.

BLOCKED if Unity compile cannot be verified or if accepted baseline evidence is missing.

FAILED if build/tests break, forbidden files must be touched, or source churn cannot be restored.

## Commit / push policy

Before commit:

```powershell
git diff --cached --name-only
git diff --cached --check
git diff --cached --name-only | Select-String -SimpleMatch ".llmgc/manual"
```

The last command must produce no matches.

Commit and push with one of:

```text
GREEN Goal 119 accepted alpha unity playable projection
BLOCKED Goal 119 accepted alpha unity playable projection
FAILED Goal 119 accepted alpha unity playable projection
```

Push to `origin/main`.

Final report must include:

- commit SHA;
- push status;
- grouped changed files;
- Unity compile result/log path;
- exact Unity menu path;
- how the user verifies hands-on;
- whether `.llmgc/manual/**` remains untracked/uncommitted;
- remaining debt.
