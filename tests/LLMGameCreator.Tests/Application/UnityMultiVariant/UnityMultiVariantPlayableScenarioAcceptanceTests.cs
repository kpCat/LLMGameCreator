using System.Text.Json;
using LLMGameCreator.Application.Design.AlphaBuild;
using LLMGameCreator.Application.Design.UnityGeneratedScene;
using LLMGameCreator.Application.Design.UnityMultiVariant;
using LLMGameCreator.Application.Design.UnityQuestLoop;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityMultiVariant;

public sealed class UnityMultiVariantPlayableScenarioAcceptanceTests
{
    private static readonly string[] ExpectedStyles = ["frontier_survival", "gothic_mystery", "trade_caravan"];

    [Fact]
    public async Task BuildsDeterministicMultiVariantArtifacts()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);
        var service = new UnityMultiVariantPlayableScenarioAcceptanceService();

        var first = service.BuildFromAcceptedEvidence(temp.Path, content, assets, new UnityMultiVariantPlayableScenarioOptions { RepositoryRootPath = repoRoot });
        var second = service.BuildFromAcceptedEvidence(temp.Path, content, assets, new UnityMultiVariantPlayableScenarioOptions { RepositoryRootPath = repoRoot });
        var write = await service.WriteAsync(temp.Path, first);

        Assert.False(first.Report.Accepted);
        Assert.Equal(UnityMultiVariantPlayableScenarioAcceptanceService.FinalGate, first.Report.FinalStatus);
        Assert.Equal(UnityMultiVariantPlayableScenarioAcceptanceService.FinalGate, first.Report.ManualGate);
        Assert.Equal("unity_generated_quest_completion_loop_verification passed", first.Report.PreviousAcceptedGate);
        Assert.Equal(["S146", "S147", "S148", "S149", "S150", "S151", "S152", "S153"], first.Report.CompletedSlices);
        Assert.Equal("unity-multi-variant-playable-scenario", first.Report.ProductSmokeRoute);
        Assert.Equal(3, first.Report.VariantCount);
        Assert.Equal(ExpectedStyles, first.Report.SelectedStyleIds);
        Assert.Equal(first.Report.VariantsHash, second.Report.VariantsHash);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.Report.InvalidMatrix.Passed, string.Join(Environment.NewLine, first.Report.InvalidMatrix.Diagnostics.Select(item => item.Code)));
        Assert.True(first.Report.InvalidMatrix.ScenarioCount >= 24);
        Assert.False(first.Report.PublicGamePackageSchemaChanged);
        Assert.False(first.Report.ProjectFilesChanged);
        Assert.False(first.Report.GeneratorLibraryChanged);
        Assert.True(first.Report.NoExternalProviderLlmRagLuaMedia);
        Assert.False(first.Report.RuntimePreviewDependency);
        Assert.True(File.Exists(write.VariantsJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var roundTrip = JsonSerializer.Deserialize<UnityMultiVariantPlayableScenarioReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(roundTrip);
        Assert.Equal(UnityMultiVariantPlayableScenarioAcceptanceService.FinalGate, roundTrip!.ManualGate);
    }

    [Fact]
    public void SelectsAtLeastThreeExpectedStyles()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new UnityMultiVariantPlayableScenarioAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new UnityMultiVariantPlayableScenarioOptions { RepositoryRootPath = repoRoot });

        Assert.Equal(3, result.Report.DistinctStyleCount);
        Assert.Equal(ExpectedStyles, result.Report.SelectedStyleIds);
        Assert.All(ExpectedStyles, styleId => Assert.Contains(result.Report.VariantSummaries, item => item.StyleId == styleId));
    }

    [Fact]
    public void EachVariantPassesQuestCompletionLoopValidationForGeneratedLines()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        foreach (var styleId in ExpectedStyles)
        {
            var alpha = new AlphaRunnableBuildAcceptanceService()
                .BuildFromAcceptedEvidence(
                    temp.Path,
                    content,
                    assets,
                    new AlphaRunnableBuildOptions
                    {
                        RepositoryRootPath = repoRoot,
                        RelativeOutputDirectoryOverride = UnityMultiVariantPlayableScenarioAcceptanceService.RelativeOutputDirectory + "/test-" + styleId,
                        SelectedStyleId = styleId
                    })
                .Report;
            var projection = UnityGeneratedSceneProjectionAcceptanceService.BuildProjection(alpha);
            var plan = UnityQuestCompletionLoopAcceptanceService.BuildPlan(projection);
            var lines = UnityQuestCompletionLoopAcceptanceService.BuildExpectedQuestLoopLines(projection, plan);

            var proof = UnityQuestCompletionLoopAcceptanceService.ValidateQuestLoopLines(lines, projection, plan);

            Assert.True(proof.QuestCompletionLoopVerified, string.Join(Environment.NewLine, proof.Diagnostics.Select(item => item.Code)));
            Assert.True(proof.QuestCompletedVerified);
            Assert.True(proof.RewardGrantedVerified);
        }
    }

    [Fact]
    public void CrossVariantDistinctnessRejectsRepeatedPackageQuestSceneAndObjectiveSignatures()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new UnityMultiVariantPlayableScenarioAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new UnityMultiVariantPlayableScenarioOptions { RepositoryRootPath = repoRoot });

        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "only_one_variant_repeated_three_times" && !item.ActualValid);
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "three_variants_with_same_package_id" && !item.ActualValid);
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "three_variants_with_same_quest_id" && !item.ActualValid);
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "three_variants_with_same_scene_signature" && !item.ActualValid);
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "three_variants_with_same_objective_signature" && !item.ActualValid);
    }

    [Fact]
    public void InvalidMatrixScenariosAreCausalAndRejected()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new UnityMultiVariantPlayableScenarioAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new UnityMultiVariantPlayableScenarioOptions { RepositoryRootPath = repoRoot });

        Assert.True(result.Report.InvalidMatrix.Passed, string.Join(Environment.NewLine, result.Report.InvalidMatrix.Diagnostics.Select(item => item.Code)));
        Assert.True(result.Report.InvalidMatrix.Scenarios.All(item => !item.ActualValid));
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "objective_command_id_mismatch");
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "cross_style_command_leakage");
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "player_log_copied_from_another_variant");
    }

    [Fact]
    public void UnknownStyleIdSelectionRejects()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new UnityMultiVariantPlayableScenarioAcceptanceService()
            .BuildFromAcceptedEvidence(
                temp.Path,
                content,
                assets,
                new UnityMultiVariantPlayableScenarioOptions
                {
                    RepositoryRootPath = repoRoot,
                    SelectedStyleIds = ["frontier_survival", "gothic_mystery", "unknown_style"]
                });

        Assert.Contains(result.Report.Diagnostics, item => item.Code == "unity_multi_variant.selection.unknown_style_id");
        Assert.False(result.Report.MultiVariantPlayableScenarioVerified);
    }

    [Fact]
    public void PreviousGoal017EvidenceMustBePresentAndMatching()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new UnityMultiVariantPlayableScenarioAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new UnityMultiVariantPlayableScenarioOptions { RepositoryRootPath = repoRoot });

        Assert.Contains(result.Report.Diagnostics, item => item.Code == "unity_multi_variant.previous.goal017_evidence_present");
        Assert.Equal("unity_generated_quest_completion_loop_verification passed", result.Report.PreviousAcceptedGate);
        Assert.Equal(UnityMultiVariantPlayableScenarioAcceptanceService.FinalGate, result.Report.ManualGate);
    }

    private static (
        LLMGameCreator.Application.Design.ContentGeneration.ContentGenerationScaleAcceptanceResult Content,
        LLMGameCreator.Application.Design.Assets.MinimumAssetPipelineAcceptanceResult Assets) BuildInputs(
            string repoRoot,
            string projectRoot)
    {
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), projectRoot);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(projectRoot, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        return (content, assets);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
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
                Directory.Delete(Path, true);
            }
        }
    }
}
