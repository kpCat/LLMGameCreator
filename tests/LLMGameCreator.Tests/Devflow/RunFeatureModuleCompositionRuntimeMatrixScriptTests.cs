using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunFeatureModuleCompositionRuntimeMatrixScriptTests
{
    [Fact]
    public void Script_has_bounded_paths_default_none_semantics_unity_smoke_and_transactional_rollback()
    {
        var root = FindRoot();
        var source = File.ReadAllText(Path.Combine(root, ".devflow", "scripts",
            "run-featuremodule-composition-runtime-matrix.ps1"));
        Assert.Contains("[string]$Goal142Root", source, StringComparison.Ordinal);
        Assert.Contains("[string]$OutputRoot", source, StringComparison.Ordinal);
        Assert.Contains("[string]$SelectedModuleIds", source, StringComparison.Ordinal);
        Assert.Contains("[string]$CompositionId", source, StringComparison.Ordinal);
        Assert.Contains("[switch]$DryRun", source, StringComparison.Ordinal);
        Assert.Contains("[switch]$ApplyCleanup", source, StringComparison.Ordinal);
        Assert.Contains("SelectedModuleIds -eq \"none\"", source, StringComparison.Ordinal);
        Assert.Contains("Goal146 refuses .llmgc/manual", source, StringComparison.Ordinal);
        Assert.Contains("FeatureModuleCompositionScriptProof", source, StringComparison.Ordinal);
        Assert.Contains("RunBatchmodeFeatureModuleCompositionMatrixSmoke", source, StringComparison.Ordinal);
        Assert.Contains("Restore-Goal146Directory", source, StringComparison.Ordinal);
        Assert.Contains("GOAL146_FEATUREMODULE_COMPOSITION_MATRIX_GREEN", source, StringComparison.Ordinal);
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
