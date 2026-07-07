# Goal 138 Runtime-backed Unity Player Loop Stepper HUD Harness

- status: GREEN
- accepted: false
- acceptedGoal137: true
- candidateId: minimal-map-game-balanced-baseline
- frameCount: 13
- requiredFrameCategoriesPresent: true
- runtimeAuthority: true
- unityGameplayTruth: false
- projectionOnly: false
- stepperWindowPresent: true
- stepperBatchSmokePassed: true
- manualUnityOptional: true
- normalCommand: .devflow\scripts\run-runtime-backed-unity-player-loop-stepper.cmd
- reportPath: .llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/one-click-runtime-backed-player-loop-stepper-report.md
- modelPath: .llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/runtime-backed-player-loop-stepper-model.json

## Source Checks

- sourcePlaybackResultGreen: true
- sourceCommandLoopResultGreen: true
- playerAdapterContractPresent: true
- playerAdapterRequiredCategoriesMatch: true

## Required Frame Categories

- load_package: true
- show_start_state: true
- show_map_position: true
- show_interaction_result: true
- show_dialogue: true
- show_quest_state: true
- show_inventory_state: true
- show_crafting_result: true
- show_harvest_result: true
- show_transaction_result: true
- show_encounter_state: true
- show_combat_round: true
- show_final_state: true

## Diagnostics

- unityExitCode=0
