local T = {}

T.manifest = {
  id = "tests/core_foundation_examples/v1",
  version = "0.1.0",
  category = "core",
  title = "Core foundation manual examples",
  purpose = "Run compact manual examples for diagnostics, rng, and schema modules when the host injects module tables.",
  capabilities = { "core.tests.manual_examples" },
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

function T.run(core)
  local report = {
    ok = true,
    data = { checks = {} },
    diagnostics = {},
    artifacts = {}
  }

  if type(core) ~= "table" then
    report.ok = false
    report.diagnostics[#report.diagnostics + 1] = make_diagnostic("core.tests.missing_core", "Test runner expects injected core module table.", "core")
    return report
  end

  local Diagnostics = core.diagnostics
  local Rng = core.rng
  local Schema = core.schema

  if type(Diagnostics) ~= "table" or type(Rng) ~= "table" or type(Schema) ~= "table" then
    report.ok = false
    report.diagnostics[#report.diagnostics + 1] = make_diagnostic("core.tests.missing_module", "Injected core table must contain diagnostics, rng, and schema modules.", "core")
    return report
  end

  local diagnostics = Diagnostics.list()
  Diagnostics.add_warning(diagnostics, "example.warning", "Example warning.", "example")
  local counts = Diagnostics.count_by_severity(diagnostics)
  add_check(report, "diagnostics_count_warning", counts.warning == 1 and counts.error == 0, counts)

  local state_a = Rng.new(12345)
  local state_b = Rng.new(12345)
  local value_a
  local value_b
  state_a, value_a = Rng.range_int(state_a, 1, 100)
  state_b, value_b = Rng.range_int(state_b, 1, 100)
  add_check(report, "rng_same_seed_same_value", value_a == value_b, { value_a = value_a, value_b = value_b })

  local seed_world = Rng.derive_seed(12345, "world")
  local seed_chunk = Rng.derive_seed(12345, "chunk")
  add_check(report, "rng_derived_seeds_differ", seed_world ~= seed_chunk, { world = seed_world, chunk = seed_chunk })

  local config_schema = {
    type = "object",
    allow_unknown = false,
    required = { "seed", "world_scale" },
    properties = {
      seed = { type = "integer", min = 1 },
      world_scale = { type = "string", enum = { "single_map", "region", "infinite_chunks" } }
    }
  }

  local valid_result = Schema.validate({ seed = 7, world_scale = "region" }, config_schema, { path = "config" })
  add_check(report, "schema_valid_config", valid_result.ok == true, { diagnostics = valid_result.diagnostics })

  local invalid_result = Schema.validate({ world_scale = "unknown" }, config_schema, { path = "config" })
  add_check(report, "schema_invalid_config_reports_errors", invalid_result.ok == false and invalid_result.diagnostics[1] ~= nil, { diagnostics = invalid_result.diagnostics })

  local json_result = Schema.validate_json_serializable({ id = "world/chunk/start", position = { x = 0, y = 0 } }, { path = "output" })
  add_check(report, "schema_json_serializable", json_result.ok == true, { diagnostics = json_result.diagnostics })

  return report
end

function T.validate_config(config)
  if config ~= nil and type(config) ~= "table" then
    return false, { make_diagnostic("core.tests.config_not_table", "Test config must be a table.", "config") }
  end
  return true, {}
end

return T
