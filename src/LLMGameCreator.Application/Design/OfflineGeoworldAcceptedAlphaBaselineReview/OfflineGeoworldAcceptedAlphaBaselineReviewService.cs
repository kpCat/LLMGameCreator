using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAcceptedAlphaBaselineReview;

public sealed partial class OfflineGeoworldAcceptedAlphaBaselineReviewService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public OfflineGeoworldAcceptedAlphaBaselineReviewBuildResult Build(
        string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var sourceIndex = BuildSourceIndex(root);
        var dashboard = BuildDashboard(root, sourceIndex);
        var negative = BuildNegativeProof();
        var quality = BuildQualityGate(root, dashboard, sourceIndex, negative);
        var report = RenderReport(dashboard, sourceIndex, quality, negative);
        var docs = RenderDocumentation(dashboard, sourceIndex, quality);
        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceIndexFileName] =
                Serialize(sourceIndex),
            [OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ReportFileName] = report,
            [OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.QualityGateScanFileName] =
                Serialize(quality),
            [OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        var manifest = BuildManifest(dashboard, sourceIndex, quality, proceduralFiles);
        proceduralFiles[OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ManifestFileName] =
            Serialize(manifest);
        var proceduralIndex = BuildFileIndex(
            proceduralFiles,
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ProceduralOutputDirectory,
            "goal118_accepted_alpha_baseline_review_evidence");
        proceduralFiles[OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceIndexFileName] =
                Serialize(sourceIndex),
            [OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ReportFileName] = report,
            [OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.QualityGateScanFileName] =
                Serialize(quality),
            [OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ManifestFileName] =
                Serialize(manifest)
        };
        var exportIndex = BuildFileIndex(
            exportFiles,
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ExportPackageDirectory,
            "goal118_accepted_alpha_baseline_review_export");
        exportFiles[OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new OfflineGeoworldAcceptedAlphaBaselineReviewBuildResult
        {
            Dashboard = dashboard,
            Manifest = manifest,
            SourceIndex = sourceIndex,
            QualityGateScan = quality,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationMarkdown = docs
        };
    }

    public async Task<OfflineGeoworldAcceptedAlphaBaselineReviewWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(
            root,
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ProceduralOutputDirectory);
        var export = Resolve(
            root,
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(
            root,
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in result.ProceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in result.ExportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, result.DocumentationMarkdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new OfflineGeoworldAcceptedAlphaBaselineReviewWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldAcceptedAlphaBaselineManifest BuildManifest(
        OfflineGeoworldAcceptedAlphaBaselineDashboard dashboard,
        OfflineGeoworldAcceptedAlphaBaselineSourceIndex sourceIndex,
        OfflineGeoworldAcceptedAlphaBaselineQualityGateScan quality,
        IReadOnlyDictionary<string, string> proceduralFiles) =>
        new()
        {
            BaselineHash = dashboard.BaselineHash,
            AcceptedBaselineReady = dashboard.AcceptedBaselineReady,
            ManualGateStatus = dashboard.ManualGateStatus,
            ManualResultSha256 = dashboard.ManualResultSha256,
            IncludedSourceGoalCount = sourceIndex.IncludedSourceGoalCount,
            AcceptedEvidenceRootCount = dashboard.AcceptedEvidenceRootCount,
            ProducedOnlyRootCount = dashboard.ProducedOnlyRootCount,
            NotFinalReleaseOrRuntimeBuild = quality.NotFinalReleaseOrRuntimeBuild,
            NoRuntimeProviderOrNetworkChanges = quality.NoRuntimeProviderOrNetworkChanges,
            NoUnityFileChangesRequired = quality.NoUnityFileChangesRequired,
            SourceIndexSha256 = HashText(
                proceduralFiles[OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceIndexFileName]),
            DashboardSha256 = HashText(
                proceduralFiles[OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.DashboardFileName])
        };

    private static OfflineGeoworldAcceptedAlphaBaselineFileIndex BuildFileIndex(
        IReadOnlyDictionary<string, string> files,
        string root,
        string role)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new OfflineGeoworldAcceptedAlphaBaselineFileIndexEntry
            {
                RelativePath = root + "/" + item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        return new OfflineGeoworldAcceptedAlphaBaselineFileIndex
        {
            IndexedFileCount = entries.Count,
            Files = entries,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        };
    }

    private static string ResolveRepositoryRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Repository root path is required.", nameof(path));
        }

        return Path.GetFullPath(path);
    }

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    private static string Resolve(string root, string relativePath) =>
        Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)
                                  ?? throw new InvalidOperationException("Missing directory."));
        await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
    }

    private static void GuardNotManualInput(string root, string path)
    {
        var relative = Relative(root, path);
        if (relative.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Goal118 must not write the manual input path.");
        }
    }

    private static void Require(bool condition, string code, List<string> errors)
    {
        if (!condition)
        {
            errors.Add(code);
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static int CountLines(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;
}
