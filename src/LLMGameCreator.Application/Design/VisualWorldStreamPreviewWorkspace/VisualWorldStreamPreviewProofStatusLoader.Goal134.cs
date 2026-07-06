using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal134CanonicalRuntimeSelectedCandidateProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ProceduralOutputDirectory;
        var goalId = CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.GoalId;
        if (!Goal134DashboardIsGreen(projectRoot, root))
        {
            return [];
        }

        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal134.canonical_runtime.dashboard",
                CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.DashboardFileName,
                "canonicalRuntimePassed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal134.canonical_runtime.package_validation",
                CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.PackageValidationFileName,
                "packageValidationPassed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal134.canonical_runtime.save_load_replay",
                CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.SaveLoadReplayResultFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal134.canonical_runtime.unity_transcript_smoke",
                CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.UnitySmokeFileName,
                "unityPlayerConsumedCanonicalTranscript", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal134.canonical_runtime.matrix",
                CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.MatrixResultFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal134.canonical_runtime.negative_proof",
                CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics)
        ];
    }

    private static bool Goal134DashboardIsGreen(string projectRoot, string root)
    {
        var relativePath =
            root + "/" + CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.DashboardFileName;
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        try
        {
            using var dashboard = JsonDocument.Parse(File.ReadAllText(fullPath));
            return Goal134String(dashboard.RootElement, "status") == "GREEN";
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
