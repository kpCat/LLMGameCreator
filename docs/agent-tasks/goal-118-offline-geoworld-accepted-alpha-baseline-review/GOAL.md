# Goal 118 — Offline Geoworld Accepted Alpha Baseline Review

## Repo

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Purpose

Goal117 selected the safe post-acceptance lane:

```text
accepted_alpha_baseline_review
```

and recommended:

```text
goal-118-offline-geoworld-accepted-alpha-baseline-review
```

Goal118 must turn the accepted offline geoworld Alpha Slice into a stable, reviewable baseline package and WinForms dashboard section.

This is not a new feature implementation. It is a post-acceptance baseline review/handoff package so the project knows exactly what was accepted, what remains only produced-for-review, what is explicitly forbidden, and which next lanes require separate approval.

## What this gives hands-on

After this goal, the user should be able to open the existing Visual World Stream Preview Workspace and see:

- accepted manual gate status from Goal116;
- the accepted manual result hash;
- a baseline id/hash/index;
- what Goal098-117 evidence chain was included;
- which artifacts are the accepted Alpha baseline proof;
- which items are still not final release / not Runtime / not live geodata;
- next-lane options, but no automatic transition into them.

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
.llmgc/procedural/goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection/offline-geoworld-alpha-post-acceptance-continuation-dashboard.json
.llmgc/procedural/goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection/offline-geoworld-alpha-post-acceptance-continuation-matrix.json
.llmgc/procedural/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record/offline-geoworld-alpha-manual-gate-acceptance-record.json
.llmgc/procedural/goal-115-offline-geoworld-alpha-human-result-revalidation/offline-geoworld-alpha-human-result-revalidation-decision-snapshot.json
.llmgc/procedural/goal-114-unity-safe-mode-compile-hotfix/unity-safe-mode-compile-hotfix-dashboard.json
.llmgc/procedural/goal-109-offline-geoworld-alpha-slice-export-package/offline-geoworld-alpha-export-manifest.json
.llmgc/procedural/goal-108-offline-geoworld-alpha-slice-orchestrator/offline-geoworld-alpha-slice-manifest.json
```

The local `.llmgc/manual/**` result may still exist as untracked human input. Do not stage it. Do not require it to exist. Never embed it.

## Allowed paths

```text
docs/agent-tasks/goal-118-offline-geoworld-accepted-alpha-baseline-review/**
.devflow/artifact-scope/artifact-scope-policy.json
.llmgc/procedural/goal-118-offline-geoworld-accepted-alpha-baseline-review/**
.llmgc/exports/goal-118-offline-geoworld-accepted-alpha-baseline-review/**
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/offline-geoworld-accepted-alpha-baseline-review.md
src/LLMGameCreator.Application/Design/OfflineGeoworldAcceptedAlphaBaselineReview/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Status.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal118.cs
tests/LLMGameCreator.Tests/Application/OfflineGeoworldAcceptedAlphaBaselineReview/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAcceptedAlphaBaselineReviewProductSmokeTests.cs
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

No live geodata ingestion, provider calls, network fetching, runtime online behavior, real map scraping, final art, atlas, scene/prefab changes, ProjectSettings/Packages changes, StreamingAssets changes, or release packaging.

## Exact behavior

Add a bounded BCL-only Application service:

```text
src/LLMGameCreator.Application/Design/OfflineGeoworldAcceptedAlphaBaselineReview/
```

It must consume Goal117/116/115/114/109/108 evidence and produce a deterministic accepted baseline review.

### Required validation

Verify from evidence:

- Goal116 manual gate status is `ACCEPTED_BY_HUMAN`.
- Goal116 `humanAccepted == true`.
- Goal116 `acceptedByCodex == false`.
- Goal116 `manualInputNotCommitted == true`.
- Goal116 `rawManualResultEmbeddedInArtifacts == false`.
- Goal117 recommended next lane is `accepted_alpha_baseline_review`.
- Goal117 recommended next goal id is `goal-118-offline-geoworld-accepted-alpha-baseline-review`.
- Goal117 has 1 ready lane, 3 candidate lanes and 3 blocked lanes.
- Goal114 Unity Safe Mode compile hotfix evidence exists.
- Goal109 portable export evidence exists.
- Goal108 alpha slice orchestrator evidence exists.

### Accepted baseline review model

Produce a baseline review with at least:

```text
goalId = goal_118_offline_geoworld_accepted_alpha_baseline_review
baselineId = offline_geoworld_alpha_accepted_baseline_v1
manualGate = offline_geoworld_alpha_manual_acceptance_verification
manualGateStatus = ACCEPTED_BY_HUMAN
acceptedByCodex = false
acceptedBaselineReady = true
manualResultSha256 = 8c2ad299d241d4315248b642b723ae8cf33ecabaa42a46462985ea5dc8335aeb
sourceGoalRange = Goal098-Goal117
includedSourceGoalCount
acceptedEvidenceRoots
producedOnlyHistoricalRoots
blockedOrSupersededNotes
notFinalReleaseOrRuntimeBuild = true
noRuntimeProviderOrNetworkChanges = true
noUnityFileChangesRequired = true
recommendedNextDecision = EXPLICIT_NEXT_LANE_SELECTION
```

### Required artifacts

Create deterministic artifacts under:

```text
.llmgc/procedural/goal-118-offline-geoworld-accepted-alpha-baseline-review/
.llmgc/exports/goal-118-offline-geoworld-accepted-alpha-baseline-review/
```

Recommended files:

```text
offline-geoworld-accepted-alpha-baseline-dashboard.json
offline-geoworld-accepted-alpha-baseline-manifest.json
offline-geoworld-accepted-alpha-baseline-source-index.json
offline-geoworld-accepted-alpha-baseline-report.md
offline-geoworld-accepted-alpha-baseline-quality-gate-scan.json
offline-geoworld-accepted-alpha-baseline-negative-proof.json
offline-geoworld-accepted-alpha-baseline-file-index.json
```

Artifacts must not embed raw `.llmgc/manual/**`.

### Negative proof

Reject/record as blocked:

- missing Goal116 accepted evidence;
- missing Goal117 post-acceptance routing evidence;
- manual input staged or embedded;
- live geodata/provider/network start;
- Runtime/schema/Lua/generator-library changes;
- Unity scenes/prefabs/settings/Packages/StreamingAssets changes;
- treating this as final release packaging.

### Visual World Stream Preview Workspace

Add a read-only Goal118 section showing at least:

```text
baselineId
acceptedBaselineReady
manualGateStatus
recommendedNextDecision
includedSourceGoalCount
acceptedEvidenceRootCount
producedOnlyRootCount
blockedOrSupersededNoteCount
doNotStartAutomatically
evidencePath
exportPath
```

### Docs/current state

Update current-state and queue docs so they clearly say:

- Goal118 created an accepted Alpha baseline review package after Goal116 human acceptance.
- This is not final release and does not authorize live geodata, providers, Runtime, schema, Lua, generator-library, final art, atlas, Unity scenes/prefabs/settings, StreamingAssets, or release packaging.
- The next safe user decision is explicit next-lane selection after reviewing the accepted baseline.
- Keep geospatial/legal/provider/runtime/schema/Unity consumption/final renderer debts open.

### Artifact-scope policy

Add scenario:

```text
goal-118-offline-geoworld-accepted-alpha-baseline-review
```

It must allow only Goal118 expected files and exclude `.llmgc/manual/**`.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~OfflineGeoworldAcceptedAlphaBaselineReview|FullyQualifiedName~Goal118|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-118-offline-geoworld-accepted-alpha-baseline-review
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

`git ls-files .llmgc/manual` must be empty.

If `.llmgc/manual/**` exists as untracked local input, leave it untracked. Do not stage it.

Unity batchmode compile is not required because this task must not change Unity files.

## Quality gate

GREEN requires:

- Goal116 acceptance record present and valid.
- Goal117 continuation selection present and recommends accepted baseline review.
- Accepted baseline review is produced.
- `.llmgc/manual/**` is not staged/committed/embedded.
- No forbidden path changes.
- Tests/checks pass.
- Artifact scope passes.
- Final worktree after push is clean except possible expected untracked `.llmgc/manual/...result.json`.

BLOCKED if accepted evidence is missing or baseline review requires forbidden changes.

FAILED if build/tests are broken or out-of-scope churn cannot be restored.

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
GREEN Goal 118 offline geoworld accepted alpha baseline review
BLOCKED Goal 118 offline geoworld accepted alpha baseline review
FAILED Goal 118 offline geoworld accepted alpha baseline review
```

Push to `origin/main`.

Final report must include commit SHA, push status, grouped changed files, `.llmgc/manual/**` status, accepted baseline id, recommended next decision, validations and remaining debt.
