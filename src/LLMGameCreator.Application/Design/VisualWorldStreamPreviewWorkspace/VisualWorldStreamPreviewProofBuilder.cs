namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewProofStatus BuildProof(
        string projectRoot,
        string sourceRoot,
        string sourceGoalId,
        string proofId,
        string fileName,
        string booleanProperty,
        IReadOnlyDictionary<string, string> ledger,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var relativePath = sourceRoot + "/" + fileName;
        var passed = false;
        var summary = "proof missing";
        using var doc = TryReadJson(projectRoot, relativePath, diagnostics);
        if (doc is not null)
        {
            passed = TryGetBool(doc.RootElement, booleanProperty);
            summary = BuildProofSummary(doc.RootElement, booleanProperty, passed);
        }
        if (!passed)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.proof.failed",
                proofId,
                "Required visual preview proof is missing or did not pass."));
        }

        return new VisualWorldPreviewProofStatus
        {
            ProofId = proofId,
            SourceGoalId = sourceGoalId,
            RelativePath = relativePath,
            Status = passed
                ? VisualWorldPreviewArtifactStatus.Passed
                : VisualWorldPreviewArtifactStatus.Failed,
            Passed = passed,
            Sha256 = File.Exists(Resolve(projectRoot, relativePath))
                ? HashFor(projectRoot, relativePath, ledger)
                : string.Empty,
            DiagnosticSummary = summary
        };
    }
}
