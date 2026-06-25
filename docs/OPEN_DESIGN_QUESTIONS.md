# Open Design Questions

Status: proposed tracking document after Goal 003.

These questions should be answered by experiments and bounded goals, not by endless discussion.

## Semantics

- What is the minimal core semantic pack needed for useful generation?
- Which term kinds are stable enough for core?
- How are project-specific terms accepted or rejected?
- How much relation typing is useful before it becomes overhead?
- What is the boundary between semantic pack and rule pack?
- Should local RAG suggest semantic candidates from user notes?
- How are NSFW, taboo, legality, consent, violence, and adult-content tags represented for authoring control?

## Lua And Rule Packs

- When does a gameplay idea fit rule-pack declarations?
- When does it require a new C# primitive?
- How are formula functions whitelisted?
- How are unsafe loops, recursion, file/network access, and random nondeterminism prevented?
- Can the same rule pack run in headless tests and Unity runtime?
- How are rule-pack versions migrated?

## Runtime Simulation

- What state runs every tick?
- What state runs only on events?
- What happens in distant/unloaded regions?
- What is the abstract simulation format for far-world updates?
- How are NPC schedules, jobs, trade, combat, and faction events simplified at distance?
- What is the save/load boundary for huge worlds?

## Unity Export

- What is the minimum Unity runtime target?
- How does package data map to scenes, chunks, prefabs, tilemaps, and UI?
- How are 2D tile sets presented in 2.5D/3D?
- What performance budget is required?
- What is streamed and what is always loaded?
- How are generated assets referenced safely?

## Assets And Audio

- What asset types can be auto-requested?
- Which asset types must require user review before import?
- How are ComfyUI requests parameterized?
- How are tilesets validated for consistency?
- How are music tracks imported and tagged?
- Is dynamic music from samples a later primitive or out of scope for alpha?

## Game Families

- Which mechanics are alpha families?
- Which mechanics are later primitive families?
- Which mechanics are only content/rule-pack variations?
- Which mechanics are too expensive for early scope?

## Process

- How many slices per goal is acceptable?
- Which risks require intermediate manual gates?
- Which tests replace manual verification?
- What is the kill criterion for a goal?
- What is the kill criterion for a stage?

## Current Answers To Reuse

- Runtime Preview is a proving ground, not the final engine.
- LLM is offline authoring help, not runtime.
- Data/rule packs should carry most game-specific behavior.
- C# should add primitive families, not bespoke content.
- One manual verification gate per goal is the default.
- Semantic packs should be curated and layered, not massive imported dumps.
