# Constrained Spatial Detail Generation Report

constrained_spatial_detail_generation_verification required
implementationStatus=GREEN
accepted=false
manualGate=constrained_spatial_detail_generation_verification
goal061AcceptedByUserHandoff=true
sourceFactsConsumed=true
rowCount=9
familyCount=3
seedCount=3
distinctRowHashCount=9
paletteCatalogPassed=true
rewriteRuleCatalogPassed=true
constraintRuleCatalogPassed=true
spatialDetailMatrixPassed=true
reachabilityProofPassed=true
repairFallbackMatrixPassed=true
previewExportPayloadPassed=true
invalidMatrixPassed=true
unityEditorOrPlayerExecuted=true
unityExitCode=0
playerExitCode=0
allUnitySpatialMarkersMatched=true
unityProvenRowCount=9
sourceManifestHash=79dee0d360f0040cba736199b10cbd0238d6a33d4745fcc312e230b5e4cc30b6
paletteCatalogHash=9ddf5bd19073dc5e391b05fd18ee6b2b1e64fec12cbc1ec86f4350e84e367d2f
rewriteRuleCatalogHash=74751c43147f07e556c75c694e679e32aafc4a84ddbd8434e200546c0543c6aa
constraintRuleCatalogHash=57423b2b1982cbef42a0101709ce14727d2649a75b653ae006190f13f8e76fa9
spatialDetailMatrixHash=47d92864c9bc35955030b6238764ab28a33ebe93615fcf9303559dab5b65bc54
reachabilityProofMatrixHash=ec3bbbb18ddea1aebc22591fb06c2a4166e13fbef8d3b90223d5370795f7f2ad
repairFallbackMatrixHash=41979396cefe51ac00430e0363b38b4f8d000ed2bf2dcede71136551880bee3c
unityCommandPlanHash=9fc49cd60dd0ca44182fe0e23bf2face8ffb41e8cbba0556a6f6c1a04afcfb2c
unityProofSummaryHash=a58205dc9291f683f5feea8e22521f6ac31e8101be906f34cc0d97a476262972
previewExportPayloadHash=d89a0b9c08b400fafc7e1d83d4e0bf557f123d473b07cc9d0695d794e61c794b
invalidMatrixHash=008af4ec6591896db6bf763ef0d5f6d96cc0e49d41bae30f7ccbcd42009185a8
reportHash=9e92a11c3caefd4901872a575d08cd1c8190ec01beb19e73964b97cd3a08b839

## Preflight

- full_campaign_playable_review_package_rc_verification: status=passed, provenance=user_handoff, evidence=Goal 062 task preflight handoff
- semantic_pack_composition_blueprint_verification: status=produced_for_review_not_passed, provenance=inherited, evidence=Goal 031 preserved policy
- dynamic_semantic_feature_system_verification: status=produced_for_review_not_passed, provenance=inherited, evidence=Goal 032 preserved policy
- constrained_spatial_detail_generation_verification: status=required, provenance=programmatic, evidence=Goal 062 produced for review

## Source Chain

- goal061ReviewPackageRcManifestPassed: true
- goal061UnityProofPassed: true
- goal060PackageInventoryConsumed: true
- goal059VarianceConsumed: true
- seed_profile_matrix: artifact=.llmgc/procedural/goal-059-full-generator-variability-regression-matrix/seed-profile-matrix.json, exists=true, hashMatches=true, hash=753f82df1136444032dbc54a1b265d1de57a609fe779c445083076761570a66a
- variance_metrics: artifact=.llmgc/procedural/goal-059-full-generator-variability-regression-matrix/variance-metrics.json, exists=true, hashMatches=true, hash=a81634cc30081c9eca51fda155bec70f87966e6b7b37523fdf47f0f2261f4712
- package_inventory: artifact=.llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix/materialized-package-inventory.json, exists=true, hashMatches=true, hash=1f41f895f3b1953cdf61acf9740c5c5f4aa5640a1c733f6373bcb8dd5729c8f7
- package_validation_matrix: artifact=.llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix/package-validation-matrix.json, exists=true, hashMatches=true, hash=68b0be48f1c2d381a9024e462d97826607b03d8de46aa468170744252bb2d52e
- package_row_selection_matrix: artifact=.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/package-row-selection-matrix.json, exists=true, hashMatches=true, hash=fe64ef86201daef85f03d6556f2e3d417241341ccf647559f768d4bbec1a0116
- review_package_rc_manifest: artifact=.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/review-package-rc-manifest.json, exists=true, hashMatches=true, hash=839d3d6b99ac19ce8b2e88ae4d765632f25f3d3a314850076d2d91a7a8fe4c99
- source_manifest: artifact=.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/source-manifest.json, exists=true, hashMatches=true, hash=4b7c688e9903726bc28ffc53aa3887b97c71cb025ed5f1afe133962643126b3f
- unity_review_package_command_plan: artifact=.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/unity-player-command-plan.json, exists=true, hashMatches=true, hash=864448d91b1d846f6cac7b01f24dc76ae3869603716ad0f0bf6914a6953afea3
- unity_review_package_proof: artifact=.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/unity-player-proof-matrix.json, exists=true, hashMatches=true, hash=f71229c3f35526743131f4bdbeb365dff306c0397fb58446497a1240e306e080

