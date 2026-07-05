namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public bool OfflineGeoworldAlphaManualResultIntakeGoal110PackagePresent { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeResultFilePresent { get; init; }
    public string OfflineGeoworldAlphaManualResultIntakeDecisionStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaManualResultIntakeAcceptableCandidate { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeAcceptedByCodex { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeChecklistHashMatched { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakePassedStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakeFailedStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakePendingStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakeSkippedStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakeMissingStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakeDuplicateStepCount { get; init; }
    public string OfflineGeoworldAlphaManualResultIntakeTopErrors { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultIntakeTopWarnings { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultIntakeDecisionPath { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaManualResultIntakeExportPath { get; init; } = string.Empty;
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysOfflineGeoworldAlphaManualResultIntake { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool OfflineGeoworldAlphaManualResultIntakeGroupPresent { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeGoal110PackagePresent { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeResultFilePresent { get; init; }
    public string OfflineGeoworldAlphaManualResultIntakeDecisionStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaManualResultIntakeAcceptableCandidate { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeAcceptedByCodex { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeChecklistHashMatched { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakePassedStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakeFailedStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakePendingStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakeSkippedStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakeMissingStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakeDuplicateStepCount { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeQualityGatePassed { get; init; }
    public bool Goal111FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsOfflineGeoworldAlphaManualResultIntakeBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public bool OfflineGeoworldAlphaManualResultIntakeGoal110PackagePresent { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeResultFilePresent { get; init; }
    public string OfflineGeoworldAlphaManualResultIntakeDecisionStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaManualResultIntakeAcceptableCandidate { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeAcceptedByCodex { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeChecklistHashMatched { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakePassedStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakeFailedStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakePendingStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakeSkippedStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakeMissingStepCount { get; init; }
    public int OfflineGeoworldAlphaManualResultIntakeDuplicateStepCount { get; init; }
    public bool OfflineGeoworldAlphaManualResultIntakeQualityGatePassed { get; init; }
    public bool Goal111FilesDiscoveredByRelativePaths { get; init; }
}
