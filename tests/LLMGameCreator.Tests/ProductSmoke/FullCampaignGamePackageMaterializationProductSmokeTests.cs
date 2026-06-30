using LLMGameCreator.Application.Design.FullCampaignGamePackageMaterialization;
using LLMGameCreator.Tests.Application.FullCampaignGamePackageMaterialization;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class FullCampaignGamePackageMaterializationProductSmokeTests
{
    [Fact]
    public async Task Goal060FullCampaignGamePackageMaterializationEvidenceIsProducedForReview()
    {
        var service = FullCampaignGamePackageMaterializationTestFactory.CreateService();
        var write = await service.BuildAndWriteAsync(
            ProjectRoot(),
            new FullCampaignGamePackageMaterializationOptions
            {
                ExecuteUnityProof = true,
                CleanupUnityWorkProject = true,
                UnityBuildTimeoutSeconds = 900,
                PlayerLaunchTimeoutSeconds = 120
            });

        Assert.True(write.Result.PackageValidationMatrix.Passed);
        Assert.True(write.Result.RuntimeConsumptionMatrix.Passed);
        Assert.True(write.Result.PreviewExportPackagePayloads.Passed);
        Assert.True(write.Result.InvalidMatrix.Passed);
        Assert.Equal(9, write.Result.Packages.Count);
        Assert.Equal(9, write.Result.PackageValidationMatrix.ValidPackageCount);
        Assert.Equal(3, write.Result.RuntimeConsumptionMatrix.RuntimePassedFamilyCount);
        Assert.True(write.Result.UnityCommandPlan.Passed);
        Assert.Contains(write.Result.Report.ImplementationStatus, new[] { "GREEN", "BLOCKED" });
        if (write.Result.Report.ImplementationStatus == "GREEN")
        {
            Assert.True(write.Result.UnityPlayerProof.Passed);
            Assert.Equal(0, write.Result.UnityPlayerProof.UnityExitCode);
            Assert.Equal(0, write.Result.UnityPlayerProof.PlayerExitCode);
            Assert.Empty(write.Result.UnityPlayerProof.MissingMarkers);
        }
        else
        {
            Assert.False(write.Result.UnityPlayerProof.Passed);
            Assert.NotEmpty(write.Result.UnityPlayerProof.Diagnostics);
        }

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, FullCampaignGamePackageMaterializationEvidenceService.ArtifactScopeReportJsonFileName)));
        Assert.All(write.Result.Packages, package =>
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, package.PackageRelativePath.Replace('/', Path.DirectorySeparatorChar))));
        });
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
