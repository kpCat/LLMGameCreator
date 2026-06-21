# LLMGameCreator Official Product Plan

## Product identity

LLMGameCreator is a Game Assembly Workbench.

It is not a generator for one fixed RPG and not a runtime that depends on LLM calls for every action. It is a modular workbench for composing game types, generators, validation, runtime preview, and future runtime targets.

The core idea:

```text
LLM supplies semantic libraries, world bibles, rare authored content, and high-level design seeds.
Program modules generate mass content, validate it, compose it, simulate it, and run it deterministically.
```

## Non-negotiable principles

1. Runtime must not depend on live LLM calls for ordinary gameplay.
2. Generated player-facing content must obey the project language policy.
3. Technical identifiers remain stable ASCII/kebab_case.
4. Game systems are added as capability modules, not as one-off hardcoded features.
5. Modules declare inputs, outputs, requirements, conflicts, and compatibility.
6. Large worlds are generated lazily and persisted; they are not fully generated at game start.
7. Offscreen simulation uses abstract state, not full per-object simulation.
8. Existing pipeline remains valuable and should not be discarded:
   `LLM/generated artifact -> review -> approve -> assemble package -> activate package -> runtime preview -> smoke validation`.

## Current foundation already built

The current product slices established:

```text
strict LLM artifact contracts
batch preset generation
artifact review
approval decisions
package assembly
assembled package activation
runtime preview
generated content browser
dialogue/quest preview stubs
NPC/encounter map placement preview
product smoke runner
devflow state discipline
```

This is the vertical spine of the workbench. Future work should build around it, not restart from scratch.

## Target architecture

```text
GameBlueprint
+ ContentLanguagePolicy
+ CapabilityGraph
+ GeneratorCatalog
+ SemanticWorldModel
+ RuntimeModules
+ PresentationAdapters
+ CompatibilityRules
+ ValidationGates
+ ProductSmokeScenarios
```

## GameBlueprint

`GameBlueprint` describes what kind of game is being assembled.

Examples:

```text
realistic_city_survival
zombie_city_survival
fantasy_open_world_rpg
crime_sandbox
colony_strategy
dating_social_sim
anomaly_zone_survival
text_rpg
map_panel_rpg
```

A blueprint selects:

```text
world source
world scale
generation mode
presentation targets
simulation modules
content language
runtime modules
LLM role
procedural role
```

## World source modes

```text
procedural_world
imported_real_map
hand_authored_map
hybrid_imported_plus_generated
fixed_large_world
infinite_lazy_world
```

`imported_real_map` should eventually use OSM-like data or prepared map extracts, not a hard dependency on proprietary map rendering/cache rules.

## Generation modes

```text
offline_generation
reviewed_generation
lazy_runtime_generation
hybrid_offline_plus_lazy
```

Large and infinite worlds should use:

```text
seed + rules + generated cache + persisted deltas
```

instead of full upfront generation.

## Simulation LOD

World simulation should use levels of detail:

```text
active_bubble:
  full positions, visible actors, direct interactions, combat/dialogue/theft/etc.

local_zone:
  coarser position/schedule updates, not fully rendered.

abstract_region:
  population counters, faction control, event queues, economy pressure.

dormant_history:
  long-term trends, major events, aggregated consequences.
```

Events can happen away from the player, but they should be processed at the appropriate abstraction level.

## NPC classes

```text
named_important_npc:
  full memory, relationship graph, story hooks, deep state.

local_persistent_npc:
  stable identity, schedule, job, home, local memory.

generated_population_npc:
  generated when needed, may be promoted to persistent if interacted with.

crowd_agent:
  cheap temporary agent representing city flow; not persistent unless promoted.
```

## Content generation responsibility split

### LLM should generate

```text
world bible
faction ideologies
culture/style packs
rare unique NPCs
rare quest arcs
rare moral dilemmas
rare anomalies
semantic hooks
tone/speech style descriptions
```

### Program should generate

```text
mass NPCs
families/households
jobs
schedules
resources
loot
shops
quests from templates
encounters from patterns
placement
economy
transport
crime/police reaction
routine dialogue variants
offscreen events
state transitions
```

### Lua/data modules should define

