using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public enum UnityArchiveReviewSnapshotReadiness
{
    Ready,
    ReadyWithWarnings,
    Blocked,
    Invalid,
    MissingArchive
}

public sealed record UnityArchiveReviewSnapshotRequest
{
    public string ArchiveDirectoryPath { get; init; } = string.Empty;
    public bool WriteReviewFiles { get; init; } = true;
}

public sealed record UnityArchiveReviewSnapshotResult
{
    public string ArchiveDirectoryPath { get; init; } = string.Empty;
    public UnityArchiveReviewSnapshotReport Report { get; init; } = new();
    public IReadOnlyList<string> WrittenRelativePaths { get; init; } = Array.Empty<string>();
}

public sealed record UnityArchiveReviewSnapshotReport
{
    public string SchemaVersion { get; init; } = "1";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveReviewSnapshotReadiness Readiness { get; init; }

    public UnityArchiveReviewSnapshotValidationSummary Validation { get; init; } = new();
    public UnityArchiveReviewSnapshotProviderSummary Providers { get; init; } = new();
    public UnityArchiveReviewSnapshotFulfillmentSummary Fulfillment { get; init; } = new();
    public UnityArchiveReviewSnapshotRequestSummary Requests { get; init; } = new();

    public int SourceFileCount { get; init; }
    public int DiagnosticCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
    public int InfoCount { get; init; }

    public IReadOnlyList<UnityArchiveReviewSnapshotDiagnostic> Diagnostics { get; init; }
        = Array.Empty<UnityArchiveReviewSnapshotDiagnostic>();

    public IReadOnlyList<UnityArchiveReviewSnapshotFileReference> SourceFiles { get; init; }
        = Array.Empty<UnityArchiveReviewSnapshotFileReference>();
}

public sealed record UnityArchiveReviewSnapshotValidationSummary
{
    public bool ExportValidationPresent { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveMaterializationReadiness MaterializationReadiness { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveExportReadiness DryRunReadiness { get; init; }

    public int MaterializedFileCount { get; init; }
}

public sealed record UnityArchiveReviewSnapshotProviderSummary
{
    public bool ReadinessReportPresent { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveProviderPlanReadiness Readiness { get; init; }

    public int AssetSlotCount { get; init; }
    public int AudioSlotCount { get; init; }
    public int LuaModuleSlotCount { get; init; }
    public int ProviderJobCount { get; init; }

    public IReadOnlyList<UnityArchiveReviewProviderBatchSummary> Batches { get; init; }
        = Array.Empty<UnityArchiveReviewProviderBatchSummary>();
}

public sealed record UnityArchiveReviewProviderBatchSummary
{
    public UnityArchiveRequestProviderKind ProviderKind { get; init; }
    public int JobCount { get; init; }
    public bool ExecutionEnabled { get; init; }
}

public sealed record UnityArchiveReviewSnapshotFulfillmentSummary
{
    public bool FulfillmentStatePresent { get; init; }
    public bool InvalidOutputsPresent { get; init; }

    public int TotalSlotCount { get; init; }
    public int MissingCount { get; init; }
    public int AvailableCount { get; init; }
    public int InvalidCount { get; init; }
    public int InvalidOutputCount { get; init; }

    public IReadOnlyList<UnityArchiveReviewInvalidOutputReasonSummary> InvalidReasons { get; init; }
        = Array.Empty<UnityArchiveReviewInvalidOutputReasonSummary>();
}

public sealed record UnityArchiveReviewInvalidOutputReasonSummary
{
    public string Reason { get; init; } = string.Empty;
    public int Count { get; init; }
}

public sealed record UnityArchiveReviewSnapshotRequestSummary
{
    public bool AssetRequestsPresent { get; init; }
    public bool AudioRequestsPresent { get; init; }
    public bool LuaModuleRequestsPresent { get; init; }

    public int AssetRequestCount { get; init; }
    public int AudioRequestCount { get; init; }
    public int LuaModuleRequestCount { get; init; }
}

public sealed record UnityArchiveReviewSnapshotDiagnostic
{
    public UnityArchiveExportDiagnosticSeverity Severity { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SourceFile { get; init; } = string.Empty;
}

public sealed record UnityArchiveReviewSnapshotFileReference
{
    public string RelativePath { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
}
