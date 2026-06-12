local T = {}

T.manifest = {
  id = "tests/time_turn_examples/v1",
  version = "0.1.0",
  category = "core",
  title = "Time, turn, and mode manual examples",
  purpose = "Run compact manual examples for time_model, turn_system, and mode_transition modules when the host injects module tables.",
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
    report.diagnostics[#report.diagnostics + 1] = make_diagnostic("core.time_turn_tests.missing_core", "Test runner expects injected core module table.", "core")
    return report
  end

  local TimeModel = core.time_model
  local TurnSystem = core.turn_system
  local ModeTransition = core.mode_transition

  if type(TimeModel) ~= "table" or type(TurnSystem) ~= "table" or type(ModeTransition) ~= "table" then
    report.ok = false
    report.diagnostics[#report.diagnostics + 1] = make_diagnostic("core.time_turn_tests.missing_module", "Injected core table must contain time_model, turn_system, and mode_transition modules.", "core")
    return report
  end

  local time_result = TimeModel.create({
    turn_mode = "mixed",
    combat_mode = "dialogue_combat",
    active_mode = "dialogue",
    simulation_tick_seconds = 1,
    cooldown_tick_unit = "turn",
    status_tick_unit = "round"
  })
  add_check(report, "time_model_create_mixed_dialogue_combat", time_result.ok == true and time_result.data.model.turn_mode == "mixed", time_result.data)

  local advanced = TimeModel.advance(time_result.data.model, { simulation_ticks = 3, turns = 1 })
  add_check(report, "time_model_advance_ticks", advanced.ok == true and advanced.data.model.tick == 3 and advanced.data.model.global_turn == 2, advanced.data)

  local bad_time = TimeModel.create({ turn_mode = "fast_forward" })
  add_check(report, "time_model_invalid_mode_diagnostic", bad_time.ok == false and bad_time.diagnostics[1] ~= nil, { diagnostics = bad_time.diagnostics })

  local turn_result = TurnSystem.create({
    config = {
      turn_mode = "mixed",
      initiative_mode = "actor",
      default_action_points = 2,
      tick_cooldowns_on = "actor_turn_end",
      tick_statuses_on = "round_end"
    },
    actors = {
      { id = "entity/player/main", side = "heroes", initiative = 10 },
      { id = "entity/enemy/rat", side = "monsters", initiative = 4 }
    }
  })
  add_check(report, "turn_system_create_order", turn_result.ok == true and turn_result.data.current_actor_id == "entity/player/main", turn_result.data)

  local spent = TurnSystem.spend_action_points(turn_result.data.state, "entity/player/main", 1)
  add_check(report, "turn_system_spend_action_points", spent.ok == true and spent.data.actor.action_points == 1, spent.data)

  local cooldown = TurnSystem.apply_cooldown(spent.data.state, "entity/player/main", "ability/power_strike", 2)
  add_check(report, "turn_system_apply_cooldown", cooldown.ok == true and cooldown.data.actor.cooldowns["ability/power_strike"] == 2, cooldown.data)

  local status = TurnSystem.add_status(cooldown.data.state, "entity/enemy/rat", "status/frightened", 1)
  add_check(report, "turn_system_add_status", status.ok == true and status.data.actor.statuses["status/frightened"] == 1, status.data)

  local ended = TurnSystem.end_turn(status.data.state)
  add_check(report, "turn_system_end_turn", ended.ok == true and ended.data.current_actor_id == "entity/enemy/rat" and ended.data.state.global_turn == 2, ended.data)

  local can_dialogue = ModeTransition.can_transition("exploration", "dialogue")
  add_check(report, "mode_transition_can_exploration_to_dialogue", can_dialogue.ok == true and can_dialogue.data.allowed == true, can_dialogue.data)

  local applied = ModeTransition.apply({ active_mode = "dialogue", combat_mode = "none", turn_mode = "mixed" }, "dialogue_combat")
  add_check(report, "mode_transition_apply_dialogue_combat", applied.ok == true and applied.data.state.active_mode == "dialogue_combat" and applied.data.state.combat_mode == "dialogue_combat", applied.data)

  local blocked = ModeTransition.can_transition("exploration", "dialogue_combat")
  add_check(report, "mode_transition_blocks_missing_rule", blocked.ok == false and blocked.diagnostics[1] ~= nil, { diagnostics = blocked.diagnostics })

  local custom = ModeTransition.can_transition(
    "dialogue",
    "dialogue_combat",
    { rules = { { from = "dialogue", to = "dialogue_combat", reason = "threat", requires = { "has_target" } } } },
    { flags = { has_target = true } }
  )
  add_check(report, "mode_transition_custom_requirement", custom.ok == true and custom.data.allowed == true, custom.data)

  return report
end

function T.validate_config(config)
  if config ~= nil and type(config) ~= "table" then
    return false, { make_diagnostic("core.time_turn_tests.config_not_table", "Test config must be a table.", "config") }
  end
  return true, {}
end

return T
