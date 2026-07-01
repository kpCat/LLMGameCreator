using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignDiagnosticsControl : UserControl
{
    public CampaignDiagnosticsControl()
    {
        InitializeComponent();
    }

    public void Bind(WorkspaceValidationDashboard dashboard, CampaignWorkspaceReport report)
    {
        _summaryLabel.Text = "Validation=" + dashboard.Passed
            + " | status=" + report.ImplementationStatus
            + " | errors=" + dashboard.ErrorCount
            + " | warnings=" + dashboard.WarningCount;
        _diagnosticsListView.BeginUpdate();
        _diagnosticsListView.Items.Clear();
        foreach (var diagnostic in dashboard.Diagnostics)
        {
            var item = new ListViewItem(diagnostic.Severity);
            item.SubItems.Add(diagnostic.Code);
            item.SubItems.Add(diagnostic.Target);
            item.SubItems.Add(diagnostic.Message);
            _diagnosticsListView.Items.Add(item);
        }

        _diagnosticsListView.EndUpdate();
    }
}
