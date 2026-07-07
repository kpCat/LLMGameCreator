using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class CanonicalRuntimePlayerCommandLoopService :
    ICanonicalRuntimePlayerCommandLoopService
{
    private static readonly JsonSerializerOptions StableJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    static CanonicalRuntimePlayerCommandLoopService()
    {
        StableJsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    private readonly IUnifiedGameRuntimeService _runtime;

    public CanonicalRuntimePlayerCommandLoopService(IUnifiedGameRuntimeService runtime)
    {
        _runtime = runtime;
    }

    public static IReadOnlyList<string> RequiredCategories =>
    [
        "load_package",
        "start_runtime",
        "move",
        "interact",
        "show_dialogue",
        "start_or_update_quest",
        "show_inventory",
        "craft",
        "harvest",
        "transaction",
        "encounter",
        "combat_round",
        "final_state"
    ];

    public static CanonicalRuntimePlayerCommandLoopService CreateDefault()
    {
        var requirementEvaluator = new RequirementEvaluator();
        var costConsumer = new CostConsumer();
        var outputApplier = new OutputApplier();
        var recipe = new RecipeRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var transaction = new TransactionRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var harvest = new HarvestRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var encounter = new EncounterRuntimeService(requirementEvaluator, outputApplier);
        var quest = new QuestRuntimeService(requirementEvaluator, outputApplier);
        var dialogue = new DialogueRuntimeService(
            requirementEvaluator,
            costConsumer,
            outputApplier,
            quest,
            transaction,
            encounter);
        var useItem = new UseItemRuntimeService(requirementEvaluator, outputApplier);
        var interaction = new InteractionRuntimeService(
            requirementEvaluator,
            outputApplier,
            recipe,
            transaction,
            new ContainerRuntimeService(),
            harvest,
            useItem,
            dialogue,
            quest,
            encounter);
        var gameplay = new GameRuntimeService(
            new GameRuntimeStateFactory(),
            recipe,
            new LootRuntimeService(requirementEvaluator, outputApplier),
            transaction,
            new ResourceNetworkRuntimeService(requirementEvaluator, costConsumer, outputApplier),
            useItem,
            interaction,
            harvestRuntimeService: harvest,
            encounterRuntimeService: encounter,
            questRuntimeService: quest,
            dialogueRuntimeService: dialogue);

        return new CanonicalRuntimePlayerCommandLoopService(
            new UnifiedGameRuntimeService(new DefaultGameRuntime(), gameplay));
    }

    public CanonicalRuntimePlayerCommandLoopResult Execute(
        GamePackageDefinition package,
        CanonicalRuntimePlayerCommandLoopRequest request)
    {
        var steps = BuildSteps();
        var session = new UnifiedRuntimeSession();
        var snapshots = new List<CanonicalRuntimePlayerCommandLoopSnapshot>();
        var diagnostics = new List<string>();
        var missingPrimitives = new List<string>();
        var eventIndex = 0;
        var stateHashBefore = "not_loaded";
        var canonicalRuntimeStarted = false;
        var runtimeExecutionSucceeded = true;

        foreach (var step in steps)
        {
            var events = new List<CanonicalRuntimePlayerCommandLoopRuntimeEvent>();
            UnifiedRuntimeResult? result = null;
            switch (step.StepId)
            {
                case "load_selected_package":
                    events.Add(CommandLoopEvent(ref eventIndex, step, "package-loaded", package.Manifest.PackageId));
                    break;
                case "start_canonical_runtime":
                    result = _runtime.Start(package);
                    canonicalRuntimeStarted = result.Success;
                    break;
                case "move_to_sign":
                    result = _runtime.ExecutePlayerCommand(
                        package,
                        session,
                        PlayerCommand.Move(Direction2D.Right));
                    break;
                case "interact_with_sign":
                    result = _runtime.ExecutePlayerCommand(package, session, PlayerCommand.Interact());
                    break;
                case "show_old_guard_dialogue":
                    result = _runtime.ExecuteGameplayCommand(
                        package,
                        session,
                        GameRuntimeCommand.OpenDialogue("dialogue/old_guard_intro"));
                    break;
                case "start_or_update_help_healer_quest":
                    result = _runtime.ExecuteMany(
                        package,
                        session,
                        [
                            GameRuntimeCommand.StartQuest("quest/help_healer"),
                            new GameRuntimeCommand { Type = GameRuntimeCommandType.RefreshQuestObjectives }
                        ]);
                    break;
                case "show_inventory_state":
                    result = _runtime.ExecuteGameplayCommand(
                        package,
                        session,
                        new GameRuntimeCommand
                        {
                            Type = GameRuntimeCommandType.AddItem,
                            Id = "item/apple",
                            InventoryId = "inventory/player_start",
                            Amount = 1
                        });
                    break;
                case "craft_healing_potion":
                    result = _runtime.ExecuteGameplayCommand(
                        package,
                        session,
                        GameRuntimeCommand.CraftRecipe(
                            "recipe/healing_potion",
                            "inventory/player_start"));
                    break;
                case "harvest_apple_tree":
                    result = _runtime.ExecuteGameplayCommand(
                        package,
                        session,
                        GameRuntimeCommand.HarvestResourceNode(
                            "node/apple_tree",
                            "inventory/player_start",
                            "item/woodcutting_axe",
                            136));
                    break;
                case "execute_transaction":
                    result = _runtime.ExecuteMany(
                        package,
                        session,
                        [
                            new GameRuntimeCommand
                            {
                                Type = GameRuntimeCommandType.ChangeResource,
                                Id = "resource/gold",
                                Amount = 25
                            },
                            GameRuntimeCommand.ExecuteTransaction(
                                "transaction/buy_healing_potion",
                                "inventory/player_start")
                        ]);
                    break;
                case "start_encounter":
                    result = _runtime.ExecuteGameplayCommand(
                        package,
                        session,
                        GameRuntimeCommand.StartEncounter("encounter/goblin_duel", 136));
                    break;
                case "combat_round":
                    result = _runtime.ExecuteGameplayCommand(
                        package,
                        session,
                        GameRuntimeCommand.BasicAttack("player", "goblin"));
                    break;
                case "final_state":
                    events.Add(CommandLoopEvent(ref eventIndex, step, "final-state", package.Manifest.PackageId));
                    break;
            }

            if (result is not null)
            {
                session = result.Session;
                events.AddRange(RuntimeEvents(ref eventIndex, step, result));
                diagnostics.AddRange(result.Diagnostics.Select(diagnostic =>
                    step.StepId + ":" + diagnostic.Code + ":" + diagnostic.Message));
                if (!result.Success)
                {
                    runtimeExecutionSucceeded = false;
                    missingPrimitives.Add(step.StepId + ":" + result.Message);
                }
            }

            var stateHashAfter = result is null && step.StepId == "load_selected_package"
                ? HashText(package.Manifest.PackageId + "|" + package.Manifest.Title + "|" + request.CandidateId)
                : HashSession(session);
            var snapshot = BuildSnapshot(step, stateHashBefore, stateHashAfter, package, session, events);
            snapshots.Add(snapshot);
            stateHashBefore = stateHashAfter;

            if (!runtimeExecutionSucceeded)
            {
                break;
            }
        }

        var presentCategories = snapshots
            .Select(snapshot => snapshot.Category)
            .ToHashSet(StringComparer.Ordinal);
        var missingCategories = RequiredCategories
            .Where(category => !presentCategories.Contains(category))
            .OrderBy(category => category, StringComparer.Ordinal)
            .ToList();
        var runtimeEventCount = snapshots
            .SelectMany(snapshot => snapshot.RuntimeEvents)
            .Count(item => item.Source is "map-runtime" or "gameplay-runtime");
        var stateHashChain = snapshots
            .Select(snapshot => snapshot.StateHashAfter)
            .ToList();
        var stateHashChainPresent = snapshots.Count == steps.Count
                                    && snapshots.All(snapshot =>
                                        !string.IsNullOrWhiteSpace(snapshot.StateHashBefore)
                                        && !string.IsNullOrWhiteSpace(snapshot.StateHashAfter))
                                    && snapshots.Skip(1).All(snapshot =>
                                        string.Equals(
                                            snapshots[snapshot.StepIndex - 1].StateHashAfter,
                                            snapshot.StateHashBefore,
                                            StringComparison.Ordinal));
        var selectedCandidateExecutedByRuntime =
            canonicalRuntimeStarted && runtimeEventCount > 0 && runtimeExecutionSucceeded;
        var passed = snapshots.Count == steps.Count
                     && snapshots.Count >= 10
                     && runtimeEventCount >= 10
                     && stateHashChainPresent
                     && missingCategories.Count == 0
                     && selectedCandidateExecutedByRuntime
                     && missingPrimitives.Count == 0;

        return new CanonicalRuntimePlayerCommandLoopResult
        {
            CandidateId = request.CandidateId,
            PackagePath = request.PackagePath,
            PlayerCommandLoopPassed = passed,
            PlayerCommandCount = steps.Count,
            PlayerSnapshotCount = snapshots.Count,
            RuntimeEventCount = runtimeEventCount,
            StateHashChainPresent = stateHashChainPresent,
            AllRequiredCategoriesPresent = missingCategories.Count == 0,
            SelectedCandidateExecutedByRuntime = selectedCandidateExecutedByRuntime,
            ProjectionOnly = false,
            UnityGameplayTruth = false,
            RuntimePrimitiveMissing = missingPrimitives.Count > 0,
            MissingRuntimePrimitives = missingPrimitives,
            RequiredCategories = RequiredCategories,
            MissingCategories = missingCategories,
            Inputs = new CanonicalRuntimePlayerCommandLoopInput
            {
                CandidateId = request.CandidateId,
                PackagePath = request.PackagePath,
                HandoffPath = request.HandoffPath,
                Goal134TranscriptPath = request.Goal134TranscriptPath,
                Goal134StateSummaryPath = request.Goal134StateSummaryPath,
                Goal135PlayerLoopPlanPath = request.Goal135PlayerLoopPlanPath,
                Goal135PlayerAdapterContractPath = request.Goal135PlayerAdapterContractPath
            },
            Steps = steps,
            Snapshots = snapshots,
            StateHashChain = stateHashChain,
            Diagnostics = diagnostics
        };
    }

    private static IReadOnlyList<CanonicalRuntimePlayerCommandLoopStep> BuildSteps() =>
    [
        Step(0, "load_selected_package", "load_package", "Load selected package", "package", "game/minimal-map-game", false),
        Step(1, "start_canonical_runtime", "start_runtime", "Start canonical runtime", nameof(GameRuntimeCommandType.Initialize), "map/village"),
        Step(2, "move_to_sign", "move", "Move right to sign", nameof(PlayerCommandType.Move), "entity/village/sign"),
        Step(3, "interact_with_sign", "interact", "Interact with sign", nameof(PlayerCommandType.Interact), "interaction/sign_inspect"),
        Step(4, "show_old_guard_dialogue", "show_dialogue", "Open old guard dialogue", nameof(GameRuntimeCommandType.OpenDialogue), "dialogue/old_guard_intro"),
        Step(5, "start_or_update_help_healer_quest", "start_or_update_quest", "Start and refresh help healer quest", nameof(GameRuntimeCommandType.StartQuest), "quest/help_healer"),
        Step(6, "show_inventory_state", "show_inventory", "Show inventory through runtime inventory event", nameof(GameRuntimeCommandType.AddItem), "inventory/player_start"),
        Step(7, "craft_healing_potion", "craft", "Craft healing potion", nameof(GameRuntimeCommandType.CraftRecipe), "recipe/healing_potion"),
        Step(8, "harvest_apple_tree", "harvest", "Harvest apple tree", nameof(GameRuntimeCommandType.HarvestResourceNode), "node/apple_tree"),
        Step(9, "execute_transaction", "transaction", "Execute healing potion transaction", nameof(GameRuntimeCommandType.ExecuteTransaction), "transaction/buy_healing_potion"),
        Step(10, "start_encounter", "encounter", "Start goblin duel", nameof(GameRuntimeCommandType.StartEncounter), "encounter/goblin_duel"),
        Step(11, "combat_round", "combat_round", "Run combat round", nameof(GameRuntimeCommandType.BasicAttack), "goblin"),
        Step(12, "final_state", "final_state", "Show final runtime state", "state-summary", "game/minimal-map-game", false)
    ];

    private static CanonicalRuntimePlayerCommandLoopStep Step(
        int index,
        string stepId,
        string category,
        string commandLabel,
        string commandKind,
        string targetId,
        bool runtimeExecuted = true) =>
        new()
        {
            Index = index,
            StepId = stepId,
            Category = category,
            CommandLabel = commandLabel,
            RuntimeCommandKind = commandKind,
            TargetId = targetId,
            RuntimePrimitiveHint = "runtime.command." + category,
            RuntimeExecuted = runtimeExecuted
        };

    private static CanonicalRuntimePlayerCommandLoopSnapshot BuildSnapshot(
        CanonicalRuntimePlayerCommandLoopStep step,
        string stateHashBefore,
        string stateHashAfter,
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        IReadOnlyList<CanonicalRuntimePlayerCommandLoopRuntimeEvent> events)
    {
        var mapSummary = string.IsNullOrWhiteSpace(session.MapState.CurrentMapId)
            ? package.Manifest.StartMapId + " @ not_started"
            : session.MapState.CurrentMapId
              + " @ "
              + session.MapState.PlayerPosition.X
              + ","
              + session.MapState.PlayerPosition.Y;
        var state = session.GameplayState;
        return new CanonicalRuntimePlayerCommandLoopSnapshot
        {
            StepIndex = step.Index,
            StepId = step.StepId,
            Category = step.Category,
            CommandLabel = step.CommandLabel,
            StateHashBefore = stateHashBefore,
            StateHashAfter = stateHashAfter,
            MapSummary = mapSummary,
            PlayerX = session.MapState.PlayerPosition.X,
            PlayerY = session.MapState.PlayerPosition.Y,
            VisibleInteractionSummary = EventSummary(events, "InteractionTriggered"),
            DialogueSummary = state.ActiveDialogue == null
                ? EventSummary(events, "DialogueOpened")
                : state.ActiveDialogue.DialogueId + ":" + state.ActiveDialogue.CurrentNodeId + ":" + state.ActiveDialogue.Open,
            QuestSummary = string.Join("; ", state.Quests
                .OrderBy(quest => quest.QuestId, StringComparer.Ordinal)
                .Select(quest => quest.QuestId + ":" + quest.State + ":" + (quest.CurrentStageId ?? string.Empty))),
            InventorySummary = string.Join("; ", state.Inventories
                .OrderBy(inventory => inventory.Id, StringComparer.Ordinal)
                .Select(inventory => inventory.Id + "=" + string.Join(",", inventory.Stacks
                    .OrderBy(stack => stack.ItemId, StringComparer.Ordinal)
                    .Select(stack => stack.ItemId + ":" + Format(stack.Amount))))),
            CombatSummary = state.ActiveEncounter == null
                ? EventSummary(events, "EncounterStarted")
                : state.ActiveEncounter.EncounterId
                  + ":round="
                  + state.ActiveEncounter.Round
                  + ":turn="
                  + state.ActiveEncounter.TurnIndex
                  + ":active="
                  + state.ActiveEncounter.Active,
            DiagnosticSummary = events.Count == 0 ? "no events emitted" : "eventCount=" + events.Count,
            ProjectionOnly = false,
            UnityGameplayTruth = false,
            RuntimeEvents = events
        };
    }

    private static CanonicalRuntimePlayerCommandLoopRuntimeEvent CommandLoopEvent(
        ref int eventIndex,
        CanonicalRuntimePlayerCommandLoopStep step,
        string eventType,
        string targetId) =>
        new()
        {
            EventIndex = eventIndex++,
            StepIndex = step.Index,
            StepId = step.StepId,
            Source = "command-loop",
            EventType = eventType,
            TargetId = targetId,
            Message = step.CommandLabel
        };

    private static IReadOnlyList<CanonicalRuntimePlayerCommandLoopRuntimeEvent> RuntimeEvents(
        ref int eventIndex,
        CanonicalRuntimePlayerCommandLoopStep step,
        UnifiedRuntimeResult result)
    {
        var events = new List<CanonicalRuntimePlayerCommandLoopRuntimeEvent>();
        foreach (var runtimeEvent in result.MapEvents)
        {
            events.Add(new CanonicalRuntimePlayerCommandLoopRuntimeEvent
            {
                EventIndex = eventIndex++,
                StepIndex = step.Index,
                StepId = step.StepId,
                Source = "map-runtime",
                EventType = runtimeEvent.Type.ToString(),
                TargetId = runtimeEvent.TargetId ?? string.Empty,
                Message = runtimeEvent.Message
            });
        }

        foreach (var runtimeEvent in result.GameplayEvents)
        {
            events.Add(new CanonicalRuntimePlayerCommandLoopRuntimeEvent
            {
                EventIndex = eventIndex++,
                StepIndex = step.Index,
                StepId = step.StepId,
                Source = "gameplay-runtime",
                EventType = runtimeEvent.Type.ToString(),
                TargetId = runtimeEvent.TargetId ?? string.Empty,
                Message = runtimeEvent.Message
            });
        }

        return events;
    }

    private static string EventSummary(
        IEnumerable<CanonicalRuntimePlayerCommandLoopRuntimeEvent> events,
        string eventType) =>
        events.FirstOrDefault(item => item.EventType == eventType)?.Message ?? string.Empty;

    private static string HashSession(UnifiedRuntimeSession session) =>
        HashText(JsonSerializer.Serialize(session, StableJsonOptions));

    private static string HashText(string text)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
        var builder = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
        {
            builder.Append(b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    private static string Format(double value) =>
        value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
}
