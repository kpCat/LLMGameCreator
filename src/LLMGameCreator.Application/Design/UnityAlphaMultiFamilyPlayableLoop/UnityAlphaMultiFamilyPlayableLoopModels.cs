using LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;

namespace LLMGameCreator.Application.Design.UnityAlphaMultiFamilyPlayableLoop;

public static class UnityAlphaMultiFamilyPlayableLoopVocabulary
{
    public const string GoalId = "goal_057_unity_alpha_multifamily_playable_loop";
    public const string ProductSmokeRoute = "goal-057-unity-alpha-multifamily-playable-loop";
    public const string FinalGate = "unity_alpha_multifamily_playable_loop_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop";
    public const string StagingRoot = "staging";
    public const string FamilyCommandPlanStagingRelativePath = "family-loop/family-command-plan.json";

    public const string SourceManifestSchemaVersion = "unity_alpha_multifamily_source_manifest_v1";
    public const string FamilyModeManifestSchemaVersion = "unity_alpha_multifamily_family_mode_manifest_v1";
    public const string StagingManifestSchemaVersion = "unity_alpha_multifamily_staging_manifest_v1";
    public const string FamilyCommandPlanSchemaVersion = "unity_alpha_multifamily_command_plan_v1";
    public const string FamilyLoopProofSchemaVersion = "unity_alpha_multifamily_family_loop_proof_v1";
    public const string PlayerLogSummarySchemaVersion = "unity_alpha_multifamily_player_log_summary_v1";
    public const string MediaBindingValidationSchemaVersion = "unity_alpha_multifamily_media_binding_validation_v1";
    public const string PreviewExportPayloadSchemaVersion = "unity_alpha_multifamily_preview_export_payload_v1";
    public const string ReviewPackageManifestSchemaVersion = "unity_alpha_multifamily_review_package_manifest_v1";
    public const string InvalidMatrixSchemaVersion = "invalid_unity_alpha_multifamily_matrix_v1";
    public const string ReportSchemaVersion = "unity_alpha_multifamily_playable_loop_report_v1";

    public static readonly IReadOnlyList<string> FamilyIds = UnityAlphaMediaBoundPlayablePackageVocabulary.FamilyIds;

    public static readonly IReadOnlyList<string> RequiredInvalidScenarioIds =
    [
        "missing_goal056_source",
        "missing_media_manifest",
        "stale_hash_mismatched_media_file",
        "fake_family_id",
        "duplicate_family_mode_id",
        "missing_family_command_plan",
        "missing_player_marker",
        "fake_player_log",
        "malformed_png_wav_bundle_ref",
        "unsafe_relative_path",
        "provider_network_llm_rag_claim",
        "lua_execution_claim",
        "runtime_gamepackage_schema_mutation_claim",
        "broad_unity_mutation_claim",
        "nondeterministic_ordering",
        "missing_review_trace"
    ];
}

public sealed record UnityAlphaMultiFamilyOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record UnityAlphaMultiFamilyDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static UnityAlphaMultiFamilyDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static UnityAlphaMultiFamilyDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static UnityAlphaMultiFamilyDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record UnityAlphaMultiFamilySourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
}

public sealed record UnityAlphaMultiFamilyFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record UnityAlphaMultiFamilyGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record UnityAlphaMultiFamilyLoopCommand
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SecondaryTargetId { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string FamilyMarker { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = string.Empty;
    public string ExpectedPlayerMarker { get; init; } = string.Empty;
}

public sealed record UnityAlphaMultiFamilySourceFamily
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string DeterministicOrderingKey { get; init; } = string.Empty;
    public string Goal043PlanRelativePath { get; init; } = string.Empty;
    public string Goal043ProofRelativePath { get; init; } = string.Empty;
    public string Goal047DryRunRelativePath { get; init; } = string.Empty;
    public string RuntimePreviewPayloadRef { get; init; } = string.Empty;
    public string ExportMode { get; init; } = string.Empty;
    public bool StateChangingLoopProof { get; init; }
    public bool FamilySpecificMinimumsPassed { get; init; }
    public IReadOnlyList<string> SourceChangedMarkers { get; init; } = [];
    public IReadOnlyList<UnityAlphaMultiFamilyLoopCommand> LoopCommands { get; init; } = [];
}

