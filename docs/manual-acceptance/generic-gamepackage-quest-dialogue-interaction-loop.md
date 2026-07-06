# Generic GamePackage Quest Dialogue Interaction Loop

Goal124 adds a projection-only Unity Editor gameplay loop over `samples/minimal-map-game/package.json`.

## Hands-on Verification

- Open `unity/LLMGameCreatorAlpha` in Unity.
- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.
- Click `Run Generic Package Gameplay Loop Verification`.
- Verify the loop status, selected sign interaction, old guard dialogue, help healer objective, inventory, resources and event log markers.
- Do not save scenes, prefabs, ProjectSettings, Packages or StreamingAssets as part of this check.

## Cleanup Command

- After Unity checks: `.\.devflow\scripts\clean-unity-editor-noise.cmd`

## Status

- genericLoopStatus: GREEN
- samplePackagePath: samples/minimal-map-game/package.json
- packageId: game/minimal-map-game
- mapId: map/village
- unitySmokeStatus: GREEN
- projectionOnly: true
- noRuntimeProviderSchemaLuaGeneratorLibrary: true
