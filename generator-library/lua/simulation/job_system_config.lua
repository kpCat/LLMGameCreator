local M = {}

M.manifest = {
  id = "simulation/job_system_config/v1",
  version = "0.1.0",
  category = "simulation",
  title = "Job System Config",
  purpose = "Generate deterministic job role and workplace assignment IR for city-builder configs.",
  capabilities = { "city_builder.jobs.configure", "city_builder.workplaces.configure", "simulation.tick_metadata" },
  input_schema = { kind = "city_builder.jobs.input" },
  output_schema = { kind = "city_builder.jobs.ir" },
  config_schema = { kind = "city_builder.jobs.config" },
  deterministic = true,
  runtime_targets = { "editor", "simulation", "unity2d", "unity3d", "codegen_ir" },
  unsafe_features = {}
}

local function diag(list, severity, code, message, target)
  list[#list + 1] = { severity = severity, code = code, message = message, target = target }
end

local function is_array(value)
  if type(value) ~= "table" then return false end
  local count = 0
  for k, _ in pairs(value) do
    if type(k) ~= "number" or k < 1 or k % 1 ~= 0 then return false end
    if k > count then count = k end
  end
  for i = 1, count do if value[i] == nil then return false end end
  return true
end

local function valid_id(value)
  return type(value) == "string" and value:match("^[a-z][a-z0-9_]*(/[a-z][a-z0-9_]*)*$") ~= nil
end

local function copy_array(value)
  local result = {}
  if type(value) == "table" then for i = 1, #value do result[i] = value[i] end end
  return result
end

local allowed_turn_modes = { realtime = true, turn_based = true, mixed = true, paused_planning = true }

local function default_jobs()
  return {
    {
      id = "job/builder",
      workplace_category = "utility",
      worker_capacity = 4,
      required_tags = { "construction" },
      required_skills = { "labor" },
      shift = { mode = "day", start_tick = 0, duration_ticks = 8 },
      economy_hooks = { upkeep_currency = "currency/coins", wage_per_tick = 1 }
    },
    {
      id = "job/farmer",
      workplace_category = "production",
      worker_capacity = 6,
      required_tags = { "food" },
      required_skills = { "farming" },
      shift = { mode = "day", start_tick = 0, duration_ticks = 10 },
      economy_hooks = { output_item = "item/food", output_per_tick = 2 }
    },
    {
      id = "job/healer",
      workplace_category = "service",
      worker_capacity = 2,
      required_tags = { "health" },
      required_skills = { "medicine" },
      shift = { mode = "mixed", start_tick = 0, duration_ticks = 12 },
      economy_hooks = { service_id = "service/clinic" }
    }
  }
end

local function validate_job(job, index, diagnostics)
  local target = "jobs[" .. index .. "]"
  if type(job) ~= "table" then
    diag(diagnostics, "error", "jobs.entry_not_table", "Job entry must be a table.", target)
    return
  end
  if not valid_id(job.id) then
    diag(diagnostics, "error", "jobs.invalid_id", "Job id must use lowercase slash notation.", target .. ".id")
  end
  if job.workplace_building_id ~= nil and not valid_id(job.workplace_building_id) then
    diag(diagnostics, "error", "jobs.invalid_workplace_id", "Workplace building reference must use lowercase slash notation.", target .. ".workplace_building_id")
  end
  if job.workplace_category == nil and job.workplace_building_id == nil then
    diag(diagnostics, "error", "jobs.missing_workplace", "Job must reference a workplace category or building id.", target)
  end
  if type(job.worker_capacity) ~= "number" or job.worker_capacity < 1 or job.worker_capacity % 1 ~= 0 then
    diag(diagnostics, "error", "jobs.invalid_capacity", "Worker capacity must be a positive integer.", target .. ".worker_capacity")
  end
  if job.required_tags ~= nil and not is_array(job.required_tags) then
    diag(diagnostics, "error", "jobs.invalid_required_tags", "Required tags must be an array.", target .. ".required_tags")
  end
  if job.required_skills ~= nil and not is_array(job.required_skills) then
    diag(diagnostics, "error", "jobs.invalid_required_skills", "Required skills must be an array.", target .. ".required_skills")
  end
  if job.shift ~= nil then
    if type(job.shift) ~= "table" then
      diag(diagnostics, "error", "jobs.invalid_shift", "Shift metadata must be a table.", target .. ".shift")
    else
      if job.shift.duration_ticks ~= nil and (type(job.shift.duration_ticks) ~= "number" or job.shift.duration_ticks < 0) then
        diag(diagnostics, "error", "jobs.invalid_shift_duration", "Shift duration must be non-negative.", target .. ".shift.duration_ticks")
      end
    end
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then return true, diagnostics end
  if type(config) ~= "table" then
    diag(diagnostics, "error", "jobs.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.turn_mode ~= nil and not allowed_turn_modes[config.turn_mode] then
    diag(diagnostics, "error", "jobs.invalid_turn_mode", "Unsupported turn mode for job config.", "turn_mode")
  end
  if config.jobs ~= nil then
    if not is_array(config.jobs) then
      diag(diagnostics, "error", "jobs.jobs_not_array", "Jobs must be an array.", "jobs")
    else
      local seen = {}
      for i = 1, #config.jobs do
        local job = config.jobs[i]
        validate_job(job, i, diagnostics)
        if type(job) == "table" and type(job.id) == "string" then
          if seen[job.id] then diag(diagnostics, "error", "jobs.duplicate_id", "Duplicate job id.", "jobs[" .. i .. "].id") end
          seen[job.id] = true
        end
      end
    end
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local config = {}
  if type(input) == "table" and type(input.config) == "table" then config = input.config elseif type(input) == "table" then config = input end
  local ok, diagnostics = M.validate_config(config)
  if not ok then return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} } end
  local source = config.jobs or default_jobs()
  local roles = {}
  for i = 1, #source do
    local job = source[i]
    roles[i] = {
      id = job.id,
      workplace_category = job.workplace_category,
      workplace_building_id = job.workplace_building_id,
      worker_capacity = job.worker_capacity,
      required_tags = copy_array(job.required_tags),
      required_skills = copy_array(job.required_skills),
      shift = job.shift or { mode = "any", start_tick = 0, duration_ticks = 0 },
      economy_hooks = job.economy_hooks or {}
    }
  end
  local data = {
    schema = "city_builder.job_system.v1",
    turn_mode = config.turn_mode or "mixed",
    tick_mode = config.tick_mode or "simulation_tick",
    roles = roles,
    assignment_constraints = config.assignment_constraints or { allow_unfilled_jobs = true, prefer_skill_match = true },
    metadata = { deterministic = true, generated_by = M.manifest.id }
  }
  return { ok = true, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
