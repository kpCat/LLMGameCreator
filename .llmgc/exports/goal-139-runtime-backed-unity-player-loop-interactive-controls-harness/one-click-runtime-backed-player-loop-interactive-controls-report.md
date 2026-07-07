# Goal 139 Runtime-backed Unity Player Loop Interactive Controls Harness

- status: GREEN
- accepted: false
- acceptedGoal138: true
- candidateId: minimal-map-game-balanced-baseline
- frameCount: 13
- requiredControlsPresent: true
- controlScriptPassed: true
- interactiveControlsWindowPresent: true
- unityInteractiveControlsSmokePassed: true
- runtimeAuthority: true
- unityGameplayTruth: false
- projectionOnly: false
- manualUnityOptional: true
- normalCommand: .devflow\scripts\run-runtime-backed-unity-player-loop-interactive-controls.cmd
- reportPath: .llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/one-click-runtime-backed-player-loop-interactive-controls-report.md
- modelPath: .llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/runtime-backed-player-loop-interactive-controls-model.json
- controlScriptPath: .llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/runtime-backed-player-loop-interactive-controls-script.json

## Source Checks

- sourceStepperResultGreen: true
- sourcePlaybackFramesPresent: true
- sourceCommandLoopSnapshotsPresent: true
- playerAdapterContractPresent: true

## Required Controls

- load_model: true
- first: true
- previous: true
- next: true
- last: true
- autoplay_tick: true
- autoplay_all: true
- copy_current_frame_summary: true
- show_runtime_hash: true
- show_hud_lines: true

## Script Steps

- 0 load_model => frame 0 passed=true
- 1 assert_frame_count => frame 0 passed=true
- 2 first => frame 0 passed=true
- 3 next => frame 1 passed=true
- 4 next => frame 2 passed=true
- 5 previous => frame 1 passed=true
- 6 last => frame 12 passed=true
- 7 first => frame 0 passed=true
- 8 autoplay_tick => frame 1 passed=true
- 9 autoplay_tick => frame 2 passed=true
- 10 autoplay_all => frame 12 passed=true
- 11 copy_current_frame_summary => frame 12 passed=true
- 12 assert_final_frame_reachable => frame 12 passed=true
- 13 assert_runtime_authority_markers => frame 12 passed=true

## Diagnostics

- unityExitCode=0
