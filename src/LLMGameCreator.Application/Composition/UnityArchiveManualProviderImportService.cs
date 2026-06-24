using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveManualProviderImportService
{
    public const string ReportJsonRelativePath = "production/manual-provider-import-report.json";
    public const string ReportMarkdownRelativePath = "production/manual-provider-import-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly UnityArchiveFulfillmentStateService _fulfillmentStateService;
    private readonly UnityArchiveReviewSnapshotService _reviewSnapshotService;
    private readonly UnityArchiveReviewHistoryService _reviewHistoryService;
    private readonly UnityArchiveReviewComparisonService _reviewComparisonService;
    private readonly UnityArchiveManualProviderImportMarkdownRenderer _markdownRenderer;

    public UnityArchiveManualProviderImportService(
        UnityArchiveFulfillmentStateService? fulfillmentStateService = null,
        UnityArchiveReviewSnapshotService? reviewSnapshotService = null,
        UnityArchiveReviewHistoryService? reviewHistoryService = null,
        UnityArchiveReviewComparisonService? reviewComparisonService = null,
        UnityArchiveManualProviderImportMarkdownRenderer? markdownRenderer = null)
    {
        _fulfillmentStateService = fulfillmentStateService ?? new UnityArchiveFulfillmentStateService();
        _reviewSnapshotService = reviewSnapshotService ?? new UnityArchiveReviewSnapshotService();
        _reviewHistoryService = reviewHistoryService ?? new UnityArchiveReviewHistoryService();
        _reviewComparisonService = reviewComparisonService ?? new UnityArchiveReviewComparisonService();
        _markdownRenderer = markdownRenderer ?? new UnityArchiveManualProviderImportMarkdownRenderer();
    }

    public async Task<UnityArchiveManualProviderImportResult> ImportAsync(
        UnityArchiveManualProviderImportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.ArchiveDirectoryPath))
        {
            throw new ArgumentException("Archive directory path is required.", nameof(request));
        }

        var archiveRoot = Path.GetFullPath(request.ArchiveDirectoryPath);
        var diagnostics = new List<UnityArchiveManualProviderImportDiagnostic>();
        var entries = new List<UnityArchiveManualProviderImportEntryResult>();
        var written = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(archiveRoot) ||
            !IsSafeDirectoryRelativePath(request.ImportDirectoryRelativePath) ||
            !UnityArchiveProviderJobPlanService.IsSafeExpectedOutputRelativePath(request.ManifestRelativePath) ||
            !TryGetContainedArchivePath(archiveRoot, request.ManifestRelativePath, out var manifestPath) ||
            !IsUnderImportDirectory(request.ImportDirectoryRelativePath, request.ManifestRelativePath))
        {
            diagnostics.Add(Error(
                "manual_import.invalid_manifest_json",
                "The archive, import directory, or manifest relative path is invalid.",
                string.Empty));
            return await CompleteAsync(
                archiveRoot,
                UnityArchiveManualProviderImportReadiness.InvalidManifest,
                entries,
                diagnostics,
                written,
                cancellationToken).ConfigureAwait(false);
        }

        if (!File.Exists(manifestPath))
        {
            diagnostics.Add(Error(
                "manual_import.missing_manifest",
                $"Manual import manifest '{NormalizeRelativePath(request.ManifestRelativePath)}' was not found.",
                string.Empty));
            return await CompleteAsync(
                archiveRoot,
                UnityArchiveManualProviderImportReadiness.MissingManifest,
                entries,
                diagnostics,
                written,
                cancellationToken).ConfigureAwait(false);
        }

        UnityArchiveManualProviderImportManifest? manifest;
        try
        {
            await using var stream = File.OpenRead(manifestPath);
            manifest = await JsonSerializer.DeserializeAsync<UnityArchiveManualProviderImportManifest>(
                stream,
                JsonOptions,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            diagnostics.Add(Error(
                "manual_import.invalid_manifest_json",
                "Manual import manifest could not be read as valid JSON.",
                string.Empty));
            return await CompleteAsync(
                archiveRoot,
                UnityArchiveManualProviderImportReadiness.InvalidManifest,
                entries,
                diagnostics,
                written,
                cancellationToken).ConfigureAwait(false);
        }

        if (manifest is null || !string.Equals(manifest.SchemaVersion, "1", StringComparison.Ordinal))
        {
            diagnostics.Add(Error(
                "manual_import.invalid_manifest_json",
                "Manual import manifest schemaVersion must be '1'.",
                string.Empty));
            return await CompleteAsync(
                archiveRoot,
                UnityArchiveManualProviderImportReadiness.InvalidManifest,
                entries,
                diagnostics,
                written,
                cancellationToken).ConfigureAwait(false);
        }

        var plan = await ReadProviderPlanAsync(archiveRoot, diagnostics, cancellationToken).ConfigureAwait(false);
        var slots = BuildSlotMap(plan, diagnostics);
        var manifestEntries = manifest.Entries ?? Array.Empty<UnityArchiveManualProviderImportManifestEntry>();
        var duplicateSlotIds = manifestEntries
            .Where(entry => !string.IsNullOrWhiteSpace(entry.SlotId))
            .GroupBy(entry => entry.SlotId.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var manifestEntry in manifestEntries)
        {
            entries.Add(await ProcessEntryAsync(
                archiveRoot,
                request,
                manifestEntry,
                slots,
                duplicateSlotIds,
                diagnostics,
                written,
                cancellationToken).ConfigureAwait(false));
        }

        if (request.RefreshFulfillmentState)
        {
            try
            {
                var fulfillment = _fulfillmentStateService.Scan(new UnityArchiveFulfillmentStateRequest
                {
                    OutputDirectoryPath = archiveRoot,
                    ProviderJobPlan = plan
                });
                await WriteJsonAsync(archiveRoot, "production/fulfillment-state.json", fulfillment.FulfillmentState, cancellationToken).ConfigureAwait(false);
                written.Add("production/fulfillment-state.json");
                await WriteJsonAsync(archiveRoot, "production/fulfilled-assets-index.json", fulfillment.FulfilledAssets, cancellationToken).ConfigureAwait(false);
                written.Add("production/fulfilled-assets-index.json");
                await WriteJsonAsync(archiveRoot, "production/fulfilled-audio-index.json", fulfillment.FulfilledAudio, cancellationToken).ConfigureAwait(false);
                written.Add("production/fulfilled-audio-index.json");
                await WriteJsonAsync(archiveRoot, "production/fulfilled-lua-index.json", fulfillment.FulfilledLua, cancellationToken).ConfigureAwait(false);
                written.Add("production/fulfilled-lua-index.json");
                await WriteJsonAsync(archiveRoot, "production/invalid-outputs.json", fulfillment.InvalidOutputs, cancellationToken).ConfigureAwait(false);
                written.Add("production/invalid-outputs.json");
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException or JsonException)
            {
                diagnostics.Add(Warning(
                    "manual_import.refresh_failed",
                    "Fulfillment state refresh failed after import.",
                    string.Empty));
            }
        }

        AddWritten(written, ReportJsonRelativePath, ReportMarkdownRelativePath);
        var interim = BuildResult(null, entries, diagnostics, written);
        await WriteReportsAsync(archiveRoot, interim, cancellationToken).ConfigureAwait(false);

        if (request.RefreshReviewHistoryComparison && interim.TargetOutputsChanged)
        {
            try
            {
                var review = await _reviewSnapshotService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
                {
                    ArchiveDirectoryPath = archiveRoot
                }, cancellationToken).ConfigureAwait(false);
                foreach (var path in review.WrittenRelativePaths)
                {
                    written.Add(path);
                }

                var history = await _reviewHistoryService.StoreAsync(new UnityArchiveReviewHistoryRequest
                {
                    ArchiveDirectoryPath = archiveRoot
                }, cancellationToken).ConfigureAwait(false);
                foreach (var path in history.WrittenRelativePaths)
                {
                    written.Add(path);
                }

                var comparison = await _reviewComparisonService.CompareAsync(new UnityArchiveReviewComparisonRequest
                {
                    ArchiveDirectoryPath = archiveRoot
                }, cancellationToken).ConfigureAwait(false);
                foreach (var path in comparison.WrittenRelativePaths)
                {
                    written.Add(path);
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException or JsonException)
            {
                diagnostics.Add(Warning(
                    "manual_import.refresh_failed",
                    "Archive review, history, or comparison refresh failed after import.",
                    string.Empty));
            }
        }

        return await CompleteAsync(
            archiveRoot,
            readiness: null,
            entries,
            diagnostics,
            written,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task<UnityArchiveProviderJobPlanResult> ReadProviderPlanAsync(
        string archiveRoot,
        ICollection<UnityArchiveManualProviderImportDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var fulfillmentPlan = await ReadSlotFileAsync<UnityArchiveFulfillmentPlan>(
            archiveRoot,
            "production/fulfillment-plan.json",
            diagnostics,
            cancellationToken).ConfigureAwait(false) ?? new UnityArchiveFulfillmentPlan();
        var assetSlots = await ReadSlotFileAsync<UnityArchiveAssetSlotIndex>(
            archiveRoot,
            "assets/asset-slots.json",
            diagnostics,
            cancellationToken).ConfigureAwait(false) ?? new UnityArchiveAssetSlotIndex();
        var audioSlots = await ReadSlotFileAsync<UnityArchiveAudioSlotIndex>(
            archiveRoot,
            "audio/audio-slots.json",
            diagnostics,
            cancellationToken).ConfigureAwait(false) ?? new UnityArchiveAudioSlotIndex();
        var luaSlots = await ReadSlotFileAsync<UnityArchiveLuaModuleSlotIndex>(
            archiveRoot,
            "lua/module-slots.json",
            diagnostics,
            cancellationToken).ConfigureAwait(false) ?? new UnityArchiveLuaModuleSlotIndex();

        return new UnityArchiveProviderJobPlanResult
        {
            FulfillmentPlan = fulfillmentPlan,
            AssetSlots = assetSlots,
            AudioSlots = audioSlots,
            LuaModuleSlots = luaSlots
        };
    }

    private static async Task<T?> ReadSlotFileAsync<T>(
        string archiveRoot,
        string relativePath,
        ICollection<UnityArchiveManualProviderImportDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        if (!TryGetContainedArchivePath(archiveRoot, relativePath, out var path) || !File.Exists(path))
        {
            diagnostics.Add(Error(
                "manual_import.missing_slot_metadata",
                $"Required slot metadata '{relativePath}' is missing.",
                string.Empty));
            return default;
        }

        try
        {
            await using var stream = File.OpenRead(path);
            var value = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken).ConfigureAwait(false);
            if (value is null)
            {
                diagnostics.Add(Error(
                    "manual_import.invalid_slot_metadata_json",
                    $"Required slot metadata '{relativePath}' is invalid.",
                    string.Empty));
            }

            return value;
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            diagnostics.Add(Error(
                "manual_import.invalid_slot_metadata_json",
                $"Required slot metadata '{relativePath}' is invalid.",
                string.Empty));
            return default;
        }
    }

    private static IReadOnlyDictionary<string, SlotMetadata> BuildSlotMap(
        UnityArchiveProviderJobPlanResult plan,
        ICollection<UnityArchiveManualProviderImportDiagnostic> diagnostics)
    {
        var slots = new Dictionary<string, SlotMetadata>(StringComparer.OrdinalIgnoreCase);
        var inconsistentSlotIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var slot in plan.FulfillmentPlan.Slots)
        {
            AddSlot(slots, inconsistentSlotIds, new SlotMetadata(slot.SlotId, slot.ProviderKind, slot.ExpectedOutputRelativePath), diagnostics);
        }

        foreach (var slot in plan.AssetSlots.Slots)
        {
            AddSlot(slots, inconsistentSlotIds, new SlotMetadata(slot.SlotId, slot.ProviderKind, slot.ExpectedOutputRelativePath), diagnostics);
        }

        foreach (var slot in plan.AudioSlots.Slots)
        {
            AddSlot(slots, inconsistentSlotIds, new SlotMetadata(slot.SlotId, slot.ProviderKind, slot.ExpectedOutputRelativePath), diagnostics);
        }

        foreach (var slot in plan.LuaModuleSlots.Slots)
        {
            AddSlot(slots, inconsistentSlotIds, new SlotMetadata(slot.SlotId, slot.ProviderKind, slot.ExpectedOutputRelativePath), diagnostics);
        }

        return slots;
    }

    private static void AddSlot(
        IDictionary<string, SlotMetadata> slots,
        ISet<string> inconsistentSlotIds,
        SlotMetadata slot,
        ICollection<UnityArchiveManualProviderImportDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(slot.SlotId))
        {
            return;
        }

        if (inconsistentSlotIds.Contains(slot.SlotId))
        {
            return;
        }

        if (slots.TryGetValue(slot.SlotId, out var existing))
        {
            if (!string.Equals(existing.ExpectedOutputRelativePath, slot.ExpectedOutputRelativePath, StringComparison.Ordinal) ||
                existing.ProviderKind != slot.ProviderKind)
            {
                diagnostics.Add(Error(
                    "manual_import.duplicate_slot",
                    $"Slot metadata for '{slot.SlotId}' is inconsistent.",
                    slot.SlotId));
                slots.Remove(slot.SlotId);
                inconsistentSlotIds.Add(slot.SlotId);
            }

            return;
        }

        slots.Add(slot.SlotId, slot);
    }

    private static async Task<UnityArchiveManualProviderImportEntryResult> ProcessEntryAsync(
        string archiveRoot,
        UnityArchiveManualProviderImportRequest request,
        UnityArchiveManualProviderImportManifestEntry manifestEntry,
        IReadOnlyDictionary<string, SlotMetadata> slots,
        IReadOnlySet<string> duplicateSlotIds,
        ICollection<UnityArchiveManualProviderImportDiagnostic> diagnostics,
        ISet<string> written,
        CancellationToken cancellationToken)
    {
        var slotId = manifestEntry.SlotId?.Trim() ?? string.Empty;
        var sourceInput = NormalizeRelativePath(manifestEntry.SourceRelativePath ?? string.Empty);
        var sourceRelativePath = string.IsNullOrWhiteSpace(sourceInput)
            ? string.Empty
            : $"{NormalizeRelativePath(request.ImportDirectoryRelativePath)}/{sourceInput}";
        var diagnosticCodes = new List<string>();

        if (string.IsNullOrWhiteSpace(slotId))
        {
            return InvalidEntry(
                slotId,
                sourceRelativePath,
                string.Empty,
                "manual_import.unknown_slot",
                "Manifest entry slotId is required.",
                diagnostics);
        }

        if (duplicateSlotIds.Contains(slotId))
        {
            return InvalidEntry(
                slotId,
                sourceRelativePath,
                slots.GetValueOrDefault(slotId)?.ExpectedOutputRelativePath ?? string.Empty,
                "manual_import.duplicate_slot",
                $"Manifest contains duplicate entries for slot '{slotId}'.",
                diagnostics,
                slots.GetValueOrDefault(slotId)?.ProviderKind ?? UnityArchiveRequestProviderKind.none);
        }

        if (!slots.TryGetValue(slotId, out var slot))
        {
            return InvalidEntry(
                slotId,
                sourceRelativePath,
                string.Empty,
                "manual_import.unknown_slot",
                $"Slot '{slotId}' does not exist in the materialized archive.",
                diagnostics);
        }

        if ((manifestEntry.SourceRelativePath ?? string.Empty).Contains('\\') ||
            !UnityArchiveProviderJobPlanService.IsSafeExpectedOutputRelativePath(sourceRelativePath) ||
            !TryGetContainedArchivePath(archiveRoot, sourceRelativePath, out var sourcePath) ||
            !IsUnderImportDirectory(request.ImportDirectoryRelativePath, sourceRelativePath))
        {
            return InvalidEntry(
                slotId,
                string.Empty,
                slot.ExpectedOutputRelativePath,
                "manual_import.unsafe_source_path",
                "Manifest source path must be relative and stay under the manual import directory.",
                diagnostics,
                slot.ProviderKind);
        }

        if (!File.Exists(sourcePath))
        {
            return InvalidEntry(
                slotId,
                sourceRelativePath,
                slot.ExpectedOutputRelativePath,
                "manual_import.missing_source_file",
                $"Source file '{sourceRelativePath}' was not found.",
                diagnostics,
                slot.ProviderKind);
        }

        byte[] sourceBytes;
        try
        {
            sourceBytes = await File.ReadAllBytesAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            const string code = "manual_import.copy_failed";
            diagnostics.Add(Error(code, "Source file could not be read.", slotId));
            return new UnityArchiveManualProviderImportEntryResult
            {
                SlotId = slotId,
                ProviderKind = slot.ProviderKind,
                SourceRelativePath = sourceRelativePath,
                ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath,
                Status = UnityArchiveManualProviderImportEntryStatus.Failed,
                DiagnosticCodes = [code]
            };
        }

        if (sourceBytes.Length == 0)
        {
            return InvalidEntry(
                slotId,
                sourceRelativePath,
                slot.ExpectedOutputRelativePath,
                "manual_import.empty_source_file",
                $"Source file '{sourceRelativePath}' is empty.",
                diagnostics,
                slot.ProviderKind);
        }

        if (!string.IsNullOrWhiteSpace(manifestEntry.ExpectedOutputRelativePath) &&
            !string.Equals(
                NormalizeRelativePath(manifestEntry.ExpectedOutputRelativePath),
                slot.ExpectedOutputRelativePath,
                StringComparison.Ordinal))
        {
            return InvalidEntry(
                slotId,
                sourceRelativePath,
                slot.ExpectedOutputRelativePath,
                "manual_import.expected_output_mismatch",
                "Manifest expected output path does not exactly match the materialized slot.",
                diagnostics,
                slot.ProviderKind);
        }

        if (!UnityArchiveProviderJobPlanService.IsSafeExpectedOutputRelativePath(slot.ExpectedOutputRelativePath) ||
            !TryGetContainedArchivePath(archiveRoot, slot.ExpectedOutputRelativePath, out var targetPath))
        {
            return InvalidEntry(
                slotId,
                sourceRelativePath,
                string.Empty,
                "manual_import.unsafe_target_path",
                "Materialized slot target path is unsafe.",
                diagnostics,
                slot.ProviderKind);
        }

        if (!string.Equals(Path.GetExtension(sourcePath), Path.GetExtension(targetPath), StringComparison.OrdinalIgnoreCase))
        {
            return InvalidEntry(
                slotId,
                sourceRelativePath,
                slot.ExpectedOutputRelativePath,
                "manual_import.extension_mismatch",
                "Source and target file extensions do not match.",
                diagnostics,
                slot.ProviderKind);
        }

        var sourceHash = ComputeSha256(sourceBytes);
        if (File.Exists(targetPath))
        {
            byte[] targetBytes;
            try
            {
                targetBytes = await File.ReadAllBytesAsync(targetPath, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                const string code = "manual_import.copy_failed";
                diagnostics.Add(Error(code, "Existing target file could not be read.", slotId));
                return new UnityArchiveManualProviderImportEntryResult
                {
                    SlotId = slotId,
                    ProviderKind = slot.ProviderKind,
                    SourceRelativePath = sourceRelativePath,
                    ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath,
                    Status = UnityArchiveManualProviderImportEntryStatus.Failed,
                    FileSizeBytes = sourceBytes.LongLength,
                    ContentSha256 = sourceHash,
                    DiagnosticCodes = [code]
                };
            }
            if (sourceBytes.AsSpan().SequenceEqual(targetBytes))
            {
                return new UnityArchiveManualProviderImportEntryResult
                {
                    SlotId = slotId,
                    ProviderKind = slot.ProviderKind,
                    SourceRelativePath = sourceRelativePath,
                    ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath,
                    Status = UnityArchiveManualProviderImportEntryStatus.AlreadyImported,
                    FileSizeBytes = sourceBytes.LongLength,
                    ContentSha256 = sourceHash
                };
            }

            if (!request.OverwriteExisting)
            {
                const string code = "manual_import.target_conflict";
                diagnostics.Add(Error(code, "Target file exists with different content.", slotId));
                diagnosticCodes.Add(code);
                return new UnityArchiveManualProviderImportEntryResult
                {
                    SlotId = slotId,
                    ProviderKind = slot.ProviderKind,
                    SourceRelativePath = sourceRelativePath,
                    ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath,
                    Status = UnityArchiveManualProviderImportEntryStatus.Conflict,
                    FileSizeBytes = sourceBytes.LongLength,
                    ContentSha256 = sourceHash,
                    DiagnosticCodes = diagnosticCodes
                };
            }
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            await File.WriteAllBytesAsync(targetPath, sourceBytes, cancellationToken).ConfigureAwait(false);
            written.Add(slot.ExpectedOutputRelativePath);
            return new UnityArchiveManualProviderImportEntryResult
            {
                SlotId = slotId,
                ProviderKind = slot.ProviderKind,
                SourceRelativePath = sourceRelativePath,
                ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath,
                Status = UnityArchiveManualProviderImportEntryStatus.Imported,
                FileSizeBytes = sourceBytes.LongLength,
                ContentSha256 = sourceHash
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            const string code = "manual_import.copy_failed";
            diagnostics.Add(Error(code, "Source file could not be copied to the expected output slot.", slotId));
            return new UnityArchiveManualProviderImportEntryResult
            {
                SlotId = slotId,
                ProviderKind = slot.ProviderKind,
                SourceRelativePath = sourceRelativePath,
                ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath,
                Status = UnityArchiveManualProviderImportEntryStatus.Failed,
                FileSizeBytes = sourceBytes.LongLength,
                ContentSha256 = sourceHash,
                DiagnosticCodes = [code]
            };
        }
    }

    private async Task<UnityArchiveManualProviderImportResult> CompleteAsync(
        string archiveRoot,
        UnityArchiveManualProviderImportReadiness? readiness,
        IReadOnlyList<UnityArchiveManualProviderImportEntryResult> entries,
        IReadOnlyList<UnityArchiveManualProviderImportDiagnostic> diagnostics,
        ISet<string> written,
        CancellationToken cancellationToken)
    {
        if (Directory.Exists(archiveRoot))
        {
            AddWritten(written, ReportJsonRelativePath, ReportMarkdownRelativePath);
        }

        var result = BuildResult(readiness, entries, diagnostics, written);
        if (Directory.Exists(archiveRoot))
        {
            await WriteReportsAsync(archiveRoot, result, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    private static UnityArchiveManualProviderImportResult BuildResult(
        UnityArchiveManualProviderImportReadiness? readiness,
        IReadOnlyList<UnityArchiveManualProviderImportEntryResult> entries,
        IReadOnlyList<UnityArchiveManualProviderImportDiagnostic> diagnostics,
        IEnumerable<string> written)
    {
        var orderedEntries = entries
            .OrderBy(entry => entry.SlotId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.SlotId, StringComparer.Ordinal)
            .ThenBy(entry => entry.ExpectedOutputRelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.ExpectedOutputRelativePath, StringComparer.Ordinal)
            .ToList();
        var orderedDiagnostics = diagnostics
            .Distinct()
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.Ordinal)
            .ThenBy(diagnostic => diagnostic.SlotId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.SlotId, StringComparer.Ordinal)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.Ordinal)
            .ToList();
        var resolvedReadiness = readiness ?? DetermineReadiness(orderedEntries, orderedDiagnostics);

        return new UnityArchiveManualProviderImportResult
        {
            Readiness = resolvedReadiness,
            ImportedCount = orderedEntries.Count(entry => entry.Status == UnityArchiveManualProviderImportEntryStatus.Imported),
            SkippedCount = orderedEntries.Count(entry => entry.Status == UnityArchiveManualProviderImportEntryStatus.AlreadyImported),
            ConflictCount = orderedEntries.Count(entry => entry.Status == UnityArchiveManualProviderImportEntryStatus.Conflict),
            InvalidCount = orderedEntries.Count(entry => entry.Status is UnityArchiveManualProviderImportEntryStatus.Invalid or UnityArchiveManualProviderImportEntryStatus.Failed),
            TargetOutputsChanged = orderedEntries.Any(entry => entry.Status == UnityArchiveManualProviderImportEntryStatus.Imported),
            Entries = orderedEntries,
            Diagnostics = orderedDiagnostics,
            WrittenRelativePaths = written
                .Select(NormalizeRelativePath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ThenBy(path => path, StringComparer.Ordinal)
                .ToList()
        };
    }

    private async Task WriteReportsAsync(
        string archiveRoot,
        UnityArchiveManualProviderImportResult result,
        CancellationToken cancellationToken)
    {
        await WriteJsonAsync(archiveRoot, ReportJsonRelativePath, result, cancellationToken).ConfigureAwait(false);
        var markdownPath = GetContainedArchivePath(archiveRoot, ReportMarkdownRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(markdownPath)!);
        await File.WriteAllTextAsync(markdownPath, _markdownRenderer.Render(result), Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
    }

    private static async Task WriteJsonAsync<T>(
        string archiveRoot,
        string relativePath,
        T value,
        CancellationToken cancellationToken)
    {
        var path = GetContainedArchivePath(archiveRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await File.WriteAllTextAsync(path, json, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
    }

    private static UnityArchiveManualProviderImportEntryResult InvalidEntry(
        string slotId,
        string sourceRelativePath,
        string expectedOutputRelativePath,
        string code,
        string message,
        ICollection<UnityArchiveManualProviderImportDiagnostic> diagnostics,
        UnityArchiveRequestProviderKind providerKind = UnityArchiveRequestProviderKind.none)
    {
        diagnostics.Add(Error(code, message, slotId));
        return new UnityArchiveManualProviderImportEntryResult
        {
            SlotId = slotId,
            ProviderKind = providerKind,
            SourceRelativePath = sourceRelativePath,
            ExpectedOutputRelativePath = expectedOutputRelativePath,
            Status = UnityArchiveManualProviderImportEntryStatus.Invalid,
            DiagnosticCodes = [code]
        };
    }

    private static UnityArchiveManualProviderImportReadiness DetermineReadiness(
        IReadOnlyList<UnityArchiveManualProviderImportEntryResult> entries,
        IReadOnlyList<UnityArchiveManualProviderImportDiagnostic> diagnostics)
    {
        if (entries.Any(entry => entry.Status is UnityArchiveManualProviderImportEntryStatus.Conflict or UnityArchiveManualProviderImportEntryStatus.Invalid or UnityArchiveManualProviderImportEntryStatus.Failed) ||
            diagnostics.Any(diagnostic => diagnostic.Severity == UnityArchiveExportDiagnosticSeverity.Error))
        {
            return UnityArchiveManualProviderImportReadiness.BlockedByErrors;
        }

        return diagnostics.Any(diagnostic => diagnostic.Severity == UnityArchiveExportDiagnosticSeverity.Warning)
            ? UnityArchiveManualProviderImportReadiness.ReadyWithWarnings
            : UnityArchiveManualProviderImportReadiness.Ready;
    }

    private static bool IsSafeDirectoryRelativePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath) ||
            relativePath.Contains('\\') || relativePath.Contains(':'))
        {
            return false;
        }

        var normalized = NormalizeRelativePath(relativePath);
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length > 0 &&
               segments.Length == normalized.Split('/').Length &&
               segments.All(segment => segment is not "." and not "..");
    }

    private static bool IsUnderImportDirectory(string importDirectoryRelativePath, string relativePath)
    {
        var importDirectory = NormalizeRelativePath(importDirectoryRelativePath).TrimEnd('/');
        var candidate = NormalizeRelativePath(relativePath);
        return candidate.StartsWith(importDirectory + "/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetContainedArchivePath(string archiveRoot, string relativePath, out string fullPath)
    {
        fullPath = string.Empty;
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath) ||
            relativePath.Contains('\\') || relativePath.Contains(':'))
        {
            return false;
        }

        var normalized = NormalizeRelativePath(relativePath);
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || segments.Length != normalized.Split('/').Length ||
            segments.Any(segment => segment is "." or ".."))
        {
            return false;
        }

        try
        {
            var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(archiveRoot));
            var candidate = Path.GetFullPath(Path.Combine(root, normalized.Replace('/', Path.DirectorySeparatorChar)));
            if (string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) ||
                !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            fullPath = candidate;
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static string GetContainedArchivePath(string archiveRoot, string relativePath)
    {
        if (!TryGetContainedArchivePath(archiveRoot, relativePath, out var path))
        {
            throw new InvalidOperationException($"Archive relative path is unsafe: {NormalizeRelativePath(relativePath)}");
        }

        return path;
    }

    private static string NormalizeRelativePath(string value) => value.Trim().Replace('\\', '/');

    private static string ComputeSha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void AddWritten(ISet<string> written, params string[] paths)
    {
        foreach (var path in paths)
        {
            written.Add(path);
        }
    }

    private static int SeverityOrder(UnityArchiveExportDiagnosticSeverity severity) => severity switch
    {
        UnityArchiveExportDiagnosticSeverity.Error => 0,
        UnityArchiveExportDiagnosticSeverity.Warning => 1,
        _ => 2
    };

    private static UnityArchiveManualProviderImportDiagnostic Error(string code, string message, string slotId) =>
        new() { Severity = UnityArchiveExportDiagnosticSeverity.Error, Code = code, Message = message, SlotId = slotId };

    private static UnityArchiveManualProviderImportDiagnostic Warning(string code, string message, string slotId) =>
        new() { Severity = UnityArchiveExportDiagnosticSeverity.Warning, Code = code, Message = message, SlotId = slotId };

    private sealed record SlotMetadata(
        string SlotId,
        UnityArchiveRequestProviderKind ProviderKind,
        string ExpectedOutputRelativePath);
}
