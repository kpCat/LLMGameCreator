local M = {}

M.manifest = {
  id = "generation/artifact_manifest/v1",
  version = "0.1.0",
  category = "generation",
  title = "Artifact manifest",
  purpose = "Create deterministic generated-artifact manifest IR from plain artifact metadata.",
  capabilities = {
    "generation.artifact_manifest.generate",
    "generation.artifact_manifest.validate",
    "generation.validation_result.reference"
  },
  input_schema = {
    artifacts = "array of artifact metadata"
  },
  output_schema = {
    manifest = "artifact manifest IR",
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

local function copy_array(value)
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

local function list_has(list, value)
  if not is_array(list) then
    return false
  end
  for index = 1, #list do
    if list[index] == value then
      return true
    end
  end
  return false
end

local allowed_kinds = {
  manifest = true,
  lua_module = true,
  doc = true,
  test_example = true,
  validation_result = true,
  ir_data = true,
  context_pack = true,
  pipeline_plan = true,
  adapter_plan = true
}

local allowed_states = {
  not_validated = true,
  valid = true,
  warning = true,
  invalid = true,
  blocked = true
}

local function validate_artifacts(config)
  local diagnostics = {}
  local artifacts = is_array(config.artifacts) and config.artifacts or {}
  local ids = {}

  for index = 1, #artifacts do
    local artifact = artifacts[index]
    local target = "artifacts[" .. index .. "]"
    if type(artifact) ~= "table" then
      diagnostics[#diagnostics + 1] = diag("error", "invalid_artifact", "Artifact must be a table.", target)
    else
      if not valid_id(artifact.id) then
        diagnostics[#diagnostics + 1] = diag("error", "invalid_artifact_id", "Artifact id must be a lowercase slash or dot id.", target .. ".id")
      elseif ids[artifact.id] then
        diagnostics[#diagnostics + 1] = diag("error", "duplicate_artifact_id", "Duplicate artifact id.", artifact.id)
      else
        ids[artifact.id] = true
      end

      if not allowed_kinds[artifact.kind] then
        diagnostics[#diagnostics + 1] = diag("error", "invalid_artifact_kind", "Artifact kind is not in the allowed set.", target .. ".kind")
      end

      if artifact.logical_path ~= nil and type(artifact.logical_path) ~= "string" then
        diagnostics[#diagnostics + 1] = diag("error", "invalid_logical_path", "logical_path must be a string when present.", target .. ".logical_path")
      end

      if not valid_id(artifact.produced_by) then
        diagnostics[#diagnostics + 1] = diag("error", "missing_producer", "Artifact must declare a producer module or step id.", target .. ".produced_by")
      elseif is_array(config.producer_ids) and not list_has(config.producer_ids, artifact.produced_by) then
        diagnostics[#diagnostics + 1] = diag("warning", "unknown_producer", "Producer id is not present in producer_ids.", target .. ".produced_by")
      end

      if not allowed_states[artifact.validation_state or "not_validated"] then
        diagnostics[#diagnostics + 1] = diag("error", "invalid_validation_state", "validation_state is not allowed.", target .. ".validation_state")
      end

      if artifact.depends_on_artifacts ~= nil and not is_array(artifact.depends_on_artifacts) then
        diagnostics[#diagnostics + 1] = diag("error", "invalid_artifact_dependencies", "depends_on_artifacts must be an array.", target .. ".depends_on_artifacts")
      end

      if artifact.validation_result_refs ~= nil and not is_array(artifact.validation_result_refs) then
        diagnostics[#diagnostics + 1] = diag("error", "invalid_validation_refs", "validation_result_refs must be an array.", target .. ".validation_result_refs")
      end
    end
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
  if config.artifacts ~= nil and not is_array(config.artifacts) then
    diagnostics[#diagnostics + 1] = diag("error", "invalid_artifacts", "artifacts must be an array when present.", "artifacts")
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
  local artifact_diags = validate_artifacts(config)
  for index = 1, #artifact_diags do
    diagnostics[#diagnostics + 1] = artifact_diags[index]
  end

  local artifacts = {}
  if is_array(config.artifacts) then
    for index = 1, #config.artifacts do
      local artifact = config.artifacts[index]
      if type(artifact) == "table" then
        artifacts[#artifacts + 1] = {
          id = artifact.id,
          kind = artifact.kind,
          logical_path = artifact.logical_path,
          produced_by = artifact.produced_by,
          validation_state = artifact.validation_state or "not_validated",
          validation_result_refs = copy_array(artifact.validation_result_refs),
          depends_on_artifacts = copy_array(artifact.depends_on_artifacts),
          metadata = type(artifact.metadata) == "table" and artifact.metadata or {}
        }
      end
    end
  end

  return {
    ok = #diagnostics == 0,
    data = {
      manifest_id = config.manifest_id or "generation/artifact_manifest/default",
      artifact_count = #artifacts,
      artifacts = artifacts,
      validation_result_index = copy_array(config.validation_result_index),
      schema_version = "0.1.0",
      ctx_tag = type(ctx) == "table" and ctx.tag or nil
    },
    diagnostics = diagnostics,
    artifacts = {
      {
        id = (config.manifest_id or "generation/artifact_manifest/default") .. "/manifest",
        kind = "artifact_manifest_ir",
        produced_by = M.manifest.id
      }
    }
  }
end

return M
