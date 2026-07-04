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
        var designerRelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Designer.cs";
        var compositionRelativePath = "src/LLMGameCreator.WinForms/CompositionRoot.cs";
        var pageText = ReadOptionalText(projectRoot, pageRelativePath);
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
            PageBindDisplaysOfflineGeoworldUnityPreview =
                bindDisplaysOfflineGeoworldUnityPreview,
            PageBindDisplaysOfflineGeoworldUnityEditorPreview =
                bindDisplaysOfflineGeoworldUnityEditorPreview,
            PageBindDisplaysOfflineGeoworldPlayModeTravel =
                bindDisplaysOfflineGeoworldPlayModeTravel,
            PageBindDisplaysOfflineGeoworldInteractiveTravel =
                bindDisplaysOfflineGeoworldInteractiveTravel,
            PageBindDisplaysOfflineGeoworldInteractions =
                bindDisplaysOfflineGeoworldInteractions,
            Diagnostics = diagnostics
        };
    }
}
