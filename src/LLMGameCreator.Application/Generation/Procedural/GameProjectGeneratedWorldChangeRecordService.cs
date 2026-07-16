using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

namespace LLMGameCreator.Application.Generation.Procedural;

public static class GameProjectGeneratedWorldChangeVocabulary
{
    public const string SchemaVersion = "generated_world_change_result_v1";
    public const string RelativePath = ".llmgc/regeneration/last-successful-world-change.json";
}

public sealed record GameProjectGeneratedWorldChangeRecord
{
    public string SchemaVersion { get; init; } = GameProjectGeneratedWorldChangeVocabulary.SchemaVersion;
    public string Status { get; init; } = "GREEN";
    public string OperationKind { get; init; } = "regeneration";
    public string AttemptId { get; init; } = string.Empty;
    public string FromWorldId { get; init; } = string.Empty;
    public string ToWorldId { get; init; } = string.Empty;
    public string OldSourceRecordSha256 { get; init; } = string.Empty;
    public string NewSourceRecordSha256 { get; init; } = string.Empty;
    public string OldPackageSha256 { get; init; } = string.Empty;
    public string NewPackageSha256 { get; init; } = string.Empty;
    public string OldCompositionPackageSha256 { get; init; } = string.Empty;
    public string NewCompositionPackageSha256 { get; init; } = string.Empty;
    public string OldFinalStateHash { get; init; } = string.Empty;
    public string NewFinalStateHash { get; init; } = string.Empty;
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public GameProjectSeedRegenerationDiff Diff { get; init; } = new();
    public string SelectedBuildHistoryFileName { get; init; } = string.Empty;
    public string? PreviousReleaseCandidateRecordSha256 { get; init; }
    public string PreviousReleaseCandidateStatus { get; init; } = "ABSENT";
    public string CandidateSealSha256 { get; init; } = string.Empty;
    public string TransactionState { get; init; } = "committed";
}

public sealed record GameProjectGeneratedWorldChangeReadResult
{
    public bool Present { get; init; }
    public bool Passed { get; init; }
    public GameProjectGeneratedWorldChangeRecord? Record { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed class GameProjectGeneratedWorldChangeRecordService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    private readonly SeededGeneratedProjectSourceService _sourceService;
    private readonly GeneratedWorldHistoryService _historyService;

    public GameProjectGeneratedWorldChangeRecordService(
        SeededGeneratedProjectSourceService sourceService,
        GeneratedWorldHistoryService historyService)
    {
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
    }

    public string Serialize(GameProjectGeneratedWorldChangeRecord record) =>
        JsonSerializer.Serialize(record, JsonOptions) + Environment.NewLine;

    public string RecordPath(string projectFolder) =>
        GameProjectFeatureModuleAuthoringService.ConfinedPath(
            projectFolder, GameProjectGeneratedWorldChangeVocabulary.RelativePath);

    public GameProjectGeneratedWorldChangeReadResult Read(string projectFolder)
    {
        var path = RecordPath(projectFolder);
        if (!File.Exists(path)) return new GameProjectGeneratedWorldChangeReadResult();
        try
        {
            var record = JsonSerializer.Deserialize<GameProjectGeneratedWorldChangeRecord>(
                File.ReadAllText(path, Encoding.UTF8), JsonOptions);
            if (record is null) return Failed("world_change.empty");
            var diagnostics = new List<string>();
            if (record.SchemaVersion != GameProjectGeneratedWorldChangeVocabulary.SchemaVersion
                || record.Status != "GREEN"
                || record.OperationKind is not "regeneration" and not "history_rollback"
                || record.TransactionState != "committed")
                diagnostics.Add("world_change.unsupported_schema");
            var source = _sourceService.Validate(projectFolder);
            if (source is not { Present: true, Passed: true, Source: not null })
                diagnostics.Add("world_change.source_invalid");
            else
            {
                var worldId = _historyService.WorldId(projectFolder, source);
                if (!string.Equals(worldId, record.ToWorldId, StringComparison.Ordinal))
                    diagnostics.Add("world_change.current_world_mismatch");
                var sourcePath = GameProjectFeatureModuleAuthoringService.ConfinedPath(
                    projectFolder, SeededGeneratedProjectVocabulary.SourceRelativePath);
                if (!string.Equals(HashFile(sourcePath), record.NewSourceRecordSha256, StringComparison.Ordinal))
                    diagnostics.Add("world_change.source_hash_mismatch");
            }
            var packagePath = GameProjectFeatureModuleAuthoringService.ConfinedPath(projectFolder, "package.json");
            if (!File.Exists(packagePath)
                || !string.Equals(HashFile(packagePath), record.NewPackageSha256, StringComparison.Ordinal))
                diagnostics.Add("world_change.package_mismatch");
            var history = GameProjectFeatureModuleAuthoringService.ConfinedPath(projectFolder,
                UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot + "/"
                + record.SelectedBuildHistoryFileName);
            if (!File.Exists(history)) diagnostics.Add("world_change.history_missing");
            return new GameProjectGeneratedWorldChangeReadResult
            {
                Present = true,
                Passed = diagnostics.Count == 0,
                Record = record,
                Diagnostics = diagnostics
            };
        }
        catch (Exception exception) when (exception is IOException or JsonException
                                           or InvalidOperationException)
        {
            return Failed("world_change.unreadable:" + exception.Message);
        }
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static GameProjectGeneratedWorldChangeReadResult Failed(string diagnostic) => new()
    {
        Present = true,
        Diagnostics = [diagnostic]
    };
}
