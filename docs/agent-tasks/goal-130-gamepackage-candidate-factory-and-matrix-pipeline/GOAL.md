# Goal 130 — GamePackage Candidate Factory + Matrix Pipeline

## Task ID

`goal-130-gamepackage-candidate-factory-and-matrix-pipeline`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product goal.

This is not a docs-only/review goal. The primary deliverable is a deterministic, repo-local GamePackage candidate factory that materializes multiple projection-compatible candidate packages and then runs the existing Goal129 candidate matrix over them with one command.

## Why this exists

Goal128 made the Unity projection runner parameterized by `-PackagePath`.

Goal129 added a matrix runner over an explicit candidate index.

Goal130 must connect the next product seam:

```text
candidate factory -> candidate index -> matrix runner -> aggregate result -> WinForms/VisualWorld status
```

This moves the project from "we can verify a hand-authored sample/candidate list" to "the tool can materialize candidate GamePackage variants and verify them as a batch."

No LLM/provider/network work is allowed in this goal. The factory is deterministic and BCL-only.

## Required hands-on result

A normal repo-local verification command:

```bat
.devflow\scripts\run-gamepackage-candidate-factory.cmd
```

It must:

1. Read `samples/minimal-map-game/package.json` as a read-only template.
2. Materialize at least 3 deterministic candidate packages under Goal130 `.llmgc/procedural/**`.
3. Write a candidate index for the Goal129 matrix runner.
4. Invoke the Goal129 matrix runner against the generated Goal130 candidate index.
5. Write compact factory and matrix result artifacts.
6. Leave the worktree clean after allowed commit.

Manual Unity inspection remains optional.

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

.devflow/scripts/run-unity-projection-verification.ps1
.devflow/scripts/run-unity-projection-verification.cmd
.devflow/scripts/run-gamepackage-projection-matrix.ps1
.devflow/scripts/run-gamepackage-projection-matrix.cmd

.llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/gamepackage-projection-matrix-result.json
.llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/gamepackage-candidate-index.json

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/GamePackageCandidateMatrixProjectionService.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ParameterizedGamePackageProjectionRunnerService.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewGamePackageCandidateMatrixInspector.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/**
.devflow/artifact-scope/artifact-scope-policy.json

.devflow/scripts/run-gamepackage-candidate-factory.ps1
.devflow/scripts/run-gamepackage-candidate-factory.cmd
.devflow/scripts/run-gamepackage-projection-matrix.ps1
.devflow/scripts/run-unity-projection-verification.ps1

.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/**
.llmgc/exports/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/gamepackage-candidate-factory-and-matrix-pipeline.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal130.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/DevFlow/RunGamePackageCandidateFactoryScriptTests.cs
tests/LLMGameCreator.Tests/DevFlow/RunGamePackageProjectionMatrixScriptTests.cs
tests/LLMGameCreator.Tests/DevFlow/RunUnityProjectionVerificationScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
samples/minimal-map-game/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.AssetPipeline/**
src/LLMGameCreator.Scripting/**
provider / LLM / RAG / media provider code
public GamePackage schema files
Lua / Scripting code
generator-library/**
unity/LLMGameCreatorAlpha/Assets/**
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**
*.sln
*.csproj
Directory.Build.*
dependency/package files
```

No Runtime/schema/provider/Lua/generator-library work. No Unity Assets changes in this goal. Do not mutate the sample package.

## Primary deliverable A — Candidate factory script

Add:

```text
.devflow/scripts/run-gamepackage-candidate-factory.ps1
.devflow/scripts/run-gamepackage-candidate-factory.cmd
```

The `.cmd` should be the default simple user entrypoint.

Script requirements:

- Supports:
  - `-TemplatePackagePath`, default `samples/minimal-map-game/package.json`
  - `-OutputRoot`, default `.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline`
  - `-UnityPath`
  - `-DryRun`
  - `-ApplyCleanup`
- Verifies `TemplatePackagePath` stays under repo root.
- Refuses `.llmgc/manual/**`.
- Refuses to write outside the Goal130 output root.
- Does not use LLM/provider/network.
- Does not mutate `samples/minimal-map-game/package.json`.
- Materializes candidates before running the matrix unless `-DryRun`.
- Calls `run-gamepackage-projection-matrix.ps1` with `-CandidateIndexPath` pointing at the generated Goal130 candidate index.
- Writes a compact factory result JSON.

## Primary deliverable B — Candidate materialization

Materialize at least 3 candidate packages under:

```text
.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/candidates/<candidate-id>/package.json
```

Minimum candidates:

```text
minimal-map-game-baseline
minimal-map-game-alchemy-route
minimal-map-game-combat-route
```

Each candidate must be projection-compatible with the Goal128/129 runner.

Candidate requirements:

- Valid JSON.
- Derived from the read-only sample.
- Preserve required full-playthrough anchors:
  - `entity/village/sign`
  - `interaction/sign_inspect`
  - `entity/village/old_guard`
  - `dialogue/old_guard_intro`
  - `quest/help_healer`
  - `inventory/player_start`
  - `recipe/healing_potion`
  - `node/apple_tree`
  - `transaction/buy_healing_potion`
  - `encounter/goblin_duel`
- Differ from one another in deterministic, explicit ways:
  - package title/description/version metadata;
  - candidate metadata;
  - optional inventory/resource/item tuning that does not break the full-playthrough projection.
- Do not require public schema changes.

Candidate index:

```text
.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/gamepackage-candidate-index.json
```

must include candidate id, package path relative to repo root, title, source template, variant kind, sha256.

Also mirror compact export artifacts under `.llmgc/exports/goal-130.../`.

## Primary deliverable C — Matrix pipeline result

Run the matrix over the generated Goal130 candidate index and produce:

```text
.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/gamepackage-candidate-factory-result.json
.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/gamepackage-candidate-factory-dashboard.json
.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/gamepackage-projection-matrix-result.json
.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/matrix/<candidate-id>/runner-result.json
.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/matrix/<candidate-id>/log-scan.json
```

Required pass condition:

```text
candidateCount >= 3
matrixPassed = true
passedCandidates = candidateCount
failedCandidates = 0
allCandidatePackagesExist = true
allCandidatePackagesDiffer = true
samplePackageUnmodified = true
manualUnityOptional = true
```

## Application evidence seam

Add/update BCL-only evidence under:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/
```

Recommended new files:

```text
GamePackageCandidateFactoryProjectionModels.cs
GamePackageCandidateFactoryProjectionService.cs
```

It must verify:

- Goal129 matrix runner remains available.
- Candidate factory script exists.
- Candidate factory result exists and is GREEN.
- Candidate count >= 3.
- Matrix result is GREEN.
- All candidates passed.
- Candidate packages are under Goal130 output roots.
- Candidate package hashes differ.
- Sample package hash matches unchanged template hash from prior evidence.
- No forbidden paths expected.
- Manual Unity inspection remains optional.

Keep new source files reasonably sized. Prefer partial/split source rather than files >700 physical lines.

## Visual World Stream Preview Workspace

Add a read-only Goal130 section showing:

```text
candidateFactoryStatus
candidateCount
passedCandidates
failedCandidates
matrixPassed
candidateIndexPath
normalCommand
factoryResultPath
matrixResultPath
manualUnityOptional
samplePackageUnmodified
projectionOnly
evidencePath
exportPath
```

## Required artifacts

Create deterministic artifacts under:

```text
.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/
.llmgc/exports/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/
```

Recommended files:

```text
gamepackage-candidate-factory-dashboard.json
gamepackage-candidate-factory-result.json
gamepackage-candidate-factory-script-scan.json
gamepackage-candidate-factory-log-scan.json
gamepackage-candidate-factory-negative-proof.json
gamepackage-candidate-factory-report.md
gamepackage-candidate-factory-file-index.json
gamepackage-candidate-index.json
```

Raw Unity `.log` files may remain local/ignored if compact runner-result/log-scan artifacts prove the matrix.

## Docs/current state

Update current-state/queue docs so they clearly say:

- Goal130 adds a deterministic GamePackage candidate factory and matrix pipeline.
- Normal verification command is `.devflow\scripts\run-gamepackage-candidate-factory.cmd`.
- It materializes candidates and runs the matrix automatically.
- Manual Unity inspection remains optional.
- No sample mutation, Runtime/schema/provider/Lua/generator-library/final-art/Unity Assets/StreamingAssets/release work is authorized.

## Artifact-scope policy

Add scenario:

```text
goal-130-gamepackage-candidate-factory-and-matrix-pipeline
```

It must allow only Goal130 expected files and exclude `.llmgc/manual/**`, samples/minimal-map-game, Runtime/schema/provider/Lua/generator-library, Unity Assets/ProjectSettings/Packages, solution/project/dependency files.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~Goal130|FullyQualifiedName~GamePackageCandidateFactory|FullyQualifiedName~RunGamePackageCandidateFactory|FullyQualifiedName~GamePackageCandidateMatrix|FullyQualifiedName~VisualWorldStreamPreviewWorkspace|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection"
.\.devflow\scripts\run-gamepackage-candidate-factory.ps1 -DryRun
.\.devflow\scripts\run-gamepackage-candidate-factory.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-130-gamepackage-candidate-factory-and-matrix-pipeline
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Also verify forbidden path diffs are empty:

```powershell
git diff --name-only -- samples/minimal-map-game src/LLMGameCreator.Runtime src/LLMGameCreator.Runtime.Abstractions src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha/Assets unity/LLMGameCreatorAlpha/ProjectSettings unity/LLMGameCreatorAlpha/Packages
```

## Quality gate

GREEN requires:

- candidate factory command exists and passes;
- at least 3 candidate packages materialized;
- matrix passes all candidates;
- candidate package hashes differ;
- sample package remains unmodified;
- manual Unity inspection optional;
- tests/checks pass;
- artifact scope passes;
- no forbidden path changes;
- no `.llmgc/manual/**` tracked/staged;
- final git status clean.

BLOCKED if the matrix cannot be run honestly.

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
GREEN Goal 130 gamepackage candidate factory and matrix pipeline
BLOCKED Goal 130 gamepackage candidate factory and matrix pipeline
FAILED Goal 130 gamepackage candidate factory and matrix pipeline
```

Final report must include commit SHA, candidate count, matrix pass count, factory command, result path, final git status, and remaining debt.
