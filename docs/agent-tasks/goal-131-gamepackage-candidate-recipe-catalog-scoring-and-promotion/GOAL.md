# Goal 131 — GamePackage Candidate Recipe Catalog + Scoring + Promotion Queue

## Task ID

`goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product goal.

This is not a docs-only/review goal. The primary deliverable is a reusable, deterministic GamePackage candidate recipe catalog and scoring/promotion pipeline on top of the Goal130 candidate factory and Goal129 matrix runner.

## Why this exists

Goal130 can materialize a hardcoded set of projection-compatible candidate packages and run the matrix.

Goal131 must make this closer to a real generator pipeline:

```text
recipe catalog -> candidate factory -> candidate index -> matrix runner -> scoring -> promoted candidate -> compact handoff
```

No LLM/provider/network work is allowed. This is deterministic and repo-local.

## Required hands-on result

A normal repo-local command:

```bat
.devflow\scripts\run-gamepackage-candidate-recipe-pipeline.cmd
```

It must:

1. Read a deterministic candidate recipe catalog.
2. Materialize at least 4 candidate packages from recipes.
3. Run the existing matrix pipeline over those candidates.
4. Score candidates using deterministic rules.
5. Promote one selected candidate to a `selected-candidate` handoff under Goal131 artifacts.
6. Write compact result/scoring/promotion JSON.
7. Leave manual Unity inspection optional.

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
.devflow/scripts/run-gamepackage-candidate-factory.ps1
.devflow/scripts/run-gamepackage-candidate-factory.cmd

.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/gamepackage-candidate-factory-result.json
.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/gamepackage-projection-matrix-result.json
.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/gamepackage-candidate-index.json
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/**
.devflow/artifact-scope/artifact-scope-policy.json

.devflow/scripts/run-gamepackage-candidate-recipe-pipeline.ps1
.devflow/scripts/run-gamepackage-candidate-recipe-pipeline.cmd
.devflow/scripts/run-gamepackage-candidate-factory.ps1
.devflow/scripts/run-gamepackage-projection-matrix.ps1
.devflow/scripts/run-unity-projection-verification.ps1

.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/**
.llmgc/exports/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/gamepackage-candidate-recipe-catalog-scoring-and-promotion.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal131.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/DevFlow/RunGamePackageCandidateRecipePipelineScriptTests.cs
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

## Primary deliverable A — recipe catalog

Create a deterministic recipe catalog under:

```text
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/candidate-recipe-catalog.json
```

It must contain at least 4 recipes. Minimum recipe ids:

```text
balanced_baseline
alchemy_focus
combat_focus
exploration_focus
```

Each recipe must specify deterministic mutations/intent such as:

```text
candidateId
titleSuffix
variantKind
description
inventoryAdjustments
resourceAdjustments
questTuning
encounterTuning
expectedFullPlaythroughAnchors
```

The recipes must preserve the full-playthrough anchors required by Goals 123-130.

## Primary deliverable B — recipe pipeline command

Add:

```text
.devflow/scripts/run-gamepackage-candidate-recipe-pipeline.ps1
.devflow/scripts/run-gamepackage-candidate-recipe-pipeline.cmd
```

Script behavior:

- Supports:
  - `-TemplatePackagePath`, default `samples/minimal-map-game/package.json`
  - `-RecipeCatalogPath`, default Goal131 recipe catalog path
  - `-OutputRoot`, default `.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion`
  - `-UnityPath`
  - `-DryRun`
  - `-ApplyCleanup`
- Verifies all input paths stay under repo root.
- Refuses `.llmgc/manual/**`.
- Refuses writes outside the Goal131 output root.
- Does not use LLM/provider/network.
- Does not mutate sample package.
- Materializes candidates from recipe catalog.
- Invokes matrix runner over generated index.
- Scores candidates.
- Promotes selected candidate.
- Writes result JSON.

## Primary deliverable C — candidate scoring and promotion

Scoring must be deterministic and transparent.

Score components should include at least:

```text
matrixPassed
fullPlaythroughPassed
anchorCoverage
candidateDistinctness
questSystemsCoverage
noForbiddenMarkers
```

Produce:

```text
candidate-scoring-result.json
selected-candidate/package.json
selected-candidate/selected-candidate-handoff.json
```

Selection rules:

- Only matrix-passed candidates are eligible.
- Highest score wins.
- Tie-breaker must be deterministic by recipe order and candidate id.
- Selected package is copied under Goal131 `selected-candidate/` only.
- No sample/package source outside Goal131 may be modified.

Required pass condition:

```text
recipeCount >= 4
candidateCount >= 4
matrixPassed = true
passedCandidates = candidateCount
failedCandidates = 0
selectedCandidateId not empty
selectedCandidatePackageExists = true
selectedCandidateScore > 0
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
GamePackageCandidateRecipePipelineModels.cs
GamePackageCandidateRecipePipelineService.cs
```

It must verify:

- recipe pipeline command exists;
- recipe catalog exists and has at least 4 recipes;
- candidates exist and are projection-compatible;
- matrix result is GREEN;
- scoring result exists;
- selected candidate handoff exists;
- sample package hash remains unchanged;
- no forbidden paths expected;
- manual Unity inspection remains optional.

Keep source files reasonably sized. Prefer partial/split source rather than files >700 physical lines.

## Visual World Stream Preview Workspace

Add a read-only Goal131 section showing:

```text
recipePipelineStatus
recipeCount
candidateCount
passedCandidates
failedCandidates
matrixPassed
selectedCandidateId
selectedCandidateScore
selectedCandidatePackagePath
normalCommand
recipeCatalogPath
pipelineResultPath
scoringResultPath
manualUnityOptional
samplePackageUnmodified
projectionOnly
evidencePath
exportPath
```

## Required artifacts

Create deterministic artifacts under:

```text
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/
.llmgc/exports/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/
```

Recommended files:

```text
candidate-recipe-catalog.json
gamepackage-candidate-index.json
gamepackage-recipe-pipeline-result.json
candidate-scoring-result.json
selected-candidate/selected-candidate-handoff.json
selected-candidate/package.json
gamepackage-candidate-recipe-pipeline-dashboard.json
gamepackage-candidate-recipe-pipeline-script-scan.json
gamepackage-candidate-recipe-pipeline-log-scan.json
gamepackage-candidate-recipe-pipeline-negative-proof.json
gamepackage-candidate-recipe-pipeline-report.md
gamepackage-candidate-recipe-pipeline-file-index.json
gamepackage-projection-matrix-result.json
```

Raw Unity `.log` files may remain local/ignored if compact runner-result/log-scan artifacts prove the matrix.

## Docs/current state

Update current-state/queue docs so they clearly say:

- Goal131 adds deterministic candidate recipe catalog, scoring and selected-candidate promotion.
- Normal verification command is `.devflow\scripts\run-gamepackage-candidate-recipe-pipeline.cmd`.
- It materializes candidates from recipes, runs the matrix, scores candidates and promotes one candidate.
- Manual Unity inspection remains optional.
- No sample mutation, Runtime/schema/provider/Lua/generator-library/final-art/Unity Assets/StreamingAssets/release work is authorized.

## Artifact-scope policy

Add scenario:

```text
goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion
```

It must allow only Goal131 expected files and exclude `.llmgc/manual/**`, samples/minimal-map-game, Runtime/schema/provider/Lua/generator-library, Unity Assets/ProjectSettings/Packages, solution/project/dependency files.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~Goal131|FullyQualifiedName~GamePackageCandidateRecipe|FullyQualifiedName~RunGamePackageCandidateRecipe|FullyQualifiedName~GamePackageCandidateFactory|FullyQualifiedName~GamePackageCandidateMatrix|FullyQualifiedName~VisualWorldStreamPreviewWorkspace|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection"
.\.devflow\scripts\run-gamepackage-candidate-recipe-pipeline.ps1 -DryRun
.\.devflow\scripts\run-gamepackage-candidate-recipe-pipeline.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion
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

- recipe pipeline command exists and passes;
- at least 4 recipes and 4 candidate packages;
- matrix passes all candidates;
- scoring result exists and is deterministic;
- one selected candidate is promoted under Goal131 artifacts;
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
GREEN Goal 131 gamepackage candidate recipe catalog scoring and promotion
BLOCKED Goal 131 gamepackage candidate recipe catalog scoring and promotion
FAILED Goal 131 gamepackage candidate recipe catalog scoring and promotion
```

Final report must include commit SHA, recipe count, candidate count, matrix pass count, selected candidate id, selected score, pipeline command, result path, final git status, and remaining debt.
