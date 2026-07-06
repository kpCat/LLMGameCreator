# Generic GamePackage Playable Projection Adapter

Goal123 adds a projection-only Unity Editor preview for `samples/minimal-map-game/package.json` under the accepted Alpha projection shell.

## Hands-on Verification

- Open `unity/LLMGameCreatorAlpha` in Unity.
- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.
- Click `Run Generic Package Projection Verification`.
- Verify the package title/id, map dimensions, start/player proxy, tile markers, entities, interaction details, item summary and event log.
- Do not save scenes, prefabs, ProjectSettings, Packages or StreamingAssets as part of this check.

## Cleanup Command

- After Unity checks: `.\.devflow\scripts\clean-unity-editor-noise.cmd`

## Status

- genericProjectionStatus: GREEN
- samplePackagePath: samples/minimal-map-game/package.json
- packageId: game/minimal-map-game
- packageTitle: Minimal Map Game
- mapId: map/village
- mapSize: 12x8
- unitySmokeStatus: GREEN
- projectionOnly: true
- noRuntimeProviderSchemaLuaGeneratorLibrary: true
