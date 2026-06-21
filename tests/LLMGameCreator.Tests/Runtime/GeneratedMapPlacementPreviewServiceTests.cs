using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Runtime;

public sealed class GeneratedMapPlacementPreviewServiceTests
{
    [Fact]
    public void Build_CreatesDeterministicNpcAndEncounterMarkersOnResolvedMap()
    {
        var package = CreatePackage();
        var state = new GameState
        {
            CurrentMapId = "map/start",
            PlayerPosition = new Position2D(1, 1)
        };
        var preview = new GeneratedPackageRuntimePreviewService().Build(package, state);
        var service = new GeneratedMapPlacementPreviewService();

        var first = service.Build(package, state, preview);
        var second = service.Build(package, state, preview);

        var npc = Assert.Single(first.Markers, marker => marker.Type == GeneratedRuntimeMapMarkerType.Npc);
        var encounter = Assert.Single(first.Markers, marker => marker.Type == GeneratedRuntimeMapMarkerType.Encounter);
        Assert.Equal("map/start", npc.MapId);
        Assert.Equal("map/start", encounter.MapId);
        Assert.Contains("dialogue/guide", npc.ReferenceIds);
        Assert.Contains("Guide Introduction", npc.DetailsText);
        Assert.Contains("npc/guide", encounter.ReferenceIds);
        Assert.NotEqual((state.PlayerPosition.X, state.PlayerPosition.Y), (npc.Position.X, npc.Position.Y));
        Assert.NotEqual((state.PlayerPosition.X, state.PlayerPosition.Y), (encounter.Position.X, encounter.Position.Y));
        Assert.NotEqual((npc.Position.X, npc.Position.Y), (encounter.Position.X, encounter.Position.Y));
        Assert.All(first.Markers, marker =>
        {
            Assert.InRange(marker.Position.X, 0, 3);
            Assert.InRange(marker.Position.Y, 0, 3);
        });
        Assert.Equal(
            first.Markers.Select(marker => (marker.MarkerId, marker.MapId, marker.Position.X, marker.Position.Y)),
            second.Markers.Select(marker => (marker.MarkerId, marker.MapId, marker.Position.X, marker.Position.Y)));
    }

    [Fact]
    public void Build_MissingReferencesUseCurrentMapAndProduceWarning()
    {
        var package = CreatePackage();
        package.GeneratedContent.Npcs[0].SceneId = "scene/missing";
        package.GeneratedContent.Npcs[0].RegionId = "region/missing";
        package.GeneratedContent.Encounters.Clear();
        var state = new GameState
        {
            CurrentMapId = "map/start",
            PlayerPosition = new Position2D(1, 1)
        };
        var preview = new GeneratedPackageRuntimePreviewService().Build(package, state);

        var result = new GeneratedMapPlacementPreviewService().Build(package, state, preview);

        var marker = Assert.Single(result.Markers);
        Assert.Equal("map/start", marker.MapId);
        Assert.NotEmpty(marker.Warning);
        Assert.Contains("fallback map", marker.DetailsText);
    }

    internal static GamePackageDefinition CreatePackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest
            {
                PackageId = "game/map-placement",
                Title = "Map Placement",
                StartMapId = "map/start"
            },
            Game = new GameDefinition
            {
                TilePrototypes = new List<TilePrototypeDefinition>
                {
                    new() { Id = "tile/floor", Name = "Floor", Walkable = true }
                },
                Maps = new List<MapDefinition>
                {
                    new()
                    {
                        Id = "map/start",
                        Name = "Start",
                        Width = 4,
                        Height = 4,
                        DefaultTileId = "tile/floor",
                        StartPosition = new Position2D(1, 1)
                    }
                }
            },
            GeneratedContent = new GeneratedContentDefinition
            {
                Scenes = new List<GeneratedSceneDefinition>
                {
                    new() { SourceId = "scene/start", PackageMapId = "map/start", Title = "Start" }
                },
                Regions = new List<GeneratedRegionDefinition>
                {
                    new() { SourceId = "region/start", SceneIds = new List<string> { "scene/start" } }
                },
                Npcs = new List<GeneratedNpcDefinition>
                {
                    new()
                    {
                        SourceId = "npc/guide",
                        Name = "Guide",
                        Description = "A generated guide.",
                        RegionId = "region/start",
                        SceneId = "scene/start"
                    }
                },
                Dialogues = new List<GeneratedDialogueDefinition>
                {
                    new()
                    {
                        SourceId = "dialogue/guide",
                        Title = "Guide Introduction",
                        NpcId = "npc/guide",
                        SceneId = "scene/start"
                    }
                },
                Encounters = new List<GeneratedEncounterDefinition>
                {
                    new()
                    {
                        SourceId = "encounter/road",
                        Title = "Road Encounter",
                        Description = "A preview-only setup.",
                        RegionId = "region/start",
                        SceneId = "scene/missing",
                        NpcIds = new List<string> { "npc/guide" }
                    }
                }
            }
        };
    }
}
