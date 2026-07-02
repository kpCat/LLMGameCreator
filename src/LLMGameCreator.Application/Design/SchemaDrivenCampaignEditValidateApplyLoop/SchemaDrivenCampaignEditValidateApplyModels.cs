namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;

public static class SchemaDrivenCampaignEditVocabulary
{
    public const string GoalId = "goal_075_schema_driven_campaign_edit_validate_apply_loop";
    public const string ProductSmokeRoute = "goal-075-schema-driven-campaign-edit-validate-apply-loop";
    public const string FinalGate = "schema_driven_campaign_edit_validate_apply_loop_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-075-schema-driven-campaign-edit-validate-apply-loop";

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

    public static readonly IReadOnlyList<string> RequiredBindingGroups =
    [
        "row_selector",
        "editable_field_summary",
        "validation_diagnostics",
        "apply_rollback_summary"
    ];

    public static readonly IReadOnlyList<string> RequiredInvalidScenarioIds =
    [
        "unknown_row_id",
        "unknown_field_id",
        "illegal_field_domain",
        "invalid_value_shape",
        "unsafe_free_form_prose",
        "fake_provenance",
        "candidate_as_applied_without_validation",
        "rollback_target_missing",
        "before_after_hash_unchanged_for_edit",
        "cross_family_leakage",
        "llm_provider_rag_media_network_claim",
        "runtime_gamepackage_ui_broad_mutation_claim",
        "unity_mutation_claim",
        "lua_generated_code_claim",
        "nondeterministic_ordering",
        "absolute_path_evidence"
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

public sealed record CampaignEditDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static CampaignEditDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static CampaignEditDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };
}

public sealed record CampaignEditSourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactRelativePath { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
    public bool Exists { get; init; }
}

public sealed record CampaignEditSourceRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string InteractiveRowHash { get; init; } = string.Empty;
    public string InitialStateHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public bool StateChanging { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public IReadOnlyList<string> SourceRefs { get; init; } = [];
}

public sealed record CampaignEditSourceBundle
{
    public bool Goal074AcceptedByUserHandoff { get; init; }
    public bool Goal072RemainsHistoricalBlocked { get; init; }
    public bool Goal031And032RemainProducedForReview { get; init; }
    public IReadOnlyList<string> FamilyIds { get; init; } = [];
    public IReadOnlyList<string> SeedIds { get; init; } = [];
    public IReadOnlyList<CampaignEditSourceRow> Rows { get; init; } = [];
    public IReadOnlyList<CampaignEditSourceArtifactReference> SourceArtifacts { get; init; } = [];
    public IReadOnlyList<CampaignEditDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CampaignEditSourceManifest
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_edit_source_manifest_v1";
    public string GoalId { get; init; } = SchemaDrivenCampaignEditVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = SchemaDrivenCampaignEditVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = SchemaDrivenCampaignEditVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public bool Goal074AcceptedByUserHandoff { get; init; }
    public bool Goal072RemainsHistoricalBlocked { get; init; }
    public bool Goal031And032RemainProducedForReview { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public IReadOnlyList<CampaignEditSourceArtifactReference> SourceArtifacts { get; init; } = [];
    public IReadOnlyList<CampaignEditDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record EditableSchemaFieldCatalog
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_editable_field_catalog_v1";
    public bool Passed { get; init; }
    public int FieldCount { get; init; }
    public IReadOnlyList<EditableSchemaField> Fields { get; init; } = [];
}

public sealed record EditableSchemaField
{
    public string FieldId { get; init; } = string.Empty;
    public string SchemaGroupId { get; init; } = string.Empty;
    public string DomainId { get; init; } = string.Empty;
    public string ValueShape { get; init; } = "enum";
    public IReadOnlyList<string> AllowedValues { get; init; } = [];
    public bool Editable { get; init; }
    public bool FinalProseAllowed { get; init; }
    public string SourcePath { get; init; } = string.Empty;
}

public sealed record ChangeSetCatalog
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_change_set_catalog_v1";
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int CandidateCount { get; init; }
    public int ManualCandidateCount { get; init; }
    public int AutoSuggestionCandidateCount { get; init; }
    public IReadOnlyList<CampaignChangeSetCandidate> Candidates { get; init; } = [];
}

