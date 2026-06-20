using LLMGameCreator.Application.Design.GeneratorPlans;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanStrictLlmArtifactValidatorTests
{
    private readonly GeneratorPlanStrictLlmArtifactContractCatalog _catalog = new();
    private readonly GeneratorPlanStrictLlmArtifactValidator _validator = new();

    [Fact]
    public void ValidGameProfilePasses()
    {
        var diagnostics = _validator.Validate(ValidGameProfile(), Contract("game_profile_v1"));

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
    }

    [Fact]
    public void RejectsWrongArtifactKind()
    {
        var json = ValidGameProfile().Replace("game_profile_v1", "scene_pack_v1", StringComparison.Ordinal);

        var diagnostics = _validator.Validate(json, Contract("game_profile_v1"));

        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.WrongArtifactKind);
    }

    [Fact]
    public void RejectsForbiddenCodeFields()
    {
        var json = """
        {
          "schema_version": "0.1",
          "artifact_kind": "scene_pack_v1",
          "scenes": [{ "id": "scene/start", "title": "Start", "description": "Start.", "purpose": "Intro." }],
          "source_context": {},
          "code": "run"
        }
        """;

        var diagnostics = _validator.Validate(json, Contract("scene_pack_v1"));

        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.ForbiddenField);
    }

    [Fact]
    public void RejectsQuestWithoutStepsOrObjectives()
    {
        var json = """
        {
          "schema_version": "0.1",
          "artifact_kind": "quest_pack_v1",
          "quests": [{ "id": "quest/intro", "title": "Intro", "description": "Intro.", "steps": [], "objectives": [] }],
          "source_context": {}
        }
        """;

        var diagnostics = _validator.Validate(json, Contract("quest_pack_v1"));

        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.InvalidContractContent);
    }

    [Fact]
    public void RejectsMechanicWithoutTitleOrName()
    {
        var json = """
        {
          "schema_version": "0.1",
          "artifact_kind": "mechanics_pack_v1",
          "mechanics": [{ "id": "mechanic/core", "description": "Core.", "tags": [] }],
          "source_context": {}
        }
        """;

        var diagnostics = _validator.Validate(json, Contract("mechanics_pack_v1"));

        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.InvalidContractContent);
    }

    [Fact]
    public void CatalogExposesExpandedContractsAndBatchPresets()
    {
        var contractIds = _catalog.ListContracts().Select(contract => contract.ContractId).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.All(
            new[] { "region_pack_v1", "npc_pack_v1", "item_pack_v1", "dialogue_pack_v1", "encounter_pack_v1" },
            contractId => Assert.Contains(contractId, contractIds));
        Assert.All(_catalog.ListContracts().Where(contract => contractIds.Contains(contract.ContractId)), contract =>
        {
            Assert.False(string.IsNullOrWhiteSpace(contract.Label));
            Assert.False(string.IsNullOrWhiteSpace(contract.Purpose));
        });

        Assert.True(_catalog.TryGetBatchPreset("full_small_rpg_seed", out var fullPreset));
        Assert.Equal(
            ["game_profile_v1", "region_pack_v1", "scene_pack_v1", "npc_pack_v1", "quest_pack_v1", "dialogue_pack_v1", "mechanics_pack_v1", "encounter_pack_v1", "item_pack_v1"],
            fullPreset.ContractIds);
        Assert.Equal(5, _catalog.ListBatchPresets().Count);
    }

    [Theory]
    [InlineData("region_pack_v1")]
    [InlineData("npc_pack_v1")]
    [InlineData("item_pack_v1")]
    [InlineData("dialogue_pack_v1")]
    [InlineData("encounter_pack_v1")]
    public void ValidExpandedStrictContractPasses(string contractId)
    {
        var diagnostics = _validator.Validate(ValidExpandedArtifact(contractId), Contract(contractId));

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
    }

    [Theory]
    [InlineData("region_pack_v1")]
    [InlineData("npc_pack_v1")]
    [InlineData("item_pack_v1")]
    [InlineData("dialogue_pack_v1")]
    [InlineData("encounter_pack_v1")]
    public void ExpandedStrictContractRejectsWrongArtifactKind(string contractId)
    {
        var json = ValidExpandedArtifact(contractId).Replace(contractId, "wrong_pack_v1", StringComparison.Ordinal);

        var diagnostics = _validator.Validate(json, Contract(contractId));

        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.WrongArtifactKind);
    }

    [Fact]
    public void ExpandedStrictContractRejectsDuplicateIdsAndNonStringReferences()
    {
        var json = """
        {
          "schema_version": "0.1",
          "artifact_kind": "npc_pack_v1",
          "npcs": [
            { "id": "npc/guide", "name": "Guide", "description": "First.", "region_id": 1 },
            { "id": "npc/guide", "name": "Guide Two", "description": "Second." }
          ],
          "source_context": {}
        }
        """;

        var diagnostics = _validator.Validate(json, Contract("npc_pack_v1"));

        Assert.Contains(diagnostics, diagnostic => diagnostic.Message.Contains("Duplicate id", StringComparison.Ordinal));
        Assert.Contains(diagnostics, diagnostic => diagnostic.Target.EndsWith("region_id", StringComparison.Ordinal));
    }

    private GeneratorPlanStrictLlmArtifactContractDefinition Contract(string id)
    {
        Assert.True(_catalog.TryGet(id, out var contract));
        return contract;
    }

    private static string ValidGameProfile()
    {
        return """
        {
          "schema_version": "0.1",
          "artifact_kind": "game_profile_v1",
          "game": {
            "title": "Test",
            "description": "Test game.",
            "genre": "RPG",
            "tone": "Bright",
            "presentation_mode": "presentation_mode/top_down_2d",
            "world_topology": "world_topology/single_map",
            "actor_model": "actor_model/single_player_character",
            "combat_model": "combat_model/turn_based",
            "core_loop": ["explore"]
          },
          "pillars": ["clear goals"],
          "source_context": {
            "capability_selection_id": "selection/test",
            "selected_variant_ids": {}
          }
        }
        """;
    }

    private static string ValidExpandedArtifact(string contractId)
    {
        return contractId switch
        {
            "region_pack_v1" => """{"schema_version":"0.1","artifact_kind":"region_pack_v1","regions":[{"id":"region/start","title":"Start","description":"Start region.","scene_ids":["scene/start"]}],"source_context":{}}""",
            "npc_pack_v1" => """{"schema_version":"0.1","artifact_kind":"npc_pack_v1","npcs":[{"id":"npc/guide","name":"Guide","description":"A guide.","region_id":"region/start","scene_id":"scene/start"}],"source_context":{}}""",
            "item_pack_v1" => """{"schema_version":"0.1","artifact_kind":"item_pack_v1","items":[{"id":"item/kit","name":"Kit","description":"A field kit."}],"source_context":{}}""",
            "dialogue_pack_v1" => """{"schema_version":"0.1","artifact_kind":"dialogue_pack_v1","dialogues":[{"id":"dialogue/intro","title":"Intro","description":"An introduction.","npc_id":"npc/guide","scene_id":"scene/start","lines":["Welcome."]}],"source_context":{}}""",
            "encounter_pack_v1" => """{"schema_version":"0.1","artifact_kind":"encounter_pack_v1","encounters":[{"id":"encounter/road","title":"Road","description":"A road encounter.","region_id":"region/start","scene_id":"scene/start","npc_ids":["npc/guide"]}],"source_context":{}}""",
            _ => throw new ArgumentOutOfRangeException(nameof(contractId), contractId, null)
        };
    }
}
