local M = {}

M.manifest = {
  id = "ui/inventory_ui/v1",
  version = "0.1.0",
  category = "ui",
  title = "Inventory UI IR generator",
  purpose = "Generates deterministic inventory UI configuration IR for grid/list inventory modes, equipment slots, item details and item description display metadata.",
  capabilities = {
    "ui.inventory.generate",
    "ui.inventory_slots.configure",
    "ui.equipment_slots.configure",
    "ui.item_details.configure"
  },
  input_schema = {
    type = "object",
    fields = {
      mode = "optional grid or list",
      slot_count = "optional integer",
      equipment_slots = "optional array",
      categories = "optional array",
      display = "optional table"
    }
  },
  output_schema = {
    type = "object",
    fields = {
      panels = "array",
      slots = "array",
      equipment_slots = "array",
      filters = "array"
    }
  },
  config_schema = {
    type = "object",
    fields = {
      max_slots = "optional integer",
      default_mode = "optional string"
    }
  },
  deterministic = true,
  runtime_targets = { "editor", "unity2d", "unity3d", "unity_ui_ir", "codegen_ir" },
  unsafe_features = {}
}

local VALID_MODES = { grid = true, list = true }

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

local function default_categories()
  return {
    { id = "inventory/category_all", label = "All", tags = {} },
    { id = "inventory/category_equipment", label = "Equipment", tags = { "equipment" } },
    { id = "inventory/category_consumable", label = "Consumables", tags = { "consumable" } },
    { id = "inventory/category_quest", label = "Quest", tags = { "quest" } }
  }
end

local function validate_unique_ids(entries, diagnostics, target, code_prefix)
  if entries == nil then
    return
  end
  if not is_table(entries) then
    add_diag(diagnostics, "error", code_prefix .. ".invalid_collection", "Expected an array.", target)
    return
  end
  local seen = {}
  for index = 1, #entries do
    local entry = entries[index]
    local entry_target = target .. "[" .. tostring(index) .. "]"
    if not is_table(entry) then
      add_diag(diagnostics, "error", code_prefix .. ".invalid_entry", "Entry must be a table.", entry_target)
    elseif not is_ui_id(entry.id) then
      add_diag(diagnostics, "error", code_prefix .. ".invalid_id", "Entry id must use lowercase slash notation.", entry_target .. ".id")
    elseif seen[entry.id] then
      add_diag(diagnostics, "error", code_prefix .. ".duplicate_id", "Entry id is duplicated.", entry_target .. ".id")
    else
      seen[entry.id] = true
    end
  end
end

local function create_slots(slot_count, mode)
  local slots = {}
  for index = 1, slot_count do
    slots[index] = {
      id = string.format("inventory/slot_%03d", index),
      slot_index = index,
      kind = "item_slot",
      mode = mode,
      bindings = {
        item = string.format("inventory.slots.%03d.item", index),
        stack = string.format("inventory.slots.%03d.stack", index),
        durability = string.format("inventory.slots.%03d.durability", index)
      },
      actions = {
        select = string.format("inventory.slot_%03d.select", index),
        use = string.format("inventory.slot_%03d.use", index)
      }
    }
  end
  return slots
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if not is_table(config) then
    add_diag(diagnostics, "error", "ui.inventory.config.invalid", "Inventory UI config must be a table when provided.", "config")
    return false, diagnostics
  end
  if config.max_slots ~= nil and (type(config.max_slots) ~= "number" or config.max_slots < 1 or config.max_slots > 500) then
    add_diag(diagnostics, "error", "ui.inventory.config.invalid_max_slots", "max_slots must be between 1 and 500.", "config.max_slots")
  end
  if config.default_mode ~= nil and not VALID_MODES[config.default_mode] then
    add_diag(diagnostics, "error", "ui.inventory.config.invalid_default_mode", "default_mode must be grid or list.", "config.default_mode")
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

  local mode = source.mode or config.default_mode or "grid"
  if not VALID_MODES[mode] then
    add_diag(diagnostics, "error", "ui.inventory.invalid_mode", "Inventory UI mode must be grid or list.", "input.mode")
    mode = "grid"
  end

  local max_slots = config.max_slots or 120
  local slot_count = source.slot_count or 24
  if type(slot_count) ~= "number" or slot_count < 1 or slot_count > max_slots then
    add_diag(diagnostics, "error", "ui.inventory.invalid_slot_count", "slot_count must be within the configured range.", "input.slot_count")
    slot_count = 24
  end

  local equipment_slots = source.equipment_slots or {
    { id = "equipment/head", label = "Head", accepts = { "armor_head" } },
    { id = "equipment/body", label = "Body", accepts = { "armor_body" } },
    { id = "equipment/weapon", label = "Weapon", accepts = { "weapon" } }
  }
  local categories = source.categories or default_categories()

  validate_unique_ids(equipment_slots, diagnostics, "input.equipment_slots", "ui.inventory.equipment")
  validate_unique_ids(categories, diagnostics, "input.categories", "ui.inventory.category")

  local display = source.display or {
    show_stack = true,
    show_durability = true,
    show_rarity = true,
    show_description = true
  }

  return {
    ok = #diagnostics == 0,
    data = {
      ir_type = "ui.inventory",
      version = "0.1.0",
      mode = mode,
      panels = {
        { id = "inventory/root", kind = "window", anchor = "center", size = { width = 84, height = 78 }, visibility = { mode = "when", binding = "ui.inventory.open" } },
        { id = "inventory/items_panel", kind = mode .. "_panel", anchor = "left", size = { width = 52, height = 70 } },
        { id = "inventory/details_panel", kind = "item_details", anchor = "right", size = { width = 30, height = 70 } }
      },
      slots = create_slots(slot_count, mode),
      equipment_slots = copy_array(equipment_slots),
      filters = copy_array(categories),
      item_details = {
        bindings = {
          selected_item = "inventory.selected_item",
          description = "inventory.selected_item.description",
          rarity = "inventory.selected_item.rarity",
          durability = "inventory.selected_item.durability"
        },
        display = display
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
