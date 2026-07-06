# Generic GamePackage Full Playthrough Projection

Goal126 adds a projection-only Unity Editor full playthrough over `samples/minimal-map-game/package.json`.

## Hands-on Verification

- Open `unity/LLMGameCreatorAlpha` in Unity.
- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.
- Click `Run Generic Package Full Playthrough Verification`.
- Verify fullPlaythroughStatus, samplePackagePath, packageId, mapId, mapPathPreviewPresent, signInteractionApplied, dialogueSummaryPresent, questObjectiveStatusPresent, inventorySummaryPresent, resourceSummaryPresent, systemsSummaryPresent, combatRoundPreviewPresent, eventTranscriptPresent, unitySmokeStatus, cleanupScriptAvailable, projectionOnly, evidencePath and exportPath.
- Do not save scenes, prefabs, project settings, packages or generated player payloads as part of this check.

## Cleanup Command

- After Unity checks: `.\.devflow\scripts\clean-unity-editor-noise.cmd`

## Status

- fullPlaythroughStatus: GREEN
- samplePackagePath: samples/minimal-map-game/package.json
- packageId: game/minimal-map-game
- mapId: map/village
- unitySmokeStatus: GREEN
- projectionOnly: true
- noRuntimeProviderSchemaLuaGeneratorLibrary: true
