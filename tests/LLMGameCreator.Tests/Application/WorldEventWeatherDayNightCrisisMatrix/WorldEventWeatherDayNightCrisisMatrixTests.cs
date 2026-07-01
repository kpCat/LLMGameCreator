using System.Text.Json;
using LLMGameCreator.Application.Design.WorldEventWeatherDayNightCrisisMatrix;
using Xunit;

namespace LLMGameCreator.Tests.Application.WorldEventWeatherDayNightCrisisMatrix;

public sealed class WorldEventSourceLoadingTests
{
    [Fact]
    public void SourceLoaderConsumesGoal060ThroughGoal068EvidenceAndPreflightHandoff()
    {
        var source = new WorldEventWeatherDayNightCrisisSourceLoader().Load(WorldEventProjectRootLocator.ProjectRoot());

        Assert.True(source.Goal068AcceptedByUserHandoff);
        Assert.True(source.Goal060PackageRowsConsumed);
        Assert.True(source.Goal061ReviewPackageRcConsumed);
        Assert.True(source.Goal062SpatialRowsConsumed);
        Assert.True(source.Goal063GameplayRowsConsumed);
        Assert.True(source.Goal064LivingWorldRowsConsumed);
        Assert.True(source.Goal065InterlockedRowsConsumed);
        Assert.True(source.Goal066SettlementRowsConsumed);
        Assert.True(source.Goal067NarrativeRowsConsumed);
        Assert.True(source.Goal068CombatMagicRowsConsumed);
        Assert.True(source.Goal068UnityProofConsumed);
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
            Assert.True(row.Goal068CombatMagicRowValid);
            Assert.True(row.Goal068SaveLoadReplayPassed);
            Assert.NotEmpty(row.UpstreamHashes);
        });
    }
}

