using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class ProjectLifecycleTests
{
    [Fact]
    public async Task ListAsync_ReturnsCreatedValidGame()
    {
        using var temp = new TempDirectory();
        var service = CreateService();
        await service.CreateAsync(CreateRequest(temp.Path, "alpha-game"), CancellationToken.None);

        var summaries = await service.ListAsync(temp.Path, CancellationToken.None);

        var summary = Assert.Single(summaries);
        Assert.Equal("game/alpha-game", summary.PackageId);
        Assert.True(summary.IsValidPackage);
        Assert.False(summary.HasValidationErrors);
    }

    [Fact]
    public async Task ListAsync_ReturnsInvalidSummaryForCorruptedPackage()
    {
        using var temp = new TempDirectory();
        var folder = Path.Combine(temp.Path, "broken-game");
        Directory.CreateDirectory(folder);
        await File.WriteAllTextAsync(Path.Combine(folder, "package.json"), "{ invalid json", CancellationToken.None);

        var summaries = await CreateService().ListAsync(temp.Path, CancellationToken.None);

        var summary = Assert.Single(summaries);
        Assert.Equal("broken-game", summary.FolderName);
        Assert.True(summary.HasPackageFile);
        Assert.False(summary.IsValidPackage);
        Assert.True(summary.HasValidationErrors);
        Assert.False(string.IsNullOrWhiteSpace(summary.ErrorMessage));
    }

    [Fact]
    public async Task CreateAsync_WritesLoadableAndValidPackageWithRequiredFolders()
    {
        using var temp = new TempDirectory();
        var service = CreateService();

        var summary = await service.CreateAsync(CreateRequest(temp.Path, "new-game"), CancellationToken.None);

        Assert.True(File.Exists(Path.Combine(summary.FolderPath, "package.json")));
        Assert.True(Directory.Exists(Path.Combine(summary.FolderPath, "assets")));
        Assert.True(Directory.Exists(Path.Combine(summary.FolderPath, "scripts")));
        Assert.True(Directory.Exists(Path.Combine(summary.FolderPath, "saves")));

        var repository = new JsonGamePackageRepository();
        var package = await repository.LoadAsync(summary.FolderPath, CancellationToken.None);
        var report = new GamePackageValidator().Validate(package, summary.FolderPath);
        Assert.True(report.IsValid, string.Join(Environment.NewLine, report.Issues.Select(i => i.ToString())));
    }

    [Fact]
    public async Task CreateAsync_RejectsPathTraversalAndExistingFolder()
    {
        using var temp = new TempDirectory();
        var service = CreateService();
        Directory.CreateDirectory(Path.Combine(temp.Path, "existing"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(CreateRequest(temp.Path, ".."), CancellationToken.None));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(CreateRequest(temp.Path, "existing"), CancellationToken.None));
    }

    [Fact]
    public async Task CurrentGamePackageService_SaveAsync_PersistsCurrentPackage()
    {
        using var temp = new TempDirectory();
        var projectService = CreateService();
        var summary = await projectService.CreateAsync(CreateRequest(temp.Path, "save-game"), CancellationToken.None);

        var repository = new JsonGamePackageRepository();
        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(summary.FolderPath, CancellationToken.None);

        Assert.NotNull(current.CurrentPackage);
        current.CurrentPackage.Manifest.Title = "Saved Title";
        await current.SaveAsync(CancellationToken.None);

        var reloaded = await repository.LoadAsync(summary.FolderPath, CancellationToken.None);
        Assert.Equal("Saved Title", reloaded.Manifest.Title);
    }

    private static GameProjectService CreateService()
    {
        return new GameProjectService(
            new JsonGamePackageRepository(),
            new GamePackageValidator(),
            new NewGamePackageFactory());
    }

    private static CreateGameProjectRequest CreateRequest(string root, string folderName)
    {
        return new CreateGameProjectRequest
        {
            GamesRootPath = root,
            FolderName = folderName,
            Title = "Test Game",
            PackageId = "game/" + folderName,
            Version = "0.1.0"
        };
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
