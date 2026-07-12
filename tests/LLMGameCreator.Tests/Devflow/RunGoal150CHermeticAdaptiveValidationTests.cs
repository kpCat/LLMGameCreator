using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunGoal150CHermeticAdaptiveValidationTests
{
    [Fact]
    public void Goal150C_runner_and_wrapper_declare_the_required_hermetic_contract()
    {
        var root = FindRoot();
        var runner = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "run-complete-test-suite.ps1"));
        var wrapperPath = Path.Combine(root, ".devflow", "scripts",
            "run-goal150c-hermetic-adaptive-validation-and-acceptance-readiness-hotfix.ps1");
        var wrapper = File.ReadAllText(wrapperPath);

        Assert.Contains("New-DisposableWorktree", runner, StringComparison.Ordinal);
        Assert.Contains("Remove-DisposableWorktree", runner, StringComparison.Ordinal);
        Assert.Contains("Initialize-ShardEnvironment", runner, StringComparison.Ordinal);
        Assert.Contains("deterministic_namespace_class_groups", runner, StringComparison.Ordinal);
        Assert.Contains("Invoke-Group $Group $Depth $true", runner, StringComparison.Ordinal);
        Assert.Contains("single_test_timeout", runner, StringComparison.Ordinal);
        Assert.Contains("maximumSimultaneousTesthostProcesses=1", runner, StringComparison.Ordinal);
        Assert.Contains("mainWorktreeUnchangedByValidation", runner, StringComparison.Ordinal);
        Assert.Contains("validation-discovery-summary.json", runner, StringComparison.Ordinal);
        Assert.Contains("validation-lane-plan.json", runner, StringComparison.Ordinal);
        Assert.Contains("validation-result.json", runner, StringComparison.Ordinal);

        Assert.Contains("run-complete-test-suite.ps1", wrapper, StringComparison.Ordinal);
        Assert.Contains("validation-failure-taxonomy.json", wrapper, StringComparison.Ordinal);
        Assert.Contains("initialGoal150BFailureCount=64", wrapper, StringComparison.Ordinal);
        Assert.Contains("rawLogsIgnored=$true", wrapper, StringComparison.Ordinal);
        Assert.Contains("manualAcceptanceClaimed=$false", wrapper, StringComparison.Ordinal);

        var cmd = File.ReadAllText(Path.ChangeExtension(wrapperPath, ".cmd"));
        Assert.Contains("run-goal150c-hermetic-adaptive-validation-and-acceptance-readiness-hotfix.ps1", cmd,
            StringComparison.OrdinalIgnoreCase);
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
