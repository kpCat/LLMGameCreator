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
        new()
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
        };
}
