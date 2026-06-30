namespace LLMGameCreator.Application.Design.FullCampaignPlayableReviewPackageRc;

public static class FullCampaignPlayableReviewPackageRcVocabulary
{
    public const string GoalId = "goal_061_full_campaign_playable_review_package_rc";
    public const string ProductSmokeRoute = "goal-061-full-campaign-playable-review-package-rc";
    public const string ReviewPackageRcId = "goal061-review-package-rc";
    public const string FinalGate = "full_campaign_playable_review_package_rc_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-061-full-campaign-playable-review-package-rc";
    public const string Goal060RelativeOutputDirectory = ".llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix";
    public const string Goal059RelativeOutputDirectory = ".llmgc/procedural/goal-059-full-generator-variability-regression-matrix";
    public const string Goal058RelativeOutputDirectory = ".llmgc/procedural/goal-058-full-media-bound-generator-campaign";
    public const string Goal057RelativeOutputDirectory = ".llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop";
    public const string Goal056RelativeOutputDirectory = ".llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package";
    public const string Goal055RelativeOutputDirectory = ".llmgc/procedural/goal-055-media-bound-playable-review-package-smoke";
    public const string Goal054RelativeOutputDirectory = ".llmgc/procedural/goal-054-media-materialization-review-package";
    public const string StagingRoot = "staging";
    public const string ReviewPackageRoot = "review-package";
    public const string UnityReviewPackageCommandPlanStagingRelativePath = "review-package-rc/unity-player-command-plan.json";

    public const string SourceManifestSchemaVersion = "full_campaign_playable_review_package_rc_source_manifest_v1";
    public const string ReviewPackageManifestSchemaVersion = "full_campaign_playable_review_package_rc_manifest_v1";
    public const string FileInventorySchemaVersion = "full_campaign_playable_review_package_rc_file_inventory_v1";
    public const string RowSelectionMatrixSchemaVersion = "full_campaign_playable_review_package_rc_row_selection_matrix_v1";
    public const string UnityCommandPlanSchemaVersion = "full_campaign_playable_review_package_rc_unity_command_plan_v1";
    public const string UnityProofMatrixSchemaVersion = "full_campaign_playable_review_package_rc_unity_proof_matrix_v1";
    public const string MediaBindingAuditSchemaVersion = "full_campaign_playable_review_package_rc_media_binding_audit_v1";
    public const string SaveLoadReplayAuditSchemaVersion = "full_campaign_playable_review_package_rc_save_load_replay_audit_v1";
    public const string ScriptManifestSchemaVersion = "full_campaign_playable_review_package_rc_script_manifest_v1";
    public const string InvalidMatrixSchemaVersion = "invalid_full_campaign_playable_review_package_rc_matrix_v1";
    public const string ReportSchemaVersion = "full_campaign_playable_review_package_rc_report_v1";

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
        "missing_goal060_inventory",
        "stale_package_hash",
        "missing_package_file",
        "malformed_package_json",
        "fake_family_seed_package_row",
        "duplicate_row_id",
        "unsafe_relative_path_traversal",
        "missing_media_binding",
        "stale_media_hash",
        "fake_unity_proof_marker",
        "provider_llm_rag_media_generation_claim",
        "runtime_gamepackage_schema_broad_mutation_claim",
        "unity_broad_mutation_claim",
        "nondeterministic_row_order",
        "missing_review_trace",
        "script_path_escaping_review_package_root"
    ];
}

public sealed record FullCampaignPlayableReviewPackageRcOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record FullCampaignPlayableReviewPackageRcDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static FullCampaignPlayableReviewPackageRcDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static FullCampaignPlayableReviewPackageRcDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static FullCampaignPlayableReviewPackageRcDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record FullCampaignPlayableSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<FullCampaignPlayableReviewPackageRcDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullCampaignPlayableFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record FullCampaignPlayablePackageRowSource
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string SourcePackageRelativePath { get; init; } = string.Empty;
    public string ReviewPackageRelativePath { get; init; } = string.Empty;
    public string StagedUnityRelativePath { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string PackageFileHash { get; init; } = string.Empty;
    public string PackageJson { get; init; } = string.Empty;
    public bool ValidationPassed { get; init; }
    public bool PackageHashVerified { get; init; }
    public string Goal059RowHash { get; init; } = string.Empty;
    public string RuntimeLoopKind { get; init; } = string.Empty;
    public bool RuntimePassed { get; init; }
    public bool SaveLoadRoundtripPassed { get; init; }
    public IReadOnlyList<string> RuntimeChangedStateKeys { get; init; } = [];
    public IReadOnlyList<string> RuntimeCommandIds { get; init; } = [];
    public IReadOnlyList<string> RuntimeCommandTypes { get; init; } = [];
    public string PreviewPayloadRef { get; init; } = string.Empty;
    public string ExportPayloadRef { get; init; } = string.Empty;
}

