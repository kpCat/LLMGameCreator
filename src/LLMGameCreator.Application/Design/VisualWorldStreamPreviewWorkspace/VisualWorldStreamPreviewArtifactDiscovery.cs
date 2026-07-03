using System.Text.Json;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildMicrotileGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics,
        List<VisualWorldPreviewSvgEntry> svgEntries)
    {
        const string sourceGoalId = "goal_086_deterministic_visual_microtile_materializer";
        const string sourceRoot =
            ".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer";
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var ledger = LoadLedger(projectRoot, sourceRoot, "visual-microtile-file-ledger.json");
        var entries = BuildCoreEntries(
            projectRoot,
            sourceRoot,
            sourceGoalId,
            [
                ("visual-microtile-materializer-report.md", "report"),
                ("visual-microtile-preview-catalog.json", "catalog"),
                ("visual-microtile-quality-gate-scan.json", "quality_gate")
            ],
            ledger,
            groupDiagnostics);

        using var catalog = TryReadJson(
            projectRoot,
            sourceRoot + "/visual-microtile-preview-catalog.json",
            groupDiagnostics);
        if (catalog is not null
            && TryGetArray(catalog.RootElement, "previews", out var previews))
        {
            foreach (var preview in previews.OrderBy(
                item => TryGetString(item, "previewId"),
                StringComparer.Ordinal))
            {
                var previewId = TryGetString(preview, "previewId");
                var relativePath = sourceRoot + "/" + TryGetString(preview, "previewRelativePath");
                var metadata = "category=" + TryGetString(preview, "category")
                    + "; adultMetadataOnly=" + TryGetBool(preview, "adultMetadataOnly")
                    + "; safeFallback=" + TryGetString(preview, "safeFallbackPreviewId");
                AddSvgEntry(
                    projectRoot,
                    entries,
                    svgEntries,
                    sourceGoalId,
                    previewId,
                    relativePath,
                    "text_svg_microtile_preview",
                    metadata,
                    ledger,
                    groupDiagnostics);
            }
        }
        else
        {
            groupDiagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.microtiles.catalog_missing_previews",
                sourceRoot + "/visual-microtile-preview-catalog.json",
                "Goal 086 preview catalog must expose preview entries."));
        }

        diagnostics.AddRange(groupDiagnostics);
        return Group("microtiles", "Goal 086 Microtiles", sourceGoalId, sourceRoot, entries, groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactGroup BuildMapPatchGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics,
        List<VisualWorldPreviewSvgEntry> svgEntries)
    {
        const string sourceGoalId = "goal_087_deterministic_visual_map_patch_composer";
        const string sourceRoot =
            ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer";
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var ledger = LoadLedger(projectRoot, sourceRoot, "visual-map-patch-file-ledger.json");
        var entries = BuildCoreEntries(
            projectRoot,
            sourceRoot,
            sourceGoalId,
            [
                ("visual-map-patch-composer-report.md", "report"),
                ("visual-map-patch-catalog.json", "catalog"),
                ("visual-map-patch-quality-gate-scan.json", "quality_gate")
            ],
            ledger,
            groupDiagnostics);

        foreach (var path in EnumerateExistingFiles(projectRoot, sourceRoot + "/patches", "*.svg"))
        {
            var relativePath = Relative(projectRoot, path);
            AddSvgEntry(
                projectRoot,
                entries,
                svgEntries,
                sourceGoalId,
                Path.GetFileNameWithoutExtension(path),
                relativePath,
                "text_svg_map_patch_preview",
                "textSvg=true; raster=false; providerOutput=false",
                ledger,
                groupDiagnostics);
        }

        if (!entries.Any(item => item.ArtifactKind == "text_svg_map_patch_preview"))
        {
            groupDiagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.map_patches.svg_missing",
                sourceRoot + "/patches",
                "Goal 087 must expose text SVG patch previews."));
        }

        diagnostics.AddRange(groupDiagnostics);
        return Group("map_patches", "Goal 087 Map Patches", sourceGoalId, sourceRoot, entries, groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactGroup BuildRegionGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics,
        List<VisualWorldPreviewSvgEntry> svgEntries)
    {
        const string sourceGoalId = "goal_088_deterministic_visual_region_composer";
        const string sourceRoot =
            ".llmgc/procedural/goal-088-deterministic-visual-region-composer";
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        var entries = BuildCoreEntries(
            projectRoot,
            sourceRoot,
            sourceGoalId,
            [
                ("visual-region-composer-report.md", "report"),
                ("visual-region-definition.json", "definition"),
                ("visual-region-quality-gate-scan.json", "quality_gate")
            ],
            ledger,
            groupDiagnostics);

        foreach (var path in EnumerateExistingFiles(projectRoot, sourceRoot, "region-overview-*.svg"))
        {
            var relativePath = Relative(projectRoot, path);
            AddSvgEntry(
                projectRoot,
                entries,
                svgEntries,
                sourceGoalId,
                Path.GetFileNameWithoutExtension(path),
                relativePath,
                "text_svg_region_overview",
                "safeSvgOverview=true; compactRegionSummary=true",
                ledger,
                groupDiagnostics);
        }

        if (!entries.Any(item => item.ArtifactKind == "text_svg_region_overview"))
        {
            groupDiagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.region.svg_missing",
                sourceRoot,
                "Goal 088 must expose text SVG region overview files."));
        }

        diagnostics.AddRange(groupDiagnostics);
        return Group("region_composer", "Goal 088 Region Composer", sourceGoalId, sourceRoot, entries, groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactGroup BuildWorldProfileGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics,
        List<VisualWorldPreviewSvgEntry> svgEntries)
    {
        const string sourceGoalId = "goal_090_parameterized_visual_world_profiles";
        const string sourceRoot =
            ".llmgc/procedural/goal-090-parameterized-visual-world-profiles";
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        var entries = BuildCoreEntries(
            projectRoot,
            sourceRoot,
            sourceGoalId,
            [
                ("visual-world-profile-report.md", "report"),
                ("visual-world-profile-catalog.json", "catalog"),
                ("visual-world-profile-quality-gate-scan.json", "quality_gate")
            ],
            ledger,
            groupDiagnostics);

        using var catalog = TryReadJson(
            projectRoot,
            sourceRoot + "/visual-world-profile-catalog.json",
            groupDiagnostics);
        if (catalog is not null
            && TryGetArray(catalog.RootElement, "profiles", out var profiles))
        {
            foreach (var profile in profiles.OrderBy(
                item => TryGetString(item, "profileId"),
                StringComparer.Ordinal))
            {
                var profileId = TryGetString(profile, "profileId");
                var relativePath = sourceRoot + "/profile-overviews/" + profileId + ".svg";
                var metadata = "mode=" + TryGetString(profile, "mode")
                    + "; infinite=" + TryGetBool(profile, "isInfinite")
                    + "; rawCellDumpAllowed=" + TryGetBool(profile, "rawCellDumpAllowed");
                AddSvgEntry(
                    projectRoot,
                    entries,
                    svgEntries,
                    sourceGoalId,
                    profileId,
                    relativePath,
                    "text_svg_world_profile_overview",
                    metadata,
                    ledger,
                    groupDiagnostics);
            }
        }
        else
        {
            groupDiagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.world_profiles.catalog_missing_profiles",
                sourceRoot + "/visual-world-profile-catalog.json",
                "Goal 090 profile catalog must expose profile entries."));
        }

        diagnostics.AddRange(groupDiagnostics);
        return Group("world_profiles", "Goal 090 World Profiles", sourceGoalId, sourceRoot, entries, groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactGroup BuildChunkStreamGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics,
        List<VisualWorldPreviewSvgEntry> svgEntries)
    {
        const string sourceGoalId = "goal_091_deterministic_visual_chunk_stream_window";
        const string sourceRoot =
            ".llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window";
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var ledger = LoadLedger(projectRoot, sourceRoot, "visual-chunk-stream-file-ledger.json");
        var entries = BuildCoreEntries(
            projectRoot,
            sourceRoot,
            sourceGoalId,
            [
                ("visual-chunk-stream-window-report.md", "report"),
                ("visual-chunk-stream-window-catalog.json", "catalog"),
                ("visual-chunk-stream-quality-gate-scan.json", "quality_gate"),
                ("visual-chunk-stream-materialization-manifest.json", "materialization_manifest")
            ],
            ledger,
            groupDiagnostics);

        using var catalog = TryReadJson(
            projectRoot,
            sourceRoot + "/visual-chunk-stream-window-catalog.json",
            groupDiagnostics);
        if (catalog is not null
            && TryGetArray(catalog.RootElement, "fixtures", out var fixtures))
        {
            foreach (var fixture in fixtures.OrderBy(
                item => TryGetString(item, "fixtureId"),
                StringComparer.Ordinal))
            {
                var fixtureId = TryGetString(fixture, "fixtureId");
                var relativePath = sourceRoot + "/" + TryGetString(fixture, "overviewSvgRelativePath");
                var metadata = "profile=" + TryGetString(fixture, "profileId")
                    + "; mode=" + TryGetString(fixture, "mode")
                    + "; windows=" + TryGetInt(fixture, "windowCount")
                    + "; chunks=" + TryGetInt(fixture, "totalMaterializedChunks");
                AddSvgEntry(
                    projectRoot,
                    entries,
                    svgEntries,
                    sourceGoalId,
                    fixtureId,
                    relativePath,
                    "text_svg_chunk_stream_window_overview",
                    metadata,
                    ledger,
                    groupDiagnostics);
            }
        }
        else
        {
            groupDiagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal092.chunk_stream.catalog_missing_fixtures",
                sourceRoot + "/visual-chunk-stream-window-catalog.json",
                "Goal 091 stream catalog must expose fixtures."));
        }

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "chunk_stream_windows",
            "Goal 091 Chunk Stream Windows",
            sourceGoalId,
            sourceRoot,
            entries,
            groupDiagnostics);
    }
}
