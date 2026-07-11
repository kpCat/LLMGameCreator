using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunGoal148CProjectIdentityHotfixScriptTests
{
    [Fact]
    public void Goal148C_runner_has_exact_output_guard_transactional_backup_and_required_proofs()
    {
        var root = FindRepositoryRoot();
        var script = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "run-goal148c-project-identity-hotfix.ps1"));
        Assert.Contains("Goal148C OutputRoot must be the exact procedural artifact root", script, StringComparison.Ordinal);
        Assert.Contains("Goal148C refuses .llmgc/manual and .llmgc/workspace outputs", script, StringComparison.Ordinal);
        Assert.Contains("LLMGC_GOAL148C_RUN", script, StringComparison.Ordinal);
        Assert.Contains("project-identity-capture-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("legacy-authoring-migration-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("two-project-identity-isolation-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("identity-rollback-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("mainform-project-title-consistency-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("Restore-Goal148CDirectory", script, StringComparison.Ordinal);
        Assert.Contains("GOAL148C_PROJECT_IDENTITY_HOTFIX_GREEN", script, StringComparison.Ordinal);
        Assert.DoesNotContain("Unity.exe", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Start-Process", script, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
