# Product Slice 001: Capability Composer v2 Foundation

## Goal

Move the current Capability Picker from a technical test UI toward a real user-facing design tool.

This slice must deliver immediate value:

- Russian-readable option and feature descriptions;
- explanation panel for selected options/bundles;
- clearer compatibility diagnostics;
- non-breaking capability composition model foundation;
- tests and green check-all.

## Why this comes first

The user already hit confusion during real M4.1 evaluation:

- English dropdown labels are unclear.
- Feature bundle checkboxes are not self-explanatory.
- Some combinations are invalid without clear explanation.
- Some warnings mean “unsupported yet”, not true incompatibility.
- Progression/combat/world systems should be composable, not single-choice only.

If we skip this and go directly to package assembly, users will keep producing unclear or bad capability selections.

## Current limitation

The current selection model is mostly:

```text
selected_variant_ids:
  presentation_mode_id
  world_topology_id
  actor_model_id
  inventory_model_id
  combat_model_id
  progression_model_id
  pathfinding_profile_id
  npc_behavior_model_id

selected_feature_bundle_ids:
  [...]
```

This is good enough for M4.1 strict evaluation, but not enough for 1.0 game design.

## New foundation

Add optional non-breaking fields:

```json
{
  "selected_module_ids": [],
  "selected_modifier_ids": [],
  "selected_constraint_ids": [],
  "runtime_requirement_ids": []
}
```

These fields must default to empty and must not break old saved selections.

## User-facing help metadata

Each option/bundle should be explainable.

Minimal metadata shape:

```text
id
display_name_ru
display_name_en
short_description_ru
details_ru
examples_ru
best_for
not_recommended_with
implementation_status
diagnostic_category_hint
```

Implementation can start with an in-memory catalog, not full localization infrastructure.

## Diagnostic categories

Diagnostics shown to the user should distinguish:

```text
impossible
unsupported_yet
risky
info
```

Examples:

- `impossible`: a combination is conceptually invalid or current core axes cannot support it.
- `unsupported_yet`: the concept can make sense, but contracts/runtime/validators are not implemented yet.
- `risky`: the concept can work but may require balance/simulation checks.
- `info`: helpful non-blocking notes.

## UI behavior

The Capability Picker should show:

- selected option details;
- selected feature bundle details;
- current diagnostics with clearer category labels;
- human-readable explanation before machine ids;
- machine ids still available for technical clarity.

## Scope limits

Do not implement:

- full Design Assistant;
- package assembly;
- runtime preview;
- Lua executor;
- economy simulation;
- full balance simulator.

This is a foundation slice.

## Done

This slice is done when:

- current smoke capability selection can still be built/saved;
- user can understand why selection is valid/invalid/warning;
- selected modules/modifiers/constraints fields exist and are compatible;
- strict prompt context can include new fields when present;
- tests and check-all pass.
