using System.Text.Json;
using LLMGameCreator.Application.Design.SettlementConstructionDestructionProductionMatrix;
using Xunit;

namespace LLMGameCreator.Tests.Application.SettlementConstructionDestructionProductionMatrix;

public sealed class SettlementConstructionSourceLoadingTests
{
    [Fact]
    public void SourceLoaderConsumesGoal060ThroughGoal065EvidenceAndPreflightHandoff()
    {
        var source = new SettlementConstructionDestructionProductionSourceLoader().Load(ProjectRootLocator.ProjectRoot());

        Assert.True(source.Goal065AcceptedByUserHandoff);
        Assert.True(source.Goal060PackageRowsConsumed);
        Assert.True(source.Goal061ReviewRowsConsumed);
        Assert.True(source.Goal062SpatialRowsConsumed);
        Assert.True(source.Goal063GameplayRowsConsumed);
        Assert.True(source.Goal064LivingWorldRowsConsumed);
        Assert.True(source.Goal065InterlockedRowsConsumed);
        Assert.True(source.Goal065UnityProofConsumed);
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
            Assert.True(row.Goal060RuntimeStateChanged);
            Assert.True(row.Goal061SaveLoadReplayVerified);
            Assert.True(row.Goal062Reachable);
            Assert.True(row.Goal063StateChanging);
            Assert.True(row.Goal064StateChanging);
            Assert.True(row.Goal065StateChanging);
            Assert.True(row.Goal065SaveLoadReplayPassed);
        });
    }
}

