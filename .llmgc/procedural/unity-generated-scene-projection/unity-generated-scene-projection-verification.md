# Unity Generated Scene Content Projection Verification

Stopped at:

```text
unity_generated_scene_content_projection_verification
```

- Previous accepted gate: unity_playable_presentation_firewall_safe_build_verification passed
- Final gate remains required: unity_generated_scene_content_projection_verification
- Projection artifact: .llmgc/procedural/unity-generated-scene-projection/unity-generated-scene-projection.json
- Report artifact: .llmgc/procedural/unity-generated-scene-projection/unity-generated-scene-projection-report.json
- Selected package/style/thread: game/content_generation/frontier-survival / frontier_survival / thread/frontier-survival/000
- Scene nodes: 6 (command_status, item, map, npc, player, quest_event)
- Projection hash: 1f99588ffd7d8c0e89c33e0482d804517e6a448843fcbdac533b4a5cd41f9c21
- Deterministic report hash: 05c1a7cf4b361608871ccac7fa587540a3664552c5c4b96017f6d0bcbfa5adc6
- Build manifest hash: 6aaf1c34b437330f1ed9cf4fdc1b09b3f5c72c125381691fa4a9a07150002948
- Final gate status: required, not passed

## Underlying Alpha Build Verification

# Alpha Runnable Windows Build Verification

Stopped at:

```text
alpha_runnable_windows_build_verification
```

- Previous accepted gate: unity_runtime_export_vertical_slice_artifact_verification passed
- Final runnable gate remains required: alpha_runnable_windows_build_verification
- Unity executable discovered: true
- Unity executable path: (omitted; local machine path is not part of deterministic evidence)
- Unity version evidence: 6000.1.10f1
- Repository Unity project found: true
- Repository Unity project: unity/LLMGameCreatorAlpha
- Repository Unity build script found: true
- Repository Unity build script: unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs
- Unity command executed: true
- Unity command: (omitted; local machine paths are not part of compact deterministic root artifacts)
- Unity build log: .llmgc/procedural/unity-generated-scene-projection/logs/unity-build.log
- Build output folder: .llmgc/procedural/unity-generated-scene-projection/build/windows
- Executable relative path: LLMGameCreatorAlpha.exe
- Launch command: (omitted; local machine paths are not part of compact deterministic root artifacts)
- Launch log: .llmgc/procedural/unity-generated-scene-projection/logs/alpha-player-launch.log
- Play-loop command: (omitted; local machine paths are not part of compact deterministic root artifacts)
- Play-loop log: .llmgc/procedural/unity-generated-scene-projection/logs/alpha-player-play-loop.log
- Launch verified: true
- Play loop verified: true
- Invalid/fake/leak scenarios rejected: 14/14

Manual review steps:

1. Review the produced Windows player folder and launch log from this run.
2. Launch the produced `.exe` interactively if a manual graphics/play pass is required.
3. Verify actual play-loop behavior before marking `alpha_runnable_windows_build_verification` passed.
4. Keep `alpha_runnable_windows_build_verification` required until the deterministic play-loop evidence is reviewed.
