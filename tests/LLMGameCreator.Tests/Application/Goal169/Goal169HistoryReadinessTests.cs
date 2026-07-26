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
using LLMGameCreator.Tests.Application.Goal168;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169HistoryReadinessTests
{
    [Fact]
    public void Behavioral_v7_history_is_regional_events_current()
    {
        var build = Goal168TestKit.Build;
        var history = Read(build.BuildHistoryPath);

        Assert.Equal(GameProjectBuildHistoryReader.SchemaVersionV7,
            history.SchemaVersion);
        Assert.Equal("REGIONAL_EVENTS_CURRENT",
            history.GeneratedCampaignRegionalEvents?.Status);
        Assert.Equal(
            history.GeneratedCampaignRegionalEvents?.FinalStateHash,
            history.FinalStateHash);
    }

    [Fact]
    public void Behavioral_v7_history_keeps_relationship_matrix_exact()
    {
        var build = Goal168TestKit.Build;
        var relationships = build.GeneratedCampaignRelationships!;
        var events = build.GeneratedCampaignRegionalEvents!;

        Assert.Equal(relationships.RelationshipBranchMatrixSha256,
            events.RelationshipBranchMatrixSha256);
        Assert.Equal(build.PackageSha256,
            relationships.ExactPackageSha256);
        Assert.Equal(build.PackageSha256,
            events.ExactPackageSha256);
    }

    [Fact]
    public void Behavioral_genuine_v6_keeps_relationships_current()
    {
        var snapshot = Goal169V6State.Value.Snapshot;

        Assert.Equal("RELATIONSHIPS_CURRENT",
            snapshot.GeneratedCampaignRelationships?.Status);
    }

    [Fact]
    public void Behavioral_genuine_v6_projects_regional_events_pending()
    {
        var snapshot = Goal169V6State.Value.Snapshot;

        Assert.Equal("REGIONAL_EVENTS_PENDING",
            snapshot.GeneratedCampaignRegionalEvents?.Status);
        Assert.Equal("REGIONAL_EVENTS_PENDING",
            snapshot.GeneratedWorld?.Status);
    }

    [Fact]
    public void Behavioral_genuine_v6_is_project_not_ready()
    {
        var capture = Goal169V6State.Value.Capture;

        Assert.Equal(GeneratedCampaignSessionStatus.PROJECT_NOT_READY,
            capture.Status);
        Assert.Contains(
            "campaign.generated_regional_events_not_current",
            capture.Diagnostics);
    }

    [Fact]
    public void Behavioral_genuine_v6_offers_build_upgrade()
    {
        var presentation = Goal162ProjectsTestKit.Presentation(
            Goal169V6State.Value.Snapshot);

        Assert.True(presentation.Enabled);
        Assert.False(presentation.Current);
        Assert.Equal("Собрать и играть", presentation.Title);
    }

    [Fact]
    public void Behavioral_one_build_upgrades_v6_to_v7_without_source_rewrite()
    {
        var state = Goal169V6State.Value;
        var build = Goal168TestKit.Build;

        Assert.True(build.Passed,
            string.Join(",", build.Diagnostics));
        Assert.True(
            build.GeneratedCampaignRegionalEvents is
            { Status: "REGIONAL_EVENTS_CURRENT" },
            Goal164TestKit.Canonical(new
            {
                build.Status,
                build.Passed,
                build.Diagnostics,
                generatedWorld =
                    build.GeneratedWorld?.Status,
                relationships =
                    build.GeneratedCampaignRelationships
                        ?.Status,
                regionalEvents =
                    build.GeneratedCampaignRegionalEvents
                        ?.Status
            }));
        Assert.True(state.SourceRecordUnchanged);
    }

    [Fact]
    public void Behavioral_v6_legacy_full_green_is_compatible()
    {
        Assert.True(Goal169HistoryCompatibilityFixture
            .LegacyAllBranchCompatible);
    }

    [Fact]
    public void Behavioral_v6_legacy_partial_false_is_rejected()
    {
        Assert.True(Goal169HistoryCompatibilityFixture
            .LegacyPartialRejected);
    }

    private static GameProjectBuildHistoryEntry Read(string path) =>
        JsonSerializer.Deserialize<GameProjectBuildHistoryEntry>(
            File.ReadAllText(path),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
}

internal static class Goal169V6State
{
    private static readonly Lazy<Goal169V6Fixture> Fixture =
        new(Create);

    internal static Goal169V6Fixture Value => Fixture.Value;

    private static Goal169V6Fixture Create()
    {
        var source = Goal168TestKit.Real;
        var project = Goal156TestKit.Copy(source.Project,
            "goal169-genuine-v6");
        var historyRoot = Path.Combine(project.Path,
            UnifiedGameProjectWorkspaceVocabulary
                .BuildHistoryRelativeRoot.Replace('/',
                    Path.DirectorySeparatorChar));
        foreach (var path in Directory.EnumerateFiles(
                     historyRoot, "*.json"))
            File.Delete(path);
        var v7 = JsonSerializer.Deserialize<
            GameProjectBuildHistoryEntry>(
            File.ReadAllText(source.Build.BuildHistoryPath),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
        var currentRelationships =
            v7.GeneratedCampaignRelationships!;
        var legacyRelationships = currentRelationships with
        {
            BranchQualifications = [],
            RelationshipBranchMatrixSha256 = string.Empty,
            SaveContinuationFactsPassed = true,
            SaveContinuationFactsEvaluationStatus = "EVALUATED"
        };
        var v6 = v7 with
        {
            SchemaVersion =
                GameProjectBuildHistoryReader.SchemaVersionV6,
            FinalStateHash = legacyRelationships.FinalStateHash,
            GeneratedCampaignRelationships = legacyRelationships,
            GeneratedCampaignRegionalEvents = null
        };
        File.WriteAllText(Path.Combine(historyRoot,
                "genuine-v6.json"),
            JsonSerializer.Serialize(v6,
                new JsonSerializerOptions { WriteIndented = true }));

        var authoringRoot = Path.Combine(project.Path,
            UnifiedGameProjectWorkspaceVocabulary.AuthoringRelativeRoot
                .Replace('/', Path.DirectorySeparatorChar));
        var authoringPath = Directory.EnumerateFiles(authoringRoot,
                "*" + FeatureModuleCompositionDocumentVocabulary
                    .FileExtension)
            .Single();
        var authoring = JsonSerializer.Deserialize<
            FeatureModuleCompositionDocument>(
            File.ReadAllText(authoringPath),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })! with
        {
            LastQualifiedFinalStateHash = v6.FinalStateHash
        };
        File.WriteAllText(authoringPath,
            JsonSerializer.Serialize(authoring,
                new JsonSerializerOptions { WriteIndented = true }));

        var controller =
            Goal156TestKit.OpenWorkspace(project.Path);
        var snapshot = controller.Snapshot();
        var current = new CurrentGamePackageService(
            Goal156TestKit.Repository);
        current.LoadAsync(project.Path, CancellationToken.None)
            .GetAwaiter().GetResult();
        var saves = Goal161ServiceBundle.Create(
            source: Goal156TestKit.SourceService);
        var capture = new GeneratedCampaignSessionTruthService(
            current, saves.Validator, saves.Coordinator).Capture();
        var generationRoot = Path.Combine(
            Goal168TestKit.Real.Project.Path,
            SeededGeneratedProjectVocabulary.GenerationRelativeRoot
                .Replace('/', Path.DirectorySeparatorChar));
        var sourceUnchanged = Goal168TestKit.Real
            .GenerationSidecarHashesBefore.All(pair =>
                pair.Value == Goal164TestKit.FileSha(
                    Path.Combine(generationRoot, pair.Key)));
        return new Goal169V6Fixture(project, snapshot, capture,
            sourceUnchanged);
    }
}

internal sealed record Goal169V6Fixture(
    GeneratedProject Project,
    UnifiedGameProjectWorkspaceSnapshot Snapshot,
    (GeneratedCampaignSessionStatus Status,
        GeneratedCampaignProjectTruth? Truth,
        IReadOnlyList<string> Diagnostics) Capture,
    bool SourceRecordUnchanged);