```text
declarative templates
generator rules
effect descriptors
quest structures
compatibility metadata
content libraries
```

Runtime executes validated deterministic rules, not arbitrary generated code.

## Capability module system

Every system should become a capability module.

Example capability families:

```text
world_source.imported_map
world_source.procedural_regions
time.calendar
population.households
population.lifecycle
schedule.daily_life
economy.jobs
economy.shops
transport.routes
law.crime_police
survival.needs
quest.procedural_templates
dialogue.semantic_realizer
combat.personal
combat.mass_battle
faction.politics
event.offscreen_scheduler
presentation.topdown_2d
presentation.isometric_2d
presentation.3d_first_person
```

## Compatibility levels

Compatibility should not be binary only.

```text
compatible
compatible_with_adapter
degraded_but_usable
conflict
unsupported_yet
```

Examples:
- `top_down_2d` + `3d_first_person`: compatible with presentation adapters if they do not compete for the same scene.
- `schedule.daily_life` without `time.calendar`: conflict.
- `imported_real_map` + `fantasy_anomalies`: compatible with adapter/overlay.
- `survival.needs` + `visual_novel`: degraded but usable.

## Generator catalog

Every generator module should declare:

```text
generator_id
input contracts
output contracts
requires
optional_requires
provides
conflicts
supported world sources
supported presentation targets
generation modes
runtime cost
validation rules
```

This turns "batch generation" into controlled composition, not a pile of ad-hoc prompts.

## Semantic model

The semantic layer should eventually include:

```text
SemanticTag
SemanticTheme
SemanticMood
SemanticRole
SemanticFactionProfile
SemanticBiomeProfile
SemanticDistrictProfile
SemanticSettlementProfile
SemanticNpcArchetype
SemanticNpcNeed
SemanticQuestMotif
SemanticDialogueIntent
SemanticEventPattern
SemanticResourcePressure
SemanticRiskProfile
```

The program should use semantic context to select procedural templates and variations.

Example:

```text
district = industrial outskirts
time = night
crime_pressure = high
npc_role = warehouse guard
resource_pressure = medicine shortage
```

This should drive NPC creation, quest motifs, encounter risks, and dialogue intents.

## Real-world map branch

A future branch may support city/life/survival games on real map data.

Pipeline:

```text
map import
-> road/building/POI classification
-> district graph
-> building roles
-> population generation
-> household placement
-> job/economy generation
-> schedules/traffic
-> events/crime/police/survival
```

This should be a capability branch, not the only game type.

## Mass battle branch

A future branch may support Mount-and-Blade-like mass battle mechanics.

Capabilities:

```text
unit archetypes
formations
morale
commander orders
battlefield AI
personal combat bridge
offscreen battle resolution
```

This should be implemented as clean capability design, not copied code.

## Language policy

Player-facing generated content must obey `content_language`.

Technical identifiers must remain ASCII/kebab_case.

Minimum supported content languages:

```text
ru
uk
en
```

UI language is separate from game content language.

## Roadmap phases

### Phase A: Product spine

Status: mostly built.

```text
artifact contracts
batch generation
review/approval
package assembly
activation
runtime preview
generated content browser
map placement preview
smoke runner
```

### Phase B: Control and composition

```text
content language policy
official product plan
game blueprint
capability graph
generator catalog
compatibility validation
```

### Phase C: Semantic generation

```text
semantic world model
semantic resolver
procedural quest templates
semantic dialogue realizer
NPC archetypes
event patterns
resource pressures
```

### Phase D: Large/lazy worlds

```text
lazy region/district generation cache
offscreen simulation
world event scheduler
NPC lifecycle
population/household simulation
```

### Phase E: World-source branches

```text
procedural open world
imported real map world
hybrid real map + anomalies
hand authored map + generated overlays
```

### Phase F: Advanced gameplay branches

```text
crime/police
transport
economy
social/relationship simulation
survival needs
mass battles
factions/wars
anomalies/magic/zombies
```

## Rule for future slices

Every new product slice should answer:

```text
Which capability does it add?
Which module owns it?
What does it require?
What does it provide?
What does it conflict with?
How is it validated?
Which smoke scenario proves it?
How does it preserve the LLM/program responsibility split?
```

Do not add isolated features without placing them in this plan.
