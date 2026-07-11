using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunSelectedRuntimeVariantLiveSessionScriptTests
{
    [Fact]
    public void ScriptHasRequiredParametersGuardsRollbackAndUnitySmoke()
    {
        var source = File.ReadAllText(Path.Combine(ProjectRoot(),
            ".devflow/scripts/run-selected-runtime-variant-live-session.ps1"));
        foreach (var marker in new[] { "SelectedHandoffPath", "SelectedPackagePath", "SelectedOutcomePath", "Goal143HandoffPath", "OutputRoot", "UnityPath", "DryRun", "ApplyCleanup", ".llmgc/manual/", "Restore-Goal144Directory", "RunBatchmodeSelectedRuntimeVariantLiveSessionSmoke" })
            Assert.Contains(marker, source, StringComparison.Ordinal);
        Assert.Contains("actionDescriptorExecutionBindingPassed", source, StringComparison.Ordinal);
        Assert.Contains("checkpointReplayedActionCount=8", source, StringComparison.Ordinal);
        Assert.Contains("finalReplayActionCount=13", source, StringComparison.Ordinal);
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }
}
