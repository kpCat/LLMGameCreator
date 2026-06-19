# Product Slice 006: Strict Contract Catalog + Batch Generation

## Goal

Move beyond the first four baseline contracts and introduce controlled batch generation.

The initial four contracts were intentionally small because they were used to prove the whole vertical path:

```text
capability selection
-> strict artifacts
-> review
-> approved artifacts
-> package assembly
-> runtime preview
-> headless smoke
```

Now add the first richer content layer:

```text
region_pack_v1
npc_pack_v1
item_pack_v1
dialogue_pack_v1
encounter_pack_v1
```

## Batch presets

Minimum presets:

```text
baseline_game_seed:
  game_profile_v1
  scene_pack_v1
  quest_pack_v1
  mechanics_pack_v1

world_content_expansion:
  region_pack_v1
  scene_pack_v1

character_content_expansion:
  npc_pack_v1
  dialogue_pack_v1

encounter_item_expansion:
  encounter_pack_v1
  item_pack_v1

full_small_rpg_seed:
  game_profile_v1
  region_pack_v1
  scene_pack_v1
  npc_pack_v1
  quest_pack_v1
  dialogue_pack_v1
  mechanics_pack_v1
  encounter_pack_v1
  item_pack_v1
```

## Rule

Do not implement full simulation for these new contracts yet. The goal is: generate, validate, review, apply/preserve/map into package, display in Runtime Preview, and smoke-test. Runtime mechanics can remain shallow.

## Done

Done when new contracts appear in LLM Artifacts, have prompts/validators, can be batched, can be approved/applied, are visible in package/generatedContent and Runtime Preview, and headless smoke covers the expanded batch.
