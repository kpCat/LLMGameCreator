using LLMGameCreator.Application.Design.OfflineGeoworldObjectiveAcceptanceRun;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldObjectiveAcceptanceRun;

public sealed class OfflineGeoworldObjectiveAcceptanceRunTests
{
    [Fact]
    public async Task ServiceBuildsObjectivePayloadAndEvidence()
    {
        var root = ProjectRoot();
        var write = await new OfflineGeoworldObjectiveAcceptanceRunEvidenceService()
            .BuildAndWriteAsync(root);
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(OfflineGeoworldObjectiveAcceptanceRunVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(
            result.QualityGateScan.Passed,
            string.Join(Environment.NewLine, result.QualityGateScan.Diagnostics.Select(item =>
                item.Code + " [" + item.Target + "] " + item.Message)));
        Assert.True(result.QualityGateScan.Goal106Consumed);
        Assert.True(result.QualityGateScan.ObjectivePayloadCreated);
        Assert.True(result.QualityGateScan.ReplayAcceptanceProofPassed);
        Assert.True(result.QualityGateScan.NegativeProofPassed);
        Assert.True(result.QualityGateScan.UnityScriptsReady);
        Assert.True(result.QualityGateScan.EditorWindowReady);
        Assert.True(result.QualityGateScan.WorkspaceBindingPassed);
        Assert.True(result.QualityGateScan.SourceLineagePassed);
        Assert.True(result.QualityGateScan.AlphaQualityConsolidationPassed);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapUnchanged);
        Assert.Equal(6, result.Manifest.PayloadFileCount);
        Assert.True(result.Manifest.ObjectiveCount >= 6);
        Assert.Equal(result.Manifest.ObjectiveCount, result.CompletionState.CompletedObjectiveCount);
        Assert.Equal("completed", result.CompletionState.FinalStatus);

        foreach (var fileName in OfflineGeoworldObjectiveAcceptanceRunVocabulary.RequiredPayloadFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.StreamingAssetsDirectoryPath, fileName)), fileName);
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }

        foreach (var fileName in OfflineGeoworldObjectiveAcceptanceRunVocabulary.RequiredEvidenceFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }
    }

    [Fact]
    public async Task ReplayAcceptanceProofCoversGoal106CheckpointAndObjectiveCompletion()
    {
        var root = ProjectRoot();
        var result = (await new OfflineGeoworldObjectiveAcceptanceRunEvidenceService()
            .BuildAndWriteAsync(root)).Result;

        Assert.True(result.SimulatedAcceptanceProof.PayloadReadAttempted);
        Assert.True(result.SimulatedAcceptanceProof.ManifestRead);
        Assert.True(result.SimulatedAcceptanceProof.ObjectivesRead);
        Assert.True(result.SimulatedAcceptanceProof.AcceptanceRunRead);
        Assert.True(result.SimulatedAcceptanceProof.CompletionStateRead);
        Assert.True(result.SimulatedAcceptanceProof.SourceGoal106PayloadRead);
        Assert.True(result.SimulatedAcceptanceProof.SourceGoal106ReplayProofRead);
        Assert.True(result.SimulatedAcceptanceProof.SourceGoal106ReplayHashChainPassed);
        Assert.True(result.SimulatedAcceptanceProof.CheckpointResumeApplied);
        Assert.True(result.SimulatedAcceptanceProof.ObjectivePrerequisitesPassed);
        Assert.True(result.SimulatedAcceptanceProof.CompletionTransitionsPassed);
        Assert.True(result.SimulatedAcceptanceProof.StateDeltaLinkagePassed);
        Assert.True(result.SimulatedAcceptanceProof.FailedPrerequisiteRejected);
        Assert.Equal(result.Manifest.ObjectiveCount, result.SimulatedAcceptanceProof.ObjectiveCount);
        Assert.Equal(result.Manifest.ObjectiveCount, result.SimulatedAcceptanceProof.CompletedObjectiveCount);
        Assert.Equal(result.Manifest.ObjectiveAcceptanceHash, result.SimulatedAcceptanceProof.FinalObjectiveAcceptanceHash);
    }

    [Fact]
    public async Task NegativeProofCoversRequiredObjectiveRisks()
    {
        var root = ProjectRoot();
        var result = (await new OfflineGeoworldObjectiveAcceptanceRunEvidenceService()
            .BuildAndWriteAsync(root)).Result;

        Assert.Equal(
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.RequiredNegativeScenarioIds.Count,
            result.NegativeProof.ScenarioCount);
        Assert.Equal(result.NegativeProof.ScenarioCount, result.NegativeProof.RejectedCount);
        foreach (var scenarioId in OfflineGeoworldObjectiveAcceptanceRunVocabulary.RequiredNegativeScenarioIds)
        {
            Assert.Contains(
                result.NegativeProof.Scenarios,
                scenario => scenario.ScenarioId == scenarioId
                            && scenario.ActualStatus == "rejected"
                            && scenario.Diagnostics.Count > 0);
        }
    }

    [Fact]
    public async Task WorkspaceGroupSurfacesObjectiveAcceptanceReadiness()
    {
        var root = ProjectRoot();
        await new OfflineGeoworldObjectiveAcceptanceRunEvidenceService().BuildAndWriteAsync(root);
        var result = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(
            result.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_objective_acceptance");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_objective_workspace_summary");

        Assert.True(
            result.QualityGateScan.Passed,
            string.Join(Environment.NewLine, result.QualityGateScan.Diagnostics.Select(item =>
                item.Code + " [" + item.Target + "] " + item.Message)));
        Assert.True(result.QualityGateScan.OfflineGeoworldObjectiveAcceptanceGroupPresent);
        Assert.True(result.QualityGateScan.OfflineGeoworldObjectiveCount >= 6);
        Assert.Equal(
            result.QualityGateScan.OfflineGeoworldObjectiveCount,
            result.QualityGateScan.OfflineGeoworldObjectiveCompletedCount);
        Assert.Equal(6, result.QualityGateScan.OfflineGeoworldObjectivePayloadFileCount);
        Assert.True(result.QualityGateScan.OfflineGeoworldObjectiveUnityScriptsReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldObjectiveEditorWindowReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldObjectiveReplayAcceptanceProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldObjectiveNegativeProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldObjectiveAlphaQualityConsolidationPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldObjectiveAlphaRuntimeBootstrapUnchanged);
        Assert.True(result.QualityGateScan.OfflineGeoworldObjectiveQualityGatePassed);
        Assert.True(result.QualityGateScan.Goal107FilesDiscoveredByRelativePaths);
        Assert.True(result.QualityGateScan.WinFormsOfflineGeoworldObjectiveAcceptanceBindingReal);
        Assert.True(summary.OfflineGeoworldObjectiveCount >= 6);
        Assert.True(summary.OfflineGeoworldObjectiveReplayAcceptanceProofPassed);
        AssertProofPassed(result.ProofStatus.Proofs, "goal107.unity_script_inventory");
        AssertProofPassed(result.ProofStatus.Proofs, "goal107.editor_window_inventory");
        AssertProofPassed(result.ProofStatus.Proofs, "goal107.replay_acceptance");
        AssertProofPassed(result.ProofStatus.Proofs, "goal107.negative");
        AssertProofPassed(result.ProofStatus.Proofs, "goal107.alpha_quality_consolidation");
        AssertProofPassed(result.ProofStatus.Proofs, "goal107.alpha_runtime_bootstrap_unchanged");
        AssertProofPassed(result.ProofStatus.Proofs, "goal107.checkpoint_resume");
        AssertProofPassed(result.ProofStatus.Proofs, "goal107.completion_transitions");
        AssertProofPassed(result.ProofStatus.Proofs, "goal107.quality_gate");
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
