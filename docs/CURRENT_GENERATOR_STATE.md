# Current Generator State

Status: source-of-truth handoff  
Updated by: Product Slice 028  
State file pair: docs/CURRENT_GENERATOR_STATE.json

## Current phase

M4.1 gate passed for sampled baseline contracts; Product Slice 028 repairs the controlled manual import workspace and adds a deterministic project-local semantic catalog foundation.

The project has a safe Capability Picker -> LLM Artifacts -> LLM Evaluation -> Artifact Review -> draft package assembly path. Product Slices 025-027 added the read-only archive review and controlled manual import workspace. Product Slice 028 fixes valid `manual-import` directory creation, suppresses no-op review-history snapshots, and maps approved `semantic_pack_v1` artifacts into deterministic `.llmgc/semantic/` sidecars without changing GamePackage v1.

## Gate decision

M4.1 real-model evaluation gate passed for sampled baseline contracts.

Evidence:

- Evaluation id: `strict_llm_evaluation/58df49dadbff5598`
- Evaluated at: `2026-06-18T16:43:35.9475873+00:00`
- Source capability selection id: `generator_plan_capability_selection/0b0addcd5c019328`
- Mode: `batch`
- Requested contracts: `game_profile_v1`, `mechanics_pack_v1`, `quest_pack_v1`, `scene_pack_v1`
- Iterations: `1`
- Repair enabled: `True`
- Stage for review: `True`
- Expected max LLM calls: `8`

Metrics:

- `total_contracts_requested`: 4
- `total_generation_runs`: 4
- `total_attempts`: 4
- `initial_pass_count`: 4
- `repair_pass_count`: 0
- `failed_count`: 0
- `valid_artifact_count`: 4
- `staged_for_review_count`: 4
- `markdown_fence_error_count`: 0
- `json_wrapper_error_count`: 0
- `json_invalid_count`: 0
- `wrong_artifact_kind_count`: 0
- `forbidden_field_count`: 0
- `invalid_id_count`: 0
- `missing_field_count`: 0
- `overall_pass_rate`: 1.0
- diagnostics: none
- quality warnings: none

Permanent evidence summary:

- docs/M4_1_REAL_EVALUATION_GATE_REPORT.md

## Last completed milestones

- M4.1: Strict LLM Generation Evaluation Pack.
- The M4.1 layer can evaluate the latest strict LLM generation audit without an LLM call or run a small explicit batch through the existing strict generation service.
- Evaluation stores JSON and markdown report artifacts with pass, repair, fail, diagnostic hot spot and quality warning metrics.
- A real local-model batch evaluation passed for the sampled baseline contracts listed above.

## Last completed product slice

- Product Slice 028: Manual Import Repair + Semantic Catalog Foundation v1.
- `UnityArchiveManualImportTemplateService` uses a dedicated archive-contained directory validator, so `manual-import` and `manual-import/put-files-here` are valid while traversal, rooted, drive-qualified, UNC-style, backslash and empty-segment paths remain rejected.
- Creating the directory does not create `import-manifest.json`, run import, or touch target outputs.
- `UnityArchiveManualProviderImportResult.TargetOutputsChanged` is true only when target bytes are written (`Imported`). Review/history/comparison refresh is skipped for `AlreadyImported`, conflict-only, invalid-only, and failed-only attempts; deterministic import reports are still written every time.
- Approved `semantic_pack_v1` artifacts map into `.llmgc/semantic/semantic-catalog.json` and Markdown report sidecars. Safe unknown terms remain `candidate`; unknown kinds normalize to `unknown` with warnings; unsafe ids and relations are diagnosed and skipped.
- The deterministic semantic generation-context preview writes JSON/Markdown with compact themes, tones, candidates, intents, motifs, asset/audio hints, relations, conflicts, and explicit LLM-minimization policy.
- `docs/GENERATION_PROCEDURE_AND_LLM_POLICY.md` records the user-facing generation procedure, LLM decision rule/load tiers, and data/formula/system/runtime extensibility tiers.
- Automatic one-click package-export invocation is intentionally deferred; the sidecar foundation is exercised directly through Application services and product smoke, while the existing GamePackage assembler continues to preserve `semantic_pack_v1` as unmapped.
- `semantic-catalog-foundation` proves all four semantic sidecar files without LLM/provider/generator/Lua/Unity/Runtime execution.

Checks recorded from the S028 run:

