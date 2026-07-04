using LLMGameCreator.Application.Design.OfflineGeoworldSessionPersistenceReplay;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldSessionPersistenceReplay;

public sealed class OfflineGeoworldSessionPersistenceReplayTests
{
    [Fact]
    public async Task ServiceBuildsSessionPayloadAndEvidence()
    {
        var write = await new OfflineGeoworldSessionPersistenceReplayEvidenceService()
            .BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(OfflineGeoworldSessionPersistenceReplayVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.Goal105Consumed);
        Assert.True(result.QualityGateScan.SessionPayloadCreated);
        Assert.True(result.UnityScriptInventory.Passed);
        Assert.True(result.EditorWindowInventory.Passed);
        Assert.True(result.SimulatedReplayProof.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.WorkspaceBindingInventory.Passed);
        Assert.True(result.SourceLineage.Passed);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapUnchanged);
        Assert.Equal(6, result.Manifest.PayloadFileCount);
        Assert.True(result.Manifest.ReplayStepCount >= 6);
        Assert.True(result.Manifest.StateDeltaCount >= 6);
        Assert.True(result.Manifest.CheckpointStepIndex >= 3);
        Assert.Equal(result.Manifest.FinalStateHash, result.DeltaLog.FinalStateHash);

        foreach (var fileName in OfflineGeoworldSessionPersistenceReplayVocabulary.RequiredPayloadFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.StreamingAssetsDirectoryPath, fileName)), fileName);
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }

        foreach (var fileName in OfflineGeoworldSessionPersistenceReplayVocabulary.RequiredEvidenceFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }
    }

    [Fact]
    public async Task SimulatedReplayProofCoversCheckpointResumeAndRejection()
    {
        var result = (await new OfflineGeoworldSessionPersistenceReplayEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.True(result.SimulatedReplayProof.PayloadReadAttempted);
        Assert.True(result.SimulatedReplayProof.ManifestRead);
        Assert.True(result.SimulatedReplayProof.InitialStateRead);
        Assert.True(result.SimulatedReplayProof.DeltaLogRead);
        Assert.True(result.SimulatedReplayProof.ReplayScriptRead);
        Assert.True(result.SimulatedReplayProof.AcceptanceChecklistRead);
        Assert.True(result.SimulatedReplayProof.PayloadHashesMatchManifest);
        Assert.True(result.SimulatedReplayProof.FirstHalfReplayApplied);
        Assert.True(result.SimulatedReplayProof.CheckpointSaved);
        Assert.True(result.SimulatedReplayProof.CheckpointLoaded);
        Assert.True(result.SimulatedReplayProof.ReplayResumedToFinalHash);
        Assert.True(result.SimulatedReplayProof.DuplicateReplayRejected);
        Assert.True(result.SimulatedReplayProof.CorruptedSnapshotRejected);
        Assert.Equal(result.Manifest.ReplayStepCount, result.SimulatedReplayProof.ReplayStepCount);
        Assert.Equal(result.Manifest.StateDeltaCount, result.SimulatedReplayProof.StateDeltaCount);
        Assert.Equal(result.Manifest.CheckpointStepIndex, result.SimulatedReplayProof.CheckpointStepIndex);
        Assert.Equal(result.Manifest.FinalStateHash, result.SimulatedReplayProof.FinalStateHash);
        Assert.Equal(result.DeltaLog.StateHashChain, result.SimulatedReplayProof.ReplayStateHashChain);
    }

    [Fact]
    public async Task NegativeProofCoversRequiredSessionRisks()
    {
        var result = (await new OfflineGeoworldSessionPersistenceReplayEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.Equal(
            OfflineGeoworldSessionPersistenceReplayVocabulary.RequiredNegativeScenarioIds.Count,
            result.NegativeProof.ScenarioCount);
        Assert.Equal(result.NegativeProof.ScenarioCount, result.NegativeProof.RejectedCount);
        foreach (var scenarioId in OfflineGeoworldSessionPersistenceReplayVocabulary.RequiredNegativeScenarioIds)
        {
            Assert.Contains(
                result.NegativeProof.Scenarios,
                scenario => scenario.ScenarioId == scenarioId
                            && scenario.ActualStatus == "rejected"
                            && scenario.Diagnostics.Count > 0);
        }
    }

    [Fact]
    public async Task WorkspaceGroupSurfacesSessionReplayReadiness()
    {
        await new OfflineGeoworldSessionPersistenceReplayEvidenceService()
            .BuildAndWriteAsync(ProjectRoot());
        var result = new VisualWorldStreamPreviewWorkspaceService().Build(ProjectRoot());
        var group = Assert.Single(
            result.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_session_replay");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_session_workspace_summary");

        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.OfflineGeoworldSessionReplayGroupPresent);
        Assert.True(result.QualityGateScan.OfflineGeoworldSessionReplayStepCount >= 6);
        Assert.True(result.QualityGateScan.OfflineGeoworldSessionStateDeltaCount >= 6);
        Assert.True(result.QualityGateScan.OfflineGeoworldSessionCheckpointStepIndex >= 3);
        Assert.True(result.QualityGateScan.OfflineGeoworldSessionUnityScriptsReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldSessionEditorWindowReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldSessionSimulatedReplayProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldSessionNegativeProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldSessionAlphaRuntimeBootstrapUnchanged);
        Assert.True(result.QualityGateScan.OfflineGeoworldSessionQualityGatePassed);
        Assert.True(result.QualityGateScan.Goal106FilesDiscoveredByRelativePaths);
        Assert.True(result.QualityGateScan.WinFormsOfflineGeoworldSessionReplayBindingReal);
        Assert.True(summary.OfflineGeoworldSessionReplayStepCount >= 6);
        Assert.True(summary.OfflineGeoworldSessionSimulatedReplayProofPassed);
        AssertProofPassed(result.ProofStatus.Proofs, "goal106.unity_script_inventory");
        AssertProofPassed(result.ProofStatus.Proofs, "goal106.editor_window_inventory");
        AssertProofPassed(result.ProofStatus.Proofs, "goal106.simulated_save_load_replay");
        AssertProofPassed(result.ProofStatus.Proofs, "goal106.negative");
        AssertProofPassed(result.ProofStatus.Proofs, "goal106.alpha_runtime_bootstrap_unchanged");
        AssertProofPassed(result.ProofStatus.Proofs, "goal106.checkpoint_resume");
        AssertProofPassed(result.ProofStatus.Proofs, "goal106.final_hash");
        AssertProofPassed(result.ProofStatus.Proofs, "goal106.quality_gate");
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
