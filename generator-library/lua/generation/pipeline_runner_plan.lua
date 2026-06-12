local M = {}

M.manifest = {
  id = "generation/pipeline_runner_plan/v1",
  version = "0.1.0",
  category = "generation",
  title = "Pipeline runner plan",
  purpose = "Create deterministic plan IR for a future pipeline runner without running steps.",
  capabilities = {
    "generation.pipeline_runner_plan.generate",
    "generation.pipeline_runner_plan.validate",
    "generation.validation_checkpoints.plan"
  },
  input_schema = {
    plan_id = "string",
    steps = "array",
    selected_module_ids = "array"
  },
  output_schema = {
    pipeline_plan = "plain planning IR",
    diagnostics = "array"
  },
  config_schema = {},
  deterministic = true,
  runtime_targets = { "editor", "validation", "simulation", "codegen_ir", "generator_plan" },
  unsafe_features = {}
}

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function is_array(value)
  if type(value) ~= "table" then
    return false
  end
  local max_index = 0
  for key, _ in pairs(value) do
    if type(key) ~= "number" or key < 1 or key % 1 ~= 0 then
      return false
    end
    if key > max_index then
      max_index = key
    end
  end
  for index = 1, max_index do
    if value[index] == nil then
      return false
    end
  end
  return true
end

local function valid_id(value)
  return type(value) == "string" and value ~= "" and value:match("^[a-z0-9_/%-%.]+$") ~= nil
end

