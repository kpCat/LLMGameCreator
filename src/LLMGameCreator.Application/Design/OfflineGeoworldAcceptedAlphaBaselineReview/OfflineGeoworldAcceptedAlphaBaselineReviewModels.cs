using LLMGameCreator.Application.Design.OfflineGeoworldAlphaHumanResultRevalidation;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceExportPackage;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAcceptedAlphaBaselineReview;

public static class OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary
{
    public const string GoalId = "goal_118_offline_geoworld_accepted_alpha_baseline_review";
    public const string ProductSmokeRoute =
        "goal-118-offline-geoworld-accepted-alpha-baseline-review";
    public const string BaselineId = "offline_geoworld_alpha_accepted_baseline_v1";
    public const string SourceGoalRange = "Goal098-Goal117";
    public const string ManualGate =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGate;
    public const string ManualGateStatusAccepted =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusAccepted;
    public const string ExpectedManualResultSha256 =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExpectedManualResultSha256;
    public const string RecommendedNextDecision = "EXPLICIT_NEXT_LANE_SELECTION";

    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-118-offline-geoworld-accepted-alpha-baseline-review";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-118-offline-geoworld-accepted-alpha-baseline-review";
    public const string DocumentationPath =
        "docs/manual-acceptance/offline-geoworld-accepted-alpha-baseline-review.md";

    public const string DashboardFileName =
        "offline-geoworld-accepted-alpha-baseline-dashboard.json";
    public const string ManifestFileName =
        "offline-geoworld-accepted-alpha-baseline-manifest.json";
    public const string SourceIndexFileName =
        "offline-geoworld-accepted-alpha-baseline-source-index.json";
    public const string ReportFileName =
        "offline-geoworld-accepted-alpha-baseline-report.md";
    public const string QualityGateScanFileName =
        "offline-geoworld-accepted-alpha-baseline-quality-gate-scan.json";
    public const string NegativeProofFileName =
        "offline-geoworld-accepted-alpha-baseline-negative-proof.json";
    public const string FileIndexFileName =
        "offline-geoworld-accepted-alpha-baseline-file-index.json";

    public static IReadOnlyList<string> SourceGoalIds =>
    [
        "goal_098_geoworld_source_adapter_streaming_contract",
        "goal_099_offline_geoworld_worldsourcegraph_streaming",
        "goal_100_offline_geoworld_visual_cache_unity_handoff",
        "goal_101_offline_geoworld_unity_preview_runner",
        "goal_102_offline_geoworld_unity_editor_preview_tool",
        "goal_102a_unity_editor_source_format_guard",
        "goal_102b_actual_unity_editor_source_reformat",
        "goal_103_offline_geoworld_playmode_travel_preview",
        "goal_104_offline_geoworld_interactive_travel_preview",
        "goal_105_offline_geoworld_interaction_playable_probe",
        "goal_106_offline_geoworld_session_persistence_replay",
        "goal_107_offline_geoworld_objective_acceptance_run",
        OfflineGeoworldAlphaSliceVocabulary.GoalId,
        "goal_108a_alpha_slice_source_split_immutability_audit",
        OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId,
        "goal_110_offline_geoworld_alpha_manual_acceptance_gate",
        "goal_111_offline_geoworld_alpha_manual_result_intake",
        "goal_112_offline_geoworld_alpha_acceptance_operator_pack",
        "goal_113_offline_geoworld_alpha_manual_result_workbench",
        "goal_114_unity_safe_mode_compile_hotfix",
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId,
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId,
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.GoalId
    ];

