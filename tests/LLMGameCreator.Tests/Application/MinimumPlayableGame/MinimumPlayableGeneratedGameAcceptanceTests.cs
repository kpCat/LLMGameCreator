using System.Text.Json;
using LLMGameCreator.Application.Design.MinimumPlayableGame;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.Application.MinimumPlayableGame;

[Collection("UnityAlphaProductSmoke")]
public sealed class MinimumPlayableGeneratedGameAcceptanceTests
{
    [Fact]
    public async Task BuildsDeterministicManifestReportAndChecklistArtifacts()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);
        var service = new MinimumPlayableGeneratedGameAcceptanceService();

        var first = service.BuildFromAcceptedEvidence(temp.Path, content, assets, new MinimumPlayableGeneratedGameOptions { RepositoryRootPath = repoRoot });
        var second = service.BuildFromAcceptedEvidence(temp.Path, content, assets, new MinimumPlayableGeneratedGameOptions { RepositoryRootPath = repoRoot });
        var write = await service.WriteAsync(temp.Path, first);

        Assert.False(first.Report.Accepted);
        Assert.Equal(MinimumPlayableGeneratedGameAcceptanceService.FinalGate, first.Report.FinalStatus);
        Assert.Equal(MinimumPlayableGeneratedGameAcceptanceService.FinalGate, first.Report.ManualGate);
        Assert.Equal("unity_alpha_readable_presentation_verification passed", first.Report.PreviousAcceptedGate);
        Assert.Equal(["S162", "S163", "S164", "S165", "S166", "S167", "S168", "S169"], first.Report.CompletedSlices);
        Assert.Equal("minimum-playable-generated-game", first.Report.ProductSmokeRoute);
        Assert.Equal(first.Report.ManifestHash, second.Report.ManifestHash);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(File.Exists(write.ManifestJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        Assert.True(File.Exists(write.ManualChecklistPath));
    }

    [Fact]
    public void CreatesReviewPackageWithRequiredFiles()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new MinimumPlayableGeneratedGameAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new MinimumPlayableGeneratedGameOptions { RepositoryRootPath = repoRoot });
        var packageRoot = Path.Combine(temp.Path, ".llmgc", "procedural", "minimum-playable-generated-game", "review-package");

        Assert.True(result.Report.ReviewPackageCreated, string.Join(Environment.NewLine, result.Report.Diagnostics.Select(item => item.Code)));
        Assert.True(File.Exists(Path.Combine(packageRoot, "LLMGameCreatorAlpha.exe")));
        Assert.True(Directory.Exists(Path.Combine(packageRoot, "LLMGameCreatorAlpha_Data")));
        Assert.True(File.Exists(Path.Combine(packageRoot, "README_PLAY.md")));
        Assert.True(File.Exists(Path.Combine(packageRoot, "RUN_MANUAL_PLAY.ps1")));
        Assert.True(File.Exists(Path.Combine(packageRoot, "RUN_AUTOMATED_SMOKE.ps1")));
        Assert.True(File.Exists(Path.Combine(packageRoot, "MANUAL_PLAY_REVIEW_CHECKLIST.md")));
        Assert.True(File.Exists(Path.Combine(packageRoot, "generated-scenario-summary.json")));
    }

    [Fact]
    public void ReviewPackageManifestHashesMatchPhysicalFiles()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new MinimumPlayableGeneratedGameAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new MinimumPlayableGeneratedGameOptions { RepositoryRootPath = repoRoot });

        Assert.True(result.Manifest.FileCount > 0);
        Assert.True(result.Manifest.TotalByteCount > 0);
        Assert.Equal(result.Manifest.ReviewPackageHash, result.Report.ReviewPackageHash);
        Assert.Equal(result.Manifest.ManifestHash, result.Report.ManifestHash);
        Assert.False(string.IsNullOrWhiteSpace(result.Manifest.PackageHash));
        Assert.False(string.IsNullOrWhiteSpace(result.Manifest.AssetManifestHash));
        Assert.False(string.IsNullOrWhiteSpace(result.Manifest.BuildManifestHash));
    }

    [Fact]
    public void ScriptsArePackageRelativeAndDoNotContainAbsolutePaths()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        new MinimumPlayableGeneratedGameAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new MinimumPlayableGeneratedGameOptions { RepositoryRootPath = repoRoot });
        var packageRoot = Path.Combine(temp.Path, ".llmgc", "procedural", "minimum-playable-generated-game", "review-package");
        var manual = File.ReadAllText(Path.Combine(packageRoot, "RUN_MANUAL_PLAY.ps1"));
        var automated = File.ReadAllText(Path.Combine(packageRoot, "RUN_AUTOMATED_SMOKE.ps1"));

        Assert.Contains(".\\LLMGameCreatorAlpha.exe", manual, StringComparison.Ordinal);
        Assert.Contains(".\\LLMGameCreatorAlpha.exe", automated, StringComparison.Ordinal);
        Assert.DoesNotContain("C:\\", manual, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:\\", automated, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Users\\", manual, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Users\\", automated, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AutomatedSmokeProofVerifiesLaunchAndQuestCompletion()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new MinimumPlayableGeneratedGameAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new MinimumPlayableGeneratedGameOptions { RepositoryRootPath = repoRoot });

        Assert.True(result.Report.AutomatedLaunchVerified, string.Join(Environment.NewLine, result.Report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(result.Report.AutomatedQuestCompletionVerified, string.Join(Environment.NewLine, result.Report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(result.Report.ReadablePresentationVerified);
    }

    [Fact]
    public void ManualChecklistIsNotPreMarkedPassed()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new MinimumPlayableGeneratedGameAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new MinimumPlayableGeneratedGameOptions { RepositoryRootPath = repoRoot });

        Assert.True(result.Report.ManualChecklistWritten);
        Assert.Contains("- [ ] Player launched from review package.", result.ManualChecklistMarkdown, StringComparison.Ordinal);
        Assert.DoesNotContain("[x]", result.ManualChecklistMarkdown, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InvalidMatrixScenariosAreCausalAndRejected()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new MinimumPlayableGeneratedGameAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new MinimumPlayableGeneratedGameOptions { RepositoryRootPath = repoRoot });

        Assert.True(result.Report.InvalidMatrix.Passed, string.Join(Environment.NewLine, result.Report.InvalidMatrix.Diagnostics.Select(item => item.Code)));
        Assert.True(result.Report.InvalidMatrix.ScenarioCount >= 24);
        Assert.True(result.Report.InvalidMatrix.Scenarios.All(item => !item.ActualValid));
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "missing_executable");
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "review_package_hash_mismatch");
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "automated_quest_completion_claim_without_play_loop_log");
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "runtime_preview_dependency_claim");
    }

    [Fact]
    public void MissingGoal019EvidenceRejects()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var (content, assets) = BuildInputs(repoRoot, temp.Path);

        var result = new MinimumPlayableGeneratedGameAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, content, assets, new MinimumPlayableGeneratedGameOptions { RepositoryRootPath = temp.Path });

        Assert.Contains(result.Report.Diagnostics, item => item.Code == "minimum_playable_game.previous.report_missing");
        Assert.False(result.Report.MinimumPlayableGeneratedGameVerified);
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
