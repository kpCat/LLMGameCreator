using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public enum UnityArchiveFulfillmentStatus
{
    missing,
    available,
    invalid
}

public sealed record UnityArchiveFulfillmentStateRequest
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public UnityArchiveProviderJobPlanResult ProviderJobPlan { get; init; } = new();
}

public sealed record UnityArchiveFulfillmentStateEntry
{
    public string SlotId { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public UnityArchiveRequestProviderKind ProviderKind { get; init; }
    public string ExpectedOutputRelativePath { get; init; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveFulfillmentStatus Status { get; init; }
    public long FileSizeBytes { get; init; }
    public DateTimeOffset LastWriteTimeUtc { get; init; }
}

public sealed record UnityArchiveFulfilledAssetEntry
{
    public string SlotId { get; init; } = string.Empty;
    public string AssetId { get; init; } = string.Empty;
    public UnityArchiveAssetKind AssetKind { get; init; }
    public string ExpectedOutputRelativePath { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public DateTimeOffset LastWriteTimeUtc { get; init; }
}

public sealed record UnityArchiveFulfilledAudioEntry
{
    public string SlotId { get; init; } = string.Empty;
    public string AudioId { get; init; } = string.Empty;
    public UnityArchiveAudioKind AudioKind { get; init; }
    public string ExpectedOutputRelativePath { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public DateTimeOffset LastWriteTimeUtc { get; init; }
}

public sealed record UnityArchiveFulfilledLuaEntry
{
    public string SlotId { get; init; } = string.Empty;
    public string ModuleId { get; init; } = string.Empty;
    public UnityArchiveLuaModuleKind ModuleKind { get; init; }
    public string ExpectedOutputRelativePath { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public DateTimeOffset LastWriteTimeUtc { get; init; }
}

public sealed record UnityArchiveInvalidOutputEntry
{
    public string SlotId { get; init; } = string.Empty;
    public string ExpectedOutputRelativePath { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public sealed record UnityArchiveFulfillmentStateDiagnostic
{
    public UnityArchiveExportDiagnosticSeverity Severity { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
}

public sealed record UnityArchiveFulfillmentStateResult
{
    public UnityArchiveFulfillmentStateReport FulfillmentState { get; init; } = new();
    public UnityArchiveFulfilledAssetsIndex FulfilledAssets { get; init; } = new();
    public UnityArchiveFulfilledAudioIndex FulfilledAudio { get; init; } = new();
    public UnityArchiveFulfilledLuaIndex FulfilledLua { get; init; } = new();
    public UnityArchiveInvalidOutputsReport InvalidOutputs { get; init; } = new();
    public IReadOnlyList<UnityArchiveFulfillmentStateDiagnostic> Diagnostics { get; init; }
        = Array.Empty<UnityArchiveFulfillmentStateDiagnostic>();
}

public sealed record UnityArchiveFulfillmentStateReport
{
    public string SchemaVersion { get; init; } = "1";
    public int TotalSlotCount { get; init; }
    public int MissingCount { get; init; }
    public int AvailableCount { get; init; }
    public int InvalidCount { get; init; }
    public IReadOnlyList<UnityArchiveFulfillmentStateEntry> Entries { get; init; }
        = Array.Empty<UnityArchiveFulfillmentStateEntry>();
}

public sealed record UnityArchiveFulfilledAssetsIndex
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<UnityArchiveFulfilledAssetEntry> Assets { get; init; }
        = Array.Empty<UnityArchiveFulfilledAssetEntry>();
}

public sealed record UnityArchiveFulfilledAudioIndex
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<UnityArchiveFulfilledAudioEntry> Audio { get; init; }
        = Array.Empty<UnityArchiveFulfilledAudioEntry>();
}

public sealed record UnityArchiveFulfilledLuaIndex
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<UnityArchiveFulfilledLuaEntry> Lua { get; init; }
        = Array.Empty<UnityArchiveFulfilledLuaEntry>();
}

public sealed record UnityArchiveInvalidOutputsReport
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<UnityArchiveInvalidOutputEntry> InvalidOutputs { get; init; }
        = Array.Empty<UnityArchiveInvalidOutputEntry>();
}