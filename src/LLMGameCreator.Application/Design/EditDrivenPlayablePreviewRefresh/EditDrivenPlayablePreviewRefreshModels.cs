namespace LLMGameCreator.Application.Design.EditDrivenPlayablePreviewRefresh;

public static class EditDrivenPlayablePreviewRefreshVocabulary
{
    public const string GoalId = "goal_076_edit_driven_playable_preview_refresh";
    public const string ProductSmokeRoute = "goal-076-edit-driven-playable-preview-refresh";
    public const string FinalGate = "edit_driven_playable_preview_refresh_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-076-edit-driven-playable-preview-refresh";
    public const string Goal075RelativeOutputDirectory =
        ".llmgc/procedural/goal-075-schema-driven-campaign-edit-validate-apply-loop";
    public const string Goal075FinalGate = "schema_driven_campaign_edit_validate_apply_loop_verification";
    public const string Goal075AcceptedHandoffText =
        "schema_driven_campaign_edit_validate_apply_loop_verification passed before Goal 076";

    public static readonly IReadOnlyList<string> RequiredBindingGroups =
    [
        "playable_refresh_status"
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_staged_handoff_manifest",
        "tampered_staged_handoff_manifest"
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

    public static string DomainForField(string fieldId) =>
        fieldId switch
        {
            "gameplay_consequence_summary.consequence_intensity" => "gameplay_consequence",
            "living_world_npc_faction_summary.faction_pressure" => "living_world_faction",
            "settlement_construction_destruction_production_summary.production_focus" => "settlement_production",
            "narrative_quest_dialogue_event_summary.event_intent" => "narrative_event_intent",
            "combat_magic_boss_summary.status_pressure" => "combat_magic_status",
            "weather_daynight_crisis_summary.crisis_pressure" => "weather_crisis_pressure",
            _ => "unknown"
        };

    public static string PackageTargetForDomain(string domainId) =>
        domainId switch
        {
            "gameplay_consequence" => "generated-content/gameplay/consequence-summary",
            "living_world_faction" => "generated-content/world/faction-pressure",
            "settlement_production" => "generated-content/settlement/production-focus",
            "narrative_event_intent" => "generated-content/narrative/event-intent",
            "combat_magic_status" => "generated-content/combat/status-pressure",
            "weather_crisis_pressure" => "generated-content/world/crisis-pressure",
            _ => "generated-content/campaign/unknown"
        };
}

public sealed record EditDrivenPlayablePreviewRefreshDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static EditDrivenPlayablePreviewRefreshDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static EditDrivenPlayablePreviewRefreshDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };
}

public sealed record EditDrivenSourceArtifactReference
{
    public string SourceGoal { get; init; } = "Goal075";
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
}

public sealed record EditDrivenSourceArtifactManifest
{
    public string SchemaVersion { get; init; } = "edit_driven_playable_preview_source_artifact_manifest_v1";
    public string GoalId { get; init; } = EditDrivenPlayablePreviewRefreshVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = EditDrivenPlayablePreviewRefreshVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenPlayablePreviewRefreshVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal075AcceptedByUserHandoff { get; init; }
    public bool Goal075ReportWasGreenProducedForReview { get; init; }
    public bool Goal075ParentActivationBindingPassed { get; init; }
    public string Goal075ReportHash { get; init; } = string.Empty;
    public int SourceArtifactCount { get; init; }
    public IReadOnlyList<EditDrivenSourceArtifactReference> SourceArtifacts { get; init; } = [];
    public IReadOnlyList<EditDrivenPlayablePreviewRefreshDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenPlayablePreviewRefreshManifest
{
    public string SchemaVersion { get; init; } = "edit_driven_playable_preview_refresh_manifest_v1";
    public string GoalId { get; init; } = EditDrivenPlayablePreviewRefreshVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = EditDrivenPlayablePreviewRefreshVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenPlayablePreviewRefreshVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public string SourceGoal075ReportHash { get; init; } = string.Empty;
    public string PreviewRefreshHash { get; init; } = string.Empty;
    public string RefreshPlanHash { get; init; } = string.Empty;
    public string HandoffManifestHash { get; init; } = string.Empty;
    public int ChangedRowCount { get; init; }
    public int PackageTargetCount { get; init; }
    public bool StateTransitionProofPassed { get; init; }
    public bool GamePackageRefreshPlanPassed { get; init; }
    public bool StagedHandoffManifestPassed { get; init; }
    public bool TamperNegativeProofPassed { get; init; }
    public bool WinFormsBindingPassed { get; init; }
    public bool QualityGatePassed { get; init; }
}

public sealed record EditDrivenRefreshRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string BeforeHash { get; init; } = string.Empty;
    public string AfterHash { get; init; } = string.Empty;
    public string RollbackHash { get; init; } = string.Empty;
    public string ReplayHash { get; init; } = string.Empty;
    public bool StateChanged { get; init; }
    public bool RollbackRestored { get; init; }
    public bool ReplayRestoredAfter { get; init; }
    public string PreviewRefreshKey { get; init; } = string.Empty;
    public string PreviewAfterHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenAppliedChange> AppliedChanges { get; init; } = [];
    public IReadOnlyList<string> PackageLogicalTargets { get; init; } = [];
}

