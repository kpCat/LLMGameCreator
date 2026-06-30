namespace LLMGameCreator.Application.Design.GameplayConsequenceDepthMatrix;

public static class GameplayConsequenceDepthMatrixVocabulary
{
    public const string GoalId = "goal_063_gameplay_consequence_depth_matrix";
    public const string ProductSmokeRoute = "goal-063-gameplay-consequence-depth-matrix";
    public const string FinalGate = "gameplay_consequence_depth_matrix_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-063-gameplay-consequence-depth-matrix";
    public const string Goal060RelativeOutputDirectory = ".llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix";
    public const string Goal061RelativeOutputDirectory = ".llmgc/procedural/goal-061-full-campaign-playable-review-package-rc";
    public const string Goal062RelativeOutputDirectory = ".llmgc/procedural/goal-062-constrained-spatial-detail-generation";
    public const string StagingRoot = "staging";
    public const string UnityGameplayCommandPlanStagingRelativePath = "gameplay-consequence/unity-gameplay-consequence-command-plan.json";

    public const string SourceManifestSchemaVersion = "gameplay_consequence_depth_matrix_source_manifest_v1";
    public const string CatalogSchemaVersion = "gameplay_consequence_catalog_v1";
    public const string CommandPlanMatrixSchemaVersion = "gameplay_consequence_command_plan_matrix_v1";
    public const string RuntimeStateDeltaMatrixSchemaVersion = "gameplay_consequence_runtime_state_delta_matrix_v1";
    public const string SaveLoadReplayAuditSchemaVersion = "gameplay_consequence_save_load_replay_audit_v1";
    public const string FamilySummarySchemaVersion = "gameplay_consequence_family_summary_v1";
    public const string UnityCommandPlanSchemaVersion = "gameplay_consequence_unity_command_plan_v1";
    public const string UnityProofSummarySchemaVersion = "gameplay_consequence_unity_proof_summary_v1";
    public const string PreviewExportPayloadSchemaVersion = "gameplay_consequence_preview_export_payload_v1";
    public const string InvalidDiagnosticsMatrixSchemaVersion = "invalid_gameplay_consequence_depth_matrix_v1";
    public const string ReportSchemaVersion = "gameplay_consequence_depth_matrix_report_v1";

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
        "missing_goal060_package_row",
        "missing_goal061_review_package_row",
        "missing_goal062_spatial_detail_row",
        "fake_family",
        "fake_seed",
        "fake_package_id",
        "fake_command_id",
        "duplicate_command_id",
        "command_without_state_delta",
        "delta_without_before_after_values",
        "replay_mismatch",
        "save_load_mismatch",
        "row_hash_collision",
        "no_meaningful_variance",
        "unsafe_path",
        "final_prose_treated_as_gameplay_consequence",
        "provider_llm_rag_media_generation_claim",
        "runtime_ui_unity_broad_mutation_claim",
        "gamepackage_schema_mutation_claim",
        "lua_arbitrary_execution_or_source_claim",
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

public sealed record GameplayConsequenceDepthMatrixOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record GameplayConsequenceDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static GameplayConsequenceDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static GameplayConsequenceDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static GameplayConsequenceDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record GameplayConsequenceSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<GameplayConsequenceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record GameplayConsequenceFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record GameplayConsequenceSourceRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourcePackageRowRef { get; init; } = string.Empty;
    public string SourceReviewPackageRowRef { get; init; } = string.Empty;
    public string SourceSpatialDetailRowRef { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string Goal060PackageRelativePath { get; init; } = string.Empty;
    public string ReviewPackageRelativePath { get; init; } = string.Empty;
    public string SpatialDetailRowHash { get; init; } = string.Empty;
    public string SpatialVarianceMarker { get; init; } = string.Empty;
    public bool Goal060RuntimeStateChanged { get; init; }
    public bool Goal060SaveLoadRoundtripPassed { get; init; }
    public bool Goal061SaveLoadReplayVerified { get; init; }
    public bool Goal062Reachable { get; init; }
    public bool Goal062RouteVerified { get; init; }
    public IReadOnlyList<string> Goal060ChangedStateKeys { get; init; } = [];
    public IReadOnlyList<string> Goal061RuntimeCommandIds { get; init; } = [];
    public IReadOnlyList<string> Goal061ReviewCommandSteps { get; init; } = [];
}

public sealed record GameplayConsequenceSourceBundle
{
    public bool Goal060AcceptedByUserHandoff { get; init; }
    public bool Goal061AcceptedByUserHandoff { get; init; }
    public bool Goal062AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewRowsConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public bool Goal060RuntimeProofConsumed { get; init; }
    public bool Goal061SaveLoadReplayConsumed { get; init; }
    public bool Goal062UnityProofConsumed { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<GameplayConsequenceSourceRow> Rows { get; init; } = [];
    public IReadOnlyList<GameplayConsequenceFilePayload> BaseStagingFiles { get; init; } = [];
    public IReadOnlyList<GameplayConsequenceSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<GameplayConsequenceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record GameplayConsequenceGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record GameplayConsequenceSourceManifest
{
    public string SchemaVersion { get; init; } = GameplayConsequenceDepthMatrixVocabulary.SourceManifestSchemaVersion;
    public string GoalId { get; init; } = GameplayConsequenceDepthMatrixVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = GameplayConsequenceDepthMatrixVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = GameplayConsequenceDepthMatrixVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal060AcceptedByUserHandoff { get; init; }
    public bool Goal061AcceptedByUserHandoff { get; init; }
    public bool Goal062AcceptedByUserHandoff { get; init; }
    public bool Goal060PackageRowsConsumed { get; init; }
    public bool Goal061ReviewRowsConsumed { get; init; }
    public bool Goal062SpatialRowsConsumed { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<GameplayConsequenceGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<GameplayConsequenceSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<GameplayConsequenceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record GameplayConsequenceFamilyTemplate
{
    public string FamilyId { get; init; } = string.Empty;
    public string ConsequenceShape { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredStateAxes { get; init; } = [];
    public IReadOnlyList<string> RequiredCommandTypes { get; init; } = [];
    public IReadOnlyList<string> ForbiddenClaims { get; init; } = [];
}

public sealed record GameplayConsequenceCatalog
{
    public string SchemaVersion { get; init; } = GameplayConsequenceDepthMatrixVocabulary.CatalogSchemaVersion;
    public string GoalId { get; init; } = GameplayConsequenceDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FamilyTemplateCount { get; init; }
    public IReadOnlyList<GameplayConsequenceFamilyTemplate> Families { get; init; } = [];
}

public sealed record GameplayConsequenceCommandStep
{
    public string StepId { get; init; } = string.Empty;
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string DeltaId { get; init; } = string.Empty;
    public string TargetRef { get; init; } = string.Empty;
    public string ConsequenceShape { get; init; } = string.Empty;
    public bool ExpectedStateChanging { get; init; } = true;
    public IReadOnlyDictionary<string, string> ExpectedChanges { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<string> SourceRefs { get; init; } = [];
}

public sealed record GameplayConsequenceCommandPlanRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourcePackageRowRef { get; init; } = string.Empty;
    public string SourceReviewPackageRowRef { get; init; } = string.Empty;
    public string SourceSpatialDetailRowRef { get; init; } = string.Empty;
    public IReadOnlyList<GameplayConsequenceCommandStep> Commands { get; init; } = [];
    public int StateChangingStepCount { get; init; }
}

public sealed record GameplayConsequenceCommandPlanMatrix
{
    public string SchemaVersion { get; init; } = GameplayConsequenceDepthMatrixVocabulary.CommandPlanMatrixSchemaVersion;
    public string GoalId { get; init; } = GameplayConsequenceDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<GameplayConsequenceCommandPlanRow> Rows { get; init; } = [];
}

public sealed record GameplayConsequenceStateSnapshot
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public int StepIndex { get; init; }
    public IReadOnlyDictionary<string, string> Values { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string StateHash { get; init; } = string.Empty;
}

public sealed record GameplayConsequenceStateDelta
{
    public string DeltaId { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string ExpectedValue { get; init; } = string.Empty;
    public string ActualValue { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record GameplayConsequenceStateTransitionProof
{
    public string RowId { get; init; } = string.Empty;
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string DeltaId { get; init; } = string.Empty;
    public GameplayConsequenceStateSnapshot Before { get; init; } = new();
    public GameplayConsequenceStateSnapshot After { get; init; } = new();
    public IReadOnlyList<GameplayConsequenceStateDelta> Deltas { get; init; } = [];
    public bool ExpectedVsActualPassed { get; init; }
    public bool StateChanged { get; init; }
}

public sealed record GameplayConsequenceVarianceContribution
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string ContributionId { get; init; } = string.Empty;
    public IReadOnlyList<string> MeaningfulAxes { get; init; } = [];
    public IReadOnlyDictionary<string, string> FinalStateHighlights { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record GameplayConsequenceRowProof
{
    public string SchemaVersion { get; init; } = "gameplay_consequence_row_proof_v1";
    public string GoalId { get; init; } = GameplayConsequenceDepthMatrixVocabulary.GoalId;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourcePackageRowRef { get; init; } = string.Empty;
    public string SourceReviewPackageRowRef { get; init; } = string.Empty;
    public string SourceSpatialDetailRowRef { get; init; } = string.Empty;
    public GameplayConsequenceStateSnapshot BeforeState { get; init; } = new();
    public GameplayConsequenceStateSnapshot AfterState { get; init; } = new();
    public IReadOnlyList<GameplayConsequenceStateTransitionProof> Transitions { get; init; } = [];
    public int StateChangingStepCount { get; init; }
    public bool StateTransitionProofPassed { get; init; }
    public bool SerializerRoundtripPassed { get; init; }
    public bool ReplayDeterminismPassed { get; init; }
    public GameplayConsequenceVarianceContribution VarianceContribution { get; init; } = new();
    public string RowHash { get; init; } = string.Empty;
}

public sealed record GameplayConsequenceRuntimeStateDeltaMatrix
{
    public string SchemaVersion { get; init; } = GameplayConsequenceDepthMatrixVocabulary.RuntimeStateDeltaMatrixSchemaVersion;
    public string GoalId { get; init; } = GameplayConsequenceDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public IReadOnlyList<GameplayConsequenceRowProof> Rows { get; init; } = [];
}

public sealed record GameplayConsequenceSaveLoadReplayAuditRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public bool SaveLoadRoundtripPassed { get; init; }
    public bool ReplayDeterminismPassed { get; init; }
    public string SerializedAfterStateHash { get; init; } = string.Empty;
    public string RestoredAfterStateHash { get; init; } = string.Empty;
    public string FirstReplayHash { get; init; } = string.Empty;
    public string SecondReplayHash { get; init; } = string.Empty;
}

public sealed record GameplayConsequenceSaveLoadReplayAudit
{
    public string SchemaVersion { get; init; } = GameplayConsequenceDepthMatrixVocabulary.SaveLoadReplayAuditSchemaVersion;
    public string GoalId { get; init; } = GameplayConsequenceDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int SaveLoadPassedRowCount { get; init; }
    public int ReplayPassedRowCount { get; init; }
    public IReadOnlyList<GameplayConsequenceSaveLoadReplayAuditRow> Rows { get; init; } = [];
}

public sealed record GameplayConsequenceFamilySummaryRow
{
    public string FamilyId { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public IReadOnlyList<string> ConsequenceShapes { get; init; } = [];
    public IReadOnlyList<string> MeaningfulVarianceAxes { get; init; } = [];
    public IReadOnlyList<string> RowHashes { get; init; } = [];
}

public sealed record GameplayConsequenceFamilySummary
{
    public string SchemaVersion { get; init; } = GameplayConsequenceDepthMatrixVocabulary.FamilySummarySchemaVersion;
    public string GoalId { get; init; } = GameplayConsequenceDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public bool MeaningfulVariancePassed { get; init; }
    public IReadOnlyList<GameplayConsequenceFamilySummaryRow> Families { get; init; } = [];
}

public sealed record GameplayConsequenceUnityCommandRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public IReadOnlyList<string> StepIds { get; init; } = [];
    public IReadOnlyList<string> DeltaIds { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record GameplayConsequenceUnityCommandPlan
{
    public string SchemaVersion { get; init; } = GameplayConsequenceDepthMatrixVocabulary.UnityCommandPlanSchemaVersion;
    public string GoalId { get; init; } = GameplayConsequenceDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = GameplayConsequenceDepthMatrixVocabulary.FinalGate;
    public IReadOnlyList<GameplayConsequenceUnityCommandRow> Rows { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record GameplayConsequenceUnityProofSummary
{
    public string SchemaVersion { get; init; } = GameplayConsequenceDepthMatrixVocabulary.UnityProofSummarySchemaVersion;
    public string GoalId { get; init; } = GameplayConsequenceDepthMatrixVocabulary.GoalId;
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
    public IReadOnlyList<GameplayConsequenceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record GameplayConsequenceUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public GameplayConsequenceUnityProofSummary PlayerProof { get; init; } = new();
    public IReadOnlyList<GameplayConsequenceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record GameplayConsequencePreviewExportRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourcePackageRef { get; init; } = string.Empty;
    public string SourceSpatialRef { get; init; } = string.Empty;
    public string GameplayStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> PreviewMarkers { get; init; } = [];
}

public sealed record GameplayConsequencePreviewExportPayload
{
    public string SchemaVersion { get; init; } = GameplayConsequenceDepthMatrixVocabulary.PreviewExportPayloadSchemaVersion;
    public string GoalId { get; init; } = GameplayConsequenceDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<GameplayConsequencePreviewExportRow> Rows { get; init; } = [];
}

public sealed record InvalidGameplayConsequenceScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<GameplayConsequenceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidGameplayConsequenceDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = GameplayConsequenceDepthMatrixVocabulary.InvalidDiagnosticsMatrixSchemaVersion;
    public string GoalId { get; init; } = GameplayConsequenceDepthMatrixVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<InvalidGameplayConsequenceScenario> Scenarios { get; init; } = [];
}

public sealed record GameplayConsequenceDepthMatrixReport
{
    public string SchemaVersion { get; init; } = GameplayConsequenceDepthMatrixVocabulary.ReportSchemaVersion;
    public string GoalId { get; init; } = GameplayConsequenceDepthMatrixVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = GameplayConsequenceDepthMatrixVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = GameplayConsequenceDepthMatrixVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal062AcceptedByUserHandoff { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool CommandPlanPassed { get; init; }
    public bool StateDeltaProofPassed { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public bool MeaningfulVariancePassed { get; init; }
    public bool UnityCommandPlanPassed { get; init; }
    public bool UnityProofPassed { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public bool AllGameplayMarkersMatched { get; init; }
    public bool PreviewExportPayloadPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string CatalogHash { get; init; } = string.Empty;
    public string CommandPlanHash { get; init; } = string.Empty;
    public string RuntimeStateDeltaMatrixHash { get; init; } = string.Empty;
    public string SaveLoadReplayAuditHash { get; init; } = string.Empty;
    public string FamilySummaryHash { get; init; } = string.Empty;
    public string UnityCommandPlanHash { get; init; } = string.Empty;
    public string UnityProofSummaryHash { get; init; } = string.Empty;
    public string PreviewExportPayloadHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<GameplayConsequenceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record GameplayConsequenceDepthMatrixBuildResult
{
    public GameplayConsequenceSourceManifest SourceManifest { get; init; } = new();
    public GameplayConsequenceCatalog Catalog { get; init; } = new();
    public GameplayConsequenceCommandPlanMatrix CommandPlanMatrix { get; init; } = new();
    public GameplayConsequenceRuntimeStateDeltaMatrix RuntimeStateDeltaMatrix { get; init; } = new();
    public GameplayConsequenceSaveLoadReplayAudit SaveLoadReplayAudit { get; init; } = new();
    public GameplayConsequenceFamilySummary FamilySummary { get; init; } = new();
    public GameplayConsequenceUnityCommandPlan UnityCommandPlan { get; init; } = new();
    public GameplayConsequenceUnityProofSummary UnityProofSummary { get; init; } = new();
    public GameplayConsequencePreviewExportPayload PreviewExportPayload { get; init; } = new();
    public InvalidGameplayConsequenceDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public GameplayConsequenceDepthMatrixReport Report { get; init; } = new();
    public IReadOnlyList<GameplayConsequenceRowProof> RowProofs { get; init; } = [];
    public IReadOnlyList<GameplayConsequenceFilePayload> StagingFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record GameplayConsequenceDepthMatrixWriteResult
{
    public GameplayConsequenceDepthMatrixBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
