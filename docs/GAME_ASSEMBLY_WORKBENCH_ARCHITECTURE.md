# Game Assembly Workbench Architecture

## Layers

```text
1. Product UI
2. Application services
3. Generator catalog
4. Semantic model
5. Validation gates
6. Package assembly
7. Runtime modules
8. Presentation adapters
9. Product smoke tests
```

## Existing pipeline

```text
LLM Artifacts / batch presets
-> Artifact Review
-> approval decisions
-> Package Assembly
-> Use assembled package as current
-> Runtime Preview
-> Product Smoke
```

This pipeline remains the foundation.

## Future composition pipeline

```text
GameBlueprint
-> CapabilityGraph
-> CompatibilityValidation
-> GeneratorPlan
-> ContentLanguagePolicy
-> LLM semantic seed generation
-> procedural generation
-> review/approval where needed
-> package assembly
-> runtime preview
```

## Runtime world architecture

Runtime should not load the whole world in maximum detail.

```text
active bubble:
  entities, player interaction, precise pathing/rendering

local simulation:
  schedules, traffic approximation, nearby events

region simulation:
  abstract economy, factions, crime, events

world history:
  long-term changes, global flags, major events
```

## Persistence

Generated world content should be persisted:

```text
world seed
generated regions
generated districts
persistent NPCs
important event history
player-caused deltas
```

Temporary crowd agents and abstract state may remain transient unless promoted.

## LLM usage budget policy

LLM should be used when one of these is true:

```text
new semantic library is needed
rare unique NPC/story/event is needed
style pack is needed
content cannot be made interesting from existing templates
```

LLM should not be used for:

```text
routine schedules
mass population
routine fetch/talk/scout/repair quests
coordinates
combat math
economy values
pathfinding
state transitions
```

## Clean-room mechanics policy

External games can inspire capability design. Do not depend on copying code/assets into reusable layers.

Translate inspiration into:

```text
capability
inputs
outputs
rules
constraints
runtime cost
validation
```

Examples:
- IFZ-like map survival -> imported map/city survival capability branch.
- Mount-and-Blade-like battles -> mass battle capability branch.
