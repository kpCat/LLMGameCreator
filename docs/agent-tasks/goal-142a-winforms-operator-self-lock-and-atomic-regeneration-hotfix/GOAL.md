# Goal 142A — WinForms Operator Self-Lock + Atomic Regeneration Hotfix

## Task ID

`goal-142a-winforms-operator-self-lock-and-atomic-regeneration-hotfix`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

P1 operator-workflow correctness hotfix.

Do not add a new product feature. Repair the Goal142 manual operator action so it can run successfully while the WinForms application itself is open.

## Human-observed failure

The user opened:

```text
Visual World Stream Preview
→ Goal142 Variants
→ Run Runtime Variant Matrix
```

The button returned:

```text
completed exitCode=1
```

The output showed `MSB3026`, `MSB3027`, and `MSB3021` copy failures because the running `LLMGameCreator.WinForms` process and Visual Studio locked DLL/PDB files in:

```text
src/LLMGameCreator.WinForms/bin/Debug/net8.0-windows/
```

The current button starts:

```text
powershell ... run-product-line-runtime-variant-matrix.ps1 -ApplyCleanup
```

The script deletes the canonical Goal142 procedural/export roots before invoking `dotnet test`. When that build/test fails because the running app locks its own output assemblies, the operator action both fails and may leave committed Goal142 artifacts deleted locally.

This is a real operator-action defect. Goal142 remains `accepted=false` until the corrected button succeeds.

## Required read-first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json

docs/agent-tasks/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/GOAL.md

.devflow/scripts/run-product-line-runtime-variant-matrix.ps1
.devflow/scripts/run-product-line-runtime-variant-matrix.cmd

src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantMatrixService.cs
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantMaterializer.cs
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantValidator.cs
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantScoringService.cs

src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal142.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs

