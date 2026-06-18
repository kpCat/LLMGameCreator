# CURRENT_RUN.md

Task id: NEXT_PRODUCT_SLICE_001_CAPABILITY_COMPOSER_V2_FOUNDATION
Goal: implement Capability Composer v2 foundation without starting M5/M6 work
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/001_CAPABILITY_COMPOSER_V2_FOUNDATION.md

Source docs read:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/CAPABILITY_COMPOSER_V2_SPEC.md
- docs/CAPABILITY_COMPOSER_V2_RU_GLOSSARY.md
- docs/PRODUCT_SLICE_001_CAPABILITY_COMPOSER_V2_FOUNDATION.md
- docs/GENERATOR_PLAN_CAPABILITY_SELECTION_PICKER.md
- docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/001_CAPABILITY_COMPOSER_V2_FOUNDATION.md
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPageControl.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPresenter.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerViewModels.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionModels.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionService.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactPromptBuilder.cs
- tests/LLMGameCreator.Tests/Design/GeneratorPlanCapabilitySelectionServiceTests.cs
- tests/LLMGameCreator.Tests/WinForms/CapabilityPickerPresenterTests.cs

Existing patterns inspected:
- Capability Picker reads atlas data and builds a deterministic capability selection artifact through Application services.
- WinForms page calls Application services and keeps runtime logic out of Designer files in the existing page style.
- Strict LLM prompt builder reads only the latest capability selection summary and bounded contract schema.
- Capability tests cover service behavior, artifact save/read and presenter mapping.

Files changed:
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionModels.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilityHelpCatalog.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionService.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionArtifactService.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactPromptBuilder.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerViewModels.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPresenter.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPageControl.cs
- tests/LLMGameCreator.Tests/Design/GeneratorPlanCapabilitySelectionServiceTests.cs
- tests/LLMGameCreator.Tests/WinForms/CapabilityPickerPresenterTests.cs
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- .devflow/CURRENT_RUN.md

Implemented:
- Added a non-breaking Capability Composer v2 foundation with optional arrays:
  - selected_module_ids
  - selected_modifier_ids
  - selected_constraint_ids
  - runtime_requirement_ids
- Added an in-memory Russian help metadata catalog for current high-impact axes, runtime targets and feature bundles.
- Added minimal composition seed metadata for future modules, modifiers, constraints and runtime requirements.
- Added diagnostic categories:
  - impossible
  - unsupported_yet
  - risky
  - info
- Added a Capability Picker help/details panel for selected variants, feature bundles and diagnostics.
- Kept machine ids visible in option labels and help text.
- Updated strict prompt context to include optional composable arrays only when non-empty.

Non-goals preserved:
- No runtime, scripting, GamePackage, generator-library, solution or project files changed.
- No devflow scripts changed.
- No Strict LLM Artifacts UI or LLM Evaluation UI changed.
- No package assembly, Lua executor, runtime preview, M5, M6 or M6-lite implementation started.

Checks run:
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Capability": passed. 23 passed, 0 failed.
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --no-build --filter "FullyQualifiedName~CurrentGeneratorState": passed. 10 passed, 0 failed.
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1: passed. Output: "Devflow state check passed. Current mode: stop (STOP_REVIEW). Tasks: 9. Known warnings: 2."
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1: passed. Build: 0 warnings, 0 errors. Tests: 449 passed, 0 failed. Run directory: .devflow\runs\20260618_225335-check-all.
- Mojibake marker scan over changed files with rg: passed, no markers found.

Manual verification:
- Not run yet in this note. Required manual UI workflow remains: start WinForms, open Capability Picker, load atlas, choose Map And Panel RPG + Region Graph, build/save selection, then verify LLM Artifacts can load latest selection.
