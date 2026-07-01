using System.Text.Json;
using LLMGameCreator.Application.Design.IntegratedCampaignTimelineSimulationMatrix;
using Xunit;

namespace LLMGameCreator.Tests.Application.IntegratedCampaignTimelineSimulationMatrix;

public sealed class IntegratedCampaignTimelineSourceLoadingTests
{
    [Fact]
    public void SourceLoaderConsumesGoal060ThroughGoal069EvidenceAndPreflightHandoff()
    {
        var source = new IntegratedCampaignTimelineSourceLoader().Load(IntegratedTimelineProjectRootLocator.ProjectRoot());

        Assert.True(source.Goal069AcceptedByUserHandoff);
        Assert.True(source.Goal060PackageRowsConsumed);
        Assert.True(source.Goal061ReviewPackageRcConsumed);
        Assert.True(source.Goal062SpatialRowsConsumed);
        Assert.True(source.Goal063GameplayRowsConsumed);
        Assert.True(source.Goal064LivingWorldRowsConsumed);
        Assert.True(source.Goal065InterlockedRowsConsumed);
        Assert.True(source.Goal066SettlementRowsConsumed);
        Assert.True(source.Goal067NarrativeRowsConsumed);
        Assert.True(source.Goal068CombatMagicRowsConsumed);
        Assert.True(source.Goal069WorldEventRowsConsumed);
        Assert.True(source.Goal069UnityProofConsumed);
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
            Assert.StartsWith("Goal067:", row.SourceNarrativeRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal068:", row.SourceCombatMagicRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal069:", row.SourceWorldEventRowRef, StringComparison.Ordinal);
            Assert.True(row.Goal069WorldEventRowValid);
            Assert.True(row.Goal069SaveLoadReplayPassed);
            Assert.NotEmpty(row.UpstreamHashes);
        });
    }
}

public sealed class IntegratedCampaignTimelineMatrixTests
{
    [Fact]
    public void BuildCreatesNineStateChangingMultiStepTimelineRows()
    {
        var result = new IntegratedCampaignTimelineEvidenceService().Build(IntegratedTimelineProjectRootLocator.ProjectRoot());

        Assert.True(result.SourceManifest.Goal069AcceptedByUserHandoff);
        Assert.True(result.SourceManifest.Goal060PackageRowsConsumed);
        Assert.True(result.SourceManifest.Goal061ReviewPackageRcConsumed);
        Assert.True(result.SourceManifest.Goal062SpatialRowsConsumed);
        Assert.True(result.SourceManifest.Goal063GameplayRowsConsumed);
        Assert.True(result.SourceManifest.Goal064LivingWorldRowsConsumed);
        Assert.True(result.SourceManifest.Goal065InterlockedRowsConsumed);
        Assert.True(result.SourceManifest.Goal066SettlementRowsConsumed);
        Assert.True(result.SourceManifest.Goal067NarrativeRowsConsumed);
        Assert.True(result.SourceManifest.Goal068CombatMagicRowsConsumed);
        Assert.True(result.SourceManifest.Goal069WorldEventRowsConsumed);
        Assert.True(result.MatrixSummary.Passed);
        Assert.Equal(9, result.MatrixSummary.RowCount);
        Assert.Equal(3, result.MatrixSummary.FamilyCount);
        Assert.Equal(3, result.MatrixSummary.SeedCount);
    }
}

