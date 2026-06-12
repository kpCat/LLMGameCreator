local M = {}

M.manifest = {
  id = "quest/fetch_quest/v1",
  version = "0.1.0",
  category = "quest",
  title = "Fetch Quest Generator",
  purpose = "Create compact fetch/delivery quest IR with item objectives, turn-in conditions, and non-XP reward hooks.",
  capabilities = { "quest.fetch.generate", "quest.progress.abstract" },
  input_schema = {
    quest_id = "lowercase slash id",
    item_id = "required item id",
    count = "required item count",
    giver_id = "optional NPC/entity id",
    delivery_target_id = "optional NPC/location id"
  },
  output_schema = {
    quest = "quest IR compatible with quest_schema/v1",
    item_requirements = "compact item requirement list"
  },
  config_schema = {
    allow_partial_progress = "optional boolean",
    require_return = "optional boolean",
    remove_items_on_complete = "optional boolean"
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

local function positive_count(value)
  if type(value) == "number" and value >= 1 then
    return value
  end
  return 1
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    add(diagnostics, "error", "fetch_quest.config.invalid", "Config must be a table.", "config")
    return false, diagnostics
  end
  local bools = { "allow_partial_progress", "require_return", "remove_items_on_complete" }
  for index = 1, #bools do
    local field = bools[index]
    if config[field] ~= nil and type(config[field]) ~= "boolean" then
      add(diagnostics, "error", "fetch_quest.config.boolean_invalid", field .. " must be boolean.", "config/" .. field)
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
    add(diagnostics, "error", "fetch_quest.input.invalid", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local quest_id = input.quest_id or input.id
  if not id_ok(quest_id) then
    add(diagnostics, "error", "fetch_quest.quest_id.invalid", "quest_id must be a lowercase slash id.", "input/quest_id")
  end
  local item_id = input.item_id
  if not id_ok(item_id) then
    add(diagnostics, "error", "fetch_quest.item_id.invalid", "item_id must be a lowercase slash id.", "input/item_id")
  end
  local count = positive_count(input.count)
  if type(input.count) ~= "number" or input.count < 1 then
    add(diagnostics, "warning", "fetch_quest.count.defaulted", "count was missing or invalid and was defaulted to 1.", "input/count")
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

  local require_return = config.require_return ~= false
  local delivery_target = input.delivery_target_id or input.giver_id
  local stages = {
    {
      id = "accepted",
      title = "Quest accepted",
      description = "The fetch quest has been accepted from dialogue or interaction.",
      objectives = {
        {
          id = "accept_fetch_quest",
          type = input.giver_id and "talk_to" or "custom_counter",
          title = "Accept the request",
          target_entity_id = input.giver_id,
          required = input.giver_id ~= nil,
          tags = { "quest_start" },
          completion_conditions = {
            { type = input.giver_id and "dialogue_choice_selected" or "counter_at_least", target = quest_id .. "/accepted", count = 1 }
          }
        }
      },
      transitions = {
        { id = "start_fetching", to_stage = "collect", conditions = { { type = "objective_complete", objective_id = "accept_fetch_quest" } } }
      },
      effects = { { type = "set_flag", target = quest_id .. "/accepted", value = true } }
    },
    {
      id = "collect",
      title = "Collect required items",
      description = "Collect items through loot, crafting, trade, or interaction.",
      objectives = {
        {
          id = "collect_items",
          type = "collect_item",
          title = safe_string(input.collect_title, "Collect required items"),
          item_id = item_id,
          count = count,
          required = true,
          tags = { "item", "fetch" },
          completion_conditions = {
            { type = "item_count", target = item_id, count = count }
          }
        }
      },
      transitions = {
        { id = "items_ready", to_stage = require_return and "deliver" or "complete", conditions = { { type = "objective_complete", objective_id = "collect_items" } } }
      },
      effects = { { type = "add_progress", target = quest_id .. "/items_collected", amount = count } }
    },
    {
      id = "deliver",
      title = "Deliver items",
      description = "Return the requested items to the target NPC or location.",
      objectives = {
        {
          id = "deliver_items",
          type = delivery_target and "deliver_item" or "custom_counter",
          title = safe_string(input.delivery_title, "Deliver requested items"),
          target_entity_id = delivery_target,
          target_location_id = input.delivery_location_id,
          item_id = item_id,
          count = count,
          required = require_return,
          tags = { "delivery", "quest_turn_in" },
          completion_conditions = {
            { type = delivery_target and "interaction_happened" or "counter_at_least", target = quest_id .. "/delivered", count = 1 }
          }
        }
      },
      transitions = {
        { id = "delivered", to_stage = "complete", conditions = { { type = "objective_complete", objective_id = "deliver_items" } } }
      },
      effects = {}
    },
    {
      id = "complete",
      title = "Fetch quest complete",
      description = "Runtime may apply reward and inventory effects.",
      objectives = {},
      transitions = {},
      effects = { { type = "complete_quest", target = quest_id } }
    }
  }

  if config.remove_items_on_complete ~= false then
    stages[3].effects[#stages[3].effects + 1] = { type = "remove_item", target = item_id, amount = count }
  end

  local triggers = {}
  if input.giver_id then
    triggers[1] = {
      id = "start_from_dialogue",
      type = "dialogue_choice",
      source_id = input.giver_id,
      choice_id = quest_id .. "/accept",
      target_stage = "accepted"
    }
  elseif input.start_interaction_id then
    triggers[1] = {
      id = "start_from_interaction",
      type = "interaction",
      interaction_id = input.start_interaction_id,
      target_stage = "accepted"
    }
  else
    triggers[1] = { id = "manual_start", type = "manual", target_stage = "accepted" }
  end

  local progress_tracks = {}
  if config.allow_partial_progress ~= false then
    progress_tracks[1] = {
      id = quest_id .. "/items_collected",
      title = "Items collected",
      kind = "counter",
      min = 0,
      max = count,
      starts_at = 0
    }
  end

  local reward_effects = type(input.reward_effects) == "table" and input.reward_effects or {}
  for index = 1, #reward_effects do
    stages[4].effects[#stages[4].effects + 1] = reward_effects[index]
  end

  return {
    ok = true,
    data = {
      quest = {
        id = quest_id,
        title = safe_string(input.title, "Fetch quest"),
        description = safe_string(input.description, "Collect and optionally deliver requested items."),
        status = "inactive",
        start_stage_id = "accepted",
        stages = stages,
        triggers = triggers,
        progress_tracks = progress_tracks,
        completion_conditions = { { type = "stage_active", stage_id = "complete" } },
        effects = {},
        tags = { "fetch", "item", "progress" },
        metadata = { generated_by = M.manifest.id }
      },
      item_requirements = {
        { item_id = item_id, count = count, remove_on_complete = config.remove_items_on_complete ~= false }
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
