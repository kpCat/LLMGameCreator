# Goal 110 — Offline Geoworld Alpha Slice Manual Acceptance Runner & Release Gate Dashboard

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Objective

Deliver a large product/readiness milestone after Goal 109.

Goal109 produced a portable offline geoworld Alpha Slice export package and clean-import verifier. Goal110 must turn that package into an explicit manual acceptance runner and release-gate dashboard for the current Alpha Slice.

This goal must not add another isolated proof layer. It must combine:
1. Application-side release-gate/acceptance-runner payload over real Goal109 package files.
2. Unity Editor manual acceptance runner window over the Goal109 package and Goal108 Alpha Slice.
3. Unity script for local acceptance result capture/readback in Alpha-only metadata form.
4. WinForms/Visual World Stream Preview Workspace dashboard for package/import/manual acceptance/readiness.
5. Deterministic simulated manual acceptance proof.
6. Release risk and milestone gate synchronization.
7. Negative proof, focused tests, product smoke.

This is not final Runtime, not final release packaging/installer/Steam, not final UI/art, not real geodata, not online provider work.

## Preflight

Confirm `main`, fetch `origin/main`, verify HEAD includes `e8e83b71 GREEN Goal 109 offline geoworld alpha slice export package`, verify Goal109 artifacts exist and `accepted=false`, verify Goal109 evidence proves packageFileCount=6, indexedFileCount=5, sourceComponentCount=7, cleanImportProofPassed=true, negativeProofPassed=true, Unity verifier/editor window ready. Record `AlphaRuntimeBootstrap.cs` hash/line count and do not modify it. Inspect dirty state and do not touch unrelated user work.

## Read first

Read AGENTS.md, GOAL_PRODUCTIVITY_POLICY.md, MILESTONE_GATES.md, RELEASE_RISK_REGISTER.md, VALIDATION_PIPELINE.md, current state/queue/context/debt docs, Goal108/108A/109 reports and quality gates, Goal109 export package files under `.llmgc/exports/goal-109-offline-geoworld-alpha-slice/`, Unity package verifier/window, and Visual World Stream Preview Workspace files.

## Allowed files

- `src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceManualAcceptanceGate/`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaSliceManualAcceptanceGate/`
- relevant VisualWorldStreamPreviewWorkspace tests
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaSliceManualAcceptanceGateProductSmokeTests.cs`
- relevant VisualWorldStreamPreviewWorkspace product smoke
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResult.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResultStore.cs`
- `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaAcceptanceRunnerWindow.cs`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal110/`
- `.llmgc/procedural/goal-110-offline-geoworld-alpha-manual-acceptance-gate/`
- `.llmgc/exports/goal-110-offline-geoworld-alpha-acceptance/`
- docs quartet, release risk register, milestone gates, debt register, artifact-scope policy and this task pack.

## Forbidden files