public sealed class IntegratedCampaignTimelineShapeTests
{
    [Fact]
    public void MatrixCascadesArbitrationReplayAndVariancePass()
    {
        var result = new IntegratedCampaignTimelineEvidenceService().Build(IntegratedTimelineProjectRootLocator.ProjectRoot());

        Assert.True(result.MatrixSummary.Passed);
        Assert.Equal(9, result.MatrixSummary.RowCount);
        Assert.Equal(9, result.MatrixSummary.StateChangingRowCount);
        Assert.Equal(9, result.MatrixSummary.RowsWithSixOrMoreTicks);
        Assert.Equal(9, result.MatrixSummary.RowsWithFiveOrMoreCategories);
        Assert.Equal(9, result.MatrixSummary.RowsWithThreeOrMoreCascades);
        Assert.Equal(9, result.MatrixSummary.RowsWithArbitration);
        Assert.True(result.CascadeLedger.Passed);
        Assert.Equal(27, result.CascadeLedger.CascadeCount);
        Assert.True(result.ArbitrationLedger.Passed);
        Assert.Equal(9, result.ArbitrationLedger.ArbitrationCount);
        Assert.True(result.SaveLoadReplayAudit.Passed);
        Assert.Equal(9, result.SaveLoadReplayAudit.StateChangingRowCount);
        Assert.Equal(9, result.SaveLoadReplayAudit.SaveLoadPassedRowCount);
        Assert.Equal(9, result.SaveLoadReplayAudit.ReplayPassedRowCount);
        Assert.True(result.VarianceMetrics.Passed);
        Assert.Equal(9, result.VarianceMetrics.DistinctRowHashCount);
        Assert.Equal(3, result.VarianceMetrics.DistinctPhaseProfileCount);
        Assert.True(result.PreviewExportPayload.Passed);
        Assert.All(result.Rows, row =>
        {
            Assert.True(row.StateChanging);
            Assert.NotEqual(row.InitialState.StateHash, row.SaveLoadReplayProof.FinalStateHash);
            Assert.True(row.Ticks.Count >= 6);
            Assert.True(row.TouchedSystemCategories.Count >= 5);
            Assert.Contains(row.TouchedSystemCategories, category => category == "world_event_weather_crisis");
            Assert.Contains(row.TouchedSystemCategories, category => category == "settlement_production");
            Assert.Contains(row.TouchedSystemCategories, category => category == "narrative_quest_dialogue");
            Assert.Contains(row.TouchedSystemCategories, category => category == "combat_magic_status");
            Assert.True(row.Cascades.Count >= 3);
            Assert.True(row.Arbitration.Passed);
            Assert.True(row.SaveLoadReplayProof.SaveLoadRoundtripPassed);
            Assert.True(row.SaveLoadReplayProof.ReplayDeterminismPassed);
            Assert.DoesNotContain("finalProse", IntegratedCampaignTimelineHash.Serialize(row), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("generatedLua", IntegratedCampaignTimelineHash.Serialize(row), StringComparison.OrdinalIgnoreCase);
        });
    }
}

public sealed class IntegratedCampaignTimelineUnityPlanAndInvalidTests
{
    [Fact]
    public void UnityCommandPlanAndInvalidMatrixCoverGoal070Contract()
    {
        var result = new IntegratedCampaignTimelineEvidenceService().Build(IntegratedTimelineProjectRootLocator.ProjectRoot());

        Assert.True(result.UnityCommandPlan.Passed);
        Assert.False(result.UnityCommandPlan.Accepted);
        Assert.Equal(9, result.UnityCommandPlan.Rows.Count);
        foreach (var marker in IntegratedCampaignTimelineProjector.RequiredUnityMarkers())
        {
            Assert.Contains(marker, result.UnityCommandPlan.ExpectedPlayerMarkers);
        }

        Assert.All(result.UnityCommandPlan.Rows, row =>
        {
            Assert.True(row.TickIds.Count >= 6);
            Assert.True(row.CascadeIds.Count >= 3);
            Assert.NotEmpty(row.ArbitrationIds);
            Assert.Contains("campaign_timeline_row_started=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("campaign_timeline_row_completed=" + row.RowId, row.ExpectedPlayerMarkers);
        });

        Assert.True(result.InvalidMatrix.Passed);
        var ids = result.InvalidMatrix.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);
        foreach (var required in IntegratedCampaignTimelineVocabulary.RequiredInvalidScenarioIds)
        {
            Assert.Contains(required, ids);
        }
    }
}

public sealed class IntegratedCampaignTimelineEvidenceWriteTests
{
    [Fact]
    public async Task WriteAsyncEmitsRequiredDeterministicArtifactsAndStagingCommandPlan()
    {
        var service = new IntegratedCampaignTimelineEvidenceService();
        var result = service.Build(IntegratedTimelineProjectRootLocator.ProjectRoot());
        var second = service.Build(IntegratedTimelineProjectRootLocator.ProjectRoot());
        var tempRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "Goal070Write", Guid.NewGuid().ToString("N"));

        try
        {
            var write = await service.WriteAsync(tempRoot, result);
            Assert.Equal(result.Report.MatrixSummaryHash, second.Report.MatrixSummaryHash);
            Assert.Equal(result.Report.CascadeLedgerHash, second.Report.CascadeLedgerHash);
            Assert.Equal(result.Report.SaveLoadReplayAuditHash, second.Report.SaveLoadReplayAuditHash);
            Assert.Equal(result.Report.VarianceMetricsHash, second.Report.VarianceMetricsHash);
            Assert.Equal(result.Report.InvalidMatrixHash, second.Report.InvalidMatrixHash);
            Assert.DoesNotContain(result.Report.Diagnostics, item => item.Severity == "error" && !item.Code.StartsWith("goal070.unity.", StringComparison.Ordinal));

            foreach (var fileName in RequiredJsonFiles())
            {
                var path = Path.Combine(write.OutputDirectoryPath, fileName);
                Assert.True(File.Exists(path), "Missing artifact: " + fileName);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            foreach (var row in result.Rows)
            {
                var path = Path.Combine(write.OutputDirectoryPath, IntegratedCampaignTimelineEvidenceService.RowFileName(row));
                Assert.True(File.Exists(path), "Missing row artifact: " + path);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
            Assert.Contains("integrated_campaign_timeline_simulation_matrix_verification required", report);
            Assert.Contains("accepted=false", report);
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, IntegratedCampaignTimelineVocabulary.StagingRoot, IntegratedCampaignTimelineVocabulary.UnityCampaignTimelineCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
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
        IntegratedCampaignTimelineEvidenceService.SourceManifestJsonFileName,
        IntegratedCampaignTimelineEvidenceService.MatrixSummaryJsonFileName,
        IntegratedCampaignTimelineEvidenceService.CascadeLedgerJsonFileName,
        IntegratedCampaignTimelineEvidenceService.ArbitrationLedgerJsonFileName,
        IntegratedCampaignTimelineEvidenceService.SaveLoadReplayAuditJsonFileName,
        IntegratedCampaignTimelineEvidenceService.VarianceMetricsJsonFileName,
        IntegratedCampaignTimelineEvidenceService.UnityCommandPlanJsonFileName,
        IntegratedCampaignTimelineEvidenceService.UnityPlayerProofSummaryJsonFileName,
        IntegratedCampaignTimelineEvidenceService.PreviewExportTimelinePayloadJsonFileName,
        IntegratedCampaignTimelineEvidenceService.InvalidDiagnosticsMatrixJsonFileName,
        IntegratedCampaignTimelineEvidenceService.ArtifactScopeReportJsonFileName
    ];
}

internal static class IntegratedTimelineProjectRootLocator
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
