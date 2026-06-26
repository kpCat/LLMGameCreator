# Minimum Asset Pipeline Report

- Accepted: true
- Manual gate: minimum_asset_pipeline_artifact_verification
- Completed slices: S092, S093, S094, S095, S096, S097, S098, S098A
- Total resolved asset slots: 90
- Manifest hash: 45d01a85a07c3bf1def7ba88b671cfa05b82319581c3a317e5b0c28d61e111c7
- Deterministic hash: adb98a59bcd45f4e461be1d6f7882b95d7c13aeee0102148b56081835d154d71
- Product smoke route: minimum-asset-pipeline
- Public schema changed: false
- Project files changed: false

## Category Counts
- item_icon_ui_graphic: 24
- music_ambience: 6
- npc_portrait: 24
- sound_effect: 24
- tile_region_graphic: 12

## Import Counts
- item_icon_ui_graphic: 15
- music_ambience: 6
- npc_portrait: 18
- sound_effect: 13
- tile_region_graphic: 8

## Fallback Counts
- item_icon_ui_graphic: 9
- npc_portrait: 6
- sound_effect: 11
- tile_region_graphic: 4

## Invalid Matrix
- absolute_path_source: actualValid=false diagnostics=asset_pipeline.source.absolute_path
- copied_expectation_report_without_files: actualValid=false diagnostics=asset_pipeline.invalid.expectation_only_mutation_present
- cross_pack_asset_leakage: actualValid=false diagnostics=asset_pipeline.validation.cross_pack_asset_leakage
- duplicate_slot_ids: actualValid=false diagnostics=asset_pipeline.request.duplicate_slot_id
- executable_script_provider_payload_injection: actualValid=false diagnostics=asset_pipeline.source.command_payload,asset_pipeline.source.executable_payload
- mismatched_file_hash: actualValid=false diagnostics=asset_pipeline.validation.hash_mismatch
- missing_fixture_without_fallback_permission: actualValid=false diagnostics=asset_pipeline.fixture.missing
- over_budget_request: actualValid=false diagnostics=asset_pipeline.request.over_budget
- path_traversal_source: actualValid=false diagnostics=asset_pipeline.source.path_traversal
- tampered_package_content_hash: actualValid=false diagnostics=asset_pipeline.validation.package_content_hash_mismatch,asset_pipeline.validation.package_hash_mismatch
- unavailable_default_resolver: actualValid=false diagnostics=asset_pipeline.resolver_unavailable
- unknown_source_kind: actualValid=false diagnostics=asset_pipeline.source.kind
- unresolved_content_id: actualValid=false diagnostics=asset_pipeline.binding.unresolved_content_id
- unsupported_media_type: actualValid=false diagnostics=asset_pipeline.source.media_type
- wrong_media_type_or_corrupt_fixture: actualValid=false diagnostics=asset_pipeline.fixture.media_type_mismatch

## Category Binding Audit
- frontier_survival: item_icon_ui_graphic=8, music_ambience=2, npc_portrait=8, sound_effect=8, tile_region_graphic=4
- gothic_mystery: item_icon_ui_graphic=8, music_ambience=2, npc_portrait=8, sound_effect=8, tile_region_graphic=4
- trade_caravan: item_icon_ui_graphic=8, music_ambience=2, npc_portrait=8, sound_effect=8, tile_region_graphic=4

## External Execution
- LLM: false
- RAG: false
- Provider: false
- Lua: false
- Unity: false
- Media generation: false
