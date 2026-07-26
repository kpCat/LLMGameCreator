# Generator Strategy Reset: Playable Procedural Generator

Goal166 update: exact qualified combat-action descriptors remain Application-layer contract truth, while tactical UI stays a projection over package/runtime state. Recovery is now proven through the production campaign service; persisted checkpoints and campaign choice branching remain separate tasks.

Goal167 attempt: Application-layer choice binding, overlay, preview and journal code was started, but the mandatory behavioral matrix is incomplete. This is not a candidate for product progression or standalone qualification.

Goal165 update: the player-facing campaign keeps exact package dispatch while combat contracts explicitly preserve the profile's actual player route: BasicAttack-only, package-ability-only or both. Defeat recovery restores a verified in-memory pre-encounter session through existing Runtime commands, never changes Runtime definitions or save schema, and fails stale truth before dispatch. Goal165 is unaccepted, has no manual gate and requires independent audit.

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

The immutable Goal142 baseline remains the compatibility anchor. Goal158 advances the player lane into generated-region travel; safe seed regeneration remains separate and must preserve explicit validation, apply and rollback boundaries.

## 17. Goal158 deterministic generated-region travel

Goal158 derives exact region-to-map bindings and directed connections only from the strict regenerated plan plus generated provenance. A build-time overlay appends one generic travel prototype and one deterministic gate per directed connection without changing pre-existing records. The ordinary `PlayerCommand.Interact` path validates the complete Args contract atomically, changes map/position only after validation and emits additive `MapChanged` correlation.

The Application planner uses package walkability/collision data and a fixed direction tie-order to execute a real origin interaction, one or more plan connections and a destination interaction. Complete-route hashes, frames, replay and save/load are player-lane authority; the baseline-start AcceptedMechanics/Social lane stays unchanged. History/UI/standalone/portable truth follows this split. Runtime remains independent from generation, UI, LLM and providers.

## 18. Goal159 transactional generated-world regeneration

Goal159 keeps GamePackage as final authority while making generated inputs safely editable. Source v2 separates the operator request from deterministically resolved options, including preset-definition correlation and explicit override truth. New creation and regeneration invoke one artifact factory; v1 remains readable/buildable and is written as v2 only after successful regeneration.

Regeneration is candidate-first: a complete copy outside the authoritative project receives the new deterministic chain, builds twice through Lane A accepted mechanics and Lane B generated travel, then reopens before a typed diff is shown. Promotion requires unchanged source, authoring, package, identity and RC tokens and uses a durable journal with exact rollback and nonterminal recovery. Runtime, public GamePackage, FeatureModule catalog and Unity remain unchanged. Save migration and user-selectable historical-world rollback are deliberately deferred.

## 19. Goal160 sealed commit boundary and generated-world history

Goal159 independent audit found that its final token check, candidate authority and semantic reopen did not share one rollback-safe commit boundary. Goal160 closes this P1 with one operation coordinator and whole-operation cross-process project lock for build, standalone, authoring mutation, regeneration, history rollback and recovery. Preview writes an immutable complete candidate seal; Apply accepts only cached attempt identity plus that seal. Truth tokens and authoritative inventory are recaptured after the lock and before any backup or mutation. Journal `validating` keeps semantic source/package/authoring/identity/history/RC/world-change checks inside the rollback window.

Generated-world history is generation-only: source and deterministic sidecars, never package/authoring/identity/RC truth. Regeneration and rollback archive both sides atomically. Restoring an old world builds an isolated candidate from its historical generation with current mechanics, parameters and identity, repeats and reopens `TRAVEL_CURRENT`, seals it and applies through the same transaction. Old histories remain and old RC bytes stay `LAST_SUCCESS` until an ordinary standalone. Runtime, public GamePackage, FeatureModule catalog and Unity remain unchanged. Generated gameplay save-state migration is the next separate product decision.

## 20. Goal161 profile-neutral world change and generated gameplay saves

Goal161 keeps the Goal160 sealed transaction but makes its semantic validator profile-neutral: exact AcceptedMechanics and compatibility summaries plus generic RC projections are authority, not an all-selectable readiness assumption. Real core-only regeneration and history rollback remain `TRAVEL_CURRENT` while AcceptedMechanics stays intentionally incomplete.

