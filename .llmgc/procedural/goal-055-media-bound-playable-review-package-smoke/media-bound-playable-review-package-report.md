# Media-Bound Playable Review Package Report

media_bound_playable_review_package_verification required
implementationStatus: GREEN
implementationStatus=GREEN
manualGate=media_bound_playable_review_package_verification
accepted=false
Goal054AcceptedByUserHandoff: true
goal054AcceptedByUserHandoff=true
providerCalls=false
networkImports=false
llmCalls=false
luaExecuted=false
publicGamePackageSchemaChanged=false
unitySourceChanged=false
unityBuildOrPlayerExecuted=false
familyCount=3
stagedFileCount=15
pngFileCount=9
wavFileCount=3
bundleJsonFileCount=3
physicalMediaStaged=true
pngProofPassed=true
wavProofPassed=true
bundleProofPassed=true
reviewPackageManifestPassed=true
streamingAssetsManifestPassed=true
previewPayloadsPassed=true
unityMediaLoadContractPassed=true
familySmokeMatrixPassed=true
invalidMatrixPassed=true
sourceManifestHash=7250137898bacf6adfcdd6ff0dd0a54520d4beef983491393c8ebc3b2038875e
reviewPackageManifestHash=6529c9df47489010c6c1396299382d1e99fa91fe5f9ce4695b2e8883a5c30901
streamingAssetsManifestHash=4b5358a21cc901d1f5600cf047348af11c5b292ddc18aef51d11cd99ba5b6aa0
previewPayloadsHash=3800806f600cc01a58deb8b6b8167e9f942ce4cf4630013f0cd3a8ee3e982a51
unityLoadContractHash=b85047d260328846d299223c6ce80950b833fb6636a5d440e5c2cbe625e16a96
familySmokeMatrixHash=8f393b3553d11ca90a053bbd1895ba67938de5f7189f07cce5c3d4c370be188c
invalidMatrixHash=6817b9aefe5a14c5094ae67df8281e22e2bcb0c77f45ff4e035461f045af958b
reportHash=450ece061878822f3200cdcd49581302231233ae189279e3e8316fa26002a13e

## Preflight

- media_materialization_review_package_verification: status=passed, provenance=user_handoff, evidence=Goal 055 task preflight handoff
- semantic_pack_composition_blueprint_verification: status=produced_for_review_not_passed, provenance=inherited, evidence=Goal 031 preserved policy
- dynamic_semantic_feature_system_verification: status=produced_for_review_not_passed, provenance=inherited, evidence=Goal 032 preserved policy
- media_bound_playable_review_package_verification: status=required, provenance=programmatic, evidence=Goal 055 produced for review

## Source Facts

- sourceArtifactRefCount: 14
- goal047FamilyDryRunCount: 3
- goal053BindingCount: 15
- goal054PhysicalMediaCount: 15
- map_panel_rpg: scenario=gothic_intrigue, png=3, wav=1, bundle=1, dryRun=.llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-map-panel-rpg-dry-run.json, goal054Payload=preview-media-payload/map-panel-rpg
- survival_sandbox: scenario=frontier_survival, png=3, wav=1, bundle=1, dryRun=.llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-survival-sandbox-dry-run.json, goal054Payload=preview-media-payload/survival-sandbox
- first_person_grid_dungeon: scenario=metamodule_kingdoms, png=3, wav=1, bundle=1, dryRun=.llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-first-person-grid-dungeon-dry-run.json, goal054Payload=preview-media-payload/first-person-grid-dungeon

## Physical Staging