public sealed record CampaignChangeSetCandidate
{
    public string CandidateId { get; init; } = string.Empty;
    public string CandidateKind { get; init; } = "manual";
    public string CandidateState { get; init; } = "candidate";
    public bool ValidatedBeforeApply { get; init; }
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string SourceFamilyId { get; init; } = string.Empty;
    public string FieldId { get; init; } = string.Empty;
    public string FieldDomain { get; init; } = string.Empty;
    public string ProposedValueKind { get; init; } = "enum";
    public string BeforeValue { get; init; } = string.Empty;
    public string ProposedValue { get; init; } = string.Empty;
    public string ProvenanceKind { get; init; } = "manual_user";
    public string EvidenceRef { get; init; } = string.Empty;
    public string RollbackTargetRowId { get; init; } = string.Empty;
    public string ExpectedBeforeHash { get; init; } = string.Empty;
    public string ExpectedAfterHash { get; init; } = string.Empty;
    public bool DeterministicOrder { get; init; } = true;
    public IReadOnlyList<string> ClaimTags { get; init; } = [];
}

public sealed record ValidationDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_validation_diagnostics_matrix_v1";
    public bool Passed { get; init; }
    public int ValidCandidateCount { get; init; }
    public int RejectedCandidateCount { get; init; }
    public IReadOnlyList<CandidateValidationRecord> Records { get; init; } = [];
}

