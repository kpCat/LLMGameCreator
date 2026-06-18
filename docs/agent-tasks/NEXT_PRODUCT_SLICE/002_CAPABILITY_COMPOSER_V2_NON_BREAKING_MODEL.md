# Task 002: Capability Composer v2 Non-Breaking Model

## Goal

Add non-breaking model support for composable capabilities while keeping the old `selected_variant_ids` flow working.

## Allowed files

- `src/LLMGameCreator.Application/Design/GeneratorPlans/**Capability*`
- `src/LLMGameCreator.WinForms/Pages/CapabilityPicker/**`
- `tests/LLMGameCreator.Tests/**/Capability*Tests.cs`
- `docs/CAPABILITY_COMPOSER_V2_SPEC.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

## Forbidden files

- runtime implementation
- Lua executor implementation
- broad contract expansion
- `.sln`
- `*.csproj`

## Required model additions

Add optional fields to capability selection model:

```json
{
  "selected_module_ids": [],
  "selected_modifier_ids": [],
  "selected_constraint_ids": [],
  "runtime_requirement_ids": []
}
```

Rules:
- Existing `selected_variant_ids` remains valid.
- Existing saved selections still load.
- Strict LLM prompt may include these fields only when present.
- Do not require all modules to have implementation.

## Acceptance

- Old selection JSON remains valid.
- New optional module/modifier/constraint fields can be serialized/deserialized.
- Capability diagnostics can reference module/modifier/constraint ids.
- check-all passes.
