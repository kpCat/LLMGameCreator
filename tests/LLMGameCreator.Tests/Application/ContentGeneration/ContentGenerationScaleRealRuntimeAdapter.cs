using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.ContentGeneration;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Tests.Application.ContentGeneration;

public static class ContentGenerationScaleAcceptanceTestFactory
{
    public static ContentGenerationScaleAcceptanceService CreateService(
        IContentGenerationScaleRuntimeAdapter? runtimeAdapter = null) =>
        new(runtimeAdapter ?? new RealContentGenerationScaleRuntimeAdapter());
}

public sealed class RealContentGenerationScaleRuntimeAdapter : IContentGenerationScaleRuntimeAdapter
{
    private string _previousPackId = string.Empty;
    private IReadOnlyDictionary<string, string> _previousDynamicSignature = new SortedDictionary<string, string>(StringComparer.Ordinal);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static RealContentGenerationScaleRuntimeAdapter()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public ContentGenerationRuntimeEvidence Run(ContentGenerationRuntimeRequest request)
    {
        var services = CreateRuntimeServices();
        var serializer = new RuntimeStateSerializer();
        var snapshotStore = new RuntimeSnapshotStore(serializer);
        var start = services.Runtime.CreateInitialState(request.Package);
        var state = start.State;
        state.Metadata["contentGeneration.packId"] = request.PackId;
        state.Metadata["contentGeneration.threadId"] = request.ThreadId;

        var initialSignature = DynamicSignature(state);
        var retained = _previousPackId.Length > 0 && !string.Equals(_previousPackId, request.PackId, StringComparison.Ordinal)
            ? initialSignature.Keys.Where(key => _previousDynamicSignature.ContainsKey(key)).ToList()
            : [];
        var isolationPassed = retained.Count == 0;

        var commandEvidence = new List<ContentGenerationRuntimeCommandEvidence>();
        var beforeQuestIds = QuestProgress(state);
        var beforeItems = InventoryItems(state);
        var beforeFlags = Flags(state);
        var beforeFactions = Factions(state);
        foreach (var command in request.Commands)
        {
            var result = services.Runtime.Execute(request.Package, state, ToRuntimeCommand(command));
            commandEvidence.Add(new ContentGenerationRuntimeCommandEvidence
            {
                CommandId = command.CommandId,
                CommandType = command.CommandType,
                TargetId = command.TargetId,
                SecondaryTargetId = command.SecondaryTargetId,
                Value = command.Value,
                InventoryId = command.InventoryId,
                Amount = command.Amount,
                Succeeded = result.Success,
                DiagnosticCode = result.Success ? "ok" : result.Diagnostics.FirstOrDefault()?.Code ?? "runtime.command_failed",
                RuntimeEventTypes = result.Events.Select(item => item.Type.ToString()).OrderBy(item => item, StringComparer.Ordinal).ToList()
            });
            state = result.State;
            state.Metadata["contentGeneration.commandLog"] = string.Join("|", commandEvidence.Select(item => item.CommandId + ":" + item.Succeeded.ToString().ToLowerInvariant()));
            if (!result.Success)
            {
                break;
            }
        }

        var afterQuestIds = QuestProgress(state);
        var afterItems = InventoryItems(state);
        var afterFlags = Flags(state);
        var afterFactions = Factions(state);
        var stateEvidence = Evidence(request, state);
        var serialized = serializer.Serialize(state);
        var restored = serializer.DeserializeGameRuntimeState(serialized);
        var snapshotProjectRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "ContentGenerationScale", Guid.NewGuid().ToString("N"));
        var slotName = "goal010_" + request.PackId + "_" + request.ThreadId.Replace('/', '_');
        var cleanupSucceeded = false;
        RuntimeSnapshotResult save;
        RuntimeSnapshotResult load;
        try
        {
            save = snapshotStore.SaveSnapshot(snapshotProjectRoot, slotName, new UnifiedRuntimeSession { GameplayState = state });
            load = snapshotStore.LoadSnapshot(snapshotProjectRoot, slotName);
            if (Directory.Exists(snapshotProjectRoot))
            {
                Directory.Delete(snapshotProjectRoot, recursive: true);
            }

            cleanupSucceeded = !Directory.Exists(snapshotProjectRoot);
        }
        finally
        {
            if (Directory.Exists(snapshotProjectRoot))
            {
                Directory.Delete(snapshotProjectRoot, recursive: true);
            }
        }

