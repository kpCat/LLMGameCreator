using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedWorldHistoryService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly SeededGeneratedProjectSourceService _sourceService;

    public GeneratedWorldHistoryService(SeededGeneratedProjectSourceService sourceService)
    {
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
    }

    public GeneratedWorldHistoryReadResult ReadAll(string projectFolder)
    {
        var current = _sourceService.Validate(projectFolder);
        if (current is not { Present: true, Passed: true, Source: not null })
            return new GeneratedWorldHistoryReadResult
            {
                Diagnostics = [current.Present ? "world_history.source_invalid" : "world_history.not_generated_project"]
            };
        var currentWorldId = WorldId(projectFolder, current);
        var root = Confined(projectFolder, GeneratedWorldHistoryVocabulary.RelativeRoot);
        if (!Directory.Exists(root)) return new GeneratedWorldHistoryReadResult
        {
            Passed = true,
            CurrentWorldId = currentWorldId
        };
        var entries = Directory.EnumerateDirectories(root, "*", SearchOption.TopDirectoryOnly)
            .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal)
            .Select(ValidateEntry)
            .Select(entry => entry with { IsCurrent = entry.WorldId == currentWorldId })
            .OrderByDescending(entry => entry.IsCurrent)
            .ThenBy(entry => entry.WorldId, StringComparer.Ordinal)
            .ToList();
        return new GeneratedWorldHistoryReadResult
        {
            Passed = entries.All(entry => entry.Passed),
            CurrentWorldId = currentWorldId,
            Entries = entries,
            Diagnostics = entries.SelectMany(entry => entry.Diagnostics).Distinct(StringComparer.Ordinal).ToList()
        };
    }

    public GeneratedWorldHistoryEntry Read(string projectFolder, string worldId)
    {
        RequireWorldId(worldId);
        var path = Confined(projectFolder, GeneratedWorldHistoryVocabulary.RelativeRoot + "/" + worldId);
        if (!Directory.Exists(path)) return Failed(path, worldId, "world_history.target_missing");
        return ValidateEntry(path);
    }

    public GeneratedWorldHistoryStageResult Stage(
        string sourceProjectFolder,
        string authoritativeProjectFolder,
        string stagingRoot,
        string createdByOperationKind)
    {
        var source = _sourceService.Validate(sourceProjectFolder);
        if (source is not { Present: true, Passed: true, Source: not null, RegeneratedPlan: not null,
                ResolvedGenerationOptions: not null })
            return new GeneratedWorldHistoryStageResult { Diagnostics = ["world_history.source_invalid"] };
        var worldId = WorldId(sourceProjectFolder, source);
        var existing = Confined(authoritativeProjectFolder,
            GeneratedWorldHistoryVocabulary.RelativeRoot + "/" + worldId);
        if (Directory.Exists(existing))
        {
            var validation = ValidateEntry(existing);
            if (!validation.Passed) return new GeneratedWorldHistoryStageResult
            {
                WorldId = worldId,
                Diagnostics = ["world_history.identity_collision"]
            };
            return new GeneratedWorldHistoryStageResult
            {
                Passed = true,
                AlreadyPresent = true,
                WorldId = worldId,
                Manifest = validation.Manifest,
                StagedEntryPath = existing
            };
        }

        var target = Confined(stagingRoot, worldId);
        if (Directory.Exists(target)) Directory.Delete(target, recursive: true);
        var generationTarget = Confined(target, "generation");
        Directory.CreateDirectory(generationTarget);
        var sourceGeneration = Confined(sourceProjectFolder, SeededGeneratedProjectVocabulary.GenerationRelativeRoot);
        var allowed = SeededGeneratedProjectVocabulary.RequiredSidecarFileNames
            .Append(Path.GetFileName(SeededGeneratedProjectVocabulary.SourceRelativePath))
            .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
        foreach (var fileName in allowed)
        {
            var sourcePath = Confined(sourceGeneration, fileName);
            if (!File.Exists(sourcePath)) return new GeneratedWorldHistoryStageResult
            {
                WorldId = worldId,
                Diagnostics = ["world_history.source_invalid"]
            };
            File.Copy(sourcePath, Confined(generationTarget, fileName), overwrite: false);
        }
        var plan = source.RegeneratedPlan;
        var manifest = new GeneratedWorldHistoryManifest
        {
            WorldId = worldId,
            SourceSchemaVersion = source.Source.SchemaVersion,
            SourceRecordSha256 = HashFile(Confined(sourceGeneration,
                Path.GetFileName(SeededGeneratedProjectVocabulary.SourceRelativePath))),
            SourceRequestSha256 = GameProjectSeedRegenerationDiffService.RequestSha256(source.Source.GenerationRequest),
            PlanSha256 = source.Source.PlanSha256,
            OverlaySha256 = source.Source.GeneratedOverlaySha256,
            GeneratedBasePackageSha256 = source.Source.GeneratedBasePackageSha256,
            Seed = source.Source.Seed,
            Mode = source.Source.Mode,
            PresetId = source.Source.PresetId,
            ResolvedStyleHintIds = source.ResolvedGenerationOptions.CompactStyleHintIds,
            ResolvedVariantIds = source.ResolvedGenerationOptions.SelectedVariantIds,
            Counts = source.Source.Counts,
            StartRegionTitle = plan.World.Regions.FirstOrDefault()?.Label ?? string.Empty,
            TravelDestinationTitle = DestinationTitle(plan),
            GenerationTreeSha256 = GameProjectSeedRegenerationCandidateSealService.TreeSha256(target, "generation"),
            CreatedByOperationKind = createdByOperationKind
        };
        WriteAtomic(Confined(target, "manifest.json"),
            JsonSerializer.Serialize(manifest, JsonOptions) + Environment.NewLine);
        var staged = ValidateEntry(target);
        return new GeneratedWorldHistoryStageResult
        {
            Passed = staged.Passed,
            WorldId = worldId,
            StagedEntryPath = target,
            Manifest = staged.Manifest,
            Diagnostics = staged.Diagnostics
        };
    }

    public GeneratedWorldHistoryEntry ValidateEntry(string entryPath)
    {
        var full = Path.GetFullPath(entryPath);
        var worldId = Path.GetFileName(full);
        try
        {
            RequireWorldId(worldId);
            var manifestPath = Confined(full, "manifest.json");
            var generationRoot = Confined(full, "generation");
            if (!File.Exists(manifestPath) || !Directory.Exists(generationRoot))
                return Failed(full, worldId, "world_history.invalid_manifest");
            var manifestJson = File.ReadAllText(manifestPath, Encoding.UTF8);
            using var manifestDocument = JsonDocument.Parse(manifestJson);
            var expectedProperties = new HashSet<string>(StringComparer.Ordinal)
            {
                "schemaVersion", "worldId", "sourceSchemaVersion", "sourceRecordSha256",
                "sourceRequestSha256", "planSha256", "overlaySha256", "generatedBasePackageSha256",
                "seed", "mode", "presetId", "resolvedStyleHintIds", "resolvedVariantIds", "counts",
                "startRegionTitle", "travelDestinationTitle", "generationTreeSha256", "createdByOperationKind"
            };
            if (!manifestDocument.RootElement.EnumerateObject().Select(property => property.Name)
                    .ToHashSet(StringComparer.Ordinal).SetEquals(expectedProperties))
                return Failed(full, worldId, "world_history.invalid_manifest");
            var manifest = JsonSerializer.Deserialize<GeneratedWorldHistoryManifest>(manifestJson, JsonOptions);
            if (manifest is null || manifest.SchemaVersion != GeneratedWorldHistoryVocabulary.SchemaVersion)
                return Failed(full, worldId, "world_history.invalid_manifest");
            if (manifest.CreatedByOperationKind is not GeneratedWorldHistoryOperationKinds.InitialCapture
                    and not GeneratedWorldHistoryOperationKinds.RegenerationBefore
                    and not GeneratedWorldHistoryOperationKinds.RegenerationAfter
                    and not GeneratedWorldHistoryOperationKinds.HistoryRollbackBefore
                    and not GeneratedWorldHistoryOperationKinds.HistoryRollbackAfter)
                return Failed(full, worldId, "world_history.invalid_manifest");
            if (!string.Equals(manifest.WorldId, worldId, StringComparison.Ordinal))
                return Failed(full, worldId, "world_history.identity_mismatch");
            var actualFiles = Directory.EnumerateFiles(generationRoot, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(generationRoot, path).Replace('\\', '/'))
                .OrderBy(value => value, StringComparer.Ordinal).ToList();
            var allowed = SeededGeneratedProjectVocabulary.RequiredSidecarFileNames
                .Append(Path.GetFileName(SeededGeneratedProjectVocabulary.SourceRelativePath))
                .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
            if (!actualFiles.SequenceEqual(allowed, StringComparer.Ordinal))
                return Failed(full, worldId, "world_history.invalid_manifest");
            var tree = GameProjectSeedRegenerationCandidateSealService.TreeSha256(full, "generation");
            if (!string.Equals(tree, manifest.GenerationTreeSha256, StringComparison.Ordinal))
                return Failed(full, worldId, "world_history.tree_hash_mismatch");

            using var adapter = TemporaryAdapter.Create(generationRoot);
            var source = _sourceService.Validate(adapter.Root);
            if (source is not { Present: true, Passed: true, Source: not null, RegeneratedPlan: not null,
                    ResolvedGenerationOptions: not null })
                return Failed(full, worldId, "world_history.source_invalid");
            var calculatedId = WorldId(adapter.Root, source);
            var plan = source.RegeneratedPlan;
            var sourceRecordPath = Confined(generationRoot,
                Path.GetFileName(SeededGeneratedProjectVocabulary.SourceRelativePath));
            if (!string.Equals(calculatedId, manifest.WorldId, StringComparison.Ordinal)
                || !string.Equals(HashFile(sourceRecordPath), manifest.SourceRecordSha256, StringComparison.Ordinal)
                || !string.Equals(GameProjectSeedRegenerationDiffService.RequestSha256(source.Source.GenerationRequest),
                    manifest.SourceRequestSha256, StringComparison.Ordinal)
                || !string.Equals(source.Source.PlanSha256, manifest.PlanSha256, StringComparison.Ordinal)
                || !string.Equals(source.Source.GeneratedOverlaySha256, manifest.OverlaySha256, StringComparison.Ordinal)
                || !string.Equals(source.Source.GeneratedBasePackageSha256,
                    manifest.GeneratedBasePackageSha256, StringComparison.Ordinal)
                || !string.Equals(source.Source.SchemaVersion, manifest.SourceSchemaVersion, StringComparison.Ordinal)
                || !string.Equals(source.Source.Seed, manifest.Seed, StringComparison.Ordinal)
                || !string.Equals(source.Source.Mode, manifest.Mode, StringComparison.Ordinal)
                || !string.Equals(source.Source.PresetId, manifest.PresetId, StringComparison.Ordinal)
                || !source.ResolvedGenerationOptions.CompactStyleHintIds.OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(manifest.ResolvedStyleHintIds.OrderBy(value => value, StringComparer.Ordinal),
                        StringComparer.Ordinal)
                || !source.ResolvedGenerationOptions.SelectedVariantIds.OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(manifest.ResolvedVariantIds.OrderBy(value => value, StringComparer.Ordinal),
                        StringComparer.Ordinal)
                || source.Source.Counts != manifest.Counts
                || !string.Equals(plan.World.Regions.FirstOrDefault()?.Label ?? string.Empty,
                    manifest.StartRegionTitle, StringComparison.Ordinal)
                || !string.Equals(DestinationTitle(plan), manifest.TravelDestinationTitle, StringComparison.Ordinal))
                return Failed(full, worldId, "world_history.identity_mismatch");
            return new GeneratedWorldHistoryEntry
            {
                Passed = true,
                WorldId = worldId,
                EntryPath = full,
                Manifest = manifest,
                SourceValidation = source
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or JsonException or InvalidOperationException)
        {
            var diagnostic = exception.Message == "world_history.path_escape"
                ? exception.Message : "world_history.invalid_manifest";
            return Failed(full, worldId, diagnostic);
        }
    }

    public string WorldId(string projectFolder, SeededGeneratedProjectSourceValidationResult source)
    {
        if (source is not { Present: true, Passed: true, Source: not null })
            throw new InvalidOperationException("world_history.source_invalid");
        var sourcePath = Confined(projectFolder, SeededGeneratedProjectVocabulary.SourceRelativePath);
        var sourceBytes = File.ReadAllBytes(sourcePath);
        var baseHash = Encoding.UTF8.GetBytes(source.Source.GeneratedBasePackageSha256);
        var combined = new byte[sourceBytes.Length + baseHash.Length];
        Buffer.BlockCopy(sourceBytes, 0, combined, 0, sourceBytes.Length);
        Buffer.BlockCopy(baseHash, 0, combined, sourceBytes.Length, baseHash.Length);
        return Convert.ToHexString(SHA256.HashData(combined)).ToLowerInvariant();
    }

    private static string DestinationTitle(ProceduralGeneratedGamePlan plan)
    {
        var destination = plan.World.Connections.FirstOrDefault()?.ToRegionId;
        return plan.World.Regions.SingleOrDefault(region => region.RegionId == destination)?.Label
               ?? plan.World.Regions.Skip(1).FirstOrDefault()?.Label
               ?? string.Empty;
    }

    private static void RequireWorldId(string worldId)
    {
        if (worldId.Length != 64 || worldId.Any(character => !char.IsAsciiHexDigit(character) || char.IsUpper(character)))
            throw new InvalidOperationException("world_history.path_escape");
    }

    private static string Confined(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!path.Equals(fullRoot, comparison) && !path.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException("world_history.path_escape");
        return path;
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void WriteAtomic(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(temporary, text, Utf8WithoutBom);
            File.Move(temporary, path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static GeneratedWorldHistoryEntry Failed(string path, string worldId, string diagnostic) => new()
    {
        WorldId = worldId,
        EntryPath = path,
        Diagnostics = [diagnostic]
    };

    private sealed class TemporaryAdapter : IDisposable
    {
        private TemporaryAdapter(string root) => Root = root;
        public string Root { get; }

        public static TemporaryAdapter Create(string generationRoot)
        {
            var root = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "WorldHistoryValidation",
                Guid.NewGuid().ToString("N"));
            var target = Confined(root, SeededGeneratedProjectVocabulary.GenerationRelativeRoot);
            Directory.CreateDirectory(target);
            foreach (var file in Directory.EnumerateFiles(generationRoot, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(generationRoot, file);
                var destination = Confined(target, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(file, destination, overwrite: false);
            }
            return new TemporaryAdapter(root);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
        }
    }
}
