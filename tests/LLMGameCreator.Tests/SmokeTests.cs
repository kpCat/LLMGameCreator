using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class SmokeTests
{
    [Fact]
    public async Task MinimalGame_Loads_Validates_And_Starts_Runtime()
    {
        var root = FindRepositoryRoot();
        var projectFolder = Path.Combine(root, "samples", "minimal-map-game");
        var repository = new JsonGamePackageRepository();
        var package = await repository.LoadAsync(projectFolder, CancellationToken.None);

        var validator = new GamePackageValidator();
        var report = validator.Validate(package);
        Assert.True(report.IsValid, string.Join(Environment.NewLine, report.Issues.Select(i => i.ToString())));

        var runtime = new DefaultGameRuntime();
        var start = runtime.Start(package);
        Assert.True(start.Success);
        Assert.Equal("map/village", start.State.CurrentMapId);
    }

    [Fact]
    public async Task MinimalGame_FirstMove_Changes_Position()
    {
        var root = FindRepositoryRoot();
        var projectFolder = Path.Combine(root, "samples", "minimal-map-game");
        var repository = new JsonGamePackageRepository();
        var package = await repository.LoadAsync(projectFolder, CancellationToken.None);
        var runtime = new DefaultGameRuntime();
        var start = runtime.Start(package);

        var result = runtime.Execute(package, start.State, PlayerCommand.Move(Direction2D.Right));

        Assert.True(result.Success);
        Assert.Equal(2, result.State.PlayerPosition.X);
        Assert.Equal(1, result.State.PlayerPosition.Y);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln")))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new InvalidOperationException("Не найден корень репозитория.");
        }

        return directory.FullName;
    }
}
