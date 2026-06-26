using System.Text.Json;
using LLMGameCreator.Application.Design.UnityReadablePresentation;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class UnityReadablePresentationSmokeTests
{
    [Fact]
    public async Task UnityReadablePresentationProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), projectRoot);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(projectRoot, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var service = new UnityAlphaReadablePresentationAcceptanceService();

        var result = service.BuildFromAcceptedEvidence(
            projectRoot,
            content,
            assets,
            new UnityAlphaReadablePresentationOptions
            {
                RepositoryRootPath = repoRoot,
                ExecuteUnityBuild = true,
                LaunchBuiltPlayer = true
            });
        var write = await service.WriteAsync(projectRoot, result);

        Assert.True(File.Exists(write.ModelJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var report = JsonSerializer.Deserialize<UnityAlphaReadablePresentationReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(report.Accepted);
        Assert.Equal(UnityAlphaReadablePresentationAcceptanceService.FinalGate, report.FinalStatus);
        Assert.Equal(UnityAlphaReadablePresentationAcceptanceService.FinalGate, report.ManualGate);
        Assert.Equal("unity_generated_multi_variant_playable_scenario_verification passed", report.PreviousAcceptedGate);
        Assert.Equal("unity-alpha-readable-presentation", report.ProductSmokeRoute);
        Assert.True(report.ReadablePresentationVerified, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(report.PresentationModelVerified);
        Assert.True(report.PresentationPlayerEvidenceVerified, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(report.QuestCompletionStillVerified);
        Assert.True(report.MultiVariantEvidenceVerified);
        Assert.True(report.FirewallSafeBuildVerified);
        Assert.True(report.VisiblePanelCount >= 8);
        Assert.True(report.RequiredPanelCount >= 8);
        Assert.True(report.ReadableLabelCount >= 12);
        Assert.Equal(0, report.RawIdOnlyLabelCount);
        Assert.True(report.ObjectiveLabelCount >= 6);
        Assert.True(report.CompletedObjectiveCount >= 6);
        Assert.True(report.ControlHintCount >= 5);
        Assert.True(report.VariantCardCount >= 3);
        Assert.True(report.InvalidMatrix.Passed, string.Join(Environment.NewLine, report.InvalidMatrix.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.False(report.RuntimePreviewDependency);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.True(report.NoExternalProviderLlmRagLuaMedia);
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
