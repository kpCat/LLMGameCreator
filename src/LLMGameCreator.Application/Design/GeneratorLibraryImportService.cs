using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design;

public sealed class GeneratorLibraryImportService : IGeneratorLibraryImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    private readonly IGeneratorLibraryRegistry _registry;

    static GeneratorLibraryImportService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public GeneratorLibraryImportService(IGeneratorLibraryRegistry registry)
    {
        _registry = registry;
    }

    public async Task<GeneratorLibraryImportReport> ImportGeneratorLibraryAsync(string repositoryRootOrLibraryRoot, CancellationToken cancellationToken)
    {
        var importId = "import/" + DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssfff");
        var issues = new List<GeneratorLibraryImportIssue>();
        var capabilities = new Dictionary<string, CapabilityModuleRecord>(StringComparer.OrdinalIgnoreCase);
        var modules = new Dictionary<string, GeneratorModuleRecord>(StringComparer.OrdinalIgnoreCase);
        var files = new Dictionary<string, GeneratorModuleFileRecord>(StringComparer.OrdinalIgnoreCase);
        var importedManifestCount = 0;

        var libraryRoot = ResolveLibraryRoot(repositoryRootOrLibraryRoot);
        if (libraryRoot == null)
        {
            issues.Add(CreateIssue(importId, "error", "generator_library.not_found", "generator-library folder was not found.", repositoryRootOrLibraryRoot));
            await _registry.SaveImportedLibraryAsync(new GeneratorLibraryImportData(Array.Empty<CapabilityModuleRecord>(), Array.Empty<GeneratorModuleRecord>(), Array.Empty<GeneratorModuleFileRecord>(), issues), cancellationToken).ConfigureAwait(false);
            return CreateReport(importId, 0, 0, capabilities.Count, modules.Count, files.Count, issues);
        }

        var manifestsRoot = Path.Combine(libraryRoot, "manifests");
        if (!Directory.Exists(manifestsRoot))
        {
            issues.Add(CreateIssue(importId, "error", "generator_library.manifests_not_found", "generator-library/manifests folder was not found.", manifestsRoot));
            await _registry.SaveImportedLibraryAsync(new GeneratorLibraryImportData(Array.Empty<CapabilityModuleRecord>(), Array.Empty<GeneratorModuleRecord>(), Array.Empty<GeneratorModuleFileRecord>(), issues), cancellationToken).ConfigureAwait(false);
            return CreateReport(importId, 0, 0, capabilities.Count, modules.Count, files.Count, issues);
        }

        var manifestPaths = Directory
            .EnumerateFiles(manifestsRoot, "*.manifest.json", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var manifestPath in manifestPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ImportManifestAsync(importId, libraryRoot, manifestPath, capabilities, modules, files, issues, cancellationToken).ConfigureAwait(false);
            if (!issues.Any(issue => issue.Target == manifestPath && issue.Severity == "error"))
            {
                importedManifestCount++;
            }
        }

        var data = new GeneratorLibraryImportData(
            capabilities.Values.OrderBy(item => item.Id, StringComparer.OrdinalIgnoreCase).ToList(),
            modules.Values.OrderBy(item => item.Id, StringComparer.OrdinalIgnoreCase).ToList(),
            files.Values.OrderBy(item => item.Id, StringComparer.OrdinalIgnoreCase).ToList(),
            issues);

        await _registry.SaveImportedLibraryAsync(data, cancellationToken).ConfigureAwait(false);
        return CreateReport(importId, manifestPaths.Count, importedManifestCount, capabilities.Count, modules.Count, files.Count, issues);
    }

    private static async Task ImportManifestAsync(
        string importId,
        string libraryRoot,
        string manifestPath,
        IDictionary<string, CapabilityModuleRecord> capabilities,
        IDictionary<string, GeneratorModuleRecord> modules,
        IDictionary<string, GeneratorModuleFileRecord> files,
        List<GeneratorLibraryImportIssue> issues,
        CancellationToken cancellationToken)
    {
        GeneratorBatchManifest? manifest;
        string rawJson;
        try
        {
            rawJson = await File.ReadAllTextAsync(manifestPath, cancellationToken).ConfigureAwait(false);
            manifest = JsonSerializer.Deserialize<GeneratorBatchManifest>(rawJson, JsonOptions);
        }
        catch (JsonException ex)
        {
            issues.Add(CreateIssue(importId, "error", "manifest.invalid_json", ex.Message, manifestPath));
            return;
        }
        catch (IOException ex)
        {
            issues.Add(CreateIssue(importId, "error", "manifest.read_failed", ex.Message, manifestPath));
            return;
        }

        if (manifest == null)
        {
            issues.Add(CreateIssue(importId, "error", "manifest.empty", "Manifest could not be read.", manifestPath));
            return;
        }

        if (string.IsNullOrWhiteSpace(manifest.Id))
        {
            issues.Add(CreateIssue(importId, "error", "manifest.id.empty", "Manifest id is required.", manifestPath));
            return;
        }

        var batchId = string.IsNullOrWhiteSpace(manifest.Batch) ? manifest.Id : manifest.Batch.Trim();
        var importedUtc = DateTimeOffset.UtcNow;
        var runtimeTargets = Merge(manifest.RuntimeTargets, manifest.SupportedRuntimeTargets);
        var turnModes = Merge(manifest.ArchitectureNotes?.TurnModes, manifest.SupportedTimeModes);
        var combatModes = Merge(manifest.ArchitectureNotes?.CombatModes, manifest.SupportedCombatModes);
        var uiModes = Clean(manifest.ArchitectureNotes?.UiModes);
        var worldScales = Clean(manifest.ArchitectureNotes?.WorldScales);
        var sourceManifestPath = Path.GetRelativePath(libraryRoot, manifestPath).Replace('\\', '/');
        var metadataJson = BuildManifestMetadataJson(manifest);

        foreach (var relativePath in Clean(manifest.Files))
        {
            var id = StableId(batchId, relativePath);
            files[id] = new GeneratorModuleFileRecord(id, batchId, relativePath, InferFileKind(relativePath), sourceManifestPath);
        }

        if (manifest.Modules == null || manifest.Modules.Count == 0)
        {
            issues.Add(CreateIssue(importId, "warning", "manifest.modules.empty", "Manifest contains no modules.", sourceManifestPath));
            return;
        }

        foreach (var module in manifest.Modules)
        {
            if (string.IsNullOrWhiteSpace(module.Id))
            {
                issues.Add(CreateIssue(importId, "error", "module.id.empty", "Module id is required.", sourceManifestPath));
                continue;
            }

            if (string.IsNullOrWhiteSpace(module.Path))
            {
                issues.Add(CreateIssue(importId, "error", "module.path.empty", "Module path is required.", module.Id));
                continue;
            }

            var category = string.IsNullOrWhiteSpace(module.Category)
                ? InferCategory(module.Id)
                : module.Category.Trim();
            var moduleCapabilities = Clean(module.Capabilities);
            var moduleRuntimeTargets = Merge(module.RuntimeTargets, runtimeTargets);
            var moduleTurnModes = Merge(module.SupportedTurnModes, turnModes);
            var moduleCombatModes = Merge(module.SupportedCombatModes, combatModes);

            var moduleRecord = new GeneratorModuleRecord(
                module.Id.Trim(),
                batchId,
                module.Path.Trim(),
                category,
                ToJson(moduleCapabilities),
                ToJson(Merge(module.Dependencies, module.DependsOn)),
                ToJson(moduleRuntimeTargets),
                ToJson(moduleTurnModes),
                ToJson(moduleCombatModes),
                sourceManifestPath,
                BuildModuleMetadataJson(module),
                importedUtc);

            modules[moduleRecord.Id] = moduleRecord;

            foreach (var capabilityId in moduleCapabilities)
            {
                capabilities[capabilityId] = new CapabilityModuleRecord(
                    capabilityId,
                    InferCapabilityCategory(capabilityId, category),
                    capabilityId,
                    manifest.Purpose ?? string.Empty,
                    sourceManifestPath,
                    ToJson(moduleRuntimeTargets),
                    ToJson(moduleTurnModes),
                    ToJson(moduleCombatModes),
                    ToJson(uiModes),
                    ToJson(worldScales),
                    metadataJson,
                    importedUtc);
            }
        }
    }

    private static string? ResolveLibraryRoot(string rootOrLibraryRoot)
    {
        if (string.IsNullOrWhiteSpace(rootOrLibraryRoot))
        {
            return null;
        }

        var current = new DirectoryInfo(Path.GetFullPath(rootOrLibraryRoot));
        if (!current.Exists)
        {
            return null;
        }

        if (current.Name.Equals("generator-library", StringComparison.OrdinalIgnoreCase) &&
            Directory.Exists(Path.Combine(current.FullName, "manifests")))
        {
            return current.FullName;
        }

        while (current != null)
        {
            var candidate = Path.Combine(current.FullName, "generator-library");
            if (Directory.Exists(Path.Combine(candidate, "manifests")))
            {
                return candidate;
            }

            current = current.Parent;
        }

        return null;
    }

    private static GeneratorLibraryImportReport CreateReport(
        string importId,
        int manifestCount,
        int importedManifestCount,
        int capabilityCount,
        int moduleCount,
        int fileCount,
        IReadOnlyList<GeneratorLibraryImportIssue> issues)
    {
        return new GeneratorLibraryImportReport(importId, manifestCount, importedManifestCount, moduleCount, capabilityCount, fileCount, issues);
    }

    private static GeneratorLibraryImportIssue CreateIssue(string importId, string severity, string code, string message, string target)
    {
        return new GeneratorLibraryImportIssue(
            StableId(importId, code, target, message),
            importId,
            severity,
            code,
            message,
            target,
            "{}");
    }

    private static List<string> Merge(IEnumerable<string>? first, IEnumerable<string>? second)
    {
        return Clean((first ?? Array.Empty<string>()).Concat(second ?? Array.Empty<string>()));
    }

    private static List<string> Clean(IEnumerable<string>? values)
    {
        return (values ?? Array.Empty<string>())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string ToJson(IEnumerable<string> values)
    {
        return JsonSerializer.Serialize(values.ToList(), JsonOptions);
    }

    private static string BuildManifestMetadataJson(GeneratorBatchManifest manifest)
    {
        var metadata = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["manifestId"] = manifest.Id,
            ["version"] = manifest.Version,
            ["name"] = manifest.Name,
            ["deterministic"] = manifest.Deterministic,
            ["unsafeFeatures"] = manifest.UnsafeFeatures ?? new List<string>(),
            ["contracts"] = manifest.Contracts,
            ["contractsIntroduced"] = manifest.ContractsIntroduced
        };

        AddExtensionData(metadata, manifest.ExtensionData);
        AddExtensionData(metadata, manifest.ArchitectureNotes?.ExtensionData, "architectureNotes.");
        return JsonSerializer.Serialize(metadata, JsonOptions);
    }

    private static string BuildModuleMetadataJson(GeneratorManifestModule module)
    {
        var metadata = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        AddExtensionData(metadata, module.ExtensionData);
        return JsonSerializer.Serialize(metadata, JsonOptions);
    }

    private static void AddExtensionData(IDictionary<string, object?> metadata, Dictionary<string, JsonElement>? extensionData, string prefix = "")
    {
        if (extensionData == null)
        {
            return;
        }

        foreach (var pair in extensionData)
        {
            metadata[prefix + pair.Key] = pair.Value;
        }
    }

    private static string InferCategory(string moduleId)
    {
        var slash = moduleId.IndexOf('/');
        return slash <= 0 ? "unknown" : moduleId[..slash];
    }

    private static string InferCapabilityCategory(string capabilityId, string fallback)
    {
        var dot = capabilityId.IndexOf('.');
        return dot <= 0 ? fallback : capabilityId[..dot];
    }

    private static string InferFileKind(string relativePath)
    {
        var extension = Path.GetExtension(relativePath).ToLowerInvariant();
        return extension switch
        {
            ".lua" => "lua",
            ".md" => "doc",
            ".json" => "manifest",
            _ => string.IsNullOrWhiteSpace(extension) ? "file" : extension.TrimStart('.')
        };
    }

    private static string StableId(params string[] parts)
    {
        var text = string.Join("|", parts);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
