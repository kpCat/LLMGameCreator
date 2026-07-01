namespace LLMGameCreator.Application.Design.LivingWorldNpcFactionSimulationMatrix;

public static class LivingWorldNpcFactionSimulationVocabulary
{
    public const string GoalId = "goal_064_living_world_npc_faction_simulation_matrix";
    public const string ProductSmokeRoute = "goal-064-living-world-npc-faction-simulation-matrix";
    public const string FinalGate = "living_world_npc_faction_simulation_matrix_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix";
    public const string Goal060RelativeOutputDirectory = ".llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix";
    public const string Goal061RelativeOutputDirectory = ".llmgc/procedural/goal-061-full-campaign-playable-review-package-rc";
    public const string Goal062RelativeOutputDirectory = ".llmgc/procedural/goal-062-constrained-spatial-detail-generation";
    public const string Goal063RelativeOutputDirectory = ".llmgc/procedural/goal-063-gameplay-consequence-depth-matrix";
    public const string StagingRoot = "staging";
    public const string UnityLivingWorldCommandPlanStagingRelativePath = "living-world/unity-living-world-command-plan.json";

    public const string SourceManifestSchemaVersion = "living_world_npc_faction_source_manifest_v1";
    public const string CatalogSchemaVersion = "living_world_actor_faction_catalog_summary_v1";
    public const string SimulationMatrixPlanSchemaVersion = "living_world_simulation_matrix_plan_v1";
    public const string SimulationRowSchemaVersion = "living_world_simulation_row_v1";
    public const string SaveLoadReplayProofSchemaVersion = "living_world_save_load_replay_proof_v1";
    public const string VarianceMetricsSchemaVersion = "living_world_variance_metrics_v1";
    public const string UnityCommandPlanSchemaVersion = "living_world_unity_command_plan_v1";
    public const string UnityProofSummarySchemaVersion = "living_world_unity_proof_summary_v1";
    public const string PreviewExportPayloadSchemaVersion = "living_world_preview_export_payload_v1";
    public const string InvalidDiagnosticsMatrixSchemaVersion = "invalid_living_world_npc_faction_simulation_matrix_v1";
    public const string ReportSchemaVersion = "living_world_npc_faction_simulation_matrix_report_v1";

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
        "missing_goal063_source",
        "missing_goal062_spatial_detail_source",
        "fake_family_id",
        "fake_seed_id",
        "duplicate_actor_id",
        "duplicate_faction_id",
        "invalid_relation_target",
        "impossible_schedule_availability_state",
        "non_state_changing_row",
        "save_load_mismatch",
        "replay_mismatch",
        "hash_only_variance",
        "missing_unity_marker",
        "unsafe_path",
        "provider_llm_rag_claim",
        "runtime_ui_gamepackage_schema_mutation_claim",
        "unity_broad_mutation_claim",
        "media_generation_import_claim",
        "arbitrary_lua_execution_claim",
        "nondeterministic_ordering"
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

public sealed record LivingWorldNpcFactionSimulationOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record LivingWorldDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static LivingWorldDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static LivingWorldDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static LivingWorldDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record LivingWorldSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<LivingWorldDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record LivingWorldFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record LivingWorldSourceRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourcePackageRowRef { get; init; } = string.Empty;
    public string SourceReviewPackageRowRef { get; init; } = string.Empty;
    public string SourceSpatialDetailRowRef { get; init; } = string.Empty;
    public string SourceGameplayConsequenceRowRef { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string Goal060PackageRelativePath { get; init; } = string.Empty;
    public string ReviewPackageRelativePath { get; init; } = string.Empty;
    public string SpatialDetailRowHash { get; init; } = string.Empty;
    public string SpatialVarianceMarker { get; init; } = string.Empty;
    public bool Goal060RuntimeStateChanged { get; init; }
    public bool Goal061SaveLoadReplayVerified { get; init; }
    public bool Goal062Reachable { get; init; }
    public bool Goal062RouteVerified { get; init; }
    public bool Goal063StateChanging { get; init; }
    public bool Goal063SaveLoadReplayPassed { get; init; }
    public string Goal063BeforeStateHash { get; init; } = string.Empty;
    public string Goal063AfterStateHash { get; init; } = string.Empty;
    public string Goal063RowHash { get; init; } = string.Empty;
    public IReadOnlyList<string> Goal060ChangedStateKeys { get; init; } = [];
    public IReadOnlyList<string> Goal061ReviewCommandSteps { get; init; } = [];
    public IReadOnlyList<string> Goal063StepIds { get; init; } = [];
    public IReadOnlyList<string> Goal063DeltaIds { get; init; } = [];
    public IReadOnlyList<string> Goal063ChangedStateKeys { get; init; } = [];
}

public sealed record LivingWorldSourceBundle
{
    public bool Goal063AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewRowsConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal063GameplayRowsConsumed { get; init; }
    public bool Goal063UnityProofConsumed { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<LivingWorldSourceRow> Rows { get; init; } = [];
    public IReadOnlyList<LivingWorldFilePayload> BaseStagingFiles { get; init; } = [];
    public IReadOnlyList<LivingWorldSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<LivingWorldDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record LivingWorldGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record LivingWorldSourceManifest
{
    public string SchemaVersion { get; init; } = LivingWorldNpcFactionSimulationVocabulary.SourceManifestSchemaVersion;
    public string GoalId { get; init; } = LivingWorldNpcFactionSimulationVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = LivingWorldNpcFactionSimulationVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = LivingWorldNpcFactionSimulationVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal063AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewRowsConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal063GameplayRowsConsumed { get; init; }
    public bool Goal063UnityProofConsumed { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<LivingWorldGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<LivingWorldSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<LivingWorldDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record LivingWorldActorRecord
{
    public string ActorId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string BeforeStatus { get; init; } = string.Empty;
    public string AfterStatus { get; init; } = string.Empty;
    public string BeforeAvailability { get; init; } = string.Empty;
    public string AfterAvailability { get; init; } = string.Empty;
    public string BeforeRouteOrLocation { get; init; } = string.Empty;
    public string AfterRouteOrLocation { get; init; } = string.Empty;
}

public sealed record LivingWorldFactionRecord
{
    public string FactionId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string GroupKind { get; init; } = string.Empty;
    public string BeforeStance { get; init; } = string.Empty;
    public string AfterStance { get; init; } = string.Empty;
    public int BeforeReputation { get; init; }
    public int AfterReputation { get; init; }
    public int BeforeTrustOrAggression { get; init; }
    public int AfterTrustOrAggression { get; init; }
}

public sealed record LivingWorldRelationshipRecord
{
    public string RelationshipId { get; init; } = string.Empty;
    public string SourceActorOrFactionId { get; init; } = string.Empty;
    public string TargetActorOrFactionId { get; init; } = string.Empty;
    public string BeforeRelation { get; init; } = string.Empty;
    public string AfterRelation { get; init; } = string.Empty;
    public int BeforeReputation { get; init; }
    public int AfterReputation { get; init; }
}

public sealed record LivingWorldScheduleRecord
{
    public string ScheduleId { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public string BeforeAvailability { get; init; } = string.Empty;
    public string AfterAvailability { get; init; } = string.Empty;
    public string BeforeSlot { get; init; } = string.Empty;
    public string AfterSlot { get; init; } = string.Empty;
    public bool AvailabilityChanged { get; init; }
}

public sealed record LivingWorldEventRecord
{
    public string EventId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string EventKind { get; init; } = string.Empty;
    public string SourceGameplayDeltaId { get; init; } = string.Empty;
    public string BeforeState { get; init; } = string.Empty;
    public string AfterState { get; init; } = string.Empty;
    public bool Resolved { get; init; }
}

public sealed record LivingWorldMemoryRumorTraceRecord
{
    public string TraceId { get; init; } = string.Empty;
    public string TraceKind { get; init; } = string.Empty;
    public string ActorOrFactionId { get; init; } = string.Empty;
    public string SourceGameplayConsequenceRowRef { get; init; } = string.Empty;
    public string SourceSpatialDetailRowRef { get; init; } = string.Empty;
    public string SourceDeltaId { get; init; } = string.Empty;
    public string MemoryState { get; init; } = string.Empty;
}

public sealed record LivingWorldTickRecord
{
    public int TickIndex { get; init; }
    public string TickId { get; init; } = string.Empty;
    public string TickKind { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public string EventId { get; init; } = string.Empty;
    public IReadOnlyList<string> ChangedKeys { get; init; } = [];
    public string BeforeStateHash { get; init; } = string.Empty;
    public string AfterStateHash { get; init; } = string.Empty;
}

public sealed record LivingWorldStateSnapshot
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public int TickIndex { get; init; }
    public IReadOnlyDictionary<string, string> Values { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string StateHash { get; init; } = string.Empty;
}

public sealed record LivingWorldStateDelta
{
    public string DeltaId { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string CausalSourceRef { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record LivingWorldSaveLoadReplayRow
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

public sealed record LivingWorldSimulationRow
{
    public string SchemaVersion { get; init; } = LivingWorldNpcFactionSimulationVocabulary.SimulationRowSchemaVersion;
    public string GoalId { get; init; } = LivingWorldNpcFactionSimulationVocabulary.GoalId;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourcePackageRowId { get; init; } = string.Empty;
    public string SourcePackageRowRef { get; init; } = string.Empty;
    public string SourceReviewPackageRowRef { get; init; } = string.Empty;
    public string SourceSpatialDetailRowRef { get; init; } = string.Empty;
    public string SourceGameplayConsequenceRowRef { get; init; } = string.Empty;
    public IReadOnlyList<LivingWorldActorRecord> ActorRecords { get; init; } = [];
    public IReadOnlyList<LivingWorldFactionRecord> FactionRecords { get; init; } = [];
    public IReadOnlyList<LivingWorldRelationshipRecord> RelationshipRecords { get; init; } = [];
    public IReadOnlyList<LivingWorldScheduleRecord> ScheduleAvailabilityRecords { get; init; } = [];
    public IReadOnlyList<LivingWorldEventRecord> WorldEventRecords { get; init; } = [];
    public IReadOnlyList<LivingWorldMemoryRumorTraceRecord> MemoryRumorConsequenceTraceRecords { get; init; } = [];
    public IReadOnlyList<LivingWorldTickRecord> OrderedTickPlan { get; init; } = [];
    public LivingWorldStateSnapshot BeforeState { get; init; } = new();
    public LivingWorldStateSnapshot AfterState { get; init; } = new();
    public IReadOnlyList<LivingWorldStateDelta> StateDeltaSummary { get; init; } = [];
    public LivingWorldSaveLoadReplayRow SaveLoadReplayProof { get; init; } = new();
    public IReadOnlyList<string> MeaningfulVarianceAxes { get; init; } = [];
    public string FamilyRuleProfile { get; init; } = string.Empty;
    public string RowHash { get; init; } = string.Empty;
}

public sealed record LivingWorldActorFactionCatalogSummary
{
    public string SchemaVersion { get; init; } = LivingWorldNpcFactionSimulationVocabulary.CatalogSchemaVersion;
    public string GoalId { get; init; } = LivingWorldNpcFactionSimulationVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ActorCount { get; init; }
    public int FactionCount { get; init; }
    public int RelationshipCount { get; init; }
    public int ScheduleCount { get; init; }
    public IReadOnlyList<string> ActorIds { get; init; } = [];
    public IReadOnlyList<string> FactionIds { get; init; } = [];
    public IReadOnlyList<string> RuleFamilies { get; init; } = [];
}

public sealed record LivingWorldSimulationMatrixPlan
{
    public string SchemaVersion { get; init; } = LivingWorldNpcFactionSimulationVocabulary.SimulationMatrixPlanSchemaVersion;
    public string GoalId { get; init; } = LivingWorldNpcFactionSimulationVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int DistinctRowHashCount { get; init; }
    public IReadOnlyList<LivingWorldSimulationRow> Rows { get; init; } = [];
}

public sealed record LivingWorldSaveLoadReplayProof
{
    public string SchemaVersion { get; init; } = LivingWorldNpcFactionSimulationVocabulary.SaveLoadReplayProofSchemaVersion;
    public string GoalId { get; init; } = LivingWorldNpcFactionSimulationVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int StateChangedRowCount { get; init; }
    public int SaveLoadPassedRowCount { get; init; }
    public int ReplayPassedRowCount { get; init; }
    public IReadOnlyList<LivingWorldSaveLoadReplayRow> Rows { get; init; } = [];
}

public sealed record LivingWorldFamilyVarianceSummary
{
    public string FamilyId { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public bool SameFamilySeedVariationPassed { get; init; }
    public IReadOnlyList<string> RuleProfiles { get; init; } = [];
    public IReadOnlyList<string> MeaningfulAxes { get; init; } = [];
    public IReadOnlyList<string> RowHashes { get; init; } = [];
}

public sealed record LivingWorldVarianceMetrics
{
    public string SchemaVersion { get; init; } = LivingWorldNpcFactionSimulationVocabulary.VarianceMetricsSchemaVersion;
    public string GoalId { get; init; } = LivingWorldNpcFactionSimulationVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool HashOnlyVarianceRejected { get; init; }
    public bool SameFamilySeedVariationPassed { get; init; }
    public bool CrossFamilyRuleVariationPassed { get; init; }
    public int DistinctAfterStateHashCount { get; init; }
    public int DistinctRuleProfileCount { get; init; }
    public IReadOnlyList<LivingWorldFamilyVarianceSummary> Families { get; init; } = [];
}

public sealed record LivingWorldUnityCommandRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public IReadOnlyList<string> TickIds { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record LivingWorldUnityCommandPlan
{
    public string SchemaVersion { get; init; } = LivingWorldNpcFactionSimulationVocabulary.UnityCommandPlanSchemaVersion;
    public string GoalId { get; init; } = LivingWorldNpcFactionSimulationVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = LivingWorldNpcFactionSimulationVocabulary.FinalGate;
    public IReadOnlyList<LivingWorldUnityCommandRow> Rows { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record LivingWorldUnityProofSummary
{
    public string SchemaVersion { get; init; } = LivingWorldNpcFactionSimulationVocabulary.UnityProofSummarySchemaVersion;
    public string GoalId { get; init; } = LivingWorldNpcFactionSimulationVocabulary.GoalId;
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
    public IReadOnlyList<LivingWorldDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record LivingWorldUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public LivingWorldUnityProofSummary PlayerProof { get; init; } = new();
    public IReadOnlyList<LivingWorldDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record LivingWorldPreviewExportRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourcePackageRef { get; init; } = string.Empty;
    public string SourceSpatialRef { get; init; } = string.Empty;
    public string SourceGameplayRef { get; init; } = string.Empty;
    public string LivingWorldAfterStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ActorIds { get; init; } = [];
    public IReadOnlyList<string> FactionIds { get; init; } = [];
    public IReadOnlyList<string> EventIds { get; init; } = [];
    public IReadOnlyList<string> PreviewMarkers { get; init; } = [];
}

public sealed record LivingWorldPreviewExportPayload
{
    public string SchemaVersion { get; init; } = LivingWorldNpcFactionSimulationVocabulary.PreviewExportPayloadSchemaVersion;
    public string GoalId { get; init; } = LivingWorldNpcFactionSimulationVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<LivingWorldPreviewExportRow> Rows { get; init; } = [];
}

public sealed record InvalidLivingWorldScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<LivingWorldDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidLivingWorldDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = LivingWorldNpcFactionSimulationVocabulary.InvalidDiagnosticsMatrixSchemaVersion;
    public string GoalId { get; init; } = LivingWorldNpcFactionSimulationVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<InvalidLivingWorldScenario> Scenarios { get; init; } = [];
}

public sealed record LivingWorldNpcFactionSimulationReport
{
    public string SchemaVersion { get; init; } = LivingWorldNpcFactionSimulationVocabulary.ReportSchemaVersion;
    public string GoalId { get; init; } = LivingWorldNpcFactionSimulationVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = LivingWorldNpcFactionSimulationVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = LivingWorldNpcFactionSimulationVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal063AcceptedByUserHandoff { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool CatalogPassed { get; init; }
    public bool SimulationMatrixPassed { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public bool MeaningfulVariancePassed { get; init; }
    public bool UnityCommandPlanPassed { get; init; }
    public bool UnityProofPassed { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public bool AllLivingWorldMarkersMatched { get; init; }
    public bool PreviewExportPayloadPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public int RowCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string CatalogHash { get; init; } = string.Empty;
    public string SimulationMatrixPlanHash { get; init; } = string.Empty;
    public string SaveLoadReplayProofHash { get; init; } = string.Empty;
    public string VarianceMetricsHash { get; init; } = string.Empty;
    public string UnityCommandPlanHash { get; init; } = string.Empty;
    public string UnityProofSummaryHash { get; init; } = string.Empty;
    public string PreviewExportPayloadHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<LivingWorldDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record LivingWorldNpcFactionSimulationBuildResult
{
    public LivingWorldSourceManifest SourceManifest { get; init; } = new();
    public LivingWorldActorFactionCatalogSummary CatalogSummary { get; init; } = new();
    public LivingWorldSimulationMatrixPlan SimulationMatrixPlan { get; init; } = new();
    public LivingWorldSaveLoadReplayProof SaveLoadReplayProof { get; init; } = new();
    public LivingWorldVarianceMetrics VarianceMetrics { get; init; } = new();
    public LivingWorldUnityCommandPlan UnityCommandPlan { get; init; } = new();
    public LivingWorldUnityProofSummary UnityProofSummary { get; init; } = new();
    public LivingWorldPreviewExportPayload PreviewExportPayload { get; init; } = new();
    public InvalidLivingWorldDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public LivingWorldNpcFactionSimulationReport Report { get; init; } = new();
    public IReadOnlyList<LivingWorldSimulationRow> Rows { get; init; } = [];
    public IReadOnlyList<LivingWorldFilePayload> StagingFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record LivingWorldNpcFactionSimulationWriteResult
{
    public LivingWorldNpcFactionSimulationBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
