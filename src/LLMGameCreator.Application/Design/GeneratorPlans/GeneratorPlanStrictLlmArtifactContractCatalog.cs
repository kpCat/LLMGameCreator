using System.Collections.ObjectModel;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanStrictLlmArtifactContractCatalog
{
    private readonly IReadOnlyDictionary<string, GeneratorPlanStrictLlmArtifactContractDefinition> _contracts;
    private readonly IReadOnlyDictionary<string, GeneratorPlanStrictLlmArtifactBatchPresetDefinition> _batchPresets;

    public GeneratorPlanStrictLlmArtifactContractCatalog()
    {
        _contracts = new ReadOnlyDictionary<string, GeneratorPlanStrictLlmArtifactContractDefinition>(
            new[]
            {
                GameProfile(),
                RegionPack(),
                ScenePack(),
                NpcPack(),
                QuestPack(),
                DialoguePack(),
                MechanicsPack(),
                EncounterPack(),
                ItemPack()
            }.ToDictionary(contract => contract.ContractId, StringComparer.OrdinalIgnoreCase));

        _batchPresets = new ReadOnlyDictionary<string, GeneratorPlanStrictLlmArtifactBatchPresetDefinition>(
            new[]
            {
                Preset("baseline_game_seed", "Baseline game seed", ["game_profile_v1", "scene_pack_v1", "quest_pack_v1", "mechanics_pack_v1"]),
                Preset("world_content_expansion", "World content expansion", ["region_pack_v1", "scene_pack_v1"]),
                Preset("character_content_expansion", "Character content expansion", ["npc_pack_v1", "dialogue_pack_v1"]),
                Preset("encounter_item_expansion", "Encounter and item expansion", ["encounter_pack_v1", "item_pack_v1"]),
                Preset("full_small_rpg_seed", "Full small RPG seed", ["game_profile_v1", "region_pack_v1", "scene_pack_v1", "npc_pack_v1", "quest_pack_v1", "dialogue_pack_v1", "mechanics_pack_v1", "encounter_pack_v1", "item_pack_v1"])
            }.ToDictionary(preset => preset.PresetId, StringComparer.OrdinalIgnoreCase));
    }

    public IReadOnlyList<GeneratorPlanStrictLlmArtifactContractDefinition> ListContracts()
    {
        return _contracts.Values.OrderBy(contract => contract.ContractId, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public bool TryGet(string contractId, out GeneratorPlanStrictLlmArtifactContractDefinition contract)
    {
        return _contracts.TryGetValue(contractId.Trim(), out contract!);
    }

    public IReadOnlyList<GeneratorPlanStrictLlmArtifactBatchPresetDefinition> ListBatchPresets()
    {
        return _batchPresets.Values.OrderBy(preset => preset.PresetId, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public bool TryGetBatchPreset(string presetId, out GeneratorPlanStrictLlmArtifactBatchPresetDefinition preset)
    {
        return _batchPresets.TryGetValue(presetId.Trim(), out preset!);
    }

    private static GeneratorPlanStrictLlmArtifactContractDefinition GameProfile()
    {
        return new GeneratorPlanStrictLlmArtifactContractDefinition
        {
            ContractId = "game_profile_v1",
            ArtifactKind = "game_profile_v1",
            Label = "Game profile",
            Purpose = "Define the compact game concept, variants, pillars and core loop.",
            RequiredTopLevelFields = ["schema_version", "artifact_kind", "game", "pillars", "source_context"],
            RequiredPayloadFields = ["game.title", "game.description", "game.genre", "game.tone", "game.presentation_mode", "game.world_topology", "game.actor_model", "game.combat_model", "game.core_loop"],
            SystemPromptAdditions = ["Generate a compact game profile and concept seed only."],
            OutputSchema = """
            {
              "schema_version": "0.1",
              "artifact_kind": "game_profile_v1",
              "game": {
                "title": "...",
                "description": "...",
                "genre": "...",
                "tone": "...",
                "presentation_mode": "...",
                "world_topology": "...",
                "actor_model": "...",
                "combat_model": "...",
                "core_loop": ["..."]
              },
              "pillars": ["..."],
              "source_context": {
                "capability_selection_id": "...",
                "selected_variant_ids": {}
              }
            }
            """,
            ValidationRules = ["game.core_loop must be a non-empty array.", "machine-readable ids and enums must be preserved."],
            RepairGuidance = ["Add missing profile fields.", "Do not change selected variant ids or artifact_kind."]
        };
    }

    private static GeneratorPlanStrictLlmArtifactContractDefinition ScenePack()
    {
        return new GeneratorPlanStrictLlmArtifactContractDefinition
        {
            ContractId = "scene_pack_v1",
            ArtifactKind = "scene_pack_v1",
            Label = "Scene pack",
            Purpose = "Define a small set of scene seeds for package assembly.",
            RequiredTopLevelFields = ["schema_version", "artifact_kind", "scenes", "source_context"],
            RequiredPayloadFields = ["scenes[].id", "scenes[].title", "scenes[].description", "scenes[].purpose"],
            SystemPromptAdditions = ["Generate a small scene seed pack only."],
            OutputSchema = """
            {
              "schema_version": "0.1",
              "artifact_kind": "scene_pack_v1",
              "scenes": [
                {
                  "id": "scene/start",
                  "title": "...",
                  "description": "...",
                  "purpose": "..."
                }
              ],
              "source_context": {}
            }
            """,
            ValidationRules = ["scenes must be a non-empty array.", "scene ids must be lowercase slash ids."],
            RepairGuidance = ["Add at least one valid scene.", "Keep artifact_kind scene_pack_v1."]
        };
    }

    private static GeneratorPlanStrictLlmArtifactContractDefinition QuestPack()
    {
        return new GeneratorPlanStrictLlmArtifactContractDefinition
        {
            ContractId = "quest_pack_v1",
            ArtifactKind = "quest_pack_v1",
            Label = "Quest pack",
            Purpose = "Define a small set of quest seeds with bounded objectives or steps.",
            RequiredTopLevelFields = ["schema_version", "artifact_kind", "quests", "source_context"],
            RequiredPayloadFields = ["quests[].id", "quests[].title", "quests[].description", "quests[].steps", "quests[].objectives"],
            SystemPromptAdditions = ["Generate a small quest seed pack only."],
            OutputSchema = """
            {
              "schema_version": "0.1",
              "artifact_kind": "quest_pack_v1",
              "quests": [
                {
                  "id": "quest/intro",
                  "title": "...",
                  "description": "...",
                  "steps": ["..."],
                  "objectives": []
                }
              ],
              "source_context": {}
            }
            """,
            ValidationRules = ["quests must be a non-empty array.", "each quest must have steps or objectives.", "quest ids must be lowercase slash ids."],
            RepairGuidance = ["Add missing steps or objectives.", "Keep quest ids lowercase slash ids."]
        };
    }

    private static GeneratorPlanStrictLlmArtifactContractDefinition MechanicsPack()
    {
        return new GeneratorPlanStrictLlmArtifactContractDefinition
        {
            ContractId = "mechanics_pack_v1",
            ArtifactKind = "mechanics_pack_v1",
            Label = "Mechanics pack",
            Purpose = "Define a small set of declarative mechanic or ability seeds.",
            RequiredTopLevelFields = ["schema_version", "artifact_kind", "mechanics", "source_context"],
            RequiredPayloadFields = ["mechanics[].id", "mechanics[].name", "mechanics[].title", "mechanics[].description", "mechanics[].tags"],
            SystemPromptAdditions = ["Generate a small mechanic or ability seed pack only."],
            OutputSchema = """
            {
              "schema_version": "0.1",
              "artifact_kind": "mechanics_pack_v1",
              "mechanics": [
                {
                  "id": "mechanic/core",
                  "name": "...",
                  "title": "...",
                  "description": "...",
                  "tags": ["..."]
                }
              ],
              "source_context": {}
            }
            """,
            ValidationRules = ["mechanics must be a non-empty array.", "each mechanic must have title or name.", "mechanic ids must be lowercase slash ids."],
            RepairGuidance = ["Add missing title or name.", "Keep artifact_kind mechanics_pack_v1."]
        };
    }

    private static GeneratorPlanStrictLlmArtifactContractDefinition RegionPack()
    {
        return new GeneratorPlanStrictLlmArtifactContractDefinition
        {
            ContractId = "region_pack_v1",
            ArtifactKind = "region_pack_v1",
            Label = "Region pack",
            Purpose = "Define a bounded set of world regions referenced by scenes, NPCs and encounters.",
            RequiredTopLevelFields = ["schema_version", "artifact_kind", "regions", "source_context"],
            RequiredPayloadFields = ["regions[].id", "regions[].title", "regions[].description"],
            SystemPromptAdditions = ["Generate 1-3 compact region records only.", "Use existing scene ids as string references when present."],
            OutputSchema = """
            {
              "schema_version": "0.1",
              "artifact_kind": "region_pack_v1",
              "regions": [
                {
                  "id": "region/start",
                  "title": "...",
                  "description": "...",
                  "scene_ids": ["scene/start"]
                }
              ],
              "source_context": {}
            }
            """,
            ValidationRules = ["regions must be a non-empty array with unique lowercase slash ids.", "scene_ids must contain string ids when present."],
            RepairGuidance = ["Add at least one valid region.", "Keep artifact_kind region_pack_v1 and preserve referenced ids exactly."]
        };
    }

    private static GeneratorPlanStrictLlmArtifactContractDefinition NpcPack()
    {
        return new GeneratorPlanStrictLlmArtifactContractDefinition
        {
            ContractId = "npc_pack_v1",
            ArtifactKind = "npc_pack_v1",
            Label = "NPC pack",
            Purpose = "Define a bounded set of NPC seeds and their region or scene references.",
            RequiredTopLevelFields = ["schema_version", "artifact_kind", "npcs", "source_context"],
            RequiredPayloadFields = ["npcs[].id", "npcs[].name", "npcs[].description"],
            SystemPromptAdditions = ["Generate 1-4 compact NPC records only.", "Use region_id and scene_id only as exact string references when present."],
            OutputSchema = """
            {
              "schema_version": "0.1",
              "artifact_kind": "npc_pack_v1",
              "npcs": [
                {
                  "id": "npc/guide",
                  "name": "...",
                  "description": "...",
                  "region_id": "region/start",
                  "scene_id": "scene/start"
                }
              ],
              "source_context": {}
            }
            """,
            ValidationRules = ["npcs must be a non-empty array with unique lowercase slash ids.", "region_id and scene_id must be strings when present."],
            RepairGuidance = ["Add at least one valid NPC.", "Keep artifact_kind npc_pack_v1 and preserve referenced ids exactly."]
        };
    }

    private static GeneratorPlanStrictLlmArtifactContractDefinition ItemPack()
    {
        return new GeneratorPlanStrictLlmArtifactContractDefinition
        {
            ContractId = "item_pack_v1",
            ArtifactKind = "item_pack_v1",
            Label = "Item pack",
            Purpose = "Define a bounded set of item seeds without economy or effect execution.",
            RequiredTopLevelFields = ["schema_version", "artifact_kind", "items", "source_context"],
            RequiredPayloadFields = ["items[].id", "items[].name", "items[].description"],
            SystemPromptAdditions = ["Generate 1-5 compact declarative item records only.", "Do not generate executable effects, code or economy simulation."],
            OutputSchema = """
            {
              "schema_version": "0.1",
              "artifact_kind": "item_pack_v1",
              "items": [
                {
                  "id": "item/field_kit",
                  "name": "...",
                  "description": "..."
                }
              ],
              "source_context": {}
            }
            """,
            ValidationRules = ["items must be a non-empty array with unique lowercase slash ids."],
            RepairGuidance = ["Add at least one valid item.", "Keep artifact_kind item_pack_v1."]
        };
    }

    private static GeneratorPlanStrictLlmArtifactContractDefinition DialoguePack()
    {
        return new GeneratorPlanStrictLlmArtifactContractDefinition
        {
            ContractId = "dialogue_pack_v1",
            ArtifactKind = "dialogue_pack_v1",
            Label = "Dialogue pack",
            Purpose = "Define a bounded set of dialogue summaries and NPC or scene references.",
            RequiredTopLevelFields = ["schema_version", "artifact_kind", "dialogues", "source_context"],
            RequiredPayloadFields = ["dialogues[].id", "dialogues[].title", "dialogues[].description", "dialogues[].lines"],
            SystemPromptAdditions = ["Generate 1-3 compact dialogue records with 1-4 short lines each.", "Use npc_id and scene_id only as exact string references when present."],
            OutputSchema = """
            {
              "schema_version": "0.1",
              "artifact_kind": "dialogue_pack_v1",
              "dialogues": [
                {
                  "id": "dialogue/guide_intro",
                  "title": "...",
                  "description": "...",
                  "npc_id": "npc/guide",
                  "scene_id": "scene/start",
                  "lines": ["..."]
                }
              ],
              "source_context": {}
            }
            """,
            ValidationRules = ["dialogues must be a non-empty array with unique lowercase slash ids.", "lines must be non-empty.", "npc_id and scene_id must be strings when present."],
            RepairGuidance = ["Add at least one valid dialogue with a line.", "Keep artifact_kind dialogue_pack_v1 and preserve referenced ids exactly."]
        };
    }

    private static GeneratorPlanStrictLlmArtifactContractDefinition EncounterPack()
    {
        return new GeneratorPlanStrictLlmArtifactContractDefinition
        {
            ContractId = "encounter_pack_v1",
            ArtifactKind = "encounter_pack_v1",
            Label = "Encounter pack",
            Purpose = "Define bounded encounter summaries without combat or effect execution.",
            RequiredTopLevelFields = ["schema_version", "artifact_kind", "encounters", "source_context"],
            RequiredPayloadFields = ["encounters[].id", "encounters[].title", "encounters[].description"],
            SystemPromptAdditions = ["Generate 1-3 compact declarative encounter records only.", "Use region_id, scene_id and npc_ids only as exact id references; do not generate executable combat or effects."],
            OutputSchema = """
            {
              "schema_version": "0.1",
              "artifact_kind": "encounter_pack_v1",
              "encounters": [
                {
                  "id": "encounter/road_block",
                  "title": "...",
                  "description": "...",
                  "region_id": "region/start",
                  "scene_id": "scene/start",
                  "npc_ids": ["npc/guide"]
                }
              ],
              "source_context": {}
            }
            """,
            ValidationRules = ["encounters must be a non-empty array with unique lowercase slash ids.", "region_id and scene_id must be strings and npc_ids must contain strings when present."],
            RepairGuidance = ["Add at least one valid encounter.", "Keep artifact_kind encounter_pack_v1 and preserve referenced ids exactly."]
        };
    }

    private static GeneratorPlanStrictLlmArtifactBatchPresetDefinition Preset(string id, string label, IReadOnlyList<string> contractIds)
    {
        return new GeneratorPlanStrictLlmArtifactBatchPresetDefinition
        {
            PresetId = id,
            Label = label,
            ContractIds = contractIds
        };
    }
}

public sealed record GeneratorPlanStrictLlmArtifactContractDefinition
{
    public string ContractId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredTopLevelFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SystemPromptAdditions { get; init; } = Array.Empty<string>();
    public string OutputSchema { get; init; } = "{}";
    public IReadOnlyList<string> ValidationRules { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RepairGuidance { get; init; } = Array.Empty<string>();
}

public sealed record GeneratorPlanStrictLlmArtifactBatchPresetDefinition
{
    public string PresetId { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public IReadOnlyList<string> ContractIds { get; init; } = Array.Empty<string>();
}
