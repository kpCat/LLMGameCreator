namespace LLMGameCreator.Application.Composition;

public enum CapabilityRuntimeCost
{
    None,
    Low,
    Medium,
    High
}

public enum CapabilityMaturity
{
    Current,
    Planned,
    Unsupported
}

public enum CompositionCompatibilityStatus
{
    Compatible,
    CompatibleWithAdapter,
    DegradedButUsable,
    Conflict,
    UnsupportedYet,
    MissingRequirement
}

public enum CompositionDiagnosticSeverity
{
    Info,
    Warning,
    Error
}

public sealed record CapabilityDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public IReadOnlyList<string> Requires { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> OptionalRequires { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Provides { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Conflicts { get; init; } = Array.Empty<string>();
    public IReadOnlyList<WorldSourceKind> SupportedWorldSources { get; init; } = Array.Empty<WorldSourceKind>();
    public IReadOnlyList<PresentationKind> SupportedPresentations { get; init; } = Array.Empty<PresentationKind>();
    public IReadOnlyList<GenerationMode> GenerationModes { get; init; } = Array.Empty<GenerationMode>();
    public CapabilityRuntimeCost RuntimeCost { get; init; }
    public CapabilityMaturity Maturity { get; init; } = CapabilityMaturity.Current;
}

public sealed record CompositionDiagnostic
{
    public CompositionDiagnosticSeverity Severity { get; init; }
    public CompositionCompatibilityStatus Status { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string CapabilityId { get; init; } = string.Empty;
    public string RelatedCapabilityId { get; init; } = string.Empty;
}

public sealed record CompositionValidationResult
{
    public bool Ok { get; init; }
    public CompositionCompatibilityStatus Status { get; init; }
    public IReadOnlyList<CompositionDiagnostic> Diagnostics { get; init; } = Array.Empty<CompositionDiagnostic>();

    public IReadOnlyList<CompositionDiagnostic> Errors => Diagnostics
        .Where(diagnostic => diagnostic.Severity == CompositionDiagnosticSeverity.Error)
        .ToList();

    public IReadOnlyList<CompositionDiagnostic> Warnings => Diagnostics
        .Where(diagnostic => diagnostic.Severity == CompositionDiagnosticSeverity.Warning)
        .ToList();

    public IReadOnlyList<string> MissingRequirements => Diagnostics
        .Where(diagnostic => diagnostic.Code == CompositionDiagnosticCodes.MissingRequirement)
        .Select(diagnostic => diagnostic.RelatedCapabilityId)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
        .ToList();
}

public static class CompositionDiagnosticCodes
{
    public const string DuplicateRegistryId = "composition.registry.duplicate_id";
    public const string UnknownCapability = "composition.capability.unknown";
    public const string MissingRequirement = "composition.capability.missing_requirement";
    public const string OptionalRequirementMissing = "composition.capability.optional_requirement_missing";
    public const string DirectConflict = "composition.capability.direct_conflict";
    public const string UnsupportedYet = "composition.capability.unsupported_yet";
    public const string UnsupportedWorldSource = "composition.capability.world_source_unsupported";
    public const string UnsupportedPresentation = "composition.capability.presentation_unsupported";
    public const string UnsupportedGenerationMode = "composition.capability.generation_mode_unsupported";
}
