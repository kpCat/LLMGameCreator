# Goal 037 Spec — Hybrid LLM Draft Plus Lua Deterministic Expansion

## Composite goal

`goal_037_hybrid_llm_draft_plus_lua_deterministic_expansion`

## Gate

`hybrid_llm_draft_lua_deterministic_expansion_verification required`

## Purpose

Turn the Goal 034/035/036 safety stack into the first real bounded hybrid expansion slice:

- selected strict draft requests/candidates remain quarantined;
- selected Lua module manifests remain registry-owned;
- sandbox decisions remain deny-first;
- a bounded executor adapter may run only approved deterministic expansion modules;
- C# validates every output before promotion;
- evidence proves frontier/gothic/caravan/metamodule scenarios can produce deterministic expansion artifacts without LLM/provider/RAG calls and without Runtime/GamePackage/UI/Unity mutation.

## Non-goals

- No final prose generation.
- No live LLM/provider/RAG calls.
- No runtime Lua modding surface.
- No GamePackage schema changes.
- No Runtime, WinForms/UI, Unity or generator-library changes.
- No arbitrary Lua execution.
- No user script execution outside repo-owned deterministic fixtures.

## Minimum proof

Goal 037 must prove at least one real bounded deterministic expansion path if safe dependency adoption succeeds. If dependency adoption or sandbox enforcement cannot be made safe, the correct result is `BLOCKED`, committed and pushed with exact diagnostics.

Required scenario coverage:

- `frontier_survival`
- `gothic_intrigue`
- `caravan_trade`
- `metamodule_kingdoms`

Required artifact families:

- NPC/species/archetype expansion hints;
- region/faction/kingdom expansion hints;
- quest/event intent expansion hints;
- economy/combat/settlement expansion hints;
- metamodule species/archetype slot expansion for 112 slots or the current canonical count from prior evidence.

## Evidence folder

`.llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion/`

Required files:

- `executor-adapter-selection.json`
- `hybrid-pipeline-summary.json`
- `draft-to-lua-request-map.json`
- `sandbox-approved-expansion-matrix.json`
- `lua-expansion-output-frontier.json`
- `lua-expansion-output-gothic.json`
- `lua-expansion-output-caravan.json`
- `lua-expansion-output-metamodule-kingdoms.json`
- `promotion-decision-matrix.json`
- `invalid-hybrid-expansion-diagnostics-matrix.json`
- `hybrid-llm-draft-lua-deterministic-expansion-report.md`

All evidence must be deterministic, compact, path-safe, timestamp-free unless repo convention already uses deterministic timestamps, and free of absolute local paths.