public sealed record FullCampaignPlayableMediaBindingSource
{
    public string BindingId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SlotId { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public string SourceRelativePath { get; init; } = string.Empty;
    public string StreamingAssetsRelativePath { get; init; } = string.Empty;
    public string SourceSha256 { get; init; } = string.Empty;
    public string ActualSha256 { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string ReviewTrace { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
}

public sealed record FullCampaignPlayableSourceBundle
{
    public bool Goal060AcceptedByUserHandoff { get; init; }
    public bool Goal060ReportWasGreenProducedForReview { get; init; }
    public bool Goal060UnityProofPassed { get; init; }
    public bool Goal059MatrixConsumed { get; init; }
    public bool Goal058CampaignProofConsumed { get; init; }
    public bool MediaProofChainConsumed { get; init; }
    public IReadOnlyList<FullCampaignPlayablePackageRowSource> PackageRows { get; init; } = [];
    public IReadOnlyList<FullCampaignPlayableMediaBindingSource> MediaBindings { get; init; } = [];
    public IReadOnlyList<FullCampaignPlayableFilePayload> StagingFiles { get; init; } = [];
    public IReadOnlyList<FullCampaignPlayableSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<FullCampaignPlayableReviewPackageRcDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullCampaignPlayableGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record FullCampaignPlayableSourceManifest
{
    public string SchemaVersion { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.SourceManifestSchemaVersion;
    public string GoalId { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal060AcceptedByUserHandoff { get; init; }
    public bool Goal060ReportWasGreenProducedForReview { get; init; }
    public bool Goal060UnityProofPassed { get; init; }
    public bool Goal059MatrixConsumed { get; init; }
    public bool Goal058CampaignProofConsumed { get; init; }
    public bool MediaProofChainConsumed { get; init; }
    public int PackageRowCount { get; init; }
    public int MediaBindingCount { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<FullCampaignPlayableGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<FullCampaignPlayableSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<FullCampaignPlayableReviewPackageRcDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullCampaignPlayableReviewPackageRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public bool PackageHashVerified { get; init; }
    public bool PackageMediaBindingsVerified { get; init; }
    public bool RuntimeLoopPassed { get; init; }
    public bool SaveLoadReplayVerified { get; init; }
    public string ScenarioSummaryRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<string> CommandPlanSteps { get; init; } = [];
}

public sealed record FullCampaignPlayableReviewPackageRcManifest
{
    public string SchemaVersion { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.ReviewPackageManifestSchemaVersion;
    public string GoalId { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.GoalId;
    public string ReviewPackageRcId { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.ReviewPackageRcId;
    public string ManualGate { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.FinalGate;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public int PackageRowCount { get; init; }
    public int PhysicalPackageCount { get; init; }
    public int ScriptCount { get; init; }
    public int ScenarioSummaryCount { get; init; }
    public bool SourceChainConsumed { get; init; }
    public bool PackageHashesVerified { get; init; }
    public bool MediaBindingsVerified { get; init; }
    public bool SaveLoadReplayTiedToPackageRows { get; init; }
    public IReadOnlyList<FullCampaignPlayableReviewPackageRow> Rows { get; init; } = [];
    public IReadOnlyList<string> ReviewPackageFiles { get; init; } = [];
}

public sealed record FullCampaignPlayableReviewPackageFileEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string ArtifactKind { get; init; } = string.Empty;
}

public sealed record FullCampaignPlayableReviewPackageFileInventory
{
    public string SchemaVersion { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.FileInventorySchemaVersion;
    public string GoalId { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FileCount { get; init; }
    public IReadOnlyList<FullCampaignPlayableReviewPackageFileEntry> Files { get; init; } = [];
}

public sealed record FullCampaignPlayablePackageRowSelectionMatrix
{
    public string SchemaVersion { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.RowSelectionMatrixSchemaVersion;
    public string GoalId { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<FullCampaignPlayableReviewPackageRow> Rows { get; init; } = [];
}

public sealed record FullCampaignPlayableUnityCommandRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public bool PackageHashVerified { get; init; }
    public bool PackageMediaBindingsVerified { get; init; }
    public bool SaveLoadReplayVerified { get; init; }
    public IReadOnlyList<string> OrderedStepIds { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record FullCampaignPlayableUnityCommandPlan
{
    public string SchemaVersion { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.UnityCommandPlanSchemaVersion;
    public string GoalId { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.GoalId;
    public string ReviewPackageRcId { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.ReviewPackageRcId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.FinalGate;
    public IReadOnlyList<FullCampaignPlayableUnityCommandRow> Rows { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record FullCampaignPlayableUnityPlayerProof
{
    public string SchemaVersion { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.UnityProofMatrixSchemaVersion;
    public string GoalId { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool UnityEditorExecuted { get; init; }
    public bool PlayerExecuted { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public string UnityBuildLogRelativePath { get; init; } = string.Empty;
    public string LaunchLogRelativePath { get; init; } = string.Empty;
    public string PlayLoopLogRelativePath { get; init; } = string.Empty;
    public int ProvenRowCount { get; init; }
    public IReadOnlyList<string> MatchedMarkers { get; init; } = [];
    public IReadOnlyList<string> MissingMarkers { get; init; } = [];
    public IReadOnlyList<FullCampaignPlayableReviewPackageRcDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullCampaignPlayableUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public FullCampaignPlayableUnityPlayerProof PlayerProof { get; init; } = new();
    public IReadOnlyList<FullCampaignPlayableReviewPackageRcDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullCampaignPlayablePackageMediaBindingAuditRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public bool PackageMediaBindingsVerified { get; init; }
    public int BindingCount { get; init; }
    public IReadOnlyList<string> BindingIds { get; init; } = [];
}

public sealed record FullCampaignPlayablePackageMediaBindingAudit
{
    public string SchemaVersion { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.MediaBindingAuditSchemaVersion;
    public string GoalId { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<FullCampaignPlayablePackageMediaBindingAuditRow> Rows { get; init; } = [];
}

public sealed record FullCampaignPlayableSaveLoadReplayPackageRowAuditRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public bool SaveLoadRoundtripPassed { get; init; }
    public bool ReplayDeterminismPassed { get; init; }
    public bool PreviewExportPayloadConsistent { get; init; }
    public IReadOnlyList<string> RuntimeCommandIds { get; init; } = [];
}

public sealed record FullCampaignPlayableSaveLoadReplayPackageRowAudit
{
    public string SchemaVersion { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.SaveLoadReplayAuditSchemaVersion;
    public string GoalId { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<FullCampaignPlayableSaveLoadReplayPackageRowAuditRow> Rows { get; init; } = [];
}

public sealed record FullCampaignPlayableSmokeScriptManifest
{
    public string SchemaVersion { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.ScriptManifestSchemaVersion;
    public string GoalId { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.GoalId;
    public bool Passed { get; init; }
    public IReadOnlyList<FullCampaignPlayableReviewPackageFileEntry> Scripts { get; init; } = [];
}

public sealed record InvalidFullCampaignPlayableReviewPackageScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<FullCampaignPlayableReviewPackageRcDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidFullCampaignPlayableReviewPackageMatrix
{
    public string SchemaVersion { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.InvalidMatrixSchemaVersion;
    public string GoalId { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<InvalidFullCampaignPlayableReviewPackageScenario> Scenarios { get; init; } = [];
}

public sealed record FullCampaignPlayableReviewPackageRcReport
{
    public string SchemaVersion { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.ReportSchemaVersion;
    public string GoalId { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = FullCampaignPlayableReviewPackageRcVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal060AcceptedByUserHandoff { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool ReviewPackageManifestPassed { get; init; }
    public bool FileInventoryPassed { get; init; }
    public bool PackageRowSelectionMatrixPassed { get; init; }
    public bool PackageMediaBindingAuditPassed { get; init; }
    public bool SaveLoadReplayAuditPassed { get; init; }
    public bool ScriptManifestPassed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public bool AllUnityReviewPackageMarkersMatched { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public int PackageRowCount { get; init; }
    public int PhysicalPackageCount { get; init; }
    public int UnityProvenRowCount { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string ReviewPackageManifestHash { get; init; } = string.Empty;
    public string FileInventoryHash { get; init; } = string.Empty;
    public string PackageRowSelectionMatrixHash { get; init; } = string.Empty;
    public string UnityCommandPlanHash { get; init; } = string.Empty;
    public string UnityProofMatrixHash { get; init; } = string.Empty;
    public string PackageMediaBindingAuditHash { get; init; } = string.Empty;
    public string SaveLoadReplayAuditHash { get; init; } = string.Empty;
    public string ScriptManifestHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<FullCampaignPlayableReviewPackageRcDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullCampaignPlayableReviewPackageRcEvidenceResult
{
    public FullCampaignPlayableSourceManifest SourceManifest { get; init; } = new();
    public FullCampaignPlayableReviewPackageRcManifest ReviewPackageManifest { get; init; } = new();
    public FullCampaignPlayableReviewPackageFileInventory FileInventory { get; init; } = new();
    public FullCampaignPlayablePackageRowSelectionMatrix PackageRowSelectionMatrix { get; init; } = new();
    public FullCampaignPlayableUnityCommandPlan UnityCommandPlan { get; init; } = new();
    public FullCampaignPlayableUnityPlayerProof UnityPlayerProof { get; init; } = new();
    public FullCampaignPlayablePackageMediaBindingAudit PackageMediaBindingAudit { get; init; } = new();
    public FullCampaignPlayableSaveLoadReplayPackageRowAudit SaveLoadReplayAudit { get; init; } = new();
    public FullCampaignPlayableSmokeScriptManifest ScriptManifest { get; init; } = new();
    public InvalidFullCampaignPlayableReviewPackageMatrix InvalidMatrix { get; init; } = new();
    public FullCampaignPlayableReviewPackageRcReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<FullCampaignPlayableFilePayload> ReviewPackageFiles { get; init; } = [];
    public IReadOnlyList<FullCampaignPlayableFilePayload> StagingFiles { get; init; } = [];
    public string ManualReviewChecklistMarkdown { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record FullCampaignPlayableReviewPackageRcWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public FullCampaignPlayableReviewPackageRcEvidenceResult Result { get; init; } = new();
}
