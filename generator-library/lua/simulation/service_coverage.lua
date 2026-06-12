local M = {}

M.manifest = {
  id = "simulation/service_coverage/v1",
  version = "0.1.0",
  category = "simulation",
  title = "Service Coverage",
  purpose = "Generate deterministic service coverage config IR for city-builder systems without solving paths.",
  capabilities = { "city_builder.service_coverage.generate", "city_builder.services.validate", "simulation.coverage_metadata" },
  input_schema = { kind = "city_builder.service_coverage.input" },
  output_schema = { kind = "city_builder.service_coverage.ir" },
  config_schema = { kind = "city_builder.service_coverage.config" },
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

local function default_services()
  return {
    {
      id = "service/market",
      provider_building_categories = { "service" },
      provider_building_ids = {},
      radius = 12,
      capacity = 80,
      coverage_target_tags = { "residential" },
      quality = 0.75,
      priority = 70,
      need_ids = { "need/food" }
    },
    {
      id = "service/clinic",
      provider_building_categories = { "service" },
      provider_building_ids = { "building/clinic" },
      radius = 10,
      capacity = 40,
      coverage_target_tags = { "residential", "workplace" },
      quality = 0.85,
      priority = 90,
      need_ids = { "need/safety" }
    }
  }
end

local function validate_service(service, index, diagnostics)
  local target = "services[" .. index .. "]"
  if type(service) ~= "table" then
    diag(diagnostics, "error", "services.entry_not_table", "Service entry must be a table.", target)
    return
  end
  if not valid_id(service.id) then
    diag(diagnostics, "error", "services.invalid_id", "Service id must use lowercase slash notation.", target .. ".id")
  end
  if service.provider_building_ids ~= nil then
    if not is_array(service.provider_building_ids) then
      diag(diagnostics, "error", "services.invalid_provider_ids", "Provider building ids must be an array.", target .. ".provider_building_ids")
    else
      for i = 1, #service.provider_building_ids do
        if not valid_id(service.provider_building_ids[i]) then
          diag(diagnostics, "error", "services.invalid_provider_id", "Provider building id must use lowercase slash notation.", target .. ".provider_building_ids[" .. i .. "]")
        end
      end
    end
  end
  if service.provider_building_categories ~= nil and not is_array(service.provider_building_categories) then
    diag(diagnostics, "error", "services.invalid_provider_categories", "Provider building categories must be an array.", target .. ".provider_building_categories")
  end
  if service.provider_building_ids == nil and service.provider_building_categories == nil then
    diag(diagnostics, "error", "services.missing_provider", "Service must declare provider building ids or categories.", target)
  end
  if type(service.radius) ~= "number" or service.radius < 0 then
    diag(diagnostics, "error", "services.invalid_radius", "Service radius must be non-negative.", target .. ".radius")
  end
  if type(service.capacity) ~= "number" or service.capacity < 0 then
    diag(diagnostics, "error", "services.invalid_capacity", "Service capacity must be non-negative.", target .. ".capacity")
  end
  if service.coverage_target_tags ~= nil and not is_array(service.coverage_target_tags) then
    diag(diagnostics, "error", "services.invalid_target_tags", "Coverage target tags must be an array.", target .. ".coverage_target_tags")
  end
  if service.quality ~= nil and (type(service.quality) ~= "number" or service.quality < 0 or service.quality > 1) then
    diag(diagnostics, "error", "services.invalid_quality", "Service quality must be in range 0..1.", target .. ".quality")
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then return true, diagnostics end
  if type(config) ~= "table" then
    diag(diagnostics, "error", "services.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.services ~= nil then
    if not is_array(config.services) then
      diag(diagnostics, "error", "services.not_array", "Services must be an array.", "services")
    else
      local seen = {}
      for i = 1, #config.services do
        local service = config.services[i]
        validate_service(service, i, diagnostics)
        if type(service) == "table" and type(service.id) == "string" then
          if seen[service.id] then diag(diagnostics, "error", "services.duplicate_id", "Duplicate service id.", "services[" .. i .. "].id") end
          seen[service.id] = true
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
  local source = config.services or default_services()
  local services = {}
  for i = 1, #source do
    local service = source[i]
    services[i] = {
      id = service.id,
      provider_building_ids = copy_array(service.provider_building_ids),
      provider_building_categories = copy_array(service.provider_building_categories),
      radius = service.radius,
      capacity = service.capacity,
      coverage_target_tags = copy_array(service.coverage_target_tags),
      quality = service.quality or 1,
      priority = service.priority or 0,
      need_ids = copy_array(service.need_ids)
    }
  end
  local data = {
    schema = "city_builder.service_coverage.v1",
    coverage_model = config.coverage_model or "radius_metadata",
    services = services,
    metadata = {
      deterministic = true,
      generated_by = M.manifest.id,
      note = "Coverage entries are config IR; host validation may later combine them with maps and path data."
    }
  }
  return { ok = true, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
