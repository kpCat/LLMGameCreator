# Unity Alpha Media-Bound Playable Package Report

unity_alpha_media_bound_playable_package_verification required
implementationStatus=GREEN
accepted=false
manualGate=unity_alpha_media_bound_playable_package_verification
goal055AcceptedByUserHandoff=true
streamingAssetsPayloadStaged=true
physicalMediaFileCount=15
pngLoadProofPassed=true
wavLoadProofPassed=true
bundleProofPassed=true
unityEditorOrPlayerExecuted=true
unityMediaLoadContractPassed=true
familyMediaPanelProofPassed=true
invalidMatrixPassed=true
unitySourceChanged=true
sourceManifestHash=7d8b4882267253375cf67f22b9a20328e7a4ace0da6904f963edd7337d8f1c40
stagingManifestHash=979d329faf2014f8d0240ff2c013c465c233680c839e55ceae20c91b61cfd6f2
familyPanelModelsHash=27b866df95a172c3d21f5927adcc0e974384057aec09a4f87d7614c9ab0f89bd
unityLoadContractHash=9e7a3dc70224668de09caac5ed1d9a5f16b55d9ca02fb9771dcc4f67729a069a
unityLoadProofHash=6a3563fc86f6cbadddcb084c71f00cc825f88572e0867ff91f14c61a5c1bcab0
invalidMatrixHash=0c0cc4aa1000e3a06ae5652d3127d94949ecbde4f7bd0f7be80a2887ca0e670f
reportHash=f6689084dfb32fde2d9bd3621f060e286786b4528d7e1bd5769fceabc3e87a4c

## Preflight

- media_bound_playable_review_package_verification: status=passed, provenance=user_handoff, evidence=Goal 056 task preflight handoff
- semantic_pack_composition_blueprint_verification: status=produced_for_review_not_passed, provenance=inherited, evidence=Goal 031 preserved policy
- dynamic_semantic_feature_system_verification: status=produced_for_review_not_passed, provenance=inherited, evidence=Goal 032 preserved policy
- unity_alpha_media_bound_playable_package_verification: status=required, provenance=programmatic, evidence=Goal 056 produced for review

## Source Facts

- goal055AcceptedByUserHandoff: true
- goal055ReportWasGreenProducedForReview: true
- goal055PhysicalMediaFileCount: 15
- baseAlphaPayloadSourceRoot: .llmgc/procedural/minimum-playable-generated-game/build-source/staging
- Goal047: artifact=.llmgc/procedural/goal-047-full-generator-without-media-dry-run/dry-run-source-manifest.json, exists=true, hashMatches=true
- Goal047: artifact=.llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-first-person-grid-dungeon-dry-run.json, exists=true, hashMatches=true
- Goal047: artifact=.llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-map-panel-rpg-dry-run.json, exists=true, hashMatches=true
- Goal047: artifact=.llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-survival-sandbox-dry-run.json, exists=true, hashMatches=true
- Goal054: artifact=.llmgc/procedural/goal-054-media-materialization-review-package/materialized-media-inventory.json, exists=true, hashMatches=true
- Goal054: artifact=.llmgc/procedural/goal-054-media-materialization-review-package/media-binding-validation.json, exists=true, hashMatches=true
- Goal054: artifact=.llmgc/procedural/goal-054-media-materialization-review-package/media-materialization-review-package-report.md, exists=true, hashMatches=true
- Goal054: artifact=.llmgc/procedural/goal-054-media-materialization-review-package/media-provenance-license-ledger.json, exists=true, hashMatches=true
- Goal054: artifact=.llmgc/procedural/goal-054-media-materialization-review-package/media-review-package-manifest.json, exists=true, hashMatches=true
- Goal054: artifact=.llmgc/procedural/goal-054-media-materialization-review-package/preview-export-media-payloads.json, exists=true, hashMatches=true
- Goal054: artifact=.llmgc/procedural/goal-054-media-materialization-review-package/source-manifest.json, exists=true, hashMatches=true
- Goal055: artifact=.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/media-bound-playable-review-package-report.md, exists=true, hashMatches=true
- Goal055: artifact=.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/media-bound-review-package-manifest.json, exists=true, hashMatches=true
- Goal055: artifact=.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/source-manifest.json, exists=true, hashMatches=true
- Goal055: artifact=.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/streaming-assets-media-manifest.json, exists=true, hashMatches=true
- Goal055: artifact=.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/unity-media-load-contract.json, exists=true, hashMatches=true
- Goal055: artifact=.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/unity-media-load-proof-first-person-grid-dungeon.json, exists=true, hashMatches=true
- Goal055: artifact=.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/unity-media-load-proof-map-panel-rpg.json, exists=true, hashMatches=true
- Goal055: artifact=.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/unity-media-load-proof-survival-sandbox.json, exists=true, hashMatches=true

