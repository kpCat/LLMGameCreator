using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaAcceptanceOperatorPack;

public static class OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary
{
    public const string GoalId = "goal_112_offline_geoworld_alpha_acceptance_operator_pack";
    public const string ProductSmokeRoute =
        "goal-112-offline-geoworld-alpha-acceptance-operator-pack";
    public const string ManualGate = OfflineGeoworldAlphaManualResultIntakeVocabulary.ManualGate;
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-112-offline-geoworld-alpha-acceptance-operator-pack";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-112-offline-geoworld-alpha-acceptance-operator-pack";
    public const string DocumentationRunbookPath =
        "docs/manual-acceptance/offline-geoworld-alpha-manual-acceptance-operator-pack.md";
    public const string PreferredManualResultPath =
        ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/"
        + OfflineGeoworldAlphaManualResultIntakeVocabulary.ResultFileName;

    public const string OperatorStatusReadyPendingHumanRun =
        "OPERATOR_READY_PENDING_HUMAN_RUN";
    public const string OperatorStatusGoal110Missing =
        "BLOCKED_GOAL110_PACKAGE_MISSING";
    public const string OperatorStatusGoal111DecisionMissing =
        "BLOCKED_GOAL111_DECISION_MISSING";
    public const string OperatorStatusGoal111Invalid =
        "BLOCKED_GOAL111_INVALID";
    public const string OperatorStatusGreenManualResultAvailable =
        "GREEN_MANUAL_RESULT_AVAILABLE_FOR_HUMAN_REVIEW";

    public const string DashboardFileName =
        "offline-geoworld-alpha-acceptance-operator-dashboard.json";
    public const string RunbookFileName =
        "offline-geoworld-alpha-acceptance-operator-runbook.md";
    public const string ResultPathMapFileName =
        "offline-geoworld-alpha-acceptance-result-path-map.json";
    public const string PreflightReportFileName =
        "offline-geoworld-alpha-acceptance-operator-preflight-report.md";
    public const string NotaryBoundaryFileName =
        "offline-geoworld-alpha-acceptance-notary-boundary.json";
    public const string QualityGateScanFileName =
        "offline-geoworld-alpha-acceptance-quality-gate-scan.json";
    public const string NegativeProofFileName =
        "offline-geoworld-alpha-acceptance-negative-proof-no-result-no-acceptance.json";
    public const string PendingResultTemplateCopyFileName =
        "offline-geoworld-alpha-pending-result-template-copy.json";
    public const string FileIndexFileName =
        "offline-geoworld-alpha-acceptance-operator-file-index.json";
    public const string ExportReadmeFileName =
        "offline-geoworld-alpha-acceptance-operator-readme.md";

    public static IReadOnlyList<string> SourceGoalIds =>
    [
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId,
        OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId
    ];

    public static IReadOnlyList<string> CandidateManualResultPaths =>
        OfflineGeoworldAlphaManualResultIntakeVocabulary.DefaultCandidateResultRelativePaths;

    public static IReadOnlyList<string> RequiredProceduralFileNames =>
    [
        DashboardFileName,
        RunbookFileName,
        ResultPathMapFileName,
        PreflightReportFileName,
        NotaryBoundaryFileName,
        QualityGateScanFileName,
        NegativeProofFileName,
        PendingResultTemplateCopyFileName,
        FileIndexFileName
    ];

    public static IReadOnlyList<string> RequiredExportFileNames =>
    [
        DashboardFileName,
        ExportReadmeFileName,
        FileIndexFileName
    ];
}

