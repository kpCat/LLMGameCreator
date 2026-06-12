local M = {}

M.manifest = {
  id = "quest/simple_investigation/v1",
  version = "0.1.0",
  category = "quest",
  title = "Simple Investigation Quest Generator",
  purpose = "Create compact investigation quest IR from clues, suspects, facts, dialogue hooks, and interaction hooks.",
  capabilities = { "quest.investigation.generate", "quest.from_dialogue", "quest.from_interaction" },
  input_schema = {
    quest_id = "lowercase slash id",
    title = "quest title",
    giver_id = "optional NPC/entity id",
    clue_targets = "array of clue interaction targets",
    suspect_id = "optional suspect NPC/entity id"
  },
  output_schema = {
    quest = "quest IR compatible with quest_schema/v1",
    integration = "dialogue and interaction hook hints"
  },
  config_schema = {
    default_clue_count = "optional positive number",
    require_report_back = "optional boolean",
    include_progress_track = "optional boolean"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d" },
  supported_turn_modes = { "realtime", "turn_based", "mixed" },
  supported_combat_modes = { "none", "dialogue_combat", "hybrid" },
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

local function as_list(value)
  if type(value) == "table" then
    return value
  end
  return {}
end

local function safe_string(value, fallback)
  if type(value) == "string" and value ~= "" then
    return value
  end
  return fallback
end

local function make_trigger(input, quest_id)
  if type(input.start_trigger) == "table" then
    return {
      id = safe_string(input.start_trigger.id, "start_from_trigger"),
      type = safe_string(input.start_trigger.type, "dialogue_choice"),
      source_id = input.start_trigger.source_id or input.giver_id,
      choice_id = input.start_trigger.choice_id,
      interaction_id = input.start_trigger.interaction_id,
      target_stage = "accept"
    }
  end
  if type(input.giver_id) == "string" and input.giver_id ~= "" then
    return {
      id = "start_from_giver_dialogue",
      type = "dialogue_choice",
      source_id = input.giver_id,
      choice_id = quest_id .. "/accept",
      target_stage = "accept"
    }
  end
  return { id = "manual_start", type = "manual", target_stage = "accept" }
end

local function make_clue_objective(clue, index)
  local clue_id = safe_string(clue.id, "clue_" .. tostring(index))
  return {
    id = "inspect_" .. clue_id,
    type = "inspect",
    title = safe_string(clue.title, "Inspect clue " .. tostring(index)),
    description = safe_string(clue.description, "Find and inspect an investigation clue."),
    target = clue.target,
    target_entity_id = clue.target_entity_id,
    target_location_id = clue.target_location_id,
    required = clue.required ~= false,
    tags = { "clue", "investigation" },
    completion_conditions = {
      { type = "interaction_happened", target = clue.target or clue.target_entity_id or clue.target_location_id or clue_id }
    },
    metadata = {
      fact_id = clue.fact_id,
      clue_id = clue_id
    }
  }
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    add(diagnostics, "error", "simple_investigation.config.invalid", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.default_clue_count ~= nil and (type(config.default_clue_count) ~= "number" or config.default_clue_count < 1) then
    add(diagnostics, "error", "simple_investigation.config.clue_count_invalid", "default_clue_count must be a positive number.", "config/default_clue_count")
  end
  if config.require_report_back ~= nil and type(config.require_report_back) ~= "boolean" then
    add(diagnostics, "error", "simple_investigation.config.report_invalid", "require_report_back must be boolean.", "config/require_report_back")
  end
  if config.include_progress_track ~= nil and type(config.include_progress_track) ~= "boolean" then
    add(diagnostics, "error", "simple_investigation.config.progress_invalid", "include_progress_track must be boolean.", "config/include_progress_track")
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
    add(diagnostics, "error", "simple_investigation.input.invalid", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  local quest_id = input.quest_id or input.id
  if not id_ok(quest_id) then
    add(diagnostics, "error", "simple_investigation.quest_id.invalid", "quest_id must be a lowercase slash id.", "input/quest_id")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local clue_targets = as_list(input.clue_targets)
  if #clue_targets == 0 then
    add(diagnostics, "warning", "simple_investigation.clues.empty", "No clue_targets were provided; a placeholder counter objective is created.", "input/clue_targets")
  end

  local clue_objectives = {}
  for index = 1, #clue_targets do
    if type(clue_targets[index]) == "table" then
      clue_objectives[#clue_objectives + 1] = make_clue_objective(clue_targets[index], index)
    else
      add(diagnostics, "warning", "simple_investigation.clue.invalid", "Clue target must be a table and was skipped.", "input/clue_targets/" .. tostring(index))
    end
  end
  if #clue_objectives == 0 then
    clue_objectives[1] = {
      id = "collect_investigation_progress",
      type = "custom_counter",
      title = "Gather investigation progress",
      description = "Runtime should increment this counter from clue interactions.",
      progress_key = quest_id .. "/clues_found",
      count = config.default_clue_count or 1,
      required = true,
      tags = { "investigation", "fallback" },
      completion_conditions = {
        { type = "counter_at_least", target = quest_id .. "/clues_found", count = config.default_clue_count or 1 }
      }
    }
  end

  local stages = {
    {
      id = "accept",
      title = "Accept investigation",
      description = "Quest is introduced through dialogue or interaction.",
      objectives = {
        {
          id = "speak_with_giver",
          type = "talk_to",
          title = "Speak with the quest giver",
          target_entity_id = input.giver_id,
          required = input.giver_id ~= nil,
          tags = { "dialogue", "quest_start" },
          completion_conditions = {
            { type = "dialogue_choice_selected", target = quest_id .. "/accept" }
          }
        }
      },
      transitions = {
        { id = "accepted", to_stage = "gather_clues", conditions = { { type = "objective_complete", objective_id = "speak_with_giver" } } }
      },
      effects = { { type = "set_flag", target = quest_id .. "/accepted", value = true } }
    },
    {
      id = "gather_clues",
      title = "Gather clues",
      description = "Inspect clue targets and convert interactions into quest progress.",
      objectives = clue_objectives,
      transitions = {
        { id = "clues_ready", to_stage = "confront", conditions = { { type = "stage_active", stage_id = "gather_clues" } } }
      },
      effects = { { type = "add_progress", target = quest_id .. "/investigation", amount = #clue_objectives } }
    },
    {
      id = "confront",
      title = "Confront or report findings",
      description = "Use gathered facts in dialogue or interaction.",
      objectives = {
        {
          id = "confront_suspect",
          type = "talk_to",
          title = safe_string(input.confront_title, "Confront the suspect"),
          target_entity_id = input.suspect_id or input.target_entity_id,
          required = input.suspect_id ~= nil or input.target_entity_id ~= nil,
          tags = { "dialogue", "fact_based_dialogue" },
          completion_conditions = {
            { type = "dialogue_choice_selected", target = quest_id .. "/confront" }
          }
        }
      },
      transitions = {
        { id = "reported", to_stage = "complete", conditions = { { type = "objective_complete", objective_id = "confront_suspect" } } }
      },
      effects = { { type = "unlock_dialogue", target = quest_id .. "/resolution" } }
    },
    {
      id = "complete",
      title = "Investigation complete",
      description = "Quest is ready for runtime completion.",
      objectives = {},
      transitions = {},
      effects = { { type = "complete_quest", target = quest_id } }
    }
  }

  if config.require_report_back and input.giver_id then
    stages[3].objectives[#stages[3].objectives + 1] = {
      id = "report_to_giver",
      type = "talk_to",
      title = "Report back",
      target_entity_id = input.giver_id,
      required = true,
      tags = { "dialogue", "quest_turn_in" },
      completion_conditions = {
        { type = "dialogue_choice_selected", target = quest_id .. "/report" }
      }
    }
  end

  local progress_tracks = {}
  if config.include_progress_track ~= false then
    progress_tracks[1] = {
      id = quest_id .. "/investigation",
      title = "Investigation progress",
      kind = "abstract_progress",
      min = 0,
      max = #clue_objectives + 1,
      starts_at = 0
    }
  end

  local quest = {
    id = quest_id,
    title = safe_string(input.title, "Investigation"),
    description = safe_string(input.description, "A compact investigation quest generated from clue and dialogue hooks."),
    status = "inactive",
    start_stage_id = "accept",
    stages = stages,
    triggers = { make_trigger(input, quest_id) },
    progress_tracks = progress_tracks,
    completion_conditions = { { type = "stage_active", stage_id = "complete" } },
    effects = type(input.reward_effects) == "table" and input.reward_effects or {},
    tags = { "investigation", "dialogue", "interaction" },
    metadata = {
      generated_by = M.manifest.id,
      facts = type(input.facts) == "table" and input.facts or {}
    }
  }

  return {
    ok = true,
    data = {
      quest = quest,
      integration = {
        dialogue_hooks = { quest_id .. "/accept", quest_id .. "/confront", quest_id .. "/resolution" },
        interaction_hooks = { quest_id .. "/clue_interaction" },
        compatible_with = { "dialogue/fact_based_dialogue/v1", "interaction/inspect_object/v1", "quest/quest_schema/v1" }
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
