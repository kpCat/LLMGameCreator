# Unity Playable Alpha Verification

Stopped at:

```text
unity_playable_presentation_firewall_safe_build_verification
```

- Previous accepted gate: alpha_runnable_windows_build_verification passed
- Final gate remains required: unity_playable_presentation_firewall_safe_build_verification
- Unity project: unity/LLMGameCreatorAlpha
- Unity build script: unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs
- Unity build log: .llmgc/procedural/unity-playable-alpha/logs/unity-build.log
- Build output folder: .llmgc/procedural/unity-playable-alpha/build/windows
- Executable relative path: LLMGameCreatorAlpha.exe
- Launch log: .llmgc/procedural/unity-playable-alpha/logs/alpha-player-launch.log
- Play-loop log: .llmgc/procedural/unity-playable-alpha/logs/alpha-player-play-loop.log
- Movement: initial=1,1 final=2,2 blockedAt=0,2
- Interaction focus: item:item/frontier-survival/7f81222990/004
- Build options: BuildOptions.None
- Development/profiler/debug flags: development=false profiler=false debugging=false
- Firewall prompt observed: not observed by automated noninteractive run

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
- Unity build log: .llmgc/procedural/unity-playable-alpha/logs/unity-build.log
- Build output folder: .llmgc/procedural/unity-playable-alpha/build/windows
- Executable relative path: LLMGameCreatorAlpha.exe
- Launch command: (omitted; local machine paths are not part of compact deterministic root artifacts)
- Launch log: .llmgc/procedural/unity-playable-alpha/logs/alpha-player-launch.log
- Play-loop command: (omitted; local machine paths are not part of compact deterministic root artifacts)
- Play-loop log: .llmgc/procedural/unity-playable-alpha/logs/alpha-player-play-loop.log
- Launch verified: true
- Play loop verified: true
- Invalid/fake/leak scenarios rejected: 14/14

Manual review steps:

1. Review the produced Windows player folder and launch log from this run.
2. Launch the produced `.exe` interactively if a manual graphics/play pass is required.
3. Verify actual play-loop behavior before marking `alpha_runnable_windows_build_verification` passed.
4. Keep `alpha_runnable_windows_build_verification` required until the deterministic play-loop evidence is reviewed.
