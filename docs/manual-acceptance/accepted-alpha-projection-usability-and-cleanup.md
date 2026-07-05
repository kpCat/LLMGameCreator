# Accepted Alpha Projection Usability And Cleanup

Goal120 keeps the accepted Alpha Unity projection as a manual Editor-only surface and adds usability controls plus a bounded Unity editor-noise cleanup script.

## Hands-on Verification

- Open `unity/LLMGameCreatorAlpha` in Unity.
- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.
- Click `Build/Refresh Playable Projection`.
- Use `Focus Projection Camera`, `Select Player Proxy`, `Select Next Interaction Target`, `Select Next Objective`, `Select Diagnostics Marker` and `Toggle/Refresh Legend`.
- Use `Clear Projection` to remove only `__LLMGC_AcceptedAlphaPlayableProjection__`.

## Cleanup Commands

- Dry run: `.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun`
- Apply after Unity batchmode: `.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply`
- Cmd wrapper: `.\.devflow\scripts\clean-unity-editor-noise.cmd`

## Status

- usabilityStatus: GREEN
- unitySmokeStatus: GREEN
- cleanupScriptContractPassed: true
- doNotStartAutomatically: true
