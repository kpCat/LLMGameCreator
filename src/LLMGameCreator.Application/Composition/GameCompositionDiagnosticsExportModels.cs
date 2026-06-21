using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public sealed record GameCompositionDiagnosticsExportRequest
{
    public string ProjectRootPath { get; init; } = string.Empty;
    public GameCompositionDiagnosticsReport Report { get; init; } = new();
}

public sealed record GameCompositionDiagnosticsExportResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string MarkdownPath { get; init; } = string.Empty;
    public string IndexPath { get; init; } = string.Empty;
    public GameCompositionDiagnosticsExportIndexEntry IndexEntry { get; init; } = new();
}

public sealed record GameCompositionDiagnosticsExportIndex
{
    public string SchemaVersion { get; init; } = "1";
    public IReadOnlyList<GameCompositionDiagnosticsExportIndexEntry> Entries { get; init; }
        = Array.Empty<GameCompositionDiagnosticsExportIndexEntry>();
}

public sealed record GameCompositionDiagnosticsExportIndexEntry
{
    public string BlueprintId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public GameCompositionReadiness Readiness { get; init; }
    public string ContentLanguage { get; init; } = string.Empty;
    public string ReportFileName { get; init; } = string.Empty;
}
