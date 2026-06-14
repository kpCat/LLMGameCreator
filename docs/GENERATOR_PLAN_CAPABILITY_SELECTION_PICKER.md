# Generator Plan Capability Selection Picker

Status: M3 implementation guide

The Capability Picker is the bounded planning/control layer between broad game intent and later generator plans. It turns explicit game shape choices into a deterministic selection artifact:

```text
variant ids
  -> resolved feature bundles
  -> required artifact contracts
  -> required validators
  -> runtime targets
  -> prompt context templates
  -> capability gaps and warnings
```

It does not generate game content, execute Lua, call an LLM/provider, mutate `GamePackage`, export packages, change package schema or change Design DB schema.

## Why Variant IDs Come First

Full generation must not start from vague requests such as "make an RPG". Before any prompt, Lua module, validator, runtime preview or assembly task runs, the editor needs concrete ids for:

- presentation mode;
- world topology;
- actor model;
- inventory model;
- combat model;
- progression model;
- pathfinding profile;
- NPC behavior model;
- runtime target;
- feature bundles.

These ids come from the M1.1 taxonomy files:

- `generator-library/atlas/game_form_factor_taxonomy.json`;
- `generator-library/atlas/game_system_variant_taxonomy.json`;
- `generator-library/atlas/feature_bundle_map.json`;
- `generator-library/atlas/capability_atlas.json`;
- `generator-library/atlas/artifact_contracts.json`.

The picker validates presentation/world compatibility, warns when actor or combat choices are not recommended by the selected presentation mode, resolves selected feature bundles, and reports future contracts or gaps as warnings unless a selected id is unknown or explicitly incompatible.

## Persisted Artifact

Saving the picker result writes the latest generated artifact through the existing generated artifact repository:

```text
id: artifact/generator_plan_capability_selection/latest
kind: generator_plan.capability_selection
path: .llmgc/generator-plans/generator_plan_capability_selection.json
```

The JSON payload includes:

- `schema_version`;
- `selection_id`;
- title and purpose;
- selected variant ids;
- selected feature bundle ids;
- selected runtime targets;
- resolved capability ids;
- resolved artifact contracts;
- resolved validators;
- resolved prompt context templates;
- resolved runtime targets;
- required Lua modules or capability gaps;
- warnings, errors and generation timestamp.

Diagnostics are also saved as validation results for the artifact. This reuses the existing Design DB tables and does not require a schema migration.

## UI Workflow

The WinForms page is `Capability Picker`.

Typical flow:

1. Load or override the atlas root.
2. Pick all required variant ids.
3. Pick one runtime target.
4. Check feature bundles.
5. Build selection.
6. Review diagnostics and resolved lists.
7. Save latest selection when there are no errors.
8. Load latest selection later to restore chosen ids.

Warnings do not block saving. Errors block saving.

## Future Consumers

Future generator-plan tasks should read `artifact/generator_plan_capability_selection/latest` before selecting generator steps, prompt context templates, Lua module manifests or artifact contracts. The artifact is an input to later planning, not a package mutation or generation output by itself.

Consumers should:

- preserve the selected ids;
- use resolved contracts to choose generator plan steps;
- use resolved validators as validation gates;
- use prompt context templates to keep LLM context bounded;
- turn capability gaps into explicit library growth proposals instead of inventing untracked implementation glue.

## Non-Goals

- no LLM/provider calls;
- no Lua execution;
- no GamePackage mutation;
- no package export;
- no runtime preview;
- no Unity/export IR implementation;
- no schema migration;
- no hidden default genre activation;
- no implicit adult/NSFW overlay activation.
