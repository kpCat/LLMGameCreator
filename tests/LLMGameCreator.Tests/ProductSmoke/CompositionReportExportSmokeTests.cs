using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class CompositionReportExportSmokeTests
{
    [Fact]
    public async Task CompositionReportExportProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var capabilities = BuiltInCapabilityRegistry.Create();
        var catalog = BuiltInGeneratorCatalog.Create();
        var diagnosticsService = new GameCompositionDiagnosticsService(
            new GameBlueprintCompositionValidator(capabilities),
            new GeneratorCatalogValidator(capabilities),
            new GeneratorPlanResolver(capabilities, catalog),
            catalog);
        Assert.True(new GameBlueprintPresetProvider().TryGet(
            GameBlueprintPresetProvider.BaselineGeneratedRpgPreview,
            out var blueprint));
        var report = diagnosticsService.CreateReport(blueprint);
        var exportService = new GameCompositionDiagnosticsExportService(new GameCompositionDiagnosticsMarkdownRenderer());
        var request = new GameCompositionDiagnosticsExportRequest { ProjectRootPath = projectRoot, Report = report };

        var first = await exportService.ExportAsync(request);
        var firstMarkdown = await File.ReadAllTextAsync(first.MarkdownPath);
        var firstIndex = await File.ReadAllTextAsync(first.IndexPath);
        var second = await exportService.ExportAsync(request);

        Assert.True(File.Exists(first.MarkdownPath));
        Assert.True(File.Exists(first.IndexPath));
        Assert.Contains("## Readiness", firstMarkdown);
        Assert.Contains("## Selected current generators", firstMarkdown);
        Assert.Equal(firstMarkdown, await File.ReadAllTextAsync(second.MarkdownPath));
        Assert.Equal(firstIndex, await File.ReadAllTextAsync(second.IndexPath));
        Assert.True(IsUnder(projectRoot, first.MarkdownPath));
        Assert.True(IsUnder(projectRoot, first.IndexPath));

        var traversal = await exportService.ExportAsync(request with
        {
            Report = report with { BlueprintId = "../../outside-report" }
        });
        Assert.True(IsUnder(projectRoot, traversal.MarkdownPath));
        Assert.All(Directory.EnumerateFiles(projectRoot, "*", SearchOption.AllDirectories), path => Assert.True(IsUnder(projectRoot, path)));
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

    private static bool IsUnder(string root, string path)
    {
        var prefix = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
        return Path.GetFullPath(path).StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
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
