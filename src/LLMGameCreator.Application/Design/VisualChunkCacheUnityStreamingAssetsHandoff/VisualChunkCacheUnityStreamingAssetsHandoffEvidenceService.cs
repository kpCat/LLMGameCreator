using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.VisualChunkCacheUnityStreamingAssetsHandoff;

public sealed class VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService
{
    public const string ReportMarkdownFileName = "visual-chunk-cache-unity-handoff-report.md";
    public const string HandoffManifestFileName = "visual-chunk-cache-unity-handoff-manifest.json";
    public const string PackageIndexFileName = "visual-chunk-cache-package-index.json";
    public const string StreamWindowIndexFileName = "visual-chunk-cache-stream-window-index.json";
    public const string ChunkKeyLedgerFileName = "visual-chunk-cache-chunk-key-ledger.json";
    public const string RuntimeReadmeFileName = "visual-chunk-cache-runtime-readme.json";
    public const string StreamingAssetsLedgerFileName = "visual-chunk-cache-unity-streamingassets-ledger.json";
    public const string ProbeSourceInventoryFileName = "visual-chunk-cache-unity-probe-source-inventory.json";
    public const string SimulatedReadProofFileName = "visual-chunk-cache-unity-simulated-read-proof.json";
    public const string NegativeProofFileName = "visual-chunk-cache-unity-negative-proof.json";
    public const string SourceLineageFileName = "visual-chunk-cache-unity-source-lineage.json";
    public const string QualityGateScanFileName = "visual-chunk-cache-unity-quality-gate-scan.json";

    private const string Goal093Root = ".llmgc/procedural/goal-093-visual-chunk-cache-export-contract";
    private const string Goal094Root = ".llmgc/procedural/goal-094-visual-chunk-cache-export-inspector";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly VisualChunkCacheUnityStreamingAssetsHandoffReadValidator _readValidator = new();
    private readonly VisualChunkCacheUnityStreamingAssetsHandoffQualityGateScanner _qualityScanner = new();

    public VisualChunkCacheUnityBuildResult Build(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payloadFiles = BuildPayloadFiles(context);
        var readProof = _readValidator.ValidatePayloadFiles(root, context, payloadFiles, payloadReadAttempted: true);
        var negativeProof = _readValidator.BuildNegativeProof(root, context, payloadFiles);
        var ledger = BuildFileLedger(payloadFiles);
        var probeInventory = _qualityScanner.BuildProbeSourceInventory(root);
        var qualityGate = _qualityScanner.Scan(
            root,
            context,
            ledger,
            readProof,
            negativeProof,
            probeInventory,
            payloadFiles);

        return BuildResult(context, payloadFiles, ledger, probeInventory, readProof, negativeProof, qualityGate);
    }

    public async Task<VisualChunkCacheUnityWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payloadFiles = BuildPayloadFiles(context);
        var streamingAssetsDirectoryPath = Resolve(
            root,
            VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.StreamingAssetsRelativeRoot);
        ResetDirectory(root, streamingAssetsDirectoryPath);

