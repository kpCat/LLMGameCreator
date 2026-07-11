using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public sealed class FeatureModuleCompositionPlanner
{
    private readonly FeatureModuleCompositionValidator _validator;

    public FeatureModuleCompositionPlanner(FeatureModuleCompositionValidator? validator = null)
    {
        _validator = validator ?? new FeatureModuleCompositionValidator();
    }

    public FeatureModuleCompositionPlan Plan(
        FeatureModuleCatalogDocument catalog,
        FeatureModuleCompositionRequest request,
        string basePackagePath,
        string basePackageSha256,
        bool sourceTemplateUnmodified)
    {
        var required = catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId)
            .OrderBy(id => id, StringComparer.Ordinal).ToList();
        var optional = request.SelectedModuleIds.OrderBy(id => id, StringComparer.Ordinal).ToList();
        var all = required.Concat(optional).ToList();
        var validation = _validator.Validate(catalog, all, request.ParameterOverrides);
        if (!validation.Passed)
        {
            throw new InvalidOperationException("FeatureModule composition validation failed: " + string.Join("; ", validation.Diagnostics));
        }

        var byId = catalog.Modules.ToDictionary(module => module.ModuleId, StringComparer.Ordinal);
        var operations = all.SelectMany(moduleId => byId[moduleId].MutationOperations)
            .OrderBy(FeatureModuleCompositionValidator.TargetKey, StringComparer.Ordinal)
            .ThenBy(operation => operation.OperationId, StringComparer.Ordinal)
            .ToList();
        var orderedOperations = operations.GroupBy(FeatureModuleCompositionValidator.TargetKey, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(FeatureModuleCompositionValidator.TargetKey, StringComparer.Ordinal)
            .ThenBy(operation => operation.OperationId, StringComparer.Ordinal)
            .ToList();
        return new FeatureModuleCompositionPlan
        {
            CompositionId = request.CompositionId,
            BaseCandidateId = request.BaseCandidateId,
            BasePackagePath = basePackagePath,
            BasePackageSha256 = basePackageSha256,
            RequiredModuleIds = required,
            SelectedOptionalModuleIds = optional,
            OrderedModuleIds = all.OrderBy(id => id, StringComparer.Ordinal).ToList(),
            OrderedMutationOperations = orderedOperations,
            DeduplicatedOperationCount = operations.Count - orderedOperations.Count,
            ConflictCount = 0,
            DependencyValidationPassed = true,
            OrderIndependencePassed = true,
            SourceTemplateUnmodified = sourceTemplateUnmodified,
            Validation = validation
        };
    }
}
