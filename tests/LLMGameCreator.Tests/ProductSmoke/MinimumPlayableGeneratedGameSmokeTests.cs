using System.Text.Json;
using LLMGameCreator.Application.Design.MinimumPlayableGame;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class MinimumPlayableGeneratedGameSmokeTests
{
    [Fact]
    public async Task MinimumPlayableGeneratedGameProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), projectRoot);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(projectRoot, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var service = new MinimumPlayableGeneratedGameAcceptanceService();

        var result = service.BuildFromAcceptedEvidence(
            projectRoot,
            content,
            assets,
            new MinimumPlayableGeneratedGameOptions
            {
                RepositoryRootPath = repoRoot,
                ExecuteUnityBuild = true,
                LaunchReviewPackageSmoke = true
            });
        var write = await service.WriteAsync(projectRoot, result);

        Assert.True(File.Exists(write.ManifestJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        Assert.True(File.Exists(write.ManualChecklistPath));

        var report = JsonSerializer.Deserialize<MinimumPlayableGeneratedGameReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(report.Accepted);
        Assert.Equal(MinimumPlayableGeneratedGameAcceptanceService.FinalGate, report.FinalStatus);
        Assert.Equal(MinimumPlayableGeneratedGameAcceptanceService.FinalGate, report.ManualGate);
        Assert.Equal("minimum-playable-generated-game", report.ProductSmokeRoute);
        Assert.True(report.ReviewPackageCreated, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(report.ReviewPackageVerified, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(report.ExecutablePresent);
        Assert.True(report.DataFolderPresent);
        Assert.True(report.StreamingAssetsPayloadVerified);
        Assert.True(report.AutomatedLaunchVerified, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(report.AutomatedQuestCompletionVerified, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(report.ReadablePresentationVerified);
        Assert.True(report.ManualChecklistWritten);
        Assert.True(report.ManualReviewRequired);
        Assert.False(report.RuntimePreviewDependency);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.True(report.NoExternalProviderLlmRagLuaMedia);

        var reviewPackage = Path.Combine(projectRoot, ".llmgc", "procedural", "minimum-playable-generated-game", "review-package");
        Assert.True(File.Exists(Path.Combine(reviewPackage, "LLMGameCreatorAlpha.exe")));
        Assert.True(Directory.Exists(Path.Combine(reviewPackage, "LLMGameCreatorAlpha_Data")));
        Assert.True(File.Exists(Path.Combine(reviewPackage, "README_PLAY.md")));
        Assert.True(File.Exists(Path.Combine(reviewPackage, "RUN_MANUAL_PLAY.ps1")));
        var automatedSmokeScriptPath = Path.Combine(reviewPackage, "RUN_AUTOMATED_SMOKE.ps1");
        Assert.True(File.Exists(automatedSmokeScriptPath));
        Assert.True(File.Exists(Path.Combine(reviewPackage, "MANUAL_PLAY_REVIEW_CHECKLIST.md")));
        Assert.True(File.Exists(Path.Combine(reviewPackage, "generated-scenario-summary.json")));

        var automatedSmokeScript = await File.ReadAllTextAsync(automatedSmokeScriptPath);
        Assert.Contains("Start-Process -FilePath \".\\LLMGameCreatorAlpha.exe\" -ArgumentList $arguments -Wait -PassThru", automatedSmokeScript, StringComparison.Ordinal);
        Assert.Contains("$process.ExitCode", automatedSmokeScript, StringComparison.Ordinal);
        Assert.DoesNotContain("$LASTEXITCODE", automatedSmokeScript, StringComparison.Ordinal);
        Assert.Contains("alpha_runtime.launch_completed=true", automatedSmokeScript, StringComparison.Ordinal);
        Assert.Contains("alpha_runtime.quest_loop_completed=true", automatedSmokeScript, StringComparison.Ordinal);
        Assert.Contains("alpha_runtime.reward_granted.after=true", automatedSmokeScript, StringComparison.Ordinal);
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
