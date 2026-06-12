local M = {}

M.manifest = {
  id = "dialogue/dialogue_schema/v1",
  version = "0.1.0",
  category = "dialogue",
  title = "Dialogue Schema",
  purpose = "Normalizes static dialogue graphs into compact dialogue IR with nodes, choices, conditions and effects.",
  capabilities = { "dialogue.schema.define", "dialogue.static_graph.normalize", "dialogue.choice.validate" },
  input_schema = {
    type = "table",
    fields = {
      dialogue_id = "lowercase slash id",
      speaker_id = "optional lowercase slash id",
      entry_node_id = "optional node id, defaults to config.default_entry_node_id or start",
      nodes = "array of dialogue node records"
    }
  },
  output_schema = {
    type = "table",
    fields = {
      dialogue = "normalized dialogue graph IR",
      indexes = "node and choice lookup metadata",
      summary = "counts and validation metadata"
    }
  },
  config_schema = {
    max_nodes = "optional positive integer",
    max_choices_per_node = "optional positive integer",
    default_entry_node_id = "optional node id",
    allowed_effect_targets = "optional array of target track names"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity_ui_ir" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local DEFAULT_EFFECT_TARGETS = {
  fact = true,
  quest_state = true,
  morale = true,
  trust = true,
  suspicion = true,
  focus = true,
  hp = true,
  status = true
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

local function shallow_json_copy(src)
  local out = {}
  if type(src) ~= "table" then
    return out
  end
  for k, v in pairs(src) do
    if type(v) ~= "function" and type(v) ~= "thread" and type(v) ~= "userdata" then
      out[k] = v
    end
  end
  return out
end

local function effect_target_map(config)
  local allowed = {}
  if type(config.allowed_effect_targets) == "table" and #config.allowed_effect_targets > 0 then
    for i = 1, #config.allowed_effect_targets do
      if type(config.allowed_effect_targets[i]) == "string" then
        allowed[config.allowed_effect_targets[i]] = true
      end
    end
    return allowed
  end
  for k, v in pairs(DEFAULT_EFFECT_TARGETS) do
    allowed[k] = v
  end
  return allowed
end

local function normalize_condition(condition)
  if type(condition) ~= "table" then
    return { kind = "invalid" }
  end
  local kind = condition.kind or condition.type or "fact"
  local op = condition.op or condition.operator or "equals"
  return {
    kind = kind,
    key = condition.key or condition.id or condition.fact_id or condition.quest_id,
    op = op,
    value = condition.value,
    invert = condition.invert == true
  }
end

local function normalize_effect(effect)
  if type(effect) ~= "table" then
    return { target = "invalid", op = "none" }
  end
  return {
    target = effect.target or effect.kind or "fact",
    key = effect.key or effect.id or effect.fact_id or effect.quest_id,
    op = effect.op or effect.operator or "set",
    value = effect.value,
    amount = effect.amount
  }
end

local function normalize_conditions(src)
  local out = {}
  if type(src) ~= "table" then
    return out
  end
  for i = 1, #src do
    out[#out + 1] = normalize_condition(src[i])
  end
  return out
end

local function normalize_effects(src)
  local out = {}
  if type(src) ~= "table" then
    return out
  end
  for i = 1, #src do
    out[#out + 1] = normalize_effect(src[i])
  end
  return out
end

local function validate_conditions(conditions, diagnostics, target)
  if conditions == nil then
    return
  end
  if type(conditions) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.conditions_not_array", "Conditions must be an array when provided.", target)
    return
  end
  for i = 1, #conditions do
    local c = conditions[i]
    local ct = target .. "[" .. i .. "]"
    if type(c) ~= "table" then
      diagnostics[#diagnostics + 1] = diag("error", "dialogue.condition_not_table", "Condition must be a table.", ct)
    else
      local kind = c.kind or c.type
      if kind ~= nil and type(kind) ~= "string" then
        diagnostics[#diagnostics + 1] = diag("error", "dialogue.condition_kind_invalid", "Condition kind must be a string.", ct .. ".kind")
      end
    end
  end
end

local function validate_effects(effects, diagnostics, target, allowed_targets)
  if effects == nil then
    return
  end
  if type(effects) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.effects_not_array", "Effects must be an array when provided.", target)
    return
  end
  for i = 1, #effects do
    local e = effects[i]
    local et = target .. "[" .. i .. "]"
    if type(e) ~= "table" then
      diagnostics[#diagnostics + 1] = diag("error", "dialogue.effect_not_table", "Effect must be a table.", et)
    else
      local target_name = e.target or e.kind or "fact"
      if type(target_name) ~= "string" or target_name == "" then
        diagnostics[#diagnostics + 1] = diag("error", "dialogue.effect_target_invalid", "Effect target must be a non-empty string.", et .. ".target")
      elseif not allowed_targets[target_name] then
        diagnostics[#diagnostics + 1] = diag("warning", "dialogue.effect_target_unknown", "Effect target is not in the allowed target list.", et .. ".target")
      end
    end
  end
end

local function normalize_choice(choice, index)
  local id = choice.id
  if type(id) ~= "string" or id == "" then
    id = "choice_" .. index
  end
  return {
    id = id,
    text = choice.text or choice.label or "Continue",
    to_node_id = choice.to_node_id or choice.to or choice.next_node_id,
    ends_dialogue = choice.ends_dialogue == true,
    conditions = normalize_conditions(choice.conditions),
    effects = normalize_effects(choice.effects),
    tags = copy_array(choice.tags),
    ui_hints = shallow_json_copy(choice.ui_hints)
  }
end

local function normalize_node(node, index, config)
  local id = node.id
  if type(id) ~= "string" or id == "" then
    id = "node_" .. index
  end
  local speaker = node.speaker_id or node.speaker or config.default_speaker_id
  local choices = {}
  if type(node.choices) == "table" then
    for i = 1, #node.choices do
      choices[#choices + 1] = normalize_choice(node.choices[i], i)
    end
  end
  return {
    id = id,
    speaker_id = speaker,
    text = node.text or "",
    tags = copy_array(node.tags),
    conditions = normalize_conditions(node.conditions),
    choices = choices,
    metadata = shallow_json_copy(node.metadata)
  }
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    config = {}
  end
  if type(config) ~= "table" then
    return false, { diag("error", "dialogue.config_not_table", "Config must be a table.", "config") }
  end
  if config.max_nodes ~= nil and not is_positive_int(config.max_nodes) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.max_nodes_invalid", "max_nodes must be a positive integer.", "config.max_nodes")
  end
  if config.max_choices_per_node ~= nil and not is_positive_int(config.max_choices_per_node) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.max_choices_invalid", "max_choices_per_node must be a positive integer.", "config.max_choices_per_node")
  end
  if config.default_entry_node_id ~= nil and not valid_node_id(config.default_entry_node_id) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.default_entry_invalid", "default_entry_node_id must be a lowercase node id.", "config.default_entry_node_id")
  end
  if config.default_speaker_id ~= nil and not valid_slash_id(config.default_speaker_id) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.default_speaker_invalid", "default_speaker_id must be a lowercase slash id.", "config.default_speaker_id")
  end
  if config.allowed_effect_targets ~= nil and type(config.allowed_effect_targets) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.allowed_targets_invalid", "allowed_effect_targets must be an array.", "config.allowed_effect_targets")
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
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local dialogue_id = input.dialogue_id or input.id
  if not valid_slash_id(dialogue_id) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.id_invalid", "dialogue_id must be a lowercase slash id.", "input.dialogue_id")
  end
  if input.speaker_id ~= nil and not valid_slash_id(input.speaker_id) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.speaker_invalid", "speaker_id must be a lowercase slash id.", "input.speaker_id")
  end
  if type(input.nodes) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.nodes_missing", "input.nodes must be an array.", "input.nodes")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if config.max_nodes ~= nil and #input.nodes > config.max_nodes then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.too_many_nodes", "Node count exceeds config.max_nodes.", "input.nodes")
  end

  local allowed_targets = effect_target_map(config)
  local nodes = {}
  local node_index = {}
  local choice_index = {}
  local edges = {}
  local node_order = {}

  for i = 1, #input.nodes do
    local raw = input.nodes[i]
    local nt = "input.nodes[" .. i .. "]"
    if type(raw) ~= "table" then
      diagnostics[#diagnostics + 1] = diag("error", "dialogue.node_not_table", "Node must be a table.", nt)
    else
      local node = normalize_node(raw, i, { default_speaker_id = input.speaker_id })
      if not valid_node_id(node.id) then
        diagnostics[#diagnostics + 1] = diag("error", "dialogue.node_id_invalid", "Node id must be lowercase letters, digits or underscore.", nt .. ".id")
      elseif node_index[node.id] ~= nil then
        diagnostics[#diagnostics + 1] = diag("error", "dialogue.node_id_duplicate", "Node id must be unique within a dialogue graph.", nt .. ".id")
      else
        node_index[node.id] = #nodes + 1
        node_order[#node_order + 1] = node.id
      end
      if type(node.text) ~= "string" then
        diagnostics[#diagnostics + 1] = diag("error", "dialogue.node_text_invalid", "Node text must be a string.", nt .. ".text")
      end
      validate_conditions(raw.conditions, diagnostics, nt .. ".conditions")
      if type(raw.choices) ~= "table" and raw.choices ~= nil then
        diagnostics[#diagnostics + 1] = diag("error", "dialogue.choices_not_array", "Node choices must be an array when provided.", nt .. ".choices")
      end
      if config.max_choices_per_node ~= nil and #node.choices > config.max_choices_per_node then
        diagnostics[#diagnostics + 1] = diag("error", "dialogue.too_many_choices", "Choice count exceeds config.max_choices_per_node.", nt .. ".choices")
      end
      local per_node_choice_ids = {}
      for c = 1, #node.choices do
        local choice = node.choices[c]
        local ct = nt .. ".choices[" .. c .. "]"
        if type(choice.id) ~= "string" or choice.id == "" then
          diagnostics[#diagnostics + 1] = diag("error", "dialogue.choice_id_invalid", "Choice id must be a non-empty string.", ct .. ".id")
        elseif per_node_choice_ids[choice.id] then
          diagnostics[#diagnostics + 1] = diag("error", "dialogue.choice_id_duplicate", "Choice id must be unique within a node.", ct .. ".id")
        else
          per_node_choice_ids[choice.id] = true
          choice_index[node.id .. "/" .. choice.id] = { node_id = node.id, choice_id = choice.id }
        end
        if type(choice.text) ~= "string" then
          diagnostics[#diagnostics + 1] = diag("error", "dialogue.choice_text_invalid", "Choice text must be a string.", ct .. ".text")
        end
        if choice.to_node_id ~= nil and not valid_node_id(choice.to_node_id) then
          diagnostics[#diagnostics + 1] = diag("error", "dialogue.choice_target_invalid", "Choice target node id must be a lowercase node id.", ct .. ".to_node_id")
        end
        if choice.to_node_id == nil and choice.ends_dialogue ~= true then
          diagnostics[#diagnostics + 1] = diag("warning", "dialogue.choice_no_target", "Choice has no target and does not explicitly end dialogue.", ct)
        end
        validate_conditions(raw.choices and raw.choices[c] and raw.choices[c].conditions, diagnostics, ct .. ".conditions")
        validate_effects(raw.choices and raw.choices[c] and raw.choices[c].effects, diagnostics, ct .. ".effects", allowed_targets)
        if choice.to_node_id ~= nil then
          edges[#edges + 1] = { from_node_id = node.id, choice_id = choice.id, to_node_id = choice.to_node_id }
        end
      end
      nodes[#nodes + 1] = node
    end
  end

  local entry = input.entry_node_id or config.default_entry_node_id or "start"
  if not valid_node_id(entry) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.entry_invalid", "Entry node id must be a lowercase node id.", "input.entry_node_id")
  elseif node_index[entry] == nil then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.entry_missing", "Entry node does not exist in the node list.", "input.entry_node_id")
  end
  for i = 1, #edges do
    if node_index[edges[i].to_node_id] == nil then
      diagnostics[#diagnostics + 1] = diag("error", "dialogue.edge_target_missing", "Choice points to a missing node.", "edges[" .. i .. "].to_node_id")
    end
  end

  local has_error = not ok_config
  for i = 1, #diagnostics do
    if diagnostics[i].severity == "error" then
      has_error = true
    end
  end

  local data = {
    dialogue = {
      id = dialogue_id,
      title = input.title or dialogue_id,
      speaker_id = input.speaker_id,
      entry_node_id = entry,
      nodes = nodes,
      metadata = shallow_json_copy(input.metadata)
    },
    indexes = {
      node_index = node_index,
      choice_index = choice_index,
      node_order = node_order,
      edges = edges
    },
    summary = {
      module_id = M.manifest.id,
      node_count = #nodes,
      edge_count = #edges,
      supports_dialogue_combat = input.supports_dialogue_combat == true
    }
  }
  return { ok = not has_error, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
