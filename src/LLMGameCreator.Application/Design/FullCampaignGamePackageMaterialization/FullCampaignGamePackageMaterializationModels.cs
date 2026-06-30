using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.FullCampaignGamePackageMaterialization;

public static class FullCampaignGamePackageMaterializationVocabulary
{
    public const string GoalId = "goal_060_full_campaign_gamepackage_materialization_matrix";
    public const string ProductSmokeRoute = "goal-060-full-campaign-gamepackage-materialization-matrix";
    public const string FinalGate = "full_campaign_gamepackage_materialization_matrix_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix";
    public const string Goal059RelativeOutputDirectory = ".llmgc/procedural/goal-059-full-generator-variability-regression-matrix";
    public const string StagingRoot = "staging";
    public const string UnityPackageCommandPlanStagingRelativePath = "package-materialization/unity-package-consumption-command-plan.json";

    public const string SourceManifestSchemaVersion = "full_campaign_gamepackage_materialization_source_manifest_v1";
    public const string PackagePlanSchemaVersion = "full_campaign_gamepackage_materialization_plan_v1";
    public const string PackageInventorySchemaVersion = "full_campaign_gamepackage_inventory_v1";
    public const string PackageValidationMatrixSchemaVersion = "full_campaign_gamepackage_validation_matrix_v1";
    public const string RuntimeConsumptionMatrixSchemaVersion = "full_campaign_gamepackage_runtime_consumption_matrix_v1";
    public const string PreviewExportSchemaVersion = "full_campaign_gamepackage_preview_export_payloads_v1";
    public const string UnityCommandPlanSchemaVersion = "full_campaign_gamepackage_unity_command_plan_v1";
    public const string UnityProofSchemaVersion = "full_campaign_gamepackage_unity_proof_v1";
    public const string InvalidMatrixSchemaVersion = "invalid_full_campaign_gamepackage_materialization_matrix_v1";
    public const string ReportSchemaVersion = "full_campaign_gamepackage_materialization_report_v1";

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
        "missing_goal059_source",
        "stale_goal059_hash",
        "fake_matrix_row_id",
        "duplicate_package_id",
        "invalid_family_id",
        "invalid_seed_id",
        "package_json_malformed",
        "package_validation_failure",
        "package_source_trace_mismatch",
        "schema_mutation_claim",
        "runtime_ui_unity_broad_mutation_claim",
        "provider_network_llm_rag_media_generation_claim",
        "arbitrary_lua_execution_claim",
        "unsafe_path",
        "nondeterministic_ordering",
        "fake_unity_marker",
        "missing_runtime_transition_proof",
        "package_immutability_breach"
    ];
}

public sealed record FullCampaignGamePackageMaterializationOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record FullCampaignGamePackageMaterializationDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static FullCampaignGamePackageMaterializationDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static FullCampaignGamePackageMaterializationDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static FullCampaignGamePackageMaterializationDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record FullCampaignSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<FullCampaignGamePackageMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullCampaignFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record FullCampaignGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record Goal059MatrixRowSource
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourceCampaignHash { get; init; } = string.Empty;
    public string DerivedCampaignHash { get; init; } = string.Empty;
    public string RowRelativePath { get; init; } = string.Empty;
    public string RowHash { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceManifestRefs { get; init; } = [];
    public IReadOnlyList<string> SelectedWorldMapChunkRefs { get; init; } = [];
    public IReadOnlyList<string> SelectedMediaRefs { get; init; } = [];
    public IReadOnlyList<string> SelectedFamilyLoopRefs { get; init; } = [];
    public IReadOnlyList<string> SelectedPreviewExportRefs { get; init; } = [];
    public IReadOnlyList<string> DeterministicMarkerPlan { get; init; } = [];
}

