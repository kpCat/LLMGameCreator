# CURRENT_RUN.md

Task id: PRODUCT_SLICE_009_GENERATED_NPC_ENCOUNTER_MAP_PLACEMENT
Goal: place generated NPCs and encounters on the Runtime Preview map as deterministic preview markers
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/009_GENERATED_NPC_ENCOUNTER_MAP_PLACEMENT.md

Source docs read:
- AGENTS.md
- README.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/WINFORMS_DESIGNER_RULES.md
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/PRODUCT_SLICE_009_GENERATED_NPC_ENCOUNTER_MAP_PLACEMENT.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/009_CODEX_PROMPT.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/009_GENERATED_NPC_ENCOUNTER_MAP_PLACEMENT.md
- target source, project, test and smoke-runner files named by the task

Patterns reused:
- GeneratedPackageRuntimePreviewService as the read-only package/state projection seam
- GeneratedContentInteractionPreviewService as the Browser/details/reference seam
- Runtime Preview Designer split and RuntimeMapCanvas overlay rendering
- expanded full_small_rpg_seed fixture and named product-smoke scenario routing

Files changed:
- src/LLMGameCreator.Application/RuntimePreview/GeneratedMapPlacementPreviewService.cs
- src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimeMapCanvas.cs
- src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.cs
- src/LLMGameCreator.WinForms/CompositionRoot.cs
- tests/LLMGameCreator.Tests/Runtime/GeneratedMapPlacementPreviewServiceTests.cs
- tests/LLMGameCreator.Tests/WinForms/RuntimePreviewMapPlacementTests.cs
- tests/LLMGameCreator.Tests/ProductSmoke/GeneratedMapPlacementPreviewSmokeTests.cs
- .devflow/scripts/run-product-smoke.ps1
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- .devflow/CURRENT_RUN.md

Implemented:
- deterministic NPC and encounter preview markers from generatedContent
- scene -> PackageMapId resolution, region-linked scene fallback and diagnostic current/start map fallback
- stable in-bounds placement that prefers walkable tiles and avoids player/start plus marker overlap when possible
- green NPC circles and orange-red encounter diamonds alongside the existing blue player marker
- Browser details and Append selected to log marker map, position, refs and linked NPC dialogue information
- generated-map-placement-preview product smoke scenario

Non-goals preserved:
- no DefaultGameRuntime rewrite or package schema change
- no combat, dialogue choice, encounter outcome, inventory, quest reward or generated effect execution
- no LLM/provider/LM Studio, Lua, Unity, generator-library, solution or project changes
- M4.1 and STOP_REVIEW remain guarded

Checks run:
- MapPlacement focused tests: passed, 4 tests
- ProductSmoke focused tests: passed, 7 tests
- baseline-strict-package-assembly: passed, run 20260621_134006-product-smoke
- generated-package-runtime-preview: passed, run 20260621_134011-product-smoke
- expanded-contract-batch-smoke: passed, run 20260621_134016-product-smoke
- generated-content-interaction-preview: passed, run 20260621_134022-product-smoke
- active-package-quest-dialogue-preview: passed, run 20260621_134027-product-smoke
- generated-map-placement-preview: passed, run 20260621_134033-product-smoke
- check-devflow-state.ps1: passed; STOP_REVIEW preserved, 9 tasks, 2 known warnings
- check-all.ps1: passed; build 0 warnings/0 errors, tests 490 passed; run 20260621_134126-check-all
- CURRENT_GENERATOR_STATE.json parse: passed; M4.1 phase/milestone preserved and Product Slice 009 recorded
- mojibake marker scan over all 12 changed files: passed, no markers found
- manual UI verification: not run; headless canvas rendering, page construction/build and all product smoke coverage passed, but visual layout and click-through confirmation remain required
