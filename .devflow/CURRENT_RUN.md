# CURRENT_RUN.md

Task id: PRODUCT_SLICE_002_COMPOSABLE_MODULE_SELECTION_UI
Goal: wire composable module/modifier/constraint/runtime requirement selection into Capability Picker without starting M5/M6/package assembly work
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/002_COMPOSABLE_MODULE_SELECTION_UI.md

Source docs read:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/CAPABILITY_COMPOSER_V2_SPEC.md
- docs/CAPABILITY_COMPOSER_V2_RU_GLOSSARY.md
- docs/PRODUCT_SLICE_001_CAPABILITY_COMPOSER_V2_FOUNDATION.md
- docs/PRODUCT_SLICE_002_COMPOSABLE_MODULE_SELECTION_UI.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/002_CODEX_PROMPT.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/002_COMPOSABLE_MODULE_SELECTION_UI.md
- src/LLMGameCreator.Application/LLMGameCreator.Application.csproj
- src/LLMGameCreator.WinForms/LLMGameCreator.WinForms.csproj
- tests/LLMGameCreator.Tests/LLMGameCreator.Tests.csproj
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilityHelpCatalog.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionModels.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionService.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionArtifactService.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactPromptBuilder.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPageControl.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPresenter.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerViewModels.cs
- tests/LLMGameCreator.Tests/Design/GeneratorPlanCapabilitySelectionServiceTests.cs
- tests/LLMGameCreator.Tests/WinForms/CapabilityPickerPresenterTests.cs
- tests/LLMGameCreator.Tests/Docs/CurrentGeneratorStateDocsTests.cs
- .devflow/CURRENT_RUN.md

Existing patterns inspected:
- Capability Picker uses a single WinForms UserControl with in-file TableLayoutPanel/SplitContainer layout and runtime event wiring.
- CapabilityPickerPresenter is the local mapping seam for atlas/help catalog/view-state/request/load behavior.
- GeneratorPlanCapabilityHelpCatalog already owns the in-memory composable seed catalog.
- GeneratorPlanCapabilitySelectionService already preserves selected composable arrays in result JSON and selection ids.
- GeneratorPlanStrictLlmArtifactPromptBuilder already appends selected arrays only when non-empty.

Files changed:
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilityHelpCatalog.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionModels.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionService.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerViewModels.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPresenter.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPageControl.cs
- tests/LLMGameCreator.Tests/Design/GeneratorPlanCapabilitySelectionServiceTests.cs
- tests/LLMGameCreator.Tests/WinForms/CapabilityPickerPresenterTests.cs
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- .devflow/CURRENT_RUN.md

Implemented:
- Added selectable Capability Picker groups for feature bundles, modules, modifiers, constraints and runtime requirements via a stable tabbed checklist area.
- Extended CapabilityPickerViewState and presenter request/load mappings for selected_module_ids, selected_modifier_ids, selected_constraint_ids and runtime_requirement_ids.
- Populated composable UI groups from GeneratorPlanCapabilityHelpCatalog.ListCompositionSeeds().
- Added initial constraint and runtime requirement seeds to the in-memory catalog.
- Kept readable Russian labels first in list items and machine ids in help/details.
- Preserved required core atlas planning feature bundle behavior.
- Added non-fatal diagnostics for unsupported-yet economy/balance/chunk choices, unknown future composable ids and hybrid realtime/turn combat info.
- Preserved strict prompt compactness when selected arrays are empty; existing prompt builder includes arrays when non-empty.
- Updated current generator state handoff to Product Slice 002 while keeping current_phase m4_1_real_model_evaluation_gate and last_completed_milestone M4.1.

Non-goals preserved:
- No runtime, scripting, GamePackage, generator-library, solution or project files changed.
- No devflow scripts, NEXT_TASK or task queue changed.
- No Strict LLM Artifacts UI or LLM Evaluation UI changed.
- No package assembly, Lua executor, runtime preview, M5, M6 or M6-lite implementation started.

Checks run:
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Capability": passed. 31 passed, 0 failed.
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~CapabilityPicker": passed. 6 passed, 0 failed.
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1: passed. Output: "Devflow state check passed. Current mode: stop (STOP_REVIEW). Tasks: 9. Known warnings: 2."
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1: passed. Build: 0 warnings, 0 errors. Tests: 457 passed, 0 failed. Run directory: .devflow\runs\20260619_124455-check-all.
- Mojibake marker scan over changed files with rg: passed, no markers found.

Manual verification:
- Not run interactively in this note. Required manual UI workflow remains: start WinForms, open Capability Picker, load atlas, select several progression/combat/world/economy/balance modules, build selection, save latest selection, open LLM Artifacts, load selection and preview prompt.
