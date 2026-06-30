using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;
using LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;

namespace LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;

public static class MediaAssetCampaignVocabulary
{
    public const string GoalId = "goal_053_media_asset_campaign_orchestration";
    public const string ProductSmokeRoute = "goal-053-media-asset-campaign-orchestration";
    public const string FinalGate = "media_asset_campaign_orchestration_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-053-media-asset-campaign-orchestration";

    public const string SourceManifestSchemaVersion = "media_campaign_source_manifest_v1";
    public const string SlotCatalogSchemaVersion = "media_slot_catalog_v1";
    public const string RequestQueueSchemaVersion = "media_request_queue_v1";
    public const string StylePolicySchemaVersion = "media_style_policy_v1";
    public const string LicenseLedgerSchemaVersion = "media_license_provenance_ledger_v1";
    public const string CandidateQuarantineSchemaVersion = "media_candidate_quarantine_v1";
    public const string ReviewLedgerSchemaVersion = "media_review_promotion_ledger_v1";
    public const string BindingManifestSchemaVersion = "media_binding_manifest_v1";
    public const string FixtureInventorySchemaVersion = "media_fixture_file_inventory_v1";
    public const string PreviewExportPayloadSchemaVersion = "preview_export_media_payloads_v1";
    public const string InvalidMatrixSchemaVersion = "invalid_media_diagnostics_matrix_v1";
    public const string ReportSchemaVersion = "media_asset_campaign_orchestration_report_v1";

    public static readonly IReadOnlyList<string> FamilyIds = FullGeneratorWithoutMediaDryRunVocabulary.FamilyIds;

    public static readonly IReadOnlyList<string> RequiredSlotIds =
    [
        "world_key_art",
        "region_tile_or_background",
        "npc_portrait",
        "species_or_archetype_portrait",
        "item_icon",
        "quest_or_event_icon",
        "ui_panel_skin",
        "sfx_interaction",
        "sfx_combat_or_hazard",
        "ambient_loop",
        "music_stinger",
        "export_placeholder_bundle"
    ];

    public static readonly IReadOnlyList<string> LicenseSourceKinds =
    [
        "fixture-generated-by-repo",
        "manual-user-provided",
        "imported-cc0",
        "imported-cc-by",
        "imported-share-alike-or-gpl-risk",
        "provider-generated-with-model-license",
        "unknown/no-license"
    ];

    public static readonly IReadOnlyList<string> RequiredReviewDecisions =
    [
        "promote_fixture",
        "needs_manual_review",
        "blocked_license",
        "blocked_missing_provenance",
        "blocked_provider_not_configured",
        "blocked_leak",
        "blocked_mismatch"
    ];
}

public sealed record MediaCampaignBoundaryClaims
{
    public bool FinalProseOrArtworkClaim { get; init; }
    public bool ProviderLlmRagCalled { get; init; }
    public bool RealMediaGenerationCalled { get; init; }
    public bool NetworkOrImportCalled { get; init; }
    public bool GamePackageSchemaChanged { get; init; }
    public bool RuntimeSourceChanged { get; init; }
    public bool RuntimeAbstractionsChanged { get; init; }
    public bool WinFormsUiChanged { get; init; }
    public bool UnitySourceOrExportChanged { get; init; }
    public bool ProviderPathChanged { get; init; }
    public bool LuaOrGeneratorLibraryChanged { get; init; }
    public bool ExternalDependencyAdded { get; init; }

    [JsonIgnore]
    public bool AllFalse =>
        !FinalProseOrArtworkClaim &&
        !ProviderLlmRagCalled &&
        !RealMediaGenerationCalled &&
        !NetworkOrImportCalled &&
        !GamePackageSchemaChanged &&
        !RuntimeSourceChanged &&
        !RuntimeAbstractionsChanged &&
        !WinFormsUiChanged &&
        !UnitySourceOrExportChanged &&
        !ProviderPathChanged &&
        !LuaOrGeneratorLibraryChanged &&
        !ExternalDependencyAdded;
}

public sealed record MediaCampaignDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static MediaCampaignDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static MediaCampaignDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static MediaCampaignDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record MediaCampaignSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactFileName { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
}

