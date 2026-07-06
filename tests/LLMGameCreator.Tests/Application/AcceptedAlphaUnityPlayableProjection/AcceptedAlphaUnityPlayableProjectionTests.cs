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

    [Fact]
    public void Goal123GenericPackageProjectionSourceScanFindsAdapterWindowBatchmodeAndSamplePackage()
    {
        var result = new GenericGamePackageProjectionService()
            .Build(ProjectRoot());

        Assert.Equal("GREEN", result.Dashboard.GenericProjectionStatus);
        Assert.Equal(
            GenericGamePackageProjectionVocabulary.SamplePackagePath,
            result.Dashboard.SamplePackagePath);
        Assert.Equal("game/minimal-map-game", result.Dashboard.PackageId);
        Assert.Equal("Minimal Map Game", result.Dashboard.PackageTitle);
        Assert.Equal("map/village", result.Dashboard.MapId);
        Assert.Equal("12x8", result.Dashboard.MapSize);
        Assert.True(result.Dashboard.EntityCount >= 2);
        Assert.True(result.Dashboard.ItemCount >= 1);
        Assert.True(result.Dashboard.Goal122StillGreen);
        Assert.True(result.Dashboard.CleanupScriptAvailable);
        Assert.NotEqual(
            "BLOCKED_UNITY_BATCHMODE_GENERIC_PACKAGE_PROJECTION",
            result.Dashboard.UnitySmokeStatus);
        Assert.True(result.SamplePackage.Exists);
        Assert.True(result.SamplePackage.Parsed);
        Assert.True(result.SamplePackage.ReadOnlySource);
        Assert.True(result.SamplePackage.ExcludedFromExpectedChangedPaths);
        Assert.True(result.SamplePackage.WallTilePresent);
        Assert.True(result.SamplePackage.RoadTilePresent);
        Assert.True(result.SamplePackage.InteractableEntityCount >= 1);
        Assert.False(string.IsNullOrWhiteSpace(result.SamplePackage.Sha256));
        Assert.True(result.ScriptInventory.Passed);
        Assert.True(result.ScriptInventory.WindowActionPresent);
        Assert.True(result.ScriptInventory.BatchmodeMethodPresent);
        Assert.True(result.ScriptInventory.BatchmodePassMarkerPresent);
        Assert.True(result.ScriptInventory.BatchmodeFailMarkerPresent);
        Assert.True(result.ScriptInventory.AdapterReadsSamplePackage);
        Assert.True(result.ScriptInventory.ControllerBuildsGenericSection);
        Assert.True(result.ScriptInventory.ControllerVerifiesRequiredMarkers);
        Assert.True(result.ScriptInventory.ModelsExposeSmokeFields);
        Assert.True(result.ScriptInventory.ExistingGoal122VerificationStillPresent);
        Assert.True(result.ScriptInventory.MarkerDescriptorCompatible);
        Assert.True(result.ScriptInventory.NoSourceWriteMarkers);
        Assert.True(result.SmokePlan.StepCount >= 10);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.NegativeProof.SamplePackageMutationRejected);
    }

    [Fact]
    public void Goal124GenericPackageLoopSourceScanFindsStateLoopWindowBatchmodeAndSampleQuest()
    {
        var result = new GenericGamePackageLoopProjectionService()
            .Build(ProjectRoot());

        Assert.Equal("GREEN", result.Dashboard.GenericLoopStatus);
        Assert.Equal(
            GenericGamePackageLoopProjectionVocabulary.SamplePackagePath,
            result.Dashboard.SamplePackagePath);
        Assert.Equal("game/minimal-map-game", result.Dashboard.PackageId);
        Assert.Equal("map/village", result.Dashboard.MapId);
        Assert.True(result.Dashboard.InteractionPreviewPresent);
        Assert.True(result.Dashboard.InteractionApplyPassed);
        Assert.True(result.Dashboard.DialogueSummaryPresent);
        Assert.True(result.Dashboard.QuestObjectiveSummaryPresent);
        Assert.True(result.Dashboard.InventorySummaryPresent);
        Assert.True(result.Dashboard.ResourceSummaryPresent);
        Assert.True(result.Dashboard.Goal123StillGreen);
        Assert.True(result.Dashboard.CleanupScriptAvailable);
        Assert.True(result.Dashboard.ProjectionOnly);
        Assert.Equal(1, result.Dashboard.AppliedInteractionCount);
        Assert.Equal(1, result.Dashboard.StartedQuestCount);
        Assert.NotEqual(
            "BLOCKED_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_LOOP",
            result.Dashboard.UnitySmokeStatus);
        Assert.True(result.SamplePackage.Exists);
        Assert.True(result.SamplePackage.Parsed);
        Assert.True(result.SamplePackage.ReadOnlySource);
        Assert.True(result.SamplePackage.ExcludedFromExpectedChangedPaths);
        Assert.True(result.SamplePackage.SignEntityPresent);
        Assert.True(result.SamplePackage.SignInspectInteractionPresent);
        Assert.True(result.SamplePackage.SignInspectSetFlagEffectPresent);
        Assert.True(result.SamplePackage.SignInspectLogEffectPresent);
        Assert.True(result.SamplePackage.OldGuardEntityPresent);
        Assert.True(result.SamplePackage.OldGuardDialoguePresent);
        Assert.True(result.SamplePackage.HelpHealerQuestPresent);
        Assert.Equal(3, result.SamplePackage.RequiredRedHerbAmount);
        Assert.Equal(2, result.SamplePackage.PlayerRedHerbAmount);
        Assert.True(result.SamplePackage.HelpHealerIncomplete);
        Assert.True(result.ScriptInventory.Passed);
        Assert.True(result.ScriptInventory.WindowActionPresent);
        Assert.True(result.ScriptInventory.BatchmodeMethodPresent);
        Assert.True(result.ScriptInventory.BatchmodePassMarkerPresent);
        Assert.True(result.ScriptInventory.BatchmodeFailMarkerPresent);
        Assert.True(result.ScriptInventory.StateClassTracksRequiredFields);
        Assert.True(result.ScriptInventory.LoopRunsRequiredSequence);
        Assert.True(result.ScriptInventory.ControllerRendersLoopMarkers);
        Assert.True(result.ScriptInventory.AdapterParsesLoopData);
        Assert.True(result.ScriptInventory.ModelsExposeLoopSmokeFields);
        Assert.True(result.ScriptInventory.ExistingGoal123VerificationStillPresent);
        Assert.True(result.ScriptInventory.NoSourceWriteMarkers);
        Assert.True(result.SmokePlan.StepCount >= 12);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.NegativeProof.ManualInputRejected);
        Assert.True(result.NegativeProof.SamplePackageMutationRejected);
    }

    [Fact]
    public void Goal125GenericPackageSystemsSourceScanFindsProjectionOnlySystemsLoop()
    {
        var result = new GenericGamePackageSystemsProjectionService()
            .Build(ProjectRoot());

        Assert.Equal("GREEN", result.Dashboard.GenericSystemsStatus);
        Assert.Equal(
            GenericGamePackageSystemsProjectionVocabulary.SamplePackagePath,
            result.Dashboard.SamplePackagePath);
        Assert.Equal("game/minimal-map-game", result.Dashboard.PackageId);
        Assert.True(result.Dashboard.RecipePreviewPresent);
        Assert.True(result.Dashboard.RecipeApplyPassed);
        Assert.True(result.Dashboard.HarvestPreviewPresent);
        Assert.True(result.Dashboard.HarvestApplyPassed);
        Assert.True(result.Dashboard.TransactionPreviewPresent);
        Assert.True(result.Dashboard.EncounterPreviewPresent);
        Assert.True(result.Dashboard.CombatRoundPreviewPresent);
        Assert.True(result.Dashboard.InventorySummaryPresent);
        Assert.True(result.Dashboard.ResourceSummaryPresent);
        Assert.True(result.Dashboard.SystemsEventLogPresent);
        Assert.True(result.Dashboard.Goal124StillGreen);
        Assert.True(result.Dashboard.CleanupScriptAvailable);
        Assert.True(result.Dashboard.ProjectionOnly);
        Assert.True(result.Dashboard.SamplePackageReadOnly);
        Assert.True(result.Dashboard.NoRuntimeProviderSchemaLuaGeneratorLibrary);
        Assert.True(result.Dashboard.NoUnityScenePrefabSettingsPackagesStreamingAssets);
        Assert.NotEqual(
            "BLOCKED_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_SYSTEMS",
            result.Dashboard.UnitySmokeStatus);
        Assert.True(result.SamplePackage.Exists);
        Assert.True(result.SamplePackage.Parsed);
        Assert.True(result.SamplePackage.ReadOnlySource);
        Assert.True(result.SamplePackage.ExcludedFromExpectedChangedPaths);
        Assert.True(result.SamplePackage.PlayerInventoryPresent);
        Assert.True(result.SamplePackage.ResourceDefaultsPresent);
        Assert.True(result.SamplePackage.RecipeHealingPotionPresent);
        Assert.True(result.SamplePackage.RecipeRequirementsMatchExpected);
        Assert.True(result.SamplePackage.HarvestNodePresent);
        Assert.True(result.SamplePackage.HarvestLootPresent);
        Assert.True(result.SamplePackage.TransactionPresent);
        Assert.True(result.SamplePackage.EncounterPresent);
        Assert.True(result.SamplePackage.CombatRoundMatchesExpected);
        Assert.True(result.ScriptInventory.Passed);
        Assert.True(result.ScriptInventory.WindowActionPresent);
        Assert.True(result.ScriptInventory.BatchmodeMethodPresent);
        Assert.True(result.ScriptInventory.BatchmodePassMarkerPresent);
        Assert.True(result.ScriptInventory.BatchmodeFailMarkerPresent);
        Assert.True(result.ScriptInventory.StateClassTracksRequiredFields);
        Assert.True(result.ScriptInventory.SystemsLoopRunsRequiredSequence);
        Assert.True(result.ScriptInventory.ControllerRendersSystemsMarkers);
        Assert.True(result.ScriptInventory.AdapterParsesSystemsData);
        Assert.True(result.ScriptInventory.ModelsExposeSystemsSmokeFields);
        Assert.True(result.ScriptInventory.ExistingGoal124VerificationStillPresent);
        Assert.True(result.ScriptInventory.NoSourceWriteMarkers);
        Assert.True(result.SmokePlan.StepCount >= 13);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.NegativeProof.ManualInputRejected);
        Assert.True(result.NegativeProof.SamplePackageMutationRejected);
        Assert.True(result.NegativeProof.RuntimeSchemaProviderLuaGeneratorLibraryRejected);
        Assert.True(result.NegativeProof.UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected);
    }

    [Fact]
    public void Goal126GenericPackageFullPlaythroughSourceScanFindsOneClickBatchmodeAndTranscript()
    {
        var result = new GenericGamePackageFullPlaythroughProjectionService()
            .Build(ProjectRoot());

        Assert.Equal("GREEN", result.Dashboard.FullPlaythroughStatus);
        Assert.Equal(
            GenericGamePackageFullPlaythroughProjectionVocabulary.SamplePackagePath,
            result.Dashboard.SamplePackagePath);
        Assert.Equal("game/minimal-map-game", result.Dashboard.PackageId);
        Assert.Equal("Minimal Map Game", result.Dashboard.PackageTitle);
        Assert.Equal("map/village", result.Dashboard.MapId);
        Assert.True(result.Dashboard.MapPathPreviewPresent);
        Assert.True(result.Dashboard.SignInteractionApplied);
        Assert.True(result.Dashboard.DialogueSummaryPresent);
        Assert.True(result.Dashboard.QuestObjectiveStatusPresent);
        Assert.True(result.Dashboard.InventorySummaryPresent);
        Assert.True(result.Dashboard.ResourceSummaryPresent);
        Assert.True(result.Dashboard.SystemsSummaryPresent);
        Assert.True(result.Dashboard.RecipeApplyPassed);
        Assert.True(result.Dashboard.HarvestApplyPassed);
        Assert.True(result.Dashboard.TransactionPreviewPresent);
        Assert.True(result.Dashboard.CombatRoundPreviewPresent);
        Assert.True(result.Dashboard.EventTranscriptPresent);
        Assert.True(result.Dashboard.Goal125StillGreen);
        Assert.True(result.Dashboard.CleanupScriptAvailable);
        Assert.True(result.Dashboard.ProjectionOnly);
        Assert.True(result.Dashboard.SamplePackageReadOnly);
        Assert.True(result.Dashboard.NoRuntimeProviderSchemaLuaGeneratorLibrary);
        Assert.True(result.Dashboard.NoUnityScenePrefabSettingsPackagesStreamingAssets);
        Assert.NotEqual(
            "BLOCKED_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH",
            result.Dashboard.UnitySmokeStatus);
        Assert.True(result.SamplePackage.Exists);
        Assert.True(result.SamplePackage.Parsed);
        Assert.True(result.SamplePackage.ReadOnlySource);
        Assert.True(result.SamplePackage.ExcludedFromExpectedChangedPaths);
        Assert.True(result.SamplePackage.StartPositionPresent);
        Assert.True(result.SamplePackage.PathTargetPresent);
        Assert.True(result.SamplePackage.PathWalkable);
        Assert.True(result.SamplePackage.SignInteractionPresent);
        Assert.True(result.SamplePackage.OldGuardDialoguePresent);
        Assert.True(result.SamplePackage.HelpHealerQuestIncomplete);
        Assert.True(result.SamplePackage.PlayerInventoryPresent);
        Assert.True(result.SamplePackage.ResourceDefaultsPresent);
        Assert.True(result.SamplePackage.RecipeRequirementsMatchExpected);
        Assert.True(result.SamplePackage.HarvestContractPresent);
        Assert.True(result.SamplePackage.TransactionPresent);
        Assert.True(result.SamplePackage.CombatRoundMatchesExpected);
        Assert.True(result.ScriptInventory.Passed);
        Assert.True(result.ScriptInventory.WindowActionPresent);
        Assert.True(result.ScriptInventory.BatchmodeMethodPresent);
        Assert.True(result.ScriptInventory.BatchmodePassMarkerPresent);
        Assert.True(result.ScriptInventory.BatchmodeFailMarkerPresent);
        Assert.True(result.ScriptInventory.StateClassTracksFullPlaythroughFields);
        Assert.True(result.ScriptInventory.PlaythroughRunsRequiredSequence);
        Assert.True(result.ScriptInventory.ControllerRendersFullPlaythroughMarkers);
        Assert.True(result.ScriptInventory.ModelsExposeFullPlaythroughSmokeFields);
        Assert.True(result.ScriptInventory.ExistingGoal125VerificationStillPresent);
        Assert.True(result.ScriptInventory.NoSourceWriteMarkers);
        Assert.True(result.SmokePlan.StepCount >= 15);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.NegativeProof.ManualInputRejected);
        Assert.True(result.NegativeProof.SamplePackageMutationRejected);
        Assert.True(result.NegativeProof.RuntimeSchemaProviderLuaGeneratorLibraryRejected);
        Assert.True(result.NegativeProof.UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected);
    }

    [Fact]
    public void Goal127UnityProjectionVerificationRunnerScansScriptsAndGoal126Evidence()
    {
        var result = new UnityProjectionVerificationRunnerService()
            .Build(ProjectRoot());

        Assert.True(result.Goal126Evidence.Passed);
        Assert.True(result.ScriptScan.Passed);
        Assert.True(result.ScriptScan.RunnerScriptExists);
        Assert.True(result.ScriptScan.RunnerCmdExists);
        Assert.True(result.ScriptScan.SupportsGenericFullPlaythroughMode);
        Assert.True(result.ScriptScan.SupportsUnityPath);
        Assert.True(result.ScriptScan.SupportsDryRun);
        Assert.True(result.ScriptScan.SupportsApplyCleanup);
        Assert.True(result.ScriptScan.ExecuteMethodPresent);
        Assert.True(result.ScriptScan.PassMarkerScanPresent);
        Assert.True(result.ScriptScan.FailMarkerScanPresent);
        Assert.True(result.ScriptScan.MaterialWarningScanPresent);
        Assert.True(result.ScriptScan.CleanupDelegatesToBoundedScript);
        Assert.True(result.ScriptScan.CmdWrapperUsesApplyCleanup);
        Assert.True(result.ScriptScan.NoBroadGitClean);
        Assert.True(result.ScriptScan.NoForbiddenMutationTargets);
        Assert.True(result.ScriptScan.WritesRequiredResultJsonFields);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.Dashboard.CleanupScriptAvailable);
        Assert.False(result.Dashboard.ManualUnityClickingRequired);
        Assert.Equal(
            ".devflow\\scripts\\run-unity-projection-verification.cmd",
            result.Dashboard.RunnerCommand);
        Assert.Equal(
            UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeExecuteMethod,
            result.Dashboard.UnityExecuteMethod);
        if (result.ResultScan.ResultExists)
        {
            Assert.True(result.ResultScan.RequiredFieldsPresent);
            Assert.Equal(UnityProjectionVerificationRunnerVocabulary.Mode, result.ResultScan.Mode);
            Assert.Equal(
                UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeLogRelativePath,
                result.ResultScan.LogPath);
        }
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
