using System.Text.Json;
using LLMGameCreator.Application.Design.InterlockedGameplaySystemsDepthMatrix;
using Xunit;

namespace LLMGameCreator.Tests.Application.InterlockedGameplaySystemsDepthMatrix;

public sealed class InterlockedGameplaySystemsSourceLoadingTests
{
    [Fact]
    public void SourceLoaderConsumesGoal060ThroughGoal064EvidenceAndPreflightHandoff()
    {
        var source = new InterlockedGameplaySystemsSourceLoader().Load(ProjectRootLocator.ProjectRoot());

        Assert.True(source.Goal064AcceptedByUserHandoff);
        Assert.True(source.Goal060PackageRowsConsumed);
        Assert.True(source.Goal061ReviewRowsConsumed);
        Assert.True(source.Goal062SpatialRowsConsumed);
        Assert.True(source.Goal063GameplayRowsConsumed);
        Assert.True(source.Goal064LivingWorldRowsConsumed);
        Assert.True(source.Goal064UnityProofConsumed);
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
            Assert.True(row.Goal060RuntimeStateChanged);
            Assert.True(row.Goal061SaveLoadReplayVerified);
            Assert.True(row.Goal062Reachable);
            Assert.True(row.Goal062RouteVerified);
            Assert.True(row.Goal063StateChanging);
            Assert.True(row.Goal063SaveLoadReplayPassed);
            Assert.True(row.Goal064StateChanging);
            Assert.True(row.Goal064SaveLoadReplayPassed);
            Assert.False(string.IsNullOrWhiteSpace(row.PackageHash));
            Assert.False(string.IsNullOrWhiteSpace(row.SpatialDetailRowHash));
            Assert.False(string.IsNullOrWhiteSpace(row.GameplayRowHash));
            Assert.False(string.IsNullOrWhiteSpace(row.LivingWorldRowHash));
        });
    }
}

public sealed class InterlockedGameplaySystemsDepthMatrixTests
{
    [Fact]
    public void BuildCreatesNineStateChangingEconomyCraftingCombatProgressionStatusRows()
    {
        var result = new InterlockedGameplaySystemsEvidenceService().Build(ProjectRootLocator.ProjectRoot());

        Assert.True(result.SourceManifest.Goal064AcceptedByUserHandoff);
        Assert.True(result.RuleCatalog.Passed);
        Assert.True(result.RowPlanMatrix.Passed);
        Assert.Equal(9, result.RowPlanMatrix.RowCount);
        Assert.Equal(9, result.RowPlanMatrix.StateChangingRowCount);
        Assert.Equal(3, result.RowPlanMatrix.FamilyCount);
        Assert.Equal(3, result.RowPlanMatrix.SeedCount);
        Assert.Equal(9, result.RowPlanMatrix.DistinctRowHashCount);
        AssertFamily(result, "map_panel_rpg", "trade/work", "conflict", "social");
        AssertFamily(result, "survival_sandbox", "hazard", "resource", "condition");
        AssertFamily(result, "first_person_grid_dungeon", "encounter", "key", "blocked/valid movement");
        Assert.All(result.Rows, row =>
        {
            Assert.True(row.StateChanging);
            Assert.True(row.Steps.Count >= 7);
            Assert.True(row.Deltas.Count >= 7);
            Assert.NotEqual(row.BeforeState.StateHash, row.AfterState.StateHash);
            Assert.All(InterlockedGameplaySystemsRuleCatalogBuilder.RequiredCategories(), category =>
                Assert.Contains(row.Deltas, delta => delta.Category == category && delta.Passed));
            Assert.All(row.Deltas, delta =>
            {
                Assert.NotEmpty(delta.SourceRefs);
                Assert.False(string.IsNullOrWhiteSpace(delta.CausalTrace));
                Assert.False(string.IsNullOrWhiteSpace(delta.Outcome));
            });
        });
    }

    private static void AssertFamily(InterlockedGameplaySystemsBuildResult result, string familyId, params string[] expectedFragments)
    {
        var rows = result.Rows.Where(item => item.FamilyId == familyId).ToList();
        Assert.Equal(3, rows.Count);
        foreach (var fragment in expectedFragments)
        {
            Assert.All(rows, row =>
            {
                var haystack = string.Join("|", row.Deltas.Select(item => item.Outcome));
                Assert.Contains(fragment, haystack, StringComparison.Ordinal);
            });
        }
    }
}

public sealed class InterlockedGameplayReplayAndVarianceTests
{
    [Fact]
    public void SaveLoadReplayAndMeaningfulVariancePassForEveryFamilySeedRow()
    {
        var result = new InterlockedGameplaySystemsEvidenceService().Build(ProjectRootLocator.ProjectRoot());

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
        Assert.Equal(3, result.VarianceMetrics.DistinctRuleSetCount);
        Assert.All(result.VarianceMetrics.Families, family =>
        {
            Assert.Equal(3, family.RowCount);
            Assert.True(family.SameFamilySeedVariationPassed);
            Assert.True(family.MeaningfulAxes.Count >= 7);
            Assert.Equal(3, family.RowHashes.Count);
        });
    }
}

