namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal128WinFormsPageText(string projectRoot)
    {
        const string pageGoal128RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal128.cs";

        return ReadOptionalText(projectRoot, pageGoal128RelativePath);
    }

    private static string ReadGoal120Through128WinFormsPageText(string projectRoot) =>
        ReadGoal120Through127WinFormsPageText(projectRoot)
        + "\n"
        + ReadGoal128WinFormsPageText(projectRoot);

    private static bool PageBindsGoal128ParameterizedGamePackageRunner(
        string pageText) =>
        pageText.Contains("parameterizedRunnerStatus", StringComparison.Ordinal)
        && pageText.Contains("packagePath", StringComparison.Ordinal)
        && pageText.Contains("packagePathRelative", StringComparison.Ordinal)
        && pageText.Contains("normalCommand", StringComparison.Ordinal)
        && pageText.Contains("exampleCommandWithPackagePath", StringComparison.Ordinal)
        && pageText.Contains("resultPath", StringComparison.Ordinal)
        && pageText.Contains("logPath", StringComparison.Ordinal)
        && pageText.Contains("unityExitCode", StringComparison.Ordinal)
        && pageText.Contains("passMarkerPresent", StringComparison.Ordinal)
        && pageText.Contains("cleanupApplied", StringComparison.Ordinal)
        && pageText.Contains("manualUnityOptional", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal);
}
