using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunGoal150EHistoricalIdentityReconciliationTests
{
    [Fact]
    public void Goal150E_runner_declares_exact_identity_reconciliation_and_current_case_accounting()
    {
        var root = FindRoot();
        var wrapper = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "run-goal150e-historical-test-identity-reconciliation-and-manual-gate-readiness-hotfix.ps1"));
        var runner = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "run-complete-test-suite.ps1"));
        Assert.Contains("historicalIdentityCount", wrapper, StringComparison.Ordinal);
        Assert.Contains("canonical_method", wrapper, StringComparison.Ordinal);
        Assert.Contains("explicit_rename", wrapper, StringComparison.Ordinal);
        Assert.Contains("historical-identity-aliases.json", wrapper, StringComparison.Ordinal);
        Assert.Contains("ReconciliationManifestPath", runner, StringComparison.Ordinal);
        Assert.Contains("currentExecutionCaseCount", runner, StringComparison.Ordinal);
        Assert.Contains("duplicateResultCount", runner, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