No LFZ archive/source, network/provider implementation, Runtime, Runtime.Abstractions, public GamePackage schema, Lua, generator-library, `.sln`, `.csproj`, lock files, `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/asmdefs/project settings/packages/build settings, existing Goal101-109 payloads/evidence except read-only references, binary/raster media, real geodata dumps, external dependencies, prompt dumps. No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Required implementation

### 1. Manual acceptance gate payload

Create a BCL-only service reading real Goal109 package/evidence and writing both `.llmgc/procedural/goal-110-offline-geoworld-alpha-manual-acceptance-gate/` and `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal110/`.

Required payload files:
- `offline-geoworld-alpha-acceptance-manifest.json`
- `offline-geoworld-alpha-acceptance-checklist.json`
- `offline-geoworld-alpha-acceptance-result-template.json`
- `offline-geoworld-alpha-release-gate-dashboard.json`
- `offline-geoworld-alpha-acceptance-readme.md`

Create export directory `.llmgc/exports/goal-110-offline-geoworld-alpha-acceptance/` with deterministic copies/summaries:
- acceptance manifest;
- checklist/runbook;
- result template;
- release-gate dashboard;
- file index/checksums.

Checklist must include at least these manual steps: open Unity project, open Alpha Slice window, setup rig, verify package, run travel, run interaction, save snapshot, load snapshot, replay, complete objectives, run package verifier, record diagnostics.

### 2. Unity manual acceptance runner

Add:
- `OfflineGeoworldAlphaAcceptanceRunnerWindow.cs`
- `OfflineGeoworldAlphaAcceptanceResult.cs`
- `OfflineGeoworldAlphaAcceptanceResultStore.cs`

Expected behavior:
- read Goal110 manifest/checklist from `Application.streamingAssetsPath`;
- show checklist step status fields and package paths;
- allow creating/clearing an acceptance runner object on demand;
- store/load local Alpha-only acceptance result JSON using `Application.persistentDataPath` or in-memory fallback;
- never auto-mutate scenes on import;
- no network/provider/LLM calls;
- no AlphaRuntimeBootstrap dependency;
- no external packages.

### 3. Simulated manual acceptance proof

.NET proof must simulate reading Goal109 package, Goal110 checklist/result template, walking every acceptance step, writing a synthetic result, loading it, validating result hashes, and producing final gate status `manualAcceptancePending=true` and `automatedGatePassed=true`.

### 4. Workspace dashboard

Add Visual World Stream Preview Workspace group `offline_geoworld_alpha_manual_acceptance` showing package status, checklist steps, automated gate status, manual gate pending, Unity runner readiness, result template path, release risk/milestone linkage, AlphaRuntimeBootstrap unchanged, diagnostics/manual instructions.

### 5. Release risk / milestone synchronization

Update release risk and milestone docs only if needed to record that the offline geoworld Alpha Slice now has a manual acceptance runner but is not final release. Keep `accepted=false` until human/manual verification.

### 6. Evidence

Create:
- `offline-geoworld-alpha-acceptance-report.md`
- `offline-geoworld-alpha-acceptance-manifest.json`
- `offline-geoworld-alpha-acceptance-checklist.json`
- `offline-geoworld-alpha-acceptance-result-template.json`
- `offline-geoworld-alpha-release-gate-dashboard.json`
- `offline-geoworld-alpha-acceptance-file-index.json`
- `offline-geoworld-alpha-acceptance-checksums.json`
- `offline-geoworld-alpha-acceptance-unity-script-inventory.json`
- `offline-geoworld-alpha-acceptance-editor-window-inventory.json`
- `offline-geoworld-alpha-acceptance-simulated-proof.json`
- `offline-geoworld-alpha-acceptance-negative-proof.json`
- `offline-geoworld-alpha-acceptance-workspace-binding-inventory.json`
- `offline-geoworld-alpha-acceptance-quality-gate-scan.json`

### 7. Negative proof

Reject missing Goal109 package, missing checklist step, accepted=true without manual result, fake manual result without file read, tampered result hash, absolute path in payload, raw geodata leak, network/provider marker, AlphaRuntimeBootstrap dependency, scene/prefab/settings mutation marker, binary/raster media marker, external dependency/new input-system marker, historical Goal101-109 artifact rewrite.

## Tests

Focused tests must verify payload/export generation, checklist step count, simulated proof, result template/readback/hash checks, Unity inventory safety, editor menu/create/clear/result markers, workspace dashboard group, release gate status, negative proof, source-health limits. Product smoke must read evidence, export directory and Unity payload, verify no forbidden areas.

## Docs/state

Update docs quartet, release risk/milestone docs if needed, and debt register. Manual gate: `offline_geoworld_alpha_manual_acceptance_verification required`. Status: `accepted=false`.

Record that this is a manual acceptance runner and release-gate dashboard for the offline geoworld Alpha Slice, not final release.

## Validation

Run restore/build, focused `OfflineGeoworldAlphaSliceManualAcceptanceGate`, product smoke `OfflineGeoworldAlphaSliceManualAcceptanceGateProductSmokeTests`, VisualWorldStreamPreviewWorkspace focused/product smoke, CurrentState, `check-current-goal.ps1` for scenario `goal-110-offline-geoworld-alpha-manual-acceptance-gate`, `check-spine-fast.ps1`, `check-artifact-scope.ps1`, `git diff --check`, `git diff --cached --check`.

## Quality gate

GREEN only if no forbidden files changed, no historical Goal101-109 artifacts rewritten, no LFZ/network/provider/Runtime/schema/project/dependency changes, no Unity scenes/settings changes, AlphaRuntimeBootstrap unchanged, acceptance payload/export/scripts/editor helper exist, simulated acceptance proof and negative proof pass, workspace integration is real, no raw geodata/binary media, source-health limits pass, validation/artifact-scope pass, final worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
`GREEN Goal 110 offline geoworld alpha manual acceptance gate`
or BLOCKED/FAILED variant.
