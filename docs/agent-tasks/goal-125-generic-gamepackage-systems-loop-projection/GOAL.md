# Goal 125 — Generic GamePackage Systems Loop Projection

## Task ID

`goal-125-generic-gamepackage-systems-loop-projection`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product goal.

This is not a proof-only/review goal and it must not require manual button-by-button verification. The primary deliverable is a projection-only systems loop over `samples/minimal-map-game/package.json` that demonstrates inventory/resource/crafting/harvest/transaction/combat semantics without touching Runtime, public schema, providers, Lua, generator-library, scenes, prefabs, ProjectSettings, Packages, or StreamingAssets.

## Why this goal exists

Goal123 added generic GamePackage projection over `samples/minimal-map-game/package.json`.
Goal124 added projection-local quest/dialogue/interaction loop.

Goal125 must broaden the generic GamePackage path into a systems loop:

- inventory/resource state;
- recipe crafting preview/apply;
- resource node harvest preview/apply;
- transaction affordability preview;
- encounter/combat preview;
- deterministic event log;
- one-click batch/manual verification.

This is still Editor projection only. It must not become Runtime or schema work.

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

.llmgc/procedural/goal-124-generic-gamepackage-quest-dialogue-interaction-loop/generic-gamepackage-loop-dashboard.json
.llmgc/procedural/goal-123-generic-gamepackage-playable-projection-adapter/generic-gamepackage-projection-dashboard.json
.llmgc/procedural/goal-120a-clean-unity-editor-noise-empty-status-hotfix/clean-unity-editor-noise-empty-status-hotfix-dashboard.json

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionLoop.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionState.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-125-generic-gamepackage-systems-loop-projection/**
.devflow/artifact-scope/artifact-scope-policy.json

.llmgc/procedural/goal-125-generic-gamepackage-systems-loop-projection/**
.llmgc/exports/goal-125-generic-gamepackage-systems-loop-projection/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/generic-gamepackage-systems-loop-projection.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal125.cs

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionLoop.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionState.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionSystems.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
samples/minimal-map-game/**
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
```

No Runtime/schema/provider/Lua/generator-library work. Do not mutate the sample package. Do not save scenes/prefabs. Do not write StreamingAssets.

## Primary deliverable A — projection-only systems state

Extend or add projection-only state/model code to track:

```text
playerInventory
resourceLedger
recipePreview
recipeApplyResult
harvestPreview
harvestApplyResult
transactionPreview
encounterPreview
combatRoundPreview
systemsEventLog
```

The state must be in-memory only. No file writes.

## Primary deliverable B — deterministic systems loop over the sample package

Add a one-click Unity window action:

```text
Run Generic Package Systems Loop Verification
```

It must perform a deterministic projection-local sequence over `samples/minimal-map-game/package.json`:

1. Load sample package.
2. Build generic package projection.
3. Initialize player inventory from `inventory/player_start`.
4. Initialize resources from resource defaults, including at least `resource/health`, `resource/stamina`, `resource/mana`, and `resource/gold` if present.
5. Preview recipe `recipe/healing_potion`:
   - inputs: 2 `item/red_herb`, 1 `item/water_flask`;
   - cost: 5 `resource/mana`;
   - output: 1 `item/healing_potion`.
6. Apply recipe projection state if requirements are satisfied.
   - Expected sample state: red herbs 2 and water flask 1 are present, mana default 10 is present, so apply should pass.
   - Expected after apply: red herbs 0, water flask 0, mana 5, healing potion count increased by 1.
7. Preview harvest node `node/apple_tree`:
   - required tool metadata points to `item/woodcutting_axe`;
   - production includes 1 `item/log`;
   - harvest loot table points to `loot/apple_tree`.
8. Apply deterministic harvest preview:
   - require axe present;
   - add at least 1 log and deterministic apple loot preview;
   - decrement axe durability if modeled in projection state.
9. Preview transaction `transaction/buy_healing_potion`:
   - cost 25 `resource/gold`;
   - output 1 healing potion;
   - if projected gold is insufficient, mark as not affordable without failing the whole smoke.
10. Preview encounter `encounter/goblin_duel`:
    - player health 30;
    - goblin health 12;
    - player basic attack damage 4;
    - goblin slash damage 3;
    - compute one deterministic combat round preview: goblin health 8, player health 27.
11. Update visible projection diagnostics/text panels for inventory/resources/recipe/harvest/transaction/encounter/combat summaries.
12. Produce a readable systems event log.

This is not a full Runtime transaction engine. It is a deterministic projection loop using the sample package data.

## Primary deliverable C — visible projection markers/panels

Add/update visible projection TextMesh markers under the generated root for:

```text
systems loop status
inventory summary
resource ledger summary
recipe craft preview/apply result
harvest preview/apply result
transaction affordability preview
encounter/combat preview
systems event log summary
```

These must be scene temporary objects only, not saved assets.

## Primary deliverable D — batchmode smoke

Add:

```text
LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageSystemsSmoke
```

It must log:

```text
GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_PASS
```

or:

```text
GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_FAIL
```

The smoke must require at least:

```text
genericSystemsPassed=True
samplePackageLoaded=True
genericProjectionBuilt=True
inventoryInitialized=True
resourcesInitialized=True
recipePreviewPresent=True
recipeApplyPassed=True
harvestPreviewPresent=True
harvestApplyPassed=True
transactionPreviewPresent=True
encounterPreviewPresent=True
combatRoundPreviewPresent=True
systemsEventLogPresent=True
zeroFatalErrors=True
```

## Application evidence seam

Add/update BCL-only evidence under:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/
```

It must verify:

- Goal124 remains green.
- Generic systems loop source markers exist.
- Unity batchmode log contains `GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_PASS`.
- Sample package is read-only.
- No forbidden paths are expected.
- Cleanup script remains available.

## Required artifacts

Create deterministic artifacts under:

```text
.llmgc/procedural/goal-125-generic-gamepackage-systems-loop-projection/
.llmgc/exports/goal-125-generic-gamepackage-systems-loop-projection/
```

Recommended files:

```text
generic-gamepackage-systems-loop-dashboard.json
generic-gamepackage-systems-loop-script-inventory.json
generic-gamepackage-systems-loop-smoke-plan.json
generic-gamepackage-systems-loop-log-scan.json
generic-gamepackage-systems-loop-report.md
generic-gamepackage-systems-loop-negative-proof.json
generic-gamepackage-systems-loop-file-index.json
unity-batchmode-generic-gamepackage-systems-loop.log
```

## Visual World Stream Preview Workspace

Add a read-only Goal125 section showing:

```text
genericSystemsStatus
samplePackagePath
packageId
recipePreviewPresent
recipeApplyPassed
harvestPreviewPresent
harvestApplyPassed
transactionPreviewPresent
encounterPreviewPresent
combatRoundPreviewPresent
inventorySummaryPresent
resourceSummaryPresent
systemsEventLogPresent
unitySmokeStatus
cleanupScriptAvailable
projectionOnly
evidencePath
exportPath
```

## Docs/current state

Update current-state/queue docs so they clearly say:

- Goal125 adds projection-local inventory/resource/crafting/harvest/transaction/encounter/combat systems loop over the generic sample GamePackage.
- It still does not authorize Runtime/schema/provider/Lua/generator-library/final-art/atlas/Unity scene/prefab/project-settings/StreamingAssets/release work.
- Manual verification remains one main button for the systems loop.
- After manual Unity checks, use `.devflow\scripts\clean-unity-editor-noise.cmd`.

## Artifact-scope policy

Add scenario:

```text
goal-125-generic-gamepackage-systems-loop-projection
```

It must allow only Goal125 expected files and exclude `.llmgc/manual/**`, samples/minimal-map-game, Unity scenes/prefabs/settings/Packages/StreamingAssets, Runtime/schema/provider/Lua/generator-library.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~Goal125|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-125-generic-gamepackage-systems-loop-projection
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Run Unity batchmode smoke:

```powershell
Unity.exe -batchmode -quit -projectPath .\unity\LLMGameCreatorAlpha -executeMethod LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageSystemsSmoke -logFile .\.llmgc\procedural\goal-125-generic-gamepackage-systems-loop-projection\unity-batchmode-generic-gamepackage-systems-loop.log
```

After Unity batchmode, run cleanup:

```powershell
.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
git status --short --untracked-files=all
```

Only stage allowed files. Final status must be clean.

## Quality gate

GREEN requires:

- batchmode log contains `GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_PASS`;
- systems smoke required fields are true;
- sample package remains unmodified;
- no forbidden path changes;
- no `.llmgc/manual/**` staged/tracked;
- tests/checks pass;
- artifact scope passes;
- final git status clean.

BLOCKED if Unity cannot run or systems smoke cannot be verified honestly.

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
GREEN Goal 125 generic gamepackage systems loop projection
BLOCKED Goal 125 generic gamepackage systems loop projection
FAILED Goal 125 generic gamepackage systems loop projection
```

Final report must include commit SHA, Unity systems smoke result, manual verification path, cleanup command, changed files grouped by area, final git status, and remaining debt.
