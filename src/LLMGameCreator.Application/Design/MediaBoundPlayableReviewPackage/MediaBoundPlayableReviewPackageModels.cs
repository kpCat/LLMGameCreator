using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;
using LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;
using LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;

namespace LLMGameCreator.Application.Design.MediaBoundPlayableReviewPackage;

public static class MediaBoundPlayableReviewPackageVocabulary
{
    public const string GoalId = "goal_055_media_bound_playable_review_package_smoke";
    public const string ProductSmokeRoute = "goal-055-media-bound-playable-review-package-smoke";
    public const string FinalGate = "media_bound_playable_review_package_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-055-media-bound-playable-review-package-smoke";
    public const string ReviewPackageRoot = "review-package";
    public const string StreamingAssetsRoot = "review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound";
    public const string StreamingAssetsMediaRoot = StreamingAssetsRoot + "/media";

    public const string SourceManifestSchemaVersion = "media_bound_source_manifest_v1";
    public const string ReviewPackageManifestSchemaVersion = "media_bound_review_package_manifest_v1";
    public const string StreamingAssetsManifestSchemaVersion = "streaming_assets_media_manifest_v1";
    public const string PreviewPayloadsSchemaVersion = "media_bound_preview_payloads_v1";
    public const string UnityLoadContractSchemaVersion = "unity_media_load_contract_v1";
    public const string UnityLoadProofSchemaVersion = "unity_media_load_proof_v1";
    public const string FamilySmokeMatrixSchemaVersion = "media_bound_family_smoke_matrix_v1";
    public const string InvalidMatrixSchemaVersion = "invalid_media_bound_package_diagnostics_matrix_v1";
    public const string ArtifactScopeReportSchemaVersion = "goal055_artifact_scope_report_v1";
    public const string ReportSchemaVersion = "media_bound_playable_review_package_report_v1";

    public static readonly IReadOnlyList<string> FamilyIds = MediaMaterializationReviewPackageVocabulary.FamilyIds;

    public static readonly IReadOnlyList<string> RequiredInvalidScenarioIds =
    [
        "missing_goal054_source",
        "missing_staged_file",
        "stale_hash",
        "malformed_png",
        "malformed_wav",
        "unsafe_relative_path",
        "duplicate_binding_id",
        "fake_family_id",
        "fake_slot_id",
        "license_provenance_blocked_promoted",
        "provider_network_llm_rag_claim",
        "lua_execution_claim",
        "runtime_ui_gamepackage_schema_mutation_claim",
        "unity_broad_mutation_claim",
        "nondeterministic_ordering",
        "missing_review_trace",
        "fake_unity_proof_line"
    ];
}

public sealed record MediaBoundBoundaryClaims
{
    public bool ProviderCalls { get; init; }
    public bool NetworkImports { get; init; }
    public bool LlmCalls { get; init; }
    public bool RagCalls { get; init; }
    public bool LuaExecuted { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool RuntimeSourceChanged { get; init; }
    public bool RuntimeAbstractionsChanged { get; init; }
    public bool WinFormsUiChanged { get; init; }
    public bool ProviderPathChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool UnityBroadMutationClaim { get; init; }
    public bool UnitySourceChanged { get; init; }
    public bool ExternalDependencyAdded { get; init; }

    [JsonIgnore]
    public bool AllFalse =>
        !ProviderCalls &&
        !NetworkImports &&
        !LlmCalls &&
        !RagCalls &&
        !LuaExecuted &&
        !PublicGamePackageSchemaChanged &&
        !RuntimeSourceChanged &&
        !RuntimeAbstractionsChanged &&
        !WinFormsUiChanged &&
        !ProviderPathChanged &&
        !GeneratorLibraryChanged &&
        !UnityBroadMutationClaim &&
        !UnitySourceChanged &&
        !ExternalDependencyAdded;
}

public sealed record MediaBoundDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static MediaBoundDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static MediaBoundDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static MediaBoundDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record PngValidationResult
{
    public bool SignatureValid { get; init; }
    public bool ChunkCrcsValid { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }

