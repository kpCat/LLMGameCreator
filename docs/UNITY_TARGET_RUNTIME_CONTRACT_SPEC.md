# Unity Target Runtime Contract Spec

## UnityGameArchiveManifest

Fields:

```text
archive_schema_version
game_id
title
content_language
target_profile_id
design_brief_id
data_packages
runtime_modules
lua_modules
ui_layouts
asset_manifests
audio_manifests
localization_files
world_streaming_policy
save_policy
build_export_policy
```

## UnityTargetProfile

Fields:

```text
target_profile_id
player_mode
rendering_modes
view_modes
input_profile
performance_budget
required_runtime_modules
optional_runtime_modules
asset_pipeline_profile
audio_pipeline_profile
```

## UnityRuntimeModuleContract

Fields:

```text
module_id
title
description
maturity
requires_capabilities
provides_capabilities
input_data_contracts
output_events
listened_events
ui_bindings
save_schema_id
can_run_offline
can_run_runtime
performance_class
implementation_status
```

## Dynamic UI Layout Contract

Fields:

```text
layout_id
theme_id
panels
widgets
bindings
input_actions
draggable_panels
visibility_rules
style_tokens
asset_refs
```

Widgets examples:

```text
health_bar
resource_orb
inventory_grid
quest_journal
dialogue_box
minimap
world_map
action_bar
skill_tree
technology_tree
build_menu
vehicle_dashboard
army_command_panel
```

## Asset/audio request contracts

Asset requests describe future ComfyUI/manual generation metadata only. Audio requests describe short SFX, ambience, loops and music theme metadata only. Do not implement providers in this slice.

## World streaming policy

Fields:

```text
world_scale
chunk_size
active_radius
background_simulation_mode
persistent_entity_policy
generated_entity_budget
npc_materialization_policy
quest_materialization_policy
save_dirty_deltas_only
```
