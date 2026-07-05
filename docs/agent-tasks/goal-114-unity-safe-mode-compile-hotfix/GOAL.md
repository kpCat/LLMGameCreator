# Goal 114 — Unity Safe Mode Compile Hotfix for Offline Geoworld Alpha Acceptance

## Status intent

This is a P0 hotfix/audit goal, not a new feature goal.

The user attempted the real Goal110/111/112/113 manual Unity acceptance flow and Unity opened the project in Safe Mode because the project has compile errors.

Commit status at the end must be one of:

- `GREEN Goal 114 unity safe mode compile hotfix`
- `BLOCKED Goal 114 unity safe mode compile hotfix`
- `FAILED Goal 114 unity safe mode compile hotfix`

Push to `origin/main` when committed.

## Repository

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`

## Human-observed blocker

Unity Safe Mode / Console showed compile blockers in `unity/LLMGameCreatorAlpha`:

```text
Assets\Scripts\OfflineGeoworldAlphaAcceptanceResult.cs(58,20): error CS0103: The name 'JsonUtility' does not exist in the current context
Assets\Scripts\OfflineGeoworldAlphaAcceptanceResultStore.cs(64,26): error CS0103: The name 'JsonUtility' does not exist in the current context
Assets\Scripts\OfflineGeoworldAlphaSliceCoordinator.cs(100,31): error CS1061: 'OfflineGeoworldPreviewRunner' does not contain a definition for 'RefreshPayloadStatus'
Assets\Scripts\OfflineGeoworldAlphaSliceCoordinator.cs(105,42): error CS1061: 'OfflineGeoworldPlayModeTravelController' does not contain a definition for 'RefreshPayloadStatus'
Assets\Scripts\OfflineGeoworldAlphaSliceCoordinator.cs(110,45): error CS1061: 'OfflineGeoworldInteractiveTravelController' does not contain a definition for 'RefreshPayloadStatus'
Assets\Scripts\OfflineGeoworldAlphaSliceCoordinator.cs(115,39): error CS1061: 'OfflineGeoworldInteractionController' does not contain a definition for 'RefreshPayloadStatus'
Assets\Scripts\OfflineGeoworldSessionSaveLoadController.cs(143,24): error CS0103: The name 'JsonUtility' does not exist in the current context
Assets\Scripts\OfflineGeoworldSessionSaveLoadController.cs(161,28): error CS0103: The name 'JsonUtility' does not exist in the current context
```

Warnings about unused fields are not the P0. Fix compile errors first.

## Read first

Read these files before editing:

```text
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/RELEASE_RISK_REGISTER.md
docs/MILESTONE_GATES.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
.devflow/artifact-scope/artifact-scope-policy.json

