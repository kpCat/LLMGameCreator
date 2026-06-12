local M = {}

M.manifest = {
  id = "item/inventory_rules/v1",
  version = "0.1.0",
  category = "item",
  title = "Inventory Rules",
  purpose = "Build compact inventory constraint IR for stack limits, capacity, quest item locks, equipment slots and durability policies.",
  capabilities = { "inventory.rules.generate", "inventory.constraints.validate", "equipment.slot_rules.define" },
  input_schema = {
    item_catalog = "optional array of item definitions",
    containers = "optional array of inventory container definitions"
  },
  output_schema = {
    inventory_rules = "normalized rules IR",
    validation = "static inventory diagnostics and indexes"
  },
  config_schema = {
    default_capacity_slots = "optional positive integer",
    default_weight_limit = "optional non-negative number",
    strict_quest_items = "optional boolean",
    allowed_equipment_slots = "optional array of equipment slot ids"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "unity_ui_ir" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local DEFAULT_SLOTS = { "head", "body", "main_hand", "off_hand", "two_hand", "ring", "amulet", "tool", "utility" }

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function add(out, severity, code, message, target)
  out[#out + 1] = diag(severity, code, message, target)
end

local function is_array(value)
  if type(value) ~= "table" then
    return false
  end
  local count = 0
  for key, _ in pairs(value) do
    if type(key) ~= "number" or key < 1 or key % 1 ~= 0 then
      return false
    end
    count = count + 1
  end
  for index = 1, count do
    if value[index] == nil then
      return false
    end
  end
  return true
end

local function positive_integer(value)
  return type(value) == "number" and value > 0 and value % 1 == 0
end

local function non_negative_number(value)
  return type(value) == "number" and value >= 0
end

local function id_ok(value)
  if type(value) ~= "string" or value == "" then
    return false
  end
  if value:sub(1, 1) == "/" or value:sub(-1) == "/" then
    return false
  end
  if value:find("//", 1, true) then
    return false
  end
  return value:match("^[a-z0-9_/%-]+$") ~= nil
end

local function token_ok(value)
  return type(value) == "string" and value ~= "" and value:match("^[a-z0-9_%-]+$") ~= nil
end

local function list_to_set(list)
  local set = {}
  if type(list) == "table" then
    for index = 1, #list do
      if type(list[index]) == "string" then
        set[list[index]] = true
      end
    end
  end
  return set
end

local function clone_array(value)
  local result = {}
  if type(value) == "table" then
    for index = 1, #value do
      result[#result + 1] = value[index]
    end
  end
  return result
end

local function normalize_slot_rules(config, diagnostics)
  local slots = config.allowed_equipment_slots or DEFAULT_SLOTS
  if not is_array(slots) then
    add(diagnostics, "error", "inventory.slots_not_array", "allowed_equipment_slots must be an array.", "config.allowed_equipment_slots")
    slots = DEFAULT_SLOTS
  end
  local result = {}
  local seen = {}
  for index = 1, #slots do
    local slot = slots[index]
    if token_ok(slot) and not seen[slot] then
      seen[slot] = true
      result[#result + 1] = {
        slot = slot,
        accepts_two_handed = slot == "two_hand",
        unique = slot ~= "ring"
      }
    end
  end
  return result
end

local function normalize_container(container, config, diagnostics, target)
  if type(container) ~= "table" then
    add(diagnostics, "error", "inventory.container_not_table", "Container must be a table.", target)
    return nil
  end
  local id = container.id or "inventory/main"
  if not id_ok(id) then
    add(diagnostics, "error", "inventory.invalid_container_id", "Container id must be a lowercase slash id.", target .. ".id")
    return nil
  end
  local capacity = container.capacity_slots or container.slots or config.default_capacity_slots
  if not positive_integer(capacity) then
    add(diagnostics, "warning", "inventory.invalid_capacity", "Container capacity replaced by default_capacity_slots.", target .. ".capacity_slots")
    capacity = config.default_capacity_slots
  end
  local weight_limit = container.weight_limit
  if weight_limit == nil then
    weight_limit = config.default_weight_limit
  end
  if not non_negative_number(weight_limit) then
    add(diagnostics, "warning", "inventory.invalid_weight_limit", "Container weight limit replaced by default_weight_limit.", target .. ".weight_limit")
    weight_limit = config.default_weight_limit
  end
  return {
    id = id,
    title = container.title or container.name or id,
    capacity_slots = capacity,
    weight_limit = weight_limit,
    accepts_tags = clone_array(container.accepts_tags),
    rejects_tags = clone_array(container.rejects_tags),
    accepts_item_types = clone_array(container.accepts_item_types),
    locked = container.locked == true,
    ui_group = container.ui_group or "inventory"
  }
end

local function normalize_item_rule(item, config, allowed_slots, diagnostics, target)
  if type(item) ~= "table" then
    add(diagnostics, "error", "inventory.item_not_table", "Item must be a table.", target)
    return nil
  end
  if not id_ok(item.id) then
    add(diagnostics, "error", "inventory.invalid_item_id", "Item id must be a lowercase slash id.", target .. ".id")
    return nil
  end
  local stack_limit = 1
  if type(item.stack) == "table" then
    stack_limit = item.stack.stack_limit or item.stack.max_stack or 1
  else
    stack_limit = item.stack_limit or item.max_stack or 1
  end
  if not positive_integer(stack_limit) then
    add(diagnostics, "warning", "inventory.invalid_item_stack", "Item stack limit replaced by 1.", target .. ".stack_limit")
    stack_limit = 1
  end
  local equipment_slot = nil
  if type(item.equipment) == "table" then
    equipment_slot = item.equipment.slot
    if equipment_slot ~= nil and not allowed_slots[equipment_slot] then
      add(diagnostics, "error", "inventory.unsupported_equipment_slot", "Item equipment slot is not allowed.", target .. ".equipment.slot")
    end
  end
  local durability_policy = "none"
  if type(item.durability) == "table" and item.durability.enabled ~= false and type(item.durability.max) == "number" and item.durability.max > 0 then
    durability_policy = item.durability.break_behavior or "disable_effects"
  end
  return {
    item_id = item.id,
    stack_limit = stack_limit,
    quest_item_locked = item.quest_item == true and config.strict_quest_items == true,
    equipment_slot = equipment_slot,
    two_handed = type(item.equipment) == "table" and item.equipment.two_handed == true,
    durability_policy = durability_policy,
    weight = type(item.weight) == "number" and item.weight or 0,
    tags = clone_array(item.tags)
  }
end

function M.validate_config(config)
  local diagnostics = {}
  config = config or {}
  if type(config) ~= "table" then
    add(diagnostics, "error", "inventory.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.default_capacity_slots ~= nil and not positive_integer(config.default_capacity_slots) then
    add(diagnostics, "error", "inventory.invalid_default_capacity", "default_capacity_slots must be a positive integer.", "config.default_capacity_slots")
  end
  if config.default_weight_limit ~= nil and not non_negative_number(config.default_weight_limit) then
    add(diagnostics, "error", "inventory.invalid_default_weight", "default_weight_limit must be a non-negative number.", "config.default_weight_limit")
  end
  if config.allowed_equipment_slots ~= nil and not is_array(config.allowed_equipment_slots) then
    add(diagnostics, "error", "inventory.allowed_slots_not_array", "allowed_equipment_slots must be an array.", "config.allowed_equipment_slots")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  input = input or {}
  local config = (ctx and ctx.config) or input.config or {}
  local ok, config_diags = M.validate_config(config)
  for index = 1, #config_diags do
    diagnostics[#diagnostics + 1] = config_diags[index]
  end
  if not ok then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  config = {
    default_capacity_slots = config.default_capacity_slots or 32,
    default_weight_limit = config.default_weight_limit or 0,
    strict_quest_items = config.strict_quest_items == true,
    allowed_equipment_slots = config.allowed_equipment_slots or DEFAULT_SLOTS
  }
  local slot_rules = normalize_slot_rules(config, diagnostics)
  local allowed_slots = list_to_set(config.allowed_equipment_slots)

  local containers_input = input.containers or { { id = "inventory/main" } }
  if not is_array(containers_input) then
    add(diagnostics, "error", "inventory.containers_not_array", "containers must be an array.", "input.containers")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  local containers = {}
  local containers_by_id = {}
  for index = 1, #containers_input do
    local container = normalize_container(containers_input[index], config, diagnostics, "input.containers[" .. index .. "]")
    if container ~= nil then
      if containers_by_id[container.id] ~= nil then
        add(diagnostics, "error", "inventory.duplicate_container", "Duplicate container id ignored.", container.id)
      else
        containers[#containers + 1] = container
        containers_by_id[container.id] = container
      end
    end
  end

  local item_rules = {}
  local item_rules_by_id = {}
  local catalog = input.item_catalog or input.items or {}
  if type(catalog) == "table" then
    for index = 1, #catalog do
      local rule = normalize_item_rule(catalog[index], config, allowed_slots, diagnostics, "input.item_catalog[" .. index .. "]")
      if rule ~= nil then
        item_rules[#item_rules + 1] = rule
        item_rules_by_id[rule.item_id] = rule
      end
    end
  end

  local has_error = false
  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      has_error = true
      break
    end
  end
  return {
    ok = not has_error,
    data = {
      inventory_rules = {
        containers = containers,
        item_rules = item_rules,
        equipment_slots = slot_rules,
        constraints = {
          strict_quest_items = config.strict_quest_items,
          default_capacity_slots = config.default_capacity_slots,
          default_weight_limit = config.default_weight_limit,
          stacking = "per_item_stack_limit",
          capacity = "slot_count_and_optional_weight_limit"
        }
      },
      validation = {
        containers_by_id = containers_by_id,
        item_rules_by_id = item_rules_by_id,
        summary = {
          container_count = #containers,
          item_rule_count = #item_rules,
          equipment_slot_count = #slot_rules
        }
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
