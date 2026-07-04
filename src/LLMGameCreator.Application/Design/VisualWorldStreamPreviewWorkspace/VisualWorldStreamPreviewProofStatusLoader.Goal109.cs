using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceExportPackage;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal109OfflineGeoworldAlphaExportPackageProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaSliceExportPackageVocabulary.ProceduralOutputDirectory;
        var goalId = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal109.alpha_export.manifest",
                OfflineGeoworldAlphaSliceExportPackageVocabulary.ManifestFileName,
                "alphaRuntimeBootstrapUnchanged", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal109.alpha_export.file_index",
                OfflineGeoworldAlphaSliceExportPackageVocabulary.FileIndexFileName,
                "packageRelativePathsOnly", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal109.alpha_export.checksums",
                OfflineGeoworldAlphaSliceExportPackageVocabulary.ChecksumsFileName,
                "checksumsFileSelfExcluded", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal109.alpha_export.clean_import",
                OfflineGeoworldAlphaSliceExportPackageVocabulary.CleanImportProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal109.alpha_export.negative_proof",
                OfflineGeoworldAlphaSliceExportPackageVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal109.alpha_export.unity_verifier",
                OfflineGeoworldAlphaSliceExportPackageVocabulary.UnityScriptInventoryFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal109.alpha_export.editor_window",
                OfflineGeoworldAlphaSliceExportPackageVocabulary.EditorWindowInventoryFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal109.alpha_export.workspace_binding",
                OfflineGeoworldAlphaSliceExportPackageVocabulary.WorkspaceBindingInventoryFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal109.alpha_export.source_lineage",
                OfflineGeoworldAlphaSliceExportPackageVocabulary.SourceLineageFileName,
                "goal108AImmutabilityAuditRead", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal109.alpha_export.quality_gate",
                OfflineGeoworldAlphaSliceExportPackageVocabulary.QualityGateScanFileName,
                "passed", ledger, diagnostics)
        ];
    }
}
