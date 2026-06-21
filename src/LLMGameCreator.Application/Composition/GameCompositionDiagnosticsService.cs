using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.Application.Composition;

public sealed class GameCompositionDiagnosticsService
{
    private const string CapabilitySource = "capability";
    private const string GeneratorCatalogSource = "generator_catalog";
    private const string GeneratorPlanSource = "generator_plan";

    private readonly GameBlueprintCompositionValidator _capabilityValidator;
    private readonly GeneratorCatalogValidator _catalogValidator;
    private readonly GeneratorPlanResolver _planResolver;
    private readonly GeneratorCatalog _catalog;

    public GameCompositionDiagnosticsService(
        GameBlueprintCompositionValidator capabilityValidator,
        GeneratorCatalogValidator catalogValidator,
        GeneratorPlanResolver planResolver,
        GeneratorCatalog catalog)
    {
        _capabilityValidator = capabilityValidator ?? throw new ArgumentNullException(nameof(capabilityValidator));
        _catalogValidator = catalogValidator ?? throw new ArgumentNullException(nameof(catalogValidator));
        _planResolver = planResolver ?? throw new ArgumentNullException(nameof(planResolver));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    public GameCompositionDiagnosticsReport CreateReport(
        GameBlueprint blueprint,
        ContentLanguagePolicy? contentLanguagePolicy = null)
    {
        ArgumentNullException.ThrowIfNull(blueprint);

        var capabilityValidation = _capabilityValidator.Validate(blueprint);
        var catalogValidation = _catalogValidator.Validate(_catalog);
        var planning = _planResolver.Resolve(blueprint);
        var diagnostics = ConsolidateDiagnostics(capabilityValidation, catalogValidation, planning);
        var selectedGeneratorIds = Normalize(planning.SelectedCurrentGenerators.Select(manifest => manifest.GeneratorId));
        var plannedGeneratorIds = Normalize(planning.RelatedPlannedGenerators.Select(manifest => manifest.GeneratorId));
        var missingGeneratorCapabilityIds = Normalize(planning.MissingGeneratorCapabilityIds);

        return new GameCompositionDiagnosticsReport
        {
            BlueprintId = blueprint.BlueprintId.Trim(),
            Title = blueprint.Title.Trim(),
            GameKind = blueprint.GameKind,
            ContentLanguage = contentLanguagePolicy?.Normalize().ContentLanguage
                ?? ContentLanguageCodes.Normalize(blueprint.ContentLanguage),
            Readiness = ResolveReadiness(capabilityValidation, catalogValidation, planning),
            RequestedCapabilityIds = Normalize(blueprint.RequestedCapabilityIds),
            CapabilityValidationResult = capabilityValidation,
            GeneratorCatalogValidationResult = catalogValidation,
            GeneratorPlanningResult = planning,
            SelectedCurrentGeneratorIds = selectedGeneratorIds,
            RelatedPlannedGeneratorIds = plannedGeneratorIds,
            MissingGeneratorCapabilityIds = missingGeneratorCapabilityIds,
            Diagnostics = diagnostics,
            RecommendedActions = BuildRecommendedActions(capabilityValidation, catalogValidation, planning)
        };
    }

    private static GameCompositionReadiness ResolveReadiness(
        CompositionValidationResult capabilityValidation,
        GeneratorCatalogValidationResult catalogValidation,
        GeneratorPlanningResult planning)
    {
        if (!catalogValidation.Ok || capabilityValidation.Diagnostics.Any(diagnostic =>
                diagnostic.Code is CompositionDiagnosticCodes.DuplicateRegistryId or CompositionDiagnosticCodes.UnknownCapability))
        {
            return GameCompositionReadiness.Invalid;
        }

        if (capabilityValidation.Status == CompositionCompatibilityStatus.Conflict)
        {
            return GameCompositionReadiness.Conflict;
        }

        if (capabilityValidation.Status == CompositionCompatibilityStatus.MissingRequirement ||
            planning.MissingGeneratorCapabilityIds.Count > 0)
        {
            return GameCompositionReadiness.MissingRequirements;
        }

        if (planning.RelatedPlannedGenerators.Count > 0 ||
            capabilityValidation.Status == CompositionCompatibilityStatus.UnsupportedYet)
        {
            return GameCompositionReadiness.PlannedFuture;
        }

        if (capabilityValidation.Diagnostics.Count > 0 ||
            catalogValidation.Diagnostics.Count > 0 ||
            planning.Diagnostics.Count > 0)
        {
            return GameCompositionReadiness.BuildableWithWarnings;
        }

        return GameCompositionReadiness.BuildableNow;
    }

    private static IReadOnlyList<GameCompositionDiagnosticItem> ConsolidateDiagnostics(
        CompositionValidationResult capabilityValidation,
        GeneratorCatalogValidationResult catalogValidation,
        GeneratorPlanningResult planning)
    {
        var diagnostics = new List<GameCompositionDiagnosticItem>();
        diagnostics.AddRange(capabilityValidation.Diagnostics.Select(diagnostic => new GameCompositionDiagnosticItem
        {
            Source = CapabilitySource,
            Severity = ConvertSeverity(diagnostic.Severity),
            Code = diagnostic.Code,
            Message = diagnostic.Message,
            SubjectId = diagnostic.CapabilityId,
            RelatedId = diagnostic.RelatedCapabilityId
        }));
        diagnostics.AddRange(catalogValidation.Diagnostics.Select(diagnostic => ConvertGeneratorDiagnostic(GeneratorCatalogSource, diagnostic)));
        diagnostics.AddRange(planning.Diagnostics.Select(diagnostic => ConvertGeneratorDiagnostic(GeneratorPlanSource, diagnostic)));

        return diagnostics
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Source, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.SubjectId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.RelatedId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<GameCompositionRecommendedAction> BuildRecommendedActions(
        CompositionValidationResult capabilityValidation,
        GeneratorCatalogValidationResult catalogValidation,
        GeneratorPlanningResult planning)
    {
        var actions = new List<GameCompositionRecommendedAction>();

        foreach (var capabilityId in capabilityValidation.Diagnostics
                     .Where(diagnostic => diagnostic.Code == CompositionDiagnosticCodes.MissingRequirement)
                     .Select(diagnostic => diagnostic.RelatedCapabilityId)
                     .Concat(capabilityValidation.Diagnostics
                         .Where(diagnostic => diagnostic.Code == CompositionDiagnosticCodes.UnknownCapability)
                         .Select(diagnostic => diagnostic.CapabilityId))
                     .Where(id => !string.IsNullOrWhiteSpace(id))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            actions.Add(Action(
                "composition.action.add_capability",
                capabilityId,
                $"Add or request capability '{capabilityId}'."));
        }

        foreach (var diagnostic in capabilityValidation.Diagnostics
                     .Where(diagnostic => diagnostic.Code == CompositionDiagnosticCodes.DirectConflict))
        {
            var pair = Normalize([diagnostic.CapabilityId, diagnostic.RelatedCapabilityId]);
            if (pair.Count == 2)
            {
                actions.Add(Action(
                    "composition.action.remove_conflict",
                    string.Join("|", pair),
                    $"Remove one of conflicting capabilities '{pair[0]}' / '{pair[1]}'."));
            }
        }

        foreach (var generatorId in planning.RelatedPlannedGenerators.Select(manifest => manifest.GeneratorId))
        {
            actions.Add(Action(
                "composition.action.implement_planned_generator",
                generatorId,
                $"Implement planned generator '{generatorId}' before runtime use."));
        }

        foreach (var capabilityId in planning.MissingGeneratorCapabilityIds)
        {
            actions.Add(Action(
                "composition.action.add_generator_support",
                capabilityId,
                $"Add generator support for capability '{capabilityId}'."));
        }

        foreach (var diagnostic in catalogValidation.Errors)
        {
            var targetId = string.IsNullOrWhiteSpace(diagnostic.GeneratorId) ? diagnostic.RelatedId : diagnostic.GeneratorId;
            actions.Add(Action(
                "composition.action.fix_generator_catalog",
                targetId,
                $"Resolve generator catalog diagnostic '{diagnostic.Code}' for '{targetId}'."));
        }

        if (actions.Count == 0)
        {
            actions.Add(Action(
                "composition.action.proceed_reviewed_generation",
                string.Empty,
                "Proceed with reviewed generation using the selected current generators."));
        }

        return actions
            .DistinctBy(action => (action.Code.ToUpperInvariant(), action.TargetId.ToUpperInvariant(), action.Message))
            .OrderBy(action => action.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(action => action.TargetId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(action => action.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static GameCompositionDiagnosticItem ConvertGeneratorDiagnostic(
        string source,
        GeneratorCatalogDiagnostic diagnostic)
    {
        return new GameCompositionDiagnosticItem
        {
            Source = source,
            Severity = ConvertSeverity(diagnostic.Severity),
            Code = diagnostic.Code,
            Message = diagnostic.Message,
            SubjectId = diagnostic.GeneratorId,
            RelatedId = diagnostic.RelatedId
        };
    }

    private static GameCompositionDiagnosticSeverity ConvertSeverity(CompositionDiagnosticSeverity severity)
    {
        return severity switch
        {
            CompositionDiagnosticSeverity.Error => GameCompositionDiagnosticSeverity.Error,
            CompositionDiagnosticSeverity.Warning => GameCompositionDiagnosticSeverity.Warning,
            _ => GameCompositionDiagnosticSeverity.Info
        };
    }

    private static GameCompositionDiagnosticSeverity ConvertSeverity(GeneratorDiagnosticSeverity severity)
    {
        return severity switch
        {
            GeneratorDiagnosticSeverity.Error => GameCompositionDiagnosticSeverity.Error,
            GeneratorDiagnosticSeverity.Warning => GameCompositionDiagnosticSeverity.Warning,
            _ => GameCompositionDiagnosticSeverity.Info
        };
    }

    private static int SeverityOrder(GameCompositionDiagnosticSeverity severity)
    {
        return severity switch
        {
            GameCompositionDiagnosticSeverity.Error => 0,
            GameCompositionDiagnosticSeverity.Warning => 1,
            _ => 2
        };
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

    private static GameCompositionRecommendedAction Action(string code, string targetId, string message)
    {
        return new GameCompositionRecommendedAction
        {
            Code = code,
            TargetId = targetId,
            Message = message
        };
    }
}
