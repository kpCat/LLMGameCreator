using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;

public static class RuntimeChunkDeltaProjector
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static IReadOnlyList<RuntimeChunkDeltaCommand> BuildCommands(RuntimeChunkTraversalPlan plan)
    {
        var commands = new List<RuntimeChunkDeltaCommand>();
        var order = 0;
        foreach (var step in plan.Steps.OrderBy(item => item.StepIndex))
        {
            commands.Add(Command(plan, step, ++order, "region_entered", step.RegionId, sourceEdgeId: step.ArrivedByEdgeId));
            commands.Add(Command(plan, step, ++order, "chunk_discovered", step.ChunkId, sourceEdgeId: step.ArrivedByEdgeId));

            if (!string.IsNullOrWhiteSpace(step.LandmarkId))
            {
                commands.Add(Command(plan, step, ++order, "landmark_discovered", step.LandmarkId, sourceEdgeId: step.ArrivedByEdgeId));
            }

            if (!string.IsNullOrWhiteSpace(step.RouteCheckpointMarkerId))
            {
                commands.Add(Command(plan, step, ++order, "route_checkpoint", step.RouteCheckpointMarkerId, sourceEdgeId: step.ArrivedByEdgeId));
            }

            if (!string.IsNullOrWhiteSpace(step.LocalMutationId))
            {
                commands.Add(Command(
                    plan,
                    step,
                    ++order,
                    "local_mutation",
                    step.LocalMutationId,
                    sourceEdgeId: step.ArrivedByEdgeId,
                    mutationKey: step.LocalMutationId,
                    mutationValue: step.LocalMutationKind));
            }
        }

        var finalStep = plan.Steps.OrderBy(item => item.StepIndex).Last();
        commands.Add(Command(plan, finalStep, ++order, "deterministic_replay_marker", $"replay/{plan.ScenarioId}/{ShortHash(plan.ReplaySeed)}"));
        return commands.OrderBy(item => item.Order).ToList();
    }

    public static RuntimeChunkDeltaStateProof Apply(RuntimeChunkTraversalPlan plan)
    {
        var state = new GameRuntimeState
        {
            PackageId = RuntimeChunkDeltaTraversalVocabulary.GoalId,
            CurrentMapId = plan.FiniteMapId,
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        };
        WriteSnapshot(state, new RuntimeChunkDeltaStateSnapshot
        {
            ScenarioId = plan.ScenarioId,
            RegionId = plan.StartRegionId,
            ChunkId = plan.Steps.First().ChunkId
        }, plan);

        var before = FromState(state);
        var beforeEvidence = ExtractStateEvidence(state);
        var beforeHash = ComputeHash(Serialize(beforeEvidence));

        foreach (var command in plan.Commands.OrderBy(item => item.Order))
        {
            ApplyCommand(state, command, plan);
        }

        var after = FromState(state);
        var afterEvidence = ExtractStateEvidence(state);
        var afterHash = ComputeHash(Serialize(afterEvidence));

        return new RuntimeChunkDeltaStateProof
        {
            ScenarioId = plan.ScenarioId,
            RuntimeAttempted = true,
            StateChangedAfterTraversal = !string.Equals(beforeHash, afterHash, StringComparison.Ordinal),
            BeforeStateHash = beforeHash,
            AfterStateHash = afterHash,
            ChangedStateKeys = ChangedKeys(beforeEvidence, afterEvidence),
            Before = before,
            After = after,
            AfterStateEvidence = afterEvidence,
            RuntimeState = state
        };
    }

    public static RuntimeChunkDeltaStateSnapshot FromState(GameRuntimeState state)
    {
        return new RuntimeChunkDeltaStateSnapshot
        {
            ScenarioId = state.Metadata.GetValueOrDefault("runtimeChunk.scenarioId", string.Empty),
            RegionId = state.Metadata.GetValueOrDefault("runtimeChunk.currentRegionId", string.Empty),
            ChunkId = state.Metadata.GetValueOrDefault("runtimeChunk.currentChunkId", string.Empty),
            VisitedRegionIds = Split(state.Metadata.GetValueOrDefault("runtimeChunk.visitedRegionIds", string.Empty)),
            DiscoveredChunkIds = Split(state.Metadata.GetValueOrDefault("runtimeChunk.discoveredChunkIds", string.Empty)),
            LandmarkDiscoveryIds = Split(state.Metadata.GetValueOrDefault("runtimeChunk.landmarkDiscoveryIds", string.Empty)),
            RouteCheckpointMarkerIds = Split(state.Metadata.GetValueOrDefault("runtimeChunk.routeCheckpointMarkerIds", string.Empty)),
            LocalMutations = DeserializeDictionary(state.Metadata.GetValueOrDefault("runtimeChunk.localMutations", "{}")),
            RuntimeDeltas = DeserializeList<RuntimeChunkDeltaRecord>(state.Metadata.GetValueOrDefault("runtimeChunk.runtimeDeltas", "[]")),
            DeterministicReplayMarkers = Split(state.Metadata.GetValueOrDefault("runtimeChunk.deterministicReplayMarkers", string.Empty))
        };
    }

    public static IReadOnlyDictionary<string, string> ExtractStateEvidence(GameRuntimeState state)
    {
        var evidence = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["currentMapId"] = state.CurrentMapId,
            ["packageId"] = state.PackageId,
            ["tick"] = state.Tick.ToString("0")
        };

        foreach (var pair in state.Metadata.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            evidence["metadata." + pair.Key] = pair.Value;
        }

        return evidence;
    }

    public static string ComputeHash(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static RuntimeChunkDeltaCommand Command(
        RuntimeChunkTraversalPlan plan,
        RuntimeChunkTraversalStep step,
        int order,
        string kind,
        string markerId,
        string? sourceEdgeId = null,
        string mutationKey = "",
        string mutationValue = "")
    {
        var stable = $"{plan.ScenarioId}|{order:000}|{kind}|{step.RegionId}|{step.ChunkId}|{markerId}";
        return new RuntimeChunkDeltaCommand
        {
            DeltaId = $"delta/{plan.ScenarioId}/{order:000}/{kind}/{ShortHash(stable)}",
            Order = order,
            ScenarioId = plan.ScenarioId,
            RegionId = step.RegionId,
            ChunkId = step.ChunkId,
            DeltaKind = kind,
            MarkerId = markerId,
            SourceEdgeId = sourceEdgeId,
            MutationKey = mutationKey,
            MutationValue = mutationValue,
            ReplaySeed = plan.ReplaySeed
        };
    }

    private static void ApplyCommand(GameRuntimeState state, RuntimeChunkDeltaCommand command, RuntimeChunkTraversalPlan plan)
    {
        var snapshot = FromState(state);
        var visitedRegions = snapshot.VisitedRegionIds.ToSortedSet();
        var discoveredChunks = snapshot.DiscoveredChunkIds.ToSortedSet();
        var landmarks = snapshot.LandmarkDiscoveryIds.ToSortedSet();
        var checkpoints = snapshot.RouteCheckpointMarkerIds.ToSortedSet();
        var mutations = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in snapshot.LocalMutations)
        {
            mutations[pair.Key] = pair.Value;
        }
        var replayMarkers = snapshot.DeterministicReplayMarkers.ToSortedSet();
        var deltas = snapshot.RuntimeDeltas.ToList();

        visitedRegions.Add(command.RegionId);
        if (command.DeltaKind is "chunk_discovered" or "region_entered" or "landmark_discovered" or "route_checkpoint" or "local_mutation")
        {
            discoveredChunks.Add(command.ChunkId);
        }

        if (command.DeltaKind == "landmark_discovered")
        {
            landmarks.Add(command.MarkerId);
        }
        else if (command.DeltaKind == "route_checkpoint")
        {
            checkpoints.Add(command.MarkerId);
        }
        else if (command.DeltaKind == "local_mutation")
        {
            mutations[command.MutationKey] = command.MutationValue;
        }
        else if (command.DeltaKind == "deterministic_replay_marker")
        {
            replayMarkers.Add(command.MarkerId);
        }

        deltas.Add(new RuntimeChunkDeltaRecord
        {
            DeltaId = command.DeltaId,
            ScenarioId = command.ScenarioId,
            RegionId = command.RegionId,
            ChunkId = command.ChunkId,
            DeltaKind = command.DeltaKind,
            MarkerId = command.MarkerId,
            MutationKey = command.MutationKey,
            MutationValue = command.MutationValue,
            RuntimeSaveOnly = true
        });

        WriteSnapshot(state, snapshot with
        {
            RegionId = command.RegionId,
            ChunkId = command.ChunkId,
            VisitedRegionIds = visitedRegions.ToList(),
            DiscoveredChunkIds = discoveredChunks.ToList(),
            LandmarkDiscoveryIds = landmarks.ToList(),
            RouteCheckpointMarkerIds = checkpoints.ToList(),
            LocalMutations = mutations,
            RuntimeDeltas = deltas.OrderBy(item => item.DeltaId, StringComparer.Ordinal).ToList(),
            DeterministicReplayMarkers = replayMarkers.ToList()
        }, plan);
        state.Tick++;
    }

    private static void WriteSnapshot(GameRuntimeState state, RuntimeChunkDeltaStateSnapshot snapshot, RuntimeChunkTraversalPlan plan)
    {
        state.Metadata["runtimeChunk.scenarioId"] = snapshot.ScenarioId;
        state.Metadata["runtimeChunk.worldGraphId"] = plan.WorldGraphId;
        state.Metadata["runtimeChunk.finiteMapId"] = plan.FiniteMapId;
        state.Metadata["runtimeChunk.replaySeed"] = plan.ReplaySeed;
        state.Metadata["runtimeChunk.sourceGoal"] = "Goal038";
        state.Metadata["runtimeChunk.currentRegionId"] = snapshot.RegionId;
        state.Metadata["runtimeChunk.currentChunkId"] = snapshot.ChunkId;
        state.Metadata["runtimeChunk.visitedRegionIds"] = Join(snapshot.VisitedRegionIds);
        state.Metadata["runtimeChunk.discoveredChunkIds"] = Join(snapshot.DiscoveredChunkIds);
        state.Metadata["runtimeChunk.landmarkDiscoveryIds"] = Join(snapshot.LandmarkDiscoveryIds);
        state.Metadata["runtimeChunk.routeCheckpointMarkerIds"] = Join(snapshot.RouteCheckpointMarkerIds);
        var sortedMutations = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in snapshot.LocalMutations)
        {
            sortedMutations[pair.Key] = pair.Value;
        }

        state.Metadata["runtimeChunk.localMutations"] = Serialize(sortedMutations);
        state.Metadata["runtimeChunk.runtimeDeltas"] = Serialize(snapshot.RuntimeDeltas.OrderBy(item => item.DeltaId, StringComparer.Ordinal).ToList());
        state.Metadata["runtimeChunk.deterministicReplayMarkers"] = Join(snapshot.DeterministicReplayMarkers);
    }

    private static SortedSet<string> ToSortedSet(this IEnumerable<string> values) =>
        new(values.Where(item => !string.IsNullOrWhiteSpace(item)), StringComparer.Ordinal);

    private static IReadOnlyList<string> Split(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Order(StringComparer.Ordinal)
                .ToList();

    private static string Join(IEnumerable<string> values) =>
        string.Join("|", values.Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));

    private static IReadOnlyList<T> DeserializeList<T>(string json) =>
        JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? [];

    private static IReadOnlyDictionary<string, string> DeserializeDictionary(string json) =>
        JsonSerializer.Deserialize<SortedDictionary<string, string>>(json, JsonOptions) ?? new SortedDictionary<string, string>(StringComparer.Ordinal);

    private static IReadOnlyList<string> ChangedKeys(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after) =>
        before.Keys.Concat(after.Keys)
            .Distinct(StringComparer.Ordinal)
            .Where(key => !before.TryGetValue(key, out var beforeValue)
                || !after.TryGetValue(key, out var afterValue)
                || !string.Equals(beforeValue, afterValue, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToList();

    private static string ShortHash(string value)
    {
        var hash = ComputeHash(value);
        return hash[..12];
    }
}
