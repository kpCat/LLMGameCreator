# Goal 071 Unity Alpha Interactive Campaign Player Report

unity_alpha_interactive_campaign_player_verification required
accepted=false
implementationStatus=GREEN
rowCount=9
stateChangingRowCount=9
familyCount=3
seedCount=3
actionCount=63
transitionCount=63
sourceFactsConsumed=True
goal070AcceptedByUserHandoff=True
rowMatrixPassed=True
selectorPassed=True
inputActionScriptPassed=True
stateTransitionLedgerPassed=True
saveLoadReplayPassed=True
saveLoadPassedRowCount=9
replayPassedRowCount=9
hudContractPassed=True
unityCommandPlanPassed=True
unityProofPassed=True
unityExitCode=0
playerExitCode=0
provenRowCount=9
allInteractiveMarkersMatched=True
previewExportPayloadPassed=True
invalidMatrixPassed=True
reportHash=ca0828e5da1ff8d08b6b6e0574bfe27568d7acef1447ec30f47ede0581d42d02

## Source Gates
- integrated_campaign_timeline_simulation_matrix_verification passed user_handoff
- unity_alpha_interactive_campaign_player_verification required current_goal_manual_gate

## Selector
- map_panel_rpg seeds=seed_alpha,seed_beta,seed_gamma rows=3
- survival_sandbox seeds=seed_alpha,seed_beta,seed_gamma rows=3
- first_person_grid_dungeon seeds=seed_alpha,seed_beta,seed_gamma rows=3

## Interactive Rows
- matrix-row-first-person-grid-dungeon-seed-alpha family=first_person_grid_dungeon seed=seed_alpha actions=7 selectedInput=input/goal071/first-person-grid-dungeon/seed-alpha/advance-step-01 selectedStep=tick-01-dawn-night-weather-crisis-pressure stateChanged=True hud=True replay=True
- matrix-row-first-person-grid-dungeon-seed-beta family=first_person_grid_dungeon seed=seed_beta actions=7 selectedInput=input/goal071/first-person-grid-dungeon/seed-beta/advance-step-01 selectedStep=tick-01-dawn-night-weather-crisis-pressure stateChanged=True hud=True replay=True
- matrix-row-first-person-grid-dungeon-seed-gamma family=first_person_grid_dungeon seed=seed_gamma actions=7 selectedInput=input/goal071/first-person-grid-dungeon/seed-gamma/advance-step-01 selectedStep=tick-01-dawn-night-weather-crisis-pressure stateChanged=True hud=True replay=True
- matrix-row-map-panel-rpg-seed-alpha family=map_panel_rpg seed=seed_alpha actions=7 selectedInput=input/goal071/map-panel-rpg/seed-alpha/advance-step-01 selectedStep=tick-01-dawn-night-weather-crisis-pressure stateChanged=True hud=True replay=True
- matrix-row-map-panel-rpg-seed-beta family=map_panel_rpg seed=seed_beta actions=7 selectedInput=input/goal071/map-panel-rpg/seed-beta/advance-step-01 selectedStep=tick-01-dawn-night-weather-crisis-pressure stateChanged=True hud=True replay=True
- matrix-row-map-panel-rpg-seed-gamma family=map_panel_rpg seed=seed_gamma actions=7 selectedInput=input/goal071/map-panel-rpg/seed-gamma/advance-step-01 selectedStep=tick-01-dawn-night-weather-crisis-pressure stateChanged=True hud=True replay=True
- matrix-row-survival-sandbox-seed-alpha family=survival_sandbox seed=seed_alpha actions=7 selectedInput=input/goal071/survival-sandbox/seed-alpha/advance-step-01 selectedStep=tick-01-dawn-night-weather-crisis-pressure stateChanged=True hud=True replay=True
- matrix-row-survival-sandbox-seed-beta family=survival_sandbox seed=seed_beta actions=7 selectedInput=input/goal071/survival-sandbox/seed-beta/advance-step-01 selectedStep=tick-01-dawn-night-weather-crisis-pressure stateChanged=True hud=True replay=True
- matrix-row-survival-sandbox-seed-gamma family=survival_sandbox seed=seed_gamma actions=7 selectedInput=input/goal071/survival-sandbox/seed-gamma/advance-step-01 selectedStep=tick-01-dawn-night-weather-crisis-pressure stateChanged=True hud=True replay=True

## Action And Transition Proof
- actionCount=63 transitionCount=63
- hudRows=9 saveLoadRows=9

## Invalid Matrix
- broad_unity_mutation_claim rejected
- command_plan_skips_required_state_transition rejected
- command_plan_unknown_row rejected
- duplicate_row_id rejected
- fake_family_seed_row_id rejected
- final_prose_leakage rejected
- missing_goal070_source rejected
- missing_hud_contract rejected
- nondeterministic_order rejected
- provider_llm_rag_claim rejected
- replay_mismatch rejected
- runtime_gamepackage_schema_mutation_claim rejected
- state_hash_unchanged rejected
- unity_marker_missing rejected
- unsafe_path rejected

## Diagnostics
- [info] goal071.preflight.goal070_handoff_recorded integrated_campaign_timeline_simulation_matrix_verification - Goal 070 is recorded as accepted by user handoff before Goal 071.
- [info] goal071.source.loaded Goal070 - Goal 071 source facts were loaded from repository-local Goal 070 compact evidence.
- [info] goal071.unity.editor_executed logs/unity-build.log - Unity Editor was invoked through the existing Alpha build entrypoint.
- [info] goal071.unity.editor_exit_success exit_code:0 - Unity Editor build process exited successfully.
- [info] goal071.unity.player_executed logs/alpha-player-play-loop.log - The produced Alpha player was launched in interactive campaign marker mode.
- [info] goal071.unity.player_exit_success exit_code:0 - Alpha player process exited successfully.
