using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal099SourceGoalId =
        "goal_099_offline_geoworld_worldsourcegraph_streaming";
    private const string Goal099SourceRoot =
        ".llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming";

    private static VisualWorldPreviewArtifactGroup BuildGeoworldGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics,
        List<VisualWorldPreviewSvgEntry> svgEntries)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadGeoworldSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
            projectRoot,
            Goal099SourceRoot,
            Goal099SourceGoalId,
            [
                (OfflineGeoworldWorldSourceGraphEvidenceService.ReportMarkdownFileName, "offline_geoworld_report"),
                (OfflineGeoworldWorldSourceGraphEvidenceService.BundleCatalogJsonFileName, "offline_bundle_catalog"),
                (OfflineGeoworldWorldSourceGraphEvidenceService.NormalizedFeaturesJsonFileName, "normalized_features"),
                (OfflineGeoworldWorldSourceGraphEvidenceService.WorldSourceGraphJsonFileName, "worldsourcegraph"),
                (OfflineGeoworldWorldSourceGraphEvidenceService.StreamWindowPlanJsonFileName, "stream_window_plan"),
                (OfflineGeoworldWorldSourceGraphEvidenceService.BoundaryPrefetchProofJsonFileName, "boundary_prefetch_proof"),
                (OfflineGeoworldWorldSourceGraphEvidenceService.VisualProjectionSummaryJsonFileName, "visual_projection_summary"),
                (OfflineGeoworldWorldSourceGraphEvidenceService.NegativeProofJsonFileName, "negative_proof"),
                (OfflineGeoworldWorldSourceGraphEvidenceService.WorkspaceBindingInventoryJsonFileName, "workspace_binding_inventory"),
                (OfflineGeoworldWorldSourceGraphEvidenceService.SourceLineageJsonFileName, "source_lineage"),
                (OfflineGeoworldWorldSourceGraphEvidenceService.QualityGateScanJsonFileName, "quality_gate")
            ],
            new Dictionary<string, string>(StringComparer.Ordinal),
            groupDiagnostics)
            .Select(entry => WithGeoworldSummary(entry, summary))
            .ToList();

        entries.Add(WithGeoworldSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = Goal099SourceGoalId + ".summary",
                RelativePath = Goal099SourceRoot + "/"
                    + OfflineGeoworldWorldSourceGraphEvidenceService.BundleCatalogJsonFileName,
                ArtifactKind = "offline_geoworld_workspace_summary",
                SourceGoalId = Goal099SourceGoalId,
                Sha256 = HashFor(
                    projectRoot,
                    Goal099SourceRoot + "/"
                    + OfflineGeoworldWorldSourceGraphEvidenceService.BundleCatalogJsonFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.Passed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "bundle=" + summary.OfflineBundleId
                    + "; features=" + summary.NormalizedFeatureCount
                    + "; graphChunks=" + summary.WorldSourceGraphChunkCount
                    + "; streamChunks=" + summary.StreamWindowChunkCount,
                SafeRatingMetadataSummary = "syntheticOffline=true; noNetwork=true; accepted=false"
            },
            summary));

        if (!string.IsNullOrWhiteSpace(summary.OverviewSvgRelativePath))
        {
            AddSvgEntry(
                projectRoot,
                entries,
                svgEntries,
                Goal099SourceGoalId,
                "synthetic_city_radius_stream_window",
                Goal099SourceRoot + "/" + summary.OverviewSvgRelativePath,
                "text_svg_geoworld_stream_window_overview",
                summary.CompactOverviewEntry,
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics);
        }

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "geoworld",
            "Goal 099 Offline Geoworld",
            Goal099SourceGoalId,
            Goal099SourceRoot,
            entries,
            groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactEntry WithGeoworldSummary(
        VisualWorldPreviewArtifactEntry entry,
        GeoworldWorkspaceSummary summary) =>
        entry with
        {
            OfflineBundleId = summary.OfflineBundleId,
            GeoworldNormalizedFeatureCount = summary.NormalizedFeatureCount,
            GeoworldWorldSourceGraphChunkCount = summary.WorldSourceGraphChunkCount,
            GeoworldStreamWindowChunkCount = summary.StreamWindowChunkCount,
            BoundaryPrefetchStatus = summary.BoundaryPrefetchStatus,
            FeatureTaxonomyCoveragePassed = summary.FeatureTaxonomyCoveragePassed,
            GeoworldNegativeProofPassed = summary.NegativeProofPassed,
            GeoworldQualityGatePassed = summary.QualityGatePassed,
            CompactOverviewEntry = summary.CompactOverviewEntry,
            NegativeProofPassed = summary.NegativeProofPassed,
            MetadataOnly = true,
            NoRawFullWorldDump = summary.NoRawGeodataDump
        };

    private static GeoworldWorkspaceSummary LoadGeoworldSummary(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var catalog = TryReadJson(
            projectRoot,
            Goal099SourceRoot + "/" + OfflineGeoworldWorldSourceGraphEvidenceService.BundleCatalogJsonFileName,
            diagnostics);
        using var normalized = TryReadJson(
            projectRoot,
            Goal099SourceRoot + "/" + OfflineGeoworldWorldSourceGraphEvidenceService.NormalizedFeaturesJsonFileName,
            diagnostics);
        using var graph = TryReadJson(
            projectRoot,
            Goal099SourceRoot + "/" + OfflineGeoworldWorldSourceGraphEvidenceService.WorldSourceGraphJsonFileName,
            diagnostics);
        using var streamWindow = TryReadJson(
            projectRoot,
            Goal099SourceRoot + "/" + OfflineGeoworldWorldSourceGraphEvidenceService.StreamWindowPlanJsonFileName,
            diagnostics);
        using var prefetch = TryReadJson(
            projectRoot,
            Goal099SourceRoot + "/" + OfflineGeoworldWorldSourceGraphEvidenceService.BoundaryPrefetchProofJsonFileName,
            diagnostics);
        using var projection = TryReadJson(
            projectRoot,
            Goal099SourceRoot + "/" + OfflineGeoworldWorldSourceGraphEvidenceService.VisualProjectionSummaryJsonFileName,
            diagnostics);
        using var negative = TryReadJson(
            projectRoot,
            Goal099SourceRoot + "/" + OfflineGeoworldWorldSourceGraphEvidenceService.NegativeProofJsonFileName,
            diagnostics);
        using var quality = TryReadJson(
            projectRoot,
            Goal099SourceRoot + "/" + OfflineGeoworldWorldSourceGraphEvidenceService.QualityGateScanJsonFileName,
            diagnostics);

        var bundleId = catalog is null ? string.Empty : ReadFirstString(catalog.RootElement, "bundleIds");
        var featureCount = normalized is null ? 0 : ReadGeoworldInt(normalized.RootElement, "featureCount");
        var kindCount = normalized is null ? 0 : ReadArrayCount(normalized.RootElement, "featureKindsCovered");
        var graphChunkCount = graph is null ? 0 : ReadArrayCount(graph.RootElement, "chunks");
        var streamChunkCount = streamWindow is null ? 0 : ReadArrayCount(streamWindow.RootElement, "requiredChunkKeys");
        var boundaryStatus = streamWindow is null
            ? string.Empty
            : TryGetString(streamWindow.RootElement, "boundaryPrefetchStatus");
        var overviewPath = projection is null
            ? string.Empty
            : TryGetString(projection.RootElement, "overviewSvgRelativePath");
        var overview = projection is null
            ? string.Empty
            : TryGetString(projection.RootElement, "compactOverviewEntry");
        var prefetchPassed = prefetch is not null && TryGetBool(prefetch.RootElement, "passed");
        var negativePassed = negative is not null && TryGetBool(negative.RootElement, "passed");
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var noRawDump = quality is not null && TryGetBool(quality.RootElement, "noRawGeodataDump");
        var taxonomyCoverage = kindCount >= 10;
        var relativePaths = IsSafeRelativePath(Goal099SourceRoot)
            && IsSafeRelativePath(Goal099SourceRoot + "/" + overviewPath);
        var passed = !string.IsNullOrWhiteSpace(bundleId)
            && featureCount >= 10
            && graphChunkCount > 0
            && streamChunkCount > 0
            && prefetchPassed
            && taxonomyCoverage
            && negativePassed
            && qualityPassed
            && noRawDump
            && relativePaths;

        AddIfFalse(passed, "goal099.workspace.summary_failed", "geoworld", diagnostics);
        return new GeoworldWorkspaceSummary(
            Passed: passed,
            OfflineBundleId: bundleId,
            NormalizedFeatureCount: featureCount,
            WorldSourceGraphChunkCount: graphChunkCount,
            StreamWindowChunkCount: streamChunkCount,
            BoundaryPrefetchStatus: boundaryStatus,
            FeatureTaxonomyCoveragePassed: taxonomyCoverage,
            BoundaryPrefetchPassed: prefetchPassed,
            NegativeProofPassed: negativePassed,
            QualityGatePassed: qualityPassed,
            NoRawGeodataDump: noRawDump,
            OverviewSvgRelativePath: overviewPath,
            CompactOverviewEntry: overview);
    }

    private static string ReadFirstString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var array)
            || array.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        return array.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
    }

    private static int ReadArrayCount(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var array)
            || array.ValueKind != JsonValueKind.Array)
        {
            return 0;
        }

        return array.GetArrayLength();
    }

    private static int ReadGeoworldInt(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value : 0;

    private sealed record GeoworldWorkspaceSummary(
        bool Passed,
        string OfflineBundleId,
        int NormalizedFeatureCount,
        int WorldSourceGraphChunkCount,
        int StreamWindowChunkCount,
        string BoundaryPrefetchStatus,
        bool FeatureTaxonomyCoveragePassed,
        bool BoundaryPrefetchPassed,
        bool NegativeProofPassed,
        bool QualityGatePassed,
        bool NoRawGeodataDump,
        string OverviewSvgRelativePath,
        string CompactOverviewEntry);
}
