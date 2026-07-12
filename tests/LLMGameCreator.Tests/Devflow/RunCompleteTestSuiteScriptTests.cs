using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunCompleteTestSuiteScriptTests
{
    [Fact]
    public void Complete_suite_runner_declares_exhaustive_disjoint_bounded_contract()
    {
        var root = FindRoot();
        var script = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "run-complete-test-suite.ps1"));
        Assert.Contains("--list-tests", script, StringComparison.Ordinal);
        Assert.Contains("partitionKind = \"disjoint_class_groups\"", script, StringComparison.Ordinal);
        Assert.Contains("missingAssignmentCount", script, StringComparison.Ordinal);
        Assert.Contains("duplicateAssignmentCount", script, StringComparison.Ordinal);
        Assert.Contains("taskkill /PID", script, StringComparison.Ordinal);
        Assert.Contains("complete-suite-slowest-tests.json", script, StringComparison.Ordinal);
        Assert.Contains("MonolithicTimeoutSeconds -gt 900", script, StringComparison.Ordinal);
        Assert.DoesNotContain("--filter Category", script, StringComparison.OrdinalIgnoreCase);

        var cmd = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "run-complete-test-suite.cmd"));
        Assert.Contains("run-complete-test-suite.ps1", cmd, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("%*", cmd, StringComparison.Ordinal);
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
