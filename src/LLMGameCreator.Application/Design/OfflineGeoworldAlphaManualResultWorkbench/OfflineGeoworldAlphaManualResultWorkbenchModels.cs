using LLMGameCreator.Application.Design.OfflineGeoworldAlphaAcceptanceOperatorPack;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultWorkbench;

public static class OfflineGeoworldAlphaManualResultWorkbenchVocabulary
{
    public const string GoalId = "goal_113_offline_geoworld_alpha_manual_result_workbench";
    public const string ProductSmokeRoute =
        "goal-113-offline-geoworld-alpha-manual-result-workbench";
    public const string ManualGate = OfflineGeoworldAlphaManualResultIntakeVocabulary.ManualGate;
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-113-offline-geoworld-alpha-manual-result-workbench";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-113-offline-geoworld-alpha-manual-result-workbench";
    public const string DocumentationPath =
        "docs/manual-acceptance/offline-geoworld-alpha-manual-result-workbench.md";
    public const string PreferredManualResultPath =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.PreferredManualResultPath;

    public const string WorkbenchStatusReadyPendingHumanResult =
        "WORKBENCH_READY_PENDING_HUMAN_RESULT";
    public const string WorkbenchStatusResultInvalid = "WORKBENCH_RESULT_INVALID";
    public const string WorkbenchStatusResultReadyForHumanReview =
        "WORKBENCH_RESULT_READY_FOR_HUMAN_REVIEW";
    public const string WorkbenchStatusMissingGoal110 = "WORKBENCH_BLOCKED_MISSING_GOAL110";
    public const string WorkbenchStatusMissingGoal111 = "WORKBENCH_BLOCKED_MISSING_GOAL111";
    public const string WorkbenchStatusMissingGoal112 = "WORKBENCH_BLOCKED_MISSING_GOAL112";

    public const string DashboardFileName =
        "offline-geoworld-alpha-manual-result-workbench-dashboard.json";
    public const string FileIndexFileName =
        "offline-geoworld-alpha-manual-result-workbench-file-index.json";
    public const string ReportFileName =
        "offline-geoworld-alpha-manual-result-workbench-report.md";
    public const string RunbookFileName =
        "offline-geoworld-alpha-manual-result-workbench-runbook.md";
    public const string DraftTemplateFileName =
        "offline-geoworld-alpha-manual-result-workbench-draft-template.json";
    public const string FieldMapFileName =
        "offline-geoworld-alpha-manual-result-workbench-field-map.json";
    public const string QualityGateScanFileName =
        "offline-geoworld-alpha-manual-result-workbench-quality-gate-scan.json";
    public const string NegativeNoResultFileName =
        "offline-geoworld-alpha-manual-result-workbench-negative-proof-no-result-no-acceptance.json";
    public const string NegativeInvalidResultFileName =
        "offline-geoworld-alpha-manual-result-workbench-negative-proof-invalid-result.json";
    public const string ExportReadmeFileName =
        "offline-geoworld-alpha-manual-result-workbench-readme.md";

    public static IReadOnlyList<string> SourceGoalIds =>
    [
        OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId,
        OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId,
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId
    ];

    public static IReadOnlyList<string> SupportedStatuses =>
        OfflineGeoworldAlphaManualResultIntakeVocabulary.SupportedStatuses;

    public static IReadOnlyList<string> RequiredProceduralFileNames =>
    [
        DashboardFileName,
        FileIndexFileName,
        ReportFileName,
        RunbookFileName,
        DraftTemplateFileName,
        FieldMapFileName,
        QualityGateScanFileName,
        NegativeNoResultFileName,
        NegativeInvalidResultFileName
    ];

    public static IReadOnlyList<string> RequiredExportFileNames =>
    [
        DashboardFileName,
        FileIndexFileName,
        ExportReadmeFileName
    ];
}

