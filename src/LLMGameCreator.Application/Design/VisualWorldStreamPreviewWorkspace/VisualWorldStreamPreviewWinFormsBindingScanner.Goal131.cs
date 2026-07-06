namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal131WinFormsPageText(string projectRoot)
    {
        const string pageGoal131RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal131.cs";

        return ReadOptionalText(projectRoot, pageGoal131RelativePath);
    }

    private static string ReadGoal120Through131WinFormsPageText(string projectRoot) =>
        ReadGoal120Through130WinFormsPageText(projectRoot)
        + "\n"
        + ReadGoal131WinFormsPageText(projectRoot);

    private static bool PageBindsGoal131GamePackageCandidateRecipePipeline(string pageText) =>
        pageText.Contains("recipePipelineStatus", StringComparison.Ordinal)
        && pageText.Contains("recipeCount", StringComparison.Ordinal)
        && pageText.Contains("candidateCount", StringComparison.Ordinal)
        && pageText.Contains("passedCandidates", StringComparison.Ordinal)
        && pageText.Contains("failedCandidates", StringComparison.Ordinal)
        && pageText.Contains("matrixPassed", StringComparison.Ordinal)
        && pageText.Contains("selectedCandidateId", StringComparison.Ordinal)
        && pageText.Contains("selectedCandidateScore", StringComparison.Ordinal)
        && pageText.Contains("recipeCatalogPath", StringComparison.Ordinal)
        && pageText.Contains("candidateIndexPath", StringComparison.Ordinal)
        && pageText.Contains("normalCommand", StringComparison.Ordinal)
        && pageText.Contains("pipelineResultPath", StringComparison.Ordinal)
        && pageText.Contains("scoringResultPath", StringComparison.Ordinal)
        && pageText.Contains("matrixResultPath", StringComparison.Ordinal)
        && pageText.Contains("selectedCandidatePackagePath", StringComparison.Ordinal)
        && pageText.Contains("selectedCandidateHandoffPath", StringComparison.Ordinal)
        && pageText.Contains("manualUnityOptional", StringComparison.Ordinal)
        && pageText.Contains("samplePackageUnmodified", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal)
        && pageText.Contains("metadataOnlyRecipeMutation", StringComparison.Ordinal)
        && pageText.Contains("evidencePath", StringComparison.Ordinal)
        && pageText.Contains("exportPath", StringComparison.Ordinal);

    private static bool ScanGoal131GamePackageCandidateRecipePipelineBinding(
        string pageText,
        string pageRelativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var binds = PageBindsGoal131GamePackageCandidateRecipePipeline(pageText);
        AddIfFalse(
            binds,
            "goal131.winforms.gamepackage_candidate_recipe_pipeline_bind_missing",
            pageRelativePath,
            diagnostics);
        return binds;
    }
}
