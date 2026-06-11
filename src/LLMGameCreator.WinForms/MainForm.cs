using LLMGameCreator.Application.Projects;
using LLMGameCreator.WinForms.Pages;
using Microsoft.Extensions.Logging;

namespace LLMGameCreator.WinForms;

public sealed partial class MainForm : Form
{
    private readonly IEditorPageRegistry? _pageRegistry;
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly ILogger? _logger;

    public MainForm()
    {
        InitializeComponent();
        _statusLabel.Text = "Design-time preview";
    }

    public MainForm(IEditorPageRegistry pageRegistry, ICurrentGamePackageService currentGamePackageService, ILoggerFactory loggerFactory)
    {
        _pageRegistry = pageRegistry;
        _currentGamePackageService = currentGamePackageService;
        _logger = loggerFactory.CreateLogger<MainForm>();

        InitializeComponent();
        BindPages();

        _currentGamePackageService.CurrentChanged += (_, _) => UpdateStatus();
        UpdateStatus();
    }

    private void BindPages()
    {
        if (_pageRegistry == null)
        {
            return;
        }

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

        _logger?.LogInformation("Открыта страница {PageId}", page.Id);
        _workspace.Controls.Clear();
        page.View.Dock = DockStyle.Fill;
        _workspace.Controls.Add(page.View);
        page.OnActivated();
    }

    private void UpdateStatus()
    {
        if (_currentGamePackageService == null)
        {
            _statusLabel.Text = "Design-time preview";
            return;
        }

        _statusLabel.Text = _currentGamePackageService.CurrentPackage == null
            ? "Проект игры не открыт"
            : $"Открыт проект: {_currentGamePackageService.CurrentPackage.Manifest.Title}";
    }
}
