local M = {}

M.manifest = {
  id = "item/loot_table_generator/v1",
  version = "0.1.0",
  category = "item",
  title = "Loot Table Generator",
  purpose = "Build compact deterministic loot table IR from item pools, weights, rarity rules, guaranteed drops and contextual tags.",
  capabilities = { "item.loot_table.generate", "item.loot_pool.validate", "item.drop_rules.describe" },
  input_schema = {
    pools = "array of loot pool definitions",
    item_catalog = "optional array of item definitions used for tag and rarity filtering"
  },
  output_schema = {
    loot_tables = "array of compact loot table IR records",
    indexes = "loot tables by id and item references by pool"
  },
  config_schema = {
    max_tables = "optional positive integer",
    max_entries_per_pool = "optional positive integer",
    default_rolls = "optional positive integer"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
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

local function build_catalog_index(items)
  local by_id = {}
  local by_tag = {}
  local by_rarity = {}
  if type(items) ~= "table" then
    return { by_id = by_id, by_tag = by_tag, by_rarity = by_rarity }
  end
  for index = 1, #items do
    local item = items[index]
    if type(item) == "table" and id_ok(item.id) then
      by_id[item.id] = item
      if type(item.rarity) == "string" then
        if by_rarity[item.rarity] == nil then
          by_rarity[item.rarity] = {}
        end
        by_rarity[item.rarity][#by_rarity[item.rarity] + 1] = item.id
      end
      if type(item.tags) == "table" then
        for tag_index = 1, #item.tags do
          local tag = item.tags[tag_index]
          if token_ok(tag) then
            if by_tag[tag] == nil then
              by_tag[tag] = {}
            end
            by_tag[tag][#by_tag[tag] + 1] = item.id
          end
        end
      end
    end
  end
  return { by_id = by_id, by_tag = by_tag, by_rarity = by_rarity }
end

local function item_matches_filter(item, filter)
  if type(item) ~= "table" or type(filter) ~= "table" then
    return false
  end
  if filter.rarity ~= nil and item.rarity ~= filter.rarity then
    return false
  end
  if filter.type ~= nil and item.type ~= filter.type then
    return false
  end
  if type(filter.tags_any) == "table" and #filter.tags_any > 0 then
    local item_tags = {}
    if type(item.tags) == "table" then
      for index = 1, #item.tags do
        item_tags[item.tags[index]] = true
      end
    end
    local matched = false
    for index = 1, #filter.tags_any do
      if item_tags[filter.tags_any[index]] then
        matched = true
        break
      end
    end
    if not matched then
      return false
    end
  end
  return true
end

local function expand_filter_entries(filter, catalog, diagnostics, target)
  local result = {}
  if type(filter) ~= "table" then
    add(diagnostics, "error", "loot.filter_not_table", "Filter entry must be a table.", target)
    return result
  end
  for item_id, item in pairs(catalog.by_id) do
    if item_matches_filter(item, filter) then
      result[#result + 1] = {
        item_id = item_id,
        weight = non_negative_number(filter.weight) and filter.weight or 1,
        quantity = type(filter.quantity) == "table" and filter.quantity or { min = 1, max = 1 },
        source = "filter"
      }
    end
  end
  return result
end

local function normalize_quantity(value, diagnostics, target)
  if value == nil then
    return { min = 1, max = 1 }
  end
  if type(value) == "number" then
    if positive_integer(value) then
      return { min = value, max = value }
    end
    add(diagnostics, "error", "loot.invalid_quantity", "Numeric quantity must be a positive integer.", target)
    return { min = 1, max = 1 }
  end
  if type(value) == "table" then
    local min_value = value.min or 1
    local max_value = value.max or min_value
    if not positive_integer(min_value) or not positive_integer(max_value) or max_value < min_value then
      add(diagnostics, "error", "loot.invalid_quantity_range", "Quantity range must use positive integers and max >= min.", target)
      return { min = 1, max = 1 }
    end
    return { min = min_value, max = max_value }
  end
  add(diagnostics, "error", "loot.quantity_not_supported", "Quantity must be a number or range table.", target)
  return { min = 1, max = 1 }
end

local function normalize_entry(entry, diagnostics, target)
  if type(entry) ~= "table" then
    add(diagnostics, "error", "loot.entry_not_table", "Loot entry must be a table.", target)
    return nil
  end
  if not id_ok(entry.item_id or entry.id) then
    add(diagnostics, "error", "loot.invalid_item_id", "Loot entry item_id must be a lowercase slash id.", target .. ".item_id")
    return nil
  end
  local weight = entry.weight or 1
  if not non_negative_number(weight) then
    add(diagnostics, "error", "loot.invalid_weight", "Loot weight must be a non-negative number.", target .. ".weight")
    weight = 0
  end
  return {
    item_id = entry.item_id or entry.id,
    weight = weight,
    quantity = normalize_quantity(entry.quantity, diagnostics, target .. ".quantity"),
    conditions = clone_array(entry.conditions),
    source = entry.source or "direct"
  }
end

local function normalize_guaranteed(value, diagnostics, target)
  local result = {}
  if value == nil then
    return result
  end
  if not is_array(value) then
    add(diagnostics, "error", "loot.guaranteed_not_array", "Guaranteed drops must be an array.", target)
    return result
  end
  for index = 1, #value do
    local entry = normalize_entry(value[index], diagnostics, target .. "[" .. index .. "]")
    if entry ~= nil then
      entry.weight = nil
      result[#result + 1] = entry
    end
  end
  return result
end

local function normalize_pool(pool, catalog, config, diagnostics, target)
  if type(pool) ~= "table" then
    add(diagnostics, "error", "loot.pool_not_table", "Pool must be a table.", target)
    return nil
  end
  local id = pool.id
  if not id_ok(id) then
    add(diagnostics, "error", "loot.invalid_pool_id", "Pool id must be a lowercase slash id.", target .. ".id")
    return nil
  end
  local rolls = pool.rolls or config.default_rolls
  if not positive_integer(rolls) then
    add(diagnostics, "warning", "loot.invalid_rolls", "Pool rolls replaced by default_rolls.", target .. ".rolls")
    rolls = config.default_rolls
  end
  local entries = {}
  if type(pool.entries) == "table" then
    for index = 1, #pool.entries do
      local entry = normalize_entry(pool.entries[index], diagnostics, target .. ".entries[" .. index .. "]")
      if entry ~= nil and #entries < config.max_entries_per_pool then
        entries[#entries + 1] = entry
      end
    end
  end
  if type(pool.filters) == "table" then
    for index = 1, #pool.filters do
      local expanded = expand_filter_entries(pool.filters[index], catalog, diagnostics, target .. ".filters[" .. index .. "]")
      for expanded_index = 1, #expanded do
        if #entries < config.max_entries_per_pool then
          entries[#entries + 1] = expanded[expanded_index]
        end
      end
    end
  end
  if #entries >= config.max_entries_per_pool then
    add(diagnostics, "warning", "loot.entries_capped", "Pool entries were capped by max_entries_per_pool.", target)
  end
  local total_weight = 0
  for index = 1, #entries do
    total_weight = total_weight + entries[index].weight
  end
  if total_weight <= 0 and #entries > 0 then
    add(diagnostics, "error", "loot.zero_total_weight", "Loot pool has entries but total weight is zero.", target)
  end
  return {
    id = id,
    rolls = rolls,
    tags = normalize_tags(pool.tags),
    context = type(pool.context) == "table" and pool.context or {},
    guaranteed = normalize_guaranteed(pool.guaranteed, diagnostics, target .. ".guaranteed"),
    entries = entries,
    total_weight = total_weight,
    empty_behavior = pool.empty_behavior or "drop_nothing"
  }
end

function M.validate_config(config)
  local diagnostics = {}
  config = config or {}
  if type(config) ~= "table" then
    add(diagnostics, "error", "loot.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.max_tables ~= nil and not positive_integer(config.max_tables) then
    add(diagnostics, "error", "loot.invalid_max_tables", "max_tables must be a positive integer.", "config.max_tables")
  end
  if config.max_entries_per_pool ~= nil and not positive_integer(config.max_entries_per_pool) then
    add(diagnostics, "error", "loot.invalid_max_entries", "max_entries_per_pool must be a positive integer.", "config.max_entries_per_pool")
  end
  if config.default_rolls ~= nil and not positive_integer(config.default_rolls) then
    add(diagnostics, "error", "loot.invalid_default_rolls", "default_rolls must be a positive integer.", "config.default_rolls")
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
    max_tables = config.max_tables or 64,
    max_entries_per_pool = config.max_entries_per_pool or 64,
    default_rolls = config.default_rolls or 1
  }

  local pools = input.pools or input.loot_tables or {}
  if not is_array(pools) then
    add(diagnostics, "error", "loot.pools_not_array", "pools must be an array.", "input.pools")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  local catalog = build_catalog_index(input.item_catalog or input.items)
  local catalog_item_count = catalog_item_count
  for _, _ in pairs(catalog.by_id) do
    catalog_item_count = catalog_item_count + 1
  end
  local loot_tables = {}
  local by_id = {}
  local pool_items = {}
  for index = 1, #pools do
    if #loot_tables >= config.max_tables then
      add(diagnostics, "warning", "loot.max_tables_reached", "Loot table generation stopped at max_tables.", "config.max_tables")
      break
    end
    local pool = normalize_pool(pools[index], catalog, config, diagnostics, "input.pools[" .. index .. "]")
    if pool ~= nil then
      if by_id[pool.id] ~= nil then
        add(diagnostics, "error", "loot.duplicate_pool_id", "Duplicate loot pool id ignored.", pool.id)
      else
        loot_tables[#loot_tables + 1] = pool
        by_id[pool.id] = pool
        local refs = {}
        for entry_index = 1, #pool.entries do
          refs[#refs + 1] = pool.entries[entry_index].item_id
        end
        pool_items[pool.id] = refs
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
      loot_tables = loot_tables,
      indexes = {
        by_id = by_id,
        item_refs_by_pool = pool_items
      },
      summary = {
        table_count = #loot_tables,
        catalog_item_count = catalog_item_count
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
