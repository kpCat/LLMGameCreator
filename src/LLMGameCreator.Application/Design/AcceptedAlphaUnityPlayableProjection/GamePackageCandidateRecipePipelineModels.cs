namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class GamePackageCandidateRecipePipelineVocabulary
{
    public const string GoalId =
        "goal_131_gamepackage_candidate_recipe_catalog_scoring_and_promotion";
    public const string ScenarioId =
        "goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion";
    public const string SamplePackagePath = "samples/minimal-map-game/package.json";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion";
    public const string CandidateRootDirectory =
        ProceduralOutputDirectory + "/candidates";
    public const string MatrixRootDirectory =
        ProceduralOutputDirectory + "/matrix";
    public const string SelectedCandidateDirectory =
        ProceduralOutputDirectory + "/selected-candidate";
    public const string DocumentationPath =
        "docs/manual-acceptance/gamepackage-candidate-recipe-catalog-scoring-and-promotion.md";
    public const string RecipePipelineScriptPath =
        ".devflow/scripts/run-gamepackage-candidate-recipe-pipeline.ps1";
    public const string RecipePipelineCmdPath =
        ".devflow/scripts/run-gamepackage-candidate-recipe-pipeline.cmd";
    public const string MatrixScriptPath =
        ".devflow/scripts/run-gamepackage-projection-matrix.ps1";
    public const string NormalCommand =
        ".devflow\\scripts\\run-gamepackage-candidate-recipe-pipeline.cmd";

    public const string BalancedBaselineRecipeId = "balanced_baseline";
    public const string AlchemyFocusRecipeId = "alchemy_focus";
    public const string CombatFocusRecipeId = "combat_focus";
    public const string ExplorationFocusRecipeId = "exploration_focus";

    public const string BalancedBaselineCandidateId = "minimal-map-game-balanced-baseline";
    public const string AlchemyFocusCandidateId = "minimal-map-game-alchemy-focus";
    public const string CombatFocusCandidateId = "minimal-map-game-combat-focus";
    public const string ExplorationFocusCandidateId = "minimal-map-game-exploration-focus";

    public const string RecipeCatalogFileName = "candidate-recipe-catalog.json";
    public const string CandidateIndexFileName = "gamepackage-candidate-index.json";
    public const string PipelineResultFileName = "gamepackage-recipe-pipeline-result.json";
    public const string ScoringResultFileName = "candidate-scoring-result.json";
    public const string DashboardFileName = "gamepackage-candidate-recipe-pipeline-dashboard.json";
    public const string ScriptScanFileName = "gamepackage-candidate-recipe-pipeline-script-scan.json";
    public const string LogScanFileName = "gamepackage-candidate-recipe-pipeline-log-scan.json";
    public const string NegativeProofFileName = "gamepackage-candidate-recipe-pipeline-negative-proof.json";
    public const string ReportFileName = "gamepackage-candidate-recipe-pipeline-report.md";
    public const string FileIndexFileName = "gamepackage-candidate-recipe-pipeline-file-index.json";
    public const string MatrixResultFileName = "gamepackage-projection-matrix-result.json";
    public const string SelectedCandidatePackageFileName = "selected-candidate/package.json";
    public const string SelectedCandidateHandoffFileName =
        "selected-candidate/selected-candidate-handoff.json";

    public const string RecipeCatalogRelativePath =
        ProceduralOutputDirectory + "/" + RecipeCatalogFileName;
    public const string CandidateIndexRelativePath =
        ProceduralOutputDirectory + "/" + CandidateIndexFileName;
    public const string PipelineResultRelativePath =
        ProceduralOutputDirectory + "/" + PipelineResultFileName;
    public const string ScoringResultRelativePath =
        ProceduralOutputDirectory + "/" + ScoringResultFileName;
    public const string DashboardRelativePath =
        ProceduralOutputDirectory + "/" + DashboardFileName;
    public const string MatrixResultRelativePath =
        ProceduralOutputDirectory + "/" + MatrixResultFileName;
    public const string SelectedCandidatePackageRelativePath =
        ProceduralOutputDirectory + "/" + SelectedCandidatePackageFileName;
    public const string SelectedCandidateHandoffRelativePath =
        ProceduralOutputDirectory + "/" + SelectedCandidateHandoffFileName;

    public static IReadOnlyList<string> RequiredRecipeIds =>
    [
        BalancedBaselineRecipeId,
        AlchemyFocusRecipeId,
        CombatFocusRecipeId,
        ExplorationFocusRecipeId
    ];

    public static IReadOnlyList<string> RequiredCandidateIds =>
    [
        BalancedBaselineCandidateId,
        AlchemyFocusCandidateId,
        CombatFocusCandidateId,
        ExplorationFocusCandidateId
    ];

    public static IReadOnlyList<string> RequiredCompatibilityIds =>
    [
        "map/village",
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

public sealed record GamePackageCandidateRecipePipelineBuildResult
{
    public GamePackageCandidateRecipePipelineDashboard Dashboard { get; init; } = new();
    public GamePackageCandidateRecipePipelineScriptScan ScriptScan { get; init; } = new();
    public GamePackageCandidateRecipeCatalogScan RecipeCatalogScan { get; init; } = new();
    public GamePackageCandidateRecipeIndexScan CandidateIndexScan { get; init; } = new();
    public GamePackageCandidateRecipePipelineResultScan PipelineResultScan { get; init; } = new();
    public GamePackageCandidateRecipeScoringResultScan ScoringResultScan { get; init; } = new();
    public GamePackageCandidateRecipeMatrixResultScan MatrixResultScan { get; init; } = new();
    public GamePackageCandidateRecipeSelectedHandoffScan SelectedHandoffScan { get; init; } = new();
    public GamePackageCandidateRecipeLogScan LogScan { get; init; } = new();
    public GamePackageCandidateRecipeNegativeProof NegativeProof { get; init; } = new();
    public GamePackageCandidateRecipeFileIndex ProceduralFileIndex { get; init; } = new();
    public GamePackageCandidateRecipeFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record GamePackageCandidateRecipePipelineWriteResult
{
    public GamePackageCandidateRecipePipelineBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record GamePackageCandidateRecipePipelineDashboard
{
    public string GoalId { get; init; } = GamePackageCandidateRecipePipelineVocabulary.GoalId;
    public string RecipePipelineStatus { get; init; } = "BLOCKED";
    public int RecipeCount { get; init; }
    public int CandidateCount { get; init; }
    public int PassedCandidates { get; init; }
    public int FailedCandidates { get; init; }
    public bool MatrixPassed { get; init; }
    public string SelectedCandidateId { get; init; } = string.Empty;
    public int SelectedCandidateScore { get; init; }
    public string RecipeCatalogPath { get; init; } =
        GamePackageCandidateRecipePipelineVocabulary.RecipeCatalogRelativePath;
    public string CandidateIndexPath { get; init; } =
        GamePackageCandidateRecipePipelineVocabulary.CandidateIndexRelativePath;
    public string PipelineResultPath { get; init; } =
        GamePackageCandidateRecipePipelineVocabulary.PipelineResultRelativePath;
    public string ScoringResultPath { get; init; } =
        GamePackageCandidateRecipePipelineVocabulary.ScoringResultRelativePath;
    public string MatrixResultPath { get; init; } =
        GamePackageCandidateRecipePipelineVocabulary.MatrixResultRelativePath;
    public string SelectedCandidatePackagePath { get; init; } =
        GamePackageCandidateRecipePipelineVocabulary.SelectedCandidatePackageRelativePath;
    public string SelectedCandidateHandoffPath { get; init; } =
        GamePackageCandidateRecipePipelineVocabulary.SelectedCandidateHandoffRelativePath;
    public string NormalCommand { get; init; } =
        GamePackageCandidateRecipePipelineVocabulary.NormalCommand;
    public string EvidencePath { get; init; } =
        GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        GamePackageCandidateRecipePipelineVocabulary.ExportPackageDirectory;
    public bool ManualUnityOptional { get; init; } = true;
    public bool SamplePackageUnmodified { get; init; }
    public bool ProjectionOnly { get; init; } = true;
    public bool MetadataOnlyRecipeMutation { get; init; }
    public bool CatalogPassed { get; init; }
    public bool CandidateIndexPassed { get; init; }
    public bool PipelineResultPassed { get; init; }
    public bool ScoringResultPassed { get; init; }
    public bool MatrixResultPassed { get; init; }
    public bool SelectedHandoffPassed { get; init; }
    public bool NoForbiddenPathsExpected { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GamePackageCandidateRecipePipelineScriptScan
{
    public string GoalId { get; init; } = GamePackageCandidateRecipePipelineVocabulary.GoalId;
    public bool RecipePipelineScriptExists { get; init; }
    public bool RecipePipelineCmdExists { get; init; }
    public bool MatrixRunnerScriptExists { get; init; }
    public bool SupportsTemplatePackagePath { get; init; }
    public bool SupportsRecipeCatalogPath { get; init; }
    public bool SupportsOutputRoot { get; init; }
    public bool SupportsUnityPath { get; init; }
    public bool SupportsDryRun { get; init; }
    public bool SupportsApplyCleanup { get; init; }
    public bool RejectsOutsideRepository { get; init; }
    public bool RejectsManualInputRoot { get; init; }
    public bool RefusesWritesOutsideGoal131Root { get; init; }
    public bool InvokesGoal129MatrixRunner { get; init; }
    public bool ScoresCandidatesAfterMatrix { get; init; }
    public bool SelectsAndWritesHandoff { get; init; }
    public bool MetadataOnlyRecipeMutation { get; init; }
    public bool CmdWrapperUsesApplyCleanup { get; init; }
    public bool NoBroadGitClean { get; init; }
    public bool NoLlmProviderNetwork { get; init; }
    public bool Passed { get; init; }
}

public sealed record GamePackageCandidateRecipeCatalogScan
{
    public string GoalId { get; init; } = GamePackageCandidateRecipePipelineVocabulary.GoalId;
    public bool CatalogExists { get; init; }
    public bool Passed { get; init; }
    public int RecipeCount { get; init; }
    public bool RequiredRecipeIdsPresent { get; init; }
    public bool RequiredCandidateIdsPresent { get; init; }
    public bool CandidateIdsUnique { get; init; }
    public bool MetadataOnlySafeTuning { get; init; }
    public bool RequiredAnchorsPresent { get; init; }
    public IReadOnlyList<GamePackageCandidateRecipeCatalogEntryScan> Recipes { get; init; } = [];
}

public sealed record GamePackageCandidateRecipeCatalogEntryScan
{
    public string RecipeId { get; init; } = string.Empty;
    public string CandidateId { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public bool MetadataOnlySafeTuning { get; init; }
    public bool RequiredAnchorsPresent { get; init; }
}

public sealed record GamePackageCandidateRecipeIndexScan
{
    public string GoalId { get; init; } = GamePackageCandidateRecipePipelineVocabulary.GoalId;
    public bool IndexExists { get; init; }
    public bool Passed { get; init; }
    public int CandidateCount { get; init; }
    public bool RequiredCandidateIdsPresent { get; init; }
    public bool CandidatePackagesExist { get; init; }
    public bool CandidatePackagesUnderGoal131Roots { get; init; }
    public bool CandidatePackageHashesDiffer { get; init; }
    public bool RequiredCompatibilityIdsPreserved { get; init; }
    public bool SourceTemplateHashMatchesSample { get; init; }
    public bool ManifestTitlePreserved { get; init; }
    public bool CandidateMetadataPreservesFullPlaythrough { get; init; }
    public string SourceTemplateSha256 { get; init; } = string.Empty;
    public IReadOnlyList<GamePackageCandidateRecipeIndexEntryScan> Candidates { get; init; } = [];
}

public sealed record GamePackageCandidateRecipeIndexEntryScan
{
    public string RecipeId { get; init; } = string.Empty;
    public int RecipeOrder { get; init; }
    public string CandidateId { get; init; } = string.Empty;
    public string PackagePathRelative { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public bool PackageExists { get; init; }
    public bool PackagePathUnderGoal131Root { get; init; }
    public bool PackageHashMatchesIndex { get; init; }
    public bool RequiredCompatibilityIdsPresent { get; init; }
    public bool ManifestTitlePreserved { get; init; }
    public bool CandidateMetadataPreservesFullPlaythrough { get; init; }
}

public sealed record GamePackageCandidateRecipePipelineResultScan
{
    public string GoalId { get; init; } = GamePackageCandidateRecipePipelineVocabulary.GoalId;
    public bool ResultExists { get; init; }
    public string RecipePipelineStatus { get; init; } = string.Empty;
    public int RecipeCount { get; init; }
    public int CandidateCount { get; init; }
    public bool MatrixPassed { get; init; }
    public int PassedCandidates { get; init; }
    public int FailedCandidates { get; init; }
    public string SelectedCandidateId { get; init; } = string.Empty;
    public int SelectedCandidateScore { get; init; }
    public bool SelectedCandidatePackageExists { get; init; }
    public bool SamplePackageUnmodified { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool ProjectionOnly { get; init; } = true;
    public string NormalCommand { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record GamePackageCandidateRecipeScoringResultScan
{
    public string GoalId { get; init; } = GamePackageCandidateRecipePipelineVocabulary.GoalId;
    public bool ResultExists { get; init; }
    public string ScoringStatus { get; init; } = string.Empty;
    public int RecipeCount { get; init; }
    public int CandidateCount { get; init; }
    public int PassedCandidates { get; init; }
    public int FailedCandidates { get; init; }
    public string SelectedCandidateId { get; init; } = string.Empty;
    public int SelectedCandidateScore { get; init; }
    public bool AllEligibleCandidatesScored { get; init; }
    public bool SelectionRulePresent { get; init; }
    public bool Passed { get; init; }
}

public sealed record GamePackageCandidateRecipeMatrixResultScan
{
    public string GoalId { get; init; } = GamePackageCandidateRecipePipelineVocabulary.GoalId;
    public bool ResultExists { get; init; }
    public string MatrixStatus { get; init; } = string.Empty;
    public int CandidateCount { get; init; }
    public int PassedCandidateCount { get; init; }
    public int FailedCandidateCount { get; init; }
    public bool AllEntriesPassed { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool ProjectionOnly { get; init; } = true;
    public bool Passed { get; init; }
}

public sealed record GamePackageCandidateRecipeSelectedHandoffScan
{
    public string GoalId { get; init; } = GamePackageCandidateRecipePipelineVocabulary.GoalId;
    public bool HandoffExists { get; init; }
    public bool Passed { get; init; }
    public string SelectedCandidateId { get; init; } = string.Empty;
    public string SelectedRecipeId { get; init; } = string.Empty;
    public int SelectedCandidateScore { get; init; }
    public string SelectedCandidatePackagePath { get; init; } = string.Empty;
    public string SourceCandidatePackagePath { get; init; } = string.Empty;
    public bool SelectedCandidatePackageExists { get; init; }
    public bool SourceCandidatePackageExists { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool ProjectionOnly { get; init; } = true;
    public bool SamplePackageUnmodified { get; init; }
}

public sealed record GamePackageCandidateRecipeLogScan
{
    public string GoalId { get; init; } = GamePackageCandidateRecipePipelineVocabulary.GoalId;
    public bool LogScanExists { get; init; }
    public bool MatrixResultExists { get; init; }
    public bool MatrixPassed { get; init; }
    public int CandidateLogScanCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record GamePackageCandidateRecipeNegativeProof
{
    public string GoalId { get; init; } = GamePackageCandidateRecipePipelineVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualInputRejected { get; init; }
    public bool TemplateUnderRepo { get; init; }
    public bool SamplePackageReadOnly { get; init; }
    public bool CandidatePathsUnderGoal131Artifacts { get; init; }
    public bool RuntimeSchemaProviderLuaGeneratorLibraryUnchanged { get; init; }
    public bool UnityAssetsProjectSettingsPackagesUnchanged { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool NoForbiddenPathsExpected { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
}

public sealed record GamePackageCandidateRecipeFileIndex
{
    public string GoalId { get; init; } = GamePackageCandidateRecipePipelineVocabulary.GoalId;
    public string RootPath { get; init; } = string.Empty;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<GamePackageCandidateRecipeFileIndexEntry> Files { get; init; } = [];
}

public sealed record GamePackageCandidateRecipeFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
