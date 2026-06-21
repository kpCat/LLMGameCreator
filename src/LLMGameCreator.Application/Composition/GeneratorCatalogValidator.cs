namespace LLMGameCreator.Application.Composition;

public sealed class GeneratorCatalogValidator
{
    private readonly CapabilityRegistry _capabilityRegistry;

    public GeneratorCatalogValidator(CapabilityRegistry capabilityRegistry)
    {
        _capabilityRegistry = capabilityRegistry ?? throw new ArgumentNullException(nameof(capabilityRegistry));
    }

    public GeneratorCatalogValidationResult Validate(GeneratorCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var diagnostics = new List<GeneratorCatalogDiagnostic>();
        foreach (var manifest in catalog.Manifests)
        {
            if (string.IsNullOrWhiteSpace(manifest.GeneratorId))
            {
                diagnostics.Add(Diagnostic(
                    GeneratorDiagnosticSeverity.Error,
                    GeneratorCatalogDiagnosticCodes.BlankGeneratorId,
                    "Generator manifest has a blank generator id.",
                    string.Empty));
                continue;
            }

            AddCapabilityDiagnostics(manifest, manifest.RequiresCapabilities, GeneratorCatalogDiagnosticCodes.UnknownRequiredCapability, "required", diagnostics);
            AddCapabilityDiagnostics(manifest, manifest.OptionalCapabilities, GeneratorCatalogDiagnosticCodes.UnknownOptionalCapability, "optional", diagnostics);
            AddCapabilityDiagnostics(manifest, manifest.ProvidesCapabilities, GeneratorCatalogDiagnosticCodes.UnknownProvidedCapability, "provided", diagnostics);

            foreach (var conflictId in Normalize(manifest.ConflictsWithGenerators))
            {
                if (!catalog.TryGet(conflictId, out _))
                {
                    diagnostics.Add(Diagnostic(
                        GeneratorDiagnosticSeverity.Error,
                        GeneratorCatalogDiagnosticCodes.UnknownConflictingGenerator,
                        $"Generator '{manifest.GeneratorId}' conflicts with unknown generator '{conflictId}'.",
                        manifest.GeneratorId,
                        conflictId));
                }
            }

            if (manifest.Maturity is GeneratorMaturity.Current or GeneratorMaturity.Preview)
            {
                foreach (var capabilityId in Normalize(manifest.RequiresCapabilities))
                {
                    if (_capabilityRegistry.TryGet(capabilityId, out var capability) &&
                        capability.Maturity != CapabilityMaturity.Current)
                    {
                        diagnostics.Add(Diagnostic(
                            GeneratorDiagnosticSeverity.Error,
                            GeneratorCatalogDiagnosticCodes.CurrentDependsOnPlannedCapability,
                            $"Current generator '{manifest.GeneratorId}' depends on non-current capability '{capabilityId}'.",
                            manifest.GeneratorId,
                            capabilityId));
                    }
                }
            }
        }

        foreach (var duplicateId in catalog.DuplicateIds)
        {
            diagnostics.Add(Diagnostic(
                GeneratorDiagnosticSeverity.Error,
                GeneratorCatalogDiagnosticCodes.DuplicateGeneratorId,
                $"Generator catalog contains duplicate id '{duplicateId}'.",
                duplicateId));
        }

        foreach (var outputGroup in catalog.Current
                     .SelectMany(manifest => Normalize(manifest.OutputContracts).Select(contractId => (manifest, contractId)))
                     .GroupBy(item => item.contractId, StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1))
        {
            diagnostics.Add(Diagnostic(
                GeneratorDiagnosticSeverity.Warning,
                GeneratorCatalogDiagnosticCodes.DuplicateCurrentOutputContract,
                $"Current output contract '{outputGroup.Key}' is produced by multiple generators.",
                outputGroup.First().manifest.GeneratorId,
                outputGroup.Key));
        }

        var ordered = Order(diagnostics);
        return new GeneratorCatalogValidationResult
        {
            Ok = ordered.All(diagnostic => diagnostic.Severity != GeneratorDiagnosticSeverity.Error),
            Diagnostics = ordered
        };
    }

    private void AddCapabilityDiagnostics(
        GeneratorModuleManifest manifest,
        IEnumerable<string> capabilityIds,
        string code,
        string relationship,
        ICollection<GeneratorCatalogDiagnostic> diagnostics)
    {
        foreach (var capabilityId in Normalize(capabilityIds))
        {
            if (!_capabilityRegistry.TryGet(capabilityId, out _))
            {
                diagnostics.Add(Diagnostic(
                    GeneratorDiagnosticSeverity.Error,
                    code,
                    $"Generator '{manifest.GeneratorId}' references unknown {relationship} capability '{capabilityId}'.",
                    manifest.GeneratorId,
                    capabilityId));
            }
        }
    }

