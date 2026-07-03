using System.Text.Json;

namespace LLMGameCreator.Application.Design.VisualChunkCacheUnityStreamingAssetsHandoff;

internal sealed class VisualChunkCacheUnityStreamingAssetsHandoffReadValidator
{
    private static readonly HashSet<string> BinaryOrRasterMediaExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif",
        ".bmp",
        ".wav",
        ".ogg",
        ".mp3",
        ".mp4",
        ".asset",
        ".bytes"
    };

    private static readonly string[] ProviderLlmNetworkMarkers =
    [
        "LLMProvider",
        "ComfyUI",
        "Fooocus",
        "ProviderCallRequested",
        "HttpClient",
        "UnityWebRequest",
        "WebRequest",
        "TcpClient",
        "http://",
        "https://"
    ];

    public VisualChunkCacheUnitySimulatedReadProof ValidateMirroredPayload(
        string repositoryRootPath,
        Goal095SourceContext context)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var payloadRoot = Resolve(
            root,
            VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.StreamingAssetsRelativeRoot);
        var payloadFiles = new SortedDictionary<string, string>(StringComparer.Ordinal);
        if (Directory.Exists(payloadRoot))
        {
            foreach (var fileName in VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredPayloadFileNames)
            {
                var path = Path.Combine(payloadRoot, fileName);
                if (File.Exists(path))
                {
                    payloadFiles[fileName] = File.ReadAllText(path);
                }
            }
        }

        return ValidatePayloadFiles(root, context, payloadFiles, payloadReadAttempted: true);
    }

    public VisualChunkCacheUnitySimulatedReadProof ValidatePayloadFiles(
        string repositoryRootPath,
        Goal095SourceContext context,
        IReadOnlyDictionary<string, string> payloadFiles,
        bool payloadReadAttempted)
    {
        var diagnostics = new List<VisualChunkCacheUnityHandoffDiagnostic>();
        if (!payloadReadAttempted)
        {
            diagnostics.Add(Error(
                "goal095.proof.file_read_required",
                "StreamingAssets",
                "Unity read proof must read mirrored StreamingAssets files."));
        }

        var requiredPresent = VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredPayloadFileNames
            .All(payloadFiles.ContainsKey);
        foreach (var missing in VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredPayloadFileNames
                     .Where(fileName => !payloadFiles.ContainsKey(fileName)))
        {
            diagnostics.Add(Error(
                "goal095.payload.required_file_missing",
                missing,
                "Required payload file was not read."));
        }

        var manifest = Read<VisualChunkCacheUnityHandoffManifest>(
            payloadFiles,
            VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.HandoffManifestFileName,
            diagnostics);
        var packageIndex = Read<VisualChunkCacheUnityPackageIndex>(
            payloadFiles,
            VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.PackageIndexFileName,
            diagnostics);
        var streamIndex = Read<VisualChunkCacheUnityStreamWindowIndex>(
            payloadFiles,
            VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.StreamWindowIndexFileName,
            diagnostics);
        var chunkLedger = Read<VisualChunkCacheUnityChunkKeyLedger>(
            payloadFiles,
            VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.ChunkKeyLedgerFileName,
            diagnostics);
        var readme = Read<VisualChunkCacheUnityRuntimeReadme>(
            payloadFiles,
            VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.RuntimeReadmeFileName,
            diagnostics);

        var hashesMatch = manifest is not null
                          && HashOf(payloadFiles, VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.PackageIndexFileName) == manifest.PackageIndexHash
                          && HashOf(payloadFiles, VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.StreamWindowIndexFileName) == manifest.StreamWindowIndexHash
                          && HashOf(payloadFiles, VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.ChunkKeyLedgerFileName) == manifest.ChunkKeyLedgerHash
                          && HashOf(payloadFiles, VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.RuntimeReadmeFileName) == manifest.RuntimeReadmeHash;
        var packageCountsMatch = manifest?.PackageCount == context.PackageCount
                                 && packageIndex?.PackageCount == context.PackageCount
                                 && packageIndex?.ExportRecordCount == context.ExportRecordCount
                                 && context.Goal094QualityGate.CacheExportPackageCount == context.PackageCount;
        var windowsRepresented = manifest?.StreamWindowCount == context.StreamWindowCount
                                 && streamIndex?.StreamWindowCount == context.StreamWindowCount
                                 && streamIndex?.StreamWindows.Count == context.StreamWindowCount
                                 && context.Goal094QualityGate.CacheExportStreamWindowCount == context.StreamWindowCount;
        var chunkKeysRepresented = manifest?.UniqueChunkKeyCount == context.UniqueChunkKeyCount
                                   && chunkLedger?.UniqueChunkKeyCount == context.UniqueChunkKeyCount
                                   && chunkLedger?.Entries.Count == context.UniqueChunkKeyCount
                                   && chunkLedger?.ExportRecordCount == context.ExportRecordCount;
        var sidecarMetadataOnly = manifest?.RuntimeHandoffSidecarMetadataOnly == true
                                  && context.RuntimeHandoffSidecarMetadataOnly;
        var countsMatch = packageCountsMatch
                          && windowsRepresented
                          && chunkKeysRepresented
                          && manifest?.ExportRecordCount == context.ExportRecordCount
                          && manifest?.SourceMaterializedChunkCount == context.SourceMaterializedChunkCount;
        var noRawDump = manifest?.NoRawFullWorldDump == true
                        && chunkLedger?.NoRawFullWorldDump == true
                        && readme?.ImplementsRuntimeStreaming == false
                        && !ContainsRawFullWorldDumpMarker(payloadFiles);
        var noAbsolutePaths = manifest?.NoAbsolutePaths == true && !ContainsAbsolutePath(payloadFiles);
        var noBinary = manifest?.NoBinaryOrRasterMedia == true && !ContainsBinaryOrRasterMedia(payloadFiles);
        var noProviderMarkers = !ContainsProviderLlmNetworkMarker(payloadFiles);

        AddIfFalse(diagnostics, hashesMatch, "goal095.proof.payload_hash_mismatch", "manifest", "Payload file hashes must match manifest hashes.");
        AddIfFalse(diagnostics, packageCountsMatch, "goal095.proof.package_count_mismatch", "package-index", "Package count must match Goal093 and Goal094.");
        AddIfFalse(diagnostics, windowsRepresented, "goal095.proof.stream_window_count_mismatch", "stream-window-index", "Stream windows must be represented.");
        AddIfFalse(diagnostics, chunkKeysRepresented, "goal095.proof.chunk_key_ledger_mismatch", "chunk-key-ledger", "Chunk key ledger must represent all unique Goal093 keys.");
        AddIfFalse(diagnostics, sidecarMetadataOnly, "goal095.proof.sidecar_metadata_only_missing", "runtime-sidecar", "Runtime handoff sidecar must remain metadata-only.");
        AddIfFalse(diagnostics, countsMatch, "goal095.proof.counts_mismatch", "payload", "Payload counts must match Goal093/094 source evidence.");
        AddIfFalse(diagnostics, noRawDump, "goal095.proof.raw_full_world_dump", "payload", "Payload must not contain raw full-world dump markers.");
        AddIfFalse(diagnostics, noAbsolutePaths, "goal095.proof.absolute_path", "payload", "Payload must not contain absolute paths.");
        AddIfFalse(diagnostics, noBinary, "goal095.proof.binary_raster_media", "payload", "Payload must not contain binary/raster media references.");
        AddIfFalse(diagnostics, noProviderMarkers, "goal095.proof.provider_marker", "payload", "Payload must not contain provider, LLM, or network call markers.");

        return new VisualChunkCacheUnitySimulatedReadProof
        {
            Passed = diagnostics.All(item => item.Severity != "error")
                     && payloadReadAttempted
                     && requiredPresent
                     && hashesMatch
                     && countsMatch
                     && sidecarMetadataOnly
                     && noRawDump
                     && noAbsolutePaths
                     && noBinary
                     && noProviderMarkers,
            PayloadReadAttempted = payloadReadAttempted,
            ManifestRead = manifest is not null,
            RequiredPayloadFilesPresent = requiredPresent,
            PayloadHashesMatchManifest = hashesMatch,
            PackageCountMatchesGoal093AndGoal094 = packageCountsMatch,
            StreamWindowsRepresented = windowsRepresented,
            ChunkKeysRepresented = chunkKeysRepresented,
            RuntimeHandoffSidecarMetadataOnly = sidecarMetadataOnly,
            CountsMatch = countsMatch,
            NoRawFullWorldDump = noRawDump,
            NoAbsolutePaths = noAbsolutePaths,
            NoBinaryOrRasterMedia = noBinary,
            NoProviderLlmNetworkMarkers = noProviderMarkers,
            PackageCount = manifest?.PackageCount ?? 0,
            ExportRecordCount = manifest?.ExportRecordCount ?? 0,
            StreamWindowCount = manifest?.StreamWindowCount ?? 0,
            UniqueChunkKeyCount = manifest?.UniqueChunkKeyCount ?? 0,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public VisualChunkCacheUnityNegativeProof BuildNegativeProof(
        string repositoryRootPath,
        Goal095SourceContext context,
        IReadOnlyDictionary<string, string> payloadFiles)
    {
        var scenarios = new List<VisualChunkCacheUnityNegativeScenario>
        {
            Scenario(
                "missing_manifest",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    Without(payloadFiles, VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.HandoffManifestFileName),
                    payloadReadAttempted: true).Diagnostics),
            Scenario(
                "tampered_manifest_hash",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    TamperManifestPackageIndexHash(payloadFiles),
                    payloadReadAttempted: true).Diagnostics),
            Scenario(
                "missing_package_index",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    Without(payloadFiles, VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.PackageIndexFileName),
                    payloadReadAttempted: true).Diagnostics),
            Scenario(
                "stream_window_count_mismatch",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    Mutate(payloadFiles, VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.StreamWindowIndexFileName, "\"streamWindowCount\": 5", "\"streamWindowCount\": 4"),
                    payloadReadAttempted: true).Diagnostics),
            Scenario(
                "chunk_key_ledger_mismatch",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    Mutate(payloadFiles, VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.ChunkKeyLedgerFileName, "\"uniqueChunkKeyCount\": 93", "\"uniqueChunkKeyCount\": 92"),
                    payloadReadAttempted: true).Diagnostics),
            Scenario(
                "absolute_path_in_payload",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    Mutate(payloadFiles, VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.RuntimeReadmeFileName, "metadata-only payload", @"C:\temp\metadata-only payload"),
                    payloadReadAttempted: true).Diagnostics),
            Scenario(
                "raw_full_world_dump_marker",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    Mutate(payloadFiles, VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.ChunkKeyLedgerFileName, "\"noRawFullWorldDump\": true", "\"noRawFullWorldDump\": false, \"rawFullWorldDumpMarker\": true"),
                    payloadReadAttempted: true).Diagnostics),
            Scenario(
                "provider_call_marker",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    Mutate(payloadFiles, VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.RuntimeReadmeFileName, "metadata-only payload", "ProviderCallRequested metadata-only payload"),
                    payloadReadAttempted: true).Diagnostics),
            Scenario(
                "fake_success_without_file_read",
                ValidatePayloadFiles(
                    repositoryRootPath,
                    context,
                    payloadFiles,
                    payloadReadAttempted: false).Diagnostics)
        };

        var ordered = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList();
        return new VisualChunkCacheUnityNegativeProof
        {
            Passed = ordered.Count == VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredNegativeScenarioIds.Count
                     && ordered.All(item => item.ActualStatus == "rejected")
                     && ordered.All(item => item.Diagnostics.Count > 0),
            ScenarioCount = ordered.Count,
            Scenarios = ordered
        };
    }

    private static VisualChunkCacheUnityNegativeScenario Scenario(
        string scenarioId,
        IReadOnlyList<VisualChunkCacheUnityHandoffDiagnostic> diagnostics)
    {
        var rejected = diagnostics.Any(item => item.Severity == "error");
        return new VisualChunkCacheUnityNegativeScenario
        {
            ScenarioId = scenarioId,
            ActualStatus = rejected ? "rejected" : "accepted",
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static T? Read<T>(
        IReadOnlyDictionary<string, string> payloadFiles,
        string fileName,
        ICollection<VisualChunkCacheUnityHandoffDiagnostic> diagnostics)
    {
        if (!payloadFiles.TryGetValue(fileName, out var json))
        {
            return default;
        }

        try
        {
            return VisualChunkCacheUnityStreamingAssetsHandoffJson.Deserialize<T>(json);
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            diagnostics.Add(Error("goal095.payload.json_invalid", fileName, exception.Message));
            return default;
        }
    }

    private static SortedDictionary<string, string> Without(
        IReadOnlyDictionary<string, string> payloadFiles,
        string fileName) =>
        new(payloadFiles
            .Where(item => item.Key != fileName)
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal), StringComparer.Ordinal);

    private static SortedDictionary<string, string> Mutate(
        IReadOnlyDictionary<string, string> payloadFiles,
        string fileName,
        string oldValue,
        string newValue) =>
        new(payloadFiles.ToDictionary(
            item => item.Key,
            item => item.Key == fileName
                ? item.Value.Replace(oldValue, newValue, StringComparison.Ordinal)
                : item.Value,
            StringComparer.Ordinal), StringComparer.Ordinal);

    private static SortedDictionary<string, string> TamperManifestPackageIndexHash(
        IReadOnlyDictionary<string, string> payloadFiles)
    {
        if (!payloadFiles.TryGetValue(
                VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.HandoffManifestFileName,
                out var manifestJson))
        {
            return Copy(payloadFiles);
        }

        var manifest = VisualChunkCacheUnityStreamingAssetsHandoffJson.Deserialize<VisualChunkCacheUnityHandoffManifest>(manifestJson);
        if (manifest is null || string.IsNullOrWhiteSpace(manifest.PackageIndexHash))
        {
            return Copy(payloadFiles);
        }

        return Mutate(
            payloadFiles,
            VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.HandoffManifestFileName,
            manifest.PackageIndexHash,
            new string('0', 64));
    }

    private static SortedDictionary<string, string> Copy(
        IReadOnlyDictionary<string, string> payloadFiles) =>
        new(payloadFiles.ToDictionary(
            item => item.Key,
            item => item.Value,
            StringComparer.Ordinal), StringComparer.Ordinal);

    private static string HashOf(IReadOnlyDictionary<string, string> payloadFiles, string fileName) =>
        payloadFiles.TryGetValue(fileName, out var value)
            ? VisualChunkCacheUnityStreamingAssetsHandoffHash.Sha256Text(value)
            : string.Empty;

    private static bool ContainsAbsolutePath(IReadOnlyDictionary<string, string> payloadFiles) =>
        payloadFiles.Values.Any(value =>
            value.Contains(@"C:\", StringComparison.OrdinalIgnoreCase)
            || value.Contains("C:/", StringComparison.OrdinalIgnoreCase)
            || value.Contains("/Users/", StringComparison.OrdinalIgnoreCase)
            || value.Contains(@"\\?\", StringComparison.OrdinalIgnoreCase));

    private static bool ContainsRawFullWorldDumpMarker(IReadOnlyDictionary<string, string> payloadFiles) =>
        payloadFiles.Values.Any(value =>
            value.Contains("\"rawFullWorldDumpMarker\": true", StringComparison.OrdinalIgnoreCase)
            || value.Contains("\"containsRawFullWorldCellDump\": true", StringComparison.OrdinalIgnoreCase)
            || value.Contains("\"noRawFullWorldDump\": false", StringComparison.OrdinalIgnoreCase));

    private static bool ContainsBinaryOrRasterMedia(IReadOnlyDictionary<string, string> payloadFiles) =>
        payloadFiles.Keys.Any(fileName => BinaryOrRasterMediaExtensions.Contains(Path.GetExtension(fileName)))
        || payloadFiles.Values.Any(value => BinaryOrRasterMediaExtensions.Any(ext =>
            value.Contains(ext, StringComparison.OrdinalIgnoreCase)));

    private static bool ContainsProviderLlmNetworkMarker(IReadOnlyDictionary<string, string> payloadFiles) =>
        payloadFiles.Values.Any(value => ProviderLlmNetworkMarkers.Any(marker =>
            value.Contains(marker, StringComparison.OrdinalIgnoreCase)));

    private static string Resolve(string root, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(root, Normalize(relativePath)));
        var normalizedRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root: " + path);
        }

        return path;
    }

    private static string Normalize(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar);

    private static void AddIfFalse(
        ICollection<VisualChunkCacheUnityHandoffDiagnostic> diagnostics,
        bool condition,
        string code,
        string target,
        string message)
    {
        if (!condition)
        {
            diagnostics.Add(Error(code, target, message));
        }
    }

    private static IReadOnlyList<VisualChunkCacheUnityHandoffDiagnostic> SortDiagnostics(
        IEnumerable<VisualChunkCacheUnityHandoffDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(
                item => item.Severity + "|" + item.Code + "|" + item.Target + "|" + item.Message,
                StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.Severity == "error" ? 0 : 1)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static VisualChunkCacheUnityHandoffDiagnostic Error(
        string code,
        string target,
        string message) =>
        VisualChunkCacheUnityHandoffDiagnostic.Error(code, target, message);
}
