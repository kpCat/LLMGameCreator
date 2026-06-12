local M = {}

M.manifest = {
  id = "validation/quest_validation/v1",
  version = "0.1.0",
  category = "validation",
  title = "Quest validation",
  purpose = "Validate quest, stage, objective, condition, transition and effect reference IR without executing gameplay logic.",
  capabilities = {
    "validation.quest.validate",
    "validation.quest.conditions",
    "validation.quest.transitions"
  },
  input_schema = {
    type = "object",
    required = { "quests" }
  },
  output_schema = {
    type = "object",
    fields = { "summary", "quest_ids" }
  },
  config_schema = {
    type = "object",
    fields = { "allow_transition_cycles" }
  },
  deterministic = true,
  runtime_targets = { "editor", "validation", "simulation", "unity_ir", "codegen_ir" },
  unsafe_features = {}
}

local function diagnostic(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
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

local function is_id(value)
  if type(value) ~= "string" or value == "" then
    return false
  end
  if value:sub(1, 1) == "/" or value:sub(-1) == "/" then
    return false
  end
  if value:find("//", 1, true) then
    return false
  end
  return value:match("^[a-z0-9_][a-z0-9_/%-]*$") ~= nil
end

local function make_index(items, label, diagnostics, target)
  local index = {}
  if items == nil then
    return index
  end
  if not is_array(items) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.invalid_" .. label .. "_list", label .. " list must be an array.", target)
    return index
  end
  for i = 1, #items do
    local item = items[i]
    if type(item) ~= "table" or not is_id(item.id) then
      diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.invalid_" .. label .. "_id", label .. " has invalid id.", target .. "[" .. tostring(i) .. "]")
    elseif index[item.id] then
      diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.duplicate_" .. label .. "_id", label .. " id is duplicated.", item.id)
    else
      index[item.id] = item
    end
  end
  return index
end

local function validate_conditions(conditions, diagnostics, base_target)
  if conditions == nil then
    return
  end
  if not is_array(conditions) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.invalid_conditions", "Conditions must be an array.", base_target)
    return
  end
  local allowed = {
    has_item = true,
    objective_complete = true,
    stage_complete = true,
    progress_at_least = true,
    flag_set = true,
    location_discovered = true,
    dialogue_choice = true
  }
  for i = 1, #conditions do
    local condition = conditions[i]
    local target = base_target .. "[" .. tostring(i) .. "]"
    if type(condition) ~= "table" then
      diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.condition_not_table", "Condition must be a table.", target)
    else
      if type(condition.type) ~= "string" or not allowed[condition.type] then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.invalid_condition_type", "Condition type is invalid.", target .. ".type")
      end
      if condition.ref ~= nil and not is_id(condition.ref) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.invalid_condition_ref", "Condition reference id is invalid.", target .. ".ref")
      end
      if condition.value ~= nil and type(condition.value) == "function" then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.invalid_condition_value", "Condition value must be plain data.", target .. ".value")
      end
    end
  end
end

local function mark_cycles(stage_index, transitions, diagnostics, quest_target)
  local adjacency = {}
  for id, _ in pairs(stage_index) do
    adjacency[id] = {}
  end
  if is_array(transitions) then
    for i = 1, #transitions do
      local transition = transitions[i]
      if type(transition) == "table" and stage_index[transition.from_stage_id] and stage_index[transition.to_stage_id] then
        adjacency[transition.from_stage_id][#adjacency[transition.from_stage_id] + 1] = transition.to_stage_id
      end
    end
  end

  local visiting = {}
  local visited = {}
  local cycle_reported = false

  local function visit(id)
    if cycle_reported then
      return
    end
    if visiting[id] then
      diagnostics[#diagnostics + 1] = diagnostic("warning", "validation.quest.transition_cycle", "Quest stage transitions contain a cycle.", quest_target)
      cycle_reported = true
      return
    end
    if visited[id] then
      return
    end
    visiting[id] = true
    local next_list = adjacency[id] or {}
    for i = 1, #next_list do
      visit(next_list[i])
    end
    visiting[id] = nil
    visited[id] = true
  end

  for id, _ in pairs(stage_index) do
    visit(id)
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.config_not_table", "Config must be a table.", "config")
  end
  if type(config) == "table" and config.allow_transition_cycles ~= nil and type(config.allow_transition_cycles) ~= "boolean" then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.invalid_cycle_flag", "allow_transition_cycles must be boolean.", "config.allow_transition_cycles")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local config = type(ctx) == "table" and type(ctx.config) == "table" and ctx.config or {}
  local ok_config, config_diagnostics = M.validate_config(config)
  for i = 1, #config_diagnostics do
    diagnostics[#diagnostics + 1] = config_diagnostics[i]
  end

  if type(input) ~= "table" or not is_array(input.quests) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.input_invalid", "Input must contain quests array.", "input.quests")
    return { ok = false, data = { summary = { checked = 0 }, quest_ids = {} }, diagnostics = diagnostics, artifacts = {} }
  end

  local quest_index = make_index(input.quests, "quest", diagnostics, "input.quests")
  local quest_ids = {}

  for qi = 1, #input.quests do
    local quest = input.quests[qi]
    if type(quest) == "table" and is_id(quest.id) then
      quest_ids[#quest_ids + 1] = quest.id
      local quest_target = "quests[" .. tostring(qi) .. "]"
      local stage_index = make_index(quest.stages, "stage", diagnostics, quest_target .. ".stages")
      local objective_index = make_index(quest.objectives, "objective", diagnostics, quest_target .. ".objectives")

      if quest.start_stage_id ~= nil and not stage_index[quest.start_stage_id] then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.missing_start_stage", "Quest start stage is missing.", quest_target .. ".start_stage_id")
      end

      if is_array(quest.stages or {}) then
        for si = 1, #quest.stages do
          local stage = quest.stages[si]
          local stage_target = quest_target .. ".stages[" .. tostring(si) .. "]"
          if type(stage) == "table" then
            if is_array(stage.objective_ids or {}) then
              for oi = 1, #stage.objective_ids do
                if not objective_index[stage.objective_ids[oi]] then
                  diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.missing_stage_objective", "Stage references missing objective.", stage_target .. ".objective_ids[" .. tostring(oi) .. "]")
                end
              end
            end
            validate_conditions(stage.completion_conditions, diagnostics, stage_target .. ".completion_conditions")
          end
        end
      end

      if is_array(quest.objectives or {}) then
        for oi = 1, #quest.objectives do
          local objective = quest.objectives[oi]
          local objective_target = quest_target .. ".objectives[" .. tostring(oi) .. "]"
          if type(objective) == "table" then
            if type(objective.type) ~= "string" or objective.type == "" then
              diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.missing_objective_type", "Objective type is required.", objective_target .. ".type")
            end
            if objective.target_ref ~= nil and not is_id(objective.target_ref) then
              diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.invalid_objective_target", "Objective target reference is invalid.", objective_target .. ".target_ref")
            end
            validate_conditions(objective.completion_conditions, diagnostics, objective_target .. ".completion_conditions")
          end
        end
      end

      local transitions = quest.transitions or {}
      if not is_array(transitions) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.invalid_transitions", "Quest transitions must be an array.", quest_target .. ".transitions")
      else
        for ti = 1, #transitions do
          local transition = transitions[ti]
          local transition_target = quest_target .. ".transitions[" .. tostring(ti) .. "]"
          if type(transition) ~= "table" then
            diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.transition_not_table", "Transition must be a table.", transition_target)
          else
            if not stage_index[transition.from_stage_id] then
              diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.missing_transition_from", "Transition source stage is missing.", transition_target .. ".from_stage_id")
            end
            if not stage_index[transition.to_stage_id] then
              diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.missing_transition_to", "Transition target stage is missing.", transition_target .. ".to_stage_id")
            end
            validate_conditions(transition.conditions, diagnostics, transition_target .. ".conditions")
          end
        end
        if config.allow_transition_cycles ~= true then
          mark_cycles(stage_index, transitions, diagnostics, quest_target)
        end
      end

      if is_array(quest.effects or {}) then
        for ei = 1, #quest.effects do
          local effect = quest.effects[ei]
          if type(effect) ~= "table" or type(effect.type) ~= "string" or effect.type == "" then
            diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.invalid_effect", "Quest effect must have a type.", quest_target .. ".effects[" .. tostring(ei) .. "]")
          elseif effect.ref ~= nil and not is_id(effect.ref) then
            diagnostics[#diagnostics + 1] = diagnostic("error", "validation.quest.invalid_effect_ref", "Quest effect reference is invalid.", quest_target .. ".effects[" .. tostring(ei) .. "].ref")
          end
        end
      end
    end
  end

  local has_errors = false
  for i = 1, #diagnostics do
    if diagnostics[i].severity == "error" then
      has_errors = true
      break
    end
  end

  local count = 0
  for _, _ in pairs(quest_index) do
    count = count + 1
  end

  return {
    ok = ok_config and not has_errors,
    data = {
      summary = {
        checked = #input.quests,
        unique_quests = count,
        diagnostic_count = #diagnostics
      },
      quest_ids = quest_ids
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
