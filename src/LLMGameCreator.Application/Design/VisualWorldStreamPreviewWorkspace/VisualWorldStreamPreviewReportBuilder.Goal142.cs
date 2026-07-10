namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal142ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            ProductLineRuntimeVariantMatrixStatus = qualityGate.ProductLineRuntimeVariantMatrixStatus,
            ProductLineRuntimeVariantCandidateCount = qualityGate.ProductLineRuntimeVariantCandidateCount,
            ProductLineRuntimeVariantPassedCandidateCount =
                qualityGate.ProductLineRuntimeVariantPassedCandidateCount,
            ProductLineRuntimeVariantFailedCandidateCount =
                qualityGate.ProductLineRuntimeVariantFailedCandidateCount,
            ProductLineRuntimeVariantRuntimeSignificantCandidateCount =
                qualityGate.ProductLineRuntimeVariantRuntimeSignificantCandidateCount,
            ProductLineRuntimeVariantDistinctFinalStateHashCount =
                qualityGate.ProductLineRuntimeVariantDistinctFinalStateHashCount,
            ProductLineRuntimeVariantSelectedCandidateId =
                qualityGate.ProductLineRuntimeVariantSelectedCandidateId,
            ProductLineRuntimeVariantSelectedVariantKind =
                qualityGate.ProductLineRuntimeVariantSelectedVariantKind,
            ProductLineRuntimeVariantSelectedScore = qualityGate.ProductLineRuntimeVariantSelectedScore,
            ProductLineRuntimeVariantSourceTemplateUnmodified =
                qualityGate.ProductLineRuntimeVariantSourceTemplateUnmodified,
            ProductLineRuntimeVariantNormalCommand = qualityGate.ProductLineRuntimeVariantNormalCommand,
            ProductLineRuntimeVariantMatrixResultPath =
                qualityGate.ProductLineRuntimeVariantMatrixResultPath,
            ProductLineRuntimeVariantSelectedHandoffPath =
                qualityGate.ProductLineRuntimeVariantSelectedHandoffPath,
            ProductLineRuntimeVariantAccepted = qualityGate.ProductLineRuntimeVariantAccepted,
            ProductLineRuntimeVariantQualityGatePassed =
                qualityGate.ProductLineRuntimeVariantQualityGatePassed,
            ProductLineRuntimeVariantFilesDiscoveredByRelativePaths =
                qualityGate.ProductLineRuntimeVariantFilesDiscoveredByRelativePaths
        };
}
