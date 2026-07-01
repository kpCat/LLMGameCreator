using System.Text.Json;
using LLMGameCreator.Application.Design.ProgrammaticNarrativeQuestDialogueEventMatrix;
using Xunit;

namespace LLMGameCreator.Tests.Application.ProgrammaticNarrativeQuestDialogueEventMatrix;

public sealed class ProgrammaticNarrativeSourceLoadingTests
{
    [Fact]
    public void SourceLoaderConsumesGoal060ThroughGoal066EvidenceAndPreflightHandoff()
    {
        var source = new ProgrammaticNarrativeSourceLoader().Load(ProjectRootLocator.ProjectRoot());

        Assert.True(source.Goal066AcceptedByUserHandoff);
        Assert.True(source.Goal060PackageRowsConsumed);
        Assert.True(source.Goal061ReviewPackageRcConsumed);
        Assert.True(source.Goal062SpatialRowsConsumed);
        Assert.True(source.Goal063GameplayRowsConsumed);
        Assert.True(source.Goal064LivingWorldRowsConsumed);
        Assert.True(source.Goal065InterlockedRowsConsumed);
        Assert.True(source.Goal066SettlementRowsConsumed);
        Assert.True(source.Goal066UnityProofConsumed);
        Assert.Equal(9, source.Rows.Count);
        Assert.Equal(3, source.FamilyIds.Count);
        Assert.Equal(3, source.SeedIds.Count);
        Assert.All(source.Rows, row =>
        {
            Assert.StartsWith("Goal060:", row.SourcePackageRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal061:", row.SourceReviewPackageRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal062:", row.SourceSpatialDetailRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal063:", row.SourceGameplayConsequenceRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal064:", row.SourceLivingWorldRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal065:", row.SourceInterlockedGameplayRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal066:", row.SourceSettlementRowRef, StringComparison.Ordinal);
            Assert.True(row.Goal060PackageValid);
            Assert.True(row.Goal061ReviewPackageRcExists);
            Assert.True(row.Goal062SpatialRowValid);
            Assert.True(row.Goal063GameplayRowValid);
            Assert.True(row.Goal064LivingWorldRowValid);
            Assert.True(row.Goal065InterlockedRowValid);
            Assert.True(row.Goal066SettlementRowValid);
            Assert.True(row.Goal066SaveLoadReplayPassed);
            Assert.NotEmpty(row.LivingWorldActorIds);
            Assert.NotEmpty(row.LivingWorldFactionIds);
            Assert.NotEmpty(row.InterlockedDeltaIds);
            Assert.NotEmpty(row.SettlementLedgerEntryIds);
        });
    }
}

