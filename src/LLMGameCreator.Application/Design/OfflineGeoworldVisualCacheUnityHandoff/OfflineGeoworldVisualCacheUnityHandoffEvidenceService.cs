using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldVisualCacheUnityHandoff;

public sealed partial class OfflineGeoworldVisualCacheUnityHandoffEvidenceService
{
    private const string Goal098Root =
        ".llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract";
    private const string Goal099Root =
        ".llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming";
    private const string Goal093Root =
        ".llmgc/procedural/goal-093-visual-chunk-cache-export-contract";
    private const string Goal095Root =
        ".llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff";
    private const string Goal096Root =
        ".llmgc/procedural/goal-096-unity-handoff-inspector-probe-readiness";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly HashSet<string> ProviderNetworkMarkers = new(StringComparer.OrdinalIgnoreCase)
    {
        "HttpClient",
        "UnityWebRequest",
        "WebRequest",
        "TcpClient",
        "NetworkStream",
        "Socket(",
        "http://",
        "https://",
        "ProviderCallRequested",
        "LLMProvider",
        "ComfyUI",
        "Fooocus"
    };

    private static readonly HashSet<string> BinaryOrRasterExtensions = new(StringComparer.OrdinalIgnoreCase)
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

    public OfflineGeoworldBuildResult Build(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payload = BuildPayload(context);
        var ledger = BuildStreamingAssetsLedger(root, payload.PayloadFiles);
        var probe = BuildProbeSourceInventory(root);
        var readProof = ValidatePayload(context, payload.PayloadFiles, payloadReadAttempted: true);
        var negative = BuildNegativeProof();
        var binding = BuildWorkspaceBindingInventory(root);
        var lineage = BuildSourceLineage(root, context);
        var quality = BuildQualityGate(root, context, payload, ledger, probe, readProof, negative, binding, lineage);
        return BuildResult(context, payload, ledger, probe, readProof, negative, binding, lineage, quality);
    }

    public async Task<OfflineGeoworldWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payload = BuildPayload(context);
        var streamingAssetsDirectory = Resolve(
            root,
            OfflineGeoworldVisualCacheUnityHandoffVocabulary.StreamingAssetsRelativeRoot);
        ResetDirectory(root, streamingAssetsDirectory);

