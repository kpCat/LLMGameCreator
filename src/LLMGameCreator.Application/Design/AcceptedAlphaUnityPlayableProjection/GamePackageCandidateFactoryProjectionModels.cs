namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class GamePackageCandidateFactoryProjectionVocabulary
{
    public const string GoalId =
        "goal_130_gamepackage_candidate_factory_and_matrix_pipeline";
    public const string ScenarioId =
        "goal-130-gamepackage-candidate-factory-and-matrix-pipeline";
    public const string SamplePackagePath = "samples/minimal-map-game/package.json";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-130-gamepackage-candidate-factory-and-matrix-pipeline";
    public const string CandidateRootDirectory =
        ProceduralOutputDirectory + "/candidates";
    public const string MatrixRootDirectory =
        ProceduralOutputDirectory + "/matrix";
    public const string DocumentationPath =
        "docs/manual-acceptance/gamepackage-candidate-factory-and-matrix-pipeline.md";
    public const string FactoryScriptPath =
        ".devflow/scripts/run-gamepackage-candidate-factory.ps1";
    public const string FactoryCmdPath =
        ".devflow/scripts/run-gamepackage-candidate-factory.cmd";
    public const string MatrixScriptPath =
        ".devflow/scripts/run-gamepackage-projection-matrix.ps1";
    public const string NormalCommand =
        ".devflow\\scripts\\run-gamepackage-candidate-factory.cmd";

    public const string BaselineCandidateId = "minimal-map-game-baseline";
    public const string AlchemyCandidateId = "minimal-map-game-alchemy-route";
    public const string CombatCandidateId = "minimal-map-game-combat-route";

    public const string CandidateIndexFileName = "gamepackage-candidate-index.json";
    public const string FactoryResultFileName = "gamepackage-candidate-factory-result.json";
    public const string DashboardFileName = "gamepackage-candidate-factory-dashboard.json";
    public const string ScriptScanFileName = "gamepackage-candidate-factory-script-scan.json";
    public const string LogScanFileName = "gamepackage-candidate-factory-log-scan.json";
    public const string NegativeProofFileName = "gamepackage-candidate-factory-negative-proof.json";
    public const string ReportFileName = "gamepackage-candidate-factory-report.md";
    public const string FileIndexFileName = "gamepackage-candidate-factory-file-index.json";
    public const string MatrixResultFileName = "gamepackage-projection-matrix-result.json";

    public const string CandidateIndexRelativePath =
        ProceduralOutputDirectory + "/" + CandidateIndexFileName;
    public const string FactoryResultRelativePath =
        ProceduralOutputDirectory + "/" + FactoryResultFileName;
    public const string DashboardRelativePath =
        ProceduralOutputDirectory + "/" + DashboardFileName;
    public const string MatrixResultRelativePath =
        ProceduralOutputDirectory + "/" + MatrixResultFileName;

    public static IReadOnlyList<string> RequiredCandidateIds =>
    [
        BaselineCandidateId,
        AlchemyCandidateId,
        CombatCandidateId
    ];

    public static IReadOnlyList<string> RequiredCompatibilityIds =>
    [
        "entity/village/sign",
        "interaction/sign_inspect",
        "entity/village/old_guard",
        "dialogue/old_guard_intro",
        "quest/help_healer",
        "inventory/player_start",
        "recipe/healing_potion",
        "node/apple_tree",
        "transaction/buy_healing_potion",
        "encounter/goblin_duel"
    ];
}