## Spatial Detail

- paletteTiles: 25
- rewriteRules: 9
- constraintRules: 25
- sameFamilyRowsDifferByTwoMetrics: true
- familiesDifferByPaletteAndRuleSet: true
- matrix-row-map-panel-rpg-seed-alpha: family=map_panel_rpg, seed=seed_alpha, dimensions=9x7, reachable=true, routeVerified=true, pathLength=15, varianceMarker=goal062-6242bb377e66, hash=07533595e37706a15955d99feba444dbbc07926c3269813198fc964d83b94db3
- matrix-row-map-panel-rpg-seed-beta: family=map_panel_rpg, seed=seed_beta, dimensions=10x7, reachable=true, routeVerified=true, pathLength=16, varianceMarker=goal062-c095182e333f, hash=cfced263bd7b643b8875d16d84f32e55ec4a42080be4266314753fe2556bbfb2
- matrix-row-map-panel-rpg-seed-gamma: family=map_panel_rpg, seed=seed_gamma, dimensions=9x8, reachable=true, routeVerified=true, pathLength=12, varianceMarker=goal062-2dc280d7c03f, hash=9adde2ca91644c8778280e7e6cd44a334ccac318abef8ae44145cd8693e0a721
- matrix-row-survival-sandbox-seed-alpha: family=survival_sandbox, seed=seed_alpha, dimensions=10x7, reachable=true, routeVerified=true, pathLength=14, varianceMarker=goal062-173c09e7372a, hash=3662976424a44cb5708a4c75a7aff619078349c75a332ce85cd79b4c91a35319
- matrix-row-survival-sandbox-seed-beta: family=survival_sandbox, seed=seed_beta, dimensions=10x8, reachable=true, routeVerified=true, pathLength=15, varianceMarker=goal062-d4b4e782c639, hash=77eb9c8e9053ea42b5b2c3edfa672b594d98f6649bbc1e358eee6d6de106760e
- matrix-row-survival-sandbox-seed-gamma: family=survival_sandbox, seed=seed_gamma, dimensions=11x7, reachable=true, routeVerified=true, pathLength=13, varianceMarker=goal062-7e5433dddce5, hash=f1e3e7cab3e355dfddf0408e907484557df15ed6fe8a6d3cbcb9b108370822d3
- matrix-row-first-person-grid-dungeon-seed-alpha: family=first_person_grid_dungeon, seed=seed_alpha, dimensions=8x8, reachable=true, routeVerified=true, pathLength=11, varianceMarker=goal062-347239d6fd2a, hash=7e06e591275ecfb833d32d91f52feaab840e5ef334509a2ae607732900ea6485
- matrix-row-first-person-grid-dungeon-seed-beta: family=first_person_grid_dungeon, seed=seed_beta, dimensions=9x8, reachable=true, routeVerified=true, pathLength=12, varianceMarker=goal062-d7d58a78b9d8, hash=f525a338be210e2dcf46ce4b9907e95e42c7bdd3b4dab6becf2d6f6b8167d9a8
- matrix-row-first-person-grid-dungeon-seed-gamma: family=first_person_grid_dungeon, seed=seed_gamma, dimensions=8x9, reachable=true, routeVerified=true, pathLength=12, varianceMarker=goal062-10114ed8533b, hash=d8eff313213bf6903aa83158b91169aed1e4773cb112b7948c3932fd49c7f9e6

## Reachability And Repair

- reachabilityRows: 9/9
- routeVerifiedRows: 9/9
- repairRows: 9
- contradictionScenarios: 1
- contradictionDiagnostic: goal062.constraint.no_tile_candidate [synthetic/contradiction/no-tile-candidate] A contradictory candidate set with no allowed tile is rejected before row promotion.
- contradictionDiagnostic: goal062.constraint.fallback_budget_recorded [fallbackBudget=3] Fallback relaxation is explicit and bounded.

## Preview/Export Payload

- passed: true
- rows: 9
- thumbnails: skipped_no_existing_bcl_png_helper_required_for_goal

## Unity Proof

