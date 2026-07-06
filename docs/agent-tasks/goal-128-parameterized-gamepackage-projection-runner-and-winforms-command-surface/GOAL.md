# Goal 128 — Parameterized GamePackage Projection Runner + WinForms Command Surface

## Task ID

`goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product goal.

This is not an evidence-only/review task. The deliverable is a parameterized verification path: the Unity projection runner must no longer be hardwired only to the default sample package. It must accept a package path, pass it into Unity batchmode, run the full playthrough projection for that package, and surface the latest result in the WinForms/VisualWorld workspace.

## Why this goal exists

Goal123-126 proved a projection-only full playthrough over `samples/minimal-map-game/package.json`.
Goal127 made verification scriptable through `.devflow\scripts\run-unity-projection-verification.cmd`.

But the product cannot stay hardcoded to one sample. Goal128 must make the runner/package path parameterized so future generated packages can be verified through the same path.

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

.devflow/scripts/run-unity-projection-verification.ps1
.devflow/scripts/run-unity-projection-verification.cmd
.devflow/scripts/clean-unity-editor-noise.ps1

samples/minimal-map-game/package.json

.llmgc/procedural/goal-127-winforms-unity-projection-verification-runner/unity-projection-verification-runner-result.json
.llmgc/procedural/goal-127-winforms-unity-projection-verification-runner/unity-projection-verification-runner-dashboard.json
.llmgc/procedural/goal-126-generic-gamepackage-full-playthrough-projection/generic-gamepackage-full-playthrough-dashboard.json

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionPlaythrough.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionState.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-unity-projection-verification.ps1
.devflow/scripts/run-unity-projection-verification.cmd

.llmgc/procedural/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface/**
.llmgc/exports/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/parameterized-gamepackage-projection-runner-and-winforms-command-surface.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal128.cs

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionPlaythrough.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionState.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/DevFlow/RunUnityProjectionVerificationScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
samples/minimal-map-game/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
unity/LLMGameCreatorAlpha/Assets/Scenes/**
unity/LLMGameCreatorAlpha/Assets/**/*.unity
unity/LLMGameCreatorAlpha/Assets/**/*.prefab
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.AssetPipeline/**
src/LLMGameCreator.Scripting/**
generator-library/**
*.sln
*.csproj
Directory.Build.*
dependency/package files
```

No Runtime/schema/provider/Lua/generator-library work. Do not mutate sample package. Do not save scenes/prefabs. Do not write StreamingAssets.

## Primary deliverable A — parameterized devflow runner

Extend `.devflow/scripts/run-unity-projection-verification.ps1`:

1. Add `-PackagePath` parameter.
2. Default must remain `samples/minimal-map-game/package.json`.
3. Accept repo-relative or absolute package paths.
4. Resolve to a canonical full path.
5. Reject missing files.
6. Reject paths outside repo root.
7. Reject `.llmgc/manual/**`.
8. Do not mutate the package file.
9. Pass the package path to Unity batchmode via a deterministic custom argument, for example:

```text
-llmgcPackagePath <full-package-path>
```

10. Use a Goal128 batchmode method and pass marker:

```text
LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeParameterizedGamePackageFullPlaythroughSmoke
GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS
GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL
```

11. The result JSON must include at least:

```text
mode
packagePath
packagePathRelative
unityPath
unityExitCode
passMarkerPresent
failMarkerAbsent
materialWarningAbsent
cleanupApplied
cleanupExitCode
passed
logPath
```

12. `.cmd` should keep the normal default call working:

```text
.devflow\scripts\run-unity-projection-verification.cmd
```

and still accept extra parameters passed through to `.ps1`.

## Primary deliverable B — Unity package path resolution

Update the Unity generic projection path so it can read the package path passed by the runner.

Required:

1. Add a deterministic package path resolver in allowed Unity projection code.
2. It must read `-llmgcPackagePath` from `Environment.GetCommandLineArgs()` or equivalent.
3. If no argument is provided, it must fall back to `samples/minimal-map-game/package.json`.
4. It must resolve repo-relative and full paths.
5. It must reject package paths outside repo root.
6. It must reject `.llmgc/manual/**`.
7. It must leave existing Goal126 full-playthrough behavior intact for the default sample.
8. It must surface selected package path in diagnostics/event transcript.
9. It must not write files, not mutate the package, not touch StreamingAssets/scenes/prefabs/settings.

## Primary deliverable C — parameterized batchmode smoke

Add Unity batchmode entrypoint:

```text
LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeParameterizedGamePackageFullPlaythroughSmoke
```

