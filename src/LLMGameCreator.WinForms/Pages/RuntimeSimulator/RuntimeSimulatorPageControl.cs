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
    private readonly IRuntimeSnapshotStore? _runtimeSnapshotStore;
    private UnifiedRuntimeSession? _session;

    public RuntimeSimulatorPageControl()
    {
        InitializeComponent();
    }

    public RuntimeSimulatorPageControl(
        ICurrentGamePackageService currentGamePackageService,
        IGameRuntimeService runtimeService,
        IUnifiedGameRuntimeService unifiedRuntimeService,
        IRuntimeStateSerializer runtimeStateSerializer,
        IRuntimeSnapshotStore runtimeSnapshotStore)
    {
        _currentGamePackageService = currentGamePackageService;
        _runtimeService = runtimeService;
        _unifiedRuntimeService = unifiedRuntimeService;
        _runtimeStateSerializer = runtimeStateSerializer;
        _runtimeSnapshotStore = runtimeSnapshotStore;
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
        _equipButton.Click += (_, _) => ExecuteEquip();
        _unequipButton.Click += (_, _) => ExecuteUnequip();
        _openContainerButton.Click += (_, _) => ExecuteContainer(GameRuntimeCommandType.OpenContainer);
        _takeContainerButton.Click += (_, _) => ExecuteContainer(GameRuntimeCommandType.TakeFromContainer);
        _depositContainerButton.Click += (_, _) => ExecuteContainer(GameRuntimeCommandType.DepositToContainer);
        _harvestButton.Click += (_, _) => ExecuteHarvest();
        _saveSnapshotButton.Click += (_, _) => SaveSnapshot();
        _loadSnapshotButton.Click += (_, _) => LoadSnapshot();
        _listSnapshotsButton.Click += (_, _) => ListSnapshots();
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

    private void ExecuteEquip()
    {
        var package = CurrentPackage();
        if (!EnsureRuntime(package))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_itemComboBox.Text) || string.IsNullOrWhiteSpace(_equipmentSlotComboBox.Text))
        {
            AppendLog("Select item and equipment slot before EquipItem.");
            return;
        }

        ApplyUnifiedResult(_unifiedRuntimeService!.ExecuteGameplayCommand(
            package!,
            _session!,
            GameRuntimeCommand.EquipItem(_itemComboBox.Text.Trim(), _equipmentSlotComboBox.Text.Trim())));
    }

    private void ExecuteUnequip()
    {
        var package = CurrentPackage();
        if (!EnsureRuntime(package))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_equipmentSlotComboBox.Text))
        {
            AppendLog("Select equipment slot before UnequipItem.");
            return;
        }

        ApplyUnifiedResult(_unifiedRuntimeService!.ExecuteGameplayCommand(
            package!,
            _session!,
            GameRuntimeCommand.UnequipItem(_equipmentSlotComboBox.Text.Trim())));
    }

    private void ExecuteContainer(GameRuntimeCommandType type)
    {
        var package = CurrentPackage();
        if (!EnsureRuntime(package))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_containerComboBox.Text))
        {
            AppendLog("Select container inventory id first.");
            return;
        }

        GameRuntimeCommand command;
        if (type == GameRuntimeCommandType.OpenContainer)
        {
            command = GameRuntimeCommand.OpenContainer(_containerComboBox.Text.Trim());
        }
        else
        {
            if (string.IsNullOrWhiteSpace(_containerItemTextBox.Text))
            {
                AppendLog("Enter item id for container transfer.");
                return;
            }

            var amount = (double)Math.Max(1, _containerAmountNumericUpDown.Value);
            command = type == GameRuntimeCommandType.TakeFromContainer
                ? GameRuntimeCommand.TakeFromContainer(_containerComboBox.Text.Trim(), _containerItemTextBox.Text.Trim(), amount)
                : GameRuntimeCommand.DepositToContainer(_containerComboBox.Text.Trim(), _containerItemTextBox.Text.Trim(), amount);
        }

        ApplyUnifiedResult(_unifiedRuntimeService!.ExecuteGameplayCommand(package!, _session!, command));
    }

    private void ExecuteHarvest()
    {
        var package = CurrentPackage();
        if (!EnsureRuntime(package))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_resourceNodeComboBox.Text))
        {
            AppendLog("Select resource node before HarvestResourceNode.");
            return;
        }

        int? seed = int.TryParse(_seedTextBox.Text.Trim(), out var parsedSeed) ? parsedSeed : null;
        var toolItemId = string.IsNullOrWhiteSpace(_toolItemTextBox.Text) ? null : _toolItemTextBox.Text.Trim();
        ApplyUnifiedResult(_unifiedRuntimeService!.ExecuteGameplayCommand(
            package!,
            _session!,
            GameRuntimeCommand.HarvestResourceNode(_resourceNodeComboBox.Text.Trim(), toolItemId: toolItemId, seed: seed)));
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
        _equipmentSlotComboBox.Items.Clear();
        _containerComboBox.Items.Clear();
        _resourceNodeComboBox.Items.Clear();

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

        foreach (var slot in package.Game.EquipmentSlots)
        {
            _equipmentSlotComboBox.Items.Add(slot.Id);
        }

        foreach (var inventory in package.Game.Inventories.Where(inventory =>
            inventory.OwnerKind.Equals("container", StringComparison.OrdinalIgnoreCase)
            || inventory.Tags.Any(tag => tag.Equals("container", StringComparison.OrdinalIgnoreCase))
            || inventory.Metadata.TryGetValue("container", out var value) && value.Equals("true", StringComparison.OrdinalIgnoreCase)))
        {
            _containerComboBox.Items.Add(inventory.Id);
        }

        foreach (var node in package.Game.ResourceNodes)
        {
            _resourceNodeComboBox.Items.Add(node.Id);
        }

        SelectFirst(_recipeComboBox);
        SelectFirst(_lootComboBox);
        SelectFirst(_transactionComboBox);
        SelectFirst(_itemComboBox);
        SelectFirst(_interactionComboBox);
        SelectFirst(_equipmentSlotComboBox);
        SelectFirst(_containerComboBox);
        SelectFirst(_resourceNodeComboBox);
        RefreshStateJson();
    }

    private void SaveSnapshot()
    {
        if (!EnsureSnapshotReady())
        {
            return;
        }

        var result = _runtimeSnapshotStore!.SaveSnapshot(_currentGamePackageService!.CurrentFolder!, _snapshotSlotTextBox.Text.Trim(), _session!);
        ApplySnapshotResult(result, replaceSession: false);
    }

    private void LoadSnapshot()
    {
        if (_runtimeSnapshotStore == null || _currentGamePackageService == null || string.IsNullOrWhiteSpace(_currentGamePackageService.CurrentFolder))
        {
            AppendLog("No current project folder or snapshot store.");
            return;
        }

        var result = _runtimeSnapshotStore.LoadSnapshot(_currentGamePackageService.CurrentFolder, _snapshotSlotTextBox.Text.Trim());
        ApplySnapshotResult(result, replaceSession: true);
    }

    private void ListSnapshots()
    {
        if (_runtimeSnapshotStore == null || _currentGamePackageService == null || string.IsNullOrWhiteSpace(_currentGamePackageService.CurrentFolder))
        {
            AppendLog("No current project folder or snapshot store.");
            return;
        }

        var result = _runtimeSnapshotStore.ListSnapshots(_currentGamePackageService.CurrentFolder);
        AppendLog(result.Success ? $"Snapshots: {string.Join(", ", result.SlotNames)}" : result.Message);
        foreach (var diagnostic in result.Diagnostics)
        {
            AppendLog($"{diagnostic.Severity} {diagnostic.Code}: {diagnostic.Message}");
        }
    }

    private bool EnsureSnapshotReady()
    {
        if (_runtimeSnapshotStore == null || _currentGamePackageService == null || string.IsNullOrWhiteSpace(_currentGamePackageService.CurrentFolder))
        {
            AppendLog("No current project folder or snapshot store.");
            return false;
        }

        if (_session == null)
        {
            AppendLog("Initialize unified runtime session first.");
            return false;
        }

        return true;
    }

    private void ApplySnapshotResult(RuntimeSnapshotResult result, bool replaceSession)
    {
        if (replaceSession && result.Success && result.Session != null)
        {
            _session = result.Session;
        }

        AppendLog(result.Message);
        foreach (var diagnostic in result.Diagnostics)
        {
            AppendLog($"{diagnostic.Severity} {diagnostic.Code}: {diagnostic.Message}");
        }

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
