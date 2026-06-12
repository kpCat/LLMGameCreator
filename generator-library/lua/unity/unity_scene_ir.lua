local M = {}

M.manifest = {
  id = "unity/unity_scene_ir/v1",
  version = "0.1.0",
  category = "unity",
  title = "Unity Scene IR",
  purpose = "Generate Unity-facing scene metadata as plain data, without creating scene files or Unity objects.",
  capabilities = { "unity.scene_ir.generate", "unity.prefab_slots.plan", "unity.spawn_points.plan" },
  input_schema = { type = "table", required = { "scenes" } },
  output_schema = { type = "table", required = { "scenes" } },
  config_schema = { type = "table" },
  deterministic = true,
  runtime_targets = { "editor", "unity2d", "unity3d", "unity_ir", "codegen_ir" },
  unsafe_features = {}
}

local function diagnostic(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function is_array(value)
  if type(value) ~= "table" then return false end
  local count = 0
  for key, _ in pairs(value) do
    if type(key) ~= "number" or key < 1 or key % 1 ~= 0 then return false end
    if key > count then count = key end
  end
  for i = 1, count do
    if value[i] == nil then return false end
  end
  return true
end

local function is_slash_id(value)
  return type(value) == "string" and value:match("^[a-z0-9][a-z0-9_%-]*(/[a-z0-9][a-z0-9_%-]*)*$") ~= nil
end

local function contains(list, value)
  for _, item in ipairs(list) do if item == value then return true end end
  return false
end

local valid_categories = { "boot", "menu", "world", "gameplay", "combat", "dialogue", "city", "automation", "test" }

local function validate_slots(slots, target, diagnostics)
  if slots == nil then return end
  if not is_array(slots) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.scene_ir.invalid_slots", "Slots must be an array when provided.", target)
    return
  end
  local seen = {}
  for index, slot in ipairs(slots) do
    local slot_target = target .. "[" .. index .. "]"
    if type(slot) ~= "table" then
      diagnostics[#diagnostics + 1] = diagnostic("error", "unity.scene_ir.slot_not_table", "Slot must be a table.", slot_target)
    else
      if not is_slash_id(slot.id) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.scene_ir.invalid_slot_id", "Slot id must be a lowercase slash id.", slot_target .. ".id")
      elseif seen[slot.id] then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.scene_ir.duplicate_slot", "Duplicate slot id.", slot.id)
      end
      seen[slot.id] = true
      if slot.prefab_ref ~= nil and not is_slash_id(slot.prefab_ref) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.scene_ir.invalid_prefab_ref", "prefab_ref must be a lowercase slash id when provided.", slot_target .. ".prefab_ref")
      end
      if slot.entity_ref ~= nil and not is_slash_id(slot.entity_ref) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.scene_ir.invalid_entity_ref", "entity_ref must be a lowercase slash id when provided.", slot_target .. ".entity_ref")
      end
    end
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.scene_ir.config_not_table", "Scene IR config must be a table.", "config")
    return false, diagnostics
  end

  if not is_array(config.scenes) or #config.scenes == 0 then
    diagnostics[#diagnostics + 1] = diagnostic("error", "unity.scene_ir.missing_scenes", "scenes must be a non-empty array.", "scenes")
    return false, diagnostics
  end

  local seen_scenes = {}
  for index, scene in ipairs(config.scenes) do
    local target = "scenes[" .. index .. "]"
    if type(scene) ~= "table" then
      diagnostics[#diagnostics + 1] = diagnostic("error", "unity.scene_ir.scene_not_table", "Scene must be a table.", target)
    else
      if not is_slash_id(scene.id) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.scene_ir.invalid_scene_id", "Scene id must be a lowercase slash id.", target .. ".id")
      elseif seen_scenes[scene.id] then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.scene_ir.duplicate_scene", "Duplicate scene id.", scene.id)
      end
      seen_scenes[scene.id] = true
      if scene.category ~= nil and not contains(valid_categories, scene.category) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.scene_ir.invalid_category", "Scene category is not supported.", target .. ".category")
      end
      if scene.world_ref ~= nil and not is_slash_id(scene.world_ref) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.scene_ir.invalid_world_ref", "world_ref must be a lowercase slash id.", target .. ".world_ref")
      end
      if scene.map_ref ~= nil and not is_slash_id(scene.map_ref) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "unity.scene_ir.invalid_map_ref", "map_ref must be a lowercase slash id.", target .. ".map_ref")
      end
      validate_slots(scene.prefab_slots, target .. ".prefab_slots", diagnostics)
      validate_slots(scene.entity_slots, target .. ".entity_slots", diagnostics)
      validate_slots(scene.spawn_points, target .. ".spawn_points", diagnostics)
    end
  end

  return #diagnostics == 0, diagnostics
end

local function copy_array(values)
  local result = {}
  if is_array(values) then for index, value in ipairs(values) do result[index] = value end end
  return result
end

function M.generate(input, ctx)
  local config = input and (input.config or input) or nil
  local ok, diagnostics = M.validate_config(config)
  if not ok then return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} } end

  local scenes = {}
  for index, scene in ipairs(config.scenes) do
    scenes[index] = {
      id = scene.id,
      category = scene.category or "gameplay",
      world_ref = scene.world_ref,
      map_ref = scene.map_ref,
      entity_slots = copy_array(scene.entity_slots),
      prefab_slots = copy_array(scene.prefab_slots),
      spawn_points = copy_array(scene.spawn_points),
      camera = scene.camera or { mode = "orthographic_2d" },
      environment = scene.environment or {},
      metadata = scene.metadata or {}
    }
  end

  return { ok = true, data = { scenes = scenes }, diagnostics = diagnostics, artifacts = {} }
end

return M
