# Media Materialization Review Package Report

media_materialization_review_package_verification required
implementationStatus=GREEN
accepted=false
manualGate=media_materialization_review_package_verification
goal053AcceptedByUserHandoff=true
goal053SourceReportGreenRequired=true
physicalMediaProduced=true
familyCount=3
queueItemCount=15
materializedFileCount=15
pngFileCount=9
wavFileCount=3
bundleJsonFileCount=3
pngProofPassed=true
wavProofPassed=true
bindingValidationPassed=true
provenanceLicenseLedgerPassed=true
previewExportPayloadsPassed=true
reviewPackageManifestPassed=true
invalidMatrixPassed=true
providerNetworkLlmRagCalled=false
gamePackageSchemaChanged=false
runtimeUiUnityChanged=false
sourceManifestHash=24815816ff26746d0fbc4f742fb242968f93bc299da716b030c4eaca04d6f060
queueHash=898121488f42fef71f7cb6d95551c674e5880ddeed40599c9cdb41dc1e59810f
inventoryHash=7840ac0deebb6952da09c90741cdab09c9787540159af1c98c675440ef72ea5d
reviewPackageManifestHash=d221b16bd28048ea8a58e156912f644b7fd8fa422496c8249e451043eb400bfd
reportHash=6e4b739ac35b6717afcce6f9a73000305e043cdea1ebd493363a322f774a3ea5

## Preflight

- media_asset_campaign_orchestration_verification: status=passed, provenance=user_handoff, evidence=Goal 054 starting handoff
- semantic_pack_composition_blueprint_verification: status=produced_for_review_not_passed, provenance=inherited, evidence=Goal 031 preserved policy
- dynamic_semantic_feature_system_verification: status=produced_for_review_not_passed, provenance=inherited, evidence=Goal 032 preserved policy
- media_materialization_review_package_verification: status=required, provenance=programmatic, evidence=Goal 054 produced for review

## Source Facts

- sourceArtifactRefCount: 14
- goal053RequestCount: 36
- goal053BindingCount: 15
- map_panel_rpg: scenario=gothic_intrigue, profile=gothic_intrigue, requests=12, bindings=5, dryRun=.llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-map-panel-rpg-dry-run.json, preview=.llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/chunked-preview-payload-gothic.json, exportProfile=export-profile/map-panel-rpg/without-media
- survival_sandbox: scenario=frontier_survival, profile=frontier_survival, requests=12, bindings=5, dryRun=.llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-survival-sandbox-dry-run.json, preview=.llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/chunked-preview-payload-frontier.json, exportProfile=export-profile/survival-sandbox/without-media
- first_person_grid_dungeon: scenario=metamodule_kingdoms, profile=metamodule_kingdoms, requests=12, bindings=5, dryRun=.llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-first-person-grid-dungeon-dry-run.json, preview=.llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/chunked-preview-payload-metamodule.json, exportProfile=export-profile/first-person-grid-dungeon/without-media

## Materialization Queue

