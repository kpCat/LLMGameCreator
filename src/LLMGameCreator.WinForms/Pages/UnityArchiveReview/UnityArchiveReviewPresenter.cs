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
    private const string ManualImportReportJsonRelativePath = "production/manual-provider-import-report.json";
    private const string ManualImportReportMarkdownRelativePath = "production/manual-provider-import-report.md";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly UnityArchiveManualImportTemplateService _templateService;
    private readonly UnityArchiveManualProviderImportService _importService;

    public UnityArchiveReviewPresenter(
        UnityArchiveManualImportTemplateService? templateService = null,
        UnityArchiveManualProviderImportService? importService = null)
    {
        _templateService = templateService ?? new UnityArchiveManualImportTemplateService();
        _importService = importService ?? new UnityArchiveManualProviderImportService();
    }

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
        UnityArchiveManualImportSlotFilter slotFilter = UnityArchiveManualImportSlotFilter.All,
        string? selectedManualImportSlotId = null,
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
        var manualImportReportJson = await ReadJsonAsync<UnityArchiveManualProviderImportResult>(
            initial.ArchiveRoot,
            ManualImportReportJsonRelativePath,
            missingFiles,
            invalidFiles,
            cancellationToken,
            required: false).ConfigureAwait(false);
        var manualImportReportMarkdown = await ReadTextAsync(
            initial.ArchiveRoot,
            ManualImportReportMarkdownRelativePath,
            missingFiles,
            invalidFiles,
            cancellationToken,
            required: false).ConfigureAwait(false);
        var workspace = await _templateService.LoadWorkspaceAsync(
            initial.ArchiveRoot,
            cancellationToken).ConfigureAwait(false);

        var snapshots = BuildSnapshotOptions(initial.ArchiveRoot, historyIndex.Value);
        var selected = snapshots.FirstOrDefault(snapshot =>
                           string.Equals(snapshot.SnapshotId, selectedSnapshotId, StringComparison.OrdinalIgnoreCase))
                       ?? snapshots.FirstOrDefault();
        var selectedSnapshot = selected is null
            ? new JsonFileResult<UnityArchiveReviewSnapshotReport>()
            : await ReadJsonAsync<UnityArchiveReviewSnapshotReport>(
                initial.ArchiveRoot,
                selected.RelativePath,
                missingFiles,
                invalidFiles,
                cancellationToken).ConfigureAwait(false);
        var selectedSnapshotStatus = selected is null
            ? "Unavailable"
            : selectedSnapshot.Value is not null
                ? "Loaded"
                : selectedSnapshot.Exists
                    ? "Invalid"
                    : "Missing";

        var manualImportReportStatus = manualImportReportJson.Value is not null
            ? $"Manual import report: {manualImportReportJson.Value.Readiness}."
            : manualImportReportJson.Exists
                ? "Manual import report JSON is invalid. Markdown remains available when readable."
                : "No manual import report yet.";
        var workspaceStatus = BuildWorkspaceStatus(workspace);
        var refreshed = initial with
        {
            Status = BuildStatus(missingFiles, invalidFiles),
            CurrentReviewReadiness = currentReview.Value?.Readiness.ToString()
                                     ?? (currentReview.Exists ? "Invalid" : "Missing"),
            ComparisonReadiness = comparison.Value?.Readiness.ToString()
                                  ?? (comparison.Exists ? "Invalid" : "Missing"),
            HistorySnapshotCount = snapshots.Count,
            SelectedSnapshotId = selected?.SnapshotId ?? string.Empty,
            SelectedSnapshotJson = selectedSnapshot.Content,
            SelectedSnapshotStatus = selectedSnapshotStatus,
            SelectedSnapshotRelativePath = selected?.RelativePath ?? string.Empty,
            SelectedSnapshotSequence = selected?.Sequence ?? 0,
            HistorySnapshots = snapshots,
            CurrentReviewMarkdown = currentMarkdown,
            ComparisonMarkdown = comparisonMarkdown,
            CurrentReviewJson = currentReview.Content,
            ComparisonJson = comparison.Content,
            HistoryIndexJson = historyIndex.Content,
            ManualImportReportMarkdown = manualImportReportMarkdown,
            ManualImportReportJson = manualImportReportJson.Content,
            ManualImportReportStatus = manualImportReportStatus,
            ManualImportWorkspaceStatus = workspaceStatus,
            ManualImportSlots = workspace.Slots,
            CanOpenArchiveFolder = true,
            CanCreateManualImportTemplate = workspace.Slots.Count > 0,
            CanRunManualImport = true,
            CanOpenManualImportFolder = true
        };

        return ApplyManualImportFilter(refreshed, slotFilter, selectedManualImportSlotId);
    }

    public UnityArchiveReviewViewState ApplyManualImportFilter(
        UnityArchiveReviewViewState state,
        UnityArchiveManualImportSlotFilter filter,
        string? selectedSlotId = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        var visible = state.ManualImportSlots
            .Where(slot => MatchesFilter(slot, filter))
            .OrderBy(slot => slot.SlotId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(slot => slot.SlotId, StringComparer.Ordinal)
            .ToList();
        var selected = visible.FirstOrDefault(slot =>
                           string.Equals(slot.SlotId, selectedSlotId, StringComparison.OrdinalIgnoreCase))
                       ?? visible.FirstOrDefault();
        return state with
        {
            ManualImportSlotFilter = filter,
            VisibleManualImportSlots = visible,
            SelectedManualImportSlotId = selected?.SlotId ?? string.Empty,
            SelectedManualImportSlotDetail = BuildSlotDetail(selected)
        };
    }

    public async Task<UnityArchiveReviewViewState> CreateManualImportTemplateAsync(
        string? projectFolder,
        string? selectedSnapshotId,
        UnityArchiveManualImportSlotFilter slotFilter,
        string? selectedManualImportSlotId,
        CancellationToken cancellationToken = default)
    {
        var initial = Initialize(projectFolder);
        if (!initial.CanRefresh || !Directory.Exists(initial.ArchiveRoot))
        {
            return initial with { ManualImportWorkspaceStatus = "Manifest template cannot be created because the Unity archive folder is missing." };
        }

        var result = await _templateService.CreateTemplateAsync(initial.ArchiveRoot, cancellationToken).ConfigureAwait(false);
        var refreshed = await RefreshAsync(
            projectFolder,
            selectedSnapshotId,
            slotFilter,
            selectedManualImportSlotId,
            cancellationToken).ConfigureAwait(false);
        return refreshed with { ManualImportWorkspaceStatus = result.Status };
    }

    public UnityArchiveManualImportDirectoryResult EnsureManualImportDirectory(string? projectFolder)
    {
        var initial = Initialize(projectFolder);
        return _templateService.EnsureManualImportDirectory(initial.ArchiveRoot);
    }

    public async Task<UnityArchiveReviewViewState> RunManualImportAsync(
        string? projectFolder,
        string? selectedSnapshotId,
        UnityArchiveManualImportSlotFilter slotFilter,
        string? selectedManualImportSlotId,
        bool overwriteExisting,
        CancellationToken cancellationToken = default)
    {
        var initial = Initialize(projectFolder);
        if (!initial.CanRefresh || !Directory.Exists(initial.ArchiveRoot))
        {
            return initial with { ManualImportWorkspaceStatus = "Manual import cannot run because the Unity archive folder is missing." };
        }

        var result = await _importService.ImportAsync(new UnityArchiveManualProviderImportRequest
        {
            ArchiveDirectoryPath = initial.ArchiveRoot,
            ImportDirectoryRelativePath = "manual-import",
            ManifestRelativePath = "manual-import/import-manifest.json",
            RefreshFulfillmentState = true,
            RefreshReviewHistoryComparison = true,
            OverwriteExisting = overwriteExisting
        }, cancellationToken).ConfigureAwait(false);
        var refreshed = await RefreshAsync(
            projectFolder,
            selectedSnapshotId,
            slotFilter,
            selectedManualImportSlotId,
            cancellationToken).ConfigureAwait(false);
        var status = result.Readiness == UnityArchiveManualProviderImportReadiness.MissingManifest
            ? "Manual import manifest is missing. Copy/edit import-manifest.template.json as manual-import/import-manifest.json, then run again."
            : $"Manual import finished: {result.Readiness}; imported={result.ImportedCount}, already imported={result.SkippedCount}, conflicts={result.ConflictCount}, invalid={result.InvalidCount}.";
        return refreshed with { ManualImportWorkspaceStatus = status };
    }

    private static async Task<JsonFileResult<T>> ReadJsonAsync<T>(
        string archiveRoot,
        string relativePath,
        ICollection<string> missingFiles,
        ICollection<string> invalidFiles,
        CancellationToken cancellationToken,
        bool required = true)
    {
        var path = GetArchivePath(archiveRoot, relativePath);
        if (!File.Exists(path))
        {
            if (required)
            {
                missingFiles.Add(relativePath);
            }
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
        CancellationToken cancellationToken,
        bool required = true)
    {
        var path = GetArchivePath(archiveRoot, relativePath);
        if (!File.Exists(path))
        {
            if (required)
            {
                missingFiles.Add(relativePath);
            }
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

    private static bool MatchesFilter(
        UnityArchiveManualImportWorkspaceSlot slot,
        UnityArchiveManualImportSlotFilter filter) => filter switch
    {
        UnityArchiveManualImportSlotFilter.Missing => slot.Status == UnityArchiveFulfillmentStatus.missing,
        UnityArchiveManualImportSlotFilter.Available => slot.Status == UnityArchiveFulfillmentStatus.available,
        UnityArchiveManualImportSlotFilter.Invalid => slot.Status == UnityArchiveFulfillmentStatus.invalid,
        UnityArchiveManualImportSlotFilter.ManualImportProvider => slot.ProviderKind == UnityArchiveRequestProviderKind.manual_import,
        UnityArchiveManualImportSlotFilter.FutureProviders => slot.ProviderKind is
            UnityArchiveRequestProviderKind.comfyui_future or
            UnityArchiveRequestProviderKind.suno_future or
            UnityArchiveRequestProviderKind.local_audio_future or
            UnityArchiveRequestProviderKind.procedural_future,
        _ => true
    };

    private static string BuildWorkspaceStatus(UnityArchiveManualImportWorkspaceResult workspace)
    {
        var summary = $"Manual import slots: {workspace.Slots.Count}; readiness: {workspace.Readiness}.";
        return workspace.Diagnostics.Count == 0
            ? summary
            : $"{summary} {string.Join(" ", workspace.Diagnostics)}";
    }

    private static string BuildSlotDetail(UnityArchiveManualImportWorkspaceSlot? slot)
    {
        if (slot is null)
        {
            return "No manual import slot is selected.";
        }

        return string.Join(Environment.NewLine,
        [
            $"slotId: {slot.SlotId}",
            $"kind: {slot.Kind}",
            $"providerKind: {slot.ProviderKind}",
            $"expectedOutputRelativePath: {slot.ExpectedOutputRelativePath}",
            $"status: {slot.Status}",
            $"file exists: {slot.FileExists}",
            $"file size: {slot.FileSizeBytes}",
            $"sha256: {(string.IsNullOrWhiteSpace(slot.ContentSha256) ? "Not available" : slot.ContentSha256)}",
            $"requestId: {slot.RequestId}",
            $"sourceId: {slot.SourceId}",
            $"suggested sourceRelativePath: {slot.SuggestedSourceRelativePath}"
        ]);
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
