local M = {}

M.manifest = {
  id = "dialogue/procedural_npc_dialogue/v1",
  version = "0.1.0",
  category = "dialogue",
  title = "Procedural NPC Dialogue",
  purpose = "Creates compact dialogue graph IR from NPC profile, topics and facts without printing large dialogue corpora.",
  capabilities = { "dialogue.procedural_npc.generate", "dialogue.fact.topic_nodes", "npc.dialogue.seed" },
  input_schema = {
    type = "table",
    fields = {
      npc = "NPC profile with id/name/role/tone",
      dialogue_id = "optional lowercase slash id",
      facts = "array of known facts used as topics",
      topics = "optional explicit topic array"
    }
  },
  output_schema = {
    type = "table",
    fields = {
      dialogue = "schema-compatible dialogue graph IR",
      source_facts = "facts used to build topic nodes",
      summary = "counts and selected style metadata"
    }
  },
  config_schema = {
    max_facts = "optional positive integer",
    max_topics = "optional positive integer",
    include_greeting = "optional boolean",
    include_farewell = "optional boolean",
    default_tone = "optional string"
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

local function make_fact_id(fact, index)
  if type(fact) == "table" then
    if valid_slash_id(fact.id) then
      return fact.id
    end
    if type(fact.key) == "string" and fact.key ~= "" then
      return "fact/" .. clean_token(fact.key)
    end
    if type(fact.topic) == "string" and fact.topic ~= "" then
      return "fact/" .. clean_token(fact.topic)
    end
  end
  return "fact/generated_" .. index
end

local function fact_title(fact, index)
  if type(fact) == "table" then
    return fact.title or fact.topic or fact.key or ("Topic " .. index)
  end
  if type(fact) == "string" then
    return fact
  end
  return "Topic " .. index
end

local function fact_summary(fact)
  if type(fact) == "table" then
    return fact.summary or fact.text or fact.value or "No details are available yet."
  end
  if type(fact) == "string" then
    return fact
  end
  return "No details are available yet."
end

local function normalize_facts(facts, limit)
  local out = {}
  if type(facts) ~= "table" then
    return out
  end
  local max_count = limit or #facts
  for i = 1, #facts do
    if #out >= max_count then
      break
    end
    local fact = facts[i]
    out[#out + 1] = {
      id = make_fact_id(fact, i),
      title = fact_title(fact, i),
      summary = fact_summary(fact),
      tags = type(fact) == "table" and copy_array(fact.tags) or {}
    }
  end
  return out
end

local function normalize_topics(topics, limit)
  local out = {}
  if type(topics) ~= "table" then
    return out
  end
  local max_count = limit or #topics
  for i = 1, #topics do
    if #out >= max_count then
      break
    end
    local topic = topics[i]
    if type(topic) == "table" then
      out[#out + 1] = {
        id = topic.id or ("topic/" .. clean_token(topic.title or topic.name or ("topic_" .. i))),
        title = topic.title or topic.name or ("Topic " .. i),
        summary = topic.summary or topic.text or "No details are available yet.",
        tags = copy_array(topic.tags)
      }
    elseif type(topic) == "string" then
      out[#out + 1] = {
        id = "topic/" .. clean_token(topic),
        title = topic,
        summary = "Ask about " .. topic .. ".",
        tags = {}
      }
    end
  end
  return out
end

local function append_unique_topics(target, source, seen)
  for i = 1, #source do
    local id = source[i].id
    if not seen[id] then
      target[#target + 1] = source[i]
      seen[id] = true
    end
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    config = {}
  end
  if type(config) ~= "table" then
    return false, { diag("error", "dialogue.proc_config_not_table", "Config must be a table.", "config") }
  end
  if config.max_facts ~= nil and not is_positive_int(config.max_facts) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.max_facts_invalid", "max_facts must be a positive integer.", "config.max_facts")
  end
  if config.max_topics ~= nil and not is_positive_int(config.max_topics) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.max_topics_invalid", "max_topics must be a positive integer.", "config.max_topics")
  end
  if config.default_tone ~= nil and type(config.default_tone) ~= "string" then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.default_tone_invalid", "default_tone must be a string.", "config.default_tone")
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
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.proc_input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  local npc = input.npc or {}
  if type(npc) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.npc_not_table", "input.npc must be a table.", "input.npc")
    npc = {}
  end
  local npc_id = npc.id or input.speaker_id or "entity/npc/generated"
  if not valid_slash_id(npc_id) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.npc_id_invalid", "NPC id must be a lowercase slash id.", "input.npc.id")
  end
  local dialogue_id = input.dialogue_id or ("dialogue/" .. clean_token(npc.name or npc_id))
  if not valid_slash_id(dialogue_id) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.proc_dialogue_id_invalid", "dialogue_id must be a lowercase slash id.", "input.dialogue_id")
  end

  local facts = normalize_facts(input.facts, config.max_facts)
  local explicit_topics = normalize_topics(input.topics, config.max_topics)
  local topics = {}
  local seen = {}
  append_unique_topics(topics, explicit_topics, seen)
  append_unique_topics(topics, facts, seen)
  if config.max_topics ~= nil and #topics > config.max_topics then
    local trimmed = {}
    for i = 1, config.max_topics do
      trimmed[#trimmed + 1] = topics[i]
    end
    topics = trimmed
  end
  if #topics == 0 then
    diagnostics[#diagnostics + 1] = diag("warning", "dialogue.no_topics", "No facts or topics were provided; generated only a greeting and farewell.", "input.facts")
  end

  local tone = npc.tone or config.default_tone or "neutral"
  local speaker_name = npc.name or "NPC"
  local nodes = {}
  local start_choices = {}
  local include_greeting = config.include_greeting ~= false
  local include_farewell = config.include_farewell ~= false

  for i = 1, #topics do
    local node_id = "topic_" .. i
    start_choices[#start_choices + 1] = {
      id = "ask_topic_" .. i,
      text = "Ask about " .. topics[i].title,
      to_node_id = node_id,
      conditions = {},
      effects = {
        { target = "fact", key = topics[i].id, op = "mark_discussed", value = true }
      }
    }
    nodes[#nodes + 1] = {
      id = node_id,
      speaker_id = npc_id,
      text = topics[i].summary,
      tags = topics[i].tags,
      choices = {
        { id = "back", text = "Ask something else.", to_node_id = "start" },
        { id = "end", text = "End the conversation.", ends_dialogue = true }
      },
      metadata = { source_topic_id = topics[i].id }
    }
  end

  if include_farewell then
    start_choices[#start_choices + 1] = { id = "farewell", text = "Goodbye.", ends_dialogue = true }
  end
  if #start_choices == 0 then
    start_choices[#start_choices + 1] = { id = "continue", text = "Continue.", ends_dialogue = true }
  end
  local greeting_text = include_greeting and (speaker_name .. " is ready to talk. Tone: " .. tone .. ".") or "Conversation started."
  local start_node = {
    id = "start",
    speaker_id = npc_id,
    text = greeting_text,
    tags = { "procedural", tone },
    choices = start_choices,
    metadata = { role = npc.role or "npc", procedural_source = M.manifest.id }
  }
  local ordered_nodes = { start_node }
  for i = 1, #nodes do
    ordered_nodes[#ordered_nodes + 1] = nodes[i]
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
        title = "Conversation with " .. speaker_name,
        speaker_id = npc_id,
        entry_node_id = "start",
        nodes = ordered_nodes,
        metadata = { source = "procedural_npc_dialogue", tone = tone }
      },
      source_facts = facts,
      source_topics = topics,
      summary = {
        module_id = M.manifest.id,
        topic_count = #topics,
        node_count = #ordered_nodes,
        choice_count = #start_choices + (#topics * 2)
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
