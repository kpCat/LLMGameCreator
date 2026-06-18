using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;

namespace LLMGameCreator.WinForms.Pages.CapabilityPicker;

public sealed class CapabilityPickerPresenter
{
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
            FeatureBundles = atlas.FeatureBundles
                .Select(bundle => new CapabilityPickerFeatureBundleViewModel
                {
                    Id = bundle.Id,
                    Title = bundle.Title,
                    Domain = bundle.Domain,
                    Category = bundle.Category,
                    Purpose = bundle.Purpose,
                    ArtifactContractCount = bundle.ArtifactContracts.Count,
                    Help = GeneratorPlanCapabilityHelpCatalog.Get(bundle.Id)
                })
                .OrderBy(bundle => bundle.Title, StringComparer.OrdinalIgnoreCase)
                .ThenBy(bundle => bundle.Id, StringComparer.OrdinalIgnoreCase)
                .ToList(),
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
                Help = GeneratorPlanCapabilityHelpCatalog.Get(option.Id)
            })
            .OrderBy(option => option.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(option => option.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();
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
