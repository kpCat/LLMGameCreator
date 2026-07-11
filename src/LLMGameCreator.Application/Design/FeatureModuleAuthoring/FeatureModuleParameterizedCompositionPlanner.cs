using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed record FeatureModuleParameterizedCompositionPlan
{
    public FeatureModuleCompositionPlan CompositionPlan { get; init; } = new();
    public FeatureModuleParameterBindingResult ParameterBinding { get; init; } = new();
}

public sealed class FeatureModuleParameterizedCompositionPlanner
{
    private readonly FeatureModuleCompositionValidator _compositionValidator;
    private readonly FeatureModuleParameterBindingService _bindingService;

    public FeatureModuleParameterizedCompositionPlanner(
        FeatureModuleCompositionValidator? compositionValidator = null,
        FeatureModuleParameterBindingService? bindingService = null)
    {
        _compositionValidator = compositionValidator ?? new FeatureModuleCompositionValidator();
        _bindingService = bindingService ?? new FeatureModuleParameterBindingService();
    }

    public FeatureModuleParameterizedCompositionPlan Plan(
        FeatureModuleCatalogDocument catalog,
        string compositionId,
        IReadOnlyList<string> selectedOptionalModuleIds,
        IReadOnlyList<FeatureModuleParameterValue> parameterValues,
        string basePackagePath,
        string basePackageSha256)
    {
        var required = catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId)
            .OrderBy(id => id, StringComparer.Ordinal).ToList();
        var optional = selectedOptionalModuleIds.OrderBy(id => id, StringComparer.Ordinal).ToList();
        var all = required.Concat(optional).ToList();
        var validation = _compositionValidator.Validate(catalog, all);
        if (!validation.Passed)
            throw new InvalidOperationException("Parameterized FeatureModule composition validation failed: "
                                                + string.Join("; ", validation.Diagnostics));
        var binding = _bindingService.Bind(catalog, optional, parameterValues);
        if (!binding.Passed)
            throw new InvalidOperationException("FeatureModule parameter binding failed: " + string.Join("; ", binding.Diagnostics));
        return new FeatureModuleParameterizedCompositionPlan
        {
            CompositionPlan = new FeatureModuleCompositionPlan
            {
                CompositionId = compositionId,
                BasePackagePath = basePackagePath,
                BasePackageSha256 = basePackageSha256,
                RequiredModuleIds = required,
                SelectedOptionalModuleIds = optional,
                OrderedModuleIds = all.OrderBy(id => id, StringComparer.Ordinal).ToList(),
                OrderedMutationOperations = binding.EffectiveMutationOperations,
                DependencyValidationPassed = true,
                OrderIndependencePassed = true,
                SourceTemplateUnmodified = true,
                Validation = validation
            },
            ParameterBinding = binding
        };
    }
}
