local M = {}

M.manifest = {
  id = "core/mode_transition/v1",
  version = "0.1.0",
  category = "core",
  title = "Mode transition rules",
  purpose = "Validate deterministic transitions between exploration, combat, dialogue, dialogue-combat, and paused planning modes.",
  capabilities = {
    "core.mode_transition.rules",
    "core.mode_transition.can_transition",
    "core.mode_transition.apply",
    "core.mode_transition.dialogue_combat"
  },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      strict = { type = "boolean" },
      rules = { type = "array" }
    }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "simulation", "codegen_ir" },
  unsafe_features = {}
}

local MODES = {
  exploration = true,
  combat = true,
  dialogue = true,
  dialogue_combat = true,
  paused_planning = true
}

local DEFAULT_RULES = {
  { from = "exploration", to = "dialogue", reason = "talk_to_npc" },
  { from = "exploration", to = "combat", reason = "hostile_encounter" },
  { from = "exploration", to = "paused_planning", reason = "planning_pause" },
  { from = "dialogue", to = "exploration", reason = "dialogue_end" },
  { from = "dialogue", to = "dialogue_combat", reason = "social_conflict" },
  { from = "dialogue", to = "combat", reason = "dialogue_escalates" },
  { from = "dialogue_combat", to = "dialogue", reason = "deescalate" },
  { from = "dialogue_combat", to = "combat", reason = "social_conflict_turns_physical" },
  { from = "dialogue_combat", to = "exploration", reason = "resolved_without_fight" },
  { from = "combat", to = "exploration", reason = "combat_end" },
  { from = "combat", to = "dialogue_combat", reason = "parley_or_morale_break" },
  { from = "combat", to = "paused_planning", reason = "tactical_pause" },
  { from = "paused_planning", to = "exploration", reason = "resume_exploration" },
  { from = "paused_planning", to = "combat", reason = "resume_combat" }
}

local function make_diagnostic(code, message, target)
  local diagnostic = {
    severity = "error",
    code = code,
    message = message
  }
  if target ~= nil then
    diagnostic.target = target
  end
  return diagnostic
end

local function make_warning(code, message, target)
  local diagnostic = make_diagnostic(code, message, target)
  diagnostic.severity = "warning"
  return diagnostic
end

local function result(ok, data, diagnostics)
  return {
    ok = ok == true,
    data = type(data) == "table" and data or {},
    diagnostics = type(diagnostics) == "table" and diagnostics or {},
    artifacts = {}
  }
end

local function is_non_empty_string(value)
  return type(value) == "string" and value ~= ""
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

local function rule_key(rule)
  return tostring(rule.from) .. "->" .. tostring(rule.to)
end

