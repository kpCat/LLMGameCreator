local M = {}

M.manifest = {
  id = "quest/location_discovery/v1",
  version = "0.1.0",
  category = "quest",
  title = "Location Discovery Quest Generator",
  purpose = "Create compact quest IR for discovering locations through hints, exploration, inspection, and optional reporting.",
  capabilities = { "quest.location_discovery.generate", "quest.from_interaction", "world.location.reveal" },
  input_schema = {
    quest_id = "lowercase slash id",
    location_id = "target location id",
    hint_source_id = "optional NPC/object/location id",
    report_target_id = "optional NPC/entity id"
  },
  output_schema = {
    quest = "quest IR compatible with quest_schema/v1",
    world_effects = "location reveal/discovery effect hints"
  },
  config_schema = {
    require_hint = "optional boolean",
    require_inspection = "optional boolean",
    require_report_back = "optional boolean"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d" },
  supported_turn_modes = { "realtime", "turn_based", "mixed" },
  supported_combat_modes = { "none", "realtime", "turn_based", "hybrid" },
  unsafe_features = {}
}

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function add(out, severity, code, message, target)
  out[#out + 1] = diag(severity, code, message, target)
end

local function id_ok(value)
  if type(value) ~= "string" or value == "" then
    return false
  end
  if value:sub(1, 1) == "/" or value:sub(-1) == "/" or value:find("//", 1, true) then
    return false
  end
  return value:match("^[a-z0-9_/%-]+$") ~= nil
end

local function safe_string(value, fallback)
  if type(value) == "string" and value ~= "" then
    return value
  end
  return fallback
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    add(diagnostics, "error", "location_discovery.config.invalid", "Config must be a table.", "config")
    return false, diagnostics
  end
  local bools = { "require_hint", "require_inspection", "require_report_back" }
  for index = 1, #bools do
    local field = bools[index]
    if config[field] ~= nil and type(config[field]) ~= "boolean" then
      add(diagnostics, "error", "location_discovery.config.boolean_invalid", field .. " must be boolean.", "config/" .. field)
    end
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local config = (ctx and type(ctx.config) == "table") and ctx.config or {}
  local ok_config, config_diags = M.validate_config(config)
  for index = 1, #config_diags do
    diagnostics[#diagnostics + 1] = config_diags[index]
  end
  if not ok_config then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if type(input) ~= "table" then
    add(diagnostics, "error", "location_discovery.input.invalid", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local quest_id = input.quest_id or input.id
  local location_id = input.location_id
  if not id_ok(quest_id) then
    add(diagnostics, "error", "location_discovery.quest_id.invalid", "quest_id must be a lowercase slash id.", "input/quest_id")
  end
  if not id_ok(location_id) then
    add(diagnostics, "error", "location_discovery.location_id.invalid", "location_id must be a lowercase slash id.", "input/location_id")
  end
  local has_error = false
  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      has_error = true
      break
    end
  end
  if has_error then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local require_hint = config.require_hint ~= false
  local require_inspection = config.require_inspection == true
  local require_report = config.require_report_back == true and input.report_target_id ~= nil
  local stages = {}

  stages[#stages + 1] = {
    id = "hint",
    title = "Learn location hint",
    description = "A dialogue, note, object, or interaction reveals the location as a lead.",
    objectives = {
      {
        id = "receive_hint",
        type = input.hint_source_id and "inspect" or "custom_counter",
        title = safe_string(input.hint_title, "Receive a hint"),
        target = input.hint_source_id,
        required = require_hint,
        tags = { "hint", "interaction" },
        completion_conditions = {
          { type = input.hint_source_id and "interaction_happened" or "counter_at_least", target = quest_id .. "/hint_received", count = 1 }
        }
      }
    },
    transitions = {
      { id = "hint_received", to_stage = "discover", conditions = { { type = "objective_complete", objective_id = "receive_hint" } } }
    },
    effects = { { type = "reveal_location", target = location_id, value = "hinted" } }
  }

  stages[#stages + 1] = {
    id = "discover",
    title = "Discover location",
    description = "Reach or reveal the target location in the world model.",
    objectives = {
      {
        id = "discover_location",
        type = "discover_location",
        title = safe_string(input.discovery_title, "Discover the location"),
        target_location_id = location_id,
        required = true,
        tags = { "exploration", "location" },
        completion_conditions = {
          { type = "location_discovered", target = location_id }
        }
      }
    },
    transitions = {
      { id = "location_discovered", to_stage = require_inspection and "inspect" or (require_report and "report" or "complete"), conditions = { { type = "objective_complete", objective_id = "discover_location" } } }
    },
    effects = { { type = "reveal_location", target = location_id, value = "discovered" } }
  }

  stages[#stages + 1] = {
    id = "inspect",
    title = "Inspect landmark",
    description = "Optional inspection step for exploration/adventure games.",
    objectives = {
      {
        id = "inspect_landmark",
        type = "inspect",
        title = safe_string(input.inspect_title, "Inspect the landmark"),
        target = input.landmark_id or location_id,
        target_location_id = location_id,
        required = require_inspection,
        tags = { "inspect", "landmark" },
        completion_conditions = {
          { type = "interaction_happened", target = input.landmark_id or location_id }
        }
      }
    },
    transitions = {
      { id = "landmark_inspected", to_stage = require_report and "report" or "complete", conditions = { { type = "objective_complete", objective_id = "inspect_landmark" } } }
    },
    effects = { { type = "add_progress", target = quest_id .. "/exploration", amount = 1 } }
  }

  stages[#stages + 1] = {
    id = "report",
    title = "Report discovery",
    description = "Optional report-back stage for RPG/adventure quest flow.",
    objectives = {
      {
        id = "report_discovery",
        type = "talk_to",
        title = safe_string(input.report_title, "Report the discovery"),
        target_entity_id = input.report_target_id,
        required = require_report,
        tags = { "dialogue", "quest_turn_in" },
        completion_conditions = {
          { type = "dialogue_choice_selected", target = quest_id .. "/report_discovery" }
        }
      }
    },
    transitions = {
      { id = "reported", to_stage = "complete", conditions = { { type = "objective_complete", objective_id = "report_discovery" } } }
    },
    effects = { { type = "unlock_dialogue", target = quest_id .. "/after_discovery" } }
  }

  stages[#stages + 1] = {
    id = "complete",
    title = "Location discovery complete",
    description = "Quest is complete and location state can be persisted by runtime.",
    objectives = {},
    transitions = {},
    effects = { { type = "complete_quest", target = quest_id } }
  }

  local triggers = {}
  if input.hint_source_id then
    triggers[1] = {
      id = "start_from_hint_interaction",
      type = "interaction",
      source_id = input.hint_source_id,
      interaction_id = quest_id .. "/hint",
      target_stage = "hint"
    }
  else
    triggers[1] = { id = "manual_start", type = "manual", target_stage = "hint" }
  end

  return {
    ok = true,
    data = {
      quest = {
        id = quest_id,
        title = safe_string(input.title, "Location discovery"),
        description = safe_string(input.description, "Find and register a location in the world model."),
        status = "inactive",
        start_stage_id = "hint",
        stages = stages,
        triggers = triggers,
        progress_tracks = {
          { id = quest_id .. "/exploration", title = "Exploration progress", kind = "abstract_progress", min = 0, max = 3, starts_at = 0 }
        },
        completion_conditions = { { type = "stage_active", stage_id = "complete" } },
        effects = type(input.reward_effects) == "table" and input.reward_effects or {},
        tags = { "exploration", "location_discovery", "interaction" },
        metadata = { generated_by = M.manifest.id }
      },
      world_effects = {
        { type = "reveal_location", target = location_id, value = "hinted" },
        { type = "reveal_location", target = location_id, value = "discovered" }
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
