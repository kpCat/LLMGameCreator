# Goal 119 Accepted Alpha Unity Playable Projection

- implementationStatus: GREEN
- projectionStatus: GREEN
- unityMenuPath: LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection
- baselineId: offline_geoworld_alpha_accepted_baseline_v1
- acceptedBaselineReady: true
- manualGateStatus: ACCEPTED_BY_HUMAN
- expectedGeneratedRootName: __LLMGC_AcceptedAlphaPlayableProjection__
- scriptInventoryCount: 5
- smokePlanStepCount: 6
- previewCommandCount: 18
- chunkWindowStepCount: 4
- boundaryCrossingCount: 2
- interactionTargetCount: 8
- objectiveCount: 6
- completedObjectiveCount: 6
- replayStepCount: 6
- forbiddenUnitySurfaceClean: true
- notFinalReleaseOrRuntimeBuild: true
- evidencePath: .llmgc/procedural/goal-119-accepted-alpha-unity-playable-projection
- exportPath: .llmgc/exports/goal-119-accepted-alpha-unity-playable-projection

## Unity Scripts

- unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs: exists=true, marker=true
- unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionController.cs: exists=true, marker=true
- unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDiagnostics.cs: exists=true, marker=true
- unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionModels.cs: exists=true, marker=true
- unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs: exists=true, marker=true

## Smoke Plan

- 1. refresh_accepted_baseline: Goal118 baseline loaded as offline_geoworld_alpha_accepted_baseline_v1
- 2. create_player_proxy: Projection root contains at least one player proxy primitive.
- 3. render_chunk_window_and_prefetch_markers: Projection contains chunk/window and boundary/prefetch markers.
- 4. render_interactions_and_objectives: Projection contains interaction target markers and objective checklist entries.
- 5. show_save_load_replay_status: Projection shows Goal106 replay/checkpoint status.
- 6. show_diagnostics_status: Projection contains diagnostics status and zero fatal errors.

## Negative Proof

- manualInputRejected: true
- runtimeSchemaProviderLuaGeneratorLibraryRejected: true
- unityScenesPrefabsSettingsPackagesStreamingAssetsRejected: true
- finalReleasePackagingRejected: true
- liveGeodataProviderNetworkRejected: true
