using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignProvenanceControl : UserControl
{
    public CampaignProvenanceControl()
    {
        InitializeComponent();
    }

    public void Bind(ReviewProvenanceLedger ledger)
    {
        _summaryLabel.Text = "Categories=" + string.Join(",", ledger.Categories)
            + " | entries=" + ledger.Entries.Count;
        _provenanceListView.BeginUpdate();
        _provenanceListView.Items.Clear();
        foreach (var entry in ledger.Entries)
        {
            var item = new ListViewItem(entry.Category);
            item.SubItems.Add(entry.SourceGoal);
            item.SubItems.Add(entry.ReviewState);
            item.SubItems.Add(entry.AcceptedByReview ? "accepted" : "not-accepted");
            item.SubItems.Add(entry.HasReviewProvenance ? "review-provenance" : "missing-provenance");
            item.SubItems.Add(entry.EvidenceRef);
            _provenanceListView.Items.Add(item);
        }

        _provenanceListView.EndUpdate();
    }
}
