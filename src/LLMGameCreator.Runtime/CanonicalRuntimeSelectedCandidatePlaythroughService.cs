using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class CanonicalRuntimeSelectedCandidatePlaythroughService :
    ICanonicalRuntimeSelectedCandidatePlaythroughService
{
    private static readonly JsonSerializerOptions StableJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    private readonly IUnifiedGameRuntimeService _runtime;
    private readonly IRuntimeStateSerializer _serializer;

    static CanonicalRuntimeSelectedCandidatePlaythroughService()
    {
        StableJsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public CanonicalRuntimeSelectedCandidatePlaythroughService(
        IUnifiedGameRuntimeService runtime,
        IRuntimeStateSerializer serializer)
    {
        _runtime = runtime;
        _serializer = serializer;
    }

    public static CanonicalRuntimeSelectedCandidatePlaythroughService CreateDefault()
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

        return new CanonicalRuntimeSelectedCandidatePlaythroughService(
            new UnifiedGameRuntimeService(new DefaultGameRuntime(), gameplay),
            new RuntimeStateSerializer());
    }

    public CanonicalRuntimeSelectedCandidatePlaythroughResult Execute(
        GamePackageDefinition package,
        CanonicalRuntimeSelectedCandidatePlaythroughRequest request)
    {
        var script = BuildScript();
        var primary = ExecuteScript(package, script);
        var stateBeforeSave = Clone(primary.Session);
        var saveJson = _serializer.Serialize(stateBeforeSave);
        var stateAfterLoad = _serializer.DeserializeUnifiedSession(saveJson);
        var saveStateHash = HashSession(stateBeforeSave);
        var loadStateHash = HashSession(stateAfterLoad);
        var replay = ExecuteScript(package, script);
        var replayStateHash = HashSession(replay.Session);
        var stateSummary = BuildStateSummary(request, package, primary.Session, primary.HashChain);
        var diagnostics = primary.Diagnostics.Concat(replay.Diagnostics).ToList();
        var eventHashChainMatch =
            HashTranscript(primary.Transcript) == HashTranscript(replay.Transcript)
            && primary.HashChain.SequenceEqual(replay.HashChain, StringComparer.Ordinal);
        var saveLoadReplay = new CanonicalRuntimeSelectedCandidateSaveLoadReplayResult
        {
            SaveStateHash = saveStateHash,
            LoadStateHash = loadStateHash,
            ReplayStateHash = replayStateHash,
            SaveLoadHashMatch = string.Equals(saveStateHash, loadStateHash, StringComparison.Ordinal),
            ReplayHashMatch = string.Equals(stateSummary.FinalStateHash, replayStateHash, StringComparison.Ordinal),
            EventHashChainMatch = eventHashChainMatch,
            Diagnostics = diagnostics
        };
        saveLoadReplay.Passed =
            saveLoadReplay.SaveLoadHashMatch
            && saveLoadReplay.ReplayHashMatch
            && saveLoadReplay.EventHashChainMatch;

        var runtimeCommandCount = script.Count(command => command.RuntimeExecuted);
        var runtimeEventCount = primary.Transcript.Count(IsRuntimeEvent);
        var missing = primary.MissingRuntimePrimitives
            .Concat(replay.MissingRuntimePrimitives)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var started = primary.CanonicalRuntimeStarted && replay.CanonicalRuntimeStarted;
        var stateHashChainPresent = primary.HashChain.Count == runtimeCommandCount
                                    && primary.HashChain.All(hash =>
                                        !string.IsNullOrWhiteSpace(hash));
        var passed =
            started
            && primary.Success
            && replay.Success
            && runtimeCommandCount >= 6
            && runtimeEventCount >= 6
            && stateHashChainPresent
            && saveLoadReplay.Passed
            && missing.Count == 0;

        return new CanonicalRuntimeSelectedCandidatePlaythroughResult
        {
            CandidateId = request.CandidateId,
            PackagePath = request.PackagePath,
            HandoffPath = request.HandoffPath,
            CanonicalRuntimeStarted = started,
            SelectedCandidateExecutedByRuntime = started && runtimeCommandCount > 0,
            ProjectionOnly = false,
            RuntimePrimitiveMissing = missing.Count > 0,
            MissingRuntimePrimitives = missing,
            RuntimeCommandCount = runtimeCommandCount,
            RuntimeEventCount = runtimeEventCount,
            StateHashChainPresent = stateHashChainPresent,
            Passed = passed,
            PlaythroughScript = script,
            Transcript = primary.Transcript,
            StateSummary = stateSummary,
            StateBeforeSave = stateBeforeSave,
            StateAfterLoad = stateAfterLoad,
            ReplayTranscript = replay.Transcript,
            SaveLoadReplay = saveLoadReplay,
            Diagnostics = diagnostics
        };
    }

    private RuntimeExecution ExecuteScript(
        GamePackageDefinition package,
        IReadOnlyList<CanonicalRuntimeSelectedCandidateCommand> script)
    {
        var session = new UnifiedRuntimeSession();
        var started = false;
        var success = true;
        var eventIndex = 0;
        var transcript = new List<CanonicalRuntimeSelectedCandidateEvent>();
        var diagnostics = new List<string>();
        var hashes = new List<string>();
        var missing = new List<string>();

        foreach (var command in script)
        {
            if (!command.RuntimeExecuted)
            {
                continue;
            }

            var beforeHash = started ? HashSession(session) : "not_started";
            UnifiedRuntimeResult result;
            switch (command.StepId)
            {
                case "initialize_world_map_state":
                    result = _runtime.Start(package);
                    started = result.Success;
                    break;
                case "move_to_sign":
                    result = _runtime.ExecutePlayerCommand(
                        package,
                        session,
                        PlayerCommand.Move(Direction2D.Right));
                    break;
                case "inspect_sign":
                    result = _runtime.ExecutePlayerCommand(package, session, PlayerCommand.Interact());
                    break;
                default:
                    result = ExecuteGameplayCommand(package, session, command);
                    break;
            }

            session = result.Session;
            var afterHash = HashSession(session);
            hashes.Add(afterHash);
            AddTranscriptEvents(
                transcript,
                ref eventIndex,
                command,
                beforeHash,
                afterHash,
                result.MapEvents,
                result.GameplayEvents);
            diagnostics.AddRange(result.Diagnostics.Select(diagnostic =>
                $"{command.StepId}:{diagnostic.Code}:{diagnostic.Message}"));

            if (!result.Success)
            {
                success = false;
                missing.Add($"{command.StepId}:{result.Message}");
                break;
            }
        }

        return new RuntimeExecution
        {
            Session = session,
            CanonicalRuntimeStarted = started,
            Success = success,
            Transcript = transcript,
            HashChain = hashes,
            Diagnostics = diagnostics,
            MissingRuntimePrimitives = missing
        };
    }

    private UnifiedRuntimeResult ExecuteGameplayCommand(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        CanonicalRuntimeSelectedCandidateCommand command)
    {
        var runtimeCommand = command.StepId switch
        {
            "open_old_guard_dialogue" =>
                GameRuntimeCommand.OpenDialogue("dialogue/old_guard_intro"),
            "start_help_healer_quest" =>
                GameRuntimeCommand.StartQuest("quest/help_healer"),
            "refresh_help_healer_objectives" =>
                new GameRuntimeCommand { Type = GameRuntimeCommandType.RefreshQuestObjectives },
            "craft_healing_potion" =>
                GameRuntimeCommand.CraftRecipe("recipe/healing_potion", "inventory/player_start"),
            "harvest_apple_tree" =>
                GameRuntimeCommand.HarvestResourceNode(
                    "node/apple_tree",
                    "inventory/player_start",
                    "item/woodcutting_axe",
                    134),
            "grant_transaction_gold" =>
                new GameRuntimeCommand
                {
                    Type = GameRuntimeCommandType.ChangeResource,
                    Id = "resource/gold",
                    Amount = 25
                },
            "buy_healing_potion" =>
                GameRuntimeCommand.ExecuteTransaction(
                    "transaction/buy_healing_potion",
                    "inventory/player_start"),
            "start_goblin_duel" =>
                GameRuntimeCommand.StartEncounter("encounter/goblin_duel", 134),
            "player_attack_goblin" =>
                GameRuntimeCommand.BasicAttack("player", "goblin"),
            "goblin_attack_player" =>
                GameRuntimeCommand.BasicAttack("goblin", "player"),
            _ => new GameRuntimeCommand { Type = GameRuntimeCommandType.Wait }
        };

        return _runtime.ExecuteGameplayCommand(package, session, runtimeCommand);
    }

    private static IReadOnlyList<CanonicalRuntimeSelectedCandidateCommand> BuildScript()
    {
        var commands = new List<CanonicalRuntimeSelectedCandidateCommand>
        {
            Command(0, "load_selected_package_identity", "package", "load_package_identity", "game/minimal-map-game", false, "Load selected candidate identity from Goal131 handoff."),
            Command(1, "validate_package_anchors", "validation", "validate_required_anchors", "goal134_required_anchor_set", false, "Validate required selected-candidate anchors before runtime execution."),
            Command(2, "initialize_world_map_state", "runtime-start", "Start", "map/village", true, "Initialize canonical map and gameplay state."),
            Command(3, "move_to_sign", "player", "MoveRight", "entity/village/sign", true, "Move from start position toward the sign interaction target."),
            Command(4, "inspect_sign", "player", "Interact", "interaction/sign_inspect", true, "Inspect the village sign through the unified runtime bridge."),
            Command(5, "open_old_guard_dialogue", "gameplay", nameof(GameRuntimeCommandType.OpenDialogue), "dialogue/old_guard_intro", true, "Open old guard dialogue summary through runtime dialogue service."),
            Command(6, "start_help_healer_quest", "gameplay", nameof(GameRuntimeCommandType.StartQuest), "quest/help_healer", true, "Start help_healer quest in canonical gameplay state."),
            Command(7, "refresh_help_healer_objectives", "gameplay", nameof(GameRuntimeCommandType.RefreshQuestObjectives), "quest/help_healer", true, "Evaluate help_healer objectives against inventory/player_start."),
            Command(8, "craft_healing_potion", "gameplay", nameof(GameRuntimeCommandType.CraftRecipe), "recipe/healing_potion", true, "Craft healing_potion using inventory/player_start."),
            Command(9, "harvest_apple_tree", "gameplay", nameof(GameRuntimeCommandType.HarvestResourceNode), "node/apple_tree", true, "Harvest node/apple_tree using item/woodcutting_axe."),
            Command(10, "grant_transaction_gold", "gameplay", nameof(GameRuntimeCommandType.ChangeResource), "resource/gold", true, "Grant deterministic gold required for transaction coverage."),
            Command(11, "buy_healing_potion", "gameplay", nameof(GameRuntimeCommandType.ExecuteTransaction), "transaction/buy_healing_potion", true, "Execute buy_healing_potion transaction."),
            Command(12, "start_goblin_duel", "gameplay", nameof(GameRuntimeCommandType.StartEncounter), "encounter/goblin_duel", true, "Start deterministic goblin duel encounter."),
            Command(13, "player_attack_goblin", "gameplay", nameof(GameRuntimeCommandType.BasicAttack), "goblin", true, "Run player basic attack against goblin."),
            Command(14, "goblin_attack_player", "gameplay", nameof(GameRuntimeCommandType.BasicAttack), "player", true, "Run goblin response attack against player.")
        };
        return commands;
    }

    private static CanonicalRuntimeSelectedCandidateCommand Command(
        int index,
        string stepId,
        string kind,
        string type,
        string targetId,
        bool runtimeExecuted,
        string description) =>
        new()
        {
            Index = index,
            StepId = stepId,
            CommandKind = kind,
            CommandType = type,
            TargetId = targetId,
            InventoryId = stepId is "craft_healing_potion" or "harvest_apple_tree" or "buy_healing_potion"
                ? "inventory/player_start"
                : string.Empty,
            Amount = stepId == "grant_transaction_gold" ? 25 : 0,
            Seed = stepId is "harvest_apple_tree" or "start_goblin_duel" ? 134 : null,
            RuntimeExecuted = runtimeExecuted,
            Description = description
        };

    private static void AddTranscriptEvents(
        List<CanonicalRuntimeSelectedCandidateEvent> transcript,
        ref int eventIndex,
        CanonicalRuntimeSelectedCandidateCommand command,
        string beforeHash,
        string afterHash,
        IEnumerable<RuntimeEvent> mapEvents,
        IEnumerable<GameRuntimeEvent> gameplayEvents)
    {
        foreach (var runtimeEvent in mapEvents)
        {
            transcript.Add(new CanonicalRuntimeSelectedCandidateEvent
            {
                EventIndex = eventIndex++,
                CommandIndex = command.Index,
                StepId = command.StepId,
                Source = "map-runtime",
                EventType = runtimeEvent.Type.ToString(),
                TargetId = runtimeEvent.TargetId ?? string.Empty,
                Message = runtimeEvent.Message,
                StateHashBefore = beforeHash,
                StateHashAfter = afterHash
            });
        }

        foreach (var runtimeEvent in gameplayEvents)
        {
            transcript.Add(new CanonicalRuntimeSelectedCandidateEvent
            {
                EventIndex = eventIndex++,
                CommandIndex = command.Index,
                StepId = command.StepId,
                Source = "gameplay-runtime",
                EventType = runtimeEvent.Type.ToString(),
                TargetId = runtimeEvent.TargetId ?? string.Empty,
                Message = runtimeEvent.Message,
                StateHashBefore = beforeHash,
                StateHashAfter = afterHash
            });
        }
    }

    private CanonicalRuntimeSelectedCandidateStateSummary BuildStateSummary(
        CanonicalRuntimeSelectedCandidatePlaythroughRequest request,
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        IReadOnlyList<string> hashChain)
    {
        var state = session.GameplayState;
        return new CanonicalRuntimeSelectedCandidateStateSummary
        {
            CandidateId = request.CandidateId,
            PackageId = package.Manifest.PackageId,
            PackageTitle = package.Manifest.Title,
            CurrentMapId = session.MapState.CurrentMapId,
            PlayerX = session.MapState.PlayerPosition.X,
            PlayerY = session.MapState.PlayerPosition.Y,
            Tick = state.Tick,
            InventorySummary = string.Join("; ", state.Inventories
                .OrderBy(inventory => inventory.Id, StringComparer.Ordinal)
                .Select(inventory => inventory.Id + "=" + string.Join(",", inventory.Stacks
                    .OrderBy(stack => stack.ItemId, StringComparer.Ordinal)
                    .Select(stack => stack.ItemId + ":" + Format(stack.Amount))))),
            ResourceSummary = string.Join("; ", state.Resources
                .OrderBy(resource => resource.ResourceId, StringComparer.Ordinal)
                .Select(resource => resource.ResourceId + ":" + Format(resource.Amount))),
            QuestSummary = string.Join("; ", state.Quests
                .OrderBy(quest => quest.QuestId, StringComparer.Ordinal)
                .Select(quest => quest.QuestId + ":" + quest.State + ":" + (quest.CurrentStageId ?? string.Empty))),
            ActiveDialogueSummary = state.ActiveDialogue == null
                ? "none"
                : state.ActiveDialogue.DialogueId + ":" + state.ActiveDialogue.CurrentNodeId + ":" + state.ActiveDialogue.Open,
            ActiveEncounterSummary = state.ActiveEncounter == null
                ? "none"
                : state.ActiveEncounter.EncounterId
                  + ":round="
                  + state.ActiveEncounter.Round
                  + ":turn="
                  + state.ActiveEncounter.TurnIndex
                  + ":active="
                  + state.ActiveEncounter.Active,
            FinalStateHash = HashSession(session),
            StateHashChain = hashChain
        };
    }

    private UnifiedRuntimeSession Clone(UnifiedRuntimeSession session) =>
        _serializer.DeserializeUnifiedSession(_serializer.Serialize(session));

    private static bool IsRuntimeEvent(CanonicalRuntimeSelectedCandidateEvent runtimeEvent) =>
        runtimeEvent.Source is "map-runtime" or "gameplay-runtime";

    private static string HashSession(UnifiedRuntimeSession session) =>
        HashText(JsonSerializer.Serialize(session, StableJsonOptions));

    private static string HashTranscript(IEnumerable<CanonicalRuntimeSelectedCandidateEvent> transcript) =>
        HashText(string.Join(
            "\n",
            transcript.Select(item =>
                string.Join(
                    "|",
                    item.CommandIndex.ToString(),
                    item.StepId,
                    item.Source,
                    item.EventType,
                    item.TargetId,
                    item.StateHashBefore,
                    item.StateHashAfter))));

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

    private sealed class RuntimeExecution
    {
        public UnifiedRuntimeSession Session { get; set; } = new();
        public bool CanonicalRuntimeStarted { get; set; }
        public bool Success { get; set; }
        public IReadOnlyList<CanonicalRuntimeSelectedCandidateEvent> Transcript { get; set; } =
            new List<CanonicalRuntimeSelectedCandidateEvent>();
        public IReadOnlyList<string> HashChain { get; set; } = new List<string>();
        public IReadOnlyList<string> Diagnostics { get; set; } = new List<string>();
        public IReadOnlyList<string> MissingRuntimePrimitives { get; set; } = new List<string>();
    }
}
