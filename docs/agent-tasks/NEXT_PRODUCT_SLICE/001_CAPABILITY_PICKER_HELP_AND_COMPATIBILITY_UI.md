# Task 001: Capability Picker Help and Compatibility UI

## Goal

Make the existing Capability Picker understandable without rewriting the full capability selection model.

This is a product usability slice, not a broad architecture rewrite.

## Allowed files

- `src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPageControl.cs`
- `src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPresenter.cs`
- `src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerViewModels.cs`
- `src/LLMGameCreator.Application/Design/GeneratorPlans/**Capability*`
- `tests/LLMGameCreator.Tests/**/Capability*Tests.cs`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

## Forbidden files

- `.sln`
- `*.csproj`
- `src/LLMGameCreator.Runtime*/**`
- `src/LLMGameCreator.Scripting/**`
- `generator-library/**`
- M5/M6/M8/M9/M10 production implementation files

## Exact behavior

1. Add a details/help panel to Capability Picker.
2. When the user selects an axis option or feature bundle, show:
   - readable name
   - machine id
   - short description
   - examples
   - best used for
   - compatibility notes
   - implementation status: supported / unsupported_yet / risky / future
3. Add Russian-friendly display names for the most visible current options.
4. Do not translate machine-readable ids or enum values.
5. Categorize diagnostics:
   - error/impossible
   - warning/unsupported_yet
   - warning/risky
   - info
6. Existing Build selection / Save latest selection behavior must continue working.
7. Do not introduce composable progression/combat model yet. Only prepare the UI/help layer.

## Acceptance

- User can understand why `Map And Panel RPG + Infinite Chunks` is invalid or discouraged.
- User can understand that some warnings are “not implemented yet”, not conceptual impossibility.
- Existing smoke flow still works:
  - Capability Picker
  - LLM Artifacts
  - LLM Evaluation
- check-all passes.

## Tests

Add/adjust small tests where practical:
- display metadata lookup for a known option;
- diagnostic category mapping;
- no translation of machine ids.

## Commands

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Capability"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Final report

Report in Russian:
- files read
- files changed
- UI behavior added
- which labels/help entries were added
- tests/checks
- remaining gaps
