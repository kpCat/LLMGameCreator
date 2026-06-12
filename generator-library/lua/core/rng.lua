local M = {}

M.manifest = {
  id = "core/rng/v1",
  version = "0.1.0",
  category = "core",
  title = "Deterministic RNG",
  purpose = "Provide deterministic seed/state helpers for generator modules without using math.random.",
  capabilities = { "core.rng.seed", "core.rng.next", "core.rng.choice", "core.rng.shuffle" },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      seed = { type = "integer", min = 1, max = 2147483646 }
    }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "simulation", "codegen_ir" },
  unsafe_features = {}
}

local MODULUS = 2147483647
local MULTIPLIER = 48271
local DEFAULT_SEED = 1

local function diagnostic(code, message, target)
  local item = {
    severity = "error",
    code = code,
    message = message
  }
  if target ~= nil then
    item.target = target
  end
  return item
end

local function is_integer(value)
  return type(value) == "number" and value == (value // 1)
end

function M.normalize_seed(seed)
  if not is_integer(seed) then
    return DEFAULT_SEED
  end

  local normalized = seed
  if normalized < 0 then
    normalized = -normalized
  end

  normalized = normalized % MODULUS
  if normalized == 0 then
    normalized = DEFAULT_SEED
  end

  return normalized
end

function M.derive_seed(seed, salt)
  local derived = M.normalize_seed(seed)
  local text = type(salt) == "string" and salt or ""

  for index = 1, #text do
    derived = ((derived * 131) + string.byte(text, index)) % MODULUS
  end

  return M.normalize_seed(derived)
end

function M.new(seed)
  return { seed = M.normalize_seed(seed) }
end

local function resolve_seed(state)
  if type(state) == "table" then
    return M.normalize_seed(state.seed)
  end
  return M.normalize_seed(state)
end

function M.next_int(state)
  local current = resolve_seed(state)
  local next_seed = (current * MULTIPLIER) % MODULUS
  if next_seed == 0 then
    next_seed = DEFAULT_SEED
  end
  return { seed = next_seed }, next_seed
end

function M.next_float(state)
  local next_state, value = M.next_int(state)
  return next_state, value / MODULUS
end

function M.range_int(state, min_value, max_value)
  if not is_integer(min_value) or not is_integer(max_value) then
    return state, nil, { diagnostic("core.rng.range_not_integer", "range_int bounds must be integers.", "range") }
  end

  if max_value < min_value then
    return state, nil, { diagnostic("core.rng.range_reversed", "range_int max_value must be greater than or equal to min_value.", "range") }
  end

  local next_state, raw = M.next_int(state)
  local span = max_value - min_value + 1
  local value = min_value + (raw % span)
  return next_state, value, {}
end

function M.choice(state, values)
  if type(values) ~= "table" or #values == 0 then
    return state, nil, nil, { diagnostic("core.rng.empty_choice", "choice requires a non-empty array table.", "values") }
  end

  local next_state, index, diagnostics = M.range_int(state, 1, #values)
  if diagnostics[1] ~= nil then
    return next_state, nil, nil, diagnostics
  end

  return next_state, values[index], index, {}
end

function M.shuffle(state, values)
  if type(values) ~= "table" then
    return state, {}, { diagnostic("core.rng.shuffle_not_table", "shuffle requires an array table.", "values") }
  end

  local result = {}
  for index = 1, #values do
    result[index] = values[index]
  end

  local next_state = state
  for index = #result, 2, -1 do
    local swap_index
    next_state, swap_index = M.range_int(next_state, 1, index)
    result[index], result[swap_index] = result[swap_index], result[index]
  end

  return next_state, result, {}
end

function M.validate_config(config)
  local diagnostics = {}

  if config == nil then
    return true, diagnostics
  end

  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diagnostic("core.rng.config_not_table", "RNG config must be a table.", "config")
    return false, diagnostics
  end

  if config.seed ~= nil and not is_integer(config.seed) then
    diagnostics[#diagnostics + 1] = diagnostic("core.rng.seed_not_integer", "seed must be an integer when provided.", "config.seed")
  end

  if is_integer(config.seed) and (config.seed < 1 or config.seed >= MODULUS) then
    diagnostics[#diagnostics + 1] = diagnostic("core.rng.seed_out_of_range", "seed must be between 1 and 2147483646.", "config.seed")
  end

  return diagnostics[1] == nil, diagnostics
end

return M
