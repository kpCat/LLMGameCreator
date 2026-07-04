using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldInteractionPlayableProbe;
using LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class VisualWorldStreamPreviewWorkspaceProductSmokeTests
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

    [Fact]
    public async Task Goal096UnityHandoffInspectorProbeReadinessProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        await new OfflineGeoworldInteractionPlayableProbeEvidenceService()
            .BuildAndWriteAsync(repoRoot);
        var service = new VisualWorldStreamPreviewWorkspaceService();
        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);

        Assert.Equal(first.CatalogJson, second.CatalogJson);
        Assert.Equal(first.ProofStatusJson, second.ProofStatusJson);
        Assert.Equal(first.QualityGateScanJson, second.QualityGateScanJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);

        Assert.NotEmpty(first.ReportMarkdown);
        Assert.NotEmpty(first.CatalogJson);
        Assert.NotEmpty(first.ProofStatusJson);
        Assert.NotEmpty(first.WinFormsBindingInventoryJson);
        Assert.NotEmpty(first.QualityGateScanJson);
        Assert.NotEmpty(first.SourceHealthScanJson);

        using var catalog = JsonDocument.Parse(first.CatalogJson);
        using var proof = JsonDocument.Parse(first.ProofStatusJson);
        using var binding = JsonDocument.Parse(first.WinFormsBindingInventoryJson);
        using var quality = JsonDocument.Parse(first.QualityGateScanJson);
        using var sourceHealth = JsonDocument.Parse(first.SourceHealthScanJson);

        var groups = catalog.RootElement.GetProperty("groups").EnumerateArray().ToArray();
        Assert.True(catalog.RootElement.GetProperty("groupCount").GetInt32() >= 8);
        Assert.True(catalog.RootElement.GetProperty("svgTextPreviewCount").GetInt32() >= 5);
        Assert.Contains(groups, item => item.GetProperty("groupId").GetString() == "microtiles");
        Assert.Contains(groups, item => item.GetProperty("groupId").GetString() == "map_patches");
        Assert.Contains(groups, item => item.GetProperty("groupId").GetString() == "region_composer");
        Assert.Contains(groups, item => item.GetProperty("groupId").GetString() == "world_profiles");
        var cacheGroup = Assert.Single(
            groups,
            item => item.GetProperty("groupId").GetString() == "cache_exports");
        var cachePackages = cacheGroup
            .GetProperty("entries")
            .EnumerateArray()
            .Where(item => item.GetProperty("artifactKind").GetString() == "cache_export_package")
            .ToArray();
        Assert.Equal(4, cachePackages.Length);
        Assert.Equal(93, cachePackages.Sum(item => item.GetProperty("cacheRecordCount").GetInt32()));
        Assert.Equal(117, cachePackages.Sum(item => item.GetProperty("sourceChunkCount").GetInt32()));
        Assert.Equal(5, cachePackages.Sum(item => item.GetProperty("streamWindowCount").GetInt32()));
        var runtimeHandoff = Assert.Single(
            cachePackages,
            item => item.GetProperty("exportTargetKind").GetString() == "runtimeHandoff");
        Assert.True(runtimeHandoff.GetProperty("runtimeHandoffMetadataOnly").GetBoolean());
        Assert.Equal(27, runtimeHandoff.GetProperty("cacheRecordCount").GetInt32());
        Assert.True(runtimeHandoff.GetProperty("noRawFullWorldDump").GetBoolean());
        var streamGroup = Assert.Single(
            groups,
            item => item.GetProperty("groupId").GetString() == "chunk_stream_windows");
        var streamEntries = streamGroup.GetProperty("entries").EnumerateArray().ToArray();
        Assert.Equal(
            4,
            streamEntries.Count(item =>
                item.GetProperty("artifactKind").GetString()
                == "text_svg_chunk_stream_window_overview"));
        var unityGroup = Assert.Single(
            groups,
            item => item.GetProperty("groupId").GetString() == "unity_handoff");
        var unityEntries = unityGroup.GetProperty("entries").EnumerateArray().ToArray();
        var unityPayloadFiles = unityEntries
            .Where(item =>
                (item.GetProperty("relativePath").GetString() ?? string.Empty).StartsWith(
                    "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/VisualChunkCacheGoal095/",
                    StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(5, unityPayloadFiles.Length);
        Assert.Contains(unityEntries, item =>
            item.GetProperty("artifactKind").GetString() == "unity_probe_source_inventory");
        Assert.Contains(unityEntries, item =>
            item.GetProperty("artifactKind").GetString() == "alpha_runtime_bootstrap_unchanged_status");
        Assert.All(unityPayloadFiles, entry =>
        {
            Assert.Equal(5, entry.GetProperty("payloadFileCount").GetInt32());
            Assert.Equal(4, entry.GetProperty("packageCount").GetInt32());
            Assert.Equal(93, entry.GetProperty("exportRecordCount").GetInt32());
            Assert.Equal(5, entry.GetProperty("streamWindowCount").GetInt32());
            Assert.Equal(93, entry.GetProperty("uniqueChunkKeyCount").GetInt32());
            Assert.True(entry.GetProperty("payloadHashesMatchGoal095Ledger").GetBoolean());
            Assert.True(entry.GetProperty("simulatedUnityReadProofPassed").GetBoolean());
            Assert.True(entry.GetProperty("negativeProofPassed").GetBoolean());
            Assert.True(entry.GetProperty("probeSourceInventoryPassed").GetBoolean());
            Assert.True(entry.GetProperty("alphaRuntimeBootstrapUnchanged").GetBoolean());
        });
        var geoworldGroup = Assert.Single(
            groups,
            item => item.GetProperty("groupId").GetString() == "geoworld");
        var geoworldEntries = geoworldGroup.GetProperty("entries").EnumerateArray().ToArray();
        var geoworldSummary = Assert.Single(
            geoworldEntries,
            item => item.GetProperty("artifactKind").GetString() == "offline_geoworld_workspace_summary");
        Assert.Equal(
            OfflineGeoworldBundleFixtures.SyntheticCityRadiusBundleId,
            geoworldSummary.GetProperty("offlineBundleId").GetString());
        Assert.Equal(10, geoworldSummary.GetProperty("geoworldNormalizedFeatureCount").GetInt32());
        Assert.Equal(9, geoworldSummary.GetProperty("geoworldStreamWindowChunkCount").GetInt32());
        Assert.Equal(
            "scheduled_no_network_cache_first",
            geoworldSummary.GetProperty("boundaryPrefetchStatus").GetString());
        Assert.True(geoworldSummary.GetProperty("featureTaxonomyCoveragePassed").GetBoolean());
        Assert.True(geoworldSummary.GetProperty("geoworldNegativeProofPassed").GetBoolean());
        Assert.True(geoworldSummary.GetProperty("geoworldQualityGatePassed").GetBoolean());
        Assert.Contains(
            geoworldEntries,
            item => item.GetProperty("artifactKind").GetString() == "text_svg_geoworld_stream_window_overview");
        var handoffGroup = Assert.Single(
            groups,
            item => item.GetProperty("groupId").GetString() == "offline_geoworld_handoff");
        var handoffEntries = handoffGroup.GetProperty("entries").EnumerateArray().ToArray();
        var handoffSummary = Assert.Single(
            handoffEntries,
            item => item.GetProperty("artifactKind").GetString()
                    == "offline_geoworld_handoff_workspace_summary");
        var handoffPayloadFiles = handoffEntries
            .Where(item =>
                item.GetProperty("artifactKind").GetString()
                == "offline_geoworld_streamingassets_payload")
            .ToArray();
        Assert.Equal(3, handoffSummary.GetProperty("packageCount").GetInt32());
        Assert.Equal(10, handoffSummary.GetProperty("geoworldNormalizedFeatureCount").GetInt32());
        Assert.Equal(18, handoffSummary.GetProperty("geoworldVisualCacheRecordCount").GetInt32());
        Assert.Equal(5, handoffSummary.GetProperty("geoworldWorldSourceGraphChunkCount").GetInt32());
        Assert.Equal(9, handoffSummary.GetProperty("geoworldStreamWindowChunkCount").GetInt32());
        Assert.Equal(5, handoffPayloadFiles.Length);
        Assert.True(handoffSummary.GetProperty("simulatedUnityReadProofPassed").GetBoolean());
        Assert.True(handoffSummary.GetProperty("negativeProofPassed").GetBoolean());
        Assert.True(handoffSummary.GetProperty("alphaRuntimeBootstrapUnchanged").GetBoolean());
        Assert.True(handoffSummary.GetProperty("offlineGeoworldHandoffQualityGatePassed").GetBoolean());
        var previewGroup = Assert.Single(
            groups,
            item => item.GetProperty("groupId").GetString() == "offline_geoworld_unity_preview");
        var previewEntries = previewGroup.GetProperty("entries").EnumerateArray().ToArray();
        var previewSummary = Assert.Single(
            previewEntries,
            item => item.GetProperty("artifactKind").GetString()
                    == "offline_geoworld_unity_preview_workspace_summary");
        var previewPayloadFiles = previewEntries
            .Where(item =>
                item.GetProperty("artifactKind").GetString()
                == "offline_geoworld_unity_preview_streamingassets_payload")
            .ToArray();
        var previewScripts = previewEntries
            .Where(item =>
                item.GetProperty("artifactKind").GetString()
                == "offline_geoworld_unity_preview_script")
            .ToArray();
        Assert.Equal(18, previewSummary.GetProperty("offlineGeoworldUnityPreviewCommandCount").GetInt32());
        Assert.Equal(10, previewSummary.GetProperty("offlineGeoworldUnityPreviewCommandKindCount").GetInt32());
        Assert.True(previewSummary.GetProperty("offlineGeoworldUnityPreviewTravelWindowStepCount").GetInt32() >= 4);
        Assert.Equal(5, previewPayloadFiles.Length);
        Assert.Equal(3, previewScripts.Length);
        Assert.True(previewSummary.GetProperty("offlineGeoworldUnityPreviewUnityScriptsReady").GetBoolean());
        Assert.True(previewSummary.GetProperty("offlineGeoworldUnityPreviewSimulatedCommandProofPassed").GetBoolean());
        Assert.True(previewSummary.GetProperty("offlineGeoworldUnityPreviewQualityGatePassed").GetBoolean());
        var editorGroup = Assert.Single(
            groups,
            item => item.GetProperty("groupId").GetString() == "offline_geoworld_unity_editor_preview");
        var editorEntries = editorGroup.GetProperty("entries").EnumerateArray().ToArray();
        var editorSummary = Assert.Single(
            editorEntries,
            item => item.GetProperty("artifactKind").GetString()
                    == "offline_geoworld_unity_editor_preview_workspace_summary");
        var editorScripts = editorEntries
            .Where(item =>
                item.GetProperty("artifactKind").GetString()
                == "offline_geoworld_unity_editor_preview_script")
            .ToArray();
        Assert.Equal(18, editorSummary.GetProperty("offlineGeoworldUnityEditorPreviewCommandCount").GetInt32());
        Assert.Equal(10, editorSummary.GetProperty("offlineGeoworldUnityEditorPreviewCommandKindCount").GetInt32());
        Assert.Equal(18, editorSummary.GetProperty("offlineGeoworldUnityEditorPreviewExpectedObjectCount").GetInt32());
        Assert.Single(editorScripts);
        Assert.True(editorSummary.GetProperty("offlineGeoworldUnityEditorPreviewEditorWindowScriptReady").GetBoolean());
        Assert.True(editorSummary.GetProperty("offlineGeoworldUnityEditorPreviewSimulatedActionProofPassed").GetBoolean());
        Assert.True(editorSummary.GetProperty("offlineGeoworldUnityEditorPreviewClearOperationProofPassed").GetBoolean());
        Assert.True(editorSummary.GetProperty("offlineGeoworldUnityEditorPreviewQualityGatePassed").GetBoolean());
        var playModeGroup = Assert.Single(
            groups,
            item => item.GetProperty("groupId").GetString() == "offline_geoworld_playmode_travel");
        var playModeEntries = playModeGroup.GetProperty("entries").EnumerateArray().ToArray();
        var playModeSummary = Assert.Single(
            playModeEntries,
            item => item.GetProperty("artifactKind").GetString()
                    == "offline_geoworld_playmode_travel_workspace_summary");
        var playModePayloads = playModeEntries
            .Where(item =>
                item.GetProperty("artifactKind").GetString()
                == "offline_geoworld_playmode_streamingassets_payload")
            .ToArray();
        var playModeScripts = playModeEntries
            .Where(item =>
                item.GetProperty("artifactKind").GetString()
                == "offline_geoworld_playmode_unity_script")
            .ToArray();
        Assert.True(playModeSummary.GetProperty("offlineGeoworldPlayModeTravelStepCount").GetInt32() >= 4);
        Assert.Equal(18, playModeSummary.GetProperty("offlineGeoworldPlayModeTravelObjectCount").GetInt32());
        Assert.Equal(5, playModePayloads.Length);
        Assert.Equal(3, playModeScripts.Length);
        Assert.True(playModeSummary.GetProperty("offlineGeoworldPlayModeTravelUnityScriptsReady").GetBoolean());
        Assert.True(playModeSummary.GetProperty("offlineGeoworldPlayModeTravelEditorWindowReady").GetBoolean());
        Assert.True(playModeSummary.GetProperty("offlineGeoworldPlayModeTravelSimulatedExecutionProofPassed").GetBoolean());
        Assert.True(playModeSummary.GetProperty("offlineGeoworldPlayModeTravelGoal102BClosureRecorded").GetBoolean());
        Assert.True(playModeSummary.GetProperty("offlineGeoworldPlayModeTravelQualityGatePassed").GetBoolean());
        var interactiveGroup = Assert.Single(
            groups,
            item => item.GetProperty("groupId").GetString() == "offline_geoworld_interactive_travel");
        var interactiveEntries = interactiveGroup.GetProperty("entries").EnumerateArray().ToArray();
        var interactiveSummary = Assert.Single(
            interactiveEntries,
            item => item.GetProperty("artifactKind").GetString()
                    == "offline_geoworld_interactive_travel_workspace_summary");
        var interactivePayloads = interactiveEntries
            .Where(item =>
                item.GetProperty("artifactKind").GetString()
                == "offline_geoworld_interactive_streamingassets_payload")
            .ToArray();
        var interactiveScripts = interactiveEntries
            .Where(item =>
                item.GetProperty("artifactKind").GetString()
                == "offline_geoworld_interactive_unity_script")
            .ToArray();
        Assert.Equal(6, interactiveSummary.GetProperty("offlineGeoworldInteractiveTravelMovementSampleCount").GetInt32());
        Assert.Equal(2, interactiveSummary.GetProperty("offlineGeoworldInteractiveTravelBoundaryCrossingCount").GetInt32());
        Assert.Equal(18, interactiveSummary.GetProperty("offlineGeoworldInteractiveTravelObjectCount").GetInt32());
        Assert.Equal(5, interactivePayloads.Length);
        Assert.Equal(3, interactiveScripts.Length);
        Assert.True(interactiveSummary.GetProperty("offlineGeoworldInteractiveTravelUnityScriptsReady").GetBoolean());
        Assert.True(interactiveSummary.GetProperty("offlineGeoworldInteractiveTravelEditorWindowReady").GetBoolean());
        Assert.True(interactiveSummary.GetProperty("offlineGeoworldInteractiveTravelSimulatedExecutionProofPassed").GetBoolean());
        Assert.True(interactiveSummary.GetProperty("offlineGeoworldInteractiveTravelNegativeProofPassed").GetBoolean());
        Assert.True(interactiveSummary.GetProperty("offlineGeoworldInteractiveTravelQualityGatePassed").GetBoolean());
        var interactionGroup = Assert.Single(
            groups,
            item => item.GetProperty("groupId").GetString() == "offline_geoworld_interactions");
        var interactionEntries = interactionGroup.GetProperty("entries").EnumerateArray().ToArray();
        var interactionSummary = Assert.Single(
            interactionEntries,
            item => item.GetProperty("artifactKind").GetString()
                    == "offline_geoworld_interaction_workspace_summary");
        var interactionPayloads = interactionEntries
            .Where(item =>
                item.GetProperty("artifactKind").GetString()
                == "offline_geoworld_interaction_streamingassets_payload")
            .ToArray();
        var interactionScripts = interactionEntries
            .Where(item =>
                item.GetProperty("artifactKind").GetString()
                == "offline_geoworld_interaction_unity_script")
            .ToArray();
        var interactionEditorScript = Assert.Single(
            interactionEntries,
            item => item.GetProperty("artifactKind").GetString()
                    == "offline_geoworld_interaction_editor_window_script");
        Assert.True(interactionSummary.GetProperty("offlineGeoworldInteractionTargetCount").GetInt32() >= 8);
        Assert.True(interactionSummary.GetProperty("offlineGeoworldInteractionActionKindCount").GetInt32() >= 5);
        Assert.True(interactionSummary.GetProperty("offlineGeoworldInteractionScriptedEventCount").GetInt32() >= 6);
        Assert.True(interactionSummary.GetProperty("offlineGeoworldInteractionStateDeltaCount").GetInt32() >= 6);
        Assert.Equal(6, interactionPayloads.Length);
        Assert.Equal(3, interactionScripts.Length);
        Assert.True(interactionEditorScript.GetProperty("offlineGeoworldInteractionEditorWindowReady").GetBoolean());
        Assert.True(interactionSummary.GetProperty("offlineGeoworldInteractionStateHashChainPassed").GetBoolean());
        Assert.True(interactionSummary.GetProperty("offlineGeoworldInteractionUnityScriptsReady").GetBoolean());
        Assert.True(interactionSummary.GetProperty("offlineGeoworldInteractionEditorWindowReady").GetBoolean());
        Assert.True(interactionSummary.GetProperty("offlineGeoworldInteractionUnitySafetyScanPassed").GetBoolean());
        Assert.True(interactionSummary.GetProperty("offlineGeoworldInteractionSimulatedSessionProofPassed").GetBoolean());
        Assert.True(interactionSummary.GetProperty("offlineGeoworldInteractionNegativeProofPassed").GetBoolean());
        Assert.True(interactionSummary.GetProperty("offlineGeoworldInteractionAlphaRuntimeBootstrapUnchanged").GetBoolean());
        Assert.True(interactionSummary.GetProperty("offlineGeoworldInteractionQualityGatePassed").GetBoolean());

        var svgEntries = catalog.RootElement.GetProperty("svgEntries").EnumerateArray().ToArray();
        Assert.All(svgEntries, entry =>
        {
            var relativePath = entry.GetProperty("relativePath").GetString() ?? string.Empty;
            Assert.False(Path.IsPathFullyQualified(relativePath), relativePath);
            Assert.EndsWith(".svg", relativePath);
            Assert.True(entry.GetProperty("safeToDisplayAsText").GetBoolean());
        });

        var proofs = proof.RootElement.GetProperty("proofs").EnumerateArray().ToArray();
        Assert.True(proof.RootElement.GetProperty("passed").GetBoolean());
        AssertProofPassed(proofs, "goal091.seam");
        AssertProofPassed(proofs, "goal091.cache_reuse");
        AssertProofPassed(proofs, "goal091.layer_transition");
        AssertProofPassed(proofs, "goal091.negative");
        AssertProofPassed(proofs, "goal093.readback");
        AssertProofPassed(proofs, "goal093.overlap_reuse");
        AssertProofPassed(proofs, "goal093.negative");
        AssertProofPassed(proofs, "goal093.invalidation_matrix");
        AssertProofPassed(proofs, "goal093.runtime_handoff_metadata_only");
        AssertProofPassed(proofs, "goal095.streamingassets_ledger");
        AssertProofPassed(proofs, "goal095.simulated_read");
        AssertProofPassed(proofs, "goal095.negative");
        AssertProofPassed(proofs, "goal095.probe_source_inventory");
        AssertProofPassed(proofs, "goal095.alpha_runtime_bootstrap_unchanged");
        AssertProofPassed(proofs, "goal095.forbidden_unity_areas_unchanged");
        AssertProofPassed(proofs, "goal095.metadata_only");
        AssertProofPassed(proofs, "goal099.boundary_prefetch");
        AssertProofPassed(proofs, "goal099.negative");
        AssertProofPassed(proofs, "goal099.visual_projection");
        AssertProofPassed(proofs, "goal099.quality_gate");
        AssertProofPassed(proofs, "goal100.streamingassets_ledger");
        AssertProofPassed(proofs, "goal100.simulated_read");
        AssertProofPassed(proofs, "goal100.negative");
        AssertProofPassed(proofs, "goal100.probe_source_inventory");
        AssertProofPassed(proofs, "goal100.alpha_runtime_bootstrap_unchanged");
        AssertProofPassed(proofs, "goal100.visual_cache_records");
        AssertProofPassed(proofs, "goal100.all_feature_kinds_mapped");
        AssertProofPassed(proofs, "goal100.workspace_binding");
        AssertProofPassed(proofs, "goal100.quality_gate");
        AssertProofPassed(proofs, "goal101.streamingassets_ledger");
        AssertProofPassed(proofs, "goal101.unity_script_inventory");
        AssertProofPassed(proofs, "goal101.simulated_command");
        AssertProofPassed(proofs, "goal101.negative");
        AssertProofPassed(proofs, "goal101.alpha_runtime_bootstrap_unchanged");
        AssertProofPassed(proofs, "goal101.all_command_kinds_mapped");
        AssertProofPassed(proofs, "goal101.travel_window_demo");
        AssertProofPassed(proofs, "goal101.quality_gate");
        AssertProofPassed(proofs, "goal102.tool_inventory");
        AssertProofPassed(proofs, "goal102.editor_window_menu");
        AssertProofPassed(proofs, "goal102.simulated_action");
        AssertProofPassed(proofs, "goal102.clear_operation");
        AssertProofPassed(proofs, "goal102.negative");
        AssertProofPassed(proofs, "goal102.alpha_runtime_bootstrap_unchanged");
        AssertProofPassed(proofs, "goal102.quality_gate");
        AssertProofPassed(proofs, "goal103.unity_script_inventory");
        AssertProofPassed(proofs, "goal103.editor_window_inventory");
        AssertProofPassed(proofs, "goal103.simulated_execution");
        AssertProofPassed(proofs, "goal103.negative");
        AssertProofPassed(proofs, "goal103.goal102b_closure");
        AssertProofPassed(proofs, "goal103.alpha_runtime_bootstrap_unchanged");
        AssertProofPassed(proofs, "goal103.boundary_prefetch");
        AssertProofPassed(proofs, "goal103.quality_gate");
        AssertProofPassed(proofs, "goal104.unity_script_inventory");
        AssertProofPassed(proofs, "goal104.editor_window_inventory");
        AssertProofPassed(proofs, "goal104.simulated_execution");
        AssertProofPassed(proofs, "goal104.negative");
        AssertProofPassed(proofs, "goal104.alpha_runtime_bootstrap_unchanged");
        AssertProofPassed(proofs, "goal104.boundary_crossings");
        AssertProofPassed(proofs, "goal104.prefetch_plan");
        AssertProofPassed(proofs, "goal104.quality_gate");
        AssertProofPassed(proofs, "goal105.unity_script_inventory");
        AssertProofPassed(proofs, "goal105.editor_window_inventory");
        AssertProofPassed(proofs, "goal105.simulated_session");
        AssertProofPassed(proofs, "goal105.negative");
        AssertProofPassed(proofs, "goal105.alpha_runtime_bootstrap_unchanged");
        AssertProofPassed(proofs, "goal105.state_hash_chain");
        AssertProofPassed(proofs, "goal105.quality_gate");
        Assert.True(binding.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(binding.RootElement.GetProperty("pageBindDisplaysCacheExports").GetBoolean());
        Assert.True(binding.RootElement.GetProperty("pageBindDisplaysUnityHandoff").GetBoolean());
        Assert.True(binding.RootElement.GetProperty("pageBindDisplaysGeoworld").GetBoolean());
        Assert.True(binding.RootElement.GetProperty("pageBindDisplaysOfflineGeoworldHandoff").GetBoolean());
        Assert.True(binding.RootElement.GetProperty("pageBindDisplaysOfflineGeoworldUnityPreview").GetBoolean());
        Assert.True(binding.RootElement.GetProperty("pageBindDisplaysOfflineGeoworldUnityEditorPreview").GetBoolean());
        Assert.True(binding.RootElement.GetProperty("pageBindDisplaysOfflineGeoworldPlayModeTravel").GetBoolean());
        Assert.True(binding.RootElement.GetProperty("pageBindDisplaysOfflineGeoworldInteractiveTravel").GetBoolean());
        Assert.True(binding.RootElement.GetProperty("pageBindDisplaysOfflineGeoworldInteractions").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("goal091StreamWindowsVisible").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("cacheExportGroupPresent").GetBoolean());
        Assert.Equal(4, quality.RootElement.GetProperty("cacheExportPackageCount").GetInt32());
        Assert.Equal(93, quality.RootElement.GetProperty("cacheExportRecordCount").GetInt32());
        Assert.True(quality.RootElement.GetProperty("runtimeHandoffSidecarVisible").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("runtimeHandoffSidecarMetadataOnly").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("cacheReadbackProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("cacheOverlapReuseProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("cacheNegativeProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("cacheInvalidationMatrixPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("cacheNoRawFullWorldDump").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("goal093FilesDiscoveredByRelativePaths").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("unityHandoffGroupPresent").GetBoolean());
        Assert.Equal(5, quality.RootElement.GetProperty("unityPayloadFileCount").GetInt32());
        Assert.Equal(4, quality.RootElement.GetProperty("unityPackageCount").GetInt32());
        Assert.Equal(93, quality.RootElement.GetProperty("unityExportRecordCount").GetInt32());
        Assert.Equal(5, quality.RootElement.GetProperty("unityStreamWindowCount").GetInt32());
        Assert.Equal(93, quality.RootElement.GetProperty("unityUniqueChunkKeyCount").GetInt32());
        Assert.True(quality.RootElement.GetProperty("unityProbeSourceInventoryVisible").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("unityProbeSourceInventoryPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("unitySimulatedReadProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("unityNegativeProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("unityAlphaRuntimeBootstrapUnchanged").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("unityForbiddenAreasUnchanged").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("unityHandoffMetadataOnly").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("unityPayloadHashesMatchGoal095Ledger").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("goal095FilesDiscoveredByRelativePaths").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noUnityFilesChangedByGoal096").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("geoworldGroupPresent").GetBoolean());
        Assert.Equal(
            OfflineGeoworldBundleFixtures.SyntheticCityRadiusBundleId,
            quality.RootElement.GetProperty("geoworldOfflineBundleId").GetString());
        Assert.Equal(10, quality.RootElement.GetProperty("geoworldNormalizedFeatureCount").GetInt32());
        Assert.Equal(9, quality.RootElement.GetProperty("geoworldStreamWindowChunkCount").GetInt32());
        Assert.True(quality.RootElement.GetProperty("geoworldBoundaryPrefetchPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("geoworldTaxonomyCoveragePassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("geoworldNegativeProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("geoworldQualityGatePassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("geoworldOverviewVisible").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("goal099FilesDiscoveredByRelativePaths").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldHandoffGroupPresent").GetBoolean());
        Assert.Equal(3, quality.RootElement.GetProperty("offlineGeoworldHandoffPackageCount").GetInt32());
        Assert.Equal(10, quality.RootElement.GetProperty("offlineGeoworldHandoffFeatureCount").GetInt32());
        Assert.Equal(
            18,
            quality.RootElement.GetProperty("offlineGeoworldHandoffVisualCacheRecordCount").GetInt32());
        Assert.Equal(5, quality.RootElement.GetProperty("offlineGeoworldHandoffSourceChunkCount").GetInt32());
        Assert.Equal(9, quality.RootElement.GetProperty("offlineGeoworldHandoffStreamWindowChunkCount").GetInt32());
        Assert.Equal(5, quality.RootElement.GetProperty("offlineGeoworldHandoffUnityPayloadFileCount").GetInt32());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldHandoffSimulatedReadProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldHandoffNegativeProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldHandoffAlphaRuntimeBootstrapUnchanged").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldHandoffQualityGatePassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("goal100FilesDiscoveredByRelativePaths").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityPreviewGroupPresent").GetBoolean());
        Assert.Equal(18, quality.RootElement.GetProperty("offlineGeoworldUnityPreviewCommandCount").GetInt32());
        Assert.Equal(10, quality.RootElement.GetProperty("offlineGeoworldUnityPreviewCommandKindCount").GetInt32());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityPreviewTravelWindowStepCount").GetInt32() >= 4);
        Assert.Equal(5, quality.RootElement.GetProperty("offlineGeoworldUnityPreviewUnityPayloadFileCount").GetInt32());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityPreviewUnityScriptsReady").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityPreviewSimulatedCommandProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityPreviewNegativeProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityPreviewAlphaRuntimeBootstrapUnchanged").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityPreviewQualityGatePassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("goal101FilesDiscoveredByRelativePaths").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityEditorPreviewGroupPresent").GetBoolean());
        Assert.Equal(18, quality.RootElement.GetProperty("offlineGeoworldUnityEditorPreviewCommandCount").GetInt32());
        Assert.Equal(10, quality.RootElement.GetProperty("offlineGeoworldUnityEditorPreviewCommandKindCount").GetInt32());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityEditorPreviewTravelWindowStepCount").GetInt32() >= 4);
        Assert.Equal(18, quality.RootElement.GetProperty("offlineGeoworldUnityEditorPreviewExpectedObjectCount").GetInt32());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityEditorPreviewToolInventoryPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityEditorPreviewEditorWindowScriptReady").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityEditorPreviewSimulatedActionProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityEditorPreviewClearOperationProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityEditorPreviewNegativeProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityEditorPreviewAlphaRuntimeBootstrapUnchanged").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldUnityEditorPreviewQualityGatePassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("goal102FilesDiscoveredByRelativePaths").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldPlayModeTravelGroupPresent").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldPlayModeTravelStepCount").GetInt32() >= 4);
        Assert.Equal(18, quality.RootElement.GetProperty("offlineGeoworldPlayModeTravelObjectCount").GetInt32());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldPlayModeTravelUnityScriptsReady").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldPlayModeTravelEditorWindowReady").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldPlayModeTravelSimulatedExecutionProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldPlayModeTravelNegativeProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldPlayModeTravelGoal102BClosureRecorded").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldPlayModeTravelAlphaRuntimeBootstrapUnchanged").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldPlayModeTravelQualityGatePassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("goal103FilesDiscoveredByRelativePaths").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractiveTravelGroupPresent").GetBoolean());
        Assert.Equal(6, quality.RootElement.GetProperty("offlineGeoworldInteractiveTravelMovementSampleCount").GetInt32());
        Assert.Equal(2, quality.RootElement.GetProperty("offlineGeoworldInteractiveTravelBoundaryCrossingCount").GetInt32());
        Assert.Equal(18, quality.RootElement.GetProperty("offlineGeoworldInteractiveTravelObjectCount").GetInt32());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractiveTravelUnityScriptsReady").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractiveTravelEditorWindowReady").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractiveTravelSimulatedExecutionProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractiveTravelNegativeProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractiveTravelAlphaRuntimeBootstrapUnchanged").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractiveTravelQualityGatePassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("goal104FilesDiscoveredByRelativePaths").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractionGroupPresent").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractionTargetCount").GetInt32() >= 8);
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractionActionKindCount").GetInt32() >= 5);
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractionActionCount").GetInt32() >= 8);
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractionScriptedEventCount").GetInt32() >= 6);
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractionStateDeltaCount").GetInt32() >= 6);
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractionStateHashChainPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractionUnityScriptsReady").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractionEditorWindowReady").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractionUnitySafetyScanPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractionSimulatedSessionProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractionNegativeProofPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractionAlphaRuntimeBootstrapUnchanged").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineGeoworldInteractionQualityGatePassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("goal105FilesDiscoveredByRelativePaths").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noAbsolutePaths").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noBinaryOrRasterMediaAdded").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noRuntimeUnityProviderSchemaProjectDependencyChanges").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noPromptDumps").GetBoolean());
        Assert.True(sourceHealth.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(0, sourceHealth.RootElement.GetProperty("filesOver1000LogicalLinesCount").GetInt32());
        Assert.Equal(
            0,
            sourceHealth.RootElement.GetProperty("filesOver700LogicalLinesInGoal092NamespaceCount").GetInt32());

        var expectedPrefixes = quality.RootElement
            .GetProperty("expectedChangedPathPrefixes")
            .EnumerateArray()
            .Select(item => item.GetString() ?? string.Empty)
            .ToArray();
        Assert.DoesNotContain(expectedPrefixes, item => item.StartsWith("src/LLMGameCreator.Runtime", StringComparison.Ordinal));
        Assert.DoesNotContain(expectedPrefixes, item =>
            item.StartsWith("unity/", StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldHandoffProbe.cs",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreview",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayMode",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPlayModeTravelWindow.cs",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractiveTravelController.cs",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPlayerMotor.cs",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldBoundaryPrefetchState.cs",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractiveTravelWindow.cs",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionController.cs",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionTarget.cs",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldStateDeltaLog.cs",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractionProbeWindow.cs",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal100/",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal101/",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal103/",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal104/",
                StringComparison.Ordinal)
            && !item.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105/",
                StringComparison.Ordinal));
        Assert.DoesNotContain(expectedPrefixes, item => item.StartsWith("src/LLMGameCreator.GamePackage", StringComparison.Ordinal));
        Assert.DoesNotContain(expectedPrefixes, item => item.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));

        var goal101ArtifactRoot = Path.Combine(
            repoRoot,
            ".llmgc",
            "procedural",
            "goal-101-offline-geoworld-unity-preview-runner");
        var goal101StreamingRoot = Path.Combine(
            repoRoot,
            "unity",
            "LLMGameCreatorAlpha",
            "Assets",
            "StreamingAssets",
            "LLMGameCreator",
            "OfflineGeoworldGoal101");
        var goal102ArtifactRoot = Path.Combine(
            repoRoot,
            ".llmgc",
            "procedural",
            "goal-102-offline-geoworld-unity-editor-preview-tool");
        var goal103ArtifactRoot = Path.Combine(
            repoRoot,
            ".llmgc",
            "procedural",
            "goal-103-offline-geoworld-playmode-travel-preview");
        var goal103StreamingRoot = Path.Combine(
            repoRoot,
            "unity",
            "LLMGameCreatorAlpha",
            "Assets",
            "StreamingAssets",
            "LLMGameCreator",
            "OfflineGeoworldGoal103");
        var goal104ArtifactRoot = Path.Combine(
            repoRoot,
            ".llmgc",
            "procedural",
            "goal-104-offline-geoworld-interactive-travel-preview");
        var goal104StreamingRoot = Path.Combine(
            repoRoot,
            "unity",
            "LLMGameCreatorAlpha",
            "Assets",
            "StreamingAssets",
            "LLMGameCreator",
            "OfflineGeoworldGoal104");
        var goal105ArtifactRoot = Path.Combine(
            repoRoot,
            ".llmgc",
            "procedural",
            "goal-105-offline-geoworld-interaction-playable-probe");
        var goal105StreamingRoot = Path.Combine(
            repoRoot,
            "unity",
            "LLMGameCreatorAlpha",
            "Assets",
            "StreamingAssets",
            "LLMGameCreator",
            "OfflineGeoworldGoal105");
        var mediaFiles = new[]
            {
                goal101ArtifactRoot,
                goal101StreamingRoot,
                goal102ArtifactRoot,
                goal103ArtifactRoot,
                goal103StreamingRoot,
                goal104ArtifactRoot,
                goal104StreamingRoot,
                goal105ArtifactRoot,
                goal105StreamingRoot
            }
            .SelectMany(path => Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            .Where(path => BinaryOrRasterMediaExtensions.Contains(Path.GetExtension(path)))
            .ToList();
        Assert.Empty(mediaFiles);

        var report = first.ReportMarkdown;
        Assert.Contains("unity_handoff_inspector_probe_readiness_verification required", report);
        Assert.Contains("goal093.readback", report);
        Assert.Contains("goal095.simulated_read", report);
        Assert.Contains("cacheExportRecordCount: 93", report);
        Assert.Contains("unityPayloadFileCount: 5", report);
        Assert.Contains("noUnityFilesChangedByGoal096: true", report);
        Assert.Contains("geoworldOfflineBundleId: " + OfflineGeoworldBundleFixtures.SyntheticCityRadiusBundleId, report);
        Assert.Contains("geoworldBoundaryPrefetchPassed: true", report);
        Assert.Contains("goal099FilesDiscoveredByRelativePaths: true", report);
        Assert.Contains("offlineGeoworldHandoffPackageCount: 3", report);
        Assert.Contains("offlineGeoworldHandoffVisualCacheRecordCount: 18", report);
        Assert.Contains("offlineGeoworldHandoffQualityGatePassed: true", report);
        Assert.Contains("goal100FilesDiscoveredByRelativePaths: true", report);
        Assert.Contains("offlineGeoworldUnityPreviewCommandCount: 18", report);
        Assert.Contains("offlineGeoworldUnityPreviewUnityScriptsReady: true", report);
        Assert.Contains("offlineGeoworldUnityPreviewQualityGatePassed: true", report);
        Assert.Contains("goal101FilesDiscoveredByRelativePaths: true", report);
        Assert.Contains("offlineGeoworldUnityEditorPreviewCommandCount: 18", report);
        Assert.Contains("offlineGeoworldUnityEditorPreviewEditorWindowScriptReady: true", report);
        Assert.Contains("offlineGeoworldUnityEditorPreviewClearOperationProofPassed: true", report);
        Assert.Contains("offlineGeoworldUnityEditorPreviewQualityGatePassed: true", report);
        Assert.Contains("goal102FilesDiscoveredByRelativePaths: true", report);
        Assert.Contains("offlineGeoworldPlayModeTravelStepCount:", report);
        Assert.Contains("offlineGeoworldPlayModeTravelObjectCount: 18", report);
        Assert.Contains("offlineGeoworldPlayModeTravelUnityScriptsReady: true", report);
        Assert.Contains("offlineGeoworldPlayModeTravelGoal102BClosureRecorded: true", report);
        Assert.Contains("offlineGeoworldPlayModeTravelQualityGatePassed: true", report);
        Assert.Contains("goal103FilesDiscoveredByRelativePaths: true", report);
        Assert.Contains("offlineGeoworldInteractiveTravelMovementSampleCount: 6", report);
        Assert.Contains("offlineGeoworldInteractiveTravelBoundaryCrossingCount: 2", report);
        Assert.Contains("offlineGeoworldInteractiveTravelObjectCount: 18", report);
        Assert.Contains("offlineGeoworldInteractiveTravelUnityScriptsReady: true", report);
        Assert.Contains("offlineGeoworldInteractiveTravelEditorWindowReady: true", report);
        Assert.Contains("offlineGeoworldInteractiveTravelQualityGatePassed: true", report);
        Assert.Contains("goal104FilesDiscoveredByRelativePaths: true", report);
        Assert.Contains("offlineGeoworldInteractionTargetCount:", report);
        Assert.Contains("offlineGeoworldInteractionActionKindCount:", report);
        Assert.Contains("offlineGeoworldInteractionScriptedEventCount:", report);
        Assert.Contains("offlineGeoworldInteractionStateDeltaCount:", report);
        Assert.Contains("offlineGeoworldInteractionStateHashChainPassed: true", report);
        Assert.Contains("offlineGeoworldInteractionUnityScriptsReady: true", report);
        Assert.Contains("offlineGeoworldInteractionEditorWindowReady: true", report);
        Assert.Contains("offlineGeoworldInteractionUnitySafetyScanPassed: true", report);
        Assert.Contains("offlineGeoworldInteractionSimulatedSessionProofPassed: true", report);
        Assert.Contains("offlineGeoworldInteractionNegativeProofPassed: true", report);
        Assert.Contains("offlineGeoworldInteractionAlphaRuntimeBootstrapUnchanged: true", report);
        Assert.Contains("offlineGeoworldInteractionQualityGatePassed: true", report);
        Assert.Contains("goal105FilesDiscoveredByRelativePaths: true", report);
        Assert.DoesNotContain(repoRoot, report, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Goal092AVisualWorldPreviewServiceSplitSourceHealthProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var service = new VisualWorldPreviewServiceSplitSourceHealthEvidenceService();
        var result = service.Build(repoRoot);

        using var beforeAfter = JsonDocument.Parse(result.SourceHealthBeforeAfterJson);
        using var inventory = JsonDocument.Parse(result.RefactorFileInventoryJson);
        using var behavior = JsonDocument.Parse(result.BehaviorEquivalenceProofJson);
        using var quality = JsonDocument.Parse(result.QualityGateScanJson);

        Assert.True(beforeAfter.RootElement.GetProperty("passed").GetBoolean());
        Assert.False(beforeAfter.RootElement.GetProperty("accepted").GetBoolean());
        Assert.True(beforeAfter.RootElement
            .GetProperty("before")
            .GetProperty("oversizedWorkspaceServiceDetected")
            .GetBoolean());
        Assert.True(beforeAfter.RootElement
            .GetProperty("before")
            .GetProperty("workspaceServiceLogicalLineCount")
            .GetInt32() > 1000);
        var after = beforeAfter.RootElement.GetProperty("after");
        Assert.Equal(0, after.GetProperty("filesOver1000LogicalLinesCount").GetInt32());
        Assert.Equal(0, after.GetProperty("filesOver700LogicalLinesInGoal092NamespaceCount").GetInt32());
        Assert.Equal(0, after.GetProperty("zeroLfSourceCount").GetInt32());
        Assert.Equal(0, after.GetProperty("crOnlySourceCount").GetInt32());
        Assert.Equal(0, after.GetProperty("rawPhysicalOneLineSourceCount").GetInt32());
        Assert.True(after.GetProperty("workspaceServiceLogicalLineCount").GetInt32() < 700);

        Assert.True(inventory.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(inventory.RootElement.GetProperty("fileCount").GetInt32() >= 8);
        Assert.True(behavior.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(behavior.RootElement.GetProperty("artifactGroupCount").GetInt32() >= 5);
        Assert.True(behavior.RootElement.GetProperty("entryCount").GetInt32() >= 54);
        Assert.True(behavior.RootElement.GetProperty("svgTextPreviewCount").GetInt32() >= 38);
        Assert.True(quality.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("afterNoFilesOver1000LogicalLines").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("afterNoFilesOver700LogicalLines").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("behaviorEquivalencePassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noForbiddenAreasRequired").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noBinaryMediaArtifacts").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noPromptDumps").GetBoolean());

        var report = result.ReportMarkdown;
        Assert.Contains("visual_world_preview_service_split_source_health_verification required", report);
        Assert.DoesNotContain(repoRoot, report, StringComparison.OrdinalIgnoreCase);
        await Task.CompletedTask;
    }

    private static void AssertProofPassed(JsonElement[] proofs, string proofId)
    {
        var proof = Assert.Single(proofs, item => item.GetProperty("proofId").GetString() == proofId);
        Assert.True(proof.GetProperty("passed").GetBoolean(), proofId);
    }

    private static string ResolveProjectFolder(string repoRoot)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? repoRoot : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }
}
