namespace LLMGameCreator.Application.Design.InterlockedGameplaySystemsDepthMatrix;

public static class InterlockedGameplaySystemsDepthMatrixVocabulary
{
    public const string GoalId = "goal_065_interlocked_gameplay_systems_depth_matrix";
    public const string ProductSmokeRoute = "goal-065-interlocked-gameplay-systems-depth-matrix";
    public const string FinalGate = "interlocked_gameplay_systems_depth_matrix_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-065-interlocked-gameplay-systems-depth-matrix";
    public const string Goal060RelativeOutputDirectory = ".llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix";
    public const string Goal061RelativeOutputDirectory = ".llmgc/procedural/goal-061-full-campaign-playable-review-package-rc";
    public const string Goal062RelativeOutputDirectory = ".llmgc/procedural/goal-062-constrained-spatial-detail-generation";
    public const string Goal063RelativeOutputDirectory = ".llmgc/procedural/goal-063-gameplay-consequence-depth-matrix";
    public const string Goal064RelativeOutputDirectory = ".llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix";
    public const string StagingRoot = "staging";
    public const string UnityInterlockedCommandPlanStagingRelativePath = "interlocked-gameplay/unity-interlocked-gameplay-command-plan.json";

    public const string SourceManifestSchemaVersion = "interlocked_gameplay_source_manifest_v1";
    public const string RuleCatalogSchemaVersion = "interlocked_gameplay_rule_catalog_v1";
    public const string RowPlanMatrixSchemaVersion = "interlocked_gameplay_row_plan_matrix_v1";
    public const string LedgerSchemaVersion = "interlocked_gameplay_ledger_v1";
    public const string SaveLoadReplaySchemaVersion = "interlocked_gameplay_save_load_replay_proof_v1";
    public const string VarianceMetricsSchemaVersion = "interlocked_gameplay_variance_metrics_v1";
    public const string UnityCommandPlanSchemaVersion = "interlocked_gameplay_unity_command_plan_v1";
    public const string UnityProofSummarySchemaVersion = "interlocked_gameplay_unity_proof_summary_v1";
    public const string PreviewExportPayloadSchemaVersion = "interlocked_gameplay_preview_export_payload_v1";
    public const string InvalidDiagnosticsMatrixSchemaVersion = "invalid_interlocked_gameplay_systems_depth_matrix_v1";
    public const string ReportSchemaVersion = "interlocked_gameplay_systems_depth_matrix_report_v1";

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
        "missing_goal060_source",
        "missing_goal061_source",
        "missing_goal062_source",
        "missing_goal063_source",
        "missing_goal064_source",
        "fake_family_id",
        "fake_seed_id",
        "duplicate_row_id",
        "non_state_changing_row",
        "economy_delta_without_source_trace",
        "crafting_delta_without_resource_input_output",
        "combat_delta_without_outcome",
        "progression_delta_without_causal_trace",
        "replay_mismatch",
        "save_load_mismatch",
        "nondeterministic_ordering",
        "unsafe_path",
        "provider_llm_rag_media_generation_claim",
        "runtime_ui_gamepackage_schema_mutation_claim",
        "unity_broad_mutation_claim",
        "arbitrary_lua_execution_claim"
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

public sealed record InterlockedGameplaySystemsOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record InterlockedGameplayDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static InterlockedGameplayDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static InterlockedGameplayDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static InterlockedGameplayDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record InterlockedGameplayFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record InterlockedGameplaySourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<InterlockedGameplayDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InterlockedGameplaySourceRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourcePackageRowRef { get; init; } = string.Empty;
    public string SourceReviewPackageRowRef { get; init; } = string.Empty;
    public string SourceSpatialDetailRowRef { get; init; } = string.Empty;
    public string SourceGameplayConsequenceRowRef { get; init; } = string.Empty;
    public string SourceLivingWorldRowRef { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public string ReviewPackageRelativePath { get; init; } = string.Empty;
    public string SpatialDetailRowHash { get; init; } = string.Empty;
    public string SpatialVarianceMarker { get; init; } = string.Empty;
    public string GameplayAfterStateHash { get; init; } = string.Empty;
    public string GameplayRowHash { get; init; } = string.Empty;
    public string LivingWorldAfterStateHash { get; init; } = string.Empty;
    public string LivingWorldRowHash { get; init; } = string.Empty;
    public string LivingWorldRuleProfile { get; init; } = string.Empty;
    public bool Goal060RuntimeStateChanged { get; init; }
    public bool Goal061SaveLoadReplayVerified { get; init; }
    public bool Goal062Reachable { get; init; }
    public bool Goal062RouteVerified { get; init; }
    public bool Goal063StateChanging { get; init; }
    public bool Goal063SaveLoadReplayPassed { get; init; }
    public bool Goal064StateChanging { get; init; }
    public bool Goal064SaveLoadReplayPassed { get; init; }
    public IReadOnlyList<string> Goal063DeltaIds { get; init; } = [];
    public IReadOnlyList<string> Goal064TickIds { get; init; } = [];
    public IReadOnlyList<string> Goal064ActorIds { get; init; } = [];
    public IReadOnlyList<string> Goal064FactionIds { get; init; } = [];
    public IReadOnlyList<string> Goal064EventIds { get; init; } = [];
    public IReadOnlyList<string> Goal064ChangedStateKeys { get; init; } = [];
}

public sealed record InterlockedGameplaySourceBundle
{
    public bool Goal064AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewRowsConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal063GameplayRowsConsumed { get; init; }
    public bool Goal064LivingWorldRowsConsumed { get; init; }
    public bool Goal064UnityProofConsumed { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<InterlockedGameplaySourceRow> Rows { get; init; } = [];
    public IReadOnlyList<InterlockedGameplayFilePayload> BaseStagingFiles { get; init; } = [];
    public IReadOnlyList<InterlockedGameplaySourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<InterlockedGameplayDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InterlockedGameplayGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record InterlockedGameplaySourceManifest
{
    public string SchemaVersion { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.SourceManifestSchemaVersion;
    public string GoalId { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal064AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewRowsConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal063GameplayRowsConsumed { get; init; }
    public bool Goal064LivingWorldRowsConsumed { get; init; }
    public bool Goal064UnityProofConsumed { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<InterlockedGameplayGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<InterlockedGameplaySourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<InterlockedGameplayDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InterlockedGameplayRuleProfile
{
    public string FamilyId { get; init; } = string.Empty;
    public string RuleSetId { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredDeltaCategories { get; init; } = [];
    public IReadOnlyList<string> FamilyExpectations { get; init; } = [];
    public IReadOnlyList<string> ForbiddenClaims { get; init; } = [];
}

public sealed record InterlockedGameplayRuleCatalog
{
    public string SchemaVersion { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.RuleCatalogSchemaVersion;
    public string GoalId { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RuleProfileCount { get; init; }
    public IReadOnlyList<InterlockedGameplayRuleProfile> Profiles { get; init; } = [];
}

public sealed record InterlockedGameplayStateSnapshot
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public int StepIndex { get; init; }
    public IReadOnlyDictionary<string, string> Values { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string StateHash { get; init; } = string.Empty;
}

public sealed record InterlockedSystemDelta
{
    public string DeltaId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Subsystem { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string Outcome { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceRefs { get; init; } = [];
    public string CausalTrace { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record InterlockedGameplayStep
{
    public int StepIndex { get; init; }
    public string StepId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string RuleId { get; init; } = string.Empty;
    public IReadOnlyList<InterlockedSystemDelta> Deltas { get; init; } = [];
    public InterlockedGameplayStateSnapshot Before { get; init; } = new();
    public InterlockedGameplayStateSnapshot After { get; init; } = new();
    public bool StateChanged { get; init; }
}

public sealed record InterlockedSaveLoadReplayRow
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

public sealed record InterlockedGameplayRow
{
    public string SchemaVersion { get; init; } = "interlocked_gameplay_row_v1";
    public string GoalId { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.GoalId;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourcePackageRowRef { get; init; } = string.Empty;
    public string SourceReviewPackageRowRef { get; init; } = string.Empty;
    public string SourceSpatialDetailRowRef { get; init; } = string.Empty;
    public string SourceGameplayConsequenceRowRef { get; init; } = string.Empty;
    public string SourceLivingWorldRowRef { get; init; } = string.Empty;
    public string DerivedRuleSetId { get; init; } = string.Empty;
    public IReadOnlyList<string> ExpectedUnityMarkerSet { get; init; } = [];
    public InterlockedGameplayStateSnapshot BeforeState { get; init; } = new();
    public InterlockedGameplayStateSnapshot AfterState { get; init; } = new();
    public IReadOnlyList<InterlockedGameplayStep> Steps { get; init; } = [];
    public IReadOnlyList<InterlockedSystemDelta> Deltas { get; init; } = [];
    public InterlockedSaveLoadReplayRow SaveLoadReplayProof { get; init; } = new();
    public IReadOnlyList<string> MeaningfulVarianceAxes { get; init; } = [];
    public bool StateChanging { get; init; }
    public string RowHash { get; init; } = string.Empty;
}

public sealed record InterlockedGameplayRowPlanMatrix
{
    public string SchemaVersion { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.RowPlanMatrixSchemaVersion;
    public string GoalId { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int DistinctRowHashCount { get; init; }
    public IReadOnlyList<InterlockedGameplayRow> Rows { get; init; } = [];
}

public sealed record InterlockedLedgerEntry
{
    public string EntryId { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Subsystem { get; init; } = string.Empty;
    public string Input { get; init; } = string.Empty;
    public string Output { get; init; } = string.Empty;
    public string Outcome { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceRefs { get; init; } = [];
}

public sealed record InterlockedGameplayLedger
{
    public string SchemaVersion { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.LedgerSchemaVersion;
    public string GoalId { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.GoalId;
    public string LedgerKind { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int EntryCount { get; init; }
    public IReadOnlyList<InterlockedLedgerEntry> Entries { get; init; } = [];
}

public sealed record InterlockedSaveLoadReplayProof
{
    public string SchemaVersion { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.SaveLoadReplaySchemaVersion;
    public string GoalId { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int StateChangedRowCount { get; init; }
    public int SaveLoadPassedRowCount { get; init; }
    public int ReplayPassedRowCount { get; init; }
    public IReadOnlyList<InterlockedSaveLoadReplayRow> Rows { get; init; } = [];
}

public sealed record InterlockedFamilyVarianceSummary
{
    public string FamilyId { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public bool SameFamilySeedVariationPassed { get; init; }
    public IReadOnlyList<string> RuleSetIds { get; init; } = [];
    public IReadOnlyList<string> MeaningfulAxes { get; init; } = [];
    public IReadOnlyList<string> RowHashes { get; init; } = [];
}

public sealed record InterlockedVarianceMetrics
{
    public string SchemaVersion { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.VarianceMetricsSchemaVersion;
    public string GoalId { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool HashOnlyVarianceRejected { get; init; }
    public bool SameFamilySeedVariationPassed { get; init; }
    public bool CrossFamilyRuleVariationPassed { get; init; }
    public int DistinctAfterStateHashCount { get; init; }
    public int DistinctRuleSetCount { get; init; }
    public IReadOnlyList<InterlockedFamilyVarianceSummary> Families { get; init; } = [];
}

public sealed record InterlockedUnityCommandRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public IReadOnlyList<string> EconomyDeltaIds { get; init; } = [];
    public IReadOnlyList<string> CraftingDeltaIds { get; init; } = [];
    public IReadOnlyList<string> CombatDeltaIds { get; init; } = [];
    public IReadOnlyList<string> ProgressionDeltaIds { get; init; } = [];
    public IReadOnlyList<string> StatusDeltaIds { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record InterlockedUnityCommandPlan
{
    public string SchemaVersion { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.UnityCommandPlanSchemaVersion;
    public string GoalId { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.FinalGate;
    public IReadOnlyList<InterlockedUnityCommandRow> Rows { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record InterlockedUnityProofSummary
{
    public string SchemaVersion { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.UnityProofSummarySchemaVersion;
    public string GoalId { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.GoalId;
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
    public IReadOnlyList<InterlockedGameplayDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InterlockedUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public InterlockedUnityProofSummary PlayerProof { get; init; } = new();
    public IReadOnlyList<InterlockedGameplayDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InterlockedPreviewExportRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourcePackageRef { get; init; } = string.Empty;
    public string SourceGameplayRef { get; init; } = string.Empty;
    public string SourceLivingWorldRef { get; init; } = string.Empty;
    public string InterlockedAfterStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> PreviewMarkers { get; init; } = [];
}

public sealed record InterlockedPreviewExportPayload
{
    public string SchemaVersion { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.PreviewExportPayloadSchemaVersion;
    public string GoalId { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<InterlockedPreviewExportRow> Rows { get; init; } = [];
}

public sealed record InvalidInterlockedGameplayScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<InterlockedGameplayDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidInterlockedGameplayDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.InvalidDiagnosticsMatrixSchemaVersion;
    public string GoalId { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<InvalidInterlockedGameplayScenario> Scenarios { get; init; } = [];
}

public sealed record InterlockedGameplaySystemsReport
{
    public string SchemaVersion { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.ReportSchemaVersion;
    public string GoalId { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = InterlockedGameplaySystemsDepthMatrixVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal064AcceptedByUserHandoff { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool RuleCatalogPassed { get; init; }
    public bool RowPlanPassed { get; init; }
    public bool EconomyCraftingLedgerPassed { get; init; }
    public bool CombatProgressionLedgerPassed { get; init; }
    public bool StatusEffectLedgerPassed { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public bool MeaningfulVariancePassed { get; init; }
    public bool UnityCommandPlanPassed { get; init; }
    public bool UnityProofPassed { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public bool AllInterlockedMarkersMatched { get; init; }
    public bool PreviewExportPayloadPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public int RowCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string RuleCatalogHash { get; init; } = string.Empty;
    public string RowPlanMatrixHash { get; init; } = string.Empty;
    public string EconomyCraftingLedgerHash { get; init; } = string.Empty;
    public string CombatProgressionLedgerHash { get; init; } = string.Empty;
    public string StatusEffectLedgerHash { get; init; } = string.Empty;
    public string SaveLoadReplayProofHash { get; init; } = string.Empty;
    public string VarianceMetricsHash { get; init; } = string.Empty;
    public string UnityCommandPlanHash { get; init; } = string.Empty;
    public string UnityProofSummaryHash { get; init; } = string.Empty;
    public string PreviewExportPayloadHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<InterlockedGameplayDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InterlockedGameplaySystemsBuildResult
{
    public InterlockedGameplaySourceManifest SourceManifest { get; init; } = new();
    public InterlockedGameplayRuleCatalog RuleCatalog { get; init; } = new();
    public InterlockedGameplayRowPlanMatrix RowPlanMatrix { get; init; } = new();
    public InterlockedGameplayLedger EconomyCraftingLedger { get; init; } = new();
    public InterlockedGameplayLedger CombatProgressionLedger { get; init; } = new();
    public InterlockedGameplayLedger StatusEffectLedger { get; init; } = new();
    public InterlockedSaveLoadReplayProof SaveLoadReplayProof { get; init; } = new();
    public InterlockedVarianceMetrics VarianceMetrics { get; init; } = new();
    public InterlockedUnityCommandPlan UnityCommandPlan { get; init; } = new();
    public InterlockedUnityProofSummary UnityProofSummary { get; init; } = new();
    public InterlockedPreviewExportPayload PreviewExportPayload { get; init; } = new();
    public InvalidInterlockedGameplayDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public InterlockedGameplaySystemsReport Report { get; init; } = new();
    public IReadOnlyList<InterlockedGameplayRow> Rows { get; init; } = [];
    public IReadOnlyList<InterlockedGameplayFilePayload> StagingFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record InterlockedGameplaySystemsWriteResult
{
    public InterlockedGameplaySystemsBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
