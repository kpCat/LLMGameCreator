using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal161;
using LLMGameCreator.Tests.Application.Goal162;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal168;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal168HistoryRegenerationTests
{
    [Fact]
    public void Behavioral_v6_primary_truth_belongs_to_relationship_route()
    {
        var build = Goal168TestKit.Build;
        var history = ReadHistory(build.BuildHistoryPath);

        Assert.Equal(GameProjectBuildHistoryReader.SchemaVersionV6,
            history.SchemaVersion);
        Assert.Equal(build.GeneratedCampaignRelationships?.FinalStateHash,
            build.FinalStateHash);
        Assert.Equal("generated-campaign-relationship-v1",
            build.RuntimePlaythroughPlanId);
    }

    [Fact]
    public void Behavioral_v6_keeps_choice_and_combat_as_independent_exact_truth()
    {
        var build = Goal168TestKit.Build;

        Assert.Equal(build.PackageSha256,
            build.GeneratedEncounterCombat?.ExactPackageSha256);
        Assert.Equal(build.PackageSha256,
            build.GeneratedCampaignChoices?.FinalPackageSha256);
        Assert.Equal(build.GeneratedEncounterCombat?.QualifiedActionsSha256,
            build.GeneratedCampaignRelationships?.QualifiedActionsSha256);
    }

    [Fact]
    public void Behavioral_regeneration_seal_contains_relationship_summary_overlay_inventory()
    {
        var seal = Goal164RegenerationState.Value.Seal;

        Assert.Equal(64,
            seal.GeneratedCampaignRelationshipSummarySha256.Length);
        Assert.Equal(64,
            seal.GeneratedCampaignRelationshipOverlaySha256.Length);
        Assert.Equal(64,
            seal.GeneratedCampaignRelationshipInventorySha256.Length);
    }

    [Fact]
    public void Behavioral_regeneration_and_rollback_restore_relationship_current()
    {
        var state = Goal164RegenerationState.Value;

        Assert.True(state.Applied.Applied,
            string.Join(",", state.Applied.Diagnostics));
        Assert.Equal("RELATIONSHIPS_CURRENT",
            state.AfterRegeneration.GeneratedCampaignRelationships?.Status);
        Assert.True(state.RolledBack.Applied,
            string.Join(",", state.RolledBack.Diagnostics));
        Assert.Equal("RELATIONSHIPS_CURRENT",
            state.AfterRollback.GeneratedCampaignRelationships?.Status);
    }

    [Fact]
    public void Behavioral_genuine_v5_projects_relationships_pending()
    {
        var snapshot = Goal168V5State.Value.Snapshot;

        Assert.Equal("CHOICE_CURRENT",
            snapshot.GeneratedCampaignChoices?.Status);
        Assert.Equal("RELATIONSHIPS_PENDING",
            snapshot.GeneratedCampaignRelationships?.Status);
        Assert.Equal("RELATIONSHIPS_PENDING",
            snapshot.GeneratedWorld?.Status);
    }

    [Fact]
    public void Behavioral_genuine_v5_is_project_not_ready()
    {
        var capture = Goal168V5State.Value.Capture;

        Assert.Equal(GeneratedCampaignSessionStatus.PROJECT_NOT_READY,
            capture.Status);
        Assert.Contains("campaign.generated_relationships_not_current",
            capture.Diagnostics);
    }

    [Fact]
    public void Behavioral_genuine_v5_offers_one_build_upgrade()
    {
        var presentation = Goal162ProjectsTestKit.Presentation(
            Goal168V5State.Value.Snapshot);

        Assert.True(presentation.Enabled);
        Assert.False(presentation.Current);
        Assert.Equal("Собрать и играть", presentation.Title);
    }

    [Fact]
    public void Behavioral_relationship_seal_hash_changes_on_overlay_tamper()
    {
        var build = Goal168TestKit.Build;
        var summary = Assert.IsType<
            GameProjectGeneratedCampaignRelationshipSummary>(
            build.GeneratedCampaignRelationships);
        var overlay = Assert.IsType<GeneratedCampaignRelationshipOverlayDocument>(
            summary.Overlay);

        Assert.NotEqual(
            GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(
                overlay),
            GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(
                overlay with
                {
                    InventorySha256 = new string('a', 64)
                }));
    }

    private static GameProjectBuildHistoryEntry ReadHistory(string path) =>
        JsonSerializer.Deserialize<GameProjectBuildHistoryEntry>(
            File.ReadAllText(path),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
}

internal static class Goal168V5State
{
    private static readonly Lazy<Goal168V5Fixture> Fixture = new(Create);
    internal static Goal168V5Fixture Value => Fixture.Value;

    private static Goal168V5Fixture Create()
    {
        var source = Goal168TestKit.Real;
        var project = Goal156TestKit.Copy(source.Project,
            "goal168-genuine-v5");
        var historyRoot = Path.Combine(project.Path,
            UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot
                .Replace('/', Path.DirectorySeparatorChar));
        foreach (var path in Directory.EnumerateFiles(historyRoot, "*.json"))
            File.Delete(path);
        var v6 = JsonSerializer.Deserialize<GameProjectBuildHistoryEntry>(
            File.ReadAllText(source.Build.BuildHistoryPath),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
        var v5 = v6 with
        {
            SchemaVersion = GameProjectBuildHistoryReader.SchemaVersionV5,
            FinalStateHash = v6.GeneratedCampaignChoices!.FinalStateHash,
            GeneratedCampaignRelationships = null
        };
        File.WriteAllText(Path.Combine(historyRoot, "genuine-v5.json"),
            JsonSerializer.Serialize(v5,
                new JsonSerializerOptions { WriteIndented = true }));

        var authoringRoot = Path.Combine(project.Path,
            UnifiedGameProjectWorkspaceVocabulary.AuthoringRelativeRoot
                .Replace('/', Path.DirectorySeparatorChar));
        var authoringPath = Directory.EnumerateFiles(authoringRoot,
            "*" + FeatureModuleCompositionDocumentVocabulary.FileExtension)
            .Single();
        var authoring =
            JsonSerializer.Deserialize<FeatureModuleCompositionDocument>(
                File.ReadAllText(authoringPath),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })! with
            {
                LastQualifiedFinalStateHash = v5.FinalStateHash
            };
        File.WriteAllText(authoringPath,
            JsonSerializer.Serialize(authoring,
                new JsonSerializerOptions { WriteIndented = true }));

        var controller = Goal156TestKit.OpenWorkspace(project.Path);
        var snapshot = controller.Snapshot();
        var current = new CurrentGamePackageService(Goal156TestKit.Repository);
        current.LoadAsync(project.Path, CancellationToken.None)
            .GetAwaiter().GetResult();
        var saves = Goal161ServiceBundle.Create(
            source: Goal156TestKit.SourceService);
        var capture = new GeneratedCampaignSessionTruthService(
            current, saves.Validator, saves.Coordinator).Capture();
        return new Goal168V5Fixture(project, snapshot, capture);
    }
}

internal sealed record Goal168V5Fixture(
    GeneratedProject Project,
    UnifiedGameProjectWorkspaceSnapshot Snapshot,
    (GeneratedCampaignSessionStatus Status,
        GeneratedCampaignProjectTruth? Truth,
        IReadOnlyList<string> Diagnostics) Capture);
