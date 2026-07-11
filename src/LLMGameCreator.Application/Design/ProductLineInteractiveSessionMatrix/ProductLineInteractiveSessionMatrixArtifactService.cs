using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.ProductLineInteractiveSessionMatrix;

public sealed class ProductLineInteractiveSessionMatrixArtifactService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<IReadOnlyList<string>> WriteAsync(
        string repositoryRoot,
        string outputRoot,
        ProductLineInteractiveSessionArtifactSet artifacts,
        IReadOnlyDictionary<string, ProductLineInteractiveSessionCandidateArtifacts> candidates,
        CancellationToken cancellationToken)
    {
        var exportRoot = Path.GetFullPath(Path.Combine(
            repositoryRoot,
            ProductLineInteractiveSessionMatrixVocabulary.ExportRoot.Replace('/', Path.DirectorySeparatorChar)));
        var written = new List<string>();
        foreach (var root in new[] { outputRoot, exportRoot })
        {
            Directory.CreateDirectory(root);
            var files = BuildFiles(artifacts, candidates);
            foreach (var file in files.OrderBy(item => item.Key, StringComparer.Ordinal))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var path = Path.Combine(root, file.Key.Replace('/', Path.DirectorySeparatorChar));
                await WriteAtomicAsync(path, file.Value, cancellationToken).ConfigureAwait(false);
                written.Add(Relative(repositoryRoot, path));
            }

            var index = new
            {
                schemaVersion = "product_line_interactive_session_file_index_v1",
                goalId = ProductLineInteractiveSessionMatrixVocabulary.GoalId,
                fileCount = files.Count,
                files = files.Keys.OrderBy(name => name, StringComparer.Ordinal).Select(name =>
                {
                    var path = Path.Combine(root, name.Replace('/', Path.DirectorySeparatorChar));
                    return new
                    {
                        relativePath = Relative(repositoryRoot, path),
                        sha256 = HashFile(path),
                        byteCount = new FileInfo(path).Length
                    };
                }).ToList()
            };
            var indexPath = Path.Combine(root, "product-line-interactive-session-file-index.json");
            await WriteAtomicAsync(indexPath, Serialize(index), cancellationToken).ConfigureAwait(false);
            written.Add(Relative(repositoryRoot, indexPath));
        }

        return written;
    }

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions) + "\n";

    public static string HashFile(string path)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(sha.ComputeHash(stream)).ToLowerInvariant();
    }

    private static Dictionary<string, string> BuildFiles(
        ProductLineInteractiveSessionArtifactSet artifacts,
        IReadOnlyDictionary<string, ProductLineInteractiveSessionCandidateArtifacts> candidates)
    {
        var files = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["goal144-human-acceptance-record.json"] = Serialize(artifacts.Acceptance),
            ["product-line-interactive-session-candidate-catalog.json"] = Serialize(artifacts.Catalog),
            ["product-line-interactive-session-matrix-result.json"] = Serialize(artifacts.Matrix),
            ["product-line-interactive-session-comparison.json"] = Serialize(artifacts.Comparison),
            ["product-line-interactive-session-dashboard.json"] = Serialize(artifacts.Dashboard),
            ["product-line-interactive-session-negative-proof.json"] = Serialize(artifacts.NegativeProof),
            ["product-line-interactive-session-selection-handoff.json"] = Serialize(artifacts.Selection),
            ["one-click-product-line-interactive-session-report.json"] = Serialize(artifacts),
            ["one-click-product-line-interactive-session-report.md"] = BuildMarkdown(artifacts),
            ["unity-product-line-interactive-session-matrix-smoke.json"] = Serialize(artifacts.UnitySmoke)
        };
        foreach (var pair in candidates.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var prefix = "candidates/" + pair.Key + "/";
            files[prefix + "session-state.json"] = Serialize(pair.Value.State);
            files[prefix + "action-catalog.json"] = Serialize(pair.Value.Catalog);
            files[prefix + "journal.json"] = Serialize(pair.Value.Journal);
            files[prefix + "checkpoint.json"] = Serialize(pair.Value.Checkpoint);
            files[prefix + "checkpoint-replay-result.json"] = Serialize(pair.Value.CheckpointReplay);
            files[prefix + "final-replay-result.json"] = Serialize(pair.Value.FinalReplay);
            files[prefix + "focus-effect-proof.json"] = Serialize(pair.Value.FocusProof);
        }

        return files;
    }

    private static string BuildMarkdown(ProductLineInteractiveSessionArtifactSet artifacts)
    {
        var lines = new List<string>
        {
            "# Goal 145 Product-Line Interactive Runtime Session Matrix",
            string.Empty,
            "Status: " + artifacts.Dashboard.Status,
            string.Empty,
            "- candidateCount: " + artifacts.Dashboard.CandidateCount,
            "- passedCandidateCount: " + artifacts.Dashboard.PassedCandidateCount,
            "- failedCandidateCount: " + artifacts.Dashboard.FailedCandidateCount,
            "- distinctFinalStateHashCount: " + artifacts.Dashboard.DistinctFinalStateHashCount,
            "- activeSelectedCandidateId: `" + artifacts.Dashboard.ActiveSelectedCandidateId + "`",
            "- allCandidateCheckpointReloadsPassed: " + Lower(artifacts.Dashboard.AllCandidateCheckpointReloadsPassed),
            "- allCandidateFullReplaysEquivalent: " + Lower(artifacts.Dashboard.AllCandidateFullReplaysEquivalent),
            "- allCandidateActionBindingsPassed: " + Lower(artifacts.Dashboard.AllCandidateActionBindingsPassed),
            "- allFocusEffectsObserved: " + Lower(artifacts.Dashboard.AllFocusEffectsObserved),
            "- operatorUsesInProcessService: true",
            "- runtimeAuthority: true",
            "- projectionOnly: false",
            "- unityGameplayTruth: false",
            "- goal144Accepted: true",
            "- goal145Accepted: false",
            string.Empty,
            "## Fresh focus comparisons",
            string.Empty
        };
        lines.AddRange(artifacts.Comparison.Comparisons.Select(comparison =>
            "- `" + comparison.CandidateId + "` / " + comparison.ComparedDimension
            + ": baseline=`" + comparison.BaselineValue + "`; candidate=`" + comparison.CandidateValue
            + "`; observed=" + Lower(comparison.FocusEffectObserved)));
        lines.Add(string.Empty);
        return string.Join("\n", lines);
    }

    private static string Lower(bool value) => value.ToString().ToLowerInvariant();

    private static async Task WriteAtomicAsync(string path, string content, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await File.WriteAllTextAsync(temporary, content, new UTF8Encoding(false), cancellationToken)
                .ConfigureAwait(false);
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');
}
