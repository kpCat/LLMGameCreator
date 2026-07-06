namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal129WinFormsPageText(string projectRoot)
    {
        const string pageGoal129RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal129.cs";

        return ReadOptionalText(projectRoot, pageGoal129RelativePath);
    }

    private static string ReadGoal120Through129WinFormsPageText(string projectRoot) =>
        ReadGoal120Through128WinFormsPageText(projectRoot)
        + "\n"
        + ReadGoal129WinFormsPageText(projectRoot);

    private static bool PageBindsGoal129GamePackageCandidateMatrix(string pageText) =>
        pageText.Contains("gamePackageCandidateMatrixStatus", StringComparison.Ordinal)
        && pageText.Contains("candidateCount", StringComparison.Ordinal)
        && pageText.Contains("passedCandidateCount", StringComparison.Ordinal)
        && pageText.Contains("failedCandidateCount", StringComparison.Ordinal)
        && pageText.Contains("candidateIndexPath", StringComparison.Ordinal)
        && pageText.Contains("matrixResultPath", StringComparison.Ordinal)
        && pageText.Contains("normalCommand", StringComparison.Ordinal)
        && pageText.Contains("exampleCommand", StringComparison.Ordinal)
        && pageText.Contains("baselineCandidatePackagePath", StringComparison.Ordinal)
        && pageText.Contains("variantCandidatePackagePath", StringComparison.Ordinal)
        && pageText.Contains("cleanupApplied", StringComparison.Ordinal)
        && pageText.Contains("manualUnityOptional", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal);
}
