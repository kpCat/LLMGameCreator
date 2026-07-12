using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunGoal152AStandaloneUxScriptTests
{
    [Fact]
    public void Goal_runner_uses_a_disposable_localappdata_copy_and_the_targeted_real_copy_test()
    {
        var root = FindRoot();
        var script = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "run-goal152a-standalone-playeradapter-ux-hotfix.ps1"));
        Assert.Contains("LLMGC_GOAL152_REAL_STANDALONE_RUN", script, StringComparison.Ordinal);
        Assert.Contains("Goal152ProjectStandaloneBuildTests", script, StringComparison.Ordinal);
        Assert.Contains("LocalApplicationData", script, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
