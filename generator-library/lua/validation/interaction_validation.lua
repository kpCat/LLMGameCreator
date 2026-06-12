local M = {}

M.manifest = {
  id = "validation/interaction_validation/v1",
  version = "0.1.0",
  category = "validation",
  title = "Interaction validation",
  purpose = "Validate interaction, targeting and dialogue bridge IR without executing interactions.",
  capabilities = {
    "validation.interaction.validate",
    "validation.interaction.targeting",
    "validation.interaction.references"
  },
  input_schema = {
    type = "object",
    required = { "interactions" }
  },
  output_schema = {
    type = "object",
    fields = { "summary", "interaction_ids" }
  },
  config_schema = {
    type = "object",
    fields = { "allowed_target_modes" }
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

local function is_id(value)
  if type(value) ~= "string" or value == "" then
    return false
  end
  if value:sub(1, 1) == "/" or value:sub(-1) == "/" then
    return false
  end
  if value:find("//", 1, true) then
    return false
  end
  return value:match("^[a-z0-9_][a-z0-9_/%-]*$") ~= nil
end

local function set_from_array(items)
  local set = {}
  if is_array(items) then
    for i = 1, #items do
      if type(items[i]) == "string" then
        set[items[i]] = true
      end
    end
  end
  return set
end

local function default_target_modes()
  return {
    facing_cell = true,
    same_cell = true,
    cardinal_adjacent = true,
    diagonal_adjacent = true,
    radius = true,
    explicit_entity = true,
    world_marker = true,
    manual_selection = true
  }
end

local function validate_reference(registry, field_name, value, diagnostics, target)
  if value == nil then
    return
  end
  if not is_id(value) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.interaction.invalid_" .. field_name, "Reference id is invalid.", target)
    return
  end
  local bucket = registry and registry[field_name]
  if type(bucket) == "table" and bucket[value] ~= true then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.interaction.missing_" .. field_name, "Reference target is missing from registry.", target)
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.interaction.config_not_table", "Config must be a table.", "config")
  end
  if type(config) == "table" and config.allowed_target_modes ~= nil and not is_array(config.allowed_target_modes) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.interaction.invalid_target_modes_config", "allowed_target_modes must be an array.", "config.allowed_target_modes")
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

  if type(input) ~= "table" or not is_array(input.interactions) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.interaction.input_invalid", "Input must contain interactions array.", "input.interactions")
    return { ok = false, data = { summary = { checked = 0 }, interaction_ids = {} }, diagnostics = diagnostics, artifacts = {} }
  end

  local allowed_modes = next(set_from_array(config.allowed_target_modes)) ~= nil and set_from_array(config.allowed_target_modes) or default_target_modes()
  local registry = input.registry or {}
  local seen = {}
  local interaction_ids = {}

  for i = 1, #input.interactions do
    local interaction = input.interactions[i]
    local target = "interactions[" .. tostring(i) .. "]"
    if type(interaction) ~= "table" then
      diagnostics[#diagnostics + 1] = diagnostic("error", "validation.interaction.not_table", "Interaction must be a table.", target)
    else
      if not is_id(interaction.id) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.interaction.invalid_id", "Interaction id is invalid.", target .. ".id")
      elseif seen[interaction.id] then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.interaction.duplicate_id", "Interaction id is duplicated.", interaction.id)
      else
        seen[interaction.id] = true
        interaction_ids[#interaction_ids + 1] = interaction.id
      end

      if type(interaction.kind) ~= "string" or interaction.kind == "" then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.interaction.missing_kind", "Interaction kind is required.", target .. ".kind")
      end

      local target_ir = interaction.target
      if target_ir == nil then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.interaction.missing_target", "Interaction has no target requirement.", target .. ".target")
      elseif type(target_ir) ~= "table" then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.interaction.target_not_table", "Interaction target must be a table.", target .. ".target")
      else
        if type(target_ir.mode) ~= "string" or not allowed_modes[target_ir.mode] then
          diagnostics[#diagnostics + 1] = diagnostic("error", "validation.interaction.invalid_target_mode", "Target mode is invalid.", target .. ".target.mode")
        end
        if target_ir.mode == "radius" and (type(target_ir.radius) ~= "number" or target_ir.radius < 0) then
          diagnostics[#diagnostics + 1] = diagnostic("error", "validation.interaction.invalid_radius", "Radius target mode requires non-negative radius.", target .. ".target.radius")
        end
        if target_ir.mode == "explicit_entity" and target_ir.entity_id == nil then
          diagnostics[#diagnostics + 1] = diagnostic("error", "validation.interaction.missing_explicit_entity", "explicit_entity target mode requires entity_id.", target .. ".target.entity_id")
        end
        validate_reference(registry, "entity_id", target_ir.entity_id, diagnostics, target .. ".target.entity_id")
      end

      validate_reference(registry, "entity_id", interaction.entity_id, diagnostics, target .. ".entity_id")
      validate_reference(registry, "dialogue_id", interaction.dialogue_id, diagnostics, target .. ".dialogue_id")
      validate_reference(registry, "quest_id", interaction.quest_id, diagnostics, target .. ".quest_id")

      if interaction.kind == "talk" and interaction.dialogue_id == nil then
        diagnostics[#diagnostics + 1] = diagnostic("warning", "validation.interaction.talk_without_dialogue", "Talk interaction has no dialogue reference.", target .. ".dialogue_id")
      end
      if interaction.kind == "pickup" and interaction.item_id ~= nil then
        validate_reference(registry, "item_id", interaction.item_id, diagnostics, target .. ".item_id")
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
        checked = #input.interactions,
        unique_interactions = #interaction_ids,
        diagnostic_count = #diagnostics
      },
      interaction_ids = interaction_ids
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
