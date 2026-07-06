# Goal 134 — Canonical Runtime Selected Candidate Playthrough Matrix

## Task ID

`goal-134-canonical-runtime-selected-candidate-playthrough-matrix`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Strategic intent

Goal133A rebaselined the product: LLMGameCreator is a data-driven game product-line combiner, not prompt-to-game. The next product milestone must stop the projection-only chain and start the canonical runtime path.

Goal134 must prove this route:

```text
selected candidate package
→ package validation
→ canonical runtime playthrough
→ save/load/replay proof
→ Unity/player consumes canonical transcript/state summary
→ one-click report
```

This is the first canonical runtime pivot after the candidate recipe/scoring/operator pipeline. It must not become another projection-only wrapper.

## Required read-first

Read, in this order:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

.llmgc/procedural/goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot/product-line-strategy-rebaseline-dashboard.json
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/selected-candidate-handoff.json
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/package.json
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/gamepackage-recipe-pipeline-result.json

src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjection*.cs
```

Read existing Runtime/Application patterns before adding anything. Prefer extension by small focused services and tests.

## Goal boundary

This goal may touch canonical runtime code. It may not change public GamePackage schema.

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/**
.devflow/artifact-scope/artifact-scope-policy.json

.devflow/scripts/run-canonical-runtime-selected-candidate-playthrough.ps1
.devflow/scripts/run-canonical-runtime-selected-candidate-playthrough.cmd

.llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/**
.llmgc/exports/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/canonical-runtime-selected-candidate-playthrough-matrix.md

src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal134.cs

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeSelectedCandidateTranscriptAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionModels.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/Runtime/**
tests/LLMGameCreator.Tests/Runtime.Abstractions/**
tests/LLMGameCreator.Tests/DevFlow/RunCanonicalRuntimeSelectedCandidatePlaythroughScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
samples/minimal-map-game/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.AssetPipeline/**
src/LLMGameCreator.Scripting/**
provider / LLM / RAG / media provider code
Lua / Scripting code
generator-library/**
unity/LLMGameCreatorAlpha/Assets/Scenes/**
unity/LLMGameCreatorAlpha/Assets/Prefabs/**
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/**
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**
*.sln
*.csproj
Directory.Build.*
dependency/package files
```

No public GamePackage schema changes. No sample package mutation. No `.llmgc/manual/**`. No provider/media/LLM/Lua/generator-library work.

Unity changes are limited to a player/presentation consumer of canonical transcript/state summary. Unity must not execute gameplay truth.

## Required normal command

Add:

```bat
.devflow\scripts\run-canonical-runtime-selected-candidate-playthrough.cmd
```

The `.cmd` must call the `.ps1` with sane defaults.

The PowerShell script should support:

```text
-SelectedCandidateHandoffPath
-SelectedCandidatePackagePath
-OutputRoot
-UnityPath
-DryRun
-ApplyCleanup
```

Defaults:

```text
SelectedCandidateHandoffPath = .llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/selected-candidate-handoff.json
SelectedCandidatePackagePath = .llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/package.json
OutputRoot = .llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix
```

The script must:

1. Validate paths stay under repo root.
2. Refuse `.llmgc/manual/**`.
3. Refuse writing outside Goal134 output root.
4. Build/run the canonical runtime playthrough proof.
5. Run save/load/replay comparison.
6. Run Unity/player transcript smoke if Unity is available or through the same known Unity path discovery used by existing runner scripts.
7. Write compact result artifacts.
8. Apply bounded Unity cleanup when `-ApplyCleanup` is provided.
9. Return non-zero on failure.

## Canonical runtime deliverable

Add a real canonical runtime seam. Do not fake this in Unity projection or WinForms UI.

Minimum acceptable shape:

- Runtime/Runtime.Abstractions owns deterministic command/event/state contracts or service.
- Application may adapt selected candidate package JSON into runtime input, but gameplay state transitions must be produced by runtime-owned code.
- Runtime playthrough must produce a canonical transcript and state summary.
- Runtime playthrough must not call LLM/providers/network/Unity.
- State summary must be serializable and replayable.

Suggested naming, adjust to repository conventions:

