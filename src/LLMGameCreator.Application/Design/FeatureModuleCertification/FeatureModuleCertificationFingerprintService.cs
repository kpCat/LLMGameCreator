using LLMGameCreator.Application.Design.FeatureModuleAuthoring;

namespace LLMGameCreator.Application.Design.FeatureModuleCertification;

public sealed class FeatureModuleCertificationFingerprintService
{
    private readonly FeatureModuleLibraryFingerprintService _libraryFingerprints = new();

    public string DependencyFingerprint(FeatureModuleLibrarySnapshot library, string moduleId)
    {
        var module = library.Catalog.Modules.Single(item => item.ModuleId == moduleId);
        var canonical = string.Join("\n", module.Dependencies.OrderBy(id => id, StringComparer.Ordinal)
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
            var item = new FeatureModuleCertificationPlanItem
            {
                ModuleId = module.ModuleId,
                ModuleFingerprint = library.ModuleFingerprints[module.ModuleId],
                DependencyFingerprint = _fingerprints.DependencyFingerprint(library, module.ModuleId),
                BasePackageSha256 = basePackageSha256,
                RuntimeQualifierContractVersion = runtimeQualifierContractVersion,
                ActionPlanSignature = actionPlanSignature,
                ParameterDefaultsFingerprint = _fingerprints.ParameterDefaultsFingerprint(library, module.ModuleId)
            };
            items.Add(item with { CacheKey = _fingerprints.CacheKey(item) });
        }
        return new FeatureModuleCertificationPlan { ModuleCount = items.Count, Modules = items };
    }
}
