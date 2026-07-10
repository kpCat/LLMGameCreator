using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal142ProductLineRuntimeVariantMatrixProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = ProductLineRuntimeVariantMatrixVocabulary.ProceduralOutputDirectory;
        var goalId = ProductLineRuntimeVariantMatrixVocabulary.GoalId;
        if (!Goal142DashboardIsGreen(projectRoot, root))
        {
            return [];
        }

        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal142.product_line_runtime_variant_matrix.distinctness",
                ProductLineRuntimeVariantMatrixVocabulary.DistinctnessProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal142.product_line_runtime_variant_matrix.result",
                ProductLineRuntimeVariantMatrixVocabulary.MatrixResultFileName,
                "sourceTemplateUnmodified", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal142.product_line_runtime_variant_matrix.negative_proof",
                ProductLineRuntimeVariantMatrixVocabulary.NegativeProofFileName,
                "noMetadataOnlyVariantAccepted", ledger, diagnostics),
            BuildProof(projectRoot, root + "/selected-runtime-variant", goalId,
                "goal142.product_line_runtime_variant_matrix.selected_handoff",
                "selected-runtime-variant-handoff.json",
                "runtimeSignificant", ledger, diagnostics)
        ];
    }

    private static bool Goal142DashboardIsGreen(string projectRoot, string root)
    {
        var relativePath = root + "/" + ProductLineRuntimeVariantMatrixVocabulary.DashboardFileName;
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        try
        {
            using var dashboard = JsonDocument.Parse(File.ReadAllText(fullPath));
            return Goal138String(dashboard.RootElement, "matrixStatus") == "GREEN";
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
