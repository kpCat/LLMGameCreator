namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanPreviewRequest
{
    public string SourcePath { get; init; } = string.Empty;
    public bool RenderMarkdown { get; init; } = true;
}

public sealed record GeneratorPlanPreviewResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = GeneratorPlanPreviewValidationState.Valid;
    public DateTimeOffset GeneratedAtUtc { get; init; }
    public GeneratorPlanPreview Preview { get; init; } = new();
    public string MarkdownReport { get; init; } = string.Empty;
    public IReadOnlyList<GeneratorPlanPreviewDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanPreviewDiagnostic>();
}

public sealed record GeneratorPlanPreview
{
    public string SourcePath { get; init; } = string.Empty;
    public string ExampleId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public string SourceProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedFeatureBundles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> TargetArtifacts { get; init; } = Array.Empty<string>();
    public IReadOnlyList<GeneratorPlanPreviewStep> Steps { get; init; } = Array.Empty<GeneratorPlanPreviewStep>();
    public IReadOnlyList<GeneratorPlanPreviewDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanPreviewDiagnostic>();
    public GeneratorPlanPreviewSummary Summary { get; init; } = new();
}

public sealed record GeneratorPlanPreviewStep
{
    public string Id { get; init; } = string.Empty;
    public int Order { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ProducerRole { get; init; } = string.Empty;
    public string ContextPackTemplate { get; init; } = string.Empty;
    public string ExpectedArtifactContract { get; init; } = string.Empty;
    public IReadOnlyList<string> Inputs { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ValidationGates { get; init; } = Array.Empty<string>();
    public string OnSuccess { get; init; } = string.Empty;
    public string OnFailure { get; init; } = string.Empty;
}

public sealed record GeneratorPlanPreviewSummary
{
    public int StepCount { get; init; }
    public int TargetArtifactCount { get; init; }
    public int FeatureBundleCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
}

public sealed record GeneratorPlanPreviewDiagnostic
{
    public string Severity { get; init; } = GeneratorPlanPreviewDiagnosticSeverity.Info;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Path { get; init; }
    public string? StepId { get; init; }
}
