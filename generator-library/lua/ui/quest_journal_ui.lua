local M = {}

M.manifest = {
  id = "ui/quest_journal_ui/v1",
  version = "0.1.0",
  category = "ui",
  title = "Quest journal and notes UI IR generator",
  purpose = "Generates deterministic quest journal, objective list, notes and codex UI configuration IR with quest/dialogue references.",
  capabilities = {
    "ui.quest_journal.generate",
    "ui.quest_objectives.configure",
    "ui.notes.generate",
    "ui.codex.generate"
  },
  input_schema = {
    type = "object",
    fields = {
      sections = "optional array",
      objective_layout = "optional string",
      tracked_quest_limit = "optional integer",
      notes = "optional table"
    }
  },
  output_schema = {
    type = "object",
    fields = {
      panels = "array",
      sections = "array",
      bindings = "table"
    }
  },
  config_schema = {
    type = "object",
    fields = {
      default_objective_layout = "optional string",
      max_tracked_quests = "optional integer"
    }
  },
  deterministic = true,
  runtime_targets = { "editor", "unity2d", "unity3d", "unity_ui_ir", "codegen_ir" },
  unsafe_features = {}
}

local VALID_LAYOUTS = {
  list = true,
  cards = true,
  timeline = true,
  compact = true
}

local function diagnostic(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function add_diag(diagnostics, severity, code, message, target)
  diagnostics[#diagnostics + 1] = diagnostic(severity, code, message, target)
end

local function is_table(value)
  return type(value) == "table"
end

local function is_non_empty_string(value)
  return type(value) == "string" and value ~= ""
end

local function is_ui_id(value)
  return is_non_empty_string(value) and value:match("^[a-z][a-z0-9_]*(/[a-z][a-z0-9_]*)*$") ~= nil
end

local function copy_array(source)
  local result = {}
  if is_table(source) then
    for index = 1, #source do
      result[index] = source[index]
    end
  end
  return result
end

local function default_sections()
  return {
    { id = "journal/active_quests", label = "Active", source = "quest.active" },
    { id = "journal/completed_quests", label = "Completed", source = "quest.completed" },
    { id = "journal/notes", label = "Notes", source = "notes.entries" },
    { id = "journal/codex", label = "Codex", source = "codex.entries" }
  }
end

local function validate_sections(sections, diagnostics)
  if sections == nil then
    return
  end
  if not is_table(sections) then
    add_diag(diagnostics, "error", "ui.quest_journal.invalid_sections", "Sections must be an array when provided.", "input.sections")
    return
  end
  local seen = {}
  for index = 1, #sections do
    local section = sections[index]
    local target = "input.sections[" .. tostring(index) .. "]"
    if not is_table(section) then
      add_diag(diagnostics, "error", "ui.quest_journal.invalid_section", "Section must be a table.", target)
    else
      if not is_ui_id(section.id) then
        add_diag(diagnostics, "error", "ui.quest_journal.invalid_section_id", "Section id must use lowercase slash notation.", target .. ".id")
      elseif seen[section.id] then
        add_diag(diagnostics, "error", "ui.quest_journal.duplicate_section", "Section id is duplicated.", target .. ".id")
      else
        seen[section.id] = true
      end
      if section.source ~= nil and not is_non_empty_string(section.source) then
        add_diag(diagnostics, "error", "ui.quest_journal.invalid_source", "Section source must be a non-empty string when provided.", target .. ".source")
      end
    end
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if not is_table(config) then
    add_diag(diagnostics, "error", "ui.quest_journal.config.invalid", "Quest journal config must be a table when provided.", "config")
    return false, diagnostics
  end
  if config.default_objective_layout ~= nil and not VALID_LAYOUTS[config.default_objective_layout] then
    add_diag(diagnostics, "error", "ui.quest_journal.config.invalid_layout", "Default objective layout is not supported.", "config.default_objective_layout")
  end
  if config.max_tracked_quests ~= nil and (type(config.max_tracked_quests) ~= "number" or config.max_tracked_quests < 0 or config.max_tracked_quests > 20) then
    add_diag(diagnostics, "error", "ui.quest_journal.config.invalid_max_tracked", "max_tracked_quests must be between 0 and 20.", "config.max_tracked_quests")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local source = input or {}
  local config = source.config or {}
  local diagnostics = {}
  local _, config_diags = M.validate_config(config)
  for index = 1, #config_diags do
    diagnostics[#diagnostics + 1] = config_diags[index]
  end

  validate_sections(source.sections, diagnostics)

  local objective_layout = source.objective_layout or config.default_objective_layout or "list"
  if not VALID_LAYOUTS[objective_layout] then
    add_diag(diagnostics, "error", "ui.quest_journal.invalid_objective_layout", "Objective layout is not supported.", "input.objective_layout")
    objective_layout = "list"
  end

  local max_tracked = config.max_tracked_quests or 5
  local tracked_limit = source.tracked_quest_limit or max_tracked
  if type(tracked_limit) ~= "number" or tracked_limit < 0 or tracked_limit > max_tracked then
    add_diag(diagnostics, "error", "ui.quest_journal.invalid_tracked_limit", "tracked_quest_limit must be within configured limits.", "input.tracked_quest_limit")
    tracked_limit = max_tracked
  end

  local sections = source.sections or default_sections()
  local notes = source.notes or { allow_player_notes = true, allow_codex_entries = true }

  return {
    ok = #diagnostics == 0,
    data = {
      ir_type = "ui.quest_journal",
      version = "0.1.0",
      panels = {
        { id = "journal/root", kind = "window", anchor = "center", size = { width = 86, height = 80 }, visibility = { mode = "when", binding = "ui.quest_journal.open" } },
        { id = "journal/quest_list", kind = "quest_list", anchor = "left", size = { width = 30, height = 74 } },
        { id = "journal/objectives", kind = "objective_list", anchor = "center", size = { width = 34, height = 74 } },
        { id = "journal/notes_panel", kind = "notes_panel", anchor = "right", size = { width = 22, height = 74 } }
      },
      sections = copy_array(sections),
      objective_layout = objective_layout,
      tracked = {
        limit = tracked_limit,
        binding = "quest.tracked",
        objective_binding = "quest.tracked.objectives"
      },
      integrations = {
        quest_source = "quest.catalog",
        dialogue_reference_source = "dialogue.nodes",
        stage_binding = "quest.selected.stage",
        note_source = "notes.entries",
        codex_source = "codex.entries"
      },
      notes = notes
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
