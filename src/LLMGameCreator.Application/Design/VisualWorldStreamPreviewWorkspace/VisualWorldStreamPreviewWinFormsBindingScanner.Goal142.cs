namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal142WinFormsPageText(string projectRoot)
    {
        const string pageGoal142RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal142.cs";

        return ReadOptionalText(projectRoot, pageGoal142RelativePath);
    }

    private static string ReadGoal120Through142WinFormsPageText(string projectRoot) =>
        ReadGoal120Through141WinFormsPageText(projectRoot)
        + Environment.NewLine
        + ReadGoal142WinFormsPageText(projectRoot);

    private static string ReadGoal108Through142WinFormsPageText(string projectRoot)
    {
        var root = "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/";
        var olderGoalPages = new[]
        {
            "VisualWorldStreamPreviewWorkspacePageControl.Goal108.cs",
            "VisualWorldStreamPreviewWorkspacePageControl.Goal109.cs",
            "VisualWorldStreamPreviewWorkspacePageControl.Goal110.cs",
            "VisualWorldStreamPreviewWorkspacePageControl.Goal111.cs",
            "VisualWorldStreamPreviewWorkspacePageControl.Goal112.cs",
            "VisualWorldStreamPreviewWorkspacePageControl.Goal113.cs",
            "VisualWorldStreamPreviewWorkspacePageControl.Goal115.cs",
            "VisualWorldStreamPreviewWorkspacePageControl.Goal116.cs",
            "VisualWorldStreamPreviewWorkspacePageControl.Goal117.cs",
            "VisualWorldStreamPreviewWorkspacePageControl.Goal118.cs",
            "VisualWorldStreamPreviewWorkspacePageControl.Goal119.cs"
        };
        var texts = olderGoalPages
            .Select(fileName => ReadOptionalText(projectRoot, root + fileName))
            .Append(ReadGoal120Through142WinFormsPageText(projectRoot));

        return string.Join(Environment.NewLine, texts);
    }

    private static bool PageBindsGoal142ProductLineRuntimeVariantMatrix(string pageText) =>
        pageText.Contains("Goal142 Runtime Variants", StringComparison.Ordinal)
        && pageText.Contains("BindGoal142ProductLineRuntimeVariantMatrix", StringComparison.Ordinal)
        && pageText.Contains("Run Runtime Variant Matrix", StringComparison.Ordinal)
        && pageText.Contains("matrixStatus", StringComparison.Ordinal)
        && pageText.Contains("candidateCount", StringComparison.Ordinal)
        && pageText.Contains("passedCandidateCount", StringComparison.Ordinal)
        && pageText.Contains("failedCandidateCount", StringComparison.Ordinal)
        && pageText.Contains("runtimeSignificantCandidateCount", StringComparison.Ordinal)
        && pageText.Contains("distinctFinalStateHashCount", StringComparison.Ordinal)
        && pageText.Contains("selectedCandidateId", StringComparison.Ordinal)
        && pageText.Contains("selectedVariantKind", StringComparison.Ordinal)
        && pageText.Contains("selectedScore", StringComparison.Ordinal)
        && pageText.Contains("sourceTemplateUnmodified", StringComparison.Ordinal)
        && pageText.Contains("normalCommand", StringComparison.Ordinal)
        && pageText.Contains("matrixResultPath", StringComparison.Ordinal)
        && pageText.Contains("selectedHandoffPath", StringComparison.Ordinal)
        && pageText.Contains("accepted", StringComparison.Ordinal)
        && pageText.Contains("Goal142RuntimeVariantMatrixOutputTail", StringComparison.Ordinal);

    private static bool ScanGoal142ProductLineRuntimeVariantMatrixBinding(
        string pageText,
        string pageRelativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var binds = PageBindsGoal142ProductLineRuntimeVariantMatrix(pageText);
        AddIfFalse(
            binds,
            "goal142.winforms.product_line_runtime_variant_matrix_bind_missing",
            pageRelativePath,
            diagnostics);
        return binds;
    }
}