        var written = new List<string>();
        foreach (var payload in payloadFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var targetPath = Path.Combine(streamingAssetsDirectoryPath, Normalize(payload.Key));
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            await File.WriteAllTextAsync(targetPath, payload.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, targetPath));
        }

        var readProof = _readValidator.ValidateMirroredPayload(root, context);
        var negativeProof = _readValidator.BuildNegativeProof(root, context, ReadPayloadFiles(root));
        var ledger = BuildFileLedger(ReadPayloadFiles(root));
        var probeInventory = _qualityScanner.BuildProbeSourceInventory(root);
        var qualityGate = _qualityScanner.Scan(
            root,
            context,
            ledger,
            readProof,
            negativeProof,
            probeInventory,
            ReadPayloadFiles(root));
        var result = BuildResult(context, ReadPayloadFiles(root), ledger, probeInventory, readProof, negativeProof, qualityGate);

        var outputDirectoryPath = Resolve(
            root,
            VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RelativeOutputDirectory);
        ResetDirectory(root, outputDirectoryPath);
        foreach (var payload in result.PayloadJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var targetPath = Path.Combine(outputDirectoryPath, payload.Key);
            await File.WriteAllTextAsync(targetPath, payload.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, targetPath));
        }

        foreach (var artifact in result.EvidenceJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var targetPath = Path.Combine(outputDirectoryPath, artifact.Key);
            await File.WriteAllTextAsync(targetPath, artifact.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, targetPath));
        }

        var reportPath = Path.Combine(outputDirectoryPath, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new VisualChunkCacheUnityWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectoryPath,
            StreamingAssetsDirectoryPath = streamingAssetsDirectoryPath,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<string> RequiredArtifactNames() =>
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredEvidenceFileNames;

    private VisualChunkCacheUnityBuildResult BuildResult(
        Goal095SourceContext context,
        IReadOnlyDictionary<string, string> payloadFiles,
        VisualChunkCacheUnityFileLedger ledger,
        VisualChunkCacheUnityProbeSourceInventory probeInventory,
        VisualChunkCacheUnitySimulatedReadProof readProof,
        VisualChunkCacheUnityNegativeProof negativeProof,
        VisualChunkCacheUnityQualityGateScan qualityGate)
    {
        var manifest = ReadPayload<VisualChunkCacheUnityHandoffManifest>(
            payloadFiles,
            HandoffManifestFileName) ?? new VisualChunkCacheUnityHandoffManifest();
        var packageIndex = ReadPayload<VisualChunkCacheUnityPackageIndex>(
            payloadFiles,
            PackageIndexFileName) ?? new VisualChunkCacheUnityPackageIndex();
        var streamIndex = ReadPayload<VisualChunkCacheUnityStreamWindowIndex>(
            payloadFiles,
            StreamWindowIndexFileName) ?? new VisualChunkCacheUnityStreamWindowIndex();
        var chunkLedger = ReadPayload<VisualChunkCacheUnityChunkKeyLedger>(
            payloadFiles,
            ChunkKeyLedgerFileName) ?? new VisualChunkCacheUnityChunkKeyLedger();
        var readme = ReadPayload<VisualChunkCacheUnityRuntimeReadme>(
            payloadFiles,
            RuntimeReadmeFileName) ?? new VisualChunkCacheUnityRuntimeReadme();

        var evidence = BuildEvidencePayloads(context.SourceLineage, ledger, probeInventory, readProof, negativeProof, qualityGate);
        var reportWithoutHash = BuildReport(
            context,
            manifest,
            payloadFiles,
            evidence,
            qualityGate);
        var reportMarkdownWithoutHash = VisualChunkCacheUnityStreamingAssetsHandoffReportRenderer.Render(reportWithoutHash);
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(reportMarkdownWithoutHash)
        };
        var reportMarkdown = VisualChunkCacheUnityStreamingAssetsHandoffReportRenderer.Render(report);

        return new VisualChunkCacheUnityBuildResult
        {
            SourceLineage = context.SourceLineage,
            HandoffManifest = manifest,
            PackageIndex = packageIndex,
            StreamWindowIndex = streamIndex,
            ChunkKeyLedger = chunkLedger,
            RuntimeReadme = readme,
            StreamingAssetsLedger = ledger,
            ProbeSourceInventory = probeInventory,
            SimulatedReadProof = readProof,
            NegativeProof = negativeProof,
            QualityGateScan = qualityGate,
            Report = report,
            ReportMarkdown = reportMarkdown,
            PayloadJsonByFileName = new SortedDictionary<string, string>(
                payloadFiles.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal),
                StringComparer.Ordinal),
            EvidenceJsonByFileName = evidence
        };
    }

    private Goal095SourceContext ReadSourceContext(string root)
    {
        var diagnostics = new List<VisualChunkCacheUnityHandoffDiagnostic>();
        var sourceLineage = BuildSourceLineage(root, diagnostics);
        var manifest = ReadJson<Goal093ManifestSource>(
            root,
            Goal093Root + "/visual-chunk-cache-export-manifest.json",
            diagnostics) ?? new Goal093ManifestSource();
        var sidecar = ReadJson<Goal093RuntimeHandoffSidecarSource>(
            root,
            Goal093Root + "/visual-chunk-cache-runtime-handoff-sidecar.json",
            diagnostics) ?? new Goal093RuntimeHandoffSidecarSource();
        var quality = ReadJson<Goal094QualityGateSource>(
            root,
            Goal094Root + "/visual-chunk-cache-export-inspector-quality-gate-scan.json",
            diagnostics) ?? new Goal094QualityGateSource();
        var goal094Report = ReadOptional(root, Goal094Root + "/visual-chunk-cache-export-inspector-report.md");

        var packageCount = manifest.PackageCount;
        var exportRecordCount = manifest.ExportRecordCount;
        var sourceChunkCount = manifest.SourceMaterializedChunkCount;
        var streamWindowCount = manifest.Packages.Sum(item => item.StreamWindowCount);
        var uniqueChunkCount = manifest.Packages
            .SelectMany(item => item.Records)
            .Select(item => item.CacheKey.ChunkKey)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var goal093AcceptedFalse = !manifest.Accepted;
        var goal094AcceptedFalse = !quality.Accepted
                                   && goal094Report.Contains("- accepted: false", StringComparison.OrdinalIgnoreCase);
        var metadataOnlySidecar = sidecar.MetadataOnly
                                  && !sidecar.ContainsRuntimeExecution
                                  && !sidecar.ContainsProviderCalls
                                  && !sidecar.ContainsUnityImplementation
                                  && !sidecar.ContainsPromptText;

        AddIfFalse(diagnostics, goal093AcceptedFalse, "goal095.source.goal093_accepted_mutated", "Goal093", "Goal093 artifacts must remain accepted=false.");
        AddIfFalse(diagnostics, goal094AcceptedFalse, "goal095.source.goal094_accepted_mutated", "Goal094", "Goal094 artifacts must remain accepted=false.");
        AddIfFalse(diagnostics, packageCount == 4, "goal095.source.package_count", "Goal093 manifest", "Goal093 must provide four cache export packages.");
        AddIfFalse(diagnostics, exportRecordCount == 93, "goal095.source.record_count", "Goal093 manifest", "Goal093 must provide 93 cache export records.");
        AddIfFalse(diagnostics, sourceChunkCount == 117, "goal095.source.source_chunk_count", "Goal093 manifest", "Goal093 must preserve the 117 source chunk summary.");
        AddIfFalse(diagnostics, streamWindowCount == 5, "goal095.source.stream_window_count", "Goal093 manifest", "Goal093 packages must cover five stream windows.");
        AddIfFalse(diagnostics, uniqueChunkCount == 93, "goal095.source.chunk_key_count", "Goal093 manifest", "Goal093 must expose 93 unique chunk keys.");
        AddIfFalse(diagnostics, metadataOnlySidecar, "goal095.source.sidecar_metadata_only", "Goal093 sidecar", "Goal093 runtime handoff sidecar must remain metadata-only.");
        AddIfFalse(diagnostics, quality.Passed, "goal095.source.goal094_quality", "Goal094 quality", "Goal094 quality gate must pass.");
        AddIfFalse(diagnostics, quality.CacheExportPackageCount == packageCount, "goal095.source.goal094_package_count", "Goal094 quality", "Goal094 package count must match Goal093.");
        AddIfFalse(diagnostics, quality.CacheExportRecordCount == exportRecordCount, "goal095.source.goal094_record_count", "Goal094 quality", "Goal094 record count must match Goal093.");

        return new Goal095SourceContext
        {
            RootPath = root,
            Goal093Manifest = manifest,
            Goal093Sidecar = sidecar,
            Goal094QualityGate = quality,
            SourceLineage = sourceLineage with
            {
                Diagnostics = SortDiagnostics(sourceLineage.Diagnostics.Concat(diagnostics)),
                Passed = sourceLineage.Passed && diagnostics.All(item => item.Severity != "error")
            },
            PackageCount = packageCount,
            ExportRecordCount = exportRecordCount,
            SourceMaterializedChunkCount = sourceChunkCount,
            StreamWindowCount = streamWindowCount,
            UniqueChunkKeyCount = uniqueChunkCount,
            Goal093AcceptedFalse = goal093AcceptedFalse,
            Goal094AcceptedFalse = goal094AcceptedFalse,
            RuntimeHandoffSidecarMetadataOnly = metadataOnlySidecar,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private SortedDictionary<string, string> BuildPayloadFiles(Goal095SourceContext context)
    {
        var packageIndex = BuildPackageIndex(context);
        var streamIndex = BuildStreamWindowIndex(context);
        var chunkLedger = BuildChunkKeyLedger(context);
        var readme = new VisualChunkCacheUnityRuntimeReadme();
        var packageIndexJson = Serialize(packageIndex);
        var streamIndexJson = Serialize(streamIndex);
        var chunkLedgerJson = Serialize(chunkLedger);
        var readmeJson = Serialize(readme);
        var manifest = new VisualChunkCacheUnityHandoffManifest
        {
            PayloadFileCount = VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredPayloadFileNames.Count,
            PackageCount = context.PackageCount,
            ExportRecordCount = context.ExportRecordCount,
            SourceMaterializedChunkCount = context.SourceMaterializedChunkCount,
            StreamWindowCount = context.StreamWindowCount,
            UniqueChunkKeyCount = context.UniqueChunkKeyCount,
            Goal093AcceptedFalse = context.Goal093AcceptedFalse,
            Goal094AcceptedFalse = context.Goal094AcceptedFalse,
            RuntimeHandoffSidecarMetadataOnly = context.RuntimeHandoffSidecarMetadataOnly,
            NoRawFullWorldDump = context.Goal093Manifest.NoRawFullWorldDump && chunkLedger.NoRawFullWorldDump,
            NoAbsolutePaths = context.Goal093Manifest.NoAbsolutePaths,
            NoBinaryOrRasterMedia = context.Goal093Manifest.NoBinaryOrRasterMedia,
            NoPromptDumps = context.Goal093Manifest.NoPromptDumps,
            ContainsRuntimeExecution = context.Goal093Sidecar.ContainsRuntimeExecution,
            ContainsProviderCalls = context.Goal093Sidecar.ContainsProviderCalls,
            ContainsUnityGameplayImplementation = false,
            PackageIndexHash = Hash(packageIndexJson),
            StreamWindowIndexHash = Hash(streamIndexJson),
            ChunkKeyLedgerHash = Hash(chunkLedgerJson),
            RuntimeReadmeHash = Hash(readmeJson)
        };

        return new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [HandoffManifestFileName] = Serialize(manifest),
            [PackageIndexFileName] = packageIndexJson,
            [StreamWindowIndexFileName] = streamIndexJson,
            [ChunkKeyLedgerFileName] = chunkLedgerJson,
            [RuntimeReadmeFileName] = readmeJson
        };
    }

    private static VisualChunkCacheUnityPackageIndex BuildPackageIndex(Goal095SourceContext context)
    {
        var packages = context.Goal093Manifest.Packages
            .OrderBy(item => item.PackageId, StringComparer.Ordinal)
            .Select(package => new VisualChunkCacheUnityPackageSummary
            {
                PackageId = package.PackageId,
                ExportTargetKind = package.ExportTargetKind,
                SourceFixtureId = package.SourceFixtureId,
                ProfileId = package.ProfileId,
                WorldSeed = package.WorldSeed,
                GeneratorVersion = package.GeneratorVersion,
                StreamWindowCount = package.StreamWindowCount,
                ExportedRecordCount = package.ExportedRecordCount,
                SourceMaterializedChunkCount = package.SourceMaterializedChunkCount,
                EstimatedFullWorldChunkCapacity = package.EstimatedFullWorldChunkCapacity,
                MetadataOnly = package.MetadataOnly,
                NoRawFullWorldDump = package.NoRawFullWorldDump,
                OnlyMaterializedChunksExported = package.OnlyMaterializedChunksExported,
                PackageMembershipHash = Hash(string.Join(
                    "|",
                    package.Records
                        .Select(record => record.CacheKey.ChunkKey)
                        .OrderBy(value => value, StringComparer.Ordinal)))
            })
            .ToList();

        return new VisualChunkCacheUnityPackageIndex
        {
            PackageCount = packages.Count,
            ExportRecordCount = packages.Sum(item => item.ExportedRecordCount),
            Packages = packages
        };
    }

    private static VisualChunkCacheUnityStreamWindowIndex BuildStreamWindowIndex(Goal095SourceContext context)
    {
        var windows = context.Goal093Manifest.Packages
            .SelectMany(package => package.StreamWindows.Select(window => new VisualChunkCacheUnityStreamWindowSummary
            {
                WindowId = window.WindowId,
                FixtureId = window.FixtureId,
                PackageId = package.PackageId,
                ProfileId = window.ProfileId,
                LayerIds = window.LayerIds.OrderBy(value => value, StringComparer.Ordinal).ToList(),
                SourceChunkCount = window.SourceChunkCount,
                ExportedRecordCount = window.ExportedRecordCount,
                MembershipStableHash = window.MembershipStableHash
            }))
            .OrderBy(item => item.PackageId, StringComparer.Ordinal)
            .ThenBy(item => item.WindowId, StringComparer.Ordinal)
            .ToList();
        return new VisualChunkCacheUnityStreamWindowIndex
        {
            StreamWindowCount = windows.Count,
            StreamWindows = windows
        };
    }

    private static VisualChunkCacheUnityChunkKeyLedger BuildChunkKeyLedger(Goal095SourceContext context)
    {
        var rows = context.Goal093Manifest.Packages
            .SelectMany(package => package.Records.Select(record => (Package: package, Record: record)))
            .GroupBy(item => item.Record.CacheKey.ChunkKey, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group =>
            {
                var first = group.First().Record;
                return new VisualChunkCacheUnityChunkKeyEntry
                {
                    ChunkKey = first.CacheKey.ChunkKey,
                    ChunkHash = first.ChunkHash,
                    ProfileId = first.CacheKey.ProfileId,
                    LayerId = first.CacheKey.LayerId,
                    ChunkX = first.CacheKey.ChunkX,
                    ChunkY = first.CacheKey.ChunkY,
                    PackageIds = group
                        .Select(item => item.Package.PackageId)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToList(),
                    StreamWindowIds = group
                        .SelectMany(item => item.Record.StreamWindowIds)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToList()
                };
            })
            .ToList();

        return new VisualChunkCacheUnityChunkKeyLedger
        {
            UniqueChunkKeyCount = rows.Count,
            ExportRecordCount = context.Goal093Manifest.Packages.Sum(item => item.Records.Count),
            CompactMetadataOnly = true,
            NoRawFullWorldDump = context.Goal093Manifest.Packages
                .SelectMany(item => item.Records)
                .All(item => item.NoRawFullWorldDump && !item.ContainsRawFullWorldCellDump),
            Entries = rows
        };
    }

    private static VisualChunkCacheUnityFileLedger BuildFileLedger(
        IReadOnlyDictionary<string, string> payloadFiles)
    {
        var diagnostics = new List<VisualChunkCacheUnityHandoffDiagnostic>();
        foreach (var required in VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredPayloadFileNames)
        {
            if (!payloadFiles.ContainsKey(required))
            {
                diagnostics.Add(Error("goal095.payload.required_file_missing", required, "Required Unity StreamingAssets payload file is missing."));
            }
        }

        var files = payloadFiles
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new VisualChunkCacheUnityFileEntry
            {
                RelativePath = item.Key,
                Role = Role(item.Key),
                Sha256 = Hash(item.Value),
                ByteCount = Encoding.UTF8.GetByteCount(item.Value)
            })
            .ToList();

        return new VisualChunkCacheUnityFileLedger
        {
            Passed = diagnostics.Count == 0
                     && files.Count == VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredPayloadFileNames.Count,
            FileCount = files.Count,
            Files = files,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static VisualChunkCacheUnitySourceLineage BuildSourceLineage(
        string root,
        ICollection<VisualChunkCacheUnityHandoffDiagnostic> diagnostics)
    {
        var records = SourceInputs()
            .Select(item => SourceRecord(root, item.Path, item.Goal))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
        foreach (var missing in records.Where(item => !item.Exists))
        {
            diagnostics.Add(Error("goal095.source.artifact_missing", missing.RelativePath, "Required source artifact is missing."));
        }

        var goal093 = records.Any(item => item.SourceGoal == "Goal093" && item.Exists);
        var goal094 = records.Any(item => item.SourceGoal == "Goal094" && item.Exists);
        return new VisualChunkCacheUnitySourceLineage
        {
            Passed = records.All(item => item.Exists && item.Sha256.Length == 64) && goal093 && goal094,
            Goal093LineagePresent = goal093,
            Goal094LineagePresent = goal094,
            Goal093AcceptedFalse = true,
            Goal094AcceptedFalse = true,
            SourceRecordCount = records.Count,
            Records = records,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static IReadOnlyList<(string Path, string Goal)> SourceInputs() =>
    [
        (Goal093Root + "/visual-chunk-cache-export-report.md", "Goal093"),
        (Goal093Root + "/visual-chunk-cache-export-manifest.json", "Goal093"),
        (Goal093Root + "/visual-chunk-cache-runtime-handoff-sidecar.json", "Goal093"),
        (Goal093Root + "/visual-chunk-cache-readback-proof.json", "Goal093"),
        (Goal093Root + "/visual-chunk-cache-overlap-reuse-proof.json", "Goal093"),
        (Goal093Root + "/visual-chunk-cache-negative-proof.json", "Goal093"),
        (Goal094Root + "/visual-chunk-cache-export-inspector-report.md", "Goal094"),
        (Goal094Root + "/visual-chunk-cache-export-inspector-quality-gate-scan.json", "Goal094")
    ];

    private static VisualChunkCacheUnitySourceArtifactReference SourceRecord(
        string root,
        string relativePath,
        string sourceGoal)
    {
        var fullPath = Resolve(root, relativePath);
        var exists = File.Exists(fullPath);
        var info = exists ? new FileInfo(fullPath) : null;
        return new VisualChunkCacheUnitySourceArtifactReference
        {
            SourceGoal = sourceGoal,
            RelativePath = relativePath,
            Sha256 = exists ? VisualChunkCacheUnityStreamingAssetsHandoffHash.Sha256File(fullPath) : string.Empty,
            ByteCount = info?.Length ?? 0,
            Exists = exists
        };
    }

    private static VisualChunkCacheUnityHandoffReport BuildReport(
        Goal095SourceContext context,
        VisualChunkCacheUnityHandoffManifest manifest,
        IReadOnlyDictionary<string, string> payloads,
        IReadOnlyDictionary<string, string> evidence,
        VisualChunkCacheUnityQualityGateScan qualityGate)
    {
        var diagnostics = SortDiagnostics(
            context.Diagnostics
                .Concat(context.SourceLineage.Diagnostics)
                .Concat(qualityGate.Diagnostics));
        return new VisualChunkCacheUnityHandoffReport
        {
            ImplementationStatus = qualityGate.Passed && diagnostics.All(item => item.Severity != "error")
                ? "GREEN"
                : "BLOCKED",
            Accepted = false,
            PackageCount = manifest.PackageCount,
            ExportRecordCount = manifest.ExportRecordCount,
            StreamWindowCount = manifest.StreamWindowCount,
            UniqueChunkKeyCount = manifest.UniqueChunkKeyCount,
            HandoffManifestHash = Hash(payloads[HandoffManifestFileName]),
            PackageIndexHash = Hash(payloads[PackageIndexFileName]),
            StreamWindowIndexHash = Hash(payloads[StreamWindowIndexFileName]),
            ChunkKeyLedgerHash = Hash(payloads[ChunkKeyLedgerFileName]),
            RuntimeReadmeHash = Hash(payloads[RuntimeReadmeFileName]),
            StreamingAssetsLedgerHash = Hash(evidence[StreamingAssetsLedgerFileName]),
            ProbeSourceInventoryHash = Hash(evidence[ProbeSourceInventoryFileName]),
            SimulatedReadProofHash = Hash(evidence[SimulatedReadProofFileName]),
            NegativeProofHash = Hash(evidence[NegativeProofFileName]),
            SourceLineageHash = Hash(evidence[SourceLineageFileName]),
            QualityGateScanHash = Hash(evidence[QualityGateScanFileName]),
            AlphaRuntimeBootstrapAfterHash = qualityGate.AlphaRuntimeBootstrapAfterHash,
            AlphaRuntimeBootstrapAfterLineCount = qualityGate.AlphaRuntimeBootstrapAfterLineCount,
            Diagnostics = diagnostics
        };
    }

    private static SortedDictionary<string, string> BuildEvidencePayloads(
        VisualChunkCacheUnitySourceLineage sourceLineage,
        VisualChunkCacheUnityFileLedger ledger,
        VisualChunkCacheUnityProbeSourceInventory probeInventory,
        VisualChunkCacheUnitySimulatedReadProof readProof,
        VisualChunkCacheUnityNegativeProof negativeProof,
        VisualChunkCacheUnityQualityGateScan qualityGate)
    {
        return new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [StreamingAssetsLedgerFileName] = Serialize(ledger),
            [ProbeSourceInventoryFileName] = Serialize(probeInventory),
            [SimulatedReadProofFileName] = Serialize(readProof),
            [NegativeProofFileName] = Serialize(negativeProof),
            [SourceLineageFileName] = Serialize(sourceLineage),
            [QualityGateScanFileName] = Serialize(qualityGate)
        };
    }

    private static SortedDictionary<string, string> ReadPayloadFiles(string root)
    {
        var payloadRoot = Resolve(root, VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.StreamingAssetsRelativeRoot);
        var payloads = new SortedDictionary<string, string>(StringComparer.Ordinal);
        if (!Directory.Exists(payloadRoot))
        {
            return payloads;
        }

        foreach (var fileName in VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredPayloadFileNames)
        {
            var path = Path.Combine(payloadRoot, fileName);
            if (File.Exists(path))
            {
                payloads[fileName] = File.ReadAllText(path, Encoding.UTF8);
            }
        }

        return payloads;
    }

    private static T? ReadJson<T>(
        string root,
        string relativePath,
        ICollection<VisualChunkCacheUnityHandoffDiagnostic> diagnostics)
    {
        var path = Resolve(root, relativePath);
        if (!File.Exists(path))
        {
            diagnostics.Add(Error("goal095.source.json_missing", relativePath, "Required JSON source artifact is missing."));
            return default;
        }

        try
        {
            return VisualChunkCacheUnityStreamingAssetsHandoffJson.Deserialize<T>(
                File.ReadAllText(path, Encoding.UTF8));
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Error("goal095.source.json_invalid", relativePath, exception.Message));
            return default;
        }
    }

    private static T? ReadPayload<T>(
        IReadOnlyDictionary<string, string> payloads,
        string fileName) =>
        payloads.TryGetValue(fileName, out var json)
            ? VisualChunkCacheUnityStreamingAssetsHandoffJson.Deserialize<T>(json)
            : default;

    private static string ReadOptional(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static void ResetDirectory(string root, string path)
    {
        EnsureContained(root, path);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static string Resolve(string root, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(root, Normalize(relativePath)));
        EnsureContained(root, path);
        return path;
    }

    private static void EnsureContained(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes repository root: " + normalizedPath);
        }
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');

    private static string Normalize(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar);

    private static string Role(string fileName) =>
        fileName switch
        {
            HandoffManifestFileName => "manifest",
            PackageIndexFileName => "package_index",
            StreamWindowIndexFileName => "stream_window_index",
            ChunkKeyLedgerFileName => "chunk_key_ledger",
            RuntimeReadmeFileName => "runtime_readme",
            _ => "payload"
        };

    private static string Serialize<T>(T value) =>
        VisualChunkCacheUnityStreamingAssetsHandoffJson.Serialize(value);

    private static string Hash(string text) =>
        VisualChunkCacheUnityStreamingAssetsHandoffHash.Sha256Text(text);

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

    private static VisualChunkCacheUnityHandoffDiagnostic Error(
        string code,
        string target,
        string message) =>
        VisualChunkCacheUnityHandoffDiagnostic.Error(code, target, message);
}
