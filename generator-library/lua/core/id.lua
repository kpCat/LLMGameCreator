local M = {}

M.manifest = {
  id = "core/id/v1",
  version = "0.1.0",
  category = "core",
  title = "Slash ID helpers",
  purpose = "Validate and build lowercase slash identifiers used by game generator data and manifests.",
  capabilities = { "core.id.validate", "core.id.build", "core.id.split" },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      max_length = { type = "integer", min = 1, max = 256 }
    }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "simulation", "codegen_ir" },
  unsafe_features = {}
}

local DEFAULT_MAX_LENGTH = 128

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

local function allowed_segment(segment)
  return type(segment) == "string" and string.match(segment, "^[a-z0-9][a-z0-9_%-]*$") ~= nil
end

local function split_segments(value)
  local segments = {}
  if type(value) ~= "string" then
    return segments
  end
  for segment in string.gmatch(value, "[^/]+") do
    segments[#segments + 1] = segment
  end
  return segments
end

local function max_length_from_options(options)
  if type(options) == "table" and is_integer(options.max_length) and options.max_length > 0 then
    return options.max_length
  end
  return DEFAULT_MAX_LENGTH
end

function M.is_valid(value, options)
  local ok, diagnostics = M.validate(value, options)
  return ok, diagnostics
end

function M.validate(value, options)
  local diagnostics = {}
  local max_length = max_length_from_options(options)

  if type(value) ~= "string" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.id.not_string", "ID must be a string.", "id")
    return false, diagnostics
  end

  if value == "" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.id.empty", "ID must not be empty.", "id")
    return false, diagnostics
  end

  if #value > max_length then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.id.too_long", "ID is longer than the configured maximum length.", "id")
  end

  if string.sub(value, 1, 1) == "/" or string.sub(value, #value, #value) == "/" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.id.edge_slash", "ID must not start or end with slash.", "id")
  end

  if string.find(value, "//", 1, true) ~= nil then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.id.empty_segment", "ID must not contain empty slash segments.", "id")
  end

  if string.lower(value) ~= value then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.id.not_lowercase", "ID must be lowercase.", "id")
  end

  local segments = split_segments(value)
  if #segments == 0 then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.id.no_segments", "ID must contain at least one segment.", "id")
  end

  for index = 1, #segments do
    if not allowed_segment(segments[index]) then
      diagnostics[#diagnostics + 1] = make_diagnostic("core.id.invalid_segment", "ID segment must start with lowercase letter or digit and contain only lowercase letters, digits, underscore, or hyphen.", "id." .. tostring(index))
    end
  end

  return diagnostics[1] == nil, diagnostics
end

function M.split(value)
  local ok, diagnostics = M.validate(value)
  if not ok then
    return { ok = false, data = { segments = {} }, diagnostics = diagnostics, artifacts = {} }
  end
  return { ok = true, data = { segments = split_segments(value) }, diagnostics = {}, artifacts = {} }
end

function M.join(segments, options)
  local diagnostics = {}
  if not is_array(segments) or #segments == 0 then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.id.segments_not_array", "segments must be a non-empty array table.", "segments")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local parts = {}
  for index = 1, #segments do
    local segment = segments[index]
    if not allowed_segment(segment) then
      diagnostics[#diagnostics + 1] = make_diagnostic("core.id.invalid_join_segment", "Each segment must already be lowercase and slash-free.", "segments." .. tostring(index))
    else
      parts[#parts + 1] = segment
    end
  end

  if diagnostics[1] ~= nil then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local value = table.concat(parts, "/")
  local ok, validation_diagnostics = M.validate(value, options)
  if not ok then
    return { ok = false, data = { id = value }, diagnostics = validation_diagnostics, artifacts = {} }
  end

  return { ok = true, data = { id = value }, diagnostics = {}, artifacts = {} }
end

function M.with_suffix(base_id, suffix, options)
  local split_result = M.split(base_id)
  if split_result.ok ~= true then
    return split_result
  end
  local segments = split_result.data.segments
  segments[#segments + 1] = suffix
  return M.join(segments, options)
end

function M.normalize_hint(value)
  if type(value) ~= "string" then
    return ""
  end

  local lowered = string.lower(value)
  local result = {}
  local previous_was_separator = false

  for index = 1, #lowered do
    local char = string.sub(lowered, index, index)
    local allowed = string.match(char, "[a-z0-9]") ~= nil or char == "_" or char == "-" or char == "/"
    if allowed then
      if char == "/" then
        if not previous_was_separator and #result > 0 then
          result[#result + 1] = char
          previous_was_separator = true
        end
      else
        result[#result + 1] = char
        previous_was_separator = false
      end
    elseif not previous_was_separator and #result > 0 then
      result[#result + 1] = "-"
      previous_was_separator = true
    end
  end

  local text = table.concat(result, "")
  while string.sub(text, #text, #text) == "/" or string.sub(text, #text, #text) == "-" do
    text = string.sub(text, 1, #text - 1)
  end
  return text
end

function M.validate_config(config)
  local diagnostics = {}

  if config == nil then
    return true, diagnostics
  end

  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.id.config_not_table", "ID config must be a table.", "config")
    return false, diagnostics
  end

  if config.max_length ~= nil and not is_integer(config.max_length) then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.id.max_length_not_integer", "max_length must be an integer when provided.", "config.max_length")
  end

  if is_integer(config.max_length) and (config.max_length < 1 or config.max_length > 256) then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.id.max_length_out_of_range", "max_length must be between 1 and 256.", "config.max_length")
  end

  return diagnostics[1] == nil, diagnostics
end

return M
