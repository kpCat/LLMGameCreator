# LLMGameCreator Product Slice 006 Pack

## Purpose

Product Slice 006 expands LLM Artifacts beyond the initial four baseline contracts and adds the first controlled batch content generation flow.

Current baseline:

```text
game_profile_v1
scene_pack_v1
quest_pack_v1
mechanics_pack_v1
```

New first expansion:

```text
region_pack_v1
npc_pack_v1
item_pack_v1
dialogue_pack_v1
encounter_pack_v1
```

## Apply

Unzip at repository root and give Codex:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/006_CODEX_PROMPT.md
```

## Recommended Codex reasoning level

Use **High**.

Reason: this needs end-to-end wiring through contract catalog, prompt builder, validators, package assembly, runtime preview projection, batch presets and product smoke. Medium is likely to miss seams; Max/Ultra is not needed unless High fails.
