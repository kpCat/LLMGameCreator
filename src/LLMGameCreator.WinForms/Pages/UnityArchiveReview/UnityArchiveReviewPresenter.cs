using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Composition;

namespace LLMGameCreator.WinForms.Pages.UnityArchiveReview;

public sealed class UnityArchiveReviewPresenter
{
    private const string ArchiveRelativePath = ".llmgc/unity-archive";
    private const string CurrentReviewJsonRelativePath = "production/archive-review.json";
    private const string CurrentReviewMarkdownRelativePath = "production/archive-review.md";
    private const string HistoryIndexRelativePath = "production/archive-review-history-index.json";
    private const string ComparisonJsonRelativePath = "production/archive-review-comparison.json";
    private const string ComparisonMarkdownRelativePath = "production/archive-review-comparison.md";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public UnityArchiveReviewViewState Initialize(string? projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            return new UnityArchiveReviewViewState();
        }

        try
        {
            var normalizedProjectFolder = Path.GetFullPath(projectFolder);
            var archiveRoot = Path.Combine(normalizedProjectFolder, ".llmgc", "unity-archive");
            return new UnityArchiveReviewViewState
            {
                ProjectFolder = normalizedProjectFolder,
                ArchiveRoot = archiveRoot,
                Status = "Archive review has not been refreshed.",
                CanRefresh = true,
                CanOpenArchiveFolder = Directory.Exists(archiveRoot)
            };
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return new UnityArchiveReviewViewState
            {
                Status = $"The current project folder is invalid: {ex.Message}"
            };
        }
    }

    public async Task<UnityArchiveReviewViewState> RefreshAsync(
        string? projectFolder,
        string? selectedSnapshotId = null,
        CancellationToken cancellationToken = default)
    {
        var initial = Initialize(projectFolder);
        if (!initial.CanRefresh)
        {
            return initial;
        }

        if (!Directory.Exists(initial.ArchiveRoot))
        {
            return initial with
            {
                Status = $"Unity archive folder was not found: {ArchiveRelativePath}",
                CurrentReviewReadiness = "Missing",
                ComparisonReadiness = "Missing",
                CanOpenArchiveFolder = false
            };
        }

        var missingFiles = new List<string>();
        var invalidFiles = new List<string>();

        var currentReview = await ReadJsonAsync<UnityArchiveReviewSnapshotReport>(
            initial.ArchiveRoot,
            CurrentReviewJsonRelativePath,
            missingFiles,
            invalidFiles,
            cancellationToken).ConfigureAwait(false);
        var comparison = await ReadJsonAsync<UnityArchiveReviewComparisonReport>(
            initial.ArchiveRoot,
            ComparisonJsonRelativePath,
            missingFiles,
            invalidFiles,
            cancellationToken).ConfigureAwait(false);
        var historyIndex = await ReadJsonAsync<UnityArchiveReviewHistoryIndex>(
            initial.ArchiveRoot,
            HistoryIndexRelativePath,
            missingFiles,
            invalidFiles,
            cancellationToken).ConfigureAwait(false);
        var currentMarkdown = await ReadTextAsync(
            initial.ArchiveRoot,
            CurrentReviewMarkdownRelativePath,
            missingFiles,
            invalidFiles,
            cancellationToken).ConfigureAwait(false);
        var comparisonMarkdown = await ReadTextAsync(
            initial.ArchiveRoot,
            ComparisonMarkdownRelativePath,
            missingFiles,
            invalidFiles,
            cancellationToken).ConfigureAwait(false);

        var snapshots = BuildSnapshotOptions(initial.ArchiveRoot, historyIndex.Value);
        var selected = snapshots.FirstOrDefault(snapshot =>
                           string.Equals(snapshot.SnapshotId, selectedSnapshotId, StringComparison.OrdinalIgnoreCase))
                       ?? snapshots.FirstOrDefault();

        return initial with
        {
            Status = BuildStatus(missingFiles, invalidFiles),
            CurrentReviewReadiness = currentReview.Value?.Readiness.ToString()
                                     ?? (currentReview.Exists ? "Invalid" : "Missing"),
            ComparisonReadiness = comparison.Value?.Readiness.ToString()
                                  ?? (comparison.Exists ? "Invalid" : "Missing"),
            HistorySnapshotCount = snapshots.Count,
            SelectedSnapshotId = selected?.SnapshotId ?? string.Empty,
            HistorySnapshots = snapshots,
            CurrentReviewMarkdown = currentMarkdown,
            ComparisonMarkdown = comparisonMarkdown,
            CurrentReviewJson = currentReview.Content,
            ComparisonJson = comparison.Content,
            HistoryIndexJson = historyIndex.Content,
            CanOpenArchiveFolder = true
        };
    }

    private static async Task<JsonFileResult<T>> ReadJsonAsync<T>(
        string archiveRoot,
        string relativePath,
        ICollection<string> missingFiles,
        ICollection<string> invalidFiles,
        CancellationToken cancellationToken)
    {
        var path = GetArchivePath(archiveRoot, relativePath);
        if (!File.Exists(path))
        {
            missingFiles.Add(relativePath);
            return new JsonFileResult<T>();
        }

        string content;
        try
        {
            content = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            invalidFiles.Add($"{relativePath} ({ex.Message})");
            return new JsonFileResult<T> { Exists = true };
        }

        try
        {
            var value = JsonSerializer.Deserialize<T>(content, JsonOptions);
            if (value is null)
            {
                invalidFiles.Add(relativePath);
            }

            return new JsonFileResult<T>
            {
                Exists = true,
                Content = content,
                Value = value
            };
        }
        catch (JsonException)
        {
            invalidFiles.Add(relativePath);
            return new JsonFileResult<T>
            {
                Exists = true,
                Content = content
            };
        }
    }

    private static async Task<string> ReadTextAsync(
        string archiveRoot,
        string relativePath,
        ICollection<string> missingFiles,
        ICollection<string> invalidFiles,
        CancellationToken cancellationToken)
    {
        var path = GetArchivePath(archiveRoot, relativePath);
        if (!File.Exists(path))
        {
            missingFiles.Add(relativePath);
            return string.Empty;
        }

        try
        {
            return await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            invalidFiles.Add($"{relativePath} ({ex.Message})");
            return $"File could not be read: {ex.Message}";
        }
    }

    private static IReadOnlyList<UnityArchiveReviewSnapshotOption> BuildSnapshotOptions(
        string archiveRoot,
        UnityArchiveReviewHistoryIndex? historyIndex)
    {
        var snapshots = new Dictionary<string, UnityArchiveReviewSnapshotOption>(StringComparer.OrdinalIgnoreCase);
        if (historyIndex is not null)
        {
            foreach (var entry in historyIndex.Entries
                         .Where(entry => IsSafeSnapshotId(entry.SnapshotId))
                         .OrderByDescending(entry => entry.Sequence)
                         .ThenBy(entry => entry.SnapshotId, StringComparer.Ordinal))
            {
                var relativePath = $"review-history/{entry.SnapshotId}/archive-review.json";
                snapshots[entry.SnapshotId] = new UnityArchiveReviewSnapshotOption
                {
                    Sequence = entry.Sequence,
                    SnapshotId = entry.SnapshotId,
                    RelativePath = relativePath,
                    FileExists = File.Exists(GetArchivePath(archiveRoot, relativePath))
                };
            }
        }

        var historyRoot = Path.Combine(archiveRoot, "review-history");
        if (Directory.Exists(historyRoot))
        {
            try
            {
                foreach (var directory in Directory.EnumerateDirectories(historyRoot))
                {
                    var snapshotId = Path.GetFileName(directory);
                    var reviewPath = Path.Combine(directory, "archive-review.json");
                    if (!IsSafeSnapshotId(snapshotId) || !File.Exists(reviewPath) || snapshots.ContainsKey(snapshotId))
                    {
                        continue;
                    }

                    snapshots[snapshotId] = new UnityArchiveReviewSnapshotOption
                    {
                        SnapshotId = snapshotId,
                        RelativePath = $"review-history/{snapshotId}/archive-review.json",
                        FileExists = true
                    };
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // The production reports remain readable even when history enumeration is denied.
            }
        }

        return snapshots.Values
            .OrderByDescending(snapshot => snapshot.Sequence)
            .ThenBy(snapshot => snapshot.SnapshotId, StringComparer.Ordinal)
            .ToList();
    }

    private static string BuildStatus(IReadOnlyCollection<string> missingFiles, IReadOnlyCollection<string> invalidFiles)
    {
        if (missingFiles.Count == 0 && invalidFiles.Count == 0)
        {
            return "Archive review, comparison, and history reports loaded.";
        }

        var parts = new List<string>();
        if (missingFiles.Count > 0)
        {
            parts.Add($"Missing files: {string.Join(", ", missingFiles)}.");
        }

        if (invalidFiles.Count > 0)
        {
            parts.Add($"Invalid JSON or unreadable files: {string.Join(", ", invalidFiles)}.");
        }

        return string.Join(" ", parts);
    }

    private static bool IsSafeSnapshotId(string? snapshotId)
    {
        return !string.IsNullOrWhiteSpace(snapshotId)
               && string.Equals(Path.GetFileName(snapshotId), snapshotId, StringComparison.Ordinal)
               && snapshotId.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
    }

    private static string GetArchivePath(string archiveRoot, string relativePath)
    {
        var normalizedRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(archiveRoot, normalizedRelativePath);
    }

    private sealed record JsonFileResult<T>
    {
        public bool Exists { get; init; }
        public string Content { get; init; } = string.Empty;
        public T? Value { get; init; }
    }
}
