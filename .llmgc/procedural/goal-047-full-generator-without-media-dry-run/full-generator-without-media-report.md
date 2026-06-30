# Full Generator Without Media Dry Run Report

implementationStatus=GREEN
accepted=false
manualGate=full_generator_without_media_verification
familyCount=3
goal043AcceptedByUserHandoff=true
reviewPromotionPassed=true
repairDiagnosticsPassed=true
runtimePreviewValidationPassed=true
exportProfileSelectionPassed=true
packageProofPassed=true
invalidMatrixPassed=true

- implementationStatus: GREEN
- productSmokeRoute: goal-047-full-generator-without-media-dry-run
- goal043AcceptedGate: multi_family_generated_template_vertical_slice_verification passed
- finalGate: full_generator_without_media_verification required
- mediaPolicy: without_media
- providerCalled: false
- mediaGenerated: false
- unityExecuted: false
- runtimeSourceChanged: false
- sourceManifestHash: bbe50583c32aad01506637d6a524a40f69063f39776300e735aa35219971be59
- reviewLedgerHash: 3d95692b9b6e4e679d0c89866d5ddb3d18b2b0808d345ee6dbbf849ff51506ea
- repairMatrixHash: 55a464184670395adf9a6c5c2c565e9c9018450ccb3d026a28c6e7c11b4bcf0f
- runtimePreviewMatrixHash: 3c0ae081597eedb39c02fd1b4919c3b3118080040356307da2cd8212942d8d05
- exportProfileMatrixHash: 305dbc097a982453a4fb5c51b3bf8a7750441990cf380a884980b1bc81094d38
- packageProofHash: a0fae5ed2cfec1ccd6912d72318fd573ef2f5a7eeddd1bc4904c1806022a91f5
- oneClickSummaryHash: 0d9496bd6f5032d5717c1c3a0ecc5587235ee4578485152b349bb0e9734d278c
- invalidMatrixHash: 75e8bd225e14eb18e556b72935aebce5572b627a846931372c4ecd27d39b59aa
- reportHash: a774f7e3d4b5907e447f5503244281d40e8dc5d51b9eea7cab8b2f8cb6cf16c1

## Preflight gates

- multi_family_generated_template_vertical_slice_verification: status=passed, provenance=user_handoff, evidence=Goal 047 starting preflight
- semantic_pack_composition_blueprint_verification: status=produced_for_review_not_passed, provenance=inherited, evidence=Goal 031 preserved policy
- dynamic_semantic_feature_system_verification: status=produced_for_review_not_passed, provenance=inherited, evidence=Goal 032 preserved policy
- full_generator_without_media_verification: status=required, provenance=programmatic, evidence=Goal 047 produced for review

## Source manifest

- sourceArtifactRefs: 54
- selectedFamilies: map_panel_rpg,survival_sandbox,first_person_grid_dungeon
- selectedTemplateLoopRefs: 11
- selectedDraftLuaExpansionRefs: 24
- selectedWorldChunkRuntimeRefs: 16

## Review and promotion

- passed: true
- transitionCount: 12
- review/map_panel_rpg/001-validated: candidate_loaded->validated, decision=validated_for_dry_run, provenance=programmatic
- review/map_panel_rpg/002-approved_for_dry_run: validated->approved_for_dry_run, decision=approved_for_goal047_dry_run, provenance=user_handoff
- review/map_panel_rpg/003-promoted_to_preview_payload: approved_for_dry_run->promoted_to_preview_payload, decision=promoted_to_runtime_preview_payload, provenance=inherited
- review/map_panel_rpg/004-promoted_to_export_candidate: promoted_to_preview_payload->promoted_to_export_candidate, decision=promoted_to_export_candidate_without_media, provenance=programmatic
- review/survival_sandbox/001-validated: candidate_loaded->validated, decision=validated_for_dry_run, provenance=programmatic
- review/survival_sandbox/002-approved_for_dry_run: validated->approved_for_dry_run, decision=approved_for_goal047_dry_run, provenance=user_handoff
- review/survival_sandbox/003-promoted_to_preview_payload: approved_for_dry_run->promoted_to_preview_payload, decision=promoted_to_runtime_preview_payload, provenance=inherited
- review/survival_sandbox/004-promoted_to_export_candidate: promoted_to_preview_payload->promoted_to_export_candidate, decision=promoted_to_export_candidate_without_media, provenance=programmatic
- review/first_person_grid_dungeon/001-validated: candidate_loaded->validated, decision=validated_for_dry_run, provenance=programmatic
- review/first_person_grid_dungeon/002-approved_for_dry_run: validated->approved_for_dry_run, decision=approved_for_goal047_dry_run, provenance=user_handoff
- review/first_person_grid_dungeon/003-promoted_to_preview_payload: approved_for_dry_run->promoted_to_preview_payload, decision=promoted_to_runtime_preview_payload, provenance=inherited
- review/first_person_grid_dungeon/004-promoted_to_export_candidate: promoted_to_preview_payload->promoted_to_export_candidate, decision=promoted_to_export_candidate_without_media, provenance=programmatic

