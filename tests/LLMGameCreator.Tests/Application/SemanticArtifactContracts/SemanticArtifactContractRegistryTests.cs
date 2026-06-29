using System.Text.Json;
using LLMGameCreator.Application.Design.SemanticArtifactContracts;
using Xunit;

namespace LLMGameCreator.Tests.Application.SemanticArtifactContracts;

public sealed class SemanticArtifactContractRegistryTests
{
    [Fact]
    public void RegistrySeedValidatesAndCoversFullGeneratorSpine()
    {
        var contracts = SemanticArtifactContractRegistry.BuildDefaultContracts();
        var diagnostics = SemanticArtifactContractValidator.ValidateContracts(contracts);

        Assert.DoesNotContain(diagnostics, item => item.Severity == "error");
        Assert.Contains(contracts, item => item.ContractId == "game_profile_v1");
        Assert.Contains(contracts, item => item.ContractId == "semantic_pack_v1");
        Assert.Contains(contracts, item => item.ContractId == "world_topology_region_route_graph_v1");
        Assert.Contains(contracts, item => item.ContractId == "entity_archetype_npc_actor_profile_v1");
        Assert.Contains(contracts, item => item.ContractId == "quest_graph_objective_reward_pattern_v1");
        Assert.Contains(contracts, item => item.ContractId == "item_resource_recipe_loot_economy_v1");
        Assert.Contains(contracts, item => item.ContractId == "combat_progression_ability_v1");
        Assert.Contains(contracts, item => item.ContractId == "settlement_building_landmark_v1" && item.LifecycleStatus == "future_required");
        Assert.Contains(contracts, item => item.ContractId == "presentation_export_ir_v1" && item.LifecycleStatus == "future_required");
    }

    [Fact]
    public void DependencyOrderIsStableAndPlacesDependenciesBeforeConsumers()
    {
        var contracts = SemanticArtifactContractRegistry.BuildDefaultContracts();

        var first = SemanticArtifactContractValidator.ResolveDependencyOrder(contracts);
        var second = SemanticArtifactContractValidator.ResolveDependencyOrder(contracts);

        Assert.Equal(first, second);
        var order = first.ToList();
        Assert.True(order.IndexOf("semantic_pack_v1") < order.IndexOf("quest_graph_objective_reward_pattern_v1"));
        Assert.True(order.IndexOf("item_resource_recipe_loot_economy_v1") < order.IndexOf("quest_graph_objective_reward_pattern_v1"));
        Assert.True(order.IndexOf("entity_archetype_npc_actor_profile_v1") < order.IndexOf("combat_progression_ability_v1"));
    }

    [Fact]
    public void SemanticPacksContainThreeDeterministicScenarios()
    {
        var packs = SemanticArtifactContractRegistry.BuildDefaultSemanticPacks();

        Assert.Contains(packs, pack => pack.SupportedProfileIds.Contains("frontier_survival"));
        Assert.Contains(packs, pack => pack.SupportedProfileIds.Contains("gothic_intrigue"));
        Assert.Contains(packs, pack => pack.SupportedProfileIds.Contains("caravan_trade"));
        Assert.Equal(packs.OrderBy(pack => pack.OrderingKey, StringComparer.Ordinal).Select(pack => pack.PackId), packs.OrderBy(pack => pack.OrderingKey, StringComparer.Ordinal).Select(pack => pack.PackId));
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndInspectable()
    {
        using var temp = new TempDirectory();
        var service = new SemanticArtifactContractEvidenceService();
        var result = service.Build();
        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(File.Exists(write.RegistrySummaryJsonPath));
        Assert.True(File.Exists(write.CompatibilityMatrixJsonPath));
        Assert.True(File.Exists(write.FrontierPlanJsonPath));
        Assert.True(File.Exists(write.GothicPlanJsonPath));
        Assert.True(File.Exists(write.CaravanPlanJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.Contains("semantic_artifact_contract_registry_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));

        using var registryJson = JsonDocument.Parse(await File.ReadAllTextAsync(write.RegistrySummaryJsonPath));
        Assert.Equal(13, registryJson.RootElement.GetProperty("contractCount").GetInt32());
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
