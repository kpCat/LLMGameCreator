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
}
