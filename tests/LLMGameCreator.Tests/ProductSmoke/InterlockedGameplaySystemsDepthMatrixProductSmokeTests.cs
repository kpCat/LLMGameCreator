using LLMGameCreator.Application.Design.InterlockedGameplaySystemsDepthMatrix;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class InterlockedGameplaySystemsDepthMatrixProductSmokeTests
{
    [Fact]
    public async Task Goal065InterlockedGameplaySystemsDepthMatrixEvidenceIsProducedForReview()
    {
        var service = new InterlockedGameplaySystemsEvidenceService();
        var write = await service.BuildAndWriteAsync(
            ProjectRoot(),
            new InterlockedGameplaySystemsOptions
            {
                ExecuteUnityProof = true,
                CleanupUnityWorkProject = true,
                UnityBuildTimeoutSeconds = 900,
                PlayerLaunchTimeoutSeconds = 120
            });

        Assert.True(write.Result.SourceManifest.Goal064AcceptedByUserHandoff);
        Assert.True(write.Result.SourceManifest.Goal060PackageRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal061ReviewRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal062SpatialRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal063GameplayRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal064LivingWorldRowsConsumed);
        Assert.True(write.Result.RuleCatalog.Passed);
        Assert.True(write.Result.RowPlanMatrix.Passed);
        Assert.True(write.Result.EconomyCraftingLedger.Passed);
        Assert.True(write.Result.CombatProgressionLedger.Passed);
        Assert.True(write.Result.StatusEffectLedger.Passed);
        Assert.True(write.Result.SaveLoadReplayProof.Passed);
        Assert.True(write.Result.VarianceMetrics.Passed);
        Assert.True(write.Result.PreviewExportPayload.Passed);
        Assert.True(write.Result.InvalidMatrix.Passed);
        Assert.True(write.Result.UnityCommandPlan.Passed);
        Assert.Equal(9, write.Result.RowPlanMatrix.RowCount);
        Assert.Equal(9, write.Result.RowPlanMatrix.StateChangingRowCount);
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
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, InterlockedGameplaySystemsEvidenceService.ArtifactScopeReportJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, InterlockedGameplaySystemsDepthMatrixVocabulary.StagingRoot, InterlockedGameplaySystemsDepthMatrixVocabulary.UnityInterlockedCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
        Assert.Equal(9, write.Result.Rows.Count(row => File.Exists(Path.Combine(write.OutputDirectoryPath, InterlockedGameplaySystemsEvidenceService.RowFileName(row)))));
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
