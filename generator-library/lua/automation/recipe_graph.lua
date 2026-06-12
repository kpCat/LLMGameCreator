local M = {}

M.manifest = {
  id = "automation/recipe_graph/v1",
  version = "0.1.0",
  category = "automation",
  title = "Recipe graph",
  purpose = "Build compact deterministic recipe and production chain IR for automation-style generators.",
  capabilities = {
    "automation.recipe_graph.generate",
    "automation.production_chain.validate",
    "automation.throughput.estimate",
    "automation.resource_node.reference"
  },
  input_schema = {
    recipes = "array",
    targets = "array optional",
    resources = "array optional"
  },
  output_schema = {
    recipe_graph = "object",
    production_chains = "array",
    missing_inputs = "array"
  },
  config_schema = {
    max_chain_depth = "integer optional",
    default_rate_per_second = "number optional"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "simulation", "codegen_ir" },
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

local function copy_list(list)
  local result = {}
  if type(list) ~= "table" then
    return result
  end
  for index = 1, #list do
    result[index] = list[index]
  end
  return result
end

local function normalize_id(value, fallback)
  if type(value) == "string" and value ~= "" then
    return value
  end
  return fallback
end

local function add_amount(map, key, amount)
  if type(key) ~= "string" or key == "" then
    return
  end
  local current = map[key] or 0
  map[key] = current + (tonumber(amount) or 0)
end

local function sorted_keys(map)
  local keys = {}
  for key, _ in pairs(map) do
    keys[#keys + 1] = key
  end
  table.sort(keys)
  return keys
end

local function normalize_stack_list(list, diagnostics, target)
  local result = {}
  if type(list) ~= "table" then
    return result
  end

  for index = 1, #list do
    local entry = list[index]
    if type(entry) ~= "table" then
      diagnostics[#diagnostics + 1] = diag("error", "automation.recipe_graph.stack_not_table", "Item stack must be a table.", target .. "[" .. index .. "]")
    else
      local item_id = normalize_id(entry.item_id or entry.id or entry.item, "")
      local amount = tonumber(entry.amount) or 0
      if item_id == "" then
        diagnostics[#diagnostics + 1] = diag("error", "automation.recipe_graph.item_id_missing", "Item stack requires item_id.", target .. "[" .. index .. "]")
      end
      if amount <= 0 then
        diagnostics[#diagnostics + 1] = diag("error", "automation.recipe_graph.amount_invalid", "Item stack amount must be positive.", target .. "[" .. index .. "]")
      end
      result[#result + 1] = {
        item_id = item_id,
        amount = amount
      }
    end
  end

  table.sort(result, function(left, right)
    return left.item_id < right.item_id
  end)

  return result
end

local function normalize_recipe(raw, index, diagnostics, default_rate)
  if type(raw) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.recipe_graph.recipe_not_table", "Recipe must be a table.", "recipes[" .. index .. "]")
    return nil
  end

  local id = normalize_id(raw.id, "automation/recipe/generated_" .. index)
  local category = normalize_id(raw.category, "crafting")
  local craft_seconds = tonumber(raw.craft_seconds or raw.duration_seconds or raw.duration) or 1
  local rate = tonumber(raw.rate_per_second) or default_rate

  if craft_seconds <= 0 then
    diagnostics[#diagnostics + 1] = diag("error", "automation.recipe_graph.duration_invalid", "Recipe duration must be positive.", id)
    craft_seconds = 1
  end
  if rate <= 0 then
    diagnostics[#diagnostics + 1] = diag("warning", "automation.recipe_graph.rate_invalid", "Recipe rate must be positive; defaulted to 1.", id)
    rate = 1
  end

  local inputs = normalize_stack_list(raw.inputs or {}, diagnostics, id .. ".inputs")
  local outputs = normalize_stack_list(raw.outputs or {}, diagnostics, id .. ".outputs")
  if #outputs == 0 then
    diagnostics[#diagnostics + 1] = diag("error", "automation.recipe_graph.outputs_missing", "Recipe must define at least one output.", id)
  end

  return {
    id = id,
    category = category,
    craft_seconds = craft_seconds,
    rate_per_second = rate,
    inputs = inputs,
    outputs = outputs,
    tags = copy_list(raw.tags),
    machine_tags = copy_list(raw.machine_tags),
    source = normalize_id(raw.source, "generated")
  }
end

local function build_recipe_index(recipes, diagnostics)
  local by_id = {}
  local producers_by_item = {}
  local consumers_by_item = {}

  for index = 1, #recipes do
    local recipe = recipes[index]
    if by_id[recipe.id] ~= nil then
      diagnostics[#diagnostics + 1] = diag("error", "automation.recipe_graph.duplicate_recipe_id", "Duplicate recipe id.", recipe.id)
    end
    by_id[recipe.id] = recipe

    for out_index = 1, #recipe.outputs do
      local output = recipe.outputs[out_index]
      producers_by_item[output.item_id] = producers_by_item[output.item_id] or {}
      producers_by_item[output.item_id][#producers_by_item[output.item_id] + 1] = recipe.id
    end

    for in_index = 1, #recipe.inputs do
      local input = recipe.inputs[in_index]
      consumers_by_item[input.item_id] = consumers_by_item[input.item_id] or {}
      consumers_by_item[input.item_id][#consumers_by_item[input.item_id] + 1] = recipe.id
    end
  end

  for _, list in pairs(producers_by_item) do
    table.sort(list)
  end
  for _, list in pairs(consumers_by_item) do
    table.sort(list)
  end

  return by_id, producers_by_item, consumers_by_item
end

local function output_rate_for(recipe, item_id)
  for index = 1, #recipe.outputs do
    local output = recipe.outputs[index]
    if output.item_id == item_id then
      return output.amount / recipe.craft_seconds * recipe.rate_per_second
    end
  end
  return 0
end

local function input_rate_for(recipe, item_id)
  for index = 1, #recipe.inputs do
    local input = recipe.inputs[index]
    if input.item_id == item_id then
      return input.amount / recipe.craft_seconds * recipe.rate_per_second
    end
  end
  return 0
end

local function build_chain_for_item(item_id, amount_per_second, producers_by_item, by_id, resource_set, max_depth, diagnostics)
  local chain = {
    target_item_id = item_id,
    target_rate_per_second = amount_per_second,
    recipe_steps = {},
    resource_inputs = {},
    missing_inputs = {},
    cycle_edges = {}
  }

  local function visit(request_item_id, required_rate, depth, stack)
    if resource_set[request_item_id] then
      add_amount(chain.resource_inputs, request_item_id, required_rate)
      return
    end

    if depth > max_depth then
      chain.missing_inputs[#chain.missing_inputs + 1] = {
        item_id = request_item_id,
        required_rate_per_second = required_rate,
        reason = "max_depth"
      }
      diagnostics[#diagnostics + 1] = diag("warning", "automation.recipe_graph.max_depth_reached", "Production chain reached configured depth limit.", request_item_id)
      return
    end

    if stack[request_item_id] then
      chain.cycle_edges[#chain.cycle_edges + 1] = {
        item_id = request_item_id,
        reason = "cycle"
      }
      diagnostics[#diagnostics + 1] = diag("error", "automation.recipe_graph.cycle_detected", "Recipe chain cycle detected.", request_item_id)
      return
    end

    local producer_ids = producers_by_item[request_item_id]
    if producer_ids == nil or #producer_ids == 0 then
      chain.missing_inputs[#chain.missing_inputs + 1] = {
        item_id = request_item_id,
        required_rate_per_second = required_rate,
        reason = "no_producer"
      }
      diagnostics[#diagnostics + 1] = diag("warning", "automation.recipe_graph.no_producer", "No producer recipe found for requested item.", request_item_id)
      return
    end

    local recipe_id = producer_ids[1]
    local recipe = by_id[recipe_id]
    local produced_rate = output_rate_for(recipe, request_item_id)
    local machine_count = 1
    if produced_rate > 0 then
      machine_count = required_rate / produced_rate
    end

    chain.recipe_steps[#chain.recipe_steps + 1] = {
      recipe_id = recipe.id,
      produces_item_id = request_item_id,
      required_rate_per_second = required_rate,
      produced_rate_per_machine = produced_rate,
      estimated_machine_count = machine_count,
      category = recipe.category
    }

    stack[request_item_id] = true
    for input_index = 1, #recipe.inputs do
      local input = recipe.inputs[input_index]
      local input_rate = input_rate_for(recipe, input.item_id) * machine_count
      visit(input.item_id, input_rate, depth + 1, stack)
    end
    stack[request_item_id] = nil
  end

  visit(item_id, amount_per_second, 1, {})
  table.sort(chain.recipe_steps, function(left, right)
    if left.recipe_id == right.recipe_id then
      return left.produces_item_id < right.produces_item_id
    end
    return left.recipe_id < right.recipe_id
  end)

  local resource_rows = {}
  for _, key in ipairs(sorted_keys(chain.resource_inputs)) do
    resource_rows[#resource_rows + 1] = {
      item_id = key,
      required_rate_per_second = chain.resource_inputs[key]
    }
  end
  chain.resource_inputs = resource_rows
  return chain
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.recipe_graph.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.max_chain_depth ~= nil and (type(config.max_chain_depth) ~= "number" or config.max_chain_depth < 1) then
    diagnostics[#diagnostics + 1] = diag("error", "automation.recipe_graph.max_chain_depth_invalid", "max_chain_depth must be a positive number.", "config.max_chain_depth")
  end
  if config.default_rate_per_second ~= nil and (type(config.default_rate_per_second) ~= "number" or config.default_rate_per_second <= 0) then
    diagnostics[#diagnostics + 1] = diag("error", "automation.recipe_graph.default_rate_invalid", "default_rate_per_second must be positive.", "config.default_rate_per_second")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local config = {}
  if type(ctx) == "table" and type(ctx.config) == "table" then
    config = ctx.config
  end
  local config_ok, config_diagnostics = M.validate_config(config)
  for index = 1, #config_diagnostics do
    diagnostics[#diagnostics + 1] = config_diagnostics[index]
  end

  if type(input) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.recipe_graph.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  if not is_array(input.recipes or {}) then
    diagnostics[#diagnostics + 1] = diag("error", "automation.recipe_graph.recipes_not_array", "recipes must be an array.", "input.recipes")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local default_rate = tonumber(config.default_rate_per_second) or 1
  local recipes = {}
  for index = 1, #(input.recipes or {}) do
    local recipe = normalize_recipe(input.recipes[index], index, diagnostics, default_rate)
    if recipe ~= nil then
      recipes[#recipes + 1] = recipe
    end
  end

  table.sort(recipes, function(left, right)
    return left.id < right.id
  end)

  local by_id, producers_by_item, consumers_by_item = build_recipe_index(recipes, diagnostics)
  local resource_set = {}
  for index = 1, #(input.resources or {}) do
    local value = input.resources[index]
    if type(value) == "string" then
      resource_set[value] = true
    elseif type(value) == "table" then
      local id = normalize_id(value.item_id or value.id, "")
      if id ~= "" then
        resource_set[id] = true
      end
    end
  end

  local targets = input.targets or {}
  local chains = {}
  for index = 1, #targets do
    local target = targets[index]
    if type(target) == "table" then
      local item_id = normalize_id(target.item_id or target.id or target.item, "")
      local rate = tonumber(target.rate_per_second or target.amount_per_second or target.amount) or 1
      if item_id ~= "" then
        chains[#chains + 1] = build_chain_for_item(item_id, rate, producers_by_item, by_id, resource_set, tonumber(config.max_chain_depth) or 8, diagnostics)
      else
        diagnostics[#diagnostics + 1] = diag("error", "automation.recipe_graph.target_id_missing", "Target item requires item_id.", "input.targets[" .. index .. "]")
      end
    end
  end

  table.sort(chains, function(left, right)
    return left.target_item_id < right.target_item_id
  end)

  local graph_items = {}
  local seen_items = {}
  for index = 1, #recipes do
    local recipe = recipes[index]
    for item_index = 1, #recipe.inputs do
      seen_items[recipe.inputs[item_index].item_id] = true
    end
    for item_index = 1, #recipe.outputs do
      seen_items[recipe.outputs[item_index].item_id] = true
    end
  end
  for _, item_id in ipairs(sorted_keys(seen_items)) do
    graph_items[#graph_items + 1] = {
      item_id = item_id,
      producer_recipe_ids = copy_list(producers_by_item[item_id]),
      consumer_recipe_ids = copy_list(consumers_by_item[item_id]),
      is_resource = resource_set[item_id] == true
    }
  end

  local data = {
    recipe_graph = {
      recipes = recipes,
      items = graph_items
    },
    production_chains = chains,
    validation = {
      recipe_count = #recipes,
      item_count = #graph_items,
      config_ok = config_ok
    }
  }

  return { ok = #diagnostics == 0, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
