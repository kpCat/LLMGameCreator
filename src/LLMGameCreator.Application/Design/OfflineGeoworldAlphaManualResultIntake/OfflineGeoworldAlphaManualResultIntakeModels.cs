using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;

public static class OfflineGeoworldAlphaManualResultIntakeVocabulary
{
    public const string GoalId = "goal_111_offline_geoworld_alpha_manual_result_intake";
    public const string SourceGoalId =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
    public const string ProductSmokeRoute =
        "goal-111-offline-geoworld-alpha-manual-result-intake";
    public const string ManualGate =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.FinalGate;
    public const string ResultSchema = "offline_geoworld_alpha_acceptance_result_v1";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-111-offline-geoworld-alpha-manual-result-intake";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-111-offline-geoworld-alpha-manual-result-intake";
    public const string ResultFileName = "offline-geoworld-alpha-acceptance-result.json";

    public const string DecisionStatusPending = "BLOCKED_PENDING_MANUAL_RESULT";
    public const string DecisionStatusInvalid = "FAILED_INVALID_RESULT";
    public const string DecisionStatusIncomplete = "BLOCKED_INCOMPLETE_RESULT";
    public const string DecisionStatusGreenCandidate = "GREEN_ACCEPTABLE_CANDIDATE";
    public const string DecisionStatusAcceptedFalse = "BLOCKED_ACCEPTED_FALSE";

    public const string DecisionFileName =
        "offline-geoworld-alpha-manual-result-intake-decision.json";
    public const string ReportFileName =
        "offline-geoworld-alpha-manual-result-intake-report.md";
    public const string FileIndexFileName =
        "offline-geoworld-alpha-manual-result-intake-file-index.json";
    public const string QualityGateScanFileName =
        "offline-geoworld-alpha-manual-result-intake-quality-gate-scan.json";
    public const string MissingResultProofFileName =
        "offline-geoworld-alpha-manual-result-intake-negative-proof-missing-result.json";
    public const string InvalidResultProofFileName =
        "offline-geoworld-alpha-manual-result-intake-negative-proof-invalid-result.json";
    public const string ValidSampleResultFileName =
        "offline-geoworld-alpha-manual-result-intake-valid-sample-result.json";
    public const string ExportDashboardFileName =
        "offline-geoworld-alpha-manual-result-intake-dashboard.json";
    public const string ExportReadmeFileName =
        "offline-geoworld-alpha-manual-result-intake-readme.md";

    public static IReadOnlyList<string> DefaultCandidateResultRelativePaths =>
    [
        ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/"
        + ResultFileName,
        ProceduralOutputDirectory + "/input/" + ResultFileName,
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/"
        + "OfflineGeoworldGoal110/"
        + ResultFileName
    ];

    public static IReadOnlyList<string> AcceptedGoalIdAliases =>
    [
        SourceGoalId,
        "goal_110_offline_geoworld_alpha_manual_acceptance_verification"
    ];

    public static IReadOnlyList<string> SupportedStatuses =>
    [
        "passed",
        "failed",
        "pending",
        "skipped"
    ];

    public static IReadOnlyList<string> RequiredProceduralFileNames =>
    [
        DecisionFileName,
        ReportFileName,
        FileIndexFileName,
        QualityGateScanFileName,
        MissingResultProofFileName,
        InvalidResultProofFileName,
        ValidSampleResultFileName
    ];

    public static IReadOnlyList<string> RequiredExportFileNames =>
    [
        ExportDashboardFileName,
        ExportReadmeFileName,
        FileIndexFileName
    ];
}

