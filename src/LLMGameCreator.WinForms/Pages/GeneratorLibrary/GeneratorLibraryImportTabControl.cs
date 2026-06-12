using LLMGameCreator.Application.Design;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratorLibraryImportTabControl : UserControl
{
    public GeneratorLibraryImportTabControl()
    {
        InitializeComponent();
        _importButton.Click += (_, _) => ImportRequested?.Invoke(this, EventArgs.Empty);
        _refreshButton.Click += (_, _) => RefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? ImportRequested;
    public event EventHandler? RefreshRequested;

    public void SetStatus(string databasePath, string status)
    {
        _databasePathLabel.Text = "DB: " + databasePath;
        _statusLabel.Text = status;
    }

    public void SetReport(GeneratorLibraryImportReport? report)
    {
        if (report == null)
        {
            _summaryTextBox.Text = string.Empty;
            return;
        }

        _summaryTextBox.Text =
            $"Import: {report.ImportId}\r\n" +
            $"Manifests: {report.ImportedManifestCount}/{report.ManifestCount}\r\n" +
            $"Modules: {report.ModuleCount}\r\n" +
            $"Capabilities: {report.CapabilityCount}\r\n" +
            $"Files: {report.FileCount}\r\n" +
            $"Issues: {report.Issues.Count}";
    }
}
