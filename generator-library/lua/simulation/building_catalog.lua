local M = {}

M.manifest = {
  id = "simulation/building_catalog/v1",
  version = "0.1.0",
  category = "simulation",
  title = "Building Catalog",
  purpose = "Generate compact city-builder building catalog IR with zones, hooks and build metadata.",
  capabilities = { "city_builder.buildings.generate", "city_builder.zones.configure", "city_builder.economy_hooks" },
  input_schema = { kind = "city_builder.buildings.input" },
  output_schema = { kind = "city_builder.buildings.ir" },
  config_schema = { kind = "city_builder.buildings.config" },
  deterministic = true,
  runtime_targets = { "editor", "simulation", "unity2d", "unity3d", "codegen_ir" },
  unsafe_features = {}
}

local function diag(list, severity, code, message, target)
  list[#list + 1] = { severity = severity, code = code, message = message, target = target }
end

local function is_array(value)
  if type(value) ~= "table" then return false end
  local count = 0
  for k, _ in pairs(value) do
    if type(k) ~= "number" or k < 1 or k % 1 ~= 0 then return false end
    if k > count then count = k end
  end
  for i = 1, count do if value[i] == nil then return false end end
  return true
end

local function valid_id(value)
  return type(value) == "string" and value:match("^[a-z][a-z0-9_]*(/[a-z][a-z0-9_]*)*$") ~= nil
end

local function copy_array(value)
  local result = {}
  if type(value) == "table" then for i = 1, #value do result[i] = value[i] end end
  return result
end

local categories = { housing = true, production = true, service = true, storage = true, utility = true }

local function default_buildings()
  return {
    {
      id = "building/house",
      category = "housing",
      footprint = { width = 2, height = 2 },
      build_costs = { wood = 12, stone = 4 },
      zone_tags = { "residential" },
      hooks = { housing_capacity = 4, need_sources = { "need/rest" } }
    },
    {
      id = "building/farm",
      category = "production",
      footprint = { width = 3, height = 3 },
      build_costs = { wood = 10, tools = 2 },
      zone_tags = { "food", "rural" },
      hooks = { job_ids = { "job/farmer" }, output_items = { "item/food" } }
    },
    {
      id = "building/clinic",
      category = "service",
      footprint = { width = 2, height = 2 },
      build_costs = { wood = 8, stone = 8, tools = 1 },
      zone_tags = { "health", "service" },
      hooks = { service_ids = { "service/clinic" }, job_ids = { "job/healer" } }
    }
  }
end

local function validate_costs(value, target, diagnostics)
  if value == nil then return end
  if type(value) ~= "table" then
    diag(diagnostics, "error", "buildings.invalid_costs", "Build costs must be a table of resource amounts.", target)
    return
  end
  for key, amount in pairs(value) do
    if type(key) ~= "string" or key == "" then
      diag(diagnostics, "error", "buildings.invalid_cost_key", "Build cost key must be a non-empty string.", target)
    end
    if type(amount) ~= "number" or amount < 0 then
      diag(diagnostics, "error", "buildings.invalid_cost_amount", "Build cost amount must be non-negative.", target .. "." .. key)
    end
  end
end

local function validate_building(building, index, diagnostics)
  local target = "buildings[" .. index .. "]"
  if type(building) ~= "table" then
    diag(diagnostics, "error", "buildings.entry_not_table", "Building entry must be a table.", target)
    return
  end
  if not valid_id(building.id) then
    diag(diagnostics, "error", "buildings.invalid_id", "Building id must use lowercase slash notation.", target .. ".id")
  end
  if not categories[building.category] then
    diag(diagnostics, "error", "buildings.invalid_category", "Building category must be housing, production, service, storage, or utility.", target .. ".category")
  end
  if type(building.footprint) ~= "table" then
    diag(diagnostics, "error", "buildings.missing_footprint", "Building footprint is required.", target .. ".footprint")
  else
    if type(building.footprint.width) ~= "number" or building.footprint.width < 1 or building.footprint.width % 1 ~= 0 then
      diag(diagnostics, "error", "buildings.invalid_width", "Footprint width must be a positive integer.", target .. ".footprint.width")
    end
    if type(building.footprint.height) ~= "number" or building.footprint.height < 1 or building.footprint.height % 1 ~= 0 then
      diag(diagnostics, "error", "buildings.invalid_height", "Footprint height must be a positive integer.", target .. ".footprint.height")
    end
  end
  validate_costs(building.build_costs, target .. ".build_costs", diagnostics)
  if building.zone_tags ~= nil and not is_array(building.zone_tags) then
    diag(diagnostics, "error", "buildings.invalid_zone_tags", "Zone tags must be an array.", target .. ".zone_tags")
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then return true, diagnostics end
  if type(config) ~= "table" then
    diag(diagnostics, "error", "buildings.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.buildings ~= nil then
    if not is_array(config.buildings) then
      diag(diagnostics, "error", "buildings.not_array", "Buildings must be an array.", "buildings")
    else
      local seen = {}
      for i = 1, #config.buildings do
        local building = config.buildings[i]
        validate_building(building, i, diagnostics)
        if type(building) == "table" and type(building.id) == "string" then
          if seen[building.id] then diag(diagnostics, "error", "buildings.duplicate_id", "Duplicate building id.", "buildings[" .. i .. "].id") end
          seen[building.id] = true
        end
      end
    end
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local config = {}
  if type(input) == "table" and type(input.config) == "table" then config = input.config elseif type(input) == "table" then config = input end
  local ok, diagnostics = M.validate_config(config)
  if not ok then return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} } end
  local source = config.buildings or default_buildings()
  local buildings = {}
  for i = 1, #source do
    local building = source[i]
    buildings[i] = {
      id = building.id,
      category = building.category,
      footprint = { width = building.footprint.width, height = building.footprint.height },
      build_costs = building.build_costs or {},
      zone_tags = copy_array(building.zone_tags),
      hooks = building.hooks or {}
    }
  end
  local data = {
    schema = "city_builder.building_catalog.v1",
    catalog_id = config.catalog_id or "building_catalog/default",
    buildings = buildings,
    zone_model = config.zone_model or { supported_tags = { "residential", "food", "service", "utility", "storage" } },
    metadata = { deterministic = true, generated_by = M.manifest.id }
  }
  return { ok = true, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
