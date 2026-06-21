using System.Text.Json;
using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class CompositionDiagnosticsExportTests
{
    [Fact]
    public async Task ExportCreatesDirectoryMarkdownAndDeterministicIndex()
    {
        using var temp = new TempDirectory();
        var service = CreateExportService();
        await Assert.ThrowsAsync<ArgumentException>(() => service.ExportAsync(new GameCompositionDiagnosticsExportRequest
        {
            ProjectRootPath = " ",
            Report = CreateReport(GameBlueprintPresetProvider.BaselineGeneratedRpgPreview)
        }));
        var request = new GameCompositionDiagnosticsExportRequest
        {
            ProjectRootPath = temp.Path,
            Report = CreateReport(GameBlueprintPresetProvider.BaselineGeneratedRpgPreview)
        };

        var first = await service.ExportAsync(request);
        var firstMarkdown = await File.ReadAllTextAsync(first.MarkdownPath);
        var firstIndex = await File.ReadAllTextAsync(first.IndexPath);
        var second = await service.ExportAsync(request);

        Assert.True(Directory.Exists(first.OutputDirectoryPath));
        Assert.True(File.Exists(first.MarkdownPath));
        Assert.True(File.Exists(first.IndexPath));
        Assert.Contains("## Readiness", firstMarkdown);
        Assert.Contains("## Selected current generators", firstMarkdown);
        Assert.Equal(firstMarkdown, await File.ReadAllTextAsync(second.MarkdownPath));
        Assert.Equal(firstIndex, await File.ReadAllTextAsync(second.IndexPath));
    }

    [Fact]
    public async Task BlueprintIdIsSanitizedAndCannotEscapeOutputDirectory()
    {
        using var temp = new TempDirectory();
        var report = CreateReport(GameBlueprintPresetProvider.BaselineGeneratedRpgPreview) with
        {
            BlueprintId = "../../unsafe:blueprint\\report"
        };

        var result = await CreateExportService().ExportAsync(new GameCompositionDiagnosticsExportRequest
        {
            ProjectRootPath = temp.Path,
            Report = report
        });

        Assert.Equal("unsafe-blueprint-report.composition-report.md", Path.GetFileName(result.MarkdownPath));
        Assert.True(IsUnder(temp.Path, result.MarkdownPath));
        Assert.True(IsUnder(temp.Path, result.IndexPath));
        Assert.All(Directory.EnumerateFiles(temp.Path, "*", SearchOption.AllDirectories), path => Assert.True(IsUnder(temp.Path, path)));
    }

    [Fact]
    public async Task IndexEntriesAreSortedByBlueprintId()
    {
        using var temp = new TempDirectory();
        var service = CreateExportService();
        var baseline = CreateReport(GameBlueprintPresetProvider.BaselineGeneratedRpgPreview);
        var second = baseline with { BlueprintId = "zeta-blueprint", Title = "Zeta" };
        var first = baseline with { BlueprintId = "alpha-blueprint", Title = "Alpha" };

        await service.ExportAsync(new GameCompositionDiagnosticsExportRequest { ProjectRootPath = temp.Path, Report = second });
        var result = await service.ExportAsync(new GameCompositionDiagnosticsExportRequest { ProjectRootPath = temp.Path, Report = first });
        var index = JsonSerializer.Deserialize<GameCompositionDiagnosticsExportIndex>(
            await File.ReadAllTextAsync(result.IndexPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(index);
        Assert.Equal(["alpha-blueprint", "zeta-blueprint"], index.Entries.Select(entry => entry.BlueprintId));
    }

    private static GameCompositionDiagnosticsExportService CreateExportService()
    {
        return new GameCompositionDiagnosticsExportService(new GameCompositionDiagnosticsMarkdownRenderer());
    }

    private static GameCompositionDiagnosticsReport CreateReport(string presetId)
    {
        var capabilities = BuiltInCapabilityRegistry.Create();
        var catalog = BuiltInGeneratorCatalog.Create();
        var service = new GameCompositionDiagnosticsService(
            new GameBlueprintCompositionValidator(capabilities),
            new GeneratorCatalogValidator(capabilities),
            new GeneratorPlanResolver(capabilities, catalog),
            catalog);
        Assert.True(new GameBlueprintPresetProvider().TryGet(presetId, out var blueprint));
        return service.CreateReport(blueprint);
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