        var restoredFromSnapshot = load.Session?.GameplayState ?? restored;
        var restoredEvidence = Evidence(request, restoredFromSnapshot);
        var restoredSerialized = serializer.Serialize(restoredFromSnapshot);
        var diagnostics = start.Diagnostics
            .Concat(save.Diagnostics)
            .Concat(load.Diagnostics)
            .Select(item => new ContentGenerationScaleDiagnostic
            {
                Severity = item.Severity,
                Code = item.Code,
                Target = item.TargetId ?? request.ThreadId,
                Message = item.Message
            })
            .ToList();

        var dynamicSignature = DynamicSignature(state);
        _previousPackId = request.PackId;
        _previousDynamicSignature = dynamicSignature;

        var evidenceWithoutHash = new ContentGenerationRuntimeEvidence
        {
            RuntimeAttempted = true,
            RuntimeStartSucceeded = start.Success,
            RuntimeStateOwner = "GameRuntimeState",
            PackageId = state.PackageId,
            PackageHash = request.PackageHash,
            RuntimeBoundary = new ContentGenerationRuntimeBoundaryEvidence
            {
                AdapterId = "real_content_generation_scale_game_runtime_service_adapter",
                RuntimeServiceType = typeof(GameRuntimeService).FullName ?? nameof(GameRuntimeService),
                StateFactoryType = typeof(GameRuntimeStateFactory).FullName ?? nameof(GameRuntimeStateFactory),
                SerializerType = typeof(RuntimeStateSerializer).FullName ?? nameof(RuntimeStateSerializer),
                SnapshotStoreType = typeof(RuntimeSnapshotStore).FullName ?? nameof(RuntimeSnapshotStore),
                UsedGameRuntimeService = true,
                UsedRuntimeStateFactory = true
            },
            Commands = commandEvidence,
            StateDelta = new ContentGenerationRuntimeStateDelta
            {
                QuestProgressChanged = !DictionaryEquals(beforeQuestIds, afterQuestIds),
                RewardItemChanged = !DictionaryEquals(beforeItems, afterItems),
                FlagChanged = !DictionaryEquals(beforeFlags, afterFlags),
                ReputationChanged = !DictionaryEquals(beforeFactions, afterFactions),
                ChangedQuestIds = ChangedKeys(beforeQuestIds, afterQuestIds),
                ChangedItemIds = ChangedKeys(beforeItems, afterItems),
                ChangedFlagIds = ChangedKeys(beforeFlags, afterFlags),
                ChangedFactionIds = ChangedKeys(beforeFactions, afterFactions)
            },
            RuntimeStateHash = ComputeHash(serialized),
            RestoredRuntimeStateHash = ComputeHash(restoredSerialized),
            SaveLoadRoundtripPassed = string.Equals(serialized, restoredSerialized, StringComparison.Ordinal) && DictionaryEquals(stateEvidence, restoredEvidence),
            SaveLoadEvidence = new ContentGenerationSaveLoadEvidence
            {
                UsedRuntimeStateSerializer = true,
                UsedRuntimeSnapshotStore = true,
                SerializedFullState = true,
                SerializedStateHash = ComputeHash(serialized),
                RestoredSerializedStateHash = ComputeHash(restoredSerialized),
                SnapshotSlotName = slotName,
                SnapshotSaveSucceeded = save.Success,
                SnapshotLoadSucceeded = load.Success,
                TempSnapshotCleanupSucceeded = cleanupSucceeded
            },
            IsolationPassed = isolationPassed,
            StateEvidence = stateEvidence,
            RestoredStateEvidence = restoredEvidence,
            Diagnostics = diagnostics
        };

