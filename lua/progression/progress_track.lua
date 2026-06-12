local M = {}

M.manifest = {
  id = "progression/progress_track/v1",
  version = "0.1.0",
  category = "progression",
  title = "Abstract Progress Track",
  purpose = "Generate abstract progress tracks for reputation, research, faction favor, suspicion, morale and similar non-XP progression.",
  capabilities = { "progression.track.generate", "progression.abstract_progress", "quest.progress_reference" },
  input_schema = {
    tracks = "array of progress track definitions or a single track definition"
  },
  output_schema = {
    tracks = "normalized progress track definitions",
    indexes = "lookup maps by id, domain and polarity",
    summary = "counts and validation result"
  },
  config_schema = {
    allowed_domains = "optional array of domain tokens",
    max_tracks = "optional positive integer",
    max_stages_per_track = "optional positive integer"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "unity_ui_ir", "codegen_ir" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local DEFAULT_DOMAINS = { "reputation", "research", "faction_favor", "suspicion", "morale", "trust", "influence", "threat" }

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function add(out, severity, code, message, target)
  out[#out + 1] = diag(severity, code, message, target)
end

local function is_array(value)
  if type(value) ~= "table" then return false end
  local count = 0
  for key, _ in pairs(value) do
    if type(key) ~= "number" or key < 1 or key % 1 ~= 0 then return false end
    count = count + 1
  end
  for index = 1, count do if value[index] == nil then return false end end
  return true
end

local function id_ok(value)
  if type(value) ~= "string" or value == "" then return false end
  if value:sub(1, 1) == "/" or value:sub(-1) == "/" or value:find("//", 1, true) then return false end
  return value:match("^[a-z0-9_/%-%.]+$") ~= nil
end

local function token_ok(value)
  return type(value) == "string" and value ~= "" and value:match("^[a-z0-9_%-%.]+$") ~= nil
end

local function positive_integer(value)
  return type(value) == "number" and value > 0 and value % 1 == 0
end

local function number_or_default(value, fallback)
  if type(value) == "number" then return value end
  return fallback
end

local function list_to_set(list)
  local result = {}
  if type(list) == "table" then
    for index = 1, #list do if type(list[index]) == "string" then result[list[index]] = true end end
  end
  return result
end

local function clone_array(value)
  local result = {}
  if type(value) == "table" then for index = 1, #value do result[#result + 1] = value[index] end end
  return result
end

local function normalize_stage(value, diagnostics, target, index)
  if type(value) ~= "table" then
    add(diagnostics, "error", "track.stage_not_table", "Progress stage must be a table.", target)
    return { id = "stage_" .. index, threshold = index, label = "Stage " .. index, effects = {} }
  end
  local id = value.id or ("stage_" .. index)
  if not token_ok(id) then
    add(diagnostics, "warning", "track.invalid_stage_id", "Invalid stage id replaced by generated id.", target .. ".id")
    id = "stage_" .. index
  end
  return {
    id = id,
    threshold = number_or_default(value.threshold or value.at, index),
    label = type(value.label) == "string" and value.label or id,
    description = type(value.description) == "string" and value.description or "",
    effects = clone_array(value.effects),
    ui = type(value.ui) == "table" and value.ui or {}
  }
end

local function normalize_track(value, cfg, diagnostics, target)
  if type(value) ~= "table" then
    add(diagnostics, "error", "track.definition_not_table", "Progress track must be a table.", target)
    return nil
  end
  local id = value.id
  if not id_ok(id) then
    add(diagnostics, "error", "track.invalid_id", "Progress track id must be a lowercase slash id.", target .. ".id")
    return nil
  end
  local domain = value.domain or value.kind or "reputation"
  if not cfg.allowed_domains[domain] then
    add(diagnostics, "error", "track.domain_not_allowed", "Progress track domain is not allowed by config.", target .. ".domain")
    domain = "reputation"
  end
  local polarity = value.polarity or "positive"
  if polarity ~= "positive" and polarity ~= "negative" and polarity ~= "mixed" then
    add(diagnostics, "warning", "track.invalid_polarity", "Track polarity replaced by positive.", target .. ".polarity")
    polarity = "positive"
  end
  local min_value = number_or_default(value.min or value.minimum, 0)
  local max_value = number_or_default(value.max or value.maximum, 100)
  if max_value <= min_value then
    add(diagnostics, "error", "track.invalid_range", "Track max must be greater than min.", target)
    max_value = min_value + 100
  end
  local raw_stages = value.stages or {}
  if not is_array(raw_stages) then
    add(diagnostics, "warning", "track.stages_not_array", "Track stages must be an array; generated default stages.", target .. ".stages")
    raw_stages = {}
  end
  if #raw_stages == 0 then
    raw_stages = {
      { id = "low", threshold = min_value, label = "Low" },
      { id = "mid", threshold = (min_value + max_value) / 2, label = "Medium" },
      { id = "high", threshold = max_value, label = "High" }
    }
  end
  local stages = {}
  local limit = #raw_stages
  if limit > cfg.max_stages_per_track then
    add(diagnostics, "warning", "track.stages_truncated", "Track stages were truncated by max_stages_per_track.", target .. ".stages")
    limit = cfg.max_stages_per_track
  end
  local last_threshold = min_value - 1
  for index = 1, limit do
    local stage = normalize_stage(raw_stages[index], diagnostics, target .. ".stages[" .. index .. "]", index)
    if stage.threshold < min_value or stage.threshold > max_value then
      add(diagnostics, "warning", "track.stage_out_of_range", "Stage threshold is outside track range.", target .. ".stages[" .. index .. "].threshold")
    end
    if stage.threshold < last_threshold then
      add(diagnostics, "warning", "track.stage_not_monotonic", "Stage thresholds should be ordered ascending.", target .. ".stages[" .. index .. "].threshold")
    end
    last_threshold = stage.threshold
    stages[#stages + 1] = stage
  end
  return {
    id = id,
    title = type(value.title) == "string" and value.title or id,
    domain = domain,
    polarity = polarity,
    range = { min = min_value, max = max_value, initial = number_or_default(value.initial, min_value) },
    stages = stages,
    decay = type(value.decay) == "table" and value.decay or { enabled = false },
    source_refs = clone_array(value.source_refs),
    effect_refs = clone_array(value.effect_refs),
    ui = type(value.ui) == "table" and value.ui or {}
  }
end

local function make_config(config)
  local cfg = type(config) == "table" and config or {}
  return {
    allowed_domains = list_to_set(cfg.allowed_domains or DEFAULT_DOMAINS),
    max_tracks = positive_integer(cfg.max_tracks) and cfg.max_tracks or 32,
    max_stages_per_track = positive_integer(cfg.max_stages_per_track) and cfg.max_stages_per_track or 12
  }
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and type(config) ~= "table" then
    add(diagnostics, "error", "track.config_not_table", "Config must be a table when provided.", "config")
  elseif type(config) == "table" then
    if config.allowed_domains ~= nil and not is_array(config.allowed_domains) then
      add(diagnostics, "error", "track.allowed_domains_not_array", "allowed_domains must be an array.", "config.allowed_domains")
    end
    if config.max_tracks ~= nil and not positive_integer(config.max_tracks) then
      add(diagnostics, "error", "track.invalid_max_tracks", "max_tracks must be a positive integer.", "config.max_tracks")
    end
    if config.max_stages_per_track ~= nil and not positive_integer(config.max_stages_per_track) then
      add(diagnostics, "error", "track.invalid_max_stages", "max_stages_per_track must be a positive integer.", "config.max_stages_per_track")
    end
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local cfg = make_config(ctx and ctx.config or nil)
  local source = input and (input.tracks or input.track or input) or {}
  if source.id then source = { source } end
  if not is_array(source) then
    add(diagnostics, "error", "track.input_not_array", "Input must contain tracks array or a single track table.", "input.tracks")
    source = {}
  end
  local tracks = {}
  local by_id = {}
  local by_domain = {}
  local by_polarity = {}
  local limit = #source
  if limit > cfg.max_tracks then
    add(diagnostics, "warning", "track.tracks_truncated", "Progress tracks were truncated by max_tracks.", "input.tracks")
    limit = cfg.max_tracks
  end
  for index = 1, limit do
    local track = normalize_track(source[index], cfg, diagnostics, "input.tracks[" .. index .. "]")
    if track then
      if by_id[track.id] then
        add(diagnostics, "error", "track.duplicate_id", "Duplicate progress track id ignored.", track.id)
      else
        by_id[track.id] = true
        tracks[#tracks + 1] = track
        if not by_domain[track.domain] then by_domain[track.domain] = {} end
        if not by_polarity[track.polarity] then by_polarity[track.polarity] = {} end
        by_domain[track.domain][#by_domain[track.domain] + 1] = track.id
        by_polarity[track.polarity][#by_polarity[track.polarity] + 1] = track.id
      end
    end
  end
  return {
    ok = #diagnostics == 0,
    data = {
      tracks = tracks,
      indexes = { by_id = by_id, by_domain = by_domain, by_polarity = by_polarity },
      summary = { track_count = #tracks, diagnostic_count = #diagnostics }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
