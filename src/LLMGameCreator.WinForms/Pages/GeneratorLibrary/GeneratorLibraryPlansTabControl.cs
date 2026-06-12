using LLMGameCreator.Application.Design;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratorLibraryPlansTabControl : UserControl
{
    private IGeneratorPlanDraftService? _draftService;
    private IGeneratorPlanRepository? _planRepository;
    private IGeneratorPlanReviewService? _reviewService;
    private IGeneratorPlanPreviewService? _previewService;
    private IReadOnlyList<GeneratorPlanRecord> _plans = Array.Empty<GeneratorPlanRecord>();

    public GeneratorLibraryPlansTabControl()
    {
        InitializeComponent();
        _createButton.Click += async (_, _) => await CreateDraftPlanAsync();
        _refreshButton.Click += async (_, _) => await RefreshPlansAsync();
        _revalidateButton.Click += async (_, _) => await RevalidateSelectedPlanAsync();
        _createPreviewButton.Click += async (_, _) => await CreatePreviewArtifactAsync();
        _approveButton.Click += async (_, _) => await ApproveSelectedPlanAsync();
        _rejectButton.Click += async (_, _) => await RejectSelectedPlanAsync();
        _archiveButton.Click += async (_, _) => await ArchiveSelectedPlanAsync();
        _plansListView.SelectedIndexChanged += async (_, _) => await ShowSelectedPlanAsync();
        UpdateActionButtons();
    }

    public void Configure(
        IGeneratorPlanDraftService draftService,
        IGeneratorPlanRepository planRepository,
        IGeneratorPlanReviewService reviewService,
        IGeneratorPlanPreviewService previewService)
    {
        _draftService = draftService;
        _planRepository = planRepository;
        _reviewService = reviewService;
        _previewService = previewService;
    }

    public async Task RefreshPlansAsync()
    {
        if (_planRepository == null)
        {
            SetStatus("Runtime services are not available.");
            return;
        }

        _plans = await _planRepository.ListGeneratorPlansAsync(CancellationToken.None).ConfigureAwait(true);
        _plansListView.Items.Clear();
        foreach (var plan in _plans)
        {
            var item = new ListViewItem(plan.Title);
            item.SubItems.Add(plan.Status);
            item.SubItems.Add(plan.Goal);
            item.SubItems.Add(plan.UpdatedUtc.ToLocalTime().ToString("g"));
            item.Tag = plan;
            _plansListView.Items.Add(item);
        }

        _stepsListView.Items.Clear();
        _issuesTextBox.Text = string.Empty;
        _previewArtifactTextBox.Text = string.Empty;
        UpdateActionButtons();
        SetStatus(_plans.Count == 0 ? "No saved draft plans." : $"Loaded {_plans.Count} draft plans.");
    }

    private async Task CreateDraftPlanAsync()
    {
        if (_draftService == null)
        {
            SetStatus("Runtime services are not available.");
            return;
        }

        try
        {
            SetStatus("Creating draft plan...");
            var result = await _draftService.CreateDraftPlanAsync(new GeneratorPlanDraftRequest(
                _titleTextBox.Text,
                _goalTextBox.Text,
                _briefTextBox.Text,
                EmptyAsNull(_runtimeTargetTextBox.Text),
                EmptyAsNull(_turnModeTextBox.Text),
                EmptyAsNull(_combatModeTextBox.Text),
                1800), CancellationToken.None).ConfigureAwait(true);

            _rawResponseTextBox.Text = result.RawLlmResponse;
            _issuesTextBox.Text = FormatIssues(result.ValidationIssues);
            SetStatus(result.Saved
                ? $"Draft plan saved: {result.Plan?.Title}"
                : "Draft plan was not saved because validation failed.");

            await RefreshPlansAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private async Task ShowSelectedPlanAsync()
    {
        if (_planRepository == null || _plansListView.SelectedItems.Count == 0 || _plansListView.SelectedItems[0].Tag is not GeneratorPlanRecord plan)
        {
            UpdateActionButtons();
            return;
        }

        var steps = await _planRepository.GetGeneratorPlanStepsAsync(plan.Id, CancellationToken.None).ConfigureAwait(true);
        _stepsListView.Items.Clear();
        foreach (var step in steps)
        {
            var item = new ListViewItem(step.StepOrder.ToString());
            item.SubItems.Add(step.ModuleId);
            item.SubItems.Add(step.Status);
            item.SubItems.Add(step.DependsOnJson);
            _stepsListView.Items.Add(item);
        }

        _issuesTextBox.Text =
            $"Id: {plan.Id}\r\n" +
            $"Title: {plan.Title}\r\n" +
            $"Goal: {plan.Goal}\r\n" +
            $"Status: {plan.Status}\r\n" +
            $"Metadata: {plan.MetadataJson}";
        _previewArtifactTextBox.Text = string.Empty;
        UpdateActionButtons();
    }

    private async Task RevalidateSelectedPlanAsync()
    {
        var plan = GetSelectedPlan();
        if (plan == null || _reviewService == null)
        {
            SetStatus("Select a plan first.");
            return;
        }

        try
        {
            var result = await _reviewService.RevalidatePlanAsync(plan.Id, CancellationToken.None).ConfigureAwait(true);
            _issuesTextBox.Text = FormatReviewResult(result);
            SetStatus(result.CanApprove ? "Plan is valid for approval." : "Plan has validation errors.");
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private async Task ApproveSelectedPlanAsync()
    {
        await UpdateSelectedPlanStatusAsync((service, plan, cancellationToken) => service.ApprovePlanAsync(plan.Id, "Approved from Plans tab.", cancellationToken)).ConfigureAwait(true);
    }

    private async Task CreatePreviewArtifactAsync()
    {
        var plan = GetSelectedPlan();
        if (plan == null || _previewService == null)
        {
            SetStatus("Select a plan first.");
            return;
        }

        try
        {
            var result = await _previewService.CreatePreviewArtifactAsync(new GeneratorPlanPreviewRequest(plan.Id), CancellationToken.None).ConfigureAwait(true);
            _issuesTextBox.Text = FormatPreviewResult(result);
            _previewArtifactTextBox.Text = result.Artifact?.Json ?? string.Empty;
            SetStatus(result.Message);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private async Task RejectSelectedPlanAsync()
    {
        await UpdateSelectedPlanStatusAsync((service, plan, cancellationToken) => service.RejectPlanAsync(plan.Id, "Rejected from Plans tab.", cancellationToken)).ConfigureAwait(true);
    }

    private async Task ArchiveSelectedPlanAsync()
    {
        await UpdateSelectedPlanStatusAsync((service, plan, cancellationToken) => service.ArchivePlanAsync(plan.Id, "Archived from Plans tab.", cancellationToken)).ConfigureAwait(true);
    }

    private async Task UpdateSelectedPlanStatusAsync(Func<IGeneratorPlanReviewService, GeneratorPlanRecord, CancellationToken, Task<GeneratorPlanStatusUpdateResult>> action)
    {
        var plan = GetSelectedPlan();
        if (plan == null || _reviewService == null)
        {
            SetStatus("Select a plan first.");
            return;
        }

        try
        {
            var result = await action(_reviewService, plan, CancellationToken.None).ConfigureAwait(true);
            var details = FormatStatusUpdateResult(result);
            var message = result.Message;
            await RefreshPlansAsync().ConfigureAwait(true);
            _issuesTextBox.Text = details;
            SetStatus(message);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private GeneratorPlanRecord? GetSelectedPlan()
    {
        return _plansListView.SelectedItems.Count == 0
            ? null
            : _plansListView.SelectedItems[0].Tag as GeneratorPlanRecord;
    }

    private void UpdateActionButtons()
    {
        var hasSelection = GetSelectedPlan() != null;
        _revalidateButton.Enabled = hasSelection;
        _approveButton.Enabled = hasSelection;
        _rejectButton.Enabled = hasSelection;
        _archiveButton.Enabled = hasSelection;
        _createPreviewButton.Enabled = hasSelection;
    }

    private void SetStatus(string status)
    {
        _statusLabel.Text = status;
    }

    private static string FormatIssues(IReadOnlyList<GeneratorPlanValidationIssue> issues)
    {
        if (issues.Count == 0)
        {
            return "No validation issues.";
        }

        return string.Join(Environment.NewLine, issues.Select(issue => $"{issue.Severity} {issue.Code}: {issue.Message} ({issue.Target})"));
    }

    private static string FormatReviewResult(GeneratorPlanReviewResult result)
    {
        var lines = new List<string>();
        if (result.Plan != null)
        {
            lines.Add($"Id: {result.Plan.Id}");
            lines.Add($"Title: {result.Plan.Title}");
            lines.Add($"Status: {result.Plan.Status}");
            lines.Add($"Can approve: {result.CanApprove}");
            lines.Add(string.Empty);
        }

        lines.Add(FormatIssues(result.ValidationIssues));
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatStatusUpdateResult(GeneratorPlanStatusUpdateResult result)
    {
        var lines = new List<string>
        {
            result.Message,
            $"Updated: {result.Updated}"
        };

        if (result.Plan != null)
        {
            lines.Add($"Status: {result.Plan.Status}");
        }

        lines.Add(string.Empty);
        lines.Add(FormatIssues(result.ValidationIssues));
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatPreviewResult(GeneratorPlanPreviewResult result)
    {
        var lines = new List<string>
        {
            result.Message,
            $"Saved: {result.Saved}"
        };

        if (result.Artifact != null)
        {
            lines.Add($"Artifact: {result.Artifact.Id}");
            lines.Add($"Validation state: {result.Artifact.ValidationState}");
        }

        lines.Add(string.Empty);
        if (result.ValidationResults.Count == 0)
        {
            lines.Add("No preview validation results.");
        }
        else
        {
            lines.AddRange(result.ValidationResults.Select(resultItem => $"{resultItem.Severity} {resultItem.Code}: {resultItem.Message} ({resultItem.Target})"));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string? EmptyAsNull(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