public sealed record MediaCampaignSourceBundle
{
    public FullGeneratorDryRunManifest Goal047Manifest { get; init; } = new();
    public FullGeneratorReviewPromotionLedger Goal047ReviewLedger { get; init; } = new();
    public IReadOnlyList<FullGeneratorFamilyDryRunRecord> Goal047FamilyDryRuns { get; init; } = [];
    public FullGeneratorRuntimePreviewValidationMatrix Goal047RuntimePreviewMatrix { get; init; } = new();
    public FullGeneratorExportProfileSelectionMatrix Goal047ExportProfileMatrix { get; init; } = new();
    public FullGeneratorPackageCompatibilitySummary Goal047PackageSummary { get; init; } = new();
    public FullGeneratorOneClickDryRunSummary Goal047OneClickSummary { get; init; } = new();
    public FamilyTemplateCatalog Goal043Catalog { get; init; } = new();
    public ChunkedPreviewPayload Goal040MetamodulePayload { get; init; } = new();
    public IReadOnlyList<MediaCampaignSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyDictionary<string, string> ArtifactHashByRelativePath { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record MediaCampaignGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record MediaCampaignFamilySourceRecord
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string StyleId { get; init; } = string.Empty;
    public string DryRunArtifactRef { get; init; } = string.Empty;
    public string DryRunArtifactHash { get; init; } = string.Empty;
    public string RuntimePreviewPayloadRef { get; init; } = string.Empty;
    public string RuntimePreviewPayloadHash { get; init; } = string.Empty;
    public string ExportProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> GeneratedTemplateIds { get; init; } = [];
    public IReadOnlyList<string> GeneratedRuntimeTargetIds { get; init; } = [];
    public IReadOnlyList<string> SemanticFeatureRefs { get; init; } = [];
}

public sealed record MediaCampaignMetamoduleStressSummary
{
    public string ScenarioId { get; init; } = "metamodule_kingdoms";
    public int KingdomOrRegionGroupCount { get; init; }
    public int RuntimeDeltaMarkerCount { get; init; }
    public int CompactedSpeciesArchetypeSlotRefCount { get; init; }
    public bool OneRequestPerSpeciesArchetypeSlotGenerated { get; init; }
    public string CompactionPolicy { get; init; } = "summary_only_no_per_species_file_expansion";
    public IReadOnlyList<string> SourceFeatureRefs { get; init; } = [];
}

public sealed record MediaCampaignSourceManifest
{
    public string SchemaVersion { get; init; } = MediaAssetCampaignVocabulary.SourceManifestSchemaVersion;
    public string GoalId { get; init; } = MediaAssetCampaignVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = MediaAssetCampaignVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = MediaAssetCampaignVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public IReadOnlyList<MediaCampaignGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<string> SelectedFamilyIds { get; init; } = [];
    public IReadOnlyList<MediaCampaignFamilySourceRecord> Families { get; init; } = [];
    public MediaCampaignMetamoduleStressSummary MetamoduleStressSummary { get; init; } = new();
    public IReadOnlyList<MediaCampaignSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public MediaCampaignBoundaryClaims BoundaryClaims { get; init; } = new();
    public IReadOnlyList<MediaCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaSlotDefinition
{
    public string SlotId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public IReadOnlyList<string> TargetFamilies { get; init; } = [];
    public string DimensionsOrDurationHint { get; init; } = string.Empty;
    public IReadOnlyList<string> SemanticStyleTags { get; init; } = [];
    public IReadOnlyList<string> AllowedSourceTypes { get; init; } = [];
    public IReadOnlyList<string> ReviewRequirements { get; init; } = [];
    public string LicensePolicyRequirement { get; init; } = string.Empty;
    public string BindingTargetKind { get; init; } = string.Empty;
    public string FallbackPlaceholderBehavior { get; init; } = string.Empty;
}

public sealed record MediaSlotCatalog
{
    public string SchemaVersion { get; init; } = MediaAssetCampaignVocabulary.SlotCatalogSchemaVersion;
    public string GoalId { get; init; } = MediaAssetCampaignVocabulary.GoalId;
    public bool Passed { get; init; }
    public IReadOnlyList<MediaSlotDefinition> Slots { get; init; } = [];
    public IReadOnlyList<MediaCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaStylePolicyRecord
{
    public string StyleId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceStyleRefs { get; init; } = [];
    public IReadOnlyList<string> VisualTags { get; init; } = [];
    public IReadOnlyList<string> AudioTags { get; init; } = [];
    public IReadOnlyList<string> UiTags { get; init; } = [];
    public IReadOnlyList<string> PromptSkeletonSections { get; init; } = [];
    public bool ContainsFinalProviderPromptText { get; init; }
}

public sealed record MediaStylePolicy
{
    public string SchemaVersion { get; init; } = MediaAssetCampaignVocabulary.StylePolicySchemaVersion;
    public string GoalId { get; init; } = MediaAssetCampaignVocabulary.GoalId;
    public bool Passed { get; init; }
    public IReadOnlyList<MediaStylePolicyRecord> Styles { get; init; } = [];
}

public sealed record MediaPromptInputSkeleton
{
    public string SubjectRef { get; init; } = string.Empty;
    public string StyleRef { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredContentFacts { get; init; } = [];
    public IReadOnlyList<string> SemanticTags { get; init; } = [];
    public IReadOnlyList<string> NegativeBoundaries { get; init; } = [];
    public string OutputContract { get; init; } = string.Empty;
    public bool FinalProviderPromptText { get; init; }
}

public sealed record MediaRequestRecord
{
    public string RequestId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string StyleId { get; init; } = string.Empty;
    public string TargetGeneratedId { get; init; } = string.Empty;
    public string TargetArtifactFamily { get; init; } = string.Empty;
    public string TargetArtifactKind { get; init; } = string.Empty;
    public string MediaSlotId { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public MediaPromptInputSkeleton PromptInputSkeleton { get; init; } = new();
    public IReadOnlyList<string> SemanticTags { get; init; } = [];
    public string RequiredProvenancePolicy { get; init; } = string.Empty;
    public string BudgetHint { get; init; } = string.Empty;
    public int Priority { get; init; }
    public string DeterministicOrderingKey { get; init; } = string.Empty;
    public string Status { get; init; } = "requested";
}

public sealed record MediaRequestQueue
{
    public string SchemaVersion { get; init; } = MediaAssetCampaignVocabulary.RequestQueueSchemaVersion;
    public string GoalId { get; init; } = MediaAssetCampaignVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public int RequestCount { get; init; }
    public MediaCampaignMetamoduleStressSummary MetamoduleStressSummary { get; init; } = new();
    public IReadOnlyList<MediaRequestRecord> Requests { get; init; } = [];
    public IReadOnlyList<MediaCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaLicensePolicyRecord
{
    public string SourceKind { get; init; } = string.Empty;
    public string PromotionPolicy { get; init; } = string.Empty;
    public string RequiredMetadata { get; init; } = string.Empty;
    public string Goal053Decision { get; init; } = string.Empty;
    public bool CanAutoPromoteInGoal053 { get; init; }
}

public sealed record MediaLicenseProvenanceLedger
{
    public string SchemaVersion { get; init; } = MediaAssetCampaignVocabulary.LicenseLedgerSchemaVersion;
    public string GoalId { get; init; } = MediaAssetCampaignVocabulary.GoalId;
    public bool Passed { get; init; }
    public IReadOnlyList<MediaLicensePolicyRecord> Policies { get; init; } = [];
    public IReadOnlyList<MediaCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaCandidateRecord
{
    public string CandidateId { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public string CandidateKind { get; init; } = string.Empty;
    public string SourceKind { get; init; } = string.Empty;
    public string LicenseKind { get; init; } = string.Empty;
    public string ProvenanceStatus { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public string DeclaredMediaSlotId { get; init; } = string.Empty;
    public string RelativeFixturePath { get; init; } = string.Empty;
    public string Attribution { get; init; } = string.Empty;
    public string ProviderModelRunMetadata { get; init; } = string.Empty;
    public bool ClaimsFinalArtworkOrProse { get; init; }
    public bool ClaimsProviderLlmRagCall { get; init; }
    public bool ClaimsRuntimeUiUnityGamePackageMutation { get; init; }
    public string ExpectedReviewDecision { get; init; } = string.Empty;
}

public sealed record MediaCandidateQuarantine
{
    public string SchemaVersion { get; init; } = MediaAssetCampaignVocabulary.CandidateQuarantineSchemaVersion;
    public string GoalId { get; init; } = MediaAssetCampaignVocabulary.GoalId;
    public bool Passed { get; init; }
    public IReadOnlyList<MediaCandidateRecord> Candidates { get; init; } = [];
}

public sealed record MediaReviewDecisionRecord
{
    public string DecisionId { get; init; } = string.Empty;
    public string CandidateId { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public string Decision { get; init; } = string.Empty;
    public string CauseCode { get; init; } = string.Empty;
    public bool Promoted { get; init; }
    public string PromotionScope { get; init; } = string.Empty;
    public IReadOnlyList<MediaCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaReviewPromotionLedger
{
    public string SchemaVersion { get; init; } = MediaAssetCampaignVocabulary.ReviewLedgerSchemaVersion;
    public string GoalId { get; init; } = MediaAssetCampaignVocabulary.GoalId;
    public bool Deterministic { get; init; }
    public bool Passed { get; init; }
    public int PromotedFixtureCount { get; init; }
    public IReadOnlyList<MediaReviewDecisionRecord> Decisions { get; init; } = [];
    public IReadOnlyList<MediaCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaFixtureFileRecord
{
    public string FixtureId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public long ByteLength { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public string BoundRequestId { get; init; } = string.Empty;
    public string BoundGeneratedTargetId { get; init; } = string.Empty;
    public string FixtureStatus { get; init; } = "fixture_asset_only";
}

public sealed record MediaFixtureFileInventory
{
    public string SchemaVersion { get; init; } = MediaAssetCampaignVocabulary.FixtureInventorySchemaVersion;
    public string GoalId { get; init; } = MediaAssetCampaignVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FixtureFileCount { get; init; }
    public IReadOnlyList<MediaFixtureFileRecord> Files { get; init; } = [];
    public IReadOnlyList<MediaCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaFixtureFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public string Contents { get; init; } = string.Empty;
}

public sealed record MediaBindingRecord
{
    public string BindingId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public string CandidateId { get; init; } = string.Empty;
    public string MediaSlotId { get; init; } = string.Empty;
    public string MediaKind { get; init; } = string.Empty;
    public string GeneratedTargetId { get; init; } = string.Empty;
    public string TargetArtifactKind { get; init; } = string.Empty;
    public string FixtureRelativePath { get; init; } = string.Empty;
    public string FixtureSha256 { get; init; } = string.Empty;
    public bool FixtureOnlyNotFinalMedia { get; init; } = true;
}

public sealed record MediaFallbackRecord
{
    public string FamilyId { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public string MediaSlotId { get; init; } = string.Empty;
    public string FallbackBehavior { get; init; } = string.Empty;
}

public sealed record MediaBindingManifest
{
    public string SchemaVersion { get; init; } = MediaAssetCampaignVocabulary.BindingManifestSchemaVersion;
    public string GoalId { get; init; } = MediaAssetCampaignVocabulary.GoalId;
    public bool Passed { get; init; }
    public int BindingCount { get; init; }
    public IReadOnlyList<MediaBindingRecord> Bindings { get; init; } = [];
    public IReadOnlyList<MediaFallbackRecord> Fallbacks { get; init; } = [];
    public IReadOnlyList<MediaCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PreviewExportMediaPayloadFamilySummary
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string RuntimePreviewPayloadRef { get; init; } = string.Empty;
    public string ExportProfileId { get; init; } = string.Empty;
    public int BindingCount { get; init; }
    public int ImageLikeFixtureBindingCount { get; init; }
    public int AudioLikeFixtureBindingCount { get; init; }
    public int UiOrBundleFixtureBindingCount { get; init; }
    public bool HasImageLikeFixtureBinding { get; init; }
    public bool HasAudioLikeFixtureBinding { get; init; }
    public bool ExplicitFallbackForUnfilledSlots { get; init; }
    public bool PackageRuntimeExportPayloadsMutated { get; init; }
    public bool GamePackageSchemaChanged { get; init; }
    public bool RuntimeChanged { get; init; }
    public bool UnityExportModified { get; init; }
}

public sealed record PreviewExportMediaPayloads
{
    public string SchemaVersion { get; init; } = MediaAssetCampaignVocabulary.PreviewExportPayloadSchemaVersion;
    public string GoalId { get; init; } = MediaAssetCampaignVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public bool EveryFamilyHasMediaBindings { get; init; }
    public bool EveryFamilyHasImageAndAudioFixtureBindings { get; init; }
    public bool PackageRuntimeExportPayloadsMutated { get; init; }
    public bool GamePackageSchemaChanged { get; init; }
    public bool UnityExportModified { get; init; }
    public IReadOnlyList<PreviewExportMediaPayloadFamilySummary> Families { get; init; } = [];
    public IReadOnlyList<MediaCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidMediaScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<MediaCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidMediaDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = MediaAssetCampaignVocabulary.InvalidMatrixSchemaVersion;
    public string GoalId { get; init; } = MediaAssetCampaignVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public int BlockedCount { get; init; }
    public IReadOnlyList<InvalidMediaScenario> Scenarios { get; init; } = [];
}

public sealed record MediaAssetCampaignReport
{
    public string SchemaVersion { get; init; } = MediaAssetCampaignVocabulary.ReportSchemaVersion;
    public string GoalId { get; init; } = MediaAssetCampaignVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = MediaAssetCampaignVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = MediaAssetCampaignVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int FamilyCount { get; init; }
    public int RequestCount { get; init; }
    public int FixtureFileCount { get; init; }
    public int BindingCount { get; init; }
    public bool Goal047AcceptedByUserHandoff { get; init; }
    public bool CatalogPassed { get; init; }
    public bool RequestQueuePassed { get; init; }
    public bool LicenseLedgerPassed { get; init; }
    public bool ReviewPromotionPassed { get; init; }
    public bool FixtureInventoryPassed { get; init; }
    public bool BindingManifestPassed { get; init; }
    public bool PreviewExportPayloadsPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool FixtureMediaProduced { get; init; }
    public bool RealProviderCalled { get; init; }
    public bool RealMediaGenerationCalled { get; init; }
    public bool NetworkOrImportCalled { get; init; }
    public bool GamePackageSchemaChanged { get; init; }
    public bool RuntimeUiUnityChanged { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string SlotCatalogHash { get; init; } = string.Empty;
    public string RequestQueueHash { get; init; } = string.Empty;
    public string LicenseLedgerHash { get; init; } = string.Empty;
    public string ReviewLedgerHash { get; init; } = string.Empty;
    public string FixtureInventoryHash { get; init; } = string.Empty;
    public string BindingManifestHash { get; init; } = string.Empty;
    public string PreviewExportPayloadsHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<MediaCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record MediaAssetCampaignEvidenceResult
{
    public MediaCampaignSourceManifest SourceManifest { get; init; } = new();
    public MediaSlotCatalog SlotCatalog { get; init; } = new();
    public MediaRequestQueue RequestQueue { get; init; } = new();
    public MediaStylePolicy StylePolicy { get; init; } = new();
    public MediaLicenseProvenanceLedger LicenseLedger { get; init; } = new();
    public MediaCandidateQuarantine CandidateQuarantine { get; init; } = new();
    public MediaReviewPromotionLedger ReviewPromotionLedger { get; init; } = new();
    public MediaFixtureFileInventory FixtureInventory { get; init; } = new();
    public MediaBindingManifest BindingManifest { get; init; } = new();
    public PreviewExportMediaPayloads PreviewExportPayloads { get; init; } = new();
    public InvalidMediaDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public MediaAssetCampaignReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<MediaFixtureFilePayload> FixtureFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record MediaAssetCampaignWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public string ReportMarkdownPath { get; init; } = string.Empty;
}
