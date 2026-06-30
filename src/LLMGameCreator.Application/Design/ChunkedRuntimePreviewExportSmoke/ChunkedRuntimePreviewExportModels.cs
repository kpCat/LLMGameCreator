using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;

public static class ChunkedRuntimePreviewExportVocabulary
{
    public const string GoalId = "goal_040_chunked_runtime_preview_export_multifamily_smoke";
    public const string FinalGate = "chunked_runtime_preview_export_multifamily_smoke_verification";
    public const string ProductSmokeRoute = "chunked-runtime-preview-export-multifamily-smoke";
    public const string CorePayloadSchemaId = "chunk_traversal_consumer_core_v1";

    public static readonly IReadOnlyList<string> ScenarioIds =
    [
        "frontier_survival",
        "gothic_intrigue",
        "caravan_trade",
        "metamodule_kingdoms"
    ];

    public static readonly IReadOnlyList<string> FamilyLensIds =
    [
        "map_panel_rpg",
        "survival_sandbox",
        "first_person_grid_dungeon"
    ];

    public static readonly IReadOnlyDictionary<string, string> PayloadFileNamesByScenario =
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["frontier_survival"] = "chunked-preview-payload-frontier.json",
            ["gothic_intrigue"] = "chunked-preview-payload-gothic.json",
            ["caravan_trade"] = "chunked-preview-payload-caravan.json",
            ["metamodule_kingdoms"] = "chunked-preview-payload-metamodule.json"
        };
}

public sealed record ChunkedConsumerBoundaryClaims
{
    public bool GamePackageDefinitionsMutation { get; init; }
    public bool RuntimeSourceMutation { get; init; }
    public bool UiWinFormsMutation { get; init; }
    public bool UnitySourceMutation { get; init; }
    public bool ProviderLlmRag { get; init; }
    public bool LuaSourceOrExecution { get; init; }
    public bool GeneratorLibraryMutation { get; init; }
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
        !GamePackageDefinitionsMutation &&
        !RuntimeSourceMutation &&
        !UiWinFormsMutation &&
        !UnitySourceMutation &&
        !ProviderLlmRag &&
        !LuaSourceOrExecution &&
        !GeneratorLibraryMutation &&
        !Filesystem &&
        !Network &&
        !Process &&
        !Reflection &&
        !Thread &&
        !Time &&
        !Random &&
        !NativeInterop;
}

public sealed record ChunkedSourceReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string EvidenceRef { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
    public string ArtifactFileName { get; init; } = string.Empty;
    public string ArtifactHash { get; init; } = string.Empty;
}

