using LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Tests.Application.RuntimeChunkDeltaTraversal;

public static class RuntimeChunkDeltaTraversalTestFactory
{
    public static RuntimeChunkDeltaEvidenceService CreateService(
        IRuntimeChunkDeltaPersistenceAdapter? persistenceAdapter = null) =>
        new(persistenceAdapter ?? new RealRuntimeChunkDeltaPersistenceAdapter());
}

public sealed class RealRuntimeChunkDeltaPersistenceAdapter : IRuntimeChunkDeltaPersistenceAdapter
{
    public RuntimeChunkPersistenceResult RoundTrip(RuntimeChunkPersistenceRequest request)
    {
        var serializer = new RuntimeStateSerializer();
        var snapshotStore = new RuntimeSnapshotStore(serializer);
        var serialized = serializer.Serialize(request.State);
        var restored = serializer.DeserializeGameRuntimeState(serialized);
        var restoredEvidence = RuntimeChunkDeltaProjector.ExtractStateEvidence(restored);
        var serializerRoundtrip = DictionaryEquals(request.ExpectedStateEvidence, restoredEvidence);
        var snapshotProjectRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "RuntimeChunkDeltaTraversal", Guid.NewGuid().ToString("N"));
        var cleanupSucceeded = false;
        RuntimeSnapshotResult save;
        RuntimeSnapshotResult load;
        try
        {
            save = snapshotStore.SaveSnapshot(snapshotProjectRoot, request.SlotName, new UnifiedRuntimeSession
            {
                GameplayState = request.State,
                Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["goal"] = "039",
                    ["scenarioId"] = request.ScenarioId
                }
            });
            load = snapshotStore.LoadSnapshot(snapshotProjectRoot, request.SlotName);
            if (Directory.Exists(snapshotProjectRoot))
            {
                Directory.Delete(snapshotProjectRoot, recursive: true);
            }

            cleanupSucceeded = !Directory.Exists(snapshotProjectRoot);
        }
        finally
        {
            if (Directory.Exists(snapshotProjectRoot))
            {
                Directory.Delete(snapshotProjectRoot, recursive: true);
            }
        }

        var restoredFromSnapshot = load.Session?.GameplayState ?? new GameRuntimeState();
        var restoredFromSnapshotEvidence = RuntimeChunkDeltaProjector.ExtractStateEvidence(restoredFromSnapshot);
        var restoredSerialized = serializer.Serialize(restoredFromSnapshot);
        var diagnostics = save.Diagnostics
            .Concat(load.Diagnostics)
            .Select(item => new RuntimeChunkDeltaDiagnostic
            {
                Severity = item.Severity,
                Code = item.Code,
                Target = item.TargetId ?? request.ScenarioId,
                Message = item.Message
            })
            .ToList();

        return new RuntimeChunkPersistenceResult
        {
            ScenarioId = request.ScenarioId,
            RuntimeAttempted = true,
            UsedRuntimeStateSerializer = true,
            UsedRuntimeSnapshotStore = true,
            SerializerType = typeof(RuntimeStateSerializer).FullName ?? nameof(RuntimeStateSerializer),
            SnapshotStoreType = typeof(RuntimeSnapshotStore).FullName ?? nameof(RuntimeSnapshotStore),
            SerializedFullState = true,
            SerializedStateHash = RuntimeChunkDeltaProjector.ComputeHash(serialized),
            RestoredSerializedStateHash = RuntimeChunkDeltaProjector.ComputeHash(restoredSerialized),
            SerializerRoundtripPassed = serializerRoundtrip,
            SnapshotSlotName = request.SlotName,
            SnapshotSaveSucceeded = save.Success,
            SnapshotLoadSucceeded = load.Success,
            SnapshotRoundtripPassed = save.Success
                && load.Success
                && DictionaryEquals(request.ExpectedStateEvidence, restoredFromSnapshotEvidence),
            TempSnapshotCleanupSucceeded = cleanupSucceeded,
            RestoredStateEvidence = restoredFromSnapshotEvidence,
            Diagnostics = diagnostics
        };
    }

    private static bool DictionaryEquals(
        IReadOnlyDictionary<string, string> left,
        IReadOnlyDictionary<string, string> right) =>
        left.Count == right.Count &&
        left.All(pair => right.TryGetValue(pair.Key, out var value) && string.Equals(pair.Value, value, StringComparison.Ordinal));
}
