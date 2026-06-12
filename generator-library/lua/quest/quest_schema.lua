local M = {}

M.manifest = {
  id = "quest/quest_schema/v1",
  version = "0.1.0",
  category = "quest",
  title = "Quest Schema",
  purpose = "Normalize and validate compact quest IR with stages, objectives, completion conditions, effects, and external triggers.",
  capabilities = { "quest.schema.normalize", "quest.progress.validate" },
  input_schema = {
    quests = "array of quest definitions or a single quest definition"
  },
  output_schema = {
    quests = "normalized quest IR array",
    summary = "counts and referenced integration hooks"
  },
  config_schema = {
    allowed_objective_types = "optional array of objective type strings",
    allowed_condition_types = "optional array of condition type strings",
    allowed_effect_types = "optional array of effect type strings",
    default_status = "optional initial quest status"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local DEFAULT_OBJECTIVES = {
  "talk_to", "inspect", "collect_item", "deliver_item", "reach_location",
  "discover_location", "use_item_on_target", "defeat_entity", "wait_ticks",
  "custom_counter"
}

local DEFAULT_CONDITIONS = {
  "objective_complete", "flag_set", "item_count", "location_discovered",
  "interaction_happened", "dialogue_choice_selected", "counter_at_least",
  "stage_active"
}

local DEFAULT_EFFECTS = {
  "set_flag", "clear_flag", "add_item", "remove_item", "start_quest",
  "complete_quest", "unlock_dialogue", "reveal_location", "advance_stage",
  "add_progress", "emit_event"
}

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function add(out, severity, code, message, target)
  out[#out + 1] = diag(severity, code, message, target)
end

local function is_array(value)
  if type(value) ~= "table" then
    return false
  end
  local count = 0
  for key, _ in pairs(value) do
    if type(key) ~= "number" or key < 1 or key % 1 ~= 0 then
      return false
    end
    count = count + 1
  end
  for index = 1, count do
    if value[index] == nil then
      return false
    end
  end
  return true
end

local function clone_array(value)
  local result = {}
  if type(value) == "table" then
    for index = 1, #value do
      result[#result + 1] = value[index]
    end
  end
  return result
end

local function list_to_set(list)
  local set = {}
  for index = 1, #list do
    set[list[index]] = true
  end
  return set
end

local function id_ok(value)
  if type(value) ~= "string" or value == "" then
    return false
  end
  if value:sub(1, 1) == "/" or value:sub(-1) == "/" then
    return false
  end
  if value:find("//", 1, true) then
    return false
  end
  return value:match("^[a-z0-9_/%-]+$") ~= nil
end

local function normalize_tags(tags)
  local result = {}
  if type(tags) ~= "table" then
    return result
  end
  local seen = {}
  for index = 1, #tags do
    local tag = tags[index]
    if type(tag) == "string" and tag ~= "" and not seen[tag] then
      seen[tag] = true
      result[#result + 1] = tag
    end
  end
  return result
end

local function normalize_effect(effect, index, allowed, diagnostics, target)
  if type(effect) ~= "table" then
    add(diagnostics, "error", "quest.effect.invalid", "Effect must be a table.", target .. "/effects/" .. tostring(index))
    return nil
  end
  local effect_type = effect.type
  if type(effect_type) ~= "string" or not allowed[effect_type] then
    add(diagnostics, "error", "quest.effect.type_invalid", "Effect type is not allowed.", target .. "/effects/" .. tostring(index) .. "/type")
    return nil
  end
  return {
    id = type(effect.id) == "string" and effect.id or (target .. "/effect/" .. tostring(index)),
    type = effect_type,
    target = effect.target,
    value = effect.value,
    amount = effect.amount,
    stage_id = effect.stage_id,
    event = effect.event,
    metadata = type(effect.metadata) == "table" and effect.metadata or {}
  }
end

local function normalize_condition(condition, index, allowed, diagnostics, target)
  if type(condition) ~= "table" then
    add(diagnostics, "error", "quest.condition.invalid", "Condition must be a table.", target .. "/conditions/" .. tostring(index))
    return nil
  end
  local condition_type = condition.type
  if type(condition_type) ~= "string" or not allowed[condition_type] then
    add(diagnostics, "error", "quest.condition.type_invalid", "Condition type is not allowed.", target .. "/conditions/" .. tostring(index) .. "/type")
    return nil
  end
  return {
    id = type(condition.id) == "string" and condition.id or (target .. "/condition/" .. tostring(index)),
    type = condition_type,
    target = condition.target,
    value = condition.value,
    count = condition.count,
    stage_id = condition.stage_id,
    objective_id = condition.objective_id,
    metadata = type(condition.metadata) == "table" and condition.metadata or {}
  }
end

local function normalize_objective(objective, index, allowed, diagnostics, target)
  if type(objective) ~= "table" then
    add(diagnostics, "error", "quest.objective.invalid", "Objective must be a table.", target .. "/objectives/" .. tostring(index))
    return nil
  end
  local objective_type = objective.type
  if type(objective_type) ~= "string" or not allowed[objective_type] then
    add(diagnostics, "error", "quest.objective.type_invalid", "Objective type is not allowed.", target .. "/objectives/" .. tostring(index) .. "/type")
    return nil
  end
  local objective_id = objective.id
  if type(objective_id) ~= "string" or objective_id == "" then
    objective_id = "objective_" .. tostring(index)
  end
  return {
    id = objective_id,
    type = objective_type,
    title = type(objective.title) == "string" and objective.title or objective_id,
    description = type(objective.description) == "string" and objective.description or "",
    target = objective.target,
    target_entity_id = objective.target_entity_id,
    target_location_id = objective.target_location_id,
    item_id = objective.item_id,
    count = type(objective.count) == "number" and objective.count or 1,
    progress_key = objective.progress_key,
    required = objective.required ~= false,
    visible = objective.visible ~= false,
    tags = normalize_tags(objective.tags),
    completion_conditions = type(objective.completion_conditions) == "table" and clone_array(objective.completion_conditions) or {},
    metadata = type(objective.metadata) == "table" and objective.metadata or {}
  }
end

local function normalize_transition(transition, index, stage_ids, diagnostics, target)
  if type(transition) ~= "table" then
    add(diagnostics, "error", "quest.transition.invalid", "Transition must be a table.", target .. "/transitions/" .. tostring(index))
    return nil
  end
  local to_stage = transition.to_stage
  if type(to_stage) ~= "string" or not stage_ids[to_stage] then
    add(diagnostics, "error", "quest.transition.target_missing", "Transition target stage is missing.", target .. "/transitions/" .. tostring(index) .. "/to_stage")
    return nil
  end
  return {
    id = type(transition.id) == "string" and transition.id or ("transition_" .. tostring(index)),
    to_stage = to_stage,
    mode = type(transition.mode) == "string" and transition.mode or "when_conditions_met",
    conditions = type(transition.conditions) == "table" and clone_array(transition.conditions) or {},
    effects = type(transition.effects) == "table" and clone_array(transition.effects) or {},
    metadata = type(transition.metadata) == "table" and transition.metadata or {}
  }
end

local function normalize_stage(stage, index, allowed_objectives, diagnostics, quest_target)
  if type(stage) ~= "table" then
    add(diagnostics, "error", "quest.stage.invalid", "Stage must be a table.", quest_target .. "/stages/" .. tostring(index))
    return nil
  end
  local stage_id = stage.id
  if type(stage_id) ~= "string" or stage_id == "" then
    stage_id = "stage_" .. tostring(index)
  end
  local target = quest_target .. "/stages/" .. stage_id
  local objectives = {}
  local source_objectives = type(stage.objectives) == "table" and stage.objectives or {}
  for objective_index = 1, #source_objectives do
    local normalized = normalize_objective(source_objectives[objective_index], objective_index, allowed_objectives, diagnostics, target)
    if normalized then
      objectives[#objectives + 1] = normalized
    end
  end
  return {
    id = stage_id,
    title = type(stage.title) == "string" and stage.title or stage_id,
    description = type(stage.description) == "string" and stage.description or "",
    objectives = objectives,
    completion_conditions = type(stage.completion_conditions) == "table" and clone_array(stage.completion_conditions) or {},
    effects = type(stage.effects) == "table" and clone_array(stage.effects) or {},
    transitions = type(stage.transitions) == "table" and clone_array(stage.transitions) or {},
    tags = normalize_tags(stage.tags),
    metadata = type(stage.metadata) == "table" and stage.metadata or {}
  }
end

local function normalize_trigger(trigger, index, diagnostics, quest_target)
  if type(trigger) ~= "table" then
    add(diagnostics, "error", "quest.trigger.invalid", "Trigger must be a table.", quest_target .. "/triggers/" .. tostring(index))
    return nil
  end
  local trigger_type = trigger.type
  if trigger_type ~= "dialogue_choice" and trigger_type ~= "interaction" and trigger_type ~= "location_entered" and trigger_type ~= "manual" then
    add(diagnostics, "warning", "quest.trigger.type_unknown", "Trigger type is not standard but is preserved.", quest_target .. "/triggers/" .. tostring(index) .. "/type")
  end
  return {
    id = type(trigger.id) == "string" and trigger.id or ("trigger_" .. tostring(index)),
    type = type(trigger_type) == "string" and trigger_type or "manual",
    source_id = trigger.source_id,
    choice_id = trigger.choice_id,
    interaction_id = trigger.interaction_id,
    target_stage = trigger.target_stage,
    metadata = type(trigger.metadata) == "table" and trigger.metadata or {}
  }
end

local function normalize_quest(quest, index, config, diagnostics)
  if type(quest) ~= "table" then
    add(diagnostics, "error", "quest.invalid", "Quest must be a table.", "quests/" .. tostring(index))
    return nil
  end
  local quest_id = quest.id
  if not id_ok(quest_id) then
    add(diagnostics, "error", "quest.id_invalid", "Quest id must be a lowercase slash id.", "quests/" .. tostring(index) .. "/id")
    return nil
  end

  local allowed_objectives = list_to_set(config.allowed_objective_types or DEFAULT_OBJECTIVES)
  local allowed_conditions = list_to_set(config.allowed_condition_types or DEFAULT_CONDITIONS)
  local allowed_effects = list_to_set(config.allowed_effect_types or DEFAULT_EFFECTS)
  local quest_target = "quests/" .. quest_id

  local stages = {}
  local stage_ids = {}
  local source_stages = type(quest.stages) == "table" and quest.stages or {}
  for stage_index = 1, #source_stages do
    local stage = normalize_stage(source_stages[stage_index], stage_index, allowed_objectives, diagnostics, quest_target)
    if stage then
      if stage_ids[stage.id] then
        add(diagnostics, "error", "quest.stage.duplicate_id", "Duplicate stage id.", quest_target .. "/stages/" .. stage.id)
      else
        stage_ids[stage.id] = true
        stages[#stages + 1] = stage
      end
    end
  end
  if #stages == 0 then
    add(diagnostics, "error", "quest.stages.empty", "Quest must contain at least one stage.", quest_target .. "/stages")
  end

  for stage_index = 1, #stages do
    local stage = stages[stage_index]
    local normalized_transitions = {}
    for transition_index = 1, #stage.transitions do
      local transition = normalize_transition(stage.transitions[transition_index], transition_index, stage_ids, diagnostics, quest_target .. "/stages/" .. stage.id)
      if transition then
        normalized_transitions[#normalized_transitions + 1] = transition
      end
    end
    stage.transitions = normalized_transitions
  end

  local conditions = {}
  local source_conditions = type(quest.completion_conditions) == "table" and quest.completion_conditions or {}
  for condition_index = 1, #source_conditions do
    local condition = normalize_condition(source_conditions[condition_index], condition_index, allowed_conditions, diagnostics, quest_target)
    if condition then
      conditions[#conditions + 1] = condition
    end
  end

  local effects = {}
  local source_effects = type(quest.effects) == "table" and quest.effects or {}
  for effect_index = 1, #source_effects do
    local effect = normalize_effect(source_effects[effect_index], effect_index, allowed_effects, diagnostics, quest_target)
    if effect then
      effects[#effects + 1] = effect
    end
  end

  local triggers = {}
  local source_triggers = type(quest.triggers) == "table" and quest.triggers or {}
  for trigger_index = 1, #source_triggers do
    local trigger = normalize_trigger(source_triggers[trigger_index], trigger_index, diagnostics, quest_target)
    if trigger then
      triggers[#triggers + 1] = trigger
    end
  end

  local start_stage_id = quest.start_stage_id
  if type(start_stage_id) ~= "string" or not stage_ids[start_stage_id] then
    start_stage_id = stages[1] and stages[1].id or ""
  end

  return {
    id = quest_id,
    title = type(quest.title) == "string" and quest.title or quest_id,
    description = type(quest.description) == "string" and quest.description or "",
    status = type(quest.status) == "string" and quest.status or config.default_status or "inactive",
    start_stage_id = start_stage_id,
    stages = stages,
    completion_conditions = conditions,
    effects = effects,
    triggers = triggers,
    progress_tracks = type(quest.progress_tracks) == "table" and clone_array(quest.progress_tracks) or {},
    tags = normalize_tags(quest.tags),
    metadata = type(quest.metadata) == "table" and quest.metadata or {}
  }
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    add(diagnostics, "error", "quest_schema.config.invalid", "Config must be a table.", "config")
    return false, diagnostics
  end
  local fields = { "allowed_objective_types", "allowed_condition_types", "allowed_effect_types" }
  for field_index = 1, #fields do
    local field = fields[field_index]
    local value = config[field]
    if value ~= nil then
      if not is_array(value) then
        add(diagnostics, "error", "quest_schema.config.list_invalid", field .. " must be an array.", "config/" .. field)
      else
        for index = 1, #value do
          if type(value[index]) ~= "string" or value[index] == "" then
            add(diagnostics, "error", "quest_schema.config.value_invalid", field .. " values must be non-empty strings.", "config/" .. field .. "/" .. tostring(index))
          end
        end
      end
    end
  end
  if config.default_status ~= nil and type(config.default_status) ~= "string" then
    add(diagnostics, "error", "quest_schema.config.status_invalid", "default_status must be a string.", "config/default_status")
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
    add(diagnostics, "error", "quest_schema.input.invalid", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local source_quests = nil
  if type(input.quests) == "table" then
    source_quests = input.quests
  else
    source_quests = { input }
  end

  local quests = {}
  local quest_ids = {}
  local trigger_count = 0
  local objective_count = 0
  for quest_index = 1, #source_quests do
    local quest = normalize_quest(source_quests[quest_index], quest_index, config, diagnostics)
    if quest then
      if quest_ids[quest.id] then
        add(diagnostics, "error", "quest.duplicate_id", "Duplicate quest id.", "quests/" .. quest.id)
      else
        quest_ids[quest.id] = true
        quests[#quests + 1] = quest
        trigger_count = trigger_count + #quest.triggers
        for stage_index = 1, #quest.stages do
          objective_count = objective_count + #quest.stages[stage_index].objectives
        end
      end
    end
  end

  local has_error = false
  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      has_error = true
      break
    end
  end

  return {
    ok = not has_error,
    data = {
      quests = quests,
      summary = {
        quest_count = #quests,
        objective_count = objective_count,
        trigger_count = trigger_count,
        supports_dialogue_triggers = true,
        supports_interaction_triggers = true,
        supports_abstract_progress = true
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
