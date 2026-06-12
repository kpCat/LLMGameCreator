using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratorLibraryPageControl : UserControl, IEditorPage
{
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly IDesignDatabaseInitializer? _databaseInitializer;
    private readonly IGeneratorLibraryImporter? _importer;
    private readonly IGeneratorLibraryRegistry? _registry;
    private readonly IGeneratorLibraryIntegrityValidator? _integrityValidator;
    private readonly IGeneratorPlanDraftService? _planDraftService;
    private readonly IGeneratorPlanRepository? _planRepository;
    private readonly IGeneratorPlanReviewService? _planReviewService;
    private readonly IGeneratorPlanPreviewService? _planPreviewService;
    private readonly IGeneratedArtifactRepository? _artifactRepository;
    private readonly IGamePackagePatchService? _patchService;

    public GeneratorLibraryPageControl()
    {
        InitializeComponent();
    }

    public GeneratorLibraryPageControl(
        ICurrentGamePackageService currentGamePackageService,
        IDesignDatabaseInitializer databaseInitializer,
        IGeneratorLibraryImporter importer,
        IGeneratorLibraryRegistry registry,
        IGeneratorLibraryIntegrityValidator integrityValidator,
        IGeneratorPlanDraftService planDraftService,
        IGeneratorPlanRepository planRepository,
        IGeneratorPlanReviewService planReviewService,
        IGeneratorPlanPreviewService planPreviewService,
        IGeneratedArtifactRepository artifactRepository,
        IGamePackagePatchService patchService)
    {
        _currentGamePackageService = currentGamePackageService;
        _databaseInitializer = databaseInitializer;
        _importer = importer;
        _registry = registry;
        _integrityValidator = integrityValidator;
        _planDraftService = planDraftService;
        _planRepository = planRepository;
        _planReviewService = planReviewService;
        _planPreviewService = planPreviewService;
        _artifactRepository = artifactRepository;
        _patchService = patchService;
        InitializeComponent();
        _plansTab.Configure(_planDraftService, _planRepository, _planReviewService, _planPreviewService);
        _artifactsTab.Configure(_artifactRepository, _patchService);
        WireEvents();
    }

    public string Id => "generator-library";
    public string Title => "Generator Library";
    public int SortOrder => 35;
    Control IEditorPage.View => this;

    public async void OnActivated()
    {
        await InitializeAndRefreshAsync();
    }

    private void WireEvents()
    {
        _importTab.ImportRequested += async (_, _) => await ImportAsync();
        _importTab.RefreshRequested += async (_, _) => await InitializeAndRefreshAsync();
        _integrityTab.ValidateRequested += async (_, _) => await ValidateIntegrityAsync();
    }

    private async Task InitializeAndRefreshAsync()
    {
        if (_databaseInitializer == null || _registry == null)
        {
            _importTab.SetStatus("unavailable", "Runtime services are not available.");
            return;
        }

        try
        {
            var databasePath = ResolveDatabasePath();
            await _databaseInitializer.InitializeAsync(databasePath, CancellationToken.None).ConfigureAwait(true);
            await RefreshListsAsync().ConfigureAwait(true);
            _importTab.SetStatus(databasePath, "Ready.");
        }
        catch (Exception ex)
        {
            _importTab.SetStatus("error", ex.Message);
        }
    }

    private async Task ImportAsync()
    {
        if (_databaseInitializer == null || _importer == null)
        {
            return;
        }

        try
        {
            var databasePath = ResolveDatabasePath();
            await _databaseInitializer.InitializeAsync(databasePath, CancellationToken.None).ConfigureAwait(true);
            var root = ResolveImportRoot();
            if (root == null)
            {
                _importTab.SetStatus(databasePath, "generator-library folder was not found.");
                return;
            }

            var integrityErrors = 0;
            if (_integrityValidator != null)
            {
                var integrityReport = await _integrityValidator.ValidateAsync(root, CancellationToken.None).ConfigureAwait(true);
                _integrityTab.SetReport(integrityReport);
                integrityErrors = integrityReport.Summary.ErrorCount;
            }

            var report = await _importer.ImportGeneratorLibraryAsync(root, CancellationToken.None).ConfigureAwait(true);
            _importTab.SetReport(report, integrityErrors);
            await RefreshListsAsync().ConfigureAwait(true);
            var status = $"Imported {report.ModuleCount} modules and {report.CapabilityCount} capabilities.";
            if (integrityErrors > 0)
            {
                status = $"Integrity validation has {integrityErrors} errors. Import may be incomplete. " + status;
            }

            _importTab.SetStatus(databasePath, status);
        }
        catch (Exception ex)
        {
            _importTab.SetStatus(ResolveDatabasePath(), ex.Message);
        }
    }

    private async Task ValidateIntegrityAsync()
    {
        if (_integrityValidator == null)
        {
            _integrityTab.SetStatus("Runtime services are not available.");
            return;
        }

        try
        {
            var root = ResolveImportRoot();
            if (root == null)
            {
                _integrityTab.SetStatus("generator-library folder was not found.");
                return;
            }

            var report = await _integrityValidator.ValidateAsync(root, CancellationToken.None).ConfigureAwait(true);
            _integrityTab.SetReport(report);
        }
        catch (Exception ex)
        {
            _integrityTab.SetStatus(ex.Message);
        }
    }

    private async Task RefreshListsAsync()
    {
        if (_registry == null)
        {
            return;
        }

        var modules = await _registry.ListModulesAsync(CancellationToken.None).ConfigureAwait(true);
        var capabilities = await _registry.ListCapabilitiesAsync(CancellationToken.None).ConfigureAwait(true);
        var issues = await _registry.ListImportIssuesAsync(CancellationToken.None).ConfigureAwait(true);
        _modulesTab.SetModules(modules);
        _capabilitiesTab.SetCapabilities(capabilities);
        _issuesTab.SetIssues(issues);
        await _plansTab.RefreshPlansAsync().ConfigureAwait(true);
        await _artifactsTab.RefreshArtifactsAsync().ConfigureAwait(true);
    }

    private string ResolveDatabasePath()
    {
        if (!string.IsNullOrWhiteSpace(_currentGamePackageService?.CurrentFolder))
        {
            return Path.Combine(_currentGamePackageService.CurrentFolder, ".llmgc", "design.db");
        }

        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LLMGameCreator");
        return Path.Combine(appData, "design.db");
    }

    private string? ResolveImportRoot()
    {
        if (!string.IsNullOrWhiteSpace(_currentGamePackageService?.CurrentFolder))
        {
            var fromProject = FindUpward(_currentGamePackageService.CurrentFolder, "generator-library");
            if (fromProject != null)
            {
                return fromProject;
            }
        }

        return FindUpward(AppContext.BaseDirectory, "generator-library");
    }

    private static string? FindUpward(string startPath, string childFolderName)
    {
        var directory = new DirectoryInfo(Path.GetFullPath(startPath));
        if (File.Exists(directory.FullName))
        {
            directory = directory.Parent!;
        }

        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, childFolderName);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
