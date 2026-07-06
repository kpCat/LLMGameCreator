# Goal 133A — Product-Line Strategy Rebaseline And Canonical Runtime Pivot

## Task ID

`goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Strategic reason

This goal is a strategic rebaseline after Goal132.

The owner clarified the core product identity:

```text
LLMGameCreator is not prompt-to-game and not an LLM that writes a game.
It is a data-driven game product-line combiner.
The user selects feature modules, rule packs, semantic packs, visual part-packs,
world/source options and player adapters through WinForms/combiner surfaces,
then receives a validated GamePackage.
LLM is only an optional local authoring assistant for lore, semantic drafts,
naming/prose/repair, with no runtime dependency.
```

The immediate risk is continuing the current projection-only candidate/review chain. Goal133A must pivot the next product milestone toward canonical runtime execution.

## Goal type

Docs/process rebaseline with compact evidence.

This is intentionally not a feature/projection goal. It is allowed because it changes the planning contract and prevents the next work from deepening `projectionOnly=true`.

## Current repository state to account for

The user already committed some strategy docs in a `docs update` commit after Goal132. Do not blindly overwrite them. Inspect and preserve useful content.

Known existing additions to verify and integrate:

```text
README.md
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md
docs/agent-tasks/goal-133a-product-line-strategy-rebaseline/GOAL.md
```

Goal133A must finish the integration:

- connect the new docs into `AGENTS.md`;
- connect the new docs into `docs/CONTEXT_INDEX.md`;
- update current state and queue;
- add compact evidence and final gate;
- explicitly route next product goal to canonical runtime path.

## Required read-first

Read these before editing:

```text
AGENTS.md
README.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md

docs/agent-tasks/goal-133a-product-line-strategy-rebaseline/GOAL.md
```

Also inspect Goal131/132 evidence enough to reference the current candidate pipeline state:

```text
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/gamepackage-recipe-pipeline-result.json
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/selected-candidate-handoff.json
.llmgc/procedural/goal-132-winforms-candidate-pipeline-operator-panel/candidate-pipeline-operator-dashboard.json
```

## Allowed paths

You may create or modify only:

```text
README.md
AGENTS.md

docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/agent-tasks/goal-133a-product-line-strategy-rebaseline/**
docs/agent-tasks/goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot/**

docs/manual-acceptance/product-line-strategy-rebaseline-and-canonical-runtime-pivot.md

.devflow/artifact-scope/artifact-scope-policy.json

.llmgc/procedural/goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot/**
.llmgc/exports/goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot/**

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineService.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineServiceTests.cs
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
unity/LLMGameCreatorAlpha/**
*.sln
*.csproj
Directory.Build.*
dependency/package files
```

No Unity changes. No Runtime changes. No Runtime.Abstractions changes. No GamePackage schema changes. No Lua/provider/media/generator-library changes. No sample package mutation. No `.llmgc/manual/**`.

## Exact required behavior

### 1. README product identity

Ensure `README.md` clearly states:

```text
LLMGameCreator is a data-driven game product-line combiner, not prompt-to-game.
```

It must also state:

```text
LLM is optional authoring assistance only and is not runtime authority.
GamePackage and canonical runtime state are the source of truth.
```

Preserve the useful content already added by the user.

### 2. Strategy docs

Ensure these docs exist, are coherent, and are not just placeholders:

```text
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md
```

They must explicitly define or reference:

```text
FeatureModule
RuntimePrimitive
SemanticPack
VisualPartPack
WorldSourceAdapter
PlayerAdapter
```

They must also state:

```text
Narrow alpha must be an expansion-safe kernel, not a hardcoded demo.
Projection-only goals are not enough for product readiness.
Canonical runtime playthrough is required for the next product milestone.
```

### 3. AGENTS.md connection

Update `AGENTS.md` so future agents read the new strategy docs without the owner repeating the instruction.

Required orientation order must include:

```text
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md
```

`AGENTS.md` must explicitly say:

```text
LLMGameCreator is a data-driven game product-line combiner, not prompt-to-game.
LLM is optional local authoring assistance only.
Next broad product work must preserve FeatureModule / RuntimePrimitive / SemanticPack / VisualPartPack / WorldSourceAdapter / PlayerAdapter seams.
```

### 4. CONTEXT_INDEX connection

Update `docs/CONTEXT_INDEX.md` so the routing index points to the three new strategy docs.

It must make them read-first for:

```text
broad generation work
candidate pipeline work
WinForms operator pipeline work
Runtime/player pivot work
Codex task shaping
roadmap/rebaseline decisions
```

### 5. Current state and queue

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

Required new gate:

```text
product_line_strategy_rebaseline_verification
```

Required state:

```text
accepted=false
manualUnityOptional=true
projectionOnlyStopCondition=true
nextProductGoal=goal_134_canonical_runtime_selected_candidate_playthrough_matrix
```

Required strategic statement:

```text
After Goal132/133A, the next product milestone is not another projection-only wrapper.
Goal134 must start the canonical runtime path:
candidate package -> package validation -> canonical runtime playthrough -> save/load/replay proof -> Unity/player consumes canonical transcript/state summary -> one-click report.
```

### 6. Old Goal133 routing

If `docs/agent-tasks/goal-133a-product-line-strategy-rebaseline/GOAL.md` already exists, reconcile it with this task. Do not leave conflicting instructions that suggest continuing the old Goal133 selected-candidate review package before the rebaseline.

If old Goal133 selected-candidate review is mentioned, re-route it as a later candidate review task only after canonical runtime pivot is established. It must not be the immediate next goal unless it is explicitly tied to canonical runtime.

### 7. Compact evidence

Add BCL-only evidence service:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineService.cs
```

It should inspect actual repo files and write/validate compact evidence:

```text
.llmgc/procedural/goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot/product-line-strategy-rebaseline-dashboard.json
.llmgc/procedural/goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot/product-line-strategy-rebaseline-doc-scan.json
.llmgc/procedural/goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot/product-line-strategy-rebaseline-negative-proof.json
.llmgc/procedural/goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot/product-line-strategy-rebaseline-report.md
.llmgc/procedural/goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot/product-line-strategy-rebaseline-file-index.json
```

Mirror compact export artifacts under:

```text
.llmgc/exports/goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot/
```

Dashboard must include:

```text
goalId=goal_133a_product_line_strategy_rebaseline_and_canonical_runtime_pivot
gate=product_line_strategy_rebaseline_verification
accepted=false
productLineCombiner=true
notPromptToGame=true
llmOptionalAuthoringOnly=true
newDocsPresent=true
agentsRoutingUpdated=true
contextIndexRoutingUpdated=true
currentStateUpdated=true
queueUpdated=true
nextGoal=goal_134_canonical_runtime_selected_candidate_playthrough_matrix
runtimeUnchanged=true
unityUnchanged=true
schemaUnchanged=true
samplePackageUnchanged=true
manualInputUnchanged=true
```

### 8. Artifact scope policy

Add scenario:

```text
goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot
```

It must allow only the Goal133A paths and exclude all forbidden zones.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~ProductLineStrategyRebaseline|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Also verify forbidden path diffs are empty:

```powershell
git diff --name-only -- samples/minimal-map-game src/LLMGameCreator.Runtime src/LLMGameCreator.Runtime.Abstractions src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha
```

No Unity batchmode smoke is required because Unity must not be changed.

## Quality gate

GREEN requires:

- README clearly says product-line combiner, not prompt-to-game.
- Three strategy docs exist and define the required seams.
- AGENTS.md routes agents to the new docs.
- CONTEXT_INDEX routes agents to the new docs.
- Current state and queue route next work to Goal134 canonical runtime path.
- Evidence artifacts exist and are based on actual file scans.
- Gate is `product_line_strategy_rebaseline_verification`.
- `accepted=false`.
- Runtime, Runtime.Abstractions, schema, Unity, Lua, provider/media, samples and `.llmgc/manual/**` are unchanged.
- Tests/checks pass.
- Final git status is clean.

BLOCKED if the repository has conflicting user-authored docs that cannot be reconciled safely.

FAILED if build/tests break or forbidden files must be changed.

## Commit / push policy

Commit and push with one of:

```text
GREEN Goal 133A product-line strategy rebaseline and canonical runtime pivot
BLOCKED Goal 133A product-line strategy rebaseline and canonical runtime pivot
FAILED Goal 133A product-line strategy rebaseline and canonical runtime pivot
```

Push to `origin/main`.

Final report must include:

- commit SHA;
- changed files grouped by docs/evidence/tests/policy;
- final gate;
- accepted=false confirmation;
- next goal name;
- forbidden-zone confirmation;
- final git status.
