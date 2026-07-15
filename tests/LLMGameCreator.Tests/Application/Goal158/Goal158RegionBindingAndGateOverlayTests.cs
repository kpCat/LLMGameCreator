using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal156;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal158;

[Collection(Goal156Collection.Name)]
public sealed class Goal158RegionBindingAndGateOverlayTests
{
    [Fact]
    public void Behavioral_every_generated_region_has_one_exact_map_binding()
    {
        var (source, package) = SourceAndPackage();

        var result = new GeneratedWorldRegionMapBindingService().Bind(source, package);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(source.RegeneratedPlan!.World.Regions.Count, result.RegionBindings.Count);
        Assert.All(result.RegionBindings, binding => Assert.Single(package.Game.Maps, map => map.Id == binding.MapId));
    }

    [Fact]
    public void Behavioral_one_gate_is_created_for_every_directed_connection()
    {
        var (source, package) = SourceAndPackage();

        var result = new GeneratedWorldTravelOverlayService().Build(source, package);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(source.RegeneratedPlan!.World.Connections.Count, result.Document.GateCount);
        Assert.Equal(result.Document.ConnectionCount, result.Document.GateCount);
    }

    [Fact]
    public void Behavioral_route_selection_is_data_driven_and_reachable()
    {
        var (source, package) = SourceAndPackage();
        var overlay = new GeneratedWorldTravelOverlayService().Build(source, package);

        var route = new GeneratedWorldTravelRoutePlanner().Plan(source, overlay.PlayerCompositionPackage);

        Assert.True(route.Passed, string.Join(Environment.NewLine, route.Diagnostics));
        Assert.NotEqual(route.OriginRegionId, route.DestinationRegionId);
        Assert.NotEmpty(route.ConnectionIds);
        Assert.Equal(route.ConnectionIds.Count + 1, route.VisitedRegionIds.Count);
    }

    [Fact]
    public void Behavioral_actual_runtime_executes_origin_travel_and_destination_route()
    {
        var (source, package) = SourceAndPackage();
        var overlay = new GeneratedWorldTravelOverlayService().Build(source, package);

        var result = new GameProjectGeneratedRegionTravelActivationService(
            new DefaultGameRuntime(), new RuntimeStateSerializer()).Activate(new()
            {
                GeneratedSource = source,
                PlayerPackage = overlay.PlayerCompositionPackage
            });

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.True(result.Summary.OriginInteractionObserved);
        Assert.True(result.Summary.TravelGateInteractionsPassed);
        Assert.True(result.Summary.DestinationInteractionObserved);
        Assert.Contains(result.Summary.RuntimeFrames, frame => frame.Category == "generated_travel");
    }

