using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaHumanResultRevalidation;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;

public sealed partial class OfflineGeoworldAlphaPostAcceptanceContinuationSelectionService
{
    private const int RequiredLaneCount = 7;
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public OfflineGeoworldAlphaPostAcceptanceContinuationSelectionBuildResult Build(
        string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var dashboard = BuildDashboard(root);
        return BuildArtifacts(root, dashboard);
    }

    public async Task<OfflineGeoworldAlphaPostAcceptanceContinuationSelectionWriteResult>
        BuildAndWriteAsync(
            string repositoryRootPath,
            CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(
            root,
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ProceduralOutputDirectory);
        var export = Resolve(
            root,
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ExportPackageDirectory);
        var docsPath = Resolve(
            root,
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.DocumentationPath);
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

        return new OfflineGeoworldAlphaPostAcceptanceContinuationSelectionWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldAlphaPostAcceptanceContinuationDashboard BuildDashboard(string root)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var goal116 = LoadGoal116Evidence(root, errors);
        var goal115 = LoadGoal115DecisionSnapshot(root, errors);
        var lanes = BuildMatrixLanes();
        var goal116Valid = ValidateGoal116Evidence(goal116, errors)
                           && ValidateGoal115Snapshot(goal115, errors);

        return new OfflineGeoworldAlphaPostAcceptanceContinuationDashboard
        {
            ManualGateStatus = goal116.ManualGateStatus,
            HumanAccepted = goal116.HumanAccepted,
            SourceDecisionStatus = goal116.SourceDecisionStatus,
            ManualResultSha256 = goal116.ManualResultSha256,
            AcceptedByCodex = goal116.AcceptedByCodex,
            ManualInputNotCommitted = goal116.ManualInputNotCommitted,
            RawManualResultEmbeddedInArtifacts = goal116.RawManualResultEmbeddedInArtifacts,
            ReadyLaneCount = CountLanes(lanes, OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .StatusReady),
            CandidateLaneCount = lanes.Count(lane =>
                lane.Status.StartsWith("CANDIDATE_", StringComparison.Ordinal)),
            BlockedLaneCount = lanes.Count(lane =>
                lane.Status.StartsWith("BLOCKED_", StringComparison.Ordinal)),
            Goal116AcceptanceRecordPresent = goal116.AcceptanceRecordPresent,
            Goal116AcceptanceRecordValid = goal116Valid,
            Goal115DecisionSnapshotPresent = goal115.Present,
            Goal115DecisionSnapshotGreen = goal115.DecisionStatus
                                           == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                                               .SourceDecisionStatusGreenCandidate,
            LaneIds = lanes.Select(lane => lane.LaneId).ToList(),
            EvidenceArtifactPaths =
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .RequiredProceduralFileNames
                    .Select(file => OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                        .ProceduralOutputDirectory + "/" + file)
                    .ToList(),
            ExportArtifactPaths =
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .RequiredExportFileNames
                    .Select(file => OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                        .ExportPackageDirectory + "/" + file)
                    .ToList(),
            Errors = errors.Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList(),
            Warnings = warnings.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldAlphaPostAcceptanceContinuationSelectionBuildResult BuildArtifacts(
        string root,
        OfflineGeoworldAlphaPostAcceptanceContinuationDashboard dashboard)
    {
        var matrix = BuildMatrix(dashboard);
        var negative = BuildNegativeProof(root, matrix);
        var quality = BuildQualityGate(root, dashboard, matrix, negative);
        var report = RenderReport(dashboard, matrix, quality, negative);
        var docs = RenderDocumentation(dashboard, matrix, quality);
        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.MatrixFileName] =
                Serialize(matrix),
            [OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.ReportFileName] =
                report,
            [OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.QualityGateScanFileName] =
                Serialize(quality),
            [OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        var proceduralIndex = BuildFileIndex(
            proceduralFiles,
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.ProceduralOutputDirectory,
            "goal117_post_acceptance_continuation_selection_evidence",
            accepted: false);
        proceduralFiles[OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
            .FileIndexFileName] = Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.MatrixFileName] =
                Serialize(matrix),
            [OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.ReportFileName] =
                report,
            [OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.QualityGateScanFileName] =
                Serialize(quality),
            [OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.ExportReadmeFileName] =
                RenderExportReadme(dashboard)
        };
        var exportIndex = BuildFileIndex(
            exportFiles,
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.ExportPackageDirectory,
            "goal117_post_acceptance_continuation_selection_export",
            accepted: false);
        exportFiles[OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
            .FileIndexFileName] = Serialize(exportIndex);

        return new OfflineGeoworldAlphaPostAcceptanceContinuationSelectionBuildResult
        {
            Dashboard = dashboard,
            Matrix = matrix,
            QualityGateScan = quality,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationMarkdown = docs
        };
    }

    private static OfflineGeoworldAlphaPostAcceptanceContinuationFileIndex BuildFileIndex(
        IReadOnlyDictionary<string, string> files,
        string root,
        string role,
        bool accepted)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new OfflineGeoworldAlphaPostAcceptanceContinuationFileIndexEntry
            {
                RelativePath = root + "/" + item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        return new OfflineGeoworldAlphaPostAcceptanceContinuationFileIndex
        {
            Accepted = accepted,
            IndexedFileCount = entries.Count,
            Files = entries,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        };
    }

    private static int CountLanes(
        IReadOnlyList<OfflineGeoworldAlphaPostAcceptanceContinuationLane> lanes,
        string status) =>
        lanes.Count(lane => lane.Status == status);

    private static void Require(bool condition, string code, List<string> errors)
    {
        if (!condition)
        {
            errors.Add(code);
        }
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
            throw new InvalidOperationException("Goal117 must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static int CountLines(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;

}
