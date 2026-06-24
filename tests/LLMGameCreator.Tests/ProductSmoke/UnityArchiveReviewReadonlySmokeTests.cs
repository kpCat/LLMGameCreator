using LLMGameCreator.Application.Projects;
using LLMGameCreator.GamePackage;
using LLMGameCreator.WinForms.Pages;
using LLMGameCreator.WinForms.Pages.UnityArchiveReview;
using LLMGameCreator.Tests.WinForms;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnityArchiveReviewReadonlySmokeTests
{
    [Fact]
    public async Task UnityArchiveReviewReadonlyProductSmoke()
    {
        using var temp = new UnityArchiveReviewPresenterTests.TempDirectory();
        var projectFolder = ResolveProjectFolder(temp.Path);
        UnityArchiveReviewPresenterTests.CreateReports(projectFolder);
        var archiveRoot = Path.Combine(projectFolder, ".llmgc", "unity-archive");
        File.WriteAllText(
            Path.Combine(archiveRoot, "production", "manual-provider-import-report.json"),
            "{\"schemaVersion\":\"1\",\"readiness\":\"Ready\"}");
        File.WriteAllText(
            Path.Combine(archiveRoot, "production", "manual-provider-import-report.md"),
            "# Manual Provider Import\n");
        var before = CaptureFiles(projectFolder);
        var presenter = new UnityArchiveReviewPresenter();
        var state = await presenter.RefreshAsync(projectFolder);
        var after = CaptureFiles(projectFolder);
        using var page = new UnityArchiveReviewPageControl(
            presenter,
            new FakeCurrentGamePackageService(projectFolder));

        Assert.Equal("unity_archive_review", page.Id);
        Assert.Equal("Unity Archive Review", page.Title);
        Assert.Equal("ReadyWithWarnings", state.CurrentReviewReadiness);
        Assert.Equal("Ready", state.ComparisonReadiness);
        Assert.Equal(2, state.HistorySnapshotCount);
        Assert.Contains("# Current Review", state.CurrentReviewMarkdown);
        Assert.Contains("# Comparison", state.ComparisonMarkdown);
        Assert.Equal("Loaded", state.SelectedSnapshotStatus);
        Assert.Contains("snapshot-b", state.SelectedSnapshotJson, StringComparison.Ordinal);
        Assert.Contains("# Manual Provider Import", state.ManualImportReportMarkdown, StringComparison.Ordinal);
        Assert.Contains("\"readiness\":\"Ready\"", state.ManualImportReportJson, StringComparison.Ordinal);
        Assert.Equal(before.Keys.Order(), after.Keys.Order());
        foreach (var entry in before)
        {
            Assert.True(after.TryGetValue(entry.Key, out var content));
            Assert.Equal(entry.Value, content);
        }
    }

    private static Dictionary<string, byte[]> CaptureFiles(string projectFolder)
    {
        return Directory.EnumerateFiles(projectFolder, "*", SearchOption.AllDirectories)
            .ToDictionary(
                path => Path.GetRelativePath(projectFolder, path),
                File.ReadAllBytes,
                StringComparer.OrdinalIgnoreCase);
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
}
