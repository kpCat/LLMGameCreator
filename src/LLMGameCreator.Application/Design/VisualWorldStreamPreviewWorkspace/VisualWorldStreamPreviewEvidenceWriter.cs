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
        var requiredGroups = new[]
        {
            "microtiles",
            "map_patches",
            "region_composer",
            "world_profiles",
            "chunk_stream_windows",
            "cache_exports"
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
        var proofStatusPassed = proofs.Count >= 12 && proofs.All(item => item.Passed);

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
        AddIfFalse(proofStatusPassed, "goal092.quality.proofs_failed", "proofStatus", diagnostics);
        AddIfFalse(noAbsolutePaths, "goal092.quality.absolute_path", "catalog", diagnostics);
        AddIfFalse(noBinaryMedia, "goal092.quality.binary_media", "catalog", diagnostics);
        AddIfFalse(binding.Passed, "goal092.quality.winforms_binding", "winformsBinding", diagnostics);
        AddIfFalse(
            binding.PageBindDisplaysCacheExports,
            "goal094.quality.winforms_cache_binding",
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
            RequiredArtifactGroupsPresent = requiredArtifactGroupsPresent,
            Goal091StreamWindowsVisible = goal091StreamEntries >= 4,
            ProofStatusPassed = proofStatusPassed,
            NoAbsolutePaths = noAbsolutePaths,
            NoBinaryOrRasterMediaAdded = noBinaryMedia,
            WinFormsBindingReal = binding.Passed,
            WinFormsCacheExportBindingReal = binding.PageBindDisplaysCacheExports,
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
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                ".llmgc/procedural/goal-094-visual-chunk-cache-export-inspector/",
                "docs/agent-tasks/goal-094-visual-chunk-cache-export-inspector/",
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
            "# Goal 094 Visual Chunk Cache Export Inspector Report",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- manualGate: {report.ManualGate} required",
            $"- deterministicReportHash: {deterministicReportHash}",
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 094 extends the existing BCL-only Visual World Stream Preview Workspace so editor review can inspect Goal 093 cache/export artifacts beside the earlier visual stack. It loads Goal 093 JSON evidence by repository-relative path and does not add Runtime, Unity, provider, schema, project-file, dependency, binary media or raster media changes.",
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