## StreamingAssets Payload

- passed: true
- manifest: media-bound/unity-alpha-media-bound-manifest.json
- basePayloadFileCount: 12
- physicalMediaFileCount: 15
- map_panel_rpg/world_key_art: kind=image, path=media-bound/media/map-panel-rpg/world-key-art-image-53ae8d80.png, sha256=53ae8d809055f1184aaaeb4f33ca70871014a6f3c3464adee9c4fbdd98f0bf38, bytes=3075, reviewTrace=goal054:materialization/media-binding-media-request-map-panel-rpg-world-key-art
- map_panel_rpg/npc_portrait: kind=image, path=media-bound/media/map-panel-rpg/npc-portrait-image-591d7cbc.png, sha256=591d7cbc2da72baf9cc1bf31d4a5ff0ab83c59c27018f64aaf8c9f1ea40a7261, bytes=3075, reviewTrace=goal054:materialization/media-binding-media-request-map-panel-rpg-npc-portrait
- map_panel_rpg/ui_panel_skin: kind=ui, path=media-bound/media/map-panel-rpg/ui-panel-skin-ui-a3cbdcfc.png, sha256=a3cbdcfc3f29edbbd07c7c5217d1d299cd3f20bf62ed754269391befbfc19c32, bytes=3070, reviewTrace=goal054:materialization/media-binding-media-request-map-panel-rpg-ui-panel-skin
- map_panel_rpg/sfx_interaction: kind=audio, path=media-bound/media/map-panel-rpg/sfx-interaction-audio-e48e6156.wav, sha256=e48e615626ba00a412cbef68bc2da2fab915b737e665ffdf3b410dac6d3246d7, bytes=8044, reviewTrace=goal054:materialization/media-binding-media-request-map-panel-rpg-sfx-interaction
- map_panel_rpg/export_placeholder_bundle: kind=bundle, path=media-bound/media/map-panel-rpg/export-placeholder-bundle-bundle-c7d669d0.json, sha256=c7d669d0fc0d31fe6eb82fe5d8ea79964ac9dfa04d6d53c7924091bbd8298376, bytes=542, reviewTrace=goal054:materialization/media-binding-media-request-map-panel-rpg-export-placeholder-bundle
- survival_sandbox/world_key_art: kind=image, path=media-bound/media/survival-sandbox/world-key-art-image-5d5642b3.png, sha256=5d5642b34a7a92d8ca07a64f65d9ae51914a00c074931c0d033f7292105b409c, bytes=3079, reviewTrace=goal054:materialization/media-binding-media-request-survival-sandbox-world-key-art
- survival_sandbox/npc_portrait: kind=image, path=media-bound/media/survival-sandbox/npc-portrait-image-b48f62ef.png, sha256=b48f62efc33557b93b9ed15c7d6c3177ab7278ae43d70deb312e00e40f48b4e8, bytes=3093, reviewTrace=goal054:materialization/media-binding-media-request-survival-sandbox-npc-portrait
- survival_sandbox/ui_panel_skin: kind=ui, path=media-bound/media/survival-sandbox/ui-panel-skin-ui-511877ab.png, sha256=511877abd6dadbad33e4ff77335988f755b57822987b664687947c99397acea4, bytes=3069, reviewTrace=goal054:materialization/media-binding-media-request-survival-sandbox-ui-panel-skin
- survival_sandbox/sfx_interaction: kind=audio, path=media-bound/media/survival-sandbox/sfx-interaction-audio-3b5dfd06.wav, sha256=3b5dfd0634ec7c50701344d33b1914d8d7bd44456b83f397147e75bfe7eff3f8, bytes=8044, reviewTrace=goal054:materialization/media-binding-media-request-survival-sandbox-sfx-interaction
- survival_sandbox/export_placeholder_bundle: kind=bundle, path=media-bound/media/survival-sandbox/export-placeholder-bundle-bundle-cc5f611d.json, sha256=cc5f611d1b0f1c7c6151b3811b473e5609fd1d1333a0599195a8f7995618a945, bytes=557, reviewTrace=goal054:materialization/media-binding-media-request-survival-sandbox-export-placeholder-bundle
- first_person_grid_dungeon/world_key_art: kind=image, path=media-bound/media/first-person-grid-dungeon/world-key-art-image-f0edddba.png, sha256=f0edddba9c021ec9be081d8e918a421a23ac4db79c83e8c771e9a98e8c238431, bytes=3072, reviewTrace=goal054:materialization/media-binding-media-request-first-person-grid-dungeon-world-key-art
- first_person_grid_dungeon/npc_portrait: kind=image, path=media-bound/media/first-person-grid-dungeon/npc-portrait-image-58b3a57f.png, sha256=58b3a57f848da37172b09ece95a764e4e5a6bdf083602e781746f969f458789c, bytes=3065, reviewTrace=goal054:materialization/media-binding-media-request-first-person-grid-dungeon-npc-portrait
- first_person_grid_dungeon/ui_panel_skin: kind=ui, path=media-bound/media/first-person-grid-dungeon/ui-panel-skin-ui-5d8c0a49.png, sha256=5d8c0a4911c6d2e3a219ed8a51fe04fc3ada0701fe86b724578739222ced1a2a, bytes=3067, reviewTrace=goal054:materialization/media-binding-media-request-first-person-grid-dungeon-ui-panel-skin
- first_person_grid_dungeon/sfx_interaction: kind=audio, path=media-bound/media/first-person-grid-dungeon/sfx-interaction-audio-951e0cd6.wav, sha256=951e0cd64e9842fe3519466109e9308f160ba0b51cdd40148533ed91f7a21fa7, bytes=8044, reviewTrace=goal054:materialization/media-binding-media-request-first-person-grid-dungeon-sfx-interaction
- first_person_grid_dungeon/export_placeholder_bundle: kind=bundle, path=media-bound/media/first-person-grid-dungeon/export-placeholder-bundle-bundle-f46e6dea.json, sha256=f46e6dea5dcd68acf0f4a8f03f8ee6351f0ce5d3557aca388d4f14cc915f3c7b, bytes=602, reviewTrace=goal054:materialization/media-binding-media-request-first-person-grid-dungeon-export-placeholder-bundle

