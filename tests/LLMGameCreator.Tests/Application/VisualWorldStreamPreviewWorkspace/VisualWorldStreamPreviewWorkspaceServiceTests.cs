using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;
using LLMGameCreator.Application.Design.OfflineGeoworldObjectiveAcceptanceRun;
using LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceServiceTests
{
    [Fact]
    public void ServiceLoadsRealGoal086Through108Artifacts()
    {
        var result = Build();
        var groupIds = result.Catalog.Groups.Select(group => group.GroupId).ToArray();

        Assert.True(result.QualityGateScan.RequiredArtifactGroupsPresent);
        Assert.Contains("microtiles", groupIds);
        Assert.Contains("map_patches", groupIds);
        Assert.Contains("region_composer", groupIds);
        Assert.Contains("world_profiles", groupIds);
        Assert.Contains("chunk_stream_windows", groupIds);
        Assert.Contains("cache_exports", groupIds);
        Assert.Contains("unity_handoff", groupIds);
        Assert.Contains("geoworld", groupIds);
        Assert.Contains("offline_geoworld_handoff", groupIds);
        Assert.Contains("offline_geoworld_unity_preview", groupIds);
        Assert.Contains("offline_geoworld_unity_editor_preview", groupIds);
        Assert.Contains("offline_geoworld_playmode_travel", groupIds);
        Assert.Contains("offline_geoworld_interactive_travel", groupIds);
        Assert.Contains("offline_geoworld_interactions", groupIds);
        Assert.Contains("offline_geoworld_session_replay", groupIds);
        Assert.Contains("offline_geoworld_objective_acceptance", groupIds);
        Assert.Contains("offline_geoworld_alpha_slice", groupIds);
        Assert.Equal(17, result.Catalog.GroupCount);
        Assert.True(result.Catalog.EntryCount >= 183);
        Assert.True(result.Catalog.SvgTextPreviewCount >= 39);
        Assert.DoesNotContain(result.Diagnostics, item => item.Severity == "error");
    }

    [Fact]
    public void CacheExportGroupSurfacesGoal093PackagesAndRuntimeHandoffSidecar()
    {
        var result = Build();
        var cacheGroup = Assert.Single(
            result.Catalog.Groups,
            group => group.GroupId == "cache_exports");
        var packages = cacheGroup.Entries
            .Where(entry => entry.ArtifactKind == "cache_export_package")
            .ToArray();

        Assert.True(result.QualityGateScan.CacheExportGroupPresent);
        Assert.Equal(4, packages.Length);
        Assert.Equal(93, packages.Sum(entry => entry.CacheRecordCount));
        Assert.Equal(117, packages.Sum(entry => entry.SourceChunkCount));
        Assert.Equal(5, packages.Sum(entry => entry.StreamWindowCount));
        Assert.Contains(packages, entry => entry.Id.EndsWith(
            "finite_custom_255x257_window_cache_export",
            StringComparison.Ordinal));
        Assert.Contains(packages, entry => entry.Id.EndsWith(
            "huge_sparse_100000x100000_window_cache_export",
            StringComparison.Ordinal));
        Assert.Contains(packages, entry => entry.Id.EndsWith(
            "infinite_streaming_overlap_cache_export",
            StringComparison.Ordinal));

        var runtimeHandoff = Assert.Single(
            packages,
            entry => entry.ExportTargetKind == "runtimeHandoff");
        Assert.EndsWith("layer_transition_runtime_handoff_sidecar", runtimeHandoff.Id);
        Assert.True(runtimeHandoff.RuntimeHandoffMetadataOnly);
        Assert.Equal(27, runtimeHandoff.CacheRecordCount);
        Assert.True(runtimeHandoff.NoRawFullWorldDump);
        Assert.True(runtimeHandoff.ReadbackProofPassed);
        Assert.True(runtimeHandoff.OverlapReuseProofPassed);
        Assert.True(runtimeHandoff.NegativeProofPassed);
        Assert.True(runtimeHandoff.InvalidationMatrixPassed);
        Assert.NotEmpty(runtimeHandoff.ChunkKeys);
        Assert.All(packages, entry =>
            Assert.False(Path.IsPathFullyQualified(entry.RelativePath), entry.RelativePath));
    }

    [Fact]
    public void UnityHandoffGroupSurfacesGoal095PayloadProbeAndReadinessProof()
    {
        var result = Build();
        var unityGroup = Assert.Single(
            result.Catalog.Groups,
            group => group.GroupId == "unity_handoff");
        var payloadFiles = unityGroup.Entries
            .Where(entry => entry.RelativePath.StartsWith(
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/VisualChunkCacheGoal095/",
                StringComparison.Ordinal))
            .ToArray();

        Assert.True(result.QualityGateScan.UnityHandoffGroupPresent);
        Assert.Equal(5, payloadFiles.Length);
        Assert.Equal(5, result.QualityGateScan.UnityPayloadFileCount);
        Assert.Equal(4, result.QualityGateScan.UnityPackageCount);
        Assert.Equal(93, result.QualityGateScan.UnityExportRecordCount);
        Assert.Equal(5, result.QualityGateScan.UnityStreamWindowCount);
        Assert.Equal(93, result.QualityGateScan.UnityUniqueChunkKeyCount);
        Assert.True(result.QualityGateScan.UnityProbeSourceInventoryVisible);
        Assert.True(result.QualityGateScan.UnityProbeSourceInventoryPassed);
        Assert.True(result.QualityGateScan.UnitySimulatedReadProofPassed);
        Assert.True(result.QualityGateScan.UnityNegativeProofPassed);
        Assert.True(result.QualityGateScan.UnityAlphaRuntimeBootstrapUnchanged);
        Assert.True(result.QualityGateScan.UnityForbiddenAreasUnchanged);
        Assert.True(result.QualityGateScan.UnityHandoffMetadataOnly);
        Assert.True(result.QualityGateScan.UnityPayloadHashesMatchGoal095Ledger);
        Assert.True(result.QualityGateScan.Goal095FilesDiscoveredByRelativePaths);
        Assert.True(result.QualityGateScan.NoUnityFilesChangedByGoal096);
        Assert.All(payloadFiles, entry =>
        {
            Assert.False(Path.IsPathFullyQualified(entry.RelativePath), entry.RelativePath);
            Assert.True(entry.PayloadHashesMatchGoal095Ledger, entry.Id);
            Assert.Equal(5, entry.PayloadFileCount);
            Assert.Equal(4, entry.PackageCount);
            Assert.Equal(93, entry.ExportRecordCount);
            Assert.Equal(5, entry.StreamWindowCount);
            Assert.Equal(93, entry.UniqueChunkKeyCount);
            Assert.True(entry.SimulatedUnityReadProofPassed);
            Assert.True(entry.NegativeProofPassed);
            Assert.True(entry.ProbeSourceInventoryPassed);
            Assert.True(entry.AlphaRuntimeBootstrapUnchanged);
        });
    }

    [Fact]
    public void GeoworldGroupSurfacesGoal099OfflineBundleGraphAndPrefetch()
    {
        var result = Build();
        var geoworldGroup = Assert.Single(
            result.Catalog.Groups,
            group => group.GroupId == "geoworld");
        var summary = Assert.Single(
            geoworldGroup.Entries,
            entry => entry.ArtifactKind == "offline_geoworld_workspace_summary");

        Assert.True(result.QualityGateScan.GeoworldGroupPresent);
        Assert.Equal(
            OfflineGeoworldBundleFixtures.SyntheticCityRadiusBundleId,
            result.QualityGateScan.GeoworldOfflineBundleId);
        Assert.Equal(10, result.QualityGateScan.GeoworldNormalizedFeatureCount);
        Assert.True(result.QualityGateScan.GeoworldWorldSourceGraphChunkCount > 0);
        Assert.Equal(9, result.QualityGateScan.GeoworldStreamWindowChunkCount);
        Assert.True(result.QualityGateScan.GeoworldBoundaryPrefetchPassed);
        Assert.True(result.QualityGateScan.GeoworldTaxonomyCoveragePassed);
        Assert.True(result.QualityGateScan.GeoworldNegativeProofPassed);
        Assert.True(result.QualityGateScan.GeoworldQualityGatePassed);
        Assert.True(result.QualityGateScan.GeoworldOverviewVisible);
        Assert.True(result.QualityGateScan.Goal099FilesDiscoveredByRelativePaths);
        Assert.Equal(OfflineGeoworldBundleFixtures.SyntheticCityRadiusBundleId, summary.OfflineBundleId);
        Assert.Equal(10, summary.GeoworldNormalizedFeatureCount);
        Assert.True(summary.GeoworldWorldSourceGraphChunkCount > 0);
        Assert.Equal(9, summary.GeoworldStreamWindowChunkCount);
        Assert.Equal("scheduled_no_network_cache_first", summary.BoundaryPrefetchStatus);
        Assert.True(summary.FeatureTaxonomyCoveragePassed);
        Assert.True(summary.GeoworldNegativeProofPassed);
        Assert.True(summary.GeoworldQualityGatePassed);
        Assert.Contains(
            geoworldGroup.Entries,
            entry => entry.ArtifactKind == "text_svg_geoworld_stream_window_overview");
        Assert.All(geoworldGroup.Entries, entry =>
            Assert.False(Path.IsPathFullyQualified(entry.RelativePath), entry.RelativePath));
    }

    [Fact]
    public void OfflineGeoworldHandoffGroupSurfacesGoal100PackagesPayloadAndProofs()
    {
        var result = Build();
        var handoffGroup = Assert.Single(
            result.Catalog.Groups,
            group => group.GroupId == "offline_geoworld_handoff");
        var summary = Assert.Single(
            handoffGroup.Entries,
            entry => entry.ArtifactKind == "offline_geoworld_handoff_workspace_summary");
        var payloadFiles = handoffGroup.Entries
            .Where(entry => entry.ArtifactKind == "offline_geoworld_streamingassets_payload")
            .ToArray();

        Assert.True(result.QualityGateScan.OfflineGeoworldHandoffGroupPresent);
        Assert.Equal(3, result.QualityGateScan.OfflineGeoworldHandoffPackageCount);
        Assert.Equal(10, result.QualityGateScan.OfflineGeoworldHandoffFeatureCount);
        Assert.Equal(18, result.QualityGateScan.OfflineGeoworldHandoffVisualCacheRecordCount);
        Assert.Equal(5, result.QualityGateScan.OfflineGeoworldHandoffSourceChunkCount);
        Assert.Equal(9, result.QualityGateScan.OfflineGeoworldHandoffStreamWindowChunkCount);
        Assert.Equal(5, result.QualityGateScan.OfflineGeoworldHandoffUnityPayloadFileCount);
        Assert.True(result.QualityGateScan.OfflineGeoworldHandoffSimulatedReadProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldHandoffNegativeProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldHandoffAlphaRuntimeBootstrapUnchanged);
        Assert.True(result.QualityGateScan.OfflineGeoworldHandoffQualityGatePassed);
        Assert.True(result.QualityGateScan.Goal100FilesDiscoveredByRelativePaths);
        Assert.Equal(5, payloadFiles.Length);
        Assert.Equal(3, summary.PackageCount);
        Assert.Equal(10, summary.GeoworldNormalizedFeatureCount);
        Assert.Equal(18, summary.GeoworldVisualCacheRecordCount);
        Assert.Equal(5, summary.GeoworldWorldSourceGraphChunkCount);
        Assert.Equal(9, summary.GeoworldStreamWindowChunkCount);
        Assert.Equal(5, summary.PayloadFileCount);
        Assert.True(summary.SimulatedUnityReadProofPassed);
        Assert.True(summary.NegativeProofPassed);
        Assert.True(summary.AlphaRuntimeBootstrapUnchanged);
        Assert.True(summary.OfflineGeoworldHandoffQualityGatePassed);
        Assert.Contains(
            "buildingFootprint=1",
            summary.OfflineGeoworldHandoffFeatureKindCountsSummary,
            StringComparison.Ordinal);
        Assert.All(handoffGroup.Entries, entry =>
            Assert.False(Path.IsPathFullyQualified(entry.RelativePath), entry.RelativePath));
    }

    [Fact]
    public void OfflineGeoworldUnityPreviewGroupSurfacesGoal101CommandsPayloadScriptsAndProofs()
    {
        var result = Build();
        var previewGroup = Assert.Single(
            result.Catalog.Groups,
            group => group.GroupId == "offline_geoworld_unity_preview");
        var summary = Assert.Single(
            previewGroup.Entries,
            entry => entry.ArtifactKind == "offline_geoworld_unity_preview_workspace_summary");
        var payloadFiles = previewGroup.Entries
            .Where(entry => entry.ArtifactKind == "offline_geoworld_unity_preview_streamingassets_payload")
            .ToArray();
        var scripts = previewGroup.Entries
            .Where(entry => entry.ArtifactKind == "offline_geoworld_unity_preview_script")
            .ToArray();

        Assert.True(result.QualityGateScan.OfflineGeoworldUnityPreviewGroupPresent);
        Assert.Equal(18, result.QualityGateScan.OfflineGeoworldUnityPreviewCommandCount);
        Assert.Equal(10, result.QualityGateScan.OfflineGeoworldUnityPreviewCommandKindCount);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityPreviewTravelWindowStepCount >= 4);
        Assert.Equal(5, result.QualityGateScan.OfflineGeoworldUnityPreviewUnityPayloadFileCount);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityPreviewUnityScriptsReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityPreviewSimulatedCommandProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityPreviewNegativeProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityPreviewAlphaRuntimeBootstrapUnchanged);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityPreviewQualityGatePassed);
        Assert.True(result.QualityGateScan.Goal101FilesDiscoveredByRelativePaths);
        Assert.Equal(5, payloadFiles.Length);
        Assert.Equal(3, scripts.Length);
        Assert.Equal(18, summary.OfflineGeoworldUnityPreviewCommandCount);
        Assert.Equal(10, summary.OfflineGeoworldUnityPreviewCommandKindCount);
        Assert.True(summary.OfflineGeoworldUnityPreviewTravelWindowStepCount >= 4);
        Assert.Contains(
            "building_footprint_marker=1",
            summary.OfflineGeoworldUnityPreviewKindCoverageSummary,
            StringComparison.Ordinal);
        Assert.True(summary.OfflineGeoworldUnityPreviewUnityScriptsReady);
        Assert.True(summary.OfflineGeoworldUnityPreviewSimulatedCommandProofPassed);
        Assert.True(summary.OfflineGeoworldUnityPreviewQualityGatePassed);
        Assert.All(previewGroup.Entries, entry =>
            Assert.False(Path.IsPathFullyQualified(entry.RelativePath), entry.RelativePath));
    }

    [Fact]
    public void SvgEntriesAreRelativeTextSafePreviewPaths()
    {
        var result = Build();

        Assert.True(result.QualityGateScan.NoAbsolutePaths);
        Assert.True(result.QualityGateScan.NoBinaryOrRasterMediaAdded);
        Assert.All(result.Catalog.SvgEntries, entry =>
        {
            Assert.False(Path.IsPathFullyQualified(entry.RelativePath), entry.RelativePath);
            Assert.EndsWith(".svg", entry.RelativePath);
            Assert.True(entry.SafeToDisplayAsText, entry.RelativePath);
            Assert.Contains("<svg", entry.PreviewText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("<script", entry.PreviewText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("http://", entry.PreviewText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("https://", entry.PreviewText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("base64", entry.PreviewText, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void Goal091Goal093Goal095Goal099Goal100Goal101Goal102AndGoal103ProofStatusesSurfaceRequiredProofs()
    {
        var result = Build();
        var required = new[]
        {
            "goal091.seam",
            "goal091.cache_reuse",
            "goal091.layer_transition",
            "goal091.negative",
            "goal093.readback",
            "goal093.overlap_reuse",
            "goal093.negative",
            "goal093.invalidation_matrix",
            "goal093.runtime_handoff_metadata_only",
            "goal095.streamingassets_ledger",
            "goal095.simulated_read",
            "goal095.negative",
            "goal095.probe_source_inventory",
            "goal095.alpha_runtime_bootstrap_unchanged",
            "goal095.forbidden_unity_areas_unchanged",
            "goal095.metadata_only",
            "goal099.boundary_prefetch",
            "goal099.negative",
            "goal099.visual_projection",
            "goal099.quality_gate",
            "goal100.streamingassets_ledger",
            "goal100.simulated_read",
            "goal100.negative",
            "goal100.probe_source_inventory",
            "goal100.alpha_runtime_bootstrap_unchanged",
            "goal100.visual_cache_records",
            "goal100.all_feature_kinds_mapped",
            "goal100.workspace_binding",
            "goal100.quality_gate",
            "goal101.streamingassets_ledger",
            "goal101.unity_script_inventory",
            "goal101.simulated_command",
            "goal101.negative",
            "goal101.alpha_runtime_bootstrap_unchanged",
            "goal101.all_command_kinds_mapped",
            "goal101.travel_window_demo",
            "goal101.quality_gate",
            "goal102.tool_inventory",
            "goal102.editor_window_menu",
            "goal102.simulated_action",
            "goal102.clear_operation",
            "goal102.negative",
            "goal102.alpha_runtime_bootstrap_unchanged",
            "goal102.quality_gate",
            "goal103.unity_script_inventory",
            "goal103.editor_window_inventory",
            "goal103.simulated_execution",
            "goal103.negative",
            "goal103.goal102b_closure",
            "goal103.alpha_runtime_bootstrap_unchanged",
            "goal103.boundary_prefetch",
            "goal103.quality_gate",
            "goal106.unity_script_inventory",
            "goal106.editor_window_inventory",
            "goal106.simulated_save_load_replay",
            "goal106.negative",
            "goal106.alpha_runtime_bootstrap_unchanged",
            "goal106.checkpoint_resume",
            "goal106.final_hash",
            "goal106.quality_gate",
            "goal107.unity_script_inventory",
            "goal107.editor_window_inventory",
            "goal107.replay_acceptance",
            "goal107.negative",
            "goal107.checkpoint_resume",
            "goal107.completion_transitions",
            "goal107.alpha_quality_consolidation",
            "goal107.alpha_runtime_bootstrap_unchanged",
            "goal107.quality_gate",
            "goal108.alpha_slice.unity_script_inventory",
            "goal108.alpha_slice.editor_window_inventory",
            "goal108.alpha_slice.full_slice_simulated_proof",
            "goal108.alpha_slice.negative_proof",
            "goal108.alpha_slice.alpha_runtime_bootstrap_unchanged",
            "goal108.alpha_slice.quality_gate"
        };

        Assert.True(result.ProofStatus.Passed);
        foreach (var proofId in required)
        {
            var proof = Assert.Single(result.ProofStatus.Proofs, item => item.ProofId == proofId);
            Assert.True(proof.Passed, proof.DiagnosticSummary);
            Assert.False(Path.IsPathFullyQualified(proof.RelativePath), proof.RelativePath);
            Assert.False(string.IsNullOrWhiteSpace(proof.Sha256), proof.ProofId);
        }
    }

    [Fact]
    public void Goal091StreamWindowSvgEntriesAreVisibleInCatalog()
    {
        var result = Build();
        var streamGroup = Assert.Single(
            result.Catalog.Groups,
            group => group.GroupId == "chunk_stream_windows");

        Assert.True(result.QualityGateScan.Goal091StreamWindowsVisible);
        Assert.Equal(4, result.QualityGateScan.Goal091StreamWindowEntryCount);
        Assert.Contains(streamGroup.Entries, entry =>
            entry.Id.Contains("infinite_streaming_multilayer_window", StringComparison.Ordinal));
        Assert.All(
            streamGroup.Entries.Where(entry => entry.ArtifactKind == "text_svg_chunk_stream_window_overview"),
            entry => Assert.False(Path.IsPathFullyQualified(entry.RelativePath), entry.RelativePath));
    }

    [Fact]
    public void WorkspaceQualityGateRecordsSourceHealthBackstop()
    {
        var result = Build();

        Assert.True(result.QualityGateScan.SourceHealthPassed);
        Assert.True(result.QualityGateScan.ScannedCSharpFileCount >= 5);
        Assert.Equal(0, result.QualityGateScan.FilesOver1000LogicalLinesCount);
        Assert.Equal(0, result.QualityGateScan.FilesOver700LogicalLinesInGoal092NamespaceCount);
        Assert.Equal(0, result.QualityGateScan.ZeroLfSourceCount);
        Assert.Equal(0, result.QualityGateScan.CrOnlySourceCount);
        Assert.Equal(0, result.QualityGateScan.RawPhysicalOneLineSourceCount);
        Assert.Equal(0, result.QualityGateScan.MinifiedSourceCount);
        Assert.True(result.QualityGateScan.WorkspaceServiceLogicalLineCount < 700);
        Assert.Equal(0, result.QualityGateScan.FilesOver1000LogicalLinesCount);
        Assert.All(result.SourceHealthScan.Files, file =>
            Assert.True(file.LogicalLineCount <= 700, file.RelativePath));
    }

    [Fact]
    public void SourceHealthScannerRejectsSyntheticOver1000LineSource()
    {
        var lines = Enumerable.Range(0, 1002)
            .Select(index => "public sealed class Synthetic" + index + " { }");
        var text = "namespace Synthetic;" + Environment.NewLine + string.Join(Environment.NewLine, lines);
        var bytes = Encoding.UTF8.GetBytes(text);

        var scan = VisualWorldStreamPreviewSourceHealthScanner.AnalyzeSourceBytes("synthetic/Oversized.cs", bytes);

        Assert.True(scan.FileOver1000LogicalLines);
        Assert.True(VisualWorldStreamPreviewSourceHealthScanner.RejectsOver1000LogicalLines(bytes));
    }

    [Fact]
    public void SourceHealthScannerRejectsZeroLfCrOnlyAndMinifiedSamples()
    {
        Assert.True(VisualWorldStreamPreviewSourceHealthScanner.RejectsSuspiciousRawSourceBytes(
            Encoding.UTF8.GetBytes("public sealed class Broken { public string Value => \""
                                   + new string('x', 520)
                                   + "\"; }")));
        Assert.True(VisualWorldStreamPreviewSourceHealthScanner.RejectsSuspiciousRawSourceBytes(
            Encoding.UTF8.GetBytes("public sealed class Broken\r{\r}\r")));
        Assert.True(VisualWorldStreamPreviewSourceHealthScanner.RejectsSuspiciousRawSourceBytes(
            Encoding.UTF8.GetBytes("namespace Broken; public sealed class OneLine { }")));
    }

    [Fact]
    public void CurrentGoal092NamespaceHasNoFilesOver1000LogicalLines()
    {
        var scan = VisualWorldStreamPreviewSourceHealthScanner.ScanGoal092Namespace(ProjectRoot());

        Assert.True(scan.Passed);
        Assert.Equal(0, scan.FilesOver1000LogicalLinesCount);
        Assert.Equal(0, scan.FilesOver700LogicalLinesInGoal092NamespaceCount);
        Assert.All(scan.Files, file => Assert.True(file.LogicalLineCount <= 700, file.RelativePath));
        Assert.True(scan.WorkspaceServiceLogicalLineCount < 700);
    }

    [Fact]
    public void MissingArtifactScenarioProducesDiagnosticsNotFakeGreen()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "llmgc-goal092-missing-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(root);
            var result = new VisualWorldStreamPreviewWorkspaceService().Build(root);

            Assert.False(result.QualityGateScan.Passed);
            Assert.Contains(result.Diagnostics, item => item.Severity == "error");
            Assert.Contains(result.Catalog.Groups, group => group.Status == VisualWorldPreviewArtifactStatus.Failed);
            Assert.False(result.ProofStatus.Passed);
            Assert.False(result.Report.QualityGatePassed);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void WinFormsWorkspaceCanBindApplicationResult()
    {
        var result = Build();
        RunSta(() =>
        {
            using var page = new VisualWorldStreamPreviewWorkspacePageControl();

            page.Bind(result);

            var stored = RequiredPrivateField<VisualWorldStreamPreviewWorkspaceResult>(page, "_result");
            var groups = RequiredPrivateField<ListBox>(page, "_groupsListBox");
            var entries = RequiredPrivateField<ListView>(page, "_entriesListView");
            var proofs = RequiredPrivateField<ListView>(page, "_proofsListView");
            var svgPreview = RequiredPrivateField<TextBox>(page, "_svgPreviewTextBox");
            var details = RequiredPrivateField<TextBox>(page, "_detailsTextBox");

            Assert.Same(result, stored);
            Assert.True(groups.Items.Count >= 6);
            Assert.True(entries.Items.Count > 0);
            Assert.True(proofs.Items.Count >= 32);
            groups.SelectedItem = groups.Items
                .Cast<object>()
                .First(item => item.ToString()!.Contains(
                    "GroupId = unity_handoff,",
                    StringComparison.Ordinal));
            var unityPayloadItem = entries.Items
                .Cast<ListViewItem>()
                .First(item => item.Tag is VisualWorldPreviewArtifactEntry entry
                               && entry.ArtifactKind.StartsWith(
                                   "streamingassets_payload_",
                                   StringComparison.Ordinal));
            unityPayloadItem.Selected = true;
            unityPayloadItem.Focused = true;

            Assert.Contains("payloadFileCount: 5", details.Text, StringComparison.Ordinal);
            Assert.Contains("packageCount: 4", details.Text, StringComparison.Ordinal);
            Assert.Contains("exportRecordCount: 93", details.Text, StringComparison.Ordinal);
            Assert.Contains("uniqueChunkKeyCount: 93", details.Text, StringComparison.Ordinal);
            Assert.Contains("simulatedUnityReadProofPassed: true", details.Text, StringComparison.Ordinal);
            Assert.Contains("alphaRuntimeBootstrapUnchanged: true", details.Text, StringComparison.Ordinal);
            groups.SelectedItem = groups.Items
                .Cast<object>()
                .First(item => item.ToString()!.Contains(
                    "GroupId = geoworld,",
                    StringComparison.Ordinal));
            var geoworldItem = entries.Items
                .Cast<ListViewItem>()
                .First(item => item.Tag is VisualWorldPreviewArtifactEntry entry
                               && entry.ArtifactKind == "offline_geoworld_workspace_summary");
            var geoworldEntry = Assert.IsType<VisualWorldPreviewArtifactEntry>(geoworldItem.Tag);
            var geoworldDetails = InvokePrivateStatic<string>(
                typeof(VisualWorldStreamPreviewWorkspacePageControl),
                "BuildEntryDetails",
                geoworldEntry);

            Assert.Contains(
                "offlineBundleId: " + OfflineGeoworldBundleFixtures.SyntheticCityRadiusBundleId,
                geoworldDetails,
                StringComparison.Ordinal);
            Assert.Contains("geoworldNormalizedFeatureCount: 10", geoworldDetails, StringComparison.Ordinal);
            Assert.Contains("geoworldStreamWindowChunkCount: 9", geoworldDetails, StringComparison.Ordinal);
            Assert.Contains(
                "boundaryPrefetchStatus: scheduled_no_network_cache_first",
                geoworldDetails,
                StringComparison.Ordinal);
            Assert.Contains("geoworldNegativeProofPassed: true", geoworldDetails, StringComparison.Ordinal);
            groups.SelectedItem = groups.Items
                .Cast<object>()
                .First(item => item.ToString()!.Contains(
                    "GroupId = offline_geoworld_handoff,",
                    StringComparison.Ordinal));
            var handoffItem = entries.Items
                .Cast<ListViewItem>()
                .First(item => item.Tag is VisualWorldPreviewArtifactEntry entry
                               && entry.ArtifactKind == "offline_geoworld_handoff_workspace_summary");
            var handoffEntry = Assert.IsType<VisualWorldPreviewArtifactEntry>(handoffItem.Tag);
            var handoffDetails = InvokePrivateStatic<string>(
                typeof(VisualWorldStreamPreviewWorkspacePageControl),
                "BuildEntryDetails",
                handoffEntry);

            Assert.Contains("packageCount: 3", handoffDetails, StringComparison.Ordinal);
            Assert.Contains("geoworldNormalizedFeatureCount: 10", handoffDetails, StringComparison.Ordinal);
            Assert.Contains("geoworldVisualCacheRecordCount: 18", handoffDetails, StringComparison.Ordinal);
            Assert.Contains(
                "offlineGeoworldHandoffQualityGatePassed: true",
                handoffDetails,
                StringComparison.Ordinal);
            groups.SelectedItem = groups.Items
                .Cast<object>()
                .First(item => item.ToString()!.Contains(
                    "GroupId = offline_geoworld_unity_preview,",
                    StringComparison.Ordinal));
            var previewItem = entries.Items
                .Cast<ListViewItem>()
                .First(item => item.Tag is VisualWorldPreviewArtifactEntry entry
                               && entry.ArtifactKind
                                   == "offline_geoworld_unity_preview_workspace_summary");
            var previewEntry = Assert.IsType<VisualWorldPreviewArtifactEntry>(previewItem.Tag);
            var previewDetails = InvokePrivateStatic<string>(
                typeof(VisualWorldStreamPreviewWorkspacePageControl),
                "BuildEntryDetails",
                previewEntry);

            Assert.Contains("offlineGeoworldUnityPreviewCommandCount: 18", previewDetails, StringComparison.Ordinal);
            Assert.Contains("offlineGeoworldUnityPreviewCommandKindCount: 10", previewDetails, StringComparison.Ordinal);
            Assert.Contains("offlineGeoworldUnityPreviewUnityScriptsReady: true", previewDetails, StringComparison.Ordinal);
            Assert.Contains(
                "offlineGeoworldUnityPreviewQualityGatePassed: true",
                previewDetails,
                StringComparison.Ordinal);
            groups.SelectedItem = groups.Items
                .Cast<object>()
                .First(item => item.ToString()!.Contains(
                    "GroupId = offline_geoworld_unity_editor_preview,",
                    StringComparison.Ordinal));
            var editorItem = entries.Items
                .Cast<ListViewItem>()
                .First(item => item.Tag is VisualWorldPreviewArtifactEntry entry
                               && entry.ArtifactKind
                                   == "offline_geoworld_unity_editor_preview_workspace_summary");
            var editorEntry = Assert.IsType<VisualWorldPreviewArtifactEntry>(editorItem.Tag);
            var editorDetails = InvokePrivateStatic<string>(
                typeof(VisualWorldStreamPreviewWorkspacePageControl),
                "BuildEntryDetails",
                editorEntry);

            Assert.Contains("offlineGeoworldUnityEditorPreviewCommandCount: 18", editorDetails, StringComparison.Ordinal);
            Assert.Contains("offlineGeoworldUnityEditorPreviewExpectedObjectCount: 18", editorDetails, StringComparison.Ordinal);
            Assert.Contains(
                "offlineGeoworldUnityEditorPreviewEditorWindowScriptReady: true",
                editorDetails,
                StringComparison.Ordinal);
            Assert.Contains(
                "offlineGeoworldUnityEditorPreviewClearOperationProofPassed: true",
                editorDetails,
                StringComparison.Ordinal);
            groups.SelectedItem = groups.Items
                .Cast<object>()
                .First(item => item.ToString()!.Contains(
                    "GroupId = offline_geoworld_interactions,",
                    StringComparison.Ordinal));
            var interactionItem = entries.Items
                .Cast<ListViewItem>()
                .First(item => item.Tag is VisualWorldPreviewArtifactEntry entry
                               && entry.ArtifactKind
                                   == "offline_geoworld_interaction_workspace_summary");
            var interactionEntry = Assert.IsType<VisualWorldPreviewArtifactEntry>(interactionItem.Tag);
            var interactionDetails = InvokePrivateStatic<string>(
                typeof(VisualWorldStreamPreviewWorkspacePageControl),
                "BuildEntryDetails",
                interactionEntry);

            Assert.Contains("offlineGeoworldInteractionTargetCount: ", interactionDetails, StringComparison.Ordinal);
            Assert.Contains(
                "offlineGeoworldInteractionStateHashChainPassed: true",
                interactionDetails,
                StringComparison.Ordinal);
            Assert.Contains(
                "offlineGeoworldInteractionUnitySafetyScanPassed: true",
                interactionDetails,
                StringComparison.Ordinal);
            Assert.True(
                svgPreview.Text.Contains("<svg", StringComparison.OrdinalIgnoreCase)
                || svgPreview.Text.Contains("No text SVG preview", StringComparison.Ordinal));
        });
    }

    [Fact]
    public void WorkspaceActivationLoadsAndBindsRealArtifacts()
    {
        RunSta(() =>
        {
            using var page = new VisualWorldStreamPreviewWorkspacePageControl();

            page.OnActivated();

            var result = RequiredPrivateField<VisualWorldStreamPreviewWorkspaceResult>(page, "_result");
            Assert.Equal("GREEN", result.Report.ImplementationStatus);
            Assert.False(result.Report.Accepted);
            Assert.True(result.QualityGateScan.Passed);
            Assert.True(result.WinFormsBindingInventory.Passed);
        });
    }

    private static VisualWorldStreamPreviewWorkspaceResult Build()
    {
        var root = ProjectRoot();
        new OfflineGeoworldObjectiveAcceptanceRunEvidenceService()
            .BuildAndWriteAsync(root)
            .GetAwaiter()
            .GetResult();
        new OfflineGeoworldAlphaSliceOrchestratorEvidenceService()
            .BuildAndWriteAsync(root)
            .GetAwaiter()
            .GetResult();
        return new VisualWorldStreamPreviewWorkspaceService().Build(root);
    }

    private static void RunSta(Action action)
    {
        Exception? caught = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                caught = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (caught is not null)
        {
            ExceptionDispatchInfo.Capture(caught).Throw();
        }
    }

    private static T RequiredPrivateField<T>(object owner, string fieldName)
    {
        var field = owner.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(field);
        var value = field!.GetValue(owner);
        Assert.NotNull(value);
        return Assert.IsType<T>(value);
    }

    private static T InvokePrivateStatic<T>(Type ownerType, string methodName, params object[] arguments)
    {
        var method = ownerType.GetMethod(
            methodName,
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(method);
        var value = method!.Invoke(null, arguments);
        Assert.NotNull(value);
        return Assert.IsType<T>(value);
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
