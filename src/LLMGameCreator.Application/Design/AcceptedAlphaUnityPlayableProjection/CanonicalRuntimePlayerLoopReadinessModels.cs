using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class CanonicalRuntimePlayerLoopReadinessVocabulary
{
    public const string GoalId =
        "goal_135_canonical_runtime_playable_player_loop_readiness";
    public const string ScenarioId =
        "goal-135-canonical-runtime-playable-player-loop-readiness";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-135-canonical-runtime-playable-player-loop-readiness";
    public const string DefaultCanonicalRuntimeTranscriptPath =
        ".llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-transcript.json";
    public const string DefaultCanonicalRuntimeStateSummaryPath =
        ".llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-state-summary.json";
    public const string DefaultCanonicalRuntimeDashboardPath =
        ".llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-dashboard.json";
    public const string DocumentationPath =
        "docs/manual-acceptance/canonical-runtime-playable-player-loop-readiness.md";
    public const string ScriptPath =
        ".devflow/scripts/run-canonical-runtime-player-loop-readiness.ps1";
    public const string CmdPath =
        ".devflow/scripts/run-canonical-runtime-player-loop-readiness.cmd";
    public const string NormalCommand =
        ".devflow\\scripts\\run-canonical-runtime-player-loop-readiness.cmd";
    public const string UnityPassMarker =
        "GOAL135_CANONICAL_RUNTIME_PLAYER_LOOP_READINESS_PASS";
    public const string UnityFailMarker =
        "GOAL135_CANONICAL_RUNTIME_PLAYER_LOOP_READINESS_FAIL";

    public const string AdapterContractFileName =
        "canonical-runtime-player-adapter-contract.json";
    public const string PlayerLoopPlanFileName =
        "canonical-runtime-player-loop-plan.json";
    public const string ReadinessResultFileName =
        "canonical-runtime-player-loop-readiness-result.json";
    public const string DashboardFileName =
        "canonical-runtime-player-loop-readiness-dashboard.json";
    public const string MatrixResultFileName =
        "canonical-runtime-player-loop-readiness-matrix-result.json";
    public const string DiagnosticClassificationFileName =
        "canonical-runtime-diagnostic-classification.json";
    public const string UnitySmokeFileName =
        "unity-player-loop-readiness-smoke.json";
    public const string ReportJsonFileName =
        "one-click-player-loop-readiness-report.json";
    public const string ReportMarkdownFileName =
        "one-click-player-loop-readiness-report.md";
    public const string NegativeProofFileName =
        "canonical-runtime-player-loop-negative-proof.json";
    public const string FileIndexFileName =
        "canonical-runtime-player-loop-file-index.json";

    public const string DashboardRelativePath =
        ProceduralOutputDirectory + "/" + DashboardFileName;
    public const string ReportMarkdownRelativePath =
        ProceduralOutputDirectory + "/" + ReportMarkdownFileName;
    public const string MatrixResultRelativePath =
        ProceduralOutputDirectory + "/" + MatrixResultFileName;
}