## Family Panels

- passed: true
- map_panel_rpg: marker=media_bound_family_panel_proof=map_panel_rpg, bindings=5
- survival_sandbox: marker=media_bound_family_panel_proof=survival_sandbox, bindings=5
- first_person_grid_dungeon: marker=media_bound_family_panel_proof=first_person_grid_dungeon, bindings=5

## Unity Proof

- passed: true
- unityEditorOrPlayerExecuted: true
- blockerCode: (none)
- blockerMessage: (none)
- launchLog: .llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package/logs/alpha-player-launch.log
- playLoopLog: .llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package/logs/alpha-player-play-loop.log
- requiredMarker: media_bound_manifest_loaded=true
- requiredMarker: media_bound_family_count=3
- requiredMarker: media_bound_png_loaded=true
- requiredMarker: media_bound_wav_loaded=true
- requiredMarker: media_bound_bundle_loaded=true
- requiredMarker: media_bound_hash_validation=true
- requiredMarker: media_bound_playable_review_package_verification=required
- requiredMarker: media_bound_family_panel_proof=map_panel_rpg
- requiredMarker: media_bound_family_panel_proof=survival_sandbox
- requiredMarker: media_bound_family_panel_proof=first_person_grid_dungeon
- matchedMarker: media_bound_bundle_loaded=true
- matchedMarker: media_bound_family_count=3
- matchedMarker: media_bound_family_panel_proof=first_person_grid_dungeon
- matchedMarker: media_bound_family_panel_proof=map_panel_rpg
- matchedMarker: media_bound_family_panel_proof=survival_sandbox
- matchedMarker: media_bound_hash_validation=true
- matchedMarker: media_bound_manifest_loaded=true
- matchedMarker: media_bound_playable_review_package_verification=required
- matchedMarker: media_bound_png_loaded=true
- matchedMarker: media_bound_wav_loaded=true

