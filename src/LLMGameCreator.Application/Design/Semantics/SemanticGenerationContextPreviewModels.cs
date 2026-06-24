namespace LLMGameCreator.Application.Design.Semantics;

public sealed record SemanticGenerationContextPreview
{
    public string SchemaVersion { get; init; } = "1";
    public string ContextId { get; init; } = "semantic-generation-context-preview";
    public SemanticGenerationLlmPolicy LlmPolicy { get; init; } = new();
    public IReadOnlyList<SemanticGenerationContextSection> Sections { get; init; } = Array.Empty<SemanticGenerationContextSection>();
    public IReadOnlyList<string> CandidateTerms { get; init; } = Array.Empty<string>();
    public IReadOnlyList<SemanticCatalogDiagnostic> Diagnostics { get; init; } = Array.Empty<SemanticCatalogDiagnostic>();
}

public sealed record SemanticGenerationLlmPolicy
{
    public IReadOnlyList<string> LlmRequiredFor { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> DeterministicSteps { get; init; } = Array.Empty<string>();
    public int MaxRecommendedPromptTerms { get; init; } = 80;
}

public sealed record SemanticGenerationContextSection
{
    public string SectionId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<string> TermIds { get; init; } = Array.Empty<string>();
}

public sealed record SemanticGenerationContextPreviewWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string PreviewJsonPath { get; init; } = string.Empty;
    public string PreviewMarkdownPath { get; init; } = string.Empty;
}
