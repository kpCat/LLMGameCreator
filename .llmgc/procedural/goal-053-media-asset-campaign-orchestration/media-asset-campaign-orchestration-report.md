# Media Asset Campaign Orchestration Report

media_asset_campaign_orchestration_verification required
implementationStatus=GREEN
accepted=false
manualGate=media_asset_campaign_orchestration_verification
realProviderCalled=false
realMediaGenerationCalled=false
fixtureMediaProduced=true
familyCount=3
requestCount=36
fixtureFileCount=15
bindingCount=15
bindingManifestPassed=true
licenseLedgerPassed=true
invalidMatrixPassed=true

- productSmokeRoute: goal-053-media-asset-campaign-orchestration
- goal047AcceptedByUserHandoff: true
- catalogPassed: true
- requestQueuePassed: true
- reviewPromotionPassed: true
- fixtureInventoryPassed: true
- previewExportPayloadsPassed: true
- gamePackageSchemaChanged: false
- runtimeUiUnityChanged: false
- networkOrImportCalled: false
- sourceManifestHash: fa99f182b36f0c951fcc943d175d221d344983f444c2efeb1e812c61a06c6313
- requestQueueHash: 81fd3c33014b50971f5131d00aec49c3e2a5d2b73e9fbe0074d7924c3873370c
- fixtureInventoryHash: a7e160c8be60b1021ac12591b4b99d7e32324780024d81a0dd1d72c083ee5c1f
- bindingManifestHash: 95a2f8423adc69a9e67ac233994938437abf3fd5dfd902deee02493fe8d33979
- reportHash: c6515425053300dfcbd57b24c028114c04ad572dda67765f9891c627edf61b46

## Preflight

- full_generator_without_media_verification: status=passed, provenance=user_handoff, evidence=Goal 053 starting handoff
- semantic_pack_composition_blueprint_verification: status=produced_for_review_not_passed, provenance=inherited, evidence=Goal 031 preserved policy
- dynamic_semantic_feature_system_verification: status=produced_for_review_not_passed, provenance=inherited, evidence=Goal 032 preserved policy
- media_asset_campaign_orchestration_verification: status=required, provenance=programmatic, evidence=Goal 053 produced for review

## Source Manifest

- sourceArtifactRefCount: 13
- selectedFamilies: map_panel_rpg,survival_sandbox,first_person_grid_dungeon
- metamoduleKingdomOrRegionGroupCount: 7
- metamoduleRuntimeDeltaMarkerCount: 35
- metamoduleCompactedSpeciesArchetypeSlotRefCount: 112
- oneRequestPerSpeciesArchetypeSlotGenerated: false
- map_panel_rpg: scenario=gothic_intrigue, profile=gothic_intrigue, style=media-style/map-panel-rpg/gothic-intrigue, exportProfile=export-profile/map-panel-rpg/without-media, targets=10
- survival_sandbox: scenario=frontier_survival, profile=frontier_survival, style=media-style/survival-sandbox/frontier-survival, exportProfile=export-profile/survival-sandbox/without-media, targets=10
- first_person_grid_dungeon: scenario=metamodule_kingdoms, profile=metamodule_kingdoms, style=media-style/first-person-grid-dungeon/metamodule-kingdoms, exportProfile=export-profile/first-person-grid-dungeon/without-media, targets=10

## Slot Catalog

- passed: true
- world_key_art: kind=image, target=generated_world_or_family, fallback=Use neutral fixture key-art descriptor until reviewed media exists.
- region_tile_or_background: kind=image, target=region_or_chunk, fallback=Use generated-content fallback color/label tile.
- npc_portrait: kind=image, target=entity_or_npc, fallback=Use generic silhouette fixture per family.
- species_or_archetype_portrait: kind=image, target=species_or_archetype, fallback=Use compact family archetype placeholder; do not expand 112 files.
- item_icon: kind=image, target=item_or_resource, fallback=Use deterministic geometric fixture icon.
- quest_or_event_icon: kind=image, target=quest_or_event, fallback=Use text marker icon fixture.
- ui_panel_skin: kind=ui, target=ui_skin, fallback=Use plain fixture UI skin descriptor.
- sfx_interaction: kind=audio, target=interaction_or_command, fallback=Use text fixture cue id; runtime remains silent/fallback.
- sfx_combat_or_hazard: kind=audio, target=combat_or_hazard, fallback=Use text fixture cue id; runtime remains silent/fallback.
- ambient_loop: kind=audio, target=scenario_or_region, fallback=Use ambient fixture cue metadata only.
- music_stinger: kind=audio, target=quest_or_event, fallback=Use music fixture cue metadata only.
- export_placeholder_bundle: kind=bundle, target=preview_export_payload, fallback=Use explicit export placeholder bundle until reviewed media exists.

