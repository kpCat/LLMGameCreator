using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceServiceTests
{
    [Fact]
    public void ServiceLoadsRealGoal086Through093Artifacts()
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
        Assert.Equal(6, result.Catalog.GroupCount);
        Assert.True(result.Catalog.EntryCount >= 67);
        Assert.True(result.Catalog.SvgTextPreviewCount >= 38);
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
    public void Goal091AndGoal093ProofStatusesSurfaceRequiredProofs()
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
            "goal093.runtime_handoff_metadata_only"
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
            Assert.True(proofs.Items.Count >= 9);
            var cachePackageItem = entries.Items
                .Cast<ListViewItem>()
                .First(item => item.Tag is VisualWorldPreviewArtifactEntry entry
                               && entry.ArtifactKind == "cache_export_package");
            cachePackageItem.Selected = true;
            cachePackageItem.Focused = true;

            Assert.Contains("cacheRecordCount:", details.Text, StringComparison.Ordinal);
            Assert.Contains("runtimeHandoffMetadataOnly:", details.Text, StringComparison.Ordinal);
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

    private static VisualWorldStreamPreviewWorkspaceResult Build() =>
        new VisualWorldStreamPreviewWorkspaceService().Build(ProjectRoot());

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
