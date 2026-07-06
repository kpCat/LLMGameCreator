# Goal 132 — WinForms Candidate Pipeline Operator Panel

## Task ID

`goal-132-winforms-candidate-pipeline-operator-panel`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product goal.

This is not a docs-only/review goal. The primary deliverable is a real WinForms operator surface for the current GamePackage candidate recipe pipeline, so the normal user path is no longer “open Unity manually” or “remember several CLI commands”.

## Why this exists

Goal131 created the deterministic candidate recipe pipeline:

```text
recipe catalog -> candidates -> matrix -> scoring -> selected candidate handoff
```

The normal command is currently:

```bat
.devflow\scripts\run-gamepackage-candidate-recipe-pipeline.cmd
```

Goal132 must surface this in the existing Visual World Stream Preview workspace as an operator panel with run/refresh/copy actions and captured results.

No Runtime/schema/provider/Lua/generator-library/Unity Assets work is allowed.

## Required hands-on result

In the WinForms Visual World Stream Preview workspace, the user can:

1. See the current candidate recipe pipeline status.
2. See the normal command and result path.
3. Click a button to run a dry-run.
4. Click a button to run the full recipe pipeline with cleanup.
5. Refresh current result from disk.
6. Copy the normal command.
7. See exit code, duration, selected candidate id, score, candidate count, matrix pass/fail count, and output tail.

The UI must not freeze. Use async process execution and marshal UI updates back to the UI thread.

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

.devflow/scripts/run-gamepackage-candidate-recipe-pipeline.ps1
.devflow/scripts/run-gamepackage-candidate-recipe-pipeline.cmd
.devflow/scripts/run-gamepackage-candidate-factory.ps1
.devflow/scripts/run-gamepackage-projection-matrix.ps1
.devflow/scripts/run-unity-projection-verification.ps1

