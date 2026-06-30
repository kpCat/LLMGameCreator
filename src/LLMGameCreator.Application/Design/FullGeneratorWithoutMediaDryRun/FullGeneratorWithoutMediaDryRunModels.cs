using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;

namespace LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;

public static class FullGeneratorWithoutMediaDryRunVocabulary
{
    public const string GoalId = "goal_047_full_generator_without_media_dry_run";
    public const string ScenarioId = "goal-047-full-generator-without-media-dry-run";
    public const string ProductSmokeRoute = "goal-047-full-generator-without-media-dry-run";
    public const string FinalGate = "full_generator_without_media_verification";
    public const string MediaPolicy = "without_media";
    public const string SourceManifestSchemaVersion = "full_generator_dry_run_source_manifest_v1";
    public const string ReviewLedgerSchemaVersion = "full_generator_review_promotion_ledger_v1";
    public const string RepairMatrixSchemaVersion = "full_generator_repair_diagnostics_matrix_v1";
    public const string FamilyDryRunSchemaVersion = "full_generator_family_dry_run_v1";
    public const string RuntimePreviewMatrixSchemaVersion = "full_generator_runtime_preview_validation_matrix_v1";
    public const string ExportProfileMatrixSchemaVersion = "full_generator_export_profile_selection_matrix_v1";
    public const string PackageProofSchemaVersion = "full_generator_package_compatibility_or_materialization_summary_v1";
    public const string OneClickSummarySchemaVersion = "full_generator_one_click_dry_run_summary_v1";
    public const string InvalidMatrixSchemaVersion = "full_generator_invalid_fake_leak_matrix_v1";

    public static readonly IReadOnlyList<string> FamilyIds = MultiFamilyGeneratedTemplateVocabulary.FamilyIds;

    public static readonly IReadOnlyDictionary<string, string> ScenarioByFamilyId =
        MultiFamilyGeneratedTemplateVocabulary.ScenarioByFamilyId;

    public static readonly IReadOnlyList<string> ReviewStates =
    [
        "candidate_loaded",
        "validated",
        "repair_required",
        "approved_for_dry_run",
        "promoted_to_preview_payload",
        "promoted_to_export_candidate",
        "blocked",
        "rejected"
    ];

    public static readonly IReadOnlyList<string> RequiredRepairDiagnostics =
    [
        "missing_source_artifact",
        "hash_mismatch",
        "missing_family_loop",
        "missing_runtime_preview_payload",
        "missing_export_profile",
        "unresolved_profile_capability_ref",
        "rejected_candidate_provenance",
        "final_prose_leakage",
        "provider_llm_rag_leakage",
        "media_leakage",
        "unity_runtime_source_mutation_claim",
        "gamepackage_schema_mutation_claim",
        "nondeterministic_ordering",
        "cross_family_leakage"
    ];

    public static readonly IReadOnlyList<string> GeneratedSystemIds =
    [
        "world",
        "entity",
        "quest",
        "dialogue",
        "item",
        "economy",
        "combat",
        "progression",
        "settlement",
        "event"
    ];
}

