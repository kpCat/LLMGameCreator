namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public static class FeatureModuleLegacyProfileLibraryView
{
    public static FeatureModuleLibrarySnapshot Create(FeatureModuleLibrarySnapshot loaded)
    {
        var visibleModules = loaded.Catalog.Modules.Where(module => module.Required || module.DefaultSelected).ToList();
        var visibleIds = visibleModules.Select(module => module.ModuleId).ToHashSet(StringComparer.Ordinal);
        var fingerprints = loaded.ModuleFingerprints.Where(pair => visibleIds.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var catalogFingerprint = new FeatureModuleLibraryFingerprintService().CatalogFingerprint(fingerprints);
        var visibleIndex = loaded.Index.Modules.Where(module => visibleIds.Contains(module.ModuleId)).ToList();
        return loaded with
        {
            Manifest = loaded.Manifest with
            {
                OptionalModuleCount = visibleModules.Count(module => module.Selectable && !module.Required),
                ModuleFileCount = visibleModules.Count,
                ModuleFiles = loaded.Manifest.ModuleFiles.Where(path =>
                    visibleIndex.Any(index => index.RelativePath == path)).ToList()
            },
            Catalog = loaded.Catalog with
            {
                OptionalProfileModuleCount = visibleModules.Count(module => module.Selectable && !module.Required),
                Modules = visibleModules
            },
            ModuleFingerprints = fingerprints,
            CatalogFingerprint = catalogFingerprint,
            Index = loaded.Index with
            {
                CatalogFingerprint = catalogFingerprint,
                OptionalModuleCount = visibleModules.Count(module => module.Selectable && !module.Required),
                ParameterDefinitionCount = visibleModules.Sum(module => module.ParameterDefinitions.Count),
                Modules = visibleIndex
            }
        };
    }
}
