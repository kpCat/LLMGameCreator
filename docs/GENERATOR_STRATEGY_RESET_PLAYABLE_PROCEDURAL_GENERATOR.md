# Generator Strategy Reset: Playable Procedural Generator

Status: source-of-truth strategy pivot  
Scope: product direction, Codex task selection, generation architecture, limit discipline  
Non-scope: production implementation, schema changes, UI changes, Lua execution, provider calls

## 1. Why this reset exists

After Product Slice 028 the project has a substantial safety, review, artifact, archive and semantic foundation. That foundation is useful, but it is not the product outcome by itself.

The product risk is now clear:

```text
The project can spend months improving control infrastructure while still failing to
produce a generated game that can be played, simulated, evaluated or iterated.
```

This reset changes the next-work rule. Infrastructure work is no longer acceptable merely because it improves safety, reports, review, history, documentation or future extensibility. The next work must move the product toward a generated playable or simulatable loop.

## 2. Product north star

LLMGameCreator is not a "prompt writes a game" tool and not a report pipeline. It is a game-generation machine:

```text
user intent / preset / example
-> generation strategy
-> optional LLM rule-pack drafting
-> deterministic procedural expansion
-> validated game data
-> playable or simulatable runtime loop
-> export/archive/media request planning
```

The target outcome is:

```text
The user can choose or describe a game direction, generate a coherent data-driven
game plan, inspect it, and run a small playable or simulatable loop without Codex
inventing a new architecture for every feature.
```

## 3. What must stop

These task types are frozen unless the user explicitly asks for them or they are required to unblock the next playable/simulatable slice:

- semantic catalog UI polish;
- manual import UI polish;
- archive review, history or comparison polish;
- more report formats;
- more documentation that does not change task selection or acceptance gates;
- broad artifact-contract expansion without a generated runtime outcome;
- more safety wrappers around a generator kernel that does not exist yet;
- "foundation" slices whose value cannot be demonstrated in a generated game loop.

This does not mean the existing infrastructure is wrong. It means the product cannot keep spending limit on infrastructure before proving the generator kernel.

## 4. What remains valuable from the current foundation

The existing work should be reused, not discarded:

- capability selection;
- strict LLM artifact generation and evaluation;
- artifact review and approval;
- deterministic package assembly/export path;
- Unity archive materialization and fulfillment planning;
- manual provider output import;
- review/history/comparison;
- semantic sidecar and generation-context preview;
- C# validation authority;
- headless runtime services already present in the repository.

The pivot is not a restart. It is a change in what future slices are allowed to optimize.

## 5. New task selection rule

Every non-trivial Codex task after this reset must answer:

```text
What can the user generate, simulate, play, or evaluate after this task that they
could not do before?
```

If the answer is only "the pipeline is safer", "the report is clearer", "the UI is more complete", or "future work is easier", the task is rejected unless the user explicitly requests it.

Allowed task outcomes:

- a deterministic generated game plan with runtime-facing data;
- a formula/effect/action registry used by generated gameplay data;
- a generated package that can be loaded by the runtime simulator;
- a repeatable seed-based world/entity/quest/encounter generation path;
- a small playable or simulatable loop;
- a failing runtime smoke converted into a concrete repair target.

## 6. Kill criterion

The next three large product slices after this strategy reset must produce a generated playable or simulatable loop.

Minimum acceptable loop:

```text
seed or preset
-> generated world/region/encounter data
-> generated actors/items/quests or events
-> package/runtime-facing validation
-> runtime simulator can execute at least one coherent loop:
   move/explore -> interact/quest/event -> reward/cost/state change
```

If the project cannot reach that within three large slices, stop and reassess the architecture before spending more limit.

This criterion is intentionally strict. It exists to prevent another long run of infrastructure-only progress.

## 7. LLM role after the reset

The LLM must not be treated as a bulk content printer.

Bad target model:

```text
LLM generates 1000 NPCs.
LLM generates 500 quests.
LLM generates 10000 dialogue lines.
LLM generates every map region or chunk.
```

Correct target model:

```text
LLM drafts compact rule packs, archetypes, grammars, examples and creative
constraints. The combiner expands them deterministically.
```

LLM may draft:

- world grammar;
- faction archetype packs;
- creature/archetype mutation rules;
- event grammar;
- quest motif grammar;
- dialogue voice/style packs;
- semantic packs;
- a small number of high-value authored examples;
- art/audio prompt style packs.

The combiner should generate:

- concrete regions/chunks;
- NPC variants;
- encounter tables;
- procedural quests;
- short dialogue variants from intent templates and phrase banks;
- loot/economy/combat tables;
- formula outputs;
- validation reports;
- runtime-facing game data.

One LLM call should compress a creative decision into reusable rules. It should not correspond to one generated item, NPC, quest, tile, chunk or line of dialogue.

## 8. Generation modes

Future work must support multiple generation modes without assuming one game shape.

| Mode | Use when | Main generation authority |
|---|---|---|
| `authored_small_world` | Small RPG, IF, visual novel, curated prototype | LLM-authored examples plus deterministic assembly |
| `semi_procedural_regions` | Region-based RPG, survival, faction game | LLM rule packs plus deterministic region expansion |
| `fully_seeded_world` | Large/infinite replayable worlds | Deterministic seeded procedural generators |
| `external_data_driven_world` | Map-based games, OSM-like data, real-world topology | External adapter plus semantic/gameplay mapping |
| `runtime_expanding_world` | Worlds generated or discovered during play | Seed, neighbor constraints, simulation state and chunk rules |

No mode should require bulk LLM calls proportional to world size.

## 9. Intermediate API / DSL stance

