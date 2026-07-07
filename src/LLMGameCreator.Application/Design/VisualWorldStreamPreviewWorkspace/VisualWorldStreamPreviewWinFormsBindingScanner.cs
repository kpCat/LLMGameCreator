namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    public VisualWorldPreviewWinFormsBindingInventory BuildWinFormsBindingInventory(
        string projectRootPath)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var pageRelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.cs";
        var pageGoal108RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal108.cs";
        var pageGoal109RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal109.cs";
        var pageGoal110RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal110.cs";
        var pageGoal111RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal111.cs";
        var pageGoal112RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal112.cs";
        var pageGoal113RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal113.cs";
        var pageGoal115RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal115.cs";
        var pageGoal116RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal116.cs";
        var pageGoal117RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal117.cs";
        var pageGoal118RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal118.cs";
        var pageGoal119RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal119.cs";
        var designerRelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Designer.cs";
        var compositionRelativePath = "src/LLMGameCreator.WinForms/CompositionRoot.cs";
        var pageText = ReadOptionalText(projectRoot, pageRelativePath)
                       + "\n"
                       + ReadOptionalText(projectRoot, pageGoal108RelativePath)
                       + "\n"
                       + ReadOptionalText(projectRoot, pageGoal109RelativePath)
                       + "\n"
                       + ReadOptionalText(projectRoot, pageGoal110RelativePath)
                       + "\n"
                       + ReadOptionalText(projectRoot, pageGoal111RelativePath)
                       + "\n"
                       + ReadOptionalText(projectRoot, pageGoal112RelativePath)
                       + "\n"
                       + ReadOptionalText(projectRoot, pageGoal113RelativePath)
                       + "\n"
                       + ReadOptionalText(projectRoot, pageGoal115RelativePath)
                       + "\n"
                       + ReadOptionalText(projectRoot, pageGoal116RelativePath)
                       + "\n"
                       + ReadOptionalText(projectRoot, pageGoal117RelativePath)
                       + "\n" + ReadOptionalText(projectRoot, pageGoal118RelativePath)
                       + "\n" + ReadOptionalText(projectRoot, pageGoal119RelativePath)
                       + "\n" + ReadGoal120Through137WinFormsPageText(projectRoot);
        var designerText = ReadOptionalText(projectRoot, designerRelativePath);
        var compositionText = ReadOptionalText(projectRoot, compositionRelativePath);
        var diagnostics = new List<VisualWorldPreviewDiagnostic>();
        var pageExists = pageText.Length > 0;
        var designerExists = designerText.Length > 0;
        var serviceRegistered = compositionText.Contains(
            "VisualWorldStreamPreviewWorkspaceService",
            StringComparison.Ordinal);
        var pageRegistered = compositionText.Contains(
            "VisualWorldStreamPreviewWorkspacePageControl",
            StringComparison.Ordinal);
        var registryIncludesPage = compositionText.Contains(
            "resolver.Resolve<VisualWorldStreamPreviewWorkspacePageControl>()",
            StringComparison.Ordinal);
        var activationLoads =
            (pageText.Contains("BuildAndWriteAsync(root)", StringComparison.Ordinal)
             && pageText.Contains("Bind(write.Result)", StringComparison.Ordinal))
            || pageText.Contains("Bind(_service.Build(root))", StringComparison.Ordinal);
        var bindDisplays = pageText.Contains("_groupsListBox", StringComparison.Ordinal)
            && pageText.Contains("_entriesListView", StringComparison.Ordinal)
            && pageText.Contains("_proofsListView", StringComparison.Ordinal)
            && pageText.Contains("_svgPreviewTextBox", StringComparison.Ordinal);
        var bindDisplaysCacheExports = pageText.Contains("CacheRecordCount", StringComparison.Ordinal)
            && pageText.Contains("ExportTargetKind", StringComparison.Ordinal)
            && pageText.Contains("RuntimeHandoffMetadataOnly", StringComparison.Ordinal)
            && pageText.Contains("ReadbackProofPassed", StringComparison.Ordinal);
        var bindDisplaysUnityHandoff = pageText.Contains("PayloadFileCount", StringComparison.Ordinal)
            && pageText.Contains("UniqueChunkKeyCount", StringComparison.Ordinal)
            && pageText.Contains("SimulatedUnityReadProofPassed", StringComparison.Ordinal)
            && pageText.Contains("AlphaRuntimeBootstrapUnchanged", StringComparison.Ordinal);
        var bindDisplaysGeoworld = pageText.Contains("OfflineBundleId", StringComparison.Ordinal)
            && pageText.Contains("GeoworldNormalizedFeatureCount", StringComparison.Ordinal)
            && pageText.Contains("GeoworldWorldSourceGraphChunkCount", StringComparison.Ordinal)
            && pageText.Contains("BoundaryPrefetchStatus", StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldHandoff = pageText.Contains(
                "offlineGeoworldHandoffPackageCount",
                StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldHandoffFeatureKindCounts", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldHandoffUnityPayloadFileCount", StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldHandoffAlphaRuntimeBootstrapUnchanged",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldUnityPreview = pageText.Contains(
                "offlineGeoworldUnityPreviewCommandCount",
                StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldUnityPreviewKindCoverage", StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldUnityPreviewTravelWindowStepCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldUnityPreviewUnityScriptsReady",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldUnityEditorPreview = pageText.Contains(
                "offlineGeoworldUnityEditorPreviewCommandCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldUnityEditorPreviewEditorWindowScriptPath",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldUnityEditorPreviewMenuItemMarker",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldUnityEditorPreviewManualInstructions",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldPlayModeTravel = pageText.Contains(
                "offlineGeoworldPlayModeTravelStepCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldPlayModeTravelActiveChunkCounts",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldPlayModeTravelBoundaryPrefetchCounts",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldPlayModeTravelExpectedVisibleObjectCounts",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldPlayModeTravelUnityScriptsReady",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldPlayModeTravelEditorWindowReady",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldPlayModeTravelSimulatedExecutionProofPassed",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldPlayModeTravelGoal102BClosureRecorded",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldPlayModeTravelAlphaRuntimeBootstrapUnchanged",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldInteractiveTravel = pageText.Contains(
                "offlineGeoworldInteractiveTravelMovementSampleCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractiveTravelBoundaryCrossingCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractiveTravelActiveChunkCounts",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractiveTravelBoundaryPrefetchCounts",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractiveTravelExpectedVisibleObjectCounts",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractiveTravelUnityScriptsReady",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractiveTravelEditorWindowReady",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractiveTravelSimulatedExecutionProofPassed",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractiveTravelNegativeProofPassed",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractiveTravelAlphaRuntimeBootstrapUnchanged",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractiveTravelQualityGatePassed",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldInteractions = pageText.Contains(
                "offlineGeoworldInteractionTargetCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractionActionKindCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractionScriptedEventCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractionStateDeltaCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractionStateHashChainPassed",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractionUnityScriptsReady",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractionEditorWindowReady",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractionUnitySafetyScanPassed",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractionSimulatedSessionProofPassed",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractionNegativeProofPassed",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractionAlphaRuntimeBootstrapUnchanged",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldInteractionQualityGatePassed",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldSessionReplay = pageText.Contains(
                "offlineGeoworldSessionReplayStepCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldSessionStateDeltaCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldSessionCheckpointStepIndex",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldSessionAcceptanceChecklistStepCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldSessionFinalStateHash",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldSessionUnityScriptsReady",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldSessionEditorWindowReady",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldSessionSimulatedReplayProofPassed",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldSessionNegativeProofPassed",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldSessionAlphaRuntimeBootstrapUnchanged",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldSessionQualityGatePassed",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldObjectiveAcceptance = pageText.Contains(
                "offlineGeoworldObjectiveCount",
                StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldObjectiveCompletedCount", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldObjectiveFinalStatus", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldObjectiveReplaySaveLoadLinkage", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldObjectiveUnityScriptsReady", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldObjectiveEditorWindowReady", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldObjectiveAlphaQualityConsolidationPassed", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldObjectiveManualChecklistSummary", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldObjectiveAlphaRuntimeBootstrapUnchanged", StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldAlphaSlice = pageText.Contains(
                "offlineGeoworldAlphaSliceComponentCount",
                StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldAlphaSliceUnityToolReady", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldAlphaSliceAcceptanceRunbookReady", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldAlphaSliceFinalProofPassed", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldAlphaSliceAlphaRuntimeBootstrapUnchanged", StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldAlphaExportPackage = pageText.Contains(
                "offlineGeoworldAlphaExportPackageFileCount",
                StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldAlphaExportChecksumStatus", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldAlphaExportCleanImportProofPassed", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldAlphaExportUnityVerifierReady", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldAlphaExportAcceptanceGateStatus", StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaExportAlphaRuntimeBootstrapUnchanged",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldAlphaManualAcceptance = pageText.Contains(
                "offlineGeoworldAlphaManualAcceptanceChecklistStepCount",
                StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldAlphaManualAcceptancePayloadFileCount", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldAlphaManualAcceptanceManualPending", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldAlphaManualAcceptanceUnityRunnerReady", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldAlphaManualAcceptanceSimulatedProofPassed", StringComparison.Ordinal)
            && pageText.Contains("offlineGeoworldAlphaManualAcceptanceNegativeProofPassed", StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualAcceptanceAlphaRuntimeBootstrapUnchanged",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldAlphaManualResultIntake = pageText.Contains(
                "offlineGeoworldAlphaManualResultIntakeDecisionStatus",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualResultIntakeAcceptableCandidate",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualResultIntakeAcceptedByCodex",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualResultIntakeChecklistHashMatched",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualResultIntakeMissingStepCount",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldAlphaAcceptanceOperatorPack = pageText.Contains(
                "offlineGeoworldAlphaAcceptanceOperatorStatus",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaAcceptanceOperatorGoal111DecisionStatus",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaAcceptanceOperatorPreferredManualResultPath",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaAcceptanceOperatorAcceptedByCodex",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaAcceptanceOperatorHumanAcceptanceStillRequired",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaAcceptanceOperatorDoNotStartYet",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldAlphaManualResultWorkbench = pageText.Contains(
                "offlineGeoworldAlphaManualResultWorkbenchStatus",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualResultWorkbenchGoal112OperatorStatus",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualResultWorkbenchPreferredManualResultPath",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualResultWorkbenchDraftTemplatePath",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualResultWorkbenchDoNotStartYet",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldAlphaHumanResultRevalidation = pageText.Contains(
                "offlineGeoworldAlphaHumanResultRevalidationDecisionStatus",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaHumanResultRevalidationGoal111DecisionStatus",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaHumanResultRevalidationManualResultSha256",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaHumanResultRevalidationAcceptableCandidate",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaHumanResultRevalidationRecommendedHumanDecision",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaHumanResultRevalidationManualInputNotCommitted",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldAlphaManualGateAcceptanceRecord = pageText.Contains(
                "offlineGeoworldAlphaManualGateAcceptanceManualGate",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualGateAcceptanceManualGateStatus",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualGateAcceptanceHumanAccepted",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualGateAcceptanceSourceDecisionStatus",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualGateAcceptanceManualResultSha256",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualGateAcceptanceAcceptedByCodex",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualGateAcceptanceManualInputNotCommitted",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualGateAcceptanceRawManualResultEmbeddedInArtifacts",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaManualGateAcceptanceRecommendedNextDecision",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldAlphaPostAcceptanceContinuation = pageText.Contains(
                "offlineGeoworldAlphaPostAcceptanceManualGateStatus",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaPostAcceptanceHumanAccepted",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaPostAcceptanceRecommendedNextLane",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaPostAcceptanceRecommendedNextGoalId",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaPostAcceptanceReadyLaneCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaPostAcceptanceCandidateLaneCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaPostAcceptanceBlockedLaneCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaPostAcceptanceDoNotStartAutomatically",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaPostAcceptanceEvidencePath",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAlphaPostAcceptanceExportPath",
                StringComparison.Ordinal);
        var bindDisplaysOfflineGeoworldAcceptedAlphaBaseline = pageText.Contains(
                "offlineGeoworldAcceptedAlphaBaselineId",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAcceptedAlphaBaselineReady",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAcceptedAlphaManualGateStatus",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAcceptedAlphaRecommendedNextDecision",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAcceptedAlphaIncludedSourceGoalCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAcceptedAlphaAcceptedEvidenceRootCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAcceptedAlphaProducedOnlyRootCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAcceptedAlphaDoNotStartAutomatically",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAcceptedAlphaEvidencePath",
                StringComparison.Ordinal)
            && pageText.Contains(
                "offlineGeoworldAcceptedAlphaExportPath",
                StringComparison.Ordinal);
        var bindDisplaysAcceptedAlphaUnityPlayableProjection = pageText.Contains(
                "acceptedAlphaUnityPlayableProjectionStatus",
                StringComparison.Ordinal)
            && pageText.Contains(
                "acceptedAlphaUnityPlayableProjectionUnityMenuPath",
                StringComparison.Ordinal)
            && pageText.Contains(
                "acceptedAlphaUnityPlayableProjectionBaselineId",
                StringComparison.Ordinal)
            && pageText.Contains(
                "acceptedAlphaUnityPlayableProjectionAcceptedBaselineReady",
                StringComparison.Ordinal)
            && pageText.Contains(
                "acceptedAlphaUnityPlayableProjectionGeneratedRootName",
                StringComparison.Ordinal)
            && pageText.Contains(
                "acceptedAlphaUnityPlayableProjectionScriptInventoryCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "acceptedAlphaUnityPlayableProjectionSmokePlanStepCount",
                StringComparison.Ordinal)
            && pageText.Contains(
                "acceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean",
                StringComparison.Ordinal)
            && pageText.Contains(
                "acceptedAlphaUnityPlayableProjectionEvidencePath",
                StringComparison.Ordinal)
            && pageText.Contains(
                "acceptedAlphaUnityPlayableProjectionExportPath",
                StringComparison.Ordinal);
        var bindDisplaysAcceptedAlphaProjectionUsability = PageBindsGoal120AcceptedAlphaProjectionUsability(pageText);
        var bindDisplaysAcceptedAlphaInteractionDrilldown = PageBindsGoal121AcceptedAlphaInteractionDrilldown(pageText);
        var bindDisplaysAcceptedAlphaProjectionActionLoop = PageBindsGoal122AcceptedAlphaProjectionActionLoop(pageText);
        var bindDisplaysGenericGamePackageProjection = PageBindsGoal123GenericGamePackageProjection(pageText);
        var bindDisplaysGenericGamePackageLoop = PageBindsGoal124GenericGamePackageLoop(pageText);
        var bindDisplaysGenericGamePackageSystems = PageBindsGoal125GenericGamePackageSystems(pageText);
        var bindDisplaysGenericGamePackageFullPlaythrough = PageBindsGoal126GenericGamePackageFullPlaythrough(pageText);
        var bindDisplaysUnityProjectionVerificationRunner = PageBindsGoal127UnityProjectionVerificationRunner(pageText);
        var bindDisplaysParameterizedGamePackageRunner = PageBindsGoal128ParameterizedGamePackageRunner(pageText);
        var bindDisplaysGamePackageCandidateMatrix = PageBindsGoal129GamePackageCandidateMatrix(pageText);
        var bindDisplaysGamePackageCandidateFactory = PageBindsGoal130GamePackageCandidateFactory(pageText);
        var bindDisplaysGamePackageCandidateRecipePipeline = ScanGoal131GamePackageCandidateRecipePipelineBinding(pageText, pageRelativePath, diagnostics);
        var bindDisplaysCandidatePipelineOperator = ScanGoal132CandidatePipelineOperatorBinding(pageText, pageRelativePath, diagnostics);
        var bindDisplaysCanonicalRuntimeSelectedCandidate = ScanGoal134CanonicalRuntimeSelectedCandidateBinding(pageText, pageRelativePath, diagnostics);
        var bindDisplaysCanonicalRuntimePlayerLoop =
            ScanGoal135CanonicalRuntimePlayerLoopBinding(pageText, pageRelativePath, diagnostics);
        var bindDisplaysCanonicalRuntimePlayerCommandLoop = ScanGoal136CanonicalRuntimePlayerCommandLoopBinding(pageText, pageRelativePath, diagnostics);
        var bindDisplaysCanonicalRuntimeUnityPlayerLoopPlayback =
            ScanGoal137CanonicalRuntimeUnityPlayerLoopPlaybackBinding(pageText, pageRelativePath, diagnostics);
        AddIfFalse(pageExists, "goal092.winforms.page_missing", pageRelativePath, diagnostics);
        AddIfFalse(designerExists, "goal092.winforms.designer_missing", designerRelativePath, diagnostics);
        AddIfFalse(serviceRegistered, "goal092.winforms.service_not_registered", compositionRelativePath, diagnostics);
        AddIfFalse(pageRegistered, "goal092.winforms.page_not_registered", compositionRelativePath, diagnostics);
        AddIfFalse(registryIncludesPage, "goal092.winforms.registry_missing", compositionRelativePath, diagnostics);
        AddIfFalse(activationLoads, "goal092.winforms.activation_missing", pageRelativePath, diagnostics);
        AddIfFalse(bindDisplays, "goal092.winforms.bind_missing", pageRelativePath, diagnostics);
        AddIfFalse(
            bindDisplaysCacheExports,
            "goal094.winforms.cache_export_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysUnityHandoff,
            "goal096.winforms.unity_handoff_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysGeoworld,
            "goal099.winforms.geoworld_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldHandoff,
            "goal100.winforms.offline_geoworld_handoff_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldUnityPreview,
            "goal101.winforms.offline_geoworld_unity_preview_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldUnityEditorPreview,
            "goal102.winforms.offline_geoworld_unity_editor_preview_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldPlayModeTravel,
            "goal103.winforms.offline_geoworld_playmode_travel_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldInteractiveTravel,
            "goal104.winforms.offline_geoworld_interactive_travel_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldInteractions,
            "goal105.winforms.offline_geoworld_interaction_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldSessionReplay,
            "goal106.winforms.offline_geoworld_session_replay_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldObjectiveAcceptance,
            "goal107.winforms.offline_geoworld_objective_acceptance_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldAlphaSlice,
            "goal108.winforms.offline_geoworld_alpha_slice_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldAlphaExportPackage,
            "goal109.winforms.offline_geoworld_alpha_export_package_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldAlphaManualAcceptance,
            "goal110.winforms.offline_geoworld_alpha_manual_acceptance_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldAlphaManualResultIntake,
            "goal111.winforms.offline_geoworld_alpha_manual_result_intake_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldAlphaAcceptanceOperatorPack,
            "goal112.winforms.offline_geoworld_alpha_acceptance_operator_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldAlphaManualResultWorkbench,
            "goal113.winforms.offline_geoworld_alpha_manual_result_workbench_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldAlphaHumanResultRevalidation,
            "goal115.winforms.offline_geoworld_alpha_human_result_revalidation_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldAlphaManualGateAcceptanceRecord,
            "goal116.winforms.offline_geoworld_alpha_manual_gate_acceptance_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldAlphaPostAcceptanceContinuation,
            "goal117.winforms.offline_geoworld_alpha_post_acceptance_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysOfflineGeoworldAcceptedAlphaBaseline,
            "goal118.winforms.offline_geoworld_accepted_alpha_baseline_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(
            bindDisplaysAcceptedAlphaUnityPlayableProjection,
            "goal119.winforms.accepted_alpha_unity_playable_projection_bind_missing",
            pageRelativePath,
            diagnostics);
        AddIfFalse(bindDisplaysAcceptedAlphaProjectionUsability, "goal120.winforms.accepted_alpha_projection_usability_bind_missing", pageRelativePath, diagnostics);
        AddIfFalse(bindDisplaysAcceptedAlphaInteractionDrilldown, "goal121.winforms.accepted_alpha_interaction_drilldown_bind_missing", pageRelativePath, diagnostics);
        AddIfFalse(bindDisplaysAcceptedAlphaProjectionActionLoop, "goal122.winforms.accepted_alpha_projection_action_loop_bind_missing", pageRelativePath, diagnostics);
        AddIfFalse(bindDisplaysGenericGamePackageProjection, "goal123.winforms.generic_gamepackage_projection_bind_missing", pageRelativePath, diagnostics);
        AddIfFalse(bindDisplaysGenericGamePackageLoop, "goal124.winforms.generic_gamepackage_loop_bind_missing", pageRelativePath, diagnostics);
        AddIfFalse(bindDisplaysGenericGamePackageSystems, "goal125.winforms.generic_gamepackage_systems_bind_missing", pageRelativePath, diagnostics);
        AddIfFalse(bindDisplaysGenericGamePackageFullPlaythrough, "goal126.winforms.generic_gamepackage_full_playthrough_bind_missing", pageRelativePath, diagnostics);
        AddIfFalse(bindDisplaysUnityProjectionVerificationRunner, "goal127.winforms.unity_projection_runner_bind_missing", pageRelativePath, diagnostics);
        AddIfFalse(bindDisplaysParameterizedGamePackageRunner, "goal128.winforms.parameterized_gamepackage_runner_bind_missing", pageRelativePath, diagnostics);
        AddIfFalse(bindDisplaysGamePackageCandidateMatrix, "goal129.winforms.gamepackage_candidate_matrix_bind_missing", pageRelativePath, diagnostics);
        AddIfFalse(bindDisplaysGamePackageCandidateFactory, "goal130.winforms.gamepackage_candidate_factory_bind_missing", pageRelativePath, diagnostics);
        return new VisualWorldPreviewWinFormsBindingInventory
        {
            Passed = diagnostics.Count == 0,
            PageControlExists = pageExists,
            DesignerExists = designerExists,
            CompositionRootRegistersService = serviceRegistered,
            CompositionRootRegistersPage = pageRegistered,
            EditorRegistryIncludesPage = registryIncludesPage,
            PageActivationLoadsApplicationResult = activationLoads,
            PageBindDisplaysGroupsEntriesProofs = bindDisplays,
            PageBindDisplaysCacheExports = bindDisplaysCacheExports,
            PageBindDisplaysUnityHandoff = bindDisplaysUnityHandoff,
            PageBindDisplaysGeoworld = bindDisplaysGeoworld,
            PageBindDisplaysOfflineGeoworldHandoff = bindDisplaysOfflineGeoworldHandoff,
            PageBindDisplaysOfflineGeoworldUnityPreview = bindDisplaysOfflineGeoworldUnityPreview,
            PageBindDisplaysOfflineGeoworldUnityEditorPreview = bindDisplaysOfflineGeoworldUnityEditorPreview,
            PageBindDisplaysOfflineGeoworldPlayModeTravel = bindDisplaysOfflineGeoworldPlayModeTravel,
            PageBindDisplaysOfflineGeoworldInteractiveTravel = bindDisplaysOfflineGeoworldInteractiveTravel,
            PageBindDisplaysOfflineGeoworldInteractions = bindDisplaysOfflineGeoworldInteractions,
            PageBindDisplaysOfflineGeoworldSessionReplay = bindDisplaysOfflineGeoworldSessionReplay,
            PageBindDisplaysOfflineGeoworldObjectiveAcceptance = bindDisplaysOfflineGeoworldObjectiveAcceptance,
            PageBindDisplaysOfflineGeoworldAlphaSlice = bindDisplaysOfflineGeoworldAlphaSlice,
            PageBindDisplaysOfflineGeoworldAlphaExportPackage = bindDisplaysOfflineGeoworldAlphaExportPackage,
            PageBindDisplaysOfflineGeoworldAlphaManualAcceptance = bindDisplaysOfflineGeoworldAlphaManualAcceptance,
            PageBindDisplaysOfflineGeoworldAlphaManualResultIntake = bindDisplaysOfflineGeoworldAlphaManualResultIntake,
            PageBindDisplaysOfflineGeoworldAlphaAcceptanceOperatorPack = bindDisplaysOfflineGeoworldAlphaAcceptanceOperatorPack,
            PageBindDisplaysOfflineGeoworldAlphaManualResultWorkbench = bindDisplaysOfflineGeoworldAlphaManualResultWorkbench,
            PageBindDisplaysOfflineGeoworldAlphaHumanResultRevalidation = bindDisplaysOfflineGeoworldAlphaHumanResultRevalidation,
            PageBindDisplaysOfflineGeoworldAlphaManualGateAcceptanceRecord = bindDisplaysOfflineGeoworldAlphaManualGateAcceptanceRecord,
            PageBindDisplaysOfflineGeoworldAlphaPostAcceptanceContinuationSelection = bindDisplaysOfflineGeoworldAlphaPostAcceptanceContinuation,
            PageBindDisplaysOfflineGeoworldAcceptedAlphaBaselineReview = bindDisplaysOfflineGeoworldAcceptedAlphaBaseline,
            PageBindDisplaysAcceptedAlphaUnityPlayableProjection = bindDisplaysAcceptedAlphaUnityPlayableProjection,
            PageBindDisplaysAcceptedAlphaProjectionUsability = bindDisplaysAcceptedAlphaProjectionUsability,
            PageBindDisplaysAcceptedAlphaInteractionDrilldown = bindDisplaysAcceptedAlphaInteractionDrilldown,
            PageBindDisplaysAcceptedAlphaProjectionActionLoop = bindDisplaysAcceptedAlphaProjectionActionLoop,
            PageBindDisplaysGenericGamePackageProjection = bindDisplaysGenericGamePackageProjection,
            PageBindDisplaysGenericGamePackageLoop = bindDisplaysGenericGamePackageLoop,
            PageBindDisplaysGenericGamePackageSystems = bindDisplaysGenericGamePackageSystems,
            PageBindDisplaysGenericGamePackageFullPlaythrough = bindDisplaysGenericGamePackageFullPlaythrough,
            PageBindDisplaysUnityProjectionVerificationRunner = bindDisplaysUnityProjectionVerificationRunner,
            PageBindDisplaysParameterizedGamePackageRunner = bindDisplaysParameterizedGamePackageRunner,
            PageBindDisplaysGamePackageCandidateMatrix = bindDisplaysGamePackageCandidateMatrix,
            PageBindDisplaysGamePackageCandidateFactory = bindDisplaysGamePackageCandidateFactory,
            PageBindDisplaysGamePackageCandidateRecipePipeline = bindDisplaysGamePackageCandidateRecipePipeline,
            PageBindDisplaysCandidatePipelineOperator = bindDisplaysCandidatePipelineOperator,
            PageBindDisplaysCanonicalRuntimeSelectedCandidate = bindDisplaysCanonicalRuntimeSelectedCandidate,
            PageBindDisplaysCanonicalRuntimePlayerLoopReadiness = bindDisplaysCanonicalRuntimePlayerLoop,
            PageBindDisplaysCanonicalRuntimePlayerCommandLoop = bindDisplaysCanonicalRuntimePlayerCommandLoop,
            PageBindDisplaysCanonicalRuntimeUnityPlayerLoopPlayback = bindDisplaysCanonicalRuntimeUnityPlayerLoopPlayback,
            Diagnostics = diagnostics
        };
    }
}
