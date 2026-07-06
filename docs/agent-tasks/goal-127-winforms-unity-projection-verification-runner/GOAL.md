# Goal 127 — WinForms/Devflow Unity Projection Verification Runner

## Task ID

`goal-127-winforms-unity-projection-verification-runner`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive workflow/product goal.

This is not a new proof-only/review goal. Goal126 produced a one-click Unity full playthrough projection, but the user still has to open Unity manually to verify it. Goal127 must create a repo-local and WinForms-visible runner that can execute the Unity batchmode full-playthrough verification and cleanup without manual Unity clicking.

The primary deliverable is a practical verification workflow:

```text
WinForms / Devflow runner -> Unity batchmode full playthrough smoke -> log scan -> cleanup -> concise result
```

Manual Unity inspection remains optional, not required after every goal.

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
.devflow/scripts/clean-unity-editor-noise.ps1
.devflow/scripts/clean-unity-editor-noise.cmd

.llmgc/procedural/goal-126-generic-gamepackage-full-playthrough-projection/generic-gamepackage-full-playthrough-dashboard.json
.llmgc/procedural/goal-126-generic-gamepackage-full-playthrough-projection/generic-gamepackage-full-playthrough-log-scan.json
.llmgc/procedural/goal-125-generic-gamepackage-systems-loop-projection/generic-gamepackage-systems-loop-dashboard.json

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionPlaythrough.cs

src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-127-winforms-unity-projection-verification-runner/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-unity-projection-verification.ps1
.devflow/scripts/run-unity-projection-verification.cmd

