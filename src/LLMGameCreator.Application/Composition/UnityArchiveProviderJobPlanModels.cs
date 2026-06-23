using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public enum UnityArchiveFulfillmentSlotStatus
{
    missing
}

public enum UnityArchiveProviderJobReadiness
{
    planned_not_executed
}

public enum UnityArchiveProviderPlanReadiness
{
    Ready,
    ReadyWithWarnings,
    BlockedByErrors
}

public sealed record UnityArchiveProviderJobPlanRequest
{
    public string ProjectRootPath { get; init; } = string.Empty;
    public UnityArchiveRequestPipelineResult RequestPipeline { get; init; } = new();
    public UnityGameArchiveManifest ArchiveManifest { get; init; } = new();
    public GameDesignBrief DesignBrief { get; init; } = new();
    public UnityTargetProfile TargetProfile { get; init; } = new();
}

public sealed record UnityArchiveProviderJobPlanDiagnostic
{
    public UnityArchiveExportDiagnosticSeverity Severity { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
}

public sealed record UnityArchiveFulfillmentSlot
{
    public string SlotId { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public UnityArchiveRequestProviderKind ProviderKind { get; init; }
    public string ExpectedOutputRelativePath { get; init; } = string.Empty;
    public bool Required { get; init; }
    public UnityArchiveFulfillmentSlotStatus Status { get; init; } = UnityArchiveFulfillmentSlotStatus.missing;
    public UnityArchiveRequestSourceRef SourceRef { get; init; } = new();
}

public sealed record UnityArchiveAssetSlot
{
    public string SlotId { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public string AssetId { get; init; } = string.Empty;
    public UnityArchiveAssetKind AssetKind { get; init; }
    public UnityArchiveRequestProviderKind ProviderKind { get; init; }
    public string ExpectedOutputRelativePath { get; init; } = string.Empty;
    public bool Required { get; init; }
    public UnityArchiveFulfillmentSlotStatus Status { get; init; } = UnityArchiveFulfillmentSlotStatus.missing;
    public UnityArchiveRequestSourceRef SourceRef { get; init; } = new();
}

public sealed record UnityArchiveAudioSlot
{
    public string SlotId { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public string AudioId { get; init; } = string.Empty;
    public UnityArchiveAudioKind AudioKind { get; init; }
    public UnityArchiveRequestProviderKind ProviderKind { get; init; }
    public string ExpectedOutputRelativePath { get; init; } = string.Empty;
    public bool Required { get; init; }
    public UnityArchiveFulfillmentSlotStatus Status { get; init; } = UnityArchiveFulfillmentSlotStatus.missing;
    public UnityArchiveRequestSourceRef SourceRef { get; init; } = new();
}

public sealed record UnityArchiveLuaModuleSlot
{
    public string SlotId { get; init; } = string.Empty;
    public string ModuleId { get; init; } = string.Empty;
    public UnityArchiveLuaModuleKind ModuleKind { get; init; }
    public UnityArchiveRequestProviderKind ProviderKind { get; init; }
    public string ExpectedOutputRelativePath { get; init; } = string.Empty;
    public bool Required { get; init; }
    public UnityArchiveFulfillmentSlotStatus Status { get; init; } = UnityArchiveFulfillmentSlotStatus.missing;
    public UnityArchiveRequestSourceRef SourceRef { get; init; } = new();
}

public sealed record UnityArchiveProviderJob
{
    public string JobId { get; init; } = string.Empty;
    public UnityArchiveRequestProviderKind ProviderKind { get; init; }
    public string RequestId { get; init; } = string.Empty;
    public string SlotId { get; init; } = string.Empty;
    public string ExpectedOutputRelativePath { get; init; } = string.Empty;
    public string PromptOrInstruction { get; init; } = string.Empty;
    public UnityArchiveRequestSourceRef SourceRef { get; init; } = new();
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
    public UnityArchiveProviderJobReadiness Readiness { get; init; } = UnityArchiveProviderJobReadiness.planned_not_executed;
    public bool ExecutionEnabled { get; init; }
}

public sealed record UnityArchiveProviderJobBatch
{
    public string SchemaVersion { get; init; } = "1";
    public UnityArchiveRequestProviderKind ProviderKind { get; init; }
    public bool ExecutionEnabled { get; init; }
    public IReadOnlyList<UnityArchiveProviderJob> Jobs { get; init; } = Array.Empty<UnityArchiveProviderJob>();
}

public sealed record UnityArchiveProviderJobIndex
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<UnityArchiveProviderJobBatch> Batches { get; init; } = Array.Empty<UnityArchiveProviderJobBatch>();
}

public sealed record UnityArchiveFulfillmentPlan
{
    public string SchemaVersion { get; init; } = "1";
    public string GameId { get; init; } = string.Empty;
    public string DesignBriefId { get; init; } = string.Empty;
    public string TargetProfileId { get; init; } = string.Empty;
    public IReadOnlyList<UnityArchiveFulfillmentSlot> Slots { get; init; } = Array.Empty<UnityArchiveFulfillmentSlot>();
}

public sealed record UnityArchiveAssetSlotIndex
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<UnityArchiveAssetSlot> Slots { get; init; } = Array.Empty<UnityArchiveAssetSlot>();
}

public sealed record UnityArchiveAudioSlotIndex
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<UnityArchiveAudioSlot> Slots { get; init; } = Array.Empty<UnityArchiveAudioSlot>();
}

public sealed record UnityArchiveLuaModuleSlotIndex
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<UnityArchiveLuaModuleSlot> Slots { get; init; } = Array.Empty<UnityArchiveLuaModuleSlot>();
}

public sealed record UnityArchiveProviderReadinessReport
{
    public string SchemaVersion { get; init; } = "1";
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveProviderPlanReadiness Readiness { get; init; }
    public int AssetSlotCount { get; init; }
    public int AudioSlotCount { get; init; }
    public int LuaModuleSlotCount { get; init; }
    public int ProviderJobCount { get; init; }
    public IReadOnlyList<UnityArchiveProviderJobReadinessEntry> Providers { get; init; }
        = Array.Empty<UnityArchiveProviderJobReadinessEntry>();
    public IReadOnlyList<UnityArchiveProviderJobPlanDiagnostic> Diagnostics { get; init; }
        = Array.Empty<UnityArchiveProviderJobPlanDiagnostic>();
}

public sealed record UnityArchiveProviderJobReadinessEntry
{
    public UnityArchiveRequestProviderKind ProviderKind { get; init; }
    public int JobCount { get; init; }
    public UnityArchiveProviderJobReadiness Readiness { get; init; } = UnityArchiveProviderJobReadiness.planned_not_executed;
    public bool ExecutionEnabled { get; init; }
}

public sealed record UnityArchiveProviderJobPlanResult
{
    public UnityArchiveFulfillmentPlan FulfillmentPlan { get; init; } = new();
    public UnityArchiveAssetSlotIndex AssetSlots { get; init; } = new();
    public UnityArchiveAudioSlotIndex AudioSlots { get; init; } = new();
    public UnityArchiveLuaModuleSlotIndex LuaModuleSlots { get; init; } = new();
    public UnityArchiveProviderJobIndex ProviderJobs { get; init; } = new();
    public UnityArchiveProviderReadinessReport ReadinessReport { get; init; } = new();
    public IReadOnlyList<UnityArchiveProviderJobPlanDiagnostic> Diagnostics { get; init; }
        = Array.Empty<UnityArchiveProviderJobPlanDiagnostic>();
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveProviderPlanReadiness Readiness { get; init; }
}
