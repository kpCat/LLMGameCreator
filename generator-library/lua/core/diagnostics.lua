local M = {}

M.manifest = {
  id = "core/diagnostics/v1",
  version = "0.1.0",
  category = "core",
  title = "Diagnostics helpers",
  purpose = "Create and aggregate JSON-serializable diagnostics for generator modules.",
  capabilities = { "core.diagnostics.create", "core.diagnostics.aggregate" },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      strict_severity = { type = "boolean" }
    }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "codegen_ir" },
  unsafe_features = {}
}

M.severities = { "error", "warning", "info" }

local allowed_severity = {
  error = true,
  warning = true,
  info = true
}

local function is_non_empty_string(value)
  return type(value) == "string" and value ~= ""
end

function M.make(severity, code, message, target)
  local resolved_severity = severity
  if not allowed_severity[resolved_severity] then
    resolved_severity = "error"
  end

  local diagnostic = {
    severity = resolved_severity,
    code = is_non_empty_string(code) and code or "core.diagnostic",
    message = is_non_empty_string(message) and message or "Diagnostic message was not provided."
  }

  if target ~= nil then
    diagnostic.target = target
  end

  return diagnostic
end

function M.error(code, message, target)
  return M.make("error", code, message, target)
end

function M.warning(code, message, target)
  return M.make("warning", code, message, target)
end

function M.info(code, message, target)
  return M.make("info", code, message, target)
end

function M.list()
  return {}
end

function M.add(list, diagnostic)
  local resolved = type(list) == "table" and list or {}
  if type(diagnostic) == "table" then
    resolved[#resolved + 1] = diagnostic
  end
  return resolved
end

function M.add_error(list, code, message, target)
  return M.add(list, M.error(code, message, target))
end

function M.add_warning(list, code, message, target)
  return M.add(list, M.warning(code, message, target))
end

function M.add_info(list, code, message, target)
  return M.add(list, M.info(code, message, target))
end

function M.extend(list, diagnostics)
  local resolved = type(list) == "table" and list or {}
  if type(diagnostics) ~= "table" then
    return resolved
  end

  for index = 1, #diagnostics do
    if type(diagnostics[index]) == "table" then
      resolved[#resolved + 1] = diagnostics[index]
    end
  end

  return resolved
end

function M.has_errors(diagnostics)
  if type(diagnostics) ~= "table" then
    return false
  end

  for index = 1, #diagnostics do
    local diagnostic = diagnostics[index]
    if type(diagnostic) == "table" and diagnostic.severity == "error" then
      return true
    end
  end

  return false
end

function M.count_by_severity(diagnostics)
  local counts = { error = 0, warning = 0, info = 0 }
  if type(diagnostics) ~= "table" then
    return counts
  end

  for index = 1, #diagnostics do
    local severity = diagnostics[index] and diagnostics[index].severity
    if allowed_severity[severity] then
      counts[severity] = counts[severity] + 1
    end
  end

  return counts
end

function M.result(ok, data, diagnostics, artifacts)
  return {
    ok = ok == true,
    data = type(data) == "table" and data or {},
    diagnostics = type(diagnostics) == "table" and diagnostics or {},
    artifacts = type(artifacts) == "table" and artifacts or {}
  }
end

function M.ok(data, diagnostics, artifacts)
  return M.result(true, data, diagnostics, artifacts)
end

function M.fail(diagnostics, data, artifacts)
  return M.result(false, data, diagnostics, artifacts)
end

function M.validate_config(config)
  local diagnostics = {}

  if config == nil then
    return true, diagnostics
  end

  if type(config) ~= "table" then
    M.add_error(diagnostics, "core.diagnostics.config_not_table", "Diagnostics config must be a table.", "config")
    return false, diagnostics
  end

  if config.strict_severity ~= nil and type(config.strict_severity) ~= "boolean" then
    M.add_error(diagnostics, "core.diagnostics.strict_severity_not_boolean", "strict_severity must be a boolean when provided.", "config.strict_severity")
  end

  return not M.has_errors(diagnostics), diagnostics
end

return M
