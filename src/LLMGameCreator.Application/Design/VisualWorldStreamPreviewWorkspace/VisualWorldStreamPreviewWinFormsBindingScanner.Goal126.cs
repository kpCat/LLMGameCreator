namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal126WinFormsPageText(string projectRoot)
    {
        const string pageGoal126RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal126.cs";

        return ReadOptionalText(projectRoot, pageGoal126RelativePath);
    }

    private static string ReadGoal120Through126WinFormsPageText(string projectRoot) =>
        ReadGoal120Through125WinFormsPageText(projectRoot)
        + "\n"
        + ReadGoal126WinFormsPageText(projectRoot);

    private static bool PageBindsGoal126GenericGamePackageFullPlaythrough(
        string pageText) =>
        pageText.Contains("fullPlaythroughStatus", StringComparison.Ordinal)
        && pageText.Contains("samplePackagePath", StringComparison.Ordinal)
        && pageText.Contains("packageId", StringComparison.Ordinal)
        && pageText.Contains("mapId", StringComparison.Ordinal)
        && pageText.Contains("mapPathPreviewPresent", StringComparison.Ordinal)
        && pageText.Contains("signInteractionApplied", StringComparison.Ordinal)
        && pageText.Contains("dialogueSummaryPresent", StringComparison.Ordinal)
        && pageText.Contains("questObjectiveStatusPresent", StringComparison.Ordinal)
        && pageText.Contains("inventorySummaryPresent", StringComparison.Ordinal)
        && pageText.Contains("resourceSummaryPresent", StringComparison.Ordinal)
        && pageText.Contains("systemsSummaryPresent", StringComparison.Ordinal)
        && pageText.Contains("combatRoundPreviewPresent", StringComparison.Ordinal)
        && pageText.Contains("eventTranscriptPresent", StringComparison.Ordinal)
        && pageText.Contains("unitySmokeStatus", StringComparison.Ordinal)
        && pageText.Contains("cleanupScriptAvailable", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal)
        && pageText.Contains("evidencePath", StringComparison.Ordinal)
        && pageText.Contains("exportPath", StringComparison.Ordinal);
}
