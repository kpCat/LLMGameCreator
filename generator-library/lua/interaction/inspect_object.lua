local M = {}

M.manifest = {
  id = "interaction/inspect_object/v1",
  version = "0.1.0",
  category = "interaction",
  title = "Inspect Object Interaction",
  purpose = "Builds compact inspection interaction IR from an inspectable entity instance.",
  capabilities = { "interaction.inspect.generate", "entity.component.inspectable" },
  input_schema = {
    type = "table",
    fields = {
      actor = "optional actor entity",
      target = "entity instance with inspectable component",
      context = "optional world or quest context"
    }
  },
  output_schema = {
    type = "table",
    fields = {
      interaction = "inspection interaction IR",
      facts_revealed = "array of fact ids",
      ui = "small UI hint object"
    }
  },
  config_schema = {
    max_summary_length = "optional positive integer",
    allow_hidden_facts = "optional boolean",
    ui_mode = "minimal_hud | rpg_hud | dialogue_focus"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d" },
  supported_world_scales = { "single_map", "multi_map", "region", "infinite_chunks" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local VALID_UI = { minimal_hud = true, rpg_hud = true, dialogue_focus = true }

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function is_int(v)
  return type(v) == "number" and v % 1 == 0
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

local function copy_visible_facts(facts, allow_hidden)
  local out = {}
  if type(facts) ~= "table" then
    return out
  end
  for i = 1, #facts do
    local f = facts[i]
    if type(f) == "string" then
      out[#out + 1] = { id = f, hidden = false }
    elseif type(f) == "table" and valid_slash_id(f.id) and (allow_hidden or f.hidden ~= true) then
      out[#out + 1] = { id = f.id, hidden = f.hidden == true, note = f.note }
    end
  end
  return out
end

local function limit_text(text, max_len)
  if type(text) ~= "string" then
    return ""
  end
  if type(max_len) ~= "number" or max_len <= 0 then
    return text
  end
  if #text <= max_len then
    return text
  end
  if max_len <= 3 then
    return string.sub(text, 1, max_len)
  end
  return string.sub(text, 1, max_len - 3) .. "..."
end

function M.validate_config(config)
  local diagnostics = {}
  local cfg = config or {}
  if type(cfg) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "inspect.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if cfg.max_summary_length ~= nil and (not is_int(cfg.max_summary_length) or cfg.max_summary_length <= 0) then
    diagnostics[#diagnostics + 1] = diag("error", "inspect.max_summary_length_invalid", "max_summary_length must be a positive integer.", "config.max_summary_length")
  end
  if cfg.allow_hidden_facts ~= nil and type(cfg.allow_hidden_facts) ~= "boolean" then
    diagnostics[#diagnostics + 1] = diag("error", "inspect.allow_hidden_facts_invalid", "allow_hidden_facts must be boolean when provided.", "config.allow_hidden_facts")
  end
  if cfg.ui_mode ~= nil and not VALID_UI[cfg.ui_mode] then
    diagnostics[#diagnostics + 1] = diag("error", "inspect.ui_mode_invalid", "ui_mode must be minimal_hud, rpg_hud or dialogue_focus.", "config.ui_mode")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local config = {}
  if type(ctx) == "table" and type(ctx.config) == "table" then
    config = ctx.config
  end
  local ok_config, config_diags = M.validate_config(config)
  for i = 1, #config_diags do
    diagnostics[#diagnostics + 1] = config_diags[i]
  end
  if not ok_config then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if type(input) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "inspect.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  local target = input.target
  if type(target) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "inspect.target_invalid", "target must be an entity instance.", "input.target")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if not valid_slash_id(target.id) then
    diagnostics[#diagnostics + 1] = diag("error", "inspect.target_id_invalid", "target.id must be a lowercase slash id.", "input.target.id")
  end
  if type(target.components) ~= "table" or type(target.components.inspectable) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "inspect.target_not_inspectable", "target must contain components.inspectable.", "input.target.components.inspectable")
  end
  if #diagnostics > 0 then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local inspectable = target.components.inspectable
  local title = inspectable.title or target.title or target.id
  local summary = limit_text(inspectable.summary or "", config.max_summary_length or 240)
  local facts = copy_visible_facts(inspectable.reveals_facts, config.allow_hidden_facts == true)

  local interaction = {
    type = "inspect",
    actor_id = type(input.actor) == "table" and input.actor.id or nil,
    target_id = target.id,
    target_title = title,
    summary = summary,
    detail_level = inspectable.detail_level or "short",
    runtime_effects = {
      reveal_facts = facts
    }
  }

  local data = {
    interaction = interaction,
    facts_revealed = facts,
    ui = {
      mode = config.ui_mode or "minimal_hud",
      panel = "inspection",
      title = title,
      text = summary
    },
    summary = {
      generator = M.manifest.id,
      target_id = target.id,
      fact_count = #facts
    }
  }
  return { ok = true, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
