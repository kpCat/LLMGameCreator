# Goal 141 Runtime-backed Unity Player Command Roundtrip Bridge

- status: GREEN
- accepted: false
- goal140Accepted: true
- candidateId: minimal-map-game-balanced-baseline
- totalControlRequestCount: 6
- roundtripRequestCount: 6
- runtimeRoutedRequestCount: 4
- presentationOnlyRequestCount: 2
- runtimeExecutedRequestCount: 4
- presentationOnlyRuntimeExecutionCount: 0
- runtimeMutatingPresentationRequestCount: 0
- responseCount: 6
- roundtripSnapshotCount: 15
- controlRequestBridgePresent: true
- stateHashChainPresent: true
- requestResponseCorrelationPassed: true
- sequentialCursorContinuityPassed: true
- stateHashContinuityPassed: true
- copySummaryStateUnchanged: true
- loadModelStateUnchanged: true
- playAllExecutedRemainingCommands: true
- noControlIntentMappedToUnrelatedGameplayCommand: true
- roundtripSemanticCorrectnessPassed: true
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

- 0 load_model route=presentation_only operation=preserve_loaded_model_state coverage=presentation_only; status=presentation_only_preserved_state; snapshotHash=not_loaded; runtimeExecuted=false; executedCommandCount=0; producedSnapshotCount=1
- 1 reset_first route=runtime_session operation=reset_or_initialize_session coverage=show_or_select_start_state; status=executed_by_runtime; snapshotHash=12f118c9a81db66faaa1371ed05943694abe0dd1969ddcc6264f7811d2c27ab9; runtimeExecuted=true; executedCommandCount=1; producedSnapshotCount=2
- 2 step_once route=runtime_command operation=execute_next_runtime_command coverage=advance_to_interaction; status=executed_by_runtime; snapshotHash=8cdb8b914519b5e62215a8ad31e21333c0b75b15f54b5460ef608335823aa8c9; runtimeExecuted=true; executedCommandCount=1; producedSnapshotCount=1
- 3 next_frame route=runtime_command operation=execute_next_runtime_command coverage=advance_to_interaction; status=executed_by_runtime; snapshotHash=9db22a14ed49e94a88a9912af6ed3213f2714a98f901136fbfdb44ca8f7b9980; runtimeExecuted=true; executedCommandCount=1; producedSnapshotCount=1
- 4 play_all_to_end route=runtime_command_batch operation=execute_remaining_runtime_commands coverage=advance_to_dialogue_or_quest; status=executed_by_runtime; snapshotHash=29c99098d25aa934b72a06063d82b5bf44b6454cb7195a178ef959a6224b95c2; runtimeExecuted=true; executedCommandCount=8; producedSnapshotCount=9
- 5 copy_frame_summary route=presentation_only operation=copy_current_frame_summary coverage=presentation_only; status=presentation_only_preserved_state; snapshotHash=29c99098d25aa934b72a06063d82b5bf44b6454cb7195a178ef959a6224b95c2; runtimeExecuted=false; executedCommandCount=0; producedSnapshotCount=1

## Diagnostics

- unityExitCode=0
- passMarkerPresent=True
- failMarkerPresent=False
- modelPathExists=True
- roundtripRequestCountPassed=True
- presentationOnlyRequestCountPassed=True
- presentationOnlyRuntimeExecutionCountPassed=True
- requestResponseCorrelationPassed=True
- sequentialCursorContinuityPassed=True
- copySummaryStateUnchanged=True
- loadModelStateUnchanged=True
- noControlIntentMappedToUnrelatedGameplayCommand=True
- runtimeSnapshotResponsePresent=True
- runtimeAuthorityMarkersPresent=True
- unityConsumesRoundtripResult=True
- unityGameplayTruth=False
