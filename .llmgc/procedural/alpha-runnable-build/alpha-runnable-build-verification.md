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
- Unity command: & "C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\endim\LLMGameCreator\.llmgc\procedural\alpha-runnable-build\unity-work\LLMGameCreatorAlpha" -executeMethod LLMGameCreatorAlpha.Editor.AlphaBuildEntrypoint.BuildWindows64 -logFile "C:\Users\endim\LLMGameCreator\.llmgc\procedural\alpha-runnable-build\logs\unity-build.log" -alphaStagingPath "C:\Users\endim\LLMGameCreator\.llmgc\procedural\alpha-runnable-build\staging" -alphaBuildOutputPath "C:\Users\endim\LLMGameCreator\.llmgc\procedural\alpha-runnable-build\build\windows"
- Unity build log: .llmgc/procedural/alpha-runnable-build/logs/unity-build.log
- Build output folder: .llmgc/procedural/alpha-runnable-build/build/windows
- Executable relative path: LLMGameCreatorAlpha.exe
- Launch command: & "C:\Users\endim\LLMGameCreator\.llmgc\procedural\alpha-runnable-build\build\windows\LLMGameCreatorAlpha.exe" -batchmode -nographics -alphaSmokeExit -alphaPlayLoopSmokeExit -alphaLogPath "C:\Users\endim\LLMGameCreator\.llmgc\procedural\alpha-runnable-build\logs\alpha-player-launch.log" -alphaPlayLoopLogPath "C:\Users\endim\LLMGameCreator\.llmgc\procedural\alpha-runnable-build\logs\alpha-player-play-loop.log"
- Launch log: .llmgc/procedural/alpha-runnable-build/logs/alpha-player-launch.log
- Play-loop command: & "C:\Users\endim\LLMGameCreator\.llmgc\procedural\alpha-runnable-build\build\windows\LLMGameCreatorAlpha.exe" -batchmode -nographics -alphaSmokeExit -alphaPlayLoopSmokeExit -alphaLogPath "C:\Users\endim\LLMGameCreator\.llmgc\procedural\alpha-runnable-build\logs\alpha-player-launch.log" -alphaPlayLoopLogPath "C:\Users\endim\LLMGameCreator\.llmgc\procedural\alpha-runnable-build\logs\alpha-player-play-loop.log"
- Play-loop log: .llmgc/procedural/alpha-runnable-build/logs/alpha-player-play-loop.log
- Launch verified: true
- Play loop verified: true
- Invalid/fake/leak scenarios rejected: 14/14

Manual review steps:

1. Review the produced Windows player folder and launch log from this run.
2. Launch the produced `.exe` interactively if a manual graphics/play pass is required.
3. Verify actual play-loop behavior before marking `alpha_runnable_windows_build_verification` passed.
4. Keep `alpha_runnable_windows_build_verification` required until the deterministic play-loop evidence is reviewed.
