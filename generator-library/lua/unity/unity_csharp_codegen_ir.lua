local M = {}

M.manifest = {
  id = "unity/unity_csharp_codegen_ir/v1",
  version = "0.1.0",
  category = "unity",
  title = "Unity CSharp Codegen IR",
  purpose = "Define schema-validated CSharp codegen metadata only, without producing source text or compiling anything.",
  capabilities = { "unity.csharp_codegen_ir.generate", "unity.codegen_units.validate", "unity.compile_metadata.plan" },
  input_schema = { type = "table", required = { "units" } },
  output_schema = { type = "table", required = { "codegen_units" } },
  config_schema = { type = "table" },
  deterministic = true,
  runtime_targets = { "editor", "unity2d", "unity3d", "unity_ir", "codegen_ir" },
  unsafe_features = {}
}

local function diagnostic(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function is_array(value)
  if type(value) ~= "table" then return false end
  local count = 0
  for key, _ in pairs(value) do
    if type(key) ~= "number" or key < 1 or key % 1 ~= 0 then return false end
    if key > count then count = key end
  end
  for i = 1, count do if value[i] == nil then return false end end
  return true
end

local function is_slash_id(value)
  return type(value) == "string" and value:match("^[a-z0-9][a-z0-9_%-]*(/[a-z0-9][a-z0-9_%-]*)*$") ~= nil
end

local function is_identifier(value)
  return type(value) == "string" and value:match("^[A-Za-z_][A-Za-z0-9_]*$") ~= nil
end

local function is_namespace(value)
  if type(value) ~= "string" or value == "" then return false end
  for part in value:gmatch("[^%.]+") do
    if not is_identifier(part) then return false end
  end
  return true
end

local unsafe_fields = {
  source_text = true,
  raw_source = true,
  method_body = true,
  command = true,
  shell_command = true,
  file_path = true,
  source_file = true
}

local valid_roles = {
  component = true,
  adapter = true,
  presenter = true,
  service = true,
  data_model = true,
  bootstrap = true,
  validator = true
}

local function validate_hooks(hooks, target, diagnostics)
  if hooks == nil then return end
  if not is_array(hooks) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.hooks_not_array", "hooks must be an array when provided.", target)
    return
  end
  for index, hook in ipairs(hooks) do
    local item_target = target .. "[" .. index .. "]"
    if type(hook) ~= "table" then
      diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.hook_not_table", "Hook descriptor must be a table.", item_target)
    else
      if not is_slash_id(hook.id) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.invalid_hook_id", "Hook id must be a lowercase slash id.", item_target .. ".id")
      end
      if hook.event_name ~= nil and not is_identifier(hook.event_name) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.invalid_event_name", "event_name must be an identifier when provided.", item_target .. ".event_name")
      end
      if hook.method_name ~= nil and not is_identifier(hook.method_name) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.invalid_method_name", "method_name must be an identifier when provided.", item_target .. ".method_name")
      end
      for field, _ in pairs(unsafe_fields) do
        if hook[field] ~= nil then
          diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.unsafe_hook_field", "Hook descriptor contains a forbidden executable/source field.", item_target .. "." .. field)
        end
      end
    end
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.config_not_table", "Codegen IR config must be a table.", "config")
    return false, diagnostics
  end
  if not is_array(config.units) or #config.units == 0 then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.missing_units", "units must be a non-empty array.", "units")
    return false, diagnostics
  end

  local known = {}
  for index, unit in ipairs(config.units) do
    local target = "units[" .. index .. "]"
    if type(unit) ~= "table" then
      diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.unit_not_table", "Unit must be a table.", target)
    else
      if not is_slash_id(unit.id) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.invalid_unit_id", "Unit id must be a lowercase slash id.", target .. ".id")
      elseif known[unit.id] then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.duplicate_unit", "Duplicate codegen unit id.", unit.id)
      end
      known[unit.id] = true
      if not valid_roles[unit.role or ""] then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.invalid_role", "Unit role is not supported.", target .. ".role")
      end
      if not is_namespace(unit.namespace) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.invalid_namespace", "namespace must be dot-separated identifiers.", target .. ".namespace")
      end
      if not is_identifier(unit.class_name) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.invalid_class_name", "class_name must be an identifier.", target .. ".class_name")
      end
      for field, _ in pairs(unsafe_fields) do
        if unit[field] ~= nil then
          diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.unsafe_unit_field", "Unit contains a forbidden executable/source field.", target .. "." .. field)
        end
      end
      validate_hooks(unit.hooks, target .. ".hooks", diagnostics)
    end
  end

  for index, unit in ipairs(config.units) do
    if type(unit) == "table" and is_array(unit.depends_on_units) then
      for dep_index, dep in ipairs(unit.depends_on_units) do
        if not known[dep] then
          diagnostics[#diagnostics + 1] = diagnostic("error", "unity.codegen.missing_dependency", "depends_on_units references an unknown unit id.", "units[" .. index .. "].depends_on_units[" .. dep_index .. "]")
        end
      end
    end
  end

  return #diagnostics == 0, diagnostics
end

local function copy_array(values)
  local result = {}
  if is_array(values) then for index, value in ipairs(values) do result[index] = value end end
  return result
end

function M.generate(input, ctx)
  local config = input and (input.config or input) or nil
  local ok, diagnostics = M.validate_config(config)
  if not ok then return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} } end

  local units = {}
  for index, unit in ipairs(config.units) do
    units[index] = {
      id = unit.id,
      role = unit.role,
      namespace = unit.namespace,
      class_name = unit.class_name,
      component_kind = unit.component_kind or "plain_metadata",
      descriptors = unit.descriptors or {},
      hooks = copy_array(unit.hooks),
      depends_on_units = copy_array(unit.depends_on_units),
      validation = unit.validation or { compile_expected = "not_run", smoke_expected = "not_run" },
      metadata = unit.metadata or {}
    }
  end

  return {
    ok = true,
    data = {
      codegen_ir_kind = "csharp_codegen_metadata_only",
      codegen_units = units,
      validation_metadata = config.validation_metadata or { compile_expected = "not_run", smoke_expected = "not_run" }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
