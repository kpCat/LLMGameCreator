using System.Text.Json.Serialization;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;

public static class RuntimeChunkDeltaTraversalVocabulary
{
    public const string SchemaVersion = "runtime_chunk_delta_traversal_smoke_v1";
    public const string GoalId = "goal_039_runtime_chunk_delta_traversal_smoke";
    public const string FinalGate = "runtime_chunk_delta_traversal_smoke_verification";
    public const string ProductSmokeRoute = "runtime-chunk-delta-traversal-smoke";

    public static readonly IReadOnlySet<string> Scenarios = new HashSet<string>(
        ["frontier_survival", "gothic_intrigue", "caravan_trade", "metamodule_kingdoms"],
        StringComparer.Ordinal);
}

public sealed record RuntimeChunkBoundaryClaims
{
    public bool RuntimeSourceMutation { get; init; }
    public bool UiWinForms { get; init; }
    public bool Unity { get; init; }
    public bool GamePackageDefinitionsMutation { get; init; }
    public bool ProviderLlmRag { get; init; }
    public bool LuaSourceOrExecution { get; init; }
    public bool GeneratorLibrary { get; init; }
    public bool ExternalDependency { get; init; }
    public bool Filesystem { get; init; }
    public bool Network { get; init; }
    public bool Process { get; init; }
    public bool Reflection { get; init; }
    public bool Thread { get; init; }
    public bool Time { get; init; }
    public bool Random { get; init; }
    public bool NativeInterop { get; init; }

    [JsonIgnore]
    public bool AllFalse =>
        !RuntimeSourceMutation &&
        !UiWinForms &&
        !Unity &&
        !GamePackageDefinitionsMutation &&
        !ProviderLlmRag &&
        !LuaSourceOrExecution &&
        !GeneratorLibrary &&
        !ExternalDependency &&
        !Filesystem &&
        !Network &&
        !Process &&
        !Reflection &&
        !Thread &&
        !Time &&
        !Random &&
        !NativeInterop;
}

public sealed record RuntimeChunkGoal038SourceFacts
{
    public string ScenarioId { get; init; } = string.Empty;
    public string WorldGraphId { get; init; } = string.Empty;
    public string FiniteMapId { get; init; } = string.Empty;
    public string CoordinateKind { get; init; } = string.Empty;
    public string ScenarioWorldSeed { get; init; } = string.Empty;
    public int ChunkSize { get; init; }
    public int RegionCount { get; init; }
    public int TravelEdgeCount { get; init; }
    public int KingdomGroupCount { get; init; }
    public int SpeciesArchetypeSlotRefCount { get; init; }
    public IReadOnlyList<string> Goal038EvidenceRefs { get; init; } = [];
}

