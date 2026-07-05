using LLMGameCreator.Application.Design.OfflineGeoworldAlphaHumanResultRevalidation;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;

public static class OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
{
    public const string GoalId = "goal_116_offline_geoworld_alpha_manual_gate_acceptance_record";
    public const string ProductSmokeRoute =
        "goal-116-offline-geoworld-alpha-manual-gate-acceptance-record";
    public const string ManualGate = OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ManualGate;
    public const string ManualGateStatusAccepted = "ACCEPTED_BY_HUMAN";
    public const string ManualGateStatusBlocked = "BLOCKED_SOURCE_EVIDENCE_INVALID";
    public const string SourceDecisionStatusGreenCandidate =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusGreenCandidate;
    public const string RecommendedNextDecision = "POST_ACCEPTANCE_CONTINUATION_SELECTION";
    public const string HumanDecisionStatement =
        "Я принимаю offline_geoworld_alpha_manual_acceptance_verification по Goal115 GREEN_ACCEPTABLE_CANDIDATE.";
    public const string ExpectedManualResultSha256 =
        "8c2ad299d241d4315248b642b723ae8cf33ecabaa42a46462985ea5dc8335aeb";

    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record";
    public const string DocumentationPath =
        "docs/manual-acceptance/offline-geoworld-alpha-manual-gate-acceptance-record.md";
    public const string ManualResultRelativePath =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ManualResultRelativePath;
    public const string SourceDecisionSnapshotRelativePath =
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ProceduralOutputDirectory
        + "/"
        + OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionSnapshotFileName;

    public const string AcceptanceRecordFileName =
        "offline-geoworld-alpha-manual-gate-acceptance-record.json";
    public const string DashboardFileName =
        "offline-geoworld-alpha-manual-gate-acceptance-dashboard.json";
    public const string ReportFileName =
        "offline-geoworld-alpha-manual-gate-acceptance-report.md";
    public const string QualityGateScanFileName =
        "offline-geoworld-alpha-manual-gate-acceptance-quality-gate-scan.json";
    public const string NegativeProofFileName =
        "offline-geoworld-alpha-manual-gate-acceptance-negative-proof.json";
    public const string FileIndexFileName =
        "offline-geoworld-alpha-manual-gate-acceptance-file-index.json";
    public const string ExportReadmeFileName =
        "offline-geoworld-alpha-manual-gate-acceptance-readme.md";

    public static IReadOnlyList<string> SourceGoalIds =>
    [
        OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId
    ];

    public static IReadOnlyList<string> RequiredProceduralFileNames =>
    [
        AcceptanceRecordFileName,
        DashboardFileName,
        ReportFileName,
        FileIndexFileName,
        QualityGateScanFileName,
        NegativeProofFileName
    ];

    public static IReadOnlyList<string> RequiredExportFileNames =>
    [
        AcceptanceRecordFileName,
        DashboardFileName,
        ExportReadmeFileName,
        FileIndexFileName
    ];
}

public sealed record OfflineGeoworldAlphaManualGateAcceptanceRecordBuildResult
{
    public OfflineGeoworldAlphaManualGateAcceptanceRecord AcceptanceRecord { get; init; } = new();
    public OfflineGeoworldAlphaManualGateAcceptanceDashboard Dashboard { get; init; } = new();
    public OfflineGeoworldAlphaManualGateAcceptanceQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldAlphaManualGateAcceptanceNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldAlphaManualGateAcceptanceFileIndex ProceduralFileIndex { get; init; } = new();
    public OfflineGeoworldAlphaManualGateAcceptanceFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaManualGateAcceptanceWriteResult
{
    public OfflineGeoworldAlphaManualGateAcceptanceRecordBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualGateAcceptanceRecord
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId;
    public IReadOnlyList<string> SourceGoalIds { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.SourceGoalIds;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGate;
    public string ManualGateStatus { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusBlocked;
    public bool HumanAccepted { get; init; }
    public string HumanDecisionStatement { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.HumanDecisionStatement;
    public string SourceDecisionStatus { get; init; } = string.Empty;
    public string SourceDecisionSnapshotRelativePath { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.SourceDecisionSnapshotRelativePath;
    public string ManualResultRelativePath { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualResultRelativePath;
    public string ManualResultSha256 { get; init; } = string.Empty;
    public bool ManualInputNotCommitted { get; init; } = true;
    public bool RawManualResultEmbeddedInArtifacts { get; init; }
    public bool AcceptedByCodex { get; init; }
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public bool NoUnityFileChangesRequired { get; init; } = true;
    public string RecommendedNextDecision { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.RecommendedNextDecision;
    public bool Goal115SnapshotPresent { get; init; }
    public bool Goal115SnapshotValid { get; init; }
    public bool ManualResultPresent { get; init; }
    public bool ManualResultHashMatchesGoal115 { get; init; }
    public bool Goal115ErrorsEmpty { get; init; }
    public bool Goal115WarningsEmpty { get; init; }
    public int RequiredStepCount { get; init; }
    public int PassedStepCount { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualGateAcceptanceDashboard
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGate;
    public string ManualGateStatus { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusBlocked;
    public bool HumanAccepted { get; init; }
    public string SourceDecisionStatus { get; init; } = string.Empty;
    public string ManualResultSha256 { get; init; } = string.Empty;
    public bool AcceptedByCodex { get; init; }
    public bool ManualInputNotCommitted { get; init; } = true;
    public bool RawManualResultEmbeddedInArtifacts { get; init; }
    public string RecommendedNextDecision { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.RecommendedNextDecision;
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public bool NoUnityFileChangesRequired { get; init; } = true;
    public int RequiredStepCount { get; init; }
    public int PassedStepCount { get; init; }
    public IReadOnlyList<string> EvidenceArtifactPaths { get; init; } = [];
    public IReadOnlyList<string> ExportArtifactPaths { get; init; } = [];
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualGateAcceptanceQualityGateScan
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public string ManualGateStatus { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusBlocked;
    public bool HumanAccepted { get; init; }
    public bool HumanDecisionStatementRecorded { get; init; }
    public bool Goal115GreenAcceptableCandidate { get; init; }
    public bool ManualResultHashMatches { get; init; }
    public bool AcceptedByCodexFalse { get; init; } = true;
    public bool ManualInputNotCommitted { get; init; } = true;
    public bool RawManualResultNotEmbedded { get; init; } = true;
    public bool NegativeProofPassed { get; init; }
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public bool NoUnityFileChangesRequired { get; init; } = true;
    public int RequiredStepCount { get; init; }
    public int PassedStepCount { get; init; }
    public int ProceduralFileCount { get; init; }
    public int ExportFileCount { get; init; }
    public int SourceHealthScannedFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualGateAcceptanceNegativeProof
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool MissingGoal115SnapshotRejected { get; init; }
    public bool NonGreenGoal115DecisionRejected { get; init; }
    public bool ManualHashMismatchRejected { get; init; }
    public bool RawManualResultEmbeddingRejected { get; init; }
    public bool ManualInputStagedOrCommittedRejected { get; init; }
    public bool ForbiddenRuntimeProviderSchemaLuaGeneratorUnityChangesRejected { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaManualGateAcceptanceFileIndex
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool SelfHashExcluded { get; init; } = true;
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<OfflineGeoworldAlphaManualGateAcceptanceFileIndexEntry> Files { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualGateAcceptanceFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
