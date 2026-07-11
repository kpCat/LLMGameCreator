using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.FeatureModuleComposition;

public sealed class FeatureModuleComposerScalabilityScriptProof
{
    [Fact]
    public async Task Run_goal146a_scalability_proof_when_requested_by_normal_script()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL146A_RUN"), "true", StringComparison.OrdinalIgnoreCase)) return;
        var root = FindRoot();
        var output = Environment.GetEnvironmentVariable("LLMGC_GOAL146A_OUTPUT_ROOT")
                     ?? FeatureModuleComposerScalabilityProofService.ProceduralRoot;
        var result = await new FeatureModuleComposerScalabilityProofService(
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
            .RunAndWriteAsync(root, output);

        Assert.Equal("GREEN", result.Dashboard.Status);
        Assert.Equal(8, result.Dashboard.CurrentGeneratedCompositionCount);
        Assert.True(result.Dashboard.SyntheticFourthModulePassed);
        Assert.True(result.Dashboard.SyntheticFourthGeneratedCompositionCount < 16);
        Assert.False(result.Dashboard.SyntheticFourthFullPowersetEnumerated);
        Assert.True(result.Dashboard.LargeCatalogCoverageBounded);
        Assert.True(result.Dashboard.LargeCatalogCoverageDeterministic);
        Assert.True(result.CompatibilityProof.Passed);
        Assert.True(result.NegativeProof.Passed);
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
