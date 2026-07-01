using System.Text.Json;
using LLMGameCreator.Application.Design.LivingWorldNpcFactionSimulationMatrix;
using Xunit;

namespace LLMGameCreator.Tests.Application.LivingWorldNpcFactionSimulationMatrix;

public sealed class LivingWorldNpcFactionSimulationSourceLoadingTests
{
    [Fact]
    public void SourceLoaderConsumesGoal060ThroughGoal063EvidenceAndPreflightHandoff()
    {
        var source = new LivingWorldNpcFactionSimulationSourceLoader().Load(ProjectRootLocator.ProjectRoot());

        Assert.True(source.Goal063AcceptedByUserHandoff);
        Assert.True(source.Goal060PackageRowsConsumed);
        Assert.True(source.Goal061ReviewRowsConsumed);
        Assert.True(source.Goal062SpatialRowsConsumed);
        Assert.True(source.Goal063GameplayRowsConsumed);
        Assert.True(source.Goal063UnityProofConsumed);
        Assert.Equal(9, source.Rows.Count);
        Assert.Equal(3, source.FamilyIds.Count);
        Assert.Equal(3, source.SeedIds.Count);
        Assert.All(source.Rows, row =>
        {
            Assert.StartsWith("Goal060:", row.SourcePackageRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal061:", row.SourceReviewPackageRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal062:", row.SourceSpatialDetailRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal063:", row.SourceGameplayConsequenceRowRef, StringComparison.Ordinal);
            Assert.True(row.Goal060RuntimeStateChanged);
            Assert.True(row.Goal061SaveLoadReplayVerified);
            Assert.True(row.Goal062Reachable);
            Assert.True(row.Goal062RouteVerified);
            Assert.True(row.Goal063StateChanging);
            Assert.True(row.Goal063SaveLoadReplayPassed);
            Assert.False(string.IsNullOrWhiteSpace(row.PackageHash));
            Assert.False(string.IsNullOrWhiteSpace(row.SpatialDetailRowHash));
            Assert.False(string.IsNullOrWhiteSpace(row.Goal063RowHash));
        });
    }
}

public sealed class LivingWorldNpcFactionSimulationMatrixTests
{
    [Fact]
    public void BuildCreatesNineStateChangingNpcFactionAndWorldEventRows()
    {
        var result = new LivingWorldNpcFactionSimulationEvidenceService().Build(ProjectRootLocator.ProjectRoot());

        Assert.True(result.SourceManifest.Goal063AcceptedByUserHandoff);
        Assert.True(result.CatalogSummary.Passed);
        Assert.True(result.SimulationMatrixPlan.Passed);
        Assert.Equal(9, result.SimulationMatrixPlan.RowCount);
        Assert.Equal(9, result.SimulationMatrixPlan.StateChangingRowCount);
        Assert.Equal(3, result.SimulationMatrixPlan.FamilyCount);
        Assert.Equal(3, result.SimulationMatrixPlan.SeedCount);
        Assert.Equal(9, result.SimulationMatrixPlan.DistinctRowHashCount);
        AssertFamily(result, "map_panel_rpg", "quest_rumor_pressure", "actor_schedule_availability", "faction_relationship_reputation", "quest_reward_rumor_pressure");
        AssertFamily(result, "survival_sandbox", "weather_hunger_shelter_danger_recovery", "actor_schedule_availability", "faction_relationship_reputation", "scarcity_resource_shelter_pressure");
        AssertFamily(result, "first_person_grid_dungeon", "alert_loot_progression_spatial_relation", "actor_schedule_availability", "faction_relationship_reputation", "alert_loot_spatial_pressure");
        Assert.All(result.Rows, row =>
        {
            Assert.True(row.ActorRecords.Count >= 2);
            Assert.True(row.FactionRecords.Count >= 2);
            Assert.True(row.RelationshipRecords.Count >= 2);
            Assert.True(row.ScheduleAvailabilityRecords.Count >= 2);
            Assert.NotEmpty(row.WorldEventRecords);
            Assert.True(row.OrderedTickPlan.Count >= 3);
            Assert.True(row.StateDeltaSummary.Count >= 8);
            Assert.NotEqual(row.BeforeState.StateHash, row.AfterState.StateHash);
            Assert.All(row.StateDeltaSummary, delta => Assert.True(delta.Passed));
        });
    }

