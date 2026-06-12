using LLMGameCreator.Application.Settings;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class JsonStorageEncodingTests
{
    [Fact]
    public async Task JsonAppSettingsRepository_SavesReadableCyrillic()
    {
        using var temp = new TempDirectory();
        var path = Path.Combine(temp.Path, "appsettings.json");
        var repository = new JsonAppSettingsRepository(path);

        await repository.SaveAsync(new AppSettings
        {
            GamesRootPath = temp.Path,
            LogsPath = Path.Combine(temp.Path, "Logs"),
            DefaultLlmProfileId = "local-main",
            LlmProfiles = new List<LlmEndpointSettings>
            {
                new LlmEndpointSettings
                {
                    Id = "local-main",
                    Title = "Основной локальный ПК",
                    Endpoint = "http://127.0.0.1:1234/v1",
                    Model = "local-model"
                }
            }
        }, CancellationToken.None);

        var text = await File.ReadAllTextAsync(path, CancellationToken.None);
        Assert.Contains("Основной локальный ПК", text);
        Assert.DoesNotContain("\\u041E", text);
    }

    [Fact]
    public async Task JsonGamePackageRepository_SavesReadableCyrillic()
    {
        using var temp = new TempDirectory();
        var repository = new JsonGamePackageRepository();

        await repository.SaveAsync(temp.Path, new GamePackageDefinition
        {
            Manifest = new GameManifest
            {
                PackageId = "game/test",
                Title = "Основной локальный ПК",
                Description = "Кириллическое описание",
                StartMapId = "map/start"
            },
            Game = new GameDefinition
            {
                TilePrototypes = new List<TilePrototypeDefinition>
                {
                    new TilePrototypeDefinition { Id = "tile/grass", Name = "Трава" }
                },
                Maps = new List<MapDefinition>
                {
                    new MapDefinition
                    {
                        Id = "map/start",
                        Name = "Старт",
                        Width = 1,
                        Height = 1,
                        DefaultTileId = "tile/grass"
                    }
                }
            }
        }, CancellationToken.None);

        var text = await File.ReadAllTextAsync(Path.Combine(temp.Path, "package.json"), CancellationToken.None);
        Assert.Contains("Основной локальный ПК", text);
        Assert.Contains("Кириллическое описание", text);
        Assert.DoesNotContain("\\u041E", text);
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
