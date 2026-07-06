# Goal 123 — Generic GamePackage Playable Projection Adapter

## Task ID

`goal-123-generic-gamepackage-playable-projection-adapter`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product goal.

This goal must move the project from the geoworld-specific accepted Alpha projection toward the real AI Game Builder / GamePackage combiner goal. It must add a hands-on generic GamePackage projection adapter over the existing sample package without modifying Runtime, public schema, providers, Lua, generator-library, Unity scenes/prefabs/settings, Packages, or StreamingAssets.

## Why this goal exists

Goal119-122 made the accepted geoworld Alpha projection usable in Unity. That is useful, but the final product is not a geoworld-only tool. Goal123 must prove the same projection shell can also inspect and visualize a normal GamePackage-like package.

The repository already contains a sample package at:

```text
samples/minimal-map-game/package.json
```

Use it as the first generic package projection source. Do not modify it.

## Hands-on result

After this goal, the user should be able to open the existing Unity window:

```text
LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection
```

and use a new main/debug action:

```text
Run Generic Package Projection Verification
```

The Unity scene should show a projection of `samples/minimal-map-game/package.json`, including at least:

- package title/id;
- map grid dimensions;
- start/player proxy;
- tile/road/wall markers;
- entity markers for NPC/object instances;
- interaction marker/details for interactable entities;
- item summary/list panel;
- simple package verification event log.

This is a projection-only preview. It is not Runtime gameplay and must not mutate the package.

## Read first

```text
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
.devflow/artifact-scope/artifact-scope-policy.json

samples/minimal-map-game/package.json

.llmgc/procedural/goal-122-accepted-alpha-projection-action-loop-and-window-polish/accepted-alpha-projection-action-loop-dashboard.json
.llmgc/procedural/goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification/accepted-alpha-interaction-drilldown-dashboard.json

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionState.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionActionPreview.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDrilldown.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-123-generic-gamepackage-playable-projection-adapter/**
.devflow/artifact-scope/artifact-scope-policy.json

.llmgc/procedural/goal-123-generic-gamepackage-playable-projection-adapter/**
.llmgc/exports/goal-123-generic-gamepackage-playable-projection-adapter/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/generic-gamepackage-playable-projection-adapter.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal123.cs

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionController.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
samples/minimal-map-game/package.json
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

No live geodata, no providers, no Runtime/schema/Lua/generator-library, no final art/atlas, no saved scene/prefab assets, no ProjectSettings/Packages/StreamingAssets writes, and no release packaging.

## Primary deliverable A — Generic package projection in Unity

Add a generic projection adapter under allowed Unity scripts. It should read `samples/minimal-map-game/package.json` from the repository root using a bounded, defensive, projection-only parser.

Do not require full schema interpretation. Extract enough to visualize and verify:

- `manifest.packageId`, `manifest.title`, `manifest.startMapId`;
- first map id/name/width/height/startPosition;
- explicit tiles and their tileId;
- entities with prototypeId and position;
- prototype names/components where practical;
- items id/name/kind where practical.

Projection requirements:

1. Create a section under `__LLMGC_AcceptedAlphaPlayableProjection__`, e.g. `goal123_generic_gamepackage_projection`.
2. Create map/tile markers scaled to a small readable grid.
3. Create player/start marker.
4. Create entity markers.
5. Add labels for package title, map id, entities and item summary.
6. Add marker descriptors compatible with the existing selection/details system.
7. Add a package event log / status panel.
8. Do not write files or mutate assets.

## Primary deliverable B — One-click generic package verification

Extend the existing window with a clearly separated control:

```text
Run Generic Package Projection Verification
```

It must:

- build/refresh the generic package projection;
- select/focus the generic projection root or first entity;
- populate details/event log;
- verify package title/id present;
- verify map dimensions present;
- verify start/player marker present;
- verify at least one tile marker present;
- verify at least one entity marker present;
- verify at least one item summary entry present;
- return pass/fail in the window.

Add batchmode method:

```text
LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageProjectionSmoke
```

It must log:

```text
GOAL123_GENERIC_PACKAGE_PROJECTION_PASS
```

or:

```text
GOAL123_GENERIC_PACKAGE_PROJECTION_FAIL
```

## Primary deliverable C — keep Goal122 geoworld projection intact

Do not regress the accepted Alpha action loop. Goal122 one-click verification should remain present and green by source/evidence checks.

## Application evidence seam

Add/update BCL-only evidence under:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/
```

