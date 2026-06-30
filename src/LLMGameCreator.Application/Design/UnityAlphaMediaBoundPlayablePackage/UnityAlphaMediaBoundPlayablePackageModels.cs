using LLMGameCreator.Application.Design.MediaBoundPlayableReviewPackage;

namespace LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;

public static class UnityAlphaMediaBoundPlayablePackageVocabulary
{
    public const string GoalId = "goal_056_unity_alpha_media_bound_playable_package";
    public const string ProductSmokeRoute = "goal-056-unity-alpha-media-bound-playable-package";
    public const string FinalGate = "unity_alpha_media_bound_playable_package_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package";
    public const string StagingRoot = "staging";
    public const string MediaBoundRoot = "media-bound";
    public const string UnityManifestRelativePath = "media-bound/unity-alpha-media-bound-manifest.json";

    public const string SourceManifestSchemaVersion = "unity_alpha_media_bound_source_manifest_v1";
    public const string StagingManifestSchemaVersion = "unity_alpha_media_bound_streamingassets_staging_manifest_v1";
    public const string FamilyPanelSchemaVersion = "unity_alpha_media_bound_family_panel_models_v1";
    public const string UnityLoadContractSchemaVersion = "unity_alpha_media_bound_load_contract_v1";
    public const string UnityLoadProofSchemaVersion = "unity_alpha_media_bound_load_proof_v1";
    public const string SmokeLogSummarySchemaVersion = "unity_alpha_media_bound_smoke_log_summary_v1";
    public const string PreviewExportPayloadSchemaVersion = "unity_alpha_media_bound_preview_export_payloads_v1";
    public const string HashInventorySchemaVersion = "unity_alpha_media_bound_hash_inventory_v1";
    public const string InvalidMatrixSchemaVersion = "invalid_unity_alpha_media_bound_matrix_v1";
    public const string ArtifactScopeReportSchemaVersion = "goal056_artifact_scope_report_v1";
    public const string ReportSchemaVersion = "unity_alpha_media_bound_playable_package_report_v1";

    public static readonly IReadOnlyList<string> FamilyIds = MediaBoundPlayableReviewPackageVocabulary.FamilyIds;

    public static readonly IReadOnlyList<string> RequiredInvalidScenarioIds =
    [
        "missing_goal055_source",
        "stale_goal055_hash",
        "missing_staged_png",
        "missing_staged_wav",
        "malformed_png",
        "malformed_wav",
        "unsafe_relative_path",
        "duplicate_media_binding_id",
        "fake_family_id",
        "fake_slot_id",
        "missing_unity_load_trace",
        "stale_unity_load_hash",
        "provider_network_llm_rag_claim",
        "lua_execution_claim",
        "gamepackage_schema_mutation_claim",
        "runtime_ui_broad_mutation_claim",
        "unity_broad_refactor_claim",
        "nondeterministic_ordering",
        "missing_review_provenance_trace"
    ];
}

public sealed record UnityAlphaMediaBoundOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record UnityAlphaMediaBoundDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static UnityAlphaMediaBoundDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static UnityAlphaMediaBoundDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static UnityAlphaMediaBoundDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record UnityAlphaMediaBoundSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
}

public sealed record UnityAlphaMediaBoundFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record UnityAlphaMediaBoundSourceBundle
{
    public MediaBoundSourceManifest Goal055SourceManifest { get; init; } = new();
    public MediaBoundReviewPackageManifest Goal055ReviewPackageManifest { get; init; } = new();
    public StreamingAssetsMediaManifest Goal055StreamingManifest { get; init; } = new();
    public UnityMediaLoadContract Goal055UnityLoadContract { get; init; } = new();
    public IReadOnlyList<UnityMediaLoadProof> Goal055UnityLoadProofs { get; init; } = [];
    public string Goal055ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyList<StagedMediaFileRecord> Goal055StagedFiles { get; init; } = [];
    public IReadOnlyList<UnityAlphaMediaBoundFilePayload> Goal055MediaFiles { get; init; } = [];
    public string BaseAlphaPayloadSourceRootRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<UnityAlphaMediaBoundFilePayload> BaseAlphaPayloadFiles { get; init; } = [];
    public IReadOnlyList<UnityAlphaMediaBoundSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<UnityAlphaMediaBoundDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityAlphaMediaBoundGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record UnityAlphaMediaBoundSourceManifest
{
    public string SchemaVersion { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.SourceManifestSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal055AcceptedByUserHandoff { get; init; }
    public bool Goal055ReportWasGreenProducedForReview { get; init; }
    public int Goal055PhysicalMediaFileCount { get; init; }
    public int Goal055PngFileCount { get; init; }
    public int Goal055WavFileCount { get; init; }
    public int Goal055BundleFileCount { get; init; }
    public string BaseAlphaPayloadSourceRoot { get; init; } = string.Empty;
    public bool BaseAlphaPayloadFound { get; init; }
    public IReadOnlyList<string> SelectedFamilyIds { get; init; } = [];
    public IReadOnlyList<UnityAlphaMediaBoundGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<UnityAlphaMediaBoundSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<UnityAlphaMediaBoundDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityAlphaMediaBoundBinding
{
    public string BindingId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SlotId { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string SourceGoal055RelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int SampleRate { get; init; }
    public int Channels { get; init; }
    public int SampleCount { get; init; }
    public bool SafeRelativePath { get; init; }
    public bool HashMatchesGoal055 { get; init; }
    public bool PngValid { get; init; }
    public bool WavValid { get; init; }
    public string ReviewTrace { get; init; } = string.Empty;
    public string DeterministicOrderingKey { get; init; } = string.Empty;
}

public sealed record UnityAlphaMediaBoundStagingManifest
{
    public string SchemaVersion { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.StagingManifestSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public string StagingRoot { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.StagingRoot;
    public string ManifestRelativePath { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.UnityManifestRelativePath;
    public int BasePayloadFileCount { get; init; }
    public int PhysicalMediaFileCount { get; init; }
    public int PngFileCount { get; init; }
    public int WavFileCount { get; init; }
    public int BundleFileCount { get; init; }
    public int FamilyCount { get; init; }
    public IReadOnlyList<UnityAlphaMediaBoundBinding> Bindings { get; init; } = [];
    public string DeterministicHash { get; init; } = string.Empty;
}

public sealed record UnityAlphaMediaBoundFamilyPanelModel
{
    public string FamilyId { get; init; } = string.Empty;
    public string PanelId { get; init; } = string.Empty;
    public string ImageBindingId { get; init; } = string.Empty;
    public string WavBindingId { get; init; } = string.Empty;
    public string BundleBindingId { get; init; } = string.Empty;
    public string PanelProofMarker { get; init; } = string.Empty;
    public IReadOnlyList<string> BindingIds { get; init; } = [];
}

public sealed record UnityAlphaMediaBoundFamilyPanelModels
{
    public string SchemaVersion { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.FamilyPanelSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public IReadOnlyList<UnityAlphaMediaBoundFamilyPanelModel> Families { get; init; } = [];
}

public sealed record UnityAlphaMediaBoundLoadContract
{
    public string SchemaVersion { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.UnityLoadContractSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public string ReadSurface { get; init; } = "Application.streamingAssetsPath";
    public string ManifestRelativePath { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.UnityManifestRelativePath;
    public IReadOnlyList<string> RequiredLogMarkers { get; init; } = [];
    public IReadOnlyList<UnityAlphaMediaBoundBinding> ExpectedBindings { get; init; } = [];
}

public sealed record UnityAlphaMediaBoundSmokeLogSummary
{
    public string SchemaVersion { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.SmokeLogSummarySchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.GoalId;
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
    public IReadOnlyList<UnityAlphaMediaBoundDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityAlphaMediaBoundLoadProof
{
    public string SchemaVersion { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.UnityLoadProofSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public bool ManifestLoadedByUnityProof { get; init; }
    public bool PngLoadProofPassed { get; init; }
    public bool WavLoadProofPassed { get; init; }
    public bool BundleProofPassed { get; init; }
    public bool HashValidationPassed { get; init; }
    public bool FamilyMediaPanelProofPassed { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public UnityAlphaMediaBoundSmokeLogSummary SmokeLogSummary { get; init; } = new();
    public IReadOnlyList<UnityAlphaMediaBoundDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityAlphaMediaBoundPreviewExportPayloadRecord
{
    public string FamilyId { get; init; } = string.Empty;
    public string PreviewPayloadId { get; init; } = string.Empty;
    public string ExportPayloadId { get; init; } = string.Empty;
    public string UnityManifestRef { get; init; } = string.Empty;
    public string PanelProofMarker { get; init; } = string.Empty;
    public IReadOnlyList<string> BindingIds { get; init; } = [];
}

public sealed record UnityAlphaMediaBoundPreviewExportPayloads
{
    public string SchemaVersion { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.PreviewExportPayloadSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public IReadOnlyList<UnityAlphaMediaBoundPreviewExportPayloadRecord> Payloads { get; init; } = [];
}

public sealed record UnityAlphaMediaBoundHashInventory
{
    public string SchemaVersion { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.HashInventorySchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FileCount { get; init; }
    public IReadOnlyList<UnityAlphaMediaBoundBinding> MediaFiles { get; init; } = [];
}

public sealed record InvalidUnityAlphaMediaBoundScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<UnityAlphaMediaBoundDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidUnityAlphaMediaBoundMatrix
{
    public string SchemaVersion { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.InvalidMatrixSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<InvalidUnityAlphaMediaBoundScenario> Scenarios { get; init; } = [];
}

public sealed record Goal056ArtifactScopeReport
{
    public string SchemaVersion { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.ArtifactScopeReportSchemaVersion;
    public string Scenario { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.ProductSmokeRoute;
    public bool Passed { get; init; }
    public IReadOnlyList<string> AllowedExactPaths { get; init; } = [];
    public IReadOnlyList<string> AllowedPathPrefixes { get; init; } = [];
}

public sealed record UnityAlphaMediaBoundPlayablePackageReport
{
    public string SchemaVersion { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.ReportSchemaVersion;
    public string GoalId { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = UnityAlphaMediaBoundPlayablePackageVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal055AcceptedByUserHandoff { get; init; }
    public bool StreamingAssetsPayloadStaged { get; init; }
    public int PhysicalMediaFileCount { get; init; }
    public bool PngLoadProofPassed { get; init; }
    public bool WavLoadProofPassed { get; init; }
    public bool BundleProofPassed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public bool UnityMediaLoadContractPassed { get; init; }
    public bool FamilyMediaPanelProofPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool UnitySourceChanged { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string StagingManifestHash { get; init; } = string.Empty;
    public string FamilyPanelModelsHash { get; init; } = string.Empty;
    public string UnityLoadContractHash { get; init; } = string.Empty;
    public string UnityLoadProofHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<UnityAlphaMediaBoundDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityAlphaMediaBoundEvidenceResult
{
    public UnityAlphaMediaBoundSourceManifest SourceManifest { get; init; } = new();
    public UnityAlphaMediaBoundStagingManifest StagingManifest { get; init; } = new();
    public UnityAlphaMediaBoundFamilyPanelModels FamilyPanelModels { get; init; } = new();
    public UnityAlphaMediaBoundLoadContract UnityLoadContract { get; init; } = new();
    public UnityAlphaMediaBoundLoadProof UnityLoadProof { get; init; } = new();
    public UnityAlphaMediaBoundSmokeLogSummary SmokeLogSummary { get; init; } = new();
    public UnityAlphaMediaBoundPreviewExportPayloads PreviewExportPayloads { get; init; } = new();
    public UnityAlphaMediaBoundHashInventory HashInventory { get; init; } = new();
    public InvalidUnityAlphaMediaBoundMatrix InvalidMatrix { get; init; } = new();
    public Goal056ArtifactScopeReport ArtifactScopeReport { get; init; } = new();
    public UnityAlphaMediaBoundPlayablePackageReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<UnityAlphaMediaBoundFilePayload> StagingFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record UnityAlphaMediaBoundWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public UnityAlphaMediaBoundEvidenceResult Result { get; init; } = new();
}
