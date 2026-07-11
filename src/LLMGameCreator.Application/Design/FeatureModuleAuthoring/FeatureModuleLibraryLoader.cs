using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed class FeatureModuleLibraryLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly FeatureModuleLibraryValidator _validator;
    private readonly FeatureModuleLibraryFingerprintService _fingerprints;

    public FeatureModuleLibraryLoader(
        FeatureModuleLibraryValidator? validator = null,
        FeatureModuleLibraryFingerprintService? fingerprints = null)
    {
        _validator = validator ?? new FeatureModuleLibraryValidator();
        _fingerprints = fingerprints ?? new FeatureModuleLibraryFingerprintService();
    }

    public FeatureModuleLibrarySnapshot Load(string libraryRoot)
    {
        if (string.IsNullOrWhiteSpace(libraryRoot)) throw new FeatureModuleLibraryException("module library root is required");
        var root = Path.GetFullPath(libraryRoot);
        var manifestPath = Path.Combine(root, FeatureModuleLibraryVocabulary.ManifestFileName);
        if (!File.Exists(manifestPath)) throw new FeatureModuleLibraryException("module library manifest was not found: " + manifestPath);
        var manifest = Read<FeatureModuleLibraryManifest>(manifestPath, "malformed module library manifest rejected");
        var discovered = Directory.EnumerateFiles(root, "*.featuremodule.json", SearchOption.AllDirectories)
            .Select(path => Relative(root, path)).OrderBy(path => path, StringComparer.Ordinal).ToList();
        var loaded = new List<(string RelativePath, FeatureModuleDefinition Module)>();
        var pathsConfined = true;
        foreach (var relative in manifest.ModuleFiles)
        {
            var path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
            if (!IsUnder(path, root))
            {
                pathsConfined = false;
                continue;
            }
            if (!File.Exists(path)) throw new FeatureModuleLibraryException("module file was not found: " + relative);
            loaded.Add((Relative(root, path), Read<FeatureModuleDefinition>(path, "malformed module JSON rejected: " + relative)));
        }
        var validation = _validator.Validate(manifest, loaded, discovered, pathsConfined);
        if (!validation.Passed)
            throw new FeatureModuleLibraryException("FeatureModule library validation failed: " + string.Join("; ", validation.Diagnostics));
        var modules = loaded.Select(item => FeatureModuleLibraryFingerprintService.Normalize(item.Module))
            .OrderBy(module => module.ModuleId, StringComparer.Ordinal).ToList();
        var moduleFingerprints = modules.ToDictionary(
            module => module.ModuleId,
            _fingerprints.ModuleFingerprint,
            StringComparer.Ordinal);
        var catalogFingerprint = _fingerprints.CatalogFingerprint(moduleFingerprints);
        var byIdPath = loaded.ToDictionary(item => item.Module.ModuleId, item => item.RelativePath, StringComparer.Ordinal);
        var catalog = new FeatureModuleCatalogDocument
        {
            RequiredCoreModuleCount = manifest.RequiredCoreModuleCount,
            OptionalProfileModuleCount = manifest.OptionalModuleCount,
            Modules = modules
        };
        var index = new FeatureModuleLibraryIndex
        {
            CatalogId = manifest.CatalogId,
            CatalogVersion = manifest.CatalogVersion,
            CatalogFingerprint = catalogFingerprint,
            RequiredCoreModuleCount = manifest.RequiredCoreModuleCount,
            OptionalModuleCount = manifest.OptionalModuleCount,
            ParameterDefinitionCount = modules.Sum(module => module.ParameterDefinitions.Count),
            Modules = modules.Select(module => new FeatureModuleLibraryIndexEntry
            {
                ModuleId = module.ModuleId,
                RelativePath = byIdPath[module.ModuleId],
                ModuleFingerprint = moduleFingerprints[module.ModuleId],
                Required = module.Required,
                Selectable = module.Selectable,
                ParameterDefinitionCount = module.ParameterDefinitions.Count
            }).ToList()
        };
        return new FeatureModuleLibrarySnapshot
        {
            LibraryRoot = root,
            Manifest = manifest,
            Catalog = catalog,
            ModuleFingerprints = moduleFingerprints,
            CatalogFingerprint = catalogFingerprint,
            Index = index,
            Validation = validation
        };
    }

    public static string SerializeCanonical<T>(T value) => JsonSerializer.Serialize(value, JsonOptions) + "\n";

    private static T Read<T>(string path, string message)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
                   ?? throw new FeatureModuleLibraryException(message);
        }
        catch (JsonException exception)
        {
            throw new FeatureModuleLibraryException(message, exception);
        }
    }

    private static bool IsUnder(string path, string directory)
    {
        var full = Path.GetFullPath(path);
        var root = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return full.Equals(root, comparison) || full.StartsWith(root + Path.DirectorySeparatorChar, comparison);
    }

    private static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
}
