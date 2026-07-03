using System.Text.Json;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal093SourceGoalId = "goal_093_visual_chunk_cache_export_contract";
    private const string Goal093SourceRoot =
        ".llmgc/procedural/goal-093-visual-chunk-cache-export-contract";
    private const string Goal093ManifestPath =
        Goal093SourceRoot + "/visual-chunk-cache-export-manifest.json";
    private const string Goal093SidecarPath =
        Goal093SourceRoot + "/visual-chunk-cache-runtime-handoff-sidecar.json";

    private static VisualWorldPreviewArtifactGroup BuildCacheExportGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var ledger = LoadLedger(projectRoot, Goal093SourceRoot, "visual-chunk-cache-file-ledger.json");
        var proofSummary = LoadCacheExportProofSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
            projectRoot,
            Goal093SourceRoot,
            Goal093SourceGoalId,
            [
                ("visual-chunk-cache-export-report.md", "cache_export_report"),
                ("visual-chunk-cache-export-manifest.json", "cache_export_manifest"),
                ("visual-chunk-cache-runtime-handoff-sidecar.json", "runtime_handoff_sidecar"),
                ("visual-chunk-cache-invalidation-matrix.json", "invalidation_matrix"),
                ("visual-chunk-cache-readback-proof.json", "readback_proof"),
                ("visual-chunk-cache-overlap-reuse-proof.json", "overlap_reuse_proof"),
                ("visual-chunk-cache-negative-proof.json", "negative_proof"),
                ("visual-chunk-cache-quality-gate-scan.json", "quality_gate"),
                ("visual-chunk-cache-source-lineage.json", "source_lineage")
            ],
            ledger,
            groupDiagnostics);

        using var manifest = TryReadJson(projectRoot, Goal093ManifestPath, groupDiagnostics);
        if (manifest is not null
            && TryGetArray(manifest.RootElement, "packages", out var packages))
        {
            foreach (var package in packages.OrderBy(
                item => TryGetString(item, "packageId"),
                StringComparer.Ordinal))
            {
                entries.Add(BuildCacheExportPackageEntry(
                    projectRoot,
                    package,
                    proofSummary,
                    ledger,
                    groupDiagnostics));
            }
        }
        else
        {
            groupDiagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal094.cache_export.manifest_missing_packages",
                Goal093ManifestPath,
                "Goal 093 cache export manifest must expose package entries."));
        }

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "cache_exports",
            "Goal 093 Cache Exports",
            Goal093SourceGoalId,
            Goal093SourceRoot,
            entries,
            groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactEntry BuildCacheExportPackageEntry(
        string projectRoot,
        JsonElement package,
        CacheExportProofSummary proofSummary,
        IReadOnlyDictionary<string, string> ledger,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var packageId = TryGetString(package, "packageId");
        var targetKind = TryGetString(package, "exportTargetKind");
        TryGetInt(package, "exportedRecordCount", out var recordCount);
        TryGetInt(package, "sourceMaterializedChunkCount", out var sourceChunkCount);
        TryGetInt(package, "streamWindowCount", out var streamWindowCount);
        var noRawDump = TryGetBool(package, "noRawFullWorldDump");
        var metadataOnly = TryGetBool(package, "metadataOnly");
        var sidecarMetadataOnly = proofSummary.RuntimeHandoffSidecarMetadataOnly;
        var runtimeHandoffMetadataOnly =
            string.Equals(targetKind, "runtimeHandoff", StringComparison.Ordinal)
                ? sidecarMetadataOnly && metadataOnly
                : metadataOnly;
        var chunkKeys = ReadChunkKeys(package);
        var relativePath = Goal093ManifestPath;
        var status = !string.IsNullOrWhiteSpace(packageId)
                     && recordCount > 0
                     && sourceChunkCount > 0
                     && streamWindowCount > 0
                     && noRawDump
                     && proofSummary.AllRequiredProofsPassed
            ? VisualWorldPreviewArtifactStatus.Passed
            : VisualWorldPreviewArtifactStatus.Failed;

        if (status == VisualWorldPreviewArtifactStatus.Failed)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal094.cache_export.package_failed",
                packageId,
                "Goal 093 cache export package is missing required counts, no-raw-dump flag, or proof status."));
        }

        return new VisualWorldPreviewArtifactEntry
        {
            Id = Goal093SourceGoalId + "." + packageId,
            RelativePath = relativePath,
            ArtifactKind = "cache_export_package",
            SourceGoalId = Goal093SourceGoalId,
            Sha256 = HashFor(projectRoot, relativePath, ledger),
            Status = status,
            DiagnosticSummary = "target=" + targetKind
                + "; records=" + recordCount
                + "; sourceChunks=" + sourceChunkCount
                + "; streamWindows=" + streamWindowCount,
            SafeRatingMetadataSummary = "noRawFullWorldDump="
                + noRawDump.ToString().ToLowerInvariant()
                + "; metadataOnly=" + metadataOnly.ToString().ToLowerInvariant(),
            ExportTargetKind = targetKind,
            CacheRecordCount = recordCount,
            SourceChunkCount = sourceChunkCount,
            StreamWindowCount = streamWindowCount,
            RuntimeHandoffMetadataOnly = runtimeHandoffMetadataOnly,
            InvalidationMatrixPassed = proofSummary.InvalidationMatrixPassed,
            ReadbackProofPassed = proofSummary.ReadbackProofPassed,
            OverlapReuseProofPassed = proofSummary.OverlapReuseProofPassed,
            NegativeProofPassed = proofSummary.NegativeProofPassed,
            NoRawFullWorldDump = noRawDump,
            ChunkKeys = chunkKeys
        };
    }

    private static CacheExportProofSummary LoadCacheExportProofSummary(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var readback = TryReadJson(
            projectRoot,
            Goal093SourceRoot + "/visual-chunk-cache-readback-proof.json",
            diagnostics);
        using var overlap = TryReadJson(
            projectRoot,
            Goal093SourceRoot + "/visual-chunk-cache-overlap-reuse-proof.json",
            diagnostics);
        using var negative = TryReadJson(
            projectRoot,
            Goal093SourceRoot + "/visual-chunk-cache-negative-proof.json",
            diagnostics);
        using var invalidation = TryReadJson(
            projectRoot,
            Goal093SourceRoot + "/visual-chunk-cache-invalidation-matrix.json",
            diagnostics);
        using var sidecar = TryReadJson(projectRoot, Goal093SidecarPath, diagnostics);

        return new CacheExportProofSummary(
            ReadbackProofPassed: readback is not null && TryGetBool(readback.RootElement, "passed"),
            OverlapReuseProofPassed: overlap is not null && TryGetBool(overlap.RootElement, "passed"),
            NegativeProofPassed: negative is not null && TryGetBool(negative.RootElement, "passed"),
            InvalidationMatrixPassed: invalidation is not null && TryGetBool(invalidation.RootElement, "passed"),
            RuntimeHandoffSidecarMetadataOnly: sidecar is not null
                && TryGetBool(sidecar.RootElement, "metadataOnly")
                && !TryGetBool(sidecar.RootElement, "containsRuntimeExecution")
                && !TryGetBool(sidecar.RootElement, "containsProviderCalls")
                && !TryGetBool(sidecar.RootElement, "containsUnityImplementation"));
    }

    private static IReadOnlyList<string> ReadChunkKeys(JsonElement package)
    {
        if (!TryGetArray(package, "records", out var records))
        {
            return [];
        }

        return records
            .Select(record =>
            {
                if (!record.TryGetProperty("cacheKey", out var cacheKey))
                {
                    return string.Empty;
                }

                return TryGetString(cacheKey, "chunkKey");
            })
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .Take(12)
            .ToList();
    }

    private sealed record CacheExportProofSummary(
        bool ReadbackProofPassed,
        bool OverlapReuseProofPassed,
        bool NegativeProofPassed,
        bool InvalidationMatrixPassed,
        bool RuntimeHandoffSidecarMetadataOnly)
    {
        public bool AllRequiredProofsPassed =>
            ReadbackProofPassed
            && OverlapReuseProofPassed
            && NegativeProofPassed
            && InvalidationMatrixPassed
            && RuntimeHandoffSidecarMetadataOnly;
    }
}
