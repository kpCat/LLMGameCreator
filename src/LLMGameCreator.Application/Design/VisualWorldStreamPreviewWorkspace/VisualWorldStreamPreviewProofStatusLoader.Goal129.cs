using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal129GamePackageCandidateMatrixProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory;
        var goalId = GamePackageCandidateMatrixProjectionVocabulary.GoalId;
        if (!Goal129DashboardIsGreen(projectRoot, root))
        {
            return [];
        }

        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal129.gamepackage_candidate_matrix.candidate_index",
                GamePackageCandidateMatrixProjectionVocabulary.CandidateIndexFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal129.gamepackage_candidate_matrix.script_scan",
                GamePackageCandidateMatrixProjectionVocabulary.ScriptScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal129.gamepackage_candidate_matrix.matrix_result",
                GamePackageCandidateMatrixProjectionVocabulary.MatrixResultFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal129.gamepackage_candidate_matrix.log_scan",
                GamePackageCandidateMatrixProjectionVocabulary.LogScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal129.gamepackage_candidate_matrix.negative_proof",
                GamePackageCandidateMatrixProjectionVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal129.gamepackage_candidate_matrix.cleanup_applied",
                GamePackageCandidateMatrixProjectionVocabulary.DashboardFileName,
                "cleanupApplied", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal129.gamepackage_candidate_matrix.sample_unmodified",
                GamePackageCandidateMatrixProjectionVocabulary.DashboardFileName,
                "samplePackageUnmodified", ledger, diagnostics)
        ];
    }

    private static bool Goal129DashboardIsGreen(string projectRoot, string root)
    {
        var relativePath =
            root + "/" + GamePackageCandidateMatrixProjectionVocabulary.DashboardFileName;
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        try
        {
            using var dashboard = JsonDocument.Parse(File.ReadAllText(fullPath));
            return Goal129String(dashboard.RootElement, "matrixStatus") == "GREEN";
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
