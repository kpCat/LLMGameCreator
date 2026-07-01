namespace LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

public sealed record CampaignInvalidDiagnosticsMatrix
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_invalid_diagnostics_matrix_v1";
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<CampaignInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record CampaignInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<CampaignWorkspaceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CampaignWorkspaceReport
{
    public string SchemaVersion { get; init; } = "schema_driven_campaign_workspace_report_v1";
    public string GoalId { get; init; } = SchemaDrivenCampaignWorkspaceVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = SchemaDrivenCampaignWorkspaceVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = SchemaDrivenCampaignWorkspaceVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal073AcceptedByUserHandoff { get; init; }
    public bool Goal072PreservedAsBlocked { get; init; }
    public bool Goal031And032RemainProducedForReview { get; init; }
    public bool SourceManifestPassed { get; init; }
    public bool RowSelectorPassed { get; init; }
    public bool DynamicSchemaPassed { get; init; }
    public bool UiBindingContractPassed { get; init; }
    public bool ProvenanceLedgerPassed { get; init; }
    public bool ActionPlanPassed { get; init; }
    public bool ValidationDashboardPassed { get; init; }
    public bool QualityGatePassed { get; init; }
    public bool WinFormsControlInventoryPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public int RowCount { get; init; }
    public int FamilyCount { get; init; }
    public int SeedCount { get; init; }
    public int SchemaGroupCount { get; init; }
    public int UiBindingGroupCount { get; init; }
    public int ProvenanceEntryCount { get; init; }
    public int ActionPlanItemCount { get; init; }
    public string SourceManifestHash { get; init; } = string.Empty;
    public string RowSelectorHash { get; init; } = string.Empty;
    public string DynamicSchemaHash { get; init; } = string.Empty;
    public string UiBindingContractHash { get; init; } = string.Empty;
    public string ValidationDashboardHash { get; init; } = string.Empty;
    public string ProvenanceLedgerHash { get; init; } = string.Empty;
    public string ActionPlanHash { get; init; } = string.Empty;
    public string QualityGateScanHash { get; init; } = string.Empty;
    public string WinFormsControlInventoryHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<CampaignWorkspaceDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CampaignWorkspaceBuildResult
{
    public CampaignWorkspaceSourceManifest SourceManifest { get; init; } = new();
    public CampaignRowSelector RowSelector { get; init; } = new();
    public CampaignAuthoringSchema DynamicSchema { get; init; } = new();
    public CampaignUiBindingContract UiBindingContract { get; init; } = new();
    public WorkspaceValidationDashboard ValidationDashboard { get; init; } = new();
    public ReviewProvenanceLedger ProvenanceLedger { get; init; } = new();
    public AuthoringActionPlan ActionPlan { get; init; } = new();
    public QualityGateScan QualityGateScan { get; init; } = new();
    public WinFormsControlInventory WinFormsControlInventory { get; init; } = new();
    public CampaignInvalidDiagnosticsMatrix InvalidMatrix { get; init; } = new();
    public CampaignWorkspaceReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record CampaignWorkspaceWriteResult
{
    public CampaignWorkspaceBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