## Repair diagnostics

- passed: true
- diagnosticCount: 14
- manualRequiredCount: 9
- cross_family_leakage: decision=bounded_repair_available, action=repair_family_scope
- final_prose_leakage: decision=bounded_repair_available, action=reject_and_strip_candidate
- gamepackage_schema_mutation_claim: decision=manual_required, action=manual_required_blocked_boundary
- hash_mismatch: decision=manual_required, action=bounded_restore_verified_source
- media_leakage: decision=manual_required, action=manual_required_blocked_boundary
- missing_export_profile: decision=bounded_repair_available, action=select_deterministic_profile
- missing_family_loop: decision=manual_required, action=bounded_goal043_regeneration
- missing_runtime_preview_payload: decision=manual_required, action=bounded_goal040_regeneration
- missing_source_artifact: decision=manual_required, action=bounded_restore_or_rerun_source_goal
- nondeterministic_ordering: decision=bounded_repair_available, action=sort_by_deterministic_key
- provider_llm_rag_leakage: decision=manual_required, action=manual_required_blocked_boundary
- rejected_candidate_provenance: decision=manual_required, action=manual_review_required
- unity_runtime_source_mutation_claim: decision=manual_required, action=manual_required_blocked_boundary
- unresolved_profile_capability_ref: decision=bounded_repair_available, action=repair_ref_from_manifest

## Family dry-runs

- map_panel_rpg: scenario=gothic_intrigue, profile=gothic_intrigue, systems=10, stateChangingLoop=true, replayHashPassed=true, payload=.llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/chunked-preview-payload-gothic.json, exportProfile=export-profile/map-panel-rpg/without-media
- survival_sandbox: scenario=frontier_survival, profile=frontier_survival, systems=10, stateChangingLoop=true, replayHashPassed=true, payload=.llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/chunked-preview-payload-frontier.json, exportProfile=export-profile/survival-sandbox/without-media
- first_person_grid_dungeon: scenario=metamodule_kingdoms, profile=metamodule_kingdoms, systems=10, stateChangingLoop=true, replayHashPassed=true, payload=.llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/chunked-preview-payload-metamodule.json, exportProfile=export-profile/first-person-grid-dungeon/without-media

## Runtime preview validation

- passed: true
- map_panel_rpg: stableRefs=true, sourceHashesMatch=true, commandStateTransitionsConsistent=true, passed=true
- survival_sandbox: stableRefs=true, sourceHashesMatch=true, commandStateTransitionsConsistent=true, passed=true
- first_person_grid_dungeon: stableRefs=true, sourceHashesMatch=true, commandStateTransitionsConsistent=true, passed=true

## Export profile selection

- passed: true
- map_panel_rpg: profile=export-profile/map-panel-rpg/without-media, presentation=map_panel_runtime_preview, withoutMedia=true, passed=true
- survival_sandbox: profile=export-profile/survival-sandbox/without-media, presentation=survival_sandbox_runtime_preview, withoutMedia=true, passed=true
- first_person_grid_dungeon: profile=export-profile/first-person-grid-dungeon/without-media, presentation=first_person_grid_runtime_preview, withoutMedia=true, passed=true

## Package proof

