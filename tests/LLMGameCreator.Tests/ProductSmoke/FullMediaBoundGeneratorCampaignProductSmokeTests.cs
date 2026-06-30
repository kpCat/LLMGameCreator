using System.Text.Json;
using LLMGameCreator.Application.Design.FullMediaBoundGeneratorCampaign;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class FullMediaBoundGeneratorCampaignProductSmokeTests
{
    [Fact]
    public async Task FullMediaBoundGeneratorCampaignProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var outputRoot = ResolveOutputFolder(repoRoot);
        var service = new FullMediaBoundGeneratorCampaignEvidenceService();
        var write = await service.BuildAndWriteAsync(
            outputRoot,
            new FullMediaBoundGeneratorCampaignOptions
            {
                RepositoryRootPath = repoRoot,
                ExecuteUnityProof = true,
                CleanupUnityWorkProject = true,
                UnityBuildTimeoutSeconds = 900,
                PlayerLaunchTimeoutSeconds = 120
            });

        AssertFile(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.SourceManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.CampaignPlanJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.FamilyRunFileName("map_panel_rpg"));
        AssertFile(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.FamilyRunFileName("survival_sandbox"));
        AssertFile(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.FamilyRunFileName("first_person_grid_dungeon"));
        AssertFile(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.ReviewPackageManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.UnityCommandPlanJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.UnityPlayerProofJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.PreviewExportPayloadJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.PackageCompatibilityProofJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.InvalidMatrixJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.ArtifactScopeReportMarkdownFileName);
        Assert.True(File.Exists(write.ReportMarkdownPath));
        AssertFile(write.StagingDirectoryPath, FullMediaBoundGeneratorCampaignVocabulary.CampaignManifestStagingRelativePath);
        AssertFile(write.StagingDirectoryPath, FullMediaBoundGeneratorCampaignVocabulary.CampaignCommandPlanStagingRelativePath);
        AssertFile(write.OutputDirectoryPath, "review-package/StreamingAssets/full-media-bound-campaign-manifest.json");
        AssertFile(write.OutputDirectoryPath, "review-package/StreamingAssets/family-command-plan.json");
        AssertFile(write.OutputDirectoryPath, "review-package/StreamingAssets/media-bound-manifest.json");

        using var sourceManifest = Parse(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.SourceManifestJsonFileName);
        using var campaignPlan = Parse(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.CampaignPlanJsonFileName);
        using var commandPlan = Parse(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.UnityCommandPlanJsonFileName);
        using var proof = Parse(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.UnityPlayerProofJsonFileName);
        using var invalid = Parse(write.OutputDirectoryPath, FullMediaBoundGeneratorCampaignEvidenceService.InvalidMatrixJsonFileName);
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.False(sourceManifest.RootElement.GetProperty("accepted").GetBoolean());
        Assert.True(sourceManifest.RootElement.GetProperty("goal057AcceptedByUserHandoff").GetBoolean());
        Assert.True(campaignPlan.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(commandPlan.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("accepted=false", report);
        Assert.Contains("manualGate=full_media_bound_generator_campaign_verification", report);
        Assert.Contains("goal057AcceptedByUserHandoff=true", report);
        Assert.Contains("sourceFactsConsumed=true", report);
        Assert.Contains("allFamiliesIncluded=true", report);
        Assert.Contains("campaignRunnerExecuted=true", report);
        Assert.Contains("reviewPackageManifestPassed=true", report);
        Assert.Contains("invalidMatrixPassed=true", report);

        var status = ExtractReportValue(report, "implementationStatus");
        Assert.Contains(status, new[] { "GREEN", "BLOCKED" });
        if (status == "GREEN")
        {
            Assert.True(proof.RootElement.GetProperty("passed").GetBoolean());
            Assert.Equal(0, proof.RootElement.GetProperty("unityExitCode").GetInt32());
            Assert.Equal(0, proof.RootElement.GetProperty("playerExitCode").GetInt32());
            Assert.Contains("campaign_loaded=goal058", report);
            Assert.Contains("campaign_media_bound=true", report);
            Assert.Contains("campaign_review_package_proof=goal058", report);
            foreach (var familyId in FullMediaBoundGeneratorCampaignVocabulary.FamilyIds)
            {
                Assert.Contains("campaign_family=" + familyId, report);
                Assert.Contains("campaign_family_completed=" + familyId, report);
            }
        }
        else
        {
            Assert.False(proof.RootElement.GetProperty("passed").GetBoolean());
            Assert.Contains("allCampaignMarkersMatched=false", report);
            Assert.NotEqual(string.Empty, write.Result.Report.Diagnostics.First(item => item.Code.StartsWith("goal058.unity.", StringComparison.Ordinal)).Code);
        }
    }

    private static void AssertFile(string directoryPath, string relativePath) =>
        Assert.True(File.Exists(Path.Combine(directoryPath, relativePath.Replace('/', Path.DirectorySeparatorChar))), "Missing evidence file: " + relativePath);

    private static JsonDocument Parse(string directoryPath, string fileName) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(directoryPath, fileName)));

    private static string ExtractReportValue(string report, string key)
    {
        foreach (var line in report.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
        {
            var prefix = key + "=";
            if (line.StartsWith(prefix, StringComparison.Ordinal))
            {
                return line[prefix.Length..].Trim();
            }
        }

        return string.Empty;
    }

    private static string ResolveOutputFolder(string repoRoot)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var outputFolder = string.IsNullOrWhiteSpace(configured) ? repoRoot : configured;
        Directory.CreateDirectory(outputFolder);
        return outputFolder;
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