public sealed class WorldEventMatrixTests
{
    [Fact]
    public void BuildCreatesNineStateChangingEnvironmentalPressureRows()
    {
        var result = new WorldEventWeatherDayNightCrisisEvidenceService().Build(WorldEventProjectRootLocator.ProjectRoot());

        Assert.True(result.SourceManifest.Goal068AcceptedByUserHandoff);
        Assert.True(result.SourceManifest.Goal060PackageRowsConsumed);
        Assert.True(result.SourceManifest.Goal061ReviewPackageRcConsumed);
        Assert.True(result.SourceManifest.Goal062SpatialRowsConsumed);
        Assert.True(result.SourceManifest.Goal063GameplayRowsConsumed);
        Assert.True(result.SourceManifest.Goal064LivingWorldRowsConsumed);
        Assert.True(result.SourceManifest.Goal065InterlockedRowsConsumed);
        Assert.True(result.SourceManifest.Goal066SettlementRowsConsumed);
        Assert.True(result.SourceManifest.Goal067NarrativeRowsConsumed);
        Assert.True(result.SourceManifest.Goal068CombatMagicRowsConsumed);
        Assert.True(result.WorldClockPolicy.Passed);
        Assert.True(result.WeatherHazardCatalog.Passed);
        Assert.True(result.CrisisEventCatalog.Passed);
        Assert.True(result.RowMatrix.Passed);
        Assert.Equal(9, result.RowMatrix.RowCount);
        Assert.Equal(9, result.RowMatrix.StateChangingRowCount);
        Assert.Equal(9, result.RowMatrix.DayNightEffectRowCount);
        Assert.Equal(9, result.RowMatrix.WeatherHazardRowCount);
        Assert.Equal(9, result.RowMatrix.CrisisConsequenceRowCount);
        Assert.Equal(9, result.RowMatrix.CrossSystemDeltaRowCount);
        Assert.Equal(3, result.RowMatrix.FamilyCount);
        Assert.Equal(3, result.RowMatrix.SeedCount);
        Assert.Equal(9, result.RowMatrix.DistinctRowHashCount);
        Assert.All(result.Rows, row =>
        {
            Assert.True(row.StateChanging);
            Assert.NotEqual(row.BeforeState.StateHash, row.AfterState.StateHash);
            Assert.True(row.DayNightEffect.Passed);
            Assert.NotEqual(row.DayNightEffect.BeforePhase, row.DayNightEffect.AfterPhase);
            Assert.True(row.WeatherHazard.Passed);
            Assert.True(row.CrisisEvent.Passed);
            Assert.True(row.CrossSystemDeltas.Select(delta => delta.Category).Distinct(StringComparer.Ordinal).Count() >= 2);
            Assert.True(row.CrossSystemDeltas.Count >= 5);
            Assert.Contains(row.CrossSystemDeltas, delta => delta.Category == "combat_magic_status");
            Assert.Contains(row.CrossSystemDeltas, delta => delta.Category == "narrative_quest_dialogue");
            Assert.DoesNotContain("weatherapi", WorldEventWeatherDayNightCrisisHash.Serialize(row), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("generatedLua", WorldEventWeatherDayNightCrisisHash.Serialize(row), StringComparison.OrdinalIgnoreCase);
        });
    }
}

public sealed class WorldEventReplayVarianceAndUnityPlanTests
{
    [Fact]
    public void ReplayVarianceAndUnityCommandPlanPassForEveryFamilySeedRow()
    {
        var result = new WorldEventWeatherDayNightCrisisEvidenceService().Build(WorldEventProjectRootLocator.ProjectRoot());

        Assert.True(result.SaveLoadReplayProof.Passed);
        Assert.Equal(9, result.SaveLoadReplayProof.StateChangedRowCount);
        Assert.Equal(9, result.SaveLoadReplayProof.SaveLoadPassedRowCount);
        Assert.Equal(9, result.SaveLoadReplayProof.ReplayPassedRowCount);
        Assert.True(result.VarianceMetrics.Passed);
        Assert.Equal(9, result.VarianceMetrics.DistinctWeatherCount);
        Assert.Equal(9, result.VarianceMetrics.DistinctCrisisCount);
        Assert.True(result.PreviewExportPayload.Passed);
        Assert.True(result.UnityCommandPlan.Passed);
        Assert.False(result.UnityCommandPlan.Accepted);
        Assert.Equal(9, result.UnityCommandPlan.Rows.Count);
        foreach (var marker in WorldEventWeatherDayNightCrisisValidator.RequiredUnityMarkers())
        {
            Assert.Contains(marker, result.UnityCommandPlan.ExpectedPlayerMarkers);
        }

        Assert.All(result.UnityCommandPlan.Rows, row =>
        {
            Assert.Contains("world_event_row=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("world_event_family=" + row.FamilyId, row.ExpectedPlayerMarkers);
            Assert.Contains("world_event_seed=" + row.SeedId, row.ExpectedPlayerMarkers);
            Assert.Contains("world_event_clock_phase=" + row.ClockPhase, row.ExpectedPlayerMarkers);
            Assert.Contains("world_event_weather=" + row.WeatherId, row.ExpectedPlayerMarkers);
            Assert.Contains("world_event_crisis=" + row.CrisisId, row.ExpectedPlayerMarkers);
            Assert.Contains("world_event_state_changed=true", row.ExpectedPlayerMarkers);
            Assert.Contains("world_event_save_load_replay=true", row.ExpectedPlayerMarkers);
            Assert.Contains("world_event_row_completed=" + row.RowId, row.ExpectedPlayerMarkers);
        });
    }
}

public sealed class WorldEventInvalidMatrixTests
{
    [Fact]
    public void InvalidFakeLeakMatrixCoversRequiredCases()
    {
        var result = new WorldEventWeatherDayNightCrisisEvidenceService().Build(WorldEventProjectRootLocator.ProjectRoot());
        var matrix = result.InvalidMatrix;
        var ids = matrix.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        foreach (var required in WorldEventWeatherDayNightCrisisVocabulary.RequiredInvalidScenarioIds)
        {
            Assert.Contains(required, ids);
        }

        Assert.All(matrix.Scenarios, scenario =>
        {
            Assert.Equal(scenario.ExpectedStatus, scenario.ActualStatus);
            Assert.NotEmpty(scenario.Diagnostics);
            Assert.All(scenario.Diagnostics, diagnostic => Assert.StartsWith("goal069.", diagnostic.Code, StringComparison.Ordinal));
        });
    }
}

public sealed class WorldEventEvidenceWriteTests
{
    [Fact]
    public async Task WriteAsyncEmitsDeterministicArtifactsRowsAndStagingCommandPlan()
    {
        var service = new WorldEventWeatherDayNightCrisisEvidenceService();
        var result = service.Build(WorldEventProjectRootLocator.ProjectRoot());
        var second = service.Build(WorldEventProjectRootLocator.ProjectRoot());
        var tempRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "Goal069Write", Guid.NewGuid().ToString("N"));

