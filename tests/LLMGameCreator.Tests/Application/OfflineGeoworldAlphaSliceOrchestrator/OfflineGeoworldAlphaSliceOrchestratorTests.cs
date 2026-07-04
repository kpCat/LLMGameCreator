using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldAlphaSliceOrchestrator;

public sealed class OfflineGeoworldAlphaSliceOrchestratorTests
{
    [Fact]
    public async Task ServiceBuildsAlphaSliceAggregatePayloadAndEvidence()
    {
        var root = ProjectRoot();
        var write = await new OfflineGeoworldAlphaSliceOrchestratorEvidenceService()
            .BuildAndWriteAsync(root);
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(OfflineGeoworldAlphaSliceVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(
            result.QualityGateScan.Passed,
            string.Join(Environment.NewLine, result.QualityGateScan.Diagnostics));
        Assert.Equal(5, result.Manifest.PayloadFileCount);
        Assert.Equal(7, result.Manifest.ComponentCount);
        Assert.Equal(7, result.Manifest.ReadyComponentCount);
        Assert.True(result.Manifest.ObjectiveCount >= 5);
        Assert.Equal(result.Manifest.ObjectiveCount, result.Manifest.CompletedObjectiveCount);
        Assert.Equal("completed", result.Manifest.FinalStatus);
        Assert.True(result.Manifest.AlphaRuntimeBootstrapUnchanged);
        Assert.Equal(7, result.Components.Components.Count);
        Assert.All(result.Components.Components, component =>
        {
            Assert.True(component.Ready, component.ComponentId);
            Assert.False(component.Accepted, component.ComponentId);
            Assert.NotEmpty(component.SourceArtifactHashes);
            Assert.False(Path.IsPathFullyQualified(component.SourceArtifactRoot), component.SourceArtifactRoot);
        });

        foreach (var fileName in OfflineGeoworldAlphaSliceVocabulary.RequiredPayloadFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.StreamingAssetsDirectoryPath, fileName)), fileName);
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }

        foreach (var fileName in OfflineGeoworldAlphaSliceVocabulary.RequiredEvidenceFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }
    }

    [Fact]
    public async Task FullSliceProofAndNegativeProofCoverRequiredRisks()
    {
        var root = ProjectRoot();
        var result = (await new OfflineGeoworldAlphaSliceOrchestratorEvidenceService()
            .BuildAndWriteAsync(root)).Result;

        Assert.True(result.SimulatedProof.PayloadReadAttempted);
        Assert.True(result.SimulatedProof.SourceGoal101To107PayloadsRead);
        Assert.True(result.SimulatedProof.SetupPreviewPassed);
        Assert.True(result.SimulatedProof.TravelPassed);
        Assert.True(result.SimulatedProof.InteractionPassed);
        Assert.True(result.SimulatedProof.SavePassed);
        Assert.True(result.SimulatedProof.LoadPassed);
        Assert.True(result.SimulatedProof.ReplayPassed);
        Assert.True(result.SimulatedProof.CompleteObjectivesPassed);
        Assert.True(result.SimulatedProof.FinalHashPropagationPassed);
        Assert.True(result.SimulatedProof.HistoricalArtifactsUnchanged);
        Assert.True(result.SimulatedProof.NoAbsolutePaths);
        Assert.True(result.SimulatedProof.NoRawGeodata);
        Assert.True(result.SimulatedProof.NoBinaryOrRasterMedia);
        Assert.True(result.SimulatedProof.NoNetworkProviderMarkers);

        Assert.Equal(
            OfflineGeoworldAlphaSliceVocabulary.RequiredNegativeScenarioIds.Count,
            result.NegativeProof.ScenarioCount);
        Assert.Equal(result.NegativeProof.ScenarioCount, result.NegativeProof.RejectedCount);
        foreach (var scenarioId in OfflineGeoworldAlphaSliceVocabulary.RequiredNegativeScenarioIds)
        {
            Assert.Contains(
                result.NegativeProof.Scenarios,
                scenario => scenario.ScenarioId == scenarioId
                            && scenario.ActualStatus == "rejected"
                            && scenario.Diagnostic.Length > 0);
        }
    }

    [Fact]
    public async Task WorkspaceGroupSurfacesAlphaSliceReadiness()
    {
        var root = ProjectRoot();
        await new OfflineGeoworldAlphaSliceOrchestratorEvidenceService().BuildAndWriteAsync(root);
        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_alpha_slice");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_alpha_slice_workspace_summary");

        Assert.True(
            workspace.QualityGateScan.Passed,
            string.Join(Environment.NewLine, workspace.QualityGateScan.Diagnostics.Select(item =>
                item.Code + " [" + item.Target + "] " + item.Message)));
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaSliceGroupPresent);
        Assert.Equal(7, workspace.QualityGateScan.OfflineGeoworldAlphaSliceComponentCount);
        Assert.Equal(7, workspace.QualityGateScan.OfflineGeoworldAlphaSliceReadyComponentCount);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaSliceObjectiveCount >= 5);
        Assert.Equal(
            workspace.QualityGateScan.OfflineGeoworldAlphaSliceObjectiveCount,
            workspace.QualityGateScan.OfflineGeoworldAlphaSliceCompletedObjectiveCount);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaSliceUnityToolReady);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaSliceAcceptanceRunbookReady);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaSliceFinalProofPassed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaSliceNegativeProofPassed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaSliceAlphaRuntimeBootstrapUnchanged);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaSliceQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal108FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.WinFormsOfflineGeoworldAlphaSliceBindingReal);
        Assert.Equal(7, summary.OfflineGeoworldAlphaSliceComponentCount);
        Assert.True(summary.OfflineGeoworldAlphaSliceFinalProofPassed);
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal108.alpha_slice.unity_script_inventory");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal108.alpha_slice.editor_window_inventory");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal108.alpha_slice.full_slice_simulated_proof");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal108.alpha_slice.negative_proof");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal108.alpha_slice.quality_gate");
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
