# Goal 126 — Generic GamePackage Full Playthrough Projection

## Task ID

`goal-126-generic-gamepackage-full-playthrough-projection`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product goal.

This is not an evidence-only goal and not another Unity button audit. The primary deliverable is a projection-only, one-click full playthrough over `samples/minimal-map-game/package.json` that ties together the generic map projection, quest/dialogue/interaction loop, systems loop, inventory/resources, and combat preview into one deterministic vertical-slice transcript.

Manual Unity verification must remain optional for this goal. Batchmode Unity smoke is the required gate.

## Why this goal exists

Goals 123–125 moved the project away from geoworld-only assumptions and into a generic GamePackage projection over `samples/minimal-map-game/package.json`.

Goal126 must consolidate these separate generic package checks into one coherent full vertical slice projection:

```text
load package -> build map -> move/inspect sign -> dialogue summary -> quest objective check -> craft/harvest/transaction preview -> combat round preview -> final state/event transcript
```

This remains projection-only. It must not touch Runtime, public schema, providers, Lua, generator-library, scenes, prefabs, ProjectSettings, Packages, StreamingAssets, or the sample package.

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

.llmgc/procedural/goal-125-generic-gamepackage-systems-loop-projection/generic-gamepackage-systems-loop-dashboard.json
.llmgc/procedural/goal-124-generic-gamepackage-quest-dialogue-interaction-loop/generic-gamepackage-loop-dashboard.json
.llmgc/procedural/goal-123-generic-gamepackage-playable-projection-adapter/generic-gamepackage-projection-dashboard.json
.llmgc/procedural/goal-120a-clean-unity-editor-noise-empty-status-hotfix/clean-unity-editor-noise-empty-status-hotfix-dashboard.json

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionState.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionLoop.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionSystems.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-126-generic-gamepackage-full-playthrough-projection/**
.devflow/artifact-scope/artifact-scope-policy.json

.llmgc/procedural/goal-126-generic-gamepackage-full-playthrough-projection/**
.llmgc/exports/goal-126-generic-gamepackage-full-playthrough-projection/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/generic-gamepackage-full-playthrough-projection.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal126.cs

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionState.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionLoop.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionSystems.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionPlaythrough.cs

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
src/LLMGameCreator.GamePackage/**
provider / LLM / RAG / media provider code
public GamePackage schema files
Lua / Scripting code
generator-library/**
*.sln
*.csproj
Directory.Build.*
dependency/package files
```

No sample mutation. No Runtime/schema/provider/Lua/generator-library work. Do not save Unity scenes/prefabs. Do not write StreamingAssets.

## Primary deliverable A — full projection playthrough state

Add projection-only Unity state code, for example:

```text
GenericGamePackageProjectionPlaythrough
```

It must produce a deterministic full playthrough transcript from `samples/minimal-map-game/package.json`.

The playthrough must include at least:

1. Package identity: packageId `game/minimal-map-game`, title `Minimal Map Game`, start map `map/village`.
2. Map path preview from the map start position toward the sign entity or old guard, with walkability summary using tile definitions where available.
3. Sign interaction: select `entity/village/sign`, preview/apply `interaction/sign_inspect`, mark projection-only flag/log event.
4. Dialogue: select `entity/village/old_guard`, show `dialogue/old_guard_intro` start node speaker/text summary.
5. Quest objective: show `quest/help_healer`, required `item/red_herb` amount 3, `inventory/player_start` has 2, objective status incomplete.
6. Inventory/resources: show player inventory summary and default resources such as health/stamina/mana/gold where resolvable.
7. Systems: craft preview/apply for `recipe/healing_potion`, harvest preview/apply for `node/apple_tree`, transaction affordability preview for `transaction/buy_healing_potion`, combat round preview for `encounter/goblin_duel`.
8. Final projection state: event transcript, final inventory/resource deltas, combat preview result, quest status.
9. Zero fatal errors.

Everything must be in-memory Editor projection state.

## Primary deliverable B — one-click full playthrough verification

Extend the Unity window with a single prominent button:

```text
Run Generic Package Full Playthrough Verification
```

This button must run the full playthrough sequence and update a compact window section, not require the user to click every debug button.

Keep prior Goal121–125 debug buttons, but they must remain secondary/optional.

## Primary deliverable C — visible projection section

Create/update visible projection markers/text under the generated root for:

```text
full playthrough status
movement/path summary
sign interaction result
dialogue summary
quest objective status
inventory/resource final summary
craft/harvest/transaction/combat summary
event transcript summary
```

These may be TextMesh objects and primitive markers. They must be temporary scene objects only.

## Primary deliverable D — batchmode smoke

Add:

```text
LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageFullPlaythroughSmoke
```

It must log:

```text
GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS
```

or:

```text
GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL
```

The smoke must require at least:

```text
fullPlaythroughPassed=True
samplePackageLoaded=True
mapPathPreviewPresent=True
signInteractionApplied=True
dialogueSummaryPresent=True
questObjectiveStatusPresent=True
inventorySummaryPresent=True
resourceSummaryPresent=True
recipeApplyPassed=True
harvestApplyPassed=True
transactionPreviewPresent=True
combatRoundPreviewPresent=True
eventTranscriptPresent=True
zeroFatalErrors=True
```

## Application evidence seam

Add/update BCL-only evidence under:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/
```

It must verify Goal125 remains green, full playthrough source markers exist, Unity batchmode log contains `GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS`, sample package is read-only, no forbidden path is expected, and cleanup script remains available.

## Required artifacts

Create deterministic artifacts under:

```text
.llmgc/procedural/goal-126-generic-gamepackage-full-playthrough-projection/
.llmgc/exports/goal-126-generic-gamepackage-full-playthrough-projection/
```

Recommended files:

```text
generic-gamepackage-full-playthrough-dashboard.json
generic-gamepackage-full-playthrough-script-inventory.json
generic-gamepackage-full-playthrough-smoke-plan.json
generic-gamepackage-full-playthrough-log-scan.json
generic-gamepackage-full-playthrough-report.md
generic-gamepackage-full-playthrough-negative-proof.json
generic-gamepackage-full-playthrough-file-index.json
unity-batchmode-generic-gamepackage-full-playthrough.log
```

## Visual World Stream Preview Workspace

Add a read-only Goal126 section showing:

```text
fullPlaythroughStatus
samplePackagePath
packageId
mapId
mapPathPreviewPresent
signInteractionApplied
dialogueSummaryPresent
questObjectiveStatusPresent
inventorySummaryPresent
resourceSummaryPresent
systemsSummaryPresent
combatRoundPreviewPresent
eventTranscriptPresent
unitySmokeStatus
cleanupScriptAvailable
projectionOnly
evidencePath
exportPath
```

## Docs/current state

Update current-state and queue docs so they clearly say:

- Goal126 adds projection-only full playthrough over the generic sample GamePackage.
- It still does not authorize Runtime/schema/provider/Lua/generator-library/final-art/atlas/Unity scene/prefab/project-settings/StreamingAssets/release work.
- Manual checking is optional and should use the one main full playthrough button.
- After manual Unity checks, use `.devflow\scripts\clean-unity-editor-noise.cmd`.

## Artifact-scope policy

Add scenario:

```text
goal-126-generic-gamepackage-full-playthrough-projection
```

It must allow only Goal126 expected files and exclude `.llmgc/manual/**`, `samples/minimal-map-game/**`, Unity scenes/prefabs/settings/Packages/StreamingAssets, Runtime/schema/provider/Lua/generator-library.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~Goal126|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-126-generic-gamepackage-full-playthrough-projection
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Run Unity batchmode smoke:

```powershell
Unity.exe -batchmode -quit -projectPath .\unity\LLMGameCreatorAlpha -executeMethod LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageFullPlaythroughSmoke -logFile .\.llmgc\procedural\goal-126-generic-gamepackage-full-playthrough-projection\unity-batchmode-generic-gamepackage-full-playthrough.log
```

If `Unity.exe` is not on PATH, use installed Unity 6000.1.10f1 if available.

After Unity batchmode, run cleanup:

```powershell
.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
git status --short --untracked-files=all
```

Only stage allowed files. Final status must be clean.

## Quality gate

GREEN requires:

- batchmode log contains `GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS`;
- full playthrough smoke required fields are true;
- sample package remains unmodified;
- no forbidden path changes;
- no `.llmgc/manual/**` staged/tracked;
- tests/checks pass;
- artifact scope passes;
- final git status clean.

BLOCKED if Unity cannot run or full playthrough smoke cannot be verified honestly.
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
GREEN Goal 126 generic gamepackage full playthrough projection
BLOCKED Goal 126 generic gamepackage full playthrough projection
FAILED Goal 126 generic gamepackage full playthrough projection
```

Final report must include commit SHA, Unity full playthrough smoke result, manual verification path, cleanup command, changed files grouped by area, final git status, and remaining debt.
