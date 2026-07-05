namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string OfflineGeoworldAlphaManualResultWorkbenchStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchGoal112OperatorStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaManualResultWorkbenchManualResultPresent { get; init; }
    public string OfflineGeoworldAlphaManualResultWorkbenchPreferredManualResultPath { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchDraftTemplatePath { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchCandidateManualResultPaths { get; init; } = string.Empty;
    public int OfflineGeoworldAlphaManualResultWorkbenchChecklistStepCount { get; init; }
    public string OfflineGeoworldAlphaManualResultWorkbenchChecklistHash { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchRequiredStepsSummary { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchValidationErrors { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchValidationWarnings { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchNextHumanActions { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchDoNotStartYet { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchProceduralPath { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchExportPath { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchRunbookPath { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaManualResultWorkbenchAcceptedByCodex { get; init; }
    public bool OfflineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired { get; init; }
    public bool OfflineGeoworldAlphaManualResultWorkbenchDraftTemplateOnly { get; init; }
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysOfflineGeoworldAlphaManualResultWorkbench { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool OfflineGeoworldAlphaManualResultWorkbenchGroupPresent { get; init; }
    public string OfflineGeoworldAlphaManualResultWorkbenchStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchGoal112OperatorStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaManualResultWorkbenchManualResultPresent { get; init; }
    public bool OfflineGeoworldAlphaManualResultWorkbenchAcceptedByCodex { get; init; }
    public bool OfflineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired { get; init; }
    public bool OfflineGeoworldAlphaManualResultWorkbenchDraftTemplateOnly { get; init; }
    public int OfflineGeoworldAlphaManualResultWorkbenchChecklistStepCount { get; init; }
    public bool OfflineGeoworldAlphaManualResultWorkbenchChecklistHashPresent { get; init; }
    public bool OfflineGeoworldAlphaManualResultWorkbenchQualityGatePassed { get; init; }
    public bool Goal113FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsOfflineGeoworldAlphaManualResultWorkbenchBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string OfflineGeoworldAlphaManualResultWorkbenchStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultWorkbenchGoal112OperatorStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaManualResultWorkbenchManualResultPresent { get; init; }
    public bool OfflineGeoworldAlphaManualResultWorkbenchAcceptedByCodex { get; init; }
    public bool OfflineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired { get; init; }
    public bool OfflineGeoworldAlphaManualResultWorkbenchDraftTemplateOnly { get; init; }
    public int OfflineGeoworldAlphaManualResultWorkbenchChecklistStepCount { get; init; }
    public bool OfflineGeoworldAlphaManualResultWorkbenchChecklistHashPresent { get; init; }
    public bool OfflineGeoworldAlphaManualResultWorkbenchQualityGatePassed { get; init; }
    public bool Goal113FilesDiscoveredByRelativePaths { get; init; }
}
