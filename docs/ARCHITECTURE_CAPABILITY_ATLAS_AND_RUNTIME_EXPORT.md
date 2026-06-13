# LLMGameCreator — Capability Atlas, Lua Library Growth and Unity Runtime Export

Status: strategic architecture draft  
Recommended path: `docs/ARCHITECTURE_CAPABILITY_ATLAS_AND_RUNTIME_EXPORT.md`  
Scope: design-time architecture, generator-library strategy, artifact contracts, Unity/runtime export direction  
Non-scope: immediate Codex implementation task, direct Unity runtime implementation, unrestricted Lua/code generation  

---

## 1. Purpose

This document defines the strategic architecture needed to prevent LLMGameCreator from becoming an endless sequence of disconnected goals, batches and manually wired C# features.

The target is not to generate a single fixed game type. The target is to build a modular game creation platform where new mechanics, genres and runtime targets are added through:

```text
Game concept / discussion
        ↓
Game profile
        ↓
Capability Atlas
        ↓
Feature bundles
        ↓
Generator plans
        ↓
Lua / LLM draft generation
        ↓
Artifact contracts
        ↓
C# validation and normalization
        ↓
Compiled runtime data
        ↓
Unity runtime shell
```

The main goal is to support rich, flexible, extensible game generation without forcing the user to manually add large amounts of C# glue every time a new mechanic is introduced.

---

## 2. Core Thesis

LLMGameCreator should not be a system where:

```text
LLM generates arbitrary Lua
Lua generates arbitrary C#
C# generates arbitrary Unity code
Unity executes whatever was generated
```

That path is unsafe, hard to validate, hard to reproduce, and hard to debug.

The correct model is:

```text
LLM / Lua generate data and IR
C# validates contracts and compiles artifacts
Unity executes a stable runtime shell
```

In other words:

```text
LLM = designer / planner / draft generator / reviewer
Lua = deterministic generator and DSL library
C# = validator / registry / compiler / artifact builder / exporter
Unity = runtime player and presentation shell
SQLite or equivalent DB = compiled runtime lookup and save state
```

---

## 3. Existing Direction to Preserve

The current repository already moves in a useful direction:

- `GamePackage` remains the source of truth for playable content.
- Lua generator-library is treated as a reusable generator/configuration library.
- Design DB stores editor-side design knowledge, registry metadata, plans, artifacts and validation results.
- Existing safe patch flow uses preview, explicit approval, dry-run, rollback, audit and validation.
- Lua restrictions already point toward deterministic, sandboxed, JSON-serializable outputs.

This document does not replace that direction. It adds a higher-level architecture layer so future development does not become a flat list of disconnected feature goals.

---

## 4. Key Problem

The current risk is not that the project lacks features. The risk is that feature growth may become linear and manual:

```text
Need mechanic X
→ add C# domain fields
→ add validator
→ add Lua batch
→ add UI
→ add runtime glue
→ add Unity glue
→ repeat forever
```

That approach can work for a few goals, but it does not scale to a flexible game generator.

The missing layer is a machine-readable map of:

```text
what the system can generate,
which modules provide which capabilities,
which contracts they output,
which validators must run,
which runtime targets can consume them,
and which Unity/runtime features are required.
```

That layer is the Capability Atlas.

---

## 5. Capability Atlas

### 5.1 Definition

The Capability Atlas is a structured registry of game creation capabilities.

It describes domains, capabilities, dependencies, feature bundles, artifact contracts, runtime targets, validators and reference game profiles.

It should not describe only files or batches. It should describe what the library can do.

### 5.2 Conceptual shape

```json
{
  "atlas_version": "0.1",
  "domains": [
    "core",
    "world",
    "entity",
    "item",
    "equipment",
    "inventory",
    "dialogue",
    "combat",
    "party",
    "magic",
    "skill",
    "quest",
    "faction",
    "reputation",
    "city_builder",
    "automation",
    "semantic_text",
    "morphology",
    "unity_ir",
    "runtime_db"
  ],
  "runtime_targets": [
    "debug",
    "headless_simulation",
    "unity2d",
    "unity3d",
    "isometric",
    "pseudo3d",
    "first_person_grid"
  ]
}
```

### 5.3 Capability entry

Example:

