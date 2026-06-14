using System.Collections.ObjectModel;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanStrictLlmArtifactContractCatalog
{
    private readonly IReadOnlyDictionary<string, GeneratorPlanStrictLlmArtifactContractDefinition> _contracts;

    public GeneratorPlanStrictLlmArtifactContractCatalog()
    {
        _contracts = new ReadOnlyDictionary<string, GeneratorPlanStrictLlmArtifactContractDefinition>(
            new[]
            {
                GameProfile(),
                ScenePack(),
                QuestPack(),
                MechanicsPack()
            }.ToDictionary(contract => contract.ContractId, StringComparer.OrdinalIgnoreCase));
    }

    public IReadOnlyList<GeneratorPlanStrictLlmArtifactContractDefinition> ListContracts()
    {
        return _contracts.Values.OrderBy(contract => contract.ContractId, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public bool TryGet(string contractId, out GeneratorPlanStrictLlmArtifactContractDefinition contract)
    {
        return _contracts.TryGetValue(contractId.Trim(), out contract!);
    }

    private static GeneratorPlanStrictLlmArtifactContractDefinition GameProfile()
    {
        return new GeneratorPlanStrictLlmArtifactContractDefinition
        {
            ContractId = "game_profile_v1",
            ArtifactKind = "game_profile_v1",
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
}

public sealed record GeneratorPlanStrictLlmArtifactContractDefinition
{
    public string ContractId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredTopLevelFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SystemPromptAdditions { get; init; } = Array.Empty<string>();
    public string OutputSchema { get; init; } = "{}";
    public IReadOnlyList<string> ValidationRules { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RepairGuidance { get; init; } = Array.Empty<string>();
}
