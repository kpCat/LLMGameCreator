# CURRENT_RUN.md

Task id: PRODUCT_SLICE_011_GAME_BLUEPRINT_CAPABILITY_GRAPH
Goal: add a machine-readable GameBlueprint, built-in capability registry, deterministic compatibility validation and headless product smoke
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/011_GAME_BLUEPRINT_CAPABILITY_GRAPH.md

Source docs read:
- AGENTS.md
- README.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/LLMGAMECREATOR_OFFICIAL_PRODUCT_PLAN.md
- docs/GAME_ASSEMBLY_WORKBENCH_ARCHITECTURE.md
- docs/CAPABILITY_GRAPH_AND_GENERATOR_CATALOG_PLAN.md
- docs/PRODUCT_SLICE_011_GAME_BLUEPRINT_CAPABILITY_GRAPH.md
- docs/GAME_BLUEPRINT_CAPABILITY_GRAPH_SPEC.md
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/011_CODEX_PROMPT.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/011_GAME_BLUEPRINT_CAPABILITY_GRAPH.md
- target Application/test csproj, ContentLanguagePolicy, capability-selection/atlas analogs, ProductSmoke tests and runner files named by the task

Patterns reused:
- immutable record/default-value style from ContentLanguagePolicy
- case-insensitive registry lookup, stable machine-readable diagnostic codes and sorted diagnostics from Atlas/Capability Selection
- named ProductSmoke test plus run-product-smoke.ps1 scenario routing

Implemented:
- GameBlueprint plus game kind, world source, presentation and generation-mode enums
- CapabilityDefinition with requires, optional requires, provides, conflicts, supported blueprint dimensions, runtime cost and maturity
- built-in registry with 15 current and 7 planned capabilities
- deterministic validator for duplicate/unknown ids, missing/optional requirements, direct conflicts, blueprint-dimension mismatch and planned maturity
- compatible baseline generated RPG preview preset and two diagnostic imported-map future presets
- game-blueprint-capability-compatibility product smoke without LLM/provider calls

Non-goals preserved:
- no Runtime, WinForms, GamePackageDefinition, package schema, Scripting, Infrastructure/Generation or generator-library changes
- no solution/project/package changes, dynamic plugins, semantic world model or imported-map implementation
- no LLM/provider calls
- M4.1 and controlled-slice gates remain guarded

Checks run before state handoff:
- Capability focused tests: passed, 40 tests
- GameBlueprint focused tests: passed, 9 tests
- ProductSmoke focused tests: passed, 9 tests
- baseline-strict-package-assembly: passed, run 20260621_154631-product-smoke
- generated-package-runtime-preview: passed, run 20260621_154636-product-smoke
- expanded-contract-batch-smoke: passed, run 20260621_154642-product-smoke
- generated-content-interaction-preview: passed, run 20260621_154647-product-smoke
- active-package-quest-dialogue-preview: passed, run 20260621_154652-product-smoke
- generated-map-placement-preview: passed, run 20260621_154658-product-smoke
- content-language-policy: passed, run 20260621_154703-product-smoke
- game-blueprint-capability-compatibility: passed, run 20260621_154709-product-smoke

Final guards:
- check-devflow-state.ps1: passed; STOP_REVIEW preserved, 9 tasks, 2 known warnings
- check-all.ps1: passed; build 0 warnings/0 errors, tests 504 passed; run 20260621_155007-check-all
- CURRENT_GENERATOR_STATE.json parse: passed; M4.1 phase/milestone preserved and Product Slice 011 recorded with Product Slice 010 as parent
- mojibake marker scan over all 12 changed files: passed, no markers found
- manual UI verification: not required; no WinForms files changed