```text
CanonicalRuntimeSelectedCandidatePlaythroughRequest
CanonicalRuntimeSelectedCandidatePlaythroughResult
CanonicalRuntimeSelectedCandidateCommand
CanonicalRuntimeSelectedCandidateEvent
CanonicalRuntimeSelectedCandidateState
CanonicalRuntimeSelectedCandidatePlaythroughService
CanonicalRuntimeSelectedCandidateSaveLoadReplayService
```

If existing Runtime services already cover this, reuse them and add only the selected-candidate adapter/matrix proof around them. Do not duplicate a parallel runtime if a suitable canonical service exists.

## Required runtime playthrough script

The selected candidate package should be executed through deterministic canonical runtime commands that cover at least:

```text
load selected package identity
validate package anchors
initialize world/map state
move/path preview to sign or interaction target
inspect sign
start/read old guard dialogue summary
evaluate quest/help_healer objective with inventory/player_start
summarize inventory/resources
craft or preview/apply recipe/healing_potion if possible
harvest or preview/apply node/apple_tree if possible
run one deterministic encounter/goblin_duel combat round
write final state hash and event transcript
```

If some action cannot be fully applied due existing runtime limits, the runtime result must explicitly mark:

```text
runtimePrimitiveMissing=true
```

and list the missing primitive. A GREEN goal may still pass only if at least the following are true:

```text
selectedCandidateLoaded=true
packageValidationPassed=true
canonicalRuntimeStarted=true
runtimeCommandCount >= 6
runtimeEventCount >= 6
stateHashChainPresent=true
saveLoadReplayPassed=true
unityConsumedCanonicalTranscript=true
projectionOnly=false
selectedCandidateExecutedByRuntime=true
```

Do not hide missing runtime primitives.

## Package validation deliverable

Before runtime execution, validate the selected candidate package:

```text
manifest/package identity present
selected-candidate handoff matches package
required anchors exist:
  map/village
  entity/village/sign
  interaction/sign_inspect
  entity/village/old_guard
  dialogue/old_guard_intro
  quest/help_healer
  inventory/player_start
  recipe/healing_potion
  node/apple_tree
  transaction/buy_healing_potion
  encounter/goblin_duel
no missing required references for the playthrough script
```

Use existing validators when available. If you add a selected-candidate validator, keep it Application-layer and focused; do not change public schema.

## Save/load/replay proof

Write deterministic artifacts:

```text
canonical-runtime-state-before-save.json
canonical-runtime-state-save.json
canonical-runtime-state-after-load.json
canonical-runtime-replay-transcript.json
canonical-runtime-save-load-replay-result.json
```

Required proof fields:

```text
saveStateHash
loadStateHash
replayStateHash
saveLoadHashMatch=true
replayHashMatch=true
eventHashChainMatch=true
```

## Selected candidate matrix

Goal134 is for selected candidate first, but keep the structure matrix-ready.

Write:

```text
canonical-runtime-selected-candidate-playthrough-matrix-result.json
```

It must contain at least one row for the selected candidate:

```text
candidateId
packagePath
packageValidationPassed
canonicalRuntimePassed
saveLoadReplayPassed
unityPlayerConsumedCanonicalTranscript
passed
```

## Unity/player canonical transcript consumer

Add minimal Unity/player batchmode consumer:

- It reads the canonical runtime transcript/state summary artifact produced by Goal134.
- It does not run game logic.
- It confirms that Unity/player can consume/display/parse canonical runtime output.
- It logs:

```text
GOAL134_CANONICAL_RUNTIME_TRANSCRIPT_PLAYER_PASS
```

or:

```text
GOAL134_CANONICAL_RUNTIME_TRANSCRIPT_PLAYER_FAIL
```

This is Tier 4 presentation proof, not gameplay truth.

## One-click report

Write:

```text
one-click-canonical-runtime-report.json
one-click-canonical-runtime-report.md
```

Required report fields:

```text
candidateId
packageValidationPassed
canonicalRuntimePassed
saveLoadReplayPassed
unityPlayerConsumedCanonicalTranscript
projectionOnly=false
selectedCandidateExecutedByRuntime=true
manualUnityOptional=true
nextRecommendedGoal
```

