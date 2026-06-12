local M = {}

M.manifest = {
  id = "generation/dependency_sort/v1",
  version = "0.1.0",
  category = "generation",
  title = "Dependency sort",
  purpose = "Create deterministic dependency order IR from plain module or plan-step metadata.",
  capabilities = {
    "generation.dependencies.sort",
    "generation.dependencies.validate",
    "generation.plan_steps.order"
  },
  input_schema = {
    modules = "array of { id, depends_on }",
    steps = "array of { id, depends_on }"
  },
  output_schema = {
    ordered_ids = "array",
    ordered_items = "array",
    diagnostics = "array"
  },
  config_schema = {},
  deterministic = true,
  runtime_targets = { "editor", "validation", "simulation", "codegen_ir", "generator_plan" },
  unsafe_features = {}
}

local function diag(severity, code, message, target)
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

local function copy_list(value)
  local result = {}
  if is_array(value) then
    for index = 1, #value do
      result[#result + 1] = value[index]
    end
  end
  return result
end

local function valid_id(value)
  return type(value) == "string" and value ~= "" and value:match("^[a-z0-9_/%-%.]+$") ~= nil
end

local function normalize_items(config)
  local source = {}
  if is_array(config.modules) then
    source = config.modules
  elseif is_array(config.steps) then
    source = config.steps
  elseif is_array(config.items) then
    source = config.items
  end

  local items = {}
  for index = 1, #source do
    local item = source[index]
    local entry = {
      id = type(item) == "table" and item.id or nil,
      order = index,
      kind = type(item) == "table" and item.kind or nil,
      depends_on = type(item) == "table" and copy_list(item.depends_on) or {}
    }
    items[#items + 1] = entry
  end
  return items
end

local function sort_ready(ready)
  table.sort(ready, function(a, b)
    if a.order == b.order then
      return a.id < b.id
    end
    return a.order < b.order
  end)
end

local function contains_id(list, id)
  for index = 1, #list do
    if list[index] == id then
      return true
    end
  end
  return false
end

local function topo_sort(items)
  local diagnostics = {}
  local by_id = {}
  local duplicates = {}
  local indegree = {}
  local reverse = {}
  local missing = {}

  for index = 1, #items do
    local item = items[index]
    if not valid_id(item.id) then
      diagnostics[#diagnostics + 1] = diag("error", "invalid_id", "Item id must be a lowercase slash or dot id.", "items[" .. index .. "].id")
    elseif by_id[item.id] ~= nil then
      duplicates[#duplicates + 1] = item.id
      diagnostics[#diagnostics + 1] = diag("error", "duplicate_id", "Duplicate dependency item id.", item.id)
    else
      by_id[item.id] = item
      indegree[item.id] = 0
      reverse[item.id] = {}
    end
  end

  for index = 1, #items do
    local item = items[index]
    if valid_id(item.id) and by_id[item.id] == item then
      for dep_index = 1, #item.depends_on do
        local dep = item.depends_on[dep_index]
        if not valid_id(dep) then
          diagnostics[#diagnostics + 1] = diag("error", "invalid_dependency_id", "Dependency id must be a lowercase slash or dot id.", item.id .. ".depends_on[" .. dep_index .. "]")
        elseif by_id[dep] == nil then
          local key = item.id .. "->" .. dep
          if not missing[key] then
            missing[key] = true
            diagnostics[#diagnostics + 1] = diag("error", "missing_dependency", "Dependency target is not present in the supplied metadata.", item.id .. " depends_on " .. dep)
          end
        else
          indegree[item.id] = indegree[item.id] + 1
          reverse[dep][#reverse[dep] + 1] = item.id
        end
      end
    end
  end

  local ready = {}
  for id, item in pairs(by_id) do
    if indegree[id] == 0 then
      ready[#ready + 1] = item
    end
  end
  sort_ready(ready)

  local ordered = {}
  local ordered_items = {}
  local position = 1
  while position <= #ready do
    local item = ready[position]
    position = position + 1
    ordered[#ordered + 1] = item.id
    ordered_items[#ordered_items + 1] = {
      id = item.id,
      order = #ordered,
      original_order = item.order,
      kind = item.kind,
      depends_on = copy_list(item.depends_on)
    }

    local children = reverse[item.id]
    table.sort(children)
    for child_index = 1, #children do
      local child_id = children[child_index]
      indegree[child_id] = indegree[child_id] - 1
      if indegree[child_id] == 0 then
        ready[#ready + 1] = by_id[child_id]
        sort_ready(ready)
      end
    end
  end

  local cyclic = {}
  for id, item in pairs(by_id) do
    if not contains_id(ordered, id) then
      cyclic[#cyclic + 1] = id
    end
  end
  table.sort(cyclic)
  for index = 1, #cyclic do
    diagnostics[#diagnostics + 1] = diag("error", "cyclic_dependency", "Dependency cycle prevents deterministic ordering.", cyclic[index])
  end

  return {
    ok = #diagnostics == 0,
    ordered_ids = ordered,
    ordered_items = ordered_items,
    duplicate_ids = duplicates,
    cyclic_ids = cyclic,
    diagnostics = diagnostics
  }
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return { ok = true, diagnostics = diagnostics }
  end
  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "invalid_config", "Config must be a table.", "config")
    return { ok = false, diagnostics = diagnostics }
  end
  if config.modules ~= nil and not is_array(config.modules) then
    diagnostics[#diagnostics + 1] = diag("error", "invalid_modules", "modules must be an array when present.", "modules")
  end
  if config.steps ~= nil and not is_array(config.steps) then
    diagnostics[#diagnostics + 1] = diag("error", "invalid_steps", "steps must be an array when present.", "steps")
  end
  return { ok = #diagnostics == 0, diagnostics = diagnostics }
end

function M.generate(input, ctx)
  local config = {}
  if type(input) == "table" then
    config = input
  end

  local base_validation = M.validate_config(config)
  local items = normalize_items(config)
  local sorted = topo_sort(items)

  local diagnostics = {}
  for index = 1, #base_validation.diagnostics do
    diagnostics[#diagnostics + 1] = base_validation.diagnostics[index]
  end
  for index = 1, #sorted.diagnostics do
    diagnostics[#diagnostics + 1] = sorted.diagnostics[index]
  end

  return {
    ok = #diagnostics == 0,
    data = {
      plan_id = config.plan_id or "generation/dependency_sort/default",
      sort_policy = "deterministic_original_order_then_id",
      ordered_ids = sorted.ordered_ids,
      ordered_items = sorted.ordered_items,
      duplicate_ids = sorted.duplicate_ids,
      cyclic_ids = sorted.cyclic_ids,
      item_count = #items,
      ctx_tag = type(ctx) == "table" and ctx.tag or nil
    },
    diagnostics = diagnostics,
    artifacts = {
      {
        id = (config.plan_id or "generation/dependency_sort/default") .. "/order",
        kind = "dependency_order_ir",
        produced_by = M.manifest.id
      }
    }
  }
end

return M
