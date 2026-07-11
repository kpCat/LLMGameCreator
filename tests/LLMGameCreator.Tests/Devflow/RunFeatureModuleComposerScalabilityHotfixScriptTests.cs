using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunFeatureModuleComposerScalabilityHotfixScriptTests
{
    [Fact]
    public void Script_has_bounded_paths_required_proofs_unity_reuse_and_transactional_rollback()
    {
        var root = FindRoot();
        var source = File.ReadAllText(Path.Combine(root, ".devflow", "scripts",
            "run-featuremodule-composer-scalability-hotfix.ps1"));
        Assert.Contains("[string]$OutputRoot", source, StringComparison.Ordinal);
        Assert.Contains("[string]$UnityPath", source, StringComparison.Ordinal);
        Assert.Contains("[switch]$DryRun", source, StringComparison.Ordinal);
        Assert.Contains("[switch]$ApplyCleanup", source, StringComparison.Ordinal);
        Assert.Contains("Goal146A refuses .llmgc/manual", source, StringComparison.Ordinal);
        Assert.Contains("run-featuremodule-composition-runtime-matrix.ps1", source, StringComparison.Ordinal);
        Assert.Contains("FeatureModuleComposerScalabilityScriptProof", source, StringComparison.Ordinal);
        Assert.Contains("Restore-Goal146ADirectory", source, StringComparison.Ordinal);
        Assert.Contains("GOAL146A_FEATUREMODULE_COMPOSER_SCALABILITY_GREEN", source, StringComparison.Ordinal);
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
