# Goal 105 — Offline Geoworld Interaction Layer, State Deltas & Playable Probe

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Objective

Deliver the next large user-visible Unity Alpha milestone after Goal 104.

Goal 104 gave an interactive travel preview with player proxy, movement samples, boundary prefetch and object visibility diagnostics. Goal 105 must add the first playable interaction loop over the synthetic offline geoworld preview:

1. Application-side interaction graph and action/state-delta payload from real Goal104 artifacts.
2. Unity scripts for proximity/selection/interaction prompts, interaction target binding and state-delta log.
3. Unity Editor helper to create/clear the interaction probe rig on demand.
4. WinForms/Visual World Stream Preview Workspace inspection.
5. Simulated interaction session proof with deterministic state hashes.
6. Unity Alpha script inventory/static safety scan across Goal101-105 geoworld scripts.
7. Negative proof, focused tests, product smoke, docs/state/debt sync.

This is Alpha tooling only: not final Runtime, not final gameplay, not final art, not real geodata fetching.

## Preflight

Confirm `main`, fetch `origin/main`, verify HEAD includes `9fc65ee GREEN Goal 104 offline geoworld interactive travel preview`, verify Goal104 artifacts exist and `accepted=false`, verify Goal104 evidence proves movement samples >=6, boundary crossings >=2, prefetch plan, object visibility diffs, Unity scripts/editor helper, simulated proof. Record `AlphaRuntimeBootstrap.cs` hash/line count and do not modify it. Inspect dirty state and do not touch unrelated user work.

## Allowed files

- `src/LLMGameCreator.Application/Design/OfflineGeoworldInteractionPlayableProbe/`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldInteractionPlayableProbe/`
- relevant VisualWorldStreamPreviewWorkspace tests
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldInteractionPlayableProbeProductSmokeTests.cs`
- relevant VisualWorldStreamPreviewWorkspace product smoke
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionController.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionTarget.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldStateDeltaLog.cs`
- `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractionProbeWindow.cs`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105/`
- `.llmgc/procedural/goal-105-offline-geoworld-interaction-playable-probe/`
- docs quartet, debt register, artifact-scope policy and this task pack.

## Forbidden files

