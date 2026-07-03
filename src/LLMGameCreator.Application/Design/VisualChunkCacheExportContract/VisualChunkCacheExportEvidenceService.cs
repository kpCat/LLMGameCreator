using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.DeterministicVisualChunkStreamWindow;

namespace LLMGameCreator.Application.Design.VisualChunkCacheExportContract;

public sealed class VisualChunkCacheExportEvidenceService
{
    public const string ReportMarkdownFileName = "visual-chunk-cache-export-report.md";
    public const string ManifestJsonFileName = "visual-chunk-cache-export-manifest.json";
    public const string FileLedgerJsonFileName = "visual-chunk-cache-file-ledger.json";
    public const string RuntimeHandoffSidecarJsonFileName = "visual-chunk-cache-runtime-handoff-sidecar.json";
    public const string InvalidationMatrixJsonFileName = "visual-chunk-cache-invalidation-matrix.json";
    public const string ReadbackProofJsonFileName = "visual-chunk-cache-readback-proof.json";
    public const string OverlapReuseProofJsonFileName = "visual-chunk-cache-overlap-reuse-proof.json";
    public const string NegativeProofJsonFileName = "visual-chunk-cache-negative-proof.json";
    public const string SourceLineageJsonFileName = "visual-chunk-cache-source-lineage.json";
    public const string QualityGateScanJsonFileName = "visual-chunk-cache-quality-gate-scan.json";

    private const string Goal090Root = ".llmgc/procedural/goal-090-parameterized-visual-world-profiles";
    private const string Goal091Root = ".llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window";
    private const string Goal092Root = ".llmgc/procedural/goal-092-visual-world-stream-preview-workspace";
    private const string Goal092ARoot = ".llmgc/procedural/goal-092a-visual-world-preview-service-split-source-health";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static VisualChunkCacheExportEvidenceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public VisualChunkCacheEvidenceResult Build(string projectRootPath)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var sourceLineage = BuildSourceLineage(projectRoot);
        var streamCatalog = ReadJson<VisualChunkStreamCatalog>(projectRoot, Goal091Path(DeterministicVisualChunkStreamWindowEvidenceService.CatalogJsonFileName));
        var streamManifest = ReadJson<VisualChunkStreamMaterializationManifest>(projectRoot, Goal091Path(DeterministicVisualChunkStreamWindowEvidenceService.MaterializationManifestJsonFileName));
        var streamLedger = ReadJson<VisualChunkStreamFileLedger>(projectRoot, Goal091Path(DeterministicVisualChunkStreamWindowEvidenceService.FileLedgerJsonFileName));
        var streamCacheProof = ReadJson<VisualChunkStreamCacheReuseProof>(projectRoot, Goal091Path(DeterministicVisualChunkStreamWindowEvidenceService.CacheReuseProofJsonFileName));

        var manifest = BuildManifest(streamManifest, streamCatalog, streamLedger, sourceLineage);
        var knownSourceChunkKeys = streamManifest.Windows
            .SelectMany(item => item.Chunks)
            .Select(item => item.ChunkKey)
            .ToHashSet(StringComparer.Ordinal);
        var sidecar = BuildRuntimeHandoffSidecar(manifest);

        var manifestJson = Serialize(manifest);
        var sidecarJson = Serialize(sidecar);
        var sourceLineageJson = Serialize(sourceLineage);
        var invalidationMatrix = BuildInvalidationMatrix(manifest);
        var invalidationMatrixJson = Serialize(invalidationMatrix);
        var readbackProof = BuildReadbackProof(manifestJson, sidecarJson, knownSourceChunkKeys);
        var overlapReuseProof = BuildOverlapReuseProof(manifest, streamCacheProof);
        var negativeProof = BuildNegativeProof(manifest, sidecar, sourceLineage, knownSourceChunkKeys);
        var qualityGate = BuildQualityGate(manifest, sidecar, readbackProof, overlapReuseProof, negativeProof, sourceLineage);

