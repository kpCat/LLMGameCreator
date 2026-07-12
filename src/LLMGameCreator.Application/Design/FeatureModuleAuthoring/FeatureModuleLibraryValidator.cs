using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed class FeatureModuleLibraryValidator
{
    public FeatureModuleLibraryValidationResult Validate(
        FeatureModuleLibraryManifest manifest,
        IReadOnlyList<(string RelativePath, FeatureModuleDefinition Module)> loaded,
        IReadOnlyList<string> discoveredFiles,
        bool pathsConfined)
    {
        var diagnostics = new List<string>();
        var manifestValid = manifest.SchemaVersion == FeatureModuleLibraryVocabulary.ManifestSchemaVersion;
        if (!manifestValid) diagnostics.Add("unsupported library manifest schema version rejected: " + manifest.SchemaVersion);
        var fileReferencesUnique = manifest.ModuleFiles.Distinct(PathComparer()).Count() == manifest.ModuleFiles.Count;
        if (!fileReferencesUnique) diagnostics.Add("duplicate module file reference rejected");
        if (!pathsConfined) diagnostics.Add("module file path escape rejected");

        var manifestSet = manifest.ModuleFiles.Select(NormalizePath).ToHashSet(PathComparer());
        var discoveredSet = discoveredFiles.Select(NormalizePath).ToHashSet(PathComparer());
        if (!manifestSet.SetEquals(discoveredSet)) diagnostics.Add("manifest and discovered module files mismatch");
        var requiredCount = loaded.Count(item => item.Module.Required);
        var optionalCount = loaded.Count(item => item.Module.Selectable && !item.Module.Required);
        var countsMatch = manifest.ModuleFileCount == loaded.Count
                          && manifest.RequiredCoreModuleCount == requiredCount
                          && manifest.OptionalModuleCount == optionalCount;
        if (!countsMatch) diagnostics.Add("module library counts mismatch");
        var moduleIdsUnique = loaded.Select(item => item.Module.ModuleId).Distinct(StringComparer.Ordinal).Count() == loaded.Count;
        if (!moduleIdsUnique) diagnostics.Add("duplicate module ID rejected");

        var byId = loaded.GroupBy(item => item.Module.ModuleId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1).ToDictionary(group => group.Key, group => group.Single().Module, StringComparer.Ordinal);
        var referencesValid = true;
        foreach (var item in loaded.OrderBy(item => item.Module.ModuleId, StringComparer.Ordinal))
        {
            var module = item.Module;
            if (module.SchemaVersion != FeatureModuleLibraryVocabulary.ModuleSchemaVersion)
            {
                diagnostics.Add("unsupported module schema rejected: " + module.ModuleId + ":" + module.SchemaVersion);
                referencesValid = false;
            }
            if (string.IsNullOrWhiteSpace(module.ModuleId) || string.IsNullOrWhiteSpace(module.Title)
                || string.IsNullOrWhiteSpace(module.Category) || string.IsNullOrWhiteSpace(module.ModuleKind)
                || string.IsNullOrWhiteSpace(module.ModuleVersion))
            {
                diagnostics.Add("required module metadata missing: " + item.RelativePath);
                referencesValid = false;
            }
            foreach (var dependency in module.Dependencies.Where(dependency => !byId.ContainsKey(dependency)))
            {
                diagnostics.Add("unknown dependency rejected: " + module.ModuleId + "->" + dependency);
                referencesValid = false;
            }
            foreach (var conflict in module.Conflicts)
            {
                if (!byId.TryGetValue(conflict, out var other))
                {
                    diagnostics.Add("unknown conflict rejected: " + module.ModuleId + "->" + conflict);
                    referencesValid = false;
                }
                else if (!other.Conflicts.Contains(module.ModuleId, StringComparer.Ordinal))
                {
                    diagnostics.Add("conflict reference mismatch rejected: " + module.ModuleId + "<->" + conflict);
                    referencesValid = false;
                }
            }
            referencesValid &= ValidateOwnedReferences(module, diagnostics);
        }

        var result = new FeatureModuleLibraryValidationResult
        {
            ManifestValidated = manifestValid,
            CountsMatch = countsMatch && manifestSet.SetEquals(discoveredSet),
            ModuleIdsUnique = moduleIdsUnique,
            FileReferencesUnique = fileReferencesUnique,
            PathsConfined = pathsConfined,
            DependenciesAndConflictsValidated = referencesValid && diagnostics.All(value =>
                !value.Contains("dependency", StringComparison.Ordinal) && !value.Contains("conflict", StringComparison.Ordinal)),
            OperationEffectParameterReferencesValidated = referencesValid && diagnostics.All(value =>
                !value.Contains("operation", StringComparison.Ordinal) && !value.Contains("effect", StringComparison.Ordinal)
                && !value.Contains("parameter", StringComparison.Ordinal)),
            Diagnostics = diagnostics
        };
        return result with { Passed = result.ManifestValidated && result.CountsMatch && result.ModuleIdsUnique
            && result.FileReferencesUnique && result.PathsConfined && result.DependenciesAndConflictsValidated
            && result.OperationEffectParameterReferencesValidated && diagnostics.Count == 0 };
    }

    private static bool ValidateOwnedReferences(FeatureModuleDefinition module, List<string> diagnostics)
    {
        var valid = true;
        var operationIds = module.MutationOperations.Select(item => item.OperationId).ToList();
        if (operationIds.Any(string.IsNullOrWhiteSpace) || operationIds.Distinct(StringComparer.Ordinal).Count() != operationIds.Count)
        {
            diagnostics.Add("operation reference mismatch rejected: " + module.ModuleId);
            valid = false;
        }
        var operationSet = operationIds.ToHashSet(StringComparer.Ordinal);
        var effects = module.RuntimeEffectContracts.ToDictionary(item => item.EffectId, StringComparer.Ordinal);
        foreach (var effect in module.RuntimeEffectContracts)
        {
            if (effect.ModuleId != module.ModuleId || effect.SourceOperationIds.Count == 0
                || effect.SourceOperationIds.Any(id => !operationSet.Contains(id)))
            {
                diagnostics.Add("effect operation reference mismatch rejected: " + effect.EffectId);
                valid = false;
            }
        }
        var parameterIds = module.ParameterDefinitions.Select(item => item.ParameterId).ToList();
        if (parameterIds.Distinct(StringComparer.Ordinal).Count() != parameterIds.Count)
        {
            diagnostics.Add("duplicate parameter rejected: " + module.ModuleId);
            valid = false;
        }
        var boundTargets = new HashSet<string>(StringComparer.Ordinal);
        foreach (var parameter in module.ParameterDefinitions)
        {
            if (parameter.ModuleId != module.ModuleId || string.IsNullOrWhiteSpace(parameter.ParameterId)
                || !SupportedType(parameter.ValueType) || !SupportedControl(parameter.AuthoringControl))
            {
                diagnostics.Add("parameter contract mismatch rejected: " + module.ModuleId + ":" + parameter.ParameterId);
                valid = false;
            }
            foreach (var binding in parameter.Bindings)
            {
                var target = binding.OperationId + "|" + binding.OperationField;
                if (!operationSet.Contains(binding.OperationId) || binding.OperationField != "newValue"
                    || binding.TransformKind != "identity" || !boundTargets.Add(target)
                    || (!string.IsNullOrWhiteSpace(parameter.AtomicGroupId)
                        && binding.AtomicGroupId != parameter.AtomicGroupId))
                {
                    diagnostics.Add("conflicting parameter binding rejected: " + module.ModuleId + ":" + target);
                    valid = false;
                }
            }
            if (parameter.RuntimeEffectIds.Any(id => !effects.ContainsKey(id)))
            {
                diagnostics.Add("parameter effect reference mismatch rejected: " + module.ModuleId + ":" + parameter.ParameterId);
                valid = false;
            }
        }
        var effectiveTargets = new HashSet<string>(StringComparer.Ordinal);
        var bindingIds = new HashSet<string>(StringComparer.Ordinal);
        var actions = module.RuntimePlaythroughContracts.ToDictionary(item => item.ActionId, StringComparer.Ordinal);
        foreach (var binding in module.EffectiveValueBindings)
        {
            var target = binding.TargetKind + "|" + binding.TargetId + "|" + binding.TargetField;
            var targetValid = binding.TargetKind switch
            {
                FeatureModuleEffectiveValueBindingTargetKinds.MutationOperationField =>
                    binding.TargetField == "newValue" && operationSet.Contains(binding.TargetId),
                FeatureModuleEffectiveValueBindingTargetKinds.RuntimeEffectExpectedValue =>
                    binding.TargetField == "expectedValue" && effects.ContainsKey(binding.TargetId),
                FeatureModuleEffectiveValueBindingTargetKinds.RuntimePlaythroughArg =>
                    actions.TryGetValue(binding.TargetId, out var action) && action.Args.ContainsKey(binding.TargetField),
                _ => false
            };
            if (string.IsNullOrWhiteSpace(binding.BindingId) || !bindingIds.Add(binding.BindingId)
                || string.IsNullOrWhiteSpace(binding.ValueExpression) || !targetValid || !effectiveTargets.Add(target))
            {
                diagnostics.Add("effective binding contract mismatch rejected: " + module.ModuleId + ":" + target);
                valid = false;
            }
        }
        return valid;
    }

    private static bool SupportedType(string value) => value is FeatureModuleParameterValueTypes.Integer
        or FeatureModuleParameterValueTypes.Number or FeatureModuleParameterValueTypes.Boolean
        or FeatureModuleParameterValueTypes.Enum;

    private static bool SupportedControl(string value) => value is FeatureModuleAuthoringControls.NumericUpDown
        or FeatureModuleAuthoringControls.CheckBox or FeatureModuleAuthoringControls.ComboBox;

    private static StringComparer PathComparer() => OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    private static string NormalizePath(string value) => value.Replace('\\', '/').TrimStart('/');
}