        var written = new List<string>();
        foreach (var item in payload.PayloadFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(streamingAssetsDirectory, item.Key);
            await File.WriteAllTextAsync(path, item.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        var mirroredPayload = ReadPayloadFiles(root);
        var ledger = BuildStreamingAssetsLedger(root, mirroredPayload);
        var probe = BuildProbeSourceInventory(root);
        var readProof = ValidateMirroredPayload(root, context, mirroredPayload);
        var negative = BuildNegativeProof();
        var binding = BuildWorkspaceBindingInventory(root);
        var lineage = BuildSourceLineage(root, context);
        var quality = BuildQualityGate(root, context, payload with { PayloadFiles = mirroredPayload },
            ledger, probe, readProof, negative, binding, lineage);
        var result = BuildResult(
            context,
            payload with { PayloadFiles = mirroredPayload },
            ledger,
            probe,
            readProof,
            negative,
            binding,
            lineage,
            quality);

        var outputDirectory = Resolve(
            root,
            OfflineGeoworldVisualCacheUnityHandoffVocabulary.RelativeOutputDirectory);
        ResetDirectory(root, outputDirectory);
        foreach (var item in result.PayloadJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(outputDirectory, item.Key);
            await File.WriteAllTextAsync(path, item.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in result.EvidenceJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(outputDirectory, item.Key);
            await File.WriteAllTextAsync(path, item.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        var reportPath = Path.Combine(
            outputDirectory,
            OfflineGeoworldVisualCacheUnityHandoffVocabulary.ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new OfflineGeoworldWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StreamingAssetsDirectoryPath = streamingAssetsDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldBuildResult BuildResult(
        Goal100SourceContext context,
        Goal100Payload payload,
        OfflineGeoworldUnityStreamingAssetsLedger ledger,
        OfflineGeoworldUnityProbeSourceInventory probe,
        OfflineGeoworldUnitySimulatedReadProof readProof,
        OfflineGeoworldNegativeProof negative,
        OfflineGeoworldWorkspaceBindingInventory binding,
        OfflineGeoworldSourceLineage lineage,
        OfflineGeoworldQualityGateScan quality)
    {
        var evidence = BuildEvidencePayloads(
            payload,
            ledger,
            probe,
            readProof,
            negative,
            binding,
            lineage,
            quality);
        var reportWithoutHash = BuildReport(context, payload, ledger, readProof, negative, binding, quality, evidence);
        var reportMarkdownWithoutHash = RenderReport(reportWithoutHash, quality);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = Hash(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, quality);
        return new OfflineGeoworldBuildResult
        {
            VisualCacheCatalog = payload.Catalog,
            PackageIndex = payload.PackageIndex,
            FeatureChunkLedger = payload.FeatureChunkLedger,
            HandoffManifest = payload.Manifest,
            StreamWindowIndex = payload.StreamWindowIndex,
            RuntimeReadme = payload.RuntimeReadme,
            StreamingAssetsLedger = ledger,
            ProbeSourceInventory = probe,
            SimulatedReadProof = readProof,
            NegativeProof = negative,
            WorkspaceBindingInventory = binding,
            SourceLineage = lineage,
            QualityGateScan = quality,
            Report = report,
            ReportMarkdown = reportMarkdown,
            PayloadJsonByFileName = payload.PayloadFiles,
            EvidenceJsonByFileName = evidence
        };
    }

    private static Goal100Payload BuildPayload(Goal100SourceContext context)
    {
        var records = BuildRecords(context);
        var featureCountByKind = records
            .GroupBy(item => item.FeatureKind, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(item => item.SourceFeatureId)
                .Distinct(StringComparer.Ordinal).Count(), StringComparer.Ordinal);
        var packages = BuildPackages(context, records);
        var catalog = new OfflineGeoworldVisualCacheCatalog
        {
            SourceBundleId = context.BundleId,
            FeatureCount = context.Features.Count,
            FeatureKindCount = featureCountByKind.Count,
            SourceChunkCount = context.SourceChunkKeys.Count,
            StreamWindowChunkCount = context.RequiredChunkKeys.Count,
            VisualCacheRecordCount = records.Count,
            PackageCount = packages.Count,
            FeatureCountByKind = featureCountByKind,
            Packages = packages,
            Records = records
        };
        var packageIndex = new OfflineGeoworldVisualCachePackageIndex
        {
            PackageCount = packages.Count,
            FeatureCount = context.Features.Count,
            VisualCacheRecordCount = records.Count,
            Packages = packages
        };
        var ledger = new OfflineGeoworldFeatureChunkLedger
        {
            FeatureCount = context.Features.Count,
            VisualCacheRecordCount = records.Count,
            SourceChunkCount = context.SourceChunkKeys.Count,
            FeatureCountByKind = featureCountByKind,
            Records = records
        };
        var streamIndex = new OfflineGeoworldStreamWindowIndex
        {
            CenterChunkKey = context.CenterChunkKey,
            RequiredChunkCount = context.RequiredChunkKeys.Count,
            BoundaryPrefetchChunkCount = context.BoundaryPrefetchChunkKeys.Count,
            BoundaryPrefetchStatus = context.BoundaryPrefetchStatus,
            NetworkFetchAttempted = context.NetworkFetchAttempted,
            RequiredChunkKeys = context.RequiredChunkKeys,
            BoundaryPrefetchChunkKeys = context.BoundaryPrefetchChunkKeys,
            SourceGraphChunkKeys = context.SourceChunkKeys
        };
        var readme = new OfflineGeoworldRuntimeReadme();
        var packageJson = Serialize(packageIndex);
        var ledgerJson = Serialize(ledger);
        var streamJson = Serialize(streamIndex);
        var readmeJson = Serialize(readme);
        var manifest = new OfflineGeoworldUnityHandoffManifest
        {
            PayloadFileCount = OfflineGeoworldVisualCacheUnityHandoffVocabulary.RequiredPayloadFileNames.Count,
            PackageCount = packages.Count,
            FeatureCount = context.Features.Count,
            FeatureKindCount = featureCountByKind.Count,
            VisualCacheRecordCount = records.Count,
            SourceChunkCount = context.SourceChunkKeys.Count,
            StreamWindowChunkCount = context.RequiredChunkKeys.Count,
            PackageIndexHash = Hash(packageJson),
            FeatureChunkLedgerHash = Hash(ledgerJson),
            StreamWindowIndexHash = Hash(streamJson),
            RuntimeReadmeHash = Hash(readmeJson)
        };
        return new Goal100Payload(
            catalog,
            packageIndex,
            ledger,
            manifest,
            streamIndex,
            readme,
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                [OfflineGeoworldVisualCacheUnityHandoffVocabulary.HandoffManifestFileName] =
                    Serialize(manifest),
                [OfflineGeoworldVisualCacheUnityHandoffVocabulary.PackageIndexFileName] = packageJson,
                [OfflineGeoworldVisualCacheUnityHandoffVocabulary.FeatureChunkLedgerFileName] = ledgerJson,
                [OfflineGeoworldVisualCacheUnityHandoffVocabulary.StreamWindowIndexFileName] = streamJson,
                [OfflineGeoworldVisualCacheUnityHandoffVocabulary.RuntimeReadmeFileName] = readmeJson
            });
    }

    private static IReadOnlyList<OfflineGeoworldVisualCacheRecord> BuildRecords(Goal100SourceContext context)
    {
        var records = new List<OfflineGeoworldVisualCacheRecord>();
        foreach (var feature in context.Features.OrderBy(item => item.FeatureId, StringComparer.Ordinal))
        {
            var mapping = MapFeatureKind(feature.Kind);
            foreach (var chunkKey in feature.ChunkKeys.OrderBy(value => value, StringComparer.Ordinal))
            {
                var sourceChunkId = context.ChunkIdByKey.GetValueOrDefault(
                    chunkKey,
                    "geo_chunk/" + Compact(chunkKey));
                var visualChunkKey = "visual_geoworld_chunk/" + Compact(chunkKey);
                var recordHash = Hash(string.Join(
                    "|",
                    feature.FeatureId,
                    mapping.VisualKind,
                    chunkKey,
                    visualChunkKey,
                    mapping.LayerId,
                    feature.LicenseProvenanceSummary));
                records.Add(new OfflineGeoworldVisualCacheRecord
                {
                    RecordId = "cache_record/" + recordHash[..16],
                    SourceFeatureId = feature.FeatureId,
                    SourceFeatureKind = feature.Kind,
                    FeatureKind = mapping.VisualKind,
                    SourceChunkKey = chunkKey,
                    SourceChunkId = sourceChunkId,
                    VisualChunkKey = visualChunkKey,
                    VisualLayerId = mapping.LayerId,
                    CacheRecordHash = recordHash,
                    LicenseProvenanceSummary = feature.LicenseProvenanceSummary
                });
            }
        }

        return records.OrderBy(item => item.RecordId, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<OfflineGeoworldVisualCachePackage> BuildPackages(
        Goal100SourceContext context,
        IReadOnlyList<OfflineGeoworldVisualCacheRecord> records)
    {
        var layerIds = records.Select(item => item.VisualLayerId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();
        return
        [
            Package("geoworld_editor_review_package", "editorReview", context, records, layerIds),
            Package("geoworld_unity_handoff_package", "unityHandoff", context, records, layerIds),
            Package("geoworld_stream_window_runtime_preview_package", "runtimePreviewMetadata",
                context, records, layerIds)
        ];
    }

    private static OfflineGeoworldVisualCachePackage Package(
        string id,
        string targetKind,
        Goal100SourceContext context,
        IReadOnlyList<OfflineGeoworldVisualCacheRecord> records,
        IReadOnlyList<string> layerIds) =>
        new()
        {
            PackageId = id,
            TargetKind = targetKind,
            FeatureCount = context.Features.Count,
            VisualCacheRecordCount = records.Count,
            SourceChunkCount = context.SourceChunkKeys.Count,
            StreamWindowChunkCount = context.RequiredChunkKeys.Count,
            IncludedVisualLayerIds = layerIds,
            PackageHash = Hash(id + "|" + string.Join(
                "|",
                records.Select(item => item.CacheRecordHash).OrderBy(value => value, StringComparer.Ordinal)))
        };

    private static Goal100SourceContext ReadSourceContext(string root)
    {
        var diagnostics = new List<OfflineGeoworldVisualCacheDiagnostic>();
        using var normalized = ReadJson(root, Goal099Root + "/offline-geoworld-normalized-features.json", diagnostics);
        using var graph = ReadJson(root, Goal099Root + "/offline-geoworld-worldsourcegraph.json", diagnostics);
        using var stream = ReadJson(root, Goal099Root + "/offline-geoworld-stream-window-plan.json", diagnostics);
        using var goal099Negative = ReadJson(root, Goal099Root + "/offline-geoworld-negative-proof.json", diagnostics);
        using var goal099Quality = ReadJson(root, Goal099Root + "/offline-geoworld-quality-gate-scan.json", diagnostics);

        var features = normalized is null ? [] : ReadFeatures(normalized.RootElement);
        var chunkIdByKey = graph is null ? new Dictionary<string, string>(StringComparer.Ordinal) : ReadChunks(graph.RootElement);
        var requiredChunks = stream is null ? [] : ReadStringArray(stream.RootElement, "requiredChunkKeys");
        var boundaryChunks = stream is null ? [] : ReadStringArray(stream.RootElement, "boundaryPrefetchChunkKeys");
        var bundleId = normalized is null ? string.Empty : TryGetString(normalized.RootElement, "bundleId");
        var centerChunk = stream is null
            ? string.Empty
            : TryGetString(stream.RootElement.GetProperty("request"), "centerChunkKey");
        var goal099Report = ReadOptionalText(root, Goal099Root + "/offline-geoworld-worldsourcegraph-report.md");
        var goal095Report = ReadOptionalText(root, Goal095Root + "/visual-chunk-cache-unity-handoff-report.md");
        var goal096Report = ReadOptionalText(root, Goal096Root + "/unity-handoff-inspector-report.md");
        var negativePassed = goal099Negative is not null && TryGetBool(goal099Negative.RootElement, "passed");
        var qualityPassed = goal099Quality is not null && TryGetBool(goal099Quality.RootElement, "passed");
        var noNetwork = goal099Report.Contains("noNetworkOrProviderImplementation: true", StringComparison.OrdinalIgnoreCase)
                        || goal099Report.Contains("no live network fetching", StringComparison.OrdinalIgnoreCase);
        var noLfz = goal099Report.Contains("noLfzCodeCopied: true", StringComparison.OrdinalIgnoreCase)
                    || goal099Report.Contains("copies no LFZ source", StringComparison.OrdinalIgnoreCase);
        var existingHandoff = goal095Report.Contains("implementationStatus: GREEN", StringComparison.Ordinal)
                              && goal096Report.Contains("implementationStatus: GREEN", StringComparison.Ordinal);

        AddIfFalse(features.Count == 10, "goal100.source.feature_count", "Goal099 normalized features", diagnostics);
        AddIfFalse(chunkIdByKey.Count == 5, "goal100.source.graph_chunk_count", "Goal099 WorldSourceGraph", diagnostics);
        AddIfFalse(requiredChunks.Count == 9, "goal100.source.stream_window_count", "Goal099 stream window", diagnostics);
        AddIfFalse(negativePassed, "goal100.source.goal099_negative", "Goal099 negative proof", diagnostics);
        AddIfFalse(qualityPassed, "goal100.source.goal099_quality", "Goal099 quality gate", diagnostics);
        AddIfFalse(noNetwork, "goal100.source.goal099_network_boundary", "Goal099 report", diagnostics);
        AddIfFalse(noLfz, "goal100.source.goal099_lfz_boundary", "Goal099 report", diagnostics);
        AddIfFalse(existingHandoff, "goal100.source.visual_handoff_missing", "Goal095/096", diagnostics);

        return new Goal100SourceContext(
            bundleId,
            features,
            chunkIdByKey,
            chunkIdByKey.Keys.OrderBy(value => value, StringComparer.Ordinal).ToList(),
            requiredChunks,
            boundaryChunks,
            centerChunk,
            stream is null ? string.Empty : TryGetString(stream.RootElement, "boundaryPrefetchStatus"),
            stream is not null && TryGetBool(stream.RootElement, "networkFetchAttempted"),
            goal099Report.Contains("- accepted: false", StringComparison.OrdinalIgnoreCase),
            noNetwork,
            noLfz,
            existingHandoff,
            SortDiagnostics(diagnostics));
    }

    private static OfflineGeoworldUnityStreamingAssetsLedger BuildStreamingAssetsLedger(
        string root,
        IReadOnlyDictionary<string, string> payload)
    {
        var diagnostics = new List<OfflineGeoworldVisualCacheDiagnostic>();
        var files = new List<OfflineGeoworldUnityPayloadFile>();
        foreach (var fileName in OfflineGeoworldVisualCacheUnityHandoffVocabulary.RequiredPayloadFileNames)
        {
            payload.TryGetValue(fileName, out var text);
            var fullPath = Resolve(
                root,
                OfflineGeoworldVisualCacheUnityHandoffVocabulary.StreamingAssetsRelativeRoot + "/" + fileName);
            var exists = File.Exists(fullPath) || text is not null;
            AddIfFalse(exists, "goal100.streamingassets.file_missing", fileName, diagnostics);
            files.Add(new OfflineGeoworldUnityPayloadFile
            {
                RelativePath = fileName,
                RepositoryRelativePath =
                    OfflineGeoworldVisualCacheUnityHandoffVocabulary.StreamingAssetsRelativeRoot + "/" + fileName,
                Role = PayloadRole(fileName),
                Sha256 = text is null ? string.Empty : Hash(text),
                ByteCount = text is null ? 0 : Encoding.UTF8.GetByteCount(text),
                Exists = exists
            });
        }

        return new OfflineGeoworldUnityStreamingAssetsLedger
        {
            Passed = diagnostics.Count == 0 && files.Count == 5,
            PayloadFileCount = files.Count(item => item.Exists),
            Files = files.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static OfflineGeoworldBuildResult BuildResult(
        Goal100SourceContext context,
        Goal100Payload payload,
        OfflineGeoworldUnityStreamingAssetsLedger ledger,
        OfflineGeoworldUnityProbeSourceInventory probe,
        OfflineGeoworldUnitySimulatedReadProof readProof,
        OfflineGeoworldNegativeProof negative,
        OfflineGeoworldWorkspaceBindingInventory binding,
        OfflineGeoworldSourceLineage lineage,
        OfflineGeoworldQualityGateScan quality,
        bool unused = false) =>
        BuildResult(context, payload, ledger, probe, readProof, negative, binding, lineage, quality);

    private static IReadOnlyDictionary<string, string> BuildEvidencePayloads(
        Goal100Payload payload,
        OfflineGeoworldUnityStreamingAssetsLedger ledger,
        OfflineGeoworldUnityProbeSourceInventory probe,
        OfflineGeoworldUnitySimulatedReadProof readProof,
        OfflineGeoworldNegativeProof negative,
        OfflineGeoworldWorkspaceBindingInventory binding,
        OfflineGeoworldSourceLineage lineage,
        OfflineGeoworldQualityGateScan quality) =>
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldVisualCacheUnityHandoffVocabulary.VisualCacheCatalogFileName] =
                Serialize(payload.Catalog),
            [OfflineGeoworldVisualCacheUnityHandoffVocabulary.VisualCachePackageIndexFileName] =
                Serialize(payload.PackageIndex),
            [OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityStreamingAssetsLedgerFileName] =
                Serialize(ledger),
            [OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityProbeSourceInventoryFileName] =
                Serialize(probe),
            [OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnitySimulatedReadProofFileName] =
                Serialize(readProof),
            [OfflineGeoworldVisualCacheUnityHandoffVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [OfflineGeoworldVisualCacheUnityHandoffVocabulary.WorkspaceBindingInventoryFileName] =
                Serialize(binding),
            [OfflineGeoworldVisualCacheUnityHandoffVocabulary.SourceLineageFileName] =
                Serialize(lineage),
            [OfflineGeoworldVisualCacheUnityHandoffVocabulary.QualityGateScanFileName] =
                Serialize(quality)
        };

    private static OfflineGeoworldReport BuildReport(
        Goal100SourceContext context,
        Goal100Payload payload,
        OfflineGeoworldUnityStreamingAssetsLedger ledger,
        OfflineGeoworldUnitySimulatedReadProof readProof,
        OfflineGeoworldNegativeProof negative,
        OfflineGeoworldWorkspaceBindingInventory binding,
        OfflineGeoworldQualityGateScan quality,
        IReadOnlyDictionary<string, string> evidence) =>
        new()
        {
            PackageCount = payload.Manifest.PackageCount,
            FeatureCount = payload.Manifest.FeatureCount,
            FeatureKindCount = payload.Manifest.FeatureKindCount,
            VisualCacheRecordCount = payload.Manifest.VisualCacheRecordCount,
            SourceChunkCount = payload.Manifest.SourceChunkCount,
            StreamWindowChunkCount = payload.Manifest.StreamWindowChunkCount,
            UnityPayloadFileCount = ledger.PayloadFileCount,
            SimulatedReadProofPassed = readProof.Passed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = binding.Passed,
            AlphaRuntimeBootstrapUnchanged = quality.AlphaRuntimeBootstrapUnchanged,
            QualityGatePassed = quality.Passed,
            VisualCacheCatalogHash = Hash(Serialize(payload.Catalog)),
            PackageIndexHash = Hash(Serialize(payload.PackageIndex)),
            FeatureChunkLedgerHash = Hash(Serialize(payload.FeatureChunkLedger)),
            HandoffManifestHash = Hash(Serialize(payload.Manifest)),
            StreamingAssetsLedgerHash =
                Hash(evidence[OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityStreamingAssetsLedgerFileName]),
            ProbeSourceInventoryHash =
                Hash(evidence[OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityProbeSourceInventoryFileName]),
            SimulatedReadProofHash =
                Hash(evidence[OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnitySimulatedReadProofFileName]),
            NegativeProofHash =
                Hash(evidence[OfflineGeoworldVisualCacheUnityHandoffVocabulary.NegativeProofFileName]),
            WorkspaceBindingInventoryHash =
                Hash(evidence[OfflineGeoworldVisualCacheUnityHandoffVocabulary.WorkspaceBindingInventoryFileName]),
            SourceLineageHash =
                Hash(evidence[OfflineGeoworldVisualCacheUnityHandoffVocabulary.SourceLineageFileName]),
            QualityGateHash =
                Hash(evidence[OfflineGeoworldVisualCacheUnityHandoffVocabulary.QualityGateScanFileName])
        };

    private static string RenderReport(
        OfflineGeoworldReport report,
        OfflineGeoworldQualityGateScan quality) =>
        string.Join(Environment.NewLine,
        [
            "# Goal 100 Offline Geoworld Visual Cache Unity Handoff",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + report.ManualGate + " required",
            "- deterministicReportHash: " + report.DeterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal100 consumes the real Goal099 synthetic offline geoworld WorldSourceGraph artifacts, projects normalized features into compact visual chunk cache records, mirrors metadata-only payload files into Unity StreamingAssets and surfaces the handoff in the Visual World Stream Preview Workspace. It remains offline/synthetic and implements no live geodata fetching, provider calls, Runtime consumption or live Unity gameplay rendering.",
            string.Empty,
            "## Counts",
            string.Empty,
            "- packageCount: " + report.PackageCount,
            "- featureCount: " + report.FeatureCount,
            "- featureKindCount: " + report.FeatureKindCount,
            "- visualCacheRecordCount: " + report.VisualCacheRecordCount,
            "- sourceChunkCount: " + report.SourceChunkCount,
            "- streamWindowChunkCount: " + report.StreamWindowChunkCount,
            "- unityPayloadFileCount: " + report.UnityPayloadFileCount,
            string.Empty,
            "## Quality Gate",
            string.Empty,
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant(),
            "- allFeatureKindsMapped: " + quality.AllFeatureKindsMapped.ToString().ToLowerInvariant(),
            "- packagesCreated: " + quality.PackagesCreated.ToString().ToLowerInvariant(),
            "- unityPayloadCreated: " + quality.UnityPayloadCreated.ToString().ToLowerInvariant(),
            "- simulatedReadProofPassed: " + report.SimulatedReadProofPassed.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + report.NegativeProofPassed.ToString().ToLowerInvariant(),
            "- workspaceBindingPassed: " + report.WorkspaceBindingPassed.ToString().ToLowerInvariant(),
            "- alphaRuntimeBootstrapUnchanged: " + report.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "- noNetworkOrProviderImplementation: " + quality.NoNetworkOrProviderImplementation.ToString().ToLowerInvariant(),
            "- noLfzCodeCopied: " + quality.NoLfzCodeCopied.ToString().ToLowerInvariant(),
            "- noRawGeodataDump: " + quality.NoRawGeodataDump.ToString().ToLowerInvariant(),
            "- noBinaryOrRasterMedia: " + quality.NoBinaryOrRasterMedia.ToString().ToLowerInvariant(),
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            "- visualCacheCatalogHash: " + report.VisualCacheCatalogHash,
            "- packageIndexHash: " + report.PackageIndexHash,
            "- featureChunkLedgerHash: " + report.FeatureChunkLedgerHash,
            "- handoffManifestHash: " + report.HandoffManifestHash,
            "- streamingAssetsLedgerHash: " + report.StreamingAssetsLedgerHash,
            "- probeSourceInventoryHash: " + report.ProbeSourceInventoryHash,
            "- simulatedReadProofHash: " + report.SimulatedReadProofHash,
            "- negativeProofHash: " + report.NegativeProofHash,
            "- workspaceBindingInventoryHash: " + report.WorkspaceBindingInventoryHash,
            "- sourceLineageHash: " + report.SourceLineageHash,
            "- qualityGateHash: " + report.QualityGateHash
        ]) + Environment.NewLine;

    private sealed record Goal100Payload(
        OfflineGeoworldVisualCacheCatalog Catalog,
        OfflineGeoworldVisualCachePackageIndex PackageIndex,
        OfflineGeoworldFeatureChunkLedger FeatureChunkLedger,
        OfflineGeoworldUnityHandoffManifest Manifest,
        OfflineGeoworldStreamWindowIndex StreamWindowIndex,
        OfflineGeoworldRuntimeReadme RuntimeReadme,
        IReadOnlyDictionary<string, string> PayloadFiles);

    private sealed record Goal100SourceContext(
        string BundleId,
        IReadOnlyList<Goal100Feature> Features,
        IReadOnlyDictionary<string, string> ChunkIdByKey,
        IReadOnlyList<string> SourceChunkKeys,
        IReadOnlyList<string> RequiredChunkKeys,
        IReadOnlyList<string> BoundaryPrefetchChunkKeys,
        string CenterChunkKey,
        string BoundaryPrefetchStatus,
        bool NetworkFetchAttempted,
        bool Goal099AcceptedFalse,
        bool Goal099NoNetworkProviderProven,
        bool Goal099NoLfzCodeCopiedProven,
        bool ExistingVisualCacheHandoffArtifactsObserved,
        IReadOnlyList<OfflineGeoworldVisualCacheDiagnostic> Diagnostics);

    private sealed record Goal100Feature(
        string FeatureId,
        string Kind,
        string LicenseProvenanceSummary,
        IReadOnlyList<string> ChunkKeys);
}