```json
{
  "id": "inventory.paper_doll_grid/v1",
  "domain": "inventory",
  "title": "Grid inventory with paper-doll equipment overlay",
  "purpose": "Supports item shapes, equipment slots, character body layout and drag/drop inventory UI.",
  "requires": [
    "item.size_2d/v1",
    "equipment.slot_layout/v1",
    "ui.drag_drop/v1",
    "unity_ir.inventory_panel/v1"
  ],
  "outputs": [
    "inventory_schema_v1",
    "equipment_layout_v1",
    "unity_inventory_ui_ir_v1"
  ],
  "validators": [
    "inventory.no_overlap",
    "equipment.slot_compatible",
    "item.shape_valid"
  ],
  "runtime_targets": [
    "unity2d",
    "unity3d",
    "pseudo3d"
  ]
}
```

### 5.4 Feature bundle

A feature bundle is a curated capability group. It is larger than a single Lua module but smaller than a whole game profile.

Example:

```json
{
  "id": "feature/mm7_party_inventory/v1",
  "title": "Party RPG inventory and equipment",
  "capabilities": [
    "party.multi_character/v1",
    "inventory.paper_doll_grid/v1",
    "equipment.body_slots/v1",
    "item.rarity_affixes/v1",
    "ui.party_inventory/v1"
  ]
}
```

---

## 6. Game Profiles

Game profiles are reference configurations. They are not hardcoded game templates. They are used to test whether the architecture is flexible enough.

A profile selects feature bundles and runtime targets.

### 6.1 Might-and-Magic-like Party RPG

Reference: party-based open-world pseudo-3D RPG.

Capabilities:

```text
world.region_based
world.dungeon_maps
world.pseudo3d_exploration
party.multi_character
inventory.paper_doll_grid
equipment.body_slots
combat.realtime_turn_hybrid
magic.spellbook
skill.training
quest.multi_stage
dialogue.npc_dialogue
faction.reputation
shop.services
runtime_db.static_content
unity_ir.pseudo3d_party_view
```

Important test points:

- party movement;
- pseudo-3D from 2D/flat assets;
- optional real-time and turn-based combat;
- inventory where item textures can be fitted or overlaid onto character/equipment UI;
- skills, spells, trainers, shops, reputation and quests;
- large or potentially infinite generated world regions.

### 6.2 Anno-1404-like City Builder with War/Conquest

Reference: city building, economy, logistics, territory and conflict.

Capabilities:

```text
city_builder.population_needs
city_builder.production_chains
economy.trade_routes
resource.storage
building.placement
building.upgrades
faction.diplomacy
territory.control
combat.army_or_naval_conflict
ui.city_builder_hud
simulation.paused_planning
runtime_db.production_lookup
unity_ir.city_builder_view
```

Important test points:

- production chains;
- population needs;
- islands/regions/territories;
- trade routes;
- conquest and defense;
- UI-heavy management;
- performance with many buildings and resources.

### 6.3 The-Last-Sovereign-like Narrative Political RPG

Reference: story-heavy RPG with factions, route logic, mature/adult-capable themes and long-term consequences.

Capabilities:

```text
dialogue.branching
dialogue.relationship_memory
narrative.flags
quest.route_logic
faction.political_state
reputation.multi_axis
character.relationships
event.consequence_chain
semantic_text.narrative_style
content_rating.policy_flags
ui.dialogue_focus
```

Important test points:

- branching scenes;
- relationship and political consequences;
- long-term flags;
- faction influence;
- optional adult/mature content as tagged content, not uncontrolled generation;
- route and event consistency;
- ability to validate content category, tone and platform restrictions.

Adult/NSFW-capable content must be treated as explicitly tagged data with policy, platform and user-controlled filters. It must not be generated or surfaced accidentally.

### 6.4 Factorio-like Automation

Capabilities:

```text
automation.recipe_graph
automation.machine_catalog
automation.conveyor_grid
automation.power_network
resource.nodes
pollution.spread
machine.failure_modes
ui.automation_hud
simulation.realtime
runtime_db.production_indexes
unity_ir.automation_view
```

Important test points:

- production graph;
- machines and belts/conveyors;
- power;
- resources;
- throughput;
- maintenance and failure;
- technical descriptions.

### 6.5 Survival Sandbox

Capabilities:

```text
survival.body_state
weather.temperature
weather.wetness
item.condition
equipment.protection
shelter.coverage
resource.food_water
injury.wounds
location.hazards
semantic_text.environment_description
```

Important test points:

- cold, wetness, hunger, thirst, fatigue;
- item condition;
- clothing and shelter;
- biome hazards;
- material reactions.

### 6.6 Visual Novel / Dialogue RPG

Capabilities:

```text
dialogue.branching
dialogue.choice_effects
relationship.axes
character.memory
scene.graph
route.flags
ui.dialogue_focus
semantic_text.speech_styles
```

