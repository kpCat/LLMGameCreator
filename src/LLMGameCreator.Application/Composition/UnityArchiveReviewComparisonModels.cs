using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public enum UnityArchiveReviewComparisonReadiness
{
    Ready,
    ReadyWithWarnings,
    NoPreviousSnapshot,
    MissingReview,
    Invalid,
    Blocked
}

public sealed record UnityArchiveReviewComparisonRequest
{
    public string ArchiveDirectoryPath { get; init; } = string.Empty;
}

public sealed record UnityArchiveReviewComparisonResult
{
    public string ArchiveDirectoryPath { get; init; } = string.Empty;
    public UnityArchiveReviewComparisonReport Report { get; init; } = new();
    public IReadOnlyList<string> WrittenRelativePaths { get; init; } = Array.Empty<string>();
}

public sealed record UnityArchiveReviewComparisonReport
{
    public string SchemaVersion { get; init; } = "1";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveReviewComparisonReadiness Readiness { get; init; }

    public string CurrentSnapshotId { get; init; } = string.Empty;
    public string PreviousSnapshotId { get; init; } = string.Empty;

    public UnityArchiveReviewComparisonSummary Summary { get; init; } = new();

    public IReadOnlyList<UnityArchiveReviewComparisonDelta> Deltas { get; init; }
        = Array.Empty<UnityArchiveReviewComparisonDelta>();

    public IReadOnlyList<UnityArchiveReviewComparisonDiagnosticChange> DiagnosticChanges { get; init; }
        = Array.Empty<UnityArchiveReviewComparisonDiagnosticChange>();

    public IReadOnlyList<UnityArchiveReviewComparisonSourceFileChange> SourceFileChanges { get; init; }
        = Array.Empty<UnityArchiveReviewComparisonSourceFileChange>();

    public IReadOnlyList<UnityArchiveReviewComparisonInvalidReasonChange> InvalidReasonChanges { get; init; }
        = Array.Empty<UnityArchiveReviewComparisonInvalidReasonChange>();
}

public sealed record UnityArchiveReviewComparisonSummary
{
    public int SourceFileCountDelta { get; init; }
    public int DiagnosticCountDelta { get; init; }
    public int ErrorCountDelta { get; init; }
    public int WarningCountDelta { get; init; }
    public int InfoCountDelta { get; init; }
    public int InvalidOutputCountDelta { get; init; }
    public int AssetSlotCount { get; init; }
    public int AudioSlotCount { get; init; }
    public int LuaModuleSlotCount { get; init; }
    public int ProviderJobCount { get; init; }
    public int AssetRequestCount { get; init; }
    public int AudioRequestCount { get; init; }
    public int LuaModuleRequestCount { get; init; }
}

public sealed record UnityArchiveReviewComparisonDelta
{
    public string Dimension { get; init; } = string.Empty;
    public string Previous { get; init; } = string.Empty;
    public string Current { get; init; } = string.Empty;
}

public sealed record UnityArchiveReviewComparisonDiagnosticChange
{
    public UnityArchiveExportDiagnosticSeverity Severity { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SourceFile { get; init; } = string.Empty;
    public string Change { get; init; } = string.Empty;
}

public sealed record UnityArchiveReviewComparisonSourceFileChange
{
    public string RelativePath { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Change { get; init; } = string.Empty;
}

public sealed record UnityArchiveReviewComparisonInvalidReasonChange
{
    public string Reason { get; init; } = string.Empty;
    public int PreviousCount { get; init; }
    public int CurrentCount { get; init; }
}