- `ManualImport` / `UnityArchiveReview` filtered tests: 54/54 passed.
- `Semantic` filtered tests: 9/9 passed.
- `semantic-catalog-foundation` product smoke: 1/1 passed.
- `unity-archive-manual-import-workflow-ui` product smoke: 1/1 passed.
- `ProductSmoke` filtered tests: 27/27 passed.
- `check-devflow-state.ps1`: passed in `STOP_REVIEW` mode.
- `check-all.ps1`: 655/655 tests passed, build 0 warnings / 0 errors.

- Product Slice 027: Controlled Manual Import Workspace UI v1.
- `Unity Archive Review` now lists expected asset/audio/Lua/unknown slots with provider, target path, status, file existence/size/hash, request/source ids, deterministic suggested source paths, selection detail, and six bounded filters.
- `UnityArchiveManualImportTemplateService` merges existing plan/index/state files without throwing on missing or malformed JSON and writes only `manual-import/import-manifest.template.json` for missing/invalid slots.
- Template creation never overwrites `manual-import/import-manifest.json`; suggested paths are relative to `manual-import/` and stay under `put-files-here/`.
- `Run manual import` calls the existing S026 `UnityArchiveManualProviderImportService` with fulfillment/review/history/comparison refresh enabled and different-byte overwrite disabled unless the risk-labelled checkbox is explicitly selected.
- Import report Markdown/JSON, fulfillment state, archive review, history, comparison, and selected snapshot detail refresh together; snapshot selection is preserved when it still exists.
- `unity-archive-manual-import-workflow-ui` proves the slot dashboard -> template -> user file/manifest -> existing import authority -> refreshed UI-state path.

Checks recorded from the S027 run:

- `ManualImport` / `UnityArchiveReview` filtered tests: 51/51 passed.
- `WinForms` filtered tests: 52/52 passed.
- `unity-archive-manual-import-workflow-ui` product smoke: 1/1 passed.
- `ProductSmoke` filtered tests: 26/26 passed.
- `check-devflow-state.ps1`: passed in `STOP_REVIEW` mode.
- `check-all.ps1`: 641/641 tests passed, build 0 warnings / 0 errors.

- Product Slice 026: Controlled Manual Provider Output Import v1.
- `UnityArchiveManualProviderImportService` remains the manifest/path/copy/report/refresh authority used by S027.

- Product Slice 025: Read-only Archive Review/History UI.
- Page id/title/sort order: `unity_archive_review` / `Unity Archive Review` / `41`.
- The page shows current review Markdown/JSON, comparison Markdown/JSON, history index JSON, readiness values, status summary, and snapshot list.
- Missing project/archive/report files and invalid JSON become stable view state instead of exceptions; Markdown remains readable when adjacent JSON is invalid.
- Refresh occurs on load, activation, and current-project changes; all displayed text is read-only.
- `unity-archive-review-ui-readonly` proves headless construction, report display, and byte-for-byte unchanged archive files.
- No S023/S024/S024.1 write-capable Application service is called by the UI.

Checks recorded from the S025 run:

- `ArchiveReview` / `UnityArchiveReview` filtered tests: 37/37 passed.
- `WinForms` filtered tests: 42/42 passed.
- `ProductSmoke` filtered tests: 24/24 passed.
- `unity-archive-review-ui-readonly` product smoke: 1/1 passed.
- `check-devflow-state.ps1`: passed in `STOP_REVIEW` mode.
- `check-all.ps1`: 619/619 tests passed, build 0 warnings / 0 errors.

- Product Slice 023: Unity Archive Read-only Review Snapshot v1.
- `UnityArchiveReviewSnapshotService` reads an already materialized `.llmgc/unity-archive` directory and writes deterministic review files under `production/`.
- Review outputs: `archive-review.json` and `archive-review.md`.
- The review aggregates materialization validation, provider readiness/job batches, fulfillment state, invalid output reasons, asset/audio/Lua request counts, diagnostics and archive-relative source file references.
- Missing archive/core files are reported as diagnostics and `MissingArchive` readiness instead of throwing.
- Review JSON contains no timestamps and no absolute archive root paths.
- Review file enumeration excludes the review outputs themselves, so repeated review of an unchanged archive remains byte-identical.
- `unity-archive-review-snapshot` product smoke passed.