- proofMode: strict_package_compatibility_proof
- packageMaterializationAttempted: false
- materializedValidatorCleanPackages: false
- compatibilityProofPassed: true
- directMaterializationSafetyDecision: direct_materialization_not_attempted: Goal 047 family dry-run records are review/preview/export candidates, not accepted GeneratorPlanApprovedArtifactSet content. Creating a new materializer would be a new adapter beyond this goal; strict compatibility proof maps selected outputs to existing package assembly targets instead.
- map_panel_rpg/world: status=compatible_existing_assembler, target=GamePackage.Game.Maps + GeneratedContent.Regions, directMaterializationSafeNow=true
- map_panel_rpg/entity: status=compatible_existing_assembler, target=GamePackage.Game.EntityPrototypes + map placements, directMaterializationSafeNow=true
- map_panel_rpg/quest: status=compatible_existing_assembler, target=GamePackage.Game.Quests, directMaterializationSafeNow=true
- map_panel_rpg/dialogue: status=compatible_existing_assembler, target=GamePackage.Game.Dialogues, directMaterializationSafeNow=true
- map_panel_rpg/item: status=compatible_existing_assembler, target=GamePackage.Game.Items + LootTables, directMaterializationSafeNow=true
- map_panel_rpg/economy: status=compatible_existing_assembler, target=GamePackage.Game.Resources + Recipes + Transactions, directMaterializationSafeNow=true
- map_panel_rpg/combat: status=compatible_existing_assembler, target=GamePackage.Game.Encounters + Abilities + Statuses, directMaterializationSafeNow=true
- map_panel_rpg/progression: status=compatible_existing_assembler, target=GamePackage.Game.Progressions + progression stages, directMaterializationSafeNow=true
- map_panel_rpg/settlement: status=compatible_existing_metadata_or_future_required, target=GeneratedContent metadata or future settlement-specific pack, directMaterializationSafeNow=false
- map_panel_rpg/event: status=compatible_existing_assembler, target=GamePackage.Game.Quests/Objectives + GeneratedContent events, directMaterializationSafeNow=true
- survival_sandbox/world: status=compatible_existing_assembler, target=GamePackage.Game.Maps + GeneratedContent.Regions, directMaterializationSafeNow=true
- survival_sandbox/entity: status=compatible_existing_assembler, target=GamePackage.Game.EntityPrototypes + map placements, directMaterializationSafeNow=true
- survival_sandbox/quest: status=compatible_existing_assembler, target=GamePackage.Game.Quests, directMaterializationSafeNow=true
- survival_sandbox/dialogue: status=compatible_existing_assembler, target=GamePackage.Game.Dialogues, directMaterializationSafeNow=true
- survival_sandbox/item: status=compatible_existing_assembler, target=GamePackage.Game.Items + LootTables, directMaterializationSafeNow=true
- survival_sandbox/economy: status=compatible_existing_assembler, target=GamePackage.Game.Resources + Recipes + Transactions, directMaterializationSafeNow=true
- survival_sandbox/combat: status=compatible_existing_assembler, target=GamePackage.Game.Encounters + Abilities + Statuses, directMaterializationSafeNow=true
- survival_sandbox/progression: status=compatible_existing_assembler, target=GamePackage.Game.Progressions + progression stages, directMaterializationSafeNow=true
- survival_sandbox/settlement: status=compatible_existing_metadata_or_future_required, target=GeneratedContent metadata or future settlement-specific pack, directMaterializationSafeNow=false
- survival_sandbox/event: status=compatible_existing_assembler, target=GamePackage.Game.Quests/Objectives + GeneratedContent events, directMaterializationSafeNow=true
- first_person_grid_dungeon/world: status=compatible_existing_assembler, target=GamePackage.Game.Maps + GeneratedContent.Regions, directMaterializationSafeNow=true
- first_person_grid_dungeon/entity: status=compatible_existing_assembler, target=GamePackage.Game.EntityPrototypes + map placements, directMaterializationSafeNow=true
- first_person_grid_dungeon/quest: status=compatible_existing_assembler, target=GamePackage.Game.Quests, directMaterializationSafeNow=true
- first_person_grid_dungeon/dialogue: status=compatible_existing_assembler, target=GamePackage.Game.Dialogues, directMaterializationSafeNow=true
- first_person_grid_dungeon/item: status=compatible_existing_assembler, target=GamePackage.Game.Items + LootTables, directMaterializationSafeNow=true
- first_person_grid_dungeon/economy: status=compatible_existing_assembler, target=GamePackage.Game.Resources + Recipes + Transactions, directMaterializationSafeNow=true
- first_person_grid_dungeon/combat: status=compatible_existing_assembler, target=GamePackage.Game.Encounters + Abilities + Statuses, directMaterializationSafeNow=true
- first_person_grid_dungeon/progression: status=compatible_existing_assembler, target=GamePackage.Game.Progressions + progression stages, directMaterializationSafeNow=true
- first_person_grid_dungeon/settlement: status=compatible_existing_metadata_or_future_required, target=GeneratedContent metadata or future settlement-specific pack, directMaterializationSafeNow=false
- first_person_grid_dungeon/event: status=compatible_existing_assembler, target=GamePackage.Game.Quests/Objectives + GeneratedContent events, directMaterializationSafeNow=true

