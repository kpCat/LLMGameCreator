using System.Text.Json;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl : UserControl, IEditorPage
{
    private readonly VisualWorldStreamPreviewWorkspaceService _service;
    private VisualWorldStreamPreviewWorkspaceResult? _result;

    public VisualWorldStreamPreviewWorkspacePageControl()
        : this(new VisualWorldStreamPreviewWorkspaceService())
    {
    }

    public VisualWorldStreamPreviewWorkspacePageControl(
        VisualWorldStreamPreviewWorkspaceService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        InitializeComponent();
        ConfigureControls();
        WireEvents();
    }

    public string Id => "visual-world-stream-preview-workspace";
    public string Title => "Visual World Stream Preview";
    public int SortOrder => 38;
    Control IEditorPage.View => this;

    public void OnActivated()
    {
        RefreshWorkspace();
    }

    public void Bind(VisualWorldStreamPreviewWorkspaceResult result)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
        _statusLabel.Text = BuildStatusText(result);
        BindGroups(result);
        BindProofs(result);
        BindDiagnostics(result);
    }

    private void ConfigureControls()
    {
        _groupsListBox.DisplayMember = nameof(GroupListItem.DisplayText);
    }

    private void WireEvents()
    {
        _refreshButton.Click += (_, _) => RefreshWorkspace();
        _groupsListBox.SelectedIndexChanged += (_, _) => SelectedGroupChanged();
        _entriesListView.SelectedIndexChanged += (_, _) => SelectedEntryChanged();
    }

    private void RefreshWorkspace()
    {
        var root = FindProjectRoot();
        if (root is null)
        {
            _statusLabel.Text = "Repository root was not found.";
            return;
        }

        try
        {
            Bind(_service.Build(root));
        }
        catch (Exception ex) when (ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or JsonException)
        {
            _statusLabel.Text = "Visual world stream preview load failed: " + ex.Message;
        }
    }

    private void BindGroups(VisualWorldStreamPreviewWorkspaceResult result)
    {
        var items = result.Catalog.Groups
            .OrderBy(group => group.GroupId, StringComparer.Ordinal)
            .Select(group => new GroupListItem(
                group.GroupId,
                group.DisplayName + " (" + group.EntryCount + ")",
                group))
            .ToList();
        _groupsListBox.DataSource = items;
        if (items.Count > 0)
        {
            _groupsListBox.SelectedIndex = 0;
        }
        else
        {
            _entriesListView.Items.Clear();
            _detailsTextBox.Clear();
            _svgPreviewTextBox.Clear();
        }
    }

    private void BindProofs(VisualWorldStreamPreviewWorkspaceResult result)
    {
        _proofsListView.BeginUpdate();
        _proofsListView.Items.Clear();
        foreach (var proof in result.ProofStatus.Proofs)
        {
            var item = new ListViewItem(proof.ProofId);
            item.SubItems.Add(proof.Passed ? "passed" : "failed");
            item.SubItems.Add(proof.RelativePath);
            item.SubItems.Add(proof.DiagnosticSummary);
            _proofsListView.Items.Add(item);
        }

        _proofsListView.EndUpdate();
    }

    private void BindDiagnostics(VisualWorldStreamPreviewWorkspaceResult result)
    {
        var lines = new List<string>
        {
            "qualityGatePassed=" + result.QualityGateScan.Passed.ToString().ToLowerInvariant(),
            "winFormsBindingPassed="
                + result.WinFormsBindingInventory.Passed.ToString().ToLowerInvariant(),
            "proofStatusPassed=" + result.ProofStatus.Passed.ToString().ToLowerInvariant(),
            "cacheExportPackageCount=" + result.Report.CacheExportPackageCount,
            "cacheExportRecordCount=" + result.Report.CacheExportRecordCount,
            "runtimeHandoffSidecarVisible="
                + result.Report.RuntimeHandoffSidecarVisible.ToString().ToLowerInvariant(),
            "runtimeHandoffSidecarMetadataOnly="
                + result.Report.RuntimeHandoffSidecarMetadataOnly.ToString().ToLowerInvariant(),
            "cacheReadbackProofPassed="
                + result.Report.CacheReadbackProofPassed.ToString().ToLowerInvariant(),
            "cacheOverlapReuseProofPassed="
                + result.Report.CacheOverlapReuseProofPassed.ToString().ToLowerInvariant(),
            "cacheNegativeProofPassed="
                + result.Report.CacheNegativeProofPassed.ToString().ToLowerInvariant(),
            "unityPayloadFileCount=" + result.Report.UnityPayloadFileCount,
            "unityPackageCount=" + result.Report.UnityPackageCount,
            "unityExportRecordCount=" + result.Report.UnityExportRecordCount,
            "unityStreamWindowCount=" + result.Report.UnityStreamWindowCount,
            "unityUniqueChunkKeyCount=" + result.Report.UnityUniqueChunkKeyCount,
            "unitySimulatedReadProofPassed="
                + result.Report.UnitySimulatedReadProofPassed.ToString().ToLowerInvariant(),
            "unityNegativeProofPassed="
                + result.Report.UnityNegativeProofPassed.ToString().ToLowerInvariant(),
            "unityProbeSourceInventoryPassed="
                + result.Report.UnityProbeSourceInventoryPassed.ToString().ToLowerInvariant(),
            "unityAlphaRuntimeBootstrapUnchanged="
                + result.Report.UnityAlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "unityForbiddenAreasUnchanged="
                + result.Report.UnityForbiddenAreasUnchanged.ToString().ToLowerInvariant(),
            "noUnityFilesChangedByGoal096="
                + result.Report.NoUnityFilesChangedByGoal096.ToString().ToLowerInvariant(),
            "geoworldOfflineBundleId=" + result.Report.GeoworldOfflineBundleId,
            "geoworldNormalizedFeatureCount=" + result.Report.GeoworldNormalizedFeatureCount,
            "geoworldWorldSourceGraphChunkCount=" + result.Report.GeoworldWorldSourceGraphChunkCount,
            "geoworldStreamWindowChunkCount=" + result.Report.GeoworldStreamWindowChunkCount,
            "geoworldBoundaryPrefetchPassed="
                + result.Report.GeoworldBoundaryPrefetchPassed.ToString().ToLowerInvariant(),
            "geoworldNegativeProofPassed="
                + result.Report.GeoworldNegativeProofPassed.ToString().ToLowerInvariant(),
            "geoworldQualityGatePassed="
                + result.Report.GeoworldQualityGatePassed.ToString().ToLowerInvariant(),
            "goal099FilesDiscoveredByRelativePaths="
                + result.Report.Goal099FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant(),
            "offlineGeoworldHandoffPackageCount="
                + result.Report.OfflineGeoworldHandoffPackageCount,
            "offlineGeoworldHandoffFeatureCount="
                + result.Report.OfflineGeoworldHandoffFeatureCount,
            "offlineGeoworldHandoffVisualCacheRecordCount="
                + result.Report.OfflineGeoworldHandoffVisualCacheRecordCount,
            "offlineGeoworldHandoffSourceChunkCount="
                + result.Report.OfflineGeoworldHandoffSourceChunkCount,
            "offlineGeoworldHandoffStreamWindowChunkCount="
                + result.Report.OfflineGeoworldHandoffStreamWindowChunkCount,
            "offlineGeoworldHandoffUnityPayloadFileCount="
                + result.Report.OfflineGeoworldHandoffUnityPayloadFileCount,
            "offlineGeoworldHandoffFeatureKindCounts="
                + result.Report.OfflineGeoworldHandoffFeatureKindCountsSummary,
            "offlineGeoworldHandoffSimulatedReadProofPassed="
                + result.Report.OfflineGeoworldHandoffSimulatedReadProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldHandoffNegativeProofPassed="
                + result.Report.OfflineGeoworldHandoffNegativeProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldHandoffAlphaRuntimeBootstrapUnchanged="
                + result.Report.OfflineGeoworldHandoffAlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "offlineGeoworldHandoffQualityGatePassed="
                + result.Report.OfflineGeoworldHandoffQualityGatePassed.ToString().ToLowerInvariant(),
            "goal100FilesDiscoveredByRelativePaths="
                + result.Report.Goal100FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityPreviewCommandCount="
                + result.Report.OfflineGeoworldUnityPreviewCommandCount,
            "offlineGeoworldUnityPreviewCommandKindCount="
                + result.Report.OfflineGeoworldUnityPreviewCommandKindCount,
            "offlineGeoworldUnityPreviewTravelWindowStepCount="
                + result.Report.OfflineGeoworldUnityPreviewTravelWindowStepCount,
            "offlineGeoworldUnityPreviewUnityPayloadFileCount="
                + result.Report.OfflineGeoworldUnityPreviewUnityPayloadFileCount,
            "offlineGeoworldUnityPreviewKindCoverage="
                + result.Report.OfflineGeoworldUnityPreviewKindCoverageSummary,
            "offlineGeoworldUnityPreviewUnityScriptsReady="
                + result.Report.OfflineGeoworldUnityPreviewUnityScriptsReady.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityPreviewSimulatedCommandProofPassed="
                + result.Report.OfflineGeoworldUnityPreviewSimulatedCommandProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityPreviewNegativeProofPassed="
                + result.Report.OfflineGeoworldUnityPreviewNegativeProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityPreviewAlphaRuntimeBootstrapUnchanged="
                + result.Report.OfflineGeoworldUnityPreviewAlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityPreviewQualityGatePassed="
                + result.Report.OfflineGeoworldUnityPreviewQualityGatePassed.ToString().ToLowerInvariant(),
            "goal101FilesDiscoveredByRelativePaths="
                + result.Report.Goal101FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityEditorPreviewCommandCount="
                + result.Report.OfflineGeoworldUnityEditorPreviewCommandCount,
            "offlineGeoworldUnityEditorPreviewCommandKindCount="
                + result.Report.OfflineGeoworldUnityEditorPreviewCommandKindCount,
            "offlineGeoworldUnityEditorPreviewTravelWindowStepCount="
                + result.Report.OfflineGeoworldUnityEditorPreviewTravelWindowStepCount,
            "offlineGeoworldUnityEditorPreviewExpectedObjectCount="
                + result.Report.OfflineGeoworldUnityEditorPreviewExpectedObjectCount,
            "offlineGeoworldUnityEditorPreviewEditorWindowScriptPath="
                + result.Report.OfflineGeoworldUnityEditorPreviewEditorWindowScriptPath,
            "offlineGeoworldUnityEditorPreviewMenuItemMarker="
                + result.Report.OfflineGeoworldUnityEditorPreviewMenuItemMarker,
            "offlineGeoworldUnityEditorPreviewPayloadPath="
                + result.Report.OfflineGeoworldUnityEditorPreviewPayloadPath,
            "offlineGeoworldUnityEditorPreviewManualInstructions="
                + result.Report.OfflineGeoworldUnityEditorPreviewManualInstructions,
            "offlineGeoworldUnityEditorPreviewToolInventoryPassed="
                + result.Report.OfflineGeoworldUnityEditorPreviewToolInventoryPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityEditorPreviewEditorWindowScriptReady="
                + result.Report.OfflineGeoworldUnityEditorPreviewEditorWindowScriptReady.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityEditorPreviewSimulatedActionProofPassed="
                + result.Report.OfflineGeoworldUnityEditorPreviewSimulatedActionProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityEditorPreviewClearOperationProofPassed="
                + result.Report.OfflineGeoworldUnityEditorPreviewClearOperationProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityEditorPreviewNegativeProofPassed="
                + result.Report.OfflineGeoworldUnityEditorPreviewNegativeProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityEditorPreviewAlphaRuntimeBootstrapUnchanged="
                + result.Report.OfflineGeoworldUnityEditorPreviewAlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityEditorPreviewQualityGatePassed="
                + result.Report.OfflineGeoworldUnityEditorPreviewQualityGatePassed.ToString().ToLowerInvariant(),
            "goal102FilesDiscoveredByRelativePaths="
                + result.Report.Goal102FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant(),
            "offlineGeoworldPlayModeTravelStepCount="
                + result.Report.OfflineGeoworldPlayModeTravelStepCount,
            "offlineGeoworldPlayModeTravelObjectCount="
                + result.Report.OfflineGeoworldPlayModeTravelObjectCount,
            "offlineGeoworldPlayModeTravelActiveChunkCounts="
                + result.Report.OfflineGeoworldPlayModeTravelActiveChunkCounts,
            "offlineGeoworldPlayModeTravelBoundaryPrefetchCounts="
                + result.Report.OfflineGeoworldPlayModeTravelBoundaryPrefetchCounts,
            "offlineGeoworldPlayModeTravelExpectedVisibleObjectCounts="
                + result.Report.OfflineGeoworldPlayModeTravelExpectedVisibleObjectCounts,
            "offlineGeoworldPlayModeTravelUnityScriptsReady="
                + result.Report.OfflineGeoworldPlayModeTravelUnityScriptsReady.ToString().ToLowerInvariant(),
            "offlineGeoworldPlayModeTravelEditorWindowReady="
                + result.Report.OfflineGeoworldPlayModeTravelEditorWindowReady.ToString().ToLowerInvariant(),
            "offlineGeoworldPlayModeTravelSimulatedExecutionProofPassed="
                + result.Report.OfflineGeoworldPlayModeTravelSimulatedExecutionProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldPlayModeTravelNegativeProofPassed="
                + result.Report.OfflineGeoworldPlayModeTravelNegativeProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldPlayModeTravelGoal102BClosureRecorded="
                + result.Report.OfflineGeoworldPlayModeTravelGoal102BClosureRecorded.ToString().ToLowerInvariant(),
            "offlineGeoworldPlayModeTravelAlphaRuntimeBootstrapUnchanged="
                + result.Report.OfflineGeoworldPlayModeTravelAlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "offlineGeoworldPlayModeTravelQualityGatePassed="
                + result.Report.OfflineGeoworldPlayModeTravelQualityGatePassed.ToString().ToLowerInvariant(),
            "goal103FilesDiscoveredByRelativePaths="
                + result.Report.Goal103FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractiveTravelMovementSampleCount="
                + result.Report.OfflineGeoworldInteractiveTravelMovementSampleCount,
            "offlineGeoworldInteractiveTravelBoundaryCrossingCount="
                + result.Report.OfflineGeoworldInteractiveTravelBoundaryCrossingCount,
            "offlineGeoworldInteractiveTravelObjectCount="
                + result.Report.OfflineGeoworldInteractiveTravelObjectCount,
            "offlineGeoworldInteractiveTravelActiveChunkCounts="
                + result.Report.OfflineGeoworldInteractiveTravelActiveChunkCounts,
            "offlineGeoworldInteractiveTravelBoundaryPrefetchCounts="
                + result.Report.OfflineGeoworldInteractiveTravelBoundaryPrefetchCounts,
            "offlineGeoworldInteractiveTravelExpectedVisibleObjectCounts="
                + result.Report.OfflineGeoworldInteractiveTravelExpectedVisibleObjectCounts,
            "offlineGeoworldInteractiveTravelUnityScriptsReady="
                + result.Report.OfflineGeoworldInteractiveTravelUnityScriptsReady.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractiveTravelEditorWindowReady="
                + result.Report.OfflineGeoworldInteractiveTravelEditorWindowReady.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractiveTravelSimulatedExecutionProofPassed="
                + result.Report.OfflineGeoworldInteractiveTravelSimulatedExecutionProofPassed
                    .ToString().ToLowerInvariant(),
            "offlineGeoworldInteractiveTravelNegativeProofPassed="
                + result.Report.OfflineGeoworldInteractiveTravelNegativeProofPassed
                    .ToString().ToLowerInvariant(),
            "offlineGeoworldInteractiveTravelAlphaRuntimeBootstrapUnchanged="
                + result.Report.OfflineGeoworldInteractiveTravelAlphaRuntimeBootstrapUnchanged
                    .ToString().ToLowerInvariant(),
            "offlineGeoworldInteractiveTravelQualityGatePassed="
                + result.Report.OfflineGeoworldInteractiveTravelQualityGatePassed
                    .ToString().ToLowerInvariant(),
            "goal104FilesDiscoveredByRelativePaths="
                + result.Report.Goal104FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionTargetCount="
                + result.Report.OfflineGeoworldInteractionTargetCount,
            "offlineGeoworldInteractionActionKindCount="
                + result.Report.OfflineGeoworldInteractionActionKindCount,
            "offlineGeoworldInteractionActionCount="
                + result.Report.OfflineGeoworldInteractionActionCount,
            "offlineGeoworldInteractionScriptedEventCount="
                + result.Report.OfflineGeoworldInteractionScriptedEventCount,
            "offlineGeoworldInteractionStateDeltaCount="
                + result.Report.OfflineGeoworldInteractionStateDeltaCount,
            "offlineGeoworldInteractionFinalStateHash="
                + result.Report.OfflineGeoworldInteractionFinalStateHash,
            "offlineGeoworldInteractionStateHashChainPassed="
                + result.Report.OfflineGeoworldInteractionStateHashChainPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionUnityScriptsReady="
                + result.Report.OfflineGeoworldInteractionUnityScriptsReady.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionEditorWindowReady="
                + result.Report.OfflineGeoworldInteractionEditorWindowReady.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionUnitySafetyScanPassed="
                + result.Report.OfflineGeoworldInteractionUnitySafetyScanPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionSimulatedSessionProofPassed="
                + result.Report.OfflineGeoworldInteractionSimulatedSessionProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionNegativeProofPassed="
                + result.Report.OfflineGeoworldInteractionNegativeProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionAlphaRuntimeBootstrapUnchanged="
                + result.Report.OfflineGeoworldInteractionAlphaRuntimeBootstrapUnchanged
                    .ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionQualityGatePassed="
                + result.Report.OfflineGeoworldInteractionQualityGatePassed.ToString().ToLowerInvariant(),
            "goal105FilesDiscoveredByRelativePaths="
                + result.Report.Goal105FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant(),
            "offlineGeoworldSessionReplayStepCount="
                + result.Report.OfflineGeoworldSessionReplayStepCount,
            "offlineGeoworldSessionStateDeltaCount="
                + result.Report.OfflineGeoworldSessionStateDeltaCount,
            "offlineGeoworldSessionCheckpointStepIndex="
                + result.Report.OfflineGeoworldSessionCheckpointStepIndex,
            "offlineGeoworldSessionAcceptanceChecklistStepCount="
                + result.Report.OfflineGeoworldSessionAcceptanceChecklistStepCount,
            "offlineGeoworldSessionFinalStateHash="
                + result.Report.OfflineGeoworldSessionFinalStateHash,
            "offlineGeoworldSessionUnityScriptsReady="
                + result.Report.OfflineGeoworldSessionUnityScriptsReady.ToString().ToLowerInvariant(),
            "offlineGeoworldSessionEditorWindowReady="
                + result.Report.OfflineGeoworldSessionEditorWindowReady.ToString().ToLowerInvariant(),
            "offlineGeoworldSessionSimulatedReplayProofPassed="
                + result.Report.OfflineGeoworldSessionSimulatedReplayProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldSessionNegativeProofPassed="
                + result.Report.OfflineGeoworldSessionNegativeProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldSessionAlphaRuntimeBootstrapUnchanged="
                + result.Report.OfflineGeoworldSessionAlphaRuntimeBootstrapUnchanged
                    .ToString().ToLowerInvariant(),
            "offlineGeoworldSessionQualityGatePassed="
                + result.Report.OfflineGeoworldSessionQualityGatePassed.ToString().ToLowerInvariant(),
            "goal106FilesDiscoveredByRelativePaths="
                + result.Report.Goal106FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant(),
            "offlineGeoworldObjectiveCount=" + result.Report.OfflineGeoworldObjectiveCount,
            "offlineGeoworldObjectiveCompletedCount=" + result.Report.OfflineGeoworldObjectiveCompletedCount,
            "offlineGeoworldObjectiveFinalStatus=" + result.Report.OfflineGeoworldObjectiveFinalStatus,
            "offlineGeoworldObjectiveReplaySaveLoadLinkage=" + result.Report.OfflineGeoworldObjectiveReplayAcceptanceProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldObjectiveUnityScriptsReady=" + result.Report.OfflineGeoworldObjectiveUnityScriptsReady.ToString().ToLowerInvariant(),
            "offlineGeoworldObjectiveEditorWindowReady=" + result.Report.OfflineGeoworldObjectiveEditorWindowReady.ToString().ToLowerInvariant(),
            "offlineGeoworldObjectiveAlphaQualityConsolidationPassed=" + result.Report.OfflineGeoworldObjectiveAlphaQualityConsolidationPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldObjectiveManualChecklistSummary=" + result.Report.Goal107FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant(),
            "offlineGeoworldObjectiveAlphaRuntimeBootstrapUnchanged=" + result.Report.OfflineGeoworldObjectiveAlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "noAbsolutePaths=" + result.QualityGateScan.NoAbsolutePaths.ToString().ToLowerInvariant(),
            "noBinaryOrRasterMediaAdded="
                + result.QualityGateScan.NoBinaryOrRasterMediaAdded.ToString().ToLowerInvariant()
        };
        lines.AddRange(BuildOfflineGeoworldAlphaSliceDiagnosticLines(result));
        lines.AddRange(BuildOfflineGeoworldAlphaExportPackageDiagnosticLines(result));
        lines.AddRange(BuildOfflineGeoworldAlphaManualAcceptanceDiagnosticLines(result));
        lines.AddRange(BuildOfflineGeoworldAlphaManualResultIntakeDiagnosticLines(result));
        lines.AddRange(BuildOfflineGeoworldAlphaAcceptanceOperatorDiagnosticLines(result));
        lines.AddRange(BuildOfflineGeoworldAlphaManualResultWorkbenchDiagnosticLines(result));
        lines.AddRange(BuildOfflineGeoworldAlphaHumanResultRevalidationDiagnosticLines(result));
        lines.AddRange(BuildOfflineGeoworldAlphaManualGateAcceptanceDiagnosticLines(result));
        lines.AddRange(BuildOfflineGeoworldAlphaPostAcceptanceDiagnosticLines(result));
        lines.AddRange(BuildOfflineGeoworldAcceptedAlphaBaselineDiagnosticLines(result));
        lines.AddRange(BuildAcceptedAlphaUnityPlayableProjectionDiagnosticLines(result));
        lines.AddRange(result.Diagnostics.Select(diagnostic =>
            diagnostic.Severity + ": " + diagnostic.Code
            + " [" + diagnostic.Target + "] " + diagnostic.Message));
        _diagnosticsTextBox.Text = string.Join(Environment.NewLine, lines);
    }

    private void SelectedGroupChanged()
    {
        if (_groupsListBox.SelectedItem is not GroupListItem selected)
        {
            return;
        }

        _entriesListView.BeginUpdate();
        _entriesListView.Items.Clear();
        foreach (var entry in selected.Group.Entries)
        {
            var item = new ListViewItem(entry.Id);
            item.SubItems.Add(entry.ArtifactKind);
            item.SubItems.Add(entry.Status.ToString());
            item.SubItems.Add(entry.RelativePath);
            item.Tag = entry;
            _entriesListView.Items.Add(item);
        }

        _entriesListView.EndUpdate();
        if (_entriesListView.Items.Count > 0)
        {
            var selectedItem = _entriesListView.Items
                .Cast<ListViewItem>()
                .FirstOrDefault(item =>
                    item.Tag is VisualWorldPreviewArtifactEntry entry
                    && !string.IsNullOrWhiteSpace(entry.TextSvgPreviewPath))
                ?? _entriesListView.Items[0];
            selectedItem.Selected = true;
            if (selectedItem.Tag is VisualWorldPreviewArtifactEntry entry)
            {
                DisplayEntry(entry);
            }
        }
    }

    private void SelectedEntryChanged()
    {
        if (_entriesListView.SelectedItems.Count == 0
            || _entriesListView.SelectedItems[0].Tag is not VisualWorldPreviewArtifactEntry entry)
        {
            return;
        }

        DisplayEntry(entry);
    }

    private void DisplayEntry(VisualWorldPreviewArtifactEntry entry)
    {
        _detailsTextBox.Text = BuildEntryDetails(entry);
        _svgPreviewTextBox.Text = string.IsNullOrWhiteSpace(entry.TextPreview)
            ? "No text SVG preview is attached to the selected entry."
            : entry.TextPreview;
    }

    private static string BuildEntryDetails(VisualWorldPreviewArtifactEntry entry)
    {
        var lines = new List<string>
        {
            "id: " + entry.Id,
            "kind: " + entry.ArtifactKind,
            "sourceGoal: " + entry.SourceGoalId,
            "relativePath: " + entry.RelativePath,
            "sha256: " + entry.Sha256,
            "status: " + entry.Status,
            "diagnosticSummary: " + entry.DiagnosticSummary,
            "textSvgPreviewPath: " + entry.TextSvgPreviewPath,
            "safeRatingMetadataSummary: " + entry.SafeRatingMetadataSummary,
            "exportTargetKind: " + entry.ExportTargetKind,
            "cacheRecordCount: " + entry.CacheRecordCount,
            "sourceChunkCount: " + entry.SourceChunkCount,
            "streamWindowCount: " + entry.StreamWindowCount,
            "runtimeHandoffMetadataOnly: "
                + entry.RuntimeHandoffMetadataOnly.ToString().ToLowerInvariant(),
            "invalidationMatrixPassed: "
                + entry.InvalidationMatrixPassed.ToString().ToLowerInvariant(),
            "readbackProofPassed: " + entry.ReadbackProofPassed.ToString().ToLowerInvariant(),
            "overlapReuseProofPassed: " + entry.OverlapReuseProofPassed.ToString().ToLowerInvariant(),
            "negativeProofPassed: " + entry.NegativeProofPassed.ToString().ToLowerInvariant(),
            "noRawFullWorldDump: " + entry.NoRawFullWorldDump.ToString().ToLowerInvariant(),
            "payloadFileCount: " + entry.PayloadFileCount,
            "packageCount: " + entry.PackageCount,
            "exportRecordCount: " + entry.ExportRecordCount,
            "uniqueChunkKeyCount: " + entry.UniqueChunkKeyCount,
            "simulatedUnityReadProofPassed: "
                + entry.SimulatedUnityReadProofPassed.ToString().ToLowerInvariant(),
            "probeSourceInventoryPassed: "
                + entry.ProbeSourceInventoryPassed.ToString().ToLowerInvariant(),
            "alphaRuntimeBootstrapUnchanged: "
                + entry.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "forbiddenUnityAreasUnchanged: "
                + entry.ForbiddenUnityAreasUnchanged.ToString().ToLowerInvariant(),
            "metadataOnly: " + entry.MetadataOnly.ToString().ToLowerInvariant(),
            "payloadHashesMatchGoal095Ledger: "
                + entry.PayloadHashesMatchGoal095Ledger.ToString().ToLowerInvariant(),
            "offlineBundleId: " + entry.OfflineBundleId,
            "geoworldNormalizedFeatureCount: " + entry.GeoworldNormalizedFeatureCount,
            "geoworldWorldSourceGraphChunkCount: " + entry.GeoworldWorldSourceGraphChunkCount,
            "geoworldStreamWindowChunkCount: " + entry.GeoworldStreamWindowChunkCount,
            "boundaryPrefetchStatus: " + entry.BoundaryPrefetchStatus,
            "featureTaxonomyCoveragePassed: "
                + entry.FeatureTaxonomyCoveragePassed.ToString().ToLowerInvariant(),
            "geoworldNegativeProofPassed: "
                + entry.GeoworldNegativeProofPassed.ToString().ToLowerInvariant(),
            "geoworldQualityGatePassed: "
                + entry.GeoworldQualityGatePassed.ToString().ToLowerInvariant(),
            "compactOverviewEntry: " + entry.CompactOverviewEntry,
            "geoworldVisualCacheRecordCount: " + entry.GeoworldVisualCacheRecordCount,
            "offlineGeoworldHandoffFeatureKindCounts: "
                + entry.OfflineGeoworldHandoffFeatureKindCountsSummary,
            "offlineGeoworldHandoffQualityGatePassed: "
                + entry.OfflineGeoworldHandoffQualityGatePassed.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityPreviewCommandCount: "
                + entry.OfflineGeoworldUnityPreviewCommandCount,
            "offlineGeoworldUnityPreviewCommandKindCount: "
                + entry.OfflineGeoworldUnityPreviewCommandKindCount,
            "offlineGeoworldUnityPreviewTravelWindowStepCount: "
                + entry.OfflineGeoworldUnityPreviewTravelWindowStepCount,
            "offlineGeoworldUnityPreviewKindCoverage: "
                + entry.OfflineGeoworldUnityPreviewKindCoverageSummary,
            "offlineGeoworldUnityPreviewUnityScriptsReady: "
                + entry.OfflineGeoworldUnityPreviewUnityScriptsReady.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityPreviewSimulatedCommandProofPassed: "
                + entry.OfflineGeoworldUnityPreviewSimulatedCommandProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityPreviewQualityGatePassed: "
                + entry.OfflineGeoworldUnityPreviewQualityGatePassed.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityEditorPreviewCommandCount: "
                + entry.OfflineGeoworldUnityEditorPreviewCommandCount,
            "offlineGeoworldUnityEditorPreviewCommandKindCount: "
                + entry.OfflineGeoworldUnityEditorPreviewCommandKindCount,
            "offlineGeoworldUnityEditorPreviewTravelWindowStepCount: "
                + entry.OfflineGeoworldUnityEditorPreviewTravelWindowStepCount,
            "offlineGeoworldUnityEditorPreviewExpectedObjectCount: "
                + entry.OfflineGeoworldUnityEditorPreviewExpectedObjectCount,
            "offlineGeoworldUnityEditorPreviewEditorWindowScriptPath: "
                + entry.OfflineGeoworldUnityEditorPreviewEditorWindowScriptPath,
            "offlineGeoworldUnityEditorPreviewMenuItemMarker: "
                + entry.OfflineGeoworldUnityEditorPreviewMenuItemMarker,
            "offlineGeoworldUnityEditorPreviewPayloadPath: "
                + entry.OfflineGeoworldUnityEditorPreviewPayloadPath,
            "offlineGeoworldUnityEditorPreviewManualInstructions: "
                + entry.OfflineGeoworldUnityEditorPreviewManualInstructions,
            "offlineGeoworldUnityEditorPreviewToolInventoryPassed: "
                + entry.OfflineGeoworldUnityEditorPreviewToolInventoryPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityEditorPreviewEditorWindowScriptReady: "
                + entry.OfflineGeoworldUnityEditorPreviewEditorWindowScriptReady.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityEditorPreviewSimulatedActionProofPassed: "
                + entry.OfflineGeoworldUnityEditorPreviewSimulatedActionProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityEditorPreviewClearOperationProofPassed: "
                + entry.OfflineGeoworldUnityEditorPreviewClearOperationProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityEditorPreviewNegativeProofPassed: "
                + entry.OfflineGeoworldUnityEditorPreviewNegativeProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityEditorPreviewAlphaRuntimeBootstrapUnchanged: "
                + entry.OfflineGeoworldUnityEditorPreviewAlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "offlineGeoworldUnityEditorPreviewQualityGatePassed: "
                + entry.OfflineGeoworldUnityEditorPreviewQualityGatePassed.ToString().ToLowerInvariant(),
            "offlineGeoworldPlayModeTravelStepCount: "
                + entry.OfflineGeoworldPlayModeTravelStepCount,
            "offlineGeoworldPlayModeTravelObjectCount: "
                + entry.OfflineGeoworldPlayModeTravelObjectCount,
            "offlineGeoworldPlayModeTravelActiveChunkCounts: "
                + entry.OfflineGeoworldPlayModeTravelActiveChunkCounts,
            "offlineGeoworldPlayModeTravelBoundaryPrefetchCounts: "
                + entry.OfflineGeoworldPlayModeTravelBoundaryPrefetchCounts,
            "offlineGeoworldPlayModeTravelExpectedVisibleObjectCounts: "
                + entry.OfflineGeoworldPlayModeTravelExpectedVisibleObjectCounts,
            "offlineGeoworldPlayModeTravelUnityScriptsReady: "
                + entry.OfflineGeoworldPlayModeTravelUnityScriptsReady.ToString().ToLowerInvariant(),
            "offlineGeoworldPlayModeTravelEditorWindowReady: "
                + entry.OfflineGeoworldPlayModeTravelEditorWindowReady.ToString().ToLowerInvariant(),
            "offlineGeoworldPlayModeTravelSimulatedExecutionProofPassed: "
                + entry.OfflineGeoworldPlayModeTravelSimulatedExecutionProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldPlayModeTravelNegativeProofPassed: "
                + entry.OfflineGeoworldPlayModeTravelNegativeProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldPlayModeTravelGoal102BClosureRecorded: "
                + entry.OfflineGeoworldPlayModeTravelGoal102BClosureRecorded.ToString().ToLowerInvariant(),
            "offlineGeoworldPlayModeTravelAlphaRuntimeBootstrapUnchanged: "
                + entry.OfflineGeoworldPlayModeTravelAlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "offlineGeoworldPlayModeTravelQualityGatePassed: "
                + entry.OfflineGeoworldPlayModeTravelQualityGatePassed.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractiveTravelMovementSampleCount: "
                + entry.OfflineGeoworldInteractiveTravelMovementSampleCount,
            "offlineGeoworldInteractiveTravelBoundaryCrossingCount: "
                + entry.OfflineGeoworldInteractiveTravelBoundaryCrossingCount,
            "offlineGeoworldInteractiveTravelObjectCount: "
                + entry.OfflineGeoworldInteractiveTravelObjectCount,
            "offlineGeoworldInteractiveTravelActiveChunkCounts: "
                + entry.OfflineGeoworldInteractiveTravelActiveChunkCounts,
            "offlineGeoworldInteractiveTravelBoundaryPrefetchCounts: "
                + entry.OfflineGeoworldInteractiveTravelBoundaryPrefetchCounts,
            "offlineGeoworldInteractiveTravelExpectedVisibleObjectCounts: "
                + entry.OfflineGeoworldInteractiveTravelExpectedVisibleObjectCounts,
            "offlineGeoworldInteractiveTravelUnityScriptsReady: "
                + entry.OfflineGeoworldInteractiveTravelUnityScriptsReady.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractiveTravelEditorWindowReady: "
                + entry.OfflineGeoworldInteractiveTravelEditorWindowReady.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractiveTravelSimulatedExecutionProofPassed: "
                + entry.OfflineGeoworldInteractiveTravelSimulatedExecutionProofPassed
                    .ToString().ToLowerInvariant(),
            "offlineGeoworldInteractiveTravelNegativeProofPassed: "
                + entry.OfflineGeoworldInteractiveTravelNegativeProofPassed
                    .ToString().ToLowerInvariant(),
            "offlineGeoworldInteractiveTravelAlphaRuntimeBootstrapUnchanged: "
                + entry.OfflineGeoworldInteractiveTravelAlphaRuntimeBootstrapUnchanged
                    .ToString().ToLowerInvariant(),
            "offlineGeoworldInteractiveTravelQualityGatePassed: "
                + entry.OfflineGeoworldInteractiveTravelQualityGatePassed.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionTargetCount: "
                + entry.OfflineGeoworldInteractionTargetCount,
            "offlineGeoworldInteractionActionKindCount: "
                + entry.OfflineGeoworldInteractionActionKindCount,
            "offlineGeoworldInteractionActionCount: "
                + entry.OfflineGeoworldInteractionActionCount,
            "offlineGeoworldInteractionScriptedEventCount: "
                + entry.OfflineGeoworldInteractionScriptedEventCount,
            "offlineGeoworldInteractionStateDeltaCount: "
                + entry.OfflineGeoworldInteractionStateDeltaCount,
            "offlineGeoworldInteractionFinalStateHash: "
                + entry.OfflineGeoworldInteractionFinalStateHash,
            "offlineGeoworldInteractionStateHashChainPassed: "
                + entry.OfflineGeoworldInteractionStateHashChainPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionUnityScriptsReady: "
                + entry.OfflineGeoworldInteractionUnityScriptsReady.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionEditorWindowReady: "
                + entry.OfflineGeoworldInteractionEditorWindowReady.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionUnitySafetyScanPassed: "
                + entry.OfflineGeoworldInteractionUnitySafetyScanPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionSimulatedSessionProofPassed: "
                + entry.OfflineGeoworldInteractionSimulatedSessionProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionNegativeProofPassed: "
                + entry.OfflineGeoworldInteractionNegativeProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionAlphaRuntimeBootstrapUnchanged: "
                + entry.OfflineGeoworldInteractionAlphaRuntimeBootstrapUnchanged
                    .ToString().ToLowerInvariant(),
            "offlineGeoworldInteractionQualityGatePassed: "
                + entry.OfflineGeoworldInteractionQualityGatePassed.ToString().ToLowerInvariant(),
            "offlineGeoworldSessionReplayStepCount: " + entry.OfflineGeoworldSessionReplayStepCount,
            "offlineGeoworldSessionStateDeltaCount: " + entry.OfflineGeoworldSessionStateDeltaCount,
            "offlineGeoworldSessionCheckpointStepIndex: " + entry.OfflineGeoworldSessionCheckpointStepIndex,
            "offlineGeoworldSessionAcceptanceChecklistStepCount: " + entry.OfflineGeoworldSessionAcceptanceChecklistStepCount,
            "offlineGeoworldSessionFinalStateHash: " + entry.OfflineGeoworldSessionFinalStateHash,
            "offlineGeoworldSessionUnityScriptsReady: " + entry.OfflineGeoworldSessionUnityScriptsReady.ToString().ToLowerInvariant(),
            "offlineGeoworldSessionEditorWindowReady: " + entry.OfflineGeoworldSessionEditorWindowReady.ToString().ToLowerInvariant(),
            "offlineGeoworldSessionSimulatedReplayProofPassed: " + entry.OfflineGeoworldSessionSimulatedReplayProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldSessionNegativeProofPassed: " + entry.OfflineGeoworldSessionNegativeProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldSessionAlphaRuntimeBootstrapUnchanged: "
                + entry.OfflineGeoworldSessionAlphaRuntimeBootstrapUnchanged
                    .ToString().ToLowerInvariant(),
            "offlineGeoworldSessionQualityGatePassed: " + entry.OfflineGeoworldSessionQualityGatePassed.ToString().ToLowerInvariant(),
            "offlineGeoworldObjectiveCount: " + entry.OfflineGeoworldObjectiveCount,
            "offlineGeoworldObjectiveCompletedCount: " + entry.OfflineGeoworldObjectiveCompletedCount,
            "offlineGeoworldObjectiveFinalStatus: " + entry.OfflineGeoworldObjectiveFinalStatus,
            "offlineGeoworldObjectiveReplaySaveLoadLinkage: " + entry.OfflineGeoworldObjectiveReplayAcceptanceProofPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldObjectiveUnityScriptsReady: " + entry.OfflineGeoworldObjectiveUnityScriptsReady.ToString().ToLowerInvariant(),
            "offlineGeoworldObjectiveEditorWindowReady: " + entry.OfflineGeoworldObjectiveEditorWindowReady.ToString().ToLowerInvariant(),
            "offlineGeoworldObjectiveAlphaQualityConsolidationPassed: " + entry.OfflineGeoworldObjectiveAlphaQualityConsolidationPassed.ToString().ToLowerInvariant(),
            "offlineGeoworldObjectiveManualChecklistSummary: " + entry.OfflineGeoworldObjectiveQualityGatePassed.ToString().ToLowerInvariant(),
            "offlineGeoworldObjectiveAlphaRuntimeBootstrapUnchanged: " + entry.OfflineGeoworldObjectiveAlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "chunkKeys: " + string.Join(",", entry.ChunkKeys)
        };
        lines.AddRange(BuildOfflineGeoworldAlphaSliceEntryLines(entry));
        lines.AddRange(BuildOfflineGeoworldAlphaExportPackageEntryLines(entry));
        lines.AddRange(BuildOfflineGeoworldAlphaManualAcceptanceEntryLines(entry));
        lines.AddRange(BuildOfflineGeoworldAlphaManualResultIntakeEntryLines(entry));
        lines.AddRange(BuildOfflineGeoworldAlphaAcceptanceOperatorEntryLines(entry));
        lines.AddRange(BuildOfflineGeoworldAlphaManualResultWorkbenchEntryLines(entry));
        lines.AddRange(BuildOfflineGeoworldAlphaHumanResultRevalidationEntryLines(entry));
        lines.AddRange(BuildOfflineGeoworldAlphaManualGateAcceptanceEntryLines(entry));
        lines.AddRange(BuildOfflineGeoworldAlphaPostAcceptanceEntryLines(entry));
        lines.AddRange(BuildOfflineGeoworldAcceptedAlphaBaselineEntryLines(entry));
        lines.AddRange(BuildAcceptedAlphaUnityPlayableProjectionEntryLines(entry));
        return string.Join(Environment.NewLine, lines);
    }

    private static string? FindProjectRoot()
    {
        foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var current = start;
            while (!string.IsNullOrWhiteSpace(current))
            {
                if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
                {
                    return current;
                }

                current = Directory.GetParent(current)?.FullName;
            }
        }

        return null;
    }

    private sealed record GroupListItem(
        string GroupId,
        string DisplayText,
        VisualWorldPreviewArtifactGroup Group);
}
