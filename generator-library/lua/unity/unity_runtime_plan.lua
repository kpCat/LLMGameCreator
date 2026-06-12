local M = {}

M.manifest = {
  id = "unity/unity_runtime_plan/v1",
  version = "0.1.0",
  category = "unity",
  title = "Unity Runtime Plan IR",
  purpose = "Generate declarative Unity-facing runtime plan metadata without integrating or executing Unity.",
  capabilities = { "unity.runtime_plan.generate", "unity.runtime_plan.validate", "unity.adapter_capabilities.plan" },
  input_schema = { type = "table", required = { "target_runtime_id", "scene_refs" } },
  output_schema = { type = "table", required = { "runtime_plan" } },
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
  for i = 1, count do
    if value[i] == nil then return false end
  end
  return true
end

local function is_slash_id(value)
  return type(value) == "string" and value:match("^[a-z0-9][a-z0-9_%-]*(/[a-z0-9][a-z0-9_%-]*)*$") ~= nil
end

local function contains(list, value)
  for _, item in ipairs(list) do
    if item == value then return true end
  end
  return false
end

local valid_loop_modes = { "realtime", "turn_based", "mixed", "paused_planning" }
local valid_input_modes = { "keyboard_mouse", "gamepad", "touch", "hybrid", "ai_driven" }

function M.validate_config(config)
  local diagnostics = {}
  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.runtime_plan.config_not_table", "Runtime plan config must be a table.", "config")
    return false, diagnostics
  end

  if not is_slash_id(config.target_runtime_id) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.runtime_plan.invalid_target_id", "target_runtime_id must be a lowercase slash id.", "target_runtime_id")
  end

  if not is_array(config.scene_refs) or #config.scene_refs == 0 then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.runtime_plan.missing_scene_refs", "scene_refs must be a non-empty array of scene ids.", "scene_refs")
  else
    local seen = {}
    for index, scene_id in ipairs(config.scene_refs) do
      if not is_slash_id(scene_id) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.runtime_plan.invalid_scene_ref", "Scene reference must be a lowercase slash id.", "scene_refs[" .. index .. "]")
      elseif seen[scene_id] then
        diagnostics[#diagnostics + 1] = diagnostic("warning", "unity.runtime_plan.duplicate_scene_ref", "Duplicate scene reference.", scene_id)
      end
      seen[scene_id] = true
    end
  end

  local mode = config.game_loop_mode or "mixed"
  if not contains(valid_loop_modes, mode) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.runtime_plan.invalid_loop_mode", "game_loop_mode is not supported.", "game_loop_mode")
  end

  local input_mode = config.input_mode or "keyboard_mouse"
  if not contains(valid_input_modes, input_mode) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.runtime_plan.invalid_input_mode", "input_mode is not supported.", "input_mode")
  end

  if config.required_adapter_capabilities ~= nil and not is_array(config.required_adapter_capabilities) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.runtime_plan.invalid_adapter_capabilities", "required_adapter_capabilities must be an array when provided.", "required_adapter_capabilities")
  end

  return #diagnostics == 0, diagnostics
end

local function copy_array(values)
  local result = {}
  if is_array(values) then
    for index, value in ipairs(values) do result[index] = value end
  end
  return result
end

function M.generate(input, ctx)
  local config = input and (input.config or input) or nil
  local ok, diagnostics = M.validate_config(config)
  if not ok then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local plan = {
    id = config.id or (config.target_runtime_id .. "/plan"),
    kind = "unity_runtime_plan_ir",
    target_runtime_id = config.target_runtime_id,
    scenes = copy_array(config.scene_refs),
    runtime_features = config.runtime_features or {},
    required_adapter_capabilities = copy_array(config.required_adapter_capabilities),
    game_loop = {
      mode = config.game_loop_mode or "mixed",
      fixed_tick = config.fixed_tick or { enabled = false }
    },
    input = {
      mode = config.input_mode or "keyboard_mouse",
      action_map_ref = config.action_map_ref
    },
    persistence_requirements = config.persistence_requirements or { profile_slots = 1, autosave = false },
    validation = {
      compile = {
        expected_status = "not_run",
        checks = copy_array(config.compile_checks)
      },
      smoke = {
        expected_status = "not_run",
        scene_refs = copy_array(config.scene_refs),
        checks = copy_array(config.smoke_checks)
      }
    },
    metadata = config.metadata or {}
  }

  return { ok = true, data = { runtime_plan = plan }, diagnostics = diagnostics, artifacts = {} }
end

return M
