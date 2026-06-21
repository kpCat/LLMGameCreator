# CURRENT_RUN.md

Task id: PRODUCT_SLICE_010_OFFICIAL_PLAN_CONTENT_LANGUAGE_POLICY
Goal: preserve the official Game Assembly Workbench plan and add project-scoped content language policy for future strict LLM artifact generation
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/010_OFFICIAL_PLAN_CONTENT_LANGUAGE_POLICY.md

Source docs read:
- AGENTS.md
- README.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/WINFORMS_DESIGNER_RULES.md
- docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/LLMGAMECREATOR_OFFICIAL_PRODUCT_PLAN.md
- docs/GAME_ASSEMBLY_WORKBENCH_ARCHITECTURE.md
- docs/CAPABILITY_GRAPH_AND_GENERATOR_CATALOG_PLAN.md
- docs/PRODUCT_SLICE_010_OFFICIAL_PLAN_CONTENT_LANGUAGE_POLICY.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/010_CODEX_PROMPT.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/010_OFFICIAL_PLAN_CONTENT_LANGUAGE_POLICY.md
- target project, strict-generation, project persistence, validation, WinForms presenter/page, ProductSmoke and runner files named by the task

Patterns reused:
- JsonAppSettingsRepository async camelCase JSON save/load and default-on-missing behavior
- StrictLlmArtifactsPresenter request construction and prompt-preview seam
- GeneratorPlanStrictLlmEvaluationService non-blocking generated-text quality diagnostics
- named ProductSmoke test plus run-product-smoke.ps1 scenario routing
- mandatory WinForms UserControl / Designer split

Implemented:
- official Game Assembly Workbench plan pack retained as repository source material
- ru/uk/en ContentLanguagePolicy with Russian default and ASCII/kebab_case technical id policy
- project persistence at .llmgc/settings/content-language-policy.json with in-memory fallback when no project is open
- Designer-safe LLM Artifacts content language selector
- selected-language instruction in strict initial and bounded repair prompts
- warning-only heuristic for obvious English player-facing prose under ru/uk, excluding technical ids
- content-language-policy unit and product-smoke coverage without an LLM/provider call

Non-goals preserved:
- no translation engine or rewrite of existing artifacts
- no runtime, GamePackageDefinition, package schema, Lua, generator-library, solution or project changes
- no real LLM/provider calls in tests
- M4.1 and controlled-slice gates remain guarded

Checks run:
- ContentLanguage focused tests: passed, 5 tests
- ProductSmoke focused tests: passed, 8 tests
- baseline-strict-package-assembly: passed, run 20260621_151412-product-smoke
- generated-package-runtime-preview: passed, run 20260621_151418-product-smoke
- expanded-contract-batch-smoke: passed, run 20260621_151423-product-smoke
- generated-content-interaction-preview: passed, run 20260621_151428-product-smoke
- active-package-quest-dialogue-preview: passed, run 20260621_151433-product-smoke
- generated-map-placement-preview: passed, run 20260621_151438-product-smoke
- content-language-policy: passed, run 20260621_151443-product-smoke
- check-devflow-state.ps1: passed; STOP_REVIEW preserved, 9 tasks, 2 known warnings
- check-all.ps1: passed; build 0 warnings/0 errors, tests 495 passed; run 20260621_151613-check-all
- CURRENT_GENERATOR_STATE.json parse: passed; M4.1 phase/milestone preserved and Product Slice 010 recorded
- mojibake marker scan over all 18 changed files: passed, no markers found
- manual UI verification: not run; headless build, presenter/request tests and product smoke passed, but Visual Studio Designer opening and click-through confirmation remain required
