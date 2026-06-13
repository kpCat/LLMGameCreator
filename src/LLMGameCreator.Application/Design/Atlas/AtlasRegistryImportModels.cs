using System.Collections.ObjectModel;

namespace LLMGameCreator.Application.Design.Atlas;

public sealed record AtlasRegistryImportResult
{
    public bool Ok { get; init; }
    public string AtlasRoot { get; init; } = string.Empty;
    public IReadOnlyList<AtlasDocumentSummary> Documents { get; init; } = Array.Empty<AtlasDocumentSummary>();
    public IReadOnlyList<AtlasExampleSummary> Examples { get; init; } = Array.Empty<AtlasExampleSummary>();
    public IReadOnlyList<AtlasDiagnostic> Diagnostics { get; init; } = Array.Empty<AtlasDiagnostic>();
    public AtlasRegistrySummary Summary { get; init; } = new();
}

public sealed record AtlasDocumentSummary
{
    public string Path { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string? SchemaVersion { get; init; }
    public string? Id { get; init; }
    public string? Title { get; init; }
    public string? Purpose { get; init; }
    public IReadOnlyList<string> TopLevelIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ReferencedIds { get; init; } = Array.Empty<string>();
    public bool Loaded { get; init; }
}

public sealed record AtlasExampleSummary
{
    public string Path { get; init; } = string.Empty;
    public string? ExampleId { get; init; }
    public string? Title { get; init; }
    public string? SourceProfileId { get; init; }
    public IReadOnlyList<string> SelectedFeatureBundles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> TargetArtifacts { get; init; } = Array.Empty<string>();
    public int StepCount { get; init; }
}

public sealed record AtlasDiagnostic
{
    public string Severity { get; init; } = AtlasDiagnosticSeverity.Info;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Path { get; init; }
    public string? Id { get; init; }
}

public sealed record AtlasRegistrySummary
{
    public int DocumentCount { get; init; }
    public int LoadedDocumentCount { get; init; }
    public int ExampleCount { get; init; }
    public int UniqueIdCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
}
