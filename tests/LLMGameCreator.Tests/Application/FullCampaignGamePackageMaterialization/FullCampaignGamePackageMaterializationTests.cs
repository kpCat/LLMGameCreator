using LLMGameCreator.Application.Design.FullCampaignGamePackageMaterialization;
using Xunit;

namespace LLMGameCreator.Tests.Application.FullCampaignGamePackageMaterialization;

public sealed class FullCampaignGamePackageMaterializationTests
{
    [Fact]
    public void SourceLoaderConsumesAcceptedGoal059MatrixInDeterministicFamilySeedOrder()
    {
        var source = new FullCampaignGamePackageMaterializationSourceLoader().Load(ProjectRoot());

        Assert.True(source.Goal059AcceptedByUserHandoff);
        Assert.True(source.Goal059ReportWasGreenProducedForReview);
        Assert.True(source.Goal059UnityProofPassed);
        Assert.Equal(9, source.Rows.Count);
        Assert.Equal(
            new[]
            {
                "matrix-row-map-panel-rpg-seed-alpha",
                "matrix-row-map-panel-rpg-seed-beta",
                "matrix-row-map-panel-rpg-seed-gamma",
                "matrix-row-survival-sandbox-seed-alpha",
                "matrix-row-survival-sandbox-seed-beta",
                "matrix-row-survival-sandbox-seed-gamma",
                "matrix-row-first-person-grid-dungeon-seed-alpha",
                "matrix-row-first-person-grid-dungeon-seed-beta",
                "matrix-row-first-person-grid-dungeon-seed-gamma"
            },
            source.Rows.Select(item => item.RowId));
        Assert.All(source.Rows, row =>
        {
            Assert.StartsWith("matrix-row-", row.RowId, StringComparison.Ordinal);
            Assert.Contains(row.FamilyId, FullCampaignGamePackageMaterializationVocabulary.FamilyIds);
            Assert.Contains(row.SeedId, FullCampaignGamePackageMaterializationVocabulary.SeedIds);
            Assert.False(string.IsNullOrWhiteSpace(row.RowHash));
            Assert.False(string.IsNullOrWhiteSpace(row.SourceCampaignHash));
        });
    }

    [Fact]
    public void BuildMaterializesNineValidatorCleanPackagesAndRuntimeConsumptionRows()
    {
        var service = FullCampaignGamePackageMaterializationTestFactory.CreateService();
        var result = service.Build(ProjectRoot());

        Assert.True(result.SourceManifest.Goal059AcceptedByUserHandoff);
        Assert.True(result.PackageMaterializationPlan.Passed);
        Assert.True(result.PackageInventory.Passed);
        Assert.True(result.PackageValidationMatrix.Passed);
        Assert.True(result.RuntimeConsumptionMatrix.Passed);
        Assert.True(result.PreviewExportPackagePayloads.Passed);
        Assert.True(result.InvalidMatrix.Passed);
        Assert.Equal(9, result.Packages.Count);
        Assert.Equal(9, result.PackageValidationMatrix.ValidPackageCount);
        Assert.Equal(3, result.RuntimeConsumptionMatrix.RuntimePassedFamilyCount);
        Assert.Equal("BLOCKED", result.Report.ImplementationStatus);
        Assert.All(result.Packages, package =>
        {
            Assert.True(package.ValidJson);
            Assert.True(package.ValidationPassed);
            Assert.StartsWith("game/goal060/", package.PackageId, StringComparison.Ordinal);
            Assert.StartsWith("packages/", package.PackageRelativePath, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void UnityCommandPlanRequiresPackageConsumptionMarkersForEveryMaterializedRow()
    {
        var service = FullCampaignGamePackageMaterializationTestFactory.CreateService();
        var result = service.Build(ProjectRoot());

        Assert.True(result.UnityCommandPlan.Passed);
        Assert.Equal(9, result.UnityCommandPlan.Rows.Count);
        Assert.Contains("package_matrix_loaded=true", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("package_materialization_goal=goal060", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("full_campaign_gamepackage_materialization_matrix_verification=required", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.All(result.UnityCommandPlan.Rows, row =>
        {
            Assert.True(row.PackageValidationPassed);
            Assert.True(row.RuntimeLoopCompleted);
            Assert.Contains("package_family=" + row.FamilyId, row.ExpectedPlayerMarkers);
            Assert.Contains("package_seed=" + row.SeedId, row.ExpectedPlayerMarkers);
            Assert.Contains("package_id=" + row.PackageId, row.ExpectedPlayerMarkers);
            Assert.Contains("package_validation_passed=true", row.ExpectedPlayerMarkers);
            Assert.Contains("package_runtime_loop_completed=true", row.ExpectedPlayerMarkers);
        });
    }

    [Fact]
    public async Task WriteAsyncEmitsRequiredEvidenceAndPhysicalPackageFiles()
    {
        var service = FullCampaignGamePackageMaterializationTestFactory.CreateService();
        var result = service.Build(ProjectRoot());
        var tempRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "Goal060Write", Guid.NewGuid().ToString("N"));
        try
        {
            var write = await service.WriteAsync(tempRoot, result);
            var output = write.OutputDirectoryPath;
            Assert.True(File.Exists(Path.Combine(output, FullCampaignGamePackageMaterializationEvidenceService.SourceManifestJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignGamePackageMaterializationEvidenceService.PackageMaterializationPlanJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignGamePackageMaterializationEvidenceService.MaterializedPackageInventoryJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignGamePackageMaterializationEvidenceService.PackageValidationMatrixJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignGamePackageMaterializationEvidenceService.RuntimeConsumptionMatrixJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignGamePackageMaterializationEvidenceService.PreviewExportPackagePayloadsJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignGamePackageMaterializationEvidenceService.UnityCommandPlanJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignGamePackageMaterializationEvidenceService.UnityProofJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignGamePackageMaterializationEvidenceService.InvalidMatrixJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignGamePackageMaterializationEvidenceService.ArtifactScopeReportJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignGamePackageMaterializationEvidenceService.ReportMarkdownFileName)));
            Assert.Equal(9, write.Result.Packages.Count);
            Assert.All(write.Result.Packages, package =>
            {
                Assert.True(File.Exists(Path.Combine(output, package.PackageRelativePath.Replace('/', Path.DirectorySeparatorChar))));
            });
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void InvalidMatrixCoversRequiredFakeLeakCasesWithStableDiagnostics()
    {
        var result = FullCampaignGamePackageMaterializationTestFactory.CreateService().Build(ProjectRoot());
        var ids = result.InvalidMatrix.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);

        Assert.True(result.InvalidMatrix.Passed);
        foreach (var required in FullCampaignGamePackageMaterializationVocabulary.RequiredInvalidScenarioIds)
        {
            Assert.Contains(required, ids);
        }

        Assert.All(result.InvalidMatrix.Scenarios, scenario =>
        {
            Assert.Equal(scenario.ExpectedStatus, scenario.ActualStatus);
            Assert.NotEmpty(scenario.Diagnostics);
            Assert.All(scenario.Diagnostics, diagnostic => Assert.StartsWith("goal060.", diagnostic.Code, StringComparison.Ordinal));
        });
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