## Required artifacts

Under both procedural and export roots:

```text
selected-candidate-package-validation.json
canonical-runtime-playthrough-script.json
canonical-runtime-transcript.json
canonical-runtime-state-summary.json
canonical-runtime-state-before-save.json
canonical-runtime-state-save.json
canonical-runtime-state-after-load.json
canonical-runtime-replay-transcript.json
canonical-runtime-save-load-replay-result.json
canonical-runtime-selected-candidate-playthrough-matrix-result.json
unity-player-canonical-transcript-smoke.json
one-click-canonical-runtime-report.json
one-click-canonical-runtime-report.md
canonical-runtime-negative-proof.json
canonical-runtime-file-index.json
canonical-runtime-dashboard.json
```

Raw Unity `.log` files may remain local/ignored if compact smoke artifacts prove pass/fail.

## VisualWorld / WinForms proof surface

Add a read-only Goal134 section showing:

```text
candidateId
packageValidationPassed
canonicalRuntimePassed
runtimeCommandCount
runtimeEventCount
saveLoadReplayPassed
unityPlayerConsumedCanonicalTranscript
projectionOnly
selectedCandidateExecutedByRuntime
normalCommand
reportPath
matrixResultPath
manualUnityOptional
```

Do not require manual Unity inspection.

## Docs/current state

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
```

Current state must say Goal134 is the canonical runtime selected-candidate playthrough matrix.

Required dashboard/current-state markers:

```text
projectionOnly=false
canonicalRuntimeCoverage=true
saveLoadReplayCoverage=true
selectedCandidateExecutedByRuntime=true
unityConsumesCanonicalTranscript=true
manualUnityOptional=true
```

## Artifact-scope policy

Add scenario:

```text
goal-134-canonical-runtime-selected-candidate-playthrough-matrix
```

It must allow only expected Goal134 paths and exclude `.llmgc/manual/**`, samples, GamePackage schema, Generation, AssetPipeline, Scripting, generator-library, provider/media, Unity scenes/prefabs/settings/packages/StreamingAssets, solution/project/dependency files.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~CanonicalRuntimeSelectedCandidate|FullyQualifiedName~Goal134|FullyQualifiedName~SelectedCandidatePlaythrough|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\run-canonical-runtime-selected-candidate-playthrough.ps1 -DryRun
.\.devflow\scripts\run-canonical-runtime-selected-candidate-playthrough.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-134-canonical-runtime-selected-candidate-playthrough-matrix
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Also verify forbidden diffs:

```powershell
git diff --name-only -- samples/minimal-map-game src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha/Assets/Scenes unity/LLMGameCreatorAlpha/Assets/Prefabs unity/LLMGameCreatorAlpha/Assets/StreamingAssets unity/LLMGameCreatorAlpha/ProjectSettings unity/LLMGameCreatorAlpha/Packages
```

## Quality gate

GREEN requires:

- selected candidate loaded from Goal131 handoff;
- package validation passed;
- canonical runtime playthrough executed;
- runtime transcript and state summary written;
- state hash chain present;
- save/load/replay proof passed;
- Unity/player consumed canonical transcript/state summary;
- `projectionOnly=false`;
- `selectedCandidateExecutedByRuntime=true`;
- one-click report exists;
- tests/checks pass;
- artifact scope passes;
- no forbidden path changes;
- no `.llmgc/manual/**` tracked/staged;
- final git status clean.

BLOCKED if existing runtime services cannot support even a minimal canonical selected-candidate command sequence without forbidden schema changes. In that case, write a concrete blocker report with the missing runtime primitive list.

FAILED if build/tests break or forbidden changes are required.

## Commit / push policy

Commit and push with one of:

```text
GREEN Goal 134 canonical runtime selected candidate playthrough matrix
BLOCKED Goal 134 canonical runtime selected candidate playthrough matrix
FAILED Goal 134 canonical runtime selected candidate playthrough matrix
```

Push to `origin/main`.

Final report must include:

- commit SHA;
- selected candidate id;
- package validation result;
- runtime command/event counts;
- save/load/replay result;
- Unity/player transcript smoke result;
- one-click report path;
- forbidden-zone confirmation;
- final git status.
