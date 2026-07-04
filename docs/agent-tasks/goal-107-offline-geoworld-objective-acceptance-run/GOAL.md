# Goal 107 — Offline Geoworld Objective Loop, Acceptance Run & Quality Consolidation

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Objective

Deliver the next large user-visible Unity Alpha milestone after Goal 106: turn the offline geoworld alpha slice into a small objective-driven playable acceptance run and consolidate the accumulated Goal101-106 Unity Alpha proof chain.

Goal 106 added save/load/replay over interaction deltas. Goal 107 must add objective/quest-like scenario payload, Unity objective tracker, Editor helper, acceptance proof, Visual Workspace inspection, and Unity Alpha quality consolidation.

This is Alpha tooling only: no final Runtime, no public GamePackage schema changes, no real geodata, no network/provider calls, no final UI/art/release build.

## Preflight

Confirm `main`, fetch `origin/main`, verify HEAD includes `08930f45 GREEN Goal 106 offline geoworld session persistence replay`, verify Goal106 artifacts exist and `accepted=false`, verify Goal106 evidence proves replayStepCount=6, stateDeltaCount=6, checkpointStepIndex=3, save/load/replay proof, corrupted snapshot rejection, Unity scripts/editor helper. Record `AlphaRuntimeBootstrap.cs` hash/line count and do not modify it. Inspect dirty state and do not touch unrelated user work.

## Read first

Read AGENTS.md, GOAL_PRODUCTIVITY_POLICY.md, MILESTONE_GATES.md, RELEASE_RISK_REGISTER.md, VALIDATION_PIPELINE.md, current state/queue/context/debt docs, Goal106 report/manifest/initial-state/delta-log/replay-script/acceptance-checklist/simulated proof/negative proof, Goal101-106 Unity script inventories, Goal106 Unity scripts/editor helper, and Visual World Stream Preview Workspace files.

## Allowed files

- `src/LLMGameCreator.Application/Design/OfflineGeoworldObjectiveAcceptanceRun/`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldObjectiveAcceptanceRun/`
- relevant VisualWorldStreamPreviewWorkspace tests
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldObjectiveAcceptanceRunProductSmokeTests.cs`
- relevant VisualWorldStreamPreviewWorkspace product smoke
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldObjectiveTracker.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldObjectiveAcceptanceController.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldObjectiveState.cs`
- `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldObjectiveAcceptanceWindow.cs`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal107/`
- `.llmgc/procedural/goal-107-offline-geoworld-objective-acceptance-run/`
- docs quartet, release risk register/milestone gates if needed, debt register, artifact-scope policy and this task pack.

## Forbidden files

