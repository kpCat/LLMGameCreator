namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class GamePackageCandidateMatrixProjectionVocabulary
{
    public const string GoalId =
        "goal_129_gamepackage_candidate_matrix_projection_runner";
    public const string ScenarioId =
        "goal-129-gamepackage-candidate-matrix-projection-runner";
    public const string Mode = "GenericFullPlaythrough";
    public const string SamplePackagePath = "samples/minimal-map-game/package.json";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-129-gamepackage-candidate-matrix-projection-runner";
    public const string CandidateRootDirectory =
        ProceduralOutputDirectory + "/candidates";
    public const string MatrixRootDirectory =
        ProceduralOutputDirectory + "/matrix";
    public const string DocumentationPath =
        "docs/manual-acceptance/gamepackage-candidate-matrix-projection-runner.md";
    public const string MatrixScriptPath =
        ".devflow/scripts/run-gamepackage-projection-matrix.ps1";
    public const string MatrixCmdPath =
        ".devflow/scripts/run-gamepackage-projection-matrix.cmd";
    public const string UnityRunnerScriptPath =
        ".devflow/scripts/run-unity-projection-verification.ps1";
    public const string NormalCommand =
        ".devflow\\scripts\\run-gamepackage-projection-matrix.cmd";
    public const string ExampleCommand =
        ".devflow\\scripts\\run-gamepackage-projection-matrix.cmd -CandidateIndexPath .llmgc\\procedural\\goal-129-gamepackage-candidate-matrix-projection-runner\\gamepackage-candidate-index.json";

    public const string BaselineCandidateId = "minimal-map-game-baseline";
    public const string VariantCandidateId = "minimal-map-game-variant";
    public const string BaselineCandidatePackagePath =
        CandidateRootDirectory + "/" + BaselineCandidateId + "/package.json";
    public const string VariantCandidatePackagePath =
        CandidateRootDirectory + "/" + VariantCandidateId + "/package.json";

    public const string CandidateIndexFileName = "gamepackage-candidate-index.json";
    public const string MatrixResultFileName = "gamepackage-projection-matrix-result.json";
    public const string DashboardFileName = "gamepackage-candidate-matrix-dashboard.json";
    public const string ScriptScanFileName = "gamepackage-candidate-matrix-script-scan.json";
    public const string LogScanFileName = "gamepackage-candidate-matrix-log-scan.json";
    public const string NegativeProofFileName = "gamepackage-candidate-matrix-negative-proof.json";
    public const string ReportFileName = "gamepackage-candidate-matrix-report.md";
    public const string FileIndexFileName = "gamepackage-candidate-matrix-file-index.json";

    public const string CandidateIndexRelativePath =
        ProceduralOutputDirectory + "/" + CandidateIndexFileName;
    public const string MatrixResultRelativePath =
        ProceduralOutputDirectory + "/" + MatrixResultFileName;

    public static IReadOnlyList<string> RequiredCompatibilityIds =>
    [
        "map/village",
        "entity/village/sign",
        "entity/village/old_guard",
        "interaction/sign_inspect",
        "dialogue/old_guard_intro",
        "quest/help_healer",
        "recipe/healing_potion",
        "node/apple_tree",
        "transaction/buy_healing_potion",
        "encounter/goblin_duel"
    ];
}

