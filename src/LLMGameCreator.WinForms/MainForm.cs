using LLMGameCreator.Application.Projects;
using LLMGameCreator.WinForms.Pages;
using Microsoft.Extensions.Logging;

namespace LLMGameCreator.WinForms;

public sealed partial class MainForm : Form
{
    private readonly IEditorPageRegistry? _pageRegistry;
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly ILogger? _logger;
    private readonly IEditorPageNavigationService? _navigationService;

    public MainForm()
    {
        InitializeComponent();
        _statusLabel.Text = "Design-time preview";
    }

    public MainForm(IEditorPageRegistry pageRegistry, ICurrentGamePackageService currentGamePackageService, ILoggerFactory loggerFactory)
        : this(pageRegistry, currentGamePackageService, loggerFactory, new EditorPageNavigationService())
    {
    }

    public MainForm(IEditorPageRegistry pageRegistry, ICurrentGamePackageService currentGamePackageService, ILoggerFactory loggerFactory, IEditorPageNavigationService navigationService)
    {
        _pageRegistry = pageRegistry;
        _currentGamePackageService = currentGamePackageService;
        _logger = loggerFactory.CreateLogger<MainForm>();
        _navigationService = navigationService;

        InitializeComponent();
        BindPages();

        _currentGamePackageService.CurrentChanged += CurrentGamePackageService_CurrentChanged;
        _navigationService.NavigationRequested += NavigationRequested;
        UpdateStatus();
    }

    private void CurrentGamePackageService_CurrentChanged(object? sender, EventArgs e)
    {
        WinFormsUiThreadDispatcher.Post(this, UpdateStatus);
    }

    private void DisposeRuntime()
    {
        if (_currentGamePackageService != null)
        {
            _currentGamePackageService.CurrentChanged -= CurrentGamePackageService_CurrentChanged;
        }
        if (_navigationService != null) _navigationService.NavigationRequested -= NavigationRequested;
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

    private void NavigationRequested(object? sender, string pageId)
    {
        var index = _pageRegistry?.Pages.ToList().FindIndex(page => page.Id == pageId) ?? -1;
        if (index >= 0) WinFormsUiThreadDispatcher.Post(this, () => _navigation.SelectedIndex = index);
        else _logger?.LogWarning("Неизвестная страница навигации {PageId}", pageId);
    }

    private void UpdateStatus()
    {
        if (_currentGamePackageService == null)
        {
            _statusLabel.Text = "Design-time preview";
            return;
        }

        var package = _currentGamePackageService.CurrentPackage;
        if (package == null)
        {
            _statusLabel.Text = "Проект игры не открыт";
            return;
        }

        _statusLabel.Text = $"Открыт проект: {package.Manifest.Title}";
    }
}
