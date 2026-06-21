# Capability Graph and Generator Catalog Plan

## CapabilityDefinition

A capability should describe a game system at composition level.

Suggested fields:

```text
id
title
description
category
requires
provides
optional_requires
conflicts
supported_world_sources
supported_presentations
runtime_cost
generation_modes
maturity
```

## GeneratorModuleManifest

A generator module should describe what it can create.

Suggested fields:

```text
generator_id
title
input_contracts
output_contracts
requires_capabilities
provides_capabilities
conflicts
supported_world_sources
supported_presentations
can_run_offline
can_run_runtime_lazy
uses_llm
deterministic
validation_rules
```

## CompatibilityResult

The composition validator should return:

```text
ok
status
errors
warnings
missing_requirements
degraded_capabilities
adapter_requirements
```

## Initial capability categories

```text
world_source
presentation
time
population
economy
transport
law
survival
quest
dialogue
combat
faction
event
map
resource
localization
```

## Initial capabilities to model first

```text
localization.content_language_policy
world_source.procedural_package
presentation.topdown_2d_runtime_preview
quest.preview_journal
dialogue.preview_lines
map.generated_marker_placement
```

Then expand:

```text
world_source.imported_real_map
population.households
schedule.daily_life
quest.procedural_templates
dialogue.semantic_realizer
event.offscreen_scheduler
```
