using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class SelectedRuntimeVariantPlayerAdapterArtifactService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static SelectedRuntimeVariantPlayerAdapterArtifactService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public T ReadJson<T>(string path) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
        ?? throw new InvalidOperationException("JSON file could not be deserialized: " + path);

    public string Serialize(object value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    public string HashSerialized(object value) => HashText(Serialize(value));

    public SelectedRuntimeVariantPlayerAdapterDashboard ReadDashboard(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var path = Path.Combine(
            root,
            SelectedRuntimeVariantPlayerAdapterVocabulary.DashboardRelativePath
                .Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(path)
            ? ReadJson<SelectedRuntimeVariantPlayerAdapterDashboard>(path)
            : new SelectedRuntimeVariantPlayerAdapterDashboard
            {
                Status = "NOT_BUILT"
            };
    }

    public async Task<SelectedRuntimeVariantPlayerAdapterWriteResult> WriteAsync(
        string repositoryRootPath,
        string outputRootPath,
        SelectedRuntimeVariantPlayerAdapterArtifactSet artifacts,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var outputRoot = Path.GetFullPath(outputRootPath);
        var exportRoot = Path.Combine(
            root,
            SelectedRuntimeVariantPlayerAdapterVocabulary.ExportPackageDirectory
                .Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(outputRoot);
        Directory.CreateDirectory(exportRoot);

        var report = new
        {
            schemaVersion = "one_click_selected_runtime_variant_playeradapter_report_v1",
            goalId = SelectedRuntimeVariantPlayerAdapterVocabulary.GoalId,
            artifacts.Dashboard,
            artifacts.Handoff,
            artifacts.Model,
            artifacts.Result,
            artifacts.NegativeProof,
            artifacts.UnitySmoke
        };
        var payloads = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SelectedRuntimeVariantPlayerAdapterVocabulary.AcceptanceFileName] =
                Serialize(artifacts.Acceptance),
            [SelectedRuntimeVariantPlayerAdapterVocabulary.HandoffFileName] =
                Serialize(artifacts.Handoff),
            [SelectedRuntimeVariantPlayerAdapterVocabulary.ModelFileName] =
                Serialize(artifacts.Model),
            [SelectedRuntimeVariantPlayerAdapterVocabulary.FramesFileName] =
                Serialize(artifacts.Frames),
            [SelectedRuntimeVariantPlayerAdapterVocabulary.ResultFileName] =
                Serialize(artifacts.Result),
            [SelectedRuntimeVariantPlayerAdapterVocabulary.DashboardFileName] =
                Serialize(artifacts.Dashboard),
            [SelectedRuntimeVariantPlayerAdapterVocabulary.NegativeProofFileName] =
                Serialize(artifacts.NegativeProof),
            [SelectedRuntimeVariantPlayerAdapterVocabulary.UnitySmokeFileName] =
                Serialize(artifacts.UnitySmoke),
            [SelectedRuntimeVariantPlayerAdapterVocabulary.OneClickReportJsonFileName] =
                Serialize(report),
            [SelectedRuntimeVariantPlayerAdapterVocabulary.OneClickReportMarkdownFileName] =
                RenderReport(artifacts)
        };

        foreach (var item in payloads)
        {
            await WriteTextAsync(
                    Path.Combine(outputRoot, item.Key),
                    item.Value,
                    cancellationToken)
                .ConfigureAwait(false);
            await WriteTextAsync(
                    Path.Combine(exportRoot, item.Key),
                    item.Value,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        await WriteFileIndexAsync(root, outputRoot, cancellationToken).ConfigureAwait(false);
        await WriteFileIndexAsync(root, exportRoot, cancellationToken).ConfigureAwait(false);

        var written = Directory
            .EnumerateFiles(outputRoot, "*", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(exportRoot, "*", SearchOption.AllDirectories))
            .Select(path => Relative(root, path))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();
        return new SelectedRuntimeVariantPlayerAdapterWriteResult
        {
            Dashboard = artifacts.Dashboard,
            Handoff = artifacts.Handoff,
            Model = artifacts.Model,
            Frames = artifacts.Frames,
            Result = artifacts.Result,
            NegativeProof = artifacts.NegativeProof,
            UnitySmoke = artifacts.UnitySmoke,
            ProceduralOutputDirectoryPath = outputRoot,
            ExportPackageDirectoryPath = exportRoot,
            WrittenFiles = written
        };
    }

    private async Task WriteFileIndexAsync(
        string repositoryRoot,
        string artifactRoot,
        CancellationToken cancellationToken)
    {
        var entries = Directory
            .EnumerateFiles(artifactRoot, "*", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(
                SelectedRuntimeVariantPlayerAdapterVocabulary.FileIndexFileName,
                StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => new SelectedRuntimeVariantPlayerAdapterFileIndexEntry
            {
                RelativePath = Relative(repositoryRoot, path),
                Role = RoleFor(path),
                Sha256 = HashFile(path)
            })
            .ToList();
        var index = new SelectedRuntimeVariantPlayerAdapterFileIndex
        {
            RootPath = Relative(repositoryRoot, artifactRoot),
            IndexedFileCount = entries.Count,
            Files = entries
        };
        await WriteTextAsync(
                Path.Combine(
                    artifactRoot,
                    SelectedRuntimeVariantPlayerAdapterVocabulary.FileIndexFileName),
                Serialize(index),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static string RenderReport(SelectedRuntimeVariantPlayerAdapterArtifactSet artifacts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Goal143 Selected Runtime Variant PlayerAdapter Handoff");
        builder.AppendLine();
        builder.AppendLine("- status: " + artifacts.Dashboard.Status);
        builder.AppendLine("- goal142Accepted: true");
        builder.AppendLine("- candidateId: " + artifacts.Model.CandidateId);
        builder.AppendLine("- variantKind: " + artifacts.Model.VariantKind);
        builder.AppendLine("- score: " + artifacts.Model.Score);
        builder.AppendLine("- packageHashMatch: "
                           + artifacts.Dashboard.PackageHashMatch.ToString().ToLowerInvariant());
        builder.AppendLine("- finalStateHashMatch: "
                           + artifacts.Dashboard.FinalStateHashMatch.ToString().ToLowerInvariant());
        builder.AppendLine("- frameCount: " + artifacts.Model.FrameCount);
        builder.AppendLine("- requestCount: " + artifacts.Model.RequestCount);
        builder.AppendLine("- snapshotCount: " + artifacts.Model.SnapshotCount);
        builder.AppendLine("- selectedVariantEffectVisible: "
                           + artifacts.Model.SelectedVariantEffectVisible
                               .ToString().ToLowerInvariant());
        builder.AppendLine("- noBalancedBaselineFallback: "
                           + artifacts.Model.NoBalancedBaselineFallback
                               .ToString().ToLowerInvariant());
        builder.AppendLine("- unitySmokePassed: "
                           + artifacts.UnitySmoke.Passed.ToString().ToLowerInvariant());
        builder.AppendLine("- runtimeAuthority: true");
        builder.AppendLine("- projectionOnly: false");
        builder.AppendLine("- unityGameplayTruth: false");
        builder.AppendLine("- accepted: false");
        return builder.ToString();
    }

    private static string RoleFor(string path)
    {
        var name = Path.GetFileName(path);
        return name switch
        {
            SelectedRuntimeVariantPlayerAdapterVocabulary.AcceptanceFileName =>
                "goal142_human_acceptance",
            SelectedRuntimeVariantPlayerAdapterVocabulary.HandoffFileName =>
                "selected_variant_playeradapter_handoff",
            SelectedRuntimeVariantPlayerAdapterVocabulary.ModelFileName =>
                "selected_variant_playeradapter_model",
            SelectedRuntimeVariantPlayerAdapterVocabulary.FramesFileName =>
                "selected_variant_playeradapter_frames",
            SelectedRuntimeVariantPlayerAdapterVocabulary.UnitySmokeFileName =>
                "unity_playeradapter_consumer_smoke",
            _ => "selected_variant_playeradapter_evidence"
        };
    }

    private static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
    }

    public static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(stream)).ToLowerInvariant();
    }

    private static string HashText(string text)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(text)))
            .ToLowerInvariant();
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');
}
