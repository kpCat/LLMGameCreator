using LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;
using LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;

namespace LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;

public static class MediaMaterializationReviewPackageVocabulary
{
    public const string GoalId = "goal_054_media_materialization_review_package";
    public const string ProductSmokeRoute = "goal-054-media-materialization-review-package";
    public const string FinalGate = "media_materialization_review_package_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-054-media-materialization-review-package";
    public const string MaterializedMediaRoot = "review-package/media";

    public const string SourceManifestSchemaVersion = "media_materialization_source_manifest_v1";
    public const string QueueSchemaVersion = "media_materialization_queue_v1";
    public const string InventorySchemaVersion = "materialized_media_inventory_v1";
    public const string LicenseLedgerSchemaVersion = "media_materialization_provenance_license_ledger_v1";
    public const string BindingValidationSchemaVersion = "media_materialization_binding_validation_v1";
    public const string ReviewPackageManifestSchemaVersion = "media_review_package_manifest_v1";
    public const string PreviewExportPayloadsSchemaVersion = "preview_export_media_bound_payloads_v1";
    public const string FamilySmokeSchemaVersion = "family_media_smoke_v1";
    public const string InvalidMatrixSchemaVersion = "invalid_media_materialization_matrix_v1";
    public const string ReportSchemaVersion = "media_materialization_review_package_report_v1";

    public static readonly IReadOnlyList<string> FamilyIds = MediaAssetCampaignVocabulary.FamilyIds;

    public static readonly IReadOnlyList<string> RequiredInvalidScenarioIds =
    [
        "missing_goal053_source",
        "fake_media_request_id",
        "fake_binding_id",
        "missing_physical_media_file",
        "hash_mismatch",
        "media_kind_mismatch",
        "unknown_prohibited_license_promoted",
        "imported_provider_candidate_promoted",
        "cross_family_binding_leak",
        "absolute_path_leak",
        "network_provider_llm_rag_call_claim",
        "gamepackage_schema_mutation_claim",
        "runtime_ui_unity_mutation_claim",
        "nondeterministic_ordering",
        "malformed_png_header",
        "malformed_wav_header",
        "missing_provenance",
        "missing_review_trace"
    ];
}

public sealed record MediaMaterializationBoundaryClaims
{
    public bool ProviderLlmRagCalled { get; init; }
    public bool NetworkOrImportCalled { get; init; }
    public bool ExternalMediaProviderCalled { get; init; }
    public bool LuaExecuted { get; init; }
    public bool GamePackageSchemaChanged { get; init; }
    public bool RuntimeSourceChanged { get; init; }
    public bool RuntimeAbstractionsChanged { get; init; }
    public bool WinFormsUiChanged { get; init; }
    public bool UnitySourceChanged { get; init; }
    public bool ProviderPathChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool ExternalDependencyAdded { get; init; }

    public bool AllFalse =>
        !ProviderLlmRagCalled &&
        !NetworkOrImportCalled &&
        !ExternalMediaProviderCalled &&
        !LuaExecuted &&
        !GamePackageSchemaChanged &&
        !RuntimeSourceChanged &&
        !RuntimeAbstractionsChanged &&
        !WinFormsUiChanged &&
        !UnitySourceChanged &&
        !ProviderPathChanged &&
        !GeneratorLibraryChanged &&
        !ExternalDependencyAdded;
}

public sealed record MediaMaterializationDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static MediaMaterializationDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static MediaMaterializationDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static MediaMaterializationDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record MediaMaterializationSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactFileName { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
}

