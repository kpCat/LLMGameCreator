namespace LLMGameCreator.Application.Composition;

public sealed class GameBlueprintCompositionValidator
{
    private readonly CapabilityRegistry _registry;

    public GameBlueprintCompositionValidator(CapabilityRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public CompositionValidationResult Validate(GameBlueprint blueprint)
    {
        ArgumentNullException.ThrowIfNull(blueprint);

        var diagnostics = new List<CompositionDiagnostic>();
        foreach (var duplicateId in _registry.DuplicateIds)
        {
            diagnostics.Add(Diagnostic(
                CompositionDiagnosticSeverity.Error,
                CompositionCompatibilityStatus.Conflict,
                CompositionDiagnosticCodes.DuplicateRegistryId,
                $"Capability registry contains duplicate id '{duplicateId}'.",
                duplicateId));
        }

        var requestedIds = blueprint.RequestedCapabilityIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var requested = new HashSet<string>(requestedIds, StringComparer.OrdinalIgnoreCase);
        var definitions = new List<CapabilityDefinition>();

        foreach (var capabilityId in requestedIds)
        {
            if (_registry.TryGet(capabilityId, out var definition))
            {
                definitions.Add(definition);
                continue;
            }

            diagnostics.Add(Diagnostic(
                CompositionDiagnosticSeverity.Error,
                CompositionCompatibilityStatus.UnsupportedYet,
                CompositionDiagnosticCodes.UnknownCapability,
                $"Requested capability '{capabilityId}' is not registered.",
                capabilityId));
        }

        var available = new HashSet<string>(requested, StringComparer.OrdinalIgnoreCase);
        foreach (var provided in definitions.SelectMany(definition => definition.Provides))
        {
            if (!string.IsNullOrWhiteSpace(provided))
            {
                available.Add(provided.Trim());
            }
        }

        foreach (var definition in definitions.OrderBy(item => item.Id, StringComparer.OrdinalIgnoreCase))
        {
            AddMaturityDiagnostic(definition, diagnostics);
            AddRequirementDiagnostics(definition, available, diagnostics);
            AddBlueprintCompatibilityDiagnostics(blueprint, definition, diagnostics);
        }

        AddConflictDiagnostics(definitions, available, diagnostics);

        var ordered = diagnostics
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.CapabilityId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.RelatedCapabilityId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var ok = ordered.All(diagnostic => diagnostic.Severity != CompositionDiagnosticSeverity.Error);

        return new CompositionValidationResult
        {
            Ok = ok,
            Status = ResolveStatus(ordered),
            Diagnostics = ordered
        };
    }

    private static void AddMaturityDiagnostic(
        CapabilityDefinition definition,
        ICollection<CompositionDiagnostic> diagnostics)
    {
        if (definition.Maturity == CapabilityMaturity.Current)
        {
            return;
        }

        diagnostics.Add(Diagnostic(
            CompositionDiagnosticSeverity.Warning,
            CompositionCompatibilityStatus.UnsupportedYet,
            CompositionDiagnosticCodes.UnsupportedYet,
            $"Capability '{definition.Id}' is registered as {definition.Maturity.ToString().ToLowerInvariant()} and is not currently implemented.",
            definition.Id));
    }

    private static void AddRequirementDiagnostics(
        CapabilityDefinition definition,
        IReadOnlySet<string> available,
        ICollection<CompositionDiagnostic> diagnostics)
    {
        foreach (var requirement in Normalize(definition.Requires))
        {
            if (!available.Contains(requirement))
            {
                diagnostics.Add(Diagnostic(
                    CompositionDiagnosticSeverity.Error,
                    CompositionCompatibilityStatus.MissingRequirement,
                    CompositionDiagnosticCodes.MissingRequirement,
                    $"Capability '{definition.Id}' requires missing capability '{requirement}'.",
                    definition.Id,
                    requirement));
            }
        }

        foreach (var requirement in Normalize(definition.OptionalRequires))
        {
            if (!available.Contains(requirement))
            {
                diagnostics.Add(Diagnostic(
                    CompositionDiagnosticSeverity.Warning,
                    CompositionCompatibilityStatus.DegradedButUsable,
                    CompositionDiagnosticCodes.OptionalRequirementMissing,
                    $"Capability '{definition.Id}' can use optional capability '{requirement}', but it is not requested.",
                    definition.Id,
                    requirement));
            }
        }
    }

    private static void AddConflictDiagnostics(
        IEnumerable<CapabilityDefinition> definitions,
        IReadOnlySet<string> available,
        ICollection<CompositionDiagnostic> diagnostics)
    {
        var reportedPairs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in definitions.OrderBy(item => item.Id, StringComparer.OrdinalIgnoreCase))
        {
            foreach (var conflict in Normalize(definition.Conflicts))
            {
                if (!available.Contains(conflict))
                {
                    continue;
                }

                var pair = string.Compare(definition.Id, conflict, StringComparison.OrdinalIgnoreCase) <= 0
                    ? definition.Id + "|" + conflict
                    : conflict + "|" + definition.Id;
                if (!reportedPairs.Add(pair))
                {
                    continue;
                }

                diagnostics.Add(Diagnostic(
                    CompositionDiagnosticSeverity.Error,
                    CompositionCompatibilityStatus.Conflict,
                    CompositionDiagnosticCodes.DirectConflict,
                    $"Capability '{definition.Id}' conflicts with '{conflict}'.",
                    definition.Id,
                    conflict));
            }
        }
    }

