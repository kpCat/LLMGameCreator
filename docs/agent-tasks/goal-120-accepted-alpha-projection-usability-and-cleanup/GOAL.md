# Goal 120 — Accepted Alpha Projection Usability Pass + Unity Noise Cleaner

## Task ID

`goal-120-accepted-alpha-projection-usability-and-cleanup`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product composite goal with one required tooling fix.

This is not another evidence-only/review goal. The primary deliverable is a more usable hands-on Unity projection over the accepted Alpha baseline. The secondary required deliverable is a safe cleanup script so manual Unity checks no longer leave the user with hundreds of generated files to delete by hand.

## Current pain to fix

Manual Unity verification after Goal119/119A works, but opening Unity creates noisy local git changes such as:

```text
M unity/LLMGameCreatorAlpha/ProjectSettings/ProjectVersion.txt
?? unity/LLMGameCreatorAlpha/Assets/**/*.meta
?? unity/LLMGameCreatorAlpha/Packages/packages-lock.json
?? unity/LLMGameCreatorAlpha/ProjectSettings/*.asset
```

Goal120 must add a safe repo-local cleanup script and use it in validation where appropriate.

## Preflight: dirty worktree handling

Before editing, run:

```powershell
git status --short --untracked-files=all
```

If the worktree contains only Unity editor-generated noise from manual verification, clean it safely before work:

- restore only `unity/LLMGameCreatorAlpha/ProjectSettings/ProjectVersion.txt`;
- delete only untracked `.meta` files under `unity/LLMGameCreatorAlpha/Assets/**`;
- delete only untracked `unity/LLMGameCreatorAlpha/Packages/packages-lock.json`;
- delete only untracked `unity/LLMGameCreatorAlpha/ProjectSettings/*.asset`.

Do not use broad `git clean -fd -- unity/LLMGameCreatorAlpha/Assets`, because it can delete new Unity `.cs` files before staging. Do not delete untracked `.cs`, `.json`, `.md`, `.unity`, `.prefab`, or user files.

