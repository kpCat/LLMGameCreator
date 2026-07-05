# Goal 116 — Offline Geoworld Alpha Manual Gate Acceptance Record

## Task ID

`goal-116-offline-geoworld-alpha-manual-gate-acceptance-record`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Local working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Human decision supplied by repository owner

The repository owner explicitly accepted the manual gate before this task:

```text
Я принимаю offline_geoworld_alpha_manual_acceptance_verification по Goal115 GREEN_ACCEPTABLE_CANDIDATE.
```

This is the only human acceptance statement for this task. Record it as evidence, but do not reinterpret it as final release approval, live geodata approval, Runtime approval, provider approval, schema approval, Lua approval, generator-library approval, scene/prefab/project-settings approval, final art approval, atlas approval, or release packaging approval.

## Why this goal exists

Goal115 revalidated the local human-created Goal110 result as:

- `decisionStatus = GREEN_ACCEPTABLE_CANDIDATE`
- `acceptableCandidate = true`
- `recommendedHumanDecision = READY_FOR_EXPLICIT_HUMAN_ACCEPTANCE_DECISION`
- `acceptedByCodex = false`
- `humanAcceptanceStillRequired = true`
- all 12 required checklist steps present exactly once and passed
- manual result SHA-256: `8c2ad299d241d4315248b642b723ae8cf33ecabaa42a46462985ea5dc8335aeb`

Goal116 must close only this manual gate:

```text
offline_geoworld_alpha_manual_acceptance_verification
```

It must not start a new feature line.

## Current expected working-tree precondition

The working tree may contain exactly this untracked local human input:

```text
.llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json
```

This file is expected to remain untracked and uncommitted.

Before doing any work, run and record:

```powershell
git status --short --untracked-files=all
git ls-files .llmgc/manual
git check-ignore -v .llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json
```

Expected:

- `git status --short --untracked-files=all` may show the one `.llmgc/manual/...result.json` file.
- `git ls-files .llmgc/manual` must return no tracked files.
- `git check-ignore -v ...` may return no output; the manual file does not need to be ignored, but it must not be staged or committed.

If there are Unity-generated `.meta`, `ProjectSettings`, `Packages`, `Library`, `Temp`, or unrelated files, do not proceed until they are cleaned. Do not commit them.

