namespace LLMGameCreator.Application.Design.UnityAlphaInteractiveCampaignPlayer;

public static class UnityAlphaInteractiveCampaignVocabulary
{
    public const string GoalId = "goal_071_unity_alpha_interactive_campaign_player";
    public const string ProductSmokeRoute = "goal-071-unity-alpha-interactive-campaign-player";
    public const string FinalGate = "unity_alpha_interactive_campaign_player_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-071-unity-alpha-interactive-campaign-player";
    public const string Goal070RelativeOutputDirectory = ".llmgc/procedural/goal-070-integrated-campaign-timeline-simulation-matrix";
    public const string StagingRoot = "staging";
    public const string UnityInteractiveCommandPlanStagingRelativePath = "interactive-campaign/unity-interactive-campaign-command-plan.json";

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
        "missing_goal070_source",
        "fake_family_seed_row_id",
        "duplicate_row_id",
        "command_plan_unknown_row",
        "command_plan_skips_required_state_transition",
        "state_hash_unchanged",
        "replay_mismatch",
        "missing_hud_contract",
        "unity_marker_missing",
        "unsafe_path",
        "provider_llm_rag_claim",
        "runtime_gamepackage_schema_mutation_claim",
        "broad_unity_mutation_claim",
        "final_prose_leakage",
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

public sealed record UnityAlphaInteractiveCampaignOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityProof { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 120;
}

public sealed record InteractiveCampaignDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static InteractiveCampaignDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static InteractiveCampaignDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static InteractiveCampaignDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}

public sealed record InteractiveCampaignFilePayload
{
    public string RelativePath { get; init; } = string.Empty;
    public byte[] Bytes { get; init; } = [];
}

