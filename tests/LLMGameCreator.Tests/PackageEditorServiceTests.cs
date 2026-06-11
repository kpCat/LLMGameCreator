using LLMGameCreator.Application.Editing;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class PackageEditorServiceTests
{
    [Fact]
    public void GetSnapshot_WithoutCurrentPackage_ReturnsEmptyState()
    {
        var service = CreateEditorServiceWithoutPackage();

        var snapshot = service.GetSnapshot();

        Assert.False(snapshot.HasCurrentPackage);
        Assert.Empty(snapshot.Maps);
        Assert.Empty(snapshot.TilePrototypes);
    }

    [Fact]
    public void UpdateManifest_ChangesCurrentPackageManifest()
    {
        var current = CreateCurrentService(CreateMinimalPackage());
        var service = CreateEditorService(current);

        service.UpdateManifest(new ManifestEditModel
        {
            PackageId = "game/changed",
            Title = "Changed Title",
            Version = "1.2.3",
            FormatVersion = "0.1",
            StartMapId = "map/start",
            Description = "Changed description"
        });

        Assert.Equal("game/changed", current.CurrentPackage?.Manifest.PackageId);
        Assert.Equal("Changed Title", current.CurrentPackage?.Manifest.Title);
        Assert.Equal("1.2.3", current.CurrentPackage?.Manifest.Version);
        Assert.Equal("Changed description", current.CurrentPackage?.Manifest.Description);
    }

    [Fact]
    public void AddTilePrototype_AddsValidTileAndRejectsDuplicateId()
    {
        var current = CreateCurrentService(CreateMinimalPackage());
        var service = CreateEditorService(current);

        service.AddTilePrototype(new TilePrototypeEditModel
        {
            Id = "tile/water",
            Name = "Water",
            Walkable = false,
            MovementCost = 3.5
        });

        Assert.Contains(current.CurrentPackage!.Game.TilePrototypes, tile => tile.Id == "tile/water");
        Assert.Throws<InvalidOperationException>(() => service.AddTilePrototype(new TilePrototypeEditModel { Id = "tile/water" }));
    }

    [Fact]
    public void AddMap_AddsValidMapAndRejectsDuplicateId()
    {
        var current = CreateCurrentService(CreateMinimalPackage());
        var service = CreateEditorService(current);

        service.AddMap(new MapEditModel
        {
            Id = "map/second",
            Name = "Second",
            Width = 4,
            Height = 4,
            DefaultTileId = "tile/grass",
            StartX = 1,
            StartY = 1
        });

        Assert.Contains(current.CurrentPackage!.Game.Maps, map => map.Id == "map/second");
        Assert.Throws<InvalidOperationException>(() => service.AddMap(new MapEditModel
        {
            Id = "map/second",
            Width = 4,
            Height = 4,
            DefaultTileId = "tile/grass"
        }));
    }

    [Fact]
    public void RemoveMap_RejectsCurrentStartMap()
    {
        var service = CreateEditorService(CreateCurrentService(CreateMinimalPackage()));

        var exception = Assert.Throws<InvalidOperationException>(() => service.RemoveMap("map/start"));

        Assert.Contains("start map", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RemoveTilePrototype_RejectsTileUsedByMapDefaultTile()
    {
        var service = CreateEditorService(CreateCurrentService(CreateMinimalPackage()));

        var exception = Assert.Throws<InvalidOperationException>(() => service.RemoveTilePrototype("tile/grass"));

        Assert.Contains("default tile", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RemoveEntityPrototype_RejectsPrototypeUsedByMapEntity()
    {
        var package = CreateMinimalPackage();
        package.Game.EntityPrototypes.Add(new EntityPrototypeDefinition
        {
            Id = "prototype/npc/guard",
            Name = "Guard"
        });
        package.Game.Maps[0].Entities.Add(new EntityInstanceDefinition
        {
            Id = "entity/start/guard",
            PrototypeId = "prototype/npc/guard",
            Position = new Position2D(1, 1)
        });
        var service = CreateEditorService(CreateCurrentService(package));

        var exception = Assert.Throws<InvalidOperationException>(() => service.RemoveEntityPrototype("prototype/npc/guard"));

        Assert.Contains("used by map entities", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveAsync_PersistsManifestEditsThroughCurrentPackageService()
    {
        using var temp = new TempDirectory();
        var repository = new JsonGamePackageRepository();
        await repository.SaveAsync(temp.Path, CreateMinimalPackage(), CancellationToken.None);

        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(temp.Path, CancellationToken.None);
        var service = CreateEditorService(current);
        service.UpdateManifest(new ManifestEditModel
        {
            PackageId = "game/saved",
            Title = "Saved Title",
            Version = "2.0.0",
            FormatVersion = "0.1",
            StartMapId = "map/start",
            Description = "Saved description"
        });

        await service.SaveAsync(CancellationToken.None);

        var reloaded = await repository.LoadAsync(temp.Path, CancellationToken.None);
        Assert.Equal("Saved Title", reloaded.Manifest.Title);
        Assert.Equal("2.0.0", reloaded.Manifest.Version);
        Assert.Equal("Saved description", reloaded.Manifest.Description);
    }

    [Fact]
    public void Validate_ReturnsExistingReportAndFormatterCanDisplayIt()
    {
        var package = CreateMinimalPackage();
        package.Manifest.StartMapId = "map/missing";
        var service = CreateEditorService(CreateCurrentService(package));

        var report = service.Validate();
        var formatted = new ValidationReportFormatter().Format(report);

        Assert.False(report.IsValid);
        Assert.Contains("manifest.start_map.missing", formatted);
    }

    private static IPackageEditorService CreateEditorServiceWithoutPackage()
    {
        return CreateEditorService(new CurrentGamePackageService(new JsonGamePackageRepository()));
    }

    private static IPackageEditorService CreateEditorService(ICurrentGamePackageService current)
    {
        return new PackageEditorService(current, new GamePackageValidator());
    }

    private static ICurrentGamePackageService CreateCurrentService(GamePackageDefinition package)
    {
        return new InMemoryCurrentGamePackageService(package);
    }

    private static GamePackageDefinition CreateMinimalPackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest
            {
                PackageId = "game/test",
                Title = "Test Game",
                Version = "0.1.0",
                FormatVersion = "0.1",
                StartMapId = "map/start"
            },
            Game = new GameDefinition
            {
                TilePrototypes = new List<TilePrototypeDefinition>
                {
                    new TilePrototypeDefinition
                    {
                        Id = "tile/grass",
                        Name = "Grass",
                        Walkable = true,
                        MovementCost = 1.0
                    }
                },
                Maps = new List<MapDefinition>
                {
                    new MapDefinition
                    {
                        Id = "map/start",
                        Name = "Start",
                        Width = 5,
                        Height = 5,
                        DefaultTileId = "tile/grass",
                        StartPosition = new Position2D(2, 2)
                    }
                }
            }
        };
    }

    private sealed class InMemoryCurrentGamePackageService : ICurrentGamePackageService
    {
        public InMemoryCurrentGamePackageService(GamePackageDefinition package)
        {
            CurrentPackage = package;
        }

        public string? CurrentFolder => null;
        public GamePackageDefinition? CurrentPackage { get; private set; }
        public event EventHandler? CurrentChanged;

        public Task LoadAsync(string projectFolder, CancellationToken cancellationToken)
        {
            CurrentChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public Task SaveAsync(CancellationToken cancellationToken)
        {
            CurrentChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

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
