# Goal 108 — Offline Geoworld Alpha Slice Orchestrator, One-Click Setup & Acceptance Pack

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Objective

Goal101-107 now provide separate Unity Alpha pieces: preview commands, editor preview, play-mode travel, interactive travel, interactions, save/load/replay, and objective acceptance. Goal108 must make this usable as one offline geoworld Alpha Slice.

Deliver in one composite goal:
1. Aggregate real Goal101-107 artifacts into one Alpha Slice manifest.
2. Add Unity Editor one-click setup/clear/verify window for the whole slice.
3. Add a small Unity coordinator script that finds/creates existing Goal101-107 controllers and reports end-to-end status.
4. Add an acceptance pack/runbook for manual Unity testing.
5. Add WinForms/Visual World Stream Preview Workspace inspection.
6. Produce deterministic full-slice proof and negative proof.
7. Consolidate evidence hygiene: do not rewrite historical Goal101-107 artifacts except by explicit aggregate references.

This is Alpha tooling only: no final Runtime, no public schema changes, no real geodata, no network/provider calls, no final art, no Unity scene/prefab/project settings changes.

## Preflight

Confirm `main`, fetch `origin/main`, verify HEAD includes `14ad9f38 GREEN Goal 107 offline geoworld objective acceptance run`. Verify Goal101-107 artifacts exist and are `accepted=false`, and Goal107 proves objectiveCount >=5, completedObjectiveCount == objectiveCount, finalStatus completed, alpha quality consolidation passed. Record `AlphaRuntimeBootstrap.cs` hash/line count and do not modify it. Inspect dirty state and do not touch unrelated user work.

## Read first

Read AGENTS.md, GOAL_PRODUCTIVITY_POLICY.md, MILESTONE_GATES.md, RELEASE_RISK_REGISTER.md, VALIDATION_PIPELINE.md, current state/queue/context/debt docs, Goal101-107 reports/quality gates/script inventories, Goal107 objective acceptance proof, current Unity geoworld scripts/editor helpers, and Visual World Stream Preview Workspace files.

## Allowed files

- `src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceOrchestrator/`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaSliceOrchestrator/`
- relevant VisualWorldStreamPreviewWorkspace tests/product smoke
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaSliceOrchestratorProductSmokeTests.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaSliceCoordinator.cs`
- `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaSliceWindow.cs`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal108/`
- `.llmgc/procedural/goal-108-offline-geoworld-alpha-slice-orchestrator/`
- docs quartet, release risk/milestone docs if needed, debt register, artifact-scope policy and this task pack.

## Forbidden files

No LFZ archive/source, network/provider implementation, Runtime, Runtime.Abstractions, public GamePackage schema, Lua, generator-library, `.sln`, `.csproj`, lock files, `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/asmdefs/project settings/packages/build settings, existing Goal101-107 payloads/evidence except read-only references, binary/raster media, real geodata dumps, external dependencies, prompt dumps. No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Required implementation

### 1. Alpha slice aggregate payload

Create BCL-only service reading real Goal101-107 artifacts and writing both `.llmgc/procedural/goal-108-offline-geoworld-alpha-slice-orchestrator/` and `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal108/`.

Required payload files:
- `offline-geoworld-alpha-slice-manifest.json`
- `offline-geoworld-alpha-slice-components.json`
- `offline-geoworld-alpha-slice-acceptance-runbook.json`
- `offline-geoworld-alpha-slice-readiness-matrix.json`
- `offline-geoworld-alpha-slice-readme.json`

Aggregate components: preview, editor preview, play-mode travel, interactive travel, interactions, session replay, objective acceptance. Include source hashes, manual gates, accepted=false status, Unity payload paths, script paths and not-final warnings.

### 2. Unity one-click editor window

Add `OfflineGeoworldAlphaSliceWindow.cs` with menu `LLMGameCreator/Offline Geoworld Alpha Slice`.

It must read Goal108 manifest, show component readiness/missing diagnostics, create a single root GameObject on demand, attach/create existing Goal101-107 controllers/helpers where appropriate, verify/run acceptance checklist, clear the root rig on demand, and avoid external packages/network/provider calls. No automatic scene mutation on import.

### 3. Unity coordinator

Add `OfflineGeoworldAlphaSliceCoordinator.cs`.

It must read Goal108 manifest from `Application.streamingAssetsPath`, find optional existing controllers from Goals101-107, expose Inspector statuses: preview ready, travel ready, interactions ready, session replay ready, objectives ready, final acceptance ready; provide manual `RefreshStatus`/`VerifySlice`; no AlphaRuntimeBootstrap dependency.

### 4. Full-slice simulated proof

.NET proof must read Goal101-107 payloads plus Goal108 aggregate, verify component readiness, simulate sequence: setup preview -> travel -> interact -> save -> load -> replay -> complete objectives, verify final hash/result propagation, no absolute paths/raw geodata/binary media/network markers, and no historical artifact rewrites.

### 5. Workspace integration

Add Visual World Stream Preview Workspace group `offline_geoworld_alpha_slice` showing component readiness, manual gate list, one-click Unity tool readiness, acceptance runbook summary, final simulated proof status, AlphaRuntimeBootstrap unchanged, remaining not-final warnings.

### 6. Evidence

Create: report, manifest, components, acceptance runbook, readiness matrix, Unity script inventory, editor window inventory, simulated proof, negative proof, workspace binding inventory, quality gate scan.

### 7. Negative proof

Reject missing Goal107 payload, missing component, accepted=true fake promotion, historical artifact rewrite attempt, component hash mismatch, one-click setup without file reads, missing clear method, objective final status not completed, absolute path, raw geodata leak, network/provider marker, AlphaRuntimeBootstrap dependency, scene/prefab/settings mutation marker, binary/raster media marker, external dependency/new input-system marker.

## Tests

Focused tests must verify aggregate payload, all seven components represented, full-slice simulated proof, missing component rejection, historical artifact immutability check, Unity inventory safety, editor menu/create/clear/verify markers, workspace group, negative proof, source-health limits. Product smoke must read evidence and Unity payload and verify no forbidden areas.

## Docs/state

Update docs quartet, milestone gates/release risk if needed, and debt register. Manual gate: `offline_geoworld_alpha_slice_orchestrator_verification required`. Status: `accepted=false`. Record this as first one-click offline geoworld Alpha Slice orchestrator, not final Runtime/release build.

## Validation

Run restore/build, focused `OfflineGeoworldAlphaSliceOrchestrator`, product smoke `OfflineGeoworldAlphaSliceOrchestratorProductSmokeTests`, VisualWorldStreamPreviewWorkspace focused/product smoke, CurrentState, `check-current-goal.ps1` for scenario `goal-108-offline-geoworld-alpha-slice-orchestrator`, `check-spine-fast.ps1`, `check-artifact-scope.ps1`, `git diff --check`, `git diff --cached --check`.

## Quality gate

GREEN only if no forbidden files changed, no historical Goal101-107 artifacts rewritten, no LFZ/network/provider/Runtime/schema/project/dependency changes, no Unity scenes/settings changes, AlphaRuntimeBootstrap unchanged, aggregate payload/scripts/editor helper exist, full-slice proof and negative proof pass, workspace integration is real, no raw geodata/binary media, source-health limits pass, validation/artifact-scope pass, final worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message: `GREEN Goal 108 offline geoworld alpha slice orchestrator` or BLOCKED/FAILED variant.
