# Goal 108A — Alpha Slice Source Split & Historical Artifact Immutability Audit

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Objective

Bounded quality/audit hotfix after Goal 108.

Goal108 functionally produced the one-click offline geoworld Alpha Slice orchestrator, but audit found two risks:
1. `OfflineGeoworldAlphaSliceOrchestratorEvidenceService.cs` is 987 physical lines / 934 loc, dangerously close to the 1000-line hard source-health ceiling.
2. Goal108 claims `historicalArtifactsUnchanged=true`, while the run report/user changed-file list indicates at least one historical Goal107 evidence file may have been rewritten. This must be audited with actual git diff/hash evidence, not trusted from the Goal108 gate.

Do not add new features. Preserve Goal108 behavior.

## Preflight

Confirm `main`, fetch `origin/main`, verify HEAD includes `989a79ab GREEN Goal 108 offline geoworld alpha slice orchestrator`. Inspect dirty state. Record `AlphaRuntimeBootstrap.cs` hash/line count and do not modify it.

## Read first

Read `AGENTS.md`, `docs/GOAL_PRODUCTIVITY_POLICY.md`, `docs/VALIDATION_PIPELINE.md`, Goal108 report/quality gate/simulated proof/negative proof/components/script inventory, `OfflineGeoworldAlphaSliceOrchestratorEvidenceService.cs`, docs quartet and debt register.

## Allowed files

- `src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceOrchestrator/`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaSliceOrchestrator/`
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaSliceOrchestratorProductSmokeTests.cs`
- `.llmgc/procedural/goal-108a-alpha-slice-source-split-immutability-audit/`
- docs quartet, debt register, artifact-scope policy
- this task pack.

## Forbidden files

Do not change Runtime, Runtime.Abstractions, public schema, providers, Lua, generator-library, `.sln`, `.csproj`, lock files, `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/settings/packages, existing Goal101-108 StreamingAssets, existing Goal101-108 evidence artifacts except new Goal108A evidence, binary/raster media, real geodata, network code, LFZ source, or external dependencies.

No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Required behavior

### 1. Split the Goal108 monolithic evidence service

Refactor `OfflineGeoworldAlphaSliceOrchestratorEvidenceService.cs` into smaller partial/helper files.

Requirements:
- Preserve public behavior and existing tests.
- Keep every file in the `OfflineGeoworldAlphaSliceOrchestrator` namespace under 700 physical/logical lines.
- Prefer cohesive files such as Builders, Readers, Proof, Quality, Writers, Hash/Utilities.
- No feature additions.
- No changed Unity files.

### 2. Actual historical artifact immutability audit

Create a BCL-only audit service or extend Goal108 tests to compare actual git blobs/hashes.

Audit must answer:
- Which Goal101-108 historical evidence/payload paths changed in commit `989a79ab` relative to parent `14ad9f38`?
- Were any Goal101-107 artifacts modified?
- Does Goal108's `historicalArtifactsUnchanged=true` match the actual git diff?
- If mismatch exists, record it as evidence-trust debt.

Do not modify/rewrite old Goal101-108 artifacts to hide the issue. Write only Goal108A evidence.

### 3. Evidence

Create `.llmgc/procedural/goal-108a-alpha-slice-source-split-immutability-audit/` with:
- `alpha-slice-source-split-immutability-audit-report.md`
- `alpha-slice-source-health-before-after.json`
- `alpha-slice-historical-artifact-diff-audit.json`
- `alpha-slice-immutability-trust-audit.json`
- `alpha-slice-source-split-quality-gate.json`
- `alpha-slice-negative-proof.json`

Evidence must prove source split completed, largest Goal108 orchestrator file is below 700 lines after split, actual git diff audit was performed, any mismatch between claimed `historicalArtifactsUnchanged` and actual changed paths is recorded honestly, no forbidden areas changed, and AlphaRuntimeBootstrap is unchanged.

### 4. Negative proof

Reject service file still over 700 lines, actual git diff not read, fake historical unchanged claim without comparing parent/head, modification of historical Goal101-108 evidence/payload artifacts by Goal108A, AlphaRuntimeBootstrap changed, Runtime/schema/provider/Unity scene/settings changes.

### 5. Tests

Focused tests: Goal108 behavior still builds evidence, source split guard passes, historical artifact diff audit reads actual git information, negative proof rejects expected cases, existing Goal108 product smoke still passes.

## Validation

Run restore/build, focused `OfflineGeoworldAlphaSliceOrchestrator`, product smoke `OfflineGeoworldAlphaSliceOrchestratorProductSmokeTests`, CurrentState, `check-current-goal.ps1 -Scenario "goal-108a-alpha-slice-source-split-immutability-audit"`, `check-spine-fast.ps1`, `check-artifact-scope.ps1`, `git diff --check`, `git diff --cached --check`.

## Quality gate

GREEN only if source split is complete, all Goal108 orchestrator files are below 700 lines, actual historical artifact immutability audit is honest, no forbidden files changed, validation passes, artifact scope passes, worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
`GREEN Goal 108A alpha slice source split immutability audit`
or BLOCKED/FAILED variant.
