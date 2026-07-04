using LLMGameCreator.Application.Design.OfflineGeoworldUnityPlayModeTravelPreview;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldUnityPlayModeTravelPreview;

public sealed class OfflineGeoworldUnityPlayModeTravelPreviewTests
{
    [Fact]
    public async Task ServiceBuildsPlayModeTravelPayloadAndEvidence()
    {
        var write = await new OfflineGeoworldPlayModeTravelPreviewEvidenceService()
            .BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(OfflineGeoworldPlayModeTravelPreviewVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.UnityScriptInventory.Passed);
        Assert.True(result.EditorWindowInventory.Passed);
        Assert.True(result.SimulatedExecutionProof.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.WorkspaceBindingInventory.Passed);
        Assert.True(result.SourceLineage.Passed);
        Assert.True(result.Goal102BClosure.Passed);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapUnchanged);
        Assert.Equal(5, result.Manifest.PayloadFileCount);
        Assert.Equal(18, result.Manifest.ObjectCount);
        Assert.True(result.Manifest.StepCount >= 4);
        Assert.Equal(18, result.ObjectStateIndex.ObjectCount);

        foreach (var fileName in OfflineGeoworldPlayModeTravelPreviewVocabulary.RequiredPayloadFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.StreamingAssetsDirectoryPath, fileName)), fileName);
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }

        foreach (var fileName in OfflineGeoworldPlayModeTravelPreviewVocabulary.RequiredEvidenceFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }
    }

    [Fact]
    public async Task SimulatedExecutionProofCoversTravelVisibilityAndSafety()
    {
        var result = (await new OfflineGeoworldPlayModeTravelPreviewEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.True(result.SimulatedExecutionProof.PayloadReadAttempted);
        Assert.True(result.SimulatedExecutionProof.ManifestRead);
        Assert.True(result.SimulatedExecutionProof.StepsFileRead);
        Assert.True(result.SimulatedExecutionProof.ChunkVisibilityFileRead);
        Assert.True(result.SimulatedExecutionProof.ObjectStateIndexRead);
        Assert.True(result.SimulatedExecutionProof.PayloadHashesMatchManifest);
        Assert.True(result.SimulatedExecutionProof.StepByStepVisibleCountsPassed);
        Assert.True(result.SimulatedExecutionProof.BoundaryPrefetchProgressionRepresented);
        Assert.True(result.SimulatedExecutionProof.DeterministicStateHashChainPassed);
        Assert.True(result.SimulatedExecutionProof.NoUnsupportedStep);
        Assert.True(result.SimulatedExecutionProof.NoAbsolutePaths);
        Assert.True(result.SimulatedExecutionProof.NoRawGeodata);
        Assert.True(result.SimulatedExecutionProof.NoBinaryOrRasterMedia);
        Assert.True(result.SimulatedExecutionProof.NoProviderOrNetworkMarkers);
        Assert.Equal(result.Manifest.StepCount, result.SimulatedExecutionProof.StepCount);
        Assert.Equal(18, result.SimulatedExecutionProof.ObjectCount);
        Assert.Contains(result.Steps.Steps, step => step.NewlyVisibleObjectIds.Count > 0);
        Assert.Contains(result.Steps.Steps, step => step.NewlyHiddenObjectIds.Count > 0);
        Assert.All(result.Steps.Steps, step =>
        {
            Assert.Equal(step.ExpectedVisibleObjectCount, step.VisibleObjectIds.Count);
            Assert.NotEmpty(step.BoundaryPrefetchChunkKeys);
        });
    }

    [Fact]
    public async Task NegativeProofAndGoal102BClosureCoverRequiredRisks()
    {
        var result = (await new OfflineGeoworldPlayModeTravelPreviewEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.True(result.Goal102BClosure.Goal102BRemainsBlocked);
        Assert.True(result.Goal102BClosure.ProductSourceBlockerClosed);
        Assert.True(result.Goal102BClosure.ActualHeadBeforeEvidenceRead);
        Assert.False(result.Goal102BClosure.ActualHeadBeforeMalformedDetected);
        Assert.True(result.Goal102BClosure.FutureGatesRequireActualTargetBytes);
        Assert.Equal(
            OfflineGeoworldPlayModeTravelPreviewVocabulary.RequiredNegativeScenarioIds.Count,
            result.NegativeProof.ScenarioCount);
        foreach (var scenarioId in OfflineGeoworldPlayModeTravelPreviewVocabulary.RequiredNegativeScenarioIds)
        {
            Assert.Contains(
                result.NegativeProof.Scenarios,
                scenario => scenario.ScenarioId == scenarioId
                            && scenario.ActualStatus == "rejected"
                            && scenario.Diagnostics.Count > 0);
        }
    }

    [Fact]
    public async Task WorkspaceGroupSurfacesPlayModeTravelReadiness()
    {
        await new OfflineGeoworldPlayModeTravelPreviewEvidenceService()
            .BuildAndWriteAsync(ProjectRoot());
        var result = new VisualWorldStreamPreviewWorkspaceService().Build(ProjectRoot());
        var group = Assert.Single(
            result.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_playmode_travel");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_playmode_travel_workspace_summary");

        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelGroupPresent);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelStepCount >= 4);
        Assert.Equal(18, result.QualityGateScan.OfflineGeoworldPlayModeTravelObjectCount);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelUnityScriptsReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelEditorWindowReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelSimulatedExecutionProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelNegativeProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelGoal102BClosureRecorded);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelAlphaRuntimeBootstrapUnchanged);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelQualityGatePassed);
        Assert.True(result.QualityGateScan.Goal103FilesDiscoveredByRelativePaths);
        Assert.Equal(18, summary.OfflineGeoworldPlayModeTravelObjectCount);
        Assert.Contains("0:", summary.OfflineGeoworldPlayModeTravelActiveChunkCounts, StringComparison.Ordinal);
        AssertProofPassed(result.ProofStatus.Proofs, "goal103.unity_script_inventory");
        AssertProofPassed(result.ProofStatus.Proofs, "goal103.editor_window_inventory");
        AssertProofPassed(result.ProofStatus.Proofs, "goal103.simulated_execution");
        AssertProofPassed(result.ProofStatus.Proofs, "goal103.goal102b_closure");
        AssertProofPassed(result.ProofStatus.Proofs, "goal103.quality_gate");
    }

    [Fact]
    public async Task EvidenceIsDeterministicAndSourceHealthClean()
    {
        var service = new OfflineGeoworldPlayModeTravelPreviewEvidenceService();
        var first = await service.BuildAndWriteAsync(ProjectRoot());
        var second = await service.BuildAndWriteAsync(ProjectRoot());

        Assert.Equal(first.Result.PayloadJsonByFileName, second.Result.PayloadJsonByFileName);
        Assert.Equal(first.Result.EvidenceJsonByFileName, second.Result.EvidenceJsonByFileName);
        Assert.Equal(first.Result.Report.DeterministicReportHash, second.Result.Report.DeterministicReportHash);
        Assert.True(second.Result.QualityGateScan.Passed);
        Assert.Equal(0, second.Result.QualityGateScan.FilesOver1000LogicalLinesCount);
        Assert.Equal(0, second.Result.QualityGateScan.FilesOver700LogicalLinesCount);
        Assert.True(second.Result.QualityGateScan.MaxLogicalLineCount <= 700);
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
