using LLMGameCreator.Application.Design;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratorLibraryPlansTabControl : UserControl
{
    private IGeneratorPlanDraftService? _draftService;
    private IGeneratorPlanRepository? _planRepository;
    private IReadOnlyList<GeneratorPlanRecord> _plans = Array.Empty<GeneratorPlanRecord>();

    public GeneratorLibraryPlansTabControl()
    {
        InitializeComponent();
        _createButton.Click += async (_, _) => await CreateDraftPlanAsync();
        _refreshButton.Click += async (_, _) => await RefreshPlansAsync();
        _plansListView.SelectedIndexChanged += async (_, _) => await ShowSelectedPlanAsync();
    }

    public void Configure(IGeneratorPlanDraftService draftService, IGeneratorPlanRepository planRepository)
    {
        _draftService = draftService;
        _planRepository = planRepository;
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

    private static string? EmptyAsNull(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
