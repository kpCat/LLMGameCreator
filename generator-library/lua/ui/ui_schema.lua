local M = {}

M.manifest = {
  id = "ui/ui_schema/v1",
  version = "0.1.0",
  category = "ui",
  title = "UI schema IR helpers",
  purpose = "Common deterministic UI IR schema helpers and validation utilities for panel, element, binding and action metadata.",
  capabilities = {
    "ui.schema.validate",
    "ui.element.validate",
    "ui.binding.reference",
    "ui.action.reference"
  },
  input_schema = {
    type = "object",
    fields = {
      panels = "optional array of panel IR tables",
      elements = "optional array of element IR tables",
      bindings = "optional dictionary of binding references",
      actions = "optional dictionary of action references"
    }
  },
  output_schema = {
    type = "object",
    fields = {
      schema_version = "string",
      panels = "array",
      elements = "array",
      diagnostics = "array"
    }
  },
  config_schema = {
    type = "object",
    fields = {
      strict_references = "optional boolean",
      min_width = "optional positive integer",
      min_height = "optional positive integer"
    }
  },
  deterministic = true,
  runtime_targets = { "editor", "unity2d", "unity3d", "unity_ui_ir", "codegen_ir" },
  unsafe_features = {}
}

local VALID_ANCHORS = {
  top_left = true,
  top = true,
  top_right = true,
  left = true,
  center = true,
  right = true,
  bottom_left = true,
  bottom = true,
  bottom_right = true,
  stretch = true,
  fill = true
}

local VALID_VISIBILITY = {
  always = true,
  never = true,
  when = true,
  unless = true
}

local function diagnostic(severity, code, message, target)
  return {
    severity = severity,
    code = code,
    message = message,
    target = target
  }
end

