# Full Generator Goal Queue

Status: planning control document

Purpose: keep LLMGameCreator moving toward the full generator without re-planning from scratch after every Codex run.

This document is not a replacement for `docs/FULL_GAME_GENERATION_MASTER_PLAN.md`. It is the operational queue from the current Unity Alpha stage to the full generator target.

## Current Accepted Position

Accepted through:

```text
unity_generated_quest_completion_loop_verification passed
```

Produced for review:

```text
unity_generated_multi_variant_playable_scenario_verification required
```

Current capabilities:

- deterministic content generation at scale;
- deterministic minimum asset pipeline using fixture/fallback assets;
- Unity runtime export payload;
- repository-local Unity project and Windows build entrypoint;
- real Windows player build and launch;
- visible Unity Alpha presentation;
- generated scene projection derived from package/config/asset refs;
- generated Unity runtime state loop evidence with quest/dialogue/item/inventory/event before-after transitions;
- generated Unity quest completion loop evidence with ordered phases, objective checklist, completion and reward proof;
- generated Unity multi-variant playable scenario evidence for frontier, gothic and caravan styles through the same Alpha pipeline;
- compact review artifacts under `.llmgc/procedural/...`;
- heavy Unity build/log/cache outputs ignored by `.gitignore`.

Current limitation:

The Unity Alpha now proves three generated style variants can produce distinct playable quest loops through the same pipeline, but the primitive IMGUI presentation still needs human-readable polish before broader manual play review.

## Queue Rules

1. Always keep one active goal and the next three candidate goals documented.
2. Only the next goal receives a fully detailed Codex task file.
3. Future goals stay as queue entries until their dependencies are verified.
4. After every accepted goal, update this file, `CURRENT_GENERATOR_STATE.*` and `CONTEXT_INDEX.md`.
5. Do not create broad platform work unless it directly moves a generated playable/simulatable game forward.
6. Do not mark a gate passed inside the same goal that produced it.
7. Prefer automated verification; reserve manual review for actual playability, profile/canon approval, and major architecture choices.

## Anti-Freeze Rule

If two consecutive goals mostly improve reports, wrappers or diagnostics without adding visible gameplay, generation coverage, validation coverage or pipeline generality, stop and reassess before creating another goal.

## Anti-False-Positive Requirements For All Future Goals

Every future goal must include:

- explicit starting gate confirmation;
- exact final gate, left `required`;
- read-first list;
- allowed files and forbidden files;
- root artifact requirements;
- product smoke route;
- invalid/fake/leak matrix with causal mutations;
- direct artifact inspection after smoke;
- state/context docs update;
- scan for nondeterminism and future-goal markers;
- final report requiring changed files, verification results and no-git confirmation.

Every future goal must also answer:

```text
What user-visible or generator-capability thing became more real?
```

If the answer is only "the report is better", the goal is probably too weak.

## Near-Term Unity Alpha Track

### Goal 016: Unity Generated Runtime State Loop

Gate:

```text
unity_generated_runtime_state_loop_verification
```

Purpose:

Turn generated scene projection into a visible state loop. Interactions must update quest/dialogue/item/event state in the Unity Alpha and prove before/after state, not only command execution logs.

Expected user-visible result:

The Alpha shows generated scene nodes plus visible state changes: quest started/progressed, dialogue opened/choice selected, item obtained, event applied, inventory/status text updated.

Status:

Accepted by user prompt before Goal 017.

### Goal 017: Unity Generated Quest Completion Loop

Gate:

```text
unity_generated_quest_completion_loop_verification
```

Purpose:

Make one generated micro-quest playable end-to-end in Unity Alpha: start, interact, obtain/apply item or event, complete objective, show completion/reward.

Expected user-visible result:

The user can run the player and complete one generated micro-quest in a primitive but coherent loop.

Status:

Accepted by user prompt before Goal 018.

### Goal 018: Unity Multi-Variant Playable Scenario

Gate:

```text
unity_generated_multi_variant_playable_scenario_verification
```

Purpose:

Prove at least three generated styles/seeds produce distinct Unity Alpha scenes and quest loops through the same pipeline.

Expected user-visible result:

Frontier/gothic/caravan scenarios are visibly different in ids, labels, nodes, objective text and command/state flow.

Status:

Produced for review. The gate remains `required`, not `passed`; do not start S154 or Goal 019 until it is accepted.

### Goal 019: Unity Alpha Human-Readable Presentation

Gate:

```text
unity_alpha_readable_presentation_verification
```

Purpose:

Improve the primitive IMGUI presentation enough for manual play review: readable panels, selected target panel, quest/status panel, inventory/event log, simple controls.

Expected user-visible result:

The Alpha stops feeling like only a diagnostic log and becomes a primitive playable UI.

### Goal 020: Minimum Playable Generated Game Gate

Gate:

```text
minimum_playable_generated_game_verification
```

Purpose:

Combine generated scene, runtime state, quest completion and readable presentation into one minimal generated game slice.

Expected user-visible result:

The user can launch the exe and play a short generated scenario from start to completion without inspecting JSON.

Manual review likely required.

## Generator Generalization Track