## Request Queue

- passed: true
- first_person_grid_dungeon: requests=12, fixtureReady=5, audio=4, image=6
- map_panel_rpg: requests=12, fixtureReady=5, audio=4, image=6
- survival_sandbox: requests=12, fixtureReady=5, audio=4, image=6

## License And Provenance

- passed: true
- fixture-generated-by-repo: policy=promote_as_fixture_only, autoPromoteGoal053=true
- manual-user-provided: policy=quarantine_until_manual_review, autoPromoteGoal053=false
- imported-cc0: policy=acceptable_with_source_record, autoPromoteGoal053=false
- imported-cc-by: policy=requires_attribution_record, autoPromoteGoal053=false
- imported-share-alike-or-gpl-risk: policy=quarantine_or_block, autoPromoteGoal053=false
- provider-generated-with-model-license: policy=future_provider_metadata_required, autoPromoteGoal053=false
- unknown/no-license: policy=reject, autoPromoteGoal053=false

## Candidate Review

- candidateCount: 27
- promotedFixtureCount: 15
- review/candidate-fixture-media-request-first-person-grid-dungeon-export-placeholder-bundle: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-fixture-media-request-first-person-grid-dungeon-npc-portrait: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-fixture-media-request-first-person-grid-dungeon-sfx-interaction: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-fixture-media-request-first-person-grid-dungeon-ui-panel-skin: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-fixture-media-request-first-person-grid-dungeon-world-key-art: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-fixture-media-request-map-panel-rpg-export-placeholder-bundle: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-fixture-media-request-map-panel-rpg-npc-portrait: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-fixture-media-request-map-panel-rpg-sfx-interaction: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-fixture-media-request-map-panel-rpg-ui-panel-skin: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-fixture-media-request-map-panel-rpg-world-key-art: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-fixture-media-request-survival-sandbox-export-placeholder-bundle: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-fixture-media-request-survival-sandbox-npc-portrait: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-fixture-media-request-survival-sandbox-sfx-interaction: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-fixture-media-request-survival-sandbox-ui-panel-skin: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-fixture-media-request-survival-sandbox-world-key-art: decision=promote_fixture, promoted=true, cause=goal053.review.fixture_promoted
- review/candidate-gpl-risk-auto-promotion-attempt: decision=blocked_license, promoted=false, cause=goal053.license.share_alike_or_gpl_risk
- review/candidate-imported-cc-by-missing-attribution: decision=blocked_missing_provenance, promoted=false, cause=goal053.license.attribution_missing
- review/candidate-imported-cc0-media-request-first-person-grid-dungeon-item-icon: decision=needs_manual_review, promoted=false, cause=goal053.review.manual_or_import_requires_later_review
- review/candidate-imported-cc0-media-request-map-panel-rpg-item-icon: decision=needs_manual_review, promoted=false, cause=goal053.review.manual_or_import_requires_later_review
- review/candidate-imported-cc0-media-request-survival-sandbox-item-icon: decision=needs_manual_review, promoted=false, cause=goal053.review.manual_or_import_requires_later_review
- review/candidate-leak-final-artwork-claim: decision=blocked_leak, promoted=false, cause=goal053.review.leak_claim
- review/candidate-manual-media-request-first-person-grid-dungeon-region-tile-or-background: decision=needs_manual_review, promoted=false, cause=goal053.review.manual_or_import_requires_later_review
- review/candidate-manual-media-request-map-panel-rpg-region-tile-or-background: decision=needs_manual_review, promoted=false, cause=goal053.review.manual_or_import_requires_later_review
- review/candidate-manual-media-request-survival-sandbox-region-tile-or-background: decision=needs_manual_review, promoted=false, cause=goal053.review.manual_or_import_requires_later_review
- review/candidate-mismatch-wrong-kind: decision=blocked_mismatch, promoted=false, cause=goal053.review.media_mismatch
- review/candidate-provider-missing-metadata: decision=blocked_provider_not_configured, promoted=false, cause=goal053.provider.metadata_missing
- review/candidate-unknown-no-license: decision=blocked_missing_provenance, promoted=false, cause=goal053.license.unknown

## Fixture Files

