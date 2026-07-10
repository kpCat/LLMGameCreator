using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.SelectedRuntimeVariantInteractiveSession;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

public sealed class SelectedRuntimeVariantInteractiveSessionScriptProof
{
    [Fact]
    public async Task WritesGoal144LiveSessionArtifacts()
    {
        var request = new SelectedRuntimeVariantInteractiveSessionRequest
        {
            SelectedHandoffPath = Env("LLMGC_GOAL144_SELECTED_HANDOFF_PATH",
                SelectedRuntimeVariantInteractiveSessionVocabulary.SelectedHandoffPath),
            SelectedPackagePath = Env("LLMGC_GOAL144_SELECTED_PACKAGE_PATH",
                SelectedRuntimeVariantInteractiveSessionVocabulary.SelectedPackagePath),
            SelectedOutcomePath = Env("LLMGC_GOAL144_SELECTED_OUTCOME_PATH",
                SelectedRuntimeVariantInteractiveSessionVocabulary.SelectedOutcomePath),
            Goal143HandoffPath = Env("LLMGC_GOAL144_GOAL143_HANDOFF_PATH",
                SelectedRuntimeVariantInteractiveSessionVocabulary.Goal143HandoffPath),
            OutputRoot = Env("LLMGC_GOAL144_OUTPUT_ROOT",
                SelectedRuntimeVariantInteractiveSessionVocabulary.ProceduralOutputDirectory),
            UnitySmokePath = Env("LLMGC_GOAL144_UNITY_SMOKE_PATH",
                SelectedRuntimeVariantInteractiveSessionVocabulary.UnitySmokeRelativePath)
        };
        var write = await new SelectedRuntimeVariantInteractiveSessionService(
                LLMGameCreator.Runtime.SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
            .RunDrillAndWriteAsync(ProjectRoot(), request);
        var dashboard = write.Artifacts.Dashboard;
        Assert.True(dashboard.SelectedRuntimeVariantInteractiveSession);
        Assert.False(dashboard.Accepted);
        Assert.True(dashboard.ActionDescriptorCount >= 10);
        Assert.True(dashboard.RuntimeRoutedActionDescriptorCount >= 8);
        Assert.True(dashboard.PresentationOnlyActionDescriptorCount >= 2);
        Assert.True(dashboard.ExecutedRuntimeActionCount >= 8);
        Assert.True(dashboard.InvalidActionStateUnchanged);
        Assert.True(dashboard.CheckpointReloadByReplayPassed);
        Assert.True(dashboard.CheckpointStateHashRestored);
        Assert.True(dashboard.FullReplayEquivalent);
        Assert.True(dashboard.FinalStateHashMatchesGoal142);
        Assert.True(dashboard.SelectedVariantEffectVisible);
        Assert.True(write.Artifacts.NegativeProof.Passed);
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        if (Environment.GetEnvironmentVariable("LLMGC_GOAL144_REQUIRE_UNITY_SMOKE") == "true")
        {
            Assert.Equal("GREEN", dashboard.Status);
            Assert.True(write.Artifacts.UnitySmoke.Passed);
        }
    }

    private static string Env(string name, string fallback) =>
        string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name))
            ? fallback
            : Environment.GetEnvironmentVariable(name)!;

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
