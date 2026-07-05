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

    [Fact]
    public void MaterialWarningHotfixSourceScanRejectsMaterialInstantiatingAccessors()
    {
        var result = new AcceptedAlphaUnityMaterialWarningHotfixService()
            .Build(ProjectRoot());

        Assert.True(result.ScriptScan.Passed);
        Assert.True(result.ScriptScan.RendererMaterialAccessAbsent);
        Assert.True(result.ScriptScan.MaterialAssignmentAbsent);
        Assert.True(result.ScriptScan.MaterialPropertyBlockUsed);
        Assert.True(result.ScriptScan.ColorPropertySet);
        Assert.True(result.ScriptScan.BaseColorPropertySet);
        Assert.True(result.ScriptScan.NoNewMaterialInPrimitiveFactory);
        Assert.All(result.ScriptScan.Files, file =>
        {
            Assert.True(file.Exists);
            Assert.False(file.ContainsRendererMaterialAccess);
            Assert.False(file.ContainsMaterialAssignment);
        });
        Assert.True(result.NegativeProof.Passed);
    }

    [Fact]
    public void Goal120UsabilitySourceScanFindsLegendDescriptorsSelectionAndCleanupContract()
    {
        var result = new AcceptedAlphaProjectionUsabilityService()
            .Build(ProjectRoot());

        Assert.Equal("GREEN", result.Dashboard.UsabilityStatus);
        Assert.True(result.Dashboard.Goal119ARemainsGreen);
        Assert.True(result.Dashboard.LegendPresent);
        Assert.True(result.Dashboard.MarkerDescriptorPresent);
        Assert.True(result.Dashboard.SelectionControlsPresent);
        Assert.True(result.Dashboard.FocusCameraControlPresent);
        Assert.True(result.Dashboard.MaterialWarningGuardPresent);
        Assert.True(result.Dashboard.CleanupScriptContractPassed);
        Assert.True(result.Dashboard.DoNotStartAutomatically);
        Assert.True(result.ScriptInventory.Passed);
        Assert.True(result.ScriptInventory.BatchmodeMethodPresent);
        Assert.True(result.ScriptInventory.BatchmodePassMarkerPresent);
        Assert.True(result.CleanupScriptScan.Passed);
        Assert.True(result.SmokePlan.StepCount >= 8);
        Assert.True(result.NegativeProof.Passed);
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
