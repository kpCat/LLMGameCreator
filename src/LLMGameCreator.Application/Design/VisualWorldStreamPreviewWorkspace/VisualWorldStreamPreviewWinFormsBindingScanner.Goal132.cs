namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal132WinFormsPageText(string projectRoot)
    {
        const string pageGoal132RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal132.cs";

        return ReadOptionalText(projectRoot, pageGoal132RelativePath);
    }

    private static string ReadGoal120Through132WinFormsPageText(string projectRoot) =>
        ReadGoal120Through131WinFormsPageText(projectRoot)
        + "\n"
        + ReadGoal132WinFormsPageText(projectRoot);

    private static bool PageBindsGoal132CandidatePipelineOperator(string pageText) =>
        pageText.Contains("Goal132 Candidate Pipeline Operator", StringComparison.Ordinal)
        && pageText.Contains("Refresh Candidate Pipeline Status", StringComparison.Ordinal)
        && pageText.Contains("Copy Candidate Pipeline Command", StringComparison.Ordinal)
        && pageText.Contains("Dry Run Candidate Recipe Pipeline", StringComparison.Ordinal)
        && pageText.Contains("Run Candidate Recipe Pipeline", StringComparison.Ordinal)
        && pageText.Contains("operatorStatus", StringComparison.Ordinal)
        && pageText.Contains("normalCommand", StringComparison.Ordinal)
        && pageText.Contains("dryRunCommand", StringComparison.Ordinal)
        && pageText.Contains("resultPath", StringComparison.Ordinal)
        && pageText.Contains("selectedCandidateId", StringComparison.Ordinal)
        && pageText.Contains("selectedCandidateScore", StringComparison.Ordinal)
        && pageText.Contains("candidateCount", StringComparison.Ordinal)
        && pageText.Contains("passedCandidates", StringComparison.Ordinal)
        && pageText.Contains("failedCandidates", StringComparison.Ordinal)
        && pageText.Contains("matrixPassed", StringComparison.Ordinal)
        && pageText.Contains("lastOperatorExitCode", StringComparison.Ordinal)
        && pageText.Contains("lastOperatorDurationMilliseconds", StringComparison.Ordinal)
        && pageText.Contains("manualUnityOptional", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal)
        && pageText.Contains("samplePackageReadOnly", StringComparison.Ordinal)
        && pageText.Contains("winFormsPanelPresent", StringComparison.Ordinal)
        && pageText.Contains("asyncRunPresent", StringComparison.Ordinal)
        && pageText.Contains("CandidatePipelineOperatorOutputTail", StringComparison.Ordinal);

    private static bool ScanGoal132CandidatePipelineOperatorBinding(
        string pageText,
        string pageRelativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var binds = PageBindsGoal132CandidatePipelineOperator(pageText);
        AddIfFalse(
            binds,
            "goal132.winforms.candidate_pipeline_operator_bind_missing",
            pageRelativePath,
            diagnostics);
        return binds;
    }
}
