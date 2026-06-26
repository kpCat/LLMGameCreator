using System.Text.Json;
using LLMGameCreator.Application.Design.UnityReadablePresentation;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityReadablePresentation;

[Collection("UnityAlphaProductSmoke")]
public sealed class UnityAlphaReadablePresentationAcceptanceTests
{
    [Fact]
    public async Task BuildsDeterministicReadablePresentationArtifacts()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);
        var service = new UnityAlphaReadablePresentationAcceptanceService();

        var first = service.BuildFromAcceptedEvidence(temp.Path, content, assets, new UnityAlphaReadablePresentationOptions { RepositoryRootPath = repoRoot });
        var second = service.BuildFromAcceptedEvidence(temp.Path, content, assets, new UnityAlphaReadablePresentationOptions { RepositoryRootPath = repoRoot });
        var write = await service.WriteAsync(temp.Path, first);

        Assert.False(first.Report.Accepted);
        Assert.Equal(UnityAlphaReadablePresentationAcceptanceService.FinalGate, first.Report.FinalStatus);
        Assert.Equal(UnityAlphaReadablePresentationAcceptanceService.FinalGate, first.Report.ManualGate);
        Assert.Equal("unity_generated_multi_variant_playable_scenario_verification passed", first.Report.PreviousAcceptedGate);
        Assert.Equal(["S154", "S155", "S156", "S157", "S158", "S159", "S160", "S161"], first.Report.CompletedSlices);
        Assert.Equal("unity-alpha-readable-presentation", first.Report.ProductSmokeRoute);
        Assert.Equal(first.Report.ModelHash, second.Report.ModelHash);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.Report.PresentationModelVerified, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => item.Code)));
        Assert.True(first.Report.InvalidMatrix.Passed, string.Join(Environment.NewLine, first.Report.InvalidMatrix.Diagnostics.Select(item => item.Code)));
        Assert.True(first.Report.InvalidMatrix.ScenarioCount >= 22);
        Assert.False(first.Report.PublicGamePackageSchemaChanged);
        Assert.False(first.Report.ProjectFilesChanged);
        Assert.False(first.Report.GeneratorLibraryChanged);
        Assert.True(first.Report.NoExternalProviderLlmRagLuaMedia);
        Assert.False(first.Report.RuntimePreviewDependency);
        Assert.True(File.Exists(write.ModelJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var roundTrip = JsonSerializer.Deserialize<UnityAlphaReadablePresentationReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(roundTrip);
        Assert.Equal(UnityAlphaReadablePresentationAcceptanceService.FinalGate, roundTrip!.ManualGate);
    }

    [Fact]
    public void ModelContainsThreeVariantCards()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new UnityAlphaReadablePresentationAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new UnityAlphaReadablePresentationOptions { RepositoryRootPath = repoRoot });

        Assert.Equal(3, result.Report.VariantCardCount);
        Assert.Contains(result.Report.PresentationModel.ScenarioCards, card => card.StyleId == "frontier_survival" && card.DisplayName == "Frontier Survival");
        Assert.Contains(result.Report.PresentationModel.ScenarioCards, card => card.StyleId == "gothic_mystery" && card.DisplayName == "Gothic Mystery");
        Assert.Contains(result.Report.PresentationModel.ScenarioCards, card => card.StyleId == "trade_caravan" && card.DisplayName == "Trade Caravan");
    }

    [Fact]
    public void PrimaryLabelsAreReadableAndNotRawIdOnly()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new UnityAlphaReadablePresentationAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new UnityAlphaReadablePresentationOptions { RepositoryRootPath = repoRoot });

        Assert.Equal(0, result.Report.RawIdOnlyLabelCount);
        Assert.True(result.Report.ReadableLabelCount >= 12);
        Assert.StartsWith("Quest ", result.Report.PresentationModel.PrimaryQuestPanel.Title, StringComparison.Ordinal);
        Assert.StartsWith("Reward: item ", result.Report.PresentationModel.RewardPanel.RewardLabel, StringComparison.Ordinal);
    }

    [Fact]
    public void RequiredPanelsAreValidatedFromPlayerLogLines()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);
        var result = new UnityAlphaReadablePresentationAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new UnityAlphaReadablePresentationOptions { RepositoryRootPath = repoRoot });
        var model = result.Report.PresentationModel;
        var lines = UnityAlphaReadablePresentationAcceptanceService.BuildExpectedPresentationLines(model);

        var proof = UnityAlphaReadablePresentationAcceptanceService.ValidatePresentationLines(lines, model);

        Assert.True(proof.PresentationReadable, string.Join(Environment.NewLine, proof.Diagnostics.Select(item => item.Code)));
        Assert.Equal(9, proof.VisiblePanelCount);
    }

    [Fact]
    public void CompletionLoopAndMultiVariantEvidenceRemainVerified()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new UnityAlphaReadablePresentationAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new UnityAlphaReadablePresentationOptions { RepositoryRootPath = repoRoot });

        Assert.True(result.Report.QuestCompletionStillVerified);
        Assert.True(result.Report.MultiVariantEvidenceVerified);
        Assert.True(result.Report.FirewallSafeBuildVerified);
    }

    [Fact]
    public void InvalidMatrixScenariosAreCausalAndRejected()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new UnityAlphaReadablePresentationAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new UnityAlphaReadablePresentationOptions { RepositoryRootPath = repoRoot });

        Assert.True(result.Report.InvalidMatrix.Passed, string.Join(Environment.NewLine, result.Report.InvalidMatrix.Diagnostics.Select(item => item.Code)));
        Assert.True(result.Report.InvalidMatrix.Scenarios.All(item => !item.ActualValid));
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "presentation_readable_true_without_required_panels");
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "raw_id_only_primary_quest_label");
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "objective_labels_not_tied_to_goal017_objective_ids");
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "development_profiler_debug_build_option_reintroduced");
    }

    [Fact]
    public void MissingGoal018EvidenceRejects()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new UnityAlphaReadablePresentationAcceptanceService()
            .BuildFromAcceptedEvidence(
                temp.Path,
                content,
                assets,
                new UnityAlphaReadablePresentationOptions { RepositoryRootPath = temp.Path });

        Assert.Contains(result.Report.Diagnostics, item => item.Code == "unity_readable_presentation.previous.variants_missing");
        Assert.False(result.Report.MultiVariantEvidenceVerified);
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
