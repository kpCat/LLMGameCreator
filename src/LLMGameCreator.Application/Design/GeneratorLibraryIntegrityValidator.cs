using System.Text.Json;

namespace LLMGameCreator.Application.Design;

public sealed class GeneratorLibraryIntegrityValidator : IGeneratorLibraryIntegrityValidator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<GeneratorLibraryIntegrityReport> ValidateAsync(string repositoryRootOrLibraryRoot, CancellationToken cancellationToken)
    {
        var issues = new List<GeneratorLibraryIntegrityIssue>();
        var resolved = ResolveRoots(repositoryRootOrLibraryRoot);
        if (resolved.LibraryRoot == null)
        {
            Add(issues, GeneratorLibraryIntegritySeverity.Error, "library.not_found", "generator-library folder was not found.", repositoryRootOrLibraryRoot, null, "Pass a repository root that contains generator-library or the generator-library folder itself.");
            return CreateReport(null, resolved.RepositoryRoot, 0, 0, 0, 0, 0, issues);
        }

        var manifestsRoot = Path.Combine(resolved.LibraryRoot, "manifests");
        if (!Directory.Exists(manifestsRoot))
        {
            Add(issues, GeneratorLibraryIntegritySeverity.Error, "manifest.folder_missing", "generator-library/manifests folder was not found.", manifestsRoot, null, "Create generator-library/manifests and place *.manifest.json files there.");
            return CreateReport(resolved.LibraryRoot, resolved.RepositoryRoot, 0, 0, 0, 0, 0, issues);
        }

        CheckContractDocs(resolved.LibraryRoot, issues);
        CheckRootLeakage(resolved.RepositoryRoot, resolved.LibraryRoot, issues);

        var manifestPaths = Directory
            .EnumerateFiles(manifestsRoot, "*.manifest.json", SearchOption.TopDirectoryOnly)
            .Where(path => !Path.GetFileName(path).Equals("MANIFEST_CONTRACT.schema.example.json", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var manifestIds = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var moduleIds = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var capabilityCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var declaredFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var manifestCount = 0;
        var moduleCount = 0;

        foreach (var manifestPath in manifestPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var manifest = await ReadManifestAsync(manifestPath, issues, cancellationToken).ConfigureAwait(false);
            if (manifest == null)
            {
                continue;
            }

            manifestCount++;
            var manifestTarget = RelativeTo(resolved.LibraryRoot, manifestPath);
            CheckBatchManifest(resolved.LibraryRoot, manifestPath, manifest, manifestIds, declaredFiles, issues);
            CheckBatchReport(resolved.LibraryRoot, manifestPath, manifest, issues);

            if (manifest.Modules == null)
            {
                continue;
            }

            foreach (var module in manifest.Modules)
            {
                cancellationToken.ThrowIfCancellationRequested();
                moduleCount++;
                CheckModule(resolved.LibraryRoot, manifestPath, manifestTarget, module, moduleIds, capabilityCounts, declaredFiles, issues);
            }
        }

        AddDuplicates(manifestIds, "manifest.id.duplicate", "Duplicate batch manifest id.", issues);
        AddDuplicates(moduleIds, "module.id.duplicate", "Duplicate module id.", issues);

        var duplicateCapabilityCount = capabilityCounts.Values.Count(count => count > 1);
        return CreateReport(
            resolved.LibraryRoot,
            resolved.RepositoryRoot,
            manifestCount,
            moduleCount,
            capabilityCounts.Count,
            declaredFiles.Count,
            duplicateCapabilityCount,
            issues);
    }

    private static async Task<GeneratorBatchManifest?> ReadManifestAsync(string manifestPath, List<GeneratorLibraryIntegrityIssue> issues, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = File.OpenRead(manifestPath);
            var manifest = await JsonSerializer.DeserializeAsync<GeneratorBatchManifest>(stream, JsonOptions, cancellationToken).ConfigureAwait(false);
            if (manifest == null)
            {
                Add(issues, GeneratorLibraryIntegritySeverity.Error, "manifest.empty", "Manifest could not be read.", manifestPath, manifestPath, "Ensure the manifest is a JSON object.");
            }

            return manifest;
        }
        catch (JsonException ex)
        {
            Add(issues, GeneratorLibraryIntegritySeverity.Error, "manifest.invalid_json", ex.Message, manifestPath, manifestPath, "Fix JSON syntax before importing or validating the batch.");
            return null;
        }
        catch (IOException ex)
        {
            Add(issues, GeneratorLibraryIntegritySeverity.Error, "manifest.read_failed", ex.Message, manifestPath, manifestPath, "Ensure the manifest file can be read.");
            return null;
        }
    }

    private static void CheckBatchManifest(
        string libraryRoot,
        string manifestPath,
        GeneratorBatchManifest manifest,
        Dictionary<string, List<string>> manifestIds,
        HashSet<string> declaredFiles,
        List<GeneratorLibraryIntegrityIssue> issues)
    {
        Require(manifest.Id, "manifest.required.id", "Batch manifest id is required.", manifestPath, manifestPath, issues);
        Require(manifest.Batch, "manifest.required.batch", "Batch number is required.", manifestPath, manifestPath, issues);
        Require(manifest.Title, "manifest.required.title", "Batch title is required.", manifestPath, manifestPath, issues);
        Require(manifest.Purpose, "manifest.required.purpose", "Batch purpose is required.", manifestPath, manifestPath, issues);

        if (!string.IsNullOrWhiteSpace(manifest.Id))
        {
            AddSeen(manifestIds, manifest.Id.Trim(), manifestPath);
        }

        if (manifest.Files == null || manifest.Files.Count == 0)
        {
            Add(issues, GeneratorLibraryIntegritySeverity.Error, "manifest.required.files", "Batch manifest files[] must not be empty.", manifestPath, manifestPath, "List every generated file relative to generator-library.");
        }
        else
        {
            foreach (var file in Clean(manifest.Files))
            {
                declaredFiles.Add(file);
                if (!ExistsUnder(libraryRoot, file))
                {
                    Add(issues, GeneratorLibraryIntegritySeverity.Error, "file.missing", "Manifest file entry does not exist under generator-library.", file, manifestPath, "Move the file under generator-library or remove the stale manifest entry.");
                }
            }
        }

        if (manifest.Modules == null || manifest.Modules.Count == 0)
        {
            Add(issues, GeneratorLibraryIntegritySeverity.Error, "manifest.required.modules", "Batch manifest modules[] must not be empty.", manifestPath, manifestPath, "Add canonical module entries or remove the invalid manifest.");
        }

        if (manifest.ExtensionData?.ContainsKey("description") == true)
        {
            var severity = string.IsNullOrWhiteSpace(manifest.Purpose) ? GeneratorLibraryIntegritySeverity.Error : GeneratorLibraryIntegritySeverity.Warning;
            Add(issues, severity, "manifest.alias.description", "Manifest uses description; purpose is the canonical field.", manifestPath, manifestPath, "Replace description with purpose.");
        }

        CheckAlias(manifest.ExtensionData, "supported_runtime_targets", "manifest.alias.supported_runtime_targets", manifestPath, manifestPath, issues);
        CheckAlias(manifest.ExtensionData, "supports", "manifest.alias.supports", manifestPath, manifestPath, issues);
        CheckUnsafeFeatures(manifest.UnsafeFeatures, manifest.ExtensionData, manifestPath, manifestPath, issues);
    }

    private static void CheckModule(
        string libraryRoot,
        string manifestPath,
        string manifestTarget,
        GeneratorManifestModule module,
        Dictionary<string, List<string>> moduleIds,
        Dictionary<string, int> capabilityCounts,
        HashSet<string> declaredFiles,
        List<GeneratorLibraryIntegrityIssue> issues)
    {
        var target = string.IsNullOrWhiteSpace(module.Id) ? manifestTarget : module.Id.Trim();
        Require(module.Id, "module.required.id", "Module id is required.", target, manifestPath, issues);
        Require(module.Path, "module.required.path", "Module path is required.", target, manifestPath, issues);
        Require(module.Category, "module.required.category", "Module category is required.", target, manifestPath, issues);

        if (module.Capabilities == null || module.Capabilities.Count == 0 || !Clean(module.Capabilities).Any())
        {
            Add(issues, GeneratorLibraryIntegritySeverity.Error, "module.required.capabilities", "Module capabilities[] must not be empty.", target, manifestPath, "Declare at least one canonical capability id.");
        }

        if (!string.IsNullOrWhiteSpace(module.Id))
        {
            AddSeen(moduleIds, module.Id.Trim(), manifestPath);
        }

        if (!string.IsNullOrWhiteSpace(module.Path))
        {
            var path = module.Path.Trim();
            declaredFiles.Add(path);
            if (!ExistsUnder(libraryRoot, path))
            {
                Add(issues, GeneratorLibraryIntegritySeverity.Error, "module.path_missing", "Module path does not exist under generator-library.", path, manifestPath, "Move the Lua module under generator-library or fix module.path.");
            }
        }

        foreach (var capability in Clean(module.Capabilities))
        {
            capabilityCounts[capability] = capabilityCounts.TryGetValue(capability, out var count) ? count + 1 : 1;
        }

        CheckAlias(module.ExtensionData, "module_id", "manifest.alias.module_id", target, manifestPath, issues);
        CheckAlias(module.ExtensionData, "file", "manifest.alias.file", target, manifestPath, issues);
        CheckAlias(module.ExtensionData, "depends_on_contracts", "manifest.alias.depends_on_contracts", target, manifestPath, issues);
        CheckAlias(module.ExtensionData, "dependencies", "manifest.alias.dependencies", target, manifestPath, issues);
        CheckUnsafeFeatures(null, module.ExtensionData, target, manifestPath, issues);
    }

    private static void CheckBatchReport(string libraryRoot, string manifestPath, GeneratorBatchManifest manifest, List<GeneratorLibraryIntegrityIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(manifest.Batch))
        {
            return;
        }

        var batch = manifest.Batch.Trim();
        var expected = $"BATCH_{batch}_REPORT.md";
        if (ExistsUnder(libraryRoot, expected))
        {
            return;
        }

        if (batch == "001" && Clean(manifest.Files).Contains("BATCH_REPORT.md", StringComparer.OrdinalIgnoreCase) && ExistsUnder(libraryRoot, "BATCH_REPORT.md"))
        {
            return;
        }

        Add(issues, GeneratorLibraryIntegritySeverity.Error, "batch_report.missing", $"Batch report {expected} is missing.", expected, manifestPath, "Add the numbered batch report under generator-library.");
    }

    private static void CheckContractDocs(string libraryRoot, List<GeneratorLibraryIntegrityIssue> issues)
    {
        foreach (var required in new[] { "docs/lua/MANIFEST_CONTRACT.md", "manifests/MANIFEST_CONTRACT.schema.example.json" })
        {
            if (!ExistsUnder(libraryRoot, required))
            {
                Add(issues, GeneratorLibraryIntegritySeverity.Error, "manifest_contract.missing", "Manifest contract file is missing.", required, null, "Restore the generator-library manifest contract document/schema.");
            }
        }
    }

    private static void CheckRootLeakage(string? repositoryRoot, string libraryRoot, List<GeneratorLibraryIntegrityIssue> issues)
    {
        if (repositoryRoot == null || SamePath(repositoryRoot, libraryRoot))
        {
            return;
        }

        foreach (var folder in new[] { "lua", "manifests" })
        {
            var path = Path.Combine(repositoryRoot, folder);
            if (Directory.Exists(path))
            {
                Add(issues, GeneratorLibraryIntegritySeverity.Error, "root.leakage", $"Root-level {folder}/ folder looks like generator-library leakage.", path, null, "Move generated batch folders under generator-library.");
            }
        }

        foreach (var file in Directory.EnumerateFiles(repositoryRoot, "BATCH_*.md", SearchOption.TopDirectoryOnly))
        {
            if (!Path.GetFileName(file).Equals("MANIFEST_STABILIZATION_REPORT.md", StringComparison.OrdinalIgnoreCase))
            {
                Add(issues, GeneratorLibraryIntegritySeverity.Error, "root.leakage", "Root-level batch report looks like generator-library leakage.", file, null, "Move batch reports under generator-library.");
            }
        }

        foreach (var file in Directory.EnumerateFiles(repositoryRoot, "*.lua", SearchOption.TopDirectoryOnly))
        {
            Add(issues, GeneratorLibraryIntegritySeverity.Error, "root.leakage", "Root-level Lua file looks like generator-library leakage.", file, null, "Move generated Lua files under generator-library/lua.");
        }
    }

    private static void CheckAlias(Dictionary<string, JsonElement>? extensionData, string propertyName, string code, string target, string? manifestPath, List<GeneratorLibraryIntegrityIssue> issues)
    {
        if (extensionData?.ContainsKey(propertyName) == true)
        {
            Add(issues, GeneratorLibraryIntegritySeverity.Warning, code, $"Alias field {propertyName} is not canonical.", target, manifestPath, "Use the canonical manifest field from MANIFEST_CONTRACT.md.");
        }
    }

    private static void CheckUnsafeFeatures(List<string>? unsafeFeatures, Dictionary<string, JsonElement>? extensionData, string target, string? manifestPath, List<GeneratorLibraryIntegrityIssue> issues)
    {
        if (extensionData != null && extensionData.TryGetValue("unsafe_features", out var value) && value.ValueKind != JsonValueKind.Array)
        {
            Add(issues, GeneratorLibraryIntegritySeverity.Error, "unsafe_features.invalid", "unsafe_features must be an array.", target, manifestPath, "Change unsafe_features to an array, normally [].");
        }

        foreach (var feature in Clean(unsafeFeatures))
        {
            var normalized = feature.Replace("-", "_", StringComparison.Ordinal).Replace(" ", "_", StringComparison.Ordinal).ToLowerInvariant();
            if (normalized.Contains("lua_execution", StringComparison.OrdinalIgnoreCase) || normalized.Contains("execute_lua", StringComparison.OrdinalIgnoreCase))
            {
                Add(issues, GeneratorLibraryIntegritySeverity.Error, "manifest.lua_execution_enabled", "Manifest must not claim Lua execution is enabled.", target, manifestPath, "Keep Lua execution out of generator-library manifests until a dedicated Lua sandbox goal.");
            }
        }
    }

    private static void Require(string? value, string code, string message, string target, string? manifestPath, List<GeneratorLibraryIntegrityIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Add(issues, GeneratorLibraryIntegritySeverity.Error, code, message, target, manifestPath, "Populate the canonical required field.");
        }
    }

    private static void AddDuplicates(Dictionary<string, List<string>> seen, string code, string message, List<GeneratorLibraryIntegrityIssue> issues)
    {
        foreach (var pair in seen.Where(pair => pair.Value.Count > 1))
        {
            Add(issues, GeneratorLibraryIntegritySeverity.Error, code, $"{message} {pair.Key}", pair.Key, string.Join("; ", pair.Value), "Keep ids unique across generator-library manifests.");
        }
    }

    private static void AddSeen(Dictionary<string, List<string>> seen, string id, string manifestPath)
    {
        if (!seen.TryGetValue(id, out var paths))
        {
            paths = new List<string>();
            seen[id] = paths;
        }

        paths.Add(manifestPath);
    }

    private static bool ExistsUnder(string root, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
        {
            return false;
        }

        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var fullRoot = EnsureTrailingSeparator(Path.GetFullPath(root));
        return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath);
    }

    private static (string? RepositoryRoot, string? LibraryRoot) ResolveRoots(string rootOrLibraryRoot)
    {
        if (string.IsNullOrWhiteSpace(rootOrLibraryRoot))
        {
            return (null, null);
        }

        var current = new DirectoryInfo(Path.GetFullPath(rootOrLibraryRoot));
        if (!current.Exists)
        {
            return (Path.GetFullPath(rootOrLibraryRoot), null);
        }

        if (current.Name.Equals("generator-library", StringComparison.OrdinalIgnoreCase))
        {
            return (current.Parent?.FullName, current.FullName);
        }

        var repositoryRoot = current.FullName;
        while (current != null)
        {
            var candidate = Path.Combine(current.FullName, "generator-library");
            if (Directory.Exists(candidate))
            {
                return (current.FullName, candidate);
            }

            current = current.Parent;
        }

        return (repositoryRoot, null);
    }

    private static GeneratorLibraryIntegrityReport CreateReport(
        string? libraryRoot,
        string? repositoryRoot,
        int manifestCount,
        int moduleCount,
        int capabilityCount,
        int fileCount,
        int duplicateCapabilityCount,
        IReadOnlyList<GeneratorLibraryIntegrityIssue> issues)
    {
        var summary = new GeneratorLibraryIntegritySummary(
            manifestCount,
            moduleCount,
            capabilityCount,
            fileCount,
            issues.Count(issue => issue.Severity == GeneratorLibraryIntegritySeverity.Error),
            issues.Count(issue => issue.Severity == GeneratorLibraryIntegritySeverity.Warning),
            issues.Count(issue => issue.Severity == GeneratorLibraryIntegritySeverity.Info),
            duplicateCapabilityCount);

        return new GeneratorLibraryIntegrityReport(libraryRoot, repositoryRoot, summary, issues.OrderBy(issue => issue.Severity).ThenBy(issue => issue.Code, StringComparer.OrdinalIgnoreCase).ThenBy(issue => issue.Target, StringComparer.OrdinalIgnoreCase).ToList());
    }

    private static void Add(List<GeneratorLibraryIntegrityIssue> issues, GeneratorLibraryIntegritySeverity severity, string code, string message, string target, string? manifestPath, string? suggestedFix)
    {
        issues.Add(new GeneratorLibraryIntegrityIssue(severity, code, message, target, manifestPath, suggestedFix));
    }

    private static IEnumerable<string> Clean(IEnumerable<string>? values)
    {
        return (values ?? Array.Empty<string>())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim());
    }

    private static string RelativeTo(string root, string path)
    {
        return Path.GetRelativePath(root, path).Replace('\\', '/');
    }

    private static bool SamePath(string left, string right)
    {
        return string.Equals(Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
    }

    private static string EnsureTrailingSeparator(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;
    }
}
