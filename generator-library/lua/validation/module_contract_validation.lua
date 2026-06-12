local M = {}

M.manifest = {
  id = "validation/module_contract_validation/v1",
  version = "0.1.0",
  category = "validation",
  title = "Module contract validation",
  purpose = "Validate generator module metadata, manifest-like module entries, capabilities and dependencies without dynamic module access.",
  capabilities = {
    "validation.module_contract.validate",
    "validation.capability_dependencies",
    "validation.manifest_metadata"
  },
  input_schema = {
    type = "object",
    required = { "modules" }
  },
  output_schema = {
    type = "object",
    fields = { "summary", "module_ids", "capability_ids" }
  },
  config_schema = {
    type = "object",
    fields = { "capability_dependencies" }
  },
  deterministic = true,
  runtime_targets = { "editor", "validation", "simulation", "unity_ir", "codegen_ir" },
  unsafe_features = {}
}

local function diagnostic(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function is_array(value)
  if type(value) ~= "table" then
    return false
  end
  local count = 0
  for key, _ in pairs(value) do
    if type(key) ~= "number" or key < 1 or key % 1 ~= 0 then
      return false
    end
    count = count + 1
  end
  for index = 1, count do
    if value[index] == nil then
      return false
    end
  end
  return true
end

local function is_module_id(value)
  if type(value) ~= "string" or value == "" then
    return false
  end
  if value:sub(1, 1) == "/" or value:sub(-1) == "/" or value:find("//", 1, true) then
    return false
  end
  return value:match("^[a-z0-9_][a-z0-9_/%-]*/v[0-9]+$") ~= nil
end

local function is_capability_id(value)
  if type(value) ~= "string" or value == "" then
    return false
  end
  if value:sub(1, 1) == "." or value:sub(-1) == "." or value:find("..", 1, true) then
    return false
  end
  return value:match("^[a-z0-9_][a-z0-9_%.%-]*$") ~= nil
end

local function check_array_field(module, field_name, diagnostics, target)
  if not is_array(module[field_name]) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.invalid_" .. field_name, field_name .. " must be an array.", target .. "." .. field_name)
    return false
  end
  return true
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.config_not_table", "Config must be a table.", "config")
  end
  if type(config) == "table" and config.capability_dependencies ~= nil and type(config.capability_dependencies) ~= "table" then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.invalid_capability_dependencies", "capability_dependencies must be a table.", "config.capability_dependencies")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local config = type(ctx) == "table" and type(ctx.config) == "table" and ctx.config or {}
  local ok_config, config_diagnostics = M.validate_config(config)
  for i = 1, #config_diagnostics do
    diagnostics[#diagnostics + 1] = config_diagnostics[i]
  end

  if type(input) ~= "table" or not is_array(input.modules) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.input_invalid", "Input must contain modules array.", "input.modules")
    return { ok = false, data = { summary = { checked = 0 }, module_ids = {}, capability_ids = {} }, diagnostics = diagnostics, artifacts = {} }
  end

  local module_index = {}
  local module_ids = {}
  local capability_index = {}
  local capability_ids = {}

  for i = 1, #input.modules do
    local module = input.modules[i]
    local target = "modules[" .. tostring(i) .. "]"
    if type(module) ~= "table" then
      diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.module_not_table", "Module entry must be a table.", target)
    else
      if not is_module_id(module.id) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.invalid_module_id", "Module id must use lowercase slash notation with version suffix.", target .. ".id")
      elseif module_index[module.id] then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.duplicate_module_id", "Module id is duplicated.", module.id)
      else
        module_index[module.id] = module
        module_ids[#module_ids + 1] = module.id
      end

      if type(module.path) ~= "string" or module.path == "" then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.missing_path", "Module path is required.", target .. ".path")
      end
      if type(module.category) ~= "string" or module.category == "" then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.missing_category", "Module category is required.", target .. ".category")
      end
      if type(module.deterministic) ~= "boolean" then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.invalid_deterministic", "deterministic must be boolean.", target .. ".deterministic")
      end

      local capabilities_ok = check_array_field(module, "capabilities", diagnostics, target)
      check_array_field(module, "depends_on", diagnostics, target)
      check_array_field(module, "runtime_targets", diagnostics, target)
      check_array_field(module, "supported_turn_modes", diagnostics, target)
      check_array_field(module, "supported_combat_modes", diagnostics, target)
      check_array_field(module, "unsafe_features", diagnostics, target)

      if capabilities_ok then
        local local_capabilities = {}
        for ci = 1, #module.capabilities do
          local capability = module.capabilities[ci]
          if not is_capability_id(capability) then
            diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.invalid_capability_id", "Capability id is invalid.", target .. ".capabilities[" .. tostring(ci) .. "]")
          else
            if local_capabilities[capability] then
              diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.duplicate_local_capability", "Capability is duplicated inside one module.", target .. ".capabilities[" .. tostring(ci) .. "]")
            end
            if capability_index[capability] == nil then
              capability_ids[#capability_ids + 1] = capability
              capability_index[capability] = {}
            end
            capability_index[capability][#capability_index[capability] + 1] = module.id or target
            local_capabilities[capability] = true
          end
        end
      end

      if is_array(module.unsafe_features) and #module.unsafe_features > 0 then
        diagnostics[#diagnostics + 1] = diagnostic("warning", "validation.module_contract.unsafe_features_declared", "Module declares unsafe features.", target .. ".unsafe_features")
      end
    end
  end

  for i = 1, #input.modules do
    local module = input.modules[i]
    local target = "modules[" .. tostring(i) .. "]"
    if type(module) == "table" and is_array(module.depends_on) then
      for di = 1, #module.depends_on do
        local dependency = module.depends_on[di]
        if not is_module_id(dependency) then
          diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.invalid_dependency_id", "Dependency id is invalid.", target .. ".depends_on[" .. tostring(di) .. "]")
        elseif not module_index[dependency] and not (input.external_modules and input.external_modules[dependency] == true) then
          diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.missing_dependency", "Module dependency is missing from provided metadata.", target .. ".depends_on[" .. tostring(di) .. "]")
        end
      end
    end
  end

  local capability_dependencies = config.capability_dependencies or input.capability_dependencies or {}
  if type(capability_dependencies) == "table" then
    for capability, dependencies in pairs(capability_dependencies) do
      if is_capability_id(capability) and is_array(dependencies) then
        local providers = capability_index[capability]
        if providers ~= nil then
          for di = 1, #dependencies do
            local required_capability = dependencies[di]
            if not is_capability_id(required_capability) then
              diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.invalid_capability_dependency", "Capability dependency id is invalid.", "capability_dependencies." .. capability)
            elseif capability_index[required_capability] == nil and not (input.external_capabilities and input.external_capabilities[required_capability] == true) then
              diagnostics[#diagnostics + 1] = diagnostic("error", "validation.module_contract.missing_capability_dependency", "Provided capability is missing a required capability dependency.", "capability_dependencies." .. capability)
            end
          end
        end
      end
    end
  end

  local has_errors = false
  for i = 1, #diagnostics do
    if diagnostics[i].severity == "error" then
      has_errors = true
      break
    end
  end

  return {
    ok = ok_config and not has_errors,
    data = {
      summary = {
        checked = #input.modules,
        unique_modules = #module_ids,
        unique_capabilities = #capability_ids,
        diagnostic_count = #diagnostics
      },
      module_ids = module_ids,
      capability_ids = capability_ids
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
