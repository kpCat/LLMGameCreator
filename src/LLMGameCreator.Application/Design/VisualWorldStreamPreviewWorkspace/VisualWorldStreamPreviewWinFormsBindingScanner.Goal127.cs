namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal127WinFormsPageText(string projectRoot)
    {
        const string pageGoal127RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal127.cs";

        return ReadOptionalText(projectRoot, pageGoal127RelativePath);
    }

    private static string ReadGoal120Through127WinFormsPageText(string projectRoot) =>
        ReadGoal120Through126WinFormsPageText(projectRoot)
        + "\n"
        + ReadGoal127WinFormsPageText(projectRoot);

    private static bool PageBindsGoal127UnityProjectionVerificationRunner(
        string pageText) =>
        pageText.Contains("runnerStatus", StringComparison.Ordinal)
        && pageText.Contains("runnerScriptPath", StringComparison.Ordinal)
        && pageText.Contains("runnerCmdPath", StringComparison.Ordinal)
        && pageText.Contains("runnerCommand", StringComparison.Ordinal)
        && pageText.Contains("mode", StringComparison.Ordinal)
        && pageText.Contains("unityExecuteMethod", StringComparison.Ordinal)
        && pageText.Contains("resultPath", StringComparison.Ordinal)
        && pageText.Contains("logPath", StringComparison.Ordinal)
        && pageText.Contains("passMarkerPresent", StringComparison.Ordinal)
        && pageText.Contains("cleanupApplied", StringComparison.Ordinal)
        && pageText.Contains("cleanupScriptAvailable", StringComparison.Ordinal)
        && pageText.Contains("cleanupCommand", StringComparison.Ordinal)
        && pageText.Contains("manualUnityClickingRequired", StringComparison.Ordinal)
        && pageText.Contains("evidencePath", StringComparison.Ordinal)
        && pageText.Contains("exportPath", StringComparison.Ordinal);
}