        var readbackProofJson = Serialize(readbackProof);
        var overlapReuseProofJson = Serialize(overlapReuseProof);
        var negativeProofJson = Serialize(negativeProof);
        var qualityGateJson = Serialize(qualityGate);
        var fileLedger = BuildFileLedger(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ManifestJsonFileName] = manifestJson,
            [RuntimeHandoffSidecarJsonFileName] = sidecarJson,
            [InvalidationMatrixJsonFileName] = invalidationMatrixJson,
            [ReadbackProofJsonFileName] = readbackProofJson,
            [OverlapReuseProofJsonFileName] = overlapReuseProofJson,
            [NegativeProofJsonFileName] = negativeProofJson,
            [SourceLineageJsonFileName] = sourceLineageJson,
            [QualityGateScanJsonFileName] = qualityGateJson
        });
        var fileLedgerJson = Serialize(fileLedger);

        var reportWithoutHash = BuildReport(
            manifest,
            fileLedgerJson,
            sidecarJson,
            invalidationMatrixJson,
            readbackProofJson,
            overlapReuseProofJson,
            negativeProofJson,
            sourceLineageJson,
            qualityGateJson);
        var reportMarkdownWithoutHash = RenderReport(reportWithoutHash, manifest, sidecar, readbackProof, overlapReuseProof, negativeProof, qualityGate, string.Empty);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = VisualChunkCacheExportContractHash.Compute(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, manifest, sidecar, readbackProof, overlapReuseProof, negativeProof, qualityGate, report.DeterministicReportHash);

        return new VisualChunkCacheEvidenceResult
        {
            Manifest = manifest,
            FileLedger = fileLedger,
            RuntimeHandoffSidecar = sidecar,
            InvalidationMatrix = invalidationMatrix,
            ReadbackProof = readbackProof,
            OverlapReuseProof = overlapReuseProof,
            NegativeProof = negativeProof,
            SourceLineage = sourceLineage,
            QualityGateScan = qualityGate,
            Report = report,
            ManifestJson = manifestJson,
            FileLedgerJson = fileLedgerJson,
            RuntimeHandoffSidecarJson = sidecarJson,
            InvalidationMatrixJson = invalidationMatrixJson,
            ReadbackProofJson = readbackProofJson,
            OverlapReuseProofJson = overlapReuseProofJson,
            NegativeProofJson = negativeProofJson,
            SourceLineageJson = sourceLineageJson,
            QualityGateScanJson = qualityGateJson,
            ReportMarkdown = reportMarkdown
        };
    }

    public async Task<VisualChunkCacheWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VisualChunkCacheWriteResult> BuildAndWriteAsync(
        string sourceProjectRootPath,
        string outputProjectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(sourceProjectRootPath);
        return await WriteAsync(outputProjectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VisualChunkCacheWriteResult> WriteAsync(
        string projectRootPath,
        VisualChunkCacheEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(
            projectRoot,
            VisualChunkCacheExportContractVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new VisualChunkCacheWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            ManifestJsonPath = Path.Combine(outputDirectory, ManifestJsonFileName),
            FileLedgerJsonPath = Path.Combine(outputDirectory, FileLedgerJsonFileName),
            RuntimeHandoffSidecarJsonPath = Path.Combine(outputDirectory, RuntimeHandoffSidecarJsonFileName),
            InvalidationMatrixJsonPath = Path.Combine(outputDirectory, InvalidationMatrixJsonFileName),
            ReadbackProofJsonPath = Path.Combine(outputDirectory, ReadbackProofJsonFileName),
            OverlapReuseProofJsonPath = Path.Combine(outputDirectory, OverlapReuseProofJsonFileName),
            NegativeProofJsonPath = Path.Combine(outputDirectory, NegativeProofJsonFileName),
            SourceLineageJsonPath = Path.Combine(outputDirectory, SourceLineageJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName)
        };

        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ManifestJsonPath, result.ManifestJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.FileLedgerJsonPath, result.FileLedgerJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.RuntimeHandoffSidecarJsonPath, result.RuntimeHandoffSidecarJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.InvalidationMatrixJsonPath, result.InvalidationMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ReadbackProofJsonPath, result.ReadbackProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.OverlapReuseProofJsonPath, result.OverlapReuseProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.NegativeProofJsonPath, result.NegativeProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.SourceLineageJsonPath, result.SourceLineageJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.QualityGateScanJsonPath, result.QualityGateScanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return write;
    }

    private static VisualChunkCacheExportManifest BuildManifest(
        VisualChunkStreamMaterializationManifest streamManifest,
        VisualChunkStreamCatalog streamCatalog,
        VisualChunkStreamFileLedger streamLedger,
        VisualChunkCacheSourceLineage sourceLineage)
    {
        var sourceHashes = sourceLineage.Records;
        var packages = new List<VisualChunkCacheExportPackage>
        {
            BuildPackage(VisualChunkCacheExportContractVocabulary.FinitePackageId, VisualChunkCacheExportTargetKind.EditorReview, DeterministicVisualChunkStreamWindowFixtures.FiniteFixtureId, streamManifest, streamCatalog, streamLedger, sourceHashes),
            BuildPackage(VisualChunkCacheExportContractVocabulary.HugeSparsePackageId, VisualChunkCacheExportTargetKind.EditorReview, DeterministicVisualChunkStreamWindowFixtures.HugeSparseFixtureId, streamManifest, streamCatalog, streamLedger, sourceHashes),
            BuildPackage(VisualChunkCacheExportContractVocabulary.InfiniteOverlapPackageId, VisualChunkCacheExportTargetKind.EditorReview, DeterministicVisualChunkStreamWindowFixtures.InfiniteFixtureId, streamManifest, streamCatalog, streamLedger, sourceHashes),
            BuildPackage(VisualChunkCacheExportContractVocabulary.LayerTransitionPackageId, VisualChunkCacheExportTargetKind.RuntimeHandoff, DeterministicVisualChunkStreamWindowFixtures.LayerTransitionFixtureId, streamManifest, streamCatalog, streamLedger, sourceHashes)
        };

        return new VisualChunkCacheExportManifest
        {
            Accepted = false,
            PackageCount = packages.Count,
            ExportRecordCount = packages.Sum(item => item.ExportedRecordCount),
            SourceMaterializedChunkCount = streamManifest.TotalMaterializedChunks,
            SourceUniqueChunkKeyCount = streamManifest.UniqueChunkKeyCount,
            NoAbsolutePaths = packages.SelectMany(item => item.ArtifactRefs).All(item => VisualChunkCacheExportContractValidator.IsSafeRelativePath(item.RelativePath)),
            NoRawFullWorldDump = streamManifest.NoRawFullWorldDump && packages.All(item => item.NoRawFullWorldDump),
            NoBinaryOrRasterMedia = packages.SelectMany(item => item.ArtifactRefs).All(item => !item.IsBinaryOrRaster),
            NoPromptDumps = packages.SelectMany(item => item.ArtifactRefs).All(item => !item.IsPromptDump),
            MetadataOnlyRuntimeHandoff = packages.Where(item => item.ExportTargetKind == VisualChunkCacheExportTargetKind.RuntimeHandoff).All(item => item.MetadataOnly),
            Packages = packages
        };
    }

    private static VisualChunkCacheExportPackage BuildPackage(
        string packageId,
        VisualChunkCacheExportTargetKind targetKind,
        string fixtureId,
        VisualChunkStreamMaterializationManifest streamManifest,
        VisualChunkStreamCatalog streamCatalog,
        VisualChunkStreamFileLedger streamLedger,
        IReadOnlyList<VisualChunkCacheSourceHash> sourceHashes)
    {
        var windows = streamManifest.Windows
            .Where(item => item.FixtureId == fixtureId)
            .OrderBy(item => item.WindowId, StringComparer.Ordinal)
            .ToList();
        var artifactRefs = BuildArtifactRefs(fixtureId, streamCatalog, streamLedger);
        var streamWindowRefs = windows.Select(BuildStreamWindowRef).ToList();
        var records = BuildRecords(packageId, targetKind, windows, artifactRefs);
        var package = new VisualChunkCacheExportPackage
        {
            PackageId = packageId,
            ExportTargetKind = targetKind,
            SourceFixtureId = fixtureId,
            ProfileId = windows.First().ProfileId,
            WorldSeed = windows.First().WorldSeed,
            GeneratorVersion = windows.First().GeneratorVersion,
            StreamWindowCount = windows.Count,
            ExportedRecordCount = records.Count,
            SourceMaterializedChunkCount = windows.Sum(item => item.ChunkCount),
            EstimatedFullWorldChunkCapacity = windows.Where(item => item.EstimatedFullWorldChunkCapacity.HasValue).Select(item => item.EstimatedFullWorldChunkCapacity).FirstOrDefault(),
            NoRawFullWorldDump = windows.All(item => item.NoRawFullWorldDump),
            OnlyMaterializedChunksExported = records.Count <= windows.Sum(item => item.ChunkCount),
            MetadataOnly = true,
            SourceGoalIds = ["goal_090_parameterized_visual_world_profiles", "goal_091_deterministic_visual_chunk_stream_window"],
            SourceHashes = sourceHashes,
            StreamWindows = streamWindowRefs,
            Records = records,
            ArtifactRefs = artifactRefs
        };

        var rules = BuildInvalidationRules(package);
        return package with
        {
            InvalidationRules = rules,
            Records = records.Select(item => item with { InvalidationRules = rules }).ToList()
        };
    }

    private static IReadOnlyList<VisualChunkCacheRecord> BuildRecords(
        string packageId,
        VisualChunkCacheExportTargetKind targetKind,
        IReadOnlyList<VisualChunkStreamWindow> windows,
        IReadOnlyList<VisualChunkCacheArtifactRef> artifactRefs)
    {
        var chunkRows = windows
            .SelectMany(window => window.Chunks.Select(chunk => (window, chunk)))
            .GroupBy(item => item.chunk.ChunkKey, StringComparer.Ordinal)
            .OrderBy(item => item.Key, StringComparer.Ordinal);
        return chunkRows.Select(group =>
        {
            var first = group.OrderBy(item => item.window.WindowId, StringComparer.Ordinal).First();
            var windowIds = group.Select(item => item.window.WindowId).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList();
            var windowsForChunk = group.Select(item => item.window).ToList();
            var overlays = windowsForChunk
                .SelectMany(item => item.DeltaOverlays)
                .GroupBy(item => item.OverlayId, StringComparer.Ordinal)
                .Select(item => item.First())
                .OrderBy(item => item.OverlayId, StringComparer.Ordinal)
                .Select(item => new VisualChunkCacheDeltaOverlayRef
                {
                    OverlayId = item.OverlayId,
                    StableHash = item.StableHash,
                    ChangedChunkCount = item.ChangedChunkCount,
                    ContainsRawCellPayload = item.ContainsRawCellPayload
                })
                .ToList();
            var containsRating = windowsForChunk.Any(item => item.ContainsAdultRatingMetadata);
            var safeFallback = windowsForChunk.Select(item => item.SafeFallbackRefId).FirstOrDefault(item => !string.IsNullOrWhiteSpace(item)) ?? string.Empty;
            return new VisualChunkCacheRecord
            {
                PackageId = packageId,
                ExportTargetKind = targetKind,
                SourceFixtureId = first.window.FixtureId,
                ProfileId = first.window.ProfileId,
                WorldSeed = first.window.WorldSeed,
                GeneratorVersion = first.window.GeneratorVersion,
                Layer = BuildLayerRef(first.window, first.chunk.LayerId, safeFallback),
                CacheKey = new VisualChunkCacheKey
                {
                    ProfileId = first.chunk.ProfileId,
                    LayerId = first.chunk.LayerId,
                    ChunkX = first.chunk.ChunkX,
                    ChunkY = first.chunk.ChunkY,
                    ChunkKey = first.chunk.ChunkKey
                },
                ChunkHash = first.chunk.DeterministicChunkHash,
                StreamWindowIds = windowIds,
                ArtifactRefs = artifactRefs,
                DeltaOverlays = overlays,
                RatingMetadata = new VisualChunkCacheRatingMetadataSummary
                {
                    ContainsAdultRatingMetadata = containsRating,
                    SafeFallbackRefId = safeFallback,
                    SafeFallbackPresent = !containsRating || !string.IsNullOrWhiteSpace(safeFallback),
                    Summary = containsRating ? "rating_metadata_with_safe_fallback" : "non_adult_or_not_present"
                },
                NoRawFullWorldDump = true,
                ContainsRawFullWorldCellDump = false,
                PromptTextIsSourceOfTruth = false
            };
        }).ToList();
    }

    private static VisualChunkCacheStreamWindowRef BuildStreamWindowRef(VisualChunkStreamWindow window)
    {
        var orderedChunkKeys = window.Chunks.Select(item => item.ChunkKey).OrderBy(item => item, StringComparer.Ordinal);
        return new VisualChunkCacheStreamWindowRef
        {
            FixtureId = window.FixtureId,
            WindowId = window.WindowId,
            ProfileId = window.ProfileId,
            LayerId = window.LayerId,
            LayerIds = window.LayerIds.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            SourceChunkCount = window.ChunkCount,
            ExportedRecordCount = window.Chunks.Select(item => item.ChunkKey).Distinct(StringComparer.Ordinal).Count(),
            MembershipStableHash = VisualChunkCacheExportContractHash.Compute(string.Join("|", orderedChunkKeys))
        };
    }

    private static VisualChunkCacheLayerRef BuildLayerRef(VisualChunkStreamWindow window, string layerId, string safeFallback)
    {
        var layer = window.Layers.FirstOrDefault(item => item.LayerId == layerId);
        var linked = window.LayerLinks
            .Where(item => item.FromLayerId == layerId || item.ToLayerId == layerId)
            .SelectMany(item => new[] { item.FromLayerId, item.ToLayerId })
            .Where(item => item != layerId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        return new VisualChunkCacheLayerRef
        {
            LayerId = layerId,
            ProfileId = window.ProfileId,
            LinkedLayerIds = linked.Count > 0 ? linked : layer?.LinkedLayerIds ?? [],
            SafeFallbackRefId = string.IsNullOrWhiteSpace(layer?.SafeFallbackRefId) ? safeFallback : layer.SafeFallbackRefId
        };
    }

    private static IReadOnlyList<VisualChunkCacheArtifactRef> BuildArtifactRefs(
        string fixtureId,
        VisualChunkStreamCatalog streamCatalog,
        VisualChunkStreamFileLedger streamLedger)
    {
        var fixture = streamCatalog.Fixtures.Single(item => item.FixtureId == fixtureId);
        var refs = new List<(string Path, string Kind, IReadOnlyList<string> Tags)>
        {
            (Goal091Path(DeterministicVisualChunkStreamWindowEvidenceService.MaterializationManifestJsonFileName), "source_materialization_manifest_json", ["goal091", "json_evidence"]),
            (Goal091Path(DeterministicVisualChunkStreamWindowEvidenceService.CatalogJsonFileName), "source_catalog_json", ["goal091", "json_evidence"]),
            (Goal091Path(DeterministicVisualChunkStreamWindowEvidenceService.FileLedgerJsonFileName), "source_file_ledger_json", ["goal091", "json_evidence"]),
            (Goal091Path(fixture.OverviewSvgRelativePath), "source_text_svg_preview", ["goal091", "text_svg_preview"])
        };
        if (fixtureId == DeterministicVisualChunkStreamWindowFixtures.InfiniteFixtureId)
        {
            refs.Add((Goal091Path(DeterministicVisualChunkStreamWindowEvidenceService.CacheReuseProofJsonFileName), "source_cache_reuse_proof_json", ["goal091", "cache_reuse"]));
        }

        if (fixtureId == DeterministicVisualChunkStreamWindowFixtures.LayerTransitionFixtureId)
        {
            refs.Add((Goal091Path(DeterministicVisualChunkStreamWindowEvidenceService.LayerTransitionProofJsonFileName), "source_layer_transition_proof_json", ["goal091", "layer_transition"]));
        }

        return refs.Select(item => ToArtifactRef(item.Path, item.Kind, item.Tags, streamLedger)).ToList();
    }

    private static VisualChunkCacheArtifactRef ToArtifactRef(
        string relativePath,
        string kind,
        IReadOnlyList<string> tags,
        VisualChunkStreamFileLedger streamLedger)
    {
        var ledgerPath = relativePath.StartsWith(Goal091Root + "/", StringComparison.Ordinal)
            ? relativePath[(Goal091Root.Length + 1)..]
            : relativePath;
        var ledgerEntry = streamLedger.Files.FirstOrDefault(item => item.RelativePath == ledgerPath);
        return new VisualChunkCacheArtifactRef
        {
            RelativePath = relativePath,
            ArtifactKind = kind,
            Sha256 = ledgerEntry?.Sha256 ?? VisualChunkCacheExportContractHash.Compute(relativePath),
            PurposeTags = tags,
            IsPreviewTextSvg = relativePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase),
            IsBinaryOrRaster = VisualChunkCacheExportContractValidator.IsBinaryOrRasterPath(relativePath),
            IsPromptDump = kind.Contains("prompt", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static IReadOnlyList<VisualChunkCacheInvalidationRule> BuildInvalidationRules(VisualChunkCacheExportPackage package) =>
    [
        Rule("goal090_source_hash", "Goal090 profile source hash", package.SourceHashes.First(item => item.SourceGoalId.StartsWith("goal_090", StringComparison.Ordinal)).Sha256),
        Rule("goal091_manifest_hash", "Goal091 materialization manifest hash", package.ArtifactRefs.First(item => item.RelativePath.EndsWith(DeterministicVisualChunkStreamWindowEvidenceService.MaterializationManifestJsonFileName, StringComparison.Ordinal)).Sha256),
        Rule("generator_version", "Generator version changed", VisualChunkCacheExportContractHash.Compute(package.GeneratorVersion)),
        Rule("profile_id", "Profile id changed", VisualChunkCacheExportContractHash.Compute(package.ProfileId)),
        Rule("world_seed", "World seed changed", VisualChunkCacheExportContractHash.Compute(package.WorldSeed)),
        Rule("stream_window_membership", "Stream-window membership changed", VisualChunkCacheExportContractHash.Compute(string.Join("|", package.StreamWindows.Select(item => item.MembershipStableHash).OrderBy(item => item, StringComparer.Ordinal))))
    ];

    private static VisualChunkCacheInvalidationRule Rule(string key, string reason, string valueHash) =>
        new()
        {
            RuleId = $"invalidate_on_{key}",
            InvalidationKey = key,
            SourceValueHash = valueHash,
            Reason = reason
        };

    private static VisualChunkCacheRuntimeHandoffSidecar BuildRuntimeHandoffSidecar(VisualChunkCacheExportManifest manifest)
    {
        var package = manifest.Packages.Single(item => item.PackageId == VisualChunkCacheExportContractVocabulary.LayerTransitionPackageId);
        return new VisualChunkCacheRuntimeHandoffSidecar
        {
            Accepted = false,
            RecordCount = package.ExportedRecordCount,
            StreamWindowIds = package.StreamWindows.Select(item => item.WindowId).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            LayerIds = package.Records.Select(item => item.Layer.LayerId).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            ArtifactRefs = package.ArtifactRefs
        };
    }

    private static VisualChunkCacheInvalidationMatrix BuildInvalidationMatrix(VisualChunkCacheExportManifest manifest)
    {
        var rows = manifest.Packages
            .OrderBy(item => item.PackageId, StringComparer.Ordinal)
            .SelectMany(package => package.InvalidationRules.Select(rule => new VisualChunkCacheInvalidationMatrixRow
            {
                PackageId = package.PackageId,
                RuleId = rule.RuleId,
                InvalidationKey = rule.InvalidationKey,
                SourceValueHash = rule.SourceValueHash,
                KnownKey = true
            }))
            .ToList();
        return new VisualChunkCacheInvalidationMatrix
        {
            Passed = rows.Count >= manifest.Packages.Count * 4,
            PackageCount = manifest.Packages.Count,
            RuleCount = rows.Count,
            Rows = rows
        };
    }

    private static VisualChunkCacheReadbackProof BuildReadbackProof(
        string manifestJson,
        string sidecarJson,
        IReadOnlySet<string> knownSourceChunkKeys)
    {
        var manifest = JsonSerializer.Deserialize<VisualChunkCacheExportManifest>(manifestJson, JsonOptions);
        var sidecar = JsonSerializer.Deserialize<VisualChunkCacheRuntimeHandoffSidecar>(sidecarJson, JsonOptions);
        var manifestValidation = manifest == null
            ? new VisualChunkCacheValidationResult { Diagnostics = [VisualChunkCacheDiagnostic.Error("visual_chunk_cache.readback.manifest_parse", "manifest", "Manifest JSON did not deserialize.")] }
            : VisualChunkCacheExportContractValidator.ValidateManifest(manifest, knownSourceChunkKeys);
        var sidecarValidation = sidecar == null
            ? new VisualChunkCacheValidationResult { Diagnostics = [VisualChunkCacheDiagnostic.Error("visual_chunk_cache.readback.sidecar_parse", "sidecar", "Sidecar JSON did not deserialize.")] }
            : VisualChunkCacheExportContractValidator.ValidateRuntimeHandoffSidecar(sidecar);
        var diagnostics = manifestValidation.Diagnostics.Concat(sidecarValidation.Diagnostics).ToList();
        return new VisualChunkCacheReadbackProof
        {
            Passed = manifest != null && sidecar != null && manifestValidation.Passed && sidecarValidation.Passed,
            ManifestRoundTripPassed = manifest != null,
            RuntimeHandoffSidecarRoundTripPassed = sidecar != null,
            ManifestValidationPassed = manifestValidation.Passed,
            RuntimeHandoffSidecarValidationPassed = sidecarValidation.Passed,
            PackageCount = manifest?.PackageCount ?? 0,
            ExportRecordCount = manifest?.ExportRecordCount ?? 0,
            Diagnostics = VisualChunkCacheExportContractValidator.SortDiagnostics(diagnostics)
        };
    }

    private static VisualChunkCacheOverlapReuseProof BuildOverlapReuseProof(
        VisualChunkCacheExportManifest manifest,
        VisualChunkStreamCacheReuseProof streamCacheProof)
    {
        var package = manifest.Packages.Single(item => item.PackageId == VisualChunkCacheExportContractVocabulary.InfiniteOverlapPackageId);
        var rows = package.Records
            .Where(item => item.StreamWindowIds.Count > 1)
            .OrderBy(item => item.CacheKey.ChunkKey, StringComparer.Ordinal)
            .Select(item => new VisualChunkCacheOverlapReuseRow
            {
                ChunkKey = item.CacheKey.ChunkKey,
                ChunkHash = item.ChunkHash,
                LayerId = item.CacheKey.LayerId,
                ChunkX = item.CacheKey.ChunkX,
                ChunkY = item.CacheKey.ChunkY,
                StreamWindowIds = item.StreamWindowIds
            })
            .ToList();
        return new VisualChunkCacheOverlapReuseProof
        {
            Passed = rows.Count == streamCacheProof.InfiniteOverlapReusedChunkKeyCount && rows.All(item => item.StreamWindowIds.Count > 1),
            SourceGoal091ReusedChunkKeyCount = streamCacheProof.InfiniteOverlapReusedChunkKeyCount,
            ExportReusedChunkKeyCount = rows.Count,
            Rows = rows
        };
    }

    private static VisualChunkCacheNegativeProof BuildNegativeProof(
        VisualChunkCacheExportManifest manifest,
        VisualChunkCacheRuntimeHandoffSidecar sidecar,
        VisualChunkCacheSourceLineage sourceLineage,
        IReadOnlySet<string> knownSourceChunkKeys)
    {
        var firstPackage = manifest.Packages.First();
        var firstRecord = firstPackage.Records.First();
        var secondRecord = firstPackage.Records.Skip(1).First();
        var hugePackage = manifest.Packages.Single(item => item.PackageId == VisualChunkCacheExportContractVocabulary.HugeSparsePackageId);
        var scenarios = new List<VisualChunkCacheNegativeScenario>
        {
            InvalidManifest("unknown_source_chunk_key", "unknown source chunk key", ReplaceRecord(manifest, firstRecord with { CacheKey = firstRecord.CacheKey with { ChunkKey = "unknown_source_chunk_key" } }), knownSourceChunkKeys),
            InvalidManifest("absolute_artifact_path", "absolute artifact path", ReplaceRecord(manifest, firstRecord with { ArtifactRefs = ReplaceFirstArtifact(firstRecord.ArtifactRefs, "C:\\temp\\visual-cache.json") }), knownSourceChunkKeys),
            InvalidManifest("missing_chunk_hash", "missing chunk hash", ReplaceRecord(manifest, firstRecord with { ChunkHash = "" }), knownSourceChunkKeys),
            InvalidManifest("duplicate_chunk_key_conflicting_hash", "duplicate chunk key with conflicting hash", ReplaceRecord(manifest, secondRecord with { CacheKey = secondRecord.CacheKey with { ChunkKey = firstRecord.CacheKey.ChunkKey }, ChunkHash = VisualChunkCacheExportContractHash.Compute("conflicting") }), knownSourceChunkKeys),
            InvalidManifest("stream_window_membership_mismatch", "stream window membership mismatch", ReplaceRecord(manifest, firstRecord with { StreamWindowIds = ["missing_window"] }), knownSourceChunkKeys),
            InvalidManifest("raw_full_world_dump", "huge export attempts raw dump", ReplacePackage(manifest, hugePackage with { NoRawFullWorldDump = false }), knownSourceChunkKeys),
            InvalidSourceLineage("missing_goal090_lineage", "missing source lineage to Goal090", sourceLineage with { Goal090LineagePresent = false }),
            InvalidManifest("stale_generator_version", "stale generator version mismatch", ReplaceRecord(manifest, firstRecord with { GeneratorVersion = "stale-generator-version" }), knownSourceChunkKeys),
            InvalidManifest("unknown_invalidation_key", "cache invalidation rule with unknown key", ReplacePackage(manifest, firstPackage with { InvalidationRules = [firstPackage.InvalidationRules[0] with { InvalidationKey = "unknown_invalidation_key" }] }), knownSourceChunkKeys),
            InvalidSidecar("runtime_handoff_provider_call", "runtime handoff sidecar with provider call instructions", sidecar with { ContainsProviderCalls = true }),
            InvalidManifest("prompt_text_source_of_truth", "prompt text as source of truth", ReplaceRecord(manifest, firstRecord with { PromptTextIsSourceOfTruth = true }), knownSourceChunkKeys),
            InvalidManifest("rating_metadata_without_safe_fallback", "adult/rating metadata without safe fallback", ReplaceRecord(manifest, firstRecord with { RatingMetadata = firstRecord.RatingMetadata with { ContainsAdultRatingMetadata = true, SafeFallbackPresent = false, SafeFallbackRefId = "" } }), knownSourceChunkKeys),
            InvalidManifest("binary_raster_artifact_ref", "binary/raster artifact ref", ReplaceRecord(manifest, firstRecord with { ArtifactRefs = ReplaceFirstArtifact(firstRecord.ArtifactRefs, "cache-preview.png") }), knownSourceChunkKeys)
        };
        return new VisualChunkCacheNegativeProof
        {
            Passed = scenarios.Count >= 13
                && scenarios.All(item => !item.ActualValid && item.ExpectedValid == item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error")),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedValid == item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private static VisualChunkCacheQualityGateScan BuildQualityGate(
        VisualChunkCacheExportManifest manifest,
        VisualChunkCacheRuntimeHandoffSidecar sidecar,
        VisualChunkCacheReadbackProof readbackProof,
        VisualChunkCacheOverlapReuseProof overlapReuseProof,
        VisualChunkCacheNegativeProof negativeProof,
        VisualChunkCacheSourceLineage sourceLineage)
    {
        var diagnostics = new List<VisualChunkCacheDiagnostic>();
        var manifestValidation = VisualChunkCacheExportContractValidator.ValidateManifest(manifest);
        var sidecarValidation = VisualChunkCacheExportContractValidator.ValidateRuntimeHandoffSidecar(sidecar);
        var sourceValidation = VisualChunkCacheExportContractValidator.ValidateSourceLineage(sourceLineage);
        diagnostics.AddRange(manifestValidation.Diagnostics);
        diagnostics.AddRange(sidecarValidation.Diagnostics);
        diagnostics.AddRange(sourceValidation.Diagnostics);

        var finite = manifest.Packages.Any(item => item.PackageId == VisualChunkCacheExportContractVocabulary.FinitePackageId);
        var huge = manifest.Packages.Any(item => item.PackageId == VisualChunkCacheExportContractVocabulary.HugeSparsePackageId && item.OnlyMaterializedChunksExported);
        var infinite = manifest.Packages.Any(item => item.PackageId == VisualChunkCacheExportContractVocabulary.InfiniteOverlapPackageId);
        var layer = sidecar.MetadataOnly && sidecar.RecordCount > 0;
        AddIfFalse(readbackProof.Passed, "visual_chunk_cache.readback.failed", "readback_proof", "Readback proof must pass.", diagnostics);
        AddIfFalse(overlapReuseProof.Passed, "visual_chunk_cache.overlap_reuse.failed", "overlap_reuse_proof", "Overlap reuse proof must pass.", diagnostics);
        AddIfFalse(negativeProof.Passed, "visual_chunk_cache.negative.failed", "negative_proof", "Negative proof must reject expected cases.", diagnostics);

        var passed = diagnostics.All(item => item.Severity != "error");
        return new VisualChunkCacheQualityGateScan
        {
            Accepted = false,
            Passed = passed,
            FiniteExportExists = finite,
            HugeSparseExportExists = huge,
            InfiniteOverlapExportExists = infinite,
            LayerTransitionRuntimeHandoffExists = layer,
            ReadbackProofPassed = readbackProof.Passed,
            OverlapReuseProofPassed = overlapReuseProof.Passed,
            NegativeProofPassed = negativeProof.Passed,
            SourceLineagePassed = sourceLineage.Passed,
            NoAbsolutePaths = manifest.NoAbsolutePaths,
            NoRawFullWorldDump = manifest.NoRawFullWorldDump,
            NoBinaryOrRasterMediaAdded = manifest.NoBinaryOrRasterMedia,
            NoPromptDumps = manifest.NoPromptDumps,
            RuntimeHandoffSidecarMetadataOnly = sidecar.MetadataOnly && !sidecar.ContainsRuntimeExecution && !sidecar.ContainsProviderCalls && !sidecar.ContainsUnityImplementation,
            ArtifactScopeReady = true,
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/VisualChunkCacheExportContract/",
                "tests/LLMGameCreator.Tests/Application/VisualChunkCacheExportContract/",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualChunkCacheExportContractProductSmokeTests.cs",
                ".llmgc/procedural/goal-093-visual-chunk-cache-export-contract/",
                "docs/agent-tasks/goal-093-visual-chunk-cache-export-contract/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = VisualChunkCacheExportContractValidator.SortDiagnostics(diagnostics)
        };
    }

    private static VisualChunkCacheSourceLineage BuildSourceLineage(string projectRoot)
    {
        var records = SourceInputs()
            .Select(item => SourceRecord(projectRoot, item.Path, item.GoalId, item.Tags))
            .ToList();
        return new VisualChunkCacheSourceLineage
        {
            Passed = records.All(item => !string.IsNullOrWhiteSpace(item.Sha256))
                && records.Any(item => item.SourceGoalId.StartsWith("goal_090", StringComparison.Ordinal))
                && records.Any(item => item.SourceGoalId.StartsWith("goal_091", StringComparison.Ordinal)),
            Goal090LineagePresent = records.Any(item => item.SourceGoalId.StartsWith("goal_090", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(item.Sha256)),
            Goal091LineagePresent = records.Any(item => item.SourceGoalId.StartsWith("goal_091", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(item.Sha256)),
            Goal092PreviewLineagePresent = records.Any(item => item.SourceGoalId.StartsWith("goal_092", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(item.Sha256)),
            SourceRecordCount = records.Count,
            Records = records
        };
    }

    private static IReadOnlyList<(string Path, string GoalId, IReadOnlyList<string> Tags)> SourceInputs() =>
    [
        ($"{Goal090Root}/visual-world-profile-report.md", "goal_090_parameterized_visual_world_profiles", ["goal090", "report"]),
        ($"{Goal090Root}/visual-world-profile-catalog.json", "goal_090_parameterized_visual_world_profiles", ["goal090", "catalog"]),
        ($"{Goal091Root}/visual-chunk-stream-window-report.md", "goal_091_deterministic_visual_chunk_stream_window", ["goal091", "report"]),
        ($"{Goal091Root}/visual-chunk-stream-window-catalog.json", "goal_091_deterministic_visual_chunk_stream_window", ["goal091", "catalog"]),
        ($"{Goal091Root}/visual-chunk-stream-materialization-manifest.json", "goal_091_deterministic_visual_chunk_stream_window", ["goal091", "manifest"]),
        ($"{Goal091Root}/visual-chunk-stream-file-ledger.json", "goal_091_deterministic_visual_chunk_stream_window", ["goal091", "ledger"]),
        ($"{Goal091Root}/visual-chunk-stream-determinism-proof.json", "goal_091_deterministic_visual_chunk_stream_window", ["goal091", "determinism"]),
        ($"{Goal091Root}/visual-chunk-stream-cache-reuse-proof.json", "goal_091_deterministic_visual_chunk_stream_window", ["goal091", "cache_reuse"]),
        ($"{Goal091Root}/visual-chunk-stream-layer-transition-proof.json", "goal_091_deterministic_visual_chunk_stream_window", ["goal091", "layer_transition"]),
        ($"{Goal092Root}/visual-world-stream-preview-workspace-report.md", "goal_092_visual_world_stream_preview_workspace", ["goal092", "preview_workspace"]),
        ($"{Goal092ARoot}/source-health-before-after.json", "goal_092a_visual_world_preview_service_split_source_health", ["goal092a", "source_health"]),
        ($"{Goal092ARoot}/quality-gate-scan.json", "goal_092a_visual_world_preview_service_split_source_health", ["goal092a", "quality"])
    ];

    private static VisualChunkCacheSourceHash SourceRecord(
        string projectRoot,
        string relativePath,
        string goalId,
        IReadOnlyList<string> tags)
    {
        var fullPath = ResolvePath(projectRoot, relativePath);
        var text = File.Exists(fullPath) ? File.ReadAllText(fullPath, Encoding.UTF8) : string.Empty;
        return new VisualChunkCacheSourceHash
        {
            SourceGoalId = goalId,
            RelativePath = relativePath,
            Sha256 = string.IsNullOrWhiteSpace(text) ? string.Empty : VisualChunkCacheExportContractHash.Compute(text),
            PurposeTags = tags
        };
    }

    private static VisualChunkCacheFileLedger BuildFileLedger(IReadOnlyDictionary<string, string> files)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new VisualChunkCacheFileLedgerEntry
            {
                RelativePath = item.Key,
                Sha256 = VisualChunkCacheExportContractHash.Compute(item.Value),
                ByteLength = Encoding.UTF8.GetByteCount(item.Value),
                PurposeTags = ["json_evidence"]
            })
            .ToList();
        return new VisualChunkCacheFileLedger
        {
            Passed = entries.Count >= 8 && entries.All(item => item.Sha256.Length == 64 && item.ByteLength > 0),
            FileCount = entries.Count,
            Files = entries
        };
    }

    private static VisualChunkCacheExportReport BuildReport(
        VisualChunkCacheExportManifest manifest,
        string fileLedgerJson,
        string sidecarJson,
        string invalidationMatrixJson,
        string readbackProofJson,
        string overlapReuseProofJson,
        string negativeProofJson,
        string sourceLineageJson,
        string qualityGateJson) =>
        new()
        {
            Accepted = false,
            PackageCount = manifest.PackageCount,
            ExportRecordCount = manifest.ExportRecordCount,
            SourceMaterializedChunkCount = manifest.SourceMaterializedChunkCount,
            ReadbackProofPassed = JsonSerializer.Deserialize<VisualChunkCacheReadbackProof>(readbackProofJson, JsonOptions)?.Passed == true,
            OverlapReuseProofPassed = JsonSerializer.Deserialize<VisualChunkCacheOverlapReuseProof>(overlapReuseProofJson, JsonOptions)?.Passed == true,
            NegativeProofPassed = JsonSerializer.Deserialize<VisualChunkCacheNegativeProof>(negativeProofJson, JsonOptions)?.Passed == true,
            SourceLineagePassed = JsonSerializer.Deserialize<VisualChunkCacheSourceLineage>(sourceLineageJson, JsonOptions)?.Passed == true,
            QualityGatePassed = JsonSerializer.Deserialize<VisualChunkCacheQualityGateScan>(qualityGateJson, JsonOptions)?.Passed == true,
            ManifestHash = VisualChunkCacheExportContractHash.Compute(Serialize(manifest)),
            FileLedgerHash = VisualChunkCacheExportContractHash.Compute(fileLedgerJson),
            RuntimeHandoffSidecarHash = VisualChunkCacheExportContractHash.Compute(sidecarJson),
            InvalidationMatrixHash = VisualChunkCacheExportContractHash.Compute(invalidationMatrixJson),
            ReadbackProofHash = VisualChunkCacheExportContractHash.Compute(readbackProofJson),
            OverlapReuseProofHash = VisualChunkCacheExportContractHash.Compute(overlapReuseProofJson),
            NegativeProofHash = VisualChunkCacheExportContractHash.Compute(negativeProofJson),
            SourceLineageHash = VisualChunkCacheExportContractHash.Compute(sourceLineageJson),
            QualityGateHash = VisualChunkCacheExportContractHash.Compute(qualityGateJson)
        };

    private static string RenderReport(
        VisualChunkCacheExportReport report,
        VisualChunkCacheExportManifest manifest,
        VisualChunkCacheRuntimeHandoffSidecar sidecar,
        VisualChunkCacheReadbackProof readbackProof,
        VisualChunkCacheOverlapReuseProof overlapReuseProof,
        VisualChunkCacheNegativeProof negativeProof,
        VisualChunkCacheQualityGateScan qualityGate,
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 093 Visual Chunk Cache Export Contract Report",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- manualGate: {report.ManualGate} required",
            $"- deterministicReportHash: {deterministicReportHash}",
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 093 adds a BCL-only Application-side visual chunk cache/export contract and runtime-handoff sidecar over real Goal 091 stream-window artifacts. It creates compact metadata-only cache packages for finite, huge sparse, infinite-overlap and layer-transition exports without Runtime, Unity, provider, public schema, Lua, generator-library, project-file, dependency, binary/raster media or prompt-output changes.",
            string.Empty,
            "## Export Packages",
            string.Empty
        };
        lines.AddRange(manifest.Packages.Select(item =>
            "- " + item.PackageId + ": target=" + item.ExportTargetKind
            + ", profile=" + item.ProfileId
            + ", windows=" + item.StreamWindowCount
            + ", records=" + item.ExportedRecordCount
            + ", sourceChunks=" + item.SourceMaterializedChunkCount
            + ", noRawFullWorldDump=" + item.NoRawFullWorldDump.ToString().ToLowerInvariant()));
        lines.AddRange(
        [
            string.Empty,
            "## Runtime Handoff Sidecar",
            string.Empty,
            $"- sidecarId: {sidecar.SidecarId}",
            $"- packageId: {sidecar.PackageId}",
            $"- metadataOnly: {sidecar.MetadataOnly.ToString().ToLowerInvariant()}",
            $"- containsRuntimeExecution: {sidecar.ContainsRuntimeExecution.ToString().ToLowerInvariant()}",
            $"- containsProviderCalls: {sidecar.ContainsProviderCalls.ToString().ToLowerInvariant()}",
            $"- containsUnityImplementation: {sidecar.ContainsUnityImplementation.ToString().ToLowerInvariant()}",
            $"- recordCount: {sidecar.RecordCount}",
            $"- layers: {string.Join(",", sidecar.LayerIds)}",
            string.Empty,
            "## Proofs",
            string.Empty,
            $"- readbackProofPassed: {readbackProof.Passed.ToString().ToLowerInvariant()}",
            $"- manifestRoundTripPassed: {readbackProof.ManifestRoundTripPassed.ToString().ToLowerInvariant()}",
            $"- runtimeHandoffSidecarRoundTripPassed: {readbackProof.RuntimeHandoffSidecarRoundTripPassed.ToString().ToLowerInvariant()}",
            $"- overlapReuseProofPassed: {overlapReuseProof.Passed.ToString().ToLowerInvariant()}",
            $"- sourceGoal091ReusedChunkKeyCount: {overlapReuseProof.SourceGoal091ReusedChunkKeyCount}",
            $"- exportReusedChunkKeyCount: {overlapReuseProof.ExportReusedChunkKeyCount}",
            $"- negativeProofPassed: {negativeProof.Passed.ToString().ToLowerInvariant()}",
            $"- negativeScenarioCount: {negativeProof.ScenarioCount}",
            $"- rejectedNegativeScenarioCount: {negativeProof.RejectedCount}",
            string.Empty,
            "## Quality Gate",
            string.Empty,
            $"- qualityGatePassed: {qualityGate.Passed.ToString().ToLowerInvariant()}",
            $"- finiteExportExists: {qualityGate.FiniteExportExists.ToString().ToLowerInvariant()}",
            $"- hugeSparseExportExists: {qualityGate.HugeSparseExportExists.ToString().ToLowerInvariant()}",
            $"- infiniteOverlapExportExists: {qualityGate.InfiniteOverlapExportExists.ToString().ToLowerInvariant()}",
            $"- layerTransitionRuntimeHandoffExists: {qualityGate.LayerTransitionRuntimeHandoffExists.ToString().ToLowerInvariant()}",
            $"- noAbsolutePaths: {qualityGate.NoAbsolutePaths.ToString().ToLowerInvariant()}",
            $"- noRawFullWorldDump: {qualityGate.NoRawFullWorldDump.ToString().ToLowerInvariant()}",
            $"- noBinaryOrRasterMediaAdded: {qualityGate.NoBinaryOrRasterMediaAdded.ToString().ToLowerInvariant()}",
            $"- noPromptDumps: {qualityGate.NoPromptDumps.ToString().ToLowerInvariant()}",
            $"- noRuntimeUnityProviderSchemaProjectDependencyChanges: {qualityGate.NoRuntimeUnityProviderSchemaProjectDependencyChanges.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            $"- manifestHash: {report.ManifestHash}",
            $"- fileLedgerHash: {report.FileLedgerHash}",
            $"- runtimeHandoffSidecarHash: {report.RuntimeHandoffSidecarHash}",
            $"- invalidationMatrixHash: {report.InvalidationMatrixHash}",
            $"- readbackProofHash: {report.ReadbackProofHash}",
            $"- overlapReuseProofHash: {report.OverlapReuseProofHash}",
            $"- negativeProofHash: {report.NegativeProofHash}",
            $"- sourceLineageHash: {report.SourceLineageHash}",
            $"- qualityGateHash: {report.QualityGateHash}"
        ]);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static VisualChunkCacheNegativeScenario InvalidManifest(
        string id,
        string mutation,
        VisualChunkCacheExportManifest manifest,
        IReadOnlySet<string> sourceChunkKeys)
    {
        var validation = VisualChunkCacheExportContractValidator.ValidateManifest(manifest, sourceChunkKeys);
        return Scenario(id, mutation, validation);
    }

    private static VisualChunkCacheNegativeScenario InvalidSidecar(
        string id,
        string mutation,
        VisualChunkCacheRuntimeHandoffSidecar sidecar)
    {
        var validation = VisualChunkCacheExportContractValidator.ValidateRuntimeHandoffSidecar(sidecar);
        return Scenario(id, mutation, validation);
    }

    private static VisualChunkCacheNegativeScenario InvalidSourceLineage(
        string id,
        string mutation,
        VisualChunkCacheSourceLineage sourceLineage)
    {
        var validation = VisualChunkCacheExportContractValidator.ValidateSourceLineage(sourceLineage);
        return Scenario(id, mutation, validation);
    }

    private static VisualChunkCacheNegativeScenario Scenario(
        string id,
        string mutation,
        VisualChunkCacheValidationResult validation) =>
        new()
        {
            ScenarioId = id,
            CausalMutation = mutation,
            ExpectedValid = false,
            ActualValid = validation.Passed,
            Diagnostics = validation.Diagnostics
        };

    private static VisualChunkCacheExportManifest ReplaceRecord(
        VisualChunkCacheExportManifest manifest,
        VisualChunkCacheRecord replacement) =>
        manifest with
        {
            Packages = manifest.Packages.Select(package =>
                package.PackageId != replacement.PackageId
                    ? package
                    : package with
                    {
                        Records = package.Records
                            .Select(record => SameAddress(record, replacement) ? replacement : record)
                            .ToList()
                    }).ToList()
        };

    private static VisualChunkCacheExportManifest ReplacePackage(
        VisualChunkCacheExportManifest manifest,
        VisualChunkCacheExportPackage replacement) =>
        manifest with
        {
            Packages = manifest.Packages.Select(package => package.PackageId == replacement.PackageId ? replacement : package).ToList()
        };

    private static IReadOnlyList<VisualChunkCacheArtifactRef> ReplaceFirstArtifact(
        IReadOnlyList<VisualChunkCacheArtifactRef> artifacts,
        string relativePath) =>
        artifacts.Select((artifact, index) => index == 0
            ? artifact with
            {
                RelativePath = relativePath,
                IsBinaryOrRaster = VisualChunkCacheExportContractValidator.IsBinaryOrRasterPath(relativePath)
            }
            : artifact).ToList();

    private static bool SameAddress(VisualChunkCacheRecord left, VisualChunkCacheRecord right) =>
        left.PackageId == right.PackageId
        && left.CacheKey.ProfileId == right.CacheKey.ProfileId
        && left.CacheKey.LayerId == right.CacheKey.LayerId
        && left.CacheKey.ChunkX == right.CacheKey.ChunkX
        && left.CacheKey.ChunkY == right.CacheKey.ChunkY;

    private static T ReadJson<T>(string projectRoot, string relativePath)
    {
        var fullPath = ResolvePath(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Required Goal093 source artifact was not found: {relativePath}", fullPath);
        }

        var text = File.ReadAllText(fullPath, Encoding.UTF8);
        return JsonSerializer.Deserialize<T>(text, JsonOptions)
            ?? throw new InvalidOperationException($"Unable to deserialize source artifact: {relativePath}");
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string Goal091Path(string relativePath) => $"{Goal091Root}/{relativePath.Replace('\\', '/')}";

    private static string ResolvePath(string projectRoot, string relativePath) =>
        Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        string message,
        List<VisualChunkCacheDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(VisualChunkCacheDiagnostic.Error(code, target, message));
        }
    }
}
