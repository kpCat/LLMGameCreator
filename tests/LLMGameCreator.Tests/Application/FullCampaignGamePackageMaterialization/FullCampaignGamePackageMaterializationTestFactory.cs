using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.FullCampaignGamePackageMaterialization;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Tests.Application.FullCampaignGamePackageMaterialization;

public static class FullCampaignGamePackageMaterializationTestFactory
{
    public static FullCampaignGamePackageMaterializationEvidenceService CreateService(
        IFullCampaignGamePackageMaterializationRuntimeAdapter? runtimeAdapter = null) =>
        new(runtimeAdapter: runtimeAdapter ?? new RealFullCampaignGamePackageMaterializationRuntimeAdapter());
}

public sealed class RealFullCampaignGamePackageMaterializationRuntimeAdapter : IFullCampaignGamePackageMaterializationRuntimeAdapter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static RealFullCampaignGamePackageMaterializationRuntimeAdapter()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public FullCampaignRuntimeEvidence Run(FullCampaignRuntimeRequest request)
    {
        var runtime = CreateRuntimeServices();
        var serializer = new RuntimeStateSerializer();
        var snapshotStore = new RuntimeSnapshotStore(serializer);
        var start = runtime.CreateInitialState(request.Package);
        var state = start.State;
        state.Metadata["goal060.rowId"] = request.RowId;
        state.Metadata["goal060.familyId"] = request.FamilyId;
        state.Metadata["goal060.seedId"] = request.SeedId;

        var before = Signature(state);
        var commandEvidence = new List<FullCampaignRuntimeCommandEvidence>();
        foreach (var command in request.Commands)
        {
            var result = runtime.Execute(request.Package, state, ToRuntimeCommand(command));
            commandEvidence.Add(new FullCampaignRuntimeCommandEvidence
            {
                CommandId = command.CommandId,
                CommandType = command.CommandType,
                TargetId = command.TargetId,
                Succeeded = result.Success,
                RuntimeEventTypes = result.Events.Select(item => item.Type.ToString()).Order(StringComparer.Ordinal).ToList(),
                DiagnosticCode = result.Success ? "ok" : result.Diagnostics.FirstOrDefault()?.Code ?? "runtime.command_failed"
            });
            state = result.State;
            if (!result.Success)
            {
                break;
            }
        }

        var after = Signature(state);
        var serialized = serializer.Serialize(state);
        var restored = serializer.DeserializeGameRuntimeState(serialized);
        var restoredSerialized = serializer.Serialize(restored);
        var snapshotProjectRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "Goal060", Guid.NewGuid().ToString("N"));
        var slotName = "goal060_" + request.RowId.Replace('/', '_');
        RuntimeSnapshotResult save;
        RuntimeSnapshotResult load;
        var cleanupSucceeded = false;
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

        var changed = ChangedKeys(before, after);
        var diagnostics = start.Diagnostics
            .Concat(save.Diagnostics)
            .Concat(load.Diagnostics)
            .Select(item => FullCampaignGamePackageMaterializationDiagnostic.Error(item.Code, item.TargetId ?? request.RowId, item.Message))
            .ToList();
        if (!cleanupSucceeded)
        {
            diagnostics.Add(FullCampaignGamePackageMaterializationDiagnostic.Warning("goal060.runtime.snapshot_cleanup_incomplete", request.RowId, "Temporary snapshot folder cleanup did not complete."));
        }

        return new FullCampaignRuntimeEvidence
        {
            RuntimeAttempted = true,
            RuntimeStartSucceeded = start.Success,
            UsedGameRuntimeService = true,
            StateChanged = changed.Count > 0,
            FamilySpecificTransitionObserved = FamilySpecificTransitionObserved(request.FamilyId, changed, state, commandEvidence),
            SaveLoadRoundtripPassed = save.Success && load.Success && string.Equals(serialized, restoredSerialized, StringComparison.Ordinal),
            RuntimeStateHash = Hash(serialized),
            RestoredRuntimeStateHash = Hash(restoredSerialized),
            Commands = commandEvidence,
            ChangedStateKeys = changed,
            Diagnostics = diagnostics
        };
    }

    private static IGameRuntimeService CreateRuntimeServices()
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

        return new GameRuntimeService(
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
    }

    private static GameRuntimeCommand ToRuntimeCommand(FullCampaignRuntimeCommandSpec command) =>
        command.CommandType switch
        {
            "quest/start" => GameRuntimeCommand.StartQuest(command.TargetId),
            "dialogue/open" => GameRuntimeCommand.OpenDialogue(command.TargetId),
            "quest/advance" => GameRuntimeCommand.AdvanceQuestObjective(command.TargetId, command.SecondaryTargetId, command.Amount <= 0 ? 1 : command.Amount),
            "inventory/add_item" => new GameRuntimeCommand { Type = GameRuntimeCommandType.AddItem, Id = command.TargetId, InventoryId = command.InventoryId, Amount = command.Amount <= 0 ? 1 : command.Amount },
            "flag/set" => new GameRuntimeCommand { Type = GameRuntimeCommandType.SetFlag, Id = command.TargetId, Value = command.Value },
            "resource/change" => new GameRuntimeCommand { Type = GameRuntimeCommandType.ChangeResource, Id = command.TargetId, Amount = command.Amount },
            "recipe/craft" => GameRuntimeCommand.CraftRecipe(command.TargetId, command.InventoryId),
            "encounter/start" => GameRuntimeCommand.StartEncounter(command.TargetId, StableSeed(command.CommandId)),
            "encounter/use_ability" => GameRuntimeCommand.UseAbility(command.TargetId, command.Value, command.SecondaryTargetId),
            _ => new GameRuntimeCommand { Type = (GameRuntimeCommandType)999, Id = command.TargetId }
        };

    private static IReadOnlyDictionary<string, string> Signature(GameRuntimeState state) =>
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["quests"] = string.Join(",", state.Quests.OrderBy(item => item.QuestId, StringComparer.Ordinal).Select(item => item.QuestId + ":" + item.State + ":" + string.Join(";", item.Objectives.OrderBy(objective => objective.ObjectiveId, StringComparer.Ordinal).Select(objective => objective.ObjectiveId + "=" + objective.CurrentAmount.ToString("0.####") + "/" + objective.RequiredAmount.ToString("0.####") + ":" + objective.Completed.ToString().ToLowerInvariant())))),
            ["items"] = string.Join(",", state.Inventories.SelectMany(item => item.Stacks).GroupBy(item => item.ItemId, StringComparer.Ordinal).OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => item.Key + "=" + item.Sum(stack => stack.Amount).ToString("0.####"))),
            ["resources"] = string.Join(",", state.Resources.OrderBy(item => item.ResourceId, StringComparer.Ordinal).Select(item => item.ResourceId + "=" + item.Amount.ToString("0.####"))),
            ["flags"] = string.Join(",", state.Flags.OrderBy(item => item.Id, StringComparer.Ordinal).Select(item => item.Id + "=" + item.Value)),
            ["dialogue"] = state.ActiveDialogue == null ? string.Empty : state.ActiveDialogue.DialogueId + ":" + state.ActiveDialogue.Open.ToString().ToLowerInvariant(),
            ["encounter"] = state.ActiveEncounter == null ? string.Empty : state.ActiveEncounter.EncounterId + ":" + state.ActiveEncounter.Active.ToString().ToLowerInvariant() + ":" + string.Join(";", state.ActiveEncounter.ActionHistory)
        };

    private static IReadOnlyList<string> ChangedKeys(IReadOnlyDictionary<string, string> before, IReadOnlyDictionary<string, string> after) =>
        before.Keys.Concat(after.Keys)
            .Distinct(StringComparer.Ordinal)
            .Where(key => !before.TryGetValue(key, out var beforeValue) || !after.TryGetValue(key, out var afterValue) || !string.Equals(beforeValue, afterValue, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToList();

    private static bool FamilySpecificTransitionObserved(
        string familyId,
        IReadOnlyCollection<string> changed,
        GameRuntimeState state,
        IReadOnlyList<FullCampaignRuntimeCommandEvidence> commands) =>
        familyId switch
        {
            "map_panel_rpg" => changed.Contains("quests") && changed.Contains("items") && changed.Contains("flags") && commands.Any(item => item.RuntimeEventTypes.Contains("DialogueOpened")),
            "survival_sandbox" => changed.Contains("resources") && changed.Contains("items") && commands.Any(item => item.RuntimeEventTypes.Contains("RecipeCrafted")),
            "first_person_grid_dungeon" => changed.Contains("encounter") && state.ActiveEncounter != null && commands.Any(item => item.RuntimeEventTypes.Contains("EncounterStarted")),
            _ => false
        };

    private static int StableSeed(string value) =>
        BitConverter.ToInt32(SHA256.HashData(Encoding.UTF8.GetBytes(value)), 0);

    private static string Hash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