.llmgc/procedural/goal-127-winforms-unity-projection-verification-runner/**
.llmgc/exports/goal-127-winforms-unity-projection-verification-runner/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/winforms-unity-projection-verification-runner.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal127.cs

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
unity/LLMGameCreatorAlpha/Assets/**
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**
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

No Unity source changes for this goal. Unity execute methods already exist from Goal126. No Runtime/schema/provider/Lua/generator-library work. No sample package mutation. No scene/prefab/StreamingAssets/ProjectSettings/Packages changes.

## Primary deliverable A — Devflow Unity projection runner

Add:

```text
.devflow/scripts/run-unity-projection-verification.ps1
.devflow/scripts/run-unity-projection-verification.cmd
```

The PowerShell script must:

1. Resolve repo root safely using existing devflow common helpers if available.
2. Support at least:

```powershell
-Mode GenericFullPlaythrough
-UnityPath <path optional>
-DryRun
-ApplyCleanup
```

3. Default mode may be `GenericFullPlaythrough`.
4. Locate Unity in this order:
   - explicit `-UnityPath`;
   - `Unity.exe` on PATH;
   - installed Unity 6000.1.10f1 path:
     `C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe`.
5. Run Unity batchmode with:

```text
-executeMethod LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageFullPlaythroughSmoke
```

6. Write log to:

```text
.llmgc/procedural/goal-127-winforms-unity-projection-verification-runner/unity-batchmode-generic-full-playthrough-runner.log
```

7. Scan log for:

```text
GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS
```

8. Fail if log contains:

```text
GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL
Instantiating material due to calling renderer.material during edit mode
UnityEngine.Renderer:get_material()
```

9. Run bounded cleanup after Unity when `-ApplyCleanup` is passed:

```powershell
.devflow/scripts/clean-unity-editor-noise.ps1 -Apply
```

10. Write a compact JSON result:

```text
.llmgc/procedural/goal-127-winforms-unity-projection-verification-runner/unity-projection-verification-runner-result.json
```

Required fields:

```text
mode
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

11. In `-DryRun`, print the exact Unity command and cleanup command, but do not run Unity and do not delete anything.
12. Exit 0 only when verification is green.

The `.cmd` wrapper must execute the PowerShell script in apply mode for normal users, for example:

```bat
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-unity-projection-verification.ps1" -Mode GenericFullPlaythrough -ApplyCleanup %*
```

## Primary deliverable B — WinForms visibility / command surface

Add a Goal127 read-only section to Visual World Stream Preview Workspace.

Show at least:

```text
runnerStatus
runnerScriptPath
runnerCmdPath
mode
unityExecuteMethod
lastResultPath
lastLogPath
passMarkerPresent
cleanupScriptAvailable
cleanupCommand
manualUnityClickingRequired
```

Also add a copyable command text in the UI or report output:

```text
.devflow\scripts\run-unity-projection-verification.cmd
```

Do not block on a full process-launching WinForms implementation if it would create UI threading risk. It is acceptable for Goal127 to provide a clear one-command runner and WinForms visibility/reporting. If adding a safe async button is low risk, add it; otherwise do not.

## Primary deliverable C — Application evidence seam

Add/update BCL-only evidence under:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/
```

It must verify:

- Goal126 full playthrough evidence is green.
- Runner script exists.
- CMD wrapper exists.
- Script contains the required execute method and pass/fail marker scan.
- Script contains no broad `git clean` and delegates cleanup to bounded cleanup script.
- Script does not mutate sample package, Unity project settings, Runtime, schema, providers, Lua or generator-library.
- Runner result/log artifacts exist after validation.

## Required artifacts

Create deterministic artifacts under:

```text
.llmgc/procedural/goal-127-winforms-unity-projection-verification-runner/
.llmgc/exports/goal-127-winforms-unity-projection-verification-runner/
```

Recommended files:

```text
unity-projection-verification-runner-dashboard.json
unity-projection-verification-runner-script-scan.json
unity-projection-verification-runner-result.json
unity-projection-verification-runner-log-scan.json
unity-projection-verification-runner-report.md
unity-projection-verification-runner-negative-proof.json
unity-projection-verification-runner-file-index.json
unity-batchmode-generic-full-playthrough-runner.log
```

## Visual World Stream Preview Workspace

Add a read-only Goal127 section showing:

```text
runnerStatus
mode
unityExecuteMethod
passMarkerPresent
cleanupApplied
cleanupScriptAvailable
manualUnityClickingRequired=false
runnerCommand
resultPath
logPath
evidencePath
exportPath
```

## Docs/current state

Update current-state and queue docs so they clearly say:

- Goal127 adds a repo-local and WinForms-visible Unity projection verification runner.
- The user no longer needs to open Unity manually after every goal.
- Normal verification command is:

```text
.devflow\scripts\run-unity-projection-verification.cmd
```

- Manual Unity inspection remains optional.
- This still does not authorize Runtime/schema/provider/Lua/generator-library/final-art/atlas/Unity scene/prefab/project-settings/StreamingAssets/release work.

## Artifact-scope policy

Add scenario:

```text
goal-127-winforms-unity-projection-verification-runner
```

It must allow only Goal127 expected files and exclude `.llmgc/manual/**`, samples/minimal-map-game, Unity files, Runtime/schema/provider/Lua/generator-library.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~Goal127|FullyQualifiedName~VisualWorldStreamPreviewWorkspace|FullyQualifiedName~RunUnityProjectionVerificationScript"
.\.devflow\scripts\run-unity-projection-verification.ps1 -Mode GenericFullPlaythrough -DryRun
.\.devflow\scripts\run-unity-projection-verification.ps1 -Mode GenericFullPlaythrough -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-127-winforms-unity-projection-verification-runner
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

After Unity batchmode, run cleanup:

```powershell
.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
git status --short --untracked-files=all
```

Only stage allowed files. Final status must be clean.

## Quality gate

GREEN requires:

- runner dry-run prints commands and exits 0;
- runner apply executes Unity batchmode full playthrough and exits 0;
- runner result JSON says `passed=true`;
- log contains `GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS`;
- forbidden markers are absent;
- cleanup is applied and leaves no Unity editor noise;
- WinForms/VisualWorld Goal127 section exists;
- no forbidden path changes;
- no `.llmgc/manual/**` staged/tracked;
- tests/checks pass;
- artifact scope passes;
- final git status clean.

BLOCKED if Unity cannot run or the runner cannot safely execute/scan/cleanup.

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
GREEN Goal 127 winforms unity projection verification runner
BLOCKED Goal 127 winforms unity projection verification runner
FAILED Goal 127 winforms unity projection verification runner
```

Final report must include commit SHA, runner dry-run/apply results, Unity log path, cleanup result, manual optional verification path, changed files grouped by area, final git status, and remaining debt.
