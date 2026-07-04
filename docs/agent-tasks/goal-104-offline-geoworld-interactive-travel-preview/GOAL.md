# Goal 104 — Offline Geoworld Interactive Travel Preview

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Objective

Build the next user-visible Unity Alpha milestone after Goal 103: an interactive offline geoworld travel preview with a lightweight player proxy, boundary-band detection, chunk prefetch state, object visibility diagnostics, Unity Editor create/clear tooling, WinForms inspection, deterministic evidence and product smoke.

This remains Alpha tooling only: no final Runtime, no final gameplay, no final art, no real geodata fetching.

## Preflight

Confirm `main`, fetch `origin/main`, verify HEAD includes `00dfa260 GREEN Goal 103 offline geoworld playmode travel preview`, verify Goal103 artifacts are present and `accepted=false`, verify Goal103 evidence has 4 steps / 18 objects / boundary prefetch / Unity scripts / editor helper. Record `AlphaRuntimeBootstrap.cs` hash+line count and do not modify it. Inspect dirty state; do not touch unrelated user work.

## Read first

Read `AGENTS.md`, `docs/GOAL_PRODUCTIVITY_POLICY.md`, `docs/VALIDATION_PIPELINE.md`, current state/queue/context/debt docs, Goal103 report/manifest/steps/chunk-visibility/object-state-index/evidence, Goal103 Unity scripts/editor helper, and existing Visual World Stream Preview Workspace files.

## Allowed files

- `src/LLMGameCreator.Application/Design/OfflineGeoworldInteractiveTravelPreview/`
- existing `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- existing `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldInteractiveTravelPreview/`
- relevant VisualWorldStreamPreviewWorkspace tests
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldInteractiveTravelPreviewProductSmokeTests.cs`
- relevant VisualWorldStreamPreviewWorkspace product smoke
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractiveTravelController.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPlayerMotor.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldBoundaryPrefetchState.cs`
- `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractiveTravelWindow.cs`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal104/`
- `.llmgc/procedural/goal-104-offline-geoworld-interactive-travel-preview/`
- docs quartet, debt register, artifact-scope policy and this task pack.

## Forbidden files

No LFZ code/archive, network/provider implementation, Runtime, public schema, Lua, generator-library, `.sln`, `.csproj`, lock files, `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/asmdefs/settings/packages/build settings, existing Goal101-103 payloads, binary/raster media, real geodata dumps, external dependencies, prompt dumps. No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Required implementation

### 1. Application payload

Create a BCL-only service reading real Goal103 artifacts and writing both `.llmgc/procedural/goal-104-offline-geoworld-interactive-travel-preview/` and `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal104/`.

Required payload files:
- `offline-geoworld-interactive-travel-manifest.json`
- `offline-geoworld-interactive-movement-path.json`
- `offline-geoworld-interactive-boundary-zones.json`
- `offline-geoworld-interactive-prefetch-plan.json`
- `offline-geoworld-interactive-readme.json`

Payload must include at least 6 movement samples, at least 2 boundary-zone crossings, active chunk set before/after each crossing, prefetch chunk set per crossing, object visibility diffs, deterministic state hash chain, and no raw geodata.

### 2. Unity scripts

Add:
- `OfflineGeoworldInteractiveTravelController.cs`
- `OfflineGeoworldPreviewPlayerMotor.cs`
- `OfflineGeoworldBoundaryPrefetchState.cs`

They must read Goal104 manifest from `Application.streamingAssetsPath`, support built-in keyboard/manual movement, compute current synthetic chunk/boundary state from metadata, update inspector diagnostics, activate/deactivate placeholder objects by metadata id/name when available, tolerate missing objects, and make no network/provider/LLM calls or AlphaRuntimeBootstrap dependency.

### 3. Unity Editor helper

Add `OfflineGeoworldInteractiveTravelWindow.cs` with menu `LLMGameCreator/Offline Geoworld Interactive Travel`. It must read payload readiness, create an interactive preview rig on demand, clear it on demand, and avoid automatic scene mutation on import.

### 4. Workspace integration

Add Visual World Stream Preview Workspace group `offline_geoworld_interactive_travel` showing movement sample count, boundary crossing count, active/prefetch chunk counts, visible object counts, Unity script/editor readiness, simulated movement proof, AlphaRuntimeBootstrap unchanged status, diagnostics and manual instructions.

### 5. Evidence

Create:
- `offline-geoworld-interactive-travel-report.md`
- `offline-geoworld-interactive-travel-manifest.json`
- `offline-geoworld-interactive-movement-path.json`
- `offline-geoworld-interactive-boundary-zones.json`
- `offline-geoworld-interactive-prefetch-plan.json`
- `offline-geoworld-interactive-unity-script-inventory.json`
- `offline-geoworld-interactive-editor-window-inventory.json`
- `offline-geoworld-interactive-simulated-execution-proof.json`
- `offline-geoworld-interactive-negative-proof.json`
- `offline-geoworld-interactive-workspace-binding-inventory.json`
- `offline-geoworld-interactive-source-lineage.json`
- `offline-geoworld-interactive-quality-gate-scan.json`

### 6. Negative proof

Reject missing Goal103 payload, movement path without boundary crossings, boundary crossing without prefetch plan, object visibility diff referencing unknown object, fake success without file reads, absolute paths, raw geodata leak, network/provider marker, AlphaRuntimeBootstrap dependency, scene/prefab/settings mutation marker, binary/raster media marker, and new input-system/external dependency marker.

## Tests

Focused tests must verify payload generation, movement samples >=6, boundary crossings >=2, prefetch changes, deterministic state hashes, Unity inventory safety, editor menu/create/clear markers, workspace group, negative proof, source-health limits. Product smoke must read evidence and Unity payload, verify no forbidden areas.

## Docs/state

Update docs quartet and debt register. Manual gate: `offline_geoworld_interactive_travel_preview_verification required`. Status: `accepted=false`.

## Validation

Run restore/build, focused `OfflineGeoworldInteractiveTravelPreview`, product smoke `OfflineGeoworldInteractiveTravelPreviewProductSmokeTests`, VisualWorldStreamPreviewWorkspace focused/product smoke, CurrentState, `check-current-goal.ps1` for scenario `goal-104-offline-geoworld-interactive-travel-preview`, `check-spine-fast.ps1`, `check-artifact-scope.ps1`, `git diff --check`, `git diff --cached --check`.

## Quality gate

GREEN only if no forbidden files changed, no LFZ/network/provider/Runtime/schema/project/dependency changes, no Unity scenes/settings changes, AlphaRuntimeBootstrap unchanged, interactive payload/scripts/editor helper exist, simulated movement proof and negative proof pass, workspace integration is real, no raw geodata/binary media, source-health limits pass, validation/artifact-scope pass, final worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
`GREEN Goal 104 offline geoworld interactive travel preview`
or BLOCKED/FAILED variant.
