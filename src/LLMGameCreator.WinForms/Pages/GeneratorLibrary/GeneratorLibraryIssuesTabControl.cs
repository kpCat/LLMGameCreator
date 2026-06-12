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
        var latestImportId = issues
            .Select(issue => issue.ImportId)
            .Where(importId => !string.IsNullOrWhiteSpace(importId))
            .OrderByDescending(importId => importId, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

        foreach (var issue in issues)
        {
            var item = new ListViewItem(string.Equals(issue.ImportId, latestImportId, StringComparison.OrdinalIgnoreCase) ? "Current" : "Historical");
            item.SubItems.Add(issue.ImportId);
            item.SubItems.Add(issue.Severity);
            item.SubItems.Add(issue.Code);
            item.SubItems.Add(issue.Message);
            item.SubItems.Add(issue.Target);
            _issuesListView.Items.Add(item);
        }
    }
}
