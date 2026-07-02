namespace LLMGameCreator.Application.Design.VisualAssetContractRatingMetadata;

public static class VisualAssetContractRatingMetadataVocabulary
{
    public const string GoalId = "goal_084_visual_asset_contract_rating_metadata";
    public const string ProductSmokeRoute = "goal-084-visual-asset-contract-rating-metadata";
    public const string FinalGate = "visual_asset_contract_rating_metadata_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-084-visual-asset-contract-rating-metadata";

    public const string ContractSchemaVersion = "visual_asset_contract_rating_metadata_v1";
    public const string CatalogSchemaVersion = "visual_asset_contract_catalog_v1";
    public const string RatingPolicySchemaVersion = "visual_rating_policy_matrix_v1";
    public const string ValidationMatrixSchemaVersion = "visual_contract_validation_matrix_v1";
    public const string NegativeProofSchemaVersion = "visual_contract_negative_proof_v1";
    public const string SourceLineageSchemaVersion = "visual_contract_source_document_lineage_v1";
    public const string QualityGateSchemaVersion = "visual_contract_quality_gate_scan_v1";
}

public enum VisualRating
{
    Unspecified = 0,
    Safe,
    Suggestive,
    AdultNudeReference,
    AdultEroticScene,
    AdultPrivateExplicit
}

public enum VisualExportPolicy
{
    Unspecified = 0,
    PublicSafe,
    MatureOptional,
    AdultBuildOnly,
    PrivateLocalOnly,
    Blocked
}

public enum VisualReviewStatus
{
    Unspecified = 0,
    CandidateQuarantined,
    ApprovedSafe,
    ApprovedAdult,
    Rejected
}

public enum VisualProviderState
{
    None = 0,
    MetadataOnly,
    CandidateQuarantine,
    ApprovedAsset,
    Rejected
}

public enum VisualBodyPlanEligibility
{
    Unspecified = 0,
    SafeOnly,
    AdultEligibleHumanoidSapient,
    AgeAmbiguous,
    NonSapient,
    NonHumanoidSafeOnly,
    Blocked
}

