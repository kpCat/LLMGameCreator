Task id: PRODUCT_SLICE_020_UNITY_ARCHIVE_ASSET_AUDIO_LUA_REQUEST_PIPELINE_V1
Goal: refactor UnityArchiveAssetAudioLuaRequestService to split monolith into internal components while preserving behavior

Source docs/code read:
- AGENTS.md, docs/CONTEXT_INDEX.md, docs/CURRENT_GENERATOR_STATE.md/json
- src/LLMGameCreator.Application/Composition/UnityArchiveRequestPipelineModels.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveRequestPipelineService.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationModels.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationService.cs
- tests/LLMGameCreator.Tests/Application/UnityArchiveRequestPipelineTests.cs
- tests/LLMGameCreator.Tests/Application/UnityArchiveMaterializationTests.cs
- tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveRequestPipelineSmokeTests.cs
- .devflow/CURRENT_RUN.md

Patterns applied:
- Extracted UnityArchiveRequestBuildContext as internal context holder
- Extracted UnityArchiveAssetRequestBuilder for asset request construction
- Extracted UnityArchiveAudioRequestBuilder for audio request construction  
- Extracted UnityArchiveLuaModuleRequestBuilder for Lua module request construction
- Kept UnityArchiveRequestDiagnosticsBuilder as static helper for diagnostics
- Service acts as facade, delegates to builders

Refactored:
- UnityArchiveAssetAudioLuaRequestService reduced from 645 to ~190 lines
- Split into 4 internal helper classes:
  - UnityArchiveRequestBuildContext - context with derived booleans
  - UnityArchiveAssetRequestBuilder - scene/map/NPC/item/ability/mechanic/tile/UI asset requests
  - UnityArchiveAudioRequestBuilder - UI SFX/footstep/ability/ambience/music requests
  - UnityArchiveLuaModuleRequestBuilder - inventory/quest/dialogue/combat/crafting/stats/world_map/factions/future modules
- All request IDs, diagnostics codes, readiness semantics preserved
- Future provider warnings aggregated correctly
- Duplicate ID validation preserved

Behavior preserved:
- Request ID format: asset-request.{kind}.{normalizedId}
- Audio request ID format: audio-request.{kind}.{normalizedId}
- Lua module ID format: lua-request.{kind}
- Readiness: no diagnostics → Ready, warnings only → ReadyWithWarnings, errors → BlockedByErrors
- Diagnostic codes: request.diagnostic.future_provider_kind.asset.comfyui_future, etc.
- Deterministic output verified through existing tests

Files changed (5):
- src/LLMGameCreator.Application/Composition/UnityArchiveRequestPipelineService.cs - refactored
- tests/LLMGameCreator.Tests/Application/UnityArchiveRequestPipelineTests.cs - fixed bug in existing test + added refactor verification test

Files added (4):
- src/LLMGameCreator.Application/Composition/UnityArchiveRequestBuildContext.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveRequestDiagnosticsBuilder.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveAssetRequestBuilder.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveAudioRequestBuilder.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveLuaModuleRequestBuilder.cs

Temporary cleanup docs already removed - not found in docs/agent-tasks/NEXT_PRODUCT_SLICE/*020_CLEANUP*

Checks run:
- UnityArchiveRequestPipeline filtered tests: passed, 11 tests
- UnityArchiveMaterialization filtered tests: passed, 5 tests
- unity-archive-request-pipeline smoke: passed
- check-all: passed, 562 tests, 0 warnings