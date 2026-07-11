using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunGoal148ANewProjectSupportFilesHotfixScriptTests
{
    [Fact]
    public void Goal148A_runner_has_exact_output_guard_transactional_backup_and_required_proofs()
    {
        var root = FindRepositoryRoot();
        var script = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "run-goal148a-new-project-support-files-hotfix.ps1"));
        Assert.Contains("Goal148A OutputRoot must be the exact procedural artifact root", script, StringComparison.Ordinal);
        Assert.Contains("Goal148A refuses .llmgc/manual and .llmgc/workspace outputs", script, StringComparison.Ordinal);
        Assert.Contains("LLMGC_GOAL148A_RUN", script, StringComparison.Ordinal);
        Assert.Contains("new-project-production-build-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("support-file-conflict-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("support-file-missing-source-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("support-file-rollback-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("Restore-Goal148ADirectory", script, StringComparison.Ordinal);
        Assert.Contains("GOAL148A_NEW_PROJECT_SUPPORT_FILES_HOTFIX_GREEN", script, StringComparison.Ordinal);
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
