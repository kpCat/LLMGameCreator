using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignQualityGateControl : UserControl
{
    public CampaignQualityGateControl()
    {
        InitializeComponent();
    }

    public void Bind(QualityGateScan scan, WinFormsControlInventory inventory)
    {
        _summaryLabel.Text = "Quality=" + scan.Passed
            + " | files=" + scan.ScannedFileCount
            + " | maxLine=" + scan.MaxLineLength
            + " | WinForms=" + inventory.Passed;
        _filesListView.BeginUpdate();
        _filesListView.Items.Clear();
        foreach (var file in scan.Files)
        {
            var item = new ListViewItem(file.RelativePath);
            item.SubItems.Add(file.LineCount.ToString());
            item.SubItems.Add(file.MaxLineLength.ToString());
            item.SubItems.Add(file.LinesOver500Count.ToString());
            item.SubItems.Add(file.MinifiedSourceCandidate ? "minified-candidate" : "source");
            _filesListView.Items.Add(item);
        }

        foreach (var control in inventory.Controls)
        {
            var item = new ListViewItem(control.RelativePath);
            item.SubItems.Add(control.SeparateUserControl ? "user-control" : "missing-user-control");
            item.SubItems.Add(control.SchemaDrivenBinding ? "schema-driven" : "not-schema-driven");
            item.SubItems.Add(control.ControlName);
            item.SubItems.Add(control.ControlRole);
            _filesListView.Items.Add(item);
        }

        _filesListView.EndUpdate();
    }
}
