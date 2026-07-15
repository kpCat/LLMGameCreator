using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed record GeneratedWorldRegionMapBinding
{
    public string RegionId { get; init; } = string.Empty;
    public string RegionTitle { get; init; } = string.Empty;
    public string GeneratedRegionSourceId { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public string MapTitle { get; init; } = string.Empty;
}

public sealed record GeneratedWorldTravelConnectionBinding
{
    public string ConnectionId { get; init; } = string.Empty;
    public string FromRegionId { get; init; } = string.Empty;
    public string ToRegionId { get; init; } = string.Empty;
    public string SourceMapId { get; init; } = string.Empty;
    public string DestinationMapId { get; init; } = string.Empty;
}

public sealed record GeneratedWorldRegionMapBindingResult
{
    public bool Passed { get; init; }
    public IReadOnlyList<GeneratedWorldRegionMapBinding> RegionBindings { get; init; } = [];
    public IReadOnlyList<GeneratedWorldTravelConnectionBinding> ConnectionBindings { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed class GeneratedWorldRegionMapBindingService
{
    public GeneratedWorldRegionMapBindingResult Bind(
        SeededGeneratedProjectSourceValidationResult source,
        GamePackageDefinition compatibilityPackage)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(compatibilityPackage);
        if (!source.Present || !source.Passed || source.RegeneratedPlan is null)
            return Failed("generated_travel.source_not_current");

        var plan = source.RegeneratedPlan;
        var diagnostics = new List<string>();
        var bindings = new List<GeneratedWorldRegionMapBinding>();
        foreach (var region in plan.World.Regions.OrderBy(item => item.RegionId, StringComparer.Ordinal))
        {
            var expectedSourceId = GeneratedSourceId(region.RegionId);
            var candidates = compatibilityPackage.GeneratedContent.Regions
                .Where(item => string.Equals(item.SourceId, region.RegionId, StringComparison.Ordinal)
                               || string.Equals(item.SourceId, expectedSourceId, StringComparison.Ordinal))
                .ToList();
            if (candidates.Count == 0)
            {
                diagnostics.Add("generated_travel.region_binding_missing:" + region.RegionId);
                continue;
            }
            if (candidates.Count != 1)
            {
                diagnostics.Add("generated_travel.region_binding_ambiguous:" + region.RegionId);
                continue;
            }

            var sceneIds = candidates[0].SceneIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            if (sceneIds.Count == 0)
            {
                diagnostics.Add("generated_travel.region_binding_missing:" + region.RegionId);
                continue;
            }
            if (sceneIds.Count != 1)
            {
                diagnostics.Add("generated_travel.region_binding_ambiguous:" + region.RegionId);
                continue;
            }

            var maps = compatibilityPackage.Game.Maps
                .Where(map => string.Equals(map.Id, sceneIds[0], StringComparison.Ordinal))
                .ToList();
            if (maps.Count != 1)
            {
                diagnostics.Add("generated_travel.destination_map_missing:" + sceneIds[0]);
                continue;
            }
            bindings.Add(new GeneratedWorldRegionMapBinding
            {
                RegionId = region.RegionId,
                RegionTitle = region.Label,
                GeneratedRegionSourceId = candidates[0].SourceId,
                MapId = maps[0].Id,
                MapTitle = maps[0].Name
            });
        }

        var planRegionIds = plan.World.Regions.Select(item => item.RegionId)
            .ToHashSet(StringComparer.Ordinal);
        var duplicateConnections = plan.World.Connections
            .GroupBy(item => item.ConnectionId, StringComparer.Ordinal)
            .Where(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() != 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);
        diagnostics.AddRange(duplicateConnections.Select(id =>
            "generated_travel.connection_duplicate:" + id));
        if (plan.World.Connections.Count == 0)
            diagnostics.Add("generated_travel.connection_missing");

        var bindingByRegion = bindings.ToDictionary(item => item.RegionId, StringComparer.Ordinal);
        var connectionBindings = new List<GeneratedWorldTravelConnectionBinding>();
        foreach (var connection in plan.World.Connections.OrderBy(item => item.ConnectionId, StringComparer.Ordinal))
        {
            if (duplicateConnections.Contains(connection.ConnectionId)) continue;
            if (!planRegionIds.Contains(connection.FromRegionId)
                || !planRegionIds.Contains(connection.ToRegionId)
                || !bindingByRegion.ContainsKey(connection.FromRegionId)
                || !bindingByRegion.ContainsKey(connection.ToRegionId))
            {
                diagnostics.Add("generated_travel.connection_region_missing:" + connection.ConnectionId);
                continue;
            }
            if (string.Equals(connection.FromRegionId, connection.ToRegionId, StringComparison.Ordinal))
            {
                diagnostics.Add("generated_travel.connection_self_loop:" + connection.ConnectionId);
                continue;
            }

            connectionBindings.Add(new GeneratedWorldTravelConnectionBinding
            {
                ConnectionId = connection.ConnectionId,
                FromRegionId = connection.FromRegionId,
                ToRegionId = connection.ToRegionId,
                SourceMapId = bindingByRegion[connection.FromRegionId].MapId,
                DestinationMapId = bindingByRegion[connection.ToRegionId].MapId
            });
        }

        diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal).ToList();
        return new GeneratedWorldRegionMapBindingResult
        {
            Passed = diagnostics.Count == 0
                     && bindings.Count == plan.World.Regions.Count
                     && connectionBindings.Count == plan.World.Connections.Count,
            RegionBindings = bindings.OrderBy(item => item.RegionId, StringComparer.Ordinal).ToList(),
            ConnectionBindings = connectionBindings.OrderBy(item => item.ConnectionId, StringComparer.Ordinal).ToList(),
            Diagnostics = diagnostics
        };
    }

    private static string GeneratedSourceId(string sourceId) =>
        sourceId.StartsWith("generated/", StringComparison.Ordinal)
            ? sourceId
            : "generated/" + sourceId;

    private static GeneratedWorldRegionMapBindingResult Failed(string diagnostic) => new()
    {
        Passed = false,
        Diagnostics = [diagnostic]
    };
}