    private static void AddBlueprintCompatibilityDiagnostics(
        GameBlueprint blueprint,
        CapabilityDefinition definition,
        ICollection<CompositionDiagnostic> diagnostics)
    {
        if (definition.SupportedWorldSources.Count > 0 &&
            blueprint.WorldSources.Count > 0 &&
            !definition.SupportedWorldSources.Intersect(blueprint.WorldSources).Any())
        {
            diagnostics.Add(Diagnostic(
                CompositionDiagnosticSeverity.Error,
                CompositionCompatibilityStatus.Conflict,
                CompositionDiagnosticCodes.UnsupportedWorldSource,
                $"Capability '{definition.Id}' does not support the blueprint world source.",
                definition.Id));
        }

        if (definition.SupportedPresentations.Count > 0 &&
            blueprint.Presentations.Count > 0 &&
            !definition.SupportedPresentations.Intersect(blueprint.Presentations).Any())
        {
            diagnostics.Add(Diagnostic(
                CompositionDiagnosticSeverity.Error,
                CompositionCompatibilityStatus.Conflict,
                CompositionDiagnosticCodes.UnsupportedPresentation,
                $"Capability '{definition.Id}' does not support the blueprint presentation.",
                definition.Id));
        }

        if (definition.GenerationModes.Count > 0 &&
            blueprint.GenerationModes.Count > 0 &&
            !definition.GenerationModes.Intersect(blueprint.GenerationModes).Any())
        {
            diagnostics.Add(Diagnostic(
                CompositionDiagnosticSeverity.Error,
                CompositionCompatibilityStatus.Conflict,
                CompositionDiagnosticCodes.UnsupportedGenerationMode,
                $"Capability '{definition.Id}' does not support the blueprint generation mode.",
                definition.Id));
        }
    }

    private static CompositionCompatibilityStatus ResolveStatus(IReadOnlyList<CompositionDiagnostic> diagnostics)
    {
        if (diagnostics.Any(diagnostic => diagnostic.Status == CompositionCompatibilityStatus.Conflict))
        {
            return CompositionCompatibilityStatus.Conflict;
        }

        if (diagnostics.Any(diagnostic => diagnostic.Status == CompositionCompatibilityStatus.MissingRequirement))
        {
            return CompositionCompatibilityStatus.MissingRequirement;
        }

        if (diagnostics.Any(diagnostic => diagnostic.Status == CompositionCompatibilityStatus.UnsupportedYet))
        {
            return CompositionCompatibilityStatus.UnsupportedYet;
        }

        if (diagnostics.Any(diagnostic => diagnostic.Status == CompositionCompatibilityStatus.DegradedButUsable))
        {
            return CompositionCompatibilityStatus.DegradedButUsable;
        }

        return CompositionCompatibilityStatus.Compatible;
    }

    private static IReadOnlyList<string> Normalize(IEnumerable<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static CompositionDiagnostic Diagnostic(
        CompositionDiagnosticSeverity severity,
        CompositionCompatibilityStatus status,
        string code,
        string message,
        string capabilityId,
        string relatedCapabilityId = "")
    {
        return new CompositionDiagnostic
        {
            Severity = severity,
            Status = status,
            Code = code,
            Message = message,
            CapabilityId = capabilityId,
            RelatedCapabilityId = relatedCapabilityId
        };
    }

    private static int SeverityOrder(CompositionDiagnosticSeverity severity)
    {
        return severity switch
        {
            CompositionDiagnosticSeverity.Error => 0,
            CompositionDiagnosticSeverity.Warning => 1,
            _ => 2
        };
    }
}
