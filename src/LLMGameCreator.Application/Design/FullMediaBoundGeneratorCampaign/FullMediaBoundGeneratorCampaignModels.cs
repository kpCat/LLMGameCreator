namespace LLMGameCreator.Application.Design.FullMediaBoundGeneratorCampaign;

public static class FullMediaBoundGeneratorCampaignVocabulary
{
    public const string GoalId = "goal_058_full_media_bound_generator_campaign";
    public const string CampaignId = "goal058";
    public const string ProductSmokeRoute = "goal-058-full-media-bound-generator-campaign";
    public const string FinalGate = "full_media_bound_generator_campaign_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-058-full-media-bound-generator-campaign";
    public const string StagingRoot = "staging";
    public const string CampaignManifestStagingRelativePath = "campaign/full-media-bound-campaign-manifest.json";
    public const string CampaignCommandPlanStagingRelativePath = "campaign/family-command-plan.json";

    public const string SourceManifestSchemaVersion = "full_media_bound_campaign_source_manifest_v1";
    public const string CampaignPlanSchemaVersion = "full_media_bound_campaign_plan_v1";
    public const string FamilyRunSchemaVersion = "full_media_bound_campaign_family_run_v1";
    public const string ReviewPackageSchemaVersion = "full_media_bound_campaign_review_package_manifest_v1";
    public const string UnityCommandPlanSchemaVersion = "full_media_bound_campaign_unity_command_plan_v1";
    public const string UnityProofSchemaVersion = "full_media_bound_campaign_unity_player_proof_v1";
    public const string PreviewExportSchemaVersion = "full_media_bound_campaign_preview_export_payload_v1";
    public const string PackageCompatibilitySchemaVersion = "full_media_bound_campaign_package_compatibility_proof_v1";
    public const string InvalidMatrixSchemaVersion = "invalid_full_media_bound_campaign_matrix_v1";
    public const string ReportSchemaVersion = "full_media_bound_campaign_report_v1";

    public static readonly IReadOnlyList<string> FamilyIds =
    [
        "map_panel_rpg",
        "survival_sandbox",
        "first_person_grid_dungeon"
    ];

    public static readonly IReadOnlyList<string> StageIds =
    [
        "strict_draft_quarantined_candidate_source_facts",
        "lua_manifest_sandbox_expansion_source_facts",
        "world_region_chunk_runtime_delta_source_facts",
        "family_simulatable_loop_source_facts",
        "full_generator_without_media_dry_run_source_facts",
        "media_materialization_review_package_source_facts",
        "unity_alpha_media_bound_package_source_facts",
        "unity_alpha_multifamily_playable_loop_source_facts",
        "campaign_review_package_plan",
        "campaign_unity_player_command_plan",
        "campaign_preview_export_payload"
    ];

    public static readonly IReadOnlyList<string> RequiredInvalidScenarioIds =
    [
        "missing_goal057_source",
        "stale_source_hash",
        "fake_family_id",
        "missing_family_command_plan",
        "missing_media_file",
        "media_hash_mismatch",
        "missing_unity_marker",
        "duplicate_campaign_id",
        "unsafe_relative_path",
        "provider_network_llm_rag_claim",
        "real_media_generation_claim",
        "lua_arbitrary_execution_claim",
        "runtime_ui_gamepackage_schema_mutation_claim",
        "unity_broad_mutation_claim",
        "nondeterministic_order",
        "missing_review_trace",
        "self_promotion_without_validation"
    ];
}

public sealed record FullMediaBoundGeneratorCampaignOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record FullMediaBoundCampaignDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static FullMediaBoundCampaignDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static FullMediaBoundCampaignDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static FullMediaBoundCampaignDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record FullMediaBoundCampaignSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<FullMediaBoundCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullMediaBoundCampaignFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record FullMediaBoundCampaignGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record FullMediaBoundCampaignLoopCommand
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string CommandType { get; init; } = string.Empty;
    public string FamilyMarker { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = string.Empty;
    public string ExpectedPlayerMarker { get; init; } = string.Empty;
}

public sealed record FullMediaBoundCampaignFamilySource
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string Goal047DryRunRef { get; init; } = string.Empty;
    public string Goal057LoopProofRef { get; init; } = string.Empty;
    public string RuntimePreviewPayloadRef { get; init; } = string.Empty;
    public string ExportMode { get; init; } = string.Empty;
    public int MediaFileCount { get; init; }
    public IReadOnlyList<FullMediaBoundCampaignLoopCommand> LoopCommands { get; init; } = [];
}

