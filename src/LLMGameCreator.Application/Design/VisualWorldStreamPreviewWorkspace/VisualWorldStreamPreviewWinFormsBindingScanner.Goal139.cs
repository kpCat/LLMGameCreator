namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal139WinFormsPageText(string projectRoot)
    {
        const string pageGoal139RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal139.cs";
        return ReadOptionalText(projectRoot, pageGoal139RelativePath);
    }

    private static string ReadGoal120Through139WinFormsPageText(string projectRoot) =>
        ReadGoal120Through138WinFormsPageText(projectRoot)
        + Environment.NewLine
        + ReadGoal139WinFormsPageText(projectRoot);

    private static bool PageBindsGoal139RuntimeBackedUnityPlayerLoopInteractiveControls(
        string pageText) =>
        pageText.Contains("Goal139 Controls", StringComparison.Ordinal)
        && pageText.Contains("BindGoal139RuntimeBackedUnityPlayerLoopInteractiveControls", StringComparison.Ordinal)
        && pageText.Contains("acceptedGoal138", StringComparison.Ordinal)
        && pageText.Contains("candidateId", StringComparison.Ordinal)
        && pageText.Contains("frameCount", StringComparison.Ordinal)
        && pageText.Contains("requiredControlsPresent", StringComparison.Ordinal)
        && pageText.Contains("controlScriptPassed", StringComparison.Ordinal)
        && pageText.Contains("interactiveControlsWindowPresent", StringComparison.Ordinal)
        && pageText.Contains("unityInteractiveControlsSmokePassed", StringComparison.Ordinal)
        && pageText.Contains("runtimeAuthority", StringComparison.Ordinal)
        && pageText.Contains("unityGameplayTruth", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal)
        && pageText.Contains("normalCommand", StringComparison.Ordinal)
        && pageText.Contains("reportPath", StringComparison.Ordinal)
        && pageText.Contains("manualUnityOptional", StringComparison.Ordinal);

    private static bool ScanGoal139RuntimeBackedUnityPlayerLoopInteractiveControlsBinding(
        string pageText,
        string pageRelativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var binds = PageBindsGoal139RuntimeBackedUnityPlayerLoopInteractiveControls(pageText);
        AddIfFalse(
            binds,
            "goal139.winforms.runtime_backed_unity_player_loop_interactive_controls_bind_missing",
            pageRelativePath,
            diagnostics);
        return binds;
    }
}
