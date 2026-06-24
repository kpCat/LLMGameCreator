using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveManualImportTemplateService
{
    public const string ManualImportDirectoryRelativePath = "manual-import";
    public const string TemplateRelativePath = "manual-import/import-manifest.template.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<UnityArchiveManualImportWorkspaceResult> LoadWorkspaceAsync(
        string archiveDirectoryPath,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetArchiveRoot(archiveDirectoryPath, out var archiveRoot) || !Directory.Exists(archiveRoot))
        {
            return new UnityArchiveManualImportWorkspaceResult
            {
                Readiness = UnityArchiveManualImportWorkspaceReadiness.MissingArchive,
                Diagnostics = ["Unity archive folder is missing."]
            };
        }

        var diagnostics = new List<string>();
        var plan = await ReadJsonAsync<UnityArchiveFulfillmentPlan>(
            archiveRoot, "production/fulfillment-plan.json", diagnostics, cancellationToken).ConfigureAwait(false);
        var assetSlots = await ReadJsonAsync<UnityArchiveAssetSlotIndex>(
            archiveRoot, "assets/asset-slots.json", diagnostics, cancellationToken).ConfigureAwait(false);
        var audioSlots = await ReadJsonAsync<UnityArchiveAudioSlotIndex>(
            archiveRoot, "audio/audio-slots.json", diagnostics, cancellationToken).ConfigureAwait(false);
        var luaSlots = await ReadJsonAsync<UnityArchiveLuaModuleSlotIndex>(
            archiveRoot, "lua/module-slots.json", diagnostics, cancellationToken).ConfigureAwait(false);
        var fulfillmentState = await ReadJsonAsync<UnityArchiveFulfillmentStateReport>(
            archiveRoot, "production/fulfillment-state.json", diagnostics, cancellationToken, required: false).ConfigureAwait(false);

        var builders = new Dictionary<string, SlotBuilder>(StringComparer.OrdinalIgnoreCase);
        foreach (var slot in plan.Value?.Slots ?? Array.Empty<UnityArchiveFulfillmentSlot>())
        {
            AddOrMerge(builders, new SlotBuilder(
                slot.SlotId,
                UnityArchiveManualImportSlotKind.Unknown,
                slot.ProviderKind,
                slot.ExpectedOutputRelativePath,
                slot.RequestId,
                slot.SourceRef?.SourceId ?? string.Empty), diagnostics);
        }

        foreach (var slot in assetSlots.Value?.Slots ?? Array.Empty<UnityArchiveAssetSlot>())
        {
            AddOrMerge(builders, new SlotBuilder(
                slot.SlotId,
                UnityArchiveManualImportSlotKind.Asset,
                slot.ProviderKind,
                slot.ExpectedOutputRelativePath,
                slot.RequestId,
                slot.SourceRef?.SourceId ?? slot.AssetId), diagnostics);
        }

        foreach (var slot in audioSlots.Value?.Slots ?? Array.Empty<UnityArchiveAudioSlot>())
        {
            AddOrMerge(builders, new SlotBuilder(
                slot.SlotId,
                UnityArchiveManualImportSlotKind.Audio,
                slot.ProviderKind,
                slot.ExpectedOutputRelativePath,
                slot.RequestId,
                slot.SourceRef?.SourceId ?? slot.AudioId), diagnostics);
        }

        foreach (var slot in luaSlots.Value?.Slots ?? Array.Empty<UnityArchiveLuaModuleSlot>())
        {
            AddOrMerge(builders, new SlotBuilder(
                slot.SlotId,
                UnityArchiveManualImportSlotKind.Lua,
                slot.ProviderKind,
                slot.ExpectedOutputRelativePath,
                slot.ModuleId,
                slot.SourceRef?.SourceId ?? slot.ModuleId), diagnostics);
        }

        var metadataFilesPresent = plan.Exists || assetSlots.Exists || audioSlots.Exists || luaSlots.Exists;
        if (!metadataFilesPresent || builders.Count == 0)
        {
            return new UnityArchiveManualImportWorkspaceResult
            {
                Readiness = metadataFilesPresent && diagnostics.Any(item => item.Contains("invalid", StringComparison.OrdinalIgnoreCase))
                    ? UnityArchiveManualImportWorkspaceReadiness.InvalidSlotMetadata
                    : UnityArchiveManualImportWorkspaceReadiness.MissingSlotMetadata,
                Diagnostics = diagnostics.Count == 0 ? ["No slot metadata was found."] : diagnostics
            };
        }

        var stateBySlot = (fulfillmentState.Value?.Entries ?? Array.Empty<UnityArchiveFulfillmentStateEntry>())
            .Where(entry => !string.IsNullOrWhiteSpace(entry.SlotId))
            .GroupBy(entry => entry.SlotId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var slots = new List<UnityArchiveManualImportWorkspaceSlot>();
        foreach (var builder in builders.Values
                     .OrderBy(item => item.SlotId, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(item => item.SlotId, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physical = await InspectOutputAsync(archiveRoot, builder, cancellationToken).ConfigureAwait(false);
            var status = builder.Invalid
                ? UnityArchiveFulfillmentStatus.invalid
                : stateBySlot.TryGetValue(builder.SlotId, out var state)
                    ? state.Status
                    : physical.Status;
            slots.Add(new UnityArchiveManualImportWorkspaceSlot
            {
                SlotId = builder.SlotId,
                Kind = builder.Kind,
                ProviderKind = builder.ProviderKind,
                ExpectedOutputRelativePath = builder.ExpectedOutputRelativePath,
                Status = status,
                FileExists = physical.FileExists,
                FileSizeBytes = physical.FileSizeBytes,
                ContentSha256 = physical.ContentSha256,
                RequestId = builder.RequestId,
                SourceId = builder.SourceId,
                SuggestedSourceRelativePath = BuildSuggestedSourceRelativePath(builder.SlotId, builder.ExpectedOutputRelativePath)
            });
        }

        var hasInvalidMetadata = plan.Invalid || assetSlots.Invalid || audioSlots.Invalid || luaSlots.Invalid;
        return new UnityArchiveManualImportWorkspaceResult
        {
            Readiness = hasInvalidMetadata
                ? UnityArchiveManualImportWorkspaceReadiness.InvalidSlotMetadata
                : diagnostics.Count > 0
                    ? UnityArchiveManualImportWorkspaceReadiness.ReadyWithWarnings
                    : UnityArchiveManualImportWorkspaceReadiness.Ready,
            Slots = slots,
            Diagnostics = diagnostics
        };
    }

    public async Task<UnityArchiveManualImportTemplateResult> CreateTemplateAsync(
        string archiveDirectoryPath,
        CancellationToken cancellationToken = default)
    {
        var workspace = await LoadWorkspaceAsync(archiveDirectoryPath, cancellationToken).ConfigureAwait(false);
        if (!TryGetArchiveRoot(archiveDirectoryPath, out var archiveRoot) || !Directory.Exists(archiveRoot) || workspace.Slots.Count == 0)
        {
            return new UnityArchiveManualImportTemplateResult
            {
                Status = "Manifest template was not created because no archive slot metadata is available."
            };
        }

        var entries = workspace.Slots
            .Where(slot => slot.Status is UnityArchiveFulfillmentStatus.missing or UnityArchiveFulfillmentStatus.invalid)
            .OrderBy(slot => slot.SlotId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(slot => slot.SlotId, StringComparer.Ordinal)
            .Select(slot => new UnityArchiveManualProviderImportManifestEntry
            {
                SlotId = slot.SlotId,
                SourceRelativePath = slot.SuggestedSourceRelativePath,
                ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath
            })
            .ToList();
        var manifest = new UnityArchiveManualProviderImportManifest { Entries = entries };
        var templatePath = GetContainedPath(archiveRoot, TemplateRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(templatePath)!);
        var json = JsonSerializer.Serialize(manifest, JsonOptions);
        await File.WriteAllTextAsync(templatePath, json, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new UnityArchiveManualImportTemplateResult
        {
            Succeeded = true,
            TemplateRelativePath = TemplateRelativePath,
            TemplateFullPath = templatePath,
            EntryCount = entries.Count,
            Status = $"Manifest template created with {entries.Count} missing/invalid slot entries. Copy it to manual-import/import-manifest.json before running import."
        };
    }

    public UnityArchiveManualImportDirectoryResult EnsureManualImportDirectory(string archiveDirectoryPath)
    {
        if (!TryGetArchiveRoot(archiveDirectoryPath, out var archiveRoot) || !Directory.Exists(archiveRoot))
        {
            return new UnityArchiveManualImportDirectoryResult
            {
                Status = "Manual import folder cannot be created because the Unity archive folder is missing."
            };
        }

        var directoryPath = GetContainedPath(archiveRoot, ManualImportDirectoryRelativePath);
        Directory.CreateDirectory(directoryPath);
        return new UnityArchiveManualImportDirectoryResult
        {
            Succeeded = true,
            DirectoryPath = directoryPath,
            Status = "Manual import folder is ready."
        };
    }

    private static async Task<PhysicalOutputState> InspectOutputAsync(
        string archiveRoot,
        SlotBuilder builder,
        CancellationToken cancellationToken)
    {
        if (!UnityArchiveProviderJobPlanService.IsSafeExpectedOutputRelativePath(builder.ExpectedOutputRelativePath))
        {
            return new PhysicalOutputState(UnityArchiveFulfillmentStatus.invalid, false, 0, string.Empty);
        }

        string path;
        try
        {
            path = GetContainedPath(archiveRoot, builder.ExpectedOutputRelativePath);
        }
        catch (InvalidOperationException)
        {
            return new PhysicalOutputState(UnityArchiveFulfillmentStatus.invalid, false, 0, string.Empty);
        }

        if (!File.Exists(path))
        {
            return new PhysicalOutputState(
                Directory.Exists(path) ? UnityArchiveFulfillmentStatus.invalid : UnityArchiveFulfillmentStatus.missing,
                false,
                0,
                string.Empty);
        }

        try
        {
            var info = new FileInfo(path);
            if (info.Length == 0)
            {
                return new PhysicalOutputState(UnityArchiveFulfillmentStatus.invalid, true, 0, string.Empty);
            }

            await using var stream = File.OpenRead(path);
            var hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
            return new PhysicalOutputState(
                UnityArchiveFulfillmentStatus.available,
                true,
                info.Length,
                Convert.ToHexString(hash).ToLowerInvariant());
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new PhysicalOutputState(UnityArchiveFulfillmentStatus.invalid, true, 0, string.Empty);
        }
    }

    private static async Task<JsonReadResult<T>> ReadJsonAsync<T>(
        string archiveRoot,
        string relativePath,
        ICollection<string> diagnostics,
        CancellationToken cancellationToken,
        bool required = true)
    {
        var path = GetContainedPath(archiveRoot, relativePath);
        if (!File.Exists(path))
        {
            if (required)
            {
                diagnostics.Add($"Missing slot metadata: {relativePath}.");
            }

            return new JsonReadResult<T>();
        }

        try
        {
            await using var stream = File.OpenRead(path);
            var value = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken).ConfigureAwait(false);
            if (value is null)
            {
                diagnostics.Add($"Invalid slot metadata JSON: {relativePath}.");
                return new JsonReadResult<T> { Exists = true, Invalid = true };
            }

            return new JsonReadResult<T> { Exists = true, Value = value };
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            diagnostics.Add($"Invalid or unreadable slot metadata JSON: {relativePath}.");
            return new JsonReadResult<T> { Exists = true, Invalid = true };
        }
    }

    private static void AddOrMerge(
        IDictionary<string, SlotBuilder> builders,
        SlotBuilder incoming,
        ICollection<string> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(incoming.SlotId))
        {
            diagnostics.Add("Slot metadata contains an empty slotId.");
            return;
        }

        if (!builders.TryGetValue(incoming.SlotId, out var existing))
        {
            builders.Add(incoming.SlotId, incoming);
            return;
        }

        if (!string.Equals(existing.ExpectedOutputRelativePath, incoming.ExpectedOutputRelativePath, StringComparison.Ordinal) ||
            existing.ProviderKind != incoming.ProviderKind)
        {
            existing.Invalid = true;
            diagnostics.Add($"Inconsistent slot metadata: {incoming.SlotId}.");
            return;
        }

        if (existing.Kind == UnityArchiveManualImportSlotKind.Unknown && incoming.Kind != UnityArchiveManualImportSlotKind.Unknown)
        {
            existing.Kind = incoming.Kind;
        }

        if (string.IsNullOrWhiteSpace(existing.RequestId))
        {
            existing.RequestId = incoming.RequestId;
        }

        if (string.IsNullOrWhiteSpace(existing.SourceId))
        {
            existing.SourceId = incoming.SourceId;
        }
    }

    private static string BuildSuggestedSourceRelativePath(string slotId, string expectedOutputRelativePath)
    {
        var safeName = new string(slotId.Trim()
            .Select(character => char.IsLetterOrDigit(character) || character is '.' or '-' or '_' ? character : '-')
            .ToArray()).Trim('.', '-', '_');
        if (string.IsNullOrWhiteSpace(safeName) || safeName is "." or "..")
        {
            safeName = Path.GetFileNameWithoutExtension(expectedOutputRelativePath);
        }

        safeName = string.IsNullOrWhiteSpace(safeName) ? "slot-output" : safeName;
        var extension = Path.GetExtension(expectedOutputRelativePath).ToLowerInvariant();
        return $"put-files-here/{safeName}{extension}";
    }

    private static bool TryGetArchiveRoot(string archiveDirectoryPath, out string archiveRoot)
    {
        archiveRoot = string.Empty;
        if (string.IsNullOrWhiteSpace(archiveDirectoryPath))
        {
            return false;
        }

        try
        {
            archiveRoot = Path.GetFullPath(archiveDirectoryPath);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static string GetContainedPath(string archiveRoot, string relativePath)
    {
        if (!UnityArchiveProviderJobPlanService.IsSafeExpectedOutputRelativePath(relativePath))
        {
            throw new InvalidOperationException($"Unsafe Unity archive relative path: {relativePath}");
        }

        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(archiveRoot));
        var candidate = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unity archive relative path escapes the archive root: {relativePath}");
        }

        return candidate;
    }

    private sealed record JsonReadResult<T>
    {
        public bool Exists { get; init; }
        public bool Invalid { get; init; }
        public T? Value { get; init; }
    }

    private sealed class SlotBuilder
    {
        public SlotBuilder(
            string slotId,
            UnityArchiveManualImportSlotKind kind,
            UnityArchiveRequestProviderKind providerKind,
            string expectedOutputRelativePath,
            string requestId,
            string sourceId)
        {
            SlotId = slotId;
            Kind = kind;
            ProviderKind = providerKind;
            ExpectedOutputRelativePath = expectedOutputRelativePath;
            RequestId = requestId;
            SourceId = sourceId;
        }

        public string SlotId { get; }
        public UnityArchiveManualImportSlotKind Kind { get; set; }
        public UnityArchiveRequestProviderKind ProviderKind { get; }
        public string ExpectedOutputRelativePath { get; }
        public string RequestId { get; set; }
        public string SourceId { get; set; }
        public bool Invalid { get; set; }
    }

    private sealed record PhysicalOutputState(
        UnityArchiveFulfillmentStatus Status,
        bool FileExists,
        long FileSizeBytes,
        string ContentSha256);
}