    internal static IReadOnlyList<GeneratorCatalogDiagnostic> Order(IEnumerable<GeneratorCatalogDiagnostic> diagnostics)
    {
        return diagnostics
            .OrderBy(diagnostic => diagnostic.Severity == GeneratorDiagnosticSeverity.Error ? 0 : diagnostic.Severity == GeneratorDiagnosticSeverity.Warning ? 1 : 2)
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.GeneratorId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.RelatedId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
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

    private static GeneratorCatalogDiagnostic Diagnostic(
        GeneratorDiagnosticSeverity severity,
        string code,
        string message,
        string generatorId,
        string relatedId = "")
    {
        return new GeneratorCatalogDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            GeneratorId = generatorId,
            RelatedId = relatedId
        };
    }
}

public sealed class GeneratorPlanResolver
{
    private readonly CapabilityRegistry _capabilityRegistry;
    private readonly GeneratorCatalog _catalog;

    public GeneratorPlanResolver(CapabilityRegistry capabilityRegistry, GeneratorCatalog catalog)
    {
        _capabilityRegistry = capabilityRegistry ?? throw new ArgumentNullException(nameof(capabilityRegistry));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    public GeneratorPlanningResult Resolve(GameBlueprint blueprint)
    {
        ArgumentNullException.ThrowIfNull(blueprint);

        var requested = blueprint.RequestedCapabilityIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var selected = _catalog.Current
            .Where(manifest => manifest.ProvidesCapabilities.Any(requested.Contains))
            .ToDictionary(manifest => manifest.GeneratorId, StringComparer.OrdinalIgnoreCase);
        var diagnostics = new List<GeneratorCatalogDiagnostic>();

        AddInputContractClosure(selected, diagnostics);

        var requestedPlannedCapabilities = requested
            .Where(id => _capabilityRegistry.TryGet(id, out var capability) &&
                         capability.Maturity != CapabilityMaturity.Current)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var planned = _catalog.Planned
            .Where(manifest => manifest.ProvidesCapabilities.Any(requested.Contains) ||
                               manifest.RequiresCapabilities.Any(requestedPlannedCapabilities.Contains))
            .OrderBy(manifest => manifest.GeneratorId, StringComparer.OrdinalIgnoreCase)
            .ToList();
        foreach (var manifest in planned)
        {
            diagnostics.Add(new GeneratorCatalogDiagnostic
            {
                Severity = GeneratorDiagnosticSeverity.Warning,
                Code = GeneratorCatalogDiagnosticCodes.PlannedGeneratorRelated,
                Message = $"Planned generator '{manifest.GeneratorId}' is related to requested blueprint capabilities.",
                GeneratorId = manifest.GeneratorId
            });
        }

        var provided = _catalog.Manifests
            .SelectMany(manifest => manifest.ProvidesCapabilities)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = requested
            .Where(id => _capabilityRegistry.TryGet(id, out var capability) &&
                         capability.Maturity != CapabilityMaturity.Current &&
                         !provided.Contains(id))
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToList();
        foreach (var capabilityId in missing)
        {
            diagnostics.Add(new GeneratorCatalogDiagnostic
            {
                Severity = GeneratorDiagnosticSeverity.Warning,
                Code = GeneratorCatalogDiagnosticCodes.MissingGeneratorSupport,
                Message = $"Requested planned capability '{capabilityId}' has no generator manifest that provides it.",
                RelatedId = capabilityId
            });
        }

        return new GeneratorPlanningResult
        {
            SelectedCurrentGenerators = selected.Values.OrderBy(manifest => manifest.GeneratorId, StringComparer.OrdinalIgnoreCase).ToList(),
            RelatedPlannedGenerators = planned,
            MissingGeneratorCapabilityIds = missing,
            Diagnostics = GeneratorCatalogValidator.Order(diagnostics)
        };
    }

    private void AddInputContractClosure(
        IDictionary<string, GeneratorModuleManifest> selected,
        ICollection<GeneratorCatalogDiagnostic> diagnostics)
    {
        var changed = true;
        while (changed)
        {
            changed = false;
            var inputContracts = selected.Values
                .SelectMany(manifest => manifest.InputContracts)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            foreach (var inputContract in inputContracts)
            {
                var producer = _catalog.Current.FirstOrDefault(manifest =>
                    manifest.OutputContracts.Contains(inputContract, StringComparer.OrdinalIgnoreCase));
                if (producer is null)
                {
                    if (!diagnostics.Any(diagnostic =>
                            diagnostic.Code == GeneratorCatalogDiagnosticCodes.MissingInputContractProducer &&
                            string.Equals(diagnostic.RelatedId, inputContract, StringComparison.OrdinalIgnoreCase)))
                    {
                        diagnostics.Add(new GeneratorCatalogDiagnostic
                        {
                            Severity = GeneratorDiagnosticSeverity.Warning,
                            Code = GeneratorCatalogDiagnosticCodes.MissingInputContractProducer,
                            Message = $"No current generator produces required input contract '{inputContract}'.",
                            RelatedId = inputContract
                        });
                    }

                    continue;
                }

                if (!selected.ContainsKey(producer.GeneratorId))
                {
                    selected.Add(producer.GeneratorId, producer);
                    changed = true;
                }
            }
        }
    }
}
