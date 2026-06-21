using LLMGameCreator.Application.Composition;
using LLMGameCreator.WinForms.Pages;
using LLMGameCreator.WinForms.Pages.CompositionWorkbench;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class CompositionWorkbenchPresenterTests
{
    [Fact]
    public void PresenterListsPresetsAndBuildsBaselineMarkdown()
    {
        var presenter = CreatePresenter();
        var initial = presenter.Initialize(null);

        var preview = presenter.BuildPreview(initial, GameBlueprintPresetProvider.BaselineGeneratedRpgPreview);

        Assert.Contains(initial.Presets, preset => preset.Id == GameBlueprintPresetProvider.BaselineGeneratedRpgPreview);
        Assert.Equal(GameCompositionReadiness.BuildableNow.ToString(), preview.Readiness);
        Assert.Contains("## Readiness", preview.Markdown);
        Assert.Contains("Recommended actions", preview.Summary);
        Assert.Contains("built in memory", preview.Status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PresenterExportsAndRefreshesSavedReports()
    {
        using var temp = new TempDirectory();
        var presenter = CreatePresenter();
        var state = presenter.BuildPreview(
            presenter.Initialize(temp.Path),
            GameBlueprintPresetProvider.BaselineGeneratedRpgPreview);

        var exported = await presenter.ExportAsync(state);
        var refreshed = await presenter.RefreshSavedReportsAsync(exported);

        var saved = Assert.Single(refreshed.SavedReports);
        Assert.Equal(GameBlueprintPresetProvider.BaselineGeneratedRpgPreview, saved.BlueprintId);
        Assert.Equal(saved.ReportFileName, refreshed.SelectedReportFileName);
        Assert.Contains("# Game Composition Diagnostics", refreshed.Markdown);
        Assert.True(File.Exists(Path.Combine(
            temp.Path,
            ".llmgc",
            "composition-diagnostics",
            GameCompositionDiagnosticsExportService.IndexFileName)));
    }

    [Fact]
    public void UserControlCanBeConstructedWithoutRuntimeServices()
    {
        using var page = new CompositionWorkbenchPageControl();

        Assert.Equal("Composition Workbench", page.Title);
        Assert.Equal("composition_workbench", page.Id);
    }

    internal static CompositionWorkbenchPresenter CreatePresenter()
    {
        var capabilities = BuiltInCapabilityRegistry.Create();
        var catalog = BuiltInGeneratorCatalog.Create();
        var renderer = new GameCompositionDiagnosticsMarkdownRenderer();
        return new CompositionWorkbenchPresenter(
            new GameBlueprintPresetProvider(),
            new GameCompositionDiagnosticsService(
                new GameBlueprintCompositionValidator(capabilities),
                new GeneratorCatalogValidator(capabilities),
                new GeneratorPlanResolver(capabilities, catalog),
                catalog),
            renderer,
            new GameCompositionDiagnosticsExportService(renderer));
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
