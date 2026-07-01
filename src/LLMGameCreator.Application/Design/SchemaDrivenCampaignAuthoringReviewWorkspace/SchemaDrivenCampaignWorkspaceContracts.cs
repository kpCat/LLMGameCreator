namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

public sealed record CampaignAuthoringSchema
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_authoring_schema_v1";
    public bool Passed { get; init; }
    public IReadOnlyList<CampaignSchemaGroup> Groups { get; init; } = [];
}

public sealed record CampaignSchemaGroup
{
    public string GroupId { get; init; } = string.Empty;
    public string TitleKey { get; init; } = string.Empty;
    public int Order { get; init; }
    public string SourceGoalRange { get; init; } = string.Empty;
    public IReadOnlyList<CampaignSchemaField> Fields { get; init; } = [];
    public IReadOnlyList<string> SourceArtifactRefs { get; init; } = [];
    public IReadOnlyList<string> DiagnosticCodes { get; init; } = [];
}

public sealed record CampaignSchemaField
{
    public string FieldId { get; init; } = string.Empty;
    public string LabelKey { get; init; } = string.Empty;
    public string ValueKind { get; init; } = "text";
    public string SourcePath { get; init; } = string.Empty;
    public bool Bindable { get; init; } = true;
}

public sealed record CampaignUiBindingContract
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_ui_binding_contract_v1";
    public bool Passed { get; init; }
    public CampaignRowSelectorBinding RowSelector { get; init; } = new();
    public IReadOnlyList<CampaignUiGroupBinding> GroupBindings { get; init; } = [];
    public IReadOnlyList<CampaignWorkspaceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CampaignRowSelectorBinding
{
    public string ControlKey { get; init; } = "campaign_rows_selector";
    public string DataPath { get; init; } = "rowSelector.rows";
    public string SelectedRowPath { get; init; } = "selectedRowId";
    public IReadOnlyList<string> RequiredColumns { get; init; } = [];
}

public sealed record CampaignUiGroupBinding
{
    public string GroupId { get; init; } = string.Empty;
    public string ControlKey { get; init; } = string.Empty;
    public string DataPath { get; init; } = string.Empty;
    public IReadOnlyList<CampaignUiFieldBinding> FieldBindings { get; init; } = [];
}

public sealed record CampaignUiFieldBinding
{
    public string FieldId { get; init; } = string.Empty;
    public string ControlKind { get; init; } = "readonly_text";
    public string DataPath { get; init; } = string.Empty;
    public bool ReadOnly { get; init; } = true;
}

public sealed record WorkspaceValidationDashboard
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_workspace_validation_dashboard_v1";
    public bool Passed { get; init; }
    public bool SourceManifestPassed { get; init; }
    public bool RowSelectorPassed { get; init; }
    public bool SchemaPassed { get; init; }
    public bool UiBindingPassed { get; init; }
    public bool ProvenancePassed { get; init; }
    public bool ActionPlanPassed { get; init; }
    public bool QualityGatePassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
    public IReadOnlyList<CampaignWorkspaceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ReviewProvenanceLedger
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_review_provenance_ledger_v1";
    public bool Passed { get; init; }
    public IReadOnlyList<string> Categories { get; init; } = [];
    public IReadOnlyList<ReviewProvenanceEntry> Entries { get; init; } = [];
}

public sealed record ReviewProvenanceEntry
{
    public string EntryId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string SourceGoal { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
    public string ReviewState { get; init; } = string.Empty;
    public bool AcceptedByReview { get; init; }
    public bool HasReviewProvenance { get; init; }
}

public sealed record AuthoringActionPlan
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_authoring_action_plan_v1";
    public bool Passed { get; init; }
    public string PlanHash { get; init; } = string.Empty;
    public IReadOnlyList<AuthoringActionPlanItem> Items { get; init; } = [];
}

public sealed record AuthoringActionPlanItem
{
    public string ActionId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string Category { get; init; } = string.Empty;
    public string SchemaGroupId { get; init; } = string.Empty;
    public string TargetRef { get; init; } = string.Empty;
    public string ReviewPolicy { get; init; } = string.Empty;
    public bool Deterministic { get; init; } = true;
}

public sealed record QualityGateScan
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_quality_gate_scan_v1";
    public bool Passed { get; init; }
    public int ScannedFileCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public int FilesOver900LinesCount { get; init; }
    public int MinifiedSourceFileCount { get; init; }
    public int FilesWithTooFewLinesForSizeCount { get; init; }
    public bool CompositionRootScanned { get; init; }
    public bool NewAlphaRuntimeBootstrapRoute { get; init; }
    public IReadOnlyList<QualityGateFileScan> Files { get; init; } = [];
    public IReadOnlyList<CampaignWorkspaceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record QualityGateFileScan
{
    public string RelativePath { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public int ByteCount { get; init; }
    public int MinimumExpectedLineCount { get; init; }
    public int MaxLineLength { get; init; }
    public int LinesOver500Count { get; init; }
    public bool TooFewLinesForSize { get; init; }
    public bool MinifiedSourceCandidate { get; init; }
}

public sealed record WinFormsControlInventory
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_winforms_control_inventory_v1";
    public bool Passed { get; init; }
    public bool NavigationRegistered { get; init; }
    public IReadOnlyList<WinFormsControlInventoryItem> Controls { get; init; } = [];
}

public sealed record WinFormsControlInventoryItem
{
    public string ControlName { get; init; } = string.Empty;
    public string ControlRole { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public bool SeparateUserControl { get; init; }
    public bool SchemaDrivenBinding { get; init; }
}