public sealed record FullCampaignSourceBundle
{
    public bool Goal059AcceptedByUserHandoff { get; init; }
    public bool Goal059ReportWasGreenProducedForReview { get; init; }
    public bool Goal059UnityProofPassed { get; init; }
    public string Goal059SourceCampaignHash { get; init; } = string.Empty;
    public string Goal059SeedProfileMatrixHash { get; init; } = string.Empty;
    public string Goal059ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyList<Goal059MatrixRowSource> Rows { get; init; } = [];
    public IReadOnlyList<FullCampaignFilePayload> Goal059StagingFiles { get; init; } = [];
    public IReadOnlyList<FullCampaignSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<FullCampaignGamePackageMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullCampaignSourceManifest
{
    public string SchemaVersion { get; init; } = FullCampaignGamePackageMaterializationVocabulary.SourceManifestSchemaVersion;
    public string GoalId { get; init; } = FullCampaignGamePackageMaterializationVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = FullCampaignGamePackageMaterializationVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = FullCampaignGamePackageMaterializationVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal059AcceptedByUserHandoff { get; init; }
    public bool Goal059ReportWasGreenProducedForReview { get; init; }
    public bool Goal059UnityProofPassed { get; init; }
    public string SourceCampaignHash { get; init; } = string.Empty;
    public string SeedProfileMatrixHash { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<FullCampaignGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<FullCampaignSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<FullCampaignGamePackageMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullCampaignPackagePlanRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourceCampaignHash { get; init; } = string.Empty;
    public string Goal059RowHash { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedPackageAssemblyDomains { get; init; } = [];
    public string ExpectedRuntimeLoopKind { get; init; } = string.Empty;
    public string ExpectedPreviewExportProfile { get; init; } = string.Empty;
    public IReadOnlyList<string> BlockedFutureRequiredGaps { get; init; } = [];
}

public sealed record FullCampaignPackageMaterializationPlan
{
    public string SchemaVersion { get; init; } = FullCampaignGamePackageMaterializationVocabulary.PackagePlanSchemaVersion;
    public string GoalId { get; init; } = FullCampaignGamePackageMaterializationVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<FullCampaignPackagePlanRow> Rows { get; init; } = [];
}

public sealed record FullCampaignMaterializedPackage
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public bool ValidJson { get; init; }
    public bool ValidationPassed { get; init; }
    public int ValidationErrorCount { get; init; }
    public int ValidationWarningCount { get; init; }
    public IReadOnlyList<string> ValidationIssueCodes { get; init; } = [];
    public string Goal059RowHash { get; init; } = string.Empty;
    public GamePackageDefinition Package { get; init; } = new();
    public string PackageJson { get; init; } = string.Empty;
}

public sealed record FullCampaignMaterializedPackageInventory
{
    public string SchemaVersion { get; init; } = FullCampaignGamePackageMaterializationVocabulary.PackageInventorySchemaVersion;
    public string GoalId { get; init; } = FullCampaignGamePackageMaterializationVocabulary.GoalId;
    public bool Passed { get; init; }
    public int PackageCount { get; init; }
    public int DistinctPackageIdCount { get; init; }
    public IReadOnlyList<FullCampaignMaterializedPackageSummary> Packages { get; init; } = [];
}

public sealed record FullCampaignMaterializedPackageSummary
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public bool ValidationPassed { get; init; }
    public string Goal059RowHash { get; init; } = string.Empty;
}

public sealed record FullCampaignPackageValidationMatrix
{
    public string SchemaVersion { get; init; } = FullCampaignGamePackageMaterializationVocabulary.PackageValidationMatrixSchemaVersion;
    public string GoalId { get; init; } = FullCampaignGamePackageMaterializationVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int ValidPackageCount { get; init; }
    public IReadOnlyList<FullCampaignPackageValidationRow> Rows { get; init; } = [];
}

public sealed record FullCampaignPackageValidationRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public bool ValidJson { get; init; }
    public bool ValidationPassed { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
    public IReadOnlyList<string> IssueCodes { get; init; } = [];
}

public sealed record FullCampaignRuntimeCommandSpec
{
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SecondaryTargetId { get; init; } = string.Empty;
    public string InventoryId { get; init; } = string.Empty;
    public double Amount { get; init; }
    public string Value { get; init; } = string.Empty;
}

public sealed record FullCampaignRuntimeRequest
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string ExpectedRuntimeLoopKind { get; init; } = string.Empty;
    public GamePackageDefinition Package { get; init; } = new();
    public IReadOnlyList<FullCampaignRuntimeCommandSpec> Commands { get; init; } = [];
}

