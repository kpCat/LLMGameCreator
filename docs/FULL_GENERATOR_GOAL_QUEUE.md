# Full Generator Goal Queue

Status: planning control document

Purpose: keep LLMGameCreator moving toward the full generator without re-planning from scratch after every Codex run.

This document is not a replacement for `docs/FULL_GAME_GENERATION_MASTER_PLAN.md`. It is the operational queue from the current Unity Alpha stage to the full generator target.

## Current Accepted Position

Accepted through:

```text
minimum_playable_generated_game_verification passed
generated_game_profile_contract_verification passed
development_complexity_stabilization_verification passed
```

Produced for review:

```text
capability_bundle_pipeline_inputs_verification required
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
- readable Unity Alpha presentation evidence with scenario, quest, objective checklist, selected target, inventory, reward, event log and controls panels;
- minimum playable generated game review package with runnable Windows player folder, README, manual/automated scripts, scenario summary, automated launch proof and quest completion proof;
- generated game profile contract with three deterministic sample profiles, exact Goal 010-020 profile-to-pipeline mapping and explicit future-required capability separation;
- compact review artifacts under `.llmgc/procedural/...`;
- capability bundle pipeline input records for the three accepted game profiles, with explicit blocked/future-required gaps;
- heavy Unity build/log/cache outputs ignored by `.gitignore`.

Current limitation:

The development complexity stabilization gate has been accepted by user prompt. Goal 023 produced capability bundle pipeline input artifacts, but the user still needs to accept whether the Goal 023 selection/input evidence is sufficient before Goal 024 or S192 starts.

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

Accepted by user prompt before Goal 019.

### Goal 019: Unity Alpha Human-Readable Presentation

Gate:

```text
unity_alpha_readable_presentation_verification
```

Purpose:

Improve the primitive IMGUI presentation enough for manual play review: readable panels, selected target panel, quest/status panel, inventory/event log, simple controls.

Expected user-visible result:

The Alpha stops feeling like only a diagnostic log and becomes a primitive playable UI.

Status:

Accepted by user prompt before Goal 020.

### Goal 020: Minimum Playable Generated Game Gate

Gate:

```text
minimum_playable_generated_game_verification
```

Purpose:

Combine generated scene, runtime state, quest completion and readable presentation into one minimal generated game slice.

Expected user-visible result:

The user can launch the exe and play a short generated scenario from start to completion without inspecting JSON.

Status:

Accepted by user prompt before Goal 021.

## Generator Generalization Track

### Goal 021: Generated Game Profile Contract Refresh

Purpose:

Define or refresh the profile/capability contract used to choose game family, presentation mode, world topology, actor model, inventory/combat/progression models and generation scope.

Manual review likely required for profile/capability approval.

Status:

Accepted by user prompt before Goal 022.

### Goal 022: Development Complexity Stabilization And Artifact Scope Governance

Gate:

```text
development_complexity_stabilization_verification
```

Purpose:

Prevent future goals from silently mutating unrelated tracked generated artifacts by adding artifact-scope policy, guard automation, check-all artifact isolation, tracked generated artifact inventory and compact stabilization evidence.

Status:

Accepted by user prompt before Goal 023.

### Goal 023: Capability Bundle Selection To Pipeline Inputs

Gate:

```text
capability_bundle_pipeline_inputs_verification
```

Purpose:

Map profile choices to capability bundles and concrete generation pipeline inputs without hardcoding one scenario.

Status:

Produced for review. The gate remains `required`, not `passed`; do not start Goal 024 or S192 until it is accepted.

### Goal 024: Rich Package Assembly Coverage Audit

Purpose:

Audit existing package assembly against full generator needs: world, entities, quests, dialogue, items/economy, combat, progression, factions and schedules.

### Goal 025: Package Assembly Expansion 1 - World And Entities

Purpose:

Generate and assemble richer world/entity data for at least one selected game family.

### Goal 026: Package Assembly Expansion 2 - Dialogue And Quests

Purpose:

Generate and assemble richer dialogue/quest stages/objectives with validation and runtime smoke.

### Goal 027: Package Assembly Expansion 3 - Items, Economy And Crafting

Purpose:

Generate and assemble item/economy/crafting loops with validators and runtime smoke.

### Goal 028: Package Assembly Expansion 4 - Combat And Progression

Purpose:

Generate and assemble combat/progression definitions with validators and runtime smoke.

### Goal 029: Full Package Assembly Vertical

Purpose:

Generate a package that combines world, entities, dialogue/quests, items/economy and combat/progression for one family.

Manual review likely required.

## LLM And Lua Controlled Generation Track

### Goal 030: Artifact Contract Registry For Full Generator

Purpose:

Stabilize artifact contract registry for profile, world, entity, quest, dialogue, item/economy, combat and UI/export IR artifacts.

### Goal 031: Strict LLM Draft Artifact Loop

Purpose:

Use LLM only for contract-bound JSON drafts with validation and repair. No runtime authority, no code generation.

Manual review likely required before enabling broad LLM usage.

### Goal 032: Lua Module Manifest Registry

Purpose:

Introduce Lua module registry/manifest validation as deterministic generator IR, still without arbitrary runtime authority.

### Goal 033: Lua Sandbox Execution Gate

Purpose:

Execute a bounded Lua module family through sandbox and manifest-declared outputs.

Manual review likely required because this opens execution of generator modules.

### Goal 034: Hybrid LLM Draft Plus Lua Deterministic Expansion

Purpose:

LLM drafts bounded high-level artifacts; Lua expands deterministic configs/IR; C# validates/promotes.

## World Scale Track

### Goal 035: Region Graph And Reachability Generalization

Purpose:

Move beyond single start maps to generated region graphs with reachability validation.

### Goal 036: Finite Map Pack Generation

Purpose:

Generate finite maps with tile/entity placements, landmarks and paths.

### Goal 037: Chunked World Config Contract

Purpose:

Represent chunk rules/seeds/configs without dumping huge tile arrays.

### Goal 038: Runtime Chunk Delta Validation

Purpose:

Persist discovered/mutated chunk state in runtime/save state, not package definitions.

### Goal 039: Infinite/Chunked World Smoke

Purpose:

Smoke a generated chunked world path through runtime preview/export.

## Multi-Family Track

### Goal 040: Family 1 - Map And Panel RPG Template

Purpose:

Generate a richer map-and-panel RPG through the full lifecycle.

### Goal 041: Family 2 - Survival Sandbox Template

Purpose:

Generate survival sandbox data loops: resources, crafting, hazards, NPCs/events.

### Goal 042: Family 3 - First-Person Grid Dungeon Template

Purpose:

Generate first-person grid/blobber data with party/blob movement and combat profile.

### Goal 043: Multi-Family Capability Regression

Purpose:

Prove three families use the same lifecycle and do not fork the architecture.

Manual review likely required.

## Full Generator Stabilization Track

### Goal 044: Review And Approval Workflow Hardening

Purpose:

Make generated artifact review/promotion auditable and scalable.

### Goal 045: Repair Diagnostics Hardening

Purpose:

Ensure validation failures produce repairable diagnostics and bounded repair attempts.

### Goal 046: Runtime Preview Validation Across Generated Systems

Purpose:

Runtime preview smokes generated world/entity/quest/dialogue/item/economy/combat systems.

### Goal 047: Unity Export Profile Generalization

Purpose:

Export generated packages through profile-selected Unity presentation modes without hardcoded Alpha-only assumptions.

### Goal 048: One-Click Full Generator Dry Run

Purpose:

Run from approved profile/capabilities to generated package, validation, preview and export artifacts.

### Goal 049: Full Generator Without Media Verification

Gate:

```text
full_generator_without_media_verification
```

Definition:

At least three distinct game families can be generated through the same lifecycle; selected capabilities produce contract-bound artifacts; LLM/Lua outputs are validated before approval; package assembly covers major systems where selected; finite and chunked/infinite world paths are supported; runtime preview/export smoke generated packages; no runtime path depends on LLM/provider/unapproved code.

Manual review required.

## Current Active Gate

```text
capability_bundle_pipeline_inputs_verification
```

Status:

```text
required
```
