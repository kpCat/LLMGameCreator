using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public enum UnityArchiveManualProviderImportReadiness
{
    Ready,
    ReadyWithWarnings,
    BlockedByErrors,
    MissingManifest,
    InvalidManifest
}

public enum UnityArchiveManualProviderImportEntryStatus
{
    Imported,
    AlreadyImported,
    Conflict,
    Invalid,
    Failed
}

public sealed record UnityArchiveManualProviderImportRequest
{
    public string ArchiveDirectoryPath { get; init; } = string.Empty;
    public string ImportDirectoryRelativePath { get; init; } = "manual-import";
    public string ManifestRelativePath { get; init; } = "manual-import/import-manifest.json";
    public bool OverwriteExisting { get; init; }
    public bool RefreshFulfillmentState { get; init; } = true;
    public bool RefreshReviewHistoryComparison { get; init; } = true;
}

public sealed record UnityArchiveManualProviderImportManifest
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<UnityArchiveManualProviderImportManifestEntry> Entries { get; init; }
        = Array.Empty<UnityArchiveManualProviderImportManifestEntry>();
}

public sealed record UnityArchiveManualProviderImportManifestEntry
{
    public string SlotId { get; init; } = string.Empty;
    public string SourceRelativePath { get; init; } = string.Empty;
    public string? ExpectedOutputRelativePath { get; init; }
}

public sealed record UnityArchiveManualProviderImportDiagnostic
{
    public UnityArchiveExportDiagnosticSeverity Severity { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string SlotId { get; init; } = string.Empty;
}

public sealed record UnityArchiveManualProviderImportEntryResult
{
    public string SlotId { get; init; } = string.Empty;
    public UnityArchiveRequestProviderKind ProviderKind { get; init; }
    public string SourceRelativePath { get; init; } = string.Empty;
    public string ExpectedOutputRelativePath { get; init; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveManualProviderImportEntryStatus Status { get; init; }

    public long FileSizeBytes { get; init; }
    public string ContentSha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> DiagnosticCodes { get; init; } = Array.Empty<string>();
}

public sealed record UnityArchiveManualProviderImportResult
{
    public string SchemaVersion { get; init; } = "1";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UnityArchiveManualProviderImportReadiness Readiness { get; init; }

    public int ImportedCount { get; init; }
    public int SkippedCount { get; init; }
    public int ConflictCount { get; init; }
    public int InvalidCount { get; init; }
    public bool TargetOutputsChanged { get; init; }
    public IReadOnlyList<UnityArchiveManualProviderImportEntryResult> Entries { get; init; }
        = Array.Empty<UnityArchiveManualProviderImportEntryResult>();
    public IReadOnlyList<UnityArchiveManualProviderImportDiagnostic> Diagnostics { get; init; }
        = Array.Empty<UnityArchiveManualProviderImportDiagnostic>();
    public IReadOnlyList<string> WrittenRelativePaths { get; init; } = Array.Empty<string>();
}
