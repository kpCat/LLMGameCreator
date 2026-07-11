using LLMGameCreator.Application.Design.FeatureModuleAuthoring;

namespace LLMGameCreator.Application.Design.FeatureModuleCertification;

public sealed class FeatureModuleCertificationFingerprintService
{
    private readonly FeatureModuleLibraryFingerprintService _libraryFingerprints = new();

    public string DependencyClosureFingerprint(
        FeatureModuleLibrarySnapshot library,
        IReadOnlyList<string> dependencyClosureIds)
    {
        var canonical = string.Join("\n", dependencyClosureIds.OrderBy(id => id, StringComparer.Ordinal)
            .Select(id => id + ":" + (library.ModuleFingerprints.TryGetValue(id, out var value)
                ? value : throw new InvalidOperationException("unknown certification dependency: " + id)))) + "\n";
        return FeatureModuleLibraryFingerprintService.Hash(canonical);
    }

    public string ParameterDefaultsFingerprint(FeatureModuleLibrarySnapshot library, string moduleId) =>
        _libraryFingerprints.ParameterDefaultsFingerprint(
            library.Catalog.Modules.Single(item => item.ModuleId == moduleId));

    public string CacheKey(FeatureModuleCertificationPlanItem item) => FeatureModuleLibraryFingerprintService.Hash(string.Join("\n",
    [
        item.ModuleId,
        string.Join(",", item.CertificationSelectedModuleIds),
        string.Join(",", item.OptionalDependencyClosureIds),
        item.DependencyClosureFingerprint,
        item.ModuleFingerprint,
        item.DependencyFingerprint,
        item.BasePackageSha256,
        item.RuntimeQualifierContractVersion,
        item.ActionPlanSignature,
        item.ParameterDefaultsFingerprint
    ]) + "\n");
}

public sealed class FeatureModuleCertificationPlanner
{
    private readonly FeatureModuleCertificationFingerprintService _fingerprints = new();

    public FeatureModuleCertificationPlan Plan(
        FeatureModuleLibrarySnapshot library,
        string basePackageSha256,
        string runtimeQualifierContractVersion,
        string actionPlanSignature)
    {
        var items = new List<FeatureModuleCertificationPlanItem>();
        foreach (var module in library.Catalog.Modules.Where(module => module.Selectable && !module.Required)
                     .OrderBy(module => module.ModuleId, StringComparer.Ordinal))
        {
            var closure = ResolveDependencyClosure(library, module.ModuleId);
            var optionalClosure = closure.Where(id =>
                    !library.Catalog.Modules.Single(candidate => candidate.ModuleId == id).Required)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
            var certificationSelectedModuleIds = optionalClosure.Append(module.ModuleId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
            var dependencyClosureFingerprint = _fingerprints.DependencyClosureFingerprint(library, closure);
            var item = new FeatureModuleCertificationPlanItem
            {
                ModuleId = module.ModuleId,
                CertificationSelectedModuleIds = certificationSelectedModuleIds,
                OptionalDependencyClosureIds = optionalClosure,
                DependencyClosureFingerprint = dependencyClosureFingerprint,
                ModuleFingerprint = library.ModuleFingerprints[module.ModuleId],
                DependencyFingerprint = dependencyClosureFingerprint,
                BasePackageSha256 = basePackageSha256,
                RuntimeQualifierContractVersion = runtimeQualifierContractVersion,
                ActionPlanSignature = actionPlanSignature,
                ParameterDefaultsFingerprint = _fingerprints.ParameterDefaultsFingerprint(library, module.ModuleId)
            };
            items.Add(item with { CacheKey = _fingerprints.CacheKey(item) });
        }
        return new FeatureModuleCertificationPlan { ModuleCount = items.Count, Modules = items };
    }

    private static IReadOnlyList<string> ResolveDependencyClosure(
        FeatureModuleLibrarySnapshot library,
        string moduleId)
    {
        var modules = library.Catalog.Modules.ToDictionary(module => module.ModuleId, StringComparer.Ordinal);
        if (!modules.ContainsKey(moduleId))
            throw new InvalidOperationException("unknown certification module: " + moduleId);

        var resolved = new HashSet<string>(StringComparer.Ordinal);
        var visiting = new List<string>();
        Visit(moduleId);
        resolved.Remove(moduleId);
        return resolved.OrderBy(id => id, StringComparer.Ordinal).ToList();

        void Visit(string currentId)
        {
            var cycleIndex = visiting.FindIndex(id => string.Equals(id, currentId, StringComparison.Ordinal));
            if (cycleIndex >= 0)
            {
                var cycle = visiting.Skip(cycleIndex).Append(currentId);
                throw new InvalidOperationException("certification dependency cycle rejected: " + string.Join(" -> ", cycle));
            }
            if (resolved.Contains(currentId)) return;
            if (!modules.TryGetValue(currentId, out var current))
                throw new InvalidOperationException("unknown certification dependency: " + currentId);

            visiting.Add(currentId);
            foreach (var dependencyId in current.Dependencies.OrderBy(id => id, StringComparer.Ordinal))
            {
                if (!modules.ContainsKey(dependencyId))
                    throw new InvalidOperationException("unknown certification dependency: " + currentId + " -> " + dependencyId);
                Visit(dependencyId);
                resolved.Add(dependencyId);
            }
            visiting.RemoveAt(visiting.Count - 1);
            resolved.Add(currentId);
        }
    }
}
