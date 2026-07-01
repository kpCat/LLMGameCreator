namespace LLMGameCreator.Application.Design.SettlementConstructionDestructionProductionMatrix;

public static class SettlementConstructionDestructionProductionVocabulary
{
    public const string GoalId = "goal_066_settlement_construction_destruction_production_matrix";
    public const string ProductSmokeRoute = "goal-066-settlement-construction-destruction-production-matrix";
    public const string FinalGate = "settlement_construction_destruction_production_matrix_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-066-settlement-construction-destruction-production-matrix";
    public const string Goal060RelativeOutputDirectory = ".llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix";
    public const string Goal061RelativeOutputDirectory = ".llmgc/procedural/goal-061-full-campaign-playable-review-package-rc";
    public const string Goal062RelativeOutputDirectory = ".llmgc/procedural/goal-062-constrained-spatial-detail-generation";
    public const string Goal063RelativeOutputDirectory = ".llmgc/procedural/goal-063-gameplay-consequence-depth-matrix";
    public const string Goal064RelativeOutputDirectory = ".llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix";
    public const string Goal065RelativeOutputDirectory = ".llmgc/procedural/goal-065-interlocked-gameplay-systems-depth-matrix";
    public const string StagingRoot = "staging";
    public const string UnitySettlementCommandPlanStagingRelativePath = "settlement-construction/unity-settlement-command-plan.json";

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
        "missing_goal065_source",
        "fake_family_id",
        "fake_seed_id",
        "missing_spatial_detail_row",
        "missing_living_world_linkage",
        "missing_interlocked_gameplay_dependency",
        "illegal_building_footprint_or_blocked_placement",
        "insufficient_construction_cost_resources",
        "invalid_production_output",
        "repair_without_damage",
        "destruction_without_affected_structure",
        "missing_save_load_replay_trace",
        "duplicate_settlement_building_id",
        "unsafe_relative_path",
        "nondeterministic_ordering",
        "provider_llm_rag_media_generation_claim",
        "arbitrary_lua_execution_claim",
        "broad_runtime_ui_unity_gamepackage_schema_mutation_claim"
    ];

    public static string FamilyOrderingKey(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "001-map-panel-rpg",
            "survival_sandbox" => "002-survival-sandbox",
            "first_person_grid_dungeon" => "003-first-person-grid-dungeon",
            _ => "999-" + familyId
        };

    public static string SeedOrderingKey(string seedId) =>
        seedId switch
        {
            "seed_alpha" => "001-seed-alpha",
            "seed_beta" => "002-seed-beta",
            "seed_gamma" => "003-seed-gamma",
            _ => "999-" + seedId
        };
}

public sealed record SettlementConstructionOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record SettlementDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static SettlementDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static SettlementDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static SettlementDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record SettlementFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record SettlementSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<SettlementDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SettlementSourceRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourcePackageRowRef { get; init; } = string.Empty;
    public string SourceReviewPackageRowRef { get; init; } = string.Empty;
    public string SourceSpatialDetailRowRef { get; init; } = string.Empty;
    public string SourceGameplayConsequenceRowRef { get; init; } = string.Empty;
    public string SourceLivingWorldRowRef { get; init; } = string.Empty;
    public string SourceInterlockedGameplayRowRef { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public string ReviewPackageRelativePath { get; init; } = string.Empty;
    public string SpatialDetailRowHash { get; init; } = string.Empty;
    public string SpatialVarianceMarker { get; init; } = string.Empty;
    public string LivingWorldRowHash { get; init; } = string.Empty;
    public string LivingWorldAfterStateHash { get; init; } = string.Empty;
    public string InterlockedRowHash { get; init; } = string.Empty;
    public string InterlockedAfterStateHash { get; init; } = string.Empty;
    public bool Goal060RuntimeStateChanged { get; init; }
    public bool Goal061SaveLoadReplayVerified { get; init; }
    public bool Goal062Reachable { get; init; }
    public bool Goal062RouteVerified { get; init; }
    public bool Goal063StateChanging { get; init; }
    public bool Goal064StateChanging { get; init; }
    public bool Goal065StateChanging { get; init; }
    public bool Goal065SaveLoadReplayPassed { get; init; }
    public IReadOnlyList<string> Goal064ActorIds { get; init; } = [];
    public IReadOnlyList<string> Goal064FactionIds { get; init; } = [];
    public IReadOnlyList<string> Goal064EventIds { get; init; } = [];
    public IReadOnlyList<string> Goal065EconomyDeltaIds { get; init; } = [];
    public IReadOnlyList<string> Goal065CraftingDeltaIds { get; init; } = [];
    public IReadOnlyList<string> Goal065CombatDeltaIds { get; init; } = [];
    public IReadOnlyList<string> Goal065ProgressionDeltaIds { get; init; } = [];
    public IReadOnlyList<string> Goal065StatusDeltaIds { get; init; } = [];
}

