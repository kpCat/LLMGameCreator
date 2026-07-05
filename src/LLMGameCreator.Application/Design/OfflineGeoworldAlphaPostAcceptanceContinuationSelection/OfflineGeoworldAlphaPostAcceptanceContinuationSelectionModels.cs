using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;

public static class OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
{
    public const string GoalId =
        "goal_117_offline_geoworld_alpha_post_acceptance_continuation_selection";
    public const string ProductSmokeRoute =
        "goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection";
    public const string ManualGate =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGate;
    public const string SourceDecisionStatusGreenCandidate =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.SourceDecisionStatusGreenCandidate;
    public const string ManualGateStatusAccepted =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusAccepted;
    public const string ExpectedManualResultSha256 =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExpectedManualResultSha256;

    public const string LaneAcceptedAlphaBaselineReview = "accepted_alpha_baseline_review";
    public const string RecommendedNextGoalId =
        "goal-118-offline-geoworld-accepted-alpha-baseline-review";

    public const string StatusReady = "READY";
    public const string StatusCandidateRequiresExplicitApproval =
        "CANDIDATE_REQUIRES_EXPLICIT_APPROVAL";
    public const string StatusBlockedRequiresExplicitSchemaRuntimeTask =
        "BLOCKED_REQUIRES_EXPLICIT_SCHEMA_RUNTIME_TASK";
    public const string StatusBlockedByPolicy = "BLOCKED_BY_POLICY";
    public const string StatusBlockedNotReleaseReady = "BLOCKED_NOT_RELEASE_READY";
    public const string StatusCandidateRequiresRendererDecision =
        "CANDIDATE_REQUIRES_RENDERER_DECISION";

    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection";
    public const string DocumentationPath =
        "docs/manual-acceptance/offline-geoworld-alpha-post-acceptance-continuation-selection.md";

    public const string DashboardFileName =
        "offline-geoworld-alpha-post-acceptance-continuation-dashboard.json";
    public const string MatrixFileName =
        "offline-geoworld-alpha-post-acceptance-continuation-matrix.json";
    public const string ReportFileName =
        "offline-geoworld-alpha-post-acceptance-continuation-report.md";
    public const string QualityGateScanFileName =
        "offline-geoworld-alpha-post-acceptance-continuation-quality-gate-scan.json";
    public const string NegativeProofFileName =
        "offline-geoworld-alpha-post-acceptance-continuation-negative-proof.json";
    public const string FileIndexFileName =
        "offline-geoworld-alpha-post-acceptance-continuation-file-index.json";
    public const string ExportReadmeFileName =
        "offline-geoworld-alpha-post-acceptance-continuation-readme.md";

    public static IReadOnlyList<string> SourceGoalIds =>
    [
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId
    ];

    public static IReadOnlyList<string> RequiredProceduralFileNames =>
    [
        DashboardFileName,
        MatrixFileName,
        ReportFileName,
        FileIndexFileName,
        QualityGateScanFileName,
        NegativeProofFileName
    ];

    public static IReadOnlyList<string> RequiredExportFileNames =>
    [
        DashboardFileName,
        MatrixFileName,
        ReportFileName,
        QualityGateScanFileName,
        NegativeProofFileName,
        ExportReadmeFileName,
        FileIndexFileName
    ];
}

