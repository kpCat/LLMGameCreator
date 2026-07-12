using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunCompleteTestSuiteScriptTests
{
    [Fact]
    public void Complete_suite_runner_declares_hermetic_adaptive_bounded_contract()
    {
        var root = FindRoot();
        var script = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "run-complete-test-suite.ps1"));
        Assert.Contains("--list-tests", script, StringComparison.Ordinal);
        Assert.Contains("git worktree add --detach", script, StringComparison.Ordinal);
        Assert.Contains("Reset-DisposableWorktree", script, StringComparison.Ordinal);
        Assert.Contains("LLMGC_PRODUCT_SMOKE_PROJECT_DIR", script, StringComparison.Ordinal);
        Assert.Contains("LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR", script, StringComparison.Ordinal);
        Assert.Contains("FullyQualifiedName!~ProductSmoke", script, StringComparison.Ordinal);
        Assert.Contains("terminal-results.json", script, StringComparison.Ordinal);
        Assert.Contains("MaximumWallClockMinutes", script, StringComparison.Ordinal);
        Assert.Contains("taskkill /PID", script, StringComparison.Ordinal);
        Assert.Contains("HeavyTestTimeoutSeconds", script, StringComparison.Ordinal);
        Assert.DoesNotContain("--filter Category", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MonolithicTimeoutSeconds", script, StringComparison.Ordinal);

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
