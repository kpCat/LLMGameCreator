# Goal 069 World Event Weather Day/Night Crisis Matrix Report

world_event_weather_daynight_crisis_matrix_verification required
accepted=false
implementationStatus=GREEN
rowCount=9
stateChangingRowCount=9
familyCount=3
seedCount=3
sourceFactsConsumed=True
goal068AcceptedByUserHandoff=True
worldClockPolicyPassed=True
weatherHazardCatalogPassed=True
crisisEventCatalogPassed=True
rowMatrixPassed=True
saveLoadReplayPassed=True
meaningfulVariancePassed=True
unityCommandPlanPassed=True
unityProofPassed=True
unityExitCode=0
playerExitCode=0
provenRowCount=9
allWorldEventMarkersMatched=True
invalidMatrixPassed=True
reportHash=40db9e42153efda4427f587873cd1cc75af4687fd0775cf429aa88430c59e63e

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
- world_event_weather_daynight_crisis_matrix_verification required current_goal_manual_gate

## Matrix Rows
- matrix-row-first-person-grid-dungeon-seed-alpha family=first_person_grid_dungeon seed=seed_alpha phase=night weather=weather/first-person-grid-dungeon/deep-fog crisis=crisis/first-person-grid-dungeon/door-seal stateChanged=True replay=True
- matrix-row-first-person-grid-dungeon-seed-beta family=first_person_grid_dungeon seed=seed_beta phase=dusk weather=weather/first-person-grid-dungeon/cold-draft crisis=crisis/first-person-grid-dungeon/warden-hunt stateChanged=True replay=True
- matrix-row-first-person-grid-dungeon-seed-gamma family=first_person_grid_dungeon seed=seed_gamma phase=night weather=weather/first-person-grid-dungeon/spore-haze crisis=crisis/first-person-grid-dungeon/loot-room-flood stateChanged=True replay=True
- matrix-row-map-panel-rpg-seed-alpha family=map_panel_rpg seed=seed_alpha phase=night weather=weather/map-panel-rpg/storm-front crisis=crisis/map-panel-rpg/refugee-convoy stateChanged=True replay=True
- matrix-row-map-panel-rpg-seed-beta family=map_panel_rpg seed=seed_beta phase=dusk weather=weather/map-panel-rpg/flooded-road crisis=crisis/map-panel-rpg/faction-border-lockdown stateChanged=True replay=True
- matrix-row-map-panel-rpg-seed-gamma family=map_panel_rpg seed=seed_gamma phase=night weather=weather/map-panel-rpg/eclipse-wind crisis=crisis/map-panel-rpg/market-fire stateChanged=True replay=True
- matrix-row-survival-sandbox-seed-alpha family=survival_sandbox seed=seed_alpha phase=night weather=weather/survival-sandbox/blizzard crisis=crisis/survival-sandbox/shelter-breach stateChanged=True replay=True
- matrix-row-survival-sandbox-seed-beta family=survival_sandbox seed=seed_beta phase=dusk weather=weather/survival-sandbox/heatwave crisis=crisis/survival-sandbox/well-contamination stateChanged=True replay=True
- matrix-row-survival-sandbox-seed-gamma family=survival_sandbox seed=seed_gamma phase=night weather=weather/survival-sandbox/acid-rain crisis=crisis/survival-sandbox/forage-collapse stateChanged=True replay=True

## Replay And Variance
- replayRows=9 saveLoadPassed=9 replayPassed=9
- distinctWeather=9 distinctCrisis=9 distinctPhaseTransitions=3

## Invalid Matrix
- arbitrary_lua_generated_lua_claim rejected
- broad_unity_weather_rendering_claim rejected
- crisis_with_no_consequence rejected
- duplicate_row_id rejected
- fake_family rejected
- fake_seed rejected
- missing_cross_system_delta rejected
- missing_goal068_source rejected
- no_day_night_effect rejected
- no_weather_hazard_effect rejected
- non_state_changing_row rejected
- nondeterministic_ordering rejected
- provider_llm_rag_claim rejected
- real_weather_network_claim rejected
- replay_mismatch rejected
- runtime_ui_gamepackage_mutation_claim rejected
- save_load_mismatch rejected
- unsafe_path rejected

## Diagnostics
- [info] goal069.preflight.goal068_handoff_recorded combat_magic_ability_boss_encounter_matrix_verification - Goal 068 is recorded as accepted by user handoff before Goal 069.
- [info] goal069.source.loaded Goal060-068 - Goal 069 source facts were loaded from repository-local Goal 060/061/062/063/064/065/066/067/068 compact evidence.
- [info] goal069.unity.editor_executed logs/unity-build.log - Unity Editor was invoked through the existing Alpha build entrypoint.
- [info] goal069.unity.editor_exit_success exit_code:0 - Unity Editor build process exited successfully.
- [info] goal069.unity.player_executed logs/alpha-player-play-loop.log - The produced Alpha player was launched in world-event marker mode.
- [info] goal069.unity.player_exit_success exit_code:0 - Alpha player process exited successfully.