local function add_diag(diagnostics, severity, code, message, target)
  diagnostics[#diagnostics + 1] = diagnostic(severity, code, message, target)
end

local function is_table(value)
  return type(value) == "table"
end

local function is_non_empty_string(value)
  return type(value) == "string" and value ~= ""
end

local function is_positive_number(value)
  return type(value) == "number" and value > 0
end

local function is_ui_id(value)
  return is_non_empty_string(value) and value:match("^[a-z][a-z0-9_]*(/[a-z][a-z0-9_]*)*$") ~= nil
end

local function is_reference_id(value)
  if not is_non_empty_string(value) then
    return false
  end
  if value:match("^[a-z][a-z0-9_]*(/[a-z][a-z0-9_]*)*$") ~= nil then
    return true
  end
  return value:match("^[a-z][a-z0-9_]*([.][a-z][a-z0-9_]*)*$") ~= nil
end

local function copy_array(source)
  local result = {}
  if is_table(source) then
    for index = 1, #source do
      result[index] = source[index]
    end
  end
  return result
end

local function validate_dimensions(size, diagnostics, target)
  if size == nil then
    return
  end
  if not is_table(size) then
    add_diag(diagnostics, "error", "ui.size.invalid", "Size metadata must be a table.", target)
    return
  end
  if size.width ~= nil and not is_positive_number(size.width) then
    add_diag(diagnostics, "error", "ui.size.invalid_width", "Width must be a positive number when provided.", target .. ".size.width")
  end
  if size.height ~= nil and not is_positive_number(size.height) then
    add_diag(diagnostics, "error", "ui.size.invalid_height", "Height must be a positive number when provided.", target .. ".size.height")
  end
end

local function validate_anchor(anchor, diagnostics, target)
  if anchor == nil then
    return
  end
  if not VALID_ANCHORS[anchor] then
    add_diag(diagnostics, "error", "ui.anchor.invalid", "Anchor is not supported by the common UI IR schema.", target)
  end
end

local function validate_reference_map(map, diagnostics, target, code_prefix)
  if map == nil then
    return
  end
  if not is_table(map) then
    add_diag(diagnostics, "error", code_prefix .. ".invalid_map", "Reference collection must be a table.", target)
    return
  end
  for key, value in pairs(map) do
    if type(key) ~= "string" or key == "" then
      add_diag(diagnostics, "error", code_prefix .. ".invalid_key", "Reference keys must be non-empty strings.", target)
    end
    if not is_reference_id(value) then
      add_diag(diagnostics, "error", code_prefix .. ".invalid_reference", "Reference value must be a lowercase dot or slash id.", target .. "." .. tostring(key))
    end
  end
end

local function validate_visibility(visibility, diagnostics, target)
  if visibility == nil then
    return
  end
  if type(visibility) == "string" then
    if not VALID_VISIBILITY[visibility] then
      add_diag(diagnostics, "error", "ui.visibility.invalid", "Visibility string is not supported.", target)
    end
    return
  end
  if not is_table(visibility) then
    add_diag(diagnostics, "error", "ui.visibility.invalid", "Visibility metadata must be a string or table.", target)
    return
  end
  local mode = visibility.mode or "when"
  if not VALID_VISIBILITY[mode] then
    add_diag(diagnostics, "error", "ui.visibility.invalid_mode", "Visibility mode is not supported.", target .. ".mode")
  end
  if visibility.binding ~= nil and not is_reference_id(visibility.binding) then
    add_diag(diagnostics, "error", "ui.visibility.invalid_binding", "Visibility binding must be a lowercase dot or slash id.", target .. ".binding")
  end
end

function M.is_ui_id(value)
  return is_ui_id(value)
end

function M.is_reference_id(value)
  return is_reference_id(value)
end

function M.diagnostic(severity, code, message, target)
  return diagnostic(severity, code, message, target)
end

function M.validate_element(element, target)
  local diagnostics = {}
  local path = target or "element"
  if not is_table(element) then
    add_diag(diagnostics, "error", "ui.element.invalid", "UI element must be a table.", path)
    return false, diagnostics
  end
  if not is_ui_id(element.id) then
    add_diag(diagnostics, "error", "ui.element.invalid_id", "UI element id must use lowercase slash notation.", path .. ".id")
  end
  if not is_non_empty_string(element.kind) then
    add_diag(diagnostics, "error", "ui.element.invalid_kind", "UI element kind must be a non-empty string.", path .. ".kind")
  end
  validate_anchor(element.anchor, diagnostics, path .. ".anchor")
  validate_dimensions(element.size, diagnostics, path)
  validate_reference_map(element.bindings, diagnostics, path .. ".bindings", "ui.binding")
  validate_reference_map(element.actions, diagnostics, path .. ".actions", "ui.action")
  validate_visibility(element.visibility, diagnostics, path .. ".visibility")
  return #diagnostics == 0, diagnostics
end

function M.validate_panel(panel, target)
  local diagnostics = {}
  local path = target or "panel"
  if not is_table(panel) then
    add_diag(diagnostics, "error", "ui.panel.invalid", "UI panel must be a table.", path)
    return false, diagnostics
  end
  if not is_ui_id(panel.id) then
    add_diag(diagnostics, "error", "ui.panel.invalid_id", "UI panel id must use lowercase slash notation.", path .. ".id")
  end
  validate_anchor(panel.anchor, diagnostics, path .. ".anchor")
  validate_dimensions(panel.size, diagnostics, path)
  validate_visibility(panel.visibility, diagnostics, path .. ".visibility")
  if panel.elements ~= nil then
    if not is_table(panel.elements) then
      add_diag(diagnostics, "error", "ui.panel.invalid_elements", "Panel elements must be an array when provided.", path .. ".elements")
    else
      for index = 1, #panel.elements do
        local _, child_diags = M.validate_element(panel.elements[index], path .. ".elements[" .. tostring(index) .. "]")
        for diag_index = 1, #child_diags do
          diagnostics[#diagnostics + 1] = child_diags[diag_index]
        end
      end
    end
  end
  return #diagnostics == 0, diagnostics
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if not is_table(config) then
    add_diag(diagnostics, "error", "ui.config.invalid", "UI schema config must be a table when provided.", "config")
    return false, diagnostics
  end
  if config.min_width ~= nil and not is_positive_number(config.min_width) then
    add_diag(diagnostics, "error", "ui.config.invalid_min_width", "min_width must be a positive number.", "config.min_width")
  end
  if config.min_height ~= nil and not is_positive_number(config.min_height) then
    add_diag(diagnostics, "error", "ui.config.invalid_min_height", "min_height must be a positive number.", "config.min_height")
  end
  if config.strict_references ~= nil and type(config.strict_references) ~= "boolean" then
    add_diag(diagnostics, "error", "ui.config.invalid_strict_references", "strict_references must be boolean when provided.", "config.strict_references")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local source = input or {}
  local config = source.config or {}
  local diagnostics = {}
  local ok, config_diags = M.validate_config(config)
  for index = 1, #config_diags do
    diagnostics[#diagnostics + 1] = config_diags[index]
  end

  local panels = copy_array(source.panels)
  local elements = copy_array(source.elements)

  for index = 1, #panels do
    local _, panel_diags = M.validate_panel(panels[index], "panels[" .. tostring(index) .. "]")
    for diag_index = 1, #panel_diags do
      diagnostics[#diagnostics + 1] = panel_diags[diag_index]
    end
  end
  for index = 1, #elements do
    local _, element_diags = M.validate_element(elements[index], "elements[" .. tostring(index) .. "]")
    for diag_index = 1, #element_diags do
      diagnostics[#diagnostics + 1] = element_diags[diag_index]
    end
  end

  ok = ok and #diagnostics == 0
  return {
    ok = ok,
    data = {
      schema_version = "ui_ir/v1",
      element_id_format = "lowercase_slash_id",
      reference_id_format = "lowercase_dot_or_slash_id",
      allowed_anchors = {
        "top_left", "top", "top_right", "left", "center", "right", "bottom_left", "bottom", "bottom_right", "stretch", "fill"
      },
      allowed_visibility_modes = { "always", "never", "when", "unless" },
      panels = panels,
      elements = elements,
      bindings = source.bindings or {},
      actions = source.actions or {},
      metadata = {
        deterministic = true,
        renderer_agnostic = true,
        host_adapter_target = source.host_adapter_target or "future_adapter"
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
