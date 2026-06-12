using LLMGameCreator.Application.Design;
using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratorLibraryArtifactsTabControl : UserControl
{
    private IGeneratedArtifactRepository? _artifactRepository;
    private IGamePackagePatchService? _patchService;
    private IReadOnlyList<GeneratedArtifactRecord> _artifacts = Array.Empty<GeneratedArtifactRecord>();

    public GeneratorLibraryArtifactsTabControl()
    {
        InitializeComponent();
        WireEvents();
        UpdateActionButtons();
    }

    public void Configure(IGeneratedArtifactRepository artifactRepository, IGamePackagePatchService patchService)
    {
        _artifactRepository = artifactRepository;
        _patchService = patchService;
    }

    public async Task RefreshArtifactsAsync()
    {
        if (_artifactRepository == null)
        {
            SetStatus("Runtime services are not available.");
            return;
        }

        _artifacts = await _artifactRepository.ListGeneratedArtifactsAsync(CancellationToken.None).ConfigureAwait(true);
        _artifactsListView.Items.Clear();
        foreach (var artifact in _artifacts)
        {
            var item = new ListViewItem(artifact.Kind);
            item.SubItems.Add(artifact.ValidationState);
            item.SubItems.Add(artifact.GeneratedBy);
            item.SubItems.Add(artifact.Id);
            item.Tag = artifact;
            _artifactsListView.Items.Add(item);
        }

        ClearDetails();
        SetStatus(_artifacts.Count == 0 ? "No generated artifacts." : $"Loaded {_artifacts.Count} generated artifacts.");
        UpdateActionButtons();
    }

    private void WireEvents()
    {
        _refreshButton.Click += async (_, _) => await RefreshArtifactsAsync();
        _createPatchButton.Click += async (_, _) => await CreatePatchFromPreviewAsync();
        _dryRunButton.Click += async (_, _) => await DryRunPatchAsync();
        _applyButton.Click += async (_, _) => await ApplyPatchAsync();
        _artifactsListView.SelectedIndexChanged += async (_, _) => await ShowSelectedArtifactAsync();
    }

    private async Task ShowSelectedArtifactAsync()
    {
        var artifact = GetSelectedArtifact();
        if (artifact == null)
        {
            ClearDetails();
            UpdateActionButtons();
            return;
        }

        _kindValueLabel.Text = artifact.Kind;
        _pathValueLabel.Text = artifact.Path;
        _generatedByValueLabel.Text = artifact.GeneratedBy;
        _validationStateValueLabel.Text = artifact.ValidationState;
        _jsonTextBox.Text = artifact.Json;
        _metadataTextBox.Text = artifact.MetadataJson;

        if (_artifactRepository != null)
        {
            var results = await _artifactRepository.ListValidationResultsByArtifactAsync(artifact.Id, CancellationToken.None).ConfigureAwait(true);
            _validationTextBox.Text = FormatValidationResults(results);
        }

        UpdateActionButtons();
    }

    private async Task CreatePatchFromPreviewAsync()
    {
        var artifact = GetSelectedArtifact();
        if (artifact == null || _patchService == null)
        {
            SetStatus("Select a preview artifact first.");
            return;
        }

        try
        {
            var result = await _patchService.CreatePatchArtifactFromPreviewAsync(artifact.Id, CancellationToken.None).ConfigureAwait(true);
            _resultTextBox.Text = FormatCreateResult(result);
            SetStatus(result.Message);
            await RefreshArtifactsAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private async Task DryRunPatchAsync()
    {
        var artifact = GetSelectedArtifact();
        if (artifact == null || _patchService == null)
        {
            SetStatus("Select a patch artifact first.");
            return;
        }

        try
        {
            var result = await _patchService.DryRunPatchArtifactAsync(artifact.Id, CancellationToken.None).ConfigureAwait(true);
            _resultTextBox.Text = FormatDryRunResult(result);
            SetStatus(result.Message);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private async Task ApplyPatchAsync()
    {
        var artifact = GetSelectedArtifact();
        if (artifact == null || _patchService == null)
        {
            SetStatus("Select a patch artifact first.");
            return;
        }

        try
        {
            var result = await _patchService.ApplyPatchArtifactAsync(artifact.Id, CancellationToken.None).ConfigureAwait(true);
            _resultTextBox.Text = FormatApplyResult(result);
            SetStatus(result.Message);
            await RefreshArtifactsAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private GeneratedArtifactRecord? GetSelectedArtifact()
    {
        return _artifactsListView.SelectedItems.Count == 0
            ? null
            : _artifactsListView.SelectedItems[0].Tag as GeneratedArtifactRecord;
    }

    private void UpdateActionButtons()
    {
        var artifact = GetSelectedArtifact();
        var isPreview = artifact?.Kind.Equals("generator_plan_preview", StringComparison.OrdinalIgnoreCase) == true;
        var isPatch = artifact?.Kind.Equals(GamePackagePatchArtifactKinds.PatchV1, StringComparison.OrdinalIgnoreCase) == true;
        _createPatchButton.Enabled = isPreview;
        _dryRunButton.Enabled = isPatch;
        _applyButton.Enabled = isPatch;
    }

    private void ClearDetails()
    {
        _kindValueLabel.Text = "-";
        _pathValueLabel.Text = "-";
        _generatedByValueLabel.Text = "-";
        _validationStateValueLabel.Text = "-";
        _jsonTextBox.Text = string.Empty;
        _metadataTextBox.Text = string.Empty;
        _validationTextBox.Text = string.Empty;
        _resultTextBox.Text = string.Empty;
    }

    private void SetStatus(string status)
    {
        _statusLabel.Text = status;
    }

    private static string FormatCreateResult(GamePackagePatchCreateResult result)
    {
        var lines = new List<string>
        {
            result.Message,
            $"Saved: {result.Saved}"
        };

        if (result.PatchArtifact != null)
        {
            lines.Add($"Patch artifact: {result.PatchArtifact.Id}");
            lines.Add($"Validation state: {result.PatchArtifact.ValidationState}");
        }

        lines.Add(string.Empty);
        lines.AddRange(result.ValidationResults.Select(FormatValidationResult));
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatDryRunResult(GamePackagePatchDryRunResult result)
    {
        var lines = new List<string>
        {
            result.Message,
            $"Can apply: {result.CanApply}",
            string.Empty,
            "Diff:"
        };
        lines.AddRange(result.DiffLines.Select(line => $"{line.ChangeKind} {line.Operation} {line.Target}: {line.Message}"));
        lines.Add(string.Empty);
        lines.Add("Validation:");
        lines.AddRange(result.ValidationIssues.Select(FormatValidationIssue));
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatApplyResult(GamePackagePatchApplyResult result)
    {
        var lines = new List<string>
        {
            result.Message,
            $"Applied: {result.Applied}",
            $"Backup: {result.BackupPath ?? "-"}"
        };

        if (result.AuditArtifact != null)
        {
            lines.Add($"Audit artifact: {result.AuditArtifact.Id}");
        }

        lines.Add(string.Empty);
        lines.Add("Diff:");
        lines.AddRange(result.DiffLines.Select(line => $"{line.ChangeKind} {line.Operation} {line.Target}: {line.Message}"));
        lines.Add(string.Empty);
        lines.Add("Validation:");
        lines.AddRange(result.ValidationIssues.Select(FormatValidationIssue));
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatValidationResults(IReadOnlyList<GeneratedArtifactValidationResultRecord> results)
    {
        return results.Count == 0
            ? "No validation results."
            : string.Join(Environment.NewLine, results.Select(FormatValidationResult));
    }

    private static string FormatValidationResult(GeneratedArtifactValidationResultRecord result)
    {
        return $"{result.Severity} {result.Code}: {result.Message} ({result.Target})";
    }

    private static string FormatValidationIssue(ValidationIssue issue)
    {
        return $"{issue.Severity} {issue.Code}: {issue.Message} ({issue.TargetId})";
    }
}

