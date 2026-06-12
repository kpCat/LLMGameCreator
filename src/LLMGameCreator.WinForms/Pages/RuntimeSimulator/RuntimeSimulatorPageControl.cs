using System.Text.Json;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class RuntimeSimulatorPageControl : UserControl, IEditorPage
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly IGameRuntimeService? _runtimeService;
    private GameRuntimeState? _state;

    public RuntimeSimulatorPageControl()
    {
        InitializeComponent();
    }

    public RuntimeSimulatorPageControl(ICurrentGamePackageService currentGamePackageService, IGameRuntimeService runtimeService)
    {
        _currentGamePackageService = currentGamePackageService;
        _runtimeService = runtimeService;
        InitializeComponent();
        WireEvents();
    }

    public string Id => "runtime-simulator";
    public string Title => "Runtime Simulator";
    public int SortOrder => 55;
    Control IEditorPage.View => this;

    public void OnActivated()
    {
        RefreshOptions();
    }

    private void WireEvents()
    {
        _initializeButton.Click += (_, _) => InitializeRuntime();
        _craftButton.Click += (_, _) => ExecuteSelected(GameRuntimeCommandType.CraftRecipe);
        _rollLootButton.Click += (_, _) => ExecuteSelected(GameRuntimeCommandType.RollLootTable);
        _transactionButton.Click += (_, _) => ExecuteSelected(GameRuntimeCommandType.ExecuteTransaction);
        _tickButton.Click += (_, _) => ExecuteTick();
        _refreshButton.Click += (_, _) => RefreshOptions();
    }

    private void InitializeRuntime()
    {
        var package = CurrentPackage();
        if (package == null || _runtimeService == null)
        {
            AppendLog("No current package or runtime service.");
            return;
        }

        var result = _runtimeService.CreateInitialState(package);
        _state = result.State;
        ApplyResult(result);
        RefreshOptions();
    }

    private void ExecuteSelected(GameRuntimeCommandType type)
    {
        var package = CurrentPackage();
        if (!EnsureRuntime(package))
        {
            return;
        }

        var id = type == GameRuntimeCommandType.CraftRecipe
            ? _recipeComboBox.Text
            : type == GameRuntimeCommandType.RollLootTable
                ? _lootComboBox.Text
                : _transactionComboBox.Text;

        if (string.IsNullOrWhiteSpace(id))
        {
            AppendLog("Select an id before executing the command.");
            return;
        }

        int? seed = null;
        if (type == GameRuntimeCommandType.RollLootTable && int.TryParse(_seedTextBox.Text.Trim(), out var parsedSeed))
        {
            seed = parsedSeed;
        }

        var command = new GameRuntimeCommand
        {
            Type = type,
            Id = id.Trim(),
            Seed = seed
        };

        ApplyResult(_runtimeService!.Execute(package!, _state!, command));
    }

    private void ExecuteTick()
    {
        var package = CurrentPackage();
        if (!EnsureRuntime(package))
        {
            return;
        }

        var ticks = (int)Math.Max(1, _ticksNumericUpDown.Value);
        ApplyResult(_runtimeService!.Execute(package!, _state!, GameRuntimeCommand.TickResourceNodes(ticks)));
    }

    private bool EnsureRuntime(GamePackageDefinition? package)
    {
        if (package == null || _runtimeService == null)
        {
            AppendLog("No current package or runtime service.");
            return false;
        }

        if (_state == null)
        {
            AppendLog("Initialize runtime state first.");
            return false;
        }

        return true;
    }

    private GamePackageDefinition? CurrentPackage()
    {
        return _currentGamePackageService?.CurrentPackage;
    }

    private void RefreshOptions()
    {
        var package = CurrentPackage();
        _recipeComboBox.Items.Clear();
        _lootComboBox.Items.Clear();
        _transactionComboBox.Items.Clear();

        if (package == null)
        {
            return;
        }

        foreach (var recipe in package.Game.Recipes)
        {
            _recipeComboBox.Items.Add(recipe.Id);
        }

        foreach (var loot in package.Game.LootTables)
        {
            _lootComboBox.Items.Add(loot.Id);
        }

        foreach (var transaction in package.Game.Transactions)
        {
            _transactionComboBox.Items.Add(transaction.Id);
        }

        SelectFirst(_recipeComboBox);
        SelectFirst(_lootComboBox);
        SelectFirst(_transactionComboBox);
        RefreshStateJson();
    }

    private void ApplyResult(GameRuntimeResult result)
    {
        _state = result.State;
        foreach (var runtimeEvent in result.Events)
        {
            AppendLog($"event {runtimeEvent.Type}: {runtimeEvent.Message}");
        }

        foreach (var diagnostic in result.Diagnostics)
        {
            AppendLog($"{diagnostic.Severity} {diagnostic.Code}: {diagnostic.Message}");
        }

        if (!string.IsNullOrWhiteSpace(result.Message))
        {
            AppendLog(result.Message);
        }

        RefreshStateJson();
    }

    private void RefreshStateJson()
    {
        _stateTextBox.Text = _state == null ? string.Empty : JsonSerializer.Serialize(_state, JsonOptions);
    }

    private void AppendLog(string message)
    {
        _eventsTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private static void SelectFirst(ComboBox comboBox)
    {
        if (comboBox.Items.Count > 0 && comboBox.SelectedIndex < 0)
        {
            comboBox.SelectedIndex = 0;
        }
    }
}
