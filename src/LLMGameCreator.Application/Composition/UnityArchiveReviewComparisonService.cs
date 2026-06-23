using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveReviewComparisonService
{
    private const string HistoryDirectoryRelativePath = "review-history";
    private const string HistoryIndexRelativePath = "production/archive-review-history-index.json";
    private const string ReviewJsonRelativePath = "production/archive-review.json";
    private const string ComparisonJsonRelativePath = "production/archive-review-comparison.json";
    private const string ComparisonMarkdownRelativePath = "production/archive-review-comparison.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly UnityArchiveReviewComparisonMarkdownRenderer _markdownRenderer;

    public UnityArchiveReviewComparisonService(UnityArchiveReviewComparisonMarkdownRenderer? markdownRenderer = null)
    {
        _markdownRenderer = markdownRenderer ?? new UnityArchiveReviewComparisonMarkdownRenderer();
    }

    public async Task<UnityArchiveReviewComparisonResult> CompareAsync(
        UnityArchiveReviewComparisonRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.ArchiveDirectoryPath))
        {
            throw new ArgumentException("Archive directory path is required.", nameof(request));
        }

        var archiveRoot = Path.GetFullPath(request.ArchiveDirectoryPath);
        var written = new List<string>();

        var reviewPath = GetArchivePath(archiveRoot, ReviewJsonRelativePath);
        if (!Directory.Exists(archiveRoot))
        {
            return await WriteComparisonAsync(
                archiveRoot,
                UnityArchiveReviewComparisonReadiness.MissingReview,
                string.Empty,
                string.Empty,
                new UnityArchiveReviewComparisonSummary(),
                Array.Empty<UnityArchiveReviewComparisonDelta>(),
                Array.Empty<UnityArchiveReviewComparisonDiagnosticChange>(),
                Array.Empty<UnityArchiveReviewComparisonSourceFileChange>(),
                Array.Empty<UnityArchiveReviewComparisonInvalidReasonChange>(),
                written,
                cancellationToken).ConfigureAwait(false);
        }

        if (!File.Exists(reviewPath))
        {
            return await WriteComparisonAsync(
                archiveRoot,
                UnityArchiveReviewComparisonReadiness.MissingReview,
                string.Empty,
                string.Empty,
                new UnityArchiveReviewComparisonSummary(),
                Array.Empty<UnityArchiveReviewComparisonDelta>(),
                Array.Empty<UnityArchiveReviewComparisonDiagnosticChange>(),
                Array.Empty<UnityArchiveReviewComparisonSourceFileChange>(),
                Array.Empty<UnityArchiveReviewComparisonInvalidReasonChange>(),
                written,
                cancellationToken).ConfigureAwait(false);
        }

        UnityArchiveReviewSnapshotReport? currentReview;
        try
        {
            await using var stream = File.OpenRead(reviewPath);
            currentReview = await JsonSerializer.DeserializeAsync<UnityArchiveReviewSnapshotReport>(stream, JsonOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            return await WriteComparisonAsync(
                archiveRoot,
                UnityArchiveReviewComparisonReadiness.Invalid,
                string.Empty,
                string.Empty,
                new UnityArchiveReviewComparisonSummary(),
                Array.Empty<UnityArchiveReviewComparisonDelta>(),
                Array.Empty<UnityArchiveReviewComparisonDiagnosticChange>(),
                Array.Empty<UnityArchiveReviewComparisonSourceFileChange>(),
                Array.Empty<UnityArchiveReviewComparisonInvalidReasonChange>(),
                written,
                cancellationToken).ConfigureAwait(false);
        }

        if (currentReview == null)
        {
            return await WriteComparisonAsync(
                archiveRoot,
                UnityArchiveReviewComparisonReadiness.Invalid,
                string.Empty,
                string.Empty,
                new UnityArchiveReviewComparisonSummary(),
                Array.Empty<UnityArchiveReviewComparisonDelta>(),
                Array.Empty<UnityArchiveReviewComparisonDiagnosticChange>(),
                Array.Empty<UnityArchiveReviewComparisonSourceFileChange>(),
                Array.Empty<UnityArchiveReviewComparisonInvalidReasonChange>(),
                written,
                cancellationToken).ConfigureAwait(false);
        }

        var currentIndexPath = GetArchivePath(archiveRoot, HistoryIndexRelativePath);
        if (!File.Exists(currentIndexPath))
        {
            return await WriteComparisonAsync(
                archiveRoot,
                UnityArchiveReviewComparisonReadiness.NoPreviousSnapshot,
                string.Empty,
                string.Empty,
                BuildSummary(currentReview, null),
                Array.Empty<UnityArchiveReviewComparisonDelta>(),
                BuildDiagnosticChanges(currentReview.Diagnostics, null),
                BuildSourceFileChanges(currentReview.SourceFiles, null),
                BuildInvalidReasonChanges(currentReview.Fulfillment.InvalidReasons, null),
                written,
                cancellationToken).ConfigureAwait(false);
        }

        UnityArchiveReviewHistoryIndex? historyIndex;
        try
        {
            await using var indexStream = File.OpenRead(currentIndexPath);
            historyIndex = await JsonSerializer.DeserializeAsync<UnityArchiveReviewHistoryIndex>(indexStream, JsonOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            return await WriteComparisonAsync(
                archiveRoot,
                UnityArchiveReviewComparisonReadiness.NoPreviousSnapshot,
                string.Empty,
                string.Empty,
                BuildSummary(currentReview, null),
                Array.Empty<UnityArchiveReviewComparisonDelta>(),
                BuildDiagnosticChanges(currentReview.Diagnostics, null),
                BuildSourceFileChanges(currentReview.SourceFiles, null),
                BuildInvalidReasonChanges(currentReview.Fulfillment.InvalidReasons, null),
                written,
                cancellationToken).ConfigureAwait(false);
        }

        if (historyIndex == null || historyIndex.Entries.Count == 0)
        {
            return await WriteComparisonAsync(
                archiveRoot,
                UnityArchiveReviewComparisonReadiness.NoPreviousSnapshot,
                string.Empty,
                string.Empty,
                BuildSummary(currentReview, null),
                Array.Empty<UnityArchiveReviewComparisonDelta>(),
                BuildDiagnosticChanges(currentReview.Diagnostics, null),
                BuildSourceFileChanges(currentReview.SourceFiles, null),
                BuildInvalidReasonChanges(currentReview.Fulfillment.InvalidReasons, null),
                written,
                cancellationToken).ConfigureAwait(false);
        }

        var sortedEntries = historyIndex.Entries
            .OrderBy(e => e.SnapshotId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(e => e.SnapshotId, StringComparer.Ordinal)
            .ToList();

        if (sortedEntries.Count < 1)
        {
            return await WriteComparisonAsync(
                archiveRoot,
                UnityArchiveReviewComparisonReadiness.NoPreviousSnapshot,
                string.Empty,
                string.Empty,
                BuildSummary(currentReview, null),
                Array.Empty<UnityArchiveReviewComparisonDelta>(),
                BuildDiagnosticChanges(currentReview.Diagnostics, null),
                BuildSourceFileChanges(currentReview.SourceFiles, null),
                BuildInvalidReasonChanges(currentReview.Fulfillment.InvalidReasons, null),
                written,
                cancellationToken).ConfigureAwait(false);
        }

        var currentSnapshotId = ComputeContentHash(currentReview);
        var currentEntry = sortedEntries.LastOrDefault(e => e.SnapshotId == currentSnapshotId);
        var previousEntry = sortedEntries
            .Where(e => e.SnapshotId != currentSnapshotId)
            .OrderByDescending(e => e.SnapshotId, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(e => e.SnapshotId, StringComparer.Ordinal)
            .FirstOrDefault() ?? currentEntry;

        if (previousEntry == null || string.Equals(previousEntry.SnapshotId, currentSnapshotId, StringComparison.OrdinalIgnoreCase))
        {
            return await WriteComparisonAsync(
                archiveRoot,
                UnityArchiveReviewComparisonReadiness.NoPreviousSnapshot,
                currentSnapshotId,
                string.Empty,
                BuildSummary(currentReview, null),
                Array.Empty<UnityArchiveReviewComparisonDelta>(),
                BuildDiagnosticChanges(currentReview.Diagnostics, null),
                BuildSourceFileChanges(currentReview.SourceFiles, null),
                BuildInvalidReasonChanges(currentReview.Fulfillment.InvalidReasons, null),
                written,
                cancellationToken).ConfigureAwait(false);
        }

        UnityArchiveReviewSnapshotReport? previousReview;
        var previousSnapshotPath = GetArchivePath(archiveRoot, previousEntry.RelativePath);
        try
        {
            await using var prevStream = File.OpenRead(previousSnapshotPath);
            previousReview = await JsonSerializer.DeserializeAsync<UnityArchiveReviewSnapshotReport>(prevStream, JsonOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            previousReview = null;
        }

        if (previousReview == null)
        {
            return await WriteComparisonAsync(
                archiveRoot,
                UnityArchiveReviewComparisonReadiness.NoPreviousSnapshot,
                currentSnapshotId,
                string.Empty,
                BuildSummary(currentReview, null),
                Array.Empty<UnityArchiveReviewComparisonDelta>(),
                BuildDiagnosticChanges(currentReview.Diagnostics, null),
                BuildSourceFileChanges(currentReview.SourceFiles, null),
                BuildInvalidReasonChanges(currentReview.Fulfillment.InvalidReasons, null),
                written,
                cancellationToken).ConfigureAwait(false);
        }

        var summary = BuildSummary(currentReview, previousReview);
        var deltas = BuildDeltas(currentReview, previousReview);
        var diagnosticChanges = BuildDiagnosticChanges(currentReview.Diagnostics, previousReview.Diagnostics);
        var sourceFileChanges = BuildSourceFileChanges(currentReview.SourceFiles, previousReview.SourceFiles);
        var invalidReasonChanges = BuildInvalidReasonChanges(currentReview.Fulfillment.InvalidReasons, previousReview.Fulfillment.InvalidReasons);
        var readiness = MapReadiness(currentReview.Readiness);

        return await WriteComparisonAsync(
            archiveRoot,
            readiness,
            currentSnapshotId,
            previousEntry.SnapshotId,
            summary,
            deltas,
            diagnosticChanges,
            sourceFileChanges,
            invalidReasonChanges,
            written,
            cancellationToken).ConfigureAwait(false);
    }

    private static UnityArchiveReviewComparisonSummary BuildSummary(
        UnityArchiveReviewSnapshotReport current,
        UnityArchiveReviewSnapshotReport? previous)
    {
        var previousCount = previous?.SourceFileCount ?? 0;
        return new UnityArchiveReviewComparisonSummary
        {
            SourceFileCountDelta = current.SourceFileCount - previousCount,
            DiagnosticCountDelta = current.DiagnosticCount - (previous?.DiagnosticCount ?? 0),
            ErrorCountDelta = current.ErrorCount - (previous?.ErrorCount ?? 0),
            WarningCountDelta = current.WarningCount - (previous?.WarningCount ?? 0),
            InfoCountDelta = current.InfoCount - (previous?.InfoCount ?? 0),
            InvalidOutputCountDelta = current.Fulfillment.InvalidOutputCount - (previous?.Fulfillment.InvalidOutputCount ?? 0),
            AssetSlotCount = current.Providers.AssetSlotCount,
            AudioSlotCount = current.Providers.AudioSlotCount,
            LuaModuleSlotCount = current.Providers.LuaModuleSlotCount,
            ProviderJobCount = current.Providers.ProviderJobCount,
            AssetRequestCount = current.Requests.AssetRequestCount,
            AudioRequestCount = current.Requests.AudioRequestCount,
            LuaModuleRequestCount = current.Requests.LuaModuleRequestCount
        };
    }

    private static IReadOnlyList<UnityArchiveReviewComparisonDelta> BuildDeltas(
        UnityArchiveReviewSnapshotReport current,
        UnityArchiveReviewSnapshotReport previous)
    {
        var deltas = new List<UnityArchiveReviewComparisonDelta>();

        AddDeltaIfDifferent(deltas, "readiness", previous.Readiness.ToString(), current.Readiness.ToString());
        AddDeltaIfDifferent(deltas, "materializationReadiness",
            previous.Validation.MaterializationReadiness.ToString(),
            current.Validation.MaterializationReadiness.ToString());
        AddDeltaIfDifferent(deltas, "providerPlanReadiness",
            previous.Providers.Readiness.ToString(),
            current.Providers.Readiness.ToString());

        return deltas.OrderBy(d => d.Dimension, StringComparer.OrdinalIgnoreCase)
            .ThenBy(d => d.Dimension, StringComparer.Ordinal)
            .ToList();
    }

    private static void AddDeltaIfDifferent(
        ICollection<UnityArchiveReviewComparisonDelta> deltas,
        string dimension,
        string previous,
        string current)
    {
        if (!string.Equals(previous, current, StringComparison.OrdinalIgnoreCase))
        {
            deltas.Add(new UnityArchiveReviewComparisonDelta
            {
                Dimension = dimension,
                Previous = previous,
                Current = current
            });
        }
    }

    private static IReadOnlyList<UnityArchiveReviewComparisonDiagnosticChange> BuildDiagnosticChanges(
        IReadOnlyList<UnityArchiveReviewSnapshotDiagnostic> current,
        IReadOnlyList<UnityArchiveReviewSnapshotDiagnostic>? previous)
    {
        var changes = new List<UnityArchiveReviewComparisonDiagnosticChange>();
        var currentSet = current.Select(d => d.Fingerprint()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var previousSet = (previous ?? Array.Empty<UnityArchiveReviewSnapshotDiagnostic>())
            .Select(d => d.Fingerprint()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var diagnostic in current)
        {
            if (!previousSet.Contains(diagnostic.Fingerprint()))
            {
                changes.Add(new UnityArchiveReviewComparisonDiagnosticChange
                {
                    Severity = diagnostic.Severity,
                    Code = diagnostic.Code,
                    Message = diagnostic.Message,
                    TargetId = diagnostic.TargetId,
                    SourceFile = diagnostic.SourceFile,
                    Change = "added"
                });
            }
        }

        foreach (var diagnostic in previous ?? Array.Empty<UnityArchiveReviewSnapshotDiagnostic>())
        {
            if (!currentSet.Contains(diagnostic.Fingerprint()))
            {
                changes.Add(new UnityArchiveReviewComparisonDiagnosticChange
                {
                    Severity = diagnostic.Severity,
                    Code = diagnostic.Code,
                    Message = diagnostic.Message,
                    TargetId = diagnostic.TargetId,
                    SourceFile = diagnostic.SourceFile,
                    Change = "resolved"
                });
            }
        }

        return changes
            .OrderBy(c => c.Change, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.Change, StringComparer.Ordinal)
            .ThenBy(c => c.Severity.ToString(), StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.Severity.ToString(), StringComparer.Ordinal)
            .ThenBy(c => c.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.Code, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<UnityArchiveReviewComparisonSourceFileChange> BuildSourceFileChanges(
        IReadOnlyList<UnityArchiveReviewSnapshotFileReference> current,
        IReadOnlyList<UnityArchiveReviewSnapshotFileReference>? previous)
    {
        var changes = new List<UnityArchiveReviewComparisonSourceFileChange>();
        var currentSet = current.Select(f => f.Fingerprint()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var previousSet = (previous ?? Array.Empty<UnityArchiveReviewSnapshotFileReference>())
            .Select(f => f.Fingerprint()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var file in current)
        {
            if (!previousSet.Contains(file.Fingerprint()))
            {
                changes.Add(new UnityArchiveReviewComparisonSourceFileChange
                {
                    RelativePath = file.RelativePath,
                    Kind = file.Kind,
                    Change = "added"
                });
            }
        }

        foreach (var file in previous ?? Array.Empty<UnityArchiveReviewSnapshotFileReference>())
        {
            if (!currentSet.Contains(file.Fingerprint()))
            {
                changes.Add(new UnityArchiveReviewComparisonSourceFileChange
                {
                    RelativePath = file.RelativePath,
                    Kind = file.Kind,
                    Change = "removed"
                });
            }
        }

        return changes
            .OrderBy(c => c.Change, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.Change, StringComparer.Ordinal)
            .ThenBy(c => c.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<UnityArchiveReviewComparisonInvalidReasonChange> BuildInvalidReasonChanges(
        IReadOnlyList<UnityArchiveReviewInvalidOutputReasonSummary> current,
        IReadOnlyList<UnityArchiveReviewInvalidOutputReasonSummary>? previous)
    {
        var changes = new List<UnityArchiveReviewComparisonInvalidReasonChange>();
        var previousDict = (previous ?? Array.Empty<UnityArchiveReviewInvalidOutputReasonSummary>())
            .ToDictionary(r => r.Reason, r => r.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var reason in current)
        {
            var previousCount = previousDict.GetValueOrDefault(reason.Reason, 0);
            if (reason.Count != previousCount)
            {
                changes.Add(new UnityArchiveReviewComparisonInvalidReasonChange
                {
                    Reason = reason.Reason,
                    PreviousCount = previousCount,
                    CurrentCount = reason.Count
                });
            }
        }

        foreach (var kvp in previousDict)
        {
            if (!current.Any(r => string.Equals(r.Reason, kvp.Key, StringComparison.OrdinalIgnoreCase)))
            {
                changes.Add(new UnityArchiveReviewComparisonInvalidReasonChange
                {
                    Reason = kvp.Key,
                    PreviousCount = kvp.Value,
                    CurrentCount = 0
                });
            }
        }

        return changes
            .OrderBy(c => c.Reason, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.Reason, StringComparer.Ordinal)
            .ToList();
    }

    private async Task<UnityArchiveReviewComparisonResult> WriteComparisonAsync(
        string archiveRoot,
        UnityArchiveReviewComparisonReadiness readiness,
        string currentSnapshotId,
        string previousSnapshotId,
        UnityArchiveReviewComparisonSummary summary,
        IReadOnlyList<UnityArchiveReviewComparisonDelta> deltas,
        IReadOnlyList<UnityArchiveReviewComparisonDiagnosticChange> diagnosticChanges,
        IReadOnlyList<UnityArchiveReviewComparisonSourceFileChange> sourceFileChanges,
        IReadOnlyList<UnityArchiveReviewComparisonInvalidReasonChange> invalidReasonChanges,
        ICollection<string> written,
        CancellationToken cancellationToken)
    {
        var report = new UnityArchiveReviewComparisonReport
        {
            SchemaVersion = "1",
            Readiness = readiness,
            CurrentSnapshotId = currentSnapshotId,
            PreviousSnapshotId = previousSnapshotId,
            Summary = summary,
            Deltas = deltas,
            DiagnosticChanges = diagnosticChanges,
            SourceFileChanges = sourceFileChanges,
            InvalidReasonChanges = invalidReasonChanges
        };

        await WriteJsonFileAsync(archiveRoot, ComparisonJsonRelativePath, report, cancellationToken).ConfigureAwait(false);
        written.Add(ComparisonJsonRelativePath);

        await WriteTextFileAsync(
            archiveRoot,
            ComparisonMarkdownRelativePath,
            _markdownRenderer.Render(report),
            cancellationToken).ConfigureAwait(false);
        written.Add(ComparisonMarkdownRelativePath);

        return new UnityArchiveReviewComparisonResult
        {
            ArchiveDirectoryPath = archiveRoot,
            Report = report,
            WrittenRelativePaths = written.ToArray()
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

    private static string ComputeContentHash(UnityArchiveReviewSnapshotReport report)
    {
        var normalized = JsonSerializer.Serialize(report, JsonOptions);
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalized));
        return string.Concat(hashBytes.Select(b => b.ToString("x2")));
    }

    private static UnityArchiveReviewComparisonReadiness MapReadiness(UnityArchiveReviewSnapshotReadiness snapshotReadiness)
    {
        return snapshotReadiness switch
        {
            UnityArchiveReviewSnapshotReadiness.Ready => UnityArchiveReviewComparisonReadiness.Ready,
            UnityArchiveReviewSnapshotReadiness.ReadyWithWarnings => UnityArchiveReviewComparisonReadiness.ReadyWithWarnings,
            UnityArchiveReviewSnapshotReadiness.Blocked => UnityArchiveReviewComparisonReadiness.Blocked,
            UnityArchiveReviewSnapshotReadiness.Invalid => UnityArchiveReviewComparisonReadiness.Invalid,
            UnityArchiveReviewSnapshotReadiness.MissingArchive => UnityArchiveReviewComparisonReadiness.MissingReview,
            _ => UnityArchiveReviewComparisonReadiness.MissingReview
        };
    }
}