    [JsonIgnore]
    public bool Passed => SignatureValid && ChunkCrcsValid && Width > 0 && Height > 0;
}

public sealed record WavValidationResult
{
    public bool HeaderValid { get; init; }
    public int SampleRate { get; init; }
    public int Channels { get; init; }
    public int BitsPerSample { get; init; }
    public int SampleCount { get; init; }

    [JsonIgnore]
    public bool Passed => HeaderValid && SampleRate > 0 && Channels > 0 && BitsPerSample == 16 && SampleCount > 0;
}

public sealed record MediaBoundSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactFileName { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
}

public sealed record Goal054PhysicalMediaSource
{
    public MaterializedMediaFileRecord InventoryRecord { get; init; } = new();
    public string SourceRelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
    public string ActualSha256 { get; init; } = string.Empty;
}

public sealed record MediaBoundSourceBundle
{
    public FullGeneratorDryRunManifest Goal047SourceManifest { get; init; } = new();
    public IReadOnlyList<FullGeneratorFamilyDryRunRecord> Goal047FamilyDryRuns { get; init; } = [];
    public MediaCampaignSourceManifest Goal053SourceManifest { get; init; } = new();
    public MediaBindingManifest Goal053BindingManifest { get; init; } = new();
    public MediaLicenseProvenanceLedger Goal053LicenseLedger { get; init; } = new();
    public MediaMaterializationSourceManifest Goal054SourceManifest { get; init; } = new();
    public MaterializedMediaInventory Goal054Inventory { get; init; } = new();
    public MediaProvenanceLicenseLedger Goal054LicenseLedger { get; init; } = new();
    public MediaBindingValidation Goal054BindingValidation { get; init; } = new();
    public MediaReviewPackageManifest Goal054ReviewPackageManifest { get; init; } = new();
    public LLMGameCreator.Application.Design.MediaMaterializationReviewPackage.PreviewExportMediaPayloads Goal054PreviewPayloads { get; init; } = new();
    public string Goal054ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyList<Goal054PhysicalMediaSource> Goal054PhysicalMediaFiles { get; init; } = [];
    public IReadOnlyList<MediaBoundSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<MediaBoundDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaBoundGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record MediaBoundFamilySourceRecord
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string DryRunArtifactRef { get; init; } = string.Empty;
    public string Goal054PreviewPayloadRef { get; init; } = string.Empty;
    public int Goal054PhysicalMediaCount { get; init; }
    public int Goal054PngCount { get; init; }
    public int Goal054WavCount { get; init; }
    public int Goal054BundleJsonCount { get; init; }
}

public sealed record MediaBoundSourceManifest
{
    public string SchemaVersion { get; init; } = MediaBoundPlayableReviewPackageVocabulary.SourceManifestSchemaVersion;
    public string GoalId { get; init; } = MediaBoundPlayableReviewPackageVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = MediaBoundPlayableReviewPackageVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = MediaBoundPlayableReviewPackageVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal054AcceptedByUserHandoff { get; init; }
    public bool Goal054ReportWasGreenProducedForReview { get; init; }
    public int Goal047FamilyDryRunCount { get; init; }
    public int Goal053BindingCount { get; init; }
    public int Goal054PhysicalMediaCount { get; init; }
    public int Goal054PngCount { get; init; }
    public int Goal054WavCount { get; init; }
    public int Goal054BundleJsonCount { get; init; }
    public IReadOnlyList<MediaBoundGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<string> SelectedFamilyIds { get; init; } = [];
    public IReadOnlyList<MediaBoundFamilySourceRecord> Families { get; init; } = [];
    public IReadOnlyList<MediaBoundSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public MediaBoundBoundaryClaims BoundaryClaims { get; init; } = new();
    public IReadOnlyList<MediaBoundDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record StagedMediaFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record StagedMediaFileRecord
{
    public string StagingId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public string SlotId { get; init; } = string.Empty;
    public string StableFileName { get; init; } = string.Empty;
    public string StagedRelativePath { get; init; } = string.Empty;
    public string SourceGoal { get; init; } = "Goal054";
    public string SourceGoalId { get; init; } = string.Empty;
    public string SourceRelativePath { get; init; } = string.Empty;
    public string SourceSha256 { get; init; } = string.Empty;
    public long SourceSizeBytes { get; init; }
    public string StagedSha256 { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string LicenseDecision { get; init; } = string.Empty;
    public string ProvenanceDecision { get; init; } = string.Empty;
    public string ReviewTrace { get; init; } = string.Empty;
    public bool SafeRelativePath { get; init; }
    public bool SourceHashMatches { get; init; }
    public bool PngValid { get; init; }
    public int PngWidth { get; init; }
    public int PngHeight { get; init; }
    public bool WavValid { get; init; }
    public int WavSampleRate { get; init; }
    public int WavChannels { get; init; }
    public int WavSampleCount { get; init; }
    public string DeterministicOrderingKey { get; init; } = string.Empty;
}

public sealed record MediaBindingRecord
{
    public string BindingId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SlotId { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public string StagedRelativePath { get; init; } = string.Empty;
    public string StreamingAssetsRelativePath { get; init; } = string.Empty;
    public string SourceGoal054RelativePath { get; init; } = string.Empty;
    public string SourceGoal054Sha256 { get; init; } = string.Empty;
    public string StagedSha256 { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string UnityAddress { get; init; } = string.Empty;
    public string ReviewTrace { get; init; } = string.Empty;
}

public sealed record FamilyReviewPackageRecord
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string PackageRoot { get; init; } = MediaBoundPlayableReviewPackageVocabulary.ReviewPackageRoot;
    public string ReadmeRelativePath { get; init; } = string.Empty;
    public string PlayableManifestRelativePath { get; init; } = string.Empty;
    public string StreamingAssetsManifestRelativePath { get; init; } = string.Empty;
    public string SourceDryRunArtifactRef { get; init; } = string.Empty;
    public int StagedFileCount { get; init; }
    public int ImagePngCount { get; init; }
    public int WavCount { get; init; }
    public int BundleJsonCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> StagedMediaRefs { get; init; } = [];
}

public sealed record MediaBoundReviewPackageManifest
{
    public string SchemaVersion { get; init; } = MediaBoundPlayableReviewPackageVocabulary.ReviewPackageManifestSchemaVersion;
    public string GoalId { get; init; } = MediaBoundPlayableReviewPackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public string ReviewPackageRoot { get; init; } = MediaBoundPlayableReviewPackageVocabulary.ReviewPackageRoot;
    public string ReadmeRelativePath { get; init; } = string.Empty;
    public string ChecklistRelativePath { get; init; } = string.Empty;
    public string PlayableManifestRelativePath { get; init; } = string.Empty;
    public int StagedFileCount { get; init; }
    public int PngFileCount { get; init; }
    public int WavFileCount { get; init; }
    public int BundleJsonFileCount { get; init; }
    public IReadOnlyList<FamilyReviewPackageRecord> Families { get; init; } = [];
    public IReadOnlyList<StagedMediaFileRecord> StagedFiles { get; init; } = [];
    public IReadOnlyList<MediaBindingRecord> Bindings { get; init; } = [];
    public IReadOnlyList<string> PackageFiles { get; init; } = [];
    public string DeterministicHash { get; init; } = string.Empty;
}

public sealed record StreamingAssetsMediaManifest
{
    public string SchemaVersion { get; init; } = MediaBoundPlayableReviewPackageVocabulary.StreamingAssetsManifestSchemaVersion;
    public string GoalId { get; init; } = MediaBoundPlayableReviewPackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public string ManifestRelativePath { get; init; } = string.Empty;
    public string StreamingAssetsRoot { get; init; } = MediaBoundPlayableReviewPackageVocabulary.StreamingAssetsRoot;
    public int FamilyCount { get; init; }
    public int BindingCount { get; init; }
    public IReadOnlyList<MediaBindingRecord> Bindings { get; init; } = [];
    public string DeterministicHash { get; init; } = string.Empty;
}

public sealed record MediaBoundPreviewPayloadRecord
{
    public string PreviewPayloadId { get; init; } = string.Empty;
    public string ExportPayloadId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ReferencedDryRunArtifactRef { get; init; } = string.Empty;
    public string Goal054PreviewPayloadId { get; init; } = string.Empty;
    public IReadOnlyList<string> StagedMediaRefs { get; init; } = [];
    public string StreamingAssetsManifestRef { get; init; } = string.Empty;
    public string UnityLoadContractRef { get; init; } = string.Empty;
    public string UnityLoadProofRef { get; init; } = string.Empty;
    public string ValidationStatus { get; init; } = "passed";
    public string HashSummary { get; init; } = string.Empty;
}

public sealed record MediaBoundPreviewPayloads
{
    public string SchemaVersion { get; init; } = MediaBoundPlayableReviewPackageVocabulary.PreviewPayloadsSchemaVersion;
    public string GoalId { get; init; } = MediaBoundPlayableReviewPackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public bool FamilyDryRunToMediaManifestProof { get; init; }
    public IReadOnlyList<MediaBoundPreviewPayloadRecord> Payloads { get; init; } = [];
}

public sealed record UnityMediaLoadFileContract
{
    public string FamilyId { get; init; } = string.Empty;
    public string SlotId { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public string StreamingAssetsRelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public int ExpectedWidth { get; init; }
    public int ExpectedHeight { get; init; }
    public int ExpectedSampleRate { get; init; }
    public int ExpectedChannels { get; init; }
    public int ExpectedSampleCount { get; init; }
}

public sealed record UnityMediaLoadContract
{
    public string SchemaVersion { get; init; } = MediaBoundPlayableReviewPackageVocabulary.UnityLoadContractSchemaVersion;
    public string GoalId { get; init; } = MediaBoundPlayableReviewPackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public string ContractId { get; init; } = "unity-media-load-contract/streaming-assets-goal055";
    public string ReadSurface { get; init; } = "Application.streamingAssetsPath";
    public string TargetPlatform { get; init; } = "desktop_windows_review_package";
    public string ImageLoadApi { get; init; } = "UnityEngine.ImageConversion.LoadImage";
    public string WavValidationMode { get; init; } = "bcl_pcm_wav_header_and_data_validation_no_playback_claim";
    public bool UnitySourceChanged { get; init; }
    public bool UnityBuildOrPlayerExecuted { get; init; }
    public string ManifestRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredProofLineTemplates { get; init; } = [];
    public IReadOnlyList<UnityMediaLoadFileContract> Files { get; init; } = [];
    public IReadOnlyList<MediaBoundDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityMediaLoadProofRecord
{
    public string ProofKind { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SlotId { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public string StagedRelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public int SampleRate { get; init; }
    public int Channels { get; init; }
    public int SampleCount { get; init; }
    public string ProofLine { get; init; } = string.Empty;
}

public sealed record UnityMediaLoadProof
{
    public string SchemaVersion { get; init; } = MediaBoundPlayableReviewPackageVocabulary.UnityLoadProofSchemaVersion;
    public string GoalId { get; init; } = MediaBoundPlayableReviewPackageVocabulary.GoalId;
    public string FamilyId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public bool ManifestLoaded { get; init; }
    public bool ImageLoaded { get; init; }
    public bool WavValidated { get; init; }
    public bool FamilyPanelReady { get; init; }
    public bool UnitySourceChanged { get; init; }
    public bool UnityBuildOrPlayerExecuted { get; init; }
    public IReadOnlyList<string> ProofLines { get; init; } = [];
    public IReadOnlyList<UnityMediaLoadProofRecord> Records { get; init; } = [];
    public IReadOnlyList<MediaBoundDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaBoundFamilySmokeResult
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int StagedFileCount { get; init; }
    public int PngFileCount { get; init; }
    public int WavFileCount { get; init; }
    public int BundleJsonFileCount { get; init; }
    public bool ManifestBound { get; init; }
    public bool PreviewPayloadBound { get; init; }
    public bool UnityProofBound { get; init; }
    public string HashSummary { get; init; } = string.Empty;
    public IReadOnlyList<string> StagedMediaRefs { get; init; } = [];
}

public sealed record MediaBoundFamilySmokeMatrix
{
    public string SchemaVersion { get; init; } = MediaBoundPlayableReviewPackageVocabulary.FamilySmokeMatrixSchemaVersion;
    public string GoalId { get; init; } = MediaBoundPlayableReviewPackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public IReadOnlyList<MediaBoundFamilySmokeResult> Families { get; init; } = [];
}

public sealed record InvalidMediaBoundPackageScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<MediaBoundDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidMediaBoundPackageDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = MediaBoundPlayableReviewPackageVocabulary.InvalidMatrixSchemaVersion;
    public string GoalId { get; init; } = MediaBoundPlayableReviewPackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public int BlockedCount { get; init; }
    public IReadOnlyList<InvalidMediaBoundPackageScenario> Scenarios { get; init; } = [];
}

public sealed record Goal055ArtifactScopeReport
{
    public string SchemaVersion { get; init; } = MediaBoundPlayableReviewPackageVocabulary.ArtifactScopeReportSchemaVersion;
    public string Scenario { get; init; } = MediaBoundPlayableReviewPackageVocabulary.ProductSmokeRoute;
    public bool Passed { get; init; }
    public IReadOnlyList<string> AllowedExactPaths { get; init; } = [];
    public IReadOnlyList<string> AllowedPathPrefixes { get; init; } = [];
    public IReadOnlyList<string> ForbiddenPathPrefixesObserved { get; init; } = [];
}

public sealed record MediaBoundPlayableReviewPackageReport
{
    public string SchemaVersion { get; init; } = MediaBoundPlayableReviewPackageVocabulary.ReportSchemaVersion;
    public string GoalId { get; init; } = MediaBoundPlayableReviewPackageVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = MediaBoundPlayableReviewPackageVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = MediaBoundPlayableReviewPackageVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Goal054AcceptedByUserHandoff { get; init; }
    public int FamilyCount { get; init; }
    public int StagedFileCount { get; init; }
    public int PngFileCount { get; init; }
    public int WavFileCount { get; init; }
    public int BundleJsonFileCount { get; init; }
    public bool PhysicalMediaStaged { get; init; }
    public bool PngProofPassed { get; init; }
    public bool WavProofPassed { get; init; }
    public bool BundleProofPassed { get; init; }
    public bool ReviewPackageManifestPassed { get; init; }
    public bool StreamingAssetsManifestPassed { get; init; }
    public bool PreviewPayloadsPassed { get; init; }
    public bool UnityMediaLoadContractPassed { get; init; }
    public bool FamilySmokeMatrixPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool ProviderCalls { get; init; }
    public bool NetworkImports { get; init; }
    public bool LlmCalls { get; init; }
    public bool LuaExecuted { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool UnitySourceChanged { get; init; }
    public bool UnityBuildOrPlayerExecuted { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string ReviewPackageManifestHash { get; init; } = string.Empty;
    public string StreamingAssetsManifestHash { get; init; } = string.Empty;
    public string PreviewPayloadsHash { get; init; } = string.Empty;
    public string UnityLoadContractHash { get; init; } = string.Empty;
    public string FamilySmokeMatrixHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<MediaBoundDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaBoundPackageTextFile
{
    public string RelativePath { get; init; } = string.Empty;
    public string Contents { get; init; } = string.Empty;
}

public sealed record MediaBoundPlayableReviewPackageEvidenceResult
{
    public MediaBoundSourceManifest SourceManifest { get; init; } = new();
    public MediaBoundReviewPackageManifest ReviewPackageManifest { get; init; } = new();
    public StreamingAssetsMediaManifest StreamingAssetsManifest { get; init; } = new();
    public MediaBoundPreviewPayloads PreviewPayloads { get; init; } = new();
    public UnityMediaLoadContract UnityLoadContract { get; init; } = new();
    public IReadOnlyList<UnityMediaLoadProof> UnityLoadProofs { get; init; } = [];
    public MediaBoundFamilySmokeMatrix FamilySmokeMatrix { get; init; } = new();
    public InvalidMediaBoundPackageDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public Goal055ArtifactScopeReport ArtifactScopeReport { get; init; } = new();
    public MediaBoundPlayableReviewPackageReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<StagedMediaFilePayload> StagedMediaFiles { get; init; } = [];
    public IReadOnlyList<MediaBoundPackageTextFile> PackageTextFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record MediaBoundPlayableReviewPackageWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public string ReportMarkdownPath { get; init; } = string.Empty;
}