No LFZ archive/source, network/provider implementation, Runtime, public schema, Lua, generator-library, `.sln`, `.csproj`, lock files, `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/asmdefs/project settings/packages/build settings, existing Goal101-104 payloads, binary/raster media, real geodata dumps, external dependencies, prompt dumps. No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Required implementation

### 1. Interaction payload

Create BCL-only service reading real Goal104 artifacts and writing both `.llmgc/procedural/goal-105-offline-geoworld-interaction-playable-probe/` and `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105/`.

Required payload files:
- `offline-geoworld-interaction-manifest.json`
- `offline-geoworld-interaction-targets.json`
- `offline-geoworld-interaction-actions.json`
- `offline-geoworld-interaction-session-script.json`
- `offline-geoworld-interaction-state-delta-plan.json`
- `offline-geoworld-interaction-readme.json`

Payload must include at least 8 interaction targets drawn from Goal104 visible objects; at least 5 action kinds: `inspect`, `enter_or_focus`, `mark_visited`, `toggle_blocked`, `collect_sample`; at least 6 scripted interaction events; state deltas separate from immutable base data; deterministic state hash chain; action availability by distance/radius; no raw geodata.

### 2. Unity scripts

Add `OfflineGeoworldInteractionController.cs`, `OfflineGeoworldInteractionTarget.cs`, `OfflineGeoworldStateDeltaLog.cs`.

Expected behavior: read Goal105 manifest from `Application.streamingAssetsPath`; bind metadata targets to existing preview objects by id/name when available; support nearest target selection/proximity diagnostics; expose available action/status fields in Inspector; execute scripted/manual interaction actions; append state deltas in memory only; tolerate missing preview objects with diagnostics; no network/provider/LLM calls; no AlphaRuntimeBootstrap dependency; no scene/prefab/settings/package changes.

### 3. Unity Editor helper

Add `OfflineGeoworldInteractionProbeWindow.cs` with menu `LLMGameCreator/Offline Geoworld Interaction Probe`. It must read payload readiness, create an interaction controller rig on demand, clear the rig on demand, and avoid automatic scene mutation on import.

### 4. Simulated interaction proof

.NET proof must simulate manifest/targets/actions/session/state-delta plan reads, target binding by id/name, distance/radius availability, scripted interactions, state delta append, deterministic state hash chain, unavailable action rejection, and no absolute paths/raw geodata/binary media/network markers.

### 5. Workspace integration

Add Visual World Stream Preview Workspace group `offline_geoworld_interactions` showing target count, action kind count, scripted event count, state delta count, deterministic hash chain status, Unity script/editor readiness, Unity Alpha script inventory/static safety, simulated interaction proof, AlphaRuntimeBootstrap unchanged, diagnostics/manual instructions.

### 6. Evidence

Create:
- `offline-geoworld-interaction-report.md`
- `offline-geoworld-interaction-manifest.json`
- `offline-geoworld-interaction-targets.json`
- `offline-geoworld-interaction-actions.json`
- `offline-geoworld-interaction-session-script.json`
- `offline-geoworld-interaction-state-delta-plan.json`
- `offline-geoworld-interaction-unity-script-inventory.json`
- `offline-geoworld-interaction-editor-window-inventory.json`
- `offline-geoworld-interaction-simulated-session-proof.json`
- `offline-geoworld-interaction-negative-proof.json`
- `offline-geoworld-interaction-workspace-binding-inventory.json`
- `offline-geoworld-interaction-source-lineage.json`
- `offline-geoworld-interaction-quality-gate-scan.json`

### 7. Unity Alpha script inventory/static safety scan

Scan Goal101-105 geoworld Unity scripts and editor helpers for source exists/not minified, no network/provider/LLM markers, no AlphaRuntimeBootstrap dependency, no external package/new input-system markers, no scene/prefab/settings mutation markers except manual create/clear in Editor windows, and source-health limits.

### 8. Negative proof

Reject missing Goal104 payload, interaction target referencing unknown object, action missing target, unavailable action accepted outside radius, state delta mutates base data directly, fake success without file reads, absolute path, raw geodata leak, network/provider marker, AlphaRuntimeBootstrap dependency marker, scene/prefab/settings mutation marker, binary/raster media marker, external dependency/new input-system marker.

## Tests

Focused tests must verify payload generation, target/action/session counts, deterministic state hashes, scripted interaction proof, unavailable action rejection, Unity inventory safety, editor menu/create/clear markers, workspace group, negative proof, source-health limits. Product smoke must read evidence and Unity payload, verify no forbidden areas.

## Docs/state

Update docs quartet and debt register. Manual gate: `offline_geoworld_interaction_playable_probe_verification required`. Status: `accepted=false`.

## Validation

Run restore/build, focused `OfflineGeoworldInteractionPlayableProbe`, product smoke `OfflineGeoworldInteractionPlayableProbeProductSmokeTests`, VisualWorldStreamPreviewWorkspace focused/product smoke, CurrentState, `check-current-goal.ps1` for scenario `goal-105-offline-geoworld-interaction-playable-probe`, `check-spine-fast.ps1`, `check-artifact-scope.ps1`, `git diff --check`, `git diff --cached --check`.

## Quality gate

GREEN only if no forbidden files changed, no LFZ/network/provider/Runtime/schema/project/dependency changes, no Unity scenes/settings changes, AlphaRuntimeBootstrap unchanged, interaction payload/scripts/editor helper exist, simulated interaction proof and negative proof pass, workspace integration is real, Unity Alpha script inventory passes, no raw geodata/binary media, source-health limits pass, validation/artifact-scope pass, final worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
`GREEN Goal 105 offline geoworld interaction playable probe`
or BLOCKED/FAILED variant.
