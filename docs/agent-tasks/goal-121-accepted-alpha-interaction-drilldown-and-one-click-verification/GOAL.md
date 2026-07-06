# Goal 121 — Accepted Alpha Interaction Drilldown + One-Click Verification Harness

## Task ID

`goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product goal.

This must move the Unity projection toward an actually usable inspection/interaction surface. It must not be another proof-only/review-only goal.

The key process requirement: after this goal, manual verification must be one menu action plus one verification button, not a sequence of many buttons.

## Why this goal exists

Goal119 made a visible Unity projection.
Goal119A removed the edit-mode material leak warning.
Goal120 improved projection usability and added the Unity noise cleanup script.
Goal120A fixed the cleanup script empty-status bug.

Manual checks now work, but the user still has to click many individual buttons. Goal121 must consolidate the hands-on verification path and add real drilldown/interaction value.

## Product deliverable

In Unity, the user should open:

```text
LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection
```

Then click one main button:

```text
Run Full Projection Verification
```

That button must automatically:

1. refresh accepted baseline;
2. build/refresh projection;
3. focus generated root;
4. select player proxy;
5. select first interaction target;
6. populate selected marker details;
7. populate interaction/action preview;
8. select first objective;
9. populate objective/replay details;
10. select diagnostics marker;
11. refresh/show legend;
12. run local smoke;
13. write a readable event log in the window.

Existing granular buttons may remain for debugging, but the primary manual path must be one button.

## Read first

```text
.devflow/scripts/clean-unity-editor-noise.ps1
.devflow/scripts/clean-unity-editor-noise.cmd

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDiagnostics.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs

.llmgc/procedural/goal-120a-clean-unity-editor-noise-empty-status-hotfix/clean-unity-editor-noise-empty-status-hotfix-dashboard.json
.llmgc/procedural/goal-120-accepted-alpha-projection-usability-and-cleanup/accepted-alpha-projection-usability-dashboard.json
.llmgc/procedural/goal-119a-accepted-alpha-unity-material-warning-hotfix/accepted-alpha-unity-material-warning-hotfix-dashboard.json
.llmgc/procedural/goal-119-accepted-alpha-unity-playable-projection/accepted-alpha-unity-playable-projection-dashboard.json
.llmgc/procedural/goal-118-offline-geoworld-accepted-alpha-baseline-review/offline-geoworld-accepted-alpha-baseline-dashboard.json

unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105/offline-geoworld-interaction-actions.json
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105/offline-geoworld-interaction-targets.json
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105/offline-geoworld-interaction-state-delta-plan.json
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal106/offline-geoworld-session-replay-script.json
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal107/offline-geoworld-objectives.json
```

Do not modify StreamingAssets. Read them only.

## Preflight cleanup

Before editing, run:

```powershell
.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
git status --short --untracked-files=all
```

If unexpected non-cleanup local changes remain, stop as BLOCKED and report them. Do not delete user work.

## Allowed paths

You may create/modify only:

```text
docs/agent-tasks/goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification/**
.devflow/artifact-scope/artifact-scope-policy.json

.llmgc/procedural/goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification/**
.llmgc/exports/goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/accepted-alpha-interaction-drilldown-and-one-click-verification.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal121.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs

tests/LLMGameCreator.Tests/DevFlow/CleanUnityEditorNoiseScriptTests.cs

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDiagnostics.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDrilldown.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionActionPreview.cs
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
```

No live geodata, no providers, no Runtime/schema/Lua/generator-library, no final art/atlas, no saved scene/prefab assets, no ProjectSettings/Packages/StreamingAssets writes, and no release packaging.

## Unity requirements

### Main window

Extend `AcceptedAlphaPlayableProjectionWindow` with one prominent button:

```text
Run Full Projection Verification
```

Place it above the individual debug buttons.

Add read-only text areas/fields:

```text
Selected Marker Details
Interaction Preview
Objective / Replay Details
Verification Event Log
```

The event log should be compact and readable, not an enormous dump.

### Drilldown data

Add drilldown support so selected markers can show:

- marker id/name;
- marker kind;
- source goal/file if known;
- display label;
- status/details;
- for interaction target: target id/name, action count, first action summary, expected state delta summary if available;
- for objective: objective id/title, completion state, related replay/checkpoint summary if available.

It is acceptable to parse the existing compact JSON payloads with the same simple deterministic string-field helpers already used in the Unity scripts. Do not introduce dependencies.

### One-click verification behavior

The new button must produce a final status similar to:

```text
Goal121 full projection verification passed
```

and smoke text must include at least:

```text
fullVerificationPassed=True
rootPresent=True
baselineLoaded=True
playerProxyPresent=True
legendPresent=True
markerDescriptorPresent=True
selectableInteractionTargetPresent=True
interactionPreviewPresent=True
selectableObjectivePresent=True
objectiveReplayDetailsPresent=True
diagnosticsStatusPresent=True
zeroFatalErrors=True
```

### Batchmode smoke

Add/extend a static execute method:

```text
LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeProjectionFullVerification
```

It must run the same one-click verification logic and log:

```text
GOAL121_FULL_PROJECTION_VERIFICATION_PASS
```

or:

```text
GOAL121_FULL_PROJECTION_VERIFICATION_FAIL
```

In batchmode, clear the temporary projection root before exit. Do not save the scene.

The Unity log must not contain:

```text
Instantiating material due to calling renderer.material during edit mode
UnityEngine.Renderer:get_material()
```

### Existing debug buttons

Keep existing buttons working:

- Refresh Accepted Baseline
- Build/Refresh Playable Projection
- Focus Projection Camera
- Select Player Proxy
- Select Next Interaction Target
- Select Next Objective
- Select Diagnostics Marker
- Toggle/Refresh Legend
- Run Local Projection Smoke
- Clear Projection
- Copy Diagnostics

But the manual acceptance path is now the single full-verification button.

## Cleanup script usage

Goal121 must use the cleanup script in validation after Unity batchmode runs:

```powershell
.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
```

Final git status must be clean.

## Application evidence seam

Add/update BCL-only evidence under:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/
```

It must verify by source/artifact scan:

- one-click button exists;
- batchmode full verification method exists;
- full verification pass/fail markers exist;
- drilldown fields exist;
- interaction preview fields exist;
- objective/replay details fields exist;
- material-warning guard remains;
- cleanup script remains available;
- forbidden paths are not expected.

## Required artifacts

Create deterministic artifacts under:

```text
.llmgc/procedural/goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification/
.llmgc/exports/goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification/
```

Recommended files:

```text
accepted-alpha-interaction-drilldown-dashboard.json
accepted-alpha-interaction-drilldown-script-inventory.json
accepted-alpha-interaction-drilldown-smoke-plan.json
accepted-alpha-interaction-drilldown-log-scan.json
accepted-alpha-interaction-drilldown-report.md
accepted-alpha-interaction-drilldown-negative-proof.json
accepted-alpha-interaction-drilldown-file-index.json
unity-batchmode-full-projection-verification.log
```

## Visual World Stream Preview Workspace

Add a read-only Goal121 section showing:

```text
fullVerificationStatus
unityMenuPath
oneClickButtonPresent
drilldownFieldsPresent
interactionPreviewPresent
objectiveReplayDetailsPresent
batchmodeFullVerificationMarker
cleanupScriptAvailable
materialWarningGuardPresent
humanManualStepsReducedToOneButton
evidencePath
exportPath
```

## Docs/current state

Update docs/current state so they say:

- Goal121 reduces manual Unity verification to one main button.
- The user should not have to click every debug button after each goal.
- After Unity manual checks, use `.devflow\scripts\clean-unity-editor-noise.cmd`.
- Next goals must continue product-visible work or automated verification, not proof-only churn.
- This does not authorize Runtime/provider/schema/Lua/generator-library/final art/atlas/release work.

## Artifact scope

Add scenario:

```text
goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification
```

It must allow only Goal121 expected files and exclude forbidden zones.

## Validation

Run:

```powershell
.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~Goal121|FullyQualifiedName~VisualWorldStreamPreviewWorkspace|FullyQualifiedName~CleanUnityEditorNoiseScript"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification
```

Run Unity batchmode full verification:

```powershell
Unity.exe -batchmode -quit -projectPath .\unity\LLMGameCreatorAlpha -executeMethod LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeProjectionFullVerification -logFile .\.llmgc\procedural\goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification\unity-batchmode-full-projection-verification.log
```

If `Unity.exe` is not on PATH, use the installed Unity 6000.1.10f1 path if available. If Unity is unavailable, BLOCK honestly; do not fake it.

After Unity batchmode, run:

```powershell
.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
git status --short --untracked-files=all
```

Then:

```powershell
git diff --check
git diff --cached --check
git ls-files .llmgc/manual
```

## Quality gate

GREEN requires:

- Full verification button exists and is the primary manual path.
- Batchmode full verification logs `GOAL121_FULL_PROJECTION_VERIFICATION_PASS`.
- Smoke text includes the required full verification fields.
- Interaction preview and objective/replay details exist.
- Material warning is absent.
- Cleanup script works after Unity batchmode.
- Final git status clean.
- No forbidden path changes.
- No `.llmgc/manual/**` staged/committed.
- Tests/checks pass.
- Artifact scope passes.

BLOCKED if Unity cannot run, one-click verification cannot be implemented without forbidden changes, or cleanup script cannot keep the worktree clean.

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
GREEN Goal 121 accepted alpha interaction drilldown and one click verification
BLOCKED Goal 121 accepted alpha interaction drilldown and one click verification
FAILED Goal 121 accepted alpha interaction drilldown and one click verification
```

Final report must include:

- commit SHA;
- Unity batchmode full verification result;
- material warning status;
- cleanup script after-Unity result;
- how the user verifies manually with one button;
- exact changed files;
- final git status;
- remaining debt.
