using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class SelectedRuntimeVariantInteractiveSessionArtifactService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public T ReadJson<T>(string path) where T : class =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
        ?? throw new InvalidOperationException("Goal144 JSON could not be read: " + path);

    public async Task<IReadOnlyList<string>> WriteAsync(
        string repositoryRoot,
        string outputRoot,
        SelectedRuntimeVariantLiveSessionArtifactSet artifacts,
        CancellationToken cancellationToken)
    {
        var exportRoot = Resolve(
            repositoryRoot,
            SelectedRuntimeVariantInteractiveSessionVocabulary.ExportPackageDirectory);
        var roots = new[] { outputRoot, exportRoot };
        var written = new List<string>();
        foreach (var root in roots)
        {
            Directory.CreateDirectory(root);
            var files = BuildFiles(artifacts);
            foreach (var item in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var path = Path.Combine(root, item.Key);
                await WriteAtomicAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
                written.Add(Relative(repositoryRoot, path));
            }

            var index = new
            {
                schemaVersion = "selected_runtime_variant_live_session_file_index_v1",
                goalId = SelectedRuntimeVariantInteractiveSessionVocabulary.GoalId,
                fileCount = files.Count,
                files = files.Keys.OrderBy(name => name, StringComparer.Ordinal).Select(name =>
                {
                    var path = Path.Combine(root, name);
                    return new
                    {
                        relativePath = Relative(repositoryRoot, path),
                        sha256 = HashFile(path),
                        byteCount = new FileInfo(path).Length
                    };
                }).ToList()
            };
            var indexPath = Path.Combine(
                root,
                SelectedRuntimeVariantInteractiveSessionVocabulary.FileIndexFileName);
            await WriteAtomicAsync(indexPath, Serialize(index), cancellationToken).ConfigureAwait(false);
            written.Add(Relative(repositoryRoot, indexPath));
        }

        return written;
    }

    public static string HashFile(string path)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(sha.ComputeHash(stream)).ToLowerInvariant();
    }

    public static string HashText(string text)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    }

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions) + "\n";

    private static Dictionary<string, string> BuildFiles(
        SelectedRuntimeVariantLiveSessionArtifactSet artifacts)
    {
        var reportMarkdown = string.Join("\n",
        [
            "# Goal 144 Selected Runtime Variant Live Session",
            string.Empty,
            "Status: " + artifacts.Dashboard.Status,
            string.Empty,
            "- selectedCandidateId: `" + artifacts.Dashboard.SelectedCandidateId + "`",
            "- selectedVariantKind: `" + artifacts.Dashboard.SelectedVariantKind + "`",
            "- actionDescriptorCount: " + artifacts.Dashboard.ActionDescriptorCount,
            "- executedRuntimeActionCount: " + artifacts.Dashboard.ExecutedRuntimeActionCount,
            "- checkpointReloadByReplayPassed: " + artifacts.Dashboard.CheckpointReloadByReplayPassed.ToString().ToLowerInvariant(),
            "- fullReplayEquivalent: " + artifacts.Dashboard.FullReplayEquivalent.ToString().ToLowerInvariant(),
            "- finalStateHashMatchesGoal142: " + artifacts.Dashboard.FinalStateHashMatchesGoal142.ToString().ToLowerInvariant(),
            "- runtimeAuthority: true",
            "- projectionOnly: false",
            "- unityGameplayTruth: false",
            string.Empty
        ]);
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SelectedRuntimeVariantInteractiveSessionVocabulary.AcceptanceFileName] = Serialize(artifacts.Acceptance),
            [SelectedRuntimeVariantInteractiveSessionVocabulary.CatalogFileName] = Serialize(artifacts.Catalog),
            [SelectedRuntimeVariantInteractiveSessionVocabulary.StateFileName] = Serialize(artifacts.State),
            [SelectedRuntimeVariantInteractiveSessionVocabulary.JournalFileName] = Serialize(artifacts.Journal),
            [SelectedRuntimeVariantInteractiveSessionVocabulary.CheckpointFileName] = Serialize(artifacts.Checkpoint),
            [SelectedRuntimeVariantInteractiveSessionVocabulary.ReloadFileName] = Serialize(artifacts.CheckpointReload),
            [SelectedRuntimeVariantInteractiveSessionVocabulary.ReplayFileName] = Serialize(artifacts.FinalReplay),
            [SelectedRuntimeVariantInteractiveSessionVocabulary.DashboardFileName] = Serialize(artifacts.Dashboard),
            [SelectedRuntimeVariantInteractiveSessionVocabulary.NegativeProofFileName] = Serialize(artifacts.NegativeProof),
            [SelectedRuntimeVariantInteractiveSessionVocabulary.UnitySmokeFileName] = Serialize(artifacts.UnitySmoke),
            [SelectedRuntimeVariantInteractiveSessionVocabulary.ReportJsonFileName] = Serialize(artifacts.Dashboard),
            [SelectedRuntimeVariantInteractiveSessionVocabulary.ReportMarkdownFileName] = reportMarkdown
        };
    }

    private static async Task WriteAtomicAsync(
        string path,
        string content,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await File.WriteAllTextAsync(
                    temporary,
                    content,
                    new UTF8Encoding(false),
                    cancellationToken)
                .ConfigureAwait(false);
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static string Resolve(string root, string relative) =>
        Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');
}
