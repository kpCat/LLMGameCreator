using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunGoal150BZeroValueRuntimeEvidenceCompleteSuiteClosureScriptTests
{
    [Fact]
    public void Goal150B_wrapper_is_status_aware_and_requires_all_evidence()
    {
        var root = FindRoot();
        var scriptPath = Path.Combine(root, ".devflow", "scripts",
            "run-goal150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix.ps1");
        var script = File.ReadAllText(scriptPath);
        Assert.Contains("run-complete-test-suite.ps1", script, StringComparison.Ordinal);
        Assert.Contains("goal150b-dashboard.json", script, StringComparison.Ordinal);
        Assert.Contains("publication-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("historical-artifact-integrity-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("goal149Accepted", script, StringComparison.Ordinal);
        Assert.Contains("manualReviewPerformed", script, StringComparison.Ordinal);
        Assert.Contains("BLOCKED", script, StringComparison.Ordinal);

        var cmd = File.ReadAllText(Path.ChangeExtension(scriptPath, ".cmd"));
        Assert.Contains("run-goal150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix.ps1",
            cmd, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }
}
