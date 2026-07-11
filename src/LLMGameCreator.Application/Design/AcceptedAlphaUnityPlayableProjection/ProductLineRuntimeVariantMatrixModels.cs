namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class ProductLineRuntimeVariantMatrixVocabulary
{
    public const string GoalId =
        "goal_142_runtime_significant_product_line_variant_matrix_and_selection_handoff";
    public const string ScenarioId =
        "goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff";
    public const string TemplatePackagePath = "samples/minimal-map-game/package.json";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff";
    public const string DocumentationPath =
        "docs/manual-acceptance/runtime-significant-product-line-variant-matrix-and-selection-handoff.md";
    public const string NormalCommand =
        ".devflow\\scripts\\run-product-line-runtime-variant-matrix.cmd";
    public const string ScriptPath =
        ".devflow/scripts/run-product-line-runtime-variant-matrix.ps1";
    public const string CmdPath =
        ".devflow/scripts/run-product-line-runtime-variant-matrix.cmd";

    public const string CatalogFileName = "product-line-runtime-variant-catalog.json";
    public const string DashboardFileName = "product-line-runtime-variant-matrix-dashboard.json";
    public const string MatrixResultFileName = "product-line-runtime-variant-matrix-result.json";
    public const string MutationSummaryFileName = "product-line-runtime-variant-mutation-summary.json";
    public const string DistinctnessProofFileName = "product-line-runtime-variant-distinctness-proof.json";
    public const string ScoreboardFileName = "product-line-runtime-variant-scoreboard.json";
    public const string NegativeProofFileName = "product-line-runtime-variant-negative-proof.json";
    public const string FileIndexFileName = "product-line-runtime-variant-file-index.json";
    public const string OneClickReportJsonFileName = "one-click-product-line-runtime-variant-matrix-report.json";
    public const string OneClickReportMarkdownFileName = "one-click-product-line-runtime-variant-matrix-report.md";

    public const string CatalogRelativePath = ProceduralOutputDirectory + "/" + CatalogFileName;
    public const string DashboardRelativePath = ProceduralOutputDirectory + "/" + DashboardFileName;
    public const string MatrixResultRelativePath = ProceduralOutputDirectory + "/" + MatrixResultFileName;
    public const string SelectedHandoffRelativePath =
        ProceduralOutputDirectory + "/selected-runtime-variant/selected-runtime-variant-handoff.json";

    public static IReadOnlyList<string> CandidateIds =>
    [
        "minimal-map-game-balanced-baseline",
        "minimal-map-game-alchemy-focus",
        "minimal-map-game-combat-focus",
        "minimal-map-game-exploration-resource-focus"
    ];

    public static IReadOnlyList<string> RequiredAnchors =>
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

public sealed record ProductLineRuntimeVariantMatrixRequest
{
    public string TemplatePackagePath { get; init; } =
        ProductLineRuntimeVariantMatrixVocabulary.TemplatePackagePath;
    public string VariantCatalogPath { get; init; } =
        ProductLineRuntimeVariantMatrixVocabulary.CatalogRelativePath;
    public string OutputRoot { get; init; } =
        ProductLineRuntimeVariantMatrixVocabulary.ProceduralOutputDirectory;
}

public sealed record ProductLineRuntimeVariantCatalogDocument
{
    public string SchemaVersion { get; init; } = "product_line_runtime_variant_catalog_v1";
    public string GoalId { get; init; } = ProductLineRuntimeVariantMatrixVocabulary.GoalId;
    public string SourceTemplate { get; init; } = ProductLineRuntimeVariantMatrixVocabulary.TemplatePackagePath;
    public bool Deterministic { get; init; } = true;
    public bool RuntimeSignificantVariants { get; init; } = true;
    public IReadOnlyList<ProductLineRuntimeVariantRecipe> Variants { get; init; } = [];
}

public sealed record ProductLineRuntimeVariantRecipe
{
    public string RecipeId { get; init; } = string.Empty;
    public string CandidateId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public bool RuntimeSignificant { get; init; } = true;
    public IReadOnlyList<ProductLineRuntimeVariantMutationOperation> MutationOperations { get; init; } = [];
    public IReadOnlyList<string> ExpectedRuntimeEffects { get; init; } = [];
    public ProductLineRuntimeVariantSelectionWeights SelectionWeights { get; init; } = new();
    public IReadOnlyList<string> RequiredAnchors { get; init; } =
        ProductLineRuntimeVariantMatrixVocabulary.RequiredAnchors;
}

public sealed record ProductLineRuntimeVariantMetadataContext
{
    public string GoalId { get; init; } = string.Empty;
    public string VersionSuffix { get; init; } = string.Empty;
    public string ManifestDescription { get; init; } = string.Empty;
    public string ProfileTitle { get; init; } = string.Empty;
    public string ProfileDescription { get; init; } = string.Empty;
    public string Genre { get; init; } = string.Empty;
    public string Tone { get; init; } = string.Empty;
    public string PresentationMode { get; init; } = string.Empty;
    public string WorldTopology { get; init; } = string.Empty;
    public string ActorModel { get; init; } = string.Empty;
    public string CombatModel { get; init; } = string.Empty;
    public string SourceContext { get; init; } = string.Empty;
}

public sealed record ProductLineRuntimeVariantSelectionWeights
{
    public int PackageValidation { get; init; } = 15;
    public int RoundtripSemanticCorrectness { get; init; } = 20;
    public int RequiredAnchorCoverage { get; init; } = 15;
    public int MutationAudit { get; init; } = 15;
    public int RuntimeEffectObserved { get; init; } = 15;
    public int RuntimeStateDistinctness { get; init; } = 10;
    public int NoBlockingDiagnostics { get; init; } = 5;
    public int ProfileSpecificObjective { get; init; } = 5;
    public int TieBreakPriority { get; init; }
}

public sealed record ProductLineRuntimeVariantMutationOperation
{
    public string OperationId { get; init; } = string.Empty;
    public string TargetKind { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string JsonPath { get; init; } = string.Empty;
    public string ExpectedValue { get; init; } = string.Empty;
    public string NewValue { get; init; } = string.Empty;
    public string RuntimeDimension { get; init; } = string.Empty;
}

public sealed record ProductLineRuntimeVariantMutationAudit
{
    public string SchemaVersion { get; init; } = "product_line_runtime_variant_mutation_audit_v1";
    public string GoalId { get; init; } = ProductLineRuntimeVariantMatrixVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public string RecipeId { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public bool RuntimeSignificant { get; init; }
    public int OperationCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<ProductLineRuntimeVariantMutationAuditEntry> Operations { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record ProductLineRuntimeVariantMutationAuditEntry
{
    public string OperationId { get; init; } = string.Empty;
    public string TargetKind { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string JsonPath { get; init; } = string.Empty;
    public string ExpectedValue { get; init; } = string.Empty;
    public string ActualOldValue { get; init; } = string.Empty;
    public string NewValue { get; init; } = string.Empty;
    public string RuntimeDimension { get; init; } = string.Empty;
    public bool Applied { get; init; }
    public bool Passed { get; init; }
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record ProductLineRuntimeVariantPackageValidation
{
    public string SchemaVersion { get; init; } = "product_line_runtime_variant_package_validation_v1";
    public string GoalId { get; init; } = ProductLineRuntimeVariantMatrixVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public string RecipeId { get; init; } = string.Empty;
    public string PackagePath { get; init; } = string.Empty;
    public bool CandidateFileExists { get; init; }
    public bool ValidJson { get; init; }
    public bool ExistingPackageValidatorPassed { get; init; }
    public bool HandoffCandidateIdMatchesPackageMetadata { get; init; }
    public bool RequiredAnchorsPresent { get; init; }
    public bool NoBrokenRequiredReferences { get; init; }
    public bool SourceTemplateUnchanged { get; init; }
    public bool CandidatePackageUnderGoal142Root { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> MissingAnchors { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record ProductLineRuntimeVariantRuntimeOutcomeSummary
{
    public string SchemaVersion { get; init; } = "product_line_runtime_variant_outcome_summary_v1";
    public string GoalId { get; init; } = ProductLineRuntimeVariantMatrixVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public string RecipeId { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string FinalInventorySummary { get; init; } = string.Empty;
    public string FinalCombatSummary { get; init; } = string.Empty;
    public string FinalQuestSummary { get; init; } = string.Empty;
    public string BaselineFinalStateHash { get; init; } = string.Empty;
    public bool RuntimeEffectObserved { get; init; }
    public bool RuntimeStateDistinctFromBaseline { get; init; }
    public bool CraftRequestPassed { get; init; }
    public bool HarvestRequestPassed { get; init; }
    public bool TransactionRequestPassed { get; init; }
    public bool CombatRequestPassed { get; init; }
    public bool RoundtripSemanticProofPassed { get; init; }
    public IReadOnlyList<string> ObservedRuntimeEffects { get; init; } = [];
}

public sealed record ProductLineRuntimeVariantScore
{
    public string SchemaVersion { get; init; } = "product_line_runtime_variant_candidate_score_v1";
    public string GoalId { get; init; } = ProductLineRuntimeVariantMatrixVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public string RecipeId { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public int Score { get; init; }
    public bool Eligible { get; init; }
    public int TieBreakPriority { get; init; }
    public string SelectionTieBreakOrder { get; init; } =
        "score desc, tieBreakPriority desc, recipeId asc, candidateId asc";
    public IReadOnlyList<ProductLineRuntimeVariantScoreComponent> ScoreBreakdown { get; init; } = [];
}

public sealed record ProductLineRuntimeVariantScoreComponent
{
    public string Name { get; init; } = string.Empty;
    public int Score { get; init; }
    public int MaxScore { get; init; }
    public bool Passed { get; init; }
    public string Explanation { get; init; } = string.Empty;
}

public sealed record ProductLineRuntimeVariantMatrixRow
{
    public string CandidateId { get; init; } = string.Empty;
    public string RecipeId { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public string PackagePath { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public ProductLineRuntimeVariantMutationAudit MutationAudit { get; init; } = new();
    public ProductLineRuntimeVariantPackageValidation PackageValidation { get; init; } = new();
    public ProductLineRuntimeVariantRuntimeOutcomeSummary RuntimeOutcomeSummary { get; init; } = new();
    public ProductLineRuntimeVariantScore CandidateScore { get; init; } = new();
    public bool Passed { get; init; }
}

public sealed record ProductLineRuntimeVariantMatrixResult
{
    public string SchemaVersion { get; init; } = "product_line_runtime_variant_matrix_result_v1";
    public string GoalId { get; init; } = ProductLineRuntimeVariantMatrixVocabulary.GoalId;
    public string MatrixStatus { get; init; } = "BLOCKED";
    public int CandidateCount { get; init; }
    public int PassedCandidateCount { get; init; }
    public int FailedCandidateCount { get; init; }
    public int RuntimeSignificantCandidateCount { get; init; }
    public int DistinctFinalStateHashCount { get; init; }
    public bool AllPackageHashesDistinct { get; init; }
    public bool AllMutationAuditsPassed { get; init; }
    public bool AllRoundtripSemanticProofsPassed { get; init; }
    public bool SourceTemplateUnmodified { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool Accepted { get; init; }
    public string SelectedCandidateId { get; init; } = string.Empty;
    public string SelectedVariantKind { get; init; } = string.Empty;
    public int SelectedScore { get; init; }
    public string NormalCommand { get; init; } = ProductLineRuntimeVariantMatrixVocabulary.NormalCommand;
    public string MatrixResultPath { get; init; } =
        ProductLineRuntimeVariantMatrixVocabulary.MatrixResultRelativePath;
    public string SelectedHandoffPath { get; init; } =
        ProductLineRuntimeVariantMatrixVocabulary.SelectedHandoffRelativePath;
    public IReadOnlyList<ProductLineRuntimeVariantMatrixRow> Candidates { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record ProductLineRuntimeVariantDistinctnessProof
{
    public string SchemaVersion { get; init; } = "product_line_runtime_variant_distinctness_proof_v1";
    public string GoalId { get; init; } = ProductLineRuntimeVariantMatrixVocabulary.GoalId;
    public int CandidateCount { get; init; }
    public int PassedCandidateCount { get; init; }
    public int FailedCandidateCount { get; init; }
    public int RuntimeSignificantCandidateCount { get; init; }
    public bool AllPackageHashesDistinct { get; init; }
    public bool AllMutationAuditsPassed { get; init; }
    public bool AllRoundtripSemanticProofsPassed { get; init; }
    public string BaselineFinalStateHash { get; init; } = string.Empty;
    public string AlchemyFinalStateHash { get; init; } = string.Empty;
    public string CombatFinalStateHash { get; init; } = string.Empty;
    public string ExplorationFinalStateHash { get; init; } = string.Empty;
    public int DistinctFinalStateHashCount { get; init; }
    public bool AlchemyRuntimeEffectObserved { get; init; }
    public bool CombatRuntimeEffectObserved { get; init; }
    public bool ExplorationRuntimeEffectObserved { get; init; }
    public bool NoMetadataOnlyVariantAccepted { get; init; }
    public bool SourceTemplateUnmodified { get; init; }
    public bool Passed { get; init; }
}

public sealed record ProductLineRuntimeVariantSelectedHandoff
{
    public string SchemaVersion { get; init; } = "selected_runtime_variant_handoff_v1";
    public string GoalId { get; init; } = ProductLineRuntimeVariantMatrixVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public string RecipeId { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public string PackagePath { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string RoundtripResultPath { get; init; } = string.Empty;
    public string RuntimeOutcomeSummaryPath { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public int Score { get; init; }
    public IReadOnlyList<ProductLineRuntimeVariantScoreComponent> ScoreBreakdown { get; init; } = [];
    public string SelectionReason { get; init; } = string.Empty;
    public bool RuntimeSignificant { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool Accepted { get; init; }
}

public sealed record ProductLineRuntimeVariantFileIndex
{
    public string SchemaVersion { get; init; } = "product_line_runtime_variant_file_index_v1";
    public string GoalId { get; init; } = ProductLineRuntimeVariantMatrixVocabulary.GoalId;
    public string RootPath { get; init; } = string.Empty;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<ProductLineRuntimeVariantFileIndexEntry> Files { get; init; } = [];
}

public sealed record ProductLineRuntimeVariantFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record ProductLineRuntimeVariantMatrixDashboard
{
    public string SchemaVersion { get; init; } = "product_line_runtime_variant_matrix_dashboard_v1";
    public string GoalId { get; init; } = ProductLineRuntimeVariantMatrixVocabulary.GoalId;
    public string MatrixStatus { get; init; } = "BLOCKED";
    public int CandidateCount { get; init; }
    public int PassedCandidateCount { get; init; }
    public int FailedCandidateCount { get; init; }
    public int RuntimeSignificantCandidateCount { get; init; }
    public int DistinctFinalStateHashCount { get; init; }
    public string SelectedCandidateId { get; init; } = string.Empty;
    public string SelectedVariantKind { get; init; } = string.Empty;
    public int SelectedScore { get; init; }
    public bool SourceTemplateUnmodified { get; init; }
    public string NormalCommand { get; init; } = ProductLineRuntimeVariantMatrixVocabulary.NormalCommand;
    public string MatrixResultPath { get; init; } =
        ProductLineRuntimeVariantMatrixVocabulary.MatrixResultRelativePath;
    public string SelectedHandoffPath { get; init; } =
        ProductLineRuntimeVariantMatrixVocabulary.SelectedHandoffRelativePath;
    public bool Accepted { get; init; }
}

public sealed record ProductLineRuntimeVariantMatrixWriteResult
{
    public ProductLineRuntimeVariantMatrixDashboard Dashboard { get; init; } = new();
    public ProductLineRuntimeVariantMatrixResult MatrixResult { get; init; } = new();
    public ProductLineRuntimeVariantDistinctnessProof DistinctnessProof { get; init; } = new();
    public ProductLineRuntimeVariantSelectedHandoff SelectedHandoff { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
