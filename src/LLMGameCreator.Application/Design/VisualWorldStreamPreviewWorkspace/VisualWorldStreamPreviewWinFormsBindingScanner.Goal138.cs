namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal138WinFormsPageText(string projectRoot)
    {
        const string pageGoal138RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal138.cs";
        return ReadOptionalText(projectRoot, pageGoal138RelativePath);
    }

    private static string ReadGoal120Through138WinFormsPageText(string projectRoot) =>
        ReadGoal120Through137WinFormsPageText(projectRoot)
        + Environment.NewLine
        + ReadGoal138WinFormsPageText(projectRoot);

    private static bool PageBindsGoal138RuntimeBackedUnityPlayerLoopStepper(
        string pageText) =>
        pageText.Contains("Goal138 Stepper", StringComparison.Ordinal)
        && pageText.Contains("BindGoal138RuntimeBackedUnityPlayerLoopStepper", StringComparison.Ordinal)
        && pageText.Contains("acceptedGoal137", StringComparison.Ordinal)
        && pageText.Contains("candidateId", StringComparison.Ordinal)
        && pageText.Contains("frameCount", StringComparison.Ordinal)
        && pageText.Contains("requiredFrameCategoriesPresent", StringComparison.Ordinal)
        && pageText.Contains("runtimeAuthority", StringComparison.Ordinal)
        && pageText.Contains("unityGameplayTruth", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal)
        && pageText.Contains("stepperWindowPresent", StringComparison.Ordinal)
        && pageText.Contains("stepperBatchSmokePassed", StringComparison.Ordinal)
        && pageText.Contains("normalCommand", StringComparison.Ordinal)
        && pageText.Contains("reportPath", StringComparison.Ordinal)
        && pageText.Contains("manualUnityOptional", StringComparison.Ordinal);

    private static bool ScanGoal138RuntimeBackedUnityPlayerLoopStepperBinding(
        string pageText,
        string pageRelativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var binds = PageBindsGoal138RuntimeBackedUnityPlayerLoopStepper(pageText);
        AddIfFalse(
            binds,
            "goal138.winforms.runtime_backed_unity_player_loop_stepper_bind_missing",
            pageRelativePath,
            diagnostics);
        return binds;
    }
}
