using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignActionPlanControl : UserControl
{
    public CampaignActionPlanControl()
    {
        InitializeComponent();
    }

    public void Bind(AuthoringActionPlan actionPlan)
    {
        _summaryLabel.Text = "Items=" + actionPlan.Items.Count + " | hash=" + actionPlan.PlanHash;
        _itemsListView.BeginUpdate();
        _itemsListView.Items.Clear();
        foreach (var item in actionPlan.Items)
        {
            var viewItem = new ListViewItem(item.Order.ToString("000"));
            viewItem.SubItems.Add(item.ActionId);
            viewItem.SubItems.Add(item.Category);
            viewItem.SubItems.Add(item.SchemaGroupId);
            viewItem.SubItems.Add(item.ReviewPolicy);
            _itemsListView.Items.Add(viewItem);
        }

        _itemsListView.EndUpdate();
    }
}
