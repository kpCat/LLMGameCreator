# Goal 142 — Runtime-Significant Product-Line Variant Matrix + Selection Handoff

## Task ID

`goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product goal.

This is not another projection/player-HUD wrapper. Goal141A corrected the Runtime request/response seam. Goal142 must now prove that the narrow alpha kernel is expansion-safe and product-line driven rather than a single hardcoded demo.

## Strategic intent

The project is a data-driven game product-line combiner.

Current weakness:

```text
Goal131 recipe variants are metadata-only.
Goal134-141 execute only minimal-map-game-balanced-baseline.
```

Goal142 must establish:

```text
read-only template
→ runtime-significant variant recipes
→ materialized GamePackage candidates
→ package validation
→ corrected Runtime request/response execution
→ candidate-specific runtime outcomes
→ runtime-aware scoring
→ selected candidate handoff
→ WinForms/VisualWorld operator surface
```

The variants must remain inside the existing public GamePackage schema. No schema changes are allowed.

## Required read-first

Read in order:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md

.llmgc/procedural/goal-141a-player-command-roundtrip-semantic-correctness-hotfix/roundtrip-semantic-correctness-dashboard.json
.llmgc/procedural/goal-141a-player-command-roundtrip-semantic-correctness-hotfix/roundtrip-semantic-correctness-regression-proof.json

.llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/runtime-backed-player-command-roundtrip-result.json
.llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/runtime-backed-player-command-roundtrip-request.json

.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/candidate-recipe-catalog.json
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/gamepackage-recipe-pipeline-result.json

samples/minimal-map-game/package.json

src/LLMGameCreator.Runtime/RuntimeBackedPlayerCommandRoundtripService.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Runtime.Abstractions/RuntimeBackedPlayerCommandRoundtripContracts.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/RuntimeBackedPlayerCommandRoundtripArtifactService.cs
```

## Scope principle

Goal142 may create new candidate packages only under Goal142 procedural/export roots.

It must not mutate:

```text
samples/minimal-map-game/package.json
Goal131 historical candidate artifacts
Goal141/141A historical evidence
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/**
.devflow/artifact-scope/artifact-scope-policy.json

.devflow/scripts/run-product-line-runtime-variant-matrix.ps1
.devflow/scripts/run-product-line-runtime-variant-matrix.cmd

.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/**
.llmgc/exports/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/runtime-significant-product-line-variant-matrix-and-selection-handoff.md

src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/**
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineRuntimeVariantMatrixModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineRuntimeVariantMatrixArtifactService.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal142.cs

src/LLMGameCreator.Runtime.Abstractions/RuntimeBackedPlayerCommandRoundtripContracts.cs
src/LLMGameCreator.Runtime/RuntimeBackedPlayerCommandRoundtripService.cs
src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs

tests/LLMGameCreator.Tests/Application/ProductLineRuntimeVariantMatrix/**
tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/ProductLineRuntimeVariantMatrixScriptProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal142Tests.cs
tests/LLMGameCreator.Tests/Devflow/RunProductLineRuntimeVariantMatrixScriptTests.cs
tests/LLMGameCreator.Tests/Runtime/RuntimeBackedPlayerCommandRoundtripServiceTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

Runtime files may be changed only for bounded parameterization/correctness needed to execute the same canonical vertical slice over multiple candidate packages. Do not add another parallel runtime.

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

unity/LLMGameCreatorAlpha/Assets/**
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**

*.sln
*.csproj
Directory.Build.*
dependency/package files
```

No Unity work in Goal142. No public schema change. No sample mutation. No provider/media/LLM/Lua/generator-library work.

## Required normal command

Add:

```bat
.devflow\scripts\run-product-line-runtime-variant-matrix.cmd
```

PowerShell script:

```text
.devflow/scripts/run-product-line-runtime-variant-matrix.ps1
```

Supported parameters:

```text
-TemplatePackagePath
-VariantCatalogPath
-OutputRoot
-DryRun
-ApplyCleanup
```

Defaults:

```text
TemplatePackagePath = samples/minimal-map-game/package.json
VariantCatalogPath = .llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/product-line-runtime-variant-catalog.json
OutputRoot = .llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff
```

Script behavior:

1. Validate all paths stay under repo root.
2. Refuse `.llmgc/manual/**`.
3. Refuse writing outside Goal142 output root.
4. Hash the read-only source template before materialization.
5. Materialize all variants.
6. Hash the source template again and prove unchanged.
7. Validate every candidate package.
8. Execute corrected Goal141A Runtime request/response sequence for every candidate.
9. Write per-candidate result/snapshots/report.
10. Compute runtime-aware scores.
11. Write aggregate matrix and selected-candidate handoff.
12. Return non-zero on any failed candidate, missing runtime distinctness, forbidden mutation, or dishonest score.

## Variant catalog

Create:

```text
.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/product-line-runtime-variant-catalog.json
```

It must define exactly four deterministic variants:

```text
balanced_baseline
alchemy_focus
combat_focus
exploration_resource_focus
```

Each entry must include:

