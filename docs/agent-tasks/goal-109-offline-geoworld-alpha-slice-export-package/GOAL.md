# Goal 109 — Offline Geoworld Alpha Slice Export Package, Import Verifier & Clean-Run Acceptance Gate

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Objective

Deliver the next large milestone after Goal 108A: package the offline geoworld Alpha Slice into a portable, self-contained, verifiable acceptance package and add import/clean-run verification tooling.

Goal108 created the one-click Unity Alpha Slice orchestrator. Goal108A split source and audited historical immutability. Goal109 must turn the slice into something the user can review as a coherent package:

1. BCL-only Application export/package service over real Goal108/108A artifacts.
2. Deterministic package directory with manifest, checksums, runbook, readiness matrix and acceptance gate.
3. Unity Alpha package verifier and Editor package window.
4. WinForms/Visual World Stream Preview Workspace inspection.
5. Clean-import/clean-run simulated proof.
6. Negative proof for missing/tampered files, absolute paths, historical rewrites and fake acceptance.
7. Docs/state/debt synchronization.

This is still Alpha tooling only: no final Runtime, no public GamePackage schema changes, no real geodata, no network/provider calls, no final art, no Unity scene/prefab/project settings changes, no binary export archive required.

## Preflight

Confirm `main`, fetch `origin/main`, verify HEAD includes `ce22ed1a GREEN Goal 108A alpha slice source split immutability audit`. Verify Goal108 and Goal108A artifacts exist and `accepted=false`. Verify Goal108A says all Goal108 orchestrator files are below 700 lines, actual git diff audit was performed, and Goal101-107 artifacts were not modified by Goal108. Record `AlphaRuntimeBootstrap.cs` hash/line count and do not modify it. Inspect dirty state and do not touch unrelated user work.

## Read first

Read AGENTS.md, GOAL_PRODUCTIVITY_POLICY.md, MILESTONE_GATES.md, RELEASE_RISK_REGISTER.md, VALIDATION_PIPELINE.md, current state/queue/context/debt docs, Goal108 report/manifest/components/readiness/simulated proof/negative proof, Goal108A report/source split/immutability audit, current Unity alpha slice scripts/editor helper, and Visual World Stream Preview Workspace files.

## Allowed files

- `src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceExportPackage/`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaSliceExportPackage/`
- relevant VisualWorldStreamPreviewWorkspace tests
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaSliceExportPackageProductSmokeTests.cs`
- relevant VisualWorldStreamPreviewWorkspace product smoke
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaSlicePackageVerifier.cs`
- `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaSlicePackageWindow.cs`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal109/`
- `.llmgc/procedural/goal-109-offline-geoworld-alpha-slice-export-package/`
- `.llmgc/exports/goal-109-offline-geoworld-alpha-slice/`
- docs quartet, release risk/milestone docs if needed, debt register, artifact-scope policy and this task pack.

## Forbidden files

No LFZ archive/source, network/provider implementation, Runtime, Runtime.Abstractions, public GamePackage schema, Lua, generator-library, `.sln`, `.csproj`, lock files, `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/asmdefs/project settings/packages/build settings, existing Goal101-108 payloads/evidence except read-only references, binary/raster media, real geodata dumps, external dependencies, prompt dumps. No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Required implementation

### 1. Alpha slice export package

Create a BCL-only export service reading real Goal108/108A artifacts and writing both:

- `.llmgc/procedural/goal-109-offline-geoworld-alpha-slice-export-package/`
- `.llmgc/exports/goal-109-offline-geoworld-alpha-slice/`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal109/`

Required export package files:
- `offline-geoworld-alpha-export-manifest.json`
- `offline-geoworld-alpha-export-file-index.json`
- `offline-geoworld-alpha-export-checksums.json`
- `offline-geoworld-alpha-export-runbook.md`
- `offline-geoworld-alpha-export-acceptance-gate.json`
- `offline-geoworld-alpha-export-readme.md`

Package must include references to Goal101-108 components, source hashes, package-relative paths, manual gates, accepted=false status, not-final warnings, and no raw geodata. Do not create a binary zip unless explicitly requested later; a deterministic directory package is enough.