- passed: true
- unityEditorOrPlayerExecuted: true
- unityExitCode: 0
- playerExitCode: 0
- provenRowCount: 9
- blockerCode: (none)
- blockerMessage: (none)
- launchLog: .llmgc/procedural/goal-062-constrained-spatial-detail-generation/logs/alpha-player-launch.log
- playLoopLog: .llmgc/procedural/goal-062-constrained-spatial-detail-generation/logs/alpha-player-play-loop.log
- expectedMarkerCount: 29
- requiredMarker: constrained_spatial_detail_generation_verification=required
- requiredMarker: review_package_proof=goal062
- requiredMarker: spatial_detail_family=first_person_grid_dungeon
- requiredMarker: spatial_detail_family=map_panel_rpg
- requiredMarker: spatial_detail_family=survival_sandbox
- requiredMarker: spatial_detail_loaded=true
- requiredMarker: spatial_detail_reachable=true
- requiredMarker: spatial_detail_route_verified=true
- requiredMarker: spatial_detail_row=matrix-row-first-person-grid-dungeon-seed-alpha
- requiredMarker: spatial_detail_row=matrix-row-first-person-grid-dungeon-seed-beta
- requiredMarker: spatial_detail_row=matrix-row-first-person-grid-dungeon-seed-gamma
- requiredMarker: spatial_detail_row=matrix-row-map-panel-rpg-seed-alpha
- requiredMarker: spatial_detail_row=matrix-row-map-panel-rpg-seed-beta
- requiredMarker: spatial_detail_row=matrix-row-map-panel-rpg-seed-gamma
- requiredMarker: spatial_detail_row=matrix-row-survival-sandbox-seed-alpha
- requiredMarker: spatial_detail_row=matrix-row-survival-sandbox-seed-beta
- requiredMarker: spatial_detail_row=matrix-row-survival-sandbox-seed-gamma
- requiredMarker: spatial_detail_seed=seed_alpha
- requiredMarker: spatial_detail_seed=seed_beta
- requiredMarker: spatial_detail_seed=seed_gamma
- requiredMarker: spatial_detail_variance_marker=goal062-10114ed8533b
- requiredMarker: spatial_detail_variance_marker=goal062-173c09e7372a
- requiredMarker: spatial_detail_variance_marker=goal062-2dc280d7c03f
- requiredMarker: spatial_detail_variance_marker=goal062-347239d6fd2a
- requiredMarker: spatial_detail_variance_marker=goal062-6242bb377e66
- requiredMarker: spatial_detail_variance_marker=goal062-7e5433dddce5
- requiredMarker: spatial_detail_variance_marker=goal062-c095182e333f
- requiredMarker: spatial_detail_variance_marker=goal062-d4b4e782c639
- requiredMarker: spatial_detail_variance_marker=goal062-d7d58a78b9d8
- matchedMarker: constrained_spatial_detail_generation_verification=required
- matchedMarker: review_package_proof=goal062
- matchedMarker: spatial_detail_family=first_person_grid_dungeon
- matchedMarker: spatial_detail_family=map_panel_rpg
- matchedMarker: spatial_detail_family=survival_sandbox
- matchedMarker: spatial_detail_loaded=true
- matchedMarker: spatial_detail_reachable=true
- matchedMarker: spatial_detail_route_verified=true
- matchedMarker: spatial_detail_row=matrix-row-first-person-grid-dungeon-seed-alpha
- matchedMarker: spatial_detail_row=matrix-row-first-person-grid-dungeon-seed-beta
- matchedMarker: spatial_detail_row=matrix-row-first-person-grid-dungeon-seed-gamma
- matchedMarker: spatial_detail_row=matrix-row-map-panel-rpg-seed-alpha
- matchedMarker: spatial_detail_row=matrix-row-map-panel-rpg-seed-beta
- matchedMarker: spatial_detail_row=matrix-row-map-panel-rpg-seed-gamma
- matchedMarker: spatial_detail_row=matrix-row-survival-sandbox-seed-alpha
- matchedMarker: spatial_detail_row=matrix-row-survival-sandbox-seed-beta
- matchedMarker: spatial_detail_row=matrix-row-survival-sandbox-seed-gamma
- matchedMarker: spatial_detail_seed=seed_alpha
- matchedMarker: spatial_detail_seed=seed_beta
- matchedMarker: spatial_detail_seed=seed_gamma
- matchedMarker: spatial_detail_variance_marker=goal062-10114ed8533b
- matchedMarker: spatial_detail_variance_marker=goal062-173c09e7372a
- matchedMarker: spatial_detail_variance_marker=goal062-2dc280d7c03f
- matchedMarker: spatial_detail_variance_marker=goal062-347239d6fd2a
- matchedMarker: spatial_detail_variance_marker=goal062-6242bb377e66
- matchedMarker: spatial_detail_variance_marker=goal062-7e5433dddce5
- matchedMarker: spatial_detail_variance_marker=goal062-c095182e333f
- matchedMarker: spatial_detail_variance_marker=goal062-d4b4e782c639
- matchedMarker: spatial_detail_variance_marker=goal062-d7d58a78b9d8