public sealed record FullCampaignRuntimeCommandEvidence
{
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public bool Succeeded { get; init; }
    public IReadOnlyList<string> RuntimeEventTypes { get; init; } = [];
    public string DiagnosticCode { get; init; } = string.Empty;
}

public sealed record FullCampaignRuntimeEvidence
{
    public bool RuntimeAttempted { get; init; }
    public bool RuntimeStartSucceeded { get; init; }
    public bool UsedGameRuntimeService { get; init; }
    public bool StateChanged { get; init; }
    public bool FamilySpecificTransitionObserved { get; init; }
    public bool SaveLoadRoundtripPassed { get; init; }
    public string RuntimeStateHash { get; init; } = string.Empty;
    public string RestoredRuntimeStateHash { get; init; } = string.Empty;
    public IReadOnlyList<FullCampaignRuntimeCommandEvidence> Commands { get; init; } = [];
    public IReadOnlyList<string> ChangedStateKeys { get; init; } = [];
    public IReadOnlyList<FullCampaignGamePackageMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public interface IFullCampaignGamePackageMaterializationRuntimeAdapter
{
    FullCampaignRuntimeEvidence Run(FullCampaignRuntimeRequest request);
}

public sealed class MissingFullCampaignRuntimeAdapter : IFullCampaignGamePackageMaterializationRuntimeAdapter
{
    public FullCampaignRuntimeEvidence Run(FullCampaignRuntimeRequest request) =>
        new()
        {
            RuntimeAttempted = false,
            RuntimeStartSucceeded = false,
            UsedGameRuntimeService = false,
            Diagnostics =
            [
                FullCampaignGamePackageMaterializationDiagnostic.Warning("goal060.runtime.adapter_missing", request.RowId, "No Application-layer runtime adapter was supplied.")
            ]
        };
}

public sealed record FullCampaignRuntimeConsumptionMatrix
{
    public string SchemaVersion { get; init; } = FullCampaignGamePackageMaterializationVocabulary.RuntimeConsumptionMatrixSchemaVersion;
    public string GoalId { get; init; } = FullCampaignGamePackageMaterializationVocabulary.GoalId;
    public bool Passed { get; init; }
    public int MaterializedFamilyCount { get; init; }
    public int RuntimePassedFamilyCount { get; init; }
    public IReadOnlyList<FullCampaignRuntimeConsumptionRow> Rows { get; init; } = [];
}

public sealed record FullCampaignRuntimeConsumptionRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string ExpectedRuntimeLoopKind { get; init; } = string.Empty;
    public bool RuntimePassed { get; init; }
    public bool StateChanged { get; init; }
    public bool FamilySpecificTransitionObserved { get; init; }
    public bool SaveLoadRoundtripPassed { get; init; }
    public IReadOnlyList<string> ChangedStateKeys { get; init; } = [];
    public IReadOnlyList<FullCampaignRuntimeCommandEvidence> Commands { get; init; } = [];
    public IReadOnlyList<FullCampaignGamePackageMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullCampaignPreviewExportPackagePayloads
{
    public string SchemaVersion { get; init; } = FullCampaignGamePackageMaterializationVocabulary.PreviewExportSchemaVersion;
    public string GoalId { get; init; } = FullCampaignGamePackageMaterializationVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public bool PackageImmutabilityAuditPassed { get; init; }
    public IReadOnlyList<FullCampaignPreviewExportPackageRow> Rows { get; init; } = [];
}

public sealed record FullCampaignPreviewExportPackageRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public string PreviewPayloadRef { get; init; } = string.Empty;
    public string ExportPayloadRef { get; init; } = string.Empty;
    public string PackageHashBeforePreviewExport { get; init; } = string.Empty;
    public string PackageHashAfterPreviewExport { get; init; } = string.Empty;
    public bool PackageImmutable { get; init; }
    public IReadOnlyList<string> ProvenanceLedger { get; init; } = [];
}

public sealed record FullCampaignUnityPackageCommandRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public bool PackageValidationPassed { get; init; }
    public bool RuntimeLoopCompleted { get; init; }
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record FullCampaignUnityPackageCommandPlan
{
    public string SchemaVersion { get; init; } = FullCampaignGamePackageMaterializationVocabulary.UnityCommandPlanSchemaVersion;
    public string GoalId { get; init; } = FullCampaignGamePackageMaterializationVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = FullCampaignGamePackageMaterializationVocabulary.FinalGate;
    public IReadOnlyList<FullCampaignUnityPackageCommandRow> Rows { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record FullCampaignUnityPlayerProof
{
    public string SchemaVersion { get; init; } = FullCampaignGamePackageMaterializationVocabulary.UnityProofSchemaVersion;
    public string GoalId { get; init; } = FullCampaignGamePackageMaterializationVocabulary.GoalId;
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
    public IReadOnlyList<FullCampaignGamePackageMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullCampaignUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public FullCampaignUnityPlayerProof PlayerProof { get; init; } = new();
    public IReadOnlyList<FullCampaignGamePackageMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidFullCampaignMaterializationScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<FullCampaignGamePackageMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidFullCampaignMaterializationMatrix
{
    public string SchemaVersion { get; init; } = FullCampaignGamePackageMaterializationVocabulary.InvalidMatrixSchemaVersion;
    public string GoalId { get; init; } = FullCampaignGamePackageMaterializationVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<InvalidFullCampaignMaterializationScenario> Scenarios { get; init; } = [];
}

public sealed record FullCampaignGamePackageMaterializationReport
{
    public string SchemaVersion { get; init; } = FullCampaignGamePackageMaterializationVocabulary.ReportSchemaVersion;
    public string GoalId { get; init; } = FullCampaignGamePackageMaterializationVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = FullCampaignGamePackageMaterializationVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = FullCampaignGamePackageMaterializationVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal059AcceptedByUserHandoff { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool PackageMaterializationPlanPassed { get; init; }
    public bool PackageInventoryPassed { get; init; }
    public bool PackageValidationMatrixPassed { get; init; }
    public bool RuntimeConsumptionMatrixPassed { get; init; }
    public bool PreviewExportPackagePayloadsPassed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public bool AllUnityPackageMarkersMatched { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public int MaterializedPackageCount { get; init; }
    public int ValidatorCleanPackageCount { get; init; }
    public int RuntimePassedFamilyCount { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string PackagePlanHash { get; init; } = string.Empty;
    public string PackageInventoryHash { get; init; } = string.Empty;
    public string PackageValidationMatrixHash { get; init; } = string.Empty;
    public string RuntimeConsumptionMatrixHash { get; init; } = string.Empty;
    public string PreviewExportPackagePayloadsHash { get; init; } = string.Empty;
    public string UnityProofHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<FullCampaignGamePackageMaterializationDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record FullCampaignGamePackageMaterializationEvidenceResult
{
    public FullCampaignSourceManifest SourceManifest { get; init; } = new();
    public FullCampaignPackageMaterializationPlan PackageMaterializationPlan { get; init; } = new();
    public FullCampaignMaterializedPackageInventory PackageInventory { get; init; } = new();
    public FullCampaignPackageValidationMatrix PackageValidationMatrix { get; init; } = new();
    public FullCampaignRuntimeConsumptionMatrix RuntimeConsumptionMatrix { get; init; } = new();
    public FullCampaignPreviewExportPackagePayloads PreviewExportPackagePayloads { get; init; } = new();
    public FullCampaignUnityPackageCommandPlan UnityCommandPlan { get; init; } = new();
    public FullCampaignUnityPlayerProof UnityPlayerProof { get; init; } = new();
    public InvalidFullCampaignMaterializationMatrix InvalidMatrix { get; init; } = new();
    public FullCampaignGamePackageMaterializationReport Report { get; init; } = new();
    public IReadOnlyList<FullCampaignMaterializedPackage> Packages { get; init; } = [];
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<FullCampaignFilePayload> StagingFiles { get; init; } = [];
    public string ArtifactScopeReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record FullCampaignGamePackageMaterializationWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public FullCampaignGamePackageMaterializationEvidenceResult Result { get; init; } = new();
}
