namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    public async Task<VisualWorldStreamPreviewWorkspaceWriteResult> WriteAsync(
        string projectRootPath,
        VisualWorldStreamPreviewWorkspaceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(
            projectRoot,
            VisualWorldStreamPreviewWorkspaceVocabulary.RelativeOutputDirectory
                .Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new VisualWorldStreamPreviewWorkspaceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            CatalogJsonPath = Path.Combine(outputDirectory, CatalogJsonFileName),
            ProofStatusJsonPath = Path.Combine(outputDirectory, ProofStatusJsonFileName),
            WinFormsBindingInventoryJsonPath =
                Path.Combine(outputDirectory, WinFormsBindingInventoryJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName),
            SourceHealthScanJsonPath = Path.Combine(outputDirectory, SourceHealthScanJsonFileName),
            Result = result
        };

        await File.WriteAllTextAsync(
            write.ReportMarkdownPath,
            result.ReportMarkdown,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.CatalogJsonPath,
            result.CatalogJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.ProofStatusJsonPath,
            result.ProofStatusJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.WinFormsBindingInventoryJsonPath,
            result.WinFormsBindingInventoryJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.QualityGateScanJsonPath,
            result.QualityGateScanJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.SourceHealthScanJsonPath,
            result.SourceHealthScanJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);

        return write;
    }

    private static VisualWorldPreviewWorkspaceQualityGate BuildQualityGate(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewSvgEntry> svgEntries,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs,
        VisualWorldPreviewWinFormsBindingInventory binding,
        VisualWorldStreamPreviewSourceHealthScan sourceHealth,
        IReadOnlyList<VisualWorldPreviewDiagnostic> sourceDiagnostics)
    {
        var diagnostics = new List<VisualWorldPreviewDiagnostic>();
        var groupCount = groups.Count;
        var entryCount = groups.Sum(group => group.EntryCount);
        var goal091StreamEntries = groups
            .Where(group => group.GroupId == "chunk_stream_windows")
            .SelectMany(group => group.Entries)
            .Count(entry => entry.ArtifactKind == "text_svg_chunk_stream_window_overview");
        var cacheExportGroup = groups.FirstOrDefault(group => group.GroupId == "cache_exports");
        var cachePackageEntries = cacheExportGroup?.Entries
            .Where(entry => entry.ArtifactKind == "cache_export_package")
            .ToList() ?? [];
        var cachePackageCount = cachePackageEntries.Count;
        var cacheRecordCount = cachePackageEntries.Sum(entry => entry.CacheRecordCount);
        var cacheSourceChunkCount = cachePackageEntries.Sum(entry => entry.SourceChunkCount);
        var cacheStreamWindowCount = cachePackageEntries.Sum(entry => entry.StreamWindowCount);
        var runtimeHandoffEntry = cachePackageEntries.FirstOrDefault(entry =>
            string.Equals(entry.ExportTargetKind, "runtimeHandoff", StringComparison.Ordinal));
        var runtimeHandoffVisible = runtimeHandoffEntry is not null;
        var runtimeHandoffMetadataOnly = runtimeHandoffEntry?.RuntimeHandoffMetadataOnly == true;
        var cacheReadbackProofPassed = proofs.Any(proof =>
            proof.ProofId == "goal093.readback" && proof.Passed);
        var cacheOverlapProofPassed = proofs.Any(proof =>
            proof.ProofId == "goal093.overlap_reuse" && proof.Passed);
        var cacheNegativeProofPassed = proofs.Any(proof =>
            proof.ProofId == "goal093.negative" && proof.Passed);
        var cacheInvalidationPassed = proofs.Any(proof =>
            proof.ProofId == "goal093.invalidation_matrix" && proof.Passed);
        var cacheNoRawDump = cachePackageEntries.Count >= 4
            && cachePackageEntries.All(entry => entry.NoRawFullWorldDump);
        var goal093RelativePaths = cacheExportGroup is not null
            && cacheExportGroup.Entries.Count > 0
            && cacheExportGroup.Entries.All(entry =>
                IsSafeRelativePath(entry.RelativePath)
                && entry.RelativePath.StartsWith(
                    ".llmgc/procedural/goal-093-visual-chunk-cache-export-contract/",
                    StringComparison.Ordinal));
        var unityGroup = groups.FirstOrDefault(group => group.GroupId == "unity_handoff");
        var unityEntries = unityGroup?.Entries.ToList() ?? [];
        var unityPayloadEntries = unityEntries
            .Where(entry => entry.RelativePath.StartsWith(
                Goal095StreamingAssetsRoot + "/",
                StringComparison.Ordinal))
            .ToList();
        var unityPayloadFileCount = unityPayloadEntries.Count;
        var unityManifestEntry = unityEntries.FirstOrDefault(entry =>
            entry.ArtifactKind == "unity_handoff_manifest"
            || entry.ArtifactKind == "streamingassets_payload_manifest");
        var unityPackageCount = unityManifestEntry?.PackageCount ?? 0;
        var unityRecordCount = unityManifestEntry?.ExportRecordCount ?? 0;
        var unityStreamWindowCount = unityManifestEntry?.StreamWindowCount ?? 0;
        var unityUniqueChunkKeyCount = unityManifestEntry?.UniqueChunkKeyCount ?? 0;
        var unityProbeVisible = unityEntries.Any(entry =>
            entry.ArtifactKind == "unity_probe_source_inventory");
        var unityProbePassed = unityEntries.Any(entry =>
            entry.ArtifactKind == "unity_probe_source_inventory"
            && entry.ProbeSourceInventoryPassed
            && entry.Status == VisualWorldPreviewArtifactStatus.Passed);
        var unitySimulatedReadProofPassed = proofs.Any(proof =>
            proof.ProofId == "goal095.simulated_read" && proof.Passed);
        var unityNegativeProofPassed = proofs.Any(proof =>
            proof.ProofId == "goal095.negative" && proof.Passed);
        var unityAlphaBootstrapUnchanged = proofs.Any(proof =>
            proof.ProofId == "goal095.alpha_runtime_bootstrap_unchanged" && proof.Passed);
        var unityForbiddenAreasUnchanged = proofs.Any(proof =>
            proof.ProofId == "goal095.forbidden_unity_areas_unchanged" && proof.Passed);
        var unityMetadataOnly = proofs.Any(proof =>
            proof.ProofId == "goal095.metadata_only" && proof.Passed);
        var unityPayloadHashesMatch = unityPayloadEntries.Count == 5
            && unityPayloadEntries.All(entry => entry.PayloadHashesMatchGoal095Ledger)
            && unityPayloadEntries.All(entry => entry.Status == VisualWorldPreviewArtifactStatus.Passed);
        var goal095RelativePaths = unityEntries.Count > 0
            && unityEntries.All(entry =>
                IsSafeRelativePath(entry.RelativePath)
                && (entry.RelativePath.StartsWith(Goal095SourceRoot + "/", StringComparison.Ordinal)
                    || entry.RelativePath.StartsWith(Goal095StreamingAssetsRoot, StringComparison.Ordinal)
                    || entry.RelativePath == Goal095AlphaRuntimeBootstrapPath));
        var noUnityFilesChanged = unityEntries.Count > 0
            && unityEntries.All(entry => entry.NoUnityFilesChangedByGoal096);
        var geoworldGroup = groups.FirstOrDefault(group => group.GroupId == "geoworld");
        var geoworldEntries = geoworldGroup?.Entries.ToList() ?? [];
        var geoworldSummary = geoworldEntries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_workspace_summary");
        var geoworldOfflineBundleId = geoworldSummary?.OfflineBundleId ?? string.Empty;
        var geoworldFeatureCount = geoworldSummary?.GeoworldNormalizedFeatureCount ?? 0;
        var geoworldGraphChunkCount = geoworldSummary?.GeoworldWorldSourceGraphChunkCount ?? 0;
        var geoworldStreamChunkCount = geoworldSummary?.GeoworldStreamWindowChunkCount ?? 0;
        var geoworldBoundaryPrefetchPassed = proofs.Any(proof =>
            proof.ProofId == "goal099.boundary_prefetch" && proof.Passed);
        var geoworldNegativeProofPassed = proofs.Any(proof =>
            proof.ProofId == "goal099.negative" && proof.Passed);
        var geoworldQualityGatePassed = proofs.Any(proof =>
            proof.ProofId == "goal099.quality_gate" && proof.Passed);
        var geoworldTaxonomyCoveragePassed =
            geoworldSummary?.FeatureTaxonomyCoveragePassed == true;
        var geoworldOverviewVisible = geoworldEntries.Any(entry =>
            entry.ArtifactKind == "text_svg_geoworld_stream_window_overview"
            && entry.Status == VisualWorldPreviewArtifactStatus.Passed
            && !string.IsNullOrWhiteSpace(entry.TextSvgPreviewPath));
        var goal099RelativePaths = geoworldEntries.Count > 0
            && geoworldEntries.All(entry =>
                IsSafeRelativePath(entry.RelativePath)
                && entry.RelativePath.StartsWith(Goal099SourceRoot + "/", StringComparison.Ordinal));
        var offlineHandoffGroup = groups.FirstOrDefault(group =>
            group.GroupId == "offline_geoworld_handoff");
        var offlineHandoffEntries = offlineHandoffGroup?.Entries.ToList() ?? [];
        var offlineHandoffSummary = offlineHandoffEntries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_handoff_workspace_summary");
        var offlineHandoffPayloadEntries = offlineHandoffEntries
            .Where(entry => entry.RelativePath.StartsWith(Goal100StreamingAssetsRoot + "/", StringComparison.Ordinal))
            .ToList();
        var offlineHandoffPackageCount = offlineHandoffSummary?.PackageCount ?? 0;
        var offlineHandoffFeatureCount = offlineHandoffSummary?.GeoworldNormalizedFeatureCount ?? 0;
        var offlineHandoffRecordCount = offlineHandoffSummary?.GeoworldVisualCacheRecordCount ?? 0;
        var offlineHandoffSourceChunkCount =
            offlineHandoffSummary?.GeoworldWorldSourceGraphChunkCount ?? 0;
        var offlineHandoffStreamChunkCount =
            offlineHandoffSummary?.GeoworldStreamWindowChunkCount ?? 0;
        var offlineHandoffPayloadFileCount = offlineHandoffPayloadEntries.Count;
        var offlineHandoffKindSummary =
            offlineHandoffSummary?.OfflineGeoworldHandoffFeatureKindCountsSummary ?? string.Empty;
        var offlineHandoffSimulatedReadPassed = proofs.Any(proof =>
            proof.ProofId == "goal100.simulated_read" && proof.Passed);
        var offlineHandoffNegativePassed = proofs.Any(proof =>
            proof.ProofId == "goal100.negative" && proof.Passed);
        var offlineHandoffAlphaUnchanged = proofs.Any(proof =>
            proof.ProofId == "goal100.alpha_runtime_bootstrap_unchanged" && proof.Passed);
        var offlineHandoffQualityPassed = proofs.Any(proof =>
            proof.ProofId == "goal100.quality_gate" && proof.Passed);
        var goal100RelativePaths = offlineHandoffEntries.Count > 0
            && offlineHandoffEntries.All(entry =>
                IsSafeRelativePath(entry.RelativePath)
                && (entry.RelativePath.StartsWith(Goal100SourceRoot + "/", StringComparison.Ordinal)
                    || entry.RelativePath.StartsWith(Goal100StreamingAssetsRoot + "/", StringComparison.Ordinal)));
        var unityPreview = BuildGoal101UnityPreviewQuality(groups, proofs);
        var requiredGroups = new[]
        {
            "microtiles",
            "map_patches",
            "region_composer",
            "world_profiles",
            "chunk_stream_windows",
            "cache_exports",
            "unity_handoff",
            "geoworld",
            "offline_geoworld_handoff",
            "offline_geoworld_unity_preview"
        };
        var requiredArtifactGroupsPresent = requiredGroups.All(required =>
            groups.Any(group => group.GroupId == required && group.EntryCount > 0));
        var noAbsolutePaths = groups.SelectMany(group => group.Entries)
            .Select(entry => entry.RelativePath)
            .Concat(svgEntries.Select(entry => entry.RelativePath))
            .All(IsSafeRelativePath);
        var noBinaryMedia = groups.SelectMany(group => group.Entries)
            .Select(entry => entry.RelativePath)
            .Concat(svgEntries.Select(entry => entry.RelativePath))
            .All(path => !IsBinaryOrRasterMedia(path));
        var proofStatusPassed = proofs.Count >= 32 && proofs.All(item => item.Passed);

        AddIfFalse(requiredArtifactGroupsPresent, "goal092.quality.groups_missing", "catalog", diagnostics);
        AddIfFalse(svgEntries.Count >= 4, "goal092.quality.svg_count", "catalog.svgEntries", diagnostics);
        AddIfFalse(goal091StreamEntries >= 4, "goal092.quality.goal091_missing", "chunk_stream_windows", diagnostics);
        AddIfFalse(cachePackageCount >= 4, "goal094.quality.cache_package_count", "cache_exports", diagnostics);
        AddIfFalse(cacheRecordCount == 93, "goal094.quality.cache_record_count", "cache_exports", diagnostics);
        AddIfFalse(runtimeHandoffVisible, "goal094.quality.runtime_handoff_missing", "cache_exports", diagnostics);
        AddIfFalse(
            runtimeHandoffMetadataOnly,
            "goal094.quality.runtime_handoff_not_metadata_only",
            "cache_exports",
            diagnostics);
        AddIfFalse(cacheReadbackProofPassed, "goal094.quality.readback_proof", "proofStatus", diagnostics);
        AddIfFalse(cacheOverlapProofPassed, "goal094.quality.overlap_proof", "proofStatus", diagnostics);
        AddIfFalse(cacheNegativeProofPassed, "goal094.quality.negative_proof", "proofStatus", diagnostics);
        AddIfFalse(cacheInvalidationPassed, "goal094.quality.invalidation_matrix", "proofStatus", diagnostics);
        AddIfFalse(cacheNoRawDump, "goal094.quality.no_raw_dump", "cache_exports", diagnostics);
        AddIfFalse(goal093RelativePaths, "goal094.quality.relative_goal093_paths", "cache_exports", diagnostics);
        AddIfFalse(unityGroup is not null, "goal096.quality.unity_group", "unity_handoff", diagnostics);
        AddIfFalse(unityPayloadFileCount == 5, "goal096.quality.payload_file_count", "unity_handoff", diagnostics);
        AddIfFalse(unityPackageCount == 4, "goal096.quality.package_count", "unity_handoff", diagnostics);
        AddIfFalse(unityRecordCount == 93, "goal096.quality.record_count", "unity_handoff", diagnostics);
        AddIfFalse(unityStreamWindowCount == 5, "goal096.quality.stream_window_count", "unity_handoff", diagnostics);
        AddIfFalse(unityUniqueChunkKeyCount == 93, "goal096.quality.chunk_key_count", "unity_handoff", diagnostics);
        AddIfFalse(unityProbeVisible, "goal096.quality.probe_visible", "unity_handoff", diagnostics);
        AddIfFalse(unityProbePassed, "goal096.quality.probe_passed", "unity_handoff", diagnostics);
        AddIfFalse(unitySimulatedReadProofPassed, "goal096.quality.simulated_read", "proofStatus", diagnostics);
        AddIfFalse(unityNegativeProofPassed, "goal096.quality.negative_proof", "proofStatus", diagnostics);
        AddIfFalse(unityAlphaBootstrapUnchanged, "goal096.quality.alpha_bootstrap", "proofStatus", diagnostics);
        AddIfFalse(unityForbiddenAreasUnchanged, "goal096.quality.forbidden_unity", "proofStatus", diagnostics);
        AddIfFalse(unityMetadataOnly, "goal096.quality.metadata_only", "proofStatus", diagnostics);
        AddIfFalse(unityPayloadHashesMatch, "goal096.quality.payload_hashes", "unity_handoff", diagnostics);
        AddIfFalse(goal095RelativePaths, "goal096.quality.relative_goal095_paths", "unity_handoff", diagnostics);
        AddIfFalse(noUnityFilesChanged, "goal096.quality.unity_files_changed", "unity_handoff", diagnostics);
        AddIfFalse(geoworldGroup is not null, "goal099.quality.geoworld_group", "geoworld", diagnostics);
        AddIfFalse(
            geoworldFeatureCount >= 10,
            "goal099.quality.normalized_feature_count",
            "geoworld",
            diagnostics);
        AddIfFalse(
            geoworldGraphChunkCount > 0,
            "goal099.quality.graph_chunk_count",
            "geoworld",
            diagnostics);
        AddIfFalse(
            geoworldStreamChunkCount > 0,
            "goal099.quality.stream_window_count",
            "geoworld",
            diagnostics);
        AddIfFalse(
            geoworldBoundaryPrefetchPassed,
            "goal099.quality.boundary_prefetch",
            "proofStatus",
            diagnostics);
        AddIfFalse(
            geoworldTaxonomyCoveragePassed,
            "goal099.quality.taxonomy_coverage",
            "geoworld",
            diagnostics);
        AddIfFalse(
            geoworldNegativeProofPassed,
            "goal099.quality.negative_proof",
            "proofStatus",
            diagnostics);
        AddIfFalse(
            geoworldQualityGatePassed,
            "goal099.quality.quality_gate",
            "proofStatus",
            diagnostics);
        AddIfFalse(
            geoworldOverviewVisible,
            "goal099.quality.overview_visible",
            "geoworld",
            diagnostics);
        AddIfFalse(
            goal099RelativePaths,
            "goal099.quality.relative_goal099_paths",
            "geoworld",
            diagnostics);
        AddIfFalse(
            offlineHandoffGroup is not null,
            "goal100.quality.offline_geoworld_handoff_group",
            "offline_geoworld_handoff",
            diagnostics);
        AddIfFalse(
            offlineHandoffPackageCount == 3,
            "goal100.quality.package_count",
            "offline_geoworld_handoff",
            diagnostics);
        AddIfFalse(
            offlineHandoffFeatureCount == 10,
            "goal100.quality.feature_count",
            "offline_geoworld_handoff",
            diagnostics);
        AddIfFalse(
            offlineHandoffRecordCount == 18,
            "goal100.quality.record_count",
            "offline_geoworld_handoff",
            diagnostics);
        AddIfFalse(
            offlineHandoffSourceChunkCount == 5,
            "goal100.quality.source_chunk_count",
            "offline_geoworld_handoff",
            diagnostics);
        AddIfFalse(
            offlineHandoffStreamChunkCount == 9,
            "goal100.quality.stream_window_count",
            "offline_geoworld_handoff",
            diagnostics);
        AddIfFalse(
            offlineHandoffPayloadFileCount == 5,
            "goal100.quality.payload_file_count",
            "offline_geoworld_handoff",
            diagnostics);
        AddIfFalse(
            !string.IsNullOrWhiteSpace(offlineHandoffKindSummary),
            "goal100.quality.feature_kind_counts",
            "offline_geoworld_handoff",
            diagnostics);
        AddIfFalse(
            offlineHandoffSimulatedReadPassed,
            "goal100.quality.simulated_read",
            "proofStatus",
            diagnostics);
        AddIfFalse(
            offlineHandoffNegativePassed,
            "goal100.quality.negative_proof",
            "proofStatus",
            diagnostics);
        AddIfFalse(
            offlineHandoffAlphaUnchanged,
            "goal100.quality.alpha_bootstrap",
            "proofStatus",
            diagnostics);
        AddIfFalse(
            offlineHandoffQualityPassed,
            "goal100.quality.quality_gate",
            "proofStatus",
            diagnostics);
        AddIfFalse(
            goal100RelativePaths,
            "goal100.quality.relative_goal100_paths",
            "offline_geoworld_handoff",
            diagnostics);
        AddGoal101UnityPreviewQualityDiagnostics(unityPreview, diagnostics);
        AddIfFalse(proofStatusPassed, "goal092.quality.proofs_failed", "proofStatus", diagnostics);
        AddIfFalse(noAbsolutePaths, "goal092.quality.absolute_path", "catalog", diagnostics);
        AddIfFalse(noBinaryMedia, "goal092.quality.binary_media", "catalog", diagnostics);
        AddIfFalse(binding.Passed, "goal092.quality.winforms_binding", "winformsBinding", diagnostics);
        AddIfFalse(
            binding.PageBindDisplaysCacheExports,
            "goal094.quality.winforms_cache_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(
            binding.PageBindDisplaysUnityHandoff,
            "goal096.quality.winforms_unity_handoff_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(
            binding.PageBindDisplaysGeoworld,
            "goal099.quality.winforms_geoworld_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(
            binding.PageBindDisplaysOfflineGeoworldHandoff,
            "goal100.quality.winforms_offline_geoworld_handoff_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(
            binding.PageBindDisplaysOfflineGeoworldUnityPreview,
            "goal101.quality.winforms_offline_geoworld_unity_preview_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(sourceHealth.Passed, "goal092.quality.source_health", "sourceHealth", diagnostics);
        foreach (var diagnostic in sourceDiagnostics
                     .Concat(sourceHealth.Diagnostics)
                     .Where(item => item.Severity == "error"))
        {
            diagnostics.Add(diagnostic);
        }

        var passed = diagnostics.All(item => item.Severity != "error");
        return new VisualWorldPreviewWorkspaceQualityGate
        {
            Accepted = false,
            Passed = passed,
            GroupCount = groupCount,
            EntryCount = entryCount,
            SvgTextPreviewCount = svgEntries.Count,
            Goal091StreamWindowEntryCount = goal091StreamEntries,
            CacheExportGroupPresent = cacheExportGroup is not null,
            CacheExportPackageCount = cachePackageCount,
            CacheExportRecordCount = cacheRecordCount,
            CacheExportSourceChunkCount = cacheSourceChunkCount,
            CacheExportStreamWindowCount = cacheStreamWindowCount,
            RuntimeHandoffSidecarVisible = runtimeHandoffVisible,
            RuntimeHandoffSidecarMetadataOnly = runtimeHandoffMetadataOnly,
            CacheReadbackProofPassed = cacheReadbackProofPassed,
            CacheOverlapReuseProofPassed = cacheOverlapProofPassed,
            CacheNegativeProofPassed = cacheNegativeProofPassed,
            CacheInvalidationMatrixPassed = cacheInvalidationPassed,
            CacheNoRawFullWorldDump = cacheNoRawDump,
            Goal093FilesDiscoveredByRelativePaths = goal093RelativePaths,
            UnityHandoffGroupPresent = unityGroup is not null,
            UnityPayloadFileCount = unityPayloadFileCount,
            UnityPackageCount = unityPackageCount,
            UnityExportRecordCount = unityRecordCount,
            UnityStreamWindowCount = unityStreamWindowCount,
            UnityUniqueChunkKeyCount = unityUniqueChunkKeyCount,
            UnityProbeSourceInventoryVisible = unityProbeVisible,
            UnityProbeSourceInventoryPassed = unityProbePassed,
            UnitySimulatedReadProofPassed = unitySimulatedReadProofPassed,
            UnityNegativeProofPassed = unityNegativeProofPassed,
            UnityAlphaRuntimeBootstrapUnchanged = unityAlphaBootstrapUnchanged,
            UnityForbiddenAreasUnchanged = unityForbiddenAreasUnchanged,
            UnityHandoffMetadataOnly = unityMetadataOnly,
            UnityPayloadHashesMatchGoal095Ledger = unityPayloadHashesMatch,
            Goal095FilesDiscoveredByRelativePaths = goal095RelativePaths,
            NoUnityFilesChangedByGoal096 = noUnityFilesChanged,
            GeoworldGroupPresent = geoworldGroup is not null,
            GeoworldOfflineBundleId = geoworldOfflineBundleId,
            GeoworldNormalizedFeatureCount = geoworldFeatureCount,
            GeoworldWorldSourceGraphChunkCount = geoworldGraphChunkCount,
            GeoworldStreamWindowChunkCount = geoworldStreamChunkCount,
            GeoworldBoundaryPrefetchPassed = geoworldBoundaryPrefetchPassed,
            GeoworldTaxonomyCoveragePassed = geoworldTaxonomyCoveragePassed,
            GeoworldNegativeProofPassed = geoworldNegativeProofPassed,
            GeoworldQualityGatePassed = geoworldQualityGatePassed,
            GeoworldOverviewVisible = geoworldOverviewVisible,
            Goal099FilesDiscoveredByRelativePaths = goal099RelativePaths,
            OfflineGeoworldHandoffGroupPresent = offlineHandoffGroup is not null,
            OfflineGeoworldHandoffPackageCount = offlineHandoffPackageCount,
            OfflineGeoworldHandoffFeatureCount = offlineHandoffFeatureCount,
            OfflineGeoworldHandoffVisualCacheRecordCount = offlineHandoffRecordCount,
            OfflineGeoworldHandoffSourceChunkCount = offlineHandoffSourceChunkCount,
            OfflineGeoworldHandoffStreamWindowChunkCount = offlineHandoffStreamChunkCount,
            OfflineGeoworldHandoffUnityPayloadFileCount = offlineHandoffPayloadFileCount,
            OfflineGeoworldHandoffFeatureKindCountsSummary = offlineHandoffKindSummary,
            OfflineGeoworldHandoffSimulatedReadProofPassed = offlineHandoffSimulatedReadPassed,
            OfflineGeoworldHandoffNegativeProofPassed = offlineHandoffNegativePassed,
            OfflineGeoworldHandoffAlphaRuntimeBootstrapUnchanged = offlineHandoffAlphaUnchanged,
            OfflineGeoworldHandoffQualityGatePassed = offlineHandoffQualityPassed,
            Goal100FilesDiscoveredByRelativePaths = goal100RelativePaths,
            OfflineGeoworldUnityPreviewGroupPresent = unityPreview.GroupPresent,
            OfflineGeoworldUnityPreviewCommandCount = unityPreview.CommandCount,
            OfflineGeoworldUnityPreviewCommandKindCount = unityPreview.CommandKindCount,
            OfflineGeoworldUnityPreviewTravelWindowStepCount =
                unityPreview.TravelWindowStepCount,
            OfflineGeoworldUnityPreviewUnityPayloadFileCount = unityPreview.PayloadFileCount,
            OfflineGeoworldUnityPreviewKindCoverageSummary =
                unityPreview.CommandKindCoverageSummary,
            OfflineGeoworldUnityPreviewUnityScriptsReady = unityPreview.UnityScriptsReady,
            OfflineGeoworldUnityPreviewSimulatedCommandProofPassed =
                unityPreview.SimulatedCommandProofPassed,
            OfflineGeoworldUnityPreviewNegativeProofPassed = unityPreview.NegativeProofPassed,
            OfflineGeoworldUnityPreviewAlphaRuntimeBootstrapUnchanged =
                unityPreview.AlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldUnityPreviewQualityGatePassed = unityPreview.QualityGatePassed,
            Goal101FilesDiscoveredByRelativePaths = unityPreview.RelativePaths,
            RequiredArtifactGroupsPresent = requiredArtifactGroupsPresent,
            Goal091StreamWindowsVisible = goal091StreamEntries >= 4,
            ProofStatusPassed = proofStatusPassed,
            NoAbsolutePaths = noAbsolutePaths,
            NoBinaryOrRasterMediaAdded = noBinaryMedia,
            WinFormsBindingReal = binding.Passed,
            WinFormsCacheExportBindingReal = binding.PageBindDisplaysCacheExports,
            WinFormsUnityHandoffBindingReal = binding.PageBindDisplaysUnityHandoff,
            WinFormsGeoworldBindingReal = binding.PageBindDisplaysGeoworld,
            WinFormsOfflineGeoworldHandoffBindingReal =
                binding.PageBindDisplaysOfflineGeoworldHandoff,
            WinFormsOfflineGeoworldUnityPreviewBindingReal =
                binding.PageBindDisplaysOfflineGeoworldUnityPreview,
            SourceHealthPassed = sourceHealth.Passed,
            ScannedCSharpFileCount = sourceHealth.ScannedCSharpFileCount,
            MaxLogicalLineCount = sourceHealth.MaxLogicalLineCount,
            MaxPhysicalLineLength = sourceHealth.MaxPhysicalLineLength,
            FilesOver1000LogicalLinesCount = sourceHealth.FilesOver1000LogicalLinesCount,
            FilesOver700LogicalLinesInGoal092NamespaceCount =
                sourceHealth.FilesOver700LogicalLinesInGoal092NamespaceCount,
            ZeroLfSourceCount = sourceHealth.ZeroLfSourceCount,
            CrOnlySourceCount = sourceHealth.CrOnlySourceCount,
            RawPhysicalOneLineSourceCount = sourceHealth.RawPhysicalOneLineSourceCount,
            MinifiedSourceCount = sourceHealth.MinifiedSourceCount,
            WorkspaceServiceLogicalLineCount = sourceHealth.WorkspaceServiceLogicalLineCount,
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.Application/Design/OfflineGeoworldWorldSourceGraph/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldWorldSourceGraph/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldWorldSourceGraphProductSmokeTests.cs",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                ".llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming/",
                ".llmgc/procedural/goal-096-unity-handoff-inspector-probe-readiness/",
                "docs/agent-tasks/goal-099-offline-geoworld-worldsourcegraph-streaming/",
                "docs/agent-tasks/goal-096-unity-handoff-inspector-probe-readiness/",
                "docs/agent-tasks/goal-100-offline-geoworld-visual-cache-unity-handoff/",
                "src/LLMGameCreator.Application/Design/OfflineGeoworldVisualCacheUnityHandoff/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldVisualCacheUnityHandoff/",
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldVisualCacheUnityHandoffProductSmokeTests.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldHandoffProbe.cs",
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal100/",
                ".llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/",
                "docs/agent-tasks/goal-101-offline-geoworld-unity-preview-runner/",
                "src/LLMGameCreator.Application/Design/OfflineGeoworldUnityPreviewRunner/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldUnityPreviewRunner/",
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldUnityPreviewRunnerProductSmokeTests.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPrimitiveFactory.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewTravelWindow.cs",
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal101/",
                ".llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = diagnostics
                .GroupBy(item => item.Code + "|" + item.Target + "|" + item.Message, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(item => item.Code, StringComparer.Ordinal)
                .ThenBy(item => item.Target, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static VisualWorldStreamPreviewWorkspaceReport BuildReport(
        VisualWorldStreamPreviewCatalog catalog,
        VisualWorldStreamPreviewProofStatusDocument proofStatus,
        VisualWorldPreviewWinFormsBindingInventory binding,
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        string catalogJson,
        string proofStatusJson,
        string bindingJson,
        string qualityJson,
        string sourceHealthJson) =>
        new()
        {
            Accepted = false,
            GroupCount = catalog.GroupCount,
            EntryCount = catalog.EntryCount,
            SvgTextPreviewCount = catalog.SvgTextPreviewCount,
            Goal091StreamWindowEntryCount = qualityGate.Goal091StreamWindowEntryCount,
            CacheExportPackageCount = qualityGate.CacheExportPackageCount,
            CacheExportRecordCount = qualityGate.CacheExportRecordCount,
            CacheExportSourceChunkCount = qualityGate.CacheExportSourceChunkCount,
            CacheExportStreamWindowCount = qualityGate.CacheExportStreamWindowCount,
            RuntimeHandoffSidecarVisible = qualityGate.RuntimeHandoffSidecarVisible,
            RuntimeHandoffSidecarMetadataOnly = qualityGate.RuntimeHandoffSidecarMetadataOnly,
            CacheReadbackProofPassed = qualityGate.CacheReadbackProofPassed,
            CacheOverlapReuseProofPassed = qualityGate.CacheOverlapReuseProofPassed,
            CacheNegativeProofPassed = qualityGate.CacheNegativeProofPassed,
            CacheInvalidationMatrixPassed = qualityGate.CacheInvalidationMatrixPassed,
            CacheNoRawFullWorldDump = qualityGate.CacheNoRawFullWorldDump,
            UnityPayloadFileCount = qualityGate.UnityPayloadFileCount,
            UnityPackageCount = qualityGate.UnityPackageCount,
            UnityExportRecordCount = qualityGate.UnityExportRecordCount,
            UnityStreamWindowCount = qualityGate.UnityStreamWindowCount,
            UnityUniqueChunkKeyCount = qualityGate.UnityUniqueChunkKeyCount,
            UnityProbeSourceInventoryVisible = qualityGate.UnityProbeSourceInventoryVisible,
            UnityProbeSourceInventoryPassed = qualityGate.UnityProbeSourceInventoryPassed,
            UnitySimulatedReadProofPassed = qualityGate.UnitySimulatedReadProofPassed,
            UnityNegativeProofPassed = qualityGate.UnityNegativeProofPassed,
            UnityAlphaRuntimeBootstrapUnchanged = qualityGate.UnityAlphaRuntimeBootstrapUnchanged,
            UnityForbiddenAreasUnchanged = qualityGate.UnityForbiddenAreasUnchanged,
            UnityHandoffMetadataOnly = qualityGate.UnityHandoffMetadataOnly,
            UnityPayloadHashesMatchGoal095Ledger = qualityGate.UnityPayloadHashesMatchGoal095Ledger,
            Goal095FilesDiscoveredByRelativePaths = qualityGate.Goal095FilesDiscoveredByRelativePaths,
            NoUnityFilesChangedByGoal096 = qualityGate.NoUnityFilesChangedByGoal096,
            GeoworldOfflineBundleId = qualityGate.GeoworldOfflineBundleId,
            GeoworldNormalizedFeatureCount = qualityGate.GeoworldNormalizedFeatureCount,
            GeoworldWorldSourceGraphChunkCount = qualityGate.GeoworldWorldSourceGraphChunkCount,
            GeoworldStreamWindowChunkCount = qualityGate.GeoworldStreamWindowChunkCount,
            GeoworldBoundaryPrefetchPassed = qualityGate.GeoworldBoundaryPrefetchPassed,
            GeoworldNegativeProofPassed = qualityGate.GeoworldNegativeProofPassed,
            GeoworldQualityGatePassed = qualityGate.GeoworldQualityGatePassed,
            Goal099FilesDiscoveredByRelativePaths = qualityGate.Goal099FilesDiscoveredByRelativePaths,
            OfflineGeoworldHandoffPackageCount = qualityGate.OfflineGeoworldHandoffPackageCount,
            OfflineGeoworldHandoffFeatureCount = qualityGate.OfflineGeoworldHandoffFeatureCount,
            OfflineGeoworldHandoffVisualCacheRecordCount =
                qualityGate.OfflineGeoworldHandoffVisualCacheRecordCount,
            OfflineGeoworldHandoffSourceChunkCount =
                qualityGate.OfflineGeoworldHandoffSourceChunkCount,
            OfflineGeoworldHandoffStreamWindowChunkCount =
                qualityGate.OfflineGeoworldHandoffStreamWindowChunkCount,
            OfflineGeoworldHandoffUnityPayloadFileCount =
                qualityGate.OfflineGeoworldHandoffUnityPayloadFileCount,
            OfflineGeoworldHandoffFeatureKindCountsSummary =
                qualityGate.OfflineGeoworldHandoffFeatureKindCountsSummary,
            OfflineGeoworldHandoffSimulatedReadProofPassed =
                qualityGate.OfflineGeoworldHandoffSimulatedReadProofPassed,
            OfflineGeoworldHandoffNegativeProofPassed =
                qualityGate.OfflineGeoworldHandoffNegativeProofPassed,
            OfflineGeoworldHandoffAlphaRuntimeBootstrapUnchanged =
                qualityGate.OfflineGeoworldHandoffAlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldHandoffQualityGatePassed =
                qualityGate.OfflineGeoworldHandoffQualityGatePassed,
            Goal100FilesDiscoveredByRelativePaths = qualityGate.Goal100FilesDiscoveredByRelativePaths,
            OfflineGeoworldUnityPreviewCommandCount =
                qualityGate.OfflineGeoworldUnityPreviewCommandCount,
            OfflineGeoworldUnityPreviewCommandKindCount =
                qualityGate.OfflineGeoworldUnityPreviewCommandKindCount,
            OfflineGeoworldUnityPreviewTravelWindowStepCount =
                qualityGate.OfflineGeoworldUnityPreviewTravelWindowStepCount,
            OfflineGeoworldUnityPreviewUnityPayloadFileCount =
                qualityGate.OfflineGeoworldUnityPreviewUnityPayloadFileCount,
            OfflineGeoworldUnityPreviewKindCoverageSummary =
                qualityGate.OfflineGeoworldUnityPreviewKindCoverageSummary,
            OfflineGeoworldUnityPreviewUnityScriptsReady =
                qualityGate.OfflineGeoworldUnityPreviewUnityScriptsReady,
            OfflineGeoworldUnityPreviewSimulatedCommandProofPassed =
                qualityGate.OfflineGeoworldUnityPreviewSimulatedCommandProofPassed,
            OfflineGeoworldUnityPreviewNegativeProofPassed =
                qualityGate.OfflineGeoworldUnityPreviewNegativeProofPassed,
            OfflineGeoworldUnityPreviewAlphaRuntimeBootstrapUnchanged =
                qualityGate.OfflineGeoworldUnityPreviewAlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldUnityPreviewQualityGatePassed =
                qualityGate.OfflineGeoworldUnityPreviewQualityGatePassed,
            Goal101FilesDiscoveredByRelativePaths = qualityGate.Goal101FilesDiscoveredByRelativePaths,
            ProofStatusPassed = proofStatus.Passed,
            WinFormsBindingPassed = binding.Passed,
            QualityGatePassed = qualityGate.Passed,
            SourceHealthPassed = qualityGate.SourceHealthPassed,
            WorkspaceServiceLogicalLineCount = qualityGate.WorkspaceServiceLogicalLineCount,
            MaxLogicalLineCount = qualityGate.MaxLogicalLineCount,
            FilesOver1000LogicalLinesCount = qualityGate.FilesOver1000LogicalLinesCount,
            FilesOver700LogicalLinesInGoal092NamespaceCount =
                qualityGate.FilesOver700LogicalLinesInGoal092NamespaceCount,
            CatalogHash = Sha256Text(catalogJson),
            ProofStatusHash = Sha256Text(proofStatusJson),
            WinFormsBindingInventoryHash = Sha256Text(bindingJson),
            QualityGateHash = Sha256Text(qualityJson),
            DeterministicReportHash = Sha256Text(sourceHealthJson)
        };

    private static string RenderReport(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldStreamPreviewCatalog catalog,
        VisualWorldStreamPreviewProofStatusDocument proofStatus,
        VisualWorldPreviewWinFormsBindingInventory binding,
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        string deterministicReportHash) =>
        RenderWorkspaceReport(
            report,
            catalog,
            proofStatus,
            binding,
            qualityGate,
            deterministicReportHash);
}
