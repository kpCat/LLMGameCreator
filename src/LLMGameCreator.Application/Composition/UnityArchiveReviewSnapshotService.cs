using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveReviewSnapshotService
{
    public const string ReviewJsonRelativePath = "production/archive-review.json";
    public const string ReviewMarkdownRelativePath = "production/archive-review.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly UnityArchiveReviewSnapshotMarkdownRenderer _markdownRenderer;

    public UnityArchiveReviewSnapshotService(UnityArchiveReviewSnapshotMarkdownRenderer? markdownRenderer = null)
    {
        _markdownRenderer = markdownRenderer ?? new UnityArchiveReviewSnapshotMarkdownRenderer();
    }

    public async Task<UnityArchiveReviewSnapshotResult> ReviewAsync(
        UnityArchiveReviewSnapshotRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.ArchiveDirectoryPath))
        {
            throw new ArgumentException("Archive directory path is required.", nameof(request));
        }

        var archiveRoot = Path.GetFullPath(request.ArchiveDirectoryPath);
        var diagnostics = new List<UnityArchiveReviewSnapshotDiagnostic>();
        var written = new List<string>();

        if (!Directory.Exists(archiveRoot))
        {
            diagnostics.Add(Diagnostic(
                UnityArchiveExportDiagnosticSeverity.Error,
                "unity.archive_review.missing_archive_directory",
                "Unity archive directory does not exist.",
                string.Empty,
                string.Empty));

            return new UnityArchiveReviewSnapshotResult
            {
                ArchiveDirectoryPath = archiveRoot,
                Report = BuildReport(
                    archiveRoot,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    diagnostics),
                WrittenRelativePaths = written
            };
        }

        var validation = await ReadJsonAsync<UnityArchiveMaterializationValidationReport>(
            archiveRoot,
            UnityArchiveMaterializationService.ValidationFilePath,
            diagnostics,
            required: true,
            cancellationToken).ConfigureAwait(false);

        var readinessReport = await ReadJsonAsync<UnityArchiveProviderReadinessReport>(
            archiveRoot,
            "production/readiness-report.json",
            diagnostics,
            required: true,
            cancellationToken).ConfigureAwait(false);

        var fulfillmentState = await ReadJsonAsync<UnityArchiveFulfillmentStateReport>(
            archiveRoot,
            "production/fulfillment-state.json",
            diagnostics,
            required: true,
            cancellationToken).ConfigureAwait(false);

        var invalidOutputs = await ReadJsonAsync<UnityArchiveInvalidOutputsReport>(
            archiveRoot,
            "production/invalid-outputs.json",
            diagnostics,
            required: true,
            cancellationToken).ConfigureAwait(false);

        var assetRequests = await ReadJsonAsync<UnityArchiveAssetRequestsIndex>(
            archiveRoot,
            "assets/asset-requests.json",
            diagnostics,
            required: false,
            cancellationToken).ConfigureAwait(false);

        var audioRequests = await ReadJsonAsync<UnityArchiveAudioRequestsIndex>(
            archiveRoot,
            "audio/audio-requests.json",
            diagnostics,
            required: false,
            cancellationToken).ConfigureAwait(false);

        var luaRequests = await ReadJsonAsync<UnityArchiveLuaModuleRequests>(
            archiveRoot,
            "lua/module-requests.json",
            diagnostics,
            required: false,
            cancellationToken).ConfigureAwait(false);

        AppendValidationDiagnostics(validation, diagnostics);
        AppendProviderDiagnostics(readinessReport, diagnostics);
        AppendFulfillmentDiagnostics(fulfillmentState, diagnostics);

        var batches = await ReadProviderBatchesAsync(archiveRoot, diagnostics, cancellationToken).ConfigureAwait(false);
        var report = BuildReport(
            archiveRoot,
            validation,
            readinessReport,
            fulfillmentState,
            invalidOutputs,
            assetRequests,
            audioRequests,
            luaRequests,
            diagnostics,
            batches);

        if (request.WriteReviewFiles)
        {
            await WriteJsonFileAsync(archiveRoot, ReviewJsonRelativePath, report, cancellationToken).ConfigureAwait(false);
            written.Add(ReviewJsonRelativePath);

            await WriteTextFileAsync(
                archiveRoot,
                ReviewMarkdownRelativePath,
                _markdownRenderer.Render(report),
                cancellationToken).ConfigureAwait(false);
            written.Add(ReviewMarkdownRelativePath);
        }

        return new UnityArchiveReviewSnapshotResult
        {
            ArchiveDirectoryPath = archiveRoot,
            Report = report,
            WrittenRelativePaths = written
        };
    }

    private static async Task<T?> ReadJsonAsync<T>(
        string archiveRoot,
        string relativePath,
        ICollection<UnityArchiveReviewSnapshotDiagnostic> diagnostics,
        bool required,
        CancellationToken cancellationToken)
    {
        var fullPath = GetArchivePath(archiveRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            if (required)
            {
                diagnostics.Add(Diagnostic(
                    UnityArchiveExportDiagnosticSeverity.Error,
                    "unity.archive_review.missing_required_file",
                    $"Required archive file '{relativePath}' is missing.",
                    relativePath,
                    relativePath));
            }

            return default;
        }

        try
        {
            await using var stream = File.OpenRead(fullPath);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Diagnostic(
                UnityArchiveExportDiagnosticSeverity.Error,
                "unity.archive_review.invalid_json",
                $"Archive file '{relativePath}' is not valid JSON: {exception.Message}",
                relativePath,
                relativePath));
            return default;
        }
    }

    private static async Task<IReadOnlyList<UnityArchiveProviderJobBatch>> ReadProviderBatchesAsync(
        string archiveRoot,
        ICollection<UnityArchiveReviewSnapshotDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var paths = new[]
        {
            "providers/manual-import/jobs.json",
            "providers/comfyui/jobs.json",
            "providers/suno/jobs.json",
            "providers/local-audio/jobs.json",
            "providers/procedural/jobs.json"
        };

        var batches = new List<UnityArchiveProviderJobBatch>();
        foreach (var path in paths)
        {
            var batch = await ReadJsonAsync<UnityArchiveProviderJobBatch>(
                archiveRoot,
                path,
                diagnostics,
                required: false,
                cancellationToken).ConfigureAwait(false);

            if (batch != null)
            {
                batches.Add(batch);
            }
        }

        return batches
            .OrderBy(batch => batch.ProviderKind.ToString(), StringComparer.OrdinalIgnoreCase)
            .ThenBy(batch => batch.ProviderKind.ToString(), StringComparer.Ordinal)
            .ToList();
    }

    private static void AppendValidationDiagnostics(
        UnityArchiveMaterializationValidationReport? validation,
        ICollection<UnityArchiveReviewSnapshotDiagnostic> diagnostics)
    {
        if (validation == null)
        {
            return;
        }

        foreach (var diagnostic in validation.Diagnostics)
        {
            diagnostics.Add(Diagnostic(
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.TargetId,
                UnityArchiveMaterializationService.ValidationFilePath));
        }
    }

    private static void AppendProviderDiagnostics(
        UnityArchiveProviderReadinessReport? readiness,
        ICollection<UnityArchiveReviewSnapshotDiagnostic> diagnostics)
    {
        if (readiness == null)
        {
            return;
        }

        foreach (var diagnostic in readiness.Diagnostics)
        {
            diagnostics.Add(Diagnostic(
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.TargetId,
                "production/readiness-report.json"));
        }
    }

    private static void AppendFulfillmentDiagnostics(
        UnityArchiveFulfillmentStateReport? fulfillmentState,
        ICollection<UnityArchiveReviewSnapshotDiagnostic> diagnostics)
    {
        if (fulfillmentState == null)
        {
            return;
        }

        foreach (var diagnostic in fulfillmentState.Diagnostics)
        {
            diagnostics.Add(Diagnostic(
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.TargetId,
                "production/fulfillment-state.json"));
        }
    }

    private static UnityArchiveReviewSnapshotReport BuildReport(
        string archiveRoot,
        UnityArchiveMaterializationValidationReport? validation,
        UnityArchiveProviderReadinessReport? readiness,
        UnityArchiveFulfillmentStateReport? fulfillmentState,
        UnityArchiveInvalidOutputsReport? invalidOutputs,
        UnityArchiveAssetRequestsIndex? assetRequests,
        UnityArchiveAudioRequestsIndex? audioRequests,
        UnityArchiveLuaModuleRequests? luaRequests,
        IReadOnlyList<UnityArchiveReviewSnapshotDiagnostic> diagnostics,
        IReadOnlyList<UnityArchiveProviderJobBatch>? providerBatches = null)
    {
        var orderedDiagnostics = diagnostics
            .Distinct()
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.SourceFile, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.TargetId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var sourceFiles = EnumerateSourceFiles(archiveRoot);
        var batches = (providerBatches ?? Array.Empty<UnityArchiveProviderJobBatch>())
            .Select(batch => new UnityArchiveReviewProviderBatchSummary
            {
                ProviderKind = batch.ProviderKind,
                JobCount = batch.Jobs.Count,
                ExecutionEnabled = batch.ExecutionEnabled
            })
            .OrderBy(item => item.ProviderKind.ToString(), StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.ProviderKind.ToString(), StringComparer.Ordinal)
            .ToList();

        return new UnityArchiveReviewSnapshotReport
        {
            Readiness = DetermineReadiness(validation, orderedDiagnostics),
            Validation = new UnityArchiveReviewSnapshotValidationSummary
            {
                ExportValidationPresent = validation != null,
                MaterializationReadiness = validation?.Readiness ?? UnityArchiveMaterializationReadiness.Invalid,
                DryRunReadiness = validation?.DryRunReadiness ?? UnityArchiveExportReadiness.Invalid,
                MaterializedFileCount = validation?.MaterializedFiles.Count ?? 0
            },
            Providers = new UnityArchiveReviewSnapshotProviderSummary
            {
                ReadinessReportPresent = readiness != null,
                Readiness = readiness?.Readiness ?? UnityArchiveProviderPlanReadiness.BlockedByErrors,
                AssetSlotCount = readiness?.AssetSlotCount ?? 0,
                AudioSlotCount = readiness?.AudioSlotCount ?? 0,
                LuaModuleSlotCount = readiness?.LuaModuleSlotCount ?? 0,
                ProviderJobCount = readiness?.ProviderJobCount ?? 0,
                Batches = batches
            },
            Fulfillment = new UnityArchiveReviewSnapshotFulfillmentSummary
            {
                FulfillmentStatePresent = fulfillmentState != null,
                InvalidOutputsPresent = invalidOutputs != null,
                TotalSlotCount = fulfillmentState?.TotalSlotCount ?? 0,
                MissingCount = fulfillmentState?.MissingCount ?? 0,
                AvailableCount = fulfillmentState?.AvailableCount ?? 0,
                InvalidCount = fulfillmentState?.InvalidCount ?? 0,
                InvalidOutputCount = invalidOutputs?.InvalidOutputs.Count ?? 0,
                InvalidReasons = BuildInvalidReasonSummaries(invalidOutputs)
            },
            Requests = new UnityArchiveReviewSnapshotRequestSummary
            {
                AssetRequestsPresent = assetRequests != null,
                AudioRequestsPresent = audioRequests != null,
                LuaModuleRequestsPresent = luaRequests != null,
                AssetRequestCount = assetRequests?.Requests.Count ?? 0,
                AudioRequestCount = audioRequests?.Requests.Count ?? 0,
                LuaModuleRequestCount = luaRequests?.Requests.Count ?? 0
            },
            SourceFileCount = sourceFiles.Count,
            DiagnosticCount = orderedDiagnostics.Count,
            ErrorCount = orderedDiagnostics.Count(item => item.Severity == UnityArchiveExportDiagnosticSeverity.Error),
            WarningCount = orderedDiagnostics.Count(item => item.Severity == UnityArchiveExportDiagnosticSeverity.Warning),
            InfoCount = orderedDiagnostics.Count(item => item.Severity == UnityArchiveExportDiagnosticSeverity.Info),
            Diagnostics = orderedDiagnostics,
            SourceFiles = sourceFiles
        };
    }

    private static UnityArchiveReviewSnapshotReadiness DetermineReadiness(
    UnityArchiveMaterializationValidationReport? validation,
    IReadOnlyList<UnityArchiveReviewSnapshotDiagnostic> diagnostics)
    {
        if (diagnostics.Any(diagnostic =>
                diagnostic.Code is "unity.archive_review.missing_archive_directory" or
                    "unity.archive_review.missing_required_file"))
        {
            return UnityArchiveReviewSnapshotReadiness.MissingArchive;
        }

        if (diagnostics.Any(diagnostic =>
                diagnostic.Code == "unity.archive_review.invalid_json"))
        {
            return UnityArchiveReviewSnapshotReadiness.Invalid;
        }

        if (validation == null)
        {
            return UnityArchiveReviewSnapshotReadiness.MissingArchive;
        }

        if (validation.Readiness == UnityArchiveMaterializationReadiness.Invalid)
        {
            return UnityArchiveReviewSnapshotReadiness.Invalid;
        }

        if (validation.Readiness == UnityArchiveMaterializationReadiness.Blocked ||
            diagnostics.Any(diagnostic => diagnostic.Severity == UnityArchiveExportDiagnosticSeverity.Error))
        {
            return UnityArchiveReviewSnapshotReadiness.Blocked;
        }

        if (validation.Readiness is UnityArchiveMaterializationReadiness.MaterializedWithWarnings or
            UnityArchiveMaterializationReadiness.MaterializedMetadataOnly ||
            diagnostics.Any(diagnostic => diagnostic.Severity == UnityArchiveExportDiagnosticSeverity.Warning))
        {
            return UnityArchiveReviewSnapshotReadiness.ReadyWithWarnings;
        }

        return UnityArchiveReviewSnapshotReadiness.Ready;
    }

    private static IReadOnlyList<UnityArchiveReviewInvalidOutputReasonSummary> BuildInvalidReasonSummaries(
        UnityArchiveInvalidOutputsReport? invalidOutputs)
    {
        if (invalidOutputs == null)
        {
            return Array.Empty<UnityArchiveReviewInvalidOutputReasonSummary>();
        }

        return invalidOutputs.InvalidOutputs
            .GroupBy(item => item.Reason, StringComparer.OrdinalIgnoreCase)
            .Select(group => new UnityArchiveReviewInvalidOutputReasonSummary
            {
                Reason = group.Key,
                Count = group.Count()
            })
            .OrderBy(item => item.Reason, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Reason, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<UnityArchiveReviewSnapshotFileReference> EnumerateSourceFiles(string archiveRoot)
    {
        if (!Directory.Exists(archiveRoot))
        {
            return Array.Empty<UnityArchiveReviewSnapshotFileReference>();
        }

        return Directory.EnumerateFiles(archiveRoot, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(archiveRoot, path).Replace('\\', '/'))
            .Where(path => !string.Equals(path, ReviewJsonRelativePath, StringComparison.OrdinalIgnoreCase))
            .Where(path => !string.Equals(path, ReviewMarkdownRelativePath, StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ThenBy(path => path, StringComparer.Ordinal)
            .Select(path => new UnityArchiveReviewSnapshotFileReference
            {
                RelativePath = path,
                Kind = InferKind(path)
            })
            .ToList();
    }

    private static string InferKind(string relativePath)
    {
        return relativePath switch
        {
            UnityArchiveMaterializationService.ValidationFilePath => "validation_report",
            "production/readiness-report.json" => "provider_readiness_report",
            "production/fulfillment-state.json" => "fulfillment_state",
            "production/invalid-outputs.json" => "invalid_outputs",
            _ when relativePath.StartsWith("providers/", StringComparison.OrdinalIgnoreCase) => "provider_jobs",
            _ when relativePath.StartsWith("assets/", StringComparison.OrdinalIgnoreCase) => "asset_metadata",
            _ when relativePath.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) => "audio_metadata",
            _ when relativePath.StartsWith("lua/", StringComparison.OrdinalIgnoreCase) => "lua_metadata",
            _ when relativePath.StartsWith("manifest/", StringComparison.OrdinalIgnoreCase) => "archive_manifest",
            _ when relativePath.StartsWith("composition/", StringComparison.OrdinalIgnoreCase) => "composition_metadata",
            _ => "archive_file"
        };
    }

    private static async Task WriteJsonFileAsync<T>(
        string archiveRoot,
        string relativePath,
        T value,
        CancellationToken cancellationToken)
    {
        var path = GetArchivePath(archiveRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await File.WriteAllTextAsync(path, json, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
    }

    private static async Task WriteTextFileAsync(
        string archiveRoot,
        string relativePath,
        string content,
        CancellationToken cancellationToken)
    {
        var path = GetArchivePath(archiveRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, content, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
    }

    private static string GetArchivePath(string archiveRoot, string relativePath)
    {
        if (Path.IsPathRooted(relativePath) || relativePath.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Archive review relative path is unsafe: {relativePath}");
        }

        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(archiveRoot, normalized));
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(archiveRoot));
        if (!string.Equals(root, path, StringComparison.OrdinalIgnoreCase) &&
            !path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Archive review path must stay under '{root}'.");
        }

        return path;
    }

    private static UnityArchiveReviewSnapshotDiagnostic Diagnostic(
        UnityArchiveExportDiagnosticSeverity severity,
        string code,
        string message,
        string targetId,
        string sourceFile)
    {
        return new UnityArchiveReviewSnapshotDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            TargetId = targetId,
            SourceFile = sourceFile
        };
    }

    private static int SeverityOrder(UnityArchiveExportDiagnosticSeverity severity)
    {
        return severity switch
        {
            UnityArchiveExportDiagnosticSeverity.Error => 0,
            UnityArchiveExportDiagnosticSeverity.Warning => 1,
            _ => 2
        };
    }
}
