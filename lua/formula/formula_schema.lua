local M = {}

M.manifest = {
  id = "formula/formula_schema/v1",
  version = "0.1.0",
  category = "formula",
  title = "Formula Schema",
  purpose = "Normalize and validate safe formula IR for stats, attributes, progression and combat references without executing raw code.",
  capabilities = { "formula.schema.normalize", "formula.validate", "stats.attribute_formula_ir" },
  input_schema = {
    formulas = "array of formula definitions or a single formula definition"
  },
  output_schema = {
    formulas = "normalized formula definitions",
    indexes = "lookup maps by id, result stat and tag",
    summary = "counts and validation summary"
  },
  config_schema = {
    allowed_ops = "optional array of operation ids",
    allowed_value_refs = "optional array of allowed value reference prefixes or exact ids",
    max_depth = "optional positive integer",
    max_args_per_op = "optional positive integer",
    max_formulas = "optional positive integer"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "unity_ui_ir", "codegen_ir" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local DEFAULT_OPS = {
  "const", "ref", "add", "sub", "mul", "div", "min", "max", "clamp", "neg", "floor", "ceil", "round", "percent", "curve_ref"
}

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function add(diagnostics, severity, code, message, target)
  diagnostics[#diagnostics + 1] = diag(severity, code, message, target)
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

local function id_ok(value)
  if type(value) ~= "string" or value == "" then
    return false
  end
  if value:sub(1, 1) == "/" or value:sub(-1) == "/" or value:find("//", 1, true) then
    return false
  end
  return value:match("^[a-z0-9_/%-%.]+$") ~= nil
end

local function token_ok(value)
  return type(value) == "string" and value ~= "" and value:match("^[a-z0-9_%-%.]+$") ~= nil
end

local function positive_integer(value)
  return type(value) == "number" and value > 0 and value % 1 == 0
end

local function clone_array(value)
  local result = {}
  if type(value) == "table" then
    for index = 1, #value do
      result[#result + 1] = value[index]
    end
  end
  return result
end

local function list_to_set(list)
  local result = {}
  if type(list) == "table" then
    for index = 1, #list do
      if type(list[index]) == "string" then
        result[list[index]] = true
      end
    end
  end
  return result
end

local function normalize_tags(tags, diagnostics, target)
  local result = {}
  local seen = {}
  if tags == nil then
    return result
  end
  if type(tags) ~= "table" then
    add(diagnostics, "warning", "formula.tags_not_array", "Formula tags must be an array of tokens; tags were ignored.", target)
    return result
  end
  for index = 1, #tags do
    local tag = tags[index]
    if token_ok(tag) and not seen[tag] then
      seen[tag] = true
      result[#result + 1] = tag
    else
      add(diagnostics, "warning", "formula.invalid_tag", "Invalid or duplicate tag ignored.", target .. "[" .. index .. "]")
    end
  end
  return result
end

local function ref_allowed(ref, allowed)
  if type(allowed) ~= "table" then
    return true
  end
  if allowed[ref] then
    return true
  end
  for key, _ in pairs(allowed) do
    if type(key) == "string" and key:sub(-1) == "/" and ref:sub(1, #key) == key then
      return true
    end
  end
  return false
end

local function normalize_node(node, cfg, diagnostics, target, depth)
  if type(node) ~= "table" then
    add(diagnostics, "error", "formula.node_not_table", "Formula expression node must be a table.", target)
    return { op = "const", value = 0 }
  end
  if depth > cfg.max_depth then
    add(diagnostics, "error", "formula.max_depth_exceeded", "Formula expression exceeds max_depth.", target)
    return { op = "const", value = 0 }
  end
  local op = node.op or node.type or node.kind
  if op == "constant" then op = "const" end
  if op == "variable" or op == "attribute" or op == "stat" then op = "ref" end
  if op == nil then
    add(diagnostics, "error", "formula.missing_op", "Formula expression node is missing op.", target)
    return { op = "const", value = 0 }
  end
  if not cfg.allowed_ops[op] then
    add(diagnostics, "error", "formula.op_not_allowed", "Formula operation is not allowed by config.", target .. ".op")
    return { op = "const", value = 0 }
  end
  if op == "const" then
    local value = node.value
    if type(value) ~= "number" then
      add(diagnostics, "error", "formula.const_not_number", "Const node value must be a number.", target .. ".value")
      value = 0
    end
    return { op = "const", value = value }
  end
  if op == "ref" then
    local ref = node.ref or node.id or node.name
    if not id_ok(ref) or not ref_allowed(ref, cfg.allowed_value_refs) then
      add(diagnostics, "error", "formula.invalid_ref", "Ref node must target an allowed slash or dotted id.", target .. ".ref")
      ref = "invalid/ref"
    end
    return { op = "ref", ref = ref, default = type(node.default) == "number" and node.default or 0 }
  end
  if op == "curve_ref" then
    local curve_id = node.curve_id or node.id
    local level_ref = node.level_ref or node.level or "progression/level"
    if not id_ok(curve_id) then
      add(diagnostics, "error", "formula.invalid_curve_ref", "Curve reference must have valid curve_id.", target .. ".curve_id")
      curve_id = "invalid/curve"
    end
    if not id_ok(level_ref) then
      add(diagnostics, "error", "formula.invalid_level_ref", "Curve level_ref must be valid id.", target .. ".level_ref")
      level_ref = "progression/level"
    end
    return { op = "curve_ref", curve_id = curve_id, level_ref = level_ref, default = type(node.default) == "number" and node.default or 0 }
  end
  if op == "neg" or op == "floor" or op == "ceil" or op == "round" or op == "percent" then
    local child = normalize_node(node.arg or node.value or node.args and node.args[1], cfg, diagnostics, target .. ".arg", depth + 1)
    return { op = op, arg = child }
  end
  if op == "clamp" then
    local value = normalize_node(node.value or node.arg or node.args and node.args[1], cfg, diagnostics, target .. ".value", depth + 1)
    local min_value = normalize_node(node.min or node.args and node.args[2] or { op = "const", value = 0 }, cfg, diagnostics, target .. ".min", depth + 1)
    local max_value = normalize_node(node.max or node.args and node.args[3] or { op = "const", value = 999999 }, cfg, diagnostics, target .. ".max", depth + 1)
    return { op = "clamp", value = value, min = min_value, max = max_value }
  end
  local args = node.args or node.terms or {}
  if not is_array(args) then
    add(diagnostics, "error", "formula.args_not_array", "Operation args must be an array.", target .. ".args")
    args = {}
  end
  local min_args = 2
  if op == "add" or op == "mul" or op == "min" or op == "max" then
    min_args = 1
  end
  if #args < min_args then
    add(diagnostics, "error", "formula.not_enough_args", "Operation does not have enough args.", target .. ".args")
  end
  if #args > cfg.max_args_per_op then
    add(diagnostics, "warning", "formula.args_truncated", "Operation args were truncated to max_args_per_op.", target .. ".args")
  end
  local out_args = {}
  local limit = #args
  if limit > cfg.max_args_per_op then limit = cfg.max_args_per_op end
  for index = 1, limit do
    out_args[#out_args + 1] = normalize_node(args[index], cfg, diagnostics, target .. ".args[" .. index .. "]", depth + 1)
  end
  return { op = op, args = out_args }
end

local function normalize_formula(value, cfg, diagnostics, target)
  if type(value) ~= "table" then
    add(diagnostics, "error", "formula.definition_not_table", "Formula definition must be a table.", target)
    return nil
  end
  local id = value.id
  if not id_ok(id) then
    add(diagnostics, "error", "formula.invalid_id", "Formula id must be a lowercase slash id.", target .. ".id")
    return nil
  end
  local expr = value.expression or value.expr or value.formula
  if type(value.raw_code) == "string" or type(value.lua) == "string" or type(value.script) == "string" then
    add(diagnostics, "error", "formula.raw_code_forbidden", "Formula IR must not contain raw executable code fields.", target)
  end
  local result_stat = value.result_stat or value.target_stat or value.output
  if result_stat ~= nil and not id_ok(result_stat) then
    add(diagnostics, "warning", "formula.invalid_result_stat", "Invalid result_stat ignored.", target .. ".result_stat")
    result_stat = nil
  end
  return {
    id = id,
    title = type(value.title) == "string" and value.title or id,
    purpose = type(value.purpose) == "string" and value.purpose or "",
    result_stat = result_stat,
    expression = normalize_node(expr or { op = "const", value = 0 }, cfg, diagnostics, target .. ".expression", 1),
    tags = normalize_tags(value.tags, diagnostics, target .. ".tags"),
    notes = type(value.notes) == "string" and value.notes or ""
  }
end

local function build_config(config)
  local cfg = type(config) == "table" and config or {}
  local max_depth = cfg.max_depth or 8
  local max_args = cfg.max_args_per_op or 8
  local max_formulas = cfg.max_formulas or 64
  return {
    allowed_ops = list_to_set(cfg.allowed_ops or DEFAULT_OPS),
    allowed_value_refs = cfg.allowed_value_refs and list_to_set(cfg.allowed_value_refs) or nil,
    max_depth = positive_integer(max_depth) and max_depth or 8,
    max_args_per_op = positive_integer(max_args) and max_args or 8,
    max_formulas = positive_integer(max_formulas) and max_formulas or 64
  }
end

function M.validate_config(config)
  local diagnostics = {}
  local cfg = type(config) == "table" and config or {}
  if config ~= nil and type(config) ~= "table" then
    add(diagnostics, "error", "formula.config_not_table", "Config must be a table when provided.", "config")
  end
  if cfg.max_depth ~= nil and not positive_integer(cfg.max_depth) then
    add(diagnostics, "error", "formula.invalid_max_depth", "max_depth must be a positive integer.", "config.max_depth")
  end
  if cfg.max_args_per_op ~= nil and not positive_integer(cfg.max_args_per_op) then
    add(diagnostics, "error", "formula.invalid_max_args", "max_args_per_op must be a positive integer.", "config.max_args_per_op")
  end
  if cfg.max_formulas ~= nil and not positive_integer(cfg.max_formulas) then
    add(diagnostics, "error", "formula.invalid_max_formulas", "max_formulas must be a positive integer.", "config.max_formulas")
  end
  if cfg.allowed_ops ~= nil and not is_array(cfg.allowed_ops) then
    add(diagnostics, "error", "formula.allowed_ops_not_array", "allowed_ops must be an array of operation ids.", "config.allowed_ops")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local config = build_config(ctx and ctx.config or nil)
  local source = input and (input.formulas or input.formula or input) or {}
  if source.id then source = { source } end
  if not is_array(source) then
    add(diagnostics, "error", "formula.input_not_array", "Input must contain formulas array or a single formula table.", "input.formulas")
    source = {}
  end
  local formulas = {}
  local by_id = {}
  local by_result_stat = {}
  local by_tag = {}
  local limit = #source
  if limit > config.max_formulas then
    add(diagnostics, "warning", "formula.formulas_truncated", "Formula list was truncated to max_formulas.", "input.formulas")
    limit = config.max_formulas
  end
  for index = 1, limit do
    local formula = normalize_formula(source[index], config, diagnostics, "input.formulas[" .. index .. "]")
    if formula ~= nil then
      if by_id[formula.id] then
        add(diagnostics, "error", "formula.duplicate_id", "Duplicate formula id ignored.", formula.id)
      else
        by_id[formula.id] = true
        formulas[#formulas + 1] = formula
        if formula.result_stat then
          if not by_result_stat[formula.result_stat] then by_result_stat[formula.result_stat] = {} end
          by_result_stat[formula.result_stat][#by_result_stat[formula.result_stat] + 1] = formula.id
        end
        for tag_index = 1, #formula.tags do
          local tag = formula.tags[tag_index]
          if not by_tag[tag] then by_tag[tag] = {} end
          by_tag[tag][#by_tag[tag] + 1] = formula.id
        end
      end
    end
  end
  return {
    ok = #diagnostics == 0,
    data = {
      formulas = formulas,
      indexes = { by_id = by_id, by_result_stat = by_result_stat, by_tag = by_tag },
      summary = { formula_count = #formulas, diagnostic_count = #diagnostics, safe_ir_only = true }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