local function normalize_rule(rule, index, diagnostics)
  if type(rule) ~= "table" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.mode_transition.rule_not_table", "Transition rule must be a table.", "rules." .. tostring(index))
    return nil
  end
  if MODES[rule.from] ~= true then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.mode_transition.invalid_from_mode", "Rule from mode is not supported.", "rules." .. tostring(index) .. ".from")
  end
  if MODES[rule.to] ~= true then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.mode_transition.invalid_to_mode", "Rule to mode is not supported.", "rules." .. tostring(index) .. ".to")
  end
  if rule.from == rule.to then
    diagnostics[#diagnostics + 1] = make_warning("core.mode_transition.self_transition", "Self-transition rule is allowed but usually unnecessary.", "rules." .. tostring(index))
  end
  local requires = {}
  if type(rule.requires) == "table" then
    for req_index = 1, #rule.requires do
      if is_non_empty_string(rule.requires[req_index]) then
        requires[#requires + 1] = rule.requires[req_index]
      else
        diagnostics[#diagnostics + 1] = make_diagnostic("core.mode_transition.invalid_requirement", "Rule requirement names must be non-empty strings.", "rules." .. tostring(index) .. ".requires." .. tostring(req_index))
      end
    end
  elseif rule.requires ~= nil then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.mode_transition.requires_not_array", "requires must be an array table when provided.", "rules." .. tostring(index) .. ".requires")
  end
  return {
    from = rule.from,
    to = rule.to,
    reason = is_non_empty_string(rule.reason) and rule.reason or "unspecified",
    requires = requires,
    combat_mode = is_non_empty_string(rule.combat_mode) and rule.combat_mode or nil,
    turn_mode = is_non_empty_string(rule.turn_mode) and rule.turn_mode or nil
  }
end

local function build_rules(config)
  local diagnostics = {}
  local rules = {}
  local source_rules = type(config) == "table" and type(config.rules) == "table" and config.rules or DEFAULT_RULES
  local seen = {}
  for index = 1, #source_rules do
    local rule = normalize_rule(source_rules[index], index, diagnostics)
    if rule ~= nil and MODES[rule.from] == true and MODES[rule.to] == true then
      local key = rule_key(rule)
      if seen[key] == true then
        diagnostics[#diagnostics + 1] = make_warning("core.mode_transition.duplicate_rule", "Duplicate transition rule keeps deterministic first-match behavior.", "rules." .. tostring(index))
      end
      seen[key] = true
      rules[#rules + 1] = rule
    end
  end
  return rules, diagnostics
end

local function has_requirement(context, requirement)
  if type(context) ~= "table" then
    return false
  end
  if type(context.flags) == "table" and context.flags[requirement] == true then
    return true
  end
  return context[requirement] == true
end

local function requirements_met(rule, context)
  local missing = {}
  for index = 1, #rule.requires do
    local requirement = rule.requires[index]
    if not has_requirement(context, requirement) then
      missing[#missing + 1] = requirement
    end
  end
  return missing[1] == nil, missing
end

function M.default_rules()
  return copy_table(DEFAULT_RULES, 0)
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.mode_transition.config_not_table", "Mode transition config must be a table.", "config")
    return false, diagnostics
  end
  if config.strict ~= nil and type(config.strict) ~= "boolean" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.mode_transition.strict_not_boolean", "strict must be a boolean when provided.", "config.strict")
  end
  if config.rules ~= nil and type(config.rules) ~= "table" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.mode_transition.rules_not_array", "rules must be an array table when provided.", "config.rules")
  elseif type(config.rules) == "table" then
    local _, rule_diagnostics = build_rules(config)
    for index = 1, #rule_diagnostics do
      diagnostics[#diagnostics + 1] = rule_diagnostics[index]
    end
  end
  for key, _ in pairs(config) do
    if key ~= "strict" and key ~= "rules" then
      diagnostics[#diagnostics + 1] = make_warning("core.mode_transition.unknown_config_key", "Unknown config key is ignored by mode transition rules.", "config." .. tostring(key))
    end
  end
  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      return false, diagnostics
    end
  end
  return true, diagnostics
end

function M.list_allowed(from_mode, config, context)
  if MODES[from_mode] ~= true then
    return result(false, {}, { make_diagnostic("core.mode_transition.invalid_from_mode", "from_mode is not supported.", "from_mode") })
  end
  local rules, diagnostics = build_rules(config)
  local allowed = {}
  for index = 1, #rules do
    local rule = rules[index]
    if rule.from == from_mode then
      local requirements_ok = requirements_met(rule, context)
      if requirements_ok then
        allowed[#allowed + 1] = copy_table(rule, 0)
      end
    end
  end
  return result(true, { from = from_mode, allowed = allowed }, diagnostics)
end

function M.can_transition(from_mode, to_mode, config, context)
  if MODES[from_mode] ~= true then
    return result(false, { allowed = false }, { make_diagnostic("core.mode_transition.invalid_from_mode", "from_mode is not supported.", "from_mode") })
  end
  if MODES[to_mode] ~= true then
    return result(false, { allowed = false }, { make_diagnostic("core.mode_transition.invalid_to_mode", "to_mode is not supported.", "to_mode") })
  end
  local rules, diagnostics = build_rules(config)
  for index = 1, #rules do
    local rule = rules[index]
    if rule.from == from_mode and rule.to == to_mode then
      local requirements_ok, missing = requirements_met(rule, context)
      if requirements_ok then
        return result(true, { allowed = true, rule = copy_table(rule, 0), missing_requirements = {} }, diagnostics)
      end
      diagnostics[#diagnostics + 1] = make_diagnostic("core.mode_transition.requirements_missing", "Transition rule exists but required context flags are missing.", "context")
      return result(false, { allowed = false, rule = copy_table(rule, 0), missing_requirements = missing }, diagnostics)
    end
  end
  diagnostics[#diagnostics + 1] = make_diagnostic("core.mode_transition.not_allowed", "No transition rule allows this mode change.", "transition")
  return result(false, { allowed = false, missing_requirements = {} }, diagnostics)
end

function M.apply(state, to_mode, config, context)
  if type(state) ~= "table" then
    return result(false, {}, { make_diagnostic("core.mode_transition.state_not_table", "Mode state must be a table.", "state") })
  end
  local from_mode = state.active_mode or state.mode
  local check = M.can_transition(from_mode, to_mode, config, context)
  if not check.ok then
    return result(false, { state = copy_table(state, 0), transition = { from = from_mode, to = to_mode, applied = false } }, check.diagnostics)
  end
  local next_state = copy_table(state, 0)
  next_state.previous_mode = from_mode
  next_state.active_mode = to_mode
  next_state.mode = to_mode
  if to_mode == "dialogue_combat" then
    next_state.combat_mode = "dialogue_combat"
  elseif to_mode == "combat" and next_state.combat_mode == nil then
    next_state.combat_mode = "turn_based"
  elseif to_mode == "paused_planning" then
    next_state.paused = true
  else
    next_state.paused = false
  end
  local transition = {
    from = from_mode,
    to = to_mode,
    applied = true,
    reason = check.data.rule.reason,
    combat_mode = next_state.combat_mode,
    turn_mode = next_state.turn_mode
  }
  return result(true, { state = next_state, transition = transition }, check.diagnostics)
end

function M.dialogue_combat_profile(config)
  local profile = {
    mode = "dialogue_combat",
    tracks = { "hp", "morale", "trust", "suspicion", "focus" },
    allowed_entry_modes = { "dialogue", "combat" },
    allowed_exit_modes = { "dialogue", "combat", "exploration" },
    transition_rules = {}
  }
  local allowed = M.list_allowed("dialogue_combat", config)
  if allowed.ok then
    profile.transition_rules = allowed.data.allowed
  end
  return result(true, { profile = profile }, allowed.diagnostics)
end

function M.generate(input, ctx)
  local source = type(input) == "table" and input or {}
  local config = source.config
  local ok, diagnostics = M.validate_config(config)
  if not ok then
    return result(false, {}, diagnostics)
  end
  local rules, rule_diagnostics = build_rules(config)
  for index = 1, #rule_diagnostics do
    diagnostics[#diagnostics + 1] = rule_diagnostics[index]
  end
  local data = {
    rules = rules,
    modes = { "exploration", "combat", "dialogue", "dialogue_combat", "paused_planning" },
    dialogue_combat = M.dialogue_combat_profile(config).data.profile
  }
  if type(ctx) == "table" and type(ctx.note) == "string" then
    data.context_note = ctx.note
  end
  return result(true, data, diagnostics)
end

return M
