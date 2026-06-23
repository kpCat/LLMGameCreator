using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveReviewHistoryService
{
    private const string HistoryDirectoryRelativePath = "review-history";
    private const string HistoryIndexRelativePath = "production/archive-review-history-index.json";
    private const string ReviewJsonRelativePath = "production/archive-review.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<UnityArchiveReviewHistoryResult> StoreAsync(
        UnityArchiveReviewHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.ArchiveDirectoryPath))
        {
            throw new ArgumentException("Archive directory path is required.", nameof(request));
        }

        var archiveRoot = Path.GetFullPath(request.ArchiveDirectoryPath);
        var written = new List<string>();
        var diagnostics = new List<UnityArchiveReviewSnapshotDiagnostic>();

        var reviewPath = GetArchivePath(archiveRoot, ReviewJsonRelativePath);
        if (!Directory.Exists(archiveRoot))
        {
            return new UnityArchiveReviewHistoryResult
            {
                ArchiveDirectoryPath = archiveRoot,
                Report = new UnityArchiveReviewHistoryReport
                {
                    Readiness = UnityArchiveReviewHistoryReadiness.MissingReview,
                    SnapshotId = string.Empty,
                    HistoryEntries = Array.Empty<UnityArchiveReviewHistorySnapshotEntry>()
                },
                WrittenRelativePaths = written
            };
        }

        if (!File.Exists(reviewPath))
        {
            return new UnityArchiveReviewHistoryResult
            {
                ArchiveDirectoryPath = archiveRoot,
                Report = new UnityArchiveReviewHistoryReport
                {
                    Readiness = UnityArchiveReviewHistoryReadiness.MissingReview,
                    SnapshotId = string.Empty,
                    HistoryEntries = Array.Empty<UnityArchiveReviewHistorySnapshotEntry>()
                },
                WrittenRelativePaths = written
            };
        }

        UnityArchiveReviewSnapshotReport? review;
        try
        {
            await using var stream = File.OpenRead(reviewPath);
            review = await JsonSerializer.DeserializeAsync<UnityArchiveReviewSnapshotReport>(stream, JsonOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            return new UnityArchiveReviewHistoryResult
            {
                ArchiveDirectoryPath = archiveRoot,
                Report = new UnityArchiveReviewHistoryReport
                {
                    Readiness = UnityArchiveReviewHistoryReadiness.Invalid,
                    SnapshotId = string.Empty,
                    HistoryEntries = Array.Empty<UnityArchiveReviewHistorySnapshotEntry>()
                },
                WrittenRelativePaths = written
            };
        }

        if (review == null)
        {
            return new UnityArchiveReviewHistoryResult
            {
                ArchiveDirectoryPath = archiveRoot,
                Report = new UnityArchiveReviewHistoryReport
                {
                    Readiness = UnityArchiveReviewHistoryReadiness.Invalid,
                    SnapshotId = string.Empty,
                    HistoryEntries = Array.Empty<UnityArchiveReviewHistorySnapshotEntry>()
                },
                WrittenRelativePaths = written
            };
        }

        var snapshotId = ComputeContentHash(review);
        var snapshotRelativePath = $"{HistoryDirectoryRelativePath}/{snapshotId}/archive-review.json";
        var snapshotPath = GetArchivePath(archiveRoot, snapshotRelativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(snapshotPath)!);
        var normalizedJson = JsonSerializer.Serialize(review, JsonOptions);
        await File.WriteAllTextAsync(snapshotPath, normalizedJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(snapshotRelativePath);

        var index = await ReadOrCreateIndexAsync(archiveRoot, cancellationToken).ConfigureAwait(false);
        var existingEntry = index.Entries.FirstOrDefault(e => e.SnapshotId == snapshotId);
        if (existingEntry == null)
        {
            var entries = index.Entries
                .Where(e => e.SnapshotId != snapshotId)
                .Concat(new[]
                {
                    new UnityArchiveReviewHistorySnapshotEntry
                    {
                        SnapshotId = snapshotId,
                        RelativePath = snapshotRelativePath
                    }
                })
                .OrderBy(e => e.SnapshotId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(e => e.SnapshotId, StringComparer.Ordinal)
                .ToList();

            var historyIndex = new UnityArchiveReviewHistoryIndex
            {
                SchemaVersion = "1",
                Entries = entries
            };

            var indexPath = GetArchivePath(archiveRoot, HistoryIndexRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(indexPath)!);
            await WriteJsonFileAsync(archiveRoot, HistoryIndexRelativePath, historyIndex, cancellationToken).ConfigureAwait(false);
            written.Add(HistoryIndexRelativePath);

            index = historyIndex;
        }

        var readiness = MapReadiness(review.Readiness);

        return new UnityArchiveReviewHistoryResult
        {
            ArchiveDirectoryPath = archiveRoot,
            Report = new UnityArchiveReviewHistoryReport
            {
                SchemaVersion = "1",
                Readiness = readiness,
                SnapshotId = snapshotId,
                HistoryEntries = index.Entries
            },
            WrittenRelativePaths = written
        };
    }

    private static string ComputeContentHash(UnityArchiveReviewSnapshotReport report)
    {
        var normalized = JsonSerializer.Serialize(report, JsonOptions);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalized));
        return string.Concat(hashBytes.Select(b => b.ToString("x2")));
    }

    private static async Task<UnityArchiveReviewHistoryIndex> ReadOrCreateIndexAsync(
        string archiveRoot,
        CancellationToken cancellationToken)
    {
        var indexPath = GetArchivePath(archiveRoot, HistoryIndexRelativePath);
        if (!File.Exists(indexPath))
        {
            return new UnityArchiveReviewHistoryIndex
            {
                SchemaVersion = "1",
                Entries = Array.Empty<UnityArchiveReviewHistorySnapshotEntry>()
            };
        }

        try
        {
            await using var stream = File.OpenRead(indexPath);
            return await JsonSerializer.DeserializeAsync<UnityArchiveReviewHistoryIndex>(stream, JsonOptions, cancellationToken).ConfigureAwait(false)
                ?? new UnityArchiveReviewHistoryIndex { SchemaVersion = "1", Entries = Array.Empty<UnityArchiveReviewHistorySnapshotEntry>() };
        }
        catch (JsonException)
        {
            return new UnityArchiveReviewHistoryIndex
            {
                SchemaVersion = "1",
                Entries = Array.Empty<UnityArchiveReviewHistorySnapshotEntry>()
            };
        }
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

    private static UnityArchiveReviewHistoryReadiness MapReadiness(UnityArchiveReviewSnapshotReadiness snapshotReadiness)
    {
        return snapshotReadiness switch
        {
            UnityArchiveReviewSnapshotReadiness.Ready => UnityArchiveReviewHistoryReadiness.Ready,
            UnityArchiveReviewSnapshotReadiness.ReadyWithWarnings => UnityArchiveReviewHistoryReadiness.ReadyWithWarnings,
            UnityArchiveReviewSnapshotReadiness.Blocked => UnityArchiveReviewHistoryReadiness.Blocked,
            UnityArchiveReviewSnapshotReadiness.Invalid => UnityArchiveReviewHistoryReadiness.Invalid,
            UnityArchiveReviewSnapshotReadiness.MissingArchive => UnityArchiveReviewHistoryReadiness.MissingReview,
            _ => UnityArchiveReviewHistoryReadiness.MissingReview
        };
    }
}