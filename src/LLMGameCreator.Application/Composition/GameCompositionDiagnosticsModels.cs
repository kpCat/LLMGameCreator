namespace LLMGameCreator.Application.Composition;

public enum GameCompositionReadiness
{
    BuildableNow,
    BuildableWithWarnings,
    PlannedFuture,
    MissingRequirements,
    Conflict,
    Invalid
}

public enum GameCompositionDiagnosticSeverity
{
    Info,
    Warning,
    Error
}

public sealed record GameCompositionDiagnosticItem
{
    public string Source { get; init; } = string.Empty;
    public GameCompositionDiagnosticSeverity Severity { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string SubjectId { get; init; } = string.Empty;
    public string RelatedId { get; init; } = string.Empty;
}

public sealed record GameCompositionRecommendedAction
{
    public string Code { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed record GameCompositionDiagnosticsReport
{
    public string BlueprintId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public GameKind GameKind { get; init; }
    public string ContentLanguage { get; init; } = string.Empty;
    public GameCompositionReadiness Readiness { get; init; }
    public IReadOnlyList<string> RequestedCapabilityIds { get; init; } = Array.Empty<string>();
    public CompositionValidationResult CapabilityValidationResult { get; init; } = new();
    public GeneratorCatalogValidationResult GeneratorCatalogValidationResult { get; init; } = new();
    public GeneratorPlanningResult GeneratorPlanningResult { get; init; } = new();
    public IReadOnlyList<string> SelectedCurrentGeneratorIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RelatedPlannedGeneratorIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingGeneratorCapabilityIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<GameCompositionDiagnosticItem> Diagnostics { get; init; } = Array.Empty<GameCompositionDiagnosticItem>();
    public IReadOnlyList<GameCompositionRecommendedAction> RecommendedActions { get; init; } = Array.Empty<GameCompositionRecommendedAction>();
}