- reviewPackagePassed: true
- map_panel_rpg/world_key_art: kind=image, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/map-panel-rpg/world-key-art-image-53ae8d80.png, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/images/media-binding-media-request-map-panel-rpg-world-key-art.png, sha256=53ae8d809055f1184aaaeb4f33ca70871014a6f3c3464adee9c4fbdd98f0bf38, bytes=3075, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes
- map_panel_rpg/npc_portrait: kind=image, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/map-panel-rpg/npc-portrait-image-591d7cbc.png, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/images/media-binding-media-request-map-panel-rpg-npc-portrait.png, sha256=591d7cbc2da72baf9cc1bf31d4a5ff0ab83c59c27018f64aaf8c9f1ea40a7261, bytes=3075, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes
- map_panel_rpg/ui_panel_skin: kind=ui, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/map-panel-rpg/ui-panel-skin-ui-a3cbdcfc.png, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/ui/media-binding-media-request-map-panel-rpg-ui-panel-skin.png, sha256=a3cbdcfc3f29edbbd07c7c5217d1d299cd3f20bf62ed754269391befbfc19c32, bytes=3070, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes
- map_panel_rpg/sfx_interaction: kind=audio, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/map-panel-rpg/sfx-interaction-audio-e48e6156.wav, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/audio/media-binding-media-request-map-panel-rpg-sfx-interaction.wav, sha256=e48e615626ba00a412cbef68bc2da2fab915b737e665ffdf3b410dac6d3246d7, bytes=8044, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes
- map_panel_rpg/export_placeholder_bundle: kind=bundle, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/map-panel-rpg/export-placeholder-bundle-bundle-c7d669d0.json, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/bundles/media-binding-media-request-map-panel-rpg-export-placeholder-bundle.json, sha256=c7d669d0fc0d31fe6eb82fe5d8ea79964ac9dfa04d6d53c7924091bbd8298376, bytes=542, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes
- survival_sandbox/world_key_art: kind=image, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/survival-sandbox/world-key-art-image-5d5642b3.png, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/images/media-binding-media-request-survival-sandbox-world-key-art.png, sha256=5d5642b34a7a92d8ca07a64f65d9ae51914a00c074931c0d033f7292105b409c, bytes=3079, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes
- survival_sandbox/npc_portrait: kind=image, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/survival-sandbox/npc-portrait-image-b48f62ef.png, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/images/media-binding-media-request-survival-sandbox-npc-portrait.png, sha256=b48f62efc33557b93b9ed15c7d6c3177ab7278ae43d70deb312e00e40f48b4e8, bytes=3093, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes
- survival_sandbox/ui_panel_skin: kind=ui, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/survival-sandbox/ui-panel-skin-ui-511877ab.png, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/ui/media-binding-media-request-survival-sandbox-ui-panel-skin.png, sha256=511877abd6dadbad33e4ff77335988f755b57822987b664687947c99397acea4, bytes=3069, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes
- survival_sandbox/sfx_interaction: kind=audio, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/survival-sandbox/sfx-interaction-audio-3b5dfd06.wav, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/audio/media-binding-media-request-survival-sandbox-sfx-interaction.wav, sha256=3b5dfd0634ec7c50701344d33b1914d8d7bd44456b83f397147e75bfe7eff3f8, bytes=8044, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes
- survival_sandbox/export_placeholder_bundle: kind=bundle, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/survival-sandbox/export-placeholder-bundle-bundle-cc5f611d.json, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/bundles/media-binding-media-request-survival-sandbox-export-placeholder-bundle.json, sha256=cc5f611d1b0f1c7c6151b3811b473e5609fd1d1333a0599195a8f7995618a945, bytes=557, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes
- first_person_grid_dungeon/world_key_art: kind=image, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/first-person-grid-dungeon/world-key-art-image-f0edddba.png, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/images/media-binding-media-request-first-person-grid-dungeon-world-key-art.png, sha256=f0edddba9c021ec9be081d8e918a421a23ac4db79c83e8c771e9a98e8c238431, bytes=3072, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes
- first_person_grid_dungeon/npc_portrait: kind=image, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/first-person-grid-dungeon/npc-portrait-image-58b3a57f.png, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/images/media-binding-media-request-first-person-grid-dungeon-npc-portrait.png, sha256=58b3a57f848da37172b09ece95a764e4e5a6bdf083602e781746f969f458789c, bytes=3065, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes
- first_person_grid_dungeon/ui_panel_skin: kind=ui, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/first-person-grid-dungeon/ui-panel-skin-ui-5d8c0a49.png, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/ui/media-binding-media-request-first-person-grid-dungeon-ui-panel-skin.png, sha256=5d8c0a4911c6d2e3a219ed8a51fe04fc3ada0701fe86b724578739222ced1a2a, bytes=3067, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes
- first_person_grid_dungeon/sfx_interaction: kind=audio, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/first-person-grid-dungeon/sfx-interaction-audio-951e0cd6.wav, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/audio/media-binding-media-request-first-person-grid-dungeon-sfx-interaction.wav, sha256=951e0cd64e9842fe3519466109e9308f160ba0b51cdd40148533ed91f7a21fa7, bytes=8044, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes
- first_person_grid_dungeon/export_placeholder_bundle: kind=bundle, staged=review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media/first-person-grid-dungeon/export-placeholder-bundle-bundle-f46e6dea.json, source=.llmgc/procedural/goal-054-media-materialization-review-package/review-package/media/bundles/media-binding-media-request-first-person-grid-dungeon-export-placeholder-bundle.json, sha256=f46e6dea5dcd68acf0f4a8f03f8ee6351f0ce5d3557aca388d4f14cc915f3c7b, bytes=602, license=repo_fixture_no_external_license, provenance=repository_generated_deterministic_bytes

