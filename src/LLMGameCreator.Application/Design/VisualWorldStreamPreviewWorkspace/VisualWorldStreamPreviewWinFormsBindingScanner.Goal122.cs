namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal122WinFormsPageText(string projectRoot)
    {
        const string pageGoal122RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal122.cs";

        return ReadOptionalText(projectRoot, pageGoal122RelativePath);
    }

    private static bool PageBindsGoal122AcceptedAlphaProjectionActionLoop(string pageText) =>
        pageText.Contains("acceptedAlphaProjectionActionLoopStatus", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionActionLoopWindowPolishStatus", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionActionLoopUnityMenuPath", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionActionLoopOneClickVerificationStillPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionActionLoopProjectionActionPreviewPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionActionLoopProjectionActionApplyPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionActionLoopProjectionStateResetPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionActionLoopWindowLayoutPolishPresent", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionActionLoopUnitySmokeStatus", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionActionLoopCleanupScriptAvailable", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionActionLoopDoNotStartAutomatically", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionActionLoopEvidencePath", StringComparison.Ordinal)
        && pageText.Contains("acceptedAlphaProjectionActionLoopExportPath", StringComparison.Ordinal);
}