No LFZ archive/source, network/provider implementation, Runtime, Runtime.Abstractions, public GamePackage schema, Lua, generator-library, `.sln`, `.csproj`, lock files, `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/asmdefs/project settings/packages/build settings, existing Goal101-106 payloads, binary/raster media, real geodata dumps, external dependencies, prompt dumps. No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Required implementation

### 1. Objective acceptance payload

Create a BCL-only service reading real Goal106 artifacts and writing both `.llmgc/procedural/goal-107-offline-geoworld-objective-acceptance-run/` and `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal107/`.

Required payload files:
- `offline-geoworld-objective-manifest.json`
- `offline-geoworld-objectives.json`
- `offline-geoworld-objective-acceptance-run.json`
- `offline-geoworld-objective-completion-state.json`
- `offline-geoworld-objective-replay-acceptance-proof.json`
- `offline-geoworld-objective-readme.json`

Payload must include at least 5 objectives derived from Goal105/106 interactions:
- inspect a POI/building target;
- mark one target visited;
- collect a sample;
- toggle/clear a blocked route or barrier;
- save/load checkpoint and resume;
- complete/finalize the acceptance run.

Each objective must include prerequisites, linked action ids, expected state deltas, visible diagnostics, completion state and deterministic hash contribution.

### 2. Unity scripts

Add `OfflineGeoworldObjectiveTracker.cs`, `OfflineGeoworldObjectiveAcceptanceController.cs`, `OfflineGeoworldObjectiveState.cs`.

Expected behavior: read Goal107 manifest from `Application.streamingAssetsPath`; bind to existing Goal105 interaction controller and Goal106 replay/save-load controller if present; track objective prerequisites/completion; expose current objective, completed count, final status and diagnostics in Inspector; allow manual advance/check/replay from metadata; tolerate missing controllers/objects with diagnostics; no network/provider/LLM calls; no AlphaRuntimeBootstrap dependency; no scene/prefab/settings/package changes.

### 3. Unity Editor helper

Add `OfflineGeoworldObjectiveAcceptanceWindow.cs` with menu `LLMGameCreator/Offline Geoworld Objective Acceptance`. It must read payload readiness, create/clear objective acceptance rig on demand, show acceptance instructions and avoid automatic scene mutation on import.

### 4. Acceptance/replay proof

.NET proof must simulate manifest/objectives/acceptance-run/completion-state reads, applying replay/save-load state from Goal106, objective prerequisites, completion transitions, state delta linkage, final accepted/completed state, deterministic hash chain, failed prerequisite rejection, and no absolute paths/raw geodata/binary media/network markers.

### 5. Unity Alpha quality consolidation

Scan Goal101-107 geoworld Unity scripts and editor helpers: source exists/readable/not minified, no network/provider/LLM markers, no AlphaRuntimeBootstrap dependency, no external package/new input-system markers, no Unity scenes/prefabs/settings/build changes, no binary/raster media, line counts under limits.

Create a geoworld alpha slice acceptance summary reporting travel preview, interactive travel, interactions, session save/load/replay, objectives/acceptance run, manual checklist and remaining not-final warnings.

### 6. Workspace integration

Add Visual World Stream Preview Workspace group `offline_geoworld_objective_acceptance` showing objective count, completed objective count in simulated proof, acceptance final status, replay/save-load linkage, Unity script/editor readiness, Unity Alpha quality consolidation status, manual acceptance checklist summary, AlphaRuntimeBootstrap unchanged status, diagnostics/manual instructions.

### 7. Evidence

Create:
- `offline-geoworld-objective-report.md`
- `offline-geoworld-objective-manifest.json`
- `offline-geoworld-objectives.json`
- `offline-geoworld-objective-acceptance-run.json`
- `offline-geoworld-objective-completion-state.json`
- `offline-geoworld-objective-unity-script-inventory.json`
- `offline-geoworld-objective-editor-window-inventory.json`
- `offline-geoworld-objective-simulated-acceptance-proof.json`
- `offline-geoworld-objective-negative-proof.json`
- `offline-geoworld-objective-workspace-binding-inventory.json`
- `offline-geoworld-objective-source-lineage.json`
- `offline-geoworld-objective-alpha-quality-consolidation.json`
- `offline-geoworld-objective-quality-gate-scan.json`

### 8. Negative proof

Reject missing Goal106 payload, objective referencing unknown action/target/delta, prerequisite bypass, completion without required state delta, save/load objective without checkpoint, replay mismatch, fake success without file reads, absolute path, raw geodata leak, network/provider marker, AlphaRuntimeBootstrap dependency marker, scene/prefab/settings mutation marker, binary/raster media marker, external dependency/new input-system marker.

## Tests

Focused tests must verify objective payload generation, objective count >=5, prerequisite/completion proof, replay/save-load linkage, deterministic hashes, failed prerequisite rejection, Unity inventory safety, editor menu/create/clear markers, workspace group, quality consolidation, negative proof, source-health limits. Product smoke must read evidence and Unity payload, verify no forbidden areas.

## Docs/state

Update docs quartet, milestone gates/release risk if needed, and debt register. Manual gate: `offline_geoworld_objective_acceptance_run_verification required`. Status: `accepted=false`.

Record that this is the first objective-driven Unity Alpha geoworld acceptance run, not final Runtime/gameplay/release build.

## Validation

Run restore/build, focused `OfflineGeoworldObjectiveAcceptanceRun`, product smoke `OfflineGeoworldObjectiveAcceptanceRunProductSmokeTests`, VisualWorldStreamPreviewWorkspace focused/product smoke, CurrentState, `check-current-goal.ps1` for scenario `goal-107-offline-geoworld-objective-acceptance-run`, `check-spine-fast.ps1`, `check-artifact-scope.ps1`, `git diff --check`, `git diff --cached --check`.

## Quality gate

GREEN only if no forbidden files changed, no LFZ/network/provider/Runtime/schema/project/dependency changes, no Unity scenes/settings changes, AlphaRuntimeBootstrap unchanged, objective payload/scripts/editor helper exist, acceptance proof and negative proof pass, workspace integration is real, Unity Alpha quality consolidation passes, no raw geodata/binary media, source-health limits pass, validation/artifact-scope pass, final worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
`GREEN Goal 107 offline geoworld objective acceptance run`
or BLOCKED/FAILED variant.
