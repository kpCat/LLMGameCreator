namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static string ReadGoal134WinFormsPageText(string projectRoot)
    {
        const string pageGoal134RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal134.cs";

        return ReadOptionalText(projectRoot, pageGoal134RelativePath);
    }

    private static string ReadGoal120Through134WinFormsPageText(string projectRoot) =>
        ReadGoal120Through132WinFormsPageText(projectRoot)
        + "\n"
        + ReadGoal134WinFormsPageText(projectRoot);

    private static bool PageBindsGoal134CanonicalRuntimeSelectedCandidate(string pageText) =>
        pageText.Contains("Goal134 Canonical Runtime", StringComparison.Ordinal)
        && pageText.Contains("candidateId", StringComparison.Ordinal)
        && pageText.Contains("packageValidationPassed", StringComparison.Ordinal)
        && pageText.Contains("canonicalRuntimePassed", StringComparison.Ordinal)
        && pageText.Contains("runtimeCommandCount", StringComparison.Ordinal)
        && pageText.Contains("runtimeEventCount", StringComparison.Ordinal)
        && pageText.Contains("saveLoadReplayPassed", StringComparison.Ordinal)
        && pageText.Contains("unityPlayerConsumedCanonicalTranscript", StringComparison.Ordinal)
        && pageText.Contains("projectionOnly", StringComparison.Ordinal)
        && pageText.Contains("selectedCandidateExecutedByRuntime", StringComparison.Ordinal)
        && pageText.Contains("normalCommand", StringComparison.Ordinal)
        && pageText.Contains("reportPath", StringComparison.Ordinal)
        && pageText.Contains("matrixResultPath", StringComparison.Ordinal)
        && pageText.Contains("manualUnityOptional", StringComparison.Ordinal);

    private static bool ScanGoal134CanonicalRuntimeSelectedCandidateBinding(
        string pageText,
        string pageRelativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var binds = PageBindsGoal134CanonicalRuntimeSelectedCandidate(pageText);
        AddIfFalse(
            binds,
            "goal134.winforms.canonical_runtime_bind_missing",
            pageRelativePath,
            diagnostics);
        return binds;
    }
}
