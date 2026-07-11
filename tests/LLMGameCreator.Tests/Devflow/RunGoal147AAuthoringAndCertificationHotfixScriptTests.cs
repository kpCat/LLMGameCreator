using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunGoal147AAuthoringAndCertificationHotfixScriptTests
{
    [Fact]
    public void Script_runs_executable_proofs_guards_outputs_and_rolls_back_transactionally()
    {
        var root = FindRoot();
        var path = Path.Combine(root, ".devflow", "scripts", "run-goal147a-authoring-and-certification-hotfix.ps1");
        var source = File.ReadAllText(path);
        Assert.Contains("$OutputRoot", source, StringComparison.Ordinal);
        Assert.Contains("$DryRun", source, StringComparison.Ordinal);
        Assert.Contains("$ApplyCleanup", source, StringComparison.Ordinal);
        Assert.Contains("Goal147A refuses .llmgc/manual and .llmgc/workspace", source, StringComparison.Ordinal);
        Assert.Contains("FullyQualifiedName~Goal147A_script", source, StringComparison.Ordinal);
        Assert.Contains("Restore-Goal147ADirectory", source, StringComparison.Ordinal);
        Assert.Contains("programmaticItemCheckAppliedCount", source, StringComparison.Ordinal);
        Assert.Contains("dependencyChangeExecutedCount", source, StringComparison.Ordinal);
        Assert.Contains("GOAL147A_AUTHORING_AND_CERTIFICATION_HOTFIX_GREEN", source, StringComparison.Ordinal);
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
