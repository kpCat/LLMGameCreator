local M = {}

M.manifest = {
  id = "dialogue/fact_based_dialogue/v1",
  version = "0.1.0",
  category = "dialogue",
  title = "Fact Based Dialogue",
  purpose = "Builds dialogue branches and choices controlled by facts, quest states and known world state flags.",
  capabilities = { "dialogue.fact_based.generate", "dialogue.quest_state.branch", "dialogue.conditions.effects" },
  input_schema = {
    type = "table",
    fields = {
      dialogue_id = "lowercase slash id",
      speaker_id = "optional lowercase slash id",
      base_nodes = "optional static node array",
      rules = "array of fact/quest branching rules"
    }
  },
  output_schema = {
    type = "table",
    fields = {
      dialogue = "schema-compatible dialogue graph IR with conditional branches",
      rules = "normalized rules",
      summary = "rule and condition counts"
    }
  },
  config_schema = {
    max_rules = "optional positive integer",
    emit_missing_fact_warnings = "optional boolean",
    default_unknown_response = "optional string"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity_ui_ir" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function is_positive_int(v)
  return type(v) == "number" and v % 1 == 0 and v > 0
end

local function valid_slash_id(id)
  if type(id) ~= "string" then
    return false
  end
  if id == "" or string.sub(id, 1, 1) == "/" or string.sub(id, #id, #id) == "/" then
    return false
  end
  if string.find(id, "//", 1, true) then
    return false
  end
  return string.match(id, "^[a-z0-9_]+(/[a-z0-9_]+)*$") ~= nil
end

local function valid_node_id(id)
  return type(id) == "string" and string.match(id, "^[a-z0-9_]+$") ~= nil
end

local function copy_array(src)
  local out = {}
  if type(src) ~= "table" then
    return out
  end
  for i = 1, #src do
    out[#out + 1] = src[i]
  end
  return out
end

local function clean_token(text)
  if type(text) ~= "string" then
    return "item"
  end
  local lower = string.lower(text)
  local out = ""
  local previous_underscore = false
  for i = 1, #lower do
    local ch = string.sub(lower, i, i)
    if string.match(ch, "[a-z0-9]") then
      out = out .. ch
      previous_underscore = false
    elseif not previous_underscore then
      out = out .. "_"
      previous_underscore = true
    end
  end
  out = string.gsub(out, "^_+", "")
  out = string.gsub(out, "_+$", "")
  if out == "" then
    out = "item"
  end
  return out
end

local function condition_fact(key, expected)
  return { kind = "fact", key = key, op = "equals", value = expected == nil and true or expected }
end

local function condition_quest(quest_id, state)
  return { kind = "quest_state", key = quest_id, op = "equals", value = state }
end

local function normalize_rule(rule, index)
  local id = rule.id or ("rule_" .. index)
  local node_id = rule.node_id or ("rule_" .. index)
  local conditions = copy_array(rule.conditions)
  if type(rule.required_facts) == "table" then
    for i = 1, #rule.required_facts do
      conditions[#conditions + 1] = condition_fact(rule.required_facts[i], true)
    end
  end
  if type(rule.blocked_by_facts) == "table" then
    for i = 1, #rule.blocked_by_facts do
      local c = condition_fact(rule.blocked_by_facts[i], true)
      c.invert = true
      conditions[#conditions + 1] = c
    end
  end
  if type(rule.quest_states) == "table" then
    for quest_id, state in pairs(rule.quest_states) do
      conditions[#conditions + 1] = condition_quest(quest_id, state)
    end
  end
  local effects = copy_array(rule.effects)
  if type(rule.set_facts) == "table" then
    for i = 1, #rule.set_facts do
      effects[#effects + 1] = { target = "fact", key = rule.set_facts[i], op = "set", value = true }
    end
  end
  if type(rule.quest_effects) == "table" then
    for quest_id, state in pairs(rule.quest_effects) do
      effects[#effects + 1] = { target = "quest_state", key = quest_id, op = "set", value = state }
    end
  end
  return {
    id = id,
    node_id = node_id,
    title = rule.title or rule.prompt or ("Rule " .. index),
    text = rule.text or rule.response or "The NPC reacts to what is currently known.",
    choice_text = rule.choice_text or rule.prompt or rule.title or ("Ask about " .. id),
    conditions = conditions,
    effects = effects,
    tags = copy_array(rule.tags),
    priority = type(rule.priority) == "number" and rule.priority or 0
  }
end

local function known_fact_map(input)
  local map = {}
  if type(input.known_facts) == "table" then
    for i = 1, #input.known_facts do
      if type(input.known_facts[i]) == "string" then
        map[input.known_facts[i]] = true
      elseif type(input.known_facts[i]) == "table" and type(input.known_facts[i].id) == "string" then
        map[input.known_facts[i].id] = true
      end
    end
  end
  if type(input.facts) == "table" then
    for i = 1, #input.facts do
      if type(input.facts[i]) == "string" then
        map[input.facts[i]] = true
      elseif type(input.facts[i]) == "table" and type(input.facts[i].id) == "string" then
        map[input.facts[i].id] = true
      end
    end
  end
  return map
end

local function validate_conditions(conditions, diagnostics, target)
  if type(conditions) ~= "table" then
    return
  end
  for i = 1, #conditions do
    local c = conditions[i]
    if type(c) ~= "table" then
      diagnostics[#diagnostics + 1] = diag("error", "dialogue.fact_condition_not_table", "Condition must be a table.", target .. "[" .. i .. "]")
    elseif type(c.kind or c.type or "fact") ~= "string" then
      diagnostics[#diagnostics + 1] = diag("error", "dialogue.fact_condition_kind_invalid", "Condition kind must be a string.", target .. "[" .. i .. "].kind")
    end
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    config = {}
  end
  if type(config) ~= "table" then
    return false, { diag("error", "dialogue.fact_config_not_table", "Config must be a table.", "config") }
  end
  if config.max_rules ~= nil and not is_positive_int(config.max_rules) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.fact_max_rules_invalid", "max_rules must be a positive integer.", "config.max_rules")
  end
  if config.default_unknown_response ~= nil and type(config.default_unknown_response) ~= "string" then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.unknown_response_invalid", "default_unknown_response must be a string.", "config.default_unknown_response")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  ctx = ctx or {}
  local config = ctx.config or {}
  local ok_config, config_diags = M.validate_config(config)
  for i = 1, #config_diags do
    diagnostics[#diagnostics + 1] = config_diags[i]
  end
  if type(input) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.fact_input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  local dialogue_id = input.dialogue_id or input.id
  if not valid_slash_id(dialogue_id) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.fact_dialogue_id_invalid", "dialogue_id must be a lowercase slash id.", "input.dialogue_id")
  end
  if input.speaker_id ~= nil and not valid_slash_id(input.speaker_id) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.fact_speaker_invalid", "speaker_id must be a lowercase slash id.", "input.speaker_id")
  end
  if type(input.rules) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.rules_missing", "input.rules must be an array.", "input.rules")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if config.max_rules ~= nil and #input.rules > config.max_rules then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.too_many_rules", "Rule count exceeds config.max_rules.", "input.rules")
  end

  local rules = {}
  local nodes = {}
  if type(input.base_nodes) == "table" then
    for i = 1, #input.base_nodes do
      nodes[#nodes + 1] = input.base_nodes[i]
    end
  end
  local start_choices = {}
  local known_facts = known_fact_map(input)
  local missing_fact_count = 0

  for i = 1, #input.rules do
    local raw = input.rules[i]
    local rt = "input.rules[" .. i .. "]"
    if type(raw) ~= "table" then
      diagnostics[#diagnostics + 1] = diag("error", "dialogue.rule_not_table", "Rule must be a table.", rt)
    else
      local rule = normalize_rule(raw, i)
      if type(rule.id) ~= "string" or rule.id == "" then
        diagnostics[#diagnostics + 1] = diag("error", "dialogue.rule_id_invalid", "Rule id must be a non-empty string.", rt .. ".id")
      end
      if not valid_node_id(rule.node_id) then
        rule.node_id = "rule_" .. i .. "_" .. clean_token(rule.id)
      end
      validate_conditions(rule.conditions, diagnostics, rt .. ".conditions")
      if config.emit_missing_fact_warnings == true and type(raw.required_facts) == "table" then
        for f = 1, #raw.required_facts do
          if not known_facts[raw.required_facts[f]] then
            missing_fact_count = missing_fact_count + 1
            diagnostics[#diagnostics + 1] = diag("warning", "dialogue.required_fact_unknown", "A required fact is not listed in known_facts/facts.", rt .. ".required_facts[" .. f .. "]")
          end
        end
      end
      rules[#rules + 1] = rule
      start_choices[#start_choices + 1] = {
        id = "choose_" .. rule.id,
        text = rule.choice_text,
        to_node_id = rule.node_id,
        conditions = rule.conditions,
        effects = {}
      }
      nodes[#nodes + 1] = {
        id = rule.node_id,
        speaker_id = input.speaker_id,
        text = rule.text,
        tags = rule.tags,
        conditions = rule.conditions,
        choices = {
          { id = "apply", text = "Continue.", to_node_id = "start", effects = rule.effects },
          { id = "end", text = "End the conversation.", ends_dialogue = true }
        },
        metadata = { source_rule_id = rule.id, priority = rule.priority }
      }
    end
  end

  local start_text = input.start_text or config.default_unknown_response or "What do you want to discuss?"
  local start_node = {
    id = "start",
    speaker_id = input.speaker_id,
    text = start_text,
    tags = { "fact_based" },
    choices = start_choices,
    metadata = { source = M.manifest.id }
  }
  if #start_choices == 0 then
    start_node.choices = { { id = "end", text = "There is nothing to discuss.", ends_dialogue = true } }
  end
  local ordered_nodes = { start_node }
  for i = 1, #nodes do
    if nodes[i].id ~= "start" then
      ordered_nodes[#ordered_nodes + 1] = nodes[i]
    end
  end

  local has_error = not ok_config
  for i = 1, #diagnostics do
    if diagnostics[i].severity == "error" then
      has_error = true
    end
  end
  return {
    ok = not has_error,
    data = {
      dialogue = {
        id = dialogue_id,
        title = input.title or dialogue_id,
        speaker_id = input.speaker_id,
        entry_node_id = "start",
        nodes = ordered_nodes,
        metadata = { source = "fact_based_dialogue" }
      },
      rules = rules,
      summary = {
        module_id = M.manifest.id,
        rule_count = #rules,
        node_count = #ordered_nodes,
        missing_fact_warning_count = missing_fact_count
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
