using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.Gameplay;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Tests.Application.Gameplay;

public static class RulePackGameplayFamilyAcceptanceTestFactory
{
    public static RulePackGameplayFamilyAcceptanceService CreateService(
        IRulePackGameplayFamilyRuntimeAdapter? runtimeAdapter = null) =>
        new(runtimeAdapter ?? new RealRulePackGameplayFamilyRuntimeAdapter());
}

public sealed class RealRulePackGameplayFamilyRuntimeAdapter : IRulePackGameplayFamilyRuntimeAdapter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static RealRulePackGameplayFamilyRuntimeAdapter()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public RulePackGameplayFamilyRuntimeEvidence Run(RulePackGameplayFamilyRuntimeRequest request)
    {
        var stateFactory = new GameRuntimeStateFactory();
        var runtime = CreateRuntime(stateFactory);
        var serializer = new RuntimeStateSerializer();
        var snapshotStore = new RuntimeSnapshotStore(serializer);
        var start = runtime.CreateInitialState(request.Package);
        var state = start.State;
        var diagnostics = start.Diagnostics
            .Select(item => Diagnostic(item.Severity, item.Code, item.TargetId ?? request.ScenarioId, item.Message))
            .ToList();

        if (start.Success)
        {
            AddInitialInventory(request.Package, runtime, state, request.InitialInventoryAmounts, diagnostics);
        }

        var commands = new List<RulePackGameplayFamilyRuntimeCommandEvidence>();
        foreach (var command in request.Commands)
        {
            var before = RuntimeSnapshot.FromState(state);
            var result = runtime.Execute(request.Package, state, ToRuntimeCommand(command));
            commands.Add(ToCommandEvidence(command, before, RuntimeSnapshot.FromState(result.State), result));
            state = result.State;
            state.Metadata["gameplayFamily.commandLog"] = string.Join("|", commands.Select(item => item.CommandId + ":" + item.Succeeded.ToString().ToLowerInvariant()));
            if (!result.Success)
            {
                break;
            }
        }

        var snapshot = RuntimeSnapshot.FromState(state);
        var stateEvidence = snapshot.ToEvidence(request.ScenarioId);
        var serialized = serializer.Serialize(state);
        var restoredState = serializer.DeserializeGameRuntimeState(serialized);
        var snapshotProjectRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "RulePackGameplayFamily", Guid.NewGuid().ToString("N"));
        var slotName = "goal008_" + request.ScenarioId.Replace('/', '_');
        var save = snapshotStore.SaveSnapshot(snapshotProjectRoot, slotName, new UnifiedRuntimeSession { GameplayState = state });
        var load = snapshotStore.LoadSnapshot(snapshotProjectRoot, slotName);
        if (save.Diagnostics.Count > 0)
        {
            diagnostics.AddRange(save.Diagnostics.Select(item => Diagnostic(item.Severity, item.Code, item.TargetId ?? slotName, item.Message)));
        }

        if (load.Diagnostics.Count > 0)
        {
            diagnostics.AddRange(load.Diagnostics.Select(item => Diagnostic(item.Severity, item.Code, item.TargetId ?? slotName, item.Message)));
        }

        var restoredFromSnapshot = load.Session?.GameplayState ?? restoredState;
        if (string.Equals(request.ScenarioId, "invalid_save_load_mismatch", StringComparison.Ordinal))
        {
            restoredFromSnapshot.Flags.Add(new RuntimeFlagState { Id = "flag/save_load_mismatch", Value = "corrupted" });
        }

        var restoredEvidence = RuntimeSnapshot.FromState(restoredFromSnapshot).ToEvidence(request.ScenarioId);
        var stateHash = ComputeHash(serialized);
        var restoredHash = ComputeHash(serializer.Serialize(restoredFromSnapshot));
        var completionFlagBefore = string.Empty;
        var completionFlagAfter = string.IsNullOrWhiteSpace(request.CompletionFlagId)
            ? string.Empty
            : state.Flags.FirstOrDefault(item => item.Id == request.CompletionFlagId)?.Value ?? string.Empty;

        var evidenceWithoutHash = new RulePackGameplayFamilyRuntimeEvidence
        {
            RuntimeAttempted = true,
            RuntimeStartSucceeded = start.Success,
            RuntimeStateOwner = "GameRuntimeState",
            PackageId = state.PackageId,
            RuntimeBoundary = new GameplayRuntimeBoundaryEvidence
            {
                AdapterId = "real_game_runtime_service_adapter",
                RuntimeServiceType = typeof(GameRuntimeService).FullName ?? nameof(GameRuntimeService),
                StateFactoryType = typeof(GameRuntimeStateFactory).FullName ?? nameof(GameRuntimeStateFactory),
                SerializerType = typeof(RuntimeStateSerializer).FullName ?? nameof(RuntimeStateSerializer),
                SnapshotStoreType = typeof(RuntimeSnapshotStore).FullName ?? nameof(RuntimeSnapshotStore),
                UsedGameRuntimeService = true,
                UsedRuntimeStateFactory = true
            },
            Commands = commands,
            InventoryBefore = commands.FirstOrDefault()?.InventoryDelta.Before ?? new SortedDictionary<string, string>(StringComparer.Ordinal),
            InventoryAfter = snapshot.InventoryAmounts,
            EquipmentBefore = commands.FirstOrDefault()?.EquipmentDelta.Before ?? new SortedDictionary<string, string>(StringComparer.Ordinal),
            EquipmentAfter = snapshot.EquipmentSlots,
            StatusBefore = commands.FirstOrDefault()?.StatusDelta.Before ?? new SortedDictionary<string, string>(StringComparer.Ordinal),
            StatusAfter = snapshot.StatusEvidence,
            CompletionRewardEvidence = new GameplayCompletionRewardEvidence
            {
                CompletionFlagId = request.CompletionFlagId,
                CompletionFlagBefore = completionFlagBefore,
                CompletionFlagAfter = completionFlagAfter,
                RewardItemIds = commands
                    .SelectMany(item => item.CraftingDelta.Outputs.Concat(item.TradeDelta.Outputs))
                    .Select(item => item.ItemId)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(item => item, StringComparer.Ordinal)
                    .ToList()
            },
            RuntimeStateHash = stateHash,
            RestoredRuntimeStateHash = restoredHash,
            SaveLoadRoundtripPassed = stateHash == restoredHash && DictionaryEquals(stateEvidence, restoredEvidence),
            SaveLoadEvidence = new GameplaySaveLoadEvidence
            {
                UsedRuntimeStateSerializer = true,
                UsedRuntimeSnapshotStore = true,
                SerializedFullState = true,
                SerializedStateHash = stateHash,
                RestoredSerializedStateHash = restoredHash,
                SnapshotSlotName = slotName,
                SnapshotSaveSucceeded = save.Success,
                SnapshotLoadSucceeded = load.Success
            },
            StateEvidence = stateEvidence,
            RestoredStateEvidence = restoredEvidence,
            Diagnostics = diagnostics
        };

        return evidenceWithoutHash with
        {
            RuntimeEvidenceHash = ComputeHash(JsonSerializer.Serialize(evidenceWithoutHash, JsonOptions))
        };
    }

    private static IGameRuntimeService CreateRuntime(GameRuntimeStateFactory stateFactory)
    {
        var requirementEvaluator = new RequirementEvaluator();
        var costConsumer = new CostConsumer();
        var outputApplier = new OutputApplier();
        var recipeRuntimeService = new RecipeRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var transactionRuntimeService = new TransactionRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var containerRuntimeService = new ContainerRuntimeService();
        var harvestRuntimeService = new HarvestRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var useItemRuntimeService = new UseItemRuntimeService(requirementEvaluator, outputApplier);

        return new GameRuntimeService(
            stateFactory,
            recipeRuntimeService,
            new LootRuntimeService(requirementEvaluator, outputApplier),
            transactionRuntimeService,
            new ResourceNetworkRuntimeService(requirementEvaluator, costConsumer, outputApplier),
            useItemRuntimeService,
            new InteractionRuntimeService(requirementEvaluator, outputApplier, recipeRuntimeService, transactionRuntimeService, containerRuntimeService, harvestRuntimeService, useItemRuntimeService),
            new EquipmentRuntimeService(requirementEvaluator),
            containerRuntimeService,
            harvestRuntimeService);
    }

    private static void AddInitialInventory(
        GamePackageDefinition package,
        IGameRuntimeService runtime,
        GameRuntimeState state,
        IReadOnlyDictionary<string, double> amounts,
        List<RulePackGameplayFamilyDiagnostic> diagnostics)
    {
        foreach (var item in amounts.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var result = runtime.Execute(package, state, new GameRuntimeCommand
            {
                Type = GameRuntimeCommandType.AddItem,
                Id = item.Key,
                Amount = item.Value,
                InventoryId = "inventory/player"
            });
            diagnostics.AddRange(result.Diagnostics.Select(diagnostic => Diagnostic(diagnostic.Severity, diagnostic.Code, diagnostic.TargetId ?? item.Key, diagnostic.Message)));
        }
    }

    private static GameRuntimeCommand ToRuntimeCommand(GameplayCommandSpec command) =>
        command.CommandType switch
        {
            "gameplay/use_item" => GameRuntimeCommand.UseItem(command.TargetId, command.InventoryId),
            "gameplay/equip_item" => GameRuntimeCommand.EquipItem(command.TargetId, command.SecondaryTargetId, command.InventoryId),
            "gameplay/craft_recipe" => GameRuntimeCommand.CraftRecipe(command.TargetId, command.InventoryId),
            "gameplay/execute_transaction" => GameRuntimeCommand.ExecuteTransaction(command.TargetId, command.InventoryId),
            "gameplay/set_flag" => new GameRuntimeCommand { Type = GameRuntimeCommandType.SetFlag, Id = command.TargetId, Value = command.Value },
            _ => new GameRuntimeCommand { Type = (GameRuntimeCommandType)999, Id = command.TargetId }
        };

    private static RulePackGameplayFamilyRuntimeCommandEvidence ToCommandEvidence(
        GameplayCommandSpec command,
        RuntimeSnapshot before,
        RuntimeSnapshot after,
        GameRuntimeResult result)
    {
        var diagnostic = result.Diagnostics.FirstOrDefault(item => item.Severity.Equals("error", StringComparison.OrdinalIgnoreCase));
        return new RulePackGameplayFamilyRuntimeCommandEvidence
        {
            CommandId = command.CommandId,
            CommandType = command.CommandType,
            TargetId = command.TargetId,
            SecondaryTargetId = command.SecondaryTargetId,
            Succeeded = result.Success,
            DiagnosticCode = result.Success ? "ok" : diagnostic?.Code ?? "runtime.command_failed",
            DiagnosticMessage = result.Success ? string.Empty : diagnostic?.Message ?? result.Message,
            RuntimeEventTypes = result.Events.Select(item => item.Type.ToString()).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            RuntimeDiagnosticCodes = result.Diagnostics.Select(item => item.Code).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            InventoryDelta = InventoryDelta(before, after),
            EquipmentDelta = EquipmentDelta(before, after),
            CraftingDelta = CraftingDelta(command, before, after),
            TradeDelta = TradeDelta(command, before, after),
            StatusDelta = StatusDelta(before, after),
            CompletionDelta = CompletionDelta(before, after)
        };
    }

    private static GameplayInventoryDelta InventoryDelta(RuntimeSnapshot before, RuntimeSnapshot after) => new()
    {
        Before = before.InventoryAmounts,
        After = after.InventoryAmounts,
        Changed = !DictionaryEquals(before.InventoryAmounts, after.InventoryAmounts)
    };

    private static GameplayEquipmentDelta EquipmentDelta(RuntimeSnapshot before, RuntimeSnapshot after) => new()
    {
        Before = before.EquipmentSlots,
        After = after.EquipmentSlots,
        Changed = !DictionaryEquals(before.EquipmentSlots, after.EquipmentSlots)
    };

    private static GameplayCraftingDelta CraftingDelta(GameplayCommandSpec command, RuntimeSnapshot before, RuntimeSnapshot after) => new()
    {
        Changed = command.CommandType == "gameplay/craft_recipe" && !DictionaryEquals(before.InventoryAmounts, after.InventoryAmounts),
        Inputs = command.CommandType == "gameplay/craft_recipe"
            ? ItemChanges(before.InventoryAmounts, after.InventoryAmounts).Where(item => item.AmountBefore > item.AmountAfter).ToList()
            : [],
        Outputs = command.CommandType == "gameplay/craft_recipe"
            ? ItemChanges(before.InventoryAmounts, after.InventoryAmounts).Where(item => item.AmountAfter > item.AmountBefore).ToList()
            : []
    };

    private static GameplayTradeDelta TradeDelta(GameplayCommandSpec command, RuntimeSnapshot before, RuntimeSnapshot after) => new()
    {
        Changed = command.CommandType == "gameplay/execute_transaction" && !DictionaryEquals(before.InventoryAmounts, after.InventoryAmounts),
        Costs = command.CommandType == "gameplay/execute_transaction"
            ? ItemChanges(before.InventoryAmounts, after.InventoryAmounts).Where(item => item.AmountBefore > item.AmountAfter).ToList()
            : [],
        Outputs = command.CommandType == "gameplay/execute_transaction"
            ? ItemChanges(before.InventoryAmounts, after.InventoryAmounts).Where(item => item.AmountAfter > item.AmountBefore).ToList()
            : []
    };

    private static GameplayStatusDelta StatusDelta(RuntimeSnapshot before, RuntimeSnapshot after) => new()
    {
        Before = before.StatusEvidence,
        After = after.StatusEvidence,
        Changed = !DictionaryEquals(before.StatusEvidence, after.StatusEvidence)
    };

    private static GameplayCompletionDelta CompletionDelta(RuntimeSnapshot before, RuntimeSnapshot after) => new()
    {
        Before = before.FlagEvidence,
        After = after.FlagEvidence,
        Changed = !DictionaryEquals(before.FlagEvidence, after.FlagEvidence)
    };

    private static IReadOnlyList<GameplayItemAmountChange> ItemChanges(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after)
    {
        var ids = before.Keys.Concat(after.Keys).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal);
        return ids.Select(id => new GameplayItemAmountChange
            {
                ItemId = id,
                AmountBefore = ParseDouble(before.GetValueOrDefault(id)) ?? 0,
                AmountAfter = ParseDouble(after.GetValueOrDefault(id)) ?? 0
            })
            .Where(item => Math.Abs(item.AmountBefore - item.AmountAfter) > double.Epsilon)
            .ToList();
    }

    private static double? ParseDouble(string? value) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private static bool DictionaryEquals(IReadOnlyDictionary<string, string> first, IReadOnlyDictionary<string, string> second) =>
        first.Count == second.Count &&
        first.All(item => second.TryGetValue(item.Key, out var value) && string.Equals(item.Value, value, StringComparison.Ordinal));

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static RulePackGameplayFamilyDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private sealed record RuntimeSnapshot
    {
        public string PackageId { get; init; } = string.Empty;
        public string CurrentMapId { get; init; } = string.Empty;
        public IReadOnlyDictionary<string, string> InventoryAmounts { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
        public IReadOnlyDictionary<string, string> EquipmentSlots { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
        public IReadOnlyDictionary<string, string> StatusEvidence { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
        public IReadOnlyDictionary<string, string> FlagEvidence { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);
        public IReadOnlyDictionary<string, string> Metadata { get; init; } = new SortedDictionary<string, string>(StringComparer.Ordinal);

        public static RuntimeSnapshot FromState(GameRuntimeState state) => new()
        {
            PackageId = state.PackageId,
            CurrentMapId = state.CurrentMapId,
            InventoryAmounts = state.Inventories
                .SelectMany(item => item.Stacks)
                .GroupBy(item => item.ItemId, StringComparer.Ordinal)
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .ToDictionary(item => item.Key, item => item.Sum(stack => stack.Amount).ToString("0.####", CultureInfo.InvariantCulture), StringComparer.Ordinal),
            EquipmentSlots = state.Equipment
                .SelectMany(item => item.Slots)
                .OrderBy(item => item.SlotId, StringComparer.Ordinal)
                .ToDictionary(item => item.SlotId, item => item.ItemId ?? string.Empty, StringComparer.Ordinal),
            StatusEvidence = state.Statuses
                .OrderBy(item => item.StatusId, StringComparer.Ordinal)
                .ThenBy(item => item.TargetId, StringComparer.Ordinal)
                .ToDictionary(item => item.StatusId + "@" + item.TargetId, item => (item.RemainingTicks?.ToString(CultureInfo.InvariantCulture) ?? string.Empty) + "|" + item.Metadata.GetValueOrDefault("sourceCommandId", string.Empty), StringComparer.Ordinal),
            FlagEvidence = state.Flags
                .OrderBy(item => item.Id, StringComparer.Ordinal)
                .ToDictionary(item => item.Id, item => item.Value, StringComparer.Ordinal),
            Metadata = state.Metadata
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal)
        };

        public IReadOnlyDictionary<string, string> ToEvidence(string scenarioId) => new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["scenarioId"] = scenarioId,
            ["packageId"] = PackageId,
            ["currentMapId"] = CurrentMapId,
            ["inventory"] = string.Join(",", InventoryAmounts.Select(item => item.Key + "=" + item.Value)),
            ["equipment"] = string.Join(",", EquipmentSlots.Select(item => item.Key + "=" + item.Value)),
            ["statuses"] = string.Join(",", StatusEvidence.Select(item => item.Key + "=" + item.Value)),
            ["flags"] = string.Join(",", FlagEvidence.Select(item => item.Key + "=" + item.Value)),
            ["commandLog"] = Metadata.GetValueOrDefault("gameplayFamily.commandLog", string.Empty)
        };
    }
}
