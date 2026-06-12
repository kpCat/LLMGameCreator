local M = {}

M.manifest = {
  id = "item/item_schema/v1",
  version = "0.1.0",
  category = "item",
  title = "Item Schema",
  purpose = "Normalize and validate compact item IR for stackable items, quest items, equipment, durability, rarity, tags and description configs.",
  capabilities = { "item.schema.normalize", "item.validate", "inventory.item_contract" },
  input_schema = {
    items = "array of item definitions or a single item definition"
  },
  output_schema = {
    items = "normalized item definitions",
    indexes = "lookup maps by id, tag, rarity and equipment slot",
    summary = "counts and schema warnings"
  },
  config_schema = {
    allowed_rarities = "optional array of rarity ids",
    allowed_item_types = "optional array of item type ids",
    allowed_equipment_slots = "optional array of equipment slot ids",
    default_stack_limit = "optional positive integer",
    max_tags_per_item = "optional positive integer"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "unity_ui_ir" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local DEFAULT_RARITIES = { "common", "uncommon", "rare", "epic", "legendary", "quest" }
local DEFAULT_ITEM_TYPES = { "material", "consumable", "quest", "equipment", "tool", "key", "currency", "note" }
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

local function positive_integer(value)
  return type(value) == "number" and value > 0 and value % 1 == 0
end

local function non_negative_integer(value)
  return type(value) == "number" and value >= 0 and value % 1 == 0
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

local function list_to_set(list)
  local result = {}
  if type(list) == "table" then
    for index = 1, #list do
      if type(list[index]) == "string" then
        result[list[index]] = true
      end
    end
  end
  return result
end

local function normalize_tags(tags, max_tags, diagnostics, target)
  local result = {}
  local seen = {}
  if tags == nil then
    return result
  end
  if type(tags) ~= "table" then
    add(diagnostics, "error", "item.tags_not_array", "Item tags must be an array of token strings.", target .. ".tags")
    return result
  end
  for index = 1, #tags do
    local tag = tags[index]
    if not token_ok(tag) then
      add(diagnostics, "warning", "item.invalid_tag", "Invalid tag ignored.", target .. ".tags[" .. index .. "]")
    elseif not seen[tag] then
      if #result < max_tags then
        seen[tag] = true
        result[#result + 1] = tag
      else
        add(diagnostics, "warning", "item.too_many_tags", "Tag ignored because max_tags_per_item was reached.", target .. ".tags[" .. index .. "]")
      end
    end
  end
  return result
end

local function normalize_description_config(value, item_name)
  local src = {}
  if type(value) == "table" then
    src = value
  end
  return {
    display_name = src.display_name or item_name or "Unnamed item",
    short = src.short or src.summary or "",
    long = src.long or src.description or "",
    style_tags = normalize_tags(src.style_tags or {}, 12, {}, "description.style_tags"),
    hidden_until_inspected = src.hidden_until_inspected == true,
    reveal_flags = clone_array(src.reveal_flags)
  }
end

local function normalize_durability(value, item_type, diagnostics, target)
  if value == nil then
    if item_type == "equipment" or item_type == "tool" then
      return { enabled = false, max = 0, current = 0, break_behavior = "none" }
    end
    return { enabled = false, max = 0, current = 0, break_behavior = "none" }
  end
  if type(value) ~= "table" then
    add(diagnostics, "error", "item.durability_not_table", "Durability must be a table when provided.", target)
    return { enabled = false, max = 0, current = 0, break_behavior = "none" }
  end
  local max_value = value.max or value.maximum or 0
  local current = value.current or max_value
  if max_value ~= 0 and not positive_integer(max_value) then
    add(diagnostics, "error", "item.invalid_durability_max", "Durability max must be a positive integer or omitted.", target .. ".max")
    max_value = 0
    current = 0
  end
  if max_value > 0 and (not non_negative_integer(current) or current > max_value) then
    add(diagnostics, "warning", "item.invalid_durability_current", "Durability current was clamped to durability max.", target .. ".current")
    current = max_value
  end
  return {
    enabled = max_value > 0,
    max = max_value,
    current = current,
    break_behavior = value.break_behavior or "disable_effects"
  }
end

local function normalize_equipment(value, item_type, allowed_slots, diagnostics, target)
  if item_type ~= "equipment" then
    if value ~= nil then
      add(diagnostics, "warning", "item.equipment_ignored", "Equipment block ignored for non-equipment item.", target)
    end
    return nil
  end
  if type(value) ~= "table" then
    add(diagnostics, "error", "item.equipment_missing", "Equipment item must define equipment table.", target)
    return { slot = "utility", modifiers = {}, requirements = {}, equip_effects = {} }
  end
  local slot = value.slot or "utility"
  if not allowed_slots[slot] then
    add(diagnostics, "error", "item.invalid_equipment_slot", "Equipment slot is not allowed by config.", target .. ".slot")
    slot = "utility"
  end
  return {
    slot = slot,
    two_handed = value.two_handed == true or slot == "two_hand",
    modifiers = type(value.modifiers) == "table" and value.modifiers or {},
    requirements = type(value.requirements) == "table" and value.requirements or {},
    equip_effects = clone_array(value.equip_effects or value.effects)
  }
end

local function normalize_stack(value, item_type, default_stack_limit, diagnostics, target)
  local stackable = value.stackable
  if stackable == nil then
    stackable = item_type == "material" or item_type == "consumable" or item_type == "currency"
  end
  local limit = value.stack_limit or value.max_stack or default_stack_limit
  if stackable == false then
    limit = 1
  elseif not positive_integer(limit) then
    add(diagnostics, "warning", "item.invalid_stack_limit", "Stack limit was replaced by default stack limit.", target)
    limit = default_stack_limit
  end
  return { stackable = stackable == true, stack_limit = limit }
end

local function normalize_item(item, config, allowed, diagnostics, target)
  if type(item) ~= "table" then
    add(diagnostics, "error", "item.not_table", "Item definition must be a table.", target)
    return nil
  end
  local id = item.id
  if not id_ok(id) then
    add(diagnostics, "error", "item.invalid_id", "Item id must be a lowercase slash id.", target .. ".id")
    return nil
  end
  local item_type = item.type or item.item_type or "material"
  if not allowed.item_types[item_type] then
    add(diagnostics, "error", "item.invalid_type", "Item type is not allowed by config.", target .. ".type")
    item_type = "material"
  end
  local rarity = item.rarity or (item_type == "quest" and "quest" or "common")
  if not allowed.rarities[rarity] then
    add(diagnostics, "error", "item.invalid_rarity", "Rarity is not allowed by config.", target .. ".rarity")
    rarity = "common"
  end
  local stack = normalize_stack(item, item_type, config.default_stack_limit, diagnostics, target .. ".stack")
  local quest_item = item.quest_item == true or item_type == "quest" or rarity == "quest"
  if quest_item and stack.stackable and stack.stack_limit > 1 then
    add(diagnostics, "info", "item.quest_stack_allowed", "Quest item is stackable; runtime should confirm whether this is intended.", target)
  end
  local normalized = {
    id = id,
    type = item_type,
    name = item.name or item.title or id,
    rarity = rarity,
    tags = normalize_tags(item.tags or {}, config.max_tags_per_item, diagnostics, target),
    stack = stack,
    quest_item = quest_item,
    equipment = normalize_equipment(item.equipment, item_type, allowed.equipment_slots, diagnostics, target .. ".equipment"),
    durability = normalize_durability(item.durability, item_type, diagnostics, target .. ".durability"),
    value = type(item.value) == "number" and item.value or 0,
    weight = type(item.weight) == "number" and item.weight or 0,
    description_config = normalize_description_config(item.description_config or item.description, item.name or item.title or id),
    hooks = {
      on_use = item.on_use,
      on_pickup = item.on_pickup,
      on_equip = item.on_equip
    }
  }
  return normalized
end

local function add_index_array(index, key, value)
  if key == nil then
    return
  end
  if index[key] == nil then
    index[key] = {}
  end
  index[key][#index[key] + 1] = value
end

function M.validate_config(config)
  local diagnostics = {}
  config = config or {}
  if type(config) ~= "table" then
    add(diagnostics, "error", "item.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.allowed_rarities ~= nil and not is_array(config.allowed_rarities) then
    add(diagnostics, "error", "item.allowed_rarities_not_array", "allowed_rarities must be an array.", "config.allowed_rarities")
  end
  if config.allowed_item_types ~= nil and not is_array(config.allowed_item_types) then
    add(diagnostics, "error", "item.allowed_item_types_not_array", "allowed_item_types must be an array.", "config.allowed_item_types")
  end
  if config.allowed_equipment_slots ~= nil and not is_array(config.allowed_equipment_slots) then
    add(diagnostics, "error", "item.allowed_slots_not_array", "allowed_equipment_slots must be an array.", "config.allowed_equipment_slots")
  end
  if config.default_stack_limit ~= nil and not positive_integer(config.default_stack_limit) then
    add(diagnostics, "error", "item.invalid_default_stack_limit", "default_stack_limit must be a positive integer.", "config.default_stack_limit")
  end
  if config.max_tags_per_item ~= nil and not positive_integer(config.max_tags_per_item) then
    add(diagnostics, "error", "item.invalid_max_tags", "max_tags_per_item must be a positive integer.", "config.max_tags_per_item")
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
    allowed_rarities = config.allowed_rarities or DEFAULT_RARITIES,
    allowed_item_types = config.allowed_item_types or DEFAULT_ITEM_TYPES,
    allowed_equipment_slots = config.allowed_equipment_slots or DEFAULT_SLOTS,
    default_stack_limit = config.default_stack_limit or 99,
    max_tags_per_item = config.max_tags_per_item or 16
  }
  local allowed = {
    rarities = list_to_set(config.allowed_rarities),
    item_types = list_to_set(config.allowed_item_types),
    equipment_slots = list_to_set(config.allowed_equipment_slots)
  }

  local source = input.items or input.item or {}
  if source.id ~= nil then
    source = { source }
  end
  if not is_array(source) then
    add(diagnostics, "error", "item.items_not_array", "Input must provide item or items array.", "input.items")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local items = {}
  local by_id = {}
  local by_tag = {}
  local by_rarity = {}
  local by_slot = {}
  for index = 1, #source do
    local item = normalize_item(source[index], config, allowed, diagnostics, "input.items[" .. index .. "]")
    if item ~= nil then
      if by_id[item.id] ~= nil then
        add(diagnostics, "error", "item.duplicate_id", "Duplicate item id ignored.", item.id)
      else
        items[#items + 1] = item
        by_id[item.id] = item
        add_index_array(by_rarity, item.rarity, item.id)
        if item.equipment ~= nil then
          add_index_array(by_slot, item.equipment.slot, item.id)
        end
        for tag_index = 1, #item.tags do
          add_index_array(by_tag, item.tags[tag_index], item.id)
        end
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
      items = items,
      indexes = {
        by_id = by_id,
        by_tag = by_tag,
        by_rarity = by_rarity,
        by_equipment_slot = by_slot
      },
      summary = {
        item_count = #items,
        rarity_count = #config.allowed_rarities,
        item_type_count = #config.allowed_item_types
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