    public static IReadOnlyList<string> SourceGoalRoots =>
    [
        ".llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract",
        ".llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming",
        ".llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff",
        ".llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner",
        ".llmgc/procedural/goal-102-offline-geoworld-unity-editor-preview-tool",
        ".llmgc/procedural/goal-102a-unity-editor-source-format-guard",
        ".llmgc/procedural/goal-102b-actual-unity-editor-source-reformat",
        ".llmgc/procedural/goal-103-offline-geoworld-playmode-travel-preview",
        ".llmgc/procedural/goal-104-offline-geoworld-interactive-travel-preview",
        ".llmgc/procedural/goal-105-offline-geoworld-interaction-playable-probe",
        ".llmgc/procedural/goal-106-offline-geoworld-session-persistence-replay",
        ".llmgc/procedural/goal-107-offline-geoworld-objective-acceptance-run",
        OfflineGeoworldAlphaSliceVocabulary.RelativeOutputDirectory,
        ".llmgc/procedural/goal-108a-alpha-slice-source-split-immutability-audit",
        OfflineGeoworldAlphaSliceExportPackageVocabulary.ProceduralOutputDirectory,
        ".llmgc/procedural/goal-110-offline-geoworld-alpha-manual-acceptance-gate",
        ".llmgc/procedural/goal-111-offline-geoworld-alpha-manual-result-intake",
        ".llmgc/procedural/goal-112-offline-geoworld-alpha-acceptance-operator-pack",
        ".llmgc/procedural/goal-113-offline-geoworld-alpha-manual-result-workbench",
        ".llmgc/procedural/goal-114-unity-safe-mode-compile-hotfix",
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ProceduralOutputDirectory,
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory,
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.ProceduralOutputDirectory
    ];

    public static IReadOnlyList<string> RequiredProceduralFileNames =>
    [
        DashboardFileName,
        ManifestFileName,
        SourceIndexFileName,
        ReportFileName,
        QualityGateScanFileName,
        NegativeProofFileName,
        FileIndexFileName
    ];

    public static IReadOnlyList<string> RequiredExportFileNames => RequiredProceduralFileNames;
}

