# Goal 115 — Offline Geoworld Alpha Human Result Revalidation + Decision Snapshot

## Task ID

`goal-115-offline-geoworld-alpha-human-result-revalidation`

## Status token

Use exactly one of these final commit statuses:

- `GREEN` only when the real local human manual result validates as `GREEN_ACCEPTABLE_CANDIDATE`.
- `BLOCKED` when the local manual result is missing, incomplete, pending, accepted=false, has failed/skipped/pending/missing/duplicate/invalid steps, or is otherwise not acceptable for a human gate decision.
- `FAILED` only for unexpected implementation/build/test/tooling failure, forbidden-zone risk, or inability to produce deterministic evidence.

## Repository

Repo: https://github.com/kpCat/LLMGameCreator
Local working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`

## Human context

The user manually opened the Unity Alpha project after Goal114, confirmed the Goal110 acceptance runner opens, and created a local human-edited result JSON at:

```text
.llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json
```

This file is intentionally **local human input**. It is untracked. Do not commit it.

The purpose of this goal is to read that local human input, validate it through the existing Goal111/112/113 acceptance chain, and commit deterministic repository evidence saying whether it is a valid candidate for the user’s explicit manual gate decision.

## Human-readable outcome

After this goal:

1. WinForms Visual World Stream Preview Workspace should show a Goal115/human-result revalidation section.
2. The repo should contain deterministic Goal115 evidence/export artifacts:
   - whether the human result exists;
   - whether it is syntactically valid JSON;
   - whether it matches the Goal110 manual gate/checklist hash/schema;
   - whether all 12 required steps are present exactly once;
   - whether all required steps are `passed`;
   - whether `accepted=true`;
   - whether the result is a `GREEN_ACCEPTABLE_CANDIDATE` or still blocked.
3. The manual gate must still remain a human/user decision. Codex must not flip historical accepted gates or declare final Alpha acceptance.
4. The local `.llmgc/manual/**` input must remain untracked/uncommitted.

## Why now

Goals 110–113 built the manual acceptance runner, result intake, operator pack, and workbench. Goal114 fixed the Unity Safe Mode compile blocker. The next necessary step is not another feature: it is deterministic revalidation of the real local human result JSON and a decision snapshot for the user.

## Read-first files

Read these before editing:

```text
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
.devflow/artifact-scope/artifact-scope-policy.json

docs/manual-acceptance/offline-geoworld-alpha-manual-result-workbench.md

src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultIntake/OfflineGeoworldAlphaManualResultIntakeModels.cs
src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultIntake/OfflineGeoworldAlphaManualResultIntakeService.cs
src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultIntake/OfflineGeoworldAlphaManualResultIntakeService.Validation.cs

src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaAcceptanceOperatorPack/
src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultWorkbench/

src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/

tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaManualResultIntake/
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/
tests/LLMGameCreator.Tests/ProductSmoke/
```

Also read, but never stage or commit:

```text
.llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json
```

## Allowed files

You may add/modify only these areas:

```text
docs/agent-tasks/goal-115-offline-geoworld-alpha-human-result-revalidation/**
docs/manual-acceptance/offline-geoworld-alpha-human-result-revalidation.md

.llmgc/procedural/goal-115-offline-geoworld-alpha-human-result-revalidation/**
.llmgc/exports/goal-115-offline-geoworld-alpha-human-result-revalidation/**

src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaHumanResultRevalidation/**

src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldPreviewGoal115Quality.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewOfflineGeoworldAlphaHumanResultRevalidationInspector.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewProofStatusLoader.Goal115.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewProofStatusLoader.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewReportBuilder.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewReportRenderer.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewRequiredArtifactGroups.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWinFormsBindingScanner.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceModels.Goal115.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceService.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewExpectedChangedPaths.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewEvidenceWriter.cs

src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal115.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Status.cs

tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaHumanResultRevalidation/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal115Tests.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceServiceTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaHumanResultRevalidationProductSmokeTests.cs

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
.devflow/artifact-scope/artifact-scope-policy.json
```

## Forbidden files and actions

Do not add, edit, delete, stage, commit, or push:

```text
.llmgc/manual/**
unity/LLMGameCreatorAlpha/Assets/Scenes/**
unity/LLMGameCreatorAlpha/Assets/**/*.unity
unity/LLMGameCreatorAlpha/Assets/**/*.prefab
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/**
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs

src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
public GamePackage schema files
providers / LLM / RAG / media provider code
Lua / Scripting code
generator-library/**
*.sln
*.csproj
Directory.Build.*
NuGet/package/dependency files
LFZ/Infection Free Zone archive or source
```

Do not create a fake manual result.
Do not edit the human manual result.
Do not copy `.llmgc/manual/**` into committed artifacts verbatim.
Do not mark Alpha accepted.
Do not start live geodata/provider/network/runtime/schema/Lua/final art/release packaging work.

## Exact behavior

### 1. Preflight

Run:

```powershell
git status --short --untracked-files=all
```

Expected before implementation:

- the untracked task pack files from this Goal;
- the untracked local manual result under `.llmgc/manual/...`;
- no Unity `.meta`, ProjectSettings, Packages, StreamingAssets, scene, prefab, or project-file noise.

If unexpected Unity-generated noise exists, do not stage it. Either restore/clean only obvious generated Unity noise if you can do so safely without touching `.llmgc/manual/**`, or commit `BLOCKED` with a clear report if the state is unsafe.

Explicitly check that the local manual result file exists:

```text
.llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json
```

### 2. Build Goal115 Application seam

Create a bounded BCL-only service under:

```text
src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaHumanResultRevalidation/
```

The service should:

- read the existing local manual result file;
- compute its SHA-256;
- call/reuse the existing Goal111 manual result intake validation logic if practical;
- never write to Goal111/Goal112/Goal113 historical artifact folders;
- never write to `.llmgc/manual/**`;
- produce a compact Goal115 snapshot model.

Required decision fields in Goal115 evidence:

```text
goalId = goal_115_offline_geoworld_alpha_human_result_revalidation
sourceGoalIds includes Goal110, Goal111, Goal112, Goal113, Goal114
manualGate = offline_geoworld_alpha_manual_acceptance_verification
manualResultRelativePath
manualResultSha256
manualResultPresent
goal111DecisionStatus
acceptableCandidate
acceptedByCodex = false
humanAcceptanceStillRequired = true
manualGateRemainsHumanDecision = true
recommendedHumanDecision
checklistHashExpected
checklistHashActual or Result checklistHash
stepSummary
errors
warnings
notFinalReleaseOrRuntimeBuild = true
noRuntimeProviderOrNetworkChanges = true
noUnityFileChangesRequired = true
manualInputNotCommitted = true
```

Decision mapping:

- `GREEN_ACCEPTABLE_CANDIDATE` only if the underlying Goal111-style decision is green.
- `BLOCKED_PENDING_MANUAL_RESULT` if missing.
- `BLOCKED_INCOMPLETE_RESULT` if any required step is missing/pending/failed/skipped/duplicated/invalid or accepted=false.
- `FAILED_INVALID_RESULT` if malformed JSON or identity/hash/schema mismatch.
- `recommendedHumanDecision = READY_FOR_EXPLICIT_HUMAN_ACCEPTANCE_DECISION` only for a green candidate.
- `recommendedHumanDecision = DO_NOT_ACCEPT_YET` otherwise.

### 3. Evidence artifacts

Write deterministic artifacts only under:

```text
.llmgc/procedural/goal-115-offline-geoworld-alpha-human-result-revalidation/
.llmgc/exports/goal-115-offline-geoworld-alpha-human-result-revalidation/
```

Required procedural artifacts:

```text
offline-geoworld-alpha-human-result-revalidation-dashboard.json
offline-geoworld-alpha-human-result-revalidation-decision-snapshot.json
offline-geoworld-alpha-human-result-revalidation-report.md
offline-geoworld-alpha-human-result-revalidation-file-index.json
offline-geoworld-alpha-human-result-revalidation-quality-gate-scan.json
offline-geoworld-alpha-human-result-revalidation-negative-proof.json
```

Required export artifacts:

```text
offline-geoworld-alpha-human-result-revalidation-dashboard.json
offline-geoworld-alpha-human-result-revalidation-readme.md
offline-geoworld-alpha-human-result-revalidation-file-index.json
```

Do not include full raw manual result JSON in committed artifacts. A hash, summary and status matrix are enough.

### 4. WinForms / Visual World Stream Preview Workspace

Add a read-only Goal115 group to the existing Visual World Stream Preview Workspace:

```text
offline_geoworld_alpha_human_result_revalidation
```

It should show at least:

- human result presence;
- manual result hash;
- validation/decision status;
- acceptable candidate yes/no;
- recommended human decision;
- acceptedByCodex false;
- humanAcceptanceStillRequired true;
- step summary;
- errors/warnings;
- evidence/export paths.

Do not create new tabs/pages. Integrate into the existing workspace pattern.

### 5. Documentation/state updates

Update current-state/queue/context/risk/debt docs.

If `GREEN_ACCEPTABLE_CANDIDATE`:

- current state must say the manual result is validated as a candidate for explicit human decision;
- active manual gate still remains required and `accepted=false`;
- user action should be to explicitly decide the gate, not to start future feature work automatically.

If blocked/failed:

- current state must clearly tell the user what is wrong with the result and not to accept the gate.

In all cases:

- do not mark Goal097–115 accepted;
- do not mark final release;
- do not start live geodata/provider/runtime/schema/Lua/generator-library/final art/atlas/release packaging.

### 6. Artifact scope policy

Add a scenario:

```text
goal-115-offline-geoworld-alpha-human-result-revalidation
```

It must include the allowed paths above and must explicitly exclude `.llmgc/manual/**`.

### 7. Tests

Add focused tests for:

- missing manual result => blocked;
- malformed manual result => invalid;
- draft-template-like result => blocked/not green;
- valid local human result => `GREEN_ACCEPTABLE_CANDIDATE`;
- `.llmgc/manual/**` is never in expected changed paths/file index;
- Visual World workspace surfaces the Goal115 group;
- product smoke over real repository artifacts.

Tests must be deterministic and BCL-only.

## Validation commands

Run, at minimum:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~OfflineGeoworldAlphaHumanResultRevalidation|FullyQualifiedName~Goal115|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-115-offline-geoworld-alpha-human-result-revalidation
git diff --check
git diff --cached --check
```

Also run before commit:

```powershell
git diff --cached --name-only
git status --short --untracked-files=all
```

The staged file list must not include:

```text
.llmgc/manual/**
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/**
unity/LLMGameCreatorAlpha/**/*.meta
```

## Commit / push policy

Commit and push to `origin/main` are required.

Commit message format:

```text
GREEN Goal 115 offline geoworld alpha human result revalidation
```

or:

```text
BLOCKED Goal 115 offline geoworld alpha human result revalidation
```

or:

```text
FAILED Goal 115 offline geoworld alpha human result revalidation
```

Use `GREEN` only if the real local manual result validates as an acceptable candidate.
Push the commit to `origin/main`.

## Final report

The final report must include:

```text
status GREEN/BLOCKED/FAILED
commit SHA
push result
worktree status
manual result path
manual result staged/committed: yes/no
manual result SHA-256
decision status
acceptableCandidate
recommendedHumanDecision
acceptedByCodex
humanAcceptanceStillRequired
step summary
errors/warnings
changed files summary
forbidden-zone confirmation
validation command results
artifact-scope result
source health summary
remaining debt
```