public sealed record UnityAlphaMultiFamilySourceBundle
{
    public UnityAlphaMediaBoundSourceManifest Goal056SourceManifest { get; init; } = new();
    public UnityAlphaMediaBoundStagingManifest Goal056StagingManifest { get; init; } = new();
    public UnityAlphaMediaBoundLoadProof Goal056LoadProof { get; init; } = new();
    public UnityAlphaMediaBoundSmokeLogSummary Goal056SmokeLogSummary { get; init; } = new();
    public string Goal056ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyList<UnityAlphaMultiFamilySourceFamily> Families { get; init; } = [];
    public IReadOnlyList<UnityAlphaMultiFamilyFilePayload> Goal056StagingFiles { get; init; } = [];
    public IReadOnlyList<UnityAlphaMultiFamilySourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityAlphaMultiFamilySourceManifest
{
    public string SchemaVersion { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.SourceManifestSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal056AcceptedByUserHandoff { get; init; }
    public bool Goal056ReportWasGreenProducedForReview { get; init; }
    public bool Goal056UnityProofPassed { get; init; }
    public int FamilyCount { get; init; }
    public IReadOnlyList<string> SelectedFamilyIds { get; init; } = [];
    public IReadOnlyList<UnityAlphaMultiFamilyGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<UnityAlphaMultiFamilySourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityAlphaFamilyModeRecord
{
    public string FamilyId { get; init; } = string.Empty;
    public string ModeId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string SelectionArgument { get; init; } = string.Empty;
    public string RuntimePreviewPayloadRef { get; init; } = string.Empty;
    public string ExportMode { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceLoopRefs { get; init; } = [];
    public IReadOnlyList<string> StagedMediaBindingIds { get; init; } = [];
    public IReadOnlyList<string> VisiblePanelRecords { get; init; } = [];
    public IReadOnlyList<string> ExpectedMarkers { get; init; } = [];
}

public sealed record UnityAlphaFamilyModeManifest
{
    public string SchemaVersion { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyModeManifestSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public IReadOnlyList<UnityAlphaFamilyModeRecord> Families { get; init; } = [];
}

public sealed record UnityAlphaMultiFamilyStagingManifest
{
    public string SchemaVersion { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.StagingManifestSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.GoalId;
    public bool Passed { get; init; }
    public string StagingRoot { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.StagingRoot;
    public string FamilyCommandPlanRelativePath { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyCommandPlanStagingRelativePath;
    public string Goal056MediaManifestRelativePath { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.UnityManifestRelativePath;
    public int CopiedGoal056StagingFileCount { get; init; }
    public int PhysicalMediaFileCount { get; init; }
    public int PngFileCount { get; init; }
    public int WavFileCount { get; init; }
    public int BundleFileCount { get; init; }
    public int FamilyCount { get; init; }
    public string DeterministicHash { get; init; } = string.Empty;
}

public sealed record UnityAlphaFamilyCommandPlanMode
{
    public string FamilyId { get; init; } = string.Empty;
    public string ModeId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
}

public sealed record UnityAlphaFamilyCommandPlan
{
    public string SchemaVersion { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyCommandPlanSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.GoalId;
    public bool Passed { get; init; }
    public string ManualGate { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public IReadOnlyList<UnityAlphaFamilyCommandPlanMode> FamilyModes { get; init; } = [];
    public IReadOnlyList<UnityAlphaMultiFamilyLoopCommand> Commands { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record UnityAlphaFamilyLoopProof
{
    public string SchemaVersion { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyLoopProofSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.GoalId;
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public bool ScenarioLoaded { get; init; }
    public bool MediaManifestHashValidationPassed { get; init; }
    public bool ReviewPackageProofPassed { get; init; }
    public int LoopStepCount { get; init; }
    public IReadOnlyList<string> ExpectedMarkers { get; init; } = [];
    public IReadOnlyList<string> MatchedMarkers { get; init; } = [];
    public IReadOnlyList<string> MissingMarkers { get; init; } = [];
    public IReadOnlyList<UnityAlphaMultiFamilyLoopCommand> Commands { get; init; } = [];
    public IReadOnlyList<string> SourceChangedMarkers { get; init; } = [];
    public IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityAlphaMultiFamilyPlayerLogSummary
{
    public string SchemaVersion { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.PlayerLogSummarySchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.GoalId;
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
    public IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityAlphaMultiFamilyUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public UnityAlphaMultiFamilyPlayerLogSummary PlayerLogSummary { get; init; } = new();
    public IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityAlphaMultiFamilyMediaBindingValidation
{
    public string SchemaVersion { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.MediaBindingValidationSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public int MediaBindingCount { get; init; }
    public int PngFileCount { get; init; }
    public int WavFileCount { get; init; }
    public int BundleFileCount { get; init; }
    public bool HashValidationPassed { get; init; }
    public IReadOnlyList<UnityAlphaMediaBoundBinding> Bindings { get; init; } = [];
}

public sealed record UnityAlphaMultiFamilyPreviewExportRecord
{
    public string FamilyId { get; init; } = string.Empty;
    public string PreviewPayloadId { get; init; } = string.Empty;
    public string ExportPayloadId { get; init; } = string.Empty;
    public string RuntimePreviewPayloadRef { get; init; } = string.Empty;
    public string ExportMode { get; init; } = string.Empty;
    public string UnityManifestRef { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.UnityManifestRelativePath;
    public string FamilyCommandPlanRef { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyCommandPlanStagingRelativePath;
}

public sealed record UnityAlphaMultiFamilyPreviewExportPayload
{
    public string SchemaVersion { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.PreviewExportPayloadSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public IReadOnlyList<UnityAlphaMultiFamilyPreviewExportRecord> Payloads { get; init; } = [];
}

public sealed record UnityAlphaMultiFamilyReviewPackageManifest
{
    public string SchemaVersion { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.ReviewPackageManifestSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.FinalGate;
    public int FamilyCount { get; init; }
    public string StreamingAssetsPayloadRoot { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.StagingRoot;
    public string Goal056MediaManifestRef { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.UnityManifestRelativePath;
    public string FamilyCommandPlanRef { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyCommandPlanStagingRelativePath;
    public IReadOnlyList<string> RequiredEvidenceFiles { get; init; } = [];
}

public sealed record InvalidUnityAlphaMultiFamilyScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidUnityAlphaMultiFamilyMatrix
{
    public string SchemaVersion { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.InvalidMatrixSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<InvalidUnityAlphaMultiFamilyScenario> Scenarios { get; init; } = [];
}

public sealed record UnityAlphaMultiFamilyPlayableLoopReport
{
    public string SchemaVersion { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.ReportSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = UnityAlphaMultiFamilyPlayableLoopVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal056AcceptedByUserHandoff { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool UnityStagingExists { get; init; }
    public bool AllFamilyModesPresent { get; init; }
    public bool AllFamilyLoopsVerified { get; init; }
    public bool MediaBindingValidationPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string FamilyModeManifestHash { get; init; } = string.Empty;
    public string UnityStagingManifestHash { get; init; } = string.Empty;
    public string FamilyCommandPlanHash { get; init; } = string.Empty;
    public string PlayerLogSummaryHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<UnityAlphaMultiFamilyDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityAlphaMultiFamilyEvidenceResult
{
    public UnityAlphaMultiFamilySourceManifest SourceManifest { get; init; } = new();
    public UnityAlphaFamilyModeManifest FamilyModeManifest { get; init; } = new();
    public UnityAlphaMultiFamilyStagingManifest UnityStagingManifest { get; init; } = new();
    public UnityAlphaFamilyCommandPlan FamilyCommandPlan { get; init; } = new();
    public IReadOnlyDictionary<string, UnityAlphaFamilyLoopProof> FamilyLoopProofsByFamilyId { get; init; } = new SortedDictionary<string, UnityAlphaFamilyLoopProof>(StringComparer.Ordinal);
    public UnityAlphaMultiFamilyPlayerLogSummary PlayerLogSummary { get; init; } = new();
    public UnityAlphaMultiFamilyMediaBindingValidation MediaBindingValidation { get; init; } = new();
    public UnityAlphaMultiFamilyPreviewExportPayload PreviewExportPayload { get; init; } = new();
    public UnityAlphaMultiFamilyReviewPackageManifest ReviewPackageManifest { get; init; } = new();
    public InvalidUnityAlphaMultiFamilyMatrix InvalidMatrix { get; init; } = new();
    public UnityAlphaMultiFamilyPlayableLoopReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<UnityAlphaMultiFamilyFilePayload> StagingFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record UnityAlphaMultiFamilyWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public UnityAlphaMultiFamilyEvidenceResult Result { get; init; } = new();
}
