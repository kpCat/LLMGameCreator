# Goal 141 Runtime-backed Unity Player Command Roundtrip Bridge

- status: GREEN
- accepted: false
- goal140Accepted: true
- candidateId: minimal-map-game-balanced-baseline
- roundtripRequestCount: 6
- runtimeExecutedRequestCount: 6
- roundtripSnapshotCount: 6
- controlRequestBridgePresent: true
- stateHashChainPresent: true
- runtimeAuthority: true
- projectionOnly: false
- unityGameplayTruth: false
- unityConsumesRoundtripResult: true
- unitySmokePassed: true
- manualUnityOptional: true
- normalCommand: .devflow\scripts\run-runtime-backed-player-command-roundtrip.cmd
- reportPath: .llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/one-click-runtime-backed-player-command-roundtrip-report.md
- negativeProofPassed: true

## Requests

- 0 load_model -> load_package_or_session; status=executed_by_runtime; snapshotHash=bfbfce6afb9f47d30409c2a3f112a6b955941af8b518807de230f072edd7fb7f; runtimeExecuted=true
- 1 reset_first -> show_or_select_start_state; status=executed_by_runtime; snapshotHash=12f118c9a81db66faaa1371ed05943694abe0dd1969ddcc6264f7811d2c27ab9; runtimeExecuted=true
- 2 step_once -> advance_to_interaction; status=executed_by_runtime; snapshotHash=9db22a14ed49e94a88a9912af6ed3213f2714a98f901136fbfdb44ca8f7b9980; runtimeExecuted=true
- 3 next_frame -> advance_to_dialogue_or_quest; status=executed_by_runtime; snapshotHash=b4a0feeef3aecc3254c07cc8bb5ce7a07d0a8d9126d2eed4c05dcfb9782a3729; runtimeExecuted=true
- 4 play_all_to_end -> advance_to_inventory_or_crafting; status=executed_by_runtime; snapshotHash=cb819cb474f7019646de72de59a85cbe1fd0909a476e218b389864fb92fb53c6; runtimeExecuted=true
- 5 copy_frame_summary -> advance_to_combat_or_final_state; status=executed_by_runtime; snapshotHash=29c99098d25aa934b72a06063d82b5bf44b6454cb7195a178ef959a6224b95c2; runtimeExecuted=true

## Diagnostics

- unityExitCode=0
- passMarkerPresent=True
- failMarkerPresent=False
- modelPathExists=True
- roundtripRequestCountPassed=True
- runtimeSnapshotResponsePresent=True
- runtimeAuthorityMarkersPresent=True
- unityConsumesRoundtripResult=True
- unityGameplayTruth=False
