using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public sealed class FeatureModuleCompositionValidator
{
    public FeatureModuleCompositionValidation Validate(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<string> selectedModuleIds,
        IReadOnlyDictionary<string, string>? parameterOverrides = null)
    {
        var diagnostics = new List<string>();
        var known = catalog.Modules.ToDictionary(module => module.ModuleId, StringComparer.Ordinal);
        var unique = selectedModuleIds.Distinct(StringComparer.Ordinal).ToList();
        var moduleIdsUnique = unique.Count == selectedModuleIds.Count;
        if (!moduleIdsUnique) diagnostics.Add("duplicate module ID rejected");

        var unknown = unique.Where(id => !known.ContainsKey(id)).OrderBy(id => id, StringComparer.Ordinal).ToList();
        if (unknown.Count > 0) diagnostics.Add("unknown module rejected: " + string.Join(", ", unknown));
        var selected = unique.Where(known.ContainsKey).Select(id => known[id]).ToList();
        var selectedSet = unique.ToHashSet(StringComparer.Ordinal);
        var missingRequired = catalog.Modules.Where(module => module.Required && !selectedSet.Contains(module.ModuleId))
            .Select(module => module.ModuleId).OrderBy(id => id, StringComparer.Ordinal).ToList();
        if (missingRequired.Count > 0) diagnostics.Add("required module deselection rejected: " + string.Join(", ", missingRequired));

        var missingDependencies = selected.SelectMany(module => module.Dependencies
                .Where(dependency => !selectedSet.Contains(dependency))
                .Select(dependency => module.ModuleId + "->" + dependency))
            .OrderBy(value => value, StringComparer.Ordinal).ToList();
        if (missingDependencies.Count > 0) diagnostics.Add("missing dependency rejected: " + string.Join(", ", missingDependencies));

        var conflicts = selected.SelectMany(module => module.Conflicts
                .Where(selectedSet.Contains)
                .Select(conflict => module.ModuleId + "<->" + conflict))
            .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
        if (conflicts.Count > 0) diagnostics.Add("declared conflict rejected: " + string.Join(", ", conflicts));

        var operations = selected.SelectMany(module => module.MutationOperations.Select(operation => (module.ModuleId, Operation: operation))).ToList();
        var duplicateOperationIds = operations.GroupBy(item => item.Operation.OperationId, StringComparer.Ordinal)
            .Where(group => group.Count() > 1).Select(group => group.Key).OrderBy(id => id, StringComparer.Ordinal).ToList();
        if (duplicateOperationIds.Count > 0) diagnostics.Add("duplicate operation ID rejected: " + string.Join(", ", duplicateOperationIds));

        var targetConflicts = new List<string>();
        foreach (var group in operations.GroupBy(item => TargetKey(item.Operation), StringComparer.Ordinal))
        {
            var signatures = group.Select(item => OperationSignature(item.Operation)).Distinct(StringComparer.Ordinal).ToList();
            if (signatures.Count > 1)
            {
                targetConflicts.Add(group.Key + " modules=" + string.Join(",", group.Select(item => item.ModuleId).Distinct(StringComparer.Ordinal)));
            }
        }
        if (targetConflicts.Count > 0) diagnostics.Add("conflicting mutation target rejected: " + string.Join("; ", targetConflicts));

        var overridesSupported = parameterOverrides is null || parameterOverrides.Count == 0;
        if (!overridesSupported) diagnostics.Add("unsupported parameter override rejected: " + string.Join(", ", parameterOverrides!.Keys));

        return new FeatureModuleCompositionValidation
        {
            AllModuleIdsExist = unknown.Count == 0,
            RequiredModulesSelected = missingRequired.Count == 0,
            DependenciesSatisfied = missingDependencies.Count == 0,
            ConflictsAbsent = conflicts.Count == 0,
            ModuleIdsUnique = moduleIdsUnique,
            OperationIdsUnique = duplicateOperationIds.Count == 0,
            MutationTargetsUniqueOrIdentical = targetConflicts.Count == 0,
            ParameterOverridesSupported = overridesSupported,
            Passed = diagnostics.Count == 0,
            Diagnostics = diagnostics
        };
    }

    public static string TargetKey(ProductLineRuntimeVariantMutationOperation operation) =>
        operation.TargetKind + "|" + operation.TargetId + "|" + operation.JsonPath;

    public static string OperationSignature(ProductLineRuntimeVariantMutationOperation operation) =>
        operation.ExpectedValue + "|" + operation.NewValue + "|" + operation.RuntimeDimension;
}