unity/LLMGameCreatorAlpha/ProjectSettings/ProjectVersion.txt
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResult.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResultStore.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionSaveLoadController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaSliceCoordinator.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractiveTravelController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionController.cs
```

Also inspect nearby tests/product-smoke patterns for Unity source scanning:

```text
tests/LLMGameCreator.Tests/ProductSmoke/
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/
```

## Allowed files

You may change only:

```text
.devflow/artifact-scope/artifact-scope-policy.json

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/agent-tasks/goal-114-unity-safe-mode-compile-hotfix/**

.llmgc/procedural/goal-114-unity-safe-mode-compile-hotfix/**
.llmgc/exports/goal-114-unity-safe-mode-compile-hotfix/**

unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResult.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResultStore.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionSaveLoadController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaSliceCoordinator.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractiveTravelController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionController.cs

tests/LLMGameCreator.Tests/ProductSmoke/UnitySafeModeCompileHotfixProductSmokeTests.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceServiceTests.cs
```

Only touch `VisualWorldStreamPreviewWorkspaceServiceTests.cs` if needed for a very small assertion update. Prefer a new focused product smoke test.

## Forbidden files and zones

Do not change:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
public GamePackage schema files
providers / LLM / RAG / media provider code
Lua / Scripting
generator-library/**
*.sln
*.csproj
Directory.Build.*
NuGet/config/dependency files

unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
unity/LLMGameCreatorAlpha/Assets/Scenes/**
unity/LLMGameCreatorAlpha/Assets/**/*.prefab
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/**

.llmgc/manual/**
LFZ archive/source
```

Do not create or commit a real manual acceptance result.
Do not mark Alpha accepted.
Do not set accepted=true in docs/state/evidence except inside explicitly synthetic/non-acceptance samples if absolutely required.

## Exact required behavior

### A. Repair `JsonUtility` compile blockers

The Unity project currently fails to compile on `JsonUtility` references in:

```text
OfflineGeoworldAlphaAcceptanceResult.cs
OfflineGeoworldAlphaAcceptanceResultStore.cs
OfflineGeoworldSessionSaveLoadController.cs
```

Fix this without adding packages/dependencies and without project-file changes.

Preferred robust approach:

1. Do not rely on `JsonUtility` in these acceptance/session helper scripts.
2. Add small local deterministic JSON helpers in the same files or as private methods:
   - string escaping;
   - object serialization for the concrete acceptance-result and session-snapshot shapes;
   - minimal load/parse for the concrete shape required by `LoadResult` / `LoadSnapshot`.
3. Keep saved JSON valid and readable.
4. Keep `OfflineGeoworldAlphaAcceptanceResult.ToJson()` returning valid JSON with all fields needed by Goal111:
   - `goalId`, `manualGate`, `accepted`, `manualAcceptancePending`, `automatedGatePassed`, `resultStatus`, `checklistHash`, `resultTemplateHash`, `packagePath`, `diagnostics`, `resultHash`, `steps`.
5. Keep `OfflineGeoworldAlphaAcceptanceResultStore.LoadResult()` functional enough to load a saved local pending result and report `result.resultStatus` and `result.steps.Count`.
6. Keep `OfflineGeoworldSessionSaveLoadController.SaveSnapshot()` and `LoadSnapshot()` functional enough for the Goal106 save/load/replay acceptance flow.

If you choose instead to fully qualify `UnityEngine.JsonUtility`, you must prove it compiles in Unity. Since the user’s Unity Safe Mode says `JsonUtility` is missing despite `using UnityEngine`, the safer route is to remove these references from the affected files.

### B. Repair `RefreshPayloadStatus` API mismatch

`OfflineGeoworldAlphaSliceCoordinator.VerifySlice()` calls `RefreshPayloadStatus()` on several components. The corresponding classes currently expose differently named refresh methods.

Fix with low-risk compatibility wrappers, not a broad coordinator rewrite:

- `OfflineGeoworldPreviewRunner.RefreshPayloadStatus()` should call existing `Refresh()`.
- `OfflineGeoworldPlayModeTravelController.RefreshPayloadStatus()` should call existing `RefreshPayload()`.
- `OfflineGeoworldInteractiveTravelController.RefreshPayloadStatus()` should call existing `RefreshPayload()`.
- `OfflineGeoworldInteractionController.RefreshPayloadStatus()` should call existing payload refresh method if present, or the correct local equivalent after inspection.

Keep existing public methods; add wrappers rather than renaming existing API.

### C. Preserve manual acceptance state

This hotfix must not advance the manual gate.

Expected post-hotfix state remains:

```text
offline_geoworld_alpha_manual_acceptance_verification required
accepted=false
manual result missing until human run
Goal113 workbench ready pending human result
```

### D. Evidence artifacts

Write compact evidence under:

```text
.llmgc/procedural/goal-114-unity-safe-mode-compile-hotfix/
.llmgc/exports/goal-114-unity-safe-mode-compile-hotfix/
```

Required evidence files:

```text
unity-safe-mode-compile-hotfix-report.md
unity-safe-mode-compile-hotfix-dashboard.json
unity-safe-mode-compile-hotfix-source-scan.json
unity-safe-mode-compile-hotfix-negative-proof.json
unity-safe-mode-compile-hotfix-file-index.json
```

The source scan must record:

- affected Unity files;
- `JsonUtility` reference count after fix in affected scripts;
- presence of wrapper methods;
- no `AlphaRuntimeBootstrap.cs` changes;
- no scene/prefab/project/package/settings changes;
- no `.llmgc/manual/**` writes;
- manual gate remains open.

### E. WinForms/docs visibility

Do not build a new UI panel unless trivial. This is a compile hotfix. It is enough to update current state, queue, risk/debt docs, context index and compact evidence.

## Tests and validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
```

Add a focused product smoke test if practical:

```text
tests/LLMGameCreator.Tests/ProductSmoke/UnitySafeModeCompileHotfixProductSmokeTests.cs
```

The test should inspect repository files as text and assert at minimum:

- affected Unity scripts no longer contain unqualified `JsonUtility.` references;
- required `RefreshPayloadStatus` wrappers exist;
- `AlphaRuntimeBootstrap.cs` is not part of Goal114 expected changed paths/evidence;
- `.llmgc/manual/` is not created by the hotfix;
- Goal114 evidence exists and states manual gate still open.

Run a focused filter, then current-goal/spine checks:

```powershell
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~UnitySafeModeCompileHotfix|FullyQualifiedName~Goal113|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.devflow\scripts\check-current-goal.ps1
.devflow\scripts\check-spine-fast.ps1
.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-114-unity-safe-mode-compile-hotfix
git diff --check
git diff --cached --check
```

If Unity editor is available locally, also run or manually verify:

```powershell
$unity = "C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe"
if (Test-Path $unity) {
  & $unity -batchmode -quit -projectPath .\unity\LLMGameCreatorAlpha -logFile .\.llmgc\procedural\goal-114-unity-safe-mode-compile-hotfix\unity-batchmode-compile.log
}
```

If Unity batchmode is not available or times out, do not fabricate success. Record it as `unityBatchmodeCompileObserved=false` and leave the hotfix based on source-level repair plus .NET/product smoke gates.

## Quality gate

Stop as BLOCKED if:

- Unity compile errors require changing forbidden files;
- a fix would require `.sln/.csproj` or dependency changes;
- `AlphaRuntimeBootstrap.cs` must be edited;
- Unity scenes/prefabs/ProjectSettings/Packages must be edited;
- `.llmgc/manual/**` must be created;
- manual acceptance would need to be fabricated;
- source health exceeds 700 logical lines in any new/changed C# file, or any file exceeds 1000 logical lines.

## Final report

Report:

```text
Status: GREEN/BLOCKED/FAILED
Commit:
Push:
Unity Safe Mode errors addressed:
JsonUtility references removed/repaired:
RefreshPayloadStatus wrappers added:
Unity files changed:
Forbidden zones untouched:
Manual result created/committed: no
Manual gate status:
Validation commands/results:
Unity batchmode/manual compile observation:
Artifact-scope result:
Source-health max lines:
Remaining debt:
```