- passed: true
- materialization/media-binding-media-request-map-panel-rpg-export-placeholder-bundle: family=map_panel_rpg, slot=export_placeholder_bundle, kind=bundle, format=bundle_manifest_json, path=review-package/media/bundles/media-binding-media-request-map-panel-rpg-export-placeholder-bundle.json, sha256=c7d669d0fc0d31fe6eb82fe5d8ea79964ac9dfa04d6d53c7924091bbd8298376, role=export_review_bundle_manifest
- materialization/media-binding-media-request-map-panel-rpg-npc-portrait: family=map_panel_rpg, slot=npc_portrait, kind=image, format=png, path=review-package/media/images/media-binding-media-request-map-panel-rpg-npc-portrait.png, sha256=591d7cbc2da72baf9cc1bf31d4a5ff0ab83c59c27018f64aaf8c9f1ea40a7261, role=preview_character_focus
- materialization/media-binding-media-request-map-panel-rpg-sfx-interaction: family=map_panel_rpg, slot=sfx_interaction, kind=audio, format=wav_pcm_s16_mono, path=review-package/media/audio/media-binding-media-request-map-panel-rpg-sfx-interaction.wav, sha256=e48e615626ba00a412cbef68bc2da2fab915b737e665ffdf3b410dac6d3246d7, role=preview_interaction_audio_cue
- materialization/media-binding-media-request-map-panel-rpg-ui-panel-skin: family=map_panel_rpg, slot=ui_panel_skin, kind=ui, format=png, path=review-package/media/ui/media-binding-media-request-map-panel-rpg-ui-panel-skin.png, sha256=a3cbdcfc3f29edbbd07c7c5217d1d299cd3f20bf62ed754269391befbfc19c32, role=review_ui_panel_skin
- materialization/media-binding-media-request-map-panel-rpg-world-key-art: family=map_panel_rpg, slot=world_key_art, kind=image, format=png, path=review-package/media/images/media-binding-media-request-map-panel-rpg-world-key-art.png, sha256=53ae8d809055f1184aaaeb4f33ca70871014a6f3c3464adee9c4fbdd98f0bf38, role=preview_world_key_art
- materialization/media-binding-media-request-survival-sandbox-export-placeholder-bundle: family=survival_sandbox, slot=export_placeholder_bundle, kind=bundle, format=bundle_manifest_json, path=review-package/media/bundles/media-binding-media-request-survival-sandbox-export-placeholder-bundle.json, sha256=cc5f611d1b0f1c7c6151b3811b473e5609fd1d1333a0599195a8f7995618a945, role=export_review_bundle_manifest
- materialization/media-binding-media-request-survival-sandbox-npc-portrait: family=survival_sandbox, slot=npc_portrait, kind=image, format=png, path=review-package/media/images/media-binding-media-request-survival-sandbox-npc-portrait.png, sha256=b48f62efc33557b93b9ed15c7d6c3177ab7278ae43d70deb312e00e40f48b4e8, role=preview_character_focus
- materialization/media-binding-media-request-survival-sandbox-sfx-interaction: family=survival_sandbox, slot=sfx_interaction, kind=audio, format=wav_pcm_s16_mono, path=review-package/media/audio/media-binding-media-request-survival-sandbox-sfx-interaction.wav, sha256=3b5dfd0634ec7c50701344d33b1914d8d7bd44456b83f397147e75bfe7eff3f8, role=preview_interaction_audio_cue
- materialization/media-binding-media-request-survival-sandbox-ui-panel-skin: family=survival_sandbox, slot=ui_panel_skin, kind=ui, format=png, path=review-package/media/ui/media-binding-media-request-survival-sandbox-ui-panel-skin.png, sha256=511877abd6dadbad33e4ff77335988f755b57822987b664687947c99397acea4, role=review_ui_panel_skin
- materialization/media-binding-media-request-survival-sandbox-world-key-art: family=survival_sandbox, slot=world_key_art, kind=image, format=png, path=review-package/media/images/media-binding-media-request-survival-sandbox-world-key-art.png, sha256=5d5642b34a7a92d8ca07a64f65d9ae51914a00c074931c0d033f7292105b409c, role=preview_world_key_art
- materialization/media-binding-media-request-first-person-grid-dungeon-export-placeholder-bundle: family=first_person_grid_dungeon, slot=export_placeholder_bundle, kind=bundle, format=bundle_manifest_json, path=review-package/media/bundles/media-binding-media-request-first-person-grid-dungeon-export-placeholder-bundle.json, sha256=f46e6dea5dcd68acf0f4a8f03f8ee6351f0ce5d3557aca388d4f14cc915f3c7b, role=export_review_bundle_manifest
- materialization/media-binding-media-request-first-person-grid-dungeon-npc-portrait: family=first_person_grid_dungeon, slot=npc_portrait, kind=image, format=png, path=review-package/media/images/media-binding-media-request-first-person-grid-dungeon-npc-portrait.png, sha256=58b3a57f848da37172b09ece95a764e4e5a6bdf083602e781746f969f458789c, role=preview_character_focus
- materialization/media-binding-media-request-first-person-grid-dungeon-sfx-interaction: family=first_person_grid_dungeon, slot=sfx_interaction, kind=audio, format=wav_pcm_s16_mono, path=review-package/media/audio/media-binding-media-request-first-person-grid-dungeon-sfx-interaction.wav, sha256=951e0cd64e9842fe3519466109e9308f160ba0b51cdd40148533ed91f7a21fa7, role=preview_interaction_audio_cue
- materialization/media-binding-media-request-first-person-grid-dungeon-ui-panel-skin: family=first_person_grid_dungeon, slot=ui_panel_skin, kind=ui, format=png, path=review-package/media/ui/media-binding-media-request-first-person-grid-dungeon-ui-panel-skin.png, sha256=5d8c0a4911c6d2e3a219ed8a51fe04fc3ada0701fe86b724578739222ced1a2a, role=review_ui_panel_skin
- materialization/media-binding-media-request-first-person-grid-dungeon-world-key-art: family=first_person_grid_dungeon, slot=world_key_art, kind=image, format=png, path=review-package/media/images/media-binding-media-request-first-person-grid-dungeon-world-key-art.png, sha256=f0edddba9c021ec9be081d8e918a421a23ac4db79c83e8c771e9a98e8c238431, role=preview_world_key_art