public sealed record FullMediaBoundCampaignSourceBundle
{
    public bool Goal057ReportWasGreenProducedForReview { get; init; }
    public bool Goal057UnityProofPassed { get; init; }
    public string Goal057ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyList<FullMediaBoundCampaignFamilySource> Families { get; init; } = [];
    public IReadOnlyList<FullMediaBoundCampaignFilePayload> Goal057StagingFiles { get; init; } = [];
    public IReadOnlyList<FullMediaBoundCampaignSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<FullMediaBoundCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullMediaBoundCampaignStageRecord
{
    public string StageId { get; init; } = string.Empty;
    public int Order { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> SourceGoals { get; init; } = [];
    public IReadOnlyList<string> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<FullMediaBoundCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullMediaBoundCampaignSourceManifest
{
    public string SchemaVersion { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.SourceManifestSchemaVersion;
    public string GoalId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal057AcceptedByUserHandoff { get; init; }
    public bool Goal057ReportWasGreenProducedForReview { get; init; }
    public bool Goal057UnityProofPassed { get; init; }
    public int SourceArtifactCount { get; init; }
    public int FamilyCount { get; init; }
    public IReadOnlyList<string> SelectedFamilyIds { get; init; } = [];
    public IReadOnlyList<FullMediaBoundCampaignGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<FullMediaBoundCampaignSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<FullMediaBoundCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullMediaBoundCampaignPlan
{
    public string SchemaVersion { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.CampaignPlanSchemaVersion;
    public string GoalId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.GoalId;
    public string CampaignId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.CampaignId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public int FamilyCount { get; init; }
    public int StageCount { get; init; }
    public IReadOnlyList<string> SeedProfileFamilySet { get; init; } = [];
    public IReadOnlyList<FullMediaBoundCampaignStageRecord> Stages { get; init; } = [];
}

public sealed record FullMediaBoundCampaignFamilyRun
{
    public string SchemaVersion { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.FamilyRunSchemaVersion;
    public string GoalId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.GoalId;
    public string CampaignId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.CampaignId;
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int CommandCount { get; init; }
    public int MediaFileCount { get; init; }
    public string RuntimePreviewPayloadRef { get; init; } = string.Empty;
    public string ExportMode { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceRefs { get; init; } = [];
    public IReadOnlyList<string> ExpectedCampaignMarkers { get; init; } = [];
}

public sealed record FullMediaBoundReviewPackageManifest
{
    public string SchemaVersion { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.ReviewPackageSchemaVersion;
    public string GoalId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.GoalId;
    public string CampaignId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.CampaignId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.FinalGate;
    public string StreamingAssetsRoot { get; init; } = "review-package/StreamingAssets";
    public IReadOnlyList<string> StreamingAssetsFiles { get; init; } = [];
    public IReadOnlyList<string> RequiredEvidenceFiles { get; init; } = [];
}

public sealed record FullMediaBoundUnityCampaignCommandPlan
{
    public string SchemaVersion { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.UnityCommandPlanSchemaVersion;
    public string GoalId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.GoalId;
    public string CampaignId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.CampaignId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.FinalGate;
    public IReadOnlyList<string> Families { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record FullMediaBoundCampaignPlayerProof
{
    public string SchemaVersion { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.UnityProofSchemaVersion;
    public string GoalId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.GoalId;
    public string CampaignId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.CampaignId;
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
    public IReadOnlyList<FullMediaBoundCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullMediaBoundCampaignUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public FullMediaBoundCampaignPlayerProof PlayerProof { get; init; } = new();
    public IReadOnlyList<FullMediaBoundCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PreviewExportCampaignPayload
{
    public string SchemaVersion { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.PreviewExportSchemaVersion;
    public string GoalId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.GoalId;
    public string CampaignId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.CampaignId;
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public IReadOnlyList<string> PreviewRefs { get; init; } = [];
    public IReadOnlyList<string> ExportModes { get; init; } = [];
}

public sealed record CampaignPackageCompatibilityProof
{
    public string SchemaVersion { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.PackageCompatibilitySchemaVersion;
    public string GoalId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.GoalId;
    public string CampaignId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.CampaignId;
    public bool Passed { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool RuntimeSourceChanged { get; init; }
    public bool WinFormsUiChanged { get; init; }
    public IReadOnlyList<string> CompatibilityRefs { get; init; } = [];
}

public sealed record InvalidFullMediaBoundCampaignScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<FullMediaBoundCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidFullMediaBoundCampaignMatrix
{
    public string SchemaVersion { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.InvalidMatrixSchemaVersion;
    public string GoalId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<InvalidFullMediaBoundCampaignScenario> Scenarios { get; init; } = [];
}

public sealed record FullMediaBoundGeneratorCampaignReport
{
    public string SchemaVersion { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.ReportSchemaVersion;
    public string GoalId { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = FullMediaBoundGeneratorCampaignVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal057AcceptedByUserHandoff { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool AllFamiliesIncluded { get; init; }
    public bool CampaignRunnerExecuted { get; init; }
    public bool ReviewPackageManifestPassed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public bool AllCampaignMarkersMatched { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string CampaignPlanHash { get; init; } = string.Empty;
    public string UnityPlayerProofHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<FullMediaBoundCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullMediaBoundCampaignEvidenceResult
{
    public FullMediaBoundCampaignSourceManifest SourceManifest { get; init; } = new();
    public FullMediaBoundCampaignPlan CampaignPlan { get; init; } = new();
    public IReadOnlyDictionary<string, FullMediaBoundCampaignFamilyRun> FamilyRunsByFamilyId { get; init; } = new SortedDictionary<string, FullMediaBoundCampaignFamilyRun>(StringComparer.Ordinal);
    public FullMediaBoundReviewPackageManifest ReviewPackageManifest { get; init; } = new();
    public FullMediaBoundUnityCampaignCommandPlan UnityCommandPlan { get; init; } = new();
    public FullMediaBoundCampaignPlayerProof UnityPlayerProof { get; init; } = new();
    public PreviewExportCampaignPayload PreviewExportPayload { get; init; } = new();
    public CampaignPackageCompatibilityProof PackageCompatibilityProof { get; init; } = new();
    public InvalidFullMediaBoundCampaignMatrix InvalidMatrix { get; init; } = new();
    public FullMediaBoundGeneratorCampaignReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<FullMediaBoundCampaignFilePayload> StagingFiles { get; init; } = [];
    public IReadOnlyList<FullMediaBoundCampaignFilePayload> ReviewPackageFiles { get; init; } = [];
    public string ArtifactScopeReportMarkdown { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record FullMediaBoundCampaignWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public FullMediaBoundCampaignEvidenceResult Result { get; init; } = new();
}