Important test points:

- strong dialogue structure;
- character memory;
- route states;
- emotion/tone;
- controlled text generation.

### 6.7 Tactical RPG

Capabilities:

```text
combat.tactical_grid
ability.targeting
status_effect.duration
party.units
enemy.ai_profiles
terrain.effects
ui.tactical_ui
runtime_db.ability_lookup
```

Important test points:

- grid combat;
- targeting;
- turn order;
- statuses;
- terrain effects;
- ability formulas.

### 6.8 Dungeon Crawler / Roguelike

Capabilities:

```text
world.procedural_dungeon
encounter.tables
loot.tables
combat.turn_based
inventory.compact
status_effects
trap.hazards
runtime_db.seeded_generation
```

Important test points:

- deterministic procedural generation;
- compact data;
- replayability;
- strong validation of reachability and progression.

### 6.9 Alien / Non-Human Perception Game

Capabilities:

```text
species.perception_model
semantic_text.sensory_palette
dialogue.nonhuman_speech
faction.social_rules
memory.nonhuman_memory
environment.biological_signals
```

Important test points:

- speech through smell, heat, moisture, electricity, spores or memory;
- non-human priorities;
- different social logic;
- text variation without generic fantasy speech.

### 6.10 Colony Sim / RimWorld-like

Capabilities:

```text
simulation.citizen_needs
schedule.jobs
ai.work_priorities
faction.raid_events
base_building
resource.storage
social.relationships
event.storyteller
ui.colony_hud
```

Important test points:

- many agents;
- jobs and schedules;
- social memory;
- raids/events;
- performance.

---

## 7. Artifact Contracts

Artifact contracts are typed outputs that can be validated, stored, compiled or exported.

The system should avoid vague outputs such as "some JSON". Every generated artifact should have a declared kind and schema.

Core proposed artifact contracts:

```text
game_package_patch_v1
semantic_pack_v1
text_pack_v1
morphology_pack_v1
formula_pack_v1
dialogue_pack_v1
unity_ir_v1
runtime_db_build_plan_v1
asset_request_pack_v1
audio_request_pack_v1
generator_library_module_proposal_v1
```

### 7.1 semantic_pack_v1

Contains traits, property channels, archetypes, material rules, NPC/social rules, faction rules, item descriptors and procedural hooks.

### 7.2 text_pack_v1

Contains text intents, phrase plans, speech styles, templates, surface variants, forbidden phrasings and style constraints.

### 7.3 morphology_pack_v1

Contains lexemes, grammatical features, inflection tables, agreement hints and pronoun data.

Russian morphology should not be left entirely to an LLM at runtime. It should be compiled offline and looked up quickly at runtime.

### 7.4 unity_ir_v1

Contains Unity-facing data, not arbitrary Unity C# code.

Possible sections:

```text
prefab bindings
scene layout
camera mode
UI screens
input actions
audio event refs
vfx event refs
animation state refs
runtime component requirements
```

### 7.5 runtime_db_build_plan_v1

Defines how validated content should be compiled into a runtime database.

Possible sections:

```text
schema_version
tables
indexes
source_artifacts
static_content_tables
text_lookup_tables
semantic_lookup_tables
save_overlay_tables
migration_rules
```

---

## 8. Runtime DB Strategy

Unity should not load thousands or hundreds of thousands of loose JSON files at runtime.

Recommended structure:

```text
runtime.db       read-only compiled content shipped with game build
save_001.db      mutable save/world state
asset_index      asset references and hashes
build_manifest   versions, content hashes, schema versions
```

### 8.1 runtime.db

Stores static content:

```text
items
entities
skills
spells
dialogues
dialogue_lines
quests
factions
locations
chunks
semantic_traits
text_templates
material_reactions
formulas
asset_refs
ui_screens
unity_bindings
```

### 8.2 save.db

Stores mutable state:

```text
world_variables
entity_state
npc_memory
relationships
inventory_state
quest_state
chunk_deltas
visited_locations
runtime_event_log
```

Read-only content and mutable player state must be separated. This simplifies updates, modding, patching and debugging.

---

## 9. Unity Runtime Shell

The final runtime target should be a stable Unity project that consumes compiled data and IR.

It should not know whether content came from an LLM, Lua generator, hand-written JSON or imported packs.

Suggested Unity modules:

```text
UnityGameRuntime.Core
UnityGameRuntime.DataAccess
UnityGameRuntime.AssetBinding
UnityGameRuntime.SceneBinding
UnityGameRuntime.EntityRuntime
UnityGameRuntime.InteractionRuntime
UnityGameRuntime.DialogueRuntime
UnityGameRuntime.CombatRuntime
UnityGameRuntime.InventoryRuntime
UnityGameRuntime.PartyRuntime
UnityGameRuntime.UIRuntime
UnityGameRuntime.AudioRuntime
UnityGameRuntime.VfxRuntime
```

The runtime shell should be extensible through stable data contracts and adapter components, not through arbitrary generated code.

---

## 10. LLM Roles

Local models should be used according to task type.

### 10.1 Larger model role

A stronger model such as a 26B A4B instruct model is suitable for:

```text
game concept discussion
lore generation
faction design
character design
complex narrative conflicts
route logic
high-level feature negotiation
reviewing contradictions
```

These tasks are fewer and benefit from a more flexible model.

### 10.2 Smaller model role

A faster model such as a 4B/E4B instruct model is suitable for:

```text
batch semantic records
material reaction packs
item descriptions
location hooks
NPC reaction variants
chunk seed drafts
speech style variants
template variants
JSON shard generation
```

These tasks are numerous and benefit from speed.

### 10.3 Prompt strictness

Both model types must receive strict prompts:

```text
copy enum/id fields exactly
do not translate machine ids
do not invent major facts unless proposed_new_facts is allowed
return only the requested artifact contract
use JSON schema when possible
mark assumptions explicitly
separate facts from proposals
```

### 10.4 Validation is still mandatory

Validation is not an insult to the model. It is the production boundary.

The system must validate:

```text
JSON validity
schema validity
ids and enum preservation
required fields
dependency closure
unknown facts
unsafe content
duplicate records
formula bounds
quest graph validity
dialogue graph validity
Unity IR references
runtime DB build plan validity
```

---

## 11. Generator Library Growth Mode

LLMGameCreator can eventually help grow its own Lua generator-library, but only through staged proposals.

Pipeline:

```text
User requests new capability
        ↓
Capability Atlas search
        ↓
Existing capability found?
        ↓
If missing: create capability proposal
        ↓
Generate module spec, schemas, examples and Lua draft
        ↓
Static checks
        ↓
Manifest checks
        ↓
Sandbox/security checks
        ↓
Example execution checks
        ↓
Staging
        ↓
Human approval
        ↓
Generator-library import
```

The LLM should not directly push a new trusted module into the active library.

Generated library modules must remain:

```text
deterministic
sandbox-compatible
JSON-serializable
schema-declared
capability-declared
validated by examples
```

---

## 12. Validation Pipeline

Validation should be layered so the user does not manually review every generated record.

### Level 0 — syntax

```text
valid JSON
valid Lua syntax when Lua is involved
valid manifest shape
```

### Level 1 — schema

```text
artifact schema valid
required fields present
enum/id fields preserved
```

### Level 2 — contract

```text
capability dependencies satisfied
runtime targets supported
input/output contracts compatible
```

### Level 3 — semantic checks

```text
no obvious contradictions
no duplicate records
no impossible material reactions
no invalid quest/dialogue graph transitions
```

### Level 4 — simulation

```text
headless runtime smoke test
quest reachability
combat formula bounds
economy/resource sanity
production graph sanity
```

### Level 5 — Unity export dry-run

```text
Unity IR references valid
prefab bindings resolvable
UI screens compile to expected layout model
runtime DB build plan applies
```

### Level 6 — human approval

Required only for:

```text
new capability domains
new runtime targets
major game profile changes
important lore/canon changes
new unsafe or sensitive content categories
```

---

## 13. How This Avoids Infinite Development

The architecture must not require a custom C# subsystem for every mechanic.

The goal is to implement a small number of stable platform layers:

```text
Capability Atlas registry
Feature bundle resolver
Artifact contract registry
Validation pipeline
Generator plan system
Runtime DB compiler
Unity IR exporter
Unity runtime shell
```

Then most future features are added as:

```text
capability entries
feature bundles
Lua generator modules
artifact schemas
validators
Unity IR extensions
runtime components only when genuinely required
```

This is still real development, but it is not endless ad-hoc wiring.

---

## 14. Practical Task Plan

This section is the concrete plan. It intentionally avoids a huge coding sprint.

### Phase 0 — Freeze the strategic model

Deliverable:

```text
docs/ARCHITECTURE_CAPABILITY_ATLAS_AND_RUNTIME_EXPORT.md
```

No C# changes.

Acceptance:

