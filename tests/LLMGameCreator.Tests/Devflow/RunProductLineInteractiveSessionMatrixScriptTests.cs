using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunProductLineInteractiveSessionMatrixScriptTests
{
    [Fact]
    public void Goal145_script_has_required_parameters_guards_unity_and_transactional_rollback()
    {
        var root = FindRoot();
        var script = File.ReadAllText(Path.Combine(root,
            ".devflow/scripts/run-product-line-interactive-session-matrix.ps1"));
        foreach (var marker in new[]
                 {
                     "$Goal142Root", "$OutputRoot", "$SelectedCandidateId", "$UnityPath", "$DryRun",
                     "$ApplyCleanup", ".llmgc/manual", "product-line-runtime-variant-matrix-result.json",
                     "Get-FileHash", "RunBatchmodeProductLineInteractiveSessionMatrixSmoke",
                     "Invoke-Goal145Core $true", "Restore-Goal145Directory", "GOAL145_PRODUCT_LINE_INTERACTIVE_SESSION_MATRIX_GREEN"
                 })
            Assert.Contains(marker, script, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (Directory.GetParent(current) is { } parent)
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = parent.FullName;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }
}
