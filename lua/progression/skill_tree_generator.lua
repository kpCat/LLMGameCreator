local M = {}

M.manifest = {
  id = "progression/skill_tree_generator/v1",
  version = "0.1.0",
  category = "progression",
  title = "Skill Tree Generator",
  purpose = "Generate compact skill tree IR with node costs, prerequisites, unlocks, stat modifiers and formula references.",
  capabilities = { "progression.skill_tree.generate", "progression.unlock_graph", "stats.modifier_reference" },
  input_schema = {
    tree = "skill tree definition with branches or explicit nodes"
  },
  output_schema = {
    tree = "normalized tree metadata",
    nodes = "skill nodes",
    edges = "prerequisite edges",
    indexes = "roots and nodes by branch"
  },
  config_schema = {
    max_nodes = "optional positive integer",
    default_currency = "optional progress currency id",
    allow_or_prerequisites = "optional boolean"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "unity_ui_ir", "codegen_ir" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

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

local function non_negative_integer(value)
  return type(value) == "number" and value >= 0 and value % 1 == 0
end

local function clone_array(value)
  local result = {}
  if type(value) == "table" then for index = 1, #value do result[#result + 1] = value[index] end end
  return result
end

local function normalize_tags(value)
  local result = {}
  local seen = {}
  if type(value) == "table" then
    for index = 1, #value do
      if token_ok(value[index]) and not seen[value[index]] then
        seen[value[index]] = true
        result[#result + 1] = value[index]
      end
    end
  end
  return result
end

local function normalize_requires(value, diagnostics, target, allow_or)
  local mode = "all"
  local refs = value
  if type(value) == "table" and value.mode ~= nil then
    mode = value.mode
    refs = value.nodes or value.refs or {}
  end
  if mode ~= "all" and mode ~= "any" then
    add(diagnostics, "warning", "skill.invalid_requires_mode", "Prerequisite mode replaced by all.", target .. ".mode")
    mode = "all"
  end
  if mode == "any" and not allow_or then
    add(diagnostics, "warning", "skill.or_requires_disabled", "Any-mode prerequisites replaced by all because config disables OR prerequisites.", target .. ".mode")
    mode = "all"
  end
  if refs == nil then refs = {} end
  if type(refs) == "string" then refs = { refs } end
  if not is_array(refs) then
    add(diagnostics, "error", "skill.requires_not_array", "Skill prerequisites must be an array of node ids.", target)
    refs = {}
  end
  local out = {}
  local seen = {}
  for index = 1, #refs do
    local ref = refs[index]
    if id_ok(ref) and not seen[ref] then
      seen[ref] = true
      out[#out + 1] = ref
    else
      add(diagnostics, "warning", "skill.invalid_prerequisite", "Invalid or duplicate prerequisite ignored.", target .. "[" .. index .. "]")
    end
  end
  return { mode = mode, nodes = out }
end

local function normalize_effects(value, diagnostics, target)
  if value == nil then return {} end
  if not is_array(value) then
    add(diagnostics, "warning", "skill.effects_not_array", "Effects must be an array; ignored.", target)
    return {}
  end
  local out = {}
  for index = 1, #value do
    local effect = value[index]
    if type(effect) == "table" then
      out[#out + 1] = {
        type = token_ok(effect.type) and effect.type or "modifier",
        target = id_ok(effect.target) and effect.target or "stats/unknown",
        formula_id = effect.formula_id and id_ok(effect.formula_id) and effect.formula_id or nil,
        value = type(effect.value) == "number" and effect.value or nil,
        tags = normalize_tags(effect.tags)
      }
    else
      add(diagnostics, "warning", "skill.invalid_effect", "Effect must be a table; ignored.", target .. "[" .. index .. "]")
    end
  end
  return out
end

local function normalize_node(value, branch_id, default_currency, diagnostics, target)
  if type(value) ~= "table" then
    add(diagnostics, "error", "skill.node_not_table", "Skill node must be a table.", target)
    return nil
  end
  local id = value.id
  if not id_ok(id) then
    add(diagnostics, "error", "skill.invalid_id", "Skill node id must be a lowercase slash id.", target .. ".id")
    return nil
  end
  local cost = value.cost or 1
  if not non_negative_integer(cost) then
    add(diagnostics, "warning", "skill.invalid_cost", "Skill node cost replaced by 1.", target .. ".cost")
    cost = 1
  end
  local tier = value.tier or 1
  if not positive_integer(tier) then
    add(diagnostics, "warning", "skill.invalid_tier", "Skill node tier replaced by 1.", target .. ".tier")
    tier = 1
  end
  return {
    id = id,
    title = type(value.title) == "string" and value.title or id,
    branch_id = branch_id,
    tier = tier,
    cost = { amount = cost, currency = value.currency or default_currency },
    requires = value.requires,
    unlocks = clone_array(value.unlocks),
    effects = normalize_effects(value.effects or value.modifiers, diagnostics, target .. ".effects"),
    tags = normalize_tags(value.tags),
    ui = type(value.ui) == "table" and value.ui or {}
  }
end

local function collect_nodes(tree, diagnostics, default_currency, max_nodes)
  local nodes = {}
  if is_array(tree.nodes or {}) then
    for index = 1, #(tree.nodes or {}) do
      if #nodes < max_nodes then
        local node = normalize_node(tree.nodes[index], tree.nodes[index].branch_id or "core", default_currency, diagnostics, "input.tree.nodes[" .. index .. "]")
        if node then nodes[#nodes + 1] = node end
      end
    end
  end
  if is_array(tree.branches or {}) then
    for branch_index = 1, #tree.branches do
      local branch = tree.branches[branch_index]
      local branch_id = branch.id or ("branch_" .. branch_index)
      if not id_ok(branch_id) then
        add(diagnostics, "warning", "skill.invalid_branch_id", "Invalid branch id replaced by generated branch id.", "input.tree.branches[" .. branch_index .. "].id")
        branch_id = "branch_" .. branch_index
      end
      local branch_nodes = branch.nodes or {}
      if is_array(branch_nodes) then
        for node_index = 1, #branch_nodes do
          if #nodes < max_nodes then
            local node = normalize_node(branch_nodes[node_index], branch_id, default_currency, diagnostics, "input.tree.branches[" .. branch_index .. "].nodes[" .. node_index .. "]")
            if node then nodes[#nodes + 1] = node end
          else
            add(diagnostics, "warning", "skill.max_nodes_reached", "Remaining skill nodes ignored because max_nodes was reached.", "input.tree")
            return nodes
          end
        end
      end
    end
  end
  return nodes
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and type(config) ~= "table" then
    add(diagnostics, "error", "skill.config_not_table", "Config must be a table when provided.", "config")
  elseif type(config) == "table" then
    if config.max_nodes ~= nil and not positive_integer(config.max_nodes) then
      add(diagnostics, "error", "skill.invalid_max_nodes", "max_nodes must be a positive integer.", "config.max_nodes")
    end
    if config.default_currency ~= nil and not id_ok(config.default_currency) then
      add(diagnostics, "error", "skill.invalid_default_currency", "default_currency must be a lowercase slash id.", "config.default_currency")
    end
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local config = ctx and type(ctx.config) == "table" and ctx.config or {}
  local max_nodes = positive_integer(config.max_nodes) and config.max_nodes or 64
  local default_currency = id_ok(config.default_currency) and config.default_currency or "progression/skill_points"
  local allow_or = config.allow_or_prerequisites ~= false
  local tree = input and (input.tree or input.skill_tree or input) or {}
  if type(tree) ~= "table" then
    add(diagnostics, "error", "skill.input_not_table", "Input tree must be a table.", "input.tree")
    tree = {}
  end
  local tree_id = tree.id or "progression/skill_tree/default"
  if not id_ok(tree_id) then
    add(diagnostics, "error", "skill.invalid_tree_id", "Skill tree id must be a lowercase slash id.", "input.tree.id")
    tree_id = "progression/skill_tree/invalid"
  end
  local nodes = collect_nodes(tree, diagnostics, default_currency, max_nodes)
  local by_id = {}
  local by_branch = {}
  for index = 1, #nodes do
    local node = nodes[index]
    if by_id[node.id] then
      add(diagnostics, "error", "skill.duplicate_node", "Duplicate skill node id detected.", node.id)
    end
    by_id[node.id] = true
    if not by_branch[node.branch_id] then by_branch[node.branch_id] = {} end
    by_branch[node.branch_id][#by_branch[node.branch_id] + 1] = node.id
  end
  local edges = {}
  local roots = {}
  for index = 1, #nodes do
    local node = nodes[index]
    node.requires = normalize_requires(node.requires, diagnostics, node.id .. ".requires", allow_or)
    if #node.requires.nodes == 0 then roots[#roots + 1] = node.id end
    for ref_index = 1, #node.requires.nodes do
      local ref = node.requires.nodes[ref_index]
      if not by_id[ref] then
        add(diagnostics, "error", "skill.missing_prerequisite", "Skill node references missing prerequisite.", node.id .. ".requires[" .. ref_index .. "]")
      end
      edges[#edges + 1] = { from = ref, to = node.id, mode = node.requires.mode }
    end
  end
  return {
    ok = #diagnostics == 0,
    data = {
      tree = { id = tree_id, title = tree.title or tree_id, currency = default_currency, ui_mode = tree.ui_mode or "rpg_hud" },
      nodes = nodes,
      edges = edges,
      indexes = { by_id = by_id, by_branch = by_branch, roots = roots },
      summary = { node_count = #nodes, edge_count = #edges, root_count = #roots }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
