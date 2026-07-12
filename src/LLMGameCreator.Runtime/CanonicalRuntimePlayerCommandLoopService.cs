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
    private readonly IReadOnlyDictionary<string, Func<GamePackageDefinition, UnifiedRuntimeSession, CanonicalRuntimePlayerCommandLoopStep, UnifiedRuntimeResult>> _primitiveHandlers;

    public CanonicalRuntimePlayerCommandLoopService(IUnifiedGameRuntimeService runtime)
    {
        _runtime = runtime;
        _primitiveHandlers = new Dictionary<string, Func<GamePackageDefinition, UnifiedRuntimeSession, CanonicalRuntimePlayerCommandLoopStep, UnifiedRuntimeResult>>(StringComparer.Ordinal)
        {
            ["runtime.command.start"] = (package, _, _) => _runtime.Start(package),
            ["runtime.command.move"] = (package, session, step) => _runtime.ExecutePlayerCommand(package, session,
                PlayerCommand.Move(ParseDirection(Arg(step, "direction", "right")))),
            ["runtime.command.interact"] = (package, session, _) => _runtime.ExecutePlayerCommand(package, session, PlayerCommand.Interact()),
            ["runtime.command.open_dialogue"] = (package, session, step) => _runtime.ExecuteGameplayCommand(package, session,
                GameRuntimeCommand.OpenDialogue(step.TargetId)),
            ["runtime.command.start_or_update_quest"] = (package, session, step) => _runtime.ExecuteMany(package, session,
                [GameRuntimeCommand.StartQuest(step.TargetId), new GameRuntimeCommand { Type = GameRuntimeCommandType.RefreshQuestObjectives }]),
            ["runtime.command.show_inventory"] = (package, session, step) => _runtime.ExecuteGameplayCommand(package, session,
                new GameRuntimeCommand
                {
                    Type = GameRuntimeCommandType.AddItem,
                    Id = Arg(step, "itemId"),
                    InventoryId = Arg(step, "inventoryId"),
                    Amount = ParseDouble(Arg(step, "amount", "1"))
                }),
            ["runtime.command.craft_recipe"] = (package, session, step) => _runtime.ExecuteGameplayCommand(package, session,
                GameRuntimeCommand.CraftRecipe(step.TargetId, Arg(step, "inventoryId"))),
            ["runtime.command.harvest_resource"] = (package, session, step) => _runtime.ExecuteGameplayCommand(package, session,
                GameRuntimeCommand.HarvestResourceNode(step.TargetId, Arg(step, "inventoryId"), Arg(step, "itemId"), ParseInt(Arg(step, "seed", "136")))),
            ["runtime.command.execute_transaction"] = (package, session, step) => _runtime.ExecuteMany(package, session,
                [new GameRuntimeCommand { Type = GameRuntimeCommandType.ChangeResource, Id = Arg(step, "grantResourceId"), Amount = ParseDouble(Arg(step, "grantAmount", "0")) },
                    GameRuntimeCommand.ExecuteTransaction(step.TargetId, Arg(step, "inventoryId"))]),
            ["runtime.command.start_encounter"] = (package, session, step) => _runtime.ExecuteGameplayCommand(package, session,
                GameRuntimeCommand.StartEncounter(step.TargetId, ParseInt(Arg(step, "seed", "136")))),
            ["runtime.command.basic_attack"] = (package, session, step) => _runtime.ExecuteGameplayCommand(package, session,
                GameRuntimeCommand.BasicAttack(Arg(step, "sourceParticipantId", "player"), step.TargetId)),
            ["runtime.command.open_container"] = (package, session, step) => _runtime.ExecuteGameplayCommand(package, session,
                GameRuntimeCommand.OpenContainer(step.TargetId)),
            ["runtime.command.take_from_container"] = (package, session, step) => _runtime.ExecuteGameplayCommand(package, session,
                GameRuntimeCommand.TakeFromContainer(Arg(step, "sourceInventoryId"), Arg(step, "itemId"),
                    ParseDouble(Arg(step, "amount", "1")), Arg(step, "targetInventoryId"))),
            ["runtime.command.equip_item"] = (package, session, step) => _runtime.ExecuteGameplayCommand(package, session,
                GameRuntimeCommand.EquipItem(Arg(step, "itemId"), Arg(step, "slotId"), Arg(step, "inventoryId")))
        };
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

    public CanonicalRuntimePlayerCommandLoopSession BeginSession(
        GamePackageDefinition package,
        CanonicalRuntimePlayerCommandLoopRequest request) =>
        new()
        {
            CandidateId = request.CandidateId,
            PackagePath = request.PackagePath,
            CurrentCommandIndex = 0,
            CurrentStateHash = "not_loaded",
            RuntimeStarted = false,
            RuntimeExecutionSucceeded = true,
            RuntimeSession = new UnifiedRuntimeSession(),
            CapabilityPlan = request.CapabilityPlan,
            Steps = request.CapabilityPlan is null ? BuildSteps() : BuildCapabilitySteps(package, request.CapabilityPlan)
        };

    public CanonicalRuntimePlayerCommandLoopExecutionResult ExecuteRange(
        GamePackageDefinition package,
        CanonicalRuntimePlayerCommandLoopSession session,
        CanonicalRuntimePlayerCommandLoopExecutionRequest request)
    {
        if (session.Steps.Count == 0)
        {
            session.Steps = session.CapabilityPlan is null ? BuildSteps() : BuildCapabilitySteps(package, session.CapabilityPlan);
        }

        var steps = session.Steps;
        var cursorBefore = session.CurrentCommandIndex;
        var stateHashBefore = session.CurrentStateHash;
        var snapshots = new List<CanonicalRuntimePlayerCommandLoopSnapshot>();
        var executedSteps = new List<CanonicalRuntimePlayerCommandLoopStep>();
        var diagnostics = new List<string>();

        if (request.RuntimeCommandStartIndex != session.CurrentCommandIndex)
        {
            diagnostics.Add(
                "canonical.cursor_mismatch:"
                + request.RuntimeCommandStartIndex
                + "!="
                + session.CurrentCommandIndex);
            session.Diagnostics.AddRange(diagnostics);
            return new CanonicalRuntimePlayerCommandLoopExecutionResult
            {
                RequestedOperation = request.RequestedOperation,
                RuntimeCommandStartIndex = request.RuntimeCommandStartIndex,
                RuntimeCommandEndIndex = request.RuntimeCommandEndIndex,
                CursorBefore = cursorBefore,
                CursorAfter = session.CurrentCommandIndex,
                StateHashBefore = stateHashBefore,
                StateHashAfter = session.CurrentStateHash,
                Success = false,
                Session = session,
                Diagnostics = diagnostics
            };
        }

        if (request.RuntimeCommandStartIndex < 0
            || request.RuntimeCommandEndIndex >= steps.Count
            || request.RuntimeCommandEndIndex < request.RuntimeCommandStartIndex)
        {
            diagnostics.Add("canonical.invalid_range:" + request.RuntimeCommandStartIndex + ".." + request.RuntimeCommandEndIndex);
            session.Diagnostics.AddRange(diagnostics);
            return new CanonicalRuntimePlayerCommandLoopExecutionResult
            {
                RequestedOperation = request.RequestedOperation,
                RuntimeCommandStartIndex = request.RuntimeCommandStartIndex,
                RuntimeCommandEndIndex = request.RuntimeCommandEndIndex,
                CursorBefore = cursorBefore,
                CursorAfter = session.CurrentCommandIndex,
                StateHashBefore = stateHashBefore,
                StateHashAfter = session.CurrentStateHash,
                Success = false,
                Session = session,
                Diagnostics = diagnostics
            };
        }

        for (var index = request.RuntimeCommandStartIndex;
             index <= request.RuntimeCommandEndIndex;
             index++)
        {
            var step = steps[index];
            var executed = ExecuteStep(package, session, step);
            snapshots.Add(executed.Snapshot);
            executedSteps.Add(step);
            session.CurrentCommandIndex = index + 1;
            diagnostics.AddRange(executed.Diagnostics);
            if (!executed.Success)
            {
                break;
            }
        }

        var eventCount = snapshots.SelectMany(snapshot => snapshot.RuntimeEvents).Count();
        var executedCommandCount = executedSteps.Count(step => step.RuntimeExecuted);
        return new CanonicalRuntimePlayerCommandLoopExecutionResult
        {
            RequestedOperation = request.RequestedOperation,
            RuntimeCommandStartIndex = request.RuntimeCommandStartIndex,
            RuntimeCommandEndIndex = request.RuntimeCommandEndIndex,
            CursorBefore = cursorBefore,
            CursorAfter = session.CurrentCommandIndex,
            RuntimeExecuted = executedCommandCount > 0 && session.RuntimeExecutionSucceeded,
            RuntimeMutation = !string.Equals(stateHashBefore, session.CurrentStateHash, StringComparison.Ordinal),
            ExecutedCommandCount = executedCommandCount,
            ProducedSnapshotCount = snapshots.Count,
            EventCount = eventCount,
            StateHashBefore = stateHashBefore,
            StateHashAfter = session.CurrentStateHash,
            Success = session.RuntimeExecutionSucceeded && snapshots.Count == executedSteps.Count,
            Session = session,
            Steps = executedSteps,
            Snapshots = snapshots,
            Diagnostics = diagnostics
        };
    }

    public CanonicalRuntimePlayerCommandLoopResult Execute(
        GamePackageDefinition package,
        CanonicalRuntimePlayerCommandLoopRequest request)
    {
        var session = BeginSession(package, request);
        ExecuteRange(package, session, new CanonicalRuntimePlayerCommandLoopExecutionRequest
        {
            RequestedOperation = "full_player_command_loop",
            RuntimeCommandStartIndex = 0,
            RuntimeCommandEndIndex = session.Steps.Count - 1
        });

        var steps = session.Steps;
        var snapshots = session.Snapshots;
        var diagnostics = session.Diagnostics;
        var missingPrimitives = session.MissingRuntimePrimitives;

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
            session.RuntimeStarted && runtimeEventCount > 0 && session.RuntimeExecutionSucceeded;
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

    private StepExecution ExecuteStep(
        GamePackageDefinition package,
        CanonicalRuntimePlayerCommandLoopSession session,
        CanonicalRuntimePlayerCommandLoopStep step)
    {
        var events = new List<CanonicalRuntimePlayerCommandLoopRuntimeEvent>();
        var diagnostics = new List<string>();
        UnifiedRuntimeResult? result = null;
        var eventIndex = session.EventIndex;
        var stateHashBefore = session.CurrentStateHash;

        if (!string.IsNullOrWhiteSpace(step.ActionId))
        {
            if (!_primitiveHandlers.TryGetValue(step.RuntimePrimitiveHint, out var handler))
                throw new InvalidOperationException("Missing Runtime primitive handler: " + step.RuntimePrimitiveHint);
            result = handler(package, session.RuntimeSession, step);
            if (step.RuntimePrimitiveHint == "runtime.command.start") session.RuntimeStarted = result.Success;
        }
        else switch (step.StepId)
        {
            case "load_selected_package":
                events.Add(CommandLoopEvent(ref eventIndex, step, "package-loaded", package.Manifest.PackageId));
                break;
            case "start_canonical_runtime":
                result = _runtime.Start(package);
                session.RuntimeStarted = result.Success;
                break;
            case "move_to_sign":
                result = _runtime.ExecutePlayerCommand(
                    package,
                    session.RuntimeSession,
                    PlayerCommand.Move(Direction2D.Right));
                break;
            case "interact_with_sign":
                result = _runtime.ExecutePlayerCommand(package, session.RuntimeSession, PlayerCommand.Interact());
                break;
            case "show_old_guard_dialogue":
                result = _runtime.ExecuteGameplayCommand(
                    package,
                    session.RuntimeSession,
                    GameRuntimeCommand.OpenDialogue(step.TargetId));
                break;
            case "start_or_update_help_healer_quest":
                result = _runtime.ExecuteMany(
                    package,
                    session.RuntimeSession,
                    [
                        GameRuntimeCommand.StartQuest(step.TargetId),
                        new GameRuntimeCommand { Type = GameRuntimeCommandType.RefreshQuestObjectives }
                    ]);
                break;
            case "show_inventory_state":
                result = _runtime.ExecuteGameplayCommand(
                    package,
                    session.RuntimeSession,
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
                    session.RuntimeSession,
                    GameRuntimeCommand.CraftRecipe(
                        step.TargetId,
                        "inventory/player_start"));
                break;
            case "harvest_apple_tree":
                result = _runtime.ExecuteGameplayCommand(
                    package,
                    session.RuntimeSession,
                    GameRuntimeCommand.HarvestResourceNode(
                        step.TargetId,
                        "inventory/player_start",
                        "item/woodcutting_axe",
                        136));
                break;
            case "execute_transaction":
                result = _runtime.ExecuteMany(
                    package,
                    session.RuntimeSession,
                    [
                        new GameRuntimeCommand
                        {
                            Type = GameRuntimeCommandType.ChangeResource,
                            Id = "resource/gold",
                            Amount = 25
                        },
                        GameRuntimeCommand.ExecuteTransaction(
                            step.TargetId,
                            "inventory/player_start")
                    ]);
                break;
            case "start_encounter":
                result = _runtime.ExecuteGameplayCommand(
                    package,
                    session.RuntimeSession,
                    GameRuntimeCommand.StartEncounter(step.TargetId, 136));
                break;
            case "combat_round":
                result = _runtime.ExecuteGameplayCommand(
                    package,
                    session.RuntimeSession,
                    GameRuntimeCommand.BasicAttack("player", step.TargetId));
                break;
            case "final_state":
                events.Add(CommandLoopEvent(ref eventIndex, step, "final-state", package.Manifest.PackageId));
                break;
        }

        if (result is not null)
        {
            session.RuntimeSession = result.Session;
            events.AddRange(RuntimeEvents(ref eventIndex, step, result));
            diagnostics.AddRange(result.Diagnostics.Select(diagnostic =>
                step.StepId + ":" + diagnostic.Code + ":" + diagnostic.Message));
            if (!result.Success)
            {
                session.RuntimeExecutionSucceeded = false;
                session.MissingRuntimePrimitives.Add(step.StepId + ":" + result.Message);
            }
        }

        var stateHashAfter = result is null && step.StepId == "load_selected_package"
            ? HashText(package.Manifest.PackageId + "|" + package.Manifest.Title + "|" + session.CandidateId)
            : HashSession(session.RuntimeSession);
        var snapshot = BuildSnapshot(
            step,
            stateHashBefore,
            stateHashAfter,
            package,
            session.RuntimeSession,
            events);
        session.EventIndex = eventIndex;
        session.CurrentStateHash = stateHashAfter;
        session.Snapshots.Add(snapshot);
        session.StateHashChain.Add(snapshot.StateHashAfter);
        session.Diagnostics.AddRange(diagnostics);

        return new StepExecution(
            snapshot,
            session.RuntimeExecutionSucceeded,
            diagnostics);
    }

    private sealed class StepExecution
    {
        public StepExecution(
            CanonicalRuntimePlayerCommandLoopSnapshot snapshot,
            bool success,
            IReadOnlyList<string> diagnostics)
        {
            Snapshot = snapshot;
            Success = success;
            Diagnostics = diagnostics;
        }

        public CanonicalRuntimePlayerCommandLoopSnapshot Snapshot { get; }
        public bool Success { get; }
        public IReadOnlyList<string> Diagnostics { get; }
    }

    private static IReadOnlyList<CanonicalRuntimePlayerCommandLoopStep> BuildCapabilitySteps(
        GamePackageDefinition package,
        CapabilityRuntimePlaythroughPlan plan)
    {
        var steps = new List<CanonicalRuntimePlayerCommandLoopStep>
        {
            Step(0, "load_selected_package", "load_package", "Load selected package", "package",
                package.Manifest.PackageId, false)
        };
        foreach (var action in plan.OrderedActions.Where(item => !item.PresentationOnly))
        {
            steps.Add(new CanonicalRuntimePlayerCommandLoopStep
            {
                Index = steps.Count,
                StepId = "capability." + action.ActionId,
                ActionId = action.ActionId,
                Category = action.Category,
                CommandLabel = action.ActionId,
                RuntimeCommandKind = PrimitiveCommandKind(action.RuntimePrimitiveId),
                TargetId = action.ResolvedTargetId,
                RuntimePrimitiveHint = action.RuntimePrimitiveId,
                Args = new SortedDictionary<string, string>(action.Args.ToDictionary(pair => pair.Key, pair => pair.Value,
                    StringComparer.Ordinal), StringComparer.Ordinal),
                RuntimeExecuted = true,
                RequiredForGreen = action.Required
            });
        }
        return steps;
    }

    private static string PrimitiveCommandKind(string primitiveId) => primitiveId switch
    {
        "runtime.command.start" => nameof(GameRuntimeCommandType.Initialize),
        "runtime.command.move" => nameof(PlayerCommandType.Move),
        "runtime.command.interact" => nameof(PlayerCommandType.Interact),
        "runtime.command.open_dialogue" => nameof(GameRuntimeCommandType.OpenDialogue),
        "runtime.command.start_or_update_quest" => nameof(GameRuntimeCommandType.StartQuest),
        "runtime.command.show_inventory" => nameof(GameRuntimeCommandType.AddItem),
        "runtime.command.craft_recipe" => nameof(GameRuntimeCommandType.CraftRecipe),
        "runtime.command.harvest_resource" => nameof(GameRuntimeCommandType.HarvestResourceNode),
        "runtime.command.execute_transaction" => nameof(GameRuntimeCommandType.ExecuteTransaction),
        "runtime.command.start_encounter" => nameof(GameRuntimeCommandType.StartEncounter),
        "runtime.command.basic_attack" => nameof(GameRuntimeCommandType.BasicAttack),
        "runtime.command.open_container" => nameof(GameRuntimeCommandType.OpenContainer),
        "runtime.command.take_from_container" => nameof(GameRuntimeCommandType.TakeFromContainer),
        "runtime.command.equip_item" => nameof(GameRuntimeCommandType.EquipItem),
        _ => string.Empty
    };

    private static string Arg(CanonicalRuntimePlayerCommandLoopStep step, string key, string fallback = "") =>
        step.Args.TryGetValue(key, out var value) ? value : fallback;

    private static int ParseInt(string value) =>
        int.Parse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);

    private static double ParseDouble(string value) =>
        double.Parse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);

    private static Direction2D ParseDirection(string value) => value.ToLowerInvariant() switch
    {
        "up" => Direction2D.Up,
        "down" => Direction2D.Down,
        "left" => Direction2D.Left,
        "right" => Direction2D.Right,
        _ => throw new InvalidOperationException("Unsupported movement direction: " + value)
    };

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
            EquipmentSummary = string.Join("; ", state.Equipment
                .OrderBy(equipment => equipment.OwnerId, StringComparer.Ordinal)
                .SelectMany(equipment => equipment.Slots.OrderBy(slot => slot.SlotId, StringComparer.Ordinal)
                    .Select(slot => slot.SlotId + ":" + (slot.ItemId ?? string.Empty)))),
            CombatSummary = CombatSummary(state, events),
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
                Message = runtimeEvent.Message,
                Args = new SortedDictionary<string, string>(runtimeEvent.Args, StringComparer.Ordinal)
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
                Message = runtimeEvent.Message,
                Args = new SortedDictionary<string, string>(runtimeEvent.Args, StringComparer.Ordinal)
            });
        }

        return events;
    }

    private static string EventSummary(
        IEnumerable<CanonicalRuntimePlayerCommandLoopRuntimeEvent> events,
        string eventType) =>
        events.FirstOrDefault(item => item.EventType == eventType)?.Message ?? string.Empty;

    private static string CombatSummary(
        GameRuntimeState state,
        IEnumerable<CanonicalRuntimePlayerCommandLoopRuntimeEvent> events)
    {
        if (state.ActiveEncounter == null)
        {
            return EventSummary(events, "EncounterStarted");
        }

        var encounter = state.ActiveEncounter;
        var participantSummary = string.Join(",", encounter.Participants
            .OrderBy(participant => participant.Id, StringComparer.Ordinal)
            .Select(participant =>
                participant.Id
                + "[alive="
                + participant.Alive
                + ";"
                + string.Join("|", participant.Resources
                    .OrderBy(resource => resource.ResourceId, StringComparer.Ordinal)
                    .Select(resource => resource.ResourceId + "=" + Format(resource.Amount)))
                + "]"));
        return encounter.EncounterId
               + ":round="
               + encounter.Round
               + ":turn="
               + encounter.TurnIndex
               + ":active="
               + encounter.Active
               + ":participants="
               + participantSummary;
    }

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