public sealed record VisualAssetContract
{
    public string SchemaVersion { get; init; } = VisualAssetContractRatingMetadataVocabulary.ContractSchemaVersion;
    public string ContractId { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
    public bool StrictReferenceValidation { get; init; } = true;
    public IReadOnlyList<VisualAssetRecipeRef> RecipeRefs { get; init; } = [];
    public IReadOnlyList<VisualPartPackRef> PartPackRefs { get; init; } = [];
    public IReadOnlyList<VisualAssetSlot> Slots { get; init; } = [];
    public IReadOnlyList<VisualCandidateRecord> CandidateRecords { get; init; } = [];
}

public sealed record VisualAssetSlot
{
    public string AssetSlot { get; init; } = string.Empty;
    public VisualRating Rating { get; init; } = VisualRating.Unspecified;
    public bool AdultEnabled { get; init; }
    public bool SafeFallbackRequired { get; init; }
    public bool CandidateQuarantine { get; init; }
    public VisualReviewStatus ReviewStatus { get; init; } = VisualReviewStatus.Unspecified;
    public VisualExportPolicy ExportPolicy { get; init; } = VisualExportPolicy.Unspecified;
    public VisualApprovedAssetRef? ApprovedAssetRef { get; init; }
    public VisualAssetRecipeRef? RecipeRef { get; init; }
    public VisualPartPackRef? PartPackRef { get; init; }
    public VisualSafeFallbackRef? SafeFallbackRef { get; init; }
    public string ProvenanceRef { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
    public VisualBodyPlanEligibility BodyPlanEligibility { get; init; } = VisualBodyPlanEligibility.SafeOnly;
    public VisualBodyPlanEligibilityFacts BodyPlanEligibilityFacts { get; init; } = new();
}

public sealed record VisualAssetRecipeRef
{
    public string RecipeId { get; init; } = string.Empty;
    public string ProvenanceRef { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
}

public sealed record VisualPartPackRef
{
    public string PartPackId { get; init; } = string.Empty;
    public string ProvenanceRef { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
}

public sealed record VisualApprovedAssetRef
{
    public string AssetId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public string ProvenanceRef { get; init; } = string.Empty;
    public VisualRating Rating { get; init; } = VisualRating.Unspecified;
    public VisualReviewStatus ReviewStatus { get; init; } = VisualReviewStatus.Unspecified;
    public VisualExportPolicy ExportPolicy { get; init; } = VisualExportPolicy.Unspecified;
}

public sealed record VisualSafeFallbackRef
{
    public string FallbackId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public string ProvenanceRef { get; init; } = string.Empty;
    public bool Deterministic { get; init; }
    public VisualRating Rating { get; init; } = VisualRating.Safe;
}

public sealed record VisualCandidateRecord
{
    public string CandidateId { get; init; } = string.Empty;
    public string AssetSlot { get; init; } = string.Empty;
    public VisualProviderState ProviderState { get; init; } = VisualProviderState.None;
    public bool CandidateQuarantine { get; init; }
    public VisualReviewStatus ReviewStatus { get; init; } = VisualReviewStatus.Unspecified;
    public bool PromotionRequested { get; init; }
    public VisualRating Rating { get; init; } = VisualRating.Unspecified;
    public VisualExportPolicy ExportPolicy { get; init; } = VisualExportPolicy.Unspecified;
    public string ProvenanceRef { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
    public string SourceOfTruthKind { get; init; } = "metadata_contract";
    public bool PromptTextIsSourceOfTruth { get; init; }
}

public sealed record VisualBodyPlanEligibilityFacts
{
    public bool AdultCharacter { get; init; }
    public bool AgeKnownAdult { get; init; }
    public bool Sapient { get; init; }
    public bool HumanoidCompatible { get; init; }
    public bool AgeAmbiguous { get; init; }
    public bool NonSapient { get; init; }
    public bool FeralOrNonHumanoidSafeOnly { get; init; }
}

public sealed record VisualAssetContractValidationResult
{
    public bool Passed { get; init; }
    public int DiagnosticCount { get; init; }
    public IReadOnlyList<VisualAssetContractDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualAssetContractDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static VisualAssetContractDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static VisualAssetContractDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record VisualAssetContractCatalog
{
    public string SchemaVersion { get; init; } = VisualAssetContractRatingMetadataVocabulary.CatalogSchemaVersion;
    public string GoalId { get; init; } = VisualAssetContractRatingMetadataVocabulary.GoalId;
    public string ManualGate { get; init; } = VisualAssetContractRatingMetadataVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int FixtureCount { get; init; }
    public IReadOnlyList<string> FixtureIds { get; init; } = [];
    public VisualAssetContract Contract { get; init; } = new();
}

public sealed record VisualRatingPolicyMatrix
{
    public string SchemaVersion { get; init; } = VisualAssetContractRatingMetadataVocabulary.RatingPolicySchemaVersion;
    public string GoalId { get; init; } = VisualAssetContractRatingMetadataVocabulary.GoalId;
    public IReadOnlyList<VisualRatingPolicyRow> Rows { get; init; } = [];
}

public sealed record VisualRatingPolicyRow
{
    public VisualRating Rating { get; init; }
    public IReadOnlyList<VisualExportPolicy> AllowedExportPolicies { get; init; } = [];
    public bool AdultEnabledAllowed { get; init; }
    public bool PublicExportAllowed { get; init; }
    public bool SafeFallbackRequiredWhenAdultEnabled { get; init; }
    public string Boundary { get; init; } = string.Empty;
}

public sealed record VisualContractValidationMatrix
{
    public string SchemaVersion { get; init; } = VisualAssetContractRatingMetadataVocabulary.ValidationMatrixSchemaVersion;
    public string GoalId { get; init; } = VisualAssetContractRatingMetadataVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FixtureCount { get; init; }
    public IReadOnlyList<VisualContractValidationRow> Rows { get; init; } = [];
}

public sealed record VisualContractValidationRow
{
    public string FixtureId { get; init; } = string.Empty;
    public bool AdultEnabled { get; init; }
    public VisualRating Rating { get; init; }
    public VisualExportPolicy ExportPolicy { get; init; }
    public bool SafeFallbackRequired { get; init; }
    public bool HasApprovedAssetRef { get; init; }
    public bool HasDeterministicSafeFallback { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<VisualAssetContractDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualContractNegativeProof
{
    public string SchemaVersion { get; init; } = VisualAssetContractRatingMetadataVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } = VisualAssetContractRatingMetadataVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<VisualContractNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record VisualContractNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<VisualAssetContractDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualSourceDocumentLineage
{
    public string SchemaVersion { get; init; } = VisualAssetContractRatingMetadataVocabulary.SourceLineageSchemaVersion;
    public string GoalId { get; init; } = VisualAssetContractRatingMetadataVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal083ArtifactsGreen { get; init; }
    public bool Goal083AcceptedFalse { get; init; }
    public bool Goal083FutureGateRouted { get; init; }
    public bool Goal082aP0P1SourceFormatEvidenceInactive { get; init; }
    public IReadOnlyList<VisualSourceDocumentLineageRecord> Records { get; init; } = [];
}

public sealed record VisualSourceDocumentLineageRecord
{
    public string Path { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> PurposeTags { get; init; } = [];
}

public sealed record VisualContractQualityGateScan
{
    public string SchemaVersion { get; init; } = VisualAssetContractRatingMetadataVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } = VisualAssetContractRatingMetadataVocabulary.GoalId;
    public string ManualGate { get; init; } = VisualAssetContractRatingMetadataVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Goal083LineagePassed { get; init; }
    public bool ValidFixturesPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool NoPublicGamePackageSchemaChanged { get; init; } = true;
    public bool NoRuntimeChanged { get; init; } = true;
    public bool NoUnityChanged { get; init; } = true;
    public bool NoProviderOrLlmOrRagOrMediaExecution { get; init; } = true;
    public bool NoLuaOrGeneratorLibraryChanged { get; init; } = true;
    public bool NoProjectFilesChanged { get; init; } = true;
    public bool NoBinaryMediaAdded { get; init; } = true;
    public bool NoGeneratedImageAssetsAdded { get; init; } = true;
    public bool NoRealAdultFixturesAdded { get; init; } = true;
    public bool NoExplicitPromptDumpAdded { get; init; } = true;
    public bool ArtifactScopeReady { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<VisualAssetContractDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualAssetContractRatingMetadataReport
{
    public string GoalId { get; init; } = VisualAssetContractRatingMetadataVocabulary.GoalId;
    public string ManualGate { get; init; } = VisualAssetContractRatingMetadataVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool ContractModelsImplemented { get; init; }
    public bool ValidatorImplemented { get; init; }
    public bool FixturesImplemented { get; init; }
    public bool ValidFixturesPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool Goal083LineagePassed { get; init; }
    public int FixtureCount { get; init; }
    public int NegativeScenarioCount { get; init; }
    public string CatalogHash { get; init; } = string.Empty;
    public string RatingPolicyHash { get; init; } = string.Empty;
    public string ValidationMatrixHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record VisualAssetContractRatingMetadataEvidenceResult
{
    public VisualAssetContractCatalog Catalog { get; init; } = new();
    public VisualRatingPolicyMatrix RatingPolicyMatrix { get; init; } = new();
    public VisualContractValidationMatrix ValidationMatrix { get; init; } = new();
    public VisualContractNegativeProof NegativeProof { get; init; } = new();
    public VisualSourceDocumentLineage SourceDocumentLineage { get; init; } = new();
    public VisualContractQualityGateScan QualityGateScan { get; init; } = new();
    public VisualAssetContractRatingMetadataReport Report { get; init; } = new();
    public string CatalogJson { get; init; } = string.Empty;
    public string RatingPolicyMatrixJson { get; init; } = string.Empty;
    public string ValidationMatrixJson { get; init; } = string.Empty;
    public string NegativeProofJson { get; init; } = string.Empty;
    public string SourceDocumentLineageJson { get; init; } = string.Empty;
    public string QualityGateScanJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record VisualAssetContractRatingMetadataWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string CatalogJsonPath { get; init; } = string.Empty;
    public string RatingPolicyMatrixJsonPath { get; init; } = string.Empty;
    public string ValidationMatrixJsonPath { get; init; } = string.Empty;
    public string NegativeProofJsonPath { get; init; } = string.Empty;
    public string SourceDocumentLineageJsonPath { get; init; } = string.Empty;
    public string QualityGateScanJsonPath { get; init; } = string.Empty;
}
