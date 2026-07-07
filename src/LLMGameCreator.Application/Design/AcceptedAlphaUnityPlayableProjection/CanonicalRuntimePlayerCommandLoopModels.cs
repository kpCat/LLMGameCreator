using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class CanonicalRuntimePlayerCommandLoopVocabulary
{
    public const string GoalId =
        "goal_136_canonical_runtime_player_command_loop_execution_matrix";
    public const string ScenarioId =
        "goal-136-canonical-runtime-player-command-loop-execution-matrix";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-136-canonical-runtime-player-command-loop-execution-matrix";
    public const string DefaultSelectedCandidateHandoffPath =
        CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.DefaultSelectedCandidateHandoffPath;
    public const string DefaultSelectedCandidatePackagePath =
        CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.DefaultSelectedCandidatePackagePath;
    public const string DefaultGoal134TranscriptPath =
        CanonicalRuntimePlayerLoopReadinessVocabulary.DefaultCanonicalRuntimeTranscriptPath;
    public const string DefaultGoal134StateSummaryPath =
        CanonicalRuntimePlayerLoopReadinessVocabulary.DefaultCanonicalRuntimeStateSummaryPath;
    public const string DefaultGoal135PlayerLoopPlanPath =
        CanonicalRuntimePlayerLoopReadinessVocabulary.ProceduralOutputDirectory
        + "/"
        + CanonicalRuntimePlayerLoopReadinessVocabulary.PlayerLoopPlanFileName;
    public const string DefaultGoal135PlayerAdapterContractPath =
        CanonicalRuntimePlayerLoopReadinessVocabulary.ProceduralOutputDirectory
        + "/"
        + CanonicalRuntimePlayerLoopReadinessVocabulary.AdapterContractFileName;
    public const string DefaultGoal135DiagnosticClassificationPath =
        CanonicalRuntimePlayerLoopReadinessVocabulary.ProceduralOutputDirectory
        + "/"
        + CanonicalRuntimePlayerLoopReadinessVocabulary.DiagnosticClassificationFileName;
    public const string DocumentationPath =
        "docs/manual-acceptance/canonical-runtime-player-command-loop-execution-matrix.md";
    public const string ScriptPath =
        ".devflow/scripts/run-canonical-runtime-player-command-loop.ps1";
    public const string CmdPath =
        ".devflow/scripts/run-canonical-runtime-player-command-loop.cmd";
    public const string NormalCommand =
        ".devflow\\scripts\\run-canonical-runtime-player-command-loop.cmd";
    public const string UnityPassMarker =
        "GOAL136_CANONICAL_RUNTIME_PLAYER_COMMAND_LOOP_PASS";
    public const string UnityFailMarker =
        "GOAL136_CANONICAL_RUNTIME_PLAYER_COMMAND_LOOP_FAIL";

    public const string DashboardFileName =
        "canonical-runtime-player-command-loop-dashboard.json";
    public const string InputsFileName =
        "canonical-runtime-player-command-loop-inputs.json";
    public const string PlanFileName =
        "canonical-runtime-player-command-loop-plan.json";
    public const string SnapshotsFileName =
        "canonical-runtime-player-command-loop-snapshots.json";
    public const string ResultFileName =
        "canonical-runtime-player-command-loop-result.json";
    public const string MatrixResultFileName =
        "canonical-runtime-player-command-loop-matrix-result.json";
    public const string DiagnosticClassificationFileName =
        "canonical-runtime-player-command-loop-diagnostic-classification.json";
    public const string UnitySmokeFileName =
        "unity-player-command-loop-smoke.json";
    public const string ReportJsonFileName =
        "one-click-player-command-loop-report.json";
    public const string ReportMarkdownFileName =
        "one-click-player-command-loop-report.md";
    public const string NegativeProofFileName =
        "canonical-runtime-player-command-loop-negative-proof.json";
    public const string FileIndexFileName =
        "canonical-runtime-player-command-loop-file-index.json";

    public const string DashboardRelativePath =
        ProceduralOutputDirectory + "/" + DashboardFileName;
    public const string ReportMarkdownRelativePath =
        ProceduralOutputDirectory + "/" + ReportMarkdownFileName;
    public const string MatrixResultRelativePath =
        ProceduralOutputDirectory + "/" + MatrixResultFileName;
}

public sealed record CanonicalRuntimePlayerCommandLoopWriteResult
{
    public CanonicalRuntimePlayerCommandLoopDashboard Dashboard { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record CanonicalRuntimePlayerCommandLoopPlanDocument
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerCommandLoopVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool CanonicalRuntimeSource { get; init; } = true;
    public bool PlayerCommandLoopCoverage { get; init; }
    public IReadOnlyList<string> RequiredCategories { get; init; } = [];
    public bool AllRequiredCategoriesPresent { get; init; }
    public int PlayerCommandCount { get; init; }
    public IReadOnlyList<CanonicalRuntimePlayerCommandLoopStep> Steps { get; init; } = [];
}

public sealed record CanonicalRuntimePlayerCommandLoopDiagnosticClassification
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerCommandLoopVocabulary.GoalId;
    public int RawDiagnosticCount { get; init; }
    public int BlockingDiagnosticCount { get; init; }
    public int NonBlockingDiagnosticCount { get; init; }
    public IReadOnlyList<CanonicalRuntimePlayerCommandLoopClassifiedDiagnostic> BlockingDiagnostics { get; init; } = [];
    public IReadOnlyList<CanonicalRuntimePlayerCommandLoopClassifiedDiagnostic> NonBlockingDiagnostics { get; init; } = [];
    public bool PassAllowsNonBlockingDiagnostics { get; init; } = true;
    public bool NoUnclassifiedErrorDiagnostics { get; init; }
}

