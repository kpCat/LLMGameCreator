using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Composition;

public enum UnityArchiveGameDataCategory
{
    Scenes,
    Npcs,
    Quests,
    Dialogues,
    Items,
    Encounters
}

public sealed record UnityArchiveGameDataPayloadRequest
{
    public string ProjectRootPath { get; init; } = string.Empty;
    public string RelativeOutputDirectory { get; init; } = UnityArchiveGameDataPayloadService.RelativeOutputDirectory;
    public GamePackageDefinition Package { get; init; } = new();
}

public sealed record UnityArchiveGameDataIndexEntry
{
    public string Id { get; init; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Title { get; init; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Name { get; init; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Kind { get; init; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Type { get; init; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string SourceArtifactId { get; init; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string SourceContract { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> LinkedIds { get; init; } = Array.Empty<string>();
}

public sealed record UnityArchiveGameDataIndex
{
    public string SchemaVersion { get; init; } = "1";
    public string Category { get; init; } = string.Empty;
    public string SourcePackageId { get; init; } = string.Empty;
    public IReadOnlyList<UnityArchiveGameDataIndexEntry> Entries { get; init; }
        = Array.Empty<UnityArchiveGameDataIndexEntry>();
}

public sealed record UnityArchiveGameDataPayloadFile
{
    public string RelativePath { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
}

public sealed record UnityArchiveGameDataPayloadDiagnostic
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed record UnityArchiveGameDataPayloadResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string SourcePackageId { get; init; } = string.Empty;
    public IReadOnlyList<UnityArchiveGameDataPayloadFile> WrittenFiles { get; init; }
        = Array.Empty<UnityArchiveGameDataPayloadFile>();
    public IReadOnlyList<UnityArchiveGameDataPayloadDiagnostic> Diagnostics { get; init; }
        = Array.Empty<UnityArchiveGameDataPayloadDiagnostic>();
}

public sealed class UnityArchiveGameDataPayloadService
{
    public const string RelativeOutputDirectory = ".llmgc/unity-archive/data";
    public const string GamePackageFilePath = "data/game-package.json";
    public const string GeneratedContentIndexFilePath = "data/generated-content-index.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<UnityArchiveGameDataPayloadResult> WriteAsync(
        UnityArchiveGameDataPayloadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Package);
        if (string.IsNullOrWhiteSpace(request.ProjectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.RelativeOutputDirectory) || Path.IsPathRooted(request.RelativeOutputDirectory))
        {
            throw new InvalidOperationException("Unity archive data output directory must be a relative path.");
        }

        var projectRoot = Path.GetFullPath(request.ProjectRootPath);
        var archiveDataRoot = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "unity-archive", "data"));
        var outputDirectory = Path.GetFullPath(Path.Combine(
            projectRoot,
            request.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory, "Unity archive data output directory");
        EnsureContained(archiveDataRoot, outputDirectory, "Unity archive data output directory");

        if (Directory.Exists(outputDirectory))
        {
            Directory.Delete(outputDirectory, true);
        }

        Directory.CreateDirectory(outputDirectory);

        var sourcePackageId = request.Package.Manifest.PackageId?.Trim() ?? string.Empty;
        var indexes = BuildIndexes(request.Package, sourcePackageId);
        var files = new List<UnityArchiveGameDataPayloadFile>();

        await WriteJsonAsync(OutputPath(outputDirectory, "game-package.json"), request.Package, cancellationToken).ConfigureAwait(false);
        files.Add(File(GamePackageFilePath, "game_package"));

        foreach (var index in indexes.OrderBy(item => IndexFilePath(item.Key), StringComparer.OrdinalIgnoreCase)
                     .ThenBy(item => IndexFilePath(item.Key), StringComparer.Ordinal))
        {
            var relativePath = IndexFilePath(index.Key);
            await WriteJsonAsync(OutputPath(outputDirectory, Path.GetFileName(relativePath)), index.Value, cancellationToken).ConfigureAwait(false);
            files.Add(File(relativePath, "game_data_index"));
        }

        var generatedContentIndex = new UnityArchiveGameDataIndex
        {
            Category = "generated-content",
            SourcePackageId = sourcePackageId,
            Entries = indexes
                .OrderBy(item => CategoryName(item.Key), StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => CategoryName(item.Key), StringComparer.Ordinal)
                .Select(item => new UnityArchiveGameDataIndexEntry
                {
                    Id = CategoryName(item.Key),
                    Name = CategoryName(item.Key),
                    Kind = "category_index",
                    LinkedIds = item.Value.Entries.Select(entry => entry.Id).ToList()
                })
                .ToList()
        };
        await WriteJsonAsync(OutputPath(outputDirectory, "generated-content-index.json"), generatedContentIndex, cancellationToken).ConfigureAwait(false);
        files.Add(File(GeneratedContentIndexFilePath, "generated_content_index"));

        return new UnityArchiveGameDataPayloadResult
        {
            OutputDirectoryPath = outputDirectory,
            SourcePackageId = sourcePackageId,
            WrittenFiles = files
                .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(file => file.RelativePath, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static IReadOnlyDictionary<UnityArchiveGameDataCategory, UnityArchiveGameDataIndex> BuildIndexes(
        GamePackageDefinition package,
        string sourcePackageId)
    {
        return new Dictionary<UnityArchiveGameDataCategory, UnityArchiveGameDataIndex>
        {
            [UnityArchiveGameDataCategory.Scenes] = Index(UnityArchiveGameDataCategory.Scenes, sourcePackageId,
                package.Game.Maps.Select(map => Entry(map.Id, name: map.Name, kind: "package_map", linkedIds: new[] { map.DefaultTileId }))
                    .Concat(package.GeneratedContent.Scenes.Select(scene => Entry(
                        scene.SourceId,
                        title: scene.Title,
                        kind: "generated_scene",
                        linkedIds: new[] { scene.PackageMapId })))),
            [UnityArchiveGameDataCategory.Npcs] = Index(UnityArchiveGameDataCategory.Npcs, sourcePackageId,
                package.GeneratedContent.Npcs.Select(npc => Entry(
                    npc.SourceId,
                    name: npc.Name,
                    kind: "generated_npc",
                    linkedIds: new[] { npc.RegionId, npc.SceneId }))),
            [UnityArchiveGameDataCategory.Quests] = Index(UnityArchiveGameDataCategory.Quests, sourcePackageId,
                package.Game.Quests.Select(quest => Entry(quest.Id, title: quest.Title, kind: quest.Kind, tags: quest.Tags))
                    .Concat(package.GeneratedContent.Quests.Select(quest => Entry(
                        FirstNonEmpty(quest.SourceId, quest.PackageQuestId),
                        title: quest.Title,
                        kind: "generated_quest",
                        linkedIds: new[] { quest.PackageQuestId })))),
            [UnityArchiveGameDataCategory.Dialogues] = Index(UnityArchiveGameDataCategory.Dialogues, sourcePackageId,
                package.Game.Dialogues.Select(dialogue => Entry(dialogue.Id, title: dialogue.Title, kind: "package_dialogue", tags: dialogue.Tags))
                    .Concat(package.GeneratedContent.Dialogues.Select(dialogue => Entry(
                        dialogue.SourceId,
                        title: dialogue.Title,
                        kind: "generated_dialogue",
                        linkedIds: new[] { dialogue.NpcId, dialogue.SceneId })))),
            [UnityArchiveGameDataCategory.Items] = Index(UnityArchiveGameDataCategory.Items, sourcePackageId,
                package.Game.Items.Select(item => Entry(item.Id, name: item.Name, kind: item.Kind, tags: item.Tags))
                    .Concat(package.GeneratedContent.Items.Select(item => Entry(item.SourceId, name: item.Name, kind: "generated_item")))),
            [UnityArchiveGameDataCategory.Encounters] = Index(UnityArchiveGameDataCategory.Encounters, sourcePackageId,
                package.Game.Encounters.Select(encounter => Entry(encounter.Id, name: encounter.Name, kind: encounter.Kind, tags: encounter.Tags))
                    .Concat(package.GeneratedContent.Encounters.Select(encounter => Entry(
                        encounter.SourceId,
                        title: encounter.Title,
                        kind: "generated_encounter",
                        linkedIds: new[] { encounter.RegionId, encounter.SceneId }.Concat(encounter.NpcIds)))))
        };
    }

    private static UnityArchiveGameDataIndex Index(
        UnityArchiveGameDataCategory category,
        string sourcePackageId,
        IEnumerable<UnityArchiveGameDataIndexEntry> entries)
    {
        return new UnityArchiveGameDataIndex
        {
            Category = CategoryName(category),
            SourcePackageId = sourcePackageId,
            Entries = entries
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Id))
                .OrderBy(entry => entry.Id, StringComparer.OrdinalIgnoreCase)
                .ThenBy(entry => entry.Id, StringComparer.Ordinal)
                .ThenBy(entry => FirstNonEmpty(entry.Title, entry.Name), StringComparer.OrdinalIgnoreCase)
                .ThenBy(entry => FirstNonEmpty(entry.Title, entry.Name), StringComparer.Ordinal)
                .ToList()
        };
    }

    private static UnityArchiveGameDataIndexEntry Entry(
        string? id,
        string? title = null,
        string? name = null,
        string? kind = null,
        string? type = null,
        IEnumerable<string?>? tags = null,
        IEnumerable<string?>? linkedIds = null)
    {
        return new UnityArchiveGameDataIndexEntry
        {
            Id = id?.Trim() ?? string.Empty,
            Title = title?.Trim() ?? string.Empty,
            Name = name?.Trim() ?? string.Empty,
            Kind = kind?.Trim() ?? string.Empty,
            Type = type?.Trim() ?? string.Empty,
            Tags = Normalize(tags),
            LinkedIds = Normalize(linkedIds)
        };
    }

    private static IReadOnlyList<string> Normalize(IEnumerable<string?>? values)
    {
        return (values ?? Array.Empty<string>())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim().Replace('\\', '/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ThenBy(value => value, StringComparer.Ordinal)
            .ToList();
    }

    private static string IndexFilePath(UnityArchiveGameDataCategory category)
    {
        return $"data/{CategoryName(category)}-index.json";
    }

    private static string CategoryName(UnityArchiveGameDataCategory category)
    {
        return category.ToString().ToLowerInvariant();
    }

    private static string FirstNonEmpty(string? first, string? second)
    {
        return !string.IsNullOrWhiteSpace(first) ? first.Trim() : second?.Trim() ?? string.Empty;
    }

    private static UnityArchiveGameDataPayloadFile File(string relativePath, string kind)
    {
        return new UnityArchiveGameDataPayloadFile { RelativePath = relativePath, Kind = kind };
    }

    private static async Task WriteJsonAsync<T>(string path, T value, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await System.IO.File.WriteAllTextAsync(path, json, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
    }

    private static string OutputPath(string outputDirectory, string fileName)
    {
        var path = Path.GetFullPath(Path.Combine(outputDirectory, fileName));
        EnsureContained(outputDirectory, path, "Unity archive data file");
        return path;
    }

    private static void EnsureContained(string rootPath, string candidatePath, string pathLabel)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"{pathLabel} must stay under '{root}'.");
        }
    }
}