## Physical Media Files

- passed: true
- review-package/media/audio/media-binding-media-request-first-person-grid-dungeon-sfx-interaction.wav: format=wav_pcm_s16_mono, bytes=8044, sha256=951e0cd64e9842fe3519466109e9308f160ba0b51cdd40148533ed91f7a21fa7, pngSignature=false, pngCrc=false, wavHeader=true
- review-package/media/audio/media-binding-media-request-map-panel-rpg-sfx-interaction.wav: format=wav_pcm_s16_mono, bytes=8044, sha256=e48e615626ba00a412cbef68bc2da2fab915b737e665ffdf3b410dac6d3246d7, pngSignature=false, pngCrc=false, wavHeader=true
- review-package/media/audio/media-binding-media-request-survival-sandbox-sfx-interaction.wav: format=wav_pcm_s16_mono, bytes=8044, sha256=3b5dfd0634ec7c50701344d33b1914d8d7bd44456b83f397147e75bfe7eff3f8, pngSignature=false, pngCrc=false, wavHeader=true
- review-package/media/bundles/media-binding-media-request-first-person-grid-dungeon-export-placeholder-bundle.json: format=bundle_manifest_json, bytes=602, sha256=f46e6dea5dcd68acf0f4a8f03f8ee6351f0ce5d3557aca388d4f14cc915f3c7b, pngSignature=false, pngCrc=false, wavHeader=false
- review-package/media/bundles/media-binding-media-request-map-panel-rpg-export-placeholder-bundle.json: format=bundle_manifest_json, bytes=542, sha256=c7d669d0fc0d31fe6eb82fe5d8ea79964ac9dfa04d6d53c7924091bbd8298376, pngSignature=false, pngCrc=false, wavHeader=false
- review-package/media/bundles/media-binding-media-request-survival-sandbox-export-placeholder-bundle.json: format=bundle_manifest_json, bytes=557, sha256=cc5f611d1b0f1c7c6151b3811b473e5609fd1d1333a0599195a8f7995618a945, pngSignature=false, pngCrc=false, wavHeader=false
- review-package/media/images/media-binding-media-request-first-person-grid-dungeon-npc-portrait.png: format=png, bytes=3065, sha256=58b3a57f848da37172b09ece95a764e4e5a6bdf083602e781746f969f458789c, pngSignature=true, pngCrc=true, wavHeader=false
- review-package/media/images/media-binding-media-request-first-person-grid-dungeon-world-key-art.png: format=png, bytes=3072, sha256=f0edddba9c021ec9be081d8e918a421a23ac4db79c83e8c771e9a98e8c238431, pngSignature=true, pngCrc=true, wavHeader=false
- review-package/media/images/media-binding-media-request-map-panel-rpg-npc-portrait.png: format=png, bytes=3075, sha256=591d7cbc2da72baf9cc1bf31d4a5ff0ab83c59c27018f64aaf8c9f1ea40a7261, pngSignature=true, pngCrc=true, wavHeader=false
- review-package/media/images/media-binding-media-request-map-panel-rpg-world-key-art.png: format=png, bytes=3075, sha256=53ae8d809055f1184aaaeb4f33ca70871014a6f3c3464adee9c4fbdd98f0bf38, pngSignature=true, pngCrc=true, wavHeader=false
- review-package/media/images/media-binding-media-request-survival-sandbox-npc-portrait.png: format=png, bytes=3093, sha256=b48f62efc33557b93b9ed15c7d6c3177ab7278ae43d70deb312e00e40f48b4e8, pngSignature=true, pngCrc=true, wavHeader=false
- review-package/media/images/media-binding-media-request-survival-sandbox-world-key-art.png: format=png, bytes=3079, sha256=5d5642b34a7a92d8ca07a64f65d9ae51914a00c074931c0d033f7292105b409c, pngSignature=true, pngCrc=true, wavHeader=false
- review-package/media/ui/media-binding-media-request-first-person-grid-dungeon-ui-panel-skin.png: format=png, bytes=3067, sha256=5d8c0a4911c6d2e3a219ed8a51fe04fc3ada0701fe86b724578739222ced1a2a, pngSignature=true, pngCrc=true, wavHeader=false
- review-package/media/ui/media-binding-media-request-map-panel-rpg-ui-panel-skin.png: format=png, bytes=3070, sha256=a3cbdcfc3f29edbbd07c7c5217d1d299cd3f20bf62ed754269391befbfc19c32, pngSignature=true, pngCrc=true, wavHeader=false
- review-package/media/ui/media-binding-media-request-survival-sandbox-ui-panel-skin.png: format=png, bytes=3069, sha256=511877abd6dadbad33e4ff77335988f755b57822987b664687947c99397acea4, pngSignature=true, pngCrc=true, wavHeader=false

