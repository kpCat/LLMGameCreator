using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

public sealed class FeatureModuleCompositionScriptProof
{
    [Fact]
    public async Task Run_goal146_application_matrix_when_requested_by_normal_script()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL146_RUN"), "true", StringComparison.OrdinalIgnoreCase)) return;
        var root = FindRoot();
        var selectedText = Environment.GetEnvironmentVariable("LLMGC_GOAL146_SELECTED_MODULE_IDS") ?? string.Empty;
        IReadOnlyList<string>? selected = string.IsNullOrWhiteSpace(selectedText)
            ? null
            : selectedText.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var result = await new FeatureModuleCompositionService(SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
            .RunAndWriteAsync(root, new FeatureModuleCompositionRunRequest
            {
                Goal142Root = Environment.GetEnvironmentVariable("LLMGC_GOAL146_GOAL142_ROOT")
                              ?? FeatureModuleCompositionVocabulary.Goal142Root,
                OutputRoot = Environment.GetEnvironmentVariable("LLMGC_GOAL146_OUTPUT_ROOT")
                             ?? FeatureModuleCompositionVocabulary.ProceduralRoot,
                SelectedModuleIds = selected,
                CompositionId = Environment.GetEnvironmentVariable("LLMGC_GOAL146_COMPOSITION_ID") ?? string.Empty,
                UnitySmokePath = Environment.GetEnvironmentVariable("LLMGC_GOAL146_UNITY_SMOKE_PATH")
                                 ?? FeatureModuleCompositionVocabulary.ProceduralRoot + "/unity-featuremodule-composition-matrix-smoke.json"
            });

        Assert.Equal(8, result.Matrix.PassedCompositionCount);
        Assert.Equal(8, result.Matrix.DistinctPackageSha256Count);
        Assert.Equal(8, result.Matrix.DistinctFinalStateHashCount);
        Assert.True(result.NegativeProof.Passed);
        if (string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL146_REQUIRE_UNITY_SMOKE"), "true", StringComparison.OrdinalIgnoreCase))
        {
            Assert.True(result.UnitySmoke.Passed);
            Assert.Equal("GREEN", result.Dashboard.Status);
        }
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
