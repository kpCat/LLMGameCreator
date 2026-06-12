using LLMGameCreator.Application.Design;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratorLibraryIssuesTabControl : UserControl
{
    public GeneratorLibraryIssuesTabControl()
    {
        InitializeComponent();
    }

    public void SetIssues(IReadOnlyList<GeneratorLibraryImportIssue> issues)
    {
        _issuesListView.Items.Clear();
        foreach (var issue in issues)
        {
            var item = new ListViewItem(issue.Severity);
            item.SubItems.Add(issue.Code);
            item.SubItems.Add(issue.Message);
            item.SubItems.Add(issue.Target);
            _issuesListView.Items.Add(item);
        }
    }
}