public sealed record ChunkedConsumerCatalogSummary
{
    public string SchemaVersion { get; init; } = "chunked_consumer_catalog_summary_v1";
    public string GoalId { get; init; } = ChunkedRuntimePreviewExportVocabulary.GoalId;
    public string FinalGate { get; init; } = ChunkedRuntimePreviewExportVocabulary.FinalGate;
    public string ProductSmokeRoute { get; init; } = ChunkedRuntimePreviewExportVocabulary.ProductSmokeRoute;
    public bool Goal039AcceptedByUserHandoff { get; init; }
    public string Goal039AcceptedGate { get; init; } = "runtime_chunk_delta_traversal_smoke_verification passed";
    public bool Goal040GatePassed { get; init; }
    public string SourceGoal039ArtifactRoot { get; init; } = string.Empty;
    public int ScenarioCount { get; init; }
    public int PayloadCount { get; init; }
    public int FamilyLensCount { get; init; }
    public bool SourceGoal039RuntimeDeltasConsumed { get; init; }
    public bool SourceGoal038StaticMapOnly { get; init; }
    public bool SaveLoadCorrelationConsumed { get; init; }
    public bool ReplayCorrelationConsumed { get; init; }
    public IReadOnlyList<ChunkedScenarioCatalogEntry> Scenarios { get; init; } = [];
    public IReadOnlyList<string> BlockedGaps { get; init; } = [];
    public IReadOnlyList<string> FutureRequiredGaps { get; init; } = [];
    public IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ChunkedScenarioCatalogEntry
{
    public string ScenarioId { get; init; } = string.Empty;
    public string PayloadFileName { get; init; } = string.Empty;
    public string PayloadHash { get; init; } = string.Empty;
    public string SourcePlanFileName { get; init; } = string.Empty;
    public string SourcePlanHash { get; init; } = string.Empty;
    public string SourceDeltaStateFileName { get; init; } = string.Empty;
    public string SourceDeltaStateHash { get; init; } = string.Empty;
    public string SourceSaveLoadProofFileName { get; init; } = string.Empty;
    public string SourceReplayProofFileName { get; init; } = string.Empty;
    public int ChunkCount { get; init; }
    public int RuntimeDeltaMarkerCount { get; init; }
    public int FamilyLensCount { get; init; }
}

public sealed record ChunkedPreviewPayload
{
    public string SchemaVersion { get; init; } = "chunked_preview_export_payload_v1";
    public string GoalId { get; init; } = ChunkedRuntimePreviewExportVocabulary.GoalId;
    public string CorePayloadSchemaId { get; init; } = ChunkedRuntimePreviewExportVocabulary.CorePayloadSchemaId;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string WorldGraphId { get; init; } = string.Empty;
    public string FiniteMapId { get; init; } = string.Empty;
    public string CoordinateKind { get; init; } = string.Empty;
    public string ReplaySeed { get; init; } = string.Empty;
    public ChunkedPreviewSourceEvidence SourceEvidence { get; init; } = new();
    public IReadOnlyList<string> ChunkIds { get; init; } = [];
    public IReadOnlyList<ChunkedTraversalRouteStep> TraversalRoute { get; init; } = [];
    public IReadOnlyList<string> VisitedRegionIds { get; init; } = [];
    public IReadOnlyList<string> DiscoveredChunkIds { get; init; } = [];
    public IReadOnlyList<string> LandmarkDiscoveryIds { get; init; } = [];
    public IReadOnlyList<string> RouteCheckpointMarkerIds { get; init; } = [];
    public IReadOnlyList<ChunkedMutationMarker> MutationMarkers { get; init; } = [];
    public IReadOnlyList<ChunkedRuntimeDeltaMarker> RuntimeDeltaMarkers { get; init; } = [];
    public ChunkedReplaySaveLoadCorrelation ReplaySaveLoadCorrelation { get; init; } = new();
    public IReadOnlyList<ChunkedFamilyLensPayloadView> FamilyLensViews { get; init; } = [];
    public ChunkedPreviewExportReadiness PreviewExportReadiness { get; init; } = new();
    public ChunkedConsumerBoundaryClaims BoundaryClaims { get; init; } = new();
    public bool FinalProseOnly { get; init; }
    public string PayloadHash { get; init; } = string.Empty;
}

public sealed record ChunkedPreviewSourceEvidence
{
    public IReadOnlyList<ChunkedSourceReference> Goal038EvidenceRefs { get; init; } = [];
    public IReadOnlyList<ChunkedSourceReference> Goal039EvidenceRefs { get; init; } = [];
    public string SourcePlanHash { get; init; } = string.Empty;
    public string SourceRuntimeDeltaStateHash { get; init; } = string.Empty;
    public string SourceSaveLoadProofHash { get; init; } = string.Empty;
    public string SourceReplayProofHash { get; init; } = string.Empty;
    public bool ConsumesGoal039RuntimeDeltaCommands { get; init; }
    public bool ConsumesGoal039SaveLoadProof { get; init; }
    public bool ConsumesGoal039ReplayProof { get; init; }
    public bool PayloadIsSourceJsonCopy { get; init; }
}

public sealed record ChunkedTraversalRouteStep
{
    public int StepIndex { get; init; }
    public string RegionId { get; init; } = string.Empty;
    public string ChunkId { get; init; } = string.Empty;
    public string Coordinate { get; init; } = string.Empty;
    public string ArrivedByEdgeId { get; init; } = string.Empty;
    public string LandmarkId { get; init; } = string.Empty;
    public string RouteCheckpointMarkerId { get; init; } = string.Empty;
    public string MutationMarkerId { get; init; } = string.Empty;
}

public sealed record ChunkedMutationMarker
{
    public string MutationId { get; init; } = string.Empty;
    public string MutationKind { get; init; } = string.Empty;
}

public sealed record ChunkedRuntimeDeltaMarker
{
    public int Order { get; init; }
    public string DeltaId { get; init; } = string.Empty;
    public string DeltaKind { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string ChunkId { get; init; } = string.Empty;
    public string MarkerId { get; init; } = string.Empty;
    public string SourceEdgeId { get; init; } = string.Empty;
    public string MutationKey { get; init; } = string.Empty;
    public string MutationValue { get; init; } = string.Empty;
}

public sealed record ChunkedReplaySaveLoadCorrelation
{
    public bool RuntimeStateOwnerIsGameRuntimeState { get; init; }
    public bool SerializerRoundtripPassed { get; init; }
    public bool SnapshotRoundtripPassed { get; init; }
    public bool ReplayDeterminismPassed { get; init; }
    public string RuntimeStateHash { get; init; } = string.Empty;
    public string RestoredRuntimeStateHash { get; init; } = string.Empty;
    public string ReplayMarker { get; init; } = string.Empty;
    public string ReplayProofHash { get; init; } = string.Empty;
    public string SaveLoadProofHash { get; init; } = string.Empty;
}

public sealed record ChunkedFamilyLensPayloadView
{
    public string FamilyLensId { get; init; } = string.Empty;
    public string CorePayloadSchemaId { get; init; } = ChunkedRuntimePreviewExportVocabulary.CorePayloadSchemaId;
    public bool ForksCoreTraversalSchema { get; init; }
    public IReadOnlyList<string> ExpectedConsumerNeeds { get; init; } = [];
    public IReadOnlyList<string> RouteOrientationHints { get; init; } = [];
    public IReadOnlyList<string> ReadinessFlags { get; init; } = [];
}

public sealed record ChunkedPreviewExportReadiness
{
    public bool PreviewPayloadReady { get; init; }
    public bool ExportManifestReady { get; init; }
    public bool RuntimeDeltaBacked { get; init; }
    public bool SaveLoadBacked { get; init; }
    public bool ReplayBacked { get; init; }
    public bool ConcreteRuntimePreviewIntegrationFutureRequired { get; init; }
    public bool ConcreteUnityExportIntegrationFutureRequired { get; init; }
    public IReadOnlyList<string> FutureRequiredGaps { get; init; } = [];
    public IReadOnlyList<string> BlockedGaps { get; init; } = [];
}

public sealed record ChunkedExportManifest
{
    public string SchemaVersion { get; init; } = "chunked_runtime_preview_export_manifest_v1";
    public string GoalId { get; init; } = ChunkedRuntimePreviewExportVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = ChunkedRuntimePreviewExportVocabulary.ProductSmokeRoute;
    public string CorePayloadSchemaId { get; init; } = ChunkedRuntimePreviewExportVocabulary.CorePayloadSchemaId;
    public string ManifestHash { get; init; } = string.Empty;
    public bool UsesGoal039RuntimeDeltas { get; init; }
    public bool RuntimePreviewCompatible { get; init; }
    public bool UnityExportCompatible { get; init; }
    public IReadOnlyList<ChunkedExportManifestEntry> Payloads { get; init; } = [];
    public IReadOnlyList<string> FutureRequiredIntegrationGaps { get; init; } = [];
    public IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ChunkedExportManifestEntry
{
    public string ScenarioId { get; init; } = string.Empty;
    public string PayloadPath { get; init; } = string.Empty;
    public string PayloadHash { get; init; } = string.Empty;
    public int ChunkCount { get; init; }
    public int RuntimeDeltaMarkerCount { get; init; }
    public bool PreviewReady { get; init; }
    public bool ExportReady { get; init; }
}

public sealed record MultiFamilyWorldScaleRegressionMatrix
{
    public string SchemaVersion { get; init; } = "multi_family_world_scale_regression_matrix_v1";
    public string CorePayloadSchemaId { get; init; } = ChunkedRuntimePreviewExportVocabulary.CorePayloadSchemaId;
    public int FamilyLensCount { get; init; }
    public int ScenarioCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<MultiFamilyLensPlan> FamilyLenses { get; init; } = [];
    public IReadOnlyList<MultiFamilyScenarioReuse> ScenarioReuse { get; init; } = [];
}

public sealed record MultiFamilyLensPlan
{
    public string FamilyLensId { get; init; } = string.Empty;
    public string CorePayloadSchemaId { get; init; } = ChunkedRuntimePreviewExportVocabulary.CorePayloadSchemaId;
    public bool ForksCoreTraversalSchema { get; init; }
    public IReadOnlyList<string> ExpectedConsumerNeeds { get; init; } = [];
}

public sealed record MultiFamilyScenarioReuse
{
    public string ScenarioId { get; init; } = string.Empty;
    public IReadOnlyList<string> FamilyLensIds { get; init; } = [];
    public string SharedCorePayloadSchemaId { get; init; } = ChunkedRuntimePreviewExportVocabulary.CorePayloadSchemaId;
    public bool ReusesSameCoreTraversalPayload { get; init; }
}

public sealed record InfiniteChunkedWorldSmokeProof
{
    public string SchemaVersion { get; init; } = "infinite_chunked_world_smoke_proof_v1";
    public string SeedId { get; init; } = string.Empty;
    public InfiniteChunkWindow Window { get; init; } = new();
    public IReadOnlyList<InfiniteDerivedChunk> DerivedChunks { get; init; } = [];
    public IReadOnlyList<string> BoundaryHandoffPlaceholders { get; init; } = [];
    public string RepeatableHash { get; init; } = string.Empty;
    public string ReplayedHash { get; init; } = string.Empty;
    public bool Deterministic { get; init; }
    public bool RealInfiniteStreamingImplemented { get; init; }
    public IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InfiniteChunkWindow
{
    public string OriginChunkId { get; init; } = string.Empty;
    public int OriginX { get; init; }
    public int OriginY { get; init; }
    public int Radius { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
}

public sealed record InfiniteDerivedChunk
{
    public string ChunkId { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public string DerivationKey { get; init; } = string.Empty;
}

public sealed record RuntimePreviewConsumptionProof
{
    public string SchemaVersion { get; init; } = "runtime_preview_consumption_proof_v1";
    public bool Goal039RuntimeDeltasConsumed { get; init; }
    public bool PayloadsAreNotSourceJsonCopies { get; init; }
    public bool PreviewExportManifestReferencesPayloads { get; init; }
    public bool FutureRuntimePreviewRouteCanConsumeManifest { get; init; }
    public bool ExistingPreviewExportSourceTouched { get; init; }
    public int PayloadCount { get; init; }
    public string ExportManifestHash { get; init; } = string.Empty;
    public IReadOnlyList<string> FutureRequiredGaps { get; init; } = [];
    public IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageImmutabilityAudit
{
    public string SchemaVersion { get; init; } = "package_immutability_audit_v1";
    public bool Passed { get; init; }
    public bool GamePackageDefinitionsMutated { get; init; }
    public bool PublicPackageSchemaMutated { get; init; }
    public bool RuntimeStateSourceContractsMutated { get; init; }
    public bool UnityEntrypointsMutated { get; init; }
    public bool WinFormsUiMutated { get; init; }
    public bool ProviderLlmRagTouched { get; init; }
    public bool LuaExecutionTouched { get; init; }
    public bool GeneratorLibraryTouched { get; init; }
    public IReadOnlyList<string> ImmutableFamilies { get; init; } = [];
    public IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record InvalidChunkedConsumerMatrix
{
    public string SchemaVersion { get; init; } = "invalid_chunked_consumer_diagnostics_matrix_v1";
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public int BlockedCount { get; init; }
    public IReadOnlyList<InvalidChunkedConsumerScenario> Scenarios { get; init; } = [];
}

public sealed record InvalidChunkedConsumerScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ChunkedRuntimePreviewExportReport
{
    public string SchemaVersion { get; init; } = "chunked_runtime_preview_export_multifamily_smoke_report_v1";
    public string GoalId { get; init; } = ChunkedRuntimePreviewExportVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } = ChunkedRuntimePreviewExportVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } = ChunkedRuntimePreviewExportVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Goal039AcceptedByUserHandoff { get; init; }
    public string Goal039AcceptedGate { get; init; } = "runtime_chunk_delta_traversal_smoke_verification passed";
    public bool Goal040GatePassed { get; init; }
    public int ScenarioPayloadCount { get; init; }
    public int FamilyLensCount { get; init; }
    public bool SourceGoal039RuntimeDeltasConsumed { get; init; }
    public bool PayloadsAreNotSourceJsonCopies { get; init; }
    public bool ExportManifestStable { get; init; }
    public bool MultiFamilyRegressionPassed { get; init; }
    public bool InfiniteChunkedSmokeProofPassed { get; init; }
    public bool PackageImmutabilityAuditPassed { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public string CatalogHash { get; init; } = string.Empty;
    public string ExportManifestHash { get; init; } = string.Empty;
    public string MultiFamilyMatrixHash { get; init; } = string.Empty;
    public string InfiniteSmokeProofHash { get; init; } = string.Empty;
    public string ConsumptionProofHash { get; init; } = string.Empty;
    public string PackageImmutabilityAuditHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<ChunkedRuntimePreviewExportDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record ChunkedRuntimePreviewExportEvidenceResult
{
    public ChunkedConsumerCatalogSummary CatalogSummary { get; init; } = new();
    public IReadOnlyList<ChunkedPreviewPayload> Payloads { get; init; } = [];
    public ChunkedExportManifest ExportManifest { get; init; } = new();
    public MultiFamilyWorldScaleRegressionMatrix MultiFamilyMatrix { get; init; } = new();
    public InfiniteChunkedWorldSmokeProof InfiniteSmokeProof { get; init; } = new();
    public RuntimePreviewConsumptionProof ConsumptionProof { get; init; } = new();
    public PackageImmutabilityAudit PackageImmutabilityAudit { get; init; } = new();
    public InvalidChunkedConsumerMatrix InvalidMatrix { get; init; } = new();
    public ChunkedRuntimePreviewExportReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record ChunkedRuntimePreviewExportWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record ChunkedRuntimePreviewExportDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static ChunkedRuntimePreviewExportDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };

    public static ChunkedRuntimePreviewExportDiagnostic Warning(string code, string target, string message) =>
        new() { Severity = "warning", Code = code, Target = target, Message = message };

    public static ChunkedRuntimePreviewExportDiagnostic Info(string code, string target, string message) =>
        new() { Severity = "info", Code = code, Target = target, Message = message };
}
