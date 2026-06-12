local M = {}

M.manifest = {
  id = "ui/hud_layout/v1",
  version = "0.1.0",
  category = "ui",
  title = "HUD layout IR generator",
  purpose = "Generates deterministic HUD layout IR for minimal, RPG, automation, city-builder, dialogue-focused and tactical UI modes.",
  capabilities = {
    "ui.hud.generate",
    "ui.status_bar.generate",
    "ui.stat_bar.generate",
    "ui.action_slots.generate",
    "ui.build_menu.layout"
  },
  input_schema = {
    type = "object",
    fields = {
      hud_mode = "optional string",
      status_bars = "optional array",
      stat_bars = "optional array",
      quick_slot_count = "optional integer",
      build_menu = "optional table"
    }
  },
  output_schema = {
    type = "object",
    fields = {
      hud_mode = "string",
      panels = "array",
      elements = "array",
      screen_regions = "array"
    }
  },
  config_schema = {
    type = "object",
    fields = {
      default_hud_mode = "optional string",
      max_quick_slots = "optional integer"
    }
  },
  deterministic = true,
  runtime_targets = { "editor", "unity2d", "unity3d", "unity_ui_ir", "codegen_ir" },
  unsafe_features = {}
}

local VALID_MODES = {
  minimal_hud = true,
  rpg_hud = true,
  automation_hud = true,
  city_builder_ui = true,
  dialogue_focus = true,
  tactical_ui = true
}