## Invalid/fake/leak Matrix

- passed: true
- scenarioCount: 19
- duplicate_media_binding_id: expectedStatus=rejected, actualStatus=rejected, codes=goal056.binding.duplicate_id
- fake_family_id: expectedStatus=rejected, actualStatus=rejected, codes=goal056.family.fake_id
- fake_slot_id: expectedStatus=rejected, actualStatus=rejected, codes=goal056.slot.fake_id
- gamepackage_schema_mutation_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal056.boundary.gamepackage_schema
- lua_execution_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal056.boundary.lua_execution
- malformed_png: expectedStatus=rejected, actualStatus=rejected, codes=goal056.media.png_malformed
- malformed_wav: expectedStatus=rejected, actualStatus=rejected, codes=goal056.media.wav_malformed
- missing_goal055_source: expectedStatus=blocked, actualStatus=blocked, codes=goal056.source.goal055_missing
- missing_review_provenance_trace: expectedStatus=rejected, actualStatus=rejected, codes=goal056.review.trace_missing
- missing_staged_png: expectedStatus=rejected, actualStatus=rejected, codes=goal056.stage.png_missing
- missing_staged_wav: expectedStatus=rejected, actualStatus=rejected, codes=goal056.stage.wav_missing
- missing_unity_load_trace: expectedStatus=blocked, actualStatus=blocked, codes=goal056.unity.trace_missing
- nondeterministic_ordering: expectedStatus=rejected, actualStatus=rejected, codes=goal056.order.nondeterministic
- provider_network_llm_rag_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal056.boundary.provider_network_llm_rag
- runtime_ui_broad_mutation_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal056.boundary.runtime_ui
- stale_goal055_hash: expectedStatus=rejected, actualStatus=rejected, codes=goal056.source.goal055_hash_mismatch
- stale_unity_load_hash: expectedStatus=rejected, actualStatus=rejected, codes=goal056.unity.hash_mismatch
- unity_broad_refactor_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal056.boundary.unity_broad_refactor
- unsafe_relative_path: expectedStatus=rejected, actualStatus=rejected, codes=goal056.path.unsafe

## Diagnostics

- info: goal056.preflight.goal055_handoff_recorded [media_bound_playable_review_package_verification] Goal 055 is recorded as accepted by user handoff before Goal 056.
- info: goal056.source.goal055_loaded [Goal055] Goal 055 staged media-bound review package facts were loaded from repository-local evidence.
- info: goal056.unity.editor_executed [logs/unity-build.log] Unity Editor was invoked through the existing Alpha build entrypoint.
- info: goal056.unity.editor_exit_success [exit_code:0] Unity Editor build process exited successfully.
- info: goal056.unity.player_executed [logs/alpha-player-play-loop.log] The produced Alpha player was launched in diagnostic play-loop mode.
- info: goal056.unity.player_exit_success [exit_code:0] Alpha player process exited successfully.

## Boundaries

No provider/media generation, no network/import/download, no LLM/RAG call, no Lua execution, no public GamePackage schema, Runtime, Runtime.Abstractions, WinForms UI, provider path, generator-library, solution or project file change is part of this Goal 056 proof. Unity changes are limited to the repo-local Alpha media manifest loader, diagnostics and compact presentation panel.

unity_alpha_media_bound_playable_package_verification required
