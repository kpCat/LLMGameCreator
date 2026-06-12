local M = {}

M.manifest = {
  id = "faction/faction_model/v1",
  version = "0.1.0",
  category = "faction",
  title = "Faction model generator",
  purpose = "Generate compact faction, role and relationship IR for NPC archetypes and future quest/dialogue systems.",
  capabilities = {
    "faction.model.generate",
    "faction.role.define",
    "faction.relationship_matrix",
    "progression.reputation_reference"
  },
  input_schema = {
    type = "object",
    required = { "factions" }
  },
  output_schema = {
    type = "object",
    fields = { "factions", "roles", "relations", "indexes" }
  },
  config_schema = {
    type = "object",
    fields = { "default_relation", "allow_unknown_roles" }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "unity_ui_ir", "codegen_ir" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local function diag(severity, code, message, target)
  return {
    severity = severity,
    code = code,
    message = message,
    target = target
  }
end

local function add_diag(list, severity, code, message, target)
  list[#list + 1] = diag(severity, code, message, target)
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
    if key > count then
      count = key
    end
  end
  for index = 1, count do
    if value[index] == nil then
      return false
    end
  end
  return true
end

local function is_slash_id(value)
  return type(value) == "string" and value:match("^[a-z][a-z0-9_]*(/[a-z][a-z0-9_]*)*$") ~= nil
end

local function is_dot_id(value)
  return type(value) == "string" and value:match("^[a-z][a-z0-9_]*(%.[a-z][a-z0-9_]*)*$") ~= nil
end

local function copy_array(value)
  local output = {}
  if is_array(value) then
    for index = 1, #value do
      output[#output + 1] = value[index]
    end
  end
  return output
end

local function normalize_relation(value, default_value)
  if value == "ally" or value == "friendly" or value == "neutral" or value == "tense" or value == "hostile" then
    return value
  end
  return default_value or "neutral"
end

local function validate_relation_entry(entry, index, diagnostics)
  local target = "relations[" .. tostring(index) .. "]"
  if type(entry) ~= "table" then
    add_diag(diagnostics, "error", "faction.relation_not_table", "Relation entry must be a table.", target)
    return false
  end
  local ok = true
  if not is_slash_id(entry.from) then
    add_diag(diagnostics, "error", "faction.invalid_relation_from", "Relation source faction id must use lowercase slash notation.", target .. ".from")
    ok = false
  end
  if not is_slash_id(entry.to) then
    add_diag(diagnostics, "error", "faction.invalid_relation_to", "Relation target faction id must use lowercase slash notation.", target .. ".to")
    ok = false
  end
  if entry.state ~= nil and normalize_relation(entry.state, nil) == nil then
    add_diag(diagnostics, "error", "faction.invalid_relation_state", "Relation state must be ally, friendly, neutral, tense or hostile.", target .. ".state")
    ok = false
  end
  return ok
end

local function validate_role(role, index, diagnostics)
  local target = "roles[" .. tostring(index) .. "]"
  if type(role) ~= "table" then
    add_diag(diagnostics, "error", "faction.role_not_table", "Role entry must be a table.", target)
    return false
  end
  local ok = true
  if not is_slash_id(role.id) then
    add_diag(diagnostics, "error", "faction.invalid_role_id", "Role id must use lowercase slash notation.", target .. ".id")
    ok = false
  end
  if role.capabilities ~= nil then
    if not is_array(role.capabilities) then
      add_diag(diagnostics, "error", "faction.invalid_role_capabilities", "Role capabilities must be an array.", target .. ".capabilities")
      ok = false
    else
      for cap_index = 1, #role.capabilities do
        if not is_dot_id(role.capabilities[cap_index]) then
          add_diag(diagnostics, "error", "faction.invalid_role_capability", "Role capability must use lowercase dot notation.", target .. ".capabilities[" .. tostring(cap_index) .. "]")
          ok = false
        end
      end
    end
  end
  return ok
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    add_diag(diagnostics, "error", "faction.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.default_relation ~= nil then
    local normalized = normalize_relation(config.default_relation, nil)
    if normalized == nil then
      add_diag(diagnostics, "error", "faction.invalid_default_relation", "default_relation must be ally, friendly, neutral, tense or hostile.", "config.default_relation")
    end
  end
  if config.allow_unknown_roles ~= nil and type(config.allow_unknown_roles) ~= "boolean" then
    add_diag(diagnostics, "error", "faction.invalid_allow_unknown_roles", "allow_unknown_roles must be a boolean.", "config.allow_unknown_roles")
  end
  return #diagnostics == 0, diagnostics
end

local function validate_input(input, diagnostics)
  if type(input) ~= "table" then
    add_diag(diagnostics, "error", "faction.input_not_table", "Input must be a table.", "input")
    return false
  end

  local ok = true

  if input.factions == nil then
    add_diag(diagnostics, "error", "faction.missing_factions", "Input must contain factions array.", "input.factions")
    return false
  end

  if not is_array(input.factions) then
    add_diag(diagnostics, "error", "faction.factions_not_array", "factions must be an array.", "input.factions")
    return false
  end

  local seen = {}
  for index = 1, #input.factions do
    local faction = input.factions[index]
    local target = "factions[" .. tostring(index) .. "]"
    if type(faction) ~= "table" then
      add_diag(diagnostics, "error", "faction.entry_not_table", "Faction entry must be a table.", target)
      ok = false
    else
      if not is_slash_id(faction.id) then
        add_diag(diagnostics, "error", "faction.invalid_faction_id", "Faction id must use lowercase slash notation.", target .. ".id")
        ok = false
      elseif seen[faction.id] then
        add_diag(diagnostics, "error", "faction.duplicate_faction_id", "Faction id must be unique.", target .. ".id")
        ok = false
      else
        seen[faction.id] = true
      end

      if faction.roles ~= nil and not is_array(faction.roles) then
        add_diag(diagnostics, "error", "faction.roles_not_array", "Faction roles must be an array of role ids.", target .. ".roles")
        ok = false
      elseif faction.roles ~= nil then
        for role_index = 1, #faction.roles do
          if not is_slash_id(faction.roles[role_index]) then
            add_diag(diagnostics, "error", "faction.invalid_faction_role_ref", "Faction role reference must use lowercase slash notation.", target .. ".roles[" .. tostring(role_index) .. "]")
            ok = false
          end
        end
      end

      if faction.reputation_track_id ~= nil and not is_slash_id(faction.reputation_track_id) then
        add_diag(diagnostics, "error", "faction.invalid_reputation_track", "reputation_track_id must use lowercase slash notation.", target .. ".reputation_track_id")
        ok = false
      end
    end
  end

  if input.roles ~= nil then
    if not is_array(input.roles) then
      add_diag(diagnostics, "error", "faction.roles_catalog_not_array", "roles must be an array.", "input.roles")
      ok = false
    else
      for index = 1, #input.roles do
        if not validate_role(input.roles[index], index, diagnostics) then
          ok = false
        end
      end
    end
  end

  if input.relations ~= nil then
    if not is_array(input.relations) then
      add_diag(diagnostics, "error", "faction.relations_not_array", "relations must be an array.", "input.relations")
      ok = false
    else
      for index = 1, #input.relations do
        if not validate_relation_entry(input.relations[index], index, diagnostics) then
          ok = false
        end
      end
    end
  end

  return ok
end

local function build_role_catalog(roles)
  local list = {}
  local by_id = {}
  if is_array(roles) then
    for index = 1, #roles do
      local role = roles[index]
      if type(role) == "table" and is_slash_id(role.id) then
        local item = {
          id = role.id,
          title = role.title or role.id,
          purpose = role.purpose or "faction_role",
          capabilities = copy_array(role.capabilities),
          tags = copy_array(role.tags)
        }
        list[#list + 1] = item
        by_id[item.id] = item
      end
    end
  end
  return list, by_id
end

function M.generate(input, ctx)
  local diagnostics = {}
  local config = {}
  if type(input) == "table" and type(input.config) == "table" then
    config = input.config
  end

  local config_ok, config_diagnostics = M.validate_config(config)
  for index = 1, #config_diagnostics do
    diagnostics[#diagnostics + 1] = config_diagnostics[index]
  end

  local input_ok = validate_input(input, diagnostics)
  if not config_ok or not input_ok then
    return {
      ok = false,
      data = {},
      diagnostics = diagnostics,
      artifacts = {}
    }
  end

  local default_relation = normalize_relation(config.default_relation, "neutral")
  local role_list, role_by_id = build_role_catalog(input.roles)

  local factions = {}
  local faction_by_id = {}
  local faction_ids = {}

  for index = 1, #input.factions do
    local source = input.factions[index]
    local faction = {
      id = source.id,
      title = source.title or source.id,
      purpose = source.purpose or "npc_faction",
      default_attitude = normalize_relation(source.default_attitude, default_relation),
      roles = copy_array(source.roles),
      tags = copy_array(source.tags),
      reputation_track_id = source.reputation_track_id,
      metadata = type(source.metadata) == "table" and source.metadata or {}
    }
    factions[#factions + 1] = faction
    faction_by_id[faction.id] = faction
    faction_ids[#faction_ids + 1] = faction.id
  end

  for index = 1, #factions do
    local faction = factions[index]
    for role_index = 1, #faction.roles do
      local role_id = faction.roles[role_index]
      if role_by_id[role_id] == nil and config.allow_unknown_roles ~= true then
        add_diag(diagnostics, "warning", "faction.unknown_role_reference", "Faction references a role not present in the role catalog.", "factions[" .. tostring(index) .. "].roles[" .. tostring(role_index) .. "]")
      end
    end
  end

  local relation_map = {}
  local relations = {}

  for from_index = 1, #faction_ids do
    local from_id = faction_ids[from_index]
    relation_map[from_id] = {}
    for to_index = 1, #faction_ids do
      local to_id = faction_ids[to_index]
      local state = default_relation
      if from_id == to_id then
        state = "ally"
      end
      relation_map[from_id][to_id] = state
    end
  end

  if is_array(input.relations) then
    for index = 1, #input.relations do
      local relation = input.relations[index]
      if type(relation) == "table" and is_slash_id(relation.from) and is_slash_id(relation.to) then
        if relation_map[relation.from] == nil then
          add_diag(diagnostics, "warning", "faction.unknown_relation_from", "Relation source faction is not present in factions.", "relations[" .. tostring(index) .. "].from")
        elseif relation_map[relation.from][relation.to] == nil then
          add_diag(diagnostics, "warning", "faction.unknown_relation_to", "Relation target faction is not present in factions.", "relations[" .. tostring(index) .. "].to")
        else
          relation_map[relation.from][relation.to] = normalize_relation(relation.state, default_relation)
        end
      end
    end
  end

  for from_index = 1, #faction_ids do
    local from_id = faction_ids[from_index]
    for to_index = 1, #faction_ids do
      local to_id = faction_ids[to_index]
      relations[#relations + 1] = {
        from = from_id,
        to = to_id,
        state = relation_map[from_id][to_id]
      }
    end
  end

  return {
    ok = true,
    data = {
      factions = factions,
      roles = role_list,
      relations = relations,
      indexes = {
        faction_ids = faction_ids,
        role_ids = (function()
          local ids = {}
          for index = 1, #role_list do
            ids[#ids + 1] = role_list[index].id
          end
          return ids
        end)()
      }
    },
    diagnostics = diagnostics,
    artifacts = {
      {
        kind = "faction_model",
        id = "artifact/faction_model",
        summary = "Compact faction model IR with role catalog and relationship matrix."
      }
    }
  }
end

return M
