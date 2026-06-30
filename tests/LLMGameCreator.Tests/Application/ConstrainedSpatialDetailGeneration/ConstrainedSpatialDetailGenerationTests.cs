using LLMGameCreator.Application.Design.ConstrainedSpatialDetailGeneration;
using Xunit;

namespace LLMGameCreator.Tests.Application.ConstrainedSpatialDetailGeneration;

public sealed class ConstrainedSpatialDetailGenerationTests
{
    [Fact]
    public void SourceLoaderConsumesGoal061Goal060AndGoal059Evidence()
    {
        var source = new ConstrainedSpatialDetailSourceLoader().Load(ProjectRoot());

        Assert.True(source.Goal061AcceptedByUserHandoff);
        Assert.True(source.Goal061ReviewPackageRcManifestPassed);
        Assert.True(source.Goal061UnityProofPassed);
        Assert.True(source.Goal060PackageInventoryConsumed);
        Assert.True(source.Goal059VarianceConsumed);
        Assert.Equal(9, source.PackageRows.Count);
        Assert.Equal(3, source.FamilyIds.Count);
        Assert.Equal(3, source.SeedIds.Count);
        Assert.All(source.PackageRows, row =>
        {
            Assert.StartsWith("matrix-row-", row.RowId, StringComparison.Ordinal);
            Assert.StartsWith("game/goal060/", row.PackageId, StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(row.PackageHash));
            Assert.False(string.IsNullOrWhiteSpace(row.Goal059DerivedCampaignHash));
            Assert.NotEmpty(row.ReviewPackageCommandSteps);
        });
    }