public sealed record OfflineGeoworldAcceptedAlphaBaselineReviewBuildResult
{
    public OfflineGeoworldAcceptedAlphaBaselineDashboard Dashboard { get; init; } = new();
    public OfflineGeoworldAcceptedAlphaBaselineManifest Manifest { get; init; } = new();
    public OfflineGeoworldAcceptedAlphaBaselineSourceIndex SourceIndex { get; init; } = new();
    public OfflineGeoworldAcceptedAlphaBaselineQualityGateScan QualityGateScan { get; init; } =
        new();
    public OfflineGeoworldAcceptedAlphaBaselineNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldAcceptedAlphaBaselineFileIndex ProceduralFileIndex { get; init; } =
        new();
    public OfflineGeoworldAcceptedAlphaBaselineFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAcceptedAlphaBaselineReviewWriteResult
{
    public OfflineGeoworldAcceptedAlphaBaselineReviewBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record OfflineGeoworldAcceptedAlphaBaselineDashboard
{
    public string GoalId { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.GoalId;
    public string BaselineId { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.BaselineId;
    public string BaselineHash { get; init; } = string.Empty;
    public string ManualGate { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ManualGate;
    public string ManualGateStatus { get; init; } = string.Empty;
    public bool AcceptedByCodex { get; init; }
    public bool AcceptedBaselineReady { get; init; }
    public string ManualResultSha256 { get; init; } = string.Empty;
    public string SourceGoalRange { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceGoalRange;
    public int IncludedSourceGoalCount { get; init; }
    public IReadOnlyList<string> AcceptedEvidenceRoots { get; init; } = [];
    public IReadOnlyList<string> ProducedOnlyHistoricalRoots { get; init; } = [];
    public IReadOnlyList<string> BlockedOrSupersededNotes { get; init; } = [];
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public bool NoUnityFileChangesRequired { get; init; } = true;
    public string RecommendedNextDecision { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.RecommendedNextDecision;
    public bool DoNotStartAutomatically { get; init; } = true;
    public string EvidencePath { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ExportPackageDirectory;
    public int AcceptedEvidenceRootCount { get; init; }
    public int ProducedOnlyRootCount { get; init; }
    public int BlockedOrSupersededNoteCount { get; init; }
    public bool Goal116AcceptanceRecordPresent { get; init; }
    public bool Goal116AcceptanceRecordValid { get; init; }
    public bool Goal117ContinuationSelectionPresent { get; init; }
    public bool Goal117ContinuationSelectionValid { get; init; }
    public bool Goal114UnitySafeModeCompileHotfixEvidencePresent { get; init; }
    public bool Goal109PortableExportEvidencePresent { get; init; }
    public bool Goal108AlphaSliceOrchestratorEvidencePresent { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed record OfflineGeoworldAcceptedAlphaBaselineManifest
{
    public string GoalId { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.GoalId;
    public string BaselineId { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.BaselineId;
    public string BaselineHash { get; init; } = string.Empty;
    public bool AcceptedBaselineReady { get; init; }
    public string ManualGateStatus { get; init; } = string.Empty;
    public string ManualResultSha256 { get; init; } = string.Empty;
    public string SourceGoalRange { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceGoalRange;
    public int IncludedSourceGoalCount { get; init; }
    public int AcceptedEvidenceRootCount { get; init; }
    public int ProducedOnlyRootCount { get; init; }
    public string RecommendedNextDecision { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.RecommendedNextDecision;
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public bool NoUnityFileChangesRequired { get; init; } = true;
    public string SourceIndexSha256 { get; init; } = string.Empty;
    public string DashboardSha256 { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAcceptedAlphaBaselineSourceIndex
{
    public string GoalId { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.GoalId;
    public string SourceGoalRange { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceGoalRange;
    public int IncludedSourceGoalCount { get; init; }
    public bool Goal098To117ChainIncluded { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<OfflineGeoworldAcceptedAlphaBaselineSourceIndexEntry> Entries { get; init; } =
        [];
}

public sealed record OfflineGeoworldAcceptedAlphaBaselineSourceIndexEntry
{
    public string SourceGoalId { get; init; } = string.Empty;
    public string RelativeRoot { get; init; } = string.Empty;
    public string Classification { get; init; } = string.Empty;
    public bool Present { get; init; }
    public bool RequiredForAcceptedBaseline { get; init; }
}

public sealed record OfflineGeoworldAcceptedAlphaBaselineQualityGateScan
{
    public string GoalId { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.GoalId;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public string BaselineId { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.BaselineId;
    public bool AcceptedBaselineReady { get; init; }
    public string ManualGateStatus { get; init; } = string.Empty;
    public bool ManualResultHashMatches { get; init; }
    public bool AcceptedByCodexFalse { get; init; }
    public bool Goal116AcceptedEvidenceValid { get; init; }
    public bool Goal117ContinuationEvidenceValid { get; init; }
    public bool Goal117ReadyCandidateBlockedCountsValid { get; init; }
    public bool Goal114UnitySafeModeEvidenceExists { get; init; }
    public bool Goal109PortableExportEvidenceExists { get; init; }
    public bool Goal108AlphaSliceEvidenceExists { get; init; }
    public bool SourceGoalRangeIncluded { get; init; }
    public bool ManualInputExcluded { get; init; }
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public bool NoUnityFileChangesRequired { get; init; } = true;
    public bool NegativeProofPassed { get; init; }
    public int IncludedSourceGoalCount { get; init; }
    public int AcceptedEvidenceRootCount { get; init; }
    public int ProducedOnlyRootCount { get; init; }
    public int BlockedOrSupersededNoteCount { get; init; }
    public int ProceduralFileCount { get; init; }
    public int ExportFileCount { get; init; }
    public int SourceHealthScannedFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldAcceptedAlphaBaselineNegativeProof
{
    public string GoalId { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool MissingGoal116AcceptedEvidenceRejected { get; init; }
    public bool MissingGoal117PostAcceptanceRoutingRejected { get; init; }
    public bool ManualInputStagedOrEmbeddedRejected { get; init; }
    public bool LiveGeodataProviderNetworkStartRejected { get; init; }
    public bool RuntimeSchemaLuaGeneratorLibraryChangesRejected { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected { get; init; }
    public bool FinalReleasePackagingRejected { get; init; }
    public bool ManualInputExcluded { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAcceptedAlphaBaselineFileIndex
{
    public string GoalId { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.GoalId;
    public string BaselineId { get; init; } =
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.BaselineId;
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool SelfHashExcluded { get; init; } = true;
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<OfflineGeoworldAcceptedAlphaBaselineFileIndexEntry> Files { get; init; } =
        [];
}

public sealed record OfflineGeoworldAcceptedAlphaBaselineFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