.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/product-line-runtime-variant-matrix-dashboard.json
.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/product-line-runtime-variant-matrix-result.json
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-142a-winforms-operator-self-lock-and-atomic-regeneration-hotfix/**
.devflow/artifact-scope/artifact-scope-policy.json

.devflow/scripts/run-product-line-runtime-variant-matrix.ps1
.devflow/scripts/run-product-line-runtime-variant-matrix.cmd

.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/**
.llmgc/exports/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/**
.llmgc/procedural/goal-142a-winforms-operator-self-lock-and-atomic-regeneration-hotfix/**
.llmgc/exports/goal-142a-winforms-operator-self-lock-and-atomic-regeneration-hotfix/**

src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/**
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineRuntimeVariantMatrixModels.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**

src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal142.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs

src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Runtime/RuntimeBackedPlayerCommandRoundtripService.cs

tests/LLMGameCreator.Tests/Application/ProductLineRuntimeVariantMatrix/**
tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/ProductLineRuntimeVariantMatrixScriptProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal142Tests.cs
tests/LLMGameCreator.Tests/Devflow/RunProductLineRuntimeVariantMatrixScriptTests.cs


docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/runtime-significant-product-line-variant-matrix-and-selection-handoff.md
```

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
samples/minimal-map-game/**
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/**
.llmgc/exports/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/**
.llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/**
.llmgc/exports/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/**
.llmgc/procedural/goal-141a-player-command-roundtrip-semantic-correctness-hotfix/**
.llmgc/exports/goal-141a-player-command-roundtrip-semantic-correctness-hotfix/**

src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.AssetPipeline/**
src/LLMGameCreator.Scripting/**
provider / LLM / RAG / media provider code
Lua / Scripting code
generator-library/**
unity/**

*.sln
*.csproj
Directory.Build.*
dependency/package files
```

No Unity work. No public GamePackage schema change. No sample mutation.

## Required fix 1 — WinForms button must not rebuild its own running executable

Replace the Goal142 button workflow that launches the PowerShell proof script with an operator-safe execution path.

Preferred implementation:

```text
WinForms button
→ Application-level ProductLineRuntimeVariantMatrixOperatorRunner
→ ProductLineRuntimeVariantMatrixService.BuildAndWriteAsync(...)
→ refresh workspace
```

The running WinForms application must not invoke `dotnet build` or `dotnet test` against `LLMGameCreator.WinForms.csproj` or a dependency graph that copies assemblies into the running WinForms output directory.

The button must remain:

```text
Run Runtime Variant Matrix
```

Required behavior:

- asynchronous;
- button disabled while running;
- no UI freeze;
- no child compiler/test process;
- no DLL/PDB copy into the currently running WinForms output directory;
- status shows `running`, then `completed` or `failed`;
- bounded output/status details;
- workspace refreshed after success;
- exit/result status mapped honestly;
- exceptions handled without crashing the application.

The external `.cmd`/PowerShell command remains available for automation and Codex validation, but the WinForms operator button must not use that build/test path.

## Required fix 2 — failure-safe artifact regeneration

The current script must not delete canonical Goal142 procedural/export artifacts before a potentially failing build/test.

Remove the destructive sequence:

```text
-ApplyCleanup
→ delete Goal142 roots
→ run dotnet test
```

Required external-script behavior:

1. Preflight and validate inputs.
2. Run required build/test/proof work before destructive cleanup, or use a transactional backup/restore strategy.
3. Preserve the last valid Goal142 artifacts when build/test/proof fails.
4. Only remove stale artifacts after a successful new matrix generation.
5. On failure, return non-zero and leave the previous valid dashboard/result/handoff intact.
6. On success, regenerate deterministic Goal142 artifacts and leave git clean.

## Required operator transaction guard

Add an Application-level operator runner, recommended:

```text
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantMatrixOperatorRunner.cs
```

It must preserve the previous valid procedural/export state if matrix generation throws.

Acceptable implementation:

- snapshot existing Goal142 procedural/export roots to temporary directories outside the repo;
- run the in-process matrix service;
- on success, remove temporary backups;
- on failure, restore the previous roots exactly;
- clean temporary backups in `finally`;
- do not leave `.tmp`, `.bak`, or work directories in the repo.

If no previous artifacts existed, a failed run must not leave a misleading partial GREEN dashboard/handoff.

## Required recovery of current local state

The user's failed manual run may already have removed tracked Goal142 artifacts locally.

The task must tolerate that state:

- do not assume Goal142 artifact files currently exist;
- regenerate the canonical procedural/export Goal142 artifacts deterministically;
- do not use broad `git restore` over unrelated files;
- final tracked Goal142 artifacts must match the corrected deterministic generation;
- final git status must be clean after commit/push.

## Manual failure record

Update:

```text
docs/manual-acceptance/runtime-significant-product-line-variant-matrix-and-selection-handoff.md
```

Record the failed attempt as an operator-run defect, not as Goal142 acceptance:

```text
manualAttemptObserved=true
manualAttemptAccepted=false
manualAttemptExitCode=1
failureClass=winforms_self_lock_build_copy
lockedByRunningWinForms=true
lockedByVisualStudio=true
artifactsMayHaveBeenRemovedBeforeFailure=true
```

Goal142 remains `accepted=false`.

## Required proof

Automated proof must demonstrate:

```text
operatorUsesInProcessService=true
operatorStartsCompilerProcess=false
operatorStartsDotnetTestProcess=false
runningWinFormsOutputCopyAttempt=false
buttonDisabledWhileRunning=true
workspaceRefreshedAfterSuccess=true
previousArtifactsPreservedOnFailure=true
partialArtifactsRemovedOrRolledBackOnFailure=true
successfulRunRegeneratesGoal142Artifacts=true
matrixStatus=GREEN
candidateCount=4
passedCandidateCount=4
distinctFinalStateHashCount=4
selectedCandidateId=minimal-map-game-exploration-resource-focus
sourceTemplateUnmodified=true
goal142Accepted=false
```

Add a failure-injection test that throws during generation after at least one attempted write and proves the previous dashboard/result/handoff bytes are restored unchanged.

Add a static/source proof that the Goal142 WinForms button no longer builds a `ProcessStartInfo("powershell")` or launches the Goal142 script.

## Goal142A artifacts

Write under procedural and export roots:

```text
operator-self-lock-hotfix-dashboard.json
operator-self-lock-hotfix-regression-proof.json
operator-self-lock-hotfix-file-index.json
operator-self-lock-hotfix-report.md
```

Required fields:

```text
status=GREEN
operatorUsesInProcessService=true
operatorStartsCompilerProcess=false
operatorStartsDotnetTestProcess=false
previousArtifactsPreservedOnFailure=true
successfulRunRegeneratesGoal142Artifacts=true
manualAttemptFailureRecorded=true
goal142Accepted=false
```

## Current state

Keep:

```text
goal142Accepted=false
accepted=false
```

Add:

```text
goal142OperatorSelfLockFixed=true
goal142OperatorUsesInProcessService=true
goal142OperatorTransactionalRegeneration=true
goal142LastManualAttemptExitCode=1
goal142ManualRetryRequired=true
```

Do not request Goal142 acceptance automatically. The user must retry the one button after this hotfix.

## Artifact-scope scenario

Add:

```text
goal-142a-winforms-operator-self-lock-and-atomic-regeneration-hotfix
```

## Validation

Before validation, the running WinForms application and Visual Studio debugging session may need to be closed to release existing locks. This is an execution precondition for Codex validation, not a workaround for the corrected operator button.

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~ProductLineRuntimeVariantMatrix|FullyQualifiedName~Goal142|FullyQualifiedName~Goal142A|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\run-product-line-runtime-variant-matrix.ps1 -DryRun
.\.devflow\scripts\run-product-line-runtime-variant-matrix.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-142a-winforms-operator-self-lock-and-atomic-regeneration-hotfix

git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Forbidden diff:

```powershell
git diff --name-only -- samples/minimal-map-game .llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion .llmgc/exports/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion .llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge .llmgc/exports/goal-141-runtime-backed-unity-player-command-roundtrip-bridge .llmgc/procedural/goal-141a-player-command-roundtrip-semantic-correctness-hotfix .llmgc/exports/goal-141a-player-command-roundtrip-semantic-correctness-hotfix src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity
```

Also check changed files for mojibake and escaped Cyrillic markers.

## Quality gate

GREEN requires:

- the running WinForms button no longer launches a build/test process;
- operator run uses the in-process Application service;
- failed regeneration preserves previous artifacts byte-for-byte;
- successful regeneration restores Goal142 canonical artifacts;
- external script is no longer destructive-before-proof;
- Goal142 matrix remains GREEN after regeneration;
- Goal142 remains unaccepted pending manual retry;
- tests/checks/artifact scope pass;
- no forbidden changes;
- no `.llmgc/manual/**` tracked/staged;
- final git status clean.

BLOCKED if the current Application service cannot be safely invoked in-process without a bounded refactor.

FAILED if the button still launches PowerShell/dotnet build/test, or if a failed run can remove the last valid Goal142 artifacts.

## Commit / push policy

Commit and push with one of:

```text
GREEN Goal 142A WinForms operator self-lock and atomic regeneration hotfix
BLOCKED Goal 142A WinForms operator self-lock and atomic regeneration hotfix
FAILED Goal 142A WinForms operator self-lock and atomic regeneration hotfix
```

Final report must include:

- commit SHA;
- exact operator execution path;
- proof no compiler/test child process is started by the button;
- rollback/failure-injection proof;
- regenerated Goal142 dashboard values;
- manual failure record status;
- forbidden-zone confirmation;
- final git status.