public sealed record CandidateValidationRecord
{
    public string CandidateId { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string FieldId { get; init; } = string.Empty;
    public bool Valid { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<CampaignEditDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ApplyRollbackLedger
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_apply_rollback_ledger_v1";
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int AppliedChangeCount { get; init; }
    public int RollbackCount { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public IReadOnlyList<RowApplyRollbackRecord> Rows { get; init; } = [];
}

public sealed record RowApplyRollbackRecord
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string BeforeHash { get; init; } = string.Empty;
    public string AfterHash { get; init; } = string.Empty;
    public string RollbackHash { get; init; } = string.Empty;
    public bool StateChanged { get; init; }
    public bool RollbackRestored { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public IReadOnlyList<RowAppliedChange> AppliedChanges { get; init; } = [];
}

public sealed record RowAppliedChange
{
    public string CandidateId { get; init; } = string.Empty;
    public string CandidateKind { get; init; } = string.Empty;
    public string FieldId { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
}

public sealed record RowBeforeAfterDiffMatrix
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_row_before_after_diff_matrix_v1";
    public bool Passed { get; init; }
    public int RowCount { get; init; }
    public int StateChangingRowCount { get; init; }
    public IReadOnlyList<RowBeforeAfterDiff> Rows { get; init; } = [];
}

public sealed record RowBeforeAfterDiff
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string BeforeHash { get; init; } = string.Empty;
    public string AfterHash { get; init; } = string.Empty;
    public IReadOnlyList<FieldDiff> ChangedFields { get; init; } = [];
}

public sealed record FieldDiff
{
    public string FieldId { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
}

public sealed record PreviewExportRefreshPayload
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_preview_export_refresh_payload_v1";
    public bool Passed { get; init; }
    public int ChangedRowCount { get; init; }
    public IReadOnlyList<string> ChangedRowIds { get; init; } = [];
    public IReadOnlyList<string> ChangedDomains { get; init; } = [];
    public IReadOnlyList<PreviewExportChangedRow> Rows { get; init; } = [];
}

public sealed record PreviewExportChangedRow
{
    public string RowId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string SeedId { get; init; } = string.Empty;
    public string RefreshKey { get; init; } = string.Empty;
    public string AfterHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ChangedFieldIds { get; init; } = [];
}

public sealed record WinFormsEditBindingInventory
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_edit_winforms_binding_inventory_v1";
    public bool Passed { get; init; }
    public bool NavigationRegistered { get; init; }
    public bool ParentPageEditLoopTabDeclared { get; init; }
    public bool ParentPageEditEvidenceServiceLoaded { get; init; }
    public bool ParentPageEditLoopBound { get; init; }
    public bool ParentPageActivationBindsGoal075Data { get; init; }
    public IReadOnlyList<WinFormsEditBindingGroup> Groups { get; init; } = [];
    public IReadOnlyList<CampaignEditDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record WinFormsEditBindingGroup
{
    public string GroupId { get; init; } = string.Empty;
    public string ControlName { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public bool SeparateUserControl { get; init; }
    public bool BindsGoal075Data { get; init; }
}

public sealed record CampaignEditQualityGateScan
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_edit_quality_gate_scan_v1";
    public bool Passed { get; init; }
    public int ScannedFileCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public int FilesOver1000LinesCount { get; init; }
    public int MinifiedSourceFileCount { get; init; }
    public bool CompositionRootScanned { get; init; }
    public bool Goal074WinFormsFilesScanned { get; init; }
    public bool ReportOnlyTestDetected { get; init; }
    public IReadOnlyList<CampaignEditQualityFileScan> Files { get; init; } = [];
    public IReadOnlyList<CampaignEditDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CampaignEditQualityFileScan
{
    public string RelativePath { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public int ByteCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public bool MinifiedSourceCandidate { get; init; }
}

public sealed record InvalidEditDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_invalid_edit_diagnostics_matrix_v1";
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<InvalidEditScenarioRecord> Scenarios { get; init; } = [];
}

public sealed record InvalidEditScenarioRecord
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CandidateId { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<CampaignEditDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SchemaDrivenCampaignEditReport
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_edit_report_v1";
    public string GoalId { get; init; } = SchemaDrivenCampaignEditVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = SchemaDrivenCampaignEditVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = SchemaDrivenCampaignEditVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal074AcceptedByUserHandoff { get; init; }
    public bool Goal072PreservedAsBlocked { get; init; }
    public bool Goal031And032RemainProducedForReview { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int EditableFieldCount { get; init; }
    public int CandidateCount { get; init; }
    public int AppliedChangeCount { get; init; }
    public int RollbackCount { get; init; }
    public int InvalidScenarioCount { get; init; }
    public bool ValidationPassed { get; init; }
    public bool ApplyRollbackPassed { get; init; }
    public bool DiffMatrixPassed { get; init; }
    public bool PreviewExportRefreshPassed { get; init; }
    public bool WinFormsBindingPassed { get; init; }
    public bool QualityGatePassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<CampaignEditDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SchemaDrivenCampaignEditBuildResult
{
    public CampaignEditSourceManifest SourceManifest { get; init; } = new();
    public EditableSchemaFieldCatalog FieldCatalog { get; init; } = new();
    public ChangeSetCatalog ChangeSetCatalog { get; init; } = new();
    public ValidationDiagnosticsMatrix ValidationMatrix { get; init; } = new();
    public ApplyRollbackLedger ApplyRollbackLedger { get; init; } = new();
    public RowBeforeAfterDiffMatrix DiffMatrix { get; init; } = new();
    public PreviewExportRefreshPayload PreviewExportRefreshPayload { get; init; } = new();
    public WinFormsEditBindingInventory WinFormsBindingInventory { get; init; } = new();
    public CampaignEditQualityGateScan QualityGateScan { get; init; } = new();
    public InvalidEditDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public SchemaDrivenCampaignEditReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record SchemaDrivenCampaignEditWriteResult
{
    public SchemaDrivenCampaignEditBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
