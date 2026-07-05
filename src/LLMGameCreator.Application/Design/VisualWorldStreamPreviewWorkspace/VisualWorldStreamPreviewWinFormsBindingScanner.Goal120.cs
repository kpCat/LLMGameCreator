namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal120WinFormsPageText(string projectRoot)
    {
        const string pageGoal120RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal120.cs";

        return ReadOptionalText(projectRoot, pageGoal120RelativePath);
    }

    private static bool PageBindsGoal120AcceptedAlphaProjectionUsability(string pageText) =>
        pageText.Contains("acceptedAlphaProjectionUsabilityStatus", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionUsabilityUnityMenuPath", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionUsabilityCleanupScriptPath", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionUsabilityCleanupScriptCmdPath", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionUsabilityLegendPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionUsabilityMarkerDescriptorPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionUsabilitySelectionControlsPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionUsabilityFocusCameraControlPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionUsabilityMaterialWarningGuardPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionUsabilityUnitySmokeStatus", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionUsabilityEvidencePath", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionUsabilityExportPath", StringComparison.Ordinal);
}
