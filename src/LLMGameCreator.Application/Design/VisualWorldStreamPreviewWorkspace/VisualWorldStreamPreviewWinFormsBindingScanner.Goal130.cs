namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal130WinFormsPageText(string projectRoot)
    {
        const string pageGoal130RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal130.cs";

        return ReadOptionalText(projectRoot, pageGoal130RelativePath);
    }

    private static string ReadGoal120Through130WinFormsPageText(string projectRoot) =>
        ReadGoal120Through129WinFormsPageText(projectRoot)
        + "\n"
        + ReadGoal130WinFormsPageText(projectRoot);

    private static bool PageBindsGoal130GamePackageCandidateFactory(string pageText) =>
        pageText.Contains("candidateFactoryStatus", StringComparison.Ordinal)
        && pageText.Contains("candidateCount", StringComparison.Ordinal)
        && pageText.Contains("passedCandidates", StringComparison.Ordinal)
        && pageText.Contains("failedCandidates", StringComparison.Ordinal)
        && pageText.Contains("matrixPassed", StringComparison.Ordinal)
        && pageText.Contains("candidateIndexPath", StringComparison.Ordinal)
        && pageText.Contains("normalCommand", StringComparison.Ordinal)
        && pageText.Contains("factoryResultPath", StringComparison.Ordinal)
        && pageText.Contains("matrixResultPath", StringComparison.Ordinal)
        && pageText.Contains("manualUnityOptional", StringComparison.Ordinal)
        && pageText.Contains("samplePackageUnmodified", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal)
        && pageText.Contains("evidencePath", StringComparison.Ordinal)
        && pageText.Contains("exportPath", StringComparison.Ordinal);
}
