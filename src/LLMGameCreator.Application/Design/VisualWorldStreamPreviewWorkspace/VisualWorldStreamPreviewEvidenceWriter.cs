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
        var requiredGroups = new[]
        {
            "microtiles",
            "map_patches",
            "region_composer",
            "world_profiles",
            "chunk_stream_windows",
            "cache_exports",
            "unity_handoff",
            "geoworld"
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
        var proofStatusPassed = proofs.Count >= 23 && proofs.All(item => item.Passed);

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
            RequiredArtifactGroupsPresent = requiredArtifactGroupsPresent,
            Goal091StreamWindowsVisible = goal091StreamEntries >= 4,
            ProofStatusPassed = proofStatusPassed,
            NoAbsolutePaths = noAbsolutePaths,
            NoBinaryOrRasterMediaAdded = noBinaryMedia,
            WinFormsBindingReal = binding.Passed,
            WinFormsCacheExportBindingReal = binding.PageBindDisplaysCacheExports,
            WinFormsUnityHandoffBindingReal = binding.PageBindDisplaysUnityHandoff,
            WinFormsGeoworldBindingReal = binding.PageBindDisplaysGeoworld,
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
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 096 Unity Handoff Inspector Probe Readiness Report",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- manualGate: {report.ManualGate} required",
            $"- deterministicReportHash: {deterministicReportHash}",
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 096 extends the existing BCL-only Visual World Stream Preview Workspace so editor review can inspect Goal 095 Unity StreamingAssets handoff readiness without launching Unity. It loads real Goal 095 evidence and mirrored payload files by repository-relative path, compares hashes against the Goal 095 ledgers, and does not change Runtime, Unity behavior, providers, schema, project files, dependencies, binary media or raster media.",
            string.Empty,
            "## Catalog",
            string.Empty,
            $"- groupCount: {catalog.GroupCount}",
            $"- entryCount: {catalog.EntryCount}",
            $"- svgTextPreviewCount: {catalog.SvgTextPreviewCount}",
            $"- goal091StreamWindowEntryCount: {qualityGate.Goal091StreamWindowEntryCount}",
            string.Empty
        };
        lines.AddRange(catalog.Groups.Select(group =>
            "- " + group.GroupId + ": entries=" + group.EntryCount
            + ", svgEntries=" + group.SvgEntryCount
            + ", sourceGoal=" + group.SourceGoalId
            + ", status=" + group.Status));
        lines.AddRange(
        [
            string.Empty,
            "## Cache Export Inspector",
            string.Empty,
            $"- cacheExportPackageCount: {report.CacheExportPackageCount}",
            $"- cacheExportRecordCount: {report.CacheExportRecordCount}",
            $"- cacheExportSourceChunkCount: {report.CacheExportSourceChunkCount}",
            $"- cacheExportStreamWindowCount: {report.CacheExportStreamWindowCount}",
            $"- runtimeHandoffSidecarVisible: {report.RuntimeHandoffSidecarVisible.ToString().ToLowerInvariant()}",
            $"- runtimeHandoffSidecarMetadataOnly: {report.RuntimeHandoffSidecarMetadataOnly.ToString().ToLowerInvariant()}",
            $"- cacheReadbackProofPassed: {report.CacheReadbackProofPassed.ToString().ToLowerInvariant()}",
            $"- cacheOverlapReuseProofPassed: {report.CacheOverlapReuseProofPassed.ToString().ToLowerInvariant()}",
            $"- cacheNegativeProofPassed: {report.CacheNegativeProofPassed.ToString().ToLowerInvariant()}",
            $"- cacheInvalidationMatrixPassed: {report.CacheInvalidationMatrixPassed.ToString().ToLowerInvariant()}",
            $"- cacheNoRawFullWorldDump: {report.CacheNoRawFullWorldDump.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Unity Handoff Inspector",
            string.Empty,
            $"- unityPayloadFileCount: {report.UnityPayloadFileCount}",
            $"- unityPackageCount: {report.UnityPackageCount}",
            $"- unityExportRecordCount: {report.UnityExportRecordCount}",
            $"- unityStreamWindowCount: {report.UnityStreamWindowCount}",
            $"- unityUniqueChunkKeyCount: {report.UnityUniqueChunkKeyCount}",
            $"- unityProbeSourceInventoryVisible: {report.UnityProbeSourceInventoryVisible.ToString().ToLowerInvariant()}",
            $"- unityProbeSourceInventoryPassed: {report.UnityProbeSourceInventoryPassed.ToString().ToLowerInvariant()}",
            $"- unitySimulatedReadProofPassed: {report.UnitySimulatedReadProofPassed.ToString().ToLowerInvariant()}",
            $"- unityNegativeProofPassed: {report.UnityNegativeProofPassed.ToString().ToLowerInvariant()}",
            $"- unityAlphaRuntimeBootstrapUnchanged: {report.UnityAlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant()}",
            $"- unityForbiddenAreasUnchanged: {report.UnityForbiddenAreasUnchanged.ToString().ToLowerInvariant()}",
            $"- unityHandoffMetadataOnly: {report.UnityHandoffMetadataOnly.ToString().ToLowerInvariant()}",
            $"- unityPayloadHashesMatchGoal095Ledger: {report.UnityPayloadHashesMatchGoal095Ledger.ToString().ToLowerInvariant()}",
            $"- goal095FilesDiscoveredByRelativePaths: {report.Goal095FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- noUnityFilesChangedByGoal096: {report.NoUnityFilesChangedByGoal096.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Geoworld Inspector",
            string.Empty,
            $"- geoworldOfflineBundleId: {report.GeoworldOfflineBundleId}",
            $"- geoworldNormalizedFeatureCount: {report.GeoworldNormalizedFeatureCount}",
            $"- geoworldWorldSourceGraphChunkCount: {report.GeoworldWorldSourceGraphChunkCount}",
            $"- geoworldStreamWindowChunkCount: {report.GeoworldStreamWindowChunkCount}",
            $"- geoworldBoundaryPrefetchPassed: {report.GeoworldBoundaryPrefetchPassed.ToString().ToLowerInvariant()}",
            $"- geoworldNegativeProofPassed: {report.GeoworldNegativeProofPassed.ToString().ToLowerInvariant()}",
            $"- geoworldQualityGatePassed: {report.GeoworldQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal099FilesDiscoveredByRelativePaths: {report.Goal099FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Proof Status",
            string.Empty,
            $"- proofStatusPassed: {proofStatus.Passed.ToString().ToLowerInvariant()}",
            $"- proofCount: {proofStatus.ProofCount}"
        ]);
        lines.AddRange(proofStatus.Proofs.Select(proof =>
            "- " + proof.ProofId + ": passed=" + proof.Passed.ToString().ToLowerInvariant()
            + ", path=" + proof.RelativePath));
        lines.AddRange(
        [
            string.Empty,
            "## WinForms Binding",
            string.Empty,
            $"- bindingPassed: {binding.Passed.ToString().ToLowerInvariant()}",
            $"- pageControlExists: {binding.PageControlExists.ToString().ToLowerInvariant()}",
            $"- designerExists: {binding.DesignerExists.ToString().ToLowerInvariant()}",
            $"- compositionRootRegistersService: {binding.CompositionRootRegistersService.ToString().ToLowerInvariant()}",
            $"- compositionRootRegistersPage: {binding.CompositionRootRegistersPage.ToString().ToLowerInvariant()}",
            $"- editorRegistryIncludesPage: {binding.EditorRegistryIncludesPage.ToString().ToLowerInvariant()}",
            $"- pageActivationLoadsApplicationResult: {binding.PageActivationLoadsApplicationResult.ToString().ToLowerInvariant()}",
            $"- pageBindDisplaysGroupsEntriesProofs: {binding.PageBindDisplaysGroupsEntriesProofs.ToString().ToLowerInvariant()}",
            $"- pageBindDisplaysCacheExports: {binding.PageBindDisplaysCacheExports.ToString().ToLowerInvariant()}",
            $"- pageBindDisplaysUnityHandoff: {binding.PageBindDisplaysUnityHandoff.ToString().ToLowerInvariant()}",
            $"- pageBindDisplaysGeoworld: {binding.PageBindDisplaysGeoworld.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Source Health",
            string.Empty,
            $"- sourceHealthPassed: {qualityGate.SourceHealthPassed.ToString().ToLowerInvariant()}",
            $"- scannedCSharpFileCount: {qualityGate.ScannedCSharpFileCount}",
            $"- workspaceServiceLogicalLineCount: {qualityGate.WorkspaceServiceLogicalLineCount}",
            $"- maxLogicalLineCount: {qualityGate.MaxLogicalLineCount}",
            $"- maxPhysicalLineLength: {qualityGate.MaxPhysicalLineLength}",
            $"- filesOver1000LogicalLinesCount: {qualityGate.FilesOver1000LogicalLinesCount}",
            $"- filesOver700LogicalLinesInGoal092NamespaceCount: {qualityGate.FilesOver700LogicalLinesInGoal092NamespaceCount}",
            $"- zeroLfSourceCount: {qualityGate.ZeroLfSourceCount}",
            $"- crOnlySourceCount: {qualityGate.CrOnlySourceCount}",
            $"- rawPhysicalOneLineSourceCount: {qualityGate.RawPhysicalOneLineSourceCount}",
            $"- minifiedSourceCount: {qualityGate.MinifiedSourceCount}",
            string.Empty,
            "## Quality Gate",
            string.Empty,
            $"- qualityGatePassed: {qualityGate.Passed.ToString().ToLowerInvariant()}",
            $"- requiredArtifactGroupsPresent: {qualityGate.RequiredArtifactGroupsPresent.ToString().ToLowerInvariant()}",
            $"- goal091StreamWindowsVisible: {qualityGate.Goal091StreamWindowsVisible.ToString().ToLowerInvariant()}",
            $"- cacheExportGroupPresent: {qualityGate.CacheExportGroupPresent.ToString().ToLowerInvariant()}",
            $"- goal093FilesDiscoveredByRelativePaths: {qualityGate.Goal093FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- unityHandoffGroupPresent: {qualityGate.UnityHandoffGroupPresent.ToString().ToLowerInvariant()}",
            $"- unityProbeSourceInventoryPassed: {qualityGate.UnityProbeSourceInventoryPassed.ToString().ToLowerInvariant()}",
            $"- unitySimulatedReadProofPassed: {qualityGate.UnitySimulatedReadProofPassed.ToString().ToLowerInvariant()}",
            $"- unityNegativeProofPassed: {qualityGate.UnityNegativeProofPassed.ToString().ToLowerInvariant()}",
            $"- unityAlphaRuntimeBootstrapUnchanged: {qualityGate.UnityAlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant()}",
            $"- unityForbiddenAreasUnchanged: {qualityGate.UnityForbiddenAreasUnchanged.ToString().ToLowerInvariant()}",
            $"- unityHandoffMetadataOnly: {qualityGate.UnityHandoffMetadataOnly.ToString().ToLowerInvariant()}",
            $"- unityPayloadHashesMatchGoal095Ledger: {qualityGate.UnityPayloadHashesMatchGoal095Ledger.ToString().ToLowerInvariant()}",
            $"- goal095FilesDiscoveredByRelativePaths: {qualityGate.Goal095FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- noUnityFilesChangedByGoal096: {qualityGate.NoUnityFilesChangedByGoal096.ToString().ToLowerInvariant()}",
            $"- geoworldGroupPresent: {qualityGate.GeoworldGroupPresent.ToString().ToLowerInvariant()}",
            $"- geoworldBoundaryPrefetchPassed: {qualityGate.GeoworldBoundaryPrefetchPassed.ToString().ToLowerInvariant()}",
            $"- geoworldTaxonomyCoveragePassed: {qualityGate.GeoworldTaxonomyCoveragePassed.ToString().ToLowerInvariant()}",
            $"- geoworldNegativeProofPassed: {qualityGate.GeoworldNegativeProofPassed.ToString().ToLowerInvariant()}",
            $"- geoworldQualityGatePassed: {qualityGate.GeoworldQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- geoworldOverviewVisible: {qualityGate.GeoworldOverviewVisible.ToString().ToLowerInvariant()}",
            $"- goal099FilesDiscoveredByRelativePaths: {qualityGate.Goal099FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- noAbsolutePaths: {qualityGate.NoAbsolutePaths.ToString().ToLowerInvariant()}",
            $"- noBinaryOrRasterMediaAdded: {qualityGate.NoBinaryOrRasterMediaAdded.ToString().ToLowerInvariant()}",
            $"- noRuntimeUnityProviderSchemaProjectDependencyChanges: {qualityGate.NoRuntimeUnityProviderSchemaProjectDependencyChanges.ToString().ToLowerInvariant()}",
            $"- noPromptDumps: {qualityGate.NoPromptDumps.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            $"- catalogHash: {report.CatalogHash}",
            $"- proofStatusHash: {report.ProofStatusHash}",
            $"- winFormsBindingInventoryHash: {report.WinFormsBindingInventoryHash}",
            $"- qualityGateHash: {report.QualityGateHash}"
        ]);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
