# Goal 076 Edit-Driven Playable Preview Refresh

- gate: edit_driven_playable_preview_refresh_verification required
- accepted: false
- implementationStatus: GREEN
- goal075Handoff: True
- goal075ImplementationGreen: True
- goal075ParentActivationBindingPassed: True
- changedRowCount: 9
- appliedChangeCount: 18
- packageTargetCount: 18
- sourceGoal075ReportHash: 4569ab956f9318d9219469da8c167af85d85e880787375356f74904422bb9e2d
- beforeStateHash: e332171ab5965f940430f6eaf385882cf604a6abc010c1b1cd4cc40222e8b137
- afterStateHash: 335a5fb2b08cf51f5eb590e5c7e96cc5a5a1bd8f4a607d30a27b5cf811961ec7
- rollbackStateHash: e332171ab5965f940430f6eaf385882cf604a6abc010c1b1cd4cc40222e8b137
- replayStateHash: 335a5fb2b08cf51f5eb590e5c7e96cc5a5a1bd8f4a607d30a27b5cf811961ec7
- previewRefreshHash: f3250138d464ede3793a5533cf90abb8ef5017ce24aa4a6cae71896ff03dcba5
- refreshPlanHash: b34703b0d8a7f566506edbf7d3a900efc02dea4ab5934e654f7d9a1eb44fac6b
- handoffManifestHash: 91fa387055c69da99df23e65dcc00379241a398f87708b5a152221cf36ec6c8d
- tamperNegativeProofHash: ecd86fc54327d922059e9d0aff7b40ecf91d0acac05497eebfe0a070b86d12a9
- reportHash: 6794b1cd1eb2c9ad3618e055ae430f8b9fc6cc7f52dbdf01955d7022d248e134

## Proof
- stateTransitionProofPassed: True
- gamePackageRefreshPlanPassed: True
- stagedHandoffManifestPassed: True
- tamperNegativeProofPassed: True
- handoffRows: 9
- packageTargets: 18

## Refresh Plan
- disposition: sidecar_refresh_plan_only_because_public_GamePackage_schema_and_Runtime_changes_are_forbidden
- previewExportRefreshPayloadRef: .llmgc/procedural/goal-075-schema-driven-campaign-edit-validate-apply-loop/preview-export-refresh-payload.json
- matrix-row-map-panel-rpg-seed-alpha targets=2 refreshKey=goal075.refresh.map_panel_rpg.seed_alpha
- matrix-row-map-panel-rpg-seed-beta targets=2 refreshKey=goal075.refresh.map_panel_rpg.seed_beta
- matrix-row-map-panel-rpg-seed-gamma targets=2 refreshKey=goal075.refresh.map_panel_rpg.seed_gamma
- matrix-row-survival-sandbox-seed-alpha targets=2 refreshKey=goal075.refresh.survival_sandbox.seed_alpha
- matrix-row-survival-sandbox-seed-beta targets=2 refreshKey=goal075.refresh.survival_sandbox.seed_beta
- matrix-row-survival-sandbox-seed-gamma targets=2 refreshKey=goal075.refresh.survival_sandbox.seed_gamma
- matrix-row-first-person-grid-dungeon-seed-alpha targets=2 refreshKey=goal075.refresh.first_person_grid_dungeon.seed_alpha
- matrix-row-first-person-grid-dungeon-seed-beta targets=2 refreshKey=goal075.refresh.first_person_grid_dungeon.seed_beta
- matrix-row-first-person-grid-dungeon-seed-gamma targets=2 refreshKey=goal075.refresh.first_person_grid_dungeon.seed_gamma

## Negative Proof
- missing_staged_handoff_manifest: rejected
- tampered_staged_handoff_manifest: rejected

## Diagnostics
- none
