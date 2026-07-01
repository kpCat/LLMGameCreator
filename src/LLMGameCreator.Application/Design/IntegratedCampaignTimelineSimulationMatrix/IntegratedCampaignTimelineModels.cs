namespace LLMGameCreator.Application.Design.IntegratedCampaignTimelineSimulationMatrix;

public static class IntegratedCampaignTimelineVocabulary
{
    public const string GoalId = "goal_070_integrated_campaign_timeline_simulation_matrix";
    public const string ProductSmokeRoute = "goal-070-integrated-campaign-timeline-simulation-matrix";
    public const string FinalGate = "integrated_campaign_timeline_simulation_matrix_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-070-integrated-campaign-timeline-simulation-matrix";
    public const string Goal060RelativeOutputDirectory = ".llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix";
    public const string Goal061RelativeOutputDirectory = ".llmgc/procedural/goal-061-full-campaign-playable-review-package-rc";
    public const string Goal062RelativeOutputDirectory = ".llmgc/procedural/goal-062-constrained-spatial-detail-generation";
    public const string Goal063RelativeOutputDirectory = ".llmgc/procedural/goal-063-gameplay-consequence-depth-matrix";
    public const string Goal064RelativeOutputDirectory = ".llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix";
    public const string Goal065RelativeOutputDirectory = ".llmgc/procedural/goal-065-interlocked-gameplay-systems-depth-matrix";
    public const string Goal066RelativeOutputDirectory = ".llmgc/procedural/goal-066-settlement-construction-destruction-production-matrix";
    public const string Goal067RelativeOutputDirectory = ".llmgc/procedural/goal-067-programmatic-narrative-quest-dialogue-event-matrix";
    public const string Goal068RelativeOutputDirectory = ".llmgc/procedural/goal-068-combat-magic-ability-boss-encounter-matrix";
    public const string Goal069RelativeOutputDirectory = ".llmgc/procedural/goal-069-world-event-weather-daynight-crisis-matrix";
    public const string StagingRoot = "staging";
    public const string UnityCampaignTimelineCommandPlanStagingRelativePath = "campaign-timeline/unity-campaign-timeline-command-plan.json";

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
        "missing_goal069_source",
        "stale_goal069_handoff",
        "missing_family_row",
        "duplicate_row_id",
        "fake_source_id",
        "fake_family",
        "fake_seed",
        "missing_cross_system_cascade",
        "missing_arbitration",
        "unchanged_final_state",
        "replay_mismatch",
        "save_load_mismatch",
        "variance_only_by_id_hash",
        "final_prose_leakage",
        "provider_llm_rag_media_generation_claim",
        "arbitrary_lua_execution_claim",
        "runtime_ui_gamepackage_schema_mutation_claim",
        "broad_unity_gameplay_mutation_claim",
        "unsafe_path",
        "nondeterministic_order"
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

public sealed record IntegratedCampaignTimelineOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record TimelineDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static TimelineDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static TimelineDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static TimelineDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record TimelineFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record TimelineSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<TimelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record TimelineSourceHash
{
    public string SourceGoal { get; init; } = string.Empty;
    public string SourceRef { get; init; } = string.Empty;
    public string HashKind { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
}

public sealed record TimelineSourceRow
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
    public string SourceSettlementRowRef { get; init; } = string.Empty;
    public string SourceNarrativeRowRef { get; init; } = string.Empty;
    public string SourceCombatMagicRowRef { get; init; } = string.Empty;
    public string SourceWorldEventRowRef { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string SpatialDetailRowHash { get; init; } = string.Empty;
    public string GameplayAfterStateHash { get; init; } = string.Empty;
    public string LivingWorldAfterStateHash { get; init; } = string.Empty;
    public string InterlockedAfterStateHash { get; init; } = string.Empty;
    public string SettlementAfterStateHash { get; init; } = string.Empty;
    public string NarrativeAfterStateHash { get; init; } = string.Empty;
    public string CombatMagicAfterStateHash { get; init; } = string.Empty;
    public string WorldEventRowHash { get; init; } = string.Empty;
    public string WorldEventAfterStateHash { get; init; } = string.Empty;
    public string WorldClockPhase { get; init; } = string.Empty;
    public string WeatherId { get; init; } = string.Empty;
    public string CrisisId { get; init; } = string.Empty;
    public IReadOnlyList<string> WorldEventChangedCategories { get; init; } = [];
    public IReadOnlyList<string> UpstreamRefs { get; init; } = [];
    public IReadOnlyList<TimelineSourceHash> UpstreamHashes { get; init; } = [];
    public bool Goal060PackageValid { get; init; }
    public bool Goal061ReviewPackageRcExists { get; init; }
    public bool Goal062SpatialRowValid { get; init; }
    public bool Goal063GameplayRowValid { get; init; }
    public bool Goal064LivingWorldRowValid { get; init; }
    public bool Goal065InterlockedRowValid { get; init; }
    public bool Goal066SettlementRowValid { get; init; }
    public bool Goal067NarrativeRowValid { get; init; }
    public bool Goal068CombatMagicRowValid { get; init; }
    public bool Goal069WorldEventRowValid { get; init; }
    public bool Goal069SaveLoadReplayPassed { get; init; }
}

public sealed record TimelineSourceBundle
{
    public bool Goal069AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewPackageRcConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal063GameplayRowsConsumed { get; init; }
    public bool Goal064LivingWorldRowsConsumed { get; init; }
    public bool Goal065InterlockedRowsConsumed { get; init; }
    public bool Goal066SettlementRowsConsumed { get; init; }
    public bool Goal067NarrativeRowsConsumed { get; init; }
    public bool Goal068CombatMagicRowsConsumed { get; init; }
    public bool Goal069WorldEventRowsConsumed { get; init; }
    public bool Goal069UnityProofConsumed { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<TimelineSourceRow> Rows { get; init; } = [];
    public IReadOnlyList<TimelineFilePayload> BaseStagingFiles { get; init; } = [];
    public IReadOnlyList<TimelineSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<TimelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record TimelineGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record TimelineSourceManifest
{
    public string SchemaVersion { get; init; } = "integrated_campaign_timeline_source_manifest_v1";
    public string GoalId { get; init; } = IntegratedCampaignTimelineVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = IntegratedCampaignTimelineVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = IntegratedCampaignTimelineVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal069AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewPackageRcConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal063GameplayRowsConsumed { get; init; }
    public bool Goal064LivingWorldRowsConsumed { get; init; }
    public bool Goal065InterlockedRowsConsumed { get; init; }
    public bool Goal066SettlementRowsConsumed { get; init; }
    public bool Goal067NarrativeRowsConsumed { get; init; }
    public bool Goal068CombatMagicRowsConsumed { get; init; }
    public bool Goal069WorldEventRowsConsumed { get; init; }
    public bool Goal069UnityProofConsumed { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<TimelineGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<TimelineSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<TimelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record TimelineStateSnapshot
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public int TickIndex { get; init; }
    public string StateHash { get; init; } = string.Empty;
    public SortedDictionary<string, string> Values { get; init; } = [];
}

public sealed record TimelineDelta
{
    public string DeltaId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string SourceRef { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record TimelineTick
{
    public string TickId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string PhaseFamily { get; init; } = string.Empty;
    public string SystemCategory { get; init; } = string.Empty;
    public string SourceRef { get; init; } = string.Empty;
    public TimelineStateSnapshot BeforeState { get; init; } = new();
    public TimelineStateSnapshot AfterState { get; init; } = new();
    public IReadOnlyList<TimelineDelta> Deltas { get; init; } = [];
    public bool StateChanging { get; init; }
}

public sealed record CrossSystemCascadeRecord
{
    public string CascadeId { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public IReadOnlyList<string> TickIds { get; init; } = [];
    public IReadOnlyList<string> SystemCategories { get; init; } = [];
    public string Cause { get; init; } = string.Empty;
    public string Effect { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record ConflictArbitrationRecord
{
    public string ArbitrationId { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string Conflict { get; init; } = string.Empty;
    public string Decision { get; init; } = string.Empty;
    public string Loser { get; init; } = string.Empty;
    public IReadOnlyList<string> AffectedCategories { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record TimelineSaveLoadReplayRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string InitialStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> PerTickStateHashes { get; init; } = [];
    public string FinalStateHash { get; init; } = string.Empty;
    public string SaveCheckpointHash { get; init; } = string.Empty;
    public string LoadedCheckpointHash { get; init; } = string.Empty;
    public string ExpectedReplayHash { get; init; } = string.Empty;
    public string ReplayHash { get; init; } = string.Empty;
    public bool StateChanging { get; init; }
    public bool SaveLoadRoundtripPassed { get; init; }
    public bool ReplayDeterminismPassed { get; init; }
}

public sealed record CampaignTimelineRow
{
    public string SchemaVersion { get; init; } = "integrated_campaign_timeline_row_v1";
    public string GoalId { get; init; } = IntegratedCampaignTimelineVocabulary.GoalId;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string FamilyPhaseProfile { get; init; } = string.Empty;
    public string SourceWorldEventRowRef { get; init; } = string.Empty;
    public IReadOnlyList<string> UpstreamRefs { get; init; } = [];
    public IReadOnlyList<TimelineSourceHash> UpstreamHashes { get; init; } = [];
    public TimelineStateSnapshot InitialState { get; init; } = new();
    public IReadOnlyList<TimelineTick> Ticks { get; init; } = [];
    public IReadOnlyList<CrossSystemCascadeRecord> Cascades { get; init; } = [];
    public ConflictArbitrationRecord Arbitration { get; init; } = new();
    public TimelineSaveLoadReplayRow SaveLoadReplayProof { get; init; } = new();
    public IReadOnlyList<string> TouchedSystemCategories { get; init; } = [];
    public bool SettlementWorldNarrativeCombatCoupled { get; init; }
    public bool StateChanging { get; init; }
    public string RowHash { get; init; } = string.Empty;
}

public sealed record TimelineMatrixSummary
{
    public string SchemaVersion { get; init; } = "integrated_campaign_timeline_matrix_summary_v1";
    public string GoalId { get; init; } = IntegratedCampaignTimelineVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int RowsWithSixOrMoreTicks { get; init; }
    public int RowsWithFiveOrMoreCategories { get; init; }
    public int RowsWithThreeOrMoreCascades { get; init; }
    public int RowsWithArbitration { get; init; }
    public int DistinctRowHashCount { get; init; }
    public IReadOnlyList<CampaignTimelineRow> Rows { get; init; } = [];
}

public sealed record CrossSystemCascadeLedger
{
    public string SchemaVersion { get; init; } = "integrated_campaign_timeline_cascade_ledger_v1";
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int CascadeCount { get; init; }
    public IReadOnlyList<CrossSystemCascadeRecord> Cascades { get; init; } = [];
}

public sealed record ConflictArbitrationLedger
{
    public string SchemaVersion { get; init; } = "integrated_campaign_timeline_conflict_arbitration_ledger_v1";
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int ArbitrationCount { get; init; }
    public IReadOnlyList<ConflictArbitrationRecord> Arbitrations { get; init; } = [];
}

public sealed record SaveLoadReplayAudit
{
    public string SchemaVersion { get; init; } = "integrated_campaign_timeline_save_load_replay_audit_v1";
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int SaveLoadPassedRowCount { get; init; }
    public int ReplayPassedRowCount { get; init; }
    public IReadOnlyList<TimelineSaveLoadReplayRow> Rows { get; init; } = [];
}

public sealed record TimelineVarianceMetrics
{
    public string SchemaVersion { get; init; } = "integrated_campaign_timeline_variance_metrics_v1";
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int DistinctRowHashCount { get; init; }
    public int DistinctPhaseProfileCount { get; init; }
    public IReadOnlyList<FamilyVarianceAxis> FamilyAxes { get; init; } = [];
}

public sealed record FamilyVarianceAxis
{
    public string FamilyId { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public IReadOnlyList<string> ChangedWeatherIds { get; init; } = [];
    public IReadOnlyList<string> ChangedCrisisIds { get; init; } = [];
    public IReadOnlyList<string> ChangedArbitrationDecisions { get; init; } = [];
    public IReadOnlyList<string> ChangedPhaseProfiles { get; init; } = [];
}

public sealed record TimelineUnityCommandPlan
{
    public string SchemaVersion { get; init; } = "integrated_campaign_timeline_unity_command_plan_v1";
    public string GoalId { get; init; } = IntegratedCampaignTimelineVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = IntegratedCampaignTimelineVocabulary.ProductSmokeRoute;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<TimelineUnityCommandPlanRow> Rows { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record TimelineUnityCommandPlanRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public IReadOnlyList<string> TickIds { get; init; } = [];
    public IReadOnlyList<string> CascadeIds { get; init; } = [];
    public IReadOnlyList<string> ArbitrationIds { get; init; } = [];
    public bool StateChanged { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record TimelineUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public TimelineUnityProofSummary PlayerProof { get; init; } = new();
    public IReadOnlyList<TimelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record TimelineUnityProofSummary
{
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
    public IReadOnlyList<TimelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record TimelineInvalidDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = "integrated_campaign_timeline_invalid_diagnostics_matrix_v1";
    public bool Passed { get; init; }
    public IReadOnlyList<TimelineInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record TimelineInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<TimelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PreviewExportTimelinePayload
{
    public string SchemaVersion { get; init; } = "integrated_campaign_timeline_preview_export_payload_v1";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<PreviewExportTimelineRow> Rows { get; init; } = [];
}

public sealed record PreviewExportTimelineRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> PreviewMarkers { get; init; } = [];
}

public sealed record TimelineReport
{
    public string SchemaVersion { get; init; } = "integrated_campaign_timeline_simulation_matrix_report_v1";
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal069AcceptedByUserHandoff { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool RowMatrixPassed { get; init; }
    public bool CascadeLedgerPassed { get; init; }
    public bool ArbitrationLedgerPassed { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public bool MeaningfulVariancePassed { get; init; }
    public bool UnityCommandPlanPassed { get; init; }
    public bool UnityProofPassed { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public bool AllTimelineMarkersMatched { get; init; }
    public bool PreviewExportPayloadPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public int RowCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int CascadeCount { get; init; }
    public int ArbitrationCount { get; init; }
    public int SaveLoadPassedRowCount { get; init; }
    public int ReplayPassedRowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string MatrixSummaryHash { get; init; } = string.Empty;
    public string CascadeLedgerHash { get; init; } = string.Empty;
    public string ArbitrationLedgerHash { get; init; } = string.Empty;
    public string SaveLoadReplayAuditHash { get; init; } = string.Empty;
    public string VarianceMetricsHash { get; init; } = string.Empty;
    public string UnityCommandPlanHash { get; init; } = string.Empty;
    public string UnityProofSummaryHash { get; init; } = string.Empty;
    public string PreviewExportPayloadHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<TimelineDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record TimelineBuildResult
{
    public TimelineSourceManifest SourceManifest { get; init; } = new();
    public TimelineMatrixSummary MatrixSummary { get; init; } = new();
    public CrossSystemCascadeLedger CascadeLedger { get; init; } = new();
    public ConflictArbitrationLedger ArbitrationLedger { get; init; } = new();
    public SaveLoadReplayAudit SaveLoadReplayAudit { get; init; } = new();
    public TimelineVarianceMetrics VarianceMetrics { get; init; } = new();
    public TimelineUnityCommandPlan UnityCommandPlan { get; init; } = new();
    public TimelineUnityProofSummary UnityProofSummary { get; init; } = new();
    public PreviewExportTimelinePayload PreviewExportPayload { get; init; } = new();
    public TimelineInvalidDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public TimelineReport Report { get; init; } = new();
    public IReadOnlyList<CampaignTimelineRow> Rows { get; init; } = [];
    public IReadOnlyList<TimelineFilePayload> StagingFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record TimelineWriteResult
{
    public TimelineBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
