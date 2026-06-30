using LLMGameCreator.Application.Design.FullCampaignPlayableReviewPackageRc;
using Xunit;

namespace LLMGameCreator.Tests.Application.FullCampaignPlayableReviewPackageRc;

public sealed class FullCampaignPlayableReviewPackageRcTests
{
    [Fact]
    public void SourceLoaderConsumesGoal060PackagesAndMediaProofChain()
    {
        var source = new FullCampaignPlayableReviewPackageRcSourceLoader().Load(ProjectRoot());

        Assert.True(source.Goal060AcceptedByUserHandoff);
        Assert.True(source.Goal060ReportWasGreenProducedForReview);
        Assert.True(source.Goal060UnityProofPassed);
        Assert.True(source.Goal059MatrixConsumed);
        Assert.True(source.MediaProofChainConsumed);
        Assert.Equal(9, source.PackageRows.Count);
        Assert.Equal(15, source.MediaBindings.Count);
        Assert.All(source.PackageRows, row =>
        {
            Assert.True(row.PackageHashVerified);
            Assert.True(row.ValidationPassed);
            Assert.True(row.RuntimePassed);
            Assert.True(row.SaveLoadRoundtripPassed);
            Assert.StartsWith("game/goal060/", row.PackageId, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void BuildCreatesReviewPackageManifestAndInventoryForNineRows()
    {
        var result = new FullCampaignPlayableReviewPackageRcEvidenceService().Build(ProjectRoot());

        Assert.True(result.SourceManifest.Goal060AcceptedByUserHandoff);
        Assert.True(result.ReviewPackageManifest.Passed);
        Assert.True(result.FileInventory.Passed);
        Assert.Equal(9, result.ReviewPackageManifest.PackageRowCount);
        Assert.Equal(9, result.ReviewPackageManifest.PhysicalPackageCount);
        Assert.Equal(9, result.ReviewPackageManifest.ScenarioSummaryCount);
        Assert.Contains(result.FileInventory.Files, file => file.RelativePath == "review-package/README.md");
        Assert.Contains(result.FileInventory.Files, file => file.RelativePath == "review-package/RUN_MANUAL.ps1");
        Assert.Contains(result.FileInventory.Files, file => file.RelativePath == "review-package/RUN_AUTOMATED_SMOKE.ps1");
    }

    [Fact]
    public void PackageRowSelectionMatrixTiesRowsToPackageHashesAndMediaBindings()
    {
        var result = new FullCampaignPlayableReviewPackageRcEvidenceService().Build(ProjectRoot());

        Assert.True(result.PackageRowSelectionMatrix.Passed);
        Assert.Equal(9, result.PackageRowSelectionMatrix.Rows.Count);
        Assert.All(result.PackageRowSelectionMatrix.Rows, row =>
        {
            Assert.True(row.PackageHashVerified);
            Assert.True(row.PackageMediaBindingsVerified);
            Assert.True(row.RuntimeLoopPassed);
            Assert.True(row.SaveLoadReplayVerified);
            Assert.False(string.IsNullOrWhiteSpace(row.PackageHash));
            Assert.Contains(row.FamilyId, FullCampaignPlayableReviewPackageRcVocabulary.FamilyIds);
            Assert.Contains(row.SeedId, FullCampaignPlayableReviewPackageRcVocabulary.SeedIds);
        });
    }

    [Fact]
    public void ScriptManifestValidatesReviewPackageLocalScripts()
    {
        var result = new FullCampaignPlayableReviewPackageRcEvidenceService().Build(ProjectRoot());

        Assert.True(result.ScriptManifest.Passed);
        Assert.Equal(2, result.ScriptManifest.Scripts.Count);
        Assert.All(result.ScriptManifest.Scripts, script =>
        {
            Assert.StartsWith("review-package/", script.RelativePath, StringComparison.Ordinal);
            Assert.DoesNotContain("..", script.RelativePath, StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(script.Sha256));
        });
    }

    [Fact]
    public void SaveLoadReplayAuditIsBoundToPackageRowsAndHashes()
    {
        var result = new FullCampaignPlayableReviewPackageRcEvidenceService().Build(ProjectRoot());

        Assert.True(result.SaveLoadReplayAudit.Passed);
        Assert.Equal(9, result.SaveLoadReplayAudit.Rows.Count);
        Assert.All(result.SaveLoadReplayAudit.Rows, row =>
        {
            Assert.StartsWith("matrix-row-", row.RowId, StringComparison.Ordinal);
            Assert.StartsWith("game/goal060/", row.PackageId, StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(row.PackageHash));
            Assert.True(row.SaveLoadRoundtripPassed);
            Assert.True(row.ReplayDeterminismPassed);
            Assert.True(row.PreviewExportPayloadConsistent);
            Assert.NotEmpty(row.RuntimeCommandIds);
        });
    }

    [Fact]
    public void UnityCommandPlanRequiresReviewPackageRcMarkersForEveryPackageRow()
    {
        var result = new FullCampaignPlayableReviewPackageRcEvidenceService().Build(ProjectRoot());

        Assert.True(result.UnityCommandPlan.Passed);
        Assert.Equal(9, result.UnityCommandPlan.Rows.Count);
        Assert.Contains("review_package_rc_loaded=true", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("review_package_rc_id=goal061-review-package-rc", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("review_package_rc_proof=goal061", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("full_campaign_playable_review_package_rc_verification=required", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.All(result.UnityCommandPlan.Rows, row =>
        {
            Assert.True(row.PackageHashVerified);
            Assert.True(row.PackageMediaBindingsVerified);
            Assert.True(row.SaveLoadReplayVerified);
            Assert.Contains("package_row_selected=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("package_id=" + row.PackageId, row.ExpectedPlayerMarkers);
            Assert.Contains("family_id=" + row.FamilyId, row.ExpectedPlayerMarkers);
            Assert.Contains("seed_id=" + row.SeedId, row.ExpectedPlayerMarkers);
            Assert.Contains("package_hash_verified=true", row.ExpectedPlayerMarkers);
            Assert.Contains("package_media_bindings_verified=true", row.ExpectedPlayerMarkers);
            Assert.Contains("save_load_replay_verified=true", row.ExpectedPlayerMarkers);
        });
    }

    [Fact]
    public void InvalidMatrixCoversRequiredFakeLeakCases()
    {
        var result = new FullCampaignPlayableReviewPackageRcEvidenceService().Build(ProjectRoot());
        var ids = result.InvalidMatrix.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);

        Assert.True(result.InvalidMatrix.Passed);
        foreach (var required in FullCampaignPlayableReviewPackageRcVocabulary.RequiredInvalidScenarioIds)
        {
            Assert.Contains(required, ids);
        }

        Assert.All(result.InvalidMatrix.Scenarios, scenario =>
        {
            Assert.Equal(scenario.ExpectedStatus, scenario.ActualStatus);
            Assert.NotEmpty(scenario.Diagnostics);
            Assert.All(scenario.Diagnostics, diagnostic => Assert.StartsWith("goal061.", diagnostic.Code, StringComparison.Ordinal));
        });
    }

    [Fact]
    public async Task WriteAsyncEmitsRequiredArtifactsAndReviewPackageFiles()
    {
        var service = new FullCampaignPlayableReviewPackageRcEvidenceService();
        var result = service.Build(ProjectRoot());
        var tempRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "Goal061Write", Guid.NewGuid().ToString("N"));
        try
        {
            var write = await service.WriteAsync(tempRoot, result);
            var output = write.OutputDirectoryPath;
            Assert.True(File.Exists(Path.Combine(output, FullCampaignPlayableReviewPackageRcEvidenceService.SourceManifestJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignPlayableReviewPackageRcEvidenceService.ReviewPackageManifestJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignPlayableReviewPackageRcEvidenceService.FileInventoryJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignPlayableReviewPackageRcEvidenceService.PackageRowSelectionMatrixJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignPlayableReviewPackageRcEvidenceService.UnityCommandPlanJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignPlayableReviewPackageRcEvidenceService.UnityProofMatrixJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignPlayableReviewPackageRcEvidenceService.PackageMediaBindingAuditJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignPlayableReviewPackageRcEvidenceService.SaveLoadReplayAuditJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignPlayableReviewPackageRcEvidenceService.ManualReviewChecklistMarkdownFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignPlayableReviewPackageRcEvidenceService.ScriptManifestJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignPlayableReviewPackageRcEvidenceService.InvalidMatrixJsonFileName)));
            Assert.True(File.Exists(Path.Combine(output, FullCampaignPlayableReviewPackageRcEvidenceService.ReportMarkdownFileName)));
            Assert.True(File.Exists(Path.Combine(output, "review-package", "README.md")));
            Assert.True(File.Exists(Path.Combine(output, "review-package", "RUN_AUTOMATED_SMOKE.ps1")));
            Assert.Equal(9, Directory.EnumerateFiles(Path.Combine(output, "review-package", "p"), "*.json", SearchOption.TopDirectoryOnly).Count());
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