It should run the same full playthrough projection but through the parameterized package path resolver.

Required smoke fields:

```text
parameterizedRunnerPassed=True
packagePathResolved=True
packagePathUnderRepo=True
samplePackageLoaded=True
fullPlaythroughPassed=True
eventTranscriptPresent=True
zeroFatalErrors=True
```

## Primary deliverable D — WinForms/VisualWorld command surface

Add a read-only Goal128 section in Visual World Stream Preview / WinForms that surfaces:

```text
parameterizedRunnerStatus
packagePath
packagePathRelative
normalCommand
exampleCommandWithPackagePath
resultPath
logPath
unityExitCode
passMarkerPresent
cleanupApplied
manualUnityOptional
projectionOnly
```

This can be read-only; do not add complex asynchronous process execution unless it is low-risk and well-tested. The critical product improvement is that the command itself is now parameterized and visible from the workspace.

## Application evidence seam

Add/update BCL-only evidence under:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/
```

It must verify:

- Goal127 remains green.
- Runner script accepts `-PackagePath`.
- Runner script passes `-llmgcPackagePath` to Unity.
- Unity code has parameterized package path resolver.
- Goal128 batchmode log contains `GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS`.
- Result JSON includes package path fields.
- Cleanup script remains integrated.
- No forbidden path is expected.

## Required artifacts

Create deterministic artifacts under:

```text
.llmgc/procedural/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface/
.llmgc/exports/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface/
```

Recommended files:

```text
parameterized-gamepackage-runner-dashboard.json
parameterized-gamepackage-runner-script-scan.json
parameterized-gamepackage-runner-log-scan.json
parameterized-gamepackage-runner-result.json
parameterized-gamepackage-runner-report.md
parameterized-gamepackage-runner-negative-proof.json
parameterized-gamepackage-runner-file-index.json
unity-batchmode-parameterized-gamepackage-full-playthrough.log
```

## Docs/current state

Update current-state/queue docs so they clearly say:

- Goal128 parameterizes the Unity projection runner with `-PackagePath`.
- Normal verification remains `.devflow\scripts\run-unity-projection-verification.cmd`.
- Manual Unity inspection remains optional.
- This does not authorize sample mutation, Runtime/provider/schema/Lua/generator-library/final-art/atlas/Unity scene/prefab/project-settings/StreamingAssets/release work.

## Artifact-scope policy

Add scenario:

```text
goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface
```

It must allow only Goal128 expected files and exclude `.llmgc/manual/**`, `samples/minimal-map-game/**`, Unity scenes/prefabs/settings/Packages/StreamingAssets, Runtime/schema/provider/Lua/generator-library.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~Goal128|FullyQualifiedName~VisualWorldStreamPreviewWorkspace|FullyQualifiedName~RunUnityProjectionVerificationScript"
.\.devflow\scripts\run-unity-projection-verification.ps1 -Mode GenericFullPlaythrough -PackagePath samples/minimal-map-game/package.json -DryRun
.\.devflow\scripts\run-unity-projection-verification.ps1 -Mode GenericFullPlaythrough -PackagePath samples/minimal-map-game/package.json -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

After Unity batchmode/runner validation, run cleanup:

```powershell
.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
git status --short --untracked-files=all
```

Only stage allowed files. Final status must be clean.

## Quality gate

GREEN requires:

- runner accepts and validates `-PackagePath`;
- runner passes the path to Unity through `-llmgcPackagePath` or equivalent;
- Unity uses the parameterized package path and falls back to the default sample;
- runner result JSON includes package path fields;
- batchmode log contains `GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS`;
- no sample package mutation;
- no forbidden path changes;
- no `.llmgc/manual/**` staged/tracked;
- tests/checks pass;
- artifact scope passes;
- final git status clean.

BLOCKED if Unity cannot honestly run or package path parameterization cannot be verified.

FAILED if build/tests break or forbidden changes are required.

## Commit / push policy

Before commit:

```powershell
git diff --cached --name-only
git diff --cached --check
git diff --cached --name-only | Select-String -SimpleMatch ".llmgc/manual"
```

Commit and push with one of:

```text
GREEN Goal 128 parameterized gamepackage projection runner and winforms command surface
BLOCKED Goal 128 parameterized gamepackage projection runner and winforms command surface
FAILED Goal 128 parameterized gamepackage projection runner and winforms command surface
```

Final report must include commit SHA, runner dry-run/apply results, package path used, Unity log path, cleanup result, changed files grouped by area, final git status, and remaining debt.