## Read first

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
.llmgc/procedural/goal-115-offline-geoworld-alpha-human-result-revalidation/offline-geoworld-alpha-human-result-revalidation-decision-snapshot.json
.llmgc/procedural/goal-115-offline-geoworld-alpha-human-result-revalidation/offline-geoworld-alpha-human-result-revalidation-dashboard.json
.llmgc/procedural/goal-115-offline-geoworld-alpha-human-result-revalidation/offline-geoworld-alpha-human-result-revalidation-quality-gate-scan.json
docs/manual-acceptance/offline-geoworld-alpha-human-result-revalidation.md
```

Also read this local untracked input only to verify its hash. Do not stage it:

```text
.llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json
```

## Allowed files and directories

You may create or modify only these paths:

```text
docs/agent-tasks/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record/**
.devflow/artifact-scope/artifact-scope-policy.json

.llmgc/procedural/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record/**
.llmgc/exports/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/offline-geoworld-alpha-manual-gate-acceptance-record.md

src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualGateAcceptanceRecord/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Status.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal116.cs

tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaManualGateAcceptanceRecord/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaManualGateAcceptanceRecordProductSmokeTests.cs
```

## Forbidden files and directories

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
public GamePackage schema files
provider / LLM / RAG / media provider code
Lua / Scripting code
generator-library/**
*.sln
*.csproj
Directory.Build.*
package/dependency files
LFZ source or archive
```

Do not add live geodata ingestion, provider calls, network fetching, runtime online behavior, real map scraping, final art, atlas generation, scene/prefab changes, or release packaging.

## Exact behavior

Implement a bounded acceptance-record seam that consumes Goal115 evidence and the local manual input hash.

### Application acceptance record

Add a BCL-only Application service under:

```text
src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualGateAcceptanceRecord/
```

It must:

1. Load the Goal115 decision snapshot.
2. Verify:
   - `decisionStatus == GREEN_ACCEPTABLE_CANDIDATE`
   - `acceptableCandidate == true`
   - `recommendedHumanDecision == READY_FOR_EXPLICIT_HUMAN_ACCEPTANCE_DECISION`
   - `acceptedByCodex == false`
   - `humanAcceptanceStillRequired == true`
   - `manualGateRemainsHumanDecision == true`
   - `manualResultSha256 == 8c2ad299d241d4315248b642b723ae8cf33ecabaa42a46462985ea5dc8335aeb`
   - required step count is 12
   - passed step count is 12
   - errors and warnings are empty
3. Read the local manual result file only to compute/confirm SHA-256. Do not copy the raw JSON into any committed artifact.
4. Produce an acceptance record with:
   - `goalId = goal_116_offline_geoworld_alpha_manual_gate_acceptance_record`
   - `manualGate = offline_geoworld_alpha_manual_acceptance_verification`
   - `manualGateStatus = ACCEPTED_BY_HUMAN`
   - `humanAccepted = true`
   - `humanDecisionStatement = "Я принимаю offline_geoworld_alpha_manual_acceptance_verification по Goal115 GREEN_ACCEPTABLE_CANDIDATE."`
   - `sourceDecisionStatus = GREEN_ACCEPTABLE_CANDIDATE`
   - `manualResultSha256 = 8c2ad299d241d4315248b642b723ae8cf33ecabaa42a46462985ea5dc8335aeb`
   - `manualInputNotCommitted = true`
   - `rawManualResultEmbeddedInArtifacts = false`
   - `acceptedByCodex = false`
   - `notFinalReleaseOrRuntimeBuild = true`
   - `noRuntimeProviderOrNetworkChanges = true`
   - `noUnityFileChangesRequired = true`
5. Produce negative proof that rejects:
   - missing Goal115 snapshot
   - non-green Goal115 decision
   - manual hash mismatch
   - raw manual result embedded into artifacts
   - `.llmgc/manual/**` staged/committed
   - forbidden Runtime/provider/schema/Lua/generator-library/Unity scene/settings/package changes
6. Produce deterministic artifacts in:
   - `.llmgc/procedural/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record/`
   - `.llmgc/exports/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record/`

Recommended artifact names:

```text
offline-geoworld-alpha-manual-gate-acceptance-record.json
offline-geoworld-alpha-manual-gate-acceptance-dashboard.json
offline-geoworld-alpha-manual-gate-acceptance-report.md
offline-geoworld-alpha-manual-gate-acceptance-quality-gate-scan.json
offline-geoworld-alpha-manual-gate-acceptance-negative-proof.json
offline-geoworld-alpha-manual-gate-acceptance-file-index.json
```

### WinForms / Visual World Stream Preview Workspace

Surface a read-only Goal116 section in the existing Visual World Stream Preview Workspace.

Show at least:

```text
manualGate
manualGateStatus
humanAccepted
sourceDecisionStatus
manualResultSha256
acceptedByCodex
manualInputNotCommitted
rawManualResultEmbeddedInArtifacts
recommendedNextDecision
```

### Docs/current state

Update current-state and queue docs so they clearly say:

- `offline_geoworld_alpha_manual_acceptance_verification` is accepted by explicit human decision in Goal116.
- Goal116 is not final release and not permission to start live geodata/provider/runtime/schema/Lua/generator-library/final-art/atlas/scene/prefab/project-settings/release-packaging work.
- `.llmgc/manual/**` remains local human input and must not be committed.
- The next safe step is post-acceptance continuation selection, not automatic live geodata or Runtime work.

Update debt/registers:

- Mark `GQ-P2-GOAL111-MANUAL-RESULT-MISSING` as resolved or superseded by Goal115/Goal116.
- Keep geoworld ingestion/consumer/final renderer/Unity consumption debts open.

### Artifact-scope policy

Add a `goal-116-offline-geoworld-alpha-manual-gate-acceptance-record` scenario.

It must:

- allow only Goal116 expected files;
- exclude `.llmgc/manual/**`;
- fail if `.llmgc/manual/**` is staged/committed;
- fail if Unity ProjectSettings/Packages/scenes/prefabs/StreamingAssets or forbidden code areas appear.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~OfflineGeoworldAlphaManualGateAcceptanceRecord|FullyQualifiedName~Goal116|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-116-offline-geoworld-alpha-manual-gate-acceptance-record
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

`git ls-files .llmgc/manual` must be empty.

After staging, verify:

```powershell
git diff --cached --name-only
git diff --cached --name-only | Select-String -SimpleMatch ".llmgc/manual"
```

The second command must produce no matches.

If `check-spine-fast.ps1` prints known historical Goal084-088 product-smoke noise but exits successfully, record it as non-blocking.

Unity batchmode compile is not required because this task must not change Unity files.

## Quality gate

GREEN requires all of these:

- Goal115 decision snapshot is `GREEN_ACCEPTABLE_CANDIDATE`.
- Local manual result file exists and SHA-256 matches `8c2ad299d241d4315248b642b723ae8cf33ecabaa42a46462985ea5dc8335aeb`.
- Human decision statement is recorded exactly.
- Manual gate status is `ACCEPTED_BY_HUMAN`.
- `acceptedByCodex=false`.
- `.llmgc/manual/**` is not staged, not committed, not embedded.
- No forbidden file changes.
- Required tests and checks pass.
- Artifact-scope scenario passes.
- Final worktree after push is clean except the expected untracked `.llmgc/manual/...result.json`.

BLOCKED if:

- Goal115 snapshot is missing or not green.
- Manual result hash does not match.
- Human decision statement cannot be recorded.
- `.llmgc/manual/**` must be staged/committed to pass.
- Forbidden files must be touched.

FAILED if:

- The implementation corrupts existing accepted evidence, breaks build/tests, or cannot safely restore out-of-scope validation churn.

## Commit / push policy

Before committing:

```powershell
git diff --cached --name-only
git diff --cached --check
```

Do not stage `.llmgc/manual/**`.

Commit and push with one of:

```text
GREEN Goal 116 offline geoworld alpha manual gate acceptance record
BLOCKED Goal 116 offline geoworld alpha manual gate acceptance record
FAILED Goal 116 offline geoworld alpha manual gate acceptance record
```

Push to `origin/main`.

After push, report:

- final commit SHA
- whether push succeeded
- changed file list grouped by area
- whether `.llmgc/manual/**` remains untracked and uncommitted
- manual result SHA used
- manual gate status
- tests/checks run and results
- remaining debt
- final `git status --short --untracked-files=all`