public sealed record RuntimeChunkTraversalPlan
{
    public string SchemaVersion { get; init; } = "runtime_chunk_traversal_plan_v1";
    public string GoalId { get; init; } = RuntimeChunkDeltaTraversalVocabulary.GoalId;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string WorldGraphId { get; init; } = string.Empty;
    public string FiniteMapId { get; init; } = string.Empty;
    public string CoordinateKind { get; init; } = string.Empty;
    public string ReplaySeed { get; init; } = string.Empty;
    public string StartRegionId { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredTargetRegionIds { get; init; } = [];
    public RuntimeChunkGoal038SourceFacts SourceFacts { get; init; } = new();
    public IReadOnlyList<RuntimeChunkTraversalStep> Steps { get; init; } = [];
    public IReadOnlyList<RuntimeChunkDeltaCommand> Commands { get; init; } = [];
    public RuntimeChunkBoundaryClaims BoundaryClaims { get; init; } = new();
}

public sealed record RuntimeChunkTraversalStep
{
    public int StepIndex { get; init; }
    public string RegionId { get; init; } = string.Empty;
    public string? ArrivedByEdgeId { get; init; }
    public string ChunkId { get; init; } = string.Empty;
    public string Coordinate { get; init; } = string.Empty;
    public string LandmarkId { get; init; } = string.Empty;
    public string RouteCheckpointMarkerId { get; init; } = string.Empty;
    public string LocalMutationId { get; init; } = string.Empty;
    public string LocalMutationKind { get; init; } = string.Empty;
}

public sealed record RuntimeChunkDeltaCommand
{
    public string DeltaId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string ScenarioId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string ChunkId { get; init; } = string.Empty;
    public string DeltaKind { get; init; } = string.Empty;
    public string MarkerId { get; init; } = string.Empty;
    public string? SourceEdgeId { get; init; }
    public string MutationKey { get; init; } = string.Empty;
    public string MutationValue { get; init; } = string.Empty;
    public string ReplaySeed { get; init; } = string.Empty;
}

public sealed record RuntimeChunkDeltaRecord
{
    public string DeltaId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string ChunkId { get; init; } = string.Empty;
    public string DeltaKind { get; init; } = string.Empty;
    public string MarkerId { get; init; } = string.Empty;
    public string MutationKey { get; init; } = string.Empty;
    public string MutationValue { get; init; } = string.Empty;
    public bool RuntimeSaveOnly { get; init; } = true;
}

public sealed record RuntimeChunkDeltaStateSnapshot
{
    public string SchemaVersion { get; init; } = "runtime_chunk_delta_state_v1";
    public string ScenarioId { get; init; } = string.Empty;
    public string RuntimeStateOwner { get; init; } = nameof(GameRuntimeState);
    public string RegionId { get; init; } = string.Empty;
    public string ChunkId { get; init; } = string.Empty;
    public IReadOnlyList<string> VisitedRegionIds { get; init; } = [];
    public IReadOnlyList<string> DiscoveredChunkIds { get; init; } = [];
    public IReadOnlyList<string> LandmarkDiscoveryIds { get; init; } = [];
    public IReadOnlyList<string> RouteCheckpointMarkerIds { get; init; } = [];
    public IReadOnlyDictionary<string, string> LocalMutations { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<RuntimeChunkDeltaRecord> RuntimeDeltas { get; init; } = [];
    public IReadOnlyList<string> DeterministicReplayMarkers { get; init; } = [];
}

public sealed record RuntimeChunkDeltaStateProof
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool RuntimeAttempted { get; init; }
    public string RuntimeStateOwner { get; init; } = nameof(GameRuntimeState);
    public bool StateChangedAfterTraversal { get; init; }
    public string BeforeStateHash { get; init; } = string.Empty;
    public string AfterStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ChangedStateKeys { get; init; } = [];
    public RuntimeChunkDeltaStateSnapshot Before { get; init; } = new();
    public RuntimeChunkDeltaStateSnapshot After { get; init; } = new();
    public IReadOnlyDictionary<string, string> AfterStateEvidence { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);

