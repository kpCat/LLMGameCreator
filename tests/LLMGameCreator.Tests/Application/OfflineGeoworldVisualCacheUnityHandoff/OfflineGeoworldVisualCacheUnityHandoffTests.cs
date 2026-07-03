using LLMGameCreator.Application.Design.OfflineGeoworldVisualCacheUnityHandoff;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldVisualCacheUnityHandoff;

public sealed class OfflineGeoworldVisualCacheUnityHandoffTests
{
    [Fact]
    public async Task ServiceBuildsVisualCachePackagesAndUnityPayloadFromGoal099Artifacts()
    {
        var write = await new OfflineGeoworldVisualCacheUnityHandoffEvidenceService()
            .BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(OfflineGeoworldVisualCacheUnityHandoffVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.SourceLineage.Passed);
        Assert.True(result.StreamingAssetsLedger.Passed);
        Assert.True(result.ProbeSourceInventory.Passed);
        Assert.True(result.SimulatedReadProof.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.WorkspaceBindingInventory.Passed);
        Assert.Equal(3, result.HandoffManifest.PackageCount);
        Assert.Equal(10, result.HandoffManifest.FeatureCount);
        Assert.Equal(10, result.HandoffManifest.FeatureKindCount);
        Assert.Equal(18, result.HandoffManifest.VisualCacheRecordCount);
        Assert.Equal(5, result.HandoffManifest.SourceChunkCount);
        Assert.Equal(9, result.HandoffManifest.StreamWindowChunkCount);
        Assert.Equal(5, result.HandoffManifest.PayloadFileCount);
        Assert.Equal(3, result.PackageIndex.PackageCount);
        Assert.Contains(result.PackageIndex.Packages, item => item.PackageId == "geoworld_editor_review_package");
        Assert.Contains(result.PackageIndex.Packages, item => item.PackageId == "geoworld_unity_handoff_package");
        Assert.Contains(result.PackageIndex.Packages, item => item.PackageId == "geoworld_stream_window_runtime_preview_package");

        foreach (var kind in OfflineGeoworldVisualCacheUnityHandoffVocabulary.RequiredVisualFeatureKinds)
        {
            Assert.True(result.VisualCacheCatalog.FeatureCountByKind.ContainsKey(kind), kind);
        }

        Assert.All(result.FeatureChunkLedger.Records, record =>
        {
            Assert.False(string.IsNullOrWhiteSpace(record.SourceFeatureId), record.RecordId);
            Assert.False(string.IsNullOrWhiteSpace(record.FeatureKind), record.RecordId);
            Assert.False(string.IsNullOrWhiteSpace(record.SourceChunkKey), record.RecordId);
            Assert.False(string.IsNullOrWhiteSpace(record.VisualChunkKey), record.RecordId);
            Assert.False(string.IsNullOrWhiteSpace(record.VisualLayerId), record.RecordId);
            Assert.Equal("projected_metadata_only", record.ProjectionStatus);
            Assert.False(string.IsNullOrWhiteSpace(record.CacheRecordHash), record.RecordId);
            Assert.False(record.RawGeodataIncluded, record.RecordId);
            Assert.True(record.MetadataOnly, record.RecordId);
            Assert.Contains("goal_098", record.Goal098Lineage, StringComparison.Ordinal);
            Assert.Contains("goal_099", record.Goal099Lineage, StringComparison.Ordinal);
            Assert.Contains("safe_public_geoworld_fallback", record.SafeRatingMetadataStatus, StringComparison.Ordinal);
        });

        foreach (var fileName in OfflineGeoworldVisualCacheUnityHandoffVocabulary.RequiredPayloadFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.StreamingAssetsDirectoryPath, fileName)), fileName);
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }

        foreach (var fileName in OfflineGeoworldVisualCacheUnityHandoffVocabulary.RequiredEvidenceFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }
    }

    [Fact]
    public async Task SimulatedReadProofAndNegativeProofCoverRequiredUnityHandoffRisks()
    {
        var result = (await new OfflineGeoworldVisualCacheUnityHandoffEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.True(result.SimulatedReadProof.PayloadReadAttempted);
        Assert.True(result.SimulatedReadProof.ManifestRead);
        Assert.True(result.SimulatedReadProof.RequiredPayloadFilesPresent);
        Assert.True(result.SimulatedReadProof.PayloadHashesMatchManifest);
        Assert.True(result.SimulatedReadProof.CountsMatchVisualCacheCatalog);
        Assert.True(result.SimulatedReadProof.NoRawGeodata);
        Assert.True(result.SimulatedReadProof.NoRawFullWorldDump);
        Assert.True(result.SimulatedReadProof.NoAbsolutePaths);
        Assert.True(result.SimulatedReadProof.NoBinaryOrRasterMedia);
        Assert.True(result.SimulatedReadProof.NoProviderOrNetworkMarkers);
        Assert.Equal(3, result.SimulatedReadProof.PackageCount);
        Assert.Equal(10, result.SimulatedReadProof.FeatureCount);
        Assert.Equal(18, result.SimulatedReadProof.VisualCacheRecordCount);

        Assert.Equal(
            OfflineGeoworldVisualCacheUnityHandoffVocabulary.RequiredNegativeScenarioIds.Count,
            result.NegativeProof.ScenarioCount);
        foreach (var scenarioId in OfflineGeoworldVisualCacheUnityHandoffVocabulary.RequiredNegativeScenarioIds)
        {
            Assert.Contains(
                result.NegativeProof.Scenarios,
                scenario => scenario.ScenarioId == scenarioId
                            && scenario.ActualStatus == "rejected"
                            && scenario.Diagnostics.Count > 0);
        }
    }

    [Fact]
    public async Task WorkspaceSurfacesOfflineGeoworldHandoffGroupAndProofs()
    {
        await new OfflineGeoworldVisualCacheUnityHandoffEvidenceService()
            .BuildAndWriteAsync(ProjectRoot());
        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(ProjectRoot());
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_handoff");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_handoff_workspace_summary");

        Assert.True(workspace.QualityGateScan.Passed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldHandoffGroupPresent);
        Assert.Equal(3, workspace.QualityGateScan.OfflineGeoworldHandoffPackageCount);
        Assert.Equal(10, workspace.QualityGateScan.OfflineGeoworldHandoffFeatureCount);
        Assert.Equal(18, workspace.QualityGateScan.OfflineGeoworldHandoffVisualCacheRecordCount);
        Assert.Equal(5, workspace.QualityGateScan.OfflineGeoworldHandoffSourceChunkCount);
        Assert.Equal(9, workspace.QualityGateScan.OfflineGeoworldHandoffStreamWindowChunkCount);
        Assert.Equal(5, workspace.QualityGateScan.OfflineGeoworldHandoffUnityPayloadFileCount);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldHandoffSimulatedReadProofPassed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldHandoffNegativeProofPassed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldHandoffAlphaRuntimeBootstrapUnchanged);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldHandoffQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal100FilesDiscoveredByRelativePaths);
        Assert.Equal(3, summary.PackageCount);
        Assert.Equal(10, summary.GeoworldNormalizedFeatureCount);
        Assert.Equal(18, summary.GeoworldVisualCacheRecordCount);
        Assert.False(string.IsNullOrWhiteSpace(summary.OfflineGeoworldHandoffFeatureKindCountsSummary));
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal100.simulated_read");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal100.negative");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal100.probe_source_inventory");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal100.alpha_runtime_bootstrap_unchanged");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal100.all_feature_kinds_mapped");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal100.quality_gate");
    }

    private static void AssertProofPassed(
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs,
        string proofId)
    {
        var proof = Assert.Single(proofs, item => item.ProofId == proofId);
        Assert.True(proof.Passed, proof.DiagnosticSummary);
        Assert.False(Path.IsPathFullyQualified(proof.RelativePath), proof.RelativePath);
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