public sealed record OfflineGeoworldAlphaAcceptanceOperatorPackBuildResult
{
    public OfflineGeoworldAlphaAcceptanceOperatorDashboard Dashboard { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceResultPathMap ResultPathMap { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceNotaryBoundary NotaryBoundary { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceOperatorQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceNegativeProofNoResultNoAcceptance NegativeProof { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceOperatorFileIndex ProceduralFileIndex { get; init; } = new();
    public OfflineGeoworldAlphaAcceptanceOperatorFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationRunbookMarkdown { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaAcceptanceOperatorPackWriteResult
{
    public OfflineGeoworldAlphaAcceptanceOperatorPackBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationRunbookPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaAcceptanceOperatorDashboard
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId;
    public IReadOnlyList<string> SourceGoalIds { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.SourceGoalIds;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ManualGate;
    public string OperatorStatus { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.OperatorStatusReadyPendingHumanRun;
    public string DecisionStatusFromGoal111 { get; init; } =
        OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending;
    public string PreferredManualResultPath { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.PreferredManualResultPath;
    public IReadOnlyList<string> CandidateManualResultPaths { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.CandidateManualResultPaths;
    public string UnityRunnerPath { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityEditorWindowScriptPath;
    public string UnityResultStorePath { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityResultStoreScriptPath;
    public int ChecklistStepCount { get; init; }
    public string ChecklistHash { get; init; } = string.Empty;
    public string ResultTemplateHash { get; init; } = string.Empty;
    public bool ManualResultPresent { get; init; }
    public bool ManualResultAvailableForHumanReview { get; init; }
    public bool AcceptedByCodex { get; init; }
    public bool HumanAcceptanceStillRequired { get; init; } = true;
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public bool NoUnityFileChangesRequired { get; init; } = true;
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
    public IReadOnlyList<string> NextHumanActions { get; init; } = [];
    public IReadOnlyList<string> DoNotDoYet { get; init; } = [];
    public string ProceduralEvidencePath { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory;
    public string ExportEvidencePath { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ExportPackageDirectory;
    public string RunbookPath { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory
        + "/"
        + OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.RunbookFileName;
}

public sealed record OfflineGeoworldAlphaAcceptanceResultPathMap
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ManualGate;
    public string PreferredManualResultPath { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.PreferredManualResultPath;
    public IReadOnlyList<string> CandidateManualResultPaths { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.CandidateManualResultPaths;
    public string UnityProjectPath { get; init; } = "unity/LLMGameCreatorAlpha";
    public string UnityRunnerPath { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityEditorWindowScriptPath;
    public string UnityResultModelPath { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityResultScriptPath;
    public string UnityResultStorePath { get; init; } =
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityResultStoreScriptPath;
    public bool PreferredPathIsManualResultTarget { get; init; } = true;
    public bool TemplateCopyIsNotManualResult { get; init; } = true;
}

public sealed record OfflineGeoworldAlphaAcceptanceNotaryBoundary
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool AcceptedByCodex { get; init; }
    public bool HumanAcceptanceStillRequired { get; init; } = true;
    public bool DoesNotWriteManualResultPath { get; init; } = true;
    public bool PendingTemplateCopyOnly { get; init; } = true;
    public bool NoUnityFileChangesRequired { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public IReadOnlyList<string> ForbiddenStarts { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaAcceptanceOperatorQualityGateScan
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ManualGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public string OperatorStatus { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.OperatorStatusReadyPendingHumanRun;
    public string DecisionStatusFromGoal111 { get; init; } =
        OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending;
    public bool Goal110PackagePresent { get; init; }
    public bool Goal111DecisionPresent { get; init; }
    public bool ChecklistHashResolved { get; init; }
    public bool ResultTemplateHashResolved { get; init; }
    public bool PendingTemplateCopySafe { get; init; }
    public bool NegativeNoResultNoAcceptancePassed { get; init; }
    public bool RunbookBoundaryScanPassed { get; init; }
    public bool AcceptedByCodexFalse { get; init; } = true;
    public bool HumanAcceptanceStillRequired { get; init; } = true;
    public bool NoUnityFileChangesRequired { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public int ChecklistStepCount { get; init; }
    public int ProceduralFileCount { get; init; }
    public int ExportFileCount { get; init; }
    public int SourceHealthScannedFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaAcceptanceNegativeProofNoResultNoAcceptance
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId;
    public string ScenarioId { get; init; } = "missing_manual_result_does_not_accept_alpha";
    public bool Passed { get; init; }
    public bool ManualResultPresent { get; init; }
    public bool AcceptedByCodex { get; init; }
    public bool HumanAcceptanceStillRequired { get; init; } = true;
    public string OperatorStatus { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.OperatorStatusReadyPendingHumanRun;
    public string Diagnostic { get; init; } =
        "No real manual result means no Alpha acceptance by Codex.";
}

public sealed record OfflineGeoworldAlphaAcceptanceOperatorFileIndex
{
    public string GoalId { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool SelfHashExcluded { get; init; } = true;
    public IReadOnlyList<OfflineGeoworldAlphaAcceptanceOperatorFileIndexEntry> Files { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaAcceptanceOperatorFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
