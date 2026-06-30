using LLMGameCreator.Application.Design.ConstrainedSpatialDetailGeneration;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class ConstrainedSpatialDetailGenerationProductSmokeTests
{
    [Fact]
    public async Task Goal062ConstrainedSpatialDetailGenerationEvidenceIsProducedForReview()
    {
        var service = new ConstrainedSpatialDetailEvidenceService();
        var write = await service.BuildAndWriteAsync(
            ProjectRoot(),
            new ConstrainedSpatialDetailOptions
            {
                ExecuteUnityProof = true,
                CleanupUnityWorkProject = true,
                UnityBuildTimeoutSeconds = 900,
                PlayerLaunchTimeoutSeconds = 120
            });

        Assert.True(write.Result.SourceManifest.Goal061AcceptedByUserHandoff);
        Assert.True(write.Result.PaletteCatalog.Passed);
        Assert.True(write.Result.RewriteRuleCatalog.Passed);
        Assert.True(write.Result.ConstraintRuleCatalog.Passed);
        Assert.True(write.Result.SpatialDetailMatrix.Passed);
        Assert.True(write.Result.ReachabilityProofMatrix.Passed);
        Assert.True(write.Result.RepairFallbackMatrix.Passed);
        Assert.True(write.Result.PreviewExportPayload.Passed);
        Assert.True(write.Result.InvalidMatrix.Passed);
        Assert.True(write.Result.UnityCommandPlan.Passed);
        Assert.Equal(9, write.Result.SpatialDetailRows.Count);
        Assert.Equal(9, write.Result.SpatialDetailMatrix.DistinctRowHashCount);
        Assert.Contains(write.Result.Report.ImplementationStatus, new[] { "GREEN", "BLOCKED" });
        if (write.Result.Report.ImplementationStatus == "GREEN")
        {
            Assert.True(write.Result.UnityProofSummary.Passed);
            Assert.Equal(0, write.Result.UnityProofSummary.UnityExitCode);
            Assert.Equal(0, write.Result.UnityProofSummary.PlayerExitCode);
            Assert.Equal(9, write.Result.UnityProofSummary.ProvenRowCount);
            Assert.Empty(write.Result.UnityProofSummary.MissingMarkers);
        }
        else
        {
            Assert.False(write.Result.UnityProofSummary.Passed);
            Assert.NotEmpty(write.Result.UnityProofSummary.Diagnostics);
        }

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ConstrainedSpatialDetailEvidenceService.ArtifactScopeReportJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ConstrainedSpatialDetailEvidenceService.UnityCommandPlanJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ConstrainedSpatialDetailVocabulary.StagingRoot, ConstrainedSpatialDetailVocabulary.UnitySpatialDetailCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
        Assert.Equal(9, Directory.EnumerateFiles(write.OutputDirectoryPath, "spatial-detail-row-*.json", SearchOption.TopDirectoryOnly).Count());
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