public sealed record MediaMaterializationSourceBundle
{
    public MediaCampaignSourceManifest Goal053SourceManifest { get; init; } = new();
    public MediaRequestQueue Goal053RequestQueue { get; init; } = new();
    public MediaLicenseProvenanceLedger Goal053LicenseLedger { get; init; } = new();
    public MediaCandidateQuarantine Goal053CandidateQuarantine { get; init; } = new();
    public MediaReviewPromotionLedger Goal053ReviewLedger { get; init; } = new();
    public MediaFixtureFileInventory Goal053FixtureInventory { get; init; } = new();
    public MediaBindingManifest Goal053BindingManifest { get; init; } = new();
    public LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration.PreviewExportMediaPayloads Goal053PreviewExportPayloads { get; init; } = new();
    public InvalidMediaDiagnosticsMatrix Goal053InvalidMatrix { get; init; } = new();
    public string Goal053ReportMarkdown { get; init; } = string.Empty;
    public FullGeneratorDryRunManifest Goal047SourceManifest { get; init; } = new();
    public IReadOnlyList<FullGeneratorFamilyDryRunRecord> Goal047FamilyDryRuns { get; init; } = [];
    public IReadOnlyList<MediaMaterializationSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<MediaMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaMaterializationGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record MediaMaterializationFamilySourceRecord
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string DryRunArtifactRef { get; init; } = string.Empty;
    public string RuntimePreviewPayloadRef { get; init; } = string.Empty;
    public string ExportProfileId { get; init; } = string.Empty;
    public int SourceMediaRequestCount { get; init; }
    public int SourceBindingCount { get; init; }
}

