namespace LLMGameCreator.Application.Design.FullGeneratorVariabilityRegressionMatrix;

public static class FullGeneratorVariabilityMatrixVocabulary
{
    public const string GoalId = "goal_059_full_generator_variability_matrix";
    public const string MatrixId = "goal059";
    public const string ProductSmokeRoute = "goal-059-full-generator-variability-regression-matrix";
    public const string FinalGate = "full_generator_variability_regression_matrix_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-059-full-generator-variability-regression-matrix";
    public const string StagingRoot = "staging";
    public const string UnityMatrixCommandPlanStagingRelativePath = "matrix/unity-alpha-matrix-command-plan.json";

    public const string SourceManifestSchemaVersion = "full_generator_variability_matrix_source_manifest_v1";
    public const string SeedProfileMatrixSchemaVersion = "full_generator_variability_seed_profile_matrix_v1";
    public const string MatrixRowSchemaVersion = "full_generator_variability_matrix_row_v1";
    public const string VarianceMetricsSchemaVersion = "full_generator_variability_metrics_v1";
    public const string ReplayProofSchemaVersion = "full_generator_variability_replay_determinism_proof_v1";
    public const string ReviewPackageMatrixManifestSchemaVersion = "full_generator_variability_review_package_matrix_manifest_v1";
    public const string PreviewExportMatrixPayloadSchemaVersion = "full_generator_variability_preview_export_matrix_payload_v1";
    public const string UnityCommandPlanSchemaVersion = "full_generator_variability_unity_matrix_command_plan_v1";
    public const string UnityProofSchemaVersion = "full_generator_variability_unity_matrix_player_proof_v1";
    public const string InvalidMatrixSchemaVersion = "invalid_full_generator_variability_matrix_v1";
    public const string ReportSchemaVersion = "full_generator_variability_matrix_report_v1";

    public static readonly IReadOnlyList<string> FamilyIds =
    [
        "map_panel_rpg",
        "survival_sandbox",
        "first_person_grid_dungeon"
    ];

    public static readonly IReadOnlyList<string> SeedIds =
    [
        "seed_alpha",
        "seed_beta",
        "seed_gamma"
    ];

    public static readonly IReadOnlyList<string> RequiredInvalidScenarioIds =
    [
        "missing_goal058_source",
        "stale_mismatched_source_hash",
        "duplicate_row_id",
        "fake_family",
        "fake_seed",
        "missing_matrix_row",
        "identical_row_overfit",
        "nondeterministic_replay",
        "missing_unity_marker",
        "malformed_preview_export_payload",
        "unsafe_relative_path",
        "provider_network_llm_rag_claim",
        "gamepackage_schema_mutation_claim",
        "runtime_broad_mutation_claim",
        "ui_winforms_mutation_claim",
        "unity_broad_mutation_claim",
        "media_generation_import_claim",
        "lua_arbitrary_execution_claim"
    ];
}

public sealed record FullGeneratorVariabilityMatrixOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record FullGeneratorVariabilityDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static FullGeneratorVariabilityDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static FullGeneratorVariabilityDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static FullGeneratorVariabilityDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record FullGeneratorVariabilitySourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<FullGeneratorVariabilityDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullGeneratorVariabilityFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record FullGeneratorVariabilityGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record FullGeneratorVariabilityMediaRef
{
    public string BindingId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SlotId { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public string ReviewTrace { get; init; } = string.Empty;
}

public sealed record FullGeneratorVariabilityLoopCommandRef
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SecondaryTargetId { get; init; } = string.Empty;
    public string FamilyMarker { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = string.Empty;
    public string ExpectedPlayerMarker { get; init; } = string.Empty;
}

public sealed record FullGeneratorVariabilityFamilySource
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string FamilyRunRef { get; init; } = string.Empty;
    public string Goal057LoopProofRef { get; init; } = string.Empty;
    public string RuntimePreviewPayloadRef { get; init; } = string.Empty;
    public string ExportMode { get; init; } = string.Empty;
    public int CommandCount { get; init; }
    public int MediaFileCount { get; init; }
    public IReadOnlyList<string> ExpectedCampaignMarkers { get; init; } = [];
    public IReadOnlyList<FullGeneratorVariabilityMediaRef> MediaRefs { get; init; } = [];
    public IReadOnlyList<FullGeneratorVariabilityLoopCommandRef> LoopCommands { get; init; } = [];
}