public sealed record OfflineGeoworldAlphaPostAcceptanceContinuationSelectionBuildResult
{
    public OfflineGeoworldAlphaPostAcceptanceContinuationDashboard Dashboard { get; init; } =
        new();
    public OfflineGeoworldAlphaPostAcceptanceContinuationMatrix Matrix { get; init; } = new();
    public OfflineGeoworldAlphaPostAcceptanceContinuationQualityGateScan QualityGateScan { get; init; } =
        new();
    public OfflineGeoworldAlphaPostAcceptanceContinuationNegativeProof NegativeProof { get; init; } =
        new();
    public OfflineGeoworldAlphaPostAcceptanceContinuationFileIndex ProceduralFileIndex { get; init; } =
        new();
    public OfflineGeoworldAlphaPostAcceptanceContinuationFileIndex ExportFileIndex { get; init; } =
        new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaPostAcceptanceContinuationSelectionWriteResult
{
    public OfflineGeoworldAlphaPostAcceptanceContinuationSelectionBuildResult Result { get; init; } =
        new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaPostAcceptanceContinuationDashboard
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.GoalId;
    public IReadOnlyList<string> SourceGoalIds { get; init; } =
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.SourceGoalIds;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.ManualGate;
    public string ManualGateStatus { get; init; } = string.Empty;
    public bool HumanAccepted { get; init; }
    public string SourceDecisionStatus { get; init; } = string.Empty;
    public string ManualResultSha256 { get; init; } = string.Empty;
    public bool AcceptedByCodex { get; init; }
    public bool ManualInputNotCommitted { get; init; }
    public bool RawManualResultEmbeddedInArtifacts { get; init; }
    public string RecommendedNextLane { get; init; } =
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
            .LaneAcceptedAlphaBaselineReview;
    public string RecommendedNextGoalId { get; init; } =
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.RecommendedNextGoalId;
    public int ReadyLaneCount { get; init; }
    public int CandidateLaneCount { get; init; }
    public int BlockedLaneCount { get; init; }
    public bool DoNotStartAutomatically { get; init; } = true;
    public string EvidencePath { get; init; } =
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
            .ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
            .ExportPackageDirectory;
    public bool Goal116AcceptanceRecordPresent { get; init; }
    public bool Goal116AcceptanceRecordValid { get; init; }
    public bool Goal115DecisionSnapshotPresent { get; init; }
    public bool Goal115DecisionSnapshotGreen { get; init; }
    public IReadOnlyList<string> LaneIds { get; init; } = [];
    public IReadOnlyList<string> EvidenceArtifactPaths { get; init; } = [];
    public IReadOnlyList<string> ExportArtifactPaths { get; init; } = [];
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaPostAcceptanceContinuationMatrix
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.GoalId;
    public string ManualGateStatus { get; init; } = string.Empty;
    public bool HumanAccepted { get; init; }
    public string RecommendedNextLane { get; init; } =
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
            .LaneAcceptedAlphaBaselineReview;
    public string RecommendedNextGoalId { get; init; } =
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.RecommendedNextGoalId;
    public bool DoNotStartAutomatically { get; init; } = true;
    public int LaneCount { get; init; }
    public IReadOnlyList<OfflineGeoworldAlphaPostAcceptanceContinuationLane> Lanes { get; init; } =
        [];
}

public sealed record OfflineGeoworldAlphaPostAcceptanceContinuationLane
{
    public string LaneId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string RecommendedNextGoalId { get; init; } = string.Empty;
    public bool IsRecommended { get; init; }
    public bool RequiresExplicitFutureApproval { get; init; }
    public bool DoNotStartAutomatically { get; init; } = true;
    public IReadOnlyList<string> Boundaries { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaPostAcceptanceContinuationQualityGateScan
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.GoalId;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool Goal116AcceptanceRecordPresent { get; init; }
    public bool Goal116AcceptanceRecordValid { get; init; }
    public string ManualGateStatus { get; init; } = string.Empty;
    public bool HumanAccepted { get; init; }
    public string SourceDecisionStatus { get; init; } = string.Empty;
    public bool ManualResultHashMatches { get; init; }
    public bool AcceptedByCodexFalse { get; init; }
    public bool ManualInputNotCommitted { get; init; }
    public bool RawManualResultNotEmbedded { get; init; }
    public bool AllRequiredLanesPresent { get; init; }
    public bool RecommendedLaneSelected { get; init; }
    public bool RecommendedNextGoalSelected { get; init; }
    public bool DoNotStartAutomatically { get; init; } = true;
    public bool NoGoal118TaskFilesCreated { get; init; }
    public int RequiredLaneCount { get; init; }
    public int ReadyLaneCount { get; init; }
    public int CandidateLaneCount { get; init; }
    public int BlockedLaneCount { get; init; }
    public bool RuntimeSchemaLuaGeneratorLibraryBlocked { get; init; }
    public bool LiveGeodataProviderNetworkBlocked { get; init; }
    public bool UnityScenePrefabSettingsReleaseBlocked { get; init; }
    public bool FinalRendererAtlasRequiresFutureDecision { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool ManualInputExcluded { get; init; }
    public int ProceduralFileCount { get; init; }
    public int ExportFileCount { get; init; }
    public int SourceHealthScannedFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaPostAcceptanceContinuationNegativeProof
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool MissingGoal116AcceptanceRejected { get; init; }
    public bool NonAcceptedGoal116Rejected { get; init; }
    public bool CodexAcceptanceRejected { get; init; }
    public bool RawManualResultEmbeddingRejected { get; init; }
    public bool ManualInputStagedOrCommittedRejected { get; init; }
    public bool AutomaticGoal118StartRejected { get; init; }
    public bool ForbiddenRuntimeProviderSchemaLuaGeneratorUnityChangesRejected { get; init; }
    public bool LiveGeodataProviderNetworkBlocked { get; init; }
    public bool ReleasePackagingBlocked { get; init; }
    public bool Goal118TaskFilesNotCreated { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaPostAcceptanceContinuationFileIndex
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool SelfHashExcluded { get; init; } = true;
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<OfflineGeoworldAlphaPostAcceptanceContinuationFileIndexEntry> Files { get; init; } =
        [];
}

public sealed record OfflineGeoworldAlphaPostAcceptanceContinuationFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
