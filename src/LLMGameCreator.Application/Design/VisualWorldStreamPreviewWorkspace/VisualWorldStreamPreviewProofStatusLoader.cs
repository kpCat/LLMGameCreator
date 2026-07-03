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
                "Required Goal 091 proof is missing or did not pass."));
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
