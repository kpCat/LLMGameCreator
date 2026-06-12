local function assert_true(value, message)
  if not value then
    return { ok = false, message = message }
  end
  return { ok = true, message = message }
end

local function run_dialogue_generation_examples(modules)
  local results = {}
  local dialogue_schema = modules.dialogue_schema
  local procedural_npc_dialogue = modules.procedural_npc_dialogue
  local fact_based_dialogue = modules.fact_based_dialogue
  local dialogue_combat = modules.dialogue_combat

  local schema_result = dialogue_schema.generate({
    dialogue_id = "dialogue/village/elder_intro",
    speaker_id = "entity/npc/elder",
    nodes = {
      {
        id = "start",
        text = "The road is unsafe.",
        choices = {
          { id = "ask", text = "What happened?", to_node_id = "rumor" },
          { id = "leave", text = "Goodbye.", ends_dialogue = true }
        }
      },
      {
        id = "rumor",
        text = "A collapsed wagon blocks the bridge.",
        choices = {
          {
            id = "mark_known",
            text = "I will investigate.",
            ends_dialogue = true,
            effects = { { target = "fact", key = "fact/blocked_bridge", op = "set", value = true } }
          }
        }
      }
    }
  }, { config = { max_nodes = 8, max_choices_per_node = 4 } })
  results[#results + 1] = assert_true(schema_result.ok, "static dialogue graph is valid")
  results[#results + 1] = assert_true(schema_result.data.summary.node_count == 2, "static graph has two nodes")

  local npc_result = procedural_npc_dialogue.generate({
    npc = { id = "entity/npc/elder", name = "Elder", role = "quest_giver", tone = "worried" },
    facts = {
      { id = "fact/blocked_bridge", title = "Blocked bridge", summary = "The old bridge cannot be crossed safely." },
      { id = "fact/night_raids", title = "Night raids", summary = "Something attacks caravans after sunset." }
    }
  }, { config = { max_facts = 4, max_topics = 4 } })
  results[#results + 1] = assert_true(npc_result.ok, "procedural NPC dialogue is valid")
  results[#results + 1] = assert_true(npc_result.data.summary.topic_count == 2, "procedural NPC dialogue uses two topics")

  local fact_result = fact_based_dialogue.generate({
    dialogue_id = "dialogue/village/elder_facts",
    speaker_id = "entity/npc/elder",
    known_facts = { "fact/blocked_bridge" },
    rules = {
      {
        id = "ask_bridge",
        required_facts = { "fact/blocked_bridge" },
        choice_text = "Ask about the blocked bridge.",
        text = "The bridge was sabotaged, not ruined by weather.",
        set_facts = { "fact/bridge_sabotage" },
        quest_effects = { ["quest/investigate_road"] = "active" }
      }
    }
  }, { config = { emit_missing_fact_warnings = true } })
  results[#results + 1] = assert_true(fact_result.ok, "fact-based dialogue is valid")
  results[#results + 1] = assert_true(fact_result.data.summary.rule_count == 1, "fact-based dialogue has one rule")

  local combat_result = dialogue_combat.generate({
    encounter_id = "dialogue_combat/elder_interrogation",
    title = "Convince the elder",
    prompt = "Choose how to press for details.",
    tracks = { morale = 55, trust = 10, suspicion = 25, focus = 40 },
    choices = {
      {
        id = "calm_reason",
        text = "Reason calmly.",
        stance = "diplomatic",
        effects = {
          { target = "trust", op = "add", amount = 10 },
          { target = "suspicion", op = "add", amount = -5 }
        }
      },
      {
        id = "hard_pressure",
        text = "Pressure him for the truth.",
        stance = "intimidate",
        conditions = { { kind = "fact", key = "fact/bridge_sabotage", op = "equals", value = true } },
        effects = {
          { target = "morale", op = "add", amount = -15 },
          { target = "focus", op = "add", amount = -5 }
        }
      }
    }
  }, { config = { track_min = 0, track_max = 100 } })
  results[#results + 1] = assert_true(combat_result.ok, "dialogue-combat encounter is valid")
  results[#results + 1] = assert_true(combat_result.data.summary.choice_count == 2, "dialogue-combat has two choices")

  local failed = {}
  for i = 1, #results do
    if not results[i].ok then
      failed[#failed + 1] = results[i].message
    end
  end
  return { ok = #failed == 0, results = results, failed = failed }
end

return { run_dialogue_generation_examples = run_dialogue_generation_examples }