public sealed record EditDrivenAppliedChange
{
    public string CandidateId { get; init; } = string.Empty;
    public string CandidateKind { get; init; } = string.Empty;
    public string FieldId { get; init; } = string.Empty;
    public string DomainId { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string PackageLogicalTarget { get; init; } = string.Empty;
}

public sealed record EditDrivenStateTransitionProof
{
    public string SchemaVersion { get; init; } = "edit_driven_playable_preview_state_transition_proof_v1";
    public bool Passed { get; init; }
    public string SourceGoal075ReportHash { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public int RollbackRestoredRowCount { get; init; }
    public int ReplayRestoredAfterRowCount { get; init; }
    public IReadOnlyList<EditDrivenRefreshRow> Rows { get; init; } = [];
}

public sealed record EditDrivenGamePackageRefreshPlan
{
    public string SchemaVersion { get; init; } = "edit_driven_gamepackage_refresh_plan_v1";
    public bool Passed { get; init; }
    public bool PublicGamePackageSchemaMutationRequired { get; init; }
    public string FullMaterializationDisposition { get; init; } = string.Empty;
    public string PreviewExportRefreshPayloadRef { get; init; } = string.Empty;
    public string PreviewExportRefreshPayloadHash { get; init; } = string.Empty;
    public int RowCount { get; init; }
    public int TargetCount { get; init; }
    public IReadOnlyList<EditDrivenGamePackageRefreshRow> Rows { get; init; } = [];
}

public sealed record EditDrivenGamePackageRefreshRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string RefreshKey { get; init; } = string.Empty;
    public string SourceAfterHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenGamePackageRefreshTarget> Targets { get; init; } = [];
}

public sealed record EditDrivenGamePackageRefreshTarget
{
    public string FieldId { get; init; } = string.Empty;
    public string DomainId { get; init; } = string.Empty;
    public string LogicalPackagePath { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string ValidationRequirement { get; init; } = string.Empty;
}

public sealed record EditDrivenUnityPlayerHandoffManifest
{
    public string SchemaVersion { get; init; } = "edit_driven_unity_player_handoff_manifest_v1";
    public string GoalId { get; init; } = EditDrivenPlayablePreviewRefreshVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = EditDrivenPlayablePreviewRefreshVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenPlayablePreviewRefreshVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string ManifestRelativePath { get; init; } =
        EditDrivenPlayablePreviewRefreshVocabulary.RelativeOutputDirectory + "/unity-player-handoff-manifest.json";
    public string SourceGoal075ReportHash { get; init; } = string.Empty;
    public string PreviewRefreshHash { get; init; } = string.Empty;
    public string RefreshPlanHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ExpectedPackageLogicalTargets { get; init; } = [];
    public IReadOnlyList<string> PlayerFacingScenarioIds { get; init; } = [];
    public IReadOnlyList<EditDrivenUnityPlayerHandoffRow> Rows { get; init; } = [];
}

public sealed record EditDrivenUnityPlayerHandoffRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PreviewRefreshKey { get; init; } = string.Empty;
    public string AfterHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ExpectedPackageLogicalTargets { get; init; } = [];
    public IReadOnlyList<string> ExpectedPlayerMarkers { get; init; } = [];
}

public sealed record EditDrivenStagedHandoffProof
{
    public string SchemaVersion { get; init; } = "edit_driven_staged_handoff_proof_v1";
    public bool Passed { get; init; }
    public bool ManifestLoaded { get; init; }
    public bool HashMatched { get; init; }
    public bool SourceHashMatched { get; init; }
    public bool PreviewHashMatched { get; init; }
    public bool PackageTargetsPresent { get; init; }
    public int RowCount { get; init; }
    public string ManifestRelativePath { get; init; } = string.Empty;
    public string ManifestHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenPlayablePreviewRefreshDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenTamperNegativeProof
{
    public string SchemaVersion { get; init; } = "edit_driven_tamper_negative_proof_v1";
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<EditDrivenTamperNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record EditDrivenTamperNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<EditDrivenPlayablePreviewRefreshDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenWinFormsBindingInventory
{
    public string SchemaVersion { get; init; } = "edit_driven_playable_refresh_winforms_binding_inventory_v1";
    public bool Passed { get; init; }
    public bool ParentPageRefreshTabDeclared { get; init; }
    public bool ParentPageRefreshEvidenceServiceLoaded { get; init; }
    public bool ParentPageRefreshControlBound { get; init; }
    public bool ParentPageActivationBindsGoal076Data { get; init; }
    public IReadOnlyList<EditDrivenWinFormsBindingGroup> Groups { get; init; } = [];
    public IReadOnlyList<EditDrivenPlayablePreviewRefreshDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenWinFormsBindingGroup
{
    public string GroupId { get; init; } = string.Empty;
    public string ControlName { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public bool SeparateUserControl { get; init; }
    public bool BindsGoal076Data { get; init; }
}

public sealed record EditDrivenQualityGateScan
{
    public string SchemaVersion { get; init; } = "edit_driven_playable_refresh_quality_gate_scan_v1";
    public bool Passed { get; init; }
    public int ScannedFileCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public int FilesOver1000LinesCount { get; init; }
    public int MinifiedSourceFileCount { get; init; }
    public int AlphaRuntimeBootstrapLineCount { get; init; }
    public bool ParentUiBindingPassed { get; init; }
    public bool ReportOnlySmokeDetected { get; init; }
    public IReadOnlyList<EditDrivenQualityFileScan> Files { get; init; } = [];
    public IReadOnlyList<EditDrivenPlayablePreviewRefreshDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenQualityFileScan
{
    public string RelativePath { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public int ByteCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public bool MinifiedSourceCandidate { get; init; }
}

public sealed record EditDrivenPlayablePreviewRefreshReport
{
    public string SchemaVersion { get; init; } = "edit_driven_playable_preview_refresh_report_v1";
    public string GoalId { get; init; } = EditDrivenPlayablePreviewRefreshVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = EditDrivenPlayablePreviewRefreshVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = EditDrivenPlayablePreviewRefreshVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal075AcceptedByUserHandoff { get; init; }
    public bool Goal075ImplementationGreen { get; init; }
    public bool Goal075WinFormsParentActivationBindingPassed { get; init; }
    public int ChangedRowCount { get; init; }
    public int AppliedChangeCount { get; init; }
    public int PackageTargetCount { get; init; }
    public string SourceGoal075ReportHash { get; init; } = string.Empty;
    public string BeforeStateHash { get; init; } = string.Empty;
    public string AfterStateHash { get; init; } = string.Empty;
    public string RollbackStateHash { get; init; } = string.Empty;
    public string ReplayStateHash { get; init; } = string.Empty;
    public string PreviewRefreshHash { get; init; } = string.Empty;
    public string RefreshPlanHash { get; init; } = string.Empty;
    public string HandoffManifestHash { get; init; } = string.Empty;
    public string TamperNegativeProofHash { get; init; } = string.Empty;
    public string WinFormsBindingHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<EditDrivenPlayablePreviewRefreshDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditDrivenPlayablePreviewRefreshBuildResult
{
    public EditDrivenSourceArtifactManifest SourceArtifactManifest { get; init; } = new();
    public EditDrivenPlayablePreviewRefreshManifest PlayablePreviewRefreshManifest { get; init; } = new();
    public EditDrivenStateTransitionProof StateTransitionProof { get; init; } = new();
    public EditDrivenGamePackageRefreshPlan GamePackageRefreshPlan { get; init; } = new();
    public EditDrivenUnityPlayerHandoffManifest UnityPlayerHandoffManifest { get; init; } = new();
    public EditDrivenStagedHandoffProof StagedHandoffProof { get; init; } = new();
    public EditDrivenTamperNegativeProof TamperNegativeProof { get; init; } = new();
    public EditDrivenWinFormsBindingInventory WinFormsBindingInventory { get; init; } = new();
    public EditDrivenQualityGateScan QualityGateScan { get; init; } = new();
    public EditDrivenPlayablePreviewRefreshReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record EditDrivenPlayablePreviewRefreshWriteResult
{
    public EditDrivenPlayablePreviewRefreshBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
