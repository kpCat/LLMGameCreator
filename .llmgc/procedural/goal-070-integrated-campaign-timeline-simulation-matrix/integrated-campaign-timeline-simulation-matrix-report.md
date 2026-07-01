# Goal 070 Integrated Campaign Timeline Simulation Matrix Report

integrated_campaign_timeline_simulation_matrix_verification required
accepted=false
implementationStatus=GREEN
rowCount=9
stateChangingRowCount=9
familyCount=3
seedCount=3
sourceFactsConsumed=True
goal069AcceptedByUserHandoff=True
rowMatrixPassed=True
cascadeLedgerPassed=True
cascadeCount=27
arbitrationLedgerPassed=True
arbitrationCount=9
saveLoadReplayPassed=True
saveLoadPassedRowCount=9
replayPassedRowCount=9
meaningfulVariancePassed=True
unityCommandPlanPassed=True
unityProofPassed=True
unityExitCode=0
playerExitCode=0
provenRowCount=9
allTimelineMarkersMatched=True
previewExportPayloadPassed=True
invalidMatrixPassed=True
reportHash=5db771792666d24cc334b9203fc8e5a6f7970f648f339f58d139377a3506aa89

## Source Gates
- full_campaign_gamepackage_materialization_matrix_verification passed user_handoff
- full_campaign_playable_review_package_rc_verification passed user_handoff
- constrained_spatial_detail_generation_verification passed user_handoff
- gameplay_consequence_depth_matrix_verification passed user_handoff
- living_world_npc_faction_simulation_matrix_verification passed user_handoff
- interlocked_gameplay_systems_depth_matrix_verification passed user_handoff
- settlement_construction_destruction_production_matrix_verification passed user_handoff
- programmatic_narrative_quest_dialogue_event_matrix_verification passed user_handoff
- combat_magic_ability_boss_encounter_matrix_verification passed user_handoff
- world_event_weather_daynight_crisis_matrix_verification passed user_handoff
- integrated_campaign_timeline_simulation_matrix_verification required current_goal_manual_gate

## Matrix Rows
- matrix-row-first-person-grid-dungeon-seed-alpha family=first_person_grid_dungeon seed=seed_alpha ticks=7 categories=7 cascades=3 arbitration=clue first then restricted loot stateChanged=True replay=True
- matrix-row-first-person-grid-dungeon-seed-beta family=first_person_grid_dungeon seed=seed_beta ticks=7 categories=7 cascades=3 arbitration=anti-hazard charm before boss loot stateChanged=True replay=True
- matrix-row-first-person-grid-dungeon-seed-gamma family=first_person_grid_dungeon seed=seed_gamma ticks=7 categories=7 cascades=3 arbitration=warden diplomacy before relic extraction stateChanged=True replay=True
- matrix-row-map-panel-rpg-seed-alpha family=map_panel_rpg seed=seed_alpha ticks=7 categories=7 cascades=3 arbitration=escort priority with repair delay stateChanged=True replay=True
- matrix-row-map-panel-rpg-seed-beta family=map_panel_rpg seed=seed_beta ticks=7 categories=7 cascades=3 arbitration=checkpoint diplomacy before travel stateChanged=True replay=True
- matrix-row-map-panel-rpg-seed-gamma family=map_panel_rpg seed=seed_gamma ticks=7 categories=7 cascades=3 arbitration=settlement repair before convoy stateChanged=True replay=True
- matrix-row-survival-sandbox-seed-alpha family=survival_sandbox seed=seed_alpha ticks=7 categories=7 cascades=3 arbitration=shelter repair before optional rescue stateChanged=True replay=True
- matrix-row-survival-sandbox-seed-beta family=survival_sandbox seed=seed_beta ticks=7 categories=7 cascades=3 arbitration=water ration before crafting stateChanged=True replay=True
- matrix-row-survival-sandbox-seed-gamma family=survival_sandbox seed=seed_gamma ticks=7 categories=7 cascades=3 arbitration=perimeter defense before scavenging stateChanged=True replay=True

## Cascade And Arbitration
- cascadeRows=9 cascadeCount=27
- arbitrationRows=9 arbitrationCount=9

## Replay And Variance
- replayRows=9 saveLoadPassed=9 replayPassed=9
- distinctPhaseProfiles=3 distinctRowHashes=9

## Invalid Matrix
- arbitrary_lua_execution_claim rejected
- broad_unity_gameplay_mutation_claim rejected
- duplicate_row_id rejected
- fake_family rejected
- fake_seed rejected
- fake_source_id rejected
- final_prose_leakage rejected
- missing_arbitration rejected
- missing_cross_system_cascade rejected
- missing_family_row rejected
- missing_goal069_source rejected
- nondeterministic_order rejected
- provider_llm_rag_media_generation_claim rejected
- replay_mismatch rejected
- runtime_ui_gamepackage_schema_mutation_claim rejected
- save_load_mismatch rejected
- stale_goal069_handoff rejected
- unchanged_final_state rejected
- unsafe_path rejected
- variance_only_by_id_hash rejected

## Diagnostics
- [info] goal070.preflight.goal069_handoff_recorded world_event_weather_daynight_crisis_matrix_verification - Goal 069 is recorded as accepted by user handoff before Goal 070.
- [info] goal070.source.loaded Goal060-069 - Goal 070 source facts were loaded from repository-local Goal 060/061/062/063/064/065/066/067/068/069 compact evidence.
- [info] goal070.unity.editor_executed logs/unity-build.log - Unity Editor was invoked through the existing Alpha build entrypoint.
- [info] goal070.unity.editor_exit_success exit_code:0 - Unity Editor build process exited successfully.
- [info] goal070.unity.player_executed logs/alpha-player-play-loop.log - The produced Alpha player was launched in integrated timeline marker mode.
- [info] goal070.unity.player_exit_success exit_code:0 - Alpha player process exited successfully.