### Goal 021: Generated Game Profile Contract Refresh

Purpose:

Define or refresh the profile/capability contract used to choose game family, presentation mode, world topology, actor model, inventory/combat/progression models and generation scope.

Manual review likely required for profile/capability approval.

### Goal 022: Capability Bundle Selection To Pipeline Inputs

Purpose:

Map profile choices to capability bundles and concrete generation pipeline inputs without hardcoding one scenario.

### Goal 023: Rich Package Assembly Coverage Audit

Purpose:

Audit existing package assembly against full generator needs: world, entities, quests, dialogue, items/economy, combat, progression, factions and schedules.

### Goal 024: Package Assembly Expansion 1 - World And Entities

Purpose:

Generate and assemble richer world/entity data for at least one selected game family.

### Goal 025: Package Assembly Expansion 2 - Dialogue And Quests

Purpose:

Generate and assemble richer dialogue/quest stages/objectives with validation and runtime smoke.

### Goal 026: Package Assembly Expansion 3 - Items, Economy And Crafting

Purpose:

Generate and assemble item/economy/crafting loops with validators and runtime smoke.

### Goal 027: Package Assembly Expansion 4 - Combat And Progression

Purpose:

Generate and assemble combat/progression definitions with validators and runtime smoke.

### Goal 028: Full Package Assembly Vertical

Purpose:

Generate a package that combines world, entities, dialogue/quests, items/economy and combat/progression for one family.

Manual review likely required.

## LLM And Lua Controlled Generation Track

### Goal 029: Artifact Contract Registry For Full Generator

Purpose:

Stabilize artifact contract registry for profile, world, entity, quest, dialogue, item/economy, combat and UI/export IR artifacts.

### Goal 030: Strict LLM Draft Artifact Loop

Purpose:

Use LLM only for contract-bound JSON drafts with validation and repair. No runtime authority, no code generation.

Manual review likely required before enabling broad LLM usage.

### Goal 031: Lua Module Manifest Registry

Purpose:

Introduce Lua module registry/manifest validation as deterministic generator IR, still without arbitrary runtime authority.

### Goal 032: Lua Sandbox Execution Gate

Purpose:

Execute a bounded Lua module family through sandbox and manifest-declared outputs.

Manual review likely required because this opens execution of generator modules.

### Goal 033: Hybrid LLM Draft Plus Lua Deterministic Expansion

Purpose:

LLM drafts bounded high-level artifacts; Lua expands deterministic configs/IR; C# validates/promotes.

## World Scale Track

### Goal 034: Region Graph And Reachability Generalization

Purpose:

Move beyond single start maps to generated region graphs with reachability validation.

### Goal 035: Finite Map Pack Generation

Purpose:

Generate finite maps with tile/entity placements, landmarks and paths.

### Goal 036: Chunked World Config Contract

Purpose:

Represent chunk rules/seeds/configs without dumping huge tile arrays.

### Goal 037: Runtime Chunk Delta Validation

Purpose:

Persist discovered/mutated chunk state in runtime/save state, not package definitions.

### Goal 038: Infinite/Chunked World Smoke

Purpose:

Smoke a generated chunked world path through runtime preview/export.

## Multi-Family Track

### Goal 039: Family 1 - Map And Panel RPG Template

Purpose:

Generate a richer map-and-panel RPG through the full lifecycle.

### Goal 040: Family 2 - Survival Sandbox Template

Purpose:

Generate survival sandbox data loops: resources, crafting, hazards, NPCs/events.

### Goal 041: Family 3 - First-Person Grid Dungeon Template

Purpose:

Generate first-person grid/blobber data with party/blob movement and combat profile.

### Goal 042: Multi-Family Capability Regression

Purpose:

Prove three families use the same lifecycle and do not fork the architecture.

Manual review likely required.

## Full Generator Stabilization Track

### Goal 043: Review And Approval Workflow Hardening

Purpose:

Make generated artifact review/promotion auditable and scalable.

### Goal 044: Repair Diagnostics Hardening

Purpose:

Ensure validation failures produce repairable diagnostics and bounded repair attempts.

### Goal 045: Runtime Preview Validation Across Generated Systems

Purpose:

Runtime preview smokes generated world/entity/quest/dialogue/item/economy/combat systems.

### Goal 046: Unity Export Profile Generalization

Purpose:

Export generated packages through profile-selected Unity presentation modes without hardcoded Alpha-only assumptions.

### Goal 047: One-Click Full Generator Dry Run

Purpose:

Run from approved profile/capabilities to generated package, validation, preview and export artifacts.

### Goal 048: Full Generator Without Media Verification

Gate:

```text
full_generator_without_media_verification
```

Definition:

At least three distinct game families can be generated through the same lifecycle; selected capabilities produce contract-bound artifacts; LLM/Lua outputs are validated before approval; package assembly covers major systems where selected; finite and chunked/infinite world paths are supported; runtime preview/export smoke generated packages; no runtime path depends on LLM/provider/unapproved code.

Manual review required.

## Current Active Gate

```text
unity_generated_multi_variant_playable_scenario_verification
```

Status:

```text
required
```
