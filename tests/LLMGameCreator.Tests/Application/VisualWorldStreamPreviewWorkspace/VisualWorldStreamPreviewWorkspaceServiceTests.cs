using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceServiceTests
{
    [Fact]
    public void ServiceLoadsRealGoal086Through091Artifacts()
    {
        var result = Build();
        var groupIds = result.Catalog.Groups.Select(group => group.GroupId).ToArray();

        Assert.True(result.QualityGateScan.RequiredArtifactGroupsPresent);
        Assert.Equal(5, result.Catalog.GroupCount);
        Assert.Contains("microtiles", groupIds);
        Assert.Contains("map_patches", groupIds);
        Assert.Contains("region_composer", groupIds);
        Assert.Contains("world_profiles", groupIds);
        Assert.Contains("chunk_stream_windows", groupIds);
        Assert.True(result.Catalog.EntryCount >= 25);
        Assert.True(result.Catalog.SvgTextPreviewCount >= 4);
        Assert.DoesNotContain(result.Diagnostics, item => item.Severity == "error");
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
    public void Goal091ProofStatusesSurfaceSeamCacheLayerAndNegativeProofs()
    {
        var result = Build();
        var required = new[]
        {
            "goal091.seam",
            "goal091.cache_reuse",
            "goal091.layer_transition",
            "goal091.negative"
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

            Assert.Same(result, stored);
            Assert.True(groups.Items.Count >= 5);
            Assert.True(entries.Items.Count > 0);
            Assert.True(proofs.Items.Count >= 4);
            Assert.Contains("<svg", svgPreview.Text, StringComparison.OrdinalIgnoreCase);
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