### 2. Clean-import / clean-run verifier

Implement a verifier that can validate the exported package from its package root:
- manifest present;
- all indexed files present;
- checksums match;
- no absolute paths;
- no raw geodata;
- no network/provider markers;
- no binary/raster media;
- all manual gates listed;
- final objective acceptance from Goal107 is included;
- Goal108A source split/immutability audit is included.

### 3. Unity verifier and Editor window

Add `OfflineGeoworldAlphaSlicePackageVerifier.cs` and `OfflineGeoworldAlphaSlicePackageWindow.cs`.

Unity verifier must read Goal109 package metadata from `Application.streamingAssetsPath`, expose status fields, verify checksums/counts where feasible, and avoid network/provider/LLM calls and AlphaRuntimeBootstrap dependency.

Editor window menu: `LLMGameCreator/Offline Geoworld Alpha Slice Package`. It must show package readiness, verify package, show runbook/acceptance summary, and not mutate scenes automatically.

### 4. Workspace integration

Add Visual World Stream Preview Workspace group `offline_geoworld_alpha_export_package` showing package file count, checksum status, clean-import proof, Unity verifier readiness, runbook summary, acceptance gate status, AlphaRuntimeBootstrap unchanged, diagnostics and manual instructions.

### 5. Evidence

Create:
- `offline-geoworld-alpha-export-report.md`
- `offline-geoworld-alpha-export-manifest.json`
- `offline-geoworld-alpha-export-file-index.json`
- `offline-geoworld-alpha-export-checksums.json`
- `offline-geoworld-alpha-export-clean-import-proof.json`
- `offline-geoworld-alpha-export-negative-proof.json`
- `offline-geoworld-alpha-export-unity-script-inventory.json`
- `offline-geoworld-alpha-export-editor-window-inventory.json`
- `offline-geoworld-alpha-export-workspace-binding-inventory.json`
- `offline-geoworld-alpha-export-source-lineage.json`
- `offline-geoworld-alpha-export-quality-gate-scan.json`

### 6. Negative proof

Reject missing Goal108 manifest, missing Goal108A audit, missing export manifest, missing indexed file, checksum mismatch, absolute path, raw geodata leak, binary/raster media marker, network/provider marker, AlphaRuntimeBootstrap dependency marker, Unity scene/settings mutation marker, accepted=true fake promotion, missing manual gate, missing not-final warnings, historical artifact rewrite attempt, fake clean import without reading files.

## Tests

Focused tests must verify export package generation, all required files, checksum verification, clean-import proof, tampered/missing file rejection, Unity inventory safety, editor menu/verify markers, workspace group, negative proof, source-health limits. Product smoke must read evidence/package/Unity payload and verify no forbidden areas.

## Docs/state

Update docs quartet, release risk/milestone docs if needed, and debt register. Manual gate: `offline_geoworld_alpha_slice_export_package_verification required`. Status: `accepted=false`.

Record that this is a portable Alpha review/export package, not a final release or runtime build.

## Validation

Run restore/build, focused `OfflineGeoworldAlphaSliceExportPackage`, product smoke `OfflineGeoworldAlphaSliceExportPackageProductSmokeTests`, VisualWorldStreamPreviewWorkspace focused/product smoke, CurrentState, `check-current-goal.ps1` for scenario `goal-109-offline-geoworld-alpha-slice-export-package`, `check-spine-fast.ps1`, `check-artifact-scope.ps1`, `git diff --check`, `git diff --cached --check`.

## Quality gate

GREEN only if no forbidden files changed, no historical Goal101-108 artifacts rewritten, no LFZ/network/provider/Runtime/schema/project/dependency changes, no Unity scenes/settings changes, AlphaRuntimeBootstrap unchanged, export package/verifier/editor helper exist, clean-import proof and negative proof pass, workspace integration is real, no raw geodata/binary media, source-health limits pass, validation/artifact-scope pass, final worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
`GREEN Goal 109 offline geoworld alpha slice export package`
or BLOCKED/FAILED variant.
