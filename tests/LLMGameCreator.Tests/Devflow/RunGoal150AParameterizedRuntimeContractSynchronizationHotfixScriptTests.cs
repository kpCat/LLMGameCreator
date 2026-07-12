using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunGoal150AParameterizedRuntimeContractSynchronizationHotfixScriptTests
{
    [Fact]
    public void Goal150A_runner_is_bounded_transactional_and_requires_green_unaccepted_artifacts()
    {
        var root = FindRoot();
        var script = File.ReadAllText(Path.Combine(root, ".devflow", "scripts",
            "run-goal150a-parameterized-runtime-contract-synchronization-hotfix.ps1"));
        Assert.Contains("FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronizationTests", script, StringComparison.Ordinal);
        Assert.Contains("FullyQualifiedName~Goal150AArtifactProofTests", script, StringComparison.Ordinal);
        Assert.Contains("GOAL150A_PARAMETERIZED_RUNTIME_CONTRACT_SYNCHRONIZATION_GREEN", script, StringComparison.Ordinal);
        Assert.Contains("goal150a-dashboard.json", script, StringComparison.Ordinal);
        Assert.Contains("goal150aAccepted", script, StringComparison.Ordinal);
        Assert.Contains("manualReviewPerformed", script, StringComparison.Ordinal);
        Assert.Contains("Restore-Goal150ADirectory", script, StringComparison.Ordinal);
        Assert.Contains("check-artifact-scope.ps1", script, StringComparison.Ordinal);
        Assert.Contains("Get-FileHash", script, StringComparison.Ordinal);
        Assert.DoesNotContain("Start-Process", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("git ", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".llmgc/manual", script.Replace("refuses .llmgc/manual", string.Empty),
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
