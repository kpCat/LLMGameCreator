# Goal 117 — Offline Geoworld Alpha Post-Acceptance Continuation Selection

## Repo

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Human/technical context

Goal116 recorded explicit human acceptance for:

```text
offline_geoworld_alpha_manual_acceptance_verification
```

Accepted statement:

```text
Я принимаю offline_geoworld_alpha_manual_acceptance_verification по Goal115 GREEN_ACCEPTABLE_CANDIDATE.
```

Goal117 must not implement live geodata, Runtime consumers, public schema, providers, Lua, generator-library, final art, atlas, Unity scenes/prefabs/settings, or release packaging. It must only create a post-acceptance continuation-selection surface.

## What this gives hands-on

After Goal117, the Visual World Stream Preview Workspace should show:

- Goal116 manual gate accepted by human;
- the accepted manual result hash;
- a post-acceptance continuation matrix;
- which lanes are READY, candidate-only, blocked-by-policy, or require explicit future approval;
- the recommended next bounded goal id.

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
.llmgc/procedural/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record/offline-geoworld-alpha-manual-gate-acceptance-record.json
.llmgc/procedural/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record/offline-geoworld-alpha-manual-gate-acceptance-dashboard.json
.llmgc/procedural/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record/offline-geoworld-alpha-manual-gate-acceptance-quality-gate-scan.json
.llmgc/procedural/goal-115-offline-geoworld-alpha-human-result-revalidation/offline-geoworld-alpha-human-result-revalidation-decision-snapshot.json
```

The local `.llmgc/manual/**` result may still exist as untracked human input. Do not stage it. Do not require it to exist; Goal116 hash evidence is enough.

## Allowed paths

```text
docs/agent-tasks/goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection/**
.devflow/artifact-scope/artifact-scope-policy.json
.llmgc/procedural/goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection/**
.llmgc/exports/goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection/**
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/offline-geoworld-alpha-post-acceptance-continuation-selection.md
src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaPostAcceptanceContinuationSelection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Status.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal117.cs
tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaPostAcceptanceContinuationSelection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaPostAcceptanceContinuationSelectionProductSmokeTests.cs
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

## Exact behavior

Add a bounded BCL-only Application service:

```text
src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaPostAcceptanceContinuationSelection/
```

It must read Goal116 acceptance evidence and verify:

- `manualGate == offline_geoworld_alpha_manual_acceptance_verification`
- `manualGateStatus == ACCEPTED_BY_HUMAN`
- `humanAccepted == true`
- `sourceDecisionStatus == GREEN_ACCEPTABLE_CANDIDATE`
- `manualResultSha256 == 8c2ad299d241d4315248b642b723ae8cf33ecabaa42a46462985ea5dc8335aeb`
- `acceptedByCodex == false`
- `manualInputNotCommitted == true`
- `rawManualResultEmbeddedInArtifacts == false`

Produce a deterministic continuation matrix with these lanes:

1. `accepted_alpha_baseline_review`
   - status: `READY`
   - recommended next goal id: `goal-118-offline-geoworld-accepted-alpha-baseline-review`

2. `offline_bundle_import_policy_scaffold`
   - status: `CANDIDATE_REQUIRES_EXPLICIT_APPROVAL`
   - no network, no providers, no Runtime, no public schema.

3. `unity_visual_consumption_or_playable_rendering`
   - status: `CANDIDATE_REQUIRES_EXPLICIT_APPROVAL`
   - no scenes/prefabs/settings in Goal117.

4. `runtime_or_gamepackage_consumers`
   - status: `BLOCKED_REQUIRES_EXPLICIT_SCHEMA_RUNTIME_TASK`

5. `live_geodata_provider_network`
   - status: `BLOCKED_BY_POLICY`

6. `release_packaging`
   - status: `BLOCKED_NOT_RELEASE_READY`

7. `visual_final_renderer_atlas`
   - status: `CANDIDATE_REQUIRES_RENDERER_DECISION`

The recommended lane must be:

```text
accepted_alpha_baseline_review
```

The recommended next goal id must be:

```text
goal-118-offline-geoworld-accepted-alpha-baseline-review
```

Goal117 must not create Goal118 task files.

## Required artifacts

Create deterministic artifacts under:

```text
.llmgc/procedural/goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection/
.llmgc/exports/goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection/
```

Recommended files:

```text
offline-geoworld-alpha-post-acceptance-continuation-dashboard.json
offline-geoworld-alpha-post-acceptance-continuation-matrix.json
offline-geoworld-alpha-post-acceptance-continuation-report.md
offline-geoworld-alpha-post-acceptance-continuation-quality-gate-scan.json
offline-geoworld-alpha-post-acceptance-continuation-negative-proof.json
offline-geoworld-alpha-post-acceptance-continuation-file-index.json
```

Do not embed raw `.llmgc/manual/**`.

## Visual World Stream Preview Workspace

Add a read-only Goal117 section showing:

```text
manualGateStatus
humanAccepted
recommendedNextLane
recommendedNextGoalId
readyLaneCount
candidateLaneCount
blockedLaneCount
doNotStartAutomatically
evidencePath
exportPath
```

## Docs and debt

Update docs/current state so they say:

- Goal116 accepted the manual gate by explicit human decision.
- Goal117 recommends post-acceptance baseline review as the next bounded lane.
- No automatic live geodata/provider/network/Runtime/schema/Lua/generator-library/final-art/atlas/Unity scene/prefab/project-settings/release-packaging work is authorized.
- Next task must be explicitly selected from the matrix.

Keep geospatial ingestion, provider/legal, Runtime/schema, Unity consumption and final renderer debts open.

## Artifact-scope policy

Add scenario:

```text
goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection
```

It must allow only Goal117 expected files and exclude `.llmgc/manual/**`.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~OfflineGeoworldAlphaPostAcceptanceContinuationSelection|FullyQualifiedName~Goal117|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

`git ls-files .llmgc/manual` must be empty.

Untracked `.llmgc/manual/...result.json` may remain; do not stage it.

## Quality gate

GREEN requires:

- Goal116 acceptance record present and valid.
- Manual gate status is `ACCEPTED_BY_HUMAN`.
- Recommended lane is `accepted_alpha_baseline_review`.
- Recommended next goal id is `goal-118-offline-geoworld-accepted-alpha-baseline-review`.
- Matrix contains all required lanes and boundaries.
- `.llmgc/manual/**` is not staged/committed/embedded.
- No forbidden path changes.
- Tests/checks pass.
- Artifact scope passes.
- Final worktree after push is clean except possible expected untracked `.llmgc/manual/...result.json`.

BLOCKED if Goal116 evidence is missing or not accepted, or if the matrix cannot be produced without forbidden changes.

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
GREEN Goal 117 offline geoworld alpha post-acceptance continuation selection
BLOCKED Goal 117 offline geoworld alpha post-acceptance continuation selection
FAILED Goal 117 offline geoworld alpha post-acceptance continuation selection
```

Push to `origin/main`.

Final report must include commit SHA, push status, grouped changed files, `.llmgc/manual/**` status, recommended lane, recommended next goal id, validations and remaining debt.