- passed: true
- fixtures/audio/media-request-first-person-grid-dungeon-sfx-interaction.txt: bytes=629, sha256=85fce5bc0eed8f1b263c91ca3cdfdecbaa9f83c83f53cc2f74f0870a428f5b43, request=media-request/first-person-grid-dungeon/sfx-interaction, target=family/first_person_grid_dungeon/scenario/metamodule_kingdoms/system/dialogue, status=fixture_asset_only_not_final_media
- fixtures/audio/media-request-map-panel-rpg-sfx-interaction.txt: bytes=557, sha256=76cdbbb09728737f279c064e9d20a9b8b2977f58086d446390a6cf9dc69647ed, request=media-request/map-panel-rpg/sfx-interaction, target=family/map_panel_rpg/scenario/gothic_intrigue/system/dialogue, status=fixture_asset_only_not_final_media
- fixtures/audio/media-request-survival-sandbox-sfx-interaction.txt: bytes=578, sha256=0bb276ff609270b5a736c41ed24825e59413fe3129333185d74f8ac17f30d3d4, request=media-request/survival-sandbox/sfx-interaction, target=family/survival_sandbox/scenario/frontier_survival/system/dialogue, status=fixture_asset_only_not_final_media
- fixtures/bundles/media-request-first-person-grid-dungeon-export-placeholder-bundle.txt: bytes=637, sha256=df6f5220644f5949f5f183623b1401720820bfbc809149830c53041e33d024cf, request=media-request/first-person-grid-dungeon/export-placeholder-bundle, target=export-profile/first-person-grid-dungeon/without-media, status=fixture_asset_only_not_final_media
- fixtures/bundles/media-request-map-panel-rpg-export-placeholder-bundle.txt: bytes=569, sha256=d6dd5dd35ae0d35f529bb6fcd53fde2fd64621f985ebdbd2360da9a525ceeaff, request=media-request/map-panel-rpg/export-placeholder-bundle, target=export-profile/map-panel-rpg/without-media, status=fixture_asset_only_not_final_media
- fixtures/bundles/media-request-survival-sandbox-export-placeholder-bundle.txt: bytes=588, sha256=d4bff0a276bca442dc00c67e40d8d13ea03fb59607c10ce752bb80e6d69508d1, request=media-request/survival-sandbox/export-placeholder-bundle, target=export-profile/survival-sandbox/without-media, status=fixture_asset_only_not_final_media
- fixtures/images/media-request-first-person-grid-dungeon-npc-portrait.txt: bytes=618, sha256=35687c444bff4c55fa6ecc4184127ec5440bcbdcdbea71c1426140e1943039ca, request=media-request/first-person-grid-dungeon/npc-portrait, target=family/first_person_grid_dungeon/scenario/metamodule_kingdoms/system/entity, status=fixture_asset_only_not_final_media
- fixtures/images/media-request-first-person-grid-dungeon-world-key-art.txt: bytes=620, sha256=52a7b73c18315cbdd5b27e47bceaef6212baa5ce20e9cf527db0d746e5ad35ab, request=media-request/first-person-grid-dungeon/world-key-art, target=family/first_person_grid_dungeon/scenario/metamodule_kingdoms/system/world, status=fixture_asset_only_not_final_media
- fixtures/images/media-request-map-panel-rpg-npc-portrait.txt: bytes=546, sha256=a021ddb80f80cb768d0fe6c5f7cdc150d3345ffe89ee14f6ee299305e40deeae, request=media-request/map-panel-rpg/npc-portrait, target=family/map_panel_rpg/scenario/gothic_intrigue/system/entity, status=fixture_asset_only_not_final_media
- fixtures/images/media-request-map-panel-rpg-world-key-art.txt: bytes=548, sha256=096a54d2d5c40215f3d88555996dd80cfa6fae013a0c63d2dfe4558405d31c21, request=media-request/map-panel-rpg/world-key-art, target=family/map_panel_rpg/scenario/gothic_intrigue/system/world, status=fixture_asset_only_not_final_media
- fixtures/images/media-request-survival-sandbox-npc-portrait.txt: bytes=567, sha256=63e048fcd31a8cfbdfa336af2b72a84604e0be75c5a432d4bc947a40c49ce333, request=media-request/survival-sandbox/npc-portrait, target=family/survival_sandbox/scenario/frontier_survival/system/entity, status=fixture_asset_only_not_final_media
- fixtures/images/media-request-survival-sandbox-world-key-art.txt: bytes=569, sha256=90c88754a233b5ff7d9f8940cca5300b5c4e43e7c3fdcc31f68ea777f6d978a0, request=media-request/survival-sandbox/world-key-art, target=family/survival_sandbox/scenario/frontier_survival/system/world, status=fixture_asset_only_not_final_media
- fixtures/ui/media-request-first-person-grid-dungeon-ui-panel-skin.txt: bytes=617, sha256=783876631b0d42c8bb7ea226b168dd8c9596ff0d67be7c7bc40281dad7a10e10, request=media-request/first-person-grid-dungeon/ui-panel-skin, target=family/first_person_grid_dungeon/scenario/metamodule_kingdoms/system/event, status=fixture_asset_only_not_final_media
- fixtures/ui/media-request-map-panel-rpg-ui-panel-skin.txt: bytes=545, sha256=3cf9dae3527d40c40abd58c992d877363bea3dc742c592c2185a0dd2dbf61dd8, request=media-request/map-panel-rpg/ui-panel-skin, target=family/map_panel_rpg/scenario/gothic_intrigue/system/event, status=fixture_asset_only_not_final_media
- fixtures/ui/media-request-survival-sandbox-ui-panel-skin.txt: bytes=566, sha256=cb9259d32a4c855f75dfd1a7b59776856a9fb0aa21c1145303f817b2a707ac28, request=media-request/survival-sandbox/ui-panel-skin, target=family/survival_sandbox/scenario/frontier_survival/system/event, status=fixture_asset_only_not_final_media