public sealed record OfflineGeoworldAlphaManualResultIntakeBuildResult
{
    public OfflineGeoworldAlphaManualResultDecision Decision { get; init; } = new();
    public OfflineGeoworldAlphaManualResultIntakeQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldAlphaManualResultIntakeReport Report { get; init; } = new();
    public OfflineGeoworldAlphaManualResultIntakeFileIndex ProceduralFileIndex { get; init; } = new();
    public OfflineGeoworldAlphaManualResultIntakeFileIndex ExportFileIndex { get; init; } = new();
    public OfflineGeoworldAlphaManualResultIntakeNegativeProof MissingResultProof { get; init; } = new();
    public OfflineGeoworldAlphaManualResultIntakeNegativeProof InvalidResultProof { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldAlphaManualResultIntakeWriteResult
{
    public OfflineGeoworldAlphaManualResultIntakeBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualResultDecision
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId;
    public string SourceGoalId { get; init; } =
        OfflineGeoworldAlphaManualResultIntakeVocabulary.SourceGoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaManualResultIntakeVocabulary.ManualGate;
    public string DecisionStatus { get; init; } =
        OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending;
    public bool AcceptedByCodex { get; init; }
    public bool HumanAcceptanceStillRequired { get; init; } = true;
    public bool AcceptableCandidate { get; init; }
    public bool ResultFilePresent { get; init; }
    public string ResultFilePath { get; init; } = string.Empty;
    public IReadOnlyList<string> CandidateResultPaths { get; init; } =
        OfflineGeoworldAlphaManualResultIntakeVocabulary.DefaultCandidateResultRelativePaths;
    public string ChecklistHashExpected { get; init; } = string.Empty;
    public string ChecklistHashActual { get; init; } = string.Empty;
    public OfflineGeoworldAlphaManualResultStepSummary StepSummary { get; init; } = new();
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
    public OfflineGeoworldAlphaManualResultInputPackageLineage InputPackageLineage { get; init; } = new();
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public string DecisionSummary { get; init; } =
        "valid manual result available for human gate decision: false";
}

public sealed record OfflineGeoworldAlphaManualResultStepSummary
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
    public IReadOnlyList<string> MissingStepIds { get; init; } = [];
    public IReadOnlyList<string> DuplicateStepIds { get; init; } = [];
    public IReadOnlyList<string> UnknownStepIds { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualResultInputPackageLineage
{
    public bool Goal110ExportPackagePresent { get; init; }
    public bool Goal110ProceduralEvidencePresent { get; init; }
    public bool Goal110StreamingAssetsPresent { get; init; }
    public bool ChecklistRead { get; init; }
    public bool ResultTemplateRead { get; init; }
    public bool DashboardRead { get; init; }
    public bool ChecksumsRead { get; init; }
    public bool FileIndexRead { get; init; }
    public bool ManifestRead { get; init; }
    public bool Goal110AcceptedFalse { get; init; }
    public bool Goal110ManualAcceptancePending { get; init; }
    public bool Goal110AutomatedGatePassed { get; init; }
    public int ChecklistStepCount { get; init; }
    public int LoadedMetadataFileCount { get; init; }
    public IReadOnlyDictionary<string, string> Sha256ByRelativePath { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldAlphaManualResultIntakeQualityGateScan
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId;
    public string SourceGoalId { get; init; } =
        OfflineGeoworldAlphaManualResultIntakeVocabulary.SourceGoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaManualResultIntakeVocabulary.ManualGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public string DecisionStatus { get; init; } =
        OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending;
    public bool Goal110PackagePresent { get; init; }
    public bool ChecklistHashResolved { get; init; }
    public bool MissingResultProofPassed { get; init; }
    public bool InvalidResultProofPassed { get; init; }
    public bool AcceptedByCodexFalse { get; init; } = true;
    public bool HumanAcceptanceStillRequired { get; init; } = true;
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public int RequiredStepCount { get; init; }
    public int ProceduralFileCount { get; init; }
    public int ExportFileCount { get; init; }
    public int SourceHealthScannedFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualResultIntakeReport
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId;
    public string SourceGoalId { get; init; } =
        OfflineGeoworldAlphaManualResultIntakeVocabulary.SourceGoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaManualResultIntakeVocabulary.ManualGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public string DecisionStatus { get; init; } =
        OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending;
    public bool AcceptableCandidate { get; init; }
    public bool AcceptedByCodex { get; init; }
    public bool HumanAcceptanceStillRequired { get; init; } = true;
    public string ResultFilePath { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaManualResultIntakeFileIndex
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool SelfHashExcluded { get; init; } = true;
    public IReadOnlyList<OfflineGeoworldAlphaManualResultIntakeFileIndexEntry> Files { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualResultIntakeFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaManualResultIntakeNegativeProof
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId;
    public string ScenarioId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string DecisionStatus { get; init; } = string.Empty;
    public string Diagnostic { get; init; } = string.Empty;
}
