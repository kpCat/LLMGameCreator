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
        Assert.Equal(7, result.ScriptInventory.ScriptCount);
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

    [Fact]
    public void Goal121FullVerificationSourceScanFindsOneClickDrilldownBatchmodeAndSmokeFields()
    {
        var result = new AcceptedAlphaInteractionDrilldownVerificationService()
            .Build(ProjectRoot());

        Assert.Equal("GREEN", result.Dashboard.FullVerificationStatus);
        Assert.True(result.Dashboard.OneClickButtonPresent);
        Assert.True(result.Dashboard.DrilldownFieldsPresent);
        Assert.True(result.Dashboard.InteractionPreviewPresent);
        Assert.True(result.Dashboard.ObjectiveReplayDetailsPresent);
        Assert.Equal(
            "GOAL121_FULL_PROJECTION_VERIFICATION_PASS",
            result.Dashboard.BatchmodeFullVerificationMarker);
        Assert.True(result.Dashboard.CleanupScriptAvailable);
        Assert.True(result.Dashboard.MaterialWarningGuardPresent);
        Assert.True(result.Dashboard.HumanManualStepsReducedToOneButton);
        Assert.NotEqual(
            "BLOCKED_UNITY_BATCHMODE_FULL_VERIFICATION",
            result.Dashboard.UnityBatchmodeLogStatus);
        Assert.True(result.ScriptInventory.Passed);
        Assert.True(result.ScriptInventory.BatchmodeMethodPresent);
        Assert.True(result.ScriptInventory.BatchmodePassMarkerPresent);
        Assert.True(result.ScriptInventory.BatchmodeFailMarkerPresent);
        Assert.True(result.ScriptInventory.DrilldownFieldsPresent);
        Assert.True(result.ScriptInventory.InteractionPreviewFieldsPresent);
        Assert.True(result.ScriptInventory.ObjectiveReplayDetailsFieldsPresent);
        Assert.True(result.ScriptInventory.VerificationEventLogPresent);
        Assert.True(result.ScriptInventory.SmokeRequiredFieldsPresent);
        Assert.True(result.ScriptInventory.MaterialWarningSourceClean);
        Assert.True(result.SmokePlan.OneClickManualPath);
        Assert.True(result.SmokePlan.StepCount >= 12);
        Assert.True(result.NegativeProof.Passed);
    }

    [Fact]
    public void Goal122ActionLoopSourceScanFindsProjectionStateWindowPolishBatchmodeAndSmokeFields()
    {
        var result = new AcceptedAlphaProjectionActionLoopService()
            .Build(ProjectRoot());

        Assert.Equal("GREEN", result.Dashboard.ActionLoopStatus);
        Assert.Equal("GREEN", result.Dashboard.WindowPolishStatus);
        Assert.True(result.Dashboard.Goal121StillGreen);
        Assert.True(result.Dashboard.OneClickVerificationStillPresent);
        Assert.True(result.Dashboard.ProjectionActionPreviewPresent);
        Assert.True(result.Dashboard.ProjectionActionApplyPresent);
        Assert.True(result.Dashboard.ProjectionStateResetPresent);
        Assert.True(result.Dashboard.WindowLayoutPolishPresent);
        Assert.True(result.Dashboard.CleanupScriptAvailable);
        Assert.True(result.Dashboard.MaterialWarningGuardPresent);
        Assert.NotEqual(
            "BLOCKED_UNITY_BATCHMODE_ACTION_LOOP_SMOKE",
            result.Dashboard.UnitySmokeStatus);
        Assert.True(result.ScriptInventory.Passed);
        Assert.True(result.ScriptInventory.BatchmodeActionLoopMethodPresent);
        Assert.True(result.ScriptInventory.BatchmodePassMarkerPresent);
        Assert.True(result.ScriptInventory.BatchmodeFailMarkerPresent);
        Assert.True(result.ScriptInventory.ActionLoopControlsPresent);
        Assert.True(result.ScriptInventory.ProjectionStateModelPresent);
        Assert.True(result.ScriptInventory.SmokeRequiredFieldsPresent);
        Assert.True(result.ScriptInventory.MaterialWarningSourceClean);
        Assert.True(result.SmokePlan.StepCount >= 9);
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
