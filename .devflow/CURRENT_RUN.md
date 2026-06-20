# CURRENT_RUN.md

Task id: PRODUCT_SLICE_008_ACTIVE_GENERATED_PACKAGE_FLOW_QUEST_DIALOGUE_PREVIEW
Goal: activate the assembled generated package without root package copying, then add preview-only quest/dialogue interactions
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/008_ACTIVE_GENERATED_PACKAGE_FLOW_QUEST_DIALOGUE_PREVIEW.md

Source docs read:
- AGENTS.md
- README.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/WINFORMS_DESIGNER_RULES.md
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/PRODUCT_SLICE_008_ACTIVE_GENERATED_PACKAGE_FLOW_QUEST_DIALOGUE_PREVIEW.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/008_CODEX_PROMPT.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/008_ACTIVE_GENERATED_PACKAGE_FLOW_QUEST_DIALOGUE_PREVIEW.md
- target source, project, test and smoke-runner files named by the task

Patterns reused:
- ICurrentGamePackageService.ReplaceCurrent as the narrow in-memory active-package seam
- JsonGamePackageRepository and IGamePackageValidator for assembled package load/validation
- GeneratedPackageRuntimePreviewService and GeneratedContentInteractionPreviewService as read-only projection seams
- Runtime Preview Designer split, log and selection-preserving Browser refresh
- expanded full_small_rpg_seed fixture and product-smoke scenario routing

Files changed:
- src/LLMGameCreator.Application/Projects/AssembledGamePackageActivationService.cs
- src/LLMGameCreator.Application/RuntimePreview/GeneratedQuestDialoguePreviewService.cs
- src/LLMGameCreator.Application/RuntimePreview/GeneratedContentInteractionPreviewService.cs
- src/LLMGameCreator.WinForms/Pages/ArtifactReview/ArtifactReviewPageControl.cs
- src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.cs
- src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.Designer.cs
- src/LLMGameCreator.WinForms/CompositionRoot.cs
- tests/LLMGameCreator.Tests/Application/AssembledGamePackageActivationServiceTests.cs
- tests/LLMGameCreator.Tests/Runtime/GeneratedQuestDialoguePreviewServiceTests.cs
- tests/LLMGameCreator.Tests/ProductSmoke/ActivePackageQuestDialoguePreviewSmokeTests.cs
- .devflow/scripts/run-product-smoke.ps1
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- .devflow/CURRENT_RUN.md

Implemented:
- explicit validated activation of current-project .llmgc/package-assembly/package.json
- no root package.json overwrite during activation
- Artifact Review action Use assembled package as current with clear status/diagnostics
- Runtime Preview startup from the activated assembled generated package without manual copy
- linked dialogue ids in NPC Browser details
- preview-only dialogue lines appended to the existing log
- in-memory quest start/next-step state and Quest Journal display
- active-package-quest-dialogue-preview product smoke scenario

Non-goals preserved:
- no DefaultGameRuntime rewrite or package schema change
- no real dialogue choice, quest reward/effect, inventory, combat or generated effect execution
- no LLM/provider/LM Studio, Lua, Unity, generator-library, solution or project changes
- M4.1 and STOP_REVIEW remain guarded

Checks run:
- Activation focused tests: passed, 2 tests
- QuestDialoguePreview focused tests: passed, 2 tests including product smoke by filter
- ProductSmoke focused tests: passed, 6 tests
- baseline-strict-package-assembly: passed, run 20260620_124939-product-smoke
- generated-package-runtime-preview: passed, run 20260620_124944-product-smoke
- expanded-contract-batch-smoke: passed, run 20260620_124949-product-smoke
- generated-content-interaction-preview: passed, run 20260620_124955-product-smoke
- active-package-quest-dialogue-preview: passed, run 20260620_125000-product-smoke
- check-devflow-state.ps1: passed; STOP_REVIEW preserved, 9 tasks, 2 known warnings
- check-all.ps1: passed; build 0 warnings/0 errors, tests 486 passed; run 20260620_125126-check-all
- CURRENT_GENERATOR_STATE.json parse: passed
- mojibake marker scan over all 15 changed files: passed, no markers found
- manual UI verification: not run because the Windows-control helper failed to initialize with `missing field sandboxPolicy`; headless UI construction/build coverage passed, but visual layout and click-through confirmation remain required
