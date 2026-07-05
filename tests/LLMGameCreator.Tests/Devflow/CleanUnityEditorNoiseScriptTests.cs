using Xunit;

namespace LLMGameCreator.Tests.DevFlow;

public sealed class CleanUnityEditorNoiseScriptTests
{
    [Fact]
    public void CleanUnityEditorNoiseScriptKeepsBoundedDryRunApplyAndSafeTargets()
    {
        var root = ProjectRoot();
        var script = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "clean-unity-editor-noise.ps1"));
        var cmd = File.ReadAllText(Path.Combine(
            root,
            ".devflow",
            "scripts",
            "clean-unity-editor-noise.cmd"));

        Assert.Contains("[switch]$DryRun", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$Apply", script, StringComparison.Ordinal);
        Assert.Contains("[switch]$AllowStaged", script, StringComparison.Ordinal);
        Assert.Contains("git status --porcelain=v1 --untracked-files=all", script, StringComparison.Ordinal);
        Assert.Contains("[string[]]$statusLines = @(", script, StringComparison.Ordinal);
        Assert.Contains("return ,$statusLines", script, StringComparison.Ordinal);
        Assert.Contains("[AllowEmptyCollection()]", script, StringComparison.Ordinal);
        Assert.Contains("[AllowNull()]", script, StringComparison.Ordinal);
        Assert.Contains("[string[]]$StatusLines = @()", script, StringComparison.Ordinal);
        Assert.Contains("foreach ($line in @($StatusLines))", script, StringComparison.Ordinal);
        Assert.Contains("[string[]]$afterStatusLines = Invoke-CleanupGitStatus", script, StringComparison.Ordinal);
        Assert.Contains("[string[]]$finalStatusLines = Invoke-CleanupGitStatus", script, StringComparison.Ordinal);
        Assert.Contains("Write-Host \"Final status:\"", script, StringComparison.Ordinal);
        Assert.Contains("Refusing cleanup because staged files are present", script, StringComparison.Ordinal);
        Assert.Contains("unity/LLMGameCreatorAlpha/Assets/", script, StringComparison.Ordinal);
        Assert.Contains(".meta", script, StringComparison.Ordinal);
        Assert.Contains("unity/LLMGameCreatorAlpha/Packages/packages-lock.json", script, StringComparison.Ordinal);
        Assert.Contains("unity/LLMGameCreatorAlpha/ProjectSettings/", script, StringComparison.Ordinal);
        Assert.Contains(".asset", script, StringComparison.Ordinal);
        Assert.Contains("git restore -- $ProjectVersionPath", script, StringComparison.Ordinal);
        Assert.Contains("\".cs\"", script, StringComparison.Ordinal);
        Assert.Contains("\".json\"", script, StringComparison.Ordinal);
        Assert.Contains("\".md\"", script, StringComparison.Ordinal);
        Assert.Contains("\".unity\"", script, StringComparison.Ordinal);
        Assert.Contains("\".prefab\"", script, StringComparison.Ordinal);
        Assert.DoesNotContain("git clean -fd -- unity/LLMGameCreatorAlpha/Assets", script, StringComparison.Ordinal);
        Assert.Contains("clean-unity-editor-noise.ps1", cmd, StringComparison.Ordinal);
        Assert.Contains("-Apply", cmd, StringComparison.Ordinal);
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
