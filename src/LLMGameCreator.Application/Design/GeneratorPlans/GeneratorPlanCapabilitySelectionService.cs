using System.Security.Cryptography;
using System.Text;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanCapabilitySelectionService
{
    private readonly GeneratorPlanCapabilitySelectionAtlasReader _atlasReader;

    public GeneratorPlanCapabilitySelectionService(GeneratorPlanCapabilitySelectionAtlasReader atlasReader)
    {
        _atlasReader = atlasReader;
    }

    public async Task<GeneratorPlanCapabilitySelectionResult> BuildSelectionAsync(
        GeneratorPlanCapabilitySelectionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var generatedAtUtc = DateTimeOffset.UtcNow;
        var diagnostics = new List<GeneratorPlanCapabilitySelectionDiagnostic>();
        var atlas = await _atlasReader.LoadAsync(request.AtlasRootPath, cancellationToken).ConfigureAwait(false);
        diagnostics.AddRange(atlas.Diagnostics);

        if (atlas.Diagnostics.Any(IsError))
        {
            return BuildResult(request, generatedAtUtc, diagnostics, new SelectionAccumulator());
        }

        var accumulator = new SelectionAccumulator();
        var selectedVariants = SelectedVariants(request);
        ValidateRequiredVariant("presentation_mode", request.PresentationModeId, atlas.PresentationModes, diagnostics, accumulator);
        ValidateRequiredVariant("world_topology", request.WorldTopologyId, atlas.WorldTopologies, diagnostics, accumulator);
        ValidateRequiredVariant("actor_model", request.ActorModelId, atlas.ActorModels, diagnostics, accumulator);
        ValidateRequiredVariant("inventory_model", request.InventoryModelId, atlas.InventoryModels, diagnostics, accumulator);
        ValidateRequiredVariant("combat_model", request.CombatModelId, atlas.CombatModels, diagnostics, accumulator);
        ValidateRequiredVariant("progression_model", request.ProgressionModelId, atlas.ProgressionModels, diagnostics, accumulator);
        ValidateRequiredVariant("pathfinding", request.PathfindingProfileId, atlas.PathfindingProfiles, diagnostics, accumulator);
        ValidateRequiredVariant("npc_behavior", request.NpcBehaviorModelId, atlas.NpcBehaviorModels, diagnostics, accumulator);

        ValidatePresentationCompatibility(request, atlas, diagnostics);
        ValidateExplicitIncompatibilities(selectedVariants, atlas, diagnostics);

        ResolveRuntimeTargets(request, atlas, diagnostics, accumulator);
        ResolveFeatureBundles(request.SelectedFeatureBundleIds, atlas, diagnostics, accumulator);
        ResolveCapabilities(atlas, diagnostics, accumulator);
        ValidateKnownArtifactContracts(atlas, diagnostics, accumulator);
        AddMissingValidatorWarnings(atlas, diagnostics, accumulator);

        return BuildResult(request, generatedAtUtc, diagnostics, accumulator);
    }

    public Task<GeneratorPlanCapabilitySelectionAtlas> LoadAtlasAsync(
        string atlasRootPath,
        CancellationToken cancellationToken = default)
    {
        return _atlasReader.LoadAsync(atlasRootPath, cancellationToken);
    }

    public string DiscoverAtlasRoot()
    {
        return _atlasReader.DiscoverAtlasRoot();
    }

    private static void ValidateRequiredVariant(
        string group,
        string selectedId,
        IReadOnlyList<GeneratorPlanCapabilitySelectionAtlasOption> options,
        List<GeneratorPlanCapabilitySelectionDiagnostic> diagnostics,
        SelectionAccumulator accumulator)
    {
        var id = Normalize(selectedId);
        if (string.IsNullOrWhiteSpace(id))
        {
            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanCapabilitySelectionDiagnosticCodes.MissingVariantId,
                $"Missing selected {group} id.",
                group));
            return;
        }

        var option = options.FirstOrDefault(item => SameId(item.Id, id));
        if (option == null)
        {
            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanCapabilitySelectionDiagnosticCodes.UnknownVariantId,
                $"Unknown {group} id: {id}",
                id));
            return;
        }

        accumulator.AddContracts(option.RequiredArtifactContracts);
        accumulator.AddValidators(option.RequiredValidators);
    }

    private static void ValidatePresentationCompatibility(
        GeneratorPlanCapabilitySelectionRequest request,
        GeneratorPlanCapabilitySelectionAtlas atlas,
        List<GeneratorPlanCapabilitySelectionDiagnostic> diagnostics)
    {
        var presentation = atlas.PresentationModes.FirstOrDefault(item => SameId(item.Id, request.PresentationModeId));
        if (presentation == null)
        {
            return;
        }

        if (presentation.AllowedWorldTopologies.Count > 0 &&
            !presentation.AllowedWorldTopologies.Any(id => SameId(id, request.WorldTopologyId)))
        {
            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanCapabilitySelectionDiagnosticCodes.IncompatiblePresentationWorld,
                $"Presentation mode '{request.PresentationModeId}' does not allow world topology '{request.WorldTopologyId}'.",
                request.WorldTopologyId));
        }

        if (presentation.RecommendedActorModels.Count > 0 &&
            !presentation.RecommendedActorModels.Any(id => SameId(id, request.ActorModelId)))
        {
            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Warning,
                GeneratorPlanCapabilitySelectionDiagnosticCodes.VariantNotRecommended,
                $"Actor model '{request.ActorModelId}' is not recommended for presentation mode '{request.PresentationModeId}'.",
                request.ActorModelId));
        }

        if (presentation.RecommendedCombatModels.Count > 0 &&
            !presentation.RecommendedCombatModels.Any(id => SameId(id, request.CombatModelId)))
        {
            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Warning,
                GeneratorPlanCapabilitySelectionDiagnosticCodes.VariantNotRecommended,
                $"Combat model '{request.CombatModelId}' is not recommended for presentation mode '{request.PresentationModeId}'.",
                request.CombatModelId));
        }
    }

    private static void ValidateExplicitIncompatibilities(
        IReadOnlyList<string> selectedVariants,
        GeneratorPlanCapabilitySelectionAtlas atlas,
        List<GeneratorPlanCapabilitySelectionDiagnostic> diagnostics)
    {
        foreach (var option in AllOptions(atlas).Where(option => selectedVariants.Any(id => SameId(id, option.Id))))
        {
            foreach (var incompatible in option.IncompatibleWith.Where(value => selectedVariants.Any(id => SameId(id, value))))
            {
                diagnostics.Add(Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanCapabilitySelectionDiagnosticCodes.VariantExplicitlyIncompatible,
                    $"Selected variant '{option.Id}' is explicitly incompatible with '{incompatible}'.",
                    option.Id));
            }
        }
    }

    private static void ResolveRuntimeTargets(
        GeneratorPlanCapabilitySelectionRequest request,
        GeneratorPlanCapabilitySelectionAtlas atlas,
        List<GeneratorPlanCapabilitySelectionDiagnostic> diagnostics,
        SelectionAccumulator accumulator)
    {
        foreach (var runtimeTargetId in NormalizeMany(request.SelectedRuntimeTargetIds))
        {
            if (!atlas.RuntimeTargets.Any(target => SameId(target.Id, runtimeTargetId)))
            {
                diagnostics.Add(Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanCapabilitySelectionDiagnosticCodes.UnknownRuntimeTargetId,
                    $"Unknown runtime target id: {runtimeTargetId}",
                    runtimeTargetId));
                continue;
            }

            accumulator.RuntimeTargets.Add(runtimeTargetId);
        }
    }

    private static void ResolveFeatureBundles(
        IReadOnlyList<string> selectedFeatureBundleIds,
        GeneratorPlanCapabilitySelectionAtlas atlas,
        List<GeneratorPlanCapabilitySelectionDiagnostic> diagnostics,
        SelectionAccumulator accumulator)
    {
        var selected = NormalizeMany(selectedFeatureBundleIds).ToList();
        if (selected.Count == 0)
        {
            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Warning,
                GeneratorPlanCapabilitySelectionDiagnosticCodes.NoFeatureBundlesSelected,
                "No feature bundles were selected.",
                "feature_bundles"));
            return;
        }

        var bundleById = atlas.FeatureBundles.ToDictionary(bundle => bundle.Id, StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>(selected);
        while (queue.Count > 0)
        {
            var bundleId = queue.Dequeue();
            if (!bundleById.TryGetValue(bundleId, out var bundle))
            {
                diagnostics.Add(Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanCapabilitySelectionDiagnosticCodes.UnknownFeatureBundleId,
                    $"Unknown feature bundle id: {bundleId}",
                    bundleId));
                continue;
            }

            if (!accumulator.FeatureBundleIds.Add(bundle.Id))
            {
                continue;
            }

            accumulator.AddContracts(bundle.ArtifactContracts);
            accumulator.AddValidators(bundle.Validators);
            accumulator.AddRuntimeTargets(bundle.RuntimeTargets);
            accumulator.AddPromptContextTemplates(bundle.PromptContextTemplates);
            accumulator.AddGaps(bundle.FutureModuleGaps);
            accumulator.AddCapabilities(bundle.Provides.Where(IsCapabilityLike));

            foreach (var requirement in bundle.Requires)
            {
                if (requirement.StartsWith("feature_bundle/", StringComparison.OrdinalIgnoreCase))
                {
                    queue.Enqueue(requirement);
                    continue;
                }

                if (IsCapabilityLike(requirement))
                {
                    accumulator.CapabilityIds.Add(requirement);
                }
                else
                {
                    accumulator.RequiredOrGapIds.Add(requirement);
                }
            }

            foreach (var incompatibleBundleId in bundle.IncompatibleWith.Where(value => accumulator.FeatureBundleIds.Contains(value)))
            {
                diagnostics.Add(Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanCapabilitySelectionDiagnosticCodes.VariantExplicitlyIncompatible,
                    $"Selected feature bundle '{bundle.Id}' is incompatible with '{incompatibleBundleId}'.",
                    bundle.Id));
            }
        }
    }

    private static void ResolveCapabilities(
        GeneratorPlanCapabilitySelectionAtlas atlas,
        List<GeneratorPlanCapabilitySelectionDiagnostic> diagnostics,
        SelectionAccumulator accumulator)
    {
        var capabilityById = atlas.Capabilities.ToDictionary(capability => capability.Id, StringComparer.OrdinalIgnoreCase);
        var providedToCapability = atlas.Capabilities
            .SelectMany(capability => capability.Provides.Select(provided => new { provided, capability }))
            .GroupBy(pair => pair.provided, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().capability, StringComparer.OrdinalIgnoreCase);

        var initialCapabilityIds = accumulator.CapabilityIds.ToList();
        accumulator.CapabilityIds.Clear();
        var queue = new Queue<string>(initialCapabilityIds);
        foreach (var requirement in accumulator.RequiredOrGapIds.ToList())
        {
            if (capabilityById.ContainsKey(requirement) || providedToCapability.ContainsKey(requirement))
            {
                queue.Enqueue(requirement);
            }
            else
            {
                accumulator.RequiredLuaModulesOrGaps.Add(requirement);
            }
        }

        var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            var capability = capabilityById.TryGetValue(id, out var direct)
                ? direct
                : providedToCapability.GetValueOrDefault(id);
            if (capability == null)
            {
                accumulator.RequiredLuaModulesOrGaps.Add(id);
                continue;
            }

            if (!processed.Add(capability.Id))
            {
                continue;
            }

            accumulator.CapabilityIds.Add(capability.Id);
            accumulator.AddContracts(capability.OutputContracts);
            accumulator.AddValidators(capability.Validators);
            accumulator.AddRuntimeTargets(capability.RuntimeTargets);
            foreach (var dependency in capability.DependsOn)
            {
                queue.Enqueue(dependency);
            }
        }

        foreach (var gap in accumulator.RequiredLuaModulesOrGaps)
        {
            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Warning,
                GeneratorPlanCapabilitySelectionDiagnosticCodes.CapabilityGap,
                $"Capability, contract, module or future requirement is not resolved by the current atlas reader: {gap}",
                gap));
        }
    }

    private static void ValidateKnownArtifactContracts(
        GeneratorPlanCapabilitySelectionAtlas atlas,
        List<GeneratorPlanCapabilitySelectionDiagnostic> diagnostics,
        SelectionAccumulator accumulator)
    {
        var knownContracts = atlas.ArtifactContracts.Select(contract => contract.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var contractId in accumulator.ArtifactContracts.OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
        {
            if (knownContracts.Contains(contractId))
            {
                continue;
            }

            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Warning,
                GeneratorPlanCapabilitySelectionDiagnosticCodes.MissingArtifactContract,
                $"Artifact contract '{contractId}' is selected but is not present in artifact_contracts atlas.",
                contractId));
        }
    }

    private static void AddMissingValidatorWarnings(
        GeneratorPlanCapabilitySelectionAtlas atlas,
        List<GeneratorPlanCapabilitySelectionDiagnostic> diagnostics,
        SelectionAccumulator accumulator)
    {
        var knownValidators = AllOptions(atlas)
            .SelectMany(option => option.RequiredValidators)
            .Concat(atlas.FeatureBundles.SelectMany(bundle => bundle.Validators))
            .Concat(atlas.Capabilities.SelectMany(capability => capability.Validators))
            .Concat(atlas.ArtifactContracts.SelectMany(contract => contract.RequiredValidators))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var validator in accumulator.Validators.OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
        {
            if (knownValidators.Contains(validator))
            {
                continue;
            }

            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Warning,
                GeneratorPlanCapabilitySelectionDiagnosticCodes.MissingValidator,
                $"Validator '{validator}' is selected but no validator definition was found in loaded atlas data.",
                validator));
        }
    }

    private static GeneratorPlanCapabilitySelectionResult BuildResult(
        GeneratorPlanCapabilitySelectionRequest request,
        DateTimeOffset generatedAtUtc,
        IReadOnlyList<GeneratorPlanCapabilitySelectionDiagnostic> diagnostics,
        SelectionAccumulator accumulator)
    {
        var selectedFeatureBundleIds = NormalizeMany(request.SelectedFeatureBundleIds).ToList();
        var errors = diagnostics
            .Where(IsError)
            .Select(diagnostic => diagnostic.Message)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(message => message, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var warnings = diagnostics
            .Where(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
            .Select(diagnostic => diagnostic.Message)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(message => message, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var selection = new GeneratorPlanCapabilitySelection
        {
            SelectionId = BuildSelectionId(request, accumulator),
            Title = Normalize(request.Title),
            Purpose = Normalize(request.Purpose),
            SelectedVariantIds = new GeneratorPlanCapabilitySelectedVariantIds
            {
                PresentationModeId = Normalize(request.PresentationModeId),
                WorldTopologyId = Normalize(request.WorldTopologyId),
                ActorModelId = Normalize(request.ActorModelId),
                InventoryModelId = Normalize(request.InventoryModelId),
                CombatModelId = Normalize(request.CombatModelId),
                ProgressionModelId = Normalize(request.ProgressionModelId),
                PathfindingProfileId = Normalize(request.PathfindingProfileId),
                NpcBehaviorModelId = Normalize(request.NpcBehaviorModelId)
            },
            SelectedFeatureBundleIds = selectedFeatureBundleIds,
            SelectedRuntimeTargets = NormalizeMany(request.SelectedRuntimeTargetIds).ToList(),
            ResolvedCapabilityIds = Sorted(accumulator.CapabilityIds),
            ResolvedArtifactContracts = Sorted(accumulator.ArtifactContracts),
            ResolvedValidators = Sorted(accumulator.Validators),
            ResolvedPromptContextTemplates = Sorted(accumulator.PromptContextTemplates),
            ResolvedRuntimeTargets = Sorted(accumulator.RuntimeTargets),
            RequiredLuaModulesOrGaps = Sorted(accumulator.RequiredLuaModulesOrGaps),
            Warnings = warnings,
            Errors = errors,
            GeneratedAtUtc = generatedAtUtc
        };

        var ok = errors.Count == 0;
        var status = !ok
            ? GeneratorPlanCapabilitySelectionStatus.Invalid
            : warnings.Count > 0
                ? GeneratorPlanCapabilitySelectionStatus.ReadyWithWarnings
                : GeneratorPlanCapabilitySelectionStatus.Ready;

        return new GeneratorPlanCapabilitySelectionResult
        {
            Ok = ok,
            Status = status,
            GeneratedAtUtc = generatedAtUtc,
            Selection = selection,
            Diagnostics = diagnostics
                .OrderBy(diagnostic => GeneratorPlanPreviewValidationPolicy.SeverityOrder(diagnostic.Severity))
                .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
                .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
                .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }

    private static string BuildSelectionId(GeneratorPlanCapabilitySelectionRequest request, SelectionAccumulator accumulator)
    {
        var parts = new[]
            {
                Normalize(request.Title),
                Normalize(request.Purpose),
                Normalize(request.PresentationModeId),
                Normalize(request.WorldTopologyId),
                Normalize(request.ActorModelId),
                Normalize(request.InventoryModelId),
                Normalize(request.CombatModelId),
                Normalize(request.ProgressionModelId),
                Normalize(request.PathfindingProfileId),
                Normalize(request.NpcBehaviorModelId)
            }
            .Concat(NormalizeMany(request.SelectedFeatureBundleIds))
            .Concat(NormalizeMany(request.SelectedRuntimeTargetIds))
            .Concat(Sorted(accumulator.FeatureBundleIds));

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", parts)));
        return "generator_plan_capability_selection/" + Convert.ToHexString(bytes)[..16].ToLowerInvariant();
    }

    private static IReadOnlyList<string> SelectedVariants(GeneratorPlanCapabilitySelectionRequest request)
    {
        return
        [
            Normalize(request.PresentationModeId),
            Normalize(request.WorldTopologyId),
            Normalize(request.ActorModelId),
            Normalize(request.InventoryModelId),
            Normalize(request.CombatModelId),
            Normalize(request.ProgressionModelId),
            Normalize(request.PathfindingProfileId),
            Normalize(request.NpcBehaviorModelId)
        ];
    }

    private static IEnumerable<GeneratorPlanCapabilitySelectionAtlasOption> AllOptions(GeneratorPlanCapabilitySelectionAtlas atlas)
    {
        return atlas.PresentationModes
            .Concat(atlas.WorldTopologies)
            .Concat(atlas.ActorModels)
            .Concat(atlas.InventoryModels)
            .Concat(atlas.CombatModels)
            .Concat(atlas.ProgressionModels)
            .Concat(atlas.PathfindingProfiles)
            .Concat(atlas.NpcBehaviorModels);
    }

    private static bool IsCapabilityLike(string id)
    {
        return !string.IsNullOrWhiteSpace(id) &&
               id.Contains('.', StringComparison.Ordinal) &&
               id.Contains("/v", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<string> NormalizeMany(IEnumerable<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(Normalize)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<string> Sorted(IEnumerable<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static bool SameId(string left, string right)
    {
        return string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsError(GeneratorPlanCapabilitySelectionDiagnostic diagnostic)
    {
        return diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error;
    }

    private static GeneratorPlanCapabilitySelectionDiagnostic Diagnostic(
        string severity,
        string code,
        string message,
        string target)
    {
        return new GeneratorPlanCapabilitySelectionDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            Target = target
        };
    }

    private sealed class SelectionAccumulator
    {
        public SortedSet<string> FeatureBundleIds { get; } = new(StringComparer.OrdinalIgnoreCase);
        public SortedSet<string> CapabilityIds { get; } = new(StringComparer.OrdinalIgnoreCase);
        public SortedSet<string> ArtifactContracts { get; } = new(StringComparer.OrdinalIgnoreCase);
        public SortedSet<string> Validators { get; } = new(StringComparer.OrdinalIgnoreCase);
        public SortedSet<string> RuntimeTargets { get; } = new(StringComparer.OrdinalIgnoreCase);
        public SortedSet<string> PromptContextTemplates { get; } = new(StringComparer.OrdinalIgnoreCase);
        public SortedSet<string> RequiredOrGapIds { get; } = new(StringComparer.OrdinalIgnoreCase);
        public SortedSet<string> RequiredLuaModulesOrGaps { get; } = new(StringComparer.OrdinalIgnoreCase);

        public void AddContracts(IEnumerable<string> values) => AddMany(ArtifactContracts, values);
        public void AddValidators(IEnumerable<string> values) => AddMany(Validators, values);
        public void AddRuntimeTargets(IEnumerable<string> values) => AddMany(RuntimeTargets, values);
        public void AddPromptContextTemplates(IEnumerable<string> values) => AddMany(PromptContextTemplates, values);
        public void AddCapabilities(IEnumerable<string> values) => AddMany(CapabilityIds, values);
        public void AddGaps(IEnumerable<string> values) => AddMany(RequiredLuaModulesOrGaps, values);

        private static void AddMany(ISet<string> target, IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    target.Add(value.Trim());
                }
            }
        }
    }
}
