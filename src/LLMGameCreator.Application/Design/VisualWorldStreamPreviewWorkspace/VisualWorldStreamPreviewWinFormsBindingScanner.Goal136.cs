namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal136WinFormsPageText(string projectRoot)
    {
        const string pageGoal136RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal136.cs";
        return ReadOptionalText(projectRoot, pageGoal136RelativePath);
    }

    private static string ReadGoal120Through136WinFormsPageText(string projectRoot) =>
        ReadGoal120Through135WinFormsPageText(projectRoot)
        + Environment.NewLine
        + ReadGoal136WinFormsPageText(projectRoot);

    private static bool PageBindsGoal136CanonicalRuntimePlayerCommandLoop(string pageText) =>
        pageText.Contains("Goal136 Command Loop", StringComparison.Ordinal)
        && pageText.Contains("BindGoal136PlayerCommandLoop", StringComparison.Ordinal)
        && pageText.Contains("playerCommandLoopPassed", StringComparison.Ordinal)
        && pageText.Contains("playerCommandCount", StringComparison.Ordinal)
        && pageText.Contains("snapshotCount", StringComparison.Ordinal)
        && pageText.Contains("runtimeEventCount", StringComparison.Ordinal)
        && pageText.Contains("allRequiredCategoriesPresent", StringComparison.Ordinal)
        && pageText.Contains("unityPlayerConsumedCommandLoopSnapshots", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal)
        && pageText.Contains("unityGameplayTruth", StringComparison.Ordinal)
        && pageText.Contains("noUnclassifiedErrorDiagnostics", StringComparison.Ordinal)
        && pageText.Contains("matrixResultPath", StringComparison.Ordinal)
        && pageText.Contains("accepted", StringComparison.Ordinal);

    private static bool ScanGoal136CanonicalRuntimePlayerCommandLoopBinding(
        string pageText,
        string pageRelativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var binds = PageBindsGoal136CanonicalRuntimePlayerCommandLoop(pageText);
        AddIfFalse(
            binds,
            "goal136.winforms.canonical_runtime_player_command_loop_bind_missing",
            pageRelativePath,
            diagnostics);
        return binds;
    }
}
