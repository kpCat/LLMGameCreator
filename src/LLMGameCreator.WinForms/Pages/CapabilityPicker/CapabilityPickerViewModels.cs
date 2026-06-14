using LLMGameCreator.Application.Design.GeneratorPlans;

namespace LLMGameCreator.WinForms.Pages.CapabilityPicker;

public sealed record CapabilityPickerViewState
{
    public string AtlasRootPath { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public string PresentationModeId { get; init; } = string.Empty;
    public string WorldTopologyId { get; init; } = string.Empty;
    public string ActorModelId { get; init; } = string.Empty;
    public string InventoryModelId { get; init; } = string.Empty;
    public string CombatModelId { get; init; } = string.Empty;
    public string ProgressionModelId { get; init; } = string.Empty;
    public string PathfindingProfileId { get; init; } = string.Empty;
    public string NpcBehaviorModelId { get; init; } = string.Empty;
    public string RuntimeTargetId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedFeatureBundleIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<CapabilityPickerOptionViewModel> PresentationModes { get; init; } = Array.Empty<CapabilityPickerOptionViewModel>();
    public IReadOnlyList<CapabilityPickerOptionViewModel> WorldTopologies { get; init; } = Array.Empty<CapabilityPickerOptionViewModel>();
    public IReadOnlyList<CapabilityPickerOptionViewModel> ActorModels { get; init; } = Array.Empty<CapabilityPickerOptionViewModel>();
    public IReadOnlyList<CapabilityPickerOptionViewModel> InventoryModels { get; init; } = Array.Empty<CapabilityPickerOptionViewModel>();
    public IReadOnlyList<CapabilityPickerOptionViewModel> CombatModels { get; init; } = Array.Empty<CapabilityPickerOptionViewModel>();
    public IReadOnlyList<CapabilityPickerOptionViewModel> ProgressionModels { get; init; } = Array.Empty<CapabilityPickerOptionViewModel>();
    public IReadOnlyList<CapabilityPickerOptionViewModel> PathfindingProfiles { get; init; } = Array.Empty<CapabilityPickerOptionViewModel>();
    public IReadOnlyList<CapabilityPickerOptionViewModel> NpcBehaviorModels { get; init; } = Array.Empty<CapabilityPickerOptionViewModel>();
    public IReadOnlyList<CapabilityPickerOptionViewModel> RuntimeTargets { get; init; } = Array.Empty<CapabilityPickerOptionViewModel>();
    public IReadOnlyList<CapabilityPickerFeatureBundleViewModel> FeatureBundles { get; init; } = Array.Empty<CapabilityPickerFeatureBundleViewModel>();
    public string Status { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string SelectionJson { get; init; } = string.Empty;
    public IReadOnlyList<string> ResolvedArtifactContracts { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ResolvedValidators { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ResolvedRuntimeTargets { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ResolvedPromptContextTemplates { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CapabilityGaps { get; init; } = Array.Empty<string>();
    public IReadOnlyList<CapabilityPickerDiagnosticRow> Diagnostics { get; init; } = Array.Empty<CapabilityPickerDiagnosticRow>();
    public GeneratorPlanCapabilitySelectionResult? CurrentResult { get; init; }
    public bool CanSave => CurrentResult != null && CurrentResult.Diagnostics.All(diagnostic => diagnostic.Severity != GeneratorPlanPreviewDiagnosticSeverity.Error);
}

public sealed record CapabilityPickerOptionViewModel
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public string DisplayName => string.IsNullOrWhiteSpace(Title) ? Id : $"{Title} ({Id})";
}

public sealed record CapabilityPickerFeatureBundleViewModel
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public int ArtifactContractCount { get; init; }
    public string DisplayName => $"{Title} | {Id} | {Domain} | {Category} | contracts: {ArtifactContractCount} | {Purpose}";
}

public sealed record CapabilityPickerDiagnosticRow
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static CapabilityPickerDiagnosticRow FromDiagnostic(GeneratorPlanCapabilitySelectionDiagnostic diagnostic)
    {
        return new CapabilityPickerDiagnosticRow
        {
            Severity = diagnostic.Severity,
            Code = diagnostic.Code,
            Target = diagnostic.Target,
            Message = diagnostic.Message
        };
    }
}