## Bindings And Payloads

- bindingManifestPassed: true
- explicitFallbackCount: 21
- map_panel_rpg: bindings=5, image=2, audio=1, uiOrBundle=2, fallback=true, packageRuntimeExportPayloadsMutated=false
- survival_sandbox: bindings=5, image=2, audio=1, uiOrBundle=2, fallback=true, packageRuntimeExportPayloadsMutated=false
- first_person_grid_dungeon: bindings=5, image=2, audio=1, uiOrBundle=2, fallback=true, packageRuntimeExportPayloadsMutated=false

## Invalid/fake/leak Matrix

- passed: true
- scenarioCount: 19
- cc_by_without_attribution: expectedStatus=rejected, actualStatus=rejected, codes=goal053.license.attribution_missing
- duplicate_media_request_id: expectedStatus=rejected, actualStatus=rejected, codes=goal053.request.duplicate_id
- external_absolute_path_in_artifact: expectedStatus=rejected, actualStatus=rejected, codes=goal053.artifact.absolute_path
- fake_source_artifact_hash_or_path: expectedStatus=rejected, actualStatus=rejected, codes=goal053.source.fake_hash_or_path
- final_prose_or_final_artwork_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal053.boundary.final_claim
- invalid_media_kind: expectedStatus=rejected, actualStatus=rejected, codes=goal053.media_kind.invalid
- missing_required_provenance: expectedStatus=rejected, actualStatus=rejected, codes=goal053.provenance.missing
- network_url_treated_as_downloaded_asset: expectedStatus=rejected, actualStatus=rejected, codes=goal053.artifact.network_url
- nondeterministic_ordering: expectedStatus=rejected, actualStatus=rejected, codes=goal053.order.nondeterministic
- path_traversal_in_fixture_path: expectedStatus=rejected, actualStatus=rejected, codes=goal053.fixture.path_traversal
- provider_candidate_without_model_license_run_metadata: expectedStatus=blocked, actualStatus=blocked, codes=goal053.provider.metadata_missing
- provider_llm_rag_call_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal053.boundary.provider_llm_rag
- runtime_ui_unity_gamepackage_mutation_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal053.boundary.runtime_ui_unity_gamepackage
- self_promotion_without_review_trace: expectedStatus=rejected, actualStatus=rejected, codes=goal053.review.trace_missing
- share_alike_gpl_risk_auto_promotion: expectedStatus=blocked, actualStatus=blocked, codes=goal053.license.share_alike_or_gpl_risk
- unknown_family_id: expectedStatus=rejected, actualStatus=rejected, codes=goal053.family.unknown
- unknown_generated_target_id: expectedStatus=rejected, actualStatus=rejected, codes=goal053.target.unknown
- unknown_media_slot_id: expectedStatus=rejected, actualStatus=rejected, codes=goal053.slot.unknown
- unknown_no_license_candidate_accepted_attempt: expectedStatus=rejected, actualStatus=rejected, codes=goal053.license.unknown

