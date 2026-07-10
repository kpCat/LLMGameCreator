namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string ProductLineRuntimeVariantMatrixStatus { get; init; } = string.Empty;
    public int ProductLineRuntimeVariantCandidateCount { get; init; }
    public int ProductLineRuntimeVariantPassedCandidateCount { get; init; }
    public int ProductLineRuntimeVariantFailedCandidateCount { get; init; }
    public int ProductLineRuntimeVariantRuntimeSignificantCandidateCount { get; init; }
    public int ProductLineRuntimeVariantDistinctFinalStateHashCount { get; init; }
    public string ProductLineRuntimeVariantSelectedCandidateId { get; init; } = string.Empty;
    public string ProductLineRuntimeVariantSelectedVariantKind { get; init; } = string.Empty;
    public int ProductLineRuntimeVariantSelectedScore { get; init; }
    public bool ProductLineRuntimeVariantSourceTemplateUnmodified { get; init; }
    public string ProductLineRuntimeVariantNormalCommand { get; init; } = string.Empty;
    public string ProductLineRuntimeVariantMatrixResultPath { get; init; } = string.Empty;
    public string ProductLineRuntimeVariantSelectedHandoffPath { get; init; } = string.Empty;
    public bool ProductLineRuntimeVariantAccepted { get; init; }
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysProductLineRuntimeVariantMatrix { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool ProductLineRuntimeVariantMatrixGroupPresent { get; init; }
    public string ProductLineRuntimeVariantMatrixStatus { get; init; } = string.Empty;
    public int ProductLineRuntimeVariantCandidateCount { get; init; }
    public int ProductLineRuntimeVariantPassedCandidateCount { get; init; }
    public int ProductLineRuntimeVariantFailedCandidateCount { get; init; }
    public int ProductLineRuntimeVariantRuntimeSignificantCandidateCount { get; init; }
    public int ProductLineRuntimeVariantDistinctFinalStateHashCount { get; init; }
    public string ProductLineRuntimeVariantSelectedCandidateId { get; init; } = string.Empty;
    public string ProductLineRuntimeVariantSelectedVariantKind { get; init; } = string.Empty;
    public int ProductLineRuntimeVariantSelectedScore { get; init; }
    public bool ProductLineRuntimeVariantSourceTemplateUnmodified { get; init; }
    public string ProductLineRuntimeVariantNormalCommand { get; init; } = string.Empty;
    public string ProductLineRuntimeVariantMatrixResultPath { get; init; } = string.Empty;
    public string ProductLineRuntimeVariantSelectedHandoffPath { get; init; } = string.Empty;
    public bool ProductLineRuntimeVariantAccepted { get; init; }
    public bool ProductLineRuntimeVariantFilesDiscoveredByRelativePaths { get; init; }
    public bool ProductLineRuntimeVariantWinFormsBindingReal { get; init; }
    public bool ProductLineRuntimeVariantQualityGatePassed { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string ProductLineRuntimeVariantMatrixStatus { get; init; } = string.Empty;
    public int ProductLineRuntimeVariantCandidateCount { get; init; }
    public int ProductLineRuntimeVariantPassedCandidateCount { get; init; }
    public int ProductLineRuntimeVariantFailedCandidateCount { get; init; }
    public int ProductLineRuntimeVariantRuntimeSignificantCandidateCount { get; init; }
    public int ProductLineRuntimeVariantDistinctFinalStateHashCount { get; init; }
    public string ProductLineRuntimeVariantSelectedCandidateId { get; init; } = string.Empty;
    public string ProductLineRuntimeVariantSelectedVariantKind { get; init; } = string.Empty;
    public int ProductLineRuntimeVariantSelectedScore { get; init; }
    public bool ProductLineRuntimeVariantSourceTemplateUnmodified { get; init; }
    public string ProductLineRuntimeVariantNormalCommand { get; init; } = string.Empty;
    public string ProductLineRuntimeVariantMatrixResultPath { get; init; } = string.Empty;
    public string ProductLineRuntimeVariantSelectedHandoffPath { get; init; } = string.Empty;
    public bool ProductLineRuntimeVariantAccepted { get; init; }
    public bool ProductLineRuntimeVariantQualityGatePassed { get; init; }
    public bool ProductLineRuntimeVariantFilesDiscoveredByRelativePaths { get; init; }
}
