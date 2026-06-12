using LLMGameCreator.Application.Design;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratorLibraryIntegrityTabControl : UserControl
{
    private IReadOnlyList<GeneratorLibraryIntegrityIssue> _issues = Array.Empty<GeneratorLibraryIntegrityIssue>();

    public GeneratorLibraryIntegrityTabControl()
    {
        InitializeComponent();
        _validateButton.Click += (_, _) => ValidateRequested?.Invoke(this, EventArgs.Empty);
        _issuesListView.SelectedIndexChanged += (_, _) => UpdateSelectedIssueDetails();
    }

    public event EventHandler? ValidateRequested;

    public void SetStatus(string status)
    {
        _statusLabel.Text = status;
    }

    public void SetReport(GeneratorLibraryIntegrityReport? report)
    {
        _issues = report?.Issues ?? Array.Empty<GeneratorLibraryIntegrityIssue>();
        _issuesListView.Items.Clear();
        _detailsTextBox.Text = string.Empty;

        if (report == null)
        {
            _summaryLabel.Text = "No integrity report.";
            return;
        }

        var summary = report.Summary;
        _summaryLabel.Text =
            $"Manifests: {summary.ManifestCount} | Modules: {summary.ModuleCount} | Capabilities: {summary.CapabilityCount} | Files: {summary.FileCount} | Errors: {summary.ErrorCount} | Warnings: {summary.WarningCount} | Info: {summary.InfoCount}";

        foreach (var issue in _issues)
        {
            var item = new ListViewItem(issue.Severity.ToString());
            item.SubItems.Add(issue.Code);
            item.SubItems.Add(issue.Message);
            item.SubItems.Add(issue.Target);
            item.SubItems.Add(issue.SuggestedFix ?? string.Empty);
            _issuesListView.Items.Add(item);
        }

        _statusLabel.Text = report.HasErrors
            ? $"Integrity validation found {summary.ErrorCount} errors."
            : "Integrity validation completed without errors.";
    }

    private void UpdateSelectedIssueDetails()
    {
        if (_issuesListView.SelectedIndices.Count == 0)
        {
            _detailsTextBox.Text = string.Empty;
            return;
        }

        var issue = _issues[_issuesListView.SelectedIndices[0]];
        _detailsTextBox.Text =
            $"Severity: {issue.Severity}\r\n" +
            $"Code: {issue.Code}\r\n" +
            $"Message: {issue.Message}\r\n" +
            $"Target: {issue.Target}\r\n" +
            $"Manifest: {issue.ManifestPath ?? string.Empty}\r\n" +
            $"Suggested fix: {issue.SuggestedFix ?? string.Empty}";
    }
}
