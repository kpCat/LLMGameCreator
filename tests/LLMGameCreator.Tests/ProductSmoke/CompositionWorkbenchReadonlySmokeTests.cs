using LLMGameCreator.Application.Composition;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.GamePackage;
using LLMGameCreator.WinForms.Pages;
using LLMGameCreator.WinForms.Pages.CompositionWorkbench;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class CompositionWorkbenchReadonlySmokeTests
{
    [Fact]
    public async Task CompositionWorkbenchReadonlyProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var capabilities = BuiltInCapabilityRegistry.Create();
        var catalog = BuiltInGeneratorCatalog.Create();
        var renderer = new GameCompositionDiagnosticsMarkdownRenderer();
        var presenter = new CompositionWorkbenchPresenter(
            new GameBlueprintPresetProvider(),
            new GameCompositionDiagnosticsService(
                new GameBlueprintCompositionValidator(capabilities),
                new GeneratorCatalogValidator(capabilities),
                new GeneratorPlanResolver(capabilities, catalog),
                catalog),
            renderer,
            new GameCompositionDiagnosticsExportService(renderer));
        using var page = new CompositionWorkbenchPageControl(presenter, new FakeCurrentGamePackageService(projectRoot));

        var initial = presenter.Initialize(projectRoot);
        var preview = presenter.BuildPreview(initial, GameBlueprintPresetProvider.BaselineGeneratedRpgPreview);
        var exported = await presenter.ExportAsync(preview);
        var refreshed = await presenter.RefreshSavedReportsAsync(exported);

        Assert.Equal("Composition Workbench", page.Title);
        Assert.Contains(initial.Presets, preset => preset.Id == GameBlueprintPresetProvider.BaselineGeneratedRpgPreview);
        Assert.Equal(GameCompositionReadiness.BuildableNow.ToString(), preview.Readiness);
        Assert.Contains("# Game Composition Diagnostics", preview.Markdown);
        Assert.Contains("## Recommended actions", refreshed.Markdown);
        Assert.Single(refreshed.SavedReports);
        Assert.True(File.Exists(Path.Combine(projectRoot, ".llmgc", "composition-diagnostics", "index.json")));
        Assert.DoesNotContain(catalog.Manifests, manifest => manifest.CanRunAtRuntime);
        Assert.DoesNotContain(catalog.Manifests, manifest =>
            manifest.GeneratorId.Contains("provider", StringComparison.OrdinalIgnoreCase));
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
    }

    private sealed class FakeCurrentGamePackageService : ICurrentGamePackageService
    {
        public FakeCurrentGamePackageService(string currentFolder)
        {
            CurrentFolder = currentFolder;
        }

        public string? CurrentFolder { get; }
        public GamePackageDefinition? CurrentPackage { get; private set; }
        public event EventHandler? CurrentChanged;
        public Task LoadAsync(string projectFolder, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public void ReplaceCurrent(GamePackageDefinition package)
        {
            CurrentPackage = package;
            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
