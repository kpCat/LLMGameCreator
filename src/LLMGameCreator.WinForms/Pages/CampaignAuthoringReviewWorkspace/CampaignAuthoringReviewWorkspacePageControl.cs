using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;
using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;
using LLMGameCreator.Application.Design.EditDrivenPlayablePreviewRefresh;
using LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;
using LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;
using LLMGameCreator.Application.Design.EditDrivenSpineQualityConsolidation;
using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;
using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewPlaythrough;

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

    public CampaignAuthoringReviewWorkspacePageControl()
        : this(
            new SchemaDrivenCampaignWorkspaceEvidenceService(),
            new SchemaDrivenCampaignEditEvidenceService(),
            new EditDrivenPlayablePreviewRefreshEvidenceService(),
            new EditDrivenPlayableReviewPackageMaterializationEvidenceService(),
            new EditDrivenReviewPackagePlayableSessionEvidenceService(),
            new EditDrivenSpineQualityConsolidationEvidenceService(),
            new EditDrivenGamePackageRuntimePreviewBridgeEvidenceService(),
            new EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService())
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
            new EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService())
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
        EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService runtimePreviewPlaythroughService)
    {
        _service = service;
        _editService = editService;
        _playableRefreshService = playableRefreshService;
        _reviewPackageService = reviewPackageService;
        _playSessionService = playSessionService;
        _spineQualityService = spineQualityService;
        _runtimePreviewBridgeService = runtimePreviewBridgeService;
        _runtimePreviewPlaythroughService = runtimePreviewPlaythroughService;
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

        CampaignWorkspaceBuildResult workspaceResult;
        try
        {
            workspaceResult = _service.Build(root);
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Workspace load failed: " + ex.Message;
            return;
        }

        SchemaDrivenCampaignEditBuildResult editResult;
        try
        {
            editResult = _editService.Build(root);
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Edit loop load failed: " + ex.Message;
            return;
        }

        EditDrivenPlayablePreviewRefreshBuildResult refreshResult;
        try
        {
            refreshResult = _playableRefreshService.Build(root);
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Playable refresh load failed: " + ex.Message;
            return;
        }

        EditDrivenPlayableReviewPackageMaterializationBuildResult reviewPackageResult;
        try
        {
            reviewPackageResult = _reviewPackageService.Build(root);
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Review package load failed: " + ex.Message;
            return;
        }

        EditDrivenReviewPackagePlayableSessionBuildResult playSessionResult;
        try
        {
            playSessionResult = _playSessionService.Build(root);
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Review package play session load failed: " + ex.Message;
            return;
        }

        EditDrivenSpineQualityConsolidationBuildResult spineQualityResult;
        try
        {
            spineQualityResult = _spineQualityService.Build(root);
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Spine quality dashboard load failed: " + ex.Message;
            return;
        }

        EditDrivenGamePackageRuntimePreviewBridgeBuildResult runtimePreviewBridgeResult;
        try
        {
            runtimePreviewBridgeResult = _runtimePreviewBridgeService.BuildAndWriteAsync(root).GetAwaiter().GetResult().Result;
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Runtime preview bridge load failed: " + ex.Message;
            return;
        }

        EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult runtimePreviewPlaythroughResult;
        try
        {
            runtimePreviewPlaythroughResult = _runtimePreviewPlaythroughService.BuildAndWriteAsync(root).GetAwaiter().GetResult().Result;
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Runtime preview playthrough load failed: " + ex.Message;
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
            runtimePreviewPlaythroughResult);
    }

    public void Bind(CampaignWorkspaceBuildResult result)
    {
        Bind(result, null, null, null, null, null, null, null);
    }

    public void Bind(CampaignWorkspaceBuildResult result, SchemaDrivenCampaignEditBuildResult? editResult)
    {
        Bind(result, editResult, null, null, null, null, null, null);
    }

    public void Bind(
        CampaignWorkspaceBuildResult result,
        SchemaDrivenCampaignEditBuildResult? editResult,
        EditDrivenPlayablePreviewRefreshBuildResult? refreshResult,
        EditDrivenPlayableReviewPackageMaterializationBuildResult? reviewPackageResult,
        EditDrivenReviewPackagePlayableSessionBuildResult? playSessionResult,
        EditDrivenSpineQualityConsolidationBuildResult? spineQualityResult,
        EditDrivenGamePackageRuntimePreviewBridgeBuildResult? runtimePreviewBridgeResult,
        EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult? runtimePreviewPlaythroughResult)
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
    }

    private void RowSelectorControlSelectedRowIdChanged(object? sender, EventArgs e)
    {
        _editLoopControl.SelectRow(_rowSelectorControl.SelectedRowId);
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
