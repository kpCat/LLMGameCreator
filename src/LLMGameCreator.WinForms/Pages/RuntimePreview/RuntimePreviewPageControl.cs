using LLMGameCreator.Application.Projects;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.WinForms.Pages;

public sealed class RuntimePreviewPageControl : UserControl, IEditorPage
{
    private readonly ICurrentGamePackageService _currentGamePackageService;
    private readonly IGameRuntime _runtime;
    private readonly RuntimeMapCanvas _canvas = new RuntimeMapCanvas();
    private readonly TextBox _logTextBox = new TextBox();
    private GameState? _state;

    public RuntimePreviewPageControl(ICurrentGamePackageService currentGamePackageService, IGameRuntime runtime)
    {
        _currentGamePackageService = currentGamePackageService;
        _runtime = runtime;
        BuildLayout();
    }

    public string Id => "runtime-preview";
    public string Title => "Runtime Preview";
    public int SortOrder => 50;
    public Control View => this;

    public void OnActivated()
    {
        _canvas.Focus();
    }

    private void BuildLayout()
    {
        var root = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 820 };
        var left = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42 };
        var startButton = new Button { Text = "Старт", Width = 100, Height = 30 };
        toolbar.Controls.Add(startButton);

        _canvas.Dock = DockStyle.Fill;
        _canvas.CommandRequested += command => ExecuteCommand(command);
        left.Controls.Add(_canvas);
        left.Controls.Add(toolbar);

        _logTextBox.Dock = DockStyle.Fill;
        _logTextBox.Multiline = true;
        _logTextBox.ReadOnly = true;
        _logTextBox.ScrollBars = ScrollBars.Vertical;

        root.Panel1.Controls.Add(left);
        root.Panel2.Controls.Add(_logTextBox);
        Controls.Add(root);

        startButton.Click += (_, _) => StartRuntime();
    }

    private void StartRuntime()
    {
        var package = _currentGamePackageService.CurrentPackage;
        if (package == null)
        {
            AppendLog("Проект игры не открыт.");
            return;
        }

        var result = _runtime.Start(package);
        _state = result.State;
        ApplyResult(package, result);
    }

    private void ExecuteCommand(PlayerCommand command)
    {
        var package = _currentGamePackageService.CurrentPackage;
        if (package == null || _state == null)
        {
            AppendLog("Сначала запусти runtime.");
            return;
        }

        var result = _runtime.Execute(package, _state, command);
        _state = result.State;
        ApplyResult(package, result);
    }

    private void ApplyResult(GamePackageDefinition package, CommandResult result)
    {
        foreach (var runtimeEvent in result.Events)
        {
            AppendLog(runtimeEvent.Message);
        }

        _canvas.SetState(package, result.State);
    }

    private void AppendLog(string message)
    {
        _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }
}
