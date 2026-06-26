using System.Text.Json;
using LLMGameCreator.Application.Design.UnityMultiVariant;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnityMultiVariantPlayableScenarioSmokeTests
{
    [Fact]
    public async Task UnityMultiVariantPlayableScenarioProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), projectRoot);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(projectRoot, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var service = new UnityMultiVariantPlayableScenarioAcceptanceService();

        var result = service.BuildFromAcceptedEvidence(
            projectRoot,
            content,
            assets,
            new UnityMultiVariantPlayableScenarioOptions
            {
                RepositoryRootPath = repoRoot,
                ExecuteUnityBuild = true,
                LaunchBuiltPlayer = true
            });
        var write = await service.WriteAsync(projectRoot, result);

        Assert.True(File.Exists(write.VariantsJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var report = JsonSerializer.Deserialize<UnityMultiVariantPlayableScenarioReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(report.Accepted);
        Assert.Equal(UnityMultiVariantPlayableScenarioAcceptanceService.FinalGate, report.FinalStatus);
        Assert.Equal(UnityMultiVariantPlayableScenarioAcceptanceService.FinalGate, report.ManualGate);
        Assert.Equal("unity_generated_quest_completion_loop_verification passed", report.PreviousAcceptedGate);
        Assert.Equal("unity-multi-variant-playable-scenario", report.ProductSmokeRoute);
        Assert.True(report.VariantCount >= 3);
        Assert.True(report.AcceptedVariantCount >= 3, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(report.AllVariantsQuestComplete);
        Assert.True(report.AllVariantsRewardGranted);
        Assert.True(report.AllVariantsUseSamePipeline);
        Assert.True(report.MultiVariantPlayableScenarioVerified, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.Equal(3, report.DistinctStyleCount);
        Assert.Equal(3, report.DistinctPackageCount);
        Assert.Equal(3, report.DistinctQuestCount);
        Assert.Equal(3, report.DistinctSceneSignatureCount);
        Assert.Equal(3, report.DistinctObjectiveSignatureCount);
        Assert.True(report.InvalidMatrix.Passed, string.Join(Environment.NewLine, report.InvalidMatrix.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.False(report.RuntimePreviewDependency);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.True(report.NoExternalProviderLlmRagLuaMedia);
        Assert.All(report.VariantSummaries, variant =>
        {
            Assert.True(variant.Accepted, string.Join(Environment.NewLine, variant.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
            Assert.True(variant.QuestCompletionLoopVerified);
            Assert.True(variant.QuestCompletedVerified);
            Assert.True(variant.RewardGrantedVerified);
            Assert.Equal(6, variant.ObjectiveIds.Count);
            Assert.False(string.IsNullOrWhiteSpace(variant.PlayerLogRelativePath));
        });
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
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
}
