using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunGoal148BCurrentPackageUiThreadHotfixScriptTests
{
    [Fact]
    public void Goal148B_runner_has_exact_output_guard_transactional_backup_and_required_proofs()
    {
        var root = FindRepositoryRoot();
        var script = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "run-goal148b-current-package-ui-thread-hotfix.ps1"));
        Assert.Contains("Goal148B OutputRoot must be the exact procedural artifact root", script, StringComparison.Ordinal);
        Assert.Contains("Goal148B refuses .llmgc/manual and .llmgc/workspace outputs", script, StringComparison.Ordinal);
        Assert.Contains("LLMGC_GOAL148B_RUN", script, StringComparison.Ordinal);
        Assert.Contains("current-package-subscriber-inventory.json", script, StringComparison.Ordinal);
        Assert.Contains("mainform-worker-currentchanged-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("mainform-disposal-race-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("async-page-currentchanged-dispatch-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("real-workspace-build-retry-proof.json", script, StringComparison.Ordinal);
        Assert.Contains("Restore-Goal148BDirectory", script, StringComparison.Ordinal);
        Assert.Contains("GOAL148B_CURRENT_PACKAGE_UI_THREAD_HOTFIX_GREEN", script, StringComparison.Ordinal);
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
