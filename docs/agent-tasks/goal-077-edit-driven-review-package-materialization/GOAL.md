# Goal 077 — Edit-Driven Review Package Materialization

Repo URL:
https://github.com/kpCat/LLMGameCreator

Working copy:
C:\Users\endim\LLMGameCreator\

Branch:
main

Codex reasoning:
very high

## Primary objective

Turn Goal 076 from a sidecar refresh proof into a deterministic, disk-backed review package materialization step.

Goal 076 proved the edit-driven playable preview refresh, produced a sidecar GamePackage refresh plan, and generated a staged Unity/player handoff manifest. Goal 077 must consume those real Goal 076 artifacts and write an actual review package directory with concrete JSON files, a package file ledger, a player-readable package index, hash verification, negative tamper/missing proof, focused tests, product smoke, and a bounded WinForms tab in the existing Campaign Authoring Review Workspace.

This goal must move the full combiner closer to a real generated game package, not just add another paper report.

## Current audit context

Post-Goal 076 audit found no P0/P1 blocker. Real GitHub/current main has:

- top commit expected: `f588ccd GREEN Goal 076 edit-driven playable preview refresh` or a later commit that preserves it;
- Goal 076 commit parent: `c8343e8 docs adaptive quality`;
- Goal 076 Application seam under `src/LLMGameCreator.Application/Design/EditDrivenPlayablePreviewRefresh/`;
- Goal 076 report `implementationStatus=GREEN`, `accepted=false`, `edit_driven_playable_preview_refresh_verification required`;
- Goal 076 proof rows=9, applied changes=18, package targets=18;
- Goal 076 quality gate currently reports minifiedSourceFileCount=0, filesOver1000LinesCount=0, maxLineLength about 251, parentUiBindingPassed=true, reportOnlySmokeDetected=false;
- Goal 076 explicitly kept public GamePackage schema/Runtime mutation out of scope and produced a sidecar refresh plan only.

That sidecar-only disposition is now the next product limitation to attack.

## Required preflight

1. Confirm current branch is `main`.
2. `git fetch origin main` is allowed for this task.
3. Confirm current top commit and whether `f588ccd` is still at `origin/main` or is an ancestor of the current top.
4. Confirm Goal 076 artifacts exist:
   - `.llmgc/procedural/goal-076-edit-driven-playable-preview-refresh/edit-driven-playable-preview-refresh-report.md`
   - `playable-preview-refresh-manifest.json`
   - `gamepackage-refresh-plan.json`
   - `unity-player-handoff-manifest.json`
   - `state-transition-proof.json`
   - `quality-gate-scan.json`
5. Confirm Goal 076 report is GREEN and accepted=false.
6. Record the user handoff acceptance of Goal 076 before starting Goal 077 by updating current-state docs. Do not mark Goal 076 artifact itself accepted/passed.
7. Confirm `c8343e8 docs adaptive quality` remains treated as P3 docs-context debt, not as a blocker.
8. Confirm no existing Goal 077 implementation already exists. If partial local work exists, inspect it and either integrate it safely or BLOCKED if it is outside allowed scope.

## Exact behavior

### 1. New Application seam

Create a new BCL-only Application seam under:

`src/LLMGameCreator.Application/Design/EditDrivenPlayableReviewPackageMaterialization/`

Suggested classes:

- `EditDrivenPlayableReviewPackageMaterializationEvidenceService`
- `EditDrivenPlayableReviewPackageMaterializationModels`
- `EditDrivenPlayableReviewPackageMaterializationHash`
- `EditDrivenPlayableReviewPackageMaterializationQualityGateScanner`

The service must consume Goal 076 artifacts from disk, not re-invent hardcoded success:

- Goal 076 report markdown or manifest hash;
- `playable-preview-refresh-manifest.json`;
- `gamepackage-refresh-plan.json`;
- `unity-player-handoff-manifest.json`;
- `state-transition-proof.json`;
- `quality-gate-scan.json`.

It must build a deterministic `EditDrivenPlayableReviewPackageMaterializationBuildResult` containing at least:

- source artifact manifest;
- review package manifest;
- package file ledger;
- player-readable package index;
- package target coverage matrix;
- state lineage proof;
- staged package read proof;
- tamper/missing negative proof;
- WinForms binding inventory;
- quality gate scan;
- report object and report markdown.

### 2. Real disk-backed review package

Write a review package directory under:

`.llmgc/procedural/goal-077-edit-driven-review-package-materialization/review-package/`

This directory must contain concrete deterministic JSON files, not only a top-level report. Required shape may be adjusted if a better existing repo convention is found, but it must include the same semantics:

```text
review-package/
  manifest.json
  package-index.json
  player-readable-index.json
  targets/
    <domain-or-family>/...
```

Every logical target from Goal 076 `gamepackage-refresh-plan.json` must materialize to a real file under `review-package/targets/**`.

Each materialized target file must include enough data to be reviewable and replayable:

- source row id;
- family id;
- seed id;
- field id;
- domain id;
- logical package path from Goal 076;
- before value;
- after value;
- before hash / after hash / rollback hash / replay hash lineage;
- validation requirement;
- source Goal 076 report hash or manifest hash.

The package manifest must list every file and expected SHA-256 hash. The product smoke must read those files from disk and validate hashes.

### 3. Player-readable package index

Create a deterministic player-readable package index that is derived from the review package files and Goal 076 handoff manifest.

It must not change Unity code. It must be a staged artifact that a future Unity/player handoff can read.

The index must prove:

