using System.Text.Json;
using LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;

namespace LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;

public sealed class ChunkedRuntimePreviewExportSourceLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public ChunkedRuntimePreviewExportSourceBundle Load(string projectRootPath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var sourceDirectory = Path.GetFullPath(Path.Combine(projectRoot, RuntimeChunkDeltaEvidenceService.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, sourceDirectory);
        if (!Directory.Exists(sourceDirectory))
        {
            throw new DirectoryNotFoundException("Goal 039 source artifact directory was not found: " + sourceDirectory);
        }

        var planFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["frontier_survival"] = RuntimeChunkDeltaEvidenceService.FrontierPlanJsonFileName,
            ["gothic_intrigue"] = RuntimeChunkDeltaEvidenceService.GothicPlanJsonFileName,
            ["caravan_trade"] = RuntimeChunkDeltaEvidenceService.CaravanPlanJsonFileName,
            ["metamodule_kingdoms"] = RuntimeChunkDeltaEvidenceService.MetamodulePlanJsonFileName
        };
        var plans = new SortedDictionary<string, RuntimeChunkTraversalPlan>(StringComparer.Ordinal);
        var artifactText = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in planFiles)
        {
            var text = ReadRequired(sourceDirectory, pair.Value);
            artifactText[pair.Value] = text;
            plans[pair.Key] = ReadJson<RuntimeChunkTraversalPlan>(text);
        }

        var states = new SortedDictionary<string, RuntimeChunkDeltaStateSnapshot>(StringComparer.Ordinal);
        ReadOptionalState(sourceDirectory, RuntimeChunkDeltaEvidenceService.FrontierStateJsonFileName, states, artifactText);
        ReadOptionalState(sourceDirectory, RuntimeChunkDeltaEvidenceService.MetamoduleStateJsonFileName, states, artifactText);

        var saveLoadText = ReadRequired(sourceDirectory, RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName);
        var replayText = ReadRequired(sourceDirectory, RuntimeChunkDeltaEvidenceService.ReplayDeterminismProofJsonFileName);
        var invalidText = ReadRequired(sourceDirectory, RuntimeChunkDeltaEvidenceService.InvalidMatrixJsonFileName);
        artifactText[RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName] = saveLoadText;
        artifactText[RuntimeChunkDeltaEvidenceService.ReplayDeterminismProofJsonFileName] = replayText;
        artifactText[RuntimeChunkDeltaEvidenceService.InvalidMatrixJsonFileName] = invalidText;

        var hashes = artifactText
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .ToDictionary(item => item.Key, item => ChunkedRuntimePreviewExportHash.Hash(item.Value), StringComparer.Ordinal);

        return new ChunkedRuntimePreviewExportSourceBundle
        {
            SourceDirectoryRelativePath = RuntimeChunkDeltaEvidenceService.RelativeOutputDirectory,
            PlansByScenario = plans,
            StatesByScenario = states,
            SaveLoadProof = ReadJson<RuntimeChunkSaveLoadRoundtripProof>(saveLoadText),
            ReplayProof = ReadJson<RuntimeChunkReplayDeterminismProof>(replayText),
            SourceInvalidMatrix = ReadJson<RuntimeChunkInvalidMatrix>(invalidText),
            ArtifactTextByFileName = artifactText,
            ArtifactHashByFileName = hashes
        };
    }

    private static void ReadOptionalState(
        string sourceDirectory,
        string fileName,
        IDictionary<string, RuntimeChunkDeltaStateSnapshot> states,
        IDictionary<string, string> artifactText)
    {
        var path = Path.Combine(sourceDirectory, fileName);
        if (!File.Exists(path))
        {
            return;
        }

        var text = File.ReadAllText(path);
        artifactText[fileName] = text;
        var state = ReadJson<RuntimeChunkDeltaStateSnapshot>(text);
        if (!string.IsNullOrWhiteSpace(state.ScenarioId))
        {
            states[state.ScenarioId] = state;
        }
    }

    private static string ReadRequired(string directory, string fileName)
    {
        var path = Path.Combine(directory, fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Required Goal 039 source artifact was not found.", path);
        }

        return File.ReadAllText(path);
    }

    private static T ReadJson<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, JsonOptions)
        ?? throw new InvalidOperationException("Artifact JSON could not be deserialized as " + typeof(T).Name + ".");

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }
}

public sealed record ChunkedRuntimePreviewExportSourceBundle
{
    public string SourceDirectoryRelativePath { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, RuntimeChunkTraversalPlan> PlansByScenario { get; init; } = new Dictionary<string, RuntimeChunkTraversalPlan>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, RuntimeChunkDeltaStateSnapshot> StatesByScenario { get; init; } = new Dictionary<string, RuntimeChunkDeltaStateSnapshot>(StringComparer.Ordinal);
    public RuntimeChunkSaveLoadRoundtripProof SaveLoadProof { get; init; } = new();
    public RuntimeChunkReplayDeterminismProof ReplayProof { get; init; } = new();
    public RuntimeChunkInvalidMatrix SourceInvalidMatrix { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactTextByFileName { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ArtifactHashByFileName { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}
