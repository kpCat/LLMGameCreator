using LLMGameCreator.Application.Design.OfflineGeoworldInteractionPlayableProbe;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldInteractionPlayableProbe;

public sealed class OfflineGeoworldInteractionPlayableProbeTests
{
    [Fact]
    public async Task ServiceBuildsInteractionPayloadAndEvidence()
    {
        var write = await new OfflineGeoworldInteractionPlayableProbeEvidenceService()
            .BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(OfflineGeoworldInteractionPlayableProbeVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.UnityScriptInventory.Passed);
        Assert.True(result.EditorWindowInventory.Passed);
        Assert.True(result.SimulatedSessionProof.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.WorkspaceBindingInventory.Passed);
        Assert.True(result.SourceLineage.Passed);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapUnchanged);
        Assert.True(result.QualityGateScan.UnityScriptInventorySafetyPassed);
        Assert.True(result.QualityGateScan.NoRawGeodataDump);
        Assert.True(result.QualityGateScan.NoBinaryOrRasterMedia);
        Assert.Equal(6, result.Manifest.PayloadFileCount);
        Assert.True(result.Manifest.TargetCount >= 8);
        Assert.True(result.Manifest.ActionKindCount >= 5);
        Assert.True(result.Manifest.ScriptedEventCount >= 6);
        Assert.True(result.Manifest.StateDeltaCount >= 6);
        Assert.Equal(result.StateDeltaPlan.FinalStateHash, result.Report.FinalStateHash);
        Assert.Equal(
            OfflineGeoworldInteractionPlayableProbeVocabulary.RequiredActionKinds.OrderBy(item => item),
            result.Actions.ActionKinds.OrderBy(item => item));

        foreach (var fileName in OfflineGeoworldInteractionPlayableProbeVocabulary.RequiredPayloadFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.StreamingAssetsDirectoryPath, fileName)), fileName);
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }

        foreach (var fileName in OfflineGeoworldInteractionPlayableProbeVocabulary.RequiredEvidenceFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }
    }

    [Fact]
    public async Task SimulatedSessionProofCoversBindingAvailabilityAndStateHashChain()
    {
        var result = (await new OfflineGeoworldInteractionPlayableProbeEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.True(result.SimulatedSessionProof.PayloadReadAttempted);
        Assert.True(result.SimulatedSessionProof.ManifestRead);
        Assert.True(result.SimulatedSessionProof.TargetsRead);
        Assert.True(result.SimulatedSessionProof.ActionsRead);
        Assert.True(result.SimulatedSessionProof.SessionScriptRead);
        Assert.True(result.SimulatedSessionProof.StateDeltaPlanRead);
        Assert.True(result.SimulatedSessionProof.PayloadHashesMatchManifest);
        Assert.True(result.SimulatedSessionProof.TargetBindingByIdOrNamePassed);
        Assert.True(result.SimulatedSessionProof.ActionAvailabilityByDistancePassed);
        Assert.True(result.SimulatedSessionProof.ScriptedInteractionsApplied);
        Assert.True(result.SimulatedSessionProof.StateDeltaAppendPassed);
        Assert.True(result.SimulatedSessionProof.DeterministicStateHashChainPassed);
        Assert.True(result.SimulatedSessionProof.UnavailableActionRejected);
        Assert.True(result.SimulatedSessionProof.StateDeltasSeparateFromBaseData);
        Assert.Equal(result.Manifest.TargetCount, result.SimulatedSessionProof.TargetCount);
        Assert.Equal(result.Manifest.ActionKindCount, result.SimulatedSessionProof.ActionKindCount);
        Assert.Equal(result.Manifest.ScriptedEventCount, result.SimulatedSessionProof.ScriptedEventCount);
        Assert.Equal(result.Manifest.StateDeltaCount, result.SimulatedSessionProof.StateDeltaCount);
        Assert.Equal(
            result.StateDeltaPlan.StateDeltaCount + 1,
            result.StateDeltaPlan.StateHashChain.Count);
        Assert.Equal(result.StateDeltaPlan.InitialStateHash, result.StateDeltaPlan.StateHashChain.First());
        Assert.Equal(result.StateDeltaPlan.FinalStateHash, result.StateDeltaPlan.StateHashChain.Last());
        Assert.All(result.StateDeltaPlan.Deltas, delta =>
        {
            Assert.False(delta.MutatesBaseDataDirectly);
            Assert.NotEmpty(delta.PreviousStateHash);
            Assert.NotEmpty(delta.DeterministicStateHash);
        });
    }

    [Fact]
    public async Task NegativeProofCoversRequiredInteractionRisks()
    {
        var result = (await new OfflineGeoworldInteractionPlayableProbeEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.Equal(
            OfflineGeoworldInteractionPlayableProbeVocabulary.RequiredNegativeScenarioIds.Count,
            result.NegativeProof.ScenarioCount);
        Assert.Equal(result.NegativeProof.ScenarioCount, result.NegativeProof.RejectedCount);
        foreach (var scenarioId in OfflineGeoworldInteractionPlayableProbeVocabulary.RequiredNegativeScenarioIds)
        {
            Assert.Contains(
                result.NegativeProof.Scenarios,
                scenario => scenario.ScenarioId == scenarioId
                            && scenario.ActualStatus == "rejected"
                            && scenario.Diagnostics.Count > 0);
        }
    }

    [Fact]
    public async Task WorkspaceGroupSurfacesInteractionReadiness()
    {
        await new OfflineGeoworldInteractionPlayableProbeEvidenceService()
            .BuildAndWriteAsync(ProjectRoot());
        var result = new VisualWorldStreamPreviewWorkspaceService().Build(ProjectRoot());
        var group = Assert.Single(
            result.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_interactions");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_interaction_workspace_summary");

        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractionGroupPresent);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractionTargetCount >= 8);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractionActionKindCount >= 5);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractionScriptedEventCount >= 6);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractionStateDeltaCount >= 6);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractionStateHashChainPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractionUnityScriptsReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractionEditorWindowReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractionUnitySafetyScanPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractionSimulatedSessionProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractionNegativeProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractionAlphaRuntimeBootstrapUnchanged);
        Assert.True(result.QualityGateScan.OfflineGeoworldInteractionQualityGatePassed);
        Assert.True(result.QualityGateScan.Goal105FilesDiscoveredByRelativePaths);
        Assert.True(result.QualityGateScan.WinFormsOfflineGeoworldInteractionBindingReal);
        Assert.True(summary.OfflineGeoworldInteractionTargetCount >= 8);
        Assert.True(summary.OfflineGeoworldInteractionStateHashChainPassed);
        AssertProofPassed(result.ProofStatus.Proofs, "goal105.unity_script_inventory");
        AssertProofPassed(result.ProofStatus.Proofs, "goal105.editor_window_inventory");
        AssertProofPassed(result.ProofStatus.Proofs, "goal105.simulated_session");
        AssertProofPassed(result.ProofStatus.Proofs, "goal105.negative");
        AssertProofPassed(result.ProofStatus.Proofs, "goal105.alpha_runtime_bootstrap_unchanged");
        AssertProofPassed(result.ProofStatus.Proofs, "goal105.state_hash_chain");
        AssertProofPassed(result.ProofStatus.Proofs, "goal105.quality_gate");
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
