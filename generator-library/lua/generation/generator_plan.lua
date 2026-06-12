local M = {}

M.manifest = {
  id = "generation/generator_plan/v1",
  version = "0.1.0",
  category = "generation",
  title = "Generator plan model",
  purpose = "Validate and normalize ordered generator plan steps before an external host executes trusted modules.",
  capabilities = {
    "generation.generator_plan.validate",
    "generation.generator_plan.normalize",
    "generation.generator_plan.execution_order",
    "generation.generator_plan.compatibility_check"
  },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      require_declared_outputs = { type = "boolean" },
      require_dependencies_before_use = { type = "boolean" }
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

local DEFAULT_RUNTIME_TARGET = "debug"
local DEFAULT_TURN_MODE = "realtime"
local DEFAULT_COMBAT_MODE = "none"

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
  return count >= 1
end

local function is_module_id(value)
  if not is_slash_id(value) then
    return false
  end
  local count = 0
  for _ in string.gmatch(value, "[^/]+") do
    count = count + 1
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

local function contains(values, item)
  if not is_array(values) then
    return false
  end
  for index = 1, #values do
    if values[index] == item then
      return true
    end
  end
  return false
end

local function validate_string_array(values, code_prefix, target, diagnostics)
  if values == nil then
    return
  end
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
    end
  end
end

local function validate_id_array(values, code_prefix, target, diagnostics)
  if values == nil then
    return
  end
  if not is_array(values) then
    add_error(diagnostics, code_prefix .. ".not_array", "Value must be an array of step ids.", target)
    return
  end
  local seen = {}
  for index = 1, #values do
    local item = values[index]
    local item_target = target .. "." .. tostring(index)
    if not is_slash_id(item) then
      add_error(diagnostics, code_prefix .. ".invalid_id", "Step id must use lowercase slash notation.", item_target)
    elseif seen[item] == true then
      add_error(diagnostics, code_prefix .. ".duplicate", "Step id must be unique in this array.", item_target)
    else
      seen[item] = true
    end
  end
end

local function normalize_step(step, index)
  local source = type(step) == "table" and step or {}
  return {
    id = source.id or ("step/" .. tostring(index)),
    title = type(source.title) == "string" and source.title or "",
    module_id = source.module_id or "",
    capability_id = source.capability_id or source.capability or "",
    inputs = type(source.inputs) == "table" and copy_table(source.inputs, 0) or {},
    outputs = array_or_default(source.outputs, {}),
    config = type(source.config) == "table" and copy_table(source.config, 0) or {},
    config_schema = type(source.config_schema) == "table" and copy_table(source.config_schema, 0) or {},
    depends_on = array_or_default(source.depends_on or source.dependencies, {}),
    incompatible_with = array_or_default(source.incompatible_with or source.incompatibilities, {}),
    supported_runtime_targets = array_or_default(source.supported_runtime_targets or source.runtime_targets, {}),
    supported_time_modes = array_or_default(source.supported_time_modes or source.time_modes, {}),
    supported_combat_modes = array_or_default(source.supported_combat_modes or source.combat_modes, {}),
    optional = source.optional == true
  }
end

local function normalize_plan(source)
  local plan = type(source) == "table" and source or {}
  local steps = {}
  if is_array(plan.steps) then
    for index = 1, #plan.steps do
      steps[#steps + 1] = normalize_step(plan.steps[index], index)
    end
  end
  return {
    id = plan.id or "generation/plan",
    title = type(plan.title) == "string" and plan.title or "",
    runtime_target = plan.runtime_target or DEFAULT_RUNTIME_TARGET,
    turn_mode = plan.turn_mode or DEFAULT_TURN_MODE,
    combat_mode = plan.combat_mode or DEFAULT_COMBAT_MODE,
    inputs = type(plan.inputs) == "table" and copy_table(plan.inputs, 0) or {},
    expected_outputs = array_or_default(plan.expected_outputs, {}),
    config_schema = type(plan.config_schema) == "table" and copy_table(plan.config_schema, 0) or {},
    steps = steps
  }
end

local function build_step_index(steps, diagnostics)
  local by_id = {}
  local order = {}
  for index = 1, #steps do
    local step = steps[index]
    if by_id[step.id] ~= nil then
      add_error(diagnostics, "generation.plan.step.duplicate_id", "Step id must be unique in a generator plan.", "steps." .. tostring(index) .. ".id")
    else
      by_id[step.id] = step
      order[#order + 1] = step.id
    end
  end
  return by_id, order
end

local function validate_step(step, index, plan, diagnostics)
  local target = "steps." .. tostring(index)
  if not is_slash_id(step.id) then
    add_error(diagnostics, "generation.plan.step.invalid_id", "Step id must use lowercase slash notation.", target .. ".id")
  end
  if step.module_id ~= "" and not is_module_id(step.module_id) then
    add_error(diagnostics, "generation.plan.step.invalid_module_id", "Step module_id must use lowercase slash notation with at least two segments.", target .. ".module_id")
  end
  if step.capability_id ~= "" and not is_capability_id(step.capability_id) then
    add_error(diagnostics, "generation.plan.step.invalid_capability_id", "Step capability_id must use lowercase dot notation.", target .. ".capability_id")
  end
  if step.module_id == "" and step.capability_id == "" then
    add_error(diagnostics, "generation.plan.step.missing_target", "Step must declare module_id or capability_id.", target)
  end
  if type(step.inputs) ~= "table" then
    add_error(diagnostics, "generation.plan.step.inputs_not_table", "Step inputs must be a table.", target .. ".inputs")
  end
  validate_string_array(step.outputs, "generation.plan.step.outputs", target .. ".outputs", diagnostics)
  if type(step.config) ~= "table" then
    add_error(diagnostics, "generation.plan.step.config_not_table", "Step config must be a table.", target .. ".config")
  end
  if type(step.config_schema) ~= "table" then
    add_error(diagnostics, "generation.plan.step.config_schema_not_table", "Step config_schema must be a table.", target .. ".config_schema")
  end
  validate_id_array(step.depends_on, "generation.plan.step.depends_on", target .. ".depends_on", diagnostics)
  validate_id_array(step.incompatible_with, "generation.plan.step.incompatible_with", target .. ".incompatible_with", diagnostics)

  if #step.supported_runtime_targets > 0 and not contains(step.supported_runtime_targets, plan.runtime_target) then
    add_error(diagnostics, "generation.plan.step.runtime_target_unsupported", "Step does not support the plan runtime target.", target .. ".supported_runtime_targets")
  end
  if #step.supported_time_modes > 0 and not contains(step.supported_time_modes, plan.turn_mode) then
    add_error(diagnostics, "generation.plan.step.time_mode_unsupported", "Step does not support the plan turn mode.", target .. ".supported_time_modes")
  end
  if #step.supported_combat_modes > 0 and not contains(step.supported_combat_modes, plan.combat_mode) then
    add_error(diagnostics, "generation.plan.step.combat_mode_unsupported", "Step does not support the plan combat mode.", target .. ".supported_combat_modes")
  end
end

local function validate_step_relations(steps, by_id, diagnostics)
  local seen_before = {}
  for index = 1, #steps do
    local step = steps[index]
    for dep_index = 1, #step.depends_on do
      local dep = step.depends_on[dep_index]
      if by_id[dep] == nil then
        add_error(diagnostics, "generation.plan.step.missing_dependency", "Step dependency does not exist in this plan.", "steps." .. tostring(index) .. ".depends_on." .. tostring(dep_index))
      elseif seen_before[dep] ~= true then
        add_error(diagnostics, "generation.plan.step.dependency_after_step", "Step dependencies must appear before the dependent step in this batch-level plan model.", "steps." .. tostring(index) .. ".depends_on." .. tostring(dep_index))
      end
    end
    for bad_index = 1, #step.incompatible_with do
      local other = step.incompatible_with[bad_index]
      if by_id[other] ~= nil then
        add_error(diagnostics, "generation.plan.step.incompatible_present", "Step declares an incompatibility with another step that is present in the plan.", "steps." .. tostring(index) .. ".incompatible_with." .. tostring(bad_index))
      end
    end
    seen_before[step.id] = true
  end
end

function M.normalize_plan(plan)
  return normalize_plan(plan)
end

function M.validate_plan(plan, options)
  local diagnostics = {}
  local _options = options
  local normalized = normalize_plan(plan)

  if type(plan) ~= "table" then
    add_error(diagnostics, "generation.plan.not_table", "Generator plan must be a table.", "plan")
    return false, diagnostics
  end
  if not is_slash_id(normalized.id) then
    add_error(diagnostics, "generation.plan.invalid_id", "Plan id must use lowercase slash notation.", "plan.id")
  end
  if RUNTIME_TARGETS[normalized.runtime_target] ~= true then
    add_error(diagnostics, "generation.plan.invalid_runtime_target", "Plan runtime_target is not supported.", "plan.runtime_target")
  end
  if TIME_MODES[normalized.turn_mode] ~= true then
    add_error(diagnostics, "generation.plan.invalid_turn_mode", "Plan turn_mode is not supported.", "plan.turn_mode")
  end
  if COMBAT_MODES[normalized.combat_mode] ~= true then
    add_error(diagnostics, "generation.plan.invalid_combat_mode", "Plan combat_mode is not supported.", "plan.combat_mode")
  end
  if not is_array(plan.steps) then
    add_error(diagnostics, "generation.plan.steps_not_array", "Plan steps must be an array.", "plan.steps")
  end
  if type(normalized.inputs) ~= "table" then
    add_error(diagnostics, "generation.plan.inputs_not_table", "Plan inputs must be a table.", "plan.inputs")
  end
  validate_string_array(normalized.expected_outputs, "generation.plan.expected_outputs", "plan.expected_outputs", diagnostics)

  local by_id = build_step_index(normalized.steps, diagnostics)
  for index = 1, #normalized.steps do
    validate_step(normalized.steps[index], index, normalized, diagnostics)
  end
  validate_step_relations(normalized.steps, by_id, diagnostics)

  if #normalized.steps == 0 then
    add_warning(diagnostics, "generation.plan.empty", "Generator plan has no steps.", "plan.steps")
  end

  local has_error = false
  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      has_error = true
    end
  end
  return not has_error, diagnostics
end

function M.create(plan, options)
  local normalized = normalize_plan(plan)
  local ok, diagnostics = M.validate_plan(plan, options)
  local by_id, order = build_step_index(normalized.steps, diagnostics)
  local step_targets = {}
  for index = 1, #normalized.steps do
    local step = normalized.steps[index]
    step_targets[#step_targets + 1] = {
      step_id = step.id,
      module_id = step.module_id,
      capability_id = step.capability_id,
      depends_on = copy_table(step.depends_on, 0),
      outputs = copy_table(step.outputs, 0)
    }
  end
  local _by_id = by_id
  return result(ok, {
    plan = normalized,
    execution_order = order,
    step_targets = step_targets
  }, diagnostics)
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and type(config) ~= "table" then
    add_error(diagnostics, "generation.plan.config_not_table", "Config must be a table when provided.", "config")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local _ctx = ctx
  local source = type(input) == "table" and input or {}
  local plan = source.plan or source
  local options = source.options or source.config or {}
  return M.create(plan, options)
end

return M
