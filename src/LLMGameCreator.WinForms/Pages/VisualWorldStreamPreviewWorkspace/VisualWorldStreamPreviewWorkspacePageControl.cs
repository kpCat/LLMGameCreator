using System.Text.Json;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl : UserControl, IEditorPage
{
    private readonly VisualWorldStreamPreviewWorkspaceService _service;
    private VisualWorldStreamPreviewWorkspaceResult? _result;

    public VisualWorldStreamPreviewWorkspacePageControl()
        : this(new VisualWorldStreamPreviewWorkspaceService())
    {
    }

    public VisualWorldStreamPreviewWorkspacePageControl(
        VisualWorldStreamPreviewWorkspaceService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        InitializeComponent();
        ConfigureControls();
        WireEvents();
    }

    public string Id => "visual-world-stream-preview-workspace";
    public string Title => "Visual World Stream Preview";
    public int SortOrder => 38;
    Control IEditorPage.View => this;

    public void OnActivated()
    {
        RefreshWorkspace();
    }

    public void Bind(VisualWorldStreamPreviewWorkspaceResult result)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
        _statusLabel.Text = "Gate: " + result.Report.ManualGate
            + " required | accepted=false | status=" + result.Report.ImplementationStatus
            + " | groups=" + result.Catalog.GroupCount
            + " | entries=" + result.Catalog.EntryCount
            + " | svg=" + result.Catalog.SvgTextPreviewCount;
        BindGroups(result);
        BindProofs(result);
        BindDiagnostics(result);
    }

    private void ConfigureControls()
    {
        _groupsListBox.DisplayMember = nameof(GroupListItem.DisplayText);
    }

    private void WireEvents()
    {
        _refreshButton.Click += (_, _) => RefreshWorkspace();
        _groupsListBox.SelectedIndexChanged += (_, _) => SelectedGroupChanged();
        _entriesListView.SelectedIndexChanged += (_, _) => SelectedEntryChanged();
    }

    private void RefreshWorkspace()
    {
        var root = FindProjectRoot();
        if (root is null)
        {
            _statusLabel.Text = "Repository root was not found.";
            return;
        }

        try
        {
            var write = _service.BuildAndWriteAsync(root).GetAwaiter().GetResult();
            Bind(write.Result);
        }
        catch (Exception ex) when (ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or JsonException)
        {
            _statusLabel.Text = "Visual world stream preview load failed: " + ex.Message;
        }
    }

    private void BindGroups(VisualWorldStreamPreviewWorkspaceResult result)
    {
        var items = result.Catalog.Groups
            .OrderBy(group => group.GroupId, StringComparer.Ordinal)
            .Select(group => new GroupListItem(
                group.GroupId,
                group.DisplayName + " (" + group.EntryCount + ")",
                group))
            .ToList();
        _groupsListBox.DataSource = items;
        if (items.Count > 0)
        {
            _groupsListBox.SelectedIndex = 0;
        }
        else
        {
            _entriesListView.Items.Clear();
            _detailsTextBox.Clear();
            _svgPreviewTextBox.Clear();
        }
    }

    private void BindProofs(VisualWorldStreamPreviewWorkspaceResult result)
    {
        _proofsListView.BeginUpdate();
        _proofsListView.Items.Clear();
        foreach (var proof in result.ProofStatus.Proofs)
        {
            var item = new ListViewItem(proof.ProofId);
            item.SubItems.Add(proof.Passed ? "passed" : "failed");
            item.SubItems.Add(proof.RelativePath);
            item.SubItems.Add(proof.DiagnosticSummary);
            _proofsListView.Items.Add(item);
        }

        _proofsListView.EndUpdate();
    }

    private void BindDiagnostics(VisualWorldStreamPreviewWorkspaceResult result)
    {
        var lines = new List<string>
        {
            "qualityGatePassed=" + result.QualityGateScan.Passed.ToString().ToLowerInvariant(),
            "winFormsBindingPassed="
                + result.WinFormsBindingInventory.Passed.ToString().ToLowerInvariant(),
            "proofStatusPassed=" + result.ProofStatus.Passed.ToString().ToLowerInvariant(),
            "noAbsolutePaths=" + result.QualityGateScan.NoAbsolutePaths.ToString().ToLowerInvariant(),
            "noBinaryOrRasterMediaAdded="
                + result.QualityGateScan.NoBinaryOrRasterMediaAdded.ToString().ToLowerInvariant()
        };
        lines.AddRange(result.Diagnostics.Select(diagnostic =>
            diagnostic.Severity + ": " + diagnostic.Code
            + " [" + diagnostic.Target + "] " + diagnostic.Message));
        _diagnosticsTextBox.Text = string.Join(Environment.NewLine, lines);
    }

    private void SelectedGroupChanged()
    {
        if (_groupsListBox.SelectedItem is not GroupListItem selected)
        {
            return;
        }

        _entriesListView.BeginUpdate();
        _entriesListView.Items.Clear();
        foreach (var entry in selected.Group.Entries)
        {
            var item = new ListViewItem(entry.Id);
            item.SubItems.Add(entry.ArtifactKind);
            item.SubItems.Add(entry.Status.ToString());
            item.SubItems.Add(entry.RelativePath);
            item.Tag = entry;
            _entriesListView.Items.Add(item);
        }

        _entriesListView.EndUpdate();
        if (_entriesListView.Items.Count > 0)
        {
            var selectedItem = _entriesListView.Items
                .Cast<ListViewItem>()
                .FirstOrDefault(item =>
                    item.Tag is VisualWorldPreviewArtifactEntry entry
                    && !string.IsNullOrWhiteSpace(entry.TextSvgPreviewPath))
                ?? _entriesListView.Items[0];
            selectedItem.Selected = true;
            if (selectedItem.Tag is VisualWorldPreviewArtifactEntry entry)
            {
                DisplayEntry(entry);
            }
        }
    }

    private void SelectedEntryChanged()
    {
        if (_entriesListView.SelectedItems.Count == 0
            || _entriesListView.SelectedItems[0].Tag is not VisualWorldPreviewArtifactEntry entry)
        {
            return;
        }

        DisplayEntry(entry);
    }

    private void DisplayEntry(VisualWorldPreviewArtifactEntry entry)
    {
        _detailsTextBox.Text = BuildEntryDetails(entry);
        _svgPreviewTextBox.Text = string.IsNullOrWhiteSpace(entry.TextPreview)
            ? "No text SVG preview is attached to the selected entry."
            : entry.TextPreview;
    }

    private static string BuildEntryDetails(VisualWorldPreviewArtifactEntry entry)
    {
        var lines = new[]
        {
            "id: " + entry.Id,
            "kind: " + entry.ArtifactKind,
            "sourceGoal: " + entry.SourceGoalId,
            "relativePath: " + entry.RelativePath,
            "sha256: " + entry.Sha256,
            "status: " + entry.Status,
            "diagnosticSummary: " + entry.DiagnosticSummary,
            "textSvgPreviewPath: " + entry.TextSvgPreviewPath,
            "safeRatingMetadataSummary: " + entry.SafeRatingMetadataSummary
        };
        return string.Join(Environment.NewLine, lines);
    }

    private static string? FindProjectRoot()
    {
        foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var current = start;
            while (!string.IsNullOrWhiteSpace(current))
            {
                if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
                {
                    return current;
                }

                current = Directory.GetParent(current)?.FullName;
            }
        }

        return null;
    }

    private sealed record GroupListItem(
        string GroupId,
        string DisplayText,
        VisualWorldPreviewArtifactGroup Group);
}
