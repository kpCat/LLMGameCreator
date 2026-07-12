using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunGoal152CExactUnityCleanupScriptTests
{
    [Fact]
    public void Cleanup_script_uses_exact_literal_authorized_paths_without_broad_cleanup()
    {
        var root = FindRoot();
        var script = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "run-goal152c-exact-unity-cleanup.ps1"));
        Assert.Equal(21, script.Split("'unity/LLMGameCreatorAlpha/", StringSplitOptions.None).Length - 1);
        Assert.Contains("Remove-Item -LiteralPath", script, StringComparison.Ordinal);
        Assert.DoesNotContain("git clean", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("-Recurse", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("exact-cleanup-before.json", script, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
