namespace LLMGameCreator.Application.Design.Atlas;

public sealed record AtlasRegistryPreviewRequest
{
    public string RepositoryRootOrAtlasRoot { get; init; } = string.Empty;
    public bool RenderMarkdown { get; init; } = true;
    public bool WriteReportFiles { get; init; }
    public string? ReportOutputRoot { get; init; }
}

public sealed record AtlasRegistryPreviewResult
{
    public bool Ok { get; init; }
    public DateTimeOffset GeneratedAtUtc { get; init; }
    public AtlasRegistryImportResult ImportResult { get; init; } = new();
    public string MarkdownReport { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = Array.Empty<string>();
}
