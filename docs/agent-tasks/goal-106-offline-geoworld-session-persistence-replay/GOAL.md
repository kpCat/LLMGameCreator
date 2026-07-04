# Goal 106 — Offline Geoworld Session Save/Load, Replay & Acceptance Harness

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Objective

Deliver the next large user-visible Unity Alpha milestone after Goal 105.

Goal 105 added a playable interaction probe with targets/actions/state deltas in memory. Goal 106 must make the synthetic offline geoworld alpha loop persistent and replayable:

1. Application-side session save/load/replay payload from real Goal105 artifacts.
2. Unity scripts for in-memory + persistent session snapshot, load/resume, and replay stepping.
3. Unity Editor helper to create/clear the save-load/replay rig and show manual acceptance steps.
4. WinForms/Visual World Stream Preview Workspace inspection.
5. Deterministic .NET save/load/replay proof.
6. Manual acceptance checklist artifact for the current Unity Alpha geoworld slice.
7. Negative proof, focused tests, product smoke, docs/state/debt sync.

This is Alpha tooling only: no final Runtime, no public GamePackage schema change, no final save system, no real geodata, no provider/network calls.

## Preflight

Confirm `main`, fetch `origin/main`, verify HEAD includes `e92f55d2 GREEN Goal 105 offline geoworld interaction playable probe`, verify Goal105 artifacts exist and `accepted=false`, verify Goal105 evidence proves 8 targets, 5 action kinds, 6 scripted events, state deltas separate from base data, deterministic state hash chain, Unity scripts/editor helper, simulated interaction proof. Record `AlphaRuntimeBootstrap.cs` hash/line count and do not modify it. Inspect dirty state and do not touch unrelated user work.

## Read first

Read `AGENTS.md`, `docs/GOAL_PRODUCTIVITY_POLICY.md`, `docs/VALIDATION_PIPELINE.md`, current state/queue/context/debt docs, Goal105 report/manifest/targets/actions/session/state-delta/simulated proof, Goal105 Unity scripts/editor helper, Goal101-105 Unity script inventory where available, and Visual World Stream Preview Workspace files.

## Allowed files

- `src/LLMGameCreator.Application/Design/OfflineGeoworldSessionPersistenceReplay/`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldSessionPersistenceReplay/`
- relevant VisualWorldStreamPreviewWorkspace tests
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldSessionPersistenceReplayProductSmokeTests.cs`
- relevant VisualWorldStreamPreviewWorkspace product smoke
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionSaveLoadController.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionReplayController.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionSnapshot.cs`
- `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldSessionReplayWindow.cs`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal106/`
- `.llmgc/procedural/goal-106-offline-geoworld-session-persistence-replay/`
- docs quartet, debt register, artifact-scope policy and this task pack.

## Forbidden files

