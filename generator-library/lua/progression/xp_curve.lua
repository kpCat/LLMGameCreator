local M = {}

M.manifest = {
  id = "progression/xp_curve/v1",
  version = "0.1.0",
  category = "progression",
  title = "XP Curve Generator",
  purpose = "Generate compact deterministic XP curve IR for RPG levels, research tiers or abstract progression ladders.",
  capabilities = { "progression.xp_curve.generate", "progression.level_thresholds", "formula.curve_reference" },
  input_schema = {
    curve = "curve definition or shorthand fields on input"
  },
  output_schema = {
    curve = "normalized curve metadata",
    levels = "array of threshold rows up to max_level",
    formula_ref = "safe formula reference metadata"
  },
  config_schema = {
    max_level_limit = "optional hard limit for generated levels",
    allow_table_output = "optional boolean; false keeps metadata only when too large"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "unity_ui_ir", "codegen_ir" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function add(out, severity, code, message, target)
  out[#out + 1] = diag(severity, code, message, target)
end

local function id_ok(value)
  if type(value) ~= "string" or value == "" then return false end
  if value:sub(1, 1) == "/" or value:sub(-1) == "/" or value:find("//", 1, true) then return false end
  return value:match("^[a-z0-9_/%-%.]+$") ~= nil
end

local function positive_integer(value)
  return type(value) == "number" and value > 0 and value % 1 == 0
end

local function non_negative_number(value)
  return type(value) == "number" and value >= 0
end

local function round(value)
  local down = value - value % 1
  if value - down >= 0.5 then return down + 1 end
  return down
end

local function make_curve_config(input, ctx, diagnostics)
  local c = {}
  if type(input) == "table" and type(input.curve) == "table" then c = input.curve else c = type(input) == "table" and input or {} end
  local hard = 100
  if ctx and type(ctx.config) == "table" and positive_integer(ctx.config.max_level_limit) then hard = ctx.config.max_level_limit end
  local max_level = c.max_level or c.levels or 10
  if not positive_integer(max_level) then
    add(diagnostics, "error", "xp.invalid_max_level", "max_level must be a positive integer.", "input.curve.max_level")
    max_level = 10
  end
  if max_level > hard then
    add(diagnostics, "warning", "xp.max_level_clamped", "max_level was clamped by max_level_limit.", "input.curve.max_level")
    max_level = hard
  end
  local curve_id = c.id or c.curve_id or "progression/xp/default"
  if not id_ok(curve_id) then
    add(diagnostics, "error", "xp.invalid_curve_id", "Curve id must be a lowercase slash id.", "input.curve.id")
    curve_id = "progression/xp/invalid"
  end
  local mode = c.mode or c.kind or "quadratic"
  if mode ~= "linear" and mode ~= "quadratic" and mode ~= "stepped" and mode ~= "exponential" then
    add(diagnostics, "warning", "xp.unknown_mode", "Unknown curve mode replaced by quadratic.", "input.curve.mode")
    mode = "quadratic"
  end
  local base = c.base or 100
  if not non_negative_number(base) then
    add(diagnostics, "error", "xp.invalid_base", "base must be a non-negative number.", "input.curve.base")
    base = 100
  end
  local growth = c.growth or 1.25
  if not non_negative_number(growth) then
    add(diagnostics, "error", "xp.invalid_growth", "growth must be a non-negative number.", "input.curve.growth")
    growth = 1.25
  end
  return { id = curve_id, title = c.title or curve_id, mode = mode, base = base, growth = growth, max_level = max_level, start_level = c.start_level or 1 }
end

local function delta_for_level(curve, level)
  if level <= curve.start_level then return 0 end
  local step = level - curve.start_level
  if curve.mode == "linear" then
    return round(curve.base + curve.growth * step)
  end
  if curve.mode == "quadratic" then
    return round(curve.base + curve.growth * step * step)
  end
  if curve.mode == "stepped" then
    local band = 1 + (step - step % 5) / 5
    return round(curve.base * band * curve.growth)
  end
  local value = curve.base
  local count = 1
  while count <= step do
    value = value * curve.growth
    count = count + 1
  end
  return round(value)
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and type(config) ~= "table" then
    add(diagnostics, "error", "xp.config_not_table", "Config must be a table when provided.", "config")
  elseif type(config) == "table" then
    if config.max_level_limit ~= nil and not positive_integer(config.max_level_limit) then
      add(diagnostics, "error", "xp.invalid_max_level_limit", "max_level_limit must be a positive integer.", "config.max_level_limit")
    end
    if config.allow_table_output ~= nil and type(config.allow_table_output) ~= "boolean" then
      add(diagnostics, "error", "xp.invalid_allow_table_output", "allow_table_output must be boolean when provided.", "config.allow_table_output")
    end
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local curve = make_curve_config(input, ctx, diagnostics)
  local levels = {}
  local total = 0
  for level = 1, curve.max_level do
    local delta = delta_for_level(curve, level)
    total = total + delta
    levels[#levels + 1] = { level = level, delta_xp = delta, total_xp = total }
  end
  local formula_ref = {
    id = curve.id .. "/formula_ref",
    kind = "curve_ref",
    curve_id = curve.id,
    level_ref = "progression/level",
    output = "progression/required_total_xp"
  }
  return {
    ok = #diagnostics == 0,
    data = {
      curve = curve,
      levels = levels,
      formula_ref = formula_ref,
      summary = { level_count = #levels, final_total_xp = total, deterministic = true }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
