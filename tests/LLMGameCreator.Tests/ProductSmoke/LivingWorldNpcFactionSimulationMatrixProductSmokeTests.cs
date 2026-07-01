using LLMGameCreator.Application.Design.LivingWorldNpcFactionSimulationMatrix;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class LivingWorldNpcFactionSimulationMatrixProductSmokeTests
{
    [Fact]
    public async Task Goal064LivingWorldNpcFactionSimulationMatrixEvidenceIsProducedForReview()
    {
        var service = new LivingWorldNpcFactionSimulationEvidenceService();
        var write = await service.BuildAndWriteAsync(
            ProjectRoot(),
            new LivingWorldNpcFactionSimulationOptions
            {
                ExecuteUnityProof = true,
                CleanupUnityWorkProject = true,
                UnityBuildTimeoutSeconds = 900,
                PlayerLaunchTimeoutSeconds = 120
            });

        Assert.True(write.Result.SourceManifest.Goal063AcceptedByUserHandoff);
        Assert.True(write.Result.CatalogSummary.Passed);
        Assert.True(write.Result.SimulationMatrixPlan.Passed);
        Assert.True(write.Result.SaveLoadReplayProof.Passed);
        Assert.True(write.Result.VarianceMetrics.Passed);
        Assert.True(write.Result.PreviewExportPayload.Passed);
        Assert.True(write.Result.InvalidMatrix.Passed);
        Assert.True(write.Result.UnityCommandPlan.Passed);
        Assert.Equal(9, write.Result.SimulationMatrixPlan.RowCount);
        Assert.Equal(9, write.Result.SimulationMatrixPlan.StateChangingRowCount);
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
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, LivingWorldNpcFactionSimulationEvidenceService.ArtifactScopeReportJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, LivingWorldNpcFactionSimulationVocabulary.StagingRoot, LivingWorldNpcFactionSimulationVocabulary.UnityLivingWorldCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
        Assert.Equal(9, Directory.EnumerateFiles(Path.Combine(write.OutputDirectoryPath, LivingWorldNpcFactionSimulationEvidenceService.RowsDirectoryName), "*-living-world-row.json", SearchOption.TopDirectoryOnly).Count());
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
