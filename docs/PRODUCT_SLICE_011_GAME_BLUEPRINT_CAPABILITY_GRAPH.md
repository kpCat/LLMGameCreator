# Product Slice 011: GameBlueprint + Capability Graph Compatibility Foundation

## Goal

Introduce the first machine-readable composition layer for the Game Assembly Workbench.

This slice should add:
- `GameBlueprint`
- `CapabilityDefinition`
- `CapabilityRegistry`
- `CapabilityGraph`
- `CompatibilityRule` / validation logic
- `CompositionValidationResult`
- a few built-in capabilities from already completed product slices
- a product smoke scenario for capability compatibility

This is not a gameplay slice and not a UI slice. It is the foundation that lets the program reason about what kind of game is being assembled.

## Minimum built-in capability ids

```text
localization.content_language_policy
generation.strict_llm_artifacts
package.artifact_review
package.assembly
package.activation
world_source.procedural_package
presentation.topdown_2d_runtime_preview
runtime.preview_movement
dialogue.preview_lines
quest.preview_journal
map.generated_marker_placement
content.generated_npcs
content.generated_quests
content.generated_dialogues
content.generated_encounters
```

## Future/planned capability ids for validation tests

```text
world_source.imported_real_map
time.calendar
population.households
schedule.daily_life
event.offscreen_scheduler
quest.procedural_templates
dialogue.semantic_realizer
```

## Non-goals

Do not implement semantic world model, generator catalog plugins, imported maps, lazy worlds, procedural quests, runtime changes, or UI wizard.
