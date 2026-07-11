using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public static class FeatureModuleLibraryVocabulary
{
    public const string ManifestSchemaVersion = "featuremodule_library_manifest_v1";
    public const string ModuleSchemaVersion = "featuremodule_definition_v1";
    public const string DefaultRelativeRoot = "catalogs/feature-modules";
    public const string ManifestFileName = "catalog.json";
}

public sealed record FeatureModuleLibraryManifest
{
    public string SchemaVersion { get; init; } = FeatureModuleLibraryVocabulary.ManifestSchemaVersion;
    public string CatalogId { get; init; } = "llmgc.feature-modules";
    public string CatalogVersion { get; init; } = "1.0.0";
    public int RequiredCoreModuleCount { get; init; }
    public int OptionalModuleCount { get; init; }
    public int ModuleFileCount { get; init; }
    public IReadOnlyList<string> ModuleFiles { get; init; } = [];
}

public sealed record FeatureModuleLibraryIndexEntry
{
    public string ModuleId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string ModuleFingerprint { get; init; } = string.Empty;
    public bool Required { get; init; }
    public bool Selectable { get; init; }
    public int ParameterDefinitionCount { get; init; }
}

public sealed record FeatureModuleLibraryIndex
{
    public string SchemaVersion { get; init; } = "featuremodule_library_index_v1";
    public string CatalogId { get; init; } = string.Empty;
    public string CatalogVersion { get; init; } = string.Empty;
    public string CatalogFingerprint { get; init; } = string.Empty;
    public int RequiredCoreModuleCount { get; init; }
    public int OptionalModuleCount { get; init; }
    public int ParameterDefinitionCount { get; init; }
    public IReadOnlyList<FeatureModuleLibraryIndexEntry> Modules { get; init; } = [];
}

public sealed record FeatureModuleLibraryValidationResult
{
    public string SchemaVersion { get; init; } = "featuremodule_library_validation_v1";
    public bool Passed { get; init; }
    public bool ManifestValidated { get; init; }
    public bool CountsMatch { get; init; }
    public bool ModuleIdsUnique { get; init; }
    public bool FileReferencesUnique { get; init; }
    public bool PathsConfined { get; init; }
    public bool DependenciesAndConflictsValidated { get; init; }
    public bool OperationEffectParameterReferencesValidated { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record FeatureModuleLibrarySnapshot
{
    public string LibraryRoot { get; init; } = string.Empty;
    public FeatureModuleLibraryManifest Manifest { get; init; } = new();
    public FeatureModuleCatalogDocument Catalog { get; init; } = new();
    public IReadOnlyDictionary<string, string> ModuleFingerprints { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
    public string CatalogFingerprint { get; init; } = string.Empty;
    public FeatureModuleLibraryIndex Index { get; init; } = new();
    public FeatureModuleLibraryValidationResult Validation { get; init; } = new();
}

public sealed class FeatureModuleLibraryException : InvalidOperationException
{
    public FeatureModuleLibraryException(string message) : base(message) { }
    public FeatureModuleLibraryException(string message, Exception innerException) : base(message, innerException) { }
}
