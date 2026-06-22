Task id: PRODUCT_SLICE_019_UNITY_ARCHIVE_GAME_DATA_PAYLOAD_V1
Goal: materialize deterministic existing GamePackage data and category indexes inside the editor-side Unity archive without implementing Unity
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/019_UNITY_ARCHIVE_GAME_DATA_PAYLOAD_V1.md

Source docs/code read:
- AGENTS.md, README.md, docs/CONTEXT_INDEX.md, docs/CURRENT_GENERATOR_STATE.md/json and docs/ROADMAP_TO_FULL_GENERATOR.md
- Product Slice 016-018 docs, Unity archive game-data payload spec and ProductSmoke docs
- target Application/GamePackage/test csproj files, Application/Composition materialization/dry-run services, GamePackageDefinition/Domain category models, current/assembled package services, focused tests and product-smoke runner
- `src/LLMGameCreator.Application/Packages` was not found; no duplicate package abstraction was created

Patterns reused:
- Slice 018 dry-run-backed archive materialization and fixed project-local path containment
- existing package/assembly camelCase JSON serialization with readable Unicode
- deterministic UTF-8 without BOM, case-insensitive then ordinal ordering and named xUnit ProductSmoke routing

Implemented:
- Unity archive game-data payload request/result, index/category/entry/file/diagnostic models and service
- optional existing `GamePackageDefinition` input on materialization; no package schema change
- `data/game-package.json`, generated-content index and scenes/NPCs/quests/dialogues/items/encounters indexes
- extraction from existing core package and generated-content collections only, including sorted tags/linked ids and valid empty indexes
- path traversal guard, deterministic no-timestamp indexes and future metadata-only behavior when package data is absent
- `unity-archive-game-data-payload` product smoke and docs/state handoff

Non-goals preserved:
- no Unity project/runtime/build and no Runtime, GamePackageDefinition/package schema, WinForms, Scripting, Infrastructure/Generation or generator-library changes
- no solution/project/package changes, provider calls, ComfyUI/Suno integration or generator/Lua execution
- M4.1 and controlled-slice gates remain guarded; Product Slice 018 is accepted as the parent

Checks run before state update:
- UnityArchiveGameDataPayload filtered tests: passed, 5 tests including product smoke
- UnityArchiveMaterialization filtered tests: passed, 5 tests
- unity-archive-game-data-payload: passed, run 20260622_140412-product-smoke

Final guards:
- ProductSmoke filtered tests: passed, 17 tests
- unity-target-contract: passed, run 20260622_140853-product-smoke
- unity-archive-export-dry-run: passed, run 20260622_140858-product-smoke
- unity-archive-materialization: passed, run 20260622_140903-product-smoke
- check-devflow-state.ps1: passed; STOP_REVIEW preserved, 9 tasks, 2 known warnings
- check-all.ps1: passed on final files; build 0 warnings/0 errors, tests 551 passed; run 20260622_141203-check-all
- final state JSON parse: passed; M4.1 phase/milestone preserved and Product Slice 019 records Product Slice 018 as parent
- mojibake marker and UTF-8 BOM scan over all 10 changed files: passed, no markers or BOM found