    [Fact]
    public void Behavioral_route_replay_and_state_roundtrip_are_exact()
    {
        var (source, package) = SourceAndPackage();
        var overlay = new GeneratedWorldTravelOverlayService().Build(source, package);

        var result = new GameProjectGeneratedRegionTravelActivationService(
            new DefaultGameRuntime(), new RuntimeStateSerializer()).Activate(new()
            {
                GeneratedSource = source,
                PlayerPackage = overlay.PlayerCompositionPackage
            });

        Assert.True(result.Summary.ReplayEquivalent, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.True(result.Summary.StateRoundtripPassed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(result.Summary.FinalStateHash, result.Summary.ReplayFinalStateHash);
    }

    [Fact]
    public void Behavioral_missing_region_binding_is_rejected()
    {
        var (source, original) = SourceAndPackage();
        var package = Goal158TestKit.Clone(original);
        var region = source.RegeneratedPlan!.World.Regions[0];
        package.GeneratedContent.Regions.RemoveAll(item =>
            item.SourceId == region.RegionId || item.SourceId == "generated/" + region.RegionId);

        var result = new GeneratedWorldRegionMapBindingService().Bind(source, package);

        Assert.False(result.Passed);
        Assert.Contains(result.Diagnostics, item => item.StartsWith(
            "generated_travel.region_binding_missing:", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_ambiguous_region_binding_is_rejected()
    {
        var (source, original) = SourceAndPackage();
        var package = Goal158TestKit.Clone(original);
        var region = source.RegeneratedPlan!.World.Regions[0];
        var existing = package.GeneratedContent.Regions.Single(item =>
            item.SourceId == region.RegionId || item.SourceId == "generated/" + region.RegionId);
        package.GeneratedContent.Regions.Add(Goal158TestKit.Clone(existing));

        var result = new GeneratedWorldRegionMapBindingService().Bind(source, package);

        Assert.False(result.Passed);
        Assert.Contains(result.Diagnostics, item => item.StartsWith(
            "generated_travel.region_binding_ambiguous:", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("duplicate", "generated_travel.connection_duplicate")]
    [InlineData("missing", "generated_travel.connection_region_missing")]
    [InlineData("self", "generated_travel.connection_self_loop")]
    public void Behavioral_invalid_plan_connections_are_rejected(string mutation, string diagnostic)
    {
        var (source, package) = SourceAndPackage();
        var connections = source.RegeneratedPlan!.World.Connections.ToList();
        connections = mutation switch
        {
            "duplicate" => connections.Concat([connections[0]]).ToList(),
            "missing" => [connections[0] with { ToRegionId = "region/not-present" }, .. connections.Skip(1)],
            _ => [connections[0] with { ToRegionId = connections[0].FromRegionId }, .. connections.Skip(1)]
        };
        var mutated = source with
        {
            RegeneratedPlan = source.RegeneratedPlan with
            {
                World = source.RegeneratedPlan.World with { Connections = connections }
            }
        };

        var result = new GeneratedWorldRegionMapBindingService().Bind(mutated, package);

        Assert.False(result.Passed);
        Assert.Contains(result.Diagnostics, item => item.StartsWith(diagnostic, StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_gate_ids_are_deterministic_and_unique()
    {
        var (source, package) = SourceAndPackage();
        var result = new GeneratedWorldTravelOverlayService().Build(source, package);

        Assert.Equal(result.Document.GateCount,
            result.Document.GateFingerprints.Select(item => item.EntityId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(result.Document.GateFingerprints, gate => Assert.Equal(
            GeneratedWorldTravelOverlayService.GateEntityId(gate.ConnectionId), gate.EntityId));
    }

    [Fact]
    public void Behavioral_gate_args_match_exact_connection_and_region_bindings()
    {
        var (source, package) = SourceAndPackage();
        var result = new GeneratedWorldTravelOverlayService().Build(source, package);

        foreach (var connection in result.Binding.ConnectionBindings)
        {
            var entity = Assert.Single(result.TravelOverlayPackage.Game.Maps
                .Single(map => map.Id == connection.SourceMapId).Entities,
                item => item.Id == GeneratedWorldTravelOverlayService.GateEntityId(connection.ConnectionId));
            var args = Assert.Single(entity.Components,
                item => item.Type == MapTransitionInteractionContract.ComponentType).Args;
            Assert.Equal(connection.ConnectionId, args[MapTransitionInteractionContract.ConnectionIdKey]);
            Assert.Equal(connection.SourceMapId, args[MapTransitionInteractionContract.SourceMapIdKey]);
            Assert.Equal(connection.DestinationMapId, args[MapTransitionInteractionContract.DestinationMapIdKey]);
            Assert.Equal(connection.FromRegionId, args[MapTransitionInteractionContract.FromRegionIdKey]);
            Assert.Equal(connection.ToRegionId, args[MapTransitionInteractionContract.ToRegionIdKey]);
        }
    }

    [Fact]
    public void Behavioral_gate_positions_are_walkable_distinct_reachable_and_not_reused()
    {
        var (source, package) = SourceAndPackage();
        var existingPositions = package.Game.Maps.ToDictionary(
            map => map.Id,
            map => map.Entities.Select(entity => (entity.Position.X, entity.Position.Y)).ToHashSet(),
            StringComparer.Ordinal);
        var result = new GeneratedWorldTravelOverlayService().Build(source, package);

        Assert.All(result.Document.GateFingerprints, gate =>
        {
            Assert.DoesNotContain((gate.X, gate.Y), existingPositions[gate.SourceMapId]);
            Assert.NotEqual((gate.X, gate.Y), (gate.ApproachX, gate.ApproachY));
            var map = result.TravelOverlayPackage.Game.Maps.Single(item => item.Id == gate.SourceMapId);
            var tileId = map.Tiles.FirstOrDefault(item => item.X == gate.X && item.Y == gate.Y)?.TileId
                         ?? map.DefaultTileId;
            Assert.True(result.TravelOverlayPackage.Game.TilePrototypes.Single(item => item.Id == tileId).Walkable);
        });
        Assert.Equal(result.Document.GateCount, result.Document.GateFingerprints
            .Select(item => (item.SourceMapId, item.X, item.Y)).Distinct().Count());
    }

    [Fact]
    public void Behavioral_insufficient_safe_cells_reject_gate_overlay()
    {
        var (source, original) = SourceAndPackage();
        var package = Goal158TestKit.Clone(original);
        foreach (var map in package.Game.Maps)
        {
            map.Width = 1;
            map.Height = 1;
            map.StartPosition = new Position2D(0, 0);
            map.Entities.Clear();
            map.Tiles.Clear();
        }

        var result = new GeneratedWorldTravelOverlayService().Build(source, package);

        Assert.False(result.Passed);
        Assert.Contains(result.Diagnostics, item => item.StartsWith(
            "generated_travel.gate_placement_insufficient:", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_repeated_travel_overlay_is_byte_deterministic()
    {
        var (source, package) = SourceAndPackage();
        var service = new GeneratedWorldTravelOverlayService();

        var first = service.Build(source, package);
        var second = service.Build(source, package);

        Assert.Equal(first.TravelOverlayPackageJson, second.TravelOverlayPackageJson);
        Assert.Equal(first.PlayerCompositionPackageJson, second.PlayerCompositionPackageJson);
        Assert.Equal(first.Document.TravelOverlaySha256, second.Document.TravelOverlaySha256);
    }

    [Fact]
    public void Behavioral_preexisting_records_are_canonical_equal_after_authorized_additions_are_removed()
    {
        var (source, package) = SourceAndPackage();
        var result = new GeneratedWorldTravelOverlayService().Build(source, package);
        var stripped = Goal158TestKit.Clone(result.PlayerCompositionPackage);
        stripped.Manifest.StartMapId = package.Manifest.StartMapId;
        stripped.Game.EntityPrototypes.RemoveAll(item =>
            item.Id == GeneratedWorldTravelOverlayService.TravelPrototypeId);
        foreach (var map in stripped.Game.Maps)
            map.Entities.RemoveAll(item => item.Id.StartsWith(
                GeneratedWorldTravelOverlayService.TravelEntityIdPrefix, StringComparison.Ordinal));

        Assert.True(result.Document.ControlledDeltaPassed);
        Assert.Equal(Goal158TestKit.Canonical(package), Goal158TestKit.Canonical(stripped));
    }

    [Fact]
    public void Behavioral_unequal_travel_prototype_collision_is_rejected()
    {
        var (source, original) = SourceAndPackage();
        var package = Goal158TestKit.Clone(original);
        package.Game.EntityPrototypes.Add(new EntityPrototypeDefinition
        {
            Id = GeneratedWorldTravelOverlayService.TravelPrototypeId,
            Name = "Collision"
        });

        var result = new GeneratedWorldTravelOverlayService().Build(source, package);

        Assert.False(result.Passed);
        Assert.Contains("generated_travel.id_collision:" + GeneratedWorldTravelOverlayService.TravelPrototypeId,
            result.Diagnostics);
    }

    [Fact]
    public void Behavioral_goal156_source_sidecars_remain_byte_identical_after_overlay()
    {
        var root = Path.Combine(Goal156TestKit.AllSelectable.Path, SeededGeneratedProjectVocabulary.GenerationRelativeRoot);
        var before = Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly)
            .ToDictionary(path => Path.GetFileName(path)!, Goal158TestKit.FileHash, StringComparer.Ordinal);
        var (source, package) = SourceAndPackage();

        _ = new GeneratedWorldTravelOverlayService().Build(source, package);

        var after = Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly)
            .ToDictionary(path => Path.GetFileName(path)!, Goal158TestKit.FileHash, StringComparer.Ordinal);
        Assert.Equal(before.OrderBy(item => item.Key), after.OrderBy(item => item.Key));
    }

    private static (SeededGeneratedProjectSourceValidationResult Source,
        LLMGameCreator.GamePackage.GamePackageDefinition Package) SourceAndPackage()
    {
        var source = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path);
        Assert.True(source.Passed, string.Join(Environment.NewLine, source.Diagnostics));
        return (source, source.GeneratedBasePackage!);
    }
}

internal static class Goal158TestKit
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    public static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(Canonical(value), JsonOptions)!;
    public static string Canonical<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    public static (SeededGeneratedProjectSourceValidationResult Source, GeneratedWorldTravelOverlayResult Overlay)
        Overlay()
    {
        var source = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path);
        var overlay = new GeneratedWorldTravelOverlayService().Build(source, source.GeneratedBasePackage!);
        Assert.True(overlay.Passed, string.Join(Environment.NewLine, overlay.Diagnostics));
        return (source, overlay);
    }

    public static GameProjectGeneratedRegionTravelActivationResult Activate(
        IGameRuntime? runtime = null,
        IRuntimeStateSerializer? serializer = null)
    {
        var (source, overlay) = Overlay();
        return new GameProjectGeneratedRegionTravelActivationService(
            runtime ?? new DefaultGameRuntime(),
            serializer ?? new RuntimeStateSerializer()).Activate(new()
            {
                GeneratedSource = source,
                PlayerPackage = overlay.PlayerCompositionPackage
            });
    }

    public static string FileHash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
