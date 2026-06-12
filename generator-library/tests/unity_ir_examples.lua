local M = {}

local runtime_plan_input = {
  target_runtime_id = "unity/runtime/default",
  scene_refs = { "scene/village/main", "scene/village/interior" },
  game_loop_mode = "mixed",
  input_mode = "keyboard_mouse",
  required_adapter_capabilities = { "adapter/scene/spawn", "adapter/ui/bindings" },
  runtime_features = { scene_streaming = false, ui_adapter = true },
  persistence_requirements = { profile_slots = 3, autosave = true },
  compile_checks = { "check/assembly/metadata" },
  smoke_checks = { "check/scene/open", "check/ui/document_refs" }
}

local scene_ir_input = {
  scenes = {
    {
      id = "scene/village/main",
      category = "world",
      world_ref = "world/blueprint/frontier",
      map_ref = "map/village/main",
      prefab_slots = {
        { id = "prefab_slot/player", prefab_ref = "prefab/player/controller" },
        { id = "prefab_slot/camera", prefab_ref = "prefab/camera/main" }
      },
      entity_slots = {
        { id = "entity_slot/elder", entity_ref = "npc/village/elder" }
      },
      spawn_points = {
        { id = "spawn/player/start", tag = "player_start", x = 2, y = 4 }
      },
      camera = { mode = "orthographic_2d", follow_ref = "entity/player" },
      environment = { lighting = "day", weather_ref = "weather/clear" }
    }
  }
}

local ui_ir_input = {
  documents = {
    {
      id = "unity_ui/document/main_hud",
      source_ui_ref = "ui/hud/main",
      canvas = { render_mode = "screen_space_overlay", sorting = 0 },
      screen_regions = { top_left = "minimap", bottom = "quick_slots" },
      panel_refs = { "ui/panel/hud", "ui/panel/minimap", "ui/panel/quest_tracker" },
      bindings = {
        { id = "binding/player/health", source_ref = "stat/player/health", target_element_ref = "ui/element/health_bar" }
      },
      action_refs = { "action/open_inventory", "action/open_journal" }
    }
  }
}

local codegen_ir_input = {
  units = {
    {
      id = "codegen/unit/player_presenter",
      role = "presenter",
      namespace = "LLMGameCreator.Unity.Generated",
      class_name = "PlayerPresenterMetadata",
      component_kind = "ui_presenter",
      hooks = {
        { id = "hook/player/health_changed", event_name = "HealthChanged", method_name = "OnHealthChanged" }
      },
      descriptors = { binds = { "binding/player/health" } },
      depends_on_units = {}
    },
    {
      id = "codegen/unit/scene_adapter",
      role = "adapter",
      namespace = "LLMGameCreator.Unity.Generated",
      class_name = "SceneAdapterMetadata",
      component_kind = "scene_adapter",
      hooks = {
        { id = "hook/scene/entered", event_name = "SceneEntered", method_name = "OnSceneEntered" }
      },
      depends_on_units = { "codegen/unit/player_presenter" }
    }
  },
  validation_metadata = { compile_expected = "not_run", smoke_expected = "not_run" }
}

local invalid_codegen_input = {
  units = {
    {
      id = "bad id",
      role = "adapter",
      namespace = "Invalid Namespace",
      class_name = "1Broken",
      method_body = "forbidden source body"
    }
  }
}

function M.run(modules)
  local results = {}
  results.runtime_plan = modules.unity_runtime_plan.generate(runtime_plan_input, {})
  results.scene_ir = modules.unity_scene_ir.generate(scene_ir_input, {})
  results.ui_ir = modules.unity_ui_ir.generate(ui_ir_input, {})
  results.codegen_ir = modules.unity_csharp_codegen_ir.generate(codegen_ir_input, {})
  results.invalid_codegen = modules.unity_csharp_codegen_ir.generate(invalid_codegen_input, {})
  return results
end

return M
