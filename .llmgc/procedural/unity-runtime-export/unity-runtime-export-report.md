# Unity Runtime Export Vertical Slice Report

- Accepted: true
- Manual gate: unity_runtime_export_vertical_slice_artifact_verification
- Completed slices: S099, S100, S101, S102, S103, S104, S105
- Selected package: game/content_generation/frontier-survival
- Selected pack: frontier_survival
- Package hash: 3e8a42663e1a2fdabd98cdd8c30ab6188810bd4d0f4d36aa4e3089a71b952d53
- Asset manifest hash: 3dd392bae4cbac24db34b1810a52c83cf64791521df8849c75ac61e8fdcfa595
- Export folder: .llmgc/procedural/unity-runtime-export/export
- Export files: 10
- Export bytes: 438774
- Export manifest hash: 9ad928a4855237b703945fb8db3b230496cce41723dce96001ec2d2ecfeb7e27
- Runtime config hash: 5d9393fae7306e18d9bd8a1d6c10754b04d36166ace0f37c20f8c5fb4c328beb
- Windows executable produced: false
- Unity Editor executed: false
- Product smoke route: unity-runtime-export

## Selected Loop

- Thread: thread/frontier-survival/000
- Generated ids: choice/frontier-survival/8cb7a7d288/028, dialogue/frontier-survival/51924b073c/028, event/frontier-survival/4f2d855a33/000, item/frontier-survival/7f81222990/004, item/frontier-survival/921dffbe2c/007, loot_table/frontier-survival/primary, objective/frontier-survival/67ba57beeb/000, quest/frontier-survival/c2ed6dc235/000
- Command hints: 5

## Asset Refs
- item_icon_ui_graphic: asset/game-content-generation-frontier-survival/item-icon-ui-graphic/item-frontier-survival-032bdc46ab-062/caravan-icon-fallback/000 -> assets/item-icon-ui-graphic/asset-game-content-generation-frontier-survival-item-icon-ui-graphic-item-frontier-survival-032bdc46ab-062-caravan-icon-fallback-000.fixture
- music_ambience: asset/game-content-generation-frontier-survival/music-ambience/event-frontier-survival-00ba2e87c9-005/caravan-music-fixture/000 -> assets/music-ambience/asset-game-content-generation-frontier-survival-music-ambience-event-frontier-survival-00ba2e87c9-005-caravan-music-fixture-000.fixture
- npc_portrait: asset/game-content-generation-frontier-survival/npc-portrait/npc-frontier-survival-11ffe0aa42-018/caravan-portrait-fallback/000 -> assets/npc-portrait/asset-game-content-generation-frontier-survival-npc-portrait-npc-frontier-survival-11ffe0aa42-018-caravan-portrait-fallback-000.fixture
- sound_effect: asset/game-content-generation-frontier-survival/sound-effect/dialogue-frontier-survival-0490f2f50e-013/caravan-sound-fallback/000 -> assets/sound-effect/asset-game-content-generation-frontier-survival-sound-effect-dialogue-frontier-survival-0490f2f50e-013-caravan-sound-fallback-000.fixture
- tile_region_graphic: asset/game-content-generation-frontier-survival/tile-region-graphic/map-frontier-survival-start/caravan-tile-fallback/002 -> assets/tile-region-graphic/asset-game-content-generation-frontier-survival-tile-region-graphic-map-frontier-survival-start-caravan-tile-fallback-002.fixture

## Invalid Matrix

- absolute_export_path: actualValid=false diagnostics=unity_runtime_export.contract.unsafe_export_path
- asset_manifest_hash_mismatch: actualValid=false diagnostics=unity_runtime_export.contract.asset_manifest_hash_mismatch
- copied_expectation_report_without_files: actualValid=false diagnostics=unity_runtime_export.contract.exported_asset_file_missing
- cross_pack_or_cross_asset_leakage: actualValid=false diagnostics=unity_runtime_export.contract.asset_ref_unresolved
- executable_script_provider_payload_injection: actualValid=false diagnostics=unity_runtime_export.contract.executable_payload_injection, unity_runtime_export.contract.exported_asset_file_missing
- mismatched_exported_file_hash: actualValid=false diagnostics=unity_runtime_export.contract.exported_asset_hash_mismatch
- missing_exported_file: actualValid=false diagnostics=unity_runtime_export.contract.exported_asset_file_missing
- missing_prior_asset_manifest_evidence: actualValid=false diagnostics=unity_runtime_export.contract.asset_manifest_hash_mismatch
- missing_prior_package_evidence: actualValid=false diagnostics=unity_runtime_export.contract.package_hash_mismatch
- package_hash_mismatch: actualValid=false diagnostics=unity_runtime_export.contract.package_hash_mismatch
- path_traversal_export_path: actualValid=false diagnostics=unity_runtime_export.contract.unsafe_export_path
- runtime_preview_only_dependency: actualValid=false diagnostics=unity_runtime_export.contract.runtime_preview_dependency
- unity_editor_build_claim_without_artifact: actualValid=false diagnostics=unity_runtime_export.contract.unity_editor_claim_without_artifact
- unresolved_asset_id: actualValid=false diagnostics=unity_runtime_export.contract.asset_ref_unresolved
- unresolved_package_id: actualValid=false diagnostics=unity_runtime_export.contract.start_map_unresolved

