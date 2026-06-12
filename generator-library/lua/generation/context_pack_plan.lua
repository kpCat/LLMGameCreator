local M = {}

M.manifest = {
  id = "generation/context_pack_plan/v1",
  version = "0.1.0",
  category = "generation",
  title = "Context pack plan",
  purpose = "Create deterministic LLM context-pack planning metadata without reading files or calling a model.",
  capabilities = {
    "generation.context_pack_plan.generate",
    "generation.context_pack_plan.validate",
    "generation.context_budget.plan"
  },
  input_schema = {
    context_pack_id = "string",
    token_budget = "table",
    included_module_ids = "array"
  },
  output_schema = {
    context_pack_plan = "plain metadata IR",
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
  local max_index = 0
  for key, _ in pairs(value) do
    if type(key) ~= "number" or key < 1 or key % 1 ~= 0 then
      return false
    end
    if key > max_index then
      max_index = key
    end
  end
  for index = 1, max_index do
    if value[index] == nil then
      return false
    end
  end
  return true
end

local function valid_id(value)
  return type(value) == "string" and value ~= "" and value:match("^[a-z0-9_/%-%.]+$") ~= nil
end

local function copy_array(value)
  local result = {}
  if is_array(value) then
    for index = 1, #value do
      result[#result + 1] = value[index]
    end
  end
  return result
end

local function build_set(list)
  local set = {}
  if is_array(list) then
    for index = 1, #list do
      set[list[index]] = true
    end
  end
  return set
end

local function validate_refs(list, available, list_name, diagnostics)
  if list ~= nil and not is_array(list) then
    diagnostics[#diagnostics + 1] = diag("error", "invalid_ref_list", list_name .. " must be an array when present.", list_name)
    return
  end

  local seen = {}
  if is_array(list) then
    local available_set = build_set(available)
    local has_available = is_array(available)
    for index = 1, #list do
      local id = list[index]
      if not valid_id(id) then
        diagnostics[#diagnostics + 1] = diag("error", "invalid_reference_id", "Reference id must be a lowercase slash or dot id.", list_name .. "[" .. index .. "]")
      elseif seen[id] then
        diagnostics[#diagnostics + 1] = diag("warning", "duplicate_reference", "Duplicate reference in context pack plan.", id)
      else
        seen[id] = true
      end
      if has_available and valid_id(id) and not available_set[id] then
        diagnostics[#diagnostics + 1] = diag("warning", "missing_reference", "Reference is not present in the supplied available list.", id)
      end
    end
  end
end

local function validate_budget(config)
  local diagnostics = {}
  local budget = config.token_budget
  if budget == nil then
    return diagnostics
  end
  if type(budget) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "invalid_token_budget", "token_budget must be a table.", "token_budget")
    return diagnostics
  end
  local max_input = budget.max_input_tokens
  local reserved = budget.reserved_tokens or 0
  local max_output = budget.max_output_tokens or 0
  if type(max_input) ~= "number" or max_input <= 0 then
    diagnostics[#diagnostics + 1] = diag("error", "invalid_max_input_tokens", "max_input_tokens must be a positive number.", "token_budget.max_input_tokens")
  end
  if type(reserved) ~= "number" or reserved < 0 then
    diagnostics[#diagnostics + 1] = diag("error", "invalid_reserved_tokens", "reserved_tokens must be zero or positive.", "token_budget.reserved_tokens")
  end
  if type(max_output) ~= "number" or max_output < 0 then
    diagnostics[#diagnostics + 1] = diag("error", "invalid_max_output_tokens", "max_output_tokens must be zero or positive.", "token_budget.max_output_tokens")
  end
  if type(max_input) == "number" and type(reserved) == "number" and reserved >= max_input then
    diagnostics[#diagnostics + 1] = diag("error", "invalid_budget_ratio", "reserved_tokens must be smaller than max_input_tokens.", "token_budget")
  end
  return diagnostics
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
  if config.context_pack_id ~= nil and not valid_id(config.context_pack_id) then
    diagnostics[#diagnostics + 1] = diag("error", "invalid_context_pack_id", "context_pack_id must be a lowercase slash or dot id.", "context_pack_id")
  end
  return { ok = #diagnostics == 0, diagnostics = diagnostics }
end

function M.generate(input, ctx)
  local config = {}
  if type(input) == "table" then
    config = input
  end

  local diagnostics = {}
  local base = M.validate_config(config)
  for index = 1, #base.diagnostics do
    diagnostics[#diagnostics + 1] = base.diagnostics[index]
  end

  local budget_diags = validate_budget(config)
  for index = 1, #budget_diags do
    diagnostics[#diagnostics + 1] = budget_diags[index]
  end

  validate_refs(config.included_knowledge_ids, config.available_knowledge_ids, "included_knowledge_ids", diagnostics)
  validate_refs(config.included_module_ids, config.available_module_ids, "included_module_ids", diagnostics)
  validate_refs(config.included_artifact_ids, config.available_artifact_ids, "included_artifact_ids", diagnostics)
  validate_refs(config.exclusions, nil, "exclusions", diagnostics)

  local budget = type(config.token_budget) == "table" and config.token_budget or {}
  return {
    ok = #diagnostics == 0,
    data = {
      context_pack_id = config.context_pack_id or "generation/context_pack_plan/default",
      purpose = config.purpose or "generator_context",
      token_budget = {
        max_input_tokens = budget.max_input_tokens or 0,
        max_output_tokens = budget.max_output_tokens or 0,
        reserved_tokens = budget.reserved_tokens or 0,
        target_tokens = budget.target_tokens or nil
      },
      included_knowledge_ids = copy_array(config.included_knowledge_ids),
      included_module_ids = copy_array(config.included_module_ids),
      included_artifact_ids = copy_array(config.included_artifact_ids),
      exclusions = copy_array(config.exclusions),
      hints = type(config.hints) == "table" and config.hints or {
        compression = "compact",
        summarization = "metadata_only",
        priority = "module_and_artifact_contracts"
      },
      does_not_call_model = true,
      does_not_read_files = true,
      ctx_tag = type(ctx) == "table" and ctx.tag or nil
    },
    diagnostics = diagnostics,
    artifacts = {
      {
        id = (config.context_pack_id or "generation/context_pack_plan/default") .. "/plan",
        kind = "context_pack_plan_ir",
        produced_by = M.manifest.id
      }
    }
  }
end

return M