    private static void AssertFamily(LivingWorldNpcFactionSimulationBuildResult result, string familyId, params string[] expectedFragments)
    {
        var rows = result.Rows.Where(item => item.FamilyId == familyId).ToList();
        Assert.Equal(3, rows.Count);
        foreach (var fragment in expectedFragments)
        {
            Assert.All(rows, row =>
            {
                var haystack = string.Join("|",
                    row.WorldEventRecords.Select(item => item.EventKind)
                        .Concat(row.OrderedTickPlan.Select(item => item.TickKind))
                        .Concat(row.StateDeltaSummary.Select(item => item.Key)));
                Assert.Contains(fragment, haystack, StringComparison.Ordinal);
            });
        }
    }
}

public sealed class LivingWorldNpcFactionReplayAndVarianceTests
{
    [Fact]
    public void SaveLoadReplayAndMeaningfulVariancePassForEveryFamilySeedRow()
    {
        var result = new LivingWorldNpcFactionSimulationEvidenceService().Build(ProjectRootLocator.ProjectRoot());

        Assert.True(result.SaveLoadReplayProof.Passed);
        Assert.Equal(9, result.SaveLoadReplayProof.StateChangedRowCount);
        Assert.Equal(9, result.SaveLoadReplayProof.SaveLoadPassedRowCount);
        Assert.Equal(9, result.SaveLoadReplayProof.ReplayPassedRowCount);
        Assert.All(result.SaveLoadReplayProof.Rows, row =>
        {
            Assert.True(row.BeforeAfterStateChanged);
            Assert.True(row.SaveLoadRoundtripPassed);
            Assert.True(row.ReplayDeterminismPassed);
            Assert.NotEqual(row.BeforeStateHash, row.AfterStateHash);
            Assert.Equal(row.SerializedAfterStateHash, row.RestoredAfterStateHash);
            Assert.Equal(row.FirstReplayHash, row.SecondReplayHash);
        });

        Assert.True(result.VarianceMetrics.Passed);
        Assert.True(result.VarianceMetrics.HashOnlyVarianceRejected);
        Assert.True(result.VarianceMetrics.SameFamilySeedVariationPassed);
        Assert.True(result.VarianceMetrics.CrossFamilyRuleVariationPassed);
        Assert.Equal(9, result.VarianceMetrics.DistinctAfterStateHashCount);
        Assert.Equal(3, result.VarianceMetrics.DistinctRuleProfileCount);
        Assert.All(result.VarianceMetrics.Families, family =>
        {
            Assert.Equal(3, family.RowCount);
            Assert.True(family.SameFamilySeedVariationPassed);
            Assert.True(family.MeaningfulAxes.Count >= 5);
            Assert.Equal(3, family.RowHashes.Count);
        });
    }
}

public sealed class LivingWorldNpcFactionUnityPlanTests
{
    [Fact]
    public void UnityCommandPlanRequiresLivingWorldMarkersForAllRowsAndTicks()
    {
        var result = new LivingWorldNpcFactionSimulationEvidenceService().Build(ProjectRootLocator.ProjectRoot());

        Assert.True(result.UnityCommandPlan.Passed);
        Assert.False(result.UnityCommandPlan.Accepted);
        Assert.Equal(9, result.UnityCommandPlan.Rows.Count);
        Assert.Contains("living_world_matrix_loaded=goal064", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("living_world_matrix_completed=true", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("review_package_proof=goal064", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("living_world_npc_faction_simulation_matrix_verification=required", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.All(result.UnityCommandPlan.Rows, row =>
        {
            Assert.True(row.TickIds.Count >= 3);
            Assert.Contains("living_world_row=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("living_world_family=" + row.FamilyId, row.ExpectedPlayerMarkers);
            Assert.Contains("living_world_seed=" + row.SeedId, row.ExpectedPlayerMarkers);
            Assert.Contains("npc_state_changed=true", row.ExpectedPlayerMarkers);
            Assert.Contains("faction_relation_changed=true", row.ExpectedPlayerMarkers);
            Assert.Contains("world_event_resolved=true", row.ExpectedPlayerMarkers);
            Assert.Contains("living_world_row_completed=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.All(row.TickIds, tick => Assert.Contains("living_world_tick=" + tick, row.ExpectedPlayerMarkers));
        });
    }
}

public sealed class LivingWorldNpcFactionInvalidMatrixTests
{
    [Fact]
    public void InvalidFakeAndLeakMatrixCoversRequiredCases()
    {
        var matrix = new LivingWorldNpcFactionSimulationEvidenceService().Build(ProjectRootLocator.ProjectRoot()).InvalidMatrix;
        var ids = matrix.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        foreach (var required in LivingWorldNpcFactionSimulationVocabulary.RequiredInvalidScenarioIds)
        {
            Assert.Contains(required, ids);
        }

        Assert.All(matrix.Scenarios, scenario =>
        {
            Assert.Equal(scenario.ExpectedStatus, scenario.ActualStatus);
            Assert.NotEmpty(scenario.Diagnostics);
            Assert.All(scenario.Diagnostics, diagnostic => Assert.StartsWith("goal064.", diagnostic.Code, StringComparison.Ordinal));
        });
    }
}

public sealed class LivingWorldNpcFactionEvidenceWriteTests
{
    [Fact]
    public async Task WriteAsyncEmitsDeterministicArtifactsRowsAndStagingCommandPlan()
    {
        var service = new LivingWorldNpcFactionSimulationEvidenceService();
        var result = service.Build(ProjectRootLocator.ProjectRoot());
        var second = service.Build(ProjectRootLocator.ProjectRoot());
        var tempRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "Goal064Write", Guid.NewGuid().ToString("N"));

        try
        {
            var write = await service.WriteAsync(tempRoot, result);
            Assert.Equal(result.Report.SimulationMatrixPlanHash, second.Report.SimulationMatrixPlanHash);
            Assert.Equal(result.Report.SaveLoadReplayProofHash, second.Report.SaveLoadReplayProofHash);
            Assert.Equal(result.Report.VarianceMetricsHash, second.Report.VarianceMetricsHash);
            Assert.Equal(result.Report.InvalidMatrixHash, second.Report.InvalidMatrixHash);
            Assert.DoesNotContain(result.Report.Diagnostics, item => item.Severity == "error" && !item.Code.StartsWith("goal064.unity.", StringComparison.Ordinal));

            foreach (var fileName in RequiredJsonFiles())
            {
                var path = Path.Combine(write.OutputDirectoryPath, fileName);
                Assert.True(File.Exists(path), "Missing artifact: " + fileName);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            var rowFiles = Directory.EnumerateFiles(Path.Combine(write.OutputDirectoryPath, LivingWorldNpcFactionSimulationEvidenceService.RowsDirectoryName), "*-living-world-row.json", SearchOption.TopDirectoryOnly).ToList();
            Assert.Equal(9, rowFiles.Count);
            foreach (var rowFile in rowFiles)
            {
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(rowFile));
            }

            Assert.True(File.Exists(write.ReportMarkdownPath));
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, LivingWorldNpcFactionSimulationVocabulary.StagingRoot, LivingWorldNpcFactionSimulationVocabulary.UnityLivingWorldCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
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
        LivingWorldNpcFactionSimulationEvidenceService.SourceManifestJsonFileName,
        LivingWorldNpcFactionSimulationEvidenceService.CatalogSummaryJsonFileName,
        LivingWorldNpcFactionSimulationEvidenceService.SimulationMatrixPlanJsonFileName,
        LivingWorldNpcFactionSimulationEvidenceService.SaveLoadReplayProofJsonFileName,
        LivingWorldNpcFactionSimulationEvidenceService.VarianceMetricsJsonFileName,
        LivingWorldNpcFactionSimulationEvidenceService.UnityCommandPlanJsonFileName,
        LivingWorldNpcFactionSimulationEvidenceService.UnityProofSummaryJsonFileName,
        LivingWorldNpcFactionSimulationEvidenceService.PreviewExportPayloadJsonFileName,
        LivingWorldNpcFactionSimulationEvidenceService.InvalidDiagnosticsMatrixJsonFileName,
        LivingWorldNpcFactionSimulationEvidenceService.ArtifactScopeReportJsonFileName
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
