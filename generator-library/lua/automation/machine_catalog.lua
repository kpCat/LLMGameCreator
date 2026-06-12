local M = {}

M.manifest = {
  id = "automation/machine_catalog/v1",
  version = "0.1.0",
  category = "automation",
  title = "Machine catalog",
  purpose = "Generate compact machine catalog IR for recipe categories, rates, modules and automation planning.",
  capabilities = {
    "automation.machine_catalog.generate",
    "machine.category.define",
    "automation.recipe_machine_map"
  },
  input_schema = {
    machines = "array",
    recipe_categories = "array optional"
  },
  output_schema = {
    machine_catalog = "object",
    recipe_category_map = "array"
  },
  config_schema = {
    default_power_demand_kw = "number optional"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "simulation", "codegen_ir" },
  unsafe_features = {}
}

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function copy_list(list)
  local result = {}
  if type(list) ~= "table" then
    return result
  end
  for index = 1, #list do
    result[index] = list[index]
  end
  return result
end

local function normalize_id(value, fallback)
  if type(value) == "string" and value ~= "" then
    return value
  end
  return fallback
end

local function sorted_keys(map)
  local keys = {}
  for key, _ in pairs(map) do
    keys[#keys + 1] = key
  end
  table.sort(keys)
  return keys
end

local function normalize_machine(raw, index, diagnostics, default_power)
  if type(raw) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.machine_catalog.machine_not_table", "Machine entry must be a table.", "machines[" .. index .. "]")
    return nil
  end

  local id = normalize_id(raw.id, "automation/machine/generated_" .. index)
  local categories = copy_list(raw.recipe_categories or raw.categories)
  if #categories == 0 then
    categories[1] = normalize_id(raw.category, "crafting")
  end
  table.sort(categories)

  local speed = tonumber(raw.speed or raw.crafting_speed or raw.rate_multiplier) or 1
  local power = tonumber(raw.power_demand_kw or raw.energy_kw or raw.power_kw) or default_power
  local size = raw.size or {}
  local width = tonumber(size.width or raw.width) or 1
  local height = tonumber(size.height or raw.height) or 1

  if speed <= 0 then
    diagnostics[#diagnostics + 1] = diag("error", "automation.machine_catalog.speed_invalid", "Machine speed must be positive.", id)
    speed = 1
  end
  if power < 0 then
    diagnostics[#diagnostics + 1] = diag("error", "automation.machine_catalog.power_invalid", "Machine power demand cannot be negative.", id)
    power = 0
  end
  if width < 1 or height < 1 then
    diagnostics[#diagnostics + 1] = diag("error", "automation.machine_catalog.size_invalid", "Machine size must be at least 1x1.", id)
    width = 1
    height = 1
  end

  return {
    id = id,
    title = normalize_id(raw.title or raw.name, id),
    recipe_categories = categories,
    speed = speed,
    power_demand_kw = power,
    size = { width = width, height = height },
    module_slots = tonumber(raw.module_slots) or 0,
    tags = copy_list(raw.tags),
    placement = {
      footprint = normalize_id(raw.footprint, "rectangle"),
      requires_resource_node = raw.requires_resource_node == true
    }
  }
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.machine_catalog.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.default_power_demand_kw ~= nil and (type(config.default_power_demand_kw) ~= "number" or config.default_power_demand_kw < 0) then
    diagnostics[#diagnostics + 1] = diag("error", "automation.machine_catalog.default_power_invalid", "default_power_demand_kw must be non-negative.", "config.default_power_demand_kw")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local config = {}
  if type(ctx) == "table" and type(ctx.config) == "table" then
    config = ctx.config
  end

  local config_ok, config_diagnostics = M.validate_config(config)
  for index = 1, #config_diagnostics do
    diagnostics[#diagnostics + 1] = config_diagnostics[index]
  end

  if type(input) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.machine_catalog.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local default_power = tonumber(config.default_power_demand_kw) or 25
  local machines = {}
  local by_id = {}
  for index = 1, #(input.machines or {}) do
    local machine = normalize_machine(input.machines[index], index, diagnostics, default_power)
    if machine ~= nil then
      if by_id[machine.id] then
        diagnostics[#diagnostics + 1] = diag("error", "automation.machine_catalog.duplicate_machine_id", "Duplicate machine id.", machine.id)
      end
      by_id[machine.id] = true
      machines[#machines + 1] = machine
    end
  end

  table.sort(machines, function(left, right)
    return left.id < right.id
  end)

  local recipe_category_map = {}
  local category_to_machines = {}
  for index = 1, #machines do
    local machine = machines[index]
    for category_index = 1, #machine.recipe_categories do
      local category = machine.recipe_categories[category_index]
      category_to_machines[category] = category_to_machines[category] or {}
      category_to_machines[category][#category_to_machines[category] + 1] = machine.id
    end
  end

  for _, category in ipairs(sorted_keys(category_to_machines)) do
    local machine_ids = category_to_machines[category]
    table.sort(machine_ids)
    recipe_category_map[#recipe_category_map + 1] = {
      recipe_category = category,
      machine_ids = machine_ids
    }
  end

  for index = 1, #(input.recipe_categories or {}) do
    local category = input.recipe_categories[index]
    if type(category) == "string" and category_to_machines[category] == nil then
      diagnostics[#diagnostics + 1] = diag("warning", "automation.machine_catalog.category_without_machine", "Recipe category has no matching machine.", category)
    end
  end

  local data = {
    machine_catalog = {
      machines = machines
    },
    recipe_category_map = recipe_category_map,
    validation = {
      machine_count = #machines,
      category_count = #recipe_category_map,
      config_ok = config_ok
    }
  }

  return { ok = #diagnostics == 0, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