The project should introduce an intermediate gameplay definition layer, but only as a product accelerator, not as another abstract framework that delays playability.

Preferred extensibility tiers:

```text
Tier 1: data tables and semantic tags
Tier 2: formulas, requirements, costs and rewards
Tier 3: effect/action/event DSL
Tier 4: deterministic procedural generators
Tier 5: sandboxed Lua data/rule modules
Tier 6: C# runtime primitives for genuinely new interaction modes
```

Most future mechanics should be expressible in tiers 1-4. Lua is for controlled data/rule generation. C# runtime primitives are reserved for features that cannot be represented as data, formula, effect/action rules or deterministic generator output.

Example target shape:

```json
{
  "ruleId": "event_rule/infection_on_hit",
  "trigger": "on_attack_hit",
  "conditions": [
    { "type": "has_tag", "target": "attacker", "tag": "carrier" }
  ],
  "actions": [
    {
      "type": "add_status",
      "target": "defender",
      "statusId": "status/infected",
      "chanceFormula": "0.15 + attacker.virulence * 0.02"
    }
  ]
}
```

The exact contract may differ, but this is the direction: new gameplay should usually be data plus validated behavior primitives, not fresh C# for every idea.

## 10. Immediate implementation sequence

The next work should be limited to this sequence unless the user explicitly overrides it.

### Slice A: Seeded Procedural Game Kernel v1

Goal: generate a deterministic runtime-facing game plan without LLM, provider calls, Unity, Lua execution or UI polish.

Required output:

- generation profile with mode, seed and selected variant ids;
- generated region/world graph or small map;
- generated factions or actor groups;
- generated actor/item/encounter/quest/event seeds;
- deterministic diagnostics and summary;
- validation proving stable same-seed output.

User-visible value:

- user can see that the combiner can create coherent game structure without asking the LLM to print bulk content.

### Slice B: Formula/Effect/Action Registry Foundation

Goal: make generated gameplay rules executable or at least runtime-facing through validated primitives.

Required output:

- formula definitions;
- requirement/cost/reward definitions;
- effect/action definitions;
- trigger/event rule definitions;
- validator coverage for unsafe refs and invalid formulas;
- at least one generated mechanic used by the procedural kernel.

User-visible value:

- new mechanics start becoming data/rule packs instead of bespoke C# slices.

### Slice C: Tiny Generated Runtime Loop

Goal: produce and run a generated package or runtime-facing plan through a small simulation loop.

Required loop:

```text
generate -> validate -> load/simulate -> move/explore -> interact/event -> state change
```

Minimum accepted scenario:

- one generated world/region;
- at least two actors or factions;
- at least one item/resource;
- at least one quest/event/interaction;
- at least one reward/cost/status/state change;
- deterministic runtime smoke.

User-visible value:

- the project proves it can generate something game-like, not merely review artifacts.

## 11. Explicit non-goals for the next three slices

Do not implement unless explicitly required for the playable/simulatable loop:

- semantic catalog review UI;
- more archive review UI;
- provider execution;
- media generation;
- Unity scene/runtime export;
- broad Lua executor unlock;
- rich GamePackage schema expansion beyond what the loop actually needs;
- large visual editor work;
- full game template families;
- external map adapters;
- infinite streaming runtime;
- C# code generation for mechanics.

## 12. Acceptance gates for future tasks

A future task must include at least one of these gates:

- deterministic same-seed generation test;
- generated artifact validation test;
- formula/effect/action validation test;
- package assembly validation test;
- runtime simulator smoke;
- generated game plan snapshot comparison;
- explicit LLM call budget estimate before any model call.

Docs-only tasks after this reset are allowed only when they directly change task selection, architecture boundaries or acceptance criteria.

## 13. Relationship to existing docs

This document overrides the "recommended next work" direction when older docs suggest polish or UI work that does not move toward a playable/simulatable generated loop.

It does not delete or invalidate:

- `docs/FULL_GAME_GENERATION_MASTER_PLAN.md`;
- `docs/CODEX_EXECUTION_DOCTRINE.md`;
- `docs/GENERATION_PROCEDURE_AND_LLM_POLICY.md`;
- `docs/ROADMAP_TO_FULL_GENERATOR.md`.

Instead, it narrows the next phase:

```text
Before expanding the platform, prove the generated game kernel.
```

## 14. One-line rule for agents

If a proposed next task does not make the generated game more playable, simulatable, or runtime-facing, stop and ask for explicit user approval before spending limit.

## 15. Goal156 durable generated-project bridge

Goal156 turns the existing procedural kernel/registry/tiny-loop/MVP proof into normal durable project creation. Deterministic seed/mode/preset inputs produce project-local source and sidecars, then an additive generated overlay is composed through the existing FeatureModule and canonical Runtime/checkpoint/replay seams. This keeps GamePackage as the only game truth and keeps LLM/provider/Lua/Unity generation outside Runtime.

## 16. Goal157 reproducible provenance and generated start

Goal157 makes the declared v1 source request causal: validation regenerates the deterministic chain and reconstructs overlay/base from the canonical Goal142 package. Modern generated builds use two explicit views of one module-composed package. The baseline-start view qualifies previously accepted mechanics; the generated-start view is the final player package and supplies primary hashes and Runtime frames. The existing Runtime owns Start → Move Right → Interact, replay and save/load truth. This preserves GamePackage authority and avoids introducing generator, UI, provider or LLM dependencies into Runtime.

The immutable Goal142 baseline remains the active anchor and start map. The next selected product slice may address generated-world activation/travel or safe seed regeneration, but must preserve explicit validation, apply and rollback boundaries.