## Diagnostics

- warning: goal053.license.attribution_missing [candidate/imported-cc-by/missing-attribution] CC-BY candidates require attribution records.
- warning: goal053.license.share_alike_or_gpl_risk [candidate/gpl-risk/auto-promotion-attempt] Share-alike/GPL-risk candidates are blocked for Goal 053.
- warning: goal053.license.unknown [candidate/unknown/no-license] Unknown/no-license candidates are rejected.
- warning: goal053.provider.metadata_missing [candidate/provider/missing-metadata] Provider candidates are blocked until model/license/run metadata and provider configuration exist.
- warning: goal053.review.leak_claim [candidate/leak/final-artwork-claim] Candidate includes a forbidden final/provenance/boundary claim.
- warning: goal053.review.media_mismatch [candidate/mismatch/wrong-kind] Fixture candidate media kind or slot does not match the request.
- info: goal053.binding.manifest_built [media-binding-manifest] Promoted fixture candidates are bound to generated target ids with explicit fallback records for unfilled slots.
- info: goal053.catalog.built [media-slot-catalog] Media slot catalog covers required image/audio/ui/bundle categories.
- info: goal053.fixture.inventory_built [media-fixture-file-inventory] Deterministic textual fixture descriptors are hashed and bound to request ids.
- info: goal053.license.ledger_built [media-license-provenance-ledger] License/provenance policies cover fixture, manual, imported, provider and unknown sources.
- info: goal053.preflight.goal047_handoff_recorded [full_generator_without_media_verification] Goal 047 is recorded as accepted by user handoff before Goal 053 evidence.
- info: goal053.preview_export.payloads_built [preview-export-media-payloads] Preview/export payload proof consumes media bindings without mutating package/runtime/export payloads.
- info: goal053.request_queue.built [media-request-queue] Request queue covers three families, all required media slot categories and compacted metamodule stress facts.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-first-person-grid-dungeon-export-placeholder-bundle] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-first-person-grid-dungeon-npc-portrait] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-first-person-grid-dungeon-sfx-interaction] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-first-person-grid-dungeon-ui-panel-skin] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-first-person-grid-dungeon-world-key-art] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-map-panel-rpg-export-placeholder-bundle] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-map-panel-rpg-npc-portrait] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-map-panel-rpg-sfx-interaction] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-map-panel-rpg-ui-panel-skin] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-map-panel-rpg-world-key-art] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-survival-sandbox-export-placeholder-bundle] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-survival-sandbox-npc-portrait] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-survival-sandbox-sfx-interaction] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-survival-sandbox-ui-panel-skin] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.fixture_promoted [candidate/fixture/media-request-survival-sandbox-world-key-art] Repository-generated fixture candidate promoted as fixture asset only.
- info: goal053.review.ledger_built [media-review-promotion-ledger] Review ledger promotes fixture candidates only and blocks risky provenance or leak candidates.
- info: goal053.review.manual_or_import_requires_later_review [candidate/imported-cc0/media-request-first-person-grid-dungeon-item-icon] Manual/import candidates remain quarantined for later review.
- info: goal053.review.manual_or_import_requires_later_review [candidate/imported-cc0/media-request-map-panel-rpg-item-icon] Manual/import candidates remain quarantined for later review.
- info: goal053.review.manual_or_import_requires_later_review [candidate/imported-cc0/media-request-survival-sandbox-item-icon] Manual/import candidates remain quarantined for later review.
- info: goal053.review.manual_or_import_requires_later_review [candidate/manual/media-request-first-person-grid-dungeon-region-tile-or-background] Manual/import candidates remain quarantined for later review.
- info: goal053.review.manual_or_import_requires_later_review [candidate/manual/media-request-map-panel-rpg-region-tile-or-background] Manual/import candidates remain quarantined for later review.
- info: goal053.review.manual_or_import_requires_later_review [candidate/manual/media-request-survival-sandbox-region-tile-or-background] Manual/import candidates remain quarantined for later review.
- info: goal053.source.compact_refs_only [media-campaign-source-manifest] Goal 053 references source artifact paths and hashes without copying heavy source JSON.

## Boundaries

No real provider/media generation, no network/import, no GamePackage schema, Runtime, Runtime.Abstractions, WinForms UI, Unity/export, provider/LLM/RAG, Lua, generator-library, solution/project or Designer file change is required by this Goal 053 evidence.

media_asset_campaign_orchestration_verification required
