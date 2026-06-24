using LLMGameCreator.WinForms;
using LLMGameCreator.WinForms.Pages;
using LLMGameCreator.WinForms.Pages.UnityArchiveReview;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class UnityArchiveReviewPresenterTests
{
    [Fact]
    public async Task PresenterInitializesWithoutProject()
    {
        var state = await new UnityArchiveReviewPresenter().RefreshAsync(null);

        Assert.Empty(state.ProjectFolder);
        Assert.Empty(state.ArchiveRoot);
        Assert.False(state.CanRefresh);
        Assert.False(state.CanOpenArchiveFolder);
        Assert.Contains("No current project", state.Status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PresenterReportsMissingArchive()
    {
        using var temp = new TempDirectory();

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path);

        Assert.Equal("Missing", state.CurrentReviewReadiness);
        Assert.Equal("Missing", state.ComparisonReadiness);
        Assert.True(state.CanRefresh);
        Assert.False(state.CanOpenArchiveFolder);
        Assert.Contains("not found", state.Status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PresenterReadsExistingReviewHistoryAndComparisonReports()
    {
        using var temp = new TempDirectory();
        CreateReports(temp.Path);

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path);

        Assert.Equal("ReadyWithWarnings", state.CurrentReviewReadiness);
        Assert.Equal("Ready", state.ComparisonReadiness);
        Assert.Equal(2, state.HistorySnapshotCount);
        Assert.Equal("snapshot-b", state.SelectedSnapshotId);
        Assert.Contains("# Current Review", state.CurrentReviewMarkdown);
        Assert.Contains("# Comparison", state.ComparisonMarkdown);
        Assert.Contains("archive-review.json", state.HistoryIndexJson);
        Assert.True(state.CanOpenArchiveFolder);
        Assert.Equal("Archive review, comparison, and history reports loaded.", state.Status);
    }

    [Fact]
    public async Task PresenterHandlesInvalidJsonWithoutThrowing()
    {
        using var temp = new TempDirectory();
        var archiveRoot = CreateArchiveRoot(temp.Path);
        var production = Path.Combine(archiveRoot, "production");
        await File.WriteAllTextAsync(Path.Combine(production, "archive-review.json"), "{ invalid json");
        await File.WriteAllTextAsync(Path.Combine(production, "archive-review.md"), "# Markdown survives");

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path);

        Assert.Equal("Invalid", state.CurrentReviewReadiness);
        Assert.Equal("{ invalid json", state.CurrentReviewJson);
        Assert.Equal("# Markdown survives", state.CurrentReviewMarkdown);
        Assert.Contains("Invalid JSON", state.Status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PresenterLoadsSelectedHistorySnapshotJson()
    {
        using var temp = new TempDirectory();
        CreateReports(temp.Path);

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path, "snapshot-a");

        Assert.Equal("snapshot-a", state.SelectedSnapshotId);
        Assert.Equal(1, state.SelectedSnapshotSequence);
        Assert.Equal("review-history/snapshot-a/archive-review.json", state.SelectedSnapshotRelativePath);
        Assert.Equal("Loaded", state.SelectedSnapshotStatus);
        Assert.Contains("snapshot-a", state.SelectedSnapshotJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PresenterReportsMissingSelectedHistorySnapshotWithoutThrowing()
    {
        using var temp = new TempDirectory();
        CreateReports(temp.Path);
        var archiveRoot = CreateArchiveRoot(temp.Path);
        File.Delete(Path.Combine(archiveRoot, "review-history", "snapshot-a", "archive-review.json"));

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path, "snapshot-a");

        Assert.Equal("snapshot-a", state.SelectedSnapshotId);
        Assert.Equal("Missing", state.SelectedSnapshotStatus);
        Assert.Empty(state.SelectedSnapshotJson);
        Assert.Contains("review-history/snapshot-a/archive-review.json", state.Status, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PresenterDisplaysManualImportReportsWhenPresent()
    {
        using var temp = new TempDirectory();
        CreateReports(temp.Path);
        var archiveRoot = CreateArchiveRoot(temp.Path);
        File.WriteAllText(
            Path.Combine(archiveRoot, "production", "manual-provider-import-report.json"),
            "{\"schemaVersion\":\"1\",\"readiness\":\"Ready\"}");
        File.WriteAllText(
            Path.Combine(archiveRoot, "production", "manual-provider-import-report.md"),
            "# Manual Import\n\nReady.");

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path);

        Assert.Contains("\"readiness\":\"Ready\"", state.ManualImportReportJson, StringComparison.Ordinal);
        Assert.Contains("# Manual Import", state.ManualImportReportMarkdown, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PresenterReportsIndividuallyMissingFiles()
    {
        using var temp = new TempDirectory();
        CreateArchiveRoot(temp.Path);

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path);

        Assert.Contains("production/archive-review.json", state.Status);
        Assert.Contains("production/archive-review.md", state.Status);
        Assert.Contains("production/archive-review-comparison.json", state.Status);
        Assert.Contains("production/archive-review-history-index.json", state.Status);
    }

    [Fact]
    public void UserControlCanBeConstructedWithoutRuntimeServices()
    {
        using var page = new UnityArchiveReviewPageControl();

        Assert.Equal("unity_archive_review", page.Id);
        Assert.Equal("Unity Archive Review", page.Title);
        Assert.Equal(41, page.SortOrder);
    }

    [Fact]
    public void CompositionRootRegistersArchiveReviewPage()
    {
        using var compositionRoot = new CompositionRoot();

        var registry = compositionRoot.ResolveEditorPageRegistry();
        var page = Assert.Single(registry.Pages, candidate => candidate.Id == "unity_archive_review");

        Assert.IsType<UnityArchiveReviewPageControl>(page);
        Assert.Equal("Unity Archive Review", page.Title);
    }

    internal static string CreateArchiveRoot(string projectFolder)
    {
        var archiveRoot = Path.Combine(projectFolder, ".llmgc", "unity-archive");
        Directory.CreateDirectory(Path.Combine(archiveRoot, "production"));
        return archiveRoot;
    }

    internal static void CreateReports(string projectFolder)
    {
        var archiveRoot = CreateArchiveRoot(projectFolder);
        var production = Path.Combine(archiveRoot, "production");
        File.WriteAllText(Path.Combine(production, "archive-review.json"), """
        {
          "schemaVersion": "1",
          "readiness": "ReadyWithWarnings",
          "sourceFileCount": 3,
          "diagnosticCount": 1
        }
        """);
        File.WriteAllText(Path.Combine(production, "archive-review.md"), "# Current Review\n\nReady with warnings.");
        File.WriteAllText(Path.Combine(production, "archive-review-comparison.json"), """
        {
          "schemaVersion": "1",
          "readiness": "Ready",
          "currentSnapshotId": "snapshot-b",
          "previousSnapshotId": "snapshot-a"
        }
        """);
        File.WriteAllText(Path.Combine(production, "archive-review-comparison.md"), "# Comparison\n\nNo blocking changes.");
        File.WriteAllText(Path.Combine(production, "archive-review-history-index.json"), """
        {
          "schemaVersion": "1",
          "entries": [
            {
              "sequence": 1,
              "snapshotId": "snapshot-a",
              "relativePath": "review-history/snapshot-a/archive-review.json"
            },
            {
              "sequence": 2,
              "snapshotId": "snapshot-b",
              "relativePath": "review-history/snapshot-b/archive-review.json"
            }
          ]
        }
        """);

        foreach (var snapshotId in new[] { "snapshot-a", "snapshot-b" })
        {
            var snapshotFolder = Path.Combine(archiveRoot, "review-history", snapshotId);
            Directory.CreateDirectory(snapshotFolder);
            File.WriteAllText(
                Path.Combine(snapshotFolder, "archive-review.json"),
                $"{{\"schemaVersion\":\"1\",\"readiness\":\"Ready\",\"snapshotMarker\":\"{snapshotId}\"}}");
        }
    }

    internal sealed class TempDirectory : IDisposable
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
