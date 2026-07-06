namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string CandidatePipelineOperatorStatus { get; init; } = string.Empty;
    public string CandidatePipelineOperatorNormalCommand { get; init; } = string.Empty;
    public string CandidatePipelineOperatorDryRunCommand { get; init; } = string.Empty;
    public string CandidatePipelineOperatorResultPath { get; init; } = string.Empty;
    public string CandidatePipelineOperatorSelectedCandidateId { get; init; } = string.Empty;
    public int CandidatePipelineOperatorSelectedCandidateScore { get; init; }
    public int CandidatePipelineOperatorCandidateCount { get; init; }
    public int CandidatePipelineOperatorPassedCandidates { get; init; }
    public int CandidatePipelineOperatorFailedCandidates { get; init; }
    public bool CandidatePipelineOperatorMatrixPassed { get; init; }
    public int CandidatePipelineOperatorLastExitCode { get; init; } = -1;
    public long CandidatePipelineOperatorLastDurationMilliseconds { get; init; }
    public string CandidatePipelineOperatorOutputTail { get; init; } = string.Empty;
    public bool CandidatePipelineOperatorManualUnityOptional { get; init; }
    public bool CandidatePipelineOperatorProjectionOnly { get; init; }
    public bool CandidatePipelineOperatorSamplePackageReadOnly { get; init; }
    public bool CandidatePipelineOperatorWinFormsPanelPresent { get; init; }
    public bool CandidatePipelineOperatorRefreshButtonPresent { get; init; }
    public bool CandidatePipelineOperatorCopyCommandButtonPresent { get; init; }
    public bool CandidatePipelineOperatorDryRunButtonPresent { get; init; }
    public bool CandidatePipelineOperatorRunButtonPresent { get; init; }
    public bool CandidatePipelineOperatorAsyncRunPresent { get; init; }
    public bool CandidatePipelineOperatorResultPresent { get; init; }
    public string CandidatePipelineOperatorEvidencePath { get; init; } = string.Empty;
    public string CandidatePipelineOperatorExportPath { get; init; } = string.Empty;
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysCandidatePipelineOperator { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool CandidatePipelineOperatorGroupPresent { get; init; }
    public string CandidatePipelineOperatorStatus { get; init; } = string.Empty;
    public string CandidatePipelineOperatorNormalCommand { get; init; } = string.Empty;
    public string CandidatePipelineOperatorDryRunCommand { get; init; } = string.Empty;
    public string CandidatePipelineOperatorResultPath { get; init; } = string.Empty;
    public string CandidatePipelineOperatorSelectedCandidateId { get; init; } = string.Empty;
    public int CandidatePipelineOperatorSelectedCandidateScore { get; init; }
    public int CandidatePipelineOperatorCandidateCount { get; init; }
    public int CandidatePipelineOperatorPassedCandidates { get; init; }
    public int CandidatePipelineOperatorFailedCandidates { get; init; }
    public bool CandidatePipelineOperatorMatrixPassed { get; init; }
    public int CandidatePipelineOperatorLastExitCode { get; init; } = -1;
    public long CandidatePipelineOperatorLastDurationMilliseconds { get; init; }
    public string CandidatePipelineOperatorOutputTail { get; init; } = string.Empty;
    public bool CandidatePipelineOperatorManualUnityOptional { get; init; }
    public bool CandidatePipelineOperatorProjectionOnly { get; init; }
    public bool CandidatePipelineOperatorSamplePackageReadOnly { get; init; }
    public bool CandidatePipelineOperatorWinFormsPanelPresent { get; init; }
    public bool CandidatePipelineOperatorRefreshButtonPresent { get; init; }
    public bool CandidatePipelineOperatorCopyCommandButtonPresent { get; init; }
    public bool CandidatePipelineOperatorDryRunButtonPresent { get; init; }
    public bool CandidatePipelineOperatorRunButtonPresent { get; init; }
    public bool CandidatePipelineOperatorAsyncRunPresent { get; init; }
    public bool CandidatePipelineOperatorResultPresent { get; init; }
    public string CandidatePipelineOperatorEvidencePath { get; init; } = string.Empty;
    public string CandidatePipelineOperatorExportPath { get; init; } = string.Empty;
    public bool CandidatePipelineOperatorQualityGatePassed { get; init; }
    public bool Goal132FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsCandidatePipelineOperatorBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string CandidatePipelineOperatorStatus { get; init; } = string.Empty;
    public string CandidatePipelineOperatorNormalCommand { get; init; } = string.Empty;
    public string CandidatePipelineOperatorDryRunCommand { get; init; } = string.Empty;
    public string CandidatePipelineOperatorResultPath { get; init; } = string.Empty;
    public string CandidatePipelineOperatorSelectedCandidateId { get; init; } = string.Empty;
    public int CandidatePipelineOperatorSelectedCandidateScore { get; init; }
    public int CandidatePipelineOperatorCandidateCount { get; init; }
    public int CandidatePipelineOperatorPassedCandidates { get; init; }
    public int CandidatePipelineOperatorFailedCandidates { get; init; }
    public bool CandidatePipelineOperatorMatrixPassed { get; init; }
    public int CandidatePipelineOperatorLastExitCode { get; init; } = -1;
    public long CandidatePipelineOperatorLastDurationMilliseconds { get; init; }
    public string CandidatePipelineOperatorOutputTail { get; init; } = string.Empty;
    public bool CandidatePipelineOperatorManualUnityOptional { get; init; }
    public bool CandidatePipelineOperatorProjectionOnly { get; init; }
    public bool CandidatePipelineOperatorSamplePackageReadOnly { get; init; }
    public bool CandidatePipelineOperatorWinFormsPanelPresent { get; init; }
    public bool CandidatePipelineOperatorRefreshButtonPresent { get; init; }
    public bool CandidatePipelineOperatorCopyCommandButtonPresent { get; init; }
    public bool CandidatePipelineOperatorDryRunButtonPresent { get; init; }
    public bool CandidatePipelineOperatorRunButtonPresent { get; init; }
    public bool CandidatePipelineOperatorAsyncRunPresent { get; init; }
    public bool CandidatePipelineOperatorResultPresent { get; init; }
    public string CandidatePipelineOperatorEvidencePath { get; init; } = string.Empty;
    public string CandidatePipelineOperatorExportPath { get; init; } = string.Empty;
    public bool CandidatePipelineOperatorQualityGatePassed { get; init; }
    public bool Goal132FilesDiscoveredByRelativePaths { get; init; }
}
