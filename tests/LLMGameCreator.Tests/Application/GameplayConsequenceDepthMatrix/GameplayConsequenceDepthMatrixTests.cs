using System.Text.Json;
using LLMGameCreator.Application.Design.GameplayConsequenceDepthMatrix;
using Xunit;

namespace LLMGameCreator.Tests.Application.GameplayConsequenceDepthMatrix;

public sealed class GameplayConsequenceDepthMatrixSourceLoadingTests
{
    [Fact]
    public void SourceLoaderConsumesGoal060Goal061AndGoal062Evidence()
    {
        var source = new GameplayConsequenceDepthMatrixSourceLoader().Load(ProjectRootLocator.ProjectRoot());

        Assert.True(source.Goal060AcceptedByUserHandoff);
        Assert.True(source.Goal061AcceptedByUserHandoff);
        Assert.True(source.Goal062AcceptedByUserHandoff);
        Assert.True(source.Goal060PackageRowsConsumed);
        Assert.True(source.Goal061ReviewRowsConsumed);
        Assert.True(source.Goal062SpatialRowsConsumed);
        Assert.True(source.Goal060RuntimeProofConsumed);
        Assert.True(source.Goal061SaveLoadReplayConsumed);
        Assert.Equal(9, source.Rows.Count);
        Assert.Equal(3, source.FamilyIds.Count);
        Assert.Equal(3, source.SeedIds.Count);
        Assert.All(source.Rows, row =>
        {
            Assert.StartsWith("matrix-row-", row.RowId, StringComparison.Ordinal);
            Assert.StartsWith("Goal060:", row.SourcePackageRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal061:", row.SourceReviewPackageRowRef, StringComparison.Ordinal);
            Assert.StartsWith("Goal062:", row.SourceSpatialDetailRowRef, StringComparison.Ordinal);
            Assert.True(row.Goal060RuntimeStateChanged);
            Assert.True(row.Goal060SaveLoadRoundtripPassed);
            Assert.True(row.Goal061SaveLoadReplayVerified);
            Assert.True(row.Goal062Reachable);
            Assert.True(row.Goal062RouteVerified);
            Assert.False(string.IsNullOrWhiteSpace(row.PackageHash));
            Assert.False(string.IsNullOrWhiteSpace(row.SpatialDetailRowHash));
        });
    }
}

public sealed class GameplayConsequenceDepthMatrixPlanTests
{
    [Fact]
    public void BuildCreatesNineRowsWithRequiredFamilyConsequenceShapes()
    {
        var result = new GameplayConsequenceDepthMatrixEvidenceService().Build(ProjectRootLocator.ProjectRoot());

        Assert.True(result.Catalog.Passed);
        Assert.True(result.CommandPlanMatrix.Passed);
        Assert.Equal(9, result.CommandPlanMatrix.RowCount);
        Assert.Equal(3, result.CommandPlanMatrix.FamilyCount);
        Assert.Equal(3, result.CommandPlanMatrix.SeedCount);
        AssertFamily(result, "map_panel_rpg", "travel/detail", "quest/npc_event", "inventory/reward", "faction/social");
        AssertFamily(result, "survival_sandbox", "survival/hazard_pressure", "survival/resource_collect", "survival/craft_mitigation", "survival/recover");
        AssertFamily(result, "first_person_grid_dungeon", "grid/traverse", "grid/blocked_move", "encounter/pressure", "progression/unlock");
        Assert.All(result.CommandPlanMatrix.Rows, row =>
        {
            Assert.True(row.StateChangingStepCount >= 3);
            Assert.All(row.Commands, command =>
            {
                Assert.NotEmpty(command.ExpectedChanges);
                Assert.False(string.IsNullOrWhiteSpace(command.DeltaId));
            });
        });
    }

