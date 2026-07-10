using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.SelectedRuntimeVariantInteractiveSession;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using ApplicationLiveSessionService = LLMGameCreator.Application.Design.SelectedRuntimeVariantInteractiveSession.SelectedRuntimeVariantInteractiveSessionService;
using RuntimeLiveSessionService = LLMGameCreator.Runtime.SelectedRuntimeVariantInteractiveSessionService;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private readonly SelectedRuntimeVariantInteractiveSessionController _goal144Controller =
        new(RuntimeLiveSessionService.CreateDefault());
    private readonly SelectedRuntimeVariantInteractiveSessionOperatorRunner _goal144OperatorRunner =
        new(new ApplicationLiveSessionService(RuntimeLiveSessionService.CreateDefault()));
    private TabPage? _goal144Tab;
    private TextBox? _goal144Status;
    private TextBox? _goal144Summaries;
    private TextBox? _goal144LastResult;
    private ComboBox? _goal144Actions;
    private readonly List<Button> _goal144Buttons = [];

    private void ConfigureGoal144LiveSessionPanel()
    {
        _goal144Tab = new TabPage { Name = "_goal144Tab", Text = "Goal144 Live Session" };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(8)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "Goal144 Runtime-owned interactive session",
            Font = new Font(Font, FontStyle.Bold)
        }, 0, 0);
        _goal144Status = Goal132ReadOnlyTextBox(multiline: true);
        _goal144Actions = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _goal144Summaries = Goal132ReadOnlyTextBox(multiline: true);
        _goal144LastResult = Goal132ReadOnlyTextBox(multiline: true);
        layout.Controls.Add(_goal144Status, 0, 1);
        layout.Controls.Add(_goal144Actions, 0, 2);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
        foreach (var caption in new[]
                 {
                     "Start / Reset Session", "Execute Selected Action", "Save Checkpoint",
                     "Reload Checkpoint", "Replay Verify", "Run Selected Variant Session Drill"
                 })
        {
            var button = Goal132Button(caption);
            _goal144Buttons.Add(button);
            buttons.Controls.Add(button);
        }

        layout.Controls.Add(buttons, 0, 3);
        layout.Controls.Add(_goal144Summaries, 0, 4);
        layout.Controls.Add(_goal144LastResult, 0, 5);
        _goal144Tab.Controls.Add(layout);
        _detailTabs.TabPages.Add(_goal144Tab);
    }

    private void WireGoal144LiveSessionEvents()
    {
        if (_goal144Buttons.Count != 6) return;
        _goal144Buttons[0].Click += async (_, _) => await Goal144RunAsync(() =>
        {
            var root = FindProjectRoot() ?? throw new InvalidOperationException("Repository root was not found.");
            _goal144Controller.StartOrReset(root);
            return "session reset";
        });
        _goal144Buttons[1].Click += async (_, _) => await Goal144RunAsync(() =>
        {
            var action = _goal144Actions?.SelectedItem as string
                         ?? throw new InvalidOperationException("Select an available action.");
            var result = _goal144Controller.ExecuteSelected(action);
            return result.Status + " " + result.ActionId;
        });
        _goal144Buttons[2].Click += async (_, _) => await Goal144RunAsync(() =>
            "checkpoint=" + _goal144Controller.SaveCheckpoint().CheckpointId);
        _goal144Buttons[3].Click += async (_, _) => await Goal144RunAsync(() =>
            "reloadPassed=" + _goal144Controller.ReloadCheckpoint().Passed);
        _goal144Buttons[4].Click += async (_, _) => await Goal144RunAsync(() =>
            "replayPassed=" + _goal144Controller.ReplayVerify().Passed);
        _goal144Buttons[5].Click += async (_, _) => await Goal144RunAsync(() =>
        {
            var root = FindProjectRoot() ?? throw new InvalidOperationException("Repository root was not found.");
            var write = _goal144OperatorRunner.RunAsync(root).GetAwaiter().GetResult();
            return "operatorUsesInProcessService=true status=" + write.Artifacts.Dashboard.Status;
        });
    }

    private void BindGoal144LiveSession()
    {
        if (_goal144Status is null || _goal144Summaries is null || _goal144Actions is null) return;
        var session = _goal144Controller.Session;
        if (session is null)
        {
            _goal144Status.Text = "Candidate: minimal-map-game-exploration-resource-focus\r\n"
                                  + "Variant: exploration_resource_focus\r\n"
                                  + "Runtime gameplay truth: true\r\nSession: not started";
            _goal144Actions.DataSource = Array.Empty<string>();
            return;
        }

        _goal144Status.Text = string.Join(Environment.NewLine,
        [
            "candidate=" + session.CandidateId,
            "variant=" + session.VariantKind,
            "packageSha256=" + session.PackageSha256,
            "sessionId=" + session.SessionId,
            "currentActionIndex=" + session.CurrentActionIndex,
            "runtimeCommandCount=" + session.RuntimeCommandExecutionCount,
            "currentStateHash=" + session.CurrentStateHash,
            "runtimeStarted=" + session.RuntimeStarted.ToString().ToLowerInvariant(),
            "completed=" + session.Completed.ToString().ToLowerInvariant(),
            "checkpoint=" + (_goal144Controller.Checkpoint?.CheckpointId ?? "none")
        ]);
        _goal144Summaries.Text = string.Join(Environment.NewLine,
        [
            "map=" + session.LatestMapSummary,
            "inventory=" + session.LatestInventorySummary,
            "quest=" + session.LatestQuestSummary,
            "combat=" + session.LatestCombatSummary
        ]);
        _goal144Actions.DataSource = session.AvailableActions
            .Where(action => action.Available)
            .Select(action => action.ActionId)
            .ToList();
    }

    private async Task Goal144RunAsync(Func<string> operation)
    {
        Goal144SetRunning(true);
        try
        {
            var message = await Task.Run(operation);
            if (_goal144LastResult is not null) _goal144LastResult.Text = Goal144Tail(message);
            BindGoal144LiveSession();
        }
        catch (Exception ex)
        {
            if (_goal144LastResult is not null) _goal144LastResult.Text = Goal144Tail("failed: " + ex.Message);
        }
        finally
        {
            Goal144SetRunning(false);
        }
    }

    private void Goal144SetRunning(bool running)
    {
        foreach (var button in _goal144Buttons) button.Enabled = !running;
        if (_goal144Actions is not null) _goal144Actions.Enabled = !running;
    }

    private static string Goal144Tail(string text) => string.Join(
        Environment.NewLine,
        text.Replace("\r\n", "\n").Split('\n').TakeLast(80));
}