Generated gameplay saves reuse the existing `UnifiedRuntimeSession` and serializer. Immutable content-addressed revisions bind project/world/source/package/authoring/history truth and canonical definition fingerprints; an atomic slot manifest selects current. Exact same-world loads do not Start/reset Runtime. World/package changes require explicit cached migration: cross-world map/transients reset, compatible definitions survive only by same-kind/same-ID canonical equality, incompatible references are dropped with reasons, and source revisions remain reusable after historical restore. Runtime, public GamePackage, FeatureModule catalog and Unity remain unchanged.

## 21. Goal167 generated choice branching

Goal167 keeps GamePackage and Runtime state as authority while adding deterministic authoring-time branch composition. Exact generated provenance binds Support, Challenge and Refuse choices; the overlay changes only declared generated dialogue nodes. Qualification executes real Runtime routes, including Support combat/manual turn-in, Challenge flee/victory resolution, Refuse non-mutation, atomic failing-choice rollback and two independent replays. Preview, journal and consequence projections use Runtime state/events, with metadata limited to human labels.

Branchable projects require v5 `CHOICE_CURRENT`; genuine v4 remains `CHOICES_PENDING` until an ordinary rebuild. Regeneration seals choice summary, overlay and persistent flag inventory. Exact save/continue and explicit definition-fingerprint migration preserve compatible flags, drop incompatible flags and prevent ghost journal rows. Runtime, public GamePackage schema, FeatureModule catalog, Unity and standalone/RC implementations remain unchanged.

## 22. Goal168 choice-driven relationships and multi-quest arcs

The relationship seam is a projection over exact generated dialogue provenance, Runtime flags/reputation and generated quest/encounter state. Assignment and ordering are deterministic from generated plan/package data and never depend on fixed IDs or counts. The overlay is confined to bound generated dialogue choices plus assigned generated quest `AutoStart`/relationship metadata.

Support starts and advances one assigned quest at a time through dialogue, exact catalog-qualified combat and manual turn-in until completion. Challenge flee/victory/recovery and Refuse use exact Runtime consequences. History v6, sealed regeneration/rollback, exact middle-arc continuation and explicit compatible-preserve/world-reset/incompatible-drop migration retain GamePackage and Runtime as authority.
Goal168 update: exact generated dialogue IDs now define actor/faction relationships, with uniquely assigned data-ordered generated quests forming sequential Support arcs. Challenge and Refuse remain exclusive Runtime consequences. All relationship combat consumes the exact Goal166 qualified-action catalog; v6 history, exact saves and explicit migration preserve Runtime/GamePackage authority.

## 23. Goal169 profile-neutral relationships and reactive regional events

Relationship qualification follows the selected profile instead of imposing a universal three-branch shape. Every relationship persists exact branch availability, requirement and qualification truth; absent branches execute no Runtime route. Exact combat consumes the existing qualified-action catalog across health, stat and status effects and accepts progress only when the exact descriptor matches a changed encounter state.

Reactive regional events are a controlled authoring-time overlay derived from Support completion, Challenge victory aftermath and Refusal fallout. Exact event-dialogue identity, source-derived reward/prerequisite fingerprints and deterministic reachable placement retain GamePackage as authority. Runtime reaches and resolves events only through ordinary relationship, movement, interaction, dialogue and choice commands. The state-backed projection/UI reads Runtime flags and exact event inventory.

History v7 makes event qualification the primary complete route while retaining independent combat, choice and relationship summaries. Genuine v6 remains rebuild-required. Regeneration seals event and relationship matrices; exact continuation and fingerprint migration preserve only compatible event state. Runtime, public GamePackage schema, Unity, standalone and RC implementations remain unchanged.

Goal169 product matrices and standalone launch are GREEN, but publication is `BLOCKED_AFTER_SINGLE_HIDDEN_SMOKE_MOVE_FRAME_ASSERTION`: the single permitted smoke emitted direction-only movement frame titles instead of the required explicit `Move.` prefix. The post-smoke fix emits `Move.<Direction>` without changing the Runtime command route; retry remains zero and an independent blocker audit is required.

## 24. Goal169A strict replay v7 correlation and post-fix smoke closure

Goal169A is a bounded continuation of Goal169, not another product system. It preserves the product-line seams and keeps Runtime independent from authoring. Replay proof is typed and complete, v7 history recomputes the relationship/event graph, Challenge events follow exact encounter-region provenance, event inventory is a semantic migration authority, and save migration exposes typed preserve/reset/drop facts without changing the persisted schema. The new one-shot cached payload proves explicit `Move.*` while the old Goal169 payload remains immutable. Status is `GREEN_ACCEPTABLE_CANDIDATE`, accepted=false, no human gate.
