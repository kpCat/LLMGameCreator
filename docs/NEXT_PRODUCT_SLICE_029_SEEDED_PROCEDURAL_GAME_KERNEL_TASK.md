# Product Slice 029 Task: Seeded Procedural Game Kernel v1

Status: current proposed Codex task  
Depends on: Product Slice 028 and strategy reset  
Primary outcome: deterministic runtime-facing generated game structure  
Non-outcome: UI polish, provider execution, Unity export, Lua execution, broad schema expansion

## Source-Of-Truth Reading Order

Read only these files before implementation unless a referenced code area requires local context:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`
5. `docs/GENERATION_PROCEDURE_AND_LLM_POLICY.md`
6. `docs/FULL_GAME_GENERATION_MASTER_PLAN.md`
7. `docs/GAME_SYSTEM_VARIANT_TAXONOMY.md`
8. runtime/domain files needed for generated data compatibility

Do not read old root `README_APPLY_*` files, old `*_CODEX_PROMPT.md`, old `*_KILO_PROMPT.md`, old archive manifests, or old apply READMEs as current planning authority.

## First Action

Before implementing code, verify that these files already point to the strategy reset:

- `README.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`

If they still recommend semantic UI, manual import polish, archive review polish, or M4.1 local-model evaluation as the next step, update them first to the strategy-reset state.

## Goal

Add the first deterministic procedural game kernel that can produce coherent, runtime-facing game structure from a seed and a small generation profile.

The point of this slice is not to build the final generator. The point is to prove that LLMGameCreator can generate useful game structure without using an LLM as a bulk content printer and without spending another slice on infrastructure-only work.

## Required Behavior

Create an Application-layer service that accepts:

- generation seed;
- generation mode, initially one of:
  - `authored_small_world`;
  - `semi_procedural_regions`;
  - `fully_seeded_world`;
- compact style/semantic hints from existing semantic catalog models when available;
- selected form/system variant ids when available, with safe defaults for the first slice.

The service must produce a deterministic generated game plan containing at least:

- generation metadata: seed, mode, version, deterministic hash or stable summary;
- small region/world graph or map plan;
- at least two factions or actor groups;
- actor archetype seeds;
- item/resource seeds;
- encounter seeds;
- quest/event seeds;
- formula/effect/action requirement notes or placeholders for Slice 030;
- diagnostics for missing unsupported features;
- a Markdown summary for review.

Same input must produce byte-stable JSON output.

Different seeds should produce visibly different but structurally valid output.

## Important Design Constraints

- No LLM calls.
- No provider calls.
- No Unity work.
- No media generation.
- No Lua execution.
- No GamePackage schema change unless absolutely required and explicitly approved.
- No WinForms UI unless needed only for smoke visibility; prefer headless product smoke.
- No semantic catalog approval UI.
- No archive review/manual import polish.
- No broad template family work.
- No C# code generation.

## Preferred Implementation Area

Prefer a new focused Application area, for example:

```text
src/LLMGameCreator.Application/Generation/Procedural/
```

or an existing nearby Application/Design generation namespace if the repository already has a better local pattern.

Keep Domain changes minimal. If new models are needed, prefer Application-side generated-plan models first unless package/runtime contracts must consume them immediately.

## Output Artifact Suggestion

Write generated artifacts under project-local `.llmgc/` when a project folder is supplied:

```text
.llmgc/procedural/generated-game-plan.json
.llmgc/procedural/generated-game-plan.md
```

The artifacts must not contain timestamps, absolute paths, machine names or nondeterministic ordering.

## Runtime-Facing Requirement

This slice does not need to run the full runtime loop yet, but its output must be shaped so Slice 031 can load or map it into a tiny generated runtime loop.

The generated plan should therefore include stable ids and clear references for:

- regions;
- factions/groups;
- actors;
- items/resources;
- encounters;
- quests/events;
- formula/effect/action placeholders.

## Validation Requirements

Add focused tests for:

- same seed and profile produce byte-identical JSON;
- different seed produces different selected values while preserving structure;
- invalid seed/profile/mode returns diagnostics instead of throwing;
- unsafe ids are rejected or normalized according to existing id rules;
- generated references point to existing generated records;
- Markdown summary is deterministic and contains the main generated counts;
- no LLM/provider/Lua/Unity/runtime execution is invoked.

Add one product smoke scenario:

```text
procedural-game-kernel
```

The smoke should prove the two output artifacts and deterministic repeatability.

## Acceptance Criteria

The slice is complete only when:

- generated-game-plan JSON and Markdown are produced deterministically;
- the output contains world/region, faction/group, actor, item/resource, encounter and quest/event seeds;
- same seed repeatability is tested;
- different seed variation is tested;
- no infrastructure-only UI/report work is introduced;
- docs state that this is the first step toward the three-slice generated playable/simulatable loop;
- `CURRENT_GENERATOR_STATE.md` and `.json` recommend Slice 030: Formula/Effect/Action Registry Foundation.

## Stop Conditions

Stop and report instead of implementing if:

- implementation would require GamePackage schema changes;
- implementation would require runtime command/state contract changes;
- implementation would require more than one new broad subsystem;
- existing runtime/domain contracts cannot represent the generated concepts even as a plan;
- the task starts drifting into UI, archive review, manual import, provider, Unity, Lua execution or broad generator-family work.

## Final Report Requirements

The final report must include:

- changed files;
- generated artifact paths;
- tests/smokes run;
- confirmation that no LLM/provider/Lua/Unity/media execution was added;
- whether Slice 030 remains viable without redesign.
