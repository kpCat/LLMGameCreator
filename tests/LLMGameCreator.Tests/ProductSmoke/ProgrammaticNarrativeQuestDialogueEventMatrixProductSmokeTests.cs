using LLMGameCreator.Application.Design.ProgrammaticNarrativeQuestDialogueEventMatrix;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class ProgrammaticNarrativeQuestDialogueEventMatrixProductSmokeTests
{
    [Fact]
    public async Task Goal067ProgrammaticNarrativeQuestDialogueEventMatrixEvidenceIsProducedForReview()
    {
        var service = new ProgrammaticNarrativeEvidenceService();
        var write = await service.BuildAndWriteAsync(
            ProjectRoot(),
            new ProgrammaticNarrativeOptions
            {
                ExecuteUnityProof = true,
                CleanupUnityWorkProject = true,
                UnityBuildTimeoutSeconds = 900,
                PlayerLaunchTimeoutSeconds = 120
            });

        Assert.True(write.Result.SourceManifest.Goal066AcceptedByUserHandoff);
        Assert.True(write.Result.SourceManifest.Goal060PackageRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal061ReviewPackageRcConsumed);
        Assert.True(write.Result.SourceManifest.Goal062SpatialRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal063GameplayRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal064LivingWorldRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal065InterlockedRowsConsumed);
        Assert.True(write.Result.SourceManifest.Goal066SettlementRowsConsumed);
        Assert.True(write.Result.TemplateCatalog.Passed);
        Assert.True(write.Result.RowMatrix.Passed);
        Assert.True(write.Result.QuestStageLedger.Passed);
        Assert.True(write.Result.DialogueOptionLedger.Passed);
        Assert.True(write.Result.EventConsequenceLedger.Passed);
        Assert.True(write.Result.LocalizationKeyTable.Passed);
        Assert.True(write.Result.MemoryRumorLedger.Passed);
        Assert.True(write.Result.SaveLoadReplayProof.Passed);
        Assert.True(write.Result.Report.MeaningfulVariancePassed);
        Assert.True(write.Result.PreviewExportPayload.Passed);
        Assert.True(write.Result.InvalidMatrix.Passed);
        Assert.True(write.Result.UnityCommandPlan.Passed);
        Assert.True(write.Result.Report.NoFinalProseLeakage);
        Assert.Equal(9, write.Result.RowMatrix.RowCount);
        Assert.Equal(9, write.Result.RowMatrix.StateChangingRowCount);
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
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ProgrammaticNarrativeEvidenceService.ArtifactScopeReportJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ProgrammaticNarrativeVocabulary.StagingRoot, ProgrammaticNarrativeVocabulary.UnityNarrativeCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
        Assert.Equal(9, Directory.EnumerateFiles(Path.Combine(write.OutputDirectoryPath, ProgrammaticNarrativeEvidenceService.RowsDirectoryName), "*-narrative-row.json", SearchOption.TopDirectoryOnly).Count());
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