## Invalid/fake/leak Matrix

- passed: true
- scenarioCount: 18
- contradiction_no_tile_candidate: expectedStatus=rejected, actualStatus=rejected, codes=goal062.constraint.no_tile_candidate
- copied_mxgmn_sample_asset_claim: expectedStatus=rejected, actualStatus=rejected, codes=goal062.leak.mxgmn_sample_asset_claim
- external_asset_provenance_leak: expectedStatus=rejected, actualStatus=rejected, codes=goal062.leak.external_asset_provenance
- fake_family: expectedStatus=rejected, actualStatus=rejected, codes=goal062.source.fake_family
- fake_package_row_id: expectedStatus=rejected, actualStatus=rejected, codes=goal062.source.fake_package_row_id
- fake_seed: expectedStatus=rejected, actualStatus=rejected, codes=goal062.source.fake_seed
- invalid_tile_id: expectedStatus=rejected, actualStatus=rejected, codes=goal062.row.invalid_tile_id
- lua_execution_claim: expectedStatus=rejected, actualStatus=rejected, codes=goal062.leak.lua_execution_claim
- missing_entry: expectedStatus=rejected, actualStatus=rejected, codes=goal062.reachability.entry_missing
- missing_exit: expectedStatus=rejected, actualStatus=rejected, codes=goal062.reachability.exit_missing
- missing_goal061_source: expectedStatus=blocked, actualStatus=blocked, codes=goal062.source.goal061_manifest_missing
- missing_unity_proof_trace: expectedStatus=rejected, actualStatus=rejected, codes=goal062.unity.missing_proof_trace
- nondeterministic_ordering: expectedStatus=rejected, actualStatus=rejected, codes=goal062.matrix.nondeterministic_ordering
- provider_network_llm_rag_claim: expectedStatus=rejected, actualStatus=rejected, codes=goal062.leak.provider_network_llm_rag_claim
- public_gamepackage_mutation_claim: expectedStatus=rejected, actualStatus=rejected, codes=goal062.leak.public_gamepackage_mutation_claim
- runtime_ui_broad_mutation_claim: expectedStatus=rejected, actualStatus=rejected, codes=goal062.leak.runtime_ui_broad_mutation_claim
- unreachable_objective: expectedStatus=rejected, actualStatus=rejected, codes=goal062.reachability.entry_to_objective_unreachable
- unsafe_path_traversal: expectedStatus=rejected, actualStatus=rejected, codes=goal062.reachability.unsafe_path_traversal

## Diagnostics

- info: goal062.preflight.goal061_handoff_recorded [full_campaign_playable_review_package_rc_verification] Goal 061 is recorded as accepted by user handoff before Goal 062.
- info: goal062.source.loaded [Goal061] Goal 062 source facts were loaded from repository-local Goal 061 review package RC evidence.
- info: goal062.unity.editor_executed [logs/unity-build.log] Unity Editor was invoked through the existing Alpha build entrypoint.
- info: goal062.unity.editor_executed [logs/unity-build.log] Unity Editor was invoked through the existing Alpha build entrypoint.
- info: goal062.unity.editor_executed [logs/unity-build.log] Unity Editor was invoked through the existing Alpha build entrypoint.
- info: goal062.unity.editor_exit_success [exit_code:0] Unity Editor build process exited successfully.
- info: goal062.unity.editor_exit_success [exit_code:0] Unity Editor build process exited successfully.
- info: goal062.unity.editor_exit_success [exit_code:0] Unity Editor build process exited successfully.
- info: goal062.unity.player_executed [logs/alpha-player-play-loop.log] The produced Alpha player was launched in spatial-detail play-loop mode.
- info: goal062.unity.player_executed [logs/alpha-player-play-loop.log] The produced Alpha player was launched in spatial-detail play-loop mode.
- info: goal062.unity.player_executed [logs/alpha-player-play-loop.log] The produced Alpha player was launched in spatial-detail play-loop mode.
- info: goal062.unity.player_exit_success [exit_code:0] Alpha player process exited successfully.
- info: goal062.unity.player_exit_success [exit_code:0] Alpha player process exited successfully.
- info: goal062.unity.player_exit_success [exit_code:0] Alpha player process exited successfully.

## Boundaries

No external dependency/source/asset import, provider/network/LLM/RAG call, media generation, arbitrary Lua execution, public GamePackage schema change, Runtime/Runtime.Abstractions change, WinForms UI change, Infrastructure provider path change, generator-library change, solution or project file change is part of this Goal 062 proof. Unity changes are limited to spatial-detail marker support in AlphaRuntimeBootstrap.

constrained_spatial_detail_generation_verification required