## Diagnostics

- error: unity_runtime_export.contract.asset_manifest_hash_mismatch [3dd392bae4cbac24db34b1810a52c83cf64791521df8849c75ac61e8fdcfa595] Runtime config asset manifest hash must match the selected Goal 011 manifest.
- error: unity_runtime_export.contract.asset_manifest_hash_mismatch [sha256/not-the-asset-manifest] Runtime config asset manifest hash must match the selected Goal 011 manifest.
- error: unity_runtime_export.contract.asset_ref_unresolved [asset/game-content-generation-frontier-survival/item-icon-ui-graphic/item-frontier-survival-032bdc46ab-062/caravan-icon-fallback/000] Runtime asset ref must be a strict subset of the selected Goal 011 asset manifest.
- error: unity_runtime_export.contract.asset_ref_unresolved [asset/missing] Runtime asset ref must be a strict subset of the selected Goal 011 asset manifest.
- error: unity_runtime_export.contract.executable_payload_injection [assets/payload.exe] Executable, script or provider payloads are not valid Unity runtime export assets.
- error: unity_runtime_export.contract.exported_asset_file_missing [assets/copied-report-only.fixture] Runtime asset ref must resolve to a real exported file.
- error: unity_runtime_export.contract.exported_asset_file_missing [assets/missing.fixture] Runtime asset ref must resolve to a real exported file.
- error: unity_runtime_export.contract.exported_asset_file_missing [assets/payload.exe] Runtime asset ref must resolve to a real exported file.
- error: unity_runtime_export.contract.exported_asset_hash_mismatch [assets/item-icon-ui-graphic/asset-game-content-generation-frontier-survival-item-icon-ui-graphic-item-frontier-survival-032bdc46ab-062-caravan-icon-fallback-000.fixture] Runtime asset ref hash must match the exported file bytes.
- error: unity_runtime_export.contract.package_hash_mismatch [game/content_generation/frontier-survival] Runtime config package id/hash must match the selected package evidence.
- error: unity_runtime_export.contract.package_hash_mismatch [game/content_generation/frontier-survival] Runtime config package id/hash must match the selected package evidence.
- error: unity_runtime_export.contract.runtime_preview_dependency [runtime_config] Unity runtime export must not depend on WinForms Runtime Preview.
- error: unity_runtime_export.contract.start_map_unresolved [map/missing] Start map must resolve in selected package data.
- error: unity_runtime_export.contract.unity_editor_claim_without_artifact [launch_metadata] Unity Editor/build execution is not accepted without a real reported artifact.
- error: unity_runtime_export.contract.unsafe_export_path [../escape.fixture] Export asset path must be relative and contained under the export root.
- error: unity_runtime_export.contract.unsafe_export_path [C:/escape.fixture] Export asset path must be relative and contained under the export root.
- info: unity_runtime_export.asset_manifest_validation_passed [asset_manifest] Selected asset refs must resolve to exported files with matching hashes.
- info: unity_runtime_export.goal011_gate_recorded [minimum_asset_pipeline_artifact_verification] User-confirmed Goal 011 artifact verification is recorded as passed.
- info: unity_runtime_export.invalid_matrix_rejected [invalid_matrix] Invalid/fake/leak scenarios must fail through the export validation path.
- info: unity_runtime_export.no_external_execution [harness] No Unity Editor, Unity build, Windows executable, LLM, RAG, provider, Lua or media execution was invoked.
- info: unity_runtime_export.package_validation_passed [package] The selected package must remain validator-clean.
- info: unity_runtime_export.valid_matrix_passed [valid_matrix] A deterministic export, replay and second valid input hash difference are required.