public sealed record CanonicalRuntimePlayerCommandLoopClassifiedDiagnostic
{
    public string RawDiagnostic { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public bool Blocking { get; init; }
    public bool NonBlockingForCanonicalRuntimePath { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public sealed record CanonicalRuntimePlayerCommandLoopUnitySmoke
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerCommandLoopVocabulary.GoalId;
    public bool UnityAvailable { get; init; }
    public bool SnapshotsPathExists { get; init; }
    public bool ResultPathExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerPresent { get; init; }
    public bool SnapshotContractPresent { get; init; }
    public bool UnityPlayerConsumedCommandLoopSnapshots { get; init; }
    public bool Passed { get; init; }
    public string UnityPath { get; init; } = string.Empty;
    public string UnityLogPath { get; init; } = string.Empty;
    public string SnapshotsPath { get; init; } = string.Empty;
    public string ResultPath { get; init; } = string.Empty;
    public string Status { get; init; } = "PENDING_UNITY_BATCHMODE";
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record CanonicalRuntimePlayerCommandLoopMatrixResult
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerCommandLoopVocabulary.GoalId;
    public bool Passed { get; init; }
    public IReadOnlyList<CanonicalRuntimePlayerCommandLoopMatrixRow> Rows { get; init; } = [];
}

public sealed record CanonicalRuntimePlayerCommandLoopMatrixRow
{
    public string CandidateId { get; init; } = string.Empty;
    public string PackagePath { get; init; } = string.Empty;
    public bool PlayerCommandLoopPassed { get; init; }
    public int PlayerCommandCount { get; init; }
    public int SnapshotCount { get; init; }
    public int RuntimeEventCount { get; init; }
    public bool AllRequiredCategoriesPresent { get; init; }
    public bool UnityPlayerConsumedCommandLoopSnapshots { get; init; }
    public bool Passed { get; init; }
}

public sealed record CanonicalRuntimePlayerCommandLoopReport
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerCommandLoopVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public bool PlayerCommandLoopPassed { get; init; }
    public int PlayerCommandCount { get; init; }
    public int SnapshotCount { get; init; }
    public int RuntimeEventCount { get; init; }
    public bool StateHashChainPresent { get; init; }
    public bool AllRequiredCategoriesPresent { get; init; }
    public bool UnityPlayerConsumedCommandLoopSnapshots { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool SelectedCandidateExecutedByRuntime { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool NoUnclassifiedErrorDiagnostics { get; init; }
    public int RawDiagnosticCount { get; init; }
    public int BlockingDiagnosticCount { get; init; }
    public int NonBlockingDiagnosticCount { get; init; }
    public string ReportPath { get; init; } =
        CanonicalRuntimePlayerCommandLoopVocabulary.ReportMarkdownRelativePath;
    public string MatrixResultPath { get; init; } =
        CanonicalRuntimePlayerCommandLoopVocabulary.MatrixResultRelativePath;
}

public sealed record CanonicalRuntimePlayerCommandLoopNegativeProof
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerCommandLoopVocabulary.GoalId;
    public bool ManualInputRejected { get; init; }
    public bool OutputRootUnderGoal136 { get; init; }
    public bool SamplePackageReadOnly { get; init; }
    public bool GamePackageSchemaUnchanged { get; init; }
    public bool GeneratorLibraryProviderLuaUnchanged { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged { get; init; }
    public bool RuntimeOwnsCommandExecution { get; init; }
    public bool PlayerAdapterDoesNotExecuteGameplay { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Passed { get; init; }
}

public sealed record CanonicalRuntimePlayerCommandLoopFileIndex
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerCommandLoopVocabulary.GoalId;
    public string RootPath { get; init; } = string.Empty;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; }
    public IReadOnlyList<CanonicalRuntimePlayerCommandLoopFileIndexEntry> Files { get; init; } = [];
}

public sealed record CanonicalRuntimePlayerCommandLoopFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record CanonicalRuntimePlayerCommandLoopDashboard
{
    public string GoalId { get; init; } = CanonicalRuntimePlayerCommandLoopVocabulary.GoalId;
    public string Status { get; init; } = "BLOCKED";
    public string CandidateId { get; init; } = string.Empty;
    public bool PlayerCommandLoopPassed { get; init; }
    public int PlayerCommandCount { get; init; }
    public int SnapshotCount { get; init; }
    public int RuntimeEventCount { get; init; }
    public bool StateHashChainPresent { get; init; }
    public bool AllRequiredCategoriesPresent { get; init; }
    public bool UnityPlayerConsumedCommandLoopSnapshots { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool SelectedCandidateExecutedByRuntime { get; init; }
    public bool RuntimePrimitiveMissing { get; init; }
    public IReadOnlyList<string> MissingRuntimePrimitives { get; init; } = [];
    public bool NoUnclassifiedErrorDiagnostics { get; init; }
    public int RawDiagnosticCount { get; init; }
    public int BlockingDiagnosticCount { get; init; }
    public int NonBlockingDiagnosticCount { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool Accepted { get; init; }
    public string NormalCommand { get; init; } =
        CanonicalRuntimePlayerCommandLoopVocabulary.NormalCommand;
    public string ReportPath { get; init; } =
        CanonicalRuntimePlayerCommandLoopVocabulary.ReportMarkdownRelativePath;
    public string MatrixResultPath { get; init; } =
        CanonicalRuntimePlayerCommandLoopVocabulary.MatrixResultRelativePath;
    public IReadOnlyList<string> MissingCategories { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}