public sealed record OfflineGeoworldAlphaManualResultWorkbenchBuildResult
{
    public OfflineGeoworldAlphaManualResultWorkbenchDashboard Dashboard { get; init; } = new();
    public OfflineGeoworldAlphaManualResultWorkbenchFieldMap FieldMap { get; init; } = new();
    public OfflineGeoworldAlphaManualResultWorkbenchQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldAlphaManualResultWorkbenchNegativeProof NegativeNoResultProof { get; init; } = new();
    public OfflineGeoworldAlphaManualResultWorkbenchNegativeProof NegativeInvalidResultProof { get; init; } = new();
    public OfflineGeoworldAlphaManualResultWorkbenchFileIndex ProceduralFileIndex { get; init; } = new();
    public OfflineGeoworldAlphaManualResultWorkbenchFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaManualResultWorkbenchWriteResult
{
    public OfflineGeoworldAlphaManualResultWorkbenchBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualResultWorkbenchDashboard
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.GoalId;
    public IReadOnlyList<string> SourceGoalIds { get; init; } =
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.SourceGoalIds;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ManualGate;
    public string WorkbenchStatus { get; init; } =
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusReadyPendingHumanResult;
    public string Goal111DecisionStatus { get; init; } =
        OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending;
    public string Goal112OperatorStatus { get; init; } =
        OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.OperatorStatusReadyPendingHumanRun;
    public bool ManualResultPresent { get; init; }
    public string PreferredManualResultPath { get; init; } =
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.PreferredManualResultPath;
    public IReadOnlyList<string> CandidateManualResultPaths { get; init; } = [];
    public string RealManualResultPath { get; init; } = string.Empty;
    public string DraftTemplatePath { get; init; } =
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory
        + "/"
        + OfflineGeoworldAlphaManualResultWorkbenchVocabulary.DraftTemplateFileName;
    public bool DoesNotWritePreferredManualResultPath { get; init; } = true;
    public bool DraftTemplateOnly { get; init; } = true;
    public bool AcceptedByCodex { get; init; }
    public bool HumanAcceptanceStillRequired { get; init; } = true;
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public bool NoUnityFileChangesRequired { get; init; } = true;
    public string ChecklistHash { get; init; } = string.Empty;
    public int ChecklistStepCount { get; init; }
    public IReadOnlyList<OfflineGeoworldAlphaManualResultWorkbenchStep> RequiredSteps { get; init; } = [];
    public OfflineGeoworldAlphaManualResultWorkbenchValidation Validation { get; init; } = new();
    public OfflineGeoworldAlphaManualResultWorkbenchSourceLineage SourceLineage { get; init; } = new();
    public IReadOnlyList<string> NextHumanActions { get; init; } = [];
    public IReadOnlyList<string> DoNotStartYet { get; init; } = [];
    public IReadOnlyList<string> ProceduralArtifactPaths { get; init; } = [];
    public IReadOnlyList<string> ExportArtifactPaths { get; init; } = [];
    public string RunbookPath { get; init; } =
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory
        + "/"
        + OfflineGeoworldAlphaManualResultWorkbenchVocabulary.RunbookFileName;
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualResultWorkbenchStep
{
    public int Order { get; init; }
    public string StepId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string EvidenceField { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
}

public sealed record OfflineGeoworldAlphaManualResultWorkbenchValidation
{
    public string ValidationStatus { get; init; } =
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusReadyPendingHumanResult;
    public bool AcceptedByCodex { get; init; }
    public bool HumanAcceptanceStillRequired { get; init; } = true;
    public bool ManualResultPresent { get; init; }
    public bool ReadyForHumanReview { get; init; }
    public string ResultFilePath { get; init; } = string.Empty;
    public string ChecklistHashExpected { get; init; } = string.Empty;
    public string ChecklistHashActual { get; init; } = string.Empty;
    public OfflineGeoworldAlphaManualResultWorkbenchStepSummary StepSummary { get; init; } = new();
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualResultWorkbenchStepSummary
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

public sealed record OfflineGeoworldAlphaManualResultWorkbenchSourceLineage
{
    public bool Goal110PackagePresent { get; init; }
    public bool Goal110ChecklistRead { get; init; }
    public bool Goal110ResultTemplateRead { get; init; }
    public bool Goal110DashboardRead { get; init; }
    public bool Goal111DecisionPresent { get; init; }
    public bool Goal111DecisionValid { get; init; }
    public bool Goal112DashboardPresent { get; init; }
    public bool Goal112PathMapPresent { get; init; }
    public bool Goal112RunbookPresent { get; init; }
    public bool Goal112ArtifactsPresent { get; init; }
    public int Goal110ChecklistStepCount { get; init; }
    public string Goal111DecisionStatus { get; init; } = string.Empty;
    public string Goal112OperatorStatus { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Sha256ByRelativePath { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldAlphaManualResultWorkbenchFieldMap
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ManualGate;
    public string DraftTemplatePath { get; init; } =
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory
        + "/"
        + OfflineGeoworldAlphaManualResultWorkbenchVocabulary.DraftTemplateFileName;
    public string PreferredManualResultPath { get; init; } =
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.PreferredManualResultPath;
    public bool DraftTemplateOnly { get; init; } = true;
    public IReadOnlyList<OfflineGeoworldAlphaManualResultWorkbenchFieldMapEntry> Fields { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualResultWorkbenchFieldMapEntry
{
    public string JsonPath { get; init; } = string.Empty;
    public string RequiredHumanAction { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaManualResultWorkbenchQualityGateScan
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ManualGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public string WorkbenchStatus { get; init; } =
        OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusReadyPendingHumanResult;
    public bool ManualResultPresent { get; init; }
    public bool AcceptedByCodexFalse { get; init; } = true;
    public bool HumanAcceptanceStillRequired { get; init; } = true;
    public bool DoesNotWritePreferredManualResultPath { get; init; } = true;
    public bool DraftTemplateOnly { get; init; } = true;
    public bool NotFinalReleaseOrRuntimeBuild { get; init; } = true;
    public bool NoRuntimeProviderOrNetworkChanges { get; init; } = true;
    public bool NoUnityFileChangesRequired { get; init; } = true;
    public bool Goal110PackagePresent { get; init; }
    public bool Goal111DecisionPresent { get; init; }
    public bool Goal112ArtifactsPresent { get; init; }
    public bool NegativeNoResultNoAcceptancePassed { get; init; }
    public bool NegativeInvalidResultPassed { get; init; }
    public bool DraftTemplateWrittenOutsideManualPath { get; init; } = true;
    public int ChecklistStepCount { get; init; }
    public int ProceduralFileCount { get; init; }
    public int ExportFileCount { get; init; }
    public int SourceHealthScannedFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualResultWorkbenchNegativeProof
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.GoalId;
    public string ScenarioId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public bool ManualResultPresent { get; init; }
    public bool AcceptedByCodex { get; init; }
    public bool HumanAcceptanceStillRequired { get; init; } = true;
    public string WorkbenchStatus { get; init; } = string.Empty;
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaManualResultWorkbenchFileIndex
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool SelfHashExcluded { get; init; } = true;
    public IReadOnlyList<OfflineGeoworldAlphaManualResultWorkbenchFileIndexEntry> Files { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaManualResultWorkbenchFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