```text
document exists
architecture terms are defined
reference profiles are listed
non-goals are explicit
```

### Phase 1 — Add Capability Atlas skeleton

Deliverables:

```text
generator-library/atlas/capability_atlas.schema.json
generator-library/atlas/capability_atlas.json
generator-library/atlas/reference_profiles.json
docs/CAPABILITY_ATLAS.md
```

Scope:

```text
static JSON only
no execution
no UI
no GamePackage changes
```

Acceptance:

```text
JSON files validate
profiles reference capability ids
no missing referenced capability ids
```

### Phase 2 — Import Atlas into Design DB

Deliverables:

```text
C# atlas import service
Design DB tables for domains/capabilities/feature_bundles/profiles
read-only query API
tests
```

Scope:

```text
no generator execution
no Unity export
no package mutation
```

Acceptance:

```text
import atlas
query by domain
query by profile
list missing dependencies
```

### Phase 3 — Game Profile Selection

Deliverables:

```text
profile selection service
profile-to-capability resolver
dependency closure validator
simple preview report
```

Acceptance:

```text
select Might-and-Magic-like profile
select Anno-like profile
see required capabilities and missing modules
```

### Phase 4 — Artifact Contract Registry

Deliverables:

```text
artifact_contracts.schema.json
artifact_contracts.json
C# artifact contract importer
validator binding map
```

Acceptance:

```text
semantic_pack_v1, text_pack_v1, unity_ir_v1, runtime_db_build_plan_v1 are declared
artifacts can be validated by kind
```

### Phase 5 — Semantic/Text/Morphology Pack Contracts

Deliverables:

```text
semantic_pack_v1 schema
text_pack_v1 schema
morphology_pack_v1 schema
sample packs
validator tests
```

Acceptance:

```text
small model generated records can be normalized into these contracts
invalid enum/id drift is detected
Russian morphology fields are explicit
```

### Phase 6 — Runtime DB Build Plan Prototype

Deliverables:

```text
runtime_db_build_plan_v1 schema
compiler prototype for a small subset
runtime.db generated from sample GamePackage and text/semantic packs
```

Scope:

```text
small subset only
no full Unity runtime yet
```

Acceptance:

```text
creates SQLite DB
creates indexes
loads sample items/dialogues/text templates
query tests pass
```

### Phase 7 — Unity IR Foundation

Deliverables:

```text
unity_ir_v1 schema
sample Unity IR for simple RPG scene
sample Unity IR for pseudo-3D party view
sample Unity IR for inventory UI
```

Scope:

```text
data only
no generated Unity C# yet
```

Acceptance:

```text
IR validates
asset refs are declared
UI screens are represented as data
```

### Phase 8 — Unity Runtime Shell Prototype

Deliverables:

```text
minimal Unity project shell
runtime.db loader
asset binding loader
simple scene/entity/UI binding
```

Scope:

```text
small reference demo only
not full game
```

Acceptance:

```text
load runtime.db
show a map/scene
show one NPC interaction
show inventory stub
```

### Phase 9 — Library Growth Mode

Deliverables:

```text
capability proposal artifact
generator module proposal artifact
staging validation
manual approval path
```

Acceptance:

```text
LLM can propose a new generator module
proposal is validated
unsafe proposals are rejected
approved proposals remain staged until explicitly imported
```

---

## 15. Immediate Recommendation

Place this document here:

```text
docs/ARCHITECTURE_CAPABILITY_ATLAS_AND_RUNTIME_EXPORT.md
```

Do not place it in repository root unless it is only a short pointer from `README.md`.

Recommended follow-up after committing this document:

```text
Create a small non-code task to add:
generator-library/atlas/capability_atlas.schema.json
generator-library/atlas/capability_atlas.json
generator-library/atlas/reference_profiles.json
docs/CAPABILITY_ATLAS.md
```

This should be a compact data/documentation task, not a large C# implementation.

---

## 16. Final Direction

The long-term architecture should be:

```text
Design discussion
        ↓
Game profile
        ↓
Capability Atlas
        ↓
Feature bundle resolution
        ↓
Generator plans
        ↓
Lua/LLM draft generation
        ↓
Artifact contracts
        ↓
C# validation/normalization
        ↓
Compiled runtime.db + unity_ir
        ↓
Unity Runtime Shell
```

This keeps the project extensible without requiring millions of lines of code or endless manual wiring.

The core principle:

```text
Generate flexible data and IR.
Validate everything.
Compile for runtime.
Let Unity execute a stable shell.
Do not let the LLM become the runtime authority.
```
