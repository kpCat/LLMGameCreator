using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;
using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;
using LLMGameCreator.Application.Design.EditDrivenPlayablePreviewRefresh;
using LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;
using LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;
using LLMGameCreator.Application.Design.EditDrivenSpineQualityConsolidation;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignAuthoringReviewWorkspacePageControl : UserControl, IEditorPage
{
    private readonly SchemaDrivenCampaignWorkspaceEvidenceService _service;
    private readonly SchemaDrivenCampaignEditEvidenceService _editService;
    private readonly EditDrivenPlayablePreviewRefreshEvidenceService _playableRefreshService;
    private readonly EditDrivenPlayableReviewPackageMaterializationEvidenceService _reviewPackageService;
    private readonly EditDrivenReviewPackagePlayableSessionEvidenceService _playSessionService;
    private readonly EditDrivenSpineQualityConsolidationEvidenceService _spineQualityService;

    public CampaignAuthoringReviewWorkspacePageControl()
        : this(
            new SchemaDrivenCampaignWorkspaceEvidenceService(),
            new SchemaDrivenCampaignEditEvidenceService(),
            new EditDrivenPlayablePreviewRefreshEvidenceService(),
            new EditDrivenPlayableReviewPackageMaterializationEvidenceService(),
            new EditDrivenReviewPackagePlayableSessionEvidenceService(),
            new EditDrivenSpineQualityConsolidationEvidenceService())
    {
    }

    public CampaignAuthoringReviewWorkspacePageControl(SchemaDrivenCampaignWorkspaceEvidenceService service)
        : this(
            service,
            new SchemaDrivenCampaignEditEvidenceService(),
            new EditDrivenPlayablePreviewRefreshEvidenceService(),
            new EditDrivenPlayableReviewPackageMaterializationEvidenceService(),
            new EditDrivenReviewPackagePlayableSessionEvidenceService(),
            new EditDrivenSpineQualityConsolidationEvidenceService())
    {
    }

    public CampaignAuthoringReviewWorkspacePageControl(
        SchemaDrivenCampaignWorkspaceEvidenceService service,
        SchemaDrivenCampaignEditEvidenceService editService)
        : this(
            service,
            editService,
            new EditDrivenPlayablePreviewRefreshEvidenceService(),
            new EditDrivenPlayableReviewPackageMaterializationEvidenceService(),
            new EditDrivenReviewPackagePlayableSessionEvidenceService(),
            new EditDrivenSpineQualityConsolidationEvidenceService())
    {
    }

    public CampaignAuthoringReviewWorkspacePageControl(
        SchemaDrivenCampaignWorkspaceEvidenceService service,
        SchemaDrivenCampaignEditEvidenceService editService,
        EditDrivenPlayablePreviewRefreshEvidenceService playableRefreshService,
        EditDrivenPlayableReviewPackageMaterializationEvidenceService reviewPackageService,
        EditDrivenReviewPackagePlayableSessionEvidenceService playSessionService)
        : this(
            service,
            editService,
            playableRefreshService,
            reviewPackageService,
            playSessionService,
            new EditDrivenSpineQualityConsolidationEvidenceService())
    {
    }

    public CampaignAuthoringReviewWorkspacePageControl(
        SchemaDrivenCampaignWorkspaceEvidenceService service,
        SchemaDrivenCampaignEditEvidenceService editService,
        EditDrivenPlayablePreviewRefreshEvidenceService playableRefreshService,
        EditDrivenPlayableReviewPackageMaterializationEvidenceService reviewPackageService,
        EditDrivenReviewPackagePlayableSessionEvidenceService playSessionService,
        EditDrivenSpineQualityConsolidationEvidenceService spineQualityService)
    {
        _service = service;
        _editService = editService;
        _playableRefreshService = playableRefreshService;
        _reviewPackageService = reviewPackageService;
        _playSessionService = playSessionService;
        _spineQualityService = spineQualityService;
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

        Bind(workspaceResult, editResult, refreshResult, reviewPackageResult, playSessionResult, spineQualityResult);
    }

    public void Bind(CampaignWorkspaceBuildResult result)
    {
        Bind(result, null, null, null, null, null);
    }

    public void Bind(CampaignWorkspaceBuildResult result, SchemaDrivenCampaignEditBuildResult? editResult)
    {
        Bind(result, editResult, null, null, null, null);
    }

    public void Bind(
        CampaignWorkspaceBuildResult result,
        SchemaDrivenCampaignEditBuildResult? editResult,
        EditDrivenPlayablePreviewRefreshBuildResult? refreshResult)
    {
        Bind(result, editResult, refreshResult, null, null, null);
    }

    public void Bind(
        CampaignWorkspaceBuildResult result,
        SchemaDrivenCampaignEditBuildResult? editResult,
        EditDrivenPlayablePreviewRefreshBuildResult? refreshResult,
        EditDrivenPlayableReviewPackageMaterializationBuildResult? reviewPackageResult)
    {
        Bind(result, editResult, refreshResult, reviewPackageResult, null, null);
    }

    public void Bind(
        CampaignWorkspaceBuildResult result,
        SchemaDrivenCampaignEditBuildResult? editResult,
        EditDrivenPlayablePreviewRefreshBuildResult? refreshResult,
        EditDrivenPlayableReviewPackageMaterializationBuildResult? reviewPackageResult,
        EditDrivenReviewPackagePlayableSessionBuildResult? playSessionResult,
        EditDrivenSpineQualityConsolidationBuildResult? spineQualityResult)
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
