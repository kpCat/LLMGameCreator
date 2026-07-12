using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunGoal150DValidationAccountingTests
{
    [Fact]
    public void Goal150D_runner_declares_closure_accounting_and_preserves_tracked_baselines()
    {
        var root = FindRoot();
        var runner = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "run-complete-test-suite.ps1"));
        var wrapper = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "run-goal150d-validation-accounting-and-bundled-manual-gate-readiness-hotfix.ps1"));
        Assert.Contains("Goal150AcceptanceClosure", runner, StringComparison.Ordinal);
        Assert.Contains("attempted", runner, StringComparison.Ordinal);
        Assert.Contains("notRun", runner, StringComparison.Ordinal);
        Assert.Contains("timedOut", runner, StringComparison.Ordinal);
        Assert.DoesNotContain("Remove-Item -LiteralPath $path -Recurse -Force", runner, StringComparison.Ordinal);
        Assert.Contains("sourceFailedCount=64", wrapper, StringComparison.Ordinal);
        Assert.Contains("sourceMissingCount=21", wrapper, StringComparison.Ordinal);
        Assert.Contains("manualGateReady", wrapper, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
