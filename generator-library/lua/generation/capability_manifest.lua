local M = {}

M.manifest = {
  id = "generation/capability_manifest/v1",
  version = "0.1.0",
  category = "generation",
  title = "Capability manifest helpers",
  purpose = "Validate, normalize, and index capability manifests used by generator selection and planning.",
  capabilities = {
    "generation.capability_manifest.validate",
    "generation.capability_manifest.normalize",
    "generation.capability_manifest.index"
  },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      allow_unknown_runtime_targets = { type = "boolean" },
      allow_unknown_time_modes = { type = "boolean" },
      allow_unknown_combat_modes = { type = "boolean" }
    }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "simulation", "codegen_ir" },
  supported_time_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local RUNTIME_TARGETS = {
  debug = true,
  unity2d = true,
  unity3d = true,
  simulation = true,
  codegen_ir = true,
  validation = true,
  editor = true
}

local TIME_MODES = {
  realtime = true,
  turn_based = true,
  mixed = true,
  paused_planning = true
}

local COMBAT_MODES = {
  none = true,
  realtime = true,
  turn_based = true,
  tactical = true,
  dialogue_combat = true,
  hybrid = true
}

local DEFAULT_RUNTIME_TARGETS = { "debug" }
local DEFAULT_TIME_MODES = { "realtime", "turn_based", "mixed" }
local DEFAULT_COMBAT_MODES = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" }

local function make_diagnostic(severity, code, message, target)
  local diagnostic = {
    severity = severity,
    code = code,
    message = message
  }
  if target ~= nil then
    diagnostic.target = target
  end
  return diagnostic
end