```text
recipeId
candidateId
displayName
variantKind
runtimeSignificant=true
mutationOperations[]
expectedRuntimeEffects[]
selectionWeights
requiredAnchors[]
```

The catalog must not use `metadata_only`.

## Candidate materialization

Create candidate packages under:

```text
.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/candidates/<candidate-id>/package.json
```

Also mirror compact selected/export artifacts under Goal142 export root.

All candidates must preserve the canonical vertical-slice anchors:

```text
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
```

## Runtime-significant mutation requirements

Use only fields already present in the current GamePackage schema/template.

Before implementing mutations, inspect the actual JSON shape and existing validators. Do not invent unsupported fields.

### balanced_baseline

- Preserve baseline gameplay values.
- Candidate metadata may identify the variant.
- Runtime result serves as comparison baseline.

### alchemy_focus

Apply at least two runtime-significant changes involving existing alchemy/inventory/resource/recipe data, for example:

```text
larger starting herb/water supply
different healing-potion output quantity
different recipe resource cost that remains executable
different starting mana that remains valid
```

Required observed effect:

```text
final inventory/resource summary differs from baseline
final runtime state hash differs from baseline
craft request still passes
```

### combat_focus

Apply at least two runtime-significant changes involving existing encounter/combat/stat/reward data, for example:

```text
player or enemy health
basic attack/slash damage
encounter reward
starting combat resource
```

Required observed effect:

```text
combat summary or post-combat resource state differs from baseline
final runtime state hash differs from baseline
combat request still passes
```

### exploration_resource_focus

Apply at least two runtime-significant changes involving existing harvest/loot/container/transaction/resource data, for example:

```text
apple-tree loot quantity
starting chest/player inventory
transaction price/output
harvest output values
```

Required observed effect:

```text
harvest/transaction/inventory summary differs from baseline
final runtime state hash differs from baseline
harvest and transaction requests still pass
```

Do not rely on title/description/version-only changes as runtime-significant evidence.

## Mutation engine

Implement a deterministic BCL-only variant materializer in Application code.

Recommended files:

```text
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantCatalog.cs
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantMaterializer.cs
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantValidator.cs
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantMatrixService.cs
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantScoringService.cs
```

Requirements:

- No string replacement over raw JSON for semantic mutations.
- Parse to an appropriate typed or structured JSON representation.
- Mutation operations must have explicit target IDs/paths, old value proof, new value proof.
- Fail if target is absent, ambiguous, unsupported, or already has an unexpected value.
- Preserve deterministic serialization where practical.
- Write mutation audit records per candidate.
- Preserve the source template unchanged.

## Package validation

For every candidate:

```text
candidate file exists
valid JSON
existing package validator passed
handoff candidateId matches package candidate metadata
required anchors present
no broken required references
source template unchanged
candidate package under Goal142 root
```

Write:

```text
candidates/<candidate-id>/package-validation.json
candidates/<candidate-id>/mutation-audit.json
```

## Corrected Runtime roundtrip matrix

For each candidate, invoke the corrected Goal141A semantics:

```text
totalControlRequestCount=6
runtimeRoutedRequestCount=4
presentationOnlyRequestCount=2
runtimeExecutedRequestCount=4
presentationOnlyRuntimeExecutionCount=0
requestResponseCorrelationPassed=true
sequentialCursorContinuityPassed=true
stateHashContinuityPassed=true
copySummaryStateUnchanged=true
loadModelStateUnchanged=true
playAllExecutedRemainingCommands=true
noControlIntentMappedToUnrelatedGameplayCommand=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
```

Per-candidate output:

```text
matrix/<candidate-id>/roundtrip-request.json
matrix/<candidate-id>/roundtrip-result.json
matrix/<candidate-id>/roundtrip-snapshots.json
matrix/<candidate-id>/runtime-outcome-summary.json
matrix/<candidate-id>/candidate-score.json
```

The Runtime service must use the candidate package supplied for that row. No fallback to Goal131 selected candidate is permitted.

## Runtime distinctness proof

Aggregate proof must include:

```text
candidateCount=4
passedCandidateCount=4
failedCandidateCount=0
runtimeSignificantCandidateCount=4
allPackageHashesDistinct=true
allMutationAuditsPassed=true
allRoundtripSemanticProofsPassed=true
baselineFinalStateHash
alchemyFinalStateHash
combatFinalStateHash
explorationFinalStateHash
distinctFinalStateHashCount >= 3
alchemyRuntimeEffectObserved=true
combatRuntimeEffectObserved=true
explorationRuntimeEffectObserved=true
noMetadataOnlyVariantAccepted=true
sourceTemplateUnmodified=true
```

A candidate does not count as runtime-significant merely because its package hash differs. The expected runtime effect must be visible in runtime snapshots/state summaries.

## Runtime-aware scoring

Score candidates from actual evidence, not declared metadata.

Minimum score components:

```text
packageValidation
roundtripSemanticCorrectness
requiredAnchorCoverage
mutationAudit
runtimeEffectObserved
runtimeStateDistinctness
noBlockingDiagnostics
profile-specific objective
```

Write an explanation for every score component.

