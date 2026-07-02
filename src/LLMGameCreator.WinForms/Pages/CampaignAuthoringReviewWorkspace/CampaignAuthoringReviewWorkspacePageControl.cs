using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;
using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;
using LLMGameCreator.Application.Design.EditDrivenPlayablePreviewRefresh;
using LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;
using LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;
using LLMGameCreator.Application.Design.EditDrivenSpineQualityConsolidation;
using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;
using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewPlaythrough;
using LLMGameCreator.Application.Design.EditDrivenUnityAlphaStreamingAssetsHandoff;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignAuthoringReviewWorkspacePageControl : UserControl, IEditorPage
{
    private readonly SchemaDrivenCampaignWorkspaceEvidenceService _service;
    private readonly SchemaDrivenCampaignEditEvidenceService _editService;
    private readonly EditDrivenPlayablePreviewRefreshEvidenceService _playableRefreshService;
    private readonly EditDrivenPlayableReviewPackageMaterializationEvidenceService _reviewPackageService;
    private readonly EditDrivenReviewPackagePlayableSessionEvidenceService _playSessionService;
    private readonly EditDrivenSpineQualityConsolidationEvidenceService _spineQualityService;
    private readonly EditDrivenGamePackageRuntimePreviewBridgeEvidenceService _runtimePreviewBridgeService;
    private readonly EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService _runtimePreviewPlaythroughService;
    private readonly EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService _unityAlphaStreamingAssetsHandoffService;

    public CampaignAuthoringReviewWorkspacePageControl()
        : this(
            new SchemaDrivenCampaignWorkspaceEvidenceService(),
            new SchemaDrivenCampaignEditEvidenceService(),
            new EditDrivenPlayablePreviewRefreshEvidenceService(),
            new EditDrivenPlayableReviewPackageMaterializationEvidenceService(),
            new EditDrivenReviewPackagePlayableSessionEvidenceService(),
            new EditDrivenSpineQualityConsolidationEvidenceService(),
            new EditDrivenGamePackageRuntimePreviewBridgeEvidenceService(),
            new EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService(),
            new EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService())
    {
    }

    public CampaignAuthoringReviewWorkspacePageControl(SchemaDrivenCampaignWorkspaceEvidenceService service)
        : this(
            service,
            new SchemaDrivenCampaignEditEvidenceService(),
            new EditDrivenPlayablePreviewRefreshEvidenceService(),
            new EditDrivenPlayableReviewPackageMaterializationEvidenceService(),
            new EditDrivenReviewPackagePlayableSessionEvidenceService(),
            new EditDrivenSpineQualityConsolidationEvidenceService(),
            new EditDrivenGamePackageRuntimePreviewBridgeEvidenceService(),
            new EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService(),
            new EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService())
    {
    }

    public CampaignAuthoringReviewWorkspacePageControl(
        SchemaDrivenCampaignWorkspaceEvidenceService service,
        SchemaDrivenCampaignEditEvidenceService editService,
        EditDrivenPlayablePreviewRefreshEvidenceService playableRefreshService,
        EditDrivenPlayableReviewPackageMaterializationEvidenceService reviewPackageService,
        EditDrivenReviewPackagePlayableSessionEvidenceService playSessionService,
        EditDrivenSpineQualityConsolidationEvidenceService spineQualityService,
        EditDrivenGamePackageRuntimePreviewBridgeEvidenceService runtimePreviewBridgeService,
        EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService runtimePreviewPlaythroughService,
        EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService unityAlphaStreamingAssetsHandoffService)
    {
        _service = service;
        _editService = editService;
        _playableRefreshService = playableRefreshService;
        _reviewPackageService = reviewPackageService;
        _playSessionService = playSessionService;
        _spineQualityService = spineQualityService;
        _runtimePreviewBridgeService = runtimePreviewBridgeService;
        _runtimePreviewPlaythroughService = runtimePreviewPlaythroughService;
        _unityAlphaStreamingAssetsHandoffService = unityAlphaStreamingAssetsHandoffService;
        InitializeComponent();
        _rowSelectorControl.SelectedRowIdChanged += RowSelectorControlSelectedRowIdChanged;
    }

    public string Id => "campaign-authoring-review-workspace";
    public string Title => "Campaign Review Workspace";
    public int SortOrder => 37;
    Control IEditorPage.View => this;

    public void OnActivated()
    {
        var root = FindProjectRoot();
        if (root is null)
        {
            _statusLabel.Text = "Repository root was not found.";
            return;
        }

        if (!TryLoad("Workspace", () => _service.Build(root), out var workspaceResult)
            || !TryLoad("Edit loop", () => _editService.Build(root), out var editResult)
            || !TryLoad("Playable refresh", () => _playableRefreshService.Build(root), out var refreshResult)
            || !TryLoad("Review package", () => _reviewPackageService.Build(root), out var reviewPackageResult)
            || !TryLoad("Review package play session", () => _playSessionService.Build(root), out var playSessionResult)
            || !TryLoad("Spine quality dashboard", () => _spineQualityService.Build(root), out var spineQualityResult)
            || !TryLoad(
                "Runtime preview bridge",
                () => _runtimePreviewBridgeService.BuildAndWriteAsync(root).GetAwaiter().GetResult().Result,
                out var runtimePreviewBridgeResult)
            || !TryLoad(
                "Runtime preview playthrough",
                () => _runtimePreviewPlaythroughService.BuildAndWriteAsync(root).GetAwaiter().GetResult().Result,
                out var runtimePreviewPlaythroughResult)
            || !TryLoad(
                "Unity Alpha StreamingAssets handoff",
                () => _unityAlphaStreamingAssetsHandoffService.BuildAndWriteAsync(root).GetAwaiter().GetResult().Result,
                out var unityAlphaStreamingAssetsHandoffResult))
        {
            return;
        }

        Bind(
            workspaceResult,
            editResult,
            refreshResult,
            reviewPackageResult,
            playSessionResult,
            spineQualityResult,
            runtimePreviewBridgeResult,
            runtimePreviewPlaythroughResult,
            unityAlphaStreamingAssetsHandoffResult);
    }

    public void Bind(CampaignWorkspaceBuildResult result)
    {
        Bind(result, null, null, null, null, null, null, null, null);
    }

    public void Bind(CampaignWorkspaceBuildResult result, SchemaDrivenCampaignEditBuildResult? editResult)
    {
        Bind(result, editResult, null, null, null, null, null, null, null);
    }

    public void Bind(
        CampaignWorkspaceBuildResult result,
        SchemaDrivenCampaignEditBuildResult? editResult,
        EditDrivenPlayablePreviewRefreshBuildResult? refreshResult,
        EditDrivenPlayableReviewPackageMaterializationBuildResult? reviewPackageResult,
        EditDrivenReviewPackagePlayableSessionBuildResult? playSessionResult,
        EditDrivenSpineQualityConsolidationBuildResult? spineQualityResult,
        EditDrivenGamePackageRuntimePreviewBridgeBuildResult? runtimePreviewBridgeResult,
        EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult? runtimePreviewPlaythroughResult,
        EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult? unityAlphaStreamingAssetsHandoffResult)
    {
        _statusLabel.Text = "Gate: " + result.Report.ManualGate
            + " required | accepted=false | status=" + result.Report.ImplementationStatus
            + " | rows=" + result.RowSelector.RowCount
            + " | groups=" + result.DynamicSchema.Groups.Count;
        _rowSelectorControl.Bind(result.RowSelector, result.UiBindingContract);
        _schemaGroupControl.Bind(result.DynamicSchema, result.UiBindingContract);
        _diagnosticsControl.Bind(result.ValidationDashboard, result.Report);
        _provenanceControl.Bind(result.ProvenanceLedger);
        _actionPlanControl.Bind(result.ActionPlan);
        _qualityGateControl.Bind(result.QualityGateScan, result.WinFormsControlInventory);
        if (editResult is not null)
        {
            _editLoopControl.Bind(editResult);
            _editLoopControl.SelectRow(_rowSelectorControl.SelectedRowId);
        }

        if (refreshResult is not null)
        {
            _playableRefreshControl.Bind(refreshResult);
        }

        if (reviewPackageResult is not null)
        {
            _reviewPackageControl.Bind(reviewPackageResult);
        }

        if (playSessionResult is not null)
        {
            _playSessionControl.Bind(playSessionResult);
        }

        if (spineQualityResult is not null)
        {
            _spineQualityControl.Bind(spineQualityResult);
        }

        if (runtimePreviewBridgeResult is not null)
        {
            _runtimePreviewBridgeControl.Bind(runtimePreviewBridgeResult);
        }

        if (runtimePreviewPlaythroughResult is not null)
        {
            _runtimePreviewPlaythroughControl.Bind(runtimePreviewPlaythroughResult);
        }

        if (unityAlphaStreamingAssetsHandoffResult is not null)
        {
            _unityAlphaStreamingAssetsHandoffControl.Bind(unityAlphaStreamingAssetsHandoffResult);
        }
    }

    private void RowSelectorControlSelectedRowIdChanged(object? sender, EventArgs e)
    {
        _editLoopControl.SelectRow(_rowSelectorControl.SelectedRowId);
    }

    private bool TryLoad<T>(string label, Func<T> build, out T result)
    {
        try
        {
            result = build();
            return true;
        }
        catch (Exception ex)
        {
            _statusLabel.Text = label + " load failed: " + ex.Message;
            result = default!;
            return false;
        }
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
}
