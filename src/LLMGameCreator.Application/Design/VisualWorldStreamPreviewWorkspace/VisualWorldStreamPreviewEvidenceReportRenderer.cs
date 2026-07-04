namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string RenderReport(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldStreamPreviewCatalog catalog,
        VisualWorldStreamPreviewProofStatusDocument proofStatus,
        VisualWorldPreviewWinFormsBindingInventory binding,
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        string deterministicReportHash) =>
        RenderWorkspaceReport(
            report,
            catalog,
            proofStatus,
            binding,
            qualityGate,
            deterministicReportHash);
}
