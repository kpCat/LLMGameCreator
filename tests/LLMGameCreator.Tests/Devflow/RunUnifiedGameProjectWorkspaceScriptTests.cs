using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunUnifiedGameProjectWorkspaceScriptTests
{
    [Fact]
    public void RunUnifiedGameProjectWorkspaceScript_has_exact_output_guard_transactional_backup_and_required_proofs()
    {
        var root = FindRepositoryRoot();
        var script = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "run-unified-game-project-workspace.ps1"));
        Assert.Contains("Goal148 OutputRoot must be the exact procedural artifact root", script, StringComparison.Ordinal);
        Assert.Contains("Goal148 refuses .llmgc/manual and .llmgc/workspace outputs", script, StringComparison.Ordinal);
        Assert.Contains("LLMGC_GOAL148_RUN", script, StringComparison.Ordinal);
        Assert.Contains("project-build-activation-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("project-build-rollback-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("normalWorkspaceGoalNumberControlCount", script, StringComparison.Ordinal);
        Assert.Contains("Restore-Goal148Directory", script, StringComparison.Ordinal);
        Assert.Contains("GOAL148_UNIFIED_GAME_PROJECT_WORKSPACE_GREEN", script, StringComparison.Ordinal);
        Assert.DoesNotContain("Unity.exe", script, StringComparison.OrdinalIgnoreCase);
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
