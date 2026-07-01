using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;
using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignAuthoringReviewWorkspacePageControl : UserControl, IEditorPage
{
    private readonly SchemaDrivenCampaignWorkspaceEvidenceService _service;
    private readonly SchemaDrivenCampaignEditEvidenceService _editService;

    public CampaignAuthoringReviewWorkspacePageControl()
        : this(new SchemaDrivenCampaignWorkspaceEvidenceService(), new SchemaDrivenCampaignEditEvidenceService())
    {
    }

    public CampaignAuthoringReviewWorkspacePageControl(SchemaDrivenCampaignWorkspaceEvidenceService service)
        : this(service, new SchemaDrivenCampaignEditEvidenceService())
    {
    }

    public CampaignAuthoringReviewWorkspacePageControl(
        SchemaDrivenCampaignWorkspaceEvidenceService service,
        SchemaDrivenCampaignEditEvidenceService editService)
    {
        _service = service;
        _editService = editService;
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

        try
        {
            Bind(_service.Build(root), _editService.Build(root));
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Workspace load failed: " + ex.Message;
        }
    }

    public void Bind(CampaignWorkspaceBuildResult result)
    {
        Bind(result, null);
    }

    public void Bind(CampaignWorkspaceBuildResult result, SchemaDrivenCampaignEditBuildResult? editResult)
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
