using LLMGameCreator.Application.Design.OfflineGeoworldVisualCacheUnityHandoff;
using LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus> BuildProofStatus(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        const string sourceGoalId = "goal_091_deterministic_visual_chunk_stream_window";
        const string sourceRoot =
            ".llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window";
        var ledger = LoadLedger(projectRoot, sourceRoot, "visual-chunk-stream-file-ledger.json");
        var proofDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var proofs = new List<VisualWorldPreviewProofStatus>
        {
            BuildProof(
                projectRoot,
                sourceRoot,
                sourceGoalId,
                "goal091.seam",
                "visual-chunk-stream-seam-proof.json",
                "passed",
                ledger,
                proofDiagnostics),
            BuildProof(
                projectRoot,
                sourceRoot,
                sourceGoalId,
                "goal091.cache_reuse",
                "visual-chunk-stream-cache-reuse-proof.json",
                "passed",
                ledger,
                proofDiagnostics),
            BuildProof(
                projectRoot,
                sourceRoot,
                sourceGoalId,
                "goal091.layer_transition",
                "visual-chunk-stream-layer-transition-proof.json",
                "passed",
                ledger,
                proofDiagnostics),
            BuildProof(
                projectRoot,
                sourceRoot,
                sourceGoalId,
                "goal091.negative",
                "visual-chunk-stream-negative-proof.json",
                "passed",
                ledger,
                proofDiagnostics),
            BuildProof(
                projectRoot,
                sourceRoot,
                sourceGoalId,
                "goal091.finite_boundary_clipping",
                "visual-chunk-stream-quality-gate-scan.json",
                "boundaryClippingExplicit",
                ledger,
                proofDiagnostics),
            BuildProof(
                projectRoot,
                sourceRoot,
                sourceGoalId,
                "goal091.huge_sparse_no_raw_dump",
                "visual-chunk-stream-quality-gate-scan.json",
                "hugeSparseNoRawDump",
                ledger,
                proofDiagnostics),
            BuildProof(
                projectRoot,
                sourceRoot,
                sourceGoalId,
                "goal091.infinite_overlap_reuse",
                "visual-chunk-stream-quality-gate-scan.json",
                "infiniteOverlapReuseProven",
                ledger,
                proofDiagnostics)
        };
        proofs.AddRange(BuildGoal093CacheProofStatus(projectRoot, proofDiagnostics));
        proofs.AddRange(BuildGoal095UnityHandoffProofStatus(projectRoot, proofDiagnostics));
        proofs.AddRange(BuildGoal099GeoworldProofStatus(projectRoot, proofDiagnostics));
        proofs.AddRange(BuildGoal100OfflineGeoworldHandoffProofStatus(projectRoot, proofDiagnostics));

        diagnostics.AddRange(proofDiagnostics);
        return proofs.OrderBy(item => item.ProofId, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<VisualWorldPreviewProofStatus> BuildGoal093CacheProofStatus(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var ledger = LoadLedger(projectRoot, Goal093SourceRoot, "visual-chunk-cache-file-ledger.json");
        return
        [
            BuildProof(
                projectRoot,
                Goal093SourceRoot,
                Goal093SourceGoalId,
                "goal093.readback",
                "visual-chunk-cache-readback-proof.json",
                "passed",
                ledger,
                diagnostics),
            BuildProof(
                projectRoot,
                Goal093SourceRoot,
                Goal093SourceGoalId,
                "goal093.overlap_reuse",
                "visual-chunk-cache-overlap-reuse-proof.json",
                "passed",
                ledger,
                diagnostics),
            BuildProof(
                projectRoot,
                Goal093SourceRoot,
                Goal093SourceGoalId,
                "goal093.negative",
                "visual-chunk-cache-negative-proof.json",
                "passed",
                ledger,
                diagnostics),
            BuildProof(
                projectRoot,
                Goal093SourceRoot,
                Goal093SourceGoalId,
                "goal093.invalidation_matrix",
                "visual-chunk-cache-invalidation-matrix.json",
                "passed",
                ledger,
                diagnostics),
            BuildProof(
                projectRoot,
                Goal093SourceRoot,
                Goal093SourceGoalId,
                "goal093.runtime_handoff_metadata_only",
                "visual-chunk-cache-runtime-handoff-sidecar.json",
                "metadataOnly",
                ledger,
                diagnostics)
        ];
    }

    private static IReadOnlyList<VisualWorldPreviewProofStatus> BuildGoal099GeoworldProofStatus(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics) =>
        [
            BuildProof(
                projectRoot,
                Goal099SourceRoot,
                Goal099SourceGoalId,
                "goal099.boundary_prefetch",
                OfflineGeoworldWorldSourceGraphEvidenceService.BoundaryPrefetchProofJsonFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal099SourceRoot,
                Goal099SourceGoalId,
                "goal099.negative",
                OfflineGeoworldWorldSourceGraphEvidenceService.NegativeProofJsonFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal099SourceRoot,
                Goal099SourceGoalId,
                "goal099.visual_projection",
                OfflineGeoworldWorldSourceGraphEvidenceService.VisualProjectionSummaryJsonFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal099SourceRoot,
                Goal099SourceGoalId,
                "goal099.quality_gate",
                OfflineGeoworldWorldSourceGraphEvidenceService.QualityGateScanJsonFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics)
        ];

    private static IReadOnlyList<VisualWorldPreviewProofStatus> BuildGoal095UnityHandoffProofStatus(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics) =>
        [
            BuildProof(
                projectRoot,
                Goal095SourceRoot,
                Goal095SourceGoalId,
                "goal095.streamingassets_ledger",
                "visual-chunk-cache-unity-streamingassets-ledger.json",
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal095SourceRoot,
                Goal095SourceGoalId,
                "goal095.simulated_read",
                "visual-chunk-cache-unity-simulated-read-proof.json",
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal095SourceRoot,
                Goal095SourceGoalId,
                "goal095.negative",
                "visual-chunk-cache-unity-negative-proof.json",
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal095SourceRoot,
                Goal095SourceGoalId,
                "goal095.probe_source_inventory",
                "visual-chunk-cache-unity-probe-source-inventory.json",
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal095SourceRoot,
                Goal095SourceGoalId,
                "goal095.alpha_runtime_bootstrap_unchanged",
                "visual-chunk-cache-unity-quality-gate-scan.json",
                "alphaRuntimeBootstrapUnchanged",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal095SourceRoot,
                Goal095SourceGoalId,
                "goal095.forbidden_unity_areas_unchanged",
                "visual-chunk-cache-unity-quality-gate-scan.json",
                "noForbiddenUnityAreasChanged",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal095SourceRoot,
                Goal095SourceGoalId,
                "goal095.metadata_only",
                "visual-chunk-cache-unity-handoff-manifest.json",
                "runtimeHandoffSidecarMetadataOnly",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics)
        ];

    private static IReadOnlyList<VisualWorldPreviewProofStatus> BuildGoal100OfflineGeoworldHandoffProofStatus(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics) =>
        [
            BuildProof(
                projectRoot,
                Goal100SourceRoot,
                Goal100SourceGoalId,
                "goal100.streamingassets_ledger",
                OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityStreamingAssetsLedgerFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal100SourceRoot,
                Goal100SourceGoalId,
                "goal100.simulated_read",
                OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnitySimulatedReadProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal100SourceRoot,
                Goal100SourceGoalId,
                "goal100.negative",
                OfflineGeoworldVisualCacheUnityHandoffVocabulary.NegativeProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal100SourceRoot,
                Goal100SourceGoalId,
                "goal100.probe_source_inventory",
                OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityProbeSourceInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal100SourceRoot,
                Goal100SourceGoalId,
                "goal100.alpha_runtime_bootstrap_unchanged",
                OfflineGeoworldVisualCacheUnityHandoffVocabulary.QualityGateScanFileName,
                "alphaRuntimeBootstrapUnchanged",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal100SourceRoot,
                Goal100SourceGoalId,
                "goal100.visual_cache_records",
                OfflineGeoworldVisualCacheUnityHandoffVocabulary.QualityGateScanFileName,
                "visualCacheRecordsBuilt",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal100SourceRoot,
                Goal100SourceGoalId,
                "goal100.all_feature_kinds_mapped",
                OfflineGeoworldVisualCacheUnityHandoffVocabulary.QualityGateScanFileName,
                "allFeatureKindsMapped",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal100SourceRoot,
                Goal100SourceGoalId,
                "goal100.workspace_binding",
                OfflineGeoworldVisualCacheUnityHandoffVocabulary.WorkspaceBindingInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal100SourceRoot,
                Goal100SourceGoalId,
                "goal100.quality_gate",
                OfflineGeoworldVisualCacheUnityHandoffVocabulary.QualityGateScanFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics)
        ];

    private static VisualWorldPreviewProofStatus BuildProof(
        string projectRoot,
        string sourceRoot,
        string sourceGoalId,
        string proofId,
        string fileName,
        string booleanProperty,
        IReadOnlyDictionary<string, string> ledger,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var relativePath = sourceRoot + "/" + fileName;
        var passed = false;
        var summary = "proof missing";
        using var doc = TryReadJson(projectRoot, relativePath, diagnostics);
        if (doc is not null)
        {
            passed = TryGetBool(doc.RootElement, booleanProperty);
            summary = BuildProofSummary(doc.RootElement, booleanProperty, passed);
        }

        if (!passed)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.proof.failed",
                proofId,
                "Required visual preview proof is missing or did not pass."));
        }

        return new VisualWorldPreviewProofStatus
        {
            ProofId = proofId,
            SourceGoalId = sourceGoalId,
            RelativePath = relativePath,
            Status = passed
                ? VisualWorldPreviewArtifactStatus.Passed
                : VisualWorldPreviewArtifactStatus.Failed,
            Passed = passed,
            Sha256 = File.Exists(Resolve(projectRoot, relativePath))
                ? HashFor(projectRoot, relativePath, ledger)
                : string.Empty,
            DiagnosticSummary = summary
        };
    }
}
