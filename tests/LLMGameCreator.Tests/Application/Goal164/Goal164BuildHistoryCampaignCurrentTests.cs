using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal164;

[Collection(Goal160Collection.Name)]
public sealed class Goal164BuildHistoryCampaignCurrentTests
{
    [Fact]
    public void Behavioral_successful_generated_regional_event_route_writes_v7_history()
    {
        var entry = History(Goal164TestKit.AllSelectable.Build.BuildHistoryPath);

        Assert.Equal(GameProjectBuildHistoryReader.SchemaVersionV7, entry.SchemaVersion);
        Assert.Equal("CAMPAIGN_CURRENT", entry.GeneratedEncounterCombat?.Status);
        Assert.Equal("CHOICE_CURRENT", entry.GeneratedCampaignChoices?.Status);
        Assert.Equal("RELATIONSHIPS_CURRENT",
            entry.GeneratedCampaignRelationships?.Status);
        Assert.Equal("REGIONAL_EVENTS_CURRENT",
            entry.GeneratedCampaignRegionalEvents?.Status);
    }

    [Fact]
    public void Behavioral_v7_history_carries_exact_primary_hashes()
    {
        var fixture = Goal164TestKit.AllSelectable;
        var entry = History(fixture.Build.BuildHistoryPath);

        Assert.Equal(fixture.Build.PackageSha256, entry.PackageSha256);
        Assert.Equal(fixture.Build.CompositionPackageSha256, entry.CompositionPackageSha256);
        Assert.Equal(fixture.Build.FinalStateHash, entry.FinalStateHash);
        Assert.Equal(entry.PackageSha256, entry.GeneratedEncounterCombat?.ExactPackageSha256);
    }

    [Fact]
    public void Behavioral_history_reader_restores_campaign_current_combat()
    {
        var fixture = Goal164TestKit.AllSelectable;
        var result = new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(
            fixture.Project.Path, Authoring(fixture.Project.Path));

        Assert.NotNull(result.LastSuccessfulBuild);
        Assert.Equal("CAMPAIGN_CURRENT", result.LastSuccessfulBuild!.GeneratedEncounterCombat?.Status);
        Assert.Equal("CAMPAIGN_CURRENT", result.LastSuccessfulBuild.GeneratedWorld?.Status);
    }

    [Fact]
    public void Behavioral_genuine_v3_history_projects_only_combat_pending()
    {
        using var copy = V3Copy();
        var result = new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(
            copy.Project.Path, Authoring(copy.Project.Path));

        Assert.NotNull(result.LastSuccessfulBuild);
        Assert.Equal("COMBAT_PENDING", result.LastSuccessfulBuild!.GeneratedEncounterCombat?.Status);
        Assert.False(result.LastSuccessfulBuild.GeneratedEncounterCombat?.Passed);
    }

    [Fact]
    public void Behavioral_genuine_v3_history_is_never_campaign_current()
    {
        using var copy = V3Copy();
        var result = new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(
            copy.Project.Path, Authoring(copy.Project.Path));

        Assert.NotEqual("CAMPAIGN_CURRENT", result.LastSuccessfulBuild?.GeneratedWorld?.Status);
        Assert.NotEqual("CAMPAIGN_CURRENT", result.LastSuccessfulBuild?.GeneratedEncounterCombat?.Status);
    }

    [Fact]
    public void Behavioral_lane_a_accepted_mechanics_hashes_remain_compatibility_truth()
    {
        var build = Goal164TestKit.AllSelectable.Build;
        var laneA = Assert.IsType<GameProjectAcceptedMechanicsCompatibilityResult>(
            build.AcceptedMechanicsCompatibility);

        Assert.True(laneA.Passed);
        Assert.NotEqual(laneA.CompatibilityCompositionPackageSha256, build.CompositionPackageSha256);
        Assert.NotEqual(laneA.CompatibilityActivatedPackageSha256, build.PackageSha256);
        Assert.Equal(Goal164TestKit.Canonical(laneA.AcceptedMechanics),
            Goal164TestKit.Canonical(build.AcceptedMechanics));
        Assert.Equal(laneA.CompatibilityActivatedPackageSha256,
            laneA.AcceptedMechanics?.QualificationPackageSha256);
    }

    [Fact]
    public void Behavioral_current_rebuild_upgrades_history_without_source_rewrite()
    {
        var fixture = Goal164TestKit.AllSelectable;
        var generationRoot = Path.Combine(fixture.Project.Path,
            SeededGeneratedProjectVocabulary.GenerationRelativeRoot.Replace('/', Path.DirectorySeparatorChar));

        Assert.All(fixture.GenerationSidecarHashesBefore, pair =>
            Assert.Equal(pair.Value, Goal164TestKit.FileSha(Path.Combine(generationRoot, pair.Key))));
        Assert.Equal(GameProjectBuildHistoryReader.SchemaVersionV7,
            History(fixture.Build.BuildHistoryPath).SchemaVersion);
    }

    private static Goal164V3Copy V3Copy()
    {
        var source = Goal164TestKit.AllSelectable;
        var project = Goal156TestKit.Copy(source.Project, "goal164-v3-history");
        var historyRoot = Path.Combine(project.Path,
            UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot.Replace('/', Path.DirectorySeparatorChar));
        foreach (var path in Directory.EnumerateFiles(historyRoot, "*.json")) File.Delete(path);
        var entry = History(source.Build.BuildHistoryPath) with
        {
            SchemaVersion = GameProjectBuildHistoryReader.SchemaVersionV3,
            FinalStateHash = History(source.Build.BuildHistoryPath).GeneratedRegionTravel!.FinalStateHash,
            GeneratedEncounterCombat = null
        };
        File.WriteAllText(Path.Combine(historyRoot, "genuine-v3.json"),
            JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = true }));
        var authoringPath = Directory.EnumerateFiles(Path.Combine(project.Path,
                UnifiedGameProjectWorkspaceVocabulary.AuthoringRelativeRoot.Replace('/', Path.DirectorySeparatorChar)),
            "*" + FeatureModuleCompositionDocumentVocabulary.FileExtension).Single();
        var authoring = Authoring(project.Path) with { LastQualifiedFinalStateHash = entry.FinalStateHash };
        File.WriteAllText(authoringPath,
            JsonSerializer.Serialize(authoring, new JsonSerializerOptions { WriteIndented = true }));
        return new Goal164V3Copy(project);
    }

    private static FeatureModuleCompositionDocument Authoring(string project) =>
        JsonSerializer.Deserialize<FeatureModuleCompositionDocument>(File.ReadAllText(
            Directory.EnumerateFiles(Path.Combine(project,
                    UnifiedGameProjectWorkspaceVocabulary.AuthoringRelativeRoot.Replace('/', Path.DirectorySeparatorChar)),
                "*" + FeatureModuleCompositionDocumentVocabulary.FileExtension).Single()),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    private static GameProjectBuildHistoryEntry History(string path) =>
        JsonSerializer.Deserialize<GameProjectBuildHistoryEntry>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
}

internal sealed record Goal164V3Copy(GeneratedProject Project) : IDisposable
{
    public void Dispose() => Project.Dispose();
}