local function diagnostic(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function add_diag(diagnostics, severity, code, message, target)
  diagnostics[#diagnostics + 1] = diagnostic(severity, code, message, target)
end

local function is_table(value)
  return type(value) == "table"
end

local function is_non_empty_string(value)
  return type(value) == "string" and value ~= ""
end

local function is_ui_id(value)
  return is_non_empty_string(value) and value:match("^[a-z][a-z0-9_]*(/[a-z][a-z0-9_]*)*$") ~= nil
end

local function copy_array(source)
  local result = {}
  if is_table(source) then
    for index = 1, #source do
      result[index] = source[index]
    end
  end
  return result
end

local function panel(id, anchor, region, width, height)
  return {
    id = id,
    kind = "panel",
    anchor = anchor,
    region = region,
    size = { width = width, height = height },
    visibility = "always"
  }
end

local function element(id, kind, panel_id, bindings, actions)
  return {
    id = id,
    kind = kind,
    panel_id = panel_id,
    bindings = bindings or {},
    actions = actions or {},
    visibility = "always"
  }
end

local function append_status_bars(elements, bars, panel_id)
  local source = bars
  if not is_table(source) or #source == 0 then
    source = {
      { id = "hud/health", label = "Health", binding = "actor.health" },
      { id = "hud/stamina", label = "Stamina", binding = "actor.stamina" }
    }
  end
  for index = 1, #source do
    local bar = source[index]
    local id = bar.id or ("hud/status_bar_" .. tostring(index))
    elements[#elements + 1] = element(id, "status_bar", panel_id, {
      value = bar.binding or bar.value_binding or "actor.status",
      max = bar.max_binding or "actor.status_max"
    }, {})
    elements[#elements].label = bar.label or id
    elements[#elements].display = { show_label = bar.show_label ~= false, show_value = bar.show_value ~= false }
  end
end

local function append_stat_bars(elements, bars, panel_id)
  local source = bars or {}
  for index = 1, #source do
    local bar = source[index]
    local id = bar.id or ("hud/stat_bar_" .. tostring(index))
    elements[#elements + 1] = element(id, "stat_bar", panel_id, {
      value = bar.binding or "actor.stat"
    }, {})
    elements[#elements].label = bar.label or id
  end
end

local function append_action_slots(elements, count, panel_id)
  for index = 1, count do
    local id = string.format("hud/action_slot_%02d", index)
    elements[#elements + 1] = element(id, "action_slot", panel_id, {
      ability = string.format("ability.slot_%02d", index)
    }, {
      activate = string.format("action.slot_%02d.activate", index)
    })
    elements[#elements].slot_index = index
  end
end

local function append_build_menu(elements, build_menu, panel_id)
  if build_menu == nil then
    return
  end
  local categories = build_menu.categories or { "production", "logistics", "power" }
  local menu = element("hud/build_menu", "build_menu", panel_id, {
    selected_category = "build_menu.selected_category"
  }, {
    build = "build_menu.build_selected"
  })
  menu.categories = copy_array(categories)
  menu.compact = build_menu.compact ~= false
  elements[#elements + 1] = menu
end

local function validate_bars(source, diagnostics, target)
  if source == nil then
    return
  end
  if not is_table(source) then
    add_diag(diagnostics, "error", "ui.hud.invalid_bars", "Bars must be an array when provided.", target)
    return
  end
  for index = 1, #source do
    local bar = source[index]
    if not is_table(bar) then
      add_diag(diagnostics, "error", "ui.hud.invalid_bar", "Bar entries must be tables.", target .. "[" .. tostring(index) .. "]")
    elseif bar.id ~= nil and not is_ui_id(bar.id) then
      add_diag(diagnostics, "error", "ui.hud.invalid_bar_id", "Bar id must use lowercase slash notation.", target .. "[" .. tostring(index) .. "].id")
    end
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if not is_table(config) then
    add_diag(diagnostics, "error", "ui.hud.config.invalid", "HUD config must be a table when provided.", "config")
    return false, diagnostics
  end
  if config.default_hud_mode ~= nil and not VALID_MODES[config.default_hud_mode] then
    add_diag(diagnostics, "error", "ui.hud.config.invalid_mode", "Default HUD mode is not supported.", "config.default_hud_mode")
  end
  if config.max_quick_slots ~= nil and (type(config.max_quick_slots) ~= "number" or config.max_quick_slots < 0 or config.max_quick_slots > 30) then
    add_diag(diagnostics, "error", "ui.hud.config.invalid_max_quick_slots", "max_quick_slots must be between 0 and 30.", "config.max_quick_slots")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local source = input or {}
  local config = source.config or {}
  local diagnostics = {}
  local _, config_diags = M.validate_config(config)
  for index = 1, #config_diags do
    diagnostics[#diagnostics + 1] = config_diags[index]
  end

  local mode = source.hud_mode or config.default_hud_mode or "minimal_hud"
  if not VALID_MODES[mode] then
    add_diag(diagnostics, "error", "ui.hud.invalid_mode", "HUD mode is not supported.", "input.hud_mode")
    mode = "minimal_hud"
  end

  validate_bars(source.status_bars, diagnostics, "input.status_bars")
  validate_bars(source.stat_bars, diagnostics, "input.stat_bars")

  local max_quick_slots = config.max_quick_slots or 12
  local quick_slot_count = source.quick_slot_count or 0
  if type(quick_slot_count) ~= "number" or quick_slot_count < 0 or quick_slot_count > max_quick_slots then
    add_diag(diagnostics, "error", "ui.hud.invalid_quick_slot_count", "quick_slot_count must be within the configured range.", "input.quick_slot_count")
    quick_slot_count = 0
  end

  local panels = {}
  local elements = {}
  local regions = {
    { id = "screen/top", anchor = "top", role = "alerts" },
    { id = "screen/bottom", anchor = "bottom", role = "actions" },
    { id = "screen/left", anchor = "left", role = "navigation" },
    { id = "screen/right", anchor = "right", role = "context" },
    { id = "screen/center", anchor = "center", role = "primary" }
  }

  panels[#panels + 1] = panel("hud/root", "fill", "screen", 100, 100)
  panels[#panels + 1] = panel("hud/status_panel", "top_left", "screen/top", 34, 18)
  append_status_bars(elements, source.status_bars, "hud/status_panel")
  append_stat_bars(elements, source.stat_bars, "hud/status_panel")

  if mode == "rpg_hud" or mode == "tactical_ui" or mode == "automation_hud" then
    panels[#panels + 1] = panel("hud/action_panel", "bottom", "screen/bottom", 72, 14)
    append_action_slots(elements, quick_slot_count > 0 and quick_slot_count or 8, "hud/action_panel")
  end

  if mode == "dialogue_focus" then
    panels[#panels + 1] = panel("hud/dialogue_window", "bottom", "screen/bottom", 86, 28)
    elements[#elements + 1] = element("hud/dialogue_text", "dialogue_text", "hud/dialogue_window", { text = "dialogue.active_node.text" }, {})
    elements[#elements + 1] = element("hud/dialogue_choices", "choice_list", "hud/dialogue_window", { choices = "dialogue.active_node.choices" }, { choose = "dialogue.choice.select" })
  end

  if mode == "automation_hud" or mode == "city_builder_ui" then
    panels[#panels + 1] = panel("hud/build_panel", "bottom_right", "screen/right", 30, 42)
    append_build_menu(elements, source.build_menu or { compact = true }, "hud/build_panel")
  end

  if mode == "tactical_ui" then
    panels[#panels + 1] = panel("hud/tactical_panel", "right", "screen/right", 28, 54)
    elements[#elements + 1] = element("hud/turn_order", "turn_order", "hud/tactical_panel", { actors = "combat.turn_order" }, {})
    elements[#elements + 1] = element("hud/objective_tracker", "objective_tracker", "hud/tactical_panel", { objectives = "quest.tracked_objectives" }, {})
  end

  return {
    ok = #diagnostics == 0,
    data = {
      ir_type = "ui.hud_layout",
      version = "0.1.0",
      hud_mode = mode,
      panels = panels,
      elements = elements,
      screen_regions = regions,
      metadata = {
        deterministic = true,
        renderer_agnostic = true,
        adapter_hint = "future_unity_adapter"
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
