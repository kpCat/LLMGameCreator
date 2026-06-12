local M = {}

M.manifest = {
  id = "generation/module_manifest/v1",
  version = "0.1.0",
  category = "generation",
  title = "Generator module manifest helpers",
  purpose = "Validate, normalize, and index Lua generator module manifests before a host imports them into a registry.",
  capabilities = {
    "generation.module_manifest.validate",
    "generation.module_manifest.normalize",
    "generation.module_manifest.index",
    "generation.module_manifest.capability_lookup"
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

local function is_slash_id(value)
  if type(value) ~= "string" or value == "" then
    return false
  end
  if string.lower(value) ~= value then
    return false
  end
  if string.sub(value, 1, 1) == "/" or string.sub(value, #value, #value) == "/" then
    return false
  end
  if string.find(value, "//", 1, true) ~= nil then
    return false
  end
  local count = 0
  for segment in string.gmatch(value, "[^/]+") do
    count = count + 1
    if string.match(segment, "^[a-z0-9][a-z0-9_%-]*$") == nil then
      return false
    end
  end
  return count >= 2
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

local function validate_capability_array(values, code_prefix, target, diagnostics)
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

local function normalize_relation_list(value)
  if value == nil then
    return { modules = {}, capabilities = {} }
  end
  if is_array(value) then
    return { modules = copy_table(value, 0), capabilities = {} }
  end
  if type(value) == "table" then
    return {
      modules = array_or_default(value.modules, {}),
      capabilities = array_or_default(value.capabilities, {})
    }
  end
  return { modules = {}, capabilities = {} }
end

local function validate_relation_list(value, code_prefix, target, diagnostics)
  local normalized = normalize_relation_list(value)
  local seen_modules = {}
  for index = 1, #normalized.modules do
    local id = normalized.modules[index]
    local item_target = target .. ".modules." .. tostring(index)
    if not is_slash_id(id) then
      add_error(diagnostics, code_prefix .. ".module_invalid_id", "Module dependency id must be lowercase slash notation.", item_target)
    elseif seen_modules[id] == true then
      add_error(diagnostics, code_prefix .. ".module_duplicate", "Module dependency id must be unique.", item_target)
    else
      seen_modules[id] = true
    end
  end
  validate_capability_array(normalized.capabilities, code_prefix .. ".capabilities", target .. ".capabilities", diagnostics)
end

local function normalize_module(source)
  local manifest = type(source) == "table" and source or {}
  return {
    id = manifest.id or "",
    version = type(manifest.version) == "string" and manifest.version or "",
    category = type(manifest.category) == "string" and manifest.category or "generation",
    title = type(manifest.title) == "string" and manifest.title or "",
    purpose = type(manifest.purpose) == "string" and manifest.purpose or "",
    capabilities = array_or_default(manifest.capabilities, {}),
    input_schema = type(manifest.input_schema) == "table" and copy_table(manifest.input_schema, 0) or {},
    output_schema = type(manifest.output_schema) == "table" and copy_table(manifest.output_schema, 0) or {},
    config_schema = type(manifest.config_schema) == "table" and copy_table(manifest.config_schema, 0) or {},
    deterministic = manifest.deterministic ~= false,
    runtime_targets = array_or_default(manifest.runtime_targets or manifest.supported_runtime_targets, DEFAULT_RUNTIME_TARGETS),
    supported_time_modes = array_or_default(manifest.supported_time_modes or manifest.time_modes, DEFAULT_TIME_MODES),
    supported_combat_modes = array_or_default(manifest.supported_combat_modes or manifest.combat_modes, DEFAULT_COMBAT_MODES),
    dependencies = normalize_relation_list(manifest.dependencies),
    incompatibilities = normalize_relation_list(manifest.incompatibilities),
    unsafe_features = array_or_default(manifest.unsafe_features, {})
  }
end

function M.is_module_id(value)
  return is_slash_id(value)
end

function M.is_capability_id(value)
  return is_capability_id(value)
end

function M.normalize_module(manifest)
  return normalize_module(manifest)
end

function M.validate_module(manifest, options)
  local diagnostics = {}
  local normalized = normalize_module(manifest)
  local validation_options = type(options) == "table" and options or {}

  if type(manifest) ~= "table" then
    add_error(diagnostics, "generation.module.not_table", "Module manifest must be a table.", "manifest")
    return false, diagnostics
  end

  if not is_slash_id(normalized.id) then
    add_error(diagnostics, "generation.module.invalid_id", "Module id must be lowercase slash notation such as generation/module_manifest/v1.", "manifest.id")
  end
  if normalized.version == "" then
    add_error(diagnostics, "generation.module.missing_version", "Module manifest must declare version.", "manifest.version")
  end
  if normalized.title == "" then
    add_warning(diagnostics, "generation.module.missing_title", "Module title is recommended for registry inspection.", "manifest.title")
  end
  if normalized.purpose == "" then
    add_warning(diagnostics, "generation.module.missing_purpose", "Module purpose is recommended for LLM planning.", "manifest.purpose")
  end

  validate_capability_array(normalized.capabilities, "generation.module.capabilities", "manifest.capabilities", diagnostics)

  if manifest.input_schema ~= nil and type(manifest.input_schema) ~= "table" then
    add_error(diagnostics, "generation.module.input_schema_not_table", "Input schema must be a table.", "manifest.input_schema")
  end
  if manifest.output_schema ~= nil and type(manifest.output_schema) ~= "table" then
    add_error(diagnostics, "generation.module.output_schema_not_table", "Output schema must be a table.", "manifest.output_schema")
  end
  if manifest.config_schema ~= nil and type(manifest.config_schema) ~= "table" then
    add_error(diagnostics, "generation.module.config_schema_not_table", "Config schema must be a table.", "manifest.config_schema")
  end

  validate_unique_string_array(normalized.runtime_targets, RUNTIME_TARGETS, { allow_unknown = validation_options.allow_unknown_runtime_targets == true }, "generation.module.runtime_targets", "manifest.runtime_targets", diagnostics)
  validate_unique_string_array(normalized.supported_time_modes, TIME_MODES, { allow_unknown = validation_options.allow_unknown_time_modes == true }, "generation.module.time_modes", "manifest.supported_time_modes", diagnostics)
  validate_unique_string_array(normalized.supported_combat_modes, COMBAT_MODES, { allow_unknown = validation_options.allow_unknown_combat_modes == true }, "generation.module.combat_modes", "manifest.supported_combat_modes", diagnostics)
  validate_unique_string_array(normalized.unsafe_features, nil, { allow_unknown = true }, "generation.module.unsafe_features", "manifest.unsafe_features", diagnostics)
  validate_relation_list(normalized.dependencies, "generation.module.dependencies", "manifest.dependencies", diagnostics)
  validate_relation_list(normalized.incompatibilities, "generation.module.incompatibilities", "manifest.incompatibilities", diagnostics)

  local has_error = false
  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      has_error = true
    end
  end
  return not has_error, diagnostics
end

function M.normalize(manifest, options)
  local ok, diagnostics = M.validate_module(manifest, options)
  return result(ok, { module = normalize_module(manifest) }, diagnostics)
end

function M.index(modules, options)
  local diagnostics = {}
  local list = {}
  local by_id = {}
  local capability_to_modules = {}
  local dependency_edges = {}
  local incompatibility_edges = {}

  if not is_array(modules) then
    add_error(diagnostics, "generation.module.index.not_array", "Modules input must be an array.", "modules")
    return result(false, {
      modules = list,
      by_id = by_id,
      capability_to_modules = capability_to_modules,
      dependency_edges = dependency_edges,
      incompatibility_edges = incompatibility_edges
    }, diagnostics)
  end

  for index = 1, #modules do
    local item = modules[index]
    local normalized = normalize_module(item)
    local ok, item_diagnostics = M.validate_module(item, options)
    for d = 1, #item_diagnostics do
      local diagnostic = copy_table(item_diagnostics[d], 0)
      diagnostic.target = "modules." .. tostring(index) .. (diagnostic.target ~= nil and "." .. diagnostic.target or "")
      diagnostics[#diagnostics + 1] = diagnostic
    end
    if normalized.id ~= "" and by_id[normalized.id] ~= nil then
      add_error(diagnostics, "generation.module.index.duplicate_id", "Module id must be unique in registry input.", "modules." .. tostring(index) .. ".id")
    end
    if ok and by_id[normalized.id] == nil then
      list[#list + 1] = normalized
      by_id[normalized.id] = normalized
      dependency_edges[normalized.id] = copy_table(normalized.dependencies, 0)
      incompatibility_edges[normalized.id] = copy_table(normalized.incompatibilities, 0)
      for cap_index = 1, #normalized.capabilities do
        local capability_id = normalized.capabilities[cap_index]
        capability_to_modules[capability_id] = capability_to_modules[capability_id] or {}
        capability_to_modules[capability_id][#capability_to_modules[capability_id] + 1] = normalized.id
      end
    end
  end

  for item_index = 1, #list do
    local module_id = list[item_index].id
    local relations = dependency_edges[module_id]
    for index = 1, #relations.modules do
      if by_id[relations.modules[index]] == nil then
        add_warning(diagnostics, "generation.module.index.missing_module_dependency", "Module dependency is not present in the provided registry slice.", "modules." .. module_id .. ".dependencies.modules." .. tostring(index))
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
    modules = list,
    by_id = by_id,
    capability_to_modules = capability_to_modules,
    dependency_edges = dependency_edges,
    incompatibility_edges = incompatibility_edges
  }, diagnostics)
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and type(config) ~= "table" then
    add_error(diagnostics, "generation.module.config_not_table", "Config must be a table when provided.", "config")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local _ctx = ctx
  local source = type(input) == "table" and input or {}
  local modules = source.modules or source.manifests or source
  local options = source.options or source.config or {}
  return M.index(modules, options)
end

return M
