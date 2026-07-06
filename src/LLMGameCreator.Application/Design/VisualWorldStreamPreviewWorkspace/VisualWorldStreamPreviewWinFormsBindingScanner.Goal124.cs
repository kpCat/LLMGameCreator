namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal124WinFormsPageText(string projectRoot)
    {
        const string pageGoal124RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal124.cs";

        return ReadOptionalText(projectRoot, pageGoal124RelativePath);
    }

    private static string ReadGoal120Through124WinFormsPageText(string projectRoot) =>
        ReadGoal120Through123WinFormsPageText(projectRoot)
        + "\n"
        + ReadGoal124WinFormsPageText(projectRoot);

    private static bool PageBindsGoal124GenericGamePackageLoop(string pageText) =>
        pageText.Contains("genericLoopStatus", StringComparison.Ordinal)
        && pageText.Contains("samplePackagePath", StringComparison.Ordinal)
        && pageText.Contains("packageId", StringComparison.Ordinal)
        && pageText.Contains("mapId", StringComparison.Ordinal)
        && pageText.Contains("interactionPreviewPresent", StringComparison.Ordinal)
        && pageText.Contains("interactionApplyPassed", StringComparison.Ordinal)
        && pageText.Contains("dialogueSummaryPresent", StringComparison.Ordinal)
        && pageText.Contains("questObjectiveSummaryPresent", StringComparison.Ordinal)
        && pageText.Contains("inventorySummaryPresent", StringComparison.Ordinal)
        && pageText.Contains("resourceSummaryPresent", StringComparison.Ordinal)
        && pageText.Contains("unitySmokeStatus", StringComparison.Ordinal)
        && pageText.Contains("cleanupCommand", StringComparison.Ordinal)
        && pageText.Contains("goal123StillGreen", StringComparison.Ordinal)
        && pageText.Contains("evidencePath", StringComparison.Ordinal)
        && pageText.Contains("exportPath", StringComparison.Ordinal);
}