public sealed record FullGeneratorBoundaryClaims
{
    public bool FinalProsePromotedAsContent { get; init; }
    public bool ProviderLlmRagCalled { get; init; }
    public bool MediaGenerated { get; init; }
    public bool RuntimeSourceChanged { get; init; }
    public bool RuntimeAbstractionsChanged { get; init; }
    public bool WinFormsUiChanged { get; init; }
    public bool UnityExecuted { get; init; }
    public bool UnitySourceChanged { get; init; }
    public bool GamePackageSchemaMutation { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool ExternalDependencyAdded { get; init; }
    public bool UnsafeAbsolutePathClaim { get; init; }

    [JsonIgnore]
    public bool AllFalse =>
        !FinalProsePromotedAsContent &&
        !ProviderLlmRagCalled &&
        !MediaGenerated &&
        !RuntimeSourceChanged &&
        !RuntimeAbstractionsChanged &&
        !WinFormsUiChanged &&
        !UnityExecuted &&
        !UnitySourceChanged &&
        !GamePackageSchemaMutation &&
        !GeneratorLibraryChanged &&
        !ExternalDependencyAdded &&
        !UnsafeAbsolutePathClaim;
}

public sealed record FullGeneratorSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactFileName { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
}

public sealed record FullGeneratorSourceBundle
{
    public FamilyTemplateCatalog Goal043Catalog { get; init; } = new();
    public SharedLifecycleContract Goal043SharedLifecycleContract { get; init; } = new();
    public IReadOnlyDictionary<string, FamilyLifecyclePlan> Goal043PlansByFamilyId { get; init; } = new SortedDictionary<string, FamilyLifecyclePlan>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, FamilySimulatableLoopProof> Goal043ProofsByFamilyId { get; init; } = new SortedDictionary<string, FamilySimulatableLoopProof>(StringComparer.Ordinal);
    public PreviewExportConsumptionMatrix Goal043PreviewExportMatrix { get; init; } = new();
    public MultiFamilyRegressionMatrix Goal043RegressionMatrix { get; init; } = new();
    public ChunkedExportManifest Goal040ExportManifest { get; init; } = new();
    public RuntimePreviewConsumptionProof Goal040RuntimePreviewConsumptionProof { get; init; } = new();
    public IReadOnlyDictionary<string, ChunkedPreviewPayload> Goal040PayloadsByScenario { get; init; } = new SortedDictionary<string, ChunkedPreviewPayload>(StringComparer.Ordinal);
    public IReadOnlyList<FullGeneratorSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyDictionary<string, string> ArtifactHashByRelativePath { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ArtifactTextByRelativePath { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record FullGeneratorGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record FullGeneratorFamilySourceSummary
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string Goal043PlanRef { get; init; } = string.Empty;
    public string Goal043PlanHash { get; init; } = string.Empty;
    public string Goal043LoopProofRef { get; init; } = string.Empty;
    public string Goal043LoopProofHash { get; init; } = string.Empty;
    public string Goal040PayloadRef { get; init; } = string.Empty;
    public string Goal040PayloadHash { get; init; } = string.Empty;
    public int RuntimeDeltaMarkerCount { get; init; }
    public int StateChangingEventCount { get; init; }
}

public sealed record FullGeneratorDryRunManifest
{
    public string SchemaVersion { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.SourceManifestSchemaVersion;
    public string GoalId { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string MediaPolicy { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.MediaPolicy;
    public string DeterministicOrderingKey { get; init; } = "001-goal-047-full-generator-without-media";
    public IReadOnlyList<FullGeneratorGateRecord> AcceptedPreflightGates { get; init; } = [];
    public IReadOnlyList<string> SelectedFamilyIds { get; init; } = [];
    public IReadOnlyList<string> ProfileCapabilityRefs { get; init; } = [];
    public IReadOnlyList<string> SelectedWorldChunkRuntimeRefs { get; init; } = [];
    public IReadOnlyList<string> SelectedTemplateLoopRefs { get; init; } = [];
    public IReadOnlyList<string> SelectedDraftLuaExpansionRefs { get; init; } = [];
    public IReadOnlyList<FullGeneratorFamilySourceSummary> FamilySourceSummaries { get; init; } = [];
    public IReadOnlyList<FullGeneratorSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public FullGeneratorBoundaryClaims BoundaryClaims { get; init; } = new();
    public IReadOnlyList<FullGeneratorDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullGeneratorReviewTransitionRule
{
    public string FromState { get; init; } = string.Empty;
    public string ToState { get; init; } = string.Empty;
    public bool Terminal { get; init; }
}

public sealed record FullGeneratorReviewTransitionRecord
{
    public string TransitionId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SourceArtifactId { get; init; } = string.Empty;
    public string BeforeState { get; init; } = string.Empty;
    public string AfterState { get; init; } = string.Empty;
    public string RequiredEvidenceHash { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string PromotionDecision { get; init; } = string.Empty;
    public IReadOnlyList<FullGeneratorDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullGeneratorReviewPromotionLedger
{
    public string SchemaVersion { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.ReviewLedgerSchemaVersion;
    public string GoalId { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.GoalId;
    public IReadOnlyList<string> States { get; init; } = [];
    public IReadOnlyList<FullGeneratorReviewTransitionRule> TransitionTable { get; init; } = [];
    public IReadOnlyList<FullGeneratorReviewTransitionRecord> Transitions { get; init; } = [];
    public bool Deterministic { get; init; }
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public int TransitionCount { get; init; }
    public IReadOnlyList<FullGeneratorDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullGeneratorRepairDiagnosticRow
{
    public string DiagnosticId { get; init; } = string.Empty;
    public string NormalizedCode { get; init; } = string.Empty;
    public string Severity { get; init; } = "error";
    public string RepairActionKind { get; init; } = string.Empty;
    public string BoundedRepairAction { get; init; } = string.Empty;
    public bool ManualRequired { get; init; }
    public bool MutatesHistoricalArtifacts { get; init; }
    public string Decision { get; init; } = string.Empty;
}

public sealed record FullGeneratorRepairDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.RepairMatrixSchemaVersion;
    public bool Passed { get; init; }
    public int DiagnosticCount { get; init; }
    public int ManualRequiredCount { get; init; }
    public int BoundedRepairCount { get; init; }
    public IReadOnlyList<FullGeneratorRepairDiagnosticRow> Rows { get; init; } = [];
}

public sealed record FullGeneratorSystemCoverageRow
{
    public string SystemId { get; init; } = string.Empty;
    public string CoverageStatus { get; init; } = string.Empty;
    public string SourceRef { get; init; } = string.Empty;
    public string PackageCompatibilityTarget { get; init; } = string.Empty;
}

public sealed record FullGeneratorRuntimePreviewPayloadSummary
{
    public string PayloadRelativePath { get; init; } = string.Empty;
    public string PayloadHash { get; init; } = string.Empty;
    public bool StableRelativeRefs { get; init; }
    public bool SourceHashesMatch { get; init; }
    public int ChunkCount { get; init; }
    public int RouteStepCount { get; init; }
    public int RuntimeDeltaMarkerCount { get; init; }
    public bool SaveLoadBacked { get; init; }
    public bool ReplayBacked { get; init; }
}

public sealed record FullGeneratorExportCandidatePayloadSummary
{
    public string ExportProfileId { get; init; } = string.Empty;
    public string ExportMode { get; init; } = string.Empty;
    public string PayloadRelativePath { get; init; } = string.Empty;
    public string PayloadHash { get; init; } = string.Empty;
    public bool DeterministicSelection { get; init; }
    public bool WithoutMedia { get; init; }
}

public sealed record FullGeneratorReplayHashProof
{
    public string SourceLoopProofHash { get; init; } = string.Empty;
    public string ReplayHash { get; init; } = string.Empty;
    public string ReplayedHash { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record FullGeneratorFamilyDryRunRecord
{
    public string SchemaVersion { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.FamilyDryRunSchemaVersion;
    public string GoalId { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.GoalId;
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string DeterministicOrderingKey { get; init; } = string.Empty;
    public IReadOnlyList<string> FamilyProfileRefs { get; init; } = [];
    public IReadOnlyList<string> ScenarioFamilyLensRefs { get; init; } = [];
    public IReadOnlyList<string> RegionChunkRuntimeTraversalRefs { get; init; } = [];
    public IReadOnlyList<string> ReviewPromotionLedgerRefs { get; init; } = [];
    public IReadOnlyList<FullGeneratorSystemCoverageRow> GeneratedSystemCoverage { get; init; } = [];
    public FullGeneratorRuntimePreviewPayloadSummary RuntimePreviewPayloadSummary { get; init; } = new();
    public FullGeneratorExportCandidatePayloadSummary ExportCandidatePayloadSummary { get; init; } = new();
    public string PackageCompatibilityOrMaterializationSummaryRef { get; init; } = "package-compatibility-or-materialization-summary.json";
    public FullGeneratorReplayHashProof ReplayHashProof { get; init; } = new();
    public bool StateChangingLoopProof { get; init; }
    public FullGeneratorBoundaryClaims BoundaryClaims { get; init; } = new();
    public IReadOnlyList<FullGeneratorSourceArtifactReference> SourceRefs { get; init; } = [];
    public IReadOnlyList<FullGeneratorDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullGeneratorRuntimePreviewValidationRow
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string PayloadRelativePath { get; init; } = string.Empty;
    public bool StableRelativeRefs { get; init; }
    public bool SourceHashesMatch { get; init; }
    public bool CommandStateTransitionsConsistent { get; init; }
    public bool ChunkWindowRefsWithinBounds { get; init; }
    public bool ExportProfileDeterministic { get; init; }
    public bool NoLeakClaims { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<FullGeneratorDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullGeneratorRuntimePreviewValidationMatrix
{
    public string SchemaVersion { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.RuntimePreviewMatrixSchemaVersion;
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public IReadOnlyList<FullGeneratorRuntimePreviewValidationRow> Rows { get; init; } = [];
}

public sealed record FullGeneratorExportProfileSelectionRow
{
    public string FamilyId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ExportProfileId { get; init; } = string.Empty;
    public string PresentationMode { get; init; } = string.Empty;
    public string PayloadRelativePath { get; init; } = string.Empty;
    public string PayloadHash { get; init; } = string.Empty;
    public bool WithoutMedia { get; init; }
    public bool DeterministicSelection { get; init; }
    public bool RuntimePreviewCompatible { get; init; }
    public bool UnityExportCompatible { get; init; }
    public bool Passed { get; init; }
}

public sealed record FullGeneratorExportProfileSelectionMatrix
{
    public string SchemaVersion { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.ExportProfileMatrixSchemaVersion;
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public string DeterministicOrderingKey { get; init; } = "export-profile-selection/by-family-id-v1";
    public IReadOnlyList<FullGeneratorExportProfileSelectionRow> Rows { get; init; } = [];
}

public sealed record FullGeneratorPackageCompatibilityRow
{
    public string FamilyId { get; init; } = string.Empty;
    public string SystemId { get; init; } = string.Empty;
    public string DryRunSource { get; init; } = string.Empty;
    public string ExistingPackageTarget { get; init; } = string.Empty;
    public string ExistingAssemblerOrValidator { get; init; } = string.Empty;
    public string CompatibilityStatus { get; init; } = string.Empty;
    public bool DirectMaterializationSafeNow { get; init; }
}

public sealed record FullGeneratorPackageCompatibilitySummary
{
    public string SchemaVersion { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.PackageProofSchemaVersion;
    public bool PackageMaterializationAttempted { get; init; }
    public bool MaterializedValidatorCleanPackages { get; init; }
    public bool CompatibilityProofPassed { get; init; }
    public string ProofMode { get; init; } = "strict_package_compatibility_proof";
    public string DirectMaterializationSafetyDecision { get; init; } = string.Empty;
    public IReadOnlyList<string> ExistingAssemblersAndValidators { get; init; } = [];
    public IReadOnlyList<FullGeneratorPackageCompatibilityRow> Rows { get; init; } = [];
    public IReadOnlyList<FullGeneratorDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullGeneratorOneClickDryRunSummary
{
    public string SchemaVersion { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.OneClickSummarySchemaVersion;
    public string GoalId { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string Status { get; init; } = string.Empty;
    public int FamilyCount { get; init; }
    public int EvidenceFileCount { get; init; }
    public bool ReviewPromotionPassed { get; init; }
    public bool RepairDiagnosticsPassed { get; init; }
    public bool RuntimePreviewValidationPassed { get; init; }
    public bool ExportProfileSelectionPassed { get; init; }
    public bool PackageProofPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool MediaGenerated { get; init; }
    public bool ProviderCalled { get; init; }
    public bool UnityExecuted { get; init; }
    public bool RuntimeSourceChanged { get; init; }
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<string> EvidenceFiles { get; init; } = [];
}

public sealed record FullGeneratorInvalidMatrix
{
    public string SchemaVersion { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.InvalidMatrixSchemaVersion;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public int BlockedCount { get; init; }
    public IReadOnlyList<FullGeneratorInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record FullGeneratorInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<FullGeneratorDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullGeneratorWithoutMediaReport
{
    public string SchemaVersion { get; init; } = "full_generator_without_media_report_v1";
    public string GoalId { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = FullGeneratorWithoutMediaDryRunVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int FamilyCount { get; init; }
    public bool Goal043AcceptedByUserHandoff { get; init; }
    public string Goal043AcceptedGate { get; init; } = "multi_family_generated_template_vertical_slice_verification passed";
    public bool ReviewPromotionPassed { get; init; }
    public bool RepairDiagnosticsPassed { get; init; }
    public bool RuntimePreviewValidationPassed { get; init; }
    public bool ExportProfileSelectionPassed { get; init; }
    public bool PackageProofPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool MediaGenerated { get; init; }
    public bool ProviderCalled { get; init; }
    public bool UnityExecuted { get; init; }
    public bool RuntimeSourceChanged { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string ReviewLedgerHash { get; init; } = string.Empty;
    public string RepairMatrixHash { get; init; } = string.Empty;
    public string RuntimePreviewMatrixHash { get; init; } = string.Empty;
    public string ExportProfileMatrixHash { get; init; } = string.Empty;
    public string PackageProofHash { get; init; } = string.Empty;
    public string OneClickSummaryHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<FullGeneratorDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullGeneratorWithoutMediaDryRunEvidenceResult
{
    public FullGeneratorDryRunManifest SourceManifest { get; init; } = new();
    public FullGeneratorReviewPromotionLedger ReviewPromotionLedger { get; init; } = new();
    public FullGeneratorRepairDiagnosticsMatrix RepairDiagnosticsMatrix { get; init; } = new();
    public IReadOnlyList<FullGeneratorFamilyDryRunRecord> FamilyDryRuns { get; init; } = [];
    public FullGeneratorRuntimePreviewValidationMatrix RuntimePreviewValidationMatrix { get; init; } = new();
    public FullGeneratorExportProfileSelectionMatrix ExportProfileSelectionMatrix { get; init; } = new();
    public FullGeneratorPackageCompatibilitySummary PackageCompatibilitySummary { get; init; } = new();
    public FullGeneratorOneClickDryRunSummary OneClickSummary { get; init; } = new();
    public FullGeneratorInvalidMatrix InvalidMatrix { get; init; } = new();
    public FullGeneratorWithoutMediaReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record FullGeneratorWithoutMediaDryRunWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record FullGeneratorDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static FullGeneratorDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static FullGeneratorDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static FullGeneratorDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}
