using LLMGameCreator.Application.Design.ProductLineInteractiveSessionMatrix;
using LLMGameCreator.Runtime;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private readonly ProductLineInteractiveSessionSelectionController _goal145Controller =
        new(SelectedRuntimeVariantInteractiveSessionService.CreateDefault());
    private readonly ProductLineInteractiveSessionMatrixOperatorRunner _goal145OperatorRunner =
        new(new ProductLineInteractiveSessionMatrixService(SelectedRuntimeVariantInteractiveSessionService.CreateDefault()));
    private TabPage? _goal145Tab;
    private ComboBox? _goal145Candidates;
    private ComboBox? _goal145Actions;
    private TextBox? _goal145Status;
    private TextBox? _goal145Comparison;
    private TextBox? _goal145LastResult;
    private readonly List<Button> _goal145Buttons = [];
    private bool _goal145BindingCandidateList;
    private int _goal145SelectionCallbackDepth;
    private int _goal145MaximumSelectionCallbackDepth;
    private int _goal145OperatorCommitSelectionCount;

    private void ConfigureGoal145VariantSessionsPanel()
    {
        _goal145Tab = new TabPage { Name = "_goal145Tab", Text = "Goal145 Variant Sessions" };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            Padding = new Padding(8)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "Goal145 operator-selectable Runtime sessions",
            Font = new Font(Font, FontStyle.Bold)
        }, 0, 0);
        _goal145Candidates = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _goal145Actions = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _goal145Status = Goal132ReadOnlyTextBox(multiline: true);
        _goal145Comparison = Goal132ReadOnlyTextBox(multiline: true);
        _goal145LastResult = Goal132ReadOnlyTextBox(multiline: true);
        layout.Controls.Add(_goal145Candidates, 0, 1);
        layout.Controls.Add(_goal145Actions, 0, 2);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
        foreach (var caption in new[]
                 {
                     "Load Candidate Matrix", "Start Selected Variant", "Execute Selected Action",
                     "Save Checkpoint", "Reload Checkpoint", "Replay Verify", "Run All Variant Sessions"
                 })
        {
            var button = Goal132Button(caption);
            _goal145Buttons.Add(button);
            buttons.Controls.Add(button);
        }

        layout.Controls.Add(buttons, 0, 3);
        layout.Controls.Add(_goal145Status, 0, 4);
        layout.Controls.Add(_goal145Comparison, 0, 5);
        layout.Controls.Add(_goal145LastResult, 0, 6);
        _goal145Tab.Controls.Add(layout);
        _detailTabs.TabPages.Add(_goal145Tab);
    }

    private void WireGoal145VariantSessionsEvents()
    {
        if (_goal145Buttons.Count != 7) return;
        _goal145Buttons[0].Click += async (_, _) => await Goal145RunAsync(() =>
        {
            var root = FindProjectRoot() ?? throw new InvalidOperationException("Repository root was not found.");
            var candidates = _goal145Controller.LoadCandidateMatrix(root);
            return "candidateCount=" + candidates.Count;
        });
        if (_goal145Candidates is not null)
        {
            _goal145Candidates.SelectionChangeCommitted += (_, _) => Goal145CandidateSelectionCommitted();
        }

        _goal145Buttons[1].Click += async (_, _) => await Goal145RunAsync(() =>
            "session=" + _goal145Controller.StartSelected().SessionId);
        _goal145Buttons[2].Click += async (_, _) => await Goal145RunAsync(() =>
        {
            var action = _goal145Actions?.SelectedValue as string
                         ?? throw new InvalidOperationException("Select an available action.");
            var result = _goal145Controller.ExecuteSelectedAction(action);
            return result.Status + " " + result.ActionId;
        });
        _goal145Buttons[3].Click += async (_, _) => await Goal145RunAsync(() =>
            "checkpoint=" + _goal145Controller.SaveCheckpoint().CheckpointId);
        _goal145Buttons[4].Click += async (_, _) => await Goal145RunAsync(() =>
            "reloadPassed=" + _goal145Controller.ReloadCheckpoint().Passed);
        _goal145Buttons[5].Click += async (_, _) => await Goal145RunAsync(() =>
            "replayPassed=" + _goal145Controller.ReplayVerify().Passed);
        _goal145Buttons[6].Click += async (_, _) => await Goal145RunAsync(() =>
        {
            var root = FindProjectRoot() ?? throw new InvalidOperationException("Repository root was not found.");
            var result = _goal145OperatorRunner.RunAsync(root).GetAwaiter().GetResult();
            return "operatorUsesInProcessService=true status=" + result.Artifacts.Dashboard.Status;
        });
    }

    private void BindGoal145VariantSessions()
    {
        if (_goal145Candidates is null || _goal145Actions is null || _goal145Status is null) return;
        var selectedId = _goal145Controller.SelectedCandidateId;
        var candidateRows = _goal145Controller.Candidates.Select(candidate => new KeyValuePair<string, string>(
            candidate.CandidateId,
            candidate.CandidateId + " | variant=" + candidate.VariantKind + " | score=" + candidate.Score
            + " | sha=" + candidate.PackageSha256 + " | pass=" + candidate.Passed.ToString().ToLowerInvariant())).ToList();
        _goal145BindingCandidateList = true;
        try
        {
            _goal145Candidates.DisplayMember = "Value";
            _goal145Candidates.ValueMember = "Key";
            _goal145Candidates.DataSource = candidateRows;
            if (!string.IsNullOrWhiteSpace(selectedId)) _goal145Candidates.SelectedValue = selectedId;
        }
        finally
        {
            _goal145BindingCandidateList = false;
        }

        var session = _goal145Controller.Session;
        _goal145Status.Text = session is null
            ? "selectedCandidate=" + (selectedId.Length == 0 ? "not loaded" : selectedId) + "\r\nsession=not started"
            : string.Join(Environment.NewLine,
            [
                "selectedCandidate=" + session.CandidateId,
                "variant=" + session.VariantKind,
                "packageSha256=" + session.PackageSha256,
                "currentActionIndex=" + session.CurrentActionIndex,
                "runtimeCommandCount=" + session.RuntimeCommandExecutionCount,
                "currentStateHash=" + session.CurrentStateHash,
                "checkpoint=" + (_goal145Controller.Checkpoint?.CheckpointId ?? "none"),
                "inventory=" + session.LatestInventorySummary,
                "quest=" + session.LatestQuestSummary,
                "combat=" + session.LatestCombatSummary
            ]);
        _goal145Actions.DisplayMember = "Value";
        _goal145Actions.ValueMember = "Key";
        _goal145Actions.DataSource = session?.AvailableActions.Where(action => action.Available)
            .Select(action => new KeyValuePair<string, string>(
                action.ActionId,
                action.ActionId + " | target=" + action.TargetId + " | step="
                + (string.IsNullOrWhiteSpace(action.CanonicalStepId) ? "presentation_only" : action.CanonicalStepId)
                + " | route=" + action.Route)).ToList() ?? [];
        BindGoal145Comparison();
    }

    private void Goal145CandidateSelectionCommitted()
    {
        if (_goal145BindingCandidateList || _goal145Candidates?.SelectedValue is not string id
            || string.IsNullOrWhiteSpace(id) || id == _goal145Controller.SelectedCandidateId)
        {
            return;
        }

        _goal145SelectionCallbackDepth++;
        _goal145MaximumSelectionCallbackDepth = Math.Max(
            _goal145MaximumSelectionCallbackDepth,
            _goal145SelectionCallbackDepth);
        try
        {
            _goal145OperatorCommitSelectionCount++;
            _goal145Controller.SelectCandidate(id);
            BindGoal145VariantSessions();
        }
        finally
        {
            _goal145SelectionCallbackDepth--;
        }
    }

    private void BindGoal145Comparison()
    {
        if (_goal145Comparison is null) return;
        var root = FindProjectRoot();
        var path = root is null ? string.Empty : Path.Combine(
            root,
            ProductLineInteractiveSessionMatrixVocabulary.ProceduralRoot.Replace('/', Path.DirectorySeparatorChar),
            "product-line-interactive-session-comparison.json");
        _goal145Comparison.Text = path.Length > 0 && File.Exists(path)
            ? File.ReadAllText(path)
            : "Fresh focus comparison: run all variant sessions.";
    }

    private async Task Goal145RunAsync(Func<string> operation)
    {
        Goal145SetRunning(true);
        try
        {
            var message = await Task.Run(operation);
            if (_goal145LastResult is not null) _goal145LastResult.Text = Goal144Tail(message);
            BindGoal145VariantSessions();
        }
        catch (Exception ex)
        {
            if (_goal145LastResult is not null) _goal145LastResult.Text = Goal144Tail("failed: " + ex.Message);
        }
        finally
        {
            Goal145SetRunning(false);
        }
    }

    private void Goal145SetRunning(bool running)
    {
        foreach (var button in _goal145Buttons) button.Enabled = !running;
        if (_goal145Candidates is not null) _goal145Candidates.Enabled = !running;
        if (_goal145Actions is not null) _goal145Actions.Enabled = !running;
    }
}
