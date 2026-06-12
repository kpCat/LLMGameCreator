local M = {}

M.manifest = {
  id = "interaction/talk_to_npc/v1",
  version = "0.1.0",
  category = "interaction",
  title = "Talk To NPC Interaction",
  purpose = "Builds dialogue start interaction IR from an entity with a dialogue_source component.",
  capabilities = { "interaction.talk.generate", "entity.component.dialogue_source", "dialogue.start.ir" },
  input_schema = {
    type = "table",
    fields = {
      actor = "optional actor entity",
      target = "entity instance with dialogue_source component",
      dialogue_state = "optional current dialogue state metadata"
    }
  },
  output_schema = {
    type = "table",
    fields = {
      interaction = "talk interaction IR",
      dialogue_start = "dialogue start request IR",
      ui = "dialogue UI hint"
    }
  },
  config_schema = {
    default_opening_node_id = "optional string",
    allow_dialogue_combat = "optional boolean",
    ui_mode = "dialogue_focus | rpg_hud | minimal_hud"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d" },
  supported_world_scales = { "single_map", "multi_map", "region", "infinite_chunks" },
  supported_turn_modes = { "realtime", "turn_based", "mixed" },
  supported_combat_modes = { "none", "dialogue_combat", "hybrid", "turn_based", "realtime" },
  unsafe_features = {}
}

local VALID_UI = { dialogue_focus = true, rpg_hud = true, minimal_hud = true }

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
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

local function copy_state(src)
  local out = {}
  if type(src) ~= "table" then
    return out
  end
  for k, v in pairs(src) do
    if type(v) ~= "function" then
      out[k] = v
    end
  end
  return out
end

function M.validate_config(config)
  local diagnostics = {}
  local cfg = config or {}
  if type(cfg) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "talk.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if cfg.default_opening_node_id ~= nil and type(cfg.default_opening_node_id) ~= "string" then
    diagnostics[#diagnostics + 1] = diag("error", "talk.default_opening_node_invalid", "default_opening_node_id must be a string when provided.", "config.default_opening_node_id")
  end
  if cfg.allow_dialogue_combat ~= nil and type(cfg.allow_dialogue_combat) ~= "boolean" then
    diagnostics[#diagnostics + 1] = diag("error", "talk.allow_dialogue_combat_invalid", "allow_dialogue_combat must be boolean when provided.", "config.allow_dialogue_combat")
  end
  if cfg.ui_mode ~= nil and not VALID_UI[cfg.ui_mode] then
    diagnostics[#diagnostics + 1] = diag("error", "talk.ui_mode_invalid", "ui_mode must be dialogue_focus, rpg_hud or minimal_hud.", "config.ui_mode")
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
    diagnostics[#diagnostics + 1] = diag("error", "talk.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  local target = input.target
  if type(target) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "talk.target_invalid", "target must be an entity instance.", "input.target")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if not valid_slash_id(target.id) then
    diagnostics[#diagnostics + 1] = diag("error", "talk.target_id_invalid", "target.id must be a lowercase slash id.", "input.target.id")
  end
  if type(target.components) ~= "table" or type(target.components.dialogue_source) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "talk.target_has_no_dialogue", "target must contain components.dialogue_source.", "input.target.components.dialogue_source")
  end
  if #diagnostics > 0 then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local source = target.components.dialogue_source
  if source.dialogue_id ~= nil and not valid_slash_id(source.dialogue_id) then
    diagnostics[#diagnostics + 1] = diag("error", "talk.dialogue_id_invalid", "dialogue_source.dialogue_id must be a lowercase slash id.", "input.target.components.dialogue_source.dialogue_id")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local opening = source.opening_node_id or config.default_opening_node_id or "start"
  local dialogue_id = source.dialogue_id or (target.id .. "/dialogue")
  local actor_id = type(input.actor) == "table" and input.actor.id or nil
  local can_dialogue_combat = source.supports_dialogue_combat == true and config.allow_dialogue_combat ~= false

  local dialogue_start = {
    dialogue_id = dialogue_id,
    npc_id = target.id,
    actor_id = actor_id,
    speaker_name = source.speaker_name or target.title or target.id,
    opening_node_id = opening,
    state = copy_state(input.dialogue_state),
    modes = {
      dialogue = true,
      dialogue_combat = can_dialogue_combat
    },
    available_bridges = can_dialogue_combat and { "morale", "trust", "suspicion", "focus" } or {}
  }

  local data = {
    interaction = {
      type = "talk",
      actor_id = actor_id,
      target_id = target.id,
      dialogue_id = dialogue_id
    },
    dialogue_start = dialogue_start,
    ui = {
      mode = config.ui_mode or "dialogue_focus",
      panel = "dialogue",
      speaker_name = dialogue_start.speaker_name
    },
    requested_runtime_capabilities = copy_array(source.required_capabilities),
    summary = {
      generator = M.manifest.id,
      target_id = target.id,
      dialogue_id = dialogue_id,
      dialogue_combat_enabled = can_dialogue_combat
    }
  }
  return { ok = true, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
