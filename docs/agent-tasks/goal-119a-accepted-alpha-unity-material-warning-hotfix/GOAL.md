# Goal 119A — Accepted Alpha Unity Material Warning Hotfix

## Task ID

`goal-119a-accepted-alpha-unity-material-warning-hotfix`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Why this hotfix exists

Manual Goal119 Unity verification succeeded functionally:

- the Unity menu opened;
- `Build/Refresh Playable Projection` built the projection;
- the temporary root `__LLMGC_AcceptedAlphaPlayableProjection__` appeared;
- local smoke reported `passed=True`.

But Unity Console logged repeated edit-mode material warnings:

```text
Instantiating material due to calling renderer.material during edit mode.
This will leak materials into the scene. You most likely want to use renderer.sharedMaterial instead.
UnityEngine.Renderer:get_material()
```

This must be fixed without turning into another review/evidence-only goal.

## Goal type

Focused P1 Unity hotfix.

The core deliverable is: `Build/Refresh Playable Projection` no longer emits `renderer.material` edit-mode material-instantiation warnings, and automated Unity editor smoke catches this class of issue.

## Read first

```text
unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDiagnostics.cs
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
.llmgc/procedural/goal-119-accepted-alpha-unity-playable-projection/accepted-alpha-unity-playable-projection-dashboard.json
```

## Allowed paths

You may modify/create only:

```text
docs/agent-tasks/goal-119a-accepted-alpha-unity-material-warning-hotfix/**
.devflow/artifact-scope/artifact-scope-policy.json

.llmgc/procedural/goal-119a-accepted-alpha-unity-material-warning-hotfix/**
.llmgc/exports/goal-119a-accepted-alpha-unity-material-warning-hotfix/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDiagnostics.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionModels.cs
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

No live geodata, no providers, no Runtime/schema/Lua/generator-library, no final art/atlas, no scene/prefab saved assets, no ProjectSettings/Packages/StreamingAssets writes, and no release packaging.

## Required implementation

### 1. Remove edit-mode `renderer.material` use

Fix `AcceptedAlphaPlayableProjectionPrimitiveFactory`.

Current pattern to eliminate:

```csharp
renderer.material = ...
renderer.material.color = color;
```

Replace with a no-leak edit-mode-safe approach.

Preferred approach:

- do not instantiate per-marker materials;
- keep the primitive's existing `sharedMaterial`;
- use `MaterialPropertyBlock` to apply color;
- set both `_Color` and `_BaseColor` if possible;
- never access `renderer.material`.

Add a source/test guard that fails if Goal119 Unity scripts contain `renderer.material` or `.material =` in the accepted alpha projection code.

### 2. Add batchmode Unity editor smoke method

Add an editor-accessible static method, preferably in `AcceptedAlphaPlayableProjectionWindow`:

```csharp
public static void RunBatchmodeProjectionSmoke()
```

It must:

- create or refresh `__LLMGC_AcceptedAlphaPlayableProjection__`;
- call `RefreshAcceptedBaseline`;
- call `BuildOrRefreshProjection`;
- call `RunLocalProjectionSmoke`;
- write clear `GOAL119A_PROJECTION_SMOKE_PASS` / `GOAL119A_PROJECTION_SMOKE_FAIL` lines to the Unity log;
- clear the temporary projection root before exit if possible;
- exit with nonzero code in batchmode if smoke fails.

Do not save the scene.

### 3. Unity log scan

Run Unity batchmode with the execute method:

```powershell
Unity.exe -batchmode -quit -projectPath .\unity\LLMGameCreatorAlpha -executeMethod LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeProjectionSmoke -logFile .\.llmgc\procedural\goal-119a-accepted-alpha-unity-material-warning-hotfix\unity-batchmode-projection-smoke.log
```

If `Unity.exe` is not on PATH, use the installed Unity 6000.1.10f1 editor path if available.

The validation must fail/BLOCK if the log contains:

```text
Instantiating material due to calling renderer.material during edit mode
UnityEngine.Renderer:get_material()
GOAL119A_PROJECTION_SMOKE_FAIL
```

The validation must pass only if the log contains:

```text
GOAL119A_PROJECTION_SMOKE_PASS
```

### 4. Artifacts

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-119a-accepted-alpha-unity-material-warning-hotfix/
.llmgc/exports/goal-119a-accepted-alpha-unity-material-warning-hotfix/
```

Recommended files:

```text
accepted-alpha-unity-material-warning-hotfix-dashboard.json
accepted-alpha-unity-material-warning-hotfix-log-scan.json
accepted-alpha-unity-material-warning-hotfix-script-scan.json
accepted-alpha-unity-material-warning-hotfix-report.md
accepted-alpha-unity-material-warning-hotfix-negative-proof.json
accepted-alpha-unity-material-warning-hotfix-file-index.json
unity-batchmode-projection-smoke.log
```

Do not embed raw `.llmgc/manual/**`.

### 5. Docs/state

Update current-state/queue/debt briefly:

- Goal119A fixes the material-instantiation warning from manual Goal119 verification.
- Goal119 remains the product deliverable.
- Next manual check is the same Unity menu, but expected Console result is no `renderer.material` material-leak warning.
- This is not final release and does not authorize forbidden lanes.

### 6. Artifact scope

Add scenario:

```text
goal-119a-accepted-alpha-unity-material-warning-hotfix
```

It must exclude `.llmgc/manual/**`, Unity scenes/prefabs/settings/Packages/StreamingAssets, Runtime/schema/providers/Lua/generator-library.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~Goal119|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-119a-accepted-alpha-unity-material-warning-hotfix
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Run Unity batchmode projection smoke with `-executeMethod` as described above.

After Unity runs, restore/clean generated Unity side effects before staging:

```text
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**
unity/LLMGameCreatorAlpha/Assets/**/*.meta
unity/LLMGameCreatorAlpha/Library/**
unity/LLMGameCreatorAlpha/Temp/**
```

Only stage allowed files.

## Quality gate

GREEN requires:

- manual Goal119 projection remains functional by batchmode smoke;
- Unity log contains `GOAL119A_PROJECTION_SMOKE_PASS`;
- Unity log does not contain the renderer.material edit-mode material-instantiation warning;
- no `renderer.material` access remains in AcceptedAlpha playable projection Unity scripts;
- no forbidden path changes;
- no `.llmgc/manual/**` staged/committed;
- tests/checks pass;
- artifact scope passes.

BLOCKED if Unity cannot run or smoke cannot be verified honestly.

FAILED if build/tests are broken or forbidden changes are required.

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
GREEN Goal 119A accepted alpha unity material warning hotfix
BLOCKED Goal 119A accepted alpha unity material warning hotfix
FAILED Goal 119A accepted alpha unity material warning hotfix
```

Final report must include:

- commit SHA;
- Unity batchmode projection smoke result;
- log path;
- whether material warning is absent;
- exact changed files;
- final git status;
- remaining debt.
