using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

public sealed class AcceptedAlphaUnityPlayableProjectionTests
{
    [Fact]
    public void RepositoryProjectionBuildIsGreenAndUsesAcceptedBaseline()
    {
        var result = new AcceptedAlphaUnityPlayableProjectionService()
            .Build(ProjectRoot());

        Assert.Equal("GREEN", result.QualityGateScan.ImplementationStatus);
        Assert.True(result.QualityGateScan.Passed);
        Assert.Equal("GREEN", result.Dashboard.ProjectionStatus);
        Assert.True(result.Dashboard.AcceptedBaselineReady);
        Assert.Equal(
            AcceptedAlphaUnityPlayableProjectionVocabulary.BaselineId,
            result.Dashboard.BaselineId);
        Assert.Equal(
            AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath,
            result.Dashboard.UnityMenuPath);
        Assert.Equal(
            AcceptedAlphaUnityPlayableProjectionVocabulary.GeneratedRootName,
            result.Dashboard.ExpectedGeneratedRootName);
        Assert.Equal(5, result.ScriptInventory.ScriptCount);
        Assert.True(result.ScriptInventory.AllScriptsPresent);
        Assert.True(result.ScriptInventory.MenuPathExistsExactly);
        Assert.True(result.SmokePlan.BaselineLoaded);
        Assert.True(result.SmokePlan.HasPlayerProxyStep);
        Assert.True(result.SmokePlan.HasChunkWindowStep);
        Assert.True(result.SmokePlan.HasInteractionOrObjectiveStep);
        Assert.True(result.SmokePlan.HasDiagnosticsStatusStep);
        Assert.True(result.NegativeProof.Passed);
    }

    [Fact]
    public void ProjectionScopeRejectsManualRuntimeSchemaProviderLuaAndUnityPayloadMutation()
    {
        var result = new AcceptedAlphaUnityPlayableProjectionService()
            .Build(ProjectRoot());

        Assert.True(result.NegativeProof.ManualInputRejected);
        Assert.True(result.NegativeProof.RuntimeSchemaProviderLuaGeneratorLibraryRejected);
        Assert.True(result.NegativeProof.UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected);
        Assert.True(result.NegativeProof.FinalReleasePackagingRejected);
        Assert.True(result.NegativeProof.LiveGeodataProviderNetworkRejected);
        Assert.True(result.QualityGateScan.ManualInputExcluded);
        Assert.True(result.QualityGateScan.NoProjectSettingsPackagesStreamingAssetsExpected);
        Assert.True(result.QualityGateScan.NoRuntimeSchemaProviderLuaGeneratorLibraryExpected);
        Assert.Contains(result.NegativeProof.RejectedPathSamples, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Contains(result.NegativeProof.RejectedPathSamples, path =>
            path.StartsWith("src/LLMGameCreator.Runtime/", StringComparison.Ordinal));
        Assert.Contains(result.NegativeProof.RejectedPathSamples, path =>
            path.Contains("StreamingAssets", StringComparison.Ordinal));
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