    [JsonIgnore]
    public GameRuntimeState RuntimeState { get; init; } = new();
}

public sealed record RuntimeChunkPersistenceRequest
{
    public string ScenarioId { get; init; } = string.Empty;
    public string SlotName { get; init; } = string.Empty;
    public GameRuntimeState State { get; init; } = new();
    public IReadOnlyDictionary<string, string> ExpectedStateEvidence { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record RuntimeChunkPersistenceResult
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool RuntimeAttempted { get; init; }
    public bool UsedRuntimeStateSerializer { get; init; }
    public bool UsedRuntimeSnapshotStore { get; init; }
    public string SerializerType { get; init; } = string.Empty;
    public string SnapshotStoreType { get; init; } = string.Empty;
    public bool SerializedFullState { get; init; }
    public string SerializedStateHash { get; init; } = string.Empty;
    public string RestoredSerializedStateHash { get; init; } = string.Empty;
    public bool SerializerRoundtripPassed { get; init; }
    public string SnapshotSlotName { get; init; } = string.Empty;
    public bool SnapshotSaveSucceeded { get; init; }
    public bool SnapshotLoadSucceeded { get; init; }
    public bool SnapshotRoundtripPassed { get; init; }
    public bool TempSnapshotCleanupSucceeded { get; init; }
    public IReadOnlyDictionary<string, string> RestoredStateEvidence { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<RuntimeChunkDeltaDiagnostic> Diagnostics { get; init; } = [];

    [JsonIgnore]
    public bool Passed =>
        RuntimeAttempted &&
        UsedRuntimeStateSerializer &&
        UsedRuntimeSnapshotStore &&
        SerializedFullState &&
        SerializerRoundtripPassed &&
        SnapshotSaveSucceeded &&
        SnapshotLoadSucceeded &&
        SnapshotRoundtripPassed &&
        Diagnostics.All(item => item.Severity != "error");
}

public interface IRuntimeChunkDeltaPersistenceAdapter
{
    RuntimeChunkPersistenceResult RoundTrip(RuntimeChunkPersistenceRequest request);
}

public sealed class MissingRuntimeChunkDeltaPersistenceAdapter : IRuntimeChunkDeltaPersistenceAdapter
{
    public RuntimeChunkPersistenceResult RoundTrip(RuntimeChunkPersistenceRequest request) =>
        new()
        {
            ScenarioId = request.ScenarioId,
            SnapshotSlotName = request.SlotName,
            Diagnostics =
            [
                RuntimeChunkDeltaDiagnostic.Error(
                    "runtime_chunk.persistence.adapter_missing",
                    request.ScenarioId,
                    "No runtime serializer/snapshot adapter was supplied.")
            ]
        };
}

public sealed record RuntimeChunkSaveLoadRoundtripProof
{
    public string SchemaVersion { get; init; } = "runtime_chunk_save_load_roundtrip_proof_v1";
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<RuntimeChunkPersistenceResult> Scenarios { get; init; } = [];
}

public sealed record RuntimeChunkReplayDeterminismProof
{
    public string SchemaVersion { get; init; } = "runtime_chunk_replay_determinism_proof_v1";
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<RuntimeChunkReplayScenarioProof> Scenarios { get; init; } = [];
}

public sealed record RuntimeChunkReplayScenarioProof
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ReplaySeed { get; init; } = string.Empty;
    public string FirstRunHash { get; init; } = string.Empty;
    public string SecondRunHash { get; init; } = string.Empty;
    public bool SameSeedDeterministic { get; init; }
    public int CommandCount { get; init; }
}

public sealed record RuntimeChunkInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<RuntimeChunkDeltaDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record RuntimeChunkInvalidMatrix
{
    public string SchemaVersion { get; init; } = "invalid_runtime_chunk_diagnostics_matrix_v1";
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public int BlockedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<RuntimeChunkInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record RuntimeChunkDeltaTraversalReport
{
    public string SchemaVersion { get; init; } = "runtime_chunk_delta_traversal_smoke_report_v1";
    public string GoalId { get; init; } = RuntimeChunkDeltaTraversalVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = RuntimeChunkDeltaTraversalVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = RuntimeChunkDeltaTraversalVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Goal038AcceptedByUserHandoff { get; init; }
    public int ScenarioCount { get; init; }
    public int TraversalPlanCount { get; init; }
    public int RuntimeStateProofCount { get; init; }
    public int RuntimeMutationScenarioCount { get; init; }
    public int TotalCommandCount { get; init; }
    public bool RuntimeStateChangedAfterTraversal { get; init; }
    public bool SaveLoadRoundtripPassed { get; init; }
    public bool ReplayDeterminismPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool GamePackageDefinitionsMutated { get; init; }
    public int MetamoduleKingdomGroupCount { get; init; }
    public int MetamoduleSpeciesArchetypeSlotRefCount { get; init; }
    public bool NoRuntimeUiUnityProviderLlmRagLuaGeneratorLibraryLeakage { get; init; }
    public string FrontierStateHash { get; init; } = string.Empty;
    public string MetamoduleStateHash { get; init; } = string.Empty;
    public string SaveLoadProofHash { get; init; } = string.Empty;
    public string ReplayProofHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<RuntimeChunkDeltaDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record RuntimeChunkDeltaEvidenceResult
{
    public IReadOnlyDictionary<string, RuntimeChunkTraversalPlan> PlansByFileName { get; init; } = new Dictionary<string, RuntimeChunkTraversalPlan>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, RuntimeChunkDeltaStateSnapshot> StatesByFileName { get; init; } = new Dictionary<string, RuntimeChunkDeltaStateSnapshot>(StringComparer.Ordinal);
    public RuntimeChunkSaveLoadRoundtripProof SaveLoadRoundtripProof { get; init; } = new();
    public RuntimeChunkReplayDeterminismProof ReplayDeterminismProof { get; init; } = new();
    public RuntimeChunkInvalidMatrix InvalidMatrix { get; init; } = new();
    public RuntimeChunkDeltaTraversalReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record RuntimeChunkDeltaEvidenceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record RuntimeChunkDeltaDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static RuntimeChunkDeltaDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static RuntimeChunkDeltaDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}
