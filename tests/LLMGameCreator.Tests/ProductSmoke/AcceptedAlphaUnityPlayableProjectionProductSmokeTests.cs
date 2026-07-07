using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class AcceptedAlphaUnityPlayableProjectionProductSmokeTests
{
    [Fact]
    public async Task ProductSmokeWritesProjectionArtifactsAndWorkspaceSurface()
    {
        var root = ProjectRoot();
        var projection = new AcceptedAlphaUnityPlayableProjectionService()
            .Build(root);
        var hotfix = new AcceptedAlphaUnityMaterialWarningHotfixService()
            .Build(root);
        var usability = new AcceptedAlphaProjectionUsabilityService()
            .Build(root);
        var drilldown = await new AcceptedAlphaInteractionDrilldownVerificationService()
            .BuildAndWriteAsync(root);
        var actionLoop = await new AcceptedAlphaProjectionActionLoopService()
            .BuildAndWriteAsync(root);
        var genericProjection = await new GenericGamePackageProjectionService()
            .BuildAndWriteAsync(root);
        var genericLoop = await new GenericGamePackageLoopProjectionService()
            .BuildAndWriteAsync(root);
        var genericSystems = await new GenericGamePackageSystemsProjectionService()
            .BuildAndWriteAsync(root);
        var genericFullPlaythrough = await new GenericGamePackageFullPlaythroughProjectionService()
            .BuildAndWriteAsync(root);
        var unityProjectionRunner = await new UnityProjectionVerificationRunnerService()
            .BuildAndWriteAsync(root);
        var parameterizedGamePackageRunner =
            await new ParameterizedGamePackageProjectionRunnerService()
                .BuildAndWriteAsync(root);
        var gamePackageCandidateMatrix =
            await new GamePackageCandidateMatrixProjectionService()
                .BuildAndWriteAsync(root);
        var gamePackageCandidateFactory =
            await new GamePackageCandidateFactoryProjectionService()
                .BuildAndWriteAsync(root);
        var gamePackageCandidateRecipePipeline =
            await new GamePackageCandidateRecipePipelineService()
                .BuildAndWriteAsync(root);
        var candidatePipelineOperator =
            await new GamePackageCandidatePipelineOperatorService()
                .BuildAndWriteAsync(root);
        var productLineStrategyRebaseline =
            await new ProductLineStrategyRebaselineService()
                .BuildAndWriteAsync(root);
        var canonicalRuntimePlayerCommandLoop =
            await BuildGoal136CanonicalRuntimePlayerCommandLoopAsync(root);
        var canonicalRuntimeUnityPlayerLoopPlayback =
            await BuildGoal137CanonicalRuntimeUnityPlayerLoopPlaybackAsync(root);
        var runtimeBackedUnityPlayerLoopStepper =
            await BuildGoal138RuntimeBackedUnityPlayerLoopStepperAsync(root);

        Assert.Equal("GREEN", projection.QualityGateScan.ImplementationStatus);
        Assert.True(projection.QualityGateScan.Passed);
        Assert.True(projection.Dashboard.AcceptedBaselineReady);
        Assert.Equal(
            AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath,
            projection.Dashboard.UnityMenuPath);
        Assert.True(hotfix.ScriptScan.Passed);
        Assert.True(hotfix.Dashboard.RendererMaterialSourceAccessAbsent);
        Assert.True(hotfix.Dashboard.MaterialAssignmentSourceAccessAbsent);
        Assert.True(hotfix.Dashboard.MaterialPropertyBlockUsed);
        Assert.True(hotfix.NegativeProof.Passed);
        Assert.Equal("GREEN", usability.Dashboard.UsabilityStatus);
        Assert.True(usability.Dashboard.LegendPresent);
        Assert.True(usability.Dashboard.MarkerDescriptorPresent);
        Assert.True(usability.Dashboard.SelectionControlsPresent);
        Assert.True(usability.Dashboard.FocusCameraControlPresent);
        Assert.True(usability.Dashboard.CleanupScriptContractPassed);
        Assert.Equal("GREEN", drilldown.Result.Dashboard.FullVerificationStatus);
        Assert.True(drilldown.Result.Dashboard.OneClickButtonPresent);
        Assert.True(drilldown.Result.Dashboard.DrilldownFieldsPresent);
        Assert.True(drilldown.Result.Dashboard.InteractionPreviewPresent);
        Assert.True(drilldown.Result.Dashboard.ObjectiveReplayDetailsPresent);
        Assert.True(drilldown.Result.Dashboard.CleanupScriptAvailable);
        Assert.True(drilldown.Result.Dashboard.MaterialWarningGuardPresent);
        Assert.True(drilldown.Result.Dashboard.HumanManualStepsReducedToOneButton);
        Assert.True(drilldown.Result.ScriptInventory.Passed);
        Assert.True(drilldown.Result.NegativeProof.Passed);
        Assert.Contains(drilldown.WrittenFiles, path =>
            path == AcceptedAlphaInteractionDrilldownVerificationVocabulary
                .ProceduralOutputDirectory
            + "/"
            + AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName);
        Assert.Contains(drilldown.WrittenFiles, path =>
            path == AcceptedAlphaInteractionDrilldownVerificationVocabulary
                .ExportPackageDirectory
            + "/"
            + AcceptedAlphaInteractionDrilldownVerificationVocabulary.LogScanFileName);
        Assert.Contains(drilldown.WrittenFiles, path =>
            path == AcceptedAlphaInteractionDrilldownVerificationVocabulary.DocumentationPath);
        Assert.DoesNotContain(drilldown.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Equal("GREEN", actionLoop.Result.Dashboard.ActionLoopStatus);
        Assert.Equal("GREEN", actionLoop.Result.Dashboard.WindowPolishStatus);
        Assert.True(actionLoop.Result.Dashboard.OneClickVerificationStillPresent);
        Assert.True(actionLoop.Result.Dashboard.ProjectionActionPreviewPresent);
        Assert.True(actionLoop.Result.Dashboard.ProjectionActionApplyPresent);
        Assert.True(actionLoop.Result.Dashboard.ProjectionStateResetPresent);
        Assert.True(actionLoop.Result.Dashboard.WindowLayoutPolishPresent);
        Assert.True(actionLoop.Result.ScriptInventory.Passed);
        Assert.True(actionLoop.Result.NegativeProof.Passed);
        Assert.Contains(actionLoop.WrittenFiles, path =>
            path == AcceptedAlphaProjectionActionLoopVocabulary
                .ProceduralOutputDirectory
            + "/"
            + AcceptedAlphaProjectionActionLoopVocabulary.DashboardFileName);
        Assert.Contains(actionLoop.WrittenFiles, path =>
            path == AcceptedAlphaProjectionActionLoopVocabulary
                .ExportPackageDirectory
            + "/"
            + AcceptedAlphaProjectionActionLoopVocabulary.LogScanFileName);
        Assert.Contains(actionLoop.WrittenFiles, path =>
            path == AcceptedAlphaProjectionActionLoopVocabulary.DocumentationPath);
        Assert.DoesNotContain(actionLoop.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Equal("GREEN", genericProjection.Result.Dashboard.GenericProjectionStatus);
        Assert.Equal("game/minimal-map-game", genericProjection.Result.Dashboard.PackageId);
        Assert.Equal("Minimal Map Game", genericProjection.Result.Dashboard.PackageTitle);
        Assert.Equal("map/village", genericProjection.Result.Dashboard.MapId);
        Assert.True(genericProjection.Result.Dashboard.EntityCount >= 2);
        Assert.True(genericProjection.Result.Dashboard.ItemCount >= 1);
        Assert.True(genericProjection.Result.Dashboard.Goal122StillGreen);
        Assert.True(genericProjection.Result.ScriptInventory.Passed);
        Assert.True(genericProjection.Result.SamplePackage.Passed);
        Assert.True(genericProjection.Result.NegativeProof.Passed);
        Assert.Contains(genericProjection.WrittenFiles, path =>
            path == GenericGamePackageProjectionVocabulary
                .ProceduralOutputDirectory
            + "/"
            + GenericGamePackageProjectionVocabulary.DashboardFileName);
        Assert.Contains(genericProjection.WrittenFiles, path =>
            path == GenericGamePackageProjectionVocabulary
                .ExportPackageDirectory
            + "/"
            + GenericGamePackageProjectionVocabulary.LogScanFileName);
        Assert.Contains(genericProjection.WrittenFiles, path =>
            path == GenericGamePackageProjectionVocabulary.DocumentationPath);
        Assert.DoesNotContain(genericProjection.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Equal("GREEN", genericLoop.Result.Dashboard.GenericLoopStatus);
        Assert.Equal("game/minimal-map-game", genericLoop.Result.Dashboard.PackageId);
        Assert.Equal("map/village", genericLoop.Result.Dashboard.MapId);
        Assert.True(genericLoop.Result.Dashboard.InteractionPreviewPresent);
        Assert.True(genericLoop.Result.Dashboard.InteractionApplyPassed);
        Assert.True(genericLoop.Result.Dashboard.DialogueSummaryPresent);
        Assert.True(genericLoop.Result.Dashboard.QuestObjectiveSummaryPresent);
        Assert.True(genericLoop.Result.Dashboard.InventorySummaryPresent);
        Assert.True(genericLoop.Result.Dashboard.ResourceSummaryPresent);
        Assert.True(genericLoop.Result.Dashboard.Goal123StillGreen);
        Assert.True(genericLoop.Result.ScriptInventory.Passed);
        Assert.True(genericLoop.Result.SamplePackage.Passed);
        Assert.True(genericLoop.Result.NegativeProof.Passed);
        Assert.Contains(genericLoop.WrittenFiles, path =>
            path == GenericGamePackageLoopProjectionVocabulary
                .ProceduralOutputDirectory
            + "/"
            + GenericGamePackageLoopProjectionVocabulary.DashboardFileName);
        Assert.Contains(genericLoop.WrittenFiles, path =>
            path == GenericGamePackageLoopProjectionVocabulary
                .ExportPackageDirectory
            + "/"
            + GenericGamePackageLoopProjectionVocabulary.LogScanFileName);
        Assert.Contains(genericLoop.WrittenFiles, path =>
            path == GenericGamePackageLoopProjectionVocabulary.DocumentationPath);
        Assert.DoesNotContain(genericLoop.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Equal("GREEN", genericSystems.Result.Dashboard.GenericSystemsStatus);
        Assert.Equal("game/minimal-map-game", genericSystems.Result.Dashboard.PackageId);
        Assert.True(genericSystems.Result.Dashboard.RecipePreviewPresent);
        Assert.True(genericSystems.Result.Dashboard.RecipeApplyPassed);
        Assert.True(genericSystems.Result.Dashboard.HarvestPreviewPresent);
        Assert.True(genericSystems.Result.Dashboard.HarvestApplyPassed);
        Assert.True(genericSystems.Result.Dashboard.TransactionPreviewPresent);
        Assert.True(genericSystems.Result.Dashboard.EncounterPreviewPresent);
        Assert.True(genericSystems.Result.Dashboard.CombatRoundPreviewPresent);
        Assert.True(genericSystems.Result.Dashboard.InventorySummaryPresent);
        Assert.True(genericSystems.Result.Dashboard.ResourceSummaryPresent);
        Assert.True(genericSystems.Result.Dashboard.SystemsEventLogPresent);
        Assert.True(genericSystems.Result.Dashboard.Goal124StillGreen);
        Assert.True(genericSystems.Result.ScriptInventory.Passed);
        Assert.True(genericSystems.Result.SamplePackage.Passed);
        Assert.True(genericSystems.Result.NegativeProof.Passed);
        Assert.Contains(genericSystems.WrittenFiles, path =>
            path == GenericGamePackageSystemsProjectionVocabulary
                .ProceduralOutputDirectory
            + "/"
            + GenericGamePackageSystemsProjectionVocabulary.DashboardFileName);
        Assert.Contains(genericSystems.WrittenFiles, path =>
            path == GenericGamePackageSystemsProjectionVocabulary
                .ExportPackageDirectory
            + "/"
            + GenericGamePackageSystemsProjectionVocabulary.LogScanFileName);
        Assert.Contains(genericSystems.WrittenFiles, path =>
            path == GenericGamePackageSystemsProjectionVocabulary.DocumentationPath);
        Assert.DoesNotContain(genericSystems.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Equal("GREEN", genericFullPlaythrough.Result.Dashboard.FullPlaythroughStatus);
        Assert.Equal("game/minimal-map-game", genericFullPlaythrough.Result.Dashboard.PackageId);
        Assert.Equal("map/village", genericFullPlaythrough.Result.Dashboard.MapId);
        Assert.True(genericFullPlaythrough.Result.Dashboard.MapPathPreviewPresent);
        Assert.True(genericFullPlaythrough.Result.Dashboard.SignInteractionApplied);
        Assert.True(genericFullPlaythrough.Result.Dashboard.DialogueSummaryPresent);
        Assert.True(genericFullPlaythrough.Result.Dashboard.QuestObjectiveStatusPresent);
        Assert.True(genericFullPlaythrough.Result.Dashboard.InventorySummaryPresent);
        Assert.True(genericFullPlaythrough.Result.Dashboard.ResourceSummaryPresent);
        Assert.True(genericFullPlaythrough.Result.Dashboard.SystemsSummaryPresent);
        Assert.True(genericFullPlaythrough.Result.Dashboard.CombatRoundPreviewPresent);
        Assert.True(genericFullPlaythrough.Result.Dashboard.EventTranscriptPresent);
        Assert.True(genericFullPlaythrough.Result.Dashboard.Goal125StillGreen);
        Assert.True(genericFullPlaythrough.Result.ScriptInventory.Passed);
        Assert.True(genericFullPlaythrough.Result.SamplePackage.Passed);
        Assert.True(genericFullPlaythrough.Result.NegativeProof.Passed);
        Assert.Contains(genericFullPlaythrough.WrittenFiles, path =>
            path == GenericGamePackageFullPlaythroughProjectionVocabulary
                .ProceduralOutputDirectory
            + "/"
            + GenericGamePackageFullPlaythroughProjectionVocabulary.DashboardFileName);
        Assert.Contains(genericFullPlaythrough.WrittenFiles, path =>
            path == GenericGamePackageFullPlaythroughProjectionVocabulary
                .ExportPackageDirectory
            + "/"
            + GenericGamePackageFullPlaythroughProjectionVocabulary.LogScanFileName);
        Assert.Contains(genericFullPlaythrough.WrittenFiles, path =>
            path == GenericGamePackageFullPlaythroughProjectionVocabulary.DocumentationPath);
        Assert.DoesNotContain(genericFullPlaythrough.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.True(unityProjectionRunner.Result.Goal126Evidence.Passed);
        Assert.True(unityProjectionRunner.Result.ScriptScan.Passed);
        Assert.True(unityProjectionRunner.Result.NegativeProof.Passed);
        Assert.False(unityProjectionRunner.Result.Dashboard.ManualUnityClickingRequired);
        Assert.Equal(
            ".devflow\\scripts\\run-unity-projection-verification.cmd",
            unityProjectionRunner.Result.Dashboard.RunnerCommand);
        Assert.Contains(unityProjectionRunner.WrittenFiles, path =>
            path == UnityProjectionVerificationRunnerVocabulary
                .ProceduralOutputDirectory
            + "/"
            + UnityProjectionVerificationRunnerVocabulary.DashboardFileName);
        Assert.Contains(unityProjectionRunner.WrittenFiles, path =>
            path == UnityProjectionVerificationRunnerVocabulary
                .ExportPackageDirectory
            + "/"
            + UnityProjectionVerificationRunnerVocabulary.ScriptScanFileName);
        Assert.Contains(unityProjectionRunner.WrittenFiles, path =>
            path == UnityProjectionVerificationRunnerVocabulary.DocumentationPath);
        Assert.DoesNotContain(unityProjectionRunner.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.True(parameterizedGamePackageRunner.Result.Goal127Evidence.Passed);
        Assert.True(parameterizedGamePackageRunner.Result.ScriptScan.Passed);
        Assert.True(parameterizedGamePackageRunner.Result.UnitySourceScan.Passed);
        Assert.True(parameterizedGamePackageRunner.Result.NegativeProof.Passed);
        Assert.Equal(
            ParameterizedGamePackageProjectionRunnerVocabulary.DefaultPackageRelativePath,
            parameterizedGamePackageRunner.Result.Dashboard.PackagePathRelative);
        Assert.True(parameterizedGamePackageRunner.Result.Dashboard.ManualUnityOptional);
        Assert.True(parameterizedGamePackageRunner.Result.Dashboard.ProjectionOnly);
        if (parameterizedGamePackageRunner.Result.ResultScan.ResultExists)
        {
            Assert.True(parameterizedGamePackageRunner.Result.ResultScan.Passed);
            Assert.True(parameterizedGamePackageRunner.Result.LogScan.Passed);
            Assert.Equal(
                "GREEN",
                parameterizedGamePackageRunner.Result.Dashboard.ParameterizedRunnerStatus);
        }

        Assert.Contains(parameterizedGamePackageRunner.WrittenFiles, path =>
            path == ParameterizedGamePackageProjectionRunnerVocabulary
                .ProceduralOutputDirectory
            + "/"
            + ParameterizedGamePackageProjectionRunnerVocabulary.DashboardFileName);
        Assert.Contains(parameterizedGamePackageRunner.WrittenFiles, path =>
            path == ParameterizedGamePackageProjectionRunnerVocabulary
                .ExportPackageDirectory
            + "/"
            + ParameterizedGamePackageProjectionRunnerVocabulary.ScriptScanFileName);
        Assert.Contains(parameterizedGamePackageRunner.WrittenFiles, path =>
            path == ParameterizedGamePackageProjectionRunnerVocabulary.DocumentationPath);
        Assert.DoesNotContain(parameterizedGamePackageRunner.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.True(gamePackageCandidateMatrix.Result.CandidateIndex.Passed);
        Assert.Equal(2, gamePackageCandidateMatrix.Result.CandidateIndex.CandidateCount);
        Assert.True(gamePackageCandidateMatrix.Result.ScriptScan.Passed);
        Assert.True(gamePackageCandidateMatrix.Result.NegativeProof.Passed);
        Assert.True(gamePackageCandidateMatrix.Result.Dashboard.ManualUnityOptional);
        Assert.True(gamePackageCandidateMatrix.Result.Dashboard.ProjectionOnly);
        Assert.Contains(gamePackageCandidateMatrix.WrittenFiles, path =>
            path == GamePackageCandidateMatrixProjectionVocabulary.CandidateIndexRelativePath);
        Assert.Contains(gamePackageCandidateMatrix.WrittenFiles, path =>
            path == GamePackageCandidateMatrixProjectionVocabulary.BaselineCandidatePackagePath);
        Assert.Contains(gamePackageCandidateMatrix.WrittenFiles, path =>
            path == GamePackageCandidateMatrixProjectionVocabulary.VariantCandidatePackagePath);
        Assert.Contains(gamePackageCandidateMatrix.WrittenFiles, path =>
            path == GamePackageCandidateMatrixProjectionVocabulary.DocumentationPath);
        Assert.DoesNotContain(gamePackageCandidateMatrix.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        if (gamePackageCandidateMatrix.Result.MatrixResultScan.ResultExists)
        {
            Assert.True(gamePackageCandidateMatrix.Result.MatrixResultScan.Passed);
            Assert.True(gamePackageCandidateMatrix.Result.LogScan.Passed);
            Assert.Equal("GREEN", gamePackageCandidateMatrix.Result.Dashboard.MatrixStatus);
        }
        Assert.True(gamePackageCandidateFactory.Result.CandidateIndexScan.Passed);
        Assert.Equal(3, gamePackageCandidateFactory.Result.CandidateIndexScan.CandidateCount);
        Assert.True(gamePackageCandidateFactory.Result.ScriptScan.Passed);
        Assert.True(gamePackageCandidateFactory.Result.FactoryResultScan.Passed);
        Assert.True(gamePackageCandidateFactory.Result.MatrixResultScan.Passed);
        Assert.True(gamePackageCandidateFactory.Result.LogScan.Passed);
        Assert.True(gamePackageCandidateFactory.Result.NegativeProof.Passed);
        Assert.Equal("GREEN", gamePackageCandidateFactory.Result.Dashboard.CandidateFactoryStatus);
        Assert.True(gamePackageCandidateFactory.Result.Dashboard.MatrixPassed);
        Assert.True(gamePackageCandidateFactory.Result.Dashboard.SamplePackageUnmodified);
        Assert.Contains(gamePackageCandidateFactory.Result.ProceduralFileIndex.Files, file =>
            file.RelativePath == GamePackageCandidateFactoryProjectionVocabulary.CandidateIndexRelativePath);
        Assert.Contains(gamePackageCandidateFactory.WrittenFiles, path =>
            path == GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory
            + "/"
            + GamePackageCandidateFactoryProjectionVocabulary.CandidateIndexFileName);
        Assert.Contains(gamePackageCandidateFactory.WrittenFiles, path =>
            path == GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory
            + "/"
            + GamePackageCandidateFactoryProjectionVocabulary.FactoryResultFileName);
        Assert.Contains(gamePackageCandidateFactory.WrittenFiles, path =>
            path == GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory
            + "/"
            + GamePackageCandidateFactoryProjectionVocabulary.MatrixResultFileName);
        Assert.Contains(gamePackageCandidateFactory.WrittenFiles, path =>
            path == GamePackageCandidateFactoryProjectionVocabulary.DocumentationPath);
        Assert.DoesNotContain(gamePackageCandidateFactory.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Equal("GREEN", gamePackageCandidateRecipePipeline.Result.Dashboard.RecipePipelineStatus);
        Assert.Equal(4, gamePackageCandidateRecipePipeline.Result.Dashboard.CandidateCount);
        Assert.Equal(4, gamePackageCandidateRecipePipeline.Result.Dashboard.PassedCandidates);
        Assert.Equal(0, gamePackageCandidateRecipePipeline.Result.Dashboard.FailedCandidates);
        Assert.True(gamePackageCandidateRecipePipeline.Result.Dashboard.MatrixPassed);
        Assert.True(gamePackageCandidateRecipePipeline.Result.Dashboard.SamplePackageUnmodified);
        Assert.Equal("GREEN_READY", candidatePipelineOperator.Result.Dashboard.OperatorStatus);
        Assert.True(candidatePipelineOperator.Result.Dashboard.WinFormsPanelPresent);
        Assert.True(candidatePipelineOperator.Result.Dashboard.RefreshButtonPresent);
        Assert.True(candidatePipelineOperator.Result.Dashboard.CopyCommandButtonPresent);
        Assert.True(candidatePipelineOperator.Result.Dashboard.DryRunButtonPresent);
        Assert.True(candidatePipelineOperator.Result.Dashboard.RunButtonPresent);
        Assert.True(candidatePipelineOperator.Result.Dashboard.AsyncRunPresent);
        Assert.True(candidatePipelineOperator.Result.Dashboard.OperatorResultPresent);
        Assert.Equal(4, candidatePipelineOperator.Result.Dashboard.CandidateCount);
        Assert.True(candidatePipelineOperator.Result.Dashboard.MatrixPassed);
        Assert.Contains(candidatePipelineOperator.WrittenFiles, path =>
            path == GamePackageCandidatePipelineOperatorVocabulary.ExportPackageDirectory
            + "/"
            + GamePackageCandidatePipelineOperatorVocabulary.DashboardFileName);
        Assert.Contains(candidatePipelineOperator.WrittenFiles, path =>
            path == GamePackageCandidatePipelineOperatorVocabulary.DocumentationPath);
        Assert.DoesNotContain(candidatePipelineOperator.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Equal("GREEN", productLineStrategyRebaseline.Result.Dashboard.ImplementationStatus);
        Assert.Equal(
            ProductLineStrategyRebaselineVocabulary.Gate,
            productLineStrategyRebaseline.Result.Dashboard.Gate);
        Assert.False(productLineStrategyRebaseline.Result.Dashboard.Accepted);
        Assert.Equal(
            ProductLineStrategyRebaselineVocabulary.NextGoal,
            productLineStrategyRebaseline.Result.Dashboard.NextGoal);
        Assert.True(productLineStrategyRebaseline.Result.Dashboard.ProductLineCombiner);
        Assert.True(productLineStrategyRebaseline.Result.Dashboard.NotPromptToGame);
        Assert.True(productLineStrategyRebaseline.Result.Dashboard.LlmOptionalAuthoringOnly);
        Assert.True(productLineStrategyRebaseline.Result.Dashboard.CurrentStateUpdated);
        Assert.True(productLineStrategyRebaseline.Result.Dashboard.QueueUpdated);
        Assert.True(productLineStrategyRebaseline.Result.NegativeProof.Passed);
        Assert.Contains(productLineStrategyRebaseline.WrittenFiles, path =>
            path == ProductLineStrategyRebaselineVocabulary.ExportPackageDirectory
            + "/"
            + ProductLineStrategyRebaselineVocabulary.DashboardFileName);
        Assert.Contains(productLineStrategyRebaseline.WrittenFiles, path =>
            path == ProductLineStrategyRebaselineVocabulary.DocumentationPath);
        Assert.DoesNotContain(productLineStrategyRebaseline.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Equal("GREEN", canonicalRuntimePlayerCommandLoop.Dashboard.Status);
        Assert.True(canonicalRuntimePlayerCommandLoop.Dashboard.PlayerCommandLoopPassed);
        Assert.Equal(13, canonicalRuntimePlayerCommandLoop.Dashboard.PlayerCommandCount);
        Assert.Equal(13, canonicalRuntimePlayerCommandLoop.Dashboard.SnapshotCount);
        Assert.True(canonicalRuntimePlayerCommandLoop.Dashboard.RuntimeEventCount >= 10);
        Assert.True(canonicalRuntimePlayerCommandLoop.Dashboard.UnityPlayerConsumedCommandLoopSnapshots);
        Assert.False(canonicalRuntimePlayerCommandLoop.Dashboard.ProjectionOnly);
        Assert.False(canonicalRuntimePlayerCommandLoop.Dashboard.UnityGameplayTruth);
        Assert.True(canonicalRuntimePlayerCommandLoop.Dashboard.NoUnclassifiedErrorDiagnostics);
        Assert.Equal("GREEN", canonicalRuntimeUnityPlayerLoopPlayback.Dashboard.Status);
        Assert.Equal(13, canonicalRuntimeUnityPlayerLoopPlayback.Dashboard.PlaybackFrameCount);
        Assert.True(canonicalRuntimeUnityPlayerLoopPlayback.Dashboard.RequiredFrameCategoriesPresent);
        Assert.True(canonicalRuntimeUnityPlayerLoopPlayback.Dashboard.UnityPlayerLoopPlaybackPassed);
        Assert.True(canonicalRuntimeUnityPlayerLoopPlayback.Dashboard.RuntimeSnapshotSource);
        Assert.False(canonicalRuntimeUnityPlayerLoopPlayback.Dashboard.UnityGameplayTruth);
        Assert.False(canonicalRuntimeUnityPlayerLoopPlayback.Dashboard.ProjectionOnly);
        Assert.True(canonicalRuntimeUnityPlayerLoopPlayback.Dashboard.SelectedCandidateExecutedByRuntime);
        Assert.Equal("GREEN", runtimeBackedUnityPlayerLoopStepper.Dashboard.Status);
        Assert.True(runtimeBackedUnityPlayerLoopStepper.Dashboard.AcceptedGoal137);
        Assert.Equal(13, runtimeBackedUnityPlayerLoopStepper.Dashboard.FrameCount);
        Assert.True(runtimeBackedUnityPlayerLoopStepper.Dashboard.RequiredFrameCategoriesPresent);
        Assert.True(runtimeBackedUnityPlayerLoopStepper.Dashboard.RuntimeAuthority);
        Assert.False(runtimeBackedUnityPlayerLoopStepper.Dashboard.UnityGameplayTruth);
        Assert.False(runtimeBackedUnityPlayerLoopStepper.Dashboard.ProjectionOnly);
        Assert.True(runtimeBackedUnityPlayerLoopStepper.Dashboard.StepperWindowPresent);
        Assert.True(runtimeBackedUnityPlayerLoopStepper.Dashboard.StepperBatchSmokePassed);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaUnityPlayableProjection);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaProjectionUsability);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaInteractionDrilldown);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaProjectionActionLoop);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysGenericGamePackageProjection);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysGenericGamePackageLoop);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysGenericGamePackageSystems);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysGenericGamePackageFullPlaythrough);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysUnityProjectionVerificationRunner);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysParameterizedGamePackageRunner);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysGamePackageCandidateMatrix);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysGamePackageCandidateFactory);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysGamePackageCandidateRecipePipeline);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysCandidatePipelineOperator);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysCanonicalRuntimePlayerCommandLoop);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysCanonicalRuntimeUnityPlayerLoopPlayback);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysRuntimeBackedUnityPlayerLoopStepper);
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "accepted_alpha_unity_playable_projection");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "accepted_alpha_projection_usability");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "accepted_alpha_interaction_drilldown_verification");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "accepted_alpha_projection_action_loop");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "generic_gamepackage_projection");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "generic_gamepackage_loop");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "generic_gamepackage_systems_loop");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "generic_gamepackage_full_playthrough");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "unity_projection_verification_runner");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "parameterized_gamepackage_projection_runner");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "gamepackage_candidate_matrix_projection_runner");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "gamepackage_candidate_factory_and_matrix_pipeline");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "gamepackage_candidate_recipe_catalog_scoring_and_promotion");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "candidate_pipeline_operator_panel");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "canonical_runtime_player_command_loop");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "canonical_runtime_unity_player_loop_playback");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "runtime_backed_unity_player_loop_stepper");
        Assert.True(workspace.QualityGateScan.AcceptedAlphaUnityPlayableProjectionQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal119FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.AcceptedAlphaProjectionUsabilityQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal120FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.AcceptedAlphaInteractionDrilldownQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal121FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.AcceptedAlphaProjectionActionLoopQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal122FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.GenericGamePackageProjectionQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal123FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.GenericGamePackageLoopQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal124FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.GenericGamePackageSystemsQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal125FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.GenericGamePackageFullPlaythroughQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal126FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.UnityProjectionVerificationRunnerGroupPresent);
        Assert.True(workspace.QualityGateScan.Goal127FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.ParameterizedGamePackageRunnerGroupPresent);
        Assert.True(workspace.QualityGateScan.Goal128FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateMatrixGroupPresent);
        Assert.True(workspace.QualityGateScan.Goal129FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateFactoryGroupPresent);
        Assert.True(workspace.QualityGateScan.Goal130FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateRecipePipelineGroupPresent);
        Assert.True(workspace.QualityGateScan.Goal131FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorGroupPresent);
        Assert.True(workspace.QualityGateScan.Goal132FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopGroupPresent);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopQualityGatePassed);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopGoal136FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackGroupPresent);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackQualityGatePassed);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackGoal137FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperGroupPresent);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperQualityGatePassed);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperFilesDiscoveredByRelativePaths);
        if (workspace.QualityGateScan.GamePackageCandidateMatrixStatus == "GREEN")
        {
            Assert.Equal(2, workspace.QualityGateScan.GamePackageCandidateMatrixCandidateCount);
            Assert.Equal(2, workspace.QualityGateScan.GamePackageCandidateMatrixPassedCandidateCount);
            Assert.Equal(0, workspace.QualityGateScan.GamePackageCandidateMatrixFailedCandidateCount);
            Assert.True(workspace.QualityGateScan.GamePackageCandidateMatrixCleanupApplied);
            Assert.True(workspace.QualityGateScan.GamePackageCandidateMatrixQualityGatePassed);
        }
        Assert.Equal("GREEN", workspace.QualityGateScan.GamePackageCandidateFactoryStatus);
        Assert.Equal(3, workspace.QualityGateScan.GamePackageCandidateFactoryCandidateCount);
        Assert.Equal(3, workspace.QualityGateScan.GamePackageCandidateFactoryPassedCandidates);
        Assert.Equal(0, workspace.QualityGateScan.GamePackageCandidateFactoryFailedCandidates);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateFactoryMatrixPassed);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateFactoryQualityGatePassed);
        Assert.Equal("GREEN", workspace.QualityGateScan.GamePackageCandidateRecipePipelineStatus);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateRecipePipelineQualityGatePassed);
        Assert.Equal("GREEN_READY", workspace.QualityGateScan.CandidatePipelineOperatorStatus);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorQualityGatePassed);
        Assert.Contains(
            "acceptedAlphaUnityPlayableProjectionGeneratedRootName: "
            + AcceptedAlphaUnityPlayableProjectionVocabulary.GeneratedRootName,
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "acceptedAlphaProjectionUsabilityCleanupScriptPath: "
            + AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath,
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "acceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "acceptedAlphaProjectionActionLoopStatus: GREEN",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "genericProjectionStatus: GREEN",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "genericLoopStatus: GREEN",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "genericSystemsStatus: GREEN",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "fullPlaythroughStatus: GREEN",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "runnerCommand: .devflow\\scripts\\run-unity-projection-verification.cmd",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "parameterizedRunnerStatus:",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "gamePackageCandidateMatrixStatus:",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "candidateFactoryStatus:",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "recipePipelineStatus:",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "candidatePipelineOperatorStatus: GREEN_READY",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "playerCommandLoopPassed: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "unityPlayerLoopPlaybackPassed: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "stepperBatchSmokePassed: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
    }

    private static async Task<CanonicalRuntimePlayerCommandLoopWriteResult>
        BuildGoal136CanonicalRuntimePlayerCommandLoopAsync(string root)
    {
        var handoffPath = Path.Combine(
            root,
            CanonicalRuntimePlayerCommandLoopVocabulary.DefaultSelectedCandidateHandoffPath);
        var packagePath = Path.Combine(
            root,
            CanonicalRuntimePlayerCommandLoopVocabulary.DefaultSelectedCandidatePackagePath);
        var request = new CanonicalRuntimePlayerCommandLoopRequest
        {
            CandidateId = CanonicalRuntimeSelectedCandidatePlaythroughArtifactService
                .ReadCandidateId(handoffPath),
            HandoffPath = Relative(root, handoffPath),
            PackagePath = Relative(root, packagePath),
            Goal134TranscriptPath =
                CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal134TranscriptPath,
            Goal134StateSummaryPath =
                CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal134StateSummaryPath,
            Goal135PlayerLoopPlanPath =
                CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal135PlayerLoopPlanPath,
            Goal135PlayerAdapterContractPath =
                CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal135PlayerAdapterContractPath
        };
        var package =
            CanonicalRuntimeSelectedCandidatePlaythroughArtifactService.LoadPackage(packagePath);
        var runtimeResult = CanonicalRuntimePlayerCommandLoopService
            .CreateDefault()
            .Execute(package, request);

        return await new CanonicalRuntimePlayerCommandLoopArtifactService()
            .BuildAndWriteAsync(
                root,
                request,
                runtimeResult,
                unitySmoke: PassedGoal136UnitySmoke(root));
    }

    private static CanonicalRuntimePlayerCommandLoopUnitySmoke PassedGoal136UnitySmoke(string root)
    {
        var snapshots = Path.Combine(
            root,
            CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory,
            CanonicalRuntimePlayerCommandLoopVocabulary.SnapshotsFileName);
        var result = Path.Combine(
            root,
            CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory,
            CanonicalRuntimePlayerCommandLoopVocabulary.ResultFileName);
        return new CanonicalRuntimePlayerCommandLoopUnitySmoke
        {
            UnityAvailable = true,
            SnapshotsPathExists = true,
            ResultPathExists = true,
            PassMarkerPresent = true,
            FailMarkerPresent = false,
            SnapshotContractPresent = true,
            UnityPlayerConsumedCommandLoopSnapshots = true,
            Passed = true,
            UnityPath = "test-unity",
            SnapshotsPath = Relative(root, snapshots),
            ResultPath = Relative(root, result),
            Status = "GREEN"
        };
    }

    private static async Task<CanonicalRuntimeUnityPlayerLoopPlaybackWriteResult>
        BuildGoal137CanonicalRuntimeUnityPlayerLoopPlaybackAsync(string root) =>
        await new CanonicalRuntimeUnityPlayerLoopPlaybackArtifactService()
            .BuildAndWriteAsync(
                root,
                new CanonicalRuntimeUnityPlayerLoopPlaybackRequest(),
                unitySmoke: PassedGoal137UnitySmoke(root));

    private static CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke PassedGoal137UnitySmoke(string root)
    {
        var frames = Path.Combine(
            root,
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory,
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.FramesFileName);
        var result = Path.Combine(
            root,
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory,
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ResultFileName);
        return new CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke
        {
            UnityAvailable = true,
            FramesPathExists = true,
            ResultPathExists = true,
            PassMarkerPresent = true,
            FailMarkerPresent = false,
            FrameCountPassed = true,
            RequiredFrameCategoriesPresent = true,
            RuntimeAuthorityMarkersPresent = true,
            UnityPlayerLoopPlaybackPassed = true,
            Passed = true,
            UnityPath = "test-unity",
            FramesPath = Relative(root, frames),
            ResultPath = Relative(root, result),
            Status = "GREEN"
        };
    }

    private static async Task<RuntimeBackedUnityPlayerLoopStepperWriteResult>
        BuildGoal138RuntimeBackedUnityPlayerLoopStepperAsync(string root) =>
        await new RuntimeBackedUnityPlayerLoopStepperArtifactService()
            .BuildAndWriteAsync(
                root,
                new RuntimeBackedUnityPlayerLoopStepperRequest(),
                unitySmoke: PassedGoal138UnitySmoke(root));

    private static RuntimeBackedUnityPlayerLoopStepperUnitySmoke PassedGoal138UnitySmoke(string root)
    {
        var model = Path.Combine(
            root,
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ProceduralOutputDirectory,
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ModelFileName);
        return new RuntimeBackedUnityPlayerLoopStepperUnitySmoke
        {
            UnityAvailable = true,
            ModelPathExists = true,
            PassMarkerPresent = true,
            FailMarkerPresent = false,
            FrameCountPassed = true,
            RequiredFrameCategoriesPresent = true,
            RuntimeAuthorityMarkersPresent = true,
            StepperWindowPresent = true,
            StepperBatchSmokePassed = true,
            Passed = true,
            UnityPath = "test-unity",
            ModelPath = Relative(root, model),
            Status = "GREEN"
        };
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

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
