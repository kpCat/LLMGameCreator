namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport BuildReport(
        VisualWorldStreamPreviewCatalog catalog,
        VisualWorldStreamPreviewProofStatusDocument proofStatus,
        VisualWorldPreviewWinFormsBindingInventory binding,
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        string catalogJson,
        string proofStatusJson,
        string bindingJson,
        string qualityJson,
        string sourceHealthJson) =>
        WithGoal130ReportFields(WithGoal129ReportFields(WithGoal128ReportFields(WithGoal127ReportFields(WithGoal126ReportFields(WithGoal125ReportFields(new()
        {
            Accepted = false,
            GroupCount = catalog.GroupCount,
            EntryCount = catalog.EntryCount,
            SvgTextPreviewCount = catalog.SvgTextPreviewCount,
            Goal091StreamWindowEntryCount = qualityGate.Goal091StreamWindowEntryCount,
            CacheExportPackageCount = qualityGate.CacheExportPackageCount,
            CacheExportRecordCount = qualityGate.CacheExportRecordCount,
            CacheExportSourceChunkCount = qualityGate.CacheExportSourceChunkCount,
            CacheExportStreamWindowCount = qualityGate.CacheExportStreamWindowCount,
            RuntimeHandoffSidecarVisible = qualityGate.RuntimeHandoffSidecarVisible,
            RuntimeHandoffSidecarMetadataOnly = qualityGate.RuntimeHandoffSidecarMetadataOnly,
            CacheReadbackProofPassed = qualityGate.CacheReadbackProofPassed,
            CacheOverlapReuseProofPassed = qualityGate.CacheOverlapReuseProofPassed,
            CacheNegativeProofPassed = qualityGate.CacheNegativeProofPassed,
            CacheInvalidationMatrixPassed = qualityGate.CacheInvalidationMatrixPassed,
            CacheNoRawFullWorldDump = qualityGate.CacheNoRawFullWorldDump,
            UnityPayloadFileCount = qualityGate.UnityPayloadFileCount,
            UnityPackageCount = qualityGate.UnityPackageCount,
            UnityExportRecordCount = qualityGate.UnityExportRecordCount,
            UnityStreamWindowCount = qualityGate.UnityStreamWindowCount,
            UnityUniqueChunkKeyCount = qualityGate.UnityUniqueChunkKeyCount,
            UnityProbeSourceInventoryVisible = qualityGate.UnityProbeSourceInventoryVisible,
            UnityProbeSourceInventoryPassed = qualityGate.UnityProbeSourceInventoryPassed,
            UnitySimulatedReadProofPassed = qualityGate.UnitySimulatedReadProofPassed,
            UnityNegativeProofPassed = qualityGate.UnityNegativeProofPassed,
            UnityAlphaRuntimeBootstrapUnchanged = qualityGate.UnityAlphaRuntimeBootstrapUnchanged,
            UnityForbiddenAreasUnchanged = qualityGate.UnityForbiddenAreasUnchanged,
            UnityHandoffMetadataOnly = qualityGate.UnityHandoffMetadataOnly,
            UnityPayloadHashesMatchGoal095Ledger = qualityGate.UnityPayloadHashesMatchGoal095Ledger,
            Goal095FilesDiscoveredByRelativePaths = qualityGate.Goal095FilesDiscoveredByRelativePaths,
            NoUnityFilesChangedByGoal096 = qualityGate.NoUnityFilesChangedByGoal096,
            GeoworldOfflineBundleId = qualityGate.GeoworldOfflineBundleId,
            GeoworldNormalizedFeatureCount = qualityGate.GeoworldNormalizedFeatureCount,
            GeoworldWorldSourceGraphChunkCount = qualityGate.GeoworldWorldSourceGraphChunkCount,
            GeoworldStreamWindowChunkCount = qualityGate.GeoworldStreamWindowChunkCount,
            GeoworldBoundaryPrefetchPassed = qualityGate.GeoworldBoundaryPrefetchPassed,
            GeoworldNegativeProofPassed = qualityGate.GeoworldNegativeProofPassed,
            GeoworldQualityGatePassed = qualityGate.GeoworldQualityGatePassed,
            Goal099FilesDiscoveredByRelativePaths = qualityGate.Goal099FilesDiscoveredByRelativePaths,
            OfflineGeoworldHandoffPackageCount = qualityGate.OfflineGeoworldHandoffPackageCount,
            OfflineGeoworldHandoffFeatureCount = qualityGate.OfflineGeoworldHandoffFeatureCount,
            OfflineGeoworldHandoffVisualCacheRecordCount =
                qualityGate.OfflineGeoworldHandoffVisualCacheRecordCount,
            OfflineGeoworldHandoffSourceChunkCount =
                qualityGate.OfflineGeoworldHandoffSourceChunkCount,
            OfflineGeoworldHandoffStreamWindowChunkCount =
                qualityGate.OfflineGeoworldHandoffStreamWindowChunkCount,
            OfflineGeoworldHandoffUnityPayloadFileCount =
                qualityGate.OfflineGeoworldHandoffUnityPayloadFileCount,
            OfflineGeoworldHandoffFeatureKindCountsSummary =
                qualityGate.OfflineGeoworldHandoffFeatureKindCountsSummary,
            OfflineGeoworldHandoffSimulatedReadProofPassed =
                qualityGate.OfflineGeoworldHandoffSimulatedReadProofPassed,
            OfflineGeoworldHandoffNegativeProofPassed =
                qualityGate.OfflineGeoworldHandoffNegativeProofPassed,
            OfflineGeoworldHandoffAlphaRuntimeBootstrapUnchanged =
                qualityGate.OfflineGeoworldHandoffAlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldHandoffQualityGatePassed =
                qualityGate.OfflineGeoworldHandoffQualityGatePassed,
            Goal100FilesDiscoveredByRelativePaths = qualityGate.Goal100FilesDiscoveredByRelativePaths,
            OfflineGeoworldUnityPreviewCommandCount =
                qualityGate.OfflineGeoworldUnityPreviewCommandCount,
            OfflineGeoworldUnityPreviewCommandKindCount =
                qualityGate.OfflineGeoworldUnityPreviewCommandKindCount,
            OfflineGeoworldUnityPreviewTravelWindowStepCount =
                qualityGate.OfflineGeoworldUnityPreviewTravelWindowStepCount,
            OfflineGeoworldUnityPreviewUnityPayloadFileCount =
                qualityGate.OfflineGeoworldUnityPreviewUnityPayloadFileCount,
            OfflineGeoworldUnityPreviewKindCoverageSummary =
                qualityGate.OfflineGeoworldUnityPreviewKindCoverageSummary,
            OfflineGeoworldUnityPreviewUnityScriptsReady =
                qualityGate.OfflineGeoworldUnityPreviewUnityScriptsReady,
            OfflineGeoworldUnityPreviewSimulatedCommandProofPassed =
                qualityGate.OfflineGeoworldUnityPreviewSimulatedCommandProofPassed,
            OfflineGeoworldUnityPreviewNegativeProofPassed =
                qualityGate.OfflineGeoworldUnityPreviewNegativeProofPassed,
            OfflineGeoworldUnityPreviewAlphaRuntimeBootstrapUnchanged =
                qualityGate.OfflineGeoworldUnityPreviewAlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldUnityPreviewQualityGatePassed =
                qualityGate.OfflineGeoworldUnityPreviewQualityGatePassed,
            Goal101FilesDiscoveredByRelativePaths = qualityGate.Goal101FilesDiscoveredByRelativePaths,
            OfflineGeoworldUnityEditorPreviewCommandCount =
                qualityGate.OfflineGeoworldUnityEditorPreviewCommandCount,
            OfflineGeoworldUnityEditorPreviewCommandKindCount =
                qualityGate.OfflineGeoworldUnityEditorPreviewCommandKindCount,
            OfflineGeoworldUnityEditorPreviewTravelWindowStepCount =
                qualityGate.OfflineGeoworldUnityEditorPreviewTravelWindowStepCount,
            OfflineGeoworldUnityEditorPreviewExpectedObjectCount =
                qualityGate.OfflineGeoworldUnityEditorPreviewExpectedObjectCount,
            OfflineGeoworldUnityEditorPreviewEditorWindowScriptPath =
                qualityGate.OfflineGeoworldUnityEditorPreviewEditorWindowScriptPath,
            OfflineGeoworldUnityEditorPreviewMenuItemMarker =
                qualityGate.OfflineGeoworldUnityEditorPreviewMenuItemMarker,
            OfflineGeoworldUnityEditorPreviewPayloadPath =
                qualityGate.OfflineGeoworldUnityEditorPreviewPayloadPath,
            OfflineGeoworldUnityEditorPreviewManualInstructions =
                qualityGate.OfflineGeoworldUnityEditorPreviewManualInstructions,
            OfflineGeoworldUnityEditorPreviewToolInventoryPassed =
                qualityGate.OfflineGeoworldUnityEditorPreviewToolInventoryPassed,
            OfflineGeoworldUnityEditorPreviewEditorWindowScriptReady =
                qualityGate.OfflineGeoworldUnityEditorPreviewEditorWindowScriptReady,
            OfflineGeoworldUnityEditorPreviewSimulatedActionProofPassed =
                qualityGate.OfflineGeoworldUnityEditorPreviewSimulatedActionProofPassed,
            OfflineGeoworldUnityEditorPreviewClearOperationProofPassed =
                qualityGate.OfflineGeoworldUnityEditorPreviewClearOperationProofPassed,
            OfflineGeoworldUnityEditorPreviewNegativeProofPassed =
                qualityGate.OfflineGeoworldUnityEditorPreviewNegativeProofPassed,
            OfflineGeoworldUnityEditorPreviewAlphaRuntimeBootstrapUnchanged =
                qualityGate.OfflineGeoworldUnityEditorPreviewAlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldUnityEditorPreviewQualityGatePassed =
                qualityGate.OfflineGeoworldUnityEditorPreviewQualityGatePassed,
            Goal102FilesDiscoveredByRelativePaths = qualityGate.Goal102FilesDiscoveredByRelativePaths,
            OfflineGeoworldPlayModeTravelStepCount =
                qualityGate.OfflineGeoworldPlayModeTravelStepCount,
            OfflineGeoworldPlayModeTravelObjectCount =
                qualityGate.OfflineGeoworldPlayModeTravelObjectCount,
            OfflineGeoworldPlayModeTravelActiveChunkCounts =
                qualityGate.OfflineGeoworldPlayModeTravelActiveChunkCounts,
            OfflineGeoworldPlayModeTravelBoundaryPrefetchCounts =
                qualityGate.OfflineGeoworldPlayModeTravelBoundaryPrefetchCounts,
            OfflineGeoworldPlayModeTravelExpectedVisibleObjectCounts =
                qualityGate.OfflineGeoworldPlayModeTravelExpectedVisibleObjectCounts,
            OfflineGeoworldPlayModeTravelUnityScriptsReady =
                qualityGate.OfflineGeoworldPlayModeTravelUnityScriptsReady,
            OfflineGeoworldPlayModeTravelEditorWindowReady =
                qualityGate.OfflineGeoworldPlayModeTravelEditorWindowReady,
            OfflineGeoworldPlayModeTravelSimulatedExecutionProofPassed =
                qualityGate.OfflineGeoworldPlayModeTravelSimulatedExecutionProofPassed,
            OfflineGeoworldPlayModeTravelNegativeProofPassed =
                qualityGate.OfflineGeoworldPlayModeTravelNegativeProofPassed,
            OfflineGeoworldPlayModeTravelGoal102BClosureRecorded =
                qualityGate.OfflineGeoworldPlayModeTravelGoal102BClosureRecorded,
            OfflineGeoworldPlayModeTravelAlphaRuntimeBootstrapUnchanged =
                qualityGate.OfflineGeoworldPlayModeTravelAlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldPlayModeTravelQualityGatePassed =
                qualityGate.OfflineGeoworldPlayModeTravelQualityGatePassed,
            Goal103FilesDiscoveredByRelativePaths = qualityGate.Goal103FilesDiscoveredByRelativePaths,
            OfflineGeoworldInteractiveTravelMovementSampleCount =
                qualityGate.OfflineGeoworldInteractiveTravelMovementSampleCount,
            OfflineGeoworldInteractiveTravelBoundaryCrossingCount =
                qualityGate.OfflineGeoworldInteractiveTravelBoundaryCrossingCount,
            OfflineGeoworldInteractiveTravelObjectCount =
                qualityGate.OfflineGeoworldInteractiveTravelObjectCount,
            OfflineGeoworldInteractiveTravelActiveChunkCounts =
                qualityGate.OfflineGeoworldInteractiveTravelActiveChunkCounts,
            OfflineGeoworldInteractiveTravelBoundaryPrefetchCounts =
                qualityGate.OfflineGeoworldInteractiveTravelBoundaryPrefetchCounts,
            OfflineGeoworldInteractiveTravelExpectedVisibleObjectCounts =
                qualityGate.OfflineGeoworldInteractiveTravelExpectedVisibleObjectCounts,
            OfflineGeoworldInteractiveTravelUnityScriptsReady =
                qualityGate.OfflineGeoworldInteractiveTravelUnityScriptsReady,
            OfflineGeoworldInteractiveTravelEditorWindowReady =
                qualityGate.OfflineGeoworldInteractiveTravelEditorWindowReady,
            OfflineGeoworldInteractiveTravelSimulatedExecutionProofPassed =
                qualityGate.OfflineGeoworldInteractiveTravelSimulatedExecutionProofPassed,
            OfflineGeoworldInteractiveTravelNegativeProofPassed =
                qualityGate.OfflineGeoworldInteractiveTravelNegativeProofPassed,
            OfflineGeoworldInteractiveTravelAlphaRuntimeBootstrapUnchanged =
                qualityGate.OfflineGeoworldInteractiveTravelAlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldInteractiveTravelQualityGatePassed =
                qualityGate.OfflineGeoworldInteractiveTravelQualityGatePassed,
            Goal104FilesDiscoveredByRelativePaths = qualityGate.Goal104FilesDiscoveredByRelativePaths,
            OfflineGeoworldInteractionTargetCount =
                qualityGate.OfflineGeoworldInteractionTargetCount,
            OfflineGeoworldInteractionActionKindCount =
                qualityGate.OfflineGeoworldInteractionActionKindCount,
            OfflineGeoworldInteractionActionCount =
                qualityGate.OfflineGeoworldInteractionActionCount,
            OfflineGeoworldInteractionScriptedEventCount =
                qualityGate.OfflineGeoworldInteractionScriptedEventCount,
            OfflineGeoworldInteractionStateDeltaCount =
                qualityGate.OfflineGeoworldInteractionStateDeltaCount,
            OfflineGeoworldInteractionFinalStateHash =
                qualityGate.OfflineGeoworldInteractionFinalStateHash,
            OfflineGeoworldInteractionStateHashChainPassed =
                qualityGate.OfflineGeoworldInteractionStateHashChainPassed,
            OfflineGeoworldInteractionUnityScriptsReady =
                qualityGate.OfflineGeoworldInteractionUnityScriptsReady,
            OfflineGeoworldInteractionEditorWindowReady =
                qualityGate.OfflineGeoworldInteractionEditorWindowReady,
            OfflineGeoworldInteractionUnitySafetyScanPassed =
                qualityGate.OfflineGeoworldInteractionUnitySafetyScanPassed,
            OfflineGeoworldInteractionSimulatedSessionProofPassed =
                qualityGate.OfflineGeoworldInteractionSimulatedSessionProofPassed,
            OfflineGeoworldInteractionNegativeProofPassed =
                qualityGate.OfflineGeoworldInteractionNegativeProofPassed,
            OfflineGeoworldInteractionAlphaRuntimeBootstrapUnchanged =
                qualityGate.OfflineGeoworldInteractionAlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldInteractionQualityGatePassed =
                qualityGate.OfflineGeoworldInteractionQualityGatePassed,
            Goal105FilesDiscoveredByRelativePaths =
                qualityGate.Goal105FilesDiscoveredByRelativePaths,
            OfflineGeoworldSessionReplayStepCount =
                qualityGate.OfflineGeoworldSessionReplayStepCount,
            OfflineGeoworldSessionStateDeltaCount =
                qualityGate.OfflineGeoworldSessionStateDeltaCount,
            OfflineGeoworldSessionCheckpointStepIndex =
                qualityGate.OfflineGeoworldSessionCheckpointStepIndex,
            OfflineGeoworldSessionAcceptanceChecklistStepCount =
                qualityGate.OfflineGeoworldSessionAcceptanceChecklistStepCount,
            OfflineGeoworldSessionFinalStateHash =
                qualityGate.OfflineGeoworldSessionFinalStateHash,
            OfflineGeoworldSessionUnityScriptsReady =
                qualityGate.OfflineGeoworldSessionUnityScriptsReady,
            OfflineGeoworldSessionEditorWindowReady =
                qualityGate.OfflineGeoworldSessionEditorWindowReady,
            OfflineGeoworldSessionSimulatedReplayProofPassed =
                qualityGate.OfflineGeoworldSessionSimulatedReplayProofPassed,
            OfflineGeoworldSessionNegativeProofPassed =
                qualityGate.OfflineGeoworldSessionNegativeProofPassed,
            OfflineGeoworldSessionAlphaRuntimeBootstrapUnchanged =
                qualityGate.OfflineGeoworldSessionAlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldSessionQualityGatePassed =
                qualityGate.OfflineGeoworldSessionQualityGatePassed,
            Goal106FilesDiscoveredByRelativePaths =
                qualityGate.Goal106FilesDiscoveredByRelativePaths,
            OfflineGeoworldObjectiveCount = qualityGate.OfflineGeoworldObjectiveCount,
            OfflineGeoworldObjectiveCompletedCount = qualityGate.OfflineGeoworldObjectiveCompletedCount,
            OfflineGeoworldObjectivePayloadFileCount = qualityGate.OfflineGeoworldObjectivePayloadFileCount,
            OfflineGeoworldObjectiveReplayStepCount = qualityGate.OfflineGeoworldObjectiveReplayStepCount,
            OfflineGeoworldObjectiveStateDeltaCount = qualityGate.OfflineGeoworldObjectiveStateDeltaCount,
            OfflineGeoworldObjectiveCheckpointStepIndex =
                qualityGate.OfflineGeoworldObjectiveCheckpointStepIndex,
            OfflineGeoworldObjectiveFinalStatus = qualityGate.OfflineGeoworldObjectiveFinalStatus,
            OfflineGeoworldObjectiveFinalStateHash = qualityGate.OfflineGeoworldObjectiveFinalStateHash,
            OfflineGeoworldObjectiveUnityScriptsReady = qualityGate.OfflineGeoworldObjectiveUnityScriptsReady,
            OfflineGeoworldObjectiveEditorWindowReady = qualityGate.OfflineGeoworldObjectiveEditorWindowReady,
            OfflineGeoworldObjectiveReplayAcceptanceProofPassed =
                qualityGate.OfflineGeoworldObjectiveReplayAcceptanceProofPassed,
            OfflineGeoworldObjectiveNegativeProofPassed =
                qualityGate.OfflineGeoworldObjectiveNegativeProofPassed,
            OfflineGeoworldObjectiveAlphaQualityConsolidationPassed =
                qualityGate.OfflineGeoworldObjectiveAlphaQualityConsolidationPassed,
            OfflineGeoworldObjectiveAlphaRuntimeBootstrapUnchanged =
                qualityGate.OfflineGeoworldObjectiveAlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldObjectiveQualityGatePassed = qualityGate.OfflineGeoworldObjectiveQualityGatePassed,
            Goal107FilesDiscoveredByRelativePaths = qualityGate.Goal107FilesDiscoveredByRelativePaths,
            OfflineGeoworldAlphaSliceComponentCount =
                qualityGate.OfflineGeoworldAlphaSliceComponentCount,
            OfflineGeoworldAlphaSliceReadyComponentCount =
                qualityGate.OfflineGeoworldAlphaSliceReadyComponentCount,
            OfflineGeoworldAlphaSliceObjectiveCount =
                qualityGate.OfflineGeoworldAlphaSliceObjectiveCount,
            OfflineGeoworldAlphaSliceCompletedObjectiveCount =
                qualityGate.OfflineGeoworldAlphaSliceCompletedObjectiveCount,
            OfflineGeoworldAlphaSliceFinalStatus =
                qualityGate.OfflineGeoworldAlphaSliceFinalStatus,
            OfflineGeoworldAlphaSliceUnityToolReady =
                qualityGate.OfflineGeoworldAlphaSliceUnityToolReady,
            OfflineGeoworldAlphaSliceAcceptanceRunbookReady =
                qualityGate.OfflineGeoworldAlphaSliceAcceptanceRunbookReady,
            OfflineGeoworldAlphaSliceFinalProofPassed =
                qualityGate.OfflineGeoworldAlphaSliceFinalProofPassed,
            OfflineGeoworldAlphaSliceNegativeProofPassed =
                qualityGate.OfflineGeoworldAlphaSliceNegativeProofPassed,
            OfflineGeoworldAlphaSliceAlphaRuntimeBootstrapUnchanged =
                qualityGate.OfflineGeoworldAlphaSliceAlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldAlphaSliceQualityGatePassed =
                qualityGate.OfflineGeoworldAlphaSliceQualityGatePassed,
            Goal108FilesDiscoveredByRelativePaths =
                qualityGate.Goal108FilesDiscoveredByRelativePaths,
            OfflineGeoworldAlphaExportPackageFileCount =
                qualityGate.OfflineGeoworldAlphaExportPackageFileCount,
            OfflineGeoworldAlphaExportIndexedFileCount =
                qualityGate.OfflineGeoworldAlphaExportIndexedFileCount,
            OfflineGeoworldAlphaExportChecksumStatus =
                qualityGate.OfflineGeoworldAlphaExportChecksumStatus,
            OfflineGeoworldAlphaExportCleanImportProofPassed =
                qualityGate.OfflineGeoworldAlphaExportCleanImportProofPassed,
            OfflineGeoworldAlphaExportNegativeProofPassed =
                qualityGate.OfflineGeoworldAlphaExportNegativeProofPassed,
            OfflineGeoworldAlphaExportUnityVerifierReady =
                qualityGate.OfflineGeoworldAlphaExportUnityVerifierReady,
            OfflineGeoworldAlphaExportEditorWindowReady =
                qualityGate.OfflineGeoworldAlphaExportEditorWindowReady,
            OfflineGeoworldAlphaExportWorkspaceBindingPassed =
                qualityGate.OfflineGeoworldAlphaExportWorkspaceBindingPassed,
            OfflineGeoworldAlphaExportSourceLineagePassed =
                qualityGate.OfflineGeoworldAlphaExportSourceLineagePassed,
            OfflineGeoworldAlphaExportRunbookSummary =
                qualityGate.OfflineGeoworldAlphaExportRunbookSummary,
            OfflineGeoworldAlphaExportAcceptanceGateStatus =
                qualityGate.OfflineGeoworldAlphaExportAcceptanceGateStatus,
            OfflineGeoworldAlphaExportAlphaRuntimeBootstrapUnchanged =
                qualityGate.OfflineGeoworldAlphaExportAlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldAlphaExportQualityGatePassed =
                qualityGate.OfflineGeoworldAlphaExportQualityGatePassed,
            Goal109FilesDiscoveredByRelativePaths =
                qualityGate.Goal109FilesDiscoveredByRelativePaths,
            OfflineGeoworldAlphaManualAcceptanceChecklistStepCount =
                qualityGate.OfflineGeoworldAlphaManualAcceptanceChecklistStepCount,
            OfflineGeoworldAlphaManualAcceptancePayloadFileCount =
                qualityGate.OfflineGeoworldAlphaManualAcceptancePayloadFileCount,
            OfflineGeoworldAlphaManualAcceptanceExportFileCount =
                qualityGate.OfflineGeoworldAlphaManualAcceptanceExportFileCount,
            OfflineGeoworldAlphaManualAcceptanceAutomatedGatePassed =
                qualityGate.OfflineGeoworldAlphaManualAcceptanceAutomatedGatePassed,
            OfflineGeoworldAlphaManualAcceptanceManualPending =
                qualityGate.OfflineGeoworldAlphaManualAcceptanceManualPending,
            OfflineGeoworldAlphaManualAcceptanceUnityRunnerReady =
                qualityGate.OfflineGeoworldAlphaManualAcceptanceUnityRunnerReady,
            OfflineGeoworldAlphaManualAcceptanceSimulatedProofPassed =
                qualityGate.OfflineGeoworldAlphaManualAcceptanceSimulatedProofPassed,
            OfflineGeoworldAlphaManualAcceptanceNegativeProofPassed =
                qualityGate.OfflineGeoworldAlphaManualAcceptanceNegativeProofPassed,
            OfflineGeoworldAlphaManualAcceptanceWorkspaceBindingPassed =
                qualityGate.OfflineGeoworldAlphaManualAcceptanceWorkspaceBindingPassed,
            OfflineGeoworldAlphaManualAcceptanceAlphaRuntimeBootstrapUnchanged =
                qualityGate.OfflineGeoworldAlphaManualAcceptanceAlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldAlphaManualAcceptanceQualityGatePassed =
                qualityGate.OfflineGeoworldAlphaManualAcceptanceQualityGatePassed,
            OfflineGeoworldAlphaManualAcceptanceResultTemplatePath =
                qualityGate.OfflineGeoworldAlphaManualAcceptanceResultTemplatePath,
            OfflineGeoworldAlphaManualAcceptanceReleaseRiskLinks =
                qualityGate.OfflineGeoworldAlphaManualAcceptanceReleaseRiskLinks,
            OfflineGeoworldAlphaManualAcceptanceMilestoneGateLinks =
                qualityGate.OfflineGeoworldAlphaManualAcceptanceMilestoneGateLinks,
            Goal110FilesDiscoveredByRelativePaths =
                qualityGate.Goal110FilesDiscoveredByRelativePaths,
            OfflineGeoworldAlphaManualResultIntakeGoal110PackagePresent =
                qualityGate.OfflineGeoworldAlphaManualResultIntakeGoal110PackagePresent,
            OfflineGeoworldAlphaManualResultIntakeResultFilePresent =
                qualityGate.OfflineGeoworldAlphaManualResultIntakeResultFilePresent,
            OfflineGeoworldAlphaManualResultIntakeDecisionStatus =
                qualityGate.OfflineGeoworldAlphaManualResultIntakeDecisionStatus,
            OfflineGeoworldAlphaManualResultIntakeAcceptableCandidate =
                qualityGate.OfflineGeoworldAlphaManualResultIntakeAcceptableCandidate,
            OfflineGeoworldAlphaManualResultIntakeAcceptedByCodex =
                qualityGate.OfflineGeoworldAlphaManualResultIntakeAcceptedByCodex,
            OfflineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired =
                qualityGate.OfflineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired,
            OfflineGeoworldAlphaManualResultIntakeChecklistHashMatched =
                qualityGate.OfflineGeoworldAlphaManualResultIntakeChecklistHashMatched,
            OfflineGeoworldAlphaManualResultIntakePassedStepCount =
                qualityGate.OfflineGeoworldAlphaManualResultIntakePassedStepCount,
            OfflineGeoworldAlphaManualResultIntakeFailedStepCount =
                qualityGate.OfflineGeoworldAlphaManualResultIntakeFailedStepCount,
            OfflineGeoworldAlphaManualResultIntakePendingStepCount =
                qualityGate.OfflineGeoworldAlphaManualResultIntakePendingStepCount,
            OfflineGeoworldAlphaManualResultIntakeSkippedStepCount =
                qualityGate.OfflineGeoworldAlphaManualResultIntakeSkippedStepCount,
            OfflineGeoworldAlphaManualResultIntakeMissingStepCount =
                qualityGate.OfflineGeoworldAlphaManualResultIntakeMissingStepCount,
            OfflineGeoworldAlphaManualResultIntakeDuplicateStepCount =
                qualityGate.OfflineGeoworldAlphaManualResultIntakeDuplicateStepCount,
            OfflineGeoworldAlphaManualResultIntakeQualityGatePassed =
                qualityGate.OfflineGeoworldAlphaManualResultIntakeQualityGatePassed,
            Goal111FilesDiscoveredByRelativePaths =
                qualityGate.Goal111FilesDiscoveredByRelativePaths,
            OfflineGeoworldAlphaAcceptanceOperatorStatus =
                qualityGate.OfflineGeoworldAlphaAcceptanceOperatorStatus,
            OfflineGeoworldAlphaAcceptanceOperatorGoal111DecisionStatus =
                qualityGate.OfflineGeoworldAlphaAcceptanceOperatorGoal111DecisionStatus,
            OfflineGeoworldAlphaAcceptanceOperatorManualResultPresent =
                qualityGate.OfflineGeoworldAlphaAcceptanceOperatorManualResultPresent,
            OfflineGeoworldAlphaAcceptanceOperatorManualResultAvailableForHumanReview =
                qualityGate.OfflineGeoworldAlphaAcceptanceOperatorManualResultAvailableForHumanReview,
            OfflineGeoworldAlphaAcceptanceOperatorAcceptedByCodex =
                qualityGate.OfflineGeoworldAlphaAcceptanceOperatorAcceptedByCodex,
            OfflineGeoworldAlphaAcceptanceOperatorHumanAcceptanceStillRequired =
                qualityGate.OfflineGeoworldAlphaAcceptanceOperatorHumanAcceptanceStillRequired,
            OfflineGeoworldAlphaAcceptanceOperatorChecklistStepCount =
                qualityGate.OfflineGeoworldAlphaAcceptanceOperatorChecklistStepCount,
            OfflineGeoworldAlphaAcceptanceOperatorChecklistHashPresent =
                qualityGate.OfflineGeoworldAlphaAcceptanceOperatorChecklistHashPresent,
            OfflineGeoworldAlphaAcceptanceOperatorQualityGatePassed =
                qualityGate.OfflineGeoworldAlphaAcceptanceOperatorQualityGatePassed,
            Goal112FilesDiscoveredByRelativePaths =
                qualityGate.Goal112FilesDiscoveredByRelativePaths,
            OfflineGeoworldAlphaManualResultWorkbenchStatus =
                qualityGate.OfflineGeoworldAlphaManualResultWorkbenchStatus,
            OfflineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus =
                qualityGate.OfflineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus,
            OfflineGeoworldAlphaManualResultWorkbenchGoal112OperatorStatus =
                qualityGate.OfflineGeoworldAlphaManualResultWorkbenchGoal112OperatorStatus,
            OfflineGeoworldAlphaManualResultWorkbenchManualResultPresent =
                qualityGate.OfflineGeoworldAlphaManualResultWorkbenchManualResultPresent,
            OfflineGeoworldAlphaManualResultWorkbenchAcceptedByCodex =
                qualityGate.OfflineGeoworldAlphaManualResultWorkbenchAcceptedByCodex,
            OfflineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired =
                qualityGate.OfflineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired,
            OfflineGeoworldAlphaManualResultWorkbenchDraftTemplateOnly =
                qualityGate.OfflineGeoworldAlphaManualResultWorkbenchDraftTemplateOnly,
            OfflineGeoworldAlphaManualResultWorkbenchChecklistStepCount =
                qualityGate.OfflineGeoworldAlphaManualResultWorkbenchChecklistStepCount,
            OfflineGeoworldAlphaManualResultWorkbenchChecklistHashPresent =
                qualityGate.OfflineGeoworldAlphaManualResultWorkbenchChecklistHashPresent,
            OfflineGeoworldAlphaManualResultWorkbenchQualityGatePassed =
                qualityGate.OfflineGeoworldAlphaManualResultWorkbenchQualityGatePassed,
            Goal113FilesDiscoveredByRelativePaths =
                qualityGate.Goal113FilesDiscoveredByRelativePaths,
            OfflineGeoworldAlphaHumanResultRevalidationDecisionStatus =
                qualityGate.OfflineGeoworldAlphaHumanResultRevalidationDecisionStatus,
            OfflineGeoworldAlphaHumanResultRevalidationGoal111DecisionStatus =
                qualityGate.OfflineGeoworldAlphaHumanResultRevalidationGoal111DecisionStatus,
            OfflineGeoworldAlphaHumanResultRevalidationManualResultPresent =
                qualityGate.OfflineGeoworldAlphaHumanResultRevalidationManualResultPresent,
            OfflineGeoworldAlphaHumanResultRevalidationManualResultJsonValid =
                qualityGate.OfflineGeoworldAlphaHumanResultRevalidationManualResultJsonValid,
            OfflineGeoworldAlphaHumanResultRevalidationAcceptableCandidate =
                qualityGate.OfflineGeoworldAlphaHumanResultRevalidationAcceptableCandidate,
            OfflineGeoworldAlphaHumanResultRevalidationRecommendedHumanDecision =
                qualityGate.OfflineGeoworldAlphaHumanResultRevalidationRecommendedHumanDecision,
            OfflineGeoworldAlphaHumanResultRevalidationAcceptedByCodex =
                qualityGate.OfflineGeoworldAlphaHumanResultRevalidationAcceptedByCodex,
            OfflineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired =
                qualityGate.OfflineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired,
            OfflineGeoworldAlphaHumanResultRevalidationManualGateRemainsHumanDecision =
                qualityGate.OfflineGeoworldAlphaHumanResultRevalidationManualGateRemainsHumanDecision,
            OfflineGeoworldAlphaHumanResultRevalidationRequiredStepCount =
                qualityGate.OfflineGeoworldAlphaHumanResultRevalidationRequiredStepCount,
            OfflineGeoworldAlphaHumanResultRevalidationPassedStepCount =
                qualityGate.OfflineGeoworldAlphaHumanResultRevalidationPassedStepCount,
            OfflineGeoworldAlphaHumanResultRevalidationManualInputNotCommitted =
                qualityGate.OfflineGeoworldAlphaHumanResultRevalidationManualInputNotCommitted,
            OfflineGeoworldAlphaHumanResultRevalidationQualityGatePassed =
                qualityGate.OfflineGeoworldAlphaHumanResultRevalidationQualityGatePassed,
            Goal115FilesDiscoveredByRelativePaths =
                qualityGate.Goal115FilesDiscoveredByRelativePaths,
            OfflineGeoworldAlphaManualGateAcceptanceManualGate =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceManualGate,
            OfflineGeoworldAlphaManualGateAcceptanceManualGateStatus =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceManualGateStatus,
            OfflineGeoworldAlphaManualGateAcceptanceHumanAccepted =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceHumanAccepted,
            OfflineGeoworldAlphaManualGateAcceptanceHumanDecisionStatement =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceHumanDecisionStatement,
            OfflineGeoworldAlphaManualGateAcceptanceSourceDecisionStatus =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceSourceDecisionStatus,
            OfflineGeoworldAlphaManualGateAcceptanceManualResultSha256 =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceManualResultSha256,
            OfflineGeoworldAlphaManualGateAcceptanceAcceptedByCodex =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceAcceptedByCodex,
            OfflineGeoworldAlphaManualGateAcceptanceManualInputNotCommitted =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceManualInputNotCommitted,
            OfflineGeoworldAlphaManualGateAcceptanceRawManualResultEmbeddedInArtifacts =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceRawManualResultEmbeddedInArtifacts,
            OfflineGeoworldAlphaManualGateAcceptanceRecommendedNextDecision =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceRecommendedNextDecision,
            OfflineGeoworldAlphaManualGateAcceptanceNotFinalReleaseOrRuntimeBuild =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceNotFinalReleaseOrRuntimeBuild,
            OfflineGeoworldAlphaManualGateAcceptanceNoRuntimeProviderOrNetworkChanges =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceNoRuntimeProviderOrNetworkChanges,
            OfflineGeoworldAlphaManualGateAcceptanceNoUnityFileChangesRequired =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceNoUnityFileChangesRequired,
            OfflineGeoworldAlphaManualGateAcceptanceRequiredStepCount =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceRequiredStepCount,
            OfflineGeoworldAlphaManualGateAcceptancePassedStepCount =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptancePassedStepCount,
            OfflineGeoworldAlphaManualGateAcceptanceQualityGatePassed =
                qualityGate.OfflineGeoworldAlphaManualGateAcceptanceQualityGatePassed,
            Goal116FilesDiscoveredByRelativePaths =
                qualityGate.Goal116FilesDiscoveredByRelativePaths,
            OfflineGeoworldAlphaPostAcceptanceManualGateStatus =
                qualityGate.OfflineGeoworldAlphaPostAcceptanceManualGateStatus,
            OfflineGeoworldAlphaPostAcceptanceHumanAccepted =
                qualityGate.OfflineGeoworldAlphaPostAcceptanceHumanAccepted,
            OfflineGeoworldAlphaPostAcceptanceManualResultSha256 =
                qualityGate.OfflineGeoworldAlphaPostAcceptanceManualResultSha256,
            OfflineGeoworldAlphaPostAcceptanceRecommendedNextLane =
                qualityGate.OfflineGeoworldAlphaPostAcceptanceRecommendedNextLane,
            OfflineGeoworldAlphaPostAcceptanceRecommendedNextGoalId =
                qualityGate.OfflineGeoworldAlphaPostAcceptanceRecommendedNextGoalId,
            OfflineGeoworldAlphaPostAcceptanceReadyLaneCount =
                qualityGate.OfflineGeoworldAlphaPostAcceptanceReadyLaneCount,
            OfflineGeoworldAlphaPostAcceptanceCandidateLaneCount =
                qualityGate.OfflineGeoworldAlphaPostAcceptanceCandidateLaneCount,
            OfflineGeoworldAlphaPostAcceptanceBlockedLaneCount =
                qualityGate.OfflineGeoworldAlphaPostAcceptanceBlockedLaneCount,
            OfflineGeoworldAlphaPostAcceptanceDoNotStartAutomatically =
                qualityGate.OfflineGeoworldAlphaPostAcceptanceDoNotStartAutomatically,
            OfflineGeoworldAlphaPostAcceptanceQualityGatePassed =
                qualityGate.OfflineGeoworldAlphaPostAcceptanceQualityGatePassed,
            Goal117FilesDiscoveredByRelativePaths =
                qualityGate.Goal117FilesDiscoveredByRelativePaths,
            OfflineGeoworldAcceptedAlphaBaselineId =
                qualityGate.OfflineGeoworldAcceptedAlphaBaselineId,
            OfflineGeoworldAcceptedAlphaBaselineHash =
                qualityGate.OfflineGeoworldAcceptedAlphaBaselineHash,
            OfflineGeoworldAcceptedAlphaBaselineReady =
                qualityGate.OfflineGeoworldAcceptedAlphaBaselineReady,
            OfflineGeoworldAcceptedAlphaManualGateStatus =
                qualityGate.OfflineGeoworldAcceptedAlphaManualGateStatus,
            OfflineGeoworldAcceptedAlphaRecommendedNextDecision =
                qualityGate.OfflineGeoworldAcceptedAlphaRecommendedNextDecision,
            OfflineGeoworldAcceptedAlphaIncludedSourceGoalCount =
                qualityGate.OfflineGeoworldAcceptedAlphaIncludedSourceGoalCount,
            OfflineGeoworldAcceptedAlphaAcceptedEvidenceRootCount =
                qualityGate.OfflineGeoworldAcceptedAlphaAcceptedEvidenceRootCount,
            OfflineGeoworldAcceptedAlphaProducedOnlyRootCount =
                qualityGate.OfflineGeoworldAcceptedAlphaProducedOnlyRootCount,
            OfflineGeoworldAcceptedAlphaBlockedOrSupersededNoteCount =
                qualityGate.OfflineGeoworldAcceptedAlphaBlockedOrSupersededNoteCount,
            OfflineGeoworldAcceptedAlphaDoNotStartAutomatically =
                qualityGate.OfflineGeoworldAcceptedAlphaDoNotStartAutomatically,
            OfflineGeoworldAcceptedAlphaQualityGatePassed =
                qualityGate.OfflineGeoworldAcceptedAlphaQualityGatePassed,
            Goal118FilesDiscoveredByRelativePaths =
                qualityGate.Goal118FilesDiscoveredByRelativePaths,
            AcceptedAlphaUnityPlayableProjectionStatus =
                qualityGate.AcceptedAlphaUnityPlayableProjectionStatus,
            AcceptedAlphaUnityPlayableProjectionUnityMenuPath =
                qualityGate.AcceptedAlphaUnityPlayableProjectionUnityMenuPath,
            AcceptedAlphaUnityPlayableProjectionBaselineId =
                qualityGate.AcceptedAlphaUnityPlayableProjectionBaselineId,
            AcceptedAlphaUnityPlayableProjectionAcceptedBaselineReady =
                qualityGate.AcceptedAlphaUnityPlayableProjectionAcceptedBaselineReady,
            AcceptedAlphaUnityPlayableProjectionGeneratedRootName =
                qualityGate.AcceptedAlphaUnityPlayableProjectionGeneratedRootName,
            AcceptedAlphaUnityPlayableProjectionScriptInventoryCount =
                qualityGate.AcceptedAlphaUnityPlayableProjectionScriptInventoryCount,
            AcceptedAlphaUnityPlayableProjectionSmokePlanStepCount =
                qualityGate.AcceptedAlphaUnityPlayableProjectionSmokePlanStepCount,
            AcceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean =
                qualityGate.AcceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean,
            AcceptedAlphaUnityPlayableProjectionDoNotStartAutomatically =
                qualityGate.AcceptedAlphaUnityPlayableProjectionDoNotStartAutomatically,
            AcceptedAlphaUnityPlayableProjectionQualityGatePassed =
                qualityGate.AcceptedAlphaUnityPlayableProjectionQualityGatePassed,
            Goal119FilesDiscoveredByRelativePaths =
                qualityGate.Goal119FilesDiscoveredByRelativePaths,
            AcceptedAlphaProjectionUsabilityStatus =
                qualityGate.AcceptedAlphaProjectionUsabilityStatus,
            AcceptedAlphaProjectionUsabilityUnityMenuPath =
                qualityGate.AcceptedAlphaProjectionUsabilityUnityMenuPath,
            AcceptedAlphaProjectionUsabilityCleanupScriptPath =
                qualityGate.AcceptedAlphaProjectionUsabilityCleanupScriptPath,
            AcceptedAlphaProjectionUsabilityCleanupScriptCmdPath =
                qualityGate.AcceptedAlphaProjectionUsabilityCleanupScriptCmdPath,
            AcceptedAlphaProjectionUsabilityLegendPresent =
                qualityGate.AcceptedAlphaProjectionUsabilityLegendPresent,
            AcceptedAlphaProjectionUsabilityMarkerDescriptorPresent =
                qualityGate.AcceptedAlphaProjectionUsabilityMarkerDescriptorPresent,
            AcceptedAlphaProjectionUsabilitySelectionControlsPresent =
                qualityGate.AcceptedAlphaProjectionUsabilitySelectionControlsPresent,
            AcceptedAlphaProjectionUsabilityFocusCameraControlPresent =
                qualityGate.AcceptedAlphaProjectionUsabilityFocusCameraControlPresent,
            AcceptedAlphaProjectionUsabilityMaterialWarningGuardPresent =
                qualityGate.AcceptedAlphaProjectionUsabilityMaterialWarningGuardPresent,
            AcceptedAlphaProjectionUsabilityUnitySmokeStatus =
                qualityGate.AcceptedAlphaProjectionUsabilityUnitySmokeStatus,
            AcceptedAlphaProjectionUsabilityDoNotStartAutomatically =
                qualityGate.AcceptedAlphaProjectionUsabilityDoNotStartAutomatically,
            AcceptedAlphaProjectionUsabilityQualityGatePassed =
                qualityGate.AcceptedAlphaProjectionUsabilityQualityGatePassed,
            Goal120FilesDiscoveredByRelativePaths =
                qualityGate.Goal120FilesDiscoveredByRelativePaths,
            AcceptedAlphaInteractionDrilldownFullVerificationStatus =
                qualityGate.AcceptedAlphaInteractionDrilldownFullVerificationStatus,
            AcceptedAlphaInteractionDrilldownUnityMenuPath =
                qualityGate.AcceptedAlphaInteractionDrilldownUnityMenuPath,
            AcceptedAlphaInteractionDrilldownOneClickButtonPresent =
                qualityGate.AcceptedAlphaInteractionDrilldownOneClickButtonPresent,
            AcceptedAlphaInteractionDrilldownDrilldownFieldsPresent =
                qualityGate.AcceptedAlphaInteractionDrilldownDrilldownFieldsPresent,
            AcceptedAlphaInteractionDrilldownInteractionPreviewPresent =
                qualityGate.AcceptedAlphaInteractionDrilldownInteractionPreviewPresent,
            AcceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent =
                qualityGate.AcceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent,
            AcceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker =
                qualityGate.AcceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker,
            AcceptedAlphaInteractionDrilldownCleanupScriptAvailable =
                qualityGate.AcceptedAlphaInteractionDrilldownCleanupScriptAvailable,
            AcceptedAlphaInteractionDrilldownMaterialWarningGuardPresent =
                qualityGate.AcceptedAlphaInteractionDrilldownMaterialWarningGuardPresent,
            AcceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton =
                qualityGate.AcceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton,
            AcceptedAlphaInteractionDrilldownUnityBatchmodeLogStatus =
                qualityGate.AcceptedAlphaInteractionDrilldownUnityBatchmodeLogStatus,
            AcceptedAlphaInteractionDrilldownQualityGatePassed =
                qualityGate.AcceptedAlphaInteractionDrilldownQualityGatePassed,
            Goal121FilesDiscoveredByRelativePaths =
                qualityGate.Goal121FilesDiscoveredByRelativePaths,
            AcceptedAlphaProjectionActionLoopStatus =
                qualityGate.AcceptedAlphaProjectionActionLoopStatus,
            AcceptedAlphaProjectionActionLoopWindowPolishStatus =
                qualityGate.AcceptedAlphaProjectionActionLoopWindowPolishStatus,
            AcceptedAlphaProjectionActionLoopUnityMenuPath =
                qualityGate.AcceptedAlphaProjectionActionLoopUnityMenuPath,
            AcceptedAlphaProjectionActionLoopOneClickVerificationStillPresent =
                qualityGate.AcceptedAlphaProjectionActionLoopOneClickVerificationStillPresent,
            AcceptedAlphaProjectionActionLoopProjectionActionPreviewPresent =
                qualityGate.AcceptedAlphaProjectionActionLoopProjectionActionPreviewPresent,
            AcceptedAlphaProjectionActionLoopProjectionActionApplyPresent =
                qualityGate.AcceptedAlphaProjectionActionLoopProjectionActionApplyPresent,
            AcceptedAlphaProjectionActionLoopProjectionStateResetPresent =
                qualityGate.AcceptedAlphaProjectionActionLoopProjectionStateResetPresent,
            AcceptedAlphaProjectionActionLoopWindowLayoutPolishPresent =
                qualityGate.AcceptedAlphaProjectionActionLoopWindowLayoutPolishPresent,
            AcceptedAlphaProjectionActionLoopUnitySmokeStatus =
                qualityGate.AcceptedAlphaProjectionActionLoopUnitySmokeStatus,
            AcceptedAlphaProjectionActionLoopCleanupScriptAvailable =
                qualityGate.AcceptedAlphaProjectionActionLoopCleanupScriptAvailable,
            AcceptedAlphaProjectionActionLoopQualityGatePassed =
                qualityGate.AcceptedAlphaProjectionActionLoopQualityGatePassed,
            Goal122FilesDiscoveredByRelativePaths =
                qualityGate.Goal122FilesDiscoveredByRelativePaths,
            GenericProjectionStatus =
                qualityGate.GenericProjectionStatus,
            GenericProjectionSamplePackagePath =
                qualityGate.GenericProjectionSamplePackagePath,
            GenericProjectionPackageId =
                qualityGate.GenericProjectionPackageId,
            GenericProjectionPackageTitle =
                qualityGate.GenericProjectionPackageTitle,
            GenericProjectionMapId =
                qualityGate.GenericProjectionMapId,
            GenericProjectionMapSize =
                qualityGate.GenericProjectionMapSize,
            GenericProjectionEntityCount =
                qualityGate.GenericProjectionEntityCount,
            GenericProjectionItemCount =
                qualityGate.GenericProjectionItemCount,
            GenericProjectionUnitySmokeStatus =
                qualityGate.GenericProjectionUnitySmokeStatus,
            GenericProjectionGoal122StillGreen =
                qualityGate.GenericProjectionGoal122StillGreen,
            GenericProjectionCleanupScriptAvailable =
                qualityGate.GenericProjectionCleanupScriptAvailable,
            GenericGamePackageProjectionQualityGatePassed =
                qualityGate.GenericGamePackageProjectionQualityGatePassed,
            Goal123FilesDiscoveredByRelativePaths =
                qualityGate.Goal123FilesDiscoveredByRelativePaths,
            GenericLoopStatus =
                qualityGate.GenericLoopStatus,
            GenericLoopSamplePackagePath =
                qualityGate.GenericLoopSamplePackagePath,
            GenericLoopPackageId =
                qualityGate.GenericLoopPackageId,
            GenericLoopMapId =
                qualityGate.GenericLoopMapId,
            GenericLoopInteractionPreviewPresent =
                qualityGate.GenericLoopInteractionPreviewPresent,
            GenericLoopInteractionApplyPassed =
                qualityGate.GenericLoopInteractionApplyPassed,
            GenericLoopDialogueSummaryPresent =
                qualityGate.GenericLoopDialogueSummaryPresent,
            GenericLoopQuestObjectiveSummaryPresent =
                qualityGate.GenericLoopQuestObjectiveSummaryPresent,
            GenericLoopInventorySummaryPresent =
                qualityGate.GenericLoopInventorySummaryPresent,
            GenericLoopResourceSummaryPresent =
                qualityGate.GenericLoopResourceSummaryPresent,
            GenericLoopUnitySmokeStatus =
                qualityGate.GenericLoopUnitySmokeStatus,
            GenericLoopCleanupScriptAvailable =
                qualityGate.GenericLoopCleanupScriptAvailable,
            GenericLoopCleanupCommand =
                qualityGate.GenericLoopCleanupCommand,
            GenericLoopGoal123StillGreen =
                qualityGate.GenericLoopGoal123StillGreen,
            GenericLoopProjectionOnly =
                qualityGate.GenericLoopProjectionOnly,
            GenericLoopAppliedInteractionCount =
                qualityGate.GenericLoopAppliedInteractionCount,
            GenericLoopStartedQuestCount =
                qualityGate.GenericLoopStartedQuestCount,
            GenericLoopEvidencePath =
                qualityGate.GenericLoopEvidencePath,
            GenericLoopExportPath =
                qualityGate.GenericLoopExportPath,
            GenericGamePackageLoopQualityGatePassed =
                qualityGate.GenericGamePackageLoopQualityGatePassed,
            Goal124FilesDiscoveredByRelativePaths =
                qualityGate.Goal124FilesDiscoveredByRelativePaths,
            ProofStatusPassed = proofStatus.Passed,
            WinFormsBindingPassed = binding.Passed,
            QualityGatePassed = qualityGate.Passed,
            SourceHealthPassed = qualityGate.SourceHealthPassed,
            WorkspaceServiceLogicalLineCount = qualityGate.WorkspaceServiceLogicalLineCount,
            MaxLogicalLineCount = qualityGate.MaxLogicalLineCount,
            FilesOver1000LogicalLinesCount = qualityGate.FilesOver1000LogicalLinesCount,
            FilesOver700LogicalLinesInGoal092NamespaceCount =
                qualityGate.FilesOver700LogicalLinesInGoal092NamespaceCount,
            CatalogHash = Sha256Text(catalogJson),
            ProofStatusHash = Sha256Text(proofStatusJson),
            WinFormsBindingInventoryHash = Sha256Text(bindingJson),
            QualityGateHash = Sha256Text(qualityJson),
            DeterministicReportHash = Sha256Text(sourceHealthJson)
        }, qualityGate), qualityGate), qualityGate), qualityGate), qualityGate), qualityGate);
}
