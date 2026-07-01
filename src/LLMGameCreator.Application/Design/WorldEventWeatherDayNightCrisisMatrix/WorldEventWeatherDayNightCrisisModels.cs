namespace LLMGameCreator.Application.Design.WorldEventWeatherDayNightCrisisMatrix;

public static class WorldEventWeatherDayNightCrisisVocabulary
{
    public const string GoalId = "goal_069_world_event_weather_daynight_crisis_matrix";
    public const string ProductSmokeRoute = "goal-069-world-event-weather-daynight-crisis-matrix";
    public const string FinalGate = "world_event_weather_daynight_crisis_matrix_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-069-world-event-weather-daynight-crisis-matrix";
    public const string Goal060RelativeOutputDirectory = ".llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix";
    public const string Goal061RelativeOutputDirectory = ".llmgc/procedural/goal-061-full-campaign-playable-review-package-rc";
    public const string Goal062RelativeOutputDirectory = ".llmgc/procedural/goal-062-constrained-spatial-detail-generation";
    public const string Goal063RelativeOutputDirectory = ".llmgc/procedural/goal-063-gameplay-consequence-depth-matrix";
    public const string Goal064RelativeOutputDirectory = ".llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix";
    public const string Goal065RelativeOutputDirectory = ".llmgc/procedural/goal-065-interlocked-gameplay-systems-depth-matrix";
    public const string Goal066RelativeOutputDirectory = ".llmgc/procedural/goal-066-settlement-construction-destruction-production-matrix";
    public const string Goal067RelativeOutputDirectory = ".llmgc/procedural/goal-067-programmatic-narrative-quest-dialogue-event-matrix";
    public const string Goal068RelativeOutputDirectory = ".llmgc/procedural/goal-068-combat-magic-ability-boss-encounter-matrix";
    public const string StagingRoot = "staging";
    public const string UnityWorldEventCommandPlanStagingRelativePath = "world-event-weather-daynight/unity-world-event-command-plan.json";

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
        "missing_goal068_source",
        "fake_family",
        "fake_seed",
        "duplicate_row_id",
        "non_state_changing_row",
        "no_day_night_effect",
        "no_weather_hazard_effect",
        "crisis_with_no_consequence",
        "missing_cross_system_delta",
        "save_load_mismatch",
        "replay_mismatch",
        "nondeterministic_ordering",
        "unsafe_path",
        "provider_llm_rag_claim",
        "real_weather_network_claim",
        "runtime_ui_gamepackage_mutation_claim",
        "broad_unity_weather_rendering_claim",
        "arbitrary_lua_generated_lua_claim"
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

public sealed record WorldEventWeatherDayNightCrisisOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record WorldEventDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static WorldEventDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static WorldEventDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static WorldEventDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record WorldEventFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record WorldEventSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<WorldEventDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record WorldEventUpstreamHash
{
    public string SourceGoal { get; init; } = string.Empty;
    public string SourceRef { get; init; } = string.Empty;
    public string HashKind { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
}

public sealed record WorldEventSourceRow
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
    public string PackageHash { get; init; } = string.Empty;
    public string SpatialDetailRowHash { get; init; } = string.Empty;
    public string GameplayAfterStateHash { get; init; } = string.Empty;
    public string LivingWorldAfterStateHash { get; init; } = string.Empty;
    public string InterlockedAfterStateHash { get; init; } = string.Empty;
    public string SettlementAfterStateHash { get; init; } = string.Empty;
    public string NarrativeAfterStateHash { get; init; } = string.Empty;
    public string CombatMagicRowHash { get; init; } = string.Empty;
    public string CombatMagicAfterStateHash { get; init; } = string.Empty;
    public string QuestArcId { get; init; } = string.Empty;
    public string DialogueGraphId { get; init; } = string.Empty;
    public string EventChainId { get; init; } = string.Empty;
    public string SettlementId { get; init; } = string.Empty;
    public string BuildingId { get; init; } = string.Empty;
    public bool Goal060PackageValid { get; init; }
    public bool Goal061ReviewPackageRcExists { get; init; }
    public bool Goal062SpatialRowValid { get; init; }
    public bool Goal063GameplayRowValid { get; init; }
    public bool Goal064LivingWorldRowValid { get; init; }
    public bool Goal065InterlockedRowValid { get; init; }
    public bool Goal066SettlementRowValid { get; init; }
    public bool Goal067NarrativeRowValid { get; init; }
    public bool Goal068CombatMagicRowValid { get; init; }
    public bool Goal068SaveLoadReplayPassed { get; init; }
    public IReadOnlyList<string> CombatMagicChangedCategories { get; init; } = [];
    public IReadOnlyList<WorldEventUpstreamHash> UpstreamHashes { get; init; } = [];
}

public sealed record WorldEventSourceBundle
{
    public bool Goal068AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewPackageRcConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal063GameplayRowsConsumed { get; init; }
    public bool Goal064LivingWorldRowsConsumed { get; init; }
    public bool Goal065InterlockedRowsConsumed { get; init; }
    public bool Goal066SettlementRowsConsumed { get; init; }
    public bool Goal067NarrativeRowsConsumed { get; init; }
    public bool Goal068CombatMagicRowsConsumed { get; init; }
    public bool Goal068UnityProofConsumed { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<WorldEventSourceRow> Rows { get; init; } = [];
    public IReadOnlyList<WorldEventFilePayload> BaseStagingFiles { get; init; } = [];
    public IReadOnlyList<WorldEventSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<WorldEventDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record WorldEventGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record WorldEventSourceManifest
{
    public string SchemaVersion { get; init; } = "world_event_source_manifest_v1";
    public string GoalId { get; init; } = WorldEventWeatherDayNightCrisisVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = WorldEventWeatherDayNightCrisisVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = WorldEventWeatherDayNightCrisisVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal068AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewPackageRcConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal063GameplayRowsConsumed { get; init; }
    public bool Goal064LivingWorldRowsConsumed { get; init; }
    public bool Goal065InterlockedRowsConsumed { get; init; }
    public bool Goal066SettlementRowsConsumed { get; init; }
    public bool Goal067NarrativeRowsConsumed { get; init; }
    public bool Goal068CombatMagicRowsConsumed { get; init; }
    public bool Goal068UnityProofConsumed { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<WorldEventGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<WorldEventSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<WorldEventDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record WorldClockCalendarPolicy
{
    public string SchemaVersion { get; init; } = "world_clock_calendar_policy_v1";
    public bool Passed { get; init; }
    public IReadOnlyList<ClockPhaseDefinition> Phases { get; init; } = [];
    public IReadOnlyList<string> DeterministicOrdering { get; init; } = [];
}

public sealed record ClockPhaseDefinition
{
    public string PhaseId { get; init; } = string.Empty;
    public int StartHourInclusive { get; init; }
    public int EndHourExclusive { get; init; }
    public int LightLevel { get; init; }
    public string GameplayPressure { get; init; } = string.Empty;
}

public sealed record WeatherHazardCatalog
{
    public string SchemaVersion { get; init; } = "weather_hazard_catalog_v1";
    public bool Passed { get; init; }
    public IReadOnlyList<WeatherHazardDefinition> WeatherHazards { get; init; } = [];
}

public sealed record WeatherHazardDefinition
{
    public string WeatherId { get; init; } = string.Empty;
    public string HazardId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string PressureKind { get; init; } = string.Empty;
    public IReadOnlyList<string> AffectedStateKeys { get; init; } = [];
}

public sealed record CrisisEventCatalog
{
    public string SchemaVersion { get; init; } = "crisis_event_catalog_v1";
    public bool Passed { get; init; }
    public IReadOnlyList<CrisisEventDefinition> CrisisEvents { get; init; } = [];
}

public sealed record CrisisEventDefinition
{
    public string CrisisId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string CrisisKind { get; init; } = string.Empty;
    public IReadOnlyList<string> ConsequenceCategories { get; init; } = [];
}

public sealed record WorldClockState
{
    public int DayIndex { get; init; }
    public int Hour { get; init; }
    public string Phase { get; init; } = string.Empty;
    public int LightLevel { get; init; }
    public string CalendarTag { get; init; } = string.Empty;
}

public sealed record DayNightEffect
{
    public string EffectId { get; init; } = string.Empty;
    public string BeforePhase { get; init; } = string.Empty;
    public string AfterPhase { get; init; } = string.Empty;
    public int BeforeLightLevel { get; init; }
    public int AfterLightLevel { get; init; }
    public IReadOnlyList<string> StateDeltaRefs { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record WeatherHazardCondition
{
    public string WeatherId { get; init; } = string.Empty;
    public string HazardId { get; init; } = string.Empty;
    public int Severity { get; init; }
    public string PressureKind { get; init; } = string.Empty;
    public IReadOnlyList<string> StateDeltaRefs { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record CrisisEventRecord
{
    public string CrisisId { get; init; } = string.Empty;
    public string CrisisKind { get; init; } = string.Empty;
    public string Trigger { get; init; } = string.Empty;
    public string ConsequenceSummary { get; init; } = string.Empty;
    public IReadOnlyList<string> StateDeltaRefs { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record CrossSystemDelta
{
    public string DeltaId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string SourceRef { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record WorldEventStateSnapshot
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public int StepIndex { get; init; }
    public string StateHash { get; init; } = string.Empty;
    public SortedDictionary<string, string> Values { get; init; } = [];
}

public sealed record WorldEventSaveLoadReplayRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string BeforeStateHash { get; init; } = string.Empty;
    public string AfterStateHash { get; init; } = string.Empty;
    public bool BeforeAfterStateChanged { get; init; }
    public string SerializedAfterStateHash { get; init; } = string.Empty;
    public string RestoredAfterStateHash { get; init; } = string.Empty;
    public bool SaveLoadRoundtripPassed { get; init; }
    public string FirstReplayHash { get; init; } = string.Empty;
    public string SecondReplayHash { get; init; } = string.Empty;
    public bool ReplayDeterminismPassed { get; init; }
}

public sealed record WorldEventRow
{
    public string SchemaVersion { get; init; } = "world_event_weather_daynight_crisis_row_v1";
    public string GoalId { get; init; } = WorldEventWeatherDayNightCrisisVocabulary.GoalId;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public IReadOnlyList<string> UpstreamRefs { get; init; } = [];
    public IReadOnlyList<WorldEventUpstreamHash> UpstreamHashes { get; init; } = [];
    public WorldClockState WorldClockBefore { get; init; } = new();
    public WorldClockState WorldClockAfter { get; init; } = new();
    public DayNightEffect DayNightEffect { get; init; } = new();
    public WeatherHazardCondition WeatherHazard { get; init; } = new();
    public CrisisEventRecord CrisisEvent { get; init; } = new();
    public IReadOnlyList<CrossSystemDelta> CrossSystemDeltas { get; init; } = [];
    public WorldEventStateSnapshot BeforeState { get; init; } = new();
    public WorldEventStateSnapshot AfterState { get; init; } = new();
    public WorldEventSaveLoadReplayRow SaveLoadReplayProof { get; init; } = new();
    public IReadOnlyList<string> ChangedCategories { get; init; } = [];
    public string FamilyPressureKind { get; init; } = string.Empty;
    public bool StateChanging { get; init; }
    public IReadOnlyList<string> UnityMarkerExpectations { get; init; } = [];
    public string RowHash { get; init; } = string.Empty;
}

public sealed record WorldEventRowMatrix
{
    public string SchemaVersion { get; init; } = "world_event_weather_daynight_row_matrix_v1";
    public string GoalId { get; init; } = WorldEventWeatherDayNightCrisisVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int DayNightEffectRowCount { get; init; }
    public int WeatherHazardRowCount { get; init; }
    public int CrisisConsequenceRowCount { get; init; }
    public int CrossSystemDeltaRowCount { get; init; }
    public int DistinctRowHashCount { get; init; }
    public IReadOnlyList<WorldEventRow> Rows { get; init; } = [];
}

public sealed record WorldEventSaveLoadReplayProof
{
    public string SchemaVersion { get; init; } = "world_event_save_load_replay_proof_v1";
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int StateChangedRowCount { get; init; }
    public int SaveLoadPassedRowCount { get; init; }
    public int ReplayPassedRowCount { get; init; }
    public IReadOnlyList<WorldEventSaveLoadReplayRow> Rows { get; init; } = [];
}

public sealed record WorldEventVarianceMetrics
{
    public string SchemaVersion { get; init; } = "world_event_variance_metrics_v1";
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int DistinctWeatherCount { get; init; }
    public int DistinctCrisisCount { get; init; }
    public int DistinctPhaseTransitionCount { get; init; }
    public int DistinctRowHashCount { get; init; }
    public IReadOnlyList<WorldEventFamilyVarianceAxis> FamilyAxes { get; init; } = [];
}

public sealed record WorldEventFamilyVarianceAxis
{
    public string FamilyId { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public IReadOnlyList<string> WeatherIds { get; init; } = [];
    public IReadOnlyList<string> CrisisIds { get; init; } = [];
    public IReadOnlyList<string> PhaseTransitions { get; init; } = [];
}

public sealed record WorldEventUnityCommandPlan
{
    public string SchemaVersion { get; init; } = "world_event_unity_command_plan_v1";
    public string GoalId { get; init; } = WorldEventWeatherDayNightCrisisVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = WorldEventWeatherDayNightCrisisVocabulary.ProductSmokeRoute;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<WorldEventUnityCommandPlanRow> Rows { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record WorldEventUnityCommandPlanRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string ClockPhase { get; init; } = string.Empty;
    public string WeatherId { get; init; } = string.Empty;
    public string CrisisId { get; init; } = string.Empty;
    public bool StateChanged { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record WorldEventUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public WorldEventUnityProofSummary PlayerProof { get; init; } = new();
    public IReadOnlyList<WorldEventDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record WorldEventUnityProofSummary
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
    public IReadOnlyList<WorldEventDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record WorldEventInvalidDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = "world_event_invalid_diagnostics_matrix_v1";
    public bool Passed { get; init; }
    public IReadOnlyList<WorldEventInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record WorldEventInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<WorldEventDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record WorldEventPreviewExportPayload
{
    public string SchemaVersion { get; init; } = "world_event_preview_export_payload_v1";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<WorldEventPreviewExportRow> Rows { get; init; } = [];
}

public sealed record WorldEventPreviewExportRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string AfterStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> PreviewMarkers { get; init; } = [];
}

public sealed record WorldEventReport
{
    public string SchemaVersion { get; init; } = "world_event_weather_daynight_crisis_report_v1";
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal068AcceptedByUserHandoff { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool WorldClockPolicyPassed { get; init; }
    public bool WeatherHazardCatalogPassed { get; init; }
    public bool CrisisEventCatalogPassed { get; init; }
    public bool RowMatrixPassed { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public bool MeaningfulVariancePassed { get; init; }
    public bool UnityCommandPlanPassed { get; init; }
    public bool UnityProofPassed { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public bool AllWorldEventMarkersMatched { get; init; }
    public bool PreviewExportPayloadPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public int RowCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string ClockPolicyHash { get; init; } = string.Empty;
    public string WeatherHazardCatalogHash { get; init; } = string.Empty;
    public string CrisisEventCatalogHash { get; init; } = string.Empty;
    public string RowMatrixHash { get; init; } = string.Empty;
    public string SaveLoadReplayProofHash { get; init; } = string.Empty;
    public string VarianceMetricsHash { get; init; } = string.Empty;
    public string UnityCommandPlanHash { get; init; } = string.Empty;
    public string UnityProofSummaryHash { get; init; } = string.Empty;
    public string PreviewExportPayloadHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<WorldEventDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record WorldEventBuildResult
{
    public WorldEventSourceManifest SourceManifest { get; init; } = new();
    public WorldClockCalendarPolicy WorldClockPolicy { get; init; } = new();
    public WeatherHazardCatalog WeatherHazardCatalog { get; init; } = new();
    public CrisisEventCatalog CrisisEventCatalog { get; init; } = new();
    public WorldEventRowMatrix RowMatrix { get; init; } = new();
    public WorldEventSaveLoadReplayProof SaveLoadReplayProof { get; init; } = new();
    public WorldEventVarianceMetrics VarianceMetrics { get; init; } = new();
    public WorldEventUnityCommandPlan UnityCommandPlan { get; init; } = new();
    public WorldEventUnityProofSummary UnityProofSummary { get; init; } = new();
    public WorldEventInvalidDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public WorldEventPreviewExportPayload PreviewExportPayload { get; init; } = new();
    public WorldEventReport Report { get; init; } = new();
    public IReadOnlyList<WorldEventRow> Rows { get; init; } = [];
    public IReadOnlyList<WorldEventFilePayload> StagingFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record WorldEventWriteResult
{
    public WorldEventBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