public sealed record FullGeneratorVariabilitySourceBundle
{
    public string Goal058CampaignId { get; init; } = string.Empty;
    public bool Goal058ReportWasGreenProducedForReview { get; init; }
    public bool Goal058UnityProofPassed { get; init; }
    public string Goal058ReportMarkdown { get; init; } = string.Empty;
    public string SourceCampaignHash { get; init; } = string.Empty;
    public IReadOnlyList<FullGeneratorVariabilityFamilySource> Families { get; init; } = [];
    public IReadOnlyList<string> Goal058UnityMatchedMarkers { get; init; } = [];
    public IReadOnlyList<FullGeneratorVariabilityFilePayload> Goal058StagingFiles { get; init; } = [];
    public IReadOnlyList<FullGeneratorVariabilitySourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<FullGeneratorVariabilityDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullGeneratorVariabilitySourceManifest
{
    public string SchemaVersion { get; init; } = FullGeneratorVariabilityMatrixVocabulary.SourceManifestSchemaVersion;
    public string GoalId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = FullGeneratorVariabilityMatrixVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = FullGeneratorVariabilityMatrixVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal058AcceptedByUserHandoff { get; init; }
    public bool Goal058ReportWasGreenProducedForReview { get; init; }
    public bool Goal058UnityProofPassed { get; init; }
    public string SourceCampaignHash { get; init; } = string.Empty;
    public int SourceArtifactCount { get; init; }
    public int FamilyCount { get; init; }
    public IReadOnlyList<string> SelectedFamilyIds { get; init; } = [];
    public IReadOnlyList<FullGeneratorVariabilityGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<FullGeneratorVariabilitySourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<FullGeneratorVariabilityDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullGeneratorVariabilityDimension
{
    public string DimensionId { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string SourceRef { get; init; } = string.Empty;
}

public sealed record FullGeneratorVariabilityMatrixRow
{
    public string SchemaVersion { get; init; } = FullGeneratorVariabilityMatrixVocabulary.MatrixRowSchemaVersion;
    public string GoalId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.GoalId;
    public string MatrixId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.MatrixId;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourceCampaignId { get; init; } = string.Empty;
    public string SourceCampaignHash { get; init; } = string.Empty;
    public string DerivedCampaignHash { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceManifestRefs { get; init; } = [];
    public IReadOnlyList<string> SelectedWorldMapChunkRefs { get; init; } = [];
    public IReadOnlyList<string> SelectedMediaRefs { get; init; } = [];
    public IReadOnlyList<string> SelectedFamilyLoopRefs { get; init; } = [];
    public IReadOnlyList<string> SelectedPreviewExportRefs { get; init; } = [];
    public IReadOnlyList<string> DeterministicMarkerPlan { get; init; } = [];
    public IReadOnlyList<FullGeneratorVariabilityDimension> VariationDimensions { get; init; } = [];
    public string VarianceExplanation { get; init; } = string.Empty;
}

public sealed record FullGeneratorVariabilityMatrixRowSummary
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string DerivedCampaignHash { get; init; } = string.Empty;
    public int VariationDimensionCount { get; init; }
}

public sealed record FullGeneratorVariabilitySeedProfileMatrix
{
    public string SchemaVersion { get; init; } = FullGeneratorVariabilityMatrixVocabulary.SeedProfileMatrixSchemaVersion;
    public string GoalId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.GoalId;
    public string MatrixId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.MatrixId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<string> Families { get; init; } = [];
    public IReadOnlyList<string> Seeds { get; init; } = [];
    public IReadOnlyList<FullGeneratorVariabilityMatrixRowSummary> Rows { get; init; } = [];
}

public sealed record FullGeneratorVariabilityFamilyVarianceSummary
{
    public string FamilyId { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public int DistinctSeedCount { get; init; }
    public int DistinctDerivedHashCount { get; init; }
    public int MeaningfulVariationDimensionCount { get; init; }
    public IReadOnlyList<string> MeaningfulVariationDimensions { get; init; } = [];
}

public sealed record FullGeneratorVariabilityPairDifferenceSummary
{
    public string LeftRowId { get; init; } = string.Empty;
    public string RightRowId { get; init; } = string.Empty;
    public int DifferenceDimensionCount { get; init; }
    public IReadOnlyList<string> DifferenceDimensions { get; init; } = [];
}

public sealed record FullGeneratorVariabilityVarianceMetrics
{
    public string SchemaVersion { get; init; } = FullGeneratorVariabilityMatrixVocabulary.VarianceMetricsSchemaVersion;
    public string GoalId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int DistinctRowIdCount { get; init; }
    public int DistinctDerivedCampaignHashCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public bool MediaBindingCoveragePassed { get; init; }
    public int MediaBindingCoverageCount { get; init; }
    public bool FamilyLoopMarkerCoveragePassed { get; init; }
    public int FamilyLoopMarkerCoverageCount { get; init; }
    public int MinimumMeaningfulVariationDimensionsPerFamily { get; init; }
    public int OverfitWarningCount { get; init; }
    public IReadOnlyList<FullGeneratorVariabilityFamilyVarianceSummary> FamilySummaries { get; init; } = [];
    public IReadOnlyList<FullGeneratorVariabilityPairDifferenceSummary> PairDifferenceSummaries { get; init; } = [];
}

public sealed record FullGeneratorVariabilityReplayRowProof
{
    public string RowId { get; init; } = string.Empty;
    public string FirstHash { get; init; } = string.Empty;
    public string SecondHash { get; init; } = string.Empty;
    public bool JsonMatches { get; init; }
    public bool HashMatches { get; init; }
}

public sealed record FullGeneratorVariabilityReplayDeterminismProof
{
    public string SchemaVersion { get; init; } = FullGeneratorVariabilityMatrixVocabulary.ReplayProofSchemaVersion;
    public string GoalId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int MatchedRowCount { get; init; }
    public IReadOnlyList<FullGeneratorVariabilityReplayRowProof> Rows { get; init; } = [];
}

public sealed record FullGeneratorVariabilityReviewPackageMatrixManifest
{
    public string SchemaVersion { get; init; } = FullGeneratorVariabilityMatrixVocabulary.ReviewPackageMatrixManifestSchemaVersion;
    public string GoalId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = FullGeneratorVariabilityMatrixVocabulary.FinalGate;
    public string SourceReviewPackageManifestRef { get; init; } = string.Empty;
    public string MatrixCommandPlanRef { get; init; } = FullGeneratorVariabilityMatrixVocabulary.UnityMatrixCommandPlanStagingRelativePath;
    public int RowCount { get; init; }
    public IReadOnlyList<string> MatrixRowRefs { get; init; } = [];
    public IReadOnlyList<string> RequiredEvidenceFiles { get; init; } = [];
}

public sealed record FullGeneratorVariabilityPreviewExportMatrixRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PreviewPayloadRef { get; init; } = string.Empty;
    public string ExportMode { get; init; } = string.Empty;
    public string DerivedCampaignHash { get; init; } = string.Empty;
}

public sealed record FullGeneratorVariabilityPreviewExportMatrixPayload
{
    public string SchemaVersion { get; init; } = FullGeneratorVariabilityMatrixVocabulary.PreviewExportMatrixPayloadSchemaVersion;
    public string GoalId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<FullGeneratorVariabilityPreviewExportMatrixRow> Rows { get; init; } = [];
}

public sealed record FullGeneratorVariabilityUnityMatrixCommandRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string DerivedCampaignHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record FullGeneratorVariabilityUnityMatrixCommandPlan
{
    public string SchemaVersion { get; init; } = FullGeneratorVariabilityMatrixVocabulary.UnityCommandPlanSchemaVersion;
    public string GoalId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.GoalId;
    public string MatrixId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.MatrixId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = FullGeneratorVariabilityMatrixVocabulary.FinalGate;
    public IReadOnlyList<FullGeneratorVariabilityUnityMatrixCommandRow> Rows { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record FullGeneratorVariabilityUnityPlayerProof
{
    public string SchemaVersion { get; init; } = FullGeneratorVariabilityMatrixVocabulary.UnityProofSchemaVersion;
    public string GoalId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.GoalId;
    public string MatrixId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.MatrixId;
    public bool Passed { get; init; }
    public bool UnityEditorExecuted { get; init; }
    public bool PlayerExecuted { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public string UnityBuildLogRelativePath { get; init; } = string.Empty;
    public string LaunchLogRelativePath { get; init; } = string.Empty;
    public string PlayLoopLogRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<string> MatchedMarkers { get; init; } = [];
    public IReadOnlyList<string> MissingMarkers { get; init; } = [];
    public IReadOnlyList<FullGeneratorVariabilityDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullGeneratorVariabilityUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public FullGeneratorVariabilityUnityPlayerProof PlayerProof { get; init; } = new();
    public IReadOnlyList<FullGeneratorVariabilityDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidFullGeneratorVariabilityScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<FullGeneratorVariabilityDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidFullGeneratorVariabilityMatrix
{
    public string SchemaVersion { get; init; } = FullGeneratorVariabilityMatrixVocabulary.InvalidMatrixSchemaVersion;
    public string GoalId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<InvalidFullGeneratorVariabilityScenario> Scenarios { get; init; } = [];
}

public sealed record FullGeneratorVariabilityMatrixReport
{
    public string SchemaVersion { get; init; } = FullGeneratorVariabilityMatrixVocabulary.ReportSchemaVersion;
    public string GoalId { get; init; } = FullGeneratorVariabilityMatrixVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = FullGeneratorVariabilityMatrixVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = FullGeneratorVariabilityMatrixVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal058AcceptedByUserHandoff { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool MatrixRowsPassed { get; init; }
    public bool VarianceMetricsPassed { get; init; }
    public bool ReplayDeterminismPassed { get; init; }
    public bool ReviewPackageMatrixManifestPassed { get; init; }
    public bool PreviewExportMatrixPayloadPassed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public bool AllMatrixMarkersMatched { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public int MatrixRowCount { get; init; }
    public int DistinctDerivedCampaignHashCount { get; init; }
    public int OverfitWarningCount { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string SeedProfileMatrixHash { get; init; } = string.Empty;
    public string VarianceMetricsHash { get; init; } = string.Empty;
    public string ReplayProofHash { get; init; } = string.Empty;
    public string UnityPlayerProofHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<FullGeneratorVariabilityDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullGeneratorVariabilityEvidenceResult
{
    public FullGeneratorVariabilitySourceManifest SourceManifest { get; init; } = new();
    public FullGeneratorVariabilitySeedProfileMatrix SeedProfileMatrix { get; init; } = new();
    public IReadOnlyDictionary<string, FullGeneratorVariabilityMatrixRow> MatrixRowsByRowId { get; init; } = new SortedDictionary<string, FullGeneratorVariabilityMatrixRow>(StringComparer.Ordinal);
    public FullGeneratorVariabilityVarianceMetrics VarianceMetrics { get; init; } = new();
    public FullGeneratorVariabilityReplayDeterminismProof ReplayProof { get; init; } = new();
    public FullGeneratorVariabilityReviewPackageMatrixManifest ReviewPackageMatrixManifest { get; init; } = new();
    public FullGeneratorVariabilityPreviewExportMatrixPayload PreviewExportMatrixPayload { get; init; } = new();
    public FullGeneratorVariabilityUnityMatrixCommandPlan UnityCommandPlan { get; init; } = new();
    public FullGeneratorVariabilityUnityPlayerProof UnityPlayerProof { get; init; } = new();
    public InvalidFullGeneratorVariabilityMatrix InvalidMatrix { get; init; } = new();
    public FullGeneratorVariabilityMatrixReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<FullGeneratorVariabilityFilePayload> StagingFiles { get; init; } = [];
    public string ArtifactScopeReportMarkdown { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record FullGeneratorVariabilityWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public FullGeneratorVariabilityEvidenceResult Result { get; init; } = new();
}
