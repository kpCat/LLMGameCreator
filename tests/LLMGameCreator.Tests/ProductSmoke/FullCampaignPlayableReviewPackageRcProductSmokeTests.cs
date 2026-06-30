using LLMGameCreator.Application.Design.FullCampaignPlayableReviewPackageRc;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class FullCampaignPlayableReviewPackageRcProductSmokeTests
{
    [Fact]
    public async Task Goal061FullCampaignPlayableReviewPackageRcEvidenceIsProducedForReview()
    {
        var service = new FullCampaignPlayableReviewPackageRcEvidenceService();
        var write = await service.BuildAndWriteAsync(
            ProjectRoot(),
            new FullCampaignPlayableReviewPackageRcOptions
            {
                ExecuteUnityProof = true,
                CleanupUnityWorkProject = true,
                UnityBuildTimeoutSeconds = 900,
                PlayerLaunchTimeoutSeconds = 120
            });

        Assert.True(write.Result.SourceManifest.Goal060AcceptedByUserHandoff);
        Assert.True(write.Result.ReviewPackageManifest.Passed);
        Assert.True(write.Result.FileInventory.Passed);
        Assert.True(write.Result.PackageRowSelectionMatrix.Passed);
        Assert.True(write.Result.PackageMediaBindingAudit.Passed);
        Assert.True(write.Result.SaveLoadReplayAudit.Passed);
        Assert.True(write.Result.ScriptManifest.Passed);
        Assert.True(write.Result.InvalidMatrix.Passed);
        Assert.Equal(9, write.Result.PackageRowSelectionMatrix.RowCount);
        Assert.Equal(9, write.Result.ReviewPackageManifest.PhysicalPackageCount);
        Assert.True(write.Result.UnityCommandPlan.Passed);
        Assert.Contains(write.Result.Report.ImplementationStatus, new[] { "GREEN", "BLOCKED" });
        if (write.Result.Report.ImplementationStatus == "GREEN")
        {
            Assert.True(write.Result.UnityPlayerProof.Passed);
            Assert.Equal(0, write.Result.UnityPlayerProof.UnityExitCode);
            Assert.Equal(0, write.Result.UnityPlayerProof.PlayerExitCode);
            Assert.Equal(9, write.Result.UnityPlayerProof.ProvenRowCount);
            Assert.Empty(write.Result.UnityPlayerProof.MissingMarkers);
        }
        else
        {
            Assert.False(write.Result.UnityPlayerProof.Passed);
            Assert.NotEmpty(write.Result.UnityPlayerProof.Diagnostics);
        }

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, FullCampaignPlayableReviewPackageRcEvidenceService.ArtifactScopeReportJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "review-package", "README.md")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "review-package", "RUN_MANUAL.ps1")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "review-package", "RUN_AUTOMATED_SMOKE.ps1")));
        Assert.Equal(9, Directory.EnumerateFiles(Path.Combine(write.OutputDirectoryPath, "review-package", "p"), "*.json", SearchOption.TopDirectoryOnly).Count());
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
