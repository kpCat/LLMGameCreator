using LLMGameCreator.Application.Design.UnityAlphaInteractiveCampaignPlayer;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class UnityAlphaInteractiveCampaignPlayerProductSmokeTests
{
    [Fact]
    public async Task Goal071UnityAlphaInteractiveCampaignPlayerEvidenceIsProducedForReview()
    {
        var service = new UnityAlphaInteractiveCampaignEvidenceService();
        var write = await service.BuildAndWriteAsync(
            ProjectRoot(),
            new UnityAlphaInteractiveCampaignOptions
            {
                ExecuteUnityProof = true,
                CleanupUnityWorkProject = true,
                UnityBuildTimeoutSeconds = 900,
                PlayerLaunchTimeoutSeconds = 120
            });

        Assert.True(write.Result.SourceManifest.Goal070AcceptedByUserHandoff);
        Assert.True(write.Result.SourceManifest.Goal070TimelineEvidenceConsumed);
        Assert.True(write.Result.SourceManifest.Goal070UnityProofConsumed);
        Assert.True(write.Result.Matrix.Passed);
        Assert.True(write.Result.Selector.Passed);
        Assert.True(write.Result.InputActionScript.Passed);
        Assert.True(write.Result.StateTransitionLedger.Passed);
        Assert.True(write.Result.SaveLoadReplayProof.Passed);
        Assert.True(write.Result.HudContract.Passed);
        Assert.True(write.Result.PreviewExportPayload.Passed);
        Assert.True(write.Result.InvalidMatrix.Passed);
        Assert.True(write.Result.UnityCommandPlan.Passed);
        Assert.Equal(9, write.Result.Matrix.RowCount);
        Assert.Equal(9, write.Result.Matrix.StateChangingRowCount);
        Assert.Equal(63, write.Result.InputActionScript.ActionCount);
        Assert.Equal(63, write.Result.StateTransitionLedger.TransitionCount);
        Assert.All(write.Result.UnityCommandPlan.Rows, row =>
        {
            Assert.Equal(7, row.InputIds.Count);
            Assert.Equal(7, row.ActionIds.Count);
            Assert.Equal(7, row.StepIds.Count);
            Assert.Equal(7, row.StateBeforeHashes.Count);
            Assert.Equal(7, row.StateAfterHashes.Count);
            Assert.All(row.StateBeforeHashes.Zip(row.StateAfterHashes), pair => Assert.NotEqual(pair.First, pair.Second));
        });

        Assert.Contains(write.Result.Report.ImplementationStatus, new[] { "GREEN", "BLOCKED" });
        if (write.Result.Report.ImplementationStatus == "GREEN")
        {
            Assert.True(write.Result.UnityProofSummary.Passed);
            Assert.Equal(0, write.Result.UnityProofSummary.UnityExitCode);
            Assert.Equal(0, write.Result.UnityProofSummary.PlayerExitCode);
            Assert.Equal(9, write.Result.UnityProofSummary.ProvenRowCount);
            Assert.Empty(write.Result.UnityProofSummary.MissingMarkers);
            Assert.NotEmpty(write.Result.UnityProofSummary.MatchedMarkers);
            Assert.All(
                write.Result.UnityCommandPlan.ExpectedPlayerMarkers,
                marker => Assert.Contains(marker, write.Result.UnityProofSummary.MatchedMarkers));
        }
        else
        {
            Assert.False(write.Result.UnityProofSummary.Passed);
            Assert.NotEmpty(write.Result.UnityProofSummary.Diagnostics);
        }

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, UnityAlphaInteractiveCampaignEvidenceService.ArtifactScopeReportJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, UnityAlphaInteractiveCampaignVocabulary.StagingRoot, UnityAlphaInteractiveCampaignVocabulary.UnityInteractiveCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
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
