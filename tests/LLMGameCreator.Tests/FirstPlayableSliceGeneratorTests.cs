using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Generation;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Settings;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class FirstPlayableSliceGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_ParsesValidStrictJson()
    {
        var service = CreateService(CreateMinimalPackage(), CreateValidJson());

        var result = await service.GenerateAsync(new GenerationInterviewModel(), CancellationToken.None);

        Assert.True(result.Success, FormatIssues(result));
        Assert.NotNull(result.Draft);
        Assert.Equal("Generated Slice", result.Draft.Title);
    }

    [Fact]
    public async Task GenerateAsync_RejectsMarkdownWrappedJson()
    {
        var service = CreateService(CreateMinimalPackage(), "```json\n" + CreateValidJson() + "\n```");

        var result = await service.GenerateAsync(new GenerationInterviewModel(), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains(result.DraftValidationReport.Issues, issue => issue.Code == "draft.json.strict");
    }

    [Fact]
    public void ApplyDraft_RejectsUnknownTileReference()
    {
        var package = CreateMinimalPackage();
        var service = CreateService(package, CreateValidJson());
        var draft = CreateValidDraft();
        draft.Maps[0].Tiles[0].TileId = "tile/missing";

        var result = service.ApplyDraft(draft);

        Assert.False(result.Success);
        Assert.Contains(result.ValidationReport.Issues, issue => issue.Code == "draft.map.tile_ref");
        Assert.Equal("map/start", package.Manifest.StartMapId);
    }

    [Fact]
    public void ApplyDraft_RejectsUnknownEntityPrototypeReference()
    {
        var service = CreateService(CreateMinimalPackage(), CreateValidJson());
        var draft = CreateValidDraft();
        draft.Maps[0].Entities[0].PrototypeId = "prototype/npc/missing";

        var result = service.ApplyDraft(draft);

        Assert.False(result.Success);
        Assert.Contains(result.ValidationReport.Issues, issue => issue.Code == "draft.entity.prototype");
    }

    [Fact]
    public void ApplyDraft_RejectsStartPositionOutsideMapBounds()
    {
        var service = CreateService(CreateMinimalPackage(), CreateValidJson());
        var draft = CreateValidDraft();
        draft.Maps[0].StartPosition = new Position2D(100, 100);

        var result = service.ApplyDraft(draft);

        Assert.False(result.Success);
        Assert.Contains(result.ValidationReport.Issues, issue => issue.Code == "draft.map.start_bounds");
    }

    [Fact]
    public void ApplyDraft_AppliesValidDraftAndPreservesPackageId()
    {
        var current = new InMemoryCurrentGamePackageService(CreateMinimalPackage());
        var service = CreateService(current, CreateValidJson());

        var result = service.ApplyDraft(CreateValidDraft());

        Assert.True(result.Success, FormatIssues(result));
        Assert.Equal("game/current", current.CurrentPackage?.Manifest.PackageId);
        Assert.Equal("Generated Slice", current.CurrentPackage?.Manifest.Title);
        Assert.Equal("map/generated", current.CurrentPackage?.Manifest.StartMapId);
        Assert.Contains(current.CurrentPackage!.Game.Maps[0].Tiles, tile => tile.TileId == "tile/wall");
    }

    private static FirstPlayableSliceGenerator CreateService(GamePackageDefinition package, string response)
    {
        return CreateService(new InMemoryCurrentGamePackageService(package), response);
    }

    private static FirstPlayableSliceGenerator CreateService(InMemoryCurrentGamePackageService current, string response)
    {
        return new FirstPlayableSliceGenerator(
            new InMemorySettingsRepository(),
            new FakeLlmChatClient(response),
            current,
            new GamePackageValidator());
    }

    private static FirstPlayableSliceDraft CreateValidDraft()
    {
        return new FirstPlayableSliceDraft
        {
            Title = "Generated Slice",
            PackageId = "game/generated",
            Description = "Generated description.",
            StartMapId = "map/generated",
            TilePrototypes = new List<TilePrototypeDefinition>
            {
                new TilePrototypeDefinition { Id = "tile/grass", Name = "Grass", Walkable = true, MovementCost = 1 },
                new TilePrototypeDefinition { Id = "tile/wall", Name = "Wall", Walkable = false, MovementCost = 999 },
                new TilePrototypeDefinition { Id = "tile/road", Name = "Road", Walkable = true, MovementCost = 0.8 }
            },
            EntityPrototypes = new List<EntityPrototypeDefinition>
            {
                new EntityPrototypeDefinition { Id = "prototype/npc/elder", Name = "Elder" }
            },
            Maps = new List<MapDefinition>
            {
                new MapDefinition
                {
                    Id = "map/generated",
                    Name = "Generated Map",
                    Width = 12,
                    Height = 8,
                    DefaultTileId = "tile/grass",
                    StartPosition = new Position2D(2, 2),
                    Tiles = new List<TileOverrideDefinition>
                    {
                        new TileOverrideDefinition { X = 0, Y = 0, TileId = "tile/wall" },
                        new TileOverrideDefinition { X = 1, Y = 0, TileId = "tile/wall" },
                        new TileOverrideDefinition { X = 2, Y = 1, TileId = "tile/road" }
                    },
                    Entities = new List<EntityInstanceDefinition>
                    {
                        new EntityInstanceDefinition
                        {
                            Id = "entity/elder",
                            PrototypeId = "prototype/npc/elder",
                            Position = new Position2D(5, 5)
                        }
                    }
                }
            },
            Dialogues = new List<DialogueDefinition>(),
            Quests = new List<QuestDefinition>()
        };
    }

    private static string CreateValidJson()
    {
        return """
{
  "title": "Generated Slice",
  "packageId": "game/generated",
  "description": "Generated description.",
  "startMapId": "map/generated",
  "tilePrototypes": [
    { "id": "tile/grass", "name": "Grass", "walkable": true, "movementCost": 1.0, "assetId": null },
    { "id": "tile/wall", "name": "Wall", "walkable": false, "movementCost": 999.0, "assetId": null },
    { "id": "tile/road", "name": "Road", "walkable": true, "movementCost": 0.8, "assetId": null }
  ],
  "entityPrototypes": [
    { "id": "prototype/npc/elder", "name": "Elder", "assetId": null, "components": [] }
  ],
  "maps": [
    {
      "id": "map/generated",
      "name": "Generated Map",
      "width": 12,
      "height": 8,
      "defaultTileId": "tile/grass",
      "startPosition": { "x": 2, "y": 2 },
      "tiles": [
        { "x": 0, "y": 0, "tileId": "tile/wall" },
        { "x": 1, "y": 0, "tileId": "tile/wall" },
        { "x": 2, "y": 1, "tileId": "tile/road" }
      ],
      "entities": [
        { "id": "entity/elder", "prototypeId": "prototype/npc/elder", "position": { "x": 5, "y": 5 }, "components": [] }
      ]
    }
  ],
  "dialogues": [],
  "quests": []
}
""";
    }

    private static GamePackageDefinition CreateMinimalPackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest
            {
                PackageId = "game/current",
                Title = "Current",
                Version = "0.1.0",
                FormatVersion = "0.1",
                StartMapId = "map/start"
            },
            Game = new GameDefinition
            {
                TilePrototypes = new List<TilePrototypeDefinition>
                {
                    new TilePrototypeDefinition { Id = "tile/grass", Name = "Grass", Walkable = true }
                },
                Maps = new List<MapDefinition>
                {
                    new MapDefinition
                    {
                        Id = "map/start",
                        Name = "Start",
                        Width = 12,
                        Height = 8,
                        DefaultTileId = "tile/grass",
                        StartPosition = new Position2D(1, 1)
                    }
                }
            }
        };
    }

    private static string FormatIssues(GenerationResult result)
    {
        return string.Join(Environment.NewLine, result.DraftValidationReport.Issues.Select(issue => issue.ToString()));
    }

    private static string FormatIssues(FirstPlayableSliceApplyResult result)
    {
        return string.Join(Environment.NewLine, result.ValidationReport.Issues.Select(issue => issue.ToString()));
    }

    private sealed class FakeLlmChatClient : ILlmChatClient
    {
        private readonly string _response;

        public FakeLlmChatClient(string response)
        {
            _response = response;
        }

        public Task<LlmChatResponse> CompleteAsync(LlmEndpointSettings profile, LlmChatRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new LlmChatResponse
            {
                Content = _response,
                Endpoint = profile.Endpoint.TrimEnd('/') + "/chat/completions",
                Model = profile.Model
            });
        }
    }

    private sealed class InMemorySettingsRepository : IAppSettingsRepository
    {
        private readonly AppSettings _settings = new AppSettings
        {
            DefaultLlmProfileId = "local-main",
            LlmProfiles = new List<LlmEndpointSettings>
            {
                new LlmEndpointSettings
                {
                    Id = "local-main",
                    Title = "Local",
                    Endpoint = "http://127.0.0.1:1234/v1",
                    Model = "test-model"
                }
            }
        };

        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_settings);
        }

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
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
}
