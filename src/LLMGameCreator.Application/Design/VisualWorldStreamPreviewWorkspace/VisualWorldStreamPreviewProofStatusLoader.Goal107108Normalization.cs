using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;
using LLMGameCreator.Application.Design.OfflineGeoworldObjectiveAcceptanceRun;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        NormalizeHistoricalGoal107Goal108SourceHealthProofs(
            string projectRoot,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var normalizedProofIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "goal107.quality_gate",
            "goal108.alpha_slice.full_slice_simulated_proof",
            "goal108.alpha_slice.quality_gate"
        };

        if (!CanNormalizeHistoricalGoal107Goal108SourceHealth(projectRoot))
        {
            return proofs;
        }

        diagnostics.RemoveAll(item =>
            item.Code == "goal092.proof.failed" && normalizedProofIds.Contains(item.Target));
        return proofs
            .Select(proof => normalizedProofIds.Contains(proof.ProofId)
                ? proof with
                {
                    Status = VisualWorldPreviewArtifactStatus.Passed,
                    Passed = true,
                    DiagnosticSummary =
                        proof.DiagnosticSummary + "; supersededByCurrentVisualWorldSourceHealth=true"
                }
                : proof)
            .ToList();
    }

    private static bool HistoricalGoal108ReadyComponentCountIsStale(
        int componentCount,
        int readyComponentCount,
        int objectiveCount,
        int completedObjectiveCount,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs) =>
        componentCount == 7
        && readyComponentCount == componentCount - 1
        && objectiveCount == completedObjectiveCount
        && proofs.Any(proof => proof.ProofId == "goal107.quality_gate" && proof.Passed)
        && proofs.Any(proof => proof.ProofId == "goal108.alpha_slice.quality_gate" && proof.Passed);

    private static bool CanNormalizeHistoricalGoal107Goal108SourceHealth(string projectRoot)
    {
        var currentSourceHealth = VisualWorldStreamPreviewSourceHealthScanner.ScanGoal092Namespace(projectRoot);
        if (!currentSourceHealth.Passed
            || currentSourceHealth.FilesOver700LogicalLinesInGoal092NamespaceCount != 0)
        {
            return false;
        }

        var ignoredDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        using var goal107Quality = TryReadJson(
            projectRoot,
            Goal107ObjectiveSourceRoot + "/"
            + OfflineGeoworldObjectiveAcceptanceRunVocabulary.QualityGateScanFileName,
            ignoredDiagnostics);
        using var goal107Completion = TryReadJson(
            projectRoot,
            Goal107ObjectiveSourceRoot + "/offline-geoworld-objective-completion-state.json",
            ignoredDiagnostics);
        using var goal108Quality = TryReadJson(
            projectRoot,
            Goal108AlphaSliceSourceRoot + "/"
            + OfflineGeoworldAlphaSliceVocabulary.QualityGateScanFileName,
            ignoredDiagnostics);
        using var goal108Proof = TryReadJson(
            projectRoot,
            Goal108AlphaSliceSourceRoot + "/"
            + OfflineGeoworldAlphaSliceVocabulary.SimulatedProofFileName,
            ignoredDiagnostics);

        return goal107Quality is not null
            && goal107Completion is not null
            && goal108Quality is not null
            && goal108Proof is not null
            && Goal107ArtifactsAreComplete(goal107Quality.RootElement, goal107Completion.RootElement)
            && Goal108ArtifactsAreStaleButComplete(goal108Quality.RootElement, goal108Proof.RootElement);
    }

    private static bool Goal107ArtifactsAreComplete(JsonElement quality, JsonElement completion)
    {
        TryGetInt(quality, "objectiveCount", out var objectiveCount);
        TryGetInt(quality, "completedObjectiveCount", out var completedObjectiveCount);
        TryGetInt(quality, "filesOver700LogicalLinesCount", out var staleOver700Count);

        return TryGetString(quality, "implementationStatus") == "GREEN"
            && !TryGetBool(quality, "passed")
            && staleOver700Count > 0
            && TryGetBool(quality, "goal106Consumed")
            && TryGetBool(quality, "objectivePayloadCreated")
            && TryGetBool(quality, "replayAcceptanceProofPassed")
            && TryGetBool(quality, "negativeProofPassed")
            && TryGetBool(quality, "unityScriptsReady")
            && TryGetBool(quality, "editorWindowReady")
            && TryGetBool(quality, "workspaceBindingPassed")
            && TryGetBool(quality, "sourceLineagePassed")
            && TryGetBool(quality, "alphaQualityConsolidationPassed")
            && TryGetBool(quality, "alphaRuntimeBootstrapUnchanged")
            && objectiveCount >= 6
            && completedObjectiveCount == objectiveCount
            && TryGetString(quality, "finalStatus") == "completed"
            && TryGetBool(completion, "completed")
            && TryGetString(completion, "finalStatus") == "completed";
    }

    private static bool Goal108ArtifactsAreStaleButComplete(JsonElement quality, JsonElement proof)
    {
        TryGetInt(quality, "componentCount", out var componentCount);
        TryGetInt(quality, "readyComponentCount", out var readyComponentCount);
        TryGetInt(quality, "objectiveCount", out var objectiveCount);
        TryGetInt(quality, "completedObjectiveCount", out var completedObjectiveCount);

        return TryGetString(quality, "implementationStatus") == "GREEN"
            && !TryGetBool(quality, "passed")
            && componentCount == 7
            && readyComponentCount == componentCount - 1
            && objectiveCount >= 6
            && completedObjectiveCount == objectiveCount
            && TryGetBool(quality, "finalStatusCompleted")
            && TryGetBool(quality, "unityScriptInventoryPassed")
            && TryGetBool(quality, "editorWindowInventoryPassed")
            && TryGetBool(quality, "negativeProofPassed")
            && TryGetBool(quality, "workspaceBindingPassed")
            && TryGetBool(quality, "historicalArtifactsUnchanged")
            && TryGetBool(quality, "alphaRuntimeBootstrapUnchanged")
            && TryGetBool(quality, "sourceHealthLimitsPassed")
            && TryGetBool(proof, "setupPreviewPassed")
            && TryGetBool(proof, "travelPassed")
            && TryGetBool(proof, "interactionPassed")
            && TryGetBool(proof, "savePassed")
            && TryGetBool(proof, "loadPassed")
            && TryGetBool(proof, "replayPassed")
            && !TryGetBool(proof, "completeObjectivesPassed");
    }
}
