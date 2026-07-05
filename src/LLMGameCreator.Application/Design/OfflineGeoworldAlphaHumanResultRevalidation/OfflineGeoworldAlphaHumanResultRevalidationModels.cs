using LLMGameCreator.Application.Design.OfflineGeoworldAlphaAcceptanceOperatorPack;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultWorkbench;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaHumanResultRevalidation;

public static class OfflineGeoworldAlphaHumanResultRevalidationVocabulary
{
    public const string GoalId = "goal_115_offline_geoworld_alpha_human_result_revalidation";
    public const string ProductSmokeRoute =
        "goal-115-offline-geoworld-alpha-human-result-revalidation";
    public const string ManualGate = OfflineGeoworldAlphaManualResultIntakeVocabulary.ManualGate;
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-115-offline-geoworld-alpha-human-result-revalidation";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-115-offline-geoworld-alpha-human-result-revalidation";
    public const string DocumentationPath =
        "docs/manual-acceptance/offline-geoworld-alpha-human-result-revalidation.md";
    public const string ManualResultRelativePath =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.PreferredManualResultPath;

    public const string DecisionStatusGreenCandidate = "GREEN_ACCEPTABLE_CANDIDATE";
    public const string DecisionStatusPending = "BLOCKED_PENDING_MANUAL_RESULT";
    public const string DecisionStatusIncomplete = "BLOCKED_INCOMPLETE_RESULT";
    public const string DecisionStatusInvalid = "FAILED_INVALID_RESULT";

    public const string RecommendedHumanDecisionReady =
        "READY_FOR_EXPLICIT_HUMAN_ACCEPTANCE_DECISION";
    public const string RecommendedHumanDecisionDoNotAccept = "DO_NOT_ACCEPT_YET";

    public const string DashboardFileName =
        "offline-geoworld-alpha-human-result-revalidation-dashboard.json";
    public const string DecisionSnapshotFileName =
        "offline-geoworld-alpha-human-result-revalidation-decision-snapshot.json";
    public const string ReportFileName =
        "offline-geoworld-alpha-human-result-revalidation-report.md";
    public const string FileIndexFileName =
        "offline-geoworld-alpha-human-result-revalidation-file-index.json";
    public const string QualityGateScanFileName =
        "offline-geoworld-alpha-human-result-revalidation-quality-gate-scan.json";
    public const string NegativeProofFileName =
        "offline-geoworld-alpha-human-result-revalidation-negative-proof.json";
    public const string ExportReadmeFileName =
        "offline-geoworld-alpha-human-result-revalidation-readme.md";

    public static IReadOnlyList<string> SourceGoalIds =>
    [
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId,
        OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId,
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId,
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.GoalId,
        "goal_114_unity_safe_mode_compile_hotfix"
    ];

    public static IReadOnlyList<string> RequiredProceduralFileNames =>
    [
        DashboardFileName,
        DecisionSnapshotFileName,
        ReportFileName,
        FileIndexFileName,
        QualityGateScanFileName,
        NegativeProofFileName
    ];

    public static IReadOnlyList<string> RequiredExportFileNames =>
    [
        DashboardFileName,
        ExportReadmeFileName,
        FileIndexFileName
    ];
}

