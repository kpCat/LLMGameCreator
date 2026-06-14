namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanExampleTemplateSummary
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public IReadOnlyList<string> TargetArtifacts { get; init; } = Array.Empty<string>();
}

public sealed record GeneratorPlanExampleTemplate
{
    public GeneratorPlanExampleTemplateSummary Summary { get; init; } = new();
    public string FileName { get; init; } = string.Empty;
    public string Json { get; init; } = string.Empty;
}

public sealed record GeneratorPlanExampleTemplateMaterializeRequest
{
    public string TemplateId { get; init; } = string.Empty;
    public string TargetDirectory { get; init; } = string.Empty;
    public bool Overwrite { get; init; }
}

public sealed record GeneratorPlanExampleTemplateMaterializeResult
{
    public bool Ok { get; init; }
    public string TemplateId { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
