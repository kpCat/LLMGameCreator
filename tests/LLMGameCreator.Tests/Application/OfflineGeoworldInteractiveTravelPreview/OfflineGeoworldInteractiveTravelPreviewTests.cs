using LLMGameCreator.Application.Design.OfflineGeoworldInteractiveTravelPreview;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldInteractiveTravelPreview;

public sealed class OfflineGeoworldInteractiveTravelPreviewTests
{
    [Fact]
    public async Task ServiceBuildsInteractiveTravelPayloadAndEvidence()
    {
        var write = await new OfflineGeoworldInteractiveTravelPreviewEvidenceService()
            .BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(OfflineGeoworldInteractiveTravelPreviewVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.UnityScriptInventory.Passed);
        Assert.True(result.EditorWindowInventory.Passed);
        Assert.True(result.SimulatedExecutionProof.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.WorkspaceBindingInventory.Passed);
        Assert.True(result.SourceLineage.Passed);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapUnchanged);
        Assert.Equal(5, result.Manifest.PayloadFileCount);
        Assert.Equal(6, result.Manifest.MovementSampleCount);
        Assert.Equal(2, result.Manifest.BoundaryCrossingCount);
        Assert.Equal(2, result.Manifest.PrefetchPlanCount);
        Assert.Equal(18, result.Manifest.ObjectCount);
        Assert.Equal(18, result.ObjectStateIndex.ObjectCount);

        foreach (var fileName in OfflineGeoworldInteractiveTravelPreviewVocabulary.RequiredPayloadFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.StreamingAssetsDirectoryPath, fileName)), fileName);
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }

        foreach (var fileName in OfflineGeoworldInteractiveTravelPreviewVocabulary.RequiredEvidenceFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }
    }

    [Fact]
    public async Task SimulatedExecutionProofCoversMovementBoundaryAndPrefetch()
    {
        var result = (await new OfflineGeoworldInteractiveTravelPreviewEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.True(result.SimulatedExecutionProof.PayloadReadAttempted);
        Assert.True(result.SimulatedExecutionProof.ManifestRead);
        Assert.True(result.SimulatedExecutionProof.StepsFileRead);
        Assert.True(result.SimulatedExecutionProof.ChunkVisibilityFileRead);
        Assert.True(result.SimulatedExecutionProof.ObjectStateIndexRead);
        Assert.True(result.SimulatedExecutionProof.PayloadHashesMatchManifest);
        Assert.True(result.SimulatedExecutionProof.MovementSampleCountPassed);
        Assert.True(result.SimulatedExecutionProof.BoundaryCrossingCountPassed);
        Assert.True(result.SimulatedExecutionProof.PrefetchPlanCoveragePassed);
        Assert.True(result.SimulatedExecutionProof.ObjectVisibilityDiffsPassed);
        Assert.True(result.SimulatedExecutionProof.DeterministicStateHashChainPassed);
        Assert.Equal(6, result.SimulatedExecutionProof.MovementSampleCount);
        Assert.Equal(2, result.SimulatedExecutionProof.BoundaryCrossingCount);
        Assert.Equal(2, result.SimulatedExecutionProof.PrefetchPlanCount);
        Assert.Equal(18, result.SimulatedExecutionProof.ObjectCount);
        Assert.Contains(result.Steps.MovementSamples, step => step.Action == "boundary_crossing");
        Assert.Contains(result.Steps.MovementSamples, step => step.NewlyVisibleObjectIds.Count > 0);
        Assert.Contains(result.Steps.MovementSamples, step => step.NewlyHiddenObjectIds.Count > 0);
        Assert.All(result.ChunkVisibility.BoundaryZones, zone =>
        {
            Assert.NotEmpty(zone.PrefetchChunkKeys);
            Assert.NotEmpty(zone.ActiveChunkKeysBefore);
            Assert.NotEmpty(zone.ActiveChunkKeysAfter);
        });
        Assert.All(result.ObjectStateIndex.Plans, plan => Assert.NotEmpty(plan.PrefetchChunkKeys));
    }

    [Fact]
    public async Task NegativeProofCoversRequiredInteractiveRisks()
    {
        var result = (await new OfflineGeoworldInteractiveTravelPreviewEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.Equal(
            OfflineGeoworldInteractiveTravelPreviewVocabulary.RequiredNegativeScenarioIds.Count,
            result.NegativeProof.ScenarioCount);
        foreach (var scenarioId in OfflineGeoworldInteractiveTravelPreviewVocabulary.RequiredNegativeScenarioIds)
        {
            Assert.Contains(
                result.NegativeProof.Scenarios,
                scenario => scenario.ScenarioId == scenarioId
                            && scenario.ActualStatus == "rejected"
                            && scenario.Diagnostics.Count > 0);
        }
    }

    [Fact]
    public async Task WorkspaceGroupSurfacesInteractiveTravelReadiness()
    {
        await new OfflineGeoworldInteractiveTravelPreviewEvidenceService()
            .BuildAndWriteAsync(ProjectRoot());
        var result = new VisualWorldStreamPreviewWorkspaceService().Build(ProjectRoot());
        var group = Assert.Single(
            result.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_interactive_travel");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_interactive_travel_workspace_summary");

        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractiveTravelGroupPresent);
        Assert.Equal(6, result.QualityGateScan.OfflineGeoworldInteractiveTravelMovementSampleCount);
        Assert.Equal(2, result.QualityGateScan.OfflineGeoworldInteractiveTravelBoundaryCrossingCount);
        Assert.Equal(18, result.QualityGateScan.OfflineGeoworldInteractiveTravelObjectCount);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractiveTravelUnityScriptsReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractiveTravelEditorWindowReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractiveTravelSimulatedExecutionProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractiveTravelNegativeProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractiveTravelAlphaRuntimeBootstrapUnchanged);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractiveTravelQualityGatePassed);
        Assert.True(result.QualityGateScan.Goal104FilesDiscoveredByRelativePaths);
        Assert.Equal(6, summary.OfflineGeoworldInteractiveTravelMovementSampleCount);
        Assert.Contains("0:", summary.OfflineGeoworldInteractiveTravelActiveChunkCounts, StringComparison.Ordinal);
        AssertProofPassed(result.ProofStatus.Proofs, "goal104.unity_script_inventory");
        AssertProofPassed(result.ProofStatus.Proofs, "goal104.editor_window_inventory");
        AssertProofPassed(result.ProofStatus.Proofs, "goal104.simulated_execution");
        AssertProofPassed(result.ProofStatus.Proofs, "goal104.boundary_crossings");
        AssertProofPassed(result.ProofStatus.Proofs, "goal104.prefetch_plan");
        AssertProofPassed(result.ProofStatus.Proofs, "goal104.quality_gate");
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