It must verify:

- Goal122 remains green;
- `samples/minimal-map-game/package.json` exists and is not modified;
- generic package projection scripts/methods exist;
- batchmode generic package smoke log contains pass marker;
- forbidden markers are absent;
- cleanup script remains available;
- no forbidden path is expected.

## Required artifacts

Create deterministic artifacts under:

```text
.llmgc/procedural/goal-123-generic-gamepackage-playable-projection-adapter/
.llmgc/exports/goal-123-generic-gamepackage-playable-projection-adapter/
```

Recommended files:

```text
generic-gamepackage-projection-dashboard.json
generic-gamepackage-projection-script-inventory.json
generic-gamepackage-projection-smoke-plan.json
generic-gamepackage-projection-log-scan.json
generic-gamepackage-projection-report.md
generic-gamepackage-projection-negative-proof.json
generic-gamepackage-projection-file-index.json
unity-batchmode-generic-gamepackage-projection.log
```

## Visual World Stream Preview Workspace

Add a read-only Goal123 section showing:

```text
genericProjectionStatus
samplePackagePath
packageId
packageTitle
mapId
mapSize
entityCount
itemCount
unitySmokeStatus
goal122StillGreen
cleanupScriptAvailable
doNotStartAutomatically
evidencePath
exportPath
```

## Docs/current state

Update current-state and queue docs so they clearly say:

- Goal123 starts moving from geoworld-only accepted Alpha projection toward generic GamePackage projection.
- It uses `samples/minimal-map-game/package.json` as a read-only package source.
- It remains projection-only and does not authorize Runtime/provider/schema/Lua/generator-library/final-art/atlas/Unity scene/prefab/project-settings/StreamingAssets/release work.
- Manual checking is optional and should use one button only.
- After manual Unity checks, use `.devflow\scripts\clean-unity-editor-noise.cmd`.

## Artifact-scope policy

Add scenario:

```text
goal-123-generic-gamepackage-playable-projection-adapter
```

It must allow only Goal123 expected files and exclude `.llmgc/manual/**`, `samples/minimal-map-game/package.json`, Unity scenes/prefabs/settings/Packages/StreamingAssets, Runtime/schema/provider/Lua/generator-library.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~Goal123|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-123-generic-gamepackage-playable-projection-adapter
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Run Unity batchmode smoke:

```powershell
Unity.exe -batchmode -quit -projectPath .\unity\LLMGameCreatorAlpha -executeMethod LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageProjectionSmoke -logFile .\.llmgc\procedural\goal-123-generic-gamepackage-playable-projection-adapter\unity-batchmode-generic-gamepackage-projection.log
```

After Unity batchmode, run cleanup:

```powershell
.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
git status --short --untracked-files=all
```

Only stage allowed files. Final status must be clean after push.

## Quality gate

GREEN requires:

- batchmode log contains `GOAL123_GENERIC_PACKAGE_PROJECTION_PASS`;
- generic package projection reads the sample package without modifying it;
- package id/title/map/entity/item summary is visible in projection/window/evidence;
- Goal122 one-click accepted-alpha verification remains present;
- cleanup script still works;
- no forbidden path changes;
- no `.llmgc/manual/**` staged/tracked;
- tests/checks pass;
- artifact scope passes;
- final git status clean.

BLOCKED if Unity cannot run or generic package projection cannot be verified honestly.

FAILED if build/tests break or forbidden changes are required.

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
GREEN Goal 123 generic gamepackage playable projection adapter
BLOCKED Goal 123 generic gamepackage playable projection adapter
FAILED Goal 123 generic gamepackage playable projection adapter
```

Final report must include commit SHA, Unity generic package smoke result, manual verification path, cleanup command, exact changed files grouped by area, final git status, and remaining debt.