## StreamingAssets Manifest

- passed: true
- manifest: review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media-bound-playable-manifest.json
- bindingCount: 15

## Preview/Export Payloads

- passed: true
- map_panel_rpg: preview=media-bound-preview/map-panel-rpg, export=media-bound-export/map-panel-rpg, dryRun=.llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-map-panel-rpg-dry-run.json, stagedRefs=5, proof=unity-media-load-proof-map-panel-rpg.json, hash=ea74e62192dee53e9e2ce94af1bf7710c644a8b168f560cbc20c272f2a50ddc7
- survival_sandbox: preview=media-bound-preview/survival-sandbox, export=media-bound-export/survival-sandbox, dryRun=.llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-survival-sandbox-dry-run.json, stagedRefs=5, proof=unity-media-load-proof-survival-sandbox.json, hash=04b85a5de714de32b21debabbc734e78a0b0d7d4f2f9a6d2b5802b0817eacf29
- first_person_grid_dungeon: preview=media-bound-preview/first-person-grid-dungeon, export=media-bound-export/first-person-grid-dungeon, dryRun=.llmgc/procedural/goal-047-full-generator-without-media-dry-run/family-first-person-grid-dungeon-dry-run.json, stagedRefs=5, proof=unity-media-load-proof-first-person-grid-dungeon.json, hash=b0151eaebfd89f5f1d5dfc124d402b527dad92bc5e1cf27b31eda0790ba4e599

## Unity-Compatible Media Load Proof

- contractPassed: true
- readSurface: Application.streamingAssetsPath
- imageLoadApi: UnityEngine.ImageConversion.LoadImage
- wavValidationMode: bcl_pcm_wav_header_and_data_validation_no_playback_claim
- unitySourceChanged: false
- unityBuildOrPlayerExecuted: false
- first_person_grid_dungeon: passed=true, lines=6
  - MEDIA_BOUND_MANIFEST_LOADED family=first_person_grid_dungeon
  - MEDIA_BOUND_IMAGE_LOADED family=first_person_grid_dungeon slot=world_key_art width=32 height=32 sha256=f0edddba9c021ec9be081d8e918a421a23ac4db79c83e8c771e9a98e8c238431
  - MEDIA_BOUND_IMAGE_LOADED family=first_person_grid_dungeon slot=npc_portrait width=32 height=32 sha256=58b3a57f848da37172b09ece95a764e4e5a6bdf083602e781746f969f458789c
  - MEDIA_BOUND_IMAGE_LOADED family=first_person_grid_dungeon slot=ui_panel_skin width=32 height=32 sha256=5d8c0a4911c6d2e3a219ed8a51fe04fc3ada0701fe86b724578739222ced1a2a
  - MEDIA_BOUND_WAV_VALIDATED family=first_person_grid_dungeon slot=sfx_interaction sampleRate=16000 channels=1 sampleCount=4000 sha256=951e0cd64e9842fe3519466109e9308f160ba0b51cdd40148533ed91f7a21fa7
  - MEDIA_BOUND_FAMILY_PANEL_READY family=first_person_grid_dungeon
- map_panel_rpg: passed=true, lines=6
  - MEDIA_BOUND_MANIFEST_LOADED family=map_panel_rpg
  - MEDIA_BOUND_IMAGE_LOADED family=map_panel_rpg slot=world_key_art width=32 height=32 sha256=53ae8d809055f1184aaaeb4f33ca70871014a6f3c3464adee9c4fbdd98f0bf38
  - MEDIA_BOUND_IMAGE_LOADED family=map_panel_rpg slot=npc_portrait width=32 height=32 sha256=591d7cbc2da72baf9cc1bf31d4a5ff0ab83c59c27018f64aaf8c9f1ea40a7261
  - MEDIA_BOUND_IMAGE_LOADED family=map_panel_rpg slot=ui_panel_skin width=32 height=32 sha256=a3cbdcfc3f29edbbd07c7c5217d1d299cd3f20bf62ed754269391befbfc19c32
  - MEDIA_BOUND_WAV_VALIDATED family=map_panel_rpg slot=sfx_interaction sampleRate=16000 channels=1 sampleCount=4000 sha256=e48e615626ba00a412cbef68bc2da2fab915b737e665ffdf3b410dac6d3246d7
  - MEDIA_BOUND_FAMILY_PANEL_READY family=map_panel_rpg