public sealed class InterlockedGameplayUnityPlanTests
{
    [Fact]
    public void UnityCommandPlanRequiresInterlockedMarkersForAllRows()
    {
        var result = new InterlockedGameplaySystemsEvidenceService().Build(ProjectRootLocator.ProjectRoot());

        Assert.True(result.UnityCommandPlan.Passed);
        Assert.False(result.UnityCommandPlan.Accepted);
        Assert.Equal(9, result.UnityCommandPlan.Rows.Count);
        Assert.Contains("interlocked_gameplay_loaded=true", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("interlocked_gameplay_completed=true", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("review_package_proof=goal065", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("interlocked_gameplay_systems_depth_matrix_verification=required", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.All(result.UnityCommandPlan.Rows, row =>
        {
            Assert.NotEmpty(row.EconomyDeltaIds);
            Assert.NotEmpty(row.CraftingDeltaIds);
            Assert.NotEmpty(row.CombatDeltaIds);
            Assert.NotEmpty(row.ProgressionDeltaIds);
            Assert.NotEmpty(row.StatusDeltaIds);
            Assert.Contains("interlocked_gameplay_row=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("interlocked_economy_delta=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("interlocked_crafting_delta=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("interlocked_combat_delta=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("interlocked_progression_delta=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("interlocked_status_delta=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("interlocked_replay_verified=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("interlocked_gameplay_row_completed=" + row.RowId, row.ExpectedPlayerMarkers);
        });
    }
}

public sealed class InterlockedGameplayInvalidMatrixTests
{
    [Fact]
    public void InvalidFakeLeakAndScopeMatrixCoversRequiredCases()
    {
        var matrix = new InterlockedGameplaySystemsEvidenceService().Build(ProjectRootLocator.ProjectRoot()).InvalidMatrix;
        var ids = matrix.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        foreach (var required in InterlockedGameplaySystemsDepthMatrixVocabulary.RequiredInvalidScenarioIds)
        {
            Assert.Contains(required, ids);
        }

        Assert.All(matrix.Scenarios, scenario =>
        {
            Assert.Equal(scenario.ExpectedStatus, scenario.ActualStatus);
            Assert.NotEmpty(scenario.Diagnostics);
            Assert.All(scenario.Diagnostics, diagnostic => Assert.StartsWith("goal065.", diagnostic.Code, StringComparison.Ordinal));
        });
    }
}

public sealed class InterlockedGameplayEvidenceWriteTests
{
    [Fact]
    public async Task WriteAsyncEmitsDeterministicArtifactsRowsAndStagingCommandPlan()
    {
        var service = new InterlockedGameplaySystemsEvidenceService();
        var result = service.Build(ProjectRootLocator.ProjectRoot());
        var second = service.Build(ProjectRootLocator.ProjectRoot());
        var tempRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "Goal065Write", Guid.NewGuid().ToString("N"));

        try
        {
            var write = await service.WriteAsync(tempRoot, result);
            Assert.Equal(result.Report.RowPlanMatrixHash, second.Report.RowPlanMatrixHash);
            Assert.Equal(result.Report.EconomyCraftingLedgerHash, second.Report.EconomyCraftingLedgerHash);
            Assert.Equal(result.Report.CombatProgressionLedgerHash, second.Report.CombatProgressionLedgerHash);
            Assert.Equal(result.Report.StatusEffectLedgerHash, second.Report.StatusEffectLedgerHash);
            Assert.Equal(result.Report.SaveLoadReplayProofHash, second.Report.SaveLoadReplayProofHash);
            Assert.Equal(result.Report.VarianceMetricsHash, second.Report.VarianceMetricsHash);
            Assert.Equal(result.Report.InvalidMatrixHash, second.Report.InvalidMatrixHash);
            Assert.DoesNotContain(result.Report.Diagnostics, item => item.Severity == "error" && !item.Code.StartsWith("goal065.unity.", StringComparison.Ordinal));

            foreach (var fileName in RequiredJsonFiles())
            {
                var path = Path.Combine(write.OutputDirectoryPath, fileName);
                Assert.True(File.Exists(path), "Missing artifact: " + fileName);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            foreach (var row in result.Rows)
            {
                var path = Path.Combine(write.OutputDirectoryPath, InterlockedGameplaySystemsEvidenceService.RowFileName(row));
                Assert.True(File.Exists(path), "Missing row artifact: " + path);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            Assert.True(File.Exists(write.ReportMarkdownPath));
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, InterlockedGameplaySystemsDepthMatrixVocabulary.StagingRoot, InterlockedGameplaySystemsDepthMatrixVocabulary.UnityInterlockedCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
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
        InterlockedGameplaySystemsEvidenceService.SourceManifestJsonFileName,
        InterlockedGameplaySystemsEvidenceService.RuleCatalogJsonFileName,
        InterlockedGameplaySystemsEvidenceService.RowPlanMatrixJsonFileName,
        InterlockedGameplaySystemsEvidenceService.EconomyCraftingLedgerJsonFileName,
        InterlockedGameplaySystemsEvidenceService.CombatProgressionLedgerJsonFileName,
        InterlockedGameplaySystemsEvidenceService.StatusEffectLedgerJsonFileName,
        InterlockedGameplaySystemsEvidenceService.SaveLoadReplayProofJsonFileName,
        InterlockedGameplaySystemsEvidenceService.VarianceMetricsJsonFileName,
        InterlockedGameplaySystemsEvidenceService.UnityCommandPlanJsonFileName,
        InterlockedGameplaySystemsEvidenceService.UnityProofSummaryJsonFileName,
        InterlockedGameplaySystemsEvidenceService.PreviewExportGameplayPayloadJsonFileName,
        InterlockedGameplaySystemsEvidenceService.InvalidDiagnosticsMatrixJsonFileName,
        InterlockedGameplaySystemsEvidenceService.ArtifactScopeReportJsonFileName
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