If there are other local changes, stop as BLOCKED and report them.

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
.llmgc/procedural/goal-119a-accepted-alpha-unity-material-warning-hotfix/accepted-alpha-unity-material-warning-hotfix-dashboard.json
.llmgc/procedural/goal-119-accepted-alpha-unity-playable-projection/accepted-alpha-unity-playable-projection-dashboard.json
.llmgc/procedural/goal-118-offline-geoworld-accepted-alpha-baseline-review/offline-geoworld-accepted-alpha-baseline-dashboard.json
unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDiagnostics.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-120-accepted-alpha-projection-usability-and-cleanup/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/clean-unity-editor-noise.ps1
.devflow/scripts/clean-unity-editor-noise.cmd
.llmgc/procedural/goal-120-accepted-alpha-projection-usability-and-cleanup/**
.llmgc/exports/goal-120-accepted-alpha-projection-usability-and-cleanup/**
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/accepted-alpha-projection-usability-and-cleanup.md
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal120.cs
unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDiagnostics.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionMarkerDescriptor.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionLegend.cs
tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
tests/LLMGameCreator.Tests/DevFlow/CleanUnityEditorNoiseScriptTests.cs
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

No live geodata, no providers, no Runtime/schema/Lua/generator-library, no final art/atlas, no saved scene/prefab assets, no ProjectSettings/Packages/StreamingAssets writes, and no release packaging.

## Primary deliverable: Unity usability/interaction pass

Improve the existing Goal119 projection so it is easier to inspect by hand.

### Required Unity UI additions

Extend `AcceptedAlphaPlayableProjectionWindow` with controls for:

```text
Focus Projection Camera
Select Player Proxy
Select Next Interaction Target
Select Next Objective
Select Diagnostics Marker
Toggle/Refresh Legend
```

It is acceptable if controls use `Selection.activeGameObject` and `SceneView.FrameLastActiveSceneView`.

### Required projection improvements

The projection must add:

1. A visible legend section under the generated root.
2. Marker descriptor components on generated markers with marker id/name, marker kind, source goal/file when known, display label, and optional status/details.
3. Better object organization: map markers, system markers, interaction markers, objective markers, legend/diagnostics.
4. Readable labels for player proxy, interaction targets, objectives, and diagnostics status.
5. A local usability smoke verifying root, player marker, legend, marker descriptors, selectable interaction target, selectable objective, diagnostics marker, and no material warning caused by projection build.
6. Safe `Clear Projection`: removes only `__LLMGC_AcceptedAlphaPlayableProjection__`.

Do not save the scene and do not create prefab/assets.

## Secondary deliverable: Unity cleanup script

Add:

```text
.devflow/scripts/clean-unity-editor-noise.ps1
.devflow/scripts/clean-unity-editor-noise.cmd
```

### Script behavior

`clean-unity-editor-noise.ps1` must:

- resolve repo root safely;
- support `-DryRun` and `-Apply`;
- default to dry-run if neither is provided;
- refuse to run if there are staged files unless `-AllowStaged` is passed;
- parse `git status --porcelain=v1 --untracked-files=all`;
- remove only:
  - untracked `.meta` files under `unity/LLMGameCreatorAlpha/Assets/**`;
  - untracked `unity/LLMGameCreatorAlpha/Packages/packages-lock.json`;
  - untracked `unity/LLMGameCreatorAlpha/ProjectSettings/*.asset`;
- restore only modified `unity/LLMGameCreatorAlpha/ProjectSettings/ProjectVersion.txt`;
- never remove untracked `.cs`, `.json`, `.md`, `.unity`, `.prefab`;
- print exactly what it would remove/restore;
- print final `git status --short --untracked-files=all`;
- exit nonzero if unexpected Unity changes remain after `-Apply`.

`clean-unity-editor-noise.cmd` must be a simple wrapper that runs the PowerShell script with `-Apply`.

### Script tests

Add focused tests that inspect the script text/behavior contract at minimum:

- contains DryRun/Apply modes;
- only deletes `.meta`, ProjectSettings `*.asset`, Packages `packages-lock.json`;
- refuses staged files by default;
- does not contain broad `git clean -fd -- unity/LLMGameCreatorAlpha/Assets`;
- does not delete `.cs`, `.json`, `.md`, `.unity`, `.prefab`.

If executing PowerShell in tests is already supported safely, add a dry-run fixture test; otherwise source-text tests are acceptable.

## Application evidence seam

Add/update BCL-only evidence under:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/
```

It must verify:

- Goal119A remains green;
- Goal120 usability features are present in source;
- cleanup script exists and passes script contract scan;
- Unity menu path remains present;
- projection root name remains present;
- material warning guard remains present;
- no forbidden path is expected.

## Required artifacts

Write deterministic artifacts under:

```text
.llmgc/procedural/goal-120-accepted-alpha-projection-usability-and-cleanup/
.llmgc/exports/goal-120-accepted-alpha-projection-usability-and-cleanup/
```

Recommended files:

```text
accepted-alpha-projection-usability-dashboard.json
accepted-alpha-projection-usability-script-inventory.json
accepted-alpha-projection-usability-smoke-plan.json
accepted-alpha-projection-cleanup-script-scan.json
accepted-alpha-projection-usability-report.md
accepted-alpha-projection-usability-negative-proof.json
accepted-alpha-projection-usability-file-index.json
unity-batchmode-projection-usability-smoke.log
```

## Visual World Stream Preview Workspace

Add a read-only Goal120 section showing:

```text
usabilityStatus
unityMenuPath
cleanupScriptPath
cleanupScriptCmdPath
legendPresent
markerDescriptorPresent
selectionControlsPresent
focusCameraControlPresent
materialWarningGuardPresent
unitySmokeStatus
doNotStartAutomatically
evidencePath
exportPath
```

## Unity batchmode smoke

Extend the existing batchmode smoke method or add:

```text
LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeProjectionUsabilitySmoke
```

It must build/refresh the projection and verify the Goal120 usability checks.

Run:

```powershell
Unity.exe -batchmode -quit -projectPath .\unity\LLMGameCreatorAlpha -executeMethod LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeProjectionUsabilitySmoke -logFile .\.llmgc\procedural\goal-120-accepted-alpha-projection-usability-and-cleanup\unity-batchmode-projection-usability-smoke.log
```

Log must contain:

```text
GOAL120_PROJECTION_USABILITY_SMOKE_PASS
```

and must not contain:

```text
Instantiating material due to calling renderer.material during edit mode
UnityEngine.Renderer:get_material()
```

## Docs/current state

Update current-state and queue docs so they clearly say:

- Goal120 improves the hands-on Unity projection usability and adds a safe Unity noise cleanup script.
- After manual Unity verification, the user can run `.devflow\scripts\clean-unity-editor-noise.cmd` or `.devflow\scripts\clean-unity-editor-noise.ps1 -Apply`.
- The next step should continue from hands-on Unity usability results, not another pure review goal.
- This is not final release and does not authorize forbidden lanes.

## Artifact-scope policy

Add scenario:

```text
goal-120-accepted-alpha-projection-usability-and-cleanup
```

It must allow only Goal120 expected files and exclude `.llmgc/manual/**`, Unity scenes/prefabs/settings/Packages/StreamingAssets, Runtime/schema/provider/Lua/generator-library.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~Goal120|FullyQualifiedName~VisualWorldStreamPreviewWorkspace|FullyQualifiedName~CleanUnityEditorNoiseScript"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-120-accepted-alpha-projection-usability-and-cleanup
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Run Unity batchmode usability smoke as described above.

After Unity runs, run:

```powershell
.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
git status --short --untracked-files=all
```

Only stage allowed files. The final worktree after push must be clean.

## Quality gate

GREEN requires:

- Unity batchmode usability smoke passes and logs `GOAL120_PROJECTION_USABILITY_SMOKE_PASS`.
- Material warning is absent.
- Projection usability controls/features are present.
- Cleanup script exists and passes contract scan.
- Cleanup script `-DryRun` runs.
- Cleanup script `-Apply` leaves no Unity-generated noise.
- No forbidden path changes.
- No `.llmgc/manual/**` staged/committed.
- Tests/checks pass.
- Artifact scope passes.
- Final git status is clean.

BLOCKED if Unity cannot run, cleanup script cannot safely distinguish noise from real files, or forbidden changes are required.

FAILED if build/tests break or source churn cannot be restored.

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
GREEN Goal 120 accepted alpha projection usability and cleanup
BLOCKED Goal 120 accepted alpha projection usability and cleanup
FAILED Goal 120 accepted alpha projection usability and cleanup
```

Final report must include commit SHA, Unity batchmode usability smoke result, cleanup script dry-run/apply result, how the user verifies by hand, exact cleanup command for after manual Unity checks, final git status, and remaining debt.