local function copy_array(value)
  local result = {}
  if is_array(value) then
    for index = 1, #value do
      result[#result + 1] = value[index]
    end
  end
  return result
end

local function build_set(list)
  local set = {}
  if is_array(list) then
    for index = 1, #list do
      set[list[index]] = true
    end
  end
  return set
end

local allowed_failure = {
  stop_on_error = true,
  continue_on_warning = true,
  collect_all_diagnostics = true,
  skip_blocked_steps = true
}

local function validate_steps(config)
  local diagnostics = {}
  local module_ids = build_set(config.selected_module_ids)
  local checkpoint_ids = build_set(config.validation_checkpoint_ids)
  local step_ids = {}
  local steps = is_array(config.steps) and config.steps or {}

  for index = 1, #steps do
    local step = steps[index]
    local target = "steps[" .. index .. "]"
    if type(step) ~= "table" then
      diagnostics[#diagnostics + 1] = diag("error", "invalid_step", "Step must be a table.", target)
    else
      if not valid_id(step.id) then
        diagnostics[#diagnostics + 1] = diag("error", "invalid_step_id", "Step id must be a lowercase slash or dot id.", target .. ".id")
      elseif step_ids[step.id] then
        diagnostics[#diagnostics + 1] = diag("error", "duplicate_step_id", "Duplicate step id.", step.id)
      else
        step_ids[step.id] = true
      end

      if not valid_id(step.module_id) then
        diagnostics[#diagnostics + 1] = diag("error", "missing_module_ref", "Step must reference a selected module id.", target .. ".module_id")
      elseif is_array(config.selected_module_ids) and not module_ids[step.module_id] then
        diagnostics[#diagnostics + 1] = diag("error", "unknown_module_ref", "Step references a module id outside selected_module_ids.", target .. ".module_id")
      end

      if step.validation_checkpoints ~= nil and not is_array(step.validation_checkpoints) then
        diagnostics[#diagnostics + 1] = diag("error", "invalid_checkpoint_refs", "validation_checkpoints must be an array.", target .. ".validation_checkpoints")
      elseif is_array(step.validation_checkpoints) and is_array(config.validation_checkpoint_ids) then
        for ref_index = 1, #step.validation_checkpoints do
          local checkpoint_id = step.validation_checkpoints[ref_index]
          if not checkpoint_ids[checkpoint_id] then
            diagnostics[#diagnostics + 1] = diag("error", "unknown_checkpoint_ref", "Step references an unknown validation checkpoint.", target .. ".validation_checkpoints[" .. ref_index .. "]")
          end
        end
      end

      if step.run_now == true or step.call_runtime == true or step.apply_outputs == true or step.mutate_package == true then
        diagnostics[#diagnostics + 1] = diag("error", "unsafe_execution_flag", "Pipeline runner plan may only declare future steps and must not request runtime work.", target)
      end

      if step.failure_policy ~= nil and not allowed_failure[step.failure_policy] then
        diagnostics[#diagnostics + 1] = diag("error", "invalid_failure_policy", "failure_policy is not allowed.", target .. ".failure_policy")
      end

      if step.depends_on_steps ~= nil and not is_array(step.depends_on_steps) then
        diagnostics[#diagnostics + 1] = diag("error", "invalid_step_dependencies", "depends_on_steps must be an array.", target .. ".depends_on_steps")
      end
    end
  end

  for index = 1, #steps do
    local step = steps[index]
    if type(step) == "table" and is_array(step.depends_on_steps) then
      for dep_index = 1, #step.depends_on_steps do
        local dep = step.depends_on_steps[dep_index]
        if not step_ids[dep] then
          diagnostics[#diagnostics + 1] = diag("error", "missing_step_dependency", "Step dependency target is missing.", step.id .. " depends_on " .. tostring(dep))
        end
      end
    end
  end

  return diagnostics
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return { ok = true, diagnostics = diagnostics }
  end
  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "invalid_config", "Config must be a table.", "config")
    return { ok = false, diagnostics = diagnostics }
  end
  if config.steps ~= nil and not is_array(config.steps) then
    diagnostics[#diagnostics + 1] = diag("error", "invalid_steps", "steps must be an array when present.", "steps")
  end
  if config.selected_module_ids ~= nil and not is_array(config.selected_module_ids) then
    diagnostics[#diagnostics + 1] = diag("error", "invalid_selected_modules", "selected_module_ids must be an array when present.", "selected_module_ids")
  end
  return { ok = #diagnostics == 0, diagnostics = diagnostics }
end

function M.generate(input, ctx)
  local config = {}
  if type(input) == "table" then
    config = input
  end

  local diagnostics = {}
  local base = M.validate_config(config)
  for index = 1, #base.diagnostics do
    diagnostics[#diagnostics + 1] = base.diagnostics[index]
  end
  local step_diags = validate_steps(config)
  for index = 1, #step_diags do
    diagnostics[#diagnostics + 1] = step_diags[index]
  end

  local steps = {}
  if is_array(config.steps) then
    for index = 1, #config.steps do
      local step = config.steps[index]
      if type(step) == "table" then
        steps[#steps + 1] = {
          id = step.id,
          order = index,
          module_id = step.module_id,
          config_ref = step.config_ref,
          inline_config = type(step.inline_config) == "table" and step.inline_config or nil,
          expected_artifacts = copy_array(step.expected_artifacts),
          validation_checkpoints = copy_array(step.validation_checkpoints),
          depends_on_steps = copy_array(step.depends_on_steps),
          dry_run = step.dry_run ~= false,
          failure_policy = step.failure_policy or "stop_on_error"
        }
      end
    end
  end

  return {
    ok = #diagnostics == 0,
    data = {
      plan_id = config.plan_id or "generation/pipeline_runner_plan/default",
      mode = "plan_only",
      selected_module_ids = copy_array(config.selected_module_ids),
      validation_checkpoint_ids = copy_array(config.validation_checkpoint_ids),
      expected_artifacts = copy_array(config.expected_artifacts),
      steps = steps,
      dry_run_default = config.dry_run_default ~= false,
      does_not_run_steps = true,
      ctx_tag = type(ctx) == "table" and ctx.tag or nil
    },
    diagnostics = diagnostics,
    artifacts = {
      {
        id = (config.plan_id or "generation/pipeline_runner_plan/default") .. "/plan",
        kind = "pipeline_runner_plan_ir",
        produced_by = M.manifest.id
      }
    }
  }
end

return M
