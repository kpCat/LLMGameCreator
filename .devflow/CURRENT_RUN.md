# CURRENT_RUN.md

Task id: PRODUCT_SLICE_007_GENERATED_CONTENT_INTERACTION_PREVIEW
Goal: add a selectable, read-only generated-content browser to Runtime Preview without changing runtime execution
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/007_GENERATED_CONTENT_INTERACTION_PREVIEW.md

Source docs read:
- AGENTS.md
- README.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/WINFORMS_DESIGNER_RULES.md
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/PRODUCT_SLICE_007_GENERATED_CONTENT_INTERACTION_PREVIEW.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/007_CODEX_PROMPT.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/007_GENERATED_CONTENT_INTERACTION_PREVIEW.md
- source, project, test and smoke-runner files named by the task

Patterns reused:
- GeneratedPackageRuntimePreviewService as the package/state read-only projection seam
- RuntimePreviewPageControl Designer split and safe splitter initialization
- existing Runtime Preview log and refresh flow after Start/commands
- expanded full_small_rpg_seed fixture and product-smoke runner scenario routing

Files changed:
- src/LLMGameCreator.Application/RuntimePreview/GeneratedPackageRuntimePreviewService.cs
- src/LLMGameCreator.Application/RuntimePreview/GeneratedContentInteractionPreviewService.cs
- src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.cs
- src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.Designer.cs
- src/LLMGameCreator.WinForms/CompositionRoot.cs
- tests/LLMGameCreator.Tests/ProductSmoke/GeneratedContentInteractionPreviewSmokeTests.cs
- .devflow/scripts/run-product-smoke.ps1
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- .devflow/CURRENT_RUN.md

Implemented:
- read-only interaction catalog categories for current scene, regions, NPCs, items, dialogues, quests, mechanics, encounters, applied artifacts and warnings
- details with ids, descriptions, references, dialogue lines, quest steps/objectives, mechanic tags and provenance/hash
- Generated Content Browser and Summary tabs with category/entry selection and read-only details
- non-destructive Append selected to log action
- refresh after Start/commands with valid selection preservation when ids still exist
- generated-content-interaction-preview product smoke scenario using expanded deterministic fixtures

Non-goals preserved:
- no DefaultGameRuntime rewrite or package schema change
- no dialogue/combat/inventory/quest/encounter simulation or generated effect execution
- no LLM/provider/LM Studio, Lua, Unity, generator-library, solution or project changes
- M4.1 and STOP_REVIEW remain guarded

Checks run:
- InteractionPreview focused test: passed, 1 test
- ProductSmoke focused test: passed, 5 tests
- baseline-strict-package-assembly: passed, run 20260620_115519-product-smoke
- generated-package-runtime-preview: passed, run 20260620_115527-product-smoke
- expanded-contract-batch-smoke: passed, run 20260620_115536-product-smoke
- generated-content-interaction-preview: passed, run 20260620_115544-product-smoke
- check-devflow-state.ps1: passed; STOP_REVIEW preserved, 9 tasks, 2 known warnings
- check-all.ps1: passed; build 0 warnings/0 errors, tests 482 passed; run 20260620_115606-check-all
- CURRENT_GENERATOR_STATE.json parse: passed
- mojibake marker scan over all 11 changed files: passed, no markers found
- manual UI verification: not run because the Windows-control helper failed to initialize with `missing field sandboxPolicy`; visual layout and click-through confirmation remain required
