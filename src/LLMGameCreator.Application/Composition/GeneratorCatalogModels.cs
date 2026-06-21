namespace LLMGameCreator.Application.Composition;

public enum GeneratorMaturity
{
    Current,
    Preview,
    Planned,
    UnsupportedYet,
    Deprecated
}

public enum GeneratorRuntimeCost
{
    None,
    Low,
    Medium,
    High
}

public enum GeneratorDiagnosticSeverity
{
    Info,
    Warning,
    Error
}

public sealed record GeneratorModuleManifest
{
    public string GeneratorId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public GeneratorMaturity Maturity { get; init; } = GeneratorMaturity.Current;
    public bool UsesLlm { get; init; }
    public bool Deterministic { get; init; }
    public bool CanRunOffline { get; init; }
    public bool CanRunAtRuntime { get; init; }
    public IReadOnlyList<string> InputContracts { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> OutputContracts { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiresCapabilities { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ProvidesCapabilities { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> OptionalCapabilities { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ConflictsWithGenerators { get; init; } = Array.Empty<string>();
    public IReadOnlyList<GameKind> SupportedGameKinds { get; init; } = Array.Empty<GameKind>();
    public IReadOnlyList<WorldSourceKind> SupportedWorldSources { get; init; } = Array.Empty<WorldSourceKind>();
    public IReadOnlyList<PresentationKind> SupportedPresentations { get; init; } = Array.Empty<PresentationKind>();
    public IReadOnlyList<GenerationMode> SupportedGenerationModes { get; init; } = Array.Empty<GenerationMode>();
    public GeneratorRuntimeCost RuntimeCost { get; init; }
    public IReadOnlyList<string> ValidationRules { get; init; } = Array.Empty<string>();
    public string Notes { get; init; } = string.Empty;
}

public sealed record GeneratorCatalogDiagnostic
{
    public GeneratorDiagnosticSeverity Severity { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string GeneratorId { get; init; } = string.Empty;
    public string RelatedId { get; init; } = string.Empty;
}

public sealed record GeneratorCatalogValidationResult
{
    public bool Ok { get; init; }
    public IReadOnlyList<GeneratorCatalogDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorCatalogDiagnostic>();

    public IReadOnlyList<GeneratorCatalogDiagnostic> Errors => Diagnostics
        .Where(diagnostic => diagnostic.Severity == GeneratorDiagnosticSeverity.Error)
        .ToList();

    public IReadOnlyList<GeneratorCatalogDiagnostic> Warnings => Diagnostics
        .Where(diagnostic => diagnostic.Severity == GeneratorDiagnosticSeverity.Warning)
        .ToList();
}

public sealed record GeneratorPlanningResult
{
    public IReadOnlyList<GeneratorModuleManifest> SelectedCurrentGenerators { get; init; } = Array.Empty<GeneratorModuleManifest>();
    public IReadOnlyList<GeneratorModuleManifest> RelatedPlannedGenerators { get; init; } = Array.Empty<GeneratorModuleManifest>();
    public IReadOnlyList<string> MissingGeneratorCapabilityIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<GeneratorCatalogDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorCatalogDiagnostic>();
}

public static class GeneratorCatalogDiagnosticCodes
{
    public const string BlankGeneratorId = "generator.catalog.blank_id";
    public const string DuplicateGeneratorId = "generator.catalog.duplicate_id";
    public const string UnknownRequiredCapability = "generator.catalog.unknown_required_capability";
    public const string UnknownOptionalCapability = "generator.catalog.unknown_optional_capability";
    public const string UnknownProvidedCapability = "generator.catalog.unknown_provided_capability";
    public const string UnknownConflictingGenerator = "generator.catalog.unknown_conflicting_generator";
    public const string CurrentDependsOnPlannedCapability = "generator.catalog.current_depends_on_planned_capability";
    public const string DuplicateCurrentOutputContract = "generator.catalog.duplicate_current_output_contract";
    public const string MissingInputContractProducer = "generator.plan.missing_input_contract_producer";
    public const string PlannedGeneratorRelated = "generator.plan.planned_generator_related";
    public const string MissingGeneratorSupport = "generator.plan.missing_generator_support";
}
