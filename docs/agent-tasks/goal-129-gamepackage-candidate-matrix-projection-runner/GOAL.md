# Goal 129 — GamePackage Candidate Matrix Projection Runner

## Task ID

`goal-129-gamepackage-candidate-matrix-projection-runner`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product goal.

This is not an evidence-only or review goal. The primary deliverable is a repo-local candidate GamePackage matrix and one-command projection verification over multiple package candidates.

## Why this goal exists

Goal128 parameterized the Unity projection runner with `-PackagePath`, but the normal verification still proves only one default package.

Goal129 must make that useful for the actual AI Game Builder/combine pipeline:

- create deterministic generated/candidate GamePackage JSONs without mutating `samples/minimal-map-game/**`;
- verify more than one package through the same projection runner;
- produce a compact matrix result;
- surface the matrix in Visual World Stream Preview / WinForms;
- keep manual Unity inspection optional.

This is still projection/verification/product-workflow only. It must not become Runtime/schema/provider/Lua/generator-library/release work.

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

.llmgc/procedural/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface/parameterized-gamepackage-runner-dashboard.json
.llmgc/procedural/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface/parameterized-gamepackage-runner-result.json
.llmgc/procedural/goal-127-winforms-unity-projection-verification-runner/unity-projection-verification-runner-dashboard.json

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ParameterizedGamePackageProjectionRunnerService.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ParameterizedGamePackageProjectionRunnerModels.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/**
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-129-gamepackage-candidate-matrix-projection-runner/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-unity-projection-verification.ps1
.devflow/scripts/run-gamepackage-projection-matrix.ps1
.devflow/scripts/run-gamepackage-projection-matrix.cmd

.llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/**
.llmgc/exports/goal-129-gamepackage-candidate-matrix-projection-runner/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/gamepackage-candidate-matrix-projection-runner.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal129.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/Devflow/RunUnityProjectionVerificationScriptTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGamePackageProjectionMatrixScriptTests.cs
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

No Unity source changes for this goal. Use existing Goal128 Unity parameterization. No Runtime/schema/provider/Lua/generator-library work. Do not mutate `samples/minimal-map-game/**`.

## Primary deliverable A — deterministic candidate package matrix

Create a BCL-only Application service under:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/
```

It must generate or verify deterministic candidate package files under:

```text
.llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/candidates/
```

Required candidates:

1. `minimal-map-game-baseline/package.json`
   - a byte-for-byte or semantically equivalent copy of `samples/minimal-map-game/package.json` placed under Goal129 procedural artifacts.
2. `minimal-map-game-variant/package.json`
   - a projection-compatible variant derived from the sample, without changing schema.
   - Must keep the same core compatible IDs needed by the full playthrough: `map/village`, `entity/village/sign`, `entity/village/old_guard`, `interaction/sign_inspect`, `dialogue/old_guard_intro`, `quest/help_healer`, `recipe/healing_potion`, `node/apple_tree`, `transaction/buy_healing_potion`, `encounter/goblin_duel`.
   - Must change at least package identity/title/description/version or visible labels so it proves `-PackagePath` is not only hardcoded sample.

Create a candidate index:

```text
.llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/gamepackage-candidate-index.json
```

It must include:

```text
candidateId
packagePath
packagePathRelative
packageId
title
sourceKind
expectedProjectionMode
requiredCompatibilityIds
sha256
```

All paths must be repo-relative and under Goal129 artifacts, except the read-only original sample input path.

## Primary deliverable B — projection matrix runner

Add:

```text
.devflow/scripts/run-gamepackage-projection-matrix.ps1
.devflow/scripts/run-gamepackage-projection-matrix.cmd
```

Required behavior:

- Default candidate index path: `.llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/gamepackage-candidate-index.json`.
- For every candidate, call `.devflow/scripts/run-unity-projection-verification.ps1 -Mode GenericFullPlaythrough -PackagePath <candidate package path> -ApplyCleanup`.
- Preserve per-candidate result/log under:

```text
.llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/matrix/<candidateId>/
```

- Write one compact aggregate result:

```text
.llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/gamepackage-projection-matrix-result.json
```

- Return exit 0 only when all candidates pass and cleanup succeeds.
- Support `-DryRun` that prints exact per-candidate runner commands without executing Unity.
- Reject candidate package paths outside repo root and paths under `.llmgc/manual/**`.

You may update `.devflow/scripts/run-unity-projection-verification.ps1` only if necessary to support per-run result/log output paths cleanly, for example by adding an optional result/evidence root parameter. Keep backwards compatibility with Goal128 command behavior.

## Primary deliverable C — evidence and workspace surface

Add BCL-only evidence under:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/
```

It must verify:

- candidate index exists;
- at least 2 candidates exist;
- baseline and variant package paths are distinct;
- sample package remains read-only/unmodified;
- matrix runner script exists;
- matrix result exists;
- all matrix entries passed;
- pass markers present;
- fail/material warning markers absent;
- no forbidden paths expected.

Add Visual World Stream Preview / WinForms read-only Goal129 section showing:

```text
matrixStatus
candidateCount
passedCandidateCount
failedCandidateCount
candidateIndexPath
matrixResultPath
normalCommand
exampleCommand
baselineCandidatePackagePath
variantCandidatePackagePath
manualUnityOptional
cleanupApplied
projectionOnly
```

## Required artifacts

Create deterministic artifacts under:

```text
.llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/
.llmgc/exports/goal-129-gamepackage-candidate-matrix-projection-runner/
```

Recommended files:

```text
gamepackage-candidate-index.json
gamepackage-projection-matrix-result.json
gamepackage-candidate-matrix-dashboard.json
gamepackage-candidate-matrix-script-scan.json
gamepackage-candidate-matrix-log-scan.json
gamepackage-candidate-matrix-negative-proof.json
gamepackage-candidate-matrix-report.md
gamepackage-candidate-matrix-file-index.json
candidates/minimal-map-game-baseline/package.json
candidates/minimal-map-game-variant/package.json
matrix/<candidateId>/runner-result.json
matrix/<candidateId>/unity.log or log-scan.json
```

If raw Unity `.log` files are ignored, commit compact result/log-scan artifacts and mention that raw logs are local-only or force-add only if the goal policy explicitly allows them.

## Docs/current state

Update current-state/queue docs so they clearly say:

- Goal129 adds deterministic candidate GamePackage matrix verification.
- Normal verification can now check multiple package candidates via `.devflow\scripts\run-gamepackage-projection-matrix.cmd`.
- Manual Unity remains optional.
- This still authorizes no sample mutation, `.llmgc/manual/**`, Runtime/provider/schema/Lua/generator-library/final-art/atlas/Unity scene/prefab/project-settings/StreamingAssets/release-packaging work.

## Artifact-scope policy

Add scenario:

```text
goal-129-gamepackage-candidate-matrix-projection-runner
```

It must allow only Goal129 expected files and exclude `.llmgc/manual/**`, samples/minimal-map-game, Runtime/schema/provider/Generation/AssetPipeline/Scripting/generator-library, Unity Assets/ProjectSettings/Packages.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~Goal129|FullyQualifiedName~VisualWorldStreamPreviewWorkspace|FullyQualifiedName~RunUnityProjectionVerificationScript|FullyQualifiedName~RunGamePackageProjectionMatrixScript"
.\.devflow\scripts\run-gamepackage-projection-matrix.ps1 -DryRun
.\.devflow\scripts\run-gamepackage-projection-matrix.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-129-gamepackage-candidate-matrix-projection-runner
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Also verify forbidden paths explicitly before staging:

```powershell
git diff --name-only -- samples/minimal-map-game src/LLMGameCreator.Runtime src/LLMGameCreator.Runtime.Abstractions src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha/Assets unity/LLMGameCreatorAlpha/ProjectSettings unity/LLMGameCreatorAlpha/Packages
```

That command must produce no forbidden changes.

## Quality gate

GREEN requires:

- at least 2 candidate packages under Goal129 artifacts;
- matrix dry-run prints exact commands;
- matrix apply runs Unity through the parameterized runner for each candidate;
- aggregate matrix result says all candidates passed;
- pass markers present and fail/material warning markers absent;
- cleanup succeeds;
- sample package is not modified;
- no forbidden path changes;
- no `.llmgc/manual/**` staged/tracked;
- tests/checks pass;
- final git status clean.

BLOCKED if Unity cannot run or parameterized matrix verification cannot be made honest.

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
GREEN Goal 129 gamepackage candidate matrix projection runner
BLOCKED Goal 129 gamepackage candidate matrix projection runner
FAILED Goal 129 gamepackage candidate matrix projection runner
```

Final report must include commit SHA, candidate count, passed/failed count, matrix command, result path, final git status, and remaining debt.