No LFZ archive/source, network/provider implementation, Runtime, public schema, Lua, generator-library, `.sln`, `.csproj`, lock files, `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/asmdefs/project settings/packages/build settings, existing Goal101-105 payloads, binary/raster media, real geodata dumps, external dependencies, prompt dumps. No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Required implementation

### 1. Session persistence/replay payload

Create BCL-only service reading real Goal105 artifacts and writing both `.llmgc/procedural/goal-106-offline-geoworld-session-persistence-replay/` and `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal106/`.

Required payload files:
- `offline-geoworld-session-manifest.json`
- `offline-geoworld-session-initial-state.json`
- `offline-geoworld-session-delta-log.json`
- `offline-geoworld-session-replay-script.json`
- `offline-geoworld-session-acceptance-checklist.json`
- `offline-geoworld-session-readme.json`

Payload must include all Goal105 targets/actions/session event lineage, at least 6 deltas, initial state hash, final state hash, replay step hashes, load/resume checkpoint after at least 3 events, manual acceptance checklist steps for Unity Alpha, and no raw geodata.

### 2. Unity scripts

Add `OfflineGeoworldSessionSaveLoadController.cs`, `OfflineGeoworldSessionReplayController.cs`, `OfflineGeoworldSessionSnapshot.cs`.

Expected behavior: read Goal106 manifest from `Application.streamingAssetsPath`; optionally use `Application.persistentDataPath` for an Alpha-only session snapshot file; create/load/delete snapshot with current delta log; replay scripted events from metadata; expose snapshot/replay/hash/status fields in Inspector; integrate by finding existing Goal105 interaction controller/state-delta log when available; tolerate missing objects with diagnostics; no network/provider/LLM calls; no AlphaRuntimeBootstrap dependency; no scene/prefab/settings/package changes.

### 3. Unity Editor helper

Add `OfflineGeoworldSessionReplayWindow.cs` with menu `LLMGameCreator/Offline Geoworld Session Replay`. It must read payload readiness, create/clear a save-load/replay rig on demand, display manual acceptance checklist text, and avoid automatic scene mutation on import.

### 4. Simulated save/load/replay proof

.NET proof must simulate manifest/initial-state/delta-log/replay/checklist reads, applying first half deltas, save checkpoint, load checkpoint, continue replay, final state hash match, idempotent replay rejection or deterministic duplicate handling, corrupted snapshot rejection, and no absolute paths/raw geodata/binary media/network markers.

### 5. Workspace integration

Add Visual World Stream Preview Workspace group `offline_geoworld_session_replay` showing delta count, replay step count, checkpoint step, final hash, Unity script/editor readiness, simulated save/load/replay proof, manual acceptance checklist summary, AlphaRuntimeBootstrap unchanged, diagnostics/manual instructions.

### 6. Evidence

Create:
- `offline-geoworld-session-report.md`
- `offline-geoworld-session-manifest.json`
- `offline-geoworld-session-initial-state.json`
- `offline-geoworld-session-delta-log.json`
- `offline-geoworld-session-replay-script.json`
- `offline-geoworld-session-acceptance-checklist.json`
- `offline-geoworld-session-unity-script-inventory.json`
- `offline-geoworld-session-editor-window-inventory.json`
- `offline-geoworld-session-simulated-save-load-replay-proof.json`
- `offline-geoworld-session-negative-proof.json`
- `offline-geoworld-session-workspace-binding-inventory.json`
- `offline-geoworld-session-source-lineage.json`
- `offline-geoworld-session-quality-gate-scan.json`

### 7. Unity Alpha script inventory/static safety scan

Scan Goal101-106 geoworld Unity scripts and editor helpers for source exists/readable, no network/provider/LLM markers, no AlphaRuntimeBootstrap dependency, no external package/new input-system markers, no scene/prefab/settings mutation markers except manual create/clear in Editor windows, and source-health limits.

### 8. Negative proof

Reject missing Goal105 payload, missing delta log, checkpoint without prior deltas, load snapshot with hash mismatch, corrupted snapshot accepted, replay final hash mismatch, duplicate replay mutates state non-deterministically, absolute path in payload, raw geodata leak, network/provider marker, AlphaRuntimeBootstrap dependency marker, scene/prefab/settings mutation marker, binary/raster media marker, external dependency/new input-system marker.

## Tests

Focused tests must verify payload generation, delta/replay/checkpoint counts, deterministic save-load-replay hashes, corrupted snapshot rejection, Unity inventory safety, editor menu/create/clear markers, workspace group, negative proof, source-health limits. Product smoke must read evidence and Unity payload, verify no forbidden areas.

## Docs/state

Update docs quartet and debt register. Manual gate: `offline_geoworld_session_persistence_replay_verification required`. Status: `accepted=false`.

## Validation

Run restore/build, focused `OfflineGeoworldSessionPersistenceReplay`, product smoke `OfflineGeoworldSessionPersistenceReplayProductSmokeTests`, VisualWorldStreamPreviewWorkspace focused/product smoke, CurrentState, `check-current-goal.ps1` for scenario `goal-106-offline-geoworld-session-persistence-replay`, `check-spine-fast.ps1`, `check-artifact-scope.ps1`, `git diff --check`, `git diff --cached --check`.

## Quality gate

GREEN only if no forbidden files changed, no LFZ/network/provider/Runtime/schema/project/dependency changes, no Unity scenes/settings changes, AlphaRuntimeBootstrap unchanged, session payload/scripts/editor helper exist, simulated save/load/replay proof and negative proof pass, workspace integration is real, Unity Alpha script inventory passes, no raw geodata/binary media, source-health limits pass, validation/artifact-scope pass, final worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
`GREEN Goal 106 offline geoworld session persistence replay`
or BLOCKED/FAILED variant.