public sealed record SettlementSourceBundle
{
    public bool Goal065AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewRowsConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal063GameplayRowsConsumed { get; init; }
    public bool Goal064LivingWorldRowsConsumed { get; init; }
    public bool Goal065InterlockedRowsConsumed { get; init; }
    public bool Goal065UnityProofConsumed { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<SettlementSourceRow> Rows { get; init; } = [];
    public IReadOnlyList<SettlementFilePayload> BaseStagingFiles { get; init; } = [];
    public IReadOnlyList<SettlementSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<SettlementDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SettlementGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record SettlementSourceManifest
{
    public string SchemaVersion { get; init; } = "settlement_construction_source_manifest_v1";
    public string GoalId { get; init; } = SettlementConstructionDestructionProductionVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = SettlementConstructionDestructionProductionVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = SettlementConstructionDestructionProductionVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal065AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewRowsConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal063GameplayRowsConsumed { get; init; }
    public bool Goal064LivingWorldRowsConsumed { get; init; }
    public bool Goal065InterlockedRowsConsumed { get; init; }
    public bool Goal065UnityProofConsumed { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<SettlementGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<SettlementSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<SettlementDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SettlementBuildingProfile
{
    public string FamilyId { get; init; } = string.Empty;
    public IReadOnlyList<string> BuildingKinds { get; init; } = [];
    public IReadOnlyList<string> ValidFootprintKinds { get; init; } = [];
    public IReadOnlyList<string> ProductionKinds { get; init; } = [];
    public IReadOnlyList<string> ThreatKinds { get; init; } = [];
    public IReadOnlyList<string> DefenseKinds { get; init; } = [];
}

public sealed record SettlementBuildingCatalog
{
    public string SchemaVersion { get; init; } = "settlement_building_catalog_v1";
    public string GoalId { get; init; } = SettlementConstructionDestructionProductionVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ProfileCount { get; init; }
    public IReadOnlyList<SettlementBuildingProfile> Profiles { get; init; } = [];
}

public sealed record SettlementBuildingSlot
{
    public string SlotId { get; init; } = string.Empty;
    public string FootprintId { get; init; } = string.Empty;
    public int OriginX { get; init; }
    public int OriginY { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public bool PlacementAllowed { get; init; }
    public string SpatialDetailRef { get; init; } = string.Empty;
}

public sealed record SettlementActionRecord
{
    public string ActionId { get; init; } = string.Empty;
    public string ActionKind { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceRefs { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record SettlementResourceDelta
{
    public string ResourceId { get; init; } = string.Empty;
    public int BeforeAmount { get; init; }
    public int Delta { get; init; }
    public int AfterAmount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record SettlementLivingWorldLinkage
{
    public string LinkageId { get; init; } = string.Empty;
    public string SourceLivingWorldRowRef { get; init; } = string.Empty;
    public IReadOnlyList<string> ActorIds { get; init; } = [];
    public IReadOnlyList<string> FactionIds { get; init; } = [];
    public IReadOnlyList<string> EventIds { get; init; } = [];
    public string ConsequenceSummary { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record SettlementInterlockedDependency
{
    public string DependencyId { get; init; } = string.Empty;
    public string SourceInterlockedGameplayRowRef { get; init; } = string.Empty;
    public IReadOnlyList<string> DeltaIds { get; init; } = [];
    public string AfterStateHash { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record SettlementStateSnapshot
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public int StepIndex { get; init; }
    public IReadOnlyDictionary<string, string> Values { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string StateHash { get; init; } = string.Empty;
}

public sealed record SettlementSaveLoadReplayRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public bool BeforeAfterStateChanged { get; init; }
    public bool SaveLoadRoundtripPassed { get; init; }
    public bool ReplayDeterminismPassed { get; init; }
    public string BeforeStateHash { get; init; } = string.Empty;
    public string AfterStateHash { get; init; } = string.Empty;
    public string SerializedAfterStateHash { get; init; } = string.Empty;
    public string RestoredAfterStateHash { get; init; } = string.Empty;
    public string FirstReplayHash { get; init; } = string.Empty;
    public string SecondReplayHash { get; init; } = string.Empty;
}

public sealed record SettlementRow
{
    public string SchemaVersion { get; init; } = "settlement_construction_row_v1";
    public string GoalId { get; init; } = SettlementConstructionDestructionProductionVocabulary.GoalId;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourcePackageRowRef { get; init; } = string.Empty;
    public string SourceReviewPackageRowRef { get; init; } = string.Empty;
    public string SourceSpatialDetailRowRef { get; init; } = string.Empty;
    public string SourceGameplayConsequenceRowRef { get; init; } = string.Empty;
    public string SourceLivingWorldRowRef { get; init; } = string.Empty;
    public string SourceInterlockedGameplayRowRef { get; init; } = string.Empty;
    public string SettlementId { get; init; } = string.Empty;
    public string SettlementName { get; init; } = string.Empty;
    public string SiteSpatialDetailRef { get; init; } = string.Empty;
    public string BuildingId { get; init; } = string.Empty;
    public string BuildingKind { get; init; } = string.Empty;
    public SettlementBuildingSlot BuildingSlot { get; init; } = new();
    public SettlementActionRecord ConstructionAction { get; init; } = new();
    public IReadOnlyList<SettlementResourceDelta> ConstructionCostLedger { get; init; } = [];
    public SettlementActionRecord ProductionAction { get; init; } = new();
    public IReadOnlyList<SettlementResourceDelta> ProductionOutputLedger { get; init; } = [];
    public SettlementActionRecord DamageDestructionThreatEvent { get; init; } = new();
    public SettlementActionRecord RepairUpgradeDefenseResponse { get; init; } = new();
    public SettlementLivingWorldLinkage LivingWorldConsequence { get; init; } = new();
    public SettlementInterlockedDependency InterlockedGameplayDependency { get; init; } = new();
    public SettlementStateSnapshot BeforeState { get; init; } = new();
    public SettlementStateSnapshot AfterState { get; init; } = new();
    public SettlementSaveLoadReplayRow SaveLoadReplayProof { get; init; } = new();
    public IReadOnlyList<string> MeaningfulVarianceAxes { get; init; } = [];
    public bool StateChanging { get; init; }
    public string RowHash { get; init; } = string.Empty;
}

public sealed record SettlementRowMatrix
{
    public string SchemaVersion { get; init; } = "settlement_construction_row_matrix_v1";
    public string GoalId { get; init; } = SettlementConstructionDestructionProductionVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int DistinctRowHashCount { get; init; }
    public IReadOnlyList<SettlementRow> Rows { get; init; } = [];
}

public sealed record SettlementLedgerEntry
{
    public string EntryId { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SettlementId { get; init; } = string.Empty;
    public string BuildingId { get; init; } = string.Empty;
    public string LedgerKind { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string Outcome { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceRefs { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record SettlementLedger
{
    public string SchemaVersion { get; init; } = "settlement_ledger_v1";
    public string GoalId { get; init; } = SettlementConstructionDestructionProductionVocabulary.GoalId;
    public string LedgerKind { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int EntryCount { get; init; }
    public IReadOnlyList<SettlementLedgerEntry> Entries { get; init; } = [];
}

public sealed record SettlementLivingWorldLinkageMatrix
{
    public string SchemaVersion { get; init; } = "settlement_living_world_linkage_v1";
    public string GoalId { get; init; } = SettlementConstructionDestructionProductionVocabulary.GoalId;
    public bool Passed { get; init; }
    public int LinkageCount { get; init; }
    public IReadOnlyList<SettlementLivingWorldLinkage> Linkages { get; init; } = [];
}

public sealed record SettlementSaveLoadReplayProof
{
    public string SchemaVersion { get; init; } = "settlement_save_load_replay_proof_v1";
    public string GoalId { get; init; } = SettlementConstructionDestructionProductionVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int StateChangedRowCount { get; init; }
    public int SaveLoadPassedRowCount { get; init; }
    public int ReplayPassedRowCount { get; init; }
    public IReadOnlyList<SettlementSaveLoadReplayRow> Rows { get; init; } = [];
}

public sealed record SettlementUnityCommandRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SettlementId { get; init; } = string.Empty;
    public string ConstructionActionId { get; init; } = string.Empty;
    public IReadOnlyList<string> ProductionLedgerEntryIds { get; init; } = [];
    public IReadOnlyList<string> DestructionRepairLedgerEntryIds { get; init; } = [];
    public IReadOnlyList<string> DefenseThreatLedgerEntryIds { get; init; } = [];
    public string LivingWorldLinkageId { get; init; } = string.Empty;
    public string InterlockedDependencyId { get; init; } = string.Empty;
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record SettlementUnityCommandPlan
{
    public string SchemaVersion { get; init; } = "settlement_unity_command_plan_v1";
    public string GoalId { get; init; } = SettlementConstructionDestructionProductionVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = SettlementConstructionDestructionProductionVocabulary.FinalGate;
    public IReadOnlyList<SettlementUnityCommandRow> Rows { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record SettlementUnityProofSummary
{
    public string SchemaVersion { get; init; } = "settlement_unity_proof_summary_v1";
    public string GoalId { get; init; } = SettlementConstructionDestructionProductionVocabulary.GoalId;
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
    public IReadOnlyList<SettlementDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SettlementUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public SettlementUnityProofSummary PlayerProof { get; init; } = new();
    public IReadOnlyList<SettlementDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SettlementPreviewExportRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SettlementId { get; init; } = string.Empty;
    public string BuildingId { get; init; } = string.Empty;
    public string SourcePackageRef { get; init; } = string.Empty;
    public string SourceSpatialRef { get; init; } = string.Empty;
    public string SourceLivingWorldRef { get; init; } = string.Empty;
    public string SourceInterlockedRef { get; init; } = string.Empty;
    public string SettlementAfterStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> PreviewMarkers { get; init; } = [];
}

public sealed record SettlementPreviewExportPayload
{
    public string SchemaVersion { get; init; } = "settlement_preview_export_payload_v1";
    public string GoalId { get; init; } = SettlementConstructionDestructionProductionVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<SettlementPreviewExportRow> Rows { get; init; } = [];
}

public sealed record InvalidSettlementScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<SettlementDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidSettlementDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = "settlement_invalid_diagnostics_matrix_v1";
    public string GoalId { get; init; } = SettlementConstructionDestructionProductionVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<InvalidSettlementScenario> Scenarios { get; init; } = [];
}

public sealed record SettlementReport
{
    public string SchemaVersion { get; init; } = "settlement_construction_destruction_production_matrix_report_v1";
    public string GoalId { get; init; } = SettlementConstructionDestructionProductionVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = SettlementConstructionDestructionProductionVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = SettlementConstructionDestructionProductionVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal065AcceptedByUserHandoff { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool BuildingCatalogPassed { get; init; }
    public bool RowMatrixPassed { get; init; }
    public bool ProductionLedgerPassed { get; init; }
    public bool DestructionRepairLedgerPassed { get; init; }
    public bool DefenseThreatLedgerPassed { get; init; }
    public bool LivingWorldLinkagePassed { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public bool MeaningfulVariancePassed { get; init; }
    public bool UnityCommandPlanPassed { get; init; }
    public bool UnityProofPassed { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public bool AllSettlementMarkersMatched { get; init; }
    public bool PreviewExportPayloadPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public int RowCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string BuildingCatalogHash { get; init; } = string.Empty;
    public string RowMatrixHash { get; init; } = string.Empty;
    public string ProductionLedgerHash { get; init; } = string.Empty;
    public string DestructionRepairLedgerHash { get; init; } = string.Empty;
    public string DefenseThreatLedgerHash { get; init; } = string.Empty;
    public string LivingWorldLinkageHash { get; init; } = string.Empty;
    public string SaveLoadReplayProofHash { get; init; } = string.Empty;
    public string UnityCommandPlanHash { get; init; } = string.Empty;
    public string UnityProofSummaryHash { get; init; } = string.Empty;
    public string PreviewExportPayloadHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<SettlementDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SettlementBuildResult
{
    public SettlementSourceManifest SourceManifest { get; init; } = new();
    public SettlementBuildingCatalog BuildingCatalog { get; init; } = new();
    public SettlementRowMatrix RowMatrix { get; init; } = new();
    public SettlementLedger ProductionLedger { get; init; } = new();
    public SettlementLedger DestructionRepairLedger { get; init; } = new();
    public SettlementLedger DefenseThreatLedger { get; init; } = new();
    public SettlementLivingWorldLinkageMatrix LivingWorldLinkage { get; init; } = new();
    public SettlementSaveLoadReplayProof SaveLoadReplayProof { get; init; } = new();
    public SettlementUnityCommandPlan UnityCommandPlan { get; init; } = new();
    public SettlementUnityProofSummary UnityProofSummary { get; init; } = new();
    public SettlementPreviewExportPayload PreviewExportPayload { get; init; } = new();
    public InvalidSettlementDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public SettlementReport Report { get; init; } = new();
    public IReadOnlyList<SettlementRow> Rows { get; init; } = [];
    public IReadOnlyList<SettlementFilePayload> StagingFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record SettlementWriteResult
{
    public SettlementBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