## Provenance And License

- passed: true
- fixture-generated-by-repo: decision=materialize_deterministic_fixture_for_review, promoted=true, attributionRequired=false
- manual-user-provided: decision=review_only_until_manual_license_record, promoted=false, attributionRequired=false
- imported-cc0: decision=review_only_not_auto_promoted, promoted=false, attributionRequired=false
- imported-cc-by: decision=review_only_requires_attribution_payload, promoted=false, attributionRequired=true
- imported-share-alike-or-gpl-risk: decision=blocked_license, promoted=false, attributionRequired=false
- provider-generated-with-model-license: decision=blocked_provider_not_configured, promoted=false, attributionRequired=false
- unknown/no-license: decision=blocked_missing_license, promoted=false, attributionRequired=false

## Binding Validation And Payloads

- bindingValidationPassed: true
- everyFamilyHasImageAndAudioFixture: true
- map_panel_rpg: preview=preview-media-payload/map-panel-rpg, export=export-media-payload/map-panel-rpg, mediaRefs=5, validation=passed, included=true, hashSummary=ea74e62192dee53e9e2ce94af1bf7710c644a8b168f560cbc20c272f2a50ddc7
- survival_sandbox: preview=preview-media-payload/survival-sandbox, export=export-media-payload/survival-sandbox, mediaRefs=5, validation=passed, included=true, hashSummary=04b85a5de714de32b21debabbc734e78a0b0d7d4f2f9a6d2b5802b0817eacf29
- first_person_grid_dungeon: preview=preview-media-payload/first-person-grid-dungeon, export=export-media-payload/first-person-grid-dungeon, mediaRefs=5, validation=passed, included=true, hashSummary=b0151eaebfd89f5f1d5dfc124d402b527dad92bc5e1cf27b31eda0790ba4e599

## Review Package

- passed: true
- deterministicHash: 0a9dcd43935a2f0a1d9045a93d0491ca6f04fc95acaf792de198e2141cf91cb3
- map_panel_rpg: mediaFiles=5
- survival_sandbox: mediaFiles=5
- first_person_grid_dungeon: mediaFiles=5

## Family Smoke

