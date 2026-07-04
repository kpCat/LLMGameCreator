using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldAlphaSliceManualAcceptanceGate;

public sealed class OfflineGeoworldAlphaSliceManualAcceptanceGateTests
{
    [Fact]
    public async Task BuildAndWriteCreatesManualAcceptancePayloadEvidenceAndWorkspaceGroup()
    {
        var root = ProjectRoot();
        var write = await new OfflineGeoworldAlphaSliceManualAcceptanceGateEvidenceService()
            .BuildAndWriteAsync(root);
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.FinalGate,
            result.Report.ManualGate);
        Assert.True(
            result.QualityGateScan.Passed,
            string.Join(Environment.NewLine, result.QualityGateScan.Diagnostics));
        Assert.True(result.Manifest.AutomatedGatePassed);
        Assert.False(result.Manifest.Accepted);
        Assert.True(result.Manifest.ManualAcceptancePending);
        Assert.True(result.Manifest.AlphaRuntimeBootstrapUnchanged);
        Assert.Equal(5, result.Manifest.PayloadFileCount);
        Assert.Equal(7, result.Manifest.ExportFileCount);
        Assert.Equal(
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredChecklistStepIds.Count,
            result.Checklist.StepCount);
        Assert.True(result.SimulatedProof.Passed);
        Assert.True(result.SimulatedProof.SyntheticResultLoaded);
        Assert.True(result.SimulatedProof.ResultHashValidationPassed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.UnityScriptInventory.Passed);
        Assert.True(result.EditorWindowInventory.Passed);
        Assert.True(result.WorkspaceBindingInventory.Passed);

        AssertFilesExist(write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredPayloadFileNames);
        AssertFilesExist(write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredEvidenceFileNames);
        AssertFilesExist(write.ExportPackageDirectoryPath,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredExportFileNames);
        AssertFilesExist(write.StreamingAssetsDirectoryPath,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredPayloadFileNames);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_alpha_manual_acceptance");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_alpha_manual_acceptance_workspace_summary");

        Assert.True(
            workspace.QualityGateScan.Passed,
            string.Join(Environment.NewLine, workspace.QualityGateScan.Diagnostics.Select(item =>
                item.Code + " [" + item.Target + "] " + item.Message)));
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualAcceptanceGroupPresent);
        Assert.Equal(12, workspace.QualityGateScan.OfflineGeoworldAlphaManualAcceptanceChecklistStepCount);
        Assert.Equal(5, workspace.QualityGateScan.OfflineGeoworldAlphaManualAcceptancePayloadFileCount);
        Assert.Equal(7, workspace.QualityGateScan.OfflineGeoworldAlphaManualAcceptanceExportFileCount);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualAcceptanceAutomatedGatePassed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualAcceptanceManualPending);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualAcceptanceUnityRunnerReady);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualAcceptanceSimulatedProofPassed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualAcceptanceNegativeProofPassed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualAcceptanceWorkspaceBindingPassed);
        Assert.True(
            workspace.QualityGateScan.OfflineGeoworldAlphaManualAcceptanceAlphaRuntimeBootstrapUnchanged);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualAcceptanceQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal110FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.WinFormsOfflineGeoworldAlphaManualAcceptanceBindingReal);
        Assert.Equal(12, summary.OfflineGeoworldAlphaManualAcceptanceChecklistStepCount);
        Assert.True(summary.OfflineGeoworldAlphaManualAcceptanceManualPending);
        Assert.True(summary.OfflineGeoworldAlphaManualAcceptanceUnityRunnerReady);
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal110.manual_acceptance.manifest");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal110.manual_acceptance.simulated_proof");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal110.manual_acceptance.quality_gate");
    }

    [Fact]
    public void NegativeProofCoversRequiredManualAcceptanceFailureClasses()
    {
        var result = new OfflineGeoworldAlphaSliceManualAcceptanceGateEvidenceService()
            .Build(ProjectRoot());

        Assert.True(result.NegativeProof.Passed);
        Assert.Equal(
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredNegativeScenarioIds.Count,
            result.NegativeProof.ScenarioCount);
        Assert.Equal(result.NegativeProof.ScenarioCount, result.NegativeProof.RejectedCount);
        foreach (var scenarioId in
                 OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredNegativeScenarioIds)
        {
            Assert.Contains(
                result.NegativeProof.Scenarios,
                scenario => scenario.ScenarioId == scenarioId
                            && scenario.ActualStatus == "rejected"
                            && scenario.Diagnostic.Length > 0);
        }
    }

    private static void AssertFilesExist(string directory, IReadOnlyList<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            Assert.True(File.Exists(Path.Combine(directory, fileName)), fileName);
        }
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
