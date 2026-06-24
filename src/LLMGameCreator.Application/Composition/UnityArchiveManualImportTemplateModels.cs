namespace LLMGameCreator.Application.Composition;

public enum UnityArchiveManualImportSlotKind
{
    Asset,
    Audio,
    Lua,
    Unknown
}

public enum UnityArchiveManualImportWorkspaceReadiness
{
    Ready,
    ReadyWithWarnings,
    MissingArchive,
    MissingSlotMetadata,
    InvalidSlotMetadata
}

public sealed record UnityArchiveManualImportWorkspaceSlot
{
    public string SlotId { get; init; } = string.Empty;
    public UnityArchiveManualImportSlotKind Kind { get; init; }
    public UnityArchiveRequestProviderKind ProviderKind { get; init; }
    public string ExpectedOutputRelativePath { get; init; } = string.Empty;
    public UnityArchiveFulfillmentStatus Status { get; init; }
    public bool FileExists { get; init; }
    public long FileSizeBytes { get; init; }
    public string ContentSha256 { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
    public string SuggestedSourceRelativePath { get; init; } = string.Empty;
}

public sealed record UnityArchiveManualImportWorkspaceResult
{
    public UnityArchiveManualImportWorkspaceReadiness Readiness { get; init; }
    public IReadOnlyList<UnityArchiveManualImportWorkspaceSlot> Slots { get; init; }
        = Array.Empty<UnityArchiveManualImportWorkspaceSlot>();
    public IReadOnlyList<string> Diagnostics { get; init; } = Array.Empty<string>();
}

public sealed record UnityArchiveManualImportTemplateResult
{
    public bool Succeeded { get; init; }
    public string TemplateRelativePath { get; init; } = string.Empty;
    public string TemplateFullPath { get; init; } = string.Empty;
    public int EntryCount { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed record UnityArchiveManualImportDirectoryResult
{
    public bool Succeeded { get; init; }
    public string DirectoryPath { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