    private static void AssertFamily(GameplayConsequenceDepthMatrixBuildResult result, string familyId, params string[] commandTypes)
    {
        var rows = result.CommandPlanMatrix.Rows.Where(item => item.FamilyId == familyId).ToList();
        Assert.Equal(3, rows.Count);
        foreach (var commandType in commandTypes)
        {
            Assert.All(rows, row => Assert.Contains(row.Commands, command => command.CommandType == commandType));
        }
    }
}

public sealed class GameplayConsequenceDepthMatrixStateDeltaTests
{
    [Fact]
    public void StateDeltaProofHasBeforeAfterExpectedActualAndMeaningfulChanges()
    {
        var result = new GameplayConsequenceDepthMatrixEvidenceService().Build(ProjectRootLocator.ProjectRoot());

        Assert.True(result.RuntimeStateDeltaMatrix.Passed);
        Assert.Equal(9, result.RuntimeStateDeltaMatrix.StateChangingRowCount);
        Assert.All(result.RuntimeStateDeltaMatrix.Rows, row =>
        {
            Assert.True(row.StateTransitionProofPassed);
            Assert.NotEqual(row.BeforeState.StateHash, row.AfterState.StateHash);
            Assert.True(row.StateChangingStepCount >= 3);
            Assert.All(row.Transitions, transition =>
            {
                Assert.True(transition.StateChanged);
                Assert.True(transition.ExpectedVsActualPassed);
                Assert.NotEmpty(transition.Deltas);
                Assert.All(transition.Deltas, delta =>
                {
                    Assert.True(delta.Passed);
                    Assert.NotEqual(delta.BeforeValue, delta.AfterValue);
                    Assert.Equal(delta.ExpectedValue, delta.ActualValue);
                });
            });
        });
    }
}

public sealed class GameplayConsequenceDepthMatrixReplayTests
{
    [Fact]
    public void SaveLoadRoundtripAndSameSeedReplayPassForEveryRow()
    {
        var result = new GameplayConsequenceDepthMatrixEvidenceService().Build(ProjectRootLocator.ProjectRoot());

        Assert.True(result.SaveLoadReplayAudit.Passed);
        Assert.Equal(9, result.SaveLoadReplayAudit.SaveLoadPassedRowCount);
        Assert.Equal(9, result.SaveLoadReplayAudit.ReplayPassedRowCount);
        Assert.All(result.SaveLoadReplayAudit.Rows, row =>
        {
            Assert.True(row.SaveLoadRoundtripPassed);
            Assert.True(row.ReplayDeterminismPassed);
            Assert.Equal(row.SerializedAfterStateHash, row.RestoredAfterStateHash);
            Assert.Equal(row.FirstReplayHash, row.SecondReplayHash);
        });
        Assert.True(result.FamilySummary.MeaningfulVariancePassed);
        Assert.All(result.FamilySummary.Families, family =>
        {
            Assert.Equal(3, family.RowHashes.Count);
            Assert.True(family.MeaningfulVarianceAxes.Count >= 5);
        });
    }
}

public sealed class GameplayConsequenceDepthMatrixUnityProofTests
{
    [Fact]
    public void UnityCommandPlanRequiresGoal063MarkersForAllRowsStepsAndDeltas()
    {
        var result = new GameplayConsequenceDepthMatrixEvidenceService().Build(ProjectRootLocator.ProjectRoot());

        Assert.True(result.UnityCommandPlan.Passed);
        Assert.Equal(9, result.UnityCommandPlan.Rows.Count);
        Assert.Contains("gameplay_consequence_goal=goal063", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("gameplay_consequence_matrix_completed=true", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("gameplay_consequence_depth_matrix_verification=required", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.All(result.UnityCommandPlan.Rows, row =>
        {
            Assert.True(row.StepIds.Count >= 3);
            Assert.True(row.DeltaIds.Count >= 3);
            Assert.Contains("gameplay_consequence_row=" + row.FamilyId + "/" + row.SeedId, row.ExpectedPlayerMarkers);
            Assert.Contains("gameplay_consequence_completed=" + row.FamilyId + "/" + row.SeedId, row.ExpectedPlayerMarkers);
            Assert.All(row.StepIds, step => Assert.Contains("gameplay_consequence_step=" + step, row.ExpectedPlayerMarkers));
            Assert.All(row.DeltaIds, delta => Assert.Contains("gameplay_consequence_delta=" + delta, row.ExpectedPlayerMarkers));
        });
    }
}

public sealed class GameplayConsequenceDepthMatrixInvalidMatrixTests
{
    [Fact]
    public void InvalidFakeAndLeakMatrixCoversRequiredCases()
    {
        var matrix = new GameplayConsequenceDepthMatrixEvidenceService().Build(ProjectRootLocator.ProjectRoot()).InvalidMatrix;
        var ids = matrix.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        foreach (var required in GameplayConsequenceDepthMatrixVocabulary.RequiredInvalidScenarioIds)
        {
            Assert.Contains(required, ids);
        }

        Assert.All(matrix.Scenarios, scenario =>
        {
            Assert.Equal(scenario.ExpectedStatus, scenario.ActualStatus);
            Assert.NotEmpty(scenario.Diagnostics);
            Assert.All(scenario.Diagnostics, diagnostic => Assert.StartsWith("goal063.", diagnostic.Code, StringComparison.Ordinal));
        });
    }
}

public sealed class GameplayConsequenceDepthMatrixEvidenceTests
{
    [Fact]
    public async Task WriteAsyncEmitsRequiredDeterministicJsonArtifactsAndRowProofFiles()
    {
        var service = new GameplayConsequenceDepthMatrixEvidenceService();
        var result = service.Build(ProjectRootLocator.ProjectRoot());
        var second = service.Build(ProjectRootLocator.ProjectRoot());
        var tempRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "Goal063Write", Guid.NewGuid().ToString("N"));

        try
        {
            var write = await service.WriteAsync(tempRoot, result);
            Assert.Equal(result.Report.CommandPlanHash, second.Report.CommandPlanHash);
            Assert.Equal(result.Report.RuntimeStateDeltaMatrixHash, second.Report.RuntimeStateDeltaMatrixHash);
            Assert.Equal(result.Report.SaveLoadReplayAuditHash, second.Report.SaveLoadReplayAuditHash);
            Assert.Equal(result.Report.InvalidMatrixHash, second.Report.InvalidMatrixHash);

            foreach (var fileName in RequiredJsonFiles())
            {
                var path = Path.Combine(write.OutputDirectoryPath, fileName);
                Assert.True(File.Exists(path), "Missing artifact: " + fileName);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            var rowFiles = Directory.EnumerateFiles(Path.Combine(write.OutputDirectoryPath, GameplayConsequenceDepthMatrixEvidenceService.RowsDirectoryName), "*-gameplay-proof.json", SearchOption.TopDirectoryOnly).ToList();
            Assert.Equal(9, rowFiles.Count);
            foreach (var rowFile in rowFiles)
            {
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(rowFile));
            }

            Assert.True(File.Exists(write.ReportMarkdownPath));
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, GameplayConsequenceDepthMatrixVocabulary.StagingRoot, GameplayConsequenceDepthMatrixVocabulary.UnityGameplayCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
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
        GameplayConsequenceDepthMatrixEvidenceService.SourceManifestJsonFileName,
        GameplayConsequenceDepthMatrixEvidenceService.CatalogJsonFileName,
        GameplayConsequenceDepthMatrixEvidenceService.CommandPlanMatrixJsonFileName,
        GameplayConsequenceDepthMatrixEvidenceService.RuntimeStateDeltaMatrixJsonFileName,
        GameplayConsequenceDepthMatrixEvidenceService.SaveLoadReplayAuditJsonFileName,
        GameplayConsequenceDepthMatrixEvidenceService.FamilyConsequenceSummaryJsonFileName,
        GameplayConsequenceDepthMatrixEvidenceService.UnityCommandPlanJsonFileName,
        GameplayConsequenceDepthMatrixEvidenceService.UnityProofSummaryJsonFileName,
        GameplayConsequenceDepthMatrixEvidenceService.PreviewExportGameplayPayloadJsonFileName,
        GameplayConsequenceDepthMatrixEvidenceService.InvalidDiagnosticsMatrixJsonFileName,
        GameplayConsequenceDepthMatrixEvidenceService.ArtifactScopeReportJsonFileName
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
