local M = {}

M.manifest = {
  id = "item/item_catalog_generator/v1",
  version = "0.1.0",
  category = "item",
  title = "Item Catalog Generator",
  purpose = "Generate a compact item catalog from item families, rarity tiers, tags and deterministic naming/description configs.",
  capabilities = { "item.catalog.generate", "item.description_config.generate", "item.prototype.expand" },
  input_schema = {
    families = "array of item family definitions",
    themes = "optional array of theme tags"
  },
  output_schema = {
    items = "generated compact item definitions compatible with item_schema",
    catalog = "family and rarity summary metadata"
  },
  config_schema = {
    max_items = "optional positive integer",
    default_rarities = "optional array of rarity tiers",
    default_stack_limit = "optional positive integer"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "unity_ui_ir" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local DEFAULT_RARITIES = {
  { id = "common", value_multiplier = 1, durability_bonus = 0 },
  { id = "uncommon", value_multiplier = 2, durability_bonus = 5 },
  { id = "rare", value_multiplier = 4, durability_bonus = 12 }
}

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

local function token_ok(value)
  return type(value) == "string" and value ~= "" and value:match("^[a-z0-9_%-]+$") ~= nil
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

local function clone_array(value)
  local result = {}
  if type(value) == "table" then
    for index = 1, #value do
      result[#result + 1] = value[index]
    end
  end
  return result
end

local function normalize_tags(value)
  local out = {}
  local seen = {}
  if type(value) == "table" then
    for index = 1, #value do
      local tag = value[index]
      if token_ok(tag) and not seen[tag] then
        seen[tag] = true
        out[#out + 1] = tag
      end
    end
  end
  return out
end

local function normalize_rarities(config)
  local result = {}
  local source = config.default_rarities or DEFAULT_RARITIES
  if type(source) ~= "table" then
    source = DEFAULT_RARITIES
  end
  for index = 1, #source do
    local item = source[index]
    if type(item) == "string" then
      result[#result + 1] = { id = item, value_multiplier = index, durability_bonus = index - 1 }
    elseif type(item) == "table" and token_ok(item.id) then
      result[#result + 1] = {
        id = item.id,
        value_multiplier = type(item.value_multiplier) == "number" and item.value_multiplier or index,
        durability_bonus = type(item.durability_bonus) == "number" and item.durability_bonus or 0,
        tags = normalize_tags(item.tags)
      }
    end
  end
  if #result == 0 then
    result = DEFAULT_RARITIES
  end
  return result
end

local function make_slug(text)
  if type(text) ~= "string" or text == "" then
    return "item"
  end
  local lower = string.lower(text)
  local out = lower:gsub("[^a-z0-9_]+", "_"):gsub("_+", "_")
  out = out:gsub("^_", ""):gsub("_$", "")
  if out == "" then
    return "item"
  end
  return out
end

local function make_item_id(namespace, family_slug, variant_slug, rarity)
  local base = namespace .. "/" .. family_slug
  if variant_slug ~= "" then
    base = base .. "/" .. variant_slug
  end
  if rarity ~= "common" then
    base = base .. "_" .. rarity
  end
  return base
end

local function normalize_family(family, diagnostics, target)
  if type(family) ~= "table" then
    add(diagnostics, "error", "catalog.family_not_table", "Family must be a table.", target)
    return nil
  end
  local namespace = family.namespace or "item/generated"
  if not id_ok(namespace) then
    add(diagnostics, "error", "catalog.invalid_namespace", "Family namespace must be a lowercase slash id.", target .. ".namespace")
    namespace = "item/generated"
  end
  local family_slug = family.slug or make_slug(family.name or family.id or "family")
  if not token_ok(family_slug) then
    add(diagnostics, "error", "catalog.invalid_family_slug", "Family slug must be a token.", target .. ".slug")
    family_slug = "family"
  end
  local variants = family.variants or { { id = "base", name = family.name or family_slug } }
  if type(variants) ~= "table" or #variants == 0 then
    add(diagnostics, "warning", "catalog.empty_variants", "Family has no variants; base variant was inserted.", target .. ".variants")
    variants = { { id = "base", name = family.name or family_slug } }
  end
  return {
    namespace = namespace,
    slug = family_slug,
    name = family.name or family_slug,
    item_type = family.type or family.item_type or "material",
    base_value = type(family.base_value) == "number" and family.base_value or 0,
    base_weight = type(family.base_weight) == "number" and family.base_weight or 0,
    stackable = family.stackable,
    stack_limit = family.stack_limit,
    tags = normalize_tags(family.tags),
    equipment = type(family.equipment) == "table" and family.equipment or nil,
    durability = type(family.durability) == "table" and family.durability or nil,
    description_style_tags = normalize_tags(family.description_style_tags),
    variants = variants,
    rarities = family.rarities
  }
end

local function variant_record(value)
  if type(value) == "string" then
    return { id = make_slug(value), name = value, tags = {} }
  end
  if type(value) == "table" then
    return {
      id = make_slug(value.id or value.slug or value.name or "variant"),
      name = value.name or value.title or value.id or "Variant",
      tags = normalize_tags(value.tags),
      value_bonus = type(value.value_bonus) == "number" and value.value_bonus or 0,
      durability_bonus = type(value.durability_bonus) == "number" and value.durability_bonus or 0,
      equipment = type(value.equipment) == "table" and value.equipment or nil,
      description = value.description
    }
  end
  return { id = "variant", name = "Variant", tags = {} }
end

local function merge_tags(a, b, c)
  local out = {}
  local seen = {}
  local lists = { a, b, c }
  for list_index = 1, #lists do
    local list = lists[list_index]
    if type(list) == "table" then
      for index = 1, #list do
        local tag = list[index]
        if token_ok(tag) and not seen[tag] then
          seen[tag] = true
          out[#out + 1] = tag
        end
      end
    end
  end
  return out
end

local function allowed_family_rarity(family, rarity_id)
  if type(family.rarities) ~= "table" or #family.rarities == 0 then
    return true
  end
  for index = 1, #family.rarities do
    if family.rarities[index] == rarity_id then
      return true
    end
  end
  return false
end

local function build_description(family, variant, rarity)
  local rarity_prefix = rarity.id == "common" and "" or (rarity.id .. " ")
  local name = rarity_prefix .. variant.name
  return {
    display_name = name,
    short = "Compact generated item: " .. name .. ".",
    long = variant.description or "Generated catalog item from family " .. family.name .. ".",
    style_tags = merge_tags(family.description_style_tags, variant.tags, rarity.tags),
    hidden_until_inspected = false,
    reveal_flags = {}
  }
end

local function build_item(family, variant, rarity, default_stack_limit)
  local item_id = make_item_id(family.namespace, family.slug, variant.id ~= "base" and variant.id or "", rarity.id)
  local durability = nil
  if family.durability ~= nil then
    local max_value = family.durability.max or 0
    durability = {
      max = max_value + rarity.durability_bonus + (variant.durability_bonus or 0),
      current = max_value + rarity.durability_bonus + (variant.durability_bonus or 0),
      break_behavior = family.durability.break_behavior or "disable_effects"
    }
  end
  local equipment = variant.equipment or family.equipment
  return {
    id = item_id,
    type = family.item_type,
    name = (rarity.id == "common" and variant.name or (rarity.id .. " " .. variant.name)),
    rarity = rarity.id,
    tags = merge_tags(family.tags, variant.tags, rarity.tags),
    stackable = family.stackable,
    stack_limit = family.stack_limit or default_stack_limit,
    value = family.base_value * rarity.value_multiplier + (variant.value_bonus or 0),
    weight = family.base_weight,
    equipment = equipment,
    durability = durability,
    description_config = build_description(family, variant, rarity)
  }
end

function M.validate_config(config)
  local diagnostics = {}
  config = config or {}
  if type(config) ~= "table" then
    add(diagnostics, "error", "catalog.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.max_items ~= nil and not positive_integer(config.max_items) then
    add(diagnostics, "error", "catalog.invalid_max_items", "max_items must be a positive integer.", "config.max_items")
  end
  if config.default_rarities ~= nil and not is_array(config.default_rarities) then
    add(diagnostics, "error", "catalog.rarities_not_array", "default_rarities must be an array.", "config.default_rarities")
  end
  if config.default_stack_limit ~= nil and not positive_integer(config.default_stack_limit) then
    add(diagnostics, "error", "catalog.invalid_stack_limit", "default_stack_limit must be a positive integer.", "config.default_stack_limit")
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

  local families = input.families or {}
  if not is_array(families) then
    add(diagnostics, "error", "catalog.families_not_array", "families must be an array.", "input.families")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  local rarities = normalize_rarities(config)
  local max_items = config.max_items or 128
  local default_stack_limit = config.default_stack_limit or 99
  local items = {}
  local family_summary = {}
  for family_index = 1, #families do
    local family = normalize_family(families[family_index], diagnostics, "input.families[" .. family_index .. "]")
    if family ~= nil then
      local before = #items
      for variant_index = 1, #family.variants do
        local variant = variant_record(family.variants[variant_index])
        for rarity_index = 1, #rarities do
          local rarity = rarities[rarity_index]
          if allowed_family_rarity(family, rarity.id) and #items < max_items then
            items[#items + 1] = build_item(family, variant, rarity, default_stack_limit)
          end
        end
      end
      family_summary[#family_summary + 1] = { family = family.slug, generated_count = #items - before }
    end
  end
  if #items >= max_items then
    add(diagnostics, "warning", "catalog.max_items_reached", "Catalog generation stopped at max_items.", "config.max_items")
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
      catalog = {
        generated_count = #items,
        family_summary = family_summary,
        rarity_tiers = rarities,
        themes = clone_array(input.themes)
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
