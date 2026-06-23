using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public enum UnityArchiveReviewHistoryReadiness
{
    Ready,
    ReadyWithWarnings,
    NoPreviousSnapshot,
    MissingReview,
    Invalid,
    Blocked
}

public sealed record UnityArchiveReviewHistoryRequest
{
    public string ArchiveDirectoryPath { get; init; } = string.Empty;
}

public sealed record UnityArchiveReviewHistoryResult
{
    public string ArchiveDirectoryPath { get; init; } = string.Empty;
    public UnityArchiveReviewHistoryReport Report { get; init; } = new();
    public IReadOnlyList<string> WrittenRelativePaths { get; init; } = Array.Empty<string>();
}

public sealed record UnityArchiveReviewHistoryReport
{
    public string SchemaVersion { get; init; } = "1";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveReviewHistoryReadiness Readiness { get; init; }

    public string SnapshotId { get; init; } = string.Empty;
    public IReadOnlyList<UnityArchiveReviewHistorySnapshotEntry> HistoryEntries { get; init; }
        = Array.Empty<UnityArchiveReviewHistorySnapshotEntry>();
}

public sealed record UnityArchiveReviewHistorySnapshotEntry
{
    public int Sequence { get; init; }
    public string SnapshotId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
}

public sealed record UnityArchiveReviewHistoryIndex
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<UnityArchiveReviewHistorySnapshotEntry> Entries { get; init; }
        = Array.Empty<UnityArchiveReviewHistorySnapshotEntry>();
}