        try
        {
            var write = await service.WriteAsync(tempRoot, result);
            Assert.Equal(result.Report.RowMatrixHash, second.Report.RowMatrixHash);
            Assert.Equal(result.Report.SaveLoadReplayProofHash, second.Report.SaveLoadReplayProofHash);
            Assert.Equal(result.Report.VarianceMetricsHash, second.Report.VarianceMetricsHash);
            Assert.Equal(result.Report.InvalidMatrixHash, second.Report.InvalidMatrixHash);
            Assert.DoesNotContain(result.Report.Diagnostics, item => item.Severity == "error" && !item.Code.StartsWith("goal069.unity.", StringComparison.Ordinal));

            foreach (var fileName in RequiredJsonFiles())
            {
                var path = Path.Combine(write.OutputDirectoryPath, fileName);
                Assert.True(File.Exists(path), "Missing artifact: " + fileName);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            foreach (var row in result.Rows)
            {
                var path = Path.Combine(write.OutputDirectoryPath, WorldEventWeatherDayNightCrisisEvidenceService.RowsDirectoryName, WorldEventWeatherDayNightCrisisEvidenceService.RowFileName(row));
                Assert.True(File.Exists(path), "Missing row artifact: " + path);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
            Assert.Contains("world_event_weather_daynight_crisis_matrix_verification required", report);
            Assert.Contains("accepted=false", report);
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, WorldEventWeatherDayNightCrisisVocabulary.StagingRoot, WorldEventWeatherDayNightCrisisVocabulary.UnityWorldEventCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
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
        WorldEventWeatherDayNightCrisisEvidenceService.SourceManifestJsonFileName,
        WorldEventWeatherDayNightCrisisEvidenceService.WorldClockCalendarPolicyJsonFileName,
        WorldEventWeatherDayNightCrisisEvidenceService.WeatherHazardCatalogJsonFileName,
        WorldEventWeatherDayNightCrisisEvidenceService.CrisisEventCatalogJsonFileName,
        WorldEventWeatherDayNightCrisisEvidenceService.RowMatrixJsonFileName,
        WorldEventWeatherDayNightCrisisEvidenceService.SaveLoadReplayProofJsonFileName,
        WorldEventWeatherDayNightCrisisEvidenceService.VarianceMetricsJsonFileName,
        WorldEventWeatherDayNightCrisisEvidenceService.UnityCommandPlanJsonFileName,
        WorldEventWeatherDayNightCrisisEvidenceService.UnityProofSummaryJsonFileName,
        WorldEventWeatherDayNightCrisisEvidenceService.InvalidDiagnosticsMatrixJsonFileName,
        WorldEventWeatherDayNightCrisisEvidenceService.PreviewExportPayloadJsonFileName,
        WorldEventWeatherDayNightCrisisEvidenceService.ArtifactScopeReportJsonFileName
    ];
}

internal static class WorldEventProjectRootLocator
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
