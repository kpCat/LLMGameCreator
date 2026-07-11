using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

public sealed class FeatureModuleAuthoringScriptProof
{
    [Fact]
    public async Task Run_goal147_featuremodule_authoring_proof_when_requested()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL147_RUN"), "true", StringComparison.OrdinalIgnoreCase))
            return;
        var root = FindRoot();
        var request = new FeatureModuleAuthoringRunRequest
        {
            CatalogRoot = Required("LLMGC_GOAL147_CATALOG_ROOT"),
            WorkspaceRoot = Required("LLMGC_GOAL147_WORKSPACE_ROOT"),
            CertificationCacheRoot = Required("LLMGC_GOAL147_CACHE_ROOT"),
            OutputRoot = Required("LLMGC_GOAL147_OUTPUT_ROOT"),
            CompositionId = Required("LLMGC_GOAL147_COMPOSITION_ID"),
            UnitySmokePath = Required("LLMGC_GOAL147_UNITY_SMOKE_PATH")
        };
        var result = await new FeatureModuleAuthoringProofService(
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
            .RunAndWriteAsync(root, request);
        var requireUnity = string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL147_REQUIRE_UNITY_SMOKE"),
            "true", StringComparison.OrdinalIgnoreCase);

        Assert.Equal(requireUnity ? "GREEN" : "READY_FOR_UNITY_SMOKE", result.Dashboard.Status);
        Assert.True(result.Library.Validation.Passed);
        Assert.Equal(10, result.Dashboard.RequiredCoreModuleCount);
        Assert.Equal(3, result.Dashboard.OptionalModuleCount);
        Assert.True(result.Dashboard.ParameterDefinitionCount >= 8);
        Assert.True(result.Dashboard.DefaultParametersPreserveGoal146Hashes);
        Assert.True(result.Dashboard.SavedCompositionRoundtripPassed);
        Assert.True(result.Dashboard.AllOptionalModulesCertified);
        Assert.True(result.Dashboard.UnchangedCertificationCacheReusePassed);
        Assert.True(result.Dashboard.ChangedModuleSelectiveInvalidationPassed);
        Assert.True(result.Dashboard.HundredModuleCatalogAccepted);
        Assert.True(result.Dashboard.HundredModuleInteractionRowCount <= 24);
        Assert.False(result.Dashboard.HundredModulePowersetEnumerated);
        Assert.True(result.Dashboard.MultiEffectModuleAccountingPassed);
        Assert.True(result.Dashboard.CustomParameterizedCompositionPassed);
        Assert.True(result.NegativeProof.Values.All(value => value));
        Assert.Equal(requireUnity, result.UnitySmoke.Passed);
        Assert.False(result.Dashboard.Goal146Accepted);
        Assert.False(result.Dashboard.Goal147Accepted);
        Assert.False(result.Dashboard.Accepted);
    }

    private static string Required(string name) => Environment.GetEnvironmentVariable(name)
        ?? throw new InvalidOperationException(name + " is required.");

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
