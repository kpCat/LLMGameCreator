# CURRENT_RUN.md

Task id: PRODUCT_SLICE_001_CAPABILITY_PICKER_UX_REPAIR
Goal: repair Capability Picker usability after Capability Composer v2 foundation without starting M5/M6 work
Task source: C:\Users\endim\.codex\attachments\fd3b125e-1496-4f5b-9cae-5f1695cad9e2\pasted-text.txt

Source docs read:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/GENERATOR_PLAN_CAPABILITY_SELECTION_PICKER.md
- docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
- docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
- docs/WINFORMS_DESIGNER_RULES.md
- src/LLMGameCreator.Application/LLMGameCreator.Application.csproj
- src/LLMGameCreator.WinForms/LLMGameCreator.WinForms.csproj
- tests/LLMGameCreator.Tests/LLMGameCreator.Tests.csproj
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilityHelpCatalog.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionModels.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionService.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionAtlasReader.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPageControl.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPresenter.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerViewModels.cs
- tests/LLMGameCreator.Tests/Design/GeneratorPlanCapabilitySelectionServiceTests.cs
- tests/LLMGameCreator.Tests/WinForms/CapabilityPickerPresenterTests.cs
- .devflow/CURRENT_RUN.md

Existing patterns inspected:
- Capability Picker reads atlas data and builds a deterministic capability selection artifact through Application services.
- WinForms page uses the existing single UserControl with TableLayoutPanel, SplitContainer, Dock and runtime event wiring in the main .cs file.
- Capability Picker presenter is the local mapping seam for atlas -> view-state display names, diagnostics and selected defaults.
- Strict LLM Artifacts and LLM Evaluation consume the saved latest selection artifact; this repair must not change that JSON compatibility.

Files changed:
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilityHelpCatalog.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerViewModels.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPresenter.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPageControl.cs
- tests/LLMGameCreator.Tests/Design/GeneratorPlanCapabilitySelectionServiceTests.cs
- tests/LLMGameCreator.Tests/WinForms/CapabilityPickerPresenterTests.cs
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- .devflow/CURRENT_RUN.md

Implemented:
- Repaired Capability Picker layout for normal editor sizes: top atlas/title/purpose, middle core axes, bottom left feature/help panel and right build/results/diagnostics panel.
- Added SplitContainer minimum sizes and wrapped action buttons so controls do not visually overlap at ordinary widths.
- Replaced main visible axis labels/actions with Russian labels while keeping machine ids visible in combo/list option text and help details.
- Added atlas-based fallback help for visible options and feature bundles when curated metadata is missing; fallback includes id, title/purpose, domain/category and counts.
- Rewrote core atlas planning help as an obligatory technical generation base, removed "M4 flow" wording, auto-selects it by default and prevents unchecking in the UI.
- Added Russian diagnostic category meanings for impossible, unsupported_yet, risky and info, shown in diagnostic rows/help.
- Preserved old selection JSON compatibility and did not add full module selection UI.

Non-goals preserved:
- No runtime, scripting, GamePackage, generator-library, solution or project files changed.
- No devflow scripts changed.
- No Strict LLM Artifacts UI or LLM Evaluation UI changed.
- No package assembly, Lua executor, runtime preview, M5, M6 or M6-lite implementation started.

Checks run:
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~CapabilityPicker": passed. 5 passed, 0 failed.
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Capability": passed. 29 passed, 0 failed.
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1: passed. Output: "Devflow state check passed. Current mode: stop (STOP_REVIEW). Tasks: 9. Known warnings: 2."
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1: passed. Build: 0 warnings, 0 errors. Tests: 455 passed, 0 failed. Run directory: .devflow\runs\20260618_231719-check-all.
- Mojibake marker scan over changed files with rg: passed, no markers found.

Manual verification:
- Not run interactively in this note. Required manual UI workflow remains: start WinForms, open Capability Picker, load atlas, check normal-size layout, click dropdowns and feature bundles, build a valid selection, save latest selection, then verify LLM Artifacts can load latest selection.
