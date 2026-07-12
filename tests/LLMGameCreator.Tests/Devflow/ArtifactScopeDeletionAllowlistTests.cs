using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class ArtifactScopeDeletionAllowlistTests
{
    [Fact]
    public void Deletion_allowlists_explicitly_accept_empty_collections_without_weakening_the_policy()
    {
        var root = FindRoot();
        var script = File.ReadAllText(Path.Combine(root, ".devflow", "scripts", "check-artifact-scope.ps1"));
        Assert.Contains("[AllowEmptyCollection()][string[]]$DeletedExactAllowed", script, StringComparison.Ordinal);
        Assert.Contains("[AllowEmptyCollection()][string[]]$DeletedPrefixAllowed", script, StringComparison.Ordinal);
        Assert.Contains("allowed_declared_diagnostic_deletion", script, StringComparison.Ordinal);
        Assert.Contains("Test-ExactAllowed -Path $path -Allowed $DeletedExactAllowed", script, StringComparison.Ordinal);
        Assert.Contains("Test-PrefixAllowed -Path $path -Allowed $DeletedPrefixAllowed", script, StringComparison.Ordinal);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
