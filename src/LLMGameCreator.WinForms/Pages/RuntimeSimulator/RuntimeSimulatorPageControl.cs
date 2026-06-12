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
    private readonly IUnifiedGameRuntimeService? _unifiedRuntimeService;
    private readonly IRuntimeStateSerializer? _runtimeStateSerializer;
    private UnifiedRuntimeSession? _session;

    public RuntimeSimulatorPageControl()
    {
        InitializeComponent();
    }

    public RuntimeSimulatorPageControl(
        ICurrentGamePackageService currentGamePackageService,
        IGameRuntimeService runtimeService,
        IUnifiedGameRuntimeService unifiedRuntimeService,
        IRuntimeStateSerializer runtimeStateSerializer)
    {
        _currentGamePackageService = currentGamePackageService;
        _runtimeService = runtimeService;
        _unifiedRuntimeService = unifiedRuntimeService;
        _runtimeStateSerializer = runtimeStateSerializer;
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
        _moveUpButton.Click += (_, _) => ExecuteMove(Direction2D.Up);
        _moveDownButton.Click += (_, _) => ExecuteMove(Direction2D.Down);
        _moveLeftButton.Click += (_, _) => ExecuteMove(Direction2D.Left);
        _moveRightButton.Click += (_, _) => ExecuteMove(Direction2D.Right);
        _interactButton.Click += (_, _) => ExecutePlayerCommand(PlayerCommand.Interact());
        _useItemButton.Click += (_, _) => ExecuteUseItem();
        _craftButton.Click += (_, _) => ExecuteSelected(GameRuntimeCommandType.CraftRecipe);
        _rollLootButton.Click += (_, _) => ExecuteSelected(GameRuntimeCommandType.RollLootTable);
        _transactionButton.Click += (_, _) => ExecuteSelected(GameRuntimeCommandType.ExecuteTransaction);
        _interactionButton.Click += (_, _) => ExecuteSelected(GameRuntimeCommandType.ExecuteInteraction);
        _tickButton.Click += (_, _) => ExecuteTick();
        _waitButton.Click += (_, _) => ExecuteWait();
        _refreshButton.Click += (_, _) => RefreshOptions();
    }

    private void InitializeRuntime()
    {
        var package = CurrentPackage();
        if (package == null || _unifiedRuntimeService == null)
        {
            AppendLog("No current package or unified runtime service.");
            return;
        }

        ApplyUnifiedResult(_unifiedRuntimeService.Start(package));
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
                : type == GameRuntimeCommandType.ExecuteTransaction
                    ? _transactionComboBox.Text
                    : _interactionComboBox.Text;

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

        ApplyUnifiedResult(_unifiedRuntimeService!.ExecuteGameplayCommand(package!, _session!, command));
    }

    private void ExecuteUseItem()
    {
        var package = CurrentPackage();
        if (!EnsureRuntime(package))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_itemComboBox.Text))
        {
            AppendLog("Select an item before UseItem.");
            return;
        }

        ApplyUnifiedResult(_unifiedRuntimeService!.ExecuteGameplayCommand(
            package!,
            _session!,
            GameRuntimeCommand.UseItem(_itemComboBox.Text.Trim())));
    }

    private void ExecuteMove(Direction2D direction)
    {
        ExecutePlayerCommand(PlayerCommand.Move(direction));
    }

    private void ExecutePlayerCommand(PlayerCommand command)
    {
        var package = CurrentPackage();
        if (!EnsureRuntime(package))
        {
            return;
        }

        ApplyUnifiedResult(_unifiedRuntimeService!.ExecutePlayerCommand(package!, _session!, command));
    }

    private void ExecuteTick()
    {
        var package = CurrentPackage();
        if (!EnsureRuntime(package))
        {
            return;
        }

        var ticks = (int)Math.Max(1, _ticksNumericUpDown.Value);
        ApplyUnifiedResult(_unifiedRuntimeService!.ExecuteGameplayCommand(package!, _session!, GameRuntimeCommand.TickResourceNodes(ticks)));
    }

    private void ExecuteWait()
    {
        var package = CurrentPackage();
        if (!EnsureRuntime(package))
        {
            return;
        }

        var ticks = (int)Math.Max(1, _ticksNumericUpDown.Value);
        ApplyUnifiedResult(_unifiedRuntimeService!.ExecuteGameplayCommand(package!, _session!, new GameRuntimeCommand { Type = GameRuntimeCommandType.Wait, Ticks = ticks }));
    }

    private bool EnsureRuntime(GamePackageDefinition? package)
    {
        if (package == null || _unifiedRuntimeService == null)
        {
            AppendLog("No current package or unified runtime service.");
            return false;
        }

        if (_session == null)
        {
            AppendLog("Initialize unified runtime session first.");
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
        _itemComboBox.Items.Clear();
        _interactionComboBox.Items.Clear();

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

        foreach (var item in package.Game.Items)
        {
            _itemComboBox.Items.Add(item.Id);
        }

        foreach (var interaction in package.Game.Interactions)
        {
            _interactionComboBox.Items.Add(interaction.Id);
        }

        SelectFirst(_recipeComboBox);
        SelectFirst(_lootComboBox);
        SelectFirst(_transactionComboBox);
        SelectFirst(_itemComboBox);
        SelectFirst(_interactionComboBox);
        RefreshStateJson();
    }

    private void ApplyUnifiedResult(UnifiedRuntimeResult result)
    {
        _session = result.Session;
        foreach (var runtimeEvent in result.MapEvents)
        {
            AppendLog($"map {runtimeEvent.Type}: {runtimeEvent.Message}");
        }

        foreach (var runtimeEvent in result.GameplayEvents)
        {
            AppendLog($"gameplay {runtimeEvent.Type}: {runtimeEvent.Message}");
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
        _stateTextBox.Text = _session == null
            ? string.Empty
            : _runtimeStateSerializer?.Serialize(_session) ?? JsonSerializer.Serialize(_session, JsonOptions);
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
