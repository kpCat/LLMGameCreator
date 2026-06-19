using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;

namespace LLMGameCreator.WinForms.Pages.CapabilityPicker;

public sealed class CapabilityPickerPresenter
{
    public const string CoreAtlasPlanningFeatureBundleId = "feature_bundle/core_atlas_planning/v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    public CapabilityPickerViewState FromAtlas(CapabilityPickerViewState state, GeneratorPlanCapabilitySelectionAtlas atlas)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(atlas);

        var diagnostics = atlas.Diagnostics
            .Select(CapabilityPickerDiagnosticRow.FromDiagnostic)
            .OrderBy(row => SeverityOrder(row.Severity))
            .ThenBy(row => row.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Target, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var featureBundles = atlas.FeatureBundles
            .Select(bundle => new CapabilityPickerFeatureBundleViewModel
            {
                Id = bundle.Id,
                Title = bundle.Title,
                Domain = bundle.Domain,
                Category = bundle.Category,
                Purpose = bundle.Purpose,
                ArtifactContractCount = bundle.ArtifactContracts.Count,
                Help = HelpForBundle(bundle),
                IsRequiredTechnicalBase = IsCoreAtlasPlanning(bundle.Id)
            })
            .OrderByDescending(bundle => bundle.IsRequiredTechnicalBase)
            .ThenBy(bundle => bundle.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(bundle => bundle.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var next = state with
        {
            AtlasRootPath = string.IsNullOrWhiteSpace(state.AtlasRootPath) ? atlas.AtlasRootPath : state.AtlasRootPath,
            PresentationModes = Options(atlas.PresentationModes),
            WorldTopologies = Options(atlas.WorldTopologies),
            ActorModels = Options(atlas.ActorModels),
            InventoryModels = Options(atlas.InventoryModels),
            CombatModels = Options(atlas.CombatModels),
            ProgressionModels = Options(atlas.ProgressionModels),
            PathfindingProfiles = Options(atlas.PathfindingProfiles),
            NpcBehaviorModels = Options(atlas.NpcBehaviorModels),
            RuntimeTargets = Options(atlas.RuntimeTargets),
            FeatureBundles = featureBundles,
            AvailableModules = CompositionItems("module"),
            AvailableModifiers = CompositionItems("modifier"),
            AvailableConstraints = CompositionItems("constraint"),
            AvailableRuntimeRequirements = CompositionItems("runtime_requirement"),
            SelectedFeatureBundleIds = SelectDefaultFeatureBundles(state.SelectedFeatureBundleIds, featureBundles),
            Diagnostics = diagnostics,
            Status = atlas.Diagnostics.Any(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error)
                ? "Atlas load failed."
                : "Atlas loaded.",
            Summary = $"Presentation {atlas.PresentationModes.Count}; World {atlas.WorldTopologies.Count}; Bundles {atlas.FeatureBundles.Count}; Runtime targets {atlas.RuntimeTargets.Count}"
        };

        return next with
        {
            PresentationModeId = FirstExisting(next.PresentationModeId, next.PresentationModes),
            WorldTopologyId = FirstExisting(next.WorldTopologyId, next.WorldTopologies),
            ActorModelId = FirstExisting(next.ActorModelId, next.ActorModels),
            InventoryModelId = FirstExisting(next.InventoryModelId, next.InventoryModels),
            CombatModelId = FirstExisting(next.CombatModelId, next.CombatModels),
            ProgressionModelId = FirstExisting(next.ProgressionModelId, next.ProgressionModels),
            PathfindingProfileId = FirstExisting(next.PathfindingProfileId, next.PathfindingProfiles),
            NpcBehaviorModelId = FirstExisting(next.NpcBehaviorModelId, next.NpcBehaviorModels),
            RuntimeTargetId = FirstExisting(next.RuntimeTargetId, next.RuntimeTargets)
        };
    }

    public GeneratorPlanCapabilitySelectionRequest BuildRequest(CapabilityPickerViewState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        return new GeneratorPlanCapabilitySelectionRequest
        {
            AtlasRootPath = state.AtlasRootPath,
            Title = state.Title,
            Purpose = state.Purpose,
            PresentationModeId = state.PresentationModeId,
            WorldTopologyId = state.WorldTopologyId,
            ActorModelId = state.ActorModelId,
            InventoryModelId = state.InventoryModelId,
            CombatModelId = state.CombatModelId,
            ProgressionModelId = state.ProgressionModelId,
            PathfindingProfileId = state.PathfindingProfileId,
            NpcBehaviorModelId = state.NpcBehaviorModelId,
            SelectedFeatureBundleIds = state.SelectedFeatureBundleIds,
            SelectedModuleIds = state.SelectedModuleIds,
            SelectedModifierIds = state.SelectedModifierIds,
            SelectedConstraintIds = state.SelectedConstraintIds,
            RuntimeRequirementIds = state.RuntimeRequirementIds,
            SelectedRuntimeTargetIds = string.IsNullOrWhiteSpace(state.RuntimeTargetId)
                ? Array.Empty<string>()
                : [state.RuntimeTargetId]
        };
    }

    public CapabilityPickerViewState FromSelectionResult(CapabilityPickerViewState state, GeneratorPlanCapabilitySelectionResult result)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(result);

        return state with
        {
            Status = result.Status,
            Summary = BuildSummary("Selection built.", result),
            SelectionJson = JsonSerializer.Serialize(result.Selection, JsonOptions),
            ResolvedArtifactContracts = result.Selection.ResolvedArtifactContracts,
            ResolvedValidators = result.Selection.ResolvedValidators,
            ResolvedRuntimeTargets = result.Selection.ResolvedRuntimeTargets,
            ResolvedPromptContextTemplates = result.Selection.ResolvedPromptContextTemplates,
            CapabilityGaps = result.Selection.RequiredLuaModulesOrGaps,
            Diagnostics = DiagnosticRows(result.Diagnostics),
            CurrentResult = result
        };
    }

    public CapabilityPickerViewState FromLatestSelection(
        CapabilityPickerViewState state,
        GeneratorPlanCapabilitySelectionArtifactReadResult result)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(result);

        if (!result.Exists || result.SelectionArtifact == null)
        {
            return state with
            {
                Status = "not_found",
                Summary = "No capability selection artifact found.",
                SelectionJson = string.Empty,
                Diagnostics = Array.Empty<CapabilityPickerDiagnosticRow>(),
                CurrentResult = null
            };
        }

        var selection = result.Selection;
        var diagnostics = result.ValidationResults
            .Select(result => new CapabilityPickerDiagnosticRow
            {
                Severity = result.Severity,
                Code = result.Code,
                Category = GeneratorPlanCapabilityHelpCatalog.MapDiagnosticCategory(result.Code),
                Target = result.Target,
                Message = result.Message
            })
            .OrderBy(row => SeverityOrder(row.Severity))
            .ThenBy(row => row.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Target, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return state with
        {
            Title = selection.Title,
            Purpose = selection.Purpose,
            PresentationModeId = selection.SelectedVariantIds.PresentationModeId,
            WorldTopologyId = selection.SelectedVariantIds.WorldTopologyId,
            ActorModelId = selection.SelectedVariantIds.ActorModelId,
            InventoryModelId = selection.SelectedVariantIds.InventoryModelId,
            CombatModelId = selection.SelectedVariantIds.CombatModelId,
            ProgressionModelId = selection.SelectedVariantIds.ProgressionModelId,
            PathfindingProfileId = selection.SelectedVariantIds.PathfindingProfileId,
            NpcBehaviorModelId = selection.SelectedVariantIds.NpcBehaviorModelId,
            RuntimeTargetId = selection.SelectedRuntimeTargets.FirstOrDefault() ?? string.Empty,
            SelectedFeatureBundleIds = selection.SelectedFeatureBundleIds,
            SelectedModuleIds = selection.SelectedModuleIds,
            SelectedModifierIds = selection.SelectedModifierIds,
            SelectedConstraintIds = selection.SelectedConstraintIds,
            RuntimeRequirementIds = selection.RuntimeRequirementIds,
            Status = result.SelectionArtifact.ValidationState,
            Summary = $"Latest selection loaded: {selection.SelectionId}",
            SelectionJson = result.SelectionArtifact.Json,
            ResolvedArtifactContracts = selection.ResolvedArtifactContracts,
            ResolvedValidators = selection.ResolvedValidators,
            ResolvedRuntimeTargets = selection.ResolvedRuntimeTargets,
            ResolvedPromptContextTemplates = selection.ResolvedPromptContextTemplates,
            CapabilityGaps = selection.RequiredLuaModulesOrGaps,
            Diagnostics = diagnostics,
            CurrentResult = null
        };
    }

    private static IReadOnlyList<CapabilityPickerDiagnosticRow> DiagnosticRows(IReadOnlyList<GeneratorPlanCapabilitySelectionDiagnostic> diagnostics)
    {
        return diagnostics
            .Select(CapabilityPickerDiagnosticRow.FromDiagnostic)
            .OrderBy(row => SeverityOrder(row.Severity))
            .ThenBy(row => row.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string BuildSummary(string prefix, GeneratorPlanCapabilitySelectionResult result)
    {
        return string.Join(Environment.NewLine, new[]
        {
            prefix,
            $"Status: {result.Status}",
            $"Selection id: {result.Selection.SelectionId}",
            $"Feature bundles: {result.Selection.SelectedFeatureBundleIds.Count}",
            $"Modules: {result.Selection.SelectedModuleIds.Count}",
            $"Modifiers: {result.Selection.SelectedModifierIds.Count}",
            $"Constraints: {result.Selection.SelectedConstraintIds.Count}",
            $"Runtime requirements: {result.Selection.RuntimeRequirementIds.Count}",
            $"Capabilities: {result.Selection.ResolvedCapabilityIds.Count}",
            $"Artifact contracts: {result.Selection.ResolvedArtifactContracts.Count}",
            $"Validators: {result.Selection.ResolvedValidators.Count}",
            $"Runtime targets: {result.Selection.ResolvedRuntimeTargets.Count}",
            $"Errors: {result.Selection.Errors.Count}",
            $"Warnings: {result.Selection.Warnings.Count}"
        });
    }

    private static IReadOnlyList<CapabilityPickerOptionViewModel> Options(IReadOnlyList<GeneratorPlanCapabilitySelectionAtlasOption> options)
    {
        return options
            .Select(option => new CapabilityPickerOptionViewModel
            {
                Id = option.Id,
                Title = option.Title,
                Purpose = option.Purpose,
                Help = HelpForOption(option)
            })
            .OrderBy(option => option.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(option => option.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<CapabilityPickerComposableItemViewModel> CompositionItems(string kind)
    {
        return GeneratorPlanCapabilityHelpCatalog.ListCompositionSeeds()
            .Where(seed => string.Equals(seed.Kind, kind, StringComparison.OrdinalIgnoreCase))
            .Select(seed =>
            {
                var help = GeneratorPlanCapabilityHelpCatalog.Get(seed.Id);
                if (help.ImplementationStatus == "metadata_missing")
                {
                    help = new GeneratorPlanCapabilityHelpMetadata
                    {
                        Id = seed.Id,
                        DisplayNameRu = seed.DisplayNameRu,
                        DisplayNameEn = seed.DisplayNameRu,
                        ShortDescriptionRu = seed.ShortDescriptionRu,
                        DetailsRu = seed.ShortDescriptionRu,
                        ExamplesRu = seed.DisplayNameRu,
                        BestForRu = DomainFromId(seed.Id),
                        WarningsRu = "\u0414\u043e\u0431\u0430\u0432\u043b\u044f\u0435\u0442\u0441\u044f \u0432 capability selection \u0438 prompt context; \u043f\u043e\u043b\u043d\u0430\u044f \u0441\u0431\u043e\u0440\u043a\u0430 GamePackage \u043d\u0435 \u0432\u0445\u043e\u0434\u0438\u0442 \u0432 \u044d\u0442\u043e\u0442 slice.",
                        ImplementationStatus = "composer_seed"
                    };
                }

                return new CapabilityPickerComposableItemViewModel
                {
                    Id = seed.Id,
                    Kind = seed.Kind,
                    Domain = DomainFromId(seed.Id),
                    DisplayNameRu = seed.DisplayNameRu,
                    ShortDescriptionRu = seed.ShortDescriptionRu,
                    Help = help
                };
            })
            .OrderBy(item => item.Domain, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.DisplayNameRu, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<string> SelectDefaultFeatureBundles(
        IReadOnlyList<string> selectedIds,
        IReadOnlyList<CapabilityPickerFeatureBundleViewModel> featureBundles)
    {
        var selected = selectedIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (featureBundles.Any(bundle => bundle.IsRequiredTechnicalBase))
        {
            selected.Add(CoreAtlasPlanningFeatureBundleId);
        }

        return selected
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static GeneratorPlanCapabilityHelpMetadata HelpForOption(GeneratorPlanCapabilitySelectionAtlasOption option)
    {
        var help = GeneratorPlanCapabilityHelpCatalog.Get(option.Id);
        if (help.ImplementationStatus != "metadata_missing")
        {
            return help;
        }

        var title = string.IsNullOrWhiteSpace(option.Title) ? option.Id : option.Title;
        var purpose = string.IsNullOrWhiteSpace(option.Purpose) ? "-" : option.Purpose;
        var contractCount = option.RequiredArtifactContracts.Count;
        var validatorCount = option.RequiredValidators.Count;

        return new GeneratorPlanCapabilityHelpMetadata
        {
            Id = option.Id,
            DisplayNameRu = title,
            DisplayNameEn = title,
            ShortDescriptionRu = "\u0418\u0437\u0432\u0435\u0441\u0442\u043d\u043e \u0438\u0437 atlas: " + purpose,
            DetailsRu = "\u041f\u043e\u0434\u0440\u043e\u0431\u043d\u0430\u044f \u0440\u0443\u0441\u0441\u043a\u0430\u044f \u0441\u043f\u0440\u0430\u0432\u043a\u0430 \u043f\u043e\u043a\u0430 \u043d\u0435 \u043d\u0430\u043f\u0438\u0441\u0430\u043d\u0430. \u042d\u0442\u043e \u043f\u043e\u043b\u044c\u0437\u043e\u0432\u0430\u0442\u0435\u043b\u044c\u0441\u043a\u0438\u0439 \u0432\u044b\u0431\u043e\u0440 \u0434\u043b\u044f \u0444\u043e\u0440\u043c\u044b \u0438\u043b\u0438 \u0441\u0438\u0441\u0442\u0435\u043c \u0438\u0433\u0440\u044b.",
            ExamplesRu = title,
            BestForRu = purpose,
            WarningsRu = $"Atlas contracts: {contractCount}; validators: {validatorCount}.",
            ImplementationStatus = "atlas_fallback"
        };
    }

    private static GeneratorPlanCapabilityHelpMetadata HelpForBundle(GeneratorPlanCapabilitySelectionFeatureBundle bundle)
    {
        var help = GeneratorPlanCapabilityHelpCatalog.Get(bundle.Id);
        if (help.ImplementationStatus != "metadata_missing")
        {
            return help;
        }

        var title = string.IsNullOrWhiteSpace(bundle.Title) ? bundle.Id : bundle.Title;
        var purpose = string.IsNullOrWhiteSpace(bundle.Purpose) ? "-" : bundle.Purpose;
        var isTechnical = IsTechnicalBundle(bundle);
        var technicalHint = isTechnical
            ? "\u042d\u0442\u043e \u0442\u0435\u0445\u043d\u0438\u0447\u0435\u0441\u043a\u0430\u044f/\u0432\u043d\u0443\u0442\u0440\u0435\u043d\u043d\u044f\u044f \u043e\u043f\u0446\u0438\u044f \u0433\u0435\u043d\u0435\u0440\u0430\u0442\u043e\u0440\u0430, \u0430 \u043d\u0435 \u043e\u0431\u044b\u0447\u043d\u0430\u044f \u0438\u0433\u0440\u043e\u0432\u0430\u044f \u0444\u0438\u0447\u0430."
            : "\u042d\u0442\u043e \u043f\u043e\u043b\u044c\u0437\u043e\u0432\u0430\u0442\u0435\u043b\u044c\u0441\u043a\u0438\u0439 \u0432\u044b\u0431\u043e\u0440 \u0434\u043b\u044f \u0444\u043e\u0440\u043c\u044b \u0438\u043b\u0438 \u0441\u0438\u0441\u0442\u0435\u043c \u0438\u0433\u0440\u044b.";

        return new GeneratorPlanCapabilityHelpMetadata
        {
            Id = bundle.Id,
            DisplayNameRu = title,
            DisplayNameEn = title,
            ShortDescriptionRu = "\u0418\u0437\u0432\u0435\u0441\u0442\u043d\u043e \u0438\u0437 atlas: " + purpose,
            DetailsRu = "\u041f\u043e\u0434\u0440\u043e\u0431\u043d\u0430\u044f \u0440\u0443\u0441\u0441\u043a\u0430\u044f \u0441\u043f\u0440\u0430\u0432\u043a\u0430 \u043f\u043e\u043a\u0430 \u043d\u0435 \u043d\u0430\u043f\u0438\u0441\u0430\u043d\u0430. " + technicalHint,
            ExamplesRu = $"Domain: {bundle.Domain}; category: {bundle.Category}.",
            BestForRu = purpose,
            WarningsRu = $"Contracts: {bundle.ArtifactContracts.Count}; validators: {bundle.Validators.Count}; future gaps: {bundle.FutureModuleGaps.Count}.",
            ImplementationStatus = isTechnical ? "atlas_fallback_technical" : "atlas_fallback_user_facing"
        };
    }

    private static bool IsTechnicalBundle(GeneratorPlanCapabilitySelectionFeatureBundle bundle)
    {
        return IsCoreAtlasPlanning(bundle.Id) ||
               bundle.Domain.Contains("core", StringComparison.OrdinalIgnoreCase) ||
               bundle.Category.Contains("core", StringComparison.OrdinalIgnoreCase) ||
               bundle.Id.Contains("runtime_db", StringComparison.OrdinalIgnoreCase) ||
               bundle.Id.Contains("unity_ir", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCoreAtlasPlanning(string id)
    {
        return string.Equals(id, CoreAtlasPlanningFeatureBundleId, StringComparison.OrdinalIgnoreCase);
    }

    private static string DomainFromId(string id)
    {
        var parts = id.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length >= 2 ? parts[1] : string.Empty;
    }

    private static string FirstExisting(string selectedId, IReadOnlyList<CapabilityPickerOptionViewModel> options)
    {
        if (!string.IsNullOrWhiteSpace(selectedId) && options.Any(option => string.Equals(option.Id, selectedId, StringComparison.OrdinalIgnoreCase)))
        {
            return selectedId;
        }

        return options.FirstOrDefault()?.Id ?? string.Empty;
    }

    private static int SeverityOrder(string severity)
    {
        return severity switch
        {
            GeneratorPlanPreviewDiagnosticSeverity.Error => 0,
            GeneratorPlanPreviewDiagnosticSeverity.Warning => 1,
            GeneratorPlanPreviewDiagnosticSeverity.Info => 2,
            _ => 3
        };
    }
}
