# Product Slice 006.1: LLM Artifacts Batch Preset Dropdown

## Goal

Make the Slice 006 batch presets usable from the LLM Artifacts page.

Current state:
- 9 strict contracts exist.
- 5 batch presets exist in `GeneratorPlanStrictLlmArtifactContractCatalog`.
- Product smoke covers expanded contract batch.
- But the user must manually check contract boxes.

Desired state:
- LLM Artifacts has a preset dropdown.
- Selecting a preset checks/unchecks the contract list to match the preset.
- Existing contract checkbox workflow still works.
- Existing Preview/Generate/Load/Audit flow is unchanged.

## Presets

The UI should surface existing catalog presets:

```text
baseline_game_seed
world_content_expansion
character_content_expansion
encounter_item_expansion
full_small_rpg_seed
```

The labels may use existing catalog labels, but user-visible UI should be understandable.

## Expected user flow

```text
1. Open LLM Artifacts.
2. Load capability selection.
3. Choose preset: full_small_rpg_seed.
4. Contract checkboxes update to:
   game_profile_v1
   region_pack_v1
   scene_pack_v1
   npc_pack_v1
   quest_pack_v1
   dialogue_pack_v1
   mechanics_pack_v1
   encounter_pack_v1
   item_pack_v1
5. Press Preview.
6. Press Generate.
```

## Non-goals

Do not:
- add new contracts;
- add new validators;
- change package assembly;
- touch runtime;
- touch Lua/generator-library;
- change LLM provider behavior;
- rewrite LLM Artifacts page architecture broadly.

## Done

Done when selecting each preset correctly updates contract selections and all existing generation/prompt tests still pass.