local function add_error(diagnostics, code, message, target)
  diagnostics[#diagnostics + 1] = make_diagnostic("error", code, message, target)
end

local function add_warning(diagnostics, code, message, target)
  diagnostics[#diagnostics + 1] = make_diagnostic("warning", code, message, target)
end

local function result(ok, data, diagnostics)
  return {
    ok = ok == true,
    data = type(data) == "table" and data or {},
    diagnostics = type(diagnostics) == "table" and diagnostics or {},
    artifacts = {}
  }
end

local function is_integer(value)
  return type(value) == "number" and value == (value // 1)
end

local function is_array(value)
  if type(value) ~= "table" then
    return false
  end
  local count = 0
  local max_index = 0
  for key, _ in pairs(value) do
    if not is_integer(key) or key < 1 then
      return false
    end
    count = count + 1
    if key > max_index then
      max_index = key
    end
  end
  return count == max_index
end

local function copy_table(value, depth)
  if type(value) ~= "table" then
    return value
  end
  if depth > 16 then
    return {}
  end
  local copy = {}
  for key, item in pairs(value) do
    copy[key] = copy_table(item, depth + 1)
  end
  return copy
end

local function array_or_default(value, fallback)
  if is_array(value) then
    return copy_table(value, 0)
  end
  return copy_table(fallback, 0)
end

local function is_capability_id(value)
  if type(value) ~= "string" or value == "" then
    return false
  end
  if string.lower(value) ~= value then
    return false
  end
  if string.sub(value, 1, 1) == "." or string.sub(value, #value, #value) == "." then
    return false
  end
  if string.find(value, "..", 1, true) ~= nil then
    return false
  end
  local count = 0
  for segment in string.gmatch(value, "[^%.]+") do
    count = count + 1
    if string.match(segment, "^[a-z][a-z0-9_]*$") == nil then
      return false
    end
  end
  return count >= 2
end

local function is_port_id(value)
  return type(value) == "string" and string.match(value, "^[a-z][a-z0-9_]*$") ~= nil
end

local function validate_unique_string_array(values, allowed, options, code_prefix, target, diagnostics)
  if not is_array(values) then
    add_error(diagnostics, code_prefix .. ".not_array", "Value must be an array of strings.", target)
    return
  end
  local seen = {}
  for index = 1, #values do
    local item = values[index]
    local item_target = target .. "." .. tostring(index)
    if type(item) ~= "string" or item == "" then
      add_error(diagnostics, code_prefix .. ".not_string", "Array item must be a non-empty string.", item_target)
    elseif seen[item] == true then
      add_error(diagnostics, code_prefix .. ".duplicate", "Array item must be unique.", item_target)
    else
      seen[item] = true
      if allowed ~= nil and allowed[item] ~= true and options.allow_unknown ~= true then
        add_error(diagnostics, code_prefix .. ".unsupported", "Array item is not in the supported set.", item_target)
      end
    end
  end
end

local function validate_capability_id_array(values, code_prefix, target, diagnostics)
  if not is_array(values) then
    add_error(diagnostics, code_prefix .. ".not_array", "Value must be an array of capability ids.", target)
    return
  end
  local seen = {}
  for index = 1, #values do
    local item = values[index]
    local item_target = target .. "." .. tostring(index)
    if not is_capability_id(item) then
      add_error(diagnostics, code_prefix .. ".invalid_id", "Capability id must be lowercase dot notation such as world.chunk.generate.", item_target)
    elseif seen[item] == true then
      add_error(diagnostics, code_prefix .. ".duplicate", "Capability id must be unique in this array.", item_target)
    else
      seen[item] = true
    end
  end
end

local function normalize_port(port)
  if type(port) == "string" then
    return {
      id = port,
      schema = {},
      required = false,
      description = ""
    }
  end
  local source = type(port) == "table" and port or {}
  return {
    id = source.id or source.name or "",
    schema = type(source.schema) == "table" and copy_table(source.schema, 0) or {},
    required = source.required == true,
    description = type(source.description) == "string" and source.description or ""
  }
end

local function normalize_ports(ports)
  local normalized = {}
  if not is_array(ports) then
    return normalized
  end
  for index = 1, #ports do
    normalized[#normalized + 1] = normalize_port(ports[index])
  end
  return normalized
end

local function validate_ports(ports, code_prefix, target, diagnostics)
  if ports == nil then
    return
  end
  if not is_array(ports) then
    add_error(diagnostics, code_prefix .. ".not_array", "Ports must be an array.", target)
    return
  end
  local seen = {}
  for index = 1, #ports do
    local port = normalize_port(ports[index])
    local item_target = target .. "." .. tostring(index)
    if not is_port_id(port.id) then
      add_error(diagnostics, code_prefix .. ".invalid_id", "Port id must be lower snake case and start with a letter.", item_target .. ".id")
    elseif seen[port.id] == true then
      add_error(diagnostics, code_prefix .. ".duplicate", "Port id must be unique.", item_target .. ".id")
    else
      seen[port.id] = true
    end
    if type(port.schema) ~= "table" then
      add_error(diagnostics, code_prefix .. ".schema_not_table", "Port schema must be a table.", item_target .. ".schema")
    end
  end
end

local function normalize_capability(source)
  local capability = type(source) == "table" and source or {}
  return {
    id = capability.id or "",
    title = type(capability.title) == "string" and capability.title or "",
    purpose = type(capability.purpose) == "string" and capability.purpose or "",
    category = type(capability.category) == "string" and capability.category or "generation",
    inputs = normalize_ports(capability.inputs),
    outputs = normalize_ports(capability.outputs),
    config_schema = type(capability.config_schema) == "table" and copy_table(capability.config_schema, 0) or {},
    supported_runtime_targets = array_or_default(capability.supported_runtime_targets or capability.runtime_targets, DEFAULT_RUNTIME_TARGETS),
    supported_time_modes = array_or_default(capability.supported_time_modes or capability.time_modes, DEFAULT_TIME_MODES),
    supported_combat_modes = array_or_default(capability.supported_combat_modes or capability.combat_modes, DEFAULT_COMBAT_MODES),
    dependencies = array_or_default(capability.dependencies, {}),
    incompatibilities = array_or_default(capability.incompatibilities, {}),
    tags = array_or_default(capability.tags, {})
  }
end

function M.supported_runtime_targets()
  return { "debug", "unity2d", "unity3d", "simulation", "codegen_ir", "validation", "editor" }
end

function M.supported_time_modes()
  return { "realtime", "turn_based", "mixed", "paused_planning" }
end

function M.supported_combat_modes()
  return { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" }
end

function M.is_capability_id(value)
  return is_capability_id(value)
end

function M.normalize_capability(capability)
  return normalize_capability(capability)
end

function M.validate_capability(capability, options)
  local diagnostics = {}
  local normalized = normalize_capability(capability)
  local validation_options = type(options) == "table" and options or {}

  if type(capability) ~= "table" then
    add_error(diagnostics, "generation.capability.not_table", "Capability manifest must be a table.", "capability")
    return false, diagnostics
  end

  if not is_capability_id(normalized.id) then
    add_error(diagnostics, "generation.capability.invalid_id", "Capability id must be lowercase dot notation such as world.chunk.generate.", "capability.id")
  end

  if normalized.title == "" then
    add_warning(diagnostics, "generation.capability.missing_title", "Capability title is recommended for registry inspection.", "capability.title")
  end
  if normalized.purpose == "" then
    add_warning(diagnostics, "generation.capability.missing_purpose", "Capability purpose is recommended for LLM planning.", "capability.purpose")
  end

  validate_ports(capability.inputs or {}, "generation.capability.inputs", "capability.inputs", diagnostics)
  validate_ports(capability.outputs or {}, "generation.capability.outputs", "capability.outputs", diagnostics)

  if capability.config_schema ~= nil and type(capability.config_schema) ~= "table" then
    add_error(diagnostics, "generation.capability.config_schema_not_table", "Config schema must be a table.", "capability.config_schema")
  end

  validate_unique_string_array(normalized.supported_runtime_targets, RUNTIME_TARGETS, { allow_unknown = validation_options.allow_unknown_runtime_targets == true }, "generation.capability.runtime_targets", "capability.supported_runtime_targets", diagnostics)
  validate_unique_string_array(normalized.supported_time_modes, TIME_MODES, { allow_unknown = validation_options.allow_unknown_time_modes == true }, "generation.capability.time_modes", "capability.supported_time_modes", diagnostics)
  validate_unique_string_array(normalized.supported_combat_modes, COMBAT_MODES, { allow_unknown = validation_options.allow_unknown_combat_modes == true }, "generation.capability.combat_modes", "capability.supported_combat_modes", diagnostics)
  validate_capability_id_array(normalized.dependencies, "generation.capability.dependencies", "capability.dependencies", diagnostics)
  validate_capability_id_array(normalized.incompatibilities, "generation.capability.incompatibilities", "capability.incompatibilities", diagnostics)
  validate_unique_string_array(normalized.tags, nil, { allow_unknown = true }, "generation.capability.tags", "capability.tags", diagnostics)

  local has_error = false
  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      has_error = true
    end
  end
  return not has_error, diagnostics
end

function M.normalize(capability, options)
  local ok, diagnostics = M.validate_capability(capability, options)
  if not ok then
    return result(false, { capability = normalize_capability(capability) }, diagnostics)
  end
  return result(true, { capability = normalize_capability(capability) }, diagnostics)
end

function M.index(capabilities, options)
  local diagnostics = {}
  local list = {}
  local by_id = {}
  local dependency_edges = {}
  local incompatibility_edges = {}

  if not is_array(capabilities) then
    add_error(diagnostics, "generation.capability.index.not_array", "Capabilities input must be an array.", "capabilities")
    return result(false, { capabilities = list, by_id = by_id, dependency_edges = dependency_edges, incompatibility_edges = incompatibility_edges }, diagnostics)
  end

  for index = 1, #capabilities do
    local item = capabilities[index]
    local normalized = normalize_capability(item)
    local ok, item_diagnostics = M.validate_capability(item, options)
    for d = 1, #item_diagnostics do
      local diagnostic = copy_table(item_diagnostics[d], 0)
      diagnostic.target = "capabilities." .. tostring(index) .. (diagnostic.target ~= nil and "." .. diagnostic.target or "")
      diagnostics[#diagnostics + 1] = diagnostic
    end
    if by_id[normalized.id] ~= nil and normalized.id ~= "" then
      add_error(diagnostics, "generation.capability.index.duplicate_id", "Capability id must be unique in registry input.", "capabilities." .. tostring(index) .. ".id")
    end
    if ok and by_id[normalized.id] == nil then
      list[#list + 1] = normalized
      by_id[normalized.id] = normalized
      dependency_edges[normalized.id] = copy_table(normalized.dependencies, 0)
      incompatibility_edges[normalized.id] = copy_table(normalized.incompatibilities, 0)
    end
  end

  for item_index = 1, #list do
    local id = list[item_index].id
    local deps = dependency_edges[id]
    for index = 1, #deps do
      if by_id[deps[index]] == nil then
        add_warning(diagnostics, "generation.capability.index.missing_dependency", "Capability dependency is not present in the provided registry slice.", "capabilities." .. id .. ".dependencies." .. tostring(index))
      end
    end
  end

  local has_error = false
  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      has_error = true
    end
  end

  return result(not has_error, {
    capabilities = list,
    by_id = by_id,
    dependency_edges = dependency_edges,
    incompatibility_edges = incompatibility_edges
  }, diagnostics)
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and type(config) ~= "table" then
    add_error(diagnostics, "generation.capability.config_not_table", "Config must be a table when provided.", "config")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local _ctx = ctx
  local source = type(input) == "table" and input or {}
  local capabilities = source.capabilities or source
  local options = source.options or source.config or {}
  return M.index(capabilities, options)
end

return M