public sealed record CanonicalRuntimePlayerLoopReadinessWriteResult
{
    public CanonicalRuntimePlayerLoopReadinessDashboard Dashboard { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record CanonicalRuntimePlayerLoopDiagnosticClassification
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId;
    public int RawDiagnosticCount { get; init; }
    public int BlockingDiagnosticCount { get; init; }
    public int NonBlockingDiagnosticCount { get; init; }
    public IReadOnlyList<CanonicalRuntimePlayerLoopClassifiedDiagnostic> BlockingDiagnostics { get; init; } = [];
    public IReadOnlyList<CanonicalRuntimePlayerLoopClassifiedDiagnostic> NonBlockingDiagnostics { get; init; } = [];
    public bool PassAllowsNonBlockingDiagnostics { get; init; } = true;
    public bool NoUnclassifiedErrorDiagnostics { get; init; }
}

public sealed record CanonicalRuntimePlayerLoopClassifiedDiagnostic
{
    public string RawDiagnostic { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public bool Blocking { get; init; }
    public bool NonBlockingForCanonicalRuntimePath { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public sealed record CanonicalRuntimePlayerLoopUnitySmoke
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId;
    public bool UnityAvailable { get; init; }
    public bool PlanPathExists { get; init; }
    public bool StateSummaryPathExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerPresent { get; init; }
    public bool RequiredStepCategoriesPresent { get; init; }
    public bool CanonicalAuthorityMarkersPresent { get; init; }
    public bool UnityPlayerLoopReadinessPassed { get; init; }
    public bool Passed { get; init; }
    public string UnityPath { get; init; } = string.Empty;
    public string UnityLogPath { get; init; } = string.Empty;
    public string PlanPath { get; init; } = string.Empty;
    public string StateSummaryPath { get; init; } = string.Empty;
    public string Status { get; init; } = "PENDING_UNITY_BATCHMODE";
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record CanonicalRuntimePlayerLoopPlanDocument
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public bool CanonicalRuntimeSource { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
    public IReadOnlyList<string> RequiredStepCategories { get; init; } = [];
    public bool RequiredStepCategoriesPresent { get; init; }
    public int PlayerLoopStepCount { get; init; }
    public IReadOnlyList<CanonicalRuntimePlayerLoopStep> Steps { get; init; } = [];
}

public sealed record CanonicalRuntimePlayerLoopReadinessMatrixResult
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId;
    public bool Passed { get; init; }
    public IReadOnlyList<CanonicalRuntimePlayerLoopReadinessMatrixRow> Rows { get; init; } = [];
}

public sealed record CanonicalRuntimePlayerLoopReadinessMatrixRow
{
    public string CandidateId { get; init; } = string.Empty;
    public bool PlayerAdapterContractPresent { get; init; }
    public bool PlayerLoopPlanPresent { get; init; }
    public int PlayerLoopStepCount { get; init; }
    public bool RequiredStepCategoriesPresent { get; init; }
    public bool UnityPlayerLoopReadinessPassed { get; init; }
    public bool NoUnclassifiedErrorDiagnostics { get; init; }
    public bool Passed { get; init; }
}

public sealed record CanonicalRuntimePlayerLoopReadinessReport
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public bool PlayerAdapterContractPresent { get; init; }
    public bool PlayerLoopPlanPresent { get; init; }
    public int PlayerLoopStepCount { get; init; }
    public bool RequiredStepCategoriesPresent { get; init; }
    public bool UnityPlayerLoopReadinessPassed { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool CanonicalRuntimeSource { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool SaveLoadReplayStillReferenced { get; init; }
    public bool SelectedCandidateExecutedByRuntime { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool NoUnclassifiedErrorDiagnostics { get; init; }
    public int RawDiagnosticCount { get; init; }
    public int BlockingDiagnosticCount { get; init; }
    public int NonBlockingDiagnosticCount { get; init; }
    public string ReportPath { get; init; } =
        CanonicalRuntimePlayerLoopReadinessVocabulary.ReportMarkdownRelativePath;
    public string MatrixResultPath { get; init; } =
        CanonicalRuntimePlayerLoopReadinessVocabulary.MatrixResultRelativePath;
}

public sealed record CanonicalRuntimePlayerLoopNegativeProof
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId;
    public bool ManualInputRejected { get; init; }
    public bool OutputRootUnderGoal135 { get; init; }
    public bool SamplePackageReadOnly { get; init; }
    public bool GamePackageSchemaUnchanged { get; init; }
    public bool GeneratorLibraryProviderLuaUnchanged { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged { get; init; }
    public bool PlayerAdapterDoesNotExecuteGameplay { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool Passed { get; init; }
}

public sealed record CanonicalRuntimePlayerLoopFileIndex
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId;
    public string RootPath { get; init; } = string.Empty;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; }
    public IReadOnlyList<CanonicalRuntimePlayerLoopFileIndexEntry> Files { get; init; } = [];
}

public sealed record CanonicalRuntimePlayerLoopFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record CanonicalRuntimePlayerLoopReadinessDashboard
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId;
    public string Status { get; init; } = "BLOCKED";
    public string CandidateId { get; init; } = string.Empty;
    public bool ProjectionOnly { get; init; }
    public bool CanonicalRuntimeSource { get; init; }
    public bool PlayerAdapterContractPresent { get; init; }
    public bool PlayerLoopPlanPresent { get; init; }
    public int PlayerLoopStepCount { get; init; }
    public bool RequiredStepCategoriesPresent { get; init; }
    public bool UnityPlayerLoopReadinessPassed { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool SaveLoadReplayStillReferenced { get; init; }
    public bool SelectedCandidateExecutedByRuntime { get; init; }
    public bool NoUnclassifiedErrorDiagnostics { get; init; }
    public string NormalCommand { get; init; } =
        CanonicalRuntimePlayerLoopReadinessVocabulary.NormalCommand;
    public string ReportPath { get; init; } =
        CanonicalRuntimePlayerLoopReadinessVocabulary.ReportMarkdownRelativePath;
    public string MatrixResultPath { get; init; } =
        CanonicalRuntimePlayerLoopReadinessVocabulary.MatrixResultRelativePath;
    public IReadOnlyList<string> MissingStepCategories { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}
