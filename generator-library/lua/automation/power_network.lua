local M = {}

M.manifest = {
  id = "automation/power_network/v1",
  version = "0.1.0",
  category = "automation",
  title = "Power network",
  purpose = "Generate deterministic power-network IR and compact power balance estimates for automation planning.",
  capabilities = {
    "automation.power_network.generate",
    "automation.power_balance.estimate",
    "automation.energy_source.ir"
  },
  input_schema = {
    generators = "array optional",
    consumers = "array optional",
    accumulators = "array optional"
  },
  output_schema = {
    power_network = "object",
    power_balance = "object"
  },
  config_schema = {
    reserve_ratio = "number optional"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "simulation", "codegen_ir" },
  unsafe_features = {}
}

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function normalize_id(value, fallback)
  if type(value) == "string" and value ~= "" then
    return value
  end
  return fallback
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

local function normalize_generator(raw, index, diagnostics)
  if type(raw) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.power_network.generator_not_table", "Generator entry must be a table.", "generators[" .. index .. "]")
    return nil
  end

  local id = normalize_id(raw.id, "automation/power_generator/generated_" .. index)
  local capacity_kw = tonumber(raw.capacity_kw or raw.output_kw or raw.power_kw) or 0
  if capacity_kw <= 0 then
    diagnostics[#diagnostics + 1] = diag("error", "automation.power_network.generator_capacity_invalid", "Generator capacity must be positive.", id)
    capacity_kw = 0
  end

  return {
    id = id,
    kind = normalize_id(raw.kind or raw.type, "generator"),
    capacity_kw = capacity_kw,
    fuel_item_id = raw.fuel_item_id,
    tags = copy_list(raw.tags)
  }
end

local function normalize_consumer(raw, index, diagnostics)
  if type(raw) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.power_network.consumer_not_table", "Consumer entry must be a table.", "consumers[" .. index .. "]")
    return nil
  end

  local id = normalize_id(raw.id, "automation/power_consumer/generated_" .. index)
  local demand_kw = tonumber(raw.demand_kw or raw.power_demand_kw or raw.energy_kw) or 0
  if demand_kw < 0 then
    diagnostics[#diagnostics + 1] = diag("error", "automation.power_network.consumer_demand_invalid", "Consumer demand cannot be negative.", id)
    demand_kw = 0
  end

  return {
    id = id,
    kind = normalize_id(raw.kind or raw.type, "machine"),
    demand_kw = demand_kw,
    priority = tonumber(raw.priority) or 1,
    tags = copy_list(raw.tags)
  }
end

local function normalize_accumulator(raw, index, diagnostics)
  if type(raw) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.power_network.accumulator_not_table", "Accumulator entry must be a table.", "accumulators[" .. index .. "]")
    return nil
  end

  local id = normalize_id(raw.id, "automation/accumulator/generated_" .. index)
  local capacity_kj = tonumber(raw.capacity_kj or raw.storage_kj) or 0
  local transfer_kw = tonumber(raw.transfer_kw or raw.max_transfer_kw) or 0
  if capacity_kj < 0 then
    diagnostics[#diagnostics + 1] = diag("error", "automation.power_network.accumulator_capacity_invalid", "Accumulator capacity cannot be negative.", id)
    capacity_kj = 0
  end
  if transfer_kw < 0 then
    diagnostics[#diagnostics + 1] = diag("error", "automation.power_network.accumulator_transfer_invalid", "Accumulator transfer cannot be negative.", id)
    transfer_kw = 0
  end

  return {
    id = id,
    capacity_kj = capacity_kj,
    transfer_kw = transfer_kw,
    tags = copy_list(raw.tags)
  }
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.power_network.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.reserve_ratio ~= nil and (type(config.reserve_ratio) ~= "number" or config.reserve_ratio < 0) then
    diagnostics[#diagnostics + 1] = diag("error", "automation.power_network.reserve_ratio_invalid", "reserve_ratio must be non-negative.", "config.reserve_ratio")
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
    diagnostics[#diagnostics + 1] = diag("error", "automation.power_network.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local generators = {}
  local total_capacity_kw = 0
  for index = 1, #(input.generators or {}) do
    local generator = normalize_generator(input.generators[index], index, diagnostics)
    if generator ~= nil then
      total_capacity_kw = total_capacity_kw + generator.capacity_kw
      generators[#generators + 1] = generator
    end
  end
  table.sort(generators, function(left, right)
    return left.id < right.id
  end)

  local consumers = {}
  local total_demand_kw = 0
  for index = 1, #(input.consumers or {}) do
    local consumer = normalize_consumer(input.consumers[index], index, diagnostics)
    if consumer ~= nil then
      total_demand_kw = total_demand_kw + consumer.demand_kw
      consumers[#consumers + 1] = consumer
    end
  end
  table.sort(consumers, function(left, right)
    if left.priority == right.priority then
      return left.id < right.id
    end
    return left.priority < right.priority
  end)

  local accumulators = {}
  local total_storage_kj = 0
  local total_transfer_kw = 0
  for index = 1, #(input.accumulators or {}) do
    local accumulator = normalize_accumulator(input.accumulators[index], index, diagnostics)
    if accumulator ~= nil then
      total_storage_kj = total_storage_kj + accumulator.capacity_kj
      total_transfer_kw = total_transfer_kw + accumulator.transfer_kw
      accumulators[#accumulators + 1] = accumulator
    end
  end
  table.sort(accumulators, function(left, right)
    return left.id < right.id
  end)

  local reserve_ratio = tonumber(config.reserve_ratio) or 0.15
  local required_with_reserve = total_demand_kw * (1 + reserve_ratio)
  local surplus_kw = total_capacity_kw - total_demand_kw
  local reserve_ok = total_capacity_kw >= required_with_reserve
  if total_capacity_kw < total_demand_kw then
    diagnostics[#diagnostics + 1] = diag("error", "automation.power_network.power_deficit", "Power capacity is lower than demand.", "power_balance")
  elseif not reserve_ok then
    diagnostics[#diagnostics + 1] = diag("warning", "automation.power_network.reserve_low", "Power reserve is below configured ratio.", "power_balance")
  end

  local data = {
    power_network = {
      generators = generators,
      consumers = consumers,
      accumulators = accumulators
    },
    power_balance = {
      capacity_kw = total_capacity_kw,
      demand_kw = total_demand_kw,
      required_with_reserve_kw = required_with_reserve,
      surplus_kw = surplus_kw,
      reserve_ratio = reserve_ratio,
      reserve_ok = reserve_ok,
      storage_kj = total_storage_kj,
      transfer_kw = total_transfer_kw
    },
    validation = {
      generator_count = #generators,
      consumer_count = #consumers,
      accumulator_count = #accumulators,
      config_ok = config_ok
    }
  }

  return { ok = #diagnostics == 0, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