public sealed class ProgrammaticNarrativeMatrixTests
{
    [Fact]
    public void BuildCreatesNineStateChangingNarrativeRowsWithoutFinalProse()
    {
        var result = new ProgrammaticNarrativeEvidenceService().Build(ProjectRootLocator.ProjectRoot());

        Assert.True(result.SourceManifest.Goal066AcceptedByUserHandoff);
        Assert.True(result.SourceManifest.Goal060PackageRowsConsumed);
        Assert.True(result.SourceManifest.Goal061ReviewPackageRcConsumed);
        Assert.True(result.SourceManifest.Goal062SpatialRowsConsumed);
        Assert.True(result.SourceManifest.Goal063GameplayRowsConsumed);
        Assert.True(result.SourceManifest.Goal064LivingWorldRowsConsumed);
        Assert.True(result.SourceManifest.Goal065InterlockedRowsConsumed);
        Assert.True(result.SourceManifest.Goal066SettlementRowsConsumed);
        Assert.True(result.TemplateCatalog.Passed);
        Assert.True(result.RowMatrix.Passed);
        Assert.Equal(9, result.RowMatrix.RowCount);
        Assert.Equal(9, result.RowMatrix.StateChangingRowCount);
        Assert.Equal(3, result.RowMatrix.FamilyCount);
        Assert.Equal(3, result.RowMatrix.SeedCount);
        Assert.Equal(9, result.RowMatrix.DistinctRowHashCount);
        Assert.All(result.Rows, row =>
        {
            Assert.True(row.StateChanging);
            Assert.True(row.NoFinalProse);
            Assert.NotEqual(row.BeforeState.StateHash, row.AfterState.StateHash);
            Assert.True(row.QuestStageGraph.Count >= 3);
            Assert.True(row.DialogueOptionGraph.Count >= 2);
            Assert.NotEmpty(row.EventTriggerConsequenceChain);
            Assert.NotEmpty(row.LocalizationKeyTable);
            Assert.NotEmpty(row.MemoryRumorPropagation);
            Assert.True(row.StateDeltas.Count >= 2);
            Assert.NotEmpty(row.SettlementId);
            Assert.NotEmpty(row.LivingWorldAfterStateHash);
            Assert.NotEmpty(row.InterlockedAfterStateHash);
            Assert.NotEmpty(row.SettlementAfterStateHash);
            Assert.DoesNotContain("lineText", ProgrammaticNarrativeHash.Serialize(row), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("finalDialogue", ProgrammaticNarrativeHash.Serialize(row), StringComparison.OrdinalIgnoreCase);
        });
    }
}

public sealed class ProgrammaticNarrativeLedgerReplayAndUnityPlanTests
{
    [Fact]
    public void LedgersReplayVarianceAndUnityCommandPlanPassForEveryFamilySeedRow()
    {
        var result = new ProgrammaticNarrativeEvidenceService().Build(ProjectRootLocator.ProjectRoot());

        Assert.True(result.QuestStageLedger.Passed);
        Assert.True(result.DialogueOptionLedger.Passed);
        Assert.True(result.EventConsequenceLedger.Passed);
        Assert.True(result.LocalizationKeyTable.Passed);
        Assert.True(result.MemoryRumorLedger.Passed);
        Assert.True(result.SaveLoadReplayProof.Passed);
        Assert.Equal(9, result.SaveLoadReplayProof.StateChangedRowCount);
        Assert.Equal(9, result.SaveLoadReplayProof.SaveLoadPassedRowCount);
        Assert.Equal(9, result.SaveLoadReplayProof.ReplayPassedRowCount);
        Assert.True(result.Report.MeaningfulVariancePassed);
        Assert.True(result.UnityCommandPlan.Passed);
        Assert.False(result.UnityCommandPlan.Accepted);
        Assert.Equal(9, result.UnityCommandPlan.Rows.Count);
        foreach (var marker in ProgrammaticNarrativeValidator.RequiredUnityMarkers())
        {
            Assert.Contains(marker, result.UnityCommandPlan.ExpectedPlayerMarkers);
        }

        Assert.All(result.UnityCommandPlan.Rows, row =>
        {
            Assert.Contains("narrative_row_loaded=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("quest_stage_started=" + row.QuestStageId, row.ExpectedPlayerMarkers);
            Assert.Contains("dialogue_option_available=" + row.DialogueOptionId, row.ExpectedPlayerMarkers);
            Assert.Contains("dialogue_option_selected=" + row.DialogueOptionId, row.ExpectedPlayerMarkers);
            Assert.Contains("event_trigger_resolved=" + row.EventTriggerId, row.ExpectedPlayerMarkers);
            Assert.Contains("event_consequence_applied=" + row.EventConsequenceId, row.ExpectedPlayerMarkers);
            Assert.Contains("memory_rumor_recorded=" + row.MemoryRumorRecordId, row.ExpectedPlayerMarkers);
            Assert.Contains("localization_key_bound=" + row.LocalizationLineKey, row.ExpectedPlayerMarkers);
            Assert.Contains("narrative_row_completed=" + row.RowId, row.ExpectedPlayerMarkers);
        });
    }
}

public sealed class ProgrammaticNarrativeInvalidMatrixTests
{
    [Fact]
    public void InvalidFakeLeakMatrixCoversRequiredCases()
    {
        var result = new ProgrammaticNarrativeEvidenceService().Build(ProjectRootLocator.ProjectRoot());
        var matrix = result.InvalidMatrix;
        var ids = matrix.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        Assert.True(result.Report.NoFinalProseLeakage);
        foreach (var required in ProgrammaticNarrativeVocabulary.RequiredInvalidScenarioIds)
        {
            Assert.Contains(required, ids);
        }

        Assert.All(matrix.Scenarios, scenario =>
        {
            Assert.Equal(scenario.ExpectedStatus, scenario.ActualStatus);
            Assert.NotEmpty(scenario.Diagnostics);
            Assert.All(scenario.Diagnostics, diagnostic => Assert.StartsWith("goal067.", diagnostic.Code, StringComparison.Ordinal));
        });
    }
}

public sealed class ProgrammaticNarrativeEvidenceWriteTests
{
    [Fact]
    public async Task WriteAsyncEmitsDeterministicArtifactsRowsAndStagingCommandPlan()
    {
        var service = new ProgrammaticNarrativeEvidenceService();
        var result = service.Build(ProjectRootLocator.ProjectRoot());
        var second = service.Build(ProjectRootLocator.ProjectRoot());
        var tempRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "Goal067Write", Guid.NewGuid().ToString("N"));