    [Fact]
    public void PaletteAndRuleCatalogsCoverRequiredFamilySemantics()
    {
        var palette = new ConstrainedSpatialPaletteCatalogBuilder().Build();
        var rewrite = new ConstrainedSpatialRewriteRuleCatalogBuilder().Build();
        var constraints = new ConstrainedSpatialConstraintPlanner().BuildConstraintRuleCatalog(palette);

        Assert.True(palette.Passed);
        Assert.True(rewrite.Passed);
        Assert.True(constraints.Passed);
        Assert.Contains(palette.Tiles, tile => tile.SemanticTags.Contains("quest") && tile.FamilyApplicability.Contains("map_panel_rpg"));
        Assert.Contains(palette.Tiles, tile => tile.SemanticTags.Contains("hazard") && tile.FamilyApplicability.Contains("survival_sandbox"));
        Assert.Contains(palette.Tiles, tile => tile.SemanticTags.Contains("door") && tile.FamilyApplicability.Contains("first_person_grid_dungeon"));
        Assert.All(palette.Tiles, tile => Assert.Equal("in_house_fixture", tile.Provenance));
        Assert.Contains(rewrite.Rules, rule => rule.RuleId.Contains("connect_critical", StringComparison.Ordinal));
        Assert.Contains(rewrite.Rules, rule => rule.RuleId.Contains("repair_isolated", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildCreatesNineReachableRowsWithMeaningfulVariance()
    {
        var result = new ConstrainedSpatialDetailEvidenceService().Build(ProjectRoot());

        Assert.True(result.SourceManifest.Goal061AcceptedByUserHandoff);
        Assert.True(result.SpatialDetailMatrix.Passed);
        Assert.True(result.ReachabilityProofMatrix.Passed);
        Assert.True(result.RepairFallbackMatrix.Passed);
        Assert.True(result.PreviewExportPayload.Passed);
        Assert.Equal(9, result.SpatialDetailRows.Count);
        Assert.Equal(9, result.SpatialDetailMatrix.DistinctRowHashCount);
        Assert.True(result.SpatialDetailMatrix.SameFamilyRowsDifferByTwoMetrics);
        Assert.All(result.SpatialDetailRows, row =>
        {
            Assert.True(row.ReachabilityProof.Reachable);
            Assert.True(row.ReachabilityProof.EntryToObjective.RouteVerified);
            Assert.True(row.ReachabilityProof.ObjectiveToExit.RouteVerified);
            Assert.True(row.ReachabilityProof.FamilySpecificRoute.RouteVerified);
            Assert.Contains(row.Anchors, anchor => anchor.AnchorId == "entry");
            Assert.Contains(row.Anchors, anchor => anchor.AnchorId == "objective");
            Assert.Contains(row.Anchors, anchor => anchor.AnchorId == "exit");
            Assert.False(string.IsNullOrWhiteSpace(row.VarianceMetrics.VarianceMarker));
            Assert.NotEmpty(row.TileDataCompact);
        });
    }

    [Fact]
    public void UnityCommandPlanRequiresSpatialMarkersForEveryRow()
    {
        var result = new ConstrainedSpatialDetailEvidenceService().Build(ProjectRoot());

        Assert.True(result.UnityCommandPlan.Passed);
        Assert.Equal(9, result.UnityCommandPlan.Rows.Count);
        Assert.Contains("spatial_detail_loaded=true", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("review_package_proof=goal062", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("constrained_spatial_detail_generation_verification=required", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.All(result.UnityCommandPlan.Rows, row =>
        {
            Assert.True(row.Reachable);
            Assert.True(row.RouteVerified);
            Assert.Contains("spatial_detail_family=" + row.FamilyId, row.ExpectedPlayerMarkers);
            Assert.Contains("spatial_detail_seed=" + row.SeedId, row.ExpectedPlayerMarkers);
            Assert.Contains("spatial_detail_row=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("spatial_detail_reachable=true", row.ExpectedPlayerMarkers);
            Assert.Contains("spatial_detail_route_verified=true", row.ExpectedPlayerMarkers);
            Assert.Contains("spatial_detail_variance_marker=" + row.VarianceMarker, row.ExpectedPlayerMarkers);
        });
    }

    [Fact]
    public void InvalidMatrixCoversRequiredFakeLeakCases()
    {
        var result = new ConstrainedSpatialDetailEvidenceService().Build(ProjectRoot());
        var ids = result.InvalidMatrix.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);

        Assert.True(result.InvalidMatrix.Passed);
        foreach (var required in ConstrainedSpatialDetailVocabulary.RequiredInvalidScenarioIds)
        {
            Assert.Contains(required, ids);
        }

        Assert.All(result.InvalidMatrix.Scenarios, scenario =>
        {
            Assert.Equal(scenario.ExpectedStatus, scenario.ActualStatus);
            Assert.NotEmpty(scenario.Diagnostics);
            Assert.All(scenario.Diagnostics, diagnostic => Assert.StartsWith("goal062.", diagnostic.Code, StringComparison.Ordinal));
        });
    }

    [Fact]
    public async Task WriteAsyncEmitsRequiredArtifactsAndNineRowFiles()
    {
        var service = new ConstrainedSpatialDetailEvidenceService();
        var result = service.Build(ProjectRoot());
        var tempRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "Goal062Write", Guid.NewGuid().ToString("N"));
        try
        {
            var write = await service.WriteAsync(tempRoot, result);
            var output = write.OutputDirectoryPath;
            Assert.True(File.Exists(Path.Combine(output, ConstrainedSpatialDetailEvidenceService.SourceManifestJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, ConstrainedSpatialDetailEvidenceService.PaletteCatalogJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, ConstrainedSpatialDetailEvidenceService.RewriteRuleCatalogJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, ConstrainedSpatialDetailEvidenceService.ConstraintRuleCatalogJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, ConstrainedSpatialDetailEvidenceService.SpatialDetailMatrixJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, ConstrainedSpatialDetailEvidenceService.ReachabilityProofMatrixJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, ConstrainedSpatialDetailEvidenceService.RepairFallbackMatrixJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, ConstrainedSpatialDetailEvidenceService.UnityCommandPlanJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, ConstrainedSpatialDetailEvidenceService.UnityProofSummaryJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, ConstrainedSpatialDetailEvidenceService.PreviewExportPayloadJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, ConstrainedSpatialDetailEvidenceService.InvalidMatrixJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, ConstrainedSpatialDetailEvidenceService.ReportMarkdownFileName)));
            Assert.Equal(9, Directory.EnumerateFiles(output, "spatial-detail-row-*.json", SearchOption.TopDirectoryOnly).Count());
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
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
