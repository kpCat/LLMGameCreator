using LLMGameCreator.Application.Projects;
using LLMGameCreator.WinForms.Pages;
using Microsoft.Extensions.Logging;

namespace LLMGameCreator.WinForms;

public sealed class MainForm : Form
{
    private readonly IEditorPageRegistry _pageRegistry;
    private readonly ICurrentGamePackageService _currentGamePackageService;
    private readonly ILogger _logger;
    private readonly ListBox _navigation = new ListBox();
    private readonly Panel _workspace = new Panel();
    private readonly StatusStrip _statusStrip = new StatusStrip();
    private readonly ToolStripStatusLabel _statusLabel = new ToolStripStatusLabel();

    public MainForm(IEditorPageRegistry pageRegistry, ICurrentGamePackageService currentGamePackageService, ILoggerFactory loggerFactory)
    {
        _pageRegistry = pageRegistry;
        _currentGamePackageService = currentGamePackageService;
        _logger = loggerFactory.CreateLogger<MainForm>();

        Text = "LLMGameCreator";
        Width = 1280;
        Height = 800;
        StartPosition = FormStartPosition.CenterScreen;

        BuildLayout();
        BindPages();

        _currentGamePackageService.CurrentChanged += (_, _) => UpdateStatus();
        UpdateStatus();
    }

    private void BuildLayout()
    {
        _navigation.Dock = DockStyle.Left;
        _navigation.Width = 220;
        _navigation.DisplayMember = nameof(IEditorPage.Title);

        _workspace.Dock = DockStyle.Fill;
        _workspace.BackColor = SystemColors.Control;

        _statusStrip.Items.Add(_statusLabel);
        _statusStrip.Dock = DockStyle.Bottom;

        Controls.Add(_workspace);
        Controls.Add(_navigation);
        Controls.Add(_statusStrip);
    }

    private void BindPages()
    {
        foreach (var page in _pageRegistry.Pages)
        {
            _navigation.Items.Add(page);
        }

        _navigation.SelectedIndexChanged += (_, _) => ShowSelectedPage();
        if (_navigation.Items.Count > 0)
        {
            _navigation.SelectedIndex = 0;
        }
    }

    private void ShowSelectedPage()
    {
        if (_navigation.SelectedItem is not IEditorPage page)
        {
            return;
        }

        _logger.LogInformation("Открыта страница {PageId}", page.Id);
        _workspace.Controls.Clear();
        page.View.Dock = DockStyle.Fill;
        _workspace.Controls.Add(page.View);
        page.OnActivated();
    }

    private void UpdateStatus()
    {
        _statusLabel.Text = _currentGamePackageService.CurrentPackage == null
            ? "Проект игры не открыт"
            : $"Открыт проект: {_currentGamePackageService.CurrentPackage.Manifest.Title}";
    }
}
