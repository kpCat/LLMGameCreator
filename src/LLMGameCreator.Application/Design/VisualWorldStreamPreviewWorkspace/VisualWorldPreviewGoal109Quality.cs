using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceExportPackage;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal109AlphaExportPackageWorkspaceQuality BuildGoal109AlphaExportPackageQuality(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item => item.GroupId == "offline_geoworld_alpha_export_package");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_alpha_export_package_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal109AllowedPath(entry.RelativePath));
        return new Goal109AlphaExportPackageWorkspaceQuality(
            GroupPresent: group is not null,
            PackageFileCount: summary?.OfflineGeoworldAlphaExportPackageFileCount ?? 0,
            IndexedFileCount: summary?.OfflineGeoworldAlphaExportIndexedFileCount ?? 0,
            ChecksumStatus: summary?.OfflineGeoworldAlphaExportChecksumStatus ?? string.Empty,
            CleanImportProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal109.alpha_export.clean_import" && proof.Passed),
            NegativeProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal109.alpha_export.negative_proof" && proof.Passed),
            UnityVerifierReady: proofs.Any(proof =>
                proof.ProofId == "goal109.alpha_export.unity_verifier" && proof.Passed),
            EditorWindowReady: proofs.Any(proof =>
                proof.ProofId == "goal109.alpha_export.editor_window" && proof.Passed),
            WorkspaceBindingPassed: proofs.Any(proof =>
                proof.ProofId == "goal109.alpha_export.workspace_binding" && proof.Passed),
            SourceLineagePassed: proofs.Any(proof =>
                proof.ProofId == "goal109.alpha_export.source_lineage" && proof.Passed),
            RunbookSummary: summary?.OfflineGeoworldAlphaExportRunbookSummary ?? string.Empty,
            AcceptanceGateStatus: summary?.OfflineGeoworldAlphaExportAcceptanceGateStatus ?? string.Empty,
            AlphaRuntimeBootstrapUnchanged: proofs.Any(proof =>
                proof.ProofId == "goal109.alpha_export.manifest" && proof.Passed),
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal109.alpha_export.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal109AlphaExportPackageQualityDiagnostics(
        Goal109AlphaExportPackageWorkspaceQuality alphaExport,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(alphaExport.GroupPresent, "goal109.quality.alpha_export_group",
            "offline_geoworld_alpha_export_package", diagnostics);
        AddIfFalse(alphaExport.PackageFileCount == 6, "goal109.quality.package_file_count",
            "offline_geoworld_alpha_export_package", diagnostics);
        AddIfFalse(alphaExport.IndexedFileCount == 5, "goal109.quality.indexed_file_count",
            "offline_geoworld_alpha_export_package", diagnostics);
        AddIfFalse(alphaExport.ChecksumStatus == "matched", "goal109.quality.checksums",
            "offline_geoworld_alpha_export_package", diagnostics);
        AddIfFalse(alphaExport.CleanImportProofPassed, "goal109.quality.clean_import",
            "proofStatus", diagnostics);
        AddIfFalse(alphaExport.NegativeProofPassed, "goal109.quality.negative_proof",
            "proofStatus", diagnostics);
        AddIfFalse(alphaExport.UnityVerifierReady, "goal109.quality.unity_verifier",
            "proofStatus", diagnostics);
        AddIfFalse(alphaExport.EditorWindowReady, "goal109.quality.editor_window",
            "proofStatus", diagnostics);
        AddIfFalse(alphaExport.WorkspaceBindingPassed, "goal109.quality.workspace_binding",
            "proofStatus", diagnostics);
        AddIfFalse(alphaExport.SourceLineagePassed, "goal109.quality.source_lineage",
            "proofStatus", diagnostics);
        AddIfFalse(alphaExport.AcceptanceGateStatus == "required",
            "goal109.quality.acceptance_gate", "offline_geoworld_alpha_export_package", diagnostics);
        AddIfFalse(alphaExport.AlphaRuntimeBootstrapUnchanged,
            "goal109.quality.alpha_bootstrap", "proofStatus", diagnostics);
        AddIfFalse(alphaExport.QualityGatePassed, "goal109.quality.quality_gate",
            "proofStatus", diagnostics);
        AddIfFalse(alphaExport.RelativePaths, "goal109.quality.relative_paths",
            "offline_geoworld_alpha_export_package", diagnostics);
    }

    private static bool Goal109AllowedPath(string path) =>
        path.StartsWith(
            OfflineGeoworldAlphaSliceExportPackageVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            OfflineGeoworldAlphaSliceExportPackageVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            OfflineGeoworldAlphaSliceExportPackageVocabulary.StreamingAssetsRelativeRoot + "/",
            StringComparison.Ordinal)
        || path == OfflineGeoworldAlphaSliceExportPackageVocabulary.UnityVerifierScriptPath
        || path == OfflineGeoworldAlphaSliceExportPackageVocabulary.UnityEditorWindowScriptPath;

    private static VisualWorldPreviewWorkspaceQualityGate ApplyGoal109AlphaExportPackageQuality(
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        Goal109AlphaExportPackageWorkspaceQuality alphaExport,
        VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            OfflineGeoworldAlphaExportPackageGroupPresent = alphaExport.GroupPresent,
            OfflineGeoworldAlphaExportPackageFileCount = alphaExport.PackageFileCount,
            OfflineGeoworldAlphaExportIndexedFileCount = alphaExport.IndexedFileCount,
            OfflineGeoworldAlphaExportChecksumStatus = alphaExport.ChecksumStatus,
            OfflineGeoworldAlphaExportCleanImportProofPassed = alphaExport.CleanImportProofPassed,
            OfflineGeoworldAlphaExportNegativeProofPassed = alphaExport.NegativeProofPassed,
            OfflineGeoworldAlphaExportUnityVerifierReady = alphaExport.UnityVerifierReady,
            OfflineGeoworldAlphaExportEditorWindowReady = alphaExport.EditorWindowReady,
            OfflineGeoworldAlphaExportWorkspaceBindingPassed = alphaExport.WorkspaceBindingPassed,
            OfflineGeoworldAlphaExportSourceLineagePassed = alphaExport.SourceLineagePassed,
            OfflineGeoworldAlphaExportRunbookSummary = alphaExport.RunbookSummary,
            OfflineGeoworldAlphaExportAcceptanceGateStatus = alphaExport.AcceptanceGateStatus,
            OfflineGeoworldAlphaExportAlphaRuntimeBootstrapUnchanged =
                alphaExport.AlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldAlphaExportQualityGatePassed = alphaExport.QualityGatePassed,
            Goal109FilesDiscoveredByRelativePaths = alphaExport.RelativePaths,
            WinFormsOfflineGeoworldAlphaExportPackageBindingReal =
                binding.PageBindDisplaysOfflineGeoworldAlphaExportPackage
        };

    private sealed record Goal109AlphaExportPackageWorkspaceQuality(
        bool GroupPresent,
        int PackageFileCount,
        int IndexedFileCount,
        string ChecksumStatus,
        bool CleanImportProofPassed,
        bool NegativeProofPassed,
        bool UnityVerifierReady,
        bool EditorWindowReady,
        bool WorkspaceBindingPassed,
        bool SourceLineagePassed,
        string RunbookSummary,
        string AcceptanceGateStatus,
        bool AlphaRuntimeBootstrapUnchanged,
        bool QualityGatePassed,
        bool RelativePaths);
}