public sealed record GamePackageCandidateMatrixProjectionBuildResult
{
    public GamePackageCandidateMatrixDashboard Dashboard { get; init; } = new();
    public GamePackageCandidateIndexDocument CandidateIndex { get; init; } = new();
    public GamePackageCandidateMatrixScriptScan ScriptScan { get; init; } = new();
    public GamePackageCandidateMatrixResultScan MatrixResultScan { get; init; } = new();
    public GamePackageCandidateMatrixLogScan LogScan { get; init; } = new();
    public GamePackageCandidateMatrixNegativeProof NegativeProof { get; init; } = new();
    public GamePackageCandidateMatrixFileIndex ProceduralFileIndex { get; init; } = new();
    public GamePackageCandidateMatrixFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, byte[]> CandidatePackageBytes { get; init; } =
        new SortedDictionary<string, byte[]>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record GamePackageCandidateMatrixProjectionWriteResult
{
    public GamePackageCandidateMatrixProjectionBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record GamePackageCandidateIndexDocument
{
    public string SchemaVersion { get; init; } = "gamepackage_candidate_index_v1";
    public string GoalId { get; init; } = GamePackageCandidateMatrixProjectionVocabulary.GoalId;
    public string SourceSamplePath { get; init; } =
        GamePackageCandidateMatrixProjectionVocabulary.SamplePackagePath;
    public int CandidateCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<GamePackageCandidateIndexEntry> Candidates { get; init; } = [];
}

public sealed record GamePackageCandidateIndexEntry
{
    public string CandidateId { get; init; } = string.Empty;
    public string PackagePath { get; init; } = string.Empty;
    public string PackagePathRelative { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string SourceKind { get; init; } = string.Empty;
    public string ExpectedProjectionMode { get; init; } =
        GamePackageCandidateMatrixProjectionVocabulary.Mode;
    public IReadOnlyList<string> RequiredCompatibilityIds { get; init; } =
        GamePackageCandidateMatrixProjectionVocabulary.RequiredCompatibilityIds;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record GamePackageCandidateMatrixDashboard
{
    public string GoalId { get; init; } = GamePackageCandidateMatrixProjectionVocabulary.GoalId;
    public string MatrixStatus { get; init; } = "BLOCKED";
    public int CandidateCount { get; init; }
    public int PassedCandidateCount { get; init; }
    public int FailedCandidateCount { get; init; }
    public string CandidateIndexPath { get; init; } =
        GamePackageCandidateMatrixProjectionVocabulary.CandidateIndexRelativePath;
    public string MatrixResultPath { get; init; } =
        GamePackageCandidateMatrixProjectionVocabulary.MatrixResultRelativePath;
    public string NormalCommand { get; init; } =
        GamePackageCandidateMatrixProjectionVocabulary.NormalCommand;
    public string ExampleCommand { get; init; } =
        GamePackageCandidateMatrixProjectionVocabulary.ExampleCommand;
    public string BaselineCandidatePackagePath { get; init; } =
        GamePackageCandidateMatrixProjectionVocabulary.BaselineCandidatePackagePath;
    public string VariantCandidatePackagePath { get; init; } =
        GamePackageCandidateMatrixProjectionVocabulary.VariantCandidatePackagePath;
    public bool ManualUnityOptional { get; init; } = true;
    public bool CleanupApplied { get; init; }
    public bool ProjectionOnly { get; init; } = true;
    public bool CandidateIndexExists { get; init; }
    public bool MatrixRunnerScriptExists { get; init; }
    public bool MatrixResultExists { get; init; }
    public bool PassMarkersPresent { get; init; }
    public bool FailMarkersAbsent { get; init; } = true;
    public bool MaterialWarningAbsent { get; init; } = true;
    public bool SamplePackageUnmodified { get; init; }
    public bool NoForbiddenPathsExpected { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GamePackageCandidateMatrixScriptScan
{
    public string GoalId { get; init; } = GamePackageCandidateMatrixProjectionVocabulary.GoalId;
    public bool MatrixRunnerScriptExists { get; init; }
    public bool MatrixRunnerCmdExists { get; init; }
    public bool SupportsDefaultCandidateIndexPath { get; init; }
    public bool SupportsDryRun { get; init; }
    public bool InvokesParameterizedUnityProjectionRunner { get; init; }
    public bool CallsGenericFullPlaythroughMode { get; init; }
    public bool PassesCandidatePackagePath { get; init; }
    public bool PassesApplyCleanup { get; init; }
    public bool SupportsPerCandidateResultAndLogPaths { get; init; }
    public bool RejectsOutsideRepository { get; init; }
    public bool RejectsManualInputRoot { get; init; }
    public bool WritesAggregateMatrixResult { get; init; }
    public bool NoBroadGitClean { get; init; }
    public bool NoForbiddenMutationTargets { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record GamePackageCandidateMatrixResultScan
{
    public string GoalId { get; init; } = GamePackageCandidateMatrixProjectionVocabulary.GoalId;
    public bool ResultExists { get; init; }
    public string MatrixStatus { get; init; } = string.Empty;
    public int CandidateCount { get; init; }
    public int PassedCandidateCount { get; init; }
    public int FailedCandidateCount { get; init; }
    public bool CleanupApplied { get; init; }
    public bool ProjectionOnly { get; init; } = true;
    public bool ManualUnityOptional { get; init; } = true;
    public bool AllEntriesPassed { get; init; }
    public bool PassMarkersPresent { get; init; }
    public bool FailMarkersAbsent { get; init; } = true;
    public bool MaterialWarningAbsent { get; init; } = true;
    public bool Passed { get; init; }
}

public sealed record GamePackageCandidateMatrixLogScan
{
    public string GoalId { get; init; } = GamePackageCandidateMatrixProjectionVocabulary.GoalId;
    public bool MatrixResultExists { get; init; }
    public int CandidateLogScanCount { get; init; }
    public bool PassMarkersPresent { get; init; }
    public bool FailMarkersAbsent { get; init; } = true;
    public bool MaterialWarningAbsent { get; init; } = true;
    public bool Passed { get; init; }
    public IReadOnlyList<string> MissingLogScans { get; init; } = [];
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record GamePackageCandidateMatrixNegativeProof
{
    public string GoalId { get; init; } = GamePackageCandidateMatrixProjectionVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualInputRejected { get; init; }
    public bool SamplePackageReadOnly { get; init; }
    public bool CandidatePathsUnderGoalArtifacts { get; init; }
    public bool RuntimeSchemaProviderLuaGeneratorLibraryRejected { get; init; }
    public bool UnitySourceProjectSettingsPackagesRejected { get; init; }
    public bool NoForbiddenPathsExpected { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
}

public sealed record GamePackageCandidateMatrixFileIndex
{
    public string GoalId { get; init; } = GamePackageCandidateMatrixProjectionVocabulary.GoalId;
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<GamePackageCandidateMatrixFileIndexEntry> Files { get; init; } = [];
}

public sealed record GamePackageCandidateMatrixFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
