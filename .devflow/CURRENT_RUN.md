# CURRENT_RUN.md

Task id: PRODUCT_SLICE_006_1_LLM_ARTIFACTS_BATCH_PRESET_DROPDOWN
Goal: expose existing strict LLM batch presets in LLM Artifacts and apply them to the current contract checkbox selection
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/006_1_LLM_ARTIFACTS_BATCH_PRESET_DROPDOWN.md

Source docs read:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/WINFORMS_DESIGNER_RULES.md
- docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/006_1_CODEX_PROMPT.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/006_1_LLM_ARTIFACTS_BATCH_PRESET_DROPDOWN.md
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactContractCatalog.cs
- src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/StrictLlmArtifactsPageControl.cs
- src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/StrictLlmArtifactsPresenter.cs
- src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/StrictLlmArtifactsViewModels.cs
- src/LLMGameCreator.WinForms/Pages/StrictLlmEvaluation/StrictLlmEvaluationPageControl.cs
- src/LLMGameCreator.WinForms/LLMGameCreator.WinForms.csproj
- tests/LLMGameCreator.Tests/WinForms/StrictLlmArtifactsPresenterTests.cs
- tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmArtifactValidatorTests.cs
- tests/LLMGameCreator.Tests/LLMGameCreator.Tests.csproj

Patterns reused:
- GeneratorPlanStrictLlmArtifactContractCatalog.ListBatchPresets/TryGetBatchPreset
- presenter-owned state mapping from StrictLlmArtifacts
- state-safe ComboBox/event handling and ordered CheckedListBox projection from StrictLlmEvaluation

Files changed:
- src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/StrictLlmArtifactsPageControl.cs
- src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/StrictLlmArtifactsPresenter.cs
- src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/StrictLlmArtifactsViewModels.cs
- tests/LLMGameCreator.Tests/WinForms/StrictLlmArtifactsPresenterTests.cs
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- .devflow/CURRENT_RUN.md

Implemented:
- default manual/custom option plus all five existing catalog batch presets on LLM Artifacts
- presenter mapping for ListBatchPresets and preset resolution through TryGetBatchPreset
- exact checkbox projection in the existing contract display order
- non-throwing unknown-preset status with unchanged contract selection
- manual checkbox override remains available; Preview/Generate still use current checked ids through the unchanged BuildRequest path
- focused presenter coverage for catalog options, baseline/full selections, unknown selection and manual request construction

Non-goals preserved:
- no LLM/provider/LM Studio calls in tests or smoke
- no contract, Application catalog, package assembly, runtime, Lua, generator-library, solution or project changes
- no broad page rewrite or new early splitter sizing

Checks run:
- StrictLlmArtifacts focused test: passed, 7 tests
- StrictLlm focused test: passed, 66 tests
- expanded-contract-batch-smoke: passed, run 20260620_113818-product-smoke
- check-devflow-state.ps1: passed; STOP_REVIEW preserved, 9 tasks, 2 known warnings
- check-all.ps1: passed; build 0 warnings/0 errors, tests 481 passed; run 20260620_113946-check-all
- CURRENT_GENERATOR_STATE.json parse: passed
- mojibake marker scan over all 7 changed files: passed, no markers found
- manual UI verification: not run in this headless execution; manual steps remain required for visual layout and click-through confirmation