public sealed record OfflineGeoworldAlphaHumanResultRevalidationBuildResult
{
    public OfflineGeoworldAlphaHumanResultRevalidationDecisionSnapshot DecisionSnapshot { get; init; } = new();
    public OfflineGeoworldAlphaHumanResultRevalidationDashboard Dashboard { get; init; } = new();
    public OfflineGeoworldAlphaHumanResultRevalidationQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldAlphaHumanResultRevalidationNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldAlphaHumanResultRevalidationFileIndex ProceduralFileIndex { get; init; } = new();
    public OfflineGeoworldAlphaHumanResultRevalidationFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaHumanResultRevalidationWriteResult
{
    public OfflineGeoworldAlphaHumanResultRevalidationBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaHumanResultRevalidationDecisionSnapshot
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId;
    public IReadOnlyList<string> SourceGoalIds { get; init; } =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.SourceGoalIds;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ManualGate;
    public string ManualResultRelativePath { get; init; } =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ManualResultRelativePath;
    public string ManualResultSha256 { get; init; } = string.Empty;
    public bool ManualResultPresent { get; init; }
    public bool ManualResultJsonValid { get; init; }
    public string Goal111DecisionStatus { get; init; } =
        OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending;
    public string DecisionStatus { get; init; } =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusPending;
    public bool AcceptableCandidate { get; init; }
    public bool AcceptedByCodex { get; init; }
    public bool HumanAcceptanceStillRequired { get; init; } = true;
    public bool ManualGateRemainsHumanDecision { get; init; } = true;
    public string RecommendedHumanDecision { get; init; } =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.RecommendedHumanDecisionDoNotAccept;
    public string ChecklistHashExpected { get; init; } = string.Empty;
    public string ChecklistHashActual { get; init; } = string.Empty;
    public string ResultChecklistHash { get; init; } = string.Empty;
    public OfflineGeoworldAlphaHumanResultRevalidationStepSummary StepSummary { get; init; } = new();
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public bool NoUnityFileChangesRequired { get; init; } = true;
    public bool ManualInputNotCommitted { get; init; } = true;
    public bool RawManualResultEmbeddedInArtifacts { get; init; }
}

public sealed record OfflineGeoworldAlphaHumanResultRevalidationStepSummary
{
    public int RequiredStepCount { get; init; }
    public int ResultStepCount { get; init; }
    public int PassedCount { get; init; }
    public int FailedCount { get; init; }
    public int PendingCount { get; init; }
    public int SkippedCount { get; init; }
    public int MissingCount { get; init; }
    public int DuplicateCount { get; init; }
    public int UnknownCount { get; init; }
    public int InvalidStatusCount { get; init; }
    public int MissingStatusCount { get; init; }
    public bool RequiredStepsPresentExactlyOnce { get; init; }
    public bool AllRequiredStepsPassed { get; init; }
    public IReadOnlyList<string> MissingStepIds { get; init; } = [];
    public IReadOnlyList<string> DuplicateStepIds { get; init; } = [];
    public IReadOnlyList<string> UnknownStepIds { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaHumanResultRevalidationDashboard
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ManualGate;
    public string DecisionStatus { get; init; } =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusPending;
    public bool AcceptableCandidate { get; init; }
    public string RecommendedHumanDecision { get; init; } =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.RecommendedHumanDecisionDoNotAccept;
    public bool ManualResultPresent { get; init; }
    public string ManualResultRelativePath { get; init; } =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ManualResultRelativePath;
    public string ManualResultSha256 { get; init; } = string.Empty;
    public bool AcceptedByCodex { get; init; }
    public bool HumanAcceptanceStillRequired { get; init; } = true;
    public bool ManualGateRemainsHumanDecision { get; init; } = true;
    public int RequiredStepCount { get; init; }
    public int PassedStepCount { get; init; }
    public int BlockingStepIssueCount { get; init; }
    public IReadOnlyList<string> EvidenceArtifactPaths { get; init; } = [];
    public IReadOnlyList<string> ExportArtifactPaths { get; init; } = [];
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaHumanResultRevalidationQualityGateScan
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ManualGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public string DecisionStatus { get; init; } =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusPending;
    public bool AcceptableCandidate { get; init; }
    public bool ManualResultPresent { get; init; }
    public bool ManualResultJsonValid { get; init; }
    public bool AcceptedByCodexFalse { get; init; } = true;
    public bool HumanAcceptanceStillRequired { get; init; } = true;
    public bool ManualGateRemainsHumanDecision { get; init; } = true;
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public bool NoUnityFileChangesRequired { get; init; } = true;
    public bool ManualInputNotCommitted { get; init; } = true;
    public bool ManualInputExcludedFromFileIndex { get; init; } = true;
    public bool NegativeProofPassed { get; init; }
    public int RequiredStepCount { get; init; }
    public int PassedStepCount { get; init; }
    public int ProceduralFileCount { get; init; }
    public int ExportFileCount { get; init; }
    public int SourceHealthScannedFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaHumanResultRevalidationNegativeProof
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId;
    public bool Passed { get; init; }
    public string MissingManualResultDecisionStatus { get; init; } = string.Empty;
    public bool MissingManualResultBlocked { get; init; }
    public string MalformedManualResultDecisionStatus { get; init; } =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusInvalid;
    public bool MalformedManualResultRejected { get; init; }
    public string DraftTemplateLikeDecisionStatus { get; init; } = string.Empty;
    public bool DraftTemplateLikeResultBlocked { get; init; }
    public bool ManualResultRawJsonCopied { get; init; }
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaHumanResultRevalidationFileIndex
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool SelfHashExcluded { get; init; } = true;
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<OfflineGeoworldAlphaHumanResultRevalidationFileIndexEntry> Files { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaHumanResultRevalidationFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