        try
        {
            var write = await service.WriteAsync(tempRoot, result);
            Assert.Equal(result.Report.RowMatrixHash, second.Report.RowMatrixHash);
            Assert.Equal(result.Report.QuestStageLedgerHash, second.Report.QuestStageLedgerHash);
            Assert.Equal(result.Report.DialogueOptionLedgerHash, second.Report.DialogueOptionLedgerHash);
            Assert.Equal(result.Report.EventConsequenceLedgerHash, second.Report.EventConsequenceLedgerHash);
            Assert.Equal(result.Report.LocalizationKeyTableHash, second.Report.LocalizationKeyTableHash);
            Assert.Equal(result.Report.MemoryRumorLedgerHash, second.Report.MemoryRumorLedgerHash);
            Assert.Equal(result.Report.SaveLoadReplayProofHash, second.Report.SaveLoadReplayProofHash);
            Assert.Equal(result.Report.InvalidMatrixHash, second.Report.InvalidMatrixHash);
            Assert.DoesNotContain(result.Report.Diagnostics, item => item.Severity == "error" && !item.Code.StartsWith("goal067.unity.", StringComparison.Ordinal));

            foreach (var fileName in RequiredJsonFiles())
            {
                var path = Path.Combine(write.OutputDirectoryPath, fileName);
                Assert.True(File.Exists(path), "Missing artifact: " + fileName);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            foreach (var row in result.Rows)
            {
                var path = Path.Combine(write.OutputDirectoryPath, ProgrammaticNarrativeEvidenceService.RowsDirectoryName, ProgrammaticNarrativeEvidenceService.RowFileName(row));
                Assert.True(File.Exists(path), "Missing row artifact: " + path);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            Assert.True(File.Exists(write.ReportMarkdownPath));
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, ProgrammaticNarrativeVocabulary.StagingRoot, ProgrammaticNarrativeVocabulary.UnityNarrativeCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    private static IReadOnlyList<string> RequiredJsonFiles() =>
    [
        ProgrammaticNarrativeEvidenceService.SourceManifestJsonFileName,
        ProgrammaticNarrativeEvidenceService.RowMatrixJsonFileName,
        ProgrammaticNarrativeEvidenceService.TemplateCatalogJsonFileName,
        ProgrammaticNarrativeEvidenceService.QuestStageLedgerJsonFileName,
        ProgrammaticNarrativeEvidenceService.DialogueOptionLedgerJsonFileName,
        ProgrammaticNarrativeEvidenceService.EventConsequenceLedgerJsonFileName,
        ProgrammaticNarrativeEvidenceService.LocalizationKeyTableJsonFileName,
        ProgrammaticNarrativeEvidenceService.MemoryRumorLedgerJsonFileName,
        ProgrammaticNarrativeEvidenceService.SaveLoadReplayProofJsonFileName,
        ProgrammaticNarrativeEvidenceService.PreviewExportPayloadJsonFileName,
        ProgrammaticNarrativeEvidenceService.UnityCommandPlanJsonFileName,
        ProgrammaticNarrativeEvidenceService.UnityProofSummaryJsonFileName,
        ProgrammaticNarrativeEvidenceService.InvalidDiagnosticsMatrixJsonFileName,
        ProgrammaticNarrativeEvidenceService.ArtifactScopeReportJsonFileName
    ];
}

internal static class ProjectRootLocator
{
    public static string ProjectRoot()
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
