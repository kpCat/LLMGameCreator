using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static string BuildStatusText(VisualWorldStreamPreviewWorkspaceResult result) =>
        "Gate: " + result.Report.ManualGate
        + " required | accepted=false | status=" + result.Report.ImplementationStatus
        + " | groups=" + result.Catalog.GroupCount
        + " | entries=" + result.Catalog.EntryCount
        + " | svg=" + result.Catalog.SvgTextPreviewCount
        + " | cachePackages=" + result.Report.CacheExportPackageCount
        + " | cacheRecords=" + result.Report.CacheExportRecordCount
        + " | unityPayloads=" + result.Report.UnityPayloadFileCount
        + " | unityRecords=" + result.Report.UnityExportRecordCount
        + " | geoworldFeatures=" + result.Report.GeoworldNormalizedFeatureCount
        + " | geoworldChunks=" + result.Report.GeoworldWorldSourceGraphChunkCount
        + " | offlineGeoPackages=" + result.Report.OfflineGeoworldHandoffPackageCount
        + " | offlineGeoRecords=" + result.Report.OfflineGeoworldHandoffVisualCacheRecordCount
        + " | offlinePreviewCommands="
        + result.Report.OfflineGeoworldUnityPreviewCommandCount
        + " | offlineEditorObjects="
        + result.Report.OfflineGeoworldUnityEditorPreviewExpectedObjectCount
        + " | playModeSteps="
        + result.Report.OfflineGeoworldPlayModeTravelStepCount
        + " | playModeObjects="
        + result.Report.OfflineGeoworldPlayModeTravelObjectCount
        + " | interactiveSamples="
        + result.Report.OfflineGeoworldInteractiveTravelMovementSampleCount
        + " | interactiveCrossings="
        + result.Report.OfflineGeoworldInteractiveTravelBoundaryCrossingCount
        + " | interactionTargets="
        + result.Report.OfflineGeoworldInteractionTargetCount
        + " | interactionEvents="
        + result.Report.OfflineGeoworldInteractionScriptedEventCount
        + " | sessionReplaySteps="
        + result.Report.OfflineGeoworldSessionReplayStepCount
        + " | objectiveCount="
        + result.Report.OfflineGeoworldObjectiveCount
        + " | alphaSliceComponents="
        + result.Report.OfflineGeoworldAlphaSliceReadyComponentCount
        + "/"
        + result.Report.OfflineGeoworldAlphaSliceComponentCount
        + " | manualAcceptanceSteps="
        + result.Report.OfflineGeoworldAlphaManualAcceptanceChecklistStepCount;
}
