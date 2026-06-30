using LLMGameCreator.Application.Design.GameplayConsequenceDepthMatrix;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class GameplayConsequenceDepthMatrixProductSmokeTests
{
    [Fact]
    public async Task Goal063GameplayConsequenceDepthMatrixEvidenceIsProducedForReview()
    {
        var service = new GameplayConsequenceDepthMatrixEvidenceService();
        var write = await service.BuildAndWriteAsync(
            ProjectRoot(),
            new GameplayConsequenceDepthMatrixOptions
            {
                ExecuteUnityProof = true,
                CleanupUnityWorkProject = true,
                UnityBuildTimeoutSeconds = 900,
                PlayerLaunchTimeoutSeconds = 120
            });

        Assert.True(write.Result.SourceManifest.Goal062AcceptedByUserHandoff);
        Assert.True(write.Result.CommandPlanMatrix.Passed);
        Assert.True(write.Result.RuntimeStateDeltaMatrix.Passed);
        Assert.True(write.Result.SaveLoadReplayAudit.Passed);
        Assert.True(write.Result.FamilySummary.MeaningfulVariancePassed);
        Assert.True(write.Result.PreviewExportPayload.Passed);
        Assert.True(write.Result.InvalidMatrix.Passed);
        Assert.True(write.Result.UnityCommandPlan.Passed);
        Assert.Equal(9, write.Result.RuntimeStateDeltaMatrix.RowCount);
        Assert.Equal(9, write.Result.RuntimeStateDeltaMatrix.StateChangingRowCount);
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
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, GameplayConsequenceDepthMatrixEvidenceService.ArtifactScopeReportJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, GameplayConsequenceDepthMatrixVocabulary.StagingRoot, GameplayConsequenceDepthMatrixVocabulary.UnityGameplayCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
        Assert.Equal(9, Directory.EnumerateFiles(Path.Combine(write.OutputDirectoryPath, GameplayConsequenceDepthMatrixEvidenceService.RowsDirectoryName), "*-gameplay-proof.json", SearchOption.TopDirectoryOnly).Count());
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