.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/gamepackage-recipe-pipeline-result.json
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/candidate-scoring-result.json
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/selected-candidate-handoff.json

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/GamePackageCandidateRecipePipelineModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/GamePackageCandidateRecipePipelineService.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewGamePackageCandidateRecipePipelineInspector.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal131.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-132-winforms-candidate-pipeline-operator-panel/**
.devflow/artifact-scope/artifact-scope-policy.json

.llmgc/procedural/goal-132-winforms-candidate-pipeline-operator-panel/**
.llmgc/exports/goal-132-winforms-candidate-pipeline-operator-panel/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/winforms-candidate-pipeline-operator-panel.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal132.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
samples/minimal-map-game/**
.devflow/scripts/run-gamepackage-candidate-recipe-pipeline.ps1
.devflow/scripts/run-gamepackage-candidate-recipe-pipeline.cmd
.devflow/scripts/run-gamepackage-candidate-factory.ps1
.devflow/scripts/run-gamepackage-projection-matrix.ps1
.devflow/scripts/run-unity-projection-verification.ps1
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

No Designer files unless the existing page already uses code-only partials for this workspace. Prefer code-only partial controls. Do not use the Visual Studio designer.

## Primary deliverable A — Application operator model/service

Add BCL-only application-layer models/service under:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/
```

Recommended files:

```text
GamePackageCandidatePipelineOperatorModels.cs
GamePackageCandidatePipelineOperatorService.cs
```

The service must provide:

- normal command string;
- dry-run command string;
- result paths;
- method to parse Goal131 pipeline result;
- method to parse selected-candidate handoff;
- method to build a compact operator status;
- method to write an operator-result JSON under Goal132 evidence root after a run.

The service must not call Unity, Runtime, providers, LLM, network, Lua or generator-library.

Keep new source files reasonably sized. If a new file approaches 700 physical lines, split it.

## Primary deliverable B — WinForms operator panel

Add a Goal132 partial for the existing Visual World Stream Preview workspace:

```text
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal132.cs
```

Required UI behavior:

- Adds a clearly labeled “Goal132 Candidate Pipeline Operator” section.
- Shows normal command:
  `.devflow\scripts\run-gamepackage-candidate-recipe-pipeline.cmd`
- Shows current result path:
  `.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/gamepackage-recipe-pipeline-result.json`
- Shows selected candidate id and score when available.
- Shows candidate count, passed count, failed count, matrix status.
- Has buttons:
  - `Refresh Candidate Pipeline Status`
  - `Copy Candidate Pipeline Command`
  - `Dry Run Candidate Recipe Pipeline`
  - `Run Candidate Recipe Pipeline`
- Dry-run executes the existing `.cmd` or `.ps1` with `-DryRun`.
- Full run executes the existing `.cmd` default or `.ps1 -ApplyCleanup`.
- Runs must be async and non-blocking.
- UI must show running/completed/failed status and process exit code.
- Capture stdout/stderr tail into the Goal132 operator result artifact.

Do not require manual Unity inspection after a successful run.

## Primary deliverable C — Operator result artifact

After validation, produce deterministic Goal132 artifacts under:

```text
.llmgc/procedural/goal-132-winforms-candidate-pipeline-operator-panel/
.llmgc/exports/goal-132-winforms-candidate-pipeline-operator-panel/
```

Recommended files:

```text
candidate-pipeline-operator-dashboard.json
candidate-pipeline-operator-result.json
candidate-pipeline-operator-script-scan.json
candidate-pipeline-operator-winforms-scan.json
candidate-pipeline-operator-negative-proof.json
candidate-pipeline-operator-report.md
candidate-pipeline-operator-file-index.json
```

Required status fields:

```text
operatorStatus
normalCommand
winFormsPanelPresent
refreshButtonPresent
copyCommandButtonPresent
dryRunButtonPresent
runButtonPresent
asyncRunPresent
resultPath
selectedCandidateId
selectedCandidateScore
candidateCount
passedCandidates
failedCandidates
matrixPassed
manualUnityOptional
projectionOnly
samplePackageReadOnly
```

## Primary deliverable D — Visual World Stream Preview workspace proof

Add/update read-only proof models/reporting so Goal132 appears in the Visual World Stream Preview report with:

```text
operatorStatus
normalCommand
dryRunCommand
resultPath
selectedCandidateId
selectedCandidateScore
candidateCount
passedCandidates
failedCandidates
matrixPassed
lastOperatorExitCode
manualUnityOptional
```

## Docs/current state

Update current-state/queue docs so they clearly say:

- Goal132 adds a WinForms candidate pipeline operator panel.
- Normal command remains `.devflow\scripts\run-gamepackage-candidate-recipe-pipeline.cmd`.
- Manual Unity inspection is optional.
- No sample mutation, Runtime/schema/provider/Lua/generator-library/final-art/Unity Assets/StreamingAssets/release work is authorized.

## Artifact-scope policy

Add scenario:

```text
goal-132-winforms-candidate-pipeline-operator-panel
```

It must allow only Goal132 expected files and exclude `.llmgc/manual/**`, samples/minimal-map-game, Runtime/schema/provider/Lua/generator-library, Unity Assets/ProjectSettings/Packages, solution/project/dependency files, and existing devflow runner scripts.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~Goal132|FullyQualifiedName~CandidatePipelineOperator|FullyQualifiedName~VisualWorldStreamPreviewWorkspace|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection"
.\.devflow\scripts\run-gamepackage-candidate-recipe-pipeline.ps1 -DryRun
.\.devflow\scripts\run-gamepackage-candidate-recipe-pipeline.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-132-winforms-candidate-pipeline-operator-panel
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Also verify forbidden path diffs are empty:

```powershell
git diff --name-only -- samples/minimal-map-game .devflow/scripts/run-gamepackage-candidate-recipe-pipeline.ps1 .devflow/scripts/run-gamepackage-candidate-recipe-pipeline.cmd .devflow/scripts/run-gamepackage-candidate-factory.ps1 .devflow/scripts/run-gamepackage-projection-matrix.ps1 .devflow/scripts/run-unity-projection-verification.ps1 src/LLMGameCreator.Runtime src/LLMGameCreator.Runtime.Abstractions src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha/Assets unity/LLMGameCreatorAlpha/ProjectSettings unity/LLMGameCreatorAlpha/Packages
```

If validation regenerates historical artifacts outside Goal132, restore those exact paths before staging and report them.

## Quality gate

GREEN requires:

- WinForms operator panel present;
- async run path present;
- refresh/copy/dry-run/full-run controls present;
- operator result artifact exists;
- Goal131 pipeline command still passes;
- selected candidate id/score parsed and displayed in proof;
- no forbidden path changes;
- no `.llmgc/manual/**` tracked/staged;
- final git status clean.

BLOCKED if the existing WinForms workspace structure cannot safely host an operator panel without broad designer/global UI changes.

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
GREEN Goal 132 winforms candidate pipeline operator panel
BLOCKED Goal 132 winforms candidate pipeline operator panel
FAILED Goal 132 winforms candidate pipeline operator panel
```

Final report must include commit SHA, whether the operator panel was implemented, command/result paths, selected candidate, tests/checks, final git status, and remaining debt.
