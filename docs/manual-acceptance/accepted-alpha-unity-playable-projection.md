# Accepted Alpha Unity Playable Projection

Goal119 creates a Unity Editor entrypoint that builds a temporary primitive projection over the accepted Alpha baseline.

## Hands-on Verification

- Open `unity/LLMGameCreatorAlpha` in Unity.
- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.
- Click `Build/Refresh Playable Projection`.
- The current scene should contain `__LLMGC_AcceptedAlphaPlayableProjection__` with a player proxy, map markers, chunk/window diagnostics, interaction targets, objectives and smoke diagnostics.
- Use `Clear Projection` to remove only the generated root object.

## Boundaries

Goal119 is not final release and does not authorize live geodata, providers, Runtime, schema, Lua, generator-library, final art, atlas, Unity scene/prefab/project-settings/StreamingAssets changes or release packaging.

## Status

- projectionStatus: GREEN
- qualityGatePassed: true
- unityMenuPath: LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection
- acceptedBaselineReady: true
