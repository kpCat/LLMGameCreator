local T = {}

T.manifest = {
  id = "tests/generation_manifest_examples/v1",
  version = "0.1.0",
  category = "generation",
  title = "Generation manifest manual examples",
  purpose = "Run compact manual examples for capability_manifest, module_manifest, and generator_plan modules when a host injects module tables.",
  capabilities = { "generation.tests.manual_examples" },
  input_schema = {},
  output_schema = {},
  config_schema = {},
  deterministic = true,
  runtime_targets = { "debug" },
  unsafe_features = {}
}

local function make_diagnostic(code, message, target)
  local diagnostic = {
    severity = "error",
    code = code,
    message = message
  }
  if target ~= nil then
    diagnostic.target = target
  end
  return diagnostic
end

local function add_check(report, name, ok, details)
  report.data.checks[#report.data.checks + 1] = {
    name = name,
    ok = ok == true,
    details = details or {}
  }
  if ok ~= true then
    report.ok = false
  end
end

local function sample_capability()
  return {
    id = "world.blueprint.generate",
    title = "World blueprint generation",
    purpose = "Create compact world blueprint IR from design constraints.",
    category = "world",
    inputs = {
      { id = "design_brief", schema = {}, required = true }
    },
    outputs = {
      { id = "world_blueprint", schema = {}, required = true }
    },
    config_schema = { type = "object" },
    supported_runtime_targets = { "debug", "unity2d" },
    supported_time_modes = { "realtime", "mixed" },
    supported_combat_modes = { "none", "dialogue_combat" },
    dependencies = {},
    incompatibilities = {},
    tags = { "world", "blueprint" }
  }
end

local function sample_module_manifest()
  return {
    id = "world/world_blueprint/v1",
    version = "0.1.0",
    category = "world",
    title = "World blueprint",
    purpose = "Generate world blueprint IR.",
    capabilities = { "world.blueprint.generate" },
    input_schema = {},
    output_schema = {},
    config_schema = {},
    deterministic = true,
    runtime_targets = { "debug", "unity2d" },
    supported_time_modes = { "realtime", "mixed" },
    supported_combat_modes = { "none", "dialogue_combat" },
    dependencies = { modules = {}, capabilities = {} },
    incompatibilities = { modules = {}, capabilities = {} },
    unsafe_features = {}
  }
end

local function sample_plan()
  return {
    id = "generation/plan/demo_world",
    title = "Demo world generation plan",
    runtime_target = "unity2d",
    turn_mode = "mixed",
    combat_mode = "dialogue_combat",
    inputs = { design_brief = { theme = "dark_fantasy_village" } },
    expected_outputs = { "world_blueprint", "chunk_ir" },
    steps = {
      {
        id = "step/world_blueprint",
        module_id = "world/world_blueprint/v1",
        capability_id = "world.blueprint.generate",
        inputs = { design_brief = "input/design_brief" },
        outputs = { "world_blueprint" },
        config = { world_scale = "region" },
        depends_on = {},
        incompatible_with = {},
        supported_runtime_targets = { "debug", "unity2d" },
        supported_time_modes = { "mixed" },
        supported_combat_modes = { "dialogue_combat" }
      },
      {
        id = "step/chunk_seed",
        module_id = "world/chunk_generator/v1",
        capability_id = "world.chunk.generate",
        inputs = { world_blueprint = "artifact/world_blueprint" },
        outputs = { "chunk_ir" },
        config = { chunk_size = 32 },
        depends_on = { "step/world_blueprint" },
        incompatible_with = {},
        supported_runtime_targets = { "debug", "unity2d" },
        supported_time_modes = { "mixed" },
        supported_combat_modes = { "dialogue_combat" }
      }
    }
  }
end

function T.run(generation)
  local report = {
    ok = true,
    data = { checks = {} },
    diagnostics = {},
    artifacts = {}
  }

  if type(generation) ~= "table" then
    report.ok = false
    report.diagnostics[#report.diagnostics + 1] = make_diagnostic("generation.tests.missing_generation", "Test runner expects injected generation module table.", "generation")
    return report
  end

  local CapabilityManifest = generation.capability_manifest
  local ModuleManifest = generation.module_manifest
  local GeneratorPlan = generation.generator_plan

  if type(CapabilityManifest) ~= "table" or type(ModuleManifest) ~= "table" or type(GeneratorPlan) ~= "table" then
    report.ok = false
    report.diagnostics[#report.diagnostics + 1] = make_diagnostic("generation.tests.missing_module", "Injected generation table must contain capability_manifest, module_manifest, and generator_plan modules.", "generation")
    return report
  end

  local capability = sample_capability()
  local capability_result = CapabilityManifest.normalize(capability)
  add_check(report, "capability_manifest_normalize", capability_result.ok == true and capability_result.data.capability.id == "world.blueprint.generate", capability_result.data)

  local bad_capability = CapabilityManifest.normalize({ id = "World.Bad" })
  add_check(report, "capability_manifest_invalid_id", bad_capability.ok == false and bad_capability.diagnostics[1] ~= nil, { diagnostics = bad_capability.diagnostics })

  local capability_index = CapabilityManifest.generate({ capabilities = { capability } })
  add_check(report, "capability_manifest_index", capability_index.ok == true and capability_index.data.by_id["world.blueprint.generate"] ~= nil, capability_index.data)

  local module_manifest = sample_module_manifest()
  local module_result = ModuleManifest.normalize(module_manifest)
  add_check(report, "module_manifest_normalize", module_result.ok == true and module_result.data.module.id == "world/world_blueprint/v1", module_result.data)

  local module_index = ModuleManifest.generate({ modules = { module_manifest } })
  add_check(report, "module_manifest_capability_lookup", module_index.ok == true and module_index.data.capability_to_modules["world.blueprint.generate"][1] == "world/world_blueprint/v1", module_index.data)

  local plan_result = GeneratorPlan.generate({ plan = sample_plan() })
  add_check(report, "generator_plan_execution_order", plan_result.ok == true and plan_result.data.execution_order[1] == "step/world_blueprint" and plan_result.data.execution_order[2] == "step/chunk_seed", plan_result.data)

  local invalid_plan = sample_plan()
  invalid_plan.steps[1].depends_on = { "step/chunk_seed" }
  local invalid_result = GeneratorPlan.generate({ plan = invalid_plan })
  add_check(report, "generator_plan_dependency_after_step", invalid_result.ok == false and invalid_result.diagnostics[1] ~= nil, { diagnostics = invalid_result.diagnostics })

  return report
end

function T.validate_config(config)
  if config ~= nil and type(config) ~= "table" then
    return false, { make_diagnostic("generation.tests.config_not_table", "Test config must be a table.", "config") }
  end
  return true, {}
end

return T