- Product Slice 024: Unity Archive Review Retention & Comparison v1.
- `UnityArchiveReviewHistoryService` stores deterministic SHA-256 content-hash snapshots of `archive-review.json` under `review-history/<hash>/archive-review.json`.
- `UnityArchiveReviewHistoryIndex` tracks all snapshots in `production/archive-review-history-index.json`.
- Same content stored twice does not duplicate the index.
- `UnityArchiveReviewComparisonService` compares current vs previous snapshot and writes `production/archive-review-comparison.json` and `production/archive-review-comparison.md`.
- Comparison readiness: `Ready`, `ReadyWithWarnings`, `NoPreviousSnapshot`, `MissingReview`, `Invalid`, `Blocked`.
- Comparison dimensions: readiness, materialization readiness, provider plan readiness, source file count, diagnostic counts, fulfillment counts, provider slot counts, request counts.
- Diagnostic changes: added/resolved with stable fingerprints (severity|code|sourceFile|targetId|message).
- Source file changes: added/removed with stable fingerprints (relativePath|kind).
- Invalid reason changes: count deltas.
- All outputs are UTF-8 without BOM, deterministic, and contain no timestamps or absolute paths.
- `unity-archive-review-history` product smoke passed.

Checks recorded from the accepted S024 run:

- `UnityArchiveReviewHistory` and `UnityArchiveReviewComparison` filtered tests: 18/18 passed.
- `unity-archive-review-history` product smoke: 1/1 passed.
- `ProductSmoke` filtered tests: 23/23 passed.
- `check-all.ps1`: 606/606 tests passed, build 0 warnings / 0 errors.

## Current M5/M6 lock semantics

M5 and M6 task specs remain **Locked** after S028. The lock is intentional: manual-import repair, semantic sidecars, and LLM policy documentation do not open Lua executor integration or rich GamePackage assembly. They require a separate controlled product vertical-slice decision and explicit user approval.

Currently locked or restricted:

- M5 Lua module executor integration is Locked until a controlled vertical slice explicitly selects it.
- M6 rich GamePackage assembly beyond the current baseline draft assembly is Locked until a controlled vertical slice explicitly selects it.
- Broad contract expansion remains restricted beyond sampled baseline evidence.
- Runtime preview repair loop remains restricted until a controlled vertical slice exists.

Allowed next work remains bounded to manual import workflow polish after user testing, one controlled product vertical-slice selection, semantic catalog UI review/approval, or formula registry foundation. No Unity implementation, provider execution, Runtime expansion, GamePackage schema change, generator execution, LLM call or Lua execution is unlocked by Product Slice 028.

Parent slice foundation:

- Product Slice 022: Unity Archive Fulfillment State Scanner v1.
- `UnityArchiveFulfillmentStateService` scans expected output paths from provider job plan slots and checks physical file existence.
- Status logic: `missing` for absent files, `available` for existing non-empty files with correct extension, `invalid` for unsafe paths, wrong extensions, empty files or directory paths.
- Materialization writes five fulfillment state files under `production/`: fulfillment-state.json, fulfilled-assets-index.json, fulfilled-audio-index.json, fulfilled-lua-index.json, invalid-outputs.json.
- Fulfillment JSON contains no timestamps or absolute paths; available entries retain deterministic `fileSizeBytes`.
- Scanner diagnostics are preserved in `fulfillment-state.json` and `export-validation.json`; invalid existing outputs use stable deterministic reasons.
- No expected output files are physically created during materialization.
- `unity-archive-fulfillment-state` proves required files, status detection for missing/available/invalid, timestamp-free byte-identical repeated scan, and no Unity/Runtime/provider execution.

Parent slice foundation:

- Product Slice 021: Unity Archive Provider Job Plan v1.
- `UnityArchiveProviderJobPlanService` converts Slice 020 asset/audio/Lua request metadata into deterministic missing fulfillment slots, safe future output paths, five provider-specific non-executable job batches and a readiness report.
- Provider plan errors block materialization readiness; future providers remain warning-only, every job and batch has execution disabled, and provider `none` creates slots but no job.
- Product Slice 020: Unity Archive Asset/Audio/Lua Request Pipeline v1.

Parent slice foundation:

- Product Slice 018: Unity Archive Materialization v1.
- `UnityArchiveMaterializationService` consumes the Slice 017 dry run and writes a deterministic UTF-8 contract/meta archive under `.llmgc/unity-archive/`.