## One-click dry-run proof

- status: GREEN
- evidenceFileCount: 12
- deterministicHash: 5682695f3065a195df339d60a0790e1ac7c83e44c1d58b3c5f505ef17a32dcd4
- evidenceFile: dry-run-source-manifest.json
- evidenceFile: export-profile-selection-matrix.json
- evidenceFile: family-first-person-grid-dungeon-dry-run.json
- evidenceFile: family-map-panel-rpg-dry-run.json
- evidenceFile: family-survival-sandbox-dry-run.json
- evidenceFile: full-generator-without-media-report.md
- evidenceFile: invalid-fake-leak-matrix.json
- evidenceFile: one-click-dry-run-summary.json
- evidenceFile: package-compatibility-or-materialization-summary.json
- evidenceFile: repair-diagnostics-matrix.json
- evidenceFile: review-promotion-ledger.json
- evidenceFile: runtime-preview-validation-matrix.json

## Invalid/fake/leak matrix

- passed: true
- scenarioCount: 17
- cross_family_source_leakage: expectedStatus=rejected, actualStatus=rejected, codes=goal047.family.cross_leakage
- duplicate_promotion_transition_id: expectedStatus=rejected, actualStatus=rejected, codes=goal047.review.transition_id.duplicate
- fake_family_id: expectedStatus=rejected, actualStatus=rejected, codes=goal047.family.fake
- final_prose_promoted_as_content: expectedStatus=rejected, actualStatus=rejected, codes=goal047.boundary.final_prose
- gamepackage_schema_mutation_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal047.boundary.gamepackage_schema
- hash_mismatch: expectedStatus=rejected, actualStatus=rejected, codes=goal047.source.hash_mismatch
- invalid_transition_order: expectedStatus=rejected, actualStatus=rejected, codes=goal047.review.transition_order.invalid
- media_generated_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal047.boundary.media
- missing_goal043_source: expectedStatus=rejected, actualStatus=rejected, codes=goal047.source.goal043_missing
- missing_repair_action: expectedStatus=rejected, actualStatus=rejected, codes=goal047.repair.action_missing
- missing_state_changing_loop: expectedStatus=rejected, actualStatus=rejected, codes=goal047.family.state_changing_loop_missing
- nondeterministic_ordering: expectedStatus=rejected, actualStatus=rejected, codes=goal047.order.nondeterministic
- provider_llm_rag_call_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal047.boundary.provider_llm_rag
- runtime_source_changed_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal047.boundary.runtime_source
- unity_executed_claim: expectedStatus=blocked, actualStatus=blocked, codes=goal047.boundary.unity
- unsafe_absolute_path: expectedStatus=rejected, actualStatus=rejected, codes=goal047.source.relative_path.invalid
- wrong_accepted_gate: expectedStatus=rejected, actualStatus=rejected, codes=goal047.preflight.goal043_handoff_missing

## Boundaries

No public GamePackage schema, Runtime, Runtime.Abstractions, WinForms UI, Unity, provider/LLM/RAG/media path, generator-library, sample/template, solution/project file, external dependency or arbitrary Lua execution change is required by this Goal 047 evidence.

full_generator_without_media_verification required
