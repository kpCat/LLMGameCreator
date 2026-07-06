namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string CanonicalRuntimeCandidateId { get; init; } = string.Empty;
    public bool CanonicalRuntimePackageValidationPassed { get; init; }
    public bool CanonicalRuntimePassed { get; init; }
    public int CanonicalRuntimeCommandCount { get; init; }
    public int CanonicalRuntimeEventCount { get; init; }
    public bool CanonicalRuntimeSaveLoadReplayPassed { get; init; }
    public bool CanonicalRuntimeUnityPlayerConsumedTranscript { get; init; }
    public bool CanonicalRuntimeProjectionOnly { get; init; }
    public bool CanonicalRuntimeSelectedCandidateExecutedByRuntime { get; init; }
    public string CanonicalRuntimeNormalCommand { get; init; } = string.Empty;
    public string CanonicalRuntimeReportPath { get; init; } = string.Empty;
    public string CanonicalRuntimeMatrixResultPath { get; init; } = string.Empty;
    public bool CanonicalRuntimeManualUnityOptional { get; init; }
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysCanonicalRuntimeSelectedCandidate { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool CanonicalRuntimeSelectedCandidateGroupPresent { get; init; }
    public string CanonicalRuntimeCandidateId { get; init; } = string.Empty;
    public bool CanonicalRuntimePackageValidationPassed { get; init; }
    public bool CanonicalRuntimePassed { get; init; }
    public int CanonicalRuntimeCommandCount { get; init; }
    public int CanonicalRuntimeEventCount { get; init; }
    public bool CanonicalRuntimeSaveLoadReplayPassed { get; init; }
    public bool CanonicalRuntimeUnityPlayerConsumedTranscript { get; init; }
    public bool CanonicalRuntimeProjectionOnly { get; init; }
    public bool CanonicalRuntimeSelectedCandidateExecutedByRuntime { get; init; }
    public string CanonicalRuntimeNormalCommand { get; init; } = string.Empty;
    public string CanonicalRuntimeReportPath { get; init; } = string.Empty;
    public string CanonicalRuntimeMatrixResultPath { get; init; } = string.Empty;
    public bool CanonicalRuntimeManualUnityOptional { get; init; }
    public bool CanonicalRuntimeGoal134FilesDiscoveredByRelativePaths { get; init; }
    public bool CanonicalRuntimeWinFormsBindingReal { get; init; }
    public bool CanonicalRuntimeQualityGatePassed { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string CanonicalRuntimeCandidateId { get; init; } = string.Empty;
    public bool CanonicalRuntimePackageValidationPassed { get; init; }
    public bool CanonicalRuntimePassed { get; init; }
    public int CanonicalRuntimeCommandCount { get; init; }
    public int CanonicalRuntimeEventCount { get; init; }
    public bool CanonicalRuntimeSaveLoadReplayPassed { get; init; }
    public bool CanonicalRuntimeUnityPlayerConsumedTranscript { get; init; }
    public bool CanonicalRuntimeProjectionOnly { get; init; }
    public bool CanonicalRuntimeSelectedCandidateExecutedByRuntime { get; init; }
    public string CanonicalRuntimeNormalCommand { get; init; } = string.Empty;
    public string CanonicalRuntimeReportPath { get; init; } = string.Empty;
    public string CanonicalRuntimeMatrixResultPath { get; init; } = string.Empty;
    public bool CanonicalRuntimeManualUnityOptional { get; init; }
    public bool CanonicalRuntimeQualityGatePassed { get; init; }
    public bool CanonicalRuntimeGoal134FilesDiscoveredByRelativePaths { get; init; }
}
