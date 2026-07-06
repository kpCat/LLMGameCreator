# WinForms Unity Projection Verification Runner

Goal127 adds a repo-local runner for the accepted alpha Unity projection full playthrough.
Normal verification no longer requires opening Unity manually after every goal.

## Normal Command

- `.devflow\scripts\run-unity-projection-verification.cmd`

## Optional Manual Inspection

- Open `unity/LLMGameCreatorAlpha` in Unity only when a hands-on review is needed.
- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.
- Click `Run Generic Package Full Playthrough Verification`.
- Run `.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply` after manual Unity use.

## Scope Guard

- This runner does not authorize Runtime, schema, provider, Lua, generator-library, final-art, atlas, Unity scene, prefab, ProjectSettings, Packages, StreamingAssets or release-package work.

## Status

- runnerStatus: GREEN
- runnerCommand: .devflow\scripts\run-unity-projection-verification.cmd
- lastResultPath: .llmgc/procedural/goal-127-winforms-unity-projection-verification-runner/unity-projection-verification-runner-result.json
- lastLogPath: .llmgc/procedural/goal-127-winforms-unity-projection-verification-runner/unity-batchmode-generic-full-playthrough-runner.log
- manualUnityClickingRequired: false
