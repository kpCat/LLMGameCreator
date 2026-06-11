using LLMGameCreator.Application.Projects;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class RuntimePreviewPageControl : UserControl, IEditorPage
{
    private readonly ICurrentGamePackageService _currentGamePackageService;
    private readonly IGameRuntime _runtime;
    private GameState? _state;

    public RuntimePreviewPageControl(ICurrentGamePackageService currentGamePackageService, IGameRuntime runtime)
    {
        _currentGamePackageService = currentGamePackageService;
        _runtime = runtime;
        InitializeComponent();

        _startButton.Click += (_, _) => StartRuntime();
        _canvas.CommandRequested += command => ExecuteCommand(command);
    }

    public string Id => "runtime-preview";
    public string Title => "Runtime Preview";
    public int SortOrder => 50;
    public Control View => this;

    public void OnActivated()
    {
        _canvas.Focus();
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
