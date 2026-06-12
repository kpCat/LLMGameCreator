local M = {}

M.manifest = {
  id = "core/schema/v1",
  version = "0.1.0",
  category = "core",
  title = "Lightweight schema validation",
  purpose = "Validate compact JSON-like Lua data tables used by generator configs, inputs, and outputs.",
  capabilities = { "core.schema.validate", "core.schema.json_serializable" },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      max_depth = { type = "integer", min = 1, max = 64 }
    }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "codegen_ir" },
  unsafe_features = {}
}

local function make_diagnostic(code, message, target)
  local diagnostic = {
    severity = "error",
    code = code,
    message = message
  }
  if target ~= nil then
    diagnostic.target = target
  end
  return diagnostic
end

local function add(diagnostics, code, message, target)
  diagnostics[#diagnostics + 1] = make_diagnostic(code, message, target)
end

local function is_integer(value)
  return type(value) == "number" and value == (value // 1)
end


local function sorted_keys(value)
  local keys = {}
  if type(value) ~= "table" then
    return keys
  end
  for key, _ in pairs(value) do
    keys[#keys + 1] = key
  end
  table.sort(keys, function(left, right)
    return tostring(left) < tostring(right)
  end)
  return keys
end

local function path_join(path, key)
  local text = tostring(key)
  if path == nil or path == "" then
    return text
  end
  return path .. "." .. text
end

function M.is_array(value)
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

local function has_error(diagnostics)
  return diagnostics[1] ~= nil
end

local function validate_enum(value, schema, diagnostics, path)
  if type(schema.enum) ~= "table" then
    return
  end

  for index = 1, #schema.enum do
    if value == schema.enum[index] then
      return
    end
  end

  add(diagnostics, "core.schema.enum_mismatch", "Value is not included in the allowed enum.", path)
end

local function validate_number_rules(value, schema, diagnostics, path)
  if type(value) ~= "number" then
    return
  end
  if schema.min ~= nil and value < schema.min then
    add(diagnostics, "core.schema.number_too_small", "Number is smaller than the configured minimum.", path)
  end
  if schema.max ~= nil and value > schema.max then
    add(diagnostics, "core.schema.number_too_large", "Number is larger than the configured maximum.", path)
  end
end

local function validate_string_rules(value, schema, diagnostics, path)
  if type(value) ~= "string" then
    return
  end
  local length = #value
  if schema.min_length ~= nil and length < schema.min_length then
    add(diagnostics, "core.schema.string_too_short", "String is shorter than the configured minimum length.", path)
  end
  if schema.max_length ~= nil and length > schema.max_length then
    add(diagnostics, "core.schema.string_too_long", "String is longer than the configured maximum length.", path)
  end
end

local function validate_array(value, schema, diagnostics, path, depth, max_depth, validate_value)
  if not M.is_array(value) then
    add(diagnostics, "core.schema.expected_array", "Expected an array-like table with contiguous integer keys starting at 1.", path)
    return
  end

  if schema.min_items ~= nil and #value < schema.min_items then
    add(diagnostics, "core.schema.array_too_short", "Array contains fewer items than allowed.", path)
  end
  if schema.max_items ~= nil and #value > schema.max_items then
    add(diagnostics, "core.schema.array_too_long", "Array contains more items than allowed.", path)
  end

  if type(schema.items) == "table" then
    for index = 1, #value do
      validate_value(value[index], schema.items, path_join(path, index), diagnostics, depth + 1, max_depth)
    end
  end
end

local function required_set(required)
  local set = {}
  if type(required) ~= "table" then
    return set
  end
  for index = 1, #required do
    if type(required[index]) == "string" then
      set[required[index]] = true
    end
  end
  return set
end

local function validate_object(value, schema, diagnostics, path, depth, max_depth, validate_value)
  if type(value) ~= "table" then
    add(diagnostics, "core.schema.expected_object", "Expected an object table.", path)
    return
  end

  local properties = type(schema.properties) == "table" and schema.properties or {}
  local required_list = type(schema.required) == "table" and schema.required or {}
  local required = required_set(required_list)

  for index = 1, #required_list do
    local name = required_list[index]
    if type(name) == "string" and required[name] == true and value[name] == nil then
      add(diagnostics, "core.schema.required_missing", "Required property is missing.", path_join(path, name))
    end
  end

  local property_keys = sorted_keys(properties)
  for index = 1, #property_keys do
    local name = property_keys[index]
    local property_schema = properties[name]
    if value[name] ~= nil then
      validate_value(value[name], property_schema, path_join(path, name), diagnostics, depth + 1, max_depth)
    end
  end

  if schema.allow_unknown == false then
    local value_keys = sorted_keys(value)
    for index = 1, #value_keys do
      local name = value_keys[index]
      if properties[name] == nil then
        add(diagnostics, "core.schema.unknown_property", "Unknown property is not allowed by this schema.", path_join(path, name))
      end
    end
  end
end

local function type_matches(value, expected)
  if expected == "any" then
    return true
  end
  if expected == "integer" then
    return is_integer(value)
  end
  if expected == "array" then
    return type(value) == "table"
  end
  if expected == "object" then
    return type(value) == "table"
  end
  if expected == "table" then
    return type(value) == "table"
  end
  return type(value) == expected
end

local validate_value
validate_value = function(value, schema, path, diagnostics, depth, max_depth)
  if depth > max_depth then
    add(diagnostics, "core.schema.max_depth_exceeded", "Schema validation exceeded configured maximum depth.", path)
    return
  end

  if type(schema) ~= "table" then
    add(diagnostics, "core.schema.invalid_schema", "Schema definition must be a table.", path)
    return
  end

  local expected = schema.type or "any"

  if value == nil then
    if schema.nullable == true then
      return
    end
    add(diagnostics, "core.schema.value_missing", "Value is nil and nullable is not enabled.", path)
    return
  end

  if not type_matches(value, expected) then
    add(diagnostics, "core.schema.type_mismatch", "Value type does not match schema type " .. expected .. ".", path)
    return
  end

  validate_enum(value, schema, diagnostics, path)
  validate_number_rules(value, schema, diagnostics, path)
  validate_string_rules(value, schema, diagnostics, path)

  if expected == "array" then
    validate_array(value, schema, diagnostics, path, depth, max_depth, validate_value)
  elseif expected == "object" then
    validate_object(value, schema, diagnostics, path, depth, max_depth, validate_value)
  end
end

function M.validate(value, schema, options)
  local diagnostics = {}
  local max_depth = 16
  local path = "value"

  if type(options) == "table" then
    if is_integer(options.max_depth) and options.max_depth > 0 then
      max_depth = options.max_depth
    end
    if type(options.path) == "string" and options.path ~= "" then
      path = options.path
    end
  end

  validate_value(value, schema, path, diagnostics, 1, max_depth)
  return { ok = not has_error(diagnostics), diagnostics = diagnostics }
end

local function validate_json_value(value, diagnostics, path, depth, max_depth)
  if depth > max_depth then
    add(diagnostics, "core.schema.json_max_depth_exceeded", "JSON-serializable validation exceeded configured maximum depth.", path)
    return
  end

  local value_type = type(value)
  if value_type == "nil" or value_type == "string" or value_type == "number" or value_type == "boolean" then
    return
  end

  if value_type ~= "table" then
    add(diagnostics, "core.schema.json_unsupported_type", "Value contains a type that cannot be represented as JSON data.", path)
    return
  end

  if M.is_array(value) then
    for index = 1, #value do
      validate_json_value(value[index], diagnostics, path_join(path, index), depth + 1, max_depth)
    end
    return
  end

  local keys = sorted_keys(value)
  for index = 1, #keys do
    local key = keys[index]
    local item = value[key]
    if type(key) ~= "string" then
      add(diagnostics, "core.schema.json_non_string_key", "Object-like table contains a non-string key.", path_join(path, key))
    else
      validate_json_value(item, diagnostics, path_join(path, key), depth + 1, max_depth)
    end
  end
end

function M.validate_json_serializable(value, options)
  local diagnostics = {}
  local max_depth = 16
  local path = "value"

  if type(options) == "table" then
    if is_integer(options.max_depth) and options.max_depth > 0 then
      max_depth = options.max_depth
    end
    if type(options.path) == "string" and options.path ~= "" then
      path = options.path
    end
  end

  validate_json_value(value, diagnostics, path, 1, max_depth)
  return { ok = not has_error(diagnostics), diagnostics = diagnostics }
end

function M.validate_config(config)
  local config_schema = M.manifest.config_schema
  local result = M.validate(config or {}, config_schema, { path = "config", max_depth = 4 })
  return result.ok, result.diagnostics
end

return M