public sealed record MediaMaterializationSourceManifest
{
    public string SchemaVersion { get; init; } = MediaMaterializationReviewPackageVocabulary.SourceManifestSchemaVersion;
    public string GoalId { get; init; } = MediaMaterializationReviewPackageVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = MediaMaterializationReviewPackageVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = MediaMaterializationReviewPackageVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal053AcceptedByUserHandoff { get; init; }
    public bool Goal053ProducedForReviewReportGreen { get; init; }
    public bool Goal053ReportKeptRequired { get; init; }
    public int Goal053RequestCount { get; init; }
    public int Goal053BindingCount { get; init; }
    public IReadOnlyList<MediaMaterializationGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<string> SelectedFamilyIds { get; init; } = [];
    public IReadOnlyList<MediaMaterializationFamilySourceRecord> Families { get; init; } = [];
    public IReadOnlyList<MediaMaterializationSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public MediaMaterializationBoundaryClaims BoundaryClaims { get; init; } = new();
    public IReadOnlyList<MediaMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaMaterializationQueueItem
{
    public string MaterializationId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SourceRequestId { get; init; } = string.Empty;
    public string SourceBindingId { get; init; } = string.Empty;
    public string SourceCandidateId { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public string MediaSlotId { get; init; } = string.Empty;
    public string GeneratedTargetId { get; init; } = string.Empty;
    public string MaterializedMediaFormat { get; init; } = string.Empty;
    public string OutputRelativePath { get; init; } = string.Empty;
    public string ProvenanceStatus { get; init; } = "repo_generated_deterministic_fixture";
    public string LicenseStatus { get; init; } = "repo_fixture_no_external_license";
    public string ReviewStatus { get; init; } = "promoted_fixture_materialized_for_review";
    public string ExpectedSha256 { get; init; } = string.Empty;
    public long ExpectedByteLength { get; init; }
    public string ConsumerPayloadRole { get; init; } = string.Empty;
    public string DeterministicOrderingKey { get; init; } = string.Empty;
}

public sealed record MediaMaterializationQueue
{
    public string SchemaVersion { get; init; } = MediaMaterializationReviewPackageVocabulary.QueueSchemaVersion;
    public string GoalId { get; init; } = MediaMaterializationReviewPackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public int QueueItemCount { get; init; }
    public IReadOnlyList<MediaMaterializationQueueItem> Items { get; init; } = [];
    public IReadOnlyList<MediaMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MaterializedMediaFileRecord
{
    public string MaterializationId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public string MediaSlotId { get; init; } = string.Empty;
    public string MaterializedMediaFormat { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public long ByteLength { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public bool PngSignatureValid { get; init; }
    public bool PngChunkCrcsValid { get; init; }
    public bool WavHeaderValid { get; init; }
    public bool DeterministicBytes { get; init; }
}

public sealed record MaterializedMediaInventory
{
    public string SchemaVersion { get; init; } = MediaMaterializationReviewPackageVocabulary.InventorySchemaVersion;
    public string GoalId { get; init; } = MediaMaterializationReviewPackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FileCount { get; init; }
    public int PngFileCount { get; init; }
    public int WavFileCount { get; init; }
    public int BundleJsonFileCount { get; init; }
    public IReadOnlyList<MaterializedMediaFileRecord> Files { get; init; } = [];
    public IReadOnlyList<MediaMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaLicenseDecisionProof
{
    public string SourceKind { get; init; } = string.Empty;
    public string Goal054Decision { get; init; } = string.Empty;
    public bool PromotedInGoal054 { get; init; }
    public bool RequiresAttributionPayload { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public sealed record MaterializedMediaProvenanceRecord
{
    public string MaterializationId { get; init; } = string.Empty;
    public string SourceBindingId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public string SourceKind { get; init; } = "fixture-generated-by-repo";
    public string LicenseStatus { get; init; } = "repo_fixture_no_external_license";
    public string ProvenanceStatus { get; init; } = "repository_generated_deterministic_bytes";
    public bool ProviderImportedOrManual { get; init; }
    public bool AttributionPayloadPresent { get; init; }
}

public sealed record MediaProvenanceLicenseLedger
{
    public string SchemaVersion { get; init; } = MediaMaterializationReviewPackageVocabulary.LicenseLedgerSchemaVersion;
    public string GoalId { get; init; } = MediaMaterializationReviewPackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public IReadOnlyList<MediaLicenseDecisionProof> LicenseDecisions { get; init; } = [];
    public IReadOnlyList<MaterializedMediaProvenanceRecord> MaterializedFiles { get; init; } = [];
    public IReadOnlyList<MediaMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaBindingValidationRecord
{
    public string BindingId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string MaterializationId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public bool SourceSlotExists { get; init; }
    public bool MaterializedFileExistsInInventory { get; init; }
    public bool FileHashMatchesExpected { get; init; }
    public bool MediaKindMatchesSlot { get; init; }
    public bool SafeRelativePath { get; init; }
    public bool CrossFamilyLeakDetected { get; init; }
    public bool UnapprovedProviderImportBound { get; init; }
    public IReadOnlyList<MediaMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaBindingValidation
{
    public string SchemaVersion { get; init; } = MediaMaterializationReviewPackageVocabulary.BindingValidationSchemaVersion;
    public string GoalId { get; init; } = MediaMaterializationReviewPackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public int BindingCount { get; init; }
    public bool EveryFamilyHasImageAndAudioFixture { get; init; }
    public IReadOnlyList<MediaBindingValidationRecord> Bindings { get; init; } = [];
    public IReadOnlyList<MediaMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaBoundPayloadRecord
{
    public string PreviewPayloadId { get; init; } = string.Empty;
    public string ExportPayloadId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ReferencedDryRunArtifactRef { get; init; } = string.Empty;
    public IReadOnlyList<string> ReferencedMediaBindingIds { get; init; } = [];
    public IReadOnlyList<string> PhysicalMediaFileRefs { get; init; } = [];
    public string HashSummary { get; init; } = string.Empty;
    public string ValidationStatus { get; init; } = "passed";
    public bool IncludedInReviewPackage { get; init; }
}

public sealed record PreviewExportMediaPayloads
{
    public string SchemaVersion { get; init; } = MediaMaterializationReviewPackageVocabulary.PreviewExportPayloadsSchemaVersion;
    public string GoalId { get; init; } = MediaMaterializationReviewPackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public bool AllMediaRefsResolveToInventory { get; init; }
    public bool GamePackageSchemaChanged { get; init; }
    public bool RuntimeUiUnityChanged { get; init; }
    public IReadOnlyList<MediaBoundPayloadRecord> Payloads { get; init; } = [];
    public IReadOnlyList<MediaMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaReviewPackageManifest
{
    public string SchemaVersion { get; init; } = MediaMaterializationReviewPackageVocabulary.ReviewPackageManifestSchemaVersion;
    public string GoalId { get; init; } = MediaMaterializationReviewPackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public string ReviewPackageRoot { get; init; } = "review-package";
    public IReadOnlyList<string> ManifestPathList { get; init; } = [];
    public IReadOnlyList<string> MediaFileList { get; init; } = [];
    public IReadOnlyList<string> PayloadList { get; init; } = [];
    public IReadOnlyList<string> LicenseProvenanceList { get; init; } = [];
    public IReadOnlyDictionary<string, int> FamilyCoverageSummary { get; init; } = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<string> ManualReviewChecklist { get; init; } = [];
    public IReadOnlyList<string> ValidatorSummary { get; init; } = [];
    public string DeterministicHash { get; init; } = string.Empty;
}

public sealed record FamilyMediaSmokeProof
{
    public string SchemaVersion { get; init; } = MediaMaterializationReviewPackageVocabulary.FamilySmokeSchemaVersion;
    public string GoalId { get; init; } = MediaMaterializationReviewPackageVocabulary.GoalId;
    public string FamilyId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int MaterializedBindingCount { get; init; }
    public int ImagePngCount { get; init; }
    public int AudioWavCount { get; init; }
    public IReadOnlyList<string> PayloadIds { get; init; } = [];
    public IReadOnlyList<string> MediaFileRefs { get; init; } = [];
    public string HashSummary { get; init; } = string.Empty;
    public IReadOnlyList<MediaMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidMediaMaterializationScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<MediaMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidMediaMaterializationMatrix
{
    public string SchemaVersion { get; init; } = MediaMaterializationReviewPackageVocabulary.InvalidMatrixSchemaVersion;
    public string GoalId { get; init; } = MediaMaterializationReviewPackageVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public int BlockedCount { get; init; }
    public IReadOnlyList<InvalidMediaMaterializationScenario> Scenarios { get; init; } = [];
}

public sealed record MediaMaterializationReviewPackageReport
{
    public string SchemaVersion { get; init; } = MediaMaterializationReviewPackageVocabulary.ReportSchemaVersion;
    public string GoalId { get; init; } = MediaMaterializationReviewPackageVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = MediaMaterializationReviewPackageVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = MediaMaterializationReviewPackageVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int FamilyCount { get; init; }
    public int QueueItemCount { get; init; }
    public int MaterializedFileCount { get; init; }
    public int PngFileCount { get; init; }
    public int WavFileCount { get; init; }
    public int BundleJsonFileCount { get; init; }
    public bool Goal053AcceptedByUserHandoff { get; init; }
    public bool Goal053SourceReportGreenRequired { get; init; }
    public bool PhysicalMediaProduced { get; init; }
    public bool PngProofPassed { get; init; }
    public bool WavProofPassed { get; init; }
    public bool ProvenanceLicenseLedgerPassed { get; init; }
    public bool BindingValidationPassed { get; init; }
    public bool PreviewExportPayloadsPassed { get; init; }
    public bool ReviewPackageManifestPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool ProviderNetworkLlmRagCalled { get; init; }
    public bool GamePackageSchemaChanged { get; init; }
    public bool RuntimeUiUnityChanged { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string QueueHash { get; init; } = string.Empty;
    public string InventoryHash { get; init; } = string.Empty;
    public string LicenseLedgerHash { get; init; } = string.Empty;
    public string BindingValidationHash { get; init; } = string.Empty;
    public string ReviewPackageManifestHash { get; init; } = string.Empty;
    public string PreviewExportPayloadsHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<MediaMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MaterializedMediaFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record MediaMaterializationReviewPackageEvidenceResult
{
    public MediaMaterializationSourceManifest SourceManifest { get; init; } = new();
    public MediaMaterializationQueue MaterializationQueue { get; init; } = new();
    public MaterializedMediaInventory MaterializedMediaInventory { get; init; } = new();
    public MediaProvenanceLicenseLedger ProvenanceLicenseLedger { get; init; } = new();
    public MediaBindingValidation BindingValidation { get; init; } = new();
    public MediaReviewPackageManifest ReviewPackageManifest { get; init; } = new();
    public PreviewExportMediaPayloads PreviewExportMediaPayloads { get; init; } = new();
    public IReadOnlyList<FamilyMediaSmokeProof> FamilySmokeProofs { get; init; } = [];
    public InvalidMediaMaterializationMatrix InvalidMatrix { get; init; } = new();
    public MediaMaterializationReviewPackageReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<MaterializedMediaFilePayload> MediaFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record MediaMaterializationReviewPackageWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public string ReportMarkdownPath { get; init; } = string.Empty;
}
