using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GameProjectSeedRegenerationRecordService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly string _repositoryRoot;
    private readonly SeededGeneratedProjectSourceService _sourceService;

    public GameProjectSeedRegenerationRecordService(
        string repositoryRoot,
        SeededGeneratedProjectSourceService sourceService)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
    }

    public string Serialize(GameProjectSeedRegenerationRecord record) =>
        JsonSerializer.Serialize(record, JsonOptions) + Environment.NewLine;

    public string RecordPath(string projectFolder) => GameProjectFeatureModuleAuthoringService.ConfinedPath(
        projectFolder, GameProjectSeedRegenerationVocabulary.LastSuccessfulRelativePath);

    public GameProjectSeedRegenerationRecordReadResult Read(string projectFolder) => Read(projectFolder, null);

    public GameProjectSeedRegenerationRecordReadResult Read(
        string projectFolder,
        GameProjectOperationLease? operationLease)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFolder);
        var path = RecordPath(projectFolder);
        if (!File.Exists(path)) return new GameProjectSeedRegenerationRecordReadResult();
        var diagnostics = new List<string>();
        try
        {
            var record = JsonSerializer.Deserialize<GameProjectSeedRegenerationRecord>(
                File.ReadAllText(path, Encoding.UTF8), JsonOptions);
            if (record is null) return Failed("regeneration_record.empty");
            if (record.SchemaVersion != GameProjectSeedRegenerationVocabulary.ResultSchemaVersion
                || record.Status != "GREEN") diagnostics.Add("regeneration_record.unsupported_schema");
            var sourcePath = GameProjectFeatureModuleAuthoringService.ConfinedPath(
                projectFolder, SeededGeneratedProjectVocabulary.SourceRelativePath);
            var source = _sourceService.Validate(projectFolder);
            if (source is not { Present: true, Passed: true, Source: not null })
                diagnostics.Add("regeneration_record.source_invalid");
            else
            {
                if (!string.Equals(HashFile(sourcePath), record.NewSourceRecordSha256, StringComparison.Ordinal)
                    || !string.Equals(source.Source.PlanSha256, record.NewPlanSha256, StringComparison.Ordinal)
                    || !string.Equals(source.Source.GeneratedOverlaySha256, record.NewOverlaySha256, StringComparison.Ordinal)
                    || !string.Equals(source.Source.GeneratedBasePackageSha256, record.NewGeneratedBaseSha256,
                        StringComparison.Ordinal))
                    diagnostics.Add("regeneration_record.source_mismatch");
            }
            var packagePath = GameProjectFeatureModuleAuthoringService.ConfinedPath(projectFolder, "package.json");
            if (!File.Exists(packagePath) || !string.Equals(HashFile(packagePath), record.NewPackageSha256,
                    StringComparison.Ordinal)) diagnostics.Add("regeneration_record.package_mismatch");
            var package = JsonSerializer.Deserialize<GamePackageDefinition>(File.ReadAllText(packagePath), JsonOptions)
                          ?? throw new InvalidOperationException("regeneration_record.package_invalid");
            var authoring = new GameProjectFeatureModuleAuthoringService(_repositoryRoot);
            var state = operationLease is null
                ? authoring.OpenProject(projectFolder, package)
                : authoring.OpenProject(projectFolder, package, operationLease);
            var fingerprint = new FeatureModuleAuthoringFingerprintService().Calculate(state.Document, state.Library);
            if (!fingerprint.Passed
                || !string.Equals(fingerprint.Sha256, record.QualifiedAuthoringFingerprint, StringComparison.Ordinal)
                || !string.Equals(state.Document.LastActivatedProjectPackageSha256, record.NewPackageSha256,
                    StringComparison.Ordinal)
                || !string.Equals(state.Document.LastCompositionPackageSha256, record.NewCompositionPackageSha256,
                    StringComparison.Ordinal)
                || !string.Equals(state.Document.LastQualifiedFinalStateHash, record.NewFinalStateHash,
                    StringComparison.Ordinal)) diagnostics.Add("regeneration_record.authoring_mismatch");
            var historyPath = GameProjectFeatureModuleAuthoringService.ConfinedPath(projectFolder,
                UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot + "/"
                + record.CandidateBuildHistoryFileName);
            if (!File.Exists(historyPath)) diagnostics.Add("regeneration_record.history_missing");
            else
            {
                var history = JsonSerializer.Deserialize<GameProjectBuildHistoryEntry>(
                    File.ReadAllText(historyPath, Encoding.UTF8), JsonOptions);
                if (history is null
                    || history.Status != "GREEN"
                    || history.AttemptStatus != "GREEN"
                    || history.SchemaVersion is not GameProjectBuildHistoryReader.SchemaVersionV4
                        and not GameProjectBuildHistoryReader.SchemaVersionV5
                        and not GameProjectBuildHistoryReader.SchemaVersionV6
                        and not GameProjectBuildHistoryReader.SchemaVersionV7
                    || !string.Equals(history.PackageSha256, record.NewPackageSha256, StringComparison.Ordinal)
                    || !string.Equals(history.CompositionPackageSha256, record.NewCompositionPackageSha256,
                        StringComparison.Ordinal)
                    || !string.Equals(history.FinalStateHash, record.NewFinalStateHash, StringComparison.Ordinal))
                    diagnostics.Add("regeneration_record.history_mismatch");
            }
            return new GameProjectSeedRegenerationRecordReadResult
            {
                Present = true,
                Passed = diagnostics.Count == 0,
                Record = record,
                Diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList()
            };
        }
        catch (Exception exception) when (exception is IOException or JsonException or InvalidOperationException)
        {
            return Failed("regeneration_record.unreadable:" + exception.Message);
        }
    }

    internal static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    internal static string HashText(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static GameProjectSeedRegenerationRecordReadResult Failed(string diagnostic) => new()
    {
        Present = true,
        Diagnostics = [diagnostic]
    };
}