        return evidenceWithoutHash with
        {
            RuntimeEvidenceHash = ComputeHash(JsonSerializer.Serialize(evidenceWithoutHash, JsonOptions))
        };
    }

    private static RuntimeServices CreateRuntimeServices()
    {
        var requirementEvaluator = new RequirementEvaluator();
        var costConsumer = new CostConsumer();
        var outputApplier = new OutputApplier();
        var stateFactory = new GameRuntimeStateFactory();
        var recipeRuntimeService = new RecipeRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var transactionRuntimeService = new TransactionRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var containerRuntimeService = new ContainerRuntimeService();
        var harvestRuntimeService = new HarvestRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var useItemRuntimeService = new UseItemRuntimeService(requirementEvaluator, outputApplier);
        var encounterRuntimeService = new EncounterRuntimeService(requirementEvaluator, outputApplier);
        var encounterAiService = new EncounterAiService(encounterRuntimeService);
        var factionRuntimeService = new FactionRuntimeService();
        var questRuntimeService = new QuestRuntimeService(requirementEvaluator, outputApplier);
        var dialogueRuntimeService = new DialogueRuntimeService(requirementEvaluator, costConsumer, outputApplier, questRuntimeService, transactionRuntimeService, encounterRuntimeService);
        var interactionRuntimeService = new InteractionRuntimeService(
            requirementEvaluator,
            outputApplier,
            recipeRuntimeService,
            transactionRuntimeService,
            containerRuntimeService,
            harvestRuntimeService,
            useItemRuntimeService,
            dialogueRuntimeService,
            questRuntimeService,
            encounterRuntimeService);

        var runtime = new GameRuntimeService(
            stateFactory,
            recipeRuntimeService,
            new LootRuntimeService(requirementEvaluator, outputApplier),
            transactionRuntimeService,
            new ResourceNetworkRuntimeService(requirementEvaluator, costConsumer, outputApplier),
            useItemRuntimeService,
            interactionRuntimeService,
            new EquipmentRuntimeService(requirementEvaluator),
            containerRuntimeService,
            harvestRuntimeService,
            encounterRuntimeService,
            encounterAiService,
            questRuntimeService,
            dialogueRuntimeService,
            factionRuntimeService);

        return new RuntimeServices(runtime);
    }

    private static GameRuntimeCommand ToRuntimeCommand(ContentGenerationRuntimeCommand command) =>
        command.CommandType switch
        {
            "quest/start" => GameRuntimeCommand.StartQuest(command.TargetId),
            "dialogue/open" => GameRuntimeCommand.OpenDialogue(command.TargetId),
            "dialogue/choose" => GameRuntimeCommand.ChooseDialogueOption(command.TargetId),
            "objective/add_item" => new GameRuntimeCommand { Type = GameRuntimeCommandType.AddItem, Id = command.TargetId, Amount = command.Amount, InventoryId = command.InventoryId },
            "objective/set_flag" => new GameRuntimeCommand { Type = GameRuntimeCommandType.SetFlag, Id = command.TargetId, Value = command.Value },
            "event/set_flag" => new GameRuntimeCommand { Type = GameRuntimeCommandType.SetFlag, Id = command.TargetId, Value = command.Value },
            "event/add_item" => new GameRuntimeCommand { Type = GameRuntimeCommandType.AddItem, Id = command.TargetId, Amount = command.Amount, InventoryId = command.InventoryId },
            "event/change_reputation" => GameRuntimeCommand.ChangeReputation(command.TargetId, command.Amount),
            "event/advance_quest" => GameRuntimeCommand.AdvanceQuestObjective(command.TargetId, command.SecondaryTargetId, command.Amount),
            "loot/roll" => GameRuntimeCommand.RollLootTable(command.TargetId, command.InventoryId, StableSeed(command.CommandId)),
            _ => new GameRuntimeCommand { Type = (GameRuntimeCommandType)999, Id = command.TargetId }
        };

    private static IReadOnlyDictionary<string, string> Evidence(ContentGenerationRuntimeRequest request, GameRuntimeState state) =>
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["packId"] = request.PackId,
            ["threadId"] = request.ThreadId,
            ["packageId"] = state.PackageId,
            ["quests"] = string.Join(",", QuestProgress(state).Select(item => item.Key + "=" + item.Value)),
            ["items"] = string.Join(",", InventoryItems(state).Select(item => item.Key + "=" + item.Value)),
            ["flags"] = string.Join(",", Flags(state).Select(item => item.Key + "=" + item.Value)),
            ["factions"] = string.Join(",", Factions(state).Select(item => item.Key + "=" + item.Value)),
            ["commandLog"] = state.Metadata.GetValueOrDefault("contentGeneration.commandLog", string.Empty)
        };

    private static IReadOnlyDictionary<string, string> DynamicSignature(GameRuntimeState state)
    {
        var signature = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in QuestProgress(state).Where(item => item.Value.Contains("active", StringComparison.Ordinal) || item.Value.Contains("completed", StringComparison.Ordinal)))
        {
            signature["quest:" + pair.Key] = pair.Value;
        }

        foreach (var pair in InventoryItems(state))
        {
            signature["item:" + pair.Key] = pair.Value;
        }

        foreach (var pair in Flags(state))
        {
            signature["flag:" + pair.Key] = pair.Value;
        }

        foreach (var pair in Factions(state).Where(item => !item.Value.StartsWith("0|", StringComparison.Ordinal)))
        {
            signature["faction:" + pair.Key] = pair.Value;
        }

        return signature;
    }

    private static IReadOnlyDictionary<string, string> QuestProgress(GameRuntimeState state) =>
        state.Quests
            .OrderBy(item => item.QuestId, StringComparer.Ordinal)
            .ToDictionary(
                item => item.QuestId,
                item => item.State + "|" + string.Join(";", item.Objectives.OrderBy(objective => objective.ObjectiveId, StringComparer.Ordinal).Select(objective => objective.ObjectiveId + ":" + objective.CurrentAmount.ToString("0.####") + "/" + objective.RequiredAmount.ToString("0.####") + ":" + objective.Completed.ToString().ToLowerInvariant())),
                StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, string> InventoryItems(GameRuntimeState state)
    {
        var player = state.Inventories.FirstOrDefault(item => item.Id == "inventory/player" || item.OwnerKind.Equals("player", StringComparison.OrdinalIgnoreCase));
        if (player == null)
        {
            return new SortedDictionary<string, string>(StringComparer.Ordinal);
        }

        return player.Stacks
            .GroupBy(item => item.ItemId, StringComparer.Ordinal)
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .ToDictionary(item => item.Key, item => item.Sum(stack => stack.Amount).ToString("0.####"), StringComparer.Ordinal);
    }

    private static IReadOnlyDictionary<string, string> Flags(GameRuntimeState state) =>
        state.Flags
            .OrderBy(item => item.Id, StringComparer.Ordinal)
            .ToDictionary(item => item.Id, item => item.Value, StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, string> Factions(GameRuntimeState state) =>
        state.Factions
            .OrderBy(item => item.FactionId, StringComparer.Ordinal)
            .ToDictionary(item => item.FactionId, item => item.Reputation.ToString("0.####") + "|" + item.RelationKind, StringComparer.Ordinal);

    private static IReadOnlyList<string> ChangedKeys(IReadOnlyDictionary<string, string> before, IReadOnlyDictionary<string, string> after) =>
        before.Keys.Concat(after.Keys)
            .Distinct(StringComparer.Ordinal)
            .Where(key => !before.TryGetValue(key, out var beforeValue) || !after.TryGetValue(key, out var afterValue) || !string.Equals(beforeValue, afterValue, StringComparison.Ordinal))
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();

    private static bool DictionaryEquals(IReadOnlyDictionary<string, string> left, IReadOnlyDictionary<string, string> right) =>
        left.Count == right.Count && left.All(item => right.TryGetValue(item.Key, out var value) && string.Equals(item.Value, value, StringComparison.Ordinal));

    private static int StableSeed(string value) =>
        BitConverter.ToInt32(SHA256.HashData(Encoding.UTF8.GetBytes(value)), 0);

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed record RuntimeServices(IGameRuntimeService Runtime);
}
