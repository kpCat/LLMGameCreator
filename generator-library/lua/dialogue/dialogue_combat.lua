local M = {}

M.manifest = {
  id = "dialogue/dialogue_combat/v1",
  version = "0.1.0",
  category = "dialogue",
  title = "Dialogue Combat",
  purpose = "Creates dialogue-combat encounter IR where choices affect morale, trust, suspicion, focus and related tracks.",
  capabilities = { "dialogue.combat.encounter_ir", "dialogue.choice.track_effects", "combat.dialogue_bridge" },
  input_schema = {
    type = "table",
    fields = {
      encounter_id = "lowercase slash id",
      participants = "optional participant array",
      tracks = "optional track initial values",
      choices = "array of dialogue-combat moves"
    }
  },
  output_schema = {
    type = "table",
    fields = {
      encounter = "dialogue-combat encounter IR",
      dialogue = "schema-compatible dialogue graph facade",
      summary = "track, choice and condition counts"
    }
  },
  config_schema = {
    track_min = "optional integer",
    track_max = "optional integer",
    default_track_values = "optional table",
    max_choices = "optional positive integer"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity_ui_ir" },
  supported_turn_modes = { "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local DEFAULT_TRACKS = { morale = 50, trust = 0, suspicion = 0, focus = 50 }
local TRACK_ORDER = { "morale", "trust", "suspicion", "focus" }

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function is_int(v)
  return type(v) == "number" and v % 1 == 0
end

local function is_positive_int(v)
  return is_int(v) and v > 0
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

local function clamp(value, lo, hi)
  if value < lo then
    return lo
  end
  if value > hi then
    return hi
  end
  return value
end

local function normalize_tracks(input_tracks, config)
  local out = {}
  local defaults = config.default_track_values or {}
  local lo = config.track_min or 0
  local hi = config.track_max or 100
  for i = 1, #TRACK_ORDER do
    local name = TRACK_ORDER[i]
    local value = DEFAULT_TRACKS[name]
    if type(defaults[name]) == "number" then
      value = defaults[name]
    end
    if type(input_tracks) == "table" and type(input_tracks[name]) == "number" then
      value = input_tracks[name]
    end
    out[name] = clamp(value, lo, hi)
  end
  if type(input_tracks) == "table" then
    for k, v in pairs(input_tracks) do
      if out[k] == nil and type(v) == "number" then
        out[k] = clamp(v, lo, hi)
      end
    end
  end
  return out
end

local function normalize_effect(effect)
  if type(effect) ~= "table" then
    return { target = "morale", op = "add", amount = 0 }
  end
  return {
    target = effect.target or effect.track or "morale",
    op = effect.op or "add",
    amount = type(effect.amount) == "number" and effect.amount or 0,
    value = effect.value
  }
end

local function normalize_effects(effects)
  local out = {}
  if type(effects) ~= "table" then
    return out
  end
  for i = 1, #effects do
    out[#out + 1] = normalize_effect(effects[i])
  end
  return out
end

local function normalize_choice(choice, index)
  local id = choice.id or ("move_" .. index)
  return {
    id = id,
    text = choice.text or choice.label or ("Dialogue move " .. index),
    stance = choice.stance or "neutral",
    conditions = copy_array(choice.conditions),
    effects = normalize_effects(choice.effects),
    cost = type(choice.cost) == "table" and choice.cost or {},
    cooldown_ticks = type(choice.cooldown_ticks) == "number" and choice.cooldown_ticks or 0,
    ends_encounter = choice.ends_encounter == true,
    tags = copy_array(choice.tags)
  }
end

local function validate_track_name(name, diagnostics, target)
  if type(name) ~= "string" or name == "" then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.track_name_invalid", "Track name must be a non-empty string.", target)
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    config = {}
  end
  if type(config) ~= "table" then
    return false, { diag("error", "dialogue.combat_config_not_table", "Config must be a table.", "config") }
  end
  if config.track_min ~= nil and not is_int(config.track_min) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.track_min_invalid", "track_min must be an integer.", "config.track_min")
  end
  if config.track_max ~= nil and not is_int(config.track_max) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.track_max_invalid", "track_max must be an integer.", "config.track_max")
  end
  if config.track_min ~= nil and config.track_max ~= nil and config.track_min >= config.track_max then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.track_bounds_invalid", "track_min must be lower than track_max.", "config.track_min")
  end
  if config.max_choices ~= nil and not is_positive_int(config.max_choices) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.max_combat_choices_invalid", "max_choices must be a positive integer.", "config.max_choices")
  end
  if config.default_track_values ~= nil and type(config.default_track_values) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.default_tracks_invalid", "default_track_values must be a table.", "config.default_track_values")
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
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.combat_input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  local encounter_id = input.encounter_id or input.id
  if not valid_slash_id(encounter_id) then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.encounter_id_invalid", "encounter_id must be a lowercase slash id.", "input.encounter_id")
  end
  if type(input.choices) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.combat_choices_missing", "input.choices must be an array.", "input.choices")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if config.max_choices ~= nil and #input.choices > config.max_choices then
    diagnostics[#diagnostics + 1] = diag("error", "dialogue.too_many_combat_choices", "Choice count exceeds config.max_choices.", "input.choices")
  end

  local tracks = normalize_tracks(input.tracks, config)
  local choices = {}
  local dialogue_choices = {}
  local effect_count = 0
  local condition_count = 0
  local lo = config.track_min or 0
  local hi = config.track_max or 100

  for i = 1, #input.choices do
    local raw = input.choices[i]
    local ct = "input.choices[" .. i .. "]"
    if type(raw) ~= "table" then
      diagnostics[#diagnostics + 1] = diag("error", "dialogue.combat_choice_not_table", "Dialogue-combat choice must be a table.", ct)
    else
      local choice = normalize_choice(raw, i)
      if type(choice.id) ~= "string" or choice.id == "" then
        diagnostics[#diagnostics + 1] = diag("error", "dialogue.combat_choice_id_invalid", "Choice id must be a non-empty string.", ct .. ".id")
      end
      for e = 1, #choice.effects do
        validate_track_name(choice.effects[e].target, diagnostics, ct .. ".effects[" .. e .. "].target")
        effect_count = effect_count + 1
      end
      if type(choice.conditions) == "table" then
        condition_count = condition_count + #choice.conditions
      end
      choices[#choices + 1] = choice
      dialogue_choices[#dialogue_choices + 1] = {
        id = choice.id,
        text = choice.text,
        to_node_id = choice.ends_encounter and nil or "exchange",
        ends_dialogue = choice.ends_encounter,
        conditions = choice.conditions,
        effects = choice.effects,
        tags = choice.tags,
        ui_hints = { stance = choice.stance, cooldown_ticks = choice.cooldown_ticks }
      }
    end
  end
  if #choices == 0 then
    diagnostics[#diagnostics + 1] = diag("warning", "dialogue.no_combat_moves", "No valid dialogue-combat moves were produced.", "input.choices")
    dialogue_choices[#dialogue_choices + 1] = { id = "end", text = "End exchange.", ends_dialogue = true }
  end

  local victory = input.victory_conditions or {
    { track = "trust", op = ">=", value = hi },
    { track = "suspicion", op = "<=", value = lo }
  }
  local defeat = input.defeat_conditions or {
    { track = "morale", op = "<=", value = lo },
    { track = "focus", op = "<=", value = lo }
  }
  local participants = copy_array(input.participants)
  if #participants == 0 then
    participants = {
      { id = "entity/player", role = "player" },
      { id = "entity/npc/opponent", role = "opponent" }
    }
  end

  local dialogue_id = input.dialogue_id or ("dialogue/" .. clean_token(encounter_id or "encounter"))
  local dialogue = {
    id = dialogue_id,
    title = input.title or "Dialogue Combat Encounter",
    speaker_id = input.speaker_id,
    entry_node_id = "exchange",
    nodes = {
      {
        id = "exchange",
        speaker_id = input.speaker_id,
        text = input.prompt or "Choose a dialogue-combat move.",
        tags = { "dialogue_combat" },
        choices = dialogue_choices,
        metadata = { encounter_id = encounter_id }
      }
    },
    metadata = { source = M.manifest.id }
  }

  local has_error = not ok_config
  for i = 1, #diagnostics do
    if diagnostics[i].severity == "error" then
      has_error = true
    end
  end
  return {
    ok = not has_error,
    data = {
      encounter = {
        id = encounter_id,
        title = input.title or encounter_id,
        combat_mode = "dialogue_combat",
        turn_mode = input.turn_mode or "turn_based",
        tracks = tracks,
        track_bounds = { min = lo, max = hi },
        participants = participants,
        choices = choices,
        victory_conditions = victory,
        defeat_conditions = defeat,
        metadata = { source = "dialogue_combat" }
      },
      dialogue = dialogue,
      summary = {
        module_id = M.manifest.id,
        track_count = 4,
        choice_count = #choices,
        effect_count = effect_count,
        condition_count = condition_count
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
