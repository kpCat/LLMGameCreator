using LLMGameCreator.Application.Design.IntegratedCampaignTimelineSimulationMatrix;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class IntegratedCampaignTimelineSimulationMatrixProductSmokeTests
{
    [Fact]
    public async Task Goal070IntegratedCampaignTimelineSimulationMatrixEvidenceIsProducedForReview()
    {
        var service = new IntegratedCampaignTimelineEvidenceService();
        var write = await service.BuildAndWriteAsync(
            ProjectRoot(),
            new IntegratedCampaignTimelineOptions
            {
                ExecuteUnityProof = true,
                CleanupUnityWorkProject = true,
                UnityBuildTimeoutSeconds = 900,
                PlayerLaunchTimeoutSeconds = 120
            });

        Assert.True(write.Result.SourceManifest.Goal069AcceptedByUserHandoff);
        Assert.True(write.Result.SourceManifest.Goal060PackageRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal061ReviewPackageRcConsumed);
        Assert.True(write.Result.SourceManifest.Goal062SpatialRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal063GameplayRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal064LivingWorldRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal065InterlockedRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal066SettlementRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal067NarrativeRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal068CombatMagicRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal069WorldEventRowsConsumed);
        Assert.True(write.Result.MatrixSummary.Passed);
        Assert.True(write.Result.CascadeLedger.Passed);
        Assert.True(write.Result.ArbitrationLedger.Passed);
        Assert.True(write.Result.SaveLoadReplayAudit.Passed);
        Assert.True(write.Result.VarianceMetrics.Passed);
        Assert.True(write.Result.PreviewExportPayload.Passed);
        Assert.True(write.Result.InvalidMatrix.Passed);
        Assert.True(write.Result.UnityCommandPlan.Passed);
        Assert.Equal(9, write.Result.MatrixSummary.RowCount);
        Assert.Equal(9, write.Result.MatrixSummary.StateChangingRowCount);
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
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, IntegratedCampaignTimelineEvidenceService.ArtifactScopeReportJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, IntegratedCampaignTimelineVocabulary.StagingRoot, IntegratedCampaignTimelineVocabulary.UnityCampaignTimelineCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
        Assert.Equal(9, Directory.EnumerateFiles(write.OutputDirectoryPath, "campaign-timeline-row-*.json", SearchOption.TopDirectoryOnly).Count());
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
