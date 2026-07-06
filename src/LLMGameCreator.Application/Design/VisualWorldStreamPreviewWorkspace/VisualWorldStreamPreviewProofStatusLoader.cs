using LLMGameCreator.Application.Design.OfflineGeoworldVisualCacheUnityHandoff;
using LLMGameCreator.Application.Design.OfflineGeoworldInteractiveTravelPreview;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityPlayModeTravelPreview;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityPreviewRunner;
using LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;
using LLMGameCreator.Application.Design.OfflineGeoworldAcceptedAlphaBaselineReview;
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
        proofs.AddRange(BuildGoal101OfflineGeoworldUnityPreviewProofStatus(projectRoot, proofDiagnostics));
        proofs.AddRange(BuildGoal102OfflineGeoworldUnityEditorPreviewProofStatus(projectRoot, proofDiagnostics));
        proofs.AddRange(BuildGoal103OfflineGeoworldPlayModeTravelProofStatus(projectRoot, proofDiagnostics));
        proofs.AddRange(BuildGoal104OfflineGeoworldInteractiveTravelProofStatus(projectRoot, proofDiagnostics));
        proofs.AddRange(BuildGoal105OfflineGeoworldInteractionProofStatus(projectRoot, proofDiagnostics));
        proofs.AddRange(BuildGoal106OfflineGeoworldSessionProofStatus(projectRoot, proofDiagnostics));
        proofs.AddRange(BuildGoal107OfflineGeoworldObjectiveProofStatus(projectRoot, proofDiagnostics));
        proofs.AddRange(BuildGoal108OfflineGeoworldAlphaSliceProofStatus(projectRoot, proofDiagnostics));
        proofs.AddRange(BuildGoal109OfflineGeoworldAlphaExportPackageProofStatus(projectRoot, proofDiagnostics));
        proofs.AddRange(BuildGoal110OfflineGeoworldAlphaManualAcceptanceProofStatus(
            projectRoot,
            proofDiagnostics));
        proofs.AddRange(BuildGoal111OfflineGeoworldAlphaManualResultIntakeProofStatus(
            projectRoot,
            proofDiagnostics));
        proofs.AddRange(BuildGoal112OfflineGeoworldAlphaAcceptanceOperatorPackProofStatus(
            projectRoot,
            proofDiagnostics));
        proofs.AddRange(BuildGoal113OfflineGeoworldAlphaManualResultWorkbenchProofStatus(
            projectRoot,
            proofDiagnostics));
        proofs.AddRange(BuildGoal115OfflineGeoworldAlphaHumanResultRevalidationProofStatus(
            projectRoot,
            proofDiagnostics));
        proofs.AddRange(BuildGoal116OfflineGeoworldAlphaManualGateAcceptanceRecordProofStatus(
            projectRoot,
            proofDiagnostics));
        proofs.AddRange(BuildGoal117OfflineGeoworldAlphaPostAcceptanceContinuationProofStatus(
            projectRoot,
            proofDiagnostics));
        proofs.AddRange(BuildGoal118OfflineGeoworldAcceptedAlphaBaselineReviewProofStatus(
            projectRoot,
            proofDiagnostics));
        proofs.AddRange(BuildGoal119AcceptedAlphaUnityPlayableProjectionProofStatus(
            projectRoot,
            proofDiagnostics));
        proofs.AddRange(BuildGoal120AcceptedAlphaProjectionUsabilityProofStatus(
            projectRoot,
            proofDiagnostics));
        proofs.AddRange(BuildGoal121AcceptedAlphaInteractionDrilldownProofStatus(
            projectRoot,
            proofDiagnostics));
        proofs.AddRange(BuildGoal122AcceptedAlphaProjectionActionLoopProofStatus(
            projectRoot,
            proofDiagnostics));

        proofs = NormalizeHistoricalManualResultNegativeProofs(proofs, proofDiagnostics).ToList();
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

    private static IReadOnlyList<VisualWorldPreviewProofStatus> BuildGoal101OfflineGeoworldUnityPreviewProofStatus(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics) =>
        [
            BuildProof(
                projectRoot,
                Goal101SourceRoot,
                Goal101SourceGoalId,
                "goal101.streamingassets_ledger",
                OfflineGeoworldUnityPreviewRunnerVocabulary.StreamingAssetsLedgerFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal101SourceRoot,
                Goal101SourceGoalId,
                "goal101.unity_script_inventory",
                OfflineGeoworldUnityPreviewRunnerVocabulary.UnityScriptInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal101SourceRoot,
                Goal101SourceGoalId,
                "goal101.simulated_command",
                OfflineGeoworldUnityPreviewRunnerVocabulary.SimulatedCommandProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal101SourceRoot,
                Goal101SourceGoalId,
                "goal101.negative",
                OfflineGeoworldUnityPreviewRunnerVocabulary.NegativeProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal101SourceRoot,
                Goal101SourceGoalId,
                "goal101.alpha_runtime_bootstrap_unchanged",
                OfflineGeoworldUnityPreviewRunnerVocabulary.QualityGateScanFileName,
                "alphaRuntimeBootstrapUnchanged",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal101SourceRoot,
                Goal101SourceGoalId,
                "goal101.all_command_kinds_mapped",
                OfflineGeoworldUnityPreviewRunnerVocabulary.QualityGateScanFileName,
                "allCommandKindsMapped",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal101SourceRoot,
                Goal101SourceGoalId,
                "goal101.travel_window_demo",
                OfflineGeoworldUnityPreviewRunnerVocabulary.QualityGateScanFileName,
                "travelWindowDemoBuilt",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal101SourceRoot,
                Goal101SourceGoalId,
                "goal101.quality_gate",
                OfflineGeoworldUnityPreviewRunnerVocabulary.QualityGateScanFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics)
        ];

    private static IReadOnlyList<VisualWorldPreviewProofStatus> BuildGoal102OfflineGeoworldUnityEditorPreviewProofStatus(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics) =>
        [
            BuildProof(
                projectRoot,
                Goal102SourceRoot,
                Goal102SourceGoalId,
                "goal102.tool_inventory",
                OfflineGeoworldUnityEditorPreviewToolVocabulary.ToolInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal102SourceRoot,
                Goal102SourceGoalId,
                "goal102.editor_window_menu",
                OfflineGeoworldUnityEditorPreviewToolVocabulary.ToolInventoryFileName,
                "menuItemMarkerPresent",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal102SourceRoot,
                Goal102SourceGoalId,
                "goal102.simulated_action",
                OfflineGeoworldUnityEditorPreviewToolVocabulary.SimulatedActionProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal102SourceRoot,
                Goal102SourceGoalId,
                "goal102.clear_operation",
                OfflineGeoworldUnityEditorPreviewToolVocabulary.SimulatedActionProofFileName,
                "clearOperationModelPassed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal102SourceRoot,
                Goal102SourceGoalId,
                "goal102.negative",
                OfflineGeoworldUnityEditorPreviewToolVocabulary.NegativeProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal102SourceRoot,
                Goal102SourceGoalId,
                "goal102.alpha_runtime_bootstrap_unchanged",
                OfflineGeoworldUnityEditorPreviewToolVocabulary.QualityGateScanFileName,
                "alphaRuntimeBootstrapUnchanged",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal102SourceRoot,
                Goal102SourceGoalId,
                "goal102.quality_gate",
                OfflineGeoworldUnityEditorPreviewToolVocabulary.QualityGateScanFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics)
        ];

    private static IReadOnlyList<VisualWorldPreviewProofStatus> BuildGoal103OfflineGeoworldPlayModeTravelProofStatus(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics) =>
        [
            BuildProof(
                projectRoot,
                Goal103PlayModeTravelSourceRoot,
                Goal103PlayModeTravelSourceGoalId,
                "goal103.unity_script_inventory",
                OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityScriptInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal103PlayModeTravelSourceRoot,
                Goal103PlayModeTravelSourceGoalId,
                "goal103.editor_window_inventory",
                OfflineGeoworldPlayModeTravelPreviewVocabulary.EditorWindowInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal103PlayModeTravelSourceRoot,
                Goal103PlayModeTravelSourceGoalId,
                "goal103.simulated_execution",
                OfflineGeoworldPlayModeTravelPreviewVocabulary.SimulatedExecutionProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal103PlayModeTravelSourceRoot,
                Goal103PlayModeTravelSourceGoalId,
                "goal103.negative",
                OfflineGeoworldPlayModeTravelPreviewVocabulary.NegativeProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal103PlayModeTravelSourceRoot,
                Goal103PlayModeTravelSourceGoalId,
                "goal103.goal102b_closure",
                OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal102BClosureFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal103PlayModeTravelSourceRoot,
                Goal103PlayModeTravelSourceGoalId,
                "goal103.alpha_runtime_bootstrap_unchanged",
                OfflineGeoworldPlayModeTravelPreviewVocabulary.QualityGateScanFileName,
                "alphaRuntimeBootstrapUnchanged",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal103PlayModeTravelSourceRoot,
                Goal103PlayModeTravelSourceGoalId,
                "goal103.boundary_prefetch",
                OfflineGeoworldPlayModeTravelPreviewVocabulary.QualityGateScanFileName,
                "boundaryPrefetchRepresented",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal103PlayModeTravelSourceRoot,
                Goal103PlayModeTravelSourceGoalId,
                "goal103.quality_gate",
                OfflineGeoworldPlayModeTravelPreviewVocabulary.QualityGateScanFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics)
        ];

    private static IReadOnlyList<VisualWorldPreviewProofStatus> BuildGoal104OfflineGeoworldInteractiveTravelProofStatus(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics) =>
        [
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.unity_script_inventory",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityScriptInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.editor_window_inventory",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.EditorWindowInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.simulated_execution",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.SimulatedExecutionProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.negative",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.NegativeProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.alpha_runtime_bootstrap_unchanged",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName,
                "alphaRuntimeBootstrapUnchanged",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.boundary_crossings",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName,
                "boundaryZonesBuilt",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.prefetch_plan",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName,
                "prefetchPlanBuilt",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.quality_gate",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics)
        ];

}