Selection must be deterministic. Tie-break order must be explicit.

The selected candidate must not automatically be the baseline unless its runtime-aware score actually wins.

## Selected candidate handoff

Write:

```text
selected-runtime-variant/selected-runtime-variant-handoff.json
selected-runtime-variant/package.json
selected-runtime-variant/runtime-outcome-summary.json
selected-runtime-variant/selection-rationale.md
```

Required handoff fields:

```text
candidateId
recipeId
variantKind
packagePath
packageSha256
roundtripResultPath
runtimeOutcomeSummaryPath
finalStateHash
score
scoreBreakdown
selectionReason
runtimeSignificant=true
projectionOnly=false
runtimeAuthority=true
accepted=false
```

Do not mutate or overwrite Goal131 selected-candidate handoff.

## WinForms / VisualWorld operator surface

Add a Goal142 section showing:

```text
matrixStatus
candidateCount
passedCandidateCount
failedCandidateCount
runtimeSignificantCandidateCount
distinctFinalStateHashCount
selectedCandidateId
selectedVariantKind
selectedScore
sourceTemplateUnmodified
normalCommand
matrixResultPath
selectedHandoffPath
accepted
```

Add one primary operator action if low-risk and testable:

```text
Run Runtime Variant Matrix
```

Requirements:

- asynchronous process execution;
- disable action while running;
- capture exit code and bounded output tail;
- refresh Goal142 status after completion;
- no JSON editing in UI;
- no direct gameplay mutation in UI.

If adding the action would destabilize the page, the read-only section plus copyable normal command is acceptable, but document the reason in evidence.

## Required artifacts

Under procedural and compact export roots:

```text
product-line-runtime-variant-catalog.json
product-line-runtime-variant-matrix-dashboard.json
product-line-runtime-variant-matrix-result.json
product-line-runtime-variant-mutation-summary.json
product-line-runtime-variant-distinctness-proof.json
product-line-runtime-variant-scoreboard.json
product-line-runtime-variant-negative-proof.json
product-line-runtime-variant-file-index.json
one-click-product-line-runtime-variant-matrix-report.json
one-click-product-line-runtime-variant-matrix-report.md
```

Plus candidate/matrix/selected-runtime-variant subtrees described above.

## Current state

Update current-state docs with:

```text
productLineRuntimeVariantMatrix=true
runtimeSignificantVariantCoverage=true
candidateCount=4
passedCandidateCount=4
runtimeSignificantCandidateCount=4
distinctFinalStateHashCount>=3
selectedRuntimeVariantId=<actual>
sourceTemplateUnmodified=true
projectionOnly=false
runtimeAuthority=true
goal141Accepted=false
goal142Accepted=false
```

Goal141 remains unaccepted; do not fabricate human acceptance.

## Artifact-scope scenario

Add:

```text
goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff
```

## Validation

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~ProductLineRuntimeVariantMatrix|FullyQualifiedName~Goal142|FullyQualifiedName~RuntimeBackedPlayerCommandRoundtrip|FullyQualifiedName~VisualWorldStreamPreviewWorkspace|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection"
.\.devflow\scripts\run-product-line-runtime-variant-matrix.ps1 -DryRun
.\.devflow\scripts\run-product-line-runtime-variant-matrix.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Forbidden diff:

```powershell
git diff --name-only -- samples/minimal-map-game .llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion .llmgc/exports/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion .llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge .llmgc/exports/goal-141-runtime-backed-unity-player-command-roundtrip-bridge .llmgc/procedural/goal-141a-player-command-roundtrip-semantic-correctness-hotfix .llmgc/exports/goal-141a-player-command-roundtrip-semantic-correctness-hotfix src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha
```

Also check changed files for mojibake and escaped Cyrillic markers.

## Quality gate

GREEN requires:

- four materialized runtime-significant candidates;
- all four package validations pass;
- all four corrected roundtrip runs pass;
- no presentation-only control executes Runtime;
- at least three distinct final runtime state hashes;
- each focus variant demonstrates its declared runtime effect;
- deterministic runtime-aware score and selected-candidate handoff;
- source sample unchanged;
- tests/checks/artifact scope pass;
- no forbidden changes;
- no `.llmgc/manual/**` tracked/staged;
- final git status clean.

BLOCKED if existing schema cannot represent one of the required variant dimensions; choose a different existing-schema runtime-significant dimension and document it.

FAILED if variants remain metadata-only, runtime effects are inferred only from package hashes, historical artifacts are overwritten, or forbidden changes are required.

## Commit / push policy

Commit and push with one of:

```text
GREEN Goal 142 runtime-significant product-line variant matrix and selection handoff
BLOCKED Goal 142 runtime-significant product-line variant matrix and selection handoff
FAILED Goal 142 runtime-significant product-line variant matrix and selection handoff
```

Final report must include:

- commit SHA;
- four candidate IDs;
- mutation dimensions;
- validation and corrected roundtrip result per candidate;
- final-state hash distinctness;
- selected candidate and score;
- selected handoff path;
- source-template unchanged proof;
- forbidden-zone confirmation;
- final git status.