- every Goal 076 player-facing scenario id maps to at least one materialized review package target;
- every expected player marker references an existing review package row/target;
- all 9 Goal 076 rows are represented;
- all 18 Goal 076 package targets are represented;
- hashes match the package ledger.

### 4. Staged package read and negative proof

Add a method that reads the staged review package from disk and validates:

- manifest exists;
- package index exists;
- player-readable index exists;
- all ledger files exist;
- every file hash matches;
- all expected rows/targets are present;
- source Goal 076 hashes match;
- before/after/rollback/replay lineage is still valid.

Negative proof must reject at least:

- missing package target file;
- tampered package target file;
- player index referencing a missing row or target.

Negative proof must be performed against temporary scratch copies or in-memory payloads, not by leaving extra tampered files in the tracked Goal 077 evidence directory.

### 5. WinForms workspace tab

Add a bounded separate UserControl:

- `CampaignReviewPackageControl.cs`
- `CampaignReviewPackageControl.Designer.cs`

Integrate it into existing `CampaignAuthoringReviewWorkspacePageControl` as a new tab, likely after the Goal 076 playable refresh tab.

Rules:

- Parent page remains a coordinator, not a god-form.
- The new tab is a separate UserControl.
- Parent `OnActivated()` loads/builds Goal 077 result through the Application seam and binds it into the new control.
- The tab should summarize status, row count, target/file count, package hash, negative proof status, and diagnostics.
- Do not perform file IO inside Designer code.

### 6. Evidence artifacts

Write Goal 077 artifacts under:

`.llmgc/procedural/goal-077-edit-driven-review-package-materialization/`

Required top-level artifacts:

- `edit-driven-review-package-materialization-report.md`
- `review-package-manifest.json`
- `package-file-ledger.json`
- `player-readable-package-index.json`
- `package-target-coverage.json`
- `state-lineage-proof.json`
- `tamper-negative-proof.json`
- `winforms-binding-inventory.json`
- `quality-gate-scan.json`
- `source-artifact-manifest.json`

Plus the disk-backed review package directory:

- `review-package/**`

The Goal 077 report must be:

- `implementationStatus=GREEN` only if all Goal 077 proofs pass;
- `accepted=false`;
- manual gate required: `edit_driven_review_package_materialization_verification required`.

Do not mark Goal 077 accepted/passed.

### 7. Current state/docs

Update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md` only if needed

Required docs behavior:

- Record that Goal 076 was accepted by user handoff before Goal 077.
- Set current produced-for-review gate to Goal 077.
- Keep Goal 077 `accepted=false` / manual verification required.
- Keep Goal 072 BLOCKED debt visible; do not claim it is solved.
- Keep `c8343e8 docs adaptive quality` as P3 docs-context debt unless you actually resolve it within allowed scope.

### 8. Quality gate scanner

The new Goal 077 scanner must fail if:

- Goal 077 report is GREEN but the review package directory has no concrete target files;
- product smoke only checks report status and does not read package files;
- parent workspace declares the tab but does not bind Goal 077 result;
- any new Goal 077 C# source/test file is minified or over 1000 lines;
- any changed/scanned C# line exceeds 500 chars;
- tracked Goal 077 evidence contains absolute local paths, timestamps, heavy logs, or scratch tamper files.

It must record AlphaRuntimeBootstrap.cs line count/hash/no-change status, but must not change Unity.

## Tests

Add focused tests under:

`tests/LLMGameCreator.Tests/Application/EditDrivenPlayableReviewPackageMaterialization/`

Minimum test coverage:

1. Service builds from real Goal 076 artifacts and writes all required Goal 077 artifacts.
2. All 9 rows and 18 targets materialize to concrete review package files.
3. Package ledger hashes match the files on disk.
4. Player-readable package index maps scenario ids/player markers to existing package rows/targets.
5. Before/after/rollback/replay lineage is preserved from Goal 076.
6. Missing package target file is rejected.
7. Tampered package target file is rejected.
8. Player index referencing a missing row/target is rejected.
9. WinForms parent activation binds Goal 077 data into `CampaignReviewPackageControl`, not only standalone control tests.
10. Quality scanner negative cases fail when package files are missing, parent bind is missing, or product smoke is report-only.

Add product smoke:

`tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenPlayableReviewPackageMaterializationProductSmokeTests.cs`

The product smoke must read the review package files from disk and verify behavior. It must not pass only because `report.ImplementationStatus == GREEN`.

## Validation

Use `validation.md` exactly unless a command is impossible in the local environment. If a command is impossible, explain why and run the closest bounded equivalent. Do not claim GREEN without a real focused validation chain and artifact-scope pass.

## Stop / block conditions

BLOCKED if:

- Goal 076 evidence is missing, not GREEN, or not accepted by user handoff in current state docs.
- Review package materialization cannot be implemented without changing forbidden public schema/Runtime/Unity/provider/Lua/generator-library/.sln/.csproj files.
- The product smoke cannot read and verify actual package files.
- check-all fails due a Goal 077 regression that cannot be repaired inside allowed scope.
- artifact scope cannot be made to pass without broadening allowed scope beyond this task.

FAILED if:

- compilation breaks and cannot be repaired inside allowed files;
- focused tests regress due Goal 077 changes and no bounded repair is possible;
- forbidden areas are touched.

## Mandatory commit/push policy

Always commit and push to `origin/main` even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:

- `GREEN Goal 077 edit-driven review package materialization`
- `BLOCKED Goal 077 edit-driven review package materialization`
- `FAILED Goal 077 edit-driven review package materialization`