- map_panel_rpg: passed=true, files=5, png=2, wav=1, hashSummary=ea74e62192dee53e9e2ce94af1bf7710c644a8b168f560cbc20c272f2a50ddc7
- survival_sandbox: passed=true, files=5, png=2, wav=1, hashSummary=04b85a5de714de32b21debabbc734e78a0b0d7d4f2f9a6d2b5802b0817eacf29
- first_person_grid_dungeon: passed=true, files=5, png=2, wav=1, hashSummary=b0151eaebfd89f5f1d5dfc124d402b527dad92bc5e1cf27b31eda0790ba4e599

## Invalid/fake/leak Matrix

- passed: true
- scenarioCount: 18
- absolute_path_leak: expectedStatus=rejected, actualStatus=rejected, codes=goal054.path.absolute
- cross_family_binding_leak: expectedStatus=rejected, actualStatus=rejected, codes=goal054.binding.cross_family_leak
- fake_binding_id: expectedStatus=rejected, actualStatus=rejected, codes=goal054.binding.fake_id
- fake_media_request_id: expectedStatus=rejected, actualStatus=rejected, codes=goal054.request.fake_id
- gamepackage_schema_mutation_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal054.boundary.gamepackage_schema
- hash_mismatch: expectedStatus=rejected, actualStatus=rejected, codes=goal054.media.hash_mismatch
- imported_provider_candidate_promoted: expectedStatus=blocked, actualStatus=blocked, codes=goal054.provenance.import_or_provider_promoted
- malformed_png_header: expectedStatus=rejected, actualStatus=rejected, codes=goal054.media.png_malformed
- malformed_wav_header: expectedStatus=rejected, actualStatus=rejected, codes=goal054.media.wav_malformed
- media_kind_mismatch: expectedStatus=rejected, actualStatus=rejected, codes=goal054.media.kind_mismatch
- missing_goal053_source: expectedStatus=blocked, actualStatus=blocked, codes=goal054.source.goal053_missing
- missing_physical_media_file: expectedStatus=rejected, actualStatus=rejected, codes=goal054.media.file_missing
- missing_provenance: expectedStatus=rejected, actualStatus=rejected, codes=goal054.provenance.missing
- missing_review_trace: expectedStatus=rejected, actualStatus=rejected, codes=goal054.review.trace_missing
- network_provider_llm_rag_call_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal054.boundary.provider_network_llm_rag
- nondeterministic_ordering: expectedStatus=rejected, actualStatus=rejected, codes=goal054.order.nondeterministic
- runtime_ui_unity_mutation_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal054.boundary.runtime_ui_unity
- unknown_prohibited_license_promoted: expectedStatus=blocked, actualStatus=blocked, codes=goal054.license.unknown_or_prohibited

## Diagnostics

- info: goal054.binding.validation_built [media-binding-validation] Every promoted Goal 053 binding resolves to a physical materialized file with hash, kind and family isolation proof.
- info: goal054.inventory.built [materialized-media-inventory] Physical deterministic PNG/WAV/bundle fixture bytes are inventoried with hashes and header proof.
- info: goal054.license.ledger_built [media-provenance-license-ledger] Only repository-generated deterministic fixture media is materialized; import/provider/unknown license paths remain blocked or review-only.
- info: goal054.preflight.goal053_handoff_recorded [media_asset_campaign_orchestration_verification] Goal 053 is recorded as accepted by user handoff before Goal 054.
- info: goal054.preview_export.payloads_built [preview-export-media-payloads] Media-bound preview/export payload records point to physical review-package media files without mutating package/runtime/Unity payloads.
- info: goal054.queue.built [media-materialization-queue] Materialization queue maps every Goal 053 promoted fixture binding to deterministic physical media bytes.
- info: goal054.source.goal053_report_verified [media-asset-campaign-orchestration-report.md] Goal 053 report remains GREEN produced-for-review evidence with its own gate required.

## Boundaries

No provider/media generation, no network/import/download, no LLM/RAG call, no Lua execution, no GamePackage schema, Runtime, Runtime.Abstractions, WinForms UI, Unity, provider path, generator-library, solution or project file change is required by this Goal 054 proof.

media_materialization_review_package_verification required