public sealed record InteractiveCampaignSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public bool HashMatches { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<InteractiveCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InteractiveCampaignSourceStep
{
    public string StepId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string SourceRef { get; init; } = string.Empty;
    public string StateBeforeHash { get; init; } = string.Empty;
    public string StateAfterHash { get; init; } = string.Empty;
    public IReadOnlyList<string> DeltaIds { get; init; } = [];
}

public sealed record InteractiveCampaignSourceRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string Goal070RowHash { get; init; } = string.Empty;
    public string Goal070InitialStateHash { get; init; } = string.Empty;
    public string Goal070FinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> UpstreamRefs { get; init; } = [];
    public IReadOnlyList<InteractiveCampaignSourceStep> Steps { get; init; } = [];
    public bool StateChanging { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
}

public sealed record InteractiveCampaignSourceBundle
{
    public bool Goal070AcceptedByUserHandoff { get; init; }
    public bool Goal070TimelineEvidenceConsumed { get; init; }
    public bool Goal070UnityProofConsumed { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<InteractiveCampaignSourceRow> Rows { get; init; } = [];
    public IReadOnlyList<InteractiveCampaignFilePayload> BaseStagingFiles { get; init; } = [];
    public IReadOnlyList<InteractiveCampaignSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<InteractiveCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InteractiveCampaignGateRecord
{
    public string GateId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
}

public sealed record InteractiveCampaignSourceManifest
{
    public string SchemaVersion { get; init; } = "unity_alpha_interactive_campaign_source_manifest_v1";
    public string GoalId { get; init; } = UnityAlphaInteractiveCampaignVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = UnityAlphaInteractiveCampaignVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = UnityAlphaInteractiveCampaignVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal070AcceptedByUserHandoff { get; init; }
    public bool Goal070TimelineEvidenceConsumed { get; init; }
    public bool Goal070UnityProofConsumed { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<InteractiveCampaignGateRecord> PreflightGates { get; init; } = [];
    public IReadOnlyList<InteractiveCampaignSourceArtifactReference> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<InteractiveCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InteractiveCampaignRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourceGoal070RowHash { get; init; } = string.Empty;
    public string InitialStateHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string SelectedActionId { get; init; } = string.Empty;
    public string SelectedInputId { get; init; } = string.Empty;
    public string SelectedStepId { get; init; } = string.Empty;
    public string SelectedStateBeforeHash { get; init; } = string.Empty;
    public string SelectedStateAfterHash { get; init; } = string.Empty;
    public IReadOnlyList<InteractiveCampaignAction> Actions { get; init; } = [];
    public bool StateChanging { get; init; }
    public bool HudRenderable { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public string RowHash { get; init; } = string.Empty;
}

public sealed record InteractiveCampaignMatrix
{
    public string SchemaVersion { get; init; } = "unity_alpha_interactive_campaign_row_matrix_v1";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int ActionCount { get; init; }
    public IReadOnlyList<InteractiveCampaignRow> Rows { get; init; } = [];
}

public sealed record FamilySeedSelectorModel
{
    public string SchemaVersion { get; init; } = "unity_alpha_interactive_campaign_family_seed_selector_v1";
    public bool Passed { get; init; }
    public IReadOnlyList<FamilySelectorRow> Families { get; init; } = [];
}

public sealed record FamilySelectorRow
{
    public string FamilyId { get; init; } = string.Empty;
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<string> RowIds { get; init; } = [];
}

public sealed record InputActionScript
{
    public string SchemaVersion { get; init; } = "unity_alpha_interactive_campaign_input_action_script_v1";
    public bool Passed { get; init; }
    public int ActionCount { get; init; }
    public IReadOnlyList<InteractiveCampaignAction> Actions { get; init; } = [];
}

public sealed record InteractiveCampaignAction
{
    public string ActionId { get; init; } = string.Empty;
    public string InputId { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string StepId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string ActionKind { get; init; } = "advance_timeline_step";
    public string SourceRef { get; init; } = string.Empty;
    public string StateBeforeHash { get; init; } = string.Empty;
    public string StateAfterHash { get; init; } = string.Empty;
    public bool DeltaApplied { get; init; }
}

public sealed record StateTransitionLedger
{
    public string SchemaVersion { get; init; } = "unity_alpha_interactive_campaign_state_transition_ledger_v1";
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int TransitionCount { get; init; }
    public IReadOnlyList<StateTransitionLedgerRow> Rows { get; init; } = [];
}

public sealed record StateTransitionLedgerRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public IReadOnlyList<StateTransitionRecord> Transitions { get; init; } = [];
}

public sealed record StateTransitionRecord
{
    public string TransitionId { get; init; } = string.Empty;
    public string ActionId { get; init; } = string.Empty;
    public string StepId { get; init; } = string.Empty;
    public string StateBeforeHash { get; init; } = string.Empty;
    public string StateAfterHash { get; init; } = string.Empty;
    public bool StateChanged { get; init; }
    public bool DeltaApplied { get; init; }
}

public sealed record InteractiveCampaignSaveLoadReplayProof
{
    public string SchemaVersion { get; init; } = "unity_alpha_interactive_campaign_save_load_replay_proof_v1";
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int SaveLoadPassedRowCount { get; init; }
    public int ReplayPassedRowCount { get; init; }
    public IReadOnlyList<InteractiveCampaignSaveLoadReplayRow> Rows { get; init; } = [];
}

public sealed record InteractiveCampaignSaveLoadReplayRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SaveCheckpointHash { get; init; } = string.Empty;
    public string LoadedCheckpointHash { get; init; } = string.Empty;
    public string ExpectedReplayHash { get; init; } = string.Empty;
    public string ReplayHash { get; init; } = string.Empty;
    public bool SaveLoadRoundtripPassed { get; init; }
    public bool ReplayDeterminismPassed { get; init; }
}

public sealed record InteractiveCampaignHudContract
{
    public string SchemaVersion { get; init; } = "unity_alpha_interactive_campaign_hud_contract_v1";
    public bool Passed { get; init; }
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<InteractiveCampaignHudRow> Rows { get; init; } = [];
}

public sealed record InteractiveCampaignHudRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string ActionId { get; init; } = string.Empty;
    public string StepId { get; init; } = string.Empty;
    public string StateBeforeHash { get; init; } = string.Empty;
    public string StateAfterHash { get; init; } = string.Empty;
    public string DeltaSummary { get; init; } = string.Empty;
}

public sealed record InteractiveCampaignUnityCommandPlan
{
    public string SchemaVersion { get; init; } = "unity_alpha_interactive_campaign_command_plan_v1";
    public string GoalId { get; init; } = UnityAlphaInteractiveCampaignVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = UnityAlphaInteractiveCampaignVocabulary.ProductSmokeRoute;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<InteractiveCampaignUnityCommandPlanRow> Rows { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record InteractiveCampaignUnityCommandPlanRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SelectedInputId { get; init; } = string.Empty;
    public string SelectedActionId { get; init; } = string.Empty;
    public string SelectedStepId { get; init; } = string.Empty;
    public IReadOnlyList<string> StepIds { get; init; } = [];
    public IReadOnlyList<string> InputIds { get; init; } = [];
    public IReadOnlyList<string> ActionIds { get; init; } = [];
    public IReadOnlyList<string> StateBeforeHashes { get; init; } = [];
    public IReadOnlyList<string> StateAfterHashes { get; init; } = [];
    public bool DeltaApplied { get; init; }
    public bool HudRendered { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record InteractiveCampaignUnityProof
{
    public bool Passed { get; init; }
    public bool UnityEditorOrPlayerExecuted { get; init; }
    public string BlockerCode { get; init; } = string.Empty;
    public string BlockerMessage { get; init; } = string.Empty;
    public InteractiveCampaignUnityProofSummary PlayerProof { get; init; } = new();
    public IReadOnlyList<InteractiveCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InteractiveCampaignUnityProofSummary
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
    public IReadOnlyList<InteractiveCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InteractiveCampaignInvalidDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = "unity_alpha_interactive_campaign_invalid_diagnostics_matrix_v1";
    public bool Passed { get; init; }
    public IReadOnlyList<InteractiveCampaignInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record InteractiveCampaignInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<InteractiveCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InteractiveCampaignPreviewExportPayload
{
    public string SchemaVersion { get; init; } = "unity_alpha_interactive_campaign_preview_export_payload_v1";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public IReadOnlyList<InteractiveCampaignPreviewExportRow> Rows { get; init; } = [];
}

public sealed record InteractiveCampaignPreviewExportRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SelectedActionId { get; init; } = string.Empty;
    public string SelectedStepId { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> PreviewMarkers { get; init; } = [];
}

public sealed record InteractiveCampaignReport
{
    public string SchemaVersion { get; init; } = "unity_alpha_interactive_campaign_player_report_v1";
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal070AcceptedByUserHandoff { get; init; }
    public bool SourceFactsConsumed { get; init; }
    public bool RowMatrixPassed { get; init; }
    public bool SelectorPassed { get; init; }
    public bool InputActionScriptPassed { get; init; }
    public bool StateTransitionLedgerPassed { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public bool HudContractPassed { get; init; }
    public bool UnityCommandPlanPassed { get; init; }
    public bool UnityProofPassed { get; init; }
    public int? UnityExitCode { get; init; }
    public int? PlayerExitCode { get; init; }
    public bool AllInteractiveMarkersMatched { get; init; }
    public bool PreviewExportPayloadPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public int RowCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int ActionCount { get; init; }
    public int TransitionCount { get; init; }
    public int SaveLoadPassedRowCount { get; init; }
    public int ReplayPassedRowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string MatrixHash { get; init; } = string.Empty;
    public string SelectorHash { get; init; } = string.Empty;
    public string InputActionScriptHash { get; init; } = string.Empty;
    public string StateTransitionLedgerHash { get; init; } = string.Empty;
    public string SaveLoadReplayProofHash { get; init; } = string.Empty;
    public string HudContractHash { get; init; } = string.Empty;
    public string UnityCommandPlanHash { get; init; } = string.Empty;
    public string UnityProofSummaryHash { get; init; } = string.Empty;
    public string PreviewExportPayloadHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<InteractiveCampaignDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InteractiveCampaignBuildResult
{
    public InteractiveCampaignSourceManifest SourceManifest { get; init; } = new();
    public InteractiveCampaignMatrix Matrix { get; init; } = new();
    public FamilySeedSelectorModel Selector { get; init; } = new();
    public InputActionScript InputActionScript { get; init; } = new();
    public StateTransitionLedger StateTransitionLedger { get; init; } = new();
    public InteractiveCampaignSaveLoadReplayProof SaveLoadReplayProof { get; init; } = new();
    public InteractiveCampaignHudContract HudContract { get; init; } = new();
    public InteractiveCampaignUnityCommandPlan UnityCommandPlan { get; init; } = new();
    public InteractiveCampaignUnityProofSummary UnityProofSummary { get; init; } = new();
    public InteractiveCampaignInvalidDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public InteractiveCampaignPreviewExportPayload PreviewExportPayload { get; init; } = new();
    public InteractiveCampaignReport Report { get; init; } = new();
    public IReadOnlyList<InteractiveCampaignFilePayload> StagingFiles { get; init; } = [];
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record InteractiveCampaignWriteResult
{
    public InteractiveCampaignBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
