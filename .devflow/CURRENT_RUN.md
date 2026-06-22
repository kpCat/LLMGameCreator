Task id: PRODUCT_SLICE_020_UNITY_ARCHIVE_ASSET_AUDIO_LUA_REQUEST_PIPELINE_V1
Goal: add deterministic asset/audio/Lua request pipeline for future Unity archive without implementing Unity
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/020_REQUEST_PIPELINE.md

Source docs/code read:
- AGENTS.md, docs/CONTEXT_INDEX.md, docs/CURRENT_GENERATOR_STATE.md/json, docs/ROADMAP_TO_FULL_GENERATOR.md
- Product Slice 016-020 task/spec docs, existing Unity archive materialization and payload services/models, UnityTargetContractPresetProvider, GameDesignBrief, ProductSmoke runner and focused tests
- Application/Composition models and validation patterns

Patterns reused:
- Slice 018/019 archive materialization with fixed project-local path containment and deterministic sorted UTF-8 without BOM
- existing camelCase JSON serialization with readable Unicode and `JsonStringEnumConverter`
- named xUnit ProductSmoke routing and TempDirectory test pattern

Implemented:
- request pipeline models: `UnityArchiveAssetRequest`, `UnityArchiveAudioRequest`, `UnityArchiveLuaModuleRequest`, source ref, diagnostic, readiness and provider/asset/audio/Lua kind enums
- `UnityArchiveAssetAudioLuaRequestService` generates asset/audio/Lua requests from existing package data, design brief, target profile and runtime modules
- asset requests from generated scenes/maps, NPC portraits, item/ability/mechanic icons, tile textures, UI theme/widgets
- audio requests for UI SFX, footstep surfaces, ability/combat effects, scene ambience and music theme slots
- Lua/data module requests for inventory, quest journal, dialogue, combat, crafting, stats, world map, factions and future modules (transport/police/army)
- validation detects duplicate/blank ids, warns on future provider kinds, keeps errors non-blocking for future metadata
- `UnityArchiveMaterializationService` integrates pipeline and writes all 6 required request pipeline files
- `unity-archive-request-pipeline` product smoke and docs/state handoff

Non-goals preserved:
- no Unity project/runtime/build and no Runtime, GamePackageDefinition/package schema, WinForms, Scripting, Infrastructure/Generation or generator-library changes
- no solution/project/package changes, provider calls, ComfyUI/Suno integration, generator/Lua execution or LLM calls
- M4.1 and controlled-slice gates remain guarded; Product Slice 019 is accepted as the parent

Checks run before state update:
- UnityArchiveRequestPipeline filtered tests: passed, 7 tests
- UnityArchiveMaterialization filtered tests: passed, 5 tests
- unity-archive-request-pipeline: passed

Final guards:
- Full test suite: passed, 558 tests
- ProductSmoke filtered tests: passed, 18 tests
- Existing scenarios unity-target-contract/unity-archive-export-dry-run/unity-archive-materialization/unity-archive-game-data-payload: passed
- Build: 0 warnings/0 errors