public sealed class SettlementConstructionDestructionProductionMatrixTests
{
    [Fact]
    public void BuildCreatesNineStateChangingSettlementRowsWithLedgers()
    {
        var result = new SettlementConstructionDestructionProductionEvidenceService().Build(ProjectRootLocator.ProjectRoot());

        Assert.True(result.SourceManifest.Goal065AcceptedByUserHandoff);
        Assert.True(result.SourceManifest.Goal060PackageRowsConsumed);
        Assert.True(result.SourceManifest.Goal061ReviewRowsConsumed);
        Assert.True(result.SourceManifest.Goal062SpatialRowsConsumed);
        Assert.True(result.SourceManifest.Goal063GameplayRowsConsumed);
        Assert.True(result.SourceManifest.Goal064LivingWorldRowsConsumed);
        Assert.True(result.SourceManifest.Goal065InterlockedRowsConsumed);
        Assert.True(result.BuildingCatalog.Passed);
        Assert.True(result.RowMatrix.Passed);
        Assert.Equal(9, result.RowMatrix.RowCount);
        Assert.Equal(9, result.RowMatrix.StateChangingRowCount);
        Assert.Equal(3, result.RowMatrix.FamilyCount);
        Assert.Equal(3, result.RowMatrix.SeedCount);
        Assert.Equal(9, result.RowMatrix.DistinctRowHashCount);
        Assert.True(result.ProductionLedger.Passed);
        Assert.True(result.DestructionRepairLedger.Passed);
        Assert.True(result.DefenseThreatLedger.Passed);
        Assert.True(result.LivingWorldLinkage.Passed);
        Assert.Equal(9, result.Rows.Select(row => row.SettlementId).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(9, result.Rows.Select(row => row.BuildingId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(result.Rows, row =>
        {
            Assert.True(row.StateChanging);
            Assert.NotEqual(row.BeforeState.StateHash, row.AfterState.StateHash);
            Assert.True(row.ConstructionAction.Passed);
            Assert.True(row.ProductionAction.Passed);
            Assert.True(row.DamageDestructionThreatEvent.Passed);
            Assert.True(row.RepairUpgradeDefenseResponse.Passed);
            Assert.True(row.LivingWorldConsequence.Passed);
            Assert.True(row.InterlockedGameplayDependency.Passed);
            Assert.NotEmpty(row.ConstructionCostLedger);
            Assert.NotEmpty(row.ProductionOutputLedger);
            Assert.NotEmpty(row.MeaningfulVarianceAxes);
        });
    }
}

public sealed class SettlementConstructionReplayAndUnityPlanTests
{
    [Fact]
    public void SaveLoadReplayVarianceAndUnityCommandPlanPassForEveryFamilySeedRow()
    {
        var result = new SettlementConstructionDestructionProductionEvidenceService().Build(ProjectRootLocator.ProjectRoot());

        Assert.True(result.SaveLoadReplayProof.Passed);
        Assert.Equal(9, result.SaveLoadReplayProof.StateChangedRowCount);
        Assert.Equal(9, result.SaveLoadReplayProof.SaveLoadPassedRowCount);
        Assert.Equal(9, result.SaveLoadReplayProof.ReplayPassedRowCount);
        Assert.True(result.Report.MeaningfulVariancePassed);
        Assert.True(result.UnityCommandPlan.Passed);
        Assert.False(result.UnityCommandPlan.Accepted);
        Assert.Equal(9, result.UnityCommandPlan.Rows.Count);
        foreach (var marker in SettlementConstructionDestructionProductionValidator.RequiredUnityMarkers())
        {
            Assert.Contains(marker, result.UnityCommandPlan.ExpectedPlayerMarkers);
        }

        Assert.All(result.UnityCommandPlan.Rows, row =>
        {
            Assert.NotEmpty(row.ProductionLedgerEntryIds);
            Assert.NotEmpty(row.DestructionRepairLedgerEntryIds);
            Assert.NotEmpty(row.DefenseThreatLedgerEntryIds);
            Assert.Contains("settlement_row=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("settlement_construction_action=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("settlement_production_delta=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("settlement_destruction_damage=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("settlement_repair_defense=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("settlement_living_world_linkage=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("settlement_interlocked_dependency=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("settlement_replay_verified=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("settlement_row_completed=" + row.RowId, row.ExpectedPlayerMarkers);
        });
    }
}

public sealed class SettlementConstructionInvalidMatrixTests
{
    [Fact]
    public void InvalidFakeLeakAndScopeMatrixCoversRequiredCases()
    {
        var matrix = new SettlementConstructionDestructionProductionEvidenceService().Build(ProjectRootLocator.ProjectRoot()).InvalidMatrix;
        var ids = matrix.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        foreach (var required in SettlementConstructionDestructionProductionVocabulary.RequiredInvalidScenarioIds)
        {
            Assert.Contains(required, ids);
        }

        Assert.All(matrix.Scenarios, scenario =>
        {
            Assert.Equal(scenario.ExpectedStatus, scenario.ActualStatus);
            Assert.NotEmpty(scenario.Diagnostics);
            Assert.All(scenario.Diagnostics, diagnostic => Assert.StartsWith("goal066.", diagnostic.Code, StringComparison.Ordinal));
        });
    }
}

public sealed class SettlementConstructionEvidenceWriteTests
{
    [Fact]
    public async Task WriteAsyncEmitsDeterministicArtifactsRowsAndStagingCommandPlan()
    {
        var service = new SettlementConstructionDestructionProductionEvidenceService();
        var result = service.Build(ProjectRootLocator.ProjectRoot());
        var second = service.Build(ProjectRootLocator.ProjectRoot());
        var tempRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "Goal066Write", Guid.NewGuid().ToString("N"));

        try
        {
            var write = await service.WriteAsync(tempRoot, result);
            Assert.Equal(result.Report.RowMatrixHash, second.Report.RowMatrixHash);
            Assert.Equal(result.Report.ProductionLedgerHash, second.Report.ProductionLedgerHash);
            Assert.Equal(result.Report.DestructionRepairLedgerHash, second.Report.DestructionRepairLedgerHash);
            Assert.Equal(result.Report.DefenseThreatLedgerHash, second.Report.DefenseThreatLedgerHash);
            Assert.Equal(result.Report.LivingWorldLinkageHash, second.Report.LivingWorldLinkageHash);
            Assert.Equal(result.Report.SaveLoadReplayProofHash, second.Report.SaveLoadReplayProofHash);
            Assert.Equal(result.Report.InvalidMatrixHash, second.Report.InvalidMatrixHash);
            Assert.DoesNotContain(result.Report.Diagnostics, item => item.Severity == "error" && !item.Code.StartsWith("goal066.unity.", StringComparison.Ordinal));

            foreach (var fileName in RequiredJsonFiles())
            {
                var path = Path.Combine(write.OutputDirectoryPath, fileName);
                Assert.True(File.Exists(path), "Missing artifact: " + fileName);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            foreach (var row in result.Rows)
            {
                var path = Path.Combine(write.OutputDirectoryPath, SettlementConstructionDestructionProductionEvidenceService.RowsDirectoryName, SettlementConstructionDestructionProductionEvidenceService.RowFileName(row));
                Assert.True(File.Exists(path), "Missing row artifact: " + path);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            Assert.True(File.Exists(write.ReportMarkdownPath));
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, SettlementConstructionDestructionProductionVocabulary.StagingRoot, SettlementConstructionDestructionProductionVocabulary.UnitySettlementCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
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
        SettlementConstructionDestructionProductionEvidenceService.SourceManifestJsonFileName,
        SettlementConstructionDestructionProductionEvidenceService.RowMatrixJsonFileName,
        SettlementConstructionDestructionProductionEvidenceService.BuildingCatalogJsonFileName,
        SettlementConstructionDestructionProductionEvidenceService.ProductionLedgerJsonFileName,
        SettlementConstructionDestructionProductionEvidenceService.DestructionRepairLedgerJsonFileName,
        SettlementConstructionDestructionProductionEvidenceService.DefenseThreatLedgerJsonFileName,
        SettlementConstructionDestructionProductionEvidenceService.LivingWorldLinkageJsonFileName,
        SettlementConstructionDestructionProductionEvidenceService.SaveLoadReplayProofJsonFileName,
        SettlementConstructionDestructionProductionEvidenceService.UnityCommandPlanJsonFileName,
        SettlementConstructionDestructionProductionEvidenceService.UnityProofSummaryJsonFileName,
        SettlementConstructionDestructionProductionEvidenceService.InvalidDiagnosticsMatrixJsonFileName,
        SettlementConstructionDestructionProductionEvidenceService.PreviewExportPayloadJsonFileName,
        SettlementConstructionDestructionProductionEvidenceService.ArtifactScopeReportJsonFileName
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