public sealed record GamePackageCandidateFactoryProjectionBuildResult
{
    public GamePackageCandidateFactoryDashboard Dashboard { get; init; } = new();
    public GamePackageCandidateFactoryScriptScan ScriptScan { get; init; } = new();
    public GamePackageCandidateFactoryIndexScan CandidateIndexScan { get; init; } = new();
    public GamePackageCandidateFactoryResultScan FactoryResultScan { get; init; } = new();
    public GamePackageCandidateFactoryMatrixResultScan MatrixResultScan { get; init; } = new();
    public GamePackageCandidateFactoryLogScan LogScan { get; init; } = new();
    public GamePackageCandidateFactoryNegativeProof NegativeProof { get; init; } = new();
    public GamePackageCandidateFactoryFileIndex ProceduralFileIndex { get; init; } = new();
    public GamePackageCandidateFactoryFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record GamePackageCandidateFactoryProjectionWriteResult
{
    public GamePackageCandidateFactoryProjectionBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record GamePackageCandidateFactoryDashboard
{
    public string GoalId { get; init; } = GamePackageCandidateFactoryProjectionVocabulary.GoalId;
    public string CandidateFactoryStatus { get; init; } = "BLOCKED";
    public int CandidateCount { get; init; }
    public int PassedCandidates { get; init; }
    public int FailedCandidates { get; init; }
    public bool MatrixPassed { get; init; }
    public string CandidateIndexPath { get; init; } =
        GamePackageCandidateFactoryProjectionVocabulary.CandidateIndexRelativePath;
    public string NormalCommand { get; init; } =
        GamePackageCandidateFactoryProjectionVocabulary.NormalCommand;
    public string FactoryResultPath { get; init; } =
        GamePackageCandidateFactoryProjectionVocabulary.FactoryResultRelativePath;
    public string MatrixResultPath { get; init; } =
        GamePackageCandidateFactoryProjectionVocabulary.MatrixResultRelativePath;
    public bool ManualUnityOptional { get; init; } = true;
    public bool SamplePackageUnmodified { get; init; }
    public bool ProjectionOnly { get; init; } = true;
    public string EvidencePath { get; init; } =
        GamePackageCandidateFactoryProjectionVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory;
    public bool FactoryScriptExists { get; init; }
    public bool CandidateFactoryResultExists { get; init; }
    public bool CandidateIndexPassed { get; init; }
    public bool FactoryResultPassed { get; init; }
    public bool MatrixResultPassed { get; init; }
    public bool CandidatePackagesUnderGoal130Roots { get; init; }
    public bool CandidatePackageHashesDiffer { get; init; }
    public bool NoForbiddenPathsExpected { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GamePackageCandidateFactoryScriptScan
{
    public string GoalId { get; init; } = GamePackageCandidateFactoryProjectionVocabulary.GoalId;
    public bool FactoryScriptExists { get; init; }
    public bool FactoryCmdExists { get; init; }
    public bool MatrixRunnerScriptExists { get; init; }
    public bool SupportsTemplatePackagePath { get; init; }
    public bool SupportsOutputRoot { get; init; }
    public bool SupportsUnityPath { get; init; }
    public bool SupportsDryRun { get; init; }
    public bool SupportsApplyCleanup { get; init; }
    public bool RejectsOutsideRepository { get; init; }
    public bool RejectsManualInputRoot { get; init; }
    public bool RefusesWritesOutsideGoal130Root { get; init; }
    public bool MaterializesCandidatesBeforeMatrix { get; init; }
    public bool InvokesGoal129MatrixRunner { get; init; }
    public bool CmdWrapperUsesApplyCleanup { get; init; }
    public bool NoBroadGitClean { get; init; }
    public bool NoLlmProviderNetwork { get; init; }
    public bool Passed { get; init; }
}

public sealed record GamePackageCandidateFactoryIndexScan
{
    public string GoalId { get; init; } = GamePackageCandidateFactoryProjectionVocabulary.GoalId;
    public bool IndexExists { get; init; }
    public bool Passed { get; init; }
    public int CandidateCount { get; init; }
    public bool RequiredCandidateIdsPresent { get; init; }
    public bool CandidatePackagesExist { get; init; }
    public bool CandidatePackagesUnderGoal130Roots { get; init; }
    public bool CandidatePackageHashesDiffer { get; init; }
    public bool RequiredCompatibilityIdsPreserved { get; init; }
    public bool SourceTemplateHashMatchesSample { get; init; }
    public string SourceTemplateSha256 { get; init; } = string.Empty;
    public IReadOnlyList<GamePackageCandidateFactoryIndexEntryScan> Candidates { get; init; } = [];
}

public sealed record GamePackageCandidateFactoryIndexEntryScan
{
    public string CandidateId { get; init; } = string.Empty;
    public string PackagePathRelative { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public bool PackageExists { get; init; }
    public bool PackagePathUnderGoal130Root { get; init; }
    public bool PackageHashMatchesIndex { get; init; }
    public bool RequiredCompatibilityIdsPresent { get; init; }
}

public sealed record GamePackageCandidateFactoryResultScan
{
    public string GoalId { get; init; } = GamePackageCandidateFactoryProjectionVocabulary.GoalId;
    public bool ResultExists { get; init; }
    public string CandidateFactoryStatus { get; init; } = string.Empty;
    public int CandidateCount { get; init; }
    public bool MatrixPassed { get; init; }
    public int PassedCandidates { get; init; }
    public int FailedCandidates { get; init; }
    public bool AllCandidatePackagesExist { get; init; }
    public bool AllCandidatePackagesDiffer { get; init; }
    public bool SamplePackageUnmodified { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool ProjectionOnly { get; init; } = true;
    public string CandidateIndexPath { get; init; } = string.Empty;
    public string NormalCommand { get; init; } = string.Empty;
    public string FactoryResultPath { get; init; } = string.Empty;
    public string MatrixResultPath { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record GamePackageCandidateFactoryMatrixResultScan
{
    public string GoalId { get; init; } = GamePackageCandidateFactoryProjectionVocabulary.GoalId;
    public bool ResultExists { get; init; }
    public string MatrixStatus { get; init; } = string.Empty;
    public int CandidateCount { get; init; }
    public int PassedCandidateCount { get; init; }
    public int FailedCandidateCount { get; init; }
    public bool Passed { get; init; }
    public bool AllEntriesPassed { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool ProjectionOnly { get; init; } = true;
}

public sealed record GamePackageCandidateFactoryLogScan
{
    public string GoalId { get; init; } = GamePackageCandidateFactoryProjectionVocabulary.GoalId;
    public bool LogScanExists { get; init; }
    public bool MatrixResultExists { get; init; }
    public bool MatrixPassed { get; init; }
    public int CandidateLogScanCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record GamePackageCandidateFactoryNegativeProof
{
    public string GoalId { get; init; } = GamePackageCandidateFactoryProjectionVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualInputRejected { get; init; }
    public bool TemplateUnderRepo { get; init; }
    public bool SamplePackageReadOnly { get; init; }
    public bool CandidatePathsUnderGoal130Artifacts { get; init; }
    public bool RuntimeSchemaProviderLuaGeneratorLibraryUnchanged { get; init; }
    public bool UnityAssetsProjectSettingsPackagesUnchanged { get; init; }
    public bool NoForbiddenPathsExpected { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
}

public sealed record GamePackageCandidateFactoryFileIndex
{
    public string GoalId { get; init; } = GamePackageCandidateFactoryProjectionVocabulary.GoalId;
    public string RootPath { get; init; } = string.Empty;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<GamePackageCandidateFactoryFileIndexEntry> Files { get; init; } = [];
}

public sealed record GamePackageCandidateFactoryFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
