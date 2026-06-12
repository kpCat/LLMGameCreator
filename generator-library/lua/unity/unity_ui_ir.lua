local M = {}

M.manifest = {
  id = "unity/unity_ui_ir/v1",
  version = "0.1.0",
  category = "unity",
  title = "Unity UI Adapter IR",
  purpose = "Bridge renderer-agnostic UI IR into Unity-facing adapter metadata without creating UI assets.",
  capabilities = { "unity.ui_ir.generate", "unity.ui_bindings.plan", "unity.ui_actions.plan" },
  input_schema = { type = "table", required = { "documents" } },
  output_schema = { type = "table", required = { "ui_documents" } },
  config_schema = { type = "table" },
  deterministic = true,
  runtime_targets = { "editor", "unity2d", "unity3d", "unity_ui_ir", "codegen_ir" },
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

local function validate_ref_array(values, target, code, diagnostics)
  if values == nil then return end
  if not is_array(values) then
    diagnostics[#diagnostics + 1] = diagnostic("error", code .. ".not_array", "Expected an array of reference ids.", target)
    return
  end
  local seen = {}
  for index, value in ipairs(values) do
    if not is_slash_id(value) then
      diagnostics[#diagnostics + 1] = diagnostic("error", code .. ".invalid_ref", "Reference must be a lowercase slash id.", target .. "[" .. index .. "]")
    elseif seen[value] then
      diagnostics[#diagnostics + 1] = diagnostic("warning", code .. ".duplicate_ref", "Duplicate reference id.", value)
    end
    seen[value] = true
  end
end

local function validate_bindings(bindings, target, diagnostics)
  if bindings == nil then return end
  if not is_array(bindings) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.ui_ir.bindings_not_array", "bindings must be an array when provided.", target)
    return
  end
  local seen = {}
  for index, binding in ipairs(bindings) do
    local item_target = target .. "[" .. index .. "]"
    if type(binding) ~= "table" then
      diagnostics[#diagnostics + 1] = diagnostic("error", "unity.ui_ir.binding_not_table", "Binding must be a table.", item_target)
    else
      if not is_slash_id(binding.id) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.ui_ir.invalid_binding_id", "Binding id must be a lowercase slash id.", item_target .. ".id")
      elseif seen[binding.id] then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.ui_ir.duplicate_binding", "Duplicate binding id.", binding.id)
      end
      seen[binding.id] = true
      if not is_slash_id(binding.source_ref) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.ui_ir.invalid_binding_source", "source_ref must be a lowercase slash id.", item_target .. ".source_ref")
      end
    end
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.ui_ir.config_not_table", "Unity UI IR config must be a table.", "config")
    return false, diagnostics
  end
  if not is_array(config.documents) or #config.documents == 0 then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.ui_ir.missing_documents", "documents must be a non-empty array.", "documents")
    return false, diagnostics
  end

  local seen_documents = {}
  for index, document in ipairs(config.documents) do
    local target = "documents[" .. index .. "]"
    if type(document) ~= "table" then
      diagnostics[#diagnostics + 1] = diagnostic("error", "unity.ui_ir.document_not_table", "Document must be a table.", target)
    else
      if not is_slash_id(document.id) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.ui_ir.invalid_document_id", "Document id must be a lowercase slash id.", target .. ".id")
      elseif seen_documents[document.id] then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.ui_ir.duplicate_document", "Duplicate document id.", document.id)
      end
      seen_documents[document.id] = true
      if not is_slash_id(document.source_ui_ref) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.ui_ir.invalid_ui_ref", "source_ui_ref must be a lowercase slash id.", target .. ".source_ui_ref")
      end
      validate_ref_array(document.panel_refs, target .. ".panel_refs", "unity.ui_ir.panel_refs", diagnostics)
      validate_ref_array(document.action_refs, target .. ".action_refs", "unity.ui_ir.action_refs", diagnostics)
      validate_bindings(document.bindings, target .. ".bindings", diagnostics)
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

  local documents = {}
  for index, document in ipairs(config.documents) do
    documents[index] = {
      id = document.id,
      kind = "unity_ui_adapter_ir",
      source_ui_ref = document.source_ui_ref,
      canvas = document.canvas or { render_mode = "screen_space_overlay" },
      screen_regions = document.screen_regions or {},
      panel_refs = copy_array(document.panel_refs),
      bindings = copy_array(document.bindings),
      action_refs = copy_array(document.action_refs),
      metadata = document.metadata or {}
    }
  end

  return { ok = true, data = { ui_documents = documents }, diagnostics = diagnostics, artifacts = {} }
end

return M