- survival_sandbox: passed=true, lines=6
  - MEDIA_BOUND_MANIFEST_LOADED family=survival_sandbox
  - MEDIA_BOUND_IMAGE_LOADED family=survival_sandbox slot=world_key_art width=32 height=32 sha256=5d5642b34a7a92d8ca07a64f65d9ae51914a00c074931c0d033f7292105b409c
  - MEDIA_BOUND_IMAGE_LOADED family=survival_sandbox slot=npc_portrait width=32 height=32 sha256=b48f62efc33557b93b9ed15c7d6c3177ab7278ae43d70deb312e00e40f48b4e8
  - MEDIA_BOUND_IMAGE_LOADED family=survival_sandbox slot=ui_panel_skin width=32 height=32 sha256=511877abd6dadbad33e4ff77335988f755b57822987b664687947c99397acea4
  - MEDIA_BOUND_WAV_VALIDATED family=survival_sandbox slot=sfx_interaction sampleRate=16000 channels=1 sampleCount=4000 sha256=3b5dfd0634ec7c50701344d33b1914d8d7bd44456b83f397147e75bfe7eff3f8
  - MEDIA_BOUND_FAMILY_PANEL_READY family=survival_sandbox

## Family Smoke

- passed: true
- map_panel_rpg: passed=true, files=5, png=3, wav=1, bundle=1, manifest=true, preview=true, unityProof=true
- survival_sandbox: passed=true, files=5, png=3, wav=1, bundle=1, manifest=true, preview=true, unityProof=true
- first_person_grid_dungeon: passed=true, files=5, png=3, wav=1, bundle=1, manifest=true, preview=true, unityProof=true

## Invalid/fake/leak Matrix

- passed: true
- scenarioCount: 17
- duplicate_binding_id: expectedStatus=rejected, actualStatus=rejected, codes=goal055.binding.duplicate_id
- fake_family_id: expectedStatus=rejected, actualStatus=rejected, codes=goal055.family.fake_id
- fake_slot_id: expectedStatus=rejected, actualStatus=rejected, codes=goal055.slot.fake_id
- fake_unity_proof_line: expectedStatus=rejected, actualStatus=rejected, codes=goal055.unity.fake_proof_line
- license_provenance_blocked_promoted: expectedStatus=blocked, actualStatus=blocked, codes=goal055.license.blocked_promoted
- lua_execution_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal055.boundary.lua_execution
- malformed_png: expectedStatus=rejected, actualStatus=rejected, codes=goal055.media.png_malformed
- malformed_wav: expectedStatus=rejected, actualStatus=rejected, codes=goal055.media.wav_malformed
- missing_goal054_source: expectedStatus=blocked, actualStatus=blocked, codes=goal055.source.goal054_missing
- missing_review_trace: expectedStatus=rejected, actualStatus=rejected, codes=goal055.review.trace_missing
- missing_staged_file: expectedStatus=rejected, actualStatus=rejected, codes=goal055.stage.file_missing
- nondeterministic_ordering: expectedStatus=rejected, actualStatus=rejected, codes=goal055.order.nondeterministic
- provider_network_llm_rag_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal055.boundary.provider_network_llm_rag
- runtime_ui_gamepackage_schema_mutation_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal055.boundary.runtime_ui_gamepackage
- stale_hash: expectedStatus=rejected, actualStatus=rejected, codes=goal055.stage.hash_mismatch
- unity_broad_mutation_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal055.boundary.unity_broad_mutation
- unsafe_relative_path: expectedStatus=rejected, actualStatus=rejected, codes=goal055.path.unsafe

## Diagnostics

- info: goal055.preflight.goal054_handoff_recorded [media_materialization_review_package_verification] Goal 054 is recorded as accepted by user handoff before Goal 055.
- info: goal055.source.physical_media_loaded [Goal054] Goal 054 physical PNG/WAV/bundle media bytes were loaded from repository-local evidence.
- info: goal055.unity.contract.application_level [unity-media-load-contract] Unity-compatible proof is produced by BCL Application validation without changing Unity source or claiming player execution.
- info: goal055.unity.proof.application_level [first_person_grid_dungeon] Proof lines are deterministic Application-level records compatible with a Unity StreamingAssets loader.
- info: goal055.unity.proof.application_level [map_panel_rpg] Proof lines are deterministic Application-level records compatible with a Unity StreamingAssets loader.
- info: goal055.unity.proof.application_level [survival_sandbox] Proof lines are deterministic Application-level records compatible with a Unity StreamingAssets loader.

## Boundaries

No provider/media generation, no network/import/download, no LLM/RAG call, no Lua execution, no public GamePackage schema, Runtime, Runtime.Abstractions, WinForms UI, provider path, generator-library, solution or project file change is required by this Goal 055 proof. Unity source/build/player execution is not claimed; the proof is a deterministic Application-level StreamingAssets-compatible contract over staged physical Goal 054 bytes.

media_bound_playable_review_package_verification required
