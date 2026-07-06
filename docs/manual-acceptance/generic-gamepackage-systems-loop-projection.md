# Generic GamePackage Systems Loop Projection

Goal125 adds a projection-only Unity Editor systems loop over `samples/minimal-map-game/package.json`.

## Hands-on Verification

- Open `unity/LLMGameCreatorAlpha` in Unity.
- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.
- Click `Run Generic Package Systems Loop Verification`.
- Verify the systems status, inventory/resources, recipe craft result, harvest result, transaction affordability, encounter/combat preview and systems event log markers.
- Do not save scenes, prefabs, ProjectSettings, Packages or StreamingAssets as part of this check.

## Cleanup Command

- After Unity checks: `.\.devflow\scripts\clean-unity-editor-noise.cmd`

## Status

- genericSystemsStatus: GREEN
- samplePackagePath: samples/minimal-map-game/package.json
- packageId: game/minimal-map-game
- unitySmokeStatus: GREEN
- projectionOnly: true
- noRuntimeProviderSchemaLuaGeneratorLibrary: true
