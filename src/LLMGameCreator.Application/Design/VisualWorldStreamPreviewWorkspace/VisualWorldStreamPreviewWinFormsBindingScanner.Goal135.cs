namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal135WinFormsPageText(string projectRoot)
    {
        const string pageGoal135RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal135.cs";

        return ReadOptionalText(projectRoot, pageGoal135RelativePath);
    }

    private static string ReadGoal120Through135WinFormsPageText(string projectRoot) =>
        ReadGoal120Through134WinFormsPageText(projectRoot)
        + "\n"
        + ReadGoal135WinFormsPageText(projectRoot);

    private static bool PageBindsGoal135CanonicalRuntimePlayerLoopReadiness(string pageText) =>
        pageText.Contains("Goal135 Player Loop", StringComparison.Ordinal)
        && pageText.Contains("candidateId", StringComparison.Ordinal)
        && pageText.Contains("playerAdapterContractPresent", StringComparison.Ordinal)
        && pageText.Contains("playerLoopStepCount", StringComparison.Ordinal)
        && pageText.Contains("requiredStepCategoriesPresent", StringComparison.Ordinal)
        && pageText.Contains("unityPlayerLoopReadinessPassed", StringComparison.Ordinal)
        && pageText.Contains("canonicalRuntimeSource", StringComparison.Ordinal)
        && pageText.Contains("unityGameplayTruth", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal)
        && pageText.Contains("noUnclassifiedErrorDiagnostics", StringComparison.Ordinal)
        && pageText.Contains("normalCommand", StringComparison.Ordinal)
        && pageText.Contains("reportPath", StringComparison.Ordinal)
        && pageText.Contains("manualUnityOptional", StringComparison.Ordinal);

    private static bool ScanGoal135CanonicalRuntimePlayerLoopBinding(
        string pageText,
        string pageRelativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var binds = PageBindsGoal135CanonicalRuntimePlayerLoopReadiness(pageText);
        AddIfFalse(
            binds,
            "goal135.winforms.player_loop_bind_missing",
            pageRelativePath,
            diagnostics);
        return binds;
    }
}
