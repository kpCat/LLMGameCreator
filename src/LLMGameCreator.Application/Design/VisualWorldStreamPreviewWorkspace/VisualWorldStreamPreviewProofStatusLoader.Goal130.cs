using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal130GamePackageCandidateFactoryProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GamePackageCandidateFactoryProjectionVocabulary.ProceduralOutputDirectory;
        var goalId = GamePackageCandidateFactoryProjectionVocabulary.GoalId;
        if (!Goal130DashboardIsGreen(projectRoot, root))
        {
            return [];
        }

        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal130.gamepackage_candidate_factory.candidate_index",
                GamePackageCandidateFactoryProjectionVocabulary.CandidateIndexFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal130.gamepackage_candidate_factory.script_scan",
                GamePackageCandidateFactoryProjectionVocabulary.ScriptScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal130.gamepackage_candidate_factory.factory_result",
                GamePackageCandidateFactoryProjectionVocabulary.FactoryResultFileName,
                "matrixPassed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal130.gamepackage_candidate_factory.matrix_result",
                GamePackageCandidateFactoryProjectionVocabulary.MatrixResultFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal130.gamepackage_candidate_factory.log_scan",
                GamePackageCandidateFactoryProjectionVocabulary.LogScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal130.gamepackage_candidate_factory.negative_proof",
                GamePackageCandidateFactoryProjectionVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal130.gamepackage_candidate_factory.sample_unmodified",
                GamePackageCandidateFactoryProjectionVocabulary.DashboardFileName,
                "samplePackageUnmodified", ledger, diagnostics)
        ];
    }

    private static bool Goal130DashboardIsGreen(string projectRoot, string root)
    {
        var relativePath =
            root + "/" + GamePackageCandidateFactoryProjectionVocabulary.DashboardFileName;
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        try
        {
            using var dashboard = JsonDocument.Parse(File.ReadAllText(fullPath));
            return Goal130String(dashboard.RootElement, "candidateFactoryStatus") == "GREEN";
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
