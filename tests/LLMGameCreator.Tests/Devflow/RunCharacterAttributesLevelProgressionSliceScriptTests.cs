using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunCharacterAttributesLevelProgressionSliceScriptTests
{
    [Fact]
    public void Goal150_runner_is_bounded_transactional_and_requires_green_unaccepted_dashboard()
    {
        var root = FindRoot();
        var script = File.ReadAllText(Path.Combine(root, ".devflow", "scripts",
            "run-character-attributes-level-progression-slice.ps1"));
        Assert.Contains("FullyQualifiedName~Goal150ArtifactProof", script, StringComparison.Ordinal);
        Assert.Contains("GOAL150_CHARACTER_ATTRIBUTES_LEVEL_PROGRESSION_GREEN", script, StringComparison.Ordinal);
        Assert.Contains("goal150-dashboard.json", script, StringComparison.Ordinal);
        Assert.Contains("goal150Accepted", script, StringComparison.Ordinal);
        Assert.Contains("Restore-Goal150Directory", script, StringComparison.Ordinal);
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
